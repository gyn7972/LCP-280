using QMC.Common;
using QMC.Common.Keithley;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QMC.LCP_280.Process.Unit.FormSetup.Page
{
    public partial class SourcemeterPage : UserControl
    {
        private Equipment equipment => Equipment.Instance;

        // Sourcemeter
        private KeithleySourcemeter selectSourcemeter;
        private KeithleySourcemeterChannel selectSourcemeterChannel;

        private PropertyCollection pcSmuConfig;
        private FormSetupKeithleyInstrument setupDialog = new FormSetupKeithleyInstrument();

        public SourcemeterPage()
        {
            InitializeComponent();
            InitializeSourcemeterListView();

            setupDialog.OnNewResourceSelected += SetupDialog_OnNewResourceSelected;
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

        private void InitializeSourcemeterListView()
        {
            List<string> names = equipment.Sourcemeters.Keys.ToList();
            lbivSelectSourcemeter.SetItems(names.ToArray());
        }

        private void lbivSelectSourcemeter_ItemSelected(object sender, int e)
        {
            KeithleySourcemeter newSelect = equipment.Sourcemeters[lbivSelectSourcemeter.SelectedItemName];
            SubscribeNewSourcemeterEvent(newSelect);

            selectSourcemeter = newSelect;
            selectSourcemeterChannel = null;

            DisplaySelectSourcemeterInformation(newSelect);
        }

        private void SubscribeNewSourcemeterEvent(KeithleySourcemeter sourcemeter)
        {
            if (selectSourcemeter != null)
            {
                selectSourcemeter.OnSessionOpened -= Sourcemeter_OnSessionOpened;
                selectSourcemeter.OnSessionClosed -= Sourcemeter_OnSessionClosed;
                selectSourcemeter.OnReceived -= Sourcemeter_OnReceived;
                selectSourcemeter.OnInitialized -= Sourcemeter_OnInitialized;
            }
            if (sourcemeter != null)
            {
                sourcemeter.OnSessionOpened += Sourcemeter_OnSessionOpened;
                sourcemeter.OnSessionClosed += Sourcemeter_OnSessionClosed;
                sourcemeter.OnReceived += Sourcemeter_OnReceived;
                sourcemeter.OnInitialized += Sourcemeter_OnInitialized;
            }
        }

        private void Sourcemeter_OnSessionOpened(object sender, EventArgs e)
        {
            if (selectSourcemeter != null)
            {
                if (lbStatusValue.InvokeRequired)
                {
                    lbStatusValue.Invoke(new Action(() => lbStatusValue.Text = "Connected"));
                }
                else
                {
                    lbStatusValue.Text = "Connected";
                }
            }
        }

        private void Sourcemeter_OnSessionClosed(object sender, EventArgs e)
        {
            if (selectSourcemeter != null)
            {
                if (lbStatusValue.InvokeRequired)
                {
                    lbStatusValue.Invoke(new Action(() => lbStatusValue.Text = "Disconnected"));
                }
                else
                {
                    lbStatusValue.Text = "Disconnected";
                }
            }
        }

        private void Sourcemeter_OnReceived(object sender, EventArgs e)
        {
            if (selectSourcemeter != null)
            {
                if (tbLog.InvokeRequired)
                {
                    tbLog.Invoke(new Action(() => tbLog.AppendText($"[{selectSourcemeter.Name}] Message Received.\n")));
                }
                else
                {
                    tbLog.AppendText($"[{selectSourcemeter.Name}] Message Received.\n");
                }
            }
        }

        private void Sourcemeter_OnInitialized(object sender, EventArgs e)
        {
            if (selectSourcemeter != null)
            {
                lbModelValue.Text = selectSourcemeter.ModelName;
                lbSerialNumberValue.Text = selectSourcemeter.SerialNo;
            }
        }   

        private void btn_Setup_Click(object sender, EventArgs e)
        {
            setupDialog?.ShowDialog(this);
        }

        private void DisplaySelectSourcemeterInformation(KeithleySourcemeter sourcemeter)
        {
            if (sourcemeter != null)
            {
                // sourcemeter information
                lbModelValue.Text = sourcemeter.ModelName;
                lbSerialNumberValue.Text = sourcemeter.SerialNo;

                // sourcemeter status
                lbStatusValue.Text = sourcemeter.Communicator.IsConnected ? "Connected" : "Disconnected";

                // sourcemeter channel
                List<string> channelNames = sourcemeter.Channels.Keys.ToList();
                lbivSelectChannel.SetItems(channelNames.ToArray());

                // Comm. Terminal
                tbLog.Clear();
                tbSendText.Clear();

                pcSmuConfig = sourcemeter.Config.GetPropertyCollection();
                pcvConfig.SetProperties(pcSmuConfig);
            }
            else
            {
                lbModelValue.Text = "-";
                lbSerialNumberValue.Text = "-";
                lbStatusValue.Text = "-";
                lbivSelectChannel.SetItems(null);
            }
        }

        private void lbivSelectChannel_ItemSelected(object sender, int e)
        {
            selectSourcemeterChannel = selectSourcemeter.Channels[lbivSelectChannel.SelectedItemName];
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
                    pcvConfig.Apply();

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
                MessageBox.Show("The sourcemeter is disconnected.");
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

            string str = "";
            if (selectSourcemeter.Communicator.Read(ref str))
            {
                MessageBox.Show(str);
            }
            else
            {
                MessageBox.Show("뿅");
            }
        }
    }
}