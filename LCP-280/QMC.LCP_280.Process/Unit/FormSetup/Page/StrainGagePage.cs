using QMC.Common;
using QMC.Common.Keithley;
using QMC.Common.StrainGage;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;


namespace QMC.LCP_280.Process.Unit.FormSetup.Page
{
    public partial class StrainGagePage : UserControl
    {
        private Equipment equipment => Equipment.Instance;

        // Strain Gage
        private StrainGage selectGage;
        private PropertyCollection pcConfig;

        // Timer
        private StrainGageVoltageMonitor monitor = new StrainGageVoltageMonitor();

        // Graph
        private const int GraphDisplayDataCount = 100;

        public StrainGagePage()
        {
            InitializeComponent();
            InitializeSourcemeterListView();
            InitializeGrid();
            InitializeChart();

            // monitor
            foreach (var gage in equipment.StrainGages.Values)
            {
                monitor.Add(gage);
            }
            monitor.OnVoltageUpdated += Monitor_OnVoltageUpdated;
        }

        private void Monitor_OnVoltageUpdated(object sender, EventArgs e)
        {
            UpdateGrid();
            UpdateChart();
        }

        private void InitializeSourcemeterListView()
        {
            List<string> names = equipment.StrainGages.Keys.ToList();
            lbivSelect.SetItems(names.ToArray());
        }

        private void lbivSelect_ItemSelected(object sender, int e)
        {
            StrainGage newSelect = equipment.StrainGages[lbivSelect.SelectedItemName];

            selectGage = newSelect;
            DisplaySelectStrainGageInformation(newSelect);
        }

        private void DisplaySelectStrainGageInformation(StrainGage gage)
        {
            if (gage != null)
            {
                pcConfig = gage.Config.GetPropertyCollection();
                pcvConfig.SetProperties(pcConfig);
            }
            else
            {
                //
            }
        }

        private void InitializeGrid()
        {
            // dataGrid가 DataGridView 컨트롤이라고 가정
            dataGrid.Columns.Clear();
            dataGrid.Rows.Clear();

            // 열 추가: voltage, pressure
            dataGrid.Columns.Add("Voltage", "Voltage");
            dataGrid.Columns.Add("Pressure", "Pressure");

            // Equipment의 StrainGages 멤버 이름을 행으로 추가
            foreach (var gageName in equipment.StrainGages.Keys)
            {
                int rowIndex = dataGrid.Rows.Add();
                dataGrid.Rows[rowIndex].HeaderCell.Value = gageName;
                // 초기값은 빈칸으로 둠
                dataGrid.Rows[rowIndex].Cells[0].Value = "";
                dataGrid.Rows[rowIndex].Cells[1].Value = "";
            }

            dataGrid.AllowUserToAddRows = false;
            dataGrid.AllowUserToDeleteRows = false;
            dataGrid.ReadOnly = true;
        }

        private void UpdateGrid()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(UpdateGrid));
                return;
            }
            else
            {
                int rowIndex = 0;
                dataGrid.SuspendLayout();
                foreach (var kv in equipment.StrainGages)
                {
                    var gage = kv.Value;
                    if (rowIndex < dataGrid.Rows.Count)
                    {
                        dataGrid.Rows[rowIndex].Cells[0].Value = gage.Voltage.ToString("F3");
                        dataGrid.Rows[rowIndex].Cells[1].Value = gage.Pressure.ToString("F3");
                    }
                    rowIndex++;
                }
                dataGrid.ResumeLayout();
            }       
        }

        private void InitializeChart()
        {
            chart.Series.Clear();

            var random = new Random();

            foreach (var gageName in equipment.StrainGages.Keys)
            {
                var series = new Series(gageName);
                series.ChartType = SeriesChartType.FastLine;
                series.ChartArea = chart.ChartAreas[0].Name;
                series.BorderWidth = 1;
                series.Color = Color.FromArgb(random.Next(64, 256), random.Next(64, 256), random.Next(64, 256));
                chart.Series.Add(series);
            }

            chart.ChartAreas[0].AxisX.Minimum = 0;
            chart.ChartAreas[0].AxisX.Maximum = GraphDisplayDataCount;
            chart.ChartAreas[0].AxisY.Minimum = -0.1;
            chart.ChartAreas[0].AxisY.Maximum = 2;

            chart.Legends[0].Enabled = true;
        }

        private void UpdateChart()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(UpdateChart));
                return;
            }
            else
            {
                foreach (var gageName in equipment.StrainGages.Keys)
                {
                    var gage = equipment.StrainGages[gageName];
                    var series = chart.Series[gageName];
                    if (series != null)
                    {
                        series.Points.AddY(gage.Voltage);
                        if (series.Points.Count > GraphDisplayDataCount)
                        {
                            series.Points.RemoveAt(0);
                        }
                    }
                }
            }          
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            monitor.Start();
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            monitor.Stop();
        }

        private void individualMenuButton1_Click(object sender, EventArgs e)
        {
            List<NIDAQResource> list = NIDAQResourceManager.FindAll();
            
            StringBuilder sb = new StringBuilder();
            foreach (var resource in list)
            {
                sb.AppendLine($"Dev: {resource.DeviceName}");
                foreach (var aiChannel in resource.AIPhysicalChannels)
                    sb.AppendLine($"ai: {aiChannel}");
                foreach (var aoChannel in resource.AOPhysicalChannels)
                    sb.AppendLine($"ao: {aoChannel}");
                sb.AppendLine("");
            }

            MessageBox.Show(sb.ToString());
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            pcvConfig.Apply();

            selectGage.Config.ApplyValueFromPropertyCollection(pcConfig);
            selectGage.Config.Save();
        }
    }
}
