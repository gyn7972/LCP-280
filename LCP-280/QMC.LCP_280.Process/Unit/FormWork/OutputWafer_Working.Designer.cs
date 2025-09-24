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
            this.manualSequenceControlCassette = new QMC.LCP_280.Process.Component.ManualSequenceControl();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBoxImageView = new System.Windows.Forms.GroupBox();
            this._OutputWaferCameraviewer = new QMC.Common.Vision.VisionImageViewer();
            this.groupBoxManual = new System.Windows.Forms.GroupBox();
            this.waferMapView = new QMC.LCP_280.Process.Component.WaferMapView();
            this.btnMapping = new QMC.Common.IndividualMenuButton();
            this.groupBoxOutputStageSeq = new System.Windows.Forms.GroupBox();
            this.manualSequenceControlOutputStage = new QMC.LCP_280.Process.Component.ManualSequenceControl();
            this.groupBox1.SuspendLayout();
            this.groupBoxImageView.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._OutputWaferCameraviewer)).BeginInit();
            this.groupBoxManual.SuspendLayout();
            this.groupBoxOutputStageSeq.SuspendLayout();
            this.SuspendLayout();
            // 
            // dioControl
            // 
            this.dioControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dioControl.IoSortMode = QMC.LCP_280.Process.Component.DIOControl.SortingMode.AlphabeticalKey;
            this.dioControl.Location = new System.Drawing.Point(8, 416);
            this.dioControl.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
            this.dioControl.Name = "dioControl";
            this.dioControl.RefreshIntervalMs = 400;
            this.dioControl.Size = new System.Drawing.Size(615, 400);
            this.dioControl.TabIndex = 15;
            // 
            // teachingPositionControl
            // 
            this.teachingPositionControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.teachingPositionControl.ButtonSize = new System.Drawing.Size(90, 32);
            this.teachingPositionControl.Location = new System.Drawing.Point(8, 6);
            this.teachingPositionControl.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
            this.teachingPositionControl.Name = "teachingPositionControl";
            this.teachingPositionControl.ShowCancelButton = true;
            this.teachingPositionControl.ShowSaveButton = true;
            this.teachingPositionControl.Size = new System.Drawing.Size(615, 400);
            this.teachingPositionControl.TabIndex = 14;
            this.teachingPositionControl.UnitName = null;
            // 
            // manualSequenceControlCassette
            // 
            this.manualSequenceControlCassette.Dock = System.Windows.Forms.DockStyle.Top;
            this.manualSequenceControlCassette.Location = new System.Drawing.Point(3, 26);
            this.manualSequenceControlCassette.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.manualSequenceControlCassette.MinimumSize = new System.Drawing.Size(297, 250);
            this.manualSequenceControlCassette.Name = "manualSequenceControlCassette";
            this.manualSequenceControlCassette.ParentUnit = null;
            this.manualSequenceControlCassette.Size = new System.Drawing.Size(389, 318);
            this.manualSequenceControlCassette.TabIndex = 13;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.manualSequenceControlCassette);
            this.groupBox1.Font = new System.Drawing.Font("맑은 고딕", 10.2F, System.Drawing.FontStyle.Bold);
            this.groupBox1.Location = new System.Drawing.Point(1027, 6);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(395, 335);
            this.groupBox1.TabIndex = 16;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Output Wafer Seq";
            // 
            // groupBoxImageView
            // 
            this.groupBoxImageView.Controls.Add(this._OutputWaferCameraviewer);
            this.groupBoxImageView.Font = new System.Drawing.Font("맑은 고딕", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.groupBoxImageView.Location = new System.Drawing.Point(629, 6);
            this.groupBoxImageView.Name = "groupBoxImageView";
            this.groupBoxImageView.Size = new System.Drawing.Size(392, 400);
            this.groupBoxImageView.TabIndex = 22;
            this.groupBoxImageView.TabStop = false;
            this.groupBoxImageView.Text = "ImageView";
            // 
            // _OutputWaferCameraviewer
            // 
            this._OutputWaferCameraviewer.BackColor = System.Drawing.Color.Black;
            this._OutputWaferCameraviewer.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this._OutputWaferCameraviewer.Camera = null;
            this._OutputWaferCameraviewer.CameraSwitch = null;
            this._OutputWaferCameraviewer.Dock = System.Windows.Forms.DockStyle.Top;
            this._OutputWaferCameraviewer.FrameRate = 1D;
            this._OutputWaferCameraviewer.InputImage = null;
            this._OutputWaferCameraviewer.IsViewCustomizedImage = false;
            this._OutputWaferCameraviewer.Location = new System.Drawing.Point(3, 26);
            this._OutputWaferCameraviewer.Name = "_OutputWaferCameraviewer";
            this._OutputWaferCameraviewer.OperatingType = QMC.Common.Vision.VisionImageViewer.OperatingTypes.Center;
            this._OutputWaferCameraviewer.Simulated = false;
            this._OutputWaferCameraviewer.Size = new System.Drawing.Size(386, 324);
            this._OutputWaferCameraviewer.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this._OutputWaferCameraviewer.TabIndex = 12;
            this._OutputWaferCameraviewer.TabStop = false;
            this._OutputWaferCameraviewer.UpdateDelayTime = 80;
            this._OutputWaferCameraviewer.VisibleCrossLine = true;
            // 
            // groupBoxManual
            // 
            this.groupBoxManual.Controls.Add(this.waferMapView);
            this.groupBoxManual.Controls.Add(this.btnMapping);
            this.groupBoxManual.Font = new System.Drawing.Font("맑은 고딕", 10.2F, System.Drawing.FontStyle.Bold);
            this.groupBoxManual.Location = new System.Drawing.Point(629, 416);
            this.groupBoxManual.Name = "groupBoxManual";
            this.groupBoxManual.Size = new System.Drawing.Size(392, 400);
            this.groupBoxManual.TabIndex = 21;
            this.groupBoxManual.TabStop = false;
            this.groupBoxManual.Text = "Manual";
            // 
            // waferMapView
            // 
            this.waferMapView.Location = new System.Drawing.Point(6, 25);
            this.waferMapView.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.waferMapView.Name = "waferMapView";
            this.waferMapView.Size = new System.Drawing.Size(157, 205);
            this.waferMapView.TabIndex = 16;
            // 
            // btnMapping
            // 
            this.btnMapping.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnMapping.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnMapping.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnMapping.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnMapping.CustomForeColor = System.Drawing.Color.Black;
            this.btnMapping.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnMapping.ForeColor = System.Drawing.Color.Black;
            this.btnMapping.ImageSize = new System.Drawing.Size(45, 45);
            this.btnMapping.Location = new System.Drawing.Point(170, 25);
            this.btnMapping.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnMapping.Name = "btnMapping";
            this.btnMapping.Size = new System.Drawing.Size(103, 44);
            this.btnMapping.TabIndex = 17;
            this.btnMapping.TabStop = false;
            this.btnMapping.Text = "Mapping";
            this.btnMapping.UseVisualStyleBackColor = false;
            // 
            // groupBoxOutputStageSeq
            // 
            this.groupBoxOutputStageSeq.Controls.Add(this.manualSequenceControlOutputStage);
            this.groupBoxOutputStageSeq.Font = new System.Drawing.Font("맑은 고딕", 10.2F, System.Drawing.FontStyle.Bold);
            this.groupBoxOutputStageSeq.Location = new System.Drawing.Point(1027, 416);
            this.groupBoxOutputStageSeq.Name = "groupBoxOutputStageSeq";
            this.groupBoxOutputStageSeq.Size = new System.Drawing.Size(395, 335);
            this.groupBoxOutputStageSeq.TabIndex = 23;
            this.groupBoxOutputStageSeq.TabStop = false;
            this.groupBoxOutputStageSeq.Text = "OutputStage Manual Seq";
            // 
            // manualSequenceControlOutputStage
            // 
            this.manualSequenceControlOutputStage.Dock = System.Windows.Forms.DockStyle.Top;
            this.manualSequenceControlOutputStage.Location = new System.Drawing.Point(3, 26);
            this.manualSequenceControlOutputStage.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.manualSequenceControlOutputStage.MinimumSize = new System.Drawing.Size(297, 250);
            this.manualSequenceControlOutputStage.Name = "manualSequenceControlOutputStage";
            this.manualSequenceControlOutputStage.ParentUnit = null;
            this.manualSequenceControlOutputStage.Size = new System.Drawing.Size(389, 316);
            this.manualSequenceControlOutputStage.TabIndex = 13;
            // 
            // OutputWafer_Working
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(120F, 120F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(1580, 939);
            this.Controls.Add(this.groupBoxOutputStageSeq);
            this.Controls.Add(this.groupBoxImageView);
            this.Controls.Add(this.groupBoxManual);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.dioControl);
            this.Controls.Add(this.teachingPositionControl);
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "OutputWafer_Working";
            this.Text = "WaferBin_Working";
            this.groupBox1.ResumeLayout(false);
            this.groupBoxImageView.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this._OutputWaferCameraviewer)).EndInit();
            this.groupBoxManual.ResumeLayout(false);
            this.groupBoxOutputStageSeq.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private Component.DIOControl dioControl;
        private Component.TeachingPositionControl teachingPositionControl;
        private Component.ManualSequenceControl manualSequenceControlCassette;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBoxImageView;
        private Common.Vision.VisionImageViewer _OutputWaferCameraviewer;
        private System.Windows.Forms.GroupBox groupBoxManual;
        private Component.WaferMapView waferMapView;
        private Common.IndividualMenuButton btnMapping;
        private System.Windows.Forms.GroupBox groupBoxOutputStageSeq;
        private Component.ManualSequenceControl manualSequenceControlOutputStage;
    }
}