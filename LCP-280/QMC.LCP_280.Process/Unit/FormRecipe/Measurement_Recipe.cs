using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using System.Windows.Controls;
using System.Windows.Forms;

namespace QMC.LCP_280.Process.Unit
{
    public partial class Measurement_Recipe : Form
    {
        public Measurement_Recipe()
        {
            InitializeComponent();
        }

        private readonly int _tabBorderWidth = 2;
        private readonly Color _tabBorderColor = Color.Black;
        private readonly Font _tabFont = new Font("맑은 고딕", 9, FontStyle.Regular);

        private void tabControl1_DrawItem(object sender, DrawItemEventArgs e)
        {
            TabControl tab = sender as TabControl;
            TabPage page = tab.TabPages[e.Index];
            Rectangle tabRect = tab.GetTabRect(e.Index);
            Color backColor = (e.Index == tab.SelectedIndex) ? Color.White : Color.Gainsboro;
            using (Brush backBrush = new SolidBrush(backColor))
            {
                e.Graphics.FillRectangle(backBrush, tabRect);
            }
            using (Pen borderPen = new Pen(_tabBorderColor, _tabBorderWidth))
            {
                Rectangle borderRect = tabRect;
                if (_tabBorderWidth > 1)
                {
                    borderRect.Inflate(-_tabBorderWidth / 2, -_tabBorderWidth / 2);
                }
                e.Graphics.DrawRectangle(borderPen, borderRect);
            }
            string text = page.Text;
            Size tabSize = tabRect.Size;
            StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            SizeF textSize = e.Graphics.MeasureString(text, _tabFont);

            if (textSize.Width > tabSize.Width - 8)
            {
                string[] words = text.Split(' ');
                string line1 = words[0];
                string line2 = string.Join(" ", words.Skip(1));
                if (words.Length > 1)
                {
                    for (int i = 1; i < words.Length; i++)
                    {
                        string testLine = line1 + " " + words[i];
                        if (e.Graphics.MeasureString(testLine, _tabFont).Width < tabSize.Width - 8)
                        {
                            line1 = testLine;
                            line2 = string.Join(" ", words.Skip(i + 1));
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                RectangleF line1Rect = new RectangleF(tabRect.X, tabRect.Y + 2, tabRect.Width, tabRect.Height / 2 - 2);
                RectangleF line2Rect = new RectangleF(tabRect.X, tabRect.Y + tabRect.Height / 2, tabRect.Width, tabRect.Height / 2 - 2);
                e.Graphics.DrawString(line1, _tabFont, Brushes.Black, line1Rect, sf);
                e.Graphics.DrawString(line2, _tabFont, Brushes.Black, line2Rect, sf);
            }
            else
            {
                TextRenderer.DrawText(
                    e.Graphics,
                    text,
                    _tabFont,
                    tabRect,
                    Color.Black,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter
                );
            }
        }
    }
}