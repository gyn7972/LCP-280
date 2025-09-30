using QMC.Common;
using QMC.Common.Cameras;
using QMC.Common.Cameras.HIKVISION;
using QMC.Common.Vision;
using QMC.LCP_280.Process.Component;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace QMC.LCP_280.Process.Unit
{
    [FormOrder(3)]
    public partial class Vision_Setup : Form
    {
        private readonly Equipment equipment = Equipment.Instance;

        public Vision_Setup()
        {
            InitializeComponent();
        }
    }
}
