using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace QMC.Common
{
    public partial class FormSetup : Form
    {
        private TabControl setupTabControl;
        private Dictionary<TabPage, Form> _tabFormInstances;

        private int _tabHeight = 32; // ★
        private Color _tabBorderColor = Color.Black;
        private int _tabBorderWidth = 2;
        private Font _tabFont = new Font("맑은 고딕", 10, FontStyle.Regular); // ★
        private const int _desiredTabRows = 2; // ★

        private bool _hostSized;

        // 재계산 재진입/중복 최소화
        private bool _isRecalcRunning; // ★ 추가
        private int _lastTabWidth = -1; // ★ 추가

        public FormSetup()
        {
            InitializeComponent();
            this.BackColor = Color.White;
            _tabFormInstances = new Dictionary<TabPage, Form>();
            InitializesetupUI();
            this.VisibleChanged += Formsetup_VisibleChanged;
        }

        private void Formsetup_VisibleChanged(object sender, EventArgs e)
        {
            if (!this.Visible) return;
            if (!_hostSized) return;
            BeginInvoke(new Action(() => UpdateActiveChildSize()));
        }

        private void InitializesetupUI()
        {
            setupTabControl = new TabControl
            {
                Dock = DockStyle.Fill,
                Font = _tabFont,
                DrawMode = TabDrawMode.OwnerDrawFixed,
                ItemSize = new Size(120, _tabHeight),
                SizeMode = TabSizeMode.Fixed,
                BackColor = Color.White,
                Multiline = true,          // ★
                Padding = new Point(12, 6) // ★
            };
            setupTabControl.DrawItem += setupTabControl_DrawItem;
            setupTabControl.SelectedIndexChanged += setupTabControl_SelectedIndexChanged;
            setupTabControl.Resize += (s, e) => RecalculateTabItemSize(); // ★

            this.Controls.Add(setupTabControl);
            LoadFormsFromManager();
            setupTabControl.Visible = true;
            setupTabControl.BringToFront();
            EnsureFirstTabLoaded();
            RecalculateTabItemSize(); // ★
        }

        private void RecalculateTabItemSize()
        {
            if (_isRecalcRunning) return; // 재진입 방지
            if (setupTabControl == null || setupTabControl.TabPages.Count == 0) return;
            try
            {
                _isRecalcRunning = true;
                int total = setupTabControl.TabPages.Count;
                int perRow = (int)Math.Ceiling(total / (double)_desiredTabRows);
                if (perRow <= 0) perRow = 1;
                int clientW = Math.Max(100, setupTabControl.ClientSize.Width - 8);
                int newWidth = Math.Max(90, (clientW / perRow) - 2);
                if (newWidth > 220) newWidth = 220;
                if (newWidth == _lastTabWidth) return; // 폭 변화 없으면 중단
                _lastTabWidth = newWidth;
                var newSize = new Size(newWidth, _tabHeight);
                if (setupTabControl.ItemSize != newSize)
                {
                    setupTabControl.ItemSize = newSize;
                    setupTabControl.Invalidate();
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
                if (setupTabControl == null || setupTabControl.TabPages.Count == 0) return;
                if (setupTabControl.SelectedIndex < 0) setupTabControl.SelectedIndex = 0;

                var first = setupTabControl.TabPages[0];
                var info = first.Tag as FormInfo;
                if (info != null && !_tabFormInstances.ContainsKey(first))
                    LoadFormIntoTab(first, info);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EnsureFirstTabLoaded(Setup) 실패: {ex.Message}");
            }
        }

        private void LoadFormsFromManager()
        {
            try
            {
                var setupForms = FormManager.Instance.GetRegisteredForms(MenuButtonType.Setup);
                foreach (var formInfo in setupForms)
                    CreateTabFromFormInfo(formInfo);
                if (setupForms.Count == 0) CreateSampleTabs();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"setup 폼 로드 중 오류 발생: {ex.Message}", "오류",
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
            setupTabControl.TabPages.Add(tabPage);
        }

        private void setupTabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            var selectedTab = setupTabControl.SelectedTab;
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
                if (setupTabControl == null) return;
                var selectedTab = setupTabControl.SelectedTab;
                if (selectedTab == null) return;
                if (!_tabFormInstances.ContainsKey(selectedTab)) return;
                var activeForm = _tabFormInstances[selectedTab];
                if (activeForm == null) return;

                int w = setupTabControl.ClientSize.Width;
                int h = setupTabControl.ClientSize.Height;
                if (w < 800 || h < 450) return;

                var mi = activeForm.GetType().GetMethod("SetPanelSize", new Type[] { typeof(int), typeof(int) });
                if (mi != null) mi.Invoke(activeForm, new object[] { w, h });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"UpdateActiveChildSize(Setup) 실패: {ex.Message}");
            }
        }

        public void RefreshsetupTabs()
        {
            foreach (var formInstance in _tabFormInstances.Values)
                formInstance?.Dispose();

            _tabFormInstances.Clear();
            setupTabControl.TabPages.Clear();
            LoadFormsFromManager();
            EnsureFirstTabLoaded();
            RecalculateTabItemSize(); // ★
            UpdateActiveChildSize();
        }

        private void setupTabControl_DrawItem(object sender, DrawItemEventArgs e)
        {
            TabPage page = setupTabControl.TabPages[e.Index];
            Rectangle tabRect = setupTabControl.GetTabRect(e.Index);
            Color backColor = (e.Index == setupTabControl.SelectedIndex) ? Color.White : Color.Gainsboro;
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
            var tab = new TabPage("Sample setup") { BackColor = Color.White };
            var lbl = new Label
            {
                Text = "No setup Forms Registered\r\n\r\nUse FormManager.Instance.RegisterForm() to add setup forms.",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("맑은 고딕", 12, FontStyle.Regular),
                BackColor = Color.White
            };
            tab.Controls.Add(lbl);
            setupTabControl.TabPages.Add(tab);
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
            RecalculateTabItemSize(); // ★
            UpdateActiveChildSize();
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
        public void SetBorderStyle(Color c, int w) { FormBorderColor = c; FormBorderWidth = w; }
        public void ResetBorderStyle() => SetBorderStyle(Color.Black, 2);
        public void HighlightBorder() => SetBorderStyle(Color.Red, 4);
    }
}
