
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using QMC.Common.Controls;   // 공용 DisplayView 사용

namespace QMC.LCP_280.Process.Unit.FormMain
{
    public partial class DieOutputControl : UserControl
    {
        public enum DieState
        {
            Empty,
            Present,
            Picked
        }

        public class Die
        {
            public int Index { get; set; }
            public Point Position { get; set; }
            public DieState State { get; set; }
        }

        public event EventHandler<DisplayView_DieOutputControl.DisplayItemEventArgs> MotorMoveRequested;

        private List<Die> _dies = new List<Die>();

        public DieOutputControl()
        {
            InitializeComponent();
            displayView1.MotorMoveRequested += OnDisplayView_MotorMoveRequested;
        }

        private void OnDisplayView_MotorMoveRequested(object sender, DisplayView_DieOutputControl.DisplayItemEventArgs e)
        {
            MotorMoveRequested?.Invoke(this, e);
        }

        public void SetWaferId(string waferId)
        {
            lblWaferIdValue.Text = waferId;
        }

        private void UpdateDieCount()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => UpdateDieCount()));
                return;
            }

            // Present 개수 = Mapped 상태 칩 수 (필요 시 Exists 로 변경 가능)
            int count = _dies.Count(d => d.State == DieState.Present);
            lblDieCountValue.Text = count.ToString();
        }

        public void SetDieList(List<Die> dies)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => SetDieList(dies)));
                return;
            }

            _dies = dies ?? new List<Die>();
            UpdateDieCount();

            // 공용 DisplayView 아이템 변환
            var items = _dies.Select(d => new DisplayView_DieOutputControl.DisplayItem
            {
                Position = d.Position,
                State = (DisplayView_DieOutputControl.ItemState)Enum.Parse(typeof(DisplayView_DieOutputControl.ItemState), d.State.ToString())
            }).ToList();

            displayView1.SetItems(items);
        }

        public void UpdateDie(Point coord, DieState state)
        {
            var die = _dies.FirstOrDefault(d => d.Position == coord);
            if (die != null)
            {
                die.State = state;
                UpdateDieCount();

                SetDieList(_dies); // 다시 반영
            }
        }
    }
}
