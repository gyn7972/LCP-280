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
            AlarmInfo alarm = new AlarmInfo();
            alarm.Code = -999;
            alarm.Title = "Unknown Error";
            alarm.Cause = "";
            alarm.Source = Name;
            alarm.Grade = "Error";
            m_dicAlarms.Add(alarm.Code, alarm);
        }
        public AlarmInfo GetAlarm(int nCode)
        {
            AlarmInfo alarm = null;
            if (m_dicAlarms.ContainsKey(nCode))
            {
                alarm = m_dicAlarms[nCode];
            }
            else
            {
                alarm = m_dicAlarms[999];
            }

            return alarm;
        }

        public virtual int Initialize()
        {
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