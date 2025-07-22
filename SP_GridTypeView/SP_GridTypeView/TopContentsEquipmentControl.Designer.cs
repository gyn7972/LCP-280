using System.Windows.Forms;

namespace SP_GridTypeView
{
    partial class TopContentsEquipmentControl
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
            this.tableLayoutContentsEquipmentPanel = new DeleteLogoBorderTableLayoutPanel();
            this.SuspendLayout();
            // 
            // tableLayoutContentsEquipmentPanel
            // 
            this.tableLayoutContentsEquipmentPanel.AutoSize = true;
            this.tableLayoutContentsEquipmentPanel.ColumnCount = 1;
            this.tableLayoutContentsEquipmentPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutContentsEquipmentPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutContentsEquipmentPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutContentsEquipmentPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutContentsEquipmentPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutContentsEquipmentPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutContentsEquipmentPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutContentsEquipmentPanel.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutContentsEquipmentPanel.Name = "tableLayoutContentsEquipmentPanel";
            this.tableLayoutContentsEquipmentPanel.RowCount = 1;
            this.tableLayoutContentsEquipmentPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutContentsEquipmentPanel.Size = new System.Drawing.Size(461, 171);
            this.tableLayoutContentsEquipmentPanel.TabIndex = 0;
            // 
            // TopContentsEquipmentControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutContentsEquipmentPanel);
            this.Name = "TopContentsEquipmentControl";
            this.Size = new System.Drawing.Size(461, 171);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DeleteLogoBorderTableLayoutPanel tableLayoutContentsEquipmentPanel;
    }
}
