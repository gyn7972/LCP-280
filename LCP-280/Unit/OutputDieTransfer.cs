using QMC.Common;
using QMC.Common.Alarm;
using QMC.Common.Component;
using QMC.Common.IOUtil;
using QMC.Common.Motions;
using QMC.Common.Unit;
using QMC.LCP_280.Process.Component;
using QMC.LCP_280.Process.Unit.FormWork.Repro;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static QMC.Common.Material;
using static QMC.LCP_280.Process.Equipment;
using static QMC.LCP_280.Process.Unit.OutputDieTransferConfig.IO; // IO 상수/배열

namespace QMC.LCP_280.Process.Unit
{
    public class OutputDieTransfer : BaseUnit<OutputDieTransferConfig>
    {
        private bool IsDryRunEqp
        {
            get
            {
                var eq = Equipment.Instance;
                bool r = eq.EquipmentConfig.IsDryRun;
                return r;
            }
        }
        public new enum AlarmKeys
        {
            eOutputDieTransferError = 11001,
            eOuputDieTransferZNotSafety,
            eOutputStageAxesMoving,
            eRotaryAxesMoving,
            eOutputDieTransferVacuum,
            eOutputDieTransferVent,
            eOutputDieTransferBlow,
            eBinStageCylinderZNotSafety,
            eOutputDieTransferMovePickUpToolT,
            eOutputDieTransferChipPickDown,
            eOutputDieTransferChipPickUp,
            eOutputDieTransferMoveOutStage,
            eOutputDieTransferRotateToolTForPlace,
            eOutputDieTransferReleaseVacuumAndPlaceUp,
            eOutputDieTransferLdPickAsMissError,
        }
        #region InitAlarm
        protected override void InitAlarm()
        {
            string source = "Bin_Arm";
            base.InitAlarm();
            // 1. 공용 파일 로더에서 알람 목록 가져오기
            var loadedAlarms = GlobalAlarmTable.Instance.GetAlarmsForSource(source);
            if (loadedAlarms == null || loadedAlarms.Count == 0)
            {
                AlarmInfo alarm = new AlarmInfo();
                alarm.Code = (int)AlarmKeys.eOuputDieTransferZNotSafety;
                alarm.Title = "Die Tr Z-Axis Not safety Pos.";
                alarm.Cause = "Die Tr Z-Axis is not in a safe position. Please check the position and restart.";
                alarm.Source = source;// this.UnitName;
                alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
                m_dicAlarms.Add(alarm.Code, alarm);

                alarm = new AlarmInfo();
                alarm.Code = (int)AlarmKeys.eOutputDieTransferError;
                alarm.Title = "Output Die Transfer Error";
                alarm.Cause = "An unknown error occurred in Output Die Transfer.";
                alarm.Source = source;// this.UnitName;
                alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
                m_dicAlarms.Add(alarm.Code, alarm);

                alarm = new AlarmInfo();
                alarm.Code = (int)AlarmKeys.eOutputStageAxesMoving;
                alarm.Title = "Output Stage Axis Moving";
                alarm.Cause = "Output Stage Axis is moving. Please wait for the movement to complete and restart.";
                alarm.Source = source;// this.UnitName;
                alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
                m_dicAlarms.Add(alarm.Code, alarm);

                alarm = new AlarmInfo();
                alarm.Code = (int)AlarmKeys.eRotaryAxesMoving;
                alarm.Title = "Rotary Axis Moving";
                alarm.Cause = "Rotary Axis is moving. Please wait for the movement to complete and restart.";
                alarm.Source = source;// this.UnitName;
                alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
                m_dicAlarms.Add(alarm.Code, alarm);

                //eOutputDieTransferVacuum
                alarm = new AlarmInfo();
                alarm.Code = (int)AlarmKeys.eOutputDieTransferVacuum;
                alarm.Title = "Output Die Transfer Vacuum Error";
                alarm.Cause = "Output Die Transfer Vacuum is Off. Please check the vacuum status and restart.";
                alarm.Source = source;// this.UnitName;
                alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
                m_dicAlarms.Add(alarm.Code, alarm);


                //
                alarm = new AlarmInfo();
                alarm.Code = (int)AlarmKeys.eOutputDieTransferVent;
                alarm.Title = "Output Die Transfer Vent Error";
                alarm.Cause = "Output Die Transfer Vent is Off. Please check the vent status and restart.";
                alarm.Source = source;// this.UnitName;
                alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
                m_dicAlarms.Add(alarm.Code, alarm);
                //
                alarm = new AlarmInfo();
                alarm.Code = (int)AlarmKeys.eOutputDieTransferBlow;
                alarm.Title = "Output Die Transfer Blow Error";
                alarm.Cause = "Output Die Transfer Blow is Off. Please check the blow status and restart.";
                alarm.Source = source;// this.UnitName;
                alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
                m_dicAlarms.Add(alarm.Code, alarm);
                //
                alarm.Code = (int)AlarmKeys.eBinStageCylinderZNotSafety;
                alarm.Title = "Bin Stage Z-Cylinder Not Safety Pos.";
                alarm.Cause = "Bin Stage Z-Cylinder is not in a safe position.\n Please check the position and restart.";
                alarm.Source = source;// this.UnitName;
                alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
                m_dicAlarms.Add(alarm.Code, alarm);

                alarm = new AlarmInfo();
                alarm.Code = (int)AlarmKeys.eOutputDieTransferMovePickUpToolT;
                alarm.Title = "Output Die Transfer Move PickUp Tool T Error";
                alarm.Cause = "An error occurred while moving Output Die Transfer PickUp Tool T.";
                alarm.Source = source;// this.UnitName;
                alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
                m_dicAlarms.Add(alarm.Code, alarm);

                alarm = new AlarmInfo();
                alarm.Code = (int)AlarmKeys.eOutputDieTransferChipPickDown;
                alarm.Title = "Output Die Transfer Chip Pick Down Error";
                alarm.Cause = "An error occurred during Output Die Transfer Chip Pick Down.";
                alarm.Source = source;// this.UnitName;
                alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
                m_dicAlarms.Add(alarm.Code, alarm);

                alarm = new AlarmInfo();
                alarm.Code = (int)AlarmKeys.eOutputDieTransferChipPickUp;
                alarm.Title = "Output Die Transfer Chip Pick Up Error";
                alarm.Cause = "An error occurred during Output Die Transfer Chip Pick Up.";
                alarm.Source = source;// this.UnitName;
                alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
                m_dicAlarms.Add(alarm.Code, alarm);

                alarm = new AlarmInfo();
                alarm.Code = (int)AlarmKeys.eOutputDieTransferMoveOutStage;
                alarm.Title = "Output Die Transfer Move Out Stage Error";
                alarm.Cause = "An error occurred while moving Output Die Transfer to Out Stage.";
                alarm.Source = source;// this.UnitName;
                alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
                m_dicAlarms.Add(alarm.Code, alarm);

                alarm = new AlarmInfo();
                alarm.Code = (int)AlarmKeys.eOutputDieTransferRotateToolTForPlace;
                alarm.Title = "Output Die Transfer Rotate Tool T For Place Error";
                alarm.Cause = "An error occurred while rotating Output Die Transfer Tool T for placement.";
                alarm.Source = source;// this.UnitName;
                alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
                m_dicAlarms.Add(alarm.Code, alarm);

                alarm = new AlarmInfo();
                alarm.Code = (int)AlarmKeys.eOutputDieTransferReleaseVacuumAndPlaceUp;
                alarm.Title = "Output Die Transfer Release Vacuum And Place Up Error";
                alarm.Cause = "An error occurred during Output Die Transfer Release Vacuum And Place Up.";
                alarm.Source = source;// this.UnitName;
                alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
                m_dicAlarms.Add(alarm.Code, alarm);

                //eOutputDieTransferLdPickAsMissError
                alarm = new AlarmInfo();
                alarm.Code = (int)AlarmKeys.eOutputDieTransferLdPickAsMissError;
                alarm.Title = "Output Die Transfer Load Pick As Miss Error";
                alarm.Cause = "Output Die Transfer is in Load Pick As Miss state. Please check the Load Pick status and restart.";
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

        #region Unit
        Rotary Rotary { get; set; }
        OutputStage OutputStage { get; set; }
        OutputFeeder OutputFeeder { get; set; }
        #endregion

        #region Axis Helpers
        private MotionAxis _toolT, _pickZ, _placeZ;
        public MotionAxis AxisOutputToolT => _toolT;
        public MotionAxis AxisOutputPickZ => _pickZ;
        public MotionAxis AxisOutputPlaceZ => _placeZ;
        // Safety 동작 중 여부
        private bool _isSafetyMoving = false;

        private void BindAxes()
        {
            var mgr = Equipment.Instance?.AxisManager;
            if (mgr == null)
            {
                Log.Write(UnitName, "[BindAxes] AxisManager null");
                return;
            }

            const string unitName = "Unit"; // Equipment에서 축 등록 시 사용한 유닛명과 동일해야 함
            BindAxis(mgr, unitName, AxisNames.RightToolT, ref _toolT);
            BindAxis(mgr, unitName, AxisNames.RightPickZ, ref _pickZ);
            BindAxis(mgr, unitName, AxisNames.RightPlaceZ, ref _placeZ);
        }
        #endregion
        public override bool IsInterlockOK(BaseComponent baseComponent, BaseComponent.InterlockEventArgs e)
        {
            bool bRet = base.IsInterlockOK(baseComponent, e);
            int nRet = 0;
            if (baseComponent == this.AxisOutputPickZ)
            {
                if (_isSafetyMoving)
                    return true;
                if (this.Rotary.IsIndexMoving())
                {
                    AxisOutputPickZ?.EmgStop();
                    PostAlarm((int)AlarmKeys.eRotaryAxesMoving);
                    return false;
                }
            }
            else if (baseComponent == this.AxisOutputPlaceZ)
            {
                if (_isSafetyMoving)
                    return true;
                if (this.OutputStage.IsAxisMoving(AxisNames.BinStageX) ||
                    this.OutputStage.IsAxisMoving(AxisNames.BinStageY) ||
                    this.OutputStage.IsAxisMoving(AxisNames.BinStageT))
                {
                    AxisOutputPlaceZ?.EmgStop();
                    PostAlarm((int)AlarmKeys.eOutputStageAxesMoving);
                    return false;
                }
                if (this.OutputStage.IsPlateDown() == false)
                {
                    AxisOutputPlaceZ?.EmgStop();
                    PostAlarm((int)AlarmKeys.eBinStageCylinderZNotSafety);
                    return false;
                }
            }
            else if (baseComponent == this.AxisOutputToolT)
            {
                if (this.IsPositionPickZSafety() == false || this.IsPositionPlaceZSafety() == false)
                {
                    nRet = MovePositionSafetyZ();
                    if(nRet != 0)
                    {
                        Log.Write(UnitName, "[IsInterlockOK] baseComponent == this.AxisOutputToolT faild");
                        AxisOutputToolT?.EmgStop();
                        PostAlarm((int)AlarmKeys.eOuputDieTransferZNotSafety);
                        return false;
                    }
                }
                if (this.OutputStage.IsPlateDown() == false)
                {
                    AxisOutputToolT?.EmgStop();
                    PostAlarm((int)AlarmKeys.eBinStageCylinderZNotSafety);
                    return false;
                }
            }
            return bRet;
        }

        private readonly ManualResetEventSlim _pickUpStartEvent = new ManualResetEventSlim(false);
        private readonly ManualResetEventSlim _pickUpdoneEvent = new ManualResetEventSlim(false);

        public void ResetPickupHandshake()
        {
            _pickUpStartEvent.Reset();
            _pickUpdoneEvent.Reset();
        }

        public void SetPickupStartEvent()
        {
            _pickUpStartEvent.Set();
        }
        public void ReSetPickupStartEvent()
        {
            _pickUpStartEvent.Reset();
        }

        public bool WaitPickupStartEvent(int timeoutMs = Timeout.Infinite)
        {
            bool bRet = false;
            bRet = _pickUpStartEvent.Wait(timeoutMs);
            return bRet;
        }

        public void SetPickupDoneEvent()
        {
            _pickUpdoneEvent.Set();
        }

        public void ResetPickupDoneEvent()
        {
            _pickUpdoneEvent.Reset();
        }

        public bool WaitPickupDoneEvent(int timeoutMs = Timeout.Infinite)
        {
            bool bRet = false;
            bRet = _pickUpdoneEvent.Wait(timeoutMs);
            return bRet;
        }

        public OutputDieTransfer(OutputDieTransferRecipe config = null)
            : base(new OutputDieTransferConfig())
        {
            AddComponents();
        }

        public override void AddComponents()
        {
            Config.LoadAndBindAxes(Equipment.Instance.AxisManager);
            Config.InitializeDefaultTeachingPositions();

            BindAxes();
            BindIoDomains();
        }
        protected override void OnBindUnit()
        {
            base.OnBindUnit();
            Rotary = Equipment.Instance.GetUnit(UnitKeys.Rotary) as Rotary;
            OutputStage = Equipment.Instance.GetUnit(UnitKeys.OutputStage) as OutputStage;
            OutputFeeder = Equipment.Instance.GetUnit(UnitKeys.OutputFeeder) as OutputFeeder;
        }

        public int GetUnloaderIndexNo()
        {
            int nIndex = 0;
            if (Rotary == null) return nIndex;
            nIndex = (Rotary.GetLoadIndexNo() + this.Config.IndexOfEnd) % Rotary.GetIndexCount();
            return nIndex;
        }
        public int MoveAxisPositionOne(MotionAxis axis, double target, bool isFine = false)
        {
            if (axis == null) return -1;

            Task<int> task = MoveAxisPositionOneAsync(axis, target, isFine);
            while (IsEndTask(task) == false)
            {
                if (axis == AxisOutputPickZ)
                {
                    if (this.Rotary.IsIndexMoving())
                    {
                        AxisOutputToolT.EmgStop();
                        AxisOutputPickZ.EmgStop();
                        AxisOutputPlaceZ.EmgStop();
                        PostAlarm((int)AlarmKeys.eRotaryAxesMoving);
                        return -1;
                    }
                }

                if (axis == AxisOutputPlaceZ)
                {
                    if (OutputStage.IsAnyAxisMoving())
                    {
                        AxisOutputToolT.EmgStop();
                        AxisOutputPickZ.EmgStop();
                        AxisOutputPlaceZ.EmgStop();
                        PostAlarm((int)AlarmKeys.eOutputStageAxesMoving);
                        return -1;
                    }
                }

                Thread.Sleep(1);
            }
            return task.Result;
        }

        public int MovePositionSafetyPlaceZ(bool isFine = false)
        {
            Task<int> task = MovePositionAsyncSafetyPlaceZ(isFine);
            while (IsEndTask(task) == false)
            {
                IsMoveInterLockSafetyPlaceZ();
                Thread.Sleep(1);
            }
            return task.Result;
        }
        public Task<int> MovePositionAsyncSafetyPlaceZ(bool isFine = false)
        {
            return Task.Run(() =>
            {
                OnMovePositionSafetyPlaceZ(isFine);
                return 0;
            });
        }
        private int OnMovePositionSafetyPlaceZ(bool isFine = false)
        {
            int nRet = 0;
            _isSafetyMoving = true;
            try
            {
                double dZPos = GetTP(OutputDieTransferRecipe.TeachingPositionName.SafetyZone.ToString(),
                        AxisNames.RightPlaceZ);
                nRet = MoveAxisPositionOne(AxisOutputPlaceZ, dZPos);
                if (nRet != 0)
                {
                    return -1;
                }
                //return MoveTeachingPositionOnce((int)OutputDieTransferConfig.TeachingPositionName.SafetyZone, isFine);
            }
            finally
            {
                _isSafetyMoving = false;
            }

            return nRet;
        }
        private int IsMoveInterLockSafetyPlaceZ()
        {
            int nRet = 0;
            // Check Interlock.!!! 구문 넣을것.!!!

            return nRet;
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
                return MoveTeachingPositionOnce((int)OutputDieTransferRecipe.TeachingPositionName.SafetyZone, isFine);
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
                            AxisOutputToolT?.EmgStop();
                            AxisOutputPickZ?.EmgStop();
                            AxisOutputPlaceZ?.EmgStop();
                        }
                        catch
                        {
                            Log.Write(UnitName, "ResetForNewRun", "catch");
                        }
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
        private Task<int> MovePositionAsyncReady(bool isFine = false)
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
            if (!IsPositionPlaceZSafety() 
                || !IsPositionPickZSafety())
            {
                nRet = MovePositionSafetyZ();
                if (nRet != 0)
                {
                    Log.Write(UnitName, "[OnMovePositionReady] MovePositionSafetyZ faild");
                    return -1;
                }
            }

            while (!IsPositionPlaceZSafety() 
                    || !IsPositionPickZSafety())
            {
                if(IsStop)
                {
                    return 0;
                }
                Thread.Sleep(1);
            }


            double dTPos = GetTP(OutputDieTransferRecipe.TeachingPositionName.Ready.ToString(),
                        AxisNames.RightToolT);
            nRet = MoveAxisPositionOne(AxisOutputToolT, dTPos);
            if (nRet != 0)
            {
                return -1;
            }

            //double dZPos = GetTP(OutputDieTransferConfig.TeachingPositionName.Ready.ToString(),
            //            AxisNames.RightPlaceZ);
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
            //우선 막아도 된다. T 움직일때 무조건 상승 시키고 움직이니깐
            //bool bPickZ = IsPositionPickZSafety();
            //bool bPlaceZ = IsPositionPlaceZSafety();
            //if (bPickZ == false 
            //    || bPlaceZ == false)
            //{
            //    AxisOutputToolT?.EmgStop();
            //    AxisOutputPickZ?.EmgStop();
            //    AxisOutputPlaceZ?.EmgStop();
            //    PostAlarm((int)AlarmKeys.eOuputDieTransferZNotSafety);
            //    return -1;
            //}

            //if (OutputStage != null && OutputStage.IsAnyAxisMoving())
            //{
            //    AxisOutputToolT?.EmgStop();
            //    AxisOutputPickZ?.EmgStop();
            //    AxisOutputPlaceZ?.EmgStop();
            //    PostAlarm((int)AlarmKeys.eOutputStageAxesMoving);
            //    return -1;
            //}

            //if (Rotary != null && Rotary.IsAnyAxisMoving())
            //{
            //    AxisOutputToolT?.EmgStop();
            //    AxisOutputPickZ?.EmgStop();
            //    AxisOutputPlaceZ?.EmgStop();
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
                            AxisOutputToolT?.EmgStop();
                            AxisOutputPickZ?.EmgStop();
                            AxisOutputPlaceZ?.EmgStop();
                        }
                        catch
                        {
                            Log.Write(UnitName, "ResetForNewRun", "catch");
                        }
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

        /// ///////////////////////////////////////////////////////////////////////////////////////////
        public int MovePositionPickUp_Index(bool isFine = false)
        {
            int index = GetUnloaderIndexNo();

            Task<int> task = MovePositionAsyncPickUp_Index(isFine, index);
            while (!IsEndTask(task))
            {
                int interlock = IsMoveInterLockPickUp_Index(index);
                if (interlock != 0)
                {
                    return -1;
                }
                Thread.Sleep(1);
            }
            return task.Result;
        }
        private Task<int> MovePositionAsyncPickUp_Index(bool isFine = false, int nIndex = 0)
        {
            return Task.Run(() =>
            {
                return OnMovePositionPickUp_Index(isFine, nIndex);
            });
        }
        private int OnMovePositionPickUp_Index(bool isFine = false, int nIndex = 0)
        {
            // 안전 Z 위치 확인 후 필요 시 이동
            if (!IsPositionPlaceZSafety() || !IsPositionPickZSafety())
            {
                int safetyMove = MovePositionSafetyZ(isFine);
                if (safetyMove != 0)
                {
                    Log.Write(UnitName, "[OnMovePositionPickUp_Index] MovePositionSafetyZ faild");
                    return -1;
                }
            }

            // Teaching 이름 확인
            string tpName;
            if (TryGetPickupTeachingName(nIndex, out tpName) == false)
            {
                return -1;
            }

            // 1) ToolT 이동
            int r = MoveToolT_ToPickupIndex(tpName, isFine);
            if (r != 0)
            {
                return -1;
            }

            // 2) PickZ 이동
            r = MovePickZ_ToPickupIndex(tpName, isFine);
            if (r != 0)
            {
                return -1;
            }

            return 0;
        }
        private int IsMoveInterLockPickUp_Index(int nIndex = 0)
        {
            if (Rotary != null && this.Rotary.IsIndexMoving())
            {
                AxisOutputToolT?.EmgStop();
                AxisOutputPickZ?.EmgStop();
                AxisOutputPlaceZ?.EmgStop();
                PostAlarm((int)AlarmKeys.eRotaryAxesMoving);
                return -1;
            }
            return 0;
        }

        public int MovePositionPickUpToolT_Index(int index = 0, bool isFine = false)
        {
            Task<int> task = MovePositionAsyncPickUpToolT_Index(isFine, index);
            while (!IsEndTask(task))
            {
                int interlock = IsMoveInterLockPickUpToolT_Index(index);
                if (interlock != 0)
                {
                    return -1;
                }
                Thread.Sleep(1);
            }
            return task.Result;
        }
        private Task<int> MovePositionAsyncPickUpToolT_Index(bool isFine = false, int nIndex = 0)
        {
            return Task.Run(() =>
            {
                return OnMovePositionPickUpToolT_Index(isFine, nIndex);
            });
        }
        private int OnMovePositionPickUpToolT_Index(bool isFine = false, int nIndex = 0)
        {
            // 안전 Z 위치 확인 후 필요 시 이동
            if (!IsPositionPlaceZSafety() || !IsPositionPickZSafety())
            {
                int safetyMove = MovePositionSafetyZ(isFine);
                if (safetyMove != 0)
                {
                    Log.Write(UnitName, "[OnMovePositionPickUpToolT_Index] MovePositionSafetyZ faild");
                    return -1;
                }
            }

            // Teaching 이름 확인
            string tpName;
            if (TryGetPickupTeachingName(nIndex, out tpName) == false)
            {
                return -1;
            }

            // 1) ToolT 이동
            int r = MoveToolT_ToPickupIndex(tpName, isFine);
            if (r != 0)
            {
                return -1;
            }
            return 0;
        }
        private int IsMoveInterLockPickUpToolT_Index(int nIndex = 0)
        {
            // 안전 Z 위치 확인 후 필요 시 이동
            if (!IsPositionPlaceZSafety() || !IsPositionPickZSafety())
            {
                int safetyMove = MovePositionSafetyZ();
                if (safetyMove != 0)
                {
                    Log.Write(UnitName, "[IsMoveInterLockPickUpToolT_Index] MovePositionSafetyZ faild");
                    return -1;
                }
            }
            return 0;
        }

        public int MovePositionPickUpPickZ_Index(int index = 0, bool isFine = false)
        {
            Task<int> task = MovePositionAsyncPickUpPickZ_Index(isFine, index);
            while (!IsEndTask(task))
            {
                int interlock = IsMoveInterLockPickUpPickZ_Index(index);
                if (interlock != 0)
                {
                    return -1;
                }
                Thread.Sleep(1);
            }
            return task.Result;
        }
        private Task<int> MovePositionAsyncPickUpPickZ_Index(bool isFine = false, int nIndex = 0)
        {
            return Task.Run(() =>
            {
                return OnMovePositionPickUpPickZ_Index(isFine, nIndex);
            });
        }
        private int OnMovePositionPickUpPickZ_Index(bool isFine = false, int nIndex = 0)
        {
            // Teaching 이름 확인
            string tpName;
            if (TryGetPickupTeachingName(nIndex, out tpName) == false)
            {
                return -1;
            }

            // 2) PickZ 이동
            int r = MovePickZ_ToPickupIndex(tpName, isFine);
            if (r != 0)
            {
                return -1;
            }

            return 0;
        }
        private int IsMoveInterLockPickUpPickZ_Index(int nIndex = 0)
        {
            if (Rotary != null && this.Rotary.IsIndexMoving())
            {
                AxisOutputToolT?.EmgStop();
                AxisOutputPickZ?.EmgStop();
                AxisOutputPlaceZ?.EmgStop();
                PostAlarm((int)AlarmKeys.eRotaryAxesMoving);
                return -1;
            }
            return 0;
        }

        private int MoveToolT_ToPickupIndex(string tpName, bool isFine)
        {
            double target = GetTP(tpName, AxisNames.RightToolT);
            int r = MoveAxisPositionOne(AxisOutputToolT, target, isFine);
            if (r != 0)
            {
                Log.Write(UnitName, "MoveToolT_ToPickupIndex", $"[MoveToolT_ToPickupIndex] ToolT move failed tp={tpName} pos={target}");
                return -1;
            }
            return 0;
        }
        private int MovePickZ_ToPickupIndex(string tpName, bool isFine)
        {
            double target = GetTP(tpName, AxisNames.RightPickZ);
            int r = MoveAxisPositionOne(AxisOutputPickZ, target, isFine);
            if (r != 0)
            {
                Log.Write(UnitName, "MovePickZ_ToPickupIndex", $"[MovePickZ_ToPickupIndex] PickZ move failed tp={tpName} pos={target}");
                return -1;
            }
            return 0;
        }
        /////////////////////////////////////////////////////////////////////////////////////////////

        public Task<int> MovePositionAsyncSafePickUp_Index(bool isFine = false, int nIndex = 0, CancellationToken ct = default(CancellationToken))
        {
            return Task.Run(() =>
            {
                // Task로 돌리고 별도 인터락/취소 감시
                var coreTask = Task.Run(() => OnMovePositionPickUp_Index(isFine, nIndex), ct);

                while (!IsEndTask(coreTask))
                {
                    if (ct.IsCancellationRequested)
                    {
                        try
                        {
                            AxisOutputToolT?.EmgStop();
                            AxisOutputPickZ?.EmgStop();
                            AxisOutputPlaceZ?.EmgStop();
                        }
                        catch
                        {
                            Log.Write(UnitName, "MovePositionAsyncSafePickUp_Index", "catch");
                        }
                        return -999; // 취소 코드
                    }

                    int nRtn = IsMoveInterLockPickUp_Index(nIndex);
                    if (nRtn != 0)
                    {
                        return -1;
                    }

                    Thread.Sleep(1); // 0→5ms로 약간 여유 (CPU 점유 감소)
                }

                return coreTask.Result;
            }, ct);
        }


        public int MovePositionPlace(bool isFine = false)
        {
            Task<int> task = MovePositionAsyncPlace(isFine);
            while (IsEndTask(task) == false)
            {
                int nRtn = IsMoveInterLockPlace();
                if (nRtn != 0)
                {
                    return -1;
                }

                Thread.Sleep(1);
            }
            return task.Result;
        }
        public Task<int> MovePositionAsyncPlace(bool isFine = false)
        {
            return Task.Run(() =>
            {
                OnMovePositionPlace(isFine);
                return 0;
            });
        }
        private int OnMovePositionPlace(bool isFine = false)
        {
            int nRet = 0;
            if (!IsPositionPlaceZSafety() || !IsPositionPickZSafety())
            {
                nRet = MovePositionSafetyZ();
                if (nRet != 0)
                {
                    Log.Write(UnitName, "[OnMovePositionPlace] MovePositionSafetyZ faild");
                    return -1;
                }
            }

            double dTPos = GetTP(OutputDieTransferRecipe.TeachingPositionName.Place.ToString(),
                        AxisNames.RightToolT);
            nRet = MoveAxisPositionOne(AxisOutputToolT, dTPos);
            if (nRet != 0)
            {
                return -1;
            }

            double dZPos = GetTP(OutputDieTransferRecipe.TeachingPositionName.Place.ToString(),
                        AxisNames.RightPlaceZ);
            nRet = MoveAxisPositionOne(AxisOutputPlaceZ, dZPos);
            if (nRet != 0)
            {
                return -1;
            }

            return nRet;
        }
        private int IsMoveInterLockPlace()
        {
            int nRet = 0;
            if (OutputStage != null && OutputStage.IsAnyAxisMoving())
            {
                AxisOutputToolT?.EmgStop();
                AxisOutputPickZ?.EmgStop();
                AxisOutputPlaceZ?.EmgStop();
                PostAlarm((int)AlarmKeys.eOutputStageAxesMoving);
                return -1;
            }

            //if (Rotary != null && Rotary.IsAnyAxisMoving())
            //{
            //    AxisToolT?.EmgStop();
            //    AxisPickZ?.EmgStop();
            //    AxisPlaceZ?.EmgStop();
            //    PostAlarm((int)AlarmKeys.eRotaryAxesMoving);
            //    return -1;
            //}

            return nRet;
        }

        public Task<int> MovePositionAsyncSafePlace(bool isFine = false, CancellationToken ct = default(CancellationToken))
        {
            return Task.Run(() =>
            {
                // OnMovePlacePosition을 Task로 돌리고 별도 인터락/취소 감시
                var coreTask = Task.Run(() => OnMovePositionPlace(isFine), ct);

                while (!IsEndTask(coreTask))
                {
                    if (ct.IsCancellationRequested)
                    {
                        try
                        {
                            AxisOutputToolT?.EmgStop();
                            AxisOutputPickZ?.EmgStop();
                            AxisOutputPlaceZ?.EmgStop();
                        }
                        catch
                        {
                            Log.Write(UnitName, "MovePositionAsyncSafePlace", "catch");
                        }
                        return -999; // 취소 코드
                    }

                    int nRtn = IsMoveInterLockPlace();
                    if (nRtn != 0)
                    {
                        return -1;
                    }

                    Thread.Sleep(1); // 0→5ms로 약간 여유 (CPU 점유 감소)
                }

                return coreTask.Result;
            }, ct);
        }


        #region Position Check (Unified Refactored)

        // ============================================================
        // 리팩토링 개요
        //  - 중복되던 위치 판정 함수를 단일 범용 헬퍼로 통합.
        //  - Safety / Ready / Place / Pickup(Index) 모두 공통 로직 사용.
        //  - 외부 기존 호출 호환 유지를 위해 기존 공개 메서드 이름은 유지.
        //  - 한 줄 형태의 축약(식 본문) 사용하지 않고 명시적 전통 방식으로 작성.
        // ============================================================

        /// <summary>
        /// 지정 Teaching 위치(Offset 적용 기준)에서 해당 축이 InPosition 인지 확인.
        /// </summary>
        private bool IsAxisInTeaching(MotionAxis axis,
                                      string axisName,
                                      string teachingName,
                                      bool treatAxisNullAsTrue = true,
                                      bool useOffset = false,
                                      double fallbackTolerance = 0.01,
                                      bool useAxisInposTolerance = true)
        {
            if (string.IsNullOrWhiteSpace(teachingName))
            {
                return false;
            }

            if (axis == null)
            {
                return treatAxisNullAsTrue;
            }

            //var tp = Config.GetTeachingPosition(teachingName);
            var tp = GetTeachingPosition(teachingName);
            if (tp == null)
            {
                return false;
            }

            double target = 0.0;
            bool targetResolved = false;

            if (tp.AxisPositions != null)
            {
                // Case-insensitive lookup 보장 안되므로 Any로 찾고 가져옴
                var key = tp.AxisPositions.Keys.FirstOrDefault(k => string.Equals(k, axisName, StringComparison.OrdinalIgnoreCase));
                if (!string.IsNullOrWhiteSpace(key))
                {
                    target = tp.AxisPositions[key];
                    targetResolved = true;
                }
            }

            if (targetResolved == false)
            {
                return false;
            }

            double current = 0.0;
            try
            {
                current = axis.GetPosition();
            }
            catch
            {
                Log.Write(UnitName, "IsAxisInTeaching", "catch");
                return false;
            }

            double tolerance;
            if (useAxisInposTolerance)
            {
                if (axis.Config != null && axis.Config.InposTolerance > 0)
                {
                    tolerance = axis.Config.InposTolerance;
                }
                else
                {
                    tolerance = fallbackTolerance;
                }
            }
            else
            {
                tolerance = fallbackTolerance;
            }

            double diff = Math.Abs(current - target);
            bool inPos = diff <= tolerance;
            return inPos;
        }

        /// <summary>
        /// Safety / SafetyZone 후보 Teaching 들 중 현재 축이 위치한지 판단.
        /// (첫 번째로 조건을 만족하는 Teaching 을 사용)
        /// </summary>
        private bool IsAxisSafetyPos(MotionAxis axis,
                                     string axisName,
                                     string[] candidateTeachingNames,
                                     bool treatMissingAsSafe,
                                     double fallbackTolerance,
                                     bool useAxisInposTolerance)
        {
            if (axis == null)
            {
                return treatMissingAsSafe;
            }

            if (candidateTeachingNames == null || candidateTeachingNames.Length == 0)
            {
                return treatMissingAsSafe;
            }

            foreach (var name in candidateTeachingNames)
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    continue;
                }

                //var tp = Config.GetTeachingPosition(name);
                var tp = GetTeachingPosition(name);
                if (tp == null)
                {
                    continue;
                }

                // 해당 Teaching 에 해당 축 좌표가 정의되어 있는지 검사
                bool containsAxis = tp.AxisPositions != null &&
                                    tp.AxisPositions.Keys.Any(k => string.Equals(k, axisName, StringComparison.OrdinalIgnoreCase));
                if (!containsAxis)
                {
                    continue;
                }

                // 실제 InPos 여부 판단
                bool result = IsAxisInTeaching(axis,
                                               axisName,
                                               name,
                                               treatAxisNullAsTrue: false,
                                               useOffset: true,
                                               fallbackTolerance: fallbackTolerance,
                                               useAxisInposTolerance: useAxisInposTolerance);
                return result;
            }

            // 어떤 Teaching 도 찾지 못한 경우
            return treatMissingAsSafe;
        }

        // ---------------- Safety Position Wrappers (기존 공개 메서드 시그니처 유지) ----------------
        public bool IsPositionPickZSafety(double fallbackTolerance = 0.01,
                                     bool useAxisInposTolerance = true,
                                     bool treatMissingAsSafe = true)
        {
            const string tpName = nameof(OutputDieTransferRecipe.TeachingPositionName.SafetyZone);
            if (AxisOutputPickZ == null)
                return true;

            // 현재 실제 위치 읽기
            double currentPos;
            try
            {
                currentPos = AxisOutputPickZ.GetPosition();
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

            double target = GetTP(tpName, AxisNames.RightPickZ);
            try
            {
                return AxisOutputPickZ.InPosition(target);
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                return false;
            }
        }

        public bool IsPositionToolTSafety()
        {
            return false;
        }

        public bool IsPositionPlaceZSafety(double fallbackTolerance = 0.01,
                                      bool useAxisInposTolerance = true,
                                      bool treatMissingAsSafe = true)
        {
            const string tpName = nameof(OutputDieTransferRecipe.TeachingPositionName.SafetyZone);
            if (AxisOutputPlaceZ == null)
                return true;

            // 현재 실제 위치 읽기
            double currentPos;
            try
            {
                currentPos = AxisOutputPlaceZ.GetPosition();
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

            double target = GetTP(tpName, AxisNames.RightPlaceZ);
            try
            {
                return AxisOutputPlaceZ.InPosition(target);
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                return false;
            }
        }

        // ---------------- Ready ----------------
        public bool IsPositionToolTReady()
        {
            const string tpName = nameof(OutputDieTransferRecipe.TeachingPositionName.Ready);
            if (AxisOutputToolT == null)
                return true;
            double target = GetTP(tpName, AxisNames.RightToolT);
            try
            {
                return AxisOutputToolT.InPosition(target);
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                return false;
            }

            //const string tpName = nameof(OutputDieTransferConfig.TeachingPositionName.Ready);
            //bool result = IsAxisInTeaching(AxisToolT,
            //                               AxisNames.RightToolT,
            //                               tpName,
            //                               treatAxisNullAsTrue: true,
            //                               useOffset: true);
            //return result;
        }

        public bool IsPositionPlaceZReady()
        {
            const string tpName = nameof(OutputDieTransferRecipe.TeachingPositionName.Ready);
            bool result = IsAxisInTeaching(AxisOutputPlaceZ,
                                           AxisNames.RightPlaceZ,
                                           tpName,
                                           treatAxisNullAsTrue: true,
                                           useOffset: true);
            return result;
        }

        // ---------------- Place (단일 Teaching) ----------------
        public bool IsPositionToolTPlace()
        {
            const string tpName = nameof(OutputDieTransferRecipe.TeachingPositionName.Place);
            bool result = IsAxisInTeaching(AxisOutputToolT,
                                           AxisNames.RightToolT,
                                           tpName,
                                           treatAxisNullAsTrue: true,
                                           useOffset: true);
            return result;
        }

        public bool IsPositionPlaceZPlace()
        {
            const string tpName = nameof(OutputDieTransferRecipe.TeachingPositionName.Place);
            bool result = IsAxisInTeaching(AxisOutputPlaceZ,
                                           AxisNames.RightPlaceZ,
                                           tpName,
                                           treatAxisNullAsTrue: true,
                                           useOffset: true);
            return result;
        }

        // ---------------- Pickup (현재 Index) ----------------
        public bool IsPositionToolTPickupIndex()
        {
            int idx = GetUnloaderIndexNo();
            bool result = IsPositionToolTPickupIndex(idx);
            return result;
        }

        public bool IsPositionPickZPickupIndex()
        {
            int idx = GetUnloaderIndexNo();
            bool result = IsPositionPickZPickupIndex(idx);
            return result;
        }

        // ---------------- Pickup (특정 Index) ----------------
        public bool IsPositionToolTPickupIndex(int index)
        {
            string tpName = GetPickupTeachingName(index);
            if (tpName == null)
            {
                return false;
            }

            bool result = IsAxisInTeaching(AxisOutputToolT,
                                           AxisNames.RightToolT,
                                           tpName,
                                           treatAxisNullAsTrue: true,
                                           useOffset: true);
            return result;
        }

        public bool IsPositionPickZPickupIndex(int index)
        {
            string tpName = GetPickupTeachingName(index);
            if (tpName == null)
            {
                return false;
            }

            bool result = IsAxisInTeaching(AxisOutputPickZ,
                                           AxisNames.RightPickZ,
                                           tpName,
                                           treatAxisNullAsTrue: true,
                                           useOffset: true);
            return result;
        }

        // ---------------- 기존 호환 API (PickUp ToolT 위치) ----------------
        public bool IsPositionPickUpToolT(int index,
                                          double fallbackTolerance = 0.01,
                                          bool useAxisInposTolerance = true)
        {
            string tpName = GetPickupTeachingName(index);
            if (tpName == null)
            {
                return false;
            }

            bool result = IsAxisInTeaching(AxisOutputToolT,
                                           AxisNames.RightToolT,
                                           tpName,
                                           treatAxisNullAsTrue: false,
                                           useOffset: true,
                                           fallbackTolerance: fallbackTolerance,
                                           useAxisInposTolerance: useAxisInposTolerance);
            return result;
        }

        public bool IsPositionPickUpToolT()
        {
            int idx = GetUnloaderIndexNo();
            bool result = IsPositionPickUpToolT(idx);
            return result;
        }

        // ---------------- Pickup Index Teaching Name Helper ----------------
        private string GetPickupTeachingName(int index)
        {
            int teachingIdx;

            if (index >= 0 && index < 8)
            {
                teachingIdx = index + 1;
            }
            else
            {
                return null;
            }

            string name = $"Pickup_Index{teachingIdx}";
            return name;
        }

        // 기존 함수 호환 (이동 코드에서 사용) - 내부적으로 새 Helper 사용
        private bool TryGetPickupTeachingName(int index, out string tpName)
        {
            tpName = GetPickupTeachingName(index);
            if (string.IsNullOrWhiteSpace(tpName))
            {
                Log.Write(UnitName, "TryGetPickupTeachingName", $"[TryGetPickupTeachingName] Invalid index {index}. Range 0~7 or 1~8");
                return false;
            }

            var tp = GetTeachingPosition(tpName);
            if (tp == null)
            {
                Log.Write(UnitName, "TryGetPickupTeachingName", $"[TryGetPickupTeachingName] Teaching not found: {tpName}");
                tpName = null;
                return false;
            }
            return true;
        }

        #endregion


        public void TeachCurrentPosition(string positionName, string description = null)
        {
            var axisPositions = new Dictionary<string, double>();
            foreach (var axisPair in Axes)
                axisPositions[axisPair.Key] = axisPair.Value.GetPosition();
            var tp = new TeachingPosition(positionName, axisPositions, description);
            Config.SetTeachingPosition(tp);
        }

        public int MoveToTeachingPosition(string positionName, bool isFine = false)
        {
            if (string.IsNullOrWhiteSpace(positionName))
            {
                Log.Write(UnitName, nameof(MoveToTeachingPosition),
                        $"[TeachingMove] TeachingPositions에서 '{positionName}' 을 찾지 못했습니다.");
                return -1;
            }

            int result = 0;

            OutputDieTransferRecipe.TeachingPositionName en;
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
            {
                var tp = r.Get(tpName);
                if (tp != null)
                    return tp;
            }

            // 혹시라도 TeachingRecipe가 null인 비정상 상태 대비(호환/안전)
            return Config?.GetTeachingPosition(tpName);
        }

        #region IO Helpers (Input / Output 상태)
        public bool ReadInput(string name)
        {
            var hi = Config.HardInputs.FirstOrDefault(i => i.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (hi == null) return false;
            var eq = Equipment.Instance; var dio = eq?.DioScan; if (dio == null) return false;
            foreach (var m in eq.UnitIO.Modules)
                if (dio.TryGetInput(m.ModuleName, hi.Disp, out var v)) return v;
            return false;
        }
        public bool WriteOutput(string name, bool on)
        {
            var ho = Config.HardOutputs.FirstOrDefault(o => o.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (ho == null) return false;
            var eq = Equipment.Instance; var dio = eq?.DioScan; if (dio == null) return false;
            foreach (var m in eq.UnitIO.Modules)
                if (dio.WriteOutput(m.ModuleName, ho.Disp, on) == 0) return true;
            return false;
        }

        // 출력 캐시 상태 조회 (입력과 무관하게 실제 On/Off 표시)
        public bool IsOutputOn(string name)
        {
            var ho = Config.HardOutputs.FirstOrDefault(o => o.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (ho == null) return false;
            var eq = Equipment.Instance; var dio = eq?.DioScan; if (dio == null) return false;
            foreach (var m in eq.UnitIO.Modules)
            {
                if (dio.TryGetOutput(m.ModuleName, ho.Disp, out var v)) return v;
            }
            return false;
        }
        #endregion

        private Vacuum[] _vacuum = new Vacuum[4];              // Vacuum + OK sensor
        public Vacuum[] _blow = new Vacuum[4];
        public Vacuum[] _vent = new Vacuum[4];

        private void BindIoDomains()
        {
            var eq = Equipment.Instance; 
            var unit = eq?.UnitIO; 
            
            if (unit == null) 
                return;

            // Vacuum 별칭으로 조회만
            if (IoAutoBindings.Vacuums.TryGetValue("OutputDieTransferVac1", out _vacuum[0]) == false)
            {
                Log.Write(UnitName, "BindIoDomains", "Vacuums not found: OutputDieTransferVac1");
            }

            if (IoAutoBindings.Vacuums.TryGetValue("OutputDieTransferVac2", out _vacuum[1]) == false)
            {
                Log.Write(UnitName, "BindIoDomains", "Vacuums not found: OutputDieTransferVac2");
            }

            if (IoAutoBindings.Vacuums.TryGetValue("OutputDieTransferVac3", out _vacuum[2]) == false)
            {
                Log.Write(UnitName, "BindIoDomains", "Vacuums not found: OutputDieTransferVac3");
            }

            if (IoAutoBindings.Vacuums.TryGetValue("OutputDieTransferVac4", out _vacuum[3]) == false)
            {
                Log.Write(UnitName, "BindIoDomains", "Vacuums not found: OutputDieTransferVac4");
            }

            if (IoAutoBindings.Vacuums.TryGetValue("OutputDieTransferBlow1", out _blow[0]) == false)
            {
                Log.Write(UnitName, "BindIoDomains", "Vacuums not found: OutputDieTransferBlow1");
            }

            if (IoAutoBindings.Vacuums.TryGetValue("OutputDieTransferBlow2", out _blow[1]) == false)
            {
                Log.Write(UnitName, "BindIoDomains", "Vacuums not found: OutputDieTransferBlow2");
            }

            if (IoAutoBindings.Vacuums.TryGetValue("OutputDieTransferBlow3", out _blow[2]) == false)
            {
                Log.Write(UnitName, "BindIoDomains", "Vacuums not found: OutputDieTransferBlow3");
            }

            if (IoAutoBindings.Vacuums.TryGetValue("OutputDieTransferBlow4", out _blow[3]) == false)
            {
                Log.Write(UnitName, "BindIoDomains", "Vacuums not found: OutputDieTransferBlow4");
            }

            if (IoAutoBindings.Vacuums.TryGetValue("OutputDieTransferVent1", out _vent[0]) == false)
            {
                Log.Write(UnitName, "BindIoDomains", "Vacuums not found: OutputDieTransferVent1");
            }

            if (IoAutoBindings.Vacuums.TryGetValue("OutputDieTransferVent2", out _vent[1]) == false)
            {
                Log.Write(UnitName, "BindIoDomains", "Vacuums not found: OutputDieTransferVent2");
            }

            if (IoAutoBindings.Vacuums.TryGetValue("OutputDieTransferVent3", out _vent[2]) == false)
            {
                Log.Write(UnitName, "BindIoDomains", "Vacuums not found: OutputDieTransferVent3");
            }

            if (IoAutoBindings.Vacuums.TryGetValue("OutputDieTransferVent4", out _vent[3]) == false)
            {
                Log.Write(UnitName, "BindIoDomains", "Vacuums not found: OutputDieTransferVent4");
            }
        }

        // === Domain Control (표준 구동) ===
        public bool SetVacuum(int nNo, bool on)
        {
            if (_vacuum[nNo] == null) return false;
            if (on) _vacuum[nNo].On();
            else _vacuum[nNo].Off();
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

        #region Arm Vacuum / Blow / Vent Control
        public bool AirTankOk() => ReadInput(AIR_TANK_PRESS);
        public bool VacuumTankOk() => ReadInput(VAC_TANK_PRESS);

        public bool IsVacuumOK(int armIndex)
        {
            if (Config.IsSimulation || (Config.IsDryRun || IsDryRunEqp))
            {
                Thread.Sleep(5);
                return true;
            }

            switch (armIndex)
            {
                case 0: return this.ReadInput(OutputDieTransferConfig.IO.ARM1_FLOW);
                case 1: return this.ReadInput(OutputDieTransferConfig.IO.ARM2_FLOW);
                case 2: return this.ReadInput(OutputDieTransferConfig.IO.ARM3_FLOW);
                case 3: return this.ReadInput(OutputDieTransferConfig.IO.ARM4_FLOW);
            }
            return false;
        }

        // === Arm Vacuum 상태 대기 공용 유틸 ===
        // expectOn: true=ON 될 때까지, false=OFF 될 때까지 대기
        // timeoutMs/pollMs: 타임아웃/폴링 간격
        private int WaitVacuumStateOrAlarm(int armIndex, bool expectOn, int timeoutMs = 1000, int pollMs = 1)
        {
            if (Config.IsSimulation || (Config.IsDryRun || IsDryRunEqp))
                return 0;

            //Todo: 2025-10-10 GYN: Vacuum 해결 되면 return 지우기.
            return 0;

            //var sw = Stopwatch.StartNew();
            //while (sw.ElapsedMilliseconds <= timeoutMs)
            //{
            //    bool ok = IsVacuumOK(armIndex);
            //    if (expectOn ? ok : !ok)
            //        return 0;

            //    Thread.Sleep(pollMs);
            //}

            //// 타임아웃 처리
            ////PostAlarm((int)AlarmKeys.eOutputDieTransferVacuum);
            //Log.Write(UnitName, expectOn ? "[Vacuum] Arm vacuum ON timeout" : "[Vacuum] Arm vacuum OFF timeout");
            //return 0;
        }

        #endregion
        /// //////////////////////////////////////////////////////////////////


        #region seq signals
        public bool CompleteOutputDie { get; set; } = false;
        private volatile bool _lastPickSucceeded;
        public bool LastPickSucceeded { get { return _lastPickSucceeded; } }

        #endregion

        #region Lifecycle
        public override int OnRun()
        {
           // TaktStart("OnRun");
            try
            {
                int ret = 0;

                if (this.RunUnitStatus == UnitStatus.Stopped ||
               this.RunUnitStatus == UnitStatus.Stopping ||
               this.RunUnitStatus == UnitStatus.Error ||
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
                    ret = -1;
                    Log.Write(ex);
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
               // TaktEnd("OnRun");
            }
        }
        protected override int OnStart()
        {
            return base.OnStart();
        }
        public override int OnStop()
        {
            int ret = 0;
            _pickUpStartEvent.Reset();
            _pickUpdoneEvent.Reset();

            this.RunUnitStatus = UnitStatus.Stopped;
            base.OnStop();
            return ret;
        }
        protected override int OnRunReady()
        {
            TaktStart("OnRunReady");
            try
            {
                int nRtn = 0;
                MaterialWafer wafer = OutputStage.GetMaterialWafer();
                if (wafer == null || wafer.Presence != Material.MaterialPresence.Exist)
                {
                    //noDieOutputStaga = OutputStage.HasNextDie();
                    return 0;
                }

                if (wafer.ProcessSatate == Material.MaterialProcessSatate.Processing)
                {
                    State = ProcessState.Work;
                }
                else if (wafer.ProcessSatate == Material.MaterialProcessSatate.Completed)
                {
                    State = ProcessState.Complete;
                }
                return nRtn;
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                return -1;
            }
        }

        // [ADD] 마지막으로 처리(Skip 또는 Pick)를 시도했던 Die의 Index를 저장
        private int _lastProcessedDieIndex = -1;

        protected override int OnRunWork()
        {
            try
            {
                int nRet = 0;
                try
                {
                    if (IsBinStageReadyForPlace() == false || OutputStage.CanPlaceDie() == false)
                    {
                        State = ProcessState.None;
                        return 0;
                    }

                    bool vac = false;
                    int nIndex = GetUnloaderIndexNo();
                    int nArmIndex = GetPlaceArmIndex();
                    Log.Write(UnitName, "OnRunWork", "OnRunWork Start");

                    // 3. binArm에 die를 가지고 있으면 바로 Complete로
                    var DeiOutTr = this.GetMaterial() as MaterialDie;
                    vac = this.IsVacuumOK(nArmIndex);
                    if (DeiOutTr != null && vac)
                    {
                        TaktStart("One Cycle");

                        Log.Write(UnitName, "OnRunWork", "Complete ->");
                        State = ProcessState.Complete;
                        return 0;
                    }

                    // 0. Rotary Index 동작 중인지 확인 - 이건 무조건.
                    if (Rotary != null && this.Rotary.IsIndexMoving())
                    {
                        return 0;
                    }


                    if (DeiOutTr == null
                        || DeiOutTr.State != DieProcessState.Picked
                        || DeiOutTr.Presence != Material.MaterialPresence.Exist)
                    {
                        TaktStart("One Cycle");

                        var sw = System.Diagnostics.Stopwatch.StartNew();
                        int timeoutMs = 60000 * 5;
                        bool signalReceived = false;

                        while (true)
                        {
                            if (IsStop)
                            {
                                ReSetPickupStartEvent();
                                _lastPickSucceeded = false;
                                return 0;
                            }

                            // 10ms 대기. 신호 오면 즉시 true 반환
                            //if (WaitPickupStartEvent(10)) //10->50
                            if (WaitPickupStartEvent(100)) //10->50  // 디버깅떄는 50으로 늘려서 신호 확인하기 편하게
                            {
                                signalReceived = true;
                                // 신호를 받았으면 Start 이벤트를 끄지 않고 유지할지, 끌지는 프로토콜 결정.
                                // 보통은 "내가 인지했다"는 의미로 로직 진행하고,
                                // Rotary는 Done을 기다리므로 여기서는 Start를 굳이 바로 끌 필요는 없으나
                                // 기존 로직(ReSetPickupStartEvent)을 유지한다면:
                                ReSetPickupStartEvent();
                                _lastPickSucceeded = false;
                                break;
                            }

                            if (sw.ElapsedMilliseconds > timeoutMs)
                            {
                                Log.Write(UnitName, $"[OutputDieTransfer] Waiting for Done... Elapsed {sw.ElapsedMilliseconds}ms");
                                //break;
                                return 0;
                            }

                            // [추가] 
                            // 만약 현재 Rotary의 자재가 불량이라면, Start 신호가 영원히 안 옴 (Rotary가 안 보내니까).
                            // 따라서 불량 여부를 여기서도 체크해서 빠져나가야 함.
                            //if (Rotary != null)
                            //{
                            //    var currentDie = Rotary.GetUnloadSocketMaterial();
                            //    if (currentDie != null 
                            //       && (currentDie.State == DieProcessState.Skip 
                            //       || currentDie.State == DieProcessState.Rejected))
                            //    {
                            //        // 불량이면 Pick 대기하지 말고 리턴
                            //        return 0;
                            //    }
                            //}
                        }

                        // 신호 못 받았으면(타임아웃) 루프 종료 혹은 리턴
                        if (!signalReceived)
                        {
                            // 타임아웃 처리. 보통 그냥 return 0 하여 다음 사이클에 다시 대기
                            Log.Write(UnitName, "OnRunWork", "[OutputDieTransfer] WaitPickupStartEvent timeout");
                            return 0;
                        }

                        // Done 이벤트는 미리 리셋해둠 (Rotary가 기다리기 전에 청소)
                        ResetPickupDoneEvent();

                        // 1.Unload Socket의 Die Index 정보 확인 - 현재 픽업 대상 Die
                        // rotary에서 pickup 신호 받고 처리하자.
                        MaterialDie DieIndex = Rotary.GetUnloadSocketMaterial();
                        if (DieIndex == null || DieIndex.Presence != Material.MaterialPresence.Exist)
                        {
                            OutputStage.HasNextDie();

                            //ReSetPickupStartEvent();
                            //_lastPickSucceeded = false;
                            return 0;
                        }

                        if (DieIndex == null || DieIndex.Presence != Material.MaterialPresence.Exist)
                        {
                            SetPickupDoneEvent();
                            return 0;
                        }

                        // [핵심 해결책] 
                        // 만약 방금 가져온 Die의 Index가 이전에 이미 처리(Skip/Pick) 완료한 Index라면?
                        // 장비가 재시작되었을 때 Rotary가 아직 인덱스를 이동하지 않아서 발생할 수 있음.
                        if (DieIndex.Index == _lastProcessedDieIndex) 
                        {
                            // 로그를 남기고 이번 사이클은 무시 (Rotary가 다음으로 넘어갈 때까지 대기하거나 신호 처리)
                            // Log.Write(UnitName, "OnRunWork", $"Skip duplicate die processing. Index={currentDie.Index} is already processed.");

                            // 만약 Rotary가 PickDone을 기다리고 있다면 신호를 줘서 보내버려야 함.
                            // 신호를 기다리고 있지 않아도 보내자.
                            //if (WaitPickupStartEvent(1))
                            {
                                ReSetPickupStartEvent();
                                SetPickupDoneEvent();
                            }
                            return 0;
                        }

                        // 2. [SKIP 처리] 이미 전공정 불량인 경우 Pass
                        // rotary에서 pickup 신호 받고 처리하자.
                        if (DieIndex.ProcessSatate == MaterialProcessSatate.Skipped
                           || DieIndex.State == DieProcessState.Skip)
                        {
                            if (OutputStage != null)
                            {
                                if (OutputStage.TryReserveNextEmptyBin(DieIndex, out double binX, out double binY, out double dT, out var slot))
                                {
                                    OutputStage.MarkCurrentReservedMissing();
                                    if (slot != null)
                                    {
                                        slot.State = DieProcessState.Skip;
                                        OutputStage.UpdateUI();
                                    }
                                }
                            }

                            this.SetMaterial(null);
                            //Rotary.ReMoveMaterialToOutputDieTransfer(); // Rotary 소켓 비우기

                            // 1) 만약 Rotary가 PickStart 신호를 줬다면 받아서 소모시켜야 함
                            // (타임아웃을 짧게 줘서 신호가 와있는지만 확인)
                            //if (WaitPickupStartEvent(10))
                            {
                                //ReSetPickupStartEvent(); // Start 신호 초기화

                                _lastPickSucceeded = false;
                                // 2) Rotary에게 "나 작업(비록 Skip이지만) 끝났어"라고 알려줌
                                SetPickupDoneEvent();
                            }

                            // [ADD] 처리 완료된 Index 기록 (다음 루프에서 중복 처리 방지)
                            _lastProcessedDieIndex = DieIndex.Index;

                            _lastPickSucceeded = false;
                            // 상태 초기화 후 리턴 (다음 사이클 진행)
                            State = ProcessState.None;
                            Log.Write(UnitName, "OnRunWork", $"Die Index={DieIndex.Index} is Rejected/Skipped. Pass sequence.");
                            return 0;
                        }

                        Material waferBin = OutputStage.GetMaterialWafer();
                        Task<int> taskOutStageMoveToNextDIe = null;
                        if (waferBin != null
                            && waferBin.Presence == Material.MaterialPresence.Exist
                            && DieIndex != null
                            && DieIndex.Presence == Material.MaterialPresence.Exist)
                        {
                            // 이동과 칩 픽업 동시 수행을 위해 비동기 호출
                            taskOutStageMoveToNextDIe = MoveOutStageASync();
                        }

                        // 2.2 픽업 위치 이동
                        try
                        {
                            TaktStart("PickDie_ToolT");
                            nRet = MovePositionPickUpToolT_Index(nIndex);
                            if (nRet != 0)
                            {
                                SetPickupDoneEvent();
                                AxisOutputPickZ?.EmgStop();
                                AxisOutputToolT?.EmgStop();
                                PostAlarm((int)AlarmKeys.eOutputDieTransferMovePickUpToolT);
                                Log.Write(UnitName, "[ReleaseVacuumAndPlaceUp] MovePositionPickUpToolT_Index failed");
                                return -1;
                            }
                        }
                        finally
                        {
                            TaktEnd("PickDie_ToolT");
                        }
                        
                        try
                        {
                            TaktStart("PickDownDie");
                            nRet = PickDownDie();
                            if (nRet != 0)
                            {
                                SetPickupDoneEvent();
                                AxisOutputPickZ?.EmgStop();
                                AxisOutputToolT?.EmgStop();
                                PostAlarm((int)AlarmKeys.eOutputDieTransferChipPickDown);
                                Log.Write(UnitName, "[OnRunWork] PickDownDie failed");
                                return -1;
                            }
                        }
                        finally
                        {
                            TaktEnd("PickDownDie");
                        }

                        try
                        {
                            TaktStart("PickUpDie");
                            nRet = PickUpDie();
                            if (nRet != 0)
                            {
                                SetPickupDoneEvent();
                                AxisOutputPickZ?.EmgStop();
                                AxisOutputToolT?.EmgStop();
                                PostAlarm((int)AlarmKeys.eOutputDieTransferChipPickUp);
                                Log.Write(UnitName, "[OnRunWork] PickUpDie failed");
                                return -1;
                            }
                        }
                        finally
                        {
                            TaktEnd("PickUpDie");
                        }

                        //버큠 형성이 안되었으면 안가져다 놓으면 된다. 
                        // 2.5 [중요] Vacuum 최종 확인 및 분기 처리
                        vac = this.IsVacuumOK(nArmIndex);
                        if (vac == true)
                        {
                            // [ADD] 성공 시에도 처리된 Index 기록
                            _lastProcessedDieIndex = DieIndex.Index;
                            // === [성공 케이스] ===
                            Rotary.MoveMaterialToOutputDieTransfer();
                            DieIndex.State = DieProcessState.Picked;
                            DieIndex.ProcessSatate = Material.MaterialProcessSatate.Processing;
                            DieIndex.Presence = Material.MaterialPresence.Exist;

                            // ==============================================================================
                            // [핵심 변경 1] 
                            // Z축이 이미 안전 위치(PickUpDie 함수 내에서 상승 완료)에 있으므로,
                            // 여기서 즉시 완료 신호(Done)를 세팅하여 로터리가 지연 없이 Rotate()를 시작하게 합니다.
                            // 이후의 스테이지 이동 및 Tool 회전 동작은 Rotate와 '병렬(Overlap)'로 돌아가게 됩니다.
                            // ==============================================================================
                            SetPickupDoneEvent();

                            if (taskOutStageMoveToNextDIe != null)
                            {
                                // --- [택타임 개선 2] ---
                                // OutputStage(XY축)가 이동을 마칠 때까지 대기하는 시간을 활용해
                                // Tool T를 Place 위치로 먼저 돌려둡니다. (PickUpDie() 직후라 Z축은 이미 안전하게 UP 상태임)
                                if (!IsPositionPlaceZSafety() || !IsPositionPickZSafety())
                                {
                                    nRet = MovePositionSafetyZ();
                                    if (nRet != 0)
                                    {
                                        Log.Write(UnitName, "[OnMovePositionPlace] MovePositionSafetyZ faild");
                                        return -1;
                                    }
                                }
                                double dTPos = GetTP(OutputDieTransferRecipe.TeachingPositionName.Place.ToString(),
                                            AxisNames.RightToolT);
                                nRet = MoveAxisPositionOne(AxisOutputToolT, dTPos);
                                if (nRet != 0)
                                {
                                    AxisOutputPlaceZ?.EmgStop();
                                    AxisOutputToolT?.EmgStop();
                                    PostAlarm((int)AlarmKeys.eOutputDieTransferRotateToolTForPlace);
                                    Log.Write(UnitName, "[OnRunWork] PlaceDie_ToolT failed");
                                    return -1;
                                }

                                // 5초 대기 후 타임아웃 체크
                                if (!taskOutStageMoveToNextDIe.Wait(5000))
                                {
                                    Log.Write(UnitName, "[OnRunWork]", "taskOutStageMoveToNextDIe Timeout (5s)");
                                }

                                // 작업 실패(0이 아님) 체크
                                if (taskOutStageMoveToNextDIe.IsCompleted && taskOutStageMoveToNextDIe.Result != 0)
                                {
                                    SetPickupDoneEvent();
                                    AxisOutputPickZ?.EmgStop();
                                    AxisOutputToolT?.EmgStop();
                                    PostAlarm((int)AlarmKeys.eOutputDieTransferMoveOutStage);
                                    Log.Write(UnitName, "[OnRunWork] taskOutStageMoveToNextDIe failed");
                                    return -1;
                                }
                            }

                            //SetPickupDoneEvent();

                            _lastPickSucceeded = true;
                            State = ProcessState.Complete;
                            Log.Write(UnitName, "PickSuccess", $"Die Index={DieIndex.Index} marked Presence=Exist");
                        }
                        else
                        {
                            // [ADD] 성공 시에도 처리된 Index 기록
                            _lastProcessedDieIndex = DieIndex.Index;

                            // [실패 케이스 수정: Load Align 실패 / Pick Miss]
                            Log.Write(UnitName, "PickFail", "Vacuum Fail -> Skip Probe/Unloader sequence");

                            // 픽 실패 시 현재 예약 슬롯을 Rejected 처리해 다음 슬롯으로 진행할 수 있게 함
                            OutputStage?.MarkCurrentReservedMissing();

                            // Rotary에 자재가 그대로 있다고 처리 (혹은 ReMoveMaterialToOutputDieTransfer 로직 확인 필요)
                            // 여기서는 픽업 실패했으므로 Rotary 소켓에 자재가 남아야 함.
                            // Rotary.ReMoveMaterialToOutputDieTransfer(); // <- 이 함수 이름이 헷갈림. "Rotary에 자재 복구" 의미라면 맞음.

                            // 3. Die 정보 업데이트
                            DieIndex.State = DieProcessState.Rejected; // Rejected로 마킹
                            DieIndex.RejectReason = "PickFail";        // 사유 입력

                            if (taskOutStageMoveToNextDIe != null)
                            {
                                taskOutStageMoveToNextDIe.Wait();
                            }

                            // ==============================================================================
                            // [핵심 변경 2] 
                            // 실패 시에도 Z축은 안전 위치이므로 즉시 Done 신호를 발생시켜 로터리가 회전하도록 합니다.
                            // ==============================================================================
                            SetPickupDoneEvent();

                            //여기서 Ready? 위치로 가서 제품 버려야 겠다.
                            //제품을 가지고 있다고 착각한다. 
                            nRet = MovePositionReady();
                            if (nRet != 0)
                            {
                                SetPickupDoneEvent();
                                AxisOutputPickZ?.EmgStop();
                                AxisOutputToolT?.EmgStop();
                                PostAlarm((int)AlarmKeys.eOutputDieTransferError);
                                Log.Write(UnitName, "[OnRunWork] MovePositionReady failed");
                                return -1;
                            }

                            // (4) IO 정리: Vacuum 끄고 Blow 살짝
                            this.SetVacuum(nArmIndex, false);
                            this.SetBlow(nArmIndex, true);
                            Thread.Sleep(100); //Vaccum Fail시에. 
                            this.SetBlow(nArmIndex, false);

                            //정보를 버리고 ==
                            this.SetMaterial(null);
                            State = ProcessState.None;
                            _lastPickSucceeded = true;

                            Log.Write(UnitName, "PickFail", $"Die Index={DieIndex.Index} Process Pass(Rejected).");
                            try
                            {
                                var ctx = Equipment.Instance.SummaryContext;
                                ctx.GetCurrentSummaryOrNull()?.AddULdPickAsMiss();

                                AxisOutputPickZ?.EmgStop();
                                AxisOutputToolT?.EmgStop();
                                PostAlarm((int)AlarmKeys.eOutputDieTransferLdPickAsMissError);
                            }
                            catch (Exception ex)
                            { Log.Write(ex); }

                            //미리 키고 대기.
                            this.SetVacuum(nArmIndex, true);

                            // [핵심] return -1 (에러 정지) 대신 return 0 (정상 진행)으로 변경
                            return 0;
                        }
                    }
                }
                catch (Exception ex)
                {
                    // 에러 시에도 로터리 무한 대기를 막기 위해 Done 발생
                    SetPickupDoneEvent();
                    Log.Write(ex);
                    return -1; // 예외 발생 시에는 정지
                }

                return 0;
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                return -1;
            }
        }

        protected override int OnRunComplete()
        {
            try
            {
                int nRtn = 0;

                TaktStart("Bin Place Die");
                if (IsBinStageReadyForPlace() == false || OutputStage.CanPlaceDie() == false)
                {
                    State = ProcessState.None;
                    return 0;
                }

                Material waferBin = OutputStage.GetMaterialWafer();
                MaterialDie die = GetMaterial() as MaterialDie;

                if (waferBin != null
                    && waferBin.Presence == Material.MaterialPresence.Exist
                    && die != null
                    && die.Presence == Material.MaterialPresence.Exist)
                {
                    try
                    {
                        TaktStart("PlaceDie_ToolT");
                        nRtn = PlaceDie_ToolT();
                        if (nRtn != 0)
                        {
                            AxisOutputPlaceZ?.EmgStop();
                            AxisOutputToolT?.EmgStop();
                            PostAlarm((int)AlarmKeys.eOutputDieTransferRotateToolTForPlace);
                            Log.Write(UnitName, "[OnRunWork] PlaceDie_ToolT failed");
                            return -1;
                        }
                    }
                    finally
                    {
                        TaktEnd("PlaceDie_ToolT");
                    }

                    TaktStart("PlaceUp");
                    try
                    {
                        nRtn = PlaceUp();
                        if (nRtn != 0)
                        {
                            Log.Write(UnitName, "[OnRunWork] AddULdPlaceAsMiss");
                            try
                            {
                                var ctx = Equipment.Instance.SummaryContext;
                                ctx.GetCurrentSummaryOrNull()?.AddULdPlaceAsMiss();
                            }
                            catch (Exception ex)
                            {
                                Log.Write(ex);
                            }

                            AxisOutputPlaceZ?.EmgStop();
                            AxisOutputToolT?.EmgStop();
                            PostAlarm((int)AlarmKeys.eOutputDieTransferReleaseVacuumAndPlaceUp);
                            Log.Write(UnitName, "[OnRunWork] ReleaseVacuumAndPlaceUp failed");
                            return -1;
                        }
                    }
                    finally
                    {
                        TaktEnd("PlaceUp");
                    }
                    
                    die.State = DieProcessState.Placed;
                    die.ProcessSatate = Material.MaterialProcessSatate.Completed;
                    die.Presence = Material.MaterialPresence.Exist;
                    Log.Write(UnitName, "PlaceStart", $"Die Index={die.Index} Presence=Exist Placed");

                    OutputStage.PlaceDie(die);
                    this.SetMaterial(null);
                }

                TaktEnd("Bin Place Die");
                State = ProcessState.None;
                return 0;
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                return -1;
            }
            finally
            {
                TaktEnd("One Cycle");
            }
        }
        #endregion

        #region Sequence 등록

        protected override void OnMakeSequence()
        {
            base.OnMakeSequence();
            this.SequencePlayers.Add(MoveOutStage);
            this.SequencePlayers.Add(PickDownDie);
            this.SequencePlayers.Add(PickUpDie);
            this.SequencePlayers.Add(PlaceDie_ToolT);
            this.SequencePlayers.Add(PlaceUp);

        }

        #endregion

        #region Seq 단위 동작 함수
        private bool IsBinStageReadyForPlace()
        {
            try
            {
                if (OutputStage == null)
                    return false;
                
                if (OutputStage.IsPlateDown() == false)
                    return false;
                //if (!OutputStage.IsPositionBinCenter()) return false;
                return true;
            }
            catch
            {
                Log.Write(UnitName, "IsBinStageReadyForPlace", "catch");
                return false;
            }
        }

        public Task<int> MoveOutStageASync(bool bFineSpeed = false)
        {
            return Task.Run(() =>
            {
                return MoveOutStage(bFineSpeed);
            });
        }
        
        public int MoveOutStage(bool bFineSpeed = false) 
        {
            int nRet = 0;
            if (RunMode == UnitRunMode.Manual)
            {
                this.CurrentFunc = MoveOutStage;
            }

            // Chip을 순차적으로 Place 하는 위치로 이동
            // 다음 빈 Bin 예약 및 이동
            if (OutputStage == null)
                return -1;

            if (OutputStage.IsPlateUp())
            {
                if (Config.IsSimulation == false)
                {
                    PostAlarm((int)AlarmKeys.eOutputStageAxesMoving);
                    return -1;
                }
            }

            var DieOutTr = this.GetMaterial() as MaterialDie;
            var DieIndex = this.Rotary?.GetUnloadSocketMaterial();
            if (OutputStage.TryReserveNextEmptyBin(DieIndex, out double binX, out double binY, out double dT, out var slot) == false)
            {
                //Log.Write(UnitName, "[MoveOutStage] No empty bin slot.");
                return 0; // 더 놓을 자리가 없으면 정상 종료로 간주
            }

            nRet = OutputStage.MoveToBinPosition(binX, binY, dT, bFineSpeed);
            if (nRet != 0)
            {
                Log.Write(UnitName, "[MoveOutStage] MoveToBinPosition failed");
                return -1;
            }

            return nRet;
        }

        public int PickDownDie(bool bFineSpeed = false)
        {
            int nRet = 0;
            if (RunMode == UnitRunMode.Manual)
            {
                this.CurrentFunc = PickDownDie;
            }

            int nIndex = 0;
            int nArmindex = 0;
            nIndex = GetUnloaderIndexNo();
            nArmindex = GetPlaceArmIndex();
            double tPos = 0, zPos = 0;
            try { tPos = AxisOutputToolT?.GetPosition() ?? 0; } catch { }
            try { zPos = AxisOutputPickZ?.GetPosition() ?? 0; } catch { }
            Log.Write(UnitName, $"[PickDown] idx={nIndex}, arm={nArmindex}, ToolT={tPos:F3}, PickZ={zPos:F3}");

            this.SetVacuum(nArmindex, true);

            // 1) ToolT 위치 확인.
            if (IsPositionPickUpToolT() == false)
            {
                nRet = MovePositionPickUpToolT_Index(nIndex, bFineSpeed);
                if (nRet != 0)
                {
                    Log.Write(UnitName, "[ChipPickDown] MovePositionPickUpToolT_Index failed");
                    return -1;
                }
            }

            nRet = MovePositionPickUpPickZ_Index(nIndex, bFineSpeed);
            if (nRet != 0)
            {
                Log.Write(UnitName, "[ChipPickDown] MovePositionPickUpPickZ_Index failed");
                return -1;
            }

            nRet = WaitVacuumStateOrAlarm(nArmindex, true);
            if (nRet != 0)
            {
                Log.Write(UnitName, "[OutputDieTrVacuumOn] Vacuum Timeout");
                return -1;
            }

            return nRet;
        }

        public int PickUpDie(bool bFineSpeed = false)
        {
            int nRet = 0;
            if (RunMode == UnitRunMode.Manual)
            {
                this.CurrentFunc = PickUpDie;
            }

            int nArmindex = GetPlaceArmIndex();
            int nIndex = GetUnloaderIndexNo();
            Log.Write(UnitName, $"[PickUp] idx={nIndex}, arm={nArmindex}");

            if (Rotary.SetVacuum(nIndex, false) == false)
            {
                if (!Config.IsSimulation)
                {
                    PostAlarm((int)AlarmKeys.eOutputDieTransferVent);
                    Log.Write(UnitName, "[ChipPickUp]", "SetBlow failed");
                    return -1;
                }
            }
            Thread.Sleep(1);

            if (Rotary.Config.UsePlaceVent == true)
            {
                if (Rotary.SetVent(nIndex, true) == false)
                {
                    if (!Config.IsSimulation)
                    {
                        PostAlarm((int)AlarmKeys.eOutputDieTransferVent);
                        Log.Write(UnitName, "[ChipPickUp]", "SetBlow failed");
                        return -1;
                    }
                }
                Thread.Sleep(1);
            }

            if (Rotary.SetVent(nIndex, false) == false)
            {
                if (!Config.IsSimulation)
                {
                    PostAlarm((int)AlarmKeys.eOutputDieTransferVent);
                    Log.Write(UnitName, "[ChipPickUp]", "SetBlow failed");
                    return -1;
                }
            }
            Thread.Sleep(1);

            if(Rotary.Config.UsePlaceBlow == true )
            {
                if (Rotary.SetBlow(nIndex, true) == false)
                {
                    if (!Config.IsSimulation)
                    {
                        PostAlarm((int)AlarmKeys.eOutputDieTransferBlow);
                        Log.Write(UnitName, "[ChipPickUp]", "SetBlow failed");
                        return -1;
                    }
                }
            }

            Thread.Sleep(Config.PickUpWaitTime);
            //Thread.Sleep(50);   //대기 <- 설정 파라미터로 필요? //100 > 50

            nRet = MovePositionSafetyZ(bFineSpeed);
            if (nRet != 0)
            {
                Log.Write(UnitName, "[ChipPickUp]", "MovePositionSafetyZ failed");
                return -1;
            }

            if (Rotary.SetBlow(nIndex, false) == false)
            {
                if (!Config.IsSimulation)
                {
                    PostAlarm((int)AlarmKeys.eOutputDieTransferBlow);
                    Log.Write(UnitName, "[ChipPickUp]", "SetBlow failed");
                    return -1;
                }   
            }

            Rotary.SetVacuum(nIndex, false);
            Thread.Sleep(1);

            // 아래 구문은 우선 확인하지 않음.
            //Rotary Vacuum Off 확인.
            nRet = Rotary.WaitVacuumStateOrAlarm(nArmindex, false);
            if (nRet != 0)
            {
                Log.Write(UnitName, "[RotaryVacuumOff] Vacuum Timeout");
                return -1;
            }

            //Thread.Sleep(50); //??

            // OutputDieTransferVacuumOn 확인.
            // 여기서도 버큠 형성이 안됨. 우석 막자.
            nRet = WaitVacuumStateOrAlarm(nArmindex, true);
            if (nRet != 0)
            {
                Log.Write(UnitName, "[OutputDieTrVacuumOn] Vacuum Timeout");
                return -1;
            }

            return nRet;
        }

        public int PlaceDie_ToolT(bool bFineSpeed = false)
        {
            if (AxisOutputToolT == null)
                return -1;

            int nRet = 0;
            if (RunMode == UnitRunMode.Manual)
            {
                this.CurrentFunc = PlaceDie_ToolT;

            }

            nRet = MovePositionPlace(bFineSpeed);
            if (nRet != 0)
            {
                Log.Write(UnitName, "[RotateToolTForPlace] MovePositionPlace 실패");
                return -1;
            }

            return nRet;
        }

        public int PlaceUp(bool bFindSpeed = false)
        {
            int nRet = 0;
            try
            {
                if (RunMode == UnitRunMode.Manual)
                {
                    this.CurrentFunc = PlaceUp;
                    LogSequence("Start");
                }

                int nIndex = GetUnloaderIndexNo();
                int armIndex = GetPlaceArmIndex();
                if (armIndex < 0 || armIndex > 3) 
                    return -1;

                // Release
                if(SetVacuum(armIndex, false) == false)
                {
                    if(Config.IsSimulation == false && (Config.IsDryRun == false && IsDryRunEqp == false))
                    {
                        PostAlarm((int)AlarmKeys.eOutputDieTransferVacuum);
                        Log.Write(UnitName, "[ReleaseVacuumAndPlaceUp] SetVacuum failed");
                        return -1;
                    }
                }
                SetVent(armIndex, true);
                Thread.Sleep(1);
                SetVent(armIndex, false);

                SetBlow(armIndex, true);
                Thread.Sleep(Config.PlaceUpWaitTime);
                //Thread.Sleep(100); //place wait time

                nRet = MovePositionSafetyPlaceZ(bFindSpeed);
                //nRet = MovePositionSafetyZ(bFindSpeed);
                if (nRet != 0)
                {
                    Log.Write(UnitName, "[ReleaseVacuumAndPlaceUp] MovePositionSafetyPlaceZ 실패");
                    return -1;
                }

                SetBlow(armIndex, false);

                while (IsPositionPickZSafety() == false 
                      || IsPositionPlaceZSafety() == false)
                {
                    if(IsStop)
                    {
                        return 0;
                    }
                    Thread.Sleep(1);
                }

                SetVacuum(armIndex, true);
                
                nRet = MovePositionReady();
                if (nRet != 0)
                {
                    Log.Write(UnitName, "[ReleaseVacuumAndPlaceUp] MovePositionReady failed");
                    return -1;
                }

                //언로더 안하면 이걸로 보내서 시컨스 진행.
                //nRet = MovePositionPickUpToolT_Index(nIndex, bFindSpeed);
                //if (nRet != 0)
                //{
                //    Log.Write(UnitName, "[ReleaseVacuumAndPlaceUp] MovePositionPickUpToolT_Index failed");
                //    return -1;
                //}
            }
            catch (Exception ex)
            {
                AxisOutputToolT?.EmgStop();
                AxisOutputPickZ?.EmgStop();
                AxisOutputPlaceZ?.EmgStop();
                Log.Write(ex);
                nRet = -1;
                PostAlarm((int)AlarmKeys.eOutputDieTransferError);
            }
            finally
            {
                LogSequence("End");
            }

            return nRet;
        }

        private int GetPlaceArmIndex()
        {
            //todo: 구현해라 구부장. 암 하나 더달면. Rotary Index에 따른 Arm Index 반환

            //if(this.AxisToolT.GetPosition() > 10)
            //{

            //}
            return 0;
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

            if (IsPositionToolTReady() == false)
            {
                nRet = MovePositionReady();
                if (nRet != 0)
                {
                    Log.Write(UnitName, "OnEnsureReady Fail - MovePositionReady");
                    return nRet;
                }
            }

            return nRet;
        }
        #endregion

        public int ManualResetForNewRun(bool bFine = false)
        {
            ResetForNewRun(true, true);
            return 0;
        }

        public void ResetForNewRun(bool moveToSafeReady = true, bool clearHeldDie = true)
        {
            // 1) 런타임/시퀀스 플래그 초기화
            _isSafetyMoving = false;
            CompleteOutputDie = false;
            _lastPickSucceeded = false;
            this.CurrentFunc = null;
            // [ADD] Index 기억 초기화
            _lastProcessedDieIndex = -1;

            // 2) 핸드셰이크 리셋
            try 
            { 
                ResetPickupHandshake(); 
            } 
            catch 
            {
                Log.Write(UnitName, "ResetForNewRun", "catch");
            }

            // 3) 보유 다이 제거(선택)
            if (clearHeldDie)
            {
                try { this.SetMaterial(null); } catch (Exception ex) { Log.Write(UnitName, $"[ResetForNewRun] SetMaterial(null) failed: {ex.Message}"); }
            }

            // 5) 축 안전/Ready 복귀(선택)
            if (moveToSafeReady)
            {
                try
                {
                    // 인접 유닛 정지 대기(타임아웃 10s)
                    var sw = System.Diagnostics.Stopwatch.StartNew();
                    const int timeoutMs = 10000;
                    while ((Rotary?.IsAnyAxisMoving() ?? false) || (OutputStage?.IsAnyAxisMoving() ?? false))
                    {
                        if (IsStop) return;
                        if (sw.ElapsedMilliseconds > timeoutMs) break;
                        Thread.Sleep(1);
                    }

                    // Z 안전 → Ready 순서로 복귀
                    EnsureReady();
                    MovePositionReady();

                    this.SetVacuum(0, false);
                    this.SetVent(0, false);
                    this.SetBlow(0, true);
                    Thread.Sleep(100);
                    this.SetBlow(0, false);
                }
                catch (Exception ex)
                {
                    Log.Write(UnitName, $"[ResetForNewRun] Move to safe/ready failed: {ex.Message}");
                }
            }
        }


        public int MoveToTeachingPositionBySelectionIndex(int teachingSelIndex, bool isFine = false)
        {
            if (Config == null)
                return -1;

            string tpName;
            if (!Config.GetTeachingPositionName(teachingSelIndex, out tpName) || string.IsNullOrWhiteSpace(tpName))
                return -1;

            OutputDieTransferRecipe.TeachingPositionName en;
            if (!Enum.TryParse(tpName, out en))
                return -1;

            int nRet = 0;
            int nIndex = -1;

            switch (en)
            {
                case OutputDieTransferRecipe.TeachingPositionName.Pickup_Index1:
                    nIndex = 0;
                    nRet = MovePositionPickUpToolT_Index(nIndex, isFine);
                    if (nRet != 0)
                    {
                        return nRet;
                    }
                    // PickZ 이동
                    nRet = MovePositionPickUpPickZ_Index(nIndex, isFine);
                    if (nRet != 0)
                    {
                        return nRet; ;
                    }
                    break;
                case OutputDieTransferRecipe.TeachingPositionName.Pickup_Index2:
                    nIndex = 1;
                    // ToolT 이동
                    nRet = MovePositionPickUpToolT_Index(nIndex, isFine);
                    if (nRet != 0)
                    {
                        return nRet; ;
                    }
                    // PickZ 이동
                    nRet = MovePositionPickUpPickZ_Index(nIndex, isFine);
                    if (nRet != 0)
                    {
                        return nRet; ;
                    }
                    break;
                case OutputDieTransferRecipe.TeachingPositionName.Pickup_Index3:
                    nIndex = 2;
                   
                    // ToolT 이동
                    nRet = MovePositionPickUpToolT_Index(nIndex, isFine);
                    if (nRet != 0)
                    {
                        return nRet;
                    }
                    // PickZ 이동
                    nRet = MovePositionPickUpPickZ_Index(nIndex, isFine);
                    if (nRet != 0)
                    {
                        return nRet; ;
                    }
                    break;
                case OutputDieTransferRecipe.TeachingPositionName.Pickup_Index4:
                    nIndex = 3;
                    
                    // ToolT 이동
                    nRet = MovePositionPickUpToolT_Index(nIndex, isFine);
                    if (nRet != 0)
                    {
                        return nRet; ;
                    }
                    // PickZ 이동
                    nRet = MovePositionPickUpPickZ_Index(nIndex, isFine);
                    if (nRet != 0)
                    {
                        return nRet; ;
                    }
                    break;
                case OutputDieTransferRecipe.TeachingPositionName.Pickup_Index5:
                    nIndex = 4;
                    
                    // ToolT 이동
                    nRet = MovePositionPickUpToolT_Index(nIndex, isFine);
                    if (nRet != 0)
                    {
                        return nRet; ;
                    }
                    // PickZ 이동
                    nRet = MovePositionPickUpPickZ_Index(nIndex, isFine);
                    if (nRet != 0)
                    {
                        return nRet; ;
                    }
                    break;
                case OutputDieTransferRecipe.TeachingPositionName.Pickup_Index6:
                    nIndex = 5;
                    
                    // ToolT 이동
                    nRet = MovePositionPickUpToolT_Index(nIndex, isFine);
                    if (nRet != 0)
                    {
                        return nRet; ;
                    }
                    // PickZ 이동
                    nRet = MovePositionPickUpPickZ_Index(nIndex, isFine);
                    if (nRet != 0)
                    {
                        return nRet; ;
                    }
                    break;
                case OutputDieTransferRecipe.TeachingPositionName.Pickup_Index7:
                    nIndex = 6;
                    
                    // ToolT 이동
                    nRet = MovePositionPickUpToolT_Index(nIndex, isFine);
                    if (nRet != 0)
                    {
                        return nRet; ;
                    }
                    // PickZ 이동
                    nRet = MovePositionPickUpPickZ_Index(nIndex, isFine);
                    if (nRet != 0)
                    {
                        return nRet; ;
                    }
                    break;
                case OutputDieTransferRecipe.TeachingPositionName.Pickup_Index8:
                    nIndex = 7;
                   
                    // ToolT 이동
                    nRet = MovePositionPickUpToolT_Index(nIndex, isFine);
                    if (nRet != 0)
                    {
                        return nRet; ;
                    }
                    // PickZ 이동
                    nRet = MovePositionPickUpPickZ_Index(nIndex, isFine);
                    if (nRet != 0)
                    {
                        return nRet; ;
                    }
                    break;

                case OutputDieTransferRecipe.TeachingPositionName.Ready:
                    nRet = MovePositionReady(isFine);
                    if (nRet != 0)
                    {
                        return nRet; 
                    }
                    break;

                case OutputDieTransferRecipe.TeachingPositionName.SafetyZone:
                    nRet = MovePositionSafetyZ(isFine);
                    if (nRet != 0)
                    {
                        return nRet;
                    }
                    break;

                case OutputDieTransferRecipe.TeachingPositionName.Place:
                    // Place 내부에서 필요 시 SafetyZ를 자체 보장
                    nRet = MovePositionPlace(isFine);
                    if (nRet != 0)
                    {
                        return nRet;
                    }
                    break;

                default:
                    break;
            }

            return nRet;
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
    }
}