using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace QMC.Common
{
    public partial class FormMain : Form
    {
        public interface ITabActivationAware
        {
            void OnActivatedInTab();
            void OnDeactivatedInTab();
        }
        // FormMain 클래스 내부 필드 추가
        private TabPage _lastActiveTab;

        // ==X 버튼 비활성화 코드 시작==
        private const int CS_NOCLOSE = 0x200;
        private const int WM_SYSCOMMAND = 0x0112;
        private const int SC_CLOSE = 0xF060;

        private const int MF_BYCOMMAND = 0x00000000;
        private const int MF_GRAYED = 0x00000001;
        private const int MF_DISABLED = 0x00000002;

        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                // 시스템 메뉴의 Close 항목 비활성화(X 버튼 회색)
                cp.ClassStyle |= CS_NOCLOSE;
                return cp;
            }
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

            // 최소화/최대화는 유지
            this.ControlBox = true;
            this.MinimizeBox = true;
            this.MaximizeBox = true;

            // 시스템 메뉴에서 Close 항목 회색 처리 및 제거
            IntPtr hMenu = GetSystemMenu(this.Handle, false);
            if (hMenu != IntPtr.Zero)
            {
                EnableMenuItem(hMenu, (uint)SC_CLOSE, MF_BYCOMMAND | MF_GRAYED | MF_DISABLED);
                RemoveMenu(hMenu, (uint)SC_CLOSE, MF_BYCOMMAND);
                DrawMenuBar(this.Handle);
            }
        }

        protected override void WndProc(ref Message m)
        {
            // X 버튼, Alt+F4, 시스템 메뉴 Close 등 SC_CLOSE을 무시
            if (m.Msg == WM_SYSCOMMAND && ((m.WParam.ToInt32() & 0xFFF0) == SC_CLOSE))
                return;

            base.WndProc(ref m);
        }

        [DllImport("user32.dll")] private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);
        [DllImport("user32.dll")] private static extern bool RemoveMenu(IntPtr hMenu, uint uPosition, uint uFlags);
        [DllImport("user32.dll")] private static extern bool EnableMenuItem(IntPtr hMenu, uint uIDEnableItem, uint uEnable);
        [DllImport("user32.dll")] private static extern bool DrawMenuBar(IntPtr hWnd);
        // ==X 버튼 비활성화 코드 끝==


        private TabControl mainTabControl;
        private Dictionary<TabPage, Form> _tabFormInstances;

        // Theme fields
        private int _tabHeight = 32;
        private Color _tabBorderColor = Color.Black;
        private int _tabBorderWidth = 2;
        private Font _tabFont = new Font("맑은 고딕", 10, FontStyle.Bold);

        // 호스트에서 유효 크기를 한 번이라도 전달받았는지
        private bool _hostSized;

        public FormMain()
        {
            InitializeComponent();

            // 배경색을 흰색으로 설정
            this.BackColor = Color.White;

            _tabFormInstances = new Dictionary<TabPage, Form>();
            InitializemainUI();

            // Visible 상태 변경 이벤트: 자식 크기만 동기화
            this.VisibleChanged += Formmain_VisibleChanged;
        }

        /// <summary>
        /// FormMain이 보여질 때 탭 자식 크기만 갱신(호스트가 최종 크기를 전달)
        /// </summary>
        private void Formmain_VisibleChanged(object sender, EventArgs e)
        {
            if (!this.Visible) return;
            // 호스트에서 최종 사이즈를 전달받기 전이면 무시
            if (!_hostSized) return;
            BeginInvoke(new Action(() => UpdateActiveChildSize()));
        }

        private void InitializemainUI()
        {
            Console.WriteLine("Formmain.InitializemainUI() 시작");

            // Formmain 배경색을 확실히 흰색으로 설정
            this.BackColor = Color.White;

            // TabControl 생성 및 테마 적용
            mainTabControl = new TabControl();
            // Dock=Fill로 즉시 부모를 가득 채움 → 초기 작은 사이즈 전달 방지
            mainTabControl.Dock = DockStyle.Fill;
            mainTabControl.Font = _tabFont;
            mainTabControl.DrawMode = TabDrawMode.OwnerDrawFixed;
            mainTabControl.ItemSize = new Size(120, _tabHeight);
            mainTabControl.SizeMode = TabSizeMode.Fixed;
            mainTabControl.DrawItem += mainTabControl_DrawItem;
            mainTabControl.SelectedIndexChanged += mainTabControl_SelectedIndexChanged;

            // TabControl 배경색도 흰색으로 설정
            mainTabControl.BackColor = Color.White;

            Console.WriteLine($"TabControl 생성 완료: Size={mainTabControl.Size}, Visible={mainTabControl.Visible}");

            this.Controls.Add(mainTabControl);

            Console.WriteLine($"TabControl을 Formmain에 추가 완료");
            Console.WriteLine($"Formmain.Controls.Count: {this.Controls.Count}");

            // FormManager에서 등록된 main 폼들을 자동으로 탭으로 추가
            LoadFormsFromManager();

            // 강제로 TabControl을 보이게 설정
            mainTabControl.Visible = true;
            mainTabControl.BringToFront();

            // 첫 탭 즉시 로드 (크기 전달은 이후 일괄 처리)
            EnsureFirstTabLoaded();

            Console.WriteLine($"InitializemainUI 완료");
            Console.WriteLine($"최종 TabControl 상태: Visible={mainTabControl.Visible}, TabCount={mainTabControl.TabPages.Count}");
        }

        private void LoadFormsFromManager()
        {
            try
            {
                Console.WriteLine("Formmain.LoadFormsFromManager() 시작");

                var mainForms = FormManager.Instance.GetRegisteredForms(MenuButtonType.Main);
                Console.WriteLine($"등록된 main 폼 개수: {mainForms.Count}");

                foreach (var formInfo in mainForms)
                {
                    Console.WriteLine($"main 폼 발견: {formInfo.DisplayName} ({formInfo.FormType.Name})");
                    CreateTabFromFormInfo(formInfo);
                }

                if (mainForms.Count == 0)
                {
                    Console.WriteLine("등록된 main 폼이 없어서 기본 샘플 탭 생성");
                    CreateSampleTabs();
                }

                Console.WriteLine($"최종 탭 개수: {mainTabControl.TabPages.Count}");
                Console.WriteLine($"mainTabControl.Visible: {mainTabControl.Visible}");
                Console.WriteLine($"mainTabControl.Size: {mainTabControl.Size}");
                Console.WriteLine($"mainTabControl.Dock: {mainTabControl.Dock}");
                Console.WriteLine($"Formmain.Visible: {this.Visible}");
                Console.WriteLine($"Formmain.Size: {this.Size}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"main 폼 로드 중 오류: {ex.Message}");

                var mb = new MessageBoxOk();
                mb.ShowDialog("Error!", $"main 폼 로드 중 오류 발생: {ex.Message}");

                CreateSampleTabs();
            }
        }

        private void CreateTabFromFormInfo(FormInfo formInfo)
        {
            Console.WriteLine($"탭 생성: {formInfo.DisplayName}");
            TabPage tabPage = new TabPage(formInfo.DisplayName);
            tabPage.Tag = formInfo;
            tabPage.BackColor = Color.White;
            mainTabControl.TabPages.Add(tabPage);
            Console.WriteLine($"   탭 추가 완료. 현재 탭 수: {mainTabControl.TabPages.Count}");
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
                    var existingForm = _tabFormInstances[tabPage];
                    existingForm.Show();
                }
            }
            catch (Exception ex)
            {
                var mb = new MessageBoxOk();
                mb.ShowDialog("Error!", $"폼 로드 중 오류 발생: {ex.Message}");

                Label errorLabel = new Label
                {
                    Text = $"폼 로드 실패: {formInfo.DisplayName}",
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = new Font("맑은 고딕", 12, FontStyle.Bold),
                    ForeColor = Color.Red
                };
                tabPage.Controls.Clear();
                tabPage.Controls.Add(errorLabel);
            }
        }

        private void UpdateActiveChildSize()
        {
            try
            {
                if (mainTabControl == null) return;
                var selectedTab = mainTabControl.SelectedTab;
                if (selectedTab == null) return;
                if (!_tabFormInstances.ContainsKey(selectedTab)) return;
                var activeForm = _tabFormInstances[selectedTab];
                if (activeForm == null) return;

                int availableWidth = mainTabControl.ClientSize.Width;
                int availableHeight = mainTabControl.ClientSize.Height;
                // 너무 작은 초기값(예: 600x400) 필터링
                if (availableWidth < 800 || availableHeight < 450)
                {
                    Console.WriteLine($"   ?? 크기 전달 보류(Working): {availableWidth}x{availableHeight}");
                    return;
                }
                var setPanelSizeMethod = activeForm.GetType().GetMethod("SetPanelSize", new Type[] { typeof(int), typeof(int) });
                if (setPanelSizeMethod != null)
                {
                    Console.WriteLine($"   활성 폼 {activeForm.GetType().Name}에 정확한 크기 전달(Main): {availableWidth}x{availableHeight}");
                    setPanelSizeMethod.Invoke(activeForm, new object[] { availableWidth, availableHeight });
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
        }

        public void RefreshmainTabs()
        {
            foreach (var formInstance in _tabFormInstances.Values)
            {
                formInstance?.Dispose();
            }
            _tabFormInstances.Clear();
            mainTabControl.TabPages.Clear();
            LoadFormsFromManager();
            EnsureFirstTabLoaded();
            UpdateActiveChildSize();
        }

        private void mainTabControl_DrawItem(object sender, DrawItemEventArgs e)
        {
            TabPage page = mainTabControl.TabPages[e.Index];
            Rectangle tabRect = mainTabControl.GetTabRect(e.Index);
            Color backColor = (e.Index == mainTabControl.SelectedIndex) ? Color.White : Color.Gainsboro;
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

        private void CreateSampleTabs()
        {
            TabPage systemTab = new TabPage("Sample main");
            systemTab.BackColor = Color.White;
            Label systemLabel = new Label();
            systemLabel.Text = "No main Forms Registered\n\nUse FormManager.Instance.RegisterForm() to add main forms.";
            systemLabel.Font = new Font("맑은 고딕", 12, FontStyle.Regular);
            systemLabel.TextAlign = ContentAlignment.MiddleCenter;
            systemLabel.Dock = DockStyle.Fill;
            systemLabel.BackColor = Color.White;
            systemTab.Controls.Add(systemLabel);
            mainTabControl.TabPages.Add(systemTab);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            foreach (var formInstance in _tabFormInstances.Values)
            {
                formInstance?.Dispose();
            }
            _tabFormInstances.Clear();
            base.OnFormClosed(e);
        }

        public void SetPanelSize(int width, int height)
        {
            Console.WriteLine($"Formmain.SetPanelSize() 호출: width={width}, height={height}");
            this.Size = new Size(width, height);
            this.ClientSize = new Size(width, height);
            _hostSized = true; // 호스트에서 유효 사이즈 전달 받음
            UpdateActiveChildSize();
            this.Invalidate();
            this.Update();
            Console.WriteLine($"Formmain.SetPanelSize() 완료: 최종 크기={this.Size}");
        }

        #region Form Border Drawing
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            using (Pen borderPen = new Pen(FormBorderColor, FormBorderWidth))
            {
                Rectangle borderRect = new Rectangle(0, 0, this.ClientSize.Width - 1, this.ClientSize.Height - 1);
                e.Graphics.DrawRectangle(borderPen, borderRect);
            }
            Console.WriteLine($"Formmain 테두리 그리기: Color={FormBorderColor}, Width={FormBorderWidth}, Size={this.ClientSize}");
        }
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            this.Invalidate();
        }
        #endregion

        #region Border Customization Methods
        public void SetBorderStyle(Color color, int width)
        {
            FormBorderColor = color;
            FormBorderWidth = width;
            Console.WriteLine($"Formmain 테두리 스타일 변경: Color={color}, Width={width}");
        }
        public void ResetBorderStyle()
        {
            SetBorderStyle(Color.Black, 2);
        }
        public void HighlightBorder()
        {
            SetBorderStyle(Color.Red, 4);
        }
        #endregion


        

        // SelectedIndexChanged 수정
        private void mainTabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            var newTab = mainTabControl.SelectedTab;
            if (newTab == null) return;

            // 이전 탭 비활성
            if (_lastActiveTab != null && _tabFormInstances.TryGetValue(_lastActiveTab, out var prevForm))
            {
                var awarePrev = prevForm as ITabActivationAware;
                awarePrev?.OnDeactivatedInTab();
            }

            // 신규 탭 폼 로드
            if (newTab.Tag is FormInfo formInfo)
                LoadFormIntoTab(newTab, formInfo);

            // 신규 탭 활성
            if (_tabFormInstances.TryGetValue(newTab, out var newForm))
            {
                var awareNew = newForm as ITabActivationAware;
                awareNew?.OnActivatedInTab();
            }

            _lastActiveTab = newTab;

            if (_hostSized)
                UpdateActiveChildSize();
        }

        //private void mainTabControl_SelectedIndexChanged(object sender, EventArgs e)
        //{
        //    TabPage selectedTab = mainTabControl.SelectedTab;
        //    if (selectedTab?.Tag is FormInfo formInfo)
        //    {
        //        LoadFormIntoTab(selectedTab, formInfo);
        //        // 호스트에서 유효 사이즈를 받기 전에는 자식 크기 전달 보류
        //        if (_hostSized)
        //            UpdateActiveChildSize();
        //    }
        //}

        // 첫 탭 로드 직후 활성화 처리
        private void EnsureFirstTabLoaded()
        {
            try
            {
                if (mainTabControl == null || mainTabControl.TabPages.Count == 0) return;
                if (mainTabControl.SelectedIndex < 0) mainTabControl.SelectedIndex = 0;

                var first = mainTabControl.TabPages[0];
                var info = first.Tag as FormInfo;
                if (info != null && !_tabFormInstances.ContainsKey(first))
                {
                    LoadFormIntoTab(first, info);
                }

                if (_tabFormInstances.TryGetValue(first, out var form))
                {
                    (form as ITabActivationAware)?.OnActivatedInTab();
                    _lastActiveTab = first;
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
        }

        //private void EnsureFirstTabLoaded()
        //{
        //    try
        //    {
        //        if (mainTabControl == null || mainTabControl.TabPages.Count == 0) return;
        //        if (mainTabControl.SelectedIndex < 0) mainTabControl.SelectedIndex = 0;

        //        var first = mainTabControl.TabPages[0];
        //        var info = first.Tag as FormInfo;
        //        if (info != null && !_tabFormInstances.ContainsKey(first))
        //        {
        //            Console.WriteLine("초기 첫 탭 폼 로드 수행(Main)");
        //            LoadFormIntoTab(first, info);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.Write(ex);
        //    }
        //}
    }
}
