using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace QMC.Common.Spectrometer
{
    public partial class CASSpectrumViewer : UserControl
    {
        private CASSpectrometer _spectrometer;
        private double _chartMinX;
        private double _chartMaxX;
        private double _chartIntervalX;

        public CASSpectrumViewer()
        {
            InitializeComponent();
        }

        public void AttachSpectrometer(CASSpectrometer spectrometer)
        {
            if (spectrometer == null)
            {
                throw new ArgumentNullException(nameof(spectrometer));
            }

            if (_spectrometer != null)
            {
                _spectrometer.OnMeasureCompleted -= Spectrometer_OnMeasureCompleted;
            }

            _spectrometer = spectrometer;
            _spectrometer.OnMeasureCompleted += Spectrometer_OnMeasureCompleted;

            _chartMinX = _spectrometer.Config.ColormetricStart;
            _chartMaxX = _spectrometer.Config.ColormetricStop;
            _chartIntervalX = (_chartMaxX - _chartMinX) / 10;
        }

        public void DetachSpectrometer()
        {
            if (_spectrometer != null)
            {
                _spectrometer.OnMeasureCompleted -= Spectrometer_OnMeasureCompleted;
                _spectrometer = null;
            }

            _chartMinX = 0;
            _chartMaxX = 0;
            _chartIntervalX = 0;
        }

        private void Spectrometer_OnMeasureCompleted(object sender)
        {
            if (!Visible)
                return;

            CASSpectrometer spectrometer = sender as CASSpectrometer;
            if (InvokeRequired)
            {
                Invoke(new Action(() => UpdateSpectrumChart(spectrometer)));
            }
            else
            {
                UpdateSpectrumChart(spectrometer);
            }
        }

        private void UpdateSpectrumChart(CASSpectrometer spectrometer)
        {
            var series = chart.Series[0];
            if (spectrometer == null)
            {
                series.Points.Clear();
                return;
            }

            var spectrumData = spectrometer.Spectrum;
            double Intensity = spectrumData.MaximumIntensity;
            double ADC = spectrumData.ADC;

            // 스펙트럼 강도 단위: dpidCalibrationUnit에서 읽어온 spectrometer 내부 필드 사용
            // 예: "W/nm", "µW/nm", "A.U." 등
            lbMaxIntensity.Text = $"Max Intensity = {Intensity:F5}";// [{spectrometer.CalibrationUnit}]";
            //lbMaxIntensity.Text = $"Max Intensity = {Intensity:F5}";// [{spectrometer.DeviceInfo != null ? spectrometer.Config != null ? "" : "" : ""}{GetCalibrationUnit(spectrometer)}]";
            lbADC.Text = $"MaxCount(ADC) = {ADC:F1}";

            var area = chart.ChartAreas[0];
            area.AxisX.Minimum = _chartMinX;
            area.AxisX.Maximum = _chartMaxX;
            area.AxisX.Interval = _chartIntervalX;

            area.AxisY.Minimum = spectrumData.MinimumIntensity;
            area.AxisY.Maximum = spectrumData.MaximumIntensity;
            area.AxisY.IsMarginVisible = false;
            area.AxisY.MajorTickMark.Size = 0;
            area.AxisY.MinorTickMark.Size = 0;

            series.Points.Clear();
            for (int i = 0; i < spectrumData.WaveLength.Length; i++)
            {
                series.Points.AddXY(spectrumData.WaveLength[i], spectrumData.Intensity[i]);
            }
        }
    }
}
