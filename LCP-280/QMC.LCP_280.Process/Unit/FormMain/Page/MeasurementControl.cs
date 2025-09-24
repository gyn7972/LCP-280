using QMC.Common.Controls;   // 공용 DisplayView 사용
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace QMC.LCP_280.Process.Unit.FormMain
{
    public partial class MeasurementControl : UserControl
    {
        private Timer updateTimer;
        private Random random = new Random(); // 테스트용 데이터

        public MeasurementControl()
        {
            InitializeComponent();
            InitializeChart();
            StartDataSimulation(); // 테스트용
        }
        private void InitializeChart()
        {
            // 차트 직접 설정
            Chart chart = spectrumViewer.Controls.OfType<GroupBox>().First()
                                        .Controls.OfType<Chart>().First();

            // 차트 기본 설정
            chart.ChartAreas[0].AxisX.Title = "Time";
            chart.ChartAreas[0].AxisY.Title = "Value";
            chart.ChartAreas[0].AxisX.MajorGrid.Enabled = true;
            chart.ChartAreas[0].AxisY.MajorGrid.Enabled = true;

            // X축 시간 형식 설정
            chart.ChartAreas[0].AxisX.LabelStyle.Format = "HH:mm:ss";
            chart.ChartAreas[0].AxisX.IntervalType = DateTimeIntervalType.Seconds;
            chart.ChartAreas[0].AxisX.Interval = 5; // 5초 간격으로 라벨 표시

            // 시리즈 설정
            chart.Series[0].ChartType = SeriesChartType.Line;
            chart.Series[0].BorderWidth = 2;
            chart.Series[0].Color = Color.Blue;
            chart.Series[0].XValueType = ChartValueType.DateTime; // 중요: DateTime 타입 설정
        }

        private void StartDataSimulation()
        {
            updateTimer = new Timer();
            updateTimer.Interval = 100; // 100ms마다 업데이트
            updateTimer.Tick += UpdateTimer_Tick;
            updateTimer.Start();
        }

        private void UpdateTimer_Tick(object sender, EventArgs e)
        {
            // 실시간 데이터 시뮬레이션
            double currentValue = Math.Sin(DateTime.Now.Millisecond * 0.01) * 100 +
                                  random.NextDouble() * 20;

            // 상단 데이터 값 업데이트
            lblChartDataValue.Text = currentValue.ToString("F4");

            // 차트 업데이트
            UpdateChart(currentValue);
        }

        private void UpdateChart(double value)
        {
            Chart chart = spectrumViewer.Controls.OfType<GroupBox>().First()
                                        .Controls.OfType<Chart>().First();

            Series series = chart.Series[0];

            // 새 데이터 포인트 추가
            series.Points.AddXY(DateTime.Now, value); // DateTime.Now 직접 사용

            // 포인트 개수 제한 (최근 50개만 유지)
            if (series.Points.Count > 50)
            {
                series.Points.RemoveAt(0);
            }

            // X축 범위 자동 조정 - 시간 형식으로
            if (series.Points.Count > 0)
            {
                DateTime minTime = DateTime.FromOADate(series.Points[0].XValue);
                DateTime maxTime = DateTime.Now;

                chart.ChartAreas[0].AxisX.Minimum = minTime.ToOADate();
                chart.ChartAreas[0].AxisX.Maximum = maxTime.ToOADate();
            }
        }
    }
}