using System;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using QMC.LCP_280.Process.Component; // DIO / teaching controls

namespace QMC.LCP_280.Process.Unit.FormWork
{
    /// <summary>
    /// 공정 메인 Working Form
    ///  - TeachingPositionControl : IndexLoadAligner / IndexUnloadAligner / IndexChipProber / IndexChipProbeController / Rotary 등록
    ///  - DIO 제어 : 각 Unit 의 Vacuum / Clamp / Expander / Probe 관련 On/Off 메서드 패턴 자동 바인딩 (Reflection 기반)
    ///  - ManualSequenceControl : 향후 시퀀스(Align / Probe / Transfer 등) 등록을 위한 자리만 확보
    /// </summary>
    public partial class Process_Working : Form
    {
        private Equipment Equipment => Equipment.Instance;

        // 대상 5개 Unit
        private IndexChipProbeController _probeControllerUnit;
        private IndexChipProber _proberUnit;
        private IndexLoadAligner _loadAlignerUnit;
        private IndexUnloadAligner _unloadAlignerUnit;
        private Rotary _rotaryUnit;

        private bool _initialized;

        #region Constructors
        // 기본 생성자 (FormManager / 리플렉션 생성 대응)
        public Process_Working() : this(
            TryGetUnit<IndexChipProbeController>("IndexChipProbeController"),
            TryGetUnit<IndexChipProber>("IndexChipProber"),
            TryGetUnit<IndexLoadAligner>("IndexLoadAligner"),
            TryGetUnit<IndexUnloadAligner>("IndexUnloadAligner"),
            TryGetUnit<Rotary>("Rotary"))
        { }

        // 의존성 주입
        public Process_Working(IndexChipProbeController probeController,
                               IndexChipProber prober,
                               IndexLoadAligner loadAligner,
                               IndexUnloadAligner unloadAligner,
                               Rotary rotary)
        {
            InitializeComponent();
            _probeControllerUnit = probeController;
            _proberUnit = prober;
            _loadAlignerUnit = loadAligner;
            _unloadAlignerUnit = unloadAligner;
            _rotaryUnit = rotary;

            Load += Process_Working_Load;
            FormClosing += Process_Working_FormClosing;
        }
        #endregion

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

        private void Process_Working_Load(object sender, EventArgs e)
        {
            if (_initialized) return;
            _initialized = true;
            Text = "Process Working";

            BindTeachingPositions();
            BindDioControls();
            InitSequences();
        }

        #region Teaching Positions
        private void BindTeachingPositions()
        {
            try
            {
                if (teachingPositionControl == null) return;
                teachingPositionControl.ClearUnits();

                // 로드 얼라이너
                if (_loadAlignerUnit != null)
                {
                    teachingPositionControl.RegisterUnit(
                        "IndexLoadAligner",
                        _loadAlignerUnit,
                        () => _loadAlignerUnit.IndexLoadAlignerConfig?.TeachingPositions,
                        (name, vel) => _loadAlignerUnit.MoveToTeachingPosition(name, vel: vel),
                        tp => _loadAlignerUnit.IndexLoadAlignerConfig?.SetTeachingPosition(tp),
                        autoReload: false);
                }

                // 언로드 얼라이너
                if (_unloadAlignerUnit != null)
                {
                    teachingPositionControl.RegisterUnit(
                        "IndexUnloadAligner",
                        _unloadAlignerUnit,
                        () => _unloadAlignerUnit.IndexUnloadAlignerConfig?.TeachingPositions,
                        (name, vel) => _unloadAlignerUnit.MoveToTeachingPosition(name, vel: vel),
                        tp => _unloadAlignerUnit.IndexUnloadAlignerConfig?.SetTeachingPosition(tp),
                        autoReload: false);
                }

                // 프로버 (Chip Z 등)
                if (_proberUnit != null)
                {
                    teachingPositionControl.RegisterUnit(
                        "IndexChipProber",
                        _proberUnit,
                        () => _proberUnit.IndexChipProberConfig?.TeachingPositions,
                        (name, vel) => _proberUnit.MoveToTeachingPosition(name, vel: vel),
                        tp => _proberUnit.IndexChipProberConfig?.SetTeachingPosition(tp),
                        autoReload: false);
                }

                // Probe Controller (보정/Offset 위치 등)
                if (_probeControllerUnit != null)
                {
                    teachingPositionControl.RegisterUnit(
                        "IndexChipProbeController",
                        _probeControllerUnit,
                        () => _probeControllerUnit.IndexChipProbeControllerConfig?.TeachingPositions,
                        (name, vel) => _probeControllerUnit.MoveToTeachingPosition(name, vel: vel),
                        tp => _probeControllerUnit.IndexChipProbeControllerConfig?.SetTeachingPosition(tp),
                        autoReload: false);
                }

                // 로터리 (T 축 위치)
                if (_rotaryUnit != null)
                {
                    teachingPositionControl.RegisterUnit(
                        "Rotary",
                        _rotaryUnit,
                        () => _rotaryUnit.RotaryConfig?.TeachingPositions,
                        (name, vel) => _rotaryUnit.MoveToTeachingPosition(name, vel: vel),
                        tp => _rotaryUnit.RotaryConfig?.SetTeachingPosition(tp),
                        autoReload: false);
                }

                teachingPositionControl.SetSaveCancelVisible(false, false);
                teachingPositionControl.RefreshData();
            }
            catch { }
        }
        #endregion

        #region DIO Binding / Reflection Helpers
        private void BindDioControls()
        {
            if (dioControl == null) return;

            try
            {
                // Load / Unload aligner : Clamp / Vacuum / Expander 패턴
                BindCommonActuators(_loadAlignerUnit, prefix: "LoadAlign");
                BindCommonActuators(_unloadAlignerUnit, prefix: "UnloadAlign");
                // Prober / ProbeController : Chuck Vacuum, Probe Down/Up 등 메서드 패턴 매칭
                BindProberActuators(_proberUnit, "Prober");
                BindProberActuators(_probeControllerUnit, "ProbeCtrl");
                // Rotary: Slot Vacuum/Blow/Vent (명시적 메서드들 SetSlotVac/Blow/Vent, AllVacOff 등)
                BindRotaryActuators(_rotaryUnit);
            }
            catch { }
        }

        private void BindCommonActuators(object unit, string prefix)
        {
            if (unit == null) return;
            // Vacuum
            TryBindToggle(unit, "VacuumOn", "VacuumOff", "IsVacuum", prefix + "_Vacuum");
            // Clamp (ClampLiftUp/Down or ClampOn/Off)
            if (!TryBindToggle(unit, "ClampLiftUp", "ClampLiftDown", "IsClamp", prefix + "_ClampLift"))
                TryBindToggle(unit, "ClampOn", "ClampOff", "IsClamp", prefix + "_Clamp");
            // Expander
            TryBindToggle(unit, "ExpanderUp", "ExpanderDown", "IsExpanderUp", prefix + "_Expander");
        }

        private void BindProberActuators(object unit, string prefix)
        {
            if (unit == null) return;
            // Chuck Vacuum
            TryBindToggle(unit, "ChuckVacuumOn", "ChuckVacuumOff", "IsChuckVacuum", prefix + "_ChuckVac");
            // Probe Up/Down
            TryBindToggle(unit, "ProbeDown", "ProbeUp", "IsProbeDown", prefix + "_Probe");
        }

        private void BindRotaryActuators(Rotary rotary)
        {
            if (rotary == null) return;
            // 8 Slot Vacuum / Blow / Vent ON/OFF 개별 버튼
            for (int i = 0; i < 8; i++)
            {
                int idx = i;
                dioControl.BindDIOOutput(
                    () => rotary.SetSlotVac(idx, true),
                    () => rotary.SetSlotVac(idx, false),
                    $"Rotary Slot{idx + 1} VAC ON/OFF", () => false, $"Rot_S{idx + 1}_Vac");
                dioControl.BindDIOOutput(
                    () => rotary.SetSlotBlow(idx, true),
                    () => rotary.SetSlotBlow(idx, false),
                    $"Rotary Slot{idx + 1} BLOW ON/OFF", () => false, $"Rot_S{idx + 1}_Blow");
                dioControl.BindDIOOutput(
                    () => rotary.SetSlotVent(idx, true),
                    () => rotary.SetSlotVent(idx, false),
                    $"Rotary Slot{idx + 1} VENT ON/OFF", () => false, $"Rot_S{idx + 1}_Vent");
            }
            // ALL OFF 버튼들
            dioControl.BindDIOOutput(() => rotary.AllVacOff(), () => rotary.AllVacOff(), "Rotary All VAC OFF", () => false, "Rot_AllVacOff");
            dioControl.BindDIOOutput(() => rotary.AllBlowOff(), () => rotary.AllBlowOff(), "Rotary All BLOW OFF", () => false, "Rot_AllBlowOff");
            dioControl.BindDIOOutput(() => rotary.AllVentOff(), () => rotary.AllVentOff(), "Rotary All VENT OFF", () => false, "Rot_AllVentOff");
        }

        private bool TryBindToggle(object unit, string onMethod, string offMethod, string stateMethod, string key)
        {
            if (unit == null) return false;
            var t = unit.GetType();
            var on = t.GetMethod(onMethod, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            var off = t.GetMethod(offMethod, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            var st = t.GetMethod(stateMethod, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (on == null || off == null) return false;

            dioControl.BindDIOOutput(
                () => { try { on.Invoke(unit, null); } catch { } },
                () => { try { off.Invoke(unit, null); } catch { } },
                key.Replace('_', ' '),
                () => {
                    try { if (st != null) { var v = st.Invoke(unit, null); if (v is bool b) return b; } } catch { }
                    return false;
                },
                key);
            return true;
        }
        #endregion

        #region Manual Sequence Placeholder
        private void InitSequences()
        {
            try
            {
                if (manualSequenceControl == null) return;
                manualSequenceControl.ClearSequences();
                // 추후: Align / Probe / Transfer / Rotate 등 시퀀스 클래스 구현 후 여기서 RegisterSequence 호출
            }
            catch { }
        }
        #endregion

        private void Process_Working_FormClosing(object sender, FormClosingEventArgs e)
        {
            // 필요시 실행중 수동 시퀀스 Stop 처리
        }
    }
}
