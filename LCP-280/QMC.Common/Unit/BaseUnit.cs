// QMC.Common\Unit\BaseUnit.cs
using QMC.Common.Alarm;
using QMC.Common.Component;
using QMC.Common.IOUtil;
using QMC.Common.Motion;
using QMC.Common.Motions;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Xml.Linq;

namespace QMC.Common.Unit
{
    public class BaseUnit
    {
        public enum AlarmKeys
        {
            ePrepareFailed = 1000,
            
        }

        public enum RunStatus
        {
            Run,
            Stop,
            CycleStop,
        }


        protected Dictionary<int, AlarmInfo> m_dicAlarms;
        private bool m_bExit;
        
        public string UnitName { get; set; }
        public List<BaseComponent> Components { get; } = new List<BaseComponent>();
        public BaseConfig Config { get; internal set; }

        // 공용 축 컴포넌트
        public Dictionary<string, MotionAxis> Axes { get; } = new Dictionary<string, MotionAxis>();
        
        // 공용 티칭 위치 관리
        public Dictionary<string, double> TeachingPositions { get; } = new Dictionary<string, double>();

        protected BaseUnit(string unitName = null)
        {
            UnitName = unitName;
            MakeAlarm();
        }

        private void MakeAlarm()
        {
            InitAlarm();

        }

        protected virtual void InitAlarm()
        {
            
            AlarmInfo alarm = new AlarmInfo();
            alarm.Code = (int)AlarmKeys.ePrepareFailed;
            alarm.Title = "PrepareFialed";
            alarm.Cause = "PrepareFialed";
            alarm.Source = this.UnitName;
            alarm.Grade = "Error";
            m_dicAlarms.Add(alarm.Code, alarm);
        }

        private Material m_currentMaterial = null;


        public virtual void AddComponents() { }

        // 축 이동
        public virtual int MoveAxis(string axisKey, double pos, double vel = 5, double acc = 10, double dec = 10, double jerk = 50)
        {
            if (Axes.TryGetValue(axisKey, out var axis))
                return axis.MoveAbs(pos, vel, acc, dec, jerk);
            return -1;
        }

        // 티칭 위치 저장/로드
        public virtual void SetTeachingPosition(string key, double pos)
        {
            TeachingPositions[key] = pos;
        }
        public virtual double GetTeachingPosition(string key, double defaultValue = 0)
        {
            if (TeachingPositions.TryGetValue(key, out var pos))
                return pos;
            return defaultValue;
        }

        public void BindAxis(MotionAxisManager mgr, string unitName, string axisName, ref MotionAxis field)
        {
            if (mgr.TryGet(unitName, axisName, out var axis) && axis != null)
            {
                field = axis;
                Axes[axisName] = axis; // Axes 딕셔너리에 일관 등록
            }
            else
            {
                if (Axes.ContainsKey(axisName))
                    Axes.Remove(axisName);
                Log.Write("UnitAxis", $"[BindAxes] Axis '{unitName}||{axisName}' 미존재");
            }
        }
        public int Start()
        {
            return OnStart();
        }

        protected virtual int OnStart()
        {
            int ret = 0;
            return ret;
        }
        public int Stop()
        {
            return OnStop();
        }
        // Unit 공통 동작 메서드
        public virtual int OnRun() 
        {
            int ret = 0;
            return ret;
        }
        public virtual int OnStop() 
        { 
            int ret = 0;
            m_bExit = true;
            return ret;
        }

        // 추가: 상태 단계별 훅
        protected virtual int OnRunReady() { return 0; }
        protected virtual int OnRunWork() { return 0; }
        protected virtual int OnRunComplete() { return 0; }

        protected void OnMainProcedure()
        {
            //int ret = 0;
            int ret = OnPrepareToMainProcedure();
            if (ret != 0)
            {
                AlarmInfo alarm = this.GetAlarm((int)AlarmKeys.ePrepareFailed);
                AlarmManager.Instance.ShowAlarm(alarm);

                return;
            }
            while (true)
            {
                if (m_bExit)
                {
                    break;
                }
                if ((ret = OnRun()) != 0)
                {
                    Log.Write(this, string.Format("OnRun Return Value : {0}", ret));
                    break;
                }
                Thread.Sleep(1);
            }

            OnStop();
        }

        public Material GetMaterial()
        {
            return m_currentMaterial;
        }

        protected void SetMaterial(Material wd)
        {
            m_currentMaterial = wd;
        }
        protected AlarmInfo GetAlarm(int nCode)
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
        private int OnPrepareToMainProcedure()
        {
            throw new NotImplementedException();
        }
    }
}