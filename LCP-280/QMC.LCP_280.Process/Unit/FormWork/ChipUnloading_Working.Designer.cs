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
            this.dioControl = new QMC.LCP_280.Process.Component.DIOControl();
            this.teachingPositionControl = new QMC.LCP_280.Process.Component.TeachingPositionControl();
            this.manualSequenceControl = new QMC.LCP_280.Process.Component.ManualSequenceControl();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBoxImageView = new System.Windows.Forms.GroupBox();
            this._ChipUnloadingCameraviewer = new QMC.Common.Vision.VisionImageViewer();
            this.groupBox1.SuspendLayout();
            this.groupBoxImageView.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._ChipUnloadingCameraviewer)).BeginInit();
            this.SuspendLayout();
            // 
            // dioControl
            // 
            this.dioControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dioControl.IoSortMode = QMC.LCP_280.Process.Component.DIOControl.SortingMode.AlphabeticalKey;
            this.dioControl.Location = new System.Drawing.Point(7, 414);
            this.dioControl.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.dioControl.Name = "dioControl";
            this.dioControl.RefreshIntervalMs = 400;
            this.dioControl.Size = new System.Drawing.Size(615, 400);
            this.dioControl.TabIndex = 16;
            // 
            // teachingPositionControl
            // 
            this.teachingPositionControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.teachingPositionControl.ButtonSize = new System.Drawing.Size(90, 32);
            this.teachingPositionControl.Location = new System.Drawing.Point(7, 6);
            this.teachingPositionControl.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.teachingPositionControl.Name = "teachingPositionControl";
            this.teachingPositionControl.ShowCancelButton = true;
            this.teachingPositionControl.ShowSaveButton = true;
            this.teachingPositionControl.Size = new System.Drawing.Size(615, 400);
            this.teachingPositionControl.TabIndex = 15;
            this.teachingPositionControl.UnitName = null;
            // 
            // manualSequenceControl
            // 
            this.manualSequenceControl.Dock = System.Windows.Forms.DockStyle.Top;
            this.manualSequenceControl.Location = new System.Drawing.Point(3, 26);
            this.manualSequenceControl.Margin = new System.Windows.Forms.Padding(2);
            this.manualSequenceControl.MinimumSize = new System.Drawing.Size(297, 250);
            this.manualSequenceControl.Name = "manualSequenceControl";
            this.manualSequenceControl.ParentUnit = null;
            this.manualSequenceControl.Size = new System.Drawing.Size(389, 319);
            this.manualSequenceControl.TabIndex = 14;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.manualSequenceControl);
            this.groupBox1.Font = new System.Drawing.Font("맑은 고딕", 10.2F, System.Drawing.FontStyle.Bold);
            this.groupBox1.Location = new System.Drawing.Point(1026, 6);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(395, 350);
            this.groupBox1.TabIndex = 17;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "ChipUnloading Seq";
            // 
            // groupBoxImageView
            // 
            this.groupBoxImageView.Controls.Add(this._ChipUnloadingCameraviewer);
            this.groupBoxImageView.Font = new System.Drawing.Font("맑은 고딕", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.groupBoxImageView.Location = new System.Drawing.Point(628, 6);
            this.groupBoxImageView.Name = "groupBoxImageView";
            this.groupBoxImageView.Size = new System.Drawing.Size(392, 400);
            this.groupBoxImageView.TabIndex = 18;
            this.groupBoxImageView.TabStop = false;
            this.groupBoxImageView.Text = "ImageView";
            // 
            // _ChipUnloadingCameraviewer
            // 
            this._ChipUnloadingCameraviewer.BackColor = System.Drawing.Color.Black;
            this._ChipUnloadingCameraviewer.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this._ChipUnloadingCameraviewer.Camera = null;
            this._ChipUnloadingCameraviewer.CameraSwitch = null;
            this._ChipUnloadingCameraviewer.Dock = System.Windows.Forms.DockStyle.Top;
            this._ChipUnloadingCameraviewer.FrameRate = 1D;
            this._ChipUnloadingCameraviewer.InputImage = null;
            this._ChipUnloadingCameraviewer.IsViewCustomizedImage = false;
            this._ChipUnloadingCameraviewer.Location = new System.Drawing.Point(3, 26);
            this._ChipUnloadingCameraviewer.Name = "_ChipUnloadingCameraviewer";
            this._ChipUnloadingCameraviewer.OperatingType = QMC.Common.Vision.VisionImageViewer.OperatingTypes.Center;
            this._ChipUnloadingCameraviewer.Simulated = false;
            this._ChipUnloadingCameraviewer.Size = new System.Drawing.Size(386, 324);
            this._ChipUnloadingCameraviewer.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this._ChipUnloadingCameraviewer.TabIndex = 12;
            this._ChipUnloadingCameraviewer.TabStop = false;
            this._ChipUnloadingCameraviewer.UpdateDelayTime = 80;
            this._ChipUnloadingCameraviewer.VisibleCrossLine = true;
            // 
            // ChipUnloading_Working
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(120F, 120F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(1580, 939);
            this.Controls.Add(this.groupBoxImageView);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.dioControl);
            this.Controls.Add(this.teachingPositionControl);
            this.Name = "ChipUnloading_Working";
            this.Text = "ChipUnloading_Working";
            this.groupBox1.ResumeLayout(false);
            this.groupBoxImageView.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this._ChipUnloadingCameraviewer)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private Component.DIOControl dioControl;
        private Component.TeachingPositionControl teachingPositionControl;
        private Component.ManualSequenceControl manualSequenceControl;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBoxImageView;
        private Common.Vision.VisionImageViewer _ChipUnloadingCameraviewer;
    }
}