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

        // 자동 간격 분배 옵션
        private bool _autoDistributeSpacing = true;
        private int _minGap = 8; // 최소 간격(px)
        private bool _centerSingleOption = true; // 옵션이 하나일 때 가운데 정렬 시도

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
                UpdateDynamicSpacing();
            }
        }

        [Browsable(true)]
        [Category("Layout")]
        [Description("그룹박스/패널 크기에 맞춰 버튼 간격을 자동으로 분배합니다.")]
        [DefaultValue(true)]
        public bool AutoDistributeSpacing
        {
            get => _autoDistributeSpacing;
            set
            {
                if (_autoDistributeSpacing != value)
                {
                    _autoDistributeSpacing = value;
                    UpdateDynamicSpacing();
                }
            }
        }

        [Browsable(true)]
        [Category("Layout")]
        [Description("자동 간격 분배 시 보장할 최소 간격(px)")]
        [DefaultValue(8)]
        public int MinGap
        {
            get => _minGap;
            set
            {
                _minGap = Math.Max(0, value);
                UpdateDynamicSpacing();
            }
        }

        [Browsable(true)]
        [Category("Layout")]
        [Description("옵션이 하나일 때 가운데 정렬 시도")]
        [DefaultValue(true)]
        public bool CenterSingleOption
        {
            get => _centerSingleOption;
            set
            {
                _centerSingleOption = value;
                UpdateDynamicSpacing();
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

            // 크기 변경 시 간격 재계산
            panel.SizeChanged += (s, e) => UpdateDynamicSpacing();

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

            UpdateDynamicSpacing();
        }

        // ListBoxItemsView와 동일한 크기 설정 이벤트
        protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
        {
            base.SetBoundsCore(x, y, width, height, specified);
            
            if ((specified & BoundsSpecified.Size) != 0)
            {
                Console.WriteLine($"🔧 RadioButtonView SetBoundsCore: Size=({width}, {height}), DesignMode={this.DesignMode}");
                UpdateDynamicSpacing();
            }
        }

        public void SetOptions(bool isVertical, params object[] options)
        {
            this.Orientation = isVertical ? Orientation.Vertical : Orientation.Horizontal;
            
            panel.Controls.Clear();
            if (options == null || options.Length == 0)
            {
                radioButtons = Array.Empty<System.Windows.Forms.RadioButton>();
                UpdateDynamicSpacing();
                return;
            }

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
                        Margin = new Padding(_minGap, 4, _minGap, 4),
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

                UpdateDynamicSpacing();
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
                    Margin = new Padding(_minGap, 4, _minGap, 4),
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

            UpdateDynamicSpacing();
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

                UpdateDynamicSpacing();
            }
            else
            {
                Console.WriteLine($"❌ GroupBox가 null입니다.");
            }
        }

        /// <summary>
        /// 그룹박스/패널 크기와 라디오 버튼 폭(혹은 높이)에 맞춰 Margin을 재계산하여 간격을 분배
        /// </summary>
        private void UpdateDynamicSpacing()
        {
            try
            {
                if (!_autoDistributeSpacing || panel == null || radioButtons == null || radioButtons.Length == 0)
                    return;

                // 클라이언트 영역 크기
                var client = panel.ClientSize;
                int n = radioButtons.Length;

                if (_orientation == Orientation.Horizontal)
                {
                    int innerWidth = Math.Max(0, client.Width - panel.Padding.Left - panel.Padding.Right);
                    if (innerWidth <= 0) return;

                    // 버튼 가로 길이 합산
                    int sumBtn = 0;
                    for (int i = 0; i < n; i++)
                    {
                        // AutoSize 시 PreferredSize 사용
                        var w = radioButtons[i].PreferredSize.Width;
                        sumBtn += w;
                    }

                    int gaps = n + 1; // 좌/우 여백 포함
                    int s = (innerWidth - sumBtn) / Math.Max(1, gaps);
                    s = Math.Max(_minGap, s);

                    if (n == 1 && _centerSingleOption)
                    {
                        int left = Math.Max(_minGap, (innerWidth - radioButtons[0].PreferredSize.Width) / 2);
                        int right = left;
                        radioButtons[0].Margin = new Padding(left, radioButtons[0].Margin.Top, right, radioButtons[0].Margin.Bottom);
                    }
                    else
                    {
                        for (int i = 0; i < n; i++)
                        {
                            int left = (i == 0) ? s : s / 2;
                            int right = (i == n - 1) ? s : s / 2;
                            var rb = radioButtons[i];
                            rb.Margin = new Padding(left, rb.Margin.Top, right, rb.Margin.Bottom);
                        }
                    }
                }
                else // Vertical
                {
                    int innerHeight = Math.Max(0, client.Height - panel.Padding.Top - panel.Padding.Bottom);
                    if (innerHeight <= 0) return;

                    int sumBtn = 0;
                    for (int i = 0; i < n; i++)
                    {
                        var h = Math.Max(radioButtons[i].PreferredSize.Height, radioButtons[i].MinimumSize.Height);
                        sumBtn += h;
                    }

                    int gaps = n + 1;
                    int s = (innerHeight - sumBtn) / Math.Max(1, gaps);
                    s = Math.Max(_minGap / 2, s); // 세로는 조금 더 촘촘하게

                    if (n == 1 && _centerSingleOption)
                    {
                        int top = Math.Max(_minGap, (innerHeight - radioButtons[0].PreferredSize.Height) / 2);
                        int bottom = top;
                        radioButtons[0].Margin = new Padding(radioButtons[0].Margin.Left, top, radioButtons[0].Margin.Right, bottom);
                    }
                    else
                    {
                        for (int i = 0; i < n; i++)
                        {
                            int top = (i == 0) ? s : s / 2;
                            int bottom = (i == n - 1) ? s : s / 2;
                            var rb = radioButtons[i];
                            rb.Margin = new Padding(rb.Margin.Left, top, rb.Margin.Right, bottom);
                        }
                    }
                }

                // 레이아웃/그리기 갱신
                panel.PerformLayout();
                panel.Invalidate();
                panel.Update();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"RadioButtonView.UpdateDynamicSpacing 오류: {ex.Message}");
            }
        }

    }
}