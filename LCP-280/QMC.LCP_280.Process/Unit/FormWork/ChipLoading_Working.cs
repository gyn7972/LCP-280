using QMC.LCP_280.Process.Component; // ensure access to DIOControl / TeachingPositionControl
using QMC.LCP_280.Process.Sequences;
using System;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Threading;

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
    public partial class ChipLoader_Working : Form
    {
        private const string WORK_NAME = "ChipLoader";
        private Equipment Equipment => Equipment.Instance;

        // Units
        private InputStage InputStageUnit { get; set; }
        private InputStageEjector InputStageEjectorUnit { get; set; }
        private InputDieTransfer InputDieTransferUnit { get; set; }

        // Sequences
        private SeqInputStage SeqInputStage { get; set; }
        private SeqInputChipAlignVision _seqAlignVision;
        private SeqInputChipMappingVision _seqMappingVision;
        private SeqInputDieTransferChipUp _seqDiePick;
        private SeqInputDieTransferChipDown _seqDiePlace;

        // State
        private bool _initialized;
        private bool _preloadRequested;
        private bool _deferredInitDone; // 지연 초기화 완료 여부
        private bool _isLayoutEditMode;

        #region Constructors
        public ChipLoader_Working() : this(
            TryGetUnit<InputStage>("InputStage"),
            TryGetUnit<InputStageEjector>("InputStageEjector"),
            TryGetUnit<InputDieTransfer>("InputDieTransfer"))
        { }

        public ChipLoader_Working(InputStage inputStage, InputStageEjector ejector, InputDieTransfer dieTransfer)
        {
            InitializeComponent();
            InputStageUnit = inputStage;
            InputStageEjectorUnit = ejector;
            InputDieTransferUnit = dieTransfer;
            Load += ChipLoader_Working_Load;
            FormClosing += ChipLoader_Working_FormClosing;
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

        private void ChipLoader_Working_Load(object sender, EventArgs e) => EnsureInitialized();

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
                        () => InputStageUnit.InputStageConfig?.TeachingPositions,
                        (name, vel) => InputStageUnit.MoveToTeachingPosition(name, vel: vel),
                        tp => InputStageUnit.InputStageConfig?.SetTeachingPosition(tp),
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
                    for (int arm = 0; arm < 1; arm++) //for (int arm = 0; arm < 4; arm++)
                    {
                        int idx = arm;
                        dioControl.BindDIOOutput(
                            () => InputDieTransferUnit.SetArmVac(idx, true),
                            () => InputDieTransferUnit.SetArmVac(idx, false),
                            $"IDT Arm{idx + 1} VAC ON/OFF",
                            () => false,
                            $"IDT_Arm{idx + 1}_Vac");
                        dioControl.BindDIOOutput(
                            () => InputDieTransferUnit.SetArmBlow(idx, true),
                            () => InputDieTransferUnit.SetArmBlow(idx, false),
                            $"IDT Arm{idx + 1} BLOW ON/OFF",
                            () => false,
                            $"IDT_Arm{idx + 1}_Blow");
                        dioControl.BindDIOOutput(
                            () => InputDieTransferUnit.SetArmVent(idx, true),
                            () => InputDieTransferUnit.SetArmVent(idx, false),
                            $"IDT Arm{idx + 1} VENT ON/OFF",
                            () => false,
                            $"IDT_Arm{idx + 1}_Vent");
                    }
                    dioControl.BindDIOOutput(() => InputDieTransferUnit.AllVacOff(), () => InputDieTransferUnit.AllVacOff(), "IDT All VAC OFF", () => false, "IDT_AllVacOff");
                    dioControl.BindDIOOutput(() => InputDieTransferUnit.AllBlowOff(), () => InputDieTransferUnit.AllBlowOff(), "IDT All BLOW OFF", () => false, "IDT_AllBlowOff");
                    dioControl.BindDIOOutput(() => InputDieTransferUnit.AllVentOff(), () => InputDieTransferUnit.AllVentOff(), "IDT All VENT OFF", () => false, "IDT_AllVentOff");
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
                dioControl.BindDIOInput(() => InputStageUnit.IsVacuum(), "Vacuum OK(Sns)", "InStageVacOk");
                dioControl.BindDIOInput(() => InputStageUnit.IsPlateUp(), "Plate UP Sns", "InStagePlateUp");
                dioControl.BindDIOInput(() => InputStageUnit.IsPlateDown(), "Plate DOWN Sns", "InStagePlateDn");
                dioControl.BindDIOInput(() => InputStageUnit.IsClampLiftDown(), "ClampLift DOWN Sns", "InStageClampDn");
                dioControl.BindDIOInput(() => InputStageUnit.IsClampFwd(), "Clamp FWD Sns", "InStageClampFwd");
                dioControl.BindDIOInput(() => InputStageUnit.Ring0(), "Ring Sns 0", "InStageRing0");
                dioControl.BindDIOInput(() => InputStageUnit.Ring1(), "Ring Sns 1", "InStageRing1");
                dioControl.BindDIOInput(() => InputStageUnit.IsRingPresent(), "Ring Any", "InStageRingAny");

                dioControl.BindDIOOutput(
                    () => InputStageUnit.SetVacuumValve(true),
                    () => InputStageUnit.SetVacuumValve(false),
                    "Vacuum",
                    () => InputStageUnit.IsVacuumValveOn(),
                    "InStageVac");

                dioControl.BindDIOOutput(
                    () => InputStageUnit.SetPlateUp(true),
                    () => InputStageUnit.SetPlateUp(false),
                    "PlateUP",
                    () => InputStageUnit.IsPlateUpOn(),
                    "InStagePlateUp");
                dioControl.BindDIOOutput(
                    () => InputStageUnit.SetPlateDown(true),
                    () => InputStageUnit.SetPlateDown(false),
                    "PlateDOWN",
                    () => InputStageUnit.IsPlateDownOn(),
                    "InStagePlateDn");
                //dioControl.BindDIOOutput(
                //    () => InputStageUnit.SetClampPlate(true),
                //    () => InputStageUnit.SetClampPlate(false),
                //    "PlateUpDn",
                //    () => InputStageUnit.IsClampFwd(),
                //    "InStagePlateUpDn");

                dioControl.BindDIOOutput(
                    () => InputStageUnit.SetClampLiftUpValve(true),
                    () => InputStageUnit.SetClampLiftUpValve(false),
                    "ClampUP",
                    () => InputStageUnit.IsClampLiftUpValveOn(),
                    "InStageClampLiftUp");

                dioControl.BindDIOOutput(
                    () => InputStageUnit.SetClampLiftDownValve(true),
                    () => InputStageUnit.SetClampLiftDownValve(false),
                    "ClampDOWN",
                    () => InputStageUnit.IsClampLiftDownValveOn(),
                    "InStageClampLiftDn");

                dioControl.BindDIOOutput(
                    () => InputStageUnit.SetClampFwdValve(true),
                    () => InputStageUnit.SetClampFwdValve(false),
                    "ClampFWD",
                    () => InputStageUnit.IsClampFwdValveOn(),
                    "InStageClampFwd");

                dioControl.BindDIOOutput(
                    () => InputStageUnit.SetClampBwdValve(true),
                    () => InputStageUnit.SetClampBwdValve(false),
                    "ClampBWD",
                    () => InputStageUnit.IsClampBwdValveOn(),
                    "InStageClampBwd");

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
                    if (SeqInputStage == null)
                        // 기존: new SeqInputStage(InputStageUnit) -> Ejector 포함 생성자로 변경
                        SeqInputStage = new SeqInputStage(InputStageUnit, InputStageEjectorUnit);
                    manualSequenceControl?.RegisterSequence(
                        "InputStage",
                        SeqInputStage,
                        () => Enum.GetNames(typeof(SeqInputStage.Step)),
                        step => SeqInputStage.StartSingle(step),
                        idx => Enum.GetName(typeof(SeqInputStage.Step), idx),
                        autoSelect: true);
                }
                // Align Vision
                if (InputStageUnit != null)
                {
                    //if (_seqAlignVision == null)
                    //    _seqAlignVision = new SeqInputChipAlignVision(InputStageUnit); // Unit 재사용
                    //manualSequenceControl?.RegisterSequence(
                    //    "AlignVision",
                    //    _seqAlignVision,
                    //    () => SeqInputChipAlignVision.GetStepNames(),
                    //    step => _seqAlignVision.StartSingle(step),
                    //    idx => Enum.GetName(typeof(SeqInputChipAlignVision.Step), idx));
                }
                // Mapping Vision
                if (InputStageUnit != null)
                {
                    if (_seqMappingVision == null)
                        _seqMappingVision = new SeqInputChipMappingVision(InputStageUnit); // Unit 재사용
                    manualSequenceControl?.RegisterSequence(
                        "MappingVision",
                        _seqMappingVision,
                        () => SeqInputChipMappingVision.GetStepNames(),
                        step => _seqMappingVision.StartSingle(step),
                        idx => Enum.GetName(typeof(SeqInputChipMappingVision.Step), idx));
                }
                // Pick Sequence
                if (InputDieTransferUnit != null)
                {
                    if (_seqDiePick == null)
                        _seqDiePick = new SeqInputDieTransferChipUp(InputDieTransferUnit, 0, InputStageUnit, InputStageEjectorUnit); // 등록된 Unit 전달
                    manualSequenceControl?.RegisterSequence(
                        "DiePick",
                        _seqDiePick,
                        () => Enum.GetNames(typeof(SeqInputDieTransferChipUp.Step)),
                        step => { try { return _seqDiePick.Start(); } catch { return false; } },
                        idx => Enum.GetName(typeof(SeqInputDieTransferChipUp.Step), idx));
                }
                // Place Sequence
                if (InputDieTransferUnit != null)
                {
                    if (_seqDiePlace == null)
                        _seqDiePlace = new SeqInputDieTransferChipDown(InputDieTransferUnit); // 등록된 Unit 전달
                    manualSequenceControl?.RegisterSequence(
                        "DiePlace",
                        _seqDiePlace,
                        () => Enum.GetNames(typeof(SeqInputDieTransferChipDown.Step)),
                        step => { try { return _seqDiePlace.Start(); } catch { return false; } },
                        idx => Enum.GetName(typeof(SeqInputDieTransferChipDown.Step), idx));
                }
            }
            catch { }
        }
        #endregion

        #region Events
        private void ChipLoader_Working_FormClosing(object sender, FormClosingEventArgs e)
        {
            try { SeqInputStage?.Stop(); } catch { }
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
                if (ejector?.AxisPinZ == null || transfer?.PickZ == null)
                {
                    MessageBox.Show(this, "축 준비가 되지 않았습니다.", "Axis Not Ready", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Teaching Position 값 가져오기
                double pinWaiting = 0, pinOffset = 0;
                try
                {
                    var posWaiting = ejector.InputStageEjectorConfig?.GetPositionWithOffset(InputStageEjectorConfig.TeachingPositionName.EjectPinWaiting.ToString());
                    var posOffset  = ejector.InputStageEjectorConfig?.GetPositionWithOffset(InputStageEjectorConfig.TeachingPositionName.EjectPinOffset.ToString());
                    if (posWaiting.HasValue) pinWaiting = posWaiting.Value.pinZ;
                    if (posOffset.HasValue)  pinOffset  = posOffset.Value.pinZ;
                }
                catch { }

                double curPinZ  = ejector.AxisPinZ.GetPosition();
                double curPickZ = transfer.PickZ.GetPosition();

                // 1단계: PinZ Waiting 이동 필요 여부
                bool needMoveToWaiting = Math.Abs(curPinZ - pinWaiting) > (ejector.AxisPinZ.Config?.InposTolerance ?? 0.005) * 2;

                // 공통 속도 (두 축의 80%)
                double vPin  = ejector.AxisPinZ.Config?.MaxVelocity  ?? 10;
                double vPick = transfer.PickZ.Config?.MaxVelocity   ?? 10;
                double commonVel = Math.Min(vPin, vPick) * 0.8; if (commonVel <= 0) commonVel = Math.Min(vPin, vPick);
                double accPin  = ejector.AxisPinZ.Config?.RunAcc  ?? 10; double decPin  = ejector.AxisPinZ.Config?.RunDec  ?? accPin; double jerkPin  = ejector.AxisPinZ.Config?.AccJerkPercent  ?? 50;
                double accPick = transfer.PickZ.Config?.RunAcc    ?? 10; double decPick = transfer.PickZ.Config?.RunDec    ?? accPick; double jerkPick = transfer.PickZ.Config?.AccJerkPercent    ?? 50;

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
                curPickZ = transfer.PickZ.GetPosition();

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
                    transfer.PickZ.MoveAbs(finalPickZ,  commonVel, accPick, decPick, jerkPick);
                    ejector.AxisPinZ.MoveAbs(finalPinZ, commonVel, accPin,  decPin,  jerkPin);
                    transfer.PickZ.WaitMoveDone(-1);
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
            InputStageUnit.SetClampUpDown(true);
        }

        private void buttonTest2_Click(object sender, EventArgs e)
        {
            InputStageUnit.SetClampUpDown(false);
        }
    }
}