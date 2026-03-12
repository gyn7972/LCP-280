namespace QMC.LCP_280.Process.Unit.FormMain
{
    partial class Operator_Main
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
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
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.IndexMAlignCamera = new QMC.Common.Vision.VisionImageViewer();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel8 = new System.Windows.Forms.TableLayoutPanel();
            this.btn_Jog = new QMC.Common.IndividualMenuButton();
            this.BtnMeasurementResult = new QMC.Common.IndividualMenuButton();
            this.groupBox_SequenceManual = new System.Windows.Forms.GroupBox();
            this.sequenceManualControl = new QMC.LCP_280.Process.Unit.FormMain.SequenceManualControl();
            this.btnHomeAll = new QMC.Common.IndividualMenuButton();
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
            this.groupBox5.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.IndexMAlignCamera)).BeginInit();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.tableLayoutPanel8.SuspendLayout();
            this.groupBox_SequenceManual.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.tableLayoutPanel4);
            this.groupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox2.Font = new System.Drawing.Font("맑은 고딕", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.groupBox2.Location = new System.Drawing.Point(2, 459);
            this.groupBox2.Margin = new System.Windows.Forms.Padding(2);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(2);
            this.groupBox2.Size = new System.Drawing.Size(445, 453);
            this.groupBox2.TabIndex = 18;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Bin Camera";
            // 
            // tableLayoutPanel4
            // 
            this.tableLayoutPanel4.ColumnCount = 1;
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel4.Controls.Add(this.OutputWaferCamera, 0, 0);
            this.tableLayoutPanel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel4.Location = new System.Drawing.Point(2, 30);
            this.tableLayoutPanel4.Margin = new System.Windows.Forms.Padding(4);
            this.tableLayoutPanel4.Name = "tableLayoutPanel4";
            this.tableLayoutPanel4.RowCount = 2;
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 98.9011F));
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 1.098901F));
            this.tableLayoutPanel4.Size = new System.Drawing.Size(441, 421);
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
            this.OutputWaferCamera.Size = new System.Drawing.Size(437, 412);
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
            this.groupBoxImageView.Size = new System.Drawing.Size(445, 453);
            this.groupBoxImageView.TabIndex = 16;
            this.groupBoxImageView.TabStop = false;
            this.groupBoxImageView.Text = "Wafer Camera";
            // 
            // tableLayoutPanel5
            // 
            this.tableLayoutPanel5.ColumnCount = 1;
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel5.Controls.Add(this.InputWaferCamera, 0, 0);
            this.tableLayoutPanel5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel5.Location = new System.Drawing.Point(2, 30);
            this.tableLayoutPanel5.Margin = new System.Windows.Forms.Padding(4);
            this.tableLayoutPanel5.Name = "tableLayoutPanel5";
            this.tableLayoutPanel5.RowCount = 2;
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 98.9011F));
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 1.098901F));
            this.tableLayoutPanel5.Size = new System.Drawing.Size(441, 421);
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
            this.InputWaferCamera.Size = new System.Drawing.Size(437, 412);
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
            this.groupBox1.Location = new System.Drawing.Point(451, 459);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(2);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(2);
            this.groupBox1.Size = new System.Drawing.Size(445, 453);
            this.groupBox1.TabIndex = 17;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Index Unload Camera";
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 1;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.Controls.Add(this.IndexOutputCamera, 0, 0);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(2, 30);
            this.tableLayoutPanel3.Margin = new System.Windows.Forms.Padding(4);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 2;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 98.9011F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 1.098901F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(441, 421);
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
            this.IndexOutputCamera.Size = new System.Drawing.Size(437, 412);
            this.IndexOutputCamera.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.IndexOutputCamera.TabIndex = 12;
            this.IndexOutputCamera.TabStop = false;
            this.IndexOutputCamera.UpdateDelayTime = 80;
            this.IndexOutputCamera.VisibleCrossLine = true;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33332F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33334F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel1.Controls.Add(this.groupBox5, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.groupBox3, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.groupBox_SequenceManual, 2, 1);
            this.tableLayoutPanel1.Controls.Add(this.groupBox2, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.groupBox1, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.groupBoxImageView, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(4);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 45.49517F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 45.49518F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 9.009654F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1348, 1005);
            this.tableLayoutPanel1.TabIndex = 19;
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.tableLayoutPanel2);
            this.groupBox5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox5.Font = new System.Drawing.Font("맑은 고딕", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.groupBox5.Location = new System.Drawing.Point(451, 2);
            this.groupBox5.Margin = new System.Windows.Forms.Padding(2);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Padding = new System.Windows.Forms.Padding(2);
            this.groupBox5.Size = new System.Drawing.Size(445, 453);
            this.groupBox5.TabIndex = 18;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "Index M-Align Camera";
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 1;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Controls.Add(this.IndexMAlignCamera, 0, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(2, 30);
            this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(4);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 2;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 98.9011F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 1.098901F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(441, 421);
            this.tableLayoutPanel2.TabIndex = 4;
            // 
            // IndexMAlignCamera
            // 
            this.IndexMAlignCamera.BackColor = System.Drawing.Color.Black;
            this.IndexMAlignCamera.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.IndexMAlignCamera.Camera = null;
            this.IndexMAlignCamera.CameraSwitch = null;
            this.IndexMAlignCamera.Dock = System.Windows.Forms.DockStyle.Fill;
            this.IndexMAlignCamera.FrameRate = 1D;
            this.IndexMAlignCamera.InputImage = null;
            this.IndexMAlignCamera.IsViewCustomizedImage = false;
            this.IndexMAlignCamera.Location = new System.Drawing.Point(2, 2);
            this.IndexMAlignCamera.Margin = new System.Windows.Forms.Padding(2);
            this.IndexMAlignCamera.Name = "IndexMAlignCamera";
            this.IndexMAlignCamera.OperatingType = QMC.Common.Vision.VisionImageViewer.OperatingTypes.Center;
            this.IndexMAlignCamera.Simulated = false;
            this.IndexMAlignCamera.Size = new System.Drawing.Size(437, 412);
            this.IndexMAlignCamera.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.IndexMAlignCamera.TabIndex = 12;
            this.IndexMAlignCamera.TabStop = false;
            this.IndexMAlignCamera.UpdateDelayTime = 80;
            this.IndexMAlignCamera.VisibleCrossLine = true;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.groupBox4);
            this.groupBox3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox3.Font = new System.Drawing.Font("맑은 고딕", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.groupBox3.Location = new System.Drawing.Point(900, 2);
            this.groupBox3.Margin = new System.Windows.Forms.Padding(2);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Padding = new System.Windows.Forms.Padding(2);
            this.groupBox3.Size = new System.Drawing.Size(446, 453);
            this.groupBox3.TabIndex = 17;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Manual Control";
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.tableLayoutPanel8);
            this.groupBox4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox4.Font = new System.Drawing.Font("맑은 고딕", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.groupBox4.Location = new System.Drawing.Point(2, 30);
            this.groupBox4.Margin = new System.Windows.Forms.Padding(2);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Padding = new System.Windows.Forms.Padding(2);
            this.groupBox4.Size = new System.Drawing.Size(442, 421);
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
            this.tableLayoutPanel8.Controls.Add(this.btn_Jog, 1, 0);
            this.tableLayoutPanel8.Controls.Add(this.BtnMeasurementResult, 0, 0);
            this.tableLayoutPanel8.Controls.Add(this.btnHomeAll, 3, 4);
            this.tableLayoutPanel8.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel8.Location = new System.Drawing.Point(2, 30);
            this.tableLayoutPanel8.Margin = new System.Windows.Forms.Padding(6, 4, 6, 4);
            this.tableLayoutPanel8.Name = "tableLayoutPanel8";
            this.tableLayoutPanel8.RowCount = 5;
            this.tableLayoutPanel8.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel8.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel8.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel8.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel8.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel8.Size = new System.Drawing.Size(438, 389);
            this.tableLayoutPanel8.TabIndex = 4;
            // 
            // btn_Jog
            // 
            this.btn_Jog.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_Jog.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btn_Jog.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_Jog.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_Jog.CustomForeColor = System.Drawing.Color.Black;
            this.btn_Jog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btn_Jog.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_Jog.ForeColor = System.Drawing.Color.Black;
            this.btn_Jog.ImageSize = new System.Drawing.Size(45, 45);
            this.btn_Jog.Location = new System.Drawing.Point(115, 6);
            this.btn_Jog.Margin = new System.Windows.Forms.Padding(6);
            this.btn_Jog.Name = "btn_Jog";
            this.btn_Jog.Size = new System.Drawing.Size(97, 65);
            this.btn_Jog.TabIndex = 17;
            this.btn_Jog.TabStop = false;
            this.btn_Jog.Text = "Jog";
            this.btn_Jog.UseVisualStyleBackColor = false;
            this.btn_Jog.Click += new System.EventHandler(this.btn_Jog_Click);
            // 
            // BtnMeasurementResult
            // 
            this.BtnMeasurementResult.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.BtnMeasurementResult.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.BtnMeasurementResult.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.BtnMeasurementResult.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.BtnMeasurementResult.CustomForeColor = System.Drawing.Color.Black;
            this.BtnMeasurementResult.Dock = System.Windows.Forms.DockStyle.Fill;
            this.BtnMeasurementResult.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.BtnMeasurementResult.ForeColor = System.Drawing.Color.Black;
            this.BtnMeasurementResult.ImageSize = new System.Drawing.Size(45, 45);
            this.BtnMeasurementResult.Location = new System.Drawing.Point(6, 6);
            this.BtnMeasurementResult.Margin = new System.Windows.Forms.Padding(6);
            this.BtnMeasurementResult.Name = "BtnMeasurementResult";
            this.BtnMeasurementResult.Size = new System.Drawing.Size(97, 65);
            this.BtnMeasurementResult.TabIndex = 19;
            this.BtnMeasurementResult.TabStop = false;
            this.BtnMeasurementResult.Text = "Measurement Result";
            this.BtnMeasurementResult.UseVisualStyleBackColor = false;
            this.BtnMeasurementResult.Click += new System.EventHandler(this.BtnMeasurementResult_Click);
            // 
            // groupBox_SequenceManual
            // 
            this.groupBox_SequenceManual.Controls.Add(this.sequenceManualControl);
            this.groupBox_SequenceManual.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox_SequenceManual.Font = new System.Drawing.Font("맑은 고딕", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.groupBox_SequenceManual.Location = new System.Drawing.Point(900, 459);
            this.groupBox_SequenceManual.Margin = new System.Windows.Forms.Padding(2);
            this.groupBox_SequenceManual.Name = "groupBox_SequenceManual";
            this.groupBox_SequenceManual.Padding = new System.Windows.Forms.Padding(2);
            this.groupBox_SequenceManual.Size = new System.Drawing.Size(446, 453);
            this.groupBox_SequenceManual.TabIndex = 18;
            this.groupBox_SequenceManual.TabStop = false;
            this.groupBox_SequenceManual.Text = "Manual Sequence ";
            // 
            // sequenceManualControl
            // 
            this.sequenceManualControl.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.sequenceManualControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.sequenceManualControl.Location = new System.Drawing.Point(2, 30);
            this.sequenceManualControl.Margin = new System.Windows.Forms.Padding(2);
            this.sequenceManualControl.Name = "sequenceManualControl";
            this.sequenceManualControl.Size = new System.Drawing.Size(442, 421);
            this.sequenceManualControl.TabIndex = 1;
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
            this.btnHomeAll.Location = new System.Drawing.Point(333, 314);
            this.btnHomeAll.Margin = new System.Windows.Forms.Padding(6);
            this.btnHomeAll.Name = "btnHomeAll";
            this.btnHomeAll.Size = new System.Drawing.Size(99, 69);
            this.btnHomeAll.TabIndex = 18;
            this.btnHomeAll.TabStop = false;
            this.btnHomeAll.Text = "Init.";
            this.btnHomeAll.UseVisualStyleBackColor = false;
            this.btnHomeAll.Click += new System.EventHandler(this.btnHomeAll_Click);
            // 
            // Operator_Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(144F, 144F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(1348, 1005);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "Operator_Main";
            this.Text = "Operator_Main";
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
            this.groupBox5.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.IndexMAlignCamera)).EndInit();
            this.groupBox3.ResumeLayout(false);
            this.groupBox4.ResumeLayout(false);
            this.tableLayoutPanel8.ResumeLayout(false);
            this.groupBox_SequenceManual.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        #endregion
        private Common.IndividualMenuButton btn_Jog;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel8;
        private Common.IndividualMenuButton BtnMeasurementResult;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private Common.Vision.VisionImageViewer IndexMAlignCamera;
        private Common.IndividualMenuButton btnHomeAll;
    }
}