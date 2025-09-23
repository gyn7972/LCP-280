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
    public partial class InputWaferCarrierControl : UserControl
    {
        private InputCassetteLifter CassetteLifter;
        private InputFeeder Feeder;
        private InputStage Stage;

        public InputWaferCarrierControl() : this(
            TryGetUnit<InputCassetteLifter>("InputCassetteLifter"),
            TryGetUnit<InputFeeder>("InputFeeder"),
            TryGetUnit<InputStage>("InputStage")
            )
        {

        }

        public InputWaferCarrierControl(InputCassetteLifter cassetteLifter, InputFeeder ringTransfer, InputStage inputStage)
        {
            InitializeComponent();

            CassetteLifter = cassetteLifter;
            Feeder = ringTransfer;
            Stage = inputStage;

            var materialCassette = CassetteLifter.GetMaterialCassette();
            waferMapView.SetMaterialCassette(materialCassette);
        }

        private static T TryGetUnit<T>(string unitName) where T : class
        {
            try
            {
                var eq = Equipment.Instance;
                if (eq?.Units != null && eq.Units.TryGetValue(unitName, out var u))
                    return u as T;
            }
            catch { }
            return null;
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
