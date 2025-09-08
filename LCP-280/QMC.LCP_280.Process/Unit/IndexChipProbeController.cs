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
    /// IndexChipProbeController Unit
    ///  - Probe Z / Probe Card XYZ / Sphere Z 축 Teaching Positions
    ///  - Sphere Forward/Backward Cylinder + Probe Card Vacuum IO 바인딩
    ///  - OutputStage 구조 패턴 적용 (Regions / Helpers / High-Level API)
    /// </summary>
    public class IndexChipProbeController : BaseUnit
    {
        #region Config / Teaching
        public IndexChipProbeControllerConfig IndexChipProbeControllerConfig { get; private set; }
        public List<TeachingPosition> TeachingPositions { get; private set; } = new List<TeachingPosition>();
        #endregion

        #region Axes
        private MotionAxis _probeZ, _probeCardX, _probeCardY, _probeCardZ, _sphereZ;
        public MotionAxis ProbeZ => _probeZ;
        public MotionAxis ProbeCardX => _probeCardX;
        public MotionAxis ProbeCardY => _probeCardY;
        public MotionAxis ProbeCardZ => _probeCardZ;
        public MotionAxis SphereZ => _sphereZ;
        #endregion

        #region IO Domain Members
        private Cylinder _cylSphere;        // FWD / BWD Cylinder
        private Vacuum _vacProbeCard;       // Probe Card Vacuum
        #endregion

        #region Constants (Names)
        private const string NAME_SPHERE_FW = IndexChipProbeControllerConfig.IO.SPHERE_FW_VLV;
        private const string NAME_SPHERE_BW = IndexChipProbeControllerConfig.IO.SPHERE_BW_VLV;
        private const string NAME_PROBE_VAC = IndexChipProbeControllerConfig.IO.PROBE_VAC_VLV;
        private const string NAME_PROBE_VAC_OK = IndexChipProbeControllerConfig.IO.PROBE_VAC_OK;
        #endregion

        #region ctor / Initialization
        public IndexChipProbeController(IndexChipProbeControllerConfig config = null) : base("IndexChipProbeControllerConfig")
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
        #endregion

        #region Axis Binding / Helpers
        private void BindAxes()
        {
            Axes.TryGetValue("Probe Z Axis", out _probeZ);
            Axes.TryGetValue("Probe Card X Axis", out _probeCardX);
            Axes.TryGetValue("Probe Card Y Axis", out _probeCardY);
            Axes.TryGetValue("Probe Card Z Axis", out _probeCardZ);
            Axes.TryGetValue("Sphere Z Axis", out _sphereZ);
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
            var tp = IndexChipProbeControllerConfig.GetTeachingPosition(tpName);
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
                    if (r != 0) result = r;
                }
            }
            return result;
        }
        public bool InPosTeaching(string positionName)
        {
            var tp = IndexChipProbeControllerConfig.GetTeachingPosition(positionName);
            if (tp == null) return false;
            foreach (var kv in tp.AxisPositions)
                if (!Axes.TryGetValue(kv.Key, out var axis) || !InPos(axis, kv.Value)) return false;
            return true;
        }
        #endregion

        #region Low-Level IO Access
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
        public bool IsOutputOn(string name)
        {
            var ho = IndexChipProbeControllerConfig.HardOutputs.FirstOrDefault(o => o.Name.Equals(name, System.StringComparison.OrdinalIgnoreCase));
            if (ho == null) return false;
            var eq = Equipment.Instance; var dio = eq?.DioScan; if (dio == null) return false;
            foreach (var m in eq.UnitIO.Modules)
                if (dio.TryGetOutput(m.ModuleName, ho.Disp, out var v)) return v;
            return false;
        }
        #endregion

        #region IO Domain Mapping (Reorganized)
        private void BindIoDomains()
        {
            var eq = Equipment.Instance; var unit = eq?.UnitIO; if (unit == null) return;

            // Sphere Cylinder (Forward / Backward)
            DIO.MapByName(unit, "ProbeCtrl.SphereFwOut", true,  NAME_SPHERE_FW);
            DIO.MapByName(unit, "ProbeCtrl.SphereBwOut", true,  NAME_SPHERE_BW);
            DIO.MapByName(unit, "ProbeCtrl.SphereFwIn",  false, NAME_SPHERE_FW);
            DIO.MapByName(unit, "ProbeCtrl.SphereBwIn",  false, NAME_SPHERE_BW);
            _cylSphere = new Cylinder("ProbeSphere", "ProbeCtrl.SphereFwOut", "ProbeCtrl.SphereBwOut", "ProbeCtrl.SphereFwIn", "ProbeCtrl.SphereBwIn");

            // Probe Card Vacuum
            DIO.MapByName(unit, "ProbeCtrl.VacOut", true,  NAME_PROBE_VAC);
            DIO.MapByName(unit, "ProbeCtrl.VacOk",  false, NAME_PROBE_VAC_OK);
            _vacProbeCard = new Vacuum("ProbeCardVac", "ProbeCtrl.VacOut", "ProbeCtrl.VacOk");
        }

        // === Direct Valve Control (강제 구동) ===
        public void SetSphereFwdValve(bool on) => WriteOutput(NAME_SPHERE_FW, on);
        public bool IsSphereFwdValveOn()       => IsOutputOn(NAME_SPHERE_FW);
        public void SetSphereBwdValve(bool on) => WriteOutput(NAME_SPHERE_BW, on);
        public bool IsSphereBwdValveOn()       => IsOutputOn(NAME_SPHERE_BW);
        public void SetProbeVacValve(bool on)  => WriteOutput(NAME_PROBE_VAC, on);
        public bool IsProbeVacValveOn()        => IsOutputOn(NAME_PROBE_VAC);
        #endregion

        #region High-Level Actuator API (Backward compatible)
        //public bool SphereForward(int timeoutMs = 2000)  => _cylSphere?.Extend(timeoutMs) ?? false;
        //public bool SphereBackward(int timeoutMs = 2000) => _cylSphere?.Retract(timeoutMs) ?? false;
        //public void SphereAllOff() => _cylSphere?.AllOff();
        //public void ProbeVacOn()  => _vacProbeCard?.On();
        //public void ProbeVacOff() => _vacProbeCard?.Off();

        public bool ProbeVacOk()  => _vacProbeCard?.IsOk() ?? false;
        public bool IsSphereForward()  => ReadInput(NAME_SPHERE_FW);   // Forward sensor
        public bool IsSphereBackward() => ReadInput(NAME_SPHERE_BW);   // Backward sensor
        #endregion

        #region Lifecycle
        public override void OnRun()  => base.OnRun();
        public override void OnStop() => base.OnStop();
        #endregion
    }
}