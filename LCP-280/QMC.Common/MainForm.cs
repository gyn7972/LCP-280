using System;
using System.Drawing;
using System.Windows.Forms;

namespace QMC.Common
{
    public partial class MainForm : Form
    {
        #region Field
        private Size MainSize;
        private TableLayoutPanel tableLayoutPanelFormMain;
        private FormTop formTop;
        private FormBottom formBottom;
        private Form currentCenterForm;
        private FormConfig formConfig;
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
            this.Size = MainSize;
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

            // FormConfig 초기화 (숨김 상태)
            formConfig = new FormConfig();
            formConfig.TopLevel = false;
            formConfig.FormBorderStyle = FormBorderStyle.None;
            formConfig.Dock = DockStyle.Fill;
            formConfig.Visible = false;
            tableLayoutPanelFormMain.Controls.Add(formConfig, 0, 1);

            // FormBottom을 세 번째 행(인덱스 2에 추가
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

            // 🚀 폼이 처음 로드될 때 기본으로 Main 폼을 중앙에 표시
            ShowMainForm();
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
                    Console.WriteLine("🔘 Config 메뉴 버튼 클릭됨");
                    currentCenterForm = formConfig;
                    
                    // 🔧 FormConfig에 현재 크기 적용
                    if (tableLayoutPanelFormMain != null)
                    {
                        int[] rowHeights = tableLayoutPanelFormMain.GetRowHeights();
                        int width = tableLayoutPanelFormMain.GetColumnWidths()[0];
                        if (rowHeights.Length > 1)
                        {
                            Console.WriteLine($"   FormConfig에 크기 적용: width={width}, height={rowHeights[1]}");
                            formConfig.SetPanelSize(width, rowHeights[1]);
                            formConfig.SetBorderStyle(Color.Black, 2);
                        }
                    }
                    
                    Console.WriteLine($"   FormConfig 설정 완료: Visible={formConfig.Visible}, Size={formConfig.Size}");
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
                
                // 🔧 폼이 표시된 후 추가로 크기 확인
                Console.WriteLine($"📏 최종 표시된 폼: {currentCenterForm.GetType().Name}, Size={currentCenterForm.Size}, Visible={currentCenterForm.Visible}");
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

        private void ShowConfigForm() 
        {
            var configForms = FormManagerConfig.Instance.GetConfigForms();
            if (configForms.Count > 0)
            {
                try
                {
                    Form configForm = FormManagerConfig.Instance.CreateConfigForm();
                    SetupCenterForm(configForm, "ConfigForm");
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
