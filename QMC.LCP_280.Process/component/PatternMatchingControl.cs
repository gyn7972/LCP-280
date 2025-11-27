using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Forms;
using QMC.Common.VisionPart;
using QMC.Common.Vision;
using QMC.Common.Vision.Tools;
using QMC.Common.Cameras;
using QMC.Common;
using System.ComponentModel;
using QMC.LCP_280.Process.Component; // for MeasurementRecipe & RecipeManager

namespace QMC.LCP_280.Process
{
    public partial class PatternMatchingControl : UserControl
    {
        private string _recipeDirectory;
        private string _currentRecipeName = "Default";

        private InternalMultiPatternMatchingVisionPart _visionPart;
        private MultiPatternMatchingParameters _parameters;

        private readonly List<Camera> _cameras = new List<Camera>();
        private readonly List<string> _cameraNames = new List<string>();

        private bool _suspendAutoLoad = false;
        private Point _lastResultPoint = Point.Empty; // 검색 결과 표시용
        private double _lastResultAngle = 0;          // 각도 저장 (미사용시 확장 대비)

        // Added: Unified runtime runner
        private PatternMatchingRunner _runner;

        // NEW: 저장된 다중 결과 목록
        private List<PatternMatchingResult.PatternMatchingResultValue> _lastValues = new List<PatternMatchingResult.PatternMatchingResultValue>();
        // NEW: 러너가 반환한 마지막 실행 결과 (대표 좌표 포함)
        private PatternMatchingRunner.PatternMatchRunResult _lastRunResult;

        // 새로 추가: 카메라 변경 시 결과/크로스라인 초기화를 묶어서 처리
        private void ResetCrossAndResults()
        {
            try
            {
                _lastResultPoint = Point.Empty;
                _lastResultAngle = 0;
                if (_lastValues != null) 
                    _lastValues.Clear();

                _lastRunResult = null;
                if (_viewer != null)
                {
                    // 내부 기본 크로스라인 버퍼 초기화
                    _viewer.InitCrossLine();
                    // VisibleCrossLine 상태 유지하면서 다시 표시
                    _viewer.ShowCrossLine(_viewer.VisibleCrossLine);
                    _viewer.Invalidate();
                }
            }
            catch { /* 무시 */ }
        }


        // 디자인 타임 가드
        private readonly bool _designMode;

        public PatternMatchingControl()
        {
            _designMode = IsActuallyInDesignMode();
            InitializeComponent();

            // 1) 오토 스케일/도킹/앵커 고정 (디자인/런타임 공통)
            ApplyFixedLayout();

            if (_designMode)
            {
                try
                {
                    this.BackColor = Color.White;
                    if (_viewer != null)
                    {
                        _viewer.BackColor = Color.Black;
                        _viewer.Image = null;
                    }
                }
                catch { }
                return;
            }

            // 라디오 버튼 직접 참조 (디자이너 partial 클래스의 필드)
            if (radioSingle != null)
            {
                radioSingle.CheckedChanged -= SearchModeRadioChanged;
                radioSingle.CheckedChanged += SearchModeRadioChanged;
                radioSingle.Checked = true; // 기본: 단일(First)
            }
            if (radioMulti != null)
            {
                radioMulti.CheckedChanged -= SearchModeRadioChanged;
                radioMulti.CheckedChanged += SearchModeRadioChanged;
            }

            _recipeDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configs", "PatternMatching");

            if (_visionPart == null)
            {
                _visionPart = new InternalMultiPatternMatchingVisionPart("PM_Dialog") { Simulated = true };
                _visionPart.Create();
            }
            _parameters = _visionPart.GetPatternMatchingParameters();

            maintROIControl.SetOwner(_visionPart);
            maintROIControl.SetImageviwer(_viewer);
            AttachEvents();

            //제거
            //if (_btnClose != null) _btnClose.Click += (s, e) => Close();

            // 중복 연결 제거: 디자이너에서 이미 연결됨
            // if (patternMatchingParamControl != null)
            // {
            //     patternMatchingParamControl.Load += patternMatchingParamControl_Load;
            //     patternMatchingParamControl.UpdateParameters(_parameters);
            // }
            patternMatchingParamControl?.UpdateParameters(_parameters);

            // 수동 Paint 이벤트 제거 (Overlay 시스템 사용)
            if (_viewer != null)
            {
                _viewer.Paint -= Viewer_PaintCross;
                _viewer.Paint -= Viewer_PaintMatches; // avoid duplicate
                //_viewer.Paint += Viewer_PaintMatches; // new unified paint
            }

            // 체크박스 이벤트 연결 (Runner 생성 전이라도 상태를 기억, Runner 생성 시 반영)
            if (chkShowIndexes != null)
            {
                chkShowIndexes.CheckedChanged += (s, e) =>
                {
                    if (_runner != null)
                        _runner.SetShowMatchIndexes(chkShowIndexes.Checked);
                    RebuildResultOverlays();
                };
            }
            if (chkHighlightRef != null)
            {
                chkHighlightRef.Checked = true; // 기본 활성화
                chkHighlightRef.CheckedChanged += (s, e) =>
                {
                    if (_runner != null)
                        _runner.SetHighlightReference(chkHighlightRef.Checked);
                    RebuildResultOverlays();
                };
            }

            // 초기 현재 MeasurementRecipe에 따라 비전 레시피 명/경로 반영
            ApplyVisionRecipeFromMeasurement();

            var eq = Equipment.Instance;
            _currentRecipeName = eq.EquipmentRecipe.CurrentRecipeName;
            LoadRecipe(_currentRecipeName);
            TryBindEquipmentCameras();
            InitializeCameraList();
            UpdateStatus("Ready");
        }

        // 모든 하위 컨트롤에 Dock=None, Anchor=Top|Left 강제, AutoScale 끔
        private void ApplyFixedLayout()
        {
            //try
            //{
            //    this.AutoScaleMode = AutoScaleMode.None;
            //    this.AutoSize = false;
            //    FreezeChildLayout(this);
            //}
            //catch { /* ignore */ }
        }

        private void FreezeChildLayout(Control root)
        {
            if (root == null) return;

            // 루트 자신(패널 포함)도 도킹 제거
            //root.Dock = DockStyle.None;

            //foreach (Control c in root.Controls)
            //{
            //    try
            //    {
            //        c.Dock = DockStyle.None;
            //        c.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            //        // 재귀 적용
            //        FreezeChildLayout(c);
            //    }
            //    catch { }
            //}
        }

        private bool IsActuallyInDesignMode()
        {
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime) 
                return true;

            try { 
                return System.Diagnostics.Process.GetCurrentProcess().ProcessName.Equals("devenv", StringComparison.OrdinalIgnoreCase); 
            }
            catch { return false; }
        }

        // 현재 열려있는 MeasurementRecipe에서 VisionRecipeName/Path를 읽어 패널 상태(_currentRecipeName 등)에 반영
        private void ApplyVisionRecipeFromMeasurement()
        {
            try
            {
                string measName = null;
                try 
                {
                    var eq = Equipment.Instance;
                    //_currentRecipeName = Equipment._CurrentRecipeName;
                    measName = eq.EquipmentRecipe.CurrentRecipeName;
                    //measName = Equipment._CurrentRecipeName; 
                } 
                catch { measName = null; }

                if (string.IsNullOrWhiteSpace(measName)) 
                { 
                    _currentRecipeName = _currentRecipeName ?? "Default"; 
                    return; 
                }

                var baseRec = RecipeManager.LoadOrCreate(typeof(MeasurementRecipe), measName) as QMC.Common.BaseRecipe;
                var mr = baseRec as MeasurementRecipe;
                if (mr == null)
                {
                    _currentRecipeName = measName; // fallback
                    return;
                }

                if (mr.UseVisionRecipe && !string.IsNullOrWhiteSpace(mr.VisionRecipeName))
                {
                    _currentRecipeName = mr.VisionRecipeName; // 비전 레시피명 우선
                }
                else
                {
                    _currentRecipeName = measName; // fallback to measurement recipe name
                }

                // 디렉토리는 런타임 Resolver에서 사용하므로 여기서 별도 설정 불필요
            }
            catch
            {
                // ignore
            }
        }

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
                int len = 15; // 반길이
                using (Pen p = new Pen(Color.Red, 2))
                {
                    e.Graphics.DrawLine(p, cx - len, cy, cx + len, cy);
                    e.Graphics.DrawLine(p, cx, cy - len, cx, cy + len);
                }
            }
            catch { }
        }

        // NEW: Direct drawing (avoid overlay coordinate issues)
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

                // NOTE: MultiPatternMatchingVisionPart.OnSearch() 이미 ROI offset을 절대좌표로 더해줌
                // 따라서 ROI가 전체보다 작아도 결과 좌표는 항상 원본 이미지 기준 절대값이다.
                // 이전 로직(ROI 범위 추론 후 재오프셋)이 중복 적용되어 작은 ROI에서 십자가가 사라지는 문제 발생 → 제거.
                Point roiStart = Point.Empty; Point roiEnd = Point.Empty; // kept for info text only
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

                // 대표 인덱스
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
                    int crossLenSel = 24; int crossLen = 16; bool debug = false; // debug off now
                    for (int i = 0; i < _lastValues.Count; i++)
                    {
                        var v = _lastValues[i];
                        double cx = v.X; // absolute
                        double cy = v.Y; // absolute

                        double dx = offX + cx * scale;
                        double dy = offY + cy * scale;
                        bool isRep = (i == repIndex);

                        // Draw simple rectangle (pattern size approximate, no rotation assumption)
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

        private void RebuildResultOverlays()
        {
            // Overlays removed – direct painting in Viewer_PaintMatches handles drawing.
            // _viewer?.Invalidate();

            if (_viewer == null) return;

            try
            {
                var overlays = _viewer.ResultOverlays;
                if (overlays == null) return;

                lock (overlays)
                {
                    overlays.Clear();

                    if (_lastValues == null || _lastValues.Count == 0)
                    {
                        _viewer.Invalidate();
                        return;
                    }

                    int patternW = 40, patternH = 40;
                    try
                    {
                        var ti = _parameters?.TrainImages?.FirstOrDefault(t => t?.Header != null && t.Header.Width > 0 && t.Header.Height > 0);
                        if (ti != null) { patternW = ti.Header.Width; patternH = ti.Header.Height; }
                    }
                    catch { }

                    bool showIdx = chkShowIndexes?.Checked ?? false;
                    bool highlight = chkHighlightRef?.Checked ?? true;

                    int repIndex = (_lastRunResult != null && _lastRunResult.ReferenceIndex >= 0)
                                    ? _lastRunResult.ReferenceIndex : 0;
                    if (listViewResults?.SelectedIndices.Count > 0)
                        repIndex = listViewResults.SelectedIndices[0];
                    if (repIndex < 0 || repIndex >= _lastValues.Count) repIndex = 0;

                    for (int i = 0; i < _lastValues.Count; i++)
                    {
                        var v = _lastValues[i];
                        var ov = new PatternMatchResultOverlay
                        {
                            Center = new PointF((float)v.X, (float)v.Y), // 절대좌표
                            PatternWidth = patternW,
                            PatternHeight = patternH,
                            AngleDeg = (float)v.R,
                            CrossHalfLenPx = (i == repIndex) ? 24 : 16,
                            Color = Color.Lime,
                            Thickness = (i == repIndex) ? 2 : 1,
                            Highlight = highlight && (i == repIndex),
                            Index = showIdx ? i : -1,
                            Visible = true
                        };
                        overlays.Add(ov);
                    }
                }

                _viewer.Invalidate();
            }
            catch { }

        }

        // Core Helpers (region removed to avoid unmatched directives)
        private void BindUiToCurrentContext(Camera cam = null)
        {
            try
            {
                // ROI 패널
                if (maintROIControl != null)
                {
                    maintROIControl.SetOwner(_visionPart);
                    maintROIControl.SetImageviwer(_viewer);
                    if (cam != null)
                        maintROIControl.UpdateImageInfo(cam.Resolution);
                    maintROIControl.EnsureDefaultRoiTools();
                }

                // 파라미터 패널
                if (_parameters == null && _visionPart != null)
                    _parameters = _visionPart.GetPatternMatchingParameters();
                patternMatchingParamControl?.UpdateParameters(_parameters);

                // 결과 패널
                patternMatchingResultControl?.Bind(_visionPart, _parameters, _viewer);
            }
            catch
            {
                // 바인딩 중 예외는 UI에만 영향. 로깅 원하면 여기서 Log.Write(...)
            }
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

        #region Camera Binding
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
                if (eq.Cameras.Count > 0) SetCameras(eq.Cameras.Values);
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
                cameraListBoxItemsView.SelectedIndex = index; // UI 표시 동기화
            ApplyCameraSelection(index); // 여기서 LoadRecipeForCurrentCamera 호출됨
        }

        public void SelectCameraByName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return;
            int idx = _cameraNames.FindIndex(n => string.Equals(n, name, StringComparison.OrdinalIgnoreCase));
            if (idx >= 0) SelectCamera(idx);
        }

        public void SetCameras(IEnumerable<Camera> cameras)
        {
            _cameras.Clear();
            _cameraNames.Clear();
            if (cameras == null) return;
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

        private void OnCameraItemSelected(object sender, int selectedIndex) => ApplyCameraSelection(selectedIndex);

        private void ApplyCameraSelection(int index)
        {
            try
            {
                if (index < 0 || index >= _cameras.Count) 
                    return;

                // 이전 뷰/결과 완전 정리
                ClearViewer();

                try { _viewer.Camera?.StopLive(); } catch { }

                var cam = _cameras[index];
                _viewer.Camera = cam;

                ResetViewerForCameraChange(cam);

                // 카메라 변경 직후 크로스라인 및 이전 검색결과 완전 제거
                ResetCrossAndResults();

                try { cam.StartLive(); } catch { }
                _viewer.StartUpdateTask();

                SyncImageInfoToControls(cam);

                _suspendAutoLoad = true;
                try { LoadRecipeForCurrentCamera(); } finally { _suspendAutoLoad = false; }

                // 아래 한 줄 추가: 현재 컨텍스트로 모든 UI 바인딩
                BindUiToCurrentContext(cam);

                try
                {
                    _runner?.Dispose();
                    var opt = new PatternMatchingRunner.RunnerOptions
                    {
                        AutoLoadRecipe = false,
                        RecipeRootDirectory = _recipeDirectory,
                        RecipeName = _currentRecipeName,
                        DrawCrossOnViewer = false,        // 내부 러너 그리기 비활성 (UI에서 직접 그림)
                        CrossColor = Color.Lime,
                        CrossHalfLength = 15,
                        EnableSaveImage = false
                    };
                    _runner = new PatternMatchingRunner(cam, _viewer, opt);
                    UpdateRunnerModeFromUI(); // 모드 적용
                    ApplyOverlayOptionCheckboxes();     // 체크박스 상태 적용
                    _runner.AfterSearch += r =>
                    {
                        if (IsHandleCreated)
                        {
                            try
                            {
                                BeginInvoke(new Action(() =>
                                {
                                    UpdateStatus(r.Success ? "Search Success." : ("Search Fail: " + r.FailReason));
                                    // 검색 후 뷰 갱신
                                    _viewer?.Invalidate();
                                    RebuildResultOverlays();
                                }));
                            }
                            catch { }
                        }
                    };
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

        private void UpdateRunnerModeFromUI()
        {
            if (_runner == null) return;
            if (radioSingle != null && radioSingle.Checked)
                _runner.SetSearchMode(PatternMatchingRunner.SearchMode.First);
            else if (radioMulti != null && radioMulti.Checked)
                _runner.SetSearchMode(PatternMatchingRunner.SearchMode.All);
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

                //try { _lastResultPoint = Point.Empty; _lastResultAngle = 0; } catch { }
                //try { _viewer.InitCrossLine(); } catch { }
                //_viewer.Invalidate();

                // 결과 및 크로스라인(검색 십자) 초기화
                _lastResultPoint = Point.Empty;
                _lastResultAngle = 0;
                if (_lastValues != null) _lastValues.Clear();
                _lastRunResult = null;

                // 뷰어 자체 중앙 크로스라인 버퍼 재설정
                _viewer.InitCrossLine();
                _viewer.ShowCrossLine(true); // 새 뷰에서도 항상 보여주려면 true 강제 (원래 상태로 두려면 _viewer.VisibleCrossLine 사용)

                _viewer.Invalidate();

                
            }
            catch { }
        }
        #endregion

        #region Recipe Save / Load
        // MeasurementRecipe 기반 VisionRecipe 경로/파일명을 우선적으로 결정한다.
        private string ResolveVisionRecipePath(string cameraName, string fallbackRecipeName)
        {
            try
            {
                string measName = null;
                try
                {
                    var eq = Equipment.Instance;
                    measName = eq.EquipmentRecipe.CurrentRecipeName;
                }
                catch { measName = null; }

                if (string.IsNullOrWhiteSpace(measName))
                    return null;

                var br = RecipeManager.LoadOrCreate(typeof(MeasurementRecipe), measName) as QMC.Common.BaseRecipe;
                var mr = br as MeasurementRecipe;
                if (mr == null || !mr.UseVisionRecipe)
                    return null;

                string vName = string.IsNullOrWhiteSpace(mr.VisionRecipeName) ? fallbackRecipeName : mr.VisionRecipeName;
                string vPath = mr.VisionRecipePath;

                if (!string.IsNullOrWhiteSpace(vPath))
                {
                    // 1) 파일 경로가 명시된 경우: 현재 설계상 '공용 파일' 사용 유지
                    //    (카메라별 분리를 강제하려면, 파일명에 카메라명을 suffix로 붙이도록 추가 로직을 도입해야 함)
                    if (File.Exists(vPath))
                    {
                        return vPath; // 공용 파일
                    }

                    // 2) 디렉터리로 간주(존재하지 않아도 됨): 항상 카메라별 하위 폴더 경로 반환
                    if (!string.IsNullOrWhiteSpace(vName))
                    {
                        string pCamera = Path.Combine(vPath, cameraName ?? "NoCamera", vName + ".Vision.json");
                        return pCamera;
                    }
                }

                if (!string.IsNullOrWhiteSpace(vName))
                {
                    string camFolder = Path.Combine(_recipeDirectory, cameraName ?? "NoCamera");
                    return Path.Combine(camFolder, vName + ".Vision.json");
                }
            }
            catch { }
            return null;

            //try
            //{
            //    string measName = null;
            //    try 
            //    {
            //        var eq = Equipment.Instance;
            //        //_currentRecipeName = Equipment._CurrentRecipeName;
            //        measName = eq.EquipmentRecipe.CurrentRecipeName;
            //        //measName = Equipment._CurrentRecipeName; 
            //    } 
            //    catch { measName = null; }

            //    if (string.IsNullOrWhiteSpace(measName)) 
            //        return null;

            //    var br = RecipeManager.LoadOrCreate(typeof(MeasurementRecipe), measName) as QMC.Common.BaseRecipe;
            //    var mr = br as MeasurementRecipe;
            //    if (mr == null || !mr.UseVisionRecipe) 
            //        return null;

            //    string vName = string.IsNullOrWhiteSpace(mr.VisionRecipeName) ? fallbackRecipeName : mr.VisionRecipeName;
            //    string vPath = mr.VisionRecipePath;

            //    if (!string.IsNullOrWhiteSpace(vPath))
            //    {
            //        if (File.Exists(vPath))
            //        {
            //            // 명시적 파일
            //            return vPath;
            //        }
            //        if (Directory.Exists(vPath))
            //        {
            //            // dir/<camera>/<name>.pmrecipe.json
            //            if (!string.IsNullOrWhiteSpace(vName))
            //            {
            //                string p1 = Path.Combine(vPath, cameraName ?? "NoCamera", vName + ".Vision.json");
            //                if (File.Exists(p1) || Directory.Exists(Path.GetDirectoryName(p1))) 
            //                    return p1; // 존재 안해도 저장시 사용

            //                string p2 = Path.Combine(vPath, vName + ".Vision.json");
            //                if (File.Exists(p2) || Directory.Exists(vPath)) 
            //                    return p2;
            //            }
            //        }
            //    }

            //    if (!string.IsNullOrWhiteSpace(vName))
            //    {
            //        string camFolder = Path.Combine(_recipeDirectory, cameraName ?? "NoCamera");
            //        return Path.Combine(camFolder, vName + ".Vision.json");
            //    }
            //}
            //catch { }
            //return null;
        }

        private string GetRecipePath(string cameraName, string recipeName)
        {
            // 우선 MeasurementRecipe 설정 사용
            string resolved = ResolveVisionRecipePath(cameraName, recipeName);
            if (!string.IsNullOrEmpty(resolved))
            {
                try { Directory.CreateDirectory(Path.GetDirectoryName(resolved)); } catch { }
                return resolved;
            }

            // fallback: 기존 구조
            string dir = Path.Combine(_recipeDirectory, cameraName ?? "NoCamera");
            Directory.CreateDirectory(dir);
            return Path.Combine(dir, recipeName + ".Vision.json");
        }
        private string GetLegacyRecipePath(string recipeName)
        {
            Directory.CreateDirectory(_recipeDirectory);
            return Path.Combine(_recipeDirectory, recipeName + ".Vision.json");
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
            //SaveRecipe(_currentRecipeName, GetCurrentCameraName());
        }
        private void LoadRecipeForCurrentCamera()
        {
            LoadRecipe(_currentRecipeName, GetCurrentCameraName());
        }
        private void SaveRecipe(string recipeName, string cameraName = null)
        {
            try
            {
                // 현재 UI ROI 정보를 VisionPart로 먼저 반영
                maintROIControl?.CommitCurrentRoi();
                SyncParametersFromUI();
                if (string.IsNullOrEmpty(cameraName)) cameraName = GetCurrentCameraName();
                var roi = new PatternMatchingRoiJson();
                try
                {
                    roi.TrainStart = _visionPart.GetTrainStartPoint();
                    roi.TrainEnd = _visionPart.GetTrainEndPoint();
                    roi.InspectStart = _visionPart.GetInspectStartPoint();
                    roi.InspectEnd = _visionPart.GetInspectEndPoint();
                }
                catch { }
                if (_parameters == null) _parameters = _visionPart.GetPatternMatchingParameters();
                var container = new PatternMatchingRecipeJson
                {
                    Parameters = _parameters?.Clone(),
                    Roi = roi,
                    LastCameraName = cameraName
                };
                var path = GetRecipePath(cameraName, recipeName);
                PatternMatchingRecipeStore.Save(path, container);
                UpdateStatus($"Recipe saved: {path}");
            }
            catch (Exception ex)
            {
                var mb = new MessageBoxOk();
                mb.ShowDialog("Error!", "Recipe 저장 실패: " + ex.Message);
            }
        }

        private void LoadRecipe(string recipeName, string cameraName = null)
        {
            try
            {
                if (string.IsNullOrEmpty(cameraName))
                {
                    cameraName = GetCurrentCameraName();
                }
                    

                var path = GetRecipePath(cameraName, recipeName);
                var container = PatternMatchingRecipeStore.Load(path);
                if (container == null && cameraName != "NoCamera")
                {
                    var legacy = PatternMatchingRecipeStore.Load(GetLegacyRecipePath(recipeName));
                    if (legacy != null)
                    {
                        container = legacy;
                        PatternMatchingRecipeStore.Save(path, container); // migrate
                    }
                }
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
                    try
                    {
                        _visionPart.SetTrainStartPoint(container.Roi.TrainStart);
                        _visionPart.SetTrainEndPoint(container.Roi.TrainEnd);
                        _visionPart.SetInspectStartPoint(container.Roi.InspectStart);
                        _visionPart.SetInspectEndPoint(container.Roi.InspectEnd);
                        // ROI를 UI에 다시 반영
                        maintROIControl?.ReloadRoiFromPart();
                    }
                    catch { }
                }
                maintROIControl.SetOwner(_visionPart);
                maintROIControl.EnsureDefaultRoiTools();
                AttachEvents();

                // 결과 패널 바인딩 추가 (레시피를 외부에서 로드했을 때도 동기화)
                patternMatchingResultControl?.Bind(_visionPart, _parameters, _viewer);


                UpdateStatus($"Recipe loaded: {path}");
            }
            catch (Exception ex)
            {
                var mb = new MessageBoxOk();
                mb.ShowDialog("Error!", "Recipe 로드 실패: " + ex.Message);
            }
        }
        #endregion

        #region Search
        private VisionImage AcquireCurrentSearchImage()
        {
            VisionImage src = null;
            try
            {
                var cam = _viewer?.Camera;
                if (cam != null && cam.Opened && cam.LatestImage?.RawData != null)
                {
                    src = cam.LatestImage;
                }

                if (src == null && cam != null && cam.Opened)
                {
                    try 
                    { 
                        cam.GrabSync(out src); 
                    } 
                    catch (Exception ex)
                    {
                        Log.Write(ex);
                    }
                }

                if (src == null && _viewer?.InputImage?.RawData != null)
                {
                    src = _viewer?.InputImage;
                }
                if (src == null && _visionPart.TestImage?.RawData != null)
                {
                    src = _visionPart.TestImage;
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
            return src;
        }

        private void BtnSearch_Click(object sender, EventArgs e)
        {
            try
            {
                SyncParametersFromUI();

                if (_parameters == null)
                {
                    _parameters = _visionPart.GetPatternMatchingParameters();
                }
                   
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

                if (testImage == null 
                    || testImage.GetImage() == null)
                {
                    var mb = new MessageBoxOk();
                    mb.ShowDialog("Notification!", "검색할 이미지(카메라 또는 로드된 이미지)가 없습니다.");
                    return;
                }

                maintROIControl?.CommitCurrentRoi();
                SaveRecipeForCurrentCamera();

                // 러너 생성은 ApplyCameraSelection에서 수행됨. 여기서는 존재 시 그대로 사용.
                if (_runner == null)
                {
                    var cam = _viewer?.Camera;
                    if (cam == null)
                    {
                        var mb = new MessageBoxOk();
                        mb.ShowDialog("Error!", "Runner 초기화 실패 (카메라 없음)");
                        return;
                    }
                    var opt = new PatternMatchingRunner.RunnerOptions
                    {
                        AutoLoadRecipe = false,
                        RecipeRootDirectory = _recipeDirectory,
                        RecipeName = _currentRecipeName,
                        DrawCrossOnViewer = false,
                        CrossColor = Color.Lime,
                        CrossHalfLength = 15,
                        EnableSaveImage = false
                    };
                    _runner = new PatternMatchingRunner(cam, _viewer, opt);
                    UpdateRunnerModeFromUI();
                    ApplyOverlayOptionCheckboxes();
                }

                _runner.LoadRecipe();
                UpdateRunnerModeFromUI();

                var res = _runner.Search(testImage, save: false);
                _lastRunResult = res;
                if (!res.Success || res.RawResult == null)
                {
                    UpdateStatus("Search Fail: " + res.FailReason);

                    var mb = new MessageBoxOk();
                    mb.ShowDialog("Notification!", $"Search 실패: {res.FailReason}");

                    listViewResults.Items.Clear();
                    _lastValues.Clear();
                    _viewer?.Invalidate();
                    return;
                }
                var raw = res.RawResult;
                _lastValues = raw.Values != null ? new List<PatternMatchingResult.PatternMatchingResultValue>(raw.Values) : new List<PatternMatchingResult.PatternMatchingResultValue>();
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
                    txtResultX.Clear(); txtResultY.Clear(); txtResultT.Clear();
                    _lastResultPoint = Point.Empty; _lastResultAngle = 0;
                }
                _viewer?.Invalidate();
                UpdateStatus(res.Success ? "Search Success." : "Search Fail.");
                var txtAvgXCtrl = this.txtAvgX;
                var txtAvgYCtrl = this.txtAvgY;
                var txtAvgTCtrl = this.txtAvgT;
                if (txtAvgXCtrl != null && txtAvgYCtrl != null && txtAvgTCtrl != null)
                {
                    bool isAll = radioMulti != null && radioMulti.Checked;
                    if (isAll && res.AvgXExcludingExtremes.HasValue)
                    {
                        txtAvgXCtrl.Text = res.AvgXExcludingExtremes.Value.ToString("0.000");
                        txtAvgYCtrl.Text = res.AvgYExcludingExtremes.Value.ToString("0.000");
                        txtAvgTCtrl.Text = res.AvgRExcludingExtremes.Value.ToString("0.000");
                    }
                    else
                    {
                        txtAvgXCtrl.Clear(); txtAvgYCtrl.Clear(); txtAvgTCtrl.Clear();
                    }
                }
                RebuildResultOverlays();
            }
            catch (Exception ex)
            {
                var mb = new MessageBoxOk();
                mb.ShowDialog("Error!", "Search 예외: " + ex.Message);

                UpdateStatus("Search exception");
            }
        }

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
                    }) { Tag = (PatternMatchingResult.PatternMatchingResultValue?)v };
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
            RebuildResultOverlays();
            _viewer?.Invalidate();
        }
        #endregion

        #region Events / ROI
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

        #region Internal VisionPart Wrapper
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

        #region Static API
        public static void ShowDialogModal(IWin32Window owner = null)
        {
            
        }
        #endregion

        private void listViewResults_SelectedIndexChanged(object sender, EventArgs e)
        {
            OnResultListSelectionChanged();
        }

        private void SearchModeRadioChanged(object sender, EventArgs e)
        {
            UpdateRunnerModeFromUI();
            // 모드 변경 시 평균 필드 초기화
            if (radioSingle != null && radioSingle.Checked)
            {
                txtAvgX.Clear(); txtAvgY.Clear(); txtAvgT.Clear();
            }
            RebuildResultOverlays();
        }

        private void ApplyOverlayOptionCheckboxes()
        {
            if (_runner == null) return;
            if (chkShowIndexes != null) _runner.SetShowMatchIndexes(chkShowIndexes.Checked);
            if (chkHighlightRef != null) _runner.SetHighlightReference(chkHighlightRef.Checked);
        }

        private void Close()
        {
            try
            {
                var form = this.FindForm();
                if (form != null)
                    form.Close();   // 이 컨트롤을 담고 있는 Form 닫기
                else
                    this.Dispose(); // 호스트 Form이 없으면 컨트롤만 정리
            }
            catch
            {
                // 무시
            }
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
                    _runner?.LoadRecipe(); // 러너 교체 대신 로드만
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
            //try
            //{
            //    maintROIControl?.CommitCurrentRoi();
            //    var ask = new MessageBoxYesNo();
            //    if (ask.ShowDialog("저장 확인", "현재 설정을 저장하시겠습니까?") == DialogResult.Yes)
            //    {
            //        SaveRecipeForCurrentCamera();
            //        _runner = VisionRunnerHub.GetOrCreate(_viewer.Camera.Name);
            //        _runner.LoadRecipe();
            //    }
            //    else
            //    {
            //        UpdateStatus("Save canceled");
            //    }
            //}
            //catch (Exception ex)
            //{
            //    var mb = new MessageBoxOk();
            //    mb.ShowDialog("Error!", $"저장 중 오류: " + ex.Message);
            //}
        }

        private void txtResultT_TextChanged(object sender, EventArgs e)
        {

        }

        private void tableLayoutPanel8_Paint(object sender, PaintEventArgs e)
        {

        }

        private void lblRX_Click(object sender, EventArgs e)
        {

        }

        private void _btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            try
            {
                if (!DesignMode)
                {
                    if (_viewer != null)
                    {
                        _viewer.Paint -= Viewer_PaintMatches;
                        try { _viewer.Camera?.StopLive(); } catch { }
                    }
                    if (maintROIControl != null)
                    {
                        maintROIControl.TrainImageCaptured -= MaintROIControl_TrainImageCaptured;
                    }
                    _runner?.Dispose();
                    _runner = null;
                }
            }
            catch { }
            base.OnHandleDestroyed(e);
        }
    }
}