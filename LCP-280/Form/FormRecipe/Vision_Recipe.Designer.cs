namespace QMC.LCP_280.Process.Unit.FormRecipe
{
    partial class Vision_Recipe
    {
        private System.ComponentModel.IContainer components = null;
        private QMC.LCP_280.Process.PatternMatchingControl patternMatchingControl1;

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
            this.patternMatchingControl1 = new QMC.LCP_280.Process.PatternMatchingControl();
            this.SuspendLayout();
            // 
            // patternMatchingControl1
            // 
            this.patternMatchingControl1.BackColor = System.Drawing.Color.White;
            this.patternMatchingControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.patternMatchingControl1.Location = new System.Drawing.Point(0, 0);
            this.patternMatchingControl1.Margin = new System.Windows.Forms.Padding(0);
            this.patternMatchingControl1.Name = "patternMatchingControl1";
            this.patternMatchingControl1.Size = new System.Drawing.Size(1264, 751);
            this.patternMatchingControl1.TabIndex = 0;
            // 
            // Vision_Recipe
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(1264, 751);
            this.Controls.Add(this.patternMatchingControl1);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "Vision_Recipe";
            this.Text = "Vision Recipe";
            this.ResumeLayout(false);

        }
    }
}