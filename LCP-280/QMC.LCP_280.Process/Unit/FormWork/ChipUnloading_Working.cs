using QMC.Common.Cameras;
using QMC.LCP_280.Process.Component;
using System;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace QMC.LCP_280.Process.Unit.FormWork
{
    /// <summary>
    /// ChipUnloading Working Form
    /// - TeachingPositionControl: OutputStage, OutputDieTransfer 등록
    /// - DIO 제어: OutputStage (Vacuum/Clamp/Expander 등 - InputStage와 동일 네이밍 가정), OutputDieTransfer Arm Vacuum/Blow/Vent
    /// - 카메라 뷰 / 향후 수동 시퀀스 등록 구조 준비
    /// </summary>
    public partial class ChipUnloading_Working : Form
    {
        private const string WORK_NAME = "ChipUnloader";
        private Equipment Equipment => Equipment.Instance;

        // (필요 시 구현될 Output 전용 시퀀스 자리 - 현재는 미구현 가정)
        // private SeqOutputStage _seqOutputStage;
        // private SeqOutputDieTransferPick _seqDiePick;
        // private SeqOutputDieTransferPlace _seqDiePlace;

        private OutputStage OutputStageUnit { get; set; }
        private OutputDieTransfer OutputDieTransferUnit { get; set; }
        private Rotary RotaryUnit { get; set; }

        private bool _initialized;
        private bool _isLayoutEditMode;

        // 기본 생성자 (FormManager 자동 생성 대응)
        public ChipUnloading_Working() : this(
            TryGetUnit<OutputStage>("OutputStage"),
            TryGetUnit<OutputDieTransfer>("OutputDieTransfer"),
            TryGetUnit<Rotary>("Rotary"))
        {
        }

        // 의존성 주입 생성자
        public ChipUnloading_Working(OutputStage outputStage, OutputDieTransfer outputDieTransfer, Rotary rotaty)
        {
            InitializeComponent();
            OutputStageUnit = outputStage;
            OutputDieTransferUnit = outputDieTransfer;
            RotaryUnit = rotaty;

            Load += ChipUnloading_Working_Load;
            FormClosing += ChipUnloading_Working_FormClosing;
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

        private void ChipUnloading_Working_Load(object sender, EventArgs e)
        {
            if (_initialized) return;
            _initialized = true;

            Text = $"{WORK_NAME} Working";

            BindTeachingPositions();
            BindDioControls();
            BindCamera();
            InitSequences(); // (시퀀스 존재 시 추가)
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
            catch
            {
                try { }
                catch { }
            }
        }
        #endregion

        #region DIO Binding
        private void BindDioControls()
        {
            try
            {
                if (dioControl == null) return;

                // --- OutputStage 강타입 IO 바인딩 ---
                StrongBindOutputStage();

                // OutputDieTransfer Arm Vacuum/Blow/Vent
                if (OutputDieTransferUnit != null)
                {
                    for (int arm = 0; arm < 4; arm++)
                    {
                        int idx = arm;
                        dioControl.BindDIOOutput(
                            () => OutputDieTransferUnit.SetArmVac(idx, true),
                            () => OutputDieTransferUnit.SetArmVac(idx, false),
                            $"ODT Arm{idx + 1} VAC ON/OFF",
                            () => false,
                            $"ODT_Arm{idx + 1}_Vac");

                        dioControl.BindDIOOutput(
                            () => OutputDieTransferUnit.SetArmBlow(idx, true),
                            () => OutputDieTransferUnit.SetArmBlow(idx, false),
                            $"ODT Arm{idx + 1} BLOW ON/OFF",
                            () => false,
                            $"ODT_Arm{idx + 1}_Blow");

                        dioControl.BindDIOOutput(
                            () => OutputDieTransferUnit.SetArmVent(idx, true),
                            () => OutputDieTransferUnit.SetArmVent(idx, false),
                            $"ODT Arm{idx + 1} VENT ON/OFF",
                            () => false,
                            $"ODT_Arm{idx + 1}_Vent");
                    }

                    // All OFF
                    dioControl.BindDIOOutput(
                        () => { OutputDieTransferUnit.AllVacOff(); },
                        () => { OutputDieTransferUnit.AllVacOff(); },
                        "ODT All VAC OFF",
                        () => false,
                        "ODT_AllVacOff");
                    dioControl.BindDIOOutput(
                        () => { OutputDieTransferUnit.AllBlowOff(); },
                        () => { OutputDieTransferUnit.AllBlowOff(); },
                        "ODT All BLOW OFF",
                        () => false,
                        "ODT_AllBlowOff");
                    dioControl.BindDIOOutput(
                        () => { OutputDieTransferUnit.AllVentOff(); },
                        () => { OutputDieTransferUnit.AllVentOff(); },
                        "ODT All VENT OFF",
                        () => false,
                        "ODT_AllVentOff");
                }

                // Rotary Air / Vacuum (reflection 안전 처리)
                if (RotaryUnit != null)
                {
                    bool hasAir = Has(RotaryUnit, "AirOn") && Has(RotaryUnit, "AirOff") && Has(RotaryUnit, "IsAirOn");
                    bool hasVac = Has(RotaryUnit, "VacOn") && Has(RotaryUnit, "VacOff") && Has(RotaryUnit, "IsVacOn");
                    if (hasAir)
                    {
                        dioControl.BindDIOOutput(
                            () => { Invoke(RotaryUnit, "AirOn"); },
                            () => { Invoke(RotaryUnit, "AirOff"); },
                            "Rotary Air ON/OFF",
                            () => InvokeBool(RotaryUnit, "IsAirOn"),
                            "RotaryAir");
                    }
                    if (hasVac)
                    {
                        dioControl.BindDIOOutput(
                            () => { Invoke(RotaryUnit, "VacOn"); },
                            () => { Invoke(RotaryUnit, "VacOff"); },
                            "Rotary Vacuum ON/OFF",
                            () => InvokeBool(RotaryUnit, "IsVacOn"),
                            "RotaryVac");
                    }
                }
            }
            catch { }
        }

        private void StrongBindOutputStage()
        {
            if (OutputStageUnit == null || dioControl == null) return;
            try
            {
                // Vacuum
                dioControl.BindDIOInput(() => OutputStageUnit.IsVacuum(), "Vacuum", "OutStageVacOk");
                dioControl.BindDIOOutput(
                    () => OutputStageUnit.VacuumOn(),
                    () => OutputStageUnit.VacuumOff(),
                    "Vacuum ON/OFF",
                    () => OutputStageUnit.IsVacuum(),
                    "OutStageVacCtrl");

                // Clamp Lift (physical sensor: DOWN only, UP = logical)
                dioControl.BindDIOOutput(
                    () => OutputStageUnit.ClampLiftUp(),
                    () => OutputStageUnit.ClampLiftDown(),
                    "ClampLift UP/DOWN",
                    () => OutputStageUnit.IsClampLiftUp(),
                    "OutStageClampLift");
                dioControl.BindDIOInput(() => OutputStageUnit.IsClampLiftUp(), "ClampLift UP (Logic)", "OutStageClampLiftUp");
                dioControl.BindDIOInput(() => OutputStageUnit.IsClampLiftDown(), "ClampLift DOWN Sns", "OutStageClampLiftDn");

                // Clamp Forward / Backward (physical sensor: FWD only, BWD = logical)
                dioControl.BindDIOOutput(
                    () => OutputStageUnit.ClampFwd(),
                    () => OutputStageUnit.ClampBwd(),
                    "Clamp FWD/BWD",
                    () => OutputStageUnit.IsClampFwd(),
                    "OutStageClampFB");
                dioControl.BindDIOInput(() => OutputStageUnit.IsClampFwd(), "Clamp FWD Sns", "OutStageClampFwd");
                dioControl.BindDIOInput(() => OutputStageUnit.IsClampBwd(), "Clamp BWD (Logic)", "OutStageClampBwd");

                // Expander (Plate) Up/Down
                dioControl.BindDIOOutput(
                    () => OutputStageUnit.ExpanderUp(),
                    () => OutputStageUnit.ExpanderDown(),
                    "Expander UP/DOWN",
                    () => OutputStageUnit.IsExpanderUp(),
                    "OutStageExp");
                dioControl.BindDIOInput(() => OutputStageUnit.IsExpanderUp(), "Expander UP Sns", "OutStageExpUp");
                dioControl.BindDIOInput(() => OutputStageUnit.IsExpanderDown(), "Expander DOWN Sns", "OutStageExpDn");

                // Ring Sensors
                dioControl.BindDIOInput(() => OutputStageUnit.Ring0(), "Ring Sns 0", "OutStageRing0");
                dioControl.BindDIOInput(() => OutputStageUnit.Ring1(), "Ring Sns 1", "OutStageRing1");
                dioControl.BindDIOInput(() => OutputStageUnit.IsRingPresent(), "Ring Present", "OutStageRingAny");
            }
            catch { }
        }

        private static bool Has(object obj, string method)
        {
            if (obj == null) return false;
            return obj.GetType().GetMethod(method,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) != null;
        }
        private static void Invoke(object obj, string method)
        {
            try { obj?.GetType().GetMethod(method, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.Invoke(obj, null); } catch { }
        }
        private static bool InvokeBool(object obj, string method)
        {
            try
            {
                var mi = obj?.GetType().GetMethod(method, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (mi == null) return false;
                var v = mi.Invoke(obj, null);
                if (v is bool b) return b;
            }
            catch { }
            return false;
        }
        #endregion

        #region Camera
        private void BindCamera()
        {
            // Camera viewer binding
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
            try
            {
            }
            catch { }
        }

        private void _btnVisionSetting_Click(object sender, EventArgs e)
        {
            try
            {
                PatternMatchingDialog dlg = new PatternMatchingDialog();
                dlg.ShowDialog();
            }
            catch { }
        }
    }
}
