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
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.label4 = new System.Windows.Forms.Label();
            this.labelIndexSocketNo = new System.Windows.Forms.Label();
            this.comboBoxIndexSocketNo = new System.Windows.Forms.ComboBox();
            this.buttonTest = new System.Windows.Forms.Button();
            this.labelsocketNumberInput = new System.Windows.Forms.Label();
            this.labelsocketNumberLAlign = new System.Windows.Forms.Label();
            this.labelsocketNumberProbe = new System.Windows.Forms.Label();
            this.labelsocketNumberUnload = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.checkBoxIndexCal = new System.Windows.Forms.CheckBox();
            this.groupBoxProcessSeq = new System.Windows.Forms.GroupBox();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel5 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this._ProcessCameraviewer = new QMC.Common.Vision.VisionImageViewer();
            this.btnRotary = new QMC.Common.IndividualMenuButton();
            this.ButtonManualTest = new QMC.Common.IndividualMenuButton();
            this.ButtonClear = new QMC.Common.IndividualMenuButton();
            this.btnInputMAlign = new QMC.Common.IndividualMenuButton();
            this.btnManualVision = new QMC.Common.IndividualMenuButton();
            this.manualSequenceControlProcessSeq = new QMC.LCP_280.Process.Component.ManualSequenceControl();
            this.dioControl = new QMC.LCP_280.Process.Component.DIOControl();
            this.teachingPositionControl = new QMC.LCP_280.Process.Component.TeachingPositionControl();
            this.manualSequenceControl = new QMC.LCP_280.Process.Component.ManualSequenceControl();
            this.manualSequenceControlProbe = new QMC.LCP_280.Process.Component.ManualSequenceControl();
            this.manualSequenceControlOutAlign = new QMC.LCP_280.Process.Component.ManualSequenceControl();
            this.groupBoxManual.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.groupBoxProcessSeq.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.tableLayoutPanel5.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._ProcessCameraviewer)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBoxManual
            // 
            this.groupBoxManual.Controls.Add(this.tableLayoutPanel2);
            this.groupBoxManual.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxManual.Font = new System.Drawing.Font("맑은 고딕", 10F);
            this.groupBoxManual.Location = new System.Drawing.Point(761, 566);
            this.groupBoxManual.Name = "groupBoxManual";
            this.groupBoxManual.Size = new System.Drawing.Size(562, 557);
            this.groupBoxManual.TabIndex = 17;
            this.groupBoxManual.TabStop = false;
            this.groupBoxManual.Text = "Manual";
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 3;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel2.Controls.Add(this.label4, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.labelIndexSocketNo, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.labelsocketNumberInput, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.btnRotary, 2, 0);
            this.tableLayoutPanel2.Controls.Add(this.labelsocketNumberLAlign, 1, 1);
            this.tableLayoutPanel2.Controls.Add(this.labelsocketNumberProbe, 1, 2);
            this.tableLayoutPanel2.Controls.Add(this.labelsocketNumberUnload, 1, 3);
            this.tableLayoutPanel2.Controls.Add(this.label5, 0, 2);
            this.tableLayoutPanel2.Controls.Add(this.label6, 0, 3);
            this.tableLayoutPanel2.Controls.Add(this.ButtonManualTest, 0, 5);
            this.tableLayoutPanel2.Controls.Add(this.ButtonClear, 1, 5);
            this.tableLayoutPanel2.Controls.Add(this.checkBoxIndexCal, 2, 5);
            this.tableLayoutPanel2.Controls.Add(this.buttonTest, 2, 9);
            this.tableLayoutPanel2.Controls.Add(this.btnInputMAlign, 1, 9);
            this.tableLayoutPanel2.Controls.Add(this.comboBoxIndexSocketNo, 0, 9);
            this.tableLayoutPanel2.Controls.Add(this.btnManualVision, 0, 7);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 30);
            this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(4);
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
            this.tableLayoutPanel2.Size = new System.Drawing.Size(556, 524);
            this.tableLayoutPanel2.TabIndex = 23;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label4.Font = new System.Drawing.Font("맑은 고딕", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label4.Location = new System.Drawing.Point(4, 56);
            this.label4.Margin = new System.Windows.Forms.Padding(4);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(177, 44);
            this.label4.TabIndex = 28;
            this.label4.Text = "L-Align No";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // labelIndexSocketNo
            // 
            this.labelIndexSocketNo.AutoSize = true;
            this.labelIndexSocketNo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelIndexSocketNo.Font = new System.Drawing.Font("맑은 고딕", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.labelIndexSocketNo.Location = new System.Drawing.Point(4, 4);
            this.labelIndexSocketNo.Margin = new System.Windows.Forms.Padding(4);
            this.labelIndexSocketNo.Name = "labelIndexSocketNo";
            this.labelIndexSocketNo.Size = new System.Drawing.Size(177, 44);
            this.labelIndexSocketNo.TabIndex = 22;
            this.labelIndexSocketNo.Text = "Load No";
            this.labelIndexSocketNo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // comboBoxIndexSocketNo
            // 
            this.comboBoxIndexSocketNo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxIndexSocketNo.Font = new System.Drawing.Font("맑은 고딕", 10F);
            this.comboBoxIndexSocketNo.FormattingEnabled = true;
            this.comboBoxIndexSocketNo.Location = new System.Drawing.Point(4, 472);
            this.comboBoxIndexSocketNo.Margin = new System.Windows.Forms.Padding(4);
            this.comboBoxIndexSocketNo.Name = "comboBoxIndexSocketNo";
            this.comboBoxIndexSocketNo.Size = new System.Drawing.Size(174, 36);
            this.comboBoxIndexSocketNo.TabIndex = 21;
            this.comboBoxIndexSocketNo.Visible = false;
            this.comboBoxIndexSocketNo.SelectedIndexChanged += new System.EventHandler(this.comboBoxIndexSocketNo_SelectedIndexChanged);
            // 
            // buttonTest
            // 
            this.buttonTest.Location = new System.Drawing.Point(374, 472);
            this.buttonTest.Margin = new System.Windows.Forms.Padding(4);
            this.buttonTest.Name = "buttonTest";
            this.buttonTest.Size = new System.Drawing.Size(177, 42);
            this.buttonTest.TabIndex = 18;
            this.buttonTest.Text = "SEQ STOP";
            this.buttonTest.UseVisualStyleBackColor = true;
            this.buttonTest.Visible = false;
            // 
            // labelsocketNumberInput
            // 
            this.labelsocketNumberInput.AutoSize = true;
            this.labelsocketNumberInput.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelsocketNumberInput.Font = new System.Drawing.Font("맑은 고딕", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.labelsocketNumberInput.Location = new System.Drawing.Point(189, 4);
            this.labelsocketNumberInput.Margin = new System.Windows.Forms.Padding(4);
            this.labelsocketNumberInput.Name = "labelsocketNumberInput";
            this.labelsocketNumberInput.Size = new System.Drawing.Size(177, 44);
            this.labelsocketNumberInput.TabIndex = 24;
            this.labelsocketNumberInput.Text = "---";
            this.labelsocketNumberInput.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // labelsocketNumberLAlign
            // 
            this.labelsocketNumberLAlign.AutoSize = true;
            this.labelsocketNumberLAlign.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelsocketNumberLAlign.Font = new System.Drawing.Font("맑은 고딕", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.labelsocketNumberLAlign.Location = new System.Drawing.Point(189, 56);
            this.labelsocketNumberLAlign.Margin = new System.Windows.Forms.Padding(4);
            this.labelsocketNumberLAlign.Name = "labelsocketNumberLAlign";
            this.labelsocketNumberLAlign.Size = new System.Drawing.Size(177, 44);
            this.labelsocketNumberLAlign.TabIndex = 25;
            this.labelsocketNumberLAlign.Text = "---";
            this.labelsocketNumberLAlign.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // labelsocketNumberProbe
            // 
            this.labelsocketNumberProbe.AutoSize = true;
            this.labelsocketNumberProbe.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelsocketNumberProbe.Font = new System.Drawing.Font("맑은 고딕", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.labelsocketNumberProbe.Location = new System.Drawing.Point(189, 108);
            this.labelsocketNumberProbe.Margin = new System.Windows.Forms.Padding(4);
            this.labelsocketNumberProbe.Name = "labelsocketNumberProbe";
            this.labelsocketNumberProbe.Size = new System.Drawing.Size(177, 44);
            this.labelsocketNumberProbe.TabIndex = 26;
            this.labelsocketNumberProbe.Text = "---";
            this.labelsocketNumberProbe.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // labelsocketNumberUnload
            // 
            this.labelsocketNumberUnload.AutoSize = true;
            this.labelsocketNumberUnload.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelsocketNumberUnload.Font = new System.Drawing.Font("맑은 고딕", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.labelsocketNumberUnload.Location = new System.Drawing.Point(189, 160);
            this.labelsocketNumberUnload.Margin = new System.Windows.Forms.Padding(4);
            this.labelsocketNumberUnload.Name = "labelsocketNumberUnload";
            this.labelsocketNumberUnload.Size = new System.Drawing.Size(177, 44);
            this.labelsocketNumberUnload.TabIndex = 27;
            this.labelsocketNumberUnload.Text = "---";
            this.labelsocketNumberUnload.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label5.Font = new System.Drawing.Font("맑은 고딕", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label5.Location = new System.Drawing.Point(4, 108);
            this.label5.Margin = new System.Windows.Forms.Padding(4);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(177, 44);
            this.label5.TabIndex = 29;
            this.label5.Text = "Probe No";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label6.Font = new System.Drawing.Font("맑은 고딕", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label6.Location = new System.Drawing.Point(4, 160);
            this.label6.Margin = new System.Windows.Forms.Padding(4);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(177, 44);
            this.label6.TabIndex = 30;
            this.label6.Text = "Unload No";
            this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // checkBoxIndexCal
            // 
            this.checkBoxIndexCal.AutoSize = true;
            this.checkBoxIndexCal.Dock = System.Windows.Forms.DockStyle.Fill;
            this.checkBoxIndexCal.Enabled = false;
            this.checkBoxIndexCal.Location = new System.Drawing.Point(373, 263);
            this.checkBoxIndexCal.Name = "checkBoxIndexCal";
            this.checkBoxIndexCal.Size = new System.Drawing.Size(180, 46);
            this.checkBoxIndexCal.TabIndex = 33;
            this.checkBoxIndexCal.Text = "Index Cal";
            this.checkBoxIndexCal.UseVisualStyleBackColor = true;
            this.checkBoxIndexCal.Visible = false;
            this.checkBoxIndexCal.CheckedChanged += new System.EventHandler(this.checkBoxIndexCal_CheckedChanged);
            // 
            // groupBoxProcessSeq
            // 
            this.groupBoxProcessSeq.Controls.Add(this.manualSequenceControlProcessSeq);
            this.groupBoxProcessSeq.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxProcessSeq.Font = new System.Drawing.Font("맑은 고딕", 10F);
            this.groupBoxProcessSeq.Location = new System.Drawing.Point(1329, 3);
            this.groupBoxProcessSeq.Name = "groupBoxProcessSeq";
            this.groupBoxProcessSeq.Size = new System.Drawing.Size(564, 557);
            this.groupBoxProcessSeq.TabIndex = 27;
            this.groupBoxProcessSeq.TabStop = false;
            this.groupBoxProcessSeq.Text = "Process Seq";
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Font = new System.Drawing.Font("맑은 고딕", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.tabControl1.Location = new System.Drawing.Point(1329, 566);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(564, 557);
            this.tabControl1.TabIndex = 28;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.manualSequenceControl);
            this.tabPage1.Location = new System.Drawing.Point(4, 39);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(556, 514);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "MAlign";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.manualSequenceControlProbe);
            this.tabPage2.Location = new System.Drawing.Point(4, 39);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(556, 514);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Probe";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.manualSequenceControlOutAlign);
            this.tabPage3.Location = new System.Drawing.Point(4, 39);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Size = new System.Drawing.Size(556, 514);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "OutAlign";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.tableLayoutPanel5);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Font = new System.Drawing.Font("맑은 고딕", 10F);
            this.groupBox1.Location = new System.Drawing.Point(761, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(562, 557);
            this.groupBox1.TabIndex = 29;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "ImageView";
            // 
            // tableLayoutPanel5
            // 
            this.tableLayoutPanel5.ColumnCount = 1;
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel5.Controls.Add(this._ProcessCameraviewer, 0, 0);
            this.tableLayoutPanel5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel5.Location = new System.Drawing.Point(3, 30);
            this.tableLayoutPanel5.Margin = new System.Windows.Forms.Padding(4);
            this.tableLayoutPanel5.Name = "tableLayoutPanel5";
            this.tableLayoutPanel5.RowCount = 2;
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 80F));
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel5.Size = new System.Drawing.Size(556, 524);
            this.tableLayoutPanel5.TabIndex = 2;
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
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(4);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1896, 1126);
            this.tableLayoutPanel1.TabIndex = 30;
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
            this._ProcessCameraviewer.Location = new System.Drawing.Point(3, 3);
            this._ProcessCameraviewer.Name = "_ProcessCameraviewer";
            this._ProcessCameraviewer.OperatingType = QMC.Common.Vision.VisionImageViewer.OperatingTypes.Center;
            this._ProcessCameraviewer.Simulated = false;
            this._ProcessCameraviewer.Size = new System.Drawing.Size(550, 413);
            this._ProcessCameraviewer.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this._ProcessCameraviewer.TabIndex = 13;
            this._ProcessCameraviewer.TabStop = false;
            this._ProcessCameraviewer.UpdateDelayTime = 80;
            this._ProcessCameraviewer.VisibleCrossLine = true;
            // 
            // btnRotary
            // 
            this.btnRotary.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnRotary.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnRotary.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnRotary.CustomFont = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnRotary.CustomForeColor = System.Drawing.Color.Black;
            this.btnRotary.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnRotary.Font = new System.Drawing.Font("맑은 고딕", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btnRotary.ForeColor = System.Drawing.Color.Black;
            this.btnRotary.ImageSize = new System.Drawing.Size(45, 45);
            this.btnRotary.Location = new System.Drawing.Point(374, 4);
            this.btnRotary.Margin = new System.Windows.Forms.Padding(4);
            this.btnRotary.Name = "btnRotary";
            this.btnRotary.Size = new System.Drawing.Size(178, 44);
            this.btnRotary.TabIndex = 23;
            this.btnRotary.TabStop = false;
            this.btnRotary.Text = "Next Index";
            this.btnRotary.UseVisualStyleBackColor = false;
            this.btnRotary.Click += new System.EventHandler(this.btnRotary_Click);
            // 
            // ButtonManualTest
            // 
            this.ButtonManualTest.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.ButtonManualTest.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.ButtonManualTest.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.ButtonManualTest.CustomFont = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ButtonManualTest.CustomForeColor = System.Drawing.Color.Black;
            this.ButtonManualTest.Font = new System.Drawing.Font("맑은 고딕", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.ButtonManualTest.ForeColor = System.Drawing.Color.Black;
            this.ButtonManualTest.ImageSize = new System.Drawing.Size(45, 45);
            this.ButtonManualTest.Location = new System.Drawing.Point(4, 264);
            this.ButtonManualTest.Margin = new System.Windows.Forms.Padding(4);
            this.ButtonManualTest.Name = "ButtonManualTest";
            this.ButtonManualTest.Size = new System.Drawing.Size(177, 44);
            this.ButtonManualTest.TabIndex = 31;
            this.ButtonManualTest.TabStop = false;
            this.ButtonManualTest.Text = "Index Cal";
            this.ButtonManualTest.UseVisualStyleBackColor = false;
            this.ButtonManualTest.Click += new System.EventHandler(this.ButtonManualTest_Click);
            // 
            // ButtonClear
            // 
            this.ButtonClear.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.ButtonClear.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.ButtonClear.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.ButtonClear.CustomFont = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ButtonClear.CustomForeColor = System.Drawing.Color.Black;
            this.ButtonClear.Font = new System.Drawing.Font("맑은 고딕", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.ButtonClear.ForeColor = System.Drawing.Color.Black;
            this.ButtonClear.ImageSize = new System.Drawing.Size(45, 45);
            this.ButtonClear.Location = new System.Drawing.Point(189, 264);
            this.ButtonClear.Margin = new System.Windows.Forms.Padding(4);
            this.ButtonClear.Name = "ButtonClear";
            this.ButtonClear.Size = new System.Drawing.Size(177, 44);
            this.ButtonClear.TabIndex = 32;
            this.ButtonClear.TabStop = false;
            this.ButtonClear.Text = "Clear Index";
            this.ButtonClear.UseVisualStyleBackColor = false;
            this.ButtonClear.Click += new System.EventHandler(this.ButtonClear_Click);
            // 
            // btnInputMAlign
            // 
            this.btnInputMAlign.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnInputMAlign.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnInputMAlign.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnInputMAlign.CustomFont = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnInputMAlign.CustomForeColor = System.Drawing.Color.Black;
            this.btnInputMAlign.Font = new System.Drawing.Font("맑은 고딕", 10F);
            this.btnInputMAlign.ForeColor = System.Drawing.Color.Black;
            this.btnInputMAlign.ImageSize = new System.Drawing.Size(45, 45);
            this.btnInputMAlign.Location = new System.Drawing.Point(189, 472);
            this.btnInputMAlign.Margin = new System.Windows.Forms.Padding(4);
            this.btnInputMAlign.Name = "btnInputMAlign";
            this.btnInputMAlign.Size = new System.Drawing.Size(176, 42);
            this.btnInputMAlign.TabIndex = 20;
            this.btnInputMAlign.TabStop = false;
            this.btnInputMAlign.Text = "InputMAlign";
            this.btnInputMAlign.UseVisualStyleBackColor = false;
            this.btnInputMAlign.Visible = false;
            this.btnInputMAlign.Click += new System.EventHandler(this.btnInputMAlign_ClickAsync);
            // 
            // btnManualVision
            // 
            this.btnManualVision.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnManualVision.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnManualVision.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnManualVision.CustomFont = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnManualVision.CustomForeColor = System.Drawing.Color.Black;
            this.btnManualVision.Font = new System.Drawing.Font("맑은 고딕", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btnManualVision.ForeColor = System.Drawing.Color.Black;
            this.btnManualVision.ImageSize = new System.Drawing.Size(45, 45);
            this.btnManualVision.Location = new System.Drawing.Point(4, 368);
            this.btnManualVision.Margin = new System.Windows.Forms.Padding(4);
            this.btnManualVision.Name = "btnManualVision";
            this.btnManualVision.Size = new System.Drawing.Size(177, 44);
            this.btnManualVision.TabIndex = 34;
            this.btnManualVision.TabStop = false;
            this.btnManualVision.Text = "ProbeVision";
            this.btnManualVision.UseVisualStyleBackColor = false;
            this.btnManualVision.Click += new System.EventHandler(this.btnManualVision_Click);
            // 
            // manualSequenceControlProcessSeq
            // 
            this.manualSequenceControlProcessSeq.Dock = System.Windows.Forms.DockStyle.Fill;
            this.manualSequenceControlProcessSeq.Location = new System.Drawing.Point(3, 30);
            this.manualSequenceControlProcessSeq.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.manualSequenceControlProcessSeq.MinimumSize = new System.Drawing.Size(357, 300);
            this.manualSequenceControlProcessSeq.Name = "manualSequenceControlProcessSeq";
            this.manualSequenceControlProcessSeq.ParentUnit = null;
            this.manualSequenceControlProcessSeq.Size = new System.Drawing.Size(558, 524);
            this.manualSequenceControlProcessSeq.TabIndex = 14;
            // 
            // dioControl
            // 
            this.dioControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dioControl.IoSortMode = QMC.LCP_280.Process.Component.DIOControl.SortingMode.AlphabeticalKey;
            this.dioControl.Location = new System.Drawing.Point(3, 569);
            this.dioControl.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.dioControl.Name = "dioControl";
            this.dioControl.RefreshIntervalMs = 400;
            this.dioControl.Size = new System.Drawing.Size(752, 551);
            this.dioControl.TabIndex = 32;
            // 
            // teachingPositionControl
            // 
            this.teachingPositionControl.ButtonSize = new System.Drawing.Size(90, 32);
            this.teachingPositionControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.teachingPositionControl.Location = new System.Drawing.Point(3, 6);
            this.teachingPositionControl.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.teachingPositionControl.Name = "teachingPositionControl";
            this.teachingPositionControl.ShowCancelButton = true;
            this.teachingPositionControl.ShowSaveButton = true;
            this.teachingPositionControl.Size = new System.Drawing.Size(752, 551);
            this.teachingPositionControl.TabIndex = 31;
            this.teachingPositionControl.UnitName = null;
            // 
            // manualSequenceControl
            // 
            this.manualSequenceControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.manualSequenceControl.Location = new System.Drawing.Point(3, 3);
            this.manualSequenceControl.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.manualSequenceControl.MinimumSize = new System.Drawing.Size(357, 300);
            this.manualSequenceControl.Name = "manualSequenceControl";
            this.manualSequenceControl.ParentUnit = null;
            this.manualSequenceControl.Size = new System.Drawing.Size(550, 508);
            this.manualSequenceControl.TabIndex = 14;
            // 
            // manualSequenceControlProbe
            // 
            this.manualSequenceControlProbe.Dock = System.Windows.Forms.DockStyle.Top;
            this.manualSequenceControlProbe.Location = new System.Drawing.Point(3, 3);
            this.manualSequenceControlProbe.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.manualSequenceControlProbe.MinimumSize = new System.Drawing.Size(357, 300);
            this.manualSequenceControlProbe.Name = "manualSequenceControlProbe";
            this.manualSequenceControlProbe.ParentUnit = null;
            this.manualSequenceControlProbe.Size = new System.Drawing.Size(550, 372);
            this.manualSequenceControlProbe.TabIndex = 15;
            // 
            // manualSequenceControlOutAlign
            // 
            this.manualSequenceControlOutAlign.Dock = System.Windows.Forms.DockStyle.Top;
            this.manualSequenceControlOutAlign.Location = new System.Drawing.Point(0, 0);
            this.manualSequenceControlOutAlign.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.manualSequenceControlOutAlign.MinimumSize = new System.Drawing.Size(357, 300);
            this.manualSequenceControlOutAlign.Name = "manualSequenceControlOutAlign";
            this.manualSequenceControlOutAlign.ParentUnit = null;
            this.manualSequenceControlOutAlign.Size = new System.Drawing.Size(556, 372);
            this.manualSequenceControlOutAlign.TabIndex = 29;
            // 
            // Process_Working
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(144F, 144F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(1896, 1126);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "Process_Working";
            this.Text = "Process_Working";
            this.groupBoxManual.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.groupBoxProcessSeq.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.tabPage3.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.tableLayoutPanel5.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this._ProcessCameraviewer)).EndInit();
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
        private Common.IndividualMenuButton btnRotary;
        private System.Windows.Forms.Label labelsocketNumberInput;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label labelsocketNumberLAlign;
        private System.Windows.Forms.Label labelsocketNumberProbe;
        private System.Windows.Forms.Label labelsocketNumberUnload;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel5;
        private Common.IndividualMenuButton ButtonManualTest;
        private Common.IndividualMenuButton ButtonClear;
        private System.Windows.Forms.CheckBox checkBoxIndexCal;
        private Common.IndividualMenuButton btnManualVision;
    }
}