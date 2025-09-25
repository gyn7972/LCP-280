using QMC.Common;
using QMC.LCP_280.Process.Component; // DIO / teaching controls
using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static QMC.LCP_280.Process.Unit.RotaryConfig.IO;
using Timer = System.Windows.Forms.Timer;

namespace QMC.LCP_280.Process.Unit.FormWork
{
    [FormOrder(3)]
    /// <summary>
    /// 공정 메인 Working Form
    ///  - TeachingPositionControl : IndexLoadAligner / IndexUnloadAligner / IndexChipProber / IndexChipProbeController / Rotary 등록
    ///  - DIO 제어 : 각 Unit 의 Vacuum / Clamp / Expander / Probe 관련 On/Off 메서드 패턴 자동 바인딩 (Reflection 기반)
    ///  - ManualSequenceControl : 향후 시퀀스(Align / Probe / Transfer 등) 등록을 위한 자리만 확보
    /// </summary>
    public partial class Process_Working : Form
    {
        private Equipment Equipment => Equipment.Instance;

        private IndexChipProbeController IndexChipProbeController;
        private IndexChipProber IndexChipProber;
        private IndexLoadAligner IndexLoadAligner;
        private IndexUnloadAligner IndexUnloadAligner;
        private Rotary Rotary;

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
            IndexChipProbeController = probeController;
            IndexChipProber = prober;
            IndexLoadAligner = loadAligner;
            IndexUnloadAligner = unloadAligner;
            Rotary = rotary;

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

        #region Camera
        private void BindCamera()
        {
            try
            {
                if (_ProcessCameraviewer != null && IndexUnloadAligner?.IndexOutCamera!= null)
                {
                    if (_ProcessCameraviewer.Camera != IndexUnloadAligner.IndexOutCamera)
                        _ProcessCameraviewer.Camera = IndexUnloadAligner.IndexOutCamera;
                    try { IndexUnloadAligner.IndexOutCamera.StartLive(); } catch { }
                    try { _ProcessCameraviewer.StartUpdateTask(); } catch { }
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
                Rotary = TryGetUnit<Rotary>("Rotary");
                IndexLoadAligner = TryGetUnit<IndexLoadAligner>("IndexLoadAligner");
                IndexChipProbeController = TryGetUnit<IndexChipProbeController>("IndexChipProbeController");
                IndexChipProber = TryGetUnit<IndexChipProber>("IndexChipProber");
                IndexUnloadAligner = TryGetUnit<IndexUnloadAligner>("IndexUnloadAligner");

                if (Rotary != null)
                {
                    manualSequenceControlProcessSeq.ParentUnit = Rotary; // 시퀀스 등록 대상 유닛 지정
                }

                if (IndexLoadAligner != null)
                {
                    manualSequenceControl.ParentUnit = IndexLoadAligner; // 시퀀스 등록 대상 유닛 지정
                }
                if(IndexChipProbeController != null)
                {
                    manualSequenceControlProbe.ParentUnit = IndexChipProbeController;
                }
                if(IndexUnloadAligner != null)
                {
                    manualSequenceControlOutAlign.ParentUnit = IndexUnloadAligner;
                }

            }
            catch { }
        }
        #endregion

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
                BindCamera();
                InitSequences();
                SetupStatusTimer();

                InitSocketIndexCombo();
            }
            catch { }
        }

        private Timer _statusTimer;
        private int _statusTimerBusy; // 재진입 방지 플래그
        private void SetupStatusTimer()
        {
            try
            {
                if (_statusTimer != null) return;
                _statusTimer = new Timer();
                _statusTimer.Interval = 500; // ms 단위 (필요 시 조정)
                _statusTimer.Tick += StatusTimer_Tick;
                _statusTimer.Start();
            }
            catch { }
        }

        // ====== 추가: Tick 이벤트 핸들러 ======
        private void StatusTimer_Tick(object sender, EventArgs e)
        {
            if (IsDisposed || Disposing) return;
            if (Interlocked.Exchange(ref _statusTimerBusy, 1) == 1) return; // 재진입 방지
            try
            {
                // 예시: 현재 로드 위치 인덱스 라벨 업데이트
                try
                {
                    if (labelsocketNumberInput != null)
                        labelsocketNumberInput.Text = Rotary?.GetLoadIndexNo().ToString() ?? "-";
                }
                catch { }

                // 필요 시 다른 상태 UI 업데이트 영역 추가
                // e.g. 장치 상태, 시퀀스 진행도, 알람 여부 등
            }
            finally
            {
                Interlocked.Exchange(ref _statusTimerBusy, 0);
            }
        }

        #region Teaching Positions
        private void BindTeachingPositions()
        {
            try
            {
                if (teachingPositionControl == null) return;
                teachingPositionControl.ClearUnits();

                if (IndexLoadAligner != null)
                {
                    teachingPositionControl.RegisterUnit(
                        "IndexLoadAligner",
                        IndexLoadAligner,
                        () => IndexLoadAligner.Config?.TeachingPositions,
                        (name, vel) => IndexLoadAligner.MoveToTeachingPosition(name, vel: vel),
                        tp => IndexLoadAligner.Config?.SetTeachingPosition(tp),
                        autoReload: false);
                }

                if (IndexUnloadAligner != null)
                {
                    //teachingPositionControl.RegisterUnit(
                    //    "IndexUnloadAligner",
                    //    IndexUnloadAligner,
                    //    () => IndexUnloadAligner.Config?.TeachingPositions,
                    //    (name, vel) => IndexUnloadAligner.MoveToTeachingPosition(name, vel: vel),
                    //    tp => IndexUnloadAligner.Config?.SetTeachingPosition(tp),
                    //    autoReload: false);
                }

                if (IndexChipProber != null)
                {
                    teachingPositionControl.RegisterUnit(
                        "IndexChipProber",
                        IndexChipProber,
                        () => IndexChipProber.Config?.TeachingPositions,
                        (name, vel) => IndexChipProber.MoveToTeachingPosition(name, vel: vel),
                        tp => IndexChipProber.Config?.SetTeachingPosition(tp),
                        autoReload: false);
                }

                if (IndexChipProbeController != null)
                {
                    teachingPositionControl.RegisterUnit(
                        "IndexChipProbeController",
                        IndexChipProbeController,
                        () => IndexChipProbeController.Config?.TeachingPositions,
                        (name, vel) => IndexChipProbeController.MoveToTeachingPosition(name, vel: vel),
                        tp => IndexChipProbeController.Config?.SetTeachingPosition(tp),
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

        #region DIO Binding / Reflection Helpers
        private void BindDioControls()
        {
            if (dioControl == null) return;

            dioControl.IoSortMode = QMC.LCP_280.Process.Component.DIOControl.SortingMode.Insertion;

            try
            {
                BindRotaryActuators(Rotary);

                if (IndexChipProbeController != null)
                {
                    dioControl.BindDIOInput(() => false, "---- ProbeController ----", "SEP_ProbeCtrl");

                    dioControl.BindDIOInput(() => IndexChipProbeController.ProbeVacOk(), "ProbeVac OK", "ProbeVacOk");
                    dioControl.BindDIOInput(() => IndexChipProbeController.IsSphereForward(), "Sphere FW Sns", "ProbeSphereFwSns");
                    dioControl.BindDIOInput(() => IndexChipProbeController.IsSphereBackward(), "Sphere BW Sns", "ProbeSphereBwSns");

                    //dioControl.BindDIOOutput(
                    //    () => _probeControllerUnit.SetProbeVacValve(true),
                    //    () => _probeControllerUnit.SetProbeVacValve(false),
                    //    "ProbeVac Valve",
                    //    () => _probeControllerUnit.IsProbeVacValveOn(),
                    //    "ProbeVac");

                    dioControl.BindVacuum(
                        label: "Vacuum",
                        on: () => IndexChipProbeController.SetProbeVac(true),
                        off: () => IndexChipProbeController.SetProbeVac(false),
                        isOk: () => IndexChipProbeController.IsProbeVacValveOn(),
                        isOnState: () => IndexChipProbeController.IsProbeVacValveOn(),
                        displayKey: "ProbeVac",
                        showOkSensor: false // 위에서 OK 센서를 이미 표시했으므로 중복 방지
                    );

                    dioControl.BindCylinder(
                        label: "SphereFB",
                        extend: () => IndexChipProbeController.SetSphereFB(true),
                        retract: () => IndexChipProbeController.SetSphereFB(false),
                        // FWD 센서만 있어도 동작. BWD는 없으면 null 가능(토글은 FWD 센서로 판단)
                        isExtended: () => IndexChipProbeController.IsSphereFwdValveOn(),
                        isRetracted: null,
                        displayKey: "SphereFB",
                        showSensors: false,
                        extendedName: "FWD",
                        retractedName: "BWD"
                    );


                    //dioControl.BindDIOOutput(
                    //    () => _probeControllerUnit.SetSphereFwdValve(true),
                    //    () => _probeControllerUnit.SetSphereFwdValve(false),
                    //    "Sphere FWD Valve",
                    //    () => _probeControllerUnit.IsSphereFwdValveOn(),
                    //    "ProbeSphereFwd");
                    //dioControl.BindDIOOutput(
                    //    () => _probeControllerUnit.SetSphereBwdValve(true),
                    //    () => _probeControllerUnit.SetSphereBwdValve(false),
                    //    "Sphere BWD Valve",
                    //    () => _probeControllerUnit.IsSphereBwdValveOn(),
                    //    "ProbeSphereBwd");
                }

                dioControl.RebuildLists();
            }
            catch { }
        }

        private void BindRotaryActuators(Rotary rotary)
        {
            // 그룹 구분선: Rotary (유지)
            if (Rotary != null)
            {
                dioControl.BindDIOInput(() => false, "---- Rotary ----", "SEP_Rotary");
                dioControl.BindDIOInput(() => Rotary.AirTankPressureOk(), "Rot AirTank OK", "Rot_AirTk");
                dioControl.BindDIOInput(() => Rotary.VacTankPressureOk(), "Rot VacTank OK", "Rot_VacTk");

                int slotCount = SLOT_VAC.Length; // 8
                for (int slot = 0; slot < slotCount; slot++)
                {
                    int idx = slot;
                    string labelBase = $"Index{idx + 1}";

                    dioControl.BindDIOInput(
                        () => Rotary.SlotFlowOk(idx),
                        $"IndexSlot{idx + 1} FLOW",
                        $"Index_S{idx + 1}_Flow");

                    // VAC: 소프트 래치 토글 사용 (isOnState: null)
                    dioControl.BindVacuum(
                        label: $"{labelBase} VAC",
                        on: () => Rotary.SetVacuum(idx, true),
                        off: () => Rotary.SetVacuum(idx, false),
                        isOk: null,
                        isOnState: null,
                        displayKey: $"IndexSlot{idx + 1}_Vac",
                        showOkSensor: false
                    );

                    // BLOW
                    dioControl.BindVacuum(
                        label: $"{labelBase} Blow",
                        on: () => Rotary.SetBlow(idx, true),
                        off: () => Rotary.SetBlow(idx, false),
                        isOk: null,
                        isOnState: null,
                        displayKey: $"IndexSlot{idx + 1}_Blow",
                        showOkSensor: false
                    );

                    // VENT
                    dioControl.BindVacuum(
                        label: $"{labelBase} Vent",
                        on: () => Rotary.SetVent(idx, true),
                        off: () => Rotary.SetVent(idx, false),
                        isOk: null,
                        isOnState: null,
                        displayKey: $"IndexSlot{idx + 1}_Vent",
                        showOkSensor: false
                    );
                }
            }
        }
        #endregion

        private void Process_Working_FormClosing(object sender, FormClosingEventArgs e)
        {
        }

        // 추가: 소켓 번호 콤보 초기화 (1~8)
        private void InitSocketIndexCombo()
        {
            try
            {
                if (comboBoxIndexSocketNo == null) return;
                if (comboBoxIndexSocketNo.Items.Count == 0)
                {
                    for (int i = 1; i <= 8; i++)
                        comboBoxIndexSocketNo.Items.Add(i);
                    comboBoxIndexSocketNo.SelectedIndex = 0;
                }
            }
            catch { }
        }

        private async void btnInputMAlign_ClickAsync(object sender, EventArgs e)
        {
            var ask = new MessageBoxYesNo();
            if (ask.ShowDialog("확인", "구동 하시겠습니까?") != DialogResult.Yes)
                return;

            if (Equipment == null)
                return;

        }

        private void comboBoxIndexSocketNo_SelectedIndexChanged(object sender, EventArgs e)
        {
            //Rotary.
        }

        private void btnRotary_Click(object sender, EventArgs e)
        {
            var ask = new MessageBoxYesNo();
            if (ask.ShowDialog("확인", "다음 소켓으로 구동 하시겠습니까?") != DialogResult.Yes)
                return;

            Rotary.Rotate();
        }
    }
}
