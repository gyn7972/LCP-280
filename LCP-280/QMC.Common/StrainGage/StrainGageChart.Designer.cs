namespace QMC.Common.StrainGage
{
    partial class StrainGageChart
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
            this.chart = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.tableLayoutPanel5 = new System.Windows.Forms.TableLayoutPanel();
            this.nudDataCount = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.cbDisplayItem = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.cbAutoScale = new System.Windows.Forms.CheckBox();
            this.btnApplyRange = new QMC.Common.IndividualMenuButton();
            this.tbAxisYMax = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.tbAxisYMin = new System.Windows.Forms.TextBox();
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            this.cbDisplayVoltage = new System.Windows.Forms.CheckBox();
            this.groupBox1.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chart)).BeginInit();
            this.tableLayoutPanel5.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudDataCount)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.tableLayoutPanel1);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Font = new System.Drawing.Font("맑은 고딕", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(7);
            this.groupBox1.Size = new System.Drawing.Size(652, 423);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Strain Gage Chart";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel4, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(7, 25);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 391F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 391F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(638, 391);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // tableLayoutPanel4
            // 
            this.tableLayoutPanel4.ColumnCount = 2;
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 150F));
            this.tableLayoutPanel4.Controls.Add(this.chart, 0, 0);
            this.tableLayoutPanel4.Controls.Add(this.tableLayoutPanel5, 1, 0);
            this.tableLayoutPanel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel4.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel4.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel4.Name = "tableLayoutPanel4";
            this.tableLayoutPanel4.RowCount = 1;
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel4.Size = new System.Drawing.Size(638, 391);
            this.tableLayoutPanel4.TabIndex = 32;
            // 
            // chart
            // 
            chartArea1.Name = "ChartArea1";
            this.chart.ChartAreas.Add(chartArea1);
            this.chart.Dock = System.Windows.Forms.DockStyle.Fill;
            legend1.Name = "Legend1";
            this.chart.Legends.Add(legend1);
            this.chart.Location = new System.Drawing.Point(3, 3);
            this.chart.Name = "chart";
            this.chart.Size = new System.Drawing.Size(482, 385);
            this.chart.TabIndex = 33;
            // 
            // tableLayoutPanel5
            // 
            this.tableLayoutPanel5.ColumnCount = 1;
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel5.Controls.Add(this.cbDisplayVoltage, 0, 10);
            this.tableLayoutPanel5.Controls.Add(this.nudDataCount, 0, 3);
            this.tableLayoutPanel5.Controls.Add(this.label2, 0, 2);
            this.tableLayoutPanel5.Controls.Add(this.cbDisplayItem, 0, 1);
            this.tableLayoutPanel5.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel5.Controls.Add(this.cbAutoScale, 0, 9);
            this.tableLayoutPanel5.Controls.Add(this.btnApplyRange, 0, 8);
            this.tableLayoutPanel5.Controls.Add(this.tbAxisYMax, 0, 7);
            this.tableLayoutPanel5.Controls.Add(this.label4, 0, 6);
            this.tableLayoutPanel5.Controls.Add(this.label3, 0, 4);
            this.tableLayoutPanel5.Controls.Add(this.tbAxisYMin, 0, 5);
            this.tableLayoutPanel5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel5.Location = new System.Drawing.Point(491, 0);
            this.tableLayoutPanel5.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.tableLayoutPanel5.Name = "tableLayoutPanel5";
            this.tableLayoutPanel5.RowCount = 12;
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel5.Size = new System.Drawing.Size(144, 391);
            this.tableLayoutPanel5.TabIndex = 34;
            // 
            // nudDataCount
            // 
            this.nudDataCount.Dock = System.Windows.Forms.DockStyle.Fill;
            this.nudDataCount.Location = new System.Drawing.Point(3, 73);
            this.nudDataCount.Maximum = new decimal(new int[] {
            500,
            0,
            0,
            0});
            this.nudDataCount.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.nudDataCount.Name = "nudDataCount";
            this.nudDataCount.Size = new System.Drawing.Size(138, 25);
            this.nudDataCount.TabIndex = 34;
            this.nudDataCount.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.nudDataCount.ValueChanged += new System.EventHandler(this.nudDataCount_ValueChanged);
            // 
            // label2
            // 
            this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label2.Location = new System.Drawing.Point(3, 50);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(138, 20);
            this.label2.TabIndex = 33;
            this.label2.Text = "Data Count";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // cbDisplayItem
            // 
            this.cbDisplayItem.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cbDisplayItem.FormattingEnabled = true;
            this.cbDisplayItem.Location = new System.Drawing.Point(0, 20);
            this.cbDisplayItem.Margin = new System.Windows.Forms.Padding(0);
            this.cbDisplayItem.Name = "cbDisplayItem";
            this.cbDisplayItem.Size = new System.Drawing.Size(144, 25);
            this.cbDisplayItem.TabIndex = 32;
            this.cbDisplayItem.SelectedValueChanged += new System.EventHandler(this.cbDisplayItem_SelectedValueChanged);
            // 
            // label1
            // 
            this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label1.Location = new System.Drawing.Point(3, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(138, 20);
            this.label1.TabIndex = 31;
            this.label1.Text = "Display Item";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // cbAutoScale
            // 
            this.cbAutoScale.AutoSize = true;
            this.cbAutoScale.Dock = System.Windows.Forms.DockStyle.Right;
            this.cbAutoScale.Location = new System.Drawing.Point(50, 238);
            this.cbAutoScale.Name = "cbAutoScale";
            this.cbAutoScale.Size = new System.Drawing.Size(91, 24);
            this.cbAutoScale.TabIndex = 30;
            this.cbAutoScale.Text = "Auto Scale";
            this.cbAutoScale.UseVisualStyleBackColor = true;
            // 
            // btnApplyRange
            // 
            this.btnApplyRange.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnApplyRange.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnApplyRange.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnApplyRange.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnApplyRange.CustomForeColor = System.Drawing.Color.Black;
            this.btnApplyRange.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnApplyRange.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnApplyRange.ForeColor = System.Drawing.Color.Black;
            this.btnApplyRange.ImageSize = new System.Drawing.Size(45, 45);
            this.btnApplyRange.Location = new System.Drawing.Point(3, 203);
            this.btnApplyRange.Name = "btnApplyRange";
            this.btnApplyRange.Size = new System.Drawing.Size(138, 29);
            this.btnApplyRange.TabIndex = 29;
            this.btnApplyRange.TabStop = false;
            this.btnApplyRange.Text = "Apply Range";
            this.btnApplyRange.UseVisualStyleBackColor = false;
            this.btnApplyRange.Click += new System.EventHandler(this.btnApplyRange_Click);
            // 
            // tbAxisYMax
            // 
            this.tbAxisYMax.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbAxisYMax.Location = new System.Drawing.Point(3, 173);
            this.tbAxisYMax.Name = "tbAxisYMax";
            this.tbAxisYMax.Size = new System.Drawing.Size(138, 25);
            this.tbAxisYMax.TabIndex = 7;
            // 
            // label4
            // 
            this.label4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label4.Location = new System.Drawing.Point(3, 150);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(138, 20);
            this.label4.TabIndex = 4;
            this.label4.Text = "Y Axis Max";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label3
            // 
            this.label3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label3.Location = new System.Drawing.Point(3, 100);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(138, 20);
            this.label3.TabIndex = 2;
            this.label3.Text = "Y Axis Min";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tbAxisYMin
            // 
            this.tbAxisYMin.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbAxisYMin.Location = new System.Drawing.Point(3, 123);
            this.tbAxisYMin.Name = "tbAxisYMin";
            this.tbAxisYMin.Size = new System.Drawing.Size(138, 25);
            this.tbAxisYMin.TabIndex = 6;
            // 
            // cbDisplayVoltage
            // 
            this.cbDisplayVoltage.AutoSize = true;
            this.cbDisplayVoltage.Dock = System.Windows.Forms.DockStyle.Right;
            this.cbDisplayVoltage.Location = new System.Drawing.Point(22, 268);
            this.cbDisplayVoltage.Name = "cbDisplayVoltage";
            this.cbDisplayVoltage.Size = new System.Drawing.Size(119, 24);
            this.cbDisplayVoltage.TabIndex = 35;
            this.cbDisplayVoltage.Text = "Display Voltage";
            this.cbDisplayVoltage.UseVisualStyleBackColor = true;
            this.cbDisplayVoltage.CheckedChanged += new System.EventHandler(this.cbDisplayVoltage_CheckedChanged);
            // 
            // StrainGageChart
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.Controls.Add(this.groupBox1);
            this.Name = "StrainGageChart";
            this.Size = new System.Drawing.Size(652, 423);
            this.groupBox1.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel4.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.chart)).EndInit();
            this.tableLayoutPanel5.ResumeLayout(false);
            this.tableLayoutPanel5.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudDataCount)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel4;
        private System.Windows.Forms.DataVisualization.Charting.Chart chart;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox tbAxisYMax;
        private System.Windows.Forms.TextBox tbAxisYMin;
        private System.Windows.Forms.CheckBox cbAutoScale;
        private IndividualMenuButton btnApplyRange;
        private System.Windows.Forms.NumericUpDown nudDataCount;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cbDisplayItem;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox cbDisplayVoltage;
    }
}
