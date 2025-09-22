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
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;

namespace QMC.LCP_280.Process.Unit
{
    /// <summary>
    /// InputFeeder (Wafer Feeder / Ring Transfer Unit)
    ///  - X ?? ??? + Lift + Clamp (Ring ???? ??? / Overload ???)
    ///  - Teaching Position ???? (InputFeederConfig)
    ///  - Cylinder ??? ???? API (FeederUp/Down, Clamp)
    ///  - OutputStage / InputStage ?? ?????? Region/???? ????
    /// </summary>
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

        }
        #endregion

        #region Config / Teaching

        #endregion

        #region Unit
        public InputCassetteLifter InputCassetteLifter { get; set; }
        public InputStage InputStage { get; set; }
        #endregion

        #region Axes
        private MotionAxis _feederY;
        public MotionAxis FeederY => _feederY;
        #endregion

        #region IO Domain Members
        private Cylinder _feederLift; // Up/Down
        private Cylinder _cylClamp;   // Clamp/Unclamp
        #endregion

        #region Status Signals
        // Status Signals
        public bool IsRequestLoadingWafer { get; private set; }
        public bool IsRequestUnloadingWafer { get; private set; }
        public bool IsWaferLoadDone { get; private set; }
        public bool IsWaferUnloadDone { get; private set; }
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
        #endregion

        protected override void OnBindUnit()
        {
            base.OnBindUnit();
            InputCassetteLifter = Equipment.Instance.GetUnit("InputCassetteLifter") as InputCassetteLifter;
            InputStage = Equipment.Instance.GetUnit("InputStage") as InputStage;
        }

        #region Axis Binding
        private void BindAxes()
        {
            var mgr = Equipment.Instance?.AxisManager;
            if (mgr == null)
            {
                Log.Write("InputFeeder", "[BindAxes] AxisManager null");
                return;
            }

            const string unitName = "Unit"; // Equipment???? ?? ??? ?? ????? ?????? ??????? ??
            BindAxis(mgr, unitName, AxisNames.WaferFeederY, ref _feederY);
        }
        #endregion

        public int MovePositionReady(bool isFine = false)
        {
            Task<int> task = MovePositionAsyncReady(isFine);
            while (IsEndTask(task) == false)
            {
                if (IsInterlockOKWaferLoading() == false)
                {
                    foreach (var ax in Axes.Values)
                    {
                        ax.EmgStop();
                    }
                    PostAlarm((int)AlarmKeys.Alarm_WaferLoadingFailed);
                    return -1;
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
            return MoveTeachingPositionOnce((int)InputFeederConfig.TeachingPositionName.Ready, isFine);
        }
        private int IsMoveInterLockReady()
        {
            int nRet = 0;
            // Check Interlock.!!! 구문 넣을것.!!!
            if(!IsFeederUp())
            {
                PostAlarm((int)AlarmKeys.Alarm_FeederClampUp);
                nRet = -1;
                return nRet;
            }

            if(!InputStage.IsAnyAxisMoving())
            {
                PostAlarm((int)AlarmKeys.Alarm_InputStageInterlockFailed);
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
            return MoveTeachingPositionOnce((int)InputFeederConfig.TeachingPositionName.Stage, isFine);
        }
        private int IsMoveInterLockStage()
        {
            int nRet = 0;
            // Check Interlock.!!! 구문 넣을것.!!!
            if (!IsFeederUp())
            {
                PostAlarm((int)AlarmKeys.Alarm_FeederClampUp);
                nRet = -1;
                return nRet;
            }

            if (!InputStage.IsAnyAxisMoving())
            {
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
            if (!IsFeederUp())
            {
                PostAlarm((int)AlarmKeys.Alarm_FeederClampUp);
                nRet = -1;
                return nRet;
            }

            if (!InputStage.IsAnyAxisMoving())
            {
                PostAlarm((int)AlarmKeys.Alarm_InputStageInterlockFailed);
                nRet = -1;
                return nRet;
            }

            if(!InputCassetteLifter.IsAnyAxisMoving())
            {
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
                    foreach (var ax in Axes.Values)
                    {
                        ax.EmgStop();
                    }
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
        private int IsMoveInterLockCassette()
        {
            int nRet = 0;
            // Check Interlock.!!! 구문 넣을것.!!!
            if (!IsFeederUp())
            {
                PostAlarm((int)AlarmKeys.Alarm_FeederClampUp);
                nRet = -1;
                return nRet;
            }

            if (!InputStage.IsAnyAxisMoving())
            {
                PostAlarm((int)AlarmKeys.Alarm_InputStageInterlockFailed);
                nRet = -1;
                return nRet;
            }

            if (!InputCassetteLifter.IsAnyAxisMoving())
            {
                PostAlarm((int)AlarmKeys.Alarm_InputStageInterlockFailed);
                nRet = -1;
                return nRet;
            }

            return nRet;
        }




        public void MoveAxisOnce(MotionAxis ax, double target)
        {
            if (ax == null) return;
            if (System.Math.Abs(ax.GetPosition() - target) > ax.Config.InposTolerance * 3)
                ax.MoveAbs(target, ax.Config.MaxVelocity, ax.Config.RunAcc, ax.Config.RunDec, ax.Config.AccJerkPercent);
        }
        public bool InPos(MotionAxis ax, double target) => ax == null || ax.InPosition(target);
        public double GetTP(string tpName, string axisName)
        {
            var tp = Config.GetTeachingPosition(tpName);
            if (tp != null && tp.AxisPositions != null && tp.AxisPositions.TryGetValue(axisName, out var v)) return v;
            return 0.0;
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

        public bool InPosTeaching(string positionName)
        {
            var tp = Config.GetTeachingPosition(positionName);
            if (tp == null) return false;
            foreach (var kv in tp.AxisPositions)
                if (!Axes.TryGetValue(kv.Key, out var axis) || !InPos(axis, kv.Value)) return false;
            return true;
        }
        #endregion

        /// <summary>
        /// Feeder Y 축이 안전(Safety) 위치에 있는지 확인.
        /// 기본 탐색 Teaching 후보: "SafetyPos" → "Safety" → "Safe" → "Ready"
        /// - 후보 중 첫 번째로 존재하고 FeederY 좌표가 포함된 Teaching 사용
        /// - 축/Teaching 미존재 시 treatMissingAsSafe 옵션에 따라 반환
        /// - allowPositiveBeyond = true 이면 목표 위치 이상(+방향)도 안전으로 허용
        /// </summary>
        /// <param name="fallbackTolerance">축 InposTolerance 미설정 시 사용할 기본 허용 오차</param>
        /// <param name="useAxisInposTolerance">축 Config.InposTolerance 사용 여부</param>
        /// <param name="treatMissingAsSafe">Teaching 또는 축이 없을 때 true 로 간주할지 여부</param>
        /// <param name="allowPositiveBeyond">
        /// true: FeederY 현재 위치가 안전 목표 이상(+방향)일 경우도 OK (Cassette 접근 위험이 주로 -방향일 때 사용)
        /// false: 목표 위치 근처(± tolerance)에서만 OK
        /// </param>
        /// <param name="customCandidates">사용자 정의 Teaching 우선순위 (null 이면 기본 후보 사용)</param>
        public bool IsFeederYSafetyPosition(double fallbackTolerance = 0.01,
                                            bool useAxisInposTolerance = true,
                                            bool treatMissingAsSafe = true,
                                            bool allowPositiveBeyond = true,
                                            IEnumerable<string> customCandidates = null)
        {
            if (FeederY == null)
                return treatMissingAsSafe;

            var cfg = Config;
            if (cfg == null)
                return treatMissingAsSafe;

            // 기본 후보 목록
            var defaultCandidates = new[] { "SafetyPos", "Safety", "Safe", "Ready" };
            var candidates = (customCandidates == null ? defaultCandidates : customCandidates)
                             .Where(s => !string.IsNullOrWhiteSpace(s));

            string axisKey = AxisNames.WaferFeederY;
            string selectedTpName = null;
            TeachingPosition selectedTp = null;

            foreach (var name in candidates)
            {
                var tp = cfg.GetTeachingPosition(name);
                if (tp == null) continue;

                // Teaching 에 해당 축 좌표가 실재 포함되는지 확인
                if (tp.AxisPositions != null &&
                    tp.AxisPositions.Keys.Any(k =>
                        string.Equals(k, axisKey, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(k, FeederY.Name, StringComparison.OrdinalIgnoreCase)))
                {
                    selectedTpName = name;
                    selectedTp = tp;
                    break;
                }
            }

            if (selectedTp == null)
                return treatMissingAsSafe;

            // 목표 좌표 가져오기 (AxisPositions 사전에서 직접 조회)
            double target;
            if (!selectedTp.AxisPositions.TryGetValue(axisKey, out target))
            {
                // Axis 이름으로 재시도
                if (!selectedTp.AxisPositions.TryGetValue(FeederY.Name, out target))
                    return treatMissingAsSafe; // 좌표가 없으면 안전 판단 불가 → 기본 정책대로
            }

            double cur = FeederY.GetPosition();
            double tol = useAxisInposTolerance
                ? (FeederY.Config?.InposTolerance ?? fallbackTolerance)
                : fallbackTolerance;

            if (allowPositiveBeyond)
            {
                // 목표 이상(+방향) 허용
                if (cur >= target - tol) return true;
                return false;
            }
            else
            {
                // 목표 근처만 허용
                return System.Math.Abs(cur - target) <= tol;
            }
        }

        /// <summary>
        /// Feeder Z(=Lift) 축이 안전(SAFE) 위치인지 판단.
        /// 안전 기준: Lift Up 센서 ON.
        /// - Down 센서 ON 이면 false
        /// - Up/Down 모두 OFF(이행 중 또는 센서 이상) 이면 false
        /// - Lift 객체 자체가 없으면 treatMissingAsSafe 플래그에 따름
        /// </summary>
        /// <param name="treatMissingAsSafe">Lift 미바인딩 시 true 로 처리할지 여부</param>
        public bool IsFeederZSafetyPosition(bool treatMissingAsSafe = true)
        {
            if (_feederLift == null)
                return treatMissingAsSafe;

            if (IsFeederUp())
                return true;

            if (IsFeederDown())
                return false;

            // 전이 상태(Up/Down 모두 OFF) → 안전 아님으로 판단
            return false;
        }


        #region Low-Level IO (Read/Write by Name)
        public bool ReadInput(string name)
        {
            var hi = Config.HardInputs.FirstOrDefault(i => i.Name.Equals(name, System.StringComparison.OrdinalIgnoreCase));
            if (hi == null) return false;
            var eq = Equipment.Instance; var dio = eq?.DioScan; if (dio == null) return false;
            foreach (var m in eq.UnitIO.Modules)
                if (dio.TryGetInput(m.ModuleName, hi.Disp, out var v)) return v;
            return false;
        }
        public bool WriteOutput(string name, bool on)
        {
            var ho = Config.HardOutputs.FirstOrDefault(o => o.Name.Equals(name, System.StringComparison.OrdinalIgnoreCase));
            if (ho == null) return false;
            var eq = Equipment.Instance; var dio = eq?.DioScan; if (dio == null) return false;
            foreach (var m in eq.UnitIO.Modules)
                if (dio.WriteOutput(m.ModuleName, ho.Disp, on) == 0) return true;
            return false;
        }
        public bool IsOutputOn(string name)
        {
            var ho = Config.HardOutputs.FirstOrDefault(o => o.Name.Equals(name, System.StringComparison.OrdinalIgnoreCase));
            if (ho == null) return false;
            var eq = Equipment.Instance; var dio = eq?.DioScan; if (dio == null) return false;
            foreach (var m in eq.UnitIO.Modules)
                if (dio.TryGetOutput(m.ModuleName, ho.Disp, out var v)) return v;
            return false;
        }
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
        }
        #endregion

        // === Domain Control (??? ????) ===
        public bool SetLift(bool bUpDn)
        {
            if (_feederLift == null) return false;
            if (bUpDn) return _feederLift.Extend();
            else return _feederLift.Retract();
        }
        public bool SetClamp(bool bUpDn)
        {
            if (_cylClamp == null) return false;
            if (bUpDn) return _cylClamp.Extend();
            else return _cylClamp.Retract();
        }
        #region Status Helpers
        public bool IsFeederUp()
        {
            if(Config.IsSimulation)
            {
                return true;
            }
            return ReadInput(InputFeederConfig.IO.FEEDER_UP);
        }
        
        public bool IsFeederDown() => ReadInput(InputFeederConfig.IO.FEEDER_DOWN);
        public bool IsUnclamped() => ReadInput(InputFeederConfig.IO.FEEDER_UNCLAMP);
        public bool IsRingPresent() => ReadInput(InputFeederConfig.IO.FEEDER_RING_CHECK);
        public bool IsOverload() => ReadInput(InputFeederConfig.IO.FEEDER_OVERLOAD);
        #endregion

        #region === Direct Valve Control (??? ???/????? ???? ???? ??????) ===
        public bool IsFeederUpValveOn() => IsOutputOn(InputFeederConfig.IO.FEEDER_UP_VALVE);
        public bool IsFeederDownValveOn() => IsOutputOn(InputFeederConfig.IO.FEEDER_DOWN_VALVE);
        public bool IsFeederClampValveOn() => IsOutputOn(InputFeederConfig.IO.FEEDER_CLAMP_VALVE);
        public bool IsFeederUnclampValveOn() => IsOutputOn(InputFeederConfig.IO.FEEDER_UNCLAMP_VALVE);
        #endregion


        /// ////////////////////////////////////////////////////////////////
        public override int OnRun()
        {
            int ret = 0;

            if (this.Status == UnitRunStatus.Stop || this.Status == UnitRunStatus.CycleStop)
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
        protected override  int OnRunReady()
        {
            int ret = 0;
            if (this.InputStage.IsStatus_RequestWafer && this.InputCassetteLifter.IsWaferReadyForUnloding)
            {
                this.State = ProcessState.Work;
            }
            else if (this.InputStage.IsStatus_CompleteWorking)
            {
                this.State = ProcessState.Complete;
            }
            return ret;
        }
        protected override int OnRunWork()
        {
            int ret = 0;
            //1. Wafer Loading
            ret = WaferLoading();
            if (ret != 0) 
            { 
                PostAlarm((int)AlarmKeys.Alarm_WaferLoadingFailed);
                this.State = ProcessState.Error; return ret; 
            }
            
            //3. Stage Loading
            ret = StageLoading();
            
            if (ret != 0) 
            { 
                PostAlarm((int)AlarmKeys.Alarm_StageLoadingFailed);
                this.State = ProcessState.Error; return ret;
            }
            //4. Stage Unloading
            ret = StageUnloading();
            if (ret != 0) 
            { 
                PostAlarm((int)AlarmKeys.Alarm_StageUnloadingFailed);
                this.State = ProcessState.Error; return ret;
            }
            //5. Wafer Unloading
            ret = WaferUnloading();
            
            if (ret != 0) 
            { 
                PostAlarm((int)AlarmKeys.Alarm_WaferUnloadingFailed);
                this.State = ProcessState.Error; return ret;
            }
            this.State = ProcessState.Complete;
            return ret;
        }
        public override int OnStop() 
        { 
            int ret = 0; 
            base.OnStop(); 
            return ret; 
        }

        //protected override void OnMakeSequence()
        //{
        //    base.OnMakeSequence();
        //    this.SequencePlayers.Add(WaferLoading);
        //}

        #region Seq 단위 동작
        public int WaferLoading(bool isFine = false)
        {
            int nRet = -1;
            nRet = MoveToReay(isFine);
            if (nRet != 0)
            {
                return nRet;
            }

            nRet = UnClampGripper();
            if (nRet != 0)
            {
                return nRet;
            }

            nRet = DownFeeder();
            if(nRet != 0)
            {
                return nRet;
            }

            nRet = MoveToCassette(isFine);
            if (nRet != 0)
            {
                return nRet;
            }

            nRet = BarcodeReading(isFine);
            if (nRet != 0)
            {
                return nRet;
            }

            nRet = StageLoading(isFine);
            if (nRet != 0)
            {
                return nRet;
            }

            nRet = MoveToReay(isFine);
            if (nRet != 0)
            {
                return nRet;
            }

            nRet = UpFeeder();
            if (nRet != 0)
            {
                return nRet;
            }

            return nRet;
        }
        public int WaferUnloading(bool isFine = false)
        {
            int nRet = -1;

            nRet = StageUnloading(isFine);
            if (nRet != 0)
            {
                PostAlarm((int)AlarmKeys.Alarm_StageUnloadingFailed);
                nRet = -1;
                return nRet;
            }

            nRet = ClampGripper();
            if (nRet != 0)
            {
                PostAlarm((int)AlarmKeys.Alarm_StageUnloadingFailed);
                nRet = -1;
                return nRet;
            }

            nRet = MovePositionCassette(isFine);
            if (nRet != 0)
            {
                PostAlarm((int)AlarmKeys.Alarm_StageUnloadingFailed);
                nRet = -1;
                return nRet;
            }

            nRet = UnClampGripper();
            if (nRet != 0)
            {
                PostAlarm((int)AlarmKeys.Alarm_StageUnloadingFailed);
                nRet = -1;
                return nRet;
            }

            //회피 Position으로 사용.
            nRet = MovePositionBarcode(isFine);
            if (nRet != 0)
            {
                PostAlarm((int)AlarmKeys.Alarm_StageUnloadingFailed);
                nRet = -1;
                return nRet;
            }

            return nRet;
        }



        // 단위에 단위.
        public int ClampGripper()
        {
            int nRet = 0;
            if (this.SetClamp(true))
            {
                Log.Write("InputFeeder", "WaferLoading", "Clamp Success");
            }
            else
            {
                Log.Write("InputFeeder", "WaferLoading", "Clamp Failed");
                PostAlarm((int)AlarmKeys.Alarm_GripperClampFailed);
                nRet = -1;
                return nRet;
            }
            return nRet;
        }
        public int UnClampGripper()
        {
            int nRet = 0;
            if (this.SetClamp(false))
            {
                Log.Write("InputFeeder", "WaferLoading", "Unclamp Success");
            }
            else
            {
                Log.Write("InputFeeder", "WaferLoading", "Unclamp Failed");
                PostAlarm((int)AlarmKeys.Alarm_GripperClampFailed);
                nRet = -1;
                return nRet;
            }

            return nRet;
        }
        public int UpFeeder()
        {
            int nRet = 0;
            if (this.SetLift(true))
            {
                Log.Write("InputFeeder", "WaferLoading", "Feeder Up Success");
            }
            else
            {
                Log.Write("InputFeeder", "WaferLoading", "Feeder Up Failed");
                PostAlarm((int)AlarmKeys.Alarm_WaferLoadingFailed);
                nRet = -1;
                return nRet;
            }
            return nRet;
        }
        public int DownFeeder()
        {
            int nRet = 0;
            if (this.SetLift(false))
            {
                Log.Write("InputFeeder", "WaferLoading", "Feeder Down Success");
            }
            else
            {
                Log.Write("InputFeeder", "WaferLoading", "Feeder Down Failed");
                PostAlarm((int)AlarmKeys.Alarm_WaferLoadingFailed);
                nRet = -1;
                return nRet;
            }
            return nRet;
        }
        public int MoveToReay(bool isFine = false)
        {
            int nRet = 0;

            nRet = MovePositionReady(isFine);
            if (nRet != 0)
            {
                PostAlarm((int)AlarmKeys.Alarm_WaferLoadingFailed);
                nRet = -1;
                return nRet;
            }

            return nRet;
        }
        
        public int MoveToCassette(bool isFine = false)
        {
            int nRet = 0;
            nRet = MovePositionCassette(isFine);
            if (nRet != 0)
            {
                PostAlarm((int)AlarmKeys.Alarm_WaferLoadingFailed);
                nRet = -1;
                return nRet;
            }

            nRet = ClampGripper();
            if (nRet != 0)
            {
                PostAlarm((int)AlarmKeys.Alarm_WaferLoadingFailed);
                nRet = -1;
                return nRet;
            }

            return nRet;
        }
        private bool IsInterlockOKMoveToCassette()
        {
            bool isOK = this.InputStage.IsWaferLoadingPosition();
            isOK &= this.InputCassetteLifter.IsWaferReadyForLoading();
            return true;
        }
        protected int OnMoveToCassette(bool isFine)
        {
            int nRet = 0;
            if (IsInterlockOKWaferLoading() == false)
            {
                PostAlarm((int)AlarmKeys.Alarm_WaferLoadingFailed);
                nRet = -1;
                return nRet;
            }
            nRet = base.MoveTeachingPositionOnce((int)InputFeederConfig.TeachingPositionName.Cassette, isFine);
            if (nRet != 0)
            {
                PostAlarm((int)AlarmKeys.Alarm_WaferLoadingFailed);
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
            if(!InputCassetteLifter.IsWaferReadyForLoading())
            {
                Log.Write(this, "InputCassetteLifter Not Ready for Loading");
                bRtn = false;
                return bRtn;
            }

            if(!InputStage.IsWaferLoadingPosition())
            {
                Log.Write(this, "InputStage Not Ready for Loading");
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
                PostAlarm((int)AlarmKeys.Alarm_BarcodeReadingFailed);
                nRet = -1;
                return nRet;
            }

            // Barcode Reading Logic
            bool isRead = true; // TODO: Barcode Reading Logic
            // isRead = BarcodeReader.Read(...);
            if (!isRead)
            {
                PostAlarm((int)AlarmKeys.Alarm_BarcodeReadingFailed);
                nRet = -1;
                return nRet;
            }

            return nRet;
        }
        public int StageLoading(bool isFine = false)
        {
            int nRet = 0;
            
            nRet = MovePositionStage(isFine);
            if (nRet != 0)
            {
                PostAlarm((int)AlarmKeys.Alarm_StageLoadingFailed);
                nRet = -1;
                return nRet;
            }

            nRet = UnClampGripper();
            if (nRet != 0)
            {
                PostAlarm((int)AlarmKeys.Alarm_StageLoadingFailed);
                nRet = -1;
                return nRet;
            }

            return nRet;
        }
        public int StageUnloading(bool isFine = false)
        {
            int nRet = -1;

            nRet = UnClampGripper();
            if (nRet != 0)
            {
                PostAlarm((int)AlarmKeys.Alarm_StageUnloadingFailed);
                nRet = -1;
                return nRet;
            }

            nRet = DownFeeder();
            if (nRet != 0)
            {
                PostAlarm((int)AlarmKeys.Alarm_StageUnloadingFailed);
                nRet = -1;
                return nRet;
            }

            nRet = MovePositionStage(isFine);
            if (nRet != 0)
            {
                PostAlarm((int)AlarmKeys.Alarm_StageUnloadingFailed);
                nRet = -1;
                return nRet;
            }

            return nRet;
        }
        
        #endregion

    }
}