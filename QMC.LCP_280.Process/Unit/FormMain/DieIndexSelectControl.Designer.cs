namespace QMC.LCP_280.Process.Unit.FormMain
{
    partial class DieIndexSelectControl
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Label lblDieId;
        private System.Windows.Forms.Label lblDieIdValue;
        private System.Windows.Forms.Label lblDieNumber;
        private System.Windows.Forms.Label lblDieNumberValue;

        private System.Windows.Forms.Panel displayPanel; // 추후에 분리 해도 될듯..?
        private System.Windows.Forms.Button btnAutoSequence;
        private System.Windows.Forms.Button btnReset;

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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DieIndexSelectControl));
            this.lblDieId = new System.Windows.Forms.Label();
            this.lblDieIdValue = new System.Windows.Forms.Label();
            this.lblDieNumber = new System.Windows.Forms.Label();
            this.lblDieNumberValue = new System.Windows.Forms.Label();
            this.displayPanel = new System.Windows.Forms.Panel();
            this.btnReset = new System.Windows.Forms.Button();
            this.btnAutoSequence = new System.Windows.Forms.Button();
            this.displayPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblDieId
            // 
            resources.ApplyResources(this.lblDieId, "lblDieId");
            this.lblDieId.Name = "lblDieId";
            // 
            // lblDieIdValue
            // 
            this.lblDieIdValue.BackColor = System.Drawing.Color.Black;
            resources.ApplyResources(this.lblDieIdValue, "lblDieIdValue");
            this.lblDieIdValue.ForeColor = System.Drawing.Color.Lime;
            this.lblDieIdValue.Name = "lblDieIdValue";
            // 
            // lblDieNumber
            // 
            resources.ApplyResources(this.lblDieNumber, "lblDieNumber");
            this.lblDieNumber.Name = "lblDieNumber";
            // 
            // lblDieNumberValue
            // 
            this.lblDieNumberValue.BackColor = System.Drawing.Color.Black;
            resources.ApplyResources(this.lblDieNumberValue, "lblDieNumberValue");
            this.lblDieNumberValue.ForeColor = System.Drawing.Color.Lime;
            this.lblDieNumberValue.Name = "lblDieNumberValue";
            // 
            // displayPanel
            // 
            this.displayPanel.BackColor = System.Drawing.Color.LightGray;
            this.displayPanel.Controls.Add(this.btnReset);
            this.displayPanel.Controls.Add(this.btnAutoSequence);
            resources.ApplyResources(this.displayPanel, "displayPanel");
            this.displayPanel.Name = "displayPanel";
            this.displayPanel.Paint += new System.Windows.Forms.PaintEventHandler(this.DisplayPanel_Paint);
            this.displayPanel.MouseClick += new System.Windows.Forms.MouseEventHandler(this.DisplayPanel_MouseClick);
            this.displayPanel.MouseMove += new System.Windows.Forms.MouseEventHandler(this.DisplayPanel_MouseMove);
            this.displayPanel.Resize += new System.EventHandler(this.DisplayPanel_Resize);
            // 
            // btnReset
            // 
            this.btnReset.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(76)))), ((int)(((byte)(60)))));
            this.btnReset.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnReset.FlatAppearance.BorderSize = 0;
            resources.ApplyResources(this.btnReset, "btnReset");
            this.btnReset.ForeColor = System.Drawing.Color.White;
            this.btnReset.Name = "btnReset";
            this.btnReset.UseVisualStyleBackColor = false;
            this.btnReset.Click += new System.EventHandler(this.BtnReset_Click);
            // 
            // btnAutoSequence
            // 
            this.btnAutoSequence.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(152)))), ((int)(((byte)(219)))));
            this.btnAutoSequence.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnAutoSequence.FlatAppearance.BorderSize = 0;
            resources.ApplyResources(this.btnAutoSequence, "btnAutoSequence");
            this.btnAutoSequence.ForeColor = System.Drawing.Color.White;
            this.btnAutoSequence.Name = "btnAutoSequence";
            this.btnAutoSequence.UseVisualStyleBackColor = false;
            this.btnAutoSequence.Click += new System.EventHandler(this.BtnAutoSequence_Click);
            // 
            // DieIndexSelectControl
            // 
            this.Controls.Add(this.lblDieId);
            this.Controls.Add(this.lblDieIdValue);
            this.Controls.Add(this.lblDieNumber);
            this.Controls.Add(this.lblDieNumberValue);
            this.Controls.Add(this.displayPanel);
            resources.ApplyResources(this, "$this");
            this.Name = "DieIndexSelectControl";
            this.displayPanel.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        // Selected 레전드의 노란색 테두리를 그리기 위한 이벤트 핸들러
        private void LegendColorSelected_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
        {
            using (System.Drawing.Pen pen = new System.Drawing.Pen(System.Drawing.Color.Yellow, 3))
            {
                e.Graphics.DrawRectangle(pen, 1, 1, 20, 20);
            }
        }

    }

}
