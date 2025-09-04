using QMC.Common;
using QMC.Common.Keithley;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QMC.LCP_280.Process.Unit
{
    public partial class Instrument_Setup : Form
    {
        private Equipment equipment => Equipment.Instance;

        private KeithleySourcemeter selectSourcemeter;
        private PropertyCollection pcConfig;
        private string selectSourcemeterName;

        private KeithleySourcemeterChannel selectSourcemeterChannel;
        private string selectSourcemeterChannelName;

        private FormSetupKeithleyInstrument setupDialog = new FormSetupKeithleyInstrument();

        public Instrument_Setup()
        {
            InitializeComponent();
            InitializeUI();

            setupDialog.OnNewResourceSelected += SetupDialog_OnNewResourceSelected;
        }

        private void Instrument_Setup_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (selectSourcemeter != null)
            {
                selectSourcemeter.Communicator.OnReceived -= SelectSourcemeter_OnReceived;
                selectSourcemeter.Communicator.OnSessionOpened -= SelectSourcemeter_OnSessionOpened;
                selectSourcemeter.Communicator.OnReceived -= SelectSourcemeter_OnSessionClosed;
                selectSourcemeter.Communicator.OnError -= SelectSourcemeter_OnError;
            }
        }     

        public void InitializeUI()
        {
            InitializeListView();
        }

        private void InitializeListView()
        {
            List<string> names = equipment.Sourcemeters.Keys.ToList();
            lbivSelectItem.SetItems(names.ToArray());
        }

        private void lbivSelectItem_ItemSelected(object sender, int e)
        {
            // Unsubscribe previous event handlers
            if (selectSourcemeter != null)
            {
                selectSourcemeter.Communicator.OnReceived -= SelectSourcemeter_OnReceived;
                selectSourcemeter.Communicator.OnSessionOpened -= SelectSourcemeter_OnSessionOpened;
                selectSourcemeter.Communicator.OnSessionClosed -= SelectSourcemeter_OnSessionClosed;
                selectSourcemeter.Communicator.OnError -= SelectSourcemeter_OnError;
            }

            // Select new sourcemeter
            selectSourcemeter = equipment.Sourcemeters[lbivSelectItem.SelectedItemName];

            // Subscribe to new event handlers
            if (selectSourcemeter != null)
            {
                selectSourcemeterName = selectSourcemeter.Name;
                selectSourcemeter.Communicator.OnReceived += SelectSourcemeter_OnReceived;
                selectSourcemeter.Communicator.OnReceived += SelectSourcemeter_OnSessionOpened;
                selectSourcemeter.Communicator.OnSessionClosed += SelectSourcemeter_OnSessionClosed;
                selectSourcemeter.Communicator.OnError += SelectSourcemeter_OnError;

                List<string> channelNames = selectSourcemeter.Channels.Keys.ToList();
                lbivSelectChannel.SetItems(channelNames.ToArray());

                selectSourcemeterChannel = null;
                selectSourcemeterChannelName = "";
            }
            
            DisplaySelectSourcemeterInformation(selectSourcemeter);
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
            pcConfig["ResourceName"].Value = e;
            pcvConfig.SetProperties(pcConfig);
        }

        private void DisplaySelectSourcemeterInformation(KeithleySourcemeter sourcemeter)
        {
            if (sourcemeter == null)
            {
                MessageBox.Show($"No sourcemeter selected or invalid instance. (Select: {lbivSelectItem.SelectedItemName})");
                return;
            }

            // Device Information
            lbModelValue.Text = selectSourcemeter.Name;

            // Device Status
            lbStatusValue.Text = selectSourcemeter.Communicator.IsConnected ? "Connected" : "Disconnected";

            // Comm. Terminal
            tbLog.Clear();
            tbSendText.Clear();

            pcConfig = selectSourcemeter.Config.GetPropertyCollection();
            pcvConfig.SetProperties(pcConfig);
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
                    selectSourcemeter.Config.ApplyValueFromPropertyCollection(pcConfig);
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
                MessageBox.Show($"Failed to save configuration. {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

            }
            catch (Exception ex)
            {

            }
        }

        private void btnChannelTest_Click(object sender, EventArgs e)
        {

        }
    }
}
