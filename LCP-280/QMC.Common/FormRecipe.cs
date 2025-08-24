using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QMC.Common
{
    public partial class FormRecipe : Form
    {
        private TabControl recipeTabControl;
        private Dictionary<TabPage, Form> _tabFormInstances;
        
        // Theme fields
        private int _tabHeight = 28;
        private Color _tabBorderColor = Color.Black;
        private int _tabBorderWidth = 2;
        private Font _tabFont = new Font("맑은 고딕", 9, FontStyle.Regular);

        public FormRecipe()
        {
            InitializeComponent();
            
            // 🔧 배경색을 흰색으로 설정
            this.BackColor = Color.White;
            
            _tabFormInstances = new Dictionary<TabPage, Form>();
            InitializerecipeUI();
            
            // 🔧 Visible 상태 변경 이벤트 추가
            this.VisibleChanged += Formrecipe_VisibleChanged;
        }
        
        /// <summary>
        /// 🔧 Formrecipe가 보여질 때마다 크기 재조정
        /// </summary>
        private void Formrecipe_VisibleChanged(object sender, EventArgs e)
        {
            if (this.Visible && this.Parent != null)
            {
                Console.WriteLine($"👁️ Formrecipe Visible 변경: {this.Visible}");
                Console.WriteLine($"   Parent: {this.Parent.GetType().Name}, Parent.Size: {this.Parent.Size}");
                
                // 🔧 TableLayoutPanel인 경우 정확한 행 크기를 계산하여 전달
                if (this.Parent is TableLayoutPanel tableLayoutPanel)
                {
                    try
                    {
                        int[] rowHeights = tableLayoutPanel.GetRowHeights();
                        int[] columnWidths = tableLayoutPanel.GetColumnWidths();
                        
                        // Formrecipe는 1번 행(인덱스 1, 80% 영역)에 위치
                        if (rowHeights.Length > 1 && columnWidths.Length > 0)
                        {
                            int width = columnWidths[0];
                            int height = rowHeights[1]; // 1번 행 (80% 영역)
                            
                            Console.WriteLine($"   계산된 Formrecipe 크기: width={width}, height={height}");
                            SetPanelSize(width, height);
                        }
                        else
                        {
                            Console.WriteLine("   ⚠️ TableLayoutPanel 행/열 정보가 부족함");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"   ❌ TableLayoutPanel 크기 계산 오류: {ex.Message}");
                        // fallback: 전체 크기 사용
                        if (this.Parent.Size.Width > 0 && this.Parent.Size.Height > 0)
                        {
                            SetPanelSize(this.Parent.Size.Width, this.Parent.Size.Height);
                        }
                    }
                }
                else
                {
                    // TableLayoutPanel이 아닌 경우 기존 방식 사용
                    if (this.Parent.Size.Width > 0 && this.Parent.Size.Height > 0)
                    {
                        Console.WriteLine($"   일반 Parent 크기 사용: {this.Parent.Size}");
                        SetPanelSize(this.Parent.Size.Width, this.Parent.Size.Height);
                    }
                }
            }
        }
        
        private void InitializerecipeUI()
        {
            Console.WriteLine("🚀 Formrecipe.InitializerecipeUI() 시작");
            
            // 🔧 Formrecipe 배경색을 확실히 흰색으로 설정
            this.BackColor = Color.White;
            
            // TabControl 생성 및 테마 적용
            recipeTabControl = new TabControl();
            recipeTabControl.Dock = DockStyle.None;
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
            
            Console.WriteLine($"✅ InitializerecipeUI 완료");
            Console.WriteLine($"   최종 TabControl 상태: Visible={recipeTabControl.Visible}, TabCount={recipeTabControl.TabPages.Count}");
        }

        /// <summary>
        /// FormManager에서 recipe 타입으로 등록된 폼들을 탭으로 로드
        /// </summary>
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
                
                // 등록된 폼이 없으면 기본 샘플 탭 생성
                if (recipeForms.Count == 0)
                {
                    Console.WriteLine("⚠️ 등록된 recipe 폼이 없어서 기본 샘플 탭 생성");
                    CreateSampleTabs();
                }
                
                Console.WriteLine($"✅ 최종 탭 개수: {recipeTabControl.TabPages.Count}");
                
                // 탭 컨트롤 상태 확인
                Console.WriteLine($"   recipeTabControl.Visible: {recipeTabControl.Visible}");
                Console.WriteLine($"   recipeTabControl.Size: {recipeTabControl.Size}");
                Console.WriteLine($"   recipeTabControl.Dock: {recipeTabControl.Dock}");
                
                // Formrecipe 자체 상태도 확인
                Console.WriteLine($"   Formrecipe.Visible: {this.Visible}");
                Console.WriteLine($"   Formrecipe.Size: {this.Size}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ recipe 폼 로드 중 오류: {ex.Message}");
                MessageBox.Show($"recipe 폼 로드 중 오류 발생: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            
            recipeTabControl.TabPages.Add(tabPage);
            
            Console.WriteLine($"   탭 추가 완료. 현재 탭 수: {recipeTabControl.TabPages.Count}");
        }

        /// <summary>
        /// 탭이 선택되었을 때 해당 폼을 로드하여 표시
        /// </summary>
        private void recipeTabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            TabPage selectedTab = recipeTabControl.SelectedTab;
            if (selectedTab?.Tag is FormInfo formInfo)
            {
                LoadFormIntoTab(selectedTab, formInfo);
            }
        }

        /// <summary>
        /// 탭에 폼을 로드하여 표시
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
                    
                    // 🔧 폼에 SetPanelSize 메서드가 있으면 탭 높이를 제외한 크기 전달
                    if (recipeTabControl != null)
                    {
                        int availableWidth = recipeTabControl.Width;
                        int availableHeight = recipeTabControl.Height - _tabHeight - 20; // 탭 높이 제외
                        
                        // 리플렉션을 사용하여 SetPanelSize 메서드 확인 및 호출
                        var setPanelSizeMethod = formInstance.GetType().GetMethod("SetPanelSize", 
                            new Type[] { typeof(int), typeof(int) });
                        
                        if (setPanelSizeMethod != null)
                        {
                            Console.WriteLine($"🔧 {formInstance.GetType().Name}에 SetPanelSize 메서드 발견");
                            Console.WriteLine($"   전달할 크기: width={availableWidth}, height={availableHeight} (탭 높이 {_tabHeight} 제외)");
                            
                            setPanelSizeMethod.Invoke(formInstance, new object[] { availableWidth, availableHeight });
                        }
                        else
                        {
                            Console.WriteLine($"   {formInstance.GetType().Name}에 SetPanelSize 메서드가 없음");
                        }
                    }
                    
                    // 탭에 폼 추가
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
                    
                    // 🔧 기존 폼에도 탭 높이를 제외한 크기 재적용
                    if (recipeTabControl != null)
                    {
                        int availableWidth = recipeTabControl.Width;
                        int availableHeight = recipeTabControl.Height - _tabHeight; // 탭 높이 제외
                        
                        var setPanelSizeMethod = existingForm.GetType().GetMethod("SetPanelSize", 
                            new Type[] { typeof(int), typeof(int) });
                        
                        if (setPanelSizeMethod != null)
                        {
                            Console.WriteLine($"🔧 기존 {existingForm.GetType().Name}에 크기 재적용");
                            Console.WriteLine($"   전달할 크기: width={availableWidth}, height={availableHeight} (탭 높이 {_tabHeight} 제외)");
                            
                            setPanelSizeMethod.Invoke(existingForm, new object[] { availableWidth, availableHeight });
                        }
                    }
                    
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
        /// FormManager에 새로운 폼이 등록되었을 때 탭을 새로고침
        /// </summary>
        public void RefreshrecipeTabs()
        {
            // 기존 탭과 폼 인스턴스 정리
            foreach (var formInstance in _tabFormInstances.Values)
            {
                formInstance?.Dispose();
            }
            _tabFormInstances.Clear();
            recipeTabControl.TabPages.Clear();
            
            // 새로 로드
            LoadFormsFromManager();
        }

        private void recipeTabControl_DrawItem(object sender, DrawItemEventArgs e)
        {
            TabPage page = recipeTabControl.TabPages[e.Index];
            Rectangle tabRect = recipeTabControl.GetTabRect(e.Index);

            // 선택된 탭은 하얀색, 아닌 탭은 회색
            Color backColor = (e.Index == recipeTabControl.SelectedIndex) ? Color.White : Color.Gainsboro;
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
            // System recipe 탭
            TabPage systemTab = new TabPage("Sample recipe");
            
            // 🔧 샘플 탭의 배경색도 흰색으로 설정
            systemTab.BackColor = Color.White;
            
            Label systemLabel = new Label();
            systemLabel.Text = "No recipe Forms Registered\n\nUse FormManager.Instance.RegisterForm() to add recipe forms.";
            systemLabel.Font = new Font("맑은 고딕", 12, FontStyle.Regular);
            systemLabel.TextAlign = ContentAlignment.MiddleCenter;
            systemLabel.Dock = DockStyle.Fill;
            
            // 🔧 라벨의 배경색도 흰색으로 설정
            systemLabel.BackColor = Color.White;
            
            systemTab.Controls.Add(systemLabel);
            recipeTabControl.TabPages.Add(systemTab);
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
            Console.WriteLine($"🔧 Formrecipe.SetPanelSize() 호출: width={width}, height={height}");
            Console.WriteLine($"   현재 Formrecipe 크기: Size={this.Size}, ClientSize={this.ClientSize}");
            
            this.Size = new Size(width, height);
            this.ClientSize = new Size(width, height);
            
            if (recipeTabControl != null)
            {
                Console.WriteLine($"   TabControl 크기 조정: {recipeTabControl.Size} → {new Size(width, height)}");
                recipeTabControl.Size = new Size(width, height);
                
                // 🔧 현재 활성화된 탭의 폼에 탭 높이를 제외한 크기 전달
                var selectedTab = recipeTabControl.SelectedTab;
                if (selectedTab != null && _tabFormInstances.ContainsKey(selectedTab))
                {
                    var activeForm = _tabFormInstances[selectedTab];
                    
                    int availableWidth = width;
                    int availableHeight = height - _tabHeight; // 탭 높이 제외
                    
                    var setPanelSizeMethod = activeForm.GetType().GetMethod("SetPanelSize", 
                        new Type[] { typeof(int), typeof(int) });
                    
                    if (setPanelSizeMethod != null)
                    {
                        Console.WriteLine($"   활성 폼 {activeForm.GetType().Name}에 크기 업데이트");
                        Console.WriteLine($"   전달할 크기: width={availableWidth}, height={availableHeight} (탭 높이 {_tabHeight} 제외)");
                        
                        setPanelSizeMethod.Invoke(activeForm, new object[] { availableWidth, availableHeight });
                    }
                }
                
                // TabControl을 강제로 다시 그리기
                recipeTabControl.Invalidate();
                recipeTabControl.Update();
            }
            
            // 폼 전체를 다시 그리기
            this.Invalidate();
            this.Update();
            
            Console.WriteLine($"✅ Formrecipe.SetPanelSize() 완료: 최종 크기={this.Size}");
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
            
            Console.WriteLine($"🖌️ Formrecipe 테두리 그리기: Color={FormBorderColor}, Width={FormBorderWidth}, Size={this.ClientSize}");
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
            Console.WriteLine($"🎨 Formrecipe 테두리 스타일 변경: Color={color}, Width={width}");
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
