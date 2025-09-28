using QMC.Common;
using QMC.Common.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QMC.LCP_280.Process.Unit.FormMain
{
    public partial class SequenceAutoControl : UserControl
    {
        public class ItemEventArgs : EventArgs
        {
            public string status { get; set; } //임시 작업, 변경 필요
            public string sequenceName { get; set; } //임시 작업, 변경 필요
        }

        public event EventHandler<ItemEventArgs> SequenceButtonRequested;

        public SequenceAutoControl()
        {
            InitializeComponent();
        }

        private void btn_Auto_Ready_Click(object sender, EventArgs e)

        {
            var ask = new MessageBoxYesNo();
            if (ask.ShowDialog("Aotu Sequence Ready", "Auto Sequence Ready 진행하시겠습니까?") == DialogResult.Yes)
            {
                // 모터 이동 이벤트 발생
                SequenceButtonRequested?.Invoke(this, new ItemEventArgs
                {
                    status = "Ready",
                    sequenceName = "Ready"
                });
            }
        }

        private void btn_Auto_Start_Click(object sender, EventArgs e)
        {
            var ask = new MessageBoxYesNo();
            if (ask.ShowDialog("Aotu Sequence Start", "Auto Sequence Start 진행하시겠습니까?") == DialogResult.Yes)
            {
                // 모터 이동 이벤트 발생
                SequenceButtonRequested?.Invoke(this, new ItemEventArgs
                {
                    status = "Start",
                    sequenceName = "Start"
                });
            }
        }

        private void btn_Auto_Stop_Click(object sender, EventArgs e)
        {
            var ask = new MessageBoxYesNo();
            if (ask.ShowDialog("Aotu Sequence Stop", "Auto Sequence Stop 진행하시겠습니까?") == DialogResult.Yes)
            {
                // 모터 이동 이벤트 발생
                SequenceButtonRequested?.Invoke(this, new ItemEventArgs
                {
                    status = "Stop",
                    sequenceName = "Stop"
                });
            }
        }

        private void btn_Auto_CycleStop_Click(object sender, EventArgs e)
        {
            var ask = new MessageBoxYesNo();
            if (ask.ShowDialog("Aotu Sequence Ready", "Auto Sequence CycleStop 진행하시겠습니까?") == DialogResult.Yes)
            {
                // 모터 이동 이벤트 발생
                SequenceButtonRequested?.Invoke(this, new ItemEventArgs
                {
                    status = "CycleStop",
                    sequenceName = "CycleStop"
                });
            }
        }

        private void btn_Auto_Reset_Click(object sender, EventArgs e)
        {
            var ask = new MessageBoxYesNo();
            if (ask.ShowDialog("Aotu Sequence Reset", "Auto Sequence Reset 진행하시겠습니까?") == DialogResult.Yes)
            {
                // 모터 이동 이벤트 발생
                SequenceButtonRequested?.Invoke(this, new ItemEventArgs
                {
                    status = "Reset",
                    sequenceName = "Reset"
                });
            }
        }
    }
}
