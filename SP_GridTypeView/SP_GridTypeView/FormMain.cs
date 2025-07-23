using System;
using System.Drawing;
using System.Windows.Forms;

namespace SP_GridTypeView
{
    public partial class FormMain : Form
    {
        #region Field
        private Size MainSize;
        private TableLayoutPanel tableLayoutPanelFormMain;
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

            // FormBottom을 첫번째 행(인덱스 0)에 추가
            var formTop = new FormTop();
            formTop.TopLevel = false;
            formTop.FormBorderStyle = FormBorderStyle.None;
            formTop.Dock = DockStyle.Fill;
            tableLayoutPanelFormMain.Controls.Add(formTop, 0, 0);
            formTop.Show();

            // Form1을 두 번째 행(인덱스 1)에 추가
            var form1 = new Form1();
            form1.TopLevel = false;
            form1.FormBorderStyle = FormBorderStyle.None;
            form1.Dock = DockStyle.Fill;
            tableLayoutPanelFormMain.Controls.Add(form1, 0, 1);
            form1.Show();

            // FormBottom을 세 번째 행(인덱스 2)에 추가
            var formBottom = new FormBottom();
            formBottom.TopLevel = false;
            formBottom.FormBorderStyle = FormBorderStyle.None;
            formBottom.Dock = DockStyle.Fill;
            tableLayoutPanelFormMain.Controls.Add(formBottom, 0, 2);
            formBottom.Show();

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


    }
}
