using QMC.Common;
using QMC.Common.Keithley;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QMC.LCP_280.Process.Unit
{
    public partial class Instrument_Setup : Form
    {
        private Equipment equipment => Equipment.Instance;
        private KeithleySourcemeter selectSourcemeter;
        private PropertyCollection propertyCollection;
        private string selectSourcemeterName;

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
                selectSourcemeter.OnReceived -= SelectSourcemeter_OnReceived;
                selectSourcemeter.OnSessionOpened -= SelectSourcemeter_OnSessionOpened;
                selectSourcemeter.OnSessionClosed -= SelectSourcemeter_OnSessionClosed;
                selectSourcemeter.OnError -= SelectSourcemeter_OnError;
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
                selectSourcemeter.OnReceived -= SelectSourcemeter_OnReceived;
                selectSourcemeter.OnSessionOpened -= SelectSourcemeter_OnSessionOpened;
                selectSourcemeter.OnSessionClosed -= SelectSourcemeter_OnSessionClosed;
                selectSourcemeter.OnError -= SelectSourcemeter_OnError;
            }

            // Select new sourcemeter
            selectSourcemeter = equipment.Sourcemeters[lbivSelectItem.SelectedItemName];

            // Subscribe to new event handlers
            if (selectSourcemeter != null)
            {
                selectSourcemeterName = selectSourcemeter.Name;
                selectSourcemeter.OnReceived += SelectSourcemeter_OnReceived;
                selectSourcemeter.OnSessionOpened += SelectSourcemeter_OnSessionOpened;
                selectSourcemeter.OnSessionClosed += SelectSourcemeter_OnSessionClosed;
                selectSourcemeter.OnError += SelectSourcemeter_OnError;
            }
            
            DisplaySelectSourcemeterInformation(selectSourcemeter);
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
            propertyCollection["ResourceName"].Value = e;
            configPropertyCollectionView.SetProperties(propertyCollection);
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
            lbStatusValue.Text = selectSourcemeter.IsConnected ? "Connected" : "Disconnected";

            // Comm. Terminal
            tbLog.Clear();
            tbSendText.Clear();

            propertyCollection = selectSourcemeter.Config.GetPropertyCollection();
            configPropertyCollectionView.SetProperties(propertyCollection);
        }

        private void btn_Connect_Click(object sender, EventArgs e)
        {
            // Do this.
        }

        private void btn_Disconnect_Click(object sender, EventArgs e)
        {
            // Do this.
        }

        private void btn_Setup_Click(object sender, EventArgs e)
        {
            setupDialog?.ShowDialog(this);
        }

        private void btn_Initialize_Click(object sender, EventArgs e)
        {
            selectSourcemeter?.Initialize();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                selectSourcemeter?.Config.ApplyValueFromPropertyCollection(propertyCollection);
                selectSourcemeter?.Config.Save();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save configuration. {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }

        private void btn_SendTest_Click(object sender, EventArgs e)
        {
            try
            {
                //selectSourcemeter?.Write(tbSendText.Text);
                string response = "";

                if (selectSourcemeter?.Query(tbSendText.Text, ref response) == 0)
                    MessageBox.Show(response);
                else
                    MessageBox.Show("No response received.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to send command. {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }

        
    }
}
