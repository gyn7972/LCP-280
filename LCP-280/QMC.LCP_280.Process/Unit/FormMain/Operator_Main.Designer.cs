namespace QMC.LCP_280.Process.Unit.FormMain
{
    partial class Operator_Main
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
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.groupBoxImageView.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.InputWaferCamera)).BeginInit();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.IndexOutputCamera)).BeginInit();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.OutputWaferCamera)).BeginInit();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBoxImageView
            // 
            this.groupBoxImageView.Controls.Add(this.InputWaferCamera);
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
            this.InputWaferCamera.Size = new System.Drawing.Size(411, 342);
            this.InputWaferCamera.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.InputWaferCamera.TabIndex = 12;
            this.InputWaferCamera.TabStop = false;
            this.InputWaferCamera.UpdateDelayTime = 80;
            this.InputWaferCamera.VisibleCrossLine = true;
            // 
            // groupBox3
            // 
            this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
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
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.IndexOutputCamera);
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
            this.IndexOutputCamera.Size = new System.Drawing.Size(411, 342);
            this.IndexOutputCamera.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.IndexOutputCamera.TabIndex = 12;
            this.IndexOutputCamera.TabStop = false;
            this.IndexOutputCamera.UpdateDelayTime = 80;
            this.IndexOutputCamera.VisibleCrossLine = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.OutputWaferCamera);
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
            this.OutputWaferCamera.Size = new System.Drawing.Size(412, 342);
            this.OutputWaferCamera.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.OutputWaferCamera.TabIndex = 12;
            this.OutputWaferCamera.TabStop = false;
            this.OutputWaferCamera.UpdateDelayTime = 80;
            this.OutputWaferCamera.VisibleCrossLine = true;
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
            // Operator_Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1264, 751);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "Operator_Main";
            this.Text = "Operator_Main";
            this.groupBoxImageView.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.InputWaferCamera)).EndInit();
            this.groupBox1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.IndexOutputCamera)).EndInit();
            this.groupBox2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.OutputWaferCamera)).EndInit();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
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
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
    }
}