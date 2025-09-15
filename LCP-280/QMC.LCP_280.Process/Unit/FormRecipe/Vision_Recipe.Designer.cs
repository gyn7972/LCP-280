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
            this.patternMatchingControl1.Location = new System.Drawing.Point(4, 4);
            this.patternMatchingControl1.Margin = new System.Windows.Forms.Padding(0);
            this.patternMatchingControl1.MinimumSize = new System.Drawing.Size(900, 600);
            this.patternMatchingControl1.Name = "patternMatchingControl1";
            this.patternMatchingControl1.Size = new System.Drawing.Size(1251, 738);
            this.patternMatchingControl1.TabIndex = 0;
            // 
            // Vision_Recipe
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(1264, 751);
            this.Controls.Add(this.patternMatchingControl1);
            this.Name = "Vision_Recipe";
            this.Text = "Vision Recipe";
            this.ResumeLayout(false);

        }
    }
}