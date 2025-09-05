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
    public class IndexChipProbeController : BaseUnit
    {
        public IndexChipProbeControllerConfig IndexChipProbeControllerConfig { get; private set; }
        public List<TeachingPosition> TeachingPositions { get; private set; } = new List<TeachingPosition>();

        public IndexChipProbeController(IndexChipProbeControllerConfig config = null)
            : base("IndexChipProbeControllerConfig")
        {
            IndexChipProbeControllerConfig = config ?? new IndexChipProbeControllerConfig();
            AddComponents();
        }

        public override void AddComponents()
        {
            IndexChipProbeControllerConfig.LoadAndBindAxes(Equipment.Instance.AxisManager);
            IndexChipProbeControllerConfig.InitializeDefaultTeachingPositions();
            TeachingPositions.Clear();
            foreach (var tp in IndexChipProbeControllerConfig.TeachingPositions)
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
            IndexChipProbeControllerConfig.SetTeachingPosition(tp);
        }

        public int MoveToTeachingPosition(string positionName, double vel = 5, double acc = 10, double dec = 10, double jerk = 50)
        {
            var tp = IndexChipProbeControllerConfig.GetTeachingPosition(positionName);
            if (tp == null) return -1;
            int result = 0;
            foreach (var axisKey in tp.AxisPositions.Keys)
            {
                if (Axes.TryGetValue(axisKey, out var axis))
                {
                    double pos = tp.AxisPositions[axisKey];
                    int r = axis.MoveAbs(pos, vel, acc, dec, jerk);
                    if (r != 0) result = r; // 마지막 에러 저장
                }
            }
            return result;
        }

        #region Axis Helpers
        private MotionAxis _probeZ, _probeCardX, _probeCardY, _probeCardZ, _sphereZ;
        public MotionAxis ProbeZ => _probeZ;
        public MotionAxis ProbeCardX => _probeCardX;
        public MotionAxis ProbeCardY => _probeCardY;
        public MotionAxis ProbeCardZ => _probeCardZ;
        public MotionAxis SphereZ => _sphereZ;
        private void BindAxes()
        {
            Axes.TryGetValue("Probe Z Axis", out _probeZ);
            Axes.TryGetValue("Probe Card X Axis", out _probeCardX);
            Axes.TryGetValue("Probe Card Y Axis", out _probeCardY);
            Axes.TryGetValue("Probe Card Z Axis", out _probeCardZ);
            Axes.TryGetValue("Sphere Z Axis", out _sphereZ);
        }
        public double GetTP(string tpName, string axisName)
        {
            var tp = IndexChipProbeControllerConfig.GetTeachingPosition(tpName);
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
            var hi = IndexChipProbeControllerConfig.HardInputs.FirstOrDefault(i => i.Name.Equals(name, System.StringComparison.OrdinalIgnoreCase));
            if (hi == null) return false;
            var eq = Equipment.Instance; var dio = eq?.DioScan; if (dio == null) return false;
            foreach (var m in eq.UnitIO.Modules)
                if (dio.TryGetInput(m.ModuleName, hi.Disp, out var v)) return v;
            return false;
        }
        public bool WriteOutput(string name, bool on)
        {
            var ho = IndexChipProbeControllerConfig.HardOutputs.FirstOrDefault(o => o.Name.Equals(name, System.StringComparison.OrdinalIgnoreCase));
            if (ho == null) return false;
            var eq = Equipment.Instance; var dio = eq?.DioScan; if (dio == null) return false;
            foreach (var m in eq.UnitIO.Modules)
                if (dio.WriteOutput(m.ModuleName, ho.Disp, on) == 0) return true;
            return false;
        }
        #endregion

        #region IO Domain (Sphere Cylinder / Probe Card Vacuum)
        private Cylinder _sphereCylinder; // FW/BW
        private Vacuum _probeCardVacuum;  // Vacuum

        private const string NAME_SPHERE_FW = "SPHERE FW";
        private const string NAME_SPHERE_BW = "SPHERE BW";
        private const string NAME_PROBE_VAC = "PROBE CARD VACUUM";
        private const string NAME_PROBE_VAC_CHECK = "PROBE CARD VACUUM CHECK"; 

        private void BindIoDomains()
        {
            var eq = Equipment.Instance; var unit = eq?.UnitIO; if (unit == null) return;
            // Map outputs
            DIO.MapByName(unit, "Probe.SphereFwOut", true, NAME_SPHERE_FW);
            DIO.MapByName(unit, "Probe.SphereBwOut", true, NAME_SPHERE_BW);
            // Map inputs (reuse same names to get sensor state)
            DIO.MapByName(unit, "Probe.SphereFwIn", false, NAME_SPHERE_FW);
            DIO.MapByName(unit, "Probe.SphereBwIn", false, NAME_SPHERE_BW);
            _sphereCylinder = new Cylinder("Sphere", "Probe.SphereFwOut", "Probe.SphereBwOut", "Probe.SphereFwIn", "Probe.SphereBwIn");

            // Vacuum (use same name for out & ok; if later distinct name added, adjust here)
            DIO.MapByName(unit, "Probe.VacOut", true, NAME_PROBE_VAC);
            DIO.MapByName(unit, "Probe.VacOk", false, NAME_PROBE_VAC_CHECK);
            _probeCardVacuum = new Vacuum("Probe", "Probe.VacOut", "Probe.VacOk");
        }

        public bool SphereForward(int timeoutMs = 2000) => _sphereCylinder?.Extend(timeoutMs) ?? false;
        public bool SphereBackward(int timeoutMs = 2000) => _sphereCylinder?.Retract(timeoutMs) ?? false;
        public void SphereAllOff() => _sphereCylinder?.AllOff();
        public void ProbeVacOn() => _probeCardVacuum?.On();
        public void ProbeVacOff() => _probeCardVacuum?.Off();
        public bool ProbeVacOk() => _probeCardVacuum?.IsOk() ?? false;
        #endregion
    }
}