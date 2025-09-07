using System.Drawing;
using System.Windows.Forms;
using System;
using QMC.Common;               // ListBoxItemsView
using QMC.Common.CustomControl; // (있다면)
using QMC.Common.Motions;

namespace QMC.LCP_280.Process.Unit
{
    partial class JogControl
    {
        private System.ComponentModel.IContainer components = null;

        private TableLayoutPanel layoutRoot;
        private TableLayoutPanel layoutLeft;
        private CustomBorderLabel lblPosition;
        private GroupBox grpSelectAxis;
        private ListBoxItemsView selectAxisListBoxItemsView;

        private TableLayoutPanel layoutRight;
        private GroupBox grpMove;
        private RadioButton rdoFine;
        private RadioButton rdoCoarse;

        private GroupBox grpMoveMode;
        private RadioButton rdoContinuous;
        private RadioButton rdoStep;
        private NumericUpDown nudStep;

        private FlowLayoutPanel stepLine;     // Step 라인(라디오 + 숫자)
        private FlowLayoutPanel presetRow;   // 프리셋 버튼 줄
        private Button btnStep1;
        private Button btnStep10;
        private Button btnStep100;
        private Button btnStep1000;
        private Button btnStepClear;

        private TableLayoutPanel tblJog;
        private Button btnTMinus;
        private Button btnYPlus;
        private Button btnTPlus;
        private Button btnZPlus;
        private Button btnXMinus;
        private Button btnStop;
        private Button btnXPlus;
        private Button btnZMinus;
        private Button btnYMinus;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.layoutRoot = new System.Windows.Forms.TableLayoutPanel();
            this.layoutLeft = new System.Windows.Forms.TableLayoutPanel();
            this.grpSelectAxis = new System.Windows.Forms.GroupBox();
            this.selectAxisListBoxItemsView = new QMC.Common.ListBoxItemsView();
            this.tblCurrentPosition = new System.Windows.Forms.TableLayoutPanel();
            this.lblPosition = new QMC.Common.CustomControl.CustomBorderLabel();
            this.lbCurrentPosition = new QMC.Common.CustomControl.CustomBorderLabel();
            this.layoutRight = new System.Windows.Forms.TableLayoutPanel();
            this.grpMove = new System.Windows.Forms.GroupBox();
            this.rdoFine = new System.Windows.Forms.RadioButton();
            this.rdoCoarse = new System.Windows.Forms.RadioButton();
            this.grpMoveMode = new System.Windows.Forms.GroupBox();
            this.rdoContinuous = new System.Windows.Forms.RadioButton();
            this.stepLine = new System.Windows.Forms.FlowLayoutPanel();
            this.rdoStep = new System.Windows.Forms.RadioButton();
            this.nudStep = new System.Windows.Forms.NumericUpDown();
            this.presetRow = new System.Windows.Forms.FlowLayoutPanel();
            this.btnStep1 = new System.Windows.Forms.Button();
            this.btnStep10 = new System.Windows.Forms.Button();
            this.btnStep100 = new System.Windows.Forms.Button();
            this.btnStep1000 = new System.Windows.Forms.Button();
            this.btnStepClear = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.tlpCommandPosition = new System.Windows.Forms.TableLayoutPanel();
            this.btnCommandPositionMove = new QMC.Common.IndividualMenuButton();
            this.nudCommandPosition = new System.Windows.Forms.NumericUpDown();
            this.tblJog = new System.Windows.Forms.TableLayoutPanel();
            this.btnXPlus = new System.Windows.Forms.Button();
            this.btnStop = new System.Windows.Forms.Button();
            this.btnXMinus = new System.Windows.Forms.Button();
            this.btnZPlus = new System.Windows.Forms.Button();
            this.btnYPlus = new System.Windows.Forms.Button();
            this.btnZMinus = new System.Windows.Forms.Button();
            this.btnYMinus = new System.Windows.Forms.Button();
            this.btnTMinus = new System.Windows.Forms.Button();
            this.btnTPlus = new System.Windows.Forms.Button();
            this.btnNextIndex = new System.Windows.Forms.Button();
            this.btnPrevIndex = new System.Windows.Forms.Button();
            this.layoutRoot.SuspendLayout();
            this.layoutLeft.SuspendLayout();
            this.grpSelectAxis.SuspendLayout();
            this.tblCurrentPosition.SuspendLayout();
            this.layoutRight.SuspendLayout();
            this.grpMove.SuspendLayout();
            this.grpMoveMode.SuspendLayout();
            this.stepLine.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudStep)).BeginInit();
            this.presetRow.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.tlpCommandPosition.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudCommandPosition)).BeginInit();
            this.tblJog.SuspendLayout();
            this.SuspendLayout();
            // 
            // layoutRoot
            // 
            this.layoutRoot.ColumnCount = 1;
            this.layoutRoot.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.layoutRoot.Controls.Add(this.layoutLeft, 0, 0);
            this.layoutRoot.Controls.Add(this.layoutRight, 0, 1);
            this.layoutRoot.Dock = System.Windows.Forms.DockStyle.Fill;
            this.layoutRoot.Location = new System.Drawing.Point(0, 0);
            this.layoutRoot.Margin = new System.Windows.Forms.Padding(0);
            this.layoutRoot.Name = "layoutRoot";
            this.layoutRoot.Padding = new System.Windows.Forms.Padding(4);
            this.layoutRoot.RowCount = 2;
            this.layoutRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 28.46821F));
            this.layoutRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 71.53179F));
            this.layoutRoot.Size = new System.Drawing.Size(1225, 755);
            this.layoutRoot.TabIndex = 0;
            // 
            // layoutLeft
            // 
            this.layoutLeft.ColumnCount = 1;
            this.layoutLeft.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.layoutLeft.Controls.Add(this.grpSelectAxis, 0, 1);
            this.layoutLeft.Controls.Add(this.tblCurrentPosition, 0, 0);
            this.layoutLeft.Location = new System.Drawing.Point(4, 4);
            this.layoutLeft.Margin = new System.Windows.Forms.Padding(0);
            this.layoutLeft.Name = "layoutLeft";
            this.layoutLeft.Padding = new System.Windows.Forms.Padding(4, 2, 4, 0);
            this.layoutLeft.RowCount = 2;
            this.layoutLeft.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            this.layoutLeft.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.layoutLeft.Size = new System.Drawing.Size(1217, 211);
            this.layoutLeft.TabIndex = 0;
            // 
            // grpSelectAxis
            // 
            this.grpSelectAxis.Controls.Add(this.selectAxisListBoxItemsView);
            this.grpSelectAxis.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpSelectAxis.Location = new System.Drawing.Point(4, 34);
            this.grpSelectAxis.Margin = new System.Windows.Forms.Padding(0);
            this.grpSelectAxis.Name = "grpSelectAxis";
            this.grpSelectAxis.Padding = new System.Windows.Forms.Padding(6);
            this.grpSelectAxis.Size = new System.Drawing.Size(1209, 177);
            this.grpSelectAxis.TabIndex = 1;
            this.grpSelectAxis.TabStop = false;
            this.grpSelectAxis.Text = "Select Axis";
            // 
            // selectAxisListBoxItemsView
            // 
            this.selectAxisListBoxItemsView.BorderColor = System.Drawing.Color.White;
            this.selectAxisListBoxItemsView.BorderWidth = 2;
            this.selectAxisListBoxItemsView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.selectAxisListBoxItemsView.GroupBackColor = System.Drawing.Color.White;
            this.selectAxisListBoxItemsView.GroupForeColor = System.Drawing.Color.Black;
            this.selectAxisListBoxItemsView.GroupName = "";
            this.selectAxisListBoxItemsView.ItemBackColor = System.Drawing.Color.Black;
            this.selectAxisListBoxItemsView.ItemForeColor = System.Drawing.Color.Lime;
            this.selectAxisListBoxItemsView.ListBackColor = System.Drawing.Color.Black;
            this.selectAxisListBoxItemsView.ListForeColor = System.Drawing.Color.Lime;
            this.selectAxisListBoxItemsView.Location = new System.Drawing.Point(6, 20);
            this.selectAxisListBoxItemsView.Name = "selectAxisListBoxItemsView";
            this.selectAxisListBoxItemsView.SelectedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(198)))), ((int)(((byte)(255)))), ((int)(((byte)(0)))));
            this.selectAxisListBoxItemsView.SelectedForeColor = System.Drawing.Color.Black;
            this.selectAxisListBoxItemsView.SelectedIndex = -1;
            this.selectAxisListBoxItemsView.Size = new System.Drawing.Size(1197, 151);
            this.selectAxisListBoxItemsView.TabIndex = 0;
            // 
            // tblCurrentPosition
            // 
            this.tblCurrentPosition.ColumnCount = 2;
            this.tblCurrentPosition.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tblCurrentPosition.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tblCurrentPosition.Controls.Add(this.lblPosition, 1, 0);
            this.tblCurrentPosition.Controls.Add(this.lbCurrentPosition, 0, 0);
            this.tblCurrentPosition.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tblCurrentPosition.Location = new System.Drawing.Point(7, 5);
            this.tblCurrentPosition.Name = "tblCurrentPosition";
            this.tblCurrentPosition.RowCount = 1;
            this.tblCurrentPosition.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tblCurrentPosition.Size = new System.Drawing.Size(1203, 26);
            this.tblCurrentPosition.TabIndex = 2;
            // 
            // lblPosition
            // 
            this.lblPosition.BackColor = System.Drawing.Color.Black;
            this.lblPosition.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(208)))), ((int)(((byte)(206)))), ((int)(((byte)(206)))));
            this.lblPosition.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblPosition.BorderWidth = 1;
            this.lblPosition.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblPosition.Font = new System.Drawing.Font("Consolas", 12F, System.Drawing.FontStyle.Bold);
            this.lblPosition.ForeColor = System.Drawing.Color.Lime;
            this.lblPosition.Location = new System.Drawing.Point(601, 0);
            this.lblPosition.Margin = new System.Windows.Forms.Padding(0, 0, 0, 4);
            this.lblPosition.Name = "lblPosition";
            this.lblPosition.Size = new System.Drawing.Size(602, 22);
            this.lblPosition.TabIndex = 0;
            this.lblPosition.Text = "000.000";
            this.lblPosition.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbCurrentPosition
            // 
            this.lbCurrentPosition.AutoSize = true;
            this.lbCurrentPosition.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(208)))), ((int)(((byte)(206)))), ((int)(((byte)(206)))));
            this.lbCurrentPosition.BorderWidth = 1;
            this.lbCurrentPosition.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbCurrentPosition.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Bold);
            this.lbCurrentPosition.Location = new System.Drawing.Point(3, 0);
            this.lbCurrentPosition.Name = "lbCurrentPosition";
            this.lbCurrentPosition.Size = new System.Drawing.Size(595, 26);
            this.lbCurrentPosition.TabIndex = 1;
            this.lbCurrentPosition.Text = "Current Position (mm)";
            this.lbCurrentPosition.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // layoutRight
            // 
            this.layoutRight.ColumnCount = 1;
            this.layoutRight.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.layoutRight.Controls.Add(this.grpMove, 0, 0);
            this.layoutRight.Controls.Add(this.grpMoveMode, 0, 2);
            this.layoutRight.Controls.Add(this.groupBox1, 0, 1);
            this.layoutRight.Controls.Add(this.tblJog, 0, 3);
            this.layoutRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.layoutRight.Location = new System.Drawing.Point(4, 220);
            this.layoutRight.Margin = new System.Windows.Forms.Padding(0, 4, 0, 0);
            this.layoutRight.Name = "layoutRight";
            this.layoutRight.Padding = new System.Windows.Forms.Padding(4, 4, 4, 0);
            this.layoutRight.RowCount = 4;
            this.layoutRight.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 11.8068F));
            this.layoutRight.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 11.44902F));
            this.layoutRight.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25.23719F));
            this.layoutRight.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 51.6129F));
            this.layoutRight.Size = new System.Drawing.Size(1217, 531);
            this.layoutRight.TabIndex = 1;
            // 
            // grpMove
            // 
            this.grpMove.Controls.Add(this.rdoFine);
            this.grpMove.Controls.Add(this.rdoCoarse);
            this.grpMove.Location = new System.Drawing.Point(4, 4);
            this.grpMove.Margin = new System.Windows.Forms.Padding(0, 0, 0, 6);
            this.grpMove.Name = "grpMove";
            this.grpMove.Padding = new System.Windows.Forms.Padding(8);
            this.grpMove.Size = new System.Drawing.Size(1209, 56);
            this.grpMove.TabIndex = 0;
            this.grpMove.TabStop = false;
            this.grpMove.Text = "Move";
            // 
            // rdoFine
            // 
            this.rdoFine.AutoSize = true;
            this.rdoFine.Location = new System.Drawing.Point(10, 24);
            this.rdoFine.Name = "rdoFine";
            this.rdoFine.Size = new System.Drawing.Size(47, 16);
            this.rdoFine.TabIndex = 0;
            this.rdoFine.Text = "Fine";
            // 
            // rdoCoarse
            // 
            this.rdoCoarse.AutoSize = true;
            this.rdoCoarse.Checked = true;
            this.rdoCoarse.Location = new System.Drawing.Point(62, 24);
            this.rdoCoarse.Name = "rdoCoarse";
            this.rdoCoarse.Size = new System.Drawing.Size(64, 16);
            this.rdoCoarse.TabIndex = 1;
            this.rdoCoarse.TabStop = true;
            this.rdoCoarse.Text = "Coarse";
            // 
            // grpMoveMode
            // 
            this.grpMoveMode.Controls.Add(this.rdoContinuous);
            this.grpMoveMode.Controls.Add(this.stepLine);
            this.grpMoveMode.Controls.Add(this.presetRow);
            this.grpMoveMode.Location = new System.Drawing.Point(4, 126);
            this.grpMoveMode.Margin = new System.Windows.Forms.Padding(0, 0, 0, 6);
            this.grpMoveMode.Name = "grpMoveMode";
            this.grpMoveMode.Padding = new System.Windows.Forms.Padding(8);
            this.grpMoveMode.Size = new System.Drawing.Size(1209, 117);
            this.grpMoveMode.TabIndex = 1;
            this.grpMoveMode.TabStop = false;
            this.grpMoveMode.Text = "Relative Position (mm)";
            // 
            // rdoContinuous
            // 
            this.rdoContinuous.AutoSize = true;
            this.rdoContinuous.Checked = true;
            this.rdoContinuous.Location = new System.Drawing.Point(10, 18);
            this.rdoContinuous.Name = "rdoContinuous";
            this.rdoContinuous.Size = new System.Drawing.Size(87, 16);
            this.rdoContinuous.TabIndex = 0;
            this.rdoContinuous.TabStop = true;
            this.rdoContinuous.Text = "Continuous";
            this.rdoContinuous.CheckedChanged += new System.EventHandler(this.rdoContinuous_CheckedChanged);
            // 
            // stepLine
            // 
            this.stepLine.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.stepLine.Controls.Add(this.rdoStep);
            this.stepLine.Controls.Add(this.nudStep);
            this.stepLine.Location = new System.Drawing.Point(8, 46);
            this.stepLine.Margin = new System.Windows.Forms.Padding(0);
            this.stepLine.Name = "stepLine";
            this.stepLine.Size = new System.Drawing.Size(1195, 28);
            this.stepLine.TabIndex = 1;
            this.stepLine.WrapContents = false;
            // 
            // rdoStep
            // 
            this.rdoStep.AutoSize = true;
            this.rdoStep.Location = new System.Drawing.Point(3, 3);
            this.rdoStep.Name = "rdoStep";
            this.rdoStep.Size = new System.Drawing.Size(48, 16);
            this.rdoStep.TabIndex = 0;
            this.rdoStep.Text = "Step";
            this.rdoStep.CheckedChanged += new System.EventHandler(this.rdoStep_CheckedChanged);
            // 
            // nudStep
            // 
            this.nudStep.DecimalPlaces = 3;
            this.nudStep.Enabled = false;
            this.nudStep.Location = new System.Drawing.Point(54, 2);
            this.nudStep.Margin = new System.Windows.Forms.Padding(0, 2, 6, 2);
            this.nudStep.Maximum = new decimal(new int[] {
            500,
            0,
            0,
            0});
            this.nudStep.Name = "nudStep";
            this.nudStep.Size = new System.Drawing.Size(62, 21);
            this.nudStep.TabIndex = 1;
            this.nudStep.Value = new decimal(new int[] {
            1000,
            0,
            0,
            196608});
            // 
            // presetRow
            // 
            this.presetRow.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.presetRow.Controls.Add(this.btnStep1);
            this.presetRow.Controls.Add(this.btnStep10);
            this.presetRow.Controls.Add(this.btnStep100);
            this.presetRow.Controls.Add(this.btnStep1000);
            this.presetRow.Controls.Add(this.btnStepClear);
            this.presetRow.Location = new System.Drawing.Point(8, 78);
            this.presetRow.Margin = new System.Windows.Forms.Padding(0);
            this.presetRow.Name = "presetRow";
            this.presetRow.Size = new System.Drawing.Size(1195, 28);
            this.presetRow.TabIndex = 2;
            this.presetRow.WrapContents = false;
            // 
            // btnStep1
            // 
            this.btnStep1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnStep1.Location = new System.Drawing.Point(3, 1);
            this.btnStep1.Margin = new System.Windows.Forms.Padding(3, 1, 3, 1);
            this.btnStep1.Name = "btnStep1";
            this.btnStep1.Size = new System.Drawing.Size(44, 24);
            this.btnStep1.TabIndex = 0;
            this.btnStep1.Text = "1";
            this.btnStep1.Click += new System.EventHandler(this.btnStep1_Click);
            // 
            // btnStep10
            // 
            this.btnStep10.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnStep10.Location = new System.Drawing.Point(53, 1);
            this.btnStep10.Margin = new System.Windows.Forms.Padding(3, 1, 3, 1);
            this.btnStep10.Name = "btnStep10";
            this.btnStep10.Size = new System.Drawing.Size(44, 24);
            this.btnStep10.TabIndex = 1;
            this.btnStep10.Text = "0.1";
            this.btnStep10.Click += new System.EventHandler(this.btnStep10_Click);
            // 
            // btnStep100
            // 
            this.btnStep100.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnStep100.Location = new System.Drawing.Point(103, 1);
            this.btnStep100.Margin = new System.Windows.Forms.Padding(3, 1, 3, 1);
            this.btnStep100.Name = "btnStep100";
            this.btnStep100.Size = new System.Drawing.Size(44, 24);
            this.btnStep100.TabIndex = 2;
            this.btnStep100.Text = "0.01";
            this.btnStep100.Click += new System.EventHandler(this.btnStep100_Click);
            // 
            // btnStep1000
            // 
            this.btnStep1000.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnStep1000.Location = new System.Drawing.Point(153, 1);
            this.btnStep1000.Margin = new System.Windows.Forms.Padding(3, 1, 3, 1);
            this.btnStep1000.Name = "btnStep1000";
            this.btnStep1000.Size = new System.Drawing.Size(50, 24);
            this.btnStep1000.TabIndex = 3;
            this.btnStep1000.Text = "0.001";
            this.btnStep1000.Click += new System.EventHandler(this.btnStep1000_Click);
            // 
            // btnStepClear
            // 
            this.btnStepClear.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnStepClear.Location = new System.Drawing.Point(209, 1);
            this.btnStepClear.Margin = new System.Windows.Forms.Padding(3, 1, 3, 1);
            this.btnStepClear.Name = "btnStepClear";
            this.btnStepClear.Size = new System.Drawing.Size(44, 24);
            this.btnStepClear.TabIndex = 4;
            this.btnStepClear.Text = "0\'";
            this.btnStepClear.Click += new System.EventHandler(this.btnStepClear_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.tlpCommandPosition);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(7, 69);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(1203, 54);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Command Position (mm)";
            // 
            // tlpCommandPosition
            // 
            this.tlpCommandPosition.ColumnCount = 2;
            this.tlpCommandPosition.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpCommandPosition.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpCommandPosition.Controls.Add(this.btnCommandPositionMove, 1, 0);
            this.tlpCommandPosition.Controls.Add(this.nudCommandPosition, 0, 0);
            this.tlpCommandPosition.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpCommandPosition.Location = new System.Drawing.Point(3, 17);
            this.tlpCommandPosition.Name = "tlpCommandPosition";
            this.tlpCommandPosition.RowCount = 1;
            this.tlpCommandPosition.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpCommandPosition.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpCommandPosition.Size = new System.Drawing.Size(1197, 34);
            this.tlpCommandPosition.TabIndex = 0;
            // 
            // btnCommandPositionMove
            // 
            this.btnCommandPositionMove.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnCommandPositionMove.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnCommandPositionMove.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnCommandPositionMove.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnCommandPositionMove.CustomForeColor = System.Drawing.Color.Black;
            this.btnCommandPositionMove.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnCommandPositionMove.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnCommandPositionMove.ForeColor = System.Drawing.Color.Black;
            this.btnCommandPositionMove.ImageSize = new System.Drawing.Size(45, 45);
            this.btnCommandPositionMove.Location = new System.Drawing.Point(601, 3);
            this.btnCommandPositionMove.Name = "btnCommandPositionMove";
            this.btnCommandPositionMove.Size = new System.Drawing.Size(593, 28);
            this.btnCommandPositionMove.TabIndex = 1;
            this.btnCommandPositionMove.TabStop = false;
            this.btnCommandPositionMove.Text = "Move";
            this.btnCommandPositionMove.UseVisualStyleBackColor = false;
            // 
            // nudCommandPosition
            // 
            this.nudCommandPosition.DecimalPlaces = 3;
            this.nudCommandPosition.Dock = System.Windows.Forms.DockStyle.Fill;
            this.nudCommandPosition.Location = new System.Drawing.Point(0, 2);
            this.nudCommandPosition.Margin = new System.Windows.Forms.Padding(0, 2, 6, 2);
            this.nudCommandPosition.Maximum = new decimal(new int[] {
            500,
            0,
            0,
            0});
            this.nudCommandPosition.Name = "nudCommandPosition";
            this.nudCommandPosition.Size = new System.Drawing.Size(592, 21);
            this.nudCommandPosition.TabIndex = 2;
            // 
            // tblJog
            // 
            this.tblJog.ColumnCount = 4;
            this.tblJog.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tblJog.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tblJog.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tblJog.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tblJog.Controls.Add(this.btnXPlus, 3, 1);
            this.tblJog.Controls.Add(this.btnStop, 2, 1);
            this.tblJog.Controls.Add(this.btnXMinus, 1, 1);
            this.tblJog.Controls.Add(this.btnZPlus, 0, 0);
            this.tblJog.Controls.Add(this.btnYPlus, 2, 0);
            this.tblJog.Controls.Add(this.btnZMinus, 0, 2);
            this.tblJog.Controls.Add(this.btnYMinus, 2, 2);
            this.tblJog.Controls.Add(this.btnTMinus, 3, 0);
            this.tblJog.Controls.Add(this.btnTPlus, 1, 0);
            this.tblJog.Controls.Add(this.btnNextIndex, 3, 3);
            this.tblJog.Controls.Add(this.btnPrevIndex, 1, 3);
            this.tblJog.Location = new System.Drawing.Point(4, 258);
            this.tblJog.Margin = new System.Windows.Forms.Padding(0);
            this.tblJog.Name = "tblJog";
            this.tblJog.Padding = new System.Windows.Forms.Padding(3);
            this.tblJog.RowCount = 4;
            this.tblJog.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tblJog.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tblJog.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tblJog.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tblJog.Size = new System.Drawing.Size(1209, 273);
            this.tblJog.TabIndex = 2;
            // 
            // btnXPlus
            // 
            this.btnXPlus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnXPlus.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnXPlus.Location = new System.Drawing.Point(906, 72);
            this.btnXPlus.Name = "btnXPlus";
            this.btnXPlus.Size = new System.Drawing.Size(297, 60);
            this.btnXPlus.TabIndex = 6;
            this.btnXPlus.Text = "+X";
            // 
            // btnStop
            // 
            this.btnStop.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnStop.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnStop.Location = new System.Drawing.Point(606, 72);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(294, 60);
            this.btnStop.TabIndex = 5;
            this.btnStop.Text = "STOP";
            // 
            // btnXMinus
            // 
            this.btnXMinus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnXMinus.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnXMinus.Location = new System.Drawing.Point(306, 72);
            this.btnXMinus.Name = "btnXMinus";
            this.btnXMinus.Size = new System.Drawing.Size(294, 60);
            this.btnXMinus.TabIndex = 4;
            this.btnXMinus.Text = "-X";
            // 
            // btnZPlus
            // 
            this.btnZPlus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnZPlus.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnZPlus.Location = new System.Drawing.Point(6, 6);
            this.btnZPlus.Name = "btnZPlus";
            this.btnZPlus.Size = new System.Drawing.Size(294, 60);
            this.btnZPlus.TabIndex = 3;
            this.btnZPlus.Text = "+Z";
            // 
            // btnYPlus
            // 
            this.btnYPlus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnYPlus.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnYPlus.Location = new System.Drawing.Point(606, 6);
            this.btnYPlus.Name = "btnYPlus";
            this.btnYPlus.Size = new System.Drawing.Size(294, 60);
            this.btnYPlus.TabIndex = 1;
            this.btnYPlus.Text = "+Y";
            // 
            // btnZMinus
            // 
            this.btnZMinus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnZMinus.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnZMinus.Location = new System.Drawing.Point(6, 138);
            this.btnZMinus.Name = "btnZMinus";
            this.btnZMinus.Size = new System.Drawing.Size(294, 60);
            this.btnZMinus.TabIndex = 7;
            this.btnZMinus.Text = "-Z";
            // 
            // btnYMinus
            // 
            this.btnYMinus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnYMinus.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnYMinus.Location = new System.Drawing.Point(606, 138);
            this.btnYMinus.Name = "btnYMinus";
            this.btnYMinus.Size = new System.Drawing.Size(294, 60);
            this.btnYMinus.TabIndex = 8;
            this.btnYMinus.Text = "-Y";
            // 
            // btnTMinus
            // 
            this.btnTMinus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnTMinus.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnTMinus.Location = new System.Drawing.Point(906, 6);
            this.btnTMinus.Name = "btnTMinus";
            this.btnTMinus.Size = new System.Drawing.Size(297, 60);
            this.btnTMinus.TabIndex = 0;
            this.btnTMinus.Text = "CW";
            // 
            // btnTPlus
            // 
            this.btnTPlus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnTPlus.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnTPlus.Location = new System.Drawing.Point(306, 6);
            this.btnTPlus.Name = "btnTPlus";
            this.btnTPlus.Size = new System.Drawing.Size(294, 60);
            this.btnTPlus.TabIndex = 2;
            this.btnTPlus.Text = "CCW";
            // 
            // btnNextIndex
            // 
            this.btnNextIndex.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnNextIndex.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnNextIndex.Location = new System.Drawing.Point(906, 204);
            this.btnNextIndex.Name = "btnNextIndex";
            this.btnNextIndex.Size = new System.Drawing.Size(297, 63);
            this.btnNextIndex.TabIndex = 10;
            this.btnNextIndex.Text = "Next Index";
            // 
            // btnPrevIndex
            // 
            this.btnPrevIndex.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnPrevIndex.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnPrevIndex.Location = new System.Drawing.Point(306, 204);
            this.btnPrevIndex.Name = "btnPrevIndex";
            this.btnPrevIndex.Size = new System.Drawing.Size(294, 63);
            this.btnPrevIndex.TabIndex = 9;
            this.btnPrevIndex.Text = "Prev Index";
            // 
            // JogControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.Controls.Add(this.layoutRoot);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "JogControl";
            this.Size = new System.Drawing.Size(1225, 755);
            this.layoutRoot.ResumeLayout(false);
            this.layoutLeft.ResumeLayout(false);
            this.grpSelectAxis.ResumeLayout(false);
            this.tblCurrentPosition.ResumeLayout(false);
            this.tblCurrentPosition.PerformLayout();
            this.layoutRight.ResumeLayout(false);
            this.grpMove.ResumeLayout(false);
            this.grpMove.PerformLayout();
            this.grpMoveMode.ResumeLayout(false);
            this.grpMoveMode.PerformLayout();
            this.stepLine.ResumeLayout(false);
            this.stepLine.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudStep)).EndInit();
            this.presetRow.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.tlpCommandPosition.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.nudCommandPosition)).EndInit();
            this.tblJog.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        private Button btnPrevIndex;
        private Button btnNextIndex;
        private TableLayoutPanel tblCurrentPosition;
        private CustomBorderLabel lbCurrentPosition;
        private GroupBox groupBox1;
        private TableLayoutPanel tlpCommandPosition;
        private IndividualMenuButton btnCommandPositionMove;
        private NumericUpDown nudCommandPosition;
    }
}
