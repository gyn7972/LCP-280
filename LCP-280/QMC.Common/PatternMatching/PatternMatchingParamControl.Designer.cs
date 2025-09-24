namespace QMC.Common
{
    partial class PatternMatchingParamControl
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 구성 요소 디자이너에서 생성한 코드
        private void InitializeComponent()
        {
            this.baseGroupBoxPatternMatching = new QMC.Common.SimpleGroupBox();
            this.baseButtonTrain = new System.Windows.Forms.Button();
            this.baseListBoxTrainList = new System.Windows.Forms.ListBox();
            this.pictureBoxTrainImage = new QMC.Common.SimpleTrainPictureBox();
            this.baseButtonDown = new System.Windows.Forms.Button();
            this.baseButtonUp = new System.Windows.Forms.Button();
            this.baseButtonClear = new System.Windows.Forms.Button();
            this.baseButtonRemove = new System.Windows.Forms.Button();
            this.baseButtonAdd = new System.Windows.Forms.Button();
            this.baseToggleButtonAvg = new QMC.Common.SimpleToggleButton();
            this.baseGroupBoxPatternMatching.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxTrainImage)).BeginInit();
            this.SuspendLayout();
            // 
            // baseGroupBoxPatternMatching
            // 
            this.baseGroupBoxPatternMatching.BackColor = System.Drawing.Color.Transparent;
            this.baseGroupBoxPatternMatching.Controls.Add(this.baseButtonTrain);
            this.baseGroupBoxPatternMatching.Controls.Add(this.baseListBoxTrainList);
            this.baseGroupBoxPatternMatching.Controls.Add(this.pictureBoxTrainImage);
            this.baseGroupBoxPatternMatching.Controls.Add(this.baseButtonDown);
            this.baseGroupBoxPatternMatching.Controls.Add(this.baseButtonUp);
            this.baseGroupBoxPatternMatching.Controls.Add(this.baseButtonClear);
            this.baseGroupBoxPatternMatching.Controls.Add(this.baseButtonRemove);
            this.baseGroupBoxPatternMatching.Controls.Add(this.baseButtonAdd);
            this.baseGroupBoxPatternMatching.Controls.Add(this.baseToggleButtonAvg);
            this.baseGroupBoxPatternMatching.ForeColor = System.Drawing.Color.Black;
            this.baseGroupBoxPatternMatching.Location = new System.Drawing.Point(0, 0);
            this.baseGroupBoxPatternMatching.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.baseGroupBoxPatternMatching.Name = "baseGroupBoxPatternMatching";
            this.baseGroupBoxPatternMatching.Padding = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.baseGroupBoxPatternMatching.Radious = 0;
            this.baseGroupBoxPatternMatching.Size = new System.Drawing.Size(537, 344);
            this.baseGroupBoxPatternMatching.TabIndex = 1;
            this.baseGroupBoxPatternMatching.TabStop = false;
            this.baseGroupBoxPatternMatching.Text = "PatternMatching";
            this.baseGroupBoxPatternMatching.TitleBackColor = System.Drawing.Color.SteelBlue;
            this.baseGroupBoxPatternMatching.TitleFontSize = 9F;
            this.baseGroupBoxPatternMatching.UseExpand = false;
            // 
            // baseButtonTrain
            // 
            this.baseButtonTrain.Location = new System.Drawing.Point(382, 289);
            this.baseButtonTrain.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.baseButtonTrain.Name = "baseButtonTrain";
            this.baseButtonTrain.Size = new System.Drawing.Size(105, 39);
            this.baseButtonTrain.TabIndex = 11;
            this.baseButtonTrain.Text = "Train";
            this.baseButtonTrain.UseVisualStyleBackColor = true;
            this.baseButtonTrain.Click += new System.EventHandler(this.baseButtonTrain_Click);
            // 
            // baseListBoxTrainList
            // 
            this.baseListBoxTrainList.FormattingEnabled = true;
            this.baseListBoxTrainList.ItemHeight = 15;
            this.baseListBoxTrainList.Location = new System.Drawing.Point(270, 51);
            this.baseListBoxTrainList.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.baseListBoxTrainList.Name = "baseListBoxTrainList";
            this.baseListBoxTrainList.Size = new System.Drawing.Size(147, 229);
            this.baseListBoxTrainList.TabIndex = 10;
            this.baseListBoxTrainList.SelectedIndexChanged += new System.EventHandler(this.baseListBoxTrainList_SelectedIndexChanged);
            // 
            // pictureBoxTrainImage
            // 
            this.pictureBoxTrainImage.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBoxTrainImage.Location = new System.Drawing.Point(15, 51);
            this.pictureBoxTrainImage.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.pictureBoxTrainImage.Name = "pictureBoxTrainImage";
            this.pictureBoxTrainImage.Size = new System.Drawing.Size(242, 274);
            this.pictureBoxTrainImage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBoxTrainImage.TabIndex = 9;
            this.pictureBoxTrainImage.TabStop = false;
            this.pictureBoxTrainImage.ImageChanged += new QMC.Common.ImageChangedEventHandler(this.pictureBoxTrainImage_ImageChanged);
            // 
            // baseButtonDown
            // 
            this.baseButtonDown.Location = new System.Drawing.Point(424, 240);
            this.baseButtonDown.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.baseButtonDown.Name = "baseButtonDown";
            this.baseButtonDown.Size = new System.Drawing.Size(105, 39);
            this.baseButtonDown.TabIndex = 7;
            this.baseButtonDown.Text = "Down";
            this.baseButtonDown.UseVisualStyleBackColor = true;
            this.baseButtonDown.Click += new System.EventHandler(this.baseButtonDown_Click);
            // 
            // baseButtonUp
            // 
            this.baseButtonUp.Location = new System.Drawing.Point(424, 192);
            this.baseButtonUp.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.baseButtonUp.Name = "baseButtonUp";
            this.baseButtonUp.Size = new System.Drawing.Size(105, 39);
            this.baseButtonUp.TabIndex = 6;
            this.baseButtonUp.Text = "Up";
            this.baseButtonUp.UseVisualStyleBackColor = true;
            this.baseButtonUp.Click += new System.EventHandler(this.baseButtonUp_Click);
            // 
            // baseButtonClear
            // 
            this.baseButtonClear.Location = new System.Drawing.Point(424, 145);
            this.baseButtonClear.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.baseButtonClear.Name = "baseButtonClear";
            this.baseButtonClear.Size = new System.Drawing.Size(105, 39);
            this.baseButtonClear.TabIndex = 5;
            this.baseButtonClear.Text = "Clear";
            this.baseButtonClear.UseVisualStyleBackColor = true;
            this.baseButtonClear.Click += new System.EventHandler(this.baseButtonClear_Click);
            // 
            // baseButtonRemove
            // 
            this.baseButtonRemove.Location = new System.Drawing.Point(424, 98);
            this.baseButtonRemove.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.baseButtonRemove.Name = "baseButtonRemove";
            this.baseButtonRemove.Size = new System.Drawing.Size(105, 39);
            this.baseButtonRemove.TabIndex = 4;
            this.baseButtonRemove.Text = "Remove";
            this.baseButtonRemove.UseVisualStyleBackColor = true;
            this.baseButtonRemove.Click += new System.EventHandler(this.baseButtonRemove_Click);
            // 
            // baseButtonAdd
            // 
            this.baseButtonAdd.Location = new System.Drawing.Point(424, 50);
            this.baseButtonAdd.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.baseButtonAdd.Name = "baseButtonAdd";
            this.baseButtonAdd.Size = new System.Drawing.Size(105, 39);
            this.baseButtonAdd.TabIndex = 3;
            this.baseButtonAdd.Text = "Add";
            this.baseButtonAdd.UseVisualStyleBackColor = true;
            this.baseButtonAdd.Click += new System.EventHandler(this.baseButtonAdd_Click);
            // 
            // baseToggleButtonAvg
            // 
            this.baseToggleButtonAvg.Location = new System.Drawing.Point(270, 289);
            this.baseToggleButtonAvg.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.baseToggleButtonAvg.Name = "baseToggleButtonAvg";
            this.baseToggleButtonAvg.Size = new System.Drawing.Size(105, 39);
            this.baseToggleButtonAvg.TabIndex = 1;
            this.baseToggleButtonAvg.Text = "Avg";
            this.baseToggleButtonAvg.UseVisualStyleBackColor = true;
            this.baseToggleButtonAvg.Click += new System.EventHandler(this.baseToggleButtonAvg_Click);
            // 
            // PatternMatchingParamControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(120F, 120F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.Controls.Add(this.baseGroupBoxPatternMatching);
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "PatternMatchingParamControl";
            this.Size = new System.Drawing.Size(537, 350);
            this.baseGroupBoxPatternMatching.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxTrainImage)).EndInit();
            this.ResumeLayout(false);

        }
        #endregion

        private SimpleTrainPictureBox pictureBoxTrainImage;
        private SimpleGroupBox baseGroupBoxPatternMatching;
        private System.Windows.Forms.Button baseButtonTrain;
        private System.Windows.Forms.ListBox baseListBoxTrainList;
        private System.Windows.Forms.Button baseButtonDown;
        private System.Windows.Forms.Button baseButtonUp;
        private System.Windows.Forms.Button baseButtonClear;
        private System.Windows.Forms.Button baseButtonRemove;
        private System.Windows.Forms.Button baseButtonAdd;
        private SimpleToggleButton baseToggleButtonAvg;
    }
}
