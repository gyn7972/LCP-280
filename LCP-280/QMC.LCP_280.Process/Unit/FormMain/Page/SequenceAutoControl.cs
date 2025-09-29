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

        private Dictionary<IndividualMenuButton, string> _buttonCommands;
        private readonly Color _defaultColor = Color.FromArgb(217, 217, 217);
        private readonly Color _activeColor = Color.LightGreen;

        public SequenceAutoControl()
        {
            InitializeComponent();
            InitializeButtonCommands();
            RegisterButtonEvents();
        }

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

            var button = _buttonCommands.FirstOrDefault(x => x.Value == e.Command).Key;
            if (button != null)
            {
                button.BackColor = e.IsActive ? _activeColor : _defaultColor;
            }
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
    }
}