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
                "바코드 읽기에 실패 하였습니다. 바코드 상태를 확인 하여 주십시요",
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
                "Bin 로딩을 위한 인터락이 맞지 않습니다. 장비 상태를 확인 하여 주십시요.",
                "Error");
            AlarmRegister((int)AlarmKeys.Alarm_GripperClampFailed,
                "Gripper Clamp Failed",
                "그리퍼 클램프에 실패 하였습니다. 장비 상태를 확인 하여 주십시요.",
                "Error");
            AlarmRegister((int)AlarmKeys.Alarm_FeederClampUp,
                "Feeder Clamp Up Failed",
                "피더 클램프 업 상태가 아닙니다. 장비 상태를 확인 하여 주십시요.",
                "Error");

            // = 2022,
            AlarmRegister((int)AlarmKeys.Alarm_IsBinReadyForLoading,
                "Bin ReadyForLoading Failed",
                "Ready for Loading 위치가 아닙니다. 장비 상태를 확인 하여 주십시요.",
                "Error");
            // = 2023,
            AlarmRegister((int)AlarmKeys.Alarm_BinLoadingPosition,
                "Bin Loading Position Failed",
                "Loading 위치가 아닙니다. 장비 상태를 확인 하여 주십시요.",
                "Error");
            // = 2024,
            AlarmRegister((int)AlarmKeys.Alarm_OutputFeederNoPosition,
                "Output Feeder No Position",
                "Output Feeder 위치가 아닙니다. 장비 상태를 확인 하여 주십시요.",
                "Error");
            // = 2025,
            AlarmRegister((int)AlarmKeys.Alarm_OutputFeederInterlockFailed,
                "Output Feeder Interlock Failed",
                "Output Feeder 인터락이 맞지 않습니다. 장비 상태를 확인 하여 주십시요.",
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
        // Safety 동작 중 여부
        private bool _isSafetyMoving = false;

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
        public override bool IsInterlockOK(BaseComponent baseComponent, BaseComponent.InterlockEventArgs e)
        {
            bool bRet = base.IsInterlockOK(baseComponent, e);
            if (baseComponent == this.AxisOutputFeederY)
            {
                if (_isSafetyMoving)
                    return true;
                if (this.IsFeederDown())
                {
                    if (this.OutputStage.IsPositionBinLoading() == false
                        || this.OutputStage.IsPositionBinUnloading() == false)
                    {
                        this.AxisOutputFeederY?.EmgStop();
                        PostAlarm((int)AlarmKeys.Alarm_OutputStageInterlockFailed);
                        bRet = false;
                    }
                    else
                    {
                        if (this.IsPositionCassette())
                        {
                            bRet = IsInterlockOKWithCassette(e);
                            if (bRet == false)
                            {
                                this.AxisOutputFeederY?.EmgStop();
                                PostAlarm((int)AlarmKeys.Alarm_OutputStageInterlockFailed);
                                return bRet;
                            }
                        }
                    }
                }
            }
            else if (baseComponent == this._feederLift)
            {
                if (this.IsPositionCassette())
                {
                    PostAlarm((int)AlarmKeys.Alarm_OutputStageInterlockFailed);
                    bRet = false;
                }
            }
            return bRet;
        }
        private bool IsInterlockOKWithCassette(BaseComponent.InterlockEventArgs e)
        {
            if (this.OutputStage.IsPlateUp() || this.OutputStage.IsClampLiftUp())
            {
                double dCurrentY = this.AxisOutputFeederY.GetPosition();
                double dStageY = this.GetTP(OutputFeederConfig.TeachingPositionName.Cassette.ToString(), this.AxisOutputFeederY.Name);
                if (dCurrentY > dStageY + this.AxisOutputFeederY.Config.InposTolerance)
                {
                    return false;
                }

            }
            return true;
        }
        private bool IsInterlockOKBinLoading()
        {
            bool bRtn = true;
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
            return isOK;
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
            _isSafetyMoving = true;
            try
            {
                return MoveTeachingPositionOnce((int)OutputFeederConfig.TeachingPositionName.Ready, isFine);
            }
            finally
            {
                _isSafetyMoving = false;
            }
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
                // 시뮬레이션: 실제 보유 머티리얼로 판단
                return this.GetMaterial() is MaterialWafer;
                //return true;
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
        // DryRun 반복 제어용 최소 상태(토글)
        private bool _dryLoadedToStage = false;   // 마지막 사이클에서 Stage에 로딩했는지 여부
        private int _dryLastSlotIndex = -1;       // 마지막으로 픽업한 Slot (언로딩 대상)
        #endregion

        #region Lifecycle
        public override int OnRun()
        {
            int ret = 0;
            if (this.RunUnitStatus == UnitStatus.Stopped
                || this.RunUnitStatus == UnitStatus.Stopping)
            {
                this.State = ProcessState.Stop;
                return 0; // 에러 리턴 없음
            }

            if (this.RunUnitStatus == UnitStatus.CycleStop)
            {
                this.State = ProcessState.Ready; // 안전 대기
                return 0;
            }
            //if (this.RunUnitStatus == UnitStatus.Stopped ||
            //    this.RunUnitStatus == UnitStatus.Stopping ||
            //    this.RunUnitStatus == UnitStatus.CycleStop)
            //{
            //    this.State = ProcessState.Stop;
            //    ret = -1;
            //}

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

            //if (this.RunUnitStatus == UnitStatus.Running)
            //{
            //    return 0;
            //}
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
            this.State = ProcessState.Stop;

            _loadStep = LoadFlowStep.None;
            _exchangeStandbyForNextLoad = false;

            base.OnStop();
            return ret;
        }
        protected override int OnRunReady()
        {
            int nRet = 0;

            MaterialWafer waferBin = this.OutputStage.GetMaterialWafer();
            try
            {
                // Stage가 가동 중인 경우(공정 진행 중)
                if (this.OutputStage.IsWorking())
                {
                    if (waferBin != null)
                    {
                        //bool canPlace = OutputStage.CanPlaceDie();
                        //if (!canPlace)
                        //{
                        //    // 더 이상 놓을 공간이 없음 → 언로딩부터 수행하도록 플래그 세팅 후 Work로 전환
                        //    waferBin.ProcessSatate = Material.MaterialProcessSatate.Completed;
                        //    if (waferBin != null && waferBin.SlotIndex != -1)
                        //    {
                        //        // 실기: 센서 기반 존재 판단
                        //        NeedUnloadFirst = OutputStage.IsRingPresent();
                        //    }
                        //    else
                        //    {
                        //        NeedUnloadFirst = false;
                        //    }
                        //    //NeedUnloadFirst = true;
                        //    this.State = ProcessState.Work;
                        //    return 0;
                        //}

                        // 정지했다가 다시 했을 경우에만 들어와야함. 안들어와야 정상임.
                        if (waferBin.ProcessSatate == Material.MaterialProcessSatate.Ready)
                        {
                            nRet = PreparetoOutputStage();
                        }
                    }
                    return nRet;
                }
                else // Stage에 제품 작업이 완료일때.
                {
                    bool sim = (Config.IsSimulation || Config.IsDryRun);
                    if (sim == false)
                    {
                        if (waferBin != null && waferBin.SlotIndex != -1)
                        {
                            // 실기: 센서 기반 존재 판단
                            NeedUnloadFirst = OutputStage.IsRingPresent();
                        }
                        else
                        {
                            NeedUnloadFirst = false;
                        }
                    }
                    else
                    {
                        // 시뮬/드라이런: 데이터 기반 판단
                        NeedUnloadFirst = (waferBin != null && waferBin.SlotIndex != -1);
                    }
                }

                this.State = ProcessState.Work;

                // 기존 코드
                {
                    //// 다음 다이가 남아 있으면 언로딩 없이 계속 진행
                    //if (this.OutputStage.HasNextDie())
                    //    {
                    //        NeedUnloadFirst = false;
                    //    }
                    //    else
                    //    {
                    //        // 더 이상 다이가 없으면 언로딩 우선
                    //        NeedUnloadFirst = true;
                    //    }

                    //    if (waferBin != null)
                    //    {
                    //        // 재개 직후, Stage 쪽 준비만 필요할 때
                    //        if (waferBin.ProcessSatate == Material.MaterialProcessSatate.Ready)
                    //        {
                    //            nRet = PreparetoOutputStage();
                    //            return nRet;
                    //        }
                    //        else if (waferBin.ProcessSatate == Material.MaterialProcessSatate.Processing)
                    //        {
                    //            // Stage 인터락이 아직 풀리지 않은 경우 완료 신호 처리
                    //            if (this.OutputStage.IsStageInterLockOK() == false)
                    //            {
                    //                nRet = this.OutputStage.LoadingBinComplete();
                    //                if (nRet != 0)
                    //                {
                    //                    AxisOutputFeederY.EmgStop();
                    //                    PostAlarm((int)AlarmKeys.Alarm_StageLoadingFailed);
                    //                    this.State = ProcessState.Error;
                    //                    return nRet;
                    //                }
                    //            }
                    //            return nRet;
                    //        }
                    //    }
                    //}
                    //else
                    //{
                    //    // Stage가 멈춘 상태(작업 완료) → 언로딩 필요 여부 판정
                    //    bool sim = (Config.IsSimulation || Config.IsDryRun);
                    //    if (!sim)
                    //    {
                    //        // 실기: wafer 정보가 있으면 언로딩이 필요한 상태로 간주
                    //        NeedUnloadFirst = (waferBin != null && waferBin.SlotIndex != -1);
                    //    }
                    //    else
                    //    {
                    //        // 시뮬/드라이런: 데이터 기반 판단
                    //        NeedUnloadFirst = (waferBin != null && waferBin.SlotIndex != -1);
                    //    }
                    //}
                }

            }
            catch (Exception ex)
            {
                Log.Write(ex);
                NeedUnloadFirst = false;
                return nRet;
            }

            return nRet;
        }
        protected override int OnRunWork()
        {
            int nRet = 0;

            MaterialWafer wafer = this.OutputStage.GetMaterialWafer();
            //기존코드
            {
                //try
                //{
                //    if (OutputStage.IsWorking())
                //    {
                //        if (OutputStage.HasNextDie())
                //        {
                //            NeedUnloadFirst = false;
                //            return 0;
                //        }
                //        else
                //        {
                //            NeedUnloadFirst = true;
                //        }

                //        if (wafer != null)
                //        {
                //            if (wafer.ProcessSatate == Material.MaterialProcessSatate.Ready)
                //            {
                //                return nRet = PreparetoOutputStage();
                //            }
                //            else if (wafer.ProcessSatate == Material.MaterialProcessSatate.Processing)
                //            {
                //                if (OutputStage.IsStageInterLockOK() == false)
                //                {
                //                    nRet = OutputStage.LoadingBinComplete();
                //                    if (nRet != 0)
                //                    {
                //                        AxisOutputFeederY.EmgStop();
                //                        PostAlarm((int)AlarmKeys.Alarm_StageLoadingFailed);
                //                        this.State = ProcessState.Error;
                //                        return nRet;
                //                    }
                //                    //if (this.IsStop) { return 0; }
                //                }
                //                return nRet;
                //            }
                //        }
                //    }
                //}
                //catch (Exception ex)
                //{
                //    Log.Write(ex);
                //    NeedUnloadFirst = false;
                //}
            }


            // 0) Stage에 제품이 있으면 "언로딩 먼저"
            if (NeedUnloadFirst || _dryLoadedToStage)
            {
                bool bBinInStage = this.OutputStage.IsRingPresent();
                bool bBinInFeeder = IsRingPresent();
                var BinStage = this.OutputStage.GetMaterialWafer();
                if (bBinInStage) // Stage에 제품이 있을 때만 Stage→Feeder 언로딩 진행
                {
                    if (BinStage == null)
                    {
                        Log.Write(UnitName, "OnRunWork: BinUnloading - wafer is null on OutputStage.");
                    }

                    // Stage 언로딩 준비
                    nRet = this.OutputStage.PrepareOutputStageUnloadingBin();
                    if (nRet != 0)
                    {
                        AxisOutputFeederY.EmgStop();
                        PostAlarm((int)AlarmKeys.Alarm_StageUnloadingFailed);
                        this.State = ProcessState.Error;
                        return nRet;
                    }

                    // Stage → Feeder
                    nRet = UnloadBinStageToFeeder();
                    if (nRet != 0)
                    {
                        AxisOutputFeederY.EmgStop();
                        PostAlarm((int)AlarmKeys.Alarm_StageUnloadingFailed);
                        this.State = ProcessState.Error;
                        return nRet;
                    }

                    nRet = ClampGripper();
                    if (nRet != 0)
                    {
                        AxisOutputFeederY.EmgStop();
                        PostAlarm((int)AlarmKeys.Alarm_StageUnloadingFailed);
                        Log.Write(this, "ClampGripper Failed");
                        return nRet;
                    }

                    // 머티리얼 이동 (Stage → Feeder)
                    var waferFromStage = BinStage;
                    this.OutputStage.MoveMaterial(waferFromStage, this);
                    this.OutputStage.SetMaterial(null);

                    // 언로딩 대상 슬롯 계산
                    var waferFromFeeder = this.GetMaterial() as MaterialWafer;
                    int slotFromStage = (waferFromFeeder != null) ? waferFromFeeder.SlotIndex : -1;
                    int lifterSlot = this.OutputCassetteLifter.GetCurrectSlotID();
                    int nSlot = slotFromStage >= 0 ? slotFromStage : (lifterSlot >= 0 ? lifterSlot : _dryLastSlotIndex);
                    if (nSlot < 0)
                    {
                        AxisOutputFeederY.EmgStop();
                        PostAlarm((int)AlarmKeys.Alarm_BinUnloadingFailed);
                        this.State = ProcessState.Error;
                        Log.Write(this, "BinUnloading - Invalid slot index (no Stage/Lifter/DryRun slot)");
                        return -1;
                    }

                    Log.Write(UnitName, "BinUnloading", $"BinUnloading - MoveToSlot : {nSlot}");
                    nRet = this.OutputCassetteLifter.MoveToSlot(nSlot);
                    if (nRet != 0)
                    {
                        AxisOutputFeederY.EmgStop();
                        PostAlarm((int)AlarmKeys.Alarm_BinUnloadingFailed);
                        this.State = ProcessState.Error;
                        Log.Write(this, "OutputCassetteLifter.MoveToSlot Failed");
                        return nRet;
                    }

                    // Feeder → Cassette만 수행
                    nRet = UnloadOnlyFeederToCassette(true);
                    if (nRet != 0)
                    {
                        AxisOutputFeederY.EmgStop();
                        PostAlarm((int)AlarmKeys.Alarm_BinUnloadingFailed);
                        this.State = ProcessState.Error;
                        return nRet;
                    }
                }
                else if (bBinInFeeder) // Stage는 비어있고 Feeder에만 제품 → Feeder→Cassette만 수행
                {
                    var waferOnFeeder = this.GetMaterial() as MaterialWafer;
                    int slotFromFeeder = (waferOnFeeder != null) ? waferOnFeeder.SlotIndex : -1;
                    int lifterSlot = this.OutputCassetteLifter.GetCurrectSlotID();
                    int nSlot = slotFromFeeder >= 0 ? slotFromFeeder : (lifterSlot >= 0 ? lifterSlot : _dryLastSlotIndex);
                    if (nSlot < 0)
                    {
                        AxisOutputFeederY.EmgStop();
                        PostAlarm((int)AlarmKeys.Alarm_BinUnloadingFailed);
                        this.State = ProcessState.Error;
                        Log.Write(this, "BinUnloading - Invalid slot index (feeder only case)");
                        return -1;
                    }

                    Log.Write(UnitName, "BinUnloading", $"BinUnloading - MoveToSlot : {nSlot}");
                    nRet = this.OutputCassetteLifter.MoveToSlot(nSlot);
                    if (nRet != 0)
                    {
                        AxisOutputFeederY.EmgStop();
                        PostAlarm((int)AlarmKeys.Alarm_BinUnloadingFailed);
                        this.State = ProcessState.Error;
                        Log.Write(this, "OutputCassetteLifter.MoveToSlot Failed");
                        return nRet;
                    }

                    nRet = UnloadOnlyFeederToCassette(true);
                    if (nRet != 0)
                    {
                        AxisOutputFeederY.EmgStop();
                        PostAlarm((int)AlarmKeys.Alarm_BinUnloadingFailed);
                        this.State = ProcessState.Error;
                        return nRet;
                    }
                }

                if (IsStop) { return 0; }

                this.State = ProcessState.Complete;
                return 0;

                //기존 코드
                {
                    // 8) Feeder -> Stage: WaferUnloadingBeforeStage
                    //nRet = BinUnloading(wafer);
                    //if (nRet != 0)
                    //{
                    //    AxisOutputFeederY.EmgStop();
                    //    PostAlarm((int)AlarmKeys.Alarm_BinUnloadingFailed);
                    //    this.State = ProcessState.Error;
                    //    return nRet;
                    //}
                    //if (IsStop) { return 0; }
                }
            }

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
            if (this.IsStop) { return 0; }

            if (this.OutputCassetteLifter.IsHaveMoreProcessWafer())
            {
                // 재시작/이상 종료 후 전 슬롯 완료 여부 1회 알람(필요 시)
                // Output 쪽에도 필요하면 동일 로직 추가 가능

                // 재시작 시 현재 상태로부터 스텝 유추 후 1스텝만 수행
                InitLoadStepIfNeeded();

                switch (_loadStep)
                {
                    case LoadFlowStep.MoveToNextSlot:
                        {
                            nRet = this.OutputCassetteLifter.MoveToNextSlot();
                            if (nRet != 0)
                            {
                                AxisOutputFeederY.EmgStop();
                                PostAlarm((int)AlarmKeys.Alarm_BinLoadingFailed);
                                this.State = ProcessState.Error;
                                return nRet;
                            }
                            _loadStep = LoadFlowStep.PrepareLoading;
                            Log.Write(UnitName, "OnRunWork", "LoadFlowStep.MoveToNextSlot completed.");
                            return 0;
                        }

                    case LoadFlowStep.PrepareLoading:
                        {
                            nRet = OutputStage.LoadingBinPrepare();
                            if (nRet != 0)
                            {
                                AxisOutputFeederY.EmgStop();
                                PostAlarm((int)AlarmKeys.Alarm_OutputStageInterlockFailed);
                                this.State = ProcessState.Error;
                                return nRet;
                            }
                            _loadStep = LoadFlowStep.PickFromCassette;
                            Log.Write(UnitName, "OnRunWork", "LoadFlowStep.PrepareLoading completed.");
                            return 0;
                        }

                    case LoadFlowStep.PickFromCassette:
                        {
                            nRet = BinLoading(); // Barcode 포함
                            if (nRet != 0)
                            {
                                AxisOutputFeederY.EmgStop();
                                PostAlarm((int)AlarmKeys.Alarm_BinLoadingFailed);
                                this.State = ProcessState.Error;
                                return nRet;
                            }
                            _loadStep = LoadFlowStep.LoadToStage;
                            Log.Write(UnitName, "OnRunWork", "LoadFlowStep.PickFromCassette completed.");
                            return 0;
                        }

                    case LoadFlowStep.LoadToStage:
                        {
                            nRet = StageLoading();
                            if (nRet != 0)
                            {
                                AxisOutputFeederY.EmgStop();
                                PostAlarm((int)AlarmKeys.Alarm_StageLoadingFailed);
                                this.State = ProcessState.Error;
                                return nRet;
                            }

                            var waferOnFeeder2 = this.GetMaterial() as MaterialWafer;
                            if (waferOnFeeder2 == null)
                            {
                                AxisOutputFeederY.EmgStop();
                                PostAlarm((int)AlarmKeys.Alarm_BinLoadingFailed);
                                Log.Write(this, "No wafer on Feeder to move to OutputStage");
                                return -1;
                            }

                            this.MoveMaterial(waferOnFeeder2, OutputStage);
                            waferOnFeeder2.ProcessSatate = Material.MaterialProcessSatate.Processing;
                            OutputStage.SetMaterial(waferOnFeeder2);
                            this.SetMaterial(null);

                            _loadStep = LoadFlowStep.FeederToReady;
                            Log.Write(UnitName, "OnRunWork", "LoadFlowStep.LoadToStage completed.");
                            return 0;
                        }

                    case LoadFlowStep.FeederToReady:
                        {
                            nRet = MoveToReady();
                            if (nRet != 0)
                            {
                                AxisOutputFeederY.EmgStop();
                                PostAlarm((int)AlarmKeys.Alarm_BinLoadingFailed);
                                this.State = ProcessState.Error;
                                return nRet;
                            }
                            _loadStep = LoadFlowStep.StageLoadingAfter;
                            Log.Write(UnitName, "OnRunWork", "LoadFlowStep.FeederToReady completed.");
                            return 0;
                        }

                    case LoadFlowStep.StageLoadingAfter:
                        {
                            nRet = OutputStage.LoadingBinComplete();
                            if (nRet != 0)
                            {
                                AxisOutputFeederY.EmgStop();
                                PostAlarm((int)AlarmKeys.Alarm_StageLoadingFailed);
                                this.State = ProcessState.Error;
                                return nRet;
                            }
                            _loadStep = LoadFlowStep.PrepareOutputStage;
                            Log.Write(UnitName, "OnRunWork", "LoadFlowStep.StageLoadingAfter completed.");
                            return 0;
                        }

                    case LoadFlowStep.PrepareOutputStage:
                        {
                            nRet = PreparetoOutputStage();
                            if (nRet != 0)
                            {
                                AxisOutputFeederY.EmgStop();
                                PostAlarm((int)AlarmKeys.Alarm_StageLoadingFailed);
                                this.State = ProcessState.Error;
                                return nRet;
                            }

                            // Path 생성 및 Stage에 material 정리는 StageLoading에서 수행함
                            if (Config.IsUnitDryRun)
                            {
                                _dryLoadedToStage = true; // 다음엔 언로딩
                                _dryLastSlotIndex = this.OutputCassetteLifter.GetCurrectSlotID();
                            }

                            this.State = ProcessState.Complete;
                            _loadStep = LoadFlowStep.None;
                            Log.Write(UnitName, "OnRunWork", "LoadFlowStep.PrepareOutputStage completed.");
                            return 0;
                        }

                    default:
                        {
                            // 처음 진입 시 스텝 설정
                            _loadStep = LoadFlowStep.MoveToNextSlot;
                            Log.Write(UnitName, "OnRunWork", "LoadFlowStep initialized to MoveToNextSlot.");
                            return 0;
                        }
                }
            }
            else
            {
                // 로딩할 웨이퍼가 없으면 Ready 복귀 및 스텝 초기화
                _loadStep = LoadFlowStep.None;

                if (!IsPositionReady())
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

            //기존 코드
            {
                //if (this.OutputCassetteLifter.IsHaveMoreProcessWafer())
                //{
                //    // 2) Feeder -> Cassette: MoveToNextSlot
                //    nRet = this.OutputCassetteLifter.MoveToNextSlot();
                //    if (nRet != 0)
                //    {
                //        AxisOutputFeederY.EmgStop();
                //        PostAlarm((int)AlarmKeys.Alarm_BinLoadingFailed);
                //        this.State = ProcessState.Error;
                //        return nRet;
                //    }
                //    //if (this.IsStop) { return 0; }

                //    // 3) Feeder -> Stage: WaferLoadingBeforeStage
                //    nRet = OutputStage.LoadingBinPrepare();
                //    if (nRet != 0)
                //    {
                //        AxisOutputFeederY.EmgStop();
                //        PostAlarm((int)AlarmKeys.Alarm_OutputStageInterlockFailed);
                //        this.State = ProcessState.Error;
                //        return nRet;
                //    }
                //    //if (this.IsStop) { return 0; }

                //    // 4) Feeder 내부 로딩 Cascette에서 Wafer Pick
                //    nRet = BinLoading(); // 여기서 Barcode Reading 포함
                //    if (nRet != 0)
                //    {
                //        AxisOutputFeederY.EmgStop();
                //        PostAlarm((int)AlarmKeys.Alarm_BinLoadingFailed);
                //        this.State = ProcessState.Error;
                //        return nRet;
                //    }
                //    //if (this.IsStop) { return 0; }

                //    // 4) Feeder 내부 로딩 Stage에 Wafer Load
                //    nRet = StageLoading();
                //    if (nRet != 0)
                //    {
                //        AxisOutputFeederY.EmgStop();
                //        PostAlarm((int)AlarmKeys.Alarm_StageLoadingFailed);
                //        this.State = ProcessState.Error;
                //        return nRet;
                //    }
                //    //if (this.IsStop) { return 0; }

                //    nRet = MoveToReady();
                //    if (nRet != 0)
                //    {
                //        AxisOutputFeederY.EmgStop();
                //        PostAlarm((int)AlarmKeys.Alarm_BinLoadingFailed);
                //        this.State = ProcessState.Error;
                //        return nRet;
                //    }
                //    //if (this.IsStop) { return 0; }

                //    // 5) Feeder -> Stage: WaferLoadingAfterStage
                //    nRet = OutputStage.LoadingBinComplete();
                //    if (nRet != 0)
                //    {
                //        AxisOutputFeederY.EmgStop();
                //        PostAlarm((int)AlarmKeys.Alarm_StageLoadingFailed);
                //        this.State = ProcessState.Error;
                //        return nRet;
                //    }
                //    //if (this.IsStop) { return 0; }

                //    nRet = PreparetoOutputStage();
                //    if (nRet != 0)
                //    {
                //        AxisOutputFeederY.EmgStop();
                //        PostAlarm((int)AlarmKeys.Alarm_StageLoadingFailed);
                //        this.State = ProcessState.Error;
                //        return nRet;
                //    }

                //    MakePath();

                //    var waferOnFeeder = this.GetMaterial() as MaterialWafer;
                //    if (waferOnFeeder != null)
                //    {
                //        // 기존 인스턴스를 Stage로 이동
                //        this.MoveMaterial(waferOnFeeder, OutputStage);

                //        // 가공 상태 설정
                //        waferOnFeeder.ProcessSatate = Material.MaterialProcessSatate.Processing;
                //        OutputStage.SetMaterial(waferOnFeeder);

                //        // Feeder의 material 비우기
                //        this.SetMaterial(null);
                //    }

                //    this.OutputStage.UpdateUI();

                //    //기존코드
                //    {
                //        //this.MoveMaterial(new MaterialWafer(), OutputStage);
                //        //var waferOutputStage = OutputStage.GetMaterialWafer();
                //        ////waferOutputStage.ProcessSatate = Material.MaterialProcessSatate.Ready;
                //        //waferOutputStage.ProcessSatate = Material.MaterialProcessSatate.Processing;
                //        //OutputStage.SetMaterial(waferOutputStage);
                //        //this.OutputStage.UpdateUI();
                //    }


                //    if (Config.IsUnitDryRun)
                //    {
                //        _dryLoadedToStage = true; // 다음엔 언로딩
                //        _dryLastSlotIndex = this.OutputCassetteLifter.GetCurrectSlotID();
                //    }

                //    this.State = ProcessState.Complete;
                //}
                //else
                //{
                //    if(IsPositionReady() == false)
                //    {
                //        nRet = MoveToReady();
                //        if (nRet != 0)
                //        {
                //            AxisOutputFeederY.EmgStop();
                //            PostAlarm((int)AlarmKeys.Alarm_BinLoadingFailed);
                //            this.State = ProcessState.Error;
                //            return nRet;
                //        }
                //    }

                //}
            }

            return nRet;
        }
        protected override int OnRunComplete()
        {
            int ret = 0;
            this.State = ProcessState.Ready;
            return ret;
        }
        #endregion

        public int MakePath()
        {
            int nRet = 0;
            MaterialWafer wafer = this.GetMaterial() as MaterialWafer;
            if (wafer != null)
            {
                // Ready 또는 Processing 이고, 아직 경로가 없을 때만 생성
                bool needPath = (wafer.Dies == null || wafer.Dies.Count == 0);
                if ((wafer.ProcessSatate == Material.MaterialProcessSatate.Ready
                     || wafer.ProcessSatate == Material.MaterialProcessSatate.Processing)
                    && needPath)
                {
                    string measName = null;
                    if (wafer.Dies != null)
                    {
                        wafer.Dies.Clear();
                    }

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

        protected override void OnMakeSequence()
        {
            base.OnMakeSequence();
            this.SequencePlayers.Add(BinLoading);
            this.SequencePlayers.Add(StageLoading);
            this.SequencePlayers.Add(MoveToReady);
            this.SequencePlayers.Add(BinUnloading);
        }

        #region Seq 단위 동작 함수
        // 로딩 플로우 체크포인트
        private enum LoadFlowStep
        {
            None = 0,
            MoveToNextSlot,
            PrepareLoading,
            PickFromCassette,
            LoadToStage,
            FeederToReady,
            StageLoadingAfter,
            PrepareOutputStage,
        }
        private LoadFlowStep _loadStep = LoadFlowStep.None;

        // 언로드 직후 다음 로딩을 바코드에서 시작하도록 하는 1회성 플래그
        private volatile bool _exchangeStandbyForNextLoad = false;

        // 현재 설비 상태를 기준으로 첫 스텝 유추
        private void InitLoadStepIfNeeded()
        {
            if (_loadStep != LoadFlowStep.None)
                return;

            bool feederHasWafer = this.GetMaterial() is MaterialWafer;
            bool atCassette = IsPositionCassette();
            bool atStage = IsPositionStage();
            bool feederDown = IsFeederDown();
            bool unclamped = IsUnClamped();

            // A) 이미 픽 완료(피더에 웨이퍼 있음) → Stage 로딩부터
            if (feederHasWafer)
            {
                _loadStep = LoadFlowStep.LoadToStage;
                return;
            }

            // B) 방금 Stage에 내려놓고 중단(피더 비어있음 + Stage 위치 + Down + Unclamp) → Ready 복귀부터
            if (!feederHasWafer && atStage && feederDown && unclamped)
            {
                _loadStep = LoadFlowStep.FeederToReady;
                return;
            }

            // C) Cassette 앞 Down+Unclamp → 픽부터
            if (atCassette && feederDown && unclamped)
            {
                _loadStep = LoadFlowStep.PickFromCassette;
                return;
            }

            // D) 기본 → 다음 슬롯 이동부터
            _loadStep = LoadFlowStep.MoveToNextSlot;
        }



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

            Log.Write(UnitName, "BinLoading Start");
            if (IsMoveInterLockCassette() == false)
            {
                Log.Write(UnitName, "Not IsMoveInterLockCassette");
                return -1;
            }

            // === Exchange 대기 전략 ===
            // - 언로드 직후 또는 이미 바코드에 있으면 Ready 이동 스킵
            bool preferBarcode = _exchangeStandbyForNextLoad || IsPositionBarcode();
            if (preferBarcode)
            {
                if (!IsPositionBarcode())
                {
                    nRet = MovePositionBarcode(isFine);
                    if (nRet != 0)
                    {
                        Log.Write(UnitName, "BinLoading Fail - MovePositionBarcode");
                        return nRet;
                    }
                }
                Log.Write(UnitName, "BinLoading", "[Exchange] Standby at Barcode → skip MoveToReady");
                _exchangeStandbyForNextLoad = false; // 1회 사용
            }
            else
            {
                // 이미 Ready면 스킵
                if (!IsPositionReady())
                {
                    nRet = MoveToReady(isFine);
                    if (nRet != 0)
                    {
                        Log.Write(UnitName, "BinLoading Failed - MoveToReady");
                        return nRet;
                    }
                }
                else
                {
                    Log.Write(UnitName, "BinLoading", "[Skip] Already at Ready");
                }
            }

            //기존코드
            {
                //if (NeedUnloadFirst)
                //{
                //    Thread.Sleep(500);
                //    if (IsPositionBarcode() == false)
                //    {
                //        if (IsPositionBarcode() == false)
                //        {
                //            Log.Write(UnitName, "WaferLoading - MovePositionBarcode First");
                //            return -1;
                //        }
                //    }
                //}
                //else
                //{
                //    if (IsPositionReady() == false)
                //    {
                //        nRet = MoveToReady(isFine);
                //        if (nRet != 0)
                //        {
                //            Log.Write(UnitName, "BinLoading Failed - MoveToReady");
                //            return nRet;
                //        }
                //    }
                //}
            }


            nRet = UnClampGripper();
            if (nRet != 0)
            {
                Log.Write(UnitName, "UnClampGripper Failed");
                return nRet;
            }

            nRet = DownFeeder();
            if (nRet != 0)
            {
                Log.Write(UnitName, "DownFeeder Failed");
                return nRet;
            }

            nRet = MoveToCassette(isFine);
            if (nRet != 0)
            {
                Log.Write(UnitName, "MoveToCassette Failed");
                return nRet;
            }

            nRet = BarcodeReading(isFine);
            if (nRet != 0)
            {
                Log.Write(UnitName, "BarcodeReading Failed");
                return nRet;
            }
            var c = this.OutputCassetteLifter.GetMaterialCassette();
            int nIndex = this.OutputCassetteLifter.GetCurrectSlotID();
            MaterialWafer wafer = c.GetWafer(nIndex);

            // 피더에 웨이퍼 세팅
            this.SetMaterial(wafer);

            // 픽업 직후 재선택 방지: Processing 전환 + SlotIndex 보정 + 경로 준비
            if (wafer != null)
            {
                wafer.Presence = Material.MaterialPresence.Exist;
                if (wafer.SlotIndex < 0) wafer.SlotIndex = nIndex; // 유실 대비 보정
                wafer.ProcessSatate = Material.MaterialProcessSatate.Processing;

                // 경로가 없으면 즉시 생성
                if (wafer.Dies == null || wafer.Dies.Count == 0)
                {
                    MakePath();
                }
            }

            Log.Write(UnitName, "BinLoading Complete");
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

            //int nSlot = wafer.SlotIndex;
            // 안전한 언로딩 슬롯 산출: Stage wafer.SlotIndex → 없으면 Lifter 현재 슬롯 → DryRun 마지막 슬롯
            int slotFromStage = (wafer != null) ? wafer.SlotIndex : -1;
            int lifterSlot = this.OutputCassetteLifter.GetCurrectSlotID();
            int nSlot = slotFromStage >= 0 ? slotFromStage
                        : (lifterSlot >= 0 ? lifterSlot : _dryLastSlotIndex);
            if (nSlot < 0)
            {
                AxisOutputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_BinUnloadingFailed);
                Log.Write(this, "BinUnloading - Invalid slot index (all sources invalid)");
                return -1;
            }

            Log.Write(UnitName, "BinUnloading", $"BinUnloading - MoveToSlot : {nSlot}");
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

        private int UnloadOnlyFeederToCassette(bool isFine = false)
        {
            int nRet = 0;

            nRet = MovePositionCassette(isFine);
            if (nRet != 0)
            {
                AxisOutputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_StageUnloadingFailed);
                Log.Write(this, "UnloadOnlyFeederToCassette Fail - MovePositionCassette");
                return -1;
            }

            nRet = UnClampGripper();
            if (nRet != 0)
            {
                AxisOutputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_StageUnloadingFailed);
                Log.Write(this, "UnloadOnlyFeederToCassette Fail - UnClampGripper");
                return -1;
            }

            // Feeder -> Cassette: 데이터 돌려놓기
            var waferOnFeeder = this.GetMaterial() as MaterialWafer;
            if (waferOnFeeder != null && waferOnFeeder.SlotIndex >= 0)
            {
                var cassette = this.OutputCassetteLifter.GetMaterialCassette();
                waferOnFeeder.ProcessSatate = Material.MaterialProcessSatate.Completed;
                waferOnFeeder.Presence = Material.MaterialPresence.Exist;
                cassette.SetWafer(waferOnFeeder.SlotIndex, waferOnFeeder);
            }
            else
            {
                Log.Write(this, "UnloadOnly: Feeder has no wafer or invalid SlotIndex");
            }

            // 회피 = 바코드 위치 대기
            nRet = MovePositionBarcode(isFine);
            if (nRet != 0)
            {
                AxisOutputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_StageUnloadingFailed);
                Log.Write(this, "UnloadOnlyFeederToCassette Fail - MovePositionBarcode");
                return -1;
            }

            // 다음 로딩은 바코드에서 시작(1회)
            _exchangeStandbyForNextLoad = true;

            // Feeder material 정리
            this.SetMaterial(null);

            return 0;
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

            // Stage의 실제 웨이퍼를 가져와 그대로 Feeder로 이동
            var waferFromStage = this.OutputStage.GetMaterialWafer();
            if (waferFromStage == null)
            {
                AxisOutputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_StageUnloadingFailed);
                Log.Write(this, "No wafer on OutputStage to move to Feeder");
                return -1;
            }
            this.OutputStage.MoveMaterial(waferFromStage, this);
            this.OutputStage.SetMaterial(null);

            //기존코드
            {
                //MaterialWafer wafer = new MaterialWafer();
                //this.OutputStage.MoveMaterial(wafer, this);
            }
            
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

            // 피더 -> 카세트: 웨이퍼 정보 되돌려 넣기
            var waferOnFeeder = this.GetMaterial() as MaterialWafer;
            if (waferOnFeeder != null && waferOnFeeder.SlotIndex >= 0)
            {
                var cassette = this.OutputCassetteLifter.GetMaterialCassette();
                waferOnFeeder.ProcessSatate = Material.MaterialProcessSatate.Completed;
                waferOnFeeder.Presence = Material.MaterialPresence.Exist;
                cassette.SetWafer(waferOnFeeder.SlotIndex, waferOnFeeder);
            }
            else
            {
                Log.Write(this, "Unload: Feeder has no wafer or invalid SlotIndex");
            }

            // 회피=바코드 위치로 대기
            nRet = MovePositionBarcode(isFine);
            if (nRet != 0)
            {
                AxisOutputFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_StageUnloadingFailed);
                Log.Write(this, "MovePositionBarcode Failed");
                nRet = -1;
                return nRet;
            }

            // 다음 로딩은 바코드에서 시작(1회)
            _exchangeStandbyForNextLoad = true;

            // Feeder의 material 정리 (배출 완료 후 비움)
            this.SetMaterial(null);

            //기존코드
            {
                //wafer = new MaterialWafer();
                //MoveMaterial(wafer, null);
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
               
            var tp = TeachingPositions[(int)OutputFeederConfig.TeachingPositionName.Cassette];
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
                    Log.Write(this, "CheckReady Fail - OutputStage.IsStageInterLockOK");
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
                    Log.Write(this, "CheckReady Fail - OutputStage.IsStageInterLockOK");
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
