using QMC.LCP_280.Process.Work;
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
    public partial class OutputWaferCarrierControl : UserControl
    {
        public OutputWaferCarrierControl()
        {
            InitializeComponent();
        }


        public Component.WaferSelectMapView GetWaferSelectMapView()
        {
            return waferSelectMapView;
        }

        // UI 스레드 실행 헬퍼
        private void RunOnUI(Action action)
        {
            if (action == null) return;
            if (IsDisposed) return;
            if (!IsHandleCreated)
            {
                // 아직 Handle 미생성일 경우 Load 이후로 미루고 싶으면 필요 시 큐에 넣는 로직 추가 가능
                return;
            }

            if (InvokeRequired)
            {
                try { BeginInvoke(action); } catch { /* Dispose 중 */ }
            }
            else
            {
                action();
            }
        }

        public void SetWaferCarrierId(string id)
        {
            RunOnUI(() =>
            {
                if (lblWaferIdValue != null && !lblWaferIdValue.IsDisposed)
                    lblWaferIdValue.Text = id ?? string.Empty;
            });
        }

        public void UpdateWaferCount(int count)
        {
            RunOnUI(() =>
            {
                if (lblWaferCountValue != null && !lblWaferCountValue.IsDisposed)
                    lblWaferCountValue.Text = count.ToString();
            });
        }
    }
}
