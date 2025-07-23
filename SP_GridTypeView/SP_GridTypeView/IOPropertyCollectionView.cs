using System.Drawing;
using System.Windows.Forms;

namespace SP_GridTypeView
{
    public class IOPropertyCollectionView : PropertyCollectionView
    {
        public IOPropertyCollectionView() : base() { }

        public new void SetProperties(PropertyCollection properties)
        {
            var tableLayoutPanelField = typeof(PropertyCollectionView).GetField("tableLayoutPanel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var tableLayoutPanel = tableLayoutPanelField.GetValue(this) as TableLayoutPanel;
            tableLayoutPanel.SuspendLayout();
            tableLayoutPanel.Controls.Clear();
            tableLayoutPanel.RowStyles.Clear();
            tableLayoutPanel.RowCount = 0;

            // State 열 추가
            tableLayoutPanel.ColumnCount = 3;
            if (tableLayoutPanel.ColumnStyles.Count < 3)
                tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            tableLayoutPanel.ColumnStyles[0].Width = 40F;
            tableLayoutPanel.ColumnStyles[1].Width = 40F;
            tableLayoutPanel.ColumnStyles[2].Width = 20F;

            int row = 0;
            foreach (var prop in properties)
            {
                tableLayoutPanel.RowCount++;
                tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));

                var titleLabel = new Label
                {
                    Text = prop.Title,
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleLeft,
                    AutoSize = false,
                    Margin = new Padding(0),
                    Padding = new Padding(0),
                    BackColor = Color.LightGray
                };

                var valueLabel = new Label
                {
                    Text = (string)prop.Value,
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleLeft,
                    AutoSize = false,
                    Margin = new Padding(0),
                    Padding = new Padding(0),
                    BackColor = Color.White
                };

                // State를 표시할 PictureBox로 변경
                var statePictureBox = new PictureBox
                {
                    Dock = DockStyle.Fill,
                    Margin = new Padding(0),
                    Padding = new Padding(0),
                    SizeMode = PictureBoxSizeMode.StretchImage,
                    BackColor = Color.Empty
                };

                if (prop is PropertyState ps)
                {
                    if (ps.State)
                    {
                        // Input.PNG 이미지를 표시
                        try
                        {
                            statePictureBox.Image = Properties.Resources.Input;
                        }
                        catch
                        {
                            statePictureBox.Image = null; // 이미지가 없으면 표시하지 않음
                        }
                    }
                    else
                    {
                        statePictureBox.Image = null;
                    }
                }

                tableLayoutPanel.Controls.Add(titleLabel, 0, row);
                tableLayoutPanel.Controls.Add(valueLabel, 1, row);
                tableLayoutPanel.Controls.Add(statePictureBox, 2, row);

                row++;
            }
            tableLayoutPanel.ResumeLayout();
        }
    }
}
