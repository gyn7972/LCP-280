using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace QMC.LCP_280.Process.Component
{
    partial class MeasurementResultForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private IContainer components = null;

        // UI
        private DataGridView dataGridResult;
        private Label lbResultValue;
        private Label lbMeasureTime;
        private Label lbCurrentIndexNo;
        private Button btnTestStart;
        private Button btnTestStop;
        private Button btnLastClear;
        private Button btnResultClear;
        private Button btnResultSave;
        private CheckBox chkRepeat;
        private NumericUpDown nudRepeatCount;
        private NumericUpDown nudIntervalDelay;

        private Label lblProbeIndex;
        private ComboBox cbProbeIndex;
        private GroupBox grpContactMode;
        private RadioButton rbTop;
        private RadioButton rbBottom;

        private TableLayoutPanel layoutRoot;
        private Panel panelTop;
        private Panel panelBottomSpacer;

        private Button buttonSeqMeasureStart;
        private Button btnResultLoad;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.layoutRoot = new System.Windows.Forms.TableLayoutPanel();
            this.panelTop = new System.Windows.Forms.Panel();
            this.lbResultValue = new System.Windows.Forms.Label();
            this.lbMeasureTime = new System.Windows.Forms.Label();
            this.lbCurrentIndexNo = new System.Windows.Forms.Label();
            this.chkRepeat = new System.Windows.Forms.CheckBox();
            this.nudRepeatCount = new System.Windows.Forms.NumericUpDown();
            this.nudIntervalDelay = new System.Windows.Forms.NumericUpDown();
            this.btnTestStart = new System.Windows.Forms.Button();
            this.btnTestStop = new System.Windows.Forms.Button();
            this.btnLastClear = new System.Windows.Forms.Button();
            this.btnResultClear = new System.Windows.Forms.Button();
            this.btnResultSave = new System.Windows.Forms.Button();
            this.buttonSeqMeasureStart = new System.Windows.Forms.Button();
            this.lblProbeIndex = new System.Windows.Forms.Label();
            this.cbProbeIndex = new System.Windows.Forms.ComboBox();
            this.grpContactMode = new System.Windows.Forms.GroupBox();
            this.rbTop = new System.Windows.Forms.RadioButton();
            this.rbBottom = new System.Windows.Forms.RadioButton();
            this.btnResultLoad = new System.Windows.Forms.Button();
            this.dataGridResult = new System.Windows.Forms.DataGridView();
            this.panelBottomSpacer = new System.Windows.Forms.Panel();
            this.layoutRoot.SuspendLayout();
            this.panelTop.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudRepeatCount)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudIntervalDelay)).BeginInit();
            this.grpContactMode.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridResult)).BeginInit();
            this.SuspendLayout();
            // 
            // layoutRoot
            // 
            this.layoutRoot.ColumnCount = 1;
            this.layoutRoot.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.layoutRoot.Controls.Add(this.panelTop, 0, 0);
            this.layoutRoot.Controls.Add(this.dataGridResult, 0, 1);
            this.layoutRoot.Controls.Add(this.panelBottomSpacer, 0, 2);
            this.layoutRoot.Dock = System.Windows.Forms.DockStyle.Fill;
            this.layoutRoot.Location = new System.Drawing.Point(0, 0);
            this.layoutRoot.Name = "layoutRoot";
            this.layoutRoot.RowCount = 3;
            this.layoutRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 90F));
            this.layoutRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.layoutRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 8F));
            this.layoutRoot.Size = new System.Drawing.Size(1109, 756);
            this.layoutRoot.TabIndex = 0;
            // 
            // panelTop
            // 
            this.panelTop.Controls.Add(this.lbResultValue);
            this.panelTop.Controls.Add(this.lbMeasureTime);
            this.panelTop.Controls.Add(this.lbCurrentIndexNo);
            this.panelTop.Controls.Add(this.chkRepeat);
            this.panelTop.Controls.Add(this.nudRepeatCount);
            this.panelTop.Controls.Add(this.nudIntervalDelay);
            this.panelTop.Controls.Add(this.btnTestStart);
            this.panelTop.Controls.Add(this.btnTestStop);
            this.panelTop.Controls.Add(this.btnLastClear);
            this.panelTop.Controls.Add(this.btnResultClear);
            this.panelTop.Controls.Add(this.btnResultSave);
            this.panelTop.Controls.Add(this.buttonSeqMeasureStart);
            this.panelTop.Controls.Add(this.lblProbeIndex);
            this.panelTop.Controls.Add(this.cbProbeIndex);
            this.panelTop.Controls.Add(this.grpContactMode);
            this.panelTop.Controls.Add(this.btnResultLoad);
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelTop.Location = new System.Drawing.Point(3, 3);
            this.panelTop.Name = "panelTop";
            this.panelTop.Size = new System.Drawing.Size(1103, 84);
            this.panelTop.TabIndex = 0;
            // 
            // lbResultValue
            // 
            this.lbResultValue.AutoSize = true;
            this.lbResultValue.ForeColor = System.Drawing.Color.Lime;
            this.lbResultValue.Location = new System.Drawing.Point(10, 8);
            this.lbResultValue.Name = "lbResultValue";
            this.lbResultValue.Size = new System.Drawing.Size(0, 18);
            this.lbResultValue.TabIndex = 0;
            // 
            // lbMeasureTime
            // 
            this.lbMeasureTime.AutoSize = true;
            this.lbMeasureTime.Location = new System.Drawing.Point(9, 8);
            this.lbMeasureTime.Name = "lbMeasureTime";
            this.lbMeasureTime.Size = new System.Drawing.Size(144, 18);
            this.lbMeasureTime.TabIndex = 1;
            this.lbMeasureTime.Text = "Measure Time: -";
            // 
            // lbCurrentIndexNo
            // 
            this.lbCurrentIndexNo.AutoSize = true;
            this.lbCurrentIndexNo.Location = new System.Drawing.Point(9, 48);
            this.lbCurrentIndexNo.Name = "lbCurrentIndexNo";
            this.lbCurrentIndexNo.Size = new System.Drawing.Size(160, 18);
            this.lbCurrentIndexNo.TabIndex = 2;
            this.lbCurrentIndexNo.Text = "Rotary Index No: -";
            // 
            // chkRepeat
            // 
            this.chkRepeat.AutoSize = true;
            this.chkRepeat.Location = new System.Drawing.Point(245, 8);
            this.chkRepeat.Name = "chkRepeat";
            this.chkRepeat.Size = new System.Drawing.Size(90, 22);
            this.chkRepeat.TabIndex = 3;
            this.chkRepeat.Text = "Repeat";
            // 
            // nudRepeatCount
            // 
            this.nudRepeatCount.Location = new System.Drawing.Point(245, 38);
            this.nudRepeatCount.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.nudRepeatCount.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudRepeatCount.Name = "nudRepeatCount";
            this.nudRepeatCount.Size = new System.Drawing.Size(70, 28);
            this.nudRepeatCount.TabIndex = 4;
            this.nudRepeatCount.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // nudIntervalDelay
            // 
            this.nudIntervalDelay.Location = new System.Drawing.Point(325, 38);
            this.nudIntervalDelay.Maximum = new decimal(new int[] {
            600000,
            0,
            0,
            0});
            this.nudIntervalDelay.Name = "nudIntervalDelay";
            this.nudIntervalDelay.Size = new System.Drawing.Size(80, 28);
            this.nudIntervalDelay.TabIndex = 5;
            this.nudIntervalDelay.Value = new decimal(new int[] {
            500,
            0,
            0,
            0});
            // 
            // btnTestStart
            // 
            this.btnTestStart.Location = new System.Drawing.Point(425, 8);
            this.btnTestStart.Name = "btnTestStart";
            this.btnTestStart.Size = new System.Drawing.Size(80, 28);
            this.btnTestStart.TabIndex = 6;
            this.btnTestStart.Text = "Start";
            this.btnTestStart.Click += new System.EventHandler(this.btnTestStart_Click);
            // 
            // btnTestStop
            // 
            this.btnTestStop.Location = new System.Drawing.Point(425, 38);
            this.btnTestStop.Name = "btnTestStop";
            this.btnTestStop.Size = new System.Drawing.Size(80, 28);
            this.btnTestStop.TabIndex = 7;
            this.btnTestStop.Text = "Stop";
            this.btnTestStop.Click += new System.EventHandler(this.btnTestStop_Click);
            // 
            // btnLastClear
            // 
            this.btnLastClear.Location = new System.Drawing.Point(515, 8);
            this.btnLastClear.Name = "btnLastClear";
            this.btnLastClear.Size = new System.Drawing.Size(100, 28);
            this.btnLastClear.TabIndex = 8;
            this.btnLastClear.Text = "Remove Last";
            this.btnLastClear.Click += new System.EventHandler(this.btnLastClear_Click);
            // 
            // btnResultClear
            // 
            this.btnResultClear.Location = new System.Drawing.Point(515, 38);
            this.btnResultClear.Name = "btnResultClear";
            this.btnResultClear.Size = new System.Drawing.Size(100, 28);
            this.btnResultClear.TabIndex = 9;
            this.btnResultClear.Text = "Clear";
            this.btnResultClear.Click += new System.EventHandler(this.btnResultClear_Click);
            // 
            // btnResultSave
            // 
            this.btnResultSave.Location = new System.Drawing.Point(620, 8);
            this.btnResultSave.Name = "btnResultSave";
            this.btnResultSave.Size = new System.Drawing.Size(100, 28);
            this.btnResultSave.TabIndex = 10;
            this.btnResultSave.Text = "Save CSV";
            this.btnResultSave.Click += new System.EventHandler(this.btnResultSave_Click);
            // 
            // buttonSeqMeasureStart
            // 
            this.buttonSeqMeasureStart.Location = new System.Drawing.Point(988, 39);
            this.buttonSeqMeasureStart.Name = "buttonSeqMeasureStart";
            this.buttonSeqMeasureStart.Size = new System.Drawing.Size(100, 28);
            this.buttonSeqMeasureStart.TabIndex = 11;
            this.buttonSeqMeasureStart.Text = "Seq Measure";
            this.buttonSeqMeasureStart.Click += new System.EventHandler(this.buttonSeqMeasureStart_Click);
            // 
            // lblProbeIndex
            // 
            this.lblProbeIndex.AutoSize = true;
            this.lblProbeIndex.Location = new System.Drawing.Point(921, 8);
            this.lblProbeIndex.Name = "lblProbeIndex";
            this.lblProbeIndex.Size = new System.Drawing.Size(64, 18);
            this.lblProbeIndex.TabIndex = 12;
            this.lblProbeIndex.Text = "Socket";
            // 
            // cbProbeIndex
            // 
            this.cbProbeIndex.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbProbeIndex.Items.AddRange(new object[] {
            "1",
            "2",
            "3",
            "4",
            "5",
            "6",
            "7",
            "8"});
            this.cbProbeIndex.Location = new System.Drawing.Point(991, 6);
            this.cbProbeIndex.Name = "cbProbeIndex";
            this.cbProbeIndex.Size = new System.Drawing.Size(97, 26);
            this.cbProbeIndex.TabIndex = 13;
            // 
            // grpContactMode
            // 
            this.grpContactMode.Controls.Add(this.rbTop);
            this.grpContactMode.Controls.Add(this.rbBottom);
            this.grpContactMode.Location = new System.Drawing.Point(727, 8);
            this.grpContactMode.Name = "grpContactMode";
            this.grpContactMode.Size = new System.Drawing.Size(188, 61);
            this.grpContactMode.TabIndex = 14;
            this.grpContactMode.TabStop = false;
            this.grpContactMode.Text = "Contact Mode";
            // 
            // rbTop
            // 
            this.rbTop.AutoSize = true;
            this.rbTop.Checked = true;
            this.rbTop.Location = new System.Drawing.Point(6, 30);
            this.rbTop.Name = "rbTop";
            this.rbTop.Size = new System.Drawing.Size(64, 22);
            this.rbTop.TabIndex = 0;
            this.rbTop.TabStop = true;
            this.rbTop.Text = "Top";
            // 
            // rbBottom
            // 
            this.rbBottom.AutoSize = true;
            this.rbBottom.Location = new System.Drawing.Point(74, 30);
            this.rbBottom.Name = "rbBottom";
            this.rbBottom.Size = new System.Drawing.Size(91, 22);
            this.rbBottom.TabIndex = 1;
            this.rbBottom.Text = "Bottom";
            // 
            // btnResultLoad
            // 
            this.btnResultLoad.Location = new System.Drawing.Point(621, 41);
            this.btnResultLoad.Name = "btnResultLoad";
            this.btnResultLoad.Size = new System.Drawing.Size(100, 28);
            this.btnResultLoad.TabIndex = 15;
            this.btnResultLoad.Text = "Load CSV";
            this.btnResultLoad.Click += new System.EventHandler(this.btnResultLoad_Click);
            // 
            // dataGridResult
            // 
            this.dataGridResult.AllowUserToAddRows = false;
            this.dataGridResult.AllowUserToDeleteRows = false;
            this.dataGridResult.AllowUserToResizeRows = false;
            this.dataGridResult.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.DisplayedCells;
            this.dataGridResult.ColumnHeadersHeight = 34;
            this.dataGridResult.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridResult.Font = new System.Drawing.Font("맑은 고딕", 8F);
            this.dataGridResult.Location = new System.Drawing.Point(3, 93);
            this.dataGridResult.Name = "dataGridResult";
            this.dataGridResult.ReadOnly = true;
            this.dataGridResult.RowHeadersWidth = 62;
            this.dataGridResult.Size = new System.Drawing.Size(1103, 652);
            this.dataGridResult.TabIndex = 1;
            // 
            // panelBottomSpacer
            // 
            this.panelBottomSpacer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelBottomSpacer.Location = new System.Drawing.Point(3, 751);
            this.panelBottomSpacer.Name = "panelBottomSpacer";
            this.panelBottomSpacer.Size = new System.Drawing.Size(1103, 2);
            this.panelBottomSpacer.TabIndex = 2;
            // 
            // MeasurementResultForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(1109, 756);
            this.Controls.Add(this.layoutRoot);
            this.Name = "MeasurementResultForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Measurement Results";
            this.layoutRoot.ResumeLayout(false);
            this.panelTop.ResumeLayout(false);
            this.panelTop.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudRepeatCount)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudIntervalDelay)).EndInit();
            this.grpContactMode.ResumeLayout(false);
            this.grpContactMode.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridResult)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion


    }
}