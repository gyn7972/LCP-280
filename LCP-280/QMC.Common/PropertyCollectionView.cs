using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

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
        private const int GroupBoxPadding = 16; // GroupBox 패딩

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
            // GroupBox 생성 및 스타일 적용 (ListBoxItemsView와 동일한 스타일)
            groupBox = new GroupBox();
            groupBox.Dock = DockStyle.Fill;
            groupBox.Font = new Font("맑은 고딕", 10f, FontStyle.Regular);
            groupBox.ForeColor = Color.Black;
            groupBox.BackColor = Color.White; // 배경색을 하얀색으로 설정
            groupBox.Text = groupName;
            groupBox.Padding = new Padding(8, 8, 8, 8); // 제목과 내용 간격 조정
            groupBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            this.Controls.Add(groupBox);

            scrollPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Color.White
            };

            tableLayoutPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, // Top에서 Fill로 변경하여 빈 공간 제거
                AutoSize = false, // AutoSize를 false로 변경
                ColumnCount = 2,
                RowCount = 0,
                AutoSizeMode = AutoSizeMode.GrowOnly, // GrowAndShrink에서 GrowOnly로 변경
                CellBorderStyle = TableLayoutPanelCellBorderStyle.Single // 외곽선 유지
            };
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F));

            scrollPanel.Controls.Add(tableLayoutPanel);
            groupBox.Controls.Add(scrollPanel);
        }

        // Override OnResize to ensure GroupBox resizes properly
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            
            // GroupBox와 내부 컨트롤들이 정확히 맞춰지도록 조정
            if (groupBox != null)
            {
                groupBox.Size = this.ClientSize;
                groupBox.Invalidate();
                
                // ScrollPanel과 TableLayoutPanel도 동기화
                if (scrollPanel != null)
                {
                    scrollPanel.Invalidate();
                }
                if (tableLayoutPanel != null)
                {
                    tableLayoutPanel.Invalidate();
                }
            }
        }

        // Override SetBoundsCore to ensure proper sizing behavior
        protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
        {
            base.SetBoundsCore(x, y, width, height, specified);
            
            // GroupBox 크기 동기화 (PropertyCollection 배열 크기에 따른 동적 조정 반영)
            if (groupBox != null && (specified & BoundsSpecified.Size) != 0)
            {
                groupBox.Size = new Size(width, height);
                
                // 내부 컨트롤들도 함께 업데이트
                if (scrollPanel != null)
                {
                    scrollPanel.Invalidate();
                }
                if (tableLayoutPanel != null)
                {
                    tableLayoutPanel.Invalidate();
                }
            }
        }

        /// <summary>
        /// PropertyCollection을 화면에 표시합니다.
        /// </summary>
        public virtual void SetProperties(PropertyCollection properties)
        {
            // 전체 컨트롤 화면 업데이트 중단
            this.SuspendLayout();
            groupBox.SuspendLayout();
            scrollPanel.SuspendLayout();
            tableLayoutPanel.SuspendLayout();
            
            // 컨트롤이 생성되는 동안 화면 표시 비활성화
            this.Visible = false;
            
            tableLayoutPanel.Controls.Clear();
            tableLayoutPanel.RowStyles.Clear();
            tableLayoutPanel.RowCount = 0;
            _textBoxPropertyMap.Clear();
            _currentProperties = properties;

            if (properties == null)
            {
                // 속성이 없을 때 최소 크기로 설정
                int minHeight = GroupBoxHeaderHeight + groupBox.Padding.Top + GroupBoxPadding;
                this.Height = minHeight;
                this.MinimumSize = new Size(this.Width, minHeight);
                this.MaximumSize = new Size(this.Width, minHeight);
                
                // 레이아웃 재개 및 화면 표시 복원
                tableLayoutPanel.ResumeLayout(false);
                scrollPanel.ResumeLayout(false);
                groupBox.ResumeLayout(false);
                this.ResumeLayout(true);
                this.Visible = true;
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
                    if (titleOnlyProp.Titles.Length == 1)
                    {
                        var titleLabel = new Label
                        {
                            Text = titleOnlyProp.Titles[0],
                            Dock = DockStyle.Fill,
                            TextAlign = System.Drawing.ContentAlignment.MiddleLeft,
                            AutoSize = false,
                            Margin = new Padding(0),
                            Padding = new Padding(0),
                            Font = new Font(_textBoxFont.FontFamily, _textBoxFont.Size, FontStyle.Bold),
                            BackColor = Color.LightGray,
                            Visible = false // 임시로 숨김
                        };

                        controlsToAdd.Add(Tuple.Create((Control)titleLabel, 0, row));
                        columnSpansToSet.Add(Tuple.Create((Control)titleLabel, tableLayoutPanel.ColumnCount));
                    }
                    else
                    {
                        // 열 개수 조정
                        tableLayoutPanel.ColumnCount = titleOnlyProp.Titles.Length;
                        tableLayoutPanel.ColumnStyles.Clear();
                        for (int i = 0; i < titleOnlyProp.Titles.Length; i++)
                        {
                            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / titleOnlyProp.Titles.Length));
                        }

                        for (int i = 0; i < titleOnlyProp.Titles.Length; i++)
                        {
                            var titleLabel = new Label
                            {
                                Text = titleOnlyProp.Titles[i],
                                Dock = DockStyle.Fill,
                                TextAlign = System.Drawing.ContentAlignment.MiddleLeft,
                                AutoSize = false,
                                Margin = new Padding(0),
                                Padding = new Padding(0),
                                Font = new Font(_textBoxFont.FontFamily, _textBoxFont.Size, FontStyle.Bold),
                                BackColor = Color.LightGray,
                                Visible = false // 임시로 숨김
                            };
                            controlsToAdd.Add(Tuple.Create((Control)titleLabel, i, row));
                        }
                    }
                }
                else if (prop is ComboBoxProperty comboBoxProperty)
                {
                    var titleLabel = new Label
                    {
                        Text = prop.Title,
                        Dock = DockStyle.Fill,
                        TextAlign = System.Drawing.ContentAlignment.MiddleLeft,
                        AutoSize = false,
                        Margin = new Padding(2),
                        Padding = new Padding(0),
                        Visible = false // 임시로 숨김
                    };

                    var comboBox = new ComboBox
                    {
                        Dock = DockStyle.Fill,
                        Margin = new Padding(0),
                        Font = _textBoxFont,
                        DropDownStyle = ComboBoxStyle.DropDownList,
                        DataSource = comboBoxProperty.Options,
                        SelectedItem = comboBoxProperty.Value?.ToString(),
                        Visible = false // 임시로 숨김
                    };

                    comboBox.SelectedIndexChanged += (sender, args) =>
                    {
                        comboBoxProperty.SetValue(comboBox.SelectedItem.ToString());
                    };

                    controlsToAdd.Add(Tuple.Create((Control)titleLabel, 0, row));
                    controlsToAdd.Add(Tuple.Create((Control)comboBox, 1, row));
                }
                else
                {
                    var titleLabel = new Label
                    {
                        Text = prop.Title,
                        Dock = DockStyle.Fill,
                        TextAlign = System.Drawing.ContentAlignment.MiddleLeft,
                        AutoSize = false,
                        Margin = new Padding(2),
                        Padding = new Padding(0),
                        Visible = false // 임시로 숨김
                    };

                    var valueTextBox = new TextBox
                    {
                        Text = prop.Value?.ToString() ?? string.Empty,
                        Dock = DockStyle.Fill,
                        Margin = new Padding(0),
                        BorderStyle = BorderStyle.FixedSingle,
                        Font = _textBoxFont,
                        TextAlign = HorizontalAlignment.Left,
                        Visible = false // 임시로 숨김
                    };

                    valueTextBox.MinimumSize = new Size(0, textBoxHeight);
                    valueTextBox.Height = textBoxHeight;

                    // 조건 분기: PropertyCollection.UseValueColor 옵션에 따라 처리
                    if (properties.IsInputParameter)
                    {
                        valueTextBox.ForeColor = Color.Black;
                        valueTextBox.BackColor = Color.White;
                    }
                    else
                    {
                        valueTextBox.ReadOnly = true;
                        valueTextBox.TabStop = false;
                        valueTextBox.ForeColor = Color.LimeGreen;
                        valueTextBox.BackColor = Color.Black;
                    }

                    controlsToAdd.Add(Tuple.Create((Control)titleLabel, 0, row));
                    controlsToAdd.Add(Tuple.Create((Control)valueTextBox, 1, row));
                    _textBoxPropertyMap.Add(Tuple.Create(valueTextBox, prop));
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

            int totalRows = properties.Count;
            int calculatedHeight = (totalRows * textBoxHeight) + GroupBoxHeaderHeight + groupBox.Padding.Top + GroupBoxPadding;
            int maxHeight = (MaxVisibleRows * textBoxHeight) + GroupBoxHeaderHeight + groupBox.Padding.Top + GroupBoxPadding;

            // TableLayoutPanel 크기를 항상 모든 행을 포함하도록 설정
            tableLayoutPanel.Height = totalRows * textBoxHeight;

            // PropertyCollection.Count가 MaxVisibleRows 이하이면 실제 row 개수만큼 크기, 초과면 MaxVisibleRows 크기+스크롤
            if (totalRows > MaxVisibleRows)
            {
                this.Height = maxHeight;
                this.MinimumSize = new Size(this.Width, maxHeight);
                this.MaximumSize = new Size(this.Width, maxHeight);
                scrollPanel.AutoScroll = true;
                scrollPanel.VerticalScroll.Visible = true;

                // 마지막 행이 스크롤 영역에 포함되도록 보장
                scrollPanel.VerticalScroll.Value = scrollPanel.VerticalScroll.Maximum;
            }
            else
            {
                this.Height = calculatedHeight;
                this.MinimumSize = new Size(this.Width, calculatedHeight);
                this.MaximumSize = new Size(this.Width, calculatedHeight);
                scrollPanel.AutoScroll = false;
                scrollPanel.VerticalScroll.Visible = false;
            }

            // 레이아웃 재개 (아직 화면 업데이트는 하지 않음)
            tableLayoutPanel.ResumeLayout(false);
            scrollPanel.ResumeLayout(false);
            groupBox.ResumeLayout(false);
            this.ResumeLayout(false);
            
            // 모든 컨트롤을 한번에 표시
            foreach (var controlInfo in controlsToAdd)
            {
                controlInfo.Item1.Visible = true;
            }
            
            // 최종적으로 화면 표시 복원 및 레이아웃 완료
            this.Visible = true;
            this.PerformLayout();

            // 부모 컨트롤에게 크기 변경 알림
            this.Invalidate();
            this.Parent?.PerformLayout();
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
    }
}