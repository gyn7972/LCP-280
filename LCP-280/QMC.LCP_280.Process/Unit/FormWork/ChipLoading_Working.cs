using QMC.Common;
using QMC.LCP_280.Process.Component; 
using System;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QMC.LCP_280.Process.Unit
{

    /// <summary>
    /// ChipLoading Working Form
    ///  - TeachingPositionControl : InputStage / InputStageEjector / InputDieTransfer 등록
    ///  - DIO 제어 :
    ///      InputStage : 센서 + 밸브 (Raw 포함) 강제 제어
    ///      InputDieTransfer : Arm Vac / Blow / Vent 제어 (4 Arms)
    ///  - Vision : Stage Camera Live 연결
    ///  - Manual Sequence : (InputStage Align / Mapping / Pick / Place 등 등록)
    /// </summary>
    /// 

    [FormOrder(1)]
    public partial class ChipLoading_Working : Form
    {
        private const string WORK_NAME = "ChipLoading";
        private Equipment Equipment => Equipment.Instance;

        // Units
        private InputStage InputStage { get; set; }
        private InputStageEjector InputStageEjector { get; set; }
        private InputDieTransfer InputDieTransfer { get; set; }

        // State
        private bool _initialized;
        private bool _preloadRequested;
        private bool _deferredInitDone; // 지연 초기화 완료 여부
        private bool _isLayoutEditMode;

        private CancellationTokenSource _ctsPickUp;

        #region Constructors
        public ChipLoading_Working() : this(
            TryGetUnit<InputStage>("InputStage"),
            TryGetUnit<InputStageEjector>("InputStageEjector"),
            TryGetUnit<InputDieTransfer>("InputDieTransfer"))
        { }

        public ChipLoading_Working(InputStage inputStage, InputStageEjector ejector, InputDieTransfer dieTransfer)
        {
            InitializeComponent();
            InputStage = inputStage;
            InputStageEjector = ejector;
            InputDieTransfer = dieTransfer;
            Load += ChipLoading_Working_Load;
            FormClosing += ChipLoading_Working_FormClosing;
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

        private void ChipLoading_Working_Load(object sender, EventArgs e) => EnsureInitialized();

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

                if (InputStage != null)
                {
                    teachingPositionControl.RegisterUnit(
                        "InputStage",
                        InputStage,
                        () => InputStage.Config?.TeachingPositions,
                        (name, vel) => InputStage.MoveToTeachingPosition(name, vel: vel),
                        tp => InputStage.Config?.SetTeachingPosition(tp),
                        autoReload: false);
                }
                if (InputStageEjector != null)
                {
                    teachingPositionControl.RegisterUnit(
                        "InputStageEjector",
                        InputStageEjector,
                        () => InputStageEjector.InputStageEjectorConfig?.TeachingPositions,
                        (name, vel) => InputStageEjector.MoveToTeachingPosition(name, vel: vel),
                        tp => InputStageEjector.InputStageEjectorConfig?.SetTeachingPosition(tp),
                        autoReload: false);
                }
                if (InputDieTransfer != null)
                {
                    teachingPositionControl.RegisterUnit(
                        "InputDieTransfer",
                        InputDieTransfer,
                        () => InputDieTransfer.InputDieTransferConfig?.TeachingPositions,
                        (name, vel) => InputDieTransfer.MoveToTeachingPosition(name, vel: vel),
                        tp => InputDieTransfer.InputDieTransferConfig?.SetTeachingPosition(tp),
                        autoReload: false);
                }

                teachingPositionControl.SetSaveCancelVisible(false, false);
                teachingPositionControl.RefreshData();
            }
            catch
            {
                try { teachingPositionControl?.SetUnits(InputStage, InputStageEjector, InputDieTransfer, true); } catch { }
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
                if (InputDieTransfer != null)
                {
                    dioControl.BindDIOInput(() => false, "---- InputDieTransfer ----", "SEP_IDT");

                    for (int arm = 0; arm < 1; arm++) // 필요 시 4로 변경: arm < 4
                    {
                        int idx = arm;
                        string labelBase = $"IDT Arm{idx + 1}";

                        // Flow 센서(입력) 표시
                        dioControl.BindDIOInput(
                            () => InputDieTransfer.ArmFlowOk(idx),
                            $"{labelBase} Flow OK(Sns)",
                            $"IDT_Arm{idx + 1}_FlowOk");

                        // VAC: 도메인 + 상태 함수 연결 (출력은 입력과 무관하게 동작)
                        dioControl.BindVacuum(
                            label: $"{labelBase} VAC",
                            on: () => InputDieTransfer.SetVacuum(idx, true),
                            off: () => InputDieTransfer.SetVacuum(idx, false),
                            isOk: null,
                            isOnState: null,
                            displayKey: $"IDT_Arm{idx + 1}_Vac",
                            showOkSensor: false
                        );

                        dioControl.BindVacuum(
                            label: $"{labelBase} Blow",
                            on: () => InputDieTransfer.SetBlow(idx, true),
                            off: () => InputDieTransfer.SetBlow(idx, false),
                            isOk: null, // 별도 센서 없음(Flow는 위에서 별도 표시)
                            isOnState: null,
                            displayKey: $"IDT_Arm{idx + 1}_Blow",
                            showOkSensor: false
                        );

                        dioControl.BindVacuum(
                            label: $"{labelBase} Vent",
                            on: () => InputDieTransfer.SetVent(idx, true),
                            off: () => InputDieTransfer.SetVent(idx, false),
                            isOk: null, // 별도 센서 없음(Flow는 위에서 별도 표시)
                            isOnState: null,
                            displayKey: $"IDT_Arm{idx + 1}_Vent",
                            showOkSensor: false
                        );
                    }
                }

                dioControl.RebuildLists();
            }
            catch { }
        }

        private void StrongBindInputStage()
        {
            if (InputStage == null || dioControl == null) return;
            try
            {
                // ===== Sensors =====
                dioControl.BindDIOInput(() => InputStage.IsVacuumOn(), "Vacuum OK(Sns)", "StageVacOk");
                dioControl.BindDIOInput(() => InputStage.IsPlateUp(), "Plate UP Sns", "StagePlateUp");
                dioControl.BindDIOInput(() => InputStage.IsPlateDown(), "Plate DOWN Sns", "StagePlateDn");
                dioControl.BindDIOInput(() => InputStage.IsClampLiftDown(), "ClampLift DOWN Sns", "StageClampDn");
                dioControl.BindDIOInput(() => InputStage.IsClampFwd(), "Clamp FWD Sns", "StageClampFwd");
                dioControl.BindDIOInput(() => InputStage.Ring0(), "Ring Sns 0", "StageRing0");
                dioControl.BindDIOInput(() => InputStage.Ring1(), "Ring Sns 1", "StageRing1");
                dioControl.BindDIOInput(() => InputStage.IsRingPresent(), "Ring Any", "StageRingAny");

                // Vacuum: 도메인 함수 사용 (출력은 입력과 무관하게 동작, 상태 표시는 밸브 상태 함수 사용)
                dioControl.BindVacuum(
                    label: "Vacuum",
                    on: () => InputStage.SetVacuum(true),
                    off: () => InputStage.SetVacuum(false),
                    isOk: () => InputStage.IsVacuumOn(),
                    isOnState: () => InputStage.IsVacuumValveOn(),
                    displayKey: "StageVac",
                    showOkSensor: false // 위에서 OK 센서를 이미 표시했으므로 중복 방지
                );

                // Plate Up/Down: 도메인 함수 사용, 상태 판단은 IsPlateUp 기준
                dioControl.BindCylinder(
                    label: "PlateUpDn",
                    extend: () => InputStage.SetClampPlate(true),
                    retract: () => InputStage.SetClampPlate(false),
                    isExtended: () => InputStage.IsPlateUp(),
                    isRetracted: () => InputStage.IsPlateDown(),
                    displayKey: "StagePlateUpDn",
                    showSensors: false // 위에서 Up/Down 센서를 이미 표시했으므로 중복 방지
                );

                // ClampLift Up/Down
                dioControl.BindCylinder(
                    label: "ClampLift",
                    extend: () => InputStage.SetClampLift(true),
                    retract: () => InputStage.SetClampLift(false),
                    // Up 센서가 없으면 밸브 상태 사용, Down은 센서 사용
                    isExtended: () => InputStage.IsClampLiftUp(),
                    isRetracted: () => InputStage.IsClampLiftDown(),
                    displayKey: "StageClampUpDn",
                    showSensors: false,
                    extendedName: "UP",
                    retractedName: "DOWN"
                );

                // Clamp FWD/BWD
                dioControl.BindCylinder(
                    label: "ClampFB",
                    extend: () => InputStage.SetClampFB(true),
                    retract: () => InputStage.SetClampFB(false),
                    // FWD 센서만 있어도 동작. BWD는 없으면 null 가능(토글은 FWD 센서로 판단)
                    isExtended: () => InputStage.IsClampFwd(),
                    isRetracted: null,
                    displayKey: "StageClampFB",
                    showSensors: false,
                    extendedName: "FWD",
                    retractedName: "BWD"
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
                if (_ChipLoadingCameraviewer != null && InputStage?.StageCamera != null)
                {
                    if (_ChipLoadingCameraviewer.Camera != InputStage.StageCamera)
                        _ChipLoadingCameraviewer.Camera = InputStage.StageCamera;
                    try { InputStage.StageCamera.StartLive(); } catch { }
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
                InputStage        = TryGetUnit<InputStage>("InputStage");
                InputStageEjector = TryGetUnit<InputStageEjector>("InputStageEjector");
                InputDieTransfer  = TryGetUnit<InputDieTransfer>("InputDieTransfer");

                if (InputDieTransfer != null)
                {
                    manualSequenceControl.ParentUnit = InputDieTransfer; // 시퀀스 등록 대상 유닛 지정
                }
            }
            catch { }
        }
        #endregion

        #region Events
        private void ChipLoading_Working_FormClosing(object sender, FormClosingEventArgs e)
        {
            System.Threading.Tasks.Task.Delay(500).Wait();
            
        }

        private void _btnVisionSetting_Click(object sender, EventArgs e)
        {
           
        }
        #endregion

        
        
    }
}