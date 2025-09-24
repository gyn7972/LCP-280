namespace QMC.LCP_280.Process.Unit.FormMain
{
    partial class Vision_Main
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
            this.groupBoxImageView = new System.Windows.Forms.GroupBox();
            this.InputWaferCamera = new QMC.Common.Vision.VisionImageViewer();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.IndexOutputCamera = new QMC.Common.Vision.VisionImageViewer();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.OutputWaferCamera = new QMC.Common.Vision.VisionImageViewer();
            this.groupBoxImageView.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.InputWaferCamera)).BeginInit();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.IndexOutputCamera)).BeginInit();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.OutputWaferCamera)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBoxImageView
            // 
            this.groupBoxImageView.Controls.Add(this.InputWaferCamera);
            this.groupBoxImageView.Font = new System.Drawing.Font("맑은 고딕", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.groupBoxImageView.Location = new System.Drawing.Point(11, 11);
            this.groupBoxImageView.Margin = new System.Windows.Forms.Padding(2);
            this.groupBoxImageView.Name = "groupBoxImageView";
            this.groupBoxImageView.Padding = new System.Windows.Forms.Padding(2);
            this.groupBoxImageView.Size = new System.Drawing.Size(314, 225);
            this.groupBoxImageView.TabIndex = 16;
            this.groupBoxImageView.TabStop = false;
            this.groupBoxImageView.Text = "Input Wafer Camera";
            // 
            // InputWaferCamera
            // 
            this.InputWaferCamera.BackColor = System.Drawing.Color.Black;
            this.InputWaferCamera.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.InputWaferCamera.Camera = null;
            this.InputWaferCamera.CameraSwitch = null;
            this.InputWaferCamera.Dock = System.Windows.Forms.DockStyle.Fill;
            this.InputWaferCamera.FrameRate = 1D;
            this.InputWaferCamera.InputImage = null;
            this.InputWaferCamera.IsViewCustomizedImage = false;
            this.InputWaferCamera.Location = new System.Drawing.Point(2, 21);
            this.InputWaferCamera.Margin = new System.Windows.Forms.Padding(2);
            this.InputWaferCamera.Name = "InputWaferCamera";
            this.InputWaferCamera.OperatingType = QMC.Common.Vision.VisionImageViewer.OperatingTypes.Center;
            this.InputWaferCamera.Simulated = false;
            this.InputWaferCamera.Size = new System.Drawing.Size(310, 202);
            this.InputWaferCamera.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.InputWaferCamera.TabIndex = 12;
            this.InputWaferCamera.TabStop = false;
            this.InputWaferCamera.UpdateDelayTime = 80;
            this.InputWaferCamera.VisibleCrossLine = true;
            // 
            // groupBox3
            // 
            this.groupBox3.Font = new System.Drawing.Font("맑은 고딕", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.groupBox3.Location = new System.Drawing.Point(13, 240);
            this.groupBox3.Margin = new System.Windows.Forms.Padding(2);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Padding = new System.Windows.Forms.Padding(2);
            this.groupBox3.Size = new System.Drawing.Size(1016, 335);
            this.groupBox3.TabIndex = 17;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Manual Control";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.IndexOutputCamera);
            this.groupBox1.Font = new System.Drawing.Font("맑은 고딕", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.groupBox1.Location = new System.Drawing.Point(363, 11);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(2);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(2);
            this.groupBox1.Size = new System.Drawing.Size(314, 225);
            this.groupBox1.TabIndex = 17;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Index Output Camera";
            // 
            // IndexOutputCamera
            // 
            this.IndexOutputCamera.BackColor = System.Drawing.Color.Black;
            this.IndexOutputCamera.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.IndexOutputCamera.Camera = null;
            this.IndexOutputCamera.CameraSwitch = null;
            this.IndexOutputCamera.Dock = System.Windows.Forms.DockStyle.Fill;
            this.IndexOutputCamera.FrameRate = 1D;
            this.IndexOutputCamera.InputImage = null;
            this.IndexOutputCamera.IsViewCustomizedImage = false;
            this.IndexOutputCamera.Location = new System.Drawing.Point(2, 21);
            this.IndexOutputCamera.Margin = new System.Windows.Forms.Padding(2);
            this.IndexOutputCamera.Name = "IndexOutputCamera";
            this.IndexOutputCamera.OperatingType = QMC.Common.Vision.VisionImageViewer.OperatingTypes.Center;
            this.IndexOutputCamera.Simulated = false;
            this.IndexOutputCamera.Size = new System.Drawing.Size(310, 202);
            this.IndexOutputCamera.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.IndexOutputCamera.TabIndex = 12;
            this.IndexOutputCamera.TabStop = false;
            this.IndexOutputCamera.UpdateDelayTime = 80;
            this.IndexOutputCamera.VisibleCrossLine = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.OutputWaferCamera);
            this.groupBox2.Font = new System.Drawing.Font("맑은 고딕", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.groupBox2.Location = new System.Drawing.Point(715, 11);
            this.groupBox2.Margin = new System.Windows.Forms.Padding(2);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(2);
            this.groupBox2.Size = new System.Drawing.Size(314, 225);
            this.groupBox2.TabIndex = 18;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Output Wafer Camera";
            // 
            // OutputWaferCamera
            // 
            this.OutputWaferCamera.BackColor = System.Drawing.Color.Black;
            this.OutputWaferCamera.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.OutputWaferCamera.Camera = null;
            this.OutputWaferCamera.CameraSwitch = null;
            this.OutputWaferCamera.Dock = System.Windows.Forms.DockStyle.Fill;
            this.OutputWaferCamera.FrameRate = 1D;
            this.OutputWaferCamera.InputImage = null;
            this.OutputWaferCamera.IsViewCustomizedImage = false;
            this.OutputWaferCamera.Location = new System.Drawing.Point(2, 21);
            this.OutputWaferCamera.Margin = new System.Windows.Forms.Padding(2);
            this.OutputWaferCamera.Name = "OutputWaferCamera";
            this.OutputWaferCamera.OperatingType = QMC.Common.Vision.VisionImageViewer.OperatingTypes.Center;
            this.OutputWaferCamera.Simulated = false;
            this.OutputWaferCamera.Size = new System.Drawing.Size(310, 202);
            this.OutputWaferCamera.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.OutputWaferCamera.TabIndex = 12;
            this.OutputWaferCamera.TabStop = false;
            this.OutputWaferCamera.UpdateDelayTime = 80;
            this.OutputWaferCamera.VisibleCrossLine = true;
            // 
            // Vision_Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1041, 586);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.groupBoxImageView);
            this.Name = "Vision_Main";
            this.Text = "Vision_Manin";
            this.groupBoxImageView.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.InputWaferCamera)).EndInit();
            this.groupBox1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.IndexOutputCamera)).EndInit();
            this.groupBox2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.OutputWaferCamera)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.GroupBox groupBoxImageView;
        private Common.Vision.VisionImageViewer InputWaferCamera;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.GroupBox groupBox1;
        private Common.Vision.VisionImageViewer IndexOutputCamera;
        private System.Windows.Forms.GroupBox groupBox2;
        private Common.Vision.VisionImageViewer OutputWaferCamera;
    }
}