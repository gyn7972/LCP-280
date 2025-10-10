using QMC.Common;
using QMC.Common.Alarm;
using QMC.Common.Component;
using QMC.Common.IOUtil;
using QMC.Common.Motion;
using QMC.Common.Motions;
using QMC.Common.Unit;
using QMC.LCP_280.Process.Component;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Mail;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms.DataVisualization.Charting;
using System.Windows.Media;
using static QMC.LCP_280.Process.Equipment;

namespace QMC.LCP_280.Process.Unit
{
    /// <summary>
    /// InputDieTransfer Unit
    ///  - Tool T / Pick Z / Place Z 축 제어 + Teaching Position 및 Offset
    ///  - 4 Arm Vacuum / Blow / Vent 제어
    ///  - Air/Vac Tank Pressure / Arm Flow 등의 입력
    ///  - DryRun 시뮬레이션 지원
    ///  - OutputStage 스타일과 Region/메서드 레이아웃 통일
    /// </summary>
    public class InputDieTransfer : BaseUnit<InputDieTransferConfig>
    {
        public enum AlarmKeys
        {
            eInputStageNotSafe = 4001,
            eRotatyNotSafe,
            eInputStageEjectorPinZNotSafe,
            eInputStageEjectorZNotSafe,
            eInputStageAxesMoving = 4010,
            eRotaryAxesMoving,
            eInputStageEjectorAxesMoving,
            eInputDieTransferError,
            eInputStageVaccum,
            eInputDieTransferVaccum,
            eInputDieTransferNotSafety,
        }
        #region InitAlarm
        protected override void InitAlarm()
        {
            base.InitAlarm();
            AlarmInfo alarm = new AlarmInfo();
            alarm.Code = (int)AlarmKeys.eInputStageNotSafe;
            alarm.Title = "InputStage Not Sfarety Pos.";
            alarm.Cause = "InputStage가 안전 위치가 아닙니다.\n 포지션 확인 후 다시 시작 하십시요.";
            alarm.Source = this.UnitName;
            alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
            m_dicAlarms.Add(alarm.Code, alarm);

            alarm = new AlarmInfo();
            alarm.Code = (int)AlarmKeys.eRotatyNotSafe;
            alarm.Title = "Rotaty Not Sfarety Pos.";
            alarm.Cause = "Rotaty가 안전 위치가 아닙니다.\n 포지션 확인 후 다시 시작 하십시요.";
            alarm.Source = this.UnitName;
            alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
            m_dicAlarms.Add(alarm.Code, alarm);

            //,
            alarm = new AlarmInfo();
            alarm.Code = (int)AlarmKeys.eInputStageEjectorPinZNotSafe;
            alarm.Title = "EjectorPin Z-Axis Not Sfarety Pos.";
            alarm.Cause = "EjectorPin Z-Axis가 안전 위치가 아닙니다.\n 포지션 확인 후 다시 시작 하십시요.";
            alarm.Source = this.UnitName;
            alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
            m_dicAlarms.Add(alarm.Code, alarm);
            //,
            alarm = new AlarmInfo();
            alarm.Code = (int)AlarmKeys.eInputStageEjectorZNotSafe;
            alarm.Title = "Ejector Z-Axis Not Sfarety Pos.";
            alarm.Cause = "Ejector Z-Axis가 안전 위치가 아닙니다.\n 포지션 확인 후 다시 시작 하십시요.";
            alarm.Source = this.UnitName;
            alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
            m_dicAlarms.Add(alarm.Code, alarm);

            alarm = new AlarmInfo();
            alarm.Code = (int)AlarmKeys.eInputStageAxesMoving;
            alarm.Title = "InputStage Axis Moving";
            alarm.Cause = "InputStage 축이 이동 중입니다. 정지 후 다시 시도하십시오.";
            alarm.Source = this.UnitName;
            alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
            m_dicAlarms.Add(alarm.Code, alarm);

            alarm = new AlarmInfo();
            alarm.Code = (int)AlarmKeys.eRotaryAxesMoving;
            alarm.Title = "Rotary Axis Moving";
            alarm.Cause = "Rotary 축이 이동 중입니다. 정지 후 다시 시도하십시오.";
            alarm.Source = this.UnitName;
            alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
            m_dicAlarms.Add(alarm.Code, alarm);

            alarm = new AlarmInfo();
            alarm.Code = (int)AlarmKeys.eInputStageEjectorAxesMoving;
            alarm.Title = "Ejector Axis Moving";
            alarm.Cause = "InputStageEjector 축이 이동 중입니다. 정지 후 다시 시도하십시오.";
            alarm.Source = this.UnitName;
            alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
            m_dicAlarms.Add(alarm.Code, alarm);


            alarm = new AlarmInfo();
            alarm.Code = (int)AlarmKeys.eInputDieTransferError;
            alarm.Title = "InputDieTransferError";
            alarm.Cause = "InputDieTransfer명령중 예기치 않은 에러를 만났습니다. 관리자에게 문의 하여 주십시요.";
            alarm.Source = this.UnitName;
            alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
            m_dicAlarms.Add(alarm.Code, alarm);

            //eInputStageVaccum
            alarm = new AlarmInfo();
            alarm.Code = (int)AlarmKeys.eInputStageVaccum;
            alarm.Title = "eInputStageVaccumError";
            alarm.Cause = "eInputStageVaccum 에러를 만났습니다. 공압 확인 바랍니다.";
            alarm.Source = this.UnitName;
            alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
            m_dicAlarms.Add(alarm.Code, alarm);

            //
            alarm = new AlarmInfo();
            alarm.Code = (int)AlarmKeys.eInputDieTransferVaccum;
            alarm.Title = "InputDieTransferVaccumError";
            alarm.Cause = "InputDieTransferVaccum 에러를 만났습니다. 공압 확인 바랍니다.";
            alarm.Source = this.UnitName;
            alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
            m_dicAlarms.Add(alarm.Code, alarm);

            alarm = new AlarmInfo();
            alarm.Code = (int)AlarmKeys.eInputDieTransferNotSafety;
            alarm.Title = "InputDieTransfer Not Sfarety Pos.";
            alarm.Cause = "InputDieTransfer가 안전 위치가 아닙니다.\n 포지션 확인 후 다시 시작 하십시요.";
            alarm.Source = this.UnitName;
            alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
            m_dicAlarms.Add(alarm.Code, alarm);

        }
        #endregion

        #region Config / Teaching
        public InputDieTransferConfig InputDieTransferConfig => Config;
        #endregion

        #region Unit
        InputStage InputStage { get; set; }
        InputStageEjector InputStageEjector { get; set; }
        Rotary Rotary { get; set; }
        #endregion

        #region Axes
        private MotionAxis _toolT, _pickZ, _placeZ;
        public MotionAxis AxisToolT => _toolT;
        public MotionAxis AxisPickZ => _pickZ;
        public MotionAxis AxisPlaceZ => _placeZ;

        
        #endregion

        #region ctor / Initialization
        public InputDieTransfer(InputDieTransferConfig config = null)
            : base(config ?? new InputDieTransferConfig())
        {
            AddComponents();
        }

        public override void AddComponents()
        {
            Config.LoadAndBindAxes(Equipment.Instance.AxisManager);
            Config.InitializeDefaultTeachingPositions();
            
            BindAxes();
            BindIoDomains();    // (Arm IO 는 단순 DO/DI 이름 관리이므로, 별도 Cylinder/Vacuum Domain 매핑은 선택)
        }
        #endregion

        protected override void OnBindUnit()
        {
            base.OnBindUnit();
            InputStage = Equipment.Instance.GetUnit(UnitKeys.InputStage) as InputStage;
            InputStageEjector = Equipment.Instance.GetUnit(UnitKeys.InputStageEjector) as InputStageEjector;
            Rotary = Equipment.Instance.GetUnit(UnitKeys.Rotary) as Rotary;
        }

        #region Axis Binding / Helpers
        private void BindAxes()
        {
            var mgr = Equipment.Instance?.AxisManager;
            if (mgr == null)
            {
                Log.Write("InputDieTransfer", "[BindAxes] AxisManager null");
                return;
            }

            const string unitName = "Unit"; // Equipment에서 축 등록 시 사용한 유닛명과 동일해야 함
            BindAxis(mgr, unitName, AxisNames.LeftToolT, ref _toolT);
            BindAxis(mgr, unitName, AxisNames.LeftPickZ, ref _pickZ);
            BindAxis(mgr, unitName, AxisNames.LeftPlaceZ, ref _placeZ);
        }

        public int MoveAxisPositionOne(MotionAxis axis, double target, bool isFine = false)
        {
            if (axis == null) return -1;

            Task<int> task = MoveAxisPositionOneAsync(axis, target, isFine);
            while (IsEndTask(task) == false)
            {
                if(axis == AxisPickZ)
                {
                    if (InputStage.IsAnyAxisMoving())
                    {
                        AxisToolT.EmgStop();
                        AxisPickZ.EmgStop();
                        AxisPlaceZ.EmgStop();
                        PostAlarm((int)AlarmKeys.eInputStageAxesMoving);
                        return -1;
                    }
                }

                if (axis == AxisPlaceZ)
                {
                    if (Rotary.IsAnyAxisMoving())
                    {
                        AxisToolT.EmgStop();
                        AxisPickZ.EmgStop();
                        AxisPlaceZ.EmgStop();
                        PostAlarm((int)AlarmKeys.eRotaryAxesMoving);
                    }
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
                IsMoveInterLockSafetyZ();
                Thread.Sleep(1);
            }
            return task.Result;
        }
        public Task<int> MovePositionAsyncSafetyZ(bool isFine = false)
        {
            return Task.Run(() =>
            {
                OnMovePositionSafetyZ(isFine);
                return 0;
            });
        }
        private int OnMovePositionSafetyZ(bool isFine = false)
        {
            return MoveTeachingPositionOnce((int)InputDieTransferConfig.TeachingPositionName.SafetyZone, isFine);
        }
        private int IsMoveInterLockSafetyZ()
        {
            int nRet = 0;
            // Check Interlock.!!! 구문 넣을것.!!!

            //SafetyZ로 이동 시에는 인터락 필요 없음.
            //if (InputStage.IsAnyAxisMoving())
            //{
            //    AxisToolT.EmgStop();
            //    AxisPickZ.EmgStop();
            //    AxisPlaceZ.EmgStop();
            //    AlarmPost((int)AlarmKeys.eInputStageAxesMoving);
            //    return -1;
            //}

            //if (InputStageEjector.IsAnyAxisMoving())
            //{
            //    AxisToolT.EmgStop();
            //    AxisPickZ.EmgStop();
            //    AxisPlaceZ.EmgStop();
            //    AlarmPost((int)AlarmKeys.eInputStageEjectorAxesMoving);
            //}

            //if (Rotary.IsAnyAxisMoving())
            //{
            //    AxisToolT.EmgStop();
            //    AxisPickZ.EmgStop();
            //    AxisPlaceZ.EmgStop();
            //    AlarmPost((int)AlarmKeys.eRotaryAxesMoving);
            //}

            return nRet;
        }
        public Task<int> MovePositionAsyncSafeSafetyZ(bool isFine = false, CancellationToken ct = default(CancellationToken))
        {
            return Task.Run(() =>
            {
                // OnMovePickUpPosition을 Task로 돌리고 별도 인터락/취소 감시
                var coreTask = Task.Run(() => OnMovePositionSafetyZ(isFine), ct);

                while (!IsEndTask(coreTask))
                {
                    if (ct.IsCancellationRequested)
                    {
                        try
                        {
                            AxisToolT?.EmgStop();
                            AxisPickZ?.EmgStop();
                            AxisPlaceZ?.EmgStop();
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
            }, 
            ct);
        }


        ///////////////////////////////////////////////////////////////////////////////////////////
        // Single Pickup (Non-Index) - 구조 통일 (Index 기반 메서드 패턴과 동일 스타일)
        public int MovePositionPickUp(bool isFine = false)
        {
            Task<int> task = MovePositionAsyncPickUp(isFine);
            while (!IsEndTask(task))
            {
                int interlock = IsMoveInterLockPickUp();
                if (interlock != 0)
                {
                    return -1;
                }
                Thread.Sleep(0);
            }
            return task.Result;
        }

        private Task<int> MovePositionAsyncPickUp(bool isFine = false)
        {
            return Task.Run(() =>
            {
                return OnMovePositionPickUp(isFine);
            });
        }

        private int OnMovePositionPickUp(bool isFine = false)
        {
            // 안전 Z 위치 확인 후 필요 시 이동
            if (!IsPositionPlaceZSafety() || !IsPositionPickZSafety())
            {
                int safety = MovePositionSafetyZ(isFine);
                if (safety != 0)
                    return -1;
            }

            // 1) ToolT 이동
            int r = MoveToolT_ToPickup(isFine);
            if (r != 0)
                return -1;

            // 2) PickZ 이동
            r = MovePickZ_ToPickup(isFine);
            if (r != 0)
                return -1;

            return 0;
        }

        private int IsMoveInterLockPickUp()
        {
            // InputStage 축 움직임 감시
            if (InputStage != null && InputStage.IsAnyAxisMoving())
            {
                AxisToolT?.EmgStop();
                AxisPickZ?.EmgStop();
                AxisPlaceZ?.EmgStop();
                PostAlarm((int)AlarmKeys.eInputStageAxesMoving);
                return -1;
            }

            // Ejector 축 움직임 감시
            if (InputStageEjector != null && InputStageEjector.IsAnyAxisMoving())
            {
                AxisToolT?.EmgStop();
                AxisPickZ?.EmgStop();
                AxisPlaceZ?.EmgStop();
                PostAlarm((int)AlarmKeys.eInputStageEjectorAxesMoving);
                return -1;
            }
            // Rotary는 Place 시에만 필요 → 기존 주석 유지
            return 0;
        }

        // ToolT만 Pickup 위치로
        public int MovePositionPickUpToolT(bool isFine = false)
        {
            Task<int> task = MovePositionAsyncPickUpToolT(isFine);
            while (!IsEndTask(task))
            {
                int interlock = IsMoveInterLockPickUpToolT();
                if (interlock != 0)
                    return -1;
                Thread.Sleep(0);
            }
            return task.Result;
        }

        private Task<int> MovePositionAsyncPickUpToolT(bool isFine = false)
        {
            return Task.Run(() => OnMovePositionPickUpToolT(isFine));
        }

        private int OnMovePositionPickUpToolT(bool isFine = false)
        {
            // Z 안전 확인
            if (!IsPositionPlaceZSafety() || !IsPositionPickZSafety())
            {
                int safety = MovePositionSafetyZ(isFine);
                if (safety != 0) return -1;
            }
            return MoveToolT_ToPickup(isFine);
        }

        private int IsMoveInterLockPickUpToolT()
        {
            // 필요 시 별도 로직 추가 (현재는 안전 Z 이동 선행하므로 단순 성공)
            return 0;
        }

        // PickZ만 Pickup 위치로
        public int MovePositionPickUpPickZ(bool isFine = false)
        {
            Task<int> task = MovePositionAsyncPickUpPickZ(isFine);
            while (!IsEndTask(task))
            {
                int interlock = IsMoveInterLockPickUpPickZ();
                if (interlock != 0)
                    return -1;
                Thread.Sleep(0);
            }
            return task.Result;
        }

        private Task<int> MovePositionAsyncPickUpPickZ(bool isFine = false)
        {
            return Task.Run(() => OnMovePositionPickUpPickZ(isFine));
        }

        private int OnMovePositionPickUpPickZ(bool isFine = false)
        {
            // ToolT는 이미 위치했다고 가정 가능. 필요 시 SafetyZ만 검사
            // (PickZ 단독 이동은 ToolT 선행 이동 후 사용을 권장)
            double target = GetTP(InputDieTransferConfig.TeachingPositionName.Pickup.ToString(),
                                  AxisNames.LeftPickZ);
            int r = MoveAxisPositionOne(AxisPickZ, target, isFine);
            if (r != 0)
            {
                Log.Write(UnitName, "[OnMovePositionPickUpPickZ] PickZ move failed");
                return -1;
            }
            return 0;
        }

        private int IsMoveInterLockPickUpPickZ()
        {
            if (InputStage != null && InputStage.IsAnyAxisMoving())
            {
                AxisToolT?.EmgStop();
                AxisPickZ?.EmgStop();
                AxisPlaceZ?.EmgStop();
                PostAlarm((int)AlarmKeys.eInputStageAxesMoving);
                return -1;
            }
            if (InputStageEjector != null && InputStageEjector.IsAnyAxisMoving())
            {
                AxisToolT?.EmgStop();
                AxisPickZ?.EmgStop();
                AxisPlaceZ?.EmgStop();
                PostAlarm((int)AlarmKeys.eInputStageEjectorAxesMoving);
                return -1;
            }
            return 0;
        }

        // 실제 축 이동 Helper (Pickup 단일 Teaching)
        private int MoveToolT_ToPickup(bool isFine)
        {
            double target = GetTP(InputDieTransferConfig.TeachingPositionName.Pickup.ToString(),
                                  AxisNames.LeftToolT);
            int r = MoveAxisPositionOne(AxisToolT, target, isFine);
            if (r != 0)
            {
                Log.Write(UnitName, "[MoveToolT_ToPickup] ToolT move failed");
                return -1;
            }
            return 0;
        }

        private int MovePickZ_ToPickup(bool isFine)
        {
            double target = GetTP(InputDieTransferConfig.TeachingPositionName.Pickup.ToString(),
                                  AxisNames.LeftPickZ);
            int r = MoveAxisPositionOne(AxisPickZ, target, isFine);
            if (r != 0)
            {
                Log.Write(UnitName, "[MovePickZ_ToPickup] PickZ move failed");
                return -1;
            }
            return 0;
        }
        ///////////////////////////////////////////////////////////////////////////////////////////

        public Task<int> MovePositionAsyncSafePickUp(bool isFine = false, CancellationToken ct = default(CancellationToken))
        {
            return Task.Run(() =>
            {
                // OnMovePickUpPosition을 Task로 돌리고 별도 인터락/취소 감시
                var coreTask = Task.Run(() => OnMovePositionPickUp(isFine), ct);

                while (!IsEndTask(coreTask))
                {
                    if (ct.IsCancellationRequested)
                    {
                        try
                        {
                            AxisToolT?.EmgStop();
                            AxisPickZ?.EmgStop();
                            AxisPlaceZ?.EmgStop();
                        }
                        catch { }
                        return -999; // 취소 코드
                    }

                    int nRtn = IsMoveInterLockPickUp();
                    if (nRtn != 0)
                    {
                        return -1;
                    }

                    Thread.Sleep(5); // 0→5ms로 약간 여유 (CPU 점유 감소)
                }

                return coreTask.Result;
            }, ct);
        }


        public int MovePositionReady(bool isFine = false)
        {
            Task<int> task = MovePositionAsyncReady(isFine);
            while (IsEndTask(task) == false)
            {
                int nRtn = IsMoveInterLockReady();
                if (nRtn != 0)
                {
                    return -1;
                }

                Thread.Sleep(0);
            }
            return task.Result;
        }
        public Task<int> MovePositionAsyncReady(bool isFine = false)
        {
            return Task.Run(() =>
            {
                OnMovePositionReady(isFine);
                return 0;
            });
        }
        private int OnMovePositionReady(bool isFine = false)
        {
            int nRet = 0;
            if (!IsPositionPlaceZSafety() || !IsPositionPickZSafety())
            {
                nRet = MovePositionSafetyZ();
                if (nRet != 0)
                {
                    return -1;
                }
            }

            double dTPos = GetTP(InputDieTransferConfig.TeachingPositionName.Ready.ToString(),
                        AxisNames.LeftToolT);
            nRet = MoveAxisPositionOne(AxisToolT, dTPos);
            if (nRet != 0)
            {
                return -1;
            }

            double dZPos = GetTP(InputDieTransferConfig.TeachingPositionName.Ready.ToString(),
                        AxisNames.LeftPlaceZ);
            nRet = MoveAxisPositionOne(AxisPlaceZ, dZPos);
            if (nRet != 0)
            {
                return -1;
            }

            return nRet;
            //return MoveTeachingPositionOnce((int)InputDieTransferConfig.TeachingPositionName.Pickup, isFine);
        }
        private int IsMoveInterLockReady()
        {
            int nRet = 0;
            if (InputStage != null && InputStage.IsAnyAxisMoving())
            {
                AxisToolT?.EmgStop();
                AxisPickZ?.EmgStop();
                AxisPlaceZ?.EmgStop();
                PostAlarm((int)AlarmKeys.eInputStageAxesMoving);
                return -1;
            }

            if (InputStageEjector != null && InputStageEjector.IsAnyAxisMoving())
            {
                AxisToolT?.EmgStop();
                AxisPickZ?.EmgStop();
                AxisPlaceZ?.EmgStop();
                PostAlarm((int)AlarmKeys.eInputStageEjectorAxesMoving);
                return -1;
            }

            if (Rotary != null && Rotary.IsAnyAxisMoving())
            {
                AxisToolT?.EmgStop();
                AxisPickZ?.EmgStop();
                AxisPlaceZ?.EmgStop();
                PostAlarm((int)AlarmKeys.eRotaryAxesMoving);
                return -1;
            }

            return nRet;
        }
        public Task<int> MovePositionAsyncSafeReady(bool isFine = false, CancellationToken ct = default(CancellationToken))
        {
            return Task.Run(() =>
            {
                // Task로 돌리고 별도 인터락/취소 감시
                var coreTask = Task.Run(() => OnMovePositionReady(isFine), ct);

                while (!IsEndTask(coreTask))
                {
                    if (ct.IsCancellationRequested)
                    {
                        try
                        {
                            AxisToolT?.EmgStop();
                            AxisPickZ?.EmgStop();
                            AxisPlaceZ?.EmgStop();
                        }
                        catch { }
                        return -999; // 취소 코드
                    }

                    int nRtn = IsMoveInterLockReady();
                    if (nRtn != 0)
                    {
                        return -1;
                    }

                    Thread.Sleep(5); // 0→5ms로 약간 여유 (CPU 점유 감소)
                }

                return coreTask.Result;
            }, ct);
        }

        public int MovePositionPlace_Index(int nIndex = 0, bool isFine = false)
        {
            Task<int> task = MovePositionAsyncPlace_Index(isFine, nIndex);
            while (IsEndTask(task) == false)
            {
                int nRtn = IsMoveInterLockPlace_Index(nIndex);
                if (nRtn != 0)
                {
                    return -1;
                }

                Thread.Sleep(0);
            }
            return task.Result;
        }
        public Task<int> MovePositionAsyncPlace_Index(bool isFine = false, int nIndex = 0)
        {
            return Task.Run(() =>
            {
                OnMovePositionPlace_Index(isFine, nIndex);
                return 0;
            });
        }
        private int OnMovePositionPlace_Index(bool isFine = false, int nIndex = 0)
        {
            int nRet = 0;
            if (!IsPositionPlaceZSafety() || !IsPositionPickZSafety())
            {
                nRet = MovePositionSafetyZ();
                if (nRet != 0)
                {
                    return -1;
                }
            }

            // nIndex 처리 (0-based와 1-based 모두 지원)
            //  - 1~8 : 그대로 사용 (Place_Index1 ~ Place_Index8)
            //  - 0~7 : +1 보정하여 1~8 매핑
            int teachingIdx;
            if (nIndex >= 1 && nIndex <= 8)
                teachingIdx = nIndex + 1;
            else if (nIndex >= 0 && nIndex < 8)
                teachingIdx = nIndex + 1; // 0-based 입력으로 판단
            else
            {
                Log.Write(UnitName, $"[OnMovePositionPlace_Index] Invalid index {nIndex}. Range 0~7 or 1~8");
                return -1;
            }

            string tpName = $"Place_Index{teachingIdx}";
            var tpObj = InputDieTransferConfig.GetTeachingPosition(tpName);
            if (tpObj == null)
            {
                Log.Write(UnitName, $"[OnMovePositionPlace_Index] Teaching not found: {tpName}");
                return -1;
            }

            double dTPos = GetTP(tpName, AxisNames.LeftToolT);
            nRet = MoveAxisPositionOne(AxisToolT, dTPos);
            if (nRet != 0)
            {
                Log.Write(UnitName, $"[OnMovePositionPlace_Index] ToolT move failed tp={tpName} pos={dTPos}");
                return -1;
            }

            double dZPos = GetTP(tpName, AxisNames.LeftPlaceZ);
            nRet = MoveAxisPositionOne(AxisPlaceZ, dZPos);
            if (nRet != 0)
            {
                Log.Write(UnitName, $"[OnMovePositionPlace_Index] PlaceZ move failed tp={tpName} pos={dZPos}");
                return -1;
            }

            return nRet;
            //return MoveTeachingPositionOnce((int)InputDieTransferConfig.TeachingPositionName.Pickup, isFine);
        }
        private int IsMoveInterLockPlace_Index(int nIndex = 0)
        {
            int nRet = 0;
            
            if (Rotary != null && Rotary.IsAnyAxisMoving())
            {
                AxisToolT?.EmgStop();
                AxisPickZ?.EmgStop();
                AxisPlaceZ?.EmgStop();
                PostAlarm((int)AlarmKeys.eRotaryAxesMoving);
                return -1;
            }

            return nRet;
        }
        public Task<int> MovePositionAsyncSafePlace_Index(bool isFine = false, int nIndex = 0, CancellationToken ct = default(CancellationToken))
        {
            return Task.Run(() =>
            {
                // Task로 돌리고 별도 인터락/취소 감시
                var coreTask = Task.Run(() => OnMovePositionPlace_Index(isFine, nIndex), ct);

                while (!IsEndTask(coreTask))
                {
                    if (ct.IsCancellationRequested)
                    {
                        try
                        {
                            AxisToolT?.EmgStop();
                            AxisPickZ?.EmgStop();
                            AxisPlaceZ?.EmgStop();
                        }
                        catch { }
                        return -999; // 취소 코드
                    }

                    int nRtn = IsMoveInterLockPlace_Index(nIndex);
                    if (nRtn != 0)
                    {
                        return -1;
                    }

                    Thread.Sleep(5); // 0→5ms로 약간 여유 (CPU 점유 감소)
                }

                return coreTask.Result;
            }, ct);
        }

        #endregion

        #region Dual Axis (PickZ + PinZ) Simultaneous Move
        /// <summary>
        /// PickZ 와 PinZ 를 Offset(상대이동)으로 동시에 구동.
        ///  - 두 축 모두 상대이동 (MoveRel) 사용
        ///  - velPickZ / velPinZ = 0 이면 각 축 설정(MaxVelocity/RunAcc/RunDec) 사용
        ///  - timeoutMs > 0 이고 시간 초과 시 -2 반환
        ///  - Interlock 위반 시 두 축 Emergency Stop 후 -1 반환
        /// </summary>
        public int MovePickZAndPinZByOffset(double pickZOffset,
                                            double pinZOffset,
                                            double velPickZ = 0,
                                            double velPinZ = 0,
                                            double acc = 0,
                                            double dec = 0,
                                            int timeoutMs = 0,
                                            bool isFine = false)
        {
            var pick = AxisPickZ;
            var pin = InputStageEjector != null ? InputStageEjector.AxisPinZ : null;

            if (pick == null || pin == null)
            {
                Log.Write(UnitName, "[MovePickZAndPinZByOffset] Axis null");
                return -1;
            }

            // 이동 필요 없으면 즉시 성공
            if (System.Math.Abs(pickZOffset) < 1e-9 && System.Math.Abs(pinZOffset) < 1e-9)
                return 0;

            // 사전 Interlock (다른 관련 Unit 축 동작 중이면 시작하지 않음)
            if (InputStage != null && InputStage.IsAnyAxisMoving())
            {
                AxisToolT.EmgStop();
                AxisPickZ.EmgStop();
                AxisPlaceZ.EmgStop();
                PostAlarm((int)AlarmKeys.eInputStageAxesMoving);
                return -1;
            }

            if (InputStageEjector != null && InputStageEjector.IsAnyAxisMoving())
            {
                AxisToolT.EmgStop();
                AxisPickZ.EmgStop();
                AxisPlaceZ.EmgStop();
                PostAlarm((int)AlarmKeys.eInputStageEjectorAxesMoving);
                return -1;
            }

            pickZOffset = Config.dPickUpOffset;
            double vPick = Config.dPickUpSpeed;
            double aPick = Config.dPickUpAcc;
            double dPick = Config.dPickUpDec;

            pinZOffset = InputStageEjector.Config.dPickUpOffset;
            double vPin = InputStageEjector.Config.dPickUpSpeed;
            double aPin = InputStageEjector.Config.dPickUpAcc;
            double dPin = InputStageEjector.Config.dPickUpDec;

            // 동시에 시작 (반환코드 OR)
            //ex) Offset값이 양수로 300 이면 Z축이 위로 300 이동
            // 두 개의 축 전부 300이면 동일하게 위로 올라간다.
            int rc = 0;
            rc |= pick.MoveRel(pickZOffset, vPick, aPick, dPick, pick.Config.AccJerkPercent);
            rc |= pin.MoveRel(pinZOffset, vPin, aPin, dPin, pin.Config.AccJerkPercent);
            if (rc != 0)
            {
                Log.Write(UnitName, "[MovePickZAndPinZByOffset] MoveRel start failed rc=" + rc);
                return -1;
            }

            var sw = timeoutMs > 0 ? Stopwatch.StartNew() : null;
            while (true)
            {
                bool pickMoving = pick.IsMoveDone();
                bool pinMoving = pin.IsMoveDone();

                // 완료
                if (pickMoving && pinMoving)
                {
                    Log.Write(UnitName, "[MovePickZAndPinZByOffset] pickMoving && pinMoving : Comp.");
                    break;
                }
                    
                // 진행 중 Interlock 감시 (기존 MoveAxisWithSafety 로직과 유사)
                if (InputStage != null && InputStage.IsAnyAxisMoving())
                {
                    pick.EmgStop(); pin.EmgStop();
                    AxisToolT.EmgStop();
                    AxisPickZ.EmgStop();
                    AxisPlaceZ.EmgStop();
                    PostAlarm((int)AlarmKeys.eInputStageAxesMoving);
                    Log.Write(UnitName, "[MovePickZAndPinZByOffset] InputStage");
                    return -1;
                }
                
                // Ejector 다른 축(EjectorZ) 움직임 감시
                if (InputStageEjector != null && 
                    InputStageEjector.IsAxisMoving(AxisNames.EjectorZ))
                {
                    pick.EmgStop(); pin.EmgStop();
                    AxisToolT.EmgStop();
                    AxisPickZ.EmgStop();
                    AxisPlaceZ.EmgStop();
                    PostAlarm((int)AlarmKeys.eInputStageEjectorAxesMoving);
                    Log.Write(UnitName, "[MovePickZAndPinZByOffset] InputStageEjector");
                    return -1;
                }

                // 타임아웃
                if (sw != null && sw.ElapsedMilliseconds > timeoutMs)
                {
                    pick.EmgStop(); pin.EmgStop();
                    Log.Write(UnitName, "[MovePickZAndPinZByOffset] Timeout");
                    return -2;
                }

                Thread.Sleep(1);
            }

            return 0;
        }

        /// <summary>
        /// 비동기 버전 (Task 반환). 필요 시 UI 에서 await 사용.
        /// </summary>
        public Task<int> MovePickZAndPinZByOffsetAsync(double pickZOffset,
                                                       double pinZOffset,
                                                       double velPickZ = 0,
                                                       double velPinZ = 0,
                                                       double acc = 0,
                                                       double dec = 0,
                                                       int timeoutMs = 0,
                                                       bool isFine = false)
        {
            return Task.Run(() =>
            {
                return MovePickZAndPinZByOffset(pickZOffset, pinZOffset, velPickZ, velPinZ, acc, dec, timeoutMs, isFine);
            });
        }
        #endregion

        #region Position Check (Pickup / Ready / Place Index)
        /// DieTransfer PickZ 축이 SafetyPos Teaching (Offset 적용) 위치(또는 허용오차 범위)인지 확인.
        /// Teaching 이름이 SafetyPos 없으면 SafetyZone 순으로 fallback (둘 다 없으면 false).
        /// 장치/축이 없으면 true(안전)로 간주. 필요 시 treatMissingAsSafe=false 로 변경 가능.
        /// </summary>
        /// <param name="fallbackTolerance">축 설정값을 못 가져올 때 사용할 기본 허용오차</param>
        /// <param name="useAxisInposTolerance">축 Config.InposTolerance 사용 여부</param>
        /// <param name="treatMissingAsSafe">장치/Teaching 미존재 시 true 반환할지 여부</param>
        public bool IsPositionPickZSafety(double fallbackTolerance = 0.01,
                                                 bool useAxisInposTolerance = true,
                                                 bool treatMissingAsSafe = true)
        {
            if (AxisPickZ == null)
                return treatMissingAsSafe;

            var cfg = InputDieTransferConfig;
            if (cfg == null) return false;

            // 우선순위: SafetyPos → SafetyZone
            string[] candidateNames =
            {
                "SafetyPos",
                InputDieTransferConfig.TeachingPositionName.SafetyZone.ToString()
            };

            string foundName = null;
            foreach (var name in candidateNames)
            {
                if (cfg.GetTeachingPosition(name) != null)
                {
                    foundName = name;
                    break;
                }
            }

            if (foundName == null)
                return treatMissingAsSafe ? true : false;

            var tp = cfg.GetTeachingPosition(foundName);
            if (tp == null) return false;

            // Offset 적용 PickZ 목표값
            var (_, pickZTarget, _) = cfg.GetPositionWithOffset(foundName);

            double cur = AxisPickZ.GetPosition();
            double tol = useAxisInposTolerance
                ? (AxisPickZ.Config?.InposTolerance ?? fallbackTolerance)
                : fallbackTolerance;

            // 동일위치(=InPos) 판정
            return System.Math.Abs(cur - pickZTarget) <= tol;
        }

        /// <summary>
        /// DieTransfer ToolT 축이 SafetyPos(or SafetyZone fallback) 위치인지 확인.
        /// SafetyZone Teaching에 ToolT 값이 없으면 다음 후보로 넘어감.
        /// </summary>
        public bool IsPositionToolTSafety(double fallbackTolerance = 0.01,
                                                 bool useAxisInposTolerance = true,
                                                 bool treatMissingAsSafe = true)
        {
            if (AxisToolT == null)
                return treatMissingAsSafe;

            var cfg = InputDieTransferConfig;
            if (cfg == null) return false;

            string[] candidateNames =
            {
                "SafetyPos",
                InputDieTransferConfig.TeachingPositionName.SafetyZone.ToString()
            };

            string foundName = null;
            foreach (var name in candidateNames)
            {
                var tpTest = cfg.GetTeachingPosition(name);
                if (tpTest == null) continue;
                // 해당 Teaching에 ToolT 좌표가 실제 존재하는지 확인 (없으면 스킵)
                if (tpTest.AxisPositions != null &&
                    tpTest.AxisPositions.Keys.Any(k => string.Equals(k, AxisNames.LeftToolT, StringComparison.OrdinalIgnoreCase)))
                {
                    foundName = name;
                    break;
                }
            }

            if (foundName == null)
                return treatMissingAsSafe;

            var (_, _, _) = cfg.GetPositionWithOffset(foundName);
            // Offset 적용 튜플에서 t 사용
            var (tTarget, _, _) = cfg.GetPositionWithOffset(foundName);

            double cur = AxisToolT.GetPosition();
            double tol = useAxisInposTolerance
                ? (AxisToolT.Config?.InposTolerance ?? fallbackTolerance)
                : fallbackTolerance;

            return System.Math.Abs(cur - tTarget) <= tol;
        }

        /// <summary>
        /// DieTransfer PlaceZ 축이 SafetyPos(or SafetyZone fallback) 위치인지 확인.
        /// </summary>
        public bool IsPositionPlaceZSafety(double fallbackTolerance = 0.01,
                                                  bool useAxisInposTolerance = true,
                                                  bool treatMissingAsSafe = true)
        {
            if (AxisPlaceZ == null)
                return treatMissingAsSafe;

            var cfg = InputDieTransferConfig;
            if (cfg == null) return false;

            string[] candidateNames =
            {
                "SafetyPos",
                InputDieTransferConfig.TeachingPositionName.SafetyZone.ToString()
            };

            string foundName = null;
            foreach (var name in candidateNames)
            {
                if (cfg.GetTeachingPosition(name) != null)
                {
                    foundName = name;
                    break;
                }
            }

            if (foundName == null)
                return treatMissingAsSafe;

            var (_, _, placeZTarget) = cfg.GetPositionWithOffset(foundName);

            double cur = AxisPlaceZ.GetPosition();
            double tol = useAxisInposTolerance
                ? (AxisPlaceZ.Config?.InposTolerance ?? fallbackTolerance)
                : fallbackTolerance;

            return System.Math.Abs(cur - placeZTarget) <= tol;
        }

        // ---- Pickup ----
        public bool IsPositionToolTPickup()
        {
            const string tpName = nameof(InputDieTransferConfig.TeachingPositionName.Pickup);
            if (AxisToolT == null) return true;
            double target = GetTP(tpName, AxisNames.LeftToolT);
            try { return AxisToolT.InPosition(target); } catch { return false; }
        }

        public bool IsPositionPickZPickup()
        {
            const string tpName = nameof(InputDieTransferConfig.TeachingPositionName.Pickup);
            if (AxisPickZ == null) return true;
            double target = GetTP(tpName, AxisNames.LeftPickZ);
            try { return AxisPickZ.InPosition(target); } catch { return false; }
        }

        // ---- Ready ----
        public bool IsPositionToolTReady()
        {
            const string tpName = nameof(InputDieTransferConfig.TeachingPositionName.Ready);
            if (AxisToolT == null) return true;
            double target = GetTP(tpName, AxisNames.LeftToolT);
            try { return AxisToolT.InPosition(target); } catch { return false; }
        }

        public bool IsPositionPlaceZReady()
        {
            const string tpName = nameof(InputDieTransferConfig.TeachingPositionName.Ready);
            if (AxisPlaceZ == null) return true;
            double target = GetTP(tpName, AxisNames.LeftPlaceZ);
            try { return AxisPlaceZ.InPosition(target); } catch { return false; }
        }

        // ---- Current Place Index (ToolT / PlaceZ) ----
        public bool IsPositionToolTPlaceIndex()
        {
            int nIndex = GetLoadIndexNo();
            return IsPositionToolTPlaceIndex(nIndex);
        }
        public bool IsPositionPlaceZPlaceIndex()
        {
            int nIndex = GetLoadIndexNo();
            return IsPositionPlaceZPlaceIndex(nIndex);
        }
        // ---- Specific Place Index (0~7 또는 1~8 허용) ----
        public bool IsPositionToolTPlaceIndex(int nIndex)
        {
            if (AxisToolT == null) return true;
            if (!TryGetPlaceTeachingName(nIndex, out string tpName)) return false;

            var tp = Config.GetTeachingPosition(tpName);
            if (tp == null) return false;

            double target = GetTP(tpName, AxisNames.LeftToolT);
            try { return AxisToolT.InPosition(target); } catch { return false; }
        }
        public bool IsPositionPlaceZPlaceIndex(int nIndex)
        {
            if (AxisPlaceZ == null) return true;
            if (!TryGetPlaceTeachingName(nIndex, out string tpName)) return false;

            var tp = Config.GetTeachingPosition(tpName);
            if (tp == null) return false;

            double target = GetTP(tpName, AxisNames.LeftPlaceZ);
            try { return AxisPlaceZ.InPosition(target); } catch { return false; }
        }
        // ---- 내부 공통: Place Index Teaching 이름 변환 (이동 로직과 동일한 인덱스 보정 규칙 유지) ----
        private bool TryGetPlaceTeachingName(int nIndex, out string tpName)
        {
            // OnMovePositionPlace_Index 와 동일 규칙:
            //  1~8 입력  -> +1 (2~9)
            //  0~7 입력  -> +1 (1~8)
            int teachingIdx;
            if (nIndex >= 1 && nIndex <= 8)
                teachingIdx = nIndex + 1;
            else if (nIndex >= 0 && nIndex < 8)
                teachingIdx = nIndex + 1;
            else
            {
                tpName = null;
                return false;
            }

            tpName = $"Place_Index{teachingIdx}";
            return true;
        }
        #endregion

        #region Teaching Helpers
        public int MoveToTeachingPosition(string name, double vel = 0, double acc = 0, double dec = 0, double jerk = 0)
        {
            var tp = Config.GetTeachingPosition(name);
            if (tp == null) return -1;
            var (t, pz, plz) = Config.GetPositionWithOffset(name);

            int rc = 0;

            //Todo : Z축 확인 후 이동 하도록 수정.
            //if (_toolT != null)  rc |= _toolT.MoveAbs(t,   vel > 0 ? vel : _toolT.Config.MaxVelocity,  acc > 0 ? acc : _toolT.Config.RunAcc,  dec > 0 ? dec : _toolT.Config.RunDec,  jerk > 0 ? jerk : _toolT.Config.AccJerkPercent);
            //if (_pickZ != null)  rc |= _pickZ.MoveAbs(pz,  vel > 0 ? vel : _pickZ.Config.MaxVelocity,  acc > 0 ? acc : _pickZ.Config.RunAcc,  dec > 0 ? dec : _pickZ.Config.RunDec,  jerk > 0 ? jerk : _pickZ.Config.AccJerkPercent);
            //if (_placeZ != null) rc |= _placeZ.MoveAbs(plz, vel > 0 ? vel : _placeZ.Config.MaxVelocity, acc > 0 ? acc : _placeZ.Config.RunAcc, dec > 0 ? dec : _placeZ.Config.RunDec, jerk > 0 ? jerk : _placeZ.Config.AccJerkPercent);

            return rc;
        }
        //public bool InPosTeaching(string positionName)
        //{
        //    var tp = Config.GetTeachingPosition(positionName);
        //    if (tp == null) return false;
        //    foreach (var kv in tp.AxisPositions)
        //    {
        //        if (!Axes.TryGetValue(kv.Key, out var axis) || !InPos(axis, kv.Value))
        //            return false;
        //    }
        //    return true;
        //}
       
        public void ApplyOffset(string name, double t, double pickZ, double placeZ)
            => Config.SetOffset(name, t, pickZ, placeZ);
        #endregion

        #region Arm Vacuum / Blow / Vent Control
        private static readonly string[] VAC_NAMES  = { InputDieTransferConfig.IO.ARM1_VAC,  InputDieTransferConfig.IO.ARM2_VAC,  InputDieTransferConfig.IO.ARM3_VAC,  InputDieTransferConfig.IO.ARM4_VAC };
        private static readonly string[] BLOW_NAMES = { InputDieTransferConfig.IO.ARM1_BLOW, InputDieTransferConfig.IO.ARM2_BLOW, InputDieTransferConfig.IO.ARM3_BLOW, InputDieTransferConfig.IO.ARM4_BLOW };
        private static readonly string[] VENT_NAMES = { InputDieTransferConfig.IO.ARM1_VENT, InputDieTransferConfig.IO.ARM2_VENT, InputDieTransferConfig.IO.ARM3_VENT, InputDieTransferConfig.IO.ARM4_VENT };

        private Vacuum[] _vacuum = new Vacuum[4];              // Vacuum + OK sensor
        public Vacuum[] _blow = new Vacuum[4];
        public Vacuum[] _vent = new Vacuum[4];

        private void BindIoDomains()
        {
            var eq = Equipment.Instance; var unit = eq?.UnitIO; if (unit == null) return;

            // Vacuum 별칭으로 조회만
            if (!IoAutoBindings.Vacuums.TryGetValue("InputDieTransferVac1", out _vacuum[0]))
            {
                Log.Write("InputDieTransfer", "BindIoDomains", "Vacuums not found: InputDieTransferVac1");
            }

            if (!IoAutoBindings.Vacuums.TryGetValue("InputDieTransferVac2", out _vacuum[1]))
            {
                Log.Write("InputDieTransfer", "BindIoDomains", "Vacuums not found: InputDieTransferVac2");
            }

            if (!IoAutoBindings.Vacuums.TryGetValue("InputDieTransferVac3", out _vacuum[2]))
            {
                Log.Write("InputDieTransfer", "BindIoDomains", "Vacuums not found: InputDieTransferVac3");
            }

            if (!IoAutoBindings.Vacuums.TryGetValue("InputDieTransferVac4", out _vacuum[3]))
            {
                Log.Write("InputDieTransfer", "BindIoDomains", "Vacuums not found: InputDieTransferVac4");
            }

            if (!IoAutoBindings.Vacuums.TryGetValue("InputDieTransferBlow1", out _blow[0]))
            {
                Log.Write("InputDieTransfer", "BindIoDomains", "Vacuums not found: InputDieTransferBlow1");
            }

            if (!IoAutoBindings.Vacuums.TryGetValue("InputDieTransferBlow2", out _blow[1]))
            {
                Log.Write("InputDieTransfer", "BindIoDomains", "Vacuums not found: InputDieTransferBlow2");
            }

            if (!IoAutoBindings.Vacuums.TryGetValue("InputDieTransferBlow3", out _blow[2]))
            {
                Log.Write("InputDieTransfer", "BindIoDomains", "Vacuums not found: InputDieTransferBlow3");
            }

            if (!IoAutoBindings.Vacuums.TryGetValue("InputDieTransferBlow4", out _blow[3]))
            {
                Log.Write("InputDieTransfer", "BindIoDomains", "Vacuums not found: InputDieTransferBlow4");
            }

            if (!IoAutoBindings.Vacuums.TryGetValue("InputDieTransferVent1", out _vent[0]))
            {
                Log.Write("InputDieTransfer", "BindIoDomains", "Vacuums not found: InputDieTransferVent1");
            }

            if (!IoAutoBindings.Vacuums.TryGetValue("InputDieTransferVent2", out _vent[1]))
            {
                Log.Write("InputDieTransfer", "BindIoDomains", "Vacuums not found: InputDieTransferVent2");
            }

            if (!IoAutoBindings.Vacuums.TryGetValue("InputDieTransferVent3", out _vent[2]))
            {
                Log.Write("InputDieTransfer", "BindIoDomains", "Vacuums not found: InputDieTransferVent3");
            }

            if (!IoAutoBindings.Vacuums.TryGetValue("InputDieTransferVent4", out _vent[3]))
            {
                Log.Write("InputDieTransfer", "BindIoDomains", "Vacuums not found: InputDieTransferVent4");
            }
        }

        // === Domain Control (표준 구동) ===
        public bool SetVacuum(int nNo, bool on, bool bCheckSignal = true)
        {
            if (_vacuum[nNo] == null) 
                return false;

            if(Config.IsSimulation || Config.IsDryRun)
            {
                if (on)
                    _vacuum[nNo].On();
                else
                    _vacuum[nNo].Off();

                return true;
            }

            if (bCheckSignal == false)
            {
                if (on) 
                    _vacuum[nNo].On();
                else 
                    _vacuum[nNo].Off();
            }
            else
            {
                if (on)
                    _vacuum[nNo].OnWaitOk();
                else
                    _vacuum[nNo].OffWaitOk();
            }
            return true;
        }
        public bool SetBlow(int nNo, bool on)
        {
            if (_blow[nNo] == null) return false;
            if (on) _blow[nNo].On();
            else _blow[nNo].Off();
            return true;
        }
        public bool SetVent(int nNo, bool on)
        {
            if (_vent[nNo] == null) return false;
            if (on) _vent[nNo].On();
            else _vent[nNo].Off();
            return true;
        }
        public bool AirTankPressureOk() => this.ReadInput(InputDieTransferConfig.IO.AIR_TANK_PRESSURE);
        public bool VacTankPressureOk() => this.ReadInput(InputDieTransferConfig.IO.VAC_TANK_PRESSURE);
        public bool IsVacuumOK(int armIndex)
        {
            if(Config.IsSimulation || Config.IsDryRun)
            {
                Thread.Sleep(100);
                return true;
            }

            switch (armIndex)
            {
                case 0: return this.ReadInput(InputDieTransferConfig.IO.ARM1_FLOW);
                case 1: return this.ReadInput(InputDieTransferConfig.IO.ARM2_FLOW);
                case 2: return this.ReadInput(InputDieTransferConfig.IO.ARM3_FLOW);
                case 3: return this.ReadInput(InputDieTransferConfig.IO.ARM4_FLOW);
            }
            return false;
        }
        /// //////////////////////////////////////////////////////////////////
        #endregion


        #region Seq Signals
        public bool CompleteInputDie { get; set; } = false;
        public bool CompleteWork { get; internal set; } = false;
        private MaterialDie _currentDie;         // PrepareNextDie에서 보관, CommitPickedDie에서 사용
        #endregion

        #region Lifecycle
        public override int OnRun() 
        {
            int ret = 0;

            if (this.RunUnitStatus == UnitStatus.Stopped ||
                this.RunUnitStatus == UnitStatus.Stopping ||
                this.RunUnitStatus == UnitStatus.CycleStop)
            {
                this.State = ProcessState.Stop;
                return -1;
            }
            
            try
            {
                switch (State)
                {
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
                        this.State = ProcessState.Ready;
                        break;
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                ret = -1;
            }
           

            if (ret != 0)
            {
                this.OnStop();
            }

            return ret;
        }

        protected override int OnStart()
        {
            this.InputStageEjector.Start();

            return base.OnStart();
        }
        public override int OnStop() 
        { 
            int ret = 0;

            this.RunUnitStatus = UnitStatus.Stopped;
            this.State = ProcessState.Stop;

            this.InputStageEjector?.OnStop();

            base.OnStop(); 
            return ret; 
        }


        protected override int OnRunReady()
        {
            int nRtn = 0;

            //Stage 이동 완료 후에.
            MaterialWafer wafer = this.InputStage.GetMaterialWafer();
            if(wafer != null)
            {
                if(wafer.Presence == Material.MaterialPresence.Exist)
                {
                    if(wafer.ProcessSatate == Material.MaterialProcessSatate.Ready
                    || wafer.ProcessSatate == Material.MaterialProcessSatate.Processing)
                    {
                        State = ProcessState.Work;
                    }
                }
            }
            return nRtn;
        }
        protected override int OnRunWork()
        {
            int nRet = 0;

            // PrepareNextDie 안에 있지만 여기서도 방어적으로 웨이퍼/상태 점검.
            // PrepareNextDie 안에서 0으로 리턴되면 여기서도 0으로 리턴되기 때문에 
            // 아래 코드는 사실상 중복이지만 안전을 위해 남겨둠.
            int nArmIndex = GetInputTrArmIndex();
            MaterialWafer wafer = this.InputStage.GetMaterialWafer();
            if (wafer == null)
            {
                Log.Write(UnitName, "[OnRunWork] wafer is null");
                return -1;
            }
            if (wafer.Presence != Material.MaterialPresence.Exist)
            {
                return 0;
            }
            if (wafer.ProcessSatate != Material.MaterialProcessSatate.Processing)
            {
                return 0;
            }

            nRet = PrepareNextDie();
            if (nRet != 0)
            {
                //Die 없음.
                Log.Write(UnitName, "[OnRunWork] PrepareNextDie None.");
                return 0;
            }
            // 아래 코드는 사실상 중복이지만 안전을 위해 남겨둠.
            if (_currentDie == null || _currentDie.Presence != Material.MaterialPresence.Exist)
            {
                return 0;
            }
            if (IsStop) { return 0; }

            //Die를 가지고 있으면 바로 Place를 수행한다.
            var MaterialDie = GetMaterial() as MaterialDie;
            if (MaterialDie == null || MaterialDie.Presence != Material.MaterialPresence.Exist)
            {
                if (IsVacuumOK(nArmIndex) == false
                 || Config.IsSimulation || Config.IsDryRun)
                {
                    nRet = RaiseEjectorForPick();
                    if (nRet != 0)
                    {
                        Log.Write(UnitName, "[OnRunWork] RaiseEjectorForPick failed");
                        return -1;
                    }
                    if (IsStop) { return 0; }

                    nRet = ChipPickDown();
                    if (nRet != 0)
                    {
                        Log.Write(UnitName, "[OnRunWork] ChipPickDown failed");
                        return -1;
                    }
                    if (IsStop) { return 0; }

                    nRet = EjectorVacuumOn();
                    if (nRet != 0)
                    {
                        Log.Write(UnitName, "[OnRunWork] EjectorVacuumOn failed");
                        return -1;
                    }
                    if (IsStop) { return 0; }

                    nRet = SyncPickPinUp();
                    if (nRet != 0)
                    {
                        Log.Write(UnitName, "[OnRunWork] SyncPickPinUp failed");
                        return -1;
                    }
                    nRet = SyncPickPinRetreat();
                    if (nRet != 0)
                    {
                        Log.Write(UnitName, "[OnRunWork] SyncPickPinRetreat failed");
                        return -1;
                    }
                    if (IsStop) { return 0; }

                    nRet = CommitPickedDie();
                    if (nRet != 0)
                    {
                        Log.Write(UnitName, "[OnRunWork] CommitPickedDie failed");
                        return -1;
                    }
                    if (IsStop) { return 0; }
                }
            }

            nRet = RotateToolTForPlace_AsyncWait();
            if (nRet != 0)
            {
                Log.Write(UnitName, "[OnRunWork] RotateToolTForPlace_AsyncWait failed");
                return -1;
            }
            if (IsStop) { return 0; }

            State = ProcessState.Complete;
            return nRet;
        }

        

        protected override int OnRunComplete()
        {
            int nRet = 0;

            if(!Rotary.RequestInputDieTrDie)
            {
                return nRet;
            }

            // Rotary에서 Place 위치 도착 신호 오면 수행.
            MaterialDie Die = this.Rotary.GetLoadSocketMaterial();
            if(Die != null)
            {
                if (Die.Presence == Material.MaterialPresence.NotExist
                 || Die.Presence == Material.MaterialPresence.Unknown)
                {
                    if (Die.ProcessSatate == Material.MaterialProcessSatate.Unknown &&
                        Rotary.IsAnyAxisMoving() == false)
                    {
                        nRet = PlaceChipDown();
                        if (nRet != 0)
                        {
                            return -1;
                        }
                        nRet = ReleaseVacuumAndPlaceUp();
                        if (nRet != 0)
                        {
                            return -1;
                        }

                        //this.MoveMaterial(new MaterialDie(), this.Rotary);
                        // Rotary에 Die 정보 전달.
                        Material material = this.GetMaterial();
                        MaterialDie die = material as MaterialDie;
                        if (die == null)
                        {
                            Log.Write(UnitName, "[OnRunComplete] die is null");
                            return -1;
                        }

                        die.State = DieProcessState.Inspecting;
                        die.ProcessSatate = Material.MaterialProcessSatate.Processing;
                        die.Presence = Material.MaterialPresence.Exist;
                        Rotary.SetMaterial(die);
                        SetMaterial(new Material());
                        _currentDie = null;

                        State = ProcessState.None;
                    }
                }
            }

            return 0;
        }

        #endregion
        #region Sequence 등록

        protected override void OnMakeSequence()
        {
            base.OnMakeSequence();
            // Pick Phase
            this.SequencePlayers.Add(PrepareNextDie);
            this.SequencePlayers.Add(RaiseEjectorForPick);
            this.SequencePlayers.Add(ChipPickDown);
            this.SequencePlayers.Add(EjectorVacuumOn);
            this.SequencePlayers.Add(SyncPickPinUp);
            this.SequencePlayers.Add(SyncPickPinRetreat);
            this.SequencePlayers.Add(CommitPickedDie);
            this.SequencePlayers.Add(RotateToolTForPlace_AsyncWait);
            this.SequencePlayers.Add(PlaceChipDown);
            this.SequencePlayers.Add(ReleaseVacuumAndPlaceUp);
        }

        #endregion
        #region Seq 단위 동작 함수
        public int PrepareNextDie(bool bFineSpeed = false)
        {
            int nRet = 0;
            if (RunMode == UnitRunMode.Manual)
            {
                this.CurrentFunc = PrepareNextDie;

                var mb = new MessageBoxOk();
                mb.Focus();
                mb.ShowDialog("알림", "웨이퍼 스테이지 이동 후 진행 바랍니다.");
                return 0;
            }

            if(RunMode == UnitRunMode.Auto)
            {
                // 웨이퍼/상태 점검은 여기서도 방어적으로 수행
                var wafer = this.InputStage?.GetMaterialWafer();
                if (wafer == null)
                {
                    Log.Write(UnitName, "[PrepareNextDie] wafer is null");
                    return -1;
                }
                if (wafer.Presence != Material.MaterialPresence.Exist)
                {
                    // 웨이퍼가 없으면 이번 사이클을 스킵
                    return 0;
                }
                if (wafer.ProcessSatate != Material.MaterialProcessSatate.Processing)
                {
                    // 가공 상태가 아니면 스킵
                    return 0;
                }

                MaterialDie die;
                nRet = MoveStageToNextDie(out die);
                if (nRet != 0)
                {
                    Log.Write(UnitName, "[PrepareNextDie] MoveStageToNextDie failed");
                    return -1;
                }

                if (die == null || die.Presence != Material.MaterialPresence.Exist)
                {
                    // 더 이상 픽할 다이가 없으면 우아하게 스킵
                    return 0;
                }

                _currentDie = die;
            }

            return nRet;
        }
        private int RecheckDieAndAlign(bool bFineSpeed = false)
        {
            int nRet = 0;
            if (RunMode == UnitRunMode.Manual)
            {
                this.CurrentFunc = RecheckDieAndAlign;
                var mb = new MessageBoxOk();
                mb.Focus();
                mb.ShowDialog("알림", "웨이퍼 스테이지 이동 후 진행 바랍니다.");
                return 0;
            }
            if (InputStage == null)
            {
                Log.Write(UnitName, "[RecheckDieAndAlign] InputStage is null");
                return -1;
            }
            MaterialWafer wafer = this.InputStage.GetMaterialWafer();
            if (wafer == null)
            {
                Log.Write(UnitName, "[RecheckDieAndAlign] wafer is null");
                return -1;
            }
            if (wafer.Presence != Material.MaterialPresence.Exist)
            {
                Log.Write(UnitName, "[RecheckDieAndAlign] wafer is not exist");
                return -1;
            }
            if (wafer.ProcessSatate != Material.MaterialProcessSatate.Processing)
            {
                Log.Write(UnitName, "[RecheckDieAndAlign] wafer is not processing state");
                return -1;
            }
            nRet = InputStage.RecheckDieAndAlign(bFineSpeed);
            if (nRet != 0)
            {
                Log.Write(UnitName, "[RecheckDieAndAlign] InputStage.RecheckDieAndAlign failed");
                return -1;
            }
            return nRet;
        }

        public int RaiseEjectorForPick(bool bFineSpeed = false)
        {
            int nRet = 0;
            if (RunMode == UnitRunMode.Manual)
            {
                this.CurrentFunc = RaiseEjectorForPick;

            }
            if (InputStageEjector == null)
            {
                Log.Write(UnitName, "[RaiseEjectorForPick] InputStageEjector is null");
                return -1;
            }

            int blockUpResult = InputStageEjector.MovePositionEjectBlockUp(bFineSpeed);
            if (blockUpResult != 0)
            {
                Log.Write(UnitName, "[RaiseEjectorForPick] EjectBlockUp 이동 실패");
                return -1;
            }

            int pinReadyResult = InputStageEjector.MovePositionEjectPinReady(bFineSpeed);
            if (pinReadyResult != 0)
            {
                Log.Write(UnitName, "[RaiseEjectorForPick] EjectPinReady 이동 실패");
                return -1;
            }

            return nRet;
        }
        public int ChipPickDown(bool bFineSpeed = false)
        {
            int nRet = 0;
            if (RunMode == UnitRunMode.Manual)
            {
                this.CurrentFunc = ChipPickDown;
            }

            int nArmIndex = GetInputTrArmIndex();
            nRet = MovePositionPickUp(bFineSpeed);
            if (nRet != 0)
            {
                Log.Write(UnitName, "[ChipPickDown] MovePositionPickUp failed");
                return -1;
            }
           
            SetVacuum(nArmIndex, true);
            if(IsVacuumOK(nArmIndex) == false)
            {
                PostAlarm((int)AlarmKeys.eInputDieTransferVaccum);
                Log.Write(UnitName, "[ChipPickDown] Vacuum not OK");
                return -1;
            }
            //if (SetVacuum(nArmIndex, true))
            //{
            //    var sw = Stopwatch.StartNew();
            //    while (!IsVacuumOK(nArmIndex))
            //    {
            //        if (!Config.IsSimulation && !Config.IsDryRun)
            //        {
            //            if (sw.ElapsedMilliseconds > 2000)
            //            {
            //                PostAlarm((int)AlarmKeys.eInputDieTransferVaccum);
            //                Log.Write(UnitName, "[DieTrVacuumOn] Vacuum Timeout");
            //                return -1;
            //            }
            //            Thread.Sleep(1);
            //        }
            //        else
            //        {
            //            break;
            //        }
            //    }
            //}

            return nRet;
        }
        public int EjectorVacuumOn(bool bFineSpeed = true)
        {
            if (InputStageEjector == null)
                return -1;

            if (RunMode == UnitRunMode.Manual)
            {
                this.CurrentFunc = EjectorVacuumOn;
            }
            int nRet = 0;

            InputStage.SetVacuum(true, true);
            if (InputStage.IsVacuumOn() == false)
            {
                PostAlarm((int)AlarmKeys.eInputStageVaccum);
                Log.Write(UnitName, "[EjectorVacuumOn] Vacuum Timeout");
                return -1;
            }

            //if (InputStage.SetVacuum(true))
            //{
            //    var sw = Stopwatch.StartNew();
            //    while (true)
            //    {
            //        if(InputStage.IsVacuumOn())
            //        {
            //            break;
            //        }
            //        else if (!Config.IsSimulation && !Config.IsDryRun)
            //        {
            //            if (sw.ElapsedMilliseconds > 2000)
            //            {
            //                PostAlarm((int)AlarmKeys.eInputStageVaccum);
            //                Log.Write(UnitName, "[EjectorVacuumOn] Vacuum Timeout");
            //                return -1;
            //            }
            //            Thread.Sleep(1);
            //        }
            //        else
            //        {
            //            break;
            //        }
            //    }
            //}
            //else
            //{
            //    Log.Write(UnitName, "[EjectorVacuumOn] SetVacuum(true) failed");
            //    return -1;
            //}

            return nRet;
        }
        public int SyncPickPinUp(bool bFineSpeed = false)
        {
            if (InputStageEjector == null)
            {
                PostAlarm((int)AlarmKeys.eInputStageNotSafe);
                return -1;
            }

            if (RunMode == UnitRunMode.Manual)
            {
                this.CurrentFunc = SyncPickPinUp;

            }
            int nRet = 0;

            double pickZOffset = InputStageEjector.Config.dPickUpOffset;
            double pinZOffset = Config.dPickUpOffset;
            double velPinZ = InputStageEjector.Config.dPickUpSpeed;
            double velPickZ = velPinZ; // 필요 시 예: (InputDieTransferUnit.AxisPickZ.Config.MaxVelocity * 0.8);
            double acc = InputStageEjector.Config.dPickUpAcc;
            double dec = InputStageEjector.Config.dPickUpAcc;
            int timeoutMs = 20000;   // 필요 시 예: 5000;

            nRet = MovePickZAndPinZByOffset(
                    pickZOffset,
                    pinZOffset,
                    velPickZ,
                    velPinZ,
                    acc,
                    dec,
                    timeoutMs,
                    bFineSpeed);

            if (nRet != 0)
            {
                Log.Write(UnitName, "[SyncPickPinDown] MovePickZAndPinZByOffset failed");
                return -1;
            }

            return nRet;
        }
        public int SyncPickPinRetreat(bool bFineSpeed = false)
        {
            int nRet = 0;

            if (InputStageEjector == null)
            {
                PostAlarm((int)AlarmKeys.eInputStageNotSafe);
                return -1;
            }

            if (RunMode == UnitRunMode.Manual)
            {
                this.CurrentFunc = SyncPickPinRetreat;
            }

            // Release
            InputStage.SetVacuum(false, true);
            if (Config.IsSimulation || Config.IsDryRun)
            {
                Thread.Sleep(100);
            }
            else if (InputStage.IsVacuumOn() == true)
            {
                PostAlarm((int)AlarmKeys.eInputStageVaccum);
                Log.Write(UnitName, "[SyncPickPinRetreat] Vacuum Release Timeout");
                return -1;
            }
            //if (InputStage.SetVacuum(false))
            //{
            //    if(Config.IsSimulation || Config.IsDryRun)
            //    {
            //        Thread.Sleep(100);
            //    }
            //    else
            //    {
            //        var sw = Stopwatch.StartNew();
            //        while (InputStage.IsVacuumOn())
            //        {
            //            if (sw.ElapsedMilliseconds > 1000)
            //            {
            //                PostAlarm((int)AlarmKeys.eInputStageVaccum);
            //                Log.Write(UnitName, "[SyncPickPinRetreat] Vacuum Release Timeout");
            //                return -1;
            //            }
            //            Thread.Sleep(1);
            //        }
            //    }
            //}

            this.WaitByTime(Config.nPickUpWaitTime, 1);

            // PickZ Safety 이동
            double dZPos = GetTP(InputDieTransferConfig.TeachingPositionName.SafetyZone.ToString(),
                                 AxisNames.LeftPickZ);
            nRet = MoveAxisPositionOne(AxisPickZ, dZPos, bFineSpeed);
            if (nRet != 0)
            {
                AxisToolT.EmgStop();
                AxisPickZ.EmgStop();
                AxisPlaceZ.EmgStop();
                PostAlarm((int)AlarmKeys.eInputDieTransferError);
                Log.Write(UnitName, "[SyncPickPinRetreat] AxisPickZ SafetyZone 이동 실패");
                return -1;
            }
            // Ejector Pin Ready
            nRet = InputStageEjector.MovePositionEjectPinReady(bFineSpeed);
            if (nRet != 0)
            {
                AxisToolT.EmgStop();
                AxisPickZ.EmgStop();
                AxisPlaceZ.EmgStop();
                PostAlarm((int)AlarmKeys.eInputDieTransferError);
                Log.Write(UnitName, "[SyncPickPinRetreat] EjectPinReady 이동 실패");
                return -1;
            }
            // Ejector Block Ready
            nRet = InputStageEjector.MovePositionEjectBlockReady(bFineSpeed);
            if (nRet != 0)
            {
                AxisToolT.EmgStop();
                AxisPickZ.EmgStop();
                AxisPlaceZ.EmgStop();
                PostAlarm((int)AlarmKeys.eInputDieTransferError);
                Log.Write(UnitName, "[SyncPickPinRetreat] EjectBlockReady 이동 실패");
                return -1;
            }
            //double dZPos = GetTP(InputDieTransferConfig.TeachingPositionName.SafetyZone.ToString(),
            //            AxisNames.LeftPickZ);
            //nRet &= MoveAxisPositionOne(AxisPickZ, dZPos, bFineSpeed);
            //nRet &= InputStageEjector.MovePositionEjectPinReady(bFineSpeed);
            //nRet &= InputStageEjector.MovePositionEjectBlockReady(bFineSpeed);

            //if (nRet != 0) //nRet = Move
            //{
            //    AxisToolT.EmgStop();
            //    AxisPickZ.EmgStop();
            //    AxisPlaceZ.EmgStop();
            //    PostAlarm((int)AlarmKeys.eInputDieTransferError);
            //    Log.Write(UnitName, "[SyncPickPinRetreat] AxisPickZ SafetyZone 이동 실패");
            //    Log.Write(UnitName, "[SyncPickPinRetreat] EjectBlockReady 이동 실패");
            //    Log.Write(UnitName, "[SyncPickPinRetreat] EjectPinReady 이동 실패");
            //    return -1;
            //}

            return nRet;
        }
        public int CommitPickedDie(bool bFineSpeed = false)
        {
            if (RunMode == UnitRunMode.Manual)
            {
                this.CurrentFunc = CommitPickedDie;
            }

            if (RunMode == UnitRunMode.Auto)
            {
                if (_currentDie == null || _currentDie.Presence != Material.MaterialPresence.Exist)
                {
                    return -1;  // 이 경우는 에러로 간주
                }
                _currentDie.State = DieProcessState.Picked;
                _currentDie.ProcessSatate = Material.MaterialProcessSatate.Processing;
                SetMaterial(_currentDie); // 이후 Complete 단계에서 Rotary로 전달
            }
               
            return 0;
        }
        public int RotateToolTForPlace_AsyncWait(bool bFineSpeed = false)
        {
            int nRet = 0;
            if (RunMode == UnitRunMode.Manual)
            {
                this.CurrentFunc = RotateToolTForPlace_AsyncWait;
                //nRet = RotateToolTForPlace(bFineSpeed);
                //if(nRet != 0)
                //{
                //    Log.Write(UnitName, "[RotateToolTForPlace_AsyncWait] RotateToolTForPlace failed");
                //    return -1;
                //}
                //return nRet;
            }

            Task<int> t = Task.Factory.StartNew(() =>
            {
                return RotateToolTForPlace(bFineSpeed);
            });

            while (t.IsCompleted == false)
            {
                // 진행 중 모니터링(필요 시 비전/로그 등)
                double dPos = AxisToolT.GetPosition();
                // TODO: 옵션에 따라 사진 촬영/좌표 업데이트 등 처리
                
                
                // 여기서 Die 유/무 재확인 및 Image Grab 후 2차 얼라인 수행.
                // InputStage class의 RecheckDieAndAlign 함수 불러와서 수행.
                nRet = RecheckDieAndAlign();
                if (nRet != 0)
                {
                    Log.Write(UnitName, "[OnRunWork] RecheckDieAndAlign failed");
                    return -1;
                }


                Thread.Sleep(1);
            }

            t.Wait();
            nRet = t.Result;
            if (nRet != 0)
            {
                Log.Write(UnitName, "[RotateToolTForPlace_AsyncWait] RotateToolTForPlace failed");
                return -1;
            }
            return nRet;
        }
        
        public int PlaceChipDown(bool bFineSpeed = false)
        {
            if (RunMode == UnitRunMode.Manual)
            {
                this.CurrentFunc = PlaceChipDown;
            }
            int nRet = 0;
            int armIndex = GetInputTrArmIndex();
            int nIndex = GetLoadIndexNo();

            // nIndex 처리 (0-based와 1-based 모두 지원)
            //  - 1~8 : 그대로 사용 (Place_Index1 ~ Place_Index8)
            //  - 0~7 : +1 보정하여 1~8 매핑
            int teachingIdx = 0;
            if (nIndex >= 1 && nIndex <= 8)
                teachingIdx = nIndex + 1;
            else if (nIndex >= 0 && nIndex < 8)
                teachingIdx = nIndex + 1; // 0-based 입력으로 판단
            else
            {
                Log.Write(UnitName, $"[PlaceChipDown] Invalid index {nIndex}. Range 0~7 or 1~8");
                return -1;
            }

            string tpName = $"Place_Index{teachingIdx}";
            var tpObj = Config.GetTeachingPosition(tpName);
            if (tpObj == null)
            {
                Log.Write(UnitName, $"[PlaceChipDown] Teaching not found: {tpName}");
                return -1;
            }

            double dTPos = GetTP(tpName, AxisNames.LeftPlaceZ);
            nRet = MoveAxisPositionOne(AxisPlaceZ, dTPos, bFineSpeed);
            if (nRet != 0)
            {
                Log.Write(UnitName, "[RotateToolTForPlace] ToolT Place 이동 실패");
                return -1;
            }

            Rotary.SetVacuum(nIndex, true);

            //todo: 2025-10-10 GYN: 나중에 열어
            //if(Rotary.IsVacuumOk(nIndex) == false)
            //{
            //    PostAlarm((int)AlarmKeys.eInputStageVaccum);
            //    Log.Write(UnitName, "[PlaceChipDown] Rotary Vacuum not OK");
            //    return -1;
            //}
            SetVacuum(armIndex, false);
            SetVent(armIndex, true);
            SetBlow(armIndex, true);

            if (Config.IsSimulation || Config.IsDryRun)
            {
                Thread.Sleep(100);
            }

            //todo: 2025-10-10 GYN: 나중에 열어
            //else if (IsVacuumOK(armIndex))
            //{
            //    PostAlarm((int)AlarmKeys.eInputDieTransferVaccum);
            //    Log.Write(UnitName, "[PlaceChipDown] Arm Vacuum not released");
            //    return -1;
            //}

            return nRet;

        }
        public int ReleaseVacuumAndPlaceUp(bool bFindSpeed = false)
        {
            int nRet = 0;
            try
            {
                if (RunMode == UnitRunMode.Manual)
                {
                    this.CurrentFunc = ReleaseVacuumAndPlaceUp;
                    LogSequence("Start");
                }
                int armIndex = GetInputTrArmIndex();
                int nIndex = GetLoadIndexNo();

                if (armIndex < 0 || armIndex > 3)
                    return -1;

                if (Rotary.SetVacuum(nIndex, true))
                {
                    SetVacuum(armIndex, false);
                    Thread.Sleep(1);
                    SetVent(armIndex, true);
                    Thread.Sleep(1);
                    SetBlow(armIndex, true);

                    var sw = Stopwatch.StartNew();
                    while (true)
                    {
                        break;   

                        if (Rotary.IsVacuumOk(nIndex))
                        {
                            break;
                        }
                        else if (!Config.IsSimulation && !Config.IsDryRun)
                        {
                            if (sw.ElapsedMilliseconds > 5000)
                            {
                                PostAlarm((int)AlarmKeys.eInputStageVaccum);
                                Log.Write(UnitName, "[SyncPickPinRetreat] Vacuum Release Timeout");
                                return -1;
                            }
                            Thread.Sleep(1);
                        }
                        else
                        {
                            break;
                        }
                    }
                }

                this.WaitByTime(Config.nPlaceUpWaitTime, 1);

                // Safety 위치로 상승
                double dZPos = GetTP(InputDieTransferConfig.TeachingPositionName.SafetyZone.ToString(),
                            AxisNames.LeftPlaceZ);
                nRet = MoveAxisPositionOne(AxisPlaceZ, dZPos, bFindSpeed);
                if (nRet != 0)
                {
                    Log.Write(UnitName, "[ReleaseVacuumAndPlaceUp] AxisPlaceZ SafetyZone 이동 실패");
                    return -1;
                }

                Thread.Sleep(1);
                SetVent(armIndex, false);
                Thread.Sleep(1);
                SetBlow(armIndex, false);

                if (IsVacuumOK(armIndex))
                {
                    PostAlarm((int)AlarmKeys.eInputDieTransferVaccum);
                    Log.Write(UnitName, "[PlaceChipDown] Arm Vacuum not released");
                    return -1;
                }

                nRet = MovePositionPickUpToolT();
                if (nRet != 0)
                {
                    Log.Write(UnitName, "[ReleaseVacuumAndPlaceUp] MovePositionPickUpToolT failed");
                    return -1;
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                nRet = -1;
                PostAlarm((int)AlarmKeys.eInputDieTransferError);

            }
            finally
            {
                LogSequence("End");
            }

            return nRet; 
        }

        private void LogSequence(string log)
        {
            if (RunMode == UnitRunMode.Manual)
            {
                if (this.CurrentFunc == null)
                    return;

                Log.Write(UnitName, this.CurrentFunc.Method.Name, $"[Sequence] {log}");
            }
        }
        public int MoveStageToNextDie(out MaterialDie die)
        {
            if (InputStage == null)
            {
                die = null;
                return -1;
            }

            int nRet = 0;
            nRet = this.InputStage.MoveStageToNextDie(out die);
            return nRet;
        }
        public int RotateToolTForPlace(bool bFineSpeed = false)
        {
            if (AxisToolT == null)
                return -1;

            int nRet = 0;
            //if (RunMode == UnitRunMode.Manual)
            //{
            //    this.CurrentFunc = RotateToolTForPlace;
            //}
            int nIndex = GetLoadIndexNo();

            // nIndex 처리 (0-based와 1-based 모두 지원)
            //  - 1~8 : 그대로 사용 (Place_Index1 ~ Place_Index8)
            //  - 0~7 : +1 보정하여 1~8 매핑
            int teachingIdx = 0;
            if (nIndex >= 1 && nIndex <= 8)
                teachingIdx = nIndex + 1;
            else if (nIndex >= 0 && nIndex < 8)
                teachingIdx = nIndex + 1; // 0-based 입력으로 판단
            else
            {
                Log.Write(UnitName, $"[RotateToolTForPlace] Invalid index {nIndex}. Range 0~7 or 1~8");
                return -1;
            }

            string tpName = $"Place_Index{teachingIdx}";
            var tpObj = Config.GetTeachingPosition(tpName);
            if (tpObj == null)
            {
                Log.Write(UnitName, $"[RotateToolTForPlace] Teaching not found: {tpName}");
                return -1;
            }

            double dTPos = GetTP(tpName, AxisNames.LeftToolT);
            nRet = MoveAxisPositionOne(AxisToolT, dTPos, bFineSpeed);
            if (nRet != 0)
            {
                Log.Write(UnitName, "[RotateToolTForPlace] ToolT Place 이동 실패");
                return -1;
            }

            return nRet;

            //double dTPos = GetTP(InputDieTransferConfig.TeachingPositionName.Place_Index1.ToString(),
            //                    AxisNames.LeftToolT);
            //nRet = MoveAxisPositionOne(AxisToolT, dTPos, bFineSpeed);
            //if (nRet != 0)
            //{
            //    Log.Write(UnitName, "[RotateToolTForPlace] ToolT Place 이동 실패");
            //    return -1;
            //}

            //isWork = true;

            //return nRet;
        }
        public int GetLoadIndexNo()
        {
            int nIndex = 0;
            if (Rotary == null) return nIndex;
            nIndex = (Rotary.GetLoadIndexNo() + this.Config.IndexOfStart) % Rotary.GetIndexCount();
            return nIndex;
        }
        public int GetInputTrArmIndex()
        {
            //todo: 구현해라 구부장. 암 하나 더달면. Rotary Index에 따른 Arm Index 반환

            //if(this.AxisToolT.GetPosition() > 10)
            //{

            //}
            return 0;
        }
        public bool IsInterlockOKWidthRotary()
        {
            double dPos = this.AxisPlaceZ.GetPosition();
            double tp = this.GetTP(InputDieTransferConfig.TeachingPositionName.Ready.ToString(),
                        AxisNames.LeftPlaceZ);
            bool bResult = false;
            if (dPos <= (tp + 0.007))
            {
                bResult = true;
            }
            return bResult;
        }
        #endregion

        #region Ready
        public int EnsureReady(bool isFine = false)
        {
            Task<int> task = EnsureReadyAsync(isFine);
            while (IsEndTask(task) == false)
            {
                Thread.Sleep(1);
            }
            return task.Result;
        }
        private Task<int> EnsureReadyAsync(bool isFine = false)
        {
            return Task.Run(() =>
            {
                OnEnsureReady(isFine);
                return 0;
            });
        }
        private int OnEnsureReady(bool isFine)
        {
            int nRet = 0;

            if (IsPositionPickZSafety() == false
             || IsPositionPlaceZSafety() == false)
            {
                nRet = MovePositionSafetyZ();
                if (nRet != 0)
                {
                    Log.Write(this, "CheckReady Fail - MovePositionSafetyZ");
                    return nRet;
                }
            }

            return nRet;
        }
        #endregion
    }
}