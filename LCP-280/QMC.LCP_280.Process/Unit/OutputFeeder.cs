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
        }
        #endregion

        #region Unit
        public OutputCassetteLifter OutputCassetteLifter { get; set; }
        public OutputStage OutputStage { get; set; }
        #endregion

        #region Axis
        private MotionAxis _feederY;
        public MotionAxis FeederY => _feederY;
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
                    foreach (var ax in Axes.Values)
                    {
                        ax.EmgStop();
                    }
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
                PostAlarm((int)AlarmKeys.Alarm_FeederClampUp);
                nRet = -1;
                return nRet;
            }

            if (!OutputStage.IsAnyAxisMoving())
            {
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
                PostAlarm((int)AlarmKeys.Alarm_FeederClampUp);
                nRet = -1;
                return nRet;
            }

            if (!OutputStage.IsAnyAxisMoving())
            {
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

            if (!OutputStage.IsAnyAxisMoving())
            {
                PostAlarm((int)AlarmKeys.Alarm_OutputStageInterlockFailed);
                nRet = -1;
                return nRet;
            }

            if (!OutputCassetteLifter.IsAnyAxisMoving())
            {
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
                    foreach (var ax in Axes.Values)
                    {
                        ax.EmgStop();
                    }
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

            if (!OutputStage.IsAnyAxisMoving())
            {
                PostAlarm((int)AlarmKeys.Alarm_OutputStageInterlockFailed);
                nRet = -1;
                return nRet;
            }

            if (!OutputCassetteLifter.IsAnyAxisMoving())
            {
                PostAlarm((int)AlarmKeys.Alarm_OutputStageInterlockFailed);
                nRet = -1;
                return nRet;
            }

            return nRet;
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
        public void MoveAxisOnce(MotionAxis ax, double target)
        {
            if (ax == null) return;
            if (System.Math.Abs(ax.GetPosition() - target) > ax.Config.InposTolerance * 3)
                ax.MoveAbs(target, ax.Config.MaxVelocity, ax.Config.RunAcc, ax.Config.RunDec, ax.Config.AccJerkPercent);
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
        public bool IsFeederUp() => ReadInput(OutputFeederConfig.IO.FEEDER_UP);
        public bool IsFeederDown() => ReadInput(OutputFeederConfig.IO.FEEDER_DOWN);
        public bool IsUnclamped() => ReadInput(OutputFeederConfig.IO.FEEDER_UNCLAMP);
        public bool IsRingPresent() => ReadInput(OutputFeederConfig.IO.FEEDER_RING_CHECK);
        public bool IsOverload() => ReadInput(OutputFeederConfig.IO.FEEDER_OVERLOAD);
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
            if (FeederY == null)
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


        #region Runtime
        public override int OnRun() { int ret = 0; return ret; }
        public override int OnStop() { int ret = 0; base.OnStop(); return ret; }
        protected override int OnRunReady() { return 0; }
        protected override int OnRunWork() { return 0; }
        protected override int OnRunComplete() { return 0; }
        #endregion

        
        
        
        #region Seq 단위 동작 함수
        




        public int BinLoading()
        {
            int nRet = 0;
            /* TODO */
            return nRet;
        }
        public int BarcodeReading()
        {
            int nRet = 0;
            /* TODO */
            return nRet;
        }
        public int StageLoading()
        {
            int nRet = 0;
            /* TODO */
            return nRet;
        }
        public int StageUnloading()
        {
            int nRet = 0;
            /* TODO */
            return nRet;
        }
        public int BinUnloading()
        {
            int nRet = 0;
            /* TODO */
            return nRet;
        }



        #endregion
    }
}
