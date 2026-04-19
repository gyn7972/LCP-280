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
using System.Windows.Forms.DataVisualization.Charting;

namespace QMC.LCP_280.Process.Component
{
    public partial class TaktMonitorDialog : Form
    {
        private readonly CycleTimer _timer;
        private readonly string _title;

        private readonly System.Windows.Forms.Timer _uiTimer = new System.Windows.Forms.Timer();
        private bool _refreshing;

        public TaktMonitorDialog(string title, CycleTimer timer)
        {
            InitializeComponent();

            _title = title ?? "Takt Monitor";
            _timer = timer;

            // ---- Auto Refresh Timer ----
            _uiTimer.Interval = 200;
            _uiTimer.Tick += UiTimer_Tick;
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            RefreshView();
            _uiTimer.Start();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            try { _uiTimer.Stop(); } catch { }
            try { _uiTimer.Tick -= UiTimer_Tick; } catch { }
            try { _uiTimer.Dispose(); } catch { }
            base.OnFormClosed(e);
        }

        private void UiTimer_Tick(object sender, EventArgs e)
        {
            // UI Timer는 UI Thread에서 호출되지만, Tick이 밀릴 수 있으니 재진입 방지
            if (_refreshing)
                return;

            try
            {
                _refreshing = true;
                RefreshView();
            }
            finally
            {
                _refreshing = false;
            }
        }

        public void RefreshView()
        {
            if (_timer == null)
                return;

            try
            {
                var cycleTimes = _timer.CycleTimes;
                int count = cycleTimes?.Count ?? 0;

                lblCount.Text = $"Count: {count}";

                if (count <= 0)
                {
                    lblLatest.Text = "Latest: -";
                    lblAvg.Text = "Avg: -";
                    lblMin.Text = "Min: -";
                    lblMax.Text = "Max: -";
                    grid.DataSource = null;
                    return;
                }

                var latest = _timer.Latest;

                lblLatest.Text = $"Latest: {latest.Interval.TotalMilliseconds:0.0} ms";
                lblAvg.Text = $"Avg: {_timer.Average.TotalMilliseconds:0.0} ms";
                lblMin.Text = $"Min: {_timer.Minimum.TotalMilliseconds:0.0} ms";
                lblMax.Text = $"Max: {_timer.Maximum.TotalMilliseconds:0.0} ms";

                // Grid는 매번 DataTable을 새로 만들어 교체(간단/안전)
                var dt = new DataTable();
                dt.Columns.Add("Start");
                dt.Columns.Add("End");
                dt.Columns.Add("IntervalMs");

                foreach (var c in cycleTimes)
                {
                    dt.Rows.Add(
                        c.Start.ToString("HH:mm:ss.fff"),
                        c.End.ToString("HH:mm:ss.fff"),
                        c.Interval.TotalMilliseconds.ToString("0.0"));
                }

                grid.DataSource = dt;
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            RefreshView();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
