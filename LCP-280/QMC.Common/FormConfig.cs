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
    public partial class FormConfig : Form
    {
        private TabControl configTabControl;
        private Dictionary<TabPage, Form> _tabFormInstances;
        
        // Theme fields
        private int _tabHeight = 28;
        private Color _tabBorderColor = Color.Black;
        private int _tabBorderWidth = 2;
        private Font _tabFont = new Font("맑은 고딕", 9, FontStyle.Regular);

        public FormConfig()
        {
            InitializeComponent();
            _tabFormInstances = new Dictionary<TabPage, Form>();
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
            configTabControl.SelectedIndexChanged += ConfigTabControl_SelectedIndexChanged;
            this.Controls.Add(configTabControl);
            
            // FormManager에서 등록된 Config 폼들을 자동으로 탭으로 추가
            LoadConfigFormsFromManager();
        }

        /// <summary>
        /// FormManager에서 Config 타입으로 등록된 폼들을 탭으로 로드
        /// </summary>
        private void LoadConfigFormsFromManager()
        {
            try
            {
                var configForms = FormManager.Instance.GetRegisteredForms(MenuButtonType.Config);
                
                foreach (var formInfo in configForms)
                {
                    CreateTabFromFormInfo(formInfo);
                }
                
                // 등록된 폼이 없으면 기본 샘플 탭 생성
                if (configForms.Count == 0)
                {
                    CreateSampleTabs();
                }
            }
            catch (Exception ex)
            {
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
            TabPage tabPage = new TabPage(formInfo.DisplayName);
            tabPage.Tag = formInfo; // FormInfo를 Tag에 저장
            configTabControl.TabPages.Add(tabPage);
        }

        /// <summary>
        /// 탭이 선택되었을 때 해당 폼을 로드하여 표시
        /// </summary>
        private void ConfigTabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            TabPage selectedTab = configTabControl.SelectedTab;
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
                    _tabFormInstances[tabPage].Show();
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
        public void RefreshConfigTabs()
        {
            // 기존 탭과 폼 인스턴스 정리
            foreach (var formInstance in _tabFormInstances.Values)
            {
                formInstance?.Dispose();
            }
            _tabFormInstances.Clear();
            configTabControl.TabPages.Clear();
            
            // 새로 로드
            LoadConfigFormsFromManager();
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
            Label systemLabel = new Label();
            systemLabel.Text = "No Config Forms Registered\n\nUse FormManager.Instance.RegisterForm() to add config forms.";
            systemLabel.Font = new Font("맑은 고딕", 12, FontStyle.Regular);
            systemLabel.TextAlign = ContentAlignment.MiddleCenter;
            systemLabel.Dock = DockStyle.Fill;
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
            this.Size = new Size(width, height);
            this.ClientSize = new Size(width, height);
            if (configTabControl != null)
            {
                configTabControl.Size = new Size(width, height);
            }
        }
    }
}
