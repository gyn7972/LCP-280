using System;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;

namespace SP_GridTypeView
{
    public partial class IOPropertyCollectionView : PropertyCollectionView
    {
        private Label _selectedNameLabel = null;

        public IOPropertyCollectionView() : base()
        {
            InitializeComponent();
        }

        public new void SetProperties(PropertyCollection properties)
        {
            var tableLayoutPanelField = typeof(PropertyCollectionView).GetField("tableLayoutPanel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var tableLayoutPanel = tableLayoutPanelField.GetValue(this) as TableLayoutPanel;
            tableLayoutPanel.SuspendLayout();
            tableLayoutPanel.Controls.Clear();
            tableLayoutPanel.RowStyles.Clear();
            tableLayoutPanel.RowCount = 0;

            int textBoxHeight = TextRenderer.MeasureText("A", _textBoxFont).Height + 8;

            var headerProp = properties.OfType<TitleOnlyProperty>().FirstOrDefault();
            int colCount = headerProp != null ? headerProp.Titles.Length : 2;
            tableLayoutPanel.ColumnCount = colCount;
            tableLayoutPanel.ColumnStyles.Clear();

            // 열 비율 설정
            if (colCount == 2)
            {
                tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 80F)); // Name
                tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F)); // State
            }
            else if (colCount == 3)
            {
                tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 15F)); // No
                tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70F)); // Name
                tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 15F)); // State
            }
            else
            {
                // 기본 균등 분할
                for (int i = 0; i < colCount; i++)
                    tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / colCount));
            }

            int row = 0;
            if (headerProp != null)
            {
                tableLayoutPanel.RowCount++;
                tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, textBoxHeight));
                for (int i = 0; i < colCount; i++)
                {
                    var headerLabel = new Label
                    {
                        Text = headerProp.Titles[i],
                        Dock = DockStyle.Fill,
                        TextAlign = ContentAlignment.MiddleCenter,
                        Font = new Font(_textBoxFont.FontFamily, _textBoxFont.Size, FontStyle.Bold),
                        BackColor = Color.LightGray,
                        BorderStyle = BorderStyle.FixedSingle,
                        AutoSize = false,
                        Margin = new Padding(0),
                        Padding = new Padding(0)
                    };
                    tableLayoutPanel.Controls.Add(headerLabel, i, row);
                }
                row++;
            }

            foreach (var prop in properties)
            {
                if (prop is TitleOnlyProperty) continue;

                tableLayoutPanel.RowCount++;
                tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, textBoxHeight));

                int colIdx = 0;
                Label nameLabel = null;

                if (headerProp != null && colCount == 3)
                {
                    // No 셀
                    var noLabel = new Label
                    {
                        Text = prop.Title,
                        Dock = DockStyle.Fill,
                        TextAlign = ContentAlignment.MiddleLeft,
                        AutoSize = false,
                        Margin = new Padding(0),
                        Padding = new Padding(0),
                        BorderStyle = BorderStyle.FixedSingle,
                        BackColor = Color.LightGray // No. 열 배경색을 회색으로 지정
                    };
                    tableLayoutPanel.Controls.Add(noLabel, colIdx++, row);

                    // Name 셀
                    nameLabel = new Label
                    {
                        Text = prop.Value?.ToString() ?? "",
                        Dock = DockStyle.Fill,
                        TextAlign = ContentAlignment.MiddleLeft,
                        AutoSize = false,
                        Margin = new Padding(0),
                        Padding = new Padding(0),
                        BorderStyle = BorderStyle.FixedSingle,
                        BackColor = Color.White
                    };
                    // 클릭 이벤트 추가
                    nameLabel.Click += (s, e) =>
                    {
                        if (_selectedNameLabel != null)
                            _selectedNameLabel.BackColor = Color.White;
                        nameLabel.BackColor = Color.Yellow;
                        _selectedNameLabel = nameLabel;
                    };
                    tableLayoutPanel.Controls.Add(nameLabel, colIdx++, row);
                }
                else
                {
                    // Name 셀만
                    nameLabel = new Label
                    {
                        Text = prop.Title,
                        Dock = DockStyle.Fill,
                        TextAlign = ContentAlignment.MiddleLeft,
                        AutoSize = false,
                        Margin = new Padding(0),
                        Padding = new Padding(0),
                        BorderStyle = BorderStyle.FixedSingle,
                        BackColor = Color.White
                    };
                    nameLabel.Click += (s, e) =>
                    {
                        if (_selectedNameLabel != null)
                            _selectedNameLabel.BackColor = Color.White;
                        nameLabel.BackColor = Color.Yellow;
                        _selectedNameLabel = nameLabel;
                    };
                    tableLayoutPanel.Controls.Add(nameLabel, colIdx++, row);
                }

                // State 셀
                var statePictureBox = new PictureBox
                {
                    Dock = DockStyle.Fill,
                    Margin = new Padding(0),
                    Padding = new Padding(0),
                    SizeMode = PictureBoxSizeMode.StretchImage,
                    BorderStyle = BorderStyle.FixedSingle,
                    BackColor = Color.Empty
                };

                if (prop is PropertyState ps)
                {
                    if (ps.State)
                    {
                        try
                        {
                            statePictureBox.Image = Properties.Resources.Input;
                        }
                        catch
                        {
                            statePictureBox.Image = null;
                        }
                    }
                    else
                    {
                        statePictureBox.Image = null;
                        statePictureBox.BackColor = Color.Red;
                    }
                }

                tableLayoutPanel.Controls.Add(statePictureBox, colIdx, row);

                row++;
            }
            tableLayoutPanel.ResumeLayout();
        }
    }
}
