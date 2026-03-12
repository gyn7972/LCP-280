using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace QMC.Common.Unit
{
    /// <summary>
    /// 파일 저장 시 필요한 데이터 구조체
    /// </summary>
    public struct TaktLogData
    {
        public string UnitName;
        public string Tag;
        public double IntervalMs;
        public string StartStr;
        public string EndStr;
        public double AverageMs;
        public double MinMs;
        public double MaxMs;
        public int Count;
        public string RecordTime;
    }

    public class UnitTaktTimer
    {
        private readonly string _unitName;
        private readonly object _owner;
        private readonly object _taktLock = new object();
        private readonly Dictionary<string, CycleTimer> _taktTimers = new Dictionary<string, CycleTimer>(StringComparer.OrdinalIgnoreCase);

        // 모든 Unit이 공유하는 전역 로그 큐 및 백그라운드 기록 스레드
        private static readonly BlockingCollection<TaktLogData> _logQueue = new BlockingCollection<TaktLogData>();
        private static readonly Thread _writerThread;

        static UnitTaktTimer()
        {
            _writerThread = new Thread(LogWriterLoop)
            {
                IsBackground = true,
                Name = "TaktTimeBackgroundWriter"
            };
            _writerThread.Start();
        }

        public UnitTaktTimer(string unitName, object owner)
        {
            _unitName = unitName;
            _owner = owner;
        }

        public CycleTimer GetOrCreateTaktTimer(string tag, int capacity = 100)
        {
            if (string.IsNullOrWhiteSpace(tag)) tag = "Unnamed";
            lock (_taktLock)
            {
                if (!_taktTimers.TryGetValue(tag, out var ct))
                {
                    ct = new CycleTimer(_owner) { Capacity = capacity };
                    _taktTimers[tag] = ct;
                }
                return ct;
            }
        }

        public void Start(string tag)
        {
            try
            {
                var ct = GetOrCreateTaktTimer(tag);
                ct.Start();
            }
            catch (Exception ex)
            {
                Log.Write(_unitName ?? "UnitTaktTimer", $"[TaktStart:{tag}] {ex.Message}");
            }
        }

        public void End(string tag, bool saveToFile = true)
        {
            try
            {
                var ct = GetOrCreateTaktTimer(tag);
                ct.End();

                if (!saveToFile) return;

                var latest = ct.Latest;
                var intervalMs = latest.Interval.TotalMilliseconds;

                if (intervalMs <= 0) return;

                EnqueueLogData(tag, ct, latest);
            }
            catch (Exception ex)
            {
                Log.Write(_unitName ?? "UnitTaktTimer", $"[TaktEnd:{tag}] {ex.Message}");
            }
        }

        public void SaveAllSummaries()
        {
            try
            {
                lock (_taktLock)
                {
                    foreach (var kv in _taktTimers)
                    {
                        var tag = kv.Key;
                        var ct = kv.Value;
                        var latest = ct.Latest;
                        if (latest.Interval.TotalMilliseconds <= 0) continue;

                        EnqueueLogData(tag, ct, latest);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Write(_unitName ?? "UnitTaktTimer", $"[SaveAllSummaries] {ex.Message}");
            }
        }

        public IReadOnlyDictionary<string, CycleTimer> GetTimers()
        {
            lock (_taktLock)
            {
                return new Dictionary<string, CycleTimer>(_taktTimers);
            }
        }

        // ==========================================
        // 백그라운드 로깅 로직
        // ==========================================
        private void EnqueueLogData(string tag, CycleTimer timer, CycleTime latest)
        {
            if (!_logQueue.IsAddingCompleted)
            {
                _logQueue.Add(new TaktLogData
                {
                    UnitName = _unitName ?? "Unit",
                    Tag = tag,
                    IntervalMs = latest.Interval.TotalMilliseconds,
                    StartStr = latest.Start != DateTime.MinValue ? latest.Start.ToString("yyyy-MM-dd HH:mm:ss.fff") : "",
                    EndStr = latest.End != DateTime.MinValue ? latest.End.ToString("yyyy-MM-dd HH:mm:ss.fff") : "",
                    AverageMs = timer.Average.TotalMilliseconds,
                    MinMs = timer.Minimum.TotalMilliseconds,
                    MaxMs = timer.Maximum.TotalMilliseconds,
                    Count = timer.CycleTimes.Count,
                    RecordTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")
                });
            }
        }

        private static void LogWriterLoop()
        {
            foreach (var data in _logQueue.GetConsumingEnumerable())
            {
                try
                {
                    var path = GetTaktLogFilePath(data.UnitName, data.Tag);
                    var exists = File.Exists(path);

                    using (var fs = new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.Read))
                    using (var sw = new StreamWriter(fs, Encoding.UTF8))
                    {
                        if (!exists || fs.Length == 0)
                        {
                            sw.WriteLine("Date,Unit,Tag,IntervalMs,Start,End,AverageMs,MinMs,MaxMs,Count");
                        }

                        var line = string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9}",
                            data.RecordTime,
                            data.UnitName,
                            data.Tag,
                            data.IntervalMs.ToString("0.###"),
                            data.StartStr,
                            data.EndStr,
                            data.AverageMs.ToString("0.###"),
                            data.MinMs.ToString("0.###"),
                            data.MaxMs.ToString("0.###"),
                            data.Count);

                        sw.WriteLine(line);
                    }
                }
                catch (Exception ex)
                {
                    Log.Write("TaktTimeBackgroundWriter", $"[File Write Fail] {ex.Message}");
                }
            }
        }

        private static string SanitizeFilePart(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return "Unnamed";
            foreach (var ch in Path.GetInvalidFileNameChars())
                s = s.Replace(ch, '_');
            return s;
        }

        private static string GetTaktLogFilePath(string unitName, string tag)
        {
            var root = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Log", "TaktTime");
            var unitDir = Path.Combine(root, SanitizeFilePart(unitName));
            Directory.CreateDirectory(unitDir);

            var tagSafe = SanitizeFilePart(tag);
            var file = $"{SanitizeFilePart(unitName)}_{tagSafe}_{DateTime.Now:yyyyMMdd}.csv";
            return Path.Combine(unitDir, file);
        }
    }
}