using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using QMC.Common.Controls;   // 공용 DisplayView 사용

namespace QMC.LCP_280.Process.Unit.FormMain
{
    public partial class DieInputControl : UserControl
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

        private List<Die> _dies = new List<Die>();

        public DieInputControl()
        {
            InitializeComponent();
        }

        public void SetWaferId(string waferId)
        {
            lblWaferIdValue.Text = waferId;
        }

        private void UpdateDieCount()
        {
            int count = _dies.Count(d => d.State == DieState.Present);
            lblDieCountValue.Text = count.ToString();
        }

        public void SetDieList(List<Die> dies)
        {
            _dies = dies ?? new List<Die>();
            UpdateDieCount();

            // 공용 DisplayView 아이템 변환
            var items = _dies.Select(d => new DisplayView.DisplayItem
            {
                Position = d.Position,
                State = (DisplayView.ItemState)Enum.Parse(typeof(DisplayView.ItemState), d.State.ToString())
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
