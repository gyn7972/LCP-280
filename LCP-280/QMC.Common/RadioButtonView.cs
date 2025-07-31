using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace QMC.Common
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
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Padding = new Padding(2)
            };
            this.AutoSize = true; // UserControl도 자동 크기 조정
            this.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            this.Controls.Add(panel);
        }

        public void SetOptions(bool isVertical, params object[] options)
        {
            panel.Controls.Clear();
            if (options == null || options.Length == 0)
                return;

            // enum 타입이 단일로 들어온 경우 처리
            if (options.Length == 1 && options[0] is Type enumType && enumType.IsEnum)
            {
                var enumValues = Enum.GetValues(enumType);
                radioButtons = new RadioButton[enumValues.Length];
                for (int i = 0; i < enumValues.Length; i++)
                {
                    string text = enumValues.GetValue(i).ToString();
                    var rb = new RadioButton
                    {
                        Text = text,
                        AutoSize = true,
                        Margin = new Padding(8, 4, 8, 4),
                        Font = new Font("맑은 고딕", 9f),
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
                radioButtons[0].Checked = true;
                this.Orientation = isVertical ? Orientation.Vertical : Orientation.Horizontal;
                return;
            }
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