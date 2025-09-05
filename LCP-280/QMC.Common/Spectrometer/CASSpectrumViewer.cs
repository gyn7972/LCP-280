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
            Series series = chart.Series[0];

            if (spectrometer == null)
            {
                series.Points.Clear();
                return;
            }

            series.Points.Clear();
            for (int i = 0; i < spectrometer.SpectrumData.WaveLength.Length; i++)
            {
                series.Points.AddXY(spectrometer.SpectrumData.WaveLength[i], spectrometer.SpectrumData.Intensity[i]);
            }
        }
    }
}
