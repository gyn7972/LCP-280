using System;
using System.Collections.Generic;
using System.Linq;

namespace QMC.Common
{
    public class AlarmManager
    {
        private static readonly Lazy<AlarmManager> _instance = new Lazy<AlarmManager>(() => new AlarmManager());
        public static AlarmManager Instance => _instance.Value;

        private readonly List<AlarmInfo> _alarms = new List<AlarmInfo>();

        // 알람 발생/해제 이벤트
        public event Action<AlarmInfo> AlarmRaised;
        public event Action<AlarmInfo> AlarmCleared;

        private AlarmManager() { }

        // 알람 발생
        public void RaiseAlarm(int id, string message)
        {
            var alarm = _alarms.FirstOrDefault(a => a.Id == id && a.IsActive);
            if (alarm == null)
            {
                var newAlarm = new AlarmInfo(id, message);
                _alarms.Add(newAlarm);
                AlarmRaised?.Invoke(newAlarm);
            }
        }

        // 알람 해제
        public void ClearAlarm(int id)
        {
            var alarm = _alarms.FirstOrDefault(a => a.Id == id && a.IsActive);
            if (alarm != null)
            {
                alarm.IsActive = false;
                AlarmCleared?.Invoke(alarm);
            }
        }

        // 현재 활성 알람 목록 조회
        public IEnumerable<AlarmInfo> GetActiveAlarms()
        {
            return _alarms.Where(a => a.IsActive).ToList();
        }

        // 전체 알람 목록 조회
        public IEnumerable<AlarmInfo> GetAllAlarms()
        {
            return _alarms.ToList();
        }
    }
}