using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QMC.LCP_280.Process.Component.FormDlg
{
    internal sealed class WaferTotalSummaryViewerController : IDisposable
    {
        private readonly DataGridView _grid;
        private readonly Label _status;
        private readonly ComboBox _sourceCombo;

        private readonly BindingList<WaferTotalSummaryRowVm> _rows = new BindingList<WaferTotalSummaryRowVm>();
        private readonly Timer _timer;

        private bool _running;
        private bool _suppressRefreshWhileScrolling;

        public WaferTotalSummaryViewerController(DataGridView grid, Label status, ComboBox sourceCombo)
        {
            _grid = grid ?? throw new ArgumentNullException(nameof(grid));
            _status = status;
            _sourceCombo = sourceCombo;

            ConfigureGrid();

            _timer = new Timer { Interval = 200 };
            _timer.Tick += (s, e) => RefreshOnce();

            ConfigureSourceUiAsSingleMode();
        }

        public void Start()
        {
            if (_running) return;
            _running = true;
            _timer.Start();
            SetStatus("RUNNING");
        }

        public void Stop()
        {
            if (!_running) return;
            _running = false;
            _timer.Stop();
            SetStatus("STOPPED");
        }

        private void ConfigureGrid()
        {
            _grid.AutoGenerateColumns = false;
            _grid.ReadOnly = true;
            _grid.AllowUserToAddRows = false;
            _grid.AllowUserToDeleteRows = false;
            _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            _grid.MultiSelect = false;
            _grid.RowHeadersVisible = false;

            _grid.Columns.Clear();
            _grid.Columns.Add(MakeText(nameof(WaferTotalSummaryRowVm.Date), "DATE", 90));
            _grid.Columns.Add(MakeText(nameof(WaferTotalSummaryRowVm.MachineName), "MachineName", 110));
            _grid.Columns.Add(MakeText(nameof(WaferTotalSummaryRowVm.WaferId), "WAFERID", 140));
            _grid.Columns.Add(MakeText(nameof(WaferTotalSummaryRowVm.BinId), "BINID", 140));
            _grid.Columns.Add(MakeText(nameof(WaferTotalSummaryRowVm.Start), "START", 80));
            _grid.Columns.Add(MakeText(nameof(WaferTotalSummaryRowVm.End), "END", 80));

            _grid.Columns.Add(MakeText(nameof(WaferTotalSummaryRowVm.TotalTime), "Total Time", 90));
            _grid.Columns.Add(MakeText(nameof(WaferTotalSummaryRowVm.RunTime), "Run Time", 90));
            _grid.Columns.Add(MakeText(nameof(WaferTotalSummaryRowVm.DownTime), "Down Time", 90));
            _grid.Columns.Add(MakeText(nameof(WaferTotalSummaryRowVm.ScanTime), "Scan Time", 90));
            _grid.Columns.Add(MakeText(nameof(WaferTotalSummaryRowVm.LoadTime), "Ld Time", 80));
            _grid.Columns.Add(MakeText(nameof(WaferTotalSummaryRowVm.UnloadTime), "ULd Time", 80));
            _grid.Columns.Add(MakeText(nameof(WaferTotalSummaryRowVm.SortTime), "SortTime", 90));

            _grid.Columns.Add(MakeText(nameof(WaferTotalSummaryRowVm.AlarmCount), "AlarmCnt", 70));
            _grid.Columns.Add(MakeText(nameof(WaferTotalSummaryRowVm.TotalCount), "Total Count", 80));
            _grid.Columns.Add(MakeText(nameof(WaferTotalSummaryRowVm.ScanCount), "Scan Count", 80));
            _grid.Columns.Add(MakeText(nameof(WaferTotalSummaryRowVm.OutCount), "Out Count", 80));
            _grid.Columns.Add(MakeText(nameof(WaferTotalSummaryRowVm.MissCount), "Miss Count", 80));
            _grid.Columns.Add(MakeText(nameof(WaferTotalSummaryRowVm.ScanNg), "Scan NG", 70));
            _grid.Columns.Add(MakeText(nameof(WaferTotalSummaryRowVm.OutSide), "OutSide", 70));

            _grid.Columns.Add(MakeText(nameof(WaferTotalSummaryRowVm.WaferVision), "WaferVision", 90));
            _grid.Columns.Add(MakeText(nameof(WaferTotalSummaryRowVm.AlignVision), "AlignVision", 90));
            _grid.Columns.Add(MakeText(nameof(WaferTotalSummaryRowVm.IndexVision), "IndexVision", 90));
            _grid.Columns.Add(MakeText(nameof(WaferTotalSummaryRowVm.Contact), "Contact", 70));

            _grid.Columns.Add(MakeText(nameof(WaferTotalSummaryRowVm.LdPick), "Ld Pick", 70));
            _grid.Columns.Add(MakeText(nameof(WaferTotalSummaryRowVm.LdPlace), "Ld Place", 70));
            _grid.Columns.Add(MakeText(nameof(WaferTotalSummaryRowVm.ULdPick), "ULd Pick", 70));
            _grid.Columns.Add(MakeText(nameof(WaferTotalSummaryRowVm.ULdPlace), "ULd Place", 70));

            _grid.Columns.Add(MakeText(nameof(WaferTotalSummaryRowVm.CycleTime), "C/T", 70));
            _grid.Columns.Add(MakeText(nameof(WaferTotalSummaryRowVm.TotalNg), "Total NG", 70));
            _grid.Columns.Add(MakeText(nameof(WaferTotalSummaryRowVm.ContactRetry), "Contact Retry", 90));
            _grid.Columns.Add(MakeText(nameof(WaferTotalSummaryRowVm.Yield), "Yield", 70));
            _grid.Columns.Add(MakeText(nameof(WaferTotalSummaryRowVm.Uph), "UPH", 70));
            _grid.Columns.Add(MakeText(nameof(WaferTotalSummaryRowVm.Upd), "UPD", 70));

            _grid.Columns.Add(MakeText(nameof(WaferTotalSummaryRowVm.Picker1), "Picker1", 70));
            _grid.Columns.Add(MakeText(nameof(WaferTotalSummaryRowVm.Picker2), "Picker2", 70));
            _grid.Columns.Add(MakeText(nameof(WaferTotalSummaryRowVm.Picker3), "Picker3", 70));
            _grid.Columns.Add(MakeText(nameof(WaferTotalSummaryRowVm.Picker4), "Picker4", 70));
            _grid.Columns.Add(MakeText(nameof(WaferTotalSummaryRowVm.Picker5), "Picker5", 70));
            _grid.Columns.Add(MakeText(nameof(WaferTotalSummaryRowVm.Picker6), "Picker6", 70));
            _grid.Columns.Add(MakeText(nameof(WaferTotalSummaryRowVm.Picker7), "Picker7", 70));
            _grid.Columns.Add(MakeText(nameof(WaferTotalSummaryRowVm.Picker8), "Picker8", 70));

            _grid.DataSource = _rows;
            _grid.Scroll += Grid_Scroll;

            EnableDoubleBuffer(_grid);
        }

        private void Grid_Scroll(object sender, ScrollEventArgs e)
        {
            // 가로 스크롤만 타겟
            if (e.ScrollOrientation != ScrollOrientation.HorizontalScroll)
                return;

            // 스크롤 이벤트가 연속으로 들어오는 동안 refresh를 잠깐 막고,
            // 마지막 스크롤 이후 일정 시간 후 풀어주기
            _suppressRefreshWhileScrolling = true;

            // 타이머 하나 더 만들지 않고, 기존 _timer 주기(200ms)보다 약간 크게 잡아서
            // 스크롤 중 갱신 충돌을 회피
            Task.Run(async () =>
            {
                await Task.Delay(250).ConfigureAwait(false);
                _suppressRefreshWhileScrolling = false;
            });
        }

        private static void EnableDoubleBuffer(Control control)
        {
            if (control == null) return;

            try
            {
                // DataGridView.DoubleBuffered는 protected라 reflection으로 설정
                var pi = control.GetType().GetProperty(
                    "DoubleBuffered",
                    System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.NonPublic);

                pi?.SetValue(control, true, null);

                // 추가로 페인팅 최적화 힌트
                control.GetType().InvokeMember(
                    "SetStyle",
                    System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.InvokeMethod,
                    null,
                    control,
                    new object[]
                    {
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint,
                true
                    });
            }
            catch
            {
                // 실패해도 동작은 해야 하므로 무시
            }
        }

        private static DataGridViewTextBoxColumn MakeText(string dataProperty, string header, int width)
        {
            return new DataGridViewTextBoxColumn
            {
                DataPropertyName = dataProperty,
                HeaderText = header,
                Width = width,
                SortMode = DataGridViewColumnSortMode.NotSortable,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.None
            };
        }

        private void ConfigureSourceUiAsSingleMode()
        {
            if (_sourceCombo == null) return;

            try
            {
                _sourceCombo.Items.Clear();
                _sourceCombo.Enabled = false;
                _sourceCombo.Visible = false;
            }
            catch { }
        }

        private void RefreshOnce()
        {
            try
            {
                if (_suppressRefreshWhileScrolling) 
                    return;

                var rows = TryGetEquipmentSummaryRows();
                if (rows == null || rows.Length == 0)
                {
                    SetStatus("NO SUMMARY");
                    _rows.Clear();
                    return;
                }

                SyncRows(rows);
                SetStatus("OK");
            }
            catch (Exception ex)
            {
                SetStatus("ERR: " + ex.Message);
            }
        }

        private WaferSummary.WaferTotalSummaryRow[] TryGetEquipmentSummaryRows()
        {
            var eq = Equipment.Instance;
            if (eq == null) return null;

            // [CHANGED] 현재 1건 스냅샷이 아니라, 히스토리 전체(+현재 진행중 포함) 요청
            return eq.SummaryContext?.GetHistoryPlusCurrentSnapshots();
        }

        private void SyncRows(WaferSummary.WaferTotalSummaryRow[] src)
        {
            // 표시 순서: 오래된 것 -> 최신
            // 최신이 위로 오길 원하면 여기서 Reverse()하면 됨.
            int needed = src.Length;

            while (_rows.Count < needed)
                _rows.Add(new WaferTotalSummaryRowVm());

            while (_rows.Count > needed)
                _rows.RemoveAt(_rows.Count - 1);

            for (int i = 0; i < needed; i++)
                _rows[i].UpdateFrom(src[i]);

            _grid.Invalidate();
        }

        private void SetStatus(string text)
        {
            if (_status == null) return;
            _status.Text = text ?? string.Empty;
        }

        public void Dispose()
        {
            Stop();
            try { _timer.Dispose(); } catch { }
        }
    }

    internal sealed class WaferTotalSummaryRowVm
    {
        public string Date { get; set; }
        public string MachineName { get; set; }
        public string WaferId { get; set; }
        public string BinId { get; set; }
        public string Start { get; set; }
        public string End { get; set; }

        public string TotalTime { get; set; }
        public string RunTime { get; set; }
        public string DownTime { get; set; }
        public string ScanTime { get; set; }
        public string LoadTime { get; set; }
        public string UnloadTime { get; set; }
        public string SortTime { get; set; }

        public int AlarmCount { get; set; }
        public int TotalCount { get; set; }
        public int ScanCount { get; set; }
        public int OutCount { get; set; }
        public int MissCount { get; set; }
        public int ScanNg { get; set; }
        public int OutSide { get; set; }

        public int WaferVision { get; set; }
        public int AlignVision { get; set; }
        public int IndexVision { get; set; }
        public int Contact { get; set; }

        public int LdPick { get; set; }
        public int LdPlace { get; set; }
        public int ULdPick { get; set; }
        public int ULdPlace { get; set; }

        public double CycleTime { get; set; }
        public int TotalNg { get; set; }
        public int ContactRetry { get; set; }

        public double Yield { get; set; }
        public int Uph { get; set; }
        public int Upd { get; set; }

        public string Picker1 { get; set; }
        public string Picker2 { get; set; }
        public string Picker3 { get; set; }
        public string Picker4 { get; set; }
        public string Picker5 { get; set; }
        public string Picker6 { get; set; }
        public string Picker7 { get; set; }
        public string Picker8 { get; set; }

        public void UpdateFrom(WaferSummary.WaferTotalSummaryRow r)
        {
            if (r == null) return;

            Date = r.Date == default(DateTime) ? "" : r.Date.ToString("MM-dd");
            MachineName = r.MachineName ?? "";
            WaferId = r.WaferId ?? "";
            BinId = r.BinId ?? "";

            Start = r.Start == default(DateTime) ? "" : r.Start.ToString("HH:mm:ss");
            End = r.End == default(DateTime) ? "" : r.End.ToString("HH:mm:ss");

            TotalTime = FormatHms(r.TotalTime);
            RunTime = FormatHms(r.RunTime);
            DownTime = FormatHms(r.DownTime);
            ScanTime = FormatHms(r.ScanTime);
            LoadTime = FormatHms(r.LoadTime);
            UnloadTime = FormatHms(r.UnloadTime);
            SortTime = FormatHms(r.SortTime);

            AlarmCount = r.AlarmCount;
            TotalCount = r.TotalCount;
            ScanCount = r.ScanCount;
            OutCount = r.OutCount;
            MissCount = r.MissCount;
            ScanNg = r.ScanNg;
            OutSide = r.OutSide;

            WaferVision = r.WaferVision;
            AlignVision = r.AlignVision;
            IndexVision = r.IndexVision;
            Contact = r.Contact;

            LdPick = r.LdPick;
            LdPlace = r.LdPlace;
            ULdPick = r.ULdPick;
            ULdPlace = r.ULdPlace;

            CycleTime = r.CycleTime;
            TotalNg = r.TotalNg;
            ContactRetry = r.ContactRetry;

            Yield = r.Yield;
            Uph = r.Uph;
            Upd = r.Upd;

            var p = r.Pickers ?? new string[0];
            Picker1 = p.Length > 0 ? p[0] : "";
            Picker2 = p.Length > 1 ? p[1] : "";
            Picker3 = p.Length > 2 ? p[2] : "";
            Picker4 = p.Length > 3 ? p[3] : "";
            Picker5 = p.Length > 4 ? p[4] : "";
            Picker6 = p.Length > 5 ? p[5] : "";
            Picker7 = p.Length > 6 ? p[6] : "";
            Picker8 = p.Length > 7 ? p[7] : "";
        }

        private static string FormatHms(TimeSpan ts)
        {
            long totalSeconds = (long)Math.Max(0, ts.TotalSeconds);
            int h = (int)(totalSeconds / 3600);
            int m = (int)((totalSeconds % 3600) / 60);
            int s = (int)(totalSeconds % 60);
            return $"{h:00}:{m:00}:{s:00}";
        }
    }
}
