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
            this.tlpMain = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tlpMain.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblChartDataTitle
            // 
            this.lblChartDataTitle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblChartDataTitle.AutoSize = true;
            this.lblChartDataTitle.Font = new System.Drawing.Font("맑은 고딕", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lblChartDataTitle.Location = new System.Drawing.Point(155, 3);
            this.lblChartDataTitle.Margin = new System.Windows.Forms.Padding(3);
            this.lblChartDataTitle.Name = "lblChartDataTitle";
            this.lblChartDataTitle.Size = new System.Drawing.Size(49, 27);
            this.lblChartDataTitle.TabIndex = 0;
            this.lblChartDataTitle.Text = "Data :";
            // 
            // lblChartDataValue
            // 
            this.lblChartDataValue.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblChartDataValue.BackColor = System.Drawing.Color.Black;
            this.lblChartDataValue.Font = new System.Drawing.Font("맑은 고딕", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lblChartDataValue.ForeColor = System.Drawing.Color.Lime;
            this.lblChartDataValue.Location = new System.Drawing.Point(210, 3);
            this.lblChartDataValue.Margin = new System.Windows.Forms.Padding(3);
            this.lblChartDataValue.Name = "lblChartDataValue";
            this.lblChartDataValue.Size = new System.Drawing.Size(202, 27);
            this.lblChartDataValue.TabIndex = 1;
            this.lblChartDataValue.Text = "N/A";
            this.lblChartDataValue.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // spectrumViewer
            // 
            this.spectrumViewer.BackColor = System.Drawing.Color.White;
            this.spectrumViewer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.spectrumViewer.Location = new System.Drawing.Point(3, 42);
            this.spectrumViewer.Name = "spectrumViewer";
            this.spectrumViewer.Size = new System.Drawing.Size(415, 325);
            this.spectrumViewer.TabIndex = 4;
            // 
            // tlpMain
            // 
            this.tlpMain.ColumnCount = 1;
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.Controls.Add(this.spectrumViewer, 0, 1);
            this.tlpMain.Controls.Add(this.tableLayoutPanel1, 0, 0);
            this.tlpMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpMain.Location = new System.Drawing.Point(0, 0);
            this.tlpMain.Name = "tlpMain";
            this.tlpMain.RowCount = 2;
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10.74553F));
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 89.25448F));
            this.tlpMain.Size = new System.Drawing.Size(421, 370);
            this.tlpMain.TabIndex = 19;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.lblChartDataTitle, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.lblChartDataValue, 1, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(415, 33);
            this.tableLayoutPanel1.TabIndex = 5;
            // 
            // MeasurementControl
            // 
            this.Controls.Add(this.tlpMain);
            this.Name = "MeasurementControl";
            this.Size = new System.Drawing.Size(421, 370);
            this.tlpMain.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        private System.Windows.Forms.TableLayoutPanel tlpMain;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
    }
}
