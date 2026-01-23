namespace QMC.LCP_280.Process.Unit.FormLog
{
    partial class TotalLog_Log
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.totalLogGridViewer1 = new QMC.Common.History.LogHistoryGridViewer();
            this.SuspendLayout();
            // 
            // totalLogGridViewer1
            // 
            this.totalLogGridViewer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.totalLogGridViewer1.Location = new System.Drawing.Point(10, 10);
            this.totalLogGridViewer1.Name = "totalLogGridViewer1";
            this.totalLogGridViewer1.Size = new System.Drawing.Size(1244, 760);
            this.totalLogGridViewer1.TabIndex = 1;
            // 
            // TotalLog_Log
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1264, 780);
            this.Controls.Add(this.totalLogGridViewer1);
            this.Name = "TotalLog_Log";
            this.Padding = new System.Windows.Forms.Padding(10);
            this.Text = "TotalLog_Log";
            this.ResumeLayout(false);

        }

        #endregion

        private Common.History.LogHistoryGridViewer totalLogGridViewer1;
    }
}