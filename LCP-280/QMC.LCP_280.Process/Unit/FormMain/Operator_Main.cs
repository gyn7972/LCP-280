using QMC.Common;
using QMC.Common.Controls;
using QMC.Common.UI;
using QMC.Common.Unit;
using QMC.LCP_280.Process.Component;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using static QMC.LCP_280.Process.Unit.FormMain.SequenceAutoControl;
using static QMC.LCP_280.Process.Unit.FormMain.SequenceManualControl;

namespace QMC.LCP_280.Process.Unit.FormMain
{
    [FormOrder(1)]
    public partial class Operator_Main : Form
    {
        // Units
        private InputCassetteLifter InputCassetteLifter { get; set; }
        private InputFeeder InputFeeder { get; set; }
        private InputStage InputStage { get; set; }
        private InputDieTransfer InputDieTransfer { get; set; }
        private Rotary Rotary { get; set; }
        private IndexLoadAligner IndexLoadAligner { get; set; }
        private IndexChipProbeController IndexChipProbeController { get; set; }
        private IndexUnloadAligner IndexUnloadAligner { get; set; }
        private OutputDieTransfer OutputDieTransfer { get; set; }
        private OutputStage OutputStage { get; set; }
        private OutputFeeder OutputFeeder{ get; set; }
        private OutputCassetteLifter OutputCassetteLifter { get; set; }

        // State
        private bool _initialized;
        private bool _preloadRequested;
        private bool _deferredInitDone;
        private bool _isLayoutEditMode;

        // Auto Sequence 상태
        private bool _autoReady = false;
        private bool _autoStarting = false;

        // Manual Sequence 상태
        private HashSet<string> _readySequences;
        private HashSet<string> _startSequences;

        // Jog Popup
        private Form_AxisJogPopup _jogPopup = null;
        private AxisPostionPopup _axisPosPopup = null;


        public Operator_Main() : this(
            TryGetUnit<InputFeeder>("InputFeeder"),
            TryGetUnit<InputDieTransfer>("InputDieTransfer"),
            TryGetUnit<Rotary>("Rotary"),
            TryGetUnit<OutputDieTransfer>("OutputDieTransfer"),
            TryGetUnit<OutputFeeder>("OutputFeeder"),
            TryGetUnit<InputStage>("InputStage"),
            TryGetUnit<IndexUnloadAligner>("IndexUnloadAligner"),
            TryGetUnit<OutputStage>("OutputStage"),

            TryGetUnit<InputCassetteLifter>("InputCassetteLifter"),
            TryGetUnit<IndexLoadAligner>("IndexLoadAligner"),
            TryGetUnit<IndexChipProbeController>("IndexChipProbeController"),
            TryGetUnit<OutputCassetteLifter>("OutputCassetteLifter"))
        {
        }

        public Operator_Main(InputFeeder inputFeeder, InputDieTransfer inputDieTransfer, Rotary rotary,
                            OutputDieTransfer outputDieTransfer, OutputFeeder outputFeeder,
                            InputStage inputStage, IndexUnloadAligner indexUnloadAligner, OutputStage outputStage,
                            InputCassetteLifter inputCassetteLifter, IndexLoadAligner indexLoadAligner,
                            IndexChipProbeController indexChipProbeController, OutputCassetteLifter outputCassetteLifter)
        {
            InitializeComponent();

            InputFeeder = inputFeeder;
            InputDieTransfer = inputDieTransfer;
            Rotary = rotary;
            OutputDieTransfer = outputDieTransfer;
            OutputFeeder = outputFeeder;
            InputStage = inputStage;
            IndexUnloadAligner = indexUnloadAligner;
            OutputStage = outputStage;

            InputCassetteLifter = inputCassetteLifter;
            IndexLoadAligner = indexLoadAligner;
            IndexChipProbeController = indexChipProbeController;
            OutputCassetteLifter = outputCassetteLifter;

            // 상태 초기화
            _readySequences = new HashSet<string>();
            _startSequences = new HashSet<string>();

            Load += Vision_Manual_Load;

            // Control → Form 이벤트 등록
            sequenceAutoControl.SequenceButtonRequested += OnAutoSequenceButtonRequested;
            sequenceManualControl.SequenceButtonRequested += OnManualSequenceButtonRequested;
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

        #region UI 초기화
        public void PreloadUI()
        {
            if (IsDisposed || Disposing) return;
            if (_preloadRequested) return;
            _preloadRequested = true;
            EnsureInitialized();
            var handle = Handle;
        }
        private void Vision_Manual_Load(object sender, EventArgs e) => EnsureInitialized();
        private void EnsureInitialized()
        {
            if (_initialized) return;
            _initialized = true;

            try
            {
                BeginInvoke(new Action(StartDeferredInit));
            }
            catch (Exception ex)
            {
                try
                {
                    Controls.Add(new Label
                    {
                        Dock = DockStyle.Fill,
                        Text = $"Init 실패: {ex.Message}",
                        ForeColor = Color.Red,
                        TextAlign = ContentAlignment.MiddleCenter
                    });
                }
                catch { }
            }
        }
        private async void StartDeferredInit()
        {
            if (_deferredInitDone) return;
            _deferredInitDone = true;
            await System.Threading.Tasks.Task.Delay(30);
            if (IsDisposed || Disposing) return;
            try
            {
                BindCamera();
            }
            catch { }
        }
        #endregion

        #region Camera
        private void BindCamera()
        {
            try
            {
                // Input Stage Camera
                if (InputWaferCamera != null && InputStage?.StageCamera != null)
                {
                    if (InputWaferCamera.Camera != InputStage.StageCamera)
                        InputWaferCamera.Camera = InputStage.StageCamera;
                    try { InputStage.StageCamera.StartLive(); } catch { }
                    try { InputWaferCamera.StartUpdateTask(); } catch { }
                }

                // Index Output Camera
                if (IndexOutputCamera != null && IndexUnloadAligner?.IndexOutCamera != null)
                {
                    if (IndexOutputCamera.Camera != IndexUnloadAligner.IndexOutCamera)
                        IndexOutputCamera.Camera = IndexUnloadAligner.IndexOutCamera;
                    try { IndexUnloadAligner.IndexOutCamera.StartLive(); } catch { }
                    try { IndexOutputCamera.StartUpdateTask(); } catch { }
                }

                // Output Wafer Camera
                if (OutputWaferCamera != null && OutputStage?.OutStageCamera != null)
                {
                    if (OutputWaferCamera.Camera != OutputStage.OutStageCamera)
                        OutputWaferCamera.Camera = OutputStage.OutStageCamera;
                    try { OutputStage.OutStageCamera.StartLive(); } catch { }
                    try { OutputWaferCamera.StartUpdateTask(); } catch { }
                }
            }
            catch { }
        }
        #endregion

        #region Form Cleanup
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // 이벤트 해제
            if (sequenceAutoControl != null)
            {
                sequenceAutoControl.SequenceButtonRequested -= OnAutoSequenceButtonRequested;
            }

            if (sequenceManualControl != null)
            {
                sequenceManualControl.SequenceButtonRequested -= OnManualSequenceButtonRequested;
            }

            base.OnFormClosing(e);
        }
        #endregion


        #region Auto Sequence 처리
        private void OnAutoSequenceButtonRequested(object sender, AutoSequenceEventArgs e)
        {
            Log.Write("Operator_Main", $"Auto Sequence {e.Command} 요청");

            switch (e.Command)
            {
                case "Ready":
                    HandleAutoReady();
                    break;

                case "Start":
                    HandleAutoStart();
                    break;

                case "Stop":
                    HandleAutoStop();
                    break;

                case "CycleStop":
                    HandleAutoCycleStop();
                    break;

                case "Reset":
                    HandleAutoReset();
                    break;
            }
        }

        private void HandleAutoReady()
        {
            // 상태 토글
            _autoReady = !_autoReady;

            // UI 업데이트 (Form → Control)
            NotifyAutoSequenceStateChanged("Ready", _autoReady);

            // 비즈니스 로직 실행
            if (_autoReady)
            {
                ExecuteAutoReady();
                Log.Write("Operator_Main", "Auto Ready ON");
            }
            else
            {
                Log.Write("Operator_Main", "Auto Ready OFF");
            }
        }

        private void HandleAutoStart()
        {
            if (!_autoStarting)
            {
                // Ready 체크
                if (!_autoReady)
                {
                    MessageBox.Show("Auto Ready를 먼저 실행해주세요.");
                    return;
                }

                // 상태 변경
                _autoReady = false;
                _autoStarting = true;

                // UI 업데이트 (Form → Control)
                NotifyAutoSequenceStateChanged("Ready", false);
                NotifyAutoSequenceStateChanged("Start", true);

                // 비즈니스 로직 실행
                ExecuteAutoStart();
                Log.Write("Operator_Main", "Auto Start 실행 (Ready OFF)");
            }
            else
            {
                // Start OFF
                _autoStarting = false;

                // UI 업데이트
                NotifyAutoSequenceStateChanged("Start", false);

                Log.Write("Operator_Main", "Auto Start OFF");
            }
        }

        private void HandleAutoStop()
        {
            // 모든 상태 초기화
            _autoReady = false;
            _autoStarting = false;
            _readySequences.Clear();
            _startSequences.Clear();

            // Auto Control UI 초기화
            sequenceAutoControl.ResetAllButtons();
            // Manual Control UI 초기화
            sequenceManualControl.ResetAllButtons();

            // Stop 버튼 깜빡임
            NotifyAutoSequenceStateChanged("Stop", true);
            System.Threading.Tasks.Task.Delay(500).ContinueWith(_ =>
            {
                this.Invoke(new Action(() =>
                {
                    NotifyAutoSequenceStateChanged("Stop", false);
                }));
            });

            // 비즈니스 로직 실행
            ExecuteAutoStop();
            Log.Write("Operator_Main", "Auto Stop 실행 - 모든 Sequence 초기화");
        }

        private void HandleAutoCycleStop()
        {
            // CycleStop 버튼 깜빡임
            NotifyAutoSequenceStateChanged("CycleStop", true);
            System.Threading.Tasks.Task.Delay(500).ContinueWith(_ =>
            {
                this.Invoke(new Action(() =>
                {
                    NotifyAutoSequenceStateChanged("CycleStop", false);
                }));
            });

            // 비즈니스 로직 실행
            ExecuteAutoCycleStop();
            Log.Write("Operator_Main", "Auto CycleStop 실행");
        }

        private void HandleAutoReset()
        {
            // Reset 버튼 깜빡임
            NotifyAutoSequenceStateChanged("Reset", true);
            System.Threading.Tasks.Task.Delay(500).ContinueWith(_ =>
            {
                this.Invoke(new Action(() =>
                {
                    NotifyAutoSequenceStateChanged("Reset", false);
                }));
            });

            // 비즈니스 로직 실행
            ExecuteAutoReset();
            Log.Write("Operator_Main", "Auto Reset 실행");
        }

        /// <summary>
        /// Control에 상태 변경 알림 (Form → Control)
        /// </summary>
        private void NotifyAutoSequenceStateChanged(string command, bool isActive)
        {
            sequenceAutoControl.OnAutoSequenceStateChanged(new AutoSequenceStateChangedEventArgs
            {
                Command = command,
                IsActive = isActive
            });
        }
        #endregion



        #region Manual Sequence 처리
        private void OnManualSequenceButtonRequested(object sender, SequenceEventArgs e)
        {
            Log.Write("Operator_Main", $"Manual Sequence {e.SequenceName} {e.Action} 요청");

            if (e.Action == "Ready")
            {
                HandleReadyAction(e.SequenceName);
            }
            else if (e.Action == "Start")
            {
                HandleStartAction(e.SequenceName);
            }
        }

        private void HandleReadyAction(string sequenceName)
        {
            // Ready 상태 토글
            bool isActive = !_readySequences.Contains(sequenceName);
            if (isActive)
            {
                _readySequences.Add(sequenceName);
                ExecuteSequence(sequenceName, "Ready");
                Log.Write("Operator_Main", $"{sequenceName} Ready ON");
            }
            else
            {
                _readySequences.Remove(sequenceName);
                Log.Write("Operator_Main", $"{sequenceName} Ready OFF");
            }

            // UI 업데이트 (Form → Control)
            NotifySequenceStateChanged(sequenceName, "Ready", isActive, false);
        }

        private void HandleStartAction(string sequenceName)
        {
            // Start 상태 토글
            bool isStarting = _startSequences.Contains(sequenceName);

            if (isStarting)
            {
                // Start OFF
                _startSequences.Remove(sequenceName);
                NotifySequenceStateChanged(sequenceName, "Start", false, true);
                Log.Write("Operator_Main", $"{sequenceName} Start OFF");
            }
            else
            {
                // Ready 체크
                if (!_readySequences.Contains(sequenceName))
                {
                    MessageBox.Show($"{sequenceName}를 먼저 Ready 상태로 만들어주세요.");
                    return;
                }

                // 상태 변경
                _readySequences.Remove(sequenceName);
                _startSequences.Add(sequenceName);

                // UI 업데이트 (Ready OFF, Start ON)
                NotifySequenceStateChanged(sequenceName, "Ready", false, false);
                NotifySequenceStateChanged(sequenceName, "Start", true, true);

                // 비즈니스 로직 실행
                ExecuteSequence(sequenceName, "Start");
                Log.Write("Operator_Main", $"{sequenceName} Start ON (Ready OFF)");
            }
        }

        /// <summary>
        /// Control에 상태 변경 알림 (Form → Control)
        /// </summary>
        private void NotifySequenceStateChanged(string sequenceName, string action, bool isActive, bool updateText)
        {
            sequenceManualControl.OnSequenceStateChanged(new SequenceStateChangedEventArgs
            {
                SequenceName = sequenceName,
                Action = action,
                IsActive = isActive,
                UpdateText = updateText
            });
        }
        #endregion


        #region 비즈니스 로직 (실제 하드웨어 제어)
        private async void ExecuteAutoReady()
        {
            // Auto Ready 로직 구현
            bool flowControl = await HandleInputWaferReady();
            if (!flowControl)
            {
                Log.Write("Operator_Main", "Auto Ready 실패 - InputWafer Ready 위치 이동 실패");
                return;
            }

            flowControl = await HandleChipLoadingReady();
            if (!flowControl)
            {
                Log.Write("Operator_Main", "Auto Ready 실패 - ChipLoading Ready 위치 이동 실패");
                return;
            }

            flowControl = await HandleProcessReady();
            if (!flowControl)
            {
                Log.Write("Operator_Main", "Auto Ready 실패 - Process Ready 위치 이동 실패");
                return;
            }

            flowControl = await HandleChipUnloadingReady();
            if (!flowControl)
            {
                Log.Write("Operator_Main", "Auto Ready 실패 - ChipUnloading Ready 위치 이동 실패");
                return;
            }

            flowControl = await HandleOutputWaferReady();
            if (!flowControl)
            {
                Log.Write("Operator_Main", "Auto Ready 실패 - OutputWafer Ready 위치 이동 실패");
                return;
            }

            Log.Write("Operator_Main", "Auto Ready 완료 - 모든 Sequence Ready 위치로 이동");
        }

        private async void ExecuteAutoStart()
        {
            // Auto Start 로직 구현
            bool flowControl = await HandleInputStart();
            if (!flowControl)
            {
                Log.Write("Operator_Main", "Auto Start 실패 - InputWafer Start 실패");
                return;
            }
            flowControl = await HandleChipLoadingStart();
            if (!flowControl)
            {
                Log.Write("Operator_Main", "Auto Start 실패 - ChipLoading Start 실패");
                return;
            }
            flowControl = await HandleProcessStart();
            if (!flowControl)
            {
                Log.Write("Operator_Main", "Auto Start 실패 - Process Start 실패");
                return;
            }
            flowControl = await HandleChipUnloadingStart();
            if (!flowControl)
            {
                Log.Write("Operator_Main", "Auto Start 실패 - ChipUnloading Start 실패");
                return;
            }
            flowControl = await HandleOutputWaferStart();
            if (!flowControl)
            {
                Log.Write("Operator_Main", "Auto Start 실패 - OutputWafer Start 실패");
                return;
            }
            Log.Write("Operator_Main", "Auto Start 완료 - 모든 Sequence Start 실행");
        }

        private async void ExecuteAutoStop()
        {
            var equipment = Equipment.Instance;
            // Auto Stop 로직 구현
            var result = await equipment.StopAllUnitsAsync();
        }

        private void ExecuteAutoCycleStop()
        {
            // Auto CycleStop 로직 구현
        }

        private void ExecuteAutoReset()
        {
            // Auto Reset 로직 구현
        }

        private void ExecuteSequence(string sequenceName, string action)
        {
            var sequenceHandlers = new Dictionary<string, Action<string>>
            {
                { "InputWafer", HandleInputWafer },
                { "ChipLoading", HandleChipLoading },
                { "Process", HandleProcess },
                { "ChipUnloading", HandleChipUnloading },
                { "OutputWafer", HandleOutputWafer }
            };

            if (sequenceHandlers.TryGetValue(sequenceName, out var handler))
            {
                handler(action);
            }
        }

        #region Handle Manual
        // 공통 Ready 이동 함수 (TeachingPosition 이름으로 인덱스 계산 → 이동 → 진행 UI → 결과 확인)
        // 공통 Ready 이동 함수 (TeachingPosition 이름으로 인덱스 계산 → 이동 → 진행 UI → 결과 확인)
        private async System.Threading.Tasks.Task<bool> MoveToTeachingPositionReadyAsync(
            BaseUnit unit,
            string teachingPositionName,
            string progressMessage = "Teaching Position 이동 중...",
            bool isFine = false)
        {
            if (unit == null)
            {
                MessageBox.Show("Unit가 초기화되지 않았습니다.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            int selIndex = -1;
            string unitName = unit.UnitName ?? "Unit";

            try
            {
                var cfg = unit.Config;
                if (cfg == null)
                {
                    MessageBox.Show("Config를 찾을 수 없습니다.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                // Config.GetTeachingPosition(string) → 인덱스 계산
                var tp = cfg.GetTeachingPosition(teachingPositionName);
                var tpListObj = cfg.TeachingPositions;

                if (tp != null && tpListObj is System.Collections.IEnumerable enumerable)
                {
                    int i = 0;
                    foreach (var item in enumerable)
                    {
                        if (Equals(item, tp))
                        {
                            selIndex = i;
                            break;
                        }
                        i++;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                selIndex = -1;
            }

            if (selIndex < 0)
            {
                MessageBox.Show("Ready Teaching Position을 찾을 수 없습니다.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            // MoveTeachingPositionOnceAsync 또는 동기 API 감싸기
            System.Threading.Tasks.Task<int> task = null;
            System.Reflection.MethodInfo moveAsync = null;
            System.Reflection.MethodInfo moveOnce = null;
            System.Reflection.MethodInfo stopOnce = null;

            try
            {
                var unitType = unit.GetType();
                var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic;

                // 정확한 시그니처로 조회하여 추가 모호성 방지
                moveAsync = unitType.GetMethod("MoveTeachingPositionOnceAsync", flags, null, new[] { typeof(int), typeof(bool) }, null);
                if (moveAsync != null)
                {
                    task = (System.Threading.Tasks.Task<int>)moveAsync.Invoke(unit, new object[] { selIndex, isFine });
                }
                else
                {
                    // 비동기 메서드가 없으면 동기 메서드를 Task로 래핑
                    moveOnce = unitType.GetMethod("MoveTeachingPositionOnce", flags, null, new[] { typeof(int), typeof(bool) }, null);
                    if (moveOnce == null)
                    {
                        MessageBox.Show("이동 API를 찾을 수 없습니다.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }

                    task = System.Threading.Tasks.Task.Run(() =>
                    {
                        var r = (int)moveOnce.Invoke(unit, new object[] { selIndex, isFine });
                        return r;
                    });
                }

                stopOnce = unitType.GetMethod("StopTeachingPositionOnce", flags, null, new[] { typeof(int) }, null);
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                MessageBox.Show("이동 작업을 시작할 수 없습니다.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            using (var pf = new ProgressForm(unitName, progressMessage, task))
            {
                var dr = pf.ShowDialog(this); // 모달: 메인 UI 입력 차단
                if (dr == DialogResult.Cancel)
                {
                    try
                    {
                        // 취소 시 중지 시도
                        stopOnce?.Invoke(unit, new object[] { selIndex });
                    }
                    catch { }
                    return false;
                }
            }

            var result = await task; // 완료 결과 수집
            if (result != 0)
            {
                MessageBox.Show("Ready Fail.", "Move", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            return true;
        }
        private async System.Threading.Tasks.Task<bool> HandleInputWaferReady()
        {
            return await MoveToTeachingPositionReadyAsync(
                        InputFeeder,
                        InputFeederConfig.TeachingPositionName.Ready.ToString());
        }
        private async System.Threading.Tasks.Task<bool> HandleChipLoadingReady()
        {
            return await MoveToTeachingPositionReadyAsync(
                        InputDieTransfer,
                        InputDieTransferConfig.TeachingPositionName.SafetyZone.ToString());
        }
        private async System.Threading.Tasks.Task<bool> HandleProcessReady()
        {
            // 1) IndexLoadAligner → SafetyZone
            var ok = await MoveToTeachingPositionReadyAsync(
                IndexLoadAligner,
                IndexLoadAlignerConfig.TeachingPositionName.SafetyZone.ToString());
            if (!ok) return false;

            // 2) IndexChipProbeController → SafetyZone
            ok = await MoveToTeachingPositionReadyAsync(
                IndexChipProbeController,
                IndexChipProbeControllerConfig.TeachingPositionName.SafetyZone.ToString());
            if (!ok) return false;

            return true;
        }
        private async System.Threading.Tasks.Task<bool> HandleChipUnloadingReady()
        {
            return await MoveToTeachingPositionReadyAsync(
                        OutputDieTransfer,
                        OutputDieTransferConfig.TeachingPositionName.SafetyZone.ToString());
        }
        private async System.Threading.Tasks.Task<bool> HandleOutputWaferReady()
        {
            return await MoveToTeachingPositionReadyAsync(
                        OutputFeeder,
                        OutputFeederConfig.TeachingPositionName.Ready.ToString());
        }

        // 공통 Start 함수: 유닛 기본 검사 → 이미 실행 중 여부 확인 → Start 요청 → Cursor 복원
        private async System.Threading.Tasks.Task<bool> TryStartUnitAsync(BaseUnit unit)
        {
            if (unit == null)
            {
                return false;
            }

            try
            {
                var eq = Equipment.Instance;
                if (eq == null)
                {
                    MessageBox.Show("Equipment 인스턴스가 초기화되지 않았습니다.", "오류",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                var unitName = unit.UnitName;
                if (string.IsNullOrEmpty(unitName))
                {
                    MessageBox.Show("UnitName 이 비어있습니다.", "오류",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                if (unit.RunUnitStatus == BaseUnit.UnitStatus.Running)
                {
                    MessageBox.Show($"Unit '{unitName}' 는 이미 실행 중입니다.", "정보",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return false;
                }

                var prev = Cursor.Current;
                Cursor.Current = Cursors.WaitCursor;

                try
                {
                    bool ok = await eq.StartUnitAsync(unitName);
                    if (!ok)
                    {
                        MessageBox.Show($"Unit '{unitName}' 시작 실패.", "오류",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                finally
                {
                    Cursor.Current = prev;
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }

            return true;
        }
        private async System.Threading.Tasks.Task<bool> HandleInputStart()
        {
            return await TryStartUnitAsync(InputFeeder);
        }
        private async System.Threading.Tasks.Task<bool> HandleChipLoadingStart()
        {
            return await TryStartUnitAsync(InputDieTransfer);
        }
        private async System.Threading.Tasks.Task<bool> HandleProcessStart()
        {
            return await TryStartUnitAsync(Rotary);
        }
        private async System.Threading.Tasks.Task<bool> HandleChipUnloadingStart()
        {
            return await TryStartUnitAsync(OutputDieTransfer);
        }
        private async System.Threading.Tasks.Task<bool> HandleOutputWaferStart()
        {
            return await TryStartUnitAsync(OutputFeeder);
        }
        #endregion


        private async void HandleInputWafer(string action)
        {
            if (action == "Ready")
            {
                Log.Write("Operator_Main", "InputWafer Ready 위치로 이동");

                bool flowControl = await HandleInputWaferReady();
                if (!flowControl)
                {
                    return;
                }
            }
            else if (action == "Start")
            {
                Log.Write("Operator_Main", "InputWafer 시퀀스 실행");

                bool flowControl = await HandleInputStart();
                if (!flowControl)
                {
                    return;
                }
            }
        }

        private async void HandleChipLoading(string action)
        {
            if (action == "Ready")
            {
                Log.Write("Operator_Main", "ChipLoading Ready");

                bool flowControl = await HandleChipLoadingReady();
                if (!flowControl)
                {
                    return;
                }
            }
            else if (action == "Start")
            {
                Log.Write("Operator_Main", "ChipLoading Start");
                bool flowControl = await HandleChipLoadingStart();
                if (!flowControl)
                {
                    return;
                }
            }
        }

        private async void HandleProcess(string action)
        {
            if (action == "Ready")
            {
                Log.Write("Operator_Main", "Process Ready");
                bool flowControl = await HandleProcessReady();
                if (!flowControl)
                {
                    return;
                }

            }
            else if (action == "Start")
            {
                Log.Write("Operator_Main", "Process Start");

                bool flowControl = await HandleProcessStart();
                if (!flowControl)
                {
                    return;
                }
            }
        }

        private async void HandleChipUnloading(string action)
        {
            if (action == "Ready")
            {
                Log.Write("Operator_Main", "ChipUnloading Ready");
                bool flowControl = await HandleChipUnloadingReady();
                if (!flowControl)
                {
                    return;
                }

            }
            else if (action == "Start")
            {
                Log.Write("Operator_Main", "ChipUnloading Start");

                bool flowControl = await HandleChipUnloadingStart();
                if (!flowControl)
                {
                    return;
                }
            }
        }

        private async void HandleOutputWafer(string action)
        {
            if (action == "Ready")
            {
                Log.Write("Operator_Main", "OutputWafer Ready 위치로 이동");

                bool flowControl = await HandleOutputWaferReady();
                if (!flowControl)
                {
                    return;
                }

            }
            else if (action == "Start")
            {
                Log.Write("Operator_Main", "OutputWafer 시퀀스 실행");

                bool flowControl = await HandleOutputWaferStart();
                if (!flowControl)
                {
                    return;
                }
            }
        }

        #endregion


        // ===== Jog Popup =====
        private void btn_Jog_Click(object sender, EventArgs e)
        {
            ShowOrRestoreJogPopup(this);
            ShowOrRestoreAxisPosPopup(this);
        }

        private void ShowOrRestoreJogPopup(IWin32Window owner)
        {
            if (_jogPopup == null || _jogPopup.IsDisposed)
            {
                _jogPopup = new Form_AxisJogPopup();
                _jogPopup.StartPosition = FormStartPosition.CenterParent;

                _jogPopup.ShowInTaskbar = true;
                _jogPopup.StartPosition = FormStartPosition.CenterScreen;

                _jogPopup.Owner = null;
                _jogPopup.Load += (s, e) =>
                {
                    TaskbarHelper.SetAppId(_jogPopup.Handle, "MyApp.JogPanel");
                };

                _jogPopup.FormClosed += (s, _) => { _jogPopup = null; };
                _jogPopup.FormClosing += (s, ev) =>
                {
                    if (ev.CloseReason == CloseReason.UserClosing)
                    {
                        ev.Cancel = true;
                        _jogPopup.Hide();
                    }
                };
            }
            if (!_jogPopup.Visible)
            {
                _jogPopup.Show();
            }

            if (_jogPopup.WindowState == FormWindowState.Minimized)
                _jogPopup.WindowState = FormWindowState.Normal;

            _jogPopup.BringToFront();
            _jogPopup.TopMost = true;
            _jogPopup.Activate();
        }

        private void ShowOrRestoreAxisPosPopup(IWin32Window owner)
        {
            if (_axisPosPopup == null || _axisPosPopup.IsDisposed)
            {
                _axisPosPopup = new AxisPostionPopup();
                _axisPosPopup.StartPosition = FormStartPosition.CenterParent;

                _axisPosPopup.ShowInTaskbar = true;
                _axisPosPopup.StartPosition = FormStartPosition.CenterScreen;

                _axisPosPopup.Owner = null;
                _axisPosPopup.Load += (s, e) =>
                {
                    TaskbarHelper.SetAppId(_axisPosPopup.Handle, "MyApp.AxisPosition");
                };

                _axisPosPopup.FormClosed += (s, _) => { _axisPosPopup = null; };
                _axisPosPopup.FormClosing += (s, ev) =>
                {
                    if (ev.CloseReason == CloseReason.UserClosing)
                    {
                        ev.Cancel = true;
                        _axisPosPopup.Hide();
                    }
                };
            }
            if (!_axisPosPopup.Visible)
            {
                _axisPosPopup.Show();
            }

            if (_axisPosPopup.WindowState == FormWindowState.Minimized)
                _axisPosPopup.WindowState = FormWindowState.Normal;

            _axisPosPopup.BringToFront();
            _axisPosPopup.TopMost = true;
            _axisPosPopup.Activate();
        }
    }
}
