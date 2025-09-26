using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace QMC.Common.StrainGage
{
    public partial class StrainGageChart : UserControl
    {
        private StrainGageMonitor _monitor;
        private Color[] colors = new Color[] { Color.Red, Color.Blue, Color.Green, Color.Orange, Color.Purple, Color.Brown, Color.Cyan, Color.Magenta };

        private const double minYDefault = 0;
        private const double maxYDefault = 4;
        private double minY = minYDefault;
        private double maxY = maxYDefault;

        public StrainGageChart()
        {
            InitializeComponent();
        }

        private void InitializeUI()
        {
            // chart
            chart.Series.Clear();

            for (int i = 0; i < _monitor.Items.Count; i++)
            {
                var item = _monitor.Items[i];
                var series = new Series(item.strainGage.Name);
                series.ChartType = SeriesChartType.FastLine;
                series.ChartArea = chart.ChartAreas[0].Name;
                series.BorderWidth = 1;
                series.Color = colors[i % colors.Length];
                chart.Series.Add(series);
            }

            var chartArea = chart.ChartAreas[0];
            chartArea.AxisX.Minimum = 0;
            chartArea.AxisX.Maximum = (double)nudDataCount.Value;
            UpdateChartAreaYRange();

            chart.Legends[0].Enabled = true;

            // combo box
            cbDisplayItem.Items.Clear();
            foreach (var item in _monitor.Items)
            {
                cbDisplayItem.Items.Add(item.strainGage.Name);
            }
            cbDisplayItem.Items.Add("All");
            cbDisplayItem.SelectedIndex = cbDisplayItem.Items.Count - 1; // Select "All"

            // text box
            tbAxisYMin.Text = minY.ToString();
            tbAxisYMax.Text = maxY.ToString();
        }

        private void Monitor_OnVoltageUpdated(object sender, EventArgs e)
        {
            if (!Visible || _monitor == null)
                return;

            UpdateChart();
        }

        public void AttachMonitor(StrainGageMonitor monitor)
        {
            if (monitor == null)
            {
                throw new ArgumentNullException(nameof(monitor));
            }
            if (_monitor != null)
            {
                _monitor.OnVoltageUpdated -= Monitor_OnVoltageUpdated;
            }

            _monitor = monitor;
            _monitor.OnVoltageUpdated += Monitor_OnVoltageUpdated;

            InitializeUI();
        }

        public void DetachMonitor()
        {
            if (_monitor != null)
            {
                _monitor.OnVoltageUpdated -= Monitor_OnVoltageUpdated;
                _monitor = null;
            }
        }

        private void UpdateChartAreaYRange()
        {
            var chartArea = chart.ChartAreas[0];
            if (cbAutoScale.Checked)
            {
                chartArea.AxisY.Minimum = Double.NaN;
                chartArea.AxisY.Maximum = Double.NaN;
                chartArea.AxisY.Interval = Double.NaN;
            }
            else
            {
                if (minY >= maxY)
                {
                    minY = minYDefault;
                    maxY = maxYDefault;
                }

                chartArea.AxisY.Minimum = minY;
                chartArea.AxisY.Maximum = maxY;
            }
        }

        private void cbDisplayItem_SelectedValueChanged(object sender, EventArgs e)
        {
            lock (chart)
            {
                string selectedItem = cbDisplayItem.SelectedItem as string;
                if (selectedItem == "All")
                {
                    foreach (var series in chart.Series)
                    {
                        series.Enabled = true;
                    }
                }
                else
                {
                    foreach (var series in chart.Series)
                    {
                        series.Enabled = (series.Name == selectedItem);
                    }
                }
            }
        }

        private void nudDataCount_ValueChanged(object sender, EventArgs e)
        {
            lock (chart)
            {
                var chartArea = chart.ChartAreas[0];
                chartArea.AxisX.Minimum = 0;
                chartArea.AxisX.Maximum = (double)nudDataCount.Value;

                int maxCount = (int)nudDataCount.Value;
                foreach (var series in chart.Series)
                {
                    while (series.Points.Count > maxCount)
                    {
                        series.Points.RemoveAt(0);
                    }
                }
            }
        }

        private void UpdateChart()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(UpdateChart));
                return;
            }
            else
            {
                chart.SuspendLayout();
                foreach (var item in _monitor.Items)
                {
                    var gage = item.strainGage;
                    var series = chart.Series[gage.Name];
                    if (series != null)
                    {
                        series.Points.AddY((cbDisplayVoltage.Checked ? gage.Voltage : gage.Force));
                        if (series.Points.Count > (int)nudDataCount.Value)
                        {
                            series.Points.RemoveAt(0);
                        }

                        UpdateChartAreaYRange();
                    }
                }
                chart.ResumeLayout();
            }
        }

        private void btnApplyRange_Click(object sender, EventArgs e)
        {
            double min = 0;
            double max = 0;
            try
            {
                min = double.Parse(tbAxisYMin.Text);
                max = double.Parse(tbAxisYMax.Text);
            }
            catch
            {
                MessageBox.Show("Invalid range value.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (min >= max)
            {
                MessageBox.Show("Min value must be less than Max value.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            minY = min;
            maxY = max;
        }

        private void cbDisplayVoltage_CheckedChanged(object sender, EventArgs e)
        {
            chart.SuspendLayout();
            foreach (var item in _monitor.Items)
            {
                var gage = item.strainGage;
                var series = chart.Series[gage.Name];
                if (series != null)
                {
                    series.Points.Clear();
                }
            }
            chart.ResumeLayout();
        }
    }
}
