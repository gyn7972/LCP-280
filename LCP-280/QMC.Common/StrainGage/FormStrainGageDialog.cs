using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QMC.Common.StrainGage
{
    public partial class FormStrainGageDialog : Form
    {
        private StrainGageMonitor _monitor = null;
        private int _selectedIndex = 0;

        public FormStrainGageDialog(StrainGageMonitor monitor)
        {
            InitializeComponent();

            _monitor = monitor;

            this.tabControl.SelectedIndexChanged += new System.EventHandler(this.TabControl_SelectedIndexChanged);
        }

        private void FormStrainGageDialog_Load(object sender, EventArgs e)
        {
            strainGageDataGridViewer1.AttachMonitor(_monitor);
            strainGageChart1.AttachMonitor(_monitor);
            strainGageChart2.AttachMonitor2(_monitor);
        }

        private void FormStrainGageDialog_FormClosed(object sender, FormClosedEventArgs e)
        {
            strainGageDataGridViewer1.DetachMonitor();
            strainGageChart1.DetachMonitor();
            strainGageChart2.DetachMonitor();
        }

        private void TabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl.SelectedTab == tabPage1)
            {
                // Voltage 탭 처리
                strainGageDataGridViewer1.AttachMonitor(_monitor);
                strainGageChart1.AttachMonitor(_monitor);

                
                Log.Write("StrainGage", "Voltage tab selected");
            }
            else if (tabControl.SelectedTab == tabPage2)
            {
                // Force 탭 처리
                strainGageDataGridViewer1.AttachMonitor(_monitor);
                strainGageChart2.AttachMonitor2(_monitor);

                Log.Write("StrainGage", "Force tab selected");
            }
        }
    }
}
