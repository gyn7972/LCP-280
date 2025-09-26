using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QMC.Common.StrainGage
{
    public partial class StrainGageDataGridViewer : UserControl
    {
        private StrainGageMonitor _monitor;

        public StrainGageDataGridViewer()
        {
            InitializeComponent();
        }

        private void Monitor_OnVoltageUpdated(object sender, EventArgs e)
        {
            if (!Visible || _monitor == null)
                return;

            UpdateGrid();
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

            InitializeGrid();
        }

        public void DetachMonitor()
        {
            if (_monitor != null)
            {
                _monitor.OnVoltageUpdated -= Monitor_OnVoltageUpdated;
                _monitor = null;
            }
        }

        private void InitializeGrid()
        {
            dataGrid.Rows.Clear();

            for (int i = 0; i < _monitor.Items.Count; i++)
            {
                var gage = _monitor.Items[i].strainGage;
                int rowIndex = dataGrid.Rows.Add();
                dataGrid.Rows[rowIndex].HeaderCell.Value = gage.Name;

                dataGrid.Rows[i].Cells[0].Value = "";
                dataGrid.Rows[i].Cells[1].Value = "";
                dataGrid.Rows[i].Cells[2].Value = "";
            }
        }

        private void UpdateGrid()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(UpdateGrid));
                return;
            }
            else
            {
                dataGrid.SuspendLayout();
                for (int i = 0; i < _monitor.Items.Count; i++)
                {
                    var gage = _monitor.Items[i].strainGage;
                    if (i < dataGrid.Rows.Count)
                    {
                        dataGrid.Rows[i].Cells[0].Value = gage.Voltage.ToString("F3");
                        dataGrid.Rows[i].Cells[1].Value = gage.ZeroVoltage.ToString("F3");
                        dataGrid.Rows[i].Cells[2].Value = gage.Force.ToString("F3");
                    }
                }
                dataGrid.ResumeLayout();
            }
        }
    }
}
