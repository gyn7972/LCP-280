using QMC.Common.CustomControl;
using System.Drawing;
using System.Windows.Forms;

namespace QMC.Common
{
    partial class TopContentsLoginModeControl
    {
        /// <summary> 
        /// 필수 디자이너 변수입니다.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 사용 중인 모든 리소스를 정리합니다.
        /// </summary>
        /// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 구성 요소 디자이너에서 생성한 코드

        /// <summary> 
        /// 디자이너 지원에 필요한 메서드입니다. 
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마세요.
        /// </summary>
        private void InitializeComponent()
        {
            this.tableLayoutContentsLoginModePanel = new DeleteInnerBorderTableLayoutPanel();
            this._loginModeTitleLabel = new Label();
            this._loginModeLabel = new CustomBorderLabel();
            this.SuspendLayout();
            // 
            // tableLayoutContentsLoginModePanel
            // 
            this.tableLayoutContentsLoginModePanel.Dock = DockStyle.Fill;
            this.tableLayoutContentsLoginModePanel.BackColor = Color.White;
            this.tableLayoutContentsLoginModePanel.Margin = new Padding(0);
            this.tableLayoutContentsLoginModePanel.Padding = new Padding(0);
            this.tableLayoutContentsLoginModePanel.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;

            this.tableLayoutContentsLoginModePanel.ColumnCount = 1;
            this.tableLayoutContentsLoginModePanel.ColumnStyles.Clear();
            this.tableLayoutContentsLoginModePanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            this.tableLayoutContentsLoginModePanel.RowCount = 2;
            this.tableLayoutContentsLoginModePanel.RowStyles.Clear();
            this.tableLayoutContentsLoginModePanel.RowStyles.Add(new RowStyle(SizeType.Percent, 40F));
            this.tableLayoutContentsLoginModePanel.RowStyles.Add(new RowStyle(SizeType.Percent, 60F));
            // 
            // _loginModeTitleLabel
            // 
            this._loginModeTitleLabel.Text = "Login Mode";
            this._loginModeTitleLabel.TextAlign = ContentAlignment.MiddleLeft;
            this._loginModeTitleLabel.Dock = DockStyle.Fill;
            this._loginModeTitleLabel.Margin = new Padding(2);
            this._loginModeTitleLabel.Font = new Font("Arial", 8F, FontStyle.Bold, GraphicsUnit.Point, 0);
            // 
            // _loginModeLabel
            // 
            this._loginModeLabel.Text = "Guest";
            this._loginModeLabel.TextAlign = ContentAlignment.MiddleCenter;
            this._loginModeLabel.Dock = DockStyle.Fill;
            this._loginModeLabel.Margin = new Padding(5);
            this._loginModeLabel.Font = new Font("Arial", 8F, FontStyle.Bold, GraphicsUnit.Point, 0);
            this._loginModeLabel.ForeColor = Color.Lime;
            this._loginModeLabel.BackColor = Color.Black;
            this._loginModeLabel.TabStop = false;

            // 
            // Add controls
            // 
            this.tableLayoutContentsLoginModePanel.Controls.Add(this._loginModeTitleLabel, 0, 0);
            this.tableLayoutContentsLoginModePanel.Controls.Add(this._loginModeLabel, 0, 1);

            // 
            // TopContentsLoginModeControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = Color.White;
            this.Controls.Add(this.tableLayoutContentsLoginModePanel);
            this.Name = "TopContentsLoginModeControl";
            this.Size = new System.Drawing.Size(360, 80);
            this.ResumeLayout(false);
        }

        #endregion

        private DeleteInnerBorderTableLayoutPanel tableLayoutContentsLoginModePanel;
    }
}