namespace QMC.Common.Spectrometer
{
    partial class CASSpectrumViewer
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
            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
            this.gbSpectrum = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.chart = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.lbMaxIntensity = new System.Windows.Forms.Label();
            this.gbSpectrum.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chart)).BeginInit();
            this.SuspendLayout();
            // 
            // gbSpectrum
            // 
            this.gbSpectrum.Controls.Add(this.tableLayoutPanel1);
            this.gbSpectrum.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbSpectrum.Font = new System.Drawing.Font("맑은 고딕", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.gbSpectrum.Location = new System.Drawing.Point(0, 0);
            this.gbSpectrum.Name = "gbSpectrum";
            this.gbSpectrum.Padding = new System.Windows.Forms.Padding(10);
            this.gbSpectrum.Size = new System.Drawing.Size(400, 250);
            this.gbSpectrum.TabIndex = 1;
            this.gbSpectrum.TabStop = false;
            this.gbSpectrum.Text = "Spectrum";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.chart, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.lbMaxIntensity, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(10, 28);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(380, 212);
            this.tableLayoutPanel1.TabIndex = 1;
            // 
            // chart
            // 
            chartArea1.AxisY.IsMarksNextToAxis = false;
            chartArea1.AxisY.LabelStyle.Enabled = false;
            chartArea1.Name = "ChartArea1";
            this.chart.ChartAreas.Add(chartArea1);
            this.chart.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chart.Location = new System.Drawing.Point(3, 23);
            this.chart.Name = "chart";
            series1.BorderWidth = 2;
            series1.ChartArea = "ChartArea1";
            series1.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.FastLine;
            series1.Color = System.Drawing.Color.Blue;
            series1.Name = "Spectrum";
            this.chart.Series.Add(series1);
            this.chart.Size = new System.Drawing.Size(374, 186);
            this.chart.TabIndex = 1;
            // 
            // lbMaxIntensity
            // 
            this.lbMaxIntensity.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbMaxIntensity.Location = new System.Drawing.Point(3, 0);
            this.lbMaxIntensity.Name = "lbMaxIntensity";
            this.lbMaxIntensity.Size = new System.Drawing.Size(374, 20);
            this.lbMaxIntensity.TabIndex = 2;
            this.lbMaxIntensity.Text = "Maximum Intensity = 0";
            // 
            // CASSpectrumViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.gbSpectrum);
            this.Name = "CASSpectrumViewer";
            this.Size = new System.Drawing.Size(400, 250);
            this.gbSpectrum.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.chart)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.GroupBox gbSpectrum;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.DataVisualization.Charting.Chart chart;
        private System.Windows.Forms.Label lbMaxIntensity;
    }
}
