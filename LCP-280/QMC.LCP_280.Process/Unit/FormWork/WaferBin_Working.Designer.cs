namespace QMC.LCP_280.Process.Unit.FormWork
{
    partial class WaferBin_Working
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
            this._ChipLoadingCameraviewer = new QMC.Common.Vision.VisionImageViewer();
            this.dioControl = new QMC.LCP_280.Process.Component.DIOControl();
            this.teachingPositionControl = new QMC.LCP_280.Process.Component.TeachingPositionControl();
            this.manualSequenceControl = new QMC.LCP_280.Process.Sequences.ManualSequenceControl();
            ((System.ComponentModel.ISupportInitialize)(this._ChipLoadingCameraviewer)).BeginInit();
            this.SuspendLayout();
            // 
            // _ChipLoadingCameraviewer
            // 
            this._ChipLoadingCameraviewer.BackColor = System.Drawing.Color.Black;
            this._ChipLoadingCameraviewer.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this._ChipLoadingCameraviewer.Camera = null;
            this._ChipLoadingCameraviewer.CameraSwitch = null;
            this._ChipLoadingCameraviewer.FrameRate = 1D;
            this._ChipLoadingCameraviewer.InputImage = null;
            this._ChipLoadingCameraviewer.IsViewCustomizedImage = false;
            this._ChipLoadingCameraviewer.Location = new System.Drawing.Point(535, 12);
            this._ChipLoadingCameraviewer.MaximumSize = new System.Drawing.Size(370, 350);
            this._ChipLoadingCameraviewer.MinimumSize = new System.Drawing.Size(370, 350);
            this._ChipLoadingCameraviewer.Name = "_ChipLoadingCameraviewer";
            this._ChipLoadingCameraviewer.OperatingType = QMC.Common.Vision.VisionImageViewer.OperatingTypes.Center;
            this._ChipLoadingCameraviewer.Simulated = false;
            this._ChipLoadingCameraviewer.Size = new System.Drawing.Size(370, 350);
            this._ChipLoadingCameraviewer.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this._ChipLoadingCameraviewer.TabIndex = 16;
            this._ChipLoadingCameraviewer.TabStop = false;
            this._ChipLoadingCameraviewer.UpdateDelayTime = 80;
            this._ChipLoadingCameraviewer.VisibleCrossLine = true;
            // 
            // dioControl
            // 
            this.dioControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dioControl.IoSortMode = QMC.LCP_280.Process.Component.DIOControl.SortingMode.AlphabeticalKey;
            this.dioControl.Location = new System.Drawing.Point(12, 368);
            this.dioControl.Name = "dioControl";
            this.dioControl.RefreshIntervalMs = 400;
            this.dioControl.Size = new System.Drawing.Size(517, 263);
            this.dioControl.TabIndex = 15;
            // 
            // teachingPositionControl
            // 
            this.teachingPositionControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.teachingPositionControl.ButtonSize = new System.Drawing.Size(90, 32);
            this.teachingPositionControl.Location = new System.Drawing.Point(12, 12);
            this.teachingPositionControl.Name = "teachingPositionControl";
            this.teachingPositionControl.ShowCancelButton = true;
            this.teachingPositionControl.ShowSaveButton = true;
            this.teachingPositionControl.Size = new System.Drawing.Size(517, 350);
            this.teachingPositionControl.TabIndex = 14;
            this.teachingPositionControl.UnitName = null;
            // 
            // manualSequenceControl
            // 
            this.manualSequenceControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.manualSequenceControl.Location = new System.Drawing.Point(911, 12);
            this.manualSequenceControl.MinimumSize = new System.Drawing.Size(260, 200);
            this.manualSequenceControl.Name = "manualSequenceControl";
            this.manualSequenceControl.Size = new System.Drawing.Size(341, 350);
            this.manualSequenceControl.TabIndex = 13;
            // 
            // WaferBin_Working
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1264, 751);
            this.Controls.Add(this._ChipLoadingCameraviewer);
            this.Controls.Add(this.dioControl);
            this.Controls.Add(this.teachingPositionControl);
            this.Controls.Add(this.manualSequenceControl);
            this.Name = "WaferBin_Working";
            this.Text = "WaferBin_Working";
            ((System.ComponentModel.ISupportInitialize)(this._ChipLoadingCameraviewer)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private Common.Vision.VisionImageViewer _ChipLoadingCameraviewer;
        private Component.DIOControl dioControl;
        private Component.TeachingPositionControl teachingPositionControl;
        private Sequences.ManualSequenceControl manualSequenceControl;
    }
}