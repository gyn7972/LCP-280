namespace QMC.LCP_280.Process.Unit.FormMain
{
    partial class MeasurementControl
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Label lblChartDataTitle;
        private System.Windows.Forms.Label lblChartDataValue;
        private Common.Spectrometer.CASSpectrumViewer spectrumViewer;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                updateTimer?.Stop();
                updateTimer?.Dispose();

                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.lblChartDataTitle = new System.Windows.Forms.Label();
            this.lblChartDataValue = new System.Windows.Forms.Label();
            this.spectrumViewer = new QMC.Common.Spectrometer.CASSpectrumViewer();
            this.SuspendLayout();
            // 
            // lblChartDataTitle
            // 
            this.lblChartDataTitle.AutoSize = true;
            this.lblChartDataTitle.Font = new System.Drawing.Font("맑은 고딕", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lblChartDataTitle.Location = new System.Drawing.Point(10, 5);
            this.lblChartDataTitle.Name = "lblChartDataTitle";
            this.lblChartDataTitle.Size = new System.Drawing.Size(40, 19);
            this.lblChartDataTitle.TabIndex = 0;
            this.lblChartDataTitle.Text = "Data";
            // 
            // lblChartDataValue
            // 
            this.lblChartDataValue.BackColor = System.Drawing.Color.Black;
            this.lblChartDataValue.Font = new System.Drawing.Font("맑은 고딕", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lblChartDataValue.ForeColor = System.Drawing.Color.Lime;
            this.lblChartDataValue.Location = new System.Drawing.Point(160, 5);
            this.lblChartDataValue.Name = "lblChartDataValue";
            this.lblChartDataValue.Size = new System.Drawing.Size(150, 23);
            this.lblChartDataValue.TabIndex = 1;
            this.lblChartDataValue.Text = "N/A";
            this.lblChartDataValue.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // spectrumViewer
            // 
            this.spectrumViewer.BackColor = System.Drawing.Color.LightGray;
            this.spectrumViewer.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.spectrumViewer.Location = new System.Drawing.Point(0, 70);
            this.spectrumViewer.Name = "displayView1";
            this.spectrumViewer.Size = new System.Drawing.Size(320, 300);
            this.spectrumViewer.TabIndex = 4;
            // 
            // MeasurementControl
            // 
            this.Controls.Add(this.lblChartDataTitle);
            this.Controls.Add(this.lblChartDataValue);
            this.Controls.Add(this.spectrumViewer);
            this.Name = "MeasurementControl";
            this.Size = new System.Drawing.Size(320, 370);
            this.ResumeLayout(false);
            this.PerformLayout();

        }
    }
}
