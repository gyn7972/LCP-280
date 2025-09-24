namespace QMC.LCP_280.Process.Unit.FormSetup.Page
{
    partial class StrainGagePage
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
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.lbivSelect = new QMC.Common.ListBoxItemsView();
            this.pcvConfig = new QMC.Common.PropertyCollectionView();
            this.panel1 = new System.Windows.Forms.Panel();
            this.chart = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.btnStop = new QMC.Common.IndividualMenuButton();
            this.btnStart = new QMC.Common.IndividualMenuButton();
            this.dataGrid = new System.Windows.Forms.DataGridView();
            this.individualMenuButton1 = new QMC.Common.IndividualMenuButton();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chart)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGrid)).BeginInit();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 29.52F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 70.48F));
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.panel1, 1, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1250, 700);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 1;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.Controls.Add(this.lbivSelect, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.pcvConfig, 0, 1);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 2;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(363, 694);
            this.tableLayoutPanel2.TabIndex = 0;
            // 
            // lbivSelect
            // 
            this.lbivSelect.BorderColor = System.Drawing.Color.White;
            this.lbivSelect.BorderWidth = 1;
            this.lbivSelect.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbivSelect.GroupBackColor = System.Drawing.Color.White;
            this.lbivSelect.GroupForeColor = System.Drawing.Color.Black;
            this.lbivSelect.GroupName = "Select Item";
            this.lbivSelect.ItemBackColor = System.Drawing.Color.Black;
            this.lbivSelect.ItemForeColor = System.Drawing.Color.Lime;
            this.lbivSelect.ListBackColor = System.Drawing.Color.Black;
            this.lbivSelect.ListForeColor = System.Drawing.Color.Lime;
            this.lbivSelect.Location = new System.Drawing.Point(3, 3);
            this.lbivSelect.Name = "lbivSelect";
            this.lbivSelect.SelectedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(198)))), ((int)(((byte)(255)))), ((int)(((byte)(0)))));
            this.lbivSelect.SelectedForeColor = System.Drawing.Color.Black;
            this.lbivSelect.SelectedIndex = -1;
            this.lbivSelect.Size = new System.Drawing.Size(357, 341);
            this.lbivSelect.TabIndex = 0;
            this.lbivSelect.ItemSelected += new System.EventHandler<int>(this.lbivSelect_ItemSelected);
            // 
            // pcvConfig
            // 
            this.pcvConfig.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pcvConfig.FastBuild = true;
            this.pcvConfig.GroupName = "Property";
            this.pcvConfig.Location = new System.Drawing.Point(3, 350);
            this.pcvConfig.Name = "pcvConfig";
            this.pcvConfig.Size = new System.Drawing.Size(357, 341);
            this.pcvConfig.SuppressResizeInvalidation = true;
            this.pcvConfig.TabIndex = 1;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.individualMenuButton1);
            this.panel1.Controls.Add(this.chart);
            this.panel1.Controls.Add(this.btnStop);
            this.panel1.Controls.Add(this.btnStart);
            this.panel1.Controls.Add(this.dataGrid);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(372, 3);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(875, 694);
            this.panel1.TabIndex = 1;
            // 
            // chart
            // 
            chartArea1.Name = "ChartArea1";
            this.chart.ChartAreas.Add(chartArea1);
            legend1.Name = "Legend1";
            this.chart.Legends.Add(legend1);
            this.chart.Location = new System.Drawing.Point(166, 367);
            this.chart.Name = "chart";
            this.chart.Size = new System.Drawing.Size(621, 249);
            this.chart.TabIndex = 27;
            // 
            // btnStop
            // 
            this.btnStop.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnStop.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnStop.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnStop.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnStop.CustomForeColor = System.Drawing.Color.Black;
            this.btnStop.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnStop.ForeColor = System.Drawing.Color.Black;
            this.btnStop.ImageSize = new System.Drawing.Size(45, 45);
            this.btnStop.Location = new System.Drawing.Point(13, 408);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(132, 43);
            this.btnStop.TabIndex = 26;
            this.btnStop.TabStop = false;
            this.btnStop.Text = "Stop";
            this.btnStop.UseVisualStyleBackColor = false;
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            // 
            // btnStart
            // 
            this.btnStart.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnStart.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnStart.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnStart.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnStart.CustomForeColor = System.Drawing.Color.Black;
            this.btnStart.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnStart.ForeColor = System.Drawing.Color.Black;
            this.btnStart.ImageSize = new System.Drawing.Size(45, 45);
            this.btnStart.Location = new System.Drawing.Point(13, 350);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(132, 43);
            this.btnStart.TabIndex = 25;
            this.btnStart.TabStop = false;
            this.btnStart.Text = "Start";
            this.btnStart.UseVisualStyleBackColor = false;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // dataGrid
            // 
            this.dataGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGrid.Location = new System.Drawing.Point(13, 20);
            this.dataGrid.Name = "dataGrid";
            this.dataGrid.RowTemplate.Height = 23;
            this.dataGrid.Size = new System.Drawing.Size(607, 314);
            this.dataGrid.TabIndex = 0;
            // 
            // individualMenuButton1
            // 
            this.individualMenuButton1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.individualMenuButton1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.individualMenuButton1.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.individualMenuButton1.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.individualMenuButton1.CustomForeColor = System.Drawing.Color.Black;
            this.individualMenuButton1.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.individualMenuButton1.ForeColor = System.Drawing.Color.Black;
            this.individualMenuButton1.ImageSize = new System.Drawing.Size(45, 45);
            this.individualMenuButton1.Location = new System.Drawing.Point(28, 503);
            this.individualMenuButton1.Name = "individualMenuButton1";
            this.individualMenuButton1.Size = new System.Drawing.Size(132, 43);
            this.individualMenuButton1.TabIndex = 28;
            this.individualMenuButton1.TabStop = false;
            this.individualMenuButton1.Text = "Stop";
            this.individualMenuButton1.UseVisualStyleBackColor = false;
            this.individualMenuButton1.Click += new System.EventHandler(this.individualMenuButton1_Click);
            // 
            // StrainGagePage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "StrainGagePage";
            this.Size = new System.Drawing.Size(1250, 700);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.chart)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGrid)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private Common.ListBoxItemsView lbivSelect;
        private Common.PropertyCollectionView pcvConfig;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.DataGridView dataGrid;
        private Common.IndividualMenuButton btnStop;
        private Common.IndividualMenuButton btnStart;
        private System.Windows.Forms.DataVisualization.Charting.Chart chart;
        private Common.IndividualMenuButton individualMenuButton1;
    }
}
