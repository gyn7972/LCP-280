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
            this.groupBoxManual = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
            this.btnMapping = new QMC.Common.IndividualMenuButton();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.checkBoxTest = new System.Windows.Forms.CheckBox();
            this.checkBoxSimulation = new System.Windows.Forms.CheckBox();
            this.tableLayoutPanel6 = new System.Windows.Forms.TableLayoutPanel();
            this.buttonRequstInput = new QMC.Common.IndividualMenuButton();
            this.groupBoxImageView = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel5 = new System.Windows.Forms.TableLayoutPanel();
            this._InputWaferCameraviewer = new QMC.Common.Vision.VisionImageViewer();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel8 = new System.Windows.Forms.TableLayoutPanel();
            this.tabControlManualSeqInputWafer = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.tableLayoutPanel9 = new System.Windows.Forms.TableLayoutPanel();
            this.teachingPositionControl = new QMC.LCP_280.Process.Component.TeachingPositionControl();
            this.waferMapView = new QMC.LCP_280.Process.Component.WaferMapView();
            this.dioControl = new QMC.LCP_280.Process.Component.DIOControl();
            this.manualSequenceControlInputCassette = new QMC.LCP_280.Process.Component.ManualSequenceControl();
            this.manualSequenceControlInputFeeder = new QMC.LCP_280.Process.Component.ManualSequenceControl();
            this.manualSequenceControlInputWaferStage = new QMC.LCP_280.Process.Component.ManualSequenceControl();
            this.groupBoxManual.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.tableLayoutPanel4.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.tableLayoutPanel6.SuspendLayout();
            this.groupBoxImageView.SuspendLayout();
            this.tableLayoutPanel5.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._InputWaferCameraviewer)).BeginInit();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel8.SuspendLayout();
            this.tabControlManualSeqInputWafer.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBoxManual
            // 
            this.groupBoxManual.Controls.Add(this.tableLayoutPanel2);
            this.groupBoxManual.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxManual.Font = new System.Drawing.Font("맑은 고딕", 10F);
            this.groupBoxManual.Location = new System.Drawing.Point(507, 377);
            this.groupBoxManual.Margin = new System.Windows.Forms.Padding(2);
            this.groupBoxManual.Name = "groupBoxManual";
            this.groupBoxManual.Padding = new System.Windows.Forms.Padding(2);
            this.groupBoxManual.Size = new System.Drawing.Size(375, 372);
            this.groupBoxManual.TabIndex = 19;
            this.groupBoxManual.TabStop = false;
            this.groupBoxManual.Text = "Manual";
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 2;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.Controls.Add(this.tableLayoutPanel4, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.tableLayoutPanel3, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.tableLayoutPanel6, 1, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(2, 20);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 2;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 80F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(371, 350);
            this.tableLayoutPanel2.TabIndex = 22;
            // 
            // tableLayoutPanel4
            // 
            this.tableLayoutPanel4.ColumnCount = 1;
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel4.Controls.Add(this.waferMapView, 0, 0);
            this.tableLayoutPanel4.Controls.Add(this.btnMapping, 0, 1);
            this.tableLayoutPanel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel4.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel4.Name = "tableLayoutPanel4";
            this.tableLayoutPanel4.RowCount = 2;
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 87F));
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 13F));
            this.tableLayoutPanel4.Size = new System.Drawing.Size(179, 274);
            this.tableLayoutPanel4.TabIndex = 18;
            // 
            // btnMapping
            // 
            this.btnMapping.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnMapping.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnMapping.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnMapping.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnMapping.CustomForeColor = System.Drawing.Color.Black;
            this.btnMapping.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnMapping.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnMapping.ForeColor = System.Drawing.Color.Black;
            this.btnMapping.ImageSize = new System.Drawing.Size(45, 45);
            this.btnMapping.Location = new System.Drawing.Point(2, 241);
            this.btnMapping.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.btnMapping.Name = "btnMapping";
            this.btnMapping.Size = new System.Drawing.Size(175, 30);
            this.btnMapping.TabIndex = 17;
            this.btnMapping.TabStop = false;
            this.btnMapping.Text = "Mapping";
            this.btnMapping.UseVisualStyleBackColor = false;
            this.btnMapping.Click += new System.EventHandler(this.btnMapping_Click);
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 1;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel3.Controls.Add(this.checkBoxTest, 0, 0);
            this.tableLayoutPanel3.Controls.Add(this.checkBoxSimulation, 0, 1);
            this.tableLayoutPanel3.Location = new System.Drawing.Point(3, 283);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 2;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(179, 63);
            this.tableLayoutPanel3.TabIndex = 20;
            // 
            // checkBoxTest
            // 
            this.checkBoxTest.AutoSize = true;
            this.checkBoxTest.Dock = System.Windows.Forms.DockStyle.Fill;
            this.checkBoxTest.Location = new System.Drawing.Point(2, 2);
            this.checkBoxTest.Margin = new System.Windows.Forms.Padding(2);
            this.checkBoxTest.Name = "checkBoxTest";
            this.checkBoxTest.Size = new System.Drawing.Size(175, 27);
            this.checkBoxTest.TabIndex = 18;
            this.checkBoxTest.Text = "DryRun";
            this.checkBoxTest.UseVisualStyleBackColor = true;
            this.checkBoxTest.Visible = false;
            this.checkBoxTest.CheckedChanged += new System.EventHandler(this.checkBoxTest_CheckedChanged);
            // 
            // checkBoxSimulation
            // 
            this.checkBoxSimulation.AutoSize = true;
            this.checkBoxSimulation.Dock = System.Windows.Forms.DockStyle.Fill;
            this.checkBoxSimulation.Location = new System.Drawing.Point(2, 33);
            this.checkBoxSimulation.Margin = new System.Windows.Forms.Padding(2);
            this.checkBoxSimulation.Name = "checkBoxSimulation";
            this.checkBoxSimulation.Size = new System.Drawing.Size(175, 28);
            this.checkBoxSimulation.TabIndex = 19;
            this.checkBoxSimulation.Text = "Simulation";
            this.checkBoxSimulation.UseVisualStyleBackColor = true;
            this.checkBoxSimulation.Visible = false;
            this.checkBoxSimulation.CheckedChanged += new System.EventHandler(this.checkBoxSimulation_CheckedChanged);
            // 
            // tableLayoutPanel6
            // 
            this.tableLayoutPanel6.ColumnCount = 1;
            this.tableLayoutPanel6.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel6.Controls.Add(this.buttonRequstInput, 0, 0);
            this.tableLayoutPanel6.Location = new System.Drawing.Point(187, 2);
            this.tableLayoutPanel6.Margin = new System.Windows.Forms.Padding(2);
            this.tableLayoutPanel6.Name = "tableLayoutPanel6";
            this.tableLayoutPanel6.RowCount = 7;
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanel6.Size = new System.Drawing.Size(182, 272);
            this.tableLayoutPanel6.TabIndex = 21;
            // 
            // buttonRequstInput
            // 
            this.buttonRequstInput.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.buttonRequstInput.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.buttonRequstInput.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.buttonRequstInput.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.buttonRequstInput.CustomForeColor = System.Drawing.Color.Black;
            this.buttonRequstInput.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonRequstInput.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.buttonRequstInput.ForeColor = System.Drawing.Color.Black;
            this.buttonRequstInput.ImageSize = new System.Drawing.Size(45, 45);
            this.buttonRequstInput.Location = new System.Drawing.Point(2, 3);
            this.buttonRequstInput.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.buttonRequstInput.Name = "buttonRequstInput";
            this.buttonRequstInput.Size = new System.Drawing.Size(178, 32);
            this.buttonRequstInput.TabIndex = 18;
            this.buttonRequstInput.TabStop = false;
            this.buttonRequstInput.Text = "InputRequst";
            this.buttonRequstInput.UseVisualStyleBackColor = false;
            // 
            // groupBoxImageView
            // 
            this.groupBoxImageView.Controls.Add(this.tableLayoutPanel5);
            this.groupBoxImageView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxImageView.Font = new System.Drawing.Font("맑은 고딕", 10F);
            this.groupBoxImageView.Location = new System.Drawing.Point(507, 2);
            this.groupBoxImageView.Margin = new System.Windows.Forms.Padding(2);
            this.groupBoxImageView.Name = "groupBoxImageView";
            this.groupBoxImageView.Padding = new System.Windows.Forms.Padding(2);
            this.groupBoxImageView.Size = new System.Drawing.Size(375, 371);
            this.groupBoxImageView.TabIndex = 20;
            this.groupBoxImageView.TabStop = false;
            this.groupBoxImageView.Text = "ImageView";
            // 
            // tableLayoutPanel5
            // 
            this.tableLayoutPanel5.ColumnCount = 1;
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel5.Controls.Add(this._InputWaferCameraviewer, 0, 0);
            this.tableLayoutPanel5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel5.Location = new System.Drawing.Point(2, 20);
            this.tableLayoutPanel5.Name = "tableLayoutPanel5";
            this.tableLayoutPanel5.RowCount = 2;
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 80F));
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel5.Size = new System.Drawing.Size(371, 349);
            this.tableLayoutPanel5.TabIndex = 0;
            // 
            // _InputWaferCameraviewer
            // 
            this._InputWaferCameraviewer.BackColor = System.Drawing.Color.Black;
            this._InputWaferCameraviewer.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this._InputWaferCameraviewer.Camera = null;
            this._InputWaferCameraviewer.CameraSwitch = null;
            this._InputWaferCameraviewer.Dock = System.Windows.Forms.DockStyle.Fill;
            this._InputWaferCameraviewer.FrameRate = 1D;
            this._InputWaferCameraviewer.InputImage = null;
            this._InputWaferCameraviewer.IsViewCustomizedImage = false;
            this._InputWaferCameraviewer.Location = new System.Drawing.Point(2, 2);
            this._InputWaferCameraviewer.Margin = new System.Windows.Forms.Padding(2);
            this._InputWaferCameraviewer.Name = "_InputWaferCameraviewer";
            this._InputWaferCameraviewer.OperatingType = QMC.Common.Vision.VisionImageViewer.OperatingTypes.Center;
            this._InputWaferCameraviewer.Simulated = false;
            this._InputWaferCameraviewer.Size = new System.Drawing.Size(367, 275);
            this._InputWaferCameraviewer.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this._InputWaferCameraviewer.TabIndex = 12;
            this._InputWaferCameraviewer.TabStop = false;
            this._InputWaferCameraviewer.UpdateDelayTime = 80;
            this._InputWaferCameraviewer.VisibleCrossLine = true;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 40F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.tableLayoutPanel1.Controls.Add(this.teachingPositionControl, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.groupBoxImageView, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.groupBoxManual, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.dioControl, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel8, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel9, 2, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1264, 751);
            this.tableLayoutPanel1.TabIndex = 21;
            // 
            // tableLayoutPanel8
            // 
            this.tableLayoutPanel8.BackColor = System.Drawing.Color.White;
            this.tableLayoutPanel8.ColumnCount = 1;
            this.tableLayoutPanel8.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel8.Controls.Add(this.tabControlManualSeqInputWafer, 0, 0);
            this.tableLayoutPanel8.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel8.Location = new System.Drawing.Point(887, 3);
            this.tableLayoutPanel8.Name = "tableLayoutPanel8";
            this.tableLayoutPanel8.RowCount = 1;
            this.tableLayoutPanel8.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel8.Size = new System.Drawing.Size(374, 369);
            this.tableLayoutPanel8.TabIndex = 22;
            // 
            // tabControlManualSeqInputWafer
            // 
            this.tabControlManualSeqInputWafer.Controls.Add(this.tabPage1);
            this.tabControlManualSeqInputWafer.Controls.Add(this.tabPage2);
            this.tabControlManualSeqInputWafer.Controls.Add(this.tabPage3);
            this.tabControlManualSeqInputWafer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControlManualSeqInputWafer.Location = new System.Drawing.Point(2, 2);
            this.tabControlManualSeqInputWafer.Margin = new System.Windows.Forms.Padding(2);
            this.tabControlManualSeqInputWafer.Name = "tabControlManualSeqInputWafer";
            this.tabControlManualSeqInputWafer.SelectedIndex = 0;
            this.tabControlManualSeqInputWafer.Size = new System.Drawing.Size(370, 365);
            this.tabControlManualSeqInputWafer.TabIndex = 21;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.manualSequenceControlInputCassette);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Margin = new System.Windows.Forms.Padding(2);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(2);
            this.tabPage1.Size = new System.Drawing.Size(362, 339);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Cassette";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.manualSequenceControlInputFeeder);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Margin = new System.Windows.Forms.Padding(2);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(2);
            this.tabPage2.Size = new System.Drawing.Size(362, 339);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Feeder";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.manualSequenceControlInputWaferStage);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Margin = new System.Windows.Forms.Padding(2);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(2);
            this.tabPage3.Size = new System.Drawing.Size(362, 339);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "Stage";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel9
            // 
            this.tableLayoutPanel9.ColumnCount = 1;
            this.tableLayoutPanel9.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel9.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel9.Location = new System.Drawing.Point(887, 378);
            this.tableLayoutPanel9.Name = "tableLayoutPanel9";
            this.tableLayoutPanel9.RowCount = 2;
            this.tableLayoutPanel9.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel9.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel9.Size = new System.Drawing.Size(374, 370);
            this.tableLayoutPanel9.TabIndex = 23;
            // 
            // teachingPositionControl
            // 
            this.teachingPositionControl.ButtonSize = new System.Drawing.Size(90, 32);
            this.teachingPositionControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.teachingPositionControl.Location = new System.Drawing.Point(2, 4);
            this.teachingPositionControl.Margin = new System.Windows.Forms.Padding(2, 4, 2, 4);
            this.teachingPositionControl.Name = "teachingPositionControl";
            this.teachingPositionControl.ShowCancelButton = true;
            this.teachingPositionControl.ShowSaveButton = true;
            this.teachingPositionControl.Size = new System.Drawing.Size(501, 367);
            this.teachingPositionControl.TabIndex = 14;
            this.teachingPositionControl.UnitName = null;
            // 
            // waferMapView
            // 
            this.waferMapView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.waferMapView.Location = new System.Drawing.Point(2, 3);
            this.waferMapView.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.waferMapView.Name = "waferMapView";
            this.waferMapView.Size = new System.Drawing.Size(175, 232);
            this.waferMapView.TabIndex = 16;
            // 
            // dioControl
            // 
            this.dioControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dioControl.IoSortMode = QMC.LCP_280.Process.Component.DIOControl.SortingMode.AlphabeticalKey;
            this.dioControl.Location = new System.Drawing.Point(2, 379);
            this.dioControl.Margin = new System.Windows.Forms.Padding(2, 4, 2, 4);
            this.dioControl.Name = "dioControl";
            this.dioControl.RefreshIntervalMs = 400;
            this.dioControl.Size = new System.Drawing.Size(501, 368);
            this.dioControl.TabIndex = 15;
            // 
            // manualSequenceControlInputCassette
            // 
            this.manualSequenceControlInputCassette.Dock = System.Windows.Forms.DockStyle.Fill;
            this.manualSequenceControlInputCassette.Location = new System.Drawing.Point(2, 2);
            this.manualSequenceControlInputCassette.Margin = new System.Windows.Forms.Padding(2);
            this.manualSequenceControlInputCassette.MinimumSize = new System.Drawing.Size(238, 100);
            this.manualSequenceControlInputCassette.Name = "manualSequenceControlInputCassette";
            this.manualSequenceControlInputCassette.ParentUnit = null;
            this.manualSequenceControlInputCassette.Size = new System.Drawing.Size(358, 335);
            this.manualSequenceControlInputCassette.TabIndex = 14;
            // 
            // manualSequenceControlInputFeeder
            // 
            this.manualSequenceControlInputFeeder.Dock = System.Windows.Forms.DockStyle.Fill;
            this.manualSequenceControlInputFeeder.Location = new System.Drawing.Point(2, 2);
            this.manualSequenceControlInputFeeder.Margin = new System.Windows.Forms.Padding(2);
            this.manualSequenceControlInputFeeder.MinimumSize = new System.Drawing.Size(238, 200);
            this.manualSequenceControlInputFeeder.Name = "manualSequenceControlInputFeeder";
            this.manualSequenceControlInputFeeder.ParentUnit = null;
            this.manualSequenceControlInputFeeder.Size = new System.Drawing.Size(358, 335);
            this.manualSequenceControlInputFeeder.TabIndex = 15;
            // 
            // manualSequenceControlInputWaferStage
            // 
            this.manualSequenceControlInputWaferStage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.manualSequenceControlInputWaferStage.Location = new System.Drawing.Point(2, 2);
            this.manualSequenceControlInputWaferStage.Margin = new System.Windows.Forms.Padding(2);
            this.manualSequenceControlInputWaferStage.MinimumSize = new System.Drawing.Size(238, 200);
            this.manualSequenceControlInputWaferStage.Name = "manualSequenceControlInputWaferStage";
            this.manualSequenceControlInputWaferStage.ParentUnit = null;
            this.manualSequenceControlInputWaferStage.Size = new System.Drawing.Size(358, 335);
            this.manualSequenceControlInputWaferStage.TabIndex = 16;
            // 
            // InputWafer_Working
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(1264, 751);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.Name = "InputWafer_Working";
            this.Text = "InputWafer_Working";
            this.groupBoxManual.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel4.ResumeLayout(false);
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel3.PerformLayout();
            this.tableLayoutPanel6.ResumeLayout(false);
            this.groupBoxImageView.ResumeLayout(false);
            this.tableLayoutPanel5.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this._InputWaferCameraviewer)).EndInit();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel8.ResumeLayout(false);
            this.tabControlManualSeqInputWafer.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.tabPage3.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private Component.DIOControl dioControl;
        private Component.TeachingPositionControl teachingPositionControl;
        private WaferMapView waferMapView;
        private Common.IndividualMenuButton btnMapping;
        private System.Windows.Forms.GroupBox groupBoxManual;
        private System.Windows.Forms.GroupBox groupBoxImageView;
        private Common.Vision.VisionImageViewer _InputWaferCameraviewer;
        private System.Windows.Forms.CheckBox checkBoxTest;
        private System.Windows.Forms.CheckBox checkBoxSimulation;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel4;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel5;
        private System.Windows.Forms.TabPage tabPage1;
        private ManualSequenceControl manualSequenceControlInputCassette;
        private System.Windows.Forms.TabPage tabPage2;
        private ManualSequenceControl manualSequenceControlInputFeeder;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel6;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel8;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel9;
        private Common.IndividualMenuButton buttonRequstInput;
        private System.Windows.Forms.TabControl tabControlManualSeqInputWafer;
        private ManualSequenceControl manualSequenceControlInputWaferStage;
    }
}