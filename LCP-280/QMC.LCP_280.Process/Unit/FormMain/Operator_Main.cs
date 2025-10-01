using QMC.Common;
using QMC.Common.Controls;
using QMC.Common.UI;
using QMC.Common.Unit;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using static QMC.LCP_280.Process.Unit.FormMain.SequenceAutoControl;
using static QMC.LCP_280.Process.Unit.FormMain.SequenceManualControl;

namespace QMC.LCP_280.Process.Unit.FormMain
{
    [FormOrder(2)]
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

        private void ExecuteAutoReady()
        {
            // Auto Ready 로직 구현
        }

        private void ExecuteAutoStart()
        {
            // Auto Start 로직 구현
        }

        private async void ExecuteAutoStop()
        {
            var equipment = Equipment.Instance;
            // Auto Stop 로직 구현
            var result = await equipment.StopAllUnitsAsync(false);

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

        private async void HandleInputWafer(string action)
        {
            if (action == "Ready")
            {
                Log.Write("Operator_Main", "InputWafer Ready 위치로 이동");
                bool isFine = false;
                // 기존 구성 요소만 활용해 Ready 티칭 인덱스 계산
                int selIndex = -1;
                try
                {
                    var cfg = InputFeeder.Config;
                    var tp = cfg?.GetTeachingPosition(InputFeederConfig.TeachingPositionName.Ready.ToString());
                    if (tp != null && cfg.TeachingPositions != null)
                        selIndex = cfg.TeachingPositions.IndexOf(tp);
                }
                catch { selIndex = -1; }

                if (selIndex < 0)
                {
                    MessageBox.Show("Ready Teaching Position을 찾을 수 없습니다.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var task = InputFeeder.MoveTeachingPositionOnceAsync(selIndex, isFine);
                using (var pf = new ProgressForm(InputFeeder.UnitName, "Teaching Position 이동 중...", task))
                {
                    var dr = pf.ShowDialog(this); // 모달: 메인 UI 입력 차단
                    if (dr == DialogResult.Cancel)
                    {
                        InputFeeder.StopTeachingPositionOnce(selIndex);
                        return;
                    }
                }

                var result = await task; // 완료 결과 수집

                if (result == 0)
                {
                    //MessageBox.Show("Ready 완료", "Move", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Ready Fail.", "Move", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            else if (action == "Start")
            {
                Log.Write("Operator_Main", "InputWafer 시퀀스 실행");
                if (InputFeeder == null)
                    return;

                try
                {
                    var eq = Equipment.Instance;
                    if (eq == null)
                    {
                        MessageBox.Show("Equipment 인스턴스가 초기화되지 않았습니다.", "오류",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    var unitName = InputFeeder.UnitName;
                    if (string.IsNullOrEmpty(unitName))
                    {
                        MessageBox.Show("UnitName 이 비어있습니다.", "오류",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    // 이미 실행 중인지 간단 체크 (RunStatus 사용 가능 시)
                    if (InputFeeder.RunUnitStatus == BaseUnit.UnitStatus.Running)
                    {
                        MessageBox.Show($"Unit '{unitName}' 는 이미 실행 중입니다.", "정보",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    Cursor prev = Cursor.Current;
                    Cursor.Current = Cursors.WaitCursor;

                    bool ok = await eq.StartUnitAsync(unitName);
                    if (!ok)
                    {
                        MessageBox.Show($"Unit '{unitName}' 시작 실패.", "오류",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    Cursor.Current = prev;
                }
                catch (Exception ex)
                {
                    Log.Write(ex);
                }
            }
        }

        private async void HandleChipLoading(string action)
        {
            if (action == "Ready")
            {
                Log.Write("Operator_Main", "ChipLoading Ready");
                bool isFine = false;
                // 기존 구성 요소만 활용해 Ready 티칭 인덱스 계산
                int selIndex = -1;
                try
                {
                    var cfg = InputDieTransfer.Config;
                    var tp = cfg?.GetTeachingPosition(InputDieTransferConfig.TeachingPositionName.SafetyZone.ToString());
                    if (tp != null && cfg.TeachingPositions != null)
                        selIndex = cfg.TeachingPositions.IndexOf(tp);
                }
                catch { selIndex = -1; }

                if (selIndex < 0)
                {
                    MessageBox.Show("Ready Teaching Position을 찾을 수 없습니다.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var task = InputDieTransfer.MoveTeachingPositionOnceAsync(selIndex, isFine);
                using (var pf = new ProgressForm(InputDieTransfer.UnitName, "Teaching Position 이동 중...", task))
                {
                    var dr = pf.ShowDialog(this); // 모달: 메인 UI 입력 차단
                    if (dr == DialogResult.Cancel)
                    {
                        InputDieTransfer.StopTeachingPositionOnce(selIndex);
                        return;
                    }
                }

                var result = await task; // 완료 결과 수집

                if (result == 0)
                {
                    //MessageBox.Show("Ready 완료", "Move", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Ready Fail.", "Move", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            else if (action == "Start")
            {
                Log.Write("Operator_Main", "ChipLoading Start");
                if (InputDieTransfer == null)
                    return;

                try
                {
                    var eq = Equipment.Instance;
                    if (eq == null)
                    {
                        MessageBox.Show("Equipment 인스턴스가 초기화되지 않았습니다.", "오류",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    var unitName = InputDieTransfer.UnitName;
                    if (string.IsNullOrEmpty(unitName))
                    {
                        MessageBox.Show("UnitName 이 비어있습니다.", "오류",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    // 이미 실행 중인지 간단 체크 (RunStatus 사용 가능 시)
                    if (InputDieTransfer.RunUnitStatus == BaseUnit.UnitStatus.Running)
                    {
                        MessageBox.Show($"Unit '{unitName}' 는 이미 실행 중입니다.", "정보",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    Cursor prev = Cursor.Current;
                    Cursor.Current = Cursors.WaitCursor;

                    bool ok = await eq.StartUnitAsync(unitName);
                    if (!ok)
                    {
                        MessageBox.Show($"Unit '{unitName}' 시작 실패.", "오류",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    Cursor.Current = prev;
                }
                catch (Exception ex)
                {
                    Log.Write(ex);
                }
            }
        }

        private async void HandleProcess(string action)
        {
            if (action == "Ready")
            {
                Log.Write("Operator_Main", "Process Ready");
                bool isFine = false;
                // 기존 구성 요소만 활용해 Ready 티칭 인덱스 계산
                int selIndex = -1;
                
                try
                {
                    var cfg = IndexLoadAligner.Config;
                    var tp = cfg?.GetTeachingPosition(IndexLoadAlignerConfig.TeachingPositionName.SafetyZone.ToString());
                    if (tp != null && cfg.TeachingPositions != null)
                        selIndex = cfg.TeachingPositions.IndexOf(tp);
                }
                catch { selIndex = -1; }
                if (selIndex < 0)
                {
                    MessageBox.Show("Ready Teaching Position을 찾을 수 없습니다.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                var task = IndexLoadAligner.MoveTeachingPositionOnceAsync(selIndex, isFine);
                using (var pf = new ProgressForm(IndexLoadAligner.UnitName, "Teaching Position 이동 중...", task))
                {
                    var dr = pf.ShowDialog(this); // 모달: 메인 UI 입력 차단
                    if (dr == DialogResult.Cancel)
                    {
                        IndexLoadAligner.StopTeachingPositionOnce(selIndex);
                        return;
                    }
                }
                var result = await task; // 완료 결과 수집
                if (result == 0)
                {
                    //MessageBox.Show("Ready 완료", "Move", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Ready Fail.", "Move", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }


                //
                try
                {
                    var cfg = IndexChipProbeController.Config;
                    var tp = cfg?.GetTeachingPosition(IndexChipProbeControllerConfig.TeachingPositionName.SafetyZone.ToString());
                    if (tp != null && cfg.TeachingPositions != null)
                        selIndex = cfg.TeachingPositions.IndexOf(tp);
                }
                catch { selIndex = -1; }
                if (selIndex < 0)
                {
                    MessageBox.Show("Ready Teaching Position을 찾을 수 없습니다.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                task = IndexChipProbeController.MoveTeachingPositionOnceAsync(selIndex, isFine);
                using (var pf = new ProgressForm(IndexChipProbeController.UnitName, "Teaching Position 이동 중...", task))
                {
                    var dr = pf.ShowDialog(this); // 모달: 메인 UI 입력 차단
                    if (dr == DialogResult.Cancel)
                    {
                        IndexChipProbeController.StopTeachingPositionOnce(selIndex);
                        return;
                    }
                }
                result = await task; // 완료 결과 수집
                if (result == 0)
                {
                    //MessageBox.Show("Ready 완료", "Move", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Ready Fail.", "Move", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

            }
            else if (action == "Start")
            {
                Log.Write("Operator_Main", "Process Start");
                if (Rotary == null)
                    return;

                try
                {
                    var eq = Equipment.Instance;
                    if (eq == null)
                    {
                        MessageBox.Show("Equipment 인스턴스가 초기화되지 않았습니다.", "오류",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    var unitName = Rotary.UnitName;
                    if (string.IsNullOrEmpty(unitName))
                    {
                        MessageBox.Show("UnitName 이 비어있습니다.", "오류",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    // 이미 실행 중인지 간단 체크 (RunStatus 사용 가능 시)
                    if (Rotary.RunUnitStatus == BaseUnit.UnitStatus.Running)
                    {
                        MessageBox.Show($"Unit '{unitName}' 는 이미 실행 중입니다.", "정보",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    Cursor prev = Cursor.Current;
                    Cursor.Current = Cursors.WaitCursor;

                    bool ok = await eq.StartUnitAsync(unitName);
                    if (!ok)
                    {
                        MessageBox.Show($"Unit '{unitName}' 시작 실패.", "오류",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    Cursor.Current = prev;
                }
                catch (Exception ex)
                {
                    Log.Write(ex);
                }
            }
        }

        private async void HandleChipUnloading(string action)
        {
            if (action == "Ready")
            {
                Log.Write("Operator_Main", "ChipUnloading Ready");
                bool isFine = false;
                // 기존 구성 요소만 활용해 Ready 티칭 인덱스 계산
                int selIndex = -1;
                try
                {
                    var cfg = OutputDieTransfer.Config;
                    var tp = cfg?.GetTeachingPosition(OutputDieTransferConfig.TeachingPositionName.SafetyZone.ToString());
                    if (tp != null && cfg.TeachingPositions != null)
                        selIndex = cfg.TeachingPositions.IndexOf(tp);
                }
                catch { selIndex = -1; }

                if (selIndex < 0)
                {
                    MessageBox.Show("Ready Teaching Position을 찾을 수 없습니다.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var task = OutputDieTransfer.MoveTeachingPositionOnceAsync(selIndex, isFine);
                using (var pf = new ProgressForm(OutputDieTransfer.UnitName, "Teaching Position 이동 중...", task))
                {
                    var dr = pf.ShowDialog(this); // 모달: 메인 UI 입력 차단
                    if (dr == DialogResult.Cancel)
                    {
                        OutputDieTransfer.StopTeachingPositionOnce(selIndex);
                        return;
                    }
                }

                var result = await task; // 완료 결과 수집

                if (result == 0)
                {
                    //MessageBox.Show("Ready 완료", "Move", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Ready Fail.", "Move", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

            }
            else if (action == "Start")
            {
                Log.Write("Operator_Main", "ChipUnloading Start");
                if (OutputDieTransfer == null)
                    return;

                try
                {
                    var eq = Equipment.Instance;
                    if (eq == null)
                    {
                        MessageBox.Show("Equipment 인스턴스가 초기화되지 않았습니다.", "오류",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    var unitName = OutputDieTransfer.UnitName;
                    if (string.IsNullOrEmpty(unitName))
                    {
                        MessageBox.Show("UnitName 이 비어있습니다.", "오류",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    // 이미 실행 중인지 간단 체크 (RunStatus 사용 가능 시)
                    if (OutputDieTransfer.RunUnitStatus == BaseUnit.UnitStatus.Running)
                    {
                        MessageBox.Show($"Unit '{unitName}' 는 이미 실행 중입니다.", "정보",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    Cursor prev = Cursor.Current;
                    Cursor.Current = Cursors.WaitCursor;

                    bool ok = await eq.StartUnitAsync(unitName);
                    if (!ok)
                    {
                        MessageBox.Show($"Unit '{unitName}' 시작 실패.", "오류",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    Cursor.Current = prev;
                }
                catch (Exception ex)
                {
                    Log.Write(ex);
                }
            }
        }

        private async void HandleOutputWafer(string action)
        {
            if (action == "Ready")
            {
                Log.Write("Operator_Main", "OutputWafer Ready 위치로 이동");

                bool isFine = false;
                // 기존 구성 요소만 활용해 Ready 티칭 인덱스 계산
                int selIndex = -1;
                try
                {
                    var cfg = OutputFeeder.Config;
                    var tp = cfg?.GetTeachingPosition(OutputFeederConfig.TeachingPositionName.Ready.ToString());
                    if (tp != null && cfg.TeachingPositions != null)
                        selIndex = cfg.TeachingPositions.IndexOf(tp);
                }
                catch { selIndex = -1; }

                if (selIndex < 0)
                {
                    MessageBox.Show("Ready Teaching Position을 찾을 수 없습니다.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var task = OutputFeeder.MoveTeachingPositionOnceAsync(selIndex, isFine);
                using (var pf = new ProgressForm(OutputFeeder.UnitName, "Teaching Position 이동 중...", task))
                {
                    var dr = pf.ShowDialog(this); // 모달: 메인 UI 입력 차단
                    if (dr == DialogResult.Cancel)
                    {
                        OutputFeeder.StopTeachingPositionOnce(selIndex);
                        return;
                    }
                }

                var result = await task; // 완료 결과 수집

                if (result == 0)
                {
                    //MessageBox.Show("Ready 완료", "Move", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Ready Fail.", "Move", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

            }
            else if (action == "Start")
            {
                Log.Write("Operator_Main", "OutputWafer 시퀀스 실행");
                if (OutputFeeder == null)
                    return;

                try
                {
                    var eq = Equipment.Instance;
                    if (eq == null)
                    {
                        MessageBox.Show("Equipment 인스턴스가 초기화되지 않았습니다.", "오류",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    var unitName = OutputFeeder.UnitName;
                    if (string.IsNullOrEmpty(unitName))
                    {
                        MessageBox.Show("UnitName 이 비어있습니다.", "오류",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    // 이미 실행 중인지 간단 체크 (RunStatus 사용 가능 시)
                    if (OutputFeeder.RunUnitStatus == BaseUnit.UnitStatus.Running)
                    {
                        MessageBox.Show($"Unit '{unitName}' 는 이미 실행 중입니다.", "정보",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    Cursor prev = Cursor.Current;
                    Cursor.Current = Cursors.WaitCursor;

                    bool ok = await eq.StartUnitAsync(unitName);
                    if (!ok)
                    {
                        MessageBox.Show($"Unit '{unitName}' 시작 실패.", "오류",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    Cursor.Current = prev;
                }
                catch (Exception ex)
                {
                    Log.Write(ex);
                }
            }
        }

        #endregion

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
    }
}
