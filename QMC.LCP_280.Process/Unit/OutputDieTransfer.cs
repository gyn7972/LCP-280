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
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms.DataVisualization.Charting;
using static QMC.LCP_280.Process.Equipment;
using static QMC.LCP_280.Process.Unit.OutputDieTransferConfig.IO; // IO 상수/배열

namespace QMC.LCP_280.Process.Unit
{
    public class OutputDieTransfer : BaseUnit<OutputDieTransferConfig>
    {
        public enum AlarmKeys
        {
            eOutputDieTransferError = 6001,
            eOuputDieTransferZNotSafety = 6002,
            eOutputStageAxesMoving = 6003,
            eRotaryAxesMoving = 6004,
            eOutputDieTransferVacuum = 6005,
            eOutputDieTransferVent = 6006,
            eOutputDieTransferBlow = 6007,
            eBinStageCylinderZNotSafety = 6008,
        }

        #region InitAlarm
        protected override void InitAlarm()
        {
            base.InitAlarm();
            AlarmInfo alarm = new AlarmInfo();
            alarm.Code = (int)AlarmKeys.eOuputDieTransferZNotSafety;
            alarm.Title = "Die Tr Z-Axis Not Sfarety Pos.";
            alarm.Cause = "Die TrZAxis이 안전 위치가 아닙니다. 포지션 확인 후 다시 시작 하십시요.";
            alarm.Source = this.UnitName;
            alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
            m_dicAlarms.Add(alarm.Code, alarm);

            alarm = new AlarmInfo();
            alarm.Code = (int)AlarmKeys.eOutputDieTransferError;
            alarm.Title = "Output Die Transfer Error";
            alarm.Cause = "Output Die Transfer에서 알수 없는 에러가 발생했습니다.";
            alarm.Source = this.UnitName;
            alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
            m_dicAlarms.Add(alarm.Code, alarm);

            alarm = new AlarmInfo();
            alarm.Code = (int)AlarmKeys.eOutputStageAxesMoving;
            alarm.Title = "Output Stage Axis Moving";
            alarm.Cause = "Output Stage Axis가 동작중입니다. Output Stage Axis 동작이 완료된 후 다시 시작 하십시요.";
            alarm.Source = this.UnitName;
            alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
            m_dicAlarms.Add(alarm.Code, alarm);

            alarm = new AlarmInfo();
            alarm.Code = (int)AlarmKeys.eRotaryAxesMoving;
            alarm.Title = "Rotary Axis Moving";
            alarm.Cause = "Rotary Axis가 동작중입니다. Rotary Axis 동작이 완료된 후 다시 시작 하십시요.";
            alarm.Source = this.UnitName;
            alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
            m_dicAlarms.Add(alarm.Code, alarm);

            //eOutputDieTransferVacuum
            alarm = new AlarmInfo();
            alarm.Code = (int)AlarmKeys.eOutputDieTransferVacuum;
            alarm.Title = "Output Die Transfer Vacuum Error";
            alarm.Cause = "Output Die Transfer Vacuum이 Off 상태입니다. Vacuum 상태를 확인 후 다시 시작 하십시요.";
            alarm.Source = this.UnitName;
            alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
            m_dicAlarms.Add(alarm.Code, alarm);


            //
            alarm = new AlarmInfo();
            alarm.Code = (int)AlarmKeys.eOutputDieTransferVent;
            alarm.Title = "Output Die Transfer Vent Error";
            alarm.Cause = "Output Die Transfer Vent가 Off 상태입니다. Vent 상태를 확인 후 다시 시작 하십시요.";
            alarm.Source = this.UnitName;
            alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
            m_dicAlarms.Add(alarm.Code, alarm);
            //
            alarm = new AlarmInfo();
            alarm.Code = (int)AlarmKeys.eOutputDieTransferBlow;
            alarm.Title = "Output Die Transfer Blow Error";
            alarm.Cause = "Output Die Transfer Blow가 Off 상태입니다. Blow 상태를 확인 후 다시 시작 하십시요.";
            alarm.Source = this.UnitName;
            alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
            m_dicAlarms.Add(alarm.Code, alarm);
            //
            alarm.Code = (int)AlarmKeys.eBinStageCylinderZNotSafety;
            alarm.Title = "Bin Stage Z-Cylinder Not Safety Pos.";
            alarm.Cause = "Bin Stage Z-Cylinder가 안전 위치가 아닙니다.\n 포지션 확인 후 다시 시작 하십시요.";
            alarm.Source = this.UnitName;
            alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
            m_dicAlarms.Add(alarm.Code, alarm);
        }

        #endregion

        #region Unit
        Rotary Rotary { get; set; }
        OutputStage OutputStage { get; set; }

        #endregion

        #region Axis Helpers
        private MotionAxis _toolT, _pickZ, _placeZ;
        public MotionAxis AxisToolT => _toolT;
        public MotionAxis AxisPickZ => _pickZ;
        public MotionAxis AxisPlaceZ => _placeZ;
        // Safety 동작 중 여부
        private bool _isSafetyMoving = false;

        private void BindAxes()
        {
            var mgr = Equipment.Instance?.AxisManager;
            if (mgr == null)
            {
                Log.Write("OutputDieTransfer", "[BindAxes] AxisManager null");
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
            if (baseComponent == this.AxisPickZ)
            {
                if (_isSafetyMoving)
                    return true;
                if (this.Rotary.IsAxisMoving(AxisNames.IndexT))
                {
                    AxisPickZ?.EmgStop();
                    PostAlarm((int)AlarmKeys.eRotaryAxesMoving);
                    return false;
                }
            }
            else if (baseComponent == this.AxisPlaceZ)
            {
                if (_isSafetyMoving)
                    return true;
                if (this.OutputStage.IsAxisMoving(AxisNames.BinStageX) ||
                    this.OutputStage.IsAxisMoving(AxisNames.BinStageY) ||
                    this.OutputStage.IsAxisMoving(AxisNames.BinStageT))
                {
                    AxisPlaceZ?.EmgStop();
                    PostAlarm((int)AlarmKeys.eOutputStageAxesMoving);
                    return false;
                }
                if (this.OutputStage.IsPlateDown() == false)
                {
                    AxisPlaceZ?.EmgStop();
                    PostAlarm((int)AlarmKeys.eBinStageCylinderZNotSafety);
                    return false;
                }
            }
            else if (baseComponent == this.AxisToolT)
            {
                if (this.IsPositionPickZSafety() == false || this.IsPositionPlaceZSafety() == false)
                {
                    AxisToolT?.EmgStop();
                    PostAlarm((int)AlarmKeys.eOuputDieTransferZNotSafety);
                    return false;
                }
                if (this.OutputStage.IsPlateDown() == false)
                {
                    AxisPlaceZ?.EmgStop();
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

        public OutputDieTransfer(OutputDieTransferConfig config = null)
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
                if (axis == AxisPickZ)
                {
                    if (Rotary.IsAnyAxisMoving())
                    {
                        AxisToolT.EmgStop();
                        AxisPickZ.EmgStop();
                        AxisPlaceZ.EmgStop();
                        PostAlarm((int)AlarmKeys.eRotaryAxesMoving);
                    }
                }

                if (axis == AxisPlaceZ)
                {
                    if (OutputStage.IsAnyAxisMoving())
                    {
                        AxisToolT.EmgStop();
                        AxisPickZ.EmgStop();
                        AxisPlaceZ.EmgStop();
                        PostAlarm((int)AlarmKeys.eOutputStageAxesMoving);
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
                return MoveTeachingPositionOnce((int)OutputDieTransferConfig.TeachingPositionName.SafetyZone, isFine);
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

                    Thread.Sleep(5); // 0→5ms로 약간 여유 (CPU 점유 감소)
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

                Thread.Sleep(0);
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
            if (OutputStage != null && OutputStage.IsAnyAxisMoving())
            {
                AxisToolT?.EmgStop();
                AxisPickZ?.EmgStop();
                AxisPlaceZ?.EmgStop();
                PostAlarm((int)AlarmKeys.eOutputStageAxesMoving);
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
                Thread.Sleep(0);
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
                    return -1;
                }
            }

            // Teaching 이름 확인
            string tpName;
            if (!TryGetPickupTeachingName(nIndex, out tpName))
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
            if (Rotary != null && Rotary.IsAnyAxisMoving())
            {
                AxisToolT?.EmgStop();
                AxisPickZ?.EmgStop();
                AxisPlaceZ?.EmgStop();
                PostAlarm((int)AlarmKeys.eRotaryAxesMoving);
                return -1;
            }
            return 0;
        }

        public int MovePositionPickUpToolT_Index(bool isFine = false)
        {
            int index = GetUnloaderIndexNo();

            Task<int> task = MovePositionAsyncPickUpToolT_Index(isFine, index);
            while (!IsEndTask(task))
            {
                int interlock = IsMoveInterLockPickUpToolT_Index(index);
                if (interlock != 0)
                {
                    return -1;
                }
                Thread.Sleep(0);
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
                    return -1;
                }
            }

            // Teaching 이름 확인
            string tpName;
            if (!TryGetPickupTeachingName(nIndex, out tpName))
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
                    return -1;
                }
            }
            return 0;
        }

        public int MovePositionPickUpPickZ_Index(bool isFine = false)
        {
            int index = GetUnloaderIndexNo();

            Task<int> task = MovePositionAsyncPickUpPickZ_Index(isFine, index);
            while (!IsEndTask(task))
            {
                int interlock = IsMoveInterLockPickUpPickZ_Index(index);
                if (interlock != 0)
                {
                    return -1;
                }
                Thread.Sleep(0);
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
            if (!TryGetPickupTeachingName(nIndex, out tpName))
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
            if (Rotary != null && Rotary.IsAnyAxisMoving())
            {
                AxisToolT?.EmgStop();
                AxisPickZ?.EmgStop();
                AxisPlaceZ?.EmgStop();
                PostAlarm((int)AlarmKeys.eRotaryAxesMoving);
                return -1;
            }
            return 0;
        }

        private int MoveToolT_ToPickupIndex(string tpName, bool isFine)
        {
            double target = GetTP(tpName, AxisNames.RightToolT);
            int r = MoveAxisPositionOne(AxisToolT, target, isFine);
            if (r != 0)
            {
                Log.Write(UnitName, $"[MoveToolT_ToPickupIndex] ToolT move failed tp={tpName} pos={target}");
                return -1;
            }
            return 0;
        }
        private int MovePickZ_ToPickupIndex(string tpName, bool isFine)
        {
            double target = GetTP(tpName, AxisNames.RightPickZ);
            int r = MoveAxisPositionOne(AxisPickZ, target, isFine);
            if (r != 0)
            {
                Log.Write(UnitName, $"[MovePickZ_ToPickupIndex] PickZ move failed tp={tpName} pos={target}");
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
                            AxisToolT?.EmgStop();
                            AxisPickZ?.EmgStop();
                            AxisPlaceZ?.EmgStop();
                        }
                        catch { }
                        return -999; // 취소 코드
                    }

                    int nRtn = IsMoveInterLockPickUp_Index(nIndex);
                    if (nRtn != 0)
                    {
                        return -1;
                    }

                    Thread.Sleep(5); // 0→5ms로 약간 여유 (CPU 점유 감소)
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

                Thread.Sleep(0);
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
                    return -1;
                }
            }

            double dTPos = GetTP(OutputDieTransferConfig.TeachingPositionName.Place.ToString(),
                        AxisNames.RightToolT);
            nRet = MoveAxisPositionOne(AxisToolT, dTPos);
            if (nRet != 0)
            {
                return -1;
            }

            double dZPos = GetTP(OutputDieTransferConfig.TeachingPositionName.Place.ToString(),
                        AxisNames.RightPlaceZ);
            nRet = MoveAxisPositionOne(AxisPlaceZ, dZPos);
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
                AxisToolT?.EmgStop();
                AxisPickZ?.EmgStop();
                AxisPlaceZ?.EmgStop();
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
                            AxisToolT?.EmgStop();
                            AxisPickZ?.EmgStop();
                            AxisPlaceZ?.EmgStop();
                        }
                        catch { }
                        return -999; // 취소 코드
                    }

                    int nRtn = IsMoveInterLockPlace();
                    if (nRtn != 0)
                    {
                        return -1;
                    }

                    Thread.Sleep(5); // 0→5ms로 약간 여유 (CPU 점유 감소)
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
                                      bool useOffset = true,
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

            var tp = Config.GetTeachingPosition(teachingName);
            if (tp == null)
            {
                return false;
            }

            double target = 0.0;
            bool targetResolved = false;

            if (useOffset)
            {
                // OutputDieTransferConfig.GetPositionWithOffset() 의 반환 순서를 가정: (ToolT, PickZ, PlaceZ)
                var tuple = Config.GetPositionWithOffset(teachingName);
                double t = tuple.Item1;
                double pz = tuple.Item2;
                double plz = tuple.Item3;

                if (string.Equals(axisName, AxisNames.RightToolT, StringComparison.OrdinalIgnoreCase))
                {
                    target = t;
                    targetResolved = true;
                }
                else if (string.Equals(axisName, AxisNames.RightPickZ, StringComparison.OrdinalIgnoreCase))
                {
                    target = pz;
                    targetResolved = true;
                }
                else if (string.Equals(axisName, AxisNames.RightPlaceZ, StringComparison.OrdinalIgnoreCase))
                {
                    target = plz;
                    targetResolved = true;
                }
                else
                {
                    // TeachingPosition 내 원시 축 좌표를 직접 조회 (호환성)
                    if (tp.AxisPositions != null && tp.AxisPositions.ContainsKey(axisName))
                    {
                        target = tp.AxisPositions[axisName];
                        targetResolved = true;
                    }
                }
            }
            else
            {
                if (tp.AxisPositions != null && tp.AxisPositions.ContainsKey(axisName))
                {
                    target = tp.AxisPositions[axisName];
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

                var tp = Config.GetTeachingPosition(name);
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
            string[] candidates = new string[]
            {
                "SafetyPos",
                OutputDieTransferConfig.TeachingPositionName.SafetyZone.ToString()
            };

            bool result = IsAxisSafetyPos(AxisPickZ,
                                          AxisNames.RightPickZ,
                                          candidates,
                                          treatMissingAsSafe,
                                          fallbackTolerance,
                                          useAxisInposTolerance);
            return result;
        }

        public bool IsPositionToolTSafety(double fallbackTolerance = 0.01,
                                     bool useAxisInposTolerance = true,
                                     bool treatMissingAsSafe = true)
        {
            string[] candidates = new string[]
            {
                "SafetyPos",
                OutputDieTransferConfig.TeachingPositionName.SafetyZone.ToString()
            };

            bool result = IsAxisSafetyPos(AxisToolT,
                                          AxisNames.RightToolT,
                                          candidates,
                                          treatMissingAsSafe,
                                          fallbackTolerance,
                                          useAxisInposTolerance);
            return result;
        }

        public bool IsPositionPlaceZSafety(double fallbackTolerance = 0.01,
                                      bool useAxisInposTolerance = true,
                                      bool treatMissingAsSafe = true)
        {
            string[] candidates = new string[]
            {
                "SafetyPos",
                OutputDieTransferConfig.TeachingPositionName.SafetyZone.ToString()
            };

            bool result = IsAxisSafetyPos(AxisPlaceZ,
                                          AxisNames.RightPlaceZ,
                                          candidates,
                                          treatMissingAsSafe,
                                          fallbackTolerance,
                                          useAxisInposTolerance);
            return result;
        }

        // ---------------- Ready ----------------
        public bool IsPositionToolTReady()
        {
            const string tpName = nameof(OutputDieTransferConfig.TeachingPositionName.Ready);
            bool result = IsAxisInTeaching(AxisToolT,
                                           AxisNames.RightToolT,
                                           tpName,
                                           treatAxisNullAsTrue: true,
                                           useOffset: true);
            return result;
        }

        public bool IsPositionPlaceZReady()
        {
            const string tpName = nameof(OutputDieTransferConfig.TeachingPositionName.Ready);
            bool result = IsAxisInTeaching(AxisPlaceZ,
                                           AxisNames.RightPlaceZ,
                                           tpName,
                                           treatAxisNullAsTrue: true,
                                           useOffset: true);
            return result;
        }

        // ---------------- Place (단일 Teaching) ----------------
        public bool IsPositionToolTPlace()
        {
            const string tpName = nameof(OutputDieTransferConfig.TeachingPositionName.Place);
            bool result = IsAxisInTeaching(AxisToolT,
                                           AxisNames.RightToolT,
                                           tpName,
                                           treatAxisNullAsTrue: true,
                                           useOffset: true);
            return result;
        }

        public bool IsPositionPlaceZPlace()
        {
            const string tpName = nameof(OutputDieTransferConfig.TeachingPositionName.Place);
            bool result = IsAxisInTeaching(AxisPlaceZ,
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

            bool result = IsAxisInTeaching(AxisToolT,
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

            bool result = IsAxisInTeaching(AxisPickZ,
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

            bool result = IsAxisInTeaching(AxisToolT,
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

            if (index >= 1 && index <= 8)
            {
                teachingIdx = index + 1;
            }
            else if (index >= 0 && index < 8)
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
                Log.Write(UnitName, $"[TryGetPickupTeachingName] Invalid index {index}. Range 0~7 or 1~8");
                return false;
            }

            var tp = Config.GetTeachingPosition(tpName);
            if (tp == null)
            {
                Log.Write(UnitName, $"[TryGetPickupTeachingName] Teaching not found: {tpName}");
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

        public int MoveToTeachingPosition(string positionName, double vel = 5, double acc = 10, double dec = 10, double jerk = 50)
        {
            var tp = Config.GetTeachingPosition(positionName);
            if (tp == null) return -1;
            int result = 0;

            //Todo : Z축 확인 후 이동 하도록 수정.
            //foreach (var axisKey in tp.AxisPositions.Keys)
            //{
            //    if (Axes.TryGetValue(axisKey, out var axis))
            //    {
            //        double pos = tp.AxisPositions[axisKey];
            //        int r = axis.MoveAbs(pos, vel, acc, dec, jerk);
            //        if (r != 0) result = r;
            //    }
            //}

            return result;
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
            var eq = Equipment.Instance; var unit = eq?.UnitIO; if (unit == null) return;

            // Vacuum 별칭으로 조회만
            if (!IoAutoBindings.Vacuums.TryGetValue("OutputDieTransferVac1", out _vacuum[0]))
            {
                Log.Write("OutputDieTransfer", "BindIoDomains", "Vacuums not found: OutputDieTransferVac1");
            }

            if (!IoAutoBindings.Vacuums.TryGetValue("OutputDieTransferVac2", out _vacuum[1]))
            {
                Log.Write("OutputDieTransfer", "BindIoDomains", "Vacuums not found: OutputDieTransferVac2");
            }

            if (!IoAutoBindings.Vacuums.TryGetValue("OutputDieTransferVac3", out _vacuum[2]))
            {
                Log.Write("OutputDieTransfer", "BindIoDomains", "Vacuums not found: OutputDieTransferVac3");
            }

            if (!IoAutoBindings.Vacuums.TryGetValue("OutputDieTransferVac4", out _vacuum[3]))
            {
                Log.Write("OutputDieTransfer", "BindIoDomains", "Vacuums not found: OutputDieTransferVac4");
            }

            if (!IoAutoBindings.Vacuums.TryGetValue("OutputDieTransferBlow1", out _blow[0]))
            {
                Log.Write("OutputDieTransfer", "BindIoDomains", "Vacuums not found: OutputDieTransferBlow1");
            }

            if (!IoAutoBindings.Vacuums.TryGetValue("OutputDieTransferBlow2", out _blow[1]))
            {
                Log.Write("OutputDieTransfer", "BindIoDomains", "Vacuums not found: OutputDieTransferBlow2");
            }

            if (!IoAutoBindings.Vacuums.TryGetValue("OutputDieTransferBlow3", out _blow[2]))
            {
                Log.Write("OutputDieTransfer", "BindIoDomains", "Vacuums not found: OutputDieTransferBlow3");
            }

            if (!IoAutoBindings.Vacuums.TryGetValue("OutputDieTransferBlow4", out _blow[3]))
            {
                Log.Write("OutputDieTransfer", "BindIoDomains", "Vacuums not found: OutputDieTransferBlow4");
            }

            if (!IoAutoBindings.Vacuums.TryGetValue("OutputDieTransferVent1", out _vent[0]))
            {
                Log.Write("OutputDieTransfer", "BindIoDomains", "Vacuums not found: OutputDieTransferVent1");
            }

            if (!IoAutoBindings.Vacuums.TryGetValue("OutputDieTransferVent2", out _vent[1]))
            {
                Log.Write("OutputDieTransfer", "BindIoDomains", "Vacuums not found: OutputDieTransferVent2");
            }

            if (!IoAutoBindings.Vacuums.TryGetValue("OutputDieTransferVent3", out _vent[2]))
            {
                Log.Write("OutputDieTransfer", "BindIoDomains", "Vacuums not found: OutputDieTransferVent3");
            }

            if (!IoAutoBindings.Vacuums.TryGetValue("OutputDieTransferVent4", out _vent[3]))
            {
                Log.Write("OutputDieTransfer", "BindIoDomains", "Vacuums not found: OutputDieTransferVent4");
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
            if (Config.IsSimulation || Config.IsDryRun)
            {
                Thread.Sleep(100);
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
            PostAlarm((int)AlarmKeys.eOutputDieTransferVacuum);
            Log.Write(UnitName, expectOn ? "[Vacuum] Arm vacuum ON timeout" : "[Vacuum] Arm vacuum OFF timeout");
            return -1;
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
                ret = -1;
                Log.Write(ex);
            }
            finally
            {
                // Motion 및 Data Ready 상태 완료 후 
                // 정지되도록 코드 구현 필요.
            }

            if (this.RunUnitStatus == UnitStatus.Running)
            {
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
            _pickUpStartEvent.Reset();
            _pickUpdoneEvent.Reset();

            this.RunUnitStatus = UnitStatus.Stopped;
            this.State = ProcessState.Stop;
            base.OnStop();
            return ret;
        }
        protected override int OnRunReady()
        {
            int nRtn = 0;

            MaterialWafer wafer = OutputStage.GetMaterialWafer();
            if(wafer== null || wafer.Presence != Material.MaterialPresence.Exist)
            {   
                return 0;
            }

            if(wafer.ProcessSatate == Material.MaterialProcessSatate.Ready)
            {
                wafer.ProcessSatate = Material.MaterialProcessSatate.Processing;
            }

            if(wafer.ProcessSatate == Material.MaterialProcessSatate.Processing)
            {
                State = ProcessState.Work;
            }
            else if (wafer.ProcessSatate == Material.MaterialProcessSatate.Completed)
            {
                State = ProcessState.Complete;
            }
            return nRtn;
        }
        protected override int OnRunWork()
        {
            int nRtn = 0;
            bool bRet = false;
            try
            {
                if (Rotary != null && Rotary.IsAnyAxisMoving())
                {
                    return 0;
                }

                MaterialDie die = Rotary.GetUnloadSocketMaterial();
                if (die == null || die.Presence != Material.MaterialPresence.Exist)
                {
                    return 0;
                }
                
                //Die를 가지고 있으면 바로 Place를 수행한다.
                var MaterialDie = GetMaterial() as MaterialDie;
                if (MaterialDie == null || MaterialDie.Presence != Material.MaterialPresence.Exist)
                {
                    //started = WaitPickupStartEvent(timeoutMs);
                    //if (!started)
                    //{
                    //    AxisPickZ?.EmgStop();
                    //    AxisToolT?.EmgStop();
                    //    PostAlarm((int)AlarmKeys.eOutputDieTransferError);
                    //    Log.Write(UnitName, "[OnRunWork] WaitPickupStartEvent timeout");
                    //    return -1;
                    //}

                    var sw = System.Diagnostics.Stopwatch.StartNew();
                    int timeoutMs = 60000 * 10;
                    while (true)
                    {
                        if (IsStop)
                        {
                            ReSetPickupStartEvent();
                            _lastPickSucceeded = false;
                            return 0;
                        }

                        bRet = WaitPickupStartEvent(10);
                        if (bRet)
                        {
                            ReSetPickupStartEvent();
                            _lastPickSucceeded = false;
                            break;
                        }

                        if (sw.ElapsedMilliseconds > timeoutMs)
                        {
                            Log.Write(UnitName, $"[OutputDieTransfer] Waiting for Done... Elapsed {sw.ElapsedMilliseconds}ms");
                            break;
                        }
                    }
                    
                    nRtn = ChipPickDown();
                    if (nRtn != 0)
                    {
                        AxisPickZ?.EmgStop();
                        AxisToolT?.EmgStop();
                        Log.Write(UnitName, "[OnRunWork] ChipPickDown failed");
                        return -1;
                    }

                    nRtn = ChipPickUp();
                    if (nRtn != 0)
                    {
                        AxisPickZ?.EmgStop();
                        AxisToolT?.EmgStop();
                        Log.Write(UnitName, "[OnRunWork] ChipPickUp failed");
                        return -1;
                    }

                    if(IsVacuumOK(0))
                    {
                        die.State = DieProcessState.Picked;
                        die.ProcessSatate = Material.MaterialProcessSatate.Processing;

                        Rotary.MoveMaterialToOutputDieTransfer();
                        SetPickupDoneEvent();

                        _lastPickSucceeded = true;
                        State = ProcessState.Complete;
                    }
                    else
                    {
                        die.State = DieProcessState.Rejected;
                        SetPickupDoneEvent();
                        return 0;
                    }
                    
                }

                if (MaterialDie != null 
                    && MaterialDie.Presence == Material.MaterialPresence.Exist)
                {
                    State = ProcessState.Complete;
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
            finally
            {
                
            }
            return 0;
        }
        protected override int OnRunComplete()
        {
            int nRtn = 0;

            if(OutputStage.CanPlaceDie() == false)
            {
                return 0;
            }

            Material wafer = OutputStage.GetMaterialWafer();

            MaterialDie die = GetMaterial() as MaterialDie;
            if (wafer != null 
                && wafer.Presence == Material.MaterialPresence.Exist 
                && die != null 
                && die.Presence == Material.MaterialPresence.Exist)
            {
                //if(OutputStage.IsStageInterLockOK() == false)
                //{
                //    return 0;
                //}
                
                nRtn = MoveOutStage();
                if (nRtn != 0)
                {
                    AxisPlaceZ?.EmgStop();
                    AxisToolT?.EmgStop();
                    Log.Write(UnitName, "[OnRunWork] MoveOutStage failed");
                    return -1;
                }
                //if (IsStop) { return 0; }

                nRtn = RotateToolTForPlace();
                if (nRtn != 0)
                {
                    AxisPlaceZ?.EmgStop();
                    AxisToolT?.EmgStop();
                    Log.Write(UnitName, "[OnRunWork] RotateToolTForPlace failed");
                    return -1;
                }
                //if (IsStop) { return 0; }

                nRtn = ReleaseVacuumAndPlaceUp();
                if (nRtn != 0)
                {
                    AxisPlaceZ?.EmgStop();
                    Log.Write(UnitName, "[OnRunWork] ReleaseVacuumAndPlaceUp failed");
                    return -1;
                }
                //if (IsStop) { return 0; }

                die.State = DieProcessState.Placed;
                die.ProcessSatate = Material.MaterialProcessSatate.Completed;
                
                 OutputStage.PlaceDie(die);

                SetMaterial(new MaterialDie() { Presence = Material.MaterialPresence.NotExist });

               
            }
            State = ProcessState.None;
            return 0;
        }
        #endregion

        #region Sequence 등록

        protected override void OnMakeSequence()
        {
            base.OnMakeSequence();
            this.SequencePlayers.Add(MoveOutStage);
            this.SequencePlayers.Add(ChipPickDown);
            this.SequencePlayers.Add(ChipPickUp);
            this.SequencePlayers.Add(RotateToolTForPlace);
            this.SequencePlayers.Add(ReleaseVacuumAndPlaceUp);

        }

        #endregion

        #region Seq 단위 동작 함수
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

            //if (!OutputStage.IsStageInterLockOK())
            //{
            //    Log.Write(UnitName, "[MoveOutStage] Stage Interlock not OK.");
            //    return -1;
            //}
            
            if (OutputStage.IsPlateUp())
            {
                if (Config.IsSimulation == false)
                {
                    PostAlarm((int)AlarmKeys.eOutputStageAxesMoving);
                    return -1;
                }
            }

            if (!OutputStage.TryReserveNextEmptyBin(out double binX, out double binY, out var slot))
            {
                //Log.Write(UnitName, "[MoveOutStage] No empty bin slot.");
                return 0; // 더 놓을 자리가 없으면 정상 종료로 간주
            }

            nRet = OutputStage.MoveToBinPosition(binX, binY, bFineSpeed);
            if (nRet != 0)
            {
                Log.Write(UnitName, "[MoveOutStage] MoveToBinPosition failed");
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

            int nIndex = 0;
            nIndex = GetUnloaderIndexNo();
            int nArmindex = 0;
            nArmindex = GetPlaceArmIndex();

            // 1) ToolT 위치 확인.
            if(IsPositionPickUpToolT() == false)
            {
                nRet = MovePositionPickUpToolT_Index(bFineSpeed);
                if (nRet != 0)
                {
                    Log.Write(UnitName, "[ChipPickDown] MovePositionPickUpToolT_Index failed");
                    return -1;
                }
            }

            SetVacuum(nArmindex, true);

            nRet = MovePositionPickUpPickZ_Index(bFineSpeed);
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

            Rotary.SetVacuum(GetUnloaderIndexNo(), false);
            Thread.Sleep(1);
            //Rotary.SetVent(GetUnloaderIndexNo(), true);
            //Thread.Sleep(50);
            //Rotary.SetVent(GetUnloaderIndexNo(), false);
            //Thread.Sleep(1);
            Rotary.SetBlow(GetUnloaderIndexNo(), true);

            //대기
            Thread.Sleep(100);

            return nRet;
        }

        public int ChipPickUp(bool bFineSpeed = false)
        {
            int nRet = 0;
            if (RunMode == UnitRunMode.Manual)
            {
                this.CurrentFunc = ChipPickUp;

            }

            int nIndex = 0;
            int nArmindex = 0;
            nArmindex = GetPlaceArmIndex();
            nIndex = GetUnloaderIndexNo();

            Rotary.SetVacuum(nIndex, false);
           
            Thread.Sleep(1); // 약간의 딜레이
            //if(!Rotary.SetVent(nIndex, true))
            //{
            //    if(!Config.IsSimulation)
            //    {
            //        PostAlarm((int)AlarmKeys.eOutputDieTransferVent);
            //        Log.Write(UnitName, "[DieTrVacuumOff] SetVent failed");
            //        return -1;
            //    }   
            //}

            if(!Rotary.SetBlow(nIndex, true))
            {
                if (!Config.IsSimulation)
                {
                    PostAlarm((int)AlarmKeys.eOutputDieTransferBlow);
                    Log.Write(UnitName, "[DieTrVacuumOff] SetBlow failed");
                    return -1;
                }
            }

            nRet = MovePositionSafetyZ(bFineSpeed);
            if (nRet != 0)
            {
                Log.Write(UnitName, "[ChipPickUp] MovePositionSafetyZ failed");
                return -1;
            }
            while(IsPositionPickZSafety() == false)
            {
                Thread.Sleep(1);
            }

            if (Rotary.SetVent(nIndex, false) == false)
            {
                if (!Config.IsSimulation)
                {
                    PostAlarm((int)AlarmKeys.eOutputDieTransferVent);
                    Log.Write(UnitName, "[DieTrVacuumOff] SetVent failed");
                    return -1;
                } 
            }

            if (Rotary.SetBlow(nIndex, false) == false)
            {
                if (!Config.IsSimulation)
                {
                    PostAlarm((int)AlarmKeys.eOutputDieTransferBlow);
                    Log.Write(UnitName, "[DieTrVacuumOff] SetBlow failed");
                    return -1;
                }   
            }

            //Rotary Vacuum Off 확인.
            nRet = Rotary.WaitVacuumStateOrAlarm(nArmindex, false);
            if (nRet != 0)
            {
                Log.Write(UnitName, "[RotaryVacuumOff] Vacuum Timeout");
                return -1;
            }
            //OutputDieTransferVacuumOn 확인.
            nRet = WaitVacuumStateOrAlarm(nArmindex, true);
            if (nRet != 0)
            {
                Log.Write(UnitName, "[OutputDieTrVacuumOn] Vacuum Timeout");
                return -1;
            }

            return nRet;
        }

        public int RotateToolTForPlace(bool bFineSpeed = false)
        {
            if (AxisToolT == null)
                return -1;

            int nRet = 0;
            if (RunMode == UnitRunMode.Manual)
            {
                this.CurrentFunc = RotateToolTForPlace;

            }

            nRet = MovePositionPlace(bFineSpeed);
            if (nRet != 0)
            {
                Log.Write(UnitName, "[RotateToolTForPlace] MovePositionPlace 실패");
                return -1;
            }

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
                int armIndex = GetPlaceArmIndex();
                if (armIndex < 0 || armIndex > 3) 
                    return -1;

                // Release
                if(!SetVacuum(armIndex, false))
                {
                    if(!Config.IsSimulation && !Config.IsDryRun)
                    {
                        PostAlarm((int)AlarmKeys.eOutputDieTransferVacuum);
                        Log.Write(UnitName, "[ReleaseVacuumAndPlaceUp] SetVacuum failed");
                        return -1;
                    }
                }
                SetVent(armIndex, true);
                Thread.Sleep(5);
                SetVent(armIndex, false);

                SetBlow(armIndex, true);
                Thread.Sleep(50);

                nRet = MovePositionSafetyZ(bFindSpeed);
                if (nRet != 0)
                {
                    Log.Write(UnitName, "[ReleaseVacuumAndPlaceUp] MovePositionSafetyZ 실패");
                    return -1;
                }
                SetBlow(armIndex, false);

                nRet = MovePositionPickUpToolT_Index(bFindSpeed);
                if (nRet != 0)
                {
                    Log.Write(UnitName, "[ReleaseVacuumAndPlaceUp] MovePositionPickUpToolT_Index failed");
                    return -1;
                }
            }
            catch (Exception ex)
            {
                AxisToolT?.EmgStop();
                AxisPickZ?.EmgStop();
                AxisPlaceZ?.EmgStop();
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

            return nRet;
        }
        #endregion
    }
}