using System;
using System.Drawing;
using System.Windows.Forms;

namespace QMC.Common.CustomControl
{
    public class DeleteInnerBorderTableLayoutPanel : TableLayoutPanel
    {
        public Color OuterBorderColor { get; set; } = Color.Black;
        public int OuterBorderWidth { get; set; } = 1;

        public Color InnerBorderColor { get; set; } = Color.White;
        public int InnerBorderWidth { get; set; } = 2;

        public bool ShowInnerBorder { get; set; } = false;

        public DeleteInnerBorderTableLayoutPanel()
        {
            this.CellBorderStyle = TableLayoutPanelCellBorderStyle.None;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            // 외부 테두리 (전체)
            using (Pen pen = new Pen(OuterBorderColor, OuterBorderWidth))
            {
                Rectangle rect = this.ClientRectangle;
                rect.Width -= OuterBorderWidth;
                rect.Height -= OuterBorderWidth;
                e.Graphics.DrawRectangle(pen, rect);
            }

            // 0열 좌측 외부 테두리만 흰색으로 덮어 그림
            using (Pen whitePen = new Pen(Color.White, OuterBorderWidth))
            {
                int x0 = 0;
                int y = 0;
                for (int row = 0; row < RowCount; row++)
                {
                    int rowHeight = GetRowHeight(row);
                    e.Graphics.DrawLine(whitePen, x0, y, x0, y + rowHeight);
                    y += rowHeight;
                }
            }

            // 내부 셀 테두리
            if (ShowInnerBorder && RowCount > 1 && ColumnCount > 1)
            {
                using (Pen pen = new Pen(InnerBorderColor, InnerBorderWidth))
                {
                    int w = this.Width;
                    int h = this.Height;

                    // 열 경계선
                    for (int col = 1; col < ColumnCount; col++)
                    {
                        int x = 0;
                        for (int c = 0; c < col; c++)
                            x += GetColumnWidth(c);
                        e.Graphics.DrawLine(pen, x, 0, x, h);
                    }
                    // 행 경계선
                    for (int row = 1; row < RowCount; row++)
                    {
                        int y = 0;
                        for (int r = 0; r < row; r++)
                            y += GetRowHeight(r);
                        e.Graphics.DrawLine(pen, 0, y, w, y);
                    }
                }
            }
        }

        private int GetColumnWidth(int col)
        {
            int[] widths = this.GetColumnWidths();
            return (col >= 0 && col < widths.Length) ? widths[col] : 0;
        }

        private int GetRowHeight(int row)
        {
            int[] heights = this.GetRowHeights();
            return (row >= 0 && row < heights.Length) ? heights[row] : 0;
        }
    }
}