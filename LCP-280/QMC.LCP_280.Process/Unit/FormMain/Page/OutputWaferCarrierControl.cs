using QMC.LCP_280.Process.Work;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace QMC.LCP_280.Process.Unit.FormMain
{
    public partial class OutputWaferCarrierControl : UserControl
    {
        public OutputWaferCarrierControl()
        {
            InitializeComponent();
        }

        //public Component.WaferMapView GetWaferMapView()
        //{
        //    return waferMapView;
        //}

        public Component.WaferSelectMapView GetWaferSelectMapView()
        {
            return waferSelectMapView;
        }

        public void SetWaferCarrierId(string id)
        {
            lblWaferIdValue.Text = id;
        }

        public void UpdateWaferCount(int count)
        {
            lblWaferCountValue.Text = count.ToString();
        }
    }
}
