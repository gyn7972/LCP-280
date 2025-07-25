using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace SP_GridTypeView
{
    public partial class RadioButtonView : UserControl
    {
        private FlowLayoutPanel panel;
        private RadioButton[] radioButtons;
        private Orientation _orientation = Orientation.Horizontal;

        public event EventHandler<int> OptionSelected;

        [Browsable(true)]
        [Category("Layout")]
        [Description("라디오버튼 배치 방향 (Horizontal/Vertical)")]
        public Orientation Orientation
        {
            get => _orientation;
            set
            {
                _orientation = value;
                if (panel != null)
                    panel.FlowDirection = (_orientation == Orientation.Horizontal)
                        ? FlowDirection.LeftToRight
                        : FlowDirection.TopDown;
            }
        }

        public RadioButtonView()
        {
            InitializeComponent();
            panel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoSize = true,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Padding = new Padding(2)
            };
            this.Controls.Add(panel);
        }

        // 옵션 설정
        public void SetOptions(params string[] options)
        {
            panel.Controls.Clear();
            if (options == null || options.Length == 0)
                return;

            radioButtons = new RadioButton[options.Length];
            for (int i = 0; i < options.Length; i++)
            {
                var rb = new RadioButton
                {
                    Text = options[i],
                    AutoSize = true,
                    Margin = new Padding(8, 4, 8, 4),
                    Font = new Font("맑은 고딕", 12f),
                    TabStop = true
                };
                rb.CheckedChanged += (s, e) =>
                {
                    if (rb.Checked)
                        OptionSelected?.Invoke(this, Array.IndexOf(radioButtons, rb));
                };
                radioButtons[i] = rb;
                panel.Controls.Add(rb);
            }
            radioButtons[0].Checked = true; // 첫 번째 옵션 기본 선택
            Orientation = _orientation; // 방향 적용
        }

        // 선택된 인덱스 접근
        public int SelectedIndex
        {
            get
            {
                for (int i = 0; i < radioButtons.Length; i++)
                    if (radioButtons[i].Checked) return i;
                return -1;
            }
            set
            {
                if (radioButtons != null && value >= 0 && value < radioButtons.Length)
                    radioButtons[value].Checked = true;
            }
        }
    }
}