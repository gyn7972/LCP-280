using Ivi.Visa;
using QMC.Common.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QMC.Common.Keithley
{
    public partial class FormSetupKeithleyInstrument : Form
    {
        private KeithleySourcemeter sourcemeter;
        private KeithelySourcemeterConfig config;
        private string selectResourceName = "";

        public FormSetupKeithleyInstrument(KeithleySourcemeter sourcemeter, KeithelySourcemeterConfig config)
        {
            this.sourcemeter = sourcemeter;
            this.config = config;

            InitializeComponent();
            InitializeUI();
        }

        private void InitializeUI()
        {
            this.Text = $"{sourcemeter.Name} Resource Manager";

            comboBoxInterface.Items.Clear();
            comboBoxInterface.Items.Add("All");
            foreach (HardwareInterfaceType type in Enum.GetValues(typeof(HardwareInterfaceType)))
            {
                if (type == HardwareInterfaceType.Custom)
                    continue;
                comboBoxInterface.Items.Add(type.ToString());
            }

            labelResult.Text = " - ";
            listBoxResource.Items.Clear();
        }

        private void buttonSearch_Click(object sender, EventArgs e)
        {
            listBoxResource.Items.Clear();
            string selectItem = comboBoxInterface.SelectedItem as string;
            if (string.IsNullOrEmpty(selectItem))
            {
                MessageBox.Show("Please select an interface type.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (selectItem == "All")
            {
                // Find all resources
                List<KeithleyInstrumentResource> resources = KeithleyInstrumentResourceManager.FindAllInstrumentResources();
                foreach (var res in resources)
                {
                    listBoxResource.Items.Add(res.Name);
                }

                labelResult.Text = $"Found {resources.Count} resources.";
            }
            else if(Enum.TryParse(selectItem, out HardwareInterfaceType interfaceType))
            {
                // Find resources by selected interface type
                List<KeithleyInstrumentResource> resources = KeithleyInstrumentResourceManager.FindAllInstrumentResources(interfaceType);
                foreach (var res in resources)
                {
                    listBoxResource.Items.Add(res.Name);
                }

                labelResult.Text = $"Found {resources.Count} resources.";
            }
            else
            {
                labelResult.Text = $"Invalid Item: {selectItem}";
            }
        }

        private void listBoxResource_SelectedIndexChanged(object sender, EventArgs e)
        {
            selectResourceName = listBoxResource.SelectedItem as string;
        }

        private void buttonApply_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(selectResourceName))
            {
                MessageBox.Show("Please select a resource.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (MessageBox.Show($"Apply selected resource?\n\n{selectResourceName}", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                config.ResourceName = selectResourceName;
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }
    }
}
