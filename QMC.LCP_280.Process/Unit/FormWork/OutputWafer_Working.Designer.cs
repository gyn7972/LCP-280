namespace QMC.LCP_280.Process.Unit.FormWork
{
    partial class OutputWafer_Working
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
            this.teachingPositionControl = new QMC.LCP_280.Process.Component.TeachingPositionControl();
            this.groupBoxImageView = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel5 = new System.Windows.Forms.TableLayoutPanel();
            this._OutputWaferCameraviewer = new QMC.Common.Vision.VisionImageViewer();
            this.groupBoxManual = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
            this.waferMapView_OutputWafer = new QMC.LCP_280.Process.Component.WaferMapView();
            this.btnMapping = new QMC.Common.IndividualMenuButton();
            this.ButtonMapChange = new QMC.Common.IndividualMenuButton();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tabControlManualSeqOutputWafer = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.manualSequenceControlOutputCassette = new QMC.LCP_280.Process.Component.ManualSequenceControl();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.manualSequenceControlOutputFeeder = new QMC.LCP_280.Process.Component.ManualSequenceControl();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.manualSequenceControlOutputBinStage = new QMC.LCP_280.Process.Component.ManualSequenceControl();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.btnTCorrection = new QMC.Common.IndividualMenuButton();
            this.groupBoxImageView.SuspendLayout();
            this.tableLayoutPanel5.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._OutputWaferCameraviewer)).BeginInit();
            this.groupBoxManual.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.tableLayoutPanel4.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.tabControlManualSeqOutputWafer.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
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
            this.dioControl.Size = new System.Drawing.Size(754, 551);
            this.dioControl.TabIndex = 15;
            // 
            // teachingPositionControl
            // 
            this.teachingPositionControl.ButtonSize = new System.Drawing.Size(90, 32);
            this.teachingPositionControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.teachingPositionControl.Location = new System.Drawing.Point(2, 6);
            this.teachingPositionControl.Margin = new System.Windows.Forms.Padding(2, 6, 2, 6);
            this.teachingPositionControl.Name = "teachingPositionControl";
            this.teachingPositionControl.ShowCancelButton = true;
            this.teachingPositionControl.ShowSaveButton = true;
            this.teachingPositionControl.Size = new System.Drawing.Size(754, 551);
            this.teachingPositionControl.TabIndex = 14;
            this.teachingPositionControl.UnitName = null;
            // 
            // groupBoxImageView
            // 
            this.groupBoxImageView.Controls.Add(this.tableLayoutPanel5);
            this.groupBoxImageView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxImageView.Font = new System.Drawing.Font("맑은 고딕", 10F);
            this.groupBoxImageView.Location = new System.Drawing.Point(760, 2);
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
            this.groupBoxManual.Location = new System.Drawing.Point(760, 565);
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
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.Controls.Add(this.tableLayoutPanel4, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.ButtonMapChange, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.tableLayoutPanel3, 1, 1);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(2, 29);
            this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(4);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 2;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 80F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(560, 528);
            this.tableLayoutPanel2.TabIndex = 21;
            // 
            // tableLayoutPanel4
            // 
            this.tableLayoutPanel4.ColumnCount = 1;
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel4.Controls.Add(this.waferMapView_OutputWafer, 0, 0);
            this.tableLayoutPanel4.Controls.Add(this.btnMapping, 0, 1);
            this.tableLayoutPanel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel4.Location = new System.Drawing.Point(4, 4);
            this.tableLayoutPanel4.Margin = new System.Windows.Forms.Padding(4);
            this.tableLayoutPanel4.Name = "tableLayoutPanel4";
            this.tableLayoutPanel4.RowCount = 2;
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 87F));
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 13F));
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel4.Size = new System.Drawing.Size(272, 414);
            this.tableLayoutPanel4.TabIndex = 18;
            // 
            // waferMapView_OutputWafer
            // 
            this.waferMapView_OutputWafer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.waferMapView_OutputWafer.Location = new System.Drawing.Point(2, 4);
            this.waferMapView_OutputWafer.Margin = new System.Windows.Forms.Padding(2, 4, 2, 4);
            this.waferMapView_OutputWafer.Name = "waferMapView_OutputWafer";
            this.waferMapView_OutputWafer.Size = new System.Drawing.Size(268, 352);
            this.waferMapView_OutputWafer.TabIndex = 16;
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
            this.btnMapping.Location = new System.Drawing.Point(2, 364);
            this.btnMapping.Margin = new System.Windows.Forms.Padding(2, 4, 2, 4);
            this.btnMapping.Name = "btnMapping";
            this.btnMapping.Size = new System.Drawing.Size(268, 46);
            this.btnMapping.TabIndex = 17;
            this.btnMapping.TabStop = false;
            this.btnMapping.Text = "Mapping";
            this.btnMapping.UseVisualStyleBackColor = false;
            this.btnMapping.Click += new System.EventHandler(this.btnMapping_Click);
            // 
            // ButtonMapChange
            // 
            this.ButtonMapChange.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.ButtonMapChange.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.ButtonMapChange.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.ButtonMapChange.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.ButtonMapChange.CustomForeColor = System.Drawing.Color.Black;
            this.ButtonMapChange.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.ButtonMapChange.ForeColor = System.Drawing.Color.Black;
            this.ButtonMapChange.ImageSize = new System.Drawing.Size(45, 45);
            this.ButtonMapChange.Location = new System.Drawing.Point(282, 4);
            this.ButtonMapChange.Margin = new System.Windows.Forms.Padding(2, 4, 2, 4);
            this.ButtonMapChange.Name = "ButtonMapChange";
            this.ButtonMapChange.Size = new System.Drawing.Size(268, 46);
            this.ButtonMapChange.TabIndex = 18;
            this.ButtonMapChange.TabStop = false;
            this.ButtonMapChange.Text = "StageMap";
            this.ButtonMapChange.UseVisualStyleBackColor = false;
            this.ButtonMapChange.Click += new System.EventHandler(this.ButtonMapChange_Click);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 40F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30F));
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
            this.tabControlManualSeqOutputWafer.Location = new System.Drawing.Point(1328, 2);
            this.tabControlManualSeqOutputWafer.Margin = new System.Windows.Forms.Padding(2);
            this.tabControlManualSeqOutputWafer.Name = "tabControlManualSeqOutputWafer";
            this.tabControlManualSeqOutputWafer.SelectedIndex = 0;
            this.tabControlManualSeqOutputWafer.Size = new System.Drawing.Size(566, 559);
            this.tabControlManualSeqOutputWafer.TabIndex = 23;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.manualSequenceControlOutputCassette);
            this.tabPage1.Location = new System.Drawing.Point(4, 39);
            this.tabPage1.Margin = new System.Windows.Forms.Padding(2);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(2);
            this.tabPage1.Size = new System.Drawing.Size(558, 516);
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
            this.manualSequenceControlOutputCassette.Size = new System.Drawing.Size(554, 512);
            this.manualSequenceControlOutputCassette.TabIndex = 14;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.manualSequenceControlOutputFeeder);
            this.tabPage2.Location = new System.Drawing.Point(4, 39);
            this.tabPage2.Margin = new System.Windows.Forms.Padding(2);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(2);
            this.tabPage2.Size = new System.Drawing.Size(558, 516);
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
            this.manualSequenceControlOutputFeeder.Size = new System.Drawing.Size(554, 512);
            this.manualSequenceControlOutputFeeder.TabIndex = 15;
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.manualSequenceControlOutputBinStage);
            this.tabPage3.Location = new System.Drawing.Point(4, 39);
            this.tabPage3.Margin = new System.Windows.Forms.Padding(2);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(2);
            this.tabPage3.Size = new System.Drawing.Size(558, 516);
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
            this.manualSequenceControlOutputBinStage.Size = new System.Drawing.Size(554, 512);
            this.manualSequenceControlOutputBinStage.TabIndex = 16;
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 2;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 79.56204F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20.43796F));
            this.tableLayoutPanel3.Controls.Add(this.btnTCorrection, 0, 0);
            this.tableLayoutPanel3.Location = new System.Drawing.Point(283, 425);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 2;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(274, 100);
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
            this.btnTCorrection.Size = new System.Drawing.Size(214, 42);
            this.btnTCorrection.TabIndex = 18;
            this.btnTCorrection.TabStop = false;
            this.btnTCorrection.Text = "T-Correction";
            this.btnTCorrection.UseVisualStyleBackColor = false;
            this.btnTCorrection.Click += new System.EventHandler(this.btnTCorrection_Click);
            // 
            // OutputWafer_Working
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(144F, 144F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(1896, 1126);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Margin = new System.Windows.Forms.Padding(2, 4, 2, 4);
            this.Name = "OutputWafer_Working";
            this.Text = "WaferBin_Working";
            this.groupBoxImageView.ResumeLayout(false);
            this.tableLayoutPanel5.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this._OutputWaferCameraviewer)).EndInit();
            this.groupBoxManual.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel4.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tabControlManualSeqOutputWafer.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.tabPage3.ResumeLayout(false);
            this.tableLayoutPanel3.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private Component.DIOControl dioControl;
        private Component.TeachingPositionControl teachingPositionControl;
        private System.Windows.Forms.GroupBox groupBoxImageView;
        private Common.Vision.VisionImageViewer _OutputWaferCameraviewer;
        private System.Windows.Forms.GroupBox groupBoxManual;
        private Component.WaferMapView waferMapView_OutputWafer;
        private Common.IndividualMenuButton btnMapping;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel4;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel5;
        private System.Windows.Forms.TabControl tabControlManualSeqOutputWafer;
        private System.Windows.Forms.TabPage tabPage1;
        private Component.ManualSequenceControl manualSequenceControlOutputCassette;
        private System.Windows.Forms.TabPage tabPage2;
        private Component.ManualSequenceControl manualSequenceControlOutputFeeder;
        private System.Windows.Forms.TabPage tabPage3;
        private Component.ManualSequenceControl manualSequenceControlOutputBinStage;
        private Common.IndividualMenuButton ButtonMapChange;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private Common.IndividualMenuButton btnTCorrection;
    }
}