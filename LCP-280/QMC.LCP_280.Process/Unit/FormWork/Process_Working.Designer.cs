namespace QMC.LCP_280.Process.Unit.FormWork
{
    partial class Process_Working
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
            this.groupBoxManual = new System.Windows.Forms.GroupBox();
            this.labelIndexSocketNo = new System.Windows.Forms.Label();
            this.comboBoxIndexSocketNo = new System.Windows.Forms.ComboBox();
            this.btnInputMAlign = new QMC.Common.IndividualMenuButton();
            this.buttonTest = new System.Windows.Forms.Button();
            this.panel2 = new System.Windows.Forms.Panel();
            this.teachingPositionControl = new QMC.LCP_280.Process.Component.TeachingPositionControl();
            this.panel1 = new System.Windows.Forms.Panel();
            this.dioControl = new QMC.LCP_280.Process.Component.DIOControl();
            this.panel3 = new System.Windows.Forms.Panel();
            this.manualSequenceControl = new QMC.LCP_280.Process.Component.ManualSequenceControl();
            this.manualSequenceControlProbe = new QMC.LCP_280.Process.Component.ManualSequenceControl();
            this.panel4 = new System.Windows.Forms.Panel();
            this.groupBoxManual.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel1.SuspendLayout();
            this.panel3.SuspendLayout();
            this.panel4.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBoxManual
            // 
            this.groupBoxManual.Controls.Add(this.labelIndexSocketNo);
            this.groupBoxManual.Controls.Add(this.comboBoxIndexSocketNo);
            this.groupBoxManual.Controls.Add(this.btnInputMAlign);
            this.groupBoxManual.Controls.Add(this.buttonTest);
            this.groupBoxManual.Location = new System.Drawing.Point(629, 422);
            this.groupBoxManual.Name = "groupBoxManual";
            this.groupBoxManual.Size = new System.Drawing.Size(395, 412);
            this.groupBoxManual.TabIndex = 17;
            this.groupBoxManual.TabStop = false;
            this.groupBoxManual.Text = "Manual";
            // 
            // labelIndexSocketNo
            // 
            this.labelIndexSocketNo.AutoSize = true;
            this.labelIndexSocketNo.Font = new System.Drawing.Font("맑은 고딕", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.labelIndexSocketNo.Location = new System.Drawing.Point(6, 24);
            this.labelIndexSocketNo.Name = "labelIndexSocketNo";
            this.labelIndexSocketNo.Size = new System.Drawing.Size(126, 23);
            this.labelIndexSocketNo.TabIndex = 22;
            this.labelIndexSocketNo.Text = "IndexSocketNo";
            // 
            // comboBoxIndexSocketNo
            // 
            this.comboBoxIndexSocketNo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxIndexSocketNo.Font = new System.Drawing.Font("맑은 고딕", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.comboBoxIndexSocketNo.FormattingEnabled = true;
            this.comboBoxIndexSocketNo.Location = new System.Drawing.Point(138, 21);
            this.comboBoxIndexSocketNo.Name = "comboBoxIndexSocketNo";
            this.comboBoxIndexSocketNo.Size = new System.Drawing.Size(130, 31);
            this.comboBoxIndexSocketNo.TabIndex = 21;
            this.comboBoxIndexSocketNo.SelectedIndexChanged += new System.EventHandler(this.comboBoxIndexSocketNo_SelectedIndexChanged);
            // 
            // btnInputMAlign
            // 
            this.btnInputMAlign.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnInputMAlign.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnInputMAlign.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnInputMAlign.CustomFont = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnInputMAlign.CustomForeColor = System.Drawing.Color.Black;
            this.btnInputMAlign.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnInputMAlign.ForeColor = System.Drawing.Color.Black;
            this.btnInputMAlign.ImageSize = new System.Drawing.Size(45, 45);
            this.btnInputMAlign.Location = new System.Drawing.Point(10, 62);
            this.btnInputMAlign.Name = "btnInputMAlign";
            this.btnInputMAlign.Size = new System.Drawing.Size(130, 35);
            this.btnInputMAlign.TabIndex = 20;
            this.btnInputMAlign.TabStop = false;
            this.btnInputMAlign.Text = "InputMAlign";
            this.btnInputMAlign.UseVisualStyleBackColor = false;
            this.btnInputMAlign.Click += new System.EventHandler(this.btnInputMAlign_ClickAsync);
            // 
            // buttonTest
            // 
            this.buttonTest.Location = new System.Drawing.Point(274, 20);
            this.buttonTest.Name = "buttonTest";
            this.buttonTest.Size = new System.Drawing.Size(104, 35);
            this.buttonTest.TabIndex = 18;
            this.buttonTest.Text = "SEQ STOP";
            this.buttonTest.UseVisualStyleBackColor = true;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.teachingPositionControl);
            this.panel2.Location = new System.Drawing.Point(8, 10);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(615, 400);
            this.panel2.TabIndex = 23;
            // 
            // teachingPositionControl
            // 
            this.teachingPositionControl.ButtonSize = new System.Drawing.Size(90, 32);
            this.teachingPositionControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.teachingPositionControl.Location = new System.Drawing.Point(0, 0);
            this.teachingPositionControl.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
            this.teachingPositionControl.Name = "teachingPositionControl";
            this.teachingPositionControl.ShowCancelButton = true;
            this.teachingPositionControl.ShowSaveButton = true;
            this.teachingPositionControl.Size = new System.Drawing.Size(615, 400);
            this.teachingPositionControl.TabIndex = 15;
            this.teachingPositionControl.UnitName = null;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.dioControl);
            this.panel1.Location = new System.Drawing.Point(8, 418);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(615, 400);
            this.panel1.TabIndex = 22;
            // 
            // dioControl
            // 
            this.dioControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dioControl.IoSortMode = QMC.LCP_280.Process.Component.DIOControl.SortingMode.AlphabeticalKey;
            this.dioControl.Location = new System.Drawing.Point(0, 0);
            this.dioControl.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
            this.dioControl.Name = "dioControl";
            this.dioControl.RefreshIntervalMs = 400;
            this.dioControl.Size = new System.Drawing.Size(615, 400);
            this.dioControl.TabIndex = 16;
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.manualSequenceControl);
            this.panel3.Location = new System.Drawing.Point(629, 10);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(380, 310);
            this.panel3.TabIndex = 24;
            // 
            // manualSequenceControl
            // 
            this.manualSequenceControl.Dock = System.Windows.Forms.DockStyle.Top;
            this.manualSequenceControl.Location = new System.Drawing.Point(0, 0);
            this.manualSequenceControl.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.manualSequenceControl.MinimumSize = new System.Drawing.Size(297, 250);
            this.manualSequenceControl.Name = "manualSequenceControl";
            this.manualSequenceControl.ParentUnit = null;
            this.manualSequenceControl.Size = new System.Drawing.Size(380, 310);
            this.manualSequenceControl.TabIndex = 14;
            // 
            // manualSequenceControlProbe
            // 
            this.manualSequenceControlProbe.Dock = System.Windows.Forms.DockStyle.Top;
            this.manualSequenceControlProbe.Location = new System.Drawing.Point(0, 0);
            this.manualSequenceControlProbe.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.manualSequenceControlProbe.MinimumSize = new System.Drawing.Size(297, 250);
            this.manualSequenceControlProbe.Name = "manualSequenceControlProbe";
            this.manualSequenceControlProbe.ParentUnit = null;
            this.manualSequenceControlProbe.Size = new System.Drawing.Size(380, 310);
            this.manualSequenceControlProbe.TabIndex = 15;
            // 
            // panel4
            // 
            this.panel4.Controls.Add(this.manualSequenceControlProbe);
            this.panel4.Location = new System.Drawing.Point(1015, 10);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(380, 310);
            this.panel4.TabIndex = 25;
            // 
            // Process_Working
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(120F, 120F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(1580, 939);
            this.Controls.Add(this.panel4);
            this.Controls.Add(this.panel3);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.groupBoxManual);
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "Process_Working";
            this.Text = "Process_Working";
            this.groupBoxManual.ResumeLayout(false);
            this.groupBoxManual.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.panel4.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private Component.DIOControl dioControl;
        private Component.TeachingPositionControl teachingPositionControl;
        private Component.ManualSequenceControl manualSequenceControl;
        private System.Windows.Forms.GroupBox groupBoxManual;
        private Common.IndividualMenuButton btnInputMAlign;
        private System.Windows.Forms.Button buttonTest;
        private System.Windows.Forms.Label labelIndexSocketNo;
        private System.Windows.Forms.ComboBox comboBoxIndexSocketNo;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel3;
        private Component.ManualSequenceControl manualSequenceControlProbe;
        private System.Windows.Forms.Panel panel4;
    }
}