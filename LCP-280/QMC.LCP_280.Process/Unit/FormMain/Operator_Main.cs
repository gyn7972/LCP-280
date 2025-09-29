using QMC.Common;
using QMC.Common.Controls;
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
        private InputStage InputStage { get; set; }
        private IndexUnloadAligner IndexUnloadAligner { get; set; }
        private OutputStage OutputStage { get; set; }

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
            TryGetUnit<InputStage>("InputStage"),
            TryGetUnit<IndexUnloadAligner>("IndexUnloadAligner"),
            TryGetUnit<OutputStage>("OutputStage"))
        {
        }

        public Operator_Main(InputStage inputStage, IndexUnloadAligner indexUnloadAligner, OutputStage outputStage)
        {
            InitializeComponent();

            InputStage = inputStage;
            IndexUnloadAligner = indexUnloadAligner;
            OutputStage = outputStage;

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

        private void ExecuteAutoStop()
        {
            // Auto Stop 로직 구현
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

        private void HandleInputWafer(string action)
        {
            if (action == "Ready")
            {
                Log.Write("Operator_Main", "InputWafer Ready 위치로 이동");
                // 실제: InputStage?.MoveToReadyPosition();
            }
            else if (action == "Start")
            {
                Log.Write("Operator_Main", "InputWafer 시퀀스 실행");
                // 실제: InputStage?.ExecuteInputSequence();
            }
        }

        private void HandleChipLoading(string action)
        {
            if (action == "Ready")
            {
                Log.Write("Operator_Main", "ChipLoading Ready");
            }
            else if (action == "Start")
            {
                Log.Write("Operator_Main", "ChipLoading Start");
            }
        }

        private void HandleProcess(string action)
        {
            if (action == "Ready")
            {
                Log.Write("Operator_Main", "Process Ready");
            }
            else if (action == "Start")
            {
                Log.Write("Operator_Main", "Process Start");
            }
        }

        private void HandleChipUnloading(string action)
        {
            if (action == "Ready")
            {
                Log.Write("Operator_Main", "ChipUnloading Ready");
            }
            else if (action == "Start")
            {
                Log.Write("Operator_Main", "ChipUnloading Start");
            }
        }

        private void HandleOutputWafer(string action)
        {
            if (action == "Ready")
            {
                Log.Write("Operator_Main", "OutputWafer Ready 위치로 이동");
                // 실제: OutputStage?.MoveToReadyPosition();
            }
            else if (action == "Start")
            {
                Log.Write("Operator_Main", "OutputWafer 시퀀스 실행");
                // 실제: OutputStage?.ExecuteOutputSequence();
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
