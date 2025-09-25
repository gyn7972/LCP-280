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

            chart.ChartAreas[0].AxisX.Minimum = 0;
            chart.ChartAreas[0].AxisX.Maximum = (double)nudDataCount.Value;

            if (cbAutoScale.Checked)
            {
                chart.ChartAreas[0].AxisY.Minimum = Double.NaN;
                chart.ChartAreas[0].AxisY.Maximum = Double.NaN;
            }
            else
            {
                chart.ChartAreas[0].AxisY.Minimum = -0.1;
                chart.ChartAreas[0].AxisY.Maximum = 2;
            }

            chart.Legends[0].Enabled = true;

            // combo box
            cbDisplayItem.Items.Clear();
            foreach (var item in _monitor.Items)
            {
                cbDisplayItem.Items.Add(item.strainGage.Name);
            }
            cbDisplayItem.Items.Add("All");
            cbDisplayItem.SelectedIndex = cbDisplayItem.Items.Count - 1; // Select "All"
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
                chart.ChartAreas[0].AxisX.Minimum = 0;
                chart.ChartAreas[0].AxisX.Maximum = (double)nudDataCount.Value;

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

        private void cbAutoScale_CheckedChanged(object sender, EventArgs e)
        {
            lock (chart)
            {
                if (cbAutoScale.Checked)
                {
                    chart.ChartAreas[0].AxisY.Minimum = Double.NaN;
                    chart.ChartAreas[0].AxisY.Maximum = Double.NaN;
                }
                else
                {
                    chart.ChartAreas[0].AxisY.Minimum = -0.1;
                    chart.ChartAreas[0].AxisY.Maximum = 2;
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
                foreach (var item in _monitor.Items)
                {
                    var gage = item.strainGage;
                    var series = chart.Series[gage.Name];
                    if (series != null)
                    {
                        series.Points.AddY(gage.Voltage);
                        if (series.Points.Count > (int)nudDataCount.Value)
                        {
                            series.Points.RemoveAt(0);
                        }

                        if (cbAutoScale.Checked)
                        {
                            chart.ChartAreas[0].RecalculateAxesScale();
                        }
                    }
                }
            }
        }
    }
}
