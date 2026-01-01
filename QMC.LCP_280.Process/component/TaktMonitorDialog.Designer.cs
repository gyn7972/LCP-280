namespace QMC.LCP_280.Process.Component
{
    partial class TaktMonitorDialog
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Label lblLatest;
        private System.Windows.Forms.Label lblAvg;
        private System.Windows.Forms.Label lblMin;
        private System.Windows.Forms.Label lblMax;
        private System.Windows.Forms.Label lblCount;
        private System.Windows.Forms.DataGridView grid;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.Button btnClose;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.lblLatest = new System.Windows.Forms.Label();
            this.lblAvg = new System.Windows.Forms.Label();
            this.lblMin = new System.Windows.Forms.Label();
            this.lblMax = new System.Windows.Forms.Label();
            this.lblCount = new System.Windows.Forms.Label();
            this.grid = new System.Windows.Forms.DataGridView();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
            this.SuspendLayout();
            // 
            // lblLatest
            // 
            this.lblLatest.AutoSize = true;
            this.lblLatest.Location = new System.Drawing.Point(12, 12);
            this.lblLatest.Name = "lblLatest";
            this.lblLatest.Size = new System.Drawing.Size(56, 12);
            this.lblLatest.TabIndex = 0;
            this.lblLatest.Text = "Latest: -";
            // 
            // lblAvg
            // 
            this.lblAvg.AutoSize = true;
            this.lblAvg.Location = new System.Drawing.Point(12, 34);
            this.lblAvg.Name = "lblAvg";
            this.lblAvg.Size = new System.Drawing.Size(43, 12);
            this.lblAvg.TabIndex = 1;
            this.lblAvg.Text = "Avg: -";
            // 
            // lblMin
            // 
            this.lblMin.AutoSize = true;
            this.lblMin.Location = new System.Drawing.Point(12, 56);
            this.lblMin.Name = "lblMin";
            this.lblMin.Size = new System.Drawing.Size(41, 12);
            this.lblMin.TabIndex = 2;
            this.lblMin.Text = "Min: -";
            // 
            // lblMax
            // 
            this.lblMax.AutoSize = true;
            this.lblMax.Location = new System.Drawing.Point(12, 78);
            this.lblMax.Name = "lblMax";
            this.lblMax.Size = new System.Drawing.Size(43, 12);
            this.lblMax.TabIndex = 3;
            this.lblMax.Text = "Max: -";
            // 
            // lblCount
            // 
            this.lblCount.AutoSize = true;
            this.lblCount.Location = new System.Drawing.Point(220, 12);
            this.lblCount.Name = "lblCount";
            this.lblCount.Size = new System.Drawing.Size(54, 12);
            this.lblCount.TabIndex = 4;
            this.lblCount.Text = "Count: 0";
            // 
            // grid
            // 
            this.grid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.grid.Location = new System.Drawing.Point(12, 105);
            this.grid.Name = "grid";
            this.grid.RowTemplate.Height = 23;
            this.grid.Size = new System.Drawing.Size(560, 260);
            this.grid.TabIndex = 5;
            // 
            // btnRefresh
            // 
            this.btnRefresh.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRefresh.Location = new System.Drawing.Point(416, 12);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(75, 23);
            this.btnRefresh.TabIndex = 6;
            this.btnRefresh.Text = "Refresh";
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.Location = new System.Drawing.Point(497, 12);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(75, 23);
            this.btnClose.TabIndex = 7;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // TaktMonitorDialog
            // 
            this.ClientSize = new System.Drawing.Size(584, 377);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnRefresh);
            this.Controls.Add(this.grid);
            this.Controls.Add(this.lblCount);
            this.Controls.Add(this.lblMax);
            this.Controls.Add(this.lblMin);
            this.Controls.Add(this.lblAvg);
            this.Controls.Add(this.lblLatest);
            this.Name = "TaktMonitorDialog";
            this.Text = "Takt Monitor";
            ((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}