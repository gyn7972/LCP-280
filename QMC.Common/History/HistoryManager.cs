using QMC.Common.Alarm;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QMC.Common.History
{
    public class HistoryManager
    {
        #region Field
        private static List<AlarmHistory> alarms = new List<AlarmHistory>();
        private static Dictionary<string, List<AlarmHistory>> cachedAlarms = new Dictionary<string, List<AlarmHistory>>();

        // [ADD] 히스토리 중복 기록 방지 (짧은 시간 내 동일 알람 반복 트리거 차단)
        private readonly object _dedupeLock = new object();
        private readonly Dictionary<string, DateTime> _lastLoggedAtByKey =
            new Dictionary<string, DateTime>(StringComparer.OrdinalIgnoreCase);

        // 너무 짧게 잡으면 정상 흐름을 먹을 수 있으니 1초 정도로 방어
        private static readonly TimeSpan _dedupeWindow = TimeSpan.FromMilliseconds(1000);

        // [ADD] 파일 기록 동기화 (단일 프로세스 내 동시성 방지)
        private static readonly object _alarmLogFileLock = new object();
        #endregion

        #region Constructor
        public HistoryManager()
        {
            AlarmManager.Instance.PostAlarm += Instance_PostAlarm;
        }

        private void Instance_PostAlarm(AlarmInfo alarm)
        {
            if (alarm == null)
                return;

            // [ADD] 동일(Source+Code) 알람이 매우 짧은 시간 안에 중복으로 들어오면 1회만 기록
            if (IsDuplicateForHistory(alarm))
                return;

            // [ADD] CSV 기록은 여기(HistoryManager)에서만 수행
            AppendAlarmCsv(alarm);

            // 메모리/캐시 반영
            AddAlarmHistory(alarm);
        }

        private bool IsDuplicateForHistory(AlarmInfo alarm)
        {
            try
            {
                var src = alarm.Source ?? "";
                var key = src + "|" + alarm.Code.ToString();

                var now = alarm.GeneratedTime != DateTime.MinValue ? alarm.GeneratedTime : DateTime.Now;

                lock (_dedupeLock)
                {
                    if (_lastLoggedAtByKey.TryGetValue(key, out var last))
                    {
                        if ((now - last) <= _dedupeWindow)
                            return true;
                    }

                    _lastLoggedAtByKey[key] = now;

                    // 메모리 무한 증가 방지용: 너무 많아지면 대충 정리
                    if (_lastLoggedAtByKey.Count > 2000)
                    {
                        var cutoff = DateTime.Now.AddMinutes(-10);
                        var oldKeys = _lastLoggedAtByKey.Where(kv => kv.Value < cutoff).Select(kv => kv.Key).ToList();
                        foreach (var k in oldKeys)
                            _lastLoggedAtByKey.Remove(k);
                    }
                }
            }
            catch
            {
                // 디듀프 실패가 알람 기록을 막으면 안 됨
                return false;
            }

            return false;
        }

        private static string GetAlarmLogFolder()
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AlarmLog");
        }

        private static string GetAlarmLogFilePath(DateTime time)
        {
            string logFolder = GetAlarmLogFolder();
            string logFile = Path.Combine(logFolder, $"AlarmLog_{time:yyyyMMdd}.csv");
            return logFile;
        }

        private void AppendAlarmCsv(AlarmInfo alarm)
        {
            try
            {
                var t = alarm.GeneratedTime != DateTime.MinValue ? alarm.GeneratedTime : DateTime.Now;

                string logFolder = GetAlarmLogFolder();
                Directory.CreateDirectory(logFolder);

                string logFile = GetAlarmLogFilePath(t);
                bool exists = File.Exists(logFile);

                lock (_alarmLogFileLock)
                {
                    using (var fs = new FileStream(logFile, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
                    using (var sw = new StreamWriter(fs, new UTF8Encoding(true)))
                    {
                        fs.Seek(0, SeekOrigin.End);

                        if (!exists || fs.Length == 0)
                            sw.WriteLine("Date,Title,Grade,Source,Cause,Code");

                        string line = string.Format("{0},{1},{2},{3},{4},{5}",
                            Csv(t.ToString("yyyy-MM-dd HH:mm:ss")),
                            Csv(alarm.Title),
                            Csv(alarm.Grade),
                            Csv(alarm.Source),
                            Csv(alarm.Cause),
                            alarm.Code);

                        sw.WriteLine(line);
                    }
                }
            }
            catch (Exception ex)
            {
                try { Log.Write(ex); } catch { }
            }
        }

        private static string Csv(string s)
        {
            if (s == null) return "\"\"";
            // CSV quoting: " -> ""
            return "\"" + s.Replace("\"", "\"\"") + "\"";
        }
        #endregion

        #region Event
        public event EventHandler<AlarmHistory> OnAddAlarmHistory;
        #endregion

        #region Singleton 
        private static HistoryManager g_HistoryManager;
        public static HistoryManager Instance
        {
            get
            {
                if (g_HistoryManager == null)
                    g_HistoryManager = new HistoryManager();
                return g_HistoryManager;
            }
        }
        #endregion

        #region Alarm History Method
        public List<AlarmHistory> LoadTodayAlarmHistory()
        {
            return LoadAlarmHistoryByDate(DateTime.Today);
        }

        public List<AlarmHistory> LoadAlarmHistoryByDate(DateTime date)
        {
            string dateKey = date.ToString("yyyy-MM-dd");

            if (cachedAlarms.ContainsKey(dateKey))
                return cachedAlarms[dateKey];

            List<AlarmHistory> dateAlarms = new List<AlarmHistory>();
            string logFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AlarmLog");

            if (!Directory.Exists(logFolder))
                return dateAlarms;

            string fileName = $"AlarmLog_{date:yyyyMMdd}.csv";
            string filePath = Path.Combine(logFolder, fileName);

            if (File.Exists(filePath))
            {
                dateAlarms = LoadAlarmHistoryFromFile(filePath, date);
                cachedAlarms[dateKey] = dateAlarms;
            }

            return dateAlarms;
        }

        public List<AlarmHistory> LoadAlarmHistoryByDateRange(DateTime startDate, DateTime endDate)
        {
            List<AlarmHistory> result = new List<AlarmHistory>();

            for (DateTime date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
            {
                result.AddRange(LoadAlarmHistoryByDate(date));
            }

            return result.OrderByDescending(a => a.Info.GeneratedTime).ToList();
        }

        public List<AlarmHistory> LoadRecentAlarmHistory(int days = 7)
        {
            DateTime endDate = DateTime.Today;
            DateTime startDate = endDate.AddDays(-days + 1);
            return LoadAlarmHistoryByDateRange(startDate, endDate);
        }

        private void AddAlarmHistory(AlarmInfo alarm)
        {
            AlarmHistory newItem = new AlarmHistory(alarm);
            alarms.Add(newItem);

            string dateKey = alarm.GeneratedTime.ToString("yyyy-MM-dd");
            if (cachedAlarms.ContainsKey(dateKey))
            {
                cachedAlarms[dateKey].Insert(0, newItem);
            }

            OnAddAlarmHistory?.Invoke(this, newItem);
        }

        private List<AlarmHistory> LoadAlarmHistoryFromFile(string filePath, DateTime targetDate)
        {
            List<AlarmHistory> result = new List<AlarmHistory>();

            try
            {
                if (!File.Exists(filePath))
                    return result;

                var lines = File.ReadAllLines(filePath, Encoding.UTF8);

                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line)) 
                        continue;

                    if (line.StartsWith("Date,", StringComparison.OrdinalIgnoreCase)) 
                        continue; // 헤더 스킵

                    // [FIX] CSV를 Split(',')로 자르면 Title/Cause에 콤마가 있는 순간부터 파싱이 깨짐.
                    // 따옴표 포함 CSV를 처리하는 파서로 6 컬럼을 안전하게 분리한다.
                    var tokens = SplitCsvLine(line);
                    if (tokens == null || tokens.Count < 6) 
                        continue;

                    //if (!DateTime.TryParse(tokens[0], out DateTime generatedTime))
                    //    continue;
                    var dateText = (tokens[0] ?? string.Empty).Trim().Trim('\uFEFF'); // BOM 제거 포함

                    DateTime generatedTime;
                    string[] formats =
                    {
                        "yyyy-MM-dd HH:mm:ss",
                        "yyyy-MM-dd HH:mm:ss.fff",
                        "yyyy/MM/dd HH:mm:ss",
                        "yyyy/MM/dd HH:mm:ss.fff",
                        "yyyy-MM-ddTHH:mm:ss",
                        "yyyy-MM-ddTHH:mm:ss.fff",
                    };

                    if (!DateTime.TryParseExact(
                            dateText,
                            formats,
                            System.Globalization.CultureInfo.InvariantCulture,
                            System.Globalization.DateTimeStyles.None,
                            out generatedTime))
                    {
                        // 호환용 fallback (로캘/기타 포맷)
                        if (!DateTime.TryParse(dateText, out generatedTime))
                            continue;
                    }

                    if (generatedTime.Date != targetDate.Date)
                        continue;

                    string title = tokens[1];
                    string grade = tokens[2];
                    string source = tokens[3];
                    string cause = tokens[4];

                    if (!int.TryParse(tokens[5], out int code))
                        continue;

                    var alarmInfo = new AlarmInfo
                    {
                        Title = title,
                        Grade = grade,
                        Source = source,
                        Cause = cause,
                        Code = code,
                        GeneratedTime = generatedTime
                    };

                    result.Add(new AlarmHistory(alarmInfo));
                }

                result = result.OrderByDescending(a => a.Info.GeneratedTime).ToList();
            }
            catch
            {
                // 로드 실패 시 빈 리스트 반환
            }

            return result;
        }

        private static List<string> SplitCsvLine(string line)
        {
            if (line == null)
                return null;

            var result = new List<string>(6);
            var sb = new StringBuilder(line.Length);

            bool inQuotes = false;

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (c == '"')
                {
                    // "" -> " (escape)
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        sb.Append('"');
                        i++;
                        continue;
                    }

                    inQuotes = !inQuotes;
                    continue;
                }

                if (c == ',' && !inQuotes)
                {
                    result.Add(sb.ToString());
                    sb.Length = 0;
                    continue;
                }

                sb.Append(c);
            }

            result.Add(sb.ToString());
            return result;
        }

        public void ClearCache()
        {
            cachedAlarms.Clear();
        }

        public void ClearCacheByDate(DateTime date)
        {
            string dateKey = date.ToString("yyyy-MM-dd");
            if (cachedAlarms.ContainsKey(dateKey))
                cachedAlarms.Remove(dateKey);
        }
        #endregion
    }
}