using QMC.Common;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace QMC.LCP_280.Process.Unit.FormMain
{
    public partial class SequenceManualControl : UserControl
    {
        #region Manual Sequence Events

        /// <summary>
        /// Manual Sequence 버튼 클릭 이벤트 인자
        /// </summary>
        public class SequenceEventArgs : EventArgs
        {
            public string SequenceName { get; set; }
            public string Action { get; set; }  // "Ready" 또는 "Start"
        }

        /// <summary>
        /// Manual Sequence 상태 변경 이벤트 인자 (Form → Control)
        /// </summary>
        public class SequenceStateChangedEventArgs : EventArgs
        {
            public string SequenceName { get; set; }
            public string Action { get; set; }  // "Ready" 또는 "Start"
            public bool IsActive { get; set; }
            public bool UpdateText { get; set; } // Start 버튼 텍스트 변경 여부
        }
        #endregion

        #region Events

        /// <summary>
        /// 버튼 클릭 시 부모 Form으로 전달 (Control → Form)
        /// </summary>
        public event EventHandler<SequenceEventArgs> SequenceButtonRequested;

        #endregion

        private Dictionary<IndividualMenuButton, (string sequenceName, string action)> _buttonActions;
        private readonly Color _defaultColor = Color.FromArgb(217, 217, 217);
        private readonly Color _activeColor = Color.LightGreen;

        public SequenceManualControl()
        {
            InitializeComponent();
            InitializeButtonActions();
            RegisterButtonEvents();
        }

        private void InitializeButtonActions()
        {
            _buttonActions = new Dictionary<IndividualMenuButton, (string, string)>
            {
                { btn_Ready_InputWafer, ("InputWafer", "Ready") },
                { btn_Start_InputWafer, ("InputWafer", "Start") },
                { btn_Ready_ChipLoading, ("ChipLoading", "Ready") },
                { btn_Start_ChipLoading, ("ChipLoading", "Start") },
                { btn_Ready_Process, ("Process", "Ready") },
                { btn_Start_Process, ("Process", "Start") },
                { btn_Ready_ChipUnloading, ("ChipUnloading", "Ready") },
                { btn_Start_ChipUnloading, ("ChipUnloading", "Start") },
                { btn_Ready_OutputWafer, ("OutputWafer", "Ready") },
                { btn_Start_OutputWafer, ("OutputWafer", "Start") }
            };
        }

        private void RegisterButtonEvents()
        {
            foreach (var button in _buttonActions.Keys)
            {
                button.Click -= OnSequenceButtonClick;
                button.Click += OnSequenceButtonClick;
            }
        }

        private void OnSequenceButtonClick(object sender, EventArgs e)
        {
            var button = sender as IndividualMenuButton;
            if (button == null || !_buttonActions.ContainsKey(button)) return;

            var (sequenceName, action) = _buttonActions[button];

            var ask = new MessageBoxYesNo();
            if (ask.ShowDialog($"Manual {sequenceName} {action}",
                $"Manual {sequenceName} {action} 진행하시겠습니까?") == DialogResult.Yes)
            {
                // 부모 Form으로 이벤트 전달 (UI는 부모가 상태 변경 이벤트로 알려줌)
                SequenceButtonRequested?.Invoke(this, new SequenceEventArgs
                {
                    SequenceName = sequenceName,
                    Action = action
                });
            }
        }

        /// <summary>
        /// 부모 Form에서 상태 변경 이벤트를 받아서 UI 업데이트 (Form → Control)
        /// </summary>
        public void OnSequenceStateChanged(SequenceStateChangedEventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => OnSequenceStateChanged(e)));
                return;
            }

            var button = _buttonActions.FirstOrDefault(x =>
                x.Value.sequenceName == e.SequenceName && x.Value.action == e.Action).Key;

            if (button != null)
            {
                button.BackColor = e.IsActive ? _activeColor : _defaultColor;

                // Start 버튼만 텍스트 변경
                if (e.Action == "Start" && e.UpdateText)
                {
                    button.Text = e.IsActive ? "Stop" : "Start";
                }
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

            foreach (var kvp in _buttonActions)
            {
                kvp.Key.BackColor = _defaultColor;

                // Start 버튼 텍스트 초기화
                if (kvp.Value.action == "Start")
                {
                    kvp.Key.Text = "Start";
                }
            }
        }

        /// <summary>
        /// 특정 Sequence의 모든 버튼 초기화
        /// </summary>
        public void ResetSequenceButtons(string sequenceName)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => ResetSequenceButtons(sequenceName)));
                return;
            }

            var buttons = _buttonActions.Where(x => x.Value.sequenceName == sequenceName);

            foreach (var kvp in buttons)
            {
                kvp.Key.BackColor = _defaultColor;

                // Start 버튼 텍스트 초기화
                if (kvp.Value.action == "Start")
                {
                    kvp.Key.Text = "Start";
                }
            }
        }

        /// <summary>
        /// 버튼 활성화/비활성화
        /// </summary>
        public void SetButtonEnabled(string sequenceName, string action, bool enabled)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => SetButtonEnabled(sequenceName, action, enabled)));
                return;
            }

            var button = _buttonActions.FirstOrDefault(x =>
                x.Value.sequenceName == sequenceName && x.Value.action == action).Key;

            if (button != null)
            {
                button.Enabled = enabled;
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