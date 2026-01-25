using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QMC.Common.Alarm
{
    public delegate void PostAlarmEvent(AlarmInfo alarm);
    public class AlarmManager
    {
        #region Singleton 
        private static AlarmManager g_AlarmManager;
        public static AlarmManager Instance
        {
            get
            {
                if (g_AlarmManager == null)
                    g_AlarmManager = new AlarmManager();
                return g_AlarmManager;
            }
        }
        #endregion

        // [ADD] 신규 알람이 "리스트에 추가된 순간"을 외부로 알림 (중복 추가는 발생 안 함)
        public event Action<AlarmInfo> AlarmAdded;

        private AlarmCollection m_Alarms;
        public AlarmCollection Alarms 
        { 
            get
            {
                return m_Alarms;
            }
        }

        public event PostAlarmEvent PostAlarm;
        public AlarmManager()
        {
            m_Alarms = new AlarmCollection();
        }

        // 알람 동시 발생으로 프로그램 다운 발생.
        // _lock을 사용하여 알람 리스트에 안전하게 추가하고,
        // PostAlarm 이벤트를 UI 스레드에서 실행하도록 수정.
        private readonly object _lock = new object();

        public bool IsAlarm
        {
            get
            {
                lock (_lock)
                {
                    return m_Alarms.Where(t => t.Grade.Equals("Error")).Count() > 0;
                }
            }
        }

        public void ShowAlarm(AlarmInfo alarm)
        {
            if (alarm == null)
                return;

            // [TEMP] 중복 원인 추적용 (문제 해결 후 제거 권장)
            try
            {
                Log.Write("AlarmManager", $"ShowAlarm Code={alarm.Code} Source={alarm.Source} Time={alarm.GeneratedTime:HH:mm:ss.fff}\n{Environment.StackTrace}");
            }
            catch { }

            bool added = false;

            lock (_lock)
            {
                CheckAlarmGrade(alarm.Grade);

                if (m_Alarms.Any(a => a.Code == alarm.Code))
                    return;

                m_Alarms.Add(alarm);
                added = true;
            }

            if (added)
            {
                try { AlarmAdded?.Invoke(alarm); } catch { }
                try { PostAlarm?.Invoke(alarm); } catch { }
            }
        }

        //public void ShowAlarm(AlarmInfo alarm)
        //{
        //    if (alarm == null)
        //        return;

        //    // [FIX] 호출 시점에 항상 시간 갱신(재발생/재로그용)
        //    alarm.GeneratedTime = DateTime.Now;

        //    bool added = false;
        //    bool updatedExisting = false;

        //    lock (_lock)
        //    {
        //        CheckAlarmGrade(alarm.Grade);

        //        // Code 기준으로 기존 알람 검색
        //        var exist = m_Alarms.FirstOrDefault(a => a.Code == alarm.Code);
        //        if (exist == null)
        //        {
        //            m_Alarms.Add(alarm);
        //            added = true;
        //        }
        //        else
        //        {
        //            // [FIX] Clear 없이도 "재발생/재로그"가 필요하면 기존 객체 갱신
        //            // (UI는 Binding된 객체값이 바뀌면 갱신 가능. 필요하면 RefreshAlarmView로 처리)
        //            exist.Title = alarm.Title;
        //            exist.Cause = alarm.Cause;
        //            exist.Grade = alarm.Grade;
        //            exist.Source = alarm.Source;
        //            exist.GeneratedTime = alarm.GeneratedTime;

        //            updatedExisting = true;
        //        }
        //    }

        //    // [ADD] 신규 추가 또는 기존 갱신 둘 다 외부에 알림
        //    if (added)
        //    {
        //        try { AlarmAdded?.Invoke(alarm); } catch { }
        //    }
        //    else if (updatedExisting)
        //    {
        //        // 기존 코드에는 "갱신"용 이벤트가 없어서 PostAlarm을 재사용 (History/그리드 갱신 트리거)
        //        // 필요하면 AlarmUpdated 이벤트를 별도로 만드는 게 더 깔끔함
        //    }

        //    // [FIX] History/UI가 다시 찍히게 하려면 updatedExisting도 이벤트를 날려야 함
        //    try { PostAlarm?.Invoke(alarm); } catch { }
        //}

        /// <summary>
        /// 지정한 알람을 리스트에서 제거하고 알림을 갱신합니다.
        /// </summary>
        /// <param name="alarm">해제할 알람</param>
        public void ClearAlarm(AlarmInfo alarm)
        {
            if (alarm == null) 
                return;

            lock (_lock)
            {
                if (m_Alarms.Contains(alarm))
                {
                    m_Alarms.Remove(alarm);
                }
            }

            // PostAlarm 호출은 필요 시 추가 가능
        }

        private void CheckAlarmGrade(string strGrade)
        {
            switch (strGrade)
            {
                case "EmergencyStop":
                    Stop();
                    break;
                case "Error":
                    CycleStop();
                    break;
                case "Inform":
                    break;
                case "Warning":
                    break;
            }
        }

        private void CycleStop()
        {
            //EquipmentLocator.Instance.StopAllUnitsAsync();
            EquipmentLocator.Instance.SequenceStopAllAsync(CancellationToken.None);
        }

        private void Stop()
        {
            //EquipmentLocator.Instance.StopAllUnitsAsync();
            EquipmentLocator.Instance.SequenceStopAllAsync(CancellationToken.None);
        }

        public void ClearAllAlarms()
        {
            lock (_lock)
            {
                m_Alarms.Clear();
                EquipmentLocator.Instance.EqState = EquipmentState.Stopped;
            }
        }
    }
}
