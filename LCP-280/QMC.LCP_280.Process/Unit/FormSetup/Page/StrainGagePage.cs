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
        private StrainGageMonitor monitor = new StrainGageMonitor();

        // Graph
        private const int GraphDisplayDataCount = 100;

        public StrainGagePage()
        {
            InitializeComponent();
            InitializeSourcemeterListView();

            // monitor
            foreach (var gage in equipment.StrainGages.Values)
            {
                monitor.Add(gage);
            }
            strainGageDataGridViewer1.AttachMonitor(monitor);
            strainGageChart1.AttachMonitor(monitor);
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
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            monitor.Start();
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            monitor.Stop();
        }

        private void btnDeviceInfo_Click(object sender, EventArgs e)
        {
            List<NIDAQResource> list = NIDAQResourceManager.FindAll();
            
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Found {list.Count} NI-DAQ devices");

            int devIndex = 1;
            foreach (var resource in list)
            {
                sb.AppendLine($"[{devIndex}] Dev: {resource.DeviceName}");
                foreach (var aiChannel in resource.AIPhysicalChannels)
                    sb.AppendLine($"ai: {aiChannel}");
                foreach (var aoChannel in resource.AOPhysicalChannels)
                    sb.AppendLine($"ao: {aoChannel}");
                sb.AppendLine("");
                devIndex++;
            }

            MessageBox.Show(sb.ToString());
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            pcvConfig.Apply();

            selectGage.Config.ApplyValueFromPropertyCollection(pcConfig);
            selectGage.Config.Save();
            selectGage.Initialize();
        }
    }
}
