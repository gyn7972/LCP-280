using QMC.Common;
using QMC.Common.Cameras;
using QMC.LCP_280.Process.Component;
using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using static QMC.LCP_280.Process.Unit.OutputDieTransferConfig.IO; // ODT IO 상수
using static QMC.LCP_280.Process.Unit.RotaryConfig.IO;             // Rotary IO 상수/배열

namespace QMC.LCP_280.Process.Unit.FormWork
{
    [FormOrder(5)]
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

        private Rotary Rotary { get; set; }
        private IndexUnloadAligner IndexUnloadAligner { get; set; }
        private OutputDieTransfer OutputDieTransfer { get; set; }
        private OutputStage OutputStage { get; set; }
        
        

        private bool _initialized;          // Text/핸들 설정 여부
        private bool _preloadRequested;     // Preload 1회 보장
        private bool _deferredInitDone;     // 무거운 바인딩 지연 수행 여부
        private bool _isLayoutEditMode;

        public ChipUnloading_Working() : this(
            TryGetUnit<Rotary>("Rotary"),
            TryGetUnit<IndexUnloadAligner>("IndexUnloadAligner"),
            TryGetUnit<OutputDieTransfer>("OutputDieTransfer"), 
            TryGetUnit<OutputStage>("OutputStage")
            
            )
        {
        }

        public ChipUnloading_Working(Rotary rotaty,
            IndexUnloadAligner indexUnloadAligner,
            OutputDieTransfer outputDieTransfer,
            OutputStage outputStage)
        {
            InitializeComponent();
            Rotary = rotaty;
            IndexUnloadAligner = indexUnloadAligner;
            OutputDieTransfer = outputDieTransfer;
            OutputStage = outputStage;

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

                if (OutputStage != null)
                {
                    teachingPositionControl.RegisterUnit(
                        "OutputStage",
                        OutputStage,
                        () => OutputStage.Config?.TeachingPositions,
                        (name, vel) => OutputStage.MoveToTeachingPosition(name, vel: vel),
                        tp => OutputStage.Config?.SetTeachingPosition(tp),
                        autoReload: false);
                }

                if (OutputDieTransfer != null)
                {
                    teachingPositionControl.RegisterUnit(
                        "OutputDieTransfer",
                        OutputDieTransfer,
                        () => OutputDieTransfer.Config?.TeachingPositions,
                        (name, vel) => OutputDieTransfer.MoveToTeachingPosition(name, vel: vel),
                        tp => OutputDieTransfer.Config?.SetTeachingPosition(tp),
                        autoReload: false);
                }

                if (Rotary != null)
                {
                    teachingPositionControl.RegisterUnit(
                        "Rotary",
                        Rotary,
                        () => Rotary.Config?.TeachingPositions,
                        (name, vel) => Rotary.MoveToTeachingPosition(name, vel: vel),
                        tp => Rotary.Config?.SetTeachingPosition(tp),
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
                if (OutputDieTransfer != null)
                {
                    dioControl.BindDIOInput(() => false, "---- OutputDieTransfer ----", "SEP_ODT");

                    for (int arm = 0; arm < 1; arm++) // 필요 시 4로 변경: arm < 4
                    {
                        int idx = arm;
                        string labelBase = $"ODT Arm{idx + 1}";

                        // Flow 센서(입력) 표시
                        dioControl.BindDIOInput(
                            () => OutputDieTransfer.IsVacuumOK(idx),
                            $"{labelBase} Flow OK(Sns)",
                            $"ODT_Arm{idx + 1}_FlowOk");

                        // VAC: 소프트 래치 토글 사용 (isOnState: null)
                        dioControl.BindVacuum(
                            label: $"{labelBase} VAC",
                            on: () => OutputDieTransfer.SetVacuum(idx, true),
                            off: () => OutputDieTransfer.SetVacuum(idx, false),
                            isOk: null,
                            isOnState: null,
                            displayKey: $"ODT_Arm{idx + 1}_Vac",
                            showOkSensor: false
                        );

                        // BLOW
                        dioControl.BindVacuum(
                            label: $"{labelBase} Blow",
                            on: () => OutputDieTransfer.SetBlow(idx, true),
                            off: () => OutputDieTransfer.SetBlow(idx, false),
                            isOk: null,
                            isOnState: null,
                            displayKey: $"ODT_Arm{idx + 1}_Blow",
                            showOkSensor: false
                        );

                        // VENT
                        dioControl.BindVacuum(
                            label: $"{labelBase} Vent",
                            on: () => OutputDieTransfer.SetVent(idx, true),
                            off: () => OutputDieTransfer.SetVent(idx, false),
                            isOk: null,
                            isOnState: null,
                            displayKey: $"ODT_Arm{idx + 1}_Vent",
                            showOkSensor: false
                        );
                    }
                }

                dioControl.RebuildLists();
            }
            catch { }
        }

        private void StrongBindOutputStage()
        {
            if (OutputStage == null || dioControl == null) return;
            try
            {
                // ===== Sensors =====
                dioControl.BindDIOInput(() => OutputStage.IsVacuumOn(), "Vacuum OK(Sns)", "OutStageVacOk");
                dioControl.BindDIOInput(() => OutputStage.IsPlateUp(), "Plate UP Sns", "OutStagePlateUp");
                dioControl.BindDIOInput(() => OutputStage.IsPlateDown(), "Plate DOWN Sns", "OutStagePlateDn");
                dioControl.BindDIOInput(() => OutputStage.IsClampLiftDown(), "ClampLift DOWN Sns", "OutStageLiftDn");
                dioControl.BindDIOInput(() => OutputStage.IsClampFwd(), "Clamp FWD Sns", "OutStageClampFwd");
                dioControl.BindDIOInput(() => OutputStage.Ring0(), "Ring Sns 0", "OutStageRing0");
                dioControl.BindDIOInput(() => OutputStage.Ring1(), "Ring Sns 1", "OutStageRing1");
                dioControl.BindDIOInput(() => OutputStage.IsRingPresent(), "Ring Any", "OutStageRingAny");

                // Vacuum: 도메인/상태 함수로 표준화 (출력은 입력과 무관하게 동작)
                dioControl.BindVacuum(
                    label: "Vacuum",
                    on: () => OutputStage.SetVacuum(true),
                    off: () => OutputStage.SetVacuum(false),
                    isOk: () => OutputStage.IsVacuumOn(),
                    isOnState: () => OutputStage.IsVacuumValveOn(),
                    displayKey: "StageVacValve",
                    showOkSensor: false // 위에서 OK 센서 이미 표시
                );

                // Plate Up/Down: 도메인 함수 사용, 상태 판단은 IsPlateUp 기준
                dioControl.BindCylinder(
                    label: "PlateUpDn",
                    extend: () => OutputStage.SetClampPlate(true),
                    retract: () => OutputStage.SetClampPlate(false),
                    isExtended: () => OutputStage.IsPlateUp(),
                    isRetracted: () => OutputStage.IsPlateDown(),
                    displayKey: "StagePlateUpDn",
                    showSensors: false // 위에서 Up/Down 센서를 이미 표시했으므로 중복 방지
                );

                // ClampLift Up/Down
                dioControl.BindCylinder(
                    label: "ClampLift",
                    extend: () => OutputStage.SetClampLift(true),
                    retract: () => OutputStage.SetClampLift(false),
                    // Up 센서가 없으면 밸브 상태 사용, Down은 센서 사용
                    isExtended: () => OutputStage.IsClampLiftUp(),
                    isRetracted: () => OutputStage.IsClampLiftDown(),
                    displayKey: "StageClampUpDn",
                    showSensors: false,
                    extendedName: "UP",
                    retractedName: "DOWN"
                );

                // Clamp FWD/BWD
                dioControl.BindCylinder(
                    label: "ClampFB",
                    extend: () => OutputStage.SetClampFB(true),
                    retract: () => OutputStage.SetClampFB(false),
                    // FWD 센서만 있어도 동작. BWD는 없으면 null 가능(토글은 FWD 센서로 판단)
                    isExtended: () => OutputStage.IsClampFwd(),
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
                if (_ChipUnloadingCameraviewer != null && IndexUnloadAligner?.IndexOutCamera != null)
                {
                    if (_ChipUnloadingCameraviewer.Camera != IndexUnloadAligner.IndexOutCamera)
                        _ChipUnloadingCameraviewer.Camera = IndexUnloadAligner.IndexOutCamera;
                    try { IndexUnloadAligner.IndexOutCamera.StartLive(); } catch { }
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
                OutputDieTransfer = TryGetUnit<OutputDieTransfer>("OutputDieTransfer");
                Rotary = TryGetUnit<Rotary>("Rotary");
                IndexUnloadAligner = TryGetUnit<IndexUnloadAligner>("IndexUnloadAligner");
                OutputStage = TryGetUnit<OutputStage>("OutputStage");

                if (OutputDieTransfer != null)
                {
                    manualSequenceControl.ParentUnit = OutputDieTransfer; // 시퀀스 등록 대상 유닛 지정
                }

            }
            catch { }
        }
        #endregion

        private void ChipUnloading_Working_FormClosing(object sender, FormClosingEventArgs e)
        {
            try { }
            catch { }
        }
    }
}
