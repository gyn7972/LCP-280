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

        public FormStrainGageDialog(StrainGageMonitor monitor)
        {
            InitializeComponent();
            _monitor = monitor;
        }

        private void FormStrainGageDialog_Load(object sender, EventArgs e)
        {
            strainGageDataGridViewer1.AttachMonitor(_monitor);
            strainGageChart1.AttachMonitor(_monitor);
        }

        private void FormStrainGageDialog_FormClosed(object sender, FormClosedEventArgs e)
        {
            strainGageChart1.DetachMonitor();
            strainGageDataGridViewer1.DetachMonitor();
        }
    }
}
