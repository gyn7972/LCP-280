using QMC.Common;
using QMC.Common.Component;
using QMC.Common.IOUtil;
using QMC.Common.Motion;
using QMC.Common.Motions;
using QMC.Common.Unit;
using QMC.LCP_280.Process.Component; // TeachingPosition
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;

namespace QMC.LCP_280.Process.Unit
{
    public class InputFeeder : BaseUnit<InputFeederConfig>
    {
        enum AlarmKeys
        {
            Alarm_WaferLoadingFailed = 2000,
            Alarm_BarcodeReadingFailed = 2001,
            Alarm_StageLoadingFailed = 2002,
            Alarm_StageUnloadingFailed = 2003,
            Alarm_WaferUnloadingFailed = 2004,
            Alarm_InputStageInterlockFailed = 2010,
            Alarm_GripperClampFailed = 2020,
            Alarm_FeederClampUp = 2021,
            Alarm_IsWaferReadyForLoading = 2022,
            Alarm_WaferLoadingPosition = 2023,
            Alarm_InputCassetteLifteInterlockFailed = 2024,
            Alarm_InputFeederNoPosition = 2025,
            Alarm_InputFeederInterlockFailed = 2026,
            Alarm_GripperUnClampFailed = 2027,
        }
        #region InitAlarm
        protected override void InitAlarm()
        {
            base.InitAlarm();
            AlarmRegister((int)AlarmKeys.Alarm_WaferLoadingFailed,
                "Wafer Loading Failed",
                "웨이퍼 로딩에 실패 하였습니다.",
                "Error");
            AlarmRegister((int)AlarmKeys.Alarm_BarcodeReadingFailed,
                "Barcode Reading Failed",
                "바코드 읽기에 실패 하였습니다.\n바코드 상태를 확인 하여 주십시요",
                "Error");
            AlarmRegister((int)AlarmKeys.Alarm_StageLoadingFailed,
                "Stage Loading Failed",
                "스테이지 로딩에 실패 하였습니다.",
                "Error");
            AlarmRegister((int)AlarmKeys.Alarm_StageUnloadingFailed,
                "Stage Unloading Failed",
                "스테이지 언로딩에 실패 하였습니다.",
                "Error");
            AlarmRegister((int)AlarmKeys.Alarm_WaferUnloadingFailed,
                "Wafer Unloading Failed",
                "웨이퍼 언로딩에 실패 하였습니다.",
                "Error");
            AlarmRegister((int)AlarmKeys.Alarm_InputStageInterlockFailed,
                "Input Stage Interlock Failed",
                "웨이퍼 로딩을 위한 인터락이 맞지 않습니다.\n장비 상태를 확인 하여 주십시요.",
                "Error");
            AlarmRegister((int)AlarmKeys.Alarm_GripperClampFailed,
                "Gripper Clamp Failed",
                "그리퍼 클램프에 실패 하였습니다.\n장비 상태를 확인 하여 주십시요.",
                "Error");
            AlarmRegister((int)AlarmKeys.Alarm_FeederClampUp,
                "Feeder Clamp Up Failed",
                "피더 클램프 업 상태가 아닙니다.\n장비 상태를 확인 하여 주십시요.",
                "Error");
            AlarmRegister((int)AlarmKeys.Alarm_IsWaferReadyForLoading,
                "IsWaferReadyForLoading Fail",
                "Cassette Ready For Loading Signal Fail.\n장비 상태를 확인 하여 주십시요.",
                "Error");
            AlarmRegister((int)AlarmKeys.Alarm_WaferLoadingPosition,
                "WaferLoadingPosition",
                "Wafer LoadingPosition Fail\n장비 상태를 확인 하여 주십시요.",
                "Error");
            AlarmRegister((int)AlarmKeys.Alarm_InputCassetteLifteInterlockFailed,
                "Input Cassette Lifter Interlock Failed",
                "Input Cassette Lifter Interlock Failed.\n장비 상태를 확인 하여 주십시요.",
                "Error");
            AlarmRegister((int)AlarmKeys.Alarm_InputFeederNoPosition,
                "Input Feeder No Position",
                "Input Feeder No Position.\n장비 상태를 확인 하여 주십시요.",
                "Error");
            AlarmRegister((int)AlarmKeys.Alarm_InputFeederInterlockFailed,
                "Input Feeder Interlock Failed",
                "Input Feeder Interlock Failed.\n장비 상태를 확인 하여 주십시요.",
                "Error");
            AlarmRegister((int)AlarmKeys.Alarm_GripperUnClampFailed,
                "Gripper UnClamp Failed",
                "Gripper UnClamp Failed.\n장비 상태를 확인 하여 주십시요.",
                "Error");
        }
        #endregion

        #region Unit
        public InputCassetteLifter InputCassetteLifter { get; set; }
        public InputStage InputStage { get; set; }
        #endregion

        #region Axes
        private MotionAxis _feederY;
        public MotionAxis AxisInputFeederY => _feederY;
        #endregion

        #region IO Domain Members
        private Cylinder _feederLift; // Up/Down
        private Cylinder _cylClamp;   // Clamp/Unclamp
        #endregion

        #region Constructor / Initialization
        public InputFeeder(InputFeederConfig config = null)
            : base(new InputFeederConfig())
        {
            AddComponents();
        }
        public override void AddComponents()
        {
            Config.LoadAndBindAxes(Equipment.Instance.AxisManager);
            Config.InitializeDefaultTeachingPositions();

            BindAxes();
            BindIoDomains();

            Config.IsSimulation = Config.IsSimulation;
            if (Config.IsSimulation)
            {
                _feederY.Config.IsSimulation = true;
                Log.Write("InputFeeder", "Simulation Mode");
            }
        }
        protected override void OnBindUnit()
        {
            base.OnBindUnit();
            InputCassetteLifter = Equipment.Instance.GetUnit("InputCassetteLifter") as InputCassetteLifter;
            InputStage = Equipment.Instance.GetUnit("InputStage") as InputStage;
        }
        #endregion

        #region Axis Binding
        private void BindAxes()
        {
            var mgr = Equipment.Instance?.AxisManager;
            if (mgr == null)
            {
                Log.Write("InputFeeder", "[BindAxes] AxisManager null");
                return;
            }

            const string unitName = "Unit"; // Equipment
            BindAxis(mgr, unitName, AxisNames.WaferFeederY, ref _feederY);
        }
        #endregion


        // Move with Interlock Check
        public int MovePositionReady(bool isFine = false)
        {
            bool bRet = false;
            bRet = InPosTeaching(TeachingPositions[(int)InputFeederConfig.TeachingPositionName.Ready]);
            if (bRet)
            {
                return 0;
            }

            Task<int> task = MovePositionAsyncReady(isFine);
            while (IsEndTask(task) == false)
            {
                if (RunMode == UnitRunMode.Auto)
                {
                    if (IsInterlockOKWaferLoading() == false)
                    {
                        AxisInputFeederY.EmgStop();
                        PostAlarm((int)AlarmKeys.Alarm_WaferLoadingFailed);
                        return -1;
                    }
                    CheckMoveInterLockReady();
                }
                else if (RunMode == UnitRunMode.Manual)
                {
                    CheckMoveInterLockReady();
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
            return MoveTeachingPositionOnce((int)InputFeederConfig.TeachingPositionName.Ready, isFine);
        }
        private int CheckMoveInterLockReady()
        {
            int nRet = 0;

            if(Config.IsSimulation == false && Config.IsDryRun == false)
            {
                if (IsRingPresent() == true)
                {
                    PostAlarm((int)AlarmKeys.Alarm_InputFeederInterlockFailed);
                    Log.Write(this, "CheckMoveInterLockReady Fail - IsRingPresent()");
                    return -1;
                }
            }
            

            if (!IsUnClamped())
            {
                AxisInputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_GripperClampFailed);
                Log.Write(UnitName, "IsMoveInterLockReady", "Feeder Clamp 닫혀 있음. (Wafer 잡고 있는지 확인 필요)");
                nRet = -1;
                return nRet;
            }

            if (InputStage.IsAnyAxisMoving())
            {
                AxisInputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_InputStageInterlockFailed);
                Log.Write(UnitName, "IsMoveInterLockReady", "InputStage 축 이동중.");
                nRet = -1;
                return nRet;
            }

            // Cassette or InputStage 위치 및 Signal 확인 후 진행. 
            if (!InputCassetteLifter.IsWaferReadyForLoading() || !InputStage.IsWaferLoadingPosition())
            {
                if (!IsFeederUp())
                {
                    AxisInputFeederY.EmgStop();
                    PostAlarm((int)AlarmKeys.Alarm_FeederClampUp);
                    Log.Write(UnitName, "IsMoveInterLockReady", "Feeder Up Fail.");
                    nRet = -1;
                    return nRet;
                }
            }

            return nRet;
        }

        public int MovePositionStage(bool isFine = false)
        {
            Task<int> task = MovePositionAsyncStage(isFine);
            while (IsEndTask(task) == false)
            {
                IsMoveInterLockStage();
                Thread.Sleep(1);
            }
            return task.Result;
        }
        public Task<int> MovePositionAsyncStage(bool isFine = false)
        {
            return Task.Run(() =>
            {
                OnMovePositionStage(isFine);
                return 0;
            });
        }
        private int OnMovePositionStage(bool isFine = false)
        {
            return MoveTeachingPositionOnce((int)InputFeederConfig.TeachingPositionName.Stage, isFine);
        }
        private int IsMoveInterLockStage()
        {
            int nRet = 0;
            // Check Interlock.!!! 구문 넣을것.!!!
            //if (IsFeederUp())
            //{
            //    FeederY.EmgStop();
            //    PostAlarm((int)AlarmKeys.Alarm_FeederClampUp);
            //    nRet = -1;
            //    return nRet;
            //}

            if (InputStage.IsAnyAxisMoving())
            {
                AxisInputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_InputStageInterlockFailed);
                nRet = -1;
                return nRet;
            }

            return nRet;
        }


        public int MovePositionBarcode(bool isFine = false)
        {
            Task<int> task = MovePositionAsyncBarcode(isFine);
            while (IsEndTask(task) == false)
            {
                IsMoveInterLockBarcode();
                Thread.Sleep(1);
            }
            return task.Result;
        }
        public Task<int> MovePositionAsyncBarcode(bool isFine = false)
        {
            return Task.Run(() =>
            {
                OnMovePositionBarcode(isFine);
                return 0;
            });
        }
        private int OnMovePositionBarcode(bool isFine = false)
        {
            return MoveTeachingPositionOnce((int)InputFeederConfig.TeachingPositionName.Barcode, isFine);
        }
        private int IsMoveInterLockBarcode()
        {
            int nRet = 0;
            // Check Interlock.!!! 구문 넣을것.!!!
            //if (IsFeederUp())
            //{
            //    FeederY.EmgStop();
            //    PostAlarm((int)AlarmKeys.Alarm_FeederClampUp);
            //    nRet = -1;
            //    return nRet;
            //}

            if (InputStage.IsAnyAxisMoving())
            {
                AxisInputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_InputStageInterlockFailed);
                nRet = -1;
                return nRet;
            }

            if (InputCassetteLifter.IsAnyAxisMoving())
            {
                AxisInputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_InputStageInterlockFailed);
                nRet = -1;
                return nRet;
            }

            return nRet;
        }

        public int MovePositionCassette(bool isFine = false)
        {
            Task<int> task = MovePositionAsyncCassette(isFine);
            while (IsEndTask(task) == false)
            {
                if (IsInterlockOKMoveToCassette() == false)
                {
                    AxisInputFeederY.EmgStop();
                    PostAlarm((int)AlarmKeys.Alarm_WaferLoadingFailed);
                    return -1;
                }

                IsMoveInterLockCassette();
                Thread.Sleep(1);
            }
            return task.Result;
        }
        public Task<int> MovePositionAsyncCassette(bool isFine = false)
        {
            return Task.Run(() =>
            {
                OnMovePositionCassette(isFine);
                return 0;
            });
        }
        private int OnMovePositionCassette(bool isFine = false)
        {
            return MoveTeachingPositionOnce((int)InputFeederConfig.TeachingPositionName.Cassette, isFine);
        }
        private bool IsMoveInterLockCassette()
        {
            bool bRet = false;

            if (InputStage.IsAnyAxisMoving())
            {
                bRet = false;
                return bRet;
            }

            if (InputCassetteLifter.IsAnyAxisMoving())
            {
                bRet = false;
                return bRet;
            }


            if (InputStage.IsWaferLoadingPosition() == false)
            {
                bRet = false;
                return bRet;
            }

            if (InputStage.IsWaferUnloadingPosition() == false)
            {
                bRet = false;
                return bRet;
            }

            bRet = true;
            return bRet;
        }

        public bool IsFeederYSafetyPosition()
        {
            bool bRtn = false;
            if (AxisInputFeederY == null)
                return bRtn;

            var cfg = Config;
            if (cfg == null)
                return bRtn;

            bRtn = IsPositionReady();
            return bRtn;
        }
        public bool IsFeederZSafetyPosition()
        {
            bool bRtn = false;

            if (_feederLift == null)
                return bRtn;

            if (this.Config.IsSimulation)
            {
                return true;
            }
            if (IsFeederUp())
                return true;

            if (IsFeederDown())
                return false;

            // 전이 상태(Up/Down 모두 OFF) → 안전 아님으로 판단
            return bRtn;
        }

        public bool IsPositionReady()
        {
            var tp = TeachingPositions[(int)InputFeederConfig.TeachingPositionName.Ready];
            if (tp == null)
                return false;
            return InPosTeaching(tp);
        }

        public bool IsPositionBarcode()
        {
            var tp = TeachingPositions[(int)InputFeederConfig.TeachingPositionName.Barcode];
            if (tp == null)
                return false;
            return InPosTeaching(tp);
        }
        public bool IsPositionStage()
        {
            var tp = TeachingPositions[(int)InputFeederConfig.TeachingPositionName.Stage];
            if (tp == null)
                return false;
            return InPosTeaching(tp);
        }
        public bool IsPositionCassette()
        {
            var tp = TeachingPositions[(int)InputFeederConfig.TeachingPositionName.Cassette];
            if (tp == null)
                return false;
            return InPosTeaching(tp);
        }

        #region Teaching Helpers
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
        #endregion

        #region Low-Level IO (Read/Write by Name)
        #endregion

        #region IO Domain Mapping
        private void BindIoDomains()
        {
            var eq = Equipment.Instance; var unit = eq?.UnitIO; if (unit == null) return;

            if (!IoAutoBindings.Cylinders.TryGetValue("InFeederLift", out _feederLift))
            {
                Log.Write("InputFeeder", "BindIoDomains", "Cylinder not found: InFeederLift");
            }

            if (!IoAutoBindings.Cylinders.TryGetValue("InFeederClamp", out _cylClamp))
            {
                Log.Write("InputFeeder", "BindIoDomains", "Cylinder not found: InFeederClamp");
            }
            BindCylinder(_cylClamp);
        }
        #endregion

        public override bool IsInterlockOK(BaseComponent baseComponent, BaseComponent.InterlockEventArgs e)
        {
            bool bRet = base.IsInterlockOK(baseComponent, e);
            if(baseComponent == this.AxisInputFeederY)
            {
                if(this.IsFeederDown())
                {
                    if (this.InputStage.IsWaferLoadingPosition() == false)
                    {
                        bRet = false;
                    }
                    else
                    {
                        //if(this.InputStage.IsRingPresent())
                        {
                            bRet = IsInterlockOKWithStage(e);
                            if (bRet == false)
                            {
                                PostAlarm((int)AlarmKeys.Alarm_InputStageInterlockFailed);
                                return bRet;
                            }
                        }
                    }
                }
                
            }
            return bRet;
        }

        private bool IsInterlockOKWithStage(BaseComponent.InterlockEventArgs e)
        {
            if (this.InputStage.IsPlateUp() || this.InputStage.IsClampLiftUp())
            {
                double dCurrentY = this.AxisInputFeederY.GetPosition();
                double dStageY = this.GetTP(InputFeederConfig.TeachingPositionName.Stage.ToString(), this.AxisInputFeederY.Name);
                if (dCurrentY < dStageY + this.AxisInputFeederY.Config.InposTolerance
                    || e.dTargetPosition < dStageY + this.AxisInputFeederY.Config.InposTolerance)
                {
                    return false;
                }

            }

            return true;
        }
        #region Status Helpers
        public bool SetLift(bool bUpDn)
        {
            if (_feederLift == null)
                return false;
            if (bUpDn)
                return _feederLift.Extend();
            else
                return _feederLift.Retract();
        }
        public bool SetClamp(bool bUpDn)
        {
            if (_cylClamp == null)
                return false;
            if (bUpDn)
                return _cylClamp.Extend();
            else
                return _cylClamp.Retract();
        }
        public bool IsFeederUp()
        {
            if (Config.IsSimulation)
            {
                return true;
            }
            return this.ReadInput(InputFeederConfig.IO.FEEDER_UP);
        }
        public bool IsFeederDown()
        {
            if (Config.IsSimulation)
            {
                return true;
            }
            return this.ReadInput(InputFeederConfig.IO.FEEDER_DOWN);
        }
        public bool IsClamped()
        {
            bool bRtn = false;
            if (Config.IsSimulation)
            {
                bRtn = true;
                return bRtn;
            }
            bRtn = !this.ReadInput(InputFeederConfig.IO.FEEDER_UNCLAMP);
            return bRtn;
        }
        public bool IsUnClamped()
        {
            bool bRtn = false;
            if (Config.IsSimulation)
            {
                return true;
            }
            bRtn = this.ReadInput(InputFeederConfig.IO.FEEDER_UNCLAMP);
            return bRtn;
        }
        public bool IsRingPresent()
        {
            if (Config.IsSimulation || Config.IsDryRun)
            {
                return true;
            }
            return this.ReadInput(OutputFeederConfig.IO.FEEDER_RING_CHECK);
        }
        public bool IsOverload()
        {
            if (Config.IsSimulation || Config.IsDryRun)
            {
                return true;
            }
            return this.ReadInput(OutputFeederConfig.IO.FEEDER_OVERLOAD);
        }

        // === Cylinder 완료 대기 Helper ===
        // Clamp: expectClamp=true(Clamp 완료 기대), false(Unclamp 완료 기대)
        private int WaitClampStateOrAlarm(bool expectClamp, int timeoutMs = 1500, int pollMs = 2)
        {
            if (Config.IsSimulation || Config.IsDryRun)
                return 0;

            var sw = System.Diagnostics.Stopwatch.StartNew();
            while (sw.ElapsedMilliseconds <= timeoutMs)
            {
                bool ok = expectClamp ? IsClamped() : IsUnClamped();
                if (ok)
                    return 0;

                Thread.Sleep(pollMs);
            }

            int alarm = expectClamp
                ? (int)AlarmKeys.Alarm_GripperClampFailed
                : (int)AlarmKeys.Alarm_GripperUnClampFailed;

            PostAlarm(alarm);
            Log.Write(UnitName, expectClamp ? "[Clamp] Gripper CLAMP timeout" : "[Clamp] Gripper UNCLAMP timeout");
            return -1;
        }

        // Lift: expectUp=true(UP 완료 기대), false(DOWN 완료 기대)
        private int WaitLiftStateOrAlarm(bool expectUp, int timeoutMs = 1500, int pollMs = 2)
        {
            if (Config.IsSimulation || Config.IsDryRun)
                return 0;

            var sw = System.Diagnostics.Stopwatch.StartNew();
            while (sw.ElapsedMilliseconds <= timeoutMs)
            {
                bool ok = expectUp ? IsFeederUp() : IsFeederDown();
                if (ok)
                    return 0;

                Thread.Sleep(pollMs);
            }

            // 별도 Down 실패 알람 키가 없어 기존 키 사용
            int alarm = expectUp
                ? (int)AlarmKeys.Alarm_FeederClampUp
                : (int)AlarmKeys.Alarm_WaferLoadingFailed;

            PostAlarm(alarm);
            Log.Write(UnitName, expectUp ? "[Lift] Feeder UP timeout" : "[Lift] Feeder DOWN timeout");
            return -1;
        }

        #endregion

        #region === Direct Valve Control ===
        public bool IsFeederUpValveOn() => this.IsOutputOn(InputFeederConfig.IO.FEEDER_UP_VALVE);
        public bool IsFeederDownValveOn() => this.IsOutputOn(InputFeederConfig.IO.FEEDER_DOWN_VALVE);
        public bool IsFeederClampValveOn() => this.IsOutputOn(InputFeederConfig.IO.FEEDER_CLAMP_VALVE);
        public bool IsFeederUnclampValveOn() => this.IsOutputOn(InputFeederConfig.IO.FEEDER_UNCLAMP_VALVE);
        #endregion


        #region Status Signals
        public bool IsWaferLoadDone { get; private set; }
        #endregion

        /// ////////////////////////////////////////////////////////////////
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

            return ret;
        }
        protected override int OnRunReady()
        {
            int ret = 0;

            this.State = ProcessState.Work;
            return ret;
        }
        protected override int OnRunWork()
        {
            int nRet = 0;
            
            MaterialWafer wafer = this.InputStage.GetMaterialWafer();
            // Stage 요청 인지 시 Busy로 표시(선택)
            if (this.InputStage.IsWorking() == true)
            {
                if (wafer != null)
                {
                    if (wafer.ProcessSatate == Material.MaterialProcessSatate.Ready)
                    {
                        nRet = PreparetoInputStage();
                    }
                }
                return nRet;
            }
            if (IsStop) { return 0; }

            // 0) Stage에 제품이 있으면 "언로딩 먼저"
            try
            {
                NeedUnloadFirst = InputStage.IsWorking();
                if (NeedUnloadFirst)
                {

                }
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                NeedUnloadFirst = false;
            }

            if (NeedUnloadFirst)
            {
                if (this.IsStop)
                {
                    Log.Write(this, "InputFeeder Stop");
                    return 0;
                }

                // 8) Feeder -> Stage: WaferUnloadingBeforeStage
                nRet = WaferUnloading(wafer);
                if (nRet != 0)
                {
                    AxisInputFeederY.EmgStop();
                    PostAlarm((int)AlarmKeys.Alarm_WaferUnloadingFailed);
                    this.State = ProcessState.Error;
                }
            }
            if (IsStop) { return 0; }

            // 1) Feeder -> Cassette: Scan
            if (this.InputCassetteLifter.IsScanCompleted() == false)
            {
                nRet = this.InputCassetteLifter.ScanWafer();
                if (nRet != 0)
                {
                    AxisInputFeederY.EmgStop();
                    PostAlarm((int)AlarmKeys.Alarm_WaferLoadingFailed);
                    this.State = ProcessState.Error;
                    return nRet;
                }
            }
            if (IsStop) { return 0; }

            if (this.InputCassetteLifter.IsHaveMoreProcessWafer())
            {
                // 2) Feeder -> Cassette: MoveToNextSlot
                nRet = this.InputCassetteLifter.MoveToNextSlot();
                if (nRet != 0)
                {
                    AxisInputFeederY.EmgStop();
                    PostAlarm((int)AlarmKeys.Alarm_WaferLoadingFailed);
                    this.State = ProcessState.Error;
                    return nRet;
                }
                if (IsStop) { return 0; }

                // 3) Feeder -> Stage: WaferLoadingBeforeStage
                nRet = PrepareLoadingWafer();
                if (nRet != 0)
                {
                    AxisInputFeederY.EmgStop();
                    PostAlarm((int)AlarmKeys.Alarm_InputStageInterlockFailed);
                    this.State = ProcessState.Error;
                    return nRet;
                }
                if (IsStop) { return 0; }
                
                // 4) Feeder 내부 로딩 Cascette에서 Wafer Pick
                nRet = WaferLoading(); // 여기서 Barcode Reading 포함
                if (nRet != 0)
                {
                    AxisInputFeederY.EmgStop();
                    PostAlarm((int)AlarmKeys.Alarm_WaferLoadingFailed);
                    this.State = ProcessState.Error;
                    return nRet;
                }
                if (IsStop) { return 0; }

                // 4) Feeder 내부 로딩 Stage에 Wafer Load
                nRet = StageLoading();
                if (nRet != 0)
                {
                    AxisInputFeederY.EmgStop();
                    PostAlarm((int)AlarmKeys.Alarm_StageLoadingFailed);
                    this.State = ProcessState.Error;
                    return nRet;
                }

                this.MoveMaterial(new MaterialWafer(), InputStage);
                if (IsStop) { return 0; }

                nRet = MoveToReady();
                if (nRet != 0)
                {
                    AxisInputFeederY.EmgStop();
                    PostAlarm((int)AlarmKeys.Alarm_WaferLoadingFailed);
                    this.State = ProcessState.Error;
                    return nRet;
                }
                if (IsStop) { return 0; }

                // 5) Feeder -> Stage: WaferLoadingAfterStage
                nRet = InputStage.LoadingWaferComplete();
                if (nRet != 0)
                {
                    AxisInputFeederY.EmgStop();
                    PostAlarm((int)AlarmKeys.Alarm_StageLoadingFailed);
                    this.State = ProcessState.Error;
                    return nRet;
                }
                if (IsStop) { return 0; }

                nRet = PreparetoInputStage();
                if (nRet != 0)
                {
                    AxisInputFeederY.EmgStop();
                    PostAlarm((int)AlarmKeys.Alarm_StageLoadingFailed);
                    this.State = ProcessState.Error;
                    Log.Write(this, "OnRunWork Fail - PreparetoInputStage");
                    return nRet;
                }

                this.State = ProcessState.Complete;
            }
            else
            {
                if(IsPositionReady() == false)
                {
                    nRet = MoveToReady();
                    if (nRet != 0)
                    {
                        AxisInputFeederY.EmgStop();
                        PostAlarm((int)AlarmKeys.Alarm_WaferLoadingFailed);
                        this.State = ProcessState.Error;
                        return nRet;
                    }

                }
            }
            return nRet;
        }
        protected override int OnRunComplete()
        {
            int ret = 0;
            this.State = ProcessState.Ready;
            return ret;
        }
        protected override int OnStart()
        {
            this.InputCassetteLifter.Start();
            this.InputStage.Start();

            return base.OnStart();
        }
        public override int OnStop()
        {
            int ret = 0;
            this.RunUnitStatus = UnitStatus.Stopped;
            this.State = ProcessState.Stop;

            InputStage.Stop();
            InputCassetteLifter.Stop();

            base.OnStop();
            return ret;
        }

        protected override void OnMakeSequence()
        {
            base.OnMakeSequence();
            this.SequencePlayers.Add(PrepareLoadingWafer);
            this.SequencePlayers.Add(WaferLoading);
            this.SequencePlayers.Add(StageLoading);
            this.SequencePlayers.Add(MoveToReady);
            this.SequencePlayers.Add(WaferUnloading);
        }

        #region Sequence Auto

        bool NeedUnloadFirst { get; set; } = false;
        private int PreparetoInputStage()
        {
            int nRet = 0;
            // 6) 정렬/매핑
            nRet = InputStage.AlignT();
            if (nRet != 0)
            {
                AxisInputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_StageLoadingFailed);
                this.State = ProcessState.Error;
                Log.Write(this, "PreparetoInputStage Fail - AlignT");
                return nRet;
            }
            if (IsStop) { return 0; }

            nRet = InputStage.AlignXY();
            if (nRet != 0)
            {
                AxisInputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_StageLoadingFailed);
                this.State = ProcessState.Error;
                Log.Write(this, "PreparetoInputStage Fail - AlignXY");
                return nRet;
            }
            if (IsStop) { return 0; }

            nRet = InputStage.PerformChipMapping();
            if (nRet != 0)
            {
                AxisInputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_StageLoadingFailed);
                this.State = ProcessState.Error;
                Log.Write(this, "PreparetoInputStage Fail - PerformChipMapping");
                return nRet;
            }

            return nRet;
        }

        public int PrepareLoadingWafer(bool isFine = false)
        {
            int nRet = 0;

            if (RunMode == UnitRunMode.Manual)
            {
                CurrentFunc = PrepareLoadingWafer;
                InputStage.RunUnitStatus = UnitStatus.Running;
            }

            nRet = InputStage.LoadingWaferPrepare();
            if(nRet != 0)
            {
                if (RunMode == UnitRunMode.Manual)
                    InputStage.RunUnitStatus = UnitStatus.Stopped;

                Log.Write(this, "PrepareLoadingWafer Fail - InputStage.LoadingWaferPrepare()");
                return -1;
            }

            if (RunMode == UnitRunMode.Manual)
                InputStage.RunUnitStatus = UnitStatus.Stopped;

            return nRet;
        }
        public int WaferLoading(bool isFine = false)
        {
            int nRet = 0;

            if (RunMode == UnitRunMode.Manual)
            {
                CurrentFunc = WaferLoading;
            }

            Log.Write(this, "WaferLoading Start");
            if (IsMoveInterLockCassette() == false)
            {
                Log.Write(this, "WaferLoading Fail - IsMoveInterLockCassette");
                return -1;
            }

            if (NeedUnloadFirst)
            {
                if (IsPositionBarcode() == false)
                {
                    Log.Write(this, "WaferLoading - MovePositionBarcode First");
                    return -1;
                }
            }
            else
            {
                nRet = MoveToReady(isFine);
                if (nRet != 0)
                {
                    Log.Write(this, "WaferLoading Fail - MoveToReay");
                    return nRet;
                }
                if (IsStop) { return 0; }
            }

            nRet = UnClampGripper();
            if (nRet != 0)
            {
                Log.Write(this, "WaferLoading Fail - UnClampGripper");
                return nRet;
            }
            if (IsStop) { return 0; }

            nRet = DownFeeder();
            if (nRet != 0)
            {
                Log.Write(this, "WaferLoading Fail - DownFeeder");
                return nRet;
            }
            if (IsStop) { return 0; }

            nRet = MoveToCassette(isFine);
            if (nRet != 0)
            {
                Log.Write(this, "WaferLoading Fail - MoveToCassette");
                return nRet;
            }
            if (IsStop) { return 0; }

            nRet = BarcodeReading(isFine);
            if (nRet != 0)
            {
                Log.Write(this, "WaferLoading Fail - BarcodeReading");
                return nRet;
            }
            var c = this.InputCassetteLifter.GetMaterialCassette();
            int nIndex = this.InputCassetteLifter.GetCurrectSlotID();
            MaterialWafer wafer = c.GetWafer(nIndex);

            wafer.CarrierId = c.CarrierId;
            this.SetMaterial(wafer);

            Log.Write(this, "WaferLoading Complete");
            return nRet;
        }
        public int StageLoading(bool isFine = false)
        {
            int nRet = 0;

            if (RunMode == UnitRunMode.Manual)
            {
                CurrentFunc = StageLoading;
            }


            Log.Write(this, "StageLoading Start");
            if (IsMoveInterLockCassette() == false)
            {
                Log.Write(this, "StageLoading Fail - IsMoveInterLockCassette");
                return -1;
            }

            nRet = MovePositionStage(isFine);
            if (nRet != 0)
            {
                AxisInputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_StageLoadingFailed);
                Log.Write(this, "StageLoading Fail - MovePositionStage");
                nRet = -1;
                return nRet;
            }
            if (IsStop) { return 0; }

            nRet = UnClampGripper();
            if (nRet != 0)
            {
                AxisInputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_StageLoadingFailed);
                Log.Write(this, "StageLoading Fail - UnClampGripper");
                nRet = -1;
                return nRet;
            }

            Log.Write(this, "StageLoading End");
            return nRet;
        }
        public int MoveToReady(bool isFine = false)
        {
            int nRet = 0;

            if (RunMode == UnitRunMode.Manual)
            {
                CurrentFunc = MoveToReady;
            }

            Log.Write(this, "MoveToReady Start");
            if (IsMoveInterLockCassette() == false)
            {
                return -1;
            }

            nRet = MovePositionReady(isFine);
            if (nRet != 0)
            {
                AxisInputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_WaferLoadingFailed);
                nRet = -1;
                return nRet;
            }
            if (IsStop) { return 0; }

            nRet = UpFeeder();
            if (nRet != 0)
            {
                AxisInputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_WaferLoadingFailed);
                nRet = -1;
                return nRet;
            }
            Log.Write(this, "MoveToReay End");
            return nRet;
        }

        public int WaferUnloading(bool isFine = false)
        {
            int nRet = 0;

            if (RunMode == UnitRunMode.Manual)
            {
                CurrentFunc = WaferUnloading;
            }
            MaterialWafer wafer = this.InputStage.GetMaterialWafer();
            nRet = WaferUnloading(wafer);
            return nRet;
        }

        private int WaferUnloading(MaterialWafer wafer)
        {
            int nRet = 0;

            nRet = this.InputStage.PrepareInputStageUnloadingWafer();
            if (nRet != 0)
            {
                AxisInputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_StageUnloadingFailed);
                this.State = ProcessState.Error;
                Log.Write(this, "WaferUnloading Fail - PrepareInputStageUnloadingWafer");
                return nRet;
            }
            if (IsStop) { return 0; }

            // 9) Feeder 내부 언로딩
            nRet = UnloadWaferStagetToFeeder();
            if (nRet != 0)
            {
                AxisInputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_StageUnloadingFailed);
                this.State = ProcessState.Error;
                Log.Write(this, "WaferUnloading Fail - UnloadWaferStagetToFeeder");
                return nRet;
            }
            if (IsStop) { return 0; }

            int nSlot = wafer.SlotIndex;
            nRet = this.InputCassetteLifter.MoveToSlot(nSlot); // 언로딩 해야하는 Slot으로 이동 요청.
            if (nRet != 0)
            {
                AxisInputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_WaferUnloadingFailed);
                this.State = ProcessState.Error;
                Log.Write(this, "WaferUnloading Fail - MoveToSlot");
                return nRet;
            }
            if (IsStop) { return 0; }

            nRet = UnloadWaferFeederToCassette(true);
            if (nRet != 0)
            {
                AxisInputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_WaferUnloadingFailed);
                this.State = ProcessState.Error;
                Log.Write(this, "WaferUnloading Fail - UnloadWaferFeederToCassette");
                return nRet;
            }

            Log.Write(this, "WaferUnloading Complete");
            return nRet;
        }
        

        public int UnloadWaferFeederToCassette(bool isFine = false)
        {
            int nRet = 0;

            nRet = UnloadWaferStagetToFeeder(isFine);
            if (nRet != 0)
            {
                AxisInputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_StageUnloadingFailed);
                Log.Write(this, "UnloadWaferFeederToCassette Fail - UnloadWaferStagetToFeeder");
                nRet = -1;
                return nRet;
            }
            if (IsStop) { return 0; }

            nRet = ClampGripper();
            if (nRet != 0)
            {
                AxisInputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_StageUnloadingFailed);
                Log.Write(this, "UnloadWaferFeederToCassette Fail - ClampGripper");
                nRet = -1;
                return nRet;
            }
            MaterialWafer wafer = new MaterialWafer();
            this.InputStage.MoveMaterial(wafer, this);
            if (IsStop) { return 0; }

            nRet = MovePositionCassette(isFine);
            if (nRet != 0)
            {
                AxisInputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_StageUnloadingFailed);
                Log.Write(this, "UnloadWaferFeederToCassette Fail - MovePositionCassette");
                nRet = -1;
                return nRet;
            }
            if (IsStop) { return 0; }

            nRet = UnClampGripper();
            if (nRet != 0)
            {
                AxisInputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_StageUnloadingFailed);
                Log.Write(this, "UnloadWaferFeederToCassette Fail - UnClampGripper");
                nRet = -1;
                return nRet;
            }
            if (IsStop) { return 0; }

            //회피 Position으로 사용.
            nRet = MovePositionBarcode(isFine);
            if (nRet != 0)
            {
                AxisInputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_StageUnloadingFailed);
                Log.Write(this, "UnloadWaferFeederToCassette Fail - MovePositionBarcode");
                nRet = -1;
                return nRet;
            }
            wafer = new MaterialWafer();
            MoveMaterial(wafer, null);
            return nRet;
        }
        #endregion

        #region Seq 단위 동작
        public int MoveToCassette(bool isFine = false)
        {
            int nRet = 0;

            CurrentFunc = MoveToCassette;

            Log.Write(this, "MoveToCassette Start");
            nRet = MovePositionCassette(isFine);
            if (nRet != 0)
            {
                AxisInputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_WaferLoadingFailed);
                nRet = -1;
                return nRet;
            }
            if (IsStop) { return 0; }

            nRet = ClampGripper();
            if (nRet != 0)
            {
                AxisInputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_WaferLoadingFailed);
                nRet = -1;
                return nRet;
            }

            Log.Write(this, "MoveToCassette End");
            return nRet;
        }
        public Task<int> MoveToCassetteAsync(bool isFine)
        {
            return Task.Run(() => OnMoveToCassette(isFine));
        }
        protected int OnMoveToCassette(bool isFine)
        {
            int nRet = 0;
            if (IsInterlockOKWaferLoading() == false)
            {
                AxisInputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_WaferLoadingFailed);
                Log.Write(this, "OnMoveToCassette Fail - IsInterlockOKWaferLoading");
                nRet = -1;
                return nRet;
            }
            nRet = base.MoveTeachingPositionOnce((int)InputFeederConfig.TeachingPositionName.Cassette, isFine);
            if (nRet != 0)
            {
                AxisInputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_WaferLoadingFailed);
                Log.Write(this, "OnMoveToCassette Fail - MoveTeachingPositionOnce");
                nRet = -1;
                return nRet;
            }
            return nRet;
        }
        public int BarcodeReading(bool isFine = false)
        {
            int nRet = 0;

            nRet = MovePositionBarcode(isFine);
            if (nRet != 0)
            {
                AxisInputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_BarcodeReadingFailed);
                Log.Write(this, "BarcodeReading Fail - MovePositionBarcode");
                nRet = -1;
                return nRet;
            }
            if (IsStop) { return 0; }

            // Barcode Reading Logic
            bool isRead = true; // TODO: Barcode Reading Logic
            // isRead = BarcodeReader.Read(...);
            if (!isRead)
            {
                AxisInputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_BarcodeReadingFailed);
                Log.Write(this, "Barcode Reading Failed");
                nRet = -1;
                return nRet;
            }

            return nRet;
        }
        
        public int UnloadWaferStagetToFeeder(bool isFine = false)
        {
            int nRet = 0;

            nRet = UnClampGripper();
            if (nRet != 0)
            {
                PostAlarm((int)AlarmKeys.Alarm_StageUnloadingFailed);
                Log.Write(this, "UnloadWaferStagetToFeeder Fail - UnClampGripper");
                nRet = -1;
                return nRet;
            }
            if (IsStop) { return 0; }

            nRet = DownFeeder();
            if (nRet != 0)
            {
                AxisInputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_StageUnloadingFailed);
                Log.Write(this, "UnloadWaferStagetToFeeder Fail - DownFeeder");
                nRet = -1;
                return nRet;
            }
            if (IsStop) { return 0; }

            nRet = MovePositionStage(isFine);
            if (nRet != 0)
            {
                AxisInputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_StageUnloadingFailed);
                Log.Write(this, "UnloadWaferStagetToFeeder Fail - MovePositionStage");
                nRet = -1;
                return nRet;
            }

            return nRet;
        }

        public int ClampGripper()
        {
            int nRet = 0;
            this.SetClamp(true);
            nRet = WaitClampStateOrAlarm(expectClamp: true, timeoutMs: 1500, pollMs: 2);
            if (nRet != 0)
            {
                AxisInputFeederY?.EmgStop();
                Log.Write(this, "Clamp Failed");
                return -1;
            }
            return 0;

            //if (!IsClamped())
            //{
            //    Log.Write(this, "Clamp Failed");
            //    PostAlarm((int)AlarmKeys.Alarm_GripperClampFailed);
            //    nRet = -1;
            //    return nRet;
            //}
            //return nRet;
        }
        public int UnClampGripper()
        {
            int nRet = 0;
            this.SetClamp(false);
            nRet = WaitClampStateOrAlarm(expectClamp: false, timeoutMs: 1500, pollMs: 2);
            if (nRet != 0)
            {
                AxisInputFeederY?.EmgStop();
                Log.Write(this, "Unclamp Failed");
                return -1;
            }
            return 0;
            //if (!IsUnClamped())
            //{
            //    Log.Write(this, "Unclamp Failed");
            //    PostAlarm((int)AlarmKeys.Alarm_GripperClampFailed);
            //    nRet = -1;
            //    return nRet;
            //}
            //return nRet;
        }
        public int UpFeeder()
        {
            int nRet = 0;
            this.SetLift(true);
            nRet = WaitLiftStateOrAlarm(expectUp: true, timeoutMs: 1500, pollMs: 2);
            if (nRet != 0)
            {
                AxisInputFeederY?.EmgStop();
                Log.Write(this, "Feeder Up Failed");
                return -1;
            }
            return 0;
            //if (!IsFeederUp())
            //{
            //    Log.Write(this, "Feeder Up Failed");
            //    PostAlarm((int)AlarmKeys.Alarm_GripperClampFailed);
            //    nRet = -1;
            //    return nRet;
            //}
            //return nRet;
        }
        public int DownFeeder()
        {
            int nRet = 0;
            this.SetLift(false);
            nRet = WaitLiftStateOrAlarm(expectUp: false, timeoutMs: 1500, pollMs: 2);
            if (nRet != 0)
            {
                AxisInputFeederY?.EmgStop();
                Log.Write("InputFeeder", "WaferLoading", "Feeder Down Failed");
                return -1;
            }
            return 0;
            //if (!IsFeederDown())
            //{
            //    AxisInputFeederY.EmgStop();
            //    Log.Write("InputFeeder", "WaferLoading", "Feeder Down Failed");
            //    PostAlarm((int)AlarmKeys.Alarm_WaferLoadingFailed);
            //    nRet = -1;
            //    return nRet;
            //}
            //return nRet;
        }

        private bool IsInterlockOKWaferLoading()
        {
            bool bRtn = true;
            // Cassette or InputStage 위치 및 Signal 확인 후 진행. 
            if (!InputCassetteLifter.IsWaferReadyForLoading())
            {
                AxisInputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_IsWaferReadyForLoading);
                Log.Write(this, "InputCassetteLifter Not Ready for Loading");
                bRtn = false;
                return bRtn;
            }

            // 이거 애매한디...
            if (!InputStage.IsWaferLoadingPosition())
            {
                AxisInputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_WaferLoadingPosition);
                Log.Write(this, "InputStage Not Ready for Loading");
                bRtn = false;
                return bRtn;
            }
            return bRtn;
        }
        private bool IsInterlockOKMoveToCassette()
        {
            bool isOK = this.InputStage.IsWaferLoadingPosition();
            isOK &= this.InputCassetteLifter.IsScanCompleted();
            return true;
        }
        public bool IsInterlockOKWithCassete()
        {
            bool bRtn = true;

            double dYSafePosOffset = Config.FeederToCassetteOverapLength;
            if (Config.IsSimulation == false)
            {
                if (IsClamped())
                {
                    dYSafePosOffset += Config.WaferRingframeSize;
                }
            }

            var tp = TeachingPositions[(int)InputFeederConfig.TeachingPositionName.Cassette];
            double dInterlockPos = tp.GetAxisPosition(this.AxisInputFeederY.Name);

            dInterlockPos += dYSafePosOffset;
            if (AxisInputFeederY.GetPosition() < dInterlockPos)
            {
                Log.Write(this.UnitName, "IsInterlockOKWithCassete",
                $"FeederY Position Low. Current:" +
                $"{AxisInputFeederY.GetPosition()}, InterlockPos:{dInterlockPos}");

                bRtn = false;
                return bRtn;
            }

            return bRtn;

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

            // --- Simulation 모드: 축 위치가 0(초기 상태) 이면 teaching 여부와 무관하게 OK 처리 ---
            if (Config != null && Config.IsSimulation)
            {
                if (AxisInputFeederY != null)
                {
                    double pos = 0;
                    try { pos = AxisInputFeederY.GetPosition(); } catch { }
                    if (Math.Abs(pos) < 0.001) // 필요 시 공차 Config 로 분리 가능
                    {
                        nRet = MovePositionReady();
                        if (nRet != 0)
                        {
                            Log.Write(this, "CheckReady Fail - MovePositionReady");
                            return nRet;
                        }

                        Log.Write(this, "Simulation - FeederY Position 0 → Ready 통과 (NoPosition 체크 생략)");
                        return nRet; // 바로 OK
                    }
                }
            }

            if (IsPositionBarcode() == false &&
                IsPositionCassette() == false &&
                IsPositionStage() == false &&
                IsPositionReady() == false)
            {
                AxisInputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_InputFeederNoPosition);
                Log.Write(this, "CheckReady Fail - No Position");
                return -1;
            }

            if (InputStage.IsStageInterLockOK() == false)
            {
                if (IsPositionReady())
                {
                    return 0;
                }
                else
                {
                    AxisInputFeederY.EmgStop();
                    PostAlarm((int)AlarmKeys.Alarm_InputStageInterlockFailed);
                    Log.Write(this, "CheckReady Fail - InputStage.IsStageInterLockOK");
                    return -1;
                }
            }

            if (IsPositionCassette()
                || IsPositionBarcode()
                || IsPositionStage())
            {
                if (IsInterlockOKWithCassete() == false)
                {
                    AxisInputFeederY.EmgStop();
                    PostAlarm((int)AlarmKeys.Alarm_InputFeederInterlockFailed);
                    Log.Write(this, "CheckReady Fail - IsInterlockOKWithCassete");
                    return -1;
                }

                if (InputStage.IsWaferLoadingPosition() == false
                || InputStage.IsWaferUnloadingPosition() == false)
                {
                    AxisInputFeederY.EmgStop();
                    PostAlarm((int)AlarmKeys.Alarm_InputStageInterlockFailed);
                    Log.Write(this, "CheckReady Fail - InputStage.IsStageInterLockOK");
                    return -1;
                }

                if (IsClamped() == true)
                {
                    nRet = UnClampGripper();
                    if (nRet != 0)
                    {
                        Log.Write(this, "CheckReady Fail - UnClampGripper");
                        return nRet;
                    }
                }

                nRet = MovePositionReady();
                if (nRet != 0)
                {
                    Log.Write(this, "CheckReady Fail - MovePositionReady");
                    return nRet;
                }

                if (IsFeederUp() == false)
                {
                    nRet = UpFeeder();
                    if (nRet != 0)
                    {
                        Log.Write(this, "CheckReady Fail - UpFeeder");
                        return nRet;
                    }
                }
            }

            return nRet;
        }

        #endregion
    }
}