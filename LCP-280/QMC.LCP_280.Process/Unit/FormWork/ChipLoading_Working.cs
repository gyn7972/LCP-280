using QMC.Common;
using QMC.LCP_280.Process.Component; 
using QMC.LCP_280.Process.Sequences;
using System;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QMC.LCP_280.Process.Unit
{
    /// <summary>
    /// ChipLoading Working Form
    ///  - TeachingPositionControl : InputStage / InputStageEjector / InputDieTransfer 등록
    ///  - DIO 제어 :
    ///      InputStage : 센서 + 밸브 (Raw 포함) 강制 제어
    ///      InputDieTransfer : Arm Vac / Blow / Vent 제어 (4 Arms)
    ///  - Vision : Stage Camera Live 연결
    ///  - Manual Sequence : (InputStage Align / Mapping / Pick / Place 등 등록)
    /// </summary>
    public partial class ChipLoading_Working : Form
    {
        private const string WORK_NAME = "ChipLoading";
        private Equipment Equipment => Equipment.Instance;

        // Units
        private InputStage InputStageUnit { get; set; }
        private InputStageEjector InputStageEjectorUnit { get; set; }
        private InputDieTransfer InputDieTransferUnit { get; set; }

        // State
        private bool _initialized;
        private bool _preloadRequested;
        private bool _deferredInitDone; // 지연 초기화 완료 여부
        private bool _isLayoutEditMode;

        #region Constructors
        public ChipLoading_Working() : this(
            TryGetUnit<InputStage>("InputStage"),
            TryGetUnit<InputStageEjector>("InputStageEjector"),
            TryGetUnit<InputDieTransfer>("InputDieTransfer"))
        { }

        public ChipLoading_Working(InputStage inputStage, InputStageEjector ejector, InputDieTransfer dieTransfer)
        {
            InitializeComponent();
            InputStageUnit = inputStage;
            InputStageEjectorUnit = ejector;
            InputDieTransferUnit = dieTransfer;
            Load += ChipLoading_Working_Load;
            FormClosing += ChipLoading_Working_FormClosing;
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
                Text = $"{WORK_NAME} Working";
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
            if (_deferredInitDone) return;
            _deferredInitDone = true;
            await Task.Delay(30); // 첫 Paint 후 실행 유도
            if (IsDisposed || Disposing) return;
            try
            {
                BindTeachingPositions();
                BindDioControls();
                BindCamera();
                InitSequences();
            }
            catch { }
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

                if (InputStageUnit != null)
                {
                    teachingPositionControl.RegisterUnit(
                        "InputStage",
                        InputStageUnit,
                        () => InputStageUnit.Config?.TeachingPositions,
                        (name, vel) => InputStageUnit.MoveToTeachingPosition(name, vel: vel),
                        tp => InputStageUnit.Config?.SetTeachingPosition(tp),
                        autoReload: false);
                }
                if (InputStageEjectorUnit != null)
                {
                    teachingPositionControl.RegisterUnit(
                        "InputStageEjector",
                        InputStageEjectorUnit,
                        () => InputStageEjectorUnit.InputStageEjectorConfig?.TeachingPositions,
                        (name, vel) => InputStageEjectorUnit.MoveToTeachingPosition(name, vel: vel),
                        tp => InputStageEjectorUnit.InputStageEjectorConfig?.SetTeachingPosition(tp),
                        autoReload: false);
                }
                if (InputDieTransferUnit != null)
                {
                    teachingPositionControl.RegisterUnit(
                        "InputDieTransfer",
                        InputDieTransferUnit,
                        () => InputDieTransferUnit.InputDieTransferConfig?.TeachingPositions,
                        (name, vel) => InputDieTransferUnit.MoveToTeachingPosition(name, vel: vel),
                        tp => InputDieTransferUnit.InputDieTransferConfig?.SetTeachingPosition(tp),
                        autoReload: false);
                }

                teachingPositionControl.SetSaveCancelVisible(false, false);
                teachingPositionControl.RefreshData();
            }
            catch
            {
                try { teachingPositionControl?.SetUnits(InputStageUnit, InputStageEjectorUnit, InputDieTransferUnit, true); } catch { }
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
                dioControl.BindDIOInput(() => false, "---- InputStage ----", "SEP_InStage");
                StrongBindInputStage();

                // 구분선: InputDieTransfer
                if (InputDieTransferUnit != null)
                {
                    dioControl.BindDIOInput(() => false, "---- InputDieTransfer ----", "SEP_IDT");

                    for (int arm = 0; arm < 1; arm++) // 필요 시 4로 변경: arm < 4
                    {
                        int idx = arm;
                        string labelBase = $"IDT Arm{idx + 1}";

                        // Flow 센서(입력) 표시
                        dioControl.BindDIOInput(
                            () => InputDieTransferUnit.ArmFlowOk(idx),
                            $"{labelBase} Flow OK(Sns)",
                            $"IDT_Arm{idx + 1}_FlowOk");

                        // VAC: 도메인 + 상태 함수 연결 (출력은 입력과 무관하게 동작)
                        dioControl.BindVacuum(
                            label: $"{labelBase} VAC",
                            on: () => InputDieTransferUnit.SetVacuum(idx, true),
                            off: () => InputDieTransferUnit.SetVacuum(idx, false),
                            isOk: null,
                            isOnState: null,
                            displayKey: $"IDT_Arm{idx + 1}_Vac",
                            showOkSensor: false
                        );

                        dioControl.BindVacuum(
                            label: $"{labelBase} Blow",
                            on: () => InputDieTransferUnit.SetBlow(idx, true),
                            off: () => InputDieTransferUnit.SetBlow(idx, false),
                            isOk: null, // 별도 센서 없음(Flow는 위에서 별도 표시)
                            isOnState: null,
                            displayKey: $"IDT_Arm{idx + 1}_Blow",
                            showOkSensor: false
                        );

                        dioControl.BindVacuum(
                            label: $"{labelBase} Vent",
                            on: () => InputDieTransferUnit.SetVent(idx, true),
                            off: () => InputDieTransferUnit.SetVent(idx, false),
                            isOk: null, // 별도 센서 없음(Flow는 위에서 별도 표시)
                            isOnState: null,
                            displayKey: $"IDT_Arm{idx + 1}_Vent",
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
            if (InputStageUnit == null || dioControl == null) return;
            try
            {
                // ===== Sensors =====
                dioControl.BindDIOInput(() => InputStageUnit.IsVacuum(), "Vacuum OK(Sns)", "StageVacOk");
                dioControl.BindDIOInput(() => InputStageUnit.IsPlateUp(), "Plate UP Sns", "StagePlateUp");
                dioControl.BindDIOInput(() => InputStageUnit.IsPlateDown(), "Plate DOWN Sns", "StagePlateDn");
                dioControl.BindDIOInput(() => InputStageUnit.IsClampLiftDown(), "ClampLift DOWN Sns", "StageClampDn");
                dioControl.BindDIOInput(() => InputStageUnit.IsClampFwd(), "Clamp FWD Sns", "StageClampFwd");
                dioControl.BindDIOInput(() => InputStageUnit.Ring0(), "Ring Sns 0", "StageRing0");
                dioControl.BindDIOInput(() => InputStageUnit.Ring1(), "Ring Sns 1", "StageRing1");
                dioControl.BindDIOInput(() => InputStageUnit.IsRingPresent(), "Ring Any", "StageRingAny");

                // Vacuum: 도메인 함수 사용 (출력은 입력과 무관하게 동작, 상태 표시는 밸브 상태 함수 사용)
                dioControl.BindVacuum(
                    label: "Vacuum",
                    on: () => InputStageUnit.SetVacuum(true),
                    off: () => InputStageUnit.SetVacuum(false),
                    isOk: () => InputStageUnit.IsVacuum(),
                    isOnState: () => InputStageUnit.IsVacuumValveOn(),
                    displayKey: "StageVac",
                    showOkSensor: false // 위에서 OK 센서를 이미 표시했으므로 중복 방지
                );

                // Plate Up/Down: 도메인 함수 사용, 상태 판단은 IsPlateUp 기준
                dioControl.BindCylinder(
                    label: "PlateUpDn",
                    extend: () => InputStageUnit.SetClampPlate(true),
                    retract: () => InputStageUnit.SetClampPlate(false),
                    isExtended: () => InputStageUnit.IsPlateUp(),
                    isRetracted: () => InputStageUnit.IsPlateDown(),
                    displayKey: "StagePlateUpDn",
                    showSensors: false // 위에서 Up/Down 센서를 이미 표시했으므로 중복 방지
                );

                // ClampLift Up/Down
                dioControl.BindCylinder(
                    label: "ClampLift",
                    extend: () => InputStageUnit.SetClampLift(true),
                    retract: () => InputStageUnit.SetClampLift(false),
                    // Up 센서가 없으면 밸브 상태 사용, Down은 센서 사용
                    isExtended: () => InputStageUnit.IsClampLiftUpValveOn(),
                    isRetracted: () => InputStageUnit.IsClampLiftDown(),
                    displayKey: "StageClampUpDn",
                    showSensors: false,
                    extendedName: "UP",
                    retractedName: "DOWN"
                );

                // Clamp FWD/BWD
                dioControl.BindCylinder(
                    label: "ClampFB",
                    extend: () => InputStageUnit.SetClampFB(true),
                    retract: () => InputStageUnit.SetClampFB(false),
                    // FWD 센서만 있어도 동작. BWD는 없으면 null 가능(토글은 FWD 센서로 판단)
                    isExtended: () => InputStageUnit.IsClampFwd(),
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
                if (_ChipLoadingCameraviewer != null && InputStageUnit?.StageCamera != null)
                {
                    if (_ChipLoadingCameraviewer.Camera != InputStageUnit.StageCamera)
                        _ChipLoadingCameraviewer.Camera = InputStageUnit.StageCamera;
                    try { InputStageUnit.StageCamera.StartLive(); } catch { }
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
                InputStageUnit        = TryGetUnit<InputStage>("InputStage");
                InputStageEjectorUnit = TryGetUnit<InputStageEjector>("InputStageEjector");
                InputDieTransferUnit  = TryGetUnit<InputDieTransfer>("InputDieTransfer");

                if (manualSequenceControl != null)
                    manualSequenceControl.ClearSequences();

                // InputStage sequence
                if (InputStageUnit != null)
                {
                    // T_Align 등록
                    //manualSequenceControl.RegisterSequence(
                    //    "Stage_T_Align",
                    //    InputStageUnit.SeqTAlign,
                    //    stepNameProvider: () => T_AlignSeq.GetStepNames(),
                    //    startSingleHandler: (name) => InputStageUnit.SeqTAlign.StartSingle(name),
                    //    stepIndexNameResolver: (idx) => T_AlignSeq.GetStepName(idx)
                    //);

                    // XY_Align 등록
                    //manualSequenceControl.RegisterSequence(
                    //    "Stage_XY_Align",
                    //    InputStageUnit.SeqXYAlign,
                    //    stepNameProvider: () => XY_AlignSeq.GetStepNames(),
                    //    startSingleHandler: (name) => InputStageUnit.SeqXYAlign.StartSingle(name),
                    //    stepIndexNameResolver: (idx) => XY_AlignSeq.GetStepName(idx)
                    //);

                    //if (SeqInputStage == null)
                    //    // 기존: new SeqInputStage(InputStageUnit) -> Ejector 포함 생성자로 변경
                    //    SeqInputStage = new SeqInputStage(InputStageUnit, InputStageEjectorUnit);
                    //manualSequenceControl?.RegisterSequence(
                    //    "InputStage",
                    //    SeqInputStage,
                    //    () => Enum.GetNames(typeof(SeqInputStage.Step)),
                    //    step => SeqInputStage.StartSingle(step),
                    //    idx => Enum.GetName(typeof(SeqInputStage.Step), idx),
                    //    autoSelect: true);
                }
            }
            catch { }
        }
        #endregion

        #region Events
        private void ChipLoading_Working_FormClosing(object sender, FormClosingEventArgs e)
        {
            System.Threading.Tasks.Task.Delay(500).Wait();
            
        }

        private void _btnVisionSetting_Click(object sender, EventArgs e)
        {
           
        }
        #endregion

        private void buttonDataManual_Click(object sender, EventArgs e)
        {
            ProcessDataManual dlg = new ProcessDataManual();
            dlg.ShowDialog();
        }

        private async void buttonPickUpNiddle_Move_Click(object sender, EventArgs e)
        {
            // 시나리오:
            // 1) PinZ 를 EjectPinWaiting 위치로 이동(대기).
            // 2) 그 후 EjectPinOffset 만큼 (Offset - Waiting) 상대 이동.
            // 3) 동일한 상대 이동량을 PickZ 에 적용 (동기 이동).
            try
            {
                var ejector  = InputStageEjectorUnit;
                var transfer = InputDieTransferUnit;
                if (ejector?.AxisPinZ == null || transfer?.AxisPickZ == null)
                {
                    MessageBox.Show(this, "축 준비가 되지 않았습니다.", "Axis Not Ready", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Teaching Position 값 가져오기
                double pinWaiting = 0, pinOffset = 0;
                try
                {
                    var posWaiting = ejector.InputStageEjectorConfig?.GetPositionWithOffset(InputStageEjectorConfig.TeachingPositionName.EjectPinReady.ToString());
                    var posOffset  = ejector.InputStageEjectorConfig?.GetPositionWithOffset(InputStageEjectorConfig.TeachingPositionName.EjectPinOffset.ToString());
                    if (posWaiting.HasValue) pinWaiting = posWaiting.Value.pinZ;
                    if (posOffset.HasValue)  pinOffset  = posOffset.Value.pinZ;
                }
                catch { }

                double curPinZ  = ejector.AxisPinZ.GetPosition();
                double curPickZ = transfer.AxisPickZ.GetPosition();

                // 1단계: PinZ Waiting 이동 필요 여부
                bool needMoveToWaiting = Math.Abs(curPinZ - pinWaiting) > (ejector.AxisPinZ.Config?.InposTolerance ?? 0.005) * 2;

                // 공통 속도 (두 축의 80%)
                double vPin  = ejector.AxisPinZ.Config?.MaxVelocity  ?? 10;
                double vPick = transfer.AxisPickZ.Config?.MaxVelocity   ?? 10;
                double commonVel = Math.Min(vPin, vPick) * 0.8; if (commonVel <= 0) commonVel = Math.Min(vPin, vPick);
                double accPin  = ejector.AxisPinZ.Config?.RunAcc  ?? 10; double decPin  = ejector.AxisPinZ.Config?.RunDec  ?? accPin; double jerkPin  = ejector.AxisPinZ.Config?.AccJerkPercent  ?? 50;
                double accPick = transfer.AxisPickZ.Config?.RunAcc    ?? 10; double decPick = transfer.AxisPickZ.Config?.RunDec    ?? accPick; double jerkPick = transfer.AxisPickZ.Config?.AccJerkPercent    ?? 50;

                buttonPickUpNiddle_Move.Enabled = false;

                // 1단계: Waiting 으로 이동
                if (needMoveToWaiting)
                {
                    string msg1 = "PinZ 를 Waiting 위치로 이동하시겠습니까?" +
                                  "\r\n" +
                                  $" Current PinZ : {curPinZ:0.###}" +
                                  "\r\n" +
                                  $" Waiting PinZ : {pinWaiting:0.###}";
                    if (MessageBox.Show(this, msg1, "Move To Waiting", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                    {
                        buttonPickUpNiddle_Move.Enabled = true;
                        return;
                    }

                    await Task.Run(() =>
                    {
                        ejector.AxisPinZ.MoveAbs(pinWaiting, commonVel, accPin, decPin, jerkPin);
                        ejector.AxisPinZ.WaitMoveDone(-1);
                    });
                }

                // PinZ 가 Waiting 에 위치한 후 현재값 갱신
                curPinZ  = ejector.AxisPinZ.GetPosition();
                curPickZ = transfer.AxisPickZ.GetPosition();

                // 2단계: Offset 만큼 상대 이동 (delta = Offset - Waiting)
                double deltaPin = pinOffset - pinWaiting; // 이 상대 이동량을 PickZ 에 동일 적용
                double finalPinZ = curPinZ + deltaPin;     // PinZ: 현재(Waiting)에서 delta 이동 → Offset 절대 위치
                double finalPickZ = curPickZ + deltaPin;   // PickZ: 현재 위치에서 delta 이동 (동기)

                string direction = deltaPin >= 0 ? "+" : "-";
                string msg2 = "Offset 상대 이동을 실행하시겠습니까?" +
                              "\r\n" +
                              $" Delta PinZ : {deltaPin:0.###} ({direction})" +
                              "\r\n" +
                              $" PinZ  : {curPinZ:0.###} -> {finalPinZ:0.###}" +
                              "\r\n" +
                              $" PickZ : {curPickZ:0.###} -> {finalPickZ:0.###}";
                if (MessageBox.Show(this, msg2, "Delta Move", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                {
                    buttonPickUpNiddle_Move.Enabled = true;
                    return;
                }

                await Task.Run(() =>
                {
                    // 동시 이동
                    transfer.AxisPickZ.MoveAbs(finalPickZ,  commonVel, accPick, decPick, jerkPick);
                    ejector.AxisPinZ.MoveAbs(finalPinZ, commonVel, accPin,  decPin,  jerkPin);
                    transfer.AxisPickZ.WaitMoveDone(-1);
                    ejector.AxisPinZ.WaitMoveDone(-1);
                });

                MessageBox.Show(this, "동기 이동 완료", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                try { buttonPickUpNiddle_Move.Enabled = true; } catch { }
            }
        }

        private void buttonTest_Click(object sender, EventArgs e)
        {
            InputStageUnit.SetClampLift(true);
        }

        private void buttonTest2_Click(object sender, EventArgs e)
        {
            InputStageUnit.SetClampLift(false);
        }

        private async void btnWaferLoading_Click(object sender, EventArgs e)
        {
            var ask = new MessageBoxYesNo();
            if (ask.ShowDialog("확인", "시컨스를 진행하시겠습니까?") != DialogResult.Yes)
                return;

            InputStageUnit.LoadingWafer();
        }

        private async void btnAlignT_Click(object sender, EventArgs e)
        {
            var ask = new MessageBoxYesNo();
            if (ask.ShowDialog("확인", "시컨스를 진행하시겠습니까?") != DialogResult.Yes)
                return;

            InputStageUnit.AlignT();
        }

        private async void btnAlignXY_Click(object sender, EventArgs e)
        {
            var ask = new MessageBoxYesNo();
            if (ask.ShowDialog("확인", "시컨스를 진행하시겠습니까?") != DialogResult.Yes)
                return;

            InputStageUnit.AlignXY();
        }

        private async void btnWaferUnloading_Click(object sender, EventArgs e)
        {
            var ask = new MessageBoxYesNo();
            if (ask.ShowDialog("확인", "시컨스를 진행하시겠습니까?") != DialogResult.Yes)
                return;

            InputStageUnit.UnloadingWafer();
        }

        private async void btnPickUpNiddleMove_Click(object sender, EventArgs e)
        {
            var ask = new MessageBoxYesNo();
            if (ask.ShowDialog("확인", "동시에 움직이시겠습니까?") != DialogResult.Yes)
                return;

            btnPickUpNiddleMove.Enabled = false;
            try
            {
                double pickZOffset = 0;
                double pinZOffset = 0;
                double velPickZ = 10; // 필요 시 예: (InputDieTransferUnit.AxisPickZ.Config.MaxVelocity * 0.8);
                double velPinZ = 10;
                double acc = 20;
                double dec = 20;
                int timeoutMs = 5000;   // 필요 시 예: 5000;
                bool isFine = false;

                int rc = await InputDieTransferUnit.MovePickZAndPinZByOffsetAsync(
                    pickZOffset,
                    pinZOffset,
                    velPickZ,
                    velPinZ,
                    acc,
                    dec,
                    timeoutMs,
                    isFine);

                if (rc == 0)
                    MessageBox.Show(this, "동시 이동 완료", "OK",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                else if (rc == -2)
                    MessageBox.Show(this, "타임아웃", "Timeout",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                else
                    MessageBox.Show(this, $"실패 rc={rc}", "Fail",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Exception",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnPickUpNiddleMove.Enabled = true;
            }

            
        }
    }
}