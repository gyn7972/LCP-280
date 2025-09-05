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
    public class OutputRingTransfer : BaseUnit
    {
        public OutputRingTransferConfig OutputRingTransferConfig { get; private set; }
        public List<TeachingPosition> TeachingPositions { get; private set; } = new List<TeachingPosition>();

        public OutputRingTransfer(OutputRingTransferConfig config = null)
            : base("OutputRingTransferConfig")
        {
            OutputRingTransferConfig = config ?? new OutputRingTransferConfig();
            AddComponents();
        }

        public override void AddComponents()
        {
            OutputRingTransferConfig.LoadAndBindAxes(Equipment.Instance.AxisManager);
            OutputRingTransferConfig.InitializeDefaultTeachingPositions();
            TeachingPositions.Clear();
            foreach (var tp in OutputRingTransferConfig.TeachingPositions)
                TeachingPositions.Add(tp);
            BindAxes();
            BindIoDomains();
        }

        public override void OnRun() => base.OnRun();
        public override void OnStop() => base.OnStop();

        public void TeachCurrentPosition(string positionName, string description = null)
        {
            var axisPositions = new Dictionary<string, double>();
            foreach (var axisPair in Axes)
                axisPositions[axisPair.Key] = axisPair.Value.GetPosition();
            var tp = new TeachingPosition(positionName, axisPositions, description);
            OutputRingTransferConfig.SetTeachingPosition(tp);
        }

        public int MoveToTeachingPosition(string positionName, double vel = 5, double acc = 10, double dec = 10, double jerk = 50)
        {
            var tp = OutputRingTransferConfig.GetTeachingPosition(positionName);
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

        #region Axis Helpers
        private MotionAxis _feederY;
        public MotionAxis FeederY => _feederY;
        private void BindAxes()
        {
            Axes.TryGetValue("Bin Feeder Y Axis", out _feederY);
        }
        public double GetTP(string tpName, string axisName)
        {
            var tp = OutputRingTransferConfig.GetTeachingPosition(tpName);
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

        #region IO Helpers
        public bool ReadInput(string name)
        {
            var hi = OutputRingTransferConfig.HardInputs.FirstOrDefault(i => i.Name.Equals(name, System.StringComparison.OrdinalIgnoreCase));
            if (hi == null) return false;
            var eq = Equipment.Instance; var dio = eq?.DioScan; if (dio == null) return false;
            foreach (var m in eq.UnitIO.Modules)
                if (dio.TryGetInput(m.ModuleName, hi.Disp, out var v)) return v;
            return false;
        }
        public bool WriteOutput(string name, bool on)
        {
            var ho = OutputRingTransferConfig.HardOutputs.FirstOrDefault(o => o.Name.Equals(name, System.StringComparison.OrdinalIgnoreCase));
            if (ho == null) return false;
            var eq = Equipment.Instance; var dio = eq?.DioScan; if (dio == null) return false;
            foreach (var m in eq.UnitIO.Modules)
                if (dio.WriteOutput(m.ModuleName, ho.Disp, on) == 0) return true;
            return false;
        }
        #endregion

        #region IO Domain (Feeder Lift / Clamp)
        private Cylinder _feederLift; // UP/DOWN
        private const string NAME_FEEDER_UP = "BIN FEEDER UP";
        private const string NAME_FEEDER_DOWN = "BIN FEEDER DOWN"; // sensor & output naming mismatch (DOWNE) handled by mapping
        private const string NAME_FEEDER_CLAMP = "BIN FEEDER CLAMP";
        private const string NAME_FEEDER_UNCLAMP = "BIN FEEDER UNCLAMP"; // output spelled UNCALMP in config output list, map by name lookup
        private const string NAME_FEEDER_UNCLAMP_ALT = "BIN FEEDER UNCLAMP"; // actual output config name
        private const string NAME_FEEDER_RING = "BIN FEEDER RING CHECK";
        private const string NAME_FEEDER_OVERLOAD = "BIN FEEDER OVERLOAD CHECK";

        private void BindIoDomains()
        {
            var eq = Equipment.Instance; var unit = eq?.UnitIO; if (unit == null) return;
            // Lift outputs
            DIO.MapByName(unit, "OutFeeder.UpOut", true, NAME_FEEDER_UP);
            DIO.MapByName(unit, "OutFeeder.DownOut", true, NAME_FEEDER_DOWN); // output actual name
            // Sensors (reuse names)
            DIO.MapByName(unit, "OutFeeder.UpIn", false, NAME_FEEDER_UP);
            DIO.MapByName(unit, "OutFeeder.DownIn", false, NAME_FEEDER_DOWN);
            _feederLift = new Cylinder("BinFeederLift", "OutFeeder.UpOut", "OutFeeder.DownOut", "OutFeeder.UpIn", "OutFeeder.DownIn");
            // Clamp outputs
            DIO.MapByName(unit, "OutFeeder.ClampOut", true, NAME_FEEDER_CLAMP);
            DIO.MapByName(unit, "OutFeeder.UnclampOut", true, NAME_FEEDER_UNCLAMP_ALT);
        }

        public bool FeederUp(int timeoutMs = 3000) => _feederLift?.Extend(timeoutMs) ?? false;
        public bool FeederDown(int timeoutMs = 3000) => _feederLift?.Retract(timeoutMs) ?? false;
        public void FeederAllOff() => _feederLift?.AllOff();
        public void SetClamp(bool clamp)
        {
            WriteOutput(NAME_FEEDER_CLAMP, clamp);
            WriteOutput(NAME_FEEDER_UNCLAMP_ALT, !clamp);
        }
        public bool IsRingPresent() => ReadInput(NAME_FEEDER_RING);
        public bool Overload() => ReadInput(NAME_FEEDER_OVERLOAD);
        #endregion
    }
}
