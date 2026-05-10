using QMC.Common;
using QMC.Common.Cameras;
using QMC.Common.Vision;
using QMC.Common.Vision.Tools;
using QMC.Common.VisionPart;
using QMC.LCP_280.Process.Component; // for MeasurementRecipe & RecipeManager
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace QMC.LCP_280.Process
{
    public partial class PatternMatchingControl : UserControl
    {
        // =========================
        #region Fields (Core / State)
        // =========================
        private string _recipeDirectory;
        private string _currentRecipeName = "Default";

        private InternalMultiPatternMatchingVisionPart _visionPart;
        private MultiPatternMatchingParameters _parameters;

        private readonly List<Camera> _cameras = new List<Camera>();
        private readonly List<string> _cameraNames = new List<string>();

        private bool _suspendAutoLoad = false;
        private Point _lastResultPoint = Point.Empty; // 검색 결과 표시용
        private double _lastResultAngle = 0;          // 각도 저장 (미사용시 확장 대비)

        // [CHG] PatternMatchingRunner 직접 생성 대신 Hub로 관리 (카메라별 재사용/옵션 통일)
        private PatternMatchingRunner _runner;

        // NEW: 저장된 다중 결과 목록
        private List<PatternMatchingResult.PatternMatchingResultValue> _lastValues = new List<PatternMatchingResult.PatternMatchingResultValue>();
        // NEW: 러너가 반환한 마지막 실행 결과 (대표 좌표 포함)
        private PatternMatchingRunner.PatternMatchRunResult _lastRunResult;

        // [ADD] 현재 viewer가 attach된 cameraKey 추적 (카메라 변경 시 detach 용)
        private string _attachedCameraKey;

        // 디자인 타임 가드
        private readonly bool _designMode;

        //private string _currentMode = "DefaultMode";
        private PatternMatchingRunner.ProcessMode _currentMode = PatternMatchingRunner.ProcessMode.Prealign;
        private string CurrentModeKey => _currentMode.ToString();
        #endregion

        // =========================
        #region Ctor / Lifetime
        // =========================
        public PatternMatchingControl()
        {
            _designMode = IsActuallyInDesignMode();
            InitializeComponent();

            if (_designMode)
                return;


            this.VisibleChanged += PatternMatchingControl_VisibleChanged;

            // 이벤트 구독
            EquipmentRecipe.CurrentRecipeChanged -= Equipment_CurrentRecipeChanged;
            EquipmentRecipe.CurrentRecipeChanged += Equipment_CurrentRecipeChanged;

            // --- VisionPart/Parameters 먼저 준비 (★중요) ---
            if (_visionPart == null)
            {
                _visionPart = new InternalMultiPatternMatchingVisionPart("PM_Dialog") { Simulated = true };
                _visionPart.Create();
            }
            _parameters = _visionPart.GetPatternMatchingParameters();

            // ROI/Param UI 연결
            maintROIControl.SetOwner(_visionPart);
            maintROIControl.SetImageviwer(_viewer);
            AttachEvents();
            patternMatchingParamControl?.UpdateParameters(_parameters);

            // 초기 레시피명 설정
            ApplyVisionRecipeFromMeasurement();
            UpdateRecipeDirectoryByCurrentRecipeName();

            // 카메라 바인딩/초기 선택
            TryBindEquipmentCameras();
            InitializeCameraList(); // 여기서 ApplyCameraSelection(0) -> LoadRecipeForCurrentCamera() 호출될 수 있음

            // 혹시 카메라 초기화 전에 한번도 LoadRecipeForCurrentCamera가 안 탔다면 대비
            // (NoCamera면 내부에서 처리됨)
            SafeLoadUiRecipeForCurrentCamera();

            cmbMode.SelectedIndex = 0; // Prealign 기본 선택


            UpdateStatus("Ready");
        }

        private void SafeLoadUiRecipeForCurrentCamera()
        {
            try
            {
                var camName = GetCurrentCameraName();
                if (string.IsNullOrWhiteSpace(camName) || camName.Equals("NoCamera", StringComparison.OrdinalIgnoreCase))
                {
                    // 카메라 없으면 Default로라도 UI 로드(경로 생성만)
                    LoadRecipe(_currentRecipeName, "NoCamera");
                    return;
                }

                LoadRecipe(_currentRecipeName, camName);
            }
            catch { }
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            try
            {
                if (!DesignMode)
                {
                    // [ADD] detach로 viewer Paint 구독/overlays 정리
                    DetachRunnerForCurrentViewer();

                    if (_viewer != null)
                    {
                        _viewer.Paint -= Viewer_PaintMatches;
                        try { _viewer.Camera?.StopLive(); } catch { }
                    }
                    if (maintROIControl != null)
                    {
                        maintROIControl.TrainImageCaptured -= MaintROIControl_TrainImageCaptured;
                    }

                    try { EquipmentRecipe.CurrentRecipeChanged -= Equipment_CurrentRecipeChanged; } catch { }

                    _runner = null;
                }
            }
            catch { }
            base.OnHandleDestroyed(e);
        }

        private void PatternMatchingControl_VisibleChanged(object sender, EventArgs e)
        {
            if (_designMode) return;

            if (this.Visible)
            {
                // 다시 구독 보장 (중복 방지 위해 먼저 제거 후 추가)
                EquipmentRecipe.CurrentRecipeChanged -= Equipment_CurrentRecipeChanged;
                EquipmentRecipe.CurrentRecipeChanged += Equipment_CurrentRecipeChanged;

                // 페이지 들어온 순간 "현재 레시피"로 강제 동기화 (놓친 이벤트 보정)
                ForceSyncFromEquipmentRecipe();
            }
            else
            {
                // 숨길 때 굳이 해제 안 해도 되지만(메모리만 괜찮다면),
                // 컨트롤이 자주 생성/파괴되면 해제 유지
                // EquipmentRecipe.CurrentRecipeChanged -= Equipment_CurrentRecipeChanged;
            }
        }

        private void ForceSyncFromEquipmentRecipe()
        {
            try
            {
                var eq = Equipment.Instance;
                var name = eq?.EquipmentRecipe?.CurrentRecipeName;

                if (string.IsNullOrWhiteSpace(name))
                    name = "Default";

                try { name = PatternMatchingRecipeStore.NormalizeRecipeName(name); } catch { }

                // 기존 이벤트핸들러 로직 재사용
                ApplyRecipeNameAndReload(name);
            }
            catch { }
        }

        private void ApplyRecipeNameAndReload(string newName)
        {
            if (string.IsNullOrWhiteSpace(newName))
                newName = "Default";

            if (string.Equals(_currentRecipeName, newName, StringComparison.OrdinalIgnoreCase))
                return;

            _currentRecipeName = newName;

            UpdateRecipeDirectoryByCurrentRecipeName();

            ResetCrossAndResults();
            try { _viewer?.ResultOverlays?.Clear(); } catch { }
            try { _viewer?.NormalOverlays?.Clear(); } catch { }
            _lastRunResult = null;

            // ★ 반드시 VisionPart가 있어야 UI 파라미터를 넣을 수 있음
            if (_visionPart == null)
            {
                _visionPart = new InternalMultiPatternMatchingVisionPart("PM_Dialog") { Simulated = true };
                _visionPart.Create();
            }
            if (_parameters == null)
                _parameters = _visionPart.GetPatternMatchingParameters();

            // ✅ UI 파라미터/ROI 갱신 (파일 로드)
            SafeLoadUiRecipeForCurrentCamera();

            // ✅ runner도 새 레시피로 로드
            if (_runner == null)
                AttachRunnerForCurrentViewer();

            if (_runner != null)
            {
                _runner.SetRecipe(_currentRecipeName);
                _runner.LoadRecipe();
            }

            UpdateStatus($"PM recipe updated: {_currentRecipeName}");
            _viewer?.Invalidate();
        }

        private void UpdateRecipeDirectoryByCurrentRecipeName()
        {
            // _currentRecipeName 이 세팅되어 있다는 전제
            var baseName = "Default";
            try
            {
                baseName = PatternMatchingRecipeStore.NormalizeRecipeBaseName(_currentRecipeName ?? "Default");
            }
            catch
            {
                baseName = _currentRecipeName ?? "Default";
            }

            _recipeDirectory = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Recipes",
                baseName,
                "PatternMatching");
        }

        private void Equipment_CurrentRecipeChanged(object sender, EquipmentRecipe.MeasurementRecipeChangedEventArgs e)
        {
            if (_designMode) 
                return;

            try
            {
                if (this.IsHandleCreated && this.InvokeRequired)
                {
                    this.BeginInvoke(new Action(() => Equipment_CurrentRecipeChanged(sender, e)));
                    return;
                }

                var newName = e?.Recipe?.Name;
                if (string.IsNullOrWhiteSpace(newName))
                    newName = Equipment.Instance?.EquipmentRecipe?.CurrentRecipeName;

                if (string.IsNullOrWhiteSpace(newName))
                    newName = "Default";

                try { newName = PatternMatchingRecipeStore.NormalizeRecipeName(newName); } catch { }

                ApplyRecipeNameAndReload(newName); // ★여기서 통일
            }
            catch (Exception ex)
            {
                Log.Write("PatternMatchingControl", "Equipment_CurrentRecipeChanged error: " + ex.Message);
            }
        }

        #endregion
        // =========================
        #region Hub Attach/Detach
        // =========================
        private void AttachRunnerForCurrentViewer()
        {
            try
            {
                if (_viewer == null || _viewer.IsDisposed)
                    return;

                var camKey = GetCurrentCameraName();
                if (string.IsNullOrWhiteSpace(camKey) || string.Equals(camKey, "NoCamera", StringComparison.OrdinalIgnoreCase))
                    return;

                // 이전 attach 해제
                if (!string.IsNullOrWhiteSpace(_attachedCameraKey) &&
                    !string.Equals(_attachedCameraKey, camKey, StringComparison.OrdinalIgnoreCase))
                {
                    VisionRunnerHub.DetachViewer(_attachedCameraKey, _viewer, clearOverlays: true);
                    _attachedCameraKey = null;
                }

                // 현재 attach
                _runner = VisionRunnerHub.AttachViewer(camKey, _viewer,
                    new PatternMatchingRunner.ViewerDisplayOptions
                    {
                        DrawCrossOnViewer = false,
                        HighlightReferenceMatch = chkHighlightRef?.Checked ?? true,
                        ShowMatchIndexes = chkShowIndexes?.Checked ?? false
                    });

                // runner에 현재 레시피명 강제 주입
                var eq = Equipment.Instance;
                var recipeName = eq?.EquipmentRecipe?.CurrentRecipeName ?? _currentRecipeName ?? "Default";
                try { recipeName = PatternMatchingRecipeStore.NormalizeRecipeName(recipeName); } catch { }
                _runner.SetRecipe(recipeName);

                _attachedCameraKey = camKey;

                UpdateRunnerModeFromUI();
                ApplyOverlayOptionCheckboxes();
            }
            catch { }
        }

        private void DetachRunnerForCurrentViewer()
        {
            try
            {
                if (_viewer == null || _viewer.IsDisposed)
                    return;

                if (!string.IsNullOrWhiteSpace(_attachedCameraKey))
                {
                    VisionRunnerHub.DetachViewer(_attachedCameraKey, _viewer, clearOverlays: true);
                    _attachedCameraKey = null;
                }

                _runner = null;
            }
            catch { }
        }
        #endregion
        // =========================
        #region Small Helpers
        // =========================
        private bool IsActuallyInDesignMode()
        {
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
                return true;

            try
            {
                return System.Diagnostics.Process.GetCurrentProcess().ProcessName.Equals("devenv", StringComparison.OrdinalIgnoreCase);
            }
            catch { return false; }
        }

        private void UpdateStatus(string text)
        {
            if (_lblStatus != null) _lblStatus.Text = text;
        }

        private void SyncParametersFromUI()
        {
            if (patternMatchingParamControl?.Parameters != null)
            {
                _parameters = patternMatchingParamControl.Parameters; // 동일 객체 참조
                _visionPart.SetPatternMatchingParameters(_parameters);
            }
        }

        private string GetCurrentCameraName() => _viewer?.Camera?.Name ?? "NoCamera";

        private void ResetCrossAndResults()
        {
            try
            {
                _lastResultPoint = Point.Empty;
                _lastResultAngle = 0;
                if (_lastValues != null)
                    _lastValues.Clear();

                if (_viewer != null)
                {
                    _viewer.InitCrossLine();
                    _viewer.ShowCrossLine(_viewer.VisibleCrossLine);
                    _viewer.Invalidate();
                }
            }
            catch { }
        }
        #endregion
        // =========================
        #region MeasurementRecipe -> VisionRecipe binding
        // =========================
        private void ApplyVisionRecipeFromMeasurement()
        {
            try
            {
                var eq = Equipment.Instance;
                var name = eq?.EquipmentRecipe?.CurrentRecipeName;

                // 무조건 EquipmentRecipe.CurrentRecipeName만 사용 (VisionRecipeName / UseVisionRecipe 무시)
                if (string.IsNullOrWhiteSpace(name))
                    name = "Default";

                // 정규화(접미사/확장자 제거 등)
                try { name = PatternMatchingRecipeStore.NormalizeRecipeName(name); } catch { }

                _currentRecipeName = name;
            }
            catch { }
        }
        #endregion
        // =========================
        #region Camera Binding
        // =========================
        private void TryBindEquipmentCameras()
        {
            try
            {
                var eq = Equipment.Instance;
                if (eq == null) return;
                if (eq.Cameras.Count == 0 && eq.EqState != EquipmentState.Initializing)
                {
                    try { eq.InitializeEquipment(); } catch { }
                }

                if (eq.Cameras.Count > 0)
                {
                    SetCameras(eq.Cameras.Values);
                }
            }
            catch (Exception ex)
            {
                Log.Write("PatternMatchingDialog", "TryBindEquipmentCameras error: " + ex.Message);
            }
        }

        public void SelectCamera(int index)
        {
            if (index < 0 || index >= _cameras.Count) return;
            if (cameraListBoxItemsView != null)
                cameraListBoxItemsView.SelectedIndex = index;
            ApplyCameraSelection(index);
        }

        public void SetCameras(IEnumerable<Camera> cameras)
        {
            _cameras.Clear();
            _cameraNames.Clear();
            if (cameras == null)
                return;

            foreach (var cam in cameras)
            {
                if (cam == null) continue;
                _cameras.Add(cam);
                _cameraNames.Add(cam.Name ?? $"Cam{_cameraNames.Count}");
            }
        }

        private void InitializeCameraList()
        {
            try
            {
                if (cameraListBoxItemsView == null || _viewer == null) return;
                cameraListBoxItemsView.ItemSelected -= OnCameraItemSelected;
                cameraListBoxItemsView.ItemSelected += OnCameraItemSelected;

                if (_cameras.Count == 0)
                {
                    cameraListBoxItemsView.SetItems();
                    return;
                }

                cameraListBoxItemsView.SetItems(_cameraNames.ToArray());

                _viewer.VisibleCrossLine = true;
                _viewer.FrameRate = 30;

                cameraListBoxItemsView.SelectedIndex = 0;
                ApplyCameraSelection(0);
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                cameraListBoxItemsView?.SetItems();
            }
        }

        private int _lastSelectedCameraIndex = -1;
        private void OnCameraItemSelected(object sender, int selectedIndex)
        {
            // [수정] 이전 선택과 동일하면 무시
            if (_lastSelectedCameraIndex == selectedIndex)
            {
                return;
            }
            
            _lastSelectedCameraIndex = selectedIndex;
            ApplyCameraSelection(selectedIndex);
        }

        private void ApplyCameraSelection(int index)
        {
            if (_viewer == null) 
                return;

            try
            {
                if (index < 0 || index >= _cameras.Count)
                    return;

                var cam = _cameras[index];
                if (cam == null) 
                    return;

                // [수정] 현재 이미 활성화된 카메라와 동일하면 리턴 (이미지가 사라지는 현상 방지)
                // 주의: _viewer.Camera가 null일 수도 있으므로 체크 필요
                if (_viewer.Camera != null && _viewer.Camera == cam)
                {
                    return;
                }

                // 0) runner detach (오버레이 포함)
                DetachRunnerForCurrentViewer();

                // 1) 이전 카메라 Live 중지 (안전하게)
                try
                {
                    if (_viewer.Camera != null && _viewer.Camera.IsLiveOn)
                    {
                        _viewer.Camera.StopLive();
                    }
                }
                catch (Exception ex) 
                { 
                    Log.Write(ex); 
                }

                // 2) 뷰어 업데이트 일시 중지 (화면 깜빡임 최소화)
                _viewer.SuspendDisplay();

                try
                {
                    // 3) 카메라 교체
                    _viewer.Camera = cam;

                    // 4) 오버레이만 정리 (이미지(Image/InputImage)는 아직 지우지 않음 -> Grab 실패 시 이전 화면이라도 남기기 위함 혹은 검은 화면 방지)
                    try
                    {
                        _viewer.ResultOverlays?.Clear();
                        _viewer.NormalOverlays?.Clear();
                        _viewer.InitCrossLine();
                    }
                    catch { }

                    // 5) 카메라 설정 (Simulated 해제 등)
                    try
                    {
                        _viewer.Simulated = false;
                        cam.SuspendedImageDisplay = false;
                    }
                    catch { }

                    _viewer.VisibleCrossLine = true;
                    _viewer.FrameRate = 30;

                    // =================================================================
                    // [핵심 수정 부분] 시뮬레이션 모드에서의 이미지 유지 전략
                    // =================================================================

                    bool imageRestored = false;

                    // 1. 카메라 객체가 마지막으로 가지고 있던 이미지가 있는지 확인
                    if (cam.LatestImage != null)
                    {
                        _viewer.SetImageNDisplay(cam.LatestImage);
                        imageRestored = true;
                    }

                    // 2. Live 시작 시도 (실제 장비 연결 시 필수)
                    try
                    {
                        // 시뮬레이션이고 이미지가 복구되었다면 굳이 StartLive를 해서 빈 화면을 만들 필요가 없음
                        // (단, 동영상 시뮬레이션이나 자동 생성 모드라면 StartLive가 필요할 수 있음)
                        bool skipLive = false;
                        if(cam.Config.IsSimulation && imageRestored)
                        {
                            skipLive = true;
                        }

                        if (skipLive == false)
                        {
                            cam.StartLive();

                            // StartLive 직후 Grab 시도
                            cam.GrabSync(out var snap);
                            if (snap != null)
                            {
                                _viewer.SetImageNDisplay(snap);
                            }
                            else if (imageRestored)
                            {
                                // GrabSync가 실패했지만(null), 기존 LatestImage가 있었다면 
                                // StartLive로 인해 뷰어가 클리어되는 것을 방지하기 위해 다시 설정
                                _viewer.SetImageNDisplay(cam.LatestImage);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Write(ex);
                    }

                    // 7) 스케일 등 뷰어 보조 기능 갱신
                    try
                    {
                        if (_viewer.Scale != null && cam.Resolution.Width > 0)
                        {
                            // [수정] 여기서 FitImageToScreen 호출
                            _viewer.FitImageToScreen();
                            // 휠/줌 리셋이 필요하다면 수행, 아니면 주석 처리하여 이전 줌 상태 유지 가능
                            //_viewer.Scale.Wheel = 1.0;
                            //_viewer.Scale.SetMousePoint(new Point(cam.Resolution.Width / 2, cam.Resolution.Height / 2));
                            //_viewer.Scale.MoveCenter(new Size(cam.Resolution.Width, cam.Resolution.Height));
                        }
                        _viewer.InitCrossLine();
                        _viewer.ShowCrossLine(_viewer.VisibleCrossLine);
                    }
                    catch { }

                    SyncImageInfoToControls(cam);
                }
                finally
                {
                    // 8) 뷰어 업데이트 재개
                    _viewer.ResumeDisplay();
                    _viewer.StartUpdateTask();
                }

                // 9) 레시피 로드 및 러너 연결
                _suspendAutoLoad = true;
                try { LoadRecipeForCurrentCamera(); }
                finally { _suspendAutoLoad = false; }

                BindUiToCurrentContext(cam);
                AttachRunnerForCurrentViewer();

                _viewer.Invalidate();
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                try 
                { 
                    _viewer.ResumeDisplay(); 
                } 
                catch { }
            }
        }

        private void UpdateRunnerModeFromUI()
        {
            if (_runner == null) return;
            if (radioSingle != null && radioSingle.Checked)
                _runner.SetSearchMode(PatternMatchingRunner.SearchMode.First);
            else if (radioMulti != null && radioMulti.Checked)
                _runner.SetSearchMode(PatternMatchingRunner.SearchMode.All);
        }

        private void ApplyOverlayOptionCheckboxes()
        {
            if (_runner == null) return;
            if (chkShowIndexes != null) _runner.SetShowMatchIndexes(chkShowIndexes.Checked);
            if (chkHighlightRef != null) _runner.SetHighlightReference(chkHighlightRef.Checked);
        }

        private void SyncImageInfoToControls(Camera cam)
        {
            if (cam == null) return;
            try
            {
                maintROIControl.SetOwner(_visionPart);
                maintROIControl.SetImageviwer(_viewer);
                maintROIControl.UpdateImageInfo(cam.Resolution);
                maintROIControl.EnsureDefaultRoiTools();
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
        }

        private void ResetViewerForCameraChange(Camera cam)
        {
            try
            {
                if (cam == null) return;
                try
                {
                    if (_viewer.Scale != null && cam.Resolution.Width > 0)
                    {
                        _viewer.Scale.Wheel = 1.0;
                        _viewer.Scale.SetMousePoint(new Point(cam.Resolution.Width / 2, cam.Resolution.Height / 2));
                        _viewer.Scale.MoveCenter(new Size(cam.Resolution.Width, cam.Resolution.Height));
                    }
                }
                catch { }

                _viewer.InitCrossLine();
                _viewer.ShowCrossLine(_viewer.VisibleCrossLine);

                cam.GrabSync(out var snap);
                if (snap != null)
                {
                    _viewer.SetImageNDisplay(snap);
                    maintROIControl.UpdateImageInfo(cam.Resolution);
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
        }

        private void ClearViewer()
        {
            try
            {
                if (_viewer == null) return;
                try { _viewer.Camera?.StopLive(); } catch { }
                try { _viewer.ResultOverlays?.Clear(); } catch { }
                try { _viewer.NormalOverlays?.Clear(); } catch { }
                try { _viewer.Image = null; } catch { }
                try { if (_viewer.InputImage != null) _viewer.InputImage = null; } catch { }

                _lastResultPoint = Point.Empty;
                _lastResultAngle = 0;
                if (_lastValues != null) _lastValues.Clear();
                _lastRunResult = null;

                _viewer.InitCrossLine();
                _viewer.ShowCrossLine(true);
                _viewer.Invalidate();
            }
            catch { }
        }

        // 카메라 전환 직전: 현재 뷰 흔적 제거
        private void PrepareViewerForCameraSwitch()
        {
            if (_viewer == null) return;
            try
            {
                _viewer.ResultOverlays?.Clear();
                _viewer.NormalOverlays?.Clear();
                _viewer.InitCrossLine(); // 버퍼만 초기화
            }
            catch { }
        }

        // 카메라 전환 직후: 새 카메라 기준으로 스케일/크로스 재구성 (Grab 없음)
        private void ResetViewerForCameraChange_NoGrab(Camera cam)
        {
            try
            {
                if (_viewer == null || cam == null) return;

                // scale reset (기존 유지)
                try
                {
                    if (_viewer.Scale != null && cam.Resolution.Width > 0)
                    {
                        _viewer.Scale.Wheel = 1.0;
                        _viewer.Scale.SetMousePoint(new Point(cam.Resolution.Width / 2, cam.Resolution.Height / 2));
                        _viewer.Scale.MoveCenter(new Size(cam.Resolution.Width, cam.Resolution.Height));
                    }
                }
                catch (Exception ex) { Log.Write(ex); }

                // ★추가/강화: 이전 오버레이 완전 제거
                _viewer.ResultOverlays?.Clear();
                _viewer.NormalOverlays?.Clear();

                // crossline 버퍼 재생성/표시
                _viewer.InitCrossLine();
                _viewer.ShowCrossLine(_viewer.VisibleCrossLine);
                _viewer.Invalidate();

                // 스냅샷 1회로 화면 안정화 (선택)
                try
                {
                    cam.GrabSync(out var snap);
                    if (snap != null)
                        _viewer.SetImageNDisplay(snap);
                }
                catch (Exception ex) { Log.Write(ex); }

                try { maintROIControl?.UpdateImageInfo(cam.Resolution); } 
                catch (Exception ex) { Log.Write(ex); }

                try
                {
                    if (_viewer.Scale != null && cam.Resolution.Width > 0 && cam.Resolution.Height > 0)
                    {
                        _viewer.Scale.Wheel = 1.0;
                        _viewer.Scale.SetMousePoint(new Point(cam.Resolution.Width / 2, cam.Resolution.Height / 2));
                        _viewer.Scale.MoveCenter(new Size(cam.Resolution.Width, cam.Resolution.Height));
                    }

                    _viewer.ResultOverlays?.Clear();
                    _viewer.NormalOverlays?.Clear();

                    _viewer.InitCrossLine();
                    _viewer.ShowCrossLine(_viewer.VisibleCrossLine);
                    _viewer.Invalidate();
                }
                catch (Exception ex)
                {
                    Log.Write(ex);
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
        }





        #endregion
        // =========================
        #region Recipe Save / Load
        // =========================
        private void LoadRecipeForCurrentCamera()
        {
            var camName = GetCurrentCameraName();
            if (string.IsNullOrWhiteSpace(camName))
                camName = "NoCamera";

            // 1) UI 파라미터/ROI 로드 (★이게 있어야 화면이 바뀜)
            LoadRecipe(_currentRecipeName, camName);

            // 2) runner 로드
            if (_runner == null)
                AttachRunnerForCurrentViewer();

            _runner?.SetRecipe(_currentRecipeName);
            _runner?.LoadRecipe();
        }

        private string GetRecipePath(string cameraName, string recipeName)
        {
            return PatternMatchingRecipeStore.ResolveRecipePath(
                _recipeDirectory,
                cameraName ?? "NoCamera",
                recipeName ?? "Default",
                createDirectoryForSave: true);
        }


        private void SaveRecipeForCurrentCamera()
        {
            var camName = GetCurrentCameraName();
            if (string.Equals(camName, "NoCamera", StringComparison.OrdinalIgnoreCase) || string.IsNullOrWhiteSpace(camName))
            {
                UpdateStatus("Save skipped: No camera selected.");
                try
                {
                    var mb = new MessageBoxOk();
                    mb.ShowDialog("Notification!", "카메라 선택 후 저장하세요.");
                }
                catch { }
                return;
            }
            SaveRecipe(_currentRecipeName, camName);
        }

        private void SaveRecipe(string recipeName, string cameraName = null)
        {
            try
            {
                maintROIControl?.CommitCurrentRoi();
                SyncParametersFromUI();

                if (string.IsNullOrEmpty(cameraName)) 
                    cameraName = GetCurrentCameraName();

                var path = GetRecipePath(cameraName, recipeName);
                var roi = new PatternMatchingRoiJson();
                try
                {
                    roi.TrainStart = _visionPart.GetTrainStartPoint();
                    roi.TrainEnd = _visionPart.GetTrainEndPoint();
                    roi.InspectStart = _visionPart.GetInspectStartPoint();
                    roi.InspectEnd = _visionPart.GetInspectEndPoint();
                }
                catch { }

                if (_parameters == null) 
                    _parameters = _visionPart.GetPatternMatchingParameters();

                var container = PatternMatchingRecipeStore.Load(path, CurrentModeKey, true) ?? new PatternMatchingRecipeJson();
                container.LastCameraName = cameraName;
                container.Parameters = _parameters?.Clone();
                container.Roi = roi;

                PatternMatchingRecipeStore.Save(path, container, CurrentModeKey);
                UpdateStatus($"Recipe saved: {path}");
            }
            catch (Exception ex)
            {
                var mb = new MessageBoxOk();
                mb.ShowDialog("Error!", "Recipe 저장 실패: " + ex.Message);
                Log.Write(ex);
            }
        }

        private void LoadRecipe(string recipeName, string cameraName = null)
        {
            try
            {
                if (string.IsNullOrEmpty(cameraName))
                    cameraName = GetCurrentCameraName();

                var path = GetRecipePath(cameraName, recipeName);
                var container = PatternMatchingRecipeStore.Load(path, CurrentModeKey, fallbackLegacy: true);
                if (container == null) 
                { 
                    UpdateStatus("Recipe 파일 없음. 새로 생성 예정."); 
                    return; 
                }

                if (container.Parameters != null)
                {
                    _parameters = container.Parameters.Clone();
                    _visionPart.SetPatternMatchingParameters(_parameters);
                    patternMatchingParamControl?.UpdateParameters(_parameters);
                }

                if (container.Roi != null)
                {
                    _visionPart.SetTrainStartPoint(container.Roi.TrainStart);
                    _visionPart.SetTrainEndPoint(container.Roi.TrainEnd);
                    _visionPart.SetInspectStartPoint(container.Roi.InspectStart);
                    _visionPart.SetInspectEndPoint(container.Roi.InspectEnd);
                    maintROIControl?.ReloadRoiFromPart();
                }

                maintROIControl.SetOwner(_visionPart);
                maintROIControl.EnsureDefaultRoiTools();
                AttachEvents();

                UpdateStatus($"Recipe loaded: {path}");
            }
            catch (Exception ex)
            {
                var mb = new MessageBoxOk();
                mb.ShowDialog("Error!", "Recipe 로드 실패: " + ex.Message);
                Log.Write(ex);
            }
        }
        #endregion

        // =========================
        #region Search
        // =========================
        private VisionImage AcquireCurrentSearchImage()
        {
            VisionImage src = null;
            try
            {
                var cam = _viewer?.Camera;
                if (cam != null && cam.Opened && cam.LatestImage?.RawData != null)
                    src = cam.LatestImage;

                if (src == null && cam != null && cam.Opened)
                {
                    try { cam.GrabSync(out src); }
                    catch (Exception ex) { Log.Write(ex); }
                }

                if (src == null && _viewer?.InputImage?.RawData != null)
                    src = _viewer?.InputImage;

                if (src == null && _visionPart.TestImage?.RawData != null)
                    src = _visionPart.TestImage;
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
            return src;
        }

        private PatternMatchingRunner.PatternMatchRunResult SearchWithCurrentMode(VisionImage testImage, bool save = false)
        {
            if (_runner == null)
                return null;

            // 핵심: 현재 컨트롤 모드를 runner에 강제 적용
            _runner.SetProcessMode(_currentMode);
            // 핵심: 모드별 검색 엔트리 사용
            return _runner.SearchByCurrentMode(testImage, save);
        }

        private void BtnSearch_Click(object sender, EventArgs e)
        {
            try
            {
                var totalWatch = System.Diagnostics.Stopwatch.StartNew();
                SyncParametersFromUI();

                if (_parameters == null)
                    _parameters = _visionPart.GetPatternMatchingParameters();

                if (_parameters == null
                    || _parameters.TrainImages == null
                    || _parameters.TrainImages.Count == 0
                    || _parameters.TrainImages.All(v => v == null || v.GetImage() == null))
                {
                    var mb = new MessageBoxOk();
                    mb.ShowDialog("Notification!", "최소 1개 이상의 Train Image가 필요합니다.");
                    return;
                }

                var testImage = AcquireCurrentSearchImage();
                if (testImage == null || testImage.GetImage() == null)
                {
                    var mb = new MessageBoxOk();
                    mb.ShowDialog("Notification!", "검색할 이미지(카메라 또는 로드된 이미지)가 없습니다.");
                    return;
                }

                maintROIControl?.CommitCurrentRoi();

                if (_runner == null)
                    AttachRunnerForCurrentViewer();

                if (_runner == null)
                {
                    var mb = new MessageBoxOk();
                    mb.ShowDialog("Error!", "Runner 초기화 실패 (카메라 없음/Hub 실패)");
                    return;
                }

                // [ADD] 현재 모드/카메라의 ROI/파라미터를 먼저 파일에 반영
                SaveRecipeForCurrentCamera();
                _runner.SetProcessMode(_currentMode);
                _runner.LoadRecipe();
                // [FIX] searchWatch 선언/측정 추가
                var searchWatch = System.Diagnostics.Stopwatch.StartNew();
                var res = _runner.SearchByCurrentMode(testImage, save: false);
                searchWatch.Stop();

                if (res == null)
                {
                    totalWatch.Stop();
                    UpdateStatus($"Search Fail: runner returned null. (Total: {totalWatch.ElapsedMilliseconds}ms, Search: {searchWatch.ElapsedMilliseconds}ms)");
                    return;
                }

                _lastRunResult = res;

                if (!res.Success || res.RawResult == null)
                {
                    totalWatch.Stop();
                    string failTimeMsg = $"Total: {totalWatch.ElapsedMilliseconds}ms, Search: {searchWatch.ElapsedMilliseconds}ms";

                    UpdateStatus($"Search Fail: {res.FailReason} ({failTimeMsg})");

                    var mb = new MessageBoxOk();
                    mb.ShowDialog("Notification!", $"Search 실패: {res.FailReason}\n소요시간: {searchWatch.ElapsedMilliseconds}ms");

                    listViewResults.Items.Clear();
                    _lastValues.Clear();
                    _viewer?.Invalidate();
                    return;
                }

                var raw = res.RawResult;
                _lastValues = raw.Values != null
                    ? new List<PatternMatchingResult.PatternMatchingResultValue>(raw.Values)
                    : new List<PatternMatchingResult.PatternMatchingResultValue>();

                PopulateResultList();

                if (_lastValues.Count > 0)
                {
                    int idx = (res.ReferenceIndex >= 0 && res.ReferenceIndex < _lastValues.Count) ? res.ReferenceIndex : 0;
                    var first = _lastValues[idx];
                    _lastResultPoint = new Point((int)first.X, (int)first.Y);
                    _lastResultAngle = first.R;
                    txtResultX.Text = first.X.ToString("0.000");
                    txtResultY.Text = first.Y.ToString("0.000");
                    txtResultT.Text = first.R.ToString("0.000");
                }
                else
                {
                    txtResultX.Clear();
                    txtResultY.Clear();
                    txtResultT.Clear();
                    _lastResultPoint = Point.Empty;
                    _lastResultAngle = 0;
                }

                _viewer?.Invalidate();

                if (txtAvgX != null && txtAvgY != null && txtAvgT != null)
                {
                    bool isAll = radioMulti != null && radioMulti.Checked;
                    if (isAll && res.AvgXExcludingExtremes.HasValue)
                    {
                        txtAvgX.Text = res.AvgXExcludingExtremes.Value.ToString("0.000");
                        txtAvgY.Text = res.AvgYExcludingExtremes.Value.ToString("0.000");
                        txtAvgT.Text = res.AvgRExcludingExtremes.Value.ToString("0.000");
                    }
                    else
                    {
                        txtAvgX.Clear();
                        txtAvgY.Clear();
                        txtAvgT.Clear();
                    }
                }

                ApplyMarksFoundOverlaysFromLastRun();

                totalWatch.Stop();
                string timeMsg = $"Total: {totalWatch.ElapsedMilliseconds}ms, Search: {searchWatch.ElapsedMilliseconds}ms";
                UpdateStatus($"Search Success. ({timeMsg})");
                try { Log.Write("PatternMatchingControl", $"Tact Time -> {timeMsg}"); } catch { }
            }
            catch (Exception ex)
            {
                var mb = new MessageBoxOk();
                mb.ShowDialog("Error!", "Search 예외: " + ex.Message);
                UpdateStatus("Search exception");
            }
        }

        //private void BtnSearch_Click(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        // 1. 전체 소요 시간 측정 시작
        //        var totalWatch = System.Diagnostics.Stopwatch.StartNew();

        //        SyncParametersFromUI();

        //        if (_parameters == null)
        //            _parameters = _visionPart.GetPatternMatchingParameters();

        //        if (_parameters == null
        //            || _parameters.TrainImages == null
        //            || _parameters.TrainImages.Count == 0
        //            || _parameters.TrainImages.All(v => v == null || v.GetImage() == null))
        //        {
        //            var mb = new MessageBoxOk();
        //            mb.ShowDialog("Notification!", "최소 1개 이상의 Train Image가 필요합니다.");
        //            return;
        //        }

        //        var testImage = AcquireCurrentSearchImage();
        //        if (testImage == null || testImage.GetImage() == null)
        //        {
        //            var mb = new MessageBoxOk();
        //            mb.ShowDialog("Notification!", "검색할 이미지(카메라 또는 로드된 이미지)가 없습니다.");
        //            return;
        //        }

        //        maintROIControl?.CommitCurrentRoi();

        //        // [최적화] 1. 매 검색마다 발생하는 불필요한 레시피 디스크 저장(I/O) 제외
        //        // 파라미터 저장은 별도의 "저장" 버튼 클릭 시에만 수행하도록 유도합니다.
        //        // SaveRecipeForCurrentCamera(); 

        //        if (_runner == null)
        //            AttachRunnerForCurrentViewer();

        //        if (_runner == null)
        //        {
        //            var mb = new MessageBoxOk();
        //            mb.ShowDialog("Error!", "Runner 초기화 실패 (카메라 없음/Hub 실패)");
        //            return;
        //        }

        //        // [최적화] 2. 러너에 레시피 파라미터를 강제 업데이트 (파일 저장을 안했으므로 메모리 갱신만)
        //        if (_runner.Parameters != null && _parameters != null)
        //        {
        //            //_runner.Parameters.CopyFrom(_parameters); // 구현된 파라미터 복사 메서드 사용(또는 참조 업데이트)
        //            _runner.LoadRecipe();
        //        }

        //        UpdateRunnerModeFromUI();

        //        // ----------------------------------------
        //        // 2. 순수 비전 알고리즘(Search) 소요 시간 측정
        //        // ----------------------------------------
        //        var searchWatch = System.Diagnostics.Stopwatch.StartNew();
        //        var res = _runner.Search(testImage, save: false);
        //        searchWatch.Stop();
        //        // ----------------------------------------

        //        _lastRunResult = res;

        //        if (!res.Success || res.RawResult == null)
        //        {
        //            totalWatch.Stop();
        //            string failTimeMsg = $"Total: {totalWatch.ElapsedMilliseconds}ms, Search: {searchWatch.ElapsedMilliseconds}ms";

        //            UpdateStatus($"Search Fail: {res.FailReason} ({failTimeMsg})");

        //            var mb = new MessageBoxOk();
        //            mb.ShowDialog("Notification!", $"Search 실패: {res.FailReason}\n소요시간: {searchWatch.ElapsedMilliseconds}ms");

        //            listViewResults.Items.Clear();
        //            _lastValues.Clear();
        //            _viewer?.Invalidate();
        //            return;
        //        }

        //        var raw = res.RawResult;
        //        _lastValues = raw.Values != null
        //            ? new List<PatternMatchingResult.PatternMatchingResultValue>(raw.Values)
        //            : new List<PatternMatchingResult.PatternMatchingResultValue>();

        //        PopulateResultList();

        //        if (_lastValues.Count > 0)
        //        {
        //            int idx = (res.ReferenceIndex >= 0 && res.ReferenceIndex < _lastValues.Count) ? res.ReferenceIndex : 0;
        //            var first = _lastValues[idx];
        //            _lastResultPoint = new Point((int)first.X, (int)first.Y);
        //            _lastResultAngle = first.R;
        //            txtResultX.Text = first.X.ToString("0.000");
        //            txtResultY.Text = first.Y.ToString("0.000");
        //            txtResultT.Text = first.R.ToString("0.000");
        //        }
        //        else
        //        {
        //            txtResultX.Clear(); txtResultY.Clear(); txtResultT.Clear();
        //            _lastResultPoint = Point.Empty; _lastResultAngle = 0;
        //        }

        //        _viewer?.Invalidate();

        //        if (txtAvgX != null && txtAvgY != null && txtAvgT != null)
        //        {
        //            bool isAll = radioMulti != null && radioMulti.Checked;
        //            if (isAll && res.AvgXExcludingExtremes.HasValue)
        //            {
        //                txtAvgX.Text = res.AvgXExcludingExtremes.Value.ToString("0.000");
        //                txtAvgY.Text = res.AvgYExcludingExtremes.Value.ToString("0.000");
        //                txtAvgT.Text = res.AvgRExcludingExtremes.Value.ToString("0.000");
        //            }
        //            else
        //            {
        //                txtAvgX.Clear(); txtAvgY.Clear(); txtAvgT.Clear();
        //            }
        //        }

        //        ApplyMarksFoundOverlaysFromLastRun();//RebuildResultOverlays();

        //        // 3. 전체 소요 시간 측정 종료
        //        totalWatch.Stop();

        //        // 결과 출력 포맷 (예: "Total: 150ms, Search: 120ms")
        //        string timeMsg = $"Total: {totalWatch.ElapsedMilliseconds}ms, Search: {searchWatch.ElapsedMilliseconds}ms";

        //        UpdateStatus($"Search Success. ({timeMsg})");

        //        // 필요시 로그에도 기록
        //        try { Log.Write("PatternMatchingControl", $"Tact Time -> {timeMsg}"); } catch { }
        //    }
        //    catch (Exception ex)
        //    {
        //        var mb = new MessageBoxOk();
        //        mb.ShowDialog("Error!", "Search 예외: " + ex.Message);
        //        UpdateStatus("Search exception");
        //    }
        //}

        private void PopulateResultList()
        {
            if (listViewResults == null) return;
            listViewResults.BeginUpdate();
            try
            {
                listViewResults.Items.Clear();
                for (int i = 0; i < _lastValues.Count; i++)
                {
                    var v = _lastValues[i];
                    var item = new ListViewItem(new string[]
                    {
                        i.ToString(),
                        v.X.ToString("0.000"),
                        v.Y.ToString("0.000"),
                        v.R.ToString("0.000"),
                        v.Score.ToString("0.000")
                    })
                    { Tag = (PatternMatchingResult.PatternMatchingResultValue?)v };
                    listViewResults.Items.Add(item);
                }
            }
            finally
            {
                listViewResults.EndUpdate();
            }
        }

        private void OnResultListSelectionChanged()
        {
            if (listViewResults == null) return;
            if (listViewResults.SelectedItems.Count <= 0) return;
            var sel = listViewResults.SelectedItems[0];
            var val = sel.Tag as PatternMatchingResult.PatternMatchingResultValue?;
            if (val == null) return;
            var vv = val.Value;

            _lastResultPoint = new Point((int)vv.X, (int)vv.Y);
            _lastResultAngle = vv.R;

            if (txtResultX != null) txtResultX.Text = vv.X.ToString("0.000");
            if (txtResultY != null) txtResultY.Text = vv.Y.ToString("0.000");
            if (txtResultT != null) txtResultT.Text = vv.R.ToString("0.000");

            ApplyMarksFoundOverlaysFromLastRun();//RebuildResultOverlays();
            _viewer?.Invalidate();
        }
        #endregion
        // =========================
        #region Events / ROI
        // =========================
        private void patternMatchingParamControl_Load(object sender, EventArgs e)
        {
            patternMatchingParamControl?.UpdateParameters(_parameters);
        }

        private void AttachEvents()
        {
            if (maintROIControl != null)
            {
                maintROIControl.TrainImageCaptured -= MaintROIControl_TrainImageCaptured;
                maintROIControl.TrainImageCaptured += MaintROIControl_TrainImageCaptured;
            }
        }

        private void MaintROIControl_TrainImageCaptured(VisionImage image)
        {
            try
            {
                if (image == null || image.GetImage() == null) return;
                SyncParametersFromUI();
                AddTrainImage(image);
            }
            catch (Exception ex)
            {
                Log.Write("PatternMatchingDialog", "TrainImageCaptured handler exception: " + ex.Message);
            }
        }

        private void AddTrainImage(VisionImage cut)
        {
            if (cut == null || cut.RawData == null || cut.Header == null || cut.Header.Width <= 0 || cut.Header.Height <= 0) return;
            maintROIControl?.CommitCurrentRoi();
            _parameters.TrainImages.RemoveAll(v => v == null || v.RawData == null);
            cut.Tag = $"Train_{_parameters.TrainImages.Count}";
            _parameters.TrainImages.Add(cut);
            patternMatchingParamControl?.UpdateParameters(_parameters);
            UpdateStatus($"Train image added (Count={_parameters.TrainImages.Count})");
        }
        #endregion
        // =========================
        #region Overlay
        // =========================
        private void ApplyMarksFoundOverlaysFromLastRun()
        {
            if (_viewer == null || _viewer.IsDisposed) return;
            if (_lastRunResult == null || !_lastRunResult.Success) return;

            // _lastValues가 비어있으면 _lastRunResult.Matches에서 채움
            if ((_lastValues == null || _lastValues.Count == 0) && _lastRunResult.Matches != null)
                _lastValues = new List<PatternMatchingResult.PatternMatchingResultValue>(_lastRunResult.Matches);

            if (_lastValues == null || _lastValues.Count == 0) return;

            // Train image 크기(overlay 사각형 크기용)
            int trainW = 40, trainH = 40;
            try
            {
                var ti = _parameters?.TrainImages?.FirstOrDefault(t => t?.Header != null && t.Header.Width > 0 && t.Header.Height > 0);
                if (ti != null) { trainW = ti.Header.Width; trainH = ti.Header.Height; }
            }
            catch { }

            // 대표 index 결정(기존 로직과 동일하게)
            int repIndex = (_lastRunResult.ReferenceIndex >= 0 && _lastRunResult.ReferenceIndex < _lastValues.Count)
                ? _lastRunResult.ReferenceIndex
                : 0;

            if (listViewResults?.SelectedIndices.Count > 0)
                repIndex = listViewResults.SelectedIndices[0];

            if (repIndex < 0 || repIndex >= _lastValues.Count)
                repIndex = 0;

            // --- 아래는 프로젝트의 실제 타입에 맞게 조정 필요 ---
            // PatternMarksFoundEventArgs / Mark 타입(예: PatternMarkResult 등) 정확한 정의에 맞춰서 생성하세요.
            var e = new PatternMarksFoundEventArgs
            {
                Image = AcquireCurrentSearchImage(),  // 가능하면 마지막 검색 이미지로
                RepresentativeIndex = repIndex,
                Marks = _lastValues.Select(v => new PatternMatchInfo
                {
                    X = v.X,
                    Y = v.Y,
                    AngleDeg = v.R,
                    Score = v.Score,
                    TrainW = trainW,
                    TrainH = trainH
                }).ToList()
            };

            var opt = new VisionRunnerHub.MarksOverlayOptions
            {
                // 필요하면 X/Y축 ROI 선택 로직 넣기
                UseXAxisRoi = true,

                // Recipe ROI를 넘기면 ApplyMarksFoundOverlays가 "RecipeROI"로 그려줌
                RecipeInspectRoiProvider = () =>
                {
                    try
                    {
                        if (_visionPart != null && _visionPart.UseInspectRoi)
                            return (_visionPart.GetInspectStartPoint(), _visionPart.GetInspectEndPoint());
                    }
                    catch { }
                    return null; // 없으면 CenterROI fallback
                },

                ShowTextInfo = true,
                TextLocation = new Point(10, 50),

                RepresentativeColor = Color.Yellow,
                NormalColor = Color.Lime,
                RecipeRoiColor = Color.Orange,
                FallbackRoiColor = Color.Cyan
            };

            VisionRunnerHub.ApplyMarksFoundOverlays(_viewer, e, opt);
        }

        #endregion
        // =========================
        #region UI Binding
        // =========================
        private void BindUiToCurrentContext(Camera cam = null)
        {
            try
            {
                if (maintROIControl != null)
                {
                    maintROIControl.SetOwner(_visionPart);
                    maintROIControl.SetImageviwer(_viewer);
                    if (cam != null)
                        maintROIControl.UpdateImageInfo(cam.Resolution);
                    maintROIControl.EnsureDefaultRoiTools();
                }

                if (_parameters == null && _visionPart != null)
                    _parameters = _visionPart.GetPatternMatchingParameters();
                patternMatchingParamControl?.UpdateParameters(_parameters);
            }
            catch { }
        }
        #endregion
        // =========================
        #region Internal VisionPart Wrapper
        // =========================
        private class InternalMultiPatternMatchingVisionPart : MultiPatternMatchingVisionPart
        {
            private MultiPatternMatchingParameters _parameters = new MultiPatternMatchingParameters();
            public InternalMultiPatternMatchingVisionPart(string name) : base(name) { Simulated = true; }
            public override MultiPatternMatchingParameters GetPatternMatchingParameters() => _parameters;
            public override void SetPatternMatchingParameters(MultiPatternMatchingParameters parameters)
            {
                _parameters = parameters ?? new MultiPatternMatchingParameters();
            }
            public override IlluminationDataSet GetIlluminationDataSet() => null;
        }
        #endregion

        // ==========================================================
        // ======== [MOVE TO END] Legacy / Unused / Duplicate ========
        // ==========================================================
        
        // UI Event wrappers
        private void listViewResults_SelectedIndexChanged(object sender, EventArgs e)
        {
            OnResultListSelectionChanged();
        }

        private void SearchModeRadioChanged(object sender, EventArgs e)
        {
            UpdateRunnerModeFromUI();
            if (radioSingle != null && radioSingle.Checked)
            {
                txtAvgX.Clear(); txtAvgY.Clear(); txtAvgT.Clear();
            }
            ApplyMarksFoundOverlaysFromLastRun();//RebuildResultOverlays();
        }

        private void Close()
        {
            try
            {
                var form = this.FindForm();
                if (form != null)
                    form.Close();
                else
                    this.Dispose();
            }
            catch { }
        }

        private void _btnLoadParam_Click(object sender, EventArgs e)
        {
            LoadRecipeForCurrentCamera();
        }

        private void _btnSaveParam_Click(object sender, EventArgs e)
        {
            try
            {
                maintROIControl?.CommitCurrentRoi();

                var ask = new MessageBoxYesNo();
                if (ask.ShowDialog("저장 확인", "현재 설정을 저장하시겠습니까?") == DialogResult.Yes)
                {
                    SaveRecipeForCurrentCamera();
                    _runner?.LoadRecipe();
                }
                else
                {
                    UpdateStatus("Save canceled");
                }
            }
            catch (Exception ex)
            {
                var mb = new MessageBoxOk();
                mb.ShowDialog("Error!", $"저장 중 오류: " + ex.Message);
            }
        }

        private void txtResultT_TextChanged(object sender, EventArgs e) { }
        private void tableLayoutPanel8_Paint(object sender, PaintEventArgs e) { }
        private void lblRX_Click(object sender, EventArgs e) { }

        private void _btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void _btnSearchOnce_Click(object sender, EventArgs e)
        {
            try
            {
                SyncParametersFromUI();

                if (_parameters == null)
                    _parameters = _visionPart.GetPatternMatchingParameters();

                if (_parameters == null
                    || _parameters.TrainImages == null
                    || _parameters.TrainImages.Count == 0
                    || _parameters.TrainImages.All(v => v == null || v.GetImage() == null))
                {
                    var mb = new MessageBoxOk();
                    mb.ShowDialog("Notification!", "최소 1개 이상의 Train Image가 필요합니다.");
                    return;
                }

                var testImage = AcquireCurrentSearchImage();
                if (testImage == null || testImage.GetImage() == null)
                {
                    var mb = new MessageBoxOk();
                    mb.ShowDialog("Notification!", "검색할 이미지(카메라 또는 로드된 이미지)가 없습니다.");
                    return;
                }

                maintROIControl?.CommitCurrentRoi();
                SaveRecipeForCurrentCamera();

                if (_runner == null)
                    AttachRunnerForCurrentViewer();

                if (_runner == null)
                {
                    var mb = new MessageBoxOk();
                    mb.ShowDialog("Error!", "Runner 초기화 실패 (카메라 없음/Hub 실패)");
                    return;
                }

                _runner.SetProcessMode(_currentMode);
                _runner.LoadRecipe();
                var res = _runner.SearchByCurrentMode(testImage, save: false);

                //_runner.LoadRecipe();
                //var res = SearchWithCurrentMode(testImage, save: false);
                
                _lastRunResult = res;
                if (res == null || !res.Success || res.RawResult == null)
                {
                    UpdateStatus("Search Fail: " + (res?.FailReason ?? "runner returned null"));

                    var mb = new MessageBoxOk();
                    mb.ShowDialog("Notification!", $"Search 실패: {res?.FailReason}");

                    listViewResults.Items.Clear();
                    _lastValues.Clear();
                    txtResultX.Clear();
                    txtResultY.Clear();
                    txtResultT.Clear();

                    _lastResultPoint = Point.Empty;
                    _lastResultAngle = 0;

                    _viewer?.Invalidate();
                    return;
                }

                var raw = res.RawResult;
                _lastValues = raw.Values != null
                    ? new List<PatternMatchingResult.PatternMatchingResultValue>(raw.Values)
                    : new List<PatternMatchingResult.PatternMatchingResultValue>();

                PopulateResultList();

                if (_lastValues.Count > 0)
                {
                    int idx = (res.ReferenceIndex >= 0 && res.ReferenceIndex < _lastValues.Count) ? res.ReferenceIndex : 0;
                    var centerMost = _lastValues[idx];

                    _lastResultPoint = new Point((int)centerMost.X, (int)centerMost.Y);
                    _lastResultAngle = centerMost.R;

                    txtResultX.Text = centerMost.X.ToString("0.000");
                    txtResultY.Text = centerMost.Y.ToString("0.000");
                    txtResultT.Text = centerMost.R.ToString("0.000");

                    try
                    {
                        listViewResults.SelectedItems.Clear();
                        if (idx >= 0 && idx < listViewResults.Items.Count)
                        {
                            listViewResults.Items[idx].Selected = true;
                            listViewResults.Items[idx].Focused = true;
                            listViewResults.EnsureVisible(idx);
                        }
                    }
                    catch { }
                }
                else
                {
                    txtResultX.Clear();
                    txtResultY.Clear();
                    txtResultT.Clear();
                    _lastResultPoint = Point.Empty;
                    _lastResultAngle = 0;
                }

                UpdateStatus("Search Success.");
                _viewer?.Invalidate();
                ApplyMarksFoundOverlaysFromLastRun();
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                var mb = new MessageBoxOk();
                mb.ShowDialog("Error!", "Search 예외: " + ex.Message);
                UpdateStatus("Search exception");
            }
        }
        //private void _btnSearchOnce_Click(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        SyncParametersFromUI();

        //        if (_parameters == null)
        //            _parameters = _visionPart.GetPatternMatchingParameters();

        //        if (_parameters == null
        //            || _parameters.TrainImages == null
        //            || _parameters.TrainImages.Count == 0
        //            || _parameters.TrainImages.All(v => v == null || v.GetImage() == null))
        //        {
        //            var mb = new MessageBoxOk();
        //            mb.ShowDialog("Notification!", "최소 1개 이상의 Train Image가 필요합니다.");
        //            return;
        //        }

        //        var testImage = AcquireCurrentSearchImage();
        //        if (testImage == null || testImage.GetImage() == null)
        //        {
        //            var mb = new MessageBoxOk();
        //            mb.ShowDialog("Notification!", "검색할 이미지(카메라 또는 로드된 이미지)가 없습니다.");
        //            return;
        //        }

        //        maintROIControl?.CommitCurrentRoi();
        //        SaveRecipeForCurrentCamera();

        //        _runner.LoadRecipe();
        //        var res = _runner.SearchCenterMark(testImage, save: false);
        //        _lastRunResult = res;

        //        if (!res.Success || res.RawResult == null)
        //        {
        //            UpdateStatus("CenterMark Search Fail: " + res.FailReason);

        //            var mb = new MessageBoxOk();
        //            mb.ShowDialog("Notification!", $"CenterMark Search 실패: {res.FailReason}");

        //            listViewResults.Items.Clear();
        //            _lastValues.Clear();
        //            txtResultX.Clear();
        //            txtResultY.Clear();
        //            txtResultT.Clear();

        //            _lastResultPoint = Point.Empty;
        //            _lastResultAngle = 0;

        //            _viewer?.Invalidate();
        //            return;
        //        }

        //        var raw = res.RawResult;
        //        _lastValues = raw.Values != null
        //            ? new List<PatternMatchingResult.PatternMatchingResultValue>(raw.Values)
        //            : new List<PatternMatchingResult.PatternMatchingResultValue>();

        //        PopulateResultList();

        //        if (_lastValues.Count > 0)
        //        {
        //            int idx = (res.ReferenceIndex >= 0 && res.ReferenceIndex < _lastValues.Count) ? res.ReferenceIndex : 0;
        //            var centerMost = _lastValues[idx];

        //            _lastResultPoint = new Point((int)centerMost.X, (int)centerMost.Y);
        //            _lastResultAngle = centerMost.R;

        //            txtResultX.Text = centerMost.X.ToString("0.000");
        //            txtResultY.Text = centerMost.Y.ToString("0.000");
        //            txtResultT.Text = centerMost.R.ToString("0.000");

        //            try
        //            {
        //                listViewResults.SelectedItems.Clear();
        //                if (idx >= 0 && idx < listViewResults.Items.Count)
        //                {
        //                    listViewResults.Items[idx].Selected = true;
        //                    listViewResults.Items[idx].Focused = true;
        //                    listViewResults.EnsureVisible(idx);
        //                }
        //            }
        //            catch { }
        //        }
        //        else
        //        {
        //            txtResultX.Clear();
        //            txtResultY.Clear();
        //            txtResultT.Clear();
        //            _lastResultPoint = Point.Empty;
        //            _lastResultAngle = 0;
        //        }

        //        UpdateStatus("CenterMark Search Success.");
        //        _viewer?.Invalidate();
        //        ApplyMarksFoundOverlaysFromLastRun();//RebuildResultOverlays();
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.Write(ex);
        //        var mb = new MessageBoxOk();
        //        mb.ShowDialog("Error!", "CenterMark Search 예외: " + ex.Message);
        //        UpdateStatus("CenterMark Search exception");
        //    }
        //}

        private void BtnSaveParam_Click(object sender, EventArgs e)
        {
            try
            {
                maintROIControl?.CommitCurrentRoi();

                var ask = new MessageBoxYesNo();
                if (ask.ShowDialog("저장 확인", "현재 설정을 저장하시겠습니까?") == DialogResult.Yes)
                {
                    SaveRecipeForCurrentCamera();
                }
                else
                {
                    UpdateStatus("Save canceled");
                }
            }
            catch (Exception ex)
            {
                var mb = new MessageBoxOk();
                mb.ShowDialog("Error!", "저장 중 오류: " + ex.Message);
            }
        }

        private void Viewer_PaintCross(object sender, PaintEventArgs e)
        {
            if (_lastResultPoint == Point.Empty)
                return;

            if (_viewer?.Image == null)
                return;

            try
            {
                var imgW = _viewer.Image.Width;
                var imgH = _viewer.Image.Height;
                var boxW = _viewer.ClientSize.Width;
                var boxH = _viewer.ClientSize.Height;
                double scale = Math.Min((double)boxW / imgW, (double)boxH / imgH);
                int drawW = (int)(imgW * scale);
                int drawH = (int)(imgH * scale);
                int offX = (boxW - drawW) / 2;
                int offY = (boxH - drawH) / 2;

                int cx = offX + (int)(_lastResultPoint.X * scale);
                int cy = offY + (int)(_lastResultPoint.Y * scale);
                int len = 15;
                using (Pen p = new Pen(Color.Red, 2))
                {
                    e.Graphics.DrawLine(p, cx - len, cy, cx + len, cy);
                    e.Graphics.DrawLine(p, cx, cy - len, cx, cy + len);
                }
            }
            catch { }
        }

        private void Viewer_PaintMatches(object sender, PaintEventArgs e)
        {
            if (_viewer?.Image == null) return;
            if ((_lastValues == null || _lastValues.Count == 0) && (_lastRunResult == null || !_lastRunResult.Success)) return;

            try
            {
                if ((_lastValues == null || _lastValues.Count == 0) && _lastRunResult?.Matches != null && _lastRunResult.Matches.Count > 0)
                {
                    _lastValues = new List<PatternMatchingResult.PatternMatchingResultValue>(_lastRunResult.Matches);
                }

                var img = _viewer.Image;
                int imgW = img.Width;
                int imgH = img.Height;
                int boxW = _viewer.ClientSize.Width;
                int boxH = _viewer.ClientSize.Height;
                double scale = Math.Min((double)boxW / imgW, (double)boxH / imgH);
                int drawW = (int)(imgW * scale);
                int drawH = (int)(imgH * scale);
                int offX = (boxW - drawW) / 2;
                int offY = (boxH - drawH) / 2;

                bool showIdx = chkShowIndexes?.Checked ?? false;
                bool highlight = chkHighlightRef?.Checked ?? false;

                int patternW = 40, patternH = 40;
                try
                {
                    var ti = _parameters?.TrainImages?.FirstOrDefault(t => t?.Header != null && t.Header.Width > 0 && t.Header.Height > 0);
                    if (ti != null) { patternW = ti.Header.Width; patternH = ti.Header.Height; }
                }
                catch { }

                Point roiStart = Point.Empty; Point roiEnd = Point.Empty;
                try
                {
                    if (_visionPart != null && _visionPart.UseInspectRoi)
                    {
                        roiStart = _visionPart.GetInspectStartPoint();
                        roiEnd = _visionPart.GetInspectEndPoint();
                    }
                }
                catch { }

                if (_lastValues == null || _lastValues.Count == 0) return;

                int repIndex = (_lastRunResult != null && _lastRunResult.ReferenceIndex >= 0) ? _lastRunResult.ReferenceIndex : 0;
                if (listViewResults?.SelectedIndices.Count > 0) repIndex = listViewResults.SelectedIndices[0];
                if (repIndex < 0 || repIndex >= _lastValues.Count) repIndex = 0;

                using (var penRectSel = new Pen(Color.Lime, 2))
                using (var penRect = new Pen(Color.FromArgb(180, 0, 255, 0), 1))
                using (var penCrossSel = new Pen(Color.Lime, 2))
                using (var penCross = new Pen(Color.Lime, 1))
                using (var penHighlight = new Pen(Color.Lime, 2) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dash })
                using (var fontIdxBig = new Font("Arial", 14f, FontStyle.Bold))
                using (var fontIdx = new Font("Arial", 12f, FontStyle.Bold))
                using (var fontDbg = new Font("Consolas", 8f))
                using (var fontInfo = new Font("Consolas", 9f, FontStyle.Bold))
                using (var brushWhite = new SolidBrush(Color.White))
                using (var brushYellow = new SolidBrush(Color.Yellow))
                using (var brushCyan = new SolidBrush(Color.Cyan))
                {
                    int crossLenSel = 24; int crossLen = 16; bool debug = false;
                    for (int i = 0; i < _lastValues.Count; i++)
                    {
                        var v = _lastValues[i];
                        double cx = v.X;
                        double cy = v.Y;

                        double dx = offX + cx * scale;
                        double dy = offY + cy * scale;
                        bool isRep = (i == repIndex);

                        double hw = patternW / 2.0; double hh = patternH / 2.0;
                        PointF[] poly = new PointF[4];
                        poly[0] = new PointF((float)(offX + (cx - hw) * scale), (float)(offY + (cy - hh) * scale));
                        poly[1] = new PointF((float)(offX + (cx + hw) * scale), (float)(offY + (cy - hh) * scale));
                        poly[2] = new PointF((float)(offX + (cx + hw) * scale), (float)(offY + (cy + hh) * scale));
                        poly[3] = new PointF((float)(offX + (cx - hw) * scale), (float)(offY + (cy + hh) * scale));
                        e.Graphics.DrawPolygon(isRep ? penRectSel : penRect, poly);

                        int len = isRep ? crossLenSel : crossLen;
                        e.Graphics.DrawLine(isRep ? penCrossSel : penCross, (float)(dx - len), (float)dy, (float)(dx + len), (float)dy);
                        e.Graphics.DrawLine(isRep ? penCrossSel : penCross, (float)dx, (float)(dy - len), (float)dx, (float)(dy + len));

                        if (showIdx)
                        {
                            var f = isRep ? fontIdxBig : fontIdx;
                            var txt = i.ToString();
                            e.Graphics.DrawString(txt, f, brushWhite, (float)(dx + len + 4), (float)(dy - len - 4));
                        }
                        if (debug && i < 3)
                        {
                            string raw = $"Abs({v.X:0.0},{v.Y:0.0})";
                            e.Graphics.DrawString(raw, fontDbg, brushYellow, (float)(dx + 6), (float)(dy + 6));
                        }
                    }

                    if (highlight && repIndex >= 0 && repIndex < _lastValues.Count)
                    {
                        var v = _lastValues[repIndex];
                        double dx = offX + v.X * scale; double dy = offY + v.Y * scale;
                        int r = (int)((Math.Max(patternW, patternH) / 2 + 10) * scale);
                        e.Graphics.DrawEllipse(penHighlight, (float)(dx - r), (float)(dy - r), r * 2, r * 2);
                    }

                    string info = $"ROIStart=({roiStart.X},{roiStart.Y}) (coords are ABSOLUTE)";
                    e.Graphics.DrawString(info, fontInfo, brushCyan, 10, 10);
                }
            }
            catch { }
        }

        private void cmbMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (cmbMode.SelectedItem?.ToString())
            {
                case "Prealign": SetMode(PatternMatchingRunner.ProcessMode.Prealign); break;
                case "MapMatching": SetMode(PatternMatchingRunner.ProcessMode.MapMatching); break;
                case "SecondAlign": SetMode(PatternMatchingRunner.ProcessMode.SecondAlign); break;
            }
        }

        public void SetMode(PatternMatchingRunner.ProcessMode mode)
        {
            _currentMode = mode;
            // Runner 동작 모드 반영
            _runner?.SetProcessMode(mode);

            // UI 라디오 동기화 (디자이너 컨트롤 그대로 사용)
            if (mode == PatternMatchingRunner.ProcessMode.Prealign)
            {
                if (radioSingle != null) radioSingle.Checked = true;
            }
            else
            {
                if (radioMulti != null) radioMulti.Checked = true;
            }

            LoadRecipeForCurrentCamera(); // mode별 데이터 로드
            UpdateStatus($"Mode changed: {mode}");
        }
    }
}