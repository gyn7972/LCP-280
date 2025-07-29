using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SP_GridTypeView
{
    public partial class FormConfig : Form
    {
        private TabControl configTabControl;
        // Theme fields (copied from VisionImageView)
        private int _tabHeight = 28;
        private Color _tabBorderColor = Color.Black;
        private int _tabBorderWidth = 2;
        private Font _tabFont = new Font("맑은 고딕", 9, FontStyle.Regular);

        public FormConfig()
        {
            InitializeComponent();
            InitializeConfigUI();
        }
        
        private void InitializeConfigUI()
        {
            this.BackColor = Color.White;
            
            // TabControl 생성 및 테마 적용
            configTabControl = new TabControl();
            configTabControl.Dock = DockStyle.Fill;
            configTabControl.Font = _tabFont;
            configTabControl.DrawMode = TabDrawMode.OwnerDrawFixed;
            configTabControl.ItemSize = new Size(120, _tabHeight);
            configTabControl.SizeMode = TabSizeMode.Fixed;
            configTabControl.DrawItem += ConfigTabControl_DrawItem;
            this.Controls.Add(configTabControl);
            
            // 샘플 탭 추가
            CreateSampleTabs();
        }

        private void ConfigTabControl_DrawItem(object sender, DrawItemEventArgs e)
        {
            TabPage page = configTabControl.TabPages[e.Index];
            Rectangle tabRect = configTabControl.GetTabRect(e.Index);

            // 선택된 탭은 하얀색, 아닌 탭은 회색
            Color backColor = (e.Index == configTabControl.SelectedIndex) ? Color.White : Color.Gainsboro;
            using (Brush backBrush = new SolidBrush(backColor))
            {
                e.Graphics.FillRectangle(backBrush, tabRect);
            }

            // 테두리 그리기 (사용자 지정 색상과 두께)
            using (Pen borderPen = new Pen(_tabBorderColor, _tabBorderWidth))
            {
                Rectangle borderRect = tabRect;
                if (_tabBorderWidth > 1)
                {
                    borderRect.Inflate(-_tabBorderWidth / 2, -_tabBorderWidth / 2);
                }
                e.Graphics.DrawRectangle(borderPen, borderRect);
            }

            // 텍스트 그리기 (두 줄 처리)
            string text = page.Text;
            Size tabSize = tabRect.Size;
            StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            SizeF textSize = e.Graphics.MeasureString(text, _tabFont);

            if (textSize.Width > tabSize.Width - 8) // 8px padding
            {
                // 두 줄로 분할
                string[] words = text.Split(' ');
                string line1 = words[0];
                string line2 = string.Join(" ", words.Skip(1));
                // 만약 단어가 2개 이상이면, 첫 단어와 나머지로 분리
                if (words.Length > 1)
                {
                    // line1에 단어를 추가하면서 width 체크
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
                // 두 줄로 그리기
                RectangleF line1Rect = new RectangleF(tabRect.X, tabRect.Y + 2, tabRect.Width, tabRect.Height / 2 - 2);
                RectangleF line2Rect = new RectangleF(tabRect.X, tabRect.Y + tabRect.Height / 2, tabRect.Width, tabRect.Height / 2 - 2);
                e.Graphics.DrawString(line1, _tabFont, Brushes.Black, line1Rect, sf);
                e.Graphics.DrawString(line2, _tabFont, Brushes.Black, line2Rect, sf);
            }
            else
            {
                // 한 줄로 그리기
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
        
        private void CreateSampleTabs()
        {
            // System Config 탭
            TabPage systemTab = new TabPage("Input Loader");
            Label systemLabel = new Label();
            systemLabel.Text = "System Configuration Settings";
            systemLabel.Font = new Font("Arial", 12, FontStyle.Bold);
            systemLabel.Dock = DockStyle.Top;
            systemTab.Controls.Add(systemLabel);
            configTabControl.TabPages.Add(systemTab);
        }
    }
}
