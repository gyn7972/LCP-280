namespace QMC.LCP_280.Process.Unit.FormMain
{
    partial class Operator_Main
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel4;
        private Common.Vision.VisionImageViewer OutputWaferCamera;
        private System.Windows.Forms.GroupBox groupBoxImageView;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel5;
        private Common.Vision.VisionImageViewer InputWaferCamera;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private Common.Vision.VisionImageViewer IndexOutputCamera;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel6;

        //GroupBox Control View 추가
        private System.Windows.Forms.GroupBox groupBox_SequenceManual;
        private QMC.LCP_280.Process.Unit.FormMain.SequenceManualControl sequenceManualControl;

        private System.Windows.Forms.GroupBox groupBox_SequenceAuto;
        private QMC.LCP_280.Process.Unit.FormMain.SequenceAutoControl sequenceAutoControl;

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
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
            this.OutputWaferCamera = new QMC.Common.Vision.VisionImageViewer();
            this.groupBoxImageView = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel5 = new System.Windows.Forms.TableLayoutPanel();
            this.InputWaferCamera = new QMC.Common.Vision.VisionImageViewer();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.IndexOutputCamera = new QMC.Common.Vision.VisionImageViewer();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel6 = new System.Windows.Forms.TableLayoutPanel();
            this.groupBox_SequenceManual = new System.Windows.Forms.GroupBox();
            this.sequenceManualControl = new QMC.LCP_280.Process.Unit.FormMain.SequenceManualControl();
            this.groupBox_SequenceAuto = new System.Windows.Forms.GroupBox();
            this.sequenceAutoControl = new QMC.LCP_280.Process.Unit.FormMain.SequenceAutoControl();
            this.tableLayoutPanel2.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.tableLayoutPanel4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.OutputWaferCamera)).BeginInit();
            this.groupBoxImageView.SuspendLayout();
            this.tableLayoutPanel5.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.InputWaferCamera)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.IndexOutputCamera)).BeginInit();
            this.tableLayoutPanel1.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.tableLayoutPanel6.SuspendLayout();
            this.groupBox_SequenceManual.SuspendLayout();
            this.groupBox_SequenceAuto.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 3;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel2.Controls.Add(this.groupBox2, 2, 0);
            this.tableLayoutPanel2.Controls.Add(this.groupBoxImageView, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.groupBox1, 1, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(1258, 369);
            this.tableLayoutPanel2.TabIndex = 0;
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.tableLayoutPanel4);
            this.groupBox2.Font = new System.Drawing.Font("맑은 고딕", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.groupBox2.Location = new System.Drawing.Point(840, 2);
            this.groupBox2.Margin = new System.Windows.Forms.Padding(2);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(2);
            this.groupBox2.Size = new System.Drawing.Size(416, 365);
            this.groupBox2.TabIndex = 18;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Output Wafer Camera";
            // 
            // tableLayoutPanel4
            // 
            this.tableLayoutPanel4.ColumnCount = 1;
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel4.Controls.Add(this.OutputWaferCamera, 0, 0);
            this.tableLayoutPanel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel4.Location = new System.Drawing.Point(2, 21);
            this.tableLayoutPanel4.Name = "tableLayoutPanel4";
            this.tableLayoutPanel4.RowCount = 2;
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 90F));
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanel4.Size = new System.Drawing.Size(412, 342);
            this.tableLayoutPanel4.TabIndex = 5;
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
            this.OutputWaferCamera.Location = new System.Drawing.Point(2, 2);
            this.OutputWaferCamera.Margin = new System.Windows.Forms.Padding(2);
            this.OutputWaferCamera.Name = "OutputWaferCamera";
            this.OutputWaferCamera.OperatingType = QMC.Common.Vision.VisionImageViewer.OperatingTypes.Center;
            this.OutputWaferCamera.Simulated = false;
            this.OutputWaferCamera.Size = new System.Drawing.Size(408, 303);
            this.OutputWaferCamera.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.OutputWaferCamera.TabIndex = 12;
            this.OutputWaferCamera.TabStop = false;
            this.OutputWaferCamera.UpdateDelayTime = 80;
            this.OutputWaferCamera.VisibleCrossLine = true;
            // 
            // groupBoxImageView
            // 
            this.groupBoxImageView.Controls.Add(this.tableLayoutPanel5);
            this.groupBoxImageView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxImageView.Font = new System.Drawing.Font("맑은 고딕", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.groupBoxImageView.Location = new System.Drawing.Point(2, 2);
            this.groupBoxImageView.Margin = new System.Windows.Forms.Padding(2);
            this.groupBoxImageView.Name = "groupBoxImageView";
            this.groupBoxImageView.Padding = new System.Windows.Forms.Padding(2);
            this.groupBoxImageView.Size = new System.Drawing.Size(415, 365);
            this.groupBoxImageView.TabIndex = 16;
            this.groupBoxImageView.TabStop = false;
            this.groupBoxImageView.Text = "Input Wafer Camera";
            // 
            // tableLayoutPanel5
            // 
            this.tableLayoutPanel5.ColumnCount = 1;
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel5.Controls.Add(this.InputWaferCamera, 0, 0);
            this.tableLayoutPanel5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel5.Location = new System.Drawing.Point(2, 21);
            this.tableLayoutPanel5.Name = "tableLayoutPanel5";
            this.tableLayoutPanel5.RowCount = 2;
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 90F));
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanel5.Size = new System.Drawing.Size(411, 342);
            this.tableLayoutPanel5.TabIndex = 3;
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
            this.InputWaferCamera.Location = new System.Drawing.Point(2, 2);
            this.InputWaferCamera.Margin = new System.Windows.Forms.Padding(2);
            this.InputWaferCamera.Name = "InputWaferCamera";
            this.InputWaferCamera.OperatingType = QMC.Common.Vision.VisionImageViewer.OperatingTypes.Center;
            this.InputWaferCamera.Simulated = false;
            this.InputWaferCamera.Size = new System.Drawing.Size(407, 303);
            this.InputWaferCamera.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.InputWaferCamera.TabIndex = 12;
            this.InputWaferCamera.TabStop = false;
            this.InputWaferCamera.UpdateDelayTime = 80;
            this.InputWaferCamera.VisibleCrossLine = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.tableLayoutPanel3);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Font = new System.Drawing.Font("맑은 고딕", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.groupBox1.Location = new System.Drawing.Point(421, 2);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(2);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(2);
            this.groupBox1.Size = new System.Drawing.Size(415, 365);
            this.groupBox1.TabIndex = 17;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Index Output Camera";
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 1;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.Controls.Add(this.IndexOutputCamera, 0, 0);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(2, 21);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 2;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 90F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(411, 342);
            this.tableLayoutPanel3.TabIndex = 4;
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
            this.IndexOutputCamera.Location = new System.Drawing.Point(2, 2);
            this.IndexOutputCamera.Margin = new System.Windows.Forms.Padding(2);
            this.IndexOutputCamera.Name = "IndexOutputCamera";
            this.IndexOutputCamera.OperatingType = QMC.Common.Vision.VisionImageViewer.OperatingTypes.Center;
            this.IndexOutputCamera.Simulated = false;
            this.IndexOutputCamera.Size = new System.Drawing.Size(407, 303);
            this.IndexOutputCamera.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.IndexOutputCamera.TabIndex = 12;
            this.IndexOutputCamera.TabStop = false;
            this.IndexOutputCamera.UpdateDelayTime = 80;
            this.IndexOutputCamera.VisibleCrossLine = true;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.groupBox3, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 1, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1264, 751);
            this.tableLayoutPanel1.TabIndex = 19;
            // 
            // groupBox3
            // 
            this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox3.Controls.Add(this.tableLayoutPanel6);
            this.groupBox3.Font = new System.Drawing.Font("맑은 고딕", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.groupBox3.Location = new System.Drawing.Point(2, 377);
            this.groupBox3.Margin = new System.Windows.Forms.Padding(2);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Padding = new System.Windows.Forms.Padding(2);
            this.groupBox3.Size = new System.Drawing.Size(1260, 372);
            this.groupBox3.TabIndex = 17;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Manual Control";
            // 
            // tableLayoutPanel6
            // 
            this.tableLayoutPanel6.ColumnCount = 3;
            this.tableLayoutPanel6.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel6.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanel6.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 40F));
            this.tableLayoutPanel6.Controls.Add(this.groupBox_SequenceManual, 0, 0);
            this.tableLayoutPanel6.Controls.Add(this.groupBox_SequenceAuto, 1, 0);
            this.tableLayoutPanel6.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel6.Location = new System.Drawing.Point(2, 21);
            this.tableLayoutPanel6.Name = "tableLayoutPanel6";
            this.tableLayoutPanel6.RowCount = 1;
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel6.Size = new System.Drawing.Size(1256, 349);
            this.tableLayoutPanel6.TabIndex = 19;
            // 
            // groupBox_SequenceManual
            // 
            this.groupBox_SequenceManual.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox_SequenceManual.Controls.Add(this.sequenceManualControl);
            this.groupBox_SequenceManual.Font = new System.Drawing.Font("맑은 고딕", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.groupBox_SequenceManual.Location = new System.Drawing.Point(2, 2);
            this.groupBox_SequenceManual.Margin = new System.Windows.Forms.Padding(2);
            this.groupBox_SequenceManual.Name = "groupBox_SequenceManual";
            this.groupBox_SequenceManual.Padding = new System.Windows.Forms.Padding(2);
            this.groupBox_SequenceManual.Size = new System.Drawing.Size(624, 345);
            this.groupBox_SequenceManual.TabIndex = 18;
            this.groupBox_SequenceManual.TabStop = false;
            this.groupBox_SequenceManual.Text = "Sequence Manual";
            // 
            // sequenceManualControl
            // 
            this.sequenceManualControl.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.sequenceManualControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.sequenceManualControl.Location = new System.Drawing.Point(2, 21);
            this.sequenceManualControl.Margin = new System.Windows.Forms.Padding(2);
            this.sequenceManualControl.Name = "sequenceManualControl";
            this.sequenceManualControl.Size = new System.Drawing.Size(620, 322);
            this.sequenceManualControl.TabIndex = 1;
            // 
            // groupBox_SequenceAuto
            // 
            this.groupBox_SequenceAuto.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox_SequenceAuto.Controls.Add(this.sequenceAutoControl);
            this.groupBox_SequenceAuto.Font = new System.Drawing.Font("맑은 고딕", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.groupBox_SequenceAuto.Location = new System.Drawing.Point(630, 2);
            this.groupBox_SequenceAuto.Margin = new System.Windows.Forms.Padding(2);
            this.groupBox_SequenceAuto.Name = "groupBox_SequenceAuto";
            this.groupBox_SequenceAuto.Padding = new System.Windows.Forms.Padding(2);
            this.groupBox_SequenceAuto.Size = new System.Drawing.Size(121, 345);
            this.groupBox_SequenceAuto.TabIndex = 19;
            this.groupBox_SequenceAuto.TabStop = false;
            this.groupBox_SequenceAuto.Text = "Sequence Auto";
            // 
            // sequenceAutoControl
            // 
            this.sequenceAutoControl.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.sequenceAutoControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.sequenceAutoControl.Location = new System.Drawing.Point(2, 21);
            this.sequenceAutoControl.Margin = new System.Windows.Forms.Padding(2);
            this.sequenceAutoControl.Name = "sequenceAutoControl";
            this.sequenceAutoControl.Size = new System.Drawing.Size(117, 322);
            this.sequenceAutoControl.TabIndex = 1;
            // 
            // Operator_Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1264, 751);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "Operator_Main";
            this.Text = "Operator_Main";
            this.tableLayoutPanel2.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.tableLayoutPanel4.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.OutputWaferCamera)).EndInit();
            this.groupBoxImageView.ResumeLayout(false);
            this.tableLayoutPanel5.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.InputWaferCamera)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.tableLayoutPanel3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.IndexOutputCamera)).EndInit();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.tableLayoutPanel6.ResumeLayout(false);
            this.groupBox_SequenceManual.ResumeLayout(false);
            this.groupBox_SequenceAuto.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion


    }
}