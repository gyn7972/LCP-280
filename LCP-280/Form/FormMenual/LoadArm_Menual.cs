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
using MessageBox = System.Windows.Forms.MessageBox;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;
using Timer = System.Windows.Forms.Timer;

namespace QMC.LCP_280.Process.Unit
{
    [FormOrder(1)]
    public partial class LoadArm_Menual : Form
    {
        private Equipment Equipment => Equipment.Instance;

        private InputStage InputStage { get; set; }
        private InputStageEjector InputStageEjector { get; set; }
        private InputDieTransfer InputDieTransfer { get; set; }
        private Rotary Rotary { get; set; }
        // State
        private bool _initialized;
        private bool _preloadRequested;
        private bool _deferredInitDone; // 지연 초기화 완료 여부
        private bool _isLayoutEditMode;

        private CancellationTokenSource _ctsPickUp;

        private volatile bool _pendingTeachingReload;

        #region Constructors
        public LoadArm_Menual() : this(
            TryGetUnit<InputStage>("InputStage"),
            TryGetUnit<InputStageEjector>("InputStageEjector"),
            TryGetUnit<InputDieTransfer>("InputDieTransfer"),
            TryGetUnit<Rotary>("Rotary"))
        { }

        public LoadArm_Menual(InputStage inputStage, 
                            InputStageEjector ejector, 
                            InputDieTransfer dieTransfer, 
                            Rotary rotary)
        {
            InitializeComponent();
            InputStage = inputStage;
            InputStageEjector = ejector;
            InputDieTransfer = dieTransfer;
            Rotary = rotary;

            Load += ChipLoading_Working_Load;
            FormClosing += ChipLoading_Working_FormClosing;

            _ChipLoadingCameraviewer.LightControlRequested += LightControlRequested;

            SubscribeRecipeChanged();
            // ★ [ADD] UI가 다시 보이거나 활성화될 때 pending teaching reload 처리
            this.VisibleChanged += (s, e) =>
            {
                if (!Visible)
                    return;

                if (_pendingTeachingReload)
                {
                    _pendingTeachingReload = false;
                    try { BindTeachingPositions(); } catch { }
                }
            };
            this.Activated += (s, e) =>
            {
                if (_pendingTeachingReload)
                {
                    _pendingTeachingReload = false;
                    try { BindTeachingPositions(); } catch { }
                }
            };
        }
        #endregion

        #region Preload / Init
        public void PreloadUI()
        {
            if (IsDisposed || Disposing) return;
            if (_preloadRequested) return;
            _preloadRequested = true;
            EnsureInitialized();
            var handle = Handle; // 강제 Handle 생성
        }

        private void ChipLoading_Working_Load(object sender, EventArgs e) => EnsureInitialized();

        private void EnsureInitialized()
        {
            if (_initialized) return;
            _initialized = true;
            try
            {
                Text = $"Load Arm";
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
            await Task.Delay(30); // 첫 Paint 후 실행 유도
            if (IsDisposed || Disposing) 
                return;

            try
            {
                InitSequences();

                // ★ [ADD] 최초 진입 시 TeachingRecipe 강제 로드/기본값 보강
                try
                {
                    if (InputDieTransfer != null)
                    {
                        // Config + TeachingRecipe 로드 및 축 바인딩
                        InputDieTransfer.Config.LoadAndBindAxes(Equipment.Instance.AxisManager);

                        // TeachingPositions 비어있으면 기본 생성(파일 저장까지)
                        InputDieTransfer.Config.InitializeDefaultTeachingPositions();

                        // 캐시 갱신(혹시 이전 캐시가 꼬였을 경우)
                        InputDieTransfer.Config.InvalidateTeachingRecipeCache();
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
        private int _lastLoadIndexNo = -1;

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
                int loadIndex0 = -1;

                try { if (Rotary != null) loadIndex0 = Rotary.GetLoadIndexNo(); } catch { }

                if (_statusTimer != null)
                {
                    //dioControl?.RefreshInputs();
                }

                // ===== 인덱스 변경 감지 → TeachingPosition alias 갱신 =====
                // alias 표시의 핵심은 probeIndex0 이므로 최소 그 변화는 감지 필요.
                bool needTeachingRefresh =
                    (loadIndex0 >= 0 && loadIndex0 != _lastLoadIndexNo);


                if (needTeachingRefresh)
                {
                    _lastLoadIndexNo = loadIndex0;
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
        #endregion

        #region TeachingPosition Binding
        private void BindTeachingPositions()
        {
            try
            {
                if (teachingPositionControl == null) return;
                teachingPositionControl.ClearUnits();

                if (InputStage != null)
                {
                    teachingPositionControl.RegisterUnit(
                        "InputStage",
                        InputStage,
                        () => InputStage.Config?.TeachingPositions,
                        (name, vel) => InputStage.MoveToTeachingPosition(name, false),
                        tp => InputStage.Config?.SetTeachingPosition(tp),
                        autoReload: false);
                }
                if (InputStageEjector != null)
                {
                    teachingPositionControl.RegisterUnit(
                        "InputStageEjector",
                        InputStageEjector,
                        () => InputStageEjector.InputStageEjectorConfig?.TeachingPositions,
                        (name, vel) => InputStageEjector.MoveToTeachingPosition(name, false),
                        tp => InputStageEjector.InputStageEjectorConfig?.SetTeachingPosition(tp),
                        autoReload: false);
                }
                if (InputDieTransfer != null)
                {
                    const string TP_PLACE_INDEX_ALIAS = "Place_Index";

                    teachingPositionControl.RegisterUnit(
                        "InputDieTransfer",
                        InputDieTransfer,

                        // 1) Provider: Place_Index1~8 숨기고, alias 1개만 추가(현재 index 값 표시)
                        () =>
                        {
                            var recipe = InputDieTransfer.InputDieTransferConfig?.TeachingRecipe;
                            var list = recipe?.TeachingPositions;
                            if (list == null)
                                return null;

                            var hidden = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                            for (int i = 1; i <= 8; i++)
                                hidden.Add($"Place_Index{i}");

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
                                    // Rotary Load Index 기준으로 Place_Index{1~8} 선택
                                    int idx0 = (Rotary != null) ? Rotary.GetLoadIndexNo() : 0; // 0..7
                                    int idx1 = idx0 + 1;
                                    string realName = $"Place_Index{idx1}";

                                    src = list.FirstOrDefault(t => t != null &&
                                        string.Equals(t.Name, realName, StringComparison.OrdinalIgnoreCase));
                                }
                                catch { /* ignore */ }

                                return CloneForDisplay(aliasName, src, axisNames, desc);
                            }

                            // alias 1개 추가 (현재 index 실제 값 표시)
                            result.Add(BuildAlias(
                                TP_PLACE_INDEX_ALIAS,
                                new[] { AxisNames.LeftToolT, AxisNames.LeftPlaceZ },
                                "Alias: Place_Index (current load index)"));

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
                                if (string.Equals(name, TP_PLACE_INDEX_ALIAS, StringComparison.OrdinalIgnoreCase))
                                {
                                    int idx0 = (Rotary != null) ? Rotary.GetLoadIndexNo() : 0; // 0..7
                                    int idx1 = idx0 + 1;
                                    realName = $"Place_Index{idx1}";
                                }
                            }
                            catch
                            {
                                realName = name; // fallback
                            }

                            InputDieTransfer.MoveToTeachingPosition(realName, false);
                        },

                        // 3) SaveAction: alias로 저장 눌러도 "현재 index의 실제 Place_Index{n}"으로 저장되게 변환
                        tp =>
                        {
                            try
                            {
                                if (tp == null) return;

                                var recipe = InputDieTransfer.InputDieTransferConfig?.TeachingRecipe;
                                if (recipe == null) return;

                                // alias 저장 시 현재 index의 실제 항목으로 name 치환
                                if (string.Equals(tp.Name, TP_PLACE_INDEX_ALIAS, StringComparison.OrdinalIgnoreCase))
                                {
                                    int idx0 = (Rotary != null) ? Rotary.GetLoadIndexNo() : 0; // 0..7
                                    int idx1 = idx0 + 1;

                                    tp.Name = $"Place_Index{idx1}";
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

                //if (InputDieTransfer != null)
                //{
                //    teachingPositionControl.RegisterUnit(
                //        "InputDieTransfer",
                //        InputDieTransfer,
                //        () => InputDieTransfer.InputDieTransferConfig?.TeachingRecipe?.TeachingPositions,
                //        (name, vel) => InputDieTransfer.MoveToTeachingPosition(name, vel: vel),
                //        tp => InputDieTransfer.InputDieTransferConfig?.SetTeachingPosition(tp),
                //        autoReload: false);
                //}

                teachingPositionControl.SetUnitAbbreviation("InputStage", "Stage", reload: false);
                teachingPositionControl.SetUnitAbbreviation("InputStageEjector", "Niddle", reload: false);
                teachingPositionControl.SetUnitAbbreviation("InputDieTransfer", "Arm", reload: false);

                teachingPositionControl.SetSaveCancelVisible(false, false);
                teachingPositionControl.RefreshData();
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

                // 구분선: InputStage
                dioControl.BindDIOInput(() => false, "---- InputStage ----", "Stage");
                StrongBindInputStage();

                // 구분선: InputDieTransfer
                if (InputDieTransfer != null)
                {
                    dioControl.BindDIOInput(() => false, "---- InputDieTransfer ----", "Arm");

                    for (int arm = 0; arm < 1; arm++) // 필요 시 4로 변경: arm < 4
                    {
                        int idx = arm;
                        string labelBase = $"Arm{idx + 1}";

                        // Flow 센서(입력) 표시
                        dioControl.BindDIOInput(
                            () => InputDieTransfer.IsVacuumOK(idx),
                            $"{labelBase} Flow OK(Sns)",
                            $"Arm{idx + 1}_FlowOk");

                        // VAC: 도메인 + 상태 함수 연결 (출력은 입력과 무관하게 동작)
                        dioControl.BindVacuum(
                            label: $"{labelBase} VAC",
                            on: () => InputDieTransfer.SetVacuum(idx, true),
                            off: () => InputDieTransfer.SetVacuum(idx, false),
                            isOk: null,
                            isOnState: null,
                            displayKey: $"Arm{idx + 1}_Vac",
                            showOkSensor: false
                        );

                        dioControl.BindVacuum(
                            label: $"{labelBase} Blow",
                            on: () => InputDieTransfer.SetBlow(idx, true),
                            off: () => InputDieTransfer.SetBlow(idx, false),
                            isOk: null, // 별도 센서 없음(Flow는 위에서 별도 표시)
                            isOnState: null,
                            displayKey: $"Arm{idx + 1}_Blow",
                            showOkSensor: false
                        );

                        dioControl.BindVacuum(
                            label: $"{labelBase} Vent",
                            on: () => InputDieTransfer.SetVent(idx, true),
                            off: () => InputDieTransfer.SetVent(idx, false),
                            isOk: null, // 별도 센서 없음(Flow는 위에서 별도 표시)
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

        private void StrongBindInputStage()
        {
            if (InputStage == null || dioControl == null) return;
            try
            {
                // ===== Sensors =====
                dioControl.BindDIOInput(() => InputStage.IsVacuumOn(), "Vacuum OK(Sns)", "StageVacOk");
                dioControl.BindDIOInput(() => InputStage.IsPlateUp(), "Plate UP Sns", "StagePlateUp");
                dioControl.BindDIOInput(() => InputStage.IsPlateDown(), "Plate DOWN Sns", "StagePlateDn");
                dioControl.BindDIOInput(() => InputStage.IsClampLiftDown(), "ClampLift DOWN Sns", "StageClampDn");
                dioControl.BindDIOInput(() => InputStage.IsClampFwd(), "Clamp FWD Sns", "StageClampFwd");
                dioControl.BindDIOInput(() => InputStage.Ring0(), "Ring Sns 0", "StageRing0");
                dioControl.BindDIOInput(() => InputStage.Ring1(), "Ring Sns 1", "StageRing1");
                dioControl.BindDIOInput(() => InputStage.IsRingPresent(), "Ring Any", "StageRingAny");

                // Vacuum: 도메인 함수 사용 (출력은 입력과 무관하게 동작, 상태 표시는 밸브 상태 함수 사용)
                dioControl.BindVacuum(
                    label: "Vacuum",
                    on: () => InputStage.SetVacuum(true),
                    off: () => InputStage.SetVacuum(false),
                    isOk: () => InputStage.IsVacuumOn(),
                    isOnState: () => InputStage.IsVacuumValveOn(),
                    displayKey: "StageVac",
                    showOkSensor: false // 위에서 OK 센서를 이미 표시했으므로 중복 방지
                );

                // Plate Up/Down: 도메인 함수 사용, 상태 판단은 IsPlateUp 기준
                dioControl.BindCylinder(
                    label: "PlateUpDn",
                    extend: () => InputStage.SetClampPlate(true),
                    retract: () => InputStage.SetClampPlate(false),
                    isExtended: () => InputStage.IsPlateUp(),
                    isRetracted: () => InputStage.IsPlateDown(),
                    displayKey: "StagePlateUpDn",
                    showSensors: false // 위에서 Up/Down 센서를 이미 표시했으므로 중복 방지
                );

                // ClampLift Up/Down
                dioControl.BindCylinder(
                    label: "ClampLift",
                    extend: () => InputStage.SetClampLift(true),
                    retract: () => InputStage.SetClampLift(false),
                    // Up 센서가 없으면 밸브 상태 사용, Down은 센서 사용
                    isExtended: () => InputStage.IsClampLiftUp(),
                    isRetracted: () => InputStage.IsClampLiftDown(),
                    displayKey: "StageClampUpDn",
                    showSensors: false,
                    extendedName: "UP",
                    retractedName: "DOWN"
                );

                // Clamp FWD/BWD
                dioControl.BindCylinder(
                    label: "ClampFB",
                    extend: () => InputStage.SetClampFB(true),
                    retract: () => InputStage.SetClampFB(false),
                    // FWD 센서만 있어도 동작. BWD는 없으면 null 가능(토글은 FWD 센서로 판단)
                    isExtended: () => InputStage.IsClampFwd(),
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
                if (_ChipLoadingCameraviewer != null && InputStage?.StageCamera != null)
                {
                    if (_ChipLoadingCameraviewer.Camera != InputStage.StageCamera)
                        _ChipLoadingCameraviewer.Camera = InputStage.StageCamera;
                    try { InputStage.StageCamera.StartLive(); } catch { }
                    try { _ChipLoadingCameraviewer.StartUpdateTask(); } catch { }
                }
            }
            catch { }
        }
        #endregion

        #region Sequences
        private void InitSequences()
        {
            try
            {
                // 최신 Equipment 등록본으로 다시 참조 갱신 (폼 생성 후 재초기화 상황 대비)
                InputStage        = TryGetUnit<InputStage>("InputStage");
                InputStageEjector = TryGetUnit<InputStageEjector>("InputStageEjector");
                InputDieTransfer  = TryGetUnit<InputDieTransfer>("InputDieTransfer");

                if (InputDieTransfer != null)
                {
                    manualSequenceControl.ParentUnit = InputDieTransfer; // 시퀀스 등록 대상 유닛 지정
                }
            }
            catch { }
        }
        #endregion

        #region Events
        private void ChipLoading_Working_FormClosing(object sender, FormClosingEventArgs e)
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

        private void _btnVisionSetting_Click(object sender, EventArgs e)
        {
           
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



        #endregion


        private int GetSelectedLoadIndex()
        {
            if (cbLoadIndex == null) return -1;

            if (cbLoadIndex.InvokeRequired)
            {
                try
                {
                    var idx = (int)cbLoadIndex.Invoke(new Func<int>(() => cbLoadIndex.SelectedIndex));
                    return idx < 0 ? -1 : idx;
                }
                catch
                {
                    return -1;
                }
            }

            var selected = cbLoadIndex.SelectedIndex;
            return selected < 0 ? -1 : selected;
        }

        private  async void btnDiePickUp_Click(object sender, EventArgs e)
        {
            var ask = new MessageBoxYesNo();
            if(ask.ShowDialog("Question", "Die PickUp?") != DialogResult.Yes)
                return;

            int nRet = 0;
            var cts = new CancellationTokenSource();
            var token = cts.Token;

            SetUnitsManualRunning(true);
            var t = Task.Run(async () =>
            {
                try
                {
                    if (InputDieTransfer != null)
                    {
                        token.ThrowIfCancellationRequested();
                        nRet = await Task.Run(() => PickDieFromWaferAsync(token), token).ConfigureAwait(false);
                        if (nRet != 0)
                        {
                            //MessageBox.Show("Pick Up Fail", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            Log.Write(InputDieTransfer.UnitName, "MoveUnitsToReady", "Rotary EnsureReady failed");
                            return nRet;
                        }
                    }
                    return nRet; // 모두 OK
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    Log.Write(ex);
                    return nRet;
                }
            }, token);

            var form = new ProgressForm("Manual Running", "DiePickUp", t, null);
            try
            {
                var mb = new MessageBoxOk();
                form.ShowDialog();
                if (t.IsFaulted)
                {
                    mb.ShowDialog("DiePickUp Fail!", t.Exception?.GetBaseException().Message);
                    SetUnitsManualRunning(false);
                    return;
                }

                if (t.IsCanceled)
                {
                    //throw new OperationCanceledException();
                    mb.ShowDialog("DiePickUp Cancel!", t.Exception?.GetBaseException().Message);
                    SetUnitsManualRunning(false);
                    return;
                }

                var rc = await t.ConfigureAwait(true);
                if (rc != 0)
                {
                    MessageBox.Show("DiePickUp Fail",
                        "DiePickUp Fail", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                SetUnitsManualRunning(false);
                return;
            }
            catch (OperationCanceledException)
            {
                SetUnitsManualRunning(false);
                throw;
            }
            catch (Exception ex)
            {
                SetUnitsManualRunning(false);
                Log.Write(ex);
                MessageBox.Show("DiePickUp Exception: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            finally
            {
                SetUnitsManualRunning(false);
            }
        }

        private async void btnDiePlaceDown_Click(object sender, EventArgs e)
        {
            var ask = new MessageBoxYesNo();
            if (ask.ShowDialog("Question", "Die PlaceDown?") != DialogResult.Yes)
            {
                return;
            }

            int nRet = 0;
            var cts = new CancellationTokenSource();
            var token = cts.Token;

            SetUnitsManualRunning(true);
            var t = Task.Run(async () =>
            {
                try
                {
                    // 1) 선택된 Probe Index로 이동(선택 없으면 현재 유지)
                    int selectedProbeIndex = GetSelectedLoadIndex();
                    if (selectedProbeIndex >= 0)
                    {
                        token.ThrowIfCancellationRequested();
                        nRet = await Task.Run(() => MoveRotaryToLoadSocketAsync(selectedProbeIndex, token), token).ConfigureAwait(false);
                        if (nRet != 0)
                        {
                            //MessageBox.Show("Rotary Target Socket Move Fail", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            Log.Write(InputDieTransfer.UnitName, "btnDiePlaceDown_Click", "MoveRotaryToLoadSocketAsync failed");
                            return nRet;
                        }
                    }
                    else
                    {
                        return -1;
                    }

                    nRet = await Task.Run(() => PlaceDieFromArmToCurrentSocketAsync(token), token).ConfigureAwait(false);
                    if (nRet != 0)
                    {
                        //MessageBox.Show("DiePlaceDown Fail.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        Log.Write(InputDieTransfer.UnitName, "btnDiePlaceDown_Click", "MoveRotaryToLoadSocketAsync failed");
                        return nRet;
                    }

                    return nRet; // 모두 OK
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    Log.Write(ex);
                    return nRet;
                }
            }, token);

            var form = new ProgressForm("Manual Running", "DiePlaceDown", t, null);
            try
            {
                var mb = new MessageBoxOk();
                form.ShowDialog();
                if (t.IsFaulted)
                {
                    mb.ShowDialog("DiePlaceDown Fail!", t.Exception?.GetBaseException().Message);
                    SetUnitsManualRunning(false);
                    return;
                }

                if (t.IsCanceled)
                {
                    //throw new OperationCanceledException();
                    mb.ShowDialog("DiePlaceDown Cancel!", t.Exception?.GetBaseException().Message);
                    SetUnitsManualRunning(false);
                    return;
                }

                var rc = await t.ConfigureAwait(true);
                if (rc != 0)
                {
                    MessageBox.Show("DiePlaceDown Fail",
                        "DiePlaceDown Fail", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                SetUnitsManualRunning(false);
                return;
            }
            catch (OperationCanceledException)
            {
                SetUnitsManualRunning(false);
                throw;
            }
            catch (Exception ex)
            {
                SetUnitsManualRunning(false);
                Log.Write(ex);
                MessageBox.Show("DiePlaceDown Exception: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            finally
            {
                SetUnitsManualRunning(false);
            }

            //int nRet = 0;
            //try
            //{
            //    var cts = new CancellationTokenSource();
            //    var token = cts.Token;
            //    // 1) 선택된 Probe Index로 이동(선택 없으면 현재 유지)
            //    int selectedProbeIndex = GetSelectedLoadIndex();
            //    if (selectedProbeIndex >= 0)
            //    {
            //        nRet = await Task.Run(() => MoveRotaryToLoadSocketAsync(selectedProbeIndex, token));
            //        if (nRet != 0)
            //        {
            //            MessageBox.Show("Rotary 목표 소켓 이동 실패", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //            return;
            //        }
            //    }

            //    nRet = await Task.Run(() => PlaceDieFromArmToCurrentSocketAsync(token));
            //    if (nRet != 0)
            //    {
            //        MessageBox.Show("Pick Down Fail.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //        return;
            //    }
            //}
            //catch (Exception ex)
            //{
            //    Log.Write(ex);
            //}
        }


        private void SetUnitsManualRunning(bool running)
        {
            try
            {
                if (running)
                {
                    Rotary.StartManual();
                    InputDieTransfer.StartManual();
                    InputStage.StartManual();
                    InputStageEjector.StartManual();
                }
                else
                {
                    Rotary.Stop();
                    InputDieTransfer.Stop();
                    InputStage.Stop();
                    InputStageEjector.Stop();
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
        }


        // Rotary를 목표 LoadIndex로 이동(회전 + 대기)
        private async Task<int> MoveRotaryToLoadSocketAsync(int targetSocket, CancellationToken ct)
        {
            int targetIdx0 = (targetSocket + 8) % 8; // 0~7
            for (int i = 0; i < 16; i++)
            {
                ct.ThrowIfCancellationRequested();
                int cur = Rotary.GetLoadIndexNo();
                if (cur == targetIdx0)
                    return 0;

                int rc = Rotary.MovePositionRotate();
                if (rc != 0)
                    return -1;

                rc = Rotary.WaitIndexMoveDone();
                if (rc != 0)
                    return -1;

                await Task.Delay(50, ct).ConfigureAwait(false);
            }
            return -1;
        }

        private async Task<int> PickDieFromWaferAsync(CancellationToken ct)
        {
            return await Task.Run(() =>
            {
                int nRet = 0;

                //흠
                //nRet = InputDieTransfer.IsVacuumOK(0) ? 0 : -1;
                //if (nRet == 0)
                //{
                //    MessageBoxOk mb = new MessageBoxOk();
                //    mb.ShowDialog("VacuumOn", "Already have die on arm.");
                //    Log.Write(InputDieTransfer.UnitName, "PickDieFromWaferAsync", "Vacuum OK");
                //    return -1;
                //}


                nRet = InputDieTransfer.MovePositionReady();
                if (nRet != 0)
                {
                    Log.Write(InputDieTransfer.UnitName, "PickDieFromWaferAsync", "RecheckDieAndAlign failed");
                    return -1;
                }

                nRet = InputDieTransfer.RecheckDieAndAlign();
                if (nRet != 0)
                {
                    Log.Write(InputDieTransfer.UnitName, "PickDieFromWaferAsync", "RecheckDieAndAlign failed");
                    return -1;
                }

                nRet = InputDieTransfer.PrepareNextDie();
                if (nRet != 0)
                {
                    MessageBoxOk mb = new MessageBoxOk();
                    mb.ShowDialog("PrepareNextDie", "PrepareNextDie Fial.");
                    Log.Write(InputDieTransfer.UnitName, "PickDieFromWaferAsync", "PrepareNextDie");
                    return -1;
                }

                nRet = InputDieTransfer.RaiseEjectorForPick(); if (nRet != 0) return -1;
                nRet = InputDieTransfer.ChipPickDown(); if (nRet != 0) return -1;
                nRet = InputDieTransfer.SyncPickPinUp(); if (nRet != 0) return -1;
                nRet = InputDieTransfer.SyncPickPinRetreat(); if (nRet != 0) return -1;
                nRet = InputDieTransfer.CommitPickedDie(); if (nRet != 0) return -1;
                return 0;
            }, ct).ConfigureAwait(false);
        }


        private async Task<int> PlaceDieFromArmToCurrentSocketAsync(CancellationToken ct)
        {
            return await Task.Run(() =>
            {
                int nRet = 0;

                //nRet = InputDieTransfer.IsVacuumOK(0) ? 0 : -1;
                //if (nRet != 0)
                //{
                //    MessageBoxOk mb = new MessageBoxOk();
                //    mb.ShowDialog("VacuumOff", "Already have not die on arm.");
                //    Log.Write(InputDieTransfer.UnitName, "PlaceDieFromArmToCurrentSocketAsync", "Vacuum Off ");
                //    return -1;
                //}

                nRet = InputDieTransfer.RotateToolTForPlace_AsyncWait(); if (nRet != 0) return -1;
                nRet = InputDieTransfer.PlaceChipDown(); if (nRet != 0) return -1;
                nRet = InputDieTransfer.ReleaseVacuumAndPlaceUp(); if (nRet != 0) return -1; // 암 Off, 로터리 Vac On
                return 0;
            }, ct).ConfigureAwait(false);
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
                try { InputDieTransfer?.Config?.InvalidateTeachingRecipeCache(); } catch { }

                BindTeachingPositions();
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
        }


    }
}