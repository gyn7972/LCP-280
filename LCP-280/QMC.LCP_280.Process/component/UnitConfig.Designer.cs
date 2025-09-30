namespace QMC.LCP_280.Process.Component
{
    partial class UnitConfig
    {
        /// <summary>
        /// 디자이너에서 필요한 변수
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        private QMC.Common.PropertyCollectionView configurationPropertyView;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelMain;

        /// <summary>
        /// 리소스 정리
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 디자이너 생성 코드
        private void InitializeComponent()
        {
            this.tableLayoutPanelMain = new System.Windows.Forms.TableLayoutPanel();
            this.configurationPropertyView = new QMC.Common.PropertyCollectionView();
            this.btnApplyConfig = new QMC.Common.IndividualMenuButton();
            this.btnReloadConfig = new QMC.Common.IndividualMenuButton();
            this.tableLayoutPanelMain.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanelMain
            // 
            this.tableLayoutPanelMain.ColumnCount = 2;
            this.tableLayoutPanelMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanelMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanelMain.Controls.Add(this.configurationPropertyView, 0, 0);
            this.tableLayoutPanelMain.Controls.Add(this.btnApplyConfig, 0, 1);
            this.tableLayoutPanelMain.Controls.Add(this.btnReloadConfig, 1, 1);
            this.tableLayoutPanelMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanelMain.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanelMain.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanelMain.Name = "tableLayoutPanelMain";
            this.tableLayoutPanelMain.RowCount = 2;
            this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 42F));
            this.tableLayoutPanelMain.Size = new System.Drawing.Size(400, 760);
            this.tableLayoutPanelMain.TabIndex = 0;
            // 
            // configurationPropertyView
            // 
            this.tableLayoutPanelMain.SetColumnSpan(this.configurationPropertyView, 2);
            this.configurationPropertyView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.configurationPropertyView.FastBuild = true;
            this.configurationPropertyView.GroupName = "Unit Config";
            this.configurationPropertyView.Location = new System.Drawing.Point(3, 3);
            this.configurationPropertyView.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
            this.configurationPropertyView.Name = "configurationPropertyView";
            this.configurationPropertyView.Size = new System.Drawing.Size(394, 715);
            this.configurationPropertyView.SuppressResizeInvalidation = true;
            this.configurationPropertyView.TabIndex = 0;
            // 
            // btnApplyConfig
            // 
            this.btnApplyConfig.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnApplyConfig.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnApplyConfig.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnApplyConfig.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnApplyConfig.CustomForeColor = System.Drawing.Color.Black;
            this.btnApplyConfig.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnApplyConfig.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnApplyConfig.ForeColor = System.Drawing.Color.Black;
            this.btnApplyConfig.ImageSize = new System.Drawing.Size(45, 45);
            this.btnApplyConfig.Location = new System.Drawing.Point(3, 721);
            this.btnApplyConfig.Name = "btnApplyConfig";
            this.btnApplyConfig.Size = new System.Drawing.Size(194, 36);
            this.btnApplyConfig.TabIndex = 4;
            this.btnApplyConfig.TabStop = false;
            this.btnApplyConfig.Text = "Apply && Save";
            this.btnApplyConfig.UseVisualStyleBackColor = false;
            // 
            // btnReloadConfig
            // 
            this.btnReloadConfig.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnReloadConfig.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnReloadConfig.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnReloadConfig.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnReloadConfig.CustomForeColor = System.Drawing.Color.Black;
            this.btnReloadConfig.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnReloadConfig.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnReloadConfig.ForeColor = System.Drawing.Color.Black;
            this.btnReloadConfig.ImageSize = new System.Drawing.Size(45, 45);
            this.btnReloadConfig.Location = new System.Drawing.Point(203, 721);
            this.btnReloadConfig.Name = "btnReloadConfig";
            this.btnReloadConfig.Size = new System.Drawing.Size(194, 36);
            this.btnReloadConfig.TabIndex = 5;
            this.btnReloadConfig.TabStop = false;
            this.btnReloadConfig.Text = "Reload";
            this.btnReloadConfig.UseVisualStyleBackColor = false;
            // 
            // UnitConfig
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.tableLayoutPanelMain);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "UnitConfig";
            this.Size = new System.Drawing.Size(400, 760);
            this.tableLayoutPanelMain.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        #endregion

        private Common.IndividualMenuButton btnApplyConfig;
        private Common.IndividualMenuButton btnReloadConfig;
    }
}