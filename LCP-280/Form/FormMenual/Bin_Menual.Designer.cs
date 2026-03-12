namespace QMC.LCP_280.Process.Unit.FormWork
{
    partial class Bin_Menual
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
            this.dioControl = new QMC.LCP_280.Process.Component.DIOControl();
            this.teachingPositionControl = new QMC.LCP_280.Process.Component.MoveToPositionControl();
            this.groupBoxImageView = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel5 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
            this._btnVisionSetting = new QMC.Common.IndividualMenuButton();
            this.buttonDataManual = new QMC.Common.IndividualMenuButton();
            this._OutputWaferCameraviewer = new QMC.Common.Vision.VisionImageViewer();
            this.groupBoxManual = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.waferMapView_OutputWafer = new QMC.LCP_280.Process.Unit.FormMain.OutputWaferCarrierControl();
            this.tableLayoutPanel6 = new System.Windows.Forms.TableLayoutPanel();
            this.btnMapping = new QMC.Common.IndividualMenuButton();
            this.btnTest = new QMC.Common.IndividualMenuButton();
            this.buttonMoveToSlot = new QMC.Common.IndividualMenuButton();
            this.comboBoxSlot = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.btnTCorrection = new QMC.Common.IndividualMenuButton();
            this.ButtonMapChange = new QMC.Common.IndividualMenuButton();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tabControlManualSeqOutputWafer = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.manualSequenceControlOutputCassette = new QMC.LCP_280.Process.Component.ManualSequenceControl();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.manualSequenceControlOutputFeeder = new QMC.LCP_280.Process.Component.ManualSequenceControl();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.manualSequenceControlOutputBinStage = new QMC.LCP_280.Process.Component.ManualSequenceControl();
            this.groupBoxImageView.SuspendLayout();
            this.tableLayoutPanel5.SuspendLayout();
            this.tableLayoutPanel4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._OutputWaferCameraviewer)).BeginInit();
            this.groupBoxManual.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.tableLayoutPanel6.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.tabControlManualSeqOutputWafer.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.SuspendLayout();
            // 
            // dioControl
            // 
            this.dioControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dioControl.IoSortMode = QMC.LCP_280.Process.Component.DIOControl.SortingMode.AlphabeticalKey;
            this.dioControl.Location = new System.Drawing.Point(2, 569);
            this.dioControl.Margin = new System.Windows.Forms.Padding(2, 6, 2, 6);
            this.dioControl.Name = "dioControl";
            this.dioControl.RefreshIntervalMs = 400;
            this.dioControl.Size = new System.Drawing.Size(849, 551);
            this.dioControl.TabIndex = 15;
            // 
            // teachingPositionControl
            // 
            this.teachingPositionControl.AxisDisplayFont = null;
            this.teachingPositionControl.ButtonSize = new System.Drawing.Size(90, 32);
            this.teachingPositionControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.teachingPositionControl.Location = new System.Drawing.Point(2, 6);
            this.teachingPositionControl.Margin = new System.Windows.Forms.Padding(2, 6, 2, 6);
            this.teachingPositionControl.Name = "teachingPositionControl";
            this.teachingPositionControl.ShowCancelButton = true;
            this.teachingPositionControl.ShowSaveButton = true;
            this.teachingPositionControl.Size = new System.Drawing.Size(849, 551);
            this.teachingPositionControl.TabIndex = 14;
            this.teachingPositionControl.UnitName = null;
            // 
            // groupBoxImageView
            // 
            this.groupBoxImageView.Controls.Add(this.tableLayoutPanel5);
            this.groupBoxImageView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxImageView.Font = new System.Drawing.Font("맑은 고딕", 10F);
            this.groupBoxImageView.Location = new System.Drawing.Point(855, 2);
            this.groupBoxImageView.Margin = new System.Windows.Forms.Padding(2);
            this.groupBoxImageView.Name = "groupBoxImageView";
            this.groupBoxImageView.Padding = new System.Windows.Forms.Padding(2);
            this.groupBoxImageView.Size = new System.Drawing.Size(564, 559);
            this.groupBoxImageView.TabIndex = 22;
            this.groupBoxImageView.TabStop = false;
            this.groupBoxImageView.Text = "ImageView";
            // 
            // tableLayoutPanel5
            // 
            this.tableLayoutPanel5.ColumnCount = 1;
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel5.Controls.Add(this.tableLayoutPanel4, 0, 1);
            this.tableLayoutPanel5.Controls.Add(this._OutputWaferCameraviewer, 0, 0);
            this.tableLayoutPanel5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel5.Location = new System.Drawing.Point(2, 29);
            this.tableLayoutPanel5.Margin = new System.Windows.Forms.Padding(4);
            this.tableLayoutPanel5.Name = "tableLayoutPanel5";
            this.tableLayoutPanel5.RowCount = 2;
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 80F));
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel5.Size = new System.Drawing.Size(560, 528);
            this.tableLayoutPanel5.TabIndex = 1;
            // 
            // tableLayoutPanel4
            // 
            this.tableLayoutPanel4.ColumnCount = 2;
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel4.Controls.Add(this._btnVisionSetting, 0, 0);
            this.tableLayoutPanel4.Controls.Add(this.buttonDataManual, 1, 0);
            this.tableLayoutPanel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel4.Location = new System.Drawing.Point(5, 427);
            this.tableLayoutPanel4.Margin = new System.Windows.Forms.Padding(5);
            this.tableLayoutPanel4.Name = "tableLayoutPanel4";
            this.tableLayoutPanel4.RowCount = 1;
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel4.Size = new System.Drawing.Size(550, 96);
            this.tableLayoutPanel4.TabIndex = 16;
            // 
            // _btnVisionSetting
            // 
            this._btnVisionSetting.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._btnVisionSetting.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this._btnVisionSetting.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this._btnVisionSetting.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this._btnVisionSetting.CustomFont = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._btnVisionSetting.CustomForeColor = System.Drawing.Color.Black;
            this._btnVisionSetting.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._btnVisionSetting.ForeColor = System.Drawing.Color.Black;
            this._btnVisionSetting.ImageSize = new System.Drawing.Size(45, 45);
            this._btnVisionSetting.Location = new System.Drawing.Point(2, 2);
            this._btnVisionSetting.Margin = new System.Windows.Forms.Padding(2);
            this._btnVisionSetting.Name = "_btnVisionSetting";
            this._btnVisionSetting.Size = new System.Drawing.Size(271, 92);
            this._btnVisionSetting.TabIndex = 34;
            this._btnVisionSetting.TabStop = false;
            this._btnVisionSetting.Text = "VisionSetting";
            this._btnVisionSetting.UseVisualStyleBackColor = false;
            this._btnVisionSetting.Visible = false;
            // 
            // buttonDataManual
            // 
            this.buttonDataManual.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonDataManual.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.buttonDataManual.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.buttonDataManual.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.buttonDataManual.CustomFont = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonDataManual.CustomForeColor = System.Drawing.Color.Black;
            this.buttonDataManual.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonDataManual.ForeColor = System.Drawing.Color.Black;
            this.buttonDataManual.ImageSize = new System.Drawing.Size(45, 45);
            this.buttonDataManual.Location = new System.Drawing.Point(277, 2);
            this.buttonDataManual.Margin = new System.Windows.Forms.Padding(2);
            this.buttonDataManual.Name = "buttonDataManual";
            this.buttonDataManual.Size = new System.Drawing.Size(271, 92);
            this.buttonDataManual.TabIndex = 35;
            this.buttonDataManual.TabStop = false;
            this.buttonDataManual.Text = "ManualData";
            this.buttonDataManual.UseVisualStyleBackColor = false;
            this.buttonDataManual.Visible = false;
            // 
            // _OutputWaferCameraviewer
            // 
            this._OutputWaferCameraviewer.BackColor = System.Drawing.Color.Black;
            this._OutputWaferCameraviewer.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this._OutputWaferCameraviewer.Camera = null;
            this._OutputWaferCameraviewer.CameraSwitch = null;
            this._OutputWaferCameraviewer.Dock = System.Windows.Forms.DockStyle.Fill;
            this._OutputWaferCameraviewer.FrameRate = 1D;
            this._OutputWaferCameraviewer.InputImage = null;
            this._OutputWaferCameraviewer.IsViewCustomizedImage = false;
            this._OutputWaferCameraviewer.Location = new System.Drawing.Point(2, 2);
            this._OutputWaferCameraviewer.Margin = new System.Windows.Forms.Padding(2);
            this._OutputWaferCameraviewer.Name = "_OutputWaferCameraviewer";
            this._OutputWaferCameraviewer.OperatingType = QMC.Common.Vision.VisionImageViewer.OperatingTypes.Center;
            this._OutputWaferCameraviewer.Simulated = false;
            this._OutputWaferCameraviewer.Size = new System.Drawing.Size(556, 418);
            this._OutputWaferCameraviewer.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this._OutputWaferCameraviewer.TabIndex = 12;
            this._OutputWaferCameraviewer.TabStop = false;
            this._OutputWaferCameraviewer.UpdateDelayTime = 80;
            this._OutputWaferCameraviewer.VisibleCrossLine = true;
            // 
            // groupBoxManual
            // 
            this.groupBoxManual.Controls.Add(this.tableLayoutPanel2);
            this.groupBoxManual.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxManual.Font = new System.Drawing.Font("맑은 고딕", 10F);
            this.groupBoxManual.Location = new System.Drawing.Point(855, 565);
            this.groupBoxManual.Margin = new System.Windows.Forms.Padding(2);
            this.groupBoxManual.Name = "groupBoxManual";
            this.groupBoxManual.Padding = new System.Windows.Forms.Padding(2);
            this.groupBoxManual.Size = new System.Drawing.Size(564, 559);
            this.groupBoxManual.TabIndex = 21;
            this.groupBoxManual.TabStop = false;
            this.groupBoxManual.Text = "Manual";
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 2;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 60.68335F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 39.31664F));
            this.tableLayoutPanel2.Controls.Add(this.waferMapView_OutputWafer, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.tableLayoutPanel6, 1, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(2, 29);
            this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(4);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(560, 528);
            this.tableLayoutPanel2.TabIndex = 21;
            // 
            // waferMapView_OutputWafer
            // 
            this.waferMapView_OutputWafer.BackColor = System.Drawing.Color.White;
            this.waferMapView_OutputWafer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.waferMapView_OutputWafer.Location = new System.Drawing.Point(2, 9);
            this.waferMapView_OutputWafer.Margin = new System.Windows.Forms.Padding(2, 9, 2, 9);
            this.waferMapView_OutputWafer.Name = "waferMapView_OutputWafer";
            this.waferMapView_OutputWafer.Size = new System.Drawing.Size(335, 510);
            this.waferMapView_OutputWafer.TabIndex = 23;
            // 
            // tableLayoutPanel6
            // 
            this.tableLayoutPanel6.ColumnCount = 1;
            this.tableLayoutPanel6.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel6.Controls.Add(this.btnMapping, 0, 3);
            this.tableLayoutPanel6.Controls.Add(this.btnTest, 0, 4);
            this.tableLayoutPanel6.Controls.Add(this.buttonMoveToSlot, 0, 2);
            this.tableLayoutPanel6.Controls.Add(this.comboBoxSlot, 0, 1);
            this.tableLayoutPanel6.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel6.Controls.Add(this.tableLayoutPanel3, 0, 6);
            this.tableLayoutPanel6.Location = new System.Drawing.Point(341, 2);
            this.tableLayoutPanel6.Margin = new System.Windows.Forms.Padding(2);
            this.tableLayoutPanel6.Name = "tableLayoutPanel6";
            this.tableLayoutPanel6.RowCount = 7;
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 8.602151F));
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10.75269F));
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.12903F));
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.12903F));
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.12903F));
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.12903F));
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.12903F));
            this.tableLayoutPanel6.Size = new System.Drawing.Size(217, 524);
            this.tableLayoutPanel6.TabIndex = 22;
            // 
            // btnMapping
            // 
            this.btnMapping.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnMapping.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnMapping.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnMapping.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnMapping.CustomForeColor = System.Drawing.Color.Black;
            this.btnMapping.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnMapping.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnMapping.ForeColor = System.Drawing.Color.Black;
            this.btnMapping.ImageSize = new System.Drawing.Size(45, 45);
            this.btnMapping.Location = new System.Drawing.Point(2, 189);
            this.btnMapping.Margin = new System.Windows.Forms.Padding(2, 4, 2, 4);
            this.btnMapping.Name = "btnMapping";
            this.btnMapping.Size = new System.Drawing.Size(213, 76);
            this.btnMapping.TabIndex = 22;
            this.btnMapping.TabStop = false;
            this.btnMapping.Text = "Mapping";
            this.btnMapping.UseVisualStyleBackColor = false;
            this.btnMapping.Click += new System.EventHandler(this.btnMapping_Click);
            // 
            // btnTest
            // 
            this.btnTest.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnTest.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnTest.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnTest.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnTest.CustomForeColor = System.Drawing.Color.Black;
            this.btnTest.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnTest.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnTest.ForeColor = System.Drawing.Color.Black;
            this.btnTest.ImageSize = new System.Drawing.Size(45, 45);
            this.btnTest.Location = new System.Drawing.Point(2, 274);
            this.btnTest.Margin = new System.Windows.Forms.Padding(2, 5, 2, 5);
            this.btnTest.Name = "btnTest";
            this.btnTest.Size = new System.Drawing.Size(213, 74);
            this.btnTest.TabIndex = 20;
            this.btnTest.TabStop = false;
            this.btnTest.Text = "Test";
            this.btnTest.UseVisualStyleBackColor = false;
            // 
            // buttonMoveToSlot
            // 
            this.buttonMoveToSlot.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.buttonMoveToSlot.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.buttonMoveToSlot.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.buttonMoveToSlot.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.buttonMoveToSlot.CustomForeColor = System.Drawing.Color.Black;
            this.buttonMoveToSlot.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonMoveToSlot.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.buttonMoveToSlot.ForeColor = System.Drawing.Color.Black;
            this.buttonMoveToSlot.ImageSize = new System.Drawing.Size(45, 45);
            this.buttonMoveToSlot.Location = new System.Drawing.Point(2, 106);
            this.buttonMoveToSlot.Margin = new System.Windows.Forms.Padding(2, 5, 2, 5);
            this.buttonMoveToSlot.Name = "buttonMoveToSlot";
            this.buttonMoveToSlot.Size = new System.Drawing.Size(213, 74);
            this.buttonMoveToSlot.TabIndex = 18;
            this.buttonMoveToSlot.TabStop = false;
            this.buttonMoveToSlot.Text = "Move Slot";
            this.buttonMoveToSlot.UseVisualStyleBackColor = false;
            this.buttonMoveToSlot.Click += new System.EventHandler(this.buttonMoveToSlot_Click);
            // 
            // comboBoxSlot
            // 
            this.comboBoxSlot.Dock = System.Windows.Forms.DockStyle.Fill;
            this.comboBoxSlot.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxSlot.Font = new System.Drawing.Font("맑은 고딕", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.comboBoxSlot.FormattingEnabled = true;
            this.comboBoxSlot.ItemHeight = 30;
            this.comboBoxSlot.Items.AddRange(new object[] {
            "1",
            "2",
            "3",
            "4",
            "5",
            "6",
            "7",
            "8",
            "9",
            "10",
            "11",
            "12",
            "13",
            "14",
            "15",
            "16",
            "17",
            "18",
            "19",
            "20",
            "21",
            "22",
            "23",
            "24",
            "25"});
            this.comboBoxSlot.Location = new System.Drawing.Point(3, 48);
            this.comboBoxSlot.Name = "comboBoxSlot";
            this.comboBoxSlot.Size = new System.Drawing.Size(211, 38);
            this.comboBoxSlot.TabIndex = 19;
            this.comboBoxSlot.SelectedIndexChanged += new System.EventHandler(this.comboBoxSlot_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label1.Font = new System.Drawing.Font("맑은 고딕", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label1.Location = new System.Drawing.Point(3, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(211, 45);
            this.label1.TabIndex = 21;
            this.label1.Text = "Slot";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 2;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 79.56204F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20.43796F));
            this.tableLayoutPanel3.Controls.Add(this.btnTCorrection, 0, 0);
            this.tableLayoutPanel3.Controls.Add(this.ButtonMapChange, 0, 1);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(3, 440);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 2;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(211, 81);
            this.tableLayoutPanel3.TabIndex = 19;
            // 
            // btnTCorrection
            // 
            this.btnTCorrection.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnTCorrection.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnTCorrection.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnTCorrection.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnTCorrection.CustomForeColor = System.Drawing.Color.Black;
            this.btnTCorrection.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnTCorrection.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnTCorrection.ForeColor = System.Drawing.Color.Black;
            this.btnTCorrection.ImageSize = new System.Drawing.Size(45, 45);
            this.btnTCorrection.Location = new System.Drawing.Point(2, 4);
            this.btnTCorrection.Margin = new System.Windows.Forms.Padding(2, 4, 2, 4);
            this.btnTCorrection.Name = "btnTCorrection";
            this.btnTCorrection.Size = new System.Drawing.Size(163, 32);
            this.btnTCorrection.TabIndex = 18;
            this.btnTCorrection.TabStop = false;
            this.btnTCorrection.Text = "T-Correction";
            this.btnTCorrection.UseVisualStyleBackColor = false;
            this.btnTCorrection.Click += new System.EventHandler(this.btnTCorrection_Click);
            // 
            // ButtonMapChange
            // 
            this.ButtonMapChange.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.ButtonMapChange.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.ButtonMapChange.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.ButtonMapChange.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.ButtonMapChange.CustomForeColor = System.Drawing.Color.Black;
            this.ButtonMapChange.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ButtonMapChange.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.ButtonMapChange.ForeColor = System.Drawing.Color.Black;
            this.ButtonMapChange.ImageSize = new System.Drawing.Size(45, 45);
            this.ButtonMapChange.Location = new System.Drawing.Point(2, 44);
            this.ButtonMapChange.Margin = new System.Windows.Forms.Padding(2, 4, 2, 4);
            this.ButtonMapChange.Name = "ButtonMapChange";
            this.ButtonMapChange.Size = new System.Drawing.Size(163, 33);
            this.ButtonMapChange.TabIndex = 18;
            this.ButtonMapChange.TabStop = false;
            this.ButtonMapChange.Text = "StageMap";
            this.ButtonMapChange.UseVisualStyleBackColor = false;
            this.ButtonMapChange.Visible = false;
            this.ButtonMapChange.Click += new System.EventHandler(this.ButtonMapChange_Click);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 45F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.Controls.Add(this.tabControlManualSeqOutputWafer, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.groupBoxImageView, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.teachingPositionControl, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.groupBoxManual, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.dioControl, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(4);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1896, 1126);
            this.tableLayoutPanel1.TabIndex = 24;
            // 
            // tabControlManualSeqOutputWafer
            // 
            this.tabControlManualSeqOutputWafer.Controls.Add(this.tabPage1);
            this.tabControlManualSeqOutputWafer.Controls.Add(this.tabPage2);
            this.tabControlManualSeqOutputWafer.Controls.Add(this.tabPage3);
            this.tabControlManualSeqOutputWafer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControlManualSeqOutputWafer.Font = new System.Drawing.Font("맑은 고딕", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.tabControlManualSeqOutputWafer.Location = new System.Drawing.Point(1423, 2);
            this.tabControlManualSeqOutputWafer.Margin = new System.Windows.Forms.Padding(2);
            this.tabControlManualSeqOutputWafer.Name = "tabControlManualSeqOutputWafer";
            this.tabControlManualSeqOutputWafer.SelectedIndex = 0;
            this.tabControlManualSeqOutputWafer.Size = new System.Drawing.Size(471, 559);
            this.tabControlManualSeqOutputWafer.TabIndex = 23;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.manualSequenceControlOutputCassette);
            this.tabPage1.Location = new System.Drawing.Point(4, 39);
            this.tabPage1.Margin = new System.Windows.Forms.Padding(2);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(2);
            this.tabPage1.Size = new System.Drawing.Size(463, 516);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Cassette";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // manualSequenceControlOutputCassette
            // 
            this.manualSequenceControlOutputCassette.Dock = System.Windows.Forms.DockStyle.Fill;
            this.manualSequenceControlOutputCassette.Location = new System.Drawing.Point(2, 2);
            this.manualSequenceControlOutputCassette.Margin = new System.Windows.Forms.Padding(2);
            this.manualSequenceControlOutputCassette.MinimumSize = new System.Drawing.Size(358, 150);
            this.manualSequenceControlOutputCassette.Name = "manualSequenceControlOutputCassette";
            this.manualSequenceControlOutputCassette.ParentUnit = null;
            this.manualSequenceControlOutputCassette.Size = new System.Drawing.Size(459, 512);
            this.manualSequenceControlOutputCassette.TabIndex = 14;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.manualSequenceControlOutputFeeder);
            this.tabPage2.Location = new System.Drawing.Point(4, 39);
            this.tabPage2.Margin = new System.Windows.Forms.Padding(2);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(2);
            this.tabPage2.Size = new System.Drawing.Size(463, 516);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Feeder";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // manualSequenceControlOutputFeeder
            // 
            this.manualSequenceControlOutputFeeder.Dock = System.Windows.Forms.DockStyle.Fill;
            this.manualSequenceControlOutputFeeder.Location = new System.Drawing.Point(2, 2);
            this.manualSequenceControlOutputFeeder.Margin = new System.Windows.Forms.Padding(2);
            this.manualSequenceControlOutputFeeder.MinimumSize = new System.Drawing.Size(358, 300);
            this.manualSequenceControlOutputFeeder.Name = "manualSequenceControlOutputFeeder";
            this.manualSequenceControlOutputFeeder.ParentUnit = null;
            this.manualSequenceControlOutputFeeder.Size = new System.Drawing.Size(459, 512);
            this.manualSequenceControlOutputFeeder.TabIndex = 15;
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.manualSequenceControlOutputBinStage);
            this.tabPage3.Location = new System.Drawing.Point(4, 39);
            this.tabPage3.Margin = new System.Windows.Forms.Padding(2);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(2);
            this.tabPage3.Size = new System.Drawing.Size(463, 516);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "Stage";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // manualSequenceControlOutputBinStage
            // 
            this.manualSequenceControlOutputBinStage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.manualSequenceControlOutputBinStage.Location = new System.Drawing.Point(2, 2);
            this.manualSequenceControlOutputBinStage.Margin = new System.Windows.Forms.Padding(2);
            this.manualSequenceControlOutputBinStage.MinimumSize = new System.Drawing.Size(358, 300);
            this.manualSequenceControlOutputBinStage.Name = "manualSequenceControlOutputBinStage";
            this.manualSequenceControlOutputBinStage.ParentUnit = null;
            this.manualSequenceControlOutputBinStage.Size = new System.Drawing.Size(459, 512);
            this.manualSequenceControlOutputBinStage.TabIndex = 16;
            // 
            // Bin_Menual
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(144F, 144F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(1896, 1126);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Margin = new System.Windows.Forms.Padding(2, 4, 2, 4);
            this.Name = "Bin_Menual";
            this.Text = "WaferBin_Working";
            this.groupBoxImageView.ResumeLayout(false);
            this.tableLayoutPanel5.ResumeLayout(false);
            this.tableLayoutPanel4.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this._OutputWaferCameraviewer)).EndInit();
            this.groupBoxManual.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel6.ResumeLayout(false);
            this.tableLayoutPanel6.PerformLayout();
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tabControlManualSeqOutputWafer.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.tabPage3.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private Component.DIOControl dioControl;
        private Component.MoveToPositionControl teachingPositionControl;
        private System.Windows.Forms.GroupBox groupBoxImageView;
        private Common.Vision.VisionImageViewer _OutputWaferCameraviewer;
        private System.Windows.Forms.GroupBox groupBoxManual;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel5;
        private System.Windows.Forms.TabControl tabControlManualSeqOutputWafer;
        private System.Windows.Forms.TabPage tabPage1;
        private Component.ManualSequenceControl manualSequenceControlOutputCassette;
        private System.Windows.Forms.TabPage tabPage2;
        private Component.ManualSequenceControl manualSequenceControlOutputFeeder;
        private System.Windows.Forms.TabPage tabPage3;
        private Component.ManualSequenceControl manualSequenceControlOutputBinStage;
        //private Component.WaferMapView waferMapView_OutputWafer;
        private QMC.LCP_280.Process.Unit.FormMain.OutputWaferCarrierControl waferMapView_OutputWafer;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel6;
        private Common.IndividualMenuButton btnMapping;
        private Common.IndividualMenuButton btnTest;
        private Common.IndividualMenuButton buttonMoveToSlot;
        private System.Windows.Forms.ComboBox comboBoxSlot;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private Common.IndividualMenuButton btnTCorrection;
        private Common.IndividualMenuButton ButtonMapChange;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel4;
        private Common.IndividualMenuButton _btnVisionSetting;
        private Common.IndividualMenuButton buttonDataManual;
    }
}