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
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.teachingPositionControl = new QMC.LCP_280.Process.Component.TeachingPositionControl();
            this.dioControl = new QMC.LCP_280.Process.Component.DIOControl();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.groupBoxManual.SuspendLayout();
            this.groupBoxProcessSeq.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._ProcessCameraviewer)).BeginInit();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBoxManual
            // 
            this.groupBoxManual.Controls.Add(this.tableLayoutPanel2);
            this.groupBoxManual.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxManual.Font = new System.Drawing.Font("맑은 고딕", 10F);
            this.groupBoxManual.Location = new System.Drawing.Point(507, 377);
            this.groupBoxManual.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.groupBoxManual.Name = "groupBoxManual";
            this.groupBoxManual.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.groupBoxManual.Size = new System.Drawing.Size(375, 372);
            this.groupBoxManual.TabIndex = 17;
            this.groupBoxManual.TabStop = false;
            this.groupBoxManual.Text = "Manual";
            // 
            // labelIndexSocketNo
            // 
            this.labelIndexSocketNo.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelIndexSocketNo.AutoSize = true;
            this.labelIndexSocketNo.Font = new System.Drawing.Font("맑은 고딕", 10F);
            this.labelIndexSocketNo.Location = new System.Drawing.Point(3, 3);
            this.labelIndexSocketNo.Margin = new System.Windows.Forms.Padding(3);
            this.labelIndexSocketNo.Name = "labelIndexSocketNo";
            this.labelIndexSocketNo.Size = new System.Drawing.Size(117, 29);
            this.labelIndexSocketNo.TabIndex = 22;
            this.labelIndexSocketNo.Text = "IndexSocketNo";
            // 
            // comboBoxIndexSocketNo
            // 
            this.comboBoxIndexSocketNo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.comboBoxIndexSocketNo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxIndexSocketNo.Font = new System.Drawing.Font("맑은 고딕", 10F);
            this.comboBoxIndexSocketNo.FormattingEnabled = true;
            this.comboBoxIndexSocketNo.Location = new System.Drawing.Point(126, 3);
            this.comboBoxIndexSocketNo.Name = "comboBoxIndexSocketNo";
            this.comboBoxIndexSocketNo.Size = new System.Drawing.Size(117, 25);
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
            this.btnInputMAlign.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnInputMAlign.Font = new System.Drawing.Font("맑은 고딕", 10F);
            this.btnInputMAlign.ForeColor = System.Drawing.Color.Black;
            this.btnInputMAlign.ImageSize = new System.Drawing.Size(45, 45);
            this.btnInputMAlign.Location = new System.Drawing.Point(3, 38);
            this.btnInputMAlign.Name = "btnInputMAlign";
            this.btnInputMAlign.Size = new System.Drawing.Size(117, 29);
            this.btnInputMAlign.TabIndex = 20;
            this.btnInputMAlign.TabStop = false;
            this.btnInputMAlign.Text = "InputMAlign";
            this.btnInputMAlign.UseVisualStyleBackColor = false;
            this.btnInputMAlign.Click += new System.EventHandler(this.btnInputMAlign_ClickAsync);
            // 
            // buttonTest
            // 
            this.buttonTest.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonTest.Location = new System.Drawing.Point(249, 3);
            this.buttonTest.Name = "buttonTest";
            this.buttonTest.Size = new System.Drawing.Size(119, 29);
            this.buttonTest.TabIndex = 18;
            this.buttonTest.Text = "SEQ STOP";
            this.buttonTest.UseVisualStyleBackColor = true;
            // 
            // manualSequenceControl
            // 
            this.manualSequenceControl.Dock = System.Windows.Forms.DockStyle.Top;
            this.manualSequenceControl.Location = new System.Drawing.Point(2, 2);
            this.manualSequenceControl.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.manualSequenceControl.MinimumSize = new System.Drawing.Size(238, 200);
            this.manualSequenceControl.Name = "manualSequenceControl";
            this.manualSequenceControl.ParentUnit = null;
            this.manualSequenceControl.Size = new System.Drawing.Size(364, 248);
            this.manualSequenceControl.TabIndex = 14;
            // 
            // manualSequenceControlProbe
            // 
            this.manualSequenceControlProbe.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.manualSequenceControlProbe.Location = new System.Drawing.Point(2, 0);
            this.manualSequenceControlProbe.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.manualSequenceControlProbe.MinimumSize = new System.Drawing.Size(238, 200);
            this.manualSequenceControlProbe.Name = "manualSequenceControlProbe";
            this.manualSequenceControlProbe.ParentUnit = null;
            this.manualSequenceControlProbe.Size = new System.Drawing.Size(320, 248);
            this.manualSequenceControlProbe.TabIndex = 15;
            // 
            // groupBoxProcessSeq
            // 
            this.groupBoxProcessSeq.Controls.Add(this.manualSequenceControlProcessSeq);
            this.groupBoxProcessSeq.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxProcessSeq.Font = new System.Drawing.Font("맑은 고딕", 10F);
            this.groupBoxProcessSeq.Location = new System.Drawing.Point(886, 2);
            this.groupBoxProcessSeq.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.groupBoxProcessSeq.Name = "groupBoxProcessSeq";
            this.groupBoxProcessSeq.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.groupBoxProcessSeq.Size = new System.Drawing.Size(376, 371);
            this.groupBoxProcessSeq.TabIndex = 27;
            this.groupBoxProcessSeq.TabStop = false;
            this.groupBoxProcessSeq.Text = "Process Seq";
            // 
            // manualSequenceControlProcessSeq
            // 
            this.manualSequenceControlProcessSeq.Dock = System.Windows.Forms.DockStyle.Top;
            this.manualSequenceControlProcessSeq.Location = new System.Drawing.Point(2, 20);
            this.manualSequenceControlProcessSeq.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.manualSequenceControlProcessSeq.MinimumSize = new System.Drawing.Size(238, 200);
            this.manualSequenceControlProcessSeq.Name = "manualSequenceControlProcessSeq";
            this.manualSequenceControlProcessSeq.ParentUnit = null;
            this.manualSequenceControlProcessSeq.Size = new System.Drawing.Size(372, 257);
            this.manualSequenceControlProcessSeq.TabIndex = 14;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Font = new System.Drawing.Font("맑은 고딕", 10F);
            this.tabControl1.Location = new System.Drawing.Point(886, 377);
            this.tabControl1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(376, 372);
            this.tabControl1.TabIndex = 28;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.manualSequenceControl);
            this.tabPage1.Location = new System.Drawing.Point(4, 26);
            this.tabPage1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tabPage1.Size = new System.Drawing.Size(368, 342);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "MAlign";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.manualSequenceControlProbe);
            this.tabPage2.Location = new System.Drawing.Point(4, 28);
            this.tabPage2.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tabPage2.Size = new System.Drawing.Size(324, 250);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Probe";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.manualSequenceControlOutAlign);
            this.tabPage3.Location = new System.Drawing.Point(4, 28);
            this.tabPage3.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Size = new System.Drawing.Size(324, 250);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "OutAlign";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // manualSequenceControlOutAlign
            // 
            this.manualSequenceControlOutAlign.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.manualSequenceControlOutAlign.Location = new System.Drawing.Point(0, 2);
            this.manualSequenceControlOutAlign.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.manualSequenceControlOutAlign.MinimumSize = new System.Drawing.Size(238, 200);
            this.manualSequenceControlOutAlign.Name = "manualSequenceControlOutAlign";
            this.manualSequenceControlOutAlign.ParentUnit = null;
            this.manualSequenceControlOutAlign.Size = new System.Drawing.Size(324, 248);
            this.manualSequenceControlOutAlign.TabIndex = 29;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this._ProcessCameraviewer);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Font = new System.Drawing.Font("맑은 고딕", 10F);
            this.groupBox1.Location = new System.Drawing.Point(507, 2);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.groupBox1.Size = new System.Drawing.Size(375, 371);
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
            this._ProcessCameraviewer.Dock = System.Windows.Forms.DockStyle.Fill;
            this._ProcessCameraviewer.FrameRate = 1D;
            this._ProcessCameraviewer.InputImage = null;
            this._ProcessCameraviewer.IsViewCustomizedImage = false;
            this._ProcessCameraviewer.Location = new System.Drawing.Point(2, 20);
            this._ProcessCameraviewer.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this._ProcessCameraviewer.Name = "_ProcessCameraviewer";
            this._ProcessCameraviewer.OperatingType = QMC.Common.Vision.VisionImageViewer.OperatingTypes.Center;
            this._ProcessCameraviewer.Simulated = false;
            this._ProcessCameraviewer.Size = new System.Drawing.Size(371, 349);
            this._ProcessCameraviewer.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this._ProcessCameraviewer.TabIndex = 13;
            this._ProcessCameraviewer.TabStop = false;
            this._ProcessCameraviewer.UpdateDelayTime = 80;
            this._ProcessCameraviewer.VisibleCrossLine = true;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 40F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.tableLayoutPanel1.Controls.Add(this.groupBox1, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.groupBoxProcessSeq, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.dioControl, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.teachingPositionControl, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.groupBoxManual, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.tabControl1, 2, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1264, 751);
            this.tableLayoutPanel1.TabIndex = 30;
            // 
            // teachingPositionControl
            // 
            this.teachingPositionControl.ButtonSize = new System.Drawing.Size(90, 32);
            this.teachingPositionControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.teachingPositionControl.Location = new System.Drawing.Point(2, 4);
            this.teachingPositionControl.Margin = new System.Windows.Forms.Padding(2, 4, 2, 4);
            this.teachingPositionControl.Name = "teachingPositionControl";
            this.teachingPositionControl.ShowCancelButton = true;
            this.teachingPositionControl.ShowSaveButton = true;
            this.teachingPositionControl.Size = new System.Drawing.Size(501, 367);
            this.teachingPositionControl.TabIndex = 31;
            this.teachingPositionControl.UnitName = null;
            // 
            // dioControl
            // 
            this.dioControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dioControl.IoSortMode = QMC.LCP_280.Process.Component.DIOControl.SortingMode.AlphabeticalKey;
            this.dioControl.Location = new System.Drawing.Point(2, 379);
            this.dioControl.Margin = new System.Windows.Forms.Padding(2, 4, 2, 4);
            this.dioControl.Name = "dioControl";
            this.dioControl.RefreshIntervalMs = 400;
            this.dioControl.Size = new System.Drawing.Size(501, 368);
            this.dioControl.TabIndex = 32;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 3;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel2.Controls.Add(this.btnInputMAlign, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.labelIndexSocketNo, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.buttonTest, 2, 0);
            this.tableLayoutPanel2.Controls.Add(this.comboBoxIndexSocketNo, 1, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(2, 20);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 10;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(371, 350);
            this.tableLayoutPanel2.TabIndex = 23;
            // 
            // Process_Working
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(1264, 751);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.Name = "Process_Working";
            this.Text = "Process_Working";
            this.groupBoxManual.ResumeLayout(false);
            this.groupBoxProcessSeq.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.tabPage3.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this._ProcessCameraviewer)).EndInit();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private Component.ManualSequenceControl manualSequenceControl;
        private System.Windows.Forms.GroupBox groupBoxManual;
        private Common.IndividualMenuButton btnInputMAlign;
        private System.Windows.Forms.Button buttonTest;
        private System.Windows.Forms.Label labelIndexSocketNo;
        private System.Windows.Forms.ComboBox comboBoxIndexSocketNo;
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
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private Component.DIOControl dioControl;
        private Component.TeachingPositionControl teachingPositionControl;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
    }
}