using QMC.LCP_280.Process.Component;
using System;
using System.Windows.Forms;
using System.Threading.Tasks;

namespace QMC.LCP_280.Process.Unit.FormWork
{
    /// <summary>
    /// WaferRing Working Form
    /// - TeachingPositionControl: InputRingTransfer, InputCassetteLifter
    /// - DIO 제어:
    ///    InputRingTransfer : Feeder Up/Down/Clamp 관련 센서 + 밸브 강제 제어
    ///    InputCassetteLifter : Cassette / RingJut / Mapping 센서 표시
    /// </summary>
    public partial class WaferRing_Working : Form
    {
        private const string WORK_NAME = "WaferRing";
        private Equipment Equipment => Equipment.Instance;

        private InputFeeder InputRingTransferUnit { get; set; }
        private InputCassetteLifter InputCassetteLifterUnit { get; set; }

        private bool _initialized;
        private bool _preloadRequested;
        private bool _deferredInitDone; // 지연 초기화 완료 여부

        public WaferRing_Working() : this(
            TryGetUnit<InputFeeder>("InputRingTransfer"),
            TryGetUnit<InputCassetteLifter>("InputCassetteLifter"))
        {
        }

        public WaferRing_Working(InputFeeder ringTransfer, InputCassetteLifter cassetteLifter)
        {
            InitializeComponent();
            InputRingTransferUnit = ringTransfer;
            InputCassetteLifterUnit = cassetteLifter;

            Load += WaferRing_Working_Load;
            FormClosing += WaferRing_Working_FormClosing;
        }

        /// <summary>
        /// 외부에서 Show 전에 UI 구성을 미리 수행 (Handle 확보)
        /// </summary>
        public void PreloadUI()
        {
            if (IsDisposed || Disposing) return;
            if (_preloadRequested) return;
            _preloadRequested = true;
            EnsureInitialized();
            var handle = Handle; // 강제 핸들 생성
        }

        private void WaferRing_Working_Load(object sender, EventArgs e)
        {
            EnsureInitialized();
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

        private void EnsureInitialized()
        {
            if (_initialized) return;
            _initialized = true;
            try
            {
                Text = $"{WORK_NAME} Working";
                BeginInvoke(new Action(StartDeferredInit)); // 무거운 바인딩 지연
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
            await Task.Delay(30); // 첫 Paint 이후 수행 유도
            if (IsDisposed || Disposing) return;
            try
            {
                BindTeachingPositions();
                BindDioControls();
                InitSequences();
            }
            catch { }
        }

        #region Teaching
        private void BindTeachingPositions()
        {
            try
            {
                if (teachingPositionControl == null) return;
                teachingPositionControl.ClearUnits();

                if (InputRingTransferUnit != null)
                {
                    teachingPositionControl.RegisterUnit(
                        "InputRingTransfer",
                        InputRingTransferUnit,
                        () => InputRingTransferUnit.InputFeederConfig?.TeachingPositions,
                        (name, vel) => InputRingTransferUnit.MoveToTeachingPosition(name, vel: vel),
                        tp => InputRingTransferUnit.InputFeederConfig?.SetTeachingPosition(tp),
                        autoReload: false);
                }

                if (InputCassetteLifterUnit != null)
                {
                    teachingPositionControl.RegisterUnit(
                        "InputCassetteLifter",
                        InputCassetteLifterUnit,
                        () => InputCassetteLifterUnit.InputCassetteLifterConfig?.TeachingPositions,
                        (name, vel) => InputCassetteLifterUnit.MoveToTeachingPosition(name, vel: vel),
                        tp => InputCassetteLifterUnit.InputCassetteLifterConfig?.SetTeachingPosition(tp),
                        autoReload: false);
                }

                teachingPositionControl.SetSaveCancelVisible(false, false);
                teachingPositionControl.RefreshData();
            }
            catch { }
        }
        #endregion

        #region DIO
        private void BindDioControls()
        {
            try
            {
                if (dioControl == null) return;
                dioControl.IoSortMode = DIOControl.SortingMode.Insertion;

                if (InputRingTransferUnit != null)
                {
                    StrongBindInputRingTransfer();
                }

                if (InputCassetteLifterUnit != null)
                {
                    dioControl.BindDIOInput(() => InputCassetteLifterUnit.IsCassettePresent0(), "Cassette Present 0", "ICL_Cass0");
                    dioControl.BindDIOInput(() => InputCassetteLifterUnit.IsCassettePresent1(), "Cassette Present 1", "ICL_Cass1");
                    dioControl.BindDIOInput(() => InputCassetteLifterUnit.IsAnyCassettePresent(), "Cassette Any", "ICL_CassAny");
                    dioControl.BindDIOInput(() => InputCassetteLifterUnit.IsWaferProtrusionDetectionSensor(), "Ring Jut", "ICL_RingJut");
                    dioControl.BindDIOInput(() => InputCassetteLifterUnit.MappingSensor(), "Mapping Sensor", "ICL_Mapping");
                }

                dioControl.RebuildLists();
            }
            catch { }
        }

        private void StrongBindInputRingTransfer()
        {
            if (InputRingTransferUnit == null || dioControl == null) return;
            try
            {
                // Sensors
                dioControl.BindDIOInput(() => InputRingTransferUnit.IsFeederUp(), "Feeder UP Sns", "IRT_FeederUp");
                dioControl.BindDIOInput(() => InputRingTransferUnit.IsFeederDown(), "Feeder DOWN Sns", "IRT_FeederDown");
                dioControl.BindDIOInput(() => InputRingTransferUnit.IsUnclamped(), "Feeder UNCLAMP Sns", "IRT_Unclamp");
                dioControl.BindDIOInput(() => InputRingTransferUnit.IsRingPresent(), "Feeder RING Sns", "IRT_Ring");
                dioControl.BindDIOInput(() => InputRingTransferUnit.IsOverload(), "Feeder OVERLOAD Sns", "IRT_Overload");

                // Feeder Up/Down (서로 배타 제어)
                dioControl.BindCylinder(
                    label: "Up/Down",
                    extend: () => InputRingTransferUnit.SetLift(true),
                    retract: () => InputRingTransferUnit.SetLift(false),
                    isExtended: () => InputRingTransferUnit.IsFeederUpValveOn(),
                    isRetracted: () => InputRingTransferUnit.IsFeederDownValveOn(),
                    displayKey: "FeederUpDn",
                    showSensors: false,
                    extendedName: "UP",
                    retractedName: "DOWN"
                );

                // Feeder Clamp/Unclamp (서로 배타 제어)
                dioControl.BindCylinder(
                    label: "Clamp/Unclamp",
                    extend: () => InputRingTransferUnit.SetClamp(true),
                    retract: () => InputRingTransferUnit.SetClamp(false),
                    isExtended: () => InputRingTransferUnit.IsFeederClampValveOn(),
                    isRetracted: () => InputRingTransferUnit.IsFeederUnclampValveOn(),
                    displayKey: "FeederClamp",
                    showSensors: false,
                    extendedName: "CLAMP",
                    retractedName: "UNCLAMP"
                );

                // Valves
                //dioControl.BindDIOOutput(
                //    () => InputRingTransferUnit.SetFeederUpValve(true),
                //    () => InputRingTransferUnit.SetFeederUpValve(false),
                //    "Feeder UP Valve",
                //    () => InputRingTransferUnit.IsFeederUpValveOn(),
                //    "IRT_FeederUpValve");
                //dioControl.BindDIOOutput(
                //    () => InputRingTransferUnit.SetFeederDownValve(true),
                //    () => InputRingTransferUnit.SetFeederDownValve(false),
                //    "Feeder DOWN Valve",
                //    () => InputRingTransferUnit.IsFeederDownValveOn(),
                //    "IRT_FeederDownValve");
                //dioControl.BindDIOOutput(
                //    () => InputRingTransferUnit.SetFeederClampValve(true),
                //    () => InputRingTransferUnit.SetFeederClampValve(false),
                //    "Feeder CLAMP Valve",
                //    () => InputRingTransferUnit.IsFeederClampValveOn(),
                //    "IRT_FeederClampValve");
                //dioControl.BindDIOOutput(
                //    () => InputRingTransferUnit.SetFeederUnclampValve(true),
                //    () => InputRingTransferUnit.SetFeederUnclampValve(false),
                //    "Feeder UNCLAMP Valve",
                //    () => InputRingTransferUnit.IsFeederUnclampValveOn(),
                //    "IRT_FeederUnclampValve");
            }
            catch { }
        }
        #endregion

        #region Sequences
        private void InitSequences()
        {
            try { manualSequenceControl?.ClearSequences(); } catch { }
        }
        #endregion

        private void WaferRing_Working_FormClosing(object sender, FormClosingEventArgs e)
        {
            try { } catch { }
        }
    }
}
