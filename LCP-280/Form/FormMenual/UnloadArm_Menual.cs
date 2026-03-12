using Newtonsoft.Json.Linq;
using QMC.Common;
using QMC.Common.Component;
using QMC.Common.UI;
using QMC.LCP_280.Process.Component;
using QMC.LCP_280.Process.Unit.FormSetup;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using static QMC.Common.FormMenual;
using MessageBox = System.Windows.Forms.MessageBox;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;
using Timer = System.Windows.Forms.Timer;

namespace QMC.LCP_280.Process.Unit.FormWork
{
    [FormOrder(5)]
    public partial class UnloadArm_Menual : Form, ITabActivationAware
    {
        private Equipment Equipment => Equipment.Instance;

        private Rotary Rotary { get; set; }
        private IndexUnloadAligner IndexUnloadAligner { get; set; }
        private OutputDieTransfer OutputDieTransfer { get; set; }
        private OutputStage OutputStage { get; set; }
        
        private bool _initialized;          // Text/핸들 설정 여부
        private bool _preloadRequested;     // Preload 1회 보장
        private bool _deferredInitDone;     // 무거운 바인딩 지연 수행 여부
        private bool _isLayoutEditMode;

        private volatile bool _pendingTeachingReload;

        public UnloadArm_Menual() : this(
            TryGetUnit<Rotary>("Rotary"),
            TryGetUnit<IndexUnloadAligner>("IndexUnloadAligner"),
            TryGetUnit<OutputDieTransfer>("OutputDieTransfer"), 
            TryGetUnit<OutputStage>("OutputStage"))
        {
        }

        public UnloadArm_Menual(Rotary rotaty,
                                IndexUnloadAligner indexUnloadAligner,
                                OutputDieTransfer outputDieTransfer,
                                OutputStage outputStage)
        {
            InitializeComponent();
            Rotary = rotaty;
            IndexUnloadAligner = indexUnloadAligner;
            OutputDieTransfer = outputDieTransfer;
            OutputStage = outputStage;

            Load += ChipUnloading_Working_Load;
            FormClosing += ChipUnloading_Working_FormClosing;

            _ChipUnloadingCameraviewer.LightControlRequested += LightControlRequested;

            SubscribeRecipeChanged();
            // ★ [ADD] UI가 다시 보이거나 활성화될 때 pending teaching reload 처리
            this.VisibleChanged += (s, e) =>
            {
                if (!Visible)
                    return;

                ReloadTeachingFromRecipeAndRefreshUi();
            };
            this.Activated += (s, e) =>
            {
                ReloadTeachingFromRecipeAndRefreshUi();
            };
        }

        // [추가] 탭 활성화 인터페이스 구현
        public void OnActivatedInTab()
        {
            ResumeCamera();
        }

        public void OnDeactivatedInTab()
        {
            PauseCamera();
        }

        // [추가] 폼 이벤트 오버라이드
        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            try
            {
                if (IsDisposed || Disposing) return;
                if (this.Visible) ResumeCamera();
                else PauseCamera();
            }
            catch { }
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            try { ResumeCamera(); } catch { }
        }

        protected override void OnDeactivate(EventArgs e)
        {
            base.OnDeactivate(e);
            try { PauseCamera(); } catch { }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            try
            {
                PauseCamera();
                // 필요한 경우 Unsubscribe 추가
            }
            catch { }
            base.OnFormClosing(e);
        }

        // [추가] 카메라 제어 헬퍼 (_ChipUnloadingCameraviewer 사용)
        private void PauseCamera()
        {
            if (_ChipUnloadingCameraviewer == null || _ChipUnloadingCameraviewer.IsDisposed) return;

            try { _ChipUnloadingCameraviewer.SuspendDisplay(); } catch { }

            var cam = _ChipUnloadingCameraviewer.Camera;
            if (cam != null)
            {
                try
                {
                    cam.SuspendedImageDisplay = true;
                    cam.StopLive();
                }
                catch { }
            }

            try
            {
                var method = _ChipUnloadingCameraviewer.GetType().GetMethod("StopUpdateTask");
                if (method != null) method.Invoke(_ChipUnloadingCameraviewer, null);
            }
            catch { }
        }

        private void ResumeCamera()
        {
            if (_ChipUnloadingCameraviewer == null || _ChipUnloadingCameraviewer.IsDisposed) return;

            var cam = _ChipUnloadingCameraviewer.Camera;
            if (cam != null)
            {
                try { cam.SuspendedImageDisplay = false; } catch { }
                try { cam.StartLive(); } catch { }
            }

            try { _ChipUnloadingCameraviewer.ResumeDisplay(); } catch { }

            try
            {
                var method = _ChipUnloadingCameraviewer.GetType().GetMethod("StartUpdateTask");
                if (method != null) method.Invoke(_ChipUnloadingCameraviewer, null);
                else _ChipUnloadingCameraviewer.StartUpdateTask();
            }
            catch { }
        }


        public void PreloadUI()
        {
            if (IsDisposed || Disposing) return;
            if (_preloadRequested) return; // 1회만
            _preloadRequested = true;
            EnsureInitialized();
            var handle = Handle; // 강제 Handle 생성
        }

        private void ChipUnloading_Working_Load(object sender, EventArgs e)
        {
            EnsureInitialized();
        }

        private void EnsureInitialized()
        {
            if (_initialized) return;
            _initialized = true;
            try
            {
                Text = $"Unload Arm";
                BeginInvoke(new Action(StartDeferredInit)); // 무거운 초기화 지연
            }
            catch (Exception ex)
            {
                try
                {
                    Controls.Add(new Label
                    {
                        Dock = DockStyle.Fill,
                        Text = $"Init 실패: {ex.Message}",
                        ForeColor = System.Drawing.Color.Red,
                        TextAlign = System.Drawing.ContentAlignment.MiddleCenter
                    });
                }
                catch { }
            }
        }

        private async void StartDeferredInit()
        {
            if (_deferredInitDone) 
                return;

            _deferredInitDone = true;
            await Task.Delay(30); // 첫 Paint 후 실행
            if (IsDisposed || Disposing) 
                return;

            try
            {
                InitSequences();

                // ★ [ADD] 최초 진입 시 TeachingRecipe 강제 로드/기본값 보강
                try
                {
                    if (OutputDieTransfer != null)
                    {
                        // Config + TeachingRecipe 로드 및 축 바인딩
                        OutputDieTransfer.Config.LoadAndBindAxes(Equipment.Instance.AxisManager);

                        // TeachingPositions 비어있으면 기본 생성(파일 저장까지)
                        OutputDieTransfer.Config.InitializeDefaultTeachingPositions();

                        // 캐시 갱신(혹시 이전 캐시가 꼬였을 경우)
                        OutputDieTransfer.Config.InvalidateTeachingRecipeCache();
                    }
                }
                catch (Exception ex)
                {
                    Log.Write(ex);
                }

                BindTeachingPositions();
                BindDioControls();
                BindCamera();
                SetupStatusTimer();
            }
            catch { }
        }

        private Timer _statusTimer;
        private int _statusTimerBusy; // 재진입 방지 플래그
        private int _lastUnloadIndexNo = -1;

        private void SetupStatusTimer()
        {
            try
            {
                if (_statusTimer != null) return;
                _statusTimer = new Timer();
                _statusTimer.Interval = 500; // ms 단위 (필요 시 조정)
                _statusTimer.Tick += StatusTimer_Tick;
                _statusTimer.Start();
            }
            catch { }
        }

        private void StatusTimer_Tick(object sender, EventArgs e)
        {
            if (IsDisposed || Disposing)
                return;

            if (Interlocked.Exchange(ref _statusTimerBusy, 1) == 1)
                return; // 재진입 방지

            try
            {
                // ===== 0-based index 추출 (변경 감지용) =====
                int UnloadIndex0 = -1;

                try { if (OutputDieTransfer != null) UnloadIndex0 = OutputDieTransfer.GetUnloaderIndexNo(); } catch { }

                if (_statusTimer != null)
                {
                    //dioControl?.RefreshInputs();
                }

                // ===== 인덱스 변경 감지 → TeachingPosition alias 갱신 =====
                // alias 표시의 핵심은 probeIndex0 이므로 최소 그 변화는 감지 필요.
                bool needTeachingRefresh =
                    (UnloadIndex0 >= 0 && UnloadIndex0 != _lastUnloadIndexNo);


                if (needTeachingRefresh)
                {
                    _lastUnloadIndexNo = UnloadIndex0;
                    if (Visible && IsHandleCreated)
                    {
                        try { BindTeachingPositions(); } catch { }
                    }
                    else
                    {
                        _pendingTeachingReload = true;
                    }
                }

            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
            finally
            {
                Interlocked.Exchange(ref _statusTimerBusy, 0);
            }
        }

        private static T TryGetUnit<T>(string unitName) where T : class
        {
            try
            {
                var eq = Equipment.Instance;
                if (eq?.Units != null && eq.Units.TryGetValue(unitName, out var u))
                    return u as T;
            }
            catch { }
            return null;
        }

        #region Teaching Position Control
        private void BindTeachingPositions()
        {
            try
            {
                if (teachingPositionControl == null) return;

                teachingPositionControl.ClearUnits();

                if (Rotary != null)
                {
                    teachingPositionControl.RegisterUnit(
                        "Rotary",
                        Rotary,
                        () => Rotary.Config?.TeachingPositions,
                        (name, vel) => Rotary.MoveToTeachingPosition(name, vel: vel),
                        tp => Rotary.Config?.SetTeachingPosition(tp),
                        autoReload: false);
                }

                if (OutputStage != null)
                {
                    teachingPositionControl.RegisterUnit(
                        "OutputStage",
                        OutputStage,
                        () => OutputStage.Config?.TeachingPositions,
                        (name, vel) => OutputStage.MoveToTeachingPosition(name, false),
                        tp => OutputStage.Config?.SetTeachingPosition(tp),
                        autoReload: false);
                }

                if (OutputDieTransfer != null)
                {
                    const string TP_PICKUP_INDEX_ALIAS = "Pickup_Index";

                    teachingPositionControl.RegisterUnit(
                        "OutputDieTransfer",
                        OutputDieTransfer,

                        // 1) Provider: Pickup_Index1~8 숨기고, alias 1개만 추가(현재 Unload index 값 표시)
                        () =>
                        {
                            var recipe = OutputDieTransfer.Config?.TeachingRecipe;
                            var list = recipe?.TeachingPositions;
                            if (list == null)
                                return null;

                            var hidden = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                            for (int i = 1; i <= 8; i++)
                                hidden.Add($"Pickup_Index{i}");

                            var result = new List<TeachingPosition>();

                            foreach (var tp in list)
                            {
                                if (tp == null || string.IsNullOrWhiteSpace(tp.Name))
                                    continue;

                                if (hidden.Contains(tp.Name))
                                    continue;

                                result.Add(tp);
                            }

                            TeachingPosition CloneForDisplay(string aliasName, TeachingPosition src, IReadOnlyList<string> axisNames, string desc)
                            {
                                var axisPositions = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
                                foreach (var a in axisNames)
                                {
                                    double v = 0.0;
                                    if (src != null && src.AxisPositions != null)
                                        src.AxisPositions.TryGetValue(a, out v);
                                    axisPositions[a] = v;
                                }
                                return new TeachingPosition(aliasName, axisPositions, desc);
                            }

                            TeachingPosition BuildAlias(string aliasName, IReadOnlyList<string> axisNames, string desc)
                            {
                                TeachingPosition src = null;

                                try
                                {
                                    // OutputDieTransfer Unload Index 기준으로 Pickup_Index{1~8} 선택
                                    int idx0 = 0;
                                    try { idx0 = OutputDieTransfer.GetUnloaderIndexNo(); } catch { idx0 = 0; } // 0..7
                                    int idx1 = idx0 + 1;

                                    string realName = $"Pickup_Index{idx1}";

                                    src = list.FirstOrDefault(t => t != null &&
                                        string.Equals(t.Name, realName, StringComparison.OrdinalIgnoreCase));
                                }
                                catch { /* ignore */ }

                                return CloneForDisplay(aliasName, src, axisNames, desc);
                            }

                            // alias 1개 추가 (현재 index의 실제 값 표시)
                            result.Add(BuildAlias(
                                TP_PICKUP_INDEX_ALIAS,
                                new[] { AxisNames.RightToolT, AxisNames.RightPickZ },
                                "Alias: Pickup_Index (current unload index)"));

                            return result;
                        },

                        // 2) MoveAction: alias면 현재 index에 맞는 실제 티칭명으로 변환 후 이동
                        (name, vel) =>
                        {
                            if (string.IsNullOrWhiteSpace(name))
                                return;

                            string realName = name;

                            try
                            {
                                if (string.Equals(name, TP_PICKUP_INDEX_ALIAS, StringComparison.OrdinalIgnoreCase))
                                {
                                    int idx0 = 0;
                                    try { idx0 = OutputDieTransfer.GetUnloaderIndexNo(); } catch { idx0 = 0; } // 0..7
                                    int idx1 = idx0 + 1;

                                    realName = $"Pickup_Index{idx1}";
                                }
                            }
                            catch
                            {
                                realName = name; // fallback
                            }

                            OutputDieTransfer.MoveToTeachingPosition(realName, false);
                        },

                        // 3) SaveAction: alias로 저장 눌러도 "현재 index의 실제 Pickup_Index{n}"으로 저장되게 변환
                        tp =>
                        {
                            try
                            {
                                if (tp == null) return;

                                var recipe = OutputDieTransfer.Config?.TeachingRecipe;
                                if (recipe == null) return;

                                if (string.Equals(tp.Name, TP_PICKUP_INDEX_ALIAS, StringComparison.OrdinalIgnoreCase))
                                {
                                    int idx0 = 0;
                                    try { idx0 = OutputDieTransfer.GetUnloaderIndexNo(); } catch { idx0 = 0; } // 0..7
                                    int idx1 = idx0 + 1;

                                    // UI에서 전달된 tp 인스턴스 이름을 실제 항목명으로 치환 후 저장
                                    tp.Name = $"Pickup_Index{idx1}";
                                }

                                recipe.UpsertFiltered(tp, save: true);
                            }
                            catch (Exception ex)
                            {
                                Log.Write(ex);
                            }
                        },

                        autoReload: false);
                }
                //if (OutputDieTransfer != null)
                //{
                //    teachingPositionControl.RegisterUnit(
                //        "OutputDieTransfer",
                //        OutputDieTransfer,
                //        () => OutputDieTransfer.Config?.TeachingPositions,
                //        (name, vel) => OutputDieTransfer.MoveToTeachingPosition(name, vel: vel),
                //        tp => OutputDieTransfer.Config?.SetTeachingPosition(tp),
                //        autoReload: false);
                //}


                teachingPositionControl.SetUnitAbbreviation("OutputStage", "Stage", reload: false);
                teachingPositionControl.SetUnitAbbreviation("OutputDieTransfer", "Arm", reload: false);

                teachingPositionControl.SetSaveCancelVisible(false, false);
                teachingPositionControl.RefreshData();
            }
            catch { }
        }

        private void ReloadTeachingFromRecipeAndRefreshUi()
        {
            try
            {
                if (OutputDieTransfer == null)
                    return;

                // 최신 Unit 참조 보장(폼이 오래 떠있을 때 Unit 교체 가능성 방어)
                try { OutputDieTransfer = TryGetUnit<OutputDieTransfer>("OutputDieTransfer"); } catch { }

                // 캐시 무효화 → 최신 recipe object 확보
                try { OutputDieTransfer?.Config?.InvalidateTeachingRecipeCache(); } catch { }

                var recipe = OutputDieTransfer?.Config?.TeachingRecipe;
                if (recipe != null)
                {
                    // 파일에서 다시 로드 + 축 바인딩
                    recipe.LoadAndBindAxes(Equipment.Instance.AxisManager);
                }

                // UI 재바인딩
                try { BindTeachingPositions(); } catch { }
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
        }

        #endregion

        #region DIO Binding
        private void BindDioControls()
        {
            try
            {
                if (dioControl == null) return;

                dioControl.IoSortMode = QMC.LCP_280.Process.Component.DIOControl.SortingMode.Insertion;

                // 그룹 구분선: OutputStage
                dioControl.BindDIOInput(() => false, "---- OutputStage ----", "Stage");
                StrongBindOutputStage();

                // 그룹 구분선: OutputDieTransfer
                if (OutputDieTransfer != null)
                {
                    dioControl.BindDIOInput(() => false, "---- OutputDieTransfer ----", "Arm");

                    for (int arm = 0; arm < 1; arm++) // 필요 시 4로 변경: arm < 4
                    {
                        int idx = arm;
                        string labelBase = $"Arm{idx + 1}";

                        // Flow 센서(입력) 표시
                        dioControl.BindDIOInput(
                            () => OutputDieTransfer.IsVacuumOK(idx),
                            $"{labelBase} Flow OK(Sns)",
                            $"Arm{idx + 1}_FlowOk");

                        // VAC: 소프트 래치 토글 사용 (isOnState: null)
                        dioControl.BindVacuum(
                            label: $"{labelBase} VAC",
                            on: () => OutputDieTransfer.SetVacuum(idx, true),
                            off: () => OutputDieTransfer.SetVacuum(idx, false),
                            isOk: null,
                            isOnState: null,
                            displayKey: $"Arm{idx + 1}_Vac",
                            showOkSensor: false
                        );

                        // BLOW
                        dioControl.BindVacuum(
                            label: $"{labelBase} Blow",
                            on: () => OutputDieTransfer.SetBlow(idx, true),
                            off: () => OutputDieTransfer.SetBlow(idx, false),
                            isOk: null,
                            isOnState: null,
                            displayKey: $"Arm{idx + 1}_Blow",
                            showOkSensor: false
                        );

                        // VENT
                        dioControl.BindVacuum(
                            label: $"{labelBase} Vent",
                            on: () => OutputDieTransfer.SetVent(idx, true),
                            off: () => OutputDieTransfer.SetVent(idx, false),
                            isOk: null,
                            isOnState: null,
                            displayKey: $"Arm{idx + 1}_Vent",
                            showOkSensor: false
                        );
                    }
                }

                dioControl.RebuildLists();
            }
            catch { }
        }

        private void StrongBindOutputStage()
        {
            if (OutputStage == null || dioControl == null) return;
            try
            {
                // ===== Sensors =====
                dioControl.BindDIOInput(() => OutputStage.IsVacuumOn(), "Vacuum OK(Sns)", "StageVacOk");
                dioControl.BindDIOInput(() => OutputStage.IsPlateUp(), "Plate UP Sns", "StagePlateUp");
                dioControl.BindDIOInput(() => OutputStage.IsPlateDown(), "Plate DOWN Sns", "StagePlateDn");
                dioControl.BindDIOInput(() => OutputStage.IsClampLiftDown(), "ClampLift DOWN Sns", "StageLiftDn");
                dioControl.BindDIOInput(() => OutputStage.IsClampFwd(), "Clamp FWD Sns", "StageClampFwd");
                dioControl.BindDIOInput(() => OutputStage.Ring0(), "Ring Sns 0", "StageRing0");
                dioControl.BindDIOInput(() => OutputStage.Ring1(), "Ring Sns 1", "StageRing1");
                dioControl.BindDIOInput(() => OutputStage.IsRingPresent(), "Ring Any", "StageRingAny");

                dioControl.BindVacuum(
                    label: "Vacuum",
                    on: () => OutputStage.SetVacuum(true),
                    off: () => OutputStage.SetVacuum(false),
                    isOk: () => OutputStage.IsVacuumOn(),
                    isOnState: () => OutputStage.IsVacuumValveOn(),
                    displayKey: "StageVacValve",
                    showOkSensor: false // 위에서 OK 센서 이미 표시
                );

                // Plate Up/Down: 도메인 함수 사용, 상태 판단은 IsPlateUp 기준
                dioControl.BindCylinder(
                    label: "PlateUpDn",
                    extend: () => OutputStage.SetClampPlate(true),
                    retract: () => OutputStage.SetClampPlate(false),
                    isExtended: () => OutputStage.IsPlateUp(),
                    isRetracted: () => OutputStage.IsPlateDown(),
                    displayKey: "StagePlateUpDn",
                    showSensors: false // 위에서 Up/Down 센서를 이미 표시했으므로 중복 방지
                );

                // ClampLift Up/Down
                dioControl.BindCylinder(
                    label: "ClampLift",
                    extend: () => OutputStage.SetClampLift(true),
                    retract: () => OutputStage.SetClampLift(false),
                    // Up 센서가 없으면 밸브 상태 사용, Down은 센서 사용
                    isExtended: () => OutputStage.IsClampLiftUp(),
                    isRetracted: () => OutputStage.IsClampLiftDown(),
                    displayKey: "StageClampUpDn",
                    showSensors: false,
                    extendedName: "UP",
                    retractedName: "DOWN"
                );

                // Clamp FWD/BWD
                dioControl.BindCylinder(
                    label: "ClampFB",
                    extend: () => OutputStage.SetClampFB(true),
                    retract: () => OutputStage.SetClampFB(false),
                    // FWD 센서만 있어도 동작. BWD는 없으면 null 가능(토글은 FWD 센서로 판단)
                    isExtended: () => OutputStage.IsClampFwd(),
                    isRetracted: null,
                    displayKey: "StageClampFB",
                    showSensors: false,
                    extendedName: "FWD",
                    retractedName: "BWD"
                );
            }
            catch { }
        }
        #endregion

        #region Camera
        private void BindCamera()
        {
            try
            {
                if (_ChipUnloadingCameraviewer != null && IndexUnloadAligner?.IndexOutCamera != null)
                {
                    if (_ChipUnloadingCameraviewer.Camera != IndexUnloadAligner.IndexOutCamera)
                        _ChipUnloadingCameraviewer.Camera = IndexUnloadAligner.IndexOutCamera;
                    try { IndexUnloadAligner.IndexOutCamera.StartLive(); } catch { }
                    try { _ChipUnloadingCameraviewer.StartUpdateTask(); } catch { }
                }
            }
            catch { }
        }
        #endregion

        #region Sequences (미구현 Placeholder)
        private void InitSequences()
        {
            try
            {
                OutputDieTransfer = TryGetUnit<OutputDieTransfer>("OutputDieTransfer");
                Rotary = TryGetUnit<Rotary>("Rotary");
                IndexUnloadAligner = TryGetUnit<IndexUnloadAligner>("IndexUnloadAligner");
                OutputStage = TryGetUnit<OutputStage>("OutputStage");

                if (OutputDieTransfer != null)
                {
                    manualSequenceControl.ParentUnit = OutputDieTransfer; // 시퀀스 등록 대상 유닛 지정
                }

            }
            catch { }
        }
        #endregion

        private void ChipUnloading_Working_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                System.Threading.Tasks.Task.Delay(500).Wait();
                UnsubscribeRecipeChanged();
                if (_statusTimer != null)
                {
                    _statusTimer.Stop();
                    _statusTimer = null;
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
        }

        private void LightControlRequested(object sender, EventArgs e)
        {
            Form _lightControlPopup = null;

            if (_lightControlPopup != null && !_lightControlPopup.IsDisposed)
            {
                _lightControlPopup.Close();
            }

            Form popupForm = new Form();
            popupForm.Text = "Light Control";
            popupForm.Size = new Size(467, 286);
            popupForm.FormBorderStyle = FormBorderStyle.SizableToolWindow;
            popupForm.MaximizeBox = false;
            popupForm.MinimizeBox = false;
            popupForm.ShowInTaskbar = false;

            popupForm.StartPosition = FormStartPosition.Manual;
            Point cursorPos = Cursor.Position;
            popupForm.Location = cursorPos;

            popupForm.Owner = null;

            SimpleLightControl lightControl = new SimpleLightControl();
            lightControl.Dock = DockStyle.Fill;
            popupForm.Controls.Add(lightControl);

            _lightControlPopup = popupForm;
            popupForm.FormClosed += (s, ev) => { _lightControlPopup = null; };
            popupForm.Show();
            this.Activate();
        }

        private void SubscribeRecipeChanged()
        {
            EquipmentRecipe.CurrentRecipeChanged -= EquipmentRecipe_CurrentRecipeChanged;
            EquipmentRecipe.CurrentRecipeChanged += EquipmentRecipe_CurrentRecipeChanged;
        }

        private void UnsubscribeRecipeChanged()
        {
            EquipmentRecipe.CurrentRecipeChanged -= EquipmentRecipe_CurrentRecipeChanged;
        }

        private void EquipmentRecipe_CurrentRecipeChanged(object sender, EquipmentRecipe.MeasurementRecipeChangedEventArgs e)
        {
            try
            {
                if (!IsHandleCreated)
                {
                    _pendingTeachingReload = true;
                    return;
                }

                if (InvokeRequired)
                {
                    BeginInvoke(new Action(() => EquipmentRecipe_CurrentRecipeChanged(sender, e)));
                    return;
                }

                if (!Visible)
                {
                    _pendingTeachingReload = true;
                    return;
                }

                // 레시피 변경 → 유닛 티칭(Recipe cache 등) 갱신 + TeachingPositionControl 재바인딩/리로드
                try { OutputDieTransfer?.Config?.InvalidateTeachingRecipeCache(); } catch { }

                BindTeachingPositions();
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
        }
    }
}
