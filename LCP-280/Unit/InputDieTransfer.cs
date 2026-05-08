using QMC.Common;
using QMC.Common.Alarm;
using QMC.Common.Cameras;
using QMC.Common.Component;
using QMC.Common.IOUtil;
using QMC.Common.Motion;
using QMC.Common.Motions;
using QMC.Common.Unit;
using QMC.LCP_280.Process.Component;
using QMC.LCP_280.Process.Unit.FormWork.Repro;
using QMC.LCP_280.Process.Work;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Pipes;
using System.Linq;
using System.Linq.Expressions;
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

        private readonly object _prepareNextDieSync = new object();
        private MaterialDie _preparedNextDie = null;
        public Task<int> taskPrepareNextDie = null;

        public new enum AlarmKeys
        {
            eInputStageNotSafety = 10401,
            eRotatyNotSafety,
            eInputStageEjectorPinZNotSafety,
            eInputStageEjectorZNotSafety,
            eInputStageAxesMoving,
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
            eInputDieTransferLdPickAsMiss,
            eInputStageMoveFaile,
            eInputDieTransferRecheckDieAndAlign,
            eInputDieTransferRotateMoveToSocket,
        }
        #region InitAlarm
        protected override void InitAlarm()
        {
            string source = "Wafer_Arm";
            base.InitAlarm();
            // 1. 공용 파일 로더에서 알람 목록 가져오기
            var loadedAlarms = GlobalAlarmTable.Instance.GetAlarmsForSource(source);
            if (loadedAlarms == null || loadedAlarms.Count == 0)
            {
                Log.Write("AlarmInit", $"Cannot find alarms for source '{source}' in the alarm file. Only default alarms will be registered.");

                AlarmInfo alarm = new AlarmInfo();
                alarm.Code = (int)AlarmKeys.eInputStageNotSafety;
                alarm.Title = "InputStage Not safety Pos.";
                alarm.Cause = "InputStage is not in a safety position. Please check the position and restart.";
                alarm.Source = source;// this.UnitName;
                alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
                m_dicAlarms.Add(alarm.Code, alarm);

                alarm = new AlarmInfo();
                alarm.Code = (int)AlarmKeys.eRotatyNotSafety;
                alarm.Title = "Rotaty Not safety Pos.";
                alarm.Cause = "Rotary is not in a safety position. Please check the position and restart.";
                alarm.Source = source;// this.UnitName;
                alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
                m_dicAlarms.Add(alarm.Code, alarm);

                //,
                alarm = new AlarmInfo();
                alarm.Code = (int)AlarmKeys.eInputStageEjectorPinZNotSafety;
                alarm.Title = "EjectorPin Z-Axis Not safety Pos.";
                alarm.Cause = "EjectorPin Z-Axis is not in a safety position. Please check the position and restart.";
                alarm.Source = source;// this.UnitName;
                alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
                m_dicAlarms.Add(alarm.Code, alarm);
                //,
                alarm = new AlarmInfo();
                alarm.Code = (int)AlarmKeys.eInputStageEjectorZNotSafety;
                alarm.Title = "Ejector Z-Axis Not safety Pos.";
                alarm.Cause = "Ejector Z-Axis is not in a safety position. Please check the position and restart.";
                alarm.Source = source;// this.UnitName;
                alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
                m_dicAlarms.Add(alarm.Code, alarm);

                alarm = new AlarmInfo();
                alarm.Code = (int)AlarmKeys.eInputStageAxesMoving;
                alarm.Title = "InputStage Axis Moving";
                alarm.Cause = "InputStage axis is moving. Please stop and try again.";
                alarm.Source = source;// this.UnitName;
                alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
                m_dicAlarms.Add(alarm.Code, alarm);

                alarm = new AlarmInfo();
                alarm.Code = (int)AlarmKeys.eRotaryAxesMoving;
                alarm.Title = "Rotary Axis Moving";
                alarm.Cause = "Rotary axis is moving. Please stop and try again.";
                alarm.Source = source;// this.UnitName;
                alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
                m_dicAlarms.Add(alarm.Code, alarm);

                alarm = new AlarmInfo();
                alarm.Code = (int)AlarmKeys.eInputStageEjectorAxesMoving;
                alarm.Title = "Ejector Axis Moving";
                alarm.Cause = "InputStageEjector axis is moving. Please stop and try again.";
                alarm.Source = source;// this.UnitName;
                alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
                m_dicAlarms.Add(alarm.Code, alarm);


                alarm = new AlarmInfo();
                alarm.Code = (int)AlarmKeys.eInputDieTransferError;
                alarm.Title = "InputDieTransferError";
                alarm.Cause = "An unexpected error occurred during the InputDieTransfer command. Please contact the administrator.";
                alarm.Source = source;// this.UnitName;
                alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
                m_dicAlarms.Add(alarm.Code, alarm);

                //eInputStageVaccum
                alarm = new AlarmInfo();
                alarm.Code = (int)AlarmKeys.eInputStageVaccum;
                alarm.Title = "eInputStageVaccumError";
                alarm.Cause = "eInputStageVaccum error occurred. Please check the pneumatic pressure.";
                alarm.Source = source;// this.UnitName;
                alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
                m_dicAlarms.Add(alarm.Code, alarm);

                //
                alarm = new AlarmInfo();
                alarm.Code = (int)AlarmKeys.eInputDieTransferVaccum;
                alarm.Title = "InputDieTransferVaccumError";
                alarm.Cause = "InputDieTransferVaccum error occurred. Please check the pneumatic pressure.";
                alarm.Source = source;// this.UnitName;
                alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
                m_dicAlarms.Add(alarm.Code, alarm);

                alarm = new AlarmInfo();
                alarm.Code = (int)AlarmKeys.eInputDieTransferNotSafety;
                alarm.Title = "InputDieTransfer Not safety Pos.";
                alarm.Cause = "InputDieTransfer is not in a safety position. Please check the position and restart.";
                alarm.Source = source;// this.UnitName;
                alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
                m_dicAlarms.Add(alarm.Code, alarm);

                alarm = new AlarmInfo();
                alarm.Code = (int)AlarmKeys.eInputDieTransferMoveFail;
                alarm.Title = "InputDieTransfer Move Fail";
                alarm.Cause = "InputDieTransfer failed to move to the commanded position. Please check the position and restart.";
                alarm.Source = source;// this.UnitName;
                alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
                m_dicAlarms.Add(alarm.Code, alarm);

                alarm = new AlarmInfo();
                alarm.Code = (int)AlarmKeys.eInputDieTransferNoWafer;
                alarm.Title = "InputDieTransfer No Wafer Detected";
                alarm.Cause = "InputDieTransfer failed to detect a wafer. Please check the position and restart.";
                alarm.Source = source;// this.UnitName;
                alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
                m_dicAlarms.Add(alarm.Code, alarm);

                alarm = new AlarmInfo();
                alarm.Code = (int)AlarmKeys.eInputDieTransferPrepareNextDie;
                alarm.Title = "InputDieTransfer Prepare Next Die";
                alarm.Cause = "A problem occurred while InputDieTransfer was preparing the next die. Please check the position and restart.";
                alarm.Source = source;// this.UnitName;
                alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
                m_dicAlarms.Add(alarm.Code, alarm);

                alarm = new AlarmInfo();
                alarm.Code = (int)AlarmKeys.eInputDieTransferRaiseEjector;
                alarm.Title = "InputDieTransfer Raise Ejector Fail";
                alarm.Cause = "A problem occurred while InputDieTransfer was raising the Ejector. Please check the position and restart.";
                alarm.Source = source;// this.UnitName;
                alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
                m_dicAlarms.Add(alarm.Code, alarm);

                alarm = new AlarmInfo();
                alarm.Code = (int)AlarmKeys.eInputDieTransferChipPickDown;
                alarm.Title = "InputDieTransfer Chip Pick Down Fail";
                alarm.Cause = "A problem occurred while InputDieTransfer was picking down the chip. Please check the position and restart.";
                alarm.Source = source;// this.UnitName;
                alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
                m_dicAlarms.Add(alarm.Code, alarm);

                alarm = new AlarmInfo();
                alarm.Code = (int)AlarmKeys.eInputDieTransferSyncPickPinUp;
                alarm.Title = "InputDieTransfer Sync Pick Pin Up Fail";
                alarm.Cause = "A problem occurred while InputDieTransfer was syncing the pick pin up. Please check the position and restart.";
                alarm.Source = source;// this.UnitName;
                alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
                m_dicAlarms.Add(alarm.Code, alarm);

                alarm = new AlarmInfo();
                alarm.Code = (int)AlarmKeys.eInputDieTransferSyncPickPinRetreat;
                alarm.Title = "InputDieTransfer Sync Pick Pin Retreat Fail";
                alarm.Cause = "A problem occurred while InputDieTransfer was syncing the pick pin retreat. Please check the position and restart.";
                alarm.Source = source;// this.UnitName;
                alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
                m_dicAlarms.Add(alarm.Code, alarm);

                alarm = new AlarmInfo();
                alarm.Code = (int)AlarmKeys.eInputDieTransferCommitPickedDie;
                alarm.Title = "InputDieTransfer Commit Picked Die Fail";
                alarm.Cause = "A problem occurred while InputDieTransfer was committing the picked die. Please check the position and restart.";
                alarm.Source = source;// this.UnitName;
                alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
                m_dicAlarms.Add(alarm.Code, alarm);

                alarm = new AlarmInfo();
                alarm.Code = (int)AlarmKeys.eInputDieTransferRotateToolTForPlace;
                alarm.Title = "InputDieTransfer Rotate Tool T For Place Fail";
                alarm.Cause = "A problem occurred while InputDieTransfer was rotating Tool T for place. Please check the position and restart.";
                alarm.Source = source;// this.UnitName;
                alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
                m_dicAlarms.Add(alarm.Code, alarm);

                alarm = new AlarmInfo();
                alarm.Code = (int)AlarmKeys.eInputDieTransferPlaceChipDown;
                alarm.Title = "InputDieTransfer Place Chip Down Fail";
                alarm.Cause = "A problem occurred while InputDieTransfer was placing down the chip. Please check the position and restart.";
                alarm.Source = source;// this.UnitName;
                alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
                m_dicAlarms.Add(alarm.Code, alarm);

                alarm = new AlarmInfo();
                alarm.Code = (int)AlarmKeys.eInputDieTransferReleaseVacuumAndPlaceUp;
                alarm.Title = "InputDieTransfer Release Vacuum And Place Up Fail";
                alarm.Cause = "A problem occurred while InputDieTransfer was releasing vacuum and placing up. Please check the position and restart.";
                alarm.Source = source;// this.UnitName;
                alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
                m_dicAlarms.Add(alarm.Code, alarm);

                alarm = new AlarmInfo();
                alarm.Code = (int)AlarmKeys.eInputDieTransferNoDie;
                alarm.Title = "InputDieTransfer No Die Detected";
                alarm.Cause = "InputDieTransfer failed to detect a die. Please check the position and restart.";
                alarm.Source = source;// this.UnitName;
                alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
                m_dicAlarms.Add(alarm.Code, alarm);

                //eInputDieTransferLdPickAsMiss
                alarm = new AlarmInfo();
                alarm.Code = (int)AlarmKeys.eInputDieTransferLdPickAsMiss;
                alarm.Title = "InputDieTransfer Loadport Pick As Miss";
                alarm.Cause = "InputDieTransfer detected a miss during pick at the loadport. Please check the position and restart.";
                alarm.Source = source;// this.UnitName;
                alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
                m_dicAlarms.Add(alarm.Code, alarm);

                //eInputStageMoveFaile
                alarm = new AlarmInfo();
                alarm.Code = (int)AlarmKeys.eInputStageMoveFaile;
                alarm.Title = "InputStage Move Fail";
                alarm.Cause = "InputStage failed to move to the commanded position. Please check the position and restart.";
                alarm.Source = source;// this.UnitName;
                alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
                m_dicAlarms.Add(alarm.Code, alarm);

                alarm = new AlarmInfo();
                alarm.Code = (int)AlarmKeys.eInputDieTransferRecheckDieAndAlign;
                alarm.Title = "InputDieTransfer Recheck Die And Align Fail";
                alarm.Cause = "A problem occurred while InputDieTransfer was rechecking the die and aligning. Please check the position and restart.";
                alarm.Source = source;// this.UnitName;
                alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
                m_dicAlarms.Add(alarm.Code, alarm);

                alarm = new AlarmInfo();
                alarm.Code = (int)AlarmKeys.eInputDieTransferRotateMoveToSocket;
                alarm.Title = "InputDieTransfer Rotate Move To Socket Fail";
                alarm.Cause = "A problem occurred while InputDieTransfer was rotating and moving to the socket. Please check the position and restart.";
                alarm.Source = source;// this.UnitName;
                alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
                m_dicAlarms.Add(alarm.Code, alarm);

            }
            else
            {
                // 2. m_dicAlarms에 일괄 등록
                foreach (var alarmInfo in loadedAlarms)
                {
                    if (!m_dicAlarms.ContainsKey(alarmInfo.Code))
                    {
                        m_dicAlarms.Add(alarmInfo.Code, alarmInfo);
                    }
                }
            }


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

        // 연속 Miss 카운트
        private int _consecutiveLdPickMissCount1 = 0;
        private int _consecutiveLdPickMissCount2 = 0;

        // N회 연속 시 알람 (원하면 Config로 빼세요)
        private int LdPickMissAlarmThreshold = 3;


        private readonly object _oneCycleTaktSync = new object();
        private bool _oneCycleTaktOpened = false;
        private void BeginOneCycleIfNeeded()
        {
            lock (_oneCycleTaktSync)
            {
                if (_oneCycleTaktOpened)
                {
                    return;
                }

                TaktStart("One Cycle");
                _oneCycleTaktOpened = true;
            }
        }

        private void EndOneCycleIfNeeded()
        {
            lock (_oneCycleTaktSync)
            {
                if (!_oneCycleTaktOpened)
                {
                    return;
                }

                TaktEnd("One Cycle");
                _oneCycleTaktOpened = false;
            }
        }

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
                    Log.Write(UnitName, nameof(IsInterlockOK), $"Rotary moving interlock. {Rotary.GetIndexMovingDebugText()}");
                    return false;
                }
            }
            else if (baseComponent == this.AxisToolT)
            {
                if (this.IsPositionPlaceZSafety() == false || this.IsPositionPickZSafety() == false)
                {
                    nRet = MovePositionSafetyZ();
                    if (nRet != 0)
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
                if (axis == AxisPickZ)
                {
                    if (InputStage.IsAnyAxisMoving())
                    {
                        AxisToolT.EmgStop();
                        AxisPickZ.EmgStop();
                        AxisPlaceZ.EmgStop();
                        PostAlarm((int)AlarmKeys.eInputStageAxesMoving);
                        Log.Write(UnitName, nameof(MoveAxisPositionOne), $"Rotary moving interlock. {Rotary.GetIndexMovingDebugText()}");
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
                        Log.Write(UnitName, nameof(MoveAxisPositionOne), $"Rotary moving interlock. {Rotary.GetIndexMovingDebugText()}");
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
                return MoveTeachingPositionOnce((int)InputDieTransferRecipe.TeachingPositionName.SafetyZone, isFine);
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

                    Thread.Sleep(1); // 0→5ms로 약간 여유 (CPU 점유 감소)
                }

                return coreTask.Result;
            },
            ct);
        }

        ///////////////////////////////////////////////////////////////////////////////////////////
        // Single Pickup (Non-Index) - 구조 통일 (Index 기반 메서드 패턴과 동일 스타일)
        // 클래스 내부(해당 Region 근처)에 추가
        public enum PickDownMoveMode
        {
            Sequential = 0, // 기존과 동일: T -> Z
            ToolTOnly = 1,  // T만 이동
            PickZOnly = 2,  // Z만 이동
            Parallel = 3    // T/Z 동시 이동
        }

        // 기존 메서드 교체
        public int MovePositionPickDown(bool isFine = false, PickDownMoveMode mode = PickDownMoveMode.Sequential)
        {
            Task<int> task = MovePositionAsyncPickDown(isFine, mode);
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

        private Task<int> MovePositionAsyncPickDown(bool isFine = false, PickDownMoveMode mode = PickDownMoveMode.Sequential)
        {
            return Task.Run(() =>
            {
                return OnMovePositionPickDown(isFine, mode);
            });
        }

        private int OnMovePositionPickDown(bool isFine = false, PickDownMoveMode mode = PickDownMoveMode.Sequential)
        {
            // 안전 Z 위치 확인 후 필요 시 이동
            if (!IsPositionPlaceZSafety() || !IsPositionPickZSafety())
            {
                int safety = MovePositionSafetyZ(isFine);
                if (safety != 0)
                    return -1;
            }

            switch (mode)
            {
                case PickDownMoveMode.ToolTOnly:
                    {
                        if (IsPositionToolTPickup())
                            return 0;

                        return MoveToolT_ToPickup(isFine);
                    }
                case PickDownMoveMode.PickZOnly:
                    {
                        if (IsPositionPickZPickup())
                            return 0;

                        return MovePickZ_ToPickup(isFine);
                    }
                case PickDownMoveMode.Sequential:
                    {
                        int r = 0;
                        if (IsPositionToolTPickup() == false)
                        {
                            r = MoveToolT_ToPickup(isFine);
                            if (r != 0)
                                return -1;
                        }

                        r = MovePickZ_ToPickup(isFine);
                        if (r != 0)
                            return -1;

                        return 0;
                    }

                case PickDownMoveMode.Parallel:
                    {
                        Task<int> tTask = Task.Run(() => MoveToolT_ToPickup(isFine));
                        Task<int> zTask = Task.Run(() => MovePickZ_ToPickup(isFine));

                        while (!tTask.IsCompleted || !zTask.IsCompleted)
                        {
                            int interlock = IsMoveInterLockPickDown();
                            if (interlock != 0)
                                return -1;

                            Thread.Sleep(1);
                        }

                        if (tTask.Result != 0 || zTask.Result != 0)
                            return -1;

                        return 0;
                    }

                default:
                    return -1;
            }
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
            double target = GetTP(InputDieTransferRecipe.TeachingPositionName.Pickup.ToString(),
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
            double target = GetTP(InputDieTransferRecipe.TeachingPositionName.Pickup.ToString(),
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
            double target = GetTP(InputDieTransferRecipe.TeachingPositionName.Pickup.ToString(),
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

                    Thread.Sleep(1); // 0→5ms로 약간 여유 (CPU 점유 감소)
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

            double dTPos = GetTP(InputDieTransferRecipe.TeachingPositionName.Ready.ToString(),
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

            if (IsPositionPickZSafety() == false
                || IsPositionPlaceZSafety() == false)
            {
                AxisToolT?.EmgStop();
                AxisPickZ?.EmgStop();
                AxisPlaceZ?.EmgStop();
                PostAlarm((int)AlarmKeys.eInputStageNotSafety);
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

                    Thread.Sleep(1); // 0→5ms로 약간 여유 (CPU 점유 감소)
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
                Log.Write(UnitName, nameof(IsMoveInterLockPlace_Index), $"Rotary moving interlock. {Rotary.GetIndexMovingDebugText()}");
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
        public int MovePickZAndPinZByOffset(int timeoutMs = 5000, bool isFine = false)
        {
            var pickZ = AxisPickZ;
            var Niddlepin = InputStageEjector != null ? InputStageEjector.AxisPinZ : null;

            if (pickZ == null || Niddlepin == null)
            {
                Log.Write(UnitName, "[MovePickZAndPinZByOffset] Axis null");
                return -1;
            }

            // 이동 필요 없으면 즉시 성공 -> 이거 자체가 이상하다.
            //if (System.Math.Abs(pickZOffset) < 1e-9 && System.Math.Abs(pinZOffset) < 1e-9)
            //    return 0;

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

            double pickZOffset = this.Config.dPickUpOffset;
            double vPick = this.Config.dPickUpSpeed;
            double aPick = this.Config.dPickUpAcc;
            double dPick = this.Config.dPickUpDec;

            double pinZOffset = InputStageEjector.Config.dPickUpOffset;
            double vPin = InputStageEjector.Config.dPickUpSpeed;
            double aPin = InputStageEjector.Config.dPickUpAcc;
            double dPin = InputStageEjector.Config.dPickUpDec;

            // 시작 위치 저장 -> 목표 위치 계산
            double pickStart = pickZ.GetPosition();
            double pinStart = Niddlepin.GetPosition();
            double pickTarget = pickStart + pickZOffset;
            double pinTarget = pinStart + pinZOffset;

            // 동시에 시작 (반환코드 OR)
            //ex) Offset값이 양수로 300 이면 Z축이 위로 300 이동
            // 두 개의 축 전부 300이면 동일하게 위로 올라간다.
            int rc = 0;
            rc |= pickZ.MoveRel(pickZOffset, vPick, aPick, dPick, pickZ.Config.AccJerkPercent);
            rc |= Niddlepin.MoveRel(pinZOffset, vPin, aPin, dPin, Niddlepin.Config.AccJerkPercent);
            if (rc != 0)
            {
                Log.Write(UnitName, "[MovePickZAndPinZByOffset] MoveRel start failed rc=" + rc);
                return -1;
            }

            var sw = timeoutMs > 0 ? Stopwatch.StartNew() : null;
            while (true)
            {
                // 1) 드라이브 Done (완전 정지)
                bool pickMoving = pickZ.IsMoveDone();
                bool pinMoving = Niddlepin.IsMoveDone();

                // 2) InPosition (목표 위치 정밀 도달)
                bool pickInPos = false;
                bool pinInPos = false;
                try
                {
                    pickInPos = pickZ.InPosition(pickTarget);
                    pinInPos = Niddlepin.InPosition(pinTarget);
                }
                catch (Exception ex)
                {
                    Log.Write(ex);
                    pickZ.EmgStop();
                    Niddlepin.EmgStop();
                    return -1;
                }

                // 두 조건 모두 만족 시에만 탈출
                if (pickMoving && pinMoving && pickInPos && pinInPos)
                {
                    break;
                }
                // 타임아웃
                if (sw != null && sw.ElapsedMilliseconds > timeoutMs)
                {
                    pickZ.EmgStop();
                    Niddlepin.EmgStop();
                    PostAlarm((int)AlarmKeys.eInputDieTransferReleaseVacuumAndPlaceUp);
                    Log.Write(UnitName, "[MovePickZAndPinZByOffset] Timeout");
                    return -1;
                }

                Thread.Sleep(2);
            }

            return 0;
        }
        #endregion

        #region Position Check (Pickup / Ready / Place Index)
        /// DieTransfer PickZ 축이 SafetyPos Teaching (Offset 적용) 위치(또는 허용오차 범위)인지 확인.
        /// Teaching 이름이 SafetyPos 없으면 SafetyZone 순으로 fallback (둘 다 없으면 false).
        /// 장치/축이 없으면 true(안전)로 간주. 필요 시 treatMissingAsSafe=false 로 변경 가능.
        public bool IsPositionPickZSafety()
        {
            const string tpName = nameof(InputDieTransferRecipe.TeachingPositionName.SafetyZone);
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
            const string tpName = nameof(InputDieTransferRecipe.TeachingPositionName.SafetyZone);
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
            const string tpName = nameof(InputDieTransferRecipe.TeachingPositionName.Pickup);
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

        public bool IsPositionPickZPickup()
        {
            const string tpName = nameof(InputDieTransferRecipe.TeachingPositionName.Pickup);
            if (AxisPickZ == null) return true;
            double target = GetTP(tpName, AxisNames.LeftPickZ);
            try { return AxisPickZ.InPosition(target); } catch { return false; }
        }

        // ---- Ready ----
        public bool IsPositionToolTReady()
        {
            const string tpName = nameof(InputDieTransferRecipe.TeachingPositionName.Ready);
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
            const string tpName = nameof(InputDieTransferRecipe.TeachingPositionName.Ready);
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

            var tp = GetTeachingPosition(tpName);
            if (tp == null) return false;

            double target = GetTP(tpName, AxisNames.LeftToolT);
            try { return AxisToolT.InPosition(target); } catch { return false; }
        }
        public bool IsPositionPlaceZPlaceIndex(int nIndex)
        {
            if (AxisPlaceZ == null) return true;
            if (!TryGetPlaceTeachingName(nIndex, out string tpName)) return false;

            var tp = GetTeachingPosition(tpName);
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
        public int MoveToTeachingPosition(string positionName, bool isFine)
        {
            if (string.IsNullOrWhiteSpace(positionName))
            {
                Log.Write(UnitName, nameof(MoveToTeachingPosition),
                        $"[TeachingMove] TeachingPositions에서 '{positionName}' 을 찾지 못했습니다.");
                return -1;
            }

            int result = 0;

            InputDieTransferRecipe.TeachingPositionName en;
            if (Enum.TryParse(positionName, out en))
            {
                int selIndex = FindTeachingSelectionIndex(positionName);
                if (selIndex >= 0)
                {
                    result = MoveToTeachingPositionBySelectionIndex(selIndex, isFine);
                }
                else
                {
                    Log.Write(UnitName, nameof(MoveToTeachingPosition),
                        $"[TeachingMove] TeachingPositions에서 '{positionName}' index를 찾지 못했습니다.");
                    return -1;
                }
            }

            return result;
        }

        private int FindTeachingSelectionIndex(string positionName)
        {
            try
            {
                var list = GetTeachingList();
                if (list == null)
                    return -1;

                for (int i = 0; i < list.Count; i++)
                {
                    var tp = list[i];
                    if (tp != null && string.Equals(tp.Name, positionName, StringComparison.OrdinalIgnoreCase))
                        return i;
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
            return -1;
        }

        private IList<TeachingPosition> GetTeachingList()
        {
            var r = Config?.TeachingRecipe;
            if (r?.TeachingPositions != null)
                return r.TeachingPositions;

            return Config?.TeachingPositions ?? new List<TeachingPosition>();
        }

        public override double GetTP(string tpName, string axisName)
        {
            try
            {
                var recipe = Config?.TeachingRecipe;
                var tp = recipe?.Get(tpName);
                if (tp != null && tp.AxisPositions != null && tp.AxisPositions.TryGetValue(axisName, out var v))
                    return v;

                // fallback: 기존 방식 (Config)
                return base.GetTP(tpName, axisName);
            }
            catch
            {
                return base.GetTP(tpName, axisName);
            }
        }
        private TeachingPosition GetTeachingPosition(string tpName)
        {
            var r = Config?.TeachingRecipe;
            if (r != null)
                return r.Get(tpName);

            // 혹시라도 TeachingRecipe가 null인 비정상 상태 대비(호환/안전)
            return Config.GetTeachingPosition(tpName);
        }

        public override bool InPosTeaching(string positionName)
        {
            var recipe = Config?.TeachingRecipe;
            var tp = recipe?.Get(positionName);
            if (tp == null)
                return base.InPosTeaching(positionName);

            foreach (var kv in tp.AxisPositions)
            {
                if (!Axes.TryGetValue(kv.Key, out var axis))
                    return false;

                // BaseUnit의 Teaching 완화 판정 재사용
                // (InPosTeachingAxis가 protected라면 여기에서 사용 가능)
                if (!InPos(axis, kv.Value) && !axis.InPosition(kv.Value))
                    return false;
            }
            return true;
        }


        //public void ApplyOffset(string name, double t, double pickZ, double placeZ)
        //{
        //    return Config.SetOffset(name, t, pickZ, placeZ);
        //}
        #endregion

        #region Arm Vacuum / Blow / Vent Control
        private static readonly string[] VAC_NAMES = { InputDieTransferConfig.IO.ARM1_VAC, InputDieTransferConfig.IO.ARM2_VAC, InputDieTransferConfig.IO.ARM3_VAC, InputDieTransferConfig.IO.ARM4_VAC };
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

            var equipment = Equipment.Instance;
            bool IsDryRunEqp = equipment.EquipmentConfig.IsDryRun;
            if (Config.IsSimulation || (Config.IsDryRun || IsDryRunEqp))
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

            Thread.Sleep(1);
            return true;
        }
        public bool SetBlow(int nNo, bool on)
        {
            if (_blow[nNo] == null)
                return false;

            if (on) _blow[nNo].On();
            else _blow[nNo].Off();

            Thread.Sleep(1);
            return true;
        }
        public bool SetVent(int nNo, bool on)
        {
            if (_vent[nNo] == null) return false;
            if (on) _vent[nNo].On();
            else _vent[nNo].Off();

            Thread.Sleep(1);
            return true;
        }
        public bool AirTankPressureOk() => this.ReadInput(InputDieTransferConfig.IO.AIR_TANK_PRESSURE);
        public bool VacTankPressureOk() => this.ReadInput(InputDieTransferConfig.IO.VAC_TANK_PRESSURE);
        public bool IsVacuumOK(int armIndex)
        {
            var equipment = Equipment.Instance;
            bool IsDryRunEqp = equipment.EquipmentConfig.IsDryRun;
            if (Config.IsSimulation || (Config.IsDryRun || IsDryRunEqp))
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


        public bool EjectorVacuumOn(int timeoutMs = 1000)
        {
            if (InputStageEjector == null)
                return false;

            bool bRet = false;
            bool bVacuumOn = false;
            bRet = InputStage.SetVacuum(true);

            var equipment = Equipment.Instance;
            bool IsDryRunEqp = equipment.EquipmentConfig.IsDryRun;
            if (Config.IsSimulation || (Config.IsDryRun || IsDryRunEqp))
            {
                Thread.Sleep(5);
                return true;
            }
            //VacuumOn은 여기서 확인 불가. Ejector 상승 후에 확인.
            //var sw = System.Diagnostics.Stopwatch.StartNew();
            //while (bRet)
            //{
            //    bVacuumOn = InputStage.IsVacuumOn();
            //    if (bVacuumOn == false)
            //    {
            //        break;
            //    }

            //    if (sw.ElapsedMilliseconds > timeoutMs)
            //    {
            //        break;
            //    }
            //    Thread.Sleep(1);
            //}
            bVacuumOn = bRet;
            return bVacuumOn;
        }
        private bool EjectorVaccumOff(int timeoutMs = 1000)
        {
            if (InputStageEjector == null)
                return false;

            bool bRet = false;
            bRet = InputStage.SetVacuum(false);
            var equipment = Equipment.Instance;
            bool IsDryRunEqp = equipment.EquipmentConfig.IsDryRun;
            if (Config.IsSimulation || (Config.IsDryRun || IsDryRunEqp))
            {
                Thread.Sleep(5);
                return true;
            }

            bool bVacuumOn = false;
            var sw = System.Diagnostics.Stopwatch.StartNew();
            while (bRet)
            {
                bVacuumOn = InputStage.IsVacuumOn();
                if (bVacuumOn == false)
                {
                    break;
                }

                if (sw.ElapsedMilliseconds > timeoutMs)
                {
                    break;
                }
                Thread.Sleep(1);
            }

            return !bVacuumOn;
        }
        // === Arm Vacuum 상태 대기 공용 유틸 ===
        // expectOn: true=ON 될 때까지, false=OFF 될 때까지 대기
        // timeoutMs/pollMs: 타임아웃/폴링 간격
        private int WaitEjectorVacuumOnStateOrAlarm(int timeoutMs = 1000, int pollMs = 1)
        {
            int nRet = 0;
            var equipment = Equipment.Instance;
            bool IsDryRunEqp = equipment.EquipmentConfig.IsDryRun;
            if (Config.IsSimulation || (Config.IsDryRun || IsDryRunEqp))
                return nRet;

            //Todo: 2025-10-10 GYN: Vacuum 해결 되면 return 지우기.
            //return nRet;

            bool ok = false;
            var sw = Stopwatch.StartNew();
            timeoutMs = 10000;
            while (true)
            {
                ok = InputStage.IsVacuumOn();
                if (ok == true)
                {
                    nRet = 0;
                    break;
                }

                if (sw.ElapsedMilliseconds > timeoutMs)
                {
                    nRet = -1;
                    break;
                }

                Thread.Sleep(pollMs);
            }

            return nRet;
        }


        /// //////////////////////////////////////////////////////////////////
        #endregion


        #region Seq Signals
        //private MaterialDie _WaferDie;         // PrepareNextDie에서 보관, CommitPickedDie에서 사용
        #endregion

        #region Lifecycle
        public override int OnRun()
        {
            int ret = 0;
            try
            {
                if (this.RunUnitStatus != UnitStatus.AutoRunning)
                {
                    this.State = ProcessState.Stop;
                    return 0;
                }

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
        // Previous log tracking for OnRunReady
        private string _lastOnRunReadyLog = string.Empty;
        protected override int OnRunReady()
        {
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
                    hasNextDie = this.InputStage.HasNextDie();
                }
                catch (Exception ex)
                {
                    Log.Write(ex);
                }

                // 1. 현재 상태를 문자열로 먼저 생성합니다.
                string currentLog =
                    $"State={State}, " +
                    $"Held={(held == null ? "null" : $"Exist={held.Presence},State={held.State},Map=({held.MapX:F3},{held.MapY:F3})")}, " +
                    $"Wafer={(wafer == null ? "null" : $"Exist={wafer.Presence},Proc={wafer.ProcessSatate},Id={wafer.WaferId}")}, " +
                    $"ChipMappingDone={chipMappingDone}, " +
                    $"HasNextDie={hasNextDie}";

                // 2. 이전 로그와 다를 때만 로그를 기록하고, 현재 로그를 저장합니다.
                if (_lastOnRunReadyLog != currentLog)
                {
                    Log.Write(UnitName, nameof(OnRunReady), currentLog);
                    _lastOnRunReadyLog = currentLog;
                }
                // ===== 우선순위 1) 이미 들고 있는 다이가 있으면 Place 진행 가능하도록 Work로 진입 =====
                // (웨이퍼 언로드되어도 DieTransfer는 들고 있는 다이를 로터리에 내려놓아야 함)
                //if (held != null 
                //    && held.Presence == Material.MaterialPresence.Exist)
                //{
                //    State = ProcessState.Work;
                //    return 0;
                //}
                // ===== 우선순위 2) 스테이지에서 픽업할 다이가 준비되어 있을 때만 Work 진입 =====
                if (wafer != null
                    && wafer.Presence == Material.MaterialPresence.Exist
                    && wafer.ProcessSatate == Material.MaterialProcessSatate.Processing
                    && chipMappingDone
                    && hasNextDie)
                {
                    State = ProcessState.Work;
                    nRet = 0;   //return 0;
                }
                else if (IsPositionToolTReady() == false) // ===== 그 외: Ready 자세 유지 =====
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

            }
            catch (Exception ex)
            {
                Log.Write(ex);
                return -1;
            }
        }

        protected override int OnRunWork()
        {
            try
            {
                int nRet = InputTrDiePick();
                if (nRet != 0)
                {
                    Log.Write(UnitName, "OnRunWork", "[OnRunWork] InputTrDiePick failed");
                    return -1;
                }
                return nRet;
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                return -1;
            }
        }

        public int InputTrDiePick(bool bSpeed = false)
        {
            int nRet = 0;
            bool handoffToPlace = false;

            try
            {
                int nArmIndex = GetInputTrArmIndex();
                MaterialDie dieTr = this.GetMaterial() as MaterialDie;

                BeginOneCycleIfNeeded();

                TaktStart("SetVacuum_On");
                try
                {
                    SetVacuum(nArmIndex, true);
                    Thread.Sleep(1);
                }
                finally
                {
                    TaktEnd("SetVacuum_On");
                }

                // 이미 다이를 들고 있으면 Pick 구간 스킵하고 Place 진행
                if (dieTr != null && this.IsVacuumOK(nArmIndex))
                {
                    TaktStart("PlaceDie_ToolT");
                    try
                    {
                        nRet = PlaceDie_ToolT();
                    }
                    finally
                    {
                        TaktEnd("PlaceDie_ToolT");
                    }

                    if (nRet != 0)
                    {
                        AxisPickZ?.EmgStop();
                        AxisToolT?.EmgStop();
                        PostAlarm((int)AlarmKeys.eInputDieTransferRotateToolTForPlace);
                        Log.Write(UnitName, nameof(InputTrDiePick), "[InputTrDiePick] RotateToolTForPlace failed");
                        return -1;
                    }

                    State = ProcessState.Complete;
                    handoffToPlace = true;
                    return 0;
                }

                TaktStart("PickUp_Wait");
                try
                {
                    if (InputStage.IsRingPresent() == false && InputStage.IsPlateUp() == false)
                    {
                        AxisPickZ?.EmgStop();
                        AxisToolT?.EmgStop();
                        PostAlarm((int)AlarmKeys.eInputStageVaccum);
                        Log.Write(UnitName, nameof(InputTrDiePick), "[InputTrDiePick] Wafer None failed");
                        return -1;
                    }

                    MaterialWafer waferStage = this.InputStage.GetMaterialWafer();
                    if (waferStage == null)
                    {
                        Log.Write(UnitName, nameof(InputTrDiePick), "[InputTrDiePick] wafer is null");
                        PostAlarm((int)AlarmKeys.eInputDieTransferNoWafer);
                        return -1;
                    }

                    if (this.RunUnitStatus == UnitStatus.ManualRunning)
                    {
                        if (waferStage.Presence != Material.MaterialPresence.Exist)
                        {
                            Log.Write(UnitName, nameof(InputTrDiePick), "[InputTrDiePick] wafer not exist");
                            return -1;
                        }

                        if (waferStage.ProcessSatate != Material.MaterialProcessSatate.Processing)
                        {
                            Log.Write(UnitName, nameof(InputTrDiePick), "[InputTrDiePick] wafer not processing");
                            return -1;
                        }

                        if (this.InputStage.HasNextDie() == false)
                        {
                            Log.Write(UnitName, nameof(InputTrDiePick), "[InputTrDiePick] no next die");
                            return -1;
                        }

                        nRet = MovePositionReady();
                        if (nRet != 0)
                        {
                            AxisPickZ?.EmgStop();
                            AxisToolT?.EmgStop();
                            PostAlarm((int)AlarmKeys.eInputDieTransferNotSafety);
                            Log.Write(UnitName, nameof(InputTrDiePick), "[InputTrDiePick] MovePositionReady failed");
                            return -1;
                        }

                        nRet = RecheckDieAndAlign();
                        if (nRet != 0)
                        {
                            AxisToolT?.EmgStop();
                            PostAlarm((int)AlarmKeys.eInputDieTransferRecheckDieAndAlign);
                            Log.Write(UnitName, nameof(InputTrDiePick), "[InputTrDiePick] RecheckDieAndAlign failed");
                            return -1;
                        }
                    }
                    else
                    {
                        if (waferStage.Presence != Material.MaterialPresence.Exist)
                        {
                            Log.Write(UnitName, nameof(InputTrDiePick), "[InputTrDiePick] wafer not exist");
                            return 0;
                        }

                        if (waferStage.ProcessSatate != Material.MaterialProcessSatate.Processing)
                        {
                            Log.Write(UnitName, nameof(InputTrDiePick), "[InputTrDiePick] wafer not processing");
                            return 0;
                        }

                        if (this.InputStage.HasNextDie() == false)
                        {
                            Log.Write(UnitName, nameof(InputTrDiePick), "[InputTrDiePick] no next die");
                            return 0;
                        }
                    }
                }
                finally
                {
                    TaktEnd("PickUp_Wait");
                }

                //TaktStart("PositionToolTPickup");
                //try
                //{
                //    if (IsPositionToolTPickup() == false)
                //    {
                //        nRet = MovePositionPickDown(true, PickDownMoveMode.ToolTOnly);
                //        if (nRet != 0)
                //        {
                //            AxisPickZ?.EmgStop();
                //            AxisToolT?.EmgStop();
                //            PostAlarm((int)AlarmKeys.eInputDieTransferChipPickDown);
                //            Log.Write(UnitName, nameof(InputTrDiePick), "[InputTrDiePick] MovePositionPickDown failed");
                //            return -1;
                //        }
                //    }
                //}
                //finally
                //{
                //    TaktEnd("PositionToolTPickup");
                //}

                TaktStart("PrepareNextDie");
                try
                {
                    Task<int> pendingTask = null;
                    bool hasPreparedDie = false;
                    lock (_prepareNextDieSync)
                    {
                        pendingTask = taskPrepareNextDie;
                        hasPreparedDie = (_preparedNextDie != null);
                    }

                    if (pendingTask != null)
                    {
                        nRet = WaitPrepareNextDieTaskOrAlarm(5000, true);
                        if (nRet != 0)
                        {
                            Log.Write(UnitName, nameof(InputTrDiePick), "[InputTrDiePick] taskPrepareNextDie wait failed");
                            return -1;
                        }
                    }
                    else if (hasPreparedDie == false)
                    {
                        nRet = PrepareNextDie();
                        if (nRet != 0)
                        {
                            Log.Write(UnitName, nameof(InputTrDiePick), "[InputTrDiePick] PrepareNextDie failed");
                            return -1;
                        }
                    }
                }
                finally
                {
                    TaktEnd("PrepareNextDie");
                }

                //택타임 영향 없음.
                MaterialDie waferDie = null;
                waferDie = ConsumePreparedNextDieOrQuery();
                if (this.RunUnitStatus == UnitStatus.ManualRunning)
                {
                    if (waferDie == null || waferDie.Presence != Material.MaterialPresence.Exist)
                    {
                        State = ProcessState.None;
                        return -1;
                    }
                }
                if (waferDie == null || waferDie.Presence != Material.MaterialPresence.Exist)
                {
                    State = ProcessState.None;
                    return 0;
                }

                TaktStart("Wafer Pick Die");
                try
                {
                    bool shouldPickFromStage = false;
                    shouldPickFromStage = waferDie != null
                                            && waferDie.Presence == Material.MaterialPresence.Exist
                                            && waferDie.State != DieProcessState.Picked;
                    if (shouldPickFromStage)
                    {
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
                            return -1;
                        }

                        if (IsStop)
                            return 0;

                        TaktStart("PickDownDie");
                        try
                        {
                            nRet = PickDownDie();
                        }
                        finally
                        {
                            TaktEnd("PickDownDie");
                        }
                        if (nRet != 0)
                        {
                            AxisPickZ?.EmgStop();
                            AxisToolT?.EmgStop();
                            PostAlarm((int)AlarmKeys.eInputDieTransferChipPickDown);
                            return -1;
                        }

                        TaktStart("SyncPickUpDie");
                        try
                        {
                            nRet = SyncPickUpDie();
                        }
                        finally
                        {
                            TaktEnd("SyncPickUpDie");
                        }
                        if (nRet != 0)
                        {
                            AxisPickZ?.EmgStop();
                            AxisToolT?.EmgStop();
                            PostAlarm((int)AlarmKeys.eInputDieTransferSyncPickPinUp);
                            return -1;
                        }

                        TaktStart("SyncPickDieRetreat");
                        try
                        {
                            nRet = SyncPickDieRetreat();
                        }
                        finally
                        {
                            TaktEnd("SyncPickDieRetreat");
                        }
                        if (nRet != 0)
                        {
                            AxisPickZ?.EmgStop();
                            AxisToolT?.EmgStop();
                            PostAlarm((int)AlarmKeys.eInputDieTransferSyncPickPinRetreat);
                            return -1;
                        }

                        bool bRet;
                        TaktStart("AfterPick_EjectorVacuumOff");
                        try
                        {
                            bRet = EjectorVaccumOff();
                        }
                        finally
                        {
                            TaktEnd("AfterPick_EjectorVacuumOff");
                        }
                        if (bRet == false)
                        {
                            PostAlarm((int)AlarmKeys.eInputStageVaccum);
                            return -1;
                        }

                        TaktStart("AfterPick_VacuumCheck_Commit");
                        try
                        {
                            if (IsVacuumOK(nArmIndex))
                            {
                                nRet = CommitPickedDie();
                                if (nRet != 0)
                                {
                                    AxisPickZ?.EmgStop();
                                    AxisToolT?.EmgStop();
                                    PostAlarm((int)AlarmKeys.eInputDieTransferCommitPickedDie);
                                    return -1;
                                }
                                ResetLdPickMissCounter1("CommitPickedDie success");
                            }
                            else
                            {
                                TaktStart("AfterPick_VacuumFail_Recover");
                                try
                                {
                                    nRet = MovePositionReady();
                                    if (nRet != 0)
                                    {
                                        AxisPickZ?.EmgStop();
                                        AxisToolT?.EmgStop();
                                        PostAlarm((int)AlarmKeys.eInputDieTransferNotSafety);
                                        return -1;
                                    }

                                    SetVacuum(nArmIndex, false);
                                    Thread.Sleep(1);
                                    SetBlow(nArmIndex, true);
                                    Thread.Sleep(100);
                                    SetBlow(nArmIndex, false);
                                    Thread.Sleep(1);
                                    SetVacuum(nArmIndex, true);

                                    nRet = CommitNotPickedDie();
                                    this.SetMaterial(null);
                                    this.State = ProcessState.Ready;

                                    var ctx = Equipment.Instance.SummaryContext;
                                    ctx.GetCurrentSummaryOrNull()?.AddLdPickAsMiss();

                                    nRet = OnLdPickMissDetected1();
                                    if (nRet != 0)
                                    {
                                        return -1;
                                    }

                                    return 0;
                                }
                                finally
                                {
                                    TaktEnd("AfterPick_VacuumFail_Recover");
                                }
                            }
                        }
                        finally
                        {
                            TaktEnd("AfterPick_VacuumCheck_Commit");
                        }
                    }

                    TaktStart("PlaceDie_ToolT");
                    try
                    {
                        nRet = PlaceDie_ToolT();
                    }
                    finally
                    {
                        TaktEnd("PlaceDie_ToolT");
                    }
                    if (nRet != 0)
                    {
                        AxisPickZ?.EmgStop();
                        AxisToolT?.EmgStop();
                        PostAlarm((int)AlarmKeys.eInputDieTransferRotateToolTForPlace);
                        return -1;
                    }
                }
                finally
                {
                    TaktEnd("Wafer Pick Die");
                }

                State = ProcessState.Complete;
                handoffToPlace = true;
                return 0;
            }
            finally
            {
                if (this.RunUnitStatus == UnitStatus.ManualRunning)
                {
                    State = ProcessState.None;
                }

                if (IsPositionPickZSafety() || IsPositionPlaceZSafety())
                {
                    nRet = MovePositionSafetyZ();
                    if (nRet != 0)
                    {
                        AxisPickZ?.EmgStop();
                        AxisPlaceZ?.EmgStop();
                        AxisToolT?.EmgStop();
                        PostAlarm((int)AlarmKeys.eInputDieTransferMoveFail);
                        Log.Write(UnitName, nameof(InputTrDiePick), "[InputTrDiePick] MovePositionSafetyZ failed");
                    }
                }

                if (!handoffToPlace)
                {
                    EndOneCycleIfNeeded();
                }
            }
        }
       
        public bool CompleteInputDieTrDie { get; set; } = false;
        protected override int OnRunComplete()
        {
            int nRet = 0;
            int nArmIndex = GetInputTrArmIndex();
            int nSocketIndex = GetLoadIndexNo();
            try
            {
                nRet = InputTrDiePlace(nSocketIndex);
                if (nRet != 0)
                {
                    Log.Write(UnitName, "OnRunWork", "[OnRunWork] InputTrDiePlace failed");
                    return -1;
                }
                return nRet;
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                return -1;
            }
        }

        public int InputTrDiePlace(int nSocketIndex, bool bSpeed = false)
        {
            int nRet = 0;
            int nArmIndex = GetInputTrArmIndex();

            try
            {
                MaterialDie trDie = null;

                TaktStart("DiePlace_Wait");
                try
                {
                    trDie = GetMaterial() as MaterialDie;
                    if (trDie == null || trDie.Presence != Material.MaterialPresence.Exist)
                    {
                        State = ProcessState.Ready;
                        return 0;
                    }
                    if (this.RunUnitStatus == UnitStatus.AutoRunning && Rotary.RequestInputDieTrDie == false)
                    {
                        // 아직 Place 조건 미충족: One Cycle 유지
                        return 0;
                    }
                    if (Rotary.IsLoadSocketEmpty() == false || Rotary.IsIndexMoving())
                    {
                        // 아직 Place 조건 미충족: One Cycle 유지
                        return 0;
                    }
                }
                finally
                {
                    TaktEnd("DiePlace_Wait");
                }

                TaktStart("DiePlace_NextDie_Req");
                try
                {
                    TaktStart("StartPrepareNextDieTask");
                    try
                    {
                        nRet = StartPrepareNextDieTask();
                    }
                    finally
                    {
                        TaktEnd("StartPrepareNextDieTask");
                    }
                    if (nRet != 0)
                    {
                        Log.Write(UnitName, nameof(InputTrDiePlace), "StartPrepareNextDieTask failed");
                        return -1;
                    }

                    if (nSocketIndex < 0 || nSocketIndex >= Rotary.GetIndexCount())
                    {
                        Log.Write(UnitName, nameof(InputTrDiePlace), $"Invalid socket index: {nSocketIndex}");
                        return -1;
                    }
                }
                finally
                {
                    TaktEnd("DiePlace_NextDie_Req");
                }

                if (this.RunUnitStatus == UnitStatus.ManualRunning)
                {
                    nRet = Rotary.MoveToSocket(
                        nSocketIndex,
                        Rotary.IndexReference.Load,
                        CancellationToken.None,
                        maxStep: 16,
                        settleMs: 50);

                    if (nRet != 0)
                    {
                        PostAlarm((int)AlarmKeys.eInputDieTransferRotateMoveToSocket);
                        return -1;
                    }

                    TaktStart("PlaceDie_ToolT");
                    try
                    {
                        nRet = PlaceDie_ToolT();
                    }
                    finally
                    {
                        TaktEnd("PlaceDie_ToolT");
                    }

                    if (nRet != 0)
                    {
                        AxisPickZ?.EmgStop();
                        AxisToolT?.EmgStop();
                        PostAlarm((int)AlarmKeys.eInputDieTransferRotateToolTForPlace);
                        return -1;
                    }
                }

                this.CompleteInputDieTrDie = false;
                TaktStart("DiePlace_Vacuum_Check");
                try
                {
                    if (this.IsVacuumOK(nArmIndex) == false)
                    {
                        nRet = MovePositionReady();
                        if (nRet != 0)
                        {
                            AxisPickZ?.EmgStop();
                            AxisToolT?.EmgStop();
                            PostAlarm((int)AlarmKeys.eInputDieTransferNotSafety);
                            return -1;
                        }

                        SetVacuum(nArmIndex, false);
                        SetVent(nArmIndex, true);
                        SetVent(nArmIndex, false);
                        SetBlow(nArmIndex, true);
                        Thread.Sleep(50);
                        SetBlow(nArmIndex, false);
                        SetVacuum(nArmIndex, true);

                        this.SetMaterial(null);
                        var ctx = Equipment.Instance.SummaryContext;
                        ctx.GetCurrentSummaryOrNull()?.AddLdPlaceAsMiss();

                        nRet = OnLdPickMissDetected2(nArmIndex);
                        if (nRet != 0)
                        {
                            return -1;
                        }

                        TaktStart("WaitPrepareNextDieTask_Miss");
                        try
                        {
                            nRet = WaitPrepareNextDieTaskOrAlarm(5000, true);
                        }
                        finally
                        {
                            TaktEnd("WaitPrepareNextDieTask_Miss");
                        }

                        if (nRet != 0)
                        {
                            return -1;
                        }

                        State = ProcessState.None;
                        return 0;
                    }

                    ResetLdPickMissCounter2();
                }
                finally
                {
                    TaktEnd("DiePlace_Vacuum_Check");
                }

                TaktStart("PlaceDownDie");
                try
                {
                    nRet = PlaceDownDie(nSocketIndex, bSpeed);
                }
                finally
                {
                    TaktEnd("PlaceDownDie");
                }
                if (nRet != 0)
                {
                    AxisPlaceZ?.EmgStop();
                    AxisToolT?.EmgStop();
                    PostAlarm((int)AlarmKeys.eInputDieTransferPlaceChipDown);
                    return -1;
                }

                TaktStart("PlaceUp");
                try
                {
                    nRet = PlaceUp();
                }
                finally
                {
                    TaktEnd("PlaceUp");
                }
                if (nRet != 0)
                {
                    AxisPlaceZ?.EmgStop();
                    AxisToolT?.EmgStop();
                    PostAlarm((int)AlarmKeys.eInputDieTransferReleaseVacuumAndPlaceUp);
                    return -1;
                }

                TaktStart("PlaceUp_AfterProcess");
                try
                {
                    trDie.State = DieProcessState.Inspecting;
                    trDie.ProcessSatate = Material.MaterialProcessSatate.Processing;
                    trDie.Presence = Material.MaterialPresence.Exist;
                    this.MoveMaterial(trDie, this.Rotary);
                    this.SetMaterial(null);

                    this.CompleteInputDieTrDie = true;
                    
                    //이거 여기서 해야하나? PickUp전에 하는데?
                    //nRet = WaitPrepareNextDieTaskOrAlarm(5000, true);
                    //if (nRet != 0)
                    //{
                    //    return -1;
                    //}

                    if (this.RunUnitStatus == UnitStatus.ManualRunning)
                    {
                        Rotary.SetSocketStatusLoaded(Rotary.RotarySocketState.Loaded);
                    }

                    SetVacuum(nArmIndex, true, false);
                }
                finally
                {
                    TaktEnd("PlaceUp_AfterProcess");
                }

                State = ProcessState.None;

                nRet = MovePositionPickDown(true, PickDownMoveMode.ToolTOnly);
                if (nRet != 0)
                {
                    AxisPickZ?.EmgStop();
                    AxisToolT?.EmgStop();
                    PostAlarm((int)AlarmKeys.eInputDieTransferChipPickDown);
                    return -1;
                }
                return 0;
            }
            finally
            {
                if (this.RunUnitStatus == UnitStatus.ManualRunning)
                {
                    State = ProcessState.None;
                }

                TaktStart("Finally_MoveSafetyZ");
                try
                {
                    if (IsPositionPickZSafety() || IsPositionPlaceZSafety())
                    {
                        nRet = MovePositionSafetyZ();
                        if (nRet != 0)
                        {
                            AxisPickZ?.EmgStop();
                            AxisPlaceZ?.EmgStop();
                            AxisToolT?.EmgStop();
                            PostAlarm((int)AlarmKeys.eInputDieTransferMoveFail);
                            Log.Write(UnitName, nameof(InputTrDiePlace), "MovePositionSafetyZ failed");
                        }
                    }
                }
                finally
                {
                    TaktEnd("Finally_MoveSafetyZ");
                }

                // One Cycle 종료 조건:
                // - 에러
                // - State가 None/Ready로 종료
                if (nRet != 0 || State == ProcessState.None || State == ProcessState.Ready)
                {
                    EndOneCycleIfNeeded();
                }
            }
        }
        #endregion

        #region Sequence Use Functions



        #endregion

        #region Sequence 등록
        protected override void OnMakeSequence()
        {
            base.OnMakeSequence();
            // Pick Phase
            this.SequencePlayers.Add(PrepareNextDie);
            this.SequencePlayers.Add(RaiseEjectorForPick);
            this.SequencePlayers.Add(PickDownDie);
            //this.SequencePlayers.Add(EjectorVacuumOn);
            this.SequencePlayers.Add(SyncPickUpDie);
            this.SequencePlayers.Add(SyncPickDieRetreat);
            this.SequencePlayers.Add(CommitPickedDie);

            this.SequencePlayers.Add(PlaceDie_ToolT);
            this.SequencePlayers.Add(PlaceDownDie);
            this.SequencePlayers.Add(PlaceUp);
        }

        #endregion
        #region Seq 단위 동작 함수
        public int PrepareNextDie(bool bFineSpeed = false)
        {
            int nRet = 0;
            this.CurrentFunc = PrepareNextDie;

            // 다음 Pick 시작 전에 이전 Place에서 시작한 Align 완료 보장
            nRet = WaitAlignBeforeNextPick();
            if (nRet != 0)
            {
                Log.Write(UnitName, nameof(PrepareNextDie), "WaitAlignBeforeNextPick failed");
                return -1;
            }

            // 웨이퍼/상태 점검은 여기서도 방어적으로 수행
            var wafer = this.InputStage != null ? this.InputStage.GetMaterialWafer() : null;
            if (wafer == null)
            {
                Log.Write(UnitName, "[PrepareNextDie] wafer is null");
                SetPreparedNextDie(null);
                return -1;
            }

            if (wafer.Presence != Material.MaterialPresence.Exist ||
                wafer.ProcessSatate != Material.MaterialProcessSatate.Processing)
            {
                SetPreparedNextDie(null);
                return 0;
            }

            MaterialDie die;
            nRet = MoveStageToNextDie(out die);
            // 핵심: 다음 다이가 없는 것은 에러가 아니라 정상 종료
            if (die == null || die.Presence != Material.MaterialPresence.Exist)
            {
                SetPreparedNextDie(null);
                return 0;
            }

            if (nRet != 0)
            {
                Log.Write(UnitName, "[PrepareNextDie] MoveStageToNextDie failed");
                SetPreparedNextDie(null);
                return -1;
            }

            SetPreparedNextDie(die);
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

            Log.Write(UnitName, "RecheckDieAndAlign", "[RecheckDieAndAlign] OK");
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

            if(InputStageEjector.IsEjectorZInPos(InputStageEjectorConfig.TeachingPositionName.EjectBlockUp) == false)
            {
                nRet = InputStageEjector.MovePositionEjectBlockUp(bFineSpeed);
                if (nRet != 0)
                {
                    AxisPickZ.EmgStop();
                    AxisPlaceZ.EmgStop();
                    AxisToolT.EmgStop();
                    PostAlarm((int)AlarmKeys.eInputStageNotSafety);
                    Log.Write(UnitName, "RaiseEjectorForPick", "[RaiseEjectorForPick] EjectBlockUp 이동 실패");
                    return -1;
                }
            }
            
            if(InputStageEjector.IsPinZInPos(InputStageEjectorConfig.TeachingPositionName.EjectPinReady) == false)
            {
                nRet = InputStageEjector.MovePositionEjectPinReady(bFineSpeed);
                if (nRet != 0)
                {
                    AxisPickZ.EmgStop();
                    AxisPlaceZ.EmgStop();
                    AxisToolT.EmgStop();
                    PostAlarm((int)AlarmKeys.eInputStageNotSafety);
                    Log.Write(UnitName, "RaiseEjectorForPick", "[RaiseEjectorForPick] EjectPinReady 이동 실패");
                    return -1;
                }
            }

            try
            {
                bool bRet = EjectorVacuumOn();
                if (bRet == false)
                {
                    Log.Write(UnitName, "RaiseEjectorForPick", "[RaiseEjectorForPick] EjectorVacuumOn failed");
                    return -1;
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }

            nRet = WaitEjectorVacuumOnStateOrAlarm();
            if (nRet != 0)
            {
                PostAlarm((int)AlarmKeys.eInputStageVaccum);
                Log.Write(UnitName, "RaiseEjectorForPick", "[RaiseEjectorForPick] WaitVacuumStateOrAlarm failed");
                return -1;
            }

            if (IsStop)
            { return 0; }

            return nRet;
        }
        public int PickDownDie(bool bFineSpeed = false)
        {
            int nRet = 0;
            if (RunMode == UnitRunMode.Manual)
            {
                this.CurrentFunc = PickDownDie;
            }

            int nArmIndex = GetInputTrArmIndex();

            SetVacuum(nArmIndex, true, false);

            Thread.Sleep(Config.nBeforePickUpWaitTime);

            nRet = MovePositionPickDown(bFineSpeed);
            if (nRet != 0)
            {
                Log.Write(UnitName, "[ChipPickDown] MovePositionPickUp failed");
                return -1;
            }

            return nRet;
        }

        public int SyncPickUpDie(bool bFineSpeed = false)
        {
            if (InputStageEjector == null)
            {
                PostAlarm((int)AlarmKeys.eInputStageNotSafety);
                return -1;
            }

            if (RunMode == UnitRunMode.Manual)
            {
                this.CurrentFunc = SyncPickUpDie;
            }

            int nRet = 0;
            int timeoutMs = 5000;   // 필요 시 예: 5000;
            nRet = MovePickZAndPinZByOffset(timeoutMs, bFineSpeed);
            if (nRet != 0)
            {
                Log.Write(UnitName, "[SyncPickPinDown] MovePickZAndPinZByOffset failed");
                return -1;
            }

            return nRet;
        }
        public int SyncPickDieRetreat(bool bFineSpeed = false)
        {
            int nRet = 0;
            if (RunMode == UnitRunMode.Manual)
            {
                this.CurrentFunc = SyncPickDieRetreat;
            }

            if (InputStageEjector == null)
            {
                PostAlarm((int)AlarmKeys.eInputStageNotSafety);
                return -1;
            }

            this.WaitByTime(Config.nAfterPickUpWaitTime, 1);
            // PickZ Safety 이동
            double dZPos = GetTP(InputDieTransferRecipe.TeachingPositionName.SafetyZone.ToString(),
                                 AxisNames.LeftPickZ);
            if (true) //병렬
            {
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

                

                bool bRet = EjectorVaccumOff();
                if (bRet == false)
                {
                    PostAlarm((int)AlarmKeys.eInputStageVaccum);
                    Log.Write(UnitName, "OnRunWork", "[OnRunWork] EjectorVacuumOff failed");
                    return -1;
                }

                //// 각 모션을 Task로 병렬화
                ////var taskZ = Task.Run(() => MoveAxisPositionOne(AxisPickZ, dZPos, bFineSpeed));
                //var taskPin = Task.Run(() => InputStageEjector.MovePositionEjectPinReady(bFineSpeed));
                //var taskBlock = Task.Run(() => InputStageEjector.MovePositionEjectBlockReady(bFineSpeed));
                ////Task.WaitAll(taskZ, taskPin, taskBlock);
                //Task.WaitAll(taskPin, taskBlock);
                //if (taskPin.Result != 0 || taskBlock.Result != 0)
                //{
                //    AxisToolT.EmgStop();
                //    AxisPickZ.EmgStop();
                //    AxisPlaceZ.EmgStop();

                //    // ... 알람 처리
                //    if (taskPin.Result != 0)
                //    {
                //        PostAlarm((int)AlarmKeys.eInputDieTransferError);
                //        Log.Write(UnitName, "[SyncPickPinRetreat] EjectPinReady 이동 실패");
                //    }

                //    if (taskBlock.Result != 0)
                //    {
                //        PostAlarm((int)AlarmKeys.eInputDieTransferError);
                //        Log.Write(UnitName, "[SyncPickPinRetreat] EjectBlockReady 이동 실패");
                //    }
                //    return -1;
                //}
            }
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


        private readonly object _alignSync = new object();
        private Task<int> _taskAlignAfterPlace = null;

        // 필요시 Config로 빼도 됨
        private const int AlignStartTimeoutMs = 1000;
        private const int AlignJoinTimeoutMs = 5000;
        private const double AlignClearToolTPos = -100.0; //-100.0;
        private int StartAlignAfterPlaceAsync()
        {
            lock (_alignSync)
            {
                if (_taskAlignAfterPlace != null && !_taskAlignAfterPlace.IsCompleted)
                {
                    Log.Write(UnitName, nameof(StartAlignAfterPlaceAsync), "Previous align task is still running.");
                    return 0;
                }

                _taskAlignAfterPlace = Task.Run(() =>
                {
                    try
                    {
                        if (AxisToolT == null)
                            return -1;

                        var sw = Stopwatch.StartNew();
                        while (sw.ElapsedMilliseconds < AlignStartTimeoutMs)
                        {
                            double currentPos = AxisToolT.GetPosition();

                            // 기존 코드의 Math.Abs(currentPos) > -100 은 항상 true라 즉시 통과됨.
                            // 실제 의도대로 임계 위치 도달 시점 체크.
                            if (currentPos <= AlignClearToolTPos)
                            {
                                //Thread.Sleep(50);
                                break;
                            }

                            Thread.Sleep(1);
                        }

                        return RecheckDieAndAlign();
                    }
                    catch (Exception ex)
                    {
                        Log.Write(ex);
                        return -1;
                    }
                });
            }

            return 0;
        }

        private int WaitAlignBeforeNextPick(int timeoutMs = AlignJoinTimeoutMs)
        {
            Task<int> alignTask = null;
            lock (_alignSync)
            {
                alignTask = _taskAlignAfterPlace;
            }

            if (alignTask == null)
                return 0;

            try
            {
                if (!alignTask.Wait(timeoutMs))
                {
                    Log.Write(UnitName, nameof(WaitAlignBeforeNextPick), $"Align task timeout({timeoutMs}ms).");
                    PostAlarm((int)AlarmKeys.eInputDieTransferRecheckDieAndAlign);
                    return -1;
                }

                if (alignTask.Result != 0)
                {
                    Log.Write(UnitName, nameof(WaitAlignBeforeNextPick), "Align task failed.");
                    PostAlarm((int)AlarmKeys.eInputDieTransferRecheckDieAndAlign);
                    return -1;
                }

                return 0;
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                PostAlarm((int)AlarmKeys.eInputDieTransferRecheckDieAndAlign);
                return -1;
            }
            finally
            {
                if (alignTask.IsCompleted)
                {
                    lock (_alignSync)
                    {
                        if (ReferenceEquals(_taskAlignAfterPlace, alignTask))
                            _taskAlignAfterPlace = null;
                    }
                }
            }
        }

        private int StartPrepareNextDieTask()
        {
            lock (_prepareNextDieSync)
            {
                if (taskPrepareNextDie != null && !taskPrepareNextDie.IsCompleted)
                {
                    Log.Write(UnitName, nameof(StartPrepareNextDieTask), "taskPrepareNextDie already running");
                    return 0;
                }

                _preparedNextDie = null;
                taskPrepareNextDie = Task.Run(() =>
                {
                    try
                    {
                        return PrepareNextDie();
                    }
                    catch (Exception ex)
                    {
                        Log.Write(ex);
                        return -1;
                    }
                });
            }

            return 0;
        }
        private int WaitPrepareNextDieTaskOrAlarm(int timeoutMs = 5000, bool requireTask = false)
        {
            Task<int> task = null;
            lock (_prepareNextDieSync)
            {
                task = taskPrepareNextDie;
            }

            if (task == null)
            {
                if (requireTask)
                {
                    Log.Write(UnitName, nameof(WaitPrepareNextDieTaskOrAlarm), "taskPrepareNextDie is null");
                    PostAlarm((int)AlarmKeys.eInputDieTransferPrepareNextDie);
                    return -1;
                }
                return 0;
            }

            try
            {
                if (!task.Wait(timeoutMs))
                {
                    Log.Write(UnitName, nameof(WaitPrepareNextDieTaskOrAlarm), $"taskPrepareNextDie timeout ({timeoutMs}ms)");
                    PostAlarm((int)AlarmKeys.eInputDieTransferPrepareNextDie);
                    return -1;
                }

                if (task.IsCanceled || task.IsFaulted)
                {
                    Log.Write(UnitName, nameof(WaitPrepareNextDieTaskOrAlarm), "taskPrepareNextDie canceled/faulted");
                    if (task.Exception != null)
                    {
                        foreach (var ex in task.Exception.Flatten().InnerExceptions)
                        {
                            Log.Write(ex);
                        }
                    }
                    PostAlarm((int)AlarmKeys.eInputDieTransferPrepareNextDie);
                    return -1;
                }

                if (task.Result != 0)
                {
                    Log.Write(UnitName, nameof(WaitPrepareNextDieTaskOrAlarm), $"taskPrepareNextDie result={task.Result}");
                    PostAlarm((int)AlarmKeys.eInputDieTransferPrepareNextDie);
                    return -1;
                }
                return 0;
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                PostAlarm((int)AlarmKeys.eInputDieTransferPrepareNextDie);
                return -1;
            }
            finally
            {
                lock (_prepareNextDieSync)
                {
                    if (ReferenceEquals(taskPrepareNextDie, task) && task.IsCompleted)
                    {
                        taskPrepareNextDie = null;
                    }
                }
            }
        }
        private void SetPreparedNextDie(MaterialDie die)
        {
            lock (_prepareNextDieSync)
            {
                _preparedNextDie = die;
            }
        }

        private MaterialDie ConsumePreparedNextDieOrQuery()
        {
            lock (_prepareNextDieSync)
            {
                if (_preparedNextDie != null)
                {
                    MaterialDie die = _preparedNextDie;
                    _preparedNextDie = null;
                    return die;
                }
            }

            return InputStage != null ? InputStage.GetNextDie() : null;
        }



        public int PlaceDie_ToolT(bool bFineSpeed = false)
        {
            int nRet = 0;
            this.CurrentFunc = PlaceDie_ToolT;

            // 1) T 회전 시작
            Task<int> taskRotate = Task.Run(() => RotateToolTForPlace(bFineSpeed));

            // 2) Align는 전역 Task로 시작만 (여기서 완료 대기 안함)
            nRet = StartAlignAfterPlaceAsync();
            if (nRet != 0)
            {
                Log.Write(UnitName, nameof(PlaceDie_ToolT), "StartAlignAfterPlaceAsync failed");
                return -1;
            }

            try
            {
                // 3) T축 완료만 대기
                taskRotate.Wait();

                // 3. 두 작업이 모두 끝날 때까지 대기
                // 택타임 줄이는 방안으로 taskAlign 이거는 확인 하지 말까? 무조건 된다라고 봐야하나.
                //Task.WaitAll(taskRotate, taskAlign);
            }
            // [제안 - 조금 더 명확하게]
            catch (AggregateException ae)
            {
                foreach (var e in ae.InnerExceptions)
                {
                    Log.Write(UnitName, "[RotateToolTForPlace_AsyncWait] Task Error", e.Message);
                }
                return -1;
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                return -1;
            }

            // 4. 결과 확인
            if (taskRotate.Result != 0)
            {
                Log.Write(UnitName, "[RotateToolTForPlace_AsyncWait] RotateToolTForPlace failed");
                return -1;
            }

            return nRet;
        }

        public int PlaceDownDie(bool bFineSpeed = false)
        {
            if (RunMode == UnitRunMode.Manual)
            {
                this.CurrentFunc = PlaceDownDie;
            }
            int nRet = 0;
            int armIndex = GetInputTrArmIndex();
            int nIndex = GetLoadIndexNo();

            if (!TryGetPlaceTeachingName(nIndex, out string tpName))
            {
                Log.Write(UnitName, $"[PlaceChipDown] Invalid index {nIndex}. Range 0~7");
                return -1;
            }

            var tpObj = GetTeachingPosition(tpName);
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
            SetVacuum(armIndex, false);
            SetVent(armIndex, true);
            this.WaitByTime(Config.nPlaceAfterVacWaitTime, 1);//Thread.Sleep(1);
            SetVent(armIndex, false);

            this.WaitByTime(Config.nPlaceBeforeBlowWaitTime, 1);
            SetBlow(armIndex, true);
            this.WaitByTime(Config.nPlaceAfterBlowWaitTime2, 1); //Blow 키고 대기 시간.

            //Die 내려놓고 Z축 올리기 전으로 코드 이동.
            //Task.Run(() =>
            //{
            //    try
            //    {
            //        this.WaitByTime(Config.nPlaceAfterBlowWaitTime, 1);
            //        SetBlow(armIndex, false);
            //    }
            //    catch (Exception ex)
            //    {
            //        Log.Write(ex);
            //    }
            //});
            return nRet;

        }
        public int PlaceDownDie(int nSocketIndex, bool bFineSpeed = false)
        {
            if (RunMode == UnitRunMode.Manual)
            {
                this.CurrentFunc = PlaceDownDie;
            }
            int nRet = 0;
            int armIndex = GetInputTrArmIndex();

            if (!TryGetPlaceTeachingName(nSocketIndex, out string tpName))
            {
                Log.Write(UnitName, $"[PlaceChipDown] Invalid index {nSocketIndex}. Range 0~7");
                return -1;
            }

            var tpObj = GetTeachingPosition(tpName);
            if (tpObj == null)
            {
                Log.Write(UnitName, $"[PlaceChipDown] Teaching not found: {tpName}");
                return -1;
            }

            Rotary.SetVacuum(nSocketIndex, true);
            Thread.Sleep(1);

            double dTPos = GetTP(tpName, AxisNames.LeftPlaceZ);
            nRet = MoveAxisPositionOne(AxisPlaceZ, dTPos, bFineSpeed);
            if (nRet != 0)
            {
                Log.Write(UnitName, "[RotateToolTForPlace] ToolT Place 이동 실패");
                return -1;
            }

            this.WaitByTime(Config.nPlaceBeforeVacWaitTime, 1);//Thread.Sleep(1);
            SetVacuum(armIndex, false);
            SetVent(armIndex, true);
            this.WaitByTime(Config.nPlaceAfterVacWaitTime, 1);//Thread.Sleep(1);
            SetVent(armIndex, false);

            this.WaitByTime(Config.nPlaceBeforeBlowWaitTime, 1);
            SetBlow(armIndex, true);
            this.WaitByTime(Config.nPlaceAfterBlowWaitTime2, 1); //Blow 키고 대기 시간.

            //Die 내려놓고 Z축 올리기 전으로 코드 이동.
            //Task.Run(() =>
            //{
            //    try
            //    {
            //        this.WaitByTime(Config.nPlaceAfterBlowWaitTime, 1);
            //        SetBlow(armIndex, false);
            //    }
            //    catch (Exception ex)
            //    {
            //        Log.Write(ex);
            //    }
            //});
            return nRet;

        }

        public int ChipPickDownReturn(bool bFineSpeed = false)
        {
            int nRet = 0;
            if (RunMode == UnitRunMode.Manual)
            {
                this.CurrentFunc = ChipPickDownReturn;
            }

            int nArmIndex = GetInputTrArmIndex();

            nRet = MovePositionPickDown(bFineSpeed); //이게 다운이다.!!
            if (nRet != 0)
            {
                Log.Write(UnitName, "[ChipPickDown] MovePositionPickUp failed");
                return -1;
            }

            //Auto Run 진행 시 TackTime 상관없는 Delay 
            SetVacuum(nArmIndex, false, false);
            SetVent(nArmIndex, true);
            Thread.Sleep(100);
            SetVent(nArmIndex, false);
            SetBlow(nArmIndex, true);
            Thread.Sleep(100);

            return nRet;
        }


        public int PlaceUp(bool bFindSpeed = false)
        {
            int nRet = 0;
            this.CurrentFunc = PlaceUp;

            try
            {
                LogSequence("Start");
                int nArmIndex = GetInputTrArmIndex();
                int nIndex = GetLoadIndexNo();
                if (nArmIndex < 0 || nArmIndex > 3)
                    return -1;

                this.WaitByTime(Config.nPlaceUpWaitTime, 1);
                // Safety 위치로 상승
                //SetVacuum(nArmIndex, false, false);
                this.SetBlow(nArmIndex, true);
                Task.Run(() =>
                {
                    try
                    {
                        this.WaitByTime(Config.nPlaceAfterBlowWaitTime, 1);
                        SetBlow(nArmIndex, false);
                    }
                    catch (Exception ex)
                    {
                        Log.Write(ex);
                    }
                });

                double dZPos = GetTP(InputDieTransferRecipe.TeachingPositionName.SafetyZone.ToString(),
                            AxisNames.LeftPlaceZ);
                nRet = MoveAxisPositionOne(AxisPlaceZ, dZPos, bFindSpeed);
                if (nRet != 0)
                {
                    Log.Write(UnitName, "[ReleaseVacuumAndPlaceUp] AxisPlaceZ SafetyZone 이동 실패");
                    return -1;
                }

                //여기서 버큠 확인해야지.
                if (true)
                {
                    bool okBlow = this.SetBlow(nArmIndex, false);
                    // 여기서 Vacuum을 키고 확인.
                    SetVacuum(nArmIndex, true, false);
                    Thread.Sleep(10);
                    bool bVacuumOK = IsVacuumOK(nArmIndex);
                    var equipment = Equipment.Instance;
                    bool IsDryRunEqp = equipment.EquipmentConfig.IsDryRun;

                    //Test
                    //Thread.Sleep(100);

                    int timeoutMs = 3000;
                    var sw = Stopwatch.StartNew();
                    if (Config.IsSimulation == false &&
                       (Config.IsDryRun == false && IsDryRunEqp == false))
                    {
                        while (sw.ElapsedMilliseconds <= timeoutMs)
                        {
                            bool ok = IsVacuumOK(nArmIndex);
                            if (ok == false)
                            {
                                bVacuumOK = ok;
                                break;
                            }
                        }
                    }

                    if (Config.IsSimulation == true ||
                       (Config.IsDryRun == true || IsDryRunEqp == true))
                    {
                        bVacuumOK = false;
                    }

                    if (bVacuumOK)
                    {
                        //20260423 - 문제 잡아야함. -1 나오는 조건
                        nRet = MovePositionReady();
                        if (nRet != 0)
                        {
                            AxisPickZ?.EmgStop();
                            AxisToolT?.EmgStop();
                            PostAlarm((int)AlarmKeys.eInputDieTransferNotSafety);
                            Log.Write(UnitName, "OnRunComplete", "CommitPickedDie failed");
                            return -1;
                        }

                        this.SetVacuum(nArmIndex, false);
                        this.SetBlow(nArmIndex, true);

                        // 여기는 택타임과 무관 //예외상태에서 대기.
                        Thread.Sleep(50);

                        this.SetBlow(nArmIndex, false);
                        Thread.Sleep(1);
                        this.SetVacuum(nArmIndex, true);

                        this.SetMaterial(null);
                        var ctx = Equipment.Instance.SummaryContext;
                        ctx.GetCurrentSummaryOrNull()?.AddLdPlaceAsMiss();

                        nRet = OnLdPickMissDetected2(nArmIndex);
                        if (nRet != 0)
                        {
                            Log.Write(UnitName, "OnRunComplete", "CommitPickedDie failed");
                            return -1;
                        }

                        this.SetMaterial(null);

                        nRet = WaitPrepareNextDieTaskOrAlarm(5000, true);
                        if (nRet != 0)
                        {
                            Log.Write(UnitName, "OnRunComplete", "[OnRunComplete] taskPrepareNextDie failed");
                            return -1;
                        }

                        // 제품 가지고 있는 상태에서 또 PickUp 할려고 하면 Needle 손상됨.
                        PostAlarm((int)AlarmKeys.eInputDieTransferLdPickAsMiss);
                        _consecutiveLdPickMissCount2 = 0; // 알람 후 다시 카운트 시작 (원치 않으면 제거)
                        return -1;
                    }
                    else
                    {
                        ResetLdPickMissCounter2();
                    }
                }

                // Rotary/암 Vacuum 확인 (동일 인덱스 사용)
                //nRet = Rotary.WaitVacuumStateOrAlarm(Rotary.GetLoadIndexNo(), true);
                nRet = Rotary.WaitVacuumStateOrAlarm(GetLoadIndexNo(), true);
                if (nRet != 0)
                {
                    Log.Write(UnitName, "[RotaryVacuumOn] Vacuum Timeout");
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

            var tpObj = GetTeachingPosition(tpName);
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
            if (Rotary == null)
                return nIndex;

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
            double tp = this.GetTP(InputDieTransferRecipe.TeachingPositionName.Ready.ToString(),
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
                if (nRet != 0)
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

        public int ManualResetForNewRun(bool bFineSpeed = false)
        {
            ResetForNewRun(true, true);
            return 0;
        }

        // 클래스 내부에 추가
        public void ResetForNewRun(bool moveToSafeReady = true, bool clearHeldDie = true)
        {
            // 1) 런타임 플래그/버퍼 초기화
            lock (_prepareNextDieSync)
            {
                taskPrepareNextDie = null;
                _preparedNextDie = null;
            }

            lock (_alignSync)
            {
                _taskAlignAfterPlace = null;
            }

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

                    this.SetVacuum(0, false, false);
                    this.SetVent(0, false);
                    this.SetBlow(0, true);
                    //Auto Run 진행 시 TackTime 상관없는 Delay 
                    Thread.Sleep(100);
                    this.SetBlow(0, false);
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

            InputDieTransferRecipe.TeachingPositionName en;
            if (!Enum.TryParse(tpName, out en))
                return -1;

            return MoveByTeachingName(en, isFine);
        }

        /// <summary>
        /// TeachingPositionName 기반 이동 라우팅(핵심).
        /// Form에서 switch 하던 것을 이 함수로 이동.
        /// </summary>
        public int MoveByTeachingName(InputDieTransferRecipe.TeachingPositionName teachingName, bool isFine)
        {
            int rc;
            int placeIndex;

            switch (teachingName)
            {
                case InputDieTransferRecipe.TeachingPositionName.Pickup:
                    rc = MovePositionSafetyZ(isFine);
                    if (rc != 0) return -1;

                    rc = MovePositionPickUpToolT(isFine);
                    if (rc != 0) return -1;

                    rc = MovePositionPickUpPickZ(isFine);
                    if (rc != 0) return -1;

                    return 0;

                case InputDieTransferRecipe.TeachingPositionName.Ready:
                    rc = MovePositionReady(isFine);
                    return rc == 0 ? 0 : -1;

                case InputDieTransferRecipe.TeachingPositionName.SafetyZone:
                    rc = MovePositionSafetyZ(isFine);
                    return rc == 0 ? 0 : -1;

                case InputDieTransferRecipe.TeachingPositionName.Place_Index1: placeIndex = 0; break;
                case InputDieTransferRecipe.TeachingPositionName.Place_Index2: placeIndex = 1; break;
                case InputDieTransferRecipe.TeachingPositionName.Place_Index3: placeIndex = 2; break;
                case InputDieTransferRecipe.TeachingPositionName.Place_Index4: placeIndex = 3; break;
                case InputDieTransferRecipe.TeachingPositionName.Place_Index5: placeIndex = 4; break;
                case InputDieTransferRecipe.TeachingPositionName.Place_Index6: placeIndex = 5; break;
                case InputDieTransferRecipe.TeachingPositionName.Place_Index7: placeIndex = 6; break;
                case InputDieTransferRecipe.TeachingPositionName.Place_Index8: placeIndex = 7; break;

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

        public int MoveToTeachingPositionBySelectionIndex(int teachingSelIndex, bool isFine = false)
        {
            if (Config == null)
                return -1;

            string tpName;
            if (!Config.GetTeachingPositionName(teachingSelIndex, out tpName) || string.IsNullOrWhiteSpace(tpName))
                return -1;

            InputDieTransferRecipe.TeachingPositionName en;
            if (!Enum.TryParse(tpName, out en))
                return -1;

            int nRet = 0;
            int nIndex = -1;

            switch (en)
            {
                case InputDieTransferRecipe.TeachingPositionName.Pickup:
                    nRet = MovePositionPickUpToolT(isFine);
                    if (nRet != 0)
                    {
                        var mbErr = new MessageBoxOk();
                        mbErr.ShowDialog("Error.", "ToolT 이동 실패");
                        return nRet;
                    }

                    nRet = MovePositionPickUpPickZ(isFine);
                    if (nRet != 0)
                    {
                        var mbErr = new MessageBoxOk();
                        mbErr.ShowDialog("Error.", "ToolT 이동 실패");
                        return nRet;
                    }
                    break;

                case InputDieTransferRecipe.TeachingPositionName.Ready:
                    nRet = MovePositionSafetyZ(isFine);
                    if (nRet != 0)
                    {
                        var mbErr = new MessageBoxOk();
                        mbErr.ShowDialog("Error.", "SafetyZ 이동 실패");
                        return nRet;
                    }

                    nRet = MovePositionReady(isFine);
                    if (nRet != 0)
                    {
                        var mbErr = new MessageBoxOk();
                        mbErr.ShowDialog("Error.", "ToolT 이동 실패");
                        return nRet;
                    }
                    break;

                case InputDieTransferRecipe.TeachingPositionName.SafetyZone:
                    nRet = MovePositionSafetyZ(isFine);
                    if (nRet != 0)
                    {
                        var mbErr = new MessageBoxOk();
                        mbErr.ShowDialog("Error.", "SafetyZ 이동 실패");
                        return nRet;
                    }
                    break;

                case InputDieTransferRecipe.TeachingPositionName.Place_Index1:
                    nIndex = 0;

                    nRet = MovePositionPlace_Index(nIndex, isFine);
                    if (nRet != 0)
                    {
                        var mbErr = new MessageBoxOk();
                        mbErr.ShowDialog("Error.", "Index 위치 이동 실패");
                        return nRet;
                    }
                    break;
                case InputDieTransferRecipe.TeachingPositionName.Place_Index2:
                    nIndex = 1;

                    nRet = MovePositionPlace_Index(nIndex, isFine);
                    if (nRet != 0)
                    {
                        var mbErr = new MessageBoxOk();
                        mbErr.ShowDialog("Error.", "Index 위치 이동 실패");
                        return nRet;
                    }
                    break;
                case InputDieTransferRecipe.TeachingPositionName.Place_Index3:
                    nIndex = 2;

                    nRet = MovePositionPlace_Index(nIndex, isFine);
                    if (nRet != 0)
                    {
                        var mbErr = new MessageBoxOk();
                        mbErr.ShowDialog("Error.", "Index 위치 이동 실패");
                        return nRet;
                    }
                    break;
                case InputDieTransferRecipe.TeachingPositionName.Place_Index4:
                    nIndex = 3;

                    nRet = MovePositionPlace_Index(nIndex, isFine);
                    if (nRet != 0)
                    {
                        var mbErr = new MessageBoxOk();
                        mbErr.ShowDialog("Error.", "Index 위치 이동 실패");
                        return nRet;
                    }
                    break;
                case InputDieTransferRecipe.TeachingPositionName.Place_Index5:
                    nIndex = 4;

                    nRet = MovePositionPlace_Index(nIndex, isFine);
                    if (nRet != 0)
                    {
                        var mbErr = new MessageBoxOk();
                        mbErr.ShowDialog("Error.", "Index 위치 이동 실패");
                        return nRet;
                    }
                    break;
                case InputDieTransferRecipe.TeachingPositionName.Place_Index6:
                    nIndex = 5;
                    nRet = MovePositionPlace_Index(nIndex, isFine);
                    if (nRet != 0)
                    {
                        var mbErr = new MessageBoxOk();
                        mbErr.ShowDialog("Error.", "Index 위치 이동 실패");
                        return nRet;
                    }
                    break;
                case InputDieTransferRecipe.TeachingPositionName.Place_Index7:
                    nIndex = 6;
                    nRet = MovePositionPlace_Index(nIndex, isFine);
                    if (nRet != 0)
                    {
                        var mbErr = new MessageBoxOk();
                        mbErr.ShowDialog("Error.", "Index 위치 이동 실패");
                        return nRet;
                    }
                    break;
                case InputDieTransferRecipe.TeachingPositionName.Place_Index8:
                    nIndex = 7;
                    nRet = MovePositionPlace_Index(nIndex, isFine);
                    if (nRet != 0)
                    {
                        var mbErr = new MessageBoxOk();
                        mbErr.ShowDialog("Error.", "Index 위치 이동 실패");
                        return nRet;
                    }
                    break;

                default:
                    break;
            }

            return nRet;
        }

        private int OnLdPickMissDetected1()
        {
            LdPickMissAlarmThreshold = Config.AlarmCount;
            _consecutiveLdPickMissCount1++;

            Log.Write(UnitName, nameof(OnLdPickMissDetected1),
                $"LdPickMiss consecutive={_consecutiveLdPickMissCount1}/{LdPickMissAlarmThreshold}");

            if (_consecutiveLdPickMissCount1 >= LdPickMissAlarmThreshold)
            {
                PostAlarm((int)AlarmKeys.eInputDieTransferLdPickAsMiss);
                _consecutiveLdPickMissCount1 = 0; // 알람 후 다시 카운트 시작 (원치 않으면 제거)
                return -1;
            }

            return 0;
        }

        // 기존 메서드 교체
        private void ResetLdPickMissCounter1(string reason = "")
        {
            if (_consecutiveLdPickMissCount1 != 0)
            {
                //Log.Write(UnitName, nameof(ResetLdPickMissCounter),
                //    $"Reset consecutive miss counter({_consecutiveLdPickMissCount}) reason={reason}");
            }
            _consecutiveLdPickMissCount1 = 0;
        }

        private int OnLdPickMissDetected2(int nArmIndex)
        {
            LdPickMissAlarmThreshold = Config.AlarmCount;
            _consecutiveLdPickMissCount2++;

            Log.Write(UnitName, nameof(OnLdPickMissDetected1),
                $"LdPickMiss consecutive={_consecutiveLdPickMissCount2}/{LdPickMissAlarmThreshold}");

            if (_consecutiveLdPickMissCount2 >= LdPickMissAlarmThreshold)
            {
                PostAlarm((int)AlarmKeys.eInputDieTransferLdPickAsMiss);
                _consecutiveLdPickMissCount2 = 0; // 알람 후 다시 카운트 시작 (원치 않으면 제거)
                return -1;
            }

            return 0;
        }

        // 기존 메서드 교체
        private void ResetLdPickMissCounter2(string reason = "")
        {
            if (_consecutiveLdPickMissCount2 != 0)
            {
                //Log.Write(UnitName, nameof(ResetLdPickMissCounter),
                //    $"Reset consecutive miss counter({_consecutiveLdPickMissCount}) reason={reason}");
            }
            _consecutiveLdPickMissCount2 = 0;
        }
    }
}