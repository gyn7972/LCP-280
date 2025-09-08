using System;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using System.Threading.Tasks;
using QMC.LCP_280.Process.Component; // DIO / teaching controls
using static QMC.LCP_280.Process.Unit.RotaryConfig.IO;

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

        private IndexChipProbeController _probeControllerUnit;
        private IndexChipProber _proberUnit;
        private IndexLoadAligner _loadAlignerUnit;
        private IndexUnloadAligner _unloadAlignerUnit;
        private Rotary _rotaryUnit;

        private bool _initialized;
        private bool _deferredInitDone; // 지연 바인딩 여부
        private bool _preloadRequested;

        #region Constructors
        public Process_Working() : this(
            TryGetUnit<IndexChipProbeController>("IndexChipProbeController"),
            TryGetUnit<IndexChipProber>("IndexChipProber"),
            TryGetUnit<IndexLoadAligner>("IndexLoadAligner"),
            TryGetUnit<IndexUnloadAligner>("IndexUnloadAligner"),
            TryGetUnit<Rotary>("Rotary"))
        { }

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

        public void PreloadUI()
        {
            if (IsDisposed || Disposing) return;
            if (_preloadRequested) return;
            _preloadRequested = true;
            EnsureInitialized();
            var h = Handle;
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

        private void Process_Working_Load(object sender, EventArgs e)
        {
            EnsureInitialized();
        }

        private void EnsureInitialized()
        {
            if (_initialized) return;
            _initialized = true;
            try
            {
                Text = "Process Working";
                BeginInvoke(new Action(StartDeferredInit));
            }
            catch (Exception ex)
            {
                try { Controls.Add(new Label { Dock = DockStyle.Fill, Text = $"Init 실패: {ex.Message}", ForeColor = System.Drawing.Color.Red, TextAlign = System.Drawing.ContentAlignment.MiddleCenter }); } catch { }
            }
        }

        private async void StartDeferredInit()
        {
            if (_deferredInitDone) return;
            _deferredInitDone = true;
            await Task.Delay(30);
            if (IsDisposed || Disposing) return;
            try
            {
                BindTeachingPositions();
                BindDioControls();
                InitSequences();
            }
            catch { }
        }

        #region Teaching Positions
        private void BindTeachingPositions()
        {
            try
            {
                if (teachingPositionControl == null) return;
                teachingPositionControl.ClearUnits();

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

            dioControl.IoSortMode = QMC.LCP_280.Process.Component.DIOControl.SortingMode.Insertion;

            try
            {
                BindRotaryActuators(_rotaryUnit);

                if (_probeControllerUnit != null)
                {
                    dioControl.BindDIOInput(() => false, "---- ProbeController ----", "SEP_ProbeCtrl");

                    dioControl.BindDIOInput(() => _probeControllerUnit.ProbeVacOk(), "ProbeVac OK", "ProbeVacOk");
                    dioControl.BindDIOInput(() => _probeControllerUnit.IsSphereForward(), "Sphere FW Sns", "ProbeSphereFwSns");
                    dioControl.BindDIOInput(() => _probeControllerUnit.IsSphereBackward(), "Sphere BW Sns", "ProbeSphereBwSns");

                    dioControl.BindDIOOutput(
                        () => _probeControllerUnit.SetProbeVacValve(true),
                        () => _probeControllerUnit.SetProbeVacValve(false),
                        "ProbeVac Valve",
                        () => _probeControllerUnit.IsProbeVacValveOn(),
                        "ProbeVac");
                    dioControl.BindDIOOutput(
                        () => _probeControllerUnit.SetSphereFwdValve(true),
                        () => _probeControllerUnit.SetSphereFwdValve(false),
                        "Sphere FWD Valve",
                        () => _probeControllerUnit.IsSphereFwdValveOn(),
                        "ProbeSphereFwd");
                    dioControl.BindDIOOutput(
                        () => _probeControllerUnit.SetSphereBwdValve(true),
                        () => _probeControllerUnit.SetSphereBwdValve(false),
                        "Sphere BWD Valve",
                        () => _probeControllerUnit.IsSphereBwdValveOn(),
                        "ProbeSphereBwd");
                }

                dioControl.RebuildLists();
            }
            catch { }
        }

        private void BindRotaryActuators(Rotary rotary)
        {
            if (_rotaryUnit != null)
            {
                dioControl.BindDIOInput(() => false, "---- Rotary ----", "SEP_Rotary");
                dioControl.BindDIOInput(() => _rotaryUnit.AirTankPressureOk(), "Rot AirTank OK", "Rot_AirTk");
                dioControl.BindDIOInput(() => _rotaryUnit.VacTankPressureOk(), "Rot VacTank OK", "Rot_VacTk");

                int slotCount = SLOT_VAC.Length; // 8
                for (int slot = 0; slot < slotCount; slot++)
                {
                    int s = slot;
                    dioControl.BindDIOInput(
                        () => _rotaryUnit.SlotFlowOk(s),
                        $"Rot Slot{s + 1} FLOW",
                        $"Rot_S{s + 1}_Flow");

                    dioControl.BindDIOOutput(
                        () => _rotaryUnit.SetSlotVac(s, true),
                        () => _rotaryUnit.SetSlotVac(s, false),
                        $"Rot Slot{s + 1} VAC",
                        () => _rotaryUnit.IsSlotVacOn(s),
                        $"Rot_S{s + 1}_Vac");

                    dioControl.BindDIOOutput(
                        () => _rotaryUnit.SetSlotBlow(s, true),
                        () => _rotaryUnit.SetSlotBlow(s, false),
                        $"Rot Slot{s + 1} BLOW",
                        () => _rotaryUnit.IsSlotBlowOn(s),
                        $"Rot_S{s + 1}_Blow");

                    dioControl.BindDIOOutput(
                        () => _rotaryUnit.SetSlotVent(s, true),
                        () => _rotaryUnit.SetSlotVent(s, false),
                        $"Rot Slot{s + 1} VENT",
                        () => _rotaryUnit.IsSlotVentOn(s),
                        $"Rot_S{s + 1}_Vent");
                }

                dioControl.BindDIOOutput(
                    () => _rotaryUnit.AllVacOff(),
                    () => _rotaryUnit.AllVacOff(),
                    "Rot All VAC OFF",
                    () => false,
                    "Rot_AllVacOff");
                dioControl.BindDIOOutput(
                    () => _rotaryUnit.AllBlowOff(),
                    () => _rotaryUnit.AllBlowOff(),
                    "Rot All BLOW OFF",
                    () => false,
                    "Rot_AllBlowOff");
                dioControl.BindDIOOutput(
                    () => _rotaryUnit.AllVentOff(),
                    () => _rotaryUnit.AllVentOff(),
                    "Rot All VENT OFF",
                    () => false,
                    "Rot_AllVentOff");
            }
        }
        #endregion

        #region Manual Sequence Placeholder
        private void InitSequences()
        {
            try
            {
                if (manualSequenceControl == null) return;
                manualSequenceControl.ClearSequences();
            }
            catch { }
        }
        #endregion

        private void Process_Working_FormClosing(object sender, FormClosingEventArgs e)
        {
        }
    }
}
