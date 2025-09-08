using QMC.Common;
using QMC.Common.Keithley;
using QMC.Common.PKGTester;
using QMC.Common.Spectrometer;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace QMC.LCP_280.Process.Unit
{
    public partial class Instrument_Setup : Form
    {
        private Equipment equipment => Equipment.Instance;

        // Sourcemeter
        private KeithleySourcemeter selectSourcemeter;
        private PropertyCollection pcSmuConfig;
        private string selectSourcemeterName;

        private KeithleySourcemeterChannel selectSourcemeterChannel;
        private string selectSourcemeterChannelName;

        private FormSetupKeithleyInstrument setupDialog = new FormSetupKeithleyInstrument();

        // Spectrometer
        private CASSpectrometer selectSpectrometer;
        private PropertyCollection pcSpcConfig;
        private string selectSpectrometerName;

        private FormSetupCASSpectrometerInterface setupSpectrometerDialog = new FormSetupCASSpectrometerInterface();


        public Instrument_Setup()
        {
            InitializeComponent();
            InitializeUI();

            setupDialog.OnNewResourceSelected += SetupDialog_OnNewResourceSelected;
            setupSpectrometerDialog.OnNewDeviceConfigApplied += SetupSpectrometerDialog_OnNewDeviceConfigApplied;
        }

        public void InitializeUI()
        {
            InitializeSourcemeterListView();
            InitializeSpectrometerListView();
        }

        private void Instrument_Setup_FormClosed(object sender, FormClosedEventArgs e)
        {
        }

        #region Sourcemeter Pages
        private void InitializeSourcemeterListView()
        {
            List<string> names = equipment.Sourcemeters.Keys.ToList();
            lbivSelectSourcemeter.SetItems(names.ToArray());
        }

        private void lbivSelectItem_ItemSelected(object sender, int e)
        {
            // Select new sourcemeter
            selectSourcemeter = equipment.Sourcemeters[lbivSelectSourcemeter.SelectedItemName];
            if (selectSourcemeter != null)
            {
                // sourcemeter
                selectSourcemeterName = selectSourcemeter.Name;

                // sourcemeter channel
                List<string> channelNames = selectSourcemeter.Channels.Keys.ToList();
                lbivSelectChannel.SetItems(channelNames.ToArray());
                selectSourcemeterChannel = null;
            }
            
            DisplaySelectSourcemeterInformation(selectSourcemeter);
        }

        private void DisplaySelectSourcemeterInformation(KeithleySourcemeter sourcemeter)
        {
            if (sourcemeter == null)
            {
                MessageBox.Show($"No sourcemeter selected or invalid instance. (Select: {lbivSelectSourcemeter.SelectedItemName})");
                return;
            }

            // Device Information
            lbModelValue.Text = selectSourcemeter.Name;

            // Device Status
            lbStatusValue.Text = selectSourcemeter.Communicator.IsConnected ? "Connected" : "Disconnected";

            // Comm. Terminal
            tbLog.Clear();
            tbSendText.Clear();

            pcSmuConfig = selectSourcemeter.Config.GetPropertyCollection();
            pcvConfig.SetProperties(pcSmuConfig);
        }

        private void lbivSelectChannel_ItemSelected(object sender, int e)
        {
            selectSourcemeterChannel = selectSourcemeter.Channels[lbivSelectChannel.SelectedItemName];
            if (selectSourcemeterChannel != null)
            {
                selectSourcemeterChannelName = selectSourcemeterChannel.Name;
            }
        }

        private void SelectSourcemeter_OnError(object sender, Exception e)
        {
            tbLog.AppendText($"[{selectSourcemeterName}] Error: {e.Message}.\n");
        }

        private void SelectSourcemeter_OnSessionClosed(object sender, EventArgs e)
        {
            tbLog.AppendText($"[{selectSourcemeterName}] Session closed.\n");
        }

        private void SelectSourcemeter_OnSessionOpened(object sender, EventArgs e)
        {
            tbLog.AppendText($"[{selectSourcemeterName}] Session opened.\n");
        }

        private void SelectSourcemeter_OnReceived(object sender, EventArgs e)
        {
            tbLog.AppendText($"[{selectSourcemeterName}] Message received.\n");
        }

        private void SetupDialog_OnNewResourceSelected(object sender, string e)
        {
            try
            {
                pcSmuConfig["ResourceName"].Value = e;
                pcvConfig.SetProperties(pcSmuConfig);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred. {ex.Message}");
                return;
            }
        }

        private void btn_Connect_Click(object sender, EventArgs e)
        {
            if (selectSourcemeter == null)
            {
                MessageBox.Show("No sourcemeter selected or invalid instance.");
                return;
            }
            if (selectSourcemeter.Communicator.IsConnected)
            {
                MessageBox.Show("The sourcemeter is already connected.");
                return;
            }

            // Open Session
            if (selectSourcemeter.Communicator.OpenSession(selectSourcemeter.Config.ResourceName))
            {
                MessageBox.Show("Communication connection successful.");
            }
            else
            {
                MessageBox.Show("Communication connection failed.");
            }
        }

        private void btn_Disconnect_Click(object sender, MouseEventArgs e)
        {
            if (selectSourcemeter == null)
            {
                MessageBox.Show("No sourcemeter selected or invalid instance.");
                return;
            }
            if (!selectSourcemeter.Communicator.IsConnected)
            {
                MessageBox.Show("The sourcemeter is already disconnected.");
                return;
            }

            // Close Session
            if (selectSourcemeter.Communicator.CloseSession())
            {
                MessageBox.Show("The communication connection has been disconnected.");
            }
            else
            {
                MessageBox.Show("Failed to disconnect communication.");
            }
        }

        private void btn_Setup_MouseClick(object sender, MouseEventArgs e)
        {
            setupDialog?.ShowDialog(this);
        }

        private void btn_Initialize_Click(object sender, EventArgs e)
        {
            if (selectSourcemeter == null)
            {
                MessageBox.Show("No sourcemeter selected or invalid instance.");
                return;
            }

            int ret = 0;
            if ((ret = selectSourcemeter.Initialize()) != 0)
            {
                MessageBox.Show($"Initialization operation failed. (Reason Code: {ret})");
                return;
            }

            MessageBox.Show($"[{selectSourcemeter.Name}] was successfully initialized.");
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (selectSourcemeter == null)
            {
                MessageBox.Show("No sourcemeter selected or invalid instance.");
                return;
            }

            try
            {
                if (MessageBox.Show("Would you like to save your settings?", "Save", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    selectSourcemeter.Config.ApplyValueFromPropertyCollection(pcSmuConfig);
                    if (selectSourcemeter.Config.Save() == 0)
                    {
                        MessageBox.Show($"The settings for [{selectSourcemeter.Name}] have been successfully saved.");
                    }
                    else
                    {
                        MessageBox.Show($"Failed to save the settings of [{selectSourcemeter.Name}].");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }

        private void btn_SendTest_Click(object sender, EventArgs e)
        {
            if (selectSourcemeter == null)
            {
                MessageBox.Show("No sourcemeter selected or invalid instance.");
                return;
            }
            if (!selectSourcemeter.Communicator.IsConnected)
            {
                MessageBox.Show("The sourcemeter is already disconnected.");
                return;
            }

            try
            {
                if (selectSourcemeter.Communicator.Write(tbSendText.Text))
                {
                    tbLog.AppendText($"[{selectSourcemeter.Name}] Sent: {tbSendText.Text}\n");
                }
                else
                {
                    tbLog.AppendText($"[{selectSourcemeter.Name}] Failed to send: {tbSendText.Text}\n");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }

        private void btnChannelTest_Click(object sender, EventArgs e)
        {
            // Do this.
            if (selectSourcemeterChannel == null)
            {
                MessageBox.Show("No sourcemeter channel selected or invalid instance.");
                return;
            }

            TestConditionItem item = new TestConditionItem("VF_Test");
            item.SourceValue = 0.001;//A
            item.SourceTime = 10;//ms
            item.SourceLimit = 3;//V
            item.MeasureTime = 10;//ms
            selectSourcemeter.ClearTestItems();
            selectSourcemeter.AddTestItem(item);
            selectSourcemeter.Measure();
        }
        #endregion

        #region Spectrometer Pages
        private void InitializeSpectrometerListView()
        {
            List<string> names = equipment.Spectrometers.Keys.ToList();
            lbivSelectSpectrometer.SetItems(names.ToArray());
        }

        private void lbivSelectSpectrometer_ItemSelected(object sender, int e)
        {
            // Select new sourcemeter
            selectSpectrometer = equipment.Spectrometers[lbivSelectSpectrometer.SelectedItemName];
            if (selectSpectrometer != null)
            {
                selectSpectrometerName = selectSpectrometer.Name;
                casSpectrumViewer.AttachSpectrometer(selectSpectrometer);
            }

            DisplaySelectSpectrometerInformation(selectSpectrometer);
        }

        private void DisplaySelectSpectrometerInformation(CASSpectrometer spectrometer)
        {
            if (spectrometer == null)
            {
                MessageBox.Show($"No spectrometer selected or invalid instance. (Select: {lbivSelectSpectrometer.SelectedItemName})");
                return;
            }

            // Device Information
            lbSpectrometerModelValue.Text = "";
            lbSpectrometerSerialNoValue.Text = "";

            // Device Status
            lbSpectrometerStatusValue.Text = "";
            lbSpectrometerDeviceInterfaceValue.Text = "";
            lbSpectrometerDeviceOption.Text = "";

            pcSpcConfig = selectSpectrometer.Config.GetPropertyCollection();
            pcvSpectrometerConfig.SetProperties(pcSpcConfig);
        }

        private void SetupSpectrometerDialog_OnNewDeviceConfigApplied(object sender, CASSpectrometerConfig e)
        {
            try
            {
                if (e == null)
                {
                    throw new ArgumentNullException("argument is null.");
                }

                pcSpcConfig["DeviceInterfaceType"].Value = e.DeviceInterfaceType;
                pcSpcConfig["DeviceInterfaceOption"].Value = e.DeviceInterfaceOption;
                pcSpcConfig["ConfigFileName"].Value = e.ConfigFileName;
                pcSpcConfig["CalibFileName"].Value = e.CalibFileName;
                pcvSpectrometerConfig.SetProperties(pcSpcConfig);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }

        private void btnSpectrometerSetup_Click(object sender, EventArgs e)
        {
            setupSpectrometerDialog?.ShowDialog(this);
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
                    selectSpectrometer.Config.ApplyValueFromPropertyCollection(pcSpcConfig);
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
        #endregion

        private void btnMeasureTest_Click(object sender, EventArgs e)
        {
            if (selectSpectrometer == null)
            {
                MessageBox.Show("No spectrometer selected or invalid instance.");
                return;
            }
            if (!selectSpectrometer.IsInitialized())
            {
                MessageBox.Show("The spectrometer is not initialized.");
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
