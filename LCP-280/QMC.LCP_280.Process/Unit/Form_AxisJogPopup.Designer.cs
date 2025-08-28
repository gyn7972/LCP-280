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
            this.components = new System.ComponentModel.Container();

            // ===== 인스턴스 생성 =====
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

            // ===== Suspend =====
            this.SuspendLayout();
            this.layoutRoot.SuspendLayout();
            this.layoutLeft.SuspendLayout();
            this.grpSelectAxis.SuspendLayout();
            this.layoutRight.SuspendLayout();
            this.grpMove.SuspendLayout();
            this.grpMoveMode.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudStep)).BeginInit();
            this.tblJog.SuspendLayout();

            // ===== Form =====
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Text = "Jog Panel";
            this.MinimumSize = new System.Drawing.Size(560, 360);
            this.Padding = new System.Windows.Forms.Padding(0);

            // ===== Root =====
            this.layoutRoot.ColumnCount = 2;
            this.layoutRoot.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 200F));
            this.layoutRoot.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.layoutRoot.RowCount = 1;
            this.layoutRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.layoutRoot.Dock = System.Windows.Forms.DockStyle.Fill;
            this.layoutRoot.Margin = new System.Windows.Forms.Padding(0);
            this.layoutRoot.Name = "layoutRoot";

            // ===== Left =====
            this.layoutLeft.ColumnCount = 1;
            this.layoutLeft.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.layoutLeft.RowCount = 2;
            this.layoutLeft.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 72F));
            this.layoutLeft.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.layoutLeft.Dock = System.Windows.Forms.DockStyle.Fill;
            this.layoutLeft.Padding = new System.Windows.Forms.Padding(6);
            this.layoutLeft.Margin = new System.Windows.Forms.Padding(0);
            this.layoutLeft.Name = "layoutLeft";

            // Position label
            this.lblPosition.BackColor = System.Drawing.Color.Black;
            this.lblPosition.ForeColor = System.Drawing.Color.Lime;
            this.lblPosition.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblPosition.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblPosition.Font = new System.Drawing.Font("Consolas", 20F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblPosition.Margin = new System.Windows.Forms.Padding(0, 0, 0, 6);
            this.lblPosition.Name = "lblPosition";
            this.lblPosition.Text = "000.000";
            this.lblPosition.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

            // Select Axis group
            this.grpSelectAxis.Text = "Select Axis";
            this.grpSelectAxis.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpSelectAxis.Padding = new System.Windows.Forms.Padding(6);
            this.grpSelectAxis.Margin = new System.Windows.Forms.Padding(0);
            this.grpSelectAxis.Name = "grpSelectAxis";

            // ListBoxItemsView
            this.selectAxisListBoxItemsView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.selectAxisListBoxItemsView.GroupName = "Select Axis";
            this.selectAxisListBoxItemsView.BorderWidth = 2;
            this.selectAxisListBoxItemsView.Name = "selectAxisListBoxItemsView";

            this.grpSelectAxis.Controls.Add(this.selectAxisListBoxItemsView);
            this.layoutLeft.Controls.Add(this.lblPosition, 0, 0);
            this.layoutLeft.Controls.Add(this.grpSelectAxis, 0, 1);

            // ===== Right =====
            this.layoutRight.ColumnCount = 1;
            this.layoutRight.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.layoutRight.RowCount = 3;
            this.layoutRight.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 56F));
            this.layoutRight.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 56F));
            this.layoutRight.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.layoutRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.layoutRight.Padding = new System.Windows.Forms.Padding(6);
            this.layoutRight.Margin = new System.Windows.Forms.Padding(0);
            this.layoutRight.Name = "layoutRight";

            // Move group
            this.grpMove.Text = "Move";
            this.grpMove.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpMove.Padding = new System.Windows.Forms.Padding(10, 8, 10, 8);
            this.grpMove.Margin = new System.Windows.Forms.Padding(0, 0, 0, 6);
            this.grpMove.Name = "grpMove";

            this.rdoFine.AutoSize = true;
            this.rdoFine.Text = "Fine";
            this.rdoFine.Location = new System.Drawing.Point(12, 22);
            this.rdoFine.Name = "rdoFine";

            this.rdoCoarse.AutoSize = true;
            this.rdoCoarse.Checked = true;
            this.rdoCoarse.Text = "Coarse";
            this.rdoCoarse.Location = new System.Drawing.Point(80, 22);
            this.rdoCoarse.Name = "rdoCoarse";

            this.grpMove.Controls.Add(this.rdoFine);
            this.grpMove.Controls.Add(this.rdoCoarse);

            // Move Mode group
            this.grpMoveMode.Text = "Move Mode";
            this.grpMoveMode.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpMoveMode.Padding = new System.Windows.Forms.Padding(10, 8, 10, 8);
            this.grpMoveMode.Margin = new System.Windows.Forms.Padding(0, 0, 0, 6);
            this.grpMoveMode.Name = "grpMoveMode";

            this.rdoContinuous.AutoSize = true;
            this.rdoContinuous.Checked = true;
            this.rdoContinuous.Text = "Continuous";
            this.rdoContinuous.Location = new System.Drawing.Point(12, 22);
            this.rdoContinuous.Name = "rdoContinuous";

            this.rdoStep.AutoSize = true;
            this.rdoStep.Text = "Step";
            this.rdoStep.Location = new System.Drawing.Point(110, 22);
            this.rdoStep.Name = "rdoStep";

            this.nudStep.DecimalPlaces = 3;
            this.nudStep.Minimum = 0.001M;
            this.nudStep.Maximum = 1000M;
            this.nudStep.Value = 1.000M;
            this.nudStep.Enabled = false;
            this.nudStep.Width = 70;
            this.nudStep.Location = new System.Drawing.Point(160, 20);
            this.nudStep.Name = "nudStep";

            this.btnStepPreset.Text = "#";
            this.btnStepPreset.Width = 28;
            this.btnStepPreset.Location = new System.Drawing.Point(236, 19);
            this.btnStepPreset.Name = "btnStepPreset";

            this.grpMoveMode.Controls.Add(this.rdoContinuous);
            this.grpMoveMode.Controls.Add(this.rdoStep);
            this.grpMoveMode.Controls.Add(this.nudStep);
            this.grpMoveMode.Controls.Add(this.btnStepPreset);

            // Jog grid
            this.tblJog.ColumnCount = 4;
            this.tblJog.RowCount = 3;
            this.tblJog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tblJog.Padding = new System.Windows.Forms.Padding(4);
            this.tblJog.Margin = new System.Windows.Forms.Padding(0);
            this.tblJog.Name = "tblJog";
            // ColumnStyles
            this.tblJog.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tblJog.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tblJog.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tblJog.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            // RowStyles
            this.tblJog.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.3333F));
            this.tblJog.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.3333F));
            this.tblJog.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.3333F));

            // 공통 폰트/마진
            System.Drawing.Font jogFont = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            System.Windows.Forms.Padding jogMargin = new System.Windows.Forms.Padding(6);

            // Buttons (개별 생성 후 설정)
            this.btnTMinus.Text = "T-"; this.btnTMinus.Dock = System.Windows.Forms.DockStyle.Fill; this.btnTMinus.Font = jogFont; this.btnTMinus.Margin = jogMargin; this.btnTMinus.Name = "btnTMinus";
            this.btnYPlus.Text = "Y+"; this.btnYPlus.Dock = System.Windows.Forms.DockStyle.Fill; this.btnYPlus.Font = jogFont; this.btnYPlus.Margin = jogMargin; this.btnYPlus.Name = "btnYPlus";
            this.btnTPlus.Text = "T+"; this.btnTPlus.Dock = System.Windows.Forms.DockStyle.Fill; this.btnTPlus.Font = jogFont; this.btnTPlus.Margin = jogMargin; this.btnTPlus.Name = "btnTPlus";
            this.btnZPlus.Text = "Z+"; this.btnZPlus.Dock = System.Windows.Forms.DockStyle.Fill; this.btnZPlus.Font = jogFont; this.btnZPlus.Margin = jogMargin; this.btnZPlus.Name = "btnZPlus";
            this.btnXMinus.Text = "X-"; this.btnXMinus.Dock = System.Windows.Forms.DockStyle.Fill; this.btnXMinus.Font = jogFont; this.btnXMinus.Margin = jogMargin; this.btnXMinus.Name = "btnXMinus";
            this.btnStop.Text = "STOP"; this.btnStop.Dock = System.Windows.Forms.DockStyle.Fill; this.btnStop.Font = jogFont; this.btnStop.Margin = jogMargin; this.btnStop.Name = "btnStop";
            this.btnXPlus.Text = "X+"; this.btnXPlus.Dock = System.Windows.Forms.DockStyle.Fill; this.btnXPlus.Font = jogFont; this.btnXPlus.Margin = jogMargin; this.btnXPlus.Name = "btnXPlus";
            this.btnZMinus.Text = "Z-"; this.btnZMinus.Dock = System.Windows.Forms.DockStyle.Fill; this.btnZMinus.Font = jogFont; this.btnZMinus.Margin = jogMargin; this.btnZMinus.Name = "btnZMinus";
            this.btnYMinus.Text = "Y-"; this.btnYMinus.Dock = System.Windows.Forms.DockStyle.Fill; this.btnYMinus.Font = jogFont; this.btnYMinus.Margin = jogMargin; this.btnYMinus.Name = "btnYMinus";

            // 그리드 추가
            this.tblJog.Controls.Add(this.btnTMinus, 0, 0);
            this.tblJog.Controls.Add(this.btnYPlus, 1, 0);
            this.tblJog.Controls.Add(this.btnTPlus, 2, 0);
            this.tblJog.Controls.Add(this.btnZPlus, 3, 0);
            this.tblJog.Controls.Add(this.btnXMinus, 0, 1);
            this.tblJog.Controls.Add(this.btnStop, 1, 1);
            this.tblJog.Controls.Add(this.btnXPlus, 2, 1);
            this.tblJog.Controls.Add(this.btnZMinus, 3, 1);
            this.tblJog.Controls.Add(this.btnYMinus, 1, 2);

            // Right compose
            this.layoutRight.Controls.Add(this.grpMove, 0, 0);
            this.layoutRight.Controls.Add(this.grpMoveMode, 0, 1);
            this.layoutRight.Controls.Add(this.tblJog, 0, 2);

            // Root compose
            this.layoutRoot.Controls.Add(this.layoutLeft, 0, 0);
            this.layoutRoot.Controls.Add(this.layoutRight, 1, 0);
            this.Controls.Add(this.layoutRoot);

            // Left children
            this.layoutLeft.Controls.Add(this.lblPosition, 0, 0);
            this.layoutLeft.Controls.Add(this.grpSelectAxis, 0, 1);

            // ===== Resume =====
            this.tblJog.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.nudStep)).EndInit();
            this.grpMoveMode.ResumeLayout(false); this.grpMoveMode.PerformLayout();
            this.grpMove.ResumeLayout(false); this.grpMove.PerformLayout();
            this.layoutRight.ResumeLayout(false);
            this.grpSelectAxis.ResumeLayout(false);
            this.layoutLeft.ResumeLayout(false);
            this.layoutRoot.ResumeLayout(false);
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
    }
}
