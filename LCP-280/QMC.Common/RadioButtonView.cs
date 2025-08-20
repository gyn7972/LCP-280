using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace QMC.Common
{
    public partial class RadioButtonView : UserControl
    {
        private GroupBox groupBox; // GroupBox 필드 추가
        private FlowLayoutPanel panel;
        private System.Windows.Forms.RadioButton[] radioButtons;
        private Orientation _orientation = Orientation.Horizontal;
        private string _groupName = "Radio Options"; // GroupBox 이름 저장용

        public event EventHandler<int> OptionSelected;

        // GroupBox 이름 프로퍼티 (ListBoxItemsView와 동일한 패턴)
        [Browsable(true)]
        [Category("Appearance")]
        [Description("GroupBox 이름")]
        public string GroupName
        {
            get => groupBox?.Text ?? _groupName;
            set
            {
                _groupName = value;
                if (groupBox != null)
                    groupBox.Text = value;
            }
        }

        [Browsable(true)]
        [Category("Layout")]
        [Description("라디오버튼 배치 방향 (Horizontal/Vertical)")]
        public Orientation Orientation
        {
            get => _orientation;
            set
            {
                _orientation = value;
                UpdateFlowDirection();
            }
        }

        public RadioButtonView()
        {
            InitializeComponent();
            InitializeUserControl(_groupName); // Ensure panel and groupBox are initialized
        }

        // 방향 변경 시 FlowDirection 업데이트하는 별도 메서드
        private void UpdateFlowDirection()
        {
            Console.WriteLine($"🔧 UpdateFlowDirection 호출: _orientation={_orientation}");
            
            if (panel != null)
            {
                var newFlowDirection = (_orientation == Orientation.Horizontal)
                    ? FlowDirection.LeftToRight
                    : FlowDirection.TopDown;
                    
                Console.WriteLine($"🔧 FlowDirection 변경: {panel.FlowDirection} → {newFlowDirection}");
                
                panel.FlowDirection = newFlowDirection;
                
                // 방향 변경 시 레이아웃 갱신
                panel.Invalidate();
                panel.PerformLayout();
                
                Console.WriteLine($"🔧 UpdateFlowDirection 완료: Orientation={_orientation}, FlowDirection={panel.FlowDirection}");
            }
            else
            {
                Console.WriteLine($"⚠️ UpdateFlowDirection: panel이 null입니다.");
            }
        }

        // ListBoxItemsView와 동일한 패턴으로 GroupBox 초기화
        private void InitializeUserControl(string groupName)
        {
            _groupName = groupName;

            // GroupBox 생성 및 설정 - ListBoxItemsView와 동일하게 UserControl 전체 크기에 맞춤
            groupBox = new GroupBox
            {
                Text = groupName,
                Font = new Font("맑은 고딕", 10f, FontStyle.Regular),
                ForeColor = Color.Black,
                BackColor = Color.White,
                Padding = new Padding(8, 8, 8, 8),
                Dock = DockStyle.Fill // Designer 크기에 완전히 맞춤
            };

            // UserControl에 GroupBox 추가
            this.Controls.Add(groupBox);

            // FlowLayoutPanel 초기화
            InitializeFlowLayoutPanel();

            Console.WriteLine($"🔧 RadioButtonView 초기화: UserControl={this.Size}, GroupBox=Fill");
        }

        private void InitializeFlowLayoutPanel()
        {
            Console.WriteLine($"🔧 InitializeFlowLayoutPanel 시작: _orientation={_orientation}");
            
            // 기존 FlowLayoutPanel이 있으면 제거
            if (panel != null && panel.Parent != null)
                panel.Parent.Controls.Remove(panel);

            var initialFlowDirection = (_orientation == Orientation.Horizontal) 
                ? FlowDirection.LeftToRight 
                : FlowDirection.TopDown;
                
            Console.WriteLine($"🔧 초기 FlowDirection 설정: {initialFlowDirection}");

            panel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoSize = false, // AutoSize 비활성화로 크기 안정성 확보
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                // 현재 Orientation 값에 맞게 FlowDirection 설정
                FlowDirection = initialFlowDirection,
                WrapContents = false, // 줄바꿈 방지 - 세로/가로 일관성을 위해 false로 변경
                Padding = new Padding(2),
                BackColor = Color.White // 배경색 설정
            };

            // UserControl 자동 크기 조정 비활성화 (Designer 크기 사용)
            this.AutoSize = false;
            this.AutoSizeMode = AutoSizeMode.GrowOnly;

            // groupBox에 FlowLayoutPanel 추가 (기존: this.Controls.Add(panel))
            if (groupBox != null)
            {
                groupBox.Controls.Clear();
                groupBox.Controls.Add(panel);
            }
            
            Console.WriteLine($"🔧 InitializeFlowLayoutPanel 완료: Orientation={_orientation}, FlowDirection={panel.FlowDirection}");
        }

        // ListBoxItemsView와 동일한 크기 조정 이벤트
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            
            Console.WriteLine($"🔧 RadioButtonView OnResize: UserControl={this.Size}, DesignMode={this.DesignMode}");
            if (groupBox != null)
            {
                Console.WriteLine($"   GroupBox 크기={groupBox.Size}, Dock={groupBox.Dock}");
            }
            
            // 내부 컨트롤들 갱신
            if (panel != null)
            {
                panel.Invalidate();
            }
            if (groupBox != null)
            {
                groupBox.Invalidate();
            }
        }

        // ListBoxItemsView와 동일한 크기 설정 이벤트
        protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
        {
            base.SetBoundsCore(x, y, width, height, specified);
            
            if ((specified & BoundsSpecified.Size) != 0)
            {
                Console.WriteLine($"🔧 RadioButtonView SetBoundsCore: Size=({width}, {height}), DesignMode={this.DesignMode}");
            }
        }

        public void SetOptions(bool isVertical, params object[] options)
        {
            this.Orientation = isVertical ? Orientation.Vertical : Orientation.Horizontal;
            
            panel.Controls.Clear();
            if (options == null || options.Length == 0)
                return;

            // enum 타입이 단일로 들어온 경우 처리
            if (options.Length == 1 && options[0] is Type enumType && enumType.IsEnum)
            {
                var enumValues = Enum.GetValues(enumType);
                radioButtons = new System.Windows.Forms.RadioButton[enumValues.Length];
                for (int i = 0; i < enumValues.Length; i++)
                {
                    string text = enumValues.GetValue(i).ToString();
                    var rb = new System.Windows.Forms.RadioButton
                    {
                        Text = text,
                        AutoSize = true,
                        Margin = new Padding(8, 4, 8, 4),
                        Font = new Font("맑은 고딕", 9f),
                        TabStop = true,
                        BackColor = Color.White, // 배경색 추가
                        // 🔧 디버깅을 위해 MinimumSize 설정
                        MinimumSize = new Size(50, 20)
                    };
                    rb.CheckedChanged += (s, e) =>
                    {
                        if (rb.Checked)
                            OptionSelected?.Invoke(this, Array.IndexOf(radioButtons, rb));
                    };
                    radioButtons[i] = rb;
                    panel.Controls.Add(rb);
                }
                if (radioButtons.Length > 0)
                    radioButtons[0].Checked = true;
                    
                // 🔧 라디오 버튼 추가 후 즉시 레이아웃 강제 갱신
                if (panel != null)
                {
                    panel.PerformLayout();
                    panel.Invalidate();
                    panel.Update();
                }
                return;
            }

            // 일반 객체 배열 처리 추가
            radioButtons = new System.Windows.Forms.RadioButton[options.Length];
            for (int i = 0; i < options.Length; i++)
            {
                string text = options[i]?.ToString() ?? $"Option {i + 1}";
                var rb = new System.Windows.Forms.RadioButton
                {
                    Text = text,
                    AutoSize = true,
                    Margin = new Padding(8, 4, 8, 4),
                    Font = new Font("맑은 고딕", 9f),
                    TabStop = true,
                    BackColor = Color.White,
                    // 🔧 디버깅을 위해 MinimumSize 설정
                    MinimumSize = new Size(50, 20)
                };
                rb.CheckedChanged += (s, e) =>
                {
                    if (rb.Checked)
                        OptionSelected?.Invoke(this, Array.IndexOf(radioButtons, rb));
                };
                radioButtons[i] = rb;
                panel.Controls.Add(rb);
            }
            
            if (radioButtons.Length > 0)
                radioButtons[0].Checked = true;
                
            // 🔧 라디오 버튼 추가 후 즉시 레이아웃 강제 갱신
            if (panel != null)
            {
                panel.PerformLayout();
                panel.Invalidate();
                panel.Update();
            }
        }

        // 선택된 인덱스 접근
        public int SelectedIndex
        {
            get
            {
                if (radioButtons == null) return -1;
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

        // ListBoxItemsView와 동일한 GroupBox 크기 정보 확인용 프로퍼티
        [Browsable(false)]
        public Size GroupBoxSize => groupBox?.Size ?? Size.Empty;

        [Browsable(false)]
        public Size UserControlSize => this.Size;

        // ListBoxItemsView와 동일한 GroupBox Size 설정 메서드
        public void SetGroupBoxSize(Size size)
        {
            if (groupBox != null)
            {
                // UserControl 자체의 Size를 변경하여 GroupBox가 따라오도록 함
                this.Size = size;
                
                // GroupBox 레이아웃 강제 업데이트
                groupBox.Invalidate();
                groupBox.Update();
                groupBox.PerformLayout();
                
                // UserControl 레이아웃 강제 업데이트
                this.Invalidate();
                this.Update();
                this.PerformLayout();
            }
            else
            {
                Console.WriteLine($"❌ GroupBox가 null입니다.");
            }
        }

    }
}