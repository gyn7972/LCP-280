using System.Windows.Forms;

namespace QMC.LCP_280.Process
{
    partial class Monitoring_Main
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
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
            this.groupBoxdieInputControl = new System.Windows.Forms.GroupBox();
            this.dieInputControl1 = new QMC.LCP_280.Process.Unit.FormMain.DieInputControl();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.groupBox6 = new System.Windows.Forms.GroupBox();
            this.groupBoxdieInputControl.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBoxdieInputControl
            // 
            this.groupBoxdieInputControl.Controls.Add(this.dieInputControl1);
            this.groupBoxdieInputControl.Location = new System.Drawing.Point(12, 138);
            this.groupBoxdieInputControl.Name = "groupBoxdieInputControl";
            this.groupBoxdieInputControl.Size = new System.Drawing.Size(330, 405);
            this.groupBoxdieInputControl.TabIndex = 0;
            this.groupBoxdieInputControl.TabStop = false;
            // 
            // dieInputControl1
            // 
            this.dieInputControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dieInputControl1.Location = new System.Drawing.Point(3, 21);
            this.dieInputControl1.Name = "dieInputControl1";
            this.dieInputControl1.Size = new System.Drawing.Size(324, 381);
            this.dieInputControl1.TabIndex = 0;
            // 
            // groupBox2
            // 
            this.groupBox2.Location = new System.Drawing.Point(348, 138);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(330, 405);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "groupBox2";
            // 
            // groupBox3
            // 
            this.groupBox3.Location = new System.Drawing.Point(684, 138);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(330, 405);
            this.groupBox3.TabIndex = 2;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "groupBox3";
            // 
            // groupBox4
            // 
            this.groupBox4.Location = new System.Drawing.Point(15, 549);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(330, 405);
            this.groupBox4.TabIndex = 4;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "groupBox4";
            // 
            // groupBox5
            // 
            this.groupBox5.Location = new System.Drawing.Point(348, 549);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(330, 405);
            this.groupBox5.TabIndex = 3;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "groupBox5";
            // 
            // groupBox6
            // 
            this.groupBox6.Location = new System.Drawing.Point(684, 549);
            this.groupBox6.Name = "groupBox6";
            this.groupBox6.Size = new System.Drawing.Size(330, 405);
            this.groupBox6.TabIndex = 4;
            this.groupBox6.TabStop = false;
            this.groupBox6.Text = "groupBox6";
            // 
            // Monitoring_Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(120F, 120F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(1303, 1003);
            this.Controls.Add(this.groupBox6);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox5);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBoxdieInputControl);
            this.Name = "Monitoring_Main";
            this.Text = "Monitoring";
            this.Load += new System.EventHandler(this.Monitoring_Main_Load);
            this.groupBoxdieInputControl.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private GroupBox groupBoxdieInputControl;
        private QMC.LCP_280.Process.Unit.FormMain.DieInputControl dieInputControl1;
        private GroupBox groupBox2;
        private GroupBox groupBox3;
        private GroupBox groupBox4;
        private GroupBox groupBox5;
        private GroupBox groupBox6;
    }
}