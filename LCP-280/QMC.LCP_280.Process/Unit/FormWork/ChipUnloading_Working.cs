using QMC.Common.Cameras;
using QMC.LCP_280.Process.Component;
using System;
using System.Windows.Forms;
using System.Threading.Tasks;
using static QMC.LCP_280.Process.Unit.OutputDieTransferConfig.IO; // ODT IO 상수
using static QMC.LCP_280.Process.Unit.RotaryConfig.IO;             // Rotary IO 상수/배열

namespace QMC.LCP_280.Process.Unit.FormWork
{
    /// <summary>
    /// ChipUnloading Working Form
    /// - TeachingPositionControl: OutputStage, OutputDieTransfer, Rotary
    /// - DIO 제어:
    ///    OutputStage : 센서 입력 + 밸브 강제 제어
    ///    OutputDieTransfer : Arm Vac/Blow/Vent 제어
    ///    Rotary : Slot Flow 입력 + Slot Vac/Blow/Vent 강제 제어
    /// </summary>
    public partial class ChipUnloading_Working : Form
    {
        private const string WORK_NAME = "ChipUnloader";
        private Equipment Equipment => Equipment.Instance;

        private OutputStage OutputStageUnit { get; set; }
        private OutputDieTransfer OutputDieTransferUnit { get; set; }
        private Rotary RotaryUnit { get; set; }

        private bool _initialized;          // Text/핸들 설정 여부
        private bool _preloadRequested;     // Preload 1회 보장
        private bool _deferredInitDone;     // 무거운 바인딩 지연 수행 여부
        private bool _isLayoutEditMode;

        public ChipUnloading_Working() : this(
            TryGetUnit<OutputStage>("OutputStage"),
            TryGetUnit<OutputDieTransfer>("OutputDieTransfer"),
            TryGetUnit<Rotary>("Rotary"))
        {
        }

        public ChipUnloading_Working(OutputStage outputStage, OutputDieTransfer outputDieTransfer, Rotary rotaty)
        {
            InitializeComponent();
            OutputStageUnit = outputStage;
            OutputDieTransferUnit = outputDieTransfer;
            RotaryUnit = rotaty;

            Load += ChipUnloading_Working_Load;
            FormClosing += ChipUnloading_Working_FormClosing;
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
                Text = $"{WORK_NAME} Working";
                BeginInvoke(new Action(StartDeferredInit)); // 무거운 초기화 지연
            }
            catch (Exception ex)
            {
                try { this.Controls.Add(new Label { Dock = DockStyle.Fill, Text = $"Init 실패: {ex.Message}", ForeColor = System.Drawing.Color.Red, TextAlign = System.Drawing.ContentAlignment.MiddleCenter }); } catch { }
            }
        }

        private async void StartDeferredInit()
        {
            if (_deferredInitDone) return;
            _deferredInitDone = true;
            await Task.Delay(30); // 첫 Paint 후 실행
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

        #region Teaching Position Control
        private void BindTeachingPositions()
        {
            try
            {
                if (teachingPositionControl == null) return;

                teachingPositionControl.ClearUnits();

                if (OutputStageUnit != null)
                {
                    teachingPositionControl.RegisterUnit(
                        "OutputStage",
                        OutputStageUnit,
                        () => OutputStageUnit.OutputStageConfig?.TeachingPositions,
                        (name, vel) => OutputStageUnit.MoveToTeachingPosition(name, vel: vel),
                        tp => OutputStageUnit.OutputStageConfig?.SetTeachingPosition(tp),
                        autoReload: false);
                }

                if (OutputDieTransferUnit != null)
                {
                    teachingPositionControl.RegisterUnit(
                        "OutputDieTransfer",
                        OutputDieTransferUnit,
                        () => OutputDieTransferUnit.OutputDieTransferConfig?.TeachingPositions,
                        (name, vel) => OutputDieTransferUnit.MoveToTeachingPosition(name, vel: vel),
                        tp => OutputDieTransferUnit.OutputDieTransferConfig?.SetTeachingPosition(tp),
                        autoReload: false);
                }

                if (RotaryUnit != null)
                {
                    teachingPositionControl.RegisterUnit(
                        "Rotary",
                        RotaryUnit,
                        () => RotaryUnit.RotaryConfig?.TeachingPositions,
                        (name, vel) => RotaryUnit.MoveToTeachingPosition(name, vel: vel),
                        tp => RotaryUnit.RotaryConfig?.SetTeachingPosition(tp),
                        autoReload: false);
                }

                teachingPositionControl.SetSaveCancelVisible(false, false);
                teachingPositionControl.RefreshData();
            }
            catch { }
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
                dioControl.BindDIOInput(() => false, "---- OutputStage ----", "SEP_OutStage");
                StrongBindOutputStage();

                // 그룹 구분선: OutputDieTransfer
                if (OutputDieTransferUnit != null)
                {
                    dioControl.BindDIOInput(() => false, "---- OutputDieTransfer ----", "SEP_ODT");
                    for (int arm = 0; arm < 1; arm++)
                    {
                        int idx = arm;
                        dioControl.BindDIOOutput(
                            () => OutputDieTransferUnit.SetArmVac(idx, true),
                            () => OutputDieTransferUnit.SetArmVac(idx, false),
                            $"ODT Arm{idx + 1} VAC ON/OFF",
                            () => OutputDieTransferUnit.IsArmVacOn(idx),
                            $"ODT_Arm{idx + 1}_Vac");

                        dioControl.BindDIOOutput(
                            () => OutputDieTransferUnit.SetArmBlow(idx, true),
                            () => OutputDieTransferUnit.SetArmBlow(idx, false),
                            $"ODT Arm{idx + 1} BLOW ON/OFF",
                            () => OutputDieTransferUnit.IsArmBlowOn(idx),
                            $"ODT_Arm{idx + 1}_Blow");

                        dioControl.BindDIOOutput(
                            () => OutputDieTransferUnit.SetArmVent(idx, true),
                            () => OutputDieTransferUnit.SetArmVent(idx, false),
                            $"ODT Arm{idx + 1} VENT ON/OFF",
                            () => OutputDieTransferUnit.IsArmVentOn(idx),
                            $"ODT_Arm{idx + 1}_Vent");
                    }

                    dioControl.BindDIOOutput(
                        () => OutputDieTransferUnit.AllVacOff(),
                        () => OutputDieTransferUnit.AllVacOff(),
                        "ODT All VAC OFF",
                        () => false,
                        "ODT_AllVacOff");
                    dioControl.BindDIOOutput(
                        () => OutputDieTransferUnit.AllBlowOff(),
                        () => OutputDieTransferUnit.AllBlowOff(),
                        "ODT All BLOW OFF",
                        () => false,
                        "ODT_AllBlowOff");
                    dioControl.BindDIOOutput(
                        () => OutputDieTransferUnit.AllVentOff(),
                        () => OutputDieTransferUnit.AllVentOff(),
                        "ODT All VENT OFF",
                        () => false,
                        "ODT_AllVentOff");
                }

                // 그룹 구분선: Rotary
                if (RotaryUnit != null)
                {
                    dioControl.BindDIOInput(() => false, "---- Rotary ----", "SEP_Rotary");
                    dioControl.BindDIOInput(() => RotaryUnit.AirTankPressureOk(), "Rot AirTank OK", "Rot_AirTk");
                    dioControl.BindDIOInput(() => RotaryUnit.VacTankPressureOk(), "Rot VacTank OK", "Rot_VacTk");

                    int slotCount = SLOT_VAC.Length; // 8
                    for (int slot = 0; slot < slotCount; slot++)
                    {
                        int s = slot;
                        dioControl.BindDIOInput(
                            () => RotaryUnit.SlotFlowOk(s),
                            $"Rot Slot{s + 1} FLOW",
                            $"Rot_S{s + 1}_Flow");

                        dioControl.BindDIOOutput(
                            () => RotaryUnit.SetSlotVac(s, true),
                            () => RotaryUnit.SetSlotVac(s, false),
                            $"Rot Slot{s + 1} VAC",
                            () => RotaryUnit.IsSlotVacOn(s),
                            $"Rot_S{s + 1}_Vac");

                        dioControl.BindDIOOutput(
                            () => RotaryUnit.SetSlotBlow(s, true),
                            () => RotaryUnit.SetSlotBlow(s, false),
                            $"Rot Slot{s + 1} BLOW",
                            () => RotaryUnit.IsSlotBlowOn(s),
                            $"Rot_S{s + 1}_Blow");

                        dioControl.BindDIOOutput(
                            () => RotaryUnit.SetSlotVent(s, true),
                            () => RotaryUnit.SetSlotVent(s, false),
                            $"Rot Slot{s + 1} VENT",
                            () => RotaryUnit.IsSlotVentOn(s),
                            $"Rot_S{s + 1}_Vent");
                    }

                    dioControl.BindDIOOutput(
                        () => RotaryUnit.AllVacOff(),
                        () => RotaryUnit.AllVacOff(),
                        "Rot All VAC OFF",
                        () => false,
                        "Rot_AllVacOff");
                    dioControl.BindDIOOutput(
                        () => RotaryUnit.AllBlowOff(),
                        () => RotaryUnit.AllBlowOff(),
                        "Rot All BLOW OFF",
                        () => false,
                        "Rot_AllBlowOff");
                    dioControl.BindDIOOutput(
                        () => RotaryUnit.AllVentOff(),
                        () => RotaryUnit.AllVentOff(),
                        "Rot All VENT OFF",
                        () => false,
                        "Rot_AllVentOff");
                }

                dioControl.RebuildLists();
            }
            catch { }
        }

        private void StrongBindOutputStage()
        {
            if (OutputStageUnit == null || dioControl == null) return;
            try
            {
                dioControl.BindDIOInput(() => OutputStageUnit.IsVacuum(), "Vacuum OK(Sns)", "OutStageVacOk");
                dioControl.BindDIOInput(() => OutputStageUnit.IsPlateUp(), "Plate UP Sns", "OutStagePlateUp");
                dioControl.BindDIOInput(() => OutputStageUnit.IsPlateDown(), "Plate DOWN Sns", "OutStagePlateDn");
                dioControl.BindDIOInput(() => OutputStageUnit.IsClampLiftDown(), "ClampLift DOWN Sns", "OutStageLiftDn");
                dioControl.BindDIOInput(() => OutputStageUnit.IsClampFwd(), "Clamp FWD Sns", "OutStageClampFwd");
                dioControl.BindDIOInput(() => OutputStageUnit.Ring0(), "Ring Sns 0", "OutStageRing0");
                dioControl.BindDIOInput(() => OutputStageUnit.Ring1(), "Ring Sns 1", "OutStageRing1");
                dioControl.BindDIOInput(() => OutputStageUnit.IsRingPresent(), "Ring Any", "OutStageRingAny");

                dioControl.BindDIOOutput(
                    () => OutputStageUnit.SetVacuumValve(true),
                    () => OutputStageUnit.SetVacuumValve(false),
                    "Vacuum Valve",
                    () => OutputStageUnit.IsVacuumValveOn(),
                    "OutStageVacValve");

                dioControl.BindDIOOutput(
                    () => OutputStageUnit.SetPlateUpValve(true),
                    () => OutputStageUnit.SetPlateUpValve(false),
                    "Plate UP Valve",
                    () => OutputStageUnit.IsPlateUpValveOn(),
                    "OutStagePlateUp");

                dioControl.BindDIOOutput(
                    () => OutputStageUnit.SetPlateDownValve(true),
                    () => OutputStageUnit.SetPlateDownValve(false),
                    "Plate DOWN Valve",
                    () => OutputStageUnit.IsPlateDownValveOn(),
                    "OutStagePlateDown");

                dioControl.BindDIOOutput(
                    () => OutputStageUnit.SetClampLiftUpValve(true),
                    () => OutputStageUnit.SetClampLiftUpValve(false),
                    "ClampLift UP Valve",
                    () => OutputStageUnit.IsClampLiftUpValveOn(),
                    "OutStageLiftUp");

                dioControl.BindDIOOutput(
                    () => OutputStageUnit.SetClampLiftDownValve(true),
                    () => OutputStageUnit.SetClampLiftDownValve(false),
                    "ClampLift DOWN Valve",
                    () => OutputStageUnit.IsClampLiftDownValveOn(),
                    "OutStageLiftDown");

                dioControl.BindDIOOutput(
                    () => OutputStageUnit.SetClampFwdValve(true),
                    () => OutputStageUnit.SetClampFwdValve(false),
                    "Clamp FWD Valve",
                    () => OutputStageUnit.IsClampFwdValveOn(),
                    "OutStageClampFwd");

                dioControl.BindDIOOutput(
                    () => OutputStageUnit.SetClampBwdValve(true),
                    () => OutputStageUnit.SetClampBwdValve(false),
                    "Clamp BWD Valve",
                    () => OutputStageUnit.IsClampBwdValveOn(),
                    "OutStageClampBwd");
            }
            catch { }
        }
        #endregion

        #region Camera
        private void BindCamera()
        {
            try
            {
                if (_ChipUnloadingCameraviewer != null && OutputStageUnit?.StageCamera != null)
                {
                    if (_ChipUnloadingCameraviewer.Camera != OutputStageUnit.StageCamera)
                        _ChipUnloadingCameraviewer.Camera = OutputStageUnit.StageCamera;
                    try { OutputStageUnit.StageCamera.StartLive(); } catch { }
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
                if (manualSequenceControl != null)
                    manualSequenceControl.ClearSequences();
            }
            catch { }
        }
        #endregion

        private void ChipUnloading_Working_FormClosing(object sender, FormClosingEventArgs e)
        {
            try { }
            catch { }
        }

        private void _btnVisionSetting_Click(object sender, EventArgs e)
        {
            
        }
    }
}
