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
        #endregion

        #region Property
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
        public bool LoadAlarmHistory()
        {
            string logFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AlarmLog");
            if (!Directory.Exists(logFolder))
                return false;

            var files = Directory.GetFiles(logFolder, "AlarmLog_*.csv");
            for (int i = files.Length - 1; i >= 0; i--)
            {
                if (alarms.Count >= 500)
                    break;

                LoadAlarmHistoryFromAlarmLogFile(files[i]);
            }
            return true;
        }
        private void AddAlarmHistory(AlarmInfo alarm)
        {
            AlarmHistory newItem = new AlarmHistory(alarm);

            alarms.Add(newItem);
            if (alarms.Count > 500)
                alarms.RemoveAt(0);

            OnAddAlarmHistory?.Invoke(this, newItem);
        }
        private bool LoadAlarmHistoryFromAlarmLogFile(string filePath)
        {
            try
            {
                if (!System.IO.File.Exists(filePath))
                    return false;

                var lines = System.IO.File.ReadAllLines(filePath, Encoding.UTF8);
                foreach (var line in lines)
                {
                    // 빈 줄 무시
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    // CSV: 날짜,Title,Grade,Source,Cause,Code
                    var tokens = line.Split(',');
                    if (tokens.Length < 6) continue;

                    // 날짜 파싱
                    if (!DateTime.TryParse(tokens[0], out DateTime generatedTime))
                        continue;

                    string title = tokens[1];
                    string grade = tokens[2];
                    string source = tokens[3];
                    string cause = tokens[4];
                    if (!int.TryParse(tokens[5], out int code))
                        continue;

                    var alarmInfo = new QMC.Common.Alarm.AlarmInfo
                    {
                        Title = title,
                        Grade = grade,
                        Source = source,
                        Cause = cause,
                        Code = code,
                        GeneratedTime = generatedTime
                    };

                    // 중복 방지: 이미 같은 코드+시간이 있으면 추가하지 않음
                    if (!alarms.Any(a => a.Info.Code == code && a.Info.GeneratedTime == generatedTime))
                    {
                        AddAlarmHistory(alarmInfo);
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
        #endregion
    }
}
