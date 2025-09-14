using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace QMC.Common
{
    public partial class FormRecipe : Form
    {
        private TabControl recipeTabControl;
        private Dictionary<TabPage, Form> _tabFormInstances;

        // Theme fields
        private int _tabHeight = 32;
        private Color _tabBorderColor = Color.Black;
        private int _tabBorderWidth = 2;
        private Font _tabFont = new Font("맑은 고딕", 10, FontStyle.Regular);

        // 호스트에서 유효 크기를 한 번이라도 전달받았는지
        private bool _hostSized;

        public FormRecipe()
        {
            InitializeComponent();

            // 🔧 배경색을 흰색으로 설정
            this.BackColor = Color.White;

            _tabFormInstances = new Dictionary<TabPage, Form>();
            InitializerecipeUI();

            // 🔧 Visible 상태 변경 이벤트: 자식 크기만 동기화
            this.VisibleChanged += Formrecipe_VisibleChanged;
        }

        /// <summary>
        /// 🔧 FormRecipe가 보여질 때 탭 자식 크기만 갱신
        /// </summary>
        private void Formrecipe_VisibleChanged(object sender, EventArgs e)
        {
            if (!this.Visible) return;
            // 호스트에서 최종 사이즈를 전달받기 전이면 무시
            if (!_hostSized) return;
            BeginInvoke(new Action(() => UpdateActiveChildSize()));
        }

        private void InitializerecipeUI()
        {
            Console.WriteLine("🚀 Formrecipe.InitializerecipeUI() 시작");

            // 🔧 Formrecipe 배경색을 확실히 흰색으로 설정
            this.BackColor = Color.White;

            // TabControl 생성 및 테마 적용
            recipeTabControl = new TabControl();
            // 🔧 Dock=Fill로 즉시 부모를 가득 채움 → 초기 작은 사이즈 전달 방지
            recipeTabControl.Dock = DockStyle.Fill;
            recipeTabControl.Font = _tabFont;
            recipeTabControl.DrawMode = TabDrawMode.OwnerDrawFixed;
            recipeTabControl.ItemSize = new Size(120, _tabHeight);
            recipeTabControl.SizeMode = TabSizeMode.Fixed;
            recipeTabControl.DrawItem += recipeTabControl_DrawItem;
            recipeTabControl.SelectedIndexChanged += recipeTabControl_SelectedIndexChanged;

            // 🔧 TabControl 배경색도 흰색으로 설정
            recipeTabControl.BackColor = Color.White;

            Console.WriteLine($"   TabControl 생성 완료: Size={recipeTabControl.Size}, Visible={recipeTabControl.Visible}");

            this.Controls.Add(recipeTabControl);

            Console.WriteLine($"   TabControl을 Formrecipe에 추가 완료");
            Console.WriteLine($"   Formrecipe.Controls.Count: {this.Controls.Count}");

            // FormManager에서 등록된 recipe 폼들을 자동으로 탭으로 추가
            LoadFormsFromManager();

            // 강제로 TabControl을 보이게 설정
            recipeTabControl.Visible = true;
            recipeTabControl.BringToFront();

            // 🔧 첫 탭 즉시 로드 (크기 전달은 이후 일괄 처리)
            EnsureFirstTabLoaded();

            Console.WriteLine($"✅ InitializerecipeUI 완료");
            Console.WriteLine($"   최종 TabControl 상태: Visible={recipeTabControl.Visible}, TabCount={recipeTabControl.TabPages.Count}");
        }

        private void EnsureFirstTabLoaded()
        {
            try
            {
                if (recipeTabControl == null || recipeTabControl.TabPages.Count == 0) return;
                if (recipeTabControl.SelectedIndex < 0) recipeTabControl.SelectedIndex = 0;

                var first = recipeTabControl.TabPages[0];
                var info = first.Tag as FormInfo;
                if (info != null && !_tabFormInstances.ContainsKey(first))
                {
                    Console.WriteLine("🔹 초기 첫 탭 폼 로드 수행(Recipe)");
                    LoadFormIntoTab(first, info);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EnsureFirstTabLoaded(Recipe) 실패: {ex.Message}");
            }
        }

        private void LoadFormsFromManager()
        {
            try
            {
                Console.WriteLine("🔍 Formrecipe.LoadFormsFromManager() 시작");

                var recipeForms = FormManager.Instance.GetRegisteredForms(MenuButtonType.Recipe);
                Console.WriteLine($"   등록된 recipe 폼 개수: {recipeForms.Count}");

                foreach (var formInfo in recipeForms)
                {
                    Console.WriteLine($"   recipe 폼 발견: {formInfo.DisplayName} ({formInfo.FormType.Name})");
                    CreateTabFromFormInfo(formInfo);
                }

                if (recipeForms.Count == 0)
                {
                    Console.WriteLine("⚠️ 등록된 recipe 폼이 없어서 기본 샘플 탭 생성");
                    CreateSampleTabs();
                }

                Console.WriteLine($"✅ 최종 탭 개수: {recipeTabControl.TabPages.Count}");
                Console.WriteLine($"   recipeTabControl.Visible: {recipeTabControl.Visible}");
                Console.WriteLine($"   recipeTabControl.Size: {recipeTabControl.Size}");
                Console.WriteLine($"   recipeTabControl.Dock: {recipeTabControl.Dock}");
                Console.WriteLine($"   Formrecipe.Visible: {this.Visible}");
                Console.WriteLine($"   Formrecipe.Size: {this.Size}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ recipe 폼 로드 중 오류: {ex.Message}");
                MessageBox.Show($"recipe 폼 로드 중 오류 발생: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                CreateSampleTabs();
            }
        }

        private void CreateTabFromFormInfo(FormInfo formInfo)
        {
            Console.WriteLine($"🔧 탭 생성: {formInfo.DisplayName}");
            TabPage tabPage = new TabPage(formInfo.DisplayName);
            tabPage.Tag = formInfo;
            tabPage.BackColor = Color.White;
            recipeTabControl.TabPages.Add(tabPage);
            Console.WriteLine($"   탭 추가 완료. 현재 탭 수: {recipeTabControl.TabPages.Count}");
        }

        private void recipeTabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            TabPage selectedTab = recipeTabControl.SelectedTab;
            if (selectedTab?.Tag is FormInfo formInfo)
            {
                LoadFormIntoTab(selectedTab, formInfo);
                // 호스트에서 유효 사이즈를 받기 전에는 자식 크기 전달 보류
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
                    var existingForm = _tabFormInstances[tabPage];
                    existingForm.Show();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"폼 로드 중 오류 발생: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                if (recipeTabControl == null) return;
                var selectedTab = recipeTabControl.SelectedTab;
                if (selectedTab == null) return;
                if (!_tabFormInstances.ContainsKey(selectedTab)) return;
                var activeForm = _tabFormInstances[selectedTab];
                if (activeForm == null) return;

                int availableWidth = recipeTabControl.ClientSize.Width;
                int availableHeight = recipeTabControl.ClientSize.Height;
                // 너무 작은 초기값(예: 600x400) 필터링
                if (availableWidth < 800 || availableHeight < 450)
                {
                    Console.WriteLine($"   ?? 크기 전달 보류(Working): {availableWidth}x{availableHeight}");
                    return;
                }
                var setPanelSizeMethod = activeForm.GetType().GetMethod("SetPanelSize", new Type[] { typeof(int), typeof(int) });
                if (setPanelSizeMethod != null)
                {
                    Console.WriteLine($"   활성 폼 {activeForm.GetType().Name}에 정확한 크기 전달(Recipe): {availableWidth}x{availableHeight}");
                    setPanelSizeMethod.Invoke(activeForm, new object[] { availableWidth, availableHeight });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"UpdateActiveChildSize(Recipe) 실패: {ex.Message}");
            }
        }

        public void RefreshrecipeTabs()
        {
            foreach (var formInstance in _tabFormInstances.Values)
            {
                formInstance?.Dispose();
            }
            _tabFormInstances.Clear();
            recipeTabControl.TabPages.Clear();
            LoadFormsFromManager();
            EnsureFirstTabLoaded();
            UpdateActiveChildSize();
        }

        private void recipeTabControl_DrawItem(object sender, DrawItemEventArgs e)
        {
            TabPage page = recipeTabControl.TabPages[e.Index];
            Rectangle tabRect = recipeTabControl.GetTabRect(e.Index);
            Color backColor = (e.Index == recipeTabControl.SelectedIndex) ? Color.White : Color.Gainsboro;
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
            TabPage systemTab = new TabPage("Sample recipe");
            systemTab.BackColor = Color.White;
            Label systemLabel = new Label();
            systemLabel.Text = "No recipe Forms Registered\n\nUse FormManager.Instance.RegisterForm() to add recipe forms.";
            systemLabel.Font = new Font("맑은 고딕", 12, FontStyle.Regular);
            systemLabel.TextAlign = ContentAlignment.MiddleCenter;
            systemLabel.Dock = DockStyle.Fill;
            systemLabel.BackColor = Color.White;
            systemTab.Controls.Add(systemLabel);
            recipeTabControl.TabPages.Add(systemTab);
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
            Console.WriteLine($"🔧 Formrecipe.SetPanelSize() 호출: width={width}, height={height}");
            this.Size = new Size(width, height);
            this.ClientSize = new Size(width, height);
            _hostSized = true; // 호스트에서 유효 사이즈 전달 받음
            UpdateActiveChildSize();
            this.Invalidate();
            this.Update();
            Console.WriteLine($"✅ Formrecipe.SetPanelSize() 완료: 최종 크기={this.Size}");
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
            Console.WriteLine($"🖌️ Formrecipe 테두리 그리기: Color={FormBorderColor}, Width={FormBorderWidth}, Size={this.ClientSize}");
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
            Console.WriteLine($"🎨 Formrecipe 테두리 스타일 변경: Color={color}, Width={width}");
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
    }
}
