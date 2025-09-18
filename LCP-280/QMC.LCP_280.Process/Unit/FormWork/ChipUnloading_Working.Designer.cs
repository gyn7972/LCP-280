namespace QMC.LCP_280.Process.Unit.FormWork
{
    partial class ChipUnloading_Working
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
            this._btnVisionSetting = new System.Windows.Forms.Button();
            this._ChipUnloadingCameraviewer = new QMC.Common.Vision.VisionImageViewer();
            this.dioControl = new QMC.LCP_280.Process.Component.DIOControl();
            this.teachingPositionControl = new QMC.LCP_280.Process.Component.TeachingPositionControl();
            this.manualSequenceControl = new QMC.LCP_280.Process.Component.ManualSequenceControl();
            ((System.ComponentModel.ISupportInitialize)(this._ChipUnloadingCameraviewer)).BeginInit();
            this.SuspendLayout();
            // 
            // _btnVisionSetting
            // 
            this._btnVisionSetting.Location = new System.Drawing.Point(535, 362);
            this._btnVisionSetting.Name = "_btnVisionSetting";
            this._btnVisionSetting.Size = new System.Drawing.Size(110, 35);
            this._btnVisionSetting.TabIndex = 18;
            this._btnVisionSetting.Text = "VisionSetting";
            this._btnVisionSetting.UseVisualStyleBackColor = true;
            // 
            // _ChipUnloadingCameraviewer
            // 
            this._ChipUnloadingCameraviewer.BackColor = System.Drawing.Color.Black;
            this._ChipUnloadingCameraviewer.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this._ChipUnloadingCameraviewer.Camera = null;
            this._ChipUnloadingCameraviewer.CameraSwitch = null;
            this._ChipUnloadingCameraviewer.FrameRate = 1D;
            this._ChipUnloadingCameraviewer.InputImage = null;
            this._ChipUnloadingCameraviewer.IsViewCustomizedImage = false;
            this._ChipUnloadingCameraviewer.Location = new System.Drawing.Point(535, 12);
            this._ChipUnloadingCameraviewer.MaximumSize = new System.Drawing.Size(370, 350);
            this._ChipUnloadingCameraviewer.MinimumSize = new System.Drawing.Size(370, 350);
            this._ChipUnloadingCameraviewer.Name = "_ChipUnloadingCameraviewer";
            this._ChipUnloadingCameraviewer.OperatingType = QMC.Common.Vision.VisionImageViewer.OperatingTypes.Center;
            this._ChipUnloadingCameraviewer.Simulated = false;
            this._ChipUnloadingCameraviewer.Size = new System.Drawing.Size(370, 350);
            this._ChipUnloadingCameraviewer.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this._ChipUnloadingCameraviewer.TabIndex = 12;
            this._ChipUnloadingCameraviewer.TabStop = false;
            this._ChipUnloadingCameraviewer.UpdateDelayTime = 80;
            this._ChipUnloadingCameraviewer.VisibleCrossLine = true;
            // 
            // dioControl
            // 
            this.dioControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dioControl.IoSortMode = QMC.LCP_280.Process.Component.DIOControl.SortingMode.AlphabeticalKey;
            this.dioControl.Location = new System.Drawing.Point(12, 550);
            this.dioControl.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.dioControl.Name = "dioControl";
            this.dioControl.RefreshIntervalMs = 400;
            this.dioControl.Size = new System.Drawing.Size(833, 263);
            this.dioControl.TabIndex = 16;
            // 
            // teachingPositionControl
            // 
            this.teachingPositionControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.teachingPositionControl.ButtonSize = new System.Drawing.Size(90, 32);
            this.teachingPositionControl.Location = new System.Drawing.Point(12, 6);
            this.teachingPositionControl.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.teachingPositionControl.Name = "teachingPositionControl";
            this.teachingPositionControl.ShowCancelButton = true;
            this.teachingPositionControl.ShowSaveButton = true;
            this.teachingPositionControl.Size = new System.Drawing.Size(517, 538);
            this.teachingPositionControl.TabIndex = 15;
            this.teachingPositionControl.UnitName = null;
            // 
            // manualSequenceControl
            // 
            this.manualSequenceControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.manualSequenceControl.Location = new System.Drawing.Point(1227, 6);
            this.manualSequenceControl.MinimumSize = new System.Drawing.Size(260, 200);
            this.manualSequenceControl.Name = "manualSequenceControl";
            this.manualSequenceControl.Size = new System.Drawing.Size(341, 538);
            this.manualSequenceControl.TabIndex = 14;
            // 
            // ChipUnloading_Working
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(120F, 120F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(1580, 939);
            this.Controls.Add(this._btnVisionSetting);
            this.Controls.Add(this._ChipUnloadingCameraviewer);
            this.Controls.Add(this.dioControl);
            this.Controls.Add(this.teachingPositionControl);
            this.Controls.Add(this.manualSequenceControl);
            this.Name = "ChipUnloading_Working";
            this.Text = "ChipUnloading_Working";
            ((System.ComponentModel.ISupportInitialize)(this._ChipUnloadingCameraviewer)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button _btnVisionSetting;
        private Common.Vision.VisionImageViewer _ChipUnloadingCameraviewer;
        private Component.DIOControl dioControl;
        private Component.TeachingPositionControl teachingPositionControl;
        private Component.ManualSequenceControl manualSequenceControl;
    }
}