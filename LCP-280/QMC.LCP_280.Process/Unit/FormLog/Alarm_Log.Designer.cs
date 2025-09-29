namespace QMC.LCP_280.Process.Unit.FormLog
{
    partial class Alarm_Log
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
            this.alarmHistoryGridViewer1 = new QMC.Common.History.AlarmHistoryGridViewer();
            this.SuspendLayout();
            // 
            // alarmHistoryGridViewer1
            // 
            this.alarmHistoryGridViewer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.alarmHistoryGridViewer1.Location = new System.Drawing.Point(10, 10);
            this.alarmHistoryGridViewer1.Name = "alarmHistoryGridViewer1";
            this.alarmHistoryGridViewer1.Size = new System.Drawing.Size(1244, 760);
            this.alarmHistoryGridViewer1.TabIndex = 0;
            // 
            // Alarm_Log
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(1264, 780);
            this.Controls.Add(this.alarmHistoryGridViewer1);
            this.Name = "Alarm_Log";
            this.Padding = new System.Windows.Forms.Padding(10);
            this.Text = "Alarm_Log";
            this.ResumeLayout(false);

        }

        #endregion

        private Common.History.AlarmHistoryGridViewer alarmHistoryGridViewer1;
    }
}