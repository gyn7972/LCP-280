namespace QMC.LCP_280.Process.Unit.FormMain
{
    partial class OutputWaferCarrierControl
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Label lblCarrierIdTitle;
        private System.Windows.Forms.Label lblWaferIdValue;
        private System.Windows.Forms.Label lblWaferCountTitle;
        private System.Windows.Forms.Label lblWaferCountValue;
        private QMC.LCP_280.Process.Unit.FormMain.DisplayView_OutputWaferCarrierControl displayView1;
        private QMC.LCP_280.Process.Component.WaferMapView waferMapView;
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.lblCarrierIdTitle = new System.Windows.Forms.Label();
            this.lblWaferIdValue = new System.Windows.Forms.Label();
            this.lblWaferCountTitle = new System.Windows.Forms.Label();
            this.lblWaferCountValue = new System.Windows.Forms.Label();
            this.displayView1 = new QMC.LCP_280.Process.Unit.FormMain.DisplayView_OutputWaferCarrierControl();
            this.waferMapView = new QMC.LCP_280.Process.Component.WaferMapView();
            this.SuspendLayout();
            // 
            // lblCarrierIdTitle
            // 
            this.lblCarrierIdTitle.AutoSize = true;
            this.lblCarrierIdTitle.Font = new System.Drawing.Font("맑은 고딕", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lblCarrierIdTitle.Location = new System.Drawing.Point(10, 5);
            this.lblCarrierIdTitle.Name = "lblCarrierIdTitle";
            this.lblCarrierIdTitle.Size = new System.Drawing.Size(79, 19);
            this.lblCarrierIdTitle.TabIndex = 0;
            this.lblCarrierIdTitle.Text = "Carrier ID:";
            // 
            // lblWaferIdValue
            // 
            this.lblWaferIdValue.BackColor = System.Drawing.Color.Black;
            this.lblWaferIdValue.Font = new System.Drawing.Font("맑은 고딕", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lblWaferIdValue.ForeColor = System.Drawing.Color.Lime;
            this.lblWaferIdValue.Location = new System.Drawing.Point(160, 5);
            this.lblWaferIdValue.Name = "lblWaferIdValue";
            this.lblWaferIdValue.Size = new System.Drawing.Size(150, 23);
            this.lblWaferIdValue.TabIndex = 1;
            this.lblWaferIdValue.Text = "N/A";
            this.lblWaferIdValue.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblWaferCountTitle
            // 
            this.lblWaferCountTitle.AutoSize = true;
            this.lblWaferCountTitle.Font = new System.Drawing.Font("맑은 고딕", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lblWaferCountTitle.Location = new System.Drawing.Point(10, 30);
            this.lblWaferCountTitle.Name = "lblWaferCountTitle";
            this.lblWaferCountTitle.Size = new System.Drawing.Size(100, 19);
            this.lblWaferCountTitle.TabIndex = 2;
            this.lblWaferCountTitle.Text = "Wafer Count:";
            // 
            // lblWaferCountValue
            // 
            this.lblWaferCountValue.BackColor = System.Drawing.Color.Black;
            this.lblWaferCountValue.Font = new System.Drawing.Font("맑은 고딕", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lblWaferCountValue.ForeColor = System.Drawing.Color.Lime;
            this.lblWaferCountValue.Location = new System.Drawing.Point(160, 30);
            this.lblWaferCountValue.Name = "lblWaferCountValue";
            this.lblWaferCountValue.Size = new System.Drawing.Size(150, 23);
            this.lblWaferCountValue.TabIndex = 3;
            this.lblWaferCountValue.Text = "0";
            this.lblWaferCountValue.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
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
            // waferMapView
            // 
            this.waferMapView.Location = new System.Drawing.Point(63, 101);
            this.waferMapView.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.waferMapView.Name = "waferMapView";
            this.waferMapView.Size = new System.Drawing.Size(195, 227);
            this.waferMapView.TabIndex = 17;
            // 
            // OutputWaferCarrierControl
            // 
            this.Controls.Add(this.waferMapView);
            this.Controls.Add(this.lblCarrierIdTitle);
            this.Controls.Add(this.lblWaferIdValue);
            this.Controls.Add(this.lblWaferCountTitle);
            this.Controls.Add(this.lblWaferCountValue);
            this.Controls.Add(this.displayView1);
            this.Name = "OutputWaferCarrierControl";
            this.Size = new System.Drawing.Size(320, 370);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        
    }
}
