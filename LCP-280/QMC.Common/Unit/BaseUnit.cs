// QMC.Common\Unit\BaseUnit.cs
using QMC.Common.Alarm;
using QMC.Common.Component;
using QMC.Common.IOUtil;
using QMC.Common.Motion;
using QMC.Common.Motions;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

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

        // 축 등록 딕셔너리
        public Dictionary<string, MotionAxis> Axes { get; } = new Dictionary<string, MotionAxis>();
        
        // 단순 키-값 Teaching 포지션 (기존 호환용)
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
                Axes[axisName] = axis; // Axes 사전에도 추가
            }
            else
            {
                if (Axes.ContainsKey(axisName))
                    Axes.Remove(axisName);
                Log.Write("UnitAxis", $"[BindAxes] Axis '{unitName}||{axisName}' 바인딩 실패");
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
        // Unit 메인 실행 루프 진입 전 호출
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

        // 추가: 준비/작업/완료 스텝
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

        #region Generic TeachingPosition Move Helpers (Reflection Based)
        // 파생 Unit Override 가능 (index 기반 인터락)
        public virtual bool IsInterlockOK(int selIndex) => true;

        // TeachingPositions (List 형태/파생 컬렉션) 리플렉션 추출
        protected IList<object> ResolveTeachingPositionObjectList()
        {
            try
            {
                var prop = GetType().GetProperty("TeachingPositions");
                if (prop == null) return null;
                var val = prop.GetValue(this, null);
                if (val is System.Collections.IEnumerable en)
                {
                    var list = new List<object>();
                    foreach (var item in en)
                        list.Add(item);
                    return list;
                }
            }
            catch { }
            return null;
        }

        private static IDictionary<string, double> GetAxisPositions(object tp)
        {
            if (tp == null) return null;
            var pi = tp.GetType().GetProperty("AxisPositions");
            if (pi == null) return null;
            var val = pi.GetValue(tp, null);
            return val as IDictionary<string, double>;
        }
        private static IDictionary<string, MotionAxis> GetAxisObjects(object tp)
        {
            if (tp == null) return null;
            var pi = tp.GetType().GetProperty("Axes");
            if (pi == null) return null;
            var val = pi.GetValue(tp, null);
            return val as IDictionary<string, MotionAxis>;
        }
        private static string GetTpName(object tp)
        {
            if (tp == null) return string.Empty;
            var pi = tp.GetType().GetProperty("Name");
            if (pi == null) return string.Empty;
            try { return pi.GetValue(tp, null) as string ?? string.Empty; } catch { return string.Empty; }
        }

        public virtual int MoveTeachingPositionOnce(int selIndex, bool isFine)
        {
            var list = ResolveTeachingPositionObjectList();
            if (list == null) return -1;
            if (selIndex < 0 || selIndex >= list.Count) return -1;
            if (!IsInterlockOK(selIndex)) return -1;

            var tp = list[selIndex];
            var axisPos = GetAxisPositions(tp);
            if (axisPos == null) return -1;
            var axisObj = GetAxisObjects(tp);

            // 이동 명령
            foreach (var kv in axisPos)
            {
                string axisKey = kv.Key; double targetPos = kv.Value;
                MotionAxis axis = null;
                if (axisObj != null && axisObj.TryGetValue(axisKey, out axis)) { }
                if (axis == null && Axes.TryGetValue(axisKey, out var directAxis)) axis = directAxis;
                if (axis == null)
                {
                    foreach (var aPair in Axes)
                    {
                        if (aPair.Value != null && string.Equals(aPair.Value.Name, axisKey, StringComparison.OrdinalIgnoreCase))
                        { axis = aPair.Value; break; }
                    }
                }
                if (axis == null) continue;
                axis.MoveAbs(targetPos, isFine);
            }

            // 완료 대기
            int waitErrors = 0;
            foreach (var kv in axisPos)
            {
                MotionAxis axis = null;
                if (axisObj != null && axisObj.TryGetValue(kv.Key, out axis)) { }
                if (axis == null && Axes.TryGetValue(kv.Key, out var directAxis)) axis = directAxis;
                if (axis == null) continue;
                if (axis.WaitMoveDone(-1) != 0) waitErrors++;
            }
            return waitErrors == 0 ? 0 : -1;
        }

        public Task<int> MoveTeachingPositionOnceAsync(int selIndex, bool isFine)
            => Task.Run(() => MoveTeachingPositionOnce(selIndex, isFine));

        public virtual void StopTeachingPositionOnce(int selIndex)
        {
            var list = ResolveTeachingPositionObjectList();
            if (list == null) return;
            if (selIndex < 0 || selIndex >= list.Count) return;
            var tp = list[selIndex];
            var axisPos = GetAxisPositions(tp);
            if (axisPos == null) return;
            var axisObj = GetAxisObjects(tp);

            foreach (var kv in axisPos)
            {
                MotionAxis axis = null;
                if (axisObj != null && axisObj.TryGetValue(kv.Key, out axis)) { }
                if (axis == null && Axes.TryGetValue(kv.Key, out var directAxis)) axis = directAxis;
                if (axis == null) continue;
                try { axis.Stop(); } catch { }
            }
        }
        #endregion
    }
}