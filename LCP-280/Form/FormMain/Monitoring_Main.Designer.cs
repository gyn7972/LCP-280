using System.Windows.Forms;

namespace QMC.LCP_280.Process
{
    partial class Monitoring_Main
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        private GroupBox groupBoxdieInputControl;
        private QMC.LCP_280.Process.Unit.FormMain.DieInputControl dieInputControl1;

        private GroupBox groupBoxdieIndexSelectControl;
        private QMC.LCP_280.Process.Unit.FormMain.DieIndexSelectControl dieIndexSelectControl1;

        private GroupBox groupBoxdieOutputControl;
        private QMC.LCP_280.Process.Unit.FormMain.DieOutputControl dieOutputControl1;

        private GroupBox groupBoxMeasurementControl;

        private GroupBox groupBoxInputWaferCarrierControl;
        private QMC.LCP_280.Process.Unit.FormMain.InputWaferCarrierControl inputWaferCarrierControl1;

        private GroupBox groupBoxOutputWaferCarrierControl;
        private QMC.LCP_280.Process.Unit.FormMain.OutputWaferCarrierControl outputWaferCarrierControl1;

        private TableLayoutPanel tableLayoutPanel1;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.groupBoxdieInputControl = new System.Windows.Forms.GroupBox();
            this.dieInputControl1 = new QMC.LCP_280.Process.Unit.FormMain.DieInputControl();
            this.groupBoxdieIndexSelectControl = new System.Windows.Forms.GroupBox();
            this.dieIndexSelectControl1 = new QMC.LCP_280.Process.Unit.FormMain.DieIndexSelectControl();
            this.groupBoxdieOutputControl = new System.Windows.Forms.GroupBox();
            this.dieOutputControl1 = new QMC.LCP_280.Process.Unit.FormMain.DieOutputControl();
            this.groupBoxInputWaferCarrierControl = new System.Windows.Forms.GroupBox();
            this.inputWaferCarrierControl1 = new QMC.LCP_280.Process.Unit.FormMain.InputWaferCarrierControl();
            this.groupBoxMeasurementControl = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.lbResultValue = new QMC.Common.CustomControl.CustomBorderLabel();
            this.casSpectrumViewer1 = new QMC.Common.Spectrometer.CASSpectrumViewer();
            this.groupBoxOutputWaferCarrierControl = new System.Windows.Forms.GroupBox();
            this.outputWaferCarrierControl1 = new QMC.LCP_280.Process.Unit.FormMain.OutputWaferCarrierControl();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel8 = new System.Windows.Forms.TableLayoutPanel();
            this.btnTack2 = new QMC.Common.IndividualMenuButton();
            this.btnProcessStatus = new QMC.Common.IndividualMenuButton();
            this.btnTack = new QMC.Common.IndividualMenuButton();
            this.lbContactValue = new System.Windows.Forms.Label();
            this.lbContactTitle = new System.Windows.Forms.Label();
            this.btnMapMatch = new QMC.Common.IndividualMenuButton();
            this.groupBox_SequenceAuto = new System.Windows.Forms.GroupBox();
            this.sequenceAutoControl = new QMC.LCP_280.Process.Unit.FormMain.SequenceAutoControl();
            this.btnManualSeq = new QMC.Common.IndividualMenuButton();
            this.groupBoxdieInputControl.SuspendLayout();
            this.groupBoxdieIndexSelectControl.SuspendLayout();
            this.groupBoxdieOutputControl.SuspendLayout();
            this.groupBoxInputWaferCarrierControl.SuspendLayout();
            this.groupBoxMeasurementControl.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.groupBoxOutputWaferCarrierControl.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel8.SuspendLayout();
            this.groupBox_SequenceAuto.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBoxdieInputControl
            // 
            this.groupBoxdieInputControl.Controls.Add(this.dieInputControl1);
            this.groupBoxdieInputControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxdieInputControl.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.groupBoxdieInputControl.Location = new System.Drawing.Point(3, 3);
            this.groupBoxdieInputControl.Name = "groupBoxdieInputControl";
            this.groupBoxdieInputControl.Size = new System.Drawing.Size(563, 557);
            this.groupBoxdieInputControl.TabIndex = 0;
            this.groupBoxdieInputControl.TabStop = false;
            this.groupBoxdieInputControl.Text = "Wafer Stage && Arm";
            // 
            // dieInputControl1
            // 
            this.dieInputControl1.BackColor = System.Drawing.Color.White;
            this.dieInputControl1.CenterOnPivot = true;
            this.dieInputControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dieInputControl1.Location = new System.Drawing.Point(3, 27);
            this.dieInputControl1.Name = "dieInputControl1";
            this.dieInputControl1.ShowPickedHistory = true;
            this.dieInputControl1.Size = new System.Drawing.Size(557, 527);
            this.dieInputControl1.TabIndex = 0;
            this.dieInputControl1.WaferMirrorView = QMC.LCP_280.Process.Component.MeasurementRecipe.MapMirrorOption.None;
            this.dieInputControl1.WaferPathPrimaryAxis = QMC.LCP_280.Process.Component.MeasurementRecipe.MapPathPrimaryAxis.XFirst;
            this.dieInputControl1.WaferPathStartCorner = QMC.LCP_280.Process.Component.MeasurementRecipe.MapPathStartCorner.BottomLeft;
            this.dieInputControl1.WaferPathTraversalMode = QMC.LCP_280.Process.Component.MeasurementRecipe.MapPathTraversalMode.Serpentine;
            this.dieInputControl1.WaferRotateView = QMC.LCP_280.Process.Component.MeasurementRecipe.MapRotateOption.None;
            // 
            // groupBoxdieIndexSelectControl
            // 
            this.groupBoxdieIndexSelectControl.Controls.Add(this.dieIndexSelectControl1);
            this.groupBoxdieIndexSelectControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxdieIndexSelectControl.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Bold);
            this.groupBoxdieIndexSelectControl.Location = new System.Drawing.Point(572, 3);
            this.groupBoxdieIndexSelectControl.Name = "groupBoxdieIndexSelectControl";
            this.groupBoxdieIndexSelectControl.Size = new System.Drawing.Size(563, 557);
            this.groupBoxdieIndexSelectControl.TabIndex = 1;
            this.groupBoxdieIndexSelectControl.TabStop = false;
            this.groupBoxdieIndexSelectControl.Text = "Index";
            // 
            // dieIndexSelectControl1
            // 
            this.dieIndexSelectControl1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.dieIndexSelectControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dieIndexSelectControl1.Location = new System.Drawing.Point(3, 27);
            this.dieIndexSelectControl1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.dieIndexSelectControl1.Name = "dieIndexSelectControl1";
            this.dieIndexSelectControl1.Size = new System.Drawing.Size(557, 527);
            this.dieIndexSelectControl1.TabIndex = 1;
            // 
            // groupBoxdieOutputControl
            // 
            this.groupBoxdieOutputControl.Controls.Add(this.dieOutputControl1);
            this.groupBoxdieOutputControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxdieOutputControl.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.groupBoxdieOutputControl.Location = new System.Drawing.Point(1141, 3);
            this.groupBoxdieOutputControl.Name = "groupBoxdieOutputControl";
            this.groupBoxdieOutputControl.Size = new System.Drawing.Size(563, 557);
            this.groupBoxdieOutputControl.TabIndex = 2;
            this.groupBoxdieOutputControl.TabStop = false;
            this.groupBoxdieOutputControl.Text = "Bin Stage && Arm";
            // 
            // dieOutputControl1
            // 
            this.dieOutputControl1.BackColor = System.Drawing.Color.White;
            this.dieOutputControl1.BinMirrorView = QMC.LCP_280.Process.Component.MeasurementRecipe.MapMirrorOption.None;
            this.dieOutputControl1.BinPathPrimaryAxis = QMC.LCP_280.Process.Component.MeasurementRecipe.MapPathPrimaryAxis.XFirst;
            this.dieOutputControl1.BinPathStartCorner = QMC.LCP_280.Process.Component.MeasurementRecipe.MapPathStartCorner.BottomLeft;
            this.dieOutputControl1.BinPathTraversalMode = QMC.LCP_280.Process.Component.MeasurementRecipe.MapPathTraversalMode.Serpentine;
            this.dieOutputControl1.BinRotateView = QMC.LCP_280.Process.Component.MeasurementRecipe.MapRotateOption.None;
            this.dieOutputControl1.CenterOnPivot = true;
            this.dieOutputControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dieOutputControl1.Location = new System.Drawing.Point(3, 27);
            this.dieOutputControl1.Name = "dieOutputControl1";
            this.dieOutputControl1.Size = new System.Drawing.Size(557, 527);
            this.dieOutputControl1.TabIndex = 2;
            // 
            // groupBoxInputWaferCarrierControl
            // 
            this.groupBoxInputWaferCarrierControl.Controls.Add(this.inputWaferCarrierControl1);
            this.groupBoxInputWaferCarrierControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxInputWaferCarrierControl.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Bold);
            this.groupBoxInputWaferCarrierControl.Location = new System.Drawing.Point(3, 566);
            this.groupBoxInputWaferCarrierControl.Name = "groupBoxInputWaferCarrierControl";
            this.groupBoxInputWaferCarrierControl.Size = new System.Drawing.Size(563, 557);
            this.groupBoxInputWaferCarrierControl.TabIndex = 3;
            this.groupBoxInputWaferCarrierControl.TabStop = false;
            this.groupBoxInputWaferCarrierControl.Text = "Wafer Carrier";
            // 
            // inputWaferCarrierControl1
            // 
            this.inputWaferCarrierControl1.BackColor = System.Drawing.Color.White;
            this.inputWaferCarrierControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.inputWaferCarrierControl1.Location = new System.Drawing.Point(3, 27);
            this.inputWaferCarrierControl1.Name = "inputWaferCarrierControl1";
            this.inputWaferCarrierControl1.Size = new System.Drawing.Size(557, 527);
            this.inputWaferCarrierControl1.TabIndex = 4;
            // 
            // groupBoxMeasurementControl
            // 
            this.groupBoxMeasurementControl.Controls.Add(this.tableLayoutPanel2);
            this.groupBoxMeasurementControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxMeasurementControl.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Bold);
            this.groupBoxMeasurementControl.Location = new System.Drawing.Point(572, 566);
            this.groupBoxMeasurementControl.Name = "groupBoxMeasurementControl";
            this.groupBoxMeasurementControl.Padding = new System.Windows.Forms.Padding(8);
            this.groupBoxMeasurementControl.Size = new System.Drawing.Size(563, 557);
            this.groupBoxMeasurementControl.TabIndex = 4;
            this.groupBoxMeasurementControl.TabStop = false;
            this.groupBoxMeasurementControl.Text = "Measurement";
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 1;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Controls.Add(this.lbResultValue, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.casSpectrumViewer1, 0, 1);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(8, 32);
            this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(4);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 2;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 120F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(547, 517);
            this.tableLayoutPanel2.TabIndex = 0;
            // 
            // lbResultValue
            // 
            this.lbResultValue.BackColor = System.Drawing.Color.Black;
            this.lbResultValue.BorderColor = System.Drawing.Color.Black;
            this.lbResultValue.BorderWidth = 1;
            this.lbResultValue.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbResultValue.Font = new System.Drawing.Font("Arial", 20.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbResultValue.ForeColor = System.Drawing.Color.Lime;
            this.lbResultValue.Location = new System.Drawing.Point(4, 4);
            this.lbResultValue.Margin = new System.Windows.Forms.Padding(4);
            this.lbResultValue.Name = "lbResultValue";
            this.lbResultValue.Size = new System.Drawing.Size(539, 112);
            this.lbResultValue.TabIndex = 23;
            this.lbResultValue.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // casSpectrumViewer1
            // 
            this.casSpectrumViewer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.casSpectrumViewer1.Location = new System.Drawing.Point(6, 126);
            this.casSpectrumViewer1.Margin = new System.Windows.Forms.Padding(6);
            this.casSpectrumViewer1.Name = "casSpectrumViewer1";
            this.casSpectrumViewer1.Size = new System.Drawing.Size(535, 385);
            this.casSpectrumViewer1.TabIndex = 0;
            // 
            // groupBoxOutputWaferCarrierControl
            // 
            this.groupBoxOutputWaferCarrierControl.Controls.Add(this.outputWaferCarrierControl1);
            this.groupBoxOutputWaferCarrierControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxOutputWaferCarrierControl.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Bold);
            this.groupBoxOutputWaferCarrierControl.Location = new System.Drawing.Point(1141, 566);
            this.groupBoxOutputWaferCarrierControl.Name = "groupBoxOutputWaferCarrierControl";
            this.groupBoxOutputWaferCarrierControl.Size = new System.Drawing.Size(563, 557);
            this.groupBoxOutputWaferCarrierControl.TabIndex = 3;
            this.groupBoxOutputWaferCarrierControl.TabStop = false;
            this.groupBoxOutputWaferCarrierControl.Text = "Bin Carrier";
            // 
            // outputWaferCarrierControl1
            // 
            this.outputWaferCarrierControl1.BackColor = System.Drawing.Color.White;
            this.outputWaferCarrierControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.outputWaferCarrierControl1.Location = new System.Drawing.Point(3, 27);
            this.outputWaferCarrierControl1.Name = "outputWaferCarrierControl1";
            this.outputWaferCarrierControl1.Size = new System.Drawing.Size(557, 527);
            this.outputWaferCarrierControl1.TabIndex = 4;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 4;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 189F));
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel8, 3, 0);
            this.tableLayoutPanel1.Controls.Add(this.groupBoxOutputWaferCarrierControl, 2, 1);
            this.tableLayoutPanel1.Controls.Add(this.groupBoxMeasurementControl, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.groupBoxInputWaferCarrierControl, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.groupBoxdieIndexSelectControl, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.groupBoxdieOutputControl, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.groupBoxdieInputControl, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.groupBox_SequenceAuto, 3, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(4);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1896, 1126);
            this.tableLayoutPanel1.TabIndex = 1;
            // 
            // tableLayoutPanel8
            // 
            this.tableLayoutPanel8.BackColor = System.Drawing.Color.White;
            this.tableLayoutPanel8.ColumnCount = 1;
            this.tableLayoutPanel8.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel8.Controls.Add(this.btnTack2, 0, 2);
            this.tableLayoutPanel8.Controls.Add(this.btnProcessStatus, 0, 0);
            this.tableLayoutPanel8.Controls.Add(this.btnTack, 0, 1);
            this.tableLayoutPanel8.Controls.Add(this.lbContactValue, 0, 9);
            this.tableLayoutPanel8.Controls.Add(this.lbContactTitle, 0, 8);
            this.tableLayoutPanel8.Controls.Add(this.btnMapMatch, 0, 7);
            this.tableLayoutPanel8.Controls.Add(this.btnManualSeq, 0, 6);
            this.tableLayoutPanel8.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel8.Location = new System.Drawing.Point(1713, 4);
            this.tableLayoutPanel8.Margin = new System.Windows.Forms.Padding(6, 4, 6, 4);
            this.tableLayoutPanel8.Name = "tableLayoutPanel8";
            this.tableLayoutPanel8.RowCount = 10;
            this.tableLayoutPanel8.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanel8.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanel8.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanel8.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanel8.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanel8.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanel8.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanel8.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanel8.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanel8.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanel8.Size = new System.Drawing.Size(177, 555);
            this.tableLayoutPanel8.TabIndex = 21;
            // 
            // btnTack2
            // 
            this.btnTack2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnTack2.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnTack2.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnTack2.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnTack2.CustomForeColor = System.Drawing.Color.Black;
            this.btnTack2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnTack2.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnTack2.ForeColor = System.Drawing.Color.Black;
            this.btnTack2.ImageSize = new System.Drawing.Size(45, 45);
            this.btnTack2.Location = new System.Drawing.Point(6, 116);
            this.btnTack2.Margin = new System.Windows.Forms.Padding(6);
            this.btnTack2.Name = "btnTack2";
            this.btnTack2.Size = new System.Drawing.Size(165, 43);
            this.btnTack2.TabIndex = 23;
            this.btnTack2.TabStop = false;
            this.btnTack2.Text = "Sub T/T";
            this.btnTack2.UseVisualStyleBackColor = false;
            this.btnTack2.Click += new System.EventHandler(this.btnTack2_Click);
            // 
            // btnProcessStatus
            // 
            this.btnProcessStatus.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnProcessStatus.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnProcessStatus.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnProcessStatus.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnProcessStatus.CustomForeColor = System.Drawing.Color.Black;
            this.btnProcessStatus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnProcessStatus.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnProcessStatus.ForeColor = System.Drawing.Color.Black;
            this.btnProcessStatus.ImageSize = new System.Drawing.Size(45, 45);
            this.btnProcessStatus.Location = new System.Drawing.Point(6, 6);
            this.btnProcessStatus.Margin = new System.Windows.Forms.Padding(6);
            this.btnProcessStatus.Name = "btnProcessStatus";
            this.btnProcessStatus.Size = new System.Drawing.Size(165, 43);
            this.btnProcessStatus.TabIndex = 21;
            this.btnProcessStatus.TabStop = false;
            this.btnProcessStatus.Text = "Status";
            this.btnProcessStatus.UseVisualStyleBackColor = false;
            this.btnProcessStatus.Click += new System.EventHandler(this.btnProcessStatus_Click);
            // 
            // btnTack
            // 
            this.btnTack.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnTack.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnTack.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnTack.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnTack.CustomForeColor = System.Drawing.Color.Black;
            this.btnTack.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnTack.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnTack.ForeColor = System.Drawing.Color.Black;
            this.btnTack.ImageSize = new System.Drawing.Size(45, 45);
            this.btnTack.Location = new System.Drawing.Point(6, 61);
            this.btnTack.Margin = new System.Windows.Forms.Padding(6);
            this.btnTack.Name = "btnTack";
            this.btnTack.Size = new System.Drawing.Size(165, 43);
            this.btnTack.TabIndex = 19;
            this.btnTack.TabStop = false;
            this.btnTack.Text = "ToTal T/T";
            this.btnTack.UseVisualStyleBackColor = false;
            this.btnTack.Click += new System.EventHandler(this.btnTack_Click);
            // 
            // lbContactValue
            // 
            this.lbContactValue.AutoSize = true;
            this.lbContactValue.Dock = System.Windows.Forms.DockStyle.Top;
            this.lbContactValue.Font = new System.Drawing.Font("맑은 고딕", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lbContactValue.Location = new System.Drawing.Point(3, 495);
            this.lbContactValue.Name = "lbContactValue";
            this.lbContactValue.Size = new System.Drawing.Size(171, 28);
            this.lbContactValue.TabIndex = 25;
            this.lbContactValue.Text = "99999999999";
            this.lbContactValue.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbContactTitle
            // 
            this.lbContactTitle.AutoSize = true;
            this.lbContactTitle.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.lbContactTitle.Font = new System.Drawing.Font("맑은 고딕", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lbContactTitle.Location = new System.Drawing.Point(3, 467);
            this.lbContactTitle.Name = "lbContactTitle";
            this.lbContactTitle.Size = new System.Drawing.Size(171, 28);
            this.lbContactTitle.TabIndex = 24;
            this.lbContactTitle.Text = "Contact Count";
            this.lbContactTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnMapMatch
            // 
            this.btnMapMatch.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnMapMatch.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnMapMatch.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnMapMatch.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnMapMatch.CustomForeColor = System.Drawing.Color.Black;
            this.btnMapMatch.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnMapMatch.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnMapMatch.ForeColor = System.Drawing.Color.Black;
            this.btnMapMatch.ImageSize = new System.Drawing.Size(45, 45);
            this.btnMapMatch.Location = new System.Drawing.Point(6, 391);
            this.btnMapMatch.Margin = new System.Windows.Forms.Padding(6);
            this.btnMapMatch.Name = "btnMapMatch";
            this.btnMapMatch.Size = new System.Drawing.Size(165, 43);
            this.btnMapMatch.TabIndex = 20;
            this.btnMapMatch.TabStop = false;
            this.btnMapMatch.Text = "MapMatch";
            this.btnMapMatch.UseVisualStyleBackColor = false;
            this.btnMapMatch.Visible = false;
            this.btnMapMatch.Click += new System.EventHandler(this.btnMapMatch_Click);
            // 
            // groupBox_SequenceAuto
            // 
            this.groupBox_SequenceAuto.Controls.Add(this.sequenceAutoControl);
            this.groupBox_SequenceAuto.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox_SequenceAuto.Font = new System.Drawing.Font("맑은 고딕", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.groupBox_SequenceAuto.Location = new System.Drawing.Point(1710, 566);
            this.groupBox_SequenceAuto.Name = "groupBox_SequenceAuto";
            this.groupBox_SequenceAuto.Size = new System.Drawing.Size(183, 557);
            this.groupBox_SequenceAuto.TabIndex = 20;
            this.groupBox_SequenceAuto.TabStop = false;
            this.groupBox_SequenceAuto.Text = "Sequence Auto";
            // 
            // sequenceAutoControl
            // 
            this.sequenceAutoControl.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.sequenceAutoControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.sequenceAutoControl.Location = new System.Drawing.Point(3, 31);
            this.sequenceAutoControl.Margin = new System.Windows.Forms.Padding(4);
            this.sequenceAutoControl.Name = "sequenceAutoControl";
            this.sequenceAutoControl.Size = new System.Drawing.Size(177, 523);
            this.sequenceAutoControl.TabIndex = 1;
            // 
            // btnManualSeq
            // 
            this.btnManualSeq.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnManualSeq.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnManualSeq.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnManualSeq.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnManualSeq.CustomForeColor = System.Drawing.Color.Black;
            this.btnManualSeq.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnManualSeq.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnManualSeq.ForeColor = System.Drawing.Color.Black;
            this.btnManualSeq.ImageSize = new System.Drawing.Size(45, 45);
            this.btnManualSeq.Location = new System.Drawing.Point(6, 336);
            this.btnManualSeq.Margin = new System.Windows.Forms.Padding(6);
            this.btnManualSeq.Name = "btnManualSeq";
            this.btnManualSeq.Size = new System.Drawing.Size(165, 43);
            this.btnManualSeq.TabIndex = 26;
            this.btnManualSeq.TabStop = false;
            this.btnManualSeq.Text = "ManualSeq";
            this.btnManualSeq.UseVisualStyleBackColor = false;
            this.btnManualSeq.Click += new System.EventHandler(this.btnManualSeq_Click);
            // 
            // Monitoring_Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(144F, 144F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(1896, 1126);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "Monitoring_Main";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Monitoring";
            this.Load += new System.EventHandler(this.Monitoring_Main_Load);
            this.groupBoxdieInputControl.ResumeLayout(false);
            this.groupBoxdieIndexSelectControl.ResumeLayout(false);
            this.groupBoxdieOutputControl.ResumeLayout(false);
            this.groupBoxInputWaferCarrierControl.ResumeLayout(false);
            this.groupBoxMeasurementControl.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.groupBoxOutputWaferCarrierControl.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel8.ResumeLayout(false);
            this.tableLayoutPanel8.PerformLayout();
            this.groupBox_SequenceAuto.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private GroupBox groupBox_SequenceAuto;
        private Unit.FormMain.SequenceAutoControl sequenceAutoControl;
        private TableLayoutPanel tableLayoutPanel2;
        private Common.Spectrometer.CASSpectrumViewer casSpectrumViewer1;
        private Common.CustomControl.CustomBorderLabel lbResultValue;
        private TableLayoutPanel tableLayoutPanel8;
        private Common.IndividualMenuButton btnTack;
        private Common.IndividualMenuButton btnMapMatch;
        private Common.IndividualMenuButton btnProcessStatus;
        private Common.IndividualMenuButton btnTack2;
        private Label lbContactTitle;
        private Label lbContactValue;
        private Common.IndividualMenuButton btnManualSeq;
    }
}