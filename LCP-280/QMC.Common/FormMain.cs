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

            // Form1을 두 번째 행(인덱스 1)에 추가 (기본 화면)
            //form1 = new Form1();
            //form1.TopLevel = false;
            //form1.FormBorderStyle = FormBorderStyle.None;
            //form1.Dock = DockStyle.Fill;
            //tableLayoutPanelFormMain.Controls.Add(form1, 0, 1);
            //form1.Show();
            //currentCenterForm = form1;

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
                    formBottom.SetPanelSize(width, rowHeights[2]);
                }
            };
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
                    //currentCenterForm = form1;
                    break;
                case MenuButtonType.Config:
                    currentCenterForm = formConfig;
                    break;
                case MenuButtonType.Working:
                case MenuButtonType.Recipe:
                case MenuButtonType.Setup:
                case MenuButtonType.Log:
                default:
                    // 아직 구현되지 않은 메뉴는 기본 폼 표시
                    currentCenterForm = formConfig;
                    MessageBox.Show($"'{menuType}' 메뉴는 아직 구현되지 않았습니다.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    break;
            }

            // 선택된 폼 표시
            if (currentCenterForm != null)
            {
                currentCenterForm.Visible = true;
                currentCenterForm.BringToFront();
            }
        }
    }
}
