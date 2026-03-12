namespace QMC.LCP_280.Process.Component
{
    partial class DIOControl
    {
        private void InitializeComponent()
        {
            this.gbDigitalIO = new System.Windows.Forms.GroupBox();
            this.ioTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.inputView = new QMC.Common.IOPropertyCollectionView();
            this.outputView = new QMC.Common.IOPropertyCollectionView();
            this.gbDigitalIO.SuspendLayout();
            this.ioTableLayoutPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // gbDigitalIO
            // 
            this.gbDigitalIO.BackColor = System.Drawing.Color.White;
            this.gbDigitalIO.Controls.Add(this.ioTableLayoutPanel);
            this.gbDigitalIO.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbDigitalIO.Font = new System.Drawing.Font("맑은 고딕", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.gbDigitalIO.Location = new System.Drawing.Point(0, 0);
            this.gbDigitalIO.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.gbDigitalIO.Name = "gbDigitalIO";
            this.gbDigitalIO.Padding = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.gbDigitalIO.Size = new System.Drawing.Size(484, 268);
            this.gbDigitalIO.TabIndex = 10;
            this.gbDigitalIO.TabStop = false;
            this.gbDigitalIO.Text = "Digital I/O (Raw)";
            // 
            // ioTableLayoutPanel
            // 
            this.ioTableLayoutPanel.ColumnCount = 2;
            this.ioTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.ioTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.ioTableLayoutPanel.Controls.Add(this.inputView, 0, 0);
            this.ioTableLayoutPanel.Controls.Add(this.outputView, 1, 0);
            this.ioTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ioTableLayoutPanel.Location = new System.Drawing.Point(2, 21);
            this.ioTableLayoutPanel.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.ioTableLayoutPanel.Name = "ioTableLayoutPanel";
            this.ioTableLayoutPanel.RowCount = 1;
            this.ioTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.ioTableLayoutPanel.Size = new System.Drawing.Size(480, 244);
            this.ioTableLayoutPanel.TabIndex = 2;
            // 
            // inputView
            // 
            this.inputView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.inputView.FastBuild = true;
            this.inputView.FastInitialPaint = true;
            this.inputView.GroupName = "Input";
            this.inputView.ListBackColor = System.Drawing.Color.Black;
            this.inputView.ListForeColor = System.Drawing.Color.Lime;
            this.inputView.Location = new System.Drawing.Point(4, 6);
            this.inputView.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.inputView.Name = "inputView";
            this.inputView.SelectedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(198)))), ((int)(((byte)(255)))), ((int)(((byte)(0)))));
            this.inputView.SelectedForeColor = System.Drawing.Color.Black;
            this.inputView.Size = new System.Drawing.Size(232, 232);
            this.inputView.SuppressResizeInvalidation = true;
            this.inputView.TabIndex = 1;
            this.inputView.Load += new System.EventHandler(this.inputView_Load);
            // 
            // outputView
            // 
            this.outputView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.outputView.FastBuild = true;
            this.outputView.FastInitialPaint = true;
            this.outputView.GroupName = "Output";
            this.outputView.ListBackColor = System.Drawing.Color.Black;
            this.outputView.ListForeColor = System.Drawing.Color.Lime;
            this.outputView.Location = new System.Drawing.Point(244, 6);
            this.outputView.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.outputView.Name = "outputView";
            this.outputView.SelectedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(198)))), ((int)(((byte)(255)))), ((int)(((byte)(0)))));
            this.outputView.SelectedForeColor = System.Drawing.Color.Black;
            this.outputView.Size = new System.Drawing.Size(232, 232);
            this.outputView.SuppressResizeInvalidation = true;
            this.outputView.TabIndex = 1;
            // 
            // DIOControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.Controls.Add(this.gbDigitalIO);
            this.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.Name = "DIOControl";
            this.Size = new System.Drawing.Size(484, 268);
            this.gbDigitalIO.ResumeLayout(false);
            this.ioTableLayoutPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        private System.Windows.Forms.GroupBox gbDigitalIO;
        private System.Windows.Forms.TableLayoutPanel ioTableLayoutPanel;
        private Common.IOPropertyCollectionView inputView;
        private Common.IOPropertyCollectionView outputView;
    }
}