namespace QMC.Common
{
    partial class FormTop
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this._tableLayoutPanelFormTop = new System.Windows.Forms.TableLayoutPanel();
            this._topContentsEquipmentControl = new QMC.Common.TopContentsEquipmentControl();
            this._topContentsStatusControl = new QMC.Common.TopContentsStatusControl();
            this._topContentsLoginModeControl = new QMC.Common.TopContentsLoginModeControl();
            this._topContentsIOStatusControl = new QMC.Common.TopContentsIOStatusControl();
            this.SuspendLayout();
            // 
            // _tableLayoutPanelFormTop
            // 
            this._tableLayoutPanelFormTop.ColumnCount = 4;
            this._tableLayoutPanelFormTop.ColumnStyles.Clear();
            // 화면 예시 비율: 18% / 58% / 12% / 12%
            this._tableLayoutPanelFormTop.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 18F));
            this._tableLayoutPanelFormTop.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 58F));
            this._tableLayoutPanelFormTop.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 12F));
            this._tableLayoutPanelFormTop.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 12F));
            this._tableLayoutPanelFormTop.RowCount = 1;
            this._tableLayoutPanelFormTop.RowStyles.Clear();
            this._tableLayoutPanelFormTop.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this._tableLayoutPanelFormTop.Dock = System.Windows.Forms.DockStyle.Fill;
            this._tableLayoutPanelFormTop.Margin = new System.Windows.Forms.Padding(0);
            this._tableLayoutPanelFormTop.Padding = new System.Windows.Forms.Padding(0);
            this._tableLayoutPanelFormTop.BackColor = System.Drawing.Color.White;

            // 
            // _topContentsEquipmentControl
            // 
            this._topContentsEquipmentControl.Dock = System.Windows.Forms.DockStyle.Fill;
            // 
            // _topContentsStatusControl
            // 
            this._topContentsStatusControl.Dock = System.Windows.Forms.DockStyle.Fill;
            // 
            // _topContentsLoginModeControl
            // 
            this._topContentsLoginModeControl.Dock = System.Windows.Forms.DockStyle.Fill;
            // 
            // _topContentsIOStatusControl
            // 
            this._topContentsIOStatusControl.Dock = System.Windows.Forms.DockStyle.Fill;

            // 테이블 레이아웃에 추가 (한 줄로 배치)
            this._tableLayoutPanelFormTop.Controls.Add(this._topContentsEquipmentControl, 0, 0);
            this._tableLayoutPanelFormTop.Controls.Add(this._topContentsStatusControl, 1, 0);
            this._tableLayoutPanelFormTop.Controls.Add(this._topContentsLoginModeControl, 2, 0);
            this._tableLayoutPanelFormTop.Controls.Add(this._topContentsIOStatusControl, 3, 0);

            // 
            // FormTop
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.BackColor = System.Drawing.Color.White;
            // 디자이너 미리보기 크기
            this.ClientSize = new System.Drawing.Size(1560, 150);
            this.Controls.Add(this._tableLayoutPanelFormTop);
            this.Name = "FormTop";
            this.Text = "FormTop";
            this.ResumeLayout(false);
        }

        #endregion

        //private System.Windows.Forms.TableLayoutPanel _tableLayoutPanelFormTop;
        //private QMC.Common.TopContentsEquipmentControl _topContentsEquipmentControl;
        //private QMC.Common.TopContentsStatusControl _topContentsStatusControl;
        //private QMC.Common.TopContentsLoginModeControl _topContentsLoginModeControl;
        //private QMC.Common.TopContentsIOStatusControl _topContentsIOStatusControl;
    }
}