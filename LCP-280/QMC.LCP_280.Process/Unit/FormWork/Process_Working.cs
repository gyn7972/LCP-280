using QMC.Common;
using QMC.LCP_280.Process.Component; // DIO / teaching controls
using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
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

                InitSocketIndexCombo();
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
                    teachingPositionControl.RegisterUnit(
                        "IndexUnloadAligner",
                        IndexUnloadAligner,
                        () => IndexUnloadAligner.Config?.TeachingPositions,
                        (name, vel) => IndexUnloadAligner.MoveToTeachingPosition(name, vel: vel),
                        tp => IndexUnloadAligner.Config?.SetTeachingPosition(tp),
                        autoReload: false);
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
            var unitName = "IndexLoadAligner";
            int selSocket = 1;

            CancellationTokenSource cts = null;
            try
            {
                if (comboBoxIndexSocketNo != null)
                {
                    if (comboBoxIndexSocketNo.SelectedItem is int v)
                        selSocket = v;                  // 1~8 그대로
                    else if (comboBoxIndexSocketNo.SelectedIndex >= 0)
                        selSocket = comboBoxIndexSocketNo.SelectedIndex + 1;
                }
            }
            catch { selSocket = 1; }

            try
            {
                btnInputMAlign.Enabled = false;

                // 선택 값 전달
                IndexLoadAligner.ManualSocketIndex = selSocket;

                IndexLoadAligner.ManualState = Common.Unit.BaseUnit.ProcessState.Manual;
                IndexLoadAligner.StepManual = 1;
                var result = await Equipment.StartUnitAsync(unitName);

                cts = new CancellationTokenSource();
                int rc = await IndexLoadAligner.WaitManualStepAsync(1, cts.Token);

                if (rc == 0)
                    MessageBox.Show(this, "Step1 완료", "OK",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                else
                    MessageBox.Show(this, $"Step1 실패 (rc={rc})", "Fail",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (OperationCanceledException)
            {
                MessageBox.Show(this, "취소됨", "Canceled",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex) { Log.Write(ex); MessageBox.Show(this, ex.Message, "예외", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            finally
            {
                btnInputMAlign.Enabled = true;
                cts?.Dispose();
            }
        }
    }
}
