namespace QMC.LCP_280.Process
{
    partial class TestGyn
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
            this.components = new System.ComponentModel.Container();
            this.comboUnit = new System.Windows.Forms.ComboBox();
            this.comboAxis = new System.Windows.Forms.ComboBox();
            this.numDist = new System.Windows.Forms.NumericUpDown();
            this.labelUnit = new System.Windows.Forms.Label();
            this.labelAxis = new System.Windows.Forms.Label();
            this.labelDist = new System.Windows.Forms.Label();
            this.button_TestGyn_Test = new System.Windows.Forms.Button();
            this.button_TestGyn_Alarm = new System.Windows.Forms.Button();
            this.pictureBox_ImageView_Test = new QMC.Common.Vision.VisionImageViewer();
            ((System.ComponentModel.ISupportInitialize)(this.numDist)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_ImageView_Test)).BeginInit();
            this.SuspendLayout();
            // 
            // comboUnit
            // 
            this.comboUnit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboUnit.FormattingEnabled = true;
            this.comboUnit.Location = new System.Drawing.Point(91, 18);
            this.comboUnit.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.comboUnit.Name = "comboUnit";
            this.comboUnit.Size = new System.Drawing.Size(205, 23);
            this.comboUnit.TabIndex = 0;
            this.comboUnit.SelectedIndexChanged += new System.EventHandler(this.comboUnit_SelectedIndexChanged);
            // 
            // comboAxis
            // 
            this.comboAxis.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboAxis.FormattingEnabled = true;
            this.comboAxis.Location = new System.Drawing.Point(91, 55);
            this.comboAxis.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.comboAxis.Name = "comboAxis";
            this.comboAxis.Size = new System.Drawing.Size(285, 23);
            this.comboAxis.TabIndex = 1;
            // 
            // numDist
            // 
            this.numDist.DecimalPlaces = 3;
            this.numDist.Location = new System.Drawing.Point(91, 92);
            this.numDist.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.numDist.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.numDist.Minimum = new decimal(new int[] {
            1000,
            0,
            0,
            -2147483648});
            this.numDist.Name = "numDist";
            this.numDist.Size = new System.Drawing.Size(137, 25);
            this.numDist.TabIndex = 2;
            this.numDist.Value = new decimal(new int[] {
            1000,
            0,
            0,
            196608});
            // 
            // labelUnit
            // 
            this.labelUnit.AutoSize = true;
            this.labelUnit.Location = new System.Drawing.Point(23, 22);
            this.labelUnit.Name = "labelUnit";
            this.labelUnit.Size = new System.Drawing.Size(42, 15);
            this.labelUnit.TabIndex = 6;
            this.labelUnit.Text = "유닛:";
            // 
            // labelAxis
            // 
            this.labelAxis.AutoSize = true;
            this.labelAxis.Location = new System.Drawing.Point(23, 60);
            this.labelAxis.Name = "labelAxis";
            this.labelAxis.Size = new System.Drawing.Size(27, 15);
            this.labelAxis.TabIndex = 7;
            this.labelAxis.Text = "축:";
            // 
            // labelDist
            // 
            this.labelDist.AutoSize = true;
            this.labelDist.Location = new System.Drawing.Point(23, 98);
            this.labelDist.Name = "labelDist";
            this.labelDist.Size = new System.Drawing.Size(71, 15);
            this.labelDist.TabIndex = 8;
            this.labelDist.Text = "거리(mm)";
            // 
            // button_TestGyn_Test
            // 
            this.button_TestGyn_Test.Location = new System.Drawing.Point(91, 132);
            this.button_TestGyn_Test.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.button_TestGyn_Test.Name = "button_TestGyn_Test";
            this.button_TestGyn_Test.Size = new System.Drawing.Size(137, 35);
            this.button_TestGyn_Test.TabIndex = 3;
            this.button_TestGyn_Test.Text = "축 테스트";
            this.button_TestGyn_Test.UseVisualStyleBackColor = true;
            this.button_TestGyn_Test.Click += new System.EventHandler(this.button_TestGyn_Test_Click);
            // 
            // button_TestGyn_Alarm
            // 
            this.button_TestGyn_Alarm.Location = new System.Drawing.Point(240, 132);
            this.button_TestGyn_Alarm.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.button_TestGyn_Alarm.Name = "button_TestGyn_Alarm";
            this.button_TestGyn_Alarm.Size = new System.Drawing.Size(137, 35);
            this.button_TestGyn_Alarm.TabIndex = 9;
            this.button_TestGyn_Alarm.Text = "알람 테스트";
            this.button_TestGyn_Alarm.UseVisualStyleBackColor = true;
            this.button_TestGyn_Alarm.Click += new System.EventHandler(this.button_TestGyn_Alarm_Click);
            // 
            // pictureBox_ImageView_Test
            // 
            this.pictureBox_ImageView_Test.BackColor = System.Drawing.Color.Black;
            this.pictureBox_ImageView_Test.Camera = null;
            this.pictureBox_ImageView_Test.CameraSwitch = null;
            this.pictureBox_ImageView_Test.FrameRate = 1D;
            this.pictureBox_ImageView_Test.InputImage = null;
            this.pictureBox_ImageView_Test.IsViewCustomizedImage = false;
            this.pictureBox_ImageView_Test.Location = new System.Drawing.Point(501, 22);
            this.pictureBox_ImageView_Test.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.pictureBox_ImageView_Test.Name = "pictureBox_ImageView_Test";
            this.pictureBox_ImageView_Test.OperatingType = QMC.Common.Vision.VisionImageViewer.OperatingTypes.Center;
            this.pictureBox_ImageView_Test.Simulated = false;
            this.pictureBox_ImageView_Test.Size = new System.Drawing.Size(208, 310);
            this.pictureBox_ImageView_Test.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox_ImageView_Test.TabIndex = 10;
            this.pictureBox_ImageView_Test.TabStop = false;
            this.pictureBox_ImageView_Test.UpdateDelayTime = 200;
            this.pictureBox_ImageView_Test.VisibleCrossLine = true;
            // 
            // TestGyn
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(914, 562);
            this.Controls.Add(this.pictureBox_ImageView_Test);
            this.Controls.Add(this.button_TestGyn_Alarm);
            this.Controls.Add(this.labelDist);
            this.Controls.Add(this.labelAxis);
            this.Controls.Add(this.labelUnit);
            this.Controls.Add(this.numDist);
            this.Controls.Add(this.comboAxis);
            this.Controls.Add(this.comboUnit);
            this.Controls.Add(this.button_TestGyn_Test);
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "TestGyn";
            this.Text = "TestGyn";
            this.Load += new System.EventHandler(this.TestGyn_Load);
            ((System.ComponentModel.ISupportInitialize)(this.numDist)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_ImageView_Test)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox comboUnit;
        private System.Windows.Forms.ComboBox comboAxis;
        private System.Windows.Forms.NumericUpDown numDist;
        private System.Windows.Forms.Label labelUnit;
        private System.Windows.Forms.Label labelAxis;
        private System.Windows.Forms.Label labelDist;
        private System.Windows.Forms.Button button_TestGyn_Test;
        private System.Windows.Forms.Button button_TestGyn_Alarm;
        //private System.Windows.Forms.PictureBox pictureBox_ImageView_Test;
        private QMC.Common.Vision.VisionImageViewer pictureBox_ImageView_Test;
    }
}