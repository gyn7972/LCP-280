using QMC.Common;
using QMC.Common.Component;
using QMC.Common.IOUtil;
using QMC.Common.Motion;
using QMC.Common.Motions;
using QMC.Common.Unit;
using QMC.LCP_280.Process.Component;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;

namespace QMC.LCP_280.Process.Unit
{
    /// <summary>
    /// OutputFeeder (Bin Feeder / Ring Transfer - Output side)
    ///  - Y 축 이송 + Lift + Clamp
    ///  - Ring 존재 / Overload 센서
    ///  - Config/Unit 구조를 다른 Unit들과 통일
    /// </summary>
    public class OutputFeeder : BaseUnit<OutputFeederConfig>
    {
        enum AlarmKeys
        {
            Alarm_BinLoadingFailed = 2000,
            Alarm_BarcodeReadingFailed = 2001,
            Alarm_StageLoadingFailed = 2002,
            Alarm_StageUnloadingFailed = 2003,
            Alarm_BinUnloadingFailed = 2004,
            Alarm_OutputStageInterlockFailed = 2010,
            Alarm_GripperClampFailed = 2020,
            Alarm_FeederClampUp = 2021,
            Alarm_IsBinReadyForLoading = 2022,
            Alarm_BinLoadingPosition = 2023,
            Alarm_OutputFeederNoPosition = 2024,
            Alarm_OutputFeederInterlockFailed = 2025,
        }

        #region InitAlarm
        protected override void InitAlarm()
        {
            base.InitAlarm();
            AlarmRegister((int)AlarmKeys.Alarm_BinLoadingFailed,
                "Bin Loading Failed",
                "Bin 로딩에 실패 하였습니다.",
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
            AlarmRegister((int)AlarmKeys.Alarm_BinUnloadingFailed,
                "Bin Unloading Failed",
                "Bin 언로딩에 실패 하였습니다.",
                "Error");
            AlarmRegister((int)AlarmKeys.Alarm_OutputStageInterlockFailed,
                "Output Stage Interlock Failed",
                "Bin 로딩을 위한 인터락이 맞지 않습니다.\n장비 상태를 확인 하여 주십시요.",
                "Error");
            AlarmRegister((int)AlarmKeys.Alarm_GripperClampFailed,
                "Gripper Clamp Failed",
                "그리퍼 클램프에 실패 하였습니다.\n장비 상태를 확인 하여 주십시요.",
                "Error");
            AlarmRegister((int)AlarmKeys.Alarm_FeederClampUp,
                "Feeder Clamp Up Failed",
                "피더 클램프 업 상태가 아닙니다.\n장비 상태를 확인 하여 주십시요.",
                "Error");

            // = 2022,
            AlarmRegister((int)AlarmKeys.Alarm_IsBinReadyForLoading,
                "Bin ReadyForLoading Failed",
                "Ready for Loading 위치가 아닙니다.\n장비 상태를 확인 하여 주십시요.",
                "Error");
            // = 2023,
            AlarmRegister((int)AlarmKeys.Alarm_BinLoadingPosition,
                "Bin Loading Position Failed",
                "Loading 위치가 아닙니다.\n장비 상태를 확인 하여 주십시요.",
                "Error");
            // = 2024,
            AlarmRegister((int)AlarmKeys.Alarm_OutputFeederNoPosition,
                "Output Feeder No Position",
                "Output Feeder 위치가 아닙니다.\n장비 상태를 확인 하여 주십시요.",
                "Error");
            // = 2025,
            AlarmRegister((int)AlarmKeys.Alarm_OutputFeederInterlockFailed,
                "Output Feeder Interlock Failed",
                "Output Feeder 인터락이 맞지 않습니다.\n장비 상태를 확인 하여 주십시요.",
                "Error");
        }
        #endregion

        #region Unit
        public OutputCassetteLifter OutputCassetteLifter { get; set; }
        public OutputStage OutputStage { get; set; }
        #endregion

        #region Axis
        private MotionAxis _feederY;
        public MotionAxis AxisOutputFeederY => _feederY;
        #endregion

        #region IO Domain Members
        private Cylinder _feederLift; // Up/Down
        private Cylinder _cylClamp;   // Clamp / Unclamp
        #endregion

        #region ctor / Initialization
        public OutputFeeder(OutputFeederConfig config = null)
            : base(new OutputFeederConfig())
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
            OutputCassetteLifter = Equipment.Instance.GetUnit("OutputCassetteLifter") as OutputCassetteLifter;
            OutputStage = Equipment.Instance.GetUnit("OutputStage") as OutputStage;
        }

        private void BindAxes()
        {
            var mgr = Equipment.Instance?.AxisManager;
            if (mgr == null)
            {
                Log.Write("OutputFeeder", "[BindAxes] AxisManager null");
                return;
            }

            const string unitName = "Unit"; // Equipment에서 축 등록 시 사용한 유닛명과 동일해야 함
            BindAxis(mgr, unitName, AxisNames.BinFeederY, ref _feederY);
        }
        #endregion

        private bool IsInterlockOKBinLoading()
        {
            bool bRtn = true;
            //Cassette or InputStage 위치 및 Signal 확인 후 진행.
            if (OutputCassetteLifter.IsBinReadyForLoading() == false)
            {
                Log.Write(this, "OutputCassetteLifter Not Ready for Loading");
                bRtn = false;
                return bRtn;
            }

            if(OutputStage.IsPositionBinLoading() == false)
            {
                if (!OutputStage.IsStageInterLockOK())
                {
                    Log.Write(this, "OutputStage Not Ready for Loading");
                    bRtn = false;
                    return bRtn;
                }
            }

            return bRtn;
        }
        private bool IsInterlockOKMoveToCassette()
        {
            bool isOK = this.OutputStage.IsPositionBinLoading();
            isOK &= this.OutputCassetteLifter.IsBinReadyForLoading();
            return true;
        }

        public int MovePositionReady(bool isFine = false)
        {
            Task<int> task = MovePositionAsyncReady(isFine);
            while (IsEndTask(task) == false)
            {
                if(RunMode == UnitRunMode.Auto)
                {
                    if (IsInterlockOKBinLoading() == false)
                    {
                        AxisOutputFeederY.EmgStop();
                        PostAlarm((int)AlarmKeys.Alarm_BinLoadingFailed);
                        return -1;
                    }
                }

                IsMoveInterLockReady();

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
            return MoveTeachingPositionOnce((int)OutputFeederConfig.TeachingPositionName.Ready, isFine);
        }
        private int IsMoveInterLockReady()
        {
            int nRet = 0;
            
            if (OutputStage.IsAnyAxisMoving())
            {
                AxisOutputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_OutputStageInterlockFailed);
                nRet = -1;
                return nRet;
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
            return MoveTeachingPositionOnce((int)OutputFeederConfig.TeachingPositionName.Stage, isFine);
        }
        private int IsMoveInterLockStage()
        {
            int nRet = 0;
            // Check Interlock.!!! 구문 넣을것.!!!
            //if (!IsFeederUp())
            //{
            //    AxisFeederY.EmgStop();
            //    PostAlarm((int)AlarmKeys.Alarm_FeederClampUp);
            //    nRet = -1;
            //    return nRet;
            //}

            if (OutputStage.IsAnyAxisMoving())
            {
                AxisOutputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_OutputStageInterlockFailed);
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
            return MoveTeachingPositionOnce((int)OutputFeederConfig.TeachingPositionName.Barcode, isFine);
        }
        private int IsMoveInterLockBarcode()
        {
            int nRet = 0;
            if (OutputStage.IsAnyAxisMoving())
            {
                AxisOutputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_OutputStageInterlockFailed);
                nRet = -1;
                return nRet;
            }

            if (OutputCassetteLifter.IsAnyAxisMoving())
            {
                AxisOutputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_OutputStageInterlockFailed);
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
                    AxisOutputFeederY.EmgStop();
                    PostAlarm((int)AlarmKeys.Alarm_BinLoadingFailed);
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
            return MoveTeachingPositionOnce((int)OutputFeederConfig.TeachingPositionName.Cassette, isFine);
        }
        private bool IsMoveInterLockCassette()
        {
            bool bRet = true;
            if (OutputStage.IsAnyAxisMoving())
            {
                bRet = false;
                return bRet;
            }

            if (OutputCassetteLifter.IsAnyAxisMoving())
            {
                bRet = false;
                return bRet;
            }

            if (OutputStage.IsPositionBinLoading() == false)
            {
                bRet = false;
                return bRet;
            }

            if (OutputStage.IsPositionBinUnloading() == false)
            {
                bRet = false;
                return bRet;
            }

            return bRet;
        }


        public bool IsFeederZSafetyPosition(bool treatMissingAsSafe = true)
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
        public bool IsFeederYSafetyPosition()
        {
            bool bRtn = false;
            if (AxisOutputFeederY == null)
                return bRtn;

            var cfg = Config;
            if (cfg == null)
                return bRtn;

            bRtn = IsPositionReady();
            return bRtn;
        }
        public bool IsPositionReady()
        {
            var tp = TeachingPositions[(int)OutputFeederConfig.TeachingPositionName.Ready];
            if (tp == null)
                return false;
            return InPosTeaching(tp);
        }
        public bool IsPositionStage()
        {
            var tp = TeachingPositions[(int)OutputFeederConfig.TeachingPositionName.Stage];
            if (tp == null)
                return false;
            return InPosTeaching(tp);
        }
        public bool IsPositionBarcode()
        {
            var tp = TeachingPositions[(int)OutputFeederConfig.TeachingPositionName.Barcode];
            if (tp == null)
                return false;
            return InPosTeaching(tp);
        }
        public bool IsPositionCassette()
        {
            var tp = TeachingPositions[(int)OutputFeederConfig.TeachingPositionName.Cassette];
            if (tp == null)
                return false;
            return InPosTeaching(tp);
        }




        #region Teaching Helpers
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
        //public double GetTP(string tpName, string axisName)
        //{
        //    var tp = Config.GetTeachingPosition(tpName);
        //    if (tp != null && tp.AxisPositions != null && tp.AxisPositions.TryGetValue(axisName, out var v)) return v;
        //    return 0.0;
        //}
        //public bool InPos(MotionAxis ax, double target) => ax == null || ax.InPosition(target);
        #endregion

        #region IO Domain Mapping
        private void BindIoDomains()
        {
            var eq = Equipment.Instance; var unit = eq?.UnitIO; if (unit == null) return;
            if (!IoAutoBindings.Cylinders.TryGetValue("OutFeederLift", out _feederLift))
            {
                Log.Write("OutputFeeder", "BindIoDomains", "Cylinder not found: OutFeederLift");
            }

            if (!IoAutoBindings.Cylinders.TryGetValue("OutFeederClamp", out _cylClamp))
            {
                Log.Write("OutputFeeder", "BindIoDomains", "Cylinder not found: OutFeederClamp");
            }
        }
        #endregion

        // === Domain Control (표준 구동) ===
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

        #region Status Helpers
        public bool IsFeederUp()
        {
            if(Config.IsSimulation)
            {
                return true;
            }
            return this.ReadInput(OutputFeederConfig.IO.FEEDER_UP);
        }
        
        public bool IsFeederDown()
        {
            if (Config.IsSimulation)
            {
                return true;
            }
            return this.ReadInput(OutputFeederConfig.IO.FEEDER_DOWN);
        }
        
        public bool IsClamped()
        {
            bool bRtn = false;
            if (Config.IsSimulation)
            {
                bRtn = true;
                return true;
            }
            bRtn = !this.ReadInput(OutputFeederConfig.IO.FEEDER_UNCLAMP);
            return bRtn;
        }

        public bool IsUnClamped()
        {
            if (Config.IsSimulation)
            {
                return true;
            }
            return this.ReadInput(OutputFeederConfig.IO.FEEDER_UNCLAMP);
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

        // === Cylinder 완료 대기 Helpers ===
        // Clamp: expectClamp=true(CLAMP 기대), false(UNCLAMP 기대)
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

            // OutputFeeder엔 Unclamp 전용 알람 키가 없어 Clamp 실패 알람을 공용 사용
            PostAlarm((int)AlarmKeys.Alarm_GripperClampFailed);
            Log.Write(UnitName, expectClamp ? "[Clamp] Gripper CLAMP timeout" : "[Clamp] Gripper UNCLAMP timeout");
            return -1;
        }

        // Lift: expectUp=true(UP 기대), false(DOWN 기대)
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

            // Up 실패는 FeederClampUp, Down 실패는 BinLoadingFailed로 처리(기존 로직과 동일한 의미)
            int alarm = expectUp
                ? (int)AlarmKeys.Alarm_FeederClampUp
                : (int)AlarmKeys.Alarm_BinLoadingFailed;

            PostAlarm(alarm);
            Log.Write(UnitName, expectUp ? "[Lift] Feeder UP timeout" : "[Lift] Feeder DOWN timeout");
            return -1;
        }

        #endregion

        /// ////////////////////////////////////////////////////////////////////////////////////////
        #region === Direct Valve Control (입력 신호/인터락 무관 강제 구동용) ===
        public bool IsFeederUpValveOn() => this.IsOutputOn(OutputFeederConfig.IO.FEEDER_UP_VALVE);
        public bool IsFeederDownValveOn() => this.IsOutputOn(OutputFeederConfig.IO.FEEDER_DOWN_VALVE);
        public bool IsFeederClampValveOn() => this.IsOutputOn(OutputFeederConfig.IO.FEEDER_CLAMP_VALVE);
        public bool IsFeederUnclampValveOn() => this.IsOutputOn(OutputFeederConfig.IO.FEEDER_UNCLAMP_VALVE);
        #endregion

        #region Sequence Signals
        bool NeedUnloadFirst { get; set; } = false;

        bool UnitDryRunTest { get; set; } = false;
        #endregion

        #region Runtime
        public override int OnRun()
        {
            int ret = 0;

            if (this.RunUnitStatus == UnitStatus.Stopped ||
                this.RunUnitStatus == UnitStatus.Stopping ||
                this.RunUnitStatus == UnitStatus.CycleStop)
            {
                this.State = ProcessState.Stop;
                return 1;
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

        // DryRun 반복 제어용 최소 상태(토글)
        private bool _dryLoadedToStage = false;   // 마지막 사이클에서 Stage에 로딩했는지 여부
        private int _dryLastSlotIndex = -1;       // 마지막으로 픽업한 Slot (언로딩 대상)
        protected override int OnRunWork()
        {
            int nRet = 0;

            MaterialWafer wafer = this.OutputStage.GetMaterialWafer();
            try
            {
                if (OutputStage.IsWorking())
                {
                    if (OutputStage.HasNextDie())
                    {
                        NeedUnloadFirst = false;
                        return 0;
                    }
                    else
                    {
                        NeedUnloadFirst = true;
                    }

                    if (wafer != null)
                    {
                        if (wafer.ProcessSatate == Material.MaterialProcessSatate.Ready)
                        {
                            return nRet = PreparetoOutputStage();
                        }
                        else if (wafer.ProcessSatate == Material.MaterialProcessSatate.Processing)
                        {
                            if (OutputStage.IsStageInterLockOK() == false)
                            {
                                nRet = OutputStage.LoadingBinComplete();
                                if (nRet != 0)
                                {
                                    AxisOutputFeederY.EmgStop();
                                    PostAlarm((int)AlarmKeys.Alarm_StageLoadingFailed);
                                    this.State = ProcessState.Error;
                                    return nRet;
                                }
                                //if (this.IsStop) { return 0; }
                            }
                            return nRet;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                NeedUnloadFirst = false;
            }

            // 0) Stage에 제품이 있으면 "언로딩 먼저"
            if (NeedUnloadFirst || _dryLoadedToStage)
            {
                NeedUnloadFirst = true;
                // 8) Feeder -> Stage: WaferUnloadingBeforeStage
                nRet = BinUnloading(wafer);
                if (nRet != 0)
                {
                    AxisOutputFeederY.EmgStop();
                    PostAlarm((int)AlarmKeys.Alarm_BinUnloadingFailed);
                    this.State = ProcessState.Error;
                    return nRet;
                }
                if (IsStop) { return 0; }
            }
            //if(this.IsStop) { return 0; }
            
            _dryLoadedToStage = false;
            // 1) Feeder -> Cassette: Scan
            if (this.OutputCassetteLifter.IsScanCompleted() == false)
            {
                nRet = this.OutputCassetteLifter.ScanBin();
                if (nRet != 0)
                {
                    AxisOutputFeederY.EmgStop();
                    PostAlarm((int)AlarmKeys.Alarm_BinLoadingFailed);
                    this.State = ProcessState.Error;
                    return nRet;
                }
            }
            //if (this.IsStop) { return 0; }

            if (this.OutputCassetteLifter.IsHaveMoreProcessWafer())
            {
                // 2) Feeder -> Cassette: MoveToNextSlot
                nRet = this.OutputCassetteLifter.MoveToNextSlot();
                if (nRet != 0)
                {
                    AxisOutputFeederY.EmgStop();
                    PostAlarm((int)AlarmKeys.Alarm_BinLoadingFailed);
                    this.State = ProcessState.Error;
                    return nRet;
                }
                //if (this.IsStop) { return 0; }

                // 3) Feeder -> Stage: WaferLoadingBeforeStage
                nRet = OutputStage.LoadingBinPrepare();
                if (nRet != 0)
                {
                    AxisOutputFeederY.EmgStop();
                    PostAlarm((int)AlarmKeys.Alarm_OutputStageInterlockFailed);
                    this.State = ProcessState.Error;
                    return nRet;
                }
                //if (this.IsStop) { return 0; }

                // 4) Feeder 내부 로딩 Cascette에서 Wafer Pick
                nRet = BinLoading(); // 여기서 Barcode Reading 포함
                if (nRet != 0)
                {
                    AxisOutputFeederY.EmgStop();
                    PostAlarm((int)AlarmKeys.Alarm_BinLoadingFailed);
                    this.State = ProcessState.Error;
                    return nRet;
                }
                //if (this.IsStop) { return 0; }

                // 4) Feeder 내부 로딩 Stage에 Wafer Load
                nRet = StageLoading();
                if (nRet != 0)
                {
                    AxisOutputFeederY.EmgStop();
                    PostAlarm((int)AlarmKeys.Alarm_StageLoadingFailed);
                    this.State = ProcessState.Error;
                    return nRet;
                }
                //if (this.IsStop) { return 0; }

                nRet = MoveToReady();
                if (nRet != 0)
                {
                    AxisOutputFeederY.EmgStop();
                    PostAlarm((int)AlarmKeys.Alarm_BinLoadingFailed);
                    this.State = ProcessState.Error;
                    return nRet;
                }
                //if (this.IsStop) { return 0; }

                // 5) Feeder -> Stage: WaferLoadingAfterStage
                nRet = OutputStage.LoadingBinComplete();
                if (nRet != 0)
                {
                    AxisOutputFeederY.EmgStop();
                    PostAlarm((int)AlarmKeys.Alarm_StageLoadingFailed);
                    this.State = ProcessState.Error;
                    return nRet;
                }
                //if (this.IsStop) { return 0; }

                nRet = PreparetoOutputStage();
                if (nRet != 0)
                {
                    AxisOutputFeederY.EmgStop();
                    PostAlarm((int)AlarmKeys.Alarm_StageLoadingFailed);
                    this.State = ProcessState.Error;
                    return nRet;
                }

                MakePath();
                this.MoveMaterial(new MaterialWafer(), OutputStage);
                var waferOutputStage = OutputStage.GetMaterialWafer();
                //waferOutputStage.ProcessSatate = Material.MaterialProcessSatate.Ready;
                waferOutputStage.ProcessSatate = Material.MaterialProcessSatate.Processing;
                OutputStage.SetMaterial(waferOutputStage);

                this.OutputStage.UpdateUI();

                if (Config.IsUnitDryRun)
                {
                    _dryLoadedToStage = true; // 다음엔 언로딩
                    _dryLastSlotIndex = this.OutputCassetteLifter.GetCurrectSlotID();
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
                        AxisOutputFeederY.EmgStop();
                        PostAlarm((int)AlarmKeys.Alarm_BinLoadingFailed);
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
            this.OutputCassetteLifter.Start();
            this.OutputStage.Start();
            
            return base.OnStart();
        }
        public override int OnStop()
        {
            int ret = 0;
            this.RunUnitStatus = UnitStatus.Stopped;
            this.State = ProcessState.Stop;

            OutputStage.Stop();
            OutputCassetteLifter.Stop();

            base.OnStop();
            return ret;
        }
        #endregion

        public int MakePath()
        {
            int nRet = 0;
            MaterialWafer wafer = this.GetMaterial() as MaterialWafer;
            if (wafer != null)
            {
                if (wafer.ProcessSatate == Material.MaterialProcessSatate.Ready)
                {
                    string measName = null;
                    wafer.Dies.Clear();

                    try
                    {
                        var eq = Equipment.Instance;
                        var recipe = eq.EquipmentRecipe.CurrentRecipe;

                        // 중심 기준(0,0) 계산을 위해 반쪽 인덱스를 실수로 보관
                        double centerX = (recipe.BinCountX - 1) / 2.0;
                        double centerY = (recipe.BinCountY - 1) / 2.0;

                        for (int y = 0; y < recipe.BinCountY; y++)
                        {
                            for (int x = 0; x < recipe.BinCountX; x++)
                            {
                                // 중심 기준 정수 좌표(반올림)
                                //double mapX = (int)Math.Round(x - centerX);
                                //double mapY = (int)Math.Round(y - centerY);
                                double mapX = (x - centerX);
                                double mapY = (y - centerY);
                                var die = new MaterialDie
                                {
                                    Presence = Material.MaterialPresence.NotExist,
                                    ProcessSatate = Material.MaterialProcessSatate.Unknown,

                                    // 배치 인덱스(로직용, 0 기반)
                                    //BinX = x,
                                    //BinY = y,

                                    BinX = mapX,
                                    BinY = mapY,
                                    // 중심 기준 좌표(표시/계산용)
                                    MapX = mapX,
                                    MapY = mapY
                                };

                                wafer.Dies.Add(die);
                            }
                        }
                    }
                    catch { measName = null; }
                }
            }

            return nRet;
        }
        //protected int MakePath()
        //{
        //    int nRet = 0;
        //    MaterialWafer wafer = this.GetMaterial() as MaterialWafer;
        //    if (wafer != null)
        //    {
        //        if (wafer.ProcessSatate == Material.MaterialProcessSatate.Ready)
        //        {
        //            string measName = null;
        //            wafer.Dies.Clear();

        //            try
        //            {
        //                var eq = Equipment.Instance;
        //                var recipe = eq.EquipmentRecipe.CurrentRecipe;
        //                for (int y = 0; y < recipe.BinCountY; y++)
        //                {
        //                    for (int x = 0; x < recipe.BinCountX; x++)
        //                    {
        //                        MaterialDie die = new MaterialDie();
        //                        die.Presence = Material.MaterialPresence.NotExist;
        //                        die.ProcessSatate = Material.MaterialProcessSatate.Unknown;
        //                        die.BinX = x;
        //                        die.BinY = y;
        //                        wafer.Dies.Add(die);
        //                    }
        //                }

        //            }
        //            catch { measName = null; }
        //        }
        //    }


        //    return nRet;
        //}

        protected override void OnMakeSequence()
        {
            base.OnMakeSequence();
            this.SequencePlayers.Add(BinLoading);
            this.SequencePlayers.Add(StageLoading);
            this.SequencePlayers.Add(MoveToReady);
            this.SequencePlayers.Add(BinUnloading);
        }

        #region Seq 단위 동작 함수
        private int PreparetoOutputStage()
        {
            int nRet = 0;

            // T 보정 필요시. 
            //nRet = OutputStage.ScanBin();

            return nRet;
        }
        public int BinLoading(bool isFine = false)
        {
            int nRet = 0;

            if(RunMode == UnitRunMode.Manual)
            {
                CurrentFunc = BinLoading;
            }

            Log.Write(this, "BinLoading Start");
            if (IsMoveInterLockCassette() == false)
            {
                Log.Write(this, "Not IsMoveInterLockCassette");
                return -1;
            }

            if (NeedUnloadFirst)
            {
                Thread.Sleep(500);
                if (IsPositionBarcode() == false)
                {
                    if (IsPositionBarcode() == false)
                    {
                        Log.Write(this, "WaferLoading - MovePositionBarcode First");
                        return -1;
                    }
                }
            }
            else
            {
                if (IsPositionReady() == false)
                {
                    nRet = MoveToReady(isFine);
                    if (nRet != 0)
                    {
                        Log.Write(this, "BinLoading Failed - MoveToReady");
                        return nRet;
                    }
                }
            }

            nRet = UnClampGripper();
            if (nRet != 0)
            {
                Log.Write(this, "UnClampGripper Failed");
                return nRet;
            }

            nRet = DownFeeder();
            if (nRet != 0)
            {
                Log.Write(this, "DownFeeder Failed");
                return nRet;
            }

            nRet = MoveToCassette(isFine);
            if (nRet != 0)
            {
                Log.Write(this, "MoveToCassette Failed");
                return nRet;
            }

            nRet = BarcodeReading(isFine);
            if (nRet != 0)
            {
                Log.Write(this, "BarcodeReading Failed");
                return nRet;
            }
            var c = this.OutputCassetteLifter.GetMaterialCassette();
            int nIndex = this.OutputCassetteLifter.GetCurrectSlotID();
            MaterialWafer wafer = c.GetWafer(nIndex);
            this.SetMaterial(wafer);
            
            Log.Write(this, "BinLoading Complete");
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
                Log.Write(this, "Not IsMoveInterLockCassette");
                return -1;
            }

            nRet = MovePositionStage(isFine);
            if (nRet != 0)
            {
                AxisOutputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_StageLoadingFailed);
                Log.Write(this, "MovePositionStage Failed");
                nRet = -1;
                return nRet;
            }
            //if (IsStop) { return 0; }

            nRet = UnClampGripper();
            if (nRet != 0)
            {
                AxisOutputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_StageLoadingFailed);
                Log.Write(this, "UnClampGripper Failed");
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
            Log.Write(this, "MoveToReay Start");
            if (IsMoveInterLockCassette() == false)
            {
                return -1;
            }

            nRet = MovePositionReady(isFine);
            if (nRet != 0)
            {
                AxisOutputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_BinLoadingFailed);
                nRet = -1;
                return nRet;
            }

            nRet = UpFeeder();
            if (nRet != 0)
            {
                AxisOutputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_BinLoadingFailed);
                nRet = -1;
                return nRet;
            }
            Log.Write(this, "MoveToReay End");


            return nRet;
        }
        public int BinUnloading(bool isFine = false)
        {
            int nRet = 0;

            if (RunMode == UnitRunMode.Manual)
            {
                CurrentFunc = BinUnloading;
            }

            MaterialWafer wafer = this.OutputStage.GetMaterialWafer();
            nRet = BinUnloading(wafer, isFine);
            return nRet;
        }
        public int BinUnloading(MaterialWafer wafer, bool isFine = false)
        {
            int nRet = 0;
            if (RunMode == UnitRunMode.Manual)
            {
                CurrentFunc = MoveToReady;
            }
            Log.Write(this, "BinUnloading Start");

            nRet = this.OutputStage.PrepareOutputStageUnloadingBin();
            if (nRet != 0)
            {
                AxisOutputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_BinUnloadingFailed);
                Log.Write(this, "OutputStage.PrepareOutputStageUnloadingBin Failed");
                nRet = -1;
                return nRet;
            }

            nRet = UnloadBinStageToFeeder(isFine);
            if (nRet != 0)
            {
                AxisOutputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_StageUnloadingFailed);
                Log.Write(this, "UnloadBinStageToFeeder Failed");
                nRet = -1;
                return nRet;
            }

            int nSlot = wafer.SlotIndex;
            nRet = this.OutputCassetteLifter.MoveToSlot(nSlot); // 언로딩 해야하는 Slot으로 이동 요청.
            if (nRet != 0)
            {
                AxisOutputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_BinUnloadingFailed);
                this.State = ProcessState.Error;
                Log.Write(this, "OutputCassetteLifter.MoveToSlot Failed");
                return nRet;
            }

            nRet = UnloadBinFeederToCassette(true);
            if (nRet != 0)
            {
                AxisOutputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_BinUnloadingFailed);
                this.State = ProcessState.Error;
                Log.Write(this, "UnloadBinFeederToCassette Failed");
                return nRet;
            }

            Log.Write(this, "BinUnloading Complete");
            return nRet;
        }
        
        
        public int UnloadBinFeederToCassette(bool isFine = false)
        {
            int nRet = 0;

            nRet = UnloadBinStagetToFeeder(isFine);
            if (nRet != 0)
            {
                AxisOutputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_StageUnloadingFailed);
                Log.Write(this, "UnloadBinStagetToFeeder Failed");
                nRet = -1;
                return nRet;
            }

            nRet = ClampGripper();
            if (nRet != 0)
            {
                AxisOutputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_StageUnloadingFailed);
                Log.Write(this, "ClampGripper Failed");
                nRet = -1;
                return nRet;
            }
            MaterialWafer wafer = new MaterialWafer();
            this.OutputStage.MoveMaterial(wafer, this);

            nRet = MovePositionCassette(isFine);
            if (nRet != 0)
            {
                AxisOutputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_StageUnloadingFailed);
                Log.Write(this, "MovePositionCassette Failed");
                nRet = -1;
                return nRet;
            }

            nRet = UnClampGripper();
            if (nRet != 0)
            {
                AxisOutputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_StageUnloadingFailed);
                Log.Write(this, "UnClampGripper Failed");
                nRet = -1;
                return nRet;
            }

            //회피 Position으로 사용.
            nRet = MovePositionBarcode(isFine);
            if (nRet != 0)
            {
                AxisOutputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_StageUnloadingFailed);
                Log.Write(this, "MovePositionBarcode Failed");
                nRet = -1;
                return nRet;
            }
            wafer = new MaterialWafer();
            MoveMaterial(wafer, null);
            return nRet;
        }
        public int ClampGripper()
        {
            int nRet = 0;
            this.SetClamp(true);
            nRet = WaitClampStateOrAlarm(expectClamp: true, timeoutMs: 1500, pollMs: 2);
            if (nRet != 0)
            {
                AxisOutputFeederY?.EmgStop();
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
                AxisOutputFeederY?.EmgStop();
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
                AxisOutputFeederY?.EmgStop();
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
                AxisOutputFeederY?.EmgStop();
                Log.Write(this, "Feeder Down Failed");
                return -1;
            }
            return 0;
            //if (!IsFeederDown())
            //{
            //    Log.Write(this, "Feeder Down Failed");
            //    PostAlarm((int)AlarmKeys.Alarm_BinLoadingFailed);
            //    nRet = -1;
            //    return nRet;
            //}
            //return nRet;
        }
        public int MoveToCassette(bool isFine = false)
        {
            int nRet = 0;
            nRet = MovePositionCassette(isFine);
            if (nRet != 0)
            {
                AxisOutputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_BinLoadingFailed);
                Log.Write(this, "MovePositionCassette Failed");
                nRet = -1;
                return nRet;
            }

            nRet = ClampGripper();
            if (nRet != 0)
            {
                PostAlarm((int)AlarmKeys.Alarm_BinLoadingFailed);
                Log.Write(this, "ClampGripper Failed");
                nRet = -1;
                return nRet;
            }

            return nRet;
        }
        protected int OnMoveToCassette(bool isFine)
        {
            int nRet = 0;
            if (IsInterlockOKWaferLoading() == false)
            {
                AxisOutputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_BinLoadingFailed);
                Log.Write(this, "Not IsInterlockOKWaferLoading");
                nRet = -1;
                return nRet;
            }
            nRet = base.MoveTeachingPositionOnce((int)OutputFeederConfig.TeachingPositionName.Cassette, isFine);
            if (nRet != 0)
            {
                AxisOutputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_BinLoadingFailed);
                Log.Write(this, "MoveTeachingPositionOnce Failed");
                nRet = -1;
                return nRet;
            }
            return nRet;
        }
        public Task<int> MoveToCassetteAsync(bool isFine)
        {
            return Task.Run(() => OnMoveToCassette(isFine));
        }
        private bool IsInterlockOKWaferLoading()
        {
            bool bRtn = true;
            // Cassette or InputStage 위치 및 Signal 확인 후 진행. 
            if (!OutputCassetteLifter.IsBinReadyForLoading())
            {
                AxisOutputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_IsBinReadyForLoading);
                Log.Write(this, "OutputCassetteLifter Not Ready for Loading");
                bRtn = false;
                return bRtn;
            }

            if (!OutputStage.IsPositionBinLoading())
            {
                AxisOutputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_BinLoadingPosition);
                Log.Write(this, "OutputStage Not Ready for Loading");
                bRtn = false;
                return bRtn;
            }
            return bRtn;
        }
        public int BarcodeReading(bool isFine = false)
        {
            int nRet = 0;

            nRet = MovePositionBarcode(isFine);
            if (nRet != 0)
            {
                AxisOutputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_BarcodeReadingFailed);
                Log.Write(this, "MovePositionBarcode Failed");
                nRet = -1;
                return nRet;
            }

            // Barcode Reading Logic
            bool isRead = true; // TODO: Barcode Reading Logic
            // isRead = BarcodeReader.Read(...);
            if (!isRead)
            {
                AxisOutputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_BarcodeReadingFailed);
                Log.Write(this, "Barcode Reading Failed");
                nRet = -1;
                return nRet;
            }

            return nRet;
        }
        public int UnloadBinStageToFeeder(bool isFine = false)
        {
            int nRet = 0;

            nRet = UnClampGripper();
            if (nRet != 0)
            {
                PostAlarm((int)AlarmKeys.Alarm_StageUnloadingFailed);
                Log.Write(this, "UnClampGripper Failed");
                nRet = -1;
                return nRet;
            }

            nRet = DownFeeder();
            if (nRet != 0)
            {
                PostAlarm((int)AlarmKeys.Alarm_StageUnloadingFailed);
                Log.Write(this, "DownFeeder Failed");
                nRet = -1;
                return nRet;
            }

            nRet = MovePositionStage(isFine);
            if (nRet != 0)
            {
                AxisOutputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_StageUnloadingFailed);
                Log.Write(this, "MovePositionStage Failed");
                nRet = -1;
                return nRet;
            }

            return nRet;
        }
        //UnloadBinStagetToFeeder
        public int UnloadBinStagetToFeeder(bool isFine = false)
        {
            int nRet = 0;

            nRet = UnClampGripper();
            if (nRet != 0)
            {
                PostAlarm((int)AlarmKeys.Alarm_StageUnloadingFailed);
                Log.Write(this, "UnClampGripper Failed");
                nRet = -1;
                return nRet;
            }

            nRet = DownFeeder();
            if (nRet != 0)
            {
                AxisOutputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_StageUnloadingFailed);
                Log.Write(this, "DownFeeder Failed");
                nRet = -1;
                return nRet;
            }

            nRet = MovePositionStage(isFine);
            if (nRet != 0)
            {
                AxisOutputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_StageUnloadingFailed);
                Log.Write(this, "MovePositionStage Failed");
                nRet = -1;
                return nRet;
            }

            return nRet;
        }
        //IsInterlockOKWithCassete
        public bool IsInterlockOKWithCassete()
        {
            bool bRtn = true;

            double dYSafePosOffset = Config.dFeederToCassetteOverapLength;
            if (Config.IsSimulation == false)
            {
                if (IsClamped())
                {
                    dYSafePosOffset += Config.dWaferRingframeSize;
                }
            }
               
            var tp = TeachingPositions[(int)InputFeederConfig.TeachingPositionName.Cassette];
            double dInterlockPos = tp.GetAxisPosition(this.AxisOutputFeederY.Name);

            dInterlockPos += dYSafePosOffset;
            if (AxisOutputFeederY.GetPosition() < dInterlockPos)
            {
                Log.Write(this.UnitName, "IsInterlockOKWithCassete", 
                $"FeederY Position Low. Current:" +
                $"{AxisOutputFeederY.GetPosition()}, InterlockPos:{dInterlockPos}");
                
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
                if (AxisOutputFeederY != null)
                {
                    double pos = 0;
                    try { pos = AxisOutputFeederY.GetPosition(); } catch { }
                    if (Math.Abs(pos) < 0.001) // 필요 시 공차 Config 로 분리 가능
                    {
                        nRet = MovePositionReady();
                        if (nRet != 0)
                        {
                            Log.Write(this, "CheckReady Fail - MovePositionReady");
                            return nRet;
                        }

                        Log.Write(this, "Simulation - AxisFeederY Position 0 → Ready 통과 (NoPosition 체크 생략)");
                        return nRet; // 바로 OK
                    }
                }
            }

            if (IsPositionBarcode() == false &&
                IsPositionCassette() == false &&
                IsPositionStage() == false &&
                IsPositionReady() == false)
            {
                PostAlarm((int)AlarmKeys.Alarm_OutputFeederNoPosition);
                Log.Write(this, "OnEnsureReady Fail - No Position");
                return -1;
            }

            if (OutputStage.IsStageInterLockOK())
            {
                if (IsPositionReady())
                {
                    return 0;
                }
                else
                {
                    PostAlarm((int)AlarmKeys.Alarm_OutputStageInterlockFailed);
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
                    PostAlarm((int)AlarmKeys.Alarm_OutputFeederInterlockFailed);
                    Log.Write(this, "CheckReady Fail - IsInterlockOKWithCassete");
                    return -1;
                }

                if (OutputStage.IsPositionBinLoading() == false
                || OutputStage.IsPositionBinUnloading() == false)
                {
                    PostAlarm((int)AlarmKeys.Alarm_OutputStageInterlockFailed);
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
