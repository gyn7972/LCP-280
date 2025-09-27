using LCP_280;
using QMC.Common;
using QMC.Common.UI;
using QMC.LCP_280.Process.Component;
using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using static QMC.Common.Unit.BaseUnit;

namespace QMC.LCP_280.Process.Unit.FormWork
{
    [FormOrder(2)]
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

        
        private InputCassetteLifter InputCassetteLifter { get; set; }
        private InputFeeder InputFeeder { get; set; }
        private InputStage InputStage { get; set; }

        private MaterialCassette MaterialCassette { get; set; }

        private bool _initialized;
        private bool _preloadRequested;
        private bool _deferredInitDone; // 지연 초기화 완료 여부


        public InputWafer_Working() : this(
            TryGetUnit<InputCassetteLifter>("InputCassetteLifter"),
            TryGetUnit<InputFeeder>("InputFeeder"),
            TryGetUnit<InputStage>("InputStage")
            )
        {
        }

        public InputWafer_Working(InputCassetteLifter cassetteLifter, InputFeeder ringTransfer, InputStage inputStage)
        {
            InitializeComponent();
            InputCassetteLifter = cassetteLifter;
            InputFeeder = ringTransfer;
            InputStage = inputStage;

            Load += InputWafer_Working_Load;
            FormClosing += InputWafer_Working_FormClosing;

            var materialCassette = InputCassetteLifter.GetMaterialCassette();
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

                if (InputFeeder != null)
                {
                    teachingPositionControl.RegisterUnit(
                        "InputFeeder",
                        InputFeeder,
                        () => InputFeeder.Config?.TeachingPositions,
                        (name, vel) => InputFeeder.MoveToTeachingPosition(name, vel: vel),
                        tp => InputFeeder.Config?.SetTeachingPosition(tp),
                        autoReload: false);
                }

                if (InputCassetteLifter != null)
                {
                    teachingPositionControl.RegisterUnit(
                        "InputCassetteLifter",
                        InputCassetteLifter,
                        () => InputCassetteLifter.Config?.TeachingPositions,
                        (name, vel) => InputCassetteLifter.MoveToTeachingPosition(name, vel: vel),
                        tp => InputCassetteLifter.Config?.SetTeachingPosition(tp),
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

                if (InputFeeder != null)
                {
                    StrongBindInputFeeder();
                }

                if (InputCassetteLifter != null)
                {
                    dioControl.BindDIOInput(() => InputCassetteLifter.IsCassettePresent0(), "Cassette Present 0", "ICL_Cass0");
                    dioControl.BindDIOInput(() => InputCassetteLifter.IsCassettePresent1(), "Cassette Present 1", "ICL_Cass1");
                    dioControl.BindDIOInput(() => InputCassetteLifter.IsAnyCassettePresent(), "Cassette Any", "ICL_CassAny");
                    dioControl.BindDIOInput(() => InputCassetteLifter.IsWaferProtrusionDetectionSensor(), "Ring Jut", "ICL_RingJut");
                    dioControl.BindDIOInput(() => InputCassetteLifter.MappingSensor(), "Mapping Sensor", "ICL_Mapping");
                }

                dioControl.RebuildLists();
            }
            catch { }
        }

        private void StrongBindInputFeeder()
        {
            if (InputFeeder == null || dioControl == null) return;
            try
            {
                // Sensors
                dioControl.BindDIOInput(() => InputFeeder.IsFeederUp(), "Feeder UP Sns", "IRT_FeederUp");
                dioControl.BindDIOInput(() => InputFeeder.IsFeederDown(), "Feeder DOWN Sns", "IRT_FeederDown");
                dioControl.BindDIOInput(() => InputFeeder.IsUnClamped(), "Feeder UNCLAMP Sns", "IRT_Unclamp");
                dioControl.BindDIOInput(() => InputFeeder.IsRingPresent(), "Feeder RING Sns", "IRT_Ring");
                dioControl.BindDIOInput(() => InputFeeder.IsOverload(), "Feeder OVERLOAD Sns", "IRT_Overload");

                // Feeder Up/Down (서로 배타 제어)
                dioControl.BindCylinder(
                    label: "Up/Down",
                    extend: () => InputFeeder.SetLift(true),
                    retract: () => InputFeeder.SetLift(false),
                    isExtended: () => InputFeeder.IsFeederUpValveOn(),
                    isRetracted: () => InputFeeder.IsFeederDownValveOn(),
                    displayKey: "FeederUpDn",
                    showSensors: false,
                    extendedName: "UP",
                    retractedName: "DOWN"
                );

                // Feeder Clamp/Unclamp (서로 배타 제어)
                dioControl.BindCylinder(
                    label: "Clamp/Unclamp",
                    extend: () => InputFeeder.SetClamp(true),
                    retract: () => InputFeeder.SetClamp(false),
                    isExtended: () => InputFeeder.IsFeederClampValveOn(),
                    isRetracted: () => InputFeeder.IsFeederUnclampValveOn(),
                    displayKey: "FeederClamp",
                    showSensors: false,
                    extendedName: "CLAMP",
                    retractedName: "UNCLAMP"
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
                if (_InputWaferCameraviewer != null && InputStage?.StageCamera != null)
                {
                    if (_InputWaferCameraviewer.Camera != InputStage.StageCamera)
                        _InputWaferCameraviewer.Camera = InputStage.StageCamera;
                    try { InputStage.StageCamera.StartLive(); } catch { }
                    try { _InputWaferCameraviewer.StartUpdateTask(); } catch { }
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
                InputCassetteLifter = TryGetUnit<InputCassetteLifter>("InputCassetteLifter");
                InputFeeder = TryGetUnit<InputFeeder>("InputFeeder");
                InputStage = TryGetUnit<InputStage>("InputStage");
                
                if (InputCassetteLifter != null)
                {
                    // InputFeeder, InputStage 시컨스 등록시 InputCassetteLifter를통하여 등록.
                    // 순차적으로 시컨스 진행 형태로.
                    // 완전 Manual Mode는 따로 등록해서 Test 하자.

                    // 완전 Manual Mode.
                    // InputStage Align, Mapping은 따로 등록해서 Test 하자.
                    manualSequenceControlCassette.ParentUnit = InputCassetteLifter; // 시퀀스 등록 대상 유닛 지정
                    

                    manualSequenceControlInputCassette.ParentUnit = InputCassetteLifter; // 시퀀스 등록 대상 유닛 지정

                }

                if (InputFeeder != null)
                {
                    //manualSequenceControlInputStage.ParentUnit = InputStage; // 시퀀스 등록 대상 유닛 지정
                    manualSequenceControlInputFeeder.ParentUnit = InputFeeder; // 시퀀스 등록 대상 유닛 지정
                }

                if (InputStage != null)
                {
                    //manualSequenceControlInputStage.ParentUnit = InputStage; // 시퀀스 등록 대상 유닛 지정
                    manualSequenceControlInputWaferStage.ParentUnit = InputStage; // 시퀀스 등록 대상 유닛 지정
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
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
                if (InputCassetteLifter == null)
                {
                    MessageBox.Show("InputCassetteLifter 초기화되지 않았습니다.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Wafer 감지 수행.
                var v = InputCassetteLifter.ScanWaferAsync();
                ProgressForm progressForm = new ProgressForm("Cassette Mapping","Scanning......" ,v);
                progressForm.ShowDialog(this);

                int nRet = v.Result;

                if (nRet != 0)
                {
                    //MessageBox.Show($"Wafer 감지 실패. 오류 코드: {nRet}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Log.Write("InputWafer_Working", "", $"Wafer 감지 실패. 오류 코드: {nRet}", "오류");
                    return;
                }
                // scan 후 재설정.
                var materialCassette = InputCassetteLifter.GetMaterialCassette();
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

        private void checkBoxTest_CheckedChanged(object sender, EventArgs e)
        {
            //todo : 사용금지 - config 불러오기/저장 다시 만들어야함.
            var ask = new MessageBoxYesNo();
            if (ask.ShowDialog("확인", "DryRun 모드를 변경합니다.\n진행하시겠습니까?") != DialogResult.Yes)
                return;

            if (checkBoxTest.Checked)
            {
                //Equipment?.ConfigManager?.ApplyGlobalDryRunAndSave(true, save: true);
            }
            else if(!checkBoxTest.Checked)
            {
                //Equipment?.ConfigManager?.ApplyGlobalDryRunAndSave(false, save: true);
            }
            else
            {
                //Equipment?.ConfigManager?.ApplyGlobalDryRunAndSave(false, save: false);
            }
        }

        private void checkBoxSimulation_CheckedChanged(object sender, EventArgs e)
        {
            //todo : 사용금지 - config 불러오기/저장 다시 만들어야함.
            var ask = new MessageBoxYesNo();
            if (ask.ShowDialog("확인", "Simulation 모드를 변경합니다.\n진행하시겠습니까?") != DialogResult.Yes)
                return;

            if (checkBoxSimulation.Checked)
            {
                //Equipment?.ConfigManager?.ApplyGlobalSimulationAndSave(true, save: true);
            }
            else if (!checkBoxSimulation.Checked)
            {
                //Equipment?.ConfigManager?.ApplyGlobalSimulationAndSave(false, save: true);
            }
            else
            {
                //Equipment?.ConfigManager?.ApplyGlobalSimulationAndSave(false, save: false);
            }
        }

        private void buttonRequstInput_Click(object sender, EventArgs e)
        {
            var ask = new MessageBoxYesNo();
            if (ask.ShowDialog("확인", "RequestLoadWafe.\n진행하시겠습니까?") != DialogResult.Yes)
                return;

            InputStage.RequestLoadWafer = IfState.Request;
        }
    }
}
