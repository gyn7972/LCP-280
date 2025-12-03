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
using System.Windows.Forms.DataVisualization.Charting;
using static QMC.Common.Component.BaseComponent;
using static QMC.Common.CycleTimer;
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
            Error = -1,
            Stopped = 0,
            Starting,
            AutoRunning,
            ManualRunning,
            Stopping,
            CycleStop,
            Unknown
        }

        public enum UnitRunMode
        {
            Auto = 0,
            Manual = 1
        }

        //Autto시에 사용되는 ProcessStage다.
        //OnRun 에서 사용한다고 생각하자.
        public enum ProcessState
        {
            None = 0,
            Stop = 1,
            Ready = 2,
            Work = 3,
            Complete = 4,
            Error = 5
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


        public UnitRunMode RunMode { get; set; } = UnitRunMode.Manual;
        public bool IsAutoMode => RunMode == UnitRunMode.Auto;
        public bool IsManualMode => RunMode == UnitRunMode.Manual;


        private UnitStatus _runUnitStatus = UnitStatus.Stopped;
        public UnitStatus RunUnitStatus
        {
            get => _runUnitStatus;
            set
            {
                if (_runUnitStatus == value) 
                    return;

                _runUnitStatus = value;
                var eq = EquipmentLocator.Instance as IEquipment;
                //eq.TryGet(out var eq);
                eq?.SetAndRaiseUnitState(this.UnitName, value);

                //EquipmentLocator.Instance.TryGet(out var eq);
                //eq?.SetAndRaiseUnitState(this.UnitName, value);
            }
        }
        
        public bool IsRunning => RunUnitStatus == UnitStatus.AutoRunning;
        public bool IsStop => RunUnitStatus == UnitStatus.Stopped; 
        public bool IsCycleStop => RunUnitStatus == UnitStatus.CycleStop;

        public ProcessState State { get; set; }



        public CancellationTokenSource CalcelToken { get;  set; }
        // 등록 축 사전 (Key: 논리 축명)
        public Dictionary<string, MotionAxis> Axes { get; } = new Dictionary<string, MotionAxis>();
        public List<TeachingPosition> TeachingPositions
        {
            get => Config != null ? (Config.TeachingPositions ?? new List<TeachingPosition>()) : new List<TeachingPosition>();
            private set
            {
                if (Config == null)
                {
                    // Initialize Config if possible or ignore safely
                    // If Config cannot be created here, just noop to avoid NRE
                    return;
                }
                Config.TeachingPositions = value ?? new List<TeachingPosition>();
            }
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


        private static readonly object _alarmLogLock = new object();
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
        public int PostAlarm(int alarmCode)
        {
            try
            {
                this.State = ProcessState.Error;
                this.RunUnitStatus = UnitStatus.Error;

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



        private Material m_currentMaterial;

        public virtual void AddComponents() { }
        public bool IsEndTask(Task<int> task)
            => task.IsCompleted || task.IsFaulted || task.IsCanceled;

        public double GetDistance(double deltaX, double deltaY)
        {
            return Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
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
            RunUnitStatus = UnitStatus.AutoRunning;
            
            this.CalcelToken = new CancellationTokenSource();
            return OnStart();
        }

        public int StartManual()
        {
            SetRunMode(UnitRunMode.Auto);
            RunUnitStatus = UnitStatus.ManualRunning;

            return 0;
        }

        private void SetRunMode(UnitRunMode mode) => RunMode = mode;

        protected virtual int OnStart() => 0;

        public int Stop() => OnStop();

        public virtual int OnRun() => 0;

        public virtual int OnStop()
        {

            SetRunMode(UnitRunMode.Manual);
            this.RunUnitStatus = UnitStatus.Stopped;
            //this.State = ProcessState.Stop;         // <-이걸 여기서 하면 안되는거지. RunUnitStatus만 보고 멈춰야지.
            
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

                if ((ret = OnRun()) != 0)
                {
                    //Log.Write(this, $"OnRun Return: {ret}");
                    OnStop();
                    //break;
                }
                
                Thread.Sleep(1);
            }
            //OnStop();
        }

        public void Terminate() { m_bExit = true; }
        public string GetUnitName() => UnitName;


        public Material GetMaterial()
        {
            return m_currentMaterial;
        }

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
        public static IDictionary<string, MotionAxis> GetAxisObjects(object tp)
        {
            if (tp == null) return null;
            var pi = tp.GetType().GetProperty("Axes");
            if (pi == null) return null;
            return pi.GetValue(tp, null) as IDictionary<string, MotionAxis>;
        }


        //public virtual Task<int> MoveTeachingPositionOnceAsync(int selIndex, bool isFine)
        //    => Task.Run(() => MoveTeachingPositionOnce(selIndex, isFine));
        public virtual Task<int> MoveTeachingPositionOnceAsync(int selIndex, bool isFine)
        {
            var token = this.CalcelToken?.Token ?? CancellationToken.None;

            return Task.Run(() =>
            {
                try
                {
                    int result = MoveTeachingPositionOnce(selIndex, isFine);
                    return result;
                }
                catch (Exception ex)
                {
                    Log.Write(ex);
                    return -1;
                }
            }, token);
        }

        public virtual int MoveTeachingPositionOnce(int selIndex, bool isFine)
        {
            int waitErrors = 0;
            string teachName = string.Empty;
            bool bSuccssed = Config.GetTeachingPositionName(selIndex, out teachName);

            if (bSuccssed == false)
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
                {
                    IsAuto = true;
                }
                else
                {
                    IsAuto = false;
                }
                waitErrors = axis.MoveAbs(target, IsAuto, isFine);
            }

            if (waitErrors != 0)
            {
                return -1;
            }

            foreach (var kv in axisPos)
            {
                MotionAxis axis = null;
                double target = kv.Value;

                if (axisObj != null && axisObj.TryGetValue(kv.Key, out axis)) { }
                if (axis == null && Axes.TryGetValue(kv.Key, out var directAxis))
                    axis = directAxis;

                if (axis == null)
                    continue;


                double timeoutMs = 2000;
                if (timeoutMs < 0) timeoutMs = axis.Setup.MoveTimeoutMs;
                var sw = Stopwatch.StartNew();
                while (sw.ElapsedMilliseconds < timeoutMs)
                {
                    if (axis.InPosition(target))
                    {
                        break;
                    }
                    Thread.Sleep(1);
                }

                if (axis.WaitMoveDone(-1) != 0 && axis.InPosition(target) == false)
                {
                    waitErrors++;
                }
            }
            return waitErrors == 0 ? 0 : -1;
        }

        protected virtual bool IsAxisAtTarget(MotionAxis axis, double target,
                                              double multiplier = 2.0,
                                              int stableSamples = 5,
                                              int sampleDelayMs = 2,
                                              double minEpsilon = 0.010)
        {
            if (axis == null) return true;

            // 드라이버 자체 InPosition 먼저 이용 (정확하면 빠르게 통과)
            try
            {
                if (axis.InPosition(target))
                    return true;
            }
            catch { }

            double tol = 0.0;
            try
            {
                tol = axis.Config != null ? Math.Max(0.0, axis.Config.InposTolerance) : 0.0;
            }
            catch { }
            double relaxedTol = (tol * multiplier) + minEpsilon;

            // 축 이동이 아직 끝나지 않았다면 불안정
            if (!axis.IsMoveDone())
                return false;

            int okCount = 0;
            for (int i = 0; i < stableSamples; i++)
            {
                double cur;
                try { cur = axis.GetPosition(); }
                catch { return false; }

                if (double.IsNaN(cur) || double.IsInfinity(cur))
                    return false;

                if (Math.Abs(cur - target) <= relaxedTol)
                {
                    okCount++;
                    if (okCount >= stableSamples)
                        return true;
                }
                else
                {
                    okCount = 0; // 연속 조건 끊김
                }

                if (sampleDelayMs > 0)
                    Thread.Sleep(sampleDelayMs);
            }
            return false;
        }


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

            bool IsDone = axis.IsMoveDone();
            if(IsDone == false)
            {
                return !IsDone;
            }
            else
            {
                return !IsDone;
            }
            //return !axis.IsMoveDone();
        }
        // 이동 완료 여부 바로 얻고 싶을 때(가독성):
        public virtual bool IsAxisStopped(string axisKeyOrName) => !IsAxisMoving(axisKeyOrName);
        // 하나라도 이동 중이면 true, 전부 멈췄으면 false
        public virtual bool IsAnyAxisMoving()
        {
            foreach (var ax in Axes.Values)
            {
                if (ax != null && ax.IsMoveDone() == false)
                    return true;
            }
            return false;
        }
        #endregion

        #region 공통 Safety 축 이동
        /// <summary>
        /// 단일 축 안전 이동(비동기).
        /// </summary>
        public virtual Task<int> MoveAxisPositionOneAsync(MotionAxis axis, double target, bool isFine = false)
            => Task.Run(() => OnMoveAxisPositionOne(axis, target, isFine));
        /// <summary>
        /// 실제 이동 실행 (파생 Override 가능).
        /// </summary>
        /// 
        //Todo: 20251105 이 함수 완료 신호 받을떄 포지션값 비교도 같이 해야함.!
        public virtual int OnMoveAxisPositionOne(MotionAxis axis, double target, bool isFine = false)
        {
            if (axis == null) 
                return -1;

            try
            {
                //LogAxisMove(axis, target, isFine);
                var cfg = axis.Config;
                //double cur = axis.GetPosition();
                double cur = axis.Status.PV.ActualPosition;
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

                double timeoutMs = 2000;
                if (timeoutMs < 0) timeoutMs = axis.Setup.MoveTimeoutMs;
                var sw = Stopwatch.StartNew();
                while (sw.ElapsedMilliseconds < timeoutMs)
                {
                    if (axis.InPosition(target))
                    {
                        break;
                    }
                    Thread.Sleep(1);
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
                //LogAxisMoveDone(axis, target, isFine);
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

        //Position 확인
        // BaseUnit 클래스 내부에 추가: Teaching 전용 판정 파라미터(필요 시 파생 클래스에서 override 가능)
        protected virtual double TeachingInposToleranceMultiplier => 2.5; // InposTolerance를 몇 배로 완화할지
        protected virtual double TeachingInposEpsilon => 0.010;//1e-6;            // 부동소수 잡음 보정
        protected virtual int TeachingInposStableSampleCount => 5;         // 안정 샘플 횟수
        protected virtual int TeachingInposSampleDelayMs => 2;             // 샘플 간 간격(ms)
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
            if (ax.IsMoveDone() == false)
            {
                return false;
            }

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
        public void WaitByTime(int milliseconds, int pollMs = 2)
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


        #region TaktTime (공통 계측/저장)
        private readonly object _taktLock = new object();
        private readonly Dictionary<string, QMC.Common.CycleTimer> _taktTimers
            = new Dictionary<string, QMC.Common.CycleTimer>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// 태그별 CycleTimer 가져오거나 생성. 기본 Capacity=100
        /// </summary>
        protected QMC.Common.CycleTimer GetOrCreateTaktTimer(string tag, int capacity = 100)
        {
            if (string.IsNullOrWhiteSpace(tag)) tag = "Unnamed";
            lock (_taktLock)
            {
                if (!_taktTimers.TryGetValue(tag, out var ct))
                {
                    ct = new QMC.Common.CycleTimer(this) { Capacity = capacity };
                    _taktTimers[tag] = ct;
                }
                return ct;
            }
        }

        /// <summary>
        /// 태그 시작(측정 시작)
        /// </summary>
        public void TaktStart(string tag)
        {
            try
            {
                var ct = GetOrCreateTaktTimer(tag);
                ct.Start();
            }
            catch (Exception ex)
            {
                Log.Write(UnitName ?? "BaseUnit", $"[TaktStart:{tag}] {ex.Message}");
            }
        }

        /// <summary>
        /// 태그 종료(측정 종료 및 파일 저장). IntervalMs <= 0은 저장 생략
        /// </summary>
        public void TaktEnd(string tag, bool saveToFile = true)
        {
            try
            {
                var ct = GetOrCreateTaktTimer(tag);
                ct.End();

                if (!saveToFile) return;

                var latest = ct.Latest;
                var intervalMs = latest.Interval.TotalMilliseconds;

                // IntervalMs가 0 이하인 경우 저장 안 함
                if (intervalMs <= 0) return;

                AppendTaktCsv(UnitName ?? GetType().Name, tag, ct, latest);
            }
            catch (Exception ex)
            {
                Log.Write(UnitName ?? "BaseUnit", $"[TaktEnd:{tag}] {ex.Message}");
            }
        }

        /// <summary>
        /// 현재까지의 태그별 평균/최소/최대/개수 요약을 CSV로 저장(스냅샷).
        /// IntervalMs <= 0인 최신 항목은 생략
        /// </summary>
        public void SaveAllTaktSummaries()
        {
            try
            {
                lock (_taktLock)
                {
                    foreach (var kv in _taktTimers)
                    {
                        var tag = kv.Key;
                        var ct = kv.Value;
                        var latest = ct.Latest;
                        if (latest.Interval.TotalMilliseconds <= 0) continue; // 생략
                        AppendTaktCsv(UnitName ?? GetType().Name, tag, ct, latest);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Write(UnitName ?? "BaseUnit", $"[SaveAllTaktSummaries] {ex.Message}");
            }
        }

        private static string SanitizeFilePart(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return "Unnamed";
            foreach (var ch in Path.GetInvalidFileNameChars())
                s = s.Replace(ch, '_');
            return s;
        }

        protected virtual string GetTaktLogRootPath()
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Log", "TaktTime");
        }

        protected virtual string GetTaktLogFilePath(string unitName, string tag)
        {
            var root = GetTaktLogRootPath();
            var unitDir = Path.Combine(root, SanitizeFilePart(unitName ?? "Unit"));
            Directory.CreateDirectory(unitDir);

            var tagSafe = SanitizeFilePart(tag);
            var file = $"{SanitizeFilePart(unitName)}_{tagSafe}_{DateTime.Now:yyyyMMdd}.csv";
            return Path.Combine(unitDir, file);
        }

        private void AppendTaktCsv(string unitName, string tag, QMC.Common.CycleTimer timer, QMC.Common.CycleTime latest)
        {
            try
            {
                // 방어: IntervalMs <= 0 은 저장하지 않음
                double intervalMs = latest.Interval.TotalMilliseconds;
                if (intervalMs <= 0) return;

                var path = GetTaktLogFilePath(unitName, tag);
                var exists = File.Exists(path);

                // 요약값
                double avgMs = timer.Average.TotalMilliseconds;
                double minMs = timer.Minimum.TotalMilliseconds;
                double maxMs = timer.Maximum.TotalMilliseconds;
                int count = timer.CycleTimes.Count;

                var startStr = latest.Start != DateTime.MinValue
                               ? latest.Start.ToString("yyyy-MM-dd HH:mm:ss.fff")
                               : "";
                var endStr = latest.End != DateTime.MinValue
                               ? latest.End.ToString("yyyy-MM-dd HH:mm:ss.fff")
                               : "";

                using (var fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
                using (var sw = new StreamWriter(fs, Encoding.UTF8))
                {
                    fs.Seek(0, SeekOrigin.End);
                    if (!exists || fs.Length == 0)
                    {
                        sw.WriteLine("Date,Unit,Tag,IntervalMs,Start,End,AverageMs,MinMs,MaxMs,Count");
                    }
                    var line = string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9}",
                        DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                        unitName,
                        tag,
                        intervalMs.ToString("0.###"),
                        startStr,
                        endStr,
                        avgMs.ToString("0.###"),
                        minMs.ToString("0.###"),
                        maxMs.ToString("0.###"),
                        count);
                    sw.WriteLine(line);
                }
            }
            catch (Exception ex)
            {
                Log.Write(UnitName ?? "BaseUnit", $"[AppendTaktCsv:{tag}] {ex.Message}");
            }
        }

        /// <summary>
        /// 읽기 전용으로 현재 태그 타이머 맵을 반환
        /// </summary>
        public IReadOnlyDictionary<string, QMC.Common.CycleTimer> TaktTimers
        {
            get
            {
                lock (_taktLock)
                {
                    return new Dictionary<string, QMC.Common.CycleTimer>(_taktTimers);
                }
            }
        }
        #endregion

    }
}