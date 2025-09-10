namespace QMC.LCP_280.Process.Unit
{
    partial class Instrument_Setup
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
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.sourcemeterPage1 = new QMC.LCP_280.Process.Unit.FormSetup.Page.SourcemeterPage();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.spectrometerPage1 = new QMC.LCP_280.Process.Unit.FormSetup.Page.SpectrometerPage();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Font = new System.Drawing.Font("맑은 고딕", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(1264, 752);
            this.tabControl1.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.sourcemeterPage1);
            this.tabPage1.Font = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.tabPage1.Location = new System.Drawing.Point(4, 29);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(1256, 719);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Sourcemeter";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // sourcemeterPage1
            // 
            this.sourcemeterPage1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.sourcemeterPage1.Location = new System.Drawing.Point(3, 3);
            this.sourcemeterPage1.Name = "sourcemeterPage1";
            this.sourcemeterPage1.Size = new System.Drawing.Size(1250, 713);
            this.sourcemeterPage1.TabIndex = 0;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.spectrometerPage1);
            this.tabPage2.Location = new System.Drawing.Point(4, 29);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(1256, 719);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Spectrometer";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // spectrometerPage1
            // 
            this.spectrometerPage1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.spectrometerPage1.Location = new System.Drawing.Point(3, 3);
            this.spectrometerPage1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.spectrometerPage1.Name = "spectrometerPage1";
            this.spectrometerPage1.Size = new System.Drawing.Size(1250, 713);
            this.spectrometerPage1.TabIndex = 0;
            // 
            // Instrument_Setup
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.ClientSize = new System.Drawing.Size(1264, 752);
            this.Controls.Add(this.tabControl1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "Instrument_Setup";
            this.Text = "Instrument_Setup";
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.TabPage tabPage1;
        private FormSetup.Page.SourcemeterPage sourcemeterPage1;
        private FormSetup.Page.SpectrometerPage spectrometerPage1;
    }
}