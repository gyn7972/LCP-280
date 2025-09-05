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
            set
            {
                if (groupBox != null)
                    groupBox.Text = value ?? "";
            }
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
            set
            {
                if (value != null && !_textBoxFont.Equals(value))
                {
                    _textBoxFont = value;
                    RefreshProperties();
                }
            }
        }

        [Category("Appearance")]
        [Description("텍스트박스의 텍스트 정렬")]
        [DefaultValue(HorizontalAlignment.Left)]
        public HorizontalAlignment TextBoxTextAlign
        {
            get => _textBoxTextAlign;
            set
            {
                if (_textBoxTextAlign != value)
                {
                    _textBoxTextAlign = value;
                    RefreshProperties();
                }
            }
        }

        // 텍스트박스와 PropertyBase 매핑을 위한 리스트
        private List<Tuple<TextBox, PropertyBase>> _textBoxPropertyMap = new List<Tuple<TextBox, PropertyBase>>();
        private PropertyCollection _currentProperties;

        public PropertyCollectionView(string groupName = "Property Group")
        {
            InitializeComponent();
            InitializeComponentUser(groupName);
        }

        public PropertyCollectionView() : this("Property Group")
        {
        }

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
                AutoScroll = true,                   // ★ 항상 켜두기
                AutoScrollMargin = new Size(0, 4),
                BackColor = Color.White,
                AutoSizeMode = AutoSizeMode.GrowAndShrink
            };

            tableLayoutPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Top,                // ★ Top으로 두고 세로로 키워지게
                AutoSize = true,                     // ★ 내용만큼 커짐
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                ColumnCount = 2,
                RowCount = 0,
                CellBorderStyle = TableLayoutPanelCellBorderStyle.Single
            };
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F));

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

            Console.WriteLine($"🔧 PropertyCollectionView 초기화: UserControl={this.Size}, GroupBox=Fill");
        }

        // Designer에서 크기 조정시 로그 출력
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            
            Console.WriteLine($"🔧 PropertyCollectionView OnResize: UserControl={this.Size}, DesignMode={this.DesignMode}");
            
            // 내부 컨트롤들 갱신
            if (scrollPanel != null)
            {
                scrollPanel.Invalidate();
            }
            if (tableLayoutPanel != null)
            {
                tableLayoutPanel.Invalidate();
            }
            if (groupBox != null)
            {
                groupBox.Invalidate();
            }
        }

        // Designer에서 크기 설정시 로그 출력
        protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
        {
            base.SetBoundsCore(x, y, width, height, specified);
            
            if ((specified & BoundsSpecified.Size) != 0)
            {
                Console.WriteLine($"🔧 PropertyCollectionView SetBoundsCore: Size=({width}, {height}), DesignMode={this.DesignMode}");
            }
        }

        /// <summary>
        /// PropertyCollection을 화면에 표시합니다.
        /// </summary>
        public virtual void SetProperties(PropertyCollection properties)
        {
            // 전체 컨트롤 화면 업데이트 중단
            this.SuspendLayout();
            if (groupBox != null)
            {
                groupBox.SuspendLayout();
                scrollPanel.SuspendLayout();
                tableLayoutPanel.SuspendLayout();
            }
            
            // 컨트롤이 생성되는 동안 화면 표시 비활성화
            this.Visible = false;
            
            tableLayoutPanel.Controls.Clear();
            tableLayoutPanel.RowStyles.Clear();
            tableLayoutPanel.RowCount = 0;
            _textBoxPropertyMap.Clear();
            _currentProperties = properties;

            if (properties == null || properties.Count == 0)
            {
                // 레이아웃 재개 및 화면 표시 복원
                if (groupBox != null)
                {
                    tableLayoutPanel.ResumeLayout(false);
                    scrollPanel.ResumeLayout(false);
                    groupBox.ResumeLayout(false);
                }
                this.ResumeLayout(true);
                this.Visible = true;
                Console.WriteLine($"🔧 SetProperties (빈 데이터): UserControl={this.Size}");
                return;
            }

            int textBoxHeight = TextRenderer.MeasureText("A", _textBoxFont).Height + 8;

            // 모든 컨트롤을 임시 리스트에 저장하여 한번에 추가
            var controlsToAdd = new List<Tuple<Control, int, int>>();
            var columnSpansToSet = new List<Tuple<Control, int>>();

            int row = 0;
            foreach (var prop in properties)
            {
                tableLayoutPanel.RowCount++;
                tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, textBoxHeight));

                if (prop is TitleOnlyProperty titleOnlyProp)
                {
                    // 섹션 헤더는 보이도록!
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
                        Visible = false
                    };
                    controlsToAdd.Add(Tuple.Create((Control)titleLabel, 0, row));
                    columnSpansToSet.Add(Tuple.Create((Control)titleLabel, tableLayoutPanel.ColumnCount));
                }
                else if (prop is ComboBoxProperty comboBoxProperty)
                {
                    //var titleLabel = new Label { /* (생략) 기존 그대로 */ };
                    var titleLabel = new Label
                    {
                        Text = prop.Title,
                        Dock = DockStyle.Fill,
                        TextAlign = ContentAlignment.MiddleLeft,
                        AutoSize = false,
                        Margin = new Padding(2),
                        Padding = new Padding(2),
                        Visible = false
                    };
                    var comboBox = new ComboBox
                    {
                        Dock = DockStyle.Fill,
                        Margin = new Padding(0),
                        Font = _textBoxFont,
                        DropDownStyle = ComboBoxStyle.DropDownList,
                        //DataSource = comboBoxProperty.Options,
                        //SelectedItem = comboBoxProperty.Value?.ToString(),
                        Visible = false
                    };

                    // 수정: 이전에 선택한 아이템이 Load시 Display되지 않는 부분 수정
                    comboBox.Items.Clear();
                    comboBox.Items.AddRange(comboBoxProperty.Options.ToArray());

                    // Value와 일치하는 인덱스를 찾아서 선택
                    if (comboBoxProperty.Value != null)
                    {
                        int idx = comboBoxProperty.Options.IndexOf(comboBoxProperty.Value.ToString());
                        if (idx >= 0)
                            comboBox.SelectedIndex = idx;
                        else
                            comboBox.SelectedIndex = 0; // 기본값
                    }
                    else
                    {
                        comboBox.SelectedIndex = 0;
                    }

                    comboBox.SelectedIndexChanged += (sender, args) =>
                    {
                        comboBoxProperty.SetValue(comboBox.SelectedItem?.ToString());
                    };
                    
                    controlsToAdd.Add(Tuple.Create((Control)titleLabel, 0, row));
                    controlsToAdd.Add(Tuple.Create((Control)comboBox, 1, row));
                }
                else
                {
                    // 공통 타이틀
                    var titleLabel = new Label
                    {
                        Text = prop.Title,
                        Dock = DockStyle.Fill,
                        TextAlign = ContentAlignment.MiddleLeft,
                        AutoSize = false,
                        Margin = new Padding(2),
                        Padding = new Padding(2),
                        Visible = false
                    };
                    controlsToAdd.Add(Tuple.Create((Control)titleLabel, 0, row));

                    // 타입별 에디터 + 즉시 바인딩
                    Control editor;

                    if (prop is BoolProperty bp)
                    {
                        var cb = new CheckBox
                        {
                            Dock = DockStyle.Left,
                            AutoSize = true,
                            Margin = new Padding(4, (textBoxHeight - 18) / 2, 0, 0),
                            Visible = false
                        };
                        BindCheckBoxToBool(cb, bp);
                        editor = cb;
                    }
                    else if (prop is IntProperty ip)
                    {
                        var tb = MakeValueTextBox(textBoxHeight);
                        BindTextBoxToInt(tb, ip);
                        editor = tb;
                    }
                    else if (prop is LongProperty lp)
                    {
                        var tb = MakeValueTextBox(textBoxHeight);
                        BindTextBoxToLong(tb, lp);
                        editor = tb;
                    }
                    else if (prop is FloatProperty fp)
                    {
                        var tb = MakeValueTextBox(textBoxHeight);
                        BindTextBoxToFloat(tb, fp);
                        editor = tb;
                    }
                    else if (prop is DoubleProperty dp)
                    {
                        var tb = MakeValueTextBox(textBoxHeight);
                        BindTextBoxToDouble(tb, dp);
                        editor = tb;
                    }
                    else if (prop is StringProperty sp)
                    {
                        var tb = MakeValueTextBox(textBoxHeight);
                        BindTextBoxToString(tb, sp);
                        editor = tb;
                    }
                    else
                    {
                        // 모르는 타입은 문자열로 폴백
                        var tb = MakeValueTextBox(textBoxHeight);
                        tb.TextChanged += (s, e) => prop.SetValue(tb.Text);
                        tb.Tag = prop;
                        editor = tb;
                    }

                    // 입력/출력 모드 색상 적용
                    if (properties.IsInputParameter)
                    {
                        editor.ForeColor = Color.Black;
                        editor.BackColor = Color.White;
                    }
                    else
                    {
                        if (editor is TextBox tbRo)
                        {
                            tbRo.ReadOnly = true;
                            tbRo.TabStop = false;
                            tbRo.ForeColor = Color.LimeGreen;
                            tbRo.BackColor = Color.Black;
                        }
                        else
                        {
                            editor.Enabled = false;
                        }
                    }

                    controlsToAdd.Add(Tuple.Create(editor, 1, row));
                }

                row++;
            }

            // 모든 컨트롤을 한번에 추가
            foreach (var controlInfo in controlsToAdd)
            {
                tableLayoutPanel.Controls.Add(controlInfo.Item1, controlInfo.Item2, controlInfo.Item3);
            }

            // ColumnSpan 설정
            foreach (var spanInfo in columnSpansToSet)
            {
                tableLayoutPanel.SetColumnSpan(spanInfo.Item1, spanInfo.Item2);
            }

            // 레이아웃 재개 (아직 화면 업데이트는 하지 않음)
            if (groupBox != null)
            {
                tableLayoutPanel.ResumeLayout(false);
                scrollPanel.ResumeLayout(false);
                groupBox.ResumeLayout(false);
            }
            this.ResumeLayout(false);
            
            // 모든 컨트롤을 한번에 표시
            foreach (var controlInfo in controlsToAdd)
            {
                controlInfo.Item1.Visible = true;
            }
            
            // 최종적으로 화면 표시 복원 및 레이아웃 완료
            this.Visible = true;
            this.PerformLayout();

            // 레이아웃 재계산 후 스크롤 높이 갱신
            tableLayoutPanel.PerformLayout();
            scrollPanel.PerformLayout();

            // ★ 컨텐츠 높이를 AutoScrollMinSize에 반영 → 스크롤 확실히 보장
            scrollPanel.AutoScrollMinSize = new Size(
                0,
                tableLayoutPanel.PreferredSize.Height + 2
            );

            // (선택) 항상 맨 위로
            if (scrollPanel.VerticalScroll.Maximum > 0)
                scrollPanel.VerticalScroll.Value = 0;

            Console.WriteLine($"🔧 SetProperties 완료: UserControl={this.Size}, Items={properties.Count}");
        }

        private TextBox MakeValueTextBox(int textBoxHeight)
        {
            var tb = new TextBox
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(0),
                BorderStyle = BorderStyle.FixedSingle,
                Font = _textBoxFont,
                TextAlign = _textBoxTextAlign,
                Visible = false,
                MinimumSize = new Size(0, textBoxHeight),
                Height = textBoxHeight
            };
            return tb;
        }


        /// <summary>
        /// 텍스트박스의 값을 PropertyCollection에 적용합니다.
        /// </summary>
        public void Apply()
        {
            foreach (var pair in _textBoxPropertyMap)
            {
                var textBox = pair.Item1;
                var property = pair.Item2;
                property.SetValue(textBox.Text);
            }
        }

        /// <summary>
        /// 🚀 현재 설정된 PropertyCollection을 반환합니다.
        /// </summary>
        /// <returns>현재 PropertyCollection</returns>
        public PropertyCollection GetCurrentProperties()
        {
            return _currentProperties;
        }

        /// <summary>
        /// 속성 변경 시 UI를 새로고침합니다.
        /// </summary>
        private void RefreshProperties()
        {
            if (_currentProperties != null)
                SetProperties(_currentProperties);
        }

        // ===== 즉시 반영 바인딩 유틸 =====
        private void BindTextBoxToDouble(TextBox tb, DoubleProperty p)
        {
            tb.Text = p.Value.ToString(CultureInfo.InvariantCulture);
            tb.TextChanged += (s, e) =>
            {
                if (double.TryParse(tb.Text, NumberStyles.Float | NumberStyles.AllowThousands,
                                    CultureInfo.InvariantCulture, out var v))
                    p.Value = v;
            };
            tb.Tag = p; // (옵션) 커밋/디버깅용
        }

        private void BindTextBoxToFloat(TextBox tb, FloatProperty p)
        {
            tb.Text = p.Value.ToString(CultureInfo.InvariantCulture);
            tb.TextChanged += (s, e) =>
            {
                if (float.TryParse(tb.Text, NumberStyles.Float | NumberStyles.AllowThousands,
                                   CultureInfo.InvariantCulture, out var v))
                    p.Value = v;
            };
            tb.Tag = p;
        }

        private void BindTextBoxToInt(TextBox tb, IntProperty p)
        {
            tb.Text = p.Value.ToString(CultureInfo.InvariantCulture);
            tb.TextChanged += (s, e) =>
            {
                if (int.TryParse(tb.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var v))
                    p.Value = v;
            };
            tb.Tag = p;
        }

        private void BindTextBoxToLong(TextBox tb, LongProperty p)
        {
            tb.Text = p.Value.ToString(CultureInfo.InvariantCulture);
            tb.TextChanged += (s, e) =>
            {
                if (long.TryParse(tb.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var v))
                    p.Value = v;
            };
            tb.Tag = p;
        }

        private void BindTextBoxToString(TextBox tb, StringProperty p)
        {
            tb.Text = p.Value ?? string.Empty;
            tb.TextChanged += (s, e) => p.Value = tb.Text ?? string.Empty;
            tb.Tag = p;
        }

        private void BindCheckBoxToBool(CheckBox cb, BoolProperty p)
        {
            cb.Checked = p.Value;
            cb.CheckedChanged += (s, e) => p.Value = cb.Checked;
            cb.Tag = p;
        }

    }
}