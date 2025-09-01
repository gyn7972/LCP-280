using QMC.Common;
using QMC.Common.Keithley;
using QMC.Common.Motions.CKD;
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

        private CKDMotorDriver motorDriver = new CKDMotorDriver("Test");

        public Instrument_Setup()
        {
            InitializeComponent();
            InitializeUI();
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
            tbLog.AppendText($"[{selectSourcemeterName}] Error: {e.Message}.");
        }

        private void SelectSourcemeter_OnSessionClosed(object sender, EventArgs e)
        {
            tbLog.AppendText($"[{selectSourcemeterName}] Session closed.");
        }

        private void SelectSourcemeter_OnSessionOpened(object sender, EventArgs e)
        {
            tbLog.AppendText($"[{selectSourcemeterName}] Session opened.");
        }

        private void SelectSourcemeter_OnReceived(object sender, EventArgs e)
        {
            tbLog.AppendText($"[{selectSourcemeterName}] Message received.");
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
            if (selectSourcemeter == null)
            {
                MessageBox.Show($"No sourcemeter selected or invalid instance. (Select: {lbivSelectItem.SelectedItemName})");
                return;
            }

            FormSetupKeithleyInstrument dialog = new FormSetupKeithleyInstrument(selectSourcemeter);
            dialog.ShowDialog(this);
        }

        private void btn_Initialize_Click(object sender, EventArgs e)
        {
            if (selectSourcemeter == null)
            {
                MessageBox.Show($"No sourcemeter selected or invalid instance. (Select: {lbivSelectItem.SelectedItemName})");
                return;
            }

            selectSourcemeter.Initialize();
        }
    }
}
