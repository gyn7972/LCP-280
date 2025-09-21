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
            this.components = new System.ComponentModel.Container();
            this.groupBoxManual = new System.Windows.Forms.GroupBox();
            this.labelIndexSocketNo = new System.Windows.Forms.Label();
            this.comboBoxIndexSocketNo = new System.Windows.Forms.ComboBox();
            this.btnInputMAlign = new QMC.Common.IndividualMenuButton();
            this.buttonTest = new System.Windows.Forms.Button();
            this.panel2 = new System.Windows.Forms.Panel();
            this.teachingPositionControl = new QMC.LCP_280.Process.Component.TeachingPositionControl();
            this.panel1 = new System.Windows.Forms.Panel();
            this.dioControl = new QMC.LCP_280.Process.Component.DIOControl();
            this.manualSequenceControl = new QMC.LCP_280.Process.Component.ManualSequenceControl();
            this.manualSequenceControlProbe = new QMC.LCP_280.Process.Component.ManualSequenceControl();
            this.groupBoxProcessSeq = new System.Windows.Forms.GroupBox();
            this.manualSequenceControlProcessSeq = new QMC.LCP_280.Process.Component.ManualSequenceControl();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.manualSequenceControlOutAlign = new QMC.LCP_280.Process.Component.ManualSequenceControl();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this._ProcessCameraviewer = new QMC.Common.Vision.VisionImageViewer();
            this.groupBoxManual.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel1.SuspendLayout();
            this.groupBoxProcessSeq.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._ProcessCameraviewer)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBoxManual
            // 
            this.groupBoxManual.Controls.Add(this.labelIndexSocketNo);
            this.groupBoxManual.Controls.Add(this.comboBoxIndexSocketNo);
            this.groupBoxManual.Controls.Add(this.btnInputMAlign);
            this.groupBoxManual.Controls.Add(this.buttonTest);
            this.groupBoxManual.Font = new System.Drawing.Font("맑은 고딕", 10.2F, System.Drawing.FontStyle.Bold);
            this.groupBoxManual.Location = new System.Drawing.Point(629, 418);
            this.groupBoxManual.Name = "groupBoxManual";
            this.groupBoxManual.Size = new System.Drawing.Size(392, 114);
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
            this.btnInputMAlign.Location = new System.Drawing.Point(6, 62);
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
            // manualSequenceControl
            // 
            this.manualSequenceControl.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.manualSequenceControl.Location = new System.Drawing.Point(3, 3);
            this.manualSequenceControl.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.manualSequenceControl.MinimumSize = new System.Drawing.Size(297, 250);
            this.manualSequenceControl.Name = "manualSequenceControl";
            this.manualSequenceControl.ParentUnit = null;
            this.manualSequenceControl.Size = new System.Drawing.Size(401, 310);
            this.manualSequenceControl.TabIndex = 14;
            // 
            // manualSequenceControlProbe
            // 
            this.manualSequenceControlProbe.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.manualSequenceControlProbe.Location = new System.Drawing.Point(3, 3);
            this.manualSequenceControlProbe.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.manualSequenceControlProbe.MinimumSize = new System.Drawing.Size(297, 250);
            this.manualSequenceControlProbe.Name = "manualSequenceControlProbe";
            this.manualSequenceControlProbe.ParentUnit = null;
            this.manualSequenceControlProbe.Size = new System.Drawing.Size(401, 310);
            this.manualSequenceControlProbe.TabIndex = 15;
            // 
            // groupBoxProcessSeq
            // 
            this.groupBoxProcessSeq.Controls.Add(this.manualSequenceControlProcessSeq);
            this.groupBoxProcessSeq.Font = new System.Drawing.Font("맑은 고딕", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.groupBoxProcessSeq.Location = new System.Drawing.Point(1027, 12);
            this.groupBoxProcessSeq.Name = "groupBoxProcessSeq";
            this.groupBoxProcessSeq.Size = new System.Drawing.Size(395, 354);
            this.groupBoxProcessSeq.TabIndex = 27;
            this.groupBoxProcessSeq.TabStop = false;
            this.groupBoxProcessSeq.Text = "Process Seq";
            // 
            // manualSequenceControlProcessSeq
            // 
            this.manualSequenceControlProcessSeq.Dock = System.Windows.Forms.DockStyle.Top;
            this.manualSequenceControlProcessSeq.Location = new System.Drawing.Point(3, 26);
            this.manualSequenceControlProcessSeq.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.manualSequenceControlProcessSeq.MinimumSize = new System.Drawing.Size(297, 250);
            this.manualSequenceControlProcessSeq.Name = "manualSequenceControlProcessSeq";
            this.manualSequenceControlProcessSeq.ParentUnit = null;
            this.manualSequenceControlProcessSeq.Size = new System.Drawing.Size(389, 321);
            this.manualSequenceControlProcessSeq.TabIndex = 14;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Font = new System.Drawing.Font("맑은 고딕", 10.2F, System.Drawing.FontStyle.Bold);
            this.tabControl1.Location = new System.Drawing.Point(1027, 418);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(415, 352);
            this.tabControl1.TabIndex = 28;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.manualSequenceControl);
            this.tabPage1.Location = new System.Drawing.Point(4, 32);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(407, 316);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "MAlign";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.manualSequenceControlProbe);
            this.tabPage2.Location = new System.Drawing.Point(4, 32);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(407, 316);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Probe";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.manualSequenceControlOutAlign);
            this.tabPage3.Location = new System.Drawing.Point(4, 32);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Size = new System.Drawing.Size(407, 316);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "OutAlign";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // manualSequenceControlOutAlign
            // 
            this.manualSequenceControlOutAlign.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.manualSequenceControlOutAlign.Location = new System.Drawing.Point(0, 6);
            this.manualSequenceControlOutAlign.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.manualSequenceControlOutAlign.MinimumSize = new System.Drawing.Size(297, 250);
            this.manualSequenceControlOutAlign.Name = "manualSequenceControlOutAlign";
            this.manualSequenceControlOutAlign.ParentUnit = null;
            this.manualSequenceControlOutAlign.Size = new System.Drawing.Size(407, 310);
            this.manualSequenceControlOutAlign.TabIndex = 29;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this._ProcessCameraviewer);
            this.groupBox1.Font = new System.Drawing.Font("맑은 고딕", 10.2F, System.Drawing.FontStyle.Bold);
            this.groupBox1.Location = new System.Drawing.Point(629, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(392, 400);
            this.groupBox1.TabIndex = 29;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "ImageView";
            // 
            // _ProcessCameraviewer
            // 
            this._ProcessCameraviewer.BackColor = System.Drawing.Color.Black;
            this._ProcessCameraviewer.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this._ProcessCameraviewer.Camera = null;
            this._ProcessCameraviewer.CameraSwitch = null;
            this._ProcessCameraviewer.Dock = System.Windows.Forms.DockStyle.Top;
            this._ProcessCameraviewer.FrameRate = 1D;
            this._ProcessCameraviewer.InputImage = null;
            this._ProcessCameraviewer.IsViewCustomizedImage = false;
            this._ProcessCameraviewer.Location = new System.Drawing.Point(3, 26);
            this._ProcessCameraviewer.Name = "_ProcessCameraviewer";
            this._ProcessCameraviewer.OperatingType = QMC.Common.Vision.VisionImageViewer.OperatingTypes.Center;
            this._ProcessCameraviewer.Simulated = false;
            this._ProcessCameraviewer.Size = new System.Drawing.Size(386, 324);
            this._ProcessCameraviewer.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this._ProcessCameraviewer.TabIndex = 13;
            this._ProcessCameraviewer.TabStop = false;
            this._ProcessCameraviewer.UpdateDelayTime = 80;
            this._ProcessCameraviewer.VisibleCrossLine = true;
            // 
            // Process_Working
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(120F, 120F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(1580, 939);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.groupBoxProcessSeq);
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
            this.groupBoxProcessSeq.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.tabPage3.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this._ProcessCameraviewer)).EndInit();
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
        private Component.ManualSequenceControl manualSequenceControlProbe;
        private System.Windows.Forms.GroupBox groupBoxProcessSeq;
        private Component.ManualSequenceControl manualSequenceControlProcessSeq;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.TabPage tabPage3;
        private Component.ManualSequenceControl manualSequenceControlOutAlign;
        private System.Windows.Forms.GroupBox groupBox1;
        private Common.Vision.VisionImageViewer _ProcessCameraviewer;
    }
}