using QMC.Common;
using QMC.LCP_280.Process.Unit;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Reflection;
using System.Windows.Forms;

namespace QMC.LCP_280.Process.Component.FormDlg
{
    public partial class DlgManualSequence : Form
    {
        private readonly bool[] _socketHasProduct = new bool[8];
        private readonly InputDieTransfer _inputDieTransfer;
        private readonly Rotary _index;
        private readonly OutputDieTransfer _outputDieTransfer;

        private readonly IndexLoadAligner _indexLoadAligner;
        private readonly IndexChipProbeController _indexProbeController;

        private readonly Timer _socketRefreshTimer = new Timer();

        public DlgManualSequence()
        {
            InitializeComponent();

            _inputDieTransfer = Equipment.Instance?.GetUnit(Equipment.UnitKeys.InputDieTransfer) as InputDieTransfer;
            _index = Equipment.Instance?.GetUnit(Equipment.UnitKeys.Rotary) as Rotary;
            _outputDieTransfer = Equipment.Instance?.GetUnit(Equipment.UnitKeys.OutputDieTransfer) as OutputDieTransfer;

            _indexLoadAligner = Equipment.Instance?.GetUnit(Equipment.UnitKeys.IndexLoadAligner) as IndexLoadAligner;
            _indexProbeController = Equipment.Instance?.GetUnit(Equipment.UnitKeys.IndexChipProbeController) as IndexChipProbeController;

            InitializeIndexSocketList();
            InitializeManualSequence();
            InitializeSocketRefreshTimer();
        }
        private void InitializeSocketRefreshTimer()
        {
            _socketRefreshTimer.Interval = 200;
            _socketRefreshTimer.Tick += SocketRefreshTimer_Tick;
        }
        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            StartSocketRefresh();
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);

            if (Visible)
            {
                StartSocketRefresh();
            }
            else
            {
                StopSocketRefresh();
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            StopSocketRefresh();
            _socketRefreshTimer.Tick -= SocketRefreshTimer_Tick;
            base.OnFormClosed(e);
        }
        private void StartSocketRefresh()
        {
            SyncSocketStatesFromRotary();
            _socketRefreshTimer.Start();
        }

        private void StopSocketRefresh()
        {
            _socketRefreshTimer.Stop();
        }

        private void SocketRefreshTimer_Tick(object sender, EventArgs e)
        {
            SyncSocketStatesFromRotary();
        }

        private void SyncSocketStatesFromRotary()
        {
            if (_index == null)
            {
                return;
            }

            int count = Math.Min(_socketHasProduct.Length, _index.GetIndexCount());

            for (int i = 0; i < count; i++)
            {
                bool hasProduct = false;

                var socket = _index.GetSocket(i);
                if (socket != null)
                {
                    var die = socket.GetMaterialDie();
                    hasProduct = die != null && die.Presence == Material.MaterialPresence.Exist;
                }

                if (_socketHasProduct[i] != hasProduct)
                {
                    SetSocketProductState(i + 1, hasProduct);
                }
            }
        }

        private void InitializeManualSequence()
        {
            if (_inputDieTransfer == null || _index == null || _outputDieTransfer == null)
            {
                manualSequenceControl1.Enabled = false;
                return;
            }

            // Unit 등록
            manualSequenceControl1.RegisterUnit("InputTr", _inputDieTransfer);
            manualSequenceControl1.RegisterUnit("Index", _index);
            manualSequenceControl1.RegisterUnit("OutputTr", _outputDieTransfer);

            // Sync Unit 상태 (특히 Index의 소켓 상태) - 수동 시퀀스가 Index의 소켓 상태를 참조하기 때문에 반드시 필요
            manualSequenceControl1.SetStatusSyncUnits(_index, _indexLoadAligner, _indexProbeController);

            // Step 등록 (Unit + Method)
            manualSequenceControl1.SetSteps(new[]
            {
                ManualSequenceControl.ManualStep.Create<InputDieTransfer>(
                    "InputTr-PickDie", "InputTr", u => u.InputTrDiePick(),
                    () => lstIndexSocket.SelectedIndex < 0 ? 0 : lstIndexSocket.SelectedIndex),

                ManualSequenceControl.ManualStep.Create<InputDieTransfer>(
                    "InputTr-Place", "InputTr", (u, idx) => u.InputTrDiePlace(idx, false),
                    () => lstIndexSocket.SelectedIndex < 0 ? 0 : lstIndexSocket.SelectedIndex),

                ManualSequenceControl.ManualStep.Create<Rotary>(
                    "Index-MechaAlign", "Index", (u, idx) => u.RunMechaAlign(idx, false),
                    () => lstIndexSocket.SelectedIndex < 0 ? 0 : lstIndexSocket.SelectedIndex),

                ManualSequenceControl.ManualStep.Create<Rotary>(
                    "Index-Probe", "Index", (u, idx) => u.RunProbeInspection(idx, false),
                    () => lstIndexSocket.SelectedIndex < 0 ? 0 : lstIndexSocket.SelectedIndex),

                ManualSequenceControl.ManualStep.Create<OutputDieTransfer>(
                    "OutputTr-PickDie", "OutputTr", (u, idx) => u.OutputTrDiePick(idx, false),
                    () => lstIndexSocket.SelectedIndex < 0 ? 0 : lstIndexSocket.SelectedIndex),

                 ManualSequenceControl.ManualStep.Create<OutputDieTransfer>(
                    "OutputTr-Place", "OutputTr", (u, idx) => u.OutputTrDiePlace(),
                    () => lstIndexSocket.SelectedIndex < 0 ? 0 : lstIndexSocket.SelectedIndex)
            });
        }

        private void InitializeIndexSocketList()
        {
            lstIndexSocket.DrawMode = DrawMode.OwnerDrawFixed;
            lstIndexSocket.BorderStyle = BorderStyle.None;
            lstIndexSocket.BackColor = Color.FromArgb(246, 246, 248);
            lstIndexSocket.ForeColor = Color.FromArgb(28, 28, 30);
            lstIndexSocket.Font = new Font("Segoe UI", 10.5F, FontStyle.Bold);
            lstIndexSocket.ItemHeight = 30;

            var dbProp = typeof(Control).GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic);
            if (dbProp != null)
            {
                dbProp.SetValue(lstIndexSocket, true, null);
            }

            lstIndexSocket.Items.Clear();

            for (int i = 1; i <= 8; i++)
            {
                lstIndexSocket.Items.Add("Index " + i);
            }

            lstIndexSocket.SelectedIndex = 0;
            SyncSocketStatesFromRotary();
            UpdateSelectedIndexLabel();
            lstIndexSocket.Invalidate();
        }

        public void SetSocketProductState(int index, bool hasProduct)
        {
            if (index < 1 || index > 8)
            {
                return;
            }

            _socketHasProduct[index - 1] = hasProduct;
            lstIndexSocket.Invalidate();
            UpdateSelectedIndexLabel();
        }

        private void lstIndexSocket_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateSelectedIndexLabel();
            lstIndexSocket.Invalidate();
        }

        private void UpdateSelectedIndexLabel()
        {
            if (lstIndexSocket.SelectedIndex < 0)
            {
                lblSelectedIndex.Text = "INDEX : -";
                lblSelectedIndex.ForeColor = Color.OrangeRed;
                return;
            }

            int selectedIndex = lstIndexSocket.SelectedIndex + 1;
            bool hasProduct = _socketHasProduct[lstIndexSocket.SelectedIndex];

            lblSelectedIndex.Text = "INDEX : " + selectedIndex;
            lblSelectedIndex.ForeColor = hasProduct ? Color.Lime : Color.White;
        }

        private void lstIndexSocket_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0 || e.Index >= lstIndexSocket.Items.Count)
            {
                return;
            }

            // 선택 상태를 최우선으로 판정
            bool isSelected = e.Index == lstIndexSocket.SelectedIndex;
            bool hasProduct = _socketHasProduct[e.Index];

            Rectangle itemBounds = new Rectangle(e.Bounds.X + 5, e.Bounds.Y + 3, e.Bounds.Width - 10, e.Bounds.Height - 6);

            Color fillColor;
            Color borderColor;
            Color textColor;

            if (isSelected)
            {
                // 1순위: 선택 강조 (항상 이 색 우선)
                fillColor = Color.FromArgb(0, 122, 255);
                borderColor = Color.FromArgb(0, 92, 204);
                textColor = Color.White;
            }
            else if (hasProduct)
            {
                fillColor = Color.FromArgb(52, 199, 89);
                borderColor = Color.FromArgb(36, 160, 74);
                textColor = Color.White;
            }
            else
            {
                fillColor = Color.White;
                borderColor = Color.FromArgb(220, 220, 225);
                textColor = Color.FromArgb(28, 28, 30);
            }

            SmoothingMode oldMode = e.Graphics.SmoothingMode;
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            using (GraphicsPath path = CreateRoundedRectanglePath(itemBounds, 10))
            using (SolidBrush fillBrush = new SolidBrush(fillColor))
            using (Pen borderPen = new Pen(borderColor, isSelected ? 2.4F : 1.2F))
            {
                e.Graphics.FillPath(fillBrush, path);
                e.Graphics.DrawPath(borderPen, path);
            }

            // 선택 인지성 강화: 좌측 강조바
            if (isSelected)
            {
                using (SolidBrush accent = new SolidBrush(Color.White))
                {
                    e.Graphics.FillRectangle(accent, itemBounds.X + 6, itemBounds.Y + 6, 5, itemBounds.Height - 12);
                }
            }

            e.Graphics.SmoothingMode = oldMode;

            string text = lstIndexSocket.Items[e.Index].ToString();
            if (isSelected)
            {
                text = "✓" + text;
            }
            else if (hasProduct)
            {
                text = "● " + text;
            }

            Rectangle textRect = new Rectangle(itemBounds.X + 18, itemBounds.Y, itemBounds.Width - 22, itemBounds.Height);
            TextRenderer.DrawText(
                e.Graphics,
                text,
                lstIndexSocket.Font,
                textRect,
                textColor,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        }

        private static GraphicsPath CreateRoundedRectanglePath(Rectangle bounds, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            int d = radius * 2;

            path.AddArc(bounds.X, bounds.Y, d, d, 180, 90);
            path.AddArc(bounds.Right - d, bounds.Y, d, d, 270, 90);
            path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - d, d, d, 90, 90);
            path.CloseFigure();

            return path;
        }
    }
}
