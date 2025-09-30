using QMC.Common;
using QMC.Common.Keithley;
using QMC.Common.Spectrometer;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QMC.LCP_280.Process.Unit.FormSetup.Page
{
    public partial class SpectrometerPage : UserControl
    {
        private Equipment equipment => Equipment.Instance;

        // Spectrometer
        private CASSpectrometer selectSpectrometer;
        private PropertyCollection pcConfig;

        private FormSetupCASSpectrometerInterface setupDialog = new FormSetupCASSpectrometerInterface();

        public SpectrometerPage()
        {
            InitializeComponent();
            InitializeSpectrometerListView();

            setupDialog.OnNewDeviceConfigApplied += SetupDialog_OnNewDeviceConfigApplied;
        }

        private void SetupDialog_OnNewDeviceConfigApplied(object sender, CASSpectrometerConfig e)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => SetupDialog_OnNewDeviceConfigApplied(sender, e)));
                return;
            }

            if (e != null && pcConfig != null)
            {
                pcConfig["DeviceInterfaceType"].Value = e.DeviceInterfaceType;
                pcConfig["DeviceInterfaceOption"].Value = e.DeviceInterfaceOption;
                pcConfig["ConfigFileName"].Value = e.ConfigFileName;
                pcConfig["CalibFileName"].Value = e.CalibFileName;
                pcvConfig.SetProperties(pcConfig);
            }
        }

        private void InitializeSpectrometerListView()
        {
            List<string> names = equipment.Spectrometers.Keys.ToList();
            lbivSelectSpectrometer.SetItems(names.ToArray());
        }

        private void lbivSelectSpectrometer_ItemSelected(object sender, int e)
        {
            // Select new spectrometer
            CASSpectrometer newSelect = equipment.Spectrometers[lbivSelectSpectrometer.SelectedItemName];
            SubscribeNewSpectrometerEvent(newSelect);

            selectSpectrometer = newSelect;
            DisplaySelectSpectrometerInformation(newSelect);
        }

        private void SubscribeNewSpectrometerEvent(CASSpectrometer spectrometer)
        {
            if (selectSpectrometer != null)
            {
                selectSpectrometer.OnDeviceCreated -= Spectrometer_OnDeviceCreated;
                selectSpectrometer.OnDeviceTerminated -= Spectrometer_OnDeviceTerminated;
            }
            if (spectrometer != null)
            {
                spectrometer.OnDeviceCreated += Spectrometer_OnDeviceCreated;
                spectrometer.OnDeviceTerminated += Spectrometer_OnDeviceTerminated;
            }
        }

        private void Spectrometer_OnDeviceCreated(object sender)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => Spectrometer_OnDeviceCreated(sender)));
                return;
            }

            CASSpectrometer spectrometer = sender as CASSpectrometer;
            if (selectSpectrometer == spectrometer && spectrometer != null)
            {
                // Device Info
                lbSpectrometerModelValue.Text = spectrometer.DeviceInfo.Name;
                lbSpectrometerSerialNoValue.Text = spectrometer.DeviceInfo.SerialNumber;

                // Status
                lbSpectrometerStatusValue.Text = "Device Created";

                // Interface
                lbSpectrometerDeviceInterfaceValue.Text = spectrometer.DeviceInfo.InterfaceType;
                lbSpectrometerDeviceOption.Text = spectrometer.DeviceInfo.InterfaceOption;
            }
        }

        private void Spectrometer_OnDeviceTerminated(object sender)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => Spectrometer_OnDeviceCreated(sender)));
                return;
            }

            CASSpectrometer spectrometer = sender as CASSpectrometer;
            if (selectSpectrometer == spectrometer && spectrometer != null)
            {
                // Device Info
                lbSpectrometerModelValue.Text = "";
                lbSpectrometerSerialNoValue.Text = "";

                // Status
                lbSpectrometerStatusValue.Text = "Device Terminated";

                // Interface
                lbSpectrometerDeviceInterfaceValue.Text = "";
                lbSpectrometerDeviceOption.Text = "";
            }
        }

        private void DisplaySelectSpectrometerInformation(CASSpectrometer spectrometer)
        {
            if (spectrometer != null)
            {
                // Device Information
                lbSpectrometerModelValue.Text = spectrometer.DeviceInfo.Name;
                lbSpectrometerSerialNoValue.Text = spectrometer.DeviceInfo.SerialNumber;

                // Device Status
                lbSpectrometerStatusValue.Text = spectrometer.IsCreated() ? "Device Created" : "Device Terminated";
                lbSpectrometerDeviceInterfaceValue.Text = spectrometer.DeviceInfo.InterfaceType;
                lbSpectrometerDeviceOption.Text = spectrometer.DeviceInfo.InterfaceOption;

                pcConfig = spectrometer.Config.GetPropertyCollection();
                pcvConfig.SetProperties(pcConfig);

                casSpectrumViewer.AttachSpectrometer(spectrometer);
            }
            else
            {
                lbSpectrometerModelValue.Text = "";
                lbSpectrometerSerialNoValue.Text = "";
                lbSpectrometerStatusValue.Text = "No Device";
                lbSpectrometerDeviceInterfaceValue.Text = "";
                lbSpectrometerDeviceOption.Text = "";

                casSpectrumViewer.DetachSpectrometer();
            }
        }

        private void btnSpectrometerSetup_Click(object sender, EventArgs e)
        {
            setupDialog?.ShowDialog(this);
        }

        private void btnSpectrometerInitialize_Click(object sender, EventArgs e)
        {
            if (selectSpectrometer == null)
            {
                MessageBox.Show("No spectrometer selected or invalid instance.");
                return;
            }

            int ret = 0;
            if ((ret = selectSpectrometer.Initialize()) != 0)
            {
                MessageBox.Show($"Initialization operation failed. (Reason Code: {ret})");
                return;
            }

            MessageBox.Show($"[{selectSpectrometer.Name}] was successfully initialized.");
        }

        private void btnSpectrometerConfigSave_Click(object sender, EventArgs e)
        {
            if (selectSpectrometer == null)
            {
                MessageBox.Show("No spectrometer selected or invalid instance.");
                return;
            }

            try
            {
                if (MessageBox.Show("Would you like to save your settings?", "Save", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    pcvConfig.Apply();

                    selectSpectrometer.Config.ApplyValueFromPropertyCollection(pcConfig);
                    if (selectSpectrometer.Config.Save() == 0)
                    {
                        MessageBox.Show($"The settings for [{selectSpectrometer.Name}] have been successfully saved.");
                    }
                    else
                    {
                        MessageBox.Show($"Failed to save the settings of [{selectSpectrometer.Name}].");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }

        private void btnMeasureTest_Click(object sender, EventArgs e)
        {
            if (selectSpectrometer == null)
            {
                MessageBox.Show("No spectrometer selected or invalid instance.");
                return;
            }

            try
            {
                int ret = 0;
                if ((ret = selectSpectrometer.Measure()) != 0)
                {
                    MessageBox.Show($"Failed to measurement. (Reason Code: {ret})");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }

        private void btnMeasureDarkCurrent_Click(object sender, EventArgs e)
        {
            if (selectSpectrometer == null)
            {
                MessageBox.Show("No spectrometer selected or invalid instance.");
                return;
            }
            if (!selectSpectrometer.IsCreated())
            {
                MessageBox.Show("The spectrometer is not created.");
                return;
            }

            try
            {
                int ret = 0;
                if ((ret = selectSpectrometer.MeasureDarkCurrent()) != 0)
                {
                    MessageBox.Show($"Failed to dark current measurement. (Reason Code: {ret})");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }
    }
}
