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
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel8 = new System.Windows.Forms.TableLayoutPanel();
            this.btnHomeAll = new QMC.Common.IndividualMenuButton();
            this.btn_Jog = new QMC.Common.IndividualMenuButton();
            this.groupBox_SequenceManual = new System.Windows.Forms.GroupBox();
            this.sequenceManualControl = new QMC.LCP_280.Process.Unit.FormMain.SequenceManualControl();
            this.BtnMeasurementResult = new QMC.Common.IndividualMenuButton();
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
            this.groupBox4.SuspendLayout();
            this.tableLayoutPanel8.SuspendLayout();
            this.groupBox_SequenceManual.SuspendLayout();
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
            this.tableLayoutPanel2.Location = new System.Drawing.Point(4, 5);
            this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(1798, 553);
            this.tableLayoutPanel2.TabIndex = 0;
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.tableLayoutPanel4);
            this.groupBox2.Font = new System.Drawing.Font("맑은 고딕", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.groupBox2.Location = new System.Drawing.Point(1200, 2);
            this.groupBox2.Margin = new System.Windows.Forms.Padding(2);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(2);
            this.groupBox2.Size = new System.Drawing.Size(596, 549);
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
            this.tableLayoutPanel4.Location = new System.Drawing.Point(2, 30);
            this.tableLayoutPanel4.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tableLayoutPanel4.Name = "tableLayoutPanel4";
            this.tableLayoutPanel4.RowCount = 2;
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 90F));
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanel4.Size = new System.Drawing.Size(592, 517);
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
            this.OutputWaferCamera.Size = new System.Drawing.Size(588, 461);
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
            this.groupBoxImageView.Size = new System.Drawing.Size(595, 549);
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
            this.tableLayoutPanel5.Location = new System.Drawing.Point(2, 30);
            this.tableLayoutPanel5.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tableLayoutPanel5.Name = "tableLayoutPanel5";
            this.tableLayoutPanel5.RowCount = 2;
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 90F));
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanel5.Size = new System.Drawing.Size(591, 517);
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
            this.InputWaferCamera.Size = new System.Drawing.Size(587, 461);
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
            this.groupBox1.Location = new System.Drawing.Point(601, 2);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(2);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(2);
            this.groupBox1.Size = new System.Drawing.Size(595, 549);
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
            this.tableLayoutPanel3.Location = new System.Drawing.Point(2, 30);
            this.tableLayoutPanel3.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 2;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 90F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(591, 517);
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
            this.IndexOutputCamera.Size = new System.Drawing.Size(587, 461);
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
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1806, 1127);
            this.tableLayoutPanel1.TabIndex = 19;
            // 
            // groupBox3
            // 
            this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox3.Controls.Add(this.tableLayoutPanel6);
            this.groupBox3.Font = new System.Drawing.Font("맑은 고딕", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.groupBox3.Location = new System.Drawing.Point(2, 565);
            this.groupBox3.Margin = new System.Windows.Forms.Padding(2);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Padding = new System.Windows.Forms.Padding(2);
            this.groupBox3.Size = new System.Drawing.Size(1802, 560);
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
            this.tableLayoutPanel6.Controls.Add(this.groupBox4, 2, 0);
            this.tableLayoutPanel6.Controls.Add(this.groupBox_SequenceManual, 0, 0);
            this.tableLayoutPanel6.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel6.Location = new System.Drawing.Point(2, 30);
            this.tableLayoutPanel6.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tableLayoutPanel6.Name = "tableLayoutPanel6";
            this.tableLayoutPanel6.RowCount = 1;
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel6.Size = new System.Drawing.Size(1798, 528);
            this.tableLayoutPanel6.TabIndex = 19;
            // 
            // groupBox4
            // 
            this.groupBox4.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox4.Controls.Add(this.tableLayoutPanel8);
            this.groupBox4.Font = new System.Drawing.Font("맑은 고딕", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.groupBox4.Location = new System.Drawing.Point(1080, 2);
            this.groupBox4.Margin = new System.Windows.Forms.Padding(2);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Padding = new System.Windows.Forms.Padding(2);
            this.groupBox4.Size = new System.Drawing.Size(716, 524);
            this.groupBox4.TabIndex = 20;
            this.groupBox4.TabStop = false;
            // 
            // tableLayoutPanel8
            // 
            this.tableLayoutPanel8.BackColor = System.Drawing.Color.White;
            this.tableLayoutPanel8.ColumnCount = 4;
            this.tableLayoutPanel8.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel8.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel8.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel8.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel8.Controls.Add(this.btnHomeAll, 0, 0);
            this.tableLayoutPanel8.Controls.Add(this.btn_Jog, 0, 4);
            this.tableLayoutPanel8.Controls.Add(this.BtnMeasurementResult, 3, 0);
            this.tableLayoutPanel8.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel8.Location = new System.Drawing.Point(2, 30);
            this.tableLayoutPanel8.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            this.tableLayoutPanel8.Name = "tableLayoutPanel8";
            this.tableLayoutPanel8.RowCount = 5;
            this.tableLayoutPanel8.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel8.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel8.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel8.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel8.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel8.Size = new System.Drawing.Size(712, 492);
            this.tableLayoutPanel8.TabIndex = 4;
            // 
            // btnHomeAll
            // 
            this.btnHomeAll.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnHomeAll.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnHomeAll.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnHomeAll.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnHomeAll.CustomForeColor = System.Drawing.Color.Black;
            this.btnHomeAll.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnHomeAll.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnHomeAll.ForeColor = System.Drawing.Color.Black;
            this.btnHomeAll.ImageSize = new System.Drawing.Size(45, 45);
            this.btnHomeAll.Location = new System.Drawing.Point(6, 6);
            this.btnHomeAll.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.btnHomeAll.Name = "btnHomeAll";
            this.btnHomeAll.Size = new System.Drawing.Size(166, 86);
            this.btnHomeAll.TabIndex = 18;
            this.btnHomeAll.TabStop = false;
            this.btnHomeAll.Text = "Motor Init.";
            this.btnHomeAll.UseVisualStyleBackColor = false;
            this.btnHomeAll.Click += new System.EventHandler(this.btnHomeAll_Click);
            // 
            // btn_Jog
            // 
            this.btn_Jog.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_Jog.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btn_Jog.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_Jog.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_Jog.CustomForeColor = System.Drawing.Color.Black;
            this.btn_Jog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btn_Jog.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_Jog.ForeColor = System.Drawing.Color.Black;
            this.btn_Jog.ImageSize = new System.Drawing.Size(45, 45);
            this.btn_Jog.Location = new System.Drawing.Point(6, 398);
            this.btn_Jog.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.btn_Jog.Name = "btn_Jog";
            this.btn_Jog.Size = new System.Drawing.Size(166, 88);
            this.btn_Jog.TabIndex = 17;
            this.btn_Jog.TabStop = false;
            this.btn_Jog.Text = "Jog Axis";
            this.btn_Jog.UseVisualStyleBackColor = false;
            this.btn_Jog.Click += new System.EventHandler(this.btn_Jog_Click);
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
            this.groupBox_SequenceManual.Size = new System.Drawing.Size(895, 524);
            this.groupBox_SequenceManual.TabIndex = 18;
            this.groupBox_SequenceManual.TabStop = false;
            this.groupBox_SequenceManual.Text = "Sequence Manual";
            // 
            // sequenceManualControl
            // 
            this.sequenceManualControl.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.sequenceManualControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.sequenceManualControl.Location = new System.Drawing.Point(2, 30);
            this.sequenceManualControl.Margin = new System.Windows.Forms.Padding(2);
            this.sequenceManualControl.Name = "sequenceManualControl";
            this.sequenceManualControl.Size = new System.Drawing.Size(891, 492);
            this.sequenceManualControl.TabIndex = 1;
            // 
            // BtnMeasurementResult
            // 
            this.BtnMeasurementResult.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.BtnMeasurementResult.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.BtnMeasurementResult.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.BtnMeasurementResult.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.BtnMeasurementResult.CustomForeColor = System.Drawing.Color.Black;
            this.BtnMeasurementResult.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.BtnMeasurementResult.ForeColor = System.Drawing.Color.Black;
            this.BtnMeasurementResult.ImageSize = new System.Drawing.Size(45, 45);
            this.BtnMeasurementResult.Location = new System.Drawing.Point(540, 6);
            this.BtnMeasurementResult.Margin = new System.Windows.Forms.Padding(6);
            this.BtnMeasurementResult.Name = "BtnMeasurementResult";
            this.BtnMeasurementResult.Size = new System.Drawing.Size(166, 86);
            this.BtnMeasurementResult.TabIndex = 19;
            this.BtnMeasurementResult.TabStop = false;
            this.BtnMeasurementResult.Text = "Measurement Result";
            this.BtnMeasurementResult.UseVisualStyleBackColor = false;
            this.BtnMeasurementResult.Click += new System.EventHandler(this.BtnMeasurementResult_Click);
            // 
            // Operator_Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1806, 1127);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
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
            this.groupBox4.ResumeLayout(false);
            this.tableLayoutPanel8.ResumeLayout(false);
            this.groupBox_SequenceManual.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        #endregion
        private Common.IndividualMenuButton btn_Jog;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel8;
        private Common.IndividualMenuButton btnHomeAll;
        private Common.IndividualMenuButton BtnMeasurementResult;
    }
}