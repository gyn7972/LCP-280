namespace QMC.LCP_280.Process.Unit.FormWork
{
    partial class IndexCalibrationDialog
    {
        private System.ComponentModel.IContainer components = null;
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.tlRoot = new System.Windows.Forms.TableLayoutPanel();
            this.tlLeft = new System.Windows.Forms.TableLayoutPanel();
            this.lblTitle = new System.Windows.Forms.Label();
            this.flLeftBlocks = new System.Windows.Forms.FlowLayoutPanel();
            this.tlRight = new System.Windows.Forms.TableLayoutPanel();
            this.lblAvg = new System.Windows.Forms.Label();
            this.dgvAvg = new System.Windows.Forms.DataGridView();
            this.lblOffset = new System.Windows.Forms.Label();
            this.dgvOffset = new System.Windows.Forms.DataGridView();
            this.tlSpecAndControls = new System.Windows.Forms.TableLayoutPanel();
            this.dgvSpec = new System.Windows.Forms.DataGridView();
            this.tlCountPanel = new System.Windows.Forms.TableLayoutPanel();
            this.lblCount = new System.Windows.Forms.Label();
            this.txtCount = new System.Windows.Forms.TextBox();
            this.btnApply = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.tlButtons = new System.Windows.Forms.TableLayoutPanel();
            this.btnStart = new System.Windows.Forms.Button();
            this.btnStop = new System.Windows.Forms.Button();
            this.dgvCopy = new System.Windows.Forms.DataGridView();
            this.tlRoot.SuspendLayout();
            this.tlLeft.SuspendLayout();
            this.tlRight.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvAvg)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvOffset)).BeginInit();
            this.tlSpecAndControls.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvSpec)).BeginInit();
            this.tlCountPanel.SuspendLayout();
            this.tlButtons.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvCopy)).BeginInit();
            this.SuspendLayout();
            // 
            // tlRoot
            // 
            this.tlRoot.ColumnCount = 2;
            this.tlRoot.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 62F));
            this.tlRoot.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 38F));
            this.tlRoot.Controls.Add(this.tlLeft, 0, 0);
            this.tlRoot.Controls.Add(this.tlRight, 1, 0);
            this.tlRoot.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlRoot.Location = new System.Drawing.Point(0, 0);
            this.tlRoot.Name = "tlRoot";
            this.tlRoot.RowCount = 1;
            this.tlRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlRoot.Size = new System.Drawing.Size(1280, 720);
            this.tlRoot.TabIndex = 0;
            // 
            // tlLeft
            // 
            this.tlLeft.ColumnCount = 1;
            this.tlLeft.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlLeft.Controls.Add(this.lblTitle, 0, 0);
            this.tlLeft.Controls.Add(this.flLeftBlocks, 0, 1);
            this.tlLeft.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlLeft.Location = new System.Drawing.Point(3, 3);
            this.tlLeft.Name = "tlLeft";
            this.tlLeft.RowCount = 2;
            this.tlLeft.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tlLeft.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlLeft.Size = new System.Drawing.Size(787, 714);
            this.tlLeft.TabIndex = 0;
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Dock = System.Windows.Forms.DockStyle.Left;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.lblTitle.Location = new System.Drawing.Point(3, 0);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(197, 30);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "Index Calibration.";
            // 
            // flLeftBlocks
            // 
            this.flLeftBlocks.AutoScroll = true;
            this.flLeftBlocks.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flLeftBlocks.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flLeftBlocks.Location = new System.Drawing.Point(3, 33);
            this.flLeftBlocks.Name = "flLeftBlocks";
            this.flLeftBlocks.Padding = new System.Windows.Forms.Padding(8);
            this.flLeftBlocks.Size = new System.Drawing.Size(781, 678);
            this.flLeftBlocks.TabIndex = 1;
            this.flLeftBlocks.WrapContents = false;
            // 
            // tlRight
            // 
            this.tlRight.ColumnCount = 1;
            this.tlRight.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlRight.Controls.Add(this.lblAvg, 0, 0);
            this.tlRight.Controls.Add(this.dgvAvg, 0, 1);
            this.tlRight.Controls.Add(this.lblOffset, 0, 2);
            this.tlRight.Controls.Add(this.dgvOffset, 0, 3);
            this.tlRight.Controls.Add(this.tlSpecAndControls, 0, 4);
            this.tlRight.Controls.Add(this.dgvCopy, 0, 5);
            this.tlRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlRight.Location = new System.Drawing.Point(796, 3);
            this.tlRight.Name = "tlRight";
            this.tlRight.RowCount = 6;
            this.tlRight.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 22F));
            this.tlRight.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 150F));
            this.tlRight.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 22F));
            this.tlRight.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 150F));
            this.tlRight.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 110F));
            this.tlRight.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlRight.Size = new System.Drawing.Size(481, 714);
            this.tlRight.TabIndex = 1;
            // 
            // lblAvg
            // 
            this.lblAvg.AutoSize = true;
            this.lblAvg.Dock = System.Windows.Forms.DockStyle.Left;
            this.lblAvg.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblAvg.Location = new System.Drawing.Point(3, 0);
            this.lblAvg.Name = "lblAvg";
            this.lblAvg.Size = new System.Drawing.Size(49, 22);
            this.lblAvg.TabIndex = 0;
            this.lblAvg.Text = "AVG";
            // 
            // dgvAvg
            // 
            this.dgvAvg.ColumnHeadersHeight = 34;
            this.dgvAvg.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvAvg.Location = new System.Drawing.Point(3, 25);
            this.dgvAvg.Name = "dgvAvg";
            this.dgvAvg.RowHeadersWidth = 62;
            this.dgvAvg.Size = new System.Drawing.Size(475, 144);
            this.dgvAvg.TabIndex = 1;
            // 
            // lblOffset
            // 
            this.lblOffset.AutoSize = true;
            this.lblOffset.Dock = System.Windows.Forms.DockStyle.Left;
            this.lblOffset.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblOffset.Location = new System.Drawing.Point(3, 172);
            this.lblOffset.Name = "lblOffset";
            this.lblOffset.Size = new System.Drawing.Size(75, 22);
            this.lblOffset.TabIndex = 2;
            this.lblOffset.Text = "OFFSET";
            // 
            // dgvOffset
            // 
            this.dgvOffset.ColumnHeadersHeight = 34;
            this.dgvOffset.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvOffset.Location = new System.Drawing.Point(3, 197);
            this.dgvOffset.Name = "dgvOffset";
            this.dgvOffset.RowHeadersWidth = 62;
            this.dgvOffset.Size = new System.Drawing.Size(475, 144);
            this.dgvOffset.TabIndex = 3;
            // 
            // tlSpecAndControls
            // 
            this.tlSpecAndControls.ColumnCount = 3;
            this.tlSpecAndControls.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 60F));
            this.tlSpecAndControls.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tlSpecAndControls.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tlSpecAndControls.Controls.Add(this.dgvSpec, 0, 0);
            this.tlSpecAndControls.Controls.Add(this.tlCountPanel, 1, 0);
            this.tlSpecAndControls.Controls.Add(this.tlButtons, 2, 0);
            this.tlSpecAndControls.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlSpecAndControls.Location = new System.Drawing.Point(3, 347);
            this.tlSpecAndControls.Name = "tlSpecAndControls";
            this.tlSpecAndControls.RowCount = 1;
            this.tlSpecAndControls.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlSpecAndControls.Size = new System.Drawing.Size(475, 104);
            this.tlSpecAndControls.TabIndex = 4;
            // 
            // dgvSpec
            // 
            this.dgvSpec.ColumnHeadersHeight = 34;
            this.dgvSpec.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvSpec.Location = new System.Drawing.Point(3, 3);
            this.dgvSpec.Name = "dgvSpec";
            this.dgvSpec.RowHeadersWidth = 62;
            this.dgvSpec.Size = new System.Drawing.Size(279, 98);
            this.dgvSpec.TabIndex = 0;
            // 
            // tlCountPanel
            // 
            this.tlCountPanel.ColumnCount = 2;
            this.tlCountPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 56F));
            this.tlCountPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlCountPanel.Controls.Add(this.lblCount, 0, 0);
            this.tlCountPanel.Controls.Add(this.txtCount, 1, 0);
            this.tlCountPanel.Controls.Add(this.btnApply, 0, 1);
            this.tlCountPanel.Controls.Add(this.btnSave, 1, 1);
            this.tlCountPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlCountPanel.Location = new System.Drawing.Point(288, 3);
            this.tlCountPanel.Name = "tlCountPanel";
            this.tlCountPanel.RowCount = 2;
            this.tlCountPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
            this.tlCountPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
            this.tlCountPanel.Size = new System.Drawing.Size(89, 98);
            this.tlCountPanel.TabIndex = 1;
            // 
            // lblCount
            // 
            this.lblCount.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblCount.Location = new System.Drawing.Point(3, 0);
            this.lblCount.Name = "lblCount";
            this.lblCount.Size = new System.Drawing.Size(50, 26);
            this.lblCount.TabIndex = 0;
            this.lblCount.Text = "Count";
            this.lblCount.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtCount
            // 
            this.txtCount.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtCount.Location = new System.Drawing.Point(59, 3);
            this.txtCount.Name = "txtCount";
            this.txtCount.Size = new System.Drawing.Size(27, 28);
            this.txtCount.TabIndex = 1;
            this.txtCount.Text = "5";
            // 
            // btnApply
            // 
            this.btnApply.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnApply.Location = new System.Drawing.Point(3, 29);
            this.btnApply.Name = "btnApply";
            this.btnApply.Size = new System.Drawing.Size(50, 66);
            this.btnApply.TabIndex = 2;
            this.btnApply.Text = "적용";
            // 
            // btnSave
            // 
            this.btnSave.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnSave.Location = new System.Drawing.Point(59, 29);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(27, 66);
            this.btnSave.TabIndex = 3;
            this.btnSave.Text = "저장";
            // 
            // tlButtons
            // 
            this.tlButtons.ColumnCount = 2;
            this.tlButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlButtons.Controls.Add(this.btnStart, 0, 0);
            this.tlButtons.Controls.Add(this.btnStop, 1, 0);
            this.tlButtons.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlButtons.Location = new System.Drawing.Point(383, 3);
            this.tlButtons.Name = "tlButtons";
            this.tlButtons.RowCount = 2;
            this.tlButtons.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 72F));
            this.tlButtons.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 28F));
            this.tlButtons.Size = new System.Drawing.Size(89, 98);
            this.tlButtons.TabIndex = 2;
            // 
            // btnStart
            // 
            this.btnStart.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnStart.Location = new System.Drawing.Point(3, 3);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(38, 66);
            this.btnStart.TabIndex = 0;
            this.btnStart.Text = "Start";
            // 
            // btnStop
            // 
            this.btnStop.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnStop.Location = new System.Drawing.Point(47, 3);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(39, 66);
            this.btnStop.TabIndex = 1;
            this.btnStop.Text = "Stop";
            // 
            // dgvCopy
            // 
            this.dgvCopy.ColumnHeadersHeight = 34;
            this.dgvCopy.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvCopy.Location = new System.Drawing.Point(3, 457);
            this.dgvCopy.Name = "dgvCopy";
            this.dgvCopy.RowHeadersWidth = 62;
            this.dgvCopy.Size = new System.Drawing.Size(475, 254);
            this.dgvCopy.TabIndex = 5;
            // 
            // IndexCalibrationDialog
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(1280, 720);
            this.Controls.Add(this.tlRoot);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "IndexCalibrationDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Index Calibration";
            this.tlRoot.ResumeLayout(false);
            this.tlLeft.ResumeLayout(false);
            this.tlLeft.PerformLayout();
            this.tlRight.ResumeLayout(false);
            this.tlRight.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvAvg)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvOffset)).EndInit();
            this.tlSpecAndControls.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvSpec)).EndInit();
            this.tlCountPanel.ResumeLayout(false);
            this.tlCountPanel.PerformLayout();
            this.tlButtons.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvCopy)).EndInit();
            this.ResumeLayout(false);

        }

        // 컨트롤 필드
        private System.Windows.Forms.TableLayoutPanel tlRoot;
        private System.Windows.Forms.TableLayoutPanel tlLeft;
        private System.Windows.Forms.FlowLayoutPanel flLeftBlocks;
        private System.Windows.Forms.TableLayoutPanel tlRight;
        private System.Windows.Forms.TableLayoutPanel tlSpecAndControls;
        private System.Windows.Forms.TableLayoutPanel tlCountPanel;
        private System.Windows.Forms.TableLayoutPanel tlButtons;

        private System.Windows.Forms.Label lblTitle, lblAvg, lblOffset, lblCount;
        private System.Windows.Forms.TextBox txtCount;
        private System.Windows.Forms.Button btnStart, btnStop, btnApply, btnSave;

        private System.Windows.Forms.DataGridView dgvAvg;
        private System.Windows.Forms.DataGridView dgvOffset;
        private System.Windows.Forms.DataGridView dgvSpec;
        private System.Windows.Forms.DataGridView dgvCopy;
    }
}