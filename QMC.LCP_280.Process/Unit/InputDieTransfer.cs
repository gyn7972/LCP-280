using QMC.Common;
using QMC.Common.Alarm;
using QMC.Common.Component;
using QMC.Common.IOUtil;
using QMC.Common.Motion;
using QMC.Common.Motions;
using QMC.Common.Unit;
using QMC.LCP_280.Process.Component;
using QMC.LCP_280.Process.Work;
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
using static System.Windows.Forms.AxHost;

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
        public class DiePickedEventArgs : EventArgs
        {
            public double MapX { get; }
            public double MapY { get; }
            public DieProcessState State { get; set; }
            public MaterialDie Die { get; }

            public DiePickedEventArgs(MaterialDie die)
            {
                Die = die;
                if (die != null)
                {
                    State = die.State;
                    MapX = die.MapX;
                    MapY = die.MapY;
                }
            }
        }

        public event EventHandler<DiePickedEventArgs> DiePicked;

        public Task<int> taskPrepareNextDie = null;
        public enum AlarmKeys
        {
            eInputStageNotSafety = 4001,
            eRotatyNotSafety,
            eInputStageEjectorPinZNotSafety,
            eInputStageEjectorZNotSafety,
            eInputStageAxesMoving = 4010,
            eRotaryAxesMoving,
            eInputStageEjectorAxesMoving,
            eInputDieTransferError,
            eInputStageVaccum,
            eInputDieTransferVaccum,
            eInputDieTransferNotSafety,
            eInputDieTransferMoveFail,
            eInputDieTransferNoWafer,
            eInputDieTransferPrepareNextDie,
            eInputDieTransferRaiseEjector,
            eInputDieTransferChipPickDown,
            eInputDieTransferSyncPickPinUp,
            eInputDieTransferSyncPickPinRetreat,
            eInputDieTransferCommitPickedDie,
            eInputDieTransferRotateToolTForPlace,
            eInputDieTransferPlaceChipDown,
            eInputDieTransferReleaseVacuumAndPlaceUp,
            eInputDieTransferNoDie,
        }
        #region InitAlarm
        protected override void InitAlarm()
        {
            base.InitAlarm();
            AlarmInfo alarm = new AlarmInfo();
            alarm.Code = (int)AlarmKeys.eInputStageNotSafety;
            alarm.Title = "InputStage Not safety Pos.";
            alarm.Cause = "InputStage가 안전 위치가 아닙니다. 포지션 확인 후 다시 시작 하십시요.";
            alarm.Source = this.UnitName;
            alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
            m_dicAlarms.Add(alarm.Code, alarm);

            alarm = new AlarmInfo();
            alarm.Code = (int)AlarmKeys.eRotatyNotSafety;
            alarm.Title = "Rotaty Not safety Pos.";
            alarm.Cause = "Rotaty가 안전 위치가 아닙니다. 포지션 확인 후 다시 시작 하십시요.";
            alarm.Source = this.UnitName;
            alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
            m_dicAlarms.Add(alarm.Code, alarm);

            //,
            alarm = new AlarmInfo();
            alarm.Code = (int)AlarmKeys.eInputStageEjectorPinZNotSafety;
            alarm.Title = "EjectorPin Z-Axis Not safety Pos.";
            alarm.Cause = "EjectorPin Z-Axis가 안전 위치가 아닙니다. 포지션 확인 후 다시 시작 하십시요.";
            alarm.Source = this.UnitName;
            alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
            m_dicAlarms.Add(alarm.Code, alarm);
            //,
            alarm = new AlarmInfo();
            alarm.Code = (int)AlarmKeys.eInputStageEjectorZNotSafety;
            alarm.Title = "Ejector Z-Axis Not safety Pos.";
            alarm.Cause = "Ejector Z-Axis가 안전 위치가 아닙니다. 포지션 확인 후 다시 시작 하십시요.";
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
            alarm.Title = "InputDieTransfer Not safety Pos.";
            alarm.Cause = "InputDieTransfer가 안전 위치가 아닙니다. 포지션 확인 후 다시 시작 하십시요.";
            alarm.Source = this.UnitName;
            alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
            m_dicAlarms.Add(alarm.Code, alarm);

            alarm = new AlarmInfo();
            alarm.Code = (int)AlarmKeys.eInputDieTransferMoveFail;
            alarm.Title = "InputDieTransfer Move Fail";
            alarm.Cause = "InputDieTransfer가 명령한 위치로 이동하지 못했습니다. 포지션 확인 후 다시 시작 하십시요.";
            alarm.Source = this.UnitName;
            alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
            m_dicAlarms.Add(alarm.Code, alarm);

            alarm = new AlarmInfo();
            alarm.Code = (int)AlarmKeys.eInputDieTransferNoWafer;
            alarm.Title = "InputDieTransfer No Wafer Detected";
            alarm.Cause = "InputDieTransfer가 Wafer를 감지하지 못했습니다. 포지션 확인 후 다시 시작 하십시요.";
            alarm.Source = this.UnitName;
            alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
            m_dicAlarms.Add(alarm.Code, alarm);

            alarm = new AlarmInfo();
            alarm.Code = (int)AlarmKeys.eInputDieTransferPrepareNextDie;
            alarm.Title = "InputDieTransfer Prepare Next Die";
            alarm.Cause = "InputDieTransfer가 다음 Die 준비 중에 문제가 발생했습니다. 포지션 확인 후 다시 시작 하십시요.";
            alarm.Source = this.UnitName;
            alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
            m_dicAlarms.Add(alarm.Code, alarm);

            alarm = new AlarmInfo();
            alarm.Code = (int)AlarmKeys.eInputDieTransferRaiseEjector;
            alarm.Title = "InputDieTransfer Raise Ejector Fail";
            alarm.Cause = "InputDieTransfer가 Ejector를 올리는 중에 문제가 발생했습니다. 포지션 확인 후 다시 시작 하십시요.";
            alarm.Source = this.UnitName;
            alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
            m_dicAlarms.Add(alarm.Code, alarm);

            alarm = new AlarmInfo();
            alarm.Code = (int)AlarmKeys.eInputDieTransferChipPickDown;
            alarm.Title = "InputDieTransfer Chip Pick Down Fail";
            alarm.Cause = "InputDieTransfer가 Chip을 Pick Down하는 중에 문제가 발생했습니다. 포지션 확인 후 다시 시작 하십시요.";
            alarm.Source = this.UnitName;
            alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
            m_dicAlarms.Add(alarm.Code, alarm);

            alarm = new AlarmInfo();
            alarm.Code = (int)AlarmKeys.eInputDieTransferSyncPickPinUp;
            alarm.Title = "InputDieTransfer Sync Pick Pin Up Fail";
            alarm.Cause = "InputDieTransfer가 Pick Pin을 올리는 중에 문제가 발생했습니다. 포지션 확인 후 다시 시작 하십시요.";
            alarm.Source = this.UnitName;
            alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
            m_dicAlarms.Add(alarm.Code, alarm);

            alarm = new AlarmInfo();
            alarm.Code = (int)AlarmKeys.eInputDieTransferSyncPickPinRetreat;
            alarm.Title = "InputDieTransfer Sync Pick Pin Retreat Fail";
            alarm.Cause = "InputDieTransfer가 Pick Pin을 후퇴시키는 중에 문제가 발생했습니다. 포지션 확인 후 다시 시작 하십시요.";
            alarm.Source = this.UnitName;
            alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
            m_dicAlarms.Add(alarm.Code, alarm);

            alarm = new AlarmInfo();
            alarm.Code = (int)AlarmKeys.eInputDieTransferCommitPickedDie;
            alarm.Title = "InputDieTransfer Commit Picked Die Fail";
            alarm.Cause = "InputDieTransfer가 Picked Die를 커밋하는 중에 문제가 발생했습니다. 포지션 확인 후 다시 시작 하십시요.";
            alarm.Source = this.UnitName;
            alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
            m_dicAlarms.Add(alarm.Code, alarm);

            alarm = new AlarmInfo();
            alarm.Code = (int)AlarmKeys.eInputDieTransferRotateToolTForPlace;
            alarm.Title = "InputDieTransfer Rotate Tool T For Place Fail";
            alarm.Cause = "InputDieTransfer가 Place를 위해 Tool T를 회전하는 중에 문제가 발생했습니다. 포지션 확인 후 다시 시작 하십시요.";
            alarm.Source = this.UnitName;
            alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
            m_dicAlarms.Add(alarm.Code, alarm);

            alarm = new AlarmInfo();
            alarm.Code = (int)AlarmKeys.eInputDieTransferPlaceChipDown;
            alarm.Title = "InputDieTransfer Place Chip Down Fail";
            alarm.Cause = "InputDieTransfer가 Chip을 Place Down하는 중에 문제가 발생했습니다. 포지션 확인 후 다시 시작 하십시요.";
            alarm.Source = this.UnitName;
            alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
            m_dicAlarms.Add(alarm.Code, alarm);

            alarm = new AlarmInfo();
            alarm.Code = (int)AlarmKeys.eInputDieTransferReleaseVacuumAndPlaceUp;
            alarm.Title = "InputDieTransfer Release Vacuum And Place Up Fail";
            alarm.Cause = "InputDieTransfer가 Vacuum을 해제하고 Place Up하는 중에 문제가 발생했습니다. 포지션 확인 후 다시 시작 하십시요.";
            alarm.Source = this.UnitName;
            alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
            m_dicAlarms.Add(alarm.Code, alarm);

            alarm = new AlarmInfo();
            alarm.Code = (int)AlarmKeys.eInputDieTransferNoDie;
            alarm.Title = "InputDieTransfer No Die Detected";
            alarm.Cause = "InputDieTransfer가 Die를 감지하지 못했습니다. 포지션 확인 후 다시 시작 하십시요.";
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
        // Safety 동작 중 여부
        private bool _isSafetyMoving = false;

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
        public override bool IsInterlockOK(BaseComponent baseComponent, BaseComponent.InterlockEventArgs e)
        {
            bool bRet = base.IsInterlockOK(baseComponent, e);
            int nRet = 0;
            if (baseComponent == this.AxisPickZ)
            {
                if (_isSafetyMoving)
                    return true;
                if (this.InputStage.IsAxisMoving(AxisNames.WaferStageX) ||
                this.InputStage.IsAxisMoving(AxisNames.WaferStageY) ||
                this.InputStage.IsAxisMoving(AxisNames.WaferStageT))
                {
                    AxisPickZ?.EmgStop();
                    PostAlarm((int)AlarmKeys.eInputStageAxesMoving);
                    return false;
                }
            }
            else if (baseComponent == this.AxisPlaceZ)
            {
                if (_isSafetyMoving)
                    return true;
                if (this.Rotary.IsIndexMoving())
                {
                    AxisPlaceZ?.EmgStop();
                    PostAlarm((int)AlarmKeys.eRotaryAxesMoving);
                    return false;
                }
            }
            else if (baseComponent == this.AxisToolT)
            {
                if (this.IsPositionPlaceZSafety() == false || this.IsPositionPickZSafety() == false)
                {
                    nRet = MovePositionSafetyZ();
                    if(nRet != 0)
                    {
                        AxisToolT?.EmgStop();
                        PostAlarm((int)AlarmKeys.eInputDieTransferNotSafety);
                        return false;
                    }
                }
            }
            return bRet;
        }
        public int MoveAxisPositionOne(MotionAxis axis, double target, bool isFine = false)
        {
            if (axis == null) 
                return -1;

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
                    if (this.Rotary.IsIndexMoving())
                    {
                        AxisToolT.EmgStop();
                        AxisPickZ.EmgStop();
                        AxisPlaceZ.EmgStop();
                        PostAlarm((int)AlarmKeys.eRotaryAxesMoving);
                        return -1;
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
            _isSafetyMoving = true;
            try
            {
                return MoveTeachingPositionOnce((int)InputDieTransferConfig.TeachingPositionName.SafetyZone, isFine);
            }
            finally
            {
                _isSafetyMoving = false;
            }
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

                    Thread.Sleep(2); // 0→5ms로 약간 여유 (CPU 점유 감소)
                }

                return coreTask.Result;
            }, 
            ct);
        }


        //public Task<int> MoveTeachingPositionOnceAsync(int selIndex, bool isFine)
        //{
        //    return Task.Run(() =>
        //    {
        //        return MoveTeachingPositionOnce(selIndex, isFine);
        //    });
        //    //Task.Run(() => MoveTeachingPositionOnce(selIndex, isFine));
        //}

        //public int MoveTeachingPositionOnce(int selIndex, bool isFine)
        //{
        //    int waitErrors = 0;
        //    string teachName = string.Empty;
        //    bool bSuccssed = Config.GetTeachingPositionName(selIndex, out teachName);

        //    if (bSuccssed == false)
        //    {
        //        Log.Write(UnitName, "MoveTeachingPositionOnce", $"[TEACH 이동 오류] 인덱스 '{selIndex}' 티칭포지션 이름을 찾을 수 없습니다.");
        //        return -1;
        //    }
        //    TeachingPosition tp = TeachingPositions.FirstOrDefault(t => t.Name == teachName);

        //    var axisPos = GetAxisPositions(tp);
        //    if (axisPos == null)
        //        return -1;
        //    var axisObj = GetAxisObjects(tp);

        //    //teachName <- 티칭 이름을 이미 알고있지.
        //    switch(selIndex)
        //    {
        //        case (int)InputDieTransferConfig.TeachingPositionName.Pickup:
        //            break;
        //    }

        //    //** 티칭포지션 중에 Z축이랑 묶여있는 거는 
        //    //** 무조건 Z축이 먼저 움직이고 구동되도록 해야함!!!
        //    foreach (var kv in axisPos)
        //    {
        //        string key = kv.Key;
        //        double target = kv.Value;
        //        MotionAxis axis = null;
        //        if (axisObj != null && axisObj.TryGetValue(key, out axis)) { }
        //        if (axis == null && Axes.TryGetValue(key, out var direct))
        //            axis = direct;

        //        if (axis == null)
        //        {
        //            foreach (var ap in Axes)
        //            {
        //                if (ap.Value != null &&
        //                   (ap.Key.Equals(key, StringComparison.OrdinalIgnoreCase) ||
        //                    ap.Value.Name.Equals(key, StringComparison.OrdinalIgnoreCase)))
        //                {
        //                    axis = ap.Value;
        //                    break;
        //                }
        //            }
        //        }
        //        if (axis == null)
        //            continue;

        //        bool IsAuto = false;
        //        if (RunMode == UnitRunMode.Auto)
        //            IsAuto = true;
        //        else
        //            IsAuto = false;

        //        waitErrors = axis.MoveAbs(target, IsAuto, isFine);
        //    }

        //    if (waitErrors != 0)
        //    {
        //        return -1;
        //    }

        //    foreach (var kv in axisPos)
        //    {
        //        MotionAxis axis = null;
        //        if (axisObj != null && axisObj.TryGetValue(kv.Key, out axis)) { }
        //        if (axis == null && Axes.TryGetValue(kv.Key, out var directAxis))
        //            axis = directAxis;

        //        if (axis == null)
        //            continue;

        //        if (axis.WaitMoveDone(-1) != 0)
        //        {
        //            waitErrors++;
        //        }
        //    }
        //    return waitErrors == 0 ? 0 : -1;
        //}




        ///////////////////////////////////////////////////////////////////////////////////////////
        // Single Pickup (Non-Index) - 구조 통일 (Index 기반 메서드 패턴과 동일 스타일)
        public int MovePositionPickDown(bool isFine = false)
        {
            Task<int> task = MovePositionAsyncPickDown(isFine);
            while (!IsEndTask(task))
            {
                int interlock = IsMoveInterLockPickDown();
                if (interlock != 0)
                {
                    return -1;
                }
                Thread.Sleep(1);
            }
            return task.Result;
        }

        private Task<int> MovePositionAsyncPickDown(bool isFine = false)
        {
            return Task.Run(() =>
            {
                return OnMovePositionPickDown(isFine);
            });
        }

        private int OnMovePositionPickDown(bool isFine = false)
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

        private int IsMoveInterLockPickDown()
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
                Thread.Sleep(1);
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
                Thread.Sleep(1);
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
                var coreTask = Task.Run(() => OnMovePositionPickDown(isFine), ct);

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

                    int nRtn = IsMoveInterLockPickDown();
                    if (nRtn != 0)
                    {
                        return -1;
                    }

                    Thread.Sleep(2); // 0→5ms로 약간 여유 (CPU 점유 감소)
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

                Thread.Sleep(1);
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

            //double dZPos = GetTP(InputDieTransferConfig.TeachingPositionName.Ready.ToString(),
            //            AxisNames.LeftPlaceZ);
            //nRet = MoveAxisPositionOne(AxisPlaceZ, dZPos);
            //if (nRet != 0)
            //{
            //    return -1;
            //}

            return nRet;
        }
        private int IsMoveInterLockReady()
        {
            int nRet = 0;
            
            if(IsPositionPickZSafety() == false
                || IsPositionPlaceZSafety() == false)
            {
                AxisToolT?.EmgStop();
                AxisPickZ?.EmgStop();
                AxisPlaceZ?.EmgStop();
                PostAlarm((int)AlarmKeys.eInputStageNotSafety);
                return -1;
            }

            // T가 움직이기떄문에 안봐도됨. 
            // Z축만 상승되어 있으면 됨.
            //if (InputStage != null && InputStage.IsAnyAxisMoving())
            //{
            //    AxisToolT?.EmgStop();
            //    AxisPickZ?.EmgStop();
            //    AxisPlaceZ?.EmgStop();
            //    PostAlarm((int)AlarmKeys.eInputStageAxesMoving);
            //    return -1;
            //}

            //if (InputStageEjector != null && InputStageEjector.IsAnyAxisMoving())
            //{
            //    AxisToolT?.EmgStop();
            //    AxisPickZ?.EmgStop();
            //    AxisPlaceZ?.EmgStop();
            //    PostAlarm((int)AlarmKeys.eInputStageEjectorAxesMoving);
            //    return -1;
            //}

            ////if (Rotary != null && Rotary.IsAnyAxisMoving())
            //if (Rotary != null && Rotary.IsAxisMoving(AxisNames.IndexT))
            //{
            //    AxisToolT?.EmgStop();
            //    AxisPickZ?.EmgStop();
            //    AxisPlaceZ?.EmgStop();
            //    PostAlarm((int)AlarmKeys.eRotaryAxesMoving);
            //    return -1;
            //}

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

                    Thread.Sleep(2); // 0→5ms로 약간 여유 (CPU 점유 감소)
                }

                return coreTask.Result;
            }, ct);
        }

        public int MovePositionPlace_Index(int nIndex = 0, bool isFine = false)
        {
            Task<int> task = MovePositionAsyncPlace_Index(nIndex, isFine);
            while (IsEndTask(task) == false)
            {
                int nRtn = IsMoveInterLockPlace_Index(nIndex);
                if (nRtn != 0)
                {
                    return -1;
                }

                Thread.Sleep(1);
            }
            return task.Result;
        }
        public Task<int> MovePositionAsyncPlace_Index(int nIndex = 0, bool isFine = false)
        {
            return Task.Run(() =>
            {
                OnMovePositionPlace_Index(nIndex, isFine);
                return 0;
            });
        }
        private int OnMovePositionPlace_Index(int nIndex = 0, bool isFine = false)
        {
            int nRet = 0;
            if (IsPositionPlaceZSafety() == false 
                || IsPositionPickZSafety() == false)
            {
                nRet = MovePositionSafetyZ();
                if (nRet != 0)
                {
                    return -1;
                }
            }

            if (!TryGetPlaceTeachingName(nIndex, out string tpName))
            {
                Log.Write(UnitName, $"[OnMovePositionPlace_Index] Invalid index {nIndex}. Range 0~7");
                return -1;
            }

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
        }
        private int IsMoveInterLockPlace_Index(int nIndex = 0)
        {
            int nRet = 0;
            
            if (Rotary != null && this.Rotary.IsIndexMoving())
            {
                AxisToolT?.EmgStop();
                AxisPickZ?.EmgStop();
                AxisPlaceZ?.EmgStop();
                PostAlarm((int)AlarmKeys.eRotaryAxesMoving);
                return -1;
            }

            return nRet;
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
                    //Log.Write(UnitName, "[MovePickZAndPinZByOffset] pickMoving && pinMoving : Comp.");
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
        public bool IsPositionPickZSafety()
        {
            const string tpName = nameof(InputDieTransferConfig.TeachingPositionName.SafetyZone);
            if (AxisPickZ == null)
                return true;

            // 현재 실제 위치 읽기
            double currentPos;
            try
            {
                currentPos = AxisPickZ.GetPosition();
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                return false;
            }
            // 요구사항: 실제 위치가 0(또는 매우 근접) 이면 Safety 로 간주
            // 허용 오차는 장비 정밀도에 따라 조정(예: 0.005 이하)
            const double zeroTolerance = 0.005;
            if (Math.Abs(currentPos) <= zeroTolerance)
            {
                return true;
            }

            double target = GetTP(tpName, AxisNames.LeftPickZ);
            try
            {
                return AxisPickZ.InPosition(target);
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                return false;
            }
        }

        /// <summary>
        /// DieTransfer ToolT 축이 SafetyPos(or SafetyZone fallback) 위치인지 확인.
        /// SafetyZone Teaching에 ToolT 값이 없으면 다음 후보로 넘어감.
        /// </summary>
        public bool IsPositionToolTSafety()
        {
            return false;
        }

        /// <summary>
        /// DieTransfer PlaceZ 축이 SafetyPos(or SafetyZone fallback) 위치인지 확인.
        /// </summary>
        public bool IsPositionPlaceZSafety()
        {
            const string tpName = nameof(InputDieTransferConfig.TeachingPositionName.SafetyZone);
            if (AxisPlaceZ == null)
                return true;

            // 현재 실제 위치 읽기
            double currentPos;
            try
            {
                currentPos = AxisPlaceZ.GetPosition();
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                return false;
            }
            // 요구사항: 실제 위치가 0(또는 매우 근접) 이면 Safety 로 간주
            // 허용 오차는 장비 정밀도에 따라 조정(예: 0.005 이하)
            const double zeroTolerance = 0.005;
            if (Math.Abs(currentPos) <= zeroTolerance)
            {
                return true;
            }
                
            double target = GetTP(tpName, AxisNames.LeftPlaceZ);
            try
            {
                return AxisPlaceZ.InPosition(target);
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                return false;
            }
        }

        // ---- Pickup ----
        public bool IsPositionToolTPickup()
        {
            const string tpName = nameof(InputDieTransferConfig.TeachingPositionName.Pickup);
            if (AxisToolT == null) 
                return true;

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
            if (AxisToolT == null) 
                return true;
            double target = GetTP(tpName, AxisNames.LeftToolT);
            try 
            { 
                return AxisToolT.InPosition(target); 
            } 
            catch (Exception ex)
            {
                Log.Write(ex);
                return false; 
            }
        }

        public bool IsPositionPlaceZReady()
        {
            const string tpName = nameof(InputDieTransferConfig.TeachingPositionName.Ready);
            if (AxisPlaceZ == null) return true;
            double target = GetTP(tpName, AxisNames.LeftPlaceZ);
            try { return AxisPlaceZ.InPosition(target); } catch { return false; }
        }

        // ---- Specific Place Index (0~7) ----
        public bool IsPositionToolTPlaceIndex(int nIndex)
        {
            if (AxisToolT == null) return true;
            if (!TryGetPlaceTeachingName(nIndex, out string tpName)) 
                return false;

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
            if (nIndex >= 0 && nIndex < 8)
            {
                teachingIdx = nIndex + 1;
            }
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
        public bool SetVacuum(int nNo, bool on, bool bCheckSignal = false)
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
                Thread.Sleep(5);
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
        
        // === Arm Vacuum 상태 대기 공용 유틸 ===
        // expectOn: true=ON 될 때까지, false=OFF 될 때까지 대기
        // timeoutMs/pollMs: 타임아웃/폴링 간격
        private int WaitVacuumStateOrAlarm(int armIndex, bool expectOn, int timeoutMs = 1000, int pollMs = 1)
        {
            if (Config.IsSimulation || Config.IsDryRun)
                return 0;

            //Todo: 2025-10-10 GYN: Vacuum 해결 되면 return 지우기.
            return 0;

            var sw = Stopwatch.StartNew();
            while (sw.ElapsedMilliseconds <= timeoutMs)
            {
                bool ok = IsVacuumOK(armIndex);
                if (expectOn ? ok : !ok)
                    return 0;

                Thread.Sleep(pollMs);
            }

            // 타임아웃 처리
            PostAlarm((int)AlarmKeys.eInputDieTransferVaccum);
            Log.Write(UnitName, expectOn ? "[Vacuum] Arm vacuum ON timeout" : "[Vacuum] Arm vacuum OFF timeout");
            return -1;
        }

        private (bool flowControl, int value) InputStageVaccumOff()
        {
            InputStage.SetVacuum(false, false);
            if (Config.IsSimulation || Config.IsDryRun)
            {
                Thread.Sleep(5);
            }
            //else if (InputStage.IsVacuumOn() == true)
            //{
            //    PostAlarm((int)AlarmKeys.eInputStageVaccum);
            //    Log.Write(UnitName, "[SyncPickPinRetreat] Vacuum Release Timeout");
            //    return (flowControl: false, value: -1);
            //}

            return (flowControl: true, value: default);
        }



        /// //////////////////////////////////////////////////////////////////
        #endregion


        #region Seq Signals
        //private MaterialDie _WaferDie;         // PrepareNextDie에서 보관, CommitPickedDie에서 사용
        #endregion

        #region Lifecycle
        public override int OnRun() 
        {
            //TaktStart("OnRun");
            try
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
                    this.State = ProcessState.Stop;
                    this.OnStop();
                }
                return ret;
            }
            finally
            {
                //TaktEnd("OnRun");
            }
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

        protected override int OnRunReady()
        {
            TaktStart("OnRunReady");
            try
            {
                int nRet = 0;
                // ===== Snapshot log (원인 파악 1순위) =====
                MaterialDie held = this.GetMaterial() as MaterialDie;
                MaterialWafer wafer = this.InputStage.GetMaterialWafer();
                bool chipMappingDone = false;
                bool hasNextDie = false;
                try 
                { 
                    chipMappingDone = this.InputStage.ChipMappingDone; 
                } 
                catch { }
                
                try 
                { 
                    hasNextDie = this.InputStage.HasNextDie(); 
                } 
                catch { }

                Log.Write(UnitName, nameof(OnRunReady),
                    $"State={State}, " +
                    $"Held={(held == null ? "null" : $"Exist={held.Presence},State={held.State},Map=({held.MapX:F3},{held.MapY:F3})")}, " +
                    $"Wafer={(wafer == null ? "null" : $"Exist={wafer.Presence},Proc={wafer.ProcessSatate},Id={wafer.WaferId}")}, " +
                    $"ChipMappingDone={chipMappingDone}, " +
                    $"HasNextDie={hasNextDie}");

                // ===== 우선순위 1) 이미 들고 있는 다이가 있으면 Place 진행 가능하도록 Work로 진입 =====
                // (웨이퍼 언로드되어도 DieTransfer는 들고 있는 다이를 로터리에 내려놓아야 함)
                if (held != null 
                    && held.Presence == Material.MaterialPresence.Exist)
                {
                    State = ProcessState.Work;
                    return 0;
                }

                // ===== 우선순위 2) 스테이지에서 픽업할 다이가 준비되어 있을 때만 Work 진입 =====
                if (wafer != null
                    && wafer.Presence == Material.MaterialPresence.Exist
                    && wafer.ProcessSatate == Material.MaterialProcessSatate.Processing
                    && chipMappingDone
                    && hasNextDie)
                {
                    State = ProcessState.Work;
                    return 0;
                }

                // ===== 그 외: Ready 자세 유지 =====
                if (IsPositionToolTReady() == false)
                {
                    nRet = MovePositionReady();
                    if (nRet != 0)
                    {
                        Log.Write(UnitName, "[OnRunReady] MovePositionReady failed");
                        PostAlarm((int)AlarmKeys.eInputDieTransferMoveFail);
                        return -1;
                    }
                }
                return nRet;

                // 기존 코드
                {
                    //MaterialWafer wafer = this.InputStage.GetMaterialWafer();
                    //if (wafer != null)
                    //{
                    //    if (wafer.Presence == Material.MaterialPresence.Exist)
                    //    {
                    //        if (wafer.ProcessSatate == Material.MaterialProcessSatate.Processing
                    //            && this.InputStage.HasNextDie()
                    //            && this.InputStage.ChipMappingDone)
                    //        {
                    //            State = ProcessState.Work;
                    //        }
                    //        else
                    //        {
                    //            if(IsPositionToolTReady() == false)
                    //            {
                    //                nRet = MovePositionReady();
                    //                if (nRet != 0)
                    //                {
                    //                    Log.Write(UnitName, "[OnRunReady] MovePositionReady failed");
                    //                    PostAlarm((int)AlarmKeys.eInputDieTransferMoveFail);
                    //                    return -1; 
                    //                }
                    //            }
                    //        }
                    //    }
                    //}
                    //return nRet;
                }
            }
            finally
            {
                TaktEnd("OnRunReady");
            }
        }
        protected override int OnRunWork()
        {
            TaktStart("OnRunWork");
            try
            {
                int nRet = 0;
                int nArmIndex = GetInputTrArmIndex();
                MaterialDie DieTr = this.GetMaterial() as MaterialDie;

                if (DieTr != null && IsVacuumOK(nArmIndex))
                {
                    if (IsVacuumOK(nArmIndex))
                    {
                        var res = InputStageVaccumOff();
                        if (!res.flowControl) return 
                                res.value;
                    }
                    else
                    {
                        this.State = ProcessState.Ready;
                        return 0;
                    }
                    TaktStart("RotateToolTForPlace_AsyncWait");
                    try
                    {
                        nRet = RotateToolTForPlace_AsyncWait();
                    }
                    finally
                    {
                        TaktEnd("RotateToolTForPlace_AsyncWait");
                    }
                    if (nRet != 0)
                    {
                        AxisPickZ?.EmgStop();
                        AxisToolT?.EmgStop();
                        PostAlarm((int)AlarmKeys.eInputDieTransferRotateToolTForPlace);
                        Log.Write(UnitName, "[OnRunWork] RotateToolTForPlace_AsyncWait failed");
                        return -1;
                    }
                    State = ProcessState.Complete;
                    return nRet;
                }

                if (InputStage.IsRingPresent() == false
                    && InputStage.IsPlateUp() == false)
                {
                    AxisPickZ?.EmgStop();
                    AxisToolT?.EmgStop();
                    PostAlarm((int)AlarmKeys.eInputStageVaccum);
                    Log.Write(UnitName, "OnRunWork", "[OnRunWork] Wafer None failed");
                    return -1;
                }

                MaterialWafer waferStage = this.InputStage.GetMaterialWafer();
                if (waferStage == null) 
                { 
                    Log.Write(UnitName, "[OnRunWork] wafer is null"); 
                    PostAlarm((int)AlarmKeys.eInputDieTransferNoWafer);
                    return -1; 
                }

                if (waferStage.Presence != Material.MaterialPresence.Exist) 
                    return 0;

                if (waferStage.ProcessSatate != Material.MaterialProcessSatate.Processing) 
                    return 0;

                if (this.InputStage.HasNextDie() == false) 
                    return 0;

                SetVacuum(nArmIndex, true);

                Log.Write("kkkkkkRotary", "PrepareNextDie");
                TaktStart("PrepareNextDie");
                try
                {
                    if(taskPrepareNextDie == null)
                    {

                        nRet = PrepareNextDie();
                    }
                    else
                    {
                        taskPrepareNextDie.Wait();
                        taskPrepareNextDie = null;
                    }
                }
                finally
                {
                    TaktEnd("PrepareNextDie");
                }

                MaterialDie waferDie = InputStage.GetNextDie();
                if (waferDie == null || waferDie.Presence != Material.MaterialPresence.Exist)
                {
                    return 0;
                }
                if (nRet != 0)
                {
                    AxisPickZ?.EmgStop();
                    AxisToolT?.EmgStop();
                    PostAlarm((int)AlarmKeys.eInputDieTransferPrepareNextDie);
                    Log.Write(UnitName, "[OnRunWork] PrepareNextDie failed");
                    return -1;
                }

                //bool haveDieOnArm = (waferDie != null 
                //                    || waferDie.State == DieProcessState.Picked);
                //if (haveDieOnArm)
                bool shouldPickFromStage = (waferDie != null
                                           && waferDie.Presence == Material.MaterialPresence.Exist
                                           && waferDie.State != DieProcessState.Picked);
                Log.Write(UnitName, "OnRunWork",
                        $"PickDecision shouldPickFromStage={shouldPickFromStage}, " +
                        $"waferDie={(waferDie == null ? "null" : $"Exist={waferDie.Presence},State={waferDie.State},Map=({waferDie.MapX:F3},{waferDie.MapY:F3}),Index={waferDie.Index}")}");
                
                if (shouldPickFromStage)
                {
                    Log.Write("kkkkkkRotary", "RaiseEjectorForPick");
                    TaktStart("RaiseEjectorForPick");
                    try
                    {
                        nRet = RaiseEjectorForPick();
                    }
                    finally
                    {
                        TaktEnd("RaiseEjectorForPick");
                    }
                    if (nRet != 0) 
                    {
                        AxisPickZ?.EmgStop();
                        AxisToolT?.EmgStop();
                        PostAlarm((int)AlarmKeys.eInputDieTransferRaiseEjector);
                        Log.Write(UnitName, "[OnRunWork] RaiseEjectorForPick failed"); 
                        return -1; 
                    }

                    if (IsStop) { return 0; }
                    TaktStart("ChipPickDown");

                    Log.Write("kkkkkkRotary", "ChipPickDown");
                    try
                    {
                        nRet = ChipPickDown();
                    }
                    finally
                    {
                        TaktEnd("ChipPickDown");
                    }
                    if (nRet != 0) 
                    {
                        AxisPickZ?.EmgStop();
                        AxisToolT?.EmgStop();
                        PostAlarm((int)AlarmKeys.eInputDieTransferChipPickDown);
                        Log.Write(UnitName, "[OnRunWork] ChipPickDown failed"); 
                        return -1; 
                    }

                    Log.Write("kkkkkkRotary", "SyncPickPinUp");
                    TaktStart("SyncPickPinUp");
                    try
                    {
                        nRet = SyncPickPinUp();
                    }
                    finally
                    {
                        TaktEnd("SyncPickPinUp");
                    }
                    if (nRet != 0) 
                    {
                        AxisPickZ?.EmgStop();
                        AxisToolT?.EmgStop();
                        PostAlarm((int)AlarmKeys.eInputDieTransferSyncPickPinUp);
                        Log.Write(UnitName, "[OnRunWork] SyncPickPinUp failed"); 
                        return -1; 
                    }

                    Log.Write("kkkkkkRotary", "SyncPickPinRetreat");
                    TaktStart("SyncPickPinRetreat");
                    try
                    {
                        nRet = SyncPickPinRetreat();
                    }
                    finally
                    {
                        TaktEnd("SyncPickPinRetreat");
                    }
                    Log.Write("kkkkkkRotary", "SyncPickPinRetreat");
                    if (nRet != 0) 
                    {
                        AxisPickZ?.EmgStop();
                        AxisToolT?.EmgStop();
                        PostAlarm((int)AlarmKeys.eInputDieTransferSyncPickPinRetreat);
                        Log.Write(UnitName, "[OnRunWork] SyncPickPinRetreat failed"); 
                        return -1; 
                    }

                    if (IsVacuumOK(nArmIndex))
                    {
                        var res = InputStageVaccumOff();
                        if (!res.flowControl) 
                            return res.value;

                        TaktStart("CommitPickedDie");
                        try
                        {
                            nRet = CommitPickedDie();
                        }
                        finally
                        {
                            TaktEnd("CommitPickedDie");
                        }
                        if (nRet != 0)
                        {
                            AxisPickZ?.EmgStop();
                            AxisToolT?.EmgStop();
                            PostAlarm((int)AlarmKeys.eInputDieTransferCommitPickedDie);
                            Log.Write(UnitName, "[OnRunWork] CommitPickedDie failed");
                            return -1;
                        }
                    }
                    else
                    {
                        nRet = MovePositionReady();
                        if (nRet != 0)
                        {
                            AxisPickZ?.EmgStop();
                            AxisToolT?.EmgStop();
                            PostAlarm((int)AlarmKeys.eInputDieTransferNotSafety);
                            Log.Write(UnitName, "[OnRunWork] CommitPickedDie failed");
                            return -1;
                        }

                        this.SetVacuum(nArmIndex, false);
                        this.SetBlow(nArmIndex, true);

                        Thread.Sleep(100);

                        this.SetBlow(nArmIndex, false);
                        this.SetVacuum(nArmIndex, true);

                        nRet = CommitNotPickedDie();
                        this.SetMaterial(null);
                        this.State = ProcessState.Ready;
                        return 0;
                    }
                }


                Log.Write("kkkkkkRotary", "RotateToolTForPlace_AsyncWait start");

                TaktStart("RotateToolTForPlace_AsyncWait");
                try
                {
                    nRet = RotateToolTForPlace_AsyncWait();
                }
                finally
                {
                    TaktEnd("RotateToolTForPlace_AsyncWait");
                }
                if (nRet != 0) 
                {
                    AxisPickZ?.EmgStop();
                    AxisToolT?.EmgStop();
                    PostAlarm((int)AlarmKeys.eInputDieTransferRotateToolTForPlace);
                    Log.Write(UnitName, "[OnRunWork] RotateToolTForPlace_AsyncWait failed"); 
                    return -1; 
                }

                Log.Write("kkkkkkRotary", "RotateToolTForPlace_AsyncWait");
                State = ProcessState.Complete;
                return nRet;
            }
            finally
            {
                TaktEnd("OnRunWork");
            }
        }

        protected override int OnRunComplete()
        {
            TaktStart("OnRunComplete");
            try
            {
                int nRet = 0;

                //Log.Write("kkkkkkRotary", "PlaceChipDown Start11");
                if (Rotary.RequestInputDieTrDie == false) 
                    return 0;

                if (this.Rotary.IsIndexMoving()) 
                    return 0;

                if (Rotary.IsLoadSocketEmpty() == false) 
                    return 0;

                MaterialDie TrDie = (GetMaterial() as MaterialDie);
                if (TrDie == null || TrDie.Presence != Material.MaterialPresence.Exist)
                {
                    return 0;
                }

                Log.Write("kkkkkkRotary", "PlaceChipDown Start12");

                this.taskPrepareNextDie = Task.Factory.StartNew(() => 
                { 
                    return MoveStageToNextDie(out var die); 
                });

                TaktStart("PlaceChipDown");
                try
                {
                    nRet = PlaceChipDown();
                }
                finally
                {
                    TaktEnd("PlaceChipDown");
                }
                if (nRet != 0)
                {
                    AxisPlaceZ?.EmgStop();
                    AxisToolT?.EmgStop();
                    PostAlarm((int)AlarmKeys.eInputDieTransferPlaceChipDown);
                    Log.Write(UnitName, "[OnRunComplete] PlaceChipDown failed");
                    return -1;
                }
                Log.Write("kkkkkkInputTr", "PlaceChipDown End");

                Log.Write("kkkkkkRotary", "PlaceChipDown Start123");
                TaktStart("ReleaseVacuumAndPlaceUp");
                try
                {
                    nRet = ReleaseVacuumAndPlaceUp();
                }
                finally
                {
                    TaktEnd("ReleaseVacuumAndPlaceUp");
                }
                if (nRet != 0)
                {
                    AxisPlaceZ?.EmgStop();
                    AxisToolT?.EmgStop();
                    PostAlarm((int)AlarmKeys.eInputDieTransferReleaseVacuumAndPlaceUp);
                    Log.Write(UnitName, "[OnRunComplete] ReleaseVacuumAndPlaceUp failed");
                    return -1;
                }

                TrDie.State = DieProcessState.Inspecting;
                TrDie.ProcessSatate = Material.MaterialProcessSatate.Processing;
                TrDie.Presence = Material.MaterialPresence.Exist;

                this.MoveMaterial(TrDie, this.Rotary);
                int armIndex = GetInputTrArmIndex();
                SetVacuum(armIndex, true, false);
                this.SetMaterial(null);

                Log.Write("kkkkkkRotary", "PlaceChipDown Start124");
                State = ProcessState.None;
                return 0;
            }
            finally
            {
                TaktEnd("OnRunComplete");
            }
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
            //this.SequencePlayers.Add(EjectorVacuumOn);
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
            //if (RunMode == UnitRunMode.Manual)
            {
                this.CurrentFunc = PrepareNextDie;
            }

            // 웨이퍼/상태 점검은 여기서도 방어적으로 수행
            var wafer = this.InputStage?.GetMaterialWafer();
            if (wafer == null)
            {
                Log.Write(UnitName, "[PrepareNextDie] wafer is null");
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

            MaterialDie die;
            nRet = MoveStageToNextDie(out die);
            if (die == null || die.Presence != Material.MaterialPresence.Exist)
            {
                // 더 이상 픽할 다이가 없으면 우아하게 스킵
                return 0;
            }

            if (nRet != 0)
            {
                Log.Write(UnitName, "[PrepareNextDie] MoveStageToNextDie failed");
                return -1;
            }

            return nRet;
        }

        public int RecheckDieAndAlign(bool bFineSpeed = false)
        {
            int nRet = 0;
            if (RunMode == UnitRunMode.Manual)
            {
                this.CurrentFunc = RecheckDieAndAlign;
                //var mb = new MessageBoxOk();
                //mb.Focus();
                //mb.ShowDialog("알림", "웨이퍼 스테이지 이동 후 진행 바랍니다.");
                Log.Write(UnitName, "RecheckDieAndAlign", "[RecheckDieAndAlign] UnitRunMode.Manual");
                return 0;
            }
            if (InputStage == null)
            {
                Log.Write(UnitName, "RecheckDieAndAlign", "[RecheckDieAndAlign] InputStage is null");
                return -1;
            }
            MaterialWafer wafer = this.InputStage.GetMaterialWafer();
            if (wafer == null)
            {
                Log.Write(UnitName, "RecheckDieAndAlign", "[RecheckDieAndAlign] wafer is null");
                return -1;
            }
            if (wafer.Presence != Material.MaterialPresence.Exist)
            {
                Log.Write(UnitName, "RecheckDieAndAlign", "[RecheckDieAndAlign] wafer is not exist");
                return -1;
            }
            if (wafer.ProcessSatate != Material.MaterialProcessSatate.Processing)
            {
                Log.Write(UnitName, "RecheckDieAndAlign", "[RecheckDieAndAlign] wafer is not processing state");
                return -1;
            }
            nRet = InputStage.RecheckDieAndAlign(bFineSpeed);
            if (nRet != 0)
            {
                Log.Write(UnitName, "RecheckDieAndAlign", "[RecheckDieAndAlign] InputStage.RecheckDieAndAlign failed");
                return -1;
            }

            Log.Write(UnitName, "RecheckDieAndAlign", "[RecheckDieAndAlign] ???");
            return nRet;
        }

        public int RaiseEjectorForPick(bool bFineSpeed = false)
        {
            int nRet = 0;
            this.CurrentFunc = RaiseEjectorForPick;
            

            if (InputStageEjector == null)
            {
                AxisPickZ.EmgStop();
                AxisPlaceZ.EmgStop();
                AxisToolT.EmgStop();
                PostAlarm((int)AlarmKeys.eInputStageNotSafety);
                Log.Write(UnitName, "[RaiseEjectorForPick] InputStageEjector is null");
                return -1;
            }

            int nArmIndex = GetInputTrArmIndex();
            this.SetVacuum(nArmIndex, true);

            TaktStart("EjectorVacuumOn");
            try
            {
                nRet = EjectorVacuumOn();
            }
            finally
            {
                TaktEnd("EjectorVacuumOn");
            }
            if (nRet != 0)
            {
                Log.Write(UnitName, "[OnRunWork] EjectorVacuumOn failed");
                return -1;
            }

            if (IsStop) 
            { return 0; }

            if (WaitVacuumStateOrAlarm(nArmIndex, expectOn: true, timeoutMs: 1000, pollMs: 1) != 0)
            {
                Log.Write(UnitName, "[OnRunWork] WaitVacuumStateOrAlarm failed");
                return -1;
            }

            int blockUpResult = InputStageEjector.MovePositionEjectBlockUp(bFineSpeed);
            if (blockUpResult != 0)
            {
                AxisPickZ.EmgStop();
                AxisPlaceZ.EmgStop();
                AxisToolT.EmgStop();
                PostAlarm((int)AlarmKeys.eInputStageNotSafety);
                Log.Write(UnitName, "[RaiseEjectorForPick] EjectBlockUp 이동 실패");
                return -1;
            }

            int pinReadyResult = InputStageEjector.MovePositionEjectPinReady(bFineSpeed);
            if (pinReadyResult != 0)
            {
                AxisPickZ.EmgStop();
                AxisPlaceZ.EmgStop();
                AxisToolT.EmgStop();
                PostAlarm((int)AlarmKeys.eInputStageNotSafety);
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

            SetVacuum(nArmIndex, true, false);
            
            nRet = MovePositionPickDown(bFineSpeed);
            if (nRet != 0)
            {
                Log.Write(UnitName, "[ChipPickDown] MovePositionPickUp failed");
                return -1;
            }
           
            return nRet;
        }

        


        public int EjectorVacuumOn(bool bFineSpeed = true)
        {
            if (InputStageEjector == null)
                return -1;

            //if (RunMode == UnitRunMode.Manual)
            //{
            //    this.CurrentFunc = EjectorVacuumOn;
            //}
            int nRet = 0;

            InputStage.SetVacuum(true, false);

            return nRet;
        }
        public int SyncPickPinUp(bool bFineSpeed = false)
        {
            if (InputStageEjector == null)
            {
                PostAlarm((int)AlarmKeys.eInputStageNotSafety);
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
                PostAlarm((int)AlarmKeys.eInputStageNotSafety);
                return -1;
            }

            if (RunMode == UnitRunMode.Manual)
            {
                this.CurrentFunc = SyncPickPinRetreat;
            }

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

            MaterialDie WaferDie = InputStage.GetNextDie();
            if (WaferDie == null || WaferDie.Presence != Material.MaterialPresence.Exist)
            {
                Log.Write(UnitName, "[CommitPickedDie] _WaferDie is null or not exist");
                PostAlarm((int)AlarmKeys.eInputDieTransferNoDie);
                return -1;  // 이 경우는 에러로 간주
            }

            // 1) 웨이퍼 내 슬롯 비우기: 동일 인스턴스를 웨이퍼에서 제거(Placeholder로 대체)
            try
            {
                var wafer = this.InputStage?.GetMaterialWafer();
                if (wafer != null && wafer.Dies != null)
                {
                    lock (wafer)
                    {
                        int idx = wafer.Dies.IndexOf(WaferDie);
                        if (idx >= 0)
                        {
                            WaferDie.State = DieProcessState.Picked;
                            WaferDie.ProcessSatate = Material.MaterialProcessSatate.Processing;
                            WaferDie.Presence = Material.MaterialPresence.Exist;
                            MoveMaterial(WaferDie, this); // DieTransfer에 올려둠
                            this.SetMaterial(WaferDie);

                            // 여기에서 Data 비우면.. outStage에서는 없는 Die로 인식함...
                            // 스테이지에 Die 비우는 것만 나중에 하자.
                            //wafer.Dies[idx] = WaferDie;
                            //InputStage.SetMaterial(wafer); // 웨이퍼 갱신
                        }
                        // else: 리스트가 다른 인스턴스를 들고 있는 경우도 있으나, 이후 이벤트/재계산으로 수렴됩니다.
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Write(UnitName, "[CommitPickedDie] Wafer slot clear failed: " + ex.Message);
                // 슬롯 비우기 실패해도 이후 흐름은 지속
            }

            InputStage.UpdateUI();

            return 0;

            //기존 코드
            //if (RunMode == UnitRunMode.Manual)
            //{
            //    this.CurrentFunc = CommitPickedDie;
            //}

            //if (RunMode == UnitRunMode.Auto)
            //{
            //    if (_currentDie == null || _currentDie.Presence != Material.MaterialPresence.Exist)
            //    {
            //        return -1;  // 이 경우는 에러로 간주
            //    }
            //    _currentDie.State = DieProcessState.Picked;
            //    _currentDie.ProcessSatate = Material.MaterialProcessSatate.Processing;

            //    // UI에게 알림: 현재 웨이퍼에서 해당 다이가 픽업되었음을 이벤트로 통지
            //    OnDiePicked(_currentDie);

            //    SetMaterial(_currentDie); // 이후 Complete 단계에서 Rotary로 전달
            //}

            //return 0;
        }

        public int CommitNotPickedDie(bool bFineSpeed = false)
        {
            if (RunMode == UnitRunMode.Manual)
            {
                this.CurrentFunc = CommitPickedDie;
            }

            MaterialDie WaferDie = InputStage.GetNextDie();
            if (WaferDie == null || WaferDie.Presence != Material.MaterialPresence.Exist)
            {
                Log.Write(UnitName, "[CommitNotPickedDie] _WaferDie is null or not exist");
                PostAlarm((int)AlarmKeys.eInputDieTransferNoDie);
                return -1;  // 이 경우는 에러로 간주
            }

            // 1) 웨이퍼 내 슬롯 비우기: 동일 인스턴스를 웨이퍼에서 제거(Placeholder로 대체)
            try
            {
                var wafer = this.InputStage?.GetMaterialWafer();
                if (wafer != null && wafer.Dies != null)
                {
                    lock (wafer)
                    {
                        int idx = wafer.Dies.IndexOf(WaferDie);
                        
                        if (idx >= 0)
                        {
                            WaferDie.State = DieProcessState.Rejected;
                            WaferDie.ProcessSatate = Material.MaterialProcessSatate.Skipped;
                            WaferDie.Presence = Material.MaterialPresence.Exist;
                            MoveMaterial(WaferDie, this); // DieTransfer에 올려둠
                            SetMaterial(WaferDie);

                            wafer.Dies[idx] = WaferDie;
                            InputStage.SetMaterial(wafer); // 웨이퍼 갱신
                        }
                        // else: 리스트가 다른 인스턴스를 들고 있는 경우도 있으나, 이후 이벤트/재계산으로 수렴됩니다.
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Write(UnitName, "[CommitNotPickedDie] Wafer slot clear failed: " + ex.Message);
                // 슬롯 비우기 실패해도 이후 흐름은 지속
            }

            InputStage.UpdateUI();

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
            Thread.Sleep(50);
            //Log.Write(UnitName, "RotateToolTForPlace_AsyncWait", "RecheckDieAndAlign Start");
            Task<int> t2 = Task.Factory.StartNew(() =>
            {
                return  RecheckDieAndAlign();
            });
            // 2차 보상 하지말고 돌려보자.
            while (t.IsCompleted == false)
            {
                // 진행 중 모니터링(필요 시 비전/로그 등)
                // 여기 아에 하지말고 돌려보자.
                // 너무 빨라서 Grab이 안됬었음...
                // 타이밍 맞춰야함....
                

                //////마지막에 스테이지 교체 하니깐 여기서는 없으면 우선 걍 패뜨
                //////아니면 마지막 칩 확인해야할듯.
                if (nRet != 0)
                {
                    Log.Write(UnitName, "[OnRunWork] RecheckDieAndAlign failed");
                    //    //return -1;
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

            if (!TryGetPlaceTeachingName(nIndex, out string tpName))
            {
                Log.Write(UnitName, $"[PlaceChipDown] Invalid index {nIndex}. Range 0~7");
                return -1;
            }

            var tpObj = Config.GetTeachingPosition(tpName);
            if (tpObj == null)
            {
                Log.Write(UnitName, $"[PlaceChipDown] Teaching not found: {tpName}");
                return -1;
            }

            Rotary.SetVacuum(nIndex, true);
            Thread.Sleep(1);

            double dTPos = GetTP(tpName, AxisNames.LeftPlaceZ);
            nRet = MoveAxisPositionOne(AxisPlaceZ, dTPos, bFineSpeed);
            if (nRet != 0)
            {
                Log.Write(UnitName, "[RotateToolTForPlace] ToolT Place 이동 실패");
                return -1;
            }

            this.WaitByTime(Config.nPlaceBeforeVacWaitTime, 1);//Thread.Sleep(1);
            SetVacuum(armIndex, false, false);
            SetVent(armIndex, true);
            this.WaitByTime(Config.nPlaceAfterVacWaitTime, 1);//Thread.Sleep(1);

            SetVent(armIndex, false);
            Thread.Sleep(1);

            this.WaitByTime(Config.nPlaceBeforeBlowWaitTime, 1);
            SetBlow(armIndex, true);

            Task.Run(() =>
            {
                try
                {
                    this.WaitByTime(Config.nPlaceAfterBlowWaitTime, 1);
                    SetBlow(armIndex, false);
                }
                catch (Exception ex)
                {
                    Log.Write(ex);
                }
            });
            
            return nRet;

        }

        public int ChipPickDownReturn(bool bFineSpeed = false)
        {
            int nRet = 0;
            if (RunMode == UnitRunMode.Manual)
            {
                this.CurrentFunc = ChipPickDown;
            }

            int nArmIndex = GetInputTrArmIndex();

            nRet = MovePositionPickDown(bFineSpeed); //이게 다운이다.!!
            if (nRet != 0)
            {
                Log.Write(UnitName, "[ChipPickDown] MovePositionPickUp failed");
                return -1;
            }

            SetVacuum(nArmIndex, false, false);
            SetVent(nArmIndex, true);
            Thread.Sleep(100);
            SetVent(nArmIndex, false);
            SetBlow(nArmIndex, true);
            Thread.Sleep(100);
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

                // Rotary/암 Vacuum 확인 (동일 인덱스 사용)
                //nRet = Rotary.WaitVacuumStateOrAlarm(Rotary.GetLoadIndexNo(), true);
                nRet = Rotary.WaitVacuumStateOrAlarm(GetLoadIndexNo(), true);
                if (nRet != 0)
                {
                    Log.Write(UnitName, "[RotaryVacuumOn] Vacuum Timeout");
                    return -1;
                }
                nRet = WaitVacuumStateOrAlarm(armIndex, false);
                if (nRet != 0)
                {
                    Log.Write(UnitName, "[DieTrVacuumOn] Vacuum Timeout");
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
            int nIndex = GetLoadIndexNo();
            if (!TryGetPlaceTeachingName(nIndex, out string tpName))
            {
                Log.Write(UnitName, $"[RotateToolTForPlace] Invalid index {nIndex}. Range 0~7");
                return -1;
            }

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
                    PostAlarm((int)AlarmKeys.eInputDieTransferNotSafety);
                    Log.Write(UnitName, "OnEnsureReady Fail - MovePositionSafetyZ");
                    return nRet;
                }
            }

            if (IsPositionToolTReady() == false)
            {
                nRet = MovePositionReady();
                if(nRet != 0)
                {
                    PostAlarm((int)AlarmKeys.eInputDieTransferNotSafety);
                    Log.Write(UnitName, "OnEnsureReady Fail - MovePositionReady");
                    return nRet;
                }
            }

            return nRet;
        }
        #endregion

        #region Update UI
        protected virtual void OnDiePicked(MaterialDie die)
        {
            var handler = DiePicked;
            if (handler == null)
            {
                return;
            }

            try
            {
                handler(this, new DiePickedEventArgs(die));
            }
            catch (Exception ex)
            {
                Log.Write(UnitName, "[OnDiePicked] " + ex.Message);
            }
        }
        #endregion

        // 클래스 내부에 추가
        public void ResetForNewRun(bool moveToSafeReady = true, bool clearHeldDie = true)
        {
            // 1) 런타임 플래그/버퍼 초기화
            taskPrepareNextDie = null;

            _isSafetyMoving = false;
            this.CurrentFunc = null;

            if (clearHeldDie)
            {
                try 
                { 
                    this.SetMaterial(null); 
                } 
                catch (Exception ex) 
                { Log.Write(UnitName, $"ResetForNewRun SetMaterial(null) failed: {ex.Message}"); }
            }

            // 2) IO 안전 상태(실기에서만) - Arm Vacuum/Blow/Vent OFF, Stage Vac OFF
            try
            {
                if (!(Config?.IsSimulation == true || Config?.IsDryRun == true))
                {
                    for (int i = 0; i < 4; i++)
                    {
                        try { SetBlow(i, false); } catch { }
                        try { SetVent(i, false); } catch { }
                        try { SetVacuum(i, false, false); } catch { }
                    }

                    try { InputStage?.SetVacuum(false, false); } catch { }

                    // 선택: 회전기 로드 소켓 진공 OFF
                    try
                    {
                        if (Rotary != null)
                        {
                            int cnt = Rotary.GetIndexCount();
                            for (int i = 0; i < cnt; i++)
                                Rotary.SetVacuum(i, false);
                        }
                    }
                    catch { }
                }
            }
            catch (Exception ex)
            {
                Log.Write(UnitName, $"ResetForNewRun IO safe-state failed: {ex.Message}");
            }

            // 3) 축 안전/Ready 복귀(선택)
            if (moveToSafeReady)
            {
                try
                {
                    // Z Safety → Ready 순
                    EnsureReady();
                    MovePositionReady();
                }
                catch (Exception ex)
                {
                    Log.Write(UnitName, $"ResetForNewRun move to ready failed: {ex.Message}");
                }
            }
        }

        public bool GetHeldDieExists()
        {
            // SetMaterial 로 올려둔 다이 또는 _currentDie 가 아직 Picked 상태로 유지 중인지
            var held = GetMaterial() as MaterialDie;
            if (held != null && held.State == DieProcessState.Picked && held.Presence == Material.MaterialPresence.Exist)
                return true;
            return false;
        }

        /// <summary>
        /// Config Form(PositionTeachingControl)에서 사용하던 티칭 이동 로직을 Unit으로 이동.
        /// - teachingIndex: Config.TeachingPositions 인덱스
        /// - isFine: Fine/Coarse
        /// 반환: 0=성공, -1=실패
        /// </summary>
        public int MoveByTeachingIndex(int teachingIndex, bool isFine)
        {
            string tpName;
            if (Config == null || !Config.GetTeachingPositionName(teachingIndex, out tpName) || string.IsNullOrWhiteSpace(tpName))
                return -1;

            InputDieTransferConfig.TeachingPositionName en;
            if (!Enum.TryParse(tpName, out en))
                return -1;

            return MoveByTeachingName(en, isFine);
        }

        /// <summary>
        /// TeachingPositionName 기반 이동 라우팅(핵심).
        /// Form에서 switch 하던 것을 이 함수로 이동.
        /// </summary>
        public int MoveByTeachingName(InputDieTransferConfig.TeachingPositionName teachingName, bool isFine)
        {
            int rc;
            int placeIndex;

            switch (teachingName)
            {
                case InputDieTransferConfig.TeachingPositionName.Pickup:
                    rc = MovePositionSafetyZ(isFine);
                    if (rc != 0) return -1;

                    rc = MovePositionPickUpToolT(isFine);
                    if (rc != 0) return -1;

                    rc = MovePositionPickUpPickZ(isFine);
                    if (rc != 0) return -1;

                    return 0;

                case InputDieTransferConfig.TeachingPositionName.Ready:
                    rc = MovePositionReady(isFine);
                    return rc == 0 ? 0 : -1;

                case InputDieTransferConfig.TeachingPositionName.SafetyZone:
                    rc = MovePositionSafetyZ(isFine);
                    return rc == 0 ? 0 : -1;

                case InputDieTransferConfig.TeachingPositionName.Place_Index1: placeIndex = 0; break;
                case InputDieTransferConfig.TeachingPositionName.Place_Index2: placeIndex = 1; break;
                case InputDieTransferConfig.TeachingPositionName.Place_Index3: placeIndex = 2; break;
                case InputDieTransferConfig.TeachingPositionName.Place_Index4: placeIndex = 3; break;
                case InputDieTransferConfig.TeachingPositionName.Place_Index5: placeIndex = 4; break;
                case InputDieTransferConfig.TeachingPositionName.Place_Index6: placeIndex = 5; break;
                case InputDieTransferConfig.TeachingPositionName.Place_Index7: placeIndex = 6; break;
                case InputDieTransferConfig.TeachingPositionName.Place_Index8: placeIndex = 7; break;

                default:
                    return -1;
            }

            // Place_Index 공통 처리
            rc = MovePositionSafetyZ(isFine);
            if (rc != 0) return -1;

            rc = MovePositionPlace_Index(placeIndex, isFine);
            return rc == 0 ? 0 : -1;
        }

        /// <summary>
        /// UI에서 await 하기 편한 비동기 래퍼.
        /// </summary>
        public Task<int> MoveByTeachingIndexAsync(int teachingIndex, bool isFine)
        {
            return Task.Run(() => MoveByTeachingIndex(teachingIndex, isFine));
        }

    }
}