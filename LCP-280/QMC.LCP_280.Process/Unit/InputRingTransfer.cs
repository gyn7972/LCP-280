using QMC.Common;
using QMC.Common.Component;
using QMC.Common.IOUtil;
using QMC.Common.Motion;
using QMC.Common.Motions;
using QMC.Common.Unit;
using QMC.LCP_280.Process.Component; // TeachingPosition
using System.Collections.Generic;
using System.Linq;

namespace QMC.LCP_280.Process.Unit
{
    /// <summary>
    /// InputRingTransfer (Wafer Feeder / Ring Transfer Unit)
    ///  - X 축 이송 + Lift + Clamp (Ring 존재 검사 / Overload 검사)
    ///  - Teaching Position 관리 (InputRingTransferConfig)
    ///  - Cylinder 기반 동작 API (FeederUp/Down, Clamp)
    ///  - OutputStage / InputStage 와 동일한 Region/패턴 구성
    /// </summary>
    public class InputRingTransfer : BaseUnit
    {
        #region Config / Teaching
        public InputRingTransferConfig InputRingTransferConfig { get; private set; }
        public List<TeachingPosition> TeachingPositions { get; private set; } = new List<TeachingPosition>();
        #endregion

        #region Axes
        private MotionAxis _feederY;
        public MotionAxis FeederX => _feederY;
        #endregion

        #region IO Domain Members
        private Cylinder _feederLift; // Up/Down
        private Cylinder _cylClamp;   // Clamp/Unclamp
        #endregion

        #region Constructor / Initialization
        public InputRingTransfer(InputRingTransferConfig config = null) : base("InputRingTransferConfig")
        {
            InputRingTransferConfig = config ?? new InputRingTransferConfig();
            AddComponents();
        }

        public override void AddComponents()
        {
            InputRingTransferConfig.LoadAndBindAxes(Equipment.Instance.AxisManager);
            InputRingTransferConfig.InitializeDefaultTeachingPositions();
            TeachingPositions.Clear();
            foreach (var tp in InputRingTransferConfig.TeachingPositions)
                TeachingPositions.Add(tp);
            BindAxes();
            BindIoDomains();
        }
        #endregion

        #region Runtime Hooks
        public override void OnRun()  => base.OnRun();
        public override void OnStop() => base.OnStop();
        #endregion

        #region Axis Binding
        private void BindAxes()
        {
            var mgr = Equipment.Instance?.AxisManager;
            if (mgr == null)
            {
                Log.Write("UnitAxis", "[BindAxes] AxisManager null");
                return;
            }

            const string unitName = "Unit"; // Equipment에서 축 등록 시 사용한 유닛명과 동일해야 함
            BindAxis(mgr, unitName, AxisNames.WaferFeederY, ref _feederY);
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
            var tp = InputRingTransferConfig.GetTeachingPosition(tpName);
            if (tp != null && tp.AxisPositions != null && tp.AxisPositions.TryGetValue(axisName, out var v)) return v;
            return 0.0;
        }
        #endregion

        #region Teaching Helpers
        public void TeachCurrentPosition(string positionName, string description = null)
        {
            var axisPositions = new Dictionary<string, double>();
            foreach (var axisPair in Axes)
                axisPositions[axisPair.Key] = axisPair.Value.GetPosition();
            var tp = new TeachingPosition(positionName, axisPositions, description);
            InputRingTransferConfig.SetTeachingPosition(tp);
        }

        public int MoveToTeachingPosition(string positionName, double vel = 5, double acc = 10, double dec = 10, double jerk = 50)
        {
            var tp = InputRingTransferConfig.GetTeachingPosition(positionName);
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
            var tp = InputRingTransferConfig.GetTeachingPosition(positionName);
            if (tp == null) return false;
            foreach (var kv in tp.AxisPositions)
                if (!Axes.TryGetValue(kv.Key, out var axis) || !InPos(axis, kv.Value)) return false;
            return true;
        }

        #endregion

        #region Low-Level IO (Read/Write by Name)
        public bool ReadInput(string name)
        {
            var hi = InputRingTransferConfig.HardInputs.FirstOrDefault(i => i.Name.Equals(name, System.StringComparison.OrdinalIgnoreCase));
            if (hi == null) return false;
            var eq = Equipment.Instance; var dio = eq?.DioScan; if (dio == null) return false;
            foreach (var m in eq.UnitIO.Modules)
                if (dio.TryGetInput(m.ModuleName, hi.Disp, out var v)) return v;
            return false;
        }
        public bool WriteOutput(string name, bool on)
        {
            var ho = InputRingTransferConfig.HardOutputs.FirstOrDefault(o => o.Name.Equals(name, System.StringComparison.OrdinalIgnoreCase));
            if (ho == null) return false;
            var eq = Equipment.Instance; var dio = eq?.DioScan; if (dio == null) return false;
            foreach (var m in eq.UnitIO.Modules)
                if (dio.WriteOutput(m.ModuleName, ho.Disp, on) == 0) return true;
            return false;
        }
        public bool IsOutputOn(string name)
        {
            var ho = InputRingTransferConfig.HardOutputs.FirstOrDefault(o => o.Name.Equals(name, System.StringComparison.OrdinalIgnoreCase));
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
                Log.Write("InputRingTransfer", "BindIoDomains", "Cylinder not found: InFeederLift");
            }

            if (!IoAutoBindings.Cylinders.TryGetValue("InFeederClamp", out _cylClamp))
            {
                Log.Write("InputRingTransfer", "BindIoDomains", "Cylinder not found: InFeederClamp");
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
        public bool SetClmp(bool bUpDn)
        {
            if (_cylClamp == null) return false;
            if (bUpDn) return _cylClamp.Extend();
            else return _cylClamp.Retract();
        }

        #region === Direct Valve Control (입력 신호/인터락 무관 강제 구동용) ===
        public void SetFeederUpValve(bool on) => WriteOutput(InputRingTransferConfig.IO.FEEDER_UP_VALVE, on);
        public bool IsFeederUpValveOn() => IsOutputOn(InputRingTransferConfig.IO.FEEDER_UP_VALVE);
        public void SetFeederDownValve(bool on) => WriteOutput(InputRingTransferConfig.IO.FEEDER_DOWN_VALVE, on);
        public bool IsFeederDownValveOn() => IsOutputOn(InputRingTransferConfig.IO.FEEDER_DOWN_VALVE);
        public void SetFeederClampValve(bool on) => WriteOutput(InputRingTransferConfig.IO.FEEDER_CLAMP_VALVE, on);
        public bool IsFeederClampValveOn() => IsOutputOn(InputRingTransferConfig.IO.FEEDER_CLAMP_VALVE);
        public void SetFeederUnclampValve(bool on) => WriteOutput(InputRingTransferConfig.IO.FEEDER_UNCLAMP_VALVE, on);
        public bool IsFeederUnclampValveOn() => IsOutputOn(InputRingTransferConfig.IO.FEEDER_UNCLAMP_VALVE);
        #endregion

        #region High-Level Actuator API
        public bool FeederUp(int timeoutMs = 3000)    => _feederLift?.Extend(timeoutMs) ?? false;
        public bool FeederDown(int timeoutMs = 3000)  => _feederLift?.Retract(timeoutMs) ?? false;
        public void FeederAllOff()                    => _feederLift?.AllOff();
        public void SetClamp(bool clamp)
        {
            WriteOutput(InputRingTransferConfig.IO.FEEDER_CLAMP_VALVE, clamp);
            WriteOutput(InputRingTransferConfig.IO.FEEDER_UNCLAMP_VALVE, !clamp);
        }
        #endregion

        #region Status Helpers
        public bool IsFeederUp()        => ReadInput(InputRingTransferConfig.IO.FEEDER_UP);
        public bool IsFeederDown()      => ReadInput(InputRingTransferConfig.IO.FEEDER_DOWN);
        public bool IsUnclamped()       => ReadInput(InputRingTransferConfig.IO.FEEDER_UNCLAMP);
        public bool IsRingPresent()     => ReadInput(InputRingTransferConfig.IO.FEEDER_RING_CHECK);
        public bool IsOverload()        => ReadInput(InputRingTransferConfig.IO.FEEDER_OVERLOAD);
        #endregion
    }
}