namespace QMC.LCP_280.Process.Unit.FormMain
{
    partial class DieInputControl
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Label lblWaferIdValue;
        private System.Windows.Forms.Label lblDieCountValue;
        private System.Windows.Forms.Label lblWaferIdTitle;
        private System.Windows.Forms.Label lblDieCountTitle;
        private QMC.Common.Controls.DisplayView displayView1;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.lblWaferIdTitle = new System.Windows.Forms.Label();
            this.lblWaferIdValue = new System.Windows.Forms.Label();
            this.lblDieCountTitle = new System.Windows.Forms.Label();
            this.lblDieCountValue = new System.Windows.Forms.Label();
            this.displayView1 = new QMC.Common.Controls.DisplayView();
            this.SuspendLayout();
            // 
            // lblWaferIdTitle
            // 
            this.lblWaferIdTitle.AutoSize = true;
            this.lblWaferIdTitle.Font = new System.Drawing.Font("맑은 고딕", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lblWaferIdTitle.Location = new System.Drawing.Point(10, 10);
            this.lblWaferIdTitle.Name = "lblWaferIdTitle";
            this.lblWaferIdTitle.Size = new System.Drawing.Size(86, 23);
            this.lblWaferIdTitle.TabIndex = 0;
            this.lblWaferIdTitle.Text = "Wafer ID:";
            // 
            // lblWaferIdValue
            // 
            this.lblWaferIdValue.BackColor = System.Drawing.Color.Black;
            this.lblWaferIdValue.Font = new System.Drawing.Font("맑은 고딕", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lblWaferIdValue.ForeColor = System.Drawing.Color.Lime;
            this.lblWaferIdValue.Location = new System.Drawing.Point(160, 10);
            this.lblWaferIdValue.Name = "lblWaferIdValue";
            this.lblWaferIdValue.Size = new System.Drawing.Size(150, 23);
            this.lblWaferIdValue.TabIndex = 1;
            this.lblWaferIdValue.Text = "N/A";
            this.lblWaferIdValue.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblDieCountTitle
            // 
            this.lblDieCountTitle.AutoSize = true;
            this.lblDieCountTitle.Font = new System.Drawing.Font("맑은 고딕", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lblDieCountTitle.Location = new System.Drawing.Point(10, 35);
            this.lblDieCountTitle.Name = "lblDieCountTitle";
            this.lblDieCountTitle.Size = new System.Drawing.Size(96, 23);
            this.lblDieCountTitle.TabIndex = 2;
            this.lblDieCountTitle.Text = "Die Count:";
            // 
            // lblDieCountValue
            // 
            this.lblDieCountValue.BackColor = System.Drawing.Color.Black;
            this.lblDieCountValue.Font = new System.Drawing.Font("맑은 고딕", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lblDieCountValue.ForeColor = System.Drawing.Color.Lime;
            this.lblDieCountValue.Location = new System.Drawing.Point(160, 35);
            this.lblDieCountValue.Name = "lblDieCountValue";
            this.lblDieCountValue.Size = new System.Drawing.Size(150, 23);
            this.lblDieCountValue.TabIndex = 3;
            this.lblDieCountValue.Text = "0";
            this.lblDieCountValue.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // displayView1
            // 
            this.displayView1.BackColor = System.Drawing.Color.LightGray;
            this.displayView1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.displayView1.Location = new System.Drawing.Point(0, 70);
            this.displayView1.Name = "displayView1";
            this.displayView1.Size = new System.Drawing.Size(320, 300);
            this.displayView1.TabIndex = 4;
            // 
            // DieInputControl
            // 
            this.Controls.Add(this.lblWaferIdTitle);
            this.Controls.Add(this.lblWaferIdValue);
            this.Controls.Add(this.lblDieCountTitle);
            this.Controls.Add(this.lblDieCountValue);
            this.Controls.Add(this.displayView1);
            this.Name = "DieInputControl";
            this.Size = new System.Drawing.Size(320, 370);
            this.ResumeLayout(false);
            this.PerformLayout();

        }
    }
}
