using QMC.Common;
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
    public partial class SequenceManualControl : UserControl
    {
        public class ItemEventArgs : EventArgs
        {
            public string status { get; set; } //임시 작업, 변경 필요
            public string sequenceName { get; set; } //임시 작업, 변경 필요
        }

        public event EventHandler<ItemEventArgs> SequenceButtonRequested;


        public SequenceManualControl()
        {
            InitializeComponent();
        }

        private void btn_Ready_InputWafer_Click(object sender, EventArgs e)
        {
            var ask = new MessageBoxYesNo();
            if (ask.ShowDialog("Manual InputWafer Ready", "Manual InputWafer Ready 진행하시겠습니까?") == DialogResult.Yes)
            {
                // 모터 이동 이벤트 발생
                SequenceButtonRequested?.Invoke(this, new ItemEventArgs
                {
                    status = "Ready",
                    sequenceName = "InputWafer"
                });
            }
        }

        private void btn_Ready_ChipLoading_Click(object sender, EventArgs e)
        {
            var ask = new MessageBoxYesNo();
            if (ask.ShowDialog("Manual ChipLoading Ready", "Manual ChipLoading Ready 진행하시겠습니까?") == DialogResult.Yes)
            {
                // 모터 이동 이벤트 발생
                SequenceButtonRequested?.Invoke(this, new ItemEventArgs
                {
                    status = "Ready",
                    sequenceName = "ChipLoading"
                });
            }
        }

        private void btn_Ready_Process_Click(object sender, EventArgs e)
        {
            var ask = new MessageBoxYesNo();
            if (ask.ShowDialog("Manual Process Ready", "Manual Process Ready 진행하시겠습니까?") == DialogResult.Yes)
            {
                // 모터 이동 이벤트 발생
                SequenceButtonRequested?.Invoke(this, new ItemEventArgs
                {
                    status = "Ready",
                    sequenceName = "Process"
                });
            }
        }

        private void btn_Ready_ChipUnloading_Click(object sender, EventArgs e)
        {
            var ask = new MessageBoxYesNo();
            if (ask.ShowDialog("Manual ChipUnloading Ready", "Manual ChipUnloading Ready 진행하시겠습니까?") == DialogResult.Yes)
            {
                // 모터 이동 이벤트 발생
                SequenceButtonRequested?.Invoke(this, new ItemEventArgs
                {
                    status = "Ready",
                    sequenceName = "ChipUnloading"
                });
            }
        }

        private void btn_Ready_OutputWafer_Click(object sender, EventArgs e)
        {
            var ask = new MessageBoxYesNo();
            if (ask.ShowDialog("Manual OutputWafer Ready", "Manual OutputWafer Ready 진행하시겠습니까?") == DialogResult.Yes)
            {
                // 모터 이동 이벤트 발생
                SequenceButtonRequested?.Invoke(this, new ItemEventArgs
                {
                    status = "Ready",
                    sequenceName = "OutputWafer"
                });
            }
        }

        private void btn_Start_InputWafer_Click(object sender, EventArgs e)
        {
            var ask = new MessageBoxYesNo();
            if (ask.ShowDialog("Manual InputWafer Start", "Manual InputWafer Start 진행하시겠습니까?") == DialogResult.Yes)
            {
                // 모터 이동 이벤트 발생
                SequenceButtonRequested?.Invoke(this, new ItemEventArgs
                {
                    status = "Start",
                    sequenceName = "InputWafer"
                });
            }
        }

        private void btn_Start_ChipLoading_Click(object sender, EventArgs e)
        {
            var ask = new MessageBoxYesNo();
            if (ask.ShowDialog("Manual ChipLoading Start", "Manual ChipLoading Start 진행하시겠습니까?") == DialogResult.Yes)
            {
                // 모터 이동 이벤트 발생
                SequenceButtonRequested?.Invoke(this, new ItemEventArgs
                {
                    status = "Start",
                    sequenceName = "ChipLoading"
                });
            }
        }

        private void btn_Start_Process_Click(object sender, EventArgs e)
        {
            var ask = new MessageBoxYesNo();
            if (ask.ShowDialog("Manual Process Start", "Manual Process Start 진행하시겠습니까?") == DialogResult.Yes)
            {
                // 모터 이동 이벤트 발생
                SequenceButtonRequested?.Invoke(this, new ItemEventArgs
                {
                    status = "Start",
                    sequenceName = "Process"
                });
            }
        }

        private void btn_Start_ChipUnloading_Click(object sender, EventArgs e)
        {
            var ask = new MessageBoxYesNo();
            if (ask.ShowDialog("Manual ChipUnloading Start", "Manual ChipUnloading Start 진행하시겠습니까?") == DialogResult.Yes)
            {
                // 모터 이동 이벤트 발생
                SequenceButtonRequested?.Invoke(this, new ItemEventArgs
                {
                    status = "Start",
                    sequenceName = "ChipUnloading"
                });
            }
        }

        private void btn_Start_OutputWafer_Click(object sender, EventArgs e)
        {
            var ask = new MessageBoxYesNo();
            if (ask.ShowDialog("Manual OutputWafer Start", "Manual OutputWafer Start 진행하시겠습니까?") == DialogResult.Yes)
            {
                // 모터 이동 이벤트 발생
                SequenceButtonRequested?.Invoke(this, new ItemEventArgs
                {
                    status = "Start",
                    sequenceName = "OutputWafer"
                });
            }
        }
    }
}
