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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static QMC.Common.Component.BaseComponent;
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

        /// <summary>
        /// Unit 상태
        /// </summary>
        public enum UnitStatus
        {
            Stopped = 0,
            Starting,
            Running,
            Stopping,
            CycleStop,
            Error,
            Unknown
        }

        public enum UnitRunMode
        {
            Auto = 0,
            Manual = 1
        }

        public enum ProcessState
        {
            None = 0,
            Stop = 1,
            Ready = 2,
            Work = 3,
            Complete = 4,
            Error = 5,
            Manual = 6
        }

        protected Dictionary<int, AlarmInfo> m_dicAlarms;
        private bool m_bExit;

        protected readonly List<Func<bool, int>> _sequencePlayers;
        private int _currentIndex = 0;

        public List<Func<bool, int>> SequencePlayers
        {
            get             
            {
                return _sequencePlayers;
            }
        }
        private Func<bool, int> _currentFunc = null;
        public Func<bool, int> CurrentFunc
        {
            get
            {
                return _currentFunc;
            }
            set
            {
                if(_currentFunc != null)
                {

                    //Log.Write(this.UnitName, "Before Function : " + _currentFunc.Method.Name);
                }
                if(value !=null)
                {
                    //Log.Write(this.UnitName, "Current Function : " + value.Method.Name);
                }
                 _currentFunc = value;
                _currentIndex = _sequencePlayers.IndexOf(value);
                if (_currentIndex == -1)
                    _currentIndex = 0;
                else
                    _currentIndex = (_currentIndex + 1) % _sequencePlayers.Count;
            }
        }

        public string UnitName { get; set; }
        public List<BaseComponent> Components { get; } = new List<BaseComponent>();
        public BaseConfig Config { get; internal set; }
        public Thread m_workThread { get; set; }

        //public UnitStatus RunUnitStatus { get; set; } = UnitStatus.Stopped;
        private UnitStatus _runUnitStatus = UnitStatus.Stopped;
        public UnitStatus RunUnitStatus
        {
            get => _runUnitStatus;
            set
            {
                if (_runUnitStatus == value) return;
                _runUnitStatus = value;

                var eq = EquipmentLocator.Instance as IEquipment;
                //eq.TryGet(out var eq);
                eq?.SetAndRaiseUnitState(this.UnitName, value);

                //EquipmentLocator.Instance.TryGet(out var eq);
                //eq?.SetAndRaiseUnitState(this.UnitName, value);
            }
        }
        public UnitRunMode RunMode { get; set; } = UnitRunMode.Manual;

        public bool IsRunning => RunUnitStatus == UnitStatus.Running;
        public bool IsStop => RunUnitStatus == UnitStatus.Stopped;
        public bool IsAutoMode => RunMode == UnitRunMode.Auto;
        public bool IsManualMode => RunMode == UnitRunMode.Manual;
        public bool IsCycleStop => RunUnitStatus == UnitStatus.CycleStop;
        
        public CancellationTokenSource CalcelToken { get;  set; }

        public ProcessState State { get; set; }
        
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
            _sequencePlayers = new List<Func<bool, int>>();
            OnMakeSequence();
            MakeAlarm();
        }

        protected virtual void OnMakeSequence()
        {
            
        }

        private void MakeAlarm()
        {
            m_dicAlarms = new Dictionary<int, AlarmInfo>();
            InitAlarm();
        }

        protected virtual void InitAlarm()
        {
            // 원래 코드 구조 유지 (AlarmPost가 먼저 호출되는 구조 그대로 둠)
            //AlarmRegister(PostAlarm((int)AlarmKeys.ePrepareFailed), "PrepareFialed", "PrepareFialed", "Error");
            //AlarmRegister((int)AlarmKeys.ePrepareFailed, "PrepareFialed", "PrepareFialed", "Error");
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

        public double GetDistance(double deltaX, double deltaY)
        {
            return Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
        }

        public int PostAlarm(int alarmCode)
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

        public void BindAxis(MotionAxisManager mgr, string unitName, string axisName, ref MotionAxis field)
        {
            if (mgr != null && mgr.TryGet(unitName, axisName, out var axis) && axis != null)
            {
                field = axis;
                axis.IsInterlockOK += Axis_IsInterlockOK; ;

                Axes[axisName] = axis;
            }
            else
            {
                if (Axes.ContainsKey(axisName)) Axes.Remove(axisName);
                Log.Write("UnitAxis", $"[BindAxes] Axis '{unitName}||{axisName}' 바인딩 실패");
            }
        }

        public void BindCylinder(Cylinder field)
        {
            if (field != null)
            {
                field.IsInterlockOK += Axis_IsInterlockOK;
            }
        }


        private bool Axis_IsInterlockOK(object sender, InterlockEventArgs e)
        {
            if (sender is BaseComponent baseComponent)
            {                
                return IsInterlockOK(baseComponent , e);
            }
            return true;
        }

        public MotionAxis GetAxis(string axisName)
        {
            if (Axes.TryGetValue(axisName, out var axis))
                return axis;
            return null;
        }
        public int GetAxisIndex(string axisName)
        {
            int index = 0;
            foreach (var key in Axes.Keys)
            {
                if (key.Equals(axisName, StringComparison.OrdinalIgnoreCase))
                    return index;
                index++;
            }
            return -1;
        }

        public void BindUnit() => OnBindUnit();
        protected virtual void OnBindUnit() { }

        public int Start()
        {
            m_bExit = false;
            if (m_workThread == null )
            {
                m_workThread = new Thread(OnMainProcedure) { IsBackground = true };
                m_workThread.Start();
            }
            else
            {

            }

            SetRunMode(UnitRunMode.Auto);
            RunUnitStatus = UnitStatus.Running;
            
           
           
            this.CalcelToken = new CancellationTokenSource();
            return OnStart();
        }

        private void SetRunMode(UnitRunMode mode) => RunMode = mode;

        protected virtual int OnStart() => 0;

        public int Stop() => OnStop();

        public virtual int OnRun() => 0;

        public virtual int OnStop()
        {
            
            
            this.RunUnitStatus = UnitStatus.Stopped;
            this.State = ProcessState.Stop;
            
            
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
                if(IsStop)
                {
                    SetRunMode(UnitRunMode.Manual);
                }
                if (m_bExit)
                    break;

                if (State == ProcessState.Manual)
                {
                    switch (EquipmentLocator.Instance.EqState)
                    {
                        case EquipmentState.Stopped:
                        case EquipmentState.Error:
                        case EquipmentState.Stopping:
                           
                            break;
                        default:
                            break;
                    }
                    Thread.Sleep(1); 
                    continue;
                }
                


                if ((ret = OnRun()) != 0)
                {
                    Log.Write(this, $"OnRun Return: {ret}");
                    OnStop();
                    //break;
                }
                
                Thread.Sleep(1);
            }
            //OnStop();
        }
        public void Terminate() { m_bExit = true; }
        public string GetUnitName() => UnitName;
        public Material GetMaterial() => m_currentMaterial;
        //public virtual void SetMaterial(Material m) => m_currentMaterial = m;
        public virtual void SetMaterial(Material m)
        {
            m_currentMaterial = m;
        }

        public virtual void MoveMaterial(Material  material , BaseUnit destinyUnit)
        {
            Material temp = GetMaterial();
            
            destinyUnit?.SetMaterial(temp);
            SetMaterial(material);
        }
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
                    Grade = "Error"
                };
            }
            return m_dicAlarms[999];
        }

        protected virtual int OnPrepareToMainProcedure() => 0;



        #region TeachingPosition Helpers (기존 구조 유지)
        public virtual bool IsInterlockOK(BaseComponent baseComponent, InterlockEventArgs e) => true;
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
            string teachName = string.Empty;
            bool bSuccssed = Config.GetTeachingPositionName(selIndex,out teachName);

            if(bSuccssed==false)
            {
                Log.Write(UnitName, "MoveTeachingPositionOnce", $"[TEACH 이동 오류] 인덱스 '{selIndex}' 티칭포지션 이름을 찾을 수 없습니다.");
                return -1;
            }
            TeachingPosition tp = TeachingPositions.FirstOrDefault(t => t.Name == teachName);

            var axisPos = GetAxisPositions(tp);
            if (axisPos == null) 
                return -1;
            var axisObj = GetAxisObjects(tp);

            //** 티칭포지션 중에 Z축이랑 묶여있는 거는 
            //** 무조건 Z축이 먼저 움직이고 구동되도록 해야함!!!
            foreach (var kv in axisPos)
            {
                string key = kv.Key;
                double target = kv.Value;
                MotionAxis axis = null;
                if (axisObj != null && axisObj.TryGetValue(key, out axis)) { }
                if (axis == null && Axes.TryGetValue(key, out var direct)) 
                    axis = direct;

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
                if (axis == null) 
                    continue;

                bool IsAuto = false;
                if (RunMode == UnitRunMode.Auto)
                    IsAuto = true;
                else
                    IsAuto = false;

                axis.MoveAbs(target, IsAuto, isFine);
            }

            // 완료 대기
            int waitErrors = 0;
            foreach (var kv in axisPos)
            {
                MotionAxis axis = null;
                if (axisObj != null && axisObj.TryGetValue(kv.Key, out axis)) { }
                if (axis == null && Axes.TryGetValue(kv.Key, out var directAxis))
                    axis = directAxis;

                if (axis == null)
                    continue;

                if (axis.WaitMoveDone(-1) != 0)
                {
                    waitErrors++;
                }
            }
            return waitErrors == 0 ? 0 : -1;

            //기존 문제 코드
            {
                //while (true)
                //{
                //    bool allDone = true;
                //    foreach (var kv in axisPos)
                //    {
                //        MotionAxis axis = null;
                //        if (axisObj != null && axisObj.TryGetValue(kv.Key, out axis)) { }
                //        if (axis == null && Axes.TryGetValue(kv.Key, out var direct)) axis = direct;
                //        if (axis == null) continue;

                //        // 완료 + InPosition을 동시에 만족해야 통과
                //        if (!axis.IsMoveDone() || !axis.InPosition(kv.Value))
                //        {
                //            allDone = false;
                //            break;
                //        }
                //    }
                //    if (allDone) break;
                //    Thread.Sleep(10);
                //}
                //int err = 0;
                //foreach (var kv in axisPos)
                //{
                //    MotionAxis axis = null;
                //    if (axisObj != null && axisObj.TryGetValue(kv.Key, out axis)) { }
                //    if (axis == null && Axes.TryGetValue(kv.Key, out var direct)) axis = direct;
                //    if (axis == null) continue;
                //    if (!axis.InPosition(kv.Value))
                //    {
                //        Log.Write("MoveTeachingPositionOnce",
                //            $"[TEACH 이동 오류] '{GetTpName(tp)}' 축 '{kv.Key}' 목표 {kv.Value}, 현재 {axis.GetPosition()}");
                //        err++;
                //    }
                //}
                //return err == 0 ? 0 : -1;
            }
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

        // NEW: 올바른 의미. 이동 중이면 true.
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
                        kv.Key.Equals(axisKeyOrName, StringComparison.OrdinalIgnoreCase) ||
                        kv.Value.Name.Equals(axisKeyOrName, StringComparison.OrdinalIgnoreCase))
                    {
                        axis = kv.Value;
                        break;
                    }
                }
            }
            if (axis == null) 
                return false;
            return !axis.IsMoveDone();
        }
        // 이동 완료 여부 바로 얻고 싶을 때(가독성):
        public virtual bool IsAxisStopped(string axisKeyOrName) => !IsAxisMoving(axisKeyOrName);


        // 하나라도 이동 중이면 true, 전부 멈췄으면 false
        public virtual bool IsAnyAxisMoving()
        {
            foreach (var ax in Axes.Values)
            {
                if (ax != null && !ax.IsMoveDone())
                    return true;
            }
            return false;
        }
        // 모든 축이 멈췄는지 확인 (가독성 보조용)
        public virtual bool AreAllAxesStopped()
        {
            foreach (var ax in Axes.Values)
            {
                if (ax != null && !ax.IsMoveDone())
                    return false;
            }
            return true;
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
            return OnMoveAxisPositionOne(axis, target, isFine);
        }
        /// <summary>
        /// 단일 축 안전 이동(비동기).
        /// </summary>
        public virtual Task<int> MoveAxisPositionOneAsync(MotionAxis axis, double target, bool isFine = false)
            => Task.Run(() => OnMoveAxisPositionOne(axis, target, isFine));
        /// <summary>
        /// 실제 이동 실행 (파생 Override 가능).
        /// </summary>
        public virtual int OnMoveAxisPositionOne(MotionAxis axis, double target, bool isFine = false)
        {
            if (axis == null) 
                return -1;

            try
            {
                LogAxisMove(axis, target, isFine);
                var cfg = axis.Config;
                double cur = axis.GetPosition();
                if (cfg != null && Math.Abs(cur - target) <= cfg.InposTolerance)
                {
                    return 0;
                }

                int rc = 0;
                bool IsAuto = false;
                if (RunMode == UnitRunMode.Auto)
                    IsAuto = true;
                else
                    IsAuto = false;

                rc = axis.MoveAbs(target, IsAuto, isFine);
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
            }
            catch (Exception ex )
            {
                Log.Write(ex);
                return -1;

            }finally
            {
                LogAxisMoveDone(axis, target, isFine);
            }
            
            return 0;
        }
        protected void LogAxisMoveDone(MotionAxis axis, double target, bool isFine)
        {
            Log.Write(UnitName, "MoveAxisWithSafety",
                $"MoveAxisWithSafety Done axis={axis.Name} target={target} isFine={isFine} " +
                $"cur={axis.GetPosition()}");
        }
        protected void LogAxisMove(MotionAxis axis, double target, bool isFine)
        {
            Log.Write(UnitName, "MoveAxisWithSafety",
                $"MoveAxisWithSafety axis={axis.Name} target={target} isFine={isFine} " +
                $"cur={axis.GetPosition()}");
        }
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



        //Position 확인
        // BaseUnit 클래스 내부에 추가: Teaching 전용 판정 파라미터(필요 시 파생 클래스에서 override 가능)
        protected virtual double TeachingInposToleranceMultiplier => 2.5; // InposTolerance를 몇 배로 완화할지
        protected virtual double TeachingInposEpsilon => 0.010;//1e-6;            // 부동소수 잡음 보정
        protected virtual int TeachingInposStableSampleCount => 5;         // 안정 샘플 횟수
        protected virtual int TeachingInposSampleDelayMs => 8;             // 샘플 간 간격(ms)
                                                                           // BaseUnit 클래스 내부에 추가: Teaching 전용 InPosition 판정
        protected bool InPosTeachingAxis(MotionAxis ax, double target)
        {
            if (ax == null) return true;

            // 1) 드라이버/축 자체 판정이 이미 OK면 통과
            if (ax.InPosition(target)) 
                return true;

            // 2) Teaching 전용 완화 허용오차 계산
            var tol = ax.Config != null ? Math.Max(0.0, ax.Config.InposTolerance) : 0.0;
            var relaxedTol = (tol * TeachingInposToleranceMultiplier) + TeachingInposEpsilon;

            // 이동 중이면 아직 도달 아님
            if (!ax.IsMoveDone()) 
                return false;

            // 3) 디바운싱: 짧게 N회 연속 허용오차 내 유지되는지 확인
            for (int i = 0; i < TeachingInposStableSampleCount; i++)
            {
                var cur = ax.GetPosition();
                if (double.IsNaN(cur) || double.IsInfinity(cur)) 
                    return false;

                if (Math.Abs(cur - target) > relaxedTol)
                    return false;

                if (TeachingInposSampleDelayMs > 0)
                    Thread.Sleep(TeachingInposSampleDelayMs);
            }
            return true;
        }


        public bool InPosTeaching(TeachingPosition tp)
        {
            if (tp == null)
                return false;
            return InPosTeaching(tp.Name);
        }
        public bool InPosTeaching(string positionName)
        {
            var tp = Config.GetTeachingPosition(positionName);
            if (tp == null)
                return false;

            foreach (var kv in tp.AxisPositions)
            {
                if (!Axes.TryGetValue(kv.Key, out var axis))
                    return false;

                if (!InPosTeachingAxis(axis, kv.Value))
                    return false;
            }
            return true;
        }
        //public bool InPosTeaching(string positionName)
        //{
        //    var tp = Config.GetTeachingPosition(positionName);

        //    if (tp == null) 
        //        return false;

        //    foreach (var kv in tp.AxisPositions)
        //        if (!Axes.TryGetValue(kv.Key, out var axis) || !InPos(axis, kv.Value)) 
        //            return false;

        //    return true;
        //}

        public bool InPos(MotionAxis ax, double target) => ax == null || ax.InPosition(target);

        public double GetTP(string tpName, string axisName)
        {
            var tp = Config.GetTeachingPosition(tpName);
            if (tp != null && tp.AxisPositions != null && tp.AxisPositions.TryGetValue(axisName, out var v)) 
                return v;

            return 0.0;
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

        
        public Task<int> RunManualFunction(Func<bool, int> func)
        {
            Task<int> task = null;
            
            if (func != null && !IsRunning)
            {
                CurrentFunc = func;
                if (this.CalcelToken == null || this.CalcelToken.IsCancellationRequested) 
                {
                    this.CalcelToken?.Dispose();
                    this.CalcelToken = new CancellationTokenSource(); 
                }
                task = Task.Factory.StartNew(() =>
                    {
                        int ret = 0;
                        try
                        {
                            if (string.IsNullOrEmpty(Thread.CurrentThread.Name))
                            {
                                string threadName = "ManualRun_" + UnitName + "_" + CurrentFunc.Method.Name;
                                Thread.CurrentThread.Name = threadName;
                            }
                        }
                        catch (Exception ex) 
                        { Log.Write(ex); }
                        
                        try
                        {
                            ret = func(false);
                        }
                        catch(Exception ex)
                        {   Log.Write(ex); }

                        return ret;
                    }
                );
            }
            return task;
        }

        public void CancelSequence()
        {
            try
            {
                this.CalcelToken?.Cancel();
            }
            catch (Exception ex)
            {
                Log.Write(ex);

            }
        }

        ~BaseUnit()
        {
            Dispose(false);
        }
        #endregion

        #region Timing Helpers
        public void WaitByTime(int milliseconds, int pollMs = 1)
        {
            if (milliseconds <= 0) return;
            if (pollMs < 0) pollMs = 0;

            var sw = Stopwatch.StartNew();
            while (sw.ElapsedMilliseconds < milliseconds)
            {
                Thread.Sleep(pollMs);
            }
        }

        public Task<int> MoveAxisPositionOneAsync(string axisName, double dTargetPos, bool isFine)
        {
            MotionAxis axis = GetAxis(axisName);
            Task<int> task = null;
            if (axis != null)
            {
                task = MoveAxisPositionOneAsync(axis, dTargetPos, isFine);
            }
            return task;
        }
        #endregion
    }
}