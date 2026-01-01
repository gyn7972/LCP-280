using QMC.Common;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace QMC.LCP_280.Process.Unit.FormMain
{
    public partial class SequenceAutoControl : UserControl
    {
        #region Auto Sequence Events
        /// <summary>
        /// Auto Sequence 버튼 클릭 이벤트 인자
        /// </summary>
        public class AutoSequenceEventArgs : EventArgs
        {
            public string Command { get; set; }  // "Ready", "Start", "Stop", "CycleStop", "Reset"
        }

        /// <summary>
        /// Auto Sequence 상태 변경 이벤트 인자 (Form → Control)
        /// </summary>
        public class AutoSequenceStateChangedEventArgs : EventArgs
        {
            public string Command { get; set; }
            public bool IsActive { get; set; }
        }
        #endregion

        #region Events

        /// <summary>
        /// 버튼 클릭 시 부모 Form으로 전달 (Control → Form)
        /// </summary>
        public event EventHandler<AutoSequenceEventArgs> SequenceButtonRequested;

        #endregion

        #region Fields

        private Dictionary<IndividualMenuButton, string> _buttonCommands;
        private readonly Color _defaultColor = Color.FromArgb(217, 217, 217);
        private readonly Color _activeColor = Color.LightGreen;

        // I/O 상태 모니터링
        private Timer _ioMonitorTimer;
        private EquipmentStatus _equipmentStatus;
        private bool _lastStartSwState = false;
        private bool _lastStopSwState = false;
        private bool _lastResetSwState = false;

        private string _command = string.Empty;
        private bool _isAction = false;

        private bool _uiStateDrivenByForm = false;
        private int _lastFormStateTick = 0;
        private const int FormStateTimeoutMs = 1000;

        #endregion

        public SequenceAutoControl()
        {
            InitializeComponent();
            InitializeButtonCommands();
            RegisterButtonEvents();
            InitializeIOMonitoring();
        }

        #region I/O Monitoring

        /// <summary>
        /// I/O 모니터링 초기화
        /// </summary>
        private void InitializeIOMonitoring()
        {
            try
            {
                // EquipmentStatus 유닛 가져오기
                var eq = Equipment.Instance;
                if (eq?.Units?.TryGetValue("EquipmentStatus", out var unit) == true)
                {
                    _equipmentStatus = unit as EquipmentStatus;
                }

                // I/O 상태 모니터링 타이머 (100ms)
                _ioMonitorTimer = new Timer();
                _ioMonitorTimer.Interval = 100;
                _ioMonitorTimer.Tick += OnIOMonitorTick;
                _ioMonitorTimer.Start();
            }
            catch (Exception ex)
            {
                Log.Write("SequenceAutoControl", $"InitializeIOMonitoring error: {ex.Message}");
            }
        }

        /// <summary>
        /// I/O 상태 주기적 확인
        /// </summary>
        private void OnIOMonitorTick(object sender, EventArgs e)
        {
            if (_equipmentStatus == null) return;

            try
            {
                var snapshot = _equipmentStatus.GetSnapshot();
                if (snapshot == null) return;

                // START_SW (X000) 체크 - Rising Edge
                if (snapshot.Inputs.TryGetValue("START_SW", out bool startSw))
                {
                    if (startSw && !_lastStartSwState) // Rising edge
                    {
                        OnPhysicalButtonPressed("Start");
                    }
                    _lastStartSwState = startSw;
                }

                // STOP_SW (X001) 체크 - Rising Edge
                if (snapshot.Inputs.TryGetValue("STOP_SW", out bool stopSw))
                {
                    if (stopSw && !_lastStopSwState) // Rising edge
                    {
                        OnPhysicalButtonPressed("Stop");
                    }
                    _lastStopSwState = stopSw;
                }

                // RESET_SW (X002) 체크 - Rising Edge
                if (snapshot.Inputs.TryGetValue("RESET_SW", out bool resetSw))
                {
                    if (resetSw && !_lastResetSwState) // Rising edge
                    {
                        OnPhysicalButtonPressed("Reset");
                    }
                    _lastResetSwState = resetSw;
                }

                // 램프 상태 업데이트 (출력 상태 표시)
                UpdateLampStates(snapshot);
            }
            catch (Exception ex)
            {
                // 예외는 조용히 처리 (로그만)
                System.Diagnostics.Debug.WriteLine($"IO Monitor error: {ex.Message}");
            }
        }

        /// <summary>
        /// 물리 버튼 눌림 처리
        /// </summary>
        private void OnPhysicalButtonPressed(string command)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => OnPhysicalButtonPressed(command)));
                return;
            }

            Log.Write("SequenceAutoControl", $"Physical button pressed: {command}");

            // UI 버튼과 동일한 동작 수행
            switch (command)
            {
                case "Start":
                    // Ready 상태에서만 Start 가능하도록 체크
                    if (btn_Auto_Start.Enabled)
                    {
                        PerformSequenceCommand("Start");
                    }
                    break;

                case "Stop":
                    if (btn_Auto_Stop.Enabled)
                    {
                        PerformSequenceCommand("Stop");
                    }
                    break;

                case "Reset":
                    if (btn_Auto_Reset.Enabled)
                    {
                        PerformSequenceCommand("Reset");
                    }
                    break;
            }
        }

        /// <summary>
        /// 시퀀스 명령 실행 (물리 버튼용)
        /// </summary>
        private void PerformSequenceCommand(string command)
        {
            // 부모 Form으로 이벤트 전달 (확인 다이얼로그 없이)
            SequenceButtonRequested?.Invoke(this, new AutoSequenceEventArgs
            {
                Command = command
            });

            // 램프 상태 업데이트
            UpdateButtonLamp(command, true);
        }

        /// <summary>
        /// 램프 상태 업데이트
        /// </summary>
        /// 20251222 - 수정 필요. 장비 실제 상태도 같이 보고 적용해야함.
        private void UpdateLampStates(EquipmentStatusSnapshot snapshot)
        {
            if (snapshot?.Outputs == null) 
                return;

            // Form 갱신이 한동안 없으면 I/O 표시로 복귀
            if (_uiStateDrivenByForm)
            {
                int now = Environment.TickCount;
                if (unchecked(now - _lastFormStateTick) <= FormStateTimeoutMs)
                    return;

                _uiStateDrivenByForm = false;
            }

            snapshot.Outputs.TryGetValue("START_LAMP", out bool startLamp);
            snapshot.Outputs.TryGetValue("STOP_LAMP", out bool stopLamp);
            snapshot.Outputs.TryGetValue("RESET_LAMP", out bool resetLamp);

            if (startLamp)
            {
                btn_Auto_Start.BackColor = _activeColor;
                btn_Auto_Stop.BackColor = _defaultColor;
                btn_Auto_Reset.BackColor = _defaultColor;
            }
            else if (stopLamp)
            {
                btn_Auto_Start.BackColor = _defaultColor;
                btn_Auto_Stop.BackColor = _activeColor;
                btn_Auto_Reset.BackColor = _defaultColor;
            }
            else if (resetLamp)
            {
                btn_Auto_Start.BackColor = _defaultColor;
                btn_Auto_Stop.BackColor = _defaultColor;
                btn_Auto_Reset.BackColor = _activeColor;
            }
            else
            {
                btn_Auto_Start.BackColor = _defaultColor;
                btn_Auto_Stop.BackColor = _defaultColor;
                btn_Auto_Reset.BackColor = _defaultColor;
            }


            // START_LAMP (Y000) 상태
            //if (snapshot.Outputs.TryGetValue("START_LAMP", out bool startLamp))
            //{
            //    btn_Auto_Start.BackColor = startLamp ? _activeColor : _defaultColor;
            //}

            //// STOP_LAMP (Y001) 상태
            //if (snapshot.Outputs.TryGetValue("STOP_LAMP", out bool stopLamp))
            //{
            //    btn_Auto_Stop.BackColor = stopLamp ? _activeColor : _defaultColor;
            //}

            //// RESET_LAMP (Y002) 상태
            //if (snapshot.Outputs.TryGetValue("RESET_LAMP", out bool resetLamp))
            //{
            //    btn_Auto_Reset.BackColor = resetLamp ? _activeColor : _defaultColor;
            //}
        }

        /// <summary>
        /// 버튼 램프 제어 (출력)
        /// </summary>
        private void UpdateButtonLamp(string command, bool on)
        {
            if (_equipmentStatus == null) return;

            try
            {
                switch (command)
                {
                    case "Start":
                        _equipmentStatus.SetOutput("START_LAMP", on);
                        break;
                    case "Stop":
                        _equipmentStatus.SetOutput("STOP_LAMP", on);
                        break;
                    case "Reset":
                        _equipmentStatus.SetOutput("RESET_LAMP", on);
                        break;
                }
            }
            catch (Exception ex)
            {
                Log.Write("SequenceAutoControl", $"UpdateButtonLamp error: {ex.Message}");
            }
        }

        #endregion

        private void InitializeButtonCommands()
        {
            _buttonCommands = new Dictionary<IndividualMenuButton, string>
            {
                { btn_Auto_Ready, "Ready" },
                { btn_Auto_Start, "Start" },
                { btn_Auto_Stop, "Stop" },
                { btn_Auto_CycleStop, "CycleStop" },
                { btn_Auto_Reset, "Reset" }
            };
        }

        private void RegisterButtonEvents()
        {
            foreach (var button in _buttonCommands.Keys)
            {
                button.Click -= OnAutoButtonClick;
                button.Click += OnAutoButtonClick;
            }
        }

        private void OnAutoButtonClick(object sender, EventArgs e)
        {
            var button = sender as IndividualMenuButton;
            if (button == null || !_buttonCommands.ContainsKey(button)) return;

            var command = _buttonCommands[button];

            var ask = new MessageBoxYesNo();
            if (ask.ShowDialog($"Auto Sequence {command}",
                $"Auto Sequence {command} 진행하시겠습니까?") == DialogResult.Yes)
            {
                // 부모 Form으로 이벤트 전달 (UI는 부모가 상태 변경 이벤트로 알려줌)
                SequenceButtonRequested?.Invoke(this, new AutoSequenceEventArgs
                {
                    Command = command
                });
            }
        }

        /// <summary>
        /// 부모 Form에서 상태 변경 이벤트를 받아서 UI 업데이트 (Form → Control)
        /// </summary>
        public void OnAutoSequenceStateChanged(AutoSequenceStateChangedEventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => OnAutoSequenceStateChanged(e)));
                return;
            }

            // Form에서 한번이라도 상태를 내려주기 시작하면, 이후 UI는 Form 상태를 우선으로 함
            _uiStateDrivenByForm = true;
            _lastFormStateTick = Environment.TickCount;

            _command = e.Command;
            _isAction = e.IsActive;

            if (e.IsActive)
            {
                foreach (var btn in _buttonCommands.Keys)
                    btn.BackColor = _defaultColor;
            }

            var button = _buttonCommands.FirstOrDefault(x => x.Value == e.Command).Key;
            if (button != null)
                button.BackColor = e.IsActive ? _activeColor : _defaultColor;
        }

        /// <summary>
        /// 모든 버튼 초기화
        /// </summary>
        public void ResetAllButtons()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(ResetAllButtons));
                return;
            }

            foreach (var button in _buttonCommands.Keys)
            {
                button.BackColor = _defaultColor;
            }
        }

        /// <summary>
        /// 버튼 활성화/비활성화
        /// </summary>
        public void SetButtonEnabled(string command, bool enabled)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => SetButtonEnabled(command, enabled)));
                return;
            }

            var button = _buttonCommands.FirstOrDefault(x => x.Value == command).Key;
            if (button != null)
            {
                button.Enabled = enabled;
            }
        }

        /// <summary>
        /// 여러 버튼 활성화/비활성화
        /// </summary>
        public void SetButtonsEnabled(bool enabled, params string[] commands)
        {
            foreach (var command in commands)
            {
                SetButtonEnabled(command, enabled);
            }
        }

        public void SetAllEnabled(bool enabled)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action<bool>(SetAllEnabled), enabled);
                return;
            }
            foreach (Control c in Controls)
            {
                if (c is Button b)
                    b.Enabled = enabled;
            }
        }
    }
}