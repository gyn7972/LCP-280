using QMC.LCP_280.Process.Component;
using System;
using System.Windows.Forms;
using System.Threading.Tasks;
using LCP_280;
using QMC.Common;

namespace QMC.LCP_280.Process.Unit.FormWork
{
    /// <summary>
    /// WaferRing Working Form
    /// - TeachingPositionControl: InputFeeder, InputCassetteLifter
    /// - DIO 제어:
    ///    InputFeeder : Feeder Up/Down/Clamp 관련 센서 + 밸브 강제 제어
    ///    InputCassetteLifter : Cassette / RingJut / Mapping 센서 표시
    /// </summary>
    public partial class InputWafer_Working : Form
    {
        private const string WORK_NAME = "InputWafer";
        private Equipment Equipment => Equipment.Instance;

        private InputFeeder InputFeederUnit { get; set; }
        private InputCassetteLifter InputCassetteLifterUnit { get; set; }

        private MaterialCassette MaterialCassette { get; set; }

        private bool _initialized;
        private bool _preloadRequested;
        private bool _deferredInitDone; // 지연 초기화 완료 여부


        public InputWafer_Working() : this(
            TryGetUnit<InputFeeder>("InputFeeder"),
            TryGetUnit<InputCassetteLifter>("InputCassetteLifter"))
        {
        }

        public InputWafer_Working(InputFeeder ringTransfer, InputCassetteLifter cassetteLifter)
        {
            InitializeComponent();
            InputFeederUnit = ringTransfer;
            InputCassetteLifterUnit = cassetteLifter;

            Load += InputWafer_Working_Load;
            FormClosing += InputWafer_Working_FormClosing;

            var materialCassette = InputCassetteLifterUnit.GetMaterialCassette();
            waferMapView.SetMaterialCassette(materialCassette);

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

        private void InputWafer_Working_Load(object sender, EventArgs e)
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

                if (InputFeederUnit != null)
                {
                    teachingPositionControl.RegisterUnit(
                        "InputFeeder",
                        InputFeederUnit,
                        () => InputFeederUnit.Config?.TeachingPositions,
                        (name, vel) => InputFeederUnit.MoveToTeachingPosition(name, vel: vel),
                        tp => InputFeederUnit.Config?.SetTeachingPosition(tp),
                        autoReload: false);
                }

                if (InputCassetteLifterUnit != null)
                {
                    teachingPositionControl.RegisterUnit(
                        "InputCassetteLifter",
                        InputCassetteLifterUnit,
                        () => InputCassetteLifterUnit.Config?.TeachingPositions,
                        (name, vel) => InputCassetteLifterUnit.MoveToTeachingPosition(name, vel: vel),
                        tp => InputCassetteLifterUnit.Config?.SetTeachingPosition(tp),
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

                if (InputFeederUnit != null)
                {
                    StrongBindInputFeeder();
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

        private void StrongBindInputFeeder()
        {
            if (InputFeederUnit == null || dioControl == null) return;
            try
            {
                // Sensors
                dioControl.BindDIOInput(() => InputFeederUnit.IsFeederUp(), "Feeder UP Sns", "IRT_FeederUp");
                dioControl.BindDIOInput(() => InputFeederUnit.IsFeederDown(), "Feeder DOWN Sns", "IRT_FeederDown");
                dioControl.BindDIOInput(() => InputFeederUnit.IsUnclamped(), "Feeder UNCLAMP Sns", "IRT_Unclamp");
                dioControl.BindDIOInput(() => InputFeederUnit.IsRingPresent(), "Feeder RING Sns", "IRT_Ring");
                dioControl.BindDIOInput(() => InputFeederUnit.IsOverload(), "Feeder OVERLOAD Sns", "IRT_Overload");

                // Feeder Up/Down (서로 배타 제어)
                dioControl.BindCylinder(
                    label: "Up/Down",
                    extend: () => InputFeederUnit.SetLift(true),
                    retract: () => InputFeederUnit.SetLift(false),
                    isExtended: () => InputFeederUnit.IsFeederUpValveOn(),
                    isRetracted: () => InputFeederUnit.IsFeederDownValveOn(),
                    displayKey: "FeederUpDn",
                    showSensors: false,
                    extendedName: "UP",
                    retractedName: "DOWN"
                );

                // Feeder Clamp/Unclamp (서로 배타 제어)
                dioControl.BindCylinder(
                    label: "Clamp/Unclamp",
                    extend: () => InputFeederUnit.SetClamp(true),
                    retract: () => InputFeederUnit.SetClamp(false),
                    isExtended: () => InputFeederUnit.IsFeederClampValveOn(),
                    isRetracted: () => InputFeederUnit.IsFeederUnclampValveOn(),
                    displayKey: "FeederClamp",
                    showSensors: false,
                    extendedName: "CLAMP",
                    retractedName: "UNCLAMP"
                );

                // Valves
                //dioControl.BindDIOOutput(
                //    () => InputFeederUnit.SetFeederUpValve(true),
                //    () => InputFeederUnit.SetFeederUpValve(false),
                //    "Feeder UP Valve",
                //    () => InputFeederUnit.IsFeederUpValveOn(),
                //    "IRT_FeederUpValve");
                //dioControl.BindDIOOutput(
                //    () => InputFeederUnit.SetFeederDownValve(true),
                //    () => InputFeederUnit.SetFeederDownValve(false),
                //    "Feeder DOWN Valve",
                //    () => InputFeederUnit.IsFeederDownValveOn(),
                //    "IRT_FeederDownValve");
                //dioControl.BindDIOOutput(
                //    () => InputFeederUnit.SetFeederClampValve(true),
                //    () => InputFeederUnit.SetFeederClampValve(false),
                //    "Feeder CLAMP Valve",
                //    () => InputFeederUnit.IsFeederClampValveOn(),
                //    "IRT_FeederClampValve");
                //dioControl.BindDIOOutput(
                //    () => InputFeederUnit.SetFeederUnclampValve(true),
                //    () => InputFeederUnit.SetFeederUnclampValve(false),
                //    "Feeder UNCLAMP Valve",
                //    () => InputFeederUnit.IsFeederUnclampValveOn(),
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

        private void InputWafer_Working_FormClosing(object sender, FormClosingEventArgs e)
        {
            try { } catch { }
        }

        private void btnMapping_Click(object sender, EventArgs e)
        {
            try
            {
                if (InputCassetteLifterUnit == null)
                {
                    MessageBox.Show("InputCassetteLifterUnit이 초기화되지 않았습니다.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Wafer 감지 수행
                int nRet = InputCassetteLifterUnit.ScanWafer();

                if (nRet != 0)
                {
                    //MessageBox.Show($"Wafer 감지 실패. 오류 코드: {nRet}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Log.Write("InputWafer_Working", "", $"Wafer 감지 실패. 오류 코드: {nRet}", "오류");
                    return;
                }
                // scan 후 재설정.
                var materialCassette = InputCassetteLifterUnit.GetMaterialCassette();
                if (materialCassette == null)
                {
                    MessageBox.Show("Wafer 감지 실패.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                // WaferMapView에 데이터 설정 //material
                waferMapView.SetMaterialCassette(materialCassette);

                MessageBox.Show("Wafer 감지가 완료되었습니다.", "정보", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Wafer 감지 중 오류 발생: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
