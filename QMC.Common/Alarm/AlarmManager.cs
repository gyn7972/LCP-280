using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public bool IsAlarm { 
            get 
            {
                return m_Alarms.Where(t=>t.Grade.Equals("Error")).Count()> 0;
            }
        }

        // 알람 동시 발생으로 프로그램 다운 발생.
        // _lock을 사용하여 알람 리스트에 안전하게 추가하고,
        // PostAlarm 이벤트를 UI 스레드에서 실행하도록 수정.
        private readonly object _lock = new object();

        public void ShowAlarm(AlarmInfo alarm)
        {
            lock (_lock)
            {
                // 1. 알람 리스트에 안전하게 추가
                CheckAlarmGrade(alarm.Grade);
                if (m_Alarms.Where(a=>a.Code == alarm.Code).Count() ==0)
                {
                    m_Alarms.Add(alarm);
                }
                else
                {
                    return;
                }
            }

            // 2. PostAlarm 이벤트 (UI 스레드에서 실행)
            if (PostAlarm != null)
            {
                if(true)
                {
                    // UI 폼이 없이 알람 발생.
                    PostAlarm?.Invoke(alarm);

                    //var eq = EquipmentLocator.Instance;
                    //eq.EqState = EquipmentState.Error;
                    //var state = eq?.EqState ?? EquipmentState.Unknown;

                    //Test임.
                    //var mb = new MessageBoxOk();
                    //mb.Text = "알람 발생";
                    //mb.Message = $"[{alarm.Grade}] {alarm.Title}\n{alarm.Cause}";
                    //mb.ShowDialog(); // UI 스레드에서 실행
                }
                else
                {
                    if (Application.OpenForms.Count > 0)
                    {
                        var form = Application.OpenForms[0];
                        if (form.InvokeRequired)
                        {
                            form.BeginInvoke(new Action(() => PostAlarm?.Invoke(alarm)));
                        }
                        else
                        {
                            PostAlarm?.Invoke(alarm);
                        }
                    }
                    else
                    {
                        // UI 폼이 없으면 그냥 호출 (예: 콘솔 앱)
                        PostAlarm?.Invoke(alarm);
                    }
                }
            }

            // 4. 마지막 알람 메세지를 Title bar 에 보이게
            //m_Alarms.Add(alarm);
            //if (PostAlarm != null)
            //{
            //    PostAlarm(alarm);
            //}
        }

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
            EquipmentLocator.Instance.StopAllUnitsAsync();
        }

        private void Stop()
        {
            EquipmentLocator.Instance.StopAllUnitsAsync();
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
