using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace SP_GridTypeView
{
    [DefaultProperty("TextBoxFontSize")]
    public class PropertyCollectionView : UserControl
    {
        private TableLayoutPanel tableLayoutPanel;
        private Panel scrollPanel;

        // 디자이너에서 편집 가능한 속성의 기본값
        private Font _textBoxFont = new Font("맑은 고딕", 9f); // Windows Forms 기본 폰트와 크기
        private HorizontalAlignment _textBoxTextAlign = HorizontalAlignment.Left;

        private const int MinVisibleRows = 5;
        
        int InitialHeight = 0; 

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

        public PropertyCollectionView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            scrollPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true
            };

            tableLayoutPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                ColumnCount = 2,
                RowCount = 0,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                CellBorderStyle = TableLayoutPanelCellBorderStyle.Single
            };
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F));

            scrollPanel.Controls.Add(tableLayoutPanel);
            Controls.Add(scrollPanel);
        }

        /// <summary>
        /// PropertyCollection을 화면에 표시합니다.
        /// </summary>
        public void SetProperties(PropertyCollection properties)
        {
            tableLayoutPanel.SuspendLayout();
            tableLayoutPanel.Controls.Clear();
            tableLayoutPanel.RowStyles.Clear();
            tableLayoutPanel.RowCount = 0;
            _textBoxPropertyMap.Clear();
            _currentProperties = properties;
            InitialHeight= Math.Max(InitialHeight, this.Height);
            if (properties == null)
            {
                tableLayoutPanel.ResumeLayout();
                return;
            }

            int textBoxHeight = TextRenderer.MeasureText("A", _textBoxFont).Height + 8;

            int row = 0;
            foreach (var prop in properties)
            {
                tableLayoutPanel.RowCount++;
                tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, textBoxHeight));

                var titleLabel = new Label
                {
                    Text = prop.Title,
                    Dock = DockStyle.Fill,
                    TextAlign = System.Drawing.ContentAlignment.MiddleLeft,
                    AutoSize = false,
                    Margin = new Padding(2),
                    Padding = new Padding(0)
                };

                var valueTextBox = new TextBox
                {
                    Text = prop.Value?.ToString() ?? string.Empty,
                    Dock = DockStyle.Fill,
                    Margin = new Padding(0),
                    BorderStyle = BorderStyle.FixedSingle,
                    Font = _textBoxFont,
                    TextAlign = _textBoxTextAlign
                };

                valueTextBox.MinimumSize = new Size(0, textBoxHeight);
                valueTextBox.Height = textBoxHeight;

                tableLayoutPanel.Controls.Add(titleLabel, 0, row);
                tableLayoutPanel.Controls.Add(valueTextBox, 1, row);

                _textBoxPropertyMap.Add(Tuple.Create(valueTextBox, prop));

                row++;
            }

            int totalRows = properties.Count;
            int MinRows= (int)Math.Max(MinVisibleRows, Math.Max(this.Height, InitialHeight) / textBoxHeight);
            int visibleRows = Math.Min(totalRows, MinRows);
            int neededHeight = visibleRows * textBoxHeight;

            tableLayoutPanel.Height = totalRows * textBoxHeight;
            this.Height = neededHeight;

            tableLayoutPanel.ResumeLayout();
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
        /// 속성 변경 시 UI를 새로고침합니다.
        /// </summary>
        private void RefreshProperties()
        {
            if (_currentProperties != null)
                SetProperties(_currentProperties);
        }
    }
}