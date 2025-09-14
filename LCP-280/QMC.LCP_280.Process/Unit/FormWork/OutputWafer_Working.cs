using QMC.LCP_280.Process.Component;
using System;
using System.Windows.Forms;
using System.Threading.Tasks;

namespace QMC.LCP_280.Process.Unit.FormWork
{
    /// <summary>
    /// WaferBin Working Form
    /// - TeachingPositionControl: OutputRingTransfer, OutputCassetteLifter
    /// - DIO 제어:
    ///    OutputRingTransfer : Feeder Up/Down/Clamp 관련 센서 + 밸브 강제 제어
    ///    OutputCassetteLifter : Cassette / RingJut / Mapping 센서 표시
    /// </summary>
    public partial class OutputWafer_Working : Form
    {
        private const string WORK_NAME = "WaferBin";
        private Equipment Equipment => Equipment.Instance;

        private OutputStage _OutputStage { get; set; }
        private OutputRingTransfer _OutputRingTransferUnit { get; set; }
        private OutputCassetteLifter _OutputCassetteLifterUnit { get; set; }

        private bool _initialized;          // 실제 UI 바인딩 완료 여부 (텍스트/핸들)
        private bool _preloadRequested;     // PreloadUI 호출 여부(1회)
        private bool _deferredInitDone;     // 무거운 바인딩 지연 수행 여부

        public OutputWafer_Working() : this(
            TryGetUnit<OutputStage>("OutputStage"),
            TryGetUnit<OutputRingTransfer>("OutputRingTransfer"),
            TryGetUnit<OutputCassetteLifter>("OutputCassetteLifter"))
        {
        }

        public OutputWafer_Working(OutputStage outputStage, OutputRingTransfer ringTransfer, OutputCassetteLifter cassetteLifter)
        {
            InitializeComponent();
            _OutputStage = outputStage;
            _OutputRingTransferUnit = ringTransfer;
            _OutputCassetteLifterUnit = cassetteLifter;

            Load += WaferBin_Working_Load;
            FormClosing += WaferBin_Working_FormClosing;
        }

        public void PreloadUI()
        {
            if (IsDisposed || Disposing) return;
            if (_preloadRequested) return;
            _preloadRequested = true;
            EnsureInitialized();
            var handle = Handle; // 강제 핸들 생성
        }

        private void WaferBin_Working_Load(object sender, EventArgs e)
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
                // 무거운 바인딩은 지연 수행
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
            // 아주 짧은 지연으로 첫 Paint 후 실행되도록 유도
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

        #region Teaching
        private void BindTeachingPositions()
        {
            try
            {
                if (teachingPositionControl == null) return;
                teachingPositionControl.ClearUnits();

                if (_OutputRingTransferUnit != null)
                {
                    teachingPositionControl.RegisterUnit(
                        "OutputRingTransfer",
                        _OutputRingTransferUnit,
                        () => _OutputRingTransferUnit.Config?.TeachingPositions,
                        (name, vel) => _OutputRingTransferUnit.MoveToTeachingPosition(name, vel: vel),
                        tp => _OutputRingTransferUnit.Config?.SetTeachingPosition(tp),
                        autoReload: false);
                }

                if (_OutputCassetteLifterUnit != null)
                {
                    teachingPositionControl.RegisterUnit(
                        "OutputCassetteLifter",
                        _OutputCassetteLifterUnit,
                        () => _OutputCassetteLifterUnit.Config?.TeachingPositions,
                        (name, vel) => _OutputCassetteLifterUnit.MoveToTeachingPosition(name, vel: vel),
                        tp => _OutputCassetteLifterUnit.Config?.SetTeachingPosition(tp),
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

                if (_OutputRingTransferUnit != null)
                {
                    dioControl.BindDIOInput(() => false, "---- OutputRingTransfer ----", "SEP_ORT");
                    StrongBindOutputRingTransfer();
                }

                if (_OutputCassetteLifterUnit != null)
                {
                    dioControl.BindDIOInput(() => false, "---- OutputCassetteLifter ----", "SEP_OCL");
                    dioControl.BindDIOInput(() => _OutputCassetteLifterUnit.CassettePresent0(), "Cassette Present 0", "OCL_Cass0");
                    dioControl.BindDIOInput(() => _OutputCassetteLifterUnit.CassettePresent1(), "Cassette Present 1", "OCL_Cass1");
                    dioControl.BindDIOInput(() => _OutputCassetteLifterUnit.AnyCassettePresent(), "Cassette Any", "OCL_CassAny");
                    dioControl.BindDIOInput(() => _OutputCassetteLifterUnit.RingJut(), "Ring Jut", "OCL_RingJut");
                    dioControl.BindDIOInput(() => _OutputCassetteLifterUnit.MappingSensor(), "Mapping Sensor", "OCL_Mapping");
                }

                dioControl.RebuildLists();
            }
            catch { }
        }

        private void StrongBindOutputRingTransfer()
        {
            if (_OutputRingTransferUnit == null || dioControl == null) return;
            try
            {
                // Sensors
                dioControl.BindDIOInput(() => _OutputRingTransferUnit.IsFeederUp(), "Feeder UP Sns", "ORT_FeederUp");
                dioControl.BindDIOInput(() => _OutputRingTransferUnit.IsFeederDown(), "Feeder DOWN Sns", "ORT_FeederDown");
                dioControl.BindDIOInput(() => _OutputRingTransferUnit.IsUnclamped(), "Feeder UNCLAMP Sns", "ORT_Unclamp");
                dioControl.BindDIOInput(() => _OutputRingTransferUnit.IsRingPresent(), "Feeder RING Sns", "ORT_Ring");
                dioControl.BindDIOInput(() => _OutputRingTransferUnit.IsOverload(), "Feeder OVERLOAD Sns", "ORT_Overload");

                // Feeder Up/Down (서로 배타 제어)
                dioControl.BindCylinder(
                    label: "Up/Down",
                    extend: () => _OutputRingTransferUnit.SetLift(true),
                    retract: () => _OutputRingTransferUnit.SetLift(false),
                    isExtended: () => _OutputRingTransferUnit.IsFeederUpValveOn(),
                    isRetracted: () => _OutputRingTransferUnit.IsFeederDownValveOn(),
                    displayKey: "FeederUpDn",
                    showSensors: false,
                    extendedName: "UP",
                    retractedName: "DOWN"
                );

                // Feeder Clamp/Unclamp (서로 배타 제어)
                dioControl.BindCylinder(
                    label: "Clamp/Unclamp",
                    extend: () => _OutputRingTransferUnit.SetClamp(true),
                    retract: () => _OutputRingTransferUnit.SetClamp(false), 
                    isExtended: () => _OutputRingTransferUnit.IsFeederClampValveOn(),
                    isRetracted: () => _OutputRingTransferUnit.IsFeederUnclampValveOn(),
                    displayKey: "FeederClamp",
                    showSensors: false,
                    extendedName: "CLAMP",
                    retractedName: "UNCLAMP"
                );

                //// Valves (direct forced control + state feedback)
                //dioControl.BindDIOOutput(
                //    () => _OutputRingTransferUnit.SetFeederUpValve(true),
                //    () => _OutputRingTransferUnit.SetFeederUpValve(false),
                //    "Feeder UP Valve",
                //    () => _OutputRingTransferUnit.IsFeederUpValveOn(),
                //    "ORT_UpValve");

                //dioControl.BindDIOOutput(
                //    () => _OutputRingTransferUnit.SetFeederDownValve(true),
                //    () => _OutputRingTransferUnit.SetFeederDownValve(false),
                //    "Feeder DOWN Valve",
                //    () => _OutputRingTransferUnit.IsFeederDownValveOn(),
                //    "ORT_DownValve");

                //dioControl.BindDIOOutput(
                //    () => _OutputRingTransferUnit.SetFeederClampValve(true),
                //    () => _OutputRingTransferUnit.SetFeederClampValve(false),
                //    "Feeder CLAMP Valve",
                //    () => _OutputRingTransferUnit.IsFeederClampValveOn(),
                //    "ORT_ClampValve");

                //dioControl.BindDIOOutput(
                //    () => _OutputRingTransferUnit.SetFeederUnclampValve(true),
                //    () => _OutputRingTransferUnit.SetFeederUnclampValve(false),
                //    "Feeder UNCLAMP Valve",
                //    () => _OutputRingTransferUnit.IsFeederUnclampValveOn(),
                //    "ORT_UnclampValve");
            }
            catch { }
        }
        #endregion

        #region Sequences (Placeholder)
        private void InitSequences()
        {
            try { manualSequenceControl?.ClearSequences(); }
            catch { }
        }
        #endregion

        private void WaferBin_Working_FormClosing(object sender, FormClosingEventArgs e)
        {
            try { } catch { }
        }
    }
}
