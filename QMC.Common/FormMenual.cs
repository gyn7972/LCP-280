using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace QMC.Common
{
    public partial class FormMenual : Form
    {
        public interface ITabActivationAware
        {
            void OnActivatedInTab();
            void OnDeactivatedInTab();
        }

        private TabControl ManualTabControl;
        private Dictionary<TabPage, Form> _tabFormInstances;

        // [추가] 현재 활성화된 자식 폼을 추적하기 위한 변수
        private Form _currentActiveForm = null;

        // Theme fields
        private int _tabHeight = 32;
        private Color _tabBorderColor = Color.Black;
        private int _tabBorderWidth = 2;
        private Font _tabFont = new Font("맑은 고딕", 10, FontStyle.Bold);

        // 호스트에서 유효 크기를 한 번이라도 전달받았는지
        private bool _hostSized;

        public FormMenual()
        {
            InitializeComponent();

            // 배경색을 흰색으로 설정
            this.BackColor = Color.White;

            _tabFormInstances = new Dictionary<TabPage, Form>();
            InitializeManualUI();

            // Visible 상태 변경 이벤트 추가(자식 크기만 동기화)
            this.VisibleChanged += FormManual_VisibleChanged;
        }

        /// <summary>
        /// FormManual이 보여질 때 탭 자식 크기만 갱신
        /// </summary>
        private void FormManual_VisibleChanged(object sender, EventArgs e)
        {
            if (!this.Visible) return;
            // 호스트에서 최종 사이즈를 전달받기 전이면 무시
            if (!_hostSized) return;
            BeginInvoke(new Action(() => UpdateActiveChildSize()));
        }

        private void InitializeManualUI()
        {
            Console.WriteLine("FormManual.InitializeManualUI() 시작");

            // FormManual 배경색을 확실히 흰색으로 설정
            this.BackColor = Color.White;

            // TabControl 생성 및 테마 적용
            ManualTabControl = new TabControl();
            // Dock=Fill로 즉시 부모를 가득 채움 → 초기 작은 사이즈 전달 방지
            ManualTabControl.Dock = DockStyle.Fill;
            ManualTabControl.Font = _tabFont;
            ManualTabControl.DrawMode = TabDrawMode.OwnerDrawFixed;
            ManualTabControl.ItemSize = new Size(120, _tabHeight);
            ManualTabControl.SizeMode = TabSizeMode.Fixed;
            ManualTabControl.DrawItem += workingTabControl_DrawItem;
            ManualTabControl.SelectedIndexChanged += workingTabControl_SelectedIndexChanged;

            // TabControl 배경색도 흰색으로 설정
            ManualTabControl.BackColor = Color.White;

            Console.WriteLine($"TabControl 생성 완료: Size={ManualTabControl.Size}, Visible={ManualTabControl.Visible}");

            this.Controls.Add(ManualTabControl);

            Console.WriteLine($"TabControl을 FormManual에 추가 완료");
            Console.WriteLine($"FormManual.Controls.Count: {this.Controls.Count}");

            // FormManager에서 등록된 working 폼들을 자동으로 탭으로 추가
            LoadFormsFromManager();

            // 강제로 TabControl을 보이게 설정
            ManualTabControl.Visible = true;
            ManualTabControl.BringToFront();

            // 첫 탭 즉시 로드 (크기 전달은 이후 일괄 처리)
            EnsureFirstTabLoaded();

            Console.WriteLine($"InitializeworkingUI 완료");
            Console.WriteLine($"   최종 TabControl 상태: Visible={ManualTabControl.Visible}, TabCount={ManualTabControl.TabPages.Count}");
        }

        private void EnsureFirstTabLoaded()
        {
            try
            {
                if (ManualTabControl == null || ManualTabControl.TabPages.Count == 0) return;
                if (ManualTabControl.SelectedIndex < 0) ManualTabControl.SelectedIndex = 0;

                var first = ManualTabControl.TabPages[0];
                var info = first.Tag as FormInfo;
                if (info != null && !_tabFormInstances.ContainsKey(first))
                {
                    Console.WriteLine("🔹 초기 첫 탭 폼 로드 수행(Working)");
                    LoadFormIntoTab(first, info);

                    // [추가] 첫 탭 로드 시 즉시 Activation 호출
                    if (_tabFormInstances.TryGetValue(first, out Form firstForm))
                    {
                        _currentActiveForm = firstForm;
                        if (firstForm is ITabActivationAware aware)
                        {
                            try { aware.OnActivatedInTab(); } catch { }
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
        }

        private void LoadFormsFromManager()
        {
            try
            {
                Console.WriteLine("FormMenual.LoadFormsFromManager() 시작");

                var MenualForms = FormManager.Instance.GetRegisteredForms(MenuButtonType.Menual);
                var desiredOrder = new[]
                {
                    "Wafer",
                    "LoadArm",
                    "Index",
                    "UnloadArm",
                    "Bin"
                };
                //var desiredOrder = new[]
                //{
                //    "InputWafer",
                //    "ChipLoading",
                //    "Process",
                //    "ChipUnloading",
                //    "OutputWafer"
                //};

                // 정규화 함수 (앞뒤 공백 제거 + null 보호)
                string Normalize(string s) => (s ?? string.Empty).Trim();

                // 원본 목록 진단 로그
                Console.WriteLine("등록된 폼(DisplayName 원본): " +
                    string.Join(", ", MenualForms.Select(f => $"[{f.DisplayName}]")));

                // DisplayName 정규화된 Lookup (동일 이름 여러 개일 수도 있으므로)
                var lookup = MenualForms
                    .GroupBy(f => Normalize(f.DisplayName), StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.OrdinalIgnoreCase);

                var ordered = new List<FormInfo>();
                var used = new HashSet<FormInfo>();

                // 1) 원하는 순서대로 추가
                foreach (var rawName in desiredOrder)
                {
                    var key = Normalize(rawName);
                    if (lookup.TryGetValue(key, out var list) && list.Count > 0)
                    {
                        // 동일 DisplayName 여러 개면 모두 순서대로(필요 시 첫 것만 사용하려면 list[0] 만 추가)
                        foreach (var fi in list)
                        {
                            if (used.Add(fi))
                            {
                                ordered.Add(fi);
                                Console.WriteLine($"순서 반영: {fi.DisplayName}");
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine($"미존재: {rawName}");
                    }
                }

                // 2) desiredOrder 에 없었던 나머지 (원래 등록 순서 보존)
                foreach (var fi in MenualForms)
                {
                    if (!used.Contains(fi))
                    {
                        ordered.Add(fi);
                        used.Add(fi);
                        Console.WriteLine($"잔여 추가: {fi.DisplayName}");
                    }
                }

                // 3) 탭 생성 (기존 탭 초기화가 이미 이 함수 호출 전에 되었는지 확인)
                foreach (var fi in ordered)
                    CreateTabFromFormInfo(fi);

                if (ordered.Count == 0)
                {
                    Console.WriteLine("등록된 working 폼이 없어서 기본 샘플 탭 생성");
                    CreateSampleTabs();
                }

                // 최종 진단
                Console.WriteLine("최종 탭 순서: " +
                    string.Join(" > ", ordered.Select(f => f.DisplayName)));

                // desiredOrder 대비 실제 매칭 안된 이름 추출
                var missing = desiredOrder
                    .Where(d => !ordered.Any(o =>
                        string.Equals(Normalize(o.DisplayName), Normalize(d), StringComparison.OrdinalIgnoreCase)))
                    .ToList();

                if (missing.Count > 0)
                {
                    Console.WriteLine("desiredOrder 미매칭 목록: " + string.Join(", ", missing));
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex);

                var mb = new MessageBoxOk();
                mb.ShowDialog("Error!", $"working 폼 로드 중 오류 발생: {ex.Message}");

                CreateSampleTabs();
            }
        }

        private void CreateTabFromFormInfo(FormInfo formInfo)
        {
            Console.WriteLine($"탭 생성: {formInfo.DisplayName}");
            TabPage tabPage = new TabPage(formInfo.DisplayName);
            tabPage.Tag = formInfo;
            tabPage.BackColor = Color.White;
            ManualTabControl.TabPages.Add(tabPage);
            Console.WriteLine($" 탭 추가 완료. 현재 탭 수: {ManualTabControl.TabPages.Count}");
        }

        private void workingTabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            // 1. 기존 활성 폼이 있다면 비활성화(Deactivate) 알림
            if (_currentActiveForm != null)
            {
                if (_currentActiveForm is ITabActivationAware oldAware)
                {
                    try { oldAware.OnDeactivatedInTab(); } catch { }
                }
            }

            TabPage selectedTab = ManualTabControl.SelectedTab;
            if (selectedTab?.Tag is FormInfo formInfo)
            {
                // 폼 로드 (없으면 생성)
                LoadFormIntoTab(selectedTab, formInfo);

                // 로드된 폼 가져오기
                if (_tabFormInstances.TryGetValue(selectedTab, out Form newForm))
                {
                    // 2. 현재 활성 폼 갱신
                    _currentActiveForm = newForm;

                    // 3. 새 폼에 활성화(Activate) 알림
                    if (newForm is ITabActivationAware newAware)
                    {
                        try { newAware.OnActivatedInTab(); } catch { }
                    }

                    // 호스트에서 유효 사이즈를 받기 전에는 자식 크기 전달 보류
                    if (_hostSized)
                        UpdateActiveChildSize();
                }
            }
            //TabPage selectedTab = ManualTabControl.SelectedTab;
            //if (selectedTab?.Tag is FormInfo formInfo)
            //{
            //    LoadFormIntoTab(selectedTab, formInfo);
            //    // 호스트에서 유효 사이즈를 받기 전에는 자식 크기 전달 보류
            //    if (_hostSized)
            //        UpdateActiveChildSize();
            //}
        }

        // [추가 권장] 메인 폼 전체가 숨겨지거나 닫힐 때도 Deactivate를 호출해주면 좋습니다.
        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);

            // 이 폼(FormMenual) 자체가 안 보이게 되면 현재 활성 탭도 멈춤
            if (!this.Visible && _currentActiveForm is ITabActivationAware aware)
            {
                try { aware.OnDeactivatedInTab(); } catch { }
            }
            // 다시 보이게 되면 현재 활성 탭 재개
            else if (this.Visible && _currentActiveForm is ITabActivationAware resumeAware)
            {
                try { resumeAware.OnActivatedInTab(); } catch { }
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
                if (ManualTabControl == null) return;
                var selectedTab = ManualTabControl.SelectedTab;
                if (selectedTab == null) return;
                if (!_tabFormInstances.ContainsKey(selectedTab)) return;
                var activeForm = _tabFormInstances[selectedTab];
                if (activeForm == null) return;

                int availableWidth = ManualTabControl.ClientSize.Width;
                int availableHeight = ManualTabControl.ClientSize.Height;
                // 너무 작은 초기값(예: 600x400) 필터링
                if (availableWidth < 800 || availableHeight < 450)
                {
                    Console.WriteLine($"크기 전달 보류(Working): {availableWidth}x{availableHeight}");
                    return;
                }
                var setPanelSizeMethod = activeForm.GetType().GetMethod("SetPanelSize", new Type[] { typeof(int), typeof(int) });
                if (setPanelSizeMethod != null)
                {
                    Console.WriteLine($"활성 폼 {activeForm.GetType().Name}에 정확한 크기 전달(Working): {availableWidth}x{availableHeight}");
                    setPanelSizeMethod.Invoke(activeForm, new object[] { availableWidth, availableHeight });
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
        }

        public void RefreshworkingTabs()
        {
            foreach (var formInstance in _tabFormInstances.Values)
            {
                formInstance?.Dispose();
            }
            _tabFormInstances.Clear();
            ManualTabControl.TabPages.Clear();
            LoadFormsFromManager();
            EnsureFirstTabLoaded();
            UpdateActiveChildSize();
        }

        private void workingTabControl_DrawItem(object sender, DrawItemEventArgs e)
        {
            TabPage page = ManualTabControl.TabPages[e.Index];
            Rectangle tabRect = ManualTabControl.GetTabRect(e.Index);
            Color backColor = (e.Index == ManualTabControl.SelectedIndex) ? Color.White : Color.Gainsboro;
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
            TabPage systemTab = new TabPage("Sample working");
            systemTab.BackColor = Color.White;
            Label systemLabel = new Label();
            systemLabel.Text = "No working Forms Registered\n\nUse FormManager.Instance.RegisterForm() to add working forms.";
            systemLabel.Font = new Font("맑은 고딕", 12, FontStyle.Regular);
            systemLabel.TextAlign = ContentAlignment.MiddleCenter;
            systemLabel.Dock = DockStyle.Fill;
            systemLabel.BackColor = Color.White;
            systemTab.Controls.Add(systemLabel);
            ManualTabControl.TabPages.Add(systemTab);
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
            Console.WriteLine($"FormManual.SetPanelSize() 호출: width={width}, height={height}");
            this.Size = new Size(width, height);
            this.ClientSize = new Size(width, height);
            _hostSized = true; // 호스트에서 유효 사이즈 전달 받음
            UpdateActiveChildSize();
            this.Invalidate();
            this.Update();
            Console.WriteLine($"FormManual.SetPanelSize() 완료: 최종 크기={this.Size}");
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
            Console.WriteLine($"FormManual 테두리 그리기: Color={FormBorderColor}, Width={FormBorderWidth}, Size={this.ClientSize}");
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
            Console.WriteLine($"FormManual 테두리 스타일 변경: Color={color}, Width={width}");
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
