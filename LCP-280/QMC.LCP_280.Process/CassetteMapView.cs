using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace LCP_280
{
    public partial class CassetteMapView : UserControl
    {
        private TabControl tabControl;
        private List<CassetteData> cassetteList = new List<CassetteData>();

        // 웨이퍼 간격 속성 추가
        public int WaferMargin { get; set; } = 5;

        // VisionImageView와 동일한 테마 속성 추가
        private Color _tabBorderColor = Color.Black;
        private int _tabBorderWidth = 2;
        private Font _tabFont = new Font("맑은 고딕", 9, FontStyle.Regular);

        public Color TabBorderColor
        {
            get => _tabBorderColor;
            set { _tabBorderColor = value; tabControl.Invalidate(); }
        }
        public int TabBorderWidth
        {
            get => _tabBorderWidth;
            set { _tabBorderWidth = Math.Max(1, value); tabControl.Invalidate(); }
        }
        public Font TabFont
        {
            get => _tabFont;
            set { _tabFont = value; tabControl.Invalidate(); }
        }

        public CassetteMapView()
        {
            InitializeComponent();
            this.AutoSize = false;
            tabControl = new TabControl
            {
                Dock = DockStyle.Fill,
                DrawMode = TabDrawMode.OwnerDrawFixed,
                ItemSize = new Size(120, 28), // VisionImageView와 동일
                SizeMode = TabSizeMode.Fixed,
                Font = _tabFont,
            };
            tabControl.DrawItem += TabControl_DrawItem;
            this.Controls.Add(tabControl);
        }

        // 카세트 리스트를 받아서 탭 생성
        public void SetCassettes(List<CassetteData> cassettes)
        {
            cassetteList = cassettes ?? new List<CassetteData>();
            tabControl.TabPages.Clear();

            for (int i = 0; i < cassetteList.Count; i++)
            {
                var cassette = cassetteList[i];
                string tabName = $"Cassette{(char)('A' + i)}";
                var tabPage = new TabPage(tabName);

                var panel = new Panel { Dock = DockStyle.Fill, BackColor = Color.Gray };

                // 상단: Cassette ID 표시
                var idPanel = new Panel
                {
                    Dock = DockStyle.Top,
                    Height = 32,
                    BackColor = Color.Black
                };
                var idLabel = new Label
                {
                    Text = "ID",
                    ForeColor = Color.Black,
                    BackColor = Color.White,
                    Font = new Font("맑은 고딕", 12, FontStyle.Bold),
                    Dock = DockStyle.Left,
                    Width = 40,
                    TextAlign = ContentAlignment.MiddleCenter,
                    BorderStyle = BorderStyle.FixedSingle
                };
                var valueLabel = new Label
                {
                    Text = cassette.CassetteId?.ToString() ?? "",
                    ForeColor = Color.LimeGreen,
                    BackColor = Color.Black,
                    Font = new Font("맑은 고딕", 12, FontStyle.Italic),
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleLeft
                };
                idPanel.Controls.Add(valueLabel);
                idPanel.Controls.Add(idLabel);

                panel.Controls.Add(idPanel); // 반드시 먼저 추가

                // 하단: WaferData 리스트 색상 표시
                int waferCount = cassette.WaferList.Count;
                int margin = WaferMargin;
                int waferHeight = 10; // 웨이퍼 하나의 높이(예시)
                int waferPanelHeight = waferHeight * waferCount + margin * (waferCount + 1);

                var waferPanel = new Panel
                {
                    Dock = DockStyle.Fill,
                    BackColor = Color.FromArgb(180, 176, 176),
                    Margin = new Padding(0, margin, 0, 0)
                };

                waferPanel.Tag = new Tuple<CassetteData, int>(cassette, idPanel.Height);
                waferPanel.Paint += WaferPanel_Paint;
                waferPanel.Resize += (s, e) => { (s as Panel)?.Invalidate(); };

                panel.Controls.Add(waferPanel); // idPanel 아래에 waferPanel 추가
                tabPage.Controls.Add(panel);
                tabControl.TabPages.Add(tabPage);

                // CassetteMapView(UserControl) 크기 동적 지정
                this.Height = idPanel.Height + waferPanel.Height;
                tabControl.Height = idPanel.Height + waferPanel.Height;
            }
        }

        private void TabControl_DrawItem(object sender, DrawItemEventArgs e)
        {
            TabPage page = tabControl.TabPages[e.Index];
            Rectangle tabRect = tabControl.GetTabRect(e.Index);

            // VisionImageView와 동일하게 선택된 탭은 White, 아닌 탭은 Gainsboro
            Color backColor = (e.Index == tabControl.SelectedIndex) ? Color.White : Color.Gainsboro;
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

            // 텍스트 그리기 (사용자 지정 폰트)
            TextRenderer.DrawText(
                e.Graphics,
                page.Text,
                _tabFont,
                tabRect,
                Color.Black,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter
            );
        }

        private void WaferPanel_Paint(object sender, PaintEventArgs e)
        {
            var panel = sender as Panel;
            if (panel == null || !(panel.Tag is Tuple<CassetteData, int> tag))
            {
                return;
            }

            var cassette = tag.Item1;
            int idPanelHeight = tag.Item2;

            int waferCount = cassette.WaferList.Count;
            if (waferCount == 0)
            {
                return;
            }

            // Antialiasing for smoother drawing
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            int panelHeight = panel.ClientSize.Height;
            int panelWidth = panel.ClientSize.Width;
            int margin = WaferMargin;
            int waferHeight = (panelHeight - margin * (waferCount + 1)) / waferCount;

            if (waferHeight <= 0) return;

            for (int w = 0; w < waferCount; w++)
            {
                var wafer = cassette.WaferList[w];
                Color waferColor = GetWaferColor(wafer.SlotStates[w]);

                int y = idPanelHeight + margin + w * (waferHeight + margin);

                Rectangle waferRect = new Rectangle(margin, y, panelWidth - 2 * margin, waferHeight);

                using (var brush = new SolidBrush(waferColor))
                {
                    e.Graphics.FillRectangle(brush, waferRect);
                }
                e.Graphics.DrawRectangle(Pens.Black, waferRect.X, waferRect.Y, waferRect.Width - 1, waferRect.Height - 1);
            }
        }

        // WaferCassetteLoadState에 따른 색상 반환
        private Color GetWaferColor(WaferCassetteLoadState state)
        {
            switch (state)
            {
                case WaferCassetteLoadState.Empty: return Color.Gray;
                case WaferCassetteLoadState.Present: return Color.DeepSkyBlue;
                case WaferCassetteLoadState.Loading: return Color.Yellow;
                case WaferCassetteLoadState.Loaded: return Color.Yellow;
                case WaferCassetteLoadState.Processing: return Color.Magenta;
                case WaferCassetteLoadState.Processed: return Color.LimeGreen;
                case WaferCassetteLoadState.Unloading: return Color.Gray;
                case WaferCassetteLoadState.Unloaded: return Color.Gray;
                default: return Color.LightGray;
            }
        }
    }
}
