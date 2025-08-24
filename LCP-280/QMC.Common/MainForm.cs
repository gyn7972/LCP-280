using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace QMC.Common
{
    public partial class MainForm : Form
    {
        #region Field
        private Size MainSize;
        private TableLayoutPanel tableLayoutPanelFormMain;
        private Panel centerPanel; // 중앙 컨텐츠 전용 컨테이너
        private FormTop formTop;
        private FormBottom formBottom;
        private Form currentCenterForm;

        // 메뉴별 중앙 폼 캐시
        private readonly Dictionary<MenuButtonType, Form> _centerFormCache = new Dictionary<MenuButtonType, Form>();
        #endregion
        
        public MainForm()
        {
            InitializeComponent();
            
            // 🔧 MainForm 배경색을 흰색으로 설정
            this.BackColor = Color.White;
            
            this.StartPosition = FormStartPosition.WindowsDefaultLocation;
            this.WindowState = FormWindowState.Normal;           // 일반 상태로 시작
            this.Load += MainForm_Load;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            MainSize = new Size(1280, 1024);
            // 권장: 클라이언트 영역 기준으로 설정
            this.ClientSize = MainSize;

            // 🔧 MainForm 배경색을 다시 한 번 확실히 흰색으로 설정
            this.BackColor = Color.White;

            // FormManager 시스템 초기화
            InitializeFormManagers();

            // TableLayoutPanel 생성 및 설정
            tableLayoutPanelFormMain = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 3,
                ColumnCount = 1,
                // 🔧 TableLayoutPanel 배경색도 흰색으로 설정
                BackColor = Color.White
            };

            // 각 행을 동일한 비율로 분할
            tableLayoutPanelFormMain.RowStyles.Add(new RowStyle(SizeType.Percent, 10F));
            tableLayoutPanelFormMain.RowStyles.Add(new RowStyle(SizeType.Percent, 80F));
            tableLayoutPanelFormMain.RowStyles.Add(new RowStyle(SizeType.Percent, 10F));
            tableLayoutPanelFormMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            this.Controls.Add(tableLayoutPanelFormMain);

            // FormTop을 첫번째 행(인덱스 0)에 추가
            formTop = new FormTop();
            formTop.TopLevel = false;
            formTop.FormBorderStyle = FormBorderStyle.None;
            formTop.Dock = DockStyle.Fill;
            tableLayoutPanelFormMain.Controls.Add(formTop, 0, 0);
            formTop.Show();

            // Center 전용 Panel 생성 및 추가
            centerPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White
            };
            tableLayoutPanelFormMain.Controls.Add(centerPanel, 0, 1);

            // FormBottom을 세 번째 행(인덱스 2에 추가
            formBottom = new FormBottom();
            formBottom.TopLevel = false;
            formBottom.FormBorderStyle = FormBorderStyle.None;
            formBottom.Dock = DockStyle.Fill;
            tableLayoutPanelFormMain.Controls.Add(formBottom, 0, 2);
            formBottom.Show();

            // FormBottom의 메뉴 버튼 클릭 이벤트 구독
            formBottom.MenuButtonClicked += FormBottom_MenuButtonClicked;

            // 폼이 보여진 후 실제 Width, Height 전달 및 리사이즈 연동
            this.Shown += (s, args) => ApplySizes();
            this.SizeChanged += (s, args) => ApplySizes();

            // 🚀 폼이 처음 로드될 때 기본으로 Main 폼을 중앙에 표시
            SwitchCenterForm(MenuButtonType.Main);
        }

        /// <summary>
        /// FormManager 시스템 초기화 및 샘플 폼 등록
        /// </summary>
        private void InitializeFormManagers()
        {
            try
            {
                Console.WriteLine("🚀 FormManager 시스템 초기화 시작");
                
                // 모든 FormManager 타입의 자동 등록 실행
                FormManagerConfig.Instance.AutoRegisterUnitConfigForms();
                FormManagerMain.Instance.AutoRegisterUnitMainForms();
                FormManagerWorking.Instance.AutoRegisterUnitWorkingForms();
                FormManagerRecipe.Instance.AutoRegisterUnitRecipeForms();
                FormManagerSetup.Instance.AutoRegisterUnitSetupForms();
                FormManagerLog.Instance.AutoRegisterUnitLogForms();
                
                // 🔧 수동 등록 (자동 등록이 실패할 경우 사용)
                // FormManagerConfig.Instance.RegisterConfigForm(typeof(QMC.LCP_280.Process.Unit.CassetteLoadingElevatorUnit_Config), "CassetteLoadingElevator", "카세트 로딩 엘리베이터 설정");
                // FormManagerConfig.Instance.RegisterConfigForm(typeof(QMC.LCP_280.Process.Unit.WaferAlignmentUnit_Config), "WaferAlignment", "웨이퍼 정렬 설정");
                // FormManagerConfig.Instance.RegisterConfigForm(typeof(QMC.LCP_280.Process.Unit.DieLoaderUnit_Config), "DieLoader", "다이 로더 설정");
                
                Console.WriteLine("✅ FormManager 시스템 초기화 완료");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ FormManager 초기화 오류: {ex.Message}");
                MessageBox.Show($"FormManager 초기화 중 오류 발생: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void FormBottom_MenuButtonClicked(MenuButtonType menuType)
        {
            SwitchCenterForm(menuType);
        }

        private void SwitchCenterForm(MenuButtonType menuType)
        {
            // 현재 표시된 폼 숨기기 및 컨테이너에서 제거
            if (currentCenterForm != null)
            {
                currentCenterForm.Hide();
                if (centerPanel != null && centerPanel.Controls.Contains(currentCenterForm))
                {
                    centerPanel.Controls.Remove(currentCenterForm);
                }
            }

            // 메뉴 타입에 따라 적절한 폼 표시
            try
            {
                switch (menuType)
                {
                    case MenuButtonType.Main:
                        ShowMainForm();
                        break;
                    case MenuButtonType.Config:
                        ShowConfigForm();
                        break;
                    case MenuButtonType.Working:
                        ShowWorkingForm();
                        break;
                    case MenuButtonType.Recipe:
                        ShowRecipeForm();
                        break;
                    case MenuButtonType.Setup:
                        ShowSetupForm();
                        break;
                    case MenuButtonType.Log:
                        ShowLogForm();
                        break;
                    default:
                        ShowMainForm();
                        break;
                }
            }
            catch (Exception ex)
            {
                ShowNotImplementedMessage(menuType.ToString(), ex.Message);
            }

            // 선택된 폼 표시
            if (currentCenterForm != null)
            {
                if (!centerPanel.Controls.Contains(currentCenterForm))
                {
                    centerPanel.Controls.Add(currentCenterForm);
                }
                currentCenterForm.Visible = true;
                currentCenterForm.BringToFront();
                
                // 사이즈 적용
                ApplySizes();

                // 🔧 폼이 표시된 후 추가로 크기 확인
                Console.WriteLine($"📏 최종 표시된 폼: {currentCenterForm.GetType().Name}, Size={currentCenterForm.Size}, Visible={currentCenterForm.Visible}");
            }
        }

        private void ShowMainForm()
        {
            var mainForms = FormManagerMain.Instance.GetMainForms();
            if (mainForms.Count > 0)
            {
                try
                {
                    Form mainForm = GetOrCreateForm(MenuButtonType.Main, () => FormManagerMain.Instance.CreateMainForm(), "FormMain");
                    SetupCenterForm(mainForm);
                }
                catch (Exception ex)
                {
                    ShowNotImplementedMessage("Main", ex.Message);
                }
            }
            else
            {
                ShowNotImplementedMessage("Main", "등록된 Main 폼이 없습니다.");
            }
        }

        private void ShowConfigForm() 
        {
            var configForms = FormManagerConfig.Instance.GetConfigForms();
            if (configForms.Count > 0)
            {
                try
                {
                    Form configForm = GetOrCreateForm(MenuButtonType.Config, () => FormManagerConfig.Instance.CreateConfigForm(), "ConfigForm");
                    SetupCenterForm(configForm);
                }
                catch (Exception ex)
                {
                    ShowNotImplementedMessage("Config", ex.Message);
                }
            }
            else
            {
                ShowNotImplementedMessage("Config", "등록된 Config 폼이 없습니다.");
            }
        }

        private void ShowWorkingForm()
        {
            var workingForms = FormManagerWorking.Instance.GetWorkingForms();
            if (workingForms.Count > 0)
            {
                try
                {
                    Form workingForm = GetOrCreateForm(MenuButtonType.Working, () => FormManagerWorking.Instance.CreateWorkingForm(), "WorkingForm");
                    SetupCenterForm(workingForm);
                }
                catch (Exception ex)
                {
                    ShowNotImplementedMessage("Working", ex.Message);
                }
            }
            else
            {
                ShowNotImplementedMessage("Working", "등록된 Working 폼이 없습니다.");
            }
        }

        private void ShowRecipeForm()
        {
            var recipeForms = FormManagerRecipe.Instance.GetRecipeForms();
            if (recipeForms.Count > 0)
            {
                try
                {
                    Form recipeForm = GetOrCreateForm(MenuButtonType.Recipe, () => FormManagerRecipe.Instance.CreateRecipeForm(), "RecipeForm");
                    SetupCenterForm(recipeForm);
                }
                catch (Exception ex)
                {
                    ShowNotImplementedMessage("Recipe", ex.Message);
                }
            }
            else
            {
                ShowNotImplementedMessage("Recipe", "등록된 Recipe 폼이 없습니다.");
            }
        }

        private void ShowSetupForm()
        {
            var setupForms = FormManagerSetup.Instance.GetSetupForms();
            if (setupForms.Count > 0)
            {
                try
                {
                    Form setupForm = GetOrCreateForm(MenuButtonType.Setup, () => FormManagerSetup.Instance.CreateSetupForm(), "SetupForm");
                    SetupCenterForm(setupForm);
                }
                catch (Exception ex)
                {
                    ShowNotImplementedMessage("Setup", ex.Message);
                }
            }
            else
            {
                ShowNotImplementedMessage("Setup", "등록된 Setup 폼이 없습니다.");
            }
        }

        private void ShowLogForm()
        {
            var logForms = FormManagerLog.Instance.GetLogForms();
            if (logForms.Count > 0)
            {
                try
                {
                    Form logForm = GetOrCreateForm(MenuButtonType.Log, () => FormManagerLog.Instance.CreateLogForm(), "LogForm");
                    SetupCenterForm(logForm);
                }
                catch (Exception ex)
                {
                    ShowNotImplementedMessage("Log", ex.Message);
                }
            }
            else
            {
                ShowNotImplementedMessage("Log", "등록된 Log 폼이 없습니다.");
            }
        }

        private Form GetOrCreateForm(MenuButtonType type, Func<Form> factory, string formName)
        {
            Form form;
            if (!_centerFormCache.TryGetValue(type, out form) || form == null || form.IsDisposed)
            {
                form = factory();
                form.TopLevel = false;
                form.FormBorderStyle = FormBorderStyle.None;
                form.Dock = DockStyle.Fill;
                form.Name = formName;
                _centerFormCache[type] = form;
            }
            return form;
        }

        private void SetupCenterForm(Form form)
        {
            // 컨테이너 비우고 폼 추가
            centerPanel.Controls.Clear();
            centerPanel.Controls.Add(form);
            currentCenterForm = form;
            if (!form.Visible) form.Show();
        }

        private void ApplySizes()
        {
            if (tableLayoutPanelFormMain == null) return;

            int[] rowHeights = tableLayoutPanelFormMain.GetRowHeights();
            int[] colWidths = tableLayoutPanelFormMain.GetColumnWidths();
            if (rowHeights.Length < 3 || colWidths.Length < 1) return;

            int width = colWidths[0];
            try
            {
                // Top/Bottom 고정 반영
                formTop?.SetPanelSize(width, rowHeights[0]);
                formBottom?.SetPanelSize(width, rowHeights[2]);

                // Center 컨텐츠 반영 (SetPanelSize가 있는 경우만)
                if (currentCenterForm != null)
                {
                    TrySetPanelSize(currentCenterForm, width, rowHeights[1]);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Size apply failed: {ex.Message}");
            }
        }

        private static void TrySetPanelSize(Form form, int width, int height)
        {
            try
            {
                MethodInfo mi = form.GetType().GetMethod(
                    "SetPanelSize",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                    null,
                    new Type[] { typeof(int), typeof(int) },
                    null);

                if (mi != null)
                {
                    mi.Invoke(form, new object[] { width, height });
                }
            }
            catch
            {
                // 무시
            }
        }

        private void ShowNotImplementedMessage(string menuName, string additionalInfo = null)
        {
            string message = $"'{menuName}' 메뉴는 아직 구현되지 않았습니다.";
            if (!string.IsNullOrEmpty(additionalInfo))
            {
                message += $"\n\n{additionalInfo}";
            }
            message += $"\n\nFormManager{menuName}.Instance.Register{menuName}Form()을 사용하여 폼을 등록하세요.";
            
            MessageBox.Show(message, "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // 기본 동작: 중앙 컨텐츠 비우기
            centerPanel.Controls.Clear();
            currentCenterForm = null;
        }
    }
}
