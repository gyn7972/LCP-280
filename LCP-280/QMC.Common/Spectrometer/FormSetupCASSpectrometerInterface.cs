using InstrumentSystems.CAS4;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QMC.Common.Spectrometer
{
    public partial class FormSetupCASSpectrometerInterface : Form
    {
        private CASSpectrometerInterface supportInterface;
        private int selectedInterfaceTypeIndex;
        private int selectedInterfaceOptionIndex;
        private string selectedConfigFilePath;
        private string selectedCalibFilePath;

        public event EventHandler<CASSpectrometerConfig> OnNewDeviceConfigApplied;

        public FormSetupCASSpectrometerInterface()
        {
            InitializeComponent();          
        }

        private void FormSetupCASSpectrometerInterface_Shown(object sender, EventArgs e)
        {
            RefreshInterfaceList();

            selectedInterfaceTypeIndex = -1;
            selectedInterfaceOptionIndex = -1;
            selectedConfigFilePath = string.Empty;
            selectedCalibFilePath = string.Empty;

            lbInterfaceTypeValue.Text = "-";
            lbInterfaceOptionValue.Text = "-";
            lbConfigFilePathValue.Text = "-";
            lbCalibFilePathValue.Text = "-";
        }

        private void RefreshInterfaceList()
        {
            supportInterface = CASSpectrometerInterfaceManager.FindSupportInterface();

            lbxIntType.Items.Clear();
            selectedInterfaceTypeIndex = -1;
            lbInterfaceTypeValue.Text = "-";

            foreach (var typeName in supportInterface.GetTypeNameList())
            {
                lbxIntType.Items.Add(typeName);
            }

            lbxIntOption.Items.Clear();
            selectedInterfaceOptionIndex = -1;
            lbInterfaceOptionValue.Text = "-";
        }

        private void lbxIntType_SelectedValueChanged(object sender, EventArgs e)
        {
            // Update selected interface type
            selectedInterfaceTypeIndex = lbxIntType.SelectedIndex;
            lbInterfaceTypeValue.Text = supportInterface.Types[selectedInterfaceTypeIndex].Name;

            // Update interface options
            lbxIntOption.Items.Clear();
            selectedInterfaceOptionIndex = -1;
            lbInterfaceOptionValue.Text = "-";

            foreach (var optionName in supportInterface.GetOptionNameList(selectedInterfaceTypeIndex))
            {
                lbxIntOption.Items.Add(optionName);
            }
        }

        private void lbxIntOption_SelectedIndexChanged(object sender, EventArgs e)
        {
            selectedInterfaceOptionIndex = lbxIntOption.SelectedIndex;
            lbInterfaceOptionValue.Text = supportInterface.Types[selectedInterfaceTypeIndex].Options[selectedInterfaceOptionIndex].Name;
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            RefreshInterfaceList();
        }

        private void btnSelectConfigFile_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "INI files (*.ini)|*.ini";
                openFileDialog.Title = "Select Configuration File";
                openFileDialog.CheckFileExists = true;
                openFileDialog.CheckPathExists = true;
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    selectedConfigFilePath = openFileDialog.FileName;
                    lbConfigFilePathValue.Text = selectedConfigFilePath;
                }
            }
        }

        private void btnSelectCalibFile_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Calib files (*.isc)|*.isc";
                openFileDialog.Title = "Select Calibration File";
                openFileDialog.CheckFileExists = true;
                openFileDialog.CheckPathExists = true;
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    selectedCalibFilePath = openFileDialog.FileName;
                    lbCalibFilePathValue.Text = selectedCalibFilePath;
                }
            }
        }

        private void btnApply_Click(object sender, EventArgs e)
        {
            try
            {
                CASSpectrometerConfig config = new CASSpectrometerConfig("");

                // Validate interface type selection
                if (selectedInterfaceTypeIndex < 0)
                {
                    MessageBox.Show("Please select an interface type.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                switch (supportInterface.Types[selectedInterfaceTypeIndex].Value)
                {
                    case CAS4DLL.InterfacePCI:
                        config.DeviceInterfaceType = CASSpectrometerConfig.DeviceInterface.PCI;
                        break;
                    case CAS4DLL.InterfaceTest:
                        config.DeviceInterfaceType = CASSpectrometerConfig.DeviceInterface.Test;
                        break;
                    case CAS4DLL.InterfaceUSB:
                        config.DeviceInterfaceType = CASSpectrometerConfig.DeviceInterface.USB;
                        break;
                    case CAS4DLL.InterfacePCIe:
                        config.DeviceInterfaceType = CASSpectrometerConfig.DeviceInterface.PCIe;
                        break;
                    case CAS4DLL.InterfaceEthernet:
                        config.DeviceInterfaceType = CASSpectrometerConfig.DeviceInterface.Ethernet;
                        break;
                    default:
                        MessageBox.Show("The selected interface is not supported.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                }

                // Validate interface option selection
                if (selectedInterfaceOptionIndex < 0)
                {
                    MessageBox.Show("Please select an interface option.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                config.DeviceInterfaceOption = supportInterface.Types[selectedInterfaceTypeIndex].Options[selectedInterfaceOptionIndex].Value;
                if (config.DeviceInterfaceOption < 0)
                {
                    MessageBox.Show("The selected interface option is not valid.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Validate config file path
                if (File.Exists(selectedConfigFilePath))
                {
                    config.ConfigFileName = selectedConfigFilePath;
                }
                else
                {
                    MessageBox.Show("The selected configuration file does not exist.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Validate calib file path
                if (File.Exists(selectedCalibFilePath))
                {
                    config.CalibFileName = selectedCalibFilePath;
                }
                else
                {
                    MessageBox.Show("The selected calibration file does not exist.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Confirm and apply configuration
                if (MessageBox.Show($"Apply device configuration?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    OnNewDeviceConfigApplied?.Invoke(this, config);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error applying configuration: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }
    }
}
