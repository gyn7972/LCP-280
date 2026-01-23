using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace QMC.LCP_280.Process.Component
{
    partial class TCorrectionDialog
    {
        private IContainer components = null;

        private DataGridView dgvScan;
        private DataGridViewTextBoxColumn colMarkIndex;
        private Button btnLoad;
        private Button btnSave;
        private Button btnStart;
        private Button btnStop;
        private Button btnCalc;
        private Button btnClose;
        private Label lblAngleStep;
        private TextBox txtAngleStep;
        private GroupBox grpMarks;
        private DataGridView dgvMarks;
        private Label lblTitle;

        private TabControl tabMarks;
        private TabPage tabMark1;
        private TabPage tabMark2;
        private TabPage tabMark3;
        private TabPage tabMark4;
        private TabPage tabMatrix;

        // 각 마크 탭에 배치될 DataGridView
        private DataGridView dgvMark1;
        private DataGridView dgvMark2;
        private DataGridView dgvMark3;
        private DataGridView dgvMark4;

        private DataGridViewTextBoxColumn colIndex;
        private DataGridViewTextBoxColumn colAngle;
        private DataGridViewTextBoxColumn colImageX;
        private DataGridViewTextBoxColumn colImageY;
        private DataGridViewTextBoxColumn colImageT;
        private DataGridViewTextBoxColumn colStageX;
        private DataGridViewTextBoxColumn colStageY;
        private DataGridViewTextBoxColumn colStageT;
        private DataGridViewTextBoxColumn colCalX;
        private DataGridViewTextBoxColumn colCalY;
        private DataGridViewTextBoxColumn colCalT;
        private DataGridViewTextBoxColumn colMIndex;
        private DataGridViewTextBoxColumn colMX;
        private DataGridViewTextBoxColumn colMY;

        private void InitializeComponent()
        {
            // dgvScan
            this.dgvScan = new System.Windows.Forms.DataGridView();
            this.colMarkIndex = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colIndex = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colAngle = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colImageX = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colImageY = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colImageT = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colStageX = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colStageY = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colStageT = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colCalX = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colCalY = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colCalT = new System.Windows.Forms.DataGridViewTextBoxColumn();



            this.btnLoad = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnStart = new System.Windows.Forms.Button();
            this.btnStop = new System.Windows.Forms.Button();
            this.btnCalc = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.lblAngleStep = new System.Windows.Forms.Label();
            this.txtAngleStep = new System.Windows.Forms.TextBox();
            this.grpMarks = new System.Windows.Forms.GroupBox();
            this.dgvMarks = new System.Windows.Forms.DataGridView();
            this.colMIndex = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colMX = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colMY = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.lblTitle = new System.Windows.Forms.Label();
            this.tabMarks = new System.Windows.Forms.TabControl();
            this.tabMark1 = new System.Windows.Forms.TabPage();
            this.tabMark2 = new System.Windows.Forms.TabPage();
            this.tabMark3 = new System.Windows.Forms.TabPage();
            this.tabMark4 = new System.Windows.Forms.TabPage();
            this.tabMatrix = new System.Windows.Forms.TabPage();
            this.dgvMark1 = new System.Windows.Forms.DataGridView();
            this.dgvMark2 = new System.Windows.Forms.DataGridView();
            this.dgvMark3 = new System.Windows.Forms.DataGridView();
            this.dgvMark4 = new System.Windows.Forms.DataGridView();
            ((System.ComponentModel.ISupportInitialize)(this.dgvScan)).BeginInit();
            this.grpMarks.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvMarks)).BeginInit();
            this.tabMarks.SuspendLayout();
            this.tabMatrix.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvMark1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvMark2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvMark3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvMark4)).BeginInit();
            this.SuspendLayout();
            // dgvScan
            this.dgvScan = new System.Windows.Forms.DataGridView();
            this.dgvScan.AllowUserToAddRows = false;
            this.dgvScan.AllowUserToDeleteRows = false;
            this.dgvScan.AllowUserToResizeRows = false;
            this.dgvScan.RowHeadersVisible = false;
            this.dgvScan.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvScan.MultiSelect = false;
            this.dgvScan.Dock = System.Windows.Forms.DockStyle.Fill;          // 탭 전체 채우기
            this.dgvScan.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.DisplayedCells;
            this.dgvScan.ColumnHeadersHeight = 34;
            this.dgvScan.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
                this.colMarkIndex,
                this.colIndex,
                this.colAngle,
                this.colImageX,
                this.colImageY,
                this.colImageT,
                this.colStageX,
                this.colStageY,
                this.colStageT,
                this.colCalX,
                this.colCalY,
                this.colCalT
            });

            // 각 컬럼 기본 폭(너무 좁을 경우 가독성 개선)
            this.colMarkIndex.HeaderText = "MarkIndex";
            this.colMarkIndex.Name = "colMarkIndex";
            this.colMarkIndex.Width = 80;

            this.colIndex.HeaderText = "Index";
            this.colIndex.Name = "colIndex";
            this.colIndex.Width = 60;

            this.colAngle.HeaderText = "Angle";
            this.colAngle.Name = "colAngle";
            this.colAngle.Width = 70;

            this.colImageX.HeaderText = "ImageX";
            this.colImageX.Name = "colImageX";
            this.colImageX.Width = 80;

            this.colImageY.HeaderText = "ImageY";
            this.colImageY.Name = "colImageY";
            this.colImageY.Width = 80;

            this.colImageT.HeaderText = "ImageT";
            this.colImageT.Name = "colImageT";
            this.colImageT.Width = 80;

            this.colStageX.HeaderText = "StageX";
            this.colStageX.Name = "colStageX";
            this.colStageX.Width = 80;

            this.colStageY.HeaderText = "StageY";
            this.colStageY.Name = "colStageY";
            this.colStageY.Width = 80;

            this.colStageT.HeaderText = "StageT";
            this.colStageT.Name = "colStageT";
            this.colStageT.Width = 80;

            this.colCalX.HeaderText = "CalX";
            this.colCalX.Name = "colCalX";
            this.colCalX.Width = 70;

            this.colCalY.HeaderText = "CalY";
            this.colCalY.Name = "colCalY";
            this.colCalY.Width = 70;

            this.colCalT.HeaderText = "CalT";
            this.colCalT.Name = "colCalT";
            this.colCalT.Width = 70;
            // 
            // 버튼/라벨/텍스트/그룹박스/마크 그리드 (우측)
            // 
            this.btnLoad.Location = new System.Drawing.Point(560, 40);
            this.btnLoad.Name = "btnLoad";
            this.btnLoad.Size = new System.Drawing.Size(120, 40);
            this.btnLoad.TabIndex = 2;
            this.btnLoad.Text = "Load";
            this.btnLoad.UseVisualStyleBackColor = true;
            this.btnLoad.Click += new System.EventHandler(this.btnLoad_Click);
            this.btnSave.Location = new System.Drawing.Point(690, 40);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(120, 40);
            this.btnSave.TabIndex = 3;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            this.btnStart.Location = new System.Drawing.Point(560, 360);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(120, 40);
            this.btnStart.TabIndex = 7;
            this.btnStart.Text = "Start";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            this.btnStop.Location = new System.Drawing.Point(690, 360);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(120, 40);
            this.btnStop.TabIndex = 8;
            this.btnStop.Text = "Stop";
            this.btnStop.UseVisualStyleBackColor = true;
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            this.btnCalc.Location = new System.Drawing.Point(560, 410);
            this.btnCalc.Name = "btnCalc";
            this.btnCalc.Size = new System.Drawing.Size(250, 40);
            this.btnCalc.TabIndex = 9;
            this.btnCalc.Text = "Cal.";
            this.btnCalc.UseVisualStyleBackColor = true;
            this.btnCalc.Click += new System.EventHandler(this.btnCalc_Click);
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.Location = new System.Drawing.Point(690, 600);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(120, 40);
            this.btnClose.TabIndex = 10;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            this.lblAngleStep.AutoSize = true;
            this.lblAngleStep.Location = new System.Drawing.Point(560, 320);
            this.lblAngleStep.Name = "lblAngleStep";
            this.lblAngleStep.Size = new System.Drawing.Size(47, 18);
            this.lblAngleStep.TabIndex = 5;
            this.lblAngleStep.Text = "angle";
            this.txtAngleStep.Location = new System.Drawing.Point(610, 316);
            this.txtAngleStep.Name = "txtAngleStep";
            this.txtAngleStep.Size = new System.Drawing.Size(80, 28);
            this.txtAngleStep.TabIndex = 6;
            this.txtAngleStep.Text = "0.1";
            this.grpMarks.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.grpMarks.Controls.Add(this.dgvMarks);
            this.grpMarks.Location = new System.Drawing.Point(560, 100);
            this.grpMarks.Name = "grpMarks";
            this.grpMarks.Size = new System.Drawing.Size(250, 200);
            this.grpMarks.TabIndex = 4;
            this.grpMarks.TabStop = false;
            this.dgvMarks.AllowUserToAddRows = false;
            this.dgvMarks.AllowUserToDeleteRows = false;
            this.dgvMarks.AllowUserToResizeRows = false;
            this.dgvMarks.ColumnHeadersHeight = 34;
            this.dgvMarks.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colMIndex,
            this.colMX,
            this.colMY});
            this.dgvMarks.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvMarks.Location = new System.Drawing.Point(3, 24);
            this.dgvMarks.MultiSelect = false;
            this.dgvMarks.Name = "dgvMarks";
            this.dgvMarks.RowHeadersVisible = false;
            this.dgvMarks.RowHeadersWidth = 62;
            this.dgvMarks.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvMarks.Size = new System.Drawing.Size(244, 173);
            this.dgvMarks.TabIndex = 0;
            this.colMIndex.HeaderText = "Index";
            this.colMIndex.MinimumWidth = 8;
            this.colMIndex.Name = "colMIndex";
            this.colMIndex.Width = 60;
            this.colMX.HeaderText = "PosX";
            this.colMX.MinimumWidth = 8;
            this.colMX.Name = "colMX";
            this.colMX.Width = 80;
            this.colMY.HeaderText = "PosY";
            this.colMY.MinimumWidth = 8;
            this.colMY.Name = "colMY";
            this.colMY.Width = 80;
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTitle.Location = new System.Drawing.Point(12, 9);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(121, 32);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "T-보정 Dlg";
            // 
            // tabMarks
            // 
            this.tabMarks.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)));
            this.tabMarks.Controls.Add(this.tabMark1);
            this.tabMarks.Controls.Add(this.tabMark2);
            this.tabMarks.Controls.Add(this.tabMark3);
            this.tabMarks.Controls.Add(this.tabMark4);
            this.tabMarks.Controls.Add(this.tabMatrix);
            this.tabMarks.Location = new System.Drawing.Point(16, 42);
            this.tabMarks.Name = "tabMarks";
            this.tabMarks.SelectedIndex = 0;
            this.tabMarks.Size = new System.Drawing.Size(520, 600);
            this.tabMarks.TabIndex = 11;
            // 
            // tabMark1
            // 
            this.tabMark1.Controls.Add(this.dgvMark1);
            this.tabMark1.Location = new System.Drawing.Point(4, 29);
            this.tabMark1.Name = "tabMark1";
            this.tabMark1.Padding = new System.Windows.Forms.Padding(3);
            this.tabMark1.Size = new System.Drawing.Size(512, 567);
            this.tabMark1.TabIndex = 0;
            this.tabMark1.Text = "Mark 1";
            this.tabMark1.UseVisualStyleBackColor = true;
            // 
            // tabMark2
            // 
            this.tabMark2.Controls.Add(this.dgvMark2);
            this.tabMark2.Location = new System.Drawing.Point(4, 29);
            this.tabMark2.Name = "tabMark2";
            this.tabMark2.Padding = new System.Windows.Forms.Padding(3);
            this.tabMark2.Size = new System.Drawing.Size(512, 567);
            this.tabMark2.TabIndex = 1;
            this.tabMark2.Text = "Mark 2";
            this.tabMark2.UseVisualStyleBackColor = true;
            // 
            // tabMark3
            // 
            this.tabMark3.Controls.Add(this.dgvMark3);
            this.tabMark3.Location = new System.Drawing.Point(4, 29);
            this.tabMark3.Name = "tabMark3";
            this.tabMark3.Padding = new System.Windows.Forms.Padding(3);
            this.tabMark3.Size = new System.Drawing.Size(512, 567);
            this.tabMark3.TabIndex = 2;
            this.tabMark3.Text = "Mark 3";
            this.tabMark3.UseVisualStyleBackColor = true;
            // 
            // tabMark4
            // 
            this.tabMark4.Controls.Add(this.dgvMark4);
            this.tabMark4.Location = new System.Drawing.Point(4, 29);
            this.tabMark4.Name = "tabMark4";
            this.tabMark4.Padding = new System.Windows.Forms.Padding(3);
            this.tabMark4.Size = new System.Drawing.Size(512, 567);
            this.tabMark4.TabIndex = 3;
            this.tabMark4.Text = "Mark 4";
            this.tabMark4.UseVisualStyleBackColor = true;
            // 
            // tabMatrix
            // 
            this.tabMatrix.Controls.Add(this.dgvScan);
            this.tabMatrix.Location = new System.Drawing.Point(4, 29);
            this.tabMatrix.Name = "tabMatrix";
            this.tabMatrix.Padding = new System.Windows.Forms.Padding(3);
            this.tabMatrix.Size = new System.Drawing.Size(512, 567);
            this.tabMatrix.TabIndex = 4;
            this.tabMatrix.Text = "Matrix 보정";
            this.tabMatrix.UseVisualStyleBackColor = true;
            // 
            // dgvMark1
            // 
            this.dgvMark1.AllowUserToAddRows = false;
            this.dgvMark1.AllowUserToDeleteRows = false;
            this.dgvMark1.AllowUserToResizeRows = false;
            this.dgvMark1.ColumnHeadersHeight = 34;
            this.dgvMark1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            new System.Windows.Forms.DataGridViewTextBoxColumn(){ HeaderText="Index", Name="colIndex", ReadOnly=true, Width=60 },
            new System.Windows.Forms.DataGridViewTextBoxColumn(){ HeaderText="각도", Name="colAngle", ReadOnly=true, Width=60 },
            new System.Windows.Forms.DataGridViewTextBoxColumn(){ HeaderText="PosX(Stage)", Name="colStageX", ReadOnly=true },
            new System.Windows.Forms.DataGridViewTextBoxColumn(){ HeaderText="PosY(Stage)", Name="colStageY", ReadOnly=true },
            new System.Windows.Forms.DataGridViewTextBoxColumn(){ HeaderText="보정 Data -PosX", Name="colCalX", ReadOnly=true },
            new System.Windows.Forms.DataGridViewTextBoxColumn(){ HeaderText="보정 Data -PosY", Name="colCalY", ReadOnly=true },
            new System.Windows.Forms.DataGridViewTextBoxColumn(){ HeaderText="보정 Data -PosT", Name="colCalT", ReadOnly=true }});
            this.dgvMark1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvMark1.MultiSelect = false;
            this.dgvMark1.Name = "dgvMark1";
            this.dgvMark1.RowHeadersVisible = false;
            this.dgvMark1.RowHeadersWidth = 62;
            this.dgvMark1.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvMark1.TabIndex = 12;
            // 
            // dgvMark2
            // 
            this.dgvMark2.AllowUserToAddRows = false;
            this.dgvMark2.AllowUserToDeleteRows = false;
            this.dgvMark2.AllowUserToResizeRows = false;
            this.dgvMark2.ColumnHeadersHeight = 34;
            this.dgvMark2.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            new System.Windows.Forms.DataGridViewTextBoxColumn(){ HeaderText="Index", Name="colIndex", ReadOnly=true, Width=60 },
            new System.Windows.Forms.DataGridViewTextBoxColumn(){ HeaderText="각도", Name="colAngle", ReadOnly=true, Width=60 },
            new System.Windows.Forms.DataGridViewTextBoxColumn(){ HeaderText="PosX(Stage)", Name="colStageX", ReadOnly=true },
            new System.Windows.Forms.DataGridViewTextBoxColumn(){ HeaderText="PosY(Stage)", Name="colStageY", ReadOnly=true },
            new System.Windows.Forms.DataGridViewTextBoxColumn(){ HeaderText="보정 Data -PosX", Name="colCalX", ReadOnly=true },
            new System.Windows.Forms.DataGridViewTextBoxColumn(){ HeaderText="보정 Data -PosY", Name="colCalY", ReadOnly=true },
            new System.Windows.Forms.DataGridViewTextBoxColumn(){ HeaderText="보정 Data -PosT", Name="colCalT", ReadOnly=true }});
            this.dgvMark2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvMark2.MultiSelect = false;
            this.dgvMark2.Name = "dgvMark2";
            this.dgvMark2.RowHeadersVisible = false;
            this.dgvMark2.RowHeadersWidth = 62;
            this.dgvMark2.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvMark2.TabIndex = 13;
            // 
            // dgvMark3
            // 
            this.dgvMark3.AllowUserToAddRows = false;
            this.dgvMark3.AllowUserToDeleteRows = false;
            this.dgvMark3.AllowUserToResizeRows = false;
            this.dgvMark3.ColumnHeadersHeight = 34;
            this.dgvMark3.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            new System.Windows.Forms.DataGridViewTextBoxColumn(){ HeaderText="Index", Name="colIndex", ReadOnly=true, Width=60 },
            new System.Windows.Forms.DataGridViewTextBoxColumn(){ HeaderText="각도", Name="colAngle", ReadOnly=true, Width=60 },
            new System.Windows.Forms.DataGridViewTextBoxColumn(){ HeaderText="PosX(Stage)", Name="colStageX", ReadOnly=true },
            new System.Windows.Forms.DataGridViewTextBoxColumn(){ HeaderText="PosY(Stage)", Name="colStageY", ReadOnly=true },
            new System.Windows.Forms.DataGridViewTextBoxColumn(){ HeaderText="보정 Data -PosX", Name="colCalX", ReadOnly=true },
            new System.Windows.Forms.DataGridViewTextBoxColumn(){ HeaderText="보정 Data -PosY", Name="colCalY", ReadOnly=true },
            new System.Windows.Forms.DataGridViewTextBoxColumn(){ HeaderText="보정 Data -PosT", Name="colCalT", ReadOnly=true }});
            this.dgvMark3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvMark3.MultiSelect = false;
            this.dgvMark3.Name = "dgvMark3";
            this.dgvMark3.RowHeadersVisible = false;
            this.dgvMark3.RowHeadersWidth = 62;
            this.dgvMark3.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvMark3.TabIndex = 14;
            // 
            // dgvMark4
            // 
            this.dgvMark4.AllowUserToAddRows = false;
            this.dgvMark4.AllowUserToDeleteRows = false;
            this.dgvMark4.AllowUserToResizeRows = false;
            this.dgvMark4.ColumnHeadersHeight = 34;
            this.dgvMark4.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            new System.Windows.Forms.DataGridViewTextBoxColumn(){ HeaderText="Index", Name="colIndex", ReadOnly=true, Width=60 },
            new System.Windows.Forms.DataGridViewTextBoxColumn(){ HeaderText="각도", Name="colAngle", ReadOnly=true, Width=60 },
            new System.Windows.Forms.DataGridViewTextBoxColumn(){ HeaderText="PosX(Stage)", Name="colStageX", ReadOnly=true },
            new System.Windows.Forms.DataGridViewTextBoxColumn(){ HeaderText="PosY(Stage)", Name="colStageY", ReadOnly=true },
            new System.Windows.Forms.DataGridViewTextBoxColumn(){ HeaderText="보정 Data -PosX", Name="colCalX", ReadOnly=true },
            new System.Windows.Forms.DataGridViewTextBoxColumn(){ HeaderText="보정 Data -PosY", Name="colCalY", ReadOnly=true },
            new System.Windows.Forms.DataGridViewTextBoxColumn(){ HeaderText="보정 Data -PosT", Name="colCalT", ReadOnly=true }});
            this.dgvMark4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvMark4.MultiSelect = false;
            this.dgvMark4.Name = "dgvMark4";
            this.dgvMark4.RowHeadersVisible = false;
            this.dgvMark4.RowHeadersWidth = 62;
            this.dgvMark4.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvMark4.TabIndex = 15;
            // 
            // TCorrectionDialog
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(830, 660);
            this.Controls.Add(this.tabMarks);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.btnLoad);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.grpMarks);
            this.Controls.Add(this.lblAngleStep);
            this.Controls.Add(this.txtAngleStep);
            this.Controls.Add(this.btnStart);
            this.Controls.Add(this.btnStop);
            this.Controls.Add(this.btnCalc);
            this.Controls.Add(this.btnClose);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "TCorrectionDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "T-보정 Dlg";
            ((System.ComponentModel.ISupportInitialize)(this.dgvScan)).EndInit();
            this.grpMarks.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvMarks)).EndInit();
            this.tabMarks.ResumeLayout(false);
            this.tabMatrix.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvMark1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvMark2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvMark3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvMark4)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}