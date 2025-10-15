using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
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
                cmbPageSize.SelectedIndex = 0;

                // DateTimePicker 오늘 날짜로 설정
                dtpDateFilter.Value = DateTime.Today;
                chkEnableDateFilter.Checked = true; // 기본적으로 오늘 날짜 필터 활성화

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

        /// <summary>
        /// 실시간 알람 추가 (현재 표시 중인 날짜에만 반영)
        /// </summary>
        private void AddRealtimeAlarm(AlarmHistory history)
        {
            // 현재 날짜 필터에 해당하는 알람만 화면에 추가
            if (chkEnableDateFilter.Checked && history.Info.GeneratedTime.Date == currentDate.Date)
            {
                currentAlarms.Insert(0, history);
                UpdateStatistics();
                ApplyFilters();
            }
            // 날짜가 다르면 화면에는 추가 안하지만, 해당 날짜 캐시는 무효화
            else
            {
                // 해당 날짜의 캐시 삭제 (다음에 조회할 때 파일에서 다시 읽도록)
                HistoryManager.Instance.ClearCacheByDate(history.Info.GeneratedTime.Date);
            }
        }

        /// <summary>
        /// 특정 날짜의 알람 로드
        /// </summary>
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
                // Type 필터
                if (currentTypeFilter != "All" && a.Info.Grade != currentTypeFilter)
                    return false;

                // 검색 필터
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

            // 페이지 재계산
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

                // Type에 따른 색상 구분
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

        // 필터 버튼 이벤트
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

            // 나머지는 기본 색상
            foreach (Button btn in new[] { btnFilterAll, btnFilterError, btnFilterWarning })
            {
                if (btn != activeButton)
                    btn.ForeColor = Color.Black;
            }
        }

        // 날짜 필터
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
                // 날짜 필터 활성화 - 선택된 날짜 로드
                currentPage = 1;
                LoadAlarmsByDate(dtpDateFilter.Value.Date);
            }
            else
            {
                // 날짜 필터 비활성화 - 최근 7일 데이터 로드
                currentPage = 1;
                currentAlarms = HistoryManager.Instance.LoadRecentAlarmHistory(7);
                currentDate = DateTime.MinValue; // 날짜 필터 없음 표시
                UpdateStatistics();
                ApplyFilters();
            }
        }

        // 페이지 크기 변경
        private void cmbPageSize_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbPageSize.SelectedItem != null)
            {
                pageSize = int.Parse(cmbPageSize.SelectedItem.ToString());
                currentPage = 1;
                ApplyFilters();
            }
        }

        // 페이지 네비게이션
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

        // 검색 기능
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
    }
}