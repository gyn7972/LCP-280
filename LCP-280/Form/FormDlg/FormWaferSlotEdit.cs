using QMC.Common;
using QMC.LCP_280.Process.Component;
using System;
using System.Drawing;
using System.Windows.Forms;
using static QMC.Common.Material;

namespace QMC.LCP_280.Process.Unit.FormMain
{
    internal sealed class FormWaferSlotEdit : Form
    {
        public enum SlotEditAction
        {
            None = 0,
            AddOrUpdate = 1,
            Delete = 2
        }

        private readonly int _slotNumber1;
        private readonly MaterialWafer _currentWafer;

        private RadioButton rbAdd;
        private RadioButton rbDelete;
        private TextBox txtWaferId;
        private ComboBox cbState;
        private Button btnOk;
        private Button btnCancel;

        public SlotEditAction ActionResult { get; private set; } = SlotEditAction.None;
        public string WaferIdResult { get; private set; } = string.Empty;
        public MaterialProcessSatate? ProcessStateResult { get; private set; } = null; // 추후 확장용

        private FormWaferSlotEdit(int slotNumber1, MaterialWafer wafer)
        {
            _slotNumber1 = slotNumber1;
            _currentWafer = wafer;

            Text = $"Slot {_slotNumber1} 편집";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(360, 190);

            InitializeUi();
            LoadCurrent();
        }

        public static bool TryShow(IWin32Window owner, int slotNumber1, MaterialWafer wafer,
            out SlotEditAction action, out string waferId, out MaterialProcessSatate? state)
        {
            using (var f = new FormWaferSlotEdit(slotNumber1, wafer))
            {
                var dr = f.ShowDialog(owner);
                action = f.ActionResult;
                waferId = f.WaferIdResult;
                state = f.ProcessStateResult;
                return dr == DialogResult.OK && action != SlotEditAction.None;
            }
        }

        private void InitializeUi()
        {
            rbAdd = new RadioButton { Left = 12, Top = 12, Width = 160, Text = "추가/수정(Exist)" };
            rbDelete = new RadioButton { Left = 200, Top = 12, Width = 140, Text = "삭제(NotExist)" };

            var lblId = new Label { Left = 12, Top = 50, Width = 80, Text = "Wafer ID" };
            txtWaferId = new TextBox { Left = 100, Top = 46, Width = 240 };

            var lblState = new Label { Left = 12, Top = 82, Width = 80, Text = "상태" };
            cbState = new ComboBox
            {
                Left = 100,
                Top = 78,
                Width = 240,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cbState.Items.Add(MaterialProcessSatate.Unknown);
            cbState.Items.Add(MaterialProcessSatate.Ready);
            cbState.Items.Add(MaterialProcessSatate.Processing);
            cbState.Items.Add(MaterialProcessSatate.Completed);

            btnOk = new Button { Left = 180, Top = 130, Width = 75, Text = "OK", DialogResult = DialogResult.OK };
            btnCancel = new Button { Left = 265, Top = 130, Width = 75, Text = "Cancel", DialogResult = DialogResult.Cancel };

            btnOk.Click += (s, e) =>
            {
                if (rbAdd.Checked)
                {
                    ActionResult = SlotEditAction.AddOrUpdate;
                    WaferIdResult = (txtWaferId.Text ?? string.Empty).Trim();
                    ProcessStateResult = (MaterialProcessSatate?)cbState.SelectedItem;
                }
                else if (rbDelete.Checked)
                {
                    ActionResult = SlotEditAction.Delete;
                    WaferIdResult = string.Empty;
                    ProcessStateResult = MaterialProcessSatate.Unknown;
                }
                else
                {
                    ActionResult = SlotEditAction.None;
                }

                if (ActionResult == SlotEditAction.AddOrUpdate && string.IsNullOrWhiteSpace(WaferIdResult))
                {
                    MessageBox.Show(this, "Wafer ID를 입력하세요.", "확인", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    DialogResult = DialogResult.None;
                }
            };

            Controls.Add(rbAdd);
            Controls.Add(rbDelete);
            Controls.Add(lblId);
            Controls.Add(txtWaferId);
            Controls.Add(lblState);
            Controls.Add(cbState);
            Controls.Add(btnOk);
            Controls.Add(btnCancel);

            AcceptButton = btnOk;
            CancelButton = btnCancel;

            rbAdd.CheckedChanged += (s, e) => UpdateEnabled();
            rbDelete.CheckedChanged += (s, e) => UpdateEnabled();
        }

        private void LoadCurrent()
        {
            bool isPresent = _currentWafer != null && _currentWafer.Presence == MaterialPresence.Exist;
            rbAdd.Checked = true;
            rbDelete.Checked = false;

            txtWaferId.Text = _currentWafer?.WaferId ?? string.Empty;

            var st = _currentWafer?.ProcessSatate ?? MaterialProcessSatate.Unknown;
            cbState.SelectedItem = st;

            UpdateEnabled();
        }

        private void UpdateEnabled()
        {
            bool add = rbAdd.Checked;
            txtWaferId.Enabled = add;
            cbState.Enabled = add;
        }
    }
}