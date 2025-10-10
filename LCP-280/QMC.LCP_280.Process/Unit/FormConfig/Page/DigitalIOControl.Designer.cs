namespace QMC.LCP_280.Process.Unit.FormConfig
{
    partial class DigitalIOControl
    {
        /// <summary> 
        /// 필수 디자이너 변수입니다.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 사용 중인 모든 리소스를 정리합니다.
        /// </summary>
        /// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 구성 요소 디자이너에서 생성한 코드

        /// <summary> 
        /// 디자이너 지원에 필요한 메서드입니다. 
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마세요.
        /// </summary>
        private void InitializeComponent()
        {
            this.ioTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.outputView = new QMC.Common.IOPropertyCollectionView();
            this.inputView = new QMC.Common.IOPropertyCollectionView();
            this.ioTableLayoutPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // ioTableLayoutPanel
            // 
            this.ioTableLayoutPanel.BackColor = System.Drawing.Color.White;
            this.ioTableLayoutPanel.ColumnCount = 2;
            this.ioTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.ioTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.ioTableLayoutPanel.Controls.Add(this.outputView, 1, 0);
            this.ioTableLayoutPanel.Controls.Add(this.inputView, 0, 0);
            this.ioTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ioTableLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this.ioTableLayoutPanel.Name = "ioTableLayoutPanel";
            this.ioTableLayoutPanel.RowCount = 1;
            this.ioTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.ioTableLayoutPanel.Size = new System.Drawing.Size(626, 384);
            this.ioTableLayoutPanel.TabIndex = 3;
            // 
            // outputView
            // 
            this.outputView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.outputView.FastBuild = true;
            this.outputView.FastInitialPaint = true;
            this.outputView.GroupName = "Output";
            this.outputView.ListBackColor = System.Drawing.Color.Black;
            this.outputView.ListForeColor = System.Drawing.Color.Lime;
            this.outputView.Location = new System.Drawing.Point(317, 6);
            this.outputView.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.outputView.Name = "outputView";
            this.outputView.SelectedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(198)))), ((int)(((byte)(255)))), ((int)(((byte)(0)))));
            this.outputView.SelectedForeColor = System.Drawing.Color.Black;
            this.outputView.Size = new System.Drawing.Size(305, 372);
            this.outputView.SuppressResizeInvalidation = true;
            this.outputView.TabIndex = 1;
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
            this.inputView.Size = new System.Drawing.Size(305, 372);
            this.inputView.SuppressResizeInvalidation = true;
            this.inputView.TabIndex = 1;
            // 
            // DigitalIOControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.ioTableLayoutPanel);
            this.Name = "DigitalIOControl";
            this.Size = new System.Drawing.Size(626, 384);
            this.ioTableLayoutPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel ioTableLayoutPanel;
        private Common.IOPropertyCollectionView outputView;
        private Common.IOPropertyCollectionView inputView;
    }
}
