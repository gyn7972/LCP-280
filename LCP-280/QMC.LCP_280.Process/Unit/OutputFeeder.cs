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
        }
        #endregion

        #region Unit
        public OutputCassetteLifter OutputCassetteLifter { get; set; }
        public OutputStage OutputStage { get; set; }
        #endregion

        #region Axis
        private MotionAxis _feederY;
        public MotionAxis AxisFeederY => _feederY;
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
            if (!OutputCassetteLifter.IsBinReadyForLoading())
            {
                Log.Write(this, "OutputCassetteLifter Not Ready for Loading");
                bRtn = false;
                return bRtn;
            }

            if (!OutputStage.IsBinLoadingPosition())
            {
                Log.Write(this, "OutputStage Not Ready for Loading");
                bRtn = false;
                return bRtn;
            }
            return bRtn;
        }
        private bool IsInterlockOKMoveToCassette()
        {
            bool isOK = this.OutputStage.IsBinLoadingPosition();
            isOK &= this.OutputCassetteLifter.IsBinReadyForLoading();
            return true;
        }

        public int MovePositionReady(bool isFine = false)
        {
            Task<int> task = MovePositionAsyncReady(isFine);
            while (IsEndTask(task) == false)
            {
                if (IsInterlockOKBinLoading() == false)
                {
                    AxisFeederY.EmgStop();
                    PostAlarm((int)AlarmKeys.Alarm_BinLoadingFailed);
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
            return MoveTeachingPositionOnce((int)OutputFeederConfig.TeachingPositionName.Ready, isFine);
        }
        private int IsMoveInterLockReady()
        {
            int nRet = 0;
            // Check Interlock.!!! 구문 넣을것.!!!
            if (!IsFeederUp())
            {
                AxisFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_FeederClampUp);
                nRet = -1;
                return nRet;
            }

            if (OutputStage.IsAnyAxisMoving())
            {
                AxisFeederY.EmgStop();
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
            if (!IsFeederUp())
            {
                AxisFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_FeederClampUp);
                nRet = -1;
                return nRet;
            }

            if (OutputStage.IsAnyAxisMoving())
            {
                AxisFeederY.EmgStop();
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
            // Check Interlock.!!! 구문 넣을것.!!!
            if (!IsFeederUp())
            {
                AxisFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_FeederClampUp);
                nRet = -1;
                return nRet;
            }

            if (OutputStage.IsAnyAxisMoving())
            {
                AxisFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_OutputStageInterlockFailed);
                nRet = -1;
                return nRet;
            }

            if (OutputCassetteLifter.IsAnyAxisMoving())
            {
                AxisFeederY.EmgStop();
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
                    AxisFeederY.EmgStop();
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
            // Check Interlock.!!! 구문 넣을것.!!!
            if (IsFeederUp() == false)
            {
                bRet = false;
                return bRet;
            }

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

            if (OutputStage.IsBinLoadingPosition() == false)
            {
                bRet = false;
                return bRet;
            }

            if (OutputStage.IsBinUnloadingPosition() == false)
            {
                bRet = false;
                return bRet;
            }

            return bRet;
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
        public double GetTP(string tpName, string axisName)
        {
            var tp = Config.GetTeachingPosition(tpName);
            if (tp != null && tp.AxisPositions != null && tp.AxisPositions.TryGetValue(axisName, out var v)) return v;
            return 0.0;
        }
        public bool InPos(MotionAxis ax, double target) => ax == null || ax.InPosition(target);
        #endregion

        #region Low-Level IO (Read/Write/State)
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
            if(Config.IsSimulation || Config.IsDryRun)
            {
                return true;
            }
            return ReadInput(OutputFeederConfig.IO.FEEDER_UP);
        }
        
        public bool IsFeederDown()
        {
            if (Config.IsSimulation || Config.IsDryRun)
            {
                return true;
            }
            return ReadInput(OutputFeederConfig.IO.FEEDER_DOWN);
        }
        
        public bool IsClamped()
        {
            bool bRtn = false;
            if (Config.IsSimulation || Config.IsDryRun)
            {
                bRtn = true;
                return true;
            }
            bRtn = !ReadInput(OutputFeederConfig.IO.FEEDER_UNCLAMP);
            return bRtn;
        }

        public bool IsUnClamped()
        {
            if (Config.IsSimulation || Config.IsDryRun)
            {
                return true;
            }
            return ReadInput(OutputFeederConfig.IO.FEEDER_UNCLAMP);
        }
        public bool IsRingPresent()
        {
            if (Config.IsSimulation || Config.IsDryRun)
            {
                return true;
            }
            return ReadInput(OutputFeederConfig.IO.FEEDER_RING_CHECK);
        }
        public bool IsOverload()
        {
            if (Config.IsSimulation || Config.IsDryRun)
            {
                return true;
            }
            return ReadInput(OutputFeederConfig.IO.FEEDER_OVERLOAD);
        }
        #endregion

        /// ////////////////////////////////////////////////////////////////////////////////////////
        #region === Direct Valve Control (입력 신호/인터락 무관 강제 구동용) ===
        public bool IsFeederUpValveOn() => IsOutputOn(OutputFeederConfig.IO.FEEDER_UP_VALVE);
        public bool IsFeederDownValveOn() => IsOutputOn(OutputFeederConfig.IO.FEEDER_DOWN_VALVE);
        public bool IsFeederClampValveOn() => IsOutputOn(OutputFeederConfig.IO.FEEDER_CLAMP_VALVE);
        public bool IsFeederUnclampValveOn() => IsOutputOn(OutputFeederConfig.IO.FEEDER_UNCLAMP_VALVE);
        #endregion
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
        public bool IsFeederYSafetyPosition(double fallbackTolerance = 0.01,
                                            bool useAxisInposTolerance = true,
                                            bool treatMissingAsSafe = true,
                                            bool allowPositiveBeyond = true,
                                            IEnumerable<string> customCandidates = null)
        {
            if (AxisFeederY == null)
                return treatMissingAsSafe;

            var cfg = Config;
            if (cfg == null)
                return treatMissingAsSafe;

            // 기본 후보 목록
            var defaultCandidates = new[] { "SafetyPos", "Safety", "Safe", "Ready" };
            var candidates = (customCandidates == null ? defaultCandidates : customCandidates)
                             .Where(s => !string.IsNullOrWhiteSpace(s));

            string axisKey = AxisNames.BinFeederY;
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
                        string.Equals(k, AxisFeederY.Name, StringComparison.OrdinalIgnoreCase)))
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
                if (!selectedTp.AxisPositions.TryGetValue(AxisFeederY.Name, out target))
                    return treatMissingAsSafe; // 좌표가 없으면 안전 판단 불가 → 기본 정책대로
            }

            double cur = AxisFeederY.GetPosition();
            double tol = useAxisInposTolerance
                ? (AxisFeederY.Config?.InposTolerance ?? fallbackTolerance)
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
        protected override int OnRunWork()
        {
            int nRet = 0;

            MaterialWafer wafer = this.OutputStage.GetMaterialWafer();

            // Stage 요청 인지 시 Busy로 표시(선택)
            if (this.OutputStage.IsWorking() == true)
            {
                if (wafer != null)
                {
                    if (wafer.ProcessSatate == Material.MaterialProcessSatate.Ready)
                    {
                        nRet = PreparetoOutputStage();
                    }
                }
                return nRet;
            }

            // 0) Stage에 제품이 있으면 "언로딩 먼저"
            bool needUnloadFirst = false;
            try
            {
                needUnloadFirst = OutputStage.IsCompletedWork();
                if (needUnloadFirst)
                {

                }
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                needUnloadFirst = false;
            }

            if (needUnloadFirst)
            {
                // 8) Feeder -> Stage: WaferUnloadingBeforeStage
                nRet = BinUnloading(wafer);
                if (nRet != 0)
                {
                    AxisFeederY.EmgStop();
                    PostAlarm((int)AlarmKeys.Alarm_BinUnloadingFailed);
                    this.State = ProcessState.Error;
                }
            }

            // 1) Feeder -> Cassette: Scan
            if (this.OutputCassetteLifter.IsScanCompleted() == false)
            {
                nRet = this.OutputCassetteLifter.ScanBin();
                if (nRet != 0)
                {
                    AxisFeederY.EmgStop();
                    PostAlarm((int)AlarmKeys.Alarm_BinLoadingFailed);
                    this.State = ProcessState.Error;
                    return nRet;
                }
            }

            if (this.OutputCassetteLifter.IsHaveMoreProcessWafer())
            {
                // 2) Feeder -> Cassette: MoveToNextSlot
                nRet = this.OutputCassetteLifter.MoveToNextSlot();
                if (nRet != 0)
                {
                    AxisFeederY.EmgStop();
                    PostAlarm((int)AlarmKeys.Alarm_BinLoadingFailed);
                    this.State = ProcessState.Error;
                    return nRet;
                }

                // 3) Feeder -> Stage: WaferLoadingBeforeStage
                nRet = OutputStage.LoadingBinPrepare();
                if (nRet != 0)
                {
                    AxisFeederY.EmgStop();
                    PostAlarm((int)AlarmKeys.Alarm_OutputStageInterlockFailed);
                    this.State = ProcessState.Error;
                    return nRet;
                }

                // 4) Feeder 내부 로딩 Cascette에서 Wafer Pick
                nRet = BinLoading(); // 여기서 Barcode Reading 포함
                if (nRet != 0)
                {
                    AxisFeederY.EmgStop();
                    PostAlarm((int)AlarmKeys.Alarm_BinLoadingFailed);
                    this.State = ProcessState.Error;
                    return nRet;
                }

                // 4) Feeder 내부 로딩 Stage에 Wafer Load
                nRet = StageLoading();
                if (nRet != 0)
                {
                    AxisFeederY.EmgStop();
                    PostAlarm((int)AlarmKeys.Alarm_StageLoadingFailed);
                    this.State = ProcessState.Error;
                    return nRet;
                }

                MakePath();
                this.MoveMaterial(new MaterialWafer(), OutputStage);
                this.OutputStage.UpdateUI();


                nRet = MoveToReay();
                if (nRet != 0)
                {
                    AxisFeederY.EmgStop();
                    PostAlarm((int)AlarmKeys.Alarm_BinLoadingFailed);
                    this.State = ProcessState.Error;
                    return nRet;
                }

                // 5) Feeder -> Stage: WaferLoadingAfterStage
                nRet = OutputStage.LoadingBinComplete();
                if (nRet != 0)
                {
                    AxisFeederY.EmgStop();
                    PostAlarm((int)AlarmKeys.Alarm_StageLoadingFailed);
                    this.State = ProcessState.Error;
                    return nRet;
                }

                nRet = PreparetoOutputStage();
                this.State = ProcessState.Complete;
            }
            else
            {
                nRet = MoveToReay();
                if (nRet != 0)
                {
                    AxisFeederY.EmgStop();
                    PostAlarm((int)AlarmKeys.Alarm_BinLoadingFailed);
                    this.State = ProcessState.Error;
                    return nRet;
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
            int ret = 0;
            this.OutputCassetteLifter.Start();
            this.OutputStage.Start();
            return ret;
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

        protected int MakePath()
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
                        for (int y = 0; y < recipe.BinCountY; y++)
                        {
                            for (int x = 0; x < recipe.BinCountX; x++)
                            {
                                MaterialDie die = new MaterialDie();
                                die.Presence = Material.MaterialPresence.NotExist;
                                die.ProcessSatate = Material.MaterialProcessSatate.Unknown;
                                die.BinX = x;
                                die.BinY = y;
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
            //this.SequencePlayers.Add();
        }

        #region Seq 단위 동작 함수
        private int PreparetoOutputStage()
        {
            int nRet = 0;

            // T 보정 필요시. 
            //nRet = OutputStage.ScanBin();

            return nRet;
        }
        public int BinUnloading(MaterialWafer wafer, bool isFine = false)
        {
            int nRet = 0;
            Log.Write(this, "BinUnloading Start");

            nRet = this.OutputStage.PrepareOutputStageUnloadingBin();
            if (nRet != 0)
            {
                AxisFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_BinUnloadingFailed);
                Log.Write(this, "OutputStage.PrepareOutputStageUnloadingBin Failed");
                nRet = -1;
                return nRet;
            }

            nRet = UnloadBinStageToFeeder(isFine);
            if (nRet != 0)
            {
                AxisFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_StageUnloadingFailed);
                Log.Write(this, "UnloadBinStageToFeeder Failed");
                nRet = -1;
                return nRet;
            }

            int nSlot = wafer.SlotIndex;
            nRet = this.OutputCassetteLifter.MoveToSlot(nSlot); // 언로딩 해야하는 Slot으로 이동 요청.
            if (nRet != 0)
            {
                AxisFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_BinUnloadingFailed);
                this.State = ProcessState.Error;
                Log.Write(this, "OutputCassetteLifter.MoveToSlot Failed");
                return nRet;
            }

            nRet = UnloadBinFeederToCassette(true);
            if (nRet != 0)
            {
                AxisFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_BinUnloadingFailed);
                this.State = ProcessState.Error;
                Log.Write(this, "UnloadBinFeederToCassette Failed");
                return nRet;
            }

            Log.Write(this, "BinUnloading Complete");
            return nRet;
        }
        public int BinLoading(bool isFine = false)
        {
            int nRet = 0;

            Log.Write(this, "WaferLoading Start");
            if (IsMoveInterLockCassette() == false)
            {
                Log.Write(this, "Not IsMoveInterLockCassette");
                return -1;
            }

            nRet = MoveToReay(isFine);
            if (nRet != 0)
            {
                Log.Write(this, "MoveToReay Failed");
                return nRet;
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

            var c = this.OutputCassetteLifter.GetMaterialCassette();
            int nIndex = this.OutputCassetteLifter.GetCurrectSlotID();
            MaterialWafer wafer = c.GetWafer(nIndex);
            this.SetMaterial(wafer);

            nRet = BarcodeReading(isFine);
            if (nRet != 0)
            {
                Log.Write(this, "BarcodeReading Failed");
                return nRet;
            }

            //nRet = StageLoading(isFine);
            //if (nRet != 0)
            //{
            //    Log.Write(this, "StageLoading Failed");
            //    return nRet;
            //}

            //nRet = MoveToReay(isFine);
            //if (nRet != 0)
            //{
            //    Log.Write(this, "MoveToReay Failed");
            //    return nRet;
            //}

            //nRet = UpFeeder();
            //if (nRet != 0)
            //{
            //    Log.Write(this, "UpFeeder Failed");
            //    return nRet;
            //}

            Log.Write(this, "BinLoading Complete");
            return nRet;
        }
        public int UnloadBinFeederToCassette(bool isFine = false)
        {
            int nRet = 0;

            nRet = UnloadBinStagetToFeeder(isFine);
            if (nRet != 0)
            {
                AxisFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_StageUnloadingFailed);
                Log.Write(this, "UnloadBinStagetToFeeder Failed");
                nRet = -1;
                return nRet;
            }

            nRet = ClampGripper();
            MaterialWafer wafer = new MaterialWafer();
            this.OutputStage.MoveMaterial(wafer, this);

            if (nRet != 0)
            {
                AxisFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_StageUnloadingFailed);
                Log.Write(this, "ClampGripper Failed");
                nRet = -1;
                return nRet;
            }

            nRet = MovePositionCassette(isFine);
            if (nRet != 0)
            {
                AxisFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_StageUnloadingFailed);
                Log.Write(this, "MovePositionCassette Failed");
                nRet = -1;
                return nRet;
            }

            nRet = UnClampGripper();
            if (nRet != 0)
            {
                AxisFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_StageUnloadingFailed);
                Log.Write(this, "UnClampGripper Failed");
                nRet = -1;
                return nRet;
            }

            //회피 Position으로 사용.
            nRet = MovePositionBarcode(isFine);
            if (nRet != 0)
            {
                AxisFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_StageUnloadingFailed);
                Log.Write(this, "MovePositionBarcode Failed");
                nRet = -1;
                return nRet;
            }
            wafer = new MaterialWafer();
            MoveMaterial(wafer, null);
            return nRet;
        }


        // 단위에 단위.
        public int ClampGripper()
        {
            int nRet = 0;
            this.SetClamp(true);
            if (!IsClamped())
            {
                Log.Write(this, "Clamp Failed");
                PostAlarm((int)AlarmKeys.Alarm_GripperClampFailed);
                nRet = -1;
                return nRet;
            }
            return nRet;
        }
        public int UnClampGripper()
        {
            int nRet = 0;
            this.SetClamp(false);
            if (!IsUnClamped())
            {
                Log.Write(this, "Unclamp Failed");
                PostAlarm((int)AlarmKeys.Alarm_GripperClampFailed);
                nRet = -1;
                return nRet;
            }
            return nRet;
        }
        public int UpFeeder()
        {
            int nRet = 0;
            this.SetLift(true);
            if (!IsFeederUp())
            {
                Log.Write(this, "Feeder Up Failed");
                PostAlarm((int)AlarmKeys.Alarm_GripperClampFailed);
                nRet = -1;
                return nRet;
            }
            return nRet;
        }
        public int DownFeeder()
        {
            int nRet = 0;
            this.SetLift(false);
            if (!IsFeederDown())
            {
                Log.Write(this, "Feeder Down Failed");
                PostAlarm((int)AlarmKeys.Alarm_BinLoadingFailed);
                nRet = -1;
                return nRet;
            }
            return nRet;
        }
        public int MoveToReay(bool isFine = false)
        {
            int nRet = 0;

            Log.Write(this, "MoveToReay Start");
            if (IsMoveInterLockCassette() == false)
            {
                return -1;
            }

            nRet = MovePositionReady(isFine);
            if (nRet != 0)
            {
                AxisFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_BinLoadingFailed);
                nRet = -1;
                return nRet;
            }

            nRet = UpFeeder();
            if (nRet != 0)
            {
                AxisFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_BinLoadingFailed);
                nRet = -1;
                return nRet;
            }
            Log.Write(this, "MoveToReay End");


            return nRet;
        }

        public int MoveToCassette(bool isFine = false)
        {
            int nRet = 0;
            nRet = MovePositionCassette(isFine);
            if (nRet != 0)
            {
                AxisFeederY.EmgStop();
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
                AxisFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_BinLoadingFailed);
                Log.Write(this, "Not IsInterlockOKWaferLoading");
                nRet = -1;
                return nRet;
            }
            nRet = base.MoveTeachingPositionOnce((int)OutputFeederConfig.TeachingPositionName.Cassette, isFine);
            if (nRet != 0)
            {
                AxisFeederY.EmgStop();
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
                AxisFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_IsBinReadyForLoading);
                Log.Write(this, "OutputCassetteLifter Not Ready for Loading");
                bRtn = false;
                return bRtn;
            }

            if (!OutputStage.IsBinLoadingPosition())
            {
                AxisFeederY.EmgStop();
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
                AxisFeederY.EmgStop();
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
                AxisFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_BarcodeReadingFailed);
                Log.Write(this, "Barcode Reading Failed");
                nRet = -1;
                return nRet;
            }

            return nRet;
        }
        public int StageLoading(bool isFine = false)
        {
            int nRet = 0;

            Log.Write(this, "StageLoading Start");
            if (IsMoveInterLockCassette() == false)
            {
                Log.Write(this, "Not IsMoveInterLockCassette");
                return -1;
            }

            nRet = MovePositionStage(isFine);
            if (nRet != 0)
            {
                AxisFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_StageLoadingFailed);
                Log.Write(this, "MovePositionStage Failed");
                nRet = -1;
                return nRet;
            }

            nRet = UnClampGripper();
            if (nRet != 0)
            {
                AxisFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_StageLoadingFailed);
                Log.Write(this, "UnClampGripper Failed");
                nRet = -1;
                return nRet;
            }

            Log.Write(this, "StageLoading End");
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
                AxisFeederY.EmgStop();
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
                AxisFeederY.EmgStop();
                PostAlarm((int)AlarmKeys.Alarm_StageUnloadingFailed);
                Log.Write(this, "DownFeeder Failed");
                nRet = -1;
                return nRet;
            }

            nRet = MovePositionStage(isFine);
            if (nRet != 0)
            {
                AxisFeederY.EmgStop();
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

            double dYSafePosOffset = Config.FeederToCassetteOverapLength;
            if (IsClamped())
            {
                dYSafePosOffset += Config.WaferRingframeSize;
            }
            var tp = TeachingPositions[(int)InputFeederConfig.TeachingPositionName.Cassette];
            double dInterlockPos = tp.GetAxisPosition(this.AxisFeederY.Name);

            dInterlockPos += dYSafePosOffset;
            if (AxisFeederY.GetPosition() < dInterlockPos)
            {
                Log.Write(this.UnitName, "IsInterlockOKWithCassete", 
                $"FeederY Position Low. Current:" +
                $"{AxisFeederY.GetPosition()}, InterlockPos:{dInterlockPos}");
                
                bRtn = false;
                return bRtn;
            }

            return bRtn;

        }


        #endregion
    }
}
