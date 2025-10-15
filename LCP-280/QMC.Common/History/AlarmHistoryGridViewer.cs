using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace QMC.Common.History
{
    public partial class AlarmHistoryGridViewer : UserControl
    {
        private bool initComplete = false;
        private List<AlarmHistory> currentAlarms = new List<AlarmHistory>();
        private List<AlarmHistory> filteredAlarms = new List<AlarmHistory>();

        private string currentTypeFilter = "All";
        private DateTime currentDate = DateTime.Today;
        private int pageSize = 10;
        private int currentPage = 1;
        private string searchKeyword = string.Empty;

        public AlarmHistoryGridViewer()
        {
            InitializeComponent();
            HistoryManager.Instance.OnAddAlarmHistory += Instance_OnAddAlarmHistory;
        }

        private void AlarmHistoryGridViewer_Load(object sender, EventArgs e)
        {
            if (!initComplete)
            {
                // 페이지 크기 기본값 설정
                cmbPageSize.SelectedIndex = 3; // 0: 10개, 3: 100개 

                // DateTimePicker 오늘 날짜로 설정
                dtpDateFilter.Value = DateTime.Today;
                chkEnableDateFilter.Checked = true;

                // 알람 있는 날짜 목록 로드
                LoadRecentAlarmDates();

                // 오늘 날짜 데이터 로드
                LoadAlarmsByDate(DateTime.Today);

                initComplete = true;
            }
        }

        private void Instance_OnAddAlarmHistory(object sender, AlarmHistory e)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action(() => AddRealtimeAlarm(e)));
            }
            else
            {
                AddRealtimeAlarm(e);
            }
        }

        private void AddRealtimeAlarm(AlarmHistory history)
        {
            if (chkEnableDateFilter.Checked && history.Info.GeneratedTime.Date == currentDate.Date)
            {
                currentAlarms.Insert(0, history);
                UpdateStatistics();
                ApplyFilters();
            }
            else
            {
                HistoryManager.Instance.ClearCacheByDate(history.Info.GeneratedTime.Date);
                LoadRecentAlarmDates();
            }
        }

        private void LoadAlarmsByDate(DateTime date)
        {
            currentDate = date;
            currentAlarms = HistoryManager.Instance.LoadAlarmHistoryByDate(date);
            UpdateStatistics();
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            filteredAlarms = currentAlarms.Where(a =>
            {
                if (currentTypeFilter != "All" && a.Info.Grade != currentTypeFilter)
                    return false;

                if (!string.IsNullOrWhiteSpace(searchKeyword))
                {
                    string keyword = searchKeyword.ToLower();
                    bool match = false;

                    if (a.Info.Source != null && a.Info.Source.ToLower().Contains(keyword))
                        match = true;
                    else if (a.Info.Title != null && a.Info.Title.ToLower().Contains(keyword))
                        match = true;
                    else if (a.Info.Cause != null && a.Info.Cause.ToLower().Contains(keyword))
                        match = true;
                    else if (a.Info.Code.ToString().Contains(keyword))
                        match = true;

                    if (!match)
                        return false;
                }

                return true;
            }).ToList();

            int totalPages = (int)Math.Ceiling(filteredAlarms.Count / (double)pageSize);
            if (currentPage > totalPages && totalPages > 0)
                currentPage = totalPages;
            if (currentPage < 1)
                currentPage = 1;

            UpdateGrid();
            UpdatePaginationInfo();
        }

        private void UpdateGrid()
        {
            dataGridView1.Rows.Clear();

            int startIndex = (currentPage - 1) * pageSize;
            int endIndex = Math.Min(startIndex + pageSize, filteredAlarms.Count);

            for (int i = startIndex; i < endIndex; i++)
            {
                var history = filteredAlarms[i];
                int rowIndex = dataGridView1.Rows.Add();
                var row = dataGridView1.Rows[rowIndex];

                row.Cells["Time"].Value = history.Info.GeneratedTime.ToString("yyyy-MM-dd HH:mm:ss");
                row.Cells["AlarmType"].Value = history.Info.Grade;
                row.Cells["AlarmCode"].Value = history.Info.Code;
                row.Cells["AlarmSource"].Value = history.Info.Source;
                row.Cells["AlarmTitle"].Value = history.Info.Title;
                row.Cells["AlarmCause"].Value = history.Info.Cause;

                if (history.Info.Grade == "Error")
                    row.DefaultCellStyle.BackColor = Color.FromArgb(255, 240, 240);
                else if (history.Info.Grade == "Warning")
                    row.DefaultCellStyle.BackColor = Color.FromArgb(255, 250, 230);
            }
        }

        private void UpdateStatistics()
        {
            int total = currentAlarms.Count;
            int errorCount = currentAlarms.Count(a => a.Info.Grade == "Error");
            int warningCount = currentAlarms.Count(a => a.Info.Grade == "Warning");

            lblTotal.Text = $"Total: {total}";
            lblError.Text = $"Error: {errorCount}";
            lblWarning.Text = $"Warning: {warningCount}";
        }

        private void UpdatePaginationInfo()
        {
            int totalPages = (int)Math.Ceiling(filteredAlarms.Count / (double)pageSize);
            int startRow = filteredAlarms.Count == 0 ? 0 : (currentPage - 1) * pageSize + 1;
            int endRow = Math.Min(currentPage * pageSize, filteredAlarms.Count);

            lblPagination.Text = $"{startRow}-{endRow} / {filteredAlarms.Count}";

            btnPrevPage.Enabled = currentPage > 1;
            btnNextPage.Enabled = currentPage < totalPages;
            btnFirstPage.Enabled = currentPage > 1;
            btnLastPage.Enabled = currentPage < totalPages;

            lblCurrentPage.Text = $"Page {currentPage} / {Math.Max(1, totalPages)}";
        }

        private void btnFilterAll_Click(object sender, EventArgs e)
        {
            currentTypeFilter = "All";
            currentPage = 1;
            HighlightFilterButton(btnFilterAll);
            ApplyFilters();
        }

        private void btnFilterError_Click(object sender, EventArgs e)
        {
            currentTypeFilter = "Error";
            currentPage = 1;
            HighlightFilterButton(btnFilterError);
            ApplyFilters();
        }

        private void btnFilterWarning_Click(object sender, EventArgs e)
        {
            currentTypeFilter = "Warning";
            currentPage = 1;
            HighlightFilterButton(btnFilterWarning);
            ApplyFilters();
        }

        private void HighlightFilterButton(Button activeButton)
        {
            btnFilterAll.BackColor = SystemColors.Control;
            btnFilterError.BackColor = SystemColors.Control;
            btnFilterWarning.BackColor = SystemColors.Control;

            activeButton.BackColor = Color.FromArgb(0, 120, 215);
            activeButton.ForeColor = Color.White;

            foreach (Button btn in new[] { btnFilterAll, btnFilterError, btnFilterWarning })
            {
                if (btn != activeButton)
                    btn.ForeColor = Color.Black;
            }
        }

        private void dtpDateFilter_ValueChanged(object sender, EventArgs e)
        {
            if (chkEnableDateFilter.Checked)
            {
                currentPage = 1;
                LoadAlarmsByDate(dtpDateFilter.Value.Date);
            }
        }

        private void chkEnableDateFilter_CheckedChanged(object sender, EventArgs e)
        {
            dtpDateFilter.Enabled = chkEnableDateFilter.Checked;

            if (chkEnableDateFilter.Checked)
            {
                currentPage = 1;
                LoadAlarmsByDate(dtpDateFilter.Value.Date);
            }
            else
            {
                currentPage = 1;
                currentAlarms = HistoryManager.Instance.LoadRecentAlarmHistory(7);
                currentDate = DateTime.MinValue;
                UpdateStatistics();
                ApplyFilters();
            }
        }

        private void cmbPageSize_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbPageSize.SelectedItem != null)
            {
                pageSize = int.Parse(cmbPageSize.SelectedItem.ToString());
                currentPage = 1;
                ApplyFilters();
            }
        }

        private void btnPrevPage_Click(object sender, EventArgs e)
        {
            if (currentPage > 1)
            {
                currentPage--;
                UpdateGrid();
                UpdatePaginationInfo();
            }
        }

        private void btnNextPage_Click(object sender, EventArgs e)
        {
            int totalPages = (int)Math.Ceiling(filteredAlarms.Count / (double)pageSize);
            if (currentPage < totalPages)
            {
                currentPage++;
                UpdateGrid();
                UpdatePaginationInfo();
            }
        }

        private void btnFirstPage_Click(object sender, EventArgs e)
        {
            currentPage = 1;
            UpdateGrid();
            UpdatePaginationInfo();
        }

        private void btnLastPage_Click(object sender, EventArgs e)
        {
            int totalPages = (int)Math.Ceiling(filteredAlarms.Count / (double)pageSize);
            if (totalPages > 0)
            {
                currentPage = totalPages;
                UpdateGrid();
                UpdatePaginationInfo();
            }
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            if (txtSearch == null) return;

            searchKeyword = txtSearch.Text.Trim();
            currentPage = 1;
            ApplyFilters();
        }

        private void btnClearSearch_Click(object sender, EventArgs e)
        {
            if (txtSearch != null)
            {
                txtSearch.Text = string.Empty;
            }
            searchKeyword = string.Empty;
            currentPage = 1;
            ApplyFilters();
        }

        private void LoadRecentAlarmDates()
        {
            string logFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AlarmLog");
            if (!Directory.Exists(logFolder))
                return;

            cmbRecentDates.Items.Clear();
            cmbRecentDates.Items.Add("-- 날짜 선택 --");

            var files = Directory.GetFiles(logFolder, "AlarmLog_*.csv")
                .OrderByDescending(f => f)
                .Take(30);

            foreach (var file in files)
            {
                try
                {
                    string fileName = Path.GetFileNameWithoutExtension(file);
                    string dateStr = fileName.Replace("AlarmLog_", "");

                    if (DateTime.TryParseExact(dateStr, "yyyyMMdd", null,
                        System.Globalization.DateTimeStyles.None, out DateTime date))
                    {
                        var lines = File.ReadAllLines(file, Encoding.UTF8);
                        int count = lines.Where(l => !string.IsNullOrWhiteSpace(l)).Count();

                        if (count > 0)
                        {
                            cmbRecentDates.Items.Add($"{date:yyyy-MM-dd} ({count}개)");
                        }
                    }
                }
                catch { }
            }

            cmbRecentDates.SelectedIndex = 0;
        }

        private void cmbRecentDates_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbRecentDates.SelectedIndex > 0 && cmbRecentDates.SelectedItem != null)
            {
                string selected = cmbRecentDates.SelectedItem.ToString();
                string dateStr = selected.Substring(0, 10);

                if (DateTime.TryParse(dateStr, out DateTime date))
                {
                    dtpDateFilter.Value = date;
                    chkEnableDateFilter.Checked = true;
                    LoadAlarmsByDate(date);
                }
            }
        }
    }
}