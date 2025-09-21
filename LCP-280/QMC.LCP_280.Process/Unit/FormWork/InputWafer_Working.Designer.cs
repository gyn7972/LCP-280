using QMC.LCP_280.Process.Component;

namespace QMC.LCP_280.Process.Unit.FormWork
{
    partial class InputWafer_Working
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
            this.groupBoxCassetteLifterSeq = new System.Windows.Forms.GroupBox();
            this.groupBoxManual = new System.Windows.Forms.GroupBox();
            this.groupBoxInputStageSeq = new System.Windows.Forms.GroupBox();
            this.groupBoxImageView = new System.Windows.Forms.GroupBox();
            this._InputWaferCameraviewer = new QMC.Common.Vision.VisionImageViewer();
            this.btnMapping = new QMC.Common.IndividualMenuButton();
            this.manualSequenceControlInputStage = new QMC.LCP_280.Process.Component.ManualSequenceControl();
            this.waferMapView = new QMC.LCP_280.Process.Component.WaferMapView();
            this.manualSequenceControlCassette = new QMC.LCP_280.Process.Component.ManualSequenceControl();
            this.dioControl = new QMC.LCP_280.Process.Component.DIOControl();
            this.teachingPositionControl = new QMC.LCP_280.Process.Component.TeachingPositionControl();
            this.groupBoxCassetteLifterSeq.SuspendLayout();
            this.groupBoxManual.SuspendLayout();
            this.groupBoxInputStageSeq.SuspendLayout();
            this.groupBoxImageView.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._InputWaferCameraviewer)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBoxCassetteLifterSeq
            // 
            this.groupBoxCassetteLifterSeq.Controls.Add(this.manualSequenceControlCassette);
            this.groupBoxCassetteLifterSeq.Font = new System.Drawing.Font("맑은 고딕", 10.2F, System.Drawing.FontStyle.Bold);
            this.groupBoxCassetteLifterSeq.Location = new System.Drawing.Point(1032, 7);
            this.groupBoxCassetteLifterSeq.Name = "groupBoxCassetteLifterSeq";
            this.groupBoxCassetteLifterSeq.Size = new System.Drawing.Size(395, 335);
            this.groupBoxCassetteLifterSeq.TabIndex = 18;
            this.groupBoxCassetteLifterSeq.TabStop = false;
            this.groupBoxCassetteLifterSeq.Text = "Input Wafer Seq";
            // 
            // groupBoxManual
            // 
            this.groupBoxManual.Controls.Add(this.waferMapView);
            this.groupBoxManual.Controls.Add(this.btnMapping);
            this.groupBoxManual.Font = new System.Drawing.Font("맑은 고딕", 10.2F, System.Drawing.FontStyle.Bold);
            this.groupBoxManual.Location = new System.Drawing.Point(631, 417);
            this.groupBoxManual.Name = "groupBoxManual";
            this.groupBoxManual.Size = new System.Drawing.Size(392, 400);
            this.groupBoxManual.TabIndex = 19;
            this.groupBoxManual.TabStop = false;
            this.groupBoxManual.Text = "Manual";
            // 
            // groupBoxInputStageSeq
            // 
            this.groupBoxInputStageSeq.Controls.Add(this.manualSequenceControlInputStage);
            this.groupBoxInputStageSeq.Font = new System.Drawing.Font("맑은 고딕", 10.2F, System.Drawing.FontStyle.Bold);
            this.groupBoxInputStageSeq.Location = new System.Drawing.Point(1029, 417);
            this.groupBoxInputStageSeq.Name = "groupBoxInputStageSeq";
            this.groupBoxInputStageSeq.Size = new System.Drawing.Size(395, 335);
            this.groupBoxInputStageSeq.TabIndex = 19;
            this.groupBoxInputStageSeq.TabStop = false;
            this.groupBoxInputStageSeq.Text = "InputStage Manual Seq";
            // 
            // groupBoxImageView
            // 
            this.groupBoxImageView.Controls.Add(this._InputWaferCameraviewer);
            this.groupBoxImageView.Font = new System.Drawing.Font("맑은 고딕", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.groupBoxImageView.Location = new System.Drawing.Point(631, 7);
            this.groupBoxImageView.Name = "groupBoxImageView";
            this.groupBoxImageView.Size = new System.Drawing.Size(392, 400);
            this.groupBoxImageView.TabIndex = 20;
            this.groupBoxImageView.TabStop = false;
            this.groupBoxImageView.Text = "ImageView";
            // 
            // _InputWaferCameraviewer
            // 
            this._InputWaferCameraviewer.BackColor = System.Drawing.Color.Black;
            this._InputWaferCameraviewer.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this._InputWaferCameraviewer.Camera = null;
            this._InputWaferCameraviewer.CameraSwitch = null;
            this._InputWaferCameraviewer.Dock = System.Windows.Forms.DockStyle.Top;
            this._InputWaferCameraviewer.FrameRate = 1D;
            this._InputWaferCameraviewer.InputImage = null;
            this._InputWaferCameraviewer.IsViewCustomizedImage = false;
            this._InputWaferCameraviewer.Location = new System.Drawing.Point(3, 26);
            this._InputWaferCameraviewer.Name = "_InputWaferCameraviewer";
            this._InputWaferCameraviewer.OperatingType = QMC.Common.Vision.VisionImageViewer.OperatingTypes.Center;
            this._InputWaferCameraviewer.Simulated = false;
            this._InputWaferCameraviewer.Size = new System.Drawing.Size(386, 324);
            this._InputWaferCameraviewer.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this._InputWaferCameraviewer.TabIndex = 12;
            this._InputWaferCameraviewer.TabStop = false;
            this._InputWaferCameraviewer.UpdateDelayTime = 80;
            this._InputWaferCameraviewer.VisibleCrossLine = true;
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
            this.btnMapping.Click += new System.EventHandler(this.btnMapping_Click);
            // 
            // manualSequenceControlInputStage
            // 
            this.manualSequenceControlInputStage.Dock = System.Windows.Forms.DockStyle.Top;
            this.manualSequenceControlInputStage.Location = new System.Drawing.Point(3, 26);
            this.manualSequenceControlInputStage.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.manualSequenceControlInputStage.MinimumSize = new System.Drawing.Size(297, 250);
            this.manualSequenceControlInputStage.Name = "manualSequenceControlInputStage";
            this.manualSequenceControlInputStage.ParentUnit = null;
            this.manualSequenceControlInputStage.Size = new System.Drawing.Size(389, 316);
            this.manualSequenceControlInputStage.TabIndex = 13;
            // 
            // waferMapView
            // 
            this.waferMapView.Location = new System.Drawing.Point(6, 25);
            this.waferMapView.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.waferMapView.Name = "waferMapView";
            this.waferMapView.Size = new System.Drawing.Size(157, 205);
            this.waferMapView.TabIndex = 16;
            // 
            // manualSequenceControlCassette
            // 
            this.manualSequenceControlCassette.Dock = System.Windows.Forms.DockStyle.Top;
            this.manualSequenceControlCassette.Location = new System.Drawing.Point(3, 26);
            this.manualSequenceControlCassette.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.manualSequenceControlCassette.MinimumSize = new System.Drawing.Size(297, 250);
            this.manualSequenceControlCassette.Name = "manualSequenceControlCassette";
            this.manualSequenceControlCassette.ParentUnit = null;
            this.manualSequenceControlCassette.Size = new System.Drawing.Size(389, 316);
            this.manualSequenceControlCassette.TabIndex = 13;
            // 
            // dioControl
            // 
            this.dioControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dioControl.IoSortMode = QMC.LCP_280.Process.Component.DIOControl.SortingMode.AlphabeticalKey;
            this.dioControl.Location = new System.Drawing.Point(7, 417);
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
            this.teachingPositionControl.Location = new System.Drawing.Point(7, 7);
            this.teachingPositionControl.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
            this.teachingPositionControl.Name = "teachingPositionControl";
            this.teachingPositionControl.ShowCancelButton = true;
            this.teachingPositionControl.ShowSaveButton = true;
            this.teachingPositionControl.Size = new System.Drawing.Size(615, 400);
            this.teachingPositionControl.TabIndex = 14;
            this.teachingPositionControl.UnitName = null;
            // 
            // InputWafer_Working
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(120F, 120F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(1580, 939);
            this.Controls.Add(this.groupBoxImageView);
            this.Controls.Add(this.groupBoxInputStageSeq);
            this.Controls.Add(this.groupBoxManual);
            this.Controls.Add(this.groupBoxCassetteLifterSeq);
            this.Controls.Add(this.dioControl);
            this.Controls.Add(this.teachingPositionControl);
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "InputWafer_Working";
            this.Text = "InputWafer_Working";
            this.groupBoxCassetteLifterSeq.ResumeLayout(false);
            this.groupBoxManual.ResumeLayout(false);
            this.groupBoxInputStageSeq.ResumeLayout(false);
            this.groupBoxImageView.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this._InputWaferCameraviewer)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private Component.DIOControl dioControl;
        private Component.TeachingPositionControl teachingPositionControl;
        private Component.ManualSequenceControl manualSequenceControlCassette;
        private WaferMapView waferMapView;
        private Common.IndividualMenuButton btnMapping;
        private System.Windows.Forms.GroupBox groupBoxCassetteLifterSeq;
        private System.Windows.Forms.GroupBox groupBoxManual;
        private System.Windows.Forms.GroupBox groupBoxInputStageSeq;
        private ManualSequenceControl manualSequenceControlInputStage;
        private System.Windows.Forms.GroupBox groupBoxImageView;
        private Common.Vision.VisionImageViewer _InputWaferCameraviewer;
    }
}