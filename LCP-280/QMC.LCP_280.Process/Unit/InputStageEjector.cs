using QMC.Common;
using QMC.Common.Alarm;
using QMC.Common.Component;
using QMC.Common.Motion;
using QMC.Common.Motions;
using QMC.Common.Unit;
using QMC.LCP_280.Process.Component;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms.DataVisualization.Charting;
using static QMC.LCP_280.Process.Equipment;

namespace QMC.LCP_280.Process.Unit
{
    /// <summary>
    /// InputStageEjector Unit
    ///  - Ejector Z / Pin Z 두 축 Teaching + Offset 이동
    ///  - OutputStage / InputStage 구조와 통일된 Region 구성
    ///  - DryRun 옵션 (현재는 단순 플래그만 유지, 향후 IO 시뮬 추가 가능)
    /// </summary>
    public class InputStageEjector : BaseUnit<InputStageEjectorConfig>
    {
        public enum AlarmKeys
        {
            eInputStageAxesMoving = 4301,

        }
        #region InitAlarm
        protected override void InitAlarm()
        {
            base.InitAlarm();
            AlarmInfo alarm = new AlarmInfo();
            alarm.Code = (int)AlarmKeys.eInputStageAxesMoving;
            alarm.Title = "InputStage Not Sfarety Pos.";
            alarm.Cause = "InputStage가 안전 위치가 아닙니다.\n 포지션 확인 후 다시 시작 하십시요.";
            alarm.Source = this.UnitName;
            alarm.Grade = AlarmInfo.AlarmType.Warning.ToString();
            m_dicAlarms.Add(alarm.Code, alarm);

        }

        #region Config 
        public InputStageEjectorConfig InputStageEjectorConfig => Config;
        #endregion

        #region Unit
        InputStage InputStage { get; set; }
        InputDieTransfer InputDieTransfer { get; set; }
        #endregion

        #region Axes
        private MotionAxis _axPinZ, _axEjectorZ;
        public MotionAxis AxisPinZ => _axPinZ;
        public MotionAxis AxisEjectorZ => _axEjectorZ;
        #endregion

        #region ctor / Initialization
        public InputStageEjector(InputStageEjectorConfig config = null) : base(config ?? new InputStageEjectorConfig())
        {
            AddComponents();
        }

        public override void AddComponents()
        {
            Config.LoadAndBindAxes(Equipment.Instance.AxisManager);
            Config.InitializeDefaultTeachingPositions();
            BindAxes();
        }
        protected override void OnBindUnit()
        {
            base.OnBindUnit();
            InputStage = Equipment.Instance.GetUnit(UnitKeys.InputStage) as InputStage;
            InputDieTransfer = Equipment.Instance.GetUnit(UnitKeys.InputDieTransfer) as InputDieTransfer;
        }
        #endregion

        #region Axis Binding / Helpers
        private void BindAxes()
        {
            var mgr = Equipment.Instance?.AxisManager;
            if (mgr == null)
            {
                Log.Write("InputStageEjector", "[BindAxes] AxisManager null");
                return;
            }

            const string unitName = "Unit"; // Equipment에서 축 등록 시 사용한 유닛명과 동일해야 함
            BindAxis(mgr, unitName, AxisNames.EjectPinZ, ref _axPinZ);
            BindAxis(mgr, unitName, AxisNames.EjectorZ, ref _axEjectorZ);
        }
        #endregion


        // ================== Generic Single Axis Move (Safety Interlock 동일 구조) ==================
        /// <summary>
        /// 단일 축 이동 (Safety 인터락 포함). 이동 완료까지 블록.
        /// </summary>
        public int MoveAxisPositionOne(MotionAxis axis, double target, bool isFine = false)
        {
            if (axis == null) return -1;

            if (CheckMoveSafety(axis) != 0)
            {
                return -1;
            }

            Task<int> task = MoveAxisPositionOneAsync(axis, target, isFine);
            while (IsEndTask(task) == false)
            {
                // 동일 Safety Interlock
                // 사전 Interlock (다른 관련 Unit 축 동작 중이면 시작하지 않음)
                if (InputStage != null && InputStage.IsAnyAxisMoving())
                {
                    AxisEjectorZ.EmgStop();
                    AxisPinZ.EmgStop();
                    PostAlarm((int)AlarmKeys.eInputStageAxesMoving);
                    return -1;
                }

                Thread.Sleep(1);
            }
            return task.Result;
        }

        public override int CheckMoveSafety(MotionAxis ax)
        {
            try
            {
                //if (/*다른 유닛 축 이동중*/) return (int)AlarmKeys.xxx;
                // 1) Ejector / PinZ Safety 검사 (우선순위 높음)
                if (InputStage != null && InputStage.IsAnyAxisMoving())
                {
                    AxisEjectorZ.EmgStop();
                    AxisPinZ.EmgStop();
                    PostAlarm((int)AlarmKeys.eInputStageAxesMoving);
                    return -1;
                }

                if(InputStage.CheckMoveSafety(InputStage.AxisX) != 0 ||
                   InputStage.CheckMoveSafety(InputStage.AxisY) != 0    )
                {
                    AxisEjectorZ.EmgStop();
                    AxisPinZ.EmgStop();
                    PostAlarm((int)AlarmKeys.eInputStageAxesMoving);
                    return -1;
                }

                // 추가로 "다른 유닛 축 이동중" 등을 넣고 싶다면 여기서 검사 후 알람 코드 반환
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                // 예외 발생 시 보수적으로 이동 중단하도록 임의 알람 
                AxisEjectorZ.EmgStop();
                AxisPinZ.EmgStop();
                PostAlarm((int)AlarmKeys.eInputStageAxesMoving);
                return -1;
            }

            return 0; // 0 = OK
        }
        /// //////////////////////////////////////////////////////////////////////////////////////////////
        // UI, sequence 용 Move 함수

        //EjectBlockUp
        public int MovePositionEjectBlockUp(bool isFine = false)
        {
            Task<int> task = MovePositionAsyncEjectBlockUp(isFine);
            while (IsEndTask(task) == false)
            {
                // Check Interlock.!!! 구문 넣을것.!!!
                int nRtn = 0;
                nRtn = IsMoveInterLockEjectBlockUp();
                if (nRtn != 0)
                {
                    return -1;
                }

                Thread.Sleep(1);
            }
            return task.Result;
           
        }
        private Task<int> MovePositionAsyncEjectBlockUp(bool isFine = false)
        {
            return Task.Run(() =>
            {
                OnMovePositionEjectBlockUp(isFine);
                return 0;
            });
        }
        private int OnMovePositionEjectBlockUp(bool isFine = false)
        {
            return MoveTeachingPositionOnce((int)InputStageEjectorConfig.TeachingPositionName.EjectBlockUp, isFine);
        }
        private int IsMoveInterLockEjectBlockUp()
        {
            int nRet = 0;
            // Check Interlock.!!! 구문 넣을것.!!!
            if (InputStage != null && !InputStage.IsAnyAxisMoving())
            {
                AxisEjectorZ.EmgStop();
                AxisPinZ.EmgStop();

                PostAlarm((int)AlarmKeys.eInputStageAxesMoving);
                return -1;
            }

            if (InputStage.CheckMoveSafety(InputStage.AxisX) != 0 ||
                InputStage.CheckMoveSafety(InputStage.AxisY) != 0  )
            {
                AxisEjectorZ.EmgStop();
                AxisPinZ.EmgStop();

                PostAlarm((int)AlarmKeys.eInputStageAxesMoving);
                return -1;
            }

            return nRet;
        }
        public Task<int> MovePositionAsyncSafeEjectBlockUp(bool isFine = false, CancellationToken ct = default(CancellationToken))
        {
            return Task.Run(() =>
            {
                // OnMovePickUpPosition을 Task로 돌리고 별도 인터락/취소 감시
                var coreTask = Task.Run(() => OnMovePositionEjectBlockUp(isFine), ct);

                while (!IsEndTask(coreTask))
                {
                    if (ct.IsCancellationRequested)
                    {
                        try
                        {
                            AxisEjectorZ?.EmgStop();
                            AxisPinZ?.EmgStop();
                        }
                        catch { }
                        return -999; // 취소 코드
                    }

                    int nRtn = IsMoveInterLockEjectBlockUp();
                    if (nRtn != 0)
                    {
                        return -1;
                    }

                    Thread.Sleep(5); // 0→5ms로 약간 여유 (CPU 점유 감소)
                }

                return coreTask.Result;
            },
            ct);
        }


        //EjectBlockReady
        public int MovePositionEjectBlockReady(bool isFine = false)
        {
            Task<int> task = MovePositionAsyncEjectBlockReady(isFine);
            while (IsEndTask(task) == false)
            {
                int nRtn = 0;
                nRtn = IsMoveInterLockEjectBlockReady();
                if (nRtn != 0)
                {
                    return -1;
                }

                Thread.Sleep(1);
            }
            return task.Result;
        }
        private Task<int> MovePositionAsyncEjectBlockReady(bool isFine = false)
        {
            return Task.Run(() =>
            {
                OnMovePositionEjectBlockReady(isFine);
                return 0;
            });
        }
        private int OnMovePositionEjectBlockReady(bool isFine = false)
        {
            return MoveTeachingPositionOnce((int)InputStageEjectorConfig.TeachingPositionName.EjectBlockReady, isFine);
        }
        private int IsMoveInterLockEjectBlockReady()
        {
            int nRet = 0;
            // Check Interlock.!!! 구문 넣을것.!!!
            if (InputStage != null && !InputStage.IsAnyAxisMoving())
            {
                AxisEjectorZ.EmgStop();
                AxisPinZ.EmgStop();
                PostAlarm((int)AlarmKeys.eInputStageAxesMoving);
                return -1;
            }
            return nRet;
        }
        public Task<int> MovePositionAsyncSafeEjectBlockReady(bool isFine = false, CancellationToken ct = default(CancellationToken))
        {
            return Task.Run(() =>
            {
                // OnMovePickUpPosition을 Task로 돌리고 별도 인터락/취소 감시
                var coreTask = Task.Run(() => OnMovePositionEjectBlockReady(isFine), ct);

                while (!IsEndTask(coreTask))
                {
                    if (ct.IsCancellationRequested)
                    {
                        try
                        {
                            AxisEjectorZ?.EmgStop();
                            AxisPinZ?.EmgStop();
                        }
                        catch { }
                        return -999; // 취소 코드
                    }

                    int nRtn = IsMoveInterLockEjectBlockReady();
                    if (nRtn != 0)
                    {
                        return -1;
                    }

                    Thread.Sleep(5); // 0→5ms로 약간 여유 (CPU 점유 감소)
                }

                return coreTask.Result;
            },
            ct);
        }


        //EjectBlocksafety
        public int MovePositionEjectBlockSafety(bool isFine = false)
        {
            Task<int> task = MovePositionAsyncEjectBlockSafety(isFine);
            while (IsEndTask(task) == false)
            {
                int nRtn = 0;
                nRtn = IsMoveInterLockEjectBlockSafety();
                if (nRtn != 0)
                {
                    return -1;
                }
                Thread.Sleep(1);
            }
            return task.Result;
        }
        public Task<int> MovePositionAsyncEjectBlockSafety(bool isFine = false)
        {
            return Task.Run(() =>
            {
                OnMovePositionEjectBlockSafety(isFine);
                return 0;
            });
        }
        private int OnMovePositionEjectBlockSafety(bool isFine = false)
        {
            return MoveTeachingPositionOnce((int)InputStageEjectorConfig.TeachingPositionName.EjectBlockSafety, isFine);
        }
        private int IsMoveInterLockEjectBlockSafety()
        {
            int nRet = 0;
            // Check Interlock.!!! 구문 넣을것.!!!
            if (InputStage != null && InputStage.IsAnyAxisMoving())
            {
                AxisEjectorZ.EmgStop();
                AxisPinZ.EmgStop();
                PostAlarm((int)AlarmKeys.eInputStageAxesMoving);
                return -1;
            }
            return nRet;
        }
        public Task<int> MovePositionAsyncSafeEjectBlockSafety(bool isFine = false, CancellationToken ct = default(CancellationToken))
        {
            return Task.Run(() =>
            {
                // OnMovePickUpPosition을 Task로 돌리고 별도 인터락/취소 감시
                var coreTask = Task.Run(() => OnMovePositionEjectBlockSafety(isFine), ct);

                while (!IsEndTask(coreTask))
                {
                    if (ct.IsCancellationRequested)
                    {
                        try
                        {
                            AxisEjectorZ?.EmgStop();
                            AxisPinZ?.EmgStop();
                        }
                        catch { }
                        return -999; // 취소 코드
                    }

                    int nRtn = IsMoveInterLockEjectBlockSafety();
                    if (nRtn != 0)
                    {
                        return -1;
                    }

                    Thread.Sleep(5); // 0→5ms로 약간 여유 (CPU 점유 감소)
                }

                return coreTask.Result;
            },
            ct);
        }


        //EjectBlockReady
        public int MovePositionEjectPinReady(bool isFine = false)
        {
            Task<int> task = MovePositionAsyncEjectPinReady(isFine);
            while (IsEndTask(task) == false)
            {
                int nRtn = 0;
                nRtn = IsMoveInterLockEjectPinReady();
                if (nRtn != 0)
                {
                    return -1;
                }

                Thread.Sleep(1);
            }
            return task.Result;
        }
        private Task<int> MovePositionAsyncEjectPinReady(bool isFine = false)
        {
            return Task.Run(() =>
            {
                OnMovePositionEjectPinReady(isFine);
                return 0;
            });
        }
        private int OnMovePositionEjectPinReady(bool isFine = false)
        {
            return MoveTeachingPositionOnce((int)InputStageEjectorConfig.TeachingPositionName.EjectPinReady, isFine);
        }
        private int IsMoveInterLockEjectPinReady()
        {
            int nRet = 0;
            // Check Interlock.!!! 구문 넣을것.!!!
            if (InputStage != null && !InputStage.IsAnyAxisMoving())
            {
                AxisEjectorZ.EmgStop();
                AxisPinZ.EmgStop();
                PostAlarm((int)AlarmKeys.eInputStageAxesMoving);
                return -1;
            }
            return nRet;
        }
        public Task<int> MovePositionAsyncSafeEjectPinReady(bool isFine = false, CancellationToken ct = default(CancellationToken))
        {
            return Task.Run(() =>
            {
                // OnMovePickUpPosition을 Task로 돌리고 별도 인터락/취소 감시
                var coreTask = Task.Run(() => OnMovePositionEjectPinReady(isFine), ct);

                while (!IsEndTask(coreTask))
                {
                    if (ct.IsCancellationRequested)
                    {
                        try
                        {
                            AxisEjectorZ?.EmgStop();
                            AxisPinZ?.EmgStop();
                        }
                        catch { }
                        return -999; // 취소 코드
                    }

                    int nRtn = IsMoveInterLockEjectPinReady();
                    if (nRtn != 0)
                    {
                        return -1;
                    }

                    Thread.Sleep(5); // 0→5ms로 약간 여유 (CPU 점유 감소)
                }

                return coreTask.Result;
            },
            ct);
        }




        public bool InPos(MotionAxis ax, double target) => ax == null || ax.InPosition(target);
        public double GetTP(TeachingPosition tp, string axisKey) => (tp == null || string.IsNullOrEmpty(axisKey)) ? 0.0 : (tp.AxisPositions.TryGetValue(axisKey, out var v) ? v : 0.0);
        public double GetTP(TeachingPosition tp, MotionAxis axis) => axis == null ? 0.0 : GetTP(tp, axis.Name);
        public double GetTP(string tpName, string axisName)
        {
            var tp = Config.GetTeachingPosition(tpName);
            if (tp != null && tp.AxisPositions != null && tp.AxisPositions.TryGetValue(axisName, out var v)) return v;
            return 0.0;
        }
        #endregion

        #region Teaching
        public int MoveToTeachingPosition(string positionName, double vel = 0, double acc = 0, double dec = 0, double jerk = 0)
        {
            var tp = Config.GetTeachingPosition(positionName);
            if (tp == null) return -1;
            var (z, pz) = Config.GetPositionWithOffset(positionName);
            int rc = 0;
            if (_axEjectorZ != null)
                rc |= _axEjectorZ.MoveAbs(z, vel > 0 ? vel : _axEjectorZ.Config.MaxVelocity, acc > 0 ? acc : _axEjectorZ.Config.RunAcc, dec > 0 ? dec : _axEjectorZ.Config.RunDec, jerk > 0 ? jerk : _axEjectorZ.Config.AccJerkPercent);
            if (_axPinZ != null)
                rc |= _axPinZ.MoveAbs(pz, vel > 0 ? vel : _axPinZ.Config.MaxVelocity, acc > 0 ? acc : _axPinZ.Config.RunAcc, dec > 0 ? dec : _axPinZ.Config.RunDec, jerk > 0 ? jerk : _axPinZ.Config.AccJerkPercent);
            return rc;
        }
        public int MoveToTeachingPosition(InputStageEjectorConfig.TeachingPositionName name, double vel = 0, double acc = 0, double dec = 0, double jerk = 0) => MoveToTeachingPosition(name.ToString(), vel, acc, dec, jerk);
        public bool InPosTeaching(string positionName)
        {
            var (z, pz) = Config.GetPositionWithOffset(positionName);
            return InPos(_axEjectorZ, z) && InPos(_axPinZ, pz);
        }
        #endregion

        public bool InPosTeaching(InputStageEjectorConfig.TeachingPositionName name) => InPosTeaching(name.ToString());
        /// <summary>
        /// EjectorZ 축이 Safety Teaching 위치(또는 허용 오차 범위)에 있는지 확인.
        /// 우선순위: EjectBlockSafety → EjectBlockUp → EjectBlockReady
        /// </summary>
        /// <param name="fallbackTolerance">축 InposTolerance를 사용할 수 없을 때 기본 허용오차</param>
        /// <param name="useAxisInposTolerance">축 Config.InposTolerance 사용 여부</param>
        /// <param name="treatMissingAsSafe">Teaching 또는 축이 없을 경우 true 로 간주할지 여부</param>
        /// <param name="allowAbove">안전 위치보다 위(더 +방향)도 허용할지 여부 (일반적으로 Z축 위쪽이면 안전)</param>
        public bool IsEjectorZSafetyPos(double fallbackTolerance = 0.01,
                                         bool useAxisInposTolerance = true,
                                         bool treatMissingAsSafe = true,
                                         bool allowAbove = false)
        {
            if (_axEjectorZ == null)
                return treatMissingAsSafe;

            //var cfg = InputStageEjectorConfig;
            //if (cfg == null) return false;
            //string[] candidates =
            //{
            //    "EjectBlockSafety",
            //    "EjectBlockUp",
            //    "EjectBlockReady"
            //};
            //string found = candidates.FirstOrDefault(n => cfg.GetTeachingPosition(n) != null);
            //if (found == null)
            //    return treatMissingAsSafe;
            //var (ejectorTarget, _) = cfg.GetPositionWithOffset(found);

            double dZPos = GetTP(InputStageEjectorConfig.TeachingPositionName.EjectBlockSafety.ToString(),
                        AxisNames.EjectorZ);

            double cur = _axEjectorZ.GetPosition();
            double tol = useAxisInposTolerance
                ? (_axEjectorZ.Config?.InposTolerance ?? fallbackTolerance)
                : fallbackTolerance;

            if (allowAbove)
                return cur >= (dZPos - tol);

            return System.Math.Abs(cur - dZPos) <= tol;
        }

        /// <summary>
        /// PinZ 축이 Safety Teaching 위치(또는 허용 오차 범위)에 있는지 확인.
        /// 우선순위: EjectPinReady → EjectPinChange → EjectPinOffset
        /// </summary>
        /// <param name="fallbackTolerance">축 InposTolerance를 사용할 수 없을 때 기본 허용오차</param>
        /// <param name="useAxisInposTolerance">축 Config.InposTolerance 사용 여부</param>
        /// <param name="treatMissingAsSafe">Teaching 또는 축이 없을 경우 true 로 간주할지 여부</param>
        /// <param name="allowAbove">안전 위치보다 위(더 +방향)도 허용할지 여부</param>
        public bool IsPinZSafetyPos(double fallbackTolerance = 0.01,
                                     bool useAxisInposTolerance = true,
                                     bool treatMissingAsSafe = true,
                                     bool allowAbove = false)
        {
            if (_axPinZ == null)
                return treatMissingAsSafe;

            var cfg = InputStageEjectorConfig;
            if (cfg?.TeachingPositions == null || cfg.TeachingPositions.Count == 0)
                return treatMissingAsSafe;

            // 우선순위 (문서 주석 기준)
            string[] candidates =
            {
                InputStageEjectorConfig.TeachingPositionName.EjectPinReady.ToString(),
                InputStageEjectorConfig.TeachingPositionName.EjectPinChange.ToString(),
                InputStageEjectorConfig.TeachingPositionName.EjectPinOffset.ToString()
            };

            var targetList = new List<double>();

            foreach (var name in candidates)
            {
                var tp = cfg.GetTeachingPosition(name);
                if (tp?.AxisPositions == null) continue;

                if (tp.AxisPositions.TryGetValue(AxisNames.EjectPinZ, out double val))
                {
                    // 실제로 0.0 위치가 유효한 Teaching 일 수도 있으므로 그대로 사용
                    targetList.Add(val);
                }
            }

            if (targetList.Count == 0)
                return treatMissingAsSafe; // Teaching 에 PinZ 가 하나도 정의 안된 경우 정책상 Safe 처리

            double cur = _axPinZ.GetPosition();
            double tol = useAxisInposTolerance
                         ? (_axPinZ.Config?.InposTolerance ?? fallbackTolerance)
                         : fallbackTolerance;

            if (allowAbove)
            {
                // 가장 낮은 위치(위험 가능성 최대) 이상이면 Safe 로 본다 (양(+)방향이 Up 이라고 가정)
                double minTarget = targetList.Min();
                return cur >= (minTarget - tol);
            }

            // 어떤 후보라도 허용오차 내면 Safe
            return targetList.Any(t => Math.Abs(cur - t) <= tol);

            //if (_axPinZ == null)
            //    return treatMissingAsSafe;

            //double dSafetyZPos = 0.0;
            //double dOffsetZPos = GetTP(InputStageEjectorConfig.TeachingPositionName.EjectPinOffset.ToString(),
            //            AxisNames.EjectPinZ);
            //double dReadyZPos = GetTP(InputStageEjectorConfig.TeachingPositionName.EjectPinReady.ToString(),
            //            AxisNames.EjectPinZ);

            //double cur = _axPinZ.GetPosition();
            //double tol = useAxisInposTolerance
            //    ? (_axPinZ.Config?.InposTolerance ?? fallbackTolerance)
            //    : fallbackTolerance;
            //if (allowAbove)
            //    return cur >= (dReadyZPos - tol);

            //return System.Math.Abs(cur - dReadyZPos) <= tol;
        }
        /// <summary>
        /// 두 축(EjectorZ & PinZ) 모두 Safety 판단
        /// </summary>
        public bool IsAllSafety() => IsEjectorZSafetyPos() && IsPinZSafetyPos();


        #region Lifecycle
        public override int OnRun() { int ret = 0; return ret; }
        public override int OnStop() { int ret = 0; base.OnStop(); return ret; }
        protected override int OnRunReady() { return 0; }
        protected override int OnRunWork() { return 0; }
        protected override int OnRunComplete() { return 0; }
        #endregion


        #region Seq 단위 동작 함수
        public int ChipPickUpWait()
        {
            int nRet = -1;
            /* TODO */
            return nRet;
        }

        public int ChipLoading()
        {
            int nRet = -1;
            /* TODO */
            return nRet;
        }
        #endregion
    }
}