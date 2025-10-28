using System.Drawing;
using System.Windows.Forms;

namespace QMC.Common.CustomControl
{
    public class DeleteLogoBorderTableLayoutPanel : TableLayoutPanel
    {
        public Color BorderColor { get; set; } = Color.Black;
        public int BorderWidth { get; set; } = 1;

        public DeleteLogoBorderTableLayoutPanel()
        {
            this.CellBorderStyle = TableLayoutPanelCellBorderStyle.None;
            this.DoubleBuffered = true;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            int[] colWidths = this.GetColumnWidths();
            int[] rowHeights = this.GetRowHeights();

            int totalCols = this.ColumnCount;
            int totalRows = this.RowCount;

            int x, y;

            using (var pen = new Pen(BorderColor, BorderWidth))
            using (var whitePen = new Pen(Color.White, BorderWidth))
            {
                for (int col = 0; col < totalCols; col++)
                {
                    x = 0;
                    for (int i = 0; i < col; i++) x += colWidths[i];

                    for (int row = 0; row < totalRows; row++)
                    {
                        y = 0;
                        for (int j = 0; j < row; j++) y += rowHeights[j];

                        // 첫번째 행 윗쪽 테두리 추가
                        if (row == 0)
                        {
                            e.Graphics.DrawLine(pen,
                                x, y,
                                x + colWidths[col], y);
                        }

                        // 첫번째 열 왼쪽 테두리 추가
                        if (col == 0)
                        {
                            e.Graphics.DrawLine(pen,
                                x, y,
                                x, y + rowHeights[row]);
                        }

                        // 오른쪽 테두리 (마지막 열 제외)
                        if (col < totalCols - 1)
                        {
                            e.Graphics.DrawLine(pen,
                                x + colWidths[col] - BorderWidth / 2, y,
                                x + colWidths[col] - BorderWidth / 2, y + rowHeights[row]);
                        }
                        // 아래쪽 테두리 (마지막 행 제외)
                        if (row < totalRows - 1)
                        {
                            // (0,0) 셀의 아래쪽 테두리는 흰색으로 처리
                            if (col == 0 && row == 0)
                            {
                                e.Graphics.DrawLine(whitePen,
                                    x, y + rowHeights[row] - BorderWidth / 2,
                                    x + colWidths[col], y + rowHeights[row] - BorderWidth / 2);
                            }
                            else
                            {
                                e.Graphics.DrawLine(pen,
                                    x, y + rowHeights[row] - BorderWidth / 2,
                                    x + colWidths[col], y + rowHeights[row] - BorderWidth / 2);
                            }
                        }
                        // 마지막 열/행의 외곽선
                        if (col == totalCols - 1)
                        {
                            e.Graphics.DrawLine(pen,
                                x + colWidths[col] - BorderWidth / 2, y,
                                x + colWidths[col] - BorderWidth / 2, y + rowHeights[row]);
                        }
                        if (row == totalRows - 1)
                        {
                            e.Graphics.DrawLine(pen,
                                x, y + rowHeights[row] - BorderWidth / 2,
                                x + colWidths[col], y + rowHeights[row] - BorderWidth / 2);
                        }
                    }
                }
            }
        }
    }
}