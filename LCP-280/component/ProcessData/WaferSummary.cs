using QMC.LCP_280.Process.Work;
using System;
using System.Diagnostics;

namespace QMC.LCP_280.Process.Component
{
    [Serializable]
    public sealed class WaferSummary
    {
        public class WaferTotalSummaryRow
        {
            public DateTime Date { get; set; }                     // DATE (MM-dd)
            public string MachineName { get; set; }                // MachineName (EqpName)

            public string WaferId { get; set; }                    // WAFERID
            public string BinId { get; set; }                      // BINID

            public DateTime Start { get; set; }                    // START (HH:mm:ss)
            public DateTime End { get; set; }                      // END (HH:mm:ss)

            public TimeSpan TotalTime { get; set; }                // Total Time
            public TimeSpan RunTime { get; set; }                  // Run Time
            public TimeSpan DownTime { get; set; }                 // Down Time
            public TimeSpan ScanTime { get; set; }                 // Scan Time
            public TimeSpan LoadTime { get; set; }                 // Ld Time
            public TimeSpan UnloadTime { get; set; }               // ULd Time
            public TimeSpan SortTime { get; set; }                 // SortTime

            public int AlarmCount { get; set; }                    // AlarmCnt

            public int TotalCount { get; set; }                    // Total Count
            public int ScanCount { get; set; }                     // Scan Count
            public int OutCount { get; set; }                      // Out Count
            public int MissCount { get; set; }                     // Miss Count (파생)
            public int ScanNg { get; set; }                        // Scan NG (파생: TotalCount-ScanCount)
            public int OutSide { get; set; }                       // OutSide

            public int WaferVision { get; set; }                   // WaferVision
            public int AlignVision { get; set; }                   // AlignVision
            public int IndexVision { get; set; }                   // IndexVision
            public int Contact { get; set; }                       // Contact

            public int LdPick { get; set; }                        // Ld Pick
            public int LdPlace { get; set; }                       // Ld Place
            public int ULdPick { get; set; }                       // ULd Pick
            public int ULdPlace { get; set; }                      // ULd Place

            public double CycleTime { get; set; }                  // C/T
            public int TotalNg { get; set; }                       // Total NG
            public int ContactRetry { get; set; }                  // Contact Retry

            public double Yield { get; set; }                      // Yield (예: 99.73)
            public int Uph { get; set; }                           // UPH
            public int Upd { get; set; }                           // UPD

            public string[] Pickers { get; set; }                  // 8개 ("USE"...)
        }

        [NonSerialized]
        private readonly object _gate = new object();

        // [CHANGE] 기본 Pickers 템플릿은 1회만 보관하고, 외부 노출/Row 저장 시에는 항상 Clone 사용(불변처럼 사용)
        private static readonly string[] s_defaultPickersTemplate = new[] { "USE", "USE", "USE", "USE", "USE", "USE", "USE", "USE" };

        private WaferTotalSummaryRow _row = new WaferTotalSummaryRow
        {
            Pickers = (string[])s_defaultPickersTemplate.Clone()
        };

        private enum Segment
        {
            Total,
            Run,
            Down,
            Load,
            Unload,
            Scan,
            Sort
        }

        [NonSerialized] private Stopwatch _swTotal;
        [NonSerialized] private Stopwatch _swRun;
        [NonSerialized] private Stopwatch _swDown;
        [NonSerialized] private Stopwatch _swLoad;
        [NonSerialized] private Stopwatch _swUnload;
        [NonSerialized] private Stopwatch _swScan;
        [NonSerialized] private Stopwatch _swSort;
        [NonSerialized] private bool _timersInited;

        private void EnsureTimers_NoLock()
        {
            if (_timersInited)
                return;

            _swTotal = new Stopwatch();
            _swRun = new Stopwatch();
            _swDown = new Stopwatch();
            _swLoad = new Stopwatch();
            _swUnload = new Stopwatch();
            _swScan = new Stopwatch();
            _swSort = new Stopwatch();

            _timersInited = true;
        }

        private void FlushTimes_NoLock()
        {
            _row.TotalTime = _swTotal != null ? _swTotal.Elapsed : TimeSpan.Zero;
            _row.RunTime = _swRun != null ? _swRun.Elapsed : TimeSpan.Zero;
            _row.DownTime = _swDown != null ? _swDown.Elapsed : TimeSpan.Zero;
            _row.LoadTime = _swLoad != null ? _swLoad.Elapsed : TimeSpan.Zero;
            _row.UnloadTime = _swUnload != null ? _swUnload.Elapsed : TimeSpan.Zero;
            _row.ScanTime = _swScan != null ? _swScan.Elapsed : TimeSpan.Zero;
            _row.SortTime = _swSort != null ? _swSort.Elapsed : TimeSpan.Zero;
        }

        // [CHANGE] 매번 new[] 생성하지 않고 템플릿 Clone
        private static string[] CreateDefaultPickers() => (string[])s_defaultPickersTemplate.Clone();

        private void EnsurePickers_NoLock()
        {
            if (_row.Pickers == null || _row.Pickers.Length != 8)
                _row.Pickers = CreateDefaultPickers();
        }

        private void ResetRow_NoLock()
        {
            _row = new WaferTotalSummaryRow
            {
                Date = DateTime.Today,
                Start = default(DateTime),
                End = default(DateTime),
                Pickers = CreateDefaultPickers()
            };
        }

        private static WaferTotalSummaryRow CloneRow_NoLock(WaferTotalSummaryRow src)
        {
            return new WaferTotalSummaryRow
            {
                Date = src.Date,
                MachineName = src.MachineName,
                WaferId = src.WaferId,
                BinId = src.BinId,
                Start = src.Start,
                End = src.End,

                TotalTime = src.TotalTime,
                RunTime = src.RunTime,
                DownTime = src.DownTime,
                ScanTime = src.ScanTime,
                LoadTime = src.LoadTime,
                UnloadTime = src.UnloadTime,
                SortTime = src.SortTime,

                AlarmCount = src.AlarmCount,

                TotalCount = src.TotalCount,
                ScanCount = src.ScanCount,
                OutCount = src.OutCount,
                MissCount = src.MissCount,
                ScanNg = src.ScanNg,
                OutSide = src.OutSide,

                WaferVision = src.WaferVision,
                AlignVision = src.AlignVision,
                IndexVision = src.IndexVision,
                Contact = src.Contact,

                LdPick = src.LdPick,
                LdPlace = src.LdPlace,
                ULdPick = src.ULdPick,
                ULdPlace = src.ULdPlace,

                CycleTime = src.CycleTime,
                TotalNg = src.TotalNg,
                ContactRetry = src.ContactRetry,

                Yield = src.Yield,
                Uph = src.Uph,
                Upd = src.Upd,

                Pickers = src.Pickers != null ? (string[])src.Pickers.Clone() : null
            };
        }

        // ---------------------------
        // Row 전체 Get
        // ---------------------------
        public WaferTotalSummaryRow GetRowSnapshot()
        {
            lock (_gate)
            {
                EnsureTimers_NoLock();
                EnsurePickers_NoLock();

                // Flush + Derived를 "한 번"에 맞춤
                RecalculateDerived_NoLock();

                return CloneRow_NoLock(_row);

                //EnsureTimers_NoLock();
                //FlushTimes_NoLock();
                //EnsurePickers_NoLock();
                //RecalculateDerivedCounts_NoLock();
                //return CloneRow_NoLock(_row);
            }
        }

        // ---------------------------
        // 시작/종료(논리적으로 필요한 값 세팅은 Begin에서만)
        // ---------------------------
        public void BeginTotalSummary(string waferId = null, string binId = null, string machineName = null)
        {
            lock (_gate)
            {
                EnsureTimers_NoLock();
                ResetRow_NoLock();

                _row.Date = DateTime.Today;
                _row.Start = DateTime.Now;

                if (!string.IsNullOrEmpty(waferId)) _row.WaferId = waferId;
                if (!string.IsNullOrEmpty(binId)) _row.BinId = binId;
                if (!string.IsNullOrEmpty(machineName)) _row.MachineName = machineName;

                _swTotal.Reset();
                _swTotal.Start();

                _swRun.Reset();
                _swRun.Start();

                _swDown.Reset();
                _swLoad.Reset();
                _swUnload.Reset();
                _swScan.Reset();
                _swSort.Reset();

                RecalculateDerived_NoLock();
            }
        }

        public void EndTotalSummary()
        {
            lock (_gate)
            {
                EnsureTimers_NoLock();

                if (_swTotal.IsRunning) _swTotal.Stop();
                if (_swRun.IsRunning) _swRun.Stop();
                if (_swDown.IsRunning) _swDown.Stop();
                if (_swLoad.IsRunning) _swLoad.Stop();
                if (_swUnload.IsRunning) _swUnload.Stop();
                if (_swScan.IsRunning) _swScan.Stop();
                if (_swSort.IsRunning) _swSort.Stop();

                _row.End = DateTime.Now;
                RecalculateDerived_NoLock();
            }
        }

        // ---------------------------
        // Segment 타이머 제어
        // ---------------------------
        private Stopwatch GetStopwatch_NoLock(Segment seg)
        {
            switch (seg)
            {
                case Segment.Total: return _swTotal;
                case Segment.Run: return _swRun;
                case Segment.Down: return _swDown;
                case Segment.Load: return _swLoad;
                case Segment.Unload: return _swUnload;
                case Segment.Scan: return _swScan;
                case Segment.Sort: return _swSort;
                default: return null;
            }
        }

        private void StartSegment(Segment seg)
        {
            lock (_gate)
            {
                EnsureTimers_NoLock();
                var sw = GetStopwatch_NoLock(seg);
                if (sw == null) return;

                if (!sw.IsRunning)
                    sw.Start();

                FlushTimes_NoLock();
            }
        }

        private void StopSegment(Segment seg)
        {
            lock (_gate)
            {
                EnsureTimers_NoLock();
                var sw = GetStopwatch_NoLock(seg);
                if (sw == null) return;

                if (sw.IsRunning)
                    sw.Stop();

                FlushTimes_NoLock();
            }
        }

        public void StopAllSegments()
        {
            lock (_gate)
            {
                EnsureTimers_NoLock();

                if (_swTotal.IsRunning) _swTotal.Stop();
                if (_swRun.IsRunning) _swRun.Stop();
                if (_swDown.IsRunning) _swDown.Stop();
                if (_swLoad.IsRunning) _swLoad.Stop();
                if (_swUnload.IsRunning) _swUnload.Stop();
                if (_swScan.IsRunning) _swScan.Stop();
                if (_swSort.IsRunning) _swSort.Stop();

                FlushTimes_NoLock();
            }
        }

        public void ResumeRun() { StartSegment(Segment.Run); }

        public void StartLoad() { StartSegment(Segment.Load); }
        public void StopLoad() { StopSegment(Segment.Load); }

        public void StartUnload() { StartSegment(Segment.Unload); }
        public void StopUnload() { StopSegment(Segment.Unload); }

        public void StartScan()
        {
            ResetScan();
            StartSegment(Segment.Scan);
        }
        public void StopScan() { StopSegment(Segment.Scan); }

        // [CHANGE] ResetScan도 EnsureTimers_NoLock() 기준으로 통일
        public void ResetScan()
        {
            lock (_gate)
            {
                EnsureTimers_NoLock();

                _swScan.Reset();
                FlushTimes_NoLock();
            }
        }

        public void StartSort() { StartSegment(Segment.Sort); }
        public void StopSort() { StopSegment(Segment.Sort); }

        public void StartRun() { StartSegment(Segment.Run); }
        public void StopRun() { StopSegment(Segment.Run); }

        public void StartDown() { StartSegment(Segment.Down); }
        public void StopDown() { StopSegment(Segment.Down); }

        // ---------------------------
        // 파생 카운트(ScanNg/MissCount)
        // ---------------------------
        private void RecalculateScanNg_NoLock()
        {
            int v = _row.TotalCount - _row.ScanCount;
            _row.ScanNg = v > 0 ? v : 0;
        }

        private void RecalculateMissCount_NoLock()
        {
            _row.MissCount =
                _row.ScanNg +
                _row.OutSide +
                _row.WaferVision +
                _row.AlignVision +
                _row.IndexVision +
                _row.Contact +
                _row.LdPick +
                _row.LdPlace +
                _row.ULdPick +
                _row.ULdPlace;

            _row.TotalNg = _row.MissCount;
        }

        // [CHANGE] KPI(C/T, UPH, UPD) 계산 - Scan/Sort/Load/Unload 제외
        private void RecalculateKpis_NoLock()
        {
            // 타이머 값을 Row에 반영(Elapsed 기준)
            FlushTimes_NoLock();

            // 기준 시간: RunTime - (Load/Unload/Scan/Sort)  => "순수 Run"
            var effective = _row.RunTime;

            if (_row.LoadTime > TimeSpan.Zero) effective = effective - _row.LoadTime;
            if (_row.UnloadTime > TimeSpan.Zero) effective = effective - _row.UnloadTime;
            if (_row.ScanTime > TimeSpan.Zero) effective = effective - _row.ScanTime;
            if (_row.SortTime > TimeSpan.Zero) effective = effective - _row.SortTime;

            if (effective < TimeSpan.Zero) effective = TimeSpan.Zero;

            double effectiveSeconds = effective.TotalSeconds;

            if (_row.OutCount <= 0 || effectiveSeconds <= 0)
            {
                _row.CycleTime = 0;
                _row.Uph = 0;
                _row.Upd = 0;
                return;
            }

            // C/T: ms/EA (소수점 없음)
            //_row.CycleTime = Math.Floor(effectiveSeconds * 1000.0 / _row.OutCount);

            // UPH/UPD: 시간/일 환산(정수)
            _row.Uph = (int)Math.Round(_row.OutCount * 3600.0 / effectiveSeconds, MidpointRounding.AwayFromZero);
            _row.Upd = (int)Math.Round(_row.OutCount * 86400.0 / effectiveSeconds, MidpointRounding.AwayFromZero);
        }

        public void RecalculateYield_NoLock()
        {
            //lock (_gate)
            {
                if (_row.TotalCount <= 0)
                {
                    _row.Yield = 0;
                    return;
                }

                // Yield = (Total - (Miss + TotalNg)) / Total * 100
                int bad = _row.TotalNg;     //_row.MissCount + _row.TotalNg;

                int good = _row.TotalCount - bad;
                if (good < 0) good = 0;

                _row.Yield = good * 100.0 / _row.TotalCount;
            }
        }

        private void RecalculateDerived_NoLock()
        {
            // 파생 Count
            RecalculateScanNg_NoLock();
            RecalculateMissCount_NoLock();

            // KPI(C/T,UPH,UPD)
            RecalculateKpis_NoLock();

            // Yield
            RecalculateYield_NoLock();
        }

        // ---------------------------
        // Count 집계(Add) + 값 세팅(Set) API 분리
        // ---------------------------
        public void AddAlarmCount(int delta = 1) { lock (_gate) { _row.AlarmCount += delta; } }

        // [CHANGE] Add는 누적만 수행(리셋 제거)
        public void AddTotalCount(int delta = 1)
        {
            lock (_gate)
            {
                _row.TotalCount += delta;
                RecalculateDerived_NoLock();
            }
        }

        // [NEW] 값 세팅 API
        public void SetTotalCount(int totalCount)
        {
            lock (_gate)
            {
                _row.TotalCount = totalCount;
                RecalculateDerived_NoLock();
            }
        }

        // [CHANGE] Add는 누적만 수행(리셋 제거)
        public void AddScanCount(int delta = 1)
        {
            lock (_gate)
            {
                _row.ScanCount += delta;
                RecalculateDerived_NoLock();
            }
        }

        // [NEW] 값 세팅 API
        public void SetScanCount(int scanCount)
        {
            lock (_gate)
            {
                _row.ScanCount = scanCount;
                RecalculateDerived_NoLock();
            }
        }

        // [NEW] Reset API는 내부 lock 중복 없이 직접 값만 변경
        public void ResetTotalCount()
        {
            lock (_gate)
            {
                _row.TotalCount = 0;
                RecalculateDerived_NoLock();
            }
        }

        // [NEW]
        public void ResetScanCount()
        {
            lock (_gate)
            {
                _row.ScanCount = 0;
                RecalculateDerived_NoLock();
            }
        }

        public void AddOutCount(int delta = 1)
        {
            lock (_gate)
            {
                _row.OutCount += delta;
                RecalculateDerived_NoLock();
            }
        }

        public void AddTotalNg(int delta = 1)
        {
            lock (_gate)
            {
                _row.TotalNg += delta;
                RecalculateDerived_NoLock();
            }
        }

        public void AddOutSideAsMiss(int delta = 1) { lock (_gate) { _row.OutSide += delta; RecalculateDerived_NoLock(); } }
        public void AddWaferVisionAsMiss(int delta = 1) { lock (_gate) { _row.WaferVision += delta; RecalculateDerived_NoLock(); } }
        public void AddAlignVisionAsMiss(int delta = 1) { lock (_gate) { _row.AlignVision += delta; RecalculateDerived_NoLock(); } }
        public void AddIndexVisionAsMiss(int delta = 1) { lock (_gate) { _row.IndexVision += delta; RecalculateDerived_NoLock(); } }
        public void AddContactAsMiss(int delta = 1) { lock (_gate) { _row.Contact += delta; RecalculateDerived_NoLock(); } }
        public void AddLdPickAsMiss(int delta = 1) { lock (_gate) { _row.LdPick += delta; RecalculateDerived_NoLock(); } }
        public void AddLdPlaceAsMiss(int delta = 1) { lock (_gate) { _row.LdPlace += delta; RecalculateDerived_NoLock(); } }
        public void AddULdPickAsMiss(int delta = 1) { lock (_gate) { _row.ULdPick += delta; RecalculateDerived_NoLock(); } }
        public void AddULdPlaceAsMiss(int delta = 1) { lock (_gate) { _row.ULdPlace += delta; RecalculateDerived_NoLock(); } }
        public void AddContactRetry(int delta = 1) { lock (_gate) { _row.ContactRetry += delta; } }


        // 외부에서 pickers를 "Set"해야 하는 경우가 있어 이름만 의도적으로 변경(집계 API가 아님)
        public void SetPickersUseFlags(string[] pickers8)
        {
            lock (_gate)
            {
                if (pickers8 == null || pickers8.Length != 8)
                    _row.Pickers = CreateDefaultPickers();
                else
                    _row.Pickers = (string[])pickers8.Clone();
            }
        }

        public void SetWaferId(string waferId)
        {
            lock (_gate)
            {
                _row.WaferId = waferId;
            }
        }

        public void SetBinId(string binId)
        {
            lock (_gate)
            {
                _row.BinId = binId;
            }
        }

        public void SetCycleTimeMsFromTakt(TimeSpan interval)
        {
            lock (_gate)
            {
                // ms 단위, 소수점 없이
                _row.CycleTime = Math.Round(interval.TotalMilliseconds, 0, MidpointRounding.AwayFromZero);
                // KPI/수율을 여기서 다시 계산하길 원하면 호출(선택)
                // RecalculateDerived_NoLock();
            }
        }

        // ---------------------------
        // Getter들
        // ---------------------------
        public DateTime GetDate() { lock (_gate) { return _row.Date; } }
        public string GetMachineName() { lock (_gate) { return _row.MachineName; } }
        public string GetWaferId() { lock (_gate) { return _row.WaferId; } }
        public string GetBinId() { lock (_gate) { return _row.BinId; } }
        public DateTime GetStart() { lock (_gate) { return _row.Start; } }
        public DateTime GetEnd() { lock (_gate) { return _row.End; } }

        public TimeSpan GetTotalTime() { lock (_gate) { EnsureTimers_NoLock(); FlushTimes_NoLock(); return _row.TotalTime; } }
        public TimeSpan GetRunTime() { lock (_gate) { EnsureTimers_NoLock(); FlushTimes_NoLock(); return _row.RunTime; } }
        public TimeSpan GetDownTime() { lock (_gate) { EnsureTimers_NoLock(); FlushTimes_NoLock(); return _row.DownTime; } }
        public TimeSpan GetScanTime() { lock (_gate) { EnsureTimers_NoLock(); FlushTimes_NoLock(); return _row.ScanTime; } }
        public TimeSpan GetLoadTime() { lock (_gate) { EnsureTimers_NoLock(); FlushTimes_NoLock(); return _row.LoadTime; } }
        public TimeSpan GetUnloadTime() { lock (_gate) { EnsureTimers_NoLock(); FlushTimes_NoLock(); return _row.UnloadTime; } }
        public TimeSpan GetSortTime() { lock (_gate) { EnsureTimers_NoLock(); FlushTimes_NoLock(); return _row.SortTime; } }

        public int GetAlarmCount() { lock (_gate) { return _row.AlarmCount; } }

        public int GetTotalCount() { lock (_gate) { return _row.TotalCount; } }
        public int GetScanCount() { lock (_gate) { return _row.ScanCount; } }
        public int GetOutCount() { lock (_gate) { return _row.OutCount; } }

        public int GetScanNg() { lock (_gate) { RecalculateDerived_NoLock(); return _row.ScanNg; } }
        public int GetMissCount() { lock (_gate) { RecalculateDerived_NoLock(); return _row.MissCount; } }

        public int GetOutSide() { lock (_gate) { return _row.OutSide; } }
        public int GetWaferVision() { lock (_gate) { return _row.WaferVision; } }
        public int GetAlignVision() { lock (_gate) { return _row.AlignVision; } }
        public int GetIndexVision() { lock (_gate) { return _row.IndexVision; } }
        public int GetContact() { lock (_gate) { return _row.Contact; } }
        public int GetLdPick() { lock (_gate) { return _row.LdPick; } }
        public int GetLdPlace() { lock (_gate) { return _row.LdPlace; } }
        public int GetULdPick() { lock (_gate) { return _row.ULdPick; } }
        public int GetULdPlace() { lock (_gate) { return _row.ULdPlace; } }

        public double GetCycleTime() { lock (_gate) { return _row.CycleTime; } }
        public int GetTotalNg() { lock (_gate) { return _row.TotalNg; } }
        public int GetContactRetry() { lock (_gate) { return _row.ContactRetry; } }
        public double GetYield() { lock (_gate) { return _row.Yield; } }
        public int GetUph() { lock (_gate) { return _row.Uph; } }
        public int GetUpd() { lock (_gate) { return _row.Upd; } }

        public string[] GetPickers()
        {
            lock (_gate)
            {
                EnsurePickers_NoLock();
                return (string[])_row.Pickers.Clone();
            }
        }
    }
}