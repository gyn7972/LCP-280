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
        #endregion

        #region Constructor
        public HistoryManager()
        {
            AlarmManager.Instance.PostAlarm += Instance_PostAlarm;
        }

        private void Instance_PostAlarm(AlarmInfo alarm)
        {
            AddAlarmHistory(alarm);
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

        /// <summary>
        /// 오늘 날짜의 알람만 로드 (기본)
        /// </summary>
        public List<AlarmHistory> LoadTodayAlarmHistory()
        {
            return LoadAlarmHistoryByDate(DateTime.Today);
        }

        /// <summary>
        /// 특정 날짜의 알람 로드
        /// </summary>
        public List<AlarmHistory> LoadAlarmHistoryByDate(DateTime date)
        {
            string dateKey = date.ToString("yyyy-MM-dd");

            // 캐시에 있으면 반환
            if (cachedAlarms.ContainsKey(dateKey))
                return cachedAlarms[dateKey];

            // 파일에서 로드
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

        /// <summary>
        /// 날짜 범위로 알람 로드
        /// </summary>
        public List<AlarmHistory> LoadAlarmHistoryByDateRange(DateTime startDate, DateTime endDate)
        {
            List<AlarmHistory> result = new List<AlarmHistory>();

            for (DateTime date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
            {
                result.AddRange(LoadAlarmHistoryByDate(date));
            }

            return result.OrderByDescending(a => a.Info.GeneratedTime).ToList();
        }

        /// <summary>
        /// 최근 N일의 알람 로드
        /// </summary>
        public List<AlarmHistory> LoadRecentAlarmHistory(int days = 7)
        {
            DateTime endDate = DateTime.Today;
            DateTime startDate = endDate.AddDays(-days + 1);
            return LoadAlarmHistoryByDateRange(startDate, endDate);
        }

        /// <summary>
        /// 실시간 알람 추가 (메모리에만 유지)
        /// </summary>
        private void AddAlarmHistory(AlarmInfo alarm)
        {
            AlarmHistory newItem = new AlarmHistory(alarm);
            alarms.Add(newItem);

            // 캐시에도 추가 (해당 날짜가 이미 로드되어 있다면)
            string dateKey = alarm.GeneratedTime.ToString("yyyy-MM-dd");
            if (cachedAlarms.ContainsKey(dateKey))
            {
                // 기존 캐시에 추가 (최신이 맨 앞)
                cachedAlarms[dateKey].Insert(0, newItem);
            }
            // 캐시에 없으면 나중에 파일에서 읽을 때 자동으로 포함됨

            OnAddAlarmHistory?.Invoke(this, newItem);
        }

        /// <summary>
        /// 파일에서 특정 날짜의 알람 로드
        /// </summary>
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
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    // CSV: 날짜,Title,Grade,Source,Cause,Code
                    var tokens = line.Split(',');
                    if (tokens.Length < 6) continue;

                    if (!DateTime.TryParse(tokens[0], out DateTime generatedTime))
                        continue;

                    // 날짜가 다르면 스킵
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

                // 최신순 정렬
                result = result.OrderByDescending(a => a.Info.GeneratedTime).ToList();
            }
            catch
            {
                // 로드 실패 시 빈 리스트 반환
            }

            return result;
        }

        /// <summary>
        /// 캐시 클리어 (메모리 관리용)
        /// </summary>
        public void ClearCache()
        {
            cachedAlarms.Clear();
        }

        /// <summary>
        /// 특정 날짜의 캐시만 클리어
        /// </summary>
        public void ClearCacheByDate(DateTime date)
        {
            string dateKey = date.ToString("yyyy-MM-dd");
            if (cachedAlarms.ContainsKey(dateKey))
                cachedAlarms.Remove(dateKey);
        }

        #endregion
    }
}