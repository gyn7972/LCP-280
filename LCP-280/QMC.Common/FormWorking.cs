using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Diagnostics;

namespace QMC.Common
{
    public partial class FormWorking : Form
    {
        private TabControl workingTabControl;
        private Dictionary<TabPage, Form> _tabFormInstances;

        // Theme fields
        private int _tabHeight = 28;
        private Color _tabBorderColor = Color.Black;
        private int _tabBorderWidth = 2;
        private Font _tabFont = new Font("맑은 고딕", 9, FontStyle.Regular);

        // 호스트에서 유효 크기를 한 번이라도 전달받았는지
        private bool _hostSized;

        public FormWorking()
        {
            InitializeComponent();

            // 🔧 배경색을 흰색으로 설정
            this.BackColor = Color.White;

            _tabFormInstances = new Dictionary<TabPage, Form>();
            InitializeworkingUI();

            // 🔧 Visible 상태 변경 이벤트 추가(자식 크기만 동기화)
            this.VisibleChanged += Formworking_VisibleChanged;
        }

        /// <summary>
        /// 🔧 FormWorking이 보여질 때 탭 자식 크기만 갱신
        /// </summary>
        private void Formworking_VisibleChanged(object sender, EventArgs e)
        {
            if (!this.Visible) return;
            // 호스트에서 최종 사이즈를 전달받기 전이면 무시
            if (!_hostSized) return;
            BeginInvoke(new Action(() => UpdateActiveChildSize()));
        }

        private void InitializeworkingUI()
        {
            Console.WriteLine("🚀 Formworking.InitializeworkingUI() 시작");

            // 🔧 Formworking 배경색을 확실히 흰색으로 설정
            this.BackColor = Color.White;

            // TabControl 생성 및 테마 적용
            workingTabControl = new TabControl();
            // 🔧 Dock=Fill로 즉시 부모를 가득 채움 → 초기 작은 사이즈 전달 방지
            workingTabControl.Dock = DockStyle.Fill;
            workingTabControl.Font = _tabFont;
            workingTabControl.DrawMode = TabDrawMode.OwnerDrawFixed;
            workingTabControl.ItemSize = new Size(120, _tabHeight);
            workingTabControl.SizeMode = TabSizeMode.Fixed;
            workingTabControl.DrawItem += workingTabControl_DrawItem;
            workingTabControl.SelectedIndexChanged += workingTabControl_SelectedIndexChanged;

            // 🔧 TabControl 배경색도 흰색으로 설정
            workingTabControl.BackColor = Color.White;

            Console.WriteLine($"   TabControl 생성 완료: Size={workingTabControl.Size}, Visible={workingTabControl.Visible}");

            this.Controls.Add(workingTabControl);

            Console.WriteLine($"   TabControl을 Formworking에 추가 완료");
            Console.WriteLine($"   Formworking.Controls.Count: {this.Controls.Count}");

            // FormManager에서 등록된 working 폼들을 자동으로 탭으로 추가
            LoadFormsFromManager();

            // 강제로 TabControl을 보이게 설정
            workingTabControl.Visible = true;
            workingTabControl.BringToFront();

            // === 모든 Working 폼 미리 생성 & Preload ===
            PreloadAllWorkingForms();

            // 🔧 첫 탭 즉시 로드 (크기 전달은 이후 일괄 처리)
            EnsureFirstTabLoaded();

            Console.WriteLine($"✅ InitializeworkingUI 완료");
            Console.WriteLine($"   최종 TabControl 상태: Visible={workingTabControl.Visible}, TabCount={workingTabControl.TabPages.Count}");
        }

        /// <summary>
        /// Working 탭의 모든 Form을 시작 시 한 번에 생성하여 내부 UI(ChipUnloading_Working.PreloadUI 등)를 준비.
        /// - 이미 생성된 탭만 순회
        /// - PreloadUI() 메서드가 있으면 호출 (예: ChipUnloading_Working)
        /// - Show()는 선택된 탭만 수행 (다른 탭은 Handle/컨트롤만 준비)
        /// </summary>
        private void PreloadAllWorkingForms()
        {
            try
            {
                Console.WriteLine("🟢 PreloadAllWorkingForms 시작");
                foreach (TabPage tab in workingTabControl.TabPages)
                {
                    if (!(tab.Tag is FormInfo info)) continue;
                    if (_tabFormInstances.ContainsKey(tab))
                    {
                        Console.WriteLine($"   ▶ 이미 로드됨: {info.DisplayName}");
                        continue;
                    }
                    try
                    {
                        var form = FormManager.Instance.CreateFormInstance(info);
                        if (form == null || form.IsDisposed)
                        {
                            Console.WriteLine($"   ⚠️ 폼 인스턴스 실패: {info.DisplayName}");
                            continue;
                        }
                        form.TopLevel = false;
                        form.FormBorderStyle = FormBorderStyle.None;
                        form.Dock = DockStyle.Fill;

                        // ChipUnloading_Working 등 PreloadUI 지원 시 호출
                        var preload = form.GetType().GetMethod("PreloadUI", Type.EmptyTypes);
                        if (preload != null)
                        {
                            try { preload.Invoke(form, null); Console.WriteLine($"   🔧 PreloadUI 호출: {info.DisplayName}"); } catch (Exception px) { Console.WriteLine($"   ❌ PreloadUI 실패: {info.DisplayName} / {px.Message}"); }
                        }
                        else
                        {
                            // Load 이벤트의 초기화 로직이 Preload로 대체되지 않은 폼은 강제 Handle만 생성
                            var _ = form.Handle;
                        }

                        // 탭에 삽입 (Clear는 이전 샘플 레이블 제거용)
                        tab.Controls.Clear();
                        tab.Controls.Add(form);

                        // 선택된 탭만 Show (나머지는 초기화만 수행)
                        if (tab == workingTabControl.SelectedTab)
                        {
                            try { form.Show(); } catch (Exception sx) { Console.WriteLine($"   ❌ Show 실패: {info.DisplayName} / {sx.Message}"); }
                        }

                        _tabFormInstances[tab] = form;
                        Console.WriteLine($"   ✅ Preloaded: {info.DisplayName}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"   ❌ Preload 실패: {info.DisplayName} / {ex.Message}");
                        // 오류 라벨 표시
                        try
                        {
                            tab.Controls.Clear();
                            tab.Controls.Add(new Label
                            {
                                Text = $"폼 로드 실패: {info.DisplayName}\r\n{ex.Message}",
                                Dock = DockStyle.Fill,
                                TextAlign = ContentAlignment.MiddleCenter,
                                ForeColor = Color.Red,
                                Font = new Font("맑은 고딕", 10, FontStyle.Bold)
                            });
                        }
                        catch { }
                    }
                }
                Console.WriteLine("🟢 PreloadAllWorkingForms 완료");
            }
            catch (Exception outer)
            {
                Console.WriteLine($"❌ PreloadAllWorkingForms 전체 실패: {outer.Message}");
            }
        }

        private void EnsureFirstTabLoaded()
        {
            try
            {
                if (workingTabControl == null || workingTabControl.TabPages.Count == 0) return;
                if (workingTabControl.SelectedIndex < 0) workingTabControl.SelectedIndex = 0;

                var first = workingTabControl.TabPages[0];
                var info = first.Tag as FormInfo;
                if (info != null && !_tabFormInstances.ContainsKey(first))
                {
                    Console.WriteLine("🔹 초기 첫 탭 폼 로드 수행(Working)");
                    LoadFormIntoTab(first, info);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EnsureFirstTabLoaded(Working) 실패: {ex.Message}");
            }
        }

        private void LoadFormsFromManager()
        {
            try
            {
                Console.WriteLine("🔍 Formworking.LoadFormsFromManager() 시작");

                var workingForms = FormManager.Instance.GetRegisteredForms(MenuButtonType.Working);
                Console.WriteLine($"   등록된 working 폼 개수: {workingForms.Count}");

                foreach (var formInfo in workingForms)
                {
                    Console.WriteLine($"   working 폼 발견: {formInfo.DisplayName} ({formInfo.FormType.Name})");
                    CreateTabFromFormInfo(formInfo);
                }

                if (workingForms.Count == 0)
                {
                    Console.WriteLine("⚠️ 등록된 working 폼이 없어서 기본 샘플 탭 생성");
                    CreateSampleTabs();
                }

                Console.WriteLine($"✅ 최종 탭 개수: {workingTabControl.TabPages.Count}");
                Console.WriteLine($"   workingTabControl.Visible: {workingTabControl.Visible}");
                Console.WriteLine($"   workingTabControl.Size: {workingTabControl.Size}");
                Console.WriteLine($"   workingTabControl.Dock: {workingTabControl.Dock}");
                Console.WriteLine($"   Formworking.Visible: {this.Visible}");
                Console.WriteLine($"   Formworking.Size: {this.Size}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ working 폼 로드 중 오류: {ex.Message}");
                MessageBox.Show($"working 폼 로드 중 오류 발생: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                CreateSampleTabs();
            }
        }

        private void CreateTabFromFormInfo(FormInfo formInfo)
        {
            Console.WriteLine($"🔧 탭 생성: {formInfo.DisplayName}");
            TabPage tabPage = new TabPage(formInfo.DisplayName);
            tabPage.Tag = formInfo;
            tabPage.BackColor = Color.White;
            workingTabControl.TabPages.Add(tabPage);
            Console.WriteLine($"   탭 추가 완료. 현재 탭 수: {workingTabControl.TabPages.Count}");
        }

        private void workingTabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            TabPage selectedTab = workingTabControl.SelectedTab;
            if (selectedTab?.Tag is FormInfo formInfo)
            {
                // Preload 단계에서 이미 생성된 경우 Show만 보장
                if (_tabFormInstances.TryGetValue(selectedTab, out var inst))
                {
                    if (!inst.IsDisposed && !inst.Visible)
                    {
                        try { inst.Show(); } catch { }
                    }
                }
                else
                {
                    // 혹시 Preload에서 실패한 경우 재시도
                    LoadFormIntoTab(selectedTab, formInfo);
                }
                if (_hostSized)
                    UpdateActiveChildSize();
            }
        }

        private void LoadFormIntoTab(TabPage tabPage, FormInfo formInfo)
        {
            try
            {
                // 새 방어 로직: 기본 유효성 검사 및 UI 스레드 보장
                if (tabPage == null || formInfo == null) return;
                if (tabPage.IsDisposed || tabPage.Disposing) return;
                if (this.IsDisposed || this.Disposing) return;
                if (tabPage.InvokeRequired)
                {
                    tabPage.BeginInvoke(new Action(() => LoadFormIntoTab(tabPage, formInfo)));
                    return;
                }

                if (!_tabFormInstances.ContainsKey(tabPage))
                {
                    Form formInstance = null;
                    try
                    {
                        formInstance = FormManager.Instance.CreateFormInstance(formInfo);
                    }
                    catch (Exception ctorEx)
                    {
                        ShowLoadError(tabPage, formInfo, ctorEx);
                        return;
                    }

                    if (formInstance == null) { ShowLoadError(tabPage, formInfo, new Exception("Form instance is null")); return; }
                    if (formInstance.IsDisposed) { ShowLoadError(tabPage, formInfo, new Exception("Form already disposed")); return; }

                    formInstance.TopLevel = false;
                    formInstance.FormBorderStyle = FormBorderStyle.None;
                    formInstance.Dock = DockStyle.Fill;

                    SafeClearControls(tabPage);

                    tabPage.Controls.Add(formInstance);

                    try
                    {
                        formInstance.Show();
                    }
                    catch (Exception showEx)
                    {
                        ShowLoadError(tabPage, formInfo, showEx);
                        formInstance.Dispose();
                        return;
                    }

                    _tabFormInstances[tabPage] = formInstance;
                }
                else
                {
                    var existingForm = _tabFormInstances[tabPage];
                    if (existingForm == null || existingForm.IsDisposed)
                    {
                        _tabFormInstances.Remove(tabPage);
                        LoadFormIntoTab(tabPage, formInfo); // 재시도
                        return;
                    }
                    try
                    {
                        if (!existingForm.Visible) existingForm.Show();
                    }
                    catch (Exception exShow)
                    {
                        ShowLoadError(tabPage, formInfo, exShow);
                    }
                }
            }
            catch (Exception ex)
            {
                ShowLoadError(tabPage, formInfo, ex);
            }
        }

        // 탭 페이지 컨트롤 안전 제거(핸들 생성 문제 최소화)
        private void SafeClearControls(TabPage tabPage)
        {
            if (tabPage == null) return;
            if (tabPage.Controls.Count == 0) return;

            try
            {
                tabPage.SuspendLayout();
                // 직접 Dispose 후 Clear (예외 발생 위치 분리)
                var toDispose = tabPage.Controls.Cast<Control>().ToList();
                foreach (var c in toDispose)
                {
                    try
                    {
                        c.Visible = false; // 불필요한 Handle 재생성 억제
                        c.Dispose();
                    }
                    catch { /* 개별 컨트롤 dispose 실패 무시 */ }
                }
                tabPage.Controls.Clear();
            }
            catch (System.ComponentModel.Win32Exception w32ex)
            {
                // 자원 고갈 가능 → GC 유도 & 로그
                Debug.WriteLine($"[FormWorking] Win32Exception while clearing controls: {w32ex.Message}");
                Debug.WriteLine($"  HandleCount={Process.GetCurrentProcess().HandleCount}");
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[FormWorking] Unexpected exception clearing controls: {ex.Message}");
            }
            finally
            {
                try { tabPage.ResumeLayout(true); } catch { }
            }
        }

        private void ShowLoadError(TabPage tabPage, FormInfo formInfo, Exception ex)
        {
            try
            {
                string name = formInfo != null ? formInfo.DisplayName : "(Unknown)";
                Debug.WriteLine($"[FormWorking] Form load error for '{name}': {ex.Message}");
                // 사용자에게 중복 팝업 방지: 이미 에러 라벨이면 패스
                if (tabPage != null && (tabPage.Controls.Count == 0 || !(tabPage.Controls[0] is Label lbl && lbl.Tag as string == "__ERR__")))
                {
                    SafeClearControls(tabPage);
                    Label errorLabel = new Label
                    {
                        Text = $"폼 로드 실패: {name}\r\n{ex.Message}",
                        Dock = DockStyle.Fill,
                        TextAlign = ContentAlignment.MiddleCenter,
                        Font = new Font("맑은 고딕", 10, FontStyle.Bold),
                        ForeColor = Color.Red,
                        Tag = "__ERR__"
                    };
                    try { tabPage.Controls.Add(errorLabel); } catch { }
                }
            }
            catch { }
        }

        private void UpdateActiveChildSize()
        {
            try
            {
                if (workingTabControl == null) return;
                var selectedTab = workingTabControl.SelectedTab;
                if (selectedTab == null) return;
                if (!_tabFormInstances.ContainsKey(selectedTab)) return;
                var activeForm = _tabFormInstances[selectedTab];
                if (activeForm == null) return;

                int availableWidth = workingTabControl.ClientSize.Width;
                int availableHeight = workingTabControl.ClientSize.Height;
                // 너무 작은 초기값(예: 600x400) 필터링
                if (availableWidth < 800 || availableHeight < 450)
                {
                    Console.WriteLine($"   ⏭️ 크기 전달 보류(Working): {availableWidth}x{availableHeight}");
                    return;
                }
                var setPanelSizeMethod = activeForm.GetType().GetMethod("SetPanelSize", new Type[] { typeof(int), typeof(int) });
                if (setPanelSizeMethod != null)
                {
                    Console.WriteLine($"   활성 폼 {activeForm.GetType().Name}에 정확한 크기 전달(Working): {availableWidth}x{availableHeight}");
                    setPanelSizeMethod.Invoke(activeForm, new object[] { availableWidth, availableHeight });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"UpdateActiveChildSize(Working) 실패: {ex.Message}");
            }
        }

        public void RefreshworkingTabs()
        {
            foreach (var formInstance in _tabFormInstances.Values)
            {
                formInstance?.Dispose();
            }
            _tabFormInstances.Clear();
            workingTabControl.TabPages.Clear();
            LoadFormsFromManager();
            EnsureFirstTabLoaded();
            UpdateActiveChildSize();
        }

        private void workingTabControl_DrawItem(object sender, DrawItemEventArgs e)
        {
            TabPage page = workingTabControl.TabPages[e.Index];
            Rectangle tabRect = workingTabControl.GetTabRect(e.Index);
            Color backColor = (e.Index == workingTabControl.SelectedIndex) ? Color.White : Color.Gainsboro;
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
            workingTabControl.TabPages.Add(systemTab);
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
            Console.WriteLine($"🔧 Formworking.SetPanelSize() 호출: width={width}, height={height}");
            this.Size = new Size(width, height);
            this.ClientSize = new Size(width, height);
            _hostSized = true; // 호스트에서 유효 사이즈 전달 받음
            UpdateActiveChildSize();
            this.Invalidate();
            this.Update();
            Console.WriteLine($"✅ Formworking.SetPanelSize() 완료: 최종 크기={this.Size}");
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
            Console.WriteLine($"🖌️ Formworking 테두리 그리기: Color={FormBorderColor}, Width={FormBorderWidth}, Size={this.ClientSize}");
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
            Console.WriteLine($"🎨 Formworking 테두리 스타일 변경: Color={color}, Width={width}");
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
