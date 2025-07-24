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

            // PropertyState의 ShowNoColumn 옵션에 따라 열 개수 결정
            var stateProps = properties.OfType<PropertyState>().ToList();
            bool hasNoColumn = stateProps.Any(p => properties.ShowNoColumn);
            int colCount = hasNoColumn ? 3 : 2;
            tableLayoutPanel.ColumnCount = colCount;
            tableLayoutPanel.ColumnStyles.Clear();

            if (colCount == 2)
            {
                tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 80F));
                tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            }
            else
            {
                tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 15F));
                tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70F));
                tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 15F));
            }

            int row = 0;

            // TitleOnlyProperty가 있으면 헤더만 생성 (열 개수에는 영향 없음)
            var headerProp = properties.OfType<TitleOnlyProperty>().FirstOrDefault();
            if (headerProp != null)
            {
                tableLayoutPanel.RowCount++;
                tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, textBoxHeight));
                for (int i = 0; i < colCount; i++)
                {
                    var titleLabel = new Label
                    {
                        Text = i < headerProp.Titles.Length ? headerProp.Titles[i] : "",
                        Dock = DockStyle.Fill,
                        TextAlign = ContentAlignment.MiddleLeft,
                        AutoSize = false,
                        Margin = new Padding(0),
                        Padding = new Padding(0),
                        Font = new Font(_textBoxFont.FontFamily, _textBoxFont.Size, FontStyle.Bold),
                        BackColor = Color.LightGray
                    };
                    tableLayoutPanel.Controls.Add(titleLabel, i, row);
                }
                row++;
            }

            // PropertyState 행 추가
            foreach (var prop in stateProps)
            {
                tableLayoutPanel.RowCount++;
                tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, textBoxHeight));

                int colIdx = 0;
                Label nameLabel = null;

                if (colCount == 3 && prop.ShowNoColumn)
                {
                    // 0번 열(No) 표시
                    var noLabel = new Label
                    {
                        Text = prop.Title,
                        Dock = DockStyle.Fill,
                        TextAlign = ContentAlignment.MiddleLeft,
                        AutoSize = false,
                        Margin = new Padding(0),
                        Padding = new Padding(0),
                        BorderStyle = BorderStyle.FixedSingle,
                        BackColor = Color.LightGray
                    };
                    tableLayoutPanel.Controls.Add(noLabel, colIdx++, row);

                    // 1번 열(Name)
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
                    // 0번 열(No) 숨김 → 1번 열(Name)부터 시작
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
                    nameLabel.Click += (s, e) =>
                    {
                        if (_selectedNameLabel != null)
                            _selectedNameLabel.BackColor = Color.White;
                        nameLabel.BackColor = Color.Yellow;
                        _selectedNameLabel = nameLabel;
                    };
                    tableLayoutPanel.Controls.Add(nameLabel, colIdx++, row);
                }

                // 2번 열(State)
                var statePictureBox = new PictureBox
                {
                    Dock = DockStyle.Fill,
                    Margin = new Padding(0),
                    Padding = new Padding(0),
                    SizeMode = PictureBoxSizeMode.StretchImage,
                    BorderStyle = BorderStyle.FixedSingle,
                    BackColor = Color.Empty
                };

                if (prop.State)
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

                tableLayoutPanel.Controls.Add(statePictureBox, colIdx, row);

                row++;
            }
            tableLayoutPanel.ResumeLayout();
        }
    }
}
