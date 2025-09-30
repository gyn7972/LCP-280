using QMC.LCP_280.Process.Component;
using System;
using System.Windows.Forms;
using System.Threading.Tasks;
using QMC.Common;

namespace QMC.LCP_280.Process.Unit.FormWork
{
    [FormOrder(4)]
    /// <summary>
    /// WaferBin Working Form
    /// - TeachingPositionControl: OutputFeeder, OutputCassetteLifter
    /// - DIO 제어:
    ///    OutputFeeder : Feeder Up/Down/Clamp 관련 센서 + 밸브 강제 제어
    ///    OutputCassetteLifter : Cassette / RingJut / Mapping 센서 표시
    /// </summary>
    public partial class OutputWafer_Working : Form
    {
        private const string WORK_NAME = "WaferBin";
        private Equipment Equipment => Equipment.Instance;

        private OutputStage OutputStage { get; set; }
        private OutputFeeder OutputFeeder { get; set; }
        private OutputCassetteLifter OutputCassetteLifter { get; set; }

        private bool _initialized;          // 실제 UI 바인딩 완료 여부 (텍스트/핸들)
        private bool _preloadRequested;     // PreloadUI 호출 여부(1회)
        private bool _deferredInitDone;     // 무거운 바인딩 지연 수행 여부

        public OutputWafer_Working() : this(
            TryGetUnit<OutputStage>("OutputStage"),
            TryGetUnit<OutputFeeder>("OutputFeeder"),
            TryGetUnit<OutputCassetteLifter>("OutputCassetteLifter"))
        {
        }

        public OutputWafer_Working(OutputStage outputStage, OutputFeeder ringTransfer, OutputCassetteLifter cassetteLifter)
        {
            InitializeComponent();
            OutputStage = outputStage;
            OutputFeeder = ringTransfer;
            OutputCassetteLifter = cassetteLifter;

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
                BindCamera();

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

                if (OutputFeeder != null)
                {
                    teachingPositionControl.RegisterUnit(
                        "OutputFeeder",
                        OutputFeeder,
                        () => OutputFeeder.Config?.TeachingPositions,
                        (name, vel) => OutputFeeder.MoveToTeachingPosition(name, vel: vel),
                        tp => OutputFeeder.Config?.SetTeachingPosition(tp),
                        autoReload: false);
                }

                if (OutputCassetteLifter != null)
                {
                    teachingPositionControl.RegisterUnit(
                        "OutputCassetteLifter",
                        OutputCassetteLifter,
                        () => OutputCassetteLifter.Config?.TeachingPositions,
                        (name, vel) => OutputCassetteLifter.MoveToTeachingPosition(name, vel: vel),
                        tp => OutputCassetteLifter.Config?.SetTeachingPosition(tp),
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

                if (OutputFeeder != null)
                {
                    dioControl.BindDIOInput(() => false, "---- OutputFeeder ----", "SEP_ORT");
                    StrongBindOutputFeeder();
                }

                if (OutputCassetteLifter != null)
                {
                    dioControl.BindDIOInput(() => false, "---- OutputCassetteLifter ----", "SEP_OCL");
                    dioControl.BindDIOInput(() => OutputCassetteLifter.IsCassettePresent0(), "Cassette Present 0", "OCL_Cass0");
                    dioControl.BindDIOInput(() => OutputCassetteLifter.IsCassettePresent1(), "Cassette Present 1", "OCL_Cass1");
                    dioControl.BindDIOInput(() => OutputCassetteLifter.IsAnyCassettePresent(), "Cassette Any", "OCL_CassAny");
                    dioControl.BindDIOInput(() => OutputCassetteLifter.RingJut(), "Ring Jut", "OCL_RingJut");
                    dioControl.BindDIOInput(() => OutputCassetteLifter.MappingSensor(), "Mapping Sensor", "OCL_Mapping");
                }

                dioControl.RebuildLists();
            }
            catch { }
        }

        private void StrongBindOutputFeeder()
        {
            if (OutputFeeder == null || dioControl == null) return;
            try
            {
                // Sensors
                dioControl.BindDIOInput(() => OutputFeeder.IsFeederUp(), "Feeder UP Sns", "ORT_FeederUp");
                dioControl.BindDIOInput(() => OutputFeeder.IsFeederDown(), "Feeder DOWN Sns", "ORT_FeederDown");
                dioControl.BindDIOInput(() => OutputFeeder.IsUnClamped(), "Feeder UNCLAMP Sns", "ORT_Unclamp");
                dioControl.BindDIOInput(() => OutputFeeder.IsRingPresent(), "Feeder RING Sns", "ORT_Ring");
                dioControl.BindDIOInput(() => OutputFeeder.IsOverload(), "Feeder OVERLOAD Sns", "ORT_Overload");

                // Feeder Up/Down (서로 배타 제어)
                dioControl.BindCylinder(
                    label: "Up/Down",
                    extend: () => OutputFeeder.SetLift(true),
                    retract: () => OutputFeeder.SetLift(false),
                    isExtended: () => OutputFeeder.IsFeederUpValveOn(),
                    isRetracted: () => OutputFeeder.IsFeederDownValveOn(),
                    displayKey: "FeederUpDn",
                    showSensors: false,
                    extendedName: "UP",
                    retractedName: "DOWN"
                );

                // Feeder Clamp/Unclamp (서로 배타 제어)
                dioControl.BindCylinder(
                    label: "Clamp/Unclamp",
                    extend: () => OutputFeeder.SetClamp(true),
                    retract: () => OutputFeeder.SetClamp(false), 
                    isExtended: () => OutputFeeder.IsFeederClampValveOn(),
                    isRetracted: () => OutputFeeder.IsFeederUnclampValveOn(),
                    displayKey: "FeederClamp",
                    showSensors: false,
                    extendedName: "CLAMP",
                    retractedName: "UNCLAMP"
                );

                //// Valves (direct forced control + state feedback)
                //dioControl.BindDIOOutput(
                //    () => _OutputFeederUnit.SetFeederUpValve(true),
                //    () => _OutputFeederUnit.SetFeederUpValve(false),
                //    "Feeder UP Valve",
                //    () => _OutputFeederUnit.IsFeederUpValveOn(),
                //    "ORT_UpValve");

                //dioControl.BindDIOOutput(
                //    () => _OutputFeederUnit.SetFeederDownValve(true),
                //    () => _OutputFeederUnit.SetFeederDownValve(false),
                //    "Feeder DOWN Valve",
                //    () => _OutputFeederUnit.IsFeederDownValveOn(),
                //    "ORT_DownValve");

                //dioControl.BindDIOOutput(
                //    () => _OutputFeederUnit.SetFeederClampValve(true),
                //    () => _OutputFeederUnit.SetFeederClampValve(false),
                //    "Feeder CLAMP Valve",
                //    () => _OutputFeederUnit.IsFeederClampValveOn(),
                //    "ORT_ClampValve");

                //dioControl.BindDIOOutput(
                //    () => _OutputFeederUnit.SetFeederUnclampValve(true),
                //    () => _OutputFeederUnit.SetFeederUnclampValve(false),
                //    "Feeder UNCLAMP Valve",
                //    () => _OutputFeederUnit.IsFeederUnclampValveOn(),
                //    "ORT_UnclampValve");
            }
            catch { }
        }
        #endregion

        #region Camera
        private void BindCamera()
        {
            try
            {
                if (_OutputWaferCameraviewer != null && OutputStage?.OutStageCamera != null)
                {
                    if (_OutputWaferCameraviewer.Camera != OutputStage.OutStageCamera)
                        _OutputWaferCameraviewer.Camera = OutputStage.OutStageCamera;
                    try { OutputStage.OutStageCamera.StartLive(); } catch { }
                    try { _OutputWaferCameraviewer.StartUpdateTask(); } catch { }
                }
            }
            catch { }
        }
        #endregion

        #region Sequences (Placeholder)
        private void InitSequences()
        {
            try
            {
                // 시퀀스 초기화
                OutputCassetteLifter = TryGetUnit<OutputCassetteLifter>("OutputCassetteLifter");
                OutputFeeder = TryGetUnit<OutputFeeder>("OutputFeeder");
                OutputStage = TryGetUnit<OutputStage>("OutputStage");

                if (OutputFeeder != null)
                {
                    manualSequenceControlCassette.ParentUnit = OutputFeeder; // 시퀀스 등록 대상 유닛 지정
                }

                //if (OutputCassetteLifter != null)
                //{
                //    manualSequenceControlCassette.ParentUnit = OutputCassetteLifter; // 시퀀스 등록 대상 유닛 지정
                //}

                //if(OutputStage != null)
                //{
                //    manualSequenceControlOutputStage.ParentUnit = OutputStage; // 시퀀스 등록 대상 유닛 지정
                //}

            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
        }
        #endregion

        private void WaferBin_Working_FormClosing(object sender, FormClosingEventArgs e)
        {
            try { } catch { }
        }
    }
}
