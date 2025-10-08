using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using QMC.Common.Controls;   // 공용 DisplayView 사용
using QMC.LCP_280.Process.Component; // MaterialChip, ChipProcessState

namespace QMC.LCP_280.Process.Unit.FormMain
{
    public partial class DieInputControl : UserControl
    {
        public event EventHandler<DisplayView.DisplayItemEventArgs> MotorMoveRequested;

        private List<MaterialDie> _chips = new List<MaterialDie>();

        public DieInputControl()
        {
            InitializeComponent();
            displayView1.MotorMoveRequested += OnDisplayView_MotorMoveRequested;
        }

        private void OnDisplayView_MotorMoveRequested(object sender, DisplayView.DisplayItemEventArgs e)
        {
            MotorMoveRequested?.Invoke(this, e);
        }

        public void SetWaferId(string waferId)
        {
            lblWaferIdValue.Text = waferId;
        }

        private void UpdateDieCount()
        {
            if(this.InvokeRequired)
            {
                this.Invoke(new Action(() => UpdateDieCount()));
                return;
            }
            // Present 개수 = Mapped 상태 칩 수 (필요 시 Exists 로 변경 가능)
            int count = _chips.Count(c => c.State == DieProcessState.Mapped || c.State == DieProcessState.Picked);
            lblDieCountValue.Text = count.ToString();
        }

        // 이름 호환성을 위해 SetDieList 유지 (MaterialChip 사용)
        public void SetDieList(List<MaterialDie> chips)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => SetDieList(chips)));
                return;
            }

            _chips = chips ?? new List<MaterialDie>();
            UpdateDieCount();

            var items = _chips.Select(c => new DisplayView.DisplayItem
            {
                Position = new Point(c.MapX, c.MapY),
                State = ConvertState(c.State)
            }).ToList();

            displayView1.SetItems(items);
            displayView1.Refresh();
        }

        private DisplayView.ItemState ConvertState(DieProcessState state)
        {
            // DisplayView.ItemState 는 Empty/Present/Picked 라고 가정
            switch (state)
            {
                case DieProcessState.Picked: return DisplayView.ItemState.Picked;
                case DieProcessState.Mapped:
                case DieProcessState.Inspecting:
                case DieProcessState.Inspected:
                case DieProcessState.Placed:
                case DieProcessState.Rejected:
                    return DisplayView.ItemState.Present;
                default:
                    return DisplayView.ItemState.Empty;
            }
        }

        public void UpdateChip(Point mapCoord, DieProcessState state)
        {
            var chip = _chips.FirstOrDefault(c => c.MapX == mapCoord.X && c.MapY == mapCoord.Y);
            if (chip != null)
            {
                chip.State = state;
                UpdateDieCount();
                // Refresh view
                SetDieList(_chips);
            }
        }
    }
}
