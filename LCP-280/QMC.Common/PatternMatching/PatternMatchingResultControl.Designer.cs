namespace QMC.Common
{
    partial class PatternMatchingResultControl
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        #region 디자이너 생성 코드
        private void InitializeComponent()
        {
            this.baseGroupBox1 = new QMC.Common.SimpleGroupBox();
            this.paramTextControlResultT = new QMC.Common.ParamTextControl();
            this.paramTextControlResultY = new QMC.Common.ParamTextControl();
            this.paramTextControlResultX = new QMC.Common.ParamTextControl();
            this.baseButtonSearch = new System.Windows.Forms.Button();
            this.baseGroupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // baseGroupBox1
            // 
            this.baseGroupBox1.BackColor = System.Drawing.Color.Transparent;
            this.baseGroupBox1.Controls.Add(this.paramTextControlResultT);
            this.baseGroupBox1.Controls.Add(this.paramTextControlResultY);
            this.baseGroupBox1.Controls.Add(this.paramTextControlResultX);
            this.baseGroupBox1.Controls.Add(this.baseButtonSearch);
            this.baseGroupBox1.ForeColor = System.Drawing.Color.Black;
            this.baseGroupBox1.Location = new System.Drawing.Point(0, 0);
            this.baseGroupBox1.Name = "baseGroupBox1";
            this.baseGroupBox1.Radious = 0;
            this.baseGroupBox1.Size = new System.Drawing.Size(460, 185);
            this.baseGroupBox1.TabIndex = 0;
            this.baseGroupBox1.TabStop = false;
            this.baseGroupBox1.Text = "Search";
            this.baseGroupBox1.TitleBackColor = System.Drawing.Color.SteelBlue;
            this.baseGroupBox1.TitleFontSize = 9F;
            this.baseGroupBox1.UseExpand = false;
            // 
            // paramTextControlResultT
            // 
            this.paramTextControlResultT.Location = new System.Drawing.Point(5, 98);
            this.paramTextControlResultT.Name = "paramTextControlResultT";
            this.paramTextControlResultT.Size = new System.Drawing.Size(450, 36);
            this.paramTextControlResultT.TabIndex = 3;
            this.paramTextControlResultT.TitleRatio = 50;
            // 
            // paramTextControlResultY
            // 
            this.paramTextControlResultY.Location = new System.Drawing.Point(7, 56);
            this.paramTextControlResultY.Name = "paramTextControlResultY";
            this.paramTextControlResultY.Size = new System.Drawing.Size(450, 36);
            this.paramTextControlResultY.TabIndex = 2;
            this.paramTextControlResultY.TitleRatio = 50;
            // 
            // paramTextControlResultX
            // 
            this.paramTextControlResultX.Location = new System.Drawing.Point(5, 14);
            this.paramTextControlResultX.Name = "paramTextControlResultX";
            this.paramTextControlResultX.Size = new System.Drawing.Size(450, 36);
            this.paramTextControlResultX.TabIndex = 1;
            this.paramTextControlResultX.TitleRatio = 50;
            // 
            // baseButtonSearch
            // 
            this.baseButtonSearch.Location = new System.Drawing.Point(7, 150);
            this.baseButtonSearch.Name = "baseButtonSearch";
            this.baseButtonSearch.Size = new System.Drawing.Size(450, 30);
            this.baseButtonSearch.TabIndex = 0;
            this.baseButtonSearch.Text = "Search";
            this.baseButtonSearch.UseVisualStyleBackColor = true;
            this.baseButtonSearch.Click += new System.EventHandler(this.baseButtonSearch_Click);
            // 
            // PatternMatchingResultControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.baseGroupBox1);
            this.Name = "PatternMatchingResultControl";
            this.Size = new System.Drawing.Size(460, 185);
            this.baseGroupBox1.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        #endregion

        private SimpleGroupBox baseGroupBox1;
        private ParamTextControl paramTextControlResultT;
        private ParamTextControl paramTextControlResultY;
        private ParamTextControl paramTextControlResultX;
        private System.Windows.Forms.Button baseButtonSearch;
    }
}
