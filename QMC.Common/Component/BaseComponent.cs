using System;
using System.Collections.Generic;
using QMC.Common;
using QMC.Common.Motion;
using QMC.Common.Unit;
using QMC.Common.Alarm;
using System.Reflection;

namespace QMC.Common.Component
{
    public abstract class BaseComponent
    {

        public class InterlockEventArgs : EventArgs
        {
            public double dTargetPosition = 0;
            public double dCurrentPosition = 0;
            public bool IsExtend = false;
        }


        public delegate bool IsInterlockOKEvent(object sender, InterlockEventArgs e);
        public event IsInterlockOKEvent IsInterlockOK;

        public bool OnIsInterlockOK(InterlockEventArgs e)
        {
            if (IsInterlockOK != null)
            {
                return this.IsInterlockOK(this, e);
            }
            return true;
        }

        public string Name { get; set; }
        public BaseUnit ParentUnit { get; set; }
        public BaseConfig Config { get; set; }

        protected Dictionary<int, AlarmInfo> m_dicAlarms;

        protected BaseComponent(string name = null)
        {
            Name = name;

            if (m_dicAlarms == null)
                m_dicAlarms = new Dictionary<int, AlarmInfo>();
            //InitAlarm();
        }

        protected virtual void InitAlarm()
        {
            // 1. 공용 파일 로더(GlobalAlarmTable)에서 현재 Component의 Name과 일치하는 알람 목록을 가져옵니다.
            //string source = this.Name;
            //var loadedAlarms = GlobalAlarmTable.Instance.GetAlarmsForSource(source);

            //if (loadedAlarms != null && loadedAlarms.Count > 0)
            //{
            //    foreach (var alarmInfo in loadedAlarms)
            //    {
            //        if (!m_dicAlarms.ContainsKey(alarmInfo.Code))
            //        {
            //            m_dicAlarms.Add(alarmInfo.Code, alarmInfo);
            //        }
            //    }
            //}
            //else
            //{
            //    // 로드된 알람이 없으면 기본 Unknown 알람 하나를 등록해 둡니다. (예외 방지용)
            //    AlarmInfo alarm = new AlarmInfo();
            //    alarm.Code = 999;
            //    alarm.Title = "Unknown Error";
            //    alarm.Cause = "알람 파일에서 해당 Source에 대한 정의를 찾지 못했습니다.";
            //    alarm.Source = Name;
            //    alarm.Grade = "Error";
            //    m_dicAlarms[alarm.Code] = alarm;
            //}

            //Sample Alarm
            //AlarmInfo alarm = new AlarmInfo();
            //alarm.Code = -999;
            //alarm.Title = "Unknown Error";
            //alarm.Cause = "";
            //alarm.Source = Name;
            //alarm.Grade = "Error";
            //m_dicAlarms.Add(alarm.Code, alarm);
        }
        public AlarmInfo GetAlarm(int nCode)
        {
            // 2. 딕셔너리에 알람이 있는지 확인 후 반환
            if (m_dicAlarms.ContainsKey(nCode))
            {
                return m_dicAlarms[nCode];
            }

            // 3. 없으면 999번 알람 반환 시도
            if (m_dicAlarms.ContainsKey(999))
            {
                return m_dicAlarms[999];
            }

            // 4. 999번마저 없다면, 프로그램이 죽지 않도록 임시 알람 객체를 만들어 반환합니다.
            return new AlarmInfo()
            {
                Code = nCode,
                Title = "Unregistered Alarm",
                Cause = $"알람코드 [{nCode}] 가 등록되어 있지 않습니다.",
                Source = this.Name,
                Grade = "Error"
            };
        }

        public virtual int Initialize()
        {
            // 초기화 시점에 알람을 로드하도록 추가
            InitAlarm();
            return 0;
        }

        public virtual int Create()
        {
            return 0;
        }

        public virtual void Close()
        {

        }

    }
}