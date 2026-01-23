using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QMC.LCP_280.Process.Component.FormDlg
{
    public partial class WaferTotalSummaryViewerForm : Form
    {
        private WaferTotalSummaryViewerController _controller;

        public WaferTotalSummaryViewerForm()
        {
            InitializeComponent();

            // 컨트롤러가 UI 제어를 전담
            _controller = new WaferTotalSummaryViewerController(gridSummary, lblStatus, cmbSource);

            // Form lifecycle만 위임 (디자이너에 로직 X)
            Load += (s, e) => _controller.Start();
            FormClosing += (s, e) => _controller.Stop();

            // 버튼 이벤트도 여기서만(디자이너에 로직 X)
            btnStart.Click += (s, e) => _controller.Start();
            btnStop.Click += (s, e) => _controller.Stop();
        }
    }
}
