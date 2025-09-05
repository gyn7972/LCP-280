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
    public class Rotary : BaseUnit
    {
        public RotaryConfig RotaryConfig { get; private set; }
        public List<TeachingPosition> TeachingPositions { get; private set; } = new List<TeachingPosition>();

        public Rotary(RotaryConfig config = null) : base("RotaryConfig")
        {
            RotaryConfig = config ?? new RotaryConfig();
            AddComponents();
        }

        public override void AddComponents()
        {
            RotaryConfig.LoadAndBindAxes(Equipment.Instance.AxisManager);
            RotaryConfig.InitializeDefaultTeachingPositions();
            TeachingPositions.Clear();
            foreach (var tp in RotaryConfig.TeachingPositions)
                TeachingPositions.Add(tp);
            BindAxes();
            BindIoDomains();
        }

        public override void OnRun() => base.OnRun();
        public override void OnStop() { base.OnStop(); }

        public void TeachCurrentPosition(string positionName, string description = null)
        {
            var axisPositions = new Dictionary<string, double>();
            foreach (var axisPair in Axes)
                axisPositions[axisPair.Key] = axisPair.Value.GetPosition();
            var tp = new TeachingPosition(positionName, axisPositions, description);
            RotaryConfig.SetTeachingPosition(tp);
        }

        public int MoveToTeachingPosition(string positionName, double vel = 5, double acc = 10, double dec = 10, double jerk = 50)
        {
            var tp = RotaryConfig.GetTeachingPosition(positionName);
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
        private MotionAxis _axT;
        public MotionAxis AxisT => _axT;
        private void BindAxes() { Axes.TryGetValue("Index T Axis", out _axT); }
        public double GetTP(string tpName, string axisName)
        {
            var tp = RotaryConfig.GetTeachingPosition(tpName);
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

        #region IO Low-Level
        public bool ReadInput(string name)
        {
            var hi = RotaryConfig.HardInputs.FirstOrDefault(i => i.Name.Equals(name, System.StringComparison.OrdinalIgnoreCase));
            if (hi == null) return false;
            var eq = Equipment.Instance; var dio = eq?.DioScan; if (dio == null) return false;
            foreach (var m in eq.UnitIO.Modules)
                if (dio.TryGetInput(m.ModuleName, hi.Disp, out var v)) return v;
            return false;
        }
        public bool WriteOutput(string name, bool on)
        {
            var ho = RotaryConfig.HardOutputs.FirstOrDefault(o => o.Name.Equals(name, System.StringComparison.OrdinalIgnoreCase));
            if (ho == null) return false;
            var eq = Equipment.Instance; var dio = eq?.DioScan; if (dio == null) return false;
            foreach (var m in eq.UnitIO.Modules)
                if (dio.WriteOutput(m.ModuleName, ho.Disp, on) == 0) return true;
            return false;
        }
        #endregion

        #region IO Domain (Per Slot Vacuum / Blow / Vent)
        private readonly Dictionary<int, Vacuum> _slotVacuums = new Dictionary<int, Vacuum>();
        public Vacuum GetSlotVacuum(int idx) => _slotVacuums.TryGetValue(idx, out var v) ? v : null;

        private void BindIoDomains()
        {
            var eq = Equipment.Instance; var unit = eq?.UnitIO; if (unit == null) return;
            for (int i = 1; i <= 8; i++)
            {
                string vacOutName = $"INDEX {i} VACUUM"; // output definition exists
                string vacKeyOut = $"Rotary.Index{i}.VacOut";
                // Map output; sensor inputs for vacuum tank pressure are generic so skip ok input (no per-slot check -> no sensor) -> create dummy vacuum
                DIO.MapByName(unit, vacKeyOut, true, vacOutName);
                var vac = new Vacuum($"Index{i}", vacKeyOut, vacKeyOut + ".Ok/*NO_SENSOR*/");
                _slotVacuums[i] = vac;
            }
        }
        #endregion
    }
}