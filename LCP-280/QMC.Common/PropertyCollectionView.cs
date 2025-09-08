using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Globalization;

namespace QMC.Common
{
    [DefaultProperty("TextBoxFontSize")]
    public partial class PropertyCollectionView : UserControl
    {
        // ----- Performance / Logging Flags -----
        /// <summary>
        /// 전역 상세 로그 출력 제어 (디폴트: false). true이면 내부 Console.WriteLine 실행.
        /// 실행 속도 개선 위해 기본 비활성화.
        /// </summary>
        public static bool GlobalVerboseLogging = false;
        /// <summary>
        /// SetProperties 빌드 중 레이아웃 최소화(기본 true). false면 기존 방식.
        /// </summary>
        [Browsable(false)] public bool FastBuild { get; set; } = true;
        /// <summary>
        /// OnResize 시 잦은 Invalidate 억제 (기본 true).
        /// </summary>
        [Browsable(false)] public bool SuppressResizeInvalidation { get; set; } = true;

        private TableLayoutPanel tableLayoutPanel;
        private Panel scrollPanel;
        private GroupBox groupBox; // GroupBox to wrap the property controls

        // 디자이너에서 편집 가능한 속성의 기본값
        protected Font _textBoxFont = new Font("맑은 고딕", 9f); // Windows Forms 기본 폰트와 크기
        private HorizontalAlignment _textBoxTextAlign = HorizontalAlignment.Left;

        private const int MinVisibleRows = 3; // 최소 보이는 행 수 줄임
        private const int MaxVisibleRows = 15; // 최대 보이는 행 수 설정
        private const int GroupBoxHeaderHeight = 20; // GroupBox 헤더 높이
        private const int GroupBoxPadding = 20; // GroupBox 패딩

        // GroupBox 이름 프로퍼티
        [Browsable(true)]
        [Category("Appearance")] 
        [Description("GroupBox 이름")]
        public string GroupName
        {
            get => groupBox?.Text ?? "";
            set { if (groupBox != null) groupBox.Text = value ?? ""; }
        }

        [Category("Appearance")]
        [Description("텍스트박스의 폰트 크기")]
        [DefaultValue(9f)]
        public float TextBoxFontSize
        {
            get => _textBoxFont.Size;
            set
            {
                if (value > 0 && Math.Abs(_textBoxFont.Size - value) > 0.01f)
                {
                    _textBoxFont = new Font(_textBoxFont.FontFamily, value);
                    RefreshProperties();
                }
            }
        }

        [Category("Appearance")]
        [Description("텍스트박스의 폰트")]
        [DefaultValue(typeof(Font), "맑은 고딕, 9pt")]
        public Font TextBoxFont
        {
            get => _textBoxFont;
            set { if (value != null && !_textBoxFont.Equals(value)) { _textBoxFont = value; RefreshProperties(); } }
        }

        [Category("Appearance")]
        [Description("텍스트박스의 텍스트 정렬")]
        [DefaultValue(HorizontalAlignment.Left)]
        public HorizontalAlignment TextBoxTextAlign
        {
            get => _textBoxTextAlign;
            set { if (_textBoxTextAlign != value) { _textBoxTextAlign = value; RefreshProperties(); } }
        }

        // 텍스트박스와 PropertyBase 매핑을 위한 리스트
        private List<Tuple<TextBox, PropertyBase>> _textBoxPropertyMap = new List<Tuple<TextBox, PropertyBase>>();
        private PropertyCollection _currentProperties;

        public PropertyCollectionView(string groupName = "Property Group")
        {
            InitializeComponent();
            InitializeComponentUser(groupName);
        }

        public PropertyCollectionView() : this("Property Group") { }

        private void InitializeComponentUser(string groupName)
        {
            groupBox = new GroupBox
            {
                Text = groupName,
                Font = new Font("맑은 고딕", 10f, FontStyle.Regular),
                ForeColor = Color.Black,
                BackColor = Color.White,
                Padding = new Padding(8, 8, 8, 12),
                Dock = DockStyle.Fill
            };
            this.Controls.Add(groupBox);

            scrollPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                AutoScrollMargin = new Size(0, 4),
                BackColor = Color.White,
                AutoSizeMode = AutoSizeMode.GrowAndShrink
            };

            tableLayoutPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                ColumnCount = 2,
                RowCount = 0,
                CellBorderStyle = TableLayoutPanelCellBorderStyle.Single
            };
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F));
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));

            scrollPanel.Controls.Add(tableLayoutPanel);
            groupBox.Controls.Add(scrollPanel);

            // 가로 스크롤 안 나오게(패널 폭에 맞춰 width를 따라가게)
            scrollPanel.Resize += (s, e) =>
            {
                // 세로 스크롤바 폭만큼 여유 주면 H-스크롤 방지
                var sbw = SystemInformation.VerticalScrollBarWidth;
                tableLayoutPanel.Width = scrollPanel.ClientSize.Width - sbw;
            };

            // (옵션) 깜빡임 줄이기
            this.DoubleBuffered = true;
            if (GlobalVerboseLogging)
                Console.WriteLine($"🔧 PropertyCollectionView 초기화: UserControl={this.Size}, GroupBox=Fill");
        }

        // Designer에서 크기 조정시 로그 출력
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (GlobalVerboseLogging)
                Console.WriteLine($"🔧 PropertyCollectionView OnResize: UserControl={this.Size}, DesignMode={this.DesignMode}");
            if (!SuppressResizeInvalidation) // 기본은 과도한 Invalidate 억제
            {
                scrollPanel?.Invalidate();
                tableLayoutPanel?.Invalidate();
                groupBox?.Invalidate();
            }
        }

        // Designer에서 크기 설정시 로그 출력
        protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
        {
            base.SetBoundsCore(x, y, width, height, specified);
            if (GlobalVerboseLogging && (specified & BoundsSpecified.Size) != 0)
                Console.WriteLine($"🔧 PropertyCollectionView SetBoundsCore: Size=({width}, {height}), DesignMode={this.DesignMode}");
        }

        /// <summary>
        /// PropertyCollection을 화면에 표시 (최적화 적용).
        /// </summary>
        public virtual void SetProperties(PropertyCollection properties)
        {
            if (FastBuild)
            {
                // 레이아웃 이벤트 최소화
                this.SuspendLayout();
                groupBox?.SuspendLayout();
                scrollPanel?.SuspendLayout();
                tableLayoutPanel?.SuspendLayout();
            }

            this.Visible = false; // 빌드 중 깜빡임 제거

            tableLayoutPanel.Controls.Clear();
            tableLayoutPanel.RowStyles.Clear();
            tableLayoutPanel.RowCount = 0;
            _textBoxPropertyMap.Clear();
            _currentProperties = properties;

            if (properties == null || properties.Count == 0)
            {
                if (FastBuild)
                {
                    tableLayoutPanel.ResumeLayout(false);
                    scrollPanel.ResumeLayout(false);
                    groupBox.ResumeLayout(false);
                    this.ResumeLayout(true);
                }
                this.Visible = true;
                if (GlobalVerboseLogging)
                    Console.WriteLine($"🔧 SetProperties (빈 데이터): UserControl={this.Size}");
                return;
            }

            int textBoxHeight = TextRenderer.MeasureText("A", _textBoxFont).Height + 8;
            var controlsToAdd = new List<Tuple<Control, int, int>>();
            var columnSpansToSet = new List<Tuple<Control, int>>();

            int row = 0;
            foreach (var prop in properties)
            {
                tableLayoutPanel.RowCount++;
                // 빠른 빌드를 위해 RowStyles 추가 최소화 (AutoSize true 시 Absolute 지정)
                tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, textBoxHeight));

                if (prop is TitleOnlyProperty titleOnlyProp)
                {
                    var titleLabel = new Label
                    {
                        Text = titleOnlyProp.Titles.Length == 1 ? titleOnlyProp.Titles[0] : string.Join(" / ", titleOnlyProp.Titles),
                        Dock = DockStyle.Fill,
                        TextAlign = ContentAlignment.MiddleLeft,
                        AutoSize = false,
                        Margin = new Padding(0),
                        Padding = new Padding(2),
                        Font = new Font(_textBoxFont.FontFamily, _textBoxFont.Size, FontStyle.Bold),
                        BackColor = Color.LightGray,
                        Visible = true // 헤더는 즉시 표시
                    };
                    controlsToAdd.Add(Tuple.Create((Control)titleLabel, 0, row));
                    columnSpansToSet.Add(Tuple.Create((Control)titleLabel, tableLayoutPanel.ColumnCount));
                }
                else if (prop is ComboBoxProperty comboBoxProperty)
                {
                    var titleLabel = new Label
                    {
                        Text = prop.Title,
                        Dock = DockStyle.Fill,
                        TextAlign = ContentAlignment.MiddleLeft,
                        AutoSize = false,
                        Margin = new Padding(2),
                        Padding = new Padding(2),
                        Visible = true
                    };
                    var comboBox = new ComboBox
                    {
                        Dock = DockStyle.Fill,
                        Margin = new Padding(0),
                        Font = _textBoxFont,
                        DropDownStyle = ComboBoxStyle.DropDownList,
                        Visible = true
                    };
                    comboBox.Items.Clear();
                    comboBox.Items.AddRange(comboBoxProperty.Options.ToArray());
                    if (comboBoxProperty.Value != null)
                    {
                        int idx = comboBoxProperty.Options.IndexOf(comboBoxProperty.Value.ToString());
                        comboBox.SelectedIndex = idx >= 0 ? idx : 0;
                    }
                    else comboBox.SelectedIndex = 0;
                    comboBox.SelectedIndexChanged += (sender, args) => comboBoxProperty.SetValue(comboBox.SelectedItem?.ToString());
                    controlsToAdd.Add(Tuple.Create((Control)titleLabel, 0, row));
                    controlsToAdd.Add(Tuple.Create((Control)comboBox, 1, row));
                }
                else
                {
                    var titleLabel = new Label
                    {
                        Text = prop.Title,
                        Dock = DockStyle.Fill,
                        TextAlign = ContentAlignment.MiddleLeft,
                        AutoSize = false,
                        Margin = new Padding(2),
                        Padding = new Padding(2),
                        Visible = true
                    };
                    controlsToAdd.Add(Tuple.Create((Control)titleLabel, 0, row));
                    Control editor;
                    if (prop is BoolProperty bp)
                    {
                        var cb = new CheckBox
                        {
                            Dock = DockStyle.Left,
                            AutoSize = true,
                            Margin = new Padding(4, (textBoxHeight - 18) / 2, 0, 0),
                            Visible = true
                        };
                        BindCheckBoxToBool(cb, bp); editor = cb;
                    }
                    else if (prop is IntProperty ip)
                    { var tb = MakeValueTextBox(textBoxHeight); BindTextBoxToInt(tb, ip); editor = tb; }
                    else if (prop is LongProperty lp)
                    { var tb = MakeValueTextBox(textBoxHeight); BindTextBoxToLong(tb, lp); editor = tb; }
                    else if (prop is FloatProperty fp)
                    { var tb = MakeValueTextBox(textBoxHeight); BindTextBoxToFloat(tb, fp); editor = tb; }
                    else if (prop is DoubleProperty dp)
                    { var tb = MakeValueTextBox(textBoxHeight); BindTextBoxToDouble(tb, dp); editor = tb; }
                    else if (prop is StringProperty sp)
                    { var tb = MakeValueTextBox(textBoxHeight); BindTextBoxToString(tb, sp); editor = tb; }
                    else
                    { var tb = MakeValueTextBox(textBoxHeight); tb.TextChanged += (s, e) => prop.SetValue(tb.Text); tb.Tag = prop; editor = tb; }

                    if (properties.IsInputParameter)
                    {
                        editor.ForeColor = Color.Black;
                        editor.BackColor = Color.White;
                    }
                    else
                    {
                        if (editor is TextBox tbRo)
                        { tbRo.ReadOnly = true; tbRo.TabStop = false; tbRo.ForeColor = Color.LimeGreen; tbRo.BackColor = Color.Black; }
                        else editor.Enabled = false;
                    }
                    editor.Visible = true;
                    controlsToAdd.Add(Tuple.Create(editor, 1, row));
                }
                row++;
            }

            foreach (var controlInfo in controlsToAdd)
                tableLayoutPanel.Controls.Add(controlInfo.Item1, controlInfo.Item2, controlInfo.Item3);
            foreach (var spanInfo in columnSpansToSet)
                tableLayoutPanel.SetColumnSpan(spanInfo.Item1, spanInfo.Item2);

            if (FastBuild)
            {
                tableLayoutPanel.ResumeLayout(false);
                scrollPanel.ResumeLayout(false);
                groupBox.ResumeLayout(false);
                this.ResumeLayout(false);
            }

            this.Visible = true;
            tableLayoutPanel.PerformLayout();
            scrollPanel.PerformLayout();
            scrollPanel.AutoScrollMinSize = new Size(0, tableLayoutPanel.PreferredSize.Height + 2);
            if (scrollPanel.VerticalScroll.Maximum > 0)
                scrollPanel.VerticalScroll.Value = 0;
            if (GlobalVerboseLogging)
                Console.WriteLine($"🔧 SetProperties 완료(FastBuild={FastBuild}): UserControl={this.Size}, Items={properties.Count}");
        }

        private TextBox MakeValueTextBox(int textBoxHeight) => new TextBox
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(0),
            BorderStyle = BorderStyle.FixedSingle,
            Font = _textBoxFont,
            TextAlign = _textBoxTextAlign,
            Visible = true,
            MinimumSize = new Size(0, textBoxHeight),
            Height = textBoxHeight
        };

        /// <summary>
        /// 텍스트박스의 값을 PropertyCollection에 적용합니다.
        /// </summary>
        public void Apply()
        {
            foreach (var pair in _textBoxPropertyMap)
            {
                var textBox = pair.Item1; var property = pair.Item2; property.SetValue(textBox.Text);
            }
        }

        /// <summary>
        /// 🚀 현재 설정된 PropertyCollection을 반환합니다.
        /// </summary>
        /// <returns>현재 PropertyCollection</returns>
        public PropertyCollection GetCurrentProperties() => _currentProperties;

        /// <summary>
        /// 속성 변경 시 UI를 새로고침합니다.
        /// </summary>
        private void RefreshProperties() { if (_currentProperties != null) SetProperties(_currentProperties); }

        // ===== 즉시 반영 바인딩 유틸 =====
        private void BindTextBoxToDouble(TextBox tb, DoubleProperty p)
        { tb.Text = p.Value.ToString(CultureInfo.InvariantCulture); tb.TextChanged += (s, e) => { if (double.TryParse(tb.Text, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out var v)) p.Value = v; }; tb.Tag = p; _textBoxPropertyMap.Add(Tuple.Create(tb, (PropertyBase)p)); }
        private void BindTextBoxToFloat(TextBox tb, FloatProperty p)
        { tb.Text = p.Value.ToString(CultureInfo.InvariantCulture); tb.TextChanged += (s, e) => { if (float.TryParse(tb.Text, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out var v)) p.Value = v; }; tb.Tag = p; _textBoxPropertyMap.Add(Tuple.Create(tb, (PropertyBase)p)); }
        private void BindTextBoxToInt(TextBox tb, IntProperty p)
        { tb.Text = p.Value.ToString(CultureInfo.InvariantCulture); tb.TextChanged += (s, e) => { if (int.TryParse(tb.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var v)) p.Value = v; }; tb.Tag = p; _textBoxPropertyMap.Add(Tuple.Create(tb, (PropertyBase)p)); }
        private void BindTextBoxToLong(TextBox tb, LongProperty p)
        { tb.Text = p.Value.ToString(CultureInfo.InvariantCulture); tb.TextChanged += (s, e) => { if (long.TryParse(tb.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var v)) p.Value = v; }; tb.Tag = p; _textBoxPropertyMap.Add(Tuple.Create(tb, (PropertyBase)p)); }
        private void BindTextBoxToString(TextBox tb, StringProperty p)
        { tb.Text = p.Value ?? string.Empty; tb.TextChanged += (s, e) => p.Value = tb.Text ?? string.Empty; tb.Tag = p; _textBoxPropertyMap.Add(Tuple.Create(tb, (PropertyBase)p)); }
        private void BindCheckBoxToBool(CheckBox cb, BoolProperty p)
        { cb.Checked = p.Value; cb.CheckedChanged += (s, e) => p.Value = cb.Checked; cb.Tag = p; }
    }
}