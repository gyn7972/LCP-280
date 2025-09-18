using QMC.Common;
using QMC.Common.Alarm;
using QMC.Common.Component;
using QMC.Common.Motion;
using QMC.Common.Motions;
using QMC.Common.Unit;
using QMC.LCP_280.Process.Component;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static QMC.LCP_280.Process.Equipment;

namespace QMC.LCP_280.Process.Unit
{
    /// <summary>
    /// IndexLoadAligner Unit
    ///  - Align T / Index Z 축 Teaching Positions 관리
    ///  - OutputStage 스타일 Region/메서드 구조 적용
    ///  - 현재 별도 IO 없음 (추후 필요 시 IO Mapping 추가)
    /// </summary>
    public class IndexLoadAligner : BaseUnit<IndexLoadAlignerConfig>
    {
        public enum AlarmKeys
        {
            eIndexLoadAligner = 4701,
            eRotaryNotSafe = 4702,
        }

        #region InitAlarm
        protected override void InitAlarm()
        {
            base.InitAlarm();
            AlarmInfo alarm = new AlarmInfo();
            alarm.Code = (int)AlarmKeys.eRotaryNotSafe;
            alarm.Title = "Rorary Not Sfarety Pos.";
            alarm.Cause = "Rorary가 안전 위치가 아닙니다.\n 포지션 확인 후 다시 시작 하십시요.";
            alarm.Source = this.UnitName;
            alarm.Grade = AlarmInfo.AlarmType.Warning.ToString();
            m_dicAlarms.Add(alarm.Code, alarm);

        }
        #endregion

        #region Config / Teaching
        public IndexLoadAlignerConfig IndexLoadAlignerConfig => Config;
        #endregion

        #region Unit
        Rotary Rotary { get; set; }
        #endregion

        #region Axes
        private MotionAxis _alignT, _indexZ;
        public MotionAxis AxisAlignT => _alignT;
        public MotionAxis AxisIndexZ => _indexZ;
        #endregion

        #region ctor / Initialization
        public IndexLoadAligner(IndexLoadAlignerConfig config = null) : base(new IndexLoadAlignerConfig())
        {
            
            AddComponents();
        }

        public override void AddComponents()
        {
            Config.LoadAndBindAxes(Equipment.Instance.AxisManager);
            Config.InitializeDefaultTeachingPositions();
            
            BindAxes();
        }
        #endregion

        #region Axis Binding / Helpers
        protected override void OnBindUnit()
        {
            base.OnBindUnit();
            Rotary = Equipment.Instance.GetUnit(UnitKeys.Rotary) as Rotary;
        }

        private void BindAxes()
        {
            var mgr = Equipment.Instance?.AxisManager;
            if (mgr == null)
            {
                Log.Write("IndexLoadAligner", "[BindAxes] AxisManager null");
                return;
            }

            const string unitName = "Unit"; // Equipment.CreateAxes 에서 사용한 유닛명과 동일해야 함
            BindAxis(mgr, unitName, AxisNames.AlignT, ref _alignT);
            BindAxis(mgr, unitName, AxisNames.IndexZ, ref _indexZ);
        }

        public override int MoveAxisPositionOne(MotionAxis axis, double target, bool isFine = false)
        {
            if (axis == null) return -1;

            Task<int> task = MoveAxisWithSafetyAsync(axis, target, isFine);
            while (IsEndTask(task) == false)
            {
                if(Rotary.IsAnyAxisMoving())
                {
                    AxisIndexZ.EmgStop();
                    AxisAlignT.EmgStop();
                    AlarmPost((int)AlarmKeys.eRotaryNotSafe);
                    return -1;
                }
                Thread.Sleep(1);
            }
            return task.Result;
        }

        public int MovePositionSafetyZ(bool isFine = false)
        {
            Task<int> task = MovePositionAsyncSafetyZ(isFine);
            while (IsEndTask(task) == false)
            {
                int nRtn = IsMoveInterLockSafetyZ();
                if (nRtn != 0)
                {
                    return -1;
                }

                Thread.Sleep(0);
            }
            return task.Result;
        }
        private Task<int> MovePositionAsyncSafetyZ(bool isFine = false)
        {
            return Task.Run(() =>
            {
                OnMovePositionSafetyZ(isFine);
                return 0;
            });
        }
        private int OnMovePositionSafetyZ(bool isFine = false)
        {
            return MoveTeachingPositionOnce((int)IndexLoadAlignerConfig.TeachingPositionName.SafetyZone, isFine);
        }
        private int IsMoveInterLockSafetyZ()
        {
            int nRet = 0;

            if (Rotary != null && Rotary.IsAnyAxisMoving())
            {
                AxisIndexZ.EmgStop();
                AxisAlignT.EmgStop();
                AlarmPost((int)AlarmKeys.eRotaryNotSafe);
                return -1;
            }

            if(IsAxisMoving(AxisNames.AlignT))
            {
                AxisIndexZ.EmgStop();
                AxisAlignT.EmgStop();
                AlarmPost((int)AlarmKeys.eIndexLoadAligner);
                return -1;
            }

            return nRet;
        }
        public Task<int> MovePositionAsyncSafeSafetyZ(bool isFine = false, CancellationToken ct = default(CancellationToken))
        {
            return Task.Run(() =>
            {
                // Task로 돌리고 별도 인터락/취소 감시
                var coreTask = Task.Run(() => OnMovePositionSafetyZ(isFine), ct);

                while (!IsEndTask(coreTask))
                {
                    if (ct.IsCancellationRequested)
                    {
                        try
                        {
                            AxisIndexZ?.EmgStop();
                            AxisAlignT?.EmgStop();
                        }
                        catch { }
                        return -999; // 취소 코드
                    }

                    int nRtn = IsMoveInterLockSafetyZ();
                    if (nRtn != 0)
                    {
                        return -1;
                    }

                    Thread.Sleep(5); // 0→5ms로 약간 여유 (CPU 점유 감소)
                }

                return coreTask.Result;
            }, ct);
        }


        public int MovePositionAlignUp(int nIndex = 0, bool isFine = false)
        {
            Task<int> task = MovePositionAsyncAlignUp(nIndex, isFine);
            while (IsEndTask(task) == false)
            {
                int nRtn = IsMoveInterLockAlignUp(nIndex);
                if (nRtn != 0)
                {
                    return -1;
                }

                Thread.Sleep(0);
            }
            return task.Result;
        }
        private Task<int> MovePositionAsyncAlignUp(int nIndex = 0, bool isFine = false)
        {
            return Task.Run(() =>
            {
                OnMovePositionAlignUp(nIndex, isFine);
                return 0;
            });
        }
        private int OnMovePositionAlignUp(int nIndex = 0, bool isFine = false)
        {
            int nRet = 0;
            // nIndex 처리 (0-based와 1-based 모두 지원)
            //  - 1~8 : 그대로 사용 (Place_Index1 ~ Place_Index8)
            //  - 0~7 : +1 보정하여 1~8 매핑
            int teachingIdx;
            if (nIndex >= 1 && nIndex <= 8)
                teachingIdx = nIndex;
            else if (nIndex >= 0 && nIndex < 8)
                teachingIdx = nIndex + 1; // 0-based 입력으로 판단
            else
            {
                Log.Write(UnitName, $"[OnMovePositionAlignUp_Index] Invalid index {nIndex}. Range 0~7 or 1~8");
                return -1;
            }

            string tpName = $"AlignZ_Index{teachingIdx}_Up";
            var tpObj = IndexLoadAlignerConfig.GetTeachingPosition(tpName);
            if (tpObj == null)
            {
                Log.Write(UnitName, $"[OnMovePositionAlignUp_Index] Teaching not found: {tpName}");
                return -1;
            }

            double dZPos = GetTP(tpName, AxisNames.IndexZ);
            nRet = MoveAxisPositionOne(AxisIndexZ, dZPos);
            if (nRet != 0)
            {
                Log.Write(UnitName, $"[OnMovePositionPlace_Index] PlaceZ move failed tp={tpName} pos={dZPos}");
                return -1;
            }

            //nRet = MoveTeachingPositionOnce((int)IndexLoadAlignerConfig.TeachingPositionName.AlignZ_Index1_Up, isFine);
            //if(nRet != 0)
            //{
            //    Log.Write(UnitName, $"[OnMovePositionAlignUp_Index] MoveTeachingPositionOnce failed: {tpName}");
            //    return -1;
            //}

            //return MoveTeachingPositionOnce((int)IndexLoadAlignerConfig.TeachingPositionName.AlignZ_Index1_Up, isFine);
            return nRet;
        }
        private int IsMoveInterLockAlignUp(int nIndex = 0)
        {
            int nRet = 0;

            if (Rotary != null && Rotary.IsAnyAxisMoving())
            {
                AxisIndexZ.EmgStop();
                AxisAlignT.EmgStop();
                AlarmPost((int)AlarmKeys.eRotaryNotSafe);
                return -1;
            }
            return nRet;
        }
        public Task<int> MovePositionAsyncSafeAlignUp(int nIndex = 0, bool isFine = false, CancellationToken ct = default(CancellationToken))
        {
            return Task.Run(() =>
            {
                // Task로 돌리고 별도 인터락/취소 감시
                var coreTask = Task.Run(() => OnMovePositionAlignUp(nIndex, isFine), ct);

                while (!IsEndTask(coreTask))
                {
                    if (ct.IsCancellationRequested)
                    {
                        try
                        {
                            AxisIndexZ?.EmgStop();
                            AxisAlignT?.EmgStop();
                        }
                        catch { }
                        return -999; // 취소 코드
                    }

                    int nRtn = IsMoveInterLockSafetyZ();
                    if (nRtn != 0)
                    {
                        return -1;
                    }

                    Thread.Sleep(5); // 0→5ms로 약간 여유 (CPU 점유 감소)
                }

                return coreTask.Result;
            }, ct);
        }


        // === AlignT_Foward ===
        public int MovePositionAlignTForward(bool isFine = false)
        {
            Task<int> task = MovePositionAsyncAlignTForward(isFine);
            while (IsEndTask(task) == false)
            {
                int nRtn = IsMoveInterLockAlignTForward();
                if (nRtn != 0)
                {
                    return -1;
                }
                Thread.Sleep(0);
            }
            return task.Result;
        }
        private Task<int> MovePositionAsyncAlignTForward(bool isFine = false)
        {
            return Task.Run(() =>
            {
                OnMovePositionAlignTForward(isFine);
                return 0;
            });
        }
        private int OnMovePositionAlignTForward(bool isFine = false)
        {
            return MoveTeachingPositionOnce((int)IndexLoadAlignerConfig.TeachingPositionName.AlignT_Foward, isFine);
        }
        private int IsMoveInterLockAlignTForward()
        {
            int nRet = 0;
            if (Rotary != null && Rotary.IsAnyAxisMoving())
            {
                AxisIndexZ?.EmgStop();
                AxisAlignT?.EmgStop();
                AlarmPost((int)AlarmKeys.eRotaryNotSafe);
                return -1;
            }
            return nRet;
        }
        public Task<int> MovePositionAsyncSafeAlignTForward(bool isFine = false, CancellationToken ct = default(CancellationToken))
        {
            return Task.Run(() =>
            {
                var coreTask = Task.Run(() => OnMovePositionAlignTForward(isFine), ct);
                while (!IsEndTask(coreTask))
                {
                    if (ct.IsCancellationRequested)
                    {
                        try { AxisIndexZ?.EmgStop(); AxisAlignT?.EmgStop(); } catch { }
                        return -999; // 취소 코드
                    }
                    int nRtn = IsMoveInterLockAlignTForward();
                    if (nRtn != 0)
                    {
                        return -1;
                    }
                    Thread.Sleep(5);
                }
                return coreTask.Result;
            }, ct);
        }


        // === AlignT_Backward ===
        public int MovePositionAlignTBackward(bool isFine = false)
        {
            Task<int> task = MovePositionAsyncAlignTBackward(isFine);
            while (IsEndTask(task) == false)
            {
                int nRtn = IsMoveInterLockAlignTBackward();
                if (nRtn != 0)
                {
                    return -1;
                }
                Thread.Sleep(0);
            }
            return task.Result;
        }
        private Task<int> MovePositionAsyncAlignTBackward(bool isFine = false)
        {
            return Task.Run(() =>
            {
                OnMovePositionAlignTBackward(isFine);
                return 0;
            });
        }
        private int OnMovePositionAlignTBackward(bool isFine = false)
        {
            return MoveTeachingPositionOnce((int)IndexLoadAlignerConfig.TeachingPositionName.AlignT_Backward, isFine);
        }
        private int IsMoveInterLockAlignTBackward()
        {
            int nRet = 0;
            if (Rotary != null && Rotary.IsAnyAxisMoving())
            {
                AxisIndexZ?.EmgStop();
                AxisAlignT?.EmgStop();
                AlarmPost((int)AlarmKeys.eRotaryNotSafe);
                return -1;
            }
            return nRet;
        }
        public Task<int> MovePositionAsyncSafeAlignTBackward(bool isFine = false, CancellationToken ct = default(CancellationToken))
        {
            return Task.Run(() =>
            {
                var coreTask = Task.Run(() => OnMovePositionAlignTBackward(isFine), ct);
                while (!IsEndTask(coreTask))
                {
                    if (ct.IsCancellationRequested)
                    {
                        try { AxisIndexZ?.EmgStop(); AxisAlignT?.EmgStop(); } catch { }
                        return -999; // 취소 코드
                    }
                    int nRtn = IsMoveInterLockAlignTBackward();
                    if (nRtn != 0)
                    {
                        return -1;
                    }
                    Thread.Sleep(5);
                }
                return coreTask.Result;
            }, ct);
        }


        // === AlignT_Ready ===
        public int MovePositionAlignTReady(bool isFine = false)
        {
            string readyName = IndexLoadAlignerConfig.TeachingPositionName.AlignT_Ready.ToString();
            if (InPosTeaching(readyName))
            {
                return 0;
            }
                
            Task<int> task = MovePositionAsyncAlignTReady(isFine);
            while (IsEndTask(task) == false)
            {
                int nRtn = IsMoveInterLockAlignTReady();
                if (nRtn != 0)
                {
                    return -1;
                }
                Thread.Sleep(0);
            }
            return task.Result;
        }
        private Task<int> MovePositionAsyncAlignTReady(bool isFine = false)
        {
            return Task.Run(() =>
            {
                OnMovePositionAlignTReady(isFine);
                return 0;
            });
        }
        private int OnMovePositionAlignTReady(bool isFine = false)
        {
            return MoveTeachingPositionOnce((int)IndexLoadAlignerConfig.TeachingPositionName.AlignT_Ready, isFine);
        }
        private int IsMoveInterLockAlignTReady()
        {
            int nRet = 0;
            if (Rotary != null && Rotary.IsAnyAxisMoving())
            {
                AxisIndexZ?.EmgStop();
                AxisAlignT?.EmgStop();
                AlarmPost((int)AlarmKeys.eRotaryNotSafe);
                return -1;
            }
            return nRet;
        }
        public Task<int> MovePositionAsyncSafeAlignTReady(bool isFine = false, CancellationToken ct = default(CancellationToken))
        {
            return Task.Run(() =>
            {
                var coreTask = Task.Run(() => OnMovePositionAlignTReady(isFine), ct);
                while (!IsEndTask(coreTask))
                {
                    if (ct.IsCancellationRequested)
                    {
                        try { AxisIndexZ?.EmgStop(); AxisAlignT?.EmgStop(); } catch { }
                        return -999; // 취소 코드
                    }
                    int nRtn = IsMoveInterLockAlignTReady();
                    if (nRtn != 0)
                    {
                        return -1;
                    }
                    Thread.Sleep(5);
                }
                return coreTask.Result;
            }, ct);
        }

        public bool IsAlignZSafetyPos(double fallbackTolerance = 0.01,
                                      bool useAxisInposTolerance = true,
                                      bool treatMissingAsSafe = true)
        {
            if (AxisIndexZ == null)
                return treatMissingAsSafe;

            var cfg = IndexLoadAlignerConfig;
            if (cfg == null) return false;

            // 우선순위: SafetyPos → SafetyZone
            string[] candidates =
            {
                "SafetyPos",
                IndexLoadAlignerConfig.TeachingPositionName.SafetyZone.ToString()
            };

            string foundName = null;
            foreach (var name in candidates)
            {
                var tp = cfg.GetTeachingPosition(name);
                if (tp == null) continue;

                // IndexZ 좌표가 실제 존재하는 티칭만 사용
                if (tp.AxisPositions != null &&
                    tp.AxisPositions.Keys.Any(k => string.Equals(k, AxisNames.IndexZ, System.StringComparison.OrdinalIgnoreCase)))
                {
                    foundName = name;
                    break;
                }
            }

            if (foundName == null)
                return treatMissingAsSafe;

            var tpFound = cfg.GetTeachingPosition(foundName);
            if (tpFound == null) return false;

            double target = tpFound.GetAxisPosition(AxisNames.IndexZ, 0.0);
            double cur = AxisIndexZ.GetPosition();
            double tol = useAxisInposTolerance
                ? (AxisIndexZ.Config?.InposTolerance ?? fallbackTolerance)
                : fallbackTolerance;

            return System.Math.Abs(cur - target) <= tol;
        }


        public bool InPos(MotionAxis ax, double target) => ax == null || ax.InPosition(target);
        public double GetTP(string tpName, string axisName)
        {
            var tp = Config.GetTeachingPosition(tpName);
            if (tp != null && tp.AxisPositions != null && tp.AxisPositions.TryGetValue(axisName, out var v)) return v;
            return 0.0;
        }
        #endregion

        #region Teaching
        public void TeachCurrentPosition(string positionName, string description = null)
        {
            var axisPositions = new Dictionary<string, double>();
            foreach (var axisPair in Axes)
                axisPositions[axisPair.Key] = axisPair.Value.GetPosition();
            var tp = new TeachingPosition(positionName, axisPositions, description);
            Config.SetTeachingPosition(tp);
        }
        public int MoveToTeachingPosition(string positionName, double vel = 5, double acc = 10, double dec = 10, double jerk = 50)
        {
            var tp = Config.GetTeachingPosition(positionName);
            if (tp == null) return -1;
            int result = 0;
            foreach (var axisKey in tp.AxisPositions.Keys)
            {
                if (Axes.TryGetValue(axisKey, out var axis))
                {
                    double pos = tp.AxisPositions[axisKey];
                    int r = axis.MoveAbs(pos, vel, acc, dec, jerk);
                    if (r != 0) result = r;
                }
            }
            return result;
        }
        public bool InPosTeaching(string positionName)
        {
            var tp = Config.GetTeachingPosition(positionName);
            if (tp == null) return false;
            foreach (var kv in tp.AxisPositions)
                if (!Axes.TryGetValue(kv.Key, out var axis) || !InPos(axis, kv.Value)) return false;
            return true;
        }
        #endregion

        #region IO Placeholders
        public bool ReadInput(string name) => false; // No IO defined yet
        public bool WriteOutput(string name, bool on) => false; // No IO defined yet
        #endregion

        #region Lifecycle
        public override int OnRun()
        {
            int ret = 0;

            if (this.Status == UnitRunStatus.Stop ||
                this.Status == UnitRunStatus.CycleStop)
            {
                this.State = ProcessState.Stop;
                ret = 1;
            }
            else
            {
                switch (State)
                {
                    case ProcessState.Manual:
                        ret = OnRunManual();
                        break;
                    case ProcessState.Ready:
                        ret = OnRunReady();
                        break;
                    case ProcessState.Work:
                        ret = OnRunWork();
                        break;
                    case ProcessState.Complete:
                        ret = OnRunComplete();
                        break;
                    default:
                        if (ManualState == ProcessState.Manual)
                        {
                            this.State = ProcessState.Manual;
                        }
                        else
                        {
                            this.State = ProcessState.Ready;
                        }
                        break;
                }
            }

            if (ret != 0)
            {
                this.State = ProcessState.Stop;
                this.OnStop();
            }

            return ret;
        }

        public ProcessState ManualState { get; set; }
        public int StepManual = 0;
        public int ManualSocketIndex = 1;
        private int OnRunManual()
        {
            int ret = 0;

            switch (StepManual)
            {
                case 0:
                    break;
                case 1:
                    ret = AlignSocketOnce(ManualSocketIndex);
                    if (ret != 0)
                    {
                        OnStop();
                    }
                    else
                    {
                        StepManual = 0;
                    }
                    break;
            }

            return ret;
        }
        public override int OnStop()
        {
            int ret = 0;

            this.State = ProcessState.Stop;
            base.OnStop();
            return ret;
        }
        protected override int OnRunReady()
        {
            int ret = 0;

            State = ProcessState.Work;
            return 0;
        }
        protected override int OnRunWork()
        {
            int ret = 0;

            State = ProcessState.Complete;
            return 0;
        }
        protected override int OnRunComplete()
        {
            int ret = 0;

            State = ProcessState.None;
            return 0;
        }
        #endregion

        #region Seq 단위 동작 함수

        
        /// <summary>
        /// Rotary(인덱스)가 정지 상태인지 즉시 확인.
        /// - 정지면 0, 이동 중이면 -1 반환(알람 포스트).
        /// - 대기는 수행하지 않는다(메인이 반복 호출/대기).
        /// </summary>
        public int IsRotaryIdle()
        {
            if (Rotary != null && Rotary.IsAnyAxisMoving())
            {
                AxisIndexZ.EmgStop();
                AxisAlignT.EmgStop();
                AlarmPost((int)AlarmKeys.eRotaryNotSafe);
                return -1;
            }
            return 0;
        }

        /// <summary>
        /// 소켓 1개에 대한 정렬 묶음
        /// 순서: Z Up(해당 소켓) -> T Forward -> T Backward -> T Ready
        /// 모든 축 이동은 안전 Async 버전 사용(내부 폴링).
        /// </summary>
        public int AlignSocketOnce(int socketIndex, bool isFine = false)
        {
            int bRtn = 0;
            // 1) Rotary Idle Check
            bRtn = IsRotaryIdle();
            if (bRtn != 0) return -1;

            // 2) T Ready
            bRtn = MovePositionAlignTReady(isFine);
            if (bRtn != 0) return -1;

            // 3) Z Up
            bRtn = MovePositionAlignUp(socketIndex, isFine);
            if(bRtn != 0) 
                return -1;

            // 4) T Forward
            bRtn = MovePositionAlignTForward(isFine);
            if (bRtn != 0) 
                return -1;

            // 5) T Backward
            bRtn = MovePositionAlignTBackward(isFine);
            if (bRtn != 0) 
                return -1;

            // 6) T Ready
            bRtn = MovePositionAlignTReady(isFine);
            if (bRtn != 0) 
                return -1;
            
            return bRtn;
        }


        public int VisionAlign()
        {
            int nRet = -1;
            /* TODO */
            return nRet;
        }

        public int M_Align()
        {
            int nRet = -1;
            /* TODO */
            return nRet;
        }
        #endregion
    }
}