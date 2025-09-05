namespace QMC.Common
{
    partial class MultiPatternMatchingParameterControl
    {
        /// <summary> 
        /// 필수 디자이너 변수입니다.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 사용 중인 모든 리소스를 정리합니다.
        /// </summary>
        /// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 구성 요소 디자이너에서 생성한 코드

        /// <summary> 
        /// 디자이너 지원에 필요한 메서드입니다. 
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마세요.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.baseGroupBoxMultiTrainImage = new QMC.Common.SimpleGroupBox();
            this.pictureBoxMultiTraimImage = new QMC.Common.SimpleTrainPictureBox();
            this.baseListBoxTrainList = new System.Windows.Forms.ListBox();
            this.baseButtonDown = new System.Windows.Forms.Button();
            this.baseButtonUp = new System.Windows.Forms.Button();
            this.baseButtonClear = new System.Windows.Forms.Button();
            this.baseButtonRemove = new System.Windows.Forms.Button();
            this.baseButtonAdd = new System.Windows.Forms.Button();
            this.baseGroupBoxMutiParameter = new QMC.Common.SimpleGroupBox();
            this.ToggleButtonUseMaskImage = new QMC.Common.SimpleToggleButton();
            this.checkBoxDupCheck = new System.Windows.Forms.CheckBox();
            this.LabelMinScore = new System.Windows.Forms.Label();
            this.LabelMaxInstance = new System.Windows.Forms.Label();
            this.LabelTolerance = new System.Windows.Forms.Label();
            this.TextMinScore = new System.Windows.Forms.TextBox();
            this.TextMaxInstance = new System.Windows.Forms.TextBox();
            this.TextBoxTolerance = new System.Windows.Forms.TextBox();
            this.baseGroupBoxMultiTrainImage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxMultiTraimImage)).BeginInit();
            this.baseGroupBoxMutiParameter.SuspendLayout();
            this.SuspendLayout();
            // 
            // baseGroupBoxMultiTrainImage
            // 
            this.baseGroupBoxMultiTrainImage.Controls.Add(this.pictureBoxMultiTraimImage);
            this.baseGroupBoxMultiTrainImage.Controls.Add(this.baseListBoxTrainList);
            this.baseGroupBoxMultiTrainImage.Controls.Add(this.baseButtonDown);
            this.baseGroupBoxMultiTrainImage.Controls.Add(this.baseButtonUp);
            this.baseGroupBoxMultiTrainImage.Controls.Add(this.baseButtonClear);
            this.baseGroupBoxMultiTrainImage.Controls.Add(this.baseButtonRemove);
            this.baseGroupBoxMultiTrainImage.Controls.Add(this.baseButtonAdd);
            this.baseGroupBoxMultiTrainImage.ForeColor = System.Drawing.Color.White;
            this.baseGroupBoxMultiTrainImage.Location = new System.Drawing.Point(1, -1);
            this.baseGroupBoxMultiTrainImage.Name = "baseGroupBoxMultiTrainImage";
            this.baseGroupBoxMultiTrainImage.Size = new System.Drawing.Size(509, 256);
            this.baseGroupBoxMultiTrainImage.TabIndex = 0;
            this.baseGroupBoxMultiTrainImage.TabStop = false;
            this.baseGroupBoxMultiTrainImage.Text = "MultiTrainImage";
            // 
            // pictureBoxMultiTraimImage
            // 
            this.pictureBoxMultiTraimImage.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.pictureBoxMultiTraimImage.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBoxMultiTraimImage.Location = new System.Drawing.Point(6, 20);
            this.pictureBoxMultiTraimImage.Name = "pictureBoxMultiTraimImage";
            this.pictureBoxMultiTraimImage.Size = new System.Drawing.Size(315, 223);
            this.pictureBoxMultiTraimImage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBoxMultiTraimImage.TabIndex = 19;
            this.pictureBoxMultiTraimImage.TabStop = false;
            // 
            // baseListBoxTrainList
            // 
            this.baseListBoxTrainList.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(200)))), ((int)(((byte)(200)))));
            this.baseListBoxTrainList.Font = new System.Drawing.Font("돋움", 15F);
            this.baseListBoxTrainList.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(79)))), ((int)(((byte)(79)))), ((int)(((byte)(79)))));
            this.baseListBoxTrainList.FormattingEnabled = true;
            this.baseListBoxTrainList.ItemHeight = 20;
            this.baseListBoxTrainList.Location = new System.Drawing.Point(327, 20);
            this.baseListBoxTrainList.Name = "baseListBoxTrainList";
            this.baseListBoxTrainList.Size = new System.Drawing.Size(95, 224);
            this.baseListBoxTrainList.TabIndex = 18;
            this.baseListBoxTrainList.SelectedIndexChanged += new System.EventHandler(this.baseListBoxTrainList_SelectedIndexChanged);
            // 
            // baseButtonDown
            // 
            this.baseButtonDown.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(78)))), ((int)(((byte)(78)))), ((int)(((byte)(78)))));
            this.baseButtonDown.FlatAppearance.BorderSize = 0;
            this.baseButtonDown.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.baseButtonDown.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.baseButtonDown.Location = new System.Drawing.Point(428, 168);
            this.baseButtonDown.Name = "baseButtonDown";
            this.baseButtonDown.Size = new System.Drawing.Size(75, 31);
            this.baseButtonDown.TabIndex = 17;
            this.baseButtonDown.Text = "Down";
            this.baseButtonDown.UseVisualStyleBackColor = false;
            this.baseButtonDown.Click += new System.EventHandler(this.baseButtonDown_Click);
            // 
            // baseButtonUp
            // 
            this.baseButtonUp.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(78)))), ((int)(((byte)(78)))), ((int)(((byte)(78)))));
            this.baseButtonUp.FlatAppearance.BorderSize = 0;
            this.baseButtonUp.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.baseButtonUp.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.baseButtonUp.Location = new System.Drawing.Point(428, 131);
            this.baseButtonUp.Name = "baseButtonUp";
            this.baseButtonUp.Size = new System.Drawing.Size(75, 31);
            this.baseButtonUp.TabIndex = 16;
            this.baseButtonUp.Text = "Up";
            this.baseButtonUp.UseVisualStyleBackColor = false;
            this.baseButtonUp.Click += new System.EventHandler(this.baseButtonUp_Click);
            // 
            // baseButtonClear
            // 
            this.baseButtonClear.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(78)))), ((int)(((byte)(78)))), ((int)(((byte)(78)))));
            this.baseButtonClear.FlatAppearance.BorderSize = 0;
            this.baseButtonClear.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.baseButtonClear.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.baseButtonClear.Location = new System.Drawing.Point(428, 94);
            this.baseButtonClear.Name = "baseButtonClear";
            this.baseButtonClear.Size = new System.Drawing.Size(75, 31);
            this.baseButtonClear.TabIndex = 15;
            this.baseButtonClear.Text = "Clear";
            this.baseButtonClear.UseVisualStyleBackColor = false;
            this.baseButtonClear.Click += new System.EventHandler(this.baseButtonClear_Click);
            // 
            // baseButtonRemove
            // 
            this.baseButtonRemove.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(78)))), ((int)(((byte)(78)))), ((int)(((byte)(78)))));
            this.baseButtonRemove.FlatAppearance.BorderSize = 0;
            this.baseButtonRemove.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.baseButtonRemove.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.baseButtonRemove.Location = new System.Drawing.Point(428, 57);
            this.baseButtonRemove.Name = "baseButtonRemove";
            this.baseButtonRemove.Size = new System.Drawing.Size(75, 31);
            this.baseButtonRemove.TabIndex = 14;
            this.baseButtonRemove.Text = "Remove";
            this.baseButtonRemove.UseVisualStyleBackColor = false;
            this.baseButtonRemove.Click += new System.EventHandler(this.baseButtonRemove_Click);
            // 
            // baseButtonAdd
            // 
            this.baseButtonAdd.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(78)))), ((int)(((byte)(78)))), ((int)(((byte)(78)))));
            this.baseButtonAdd.FlatAppearance.BorderSize = 0;
            this.baseButtonAdd.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.baseButtonAdd.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.baseButtonAdd.Location = new System.Drawing.Point(428, 20);
            this.baseButtonAdd.Name = "baseButtonAdd";
            this.baseButtonAdd.Size = new System.Drawing.Size(75, 31);
            this.baseButtonAdd.TabIndex = 13;
            this.baseButtonAdd.Text = "Add";
            this.baseButtonAdd.UseVisualStyleBackColor = false;
            this.baseButtonAdd.Click += new System.EventHandler(this.baseButtonAdd_Click);
            // 
            // baseGroupBoxMutiParameter
            // 
            this.baseGroupBoxMutiParameter.Controls.Add(this.ToggleButtonUseMaskImage);
            this.baseGroupBoxMutiParameter.Controls.Add(this.checkBoxDupCheck);
            this.baseGroupBoxMutiParameter.Controls.Add(this.LabelMinScore);
            this.baseGroupBoxMutiParameter.Controls.Add(this.LabelMaxInstance);
            this.baseGroupBoxMutiParameter.Controls.Add(this.LabelTolerance);
            this.baseGroupBoxMutiParameter.Controls.Add(this.TextMinScore);
            this.baseGroupBoxMutiParameter.Controls.Add(this.TextMaxInstance);
            this.baseGroupBoxMutiParameter.Controls.Add(this.TextBoxTolerance);
            this.baseGroupBoxMutiParameter.ForeColor = System.Drawing.Color.White;
            this.baseGroupBoxMutiParameter.Location = new System.Drawing.Point(5, 261);
            this.baseGroupBoxMutiParameter.Name = "baseGroupBoxMutiParameter";
            this.baseGroupBoxMutiParameter.Size = new System.Drawing.Size(315, 183);
            this.baseGroupBoxMutiParameter.TabIndex = 1;
            this.baseGroupBoxMutiParameter.TabStop = false;
            this.baseGroupBoxMutiParameter.Text = "Multi Pattern Matching Parameter";
            // 
            // ToggleButtonUseMaskImage
            // 
            this.ToggleButtonUseMaskImage.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(78)))), ((int)(((byte)(78)))), ((int)(((byte)(78)))));
            this.ToggleButtonUseMaskImage.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ToggleButtonUseMaskImage.Font = new System.Drawing.Font("굴림", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.ToggleButtonUseMaskImage.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.ToggleButtonUseMaskImage.Location = new System.Drawing.Point(203, 144);
            this.ToggleButtonUseMaskImage.Name = "ToggleButtonUseMaskImage";
            this.ToggleButtonUseMaskImage.Size = new System.Drawing.Size(100, 30);
            this.ToggleButtonUseMaskImage.TabIndex = 15;
            this.ToggleButtonUseMaskImage.Text = "Use MaskImage";
            this.ToggleButtonUseMaskImage.UseVisualStyleBackColor = false;
            this.ToggleButtonUseMaskImage.Click += new System.EventHandler(this.baseToggleButton_Click);
            // 
            // checkBoxDupCheck
            // 
            this.checkBoxDupCheck.AutoSize = true;
            this.checkBoxDupCheck.Location = new System.Drawing.Point(175, 117);
            this.checkBoxDupCheck.Name = "checkBoxDupCheck";
            this.checkBoxDupCheck.Size = new System.Drawing.Size(116, 16);
            this.checkBoxDupCheck.TabIndex = 14;
            this.checkBoxDupCheck.Text = "Duplicate Check";
            this.checkBoxDupCheck.UseVisualStyleBackColor = true;
            this.checkBoxDupCheck.CheckedChanged += new System.EventHandler(this.checkBoxDupCheck_CheckedChanged);
            // 
            // LabelMinScore
            // 
            this.LabelMinScore.Font = new System.Drawing.Font("돋움", 11F, System.Drawing.FontStyle.Bold);
            this.LabelMinScore.ForeColor = System.Drawing.Color.White;
            this.LabelMinScore.Location = new System.Drawing.Point(15, 91);
            this.LabelMinScore.Name = "LabelMinScore";
            this.LabelMinScore.Size = new System.Drawing.Size(182, 16);
            this.LabelMinScore.TabIndex = 13;
            this.LabelMinScore.Text = "Min Score :";
            this.LabelMinScore.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // LabelMaxInstance
            // 
            this.LabelMaxInstance.Font = new System.Drawing.Font("돋움", 11F, System.Drawing.FontStyle.Bold);
            this.LabelMaxInstance.ForeColor = System.Drawing.Color.White;
            this.LabelMaxInstance.Location = new System.Drawing.Point(15, 60);
            this.LabelMaxInstance.Name = "LabelMaxInstance";
            this.LabelMaxInstance.Size = new System.Drawing.Size(182, 23);
            this.LabelMaxInstance.TabIndex = 12;
            this.LabelMaxInstance.Text = "Max Instance [ea] :";
            this.LabelMaxInstance.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // LabelTolerance
            // 
            this.LabelTolerance.Font = new System.Drawing.Font("돋움", 8F, System.Drawing.FontStyle.Bold);
            this.LabelTolerance.ForeColor = System.Drawing.Color.White;
            this.LabelTolerance.Location = new System.Drawing.Point(16, 26);
            this.LabelTolerance.Name = "LabelTolerance";
            this.LabelTolerance.Size = new System.Drawing.Size(182, 23);
            this.LabelTolerance.TabIndex = 11;
            this.LabelTolerance.Text = "Angle Tolerance [Degree] :";
            this.LabelTolerance.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // TextMinScore
            // 
            this.TextMinScore.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(78)))), ((int)(((byte)(78)))), ((int)(((byte)(78)))));
            this.TextMinScore.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.TextMinScore.Font = new System.Drawing.Font("굴림", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.TextMinScore.ForeColor = System.Drawing.Color.White;
            this.TextMinScore.Location = new System.Drawing.Point(204, 94);
            this.TextMinScore.Name = "TextMinScore";
            this.TextMinScore.Size = new System.Drawing.Size(100, 17);
            this.TextMinScore.TabIndex = 10;
            this.TextMinScore.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.TextMinScore.TextChanged += new System.EventHandler(this.ChangeParametersMinScore);
            // 
            // TextMaxInstance
            // 
            this.TextMaxInstance.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(78)))), ((int)(((byte)(78)))), ((int)(((byte)(78)))));
            this.TextMaxInstance.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.TextMaxInstance.Font = new System.Drawing.Font("굴림", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.TextMaxInstance.ForeColor = System.Drawing.Color.White;
            this.TextMaxInstance.Location = new System.Drawing.Point(204, 63);
            this.TextMaxInstance.Name = "TextMaxInstance";
            this.TextMaxInstance.Size = new System.Drawing.Size(100, 17);
            this.TextMaxInstance.TabIndex = 9;
            this.TextMaxInstance.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.TextMaxInstance.TextChanged += new System.EventHandler(this.ChangeParametersMaxInstnce);
            // 
            // TextBoxTolerance
            // 
            this.TextBoxTolerance.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(78)))), ((int)(((byte)(78)))), ((int)(((byte)(78)))));
            this.TextBoxTolerance.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.TextBoxTolerance.Font = new System.Drawing.Font("굴림", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.TextBoxTolerance.ForeColor = System.Drawing.Color.White;
            this.TextBoxTolerance.Location = new System.Drawing.Point(204, 32);
            this.TextBoxTolerance.Name = "TextBoxTolerance";
            this.TextBoxTolerance.Size = new System.Drawing.Size(100, 17);
            this.TextBoxTolerance.TabIndex = 8;
            this.TextBoxTolerance.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.TextBoxTolerance.TextChanged += new System.EventHandler(this.ChangeParametersTolerance);
            // 
            // MultiPatternMatchingParameterControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.baseGroupBoxMutiParameter);
            this.Controls.Add(this.baseGroupBoxMultiTrainImage);
            this.Name = "MultiPatternMatchingParameterControl";
            this.Size = new System.Drawing.Size(513, 449);
            this.baseGroupBoxMultiTrainImage.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxMultiTraimImage)).EndInit();
            this.baseGroupBoxMutiParameter.ResumeLayout(false);
            this.baseGroupBoxMutiParameter.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private SimpleGroupBox baseGroupBoxMultiTrainImage;
        private System.Windows.Forms.ListBox baseListBoxTrainList;
        private System.Windows.Forms.Button baseButtonDown;
        private System.Windows.Forms.Button baseButtonUp;
        private System.Windows.Forms.Button baseButtonClear;
        private System.Windows.Forms.Button baseButtonRemove;
        private SimpleGroupBox baseGroupBoxMutiParameter;
        private SimpleToggleButton ToggleButtonUseMaskImage;
        private System.Windows.Forms.CheckBox checkBoxDupCheck;
        private System.Windows.Forms.Label LabelMinScore;
        private System.Windows.Forms.Label LabelMaxInstance;
        private System.Windows.Forms.Label LabelTolerance;
        private System.Windows.Forms.TextBox TextMinScore;
        private System.Windows.Forms.TextBox TextMaxInstance;
        private System.Windows.Forms.TextBox TextBoxTolerance;
        private System.Windows.Forms.Button baseButtonAdd;
        private SimpleTrainPictureBox pictureBoxMultiTraimImage;
    }
}
