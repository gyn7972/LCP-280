// QMC.Common\Unit\BaseUnit.cs
using Ivi.Visa;
using QMC.Common;
using QMC.Common.Alarm;
using QMC.Common.Component;
using QMC.Common.IOUtil;
using QMC.Common.Motion;
using QMC.Common.Motions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static QMC.Common.Motions.MotionAxis;

namespace QMC.Common.Unit
{
    public interface IUnit : IDisposable
    {
        BaseConfig Config { get; }
        void SetName(string name);
    }

    public class BaseUnit : IUnit
    {
        public enum AlarmKeys
        {
            ePrepareFailed = 1000,
        }

        public enum UnitRunStatus
        {
            Run,
            Stop,
            CycleStop,
        }
        public enum UnitRunMode
        {
            Manual,
            Auto,
        }

        public enum ProcessState
        {
            None = 0,
            Stop = 1,
            Ready = 2,
            Work = 3,
            Complete = 4,
            Error = 5,
        }

        protected Dictionary<int, AlarmInfo> m_dicAlarms;
        private bool m_bExit;

        public string UnitName { get; set; }
        public List<BaseComponent> Components { get; } = new List<BaseComponent>();
        public BaseConfig Config { get; internal set; }
        public Thread m_workThread { get; set; }
        public UnitRunStatus RunStatus { get; set; } = UnitRunStatus.Stop;
        public UnitRunMode RunMode { get; set; } = UnitRunMode.Manual;
        public bool IsRunning => RunStatus == UnitRunStatus.Run;
        public bool IsAutoMode => RunMode == UnitRunMode.Auto;
        public bool IsManualMode => RunMode == UnitRunMode.Manual;
        public bool IsCycleStop => RunStatus == UnitRunStatus.CycleStop;

        public UnitRunStatus Status { get; protected set; }
        public ProcessState State { get; protected set; }

        // 등록 축 사전 (Key: 논리 축명)
        public Dictionary<string, MotionAxis> Axes { get; } = new Dictionary<string, MotionAxis>();

        public List<TeachingPosition> TeachingPositions
        {
            get => Config.TeachingPositions;
            private set => Config.TeachingPositions = value;
        }

        protected BaseUnit(string unitName)
        {
            UnitName = unitName;
            m_dicAlarms = new Dictionary<int, AlarmInfo>();
            MakeAlarm();
        }

        private void MakeAlarm()
        {
            m_dicAlarms = new Dictionary<int, AlarmInfo>();
            InitAlarm();
        }

        protected virtual void InitAlarm()
        {
            // 원래 코드 구조 유지 (AlarmPost가 먼저 호출되는 구조 그대로 둠)
            AlarmRegister(AlarmPost((int)AlarmKeys.ePrepareFailed), "PrepareFialed", "PrepareFialed", "Error");
        }

        protected void AlarmRegister(int alarmCode, string title, string cause, string grade)
        {
            if (m_dicAlarms.ContainsKey(alarmCode)) return;
            var alarm = new AlarmInfo
            {
                Code = alarmCode,
                Title = title,
                Cause = cause,
                Source = UnitName,
                Grade = grade
            };
            m_dicAlarms.Add(alarm.Code, alarm);
        }

        private Material m_currentMaterial;

        public virtual void AddComponents() { }

        public bool IsEndTask(Task<int> task)
            => task.IsCompleted || task.IsFaulted || task.IsCanceled;

        private static readonly object _alarmLogLock = new object();

        public int AlarmPost(int alarmCode)
        {
            try
            {
                AlarmInfo alarm = GetAlarm(alarmCode);
                alarm.GeneratedTime = DateTime.Now;

                if (AlarmManager.Instance.Alarms.Any(a => a.Code == alarm.Code))
                    return alarmCode;

                Log.Write("AlarmPost", $"[ALARM] Code:{alarmCode} Grade:{alarm.Grade} Cause:{alarm.Cause}");

                string logFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AlarmLog");
                string logFile = Path.Combine(logFolder, $"AlarmLog_{DateTime.Now:yyyyMMdd}.csv");
                Directory.CreateDirectory(logFolder);

                lock (_alarmLogLock)
                {
                    using (var fs = new FileStream(logFile, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                    using (var writer = new StreamWriter(fs, new UTF8Encoding(true)))
                    {
                        string logLine = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss},{alarm.Title},{alarm.Grade},{alarm.Source},{alarm.Cause},{alarm.Code}";
                        writer.WriteLine(logLine);
                    }
                }

                AlarmManager.Instance.ShowAlarm(alarm);
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
            return alarmCode;
        }

        // 단순 축 이동 (공통)
        public virtual int MoveAxis(string axisKey, double pos, double vel = 5, double acc = 10, double dec = 10, double jerk = 50)
        {
            if (Axes.TryGetValue(axisKey, out var axis))
                return axis.MoveAbs(pos, vel, acc, dec, jerk);
            return -1;
        }

        public void BindAxis(MotionAxisManager mgr, string unitName, string axisName, ref MotionAxis field)
        {
            if (mgr != null && mgr.TryGet(unitName, axisName, out var axis) && axis != null)
            {
                field = axis;
                Axes[axisName] = axis;
            }
            else
            {
                if (Axes.ContainsKey(axisName)) Axes.Remove(axisName);
                Log.Write("UnitAxis", $"[BindAxes] Axis '{unitName}||{axisName}' 바인딩 실패");
            }
        }

        public void BindUnit() => OnBindUnit();
        protected virtual void OnBindUnit() { }

        public int Start()
        {
            SetRunMode(UnitRunMode.Auto);
            m_bExit = false;
            m_workThread = new Thread(OnMainProcedure) { IsBackground = true };
            m_workThread.Start();
            return OnStart();
        }

        private void SetRunMode(UnitRunMode mode) => RunMode = mode;

        protected virtual int OnStart() => 0;

        public int Stop() => OnStop();

        public virtual int OnRun() => 0;

        public virtual int OnStop()
        {
            m_bExit = true;
            SetRunMode(UnitRunMode.Manual);
            return 0;
        }

        protected virtual int OnRunReady() => 0;
        protected virtual int OnRunWork() => 0;
        protected virtual int OnRunComplete() => 0;

        protected void OnMainProcedure()
        {
            try
            {
                if (string.IsNullOrEmpty(Thread.CurrentThread.Name))
                    Thread.CurrentThread.Name = string.IsNullOrWhiteSpace(UnitName) ? GetType().Name : UnitName;
            }
            catch { }

            int ret = OnPrepareToMainProcedure();
            if (ret != 0)
            {
                var alarm = GetAlarm((int)AlarmKeys.ePrepareFailed);
                AlarmManager.Instance.ShowAlarm(alarm);
                return;
            }

            while (true)
            {
                if (m_bExit) break;
                if ((ret = OnRun()) != 0)
                {
                    Log.Write(this, $"OnRun Return: {ret}");
                    break;
                }
                Thread.Sleep(1);
            }
            OnStop();
        }

        public string GetUnitName() => UnitName;
        public Material GetMaterial() => m_currentMaterial;
        protected void SetMaterial(Material m) => m_currentMaterial = m;

        protected AlarmInfo GetAlarm(int code)
        {
            if (m_dicAlarms.ContainsKey(code))
                return m_dicAlarms[code];
            // fallback (999 없으면 신규 생성)
            if (!m_dicAlarms.ContainsKey(999))
            {
                m_dicAlarms[999] = new AlarmInfo
                {
                    Code = 999,
                    Title = "Unknown",
                    Cause = "Unknown",
                    Source = UnitName,
                    Grade = "Warning"
                };
            }
            return m_dicAlarms[999];
        }

        protected virtual int OnPrepareToMainProcedure() => 0;

        #region TeachingPosition Helpers (기존 구조 유지)
        public virtual bool IsInterlockOK(int selIndex) => true;

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
                    foreach (var item in en) list.Add(item);
                    return list;
                }
            }
            catch { }
            return null;
        }

        protected static IDictionary<string, double> GetAxisPositions(object tp)
        {
            if (tp == null) return null;
            var pi = tp.GetType().GetProperty("AxisPositions");
            if (pi == null) return null;
            return pi.GetValue(tp, null) as IDictionary<string, double>;
        }

        private static IDictionary<string, MotionAxis> GetAxisObjects(object tp)
        {
            if (tp == null) return null;
            var pi = tp.GetType().GetProperty("Axes");
            if (pi == null) return null;
            return pi.GetValue(tp, null) as IDictionary<string, MotionAxis>;
        }

        private static string GetTpName(object tp)
        {
            if (tp == null) return string.Empty;
            var pi = tp.GetType().GetProperty("Name");
            if (pi == null) return string.Empty;
            try { return pi.GetValue(tp, null) as string ?? string.Empty; }
            catch { return string.Empty; }
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

            foreach (var kv in axisPos)
            {
                string key = kv.Key;
                double target = kv.Value;
                MotionAxis axis = null;
                if (axisObj != null && axisObj.TryGetValue(key, out axis)) { }
                if (axis == null && Axes.TryGetValue(key, out var direct)) axis = direct;
                if (axis == null)
                {
                    foreach (var ap in Axes)
                    {
                        if (ap.Value != null &&
                            (ap.Key.Equals(key, StringComparison.OrdinalIgnoreCase) ||
                             ap.Value.Name.Equals(key, StringComparison.OrdinalIgnoreCase)))
                        {
                            axis = ap.Value;
                            break;
                        }
                    }
                }
                if (axis == null) continue;
                axis.MoveAbs(target, isFine);
            }

            Thread.Sleep(500);

            // 완료 대기
            int waitErrors = 0;
            while(true)
            {
                bool allDone = true;
                foreach (var kv in axisPos)
                {
                    MotionAxis axis = null;
                    if (axisObj != null && axisObj.TryGetValue(kv.Key, out axis)) { }

                    if (axis == null && Axes.TryGetValue(kv.Key, out var directAxis)) 
                        axis = directAxis;

                    if (axis == null) 
                        continue;

                    if (!axis.IsMoveDone()) 
                    { 
                        allDone = false; 
                        break; 
                    }
                }
                if (allDone) break;
                Thread.Sleep(0);
            }

            foreach (var kv in axisPos)
            {
                MotionAxis axis = null;
                if (axisObj != null && axisObj.TryGetValue(kv.Key, out axis)) { }
                if (axis == null && Axes.TryGetValue(kv.Key, out var direct)) axis = direct;
                if (axis == null) continue;
                double dTarget = kv.Value;
                if (axis.InPosition(dTarget) == false)
                { 
                    Log.Write("MoveTeachingPositionOnce", 
                        $"[티칭 이동 오류] '{GetTpName(tp)}' 축 '{kv.Key}' 목표 {dTarget}, 현재 {axis.GetPosition()}");
                    waitErrors++; 
                }
            }
            return err == 0 ? 0 : -1;
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
                if (axis == null && Axes.TryGetValue(kv.Key, out var direct)) axis = direct;
                if (axis == null) continue;
                try { axis.Stop(); } catch { }
            }
        }

        public virtual bool IsAxisMoving(string axisKeyOrName)
        {
            if (string.IsNullOrWhiteSpace(axisKeyOrName) || Axes.Count == 0)
                return false;

            MotionAxis axis = null;
            if (!Axes.TryGetValue(axisKeyOrName, out axis))
            {
                foreach (var kv in Axes)
                {
                    if (kv.Value == null) continue;
                    if (kv.Value.Name.Equals(axisKeyOrName, StringComparison.OrdinalIgnoreCase) ||
                        kv.Key.Equals(axisKeyOrName, StringComparison.OrdinalIgnoreCase))
                    {
                        axis = kv.Value;
                        break;
                    }
                }
            }
                return axis != null && !axis.IsMoveDone();
        }

        public virtual bool IsAnyAxisMoving()
        {
            foreach (var ax in Axes.Values)
            {
                if (ax != null && !ax.IsMoveDone())
                    return true;
            }
            return false;
        }

        public virtual IDictionary<string, bool> GetAxesMovingMap()
        {
            var map = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
            foreach (var kv in Axes)
            {
                if (kv.Value != null)
                    map[kv.Key] = !kv.Value.IsMoveDone();
            }
            return map;
        }
        #endregion

        #region 공통 Safety 축 이동
        /// <summary>
        /// 축 Key 또는 MotionAxis.Name 으로 안전 이동.
        /// </summary>
        public virtual int MoveAxisWithSafety(string axisKeyOrName, double target, bool isFine = false)
        {
            var axis = ResolveAxis(axisKeyOrName);
            if (axis == null)
            {
                Log.Write(UnitName, "MoveAxisWithSafety", $"Axis not found : {axisKeyOrName}");
                return -1;
            }
            return MoveAxisWithSafety(axis, target, isFine);
        }

        /// <summary>
        /// 단일 축 안전 이동(동기). CheckMoveSafety != 0 이면 모든 축 EmgStop 후 알람.
        /// </summary>
        public virtual int MoveAxisWithSafety(MotionAxis axis, double target, bool isFine = false)
        {
            if (axis == null) return -1;

            var task = MoveAxisWithSafetyAsync(axis, target, isFine);
            while (!IsEndTask(task))
            {
                int alarmCode = CheckMoveSafety(axis);
                if (alarmCode != 0)
                {
                    foreach (var ax in Axes.Values)
                    {
                        try { ax?.EmgStop(); } catch { }
                    }
                    AlarmPost(alarmCode);
                    return -1;
                }
                Thread.Sleep(0);
            }
            return task.Result;
        }

        /// <summary>
        /// 단일 축 안전 이동(비동기).
        /// </summary>
        public virtual Task<int> MoveAxisWithSafetyAsync(MotionAxis axis, double target, bool isFine = false)
            => Task.Run(() => OnMoveAxisWithSafety(axis, target, isFine));

        /// <summary>
        /// 실제 이동 실행 (파생 Override 가능).
        /// </summary>
        protected virtual int OnMoveAxisWithSafety(MotionAxis axis, double target, bool isFine)
        {
            if (axis == null) return -1;

            var cfg = axis.Config;
            double cur = axis.GetPosition();
            if (cfg != null && Math.Abs(cur - target) <= cfg.InposTolerance)
                return 0;

            double vel = cfg != null ? cfg.MaxVelocity : 0;
            if (isFine && vel > 0) vel *= 0.2;

            int rc;
            if (cfg != null)
                rc = axis.MoveAbs(target, vel, cfg.RunAcc, cfg.RunDec, cfg.AccJerkPercent);
            else
                rc = axis.MoveAbs(target, false);

            if (rc != 0)
            {
                Log.Write(UnitName, "MoveAxisWithSafety",
                    $"MoveAbs Fail axis={axis.Name} rc={rc}");
                return -1;
            }

            if (axis.WaitMoveDone(-1) != 0)
            {
                Log.Write(UnitName, "MoveAxisWithSafety",
                    $"WaitMoveDone Timeout axis={axis.Name}");
                return -1;
            }
            return 0;
        }

        /// <summary>
        /// Safety 인터락 검사 (0=OK, 알람코드!=0 → 중단/알람).
        /// 파생 클래스에서 조건 구현.
        /// </summary>
        protected virtual int CheckMoveSafety(MotionAxis movingAxis) => 0;

        /// <summary>
        /// 기본 축 검색 (Key 우선 → Name 매칭). 파생에서 필요 시 Override.
        /// </summary>
        protected virtual MotionAxis ResolveAxis(string axisKeyOrName)
        {
            if (string.IsNullOrWhiteSpace(axisKeyOrName))
                return null;

            if (Axes.TryGetValue(axisKeyOrName, out var ax) && ax != null)
                return ax;

            foreach (var kv in Axes)
            {
                if (kv.Value == null) continue;
                if (kv.Key.Equals(axisKeyOrName, StringComparison.OrdinalIgnoreCase) ||
                    kv.Value.Name.Equals(axisKeyOrName, StringComparison.OrdinalIgnoreCase))
                    return kv.Value;
            }
            return null;
        }
        #endregion

        #region IDisposable
        private bool _disposed;
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;
            if (disposing)
            {
                try { m_workThread = null; } catch { }
            }
            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void SetName(string name) => UnitName = name;

        ~BaseUnit()
        {
            Dispose(false);
        }
        #endregion
    }
}