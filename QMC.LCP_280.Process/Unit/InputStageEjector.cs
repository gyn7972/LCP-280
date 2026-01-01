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
            alarm.Title = "InputStage Not safety Pos.";
            alarm.Cause = "InputStage가 안전 위치가 아닙니다. 포지션 확인 후 다시 시작 하십시요.";
            alarm.Source = this.UnitName;
            alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
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
        // Safety 동작 중 여부
        private bool _isSafetyMoving = false;

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
        public override bool IsInterlockOK(BaseComponent baseComponent, BaseComponent.InterlockEventArgs e)
        {
            bool bRet = base.IsInterlockOK(baseComponent, e);
            if (baseComponent == this.AxisEjectorZ || baseComponent == this.AxisPinZ)
            {
                if (_isSafetyMoving)
                    return true;
                if (IsStageSafeForEjectorMove() == false)
                {
                    AxisEjectorZ?.EmgStop();
                    AxisPinZ?.EmgStop();
                    PostAlarm((int)AlarmKeys.eInputStageAxesMoving);
                    return false;
                }
            }
            return bRet;
        }
        private bool IsStageSafeForEjectorMove()
        {
            var stage = this.InputStage;
            if (stage == null)
                return false;

            // 1) 스테이지 자체 인터락 체크
            if (!stage.IsStageInterLockOK())
                return false;

            // 2) 축별로 분해해서 체크(가독성↑, 디버깅↑)
            bool xMoving, yMoving, tMoving;
            bool anyMoving = IsAnyStageAxisMovingDetailed(stage, out xMoving, out yMoving, out tMoving);

            if (anyMoving)
            {
                // 어느 축이 움직이는지 로그로 남김
                Log.Write(UnitName, "IsStageSafeForEjectorMove", $"Stage moving detected. X={xMoving}, Y={yMoving}, T={tMoving}");
                return false;
            }

            return true;
        }

        // 축별 상세 체크 헬퍼: X/Y/T 중 무엇이 움직이는지 반환
        private static bool IsAnyStageAxisMovingDetailed(InputStage stage, out bool xMoving, out bool yMoving, out bool tMoving)
        {
            xMoving = yMoving = tMoving = false;
            if (stage == null) return false;

            try { xMoving = stage.IsAxisMoving(AxisNames.WaferStageX); } catch { }
            try { yMoving = stage.IsAxisMoving(AxisNames.WaferStageY); } catch { }
            try { tMoving = stage.IsAxisMoving(AxisNames.WaferStageT); } catch { }

            return xMoving || yMoving || tMoving;
        }

        // ================== Generic Single Axis Move (Safety Interlock 동일 구조) ==================
        /// <summary>
        /// 단일 축 이동 (Safety 인터락 포함). 이동 완료까지 블록.
        /// </summary>
        public int MoveAxisPositionOne(MotionAxis axis, double target, bool isFine = false)
        {
            if (axis == null) return -1;

            if (InputStage != null && InputStage.IsAnyAxisMoving())
            {
                AxisEjectorZ.EmgStop();
                AxisPinZ.EmgStop();
                PostAlarm((int)AlarmKeys.eInputStageAxesMoving);
                return -1;
            }

            if (InputStage.IsStageInterLockOK() == false)
            {
                AxisEjectorZ.EmgStop();
                AxisPinZ.EmgStop();
                PostAlarm((int)AlarmKeys.eInputStageAxesMoving);
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
        /// //////////////////////////////////////////////////////////////////////////////////////////////
        /// 
        // UI, sequence 용 Move 함수

        //EjectBlockUp
        public int MovePositionEjectBlockUp(bool isFine = false)
        {
            int nRtn = 0;

            //움직이기전에 확인 후 구동.
            nRtn = IsMoveInterLockEjectBlockUp();
            if (nRtn != 0)
            {
                return -1;
            }

            Task<int> task = MovePositionAsyncEjectBlockUp(isFine);
            while (IsEndTask(task) == false)
            {
                // Check Interlock.!!! 구문 넣을것.!!!
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
            if (InputStage != null && InputStage.IsAnyAxisMoving())
            {
                if(InputStage.IsAnyAxisMoving())
                {
                    AxisEjectorZ.EmgStop();
                    AxisPinZ.EmgStop();

                    PostAlarm((int)AlarmKeys.eInputStageAxesMoving);
                    return -1;
                }
            }

            if (InputStage.IsStageInterLockOK() == false)
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
            if (InputStage != null && InputStage.IsAnyAxisMoving())
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

            while(IsEjectorZSafetyPos() == false)
            {
                if (IsStop)
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
            _isSafetyMoving = true;
            try
            {
                return MoveTeachingPositionOnce((int)InputStageEjectorConfig.TeachingPositionName.EjectBlockSafety, isFine);
            }
            finally
            {
                _isSafetyMoving = false;
            }
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
            _isSafetyMoving = true;
            try
            {
                return MoveTeachingPositionOnce((int)InputStageEjectorConfig.TeachingPositionName.EjectPinReady, isFine);
            }

            finally
            {
                _isSafetyMoving = false;
            }
        }
        private int IsMoveInterLockEjectPinReady()
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

        public int MovePositionEjectPinOffset(bool isFine = false)
        {
            //MovePositionEjectPinOffset <- 현재 Safety 역할 :: 이름 바꾸자.
            Task<int> task = MovePositionAsyncEjectPinOffset(isFine);
            while (IsEndTask(task) == false)
            {
                int nRtn = 0;
                nRtn = IsMoveInterLockEjectPinOffset();
                if (nRtn != 0)
                {
                    return -1;
                }

                Thread.Sleep(1);
            }
            return task.Result;
        }
        private Task<int> MovePositionAsyncEjectPinOffset(bool isFine = false)
        {
            return Task.Run(() =>
            {
                OnMovePositionEjectPinOffset(isFine);
                return 0;
            });
        }
        private int OnMovePositionEjectPinOffset(bool isFine = false)
        {
            _isSafetyMoving = true;
            try
            {
                return MoveTeachingPositionOnce((int)InputStageEjectorConfig.TeachingPositionName.EjectPinOffset, isFine);
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                return -1;
            }
            finally
            {
                _isSafetyMoving = false;
            }
        }

        private int IsMoveInterLockEjectPinOffset()
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

        public int MovePositionEjectPinChange(bool isFine = false)
        {
            Task<int> task = MovePositionAsyncEjectPinChange(isFine);
            while (IsEndTask(task) == false)
            {
                int nRtn = 0;
                nRtn = IsMoveInterLockEjectPinChange();
                if (nRtn != 0)
                {
                    return -1;
                }

                Thread.Sleep(1);
            }
            return task.Result;
        }
        private Task<int> MovePositionAsyncEjectPinChange(bool isFine = false)
        {
            return Task.Run(() =>
            {
                OnMovePositionEjectPinChange(isFine);
                return 0;
            });
        }
        private int OnMovePositionEjectPinChange(bool isFine = false)
        {
            _isSafetyMoving = true;
            try
            {
                return MoveTeachingPositionOnce((int)InputStageEjectorConfig.TeachingPositionName.EjectPinChange, isFine);
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                return -1;
            }
            finally
            {
                _isSafetyMoving = false;
            }
        }

        private int IsMoveInterLockEjectPinChange()
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

        public double GetTP(TeachingPosition tp, string axisKey) => (tp == null || string.IsNullOrEmpty(axisKey)) ? 0.0 : (tp.AxisPositions.TryGetValue(axisKey, out var v) ? v : 0.0);
        public double GetTP(TeachingPosition tp, MotionAxis axis) => axis == null ? 0.0 : GetTP(tp, axis.Name);
        #endregion

        #region Teaching
        public int MoveToTeachingPosition(string positionName, double vel = 0, double acc = 0, double dec = 0, double jerk = 0)
        {
            var tp = Config.GetTeachingPosition(positionName);
            if (tp == null) return -1;
            var (z, pz) = Config.GetPositionWithOffset(positionName);
            int rc = 0;

            //Todo : 인터락 확인 후 이동 하도록 수정.
            //if (_axEjectorZ != null)
            //    rc |= _axEjectorZ.MoveAbs(z, vel > 0 ? vel : _axEjectorZ.Config.MaxVelocity, acc > 0 ? acc : _axEjectorZ.Config.RunAcc, dec > 0 ? dec : _axEjectorZ.Config.RunDec, jerk > 0 ? jerk : _axEjectorZ.Config.AccJerkPercent);
            //if (_axPinZ != null)
            //    rc |= _axPinZ.MoveAbs(pz, vel > 0 ? vel : _axPinZ.Config.MaxVelocity, acc > 0 ? acc : _axPinZ.Config.RunAcc, dec > 0 ? dec : _axPinZ.Config.RunDec, jerk > 0 ? jerk : _axPinZ.Config.AccJerkPercent);

            return rc;
        }
        public int MoveToTeachingPosition(InputStageEjectorConfig.TeachingPositionName name, double vel = 0, double acc = 0, double dec = 0, double jerk = 0) => MoveToTeachingPosition(name.ToString(), vel, acc, dec, jerk);
        
        #endregion

        public bool IsEjectorZSafetyPos(double fallbackTolerance = 0.01,
                                         bool useAxisInposTolerance = true,
                                         bool treatMissingAsSafe = true,
                                         bool allowAbove = false)
        {
            if (_axEjectorZ == null)
                return treatMissingAsSafe;

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
            var tp1 = TeachingPositions[(int)InputStageEjectorConfig.TeachingPositionName.EjectPinReady];
            if (tp1 == null)
                return false;

            var tp2 = TeachingPositions[(int)InputStageEjectorConfig.TeachingPositionName.EjectPinOffset];
            if (tp2 == null)
                return false;

            bool bRet1 = InPosTeaching(tp1);
            bool bRet2 = InPosTeaching(tp2);
            if(bRet1 == true || bRet2 == true)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 두 축(EjectorZ & PinZ) 모두 Safety 판단
        /// </summary>
        public bool IsAllSafety() => IsEjectorZSafetyPos() && IsPinZSafetyPos();

        #region Position Checkers
        /// <summary>
        /// Teaching 포지션 이름 기준으로 이 유닛의 모든 대상 축이 In-Position인지 확인합니다.
        /// BaseUnit.InPosTeaching 사용.
        /// </summary>
        public bool IsInPosTeaching(string positionName)
        {
            try
            {
                if (string.IsNullOrEmpty(positionName))
                    return false;

                if (Config == null)
                    return false;

                var tp = Config.GetTeachingPosition(positionName);
                if (tp == null)
                    return false;

                bool inPos = InPosTeaching(positionName);
                return inPos;
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                return false;
            }
        }
        /// <summary>
        /// Teaching 인덱스로 현재 위치 일치 여부 확인.
        /// BaseConfig.GetTeachingPositionName을 통해 이름으로 매핑 후 InPosTeaching 사용.
        /// </summary>
        public bool IsInPosTeaching(int selIndex)
        {
            try
            {
                if (Config == null)
                    return false;

                string name;
                bool ok = Config.GetTeachingPositionName(selIndex, out name);
                if (ok == false)
                    return false;

                if (string.IsNullOrEmpty(name))
                    return false;

                bool inPos = IsInPosTeaching(name);
                return inPos;
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                return false;
            }
        }
        /// <summary>
        /// Teaching enum으로 현재 위치 일치 여부 확인.
        /// </summary>
        public bool IsInPosTeaching(InputStageEjectorConfig.TeachingPositionName name)
        {
            try
            {
                string tpName = name.ToString();
                bool inPos = IsInPosTeaching(tpName);
                return inPos;
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                return false;
            }
        }
        /// <summary>
        /// EjectorZ 축만 해당 Teaching 포지션 목표와 In-Position인지 확인합니다.
        /// BaseUnit.GetTP + MotionAxis.InPosition 사용.
        /// </summary>
        public bool IsEjectorZInPos(string positionName)
        {
            try
            {
                if (string.IsNullOrEmpty(positionName))
                    return false;

                if (_axEjectorZ == null)
                    return true; // 축 미구성 시 통과로 간주 (샘플 코드 정책과 동일)

                var tp = Config?.GetTeachingPosition(positionName);
                if (tp == null)
                    return false;

                double target = GetTP(positionName, AxisNames.EjectorZ);
                bool inpos;
                try
                {
                    inpos = _axEjectorZ.InPosition(target);
                }
                catch
                {
                    inpos = false;
                }
                return inpos;
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                return false;
            }
        }
        public bool IsEjectorZInPos(int selIndex)
        {
            try
            {
                if (Config == null)
                    return false;

                string name;
                bool ok = Config.GetTeachingPositionName(selIndex, out name);
                if (ok == false)
                    return false;

                if (string.IsNullOrEmpty(name))
                    return false;

                bool inPos = IsEjectorZInPos(name);
                return inPos;
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                return false;
            }
        }
        public bool IsEjectorZInPos(InputStageEjectorConfig.TeachingPositionName name)
        {
            try
            {
                string tpName = name.ToString();
                bool inPos = IsEjectorZInPos(tpName);
                return inPos;
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                return false;
            }
        }
        /// <summary>
        /// PinZ 축만 해당 Teaching 포지션 목표와 In-Position인지 확인합니다.
        /// BaseUnit.GetTP + MotionAxis.InPosition 사용.
        /// </summary>
        public bool IsPinZInPos(string positionName)
        {
            try
            {
                if (string.IsNullOrEmpty(positionName))
                    return false;

                if (_axPinZ == null)
                    return true; // 축 미구성 시 통과로 간주

                var tp = Config?.GetTeachingPosition(positionName);
                if (tp == null)
                    return false;

                double target = GetTP(positionName, AxisNames.EjectPinZ);
                bool inpos;
                try
                {
                    inpos = _axPinZ.InPosition(target);
                }
                catch
                {
                    inpos = false;
                }
                return inpos;
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                return false;
            }
        }
        public bool IsPinZInPos(int selIndex)
        {
            try
            {
                if (Config == null)
                    return false;

                string name;
                bool ok = Config.GetTeachingPositionName(selIndex, out name);
                if (ok == false)
                    return false;

                if (string.IsNullOrEmpty(name))
                    return false;

                bool inPos = IsPinZInPos(name);
                return inPos;
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                return false;
            }
        }
        public bool IsPinZInPos(InputStageEjectorConfig.TeachingPositionName name)
        {
            try
            {
                string tpName = name.ToString();
                bool inPos = IsPinZInPos(tpName);
                return inPos;
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                return false;
            }
        }

        // === 주요 포지션 편의 메서드 (전체 축 기준) ===
        public bool IsAtEjectBlockUp()
        {
            try
            {
                bool inPos = IsInPosTeaching(InputStageEjectorConfig.TeachingPositionName.EjectBlockUp);
                return inPos;
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                return false;
            }
        }
        public bool IsAtEjectBlockReady()
        {
            try
            {
                bool inPos = IsInPosTeaching(InputStageEjectorConfig.TeachingPositionName.EjectBlockReady);
                return inPos;
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                return false;
            }
        }
        public bool IsAtEjectBlockSafety()
        {
            try
            {
                bool inPos = IsInPosTeaching(InputStageEjectorConfig.TeachingPositionName.EjectBlockSafety);
                return inPos;
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                return false;
            }
        }
        public bool IsAtEjectPinReady()
        {
            try
            {
                bool inPos = IsInPosTeaching(InputStageEjectorConfig.TeachingPositionName.EjectPinReady);
                return inPos;
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                return false;
            }
        }
        #endregion



        #region Lifecycle
        public override int OnRun()
        {
            int ret = 0;
            if (this.RunUnitStatus == UnitStatus.Stopped ||
               this.RunUnitStatus == UnitStatus.Stopping ||
               this.RunUnitStatus == UnitStatus.CycleStop ||
               this.RunUnitStatus == UnitStatus.ManualRunning)
            {
                this.State = ProcessState.Stop;
                return 0;
            }
            
            if (ret != 0)
            {
                this.State = ProcessState.Stop;
                this.OnStop();
            }
            return ret;
        }
        protected override int OnStart()
        {
            return base.OnStart();
        }
        public override int OnStop()
        {
            int ret = 0;
            this.RunUnitStatus = UnitStatus.Stopped;
            base.OnStop();
            return ret;
        }
        protected override int OnRunReady() { return 0; }
        protected override int OnRunWork() { return 0; }
        protected override int OnRunComplete() { return 0; }
        #endregion

        #region Seq 단위 동작 함수
        #endregion

        #region Ready

        public int CheckReady(bool isFine = false)
        {
            Task<int> task = CheckReadyAsync(isFine);
            while (IsEndTask(task) == false)
            {
                Thread.Sleep(1);
            }
            return task.Result;
        }
        private Task<int> CheckReadyAsync(bool isFine = false)
        {
            return Task.Run(() =>
            {
                OnCheckReady(isFine);
                return 0;
            });
        }
        private int OnCheckReady(bool isFine)
        {
            int nRet = 0;

            if (IsAtEjectBlockSafety() == false ||
                IsAtEjectPinReady() == false)
            {
                nRet = MovePositionEjectBlockSafety(isFine);
                if (nRet != 0) 
                    return nRet;
                nRet = MovePositionEjectPinReady(isFine);
                if (nRet != 0) 
                    return nRet;
            }

            return nRet;
        }

        #endregion

        // 클래스 내부에 추가
        public void ResetForNewRun(bool moveToSafeReady = true)
        {
            // 1) 런타임/시퀀스 플래그 초기화
            _isSafetyMoving = false;
            this.CurrentFunc = null;

            // 2) 안전 위치 복귀(선택)
            if (moveToSafeReady)
            {
                try
                {
                    // 내부에서 인터락/축 동작을 확인하며 Safety/Ready로 이동
                    CheckReady();
                }
                catch (Exception ex)
                {
                    Log.Write(UnitName, $"[ResetForNewRun] CheckReady failed: {ex.Message}");
                }
            }
        }

        
    }
}