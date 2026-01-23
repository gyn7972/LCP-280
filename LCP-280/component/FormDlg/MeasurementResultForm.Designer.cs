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
        private Button btnResultClear;
        private Button btnResultSave;

        private TableLayoutPanel layoutRoot;
        private Panel panelTop;
        private Panel panelBottomSpacer;
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
            this.btnResultClear = new System.Windows.Forms.Button();
            this.btnResultSave = new System.Windows.Forms.Button();
            this.btnResultLoad = new System.Windows.Forms.Button();
            this.dataGridResult = new System.Windows.Forms.DataGridView();
            this.panelBottomSpacer = new System.Windows.Forms.Panel();
            this.layoutRoot.SuspendLayout();
            this.panelTop.SuspendLayout();
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
            this.panelTop.Controls.Add(this.btnResultClear);
            this.panelTop.Controls.Add(this.btnResultSave);
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
            this.lbMeasureTime.Location = new System.Drawing.Point(216, 48);
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
            // btnResultClear
            // 
            this.btnResultClear.Location = new System.Drawing.Point(888, 43);
            this.btnResultClear.Name = "btnResultClear";
            this.btnResultClear.Size = new System.Drawing.Size(100, 28);
            this.btnResultClear.TabIndex = 9;
            this.btnResultClear.Text = "Clear";
            this.btnResultClear.Click += new System.EventHandler(this.btnResultClear_Click);
            // 
            // btnResultSave
            // 
            this.btnResultSave.Location = new System.Drawing.Point(994, 8);
            this.btnResultSave.Name = "btnResultSave";
            this.btnResultSave.Size = new System.Drawing.Size(100, 28);
            this.btnResultSave.TabIndex = 10;
            this.btnResultSave.Text = "Save CSV";
            this.btnResultSave.Click += new System.EventHandler(this.btnResultSave_Click);
            // 
            // btnResultLoad
            // 
            this.btnResultLoad.Location = new System.Drawing.Point(994, 43);
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
            ((System.ComponentModel.ISupportInitialize)(this.dataGridResult)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion


    }
}