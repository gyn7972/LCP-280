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
        private int _tabHeight = 28;
        private Color _tabBorderColor = Color.Black;
        private int _tabBorderWidth = 2;
        private Font _tabFont = new Font("맑은 고딕", 9, FontStyle.Regular);

        // 방어: 너무 작은 크기 전달 차단
        private bool _hasAppliedSize;
        private Size _lastAppliedSize;

        // 초기 탭 로딩 시 SelectedIndexChanged 억제용
        private bool _tabsReady;

        public FormConfig()
        {
            InitializeComponent();
            
            // 🔧 배경색을 흰색으로 설정
            this.BackColor = Color.White;
            
            _tabFormInstances = new Dictionary<TabPage, Form>();
            InitializeConfigUI();
            
            // 🔧 Visible 상태 변경 이벤트 추가 (자식 크기만 동기화, 콘텐츠는 클릭 시 로드)
            this.VisibleChanged += FormConfig_VisibleChanged;
        }
        
        /// <summary>
        /// 🔧 FormConfig가 보여질 때마다 탭 자식 크기만 갱신(호스트가 최종 크기를 전달하도록)
        /// 실제 폼 로드는 사용자가 탭을 클릭하거나 선택을 변경할 때 수행한다.
        /// </summary>
        private void FormConfig_VisibleChanged(object sender, EventArgs e)
        {
            if (!this.Visible) return;
            // 호스트(MainForm→FormAdapterControl)가 SetPanelSize를 호출하므로 이곳에서는 자식만 동기화
            BeginInvoke(new Action(() => UpdateActiveChildSize()));
        }
        
        private void InitializeConfigUI()
        {
            Console.WriteLine("🚀 FormConfig.InitializeConfigUI() 시작");
            
            // 🔧 FormConfig 배경색을 확실히 흰색으로 설정
            this.BackColor = Color.White;
            
            // TabControl 생성 및 테마 적용
            configTabControl = new TabControl();
            // 🔧 즉시 부모(FormConfig)를 꽉 채우도록 설정 → 초기 잘못된 200x52 사이즈 전달 방지
            configTabControl.Dock = DockStyle.Fill;
            configTabControl.Font = _tabFont;
            configTabControl.DrawMode = TabDrawMode.OwnerDrawFixed;
            configTabControl.ItemSize = new Size(120, _tabHeight);
            configTabControl.SizeMode = TabSizeMode.Fixed;
            configTabControl.DrawItem += ConfigTabControl_DrawItem;
            configTabControl.SelectedIndexChanged += ConfigTabControl_SelectedIndexChanged;
            // ✅ 사용자가 현재 선택된 탭을 클릭했을 때만 로드되도록 처리 (초기 자동 로드 방지)
            configTabControl.MouseUp += ConfigTabControl_MouseUp;
            
            // 🔧 TabControl 배경색도 흰색으로 설정
            configTabControl.BackColor = Color.White;
            
            Console.WriteLine($"   TabControl 생성 완료: Size={configTabControl.Size}, Visible={configTabControl.Visible}");
            
            this.Controls.Add(configTabControl);
            
            Console.WriteLine($"   TabControl을 FormConfig에 추가 완료");
            Console.WriteLine($"   FormConfig.Controls.Count: {this.Controls.Count}");
            
            // FormManager에서 등록된 Config 폼들을 자동으로 탭으로 추가
            _tabsReady = false; // 초기화 중 이벤트 무시
            LoadFormsFromManager();
            _tabsReady = true;  // 초기화 완료
            
            // 강제로 TabControl을 보이게 설정
            configTabControl.Visible = true;
            configTabControl.BringToFront();
            
            // ❌ 초기 첫 탭 자동 로드 제거: 사용자가 탭을 클릭하거나 변경할 때만 로드
            Console.WriteLine($"✅ InitializeConfigUI 완료");
            Console.WriteLine($"   최종 TabControl 상태: Visible={configTabControl.Visible}, TabCount={configTabControl.TabPages.Count}");
        }

        /// <summary>
        /// FormManager에서 Config 타입으로 등록된 폼들을 탭으로 로드
        /// </summary>
        private void LoadFormsFromManager()
        {
            try
            {
                Console.WriteLine("🔍 FormConfig.LoadFormsFromManager() 시작");
                
                var configForms = FormManager.Instance.GetRegisteredForms(MenuButtonType.Config);
                Console.WriteLine($"   등록된 Config 폼 개수: {configForms.Count}");
                
                foreach (var formInfo in configForms)
                {
                    Console.WriteLine($"   Config 폼 발견: {formInfo.DisplayName} ({formInfo.FormType.Name})");
                    CreateTabFromFormInfo(formInfo);
                }
                
                // 등록된 폼이 없으면 기본 샘플 탭 생성
                if (configForms.Count == 0)
                {
                    Console.WriteLine("⚠️ 등록된 Config 폼이 없어서 기본 샘플 탭 생성");
                    CreateSampleTabs();
                }
                
                Console.WriteLine($"✅ 최종 탭 개수: {configTabControl.TabPages.Count}");
                
                // 탭 컨트롤 상태 확인
                Console.WriteLine($"   configTabControl.Visible: {configTabControl.Visible}");
                Console.WriteLine($"   configTabControl.Size: {configTabControl.Size}");
                Console.WriteLine($"   configTabControl.Dock: {configTabControl.Dock}");
                
                // FormConfig 자체 상태도 확인
                Console.WriteLine($"   FormConfig.Visible: {this.Visible}");
                Console.WriteLine($"   FormConfig.Size: {this.Size}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Config 폼 로드 중 오류: {ex.Message}");
                MessageBox.Show($"Config 폼 로드 중 오류 발생: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                CreateSampleTabs(); // 오류 발생시 기본 탭 생성
            }
        }

        /// <summary>
        /// FormInfo를 기반으로 탭 페이지 생성
        /// </summary>
        /// <param name="formInfo">폼 정보</param>
        private void CreateTabFromFormInfo(FormInfo formInfo)
        {
            Console.WriteLine($"🔧 탭 생성: {formInfo.DisplayName}");
            
            TabPage tabPage = new TabPage(formInfo.DisplayName);
            tabPage.Tag = formInfo; // FormInfo를 Tag에 저장
            
            // 🔧 TabPage 배경색도 흰색으로 설정
            tabPage.BackColor = Color.White;
            
            configTabControl.TabPages.Add(tabPage);
            
            Console.WriteLine($"   탭 추가 완료. 현재 탭 수: {configTabControl.TabPages.Count}");
        }

        /// <summary>
        /// 사용자가 탭 선택을 변경했을 때 해당 폼을 로드하여 표시 (Lazy)
        /// </summary>
        private void ConfigTabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!_tabsReady) return; // 초기 로드 중에는 무시

            TabPage selectedTab = configTabControl.SelectedTab;
            if (selectedTab?.Tag is FormInfo formInfo)
            {
                LoadFormIntoTab(selectedTab, formInfo);
                // 선택 변경 시 즉시 크기 반영
                UpdateActiveChildSize();
            }
        }

        /// <summary>
        /// 사용자가 현재 선택된 탭을 클릭했을 때 아직 로드되지 않았으면 로드
        /// </summary>
        private void ConfigTabControl_MouseUp(object sender, MouseEventArgs e)
        {
            try
            {
                if (e.Button != MouseButtons.Left) return;
                for (int i = 0; i < configTabControl.TabPages.Count; i++)
                {
                    Rectangle rect = configTabControl.GetTabRect(i);
                    if (rect.Contains(e.Location))
                    {
                        var tab = configTabControl.TabPages[i];
                        if (tab != null && tab.Tag is FormInfo info && !_tabFormInstances.ContainsKey(tab))
                        {
                            LoadFormIntoTab(tab, info);
                            UpdateActiveChildSize();
                        }
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ConfigTabControl_MouseUp 실패: {ex.Message}");
            }
        }

        /// <summary>
        /// 탭에 폼을 로드하여 표시 (이 시점에서는 크기 전달을 지연)
        /// </summary>
        /// <param name="tabPage">대상 탭 페이지</param>
        /// <param name="formInfo">로드할 폼 정보</param>
        private void LoadFormIntoTab(TabPage tabPage, FormInfo formInfo)
        {
            try
            {
                // 이미 로드된 폼이 있는지 확인
                if (!_tabFormInstances.ContainsKey(tabPage))
                {
                    // 폼 인스턴스 생성
                    Form formInstance = FormManager.Instance.CreateFormInstance(formInfo);
                    
                    // 폼을 탭에 임베드하기 위한 설정
                    formInstance.TopLevel = false;
                    formInstance.FormBorderStyle = FormBorderStyle.None;
                    formInstance.Dock = DockStyle.Fill;
                    
                    // 탭에 폼 추가 (크기 전달은 나중에)
                    tabPage.Controls.Clear();
                    tabPage.Controls.Add(formInstance);
                    
                    // 폼 표시
                    formInstance.Show();
                    
                    // 인스턴스 저장
                    _tabFormInstances[tabPage] = formInstance;
                }
                else
                {
                    // 이미 로드된 폼이 있으면 다시 표시
                    var existingForm = _tabFormInstances[tabPage];
                    existingForm.Show();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"폼 로드 중 오류 발생: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                
                // 오류시 기본 메시지 표시
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

        /// <summary>
        /// 현재 활성 탭의 폼에 정확한 크기를 전달
        /// </summary>
        private void UpdateActiveChildSize()
        {
            try
            {
                if (configTabControl == null) return;
                var selectedTab = configTabControl.SelectedTab;
                if (selectedTab == null) return;
                if (!_tabFormInstances.ContainsKey(selectedTab)) return; // 아직 로드 안 되었으면 스킵

                var activeForm = _tabFormInstances[selectedTab];
                if (activeForm == null) return;

                int availableWidth = configTabControl.ClientSize.Width;
                int availableHeight = configTabControl.ClientSize.Height - _tabHeight; // 탭 헤더 제외

                var setPanelSizeMethod = activeForm.GetType().GetMethod("SetPanelSize", new Type[] { typeof(int), typeof(int) });
                if (setPanelSizeMethod != null)
                {
                    Console.WriteLine($"   활성 폼 {activeForm.GetType().Name}에 정확한 크기 전달: {availableWidth}x{availableHeight}");
                    setPanelSizeMethod.Invoke(activeForm, new object[] { availableWidth, availableHeight });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"UpdateActiveChildSize 실패: {ex.Message}");
            }
        }

        /// <summary>
        /// FormManager에 새로운 폼이 등록되었을 때 탭을 새로고침
        /// </summary>
        public void RefreshConfigTabs()
        {
            // 기존 탭과 폼 인스턴스 정리
            foreach (var formInstance in _tabFormInstances.Values)
            {
                formInstance?.Dispose();
            }
            _tabFormInstances.Clear();
            configTabControl.TabPages.Clear();
            
            // 새로 로드 (초기에는 콘텐츠 미로딩)
            _tabsReady = false;
            LoadFormsFromManager();
            _tabsReady = true;
            
            // 선택 변경 시 로드되므로 크기만 반영 시도
            UpdateActiveChildSize();
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
            TabPage systemTab = new TabPage("Sample Config");
            
            // 🔧 샘플 탭의 배경색도 흰색으로 설정
            systemTab.BackColor = Color.White;
            
            Label systemLabel = new Label();
            systemLabel.Text = "No Config Forms Registered\n\nUse FormManager.Instance.RegisterForm() to add config forms.";
            systemLabel.Font = new Font("맑은 고딕", 12, FontStyle.Regular);
            systemLabel.TextAlign = ContentAlignment.MiddleCenter;
            systemLabel.Dock = DockStyle.Fill;
            
            // 🔧 라벨의 배경색도 흰색으로 설정
            systemLabel.BackColor = Color.White;
            
            systemTab.Controls.Add(systemLabel);
            configTabControl.TabPages.Add(systemTab);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            // 폼 종료시 리소스 정리
            foreach (var formInstance in _tabFormInstances.Values)
            {
                formInstance?.Dispose();
            }
            _tabFormInstances.Clear();
            
            base.OnFormClosed(e);
        }

        public void SetPanelSize(int width, int height)
        {
            Console.WriteLine($"🔧 FormConfig.SetPanelSize() 호출: width={width}, height={height}");

            // 방어: 의미 없는 작은 값(초기 200x52 등)은 무시 (이미 한 번 이상 적용된 상태라면)
            if (_hasAppliedSize && (width < 400 || height < 200))
            {
                Console.WriteLine($"   ⏭️ 무시: 너무 작은 값 전달({width}x{height})");
                return;
            }

            this.Size = new Size(width, height);
            this.ClientSize = new Size(width, height);
            _hasAppliedSize = true;
            _lastAppliedSize = new Size(width, height);
            
            // Dock=Fill이므로 Size 설정 후 레이아웃 진행 시 TabControl도 자동 확장됨
            if (configTabControl != null)
            {
                // 레이아웃 및 자식 크기 동기화
                this.PerformLayout();
                configTabControl.Invalidate();
                configTabControl.Update();

                // 현재 활성화된 탭에 폼이 이미 로드되어 있다면 탭 높이를 제외한 크기 전달
                UpdateActiveChildSize();
            }
            
            // 폼 전체를 다시 그리기
            this.Invalidate();
            this.Update();
            
            Console.WriteLine($"✅ FormConfig.SetPanelSize() 완료: 최종 크기={this.Size}");
        }
        
        #region Form Border Drawing

        /// <summary>
        /// 폼 테두리 그리기
        /// </summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            
            // 폼 전체 테두리 그리기
            using (Pen borderPen = new Pen(FormBorderColor, FormBorderWidth))
            {
                Rectangle borderRect = new Rectangle(0, 0, this.ClientSize.Width - 1, this.ClientSize.Height - 1);
                e.Graphics.DrawRectangle(borderPen, borderRect);
            }
            
            Console.WriteLine($"🖌️ FormConfig 테두리 그리기: Color={FormBorderColor}, Width={FormBorderWidth}, Size={this.ClientSize}");
        }

        /// <summary>
        /// 크기 변경 시 다시 그리기
        /// </summary>
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            // 크기가 변경되면 다시 그리기
            this.Invalidate();
        }

        #endregion
        
        #region Border Customization Methods

        /// <summary>
        /// 테두리 스타일을 설정하는 헬퍼 메서드
        /// </summary>
        /// <param name="color">테두리 색상</param>
        /// <param name="width">테두리 두께</param>
        public void SetBorderStyle(Color color, int width)
        {
            FormBorderColor = color;
            FormBorderWidth = width;
            Console.WriteLine($"🎨 FormConfig 테두리 스타일 변경: Color={color}, Width={width}");
        }

        /// <summary>
        /// 기본 테두리 스타일로 복원
        /// </summary>
        public void ResetBorderStyle()
        {
            SetBorderStyle(Color.Black, 2);
        }

        /// <summary>
        /// 테두리 강조 (빨간색, 두꺼운 선)
        /// </summary>
        public void HighlightBorder()
        {
            SetBorderStyle(Color.Red, 4);
        }

        #endregion
    }
}
