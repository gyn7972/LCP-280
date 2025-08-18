using System;
using System.Drawing;
using System.Windows.Forms;

namespace QMC.Common
{
    public partial class FormMain : Form
    {
        #region Field
        private Size MainSize;
        private TableLayoutPanel tableLayoutPanelFormMain;
        private FormTop formTop;
        private FormBottom formBottom;
        private Form currentCenterForm;
        private FormConfig formConfig;
        #endregion
        
        public FormMain()
        {
            InitializeComponent();
            this.StartPosition = FormStartPosition.WindowsDefaultLocation;
            this.WindowState = FormWindowState.Normal;           // 일반 상태로 시작
            this.Load += MainForm_Load;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            MainSize = new Size(1280, 1024);
            this.Size = MainSize;
            this.ClientSize = MainSize;

            // FormManager 시스템 초기화
            InitializeFormManagers();

            // TableLayoutPanel 생성 및 설정
            tableLayoutPanelFormMain = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 3,
                ColumnCount = 1
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

            // FormConfig 초기화 (숨김 상태)
            formConfig = new FormConfig();
            formConfig.TopLevel = false;
            formConfig.FormBorderStyle = FormBorderStyle.None;
            formConfig.Dock = DockStyle.Fill;
            formConfig.Visible = false;
            tableLayoutPanelFormMain.Controls.Add(formConfig, 0, 1);

            // FormBottom을 세 번째 행(인덱스 2)에 추가
            formBottom = new FormBottom();
            formBottom.TopLevel = false;
            formBottom.FormBorderStyle = FormBorderStyle.None;
            formBottom.Dock = DockStyle.Fill;
            tableLayoutPanelFormMain.Controls.Add(formBottom, 0, 2);
            formBottom.Show();

            // FormBottom의 메뉴 버튼 클릭 이벤트 구독
            formBottom.MenuButtonClicked += FormBottom_MenuButtonClicked;

            // 폼이 보여진 후 실제 Width, Height 전달
            this.Shown += (s, args) =>
            {
                int[] rowHeights = tableLayoutPanelFormMain.GetRowHeights();
                int width = tableLayoutPanelFormMain.GetColumnWidths()[0];
                if (rowHeights.Length > 2)
                {
                    formTop.SetPanelSize(width, rowHeights[0]);
                    formConfig.SetPanelSize(width, rowHeights[1]);
                    formBottom.SetPanelSize(width, rowHeights[2]);

                    // MainForm이 현재 표시 중이면 사이즈 적용
                    if (currentCenterForm != null)
                    {
                        // FormConfig 또는 SetPanelSize를 가진 타입에만 적용
                        if (currentCenterForm is FormConfig fc)
                        {
                            fc.SetPanelSize(width, rowHeights[1]);
                        }
                        else if (currentCenterForm is FormTop ft)
                        {
                            ft.SetPanelSize(width, rowHeights[1]);
                        }
                        else if (currentCenterForm is FormBottom fb)
                        {
                            fb.SetPanelSize(width, rowHeights[1]);
                        }
                    }
                }
            };
        }

        /// <summary>
        /// FormManager 시스템 초기화 및 샘플 폼 등록
        /// </summary>
        private void InitializeFormManagers()
        {
            try
            {
                // 모든 FormManager 타입의 자동 등록 실행
                FormManagerConfig.Instance.AutoRegisterUnitConfigForms();
                FormManagerMain.Instance.AutoRegisterUnitMainForms();
                FormManagerWorking.Instance.AutoRegisterUnitWorkingForms();
                FormManagerRecipe.Instance.AutoRegisterUnitRecipeForms();
                FormManagerSetup.Instance.AutoRegisterUnitSetupForms();
                FormManagerLog.Instance.AutoRegisterUnitLogForms();
                
                // 추가적인 수동 등록 예시 (필요시 사용)
                // FormManagerMain.Instance.RegisterMainForm(typeof(CustomMainForm), "Custom Main", "사용자 정의 메인 화면");
                // FormManagerWorking.Instance.RegisterWorkingForm(typeof(ProcessMonitorForm), "Process Monitor", "공정 모니터링");
                // FormManagerRecipe.Instance.RegisterRecipeForm(typeof(RecipeEditorForm), "Recipe Editor", "레시피 편집기");
                // FormManagerSetup.Instance.RegisterSetupForm(typeof(SystemSetupForm), "System Setup", "시스템 설정");
                // FormManagerLog.Instance.RegisterLogForm(typeof(SystemLogForm), "System Log", "시스템 로그");
                
            }
            catch (Exception ex)
            {
                MessageBox.Show($"FormManager 초기화 중 오류 발생: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void FormBottom_MenuButtonClicked(MenuButtonType menuType)
        {
            SwitchCenterForm(menuType);
        }

        private void SwitchCenterForm(MenuButtonType menuType)
        {
            // 현재 표시된 폼 숨기기
            if (currentCenterForm != null)
            {
                currentCenterForm.Visible = false;
            }

            // 메뉴 타입에 따라 적절한 폼 표시
            switch (menuType)
            {
                case MenuButtonType.Main:
                    ShowMainForm();
                    break;
                case MenuButtonType.Config:
                    currentCenterForm = formConfig;
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
                    // 기본값으로 Config 표시
                    currentCenterForm = formConfig;
                    break;
            }

            // 선택된 폼 표시
            if (currentCenterForm != null)
            {
                currentCenterForm.Visible = true;
                currentCenterForm.BringToFront();
            }
        }

        private void ShowMainForm()
        {
            try
            {
                // 기존 Main 폼이 있으면 제거
                if (currentCenterForm != null && currentCenterForm.Name == "MainForm")
                {
                    currentCenterForm = null;
                }

                // FormManagerMain에서 Main 폼 생성
                Form mainForm = FormManagerMain.Instance.CreateMainForm();
                mainForm.TopLevel = false;
                mainForm.FormBorderStyle = FormBorderStyle.None;
                mainForm.Dock = DockStyle.Fill;
                mainForm.Name = "MainForm";
                
                tableLayoutPanelFormMain.Controls.Add(mainForm, 0, 1);
                currentCenterForm = mainForm;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Main 폼 로드 중 오류: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                currentCenterForm = formConfig; // fallback
            }
        }

        private void ShowWorkingForm()
        {
            var workingForms = FormManagerWorking.Instance.GetWorkingForms();
            if (workingForms.Count > 0)
            {
                try
                {
                    Form workingForm = FormManagerWorking.Instance.CreateWorkingForm();
                    SetupCenterForm(workingForm, "WorkingForm");
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
                    Form recipeForm = FormManagerRecipe.Instance.CreateRecipeForm();
                    SetupCenterForm(recipeForm, "RecipeForm");
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
                    Form setupForm = FormManagerSetup.Instance.CreateSetupForm();
                    SetupCenterForm(setupForm, "SetupForm");
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
                    Form logForm = FormManagerLog.Instance.CreateLogForm();
                    SetupCenterForm(logForm, "LogForm");
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

        private void SetupCenterForm(Form form, string formName)
        {
            form.TopLevel = false;
            form.FormBorderStyle = FormBorderStyle.None;
            form.Dock = DockStyle.Fill;
            form.Name = formName;
            
            tableLayoutPanelFormMain.Controls.Add(form, 0, 1);
            currentCenterForm = form;
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
            currentCenterForm = formConfig; // fallback to config
        }
    }
}
