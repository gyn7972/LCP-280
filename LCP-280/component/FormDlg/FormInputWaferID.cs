using System;
using System.Windows.Forms;

namespace QMC.LCP_280.Process.Unit.FormMain
{
    public partial class FormInputWaferID : Form
    {
        public FormInputWaferID()
        {
            InitializeComponent();
        }

        public string WaferId
        {
            get => txtWaferId.Text?.Trim() ?? string.Empty;
            set => txtWaferId.Text = value ?? string.Empty;
        }

        private void WaferIdInputForm_Load(object sender, EventArgs e)
        {
            txtWaferId.SelectAll();
            txtWaferId.Focus();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(WaferId))
            {
                MessageBox.Show(this, "Wafer ID를 입력하세요.", "입력 필요", MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtWaferId.Focus();
                this.DialogResult = DialogResult.None;
                return;
            }
        }

        public static bool TryGetWaferId(IWin32Window owner, string initialValue, out string waferId)
        {
            using (var dlg = new FormInputWaferID())
            {
                dlg.WaferId = initialValue ?? string.Empty;
                var dr = dlg.ShowDialog(owner);
                if (dr == DialogResult.OK)
                {
                    waferId = dlg.WaferId;
                    return true;
                }
            }
            waferId = null;
            return false;
        }
    }
}