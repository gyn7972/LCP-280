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
    public class OutputCassetteLifter : BaseUnit
    {
        public OutputCassetteLifterConfig OutputCassetteLifterConfig { get; private set; }
        public List<TeachingPosition> TeachingPositions { get; private set; } = new List<TeachingPosition>();

        public OutputCassetteLifter(OutputCassetteLifterConfig config = null) : base("OutputCassetteLifterConfig")
        {
            OutputCassetteLifterConfig = config ?? new OutputCassetteLifterConfig();
            AddComponents();
        }

        public override void AddComponents()
        {
            OutputCassetteLifterConfig.LoadAndBindAxes(Equipment.Instance.AxisManager);
            OutputCassetteLifterConfig.InitializeDefaultTeachingPositions();
            TeachingPositions.Clear();
            foreach (var tp in OutputCassetteLifterConfig.TeachingPositions)
                TeachingPositions.Add(tp);
            BindAxes();
            // No specific IO domain yet (only sensors in config) ? placeholder
        }

        public override void OnRun() => base.OnRun();
        public override void OnStop() { base.OnStop(); }

        public void TeachCurrentPosition(string positionName, string description = null)
        {
            var axisPositions = new Dictionary<string, double>();
            foreach (var axisPair in Axes)
                axisPositions[axisPair.Key] = axisPair.Value.GetPosition();
            var tp = new TeachingPosition(positionName, axisPositions, description);
            OutputCassetteLifterConfig.SetTeachingPosition(tp);
        }

        public int MoveToTeachingPosition(string positionName, double vel = 5, double acc = 10, double dec = 10, double jerk = 50)
        {
            var tp = OutputCassetteLifterConfig.GetTeachingPosition(positionName);
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
        private MotionAxis _axZ;
        public MotionAxis AxisZ => _axZ;
        private void BindAxes() { Axes.TryGetValue("Bin Lifter Z Axis", out _axZ); }
        public double GetTP(string tpName, string axisName)
        {
            var tp = OutputCassetteLifterConfig.GetTeachingPosition(tpName);
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

        #region IO (Sensors Only Placeholder)
        // Config has only inputs; if needed later we can map with DIO.MapByName similar to stages.
        public bool ReadInput(string name)
        {
            var hi = OutputCassetteLifterConfig.HardInputs?.FirstOrDefault(i => i.Name.Equals(name, System.StringComparison.OrdinalIgnoreCase));
            if (hi == null) return false;
            var eq = Equipment.Instance; var dio = eq?.DioScan; if (dio == null) return false;
            foreach (var m in eq.UnitIO.Modules)
                if (dio.TryGetInput(m.ModuleName, hi.Disp, out var v)) return v;
            return false;
        }
        #endregion
    }
}