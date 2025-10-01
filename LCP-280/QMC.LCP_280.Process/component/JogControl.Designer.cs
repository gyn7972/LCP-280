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
        private CustomBorderLabel lblPosition;
        private GroupBox grpSelectAxis;
        private ListBoxItemsView selectAxisListBoxItemsView;
        private GroupBox grpMove;
        private RadioButton rdoFine;
        private RadioButton rdoCoarse;

        private GroupBox grpMoveMode;
        private RadioButton rdoContinuous;

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
            this.grpSelectAxis = new System.Windows.Forms.GroupBox();
            this.selectAxisListBoxItemsView = new QMC.Common.ListBoxItemsView();
            this.lblPosition = new QMC.Common.CustomControl.CustomBorderLabel();
            this.lbCurrentPosition = new QMC.Common.CustomControl.CustomBorderLabel();
            this.grpMove = new System.Windows.Forms.GroupBox();
            this.rdoFine = new System.Windows.Forms.RadioButton();
            this.rdoCoarse = new System.Windows.Forms.RadioButton();
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
            this.grpMoveMode = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.btnStepClear = new QMC.Common.IndividualMenuButton();
            this.btnStep1000 = new QMC.Common.IndividualMenuButton();
            this.btnStep100 = new QMC.Common.IndividualMenuButton();
            this.btnStep10 = new QMC.Common.IndividualMenuButton();
            this.btnStep1 = new QMC.Common.IndividualMenuButton();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.rdoStep = new System.Windows.Forms.RadioButton();
            this.rdoContinuous = new System.Windows.Forms.RadioButton();
            this.tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
            this.nudStep = new System.Windows.Forms.NumericUpDown();
            this.tableLayoutPanel5 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel6 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel7 = new System.Windows.Forms.TableLayoutPanel();
            this.grpSelectAxis.SuspendLayout();
            this.grpMove.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.tlpCommandPosition.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudCommandPosition)).BeginInit();
            this.tblJog.SuspendLayout();
            this.grpMoveMode.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.tableLayoutPanel4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudStep)).BeginInit();
            this.tableLayoutPanel5.SuspendLayout();
            this.tableLayoutPanel6.SuspendLayout();
            this.tableLayoutPanel7.SuspendLayout();
            this.SuspendLayout();
            // 
            // grpSelectAxis
            // 
            this.grpSelectAxis.Controls.Add(this.selectAxisListBoxItemsView);
            this.grpSelectAxis.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpSelectAxis.Location = new System.Drawing.Point(0, 45);
            this.grpSelectAxis.Margin = new System.Windows.Forms.Padding(0);
            this.grpSelectAxis.Name = "grpSelectAxis";
            this.grpSelectAxis.Padding = new System.Windows.Forms.Padding(6);
            this.grpSelectAxis.Size = new System.Drawing.Size(304, 181);
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
            this.selectAxisListBoxItemsView.Size = new System.Drawing.Size(292, 155);
            this.selectAxisListBoxItemsView.TabIndex = 0;
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
            this.lblPosition.Location = new System.Drawing.Point(152, 3);
            this.lblPosition.Margin = new System.Windows.Forms.Padding(3);
            this.lblPosition.Name = "lblPosition";
            this.lblPosition.Size = new System.Drawing.Size(143, 33);
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
            this.lbCurrentPosition.Location = new System.Drawing.Point(3, 3);
            this.lbCurrentPosition.Margin = new System.Windows.Forms.Padding(3);
            this.lbCurrentPosition.Name = "lbCurrentPosition";
            this.lbCurrentPosition.Size = new System.Drawing.Size(143, 33);
            this.lbCurrentPosition.TabIndex = 1;
            this.lbCurrentPosition.Text = "Current Position (mm)";
            this.lbCurrentPosition.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // grpMove
            // 
            this.grpMove.Controls.Add(this.rdoFine);
            this.grpMove.Controls.Add(this.rdoCoarse);
            this.grpMove.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpMove.Location = new System.Drawing.Point(0, 232);
            this.grpMove.Margin = new System.Windows.Forms.Padding(0, 0, 0, 6);
            this.grpMove.Name = "grpMove";
            this.grpMove.Padding = new System.Windows.Forms.Padding(8);
            this.grpMove.Size = new System.Drawing.Size(310, 71);
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
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.tlpCommandPosition);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(3, 312);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(304, 71);
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
            this.tlpCommandPosition.Size = new System.Drawing.Size(298, 51);
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
            this.btnCommandPositionMove.Location = new System.Drawing.Point(152, 3);
            this.btnCommandPositionMove.Name = "btnCommandPositionMove";
            this.btnCommandPositionMove.Size = new System.Drawing.Size(143, 45);
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
            this.nudCommandPosition.Size = new System.Drawing.Size(143, 21);
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
            this.tblJog.Location = new System.Drawing.Point(0, 540);
            this.tblJog.Margin = new System.Windows.Forms.Padding(0);
            this.tblJog.Name = "tblJog";
            this.tblJog.Padding = new System.Windows.Forms.Padding(3);
            this.tblJog.RowCount = 4;
            this.tblJog.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tblJog.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tblJog.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tblJog.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tblJog.Size = new System.Drawing.Size(310, 234);
            this.tblJog.TabIndex = 2;
            // 
            // btnXPlus
            // 
            this.btnXPlus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnXPlus.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnXPlus.Location = new System.Drawing.Point(234, 63);
            this.btnXPlus.Name = "btnXPlus";
            this.btnXPlus.Size = new System.Drawing.Size(70, 51);
            this.btnXPlus.TabIndex = 6;
            this.btnXPlus.Text = "+X";
            // 
            // btnStop
            // 
            this.btnStop.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnStop.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnStop.Location = new System.Drawing.Point(158, 63);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(70, 51);
            this.btnStop.TabIndex = 5;
            this.btnStop.Text = "STOP";
            // 
            // btnXMinus
            // 
            this.btnXMinus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnXMinus.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnXMinus.Location = new System.Drawing.Point(82, 63);
            this.btnXMinus.Name = "btnXMinus";
            this.btnXMinus.Size = new System.Drawing.Size(70, 51);
            this.btnXMinus.TabIndex = 4;
            this.btnXMinus.Text = "-X";
            // 
            // btnZPlus
            // 
            this.btnZPlus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnZPlus.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnZPlus.Location = new System.Drawing.Point(6, 6);
            this.btnZPlus.Name = "btnZPlus";
            this.btnZPlus.Size = new System.Drawing.Size(70, 51);
            this.btnZPlus.TabIndex = 3;
            this.btnZPlus.Text = "+Z";
            // 
            // btnYPlus
            // 
            this.btnYPlus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnYPlus.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnYPlus.Location = new System.Drawing.Point(158, 6);
            this.btnYPlus.Name = "btnYPlus";
            this.btnYPlus.Size = new System.Drawing.Size(70, 51);
            this.btnYPlus.TabIndex = 1;
            this.btnYPlus.Text = "+Y";
            // 
            // btnZMinus
            // 
            this.btnZMinus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnZMinus.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnZMinus.Location = new System.Drawing.Point(6, 120);
            this.btnZMinus.Name = "btnZMinus";
            this.btnZMinus.Size = new System.Drawing.Size(70, 51);
            this.btnZMinus.TabIndex = 7;
            this.btnZMinus.Text = "-Z";
            // 
            // btnYMinus
            // 
            this.btnYMinus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnYMinus.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnYMinus.Location = new System.Drawing.Point(158, 120);
            this.btnYMinus.Name = "btnYMinus";
            this.btnYMinus.Size = new System.Drawing.Size(70, 51);
            this.btnYMinus.TabIndex = 8;
            this.btnYMinus.Text = "-Y";
            // 
            // btnTMinus
            // 
            this.btnTMinus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnTMinus.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnTMinus.Location = new System.Drawing.Point(234, 6);
            this.btnTMinus.Name = "btnTMinus";
            this.btnTMinus.Size = new System.Drawing.Size(70, 51);
            this.btnTMinus.TabIndex = 0;
            this.btnTMinus.Text = "CW";
            // 
            // btnTPlus
            // 
            this.btnTPlus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnTPlus.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnTPlus.Location = new System.Drawing.Point(82, 6);
            this.btnTPlus.Name = "btnTPlus";
            this.btnTPlus.Size = new System.Drawing.Size(70, 51);
            this.btnTPlus.TabIndex = 2;
            this.btnTPlus.Text = "CCW";
            // 
            // btnNextIndex
            // 
            this.btnNextIndex.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnNextIndex.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnNextIndex.Location = new System.Drawing.Point(234, 177);
            this.btnNextIndex.Name = "btnNextIndex";
            this.btnNextIndex.Size = new System.Drawing.Size(70, 51);
            this.btnNextIndex.TabIndex = 10;
            this.btnNextIndex.Text = "Next Index";
            // 
            // btnPrevIndex
            // 
            this.btnPrevIndex.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnPrevIndex.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnPrevIndex.Location = new System.Drawing.Point(82, 177);
            this.btnPrevIndex.Name = "btnPrevIndex";
            this.btnPrevIndex.Size = new System.Drawing.Size(70, 51);
            this.btnPrevIndex.TabIndex = 9;
            this.btnPrevIndex.Text = "Prev Index";
            // 
            // grpMoveMode
            // 
            this.grpMoveMode.Controls.Add(this.tableLayoutPanel1);
            this.grpMoveMode.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpMoveMode.Location = new System.Drawing.Point(0, 386);
            this.grpMoveMode.Margin = new System.Windows.Forms.Padding(0, 0, 0, 6);
            this.grpMoveMode.Name = "grpMoveMode";
            this.grpMoveMode.Size = new System.Drawing.Size(310, 148);
            this.grpMoveMode.TabIndex = 1;
            this.grpMoveMode.TabStop = false;
            this.grpMoveMode.Text = "Relative Position (mm)";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel3, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel4, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(3, 17);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 40F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(304, 128);
            this.tableLayoutPanel1.TabIndex = 5;
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 5;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel3.Controls.Add(this.btnStepClear, 4, 0);
            this.tableLayoutPanel3.Controls.Add(this.btnStep1000, 3, 0);
            this.tableLayoutPanel3.Controls.Add(this.btnStep100, 2, 0);
            this.tableLayoutPanel3.Controls.Add(this.btnStep10, 1, 0);
            this.tableLayoutPanel3.Controls.Add(this.btnStep1, 0, 0);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(3, 92);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 1;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(298, 33);
            this.tableLayoutPanel3.TabIndex = 3;
            // 
            // btnStepClear
            // 
            this.btnStepClear.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnStepClear.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnStepClear.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnStepClear.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnStepClear.CustomForeColor = System.Drawing.Color.Black;
            this.btnStepClear.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnStepClear.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Bold);
            this.btnStepClear.ForeColor = System.Drawing.Color.Black;
            this.btnStepClear.ImageSize = new System.Drawing.Size(45, 45);
            this.btnStepClear.Location = new System.Drawing.Point(239, 3);
            this.btnStepClear.Name = "btnStepClear";
            this.btnStepClear.Size = new System.Drawing.Size(56, 27);
            this.btnStepClear.TabIndex = 6;
            this.btnStepClear.TabStop = false;
            this.btnStepClear.Text = "0\'";
            this.btnStepClear.UseVisualStyleBackColor = false;
            // 
            // btnStep1000
            // 
            this.btnStep1000.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnStep1000.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnStep1000.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnStep1000.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnStep1000.CustomForeColor = System.Drawing.Color.Black;
            this.btnStep1000.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnStep1000.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Bold);
            this.btnStep1000.ForeColor = System.Drawing.Color.Black;
            this.btnStep1000.ImageSize = new System.Drawing.Size(45, 45);
            this.btnStep1000.Location = new System.Drawing.Point(180, 3);
            this.btnStep1000.Name = "btnStep1000";
            this.btnStep1000.Size = new System.Drawing.Size(53, 27);
            this.btnStep1000.TabIndex = 5;
            this.btnStep1000.TabStop = false;
            this.btnStep1000.Text = "0.001";
            this.btnStep1000.UseVisualStyleBackColor = false;
            // 
            // btnStep100
            // 
            this.btnStep100.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnStep100.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnStep100.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnStep100.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnStep100.CustomForeColor = System.Drawing.Color.Black;
            this.btnStep100.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnStep100.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Bold);
            this.btnStep100.ForeColor = System.Drawing.Color.Black;
            this.btnStep100.ImageSize = new System.Drawing.Size(45, 45);
            this.btnStep100.Location = new System.Drawing.Point(121, 3);
            this.btnStep100.Name = "btnStep100";
            this.btnStep100.Size = new System.Drawing.Size(53, 27);
            this.btnStep100.TabIndex = 4;
            this.btnStep100.TabStop = false;
            this.btnStep100.Text = "0.01";
            this.btnStep100.UseVisualStyleBackColor = false;
            // 
            // btnStep10
            // 
            this.btnStep10.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnStep10.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnStep10.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnStep10.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnStep10.CustomForeColor = System.Drawing.Color.Black;
            this.btnStep10.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnStep10.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Bold);
            this.btnStep10.ForeColor = System.Drawing.Color.Black;
            this.btnStep10.ImageSize = new System.Drawing.Size(45, 45);
            this.btnStep10.Location = new System.Drawing.Point(62, 3);
            this.btnStep10.Name = "btnStep10";
            this.btnStep10.Size = new System.Drawing.Size(53, 27);
            this.btnStep10.TabIndex = 3;
            this.btnStep10.TabStop = false;
            this.btnStep10.Text = "0.1";
            this.btnStep10.UseVisualStyleBackColor = false;
            // 
            // btnStep1
            // 
            this.btnStep1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnStep1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnStep1.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnStep1.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnStep1.CustomForeColor = System.Drawing.Color.Black;
            this.btnStep1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnStep1.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Bold);
            this.btnStep1.ForeColor = System.Drawing.Color.Black;
            this.btnStep1.ImageSize = new System.Drawing.Size(45, 45);
            this.btnStep1.Location = new System.Drawing.Point(3, 3);
            this.btnStep1.Name = "btnStep1";
            this.btnStep1.Size = new System.Drawing.Size(53, 27);
            this.btnStep1.TabIndex = 2;
            this.btnStep1.TabStop = false;
            this.btnStep1.Text = "1";
            this.btnStep1.UseVisualStyleBackColor = false;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 3;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel2.Controls.Add(this.rdoStep, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.rdoContinuous, 0, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(298, 45);
            this.tableLayoutPanel2.TabIndex = 2;
            // 
            // rdoStep
            // 
            this.rdoStep.AutoSize = true;
            this.rdoStep.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rdoStep.Location = new System.Drawing.Point(102, 3);
            this.rdoStep.Name = "rdoStep";
            this.rdoStep.Size = new System.Drawing.Size(93, 39);
            this.rdoStep.TabIndex = 0;
            this.rdoStep.TabStop = true;
            this.rdoStep.Text = "Step";
            this.rdoStep.CheckedChanged += new System.EventHandler(this.rdoStep_CheckedChanged);
            // 
            // rdoContinuous
            // 
            this.rdoContinuous.AutoSize = true;
            this.rdoContinuous.Checked = true;
            this.rdoContinuous.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rdoContinuous.Location = new System.Drawing.Point(3, 3);
            this.rdoContinuous.Name = "rdoContinuous";
            this.rdoContinuous.Size = new System.Drawing.Size(93, 39);
            this.rdoContinuous.TabIndex = 0;
            this.rdoContinuous.TabStop = true;
            this.rdoContinuous.Text = "Continuous";
            this.rdoContinuous.CheckedChanged += new System.EventHandler(this.rdoContinuous_CheckedChanged);
            // 
            // tableLayoutPanel4
            // 
            this.tableLayoutPanel4.ColumnCount = 3;
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel4.Controls.Add(this.nudStep, 0, 0);
            this.tableLayoutPanel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel4.Location = new System.Drawing.Point(3, 54);
            this.tableLayoutPanel4.Name = "tableLayoutPanel4";
            this.tableLayoutPanel4.RowCount = 1;
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel4.Size = new System.Drawing.Size(298, 32);
            this.tableLayoutPanel4.TabIndex = 1;
            // 
            // nudStep
            // 
            this.nudStep.DecimalPlaces = 3;
            this.nudStep.Dock = System.Windows.Forms.DockStyle.Fill;
            this.nudStep.Enabled = false;
            this.nudStep.Location = new System.Drawing.Point(0, 2);
            this.nudStep.Margin = new System.Windows.Forms.Padding(0, 2, 6, 2);
            this.nudStep.Maximum = new decimal(new int[] {
            500,
            0,
            0,
            0});
            this.nudStep.Name = "nudStep";
            this.nudStep.Size = new System.Drawing.Size(93, 21);
            this.nudStep.TabIndex = 1;
            this.nudStep.Value = new decimal(new int[] {
            1000,
            0,
            0,
            196608});
            // 
            // tableLayoutPanel5
            // 
            this.tableLayoutPanel5.ColumnCount = 1;
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel5.Controls.Add(this.tblJog, 0, 4);
            this.tableLayoutPanel5.Controls.Add(this.groupBox1, 0, 2);
            this.tableLayoutPanel5.Controls.Add(this.grpMoveMode, 0, 3);
            this.tableLayoutPanel5.Controls.Add(this.grpMove, 0, 1);
            this.tableLayoutPanel5.Controls.Add(this.tableLayoutPanel6, 0, 0);
            this.tableLayoutPanel5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel5.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel5.Name = "tableLayoutPanel5";
            this.tableLayoutPanel5.RowCount = 5;
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.tableLayoutPanel5.Size = new System.Drawing.Size(310, 774);
            this.tableLayoutPanel5.TabIndex = 1;
            // 
            // tableLayoutPanel6
            // 
            this.tableLayoutPanel6.ColumnCount = 1;
            this.tableLayoutPanel6.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel6.Controls.Add(this.grpSelectAxis, 0, 1);
            this.tableLayoutPanel6.Controls.Add(this.tableLayoutPanel7, 0, 0);
            this.tableLayoutPanel6.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel6.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel6.Name = "tableLayoutPanel6";
            this.tableLayoutPanel6.RowCount = 2;
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 80F));
            this.tableLayoutPanel6.Size = new System.Drawing.Size(304, 226);
            this.tableLayoutPanel6.TabIndex = 0;
            // 
            // tableLayoutPanel7
            // 
            this.tableLayoutPanel7.ColumnCount = 2;
            this.tableLayoutPanel7.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel7.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel7.Controls.Add(this.lblPosition, 1, 0);
            this.tableLayoutPanel7.Controls.Add(this.lbCurrentPosition, 0, 0);
            this.tableLayoutPanel7.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel7.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel7.Name = "tableLayoutPanel7";
            this.tableLayoutPanel7.RowCount = 1;
            this.tableLayoutPanel7.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel7.Size = new System.Drawing.Size(298, 39);
            this.tableLayoutPanel7.TabIndex = 0;
            // 
            // JogControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.Controls.Add(this.tableLayoutPanel5);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "JogControl";
            this.Size = new System.Drawing.Size(310, 774);
            this.grpSelectAxis.ResumeLayout(false);
            this.grpMove.ResumeLayout(false);
            this.grpMove.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.tlpCommandPosition.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.nudCommandPosition)).EndInit();
            this.tblJog.ResumeLayout(false);
            this.grpMoveMode.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.tableLayoutPanel4.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.nudStep)).EndInit();
            this.tableLayoutPanel5.ResumeLayout(false);
            this.tableLayoutPanel6.ResumeLayout(false);
            this.tableLayoutPanel7.ResumeLayout(false);
            this.tableLayoutPanel7.PerformLayout();
            this.ResumeLayout(false);

        }

        private Button btnPrevIndex;
        private Button btnNextIndex;
        private CustomBorderLabel lbCurrentPosition;
        private GroupBox groupBox1;
        private TableLayoutPanel tlpCommandPosition;
        private IndividualMenuButton btnCommandPositionMove;
        private NumericUpDown nudCommandPosition;
        private TableLayoutPanel tableLayoutPanel1;
        private TableLayoutPanel tableLayoutPanel3;
        private IndividualMenuButton btnStepClear;
        private IndividualMenuButton btnStep1000;
        private IndividualMenuButton btnStep100;
        private IndividualMenuButton btnStep10;
        private IndividualMenuButton btnStep1;
        private RadioButton rdoStep;
        private NumericUpDown nudStep;
        private TableLayoutPanel tableLayoutPanel2;
        private TableLayoutPanel tableLayoutPanel4;
        private TableLayoutPanel tableLayoutPanel5;
        private TableLayoutPanel tableLayoutPanel6;
        private TableLayoutPanel tableLayoutPanel7;
    }
}
