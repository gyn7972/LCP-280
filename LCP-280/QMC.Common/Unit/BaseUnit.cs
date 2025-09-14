// QMC.Common\Unit\BaseUnit.cs
using Ivi.Visa;
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

        // 축 등록 딕셔너리
        public Dictionary<string, MotionAxis> Axes { get; } = new Dictionary<string, MotionAxis>();
        public List<TeachingPosition> TeachingPositions
        {
            get
            {
                return Config.TeachingPositions;
            }
            private set
            {

                Config.TeachingPositions = value;
            }
        }




        protected BaseUnit(string unitName)
        {
            
            UnitName = unitName;
            m_dicAlarms = new Dictionary<int, AlarmInfo>(); // <-- Add this line
            MakeAlarm();
        }

        private void MakeAlarm()
        {
            m_dicAlarms = new Dictionary<int, AlarmInfo>();
            InitAlarm();

        }

        protected virtual void InitAlarm()
        {

            AlarmRegister(AlarmPost((int)AlarmKeys.ePrepareFailed), "PrepareFialed", "PrepareFialed", "Error");
        }

        protected void AlarmRegister(int alarmCode, string title, string cause,string grade)
        {
            AlarmInfo alarm = new AlarmInfo();
            alarm.Code = (int)alarmCode;
            alarm.Title = title;
            alarm.Cause = cause;
            alarm.Source = this.UnitName;
            alarm.Grade = grade;
            m_dicAlarms.Add(alarm.Code, alarm);
        }

        private Material m_currentMaterial = null;


        public virtual void AddComponents() { }
        public bool IsEndTask(Task<int> task)
        {
            return task.IsCompleted || task.IsFaulted || task.IsCanceled;
        }

        private static readonly object _alarmLogLock = new object();
        // 알람 발생시 사용.
        public int AlarmPost(int AlarmCode)
        {
            try
            {
                AlarmInfo alarm = GetAlarm((int)AlarmCode);
                alarm.GeneratedTime = DateTime.Now;

                // 중복 알람 방지 인터락
                if (AlarmManager.Instance.Alarms.Any(a => a.Code == alarm.Code))
                {
                    //Log.Write("AlarmPost", $"[ALARM 무시 - 중복] Code: {(int)AlarmCode}, 이미 발생 중인 알람입니다.");
                    return (int)AlarmCode;
                }

                // 알람 정보 로그 기록
                Log.Write("AlarmPost", $"[ALARM 발생] Code: {(int)AlarmCode}, Grade: {alarm.Grade}, Cause: {alarm.Cause}");

                string logFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AlarmLog");
                string logFile = Path.Combine(logFolder, $"AlarmLog_{DateTime.Now:yyyyMMdd}.csv");
                Directory.CreateDirectory(logFolder);

                // UTF-8 with BOM로 저장 (동시 접근 안전하게 lock 처리)
                lock (_alarmLogLock)
                {
                    using (var fs = new FileStream(logFile, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                    using (var writer = new StreamWriter(fs, new UTF8Encoding(true)))
                    {
                        string logLine = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss},{alarm.Title},{alarm.Grade},{alarm.Source},{alarm.Cause},{(int)AlarmCode}";
                        writer.WriteLine(logLine);
                    }
                }

                if (alarm.Grade.Equals("Error"))
                {
                    // 장비 내부 멈춰야 하는 이것저것
                    //this.m_LoaderWork_Start = false;
                }
                AlarmManager.Instance.ShowAlarm(alarm);
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }

            return (int)AlarmCode;
        }


        // 축 이동
        public virtual int MoveAxis(string axisKey, double pos, double vel = 5, double acc = 10, double dec = 10, double jerk = 50)
        {
            if (Axes.TryGetValue(axisKey, out var axis))
                return axis.MoveAbs(pos, vel, acc, dec, jerk);
            return -1;
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
        public void BindUnit()
        {
            OnBindUnit();
        }

        protected virtual void OnBindUnit()
        {
            
        }

        public int Start()
        {
            SetRunMode(UnitRunMode.Auto);
            m_bExit = false;
            m_workThread = new Thread(new ThreadStart(OnMainProcedure));
            m_workThread.Start();

            return OnStart();
        }

        private void SetRunMode(UnitRunMode auto)
        {
            this.RunMode = auto;
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
            // 현재 워커 스레드 이름을 유닛 이름으로 설정 (이미 설정되어 있으면 유지)
            try
            {
                if (string.IsNullOrEmpty(Thread.CurrentThread.Name))
                {
                    Thread.CurrentThread.Name = string.IsNullOrWhiteSpace(UnitName) ? GetType().Name : UnitName;
                }
            }
            catch { /* ignore if thread name already set */ }

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

        public string GetUnitName()
        {   
            return UnitName;
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
        protected virtual int OnPrepareToMainProcedure()
        {
            
            return 0;
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

        protected static IDictionary<string, double> GetAxisPositions(object tp)
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
            while(true)
            {
                bool allDone = true;
                foreach (var kv in axisPos)
                {
                    MotionAxis axis = null;
                    if (axisObj != null && axisObj.TryGetValue(kv.Key, out axis)) { }
                    if (axis == null && Axes.TryGetValue(kv.Key, out var directAxis)) axis = directAxis;
                    if (axis == null) continue;
                    if (!axis.IsMoveDone()) { allDone = false; break; }
                }
                if (allDone) break;
                Thread.Sleep(0);
            }
            foreach (var kv in axisPos)
            {
                MotionAxis axis = null;
                if (axisObj != null && axisObj.TryGetValue(kv.Key, out axis)) { }
                if (axis == null && Axes.TryGetValue(kv.Key, out var directAxis)) axis = directAxis;
                if (axis == null) continue;
                double dTarget = kv.Value;
                if (axis.InPosition(dTarget) == false)
                { 
                    Log.Write("MoveTeachingPositionOnce", $"[티칭 이동 오류] '{GetTpName(tp)}' 축 '{kv.Key}' 목표 {dTarget}, 현재 {axis.GetPosition()}");
                    waitErrors++; 
                }
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
        ~BaseUnit()
        {
            Dispose(false);
        }
        #endregion
    }
}