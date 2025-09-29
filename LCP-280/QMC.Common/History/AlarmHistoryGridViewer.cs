using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QMC.Common.History
{
    public partial class AlarmHistoryGridViewer : UserControl
    {
        private bool initComplete = false;

        public AlarmHistoryGridViewer()
        {
            InitializeComponent();
            HistoryManager.Instance.OnAddAlarmHistory += Instance_OnAddAlarmHistory;
        }

        private void AlarmHistoryGridViewer_Load(object sender, EventArgs e)
        {
            if (!initComplete)
            {
                dataGridView1.Rows.Clear();
                HistoryManager.Instance.LoadAlarmHistory();
                initComplete = true;
            }
        }

        private void Instance_OnAddAlarmHistory(object sender, AlarmHistory e)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action(() => AddAlarmHistory(e)));
            }
            else
            {
                AddAlarmHistory(e);
            }
        }

        private void AddAlarmHistory(AlarmHistory history)
        {
            int rowIndex = dataGridView1.Rows.Add();
            if (dataGridView1.Rows.Count > 500)
                dataGridView1.Rows.RemoveAt(0);

            var row = dataGridView1.Rows[rowIndex];
            row.Cells["Time"].Value = history.Info.GeneratedTime.ToString("yyyy-MM-dd HH:mm:ss");
            row.Cells["AlarmType"].Value = history.Info.Grade;
            row.Cells["AlarmCode"].Value = history.Info.Code;
            row.Cells["AlarmSource"].Value = history.Info.Source;
            row.Cells["AlarmTitle"].Value = history.Info.Title;
            row.Cells["AlarmCause"].Value = history.Info.Cause;
            
            // 가장 최근 알람이 위에 오도록 정렬
            dataGridView1.Sort(dataGridView1.Columns[0], ListSortDirection.Descending);
        }
    }
}