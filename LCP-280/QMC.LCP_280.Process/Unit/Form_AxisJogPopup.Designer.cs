using System.Drawing;
using System.Windows.Forms;
using System;
using QMC.Common;               // ListBoxItemsView
using QMC.Common.CustomControl; // (있다면)
using QMC.Common.Motions;

namespace QMC.LCP_280.Process.Unit
{
    partial class Form_AxisJogPopup
    {
        private System.ComponentModel.IContainer components = null;

        private TableLayoutPanel layoutRoot;
        private TableLayoutPanel layoutLeft;
        private Label lblPosition;
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
        private Button btnStepPreset;

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
            this.lblPosition = new System.Windows.Forms.Label();
            this.grpSelectAxis = new System.Windows.Forms.GroupBox();
            this.selectAxisListBoxItemsView = new QMC.Common.ListBoxItemsView();
            this.layoutRight = new System.Windows.Forms.TableLayoutPanel();
            this.grpMove = new System.Windows.Forms.GroupBox();
            this.rdoFine = new System.Windows.Forms.RadioButton();
            this.rdoCoarse = new System.Windows.Forms.RadioButton();
            this.grpMoveMode = new System.Windows.Forms.GroupBox();
            this.rdoContinuous = new System.Windows.Forms.RadioButton();
            this.rdoStep = new System.Windows.Forms.RadioButton();
            this.nudStep = new System.Windows.Forms.NumericUpDown();
            this.btnStepPreset = new System.Windows.Forms.Button();
            this.tblJog = new System.Windows.Forms.TableLayoutPanel();
            this.btnTMinus = new System.Windows.Forms.Button();
            this.btnYPlus = new System.Windows.Forms.Button();
            this.btnTPlus = new System.Windows.Forms.Button();
            this.btnZPlus = new System.Windows.Forms.Button();
            this.btnXMinus = new System.Windows.Forms.Button();
            this.btnStop = new System.Windows.Forms.Button();
            this.btnXPlus = new System.Windows.Forms.Button();
            this.btnZMinus = new System.Windows.Forms.Button();
            this.btnYMinus = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.layoutRoot.SuspendLayout();
            this.layoutLeft.SuspendLayout();
            this.grpSelectAxis.SuspendLayout();
            this.layoutRight.SuspendLayout();
            this.grpMove.SuspendLayout();
            this.grpMoveMode.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudStep)).BeginInit();
            this.tblJog.SuspendLayout();
            this.SuspendLayout();
            // 
            // layoutRoot
            // 
            this.layoutRoot.ColumnCount = 2;
            this.layoutRoot.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 200F));
            this.layoutRoot.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.layoutRoot.Controls.Add(this.layoutLeft, 0, 0);
            this.layoutRoot.Controls.Add(this.layoutRight, 1, 0);
            this.layoutRoot.Dock = System.Windows.Forms.DockStyle.Fill;
            this.layoutRoot.Location = new System.Drawing.Point(0, 0);
            this.layoutRoot.Margin = new System.Windows.Forms.Padding(0);
            this.layoutRoot.Name = "layoutRoot";
            this.layoutRoot.RowCount = 1;
            this.layoutRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.layoutRoot.Size = new System.Drawing.Size(544, 321);
            this.layoutRoot.TabIndex = 0;
            // 
            // layoutLeft
            // 
            this.layoutLeft.ColumnCount = 1;
            this.layoutLeft.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.layoutLeft.Controls.Add(this.lblPosition, 0, 0);
            this.layoutLeft.Controls.Add(this.grpSelectAxis, 0, 1);
            this.layoutLeft.Dock = System.Windows.Forms.DockStyle.Fill;
            this.layoutLeft.Location = new System.Drawing.Point(0, 0);
            this.layoutLeft.Margin = new System.Windows.Forms.Padding(0);
            this.layoutLeft.Name = "layoutLeft";
            this.layoutLeft.Padding = new System.Windows.Forms.Padding(6);
            this.layoutLeft.RowCount = 2;
            this.layoutLeft.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 72F));
            this.layoutLeft.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.layoutLeft.Size = new System.Drawing.Size(200, 321);
            this.layoutLeft.TabIndex = 0;
            // 
            // lblPosition
            // 
            this.lblPosition.BackColor = System.Drawing.Color.Black;
            this.lblPosition.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblPosition.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblPosition.Font = new System.Drawing.Font("Consolas", 20F, System.Drawing.FontStyle.Bold);
            this.lblPosition.ForeColor = System.Drawing.Color.Lime;
            this.lblPosition.Location = new System.Drawing.Point(6, 6);
            this.lblPosition.Margin = new System.Windows.Forms.Padding(0, 0, 0, 6);
            this.lblPosition.Name = "lblPosition";
            this.lblPosition.Size = new System.Drawing.Size(188, 66);
            this.lblPosition.TabIndex = 0;
            this.lblPosition.Text = "000.000";
            this.lblPosition.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // grpSelectAxis
            // 
            this.grpSelectAxis.Controls.Add(this.selectAxisListBoxItemsView);
            this.grpSelectAxis.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpSelectAxis.Location = new System.Drawing.Point(6, 78);
            this.grpSelectAxis.Margin = new System.Windows.Forms.Padding(0);
            this.grpSelectAxis.Name = "grpSelectAxis";
            this.grpSelectAxis.Padding = new System.Windows.Forms.Padding(6);
            this.grpSelectAxis.Size = new System.Drawing.Size(188, 237);
            this.grpSelectAxis.TabIndex = 1;
            this.grpSelectAxis.TabStop = false;
            this.grpSelectAxis.Text = "Select Axis";
            // 
            // selectAxisListBoxItemsView
            // 
            this.selectAxisListBoxItemsView.BorderWidth = 2;
            this.selectAxisListBoxItemsView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.selectAxisListBoxItemsView.GroupName = "Select Axis";
            this.selectAxisListBoxItemsView.Location = new System.Drawing.Point(6, 20);
            this.selectAxisListBoxItemsView.Name = "selectAxisListBoxItemsView";
            this.selectAxisListBoxItemsView.SelectedIndex = -1;
            this.selectAxisListBoxItemsView.Size = new System.Drawing.Size(176, 211);
            this.selectAxisListBoxItemsView.TabIndex = 0;
            // 
            // layoutRight
            // 
            this.layoutRight.ColumnCount = 1;
            this.layoutRight.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.layoutRight.Controls.Add(this.grpMove, 0, 0);
            this.layoutRight.Controls.Add(this.grpMoveMode, 0, 1);
            this.layoutRight.Controls.Add(this.tblJog, 0, 2);
            this.layoutRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.layoutRight.Location = new System.Drawing.Point(200, 0);
            this.layoutRight.Margin = new System.Windows.Forms.Padding(0);
            this.layoutRight.Name = "layoutRight";
            this.layoutRight.Padding = new System.Windows.Forms.Padding(6);
            this.layoutRight.RowCount = 3;
            this.layoutRight.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 56F));
            this.layoutRight.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 56F));
            this.layoutRight.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.layoutRight.Size = new System.Drawing.Size(344, 321);
            this.layoutRight.TabIndex = 1;
            // 
            // grpMove
            // 
            this.grpMove.Controls.Add(this.rdoFine);
            this.grpMove.Controls.Add(this.rdoCoarse);
            this.grpMove.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpMove.Location = new System.Drawing.Point(6, 6);
            this.grpMove.Margin = new System.Windows.Forms.Padding(0, 0, 0, 6);
            this.grpMove.Name = "grpMove";
            this.grpMove.Padding = new System.Windows.Forms.Padding(10, 8, 10, 8);
            this.grpMove.Size = new System.Drawing.Size(332, 50);
            this.grpMove.TabIndex = 0;
            this.grpMove.TabStop = false;
            this.grpMove.Text = "Move";
            // 
            // rdoFine
            // 
            this.rdoFine.AutoSize = true;
            this.rdoFine.Location = new System.Drawing.Point(12, 22);
            this.rdoFine.Name = "rdoFine";
            this.rdoFine.Size = new System.Drawing.Size(47, 16);
            this.rdoFine.TabIndex = 0;
            this.rdoFine.Text = "Fine";
            // 
            // rdoCoarse
            // 
            this.rdoCoarse.AutoSize = true;
            this.rdoCoarse.Checked = true;
            this.rdoCoarse.Location = new System.Drawing.Point(80, 22);
            this.rdoCoarse.Name = "rdoCoarse";
            this.rdoCoarse.Size = new System.Drawing.Size(64, 16);
            this.rdoCoarse.TabIndex = 1;
            this.rdoCoarse.TabStop = true;
            this.rdoCoarse.Text = "Coarse";
            // 
            // grpMoveMode
            // 
            this.grpMoveMode.Controls.Add(this.rdoContinuous);
            this.grpMoveMode.Controls.Add(this.rdoStep);
            this.grpMoveMode.Controls.Add(this.nudStep);
            this.grpMoveMode.Controls.Add(this.btnStepPreset);
            this.grpMoveMode.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpMoveMode.Location = new System.Drawing.Point(6, 62);
            this.grpMoveMode.Margin = new System.Windows.Forms.Padding(0, 0, 0, 6);
            this.grpMoveMode.Name = "grpMoveMode";
            this.grpMoveMode.Padding = new System.Windows.Forms.Padding(10, 8, 10, 8);
            this.grpMoveMode.Size = new System.Drawing.Size(332, 50);
            this.grpMoveMode.TabIndex = 1;
            this.grpMoveMode.TabStop = false;
            this.grpMoveMode.Text = "Move Mode";
            // 
            // rdoContinuous
            // 
            this.rdoContinuous.AutoSize = true;
            this.rdoContinuous.Checked = true;
            this.rdoContinuous.Location = new System.Drawing.Point(12, 22);
            this.rdoContinuous.Name = "rdoContinuous";
            this.rdoContinuous.Size = new System.Drawing.Size(87, 16);
            this.rdoContinuous.TabIndex = 0;
            this.rdoContinuous.TabStop = true;
            this.rdoContinuous.Text = "Continuous";
            // 
            // rdoStep
            // 
            this.rdoStep.AutoSize = true;
            this.rdoStep.Location = new System.Drawing.Point(110, 22);
            this.rdoStep.Name = "rdoStep";
            this.rdoStep.Size = new System.Drawing.Size(48, 16);
            this.rdoStep.TabIndex = 1;
            this.rdoStep.Text = "Step";
            // 
            // nudStep
            // 
            this.nudStep.DecimalPlaces = 3;
            this.nudStep.Enabled = false;
            this.nudStep.Location = new System.Drawing.Point(160, 20);
            this.nudStep.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.nudStep.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            196608});
            this.nudStep.Name = "nudStep";
            this.nudStep.Size = new System.Drawing.Size(70, 21);
            this.nudStep.TabIndex = 2;
            this.nudStep.Value = new decimal(new int[] {
            1000,
            0,
            0,
            196608});
            // 
            // btnStepPreset
            // 
            this.btnStepPreset.Location = new System.Drawing.Point(236, 19);
            this.btnStepPreset.Name = "btnStepPreset";
            this.btnStepPreset.Size = new System.Drawing.Size(28, 23);
            this.btnStepPreset.TabIndex = 3;
            this.btnStepPreset.Text = "#";
            // 
            // tblJog
            // 
            this.tblJog.ColumnCount = 4;
            this.tblJog.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tblJog.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tblJog.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tblJog.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tblJog.Controls.Add(this.button2, 2, 2);
            this.tblJog.Controls.Add(this.button1, 0, 2);
            this.tblJog.Controls.Add(this.btnTMinus, 0, 0);
            this.tblJog.Controls.Add(this.btnYPlus, 1, 0);
            this.tblJog.Controls.Add(this.btnTPlus, 2, 0);
            this.tblJog.Controls.Add(this.btnZPlus, 3, 0);
            this.tblJog.Controls.Add(this.btnXMinus, 0, 1);
            this.tblJog.Controls.Add(this.btnStop, 1, 1);
            this.tblJog.Controls.Add(this.btnXPlus, 2, 1);
            this.tblJog.Controls.Add(this.btnZMinus, 3, 1);
            this.tblJog.Controls.Add(this.btnYMinus, 1, 2);
            this.tblJog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tblJog.Location = new System.Drawing.Point(6, 118);
            this.tblJog.Margin = new System.Windows.Forms.Padding(0);
            this.tblJog.Name = "tblJog";
            this.tblJog.Padding = new System.Windows.Forms.Padding(4);
            this.tblJog.RowCount = 3;
            this.tblJog.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.3333F));
            this.tblJog.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.3333F));
            this.tblJog.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.3333F));
            this.tblJog.Size = new System.Drawing.Size(332, 197);
            this.tblJog.TabIndex = 2;
            // 
            // btnTMinus
            // 
            this.btnTMinus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnTMinus.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.btnTMinus.Location = new System.Drawing.Point(10, 10);
            this.btnTMinus.Margin = new System.Windows.Forms.Padding(6);
            this.btnTMinus.Name = "btnTMinus";
            this.btnTMinus.Size = new System.Drawing.Size(69, 51);
            this.btnTMinus.TabIndex = 0;
            this.btnTMinus.Text = "T-";
            // 
            // btnYPlus
            // 
            this.btnYPlus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnYPlus.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.btnYPlus.Location = new System.Drawing.Point(91, 10);
            this.btnYPlus.Margin = new System.Windows.Forms.Padding(6);
            this.btnYPlus.Name = "btnYPlus";
            this.btnYPlus.Size = new System.Drawing.Size(69, 51);
            this.btnYPlus.TabIndex = 1;
            this.btnYPlus.Text = "Y+";
            // 
            // btnTPlus
            // 
            this.btnTPlus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnTPlus.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.btnTPlus.Location = new System.Drawing.Point(172, 10);
            this.btnTPlus.Margin = new System.Windows.Forms.Padding(6);
            this.btnTPlus.Name = "btnTPlus";
            this.btnTPlus.Size = new System.Drawing.Size(69, 51);
            this.btnTPlus.TabIndex = 2;
            this.btnTPlus.Text = "T+";
            // 
            // btnZPlus
            // 
            this.btnZPlus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnZPlus.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.btnZPlus.Location = new System.Drawing.Point(253, 10);
            this.btnZPlus.Margin = new System.Windows.Forms.Padding(6);
            this.btnZPlus.Name = "btnZPlus";
            this.btnZPlus.Size = new System.Drawing.Size(69, 51);
            this.btnZPlus.TabIndex = 3;
            this.btnZPlus.Text = "Z+";
            // 
            // btnXMinus
            // 
            this.btnXMinus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnXMinus.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.btnXMinus.Location = new System.Drawing.Point(10, 73);
            this.btnXMinus.Margin = new System.Windows.Forms.Padding(6);
            this.btnXMinus.Name = "btnXMinus";
            this.btnXMinus.Size = new System.Drawing.Size(69, 51);
            this.btnXMinus.TabIndex = 4;
            this.btnXMinus.Text = "X-";
            // 
            // btnStop
            // 
            this.btnStop.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnStop.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.btnStop.Location = new System.Drawing.Point(91, 73);
            this.btnStop.Margin = new System.Windows.Forms.Padding(6);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(69, 51);
            this.btnStop.TabIndex = 5;
            this.btnStop.Text = "STOP";
            // 
            // btnXPlus
            // 
            this.btnXPlus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnXPlus.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.btnXPlus.Location = new System.Drawing.Point(172, 73);
            this.btnXPlus.Margin = new System.Windows.Forms.Padding(6);
            this.btnXPlus.Name = "btnXPlus";
            this.btnXPlus.Size = new System.Drawing.Size(69, 51);
            this.btnXPlus.TabIndex = 6;
            this.btnXPlus.Text = "X+";
            // 
            // btnZMinus
            // 
            this.btnZMinus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnZMinus.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.btnZMinus.Location = new System.Drawing.Point(253, 73);
            this.btnZMinus.Margin = new System.Windows.Forms.Padding(6);
            this.btnZMinus.Name = "btnZMinus";
            this.btnZMinus.Size = new System.Drawing.Size(69, 51);
            this.btnZMinus.TabIndex = 7;
            this.btnZMinus.Text = "Z-";
            // 
            // btnYMinus
            // 
            this.btnYMinus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnYMinus.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.btnYMinus.Location = new System.Drawing.Point(91, 136);
            this.btnYMinus.Margin = new System.Windows.Forms.Padding(6);
            this.btnYMinus.Name = "btnYMinus";
            this.btnYMinus.Size = new System.Drawing.Size(69, 51);
            this.btnYMinus.TabIndex = 8;
            this.btnYMinus.Text = "Y-";
            // 
            // button1
            // 
            this.button1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.button1.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.button1.Location = new System.Drawing.Point(10, 136);
            this.button1.Margin = new System.Windows.Forms.Padding(6);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(69, 51);
            this.button1.TabIndex = 9;
            this.button1.Text = "45-";
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.button2.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.button2.Location = new System.Drawing.Point(172, 136);
            this.button2.Margin = new System.Windows.Forms.Padding(6);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(69, 51);
            this.button2.TabIndex = 10;
            this.button2.Text = "45+";
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // Form_AxisJogPopup
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(544, 321);
            this.Controls.Add(this.layoutRoot);
            this.MinimumSize = new System.Drawing.Size(560, 360);
            this.Name = "Form_AxisJogPopup";
            this.Text = "Jog Panel";
            this.layoutRoot.ResumeLayout(false);
            this.layoutLeft.ResumeLayout(false);
            this.grpSelectAxis.ResumeLayout(false);
            this.layoutRight.ResumeLayout(false);
            this.grpMove.ResumeLayout(false);
            this.grpMove.PerformLayout();
            this.grpMoveMode.ResumeLayout(false);
            this.grpMoveMode.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudStep)).EndInit();
            this.tblJog.ResumeLayout(false);
            this.ResumeLayout(false);

        }


        private Button CreateJogButton(string text)
        {
            Button b = new Button();
            b.Text = text;
            b.Dock = DockStyle.Fill;
            b.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            b.Margin = new Padding(6);
            return b;
        }

        private Button button2;
        private Button button1;
    }
}
