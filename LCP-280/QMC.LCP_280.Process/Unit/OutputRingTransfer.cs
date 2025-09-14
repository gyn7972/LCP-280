using QMC.Common;
using QMC.Common.Component;
using QMC.Common.IOUtil;
using QMC.Common.Motion;
using QMC.Common.Motions;
using QMC.Common.Unit;
using QMC.LCP_280.Process.Component;
using System.Collections.Generic;
using System.Linq;

namespace QMC.LCP_280.Process.Unit
{
    /// <summary>
    /// OutputRingTransfer (Bin Feeder / Ring Transfer - Output side)
    ///  - Y 축 이송 + Lift + Clamp
    ///  - Ring 존재 / Overload 센서
    ///  - Config/Unit 구조를 다른 Unit들과 통일
    /// </summary>
    public class OutputRingTransfer : BaseUnit<OutputRingTransferConfig>
    {
        #region Config / Teaching
        
        
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
        public OutputRingTransfer(OutputRingTransferConfig config = null) : base(new OutputRingTransferConfig())
        {
            
            AddComponents();
        }

        public override void AddComponents()
        {
            Config.LoadAndBindAxes(Equipment.Instance.AxisManager);
            Config.InitializeDefaultTeachingPositions();
            TeachingPositions.Clear();
            foreach (var tp in Config.TeachingPositions)
                TeachingPositions.Add(tp);
            BindAxes();
            BindIoDomains();
        }

        private void BindAxes()
        {
            // { AxisNames.WaferFeederY, 0.0 }

            var mgr = Equipment.Instance?.AxisManager;
            if (mgr == null)
            {
                Log.Write("OutputRingTransfer", "[BindAxes] AxisManager null");
                return;
            }

            const string unitName = "Unit"; // Equipment에서 축 등록 시 사용한 유닛명과 동일해야 함
            BindAxis(mgr, unitName, AxisNames.WaferFeederY, ref _feederY);
        }
        #endregion

        #region Runtime
        public override int OnRun()  { int ret = 0; return ret; }
        public override int OnStop() { int ret = 0; base.OnStop(); return ret; }
        protected override int OnRunReady() { return 0; }
        protected override int OnRunWork() { return 0; }
        protected override int OnRunComplete() { return 0; }
        #endregion

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
                Log.Write("OutputRingTransfer", "BindIoDomains", "Cylinder not found: OutFeederLift");
            }

            if (!IoAutoBindings.Cylinders.TryGetValue("OutFeederClamp", out _cylClamp))
            {
                Log.Write("OutputRingTransfer", "BindIoDomains", "Cylinder not found: OutFeederClamp");
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
        public bool IsFeederUp() => ReadInput(OutputRingTransferConfig.IO.FEEDER_UP);
        public bool IsFeederDown() => ReadInput(OutputRingTransferConfig.IO.FEEDER_DOWN);
        public bool IsUnclamped() => ReadInput(OutputRingTransferConfig.IO.FEEDER_UNCLAMP);
        public bool IsRingPresent() => ReadInput(OutputRingTransferConfig.IO.FEEDER_RING_CHECK);
        public bool IsOverload() => ReadInput(OutputRingTransferConfig.IO.FEEDER_OVERLOAD);
        #endregion

        /// ////////////////////////////////////////////////////////////////////////////////////////

        #region === Direct Valve Control (입력 신호/인터락 무관 강제 구동용) ===
        public bool IsFeederUpValveOn() => IsOutputOn(OutputRingTransferConfig.IO.FEEDER_UP_VALVE);
        public bool IsFeederDownValveOn() => IsOutputOn(OutputRingTransferConfig.IO.FEEDER_DOWN_VALVE);
        public bool IsFeederClampValveOn() => IsOutputOn(OutputRingTransferConfig.IO.FEEDER_CLAMP_VALVE);
        public bool IsFeederUnclampValveOn() => IsOutputOn(OutputRingTransferConfig.IO.FEEDER_UNCLAMP_VALVE);
        #endregion

        #region Seq 단위 동작 함수
        public int BinLoading()
        {
            int nRet = -1;
            /* TODO */
            return nRet;
        }

        public int BarcodeReading()
        {
            int nRet = -1;
            /* TODO */
            return nRet;
        }

        public int StageLoading()
        {
            int nRet = -1;
            /* TODO */
            return nRet;
        }

        public int StageUnloading()
        {
            int nRet = -1;
            /* TODO */
            return nRet;
        }

        public int BinUnloading()
        {
            int nRet = -1;
            /* TODO */
            return nRet;
        }
        #endregion
    }
}
