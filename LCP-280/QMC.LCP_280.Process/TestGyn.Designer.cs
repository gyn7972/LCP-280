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
            this.comboUnit = new System.Windows.Forms.ComboBox();
            this.comboAxis = new System.Windows.Forms.ComboBox();
            this.numDist = new System.Windows.Forms.NumericUpDown();
            this.labelUnit = new System.Windows.Forms.Label();
            this.labelAxis = new System.Windows.Forms.Label();
            this.labelDist = new System.Windows.Forms.Label();
            this.button_TestGyn_Test = new System.Windows.Forms.Button();
            this.button_TestGyn_Alarm = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.numDist)).BeginInit();
            this.SuspendLayout();
            // 
            // comboUnit
            // 
            this.comboUnit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboUnit.FormattingEnabled = true;
            this.comboUnit.Location = new System.Drawing.Point(80, 14);
            this.comboUnit.Name = "comboUnit";
            this.comboUnit.Size = new System.Drawing.Size(180, 20);
            this.comboUnit.TabIndex = 0;
            this.comboUnit.SelectedIndexChanged += new System.EventHandler(this.comboUnit_SelectedIndexChanged);
            // 
            // comboAxis
            // 
            this.comboAxis.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboAxis.FormattingEnabled = true;
            this.comboAxis.Location = new System.Drawing.Point(80, 44);
            this.comboAxis.Name = "comboAxis";
            this.comboAxis.Size = new System.Drawing.Size(250, 20);
            this.comboAxis.TabIndex = 1;
            // 
            // numDist
            // 
            this.numDist.DecimalPlaces = 3;
            this.numDist.Location = new System.Drawing.Point(80, 74);
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
            this.numDist.Size = new System.Drawing.Size(120, 21);
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
            this.labelUnit.Location = new System.Drawing.Point(20, 18);
            this.labelUnit.Name = "labelUnit";
            this.labelUnit.Size = new System.Drawing.Size(33, 12);
            this.labelUnit.TabIndex = 6;
            this.labelUnit.Text = "유닛:";
            // 
            // labelAxis
            // 
            this.labelAxis.AutoSize = true;
            this.labelAxis.Location = new System.Drawing.Point(20, 48);
            this.labelAxis.Name = "labelAxis";
            this.labelAxis.Size = new System.Drawing.Size(21, 12);
            this.labelAxis.TabIndex = 7;
            this.labelAxis.Text = "축:";
            // 
            // labelDist
            // 
            this.labelDist.AutoSize = true;
            this.labelDist.Location = new System.Drawing.Point(20, 78);
            this.labelDist.Name = "labelDist";
            this.labelDist.Size = new System.Drawing.Size(61, 12);
            this.labelDist.TabIndex = 8;
            this.labelDist.Text = "거리(mm)";
            // 
            // button_TestGyn_Test
            // 
            this.button_TestGyn_Test.Location = new System.Drawing.Point(80, 106);
            this.button_TestGyn_Test.Name = "button_TestGyn_Test";
            this.button_TestGyn_Test.Size = new System.Drawing.Size(120, 28);
            this.button_TestGyn_Test.TabIndex = 3;
            this.button_TestGyn_Test.Text = "축 테스트";
            this.button_TestGyn_Test.UseVisualStyleBackColor = true;
            this.button_TestGyn_Test.Click += new System.EventHandler(this.button_TestGyn_Test_Click);
            // 
            // button_TestGyn_Alarm
            // 
            this.button_TestGyn_Alarm.Location = new System.Drawing.Point(210, 106);
            this.button_TestGyn_Alarm.Name = "button_TestGyn_Alarm";
            this.button_TestGyn_Alarm.Size = new System.Drawing.Size(120, 28);
            this.button_TestGyn_Alarm.TabIndex = 9;
            this.button_TestGyn_Alarm.Text = "알람 테스트";
            this.button_TestGyn_Alarm.UseVisualStyleBackColor = true;
            this.button_TestGyn_Alarm.Click += new System.EventHandler(this.button_TestGyn_Alarm_Click);
            // 
            // TestGyn
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.button_TestGyn_Alarm);
            this.Controls.Add(this.labelDist);
            this.Controls.Add(this.labelAxis);
            this.Controls.Add(this.labelUnit);
            this.Controls.Add(this.numDist);
            this.Controls.Add(this.comboAxis);
            this.Controls.Add(this.comboUnit);
            this.Controls.Add(this.button_TestGyn_Test);
            this.Name = "TestGyn";
            this.Text = "TestGyn";
            this.Load += new System.EventHandler(this.TestGyn_Load);
            ((System.ComponentModel.ISupportInitialize)(this.numDist)).EndInit();
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
    }
}