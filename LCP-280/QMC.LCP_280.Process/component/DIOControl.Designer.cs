namespace QMC.LCP_280.Process.Component
{
    partial class DIOControl
    {
        private System.ComponentModel.IContainer components = null;

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
            this.gbDigitalIO.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.gbDigitalIO.Name = "gbDigitalIO";
            this.gbDigitalIO.Padding = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.gbDigitalIO.Size = new System.Drawing.Size(605, 335);
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
            this.ioTableLayoutPanel.Location = new System.Drawing.Point(3, 26);
            this.ioTableLayoutPanel.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.ioTableLayoutPanel.Name = "ioTableLayoutPanel";
            this.ioTableLayoutPanel.RowCount = 1;
            this.ioTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.ioTableLayoutPanel.Size = new System.Drawing.Size(591, 298);
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
            this.inputView.Location = new System.Drawing.Point(5, 8);
            this.inputView.Margin = new System.Windows.Forms.Padding(5, 8, 5, 8);
            this.inputView.Name = "inputView";
            this.inputView.SelectedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(198)))), ((int)(((byte)(255)))), ((int)(((byte)(0)))));
            this.inputView.SelectedForeColor = System.Drawing.Color.Black;
            this.inputView.Size = new System.Drawing.Size(285, 282);
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
            this.outputView.Location = new System.Drawing.Point(300, 8);
            this.outputView.Margin = new System.Windows.Forms.Padding(5, 8, 5, 8);
            this.outputView.Name = "outputView";
            this.outputView.SelectedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(198)))), ((int)(((byte)(255)))), ((int)(((byte)(0)))));
            this.outputView.SelectedForeColor = System.Drawing.Color.Black;
            this.outputView.Size = new System.Drawing.Size(286, 282);
            this.outputView.SuppressResizeInvalidation = true;
            this.outputView.TabIndex = 1;
            // 
            // DIOControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(120F, 120F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.Controls.Add(this.gbDigitalIO);
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "DIOControl";
            this.Size = new System.Drawing.Size(605, 335);
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