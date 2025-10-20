using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace QMC.Common.History
{
    public partial class LogHistoryGridViewer : UserControl
    {
        private bool initComplete = false;
        private List<LogEntry> currentLogs = new List<LogEntry>();
        private List<LogEntry> filteredLogs = new List<LogEntry>();

        private string currentLevelFilter = "All";
        private string currentCategoryFilter = "All";
        private DateTime currentDate = DateTime.Today;
        private int pageSize = 10;
        private int currentPage = 1;
        private string searchKeyword = string.Empty;

        public LogHistoryGridViewer()
        {
            InitializeComponent();
        }

        private void LogHistoryGridViewer_Load(object sender, EventArgs e)
        {
            if (!initComplete)
            {
                cmbPageSize.SelectedIndex = 3;
                dtpDateFilter.Value = DateTime.Today;
                chkEnableDateFilter.Checked = true;

                LoadLogCategories();
                LoadRecentLogDates();
                LoadLogsByDate(DateTime.Today);

                initComplete = true;
            }
        }

        private void LoadLogsByDate(DateTime date)
        {
            currentDate = date;
            currentLogs = LoadLogsFromFiles(date);
            UpdateStatistics();
            ApplyFilters();
        }

        private List<LogEntry> LoadLogsFromFiles(DateTime date)
        {
            List<LogEntry> logs = new List<LogEntry>();
            string logFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Log");

            if (!Directory.Exists(logFolder))
                return logs;

            // 날짜 패턴: YYYY-MM-DD
            string datePattern = date.ToString("yyyy-MM-dd");

            // 해당 날짜의 모든 로그 파일 찾기
            var files = Directory.GetFiles(logFolder, "*.log")
                .Where(f => Path.GetFileName(f).Contains(datePattern))
                .ToList();

            foreach (var file in files)
            {
                try
                {
                    string category = ExtractCategoryFromFileName(Path.GetFileName(file));
                    var lines = File.ReadAllLines(file, Encoding.UTF8);

                    foreach (var line in lines)
                    {
                        if (string.IsNullOrWhiteSpace(line)) continue;

                        var logEntry = ParseLogLine(line, category, file);
                        if (logEntry != null)
                        {
                            logs.Add(logEntry);
                        }
                    }
                }
                catch { }
            }

            return logs.OrderByDescending(l => l.Time).ToList();
        }

        private string ExtractCategoryFromFileName(string fileName)
        {
            // 예: AlarmPost_2025-08-24.log -> AlarmPost
            // 예: Camera_2025-08-22.log -> Camera
            // 예: BarcodeControl_2025-10-20.log -> BarcodeControl

            int underscoreIndex = fileName.IndexOf('_');
            if (underscoreIndex > 0)
            {
                return fileName.Substring(0, underscoreIndex);
            }

            // 날짜가 없는 경우 .log 전까지
            return Path.GetFileNameWithoutExtension(fileName);
        }

        private LogEntry ParseLogLine(string line, string category, string filePath)
        {
            try
            {
                // 로그 형식 예상: [2025-08-24 10:30:45] [INFO] Message
                // 또는: 2025-08-24 10:30:45 | INFO | Message
                // 기본적인 파싱 로직 (실제 로그 형식에 맞게 수정 필요)

                var entry = new LogEntry();
                entry.Category = category;
                entry.FilePath = filePath;
                entry.RawLine = line;

                // 시간 추출 시도
                if (line.Contains("[") && line.Contains("]"))
                {
                    int start = line.IndexOf('[') + 1;
                    int end = line.IndexOf(']', start);
                    if (end > start)
                    {
                        string timeStr = line.Substring(start, end - start);
                        if (DateTime.TryParse(timeStr, out DateTime time))
                        {
                            entry.Time = time;
                        }
                    }

                    // 레벨 추출 (INFO, ERROR, WARNING 등)
                    int levelStart = line.IndexOf('[', end + 1);
                    if (levelStart > 0)
                    {
                        int levelEnd = line.IndexOf(']', levelStart);
                        if (levelEnd > levelStart)
                        {
                            entry.Level = line.Substring(levelStart + 1, levelEnd - levelStart - 1).Trim();
                        }
                    }

                    // 메시지 추출
                    int msgStart = line.LastIndexOf(']') + 1;
                    if (msgStart < line.Length)
                    {
                        entry.Message = line.Substring(msgStart).Trim();
                    }
                }
                else
                {
                    // 단순 텍스트 로그
                    entry.Time = DateTime.Now;
                    entry.Level = "INFO";
                    entry.Message = line;
                }

                return entry;
            }
            catch
            {
                return null;
            }
        }

        private void ApplyFilters()
        {
            filteredLogs = currentLogs.Where(l =>
            {
                if (currentLevelFilter != "All" && l.Level != currentLevelFilter)
                    return false;

                if (currentCategoryFilter != "All" && l.Category != currentCategoryFilter)
                    return false;

                if (!string.IsNullOrWhiteSpace(searchKeyword))
                {
                    string keyword = searchKeyword.ToLower();
                    if (!l.Message.ToLower().Contains(keyword) &&
                        !l.Category.ToLower().Contains(keyword) &&
                        !l.Level.ToLower().Contains(keyword))
                        return false;
                }

                return true;
            }).ToList();

            int totalPages = (int)Math.Ceiling(filteredLogs.Count / (double)pageSize);
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
            int endIndex = Math.Min(startIndex + pageSize, filteredLogs.Count);

            for (int i = startIndex; i < endIndex; i++)
            {
                var log = filteredLogs[i];
                int rowIndex = dataGridView1.Rows.Add();
                var row = dataGridView1.Rows[rowIndex];

                row.Cells["Time"].Value = log.Time.ToString("yyyy-MM-dd HH:mm:ss");
                row.Cells["Category"].Value = log.Category;
                row.Cells["Level"].Value = log.Level;
                row.Cells["Message"].Value = log.Message;

                if (log.Level.ToUpper().Contains("ERROR") || log.Level.ToUpper().Contains("FATAL"))
                    row.DefaultCellStyle.BackColor = Color.FromArgb(255, 240, 240);
                else if (log.Level.ToUpper().Contains("WARN"))
                    row.DefaultCellStyle.BackColor = Color.FromArgb(255, 250, 230);
                else if (log.Level.ToUpper().Contains("DEBUG"))
                    row.DefaultCellStyle.BackColor = Color.FromArgb(240, 240, 255);
            }
        }

        private void UpdateStatistics()
        {
            int total = currentLogs.Count;
            int errorCount = currentLogs.Count(l => l.Level.ToUpper().Contains("ERROR") || l.Level.ToUpper().Contains("FATAL"));
            int warnCount = currentLogs.Count(l => l.Level.ToUpper().Contains("WARN"));

            lblTotal.Text = $"Total: {total}";
            lblError.Text = $"Error: {errorCount}";
            lblWarning.Text = $"Warning: {warnCount}";
        }

        private void UpdatePaginationInfo()
        {
            int totalPages = (int)Math.Ceiling(filteredLogs.Count / (double)pageSize);
            int startRow = filteredLogs.Count == 0 ? 0 : (currentPage - 1) * pageSize + 1;
            int endRow = Math.Min(currentPage * pageSize, filteredLogs.Count);

            lblPagination.Text = $"{startRow}-{endRow} / {filteredLogs.Count}";

            btnPrevPage.Enabled = currentPage > 1;
            btnNextPage.Enabled = currentPage < totalPages;
            btnFirstPage.Enabled = currentPage > 1;
            btnLastPage.Enabled = currentPage < totalPages;

            lblCurrentPage.Text = $"Page {currentPage} / {Math.Max(1, totalPages)}";
        }

        private void LoadLogCategories()
        {
            string logFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Log");
            if (!Directory.Exists(logFolder))
                return;

            cmbCategoryFilter.Items.Clear();
            cmbCategoryFilter.Items.Add("All");

            var files = Directory.GetFiles(logFolder, "*.log");
            var categories = files.Select(f => ExtractCategoryFromFileName(Path.GetFileName(f)))
                .Distinct()
                .OrderBy(c => c)
                .ToList();

            foreach (var category in categories)
            {
                cmbCategoryFilter.Items.Add(category);
            }

            cmbCategoryFilter.SelectedIndex = 0;
        }

        private void LoadRecentLogDates()
        {
            string logFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Log");
            if (!Directory.Exists(logFolder))
                return;

            cmbRecentDates.Items.Clear();
            cmbRecentDates.Items.Add("-- 날짜 선택 --");

            var files = Directory.GetFiles(logFolder, "*.log")
                .OrderByDescending(f => f)
                .Take(100);

            var dateGroups = new Dictionary<string, int>();

            foreach (var file in files)
            {
                try
                {
                    string fileName = Path.GetFileName(file);
                    DateTime? fileDate = ExtractDateFromFileName(fileName);

                    if (fileDate.HasValue)
                    {
                        string dateKey = fileDate.Value.ToString("yyyy-MM-dd");
                        if (!dateGroups.ContainsKey(dateKey))
                            dateGroups[dateKey] = 0;

                        var lines = File.ReadAllLines(file);
                        dateGroups[dateKey] += lines.Where(l => !string.IsNullOrWhiteSpace(l)).Count();
                    }
                }
                catch { }
            }

            foreach (var kvp in dateGroups.OrderByDescending(k => k.Key).Take(30))
            {
                cmbRecentDates.Items.Add($"{kvp.Key} ({kvp.Value}개)");
            }

            cmbRecentDates.SelectedIndex = 0;
        }

        private DateTime? ExtractDateFromFileName(string fileName)
        {
            // YYYY-MM-DD 패턴 찾기
            var match = System.Text.RegularExpressions.Regex.Match(fileName, @"(\d{4})-(\d{2})-(\d{2})");
            if (match.Success)
            {
                if (DateTime.TryParse(match.Value, out DateTime date))
                    return date;
            }
            return null;
        }

        // 이벤트 핸들러들
        private void btnFilterAll_Click(object sender, EventArgs e)
        {
            currentLevelFilter = "All";
            currentPage = 1;
            HighlightFilterButton(btnFilterAll);
            ApplyFilters();
        }

        private void btnFilterError_Click(object sender, EventArgs e)
        {
            currentLevelFilter = "ERROR";
            currentPage = 1;
            HighlightFilterButton(btnFilterError);
            ApplyFilters();
        }

        private void btnFilterWarning_Click(object sender, EventArgs e)
        {
            currentLevelFilter = "WARNING";
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

        private void cmbCategoryFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbCategoryFilter.SelectedItem != null)
            {
                currentCategoryFilter = cmbCategoryFilter.SelectedItem.ToString();
                currentPage = 1;
                ApplyFilters();
            }
        }

        private void dtpDateFilter_ValueChanged(object sender, EventArgs e)
        {
            if (chkEnableDateFilter.Checked)
            {
                currentPage = 1;
                LoadLogsByDate(dtpDateFilter.Value.Date);
            }
        }

        private void chkEnableDateFilter_CheckedChanged(object sender, EventArgs e)
        {
            dtpDateFilter.Enabled = chkEnableDateFilter.Checked;

            if (chkEnableDateFilter.Checked)
            {
                currentPage = 1;
                LoadLogsByDate(dtpDateFilter.Value.Date);
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
            int totalPages = (int)Math.Ceiling(filteredLogs.Count / (double)pageSize);
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
            int totalPages = (int)Math.Ceiling(filteredLogs.Count / (double)pageSize);
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
                    LoadLogsByDate(date);
                }
            }
        }
    }

    // 로그 엔트리 클래스
    public class LogEntry
    {
        public DateTime Time { get; set; }
        public string Category { get; set; }
        public string Level { get; set; }
        public string Message { get; set; }
        public string FilePath { get; set; }
        public string RawLine { get; set; }

        public LogEntry()
        {
            Time = DateTime.Now;
            Category = "Unknown";
            Level = "INFO";
            Message = "";
            FilePath = "";
            RawLine = "";
        }
    }
}