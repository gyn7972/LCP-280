using QMC.Common.CustomControl;
using System.Drawing;
using System.Windows.Forms;

namespace QMC.Common
{
    partial class TopContentsStatusControl
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
            this.tableLayoutContentsStatusPanel = new DeleteInnerBorderTableLayoutPanel();
            this._mesMessageTitleLabel = new CustomBorderLabel();
            this._systemMessageTitleLabel = new CustomBorderLabel();
            this._operationRecipeTitleLabel = new CustomBorderLabel();
            this._mesMessageLabel = new CustomBorderLabel();
            this._systemMessageLabel = new CustomBorderLabel();
            this._operationRecipeLabel = new CustomBorderLabel();
            this._AlarmClearButton = new IndividualMenuButton();
            this.SuspendLayout();
            // 
            // tableLayoutContentsStatusPanel
            // 
            this.tableLayoutContentsStatusPanel.Dock = DockStyle.Fill;
            this.tableLayoutContentsStatusPanel.BackColor = Color.White;
            this.tableLayoutContentsStatusPanel.Margin = new Padding(0);
            this.tableLayoutContentsStatusPanel.Padding = new Padding(0);
            this.tableLayoutContentsStatusPanel.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;

            this.tableLayoutContentsStatusPanel.ColumnCount = 3;
            this.tableLayoutContentsStatusPanel.ColumnStyles.Clear();
            this.tableLayoutContentsStatusPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 10F));
            this.tableLayoutContentsStatusPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 80F));
            this.tableLayoutContentsStatusPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 10F));

            this.tableLayoutContentsStatusPanel.RowCount = 3;
            this.tableLayoutContentsStatusPanel.RowStyles.Clear();
            this.tableLayoutContentsStatusPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 33.3333F));
            this.tableLayoutContentsStatusPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 33.3333F));
            this.tableLayoutContentsStatusPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 33.3333F));

            // 
            // _mesMessageTitleLabel
            // 
            this._mesMessageTitleLabel.Text = "MES MSG.";
            this._mesMessageTitleLabel.Dock = DockStyle.Fill;
            this._mesMessageTitleLabel.TextAlign = ContentAlignment.MiddleCenter;
            this._mesMessageTitleLabel.Margin = new Padding(2);
            // 
            // _systemMessageTitleLabel
            // 
            this._systemMessageTitleLabel.Text = "SYSTEM";
            this._systemMessageTitleLabel.Dock = DockStyle.Fill;
            this._systemMessageTitleLabel.TextAlign = ContentAlignment.MiddleCenter;
            this._systemMessageTitleLabel.Margin = new Padding(2);
            // 
            // _operationRecipeTitleLabel
            // 
            this._operationRecipeTitleLabel.Text = "OP Recipe";
            this._operationRecipeTitleLabel.Dock = DockStyle.Fill;
            this._operationRecipeTitleLabel.TextAlign = ContentAlignment.MiddleCenter;
            this._operationRecipeTitleLabel.Margin = new Padding(2);
            // 
            // _mesMessageLabel
            // 
            this._mesMessageLabel.Text = "MES Message";
            this._mesMessageLabel.TextAlign = ContentAlignment.MiddleLeft;
            this._mesMessageLabel.Dock = DockStyle.Fill;
            this._mesMessageLabel.Font = new Font("Arial", 8F, FontStyle.Bold, GraphicsUnit.Point, 0);
            this._mesMessageLabel.ForeColor = Color.Lime;
            this._mesMessageLabel.BackColor = Color.Black;
            this._mesMessageLabel.TabStop = false;
            this._mesMessageLabel.Margin = new Padding(2);
            // 
            // _systemMessageLabel
            // 
            this._systemMessageLabel.Text = "System Message";
            this._systemMessageLabel.TextAlign = ContentAlignment.MiddleLeft;
            this._systemMessageLabel.Dock = DockStyle.Fill;
            this._systemMessageLabel.Font = new Font("Arial", 8F, FontStyle.Bold, GraphicsUnit.Point, 0);
            this._systemMessageLabel.ForeColor = Color.Lime;
            this._systemMessageLabel.BackColor = Color.Black;
            this._systemMessageLabel.TabStop = false;
            this._systemMessageLabel.Margin = new Padding(2);
            // 
            // _operationRecipeLabel
            // 
            this._operationRecipeLabel.Text = "Operation Recipe";
            this._operationRecipeLabel.TextAlign = ContentAlignment.MiddleLeft;
            this._operationRecipeLabel.Dock = DockStyle.Fill;
            this._operationRecipeLabel.Font = new Font("Arial", 8F, FontStyle.Bold, GraphicsUnit.Point, 0);
            this._operationRecipeLabel.ForeColor = Color.Lime;
            this._operationRecipeLabel.BackColor = Color.Black;
            this._operationRecipeLabel.TabStop = false;
            this._operationRecipeLabel.Margin = new Padding(2);
            // 
            // _AlarmClearButton
            // 
            this._AlarmClearButton.Name = "Alarm Clear";
            this._AlarmClearButton.Text = "Alarm Clear";
            this._AlarmClearButton.Dock = DockStyle.Fill;
            this._AlarmClearButton.TabStop = false;
            this._AlarmClearButton.Click += new System.EventHandler(this.Button_Click);

            // 
            // Add controls to tableLayoutContentsStatusPanel
            // 
            this.tableLayoutContentsStatusPanel.Controls.Add(this._mesMessageTitleLabel, 0, 0);
            this.tableLayoutContentsStatusPanel.Controls.Add(this._systemMessageTitleLabel, 0, 1);
            this.tableLayoutContentsStatusPanel.Controls.Add(this._operationRecipeTitleLabel, 0, 2);

            this.tableLayoutContentsStatusPanel.Controls.Add(this._mesMessageLabel, 1, 0);
            this.tableLayoutContentsStatusPanel.Controls.Add(this._systemMessageLabel, 1, 1);
            this.tableLayoutContentsStatusPanel.Controls.Add(this._operationRecipeLabel, 1, 2);

            this.tableLayoutContentsStatusPanel.Controls.Add(this._AlarmClearButton, 2, 0);
            this.tableLayoutContentsStatusPanel.SetRowSpan(this._AlarmClearButton, 3);

            // 
            // TopContentsStatusControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = Color.White;
            this.Controls.Add(this.tableLayoutContentsStatusPanel);
            this.Name = "TopContentsStatusControl";
            this.Size = new System.Drawing.Size(800, 120);
            this.ResumeLayout(false);
        }

        #endregion

        private DeleteInnerBorderTableLayoutPanel tableLayoutContentsStatusPanel;
    }
}