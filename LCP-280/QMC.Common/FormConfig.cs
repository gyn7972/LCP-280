using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace QMC.Common
{
    public partial class FormConfig : Form
    {
        private TabControl configTabControl;
        private Dictionary<TabPage, Form> _tabFormInstances;

        // Theme fields
        private int _tabHeight = 32; // ★ 높이 확대 (기존 28)
        private Color _tabBorderColor = Color.Black;
        private int _tabBorderWidth = 2;
        private Font _tabFont = new Font("맑은 고딕", 10, FontStyle.Regular); // ★ 폰트 약간 확대

        // 2줄 고정 설정
        private const int _desiredTabRows = 2; // ★ 추가

        private bool _hostSized;

        // 재계산 재진입/중복 최소화
        private bool _isRecalcRunning; // ★ 추가
        private int _lastTabWidth = -1; // ★ 추가

        public FormConfig()
        {
            InitializeComponent();
            this.BackColor = Color.White;
            _tabFormInstances = new Dictionary<TabPage, Form>();
            InitializeconfigUI();
            this.VisibleChanged += Formconfig_VisibleChanged;
        }

        private void Formconfig_VisibleChanged(object sender, EventArgs e)
        {
            if (!this.Visible) return;
            if (!_hostSized) return;
            BeginInvoke(new Action(() => UpdateActiveChildSize()));
        }

        private void InitializeconfigUI()
        {
            configTabControl = new TabControl
            {
                Dock = DockStyle.Fill,
                Font = _tabFont,
                DrawMode = TabDrawMode.OwnerDrawFixed,
                ItemSize = new Size(120, _tabHeight),
                SizeMode = TabSizeMode.Fixed,
                BackColor = Color.White,
                Multiline = true,              // ★ 2줄 이상 허용
                Padding = new Point(12, 6)     // ★ 상하 여백 (텍스트 배치용)
            };
            configTabControl.DrawItem += configTabControl_DrawItem;
            configTabControl.SelectedIndexChanged += configTabControl_SelectedIndexChanged;
            configTabControl.Resize += (s, e) => RecalculateTabItemSize(); // ★ 리사이즈 대응

            this.Controls.Add(configTabControl);

            LoadFormsFromManager();
            configTabControl.Visible = true;
            configTabControl.BringToFront();
            EnsureFirstTabLoaded();

            // ★ 탭 로드 후 폭/줄 재계산
            RecalculateTabItemSize();
        }

        // ★ 탭 폭/높이 재계산: 항상 2줄 목표 + 재진입/불필요 갱신 방지
        private void RecalculateTabItemSize()
        {
            if (_isRecalcRunning) return; // 재진입 방지
            if (configTabControl == null || configTabControl.TabPages.Count == 0) return;

            try
            {
                _isRecalcRunning = true;
                int total = configTabControl.TabPages.Count;
                int rows = _desiredTabRows;
                int perRow = (int)Math.Ceiling(total / (double)rows);
                if (perRow <= 0) perRow = 1;

                int clientW = Math.Max(100, configTabControl.ClientSize.Width - 8);
                int newWidth = Math.Max(90, (clientW / perRow) - 2);
                if (newWidth > 220) newWidth = 220;

                // 폭이 변하지 않으면 갱신 생략 -> 불필요한 Resize 루프 방지
                if (newWidth == _lastTabWidth) return;
                _lastTabWidth = newWidth;

                var newSize = new Size(newWidth, _tabHeight);
                if (configTabControl.ItemSize != newSize)
                {
                    configTabControl.ItemSize = newSize; // 내부적으로 레이아웃 발생
                    configTabControl.Invalidate();
                }
            }
            finally
            {
                _isRecalcRunning = false;
            }
        }

        private void EnsureFirstTabLoaded()
        {
            try
            {
                if (configTabControl == null || configTabControl.TabPages.Count == 0) return;
                if (configTabControl.SelectedIndex < 0) configTabControl.SelectedIndex = 0;

                var first = configTabControl.TabPages[0];
                var info = first.Tag as FormInfo;
                if (info != null && !_tabFormInstances.ContainsKey(first))
                {
                    LoadFormIntoTab(first, info);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EnsureFirstTabLoaded(Config) 실패: {ex.Message}");
            }
        }

        private void LoadFormsFromManager()
        {
            try
            {
                var configForms = FormManager.Instance.GetRegisteredForms(MenuButtonType.Config);

                var desiredOrder = new[]
                {
                    "InputCassetteLifter","InputFeeder","InputStage","InputStageEjector",
                    "InputDieTransfer","Rotary","IndexLoadAligner","IndexChipProbeController",
                    "IndexChipProber","IndexUnloadAligner","OutputDieTransfer","OutputStage",
                    "OutputCassetteLifter","OutputFeeder"
                };
                var indexMap = desiredOrder
                    .Select((name, idx) => new { name, idx })
                    .ToDictionary(x => x.name, x => x.idx);

                configForms = configForms
                    .OrderBy(f => indexMap.ContainsKey(f.DisplayName) ? indexMap[f.DisplayName] : int.MaxValue)
                    .ThenBy(f => f.DisplayName)
                    .ToList();

                foreach (var formInfo in configForms)
                    CreateTabFromFormInfo(formInfo);

                if (configForms.Count == 0)
                    CreateSampleTabs();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"config 폼 로드 중 오류 발생: {ex.Message}", "오류",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                CreateSampleTabs();
            }
        }

        private void CreateTabFromFormInfo(FormInfo formInfo)
        {
            var tabPage = new TabPage(formInfo.DisplayName)
            {
                Tag = formInfo,
                BackColor = Color.White
            };
            configTabControl.TabPages.Add(tabPage);
        }

        private void configTabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            var selectedTab = configTabControl.SelectedTab;
            if (selectedTab?.Tag is FormInfo formInfo)
            {
                LoadFormIntoTab(selectedTab, formInfo);
                if (_hostSized)
                    UpdateActiveChildSize();
            }
        }

        private void LoadFormIntoTab(TabPage tabPage, FormInfo formInfo)
        {
            try
            {
                if (!_tabFormInstances.ContainsKey(tabPage))
                {
                    Form formInstance = FormManager.Instance.CreateFormInstance(formInfo);
                    formInstance.TopLevel = false;
                    formInstance.FormBorderStyle = FormBorderStyle.None;
                    formInstance.Dock = DockStyle.Fill;
                    tabPage.Controls.Clear();
                    tabPage.Controls.Add(formInstance);
                    formInstance.Show();
                    _tabFormInstances[tabPage] = formInstance;
                }
                else
                {
                    _tabFormInstances[tabPage].Show();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"폼 로드 중 오류 발생: {ex.Message}", "오류",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                var lbl = new Label
                {
                    Text = $"폼 로드 실패: {formInfo.DisplayName}",
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = new Font("맑은 고딕", 12, FontStyle.Bold),
                    ForeColor = Color.Red
                };
                tabPage.Controls.Clear();
                tabPage.Controls.Add(lbl);
            }
        }

        private void UpdateActiveChildSize()
        {
            try
            {
                if (configTabControl == null) return;
                var selectedTab = configTabControl.SelectedTab;
                if (selectedTab == null) return;
                if (!_tabFormInstances.ContainsKey(selectedTab)) return;
                var activeForm = _tabFormInstances[selectedTab];
                if (activeForm == null) return;

                int availableWidth = configTabControl.ClientSize.Width;
                int availableHeight = configTabControl.ClientSize.Height;
                if (availableWidth < 800 || availableHeight < 450) return;

                var setPanelSizeMethod = activeForm.GetType()
                    .GetMethod("SetPanelSize", new Type[] { typeof(int), typeof(int) });
                if (setPanelSizeMethod != null)
                    setPanelSizeMethod.Invoke(activeForm, new object[] { availableWidth, availableHeight });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"UpdateActiveChildSize(Config) 실패: {ex.Message}");
            }
        }

        public void RefreshconfigTabs()
        {
            foreach (var formInstance in _tabFormInstances.Values)
                formInstance?.Dispose();

            _tabFormInstances.Clear();
            configTabControl.TabPages.Clear();
            LoadFormsFromManager();
            EnsureFirstTabLoaded();
            RecalculateTabItemSize(); // ★ 갱신
            UpdateActiveChildSize();
        }

        private void configTabControl_DrawItem(object sender, DrawItemEventArgs e)
        {
            TabPage page = configTabControl.TabPages[e.Index];
            Rectangle tabRect = configTabControl.GetTabRect(e.Index);

            Color backColor = (e.Index == configTabControl.SelectedIndex) ? Color.White : Color.Gainsboro;
            using (Brush b = new SolidBrush(backColor))
                e.Graphics.FillRectangle(b, tabRect);

            using (Pen pen = new Pen(_tabBorderColor, _tabBorderWidth))
            {
                Rectangle borderRect = tabRect;
                if (_tabBorderWidth > 1)
                    borderRect.Inflate(-_tabBorderWidth / 2, -_tabBorderWidth / 2);
                e.Graphics.DrawRectangle(pen, borderRect);
            }

            string text = page.Text;
            SizeF textSize = e.Graphics.MeasureString(text, _tabFont);
            if (textSize.Width > tabRect.Width - 10)
            {
                string[] words = text.Split(' ');
                string line1 = words[0];
                string line2 = string.Join(" ", words.Skip(1));
                if (words.Length > 1)
                {
                    for (int i = 1; i < words.Length; i++)
                    {
                        string testLine = line1 + " " + words[i];
                        if (e.Graphics.MeasureString(testLine, _tabFont).Width < tabRect.Width - 10)
                        {
                            line1 = testLine;
                            line2 = string.Join(" ", words.Skip(i + 1));
                        }
                        else break;
                    }
                }
                int half = tabRect.Height / 2;
                RectangleF r1 = new RectangleF(tabRect.X, tabRect.Y + 2, tabRect.Width, half - 2);
                RectangleF r2 = new RectangleF(tabRect.X, tabRect.Y + half - 2, tabRect.Width, half);
                StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                e.Graphics.DrawString(line1, _tabFont, Brushes.Black, r1, sf);
                e.Graphics.DrawString(line2, _tabFont, Brushes.Black, r2, sf);
            }
            else
            {
                TextRenderer.DrawText(
                    e.Graphics,
                    text,
                    _tabFont,
                    tabRect,
                    Color.Black,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis
                );
            }
        }

        private void CreateSampleTabs()
        {
            var tab = new TabPage("Sample config") { BackColor = Color.White };
            var lbl = new Label
            {
                Text = "No config Forms Registered\r\n\r\nUse FormManager.Instance.RegisterForm() to add config forms.",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("맑은 고딕", 12, FontStyle.Regular),
                BackColor = Color.White
            };
            tab.Controls.Add(lbl);
            configTabControl.TabPages.Add(tab);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            foreach (var formInstance in _tabFormInstances.Values)
                formInstance?.Dispose();
            _tabFormInstances.Clear();
            base.OnFormClosed(e);
        }

        public void SetPanelSize(int width, int height)
        {
            this.Size = new Size(width, height);
            this.ClientSize = new Size(width, height);
            _hostSized = true;
            RecalculateTabItemSize(); // ★ 부모 크기 반영 후 재계산
            UpdateActiveChildSize();
            this.Invalidate();
            this.Update();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            using (Pen pen = new Pen(FormBorderColor, FormBorderWidth))
                e.Graphics.DrawRectangle(pen, new Rectangle(0, 0, this.ClientSize.Width - 1, this.ClientSize.Height - 1));
        }
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            this.Invalidate();
        }
        public void SetBorderStyle(Color color, int width)
        {
            FormBorderColor = color;
            FormBorderWidth = width;
        }
        public void ResetBorderStyle() => SetBorderStyle(Color.Black, 2);
        public void HighlightBorder() => SetBorderStyle(Color.Red, 4);
    }
}
