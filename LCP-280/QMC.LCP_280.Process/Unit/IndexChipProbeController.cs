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
            var mgr = Equipment.Instance?.AxisManager;
            if (mgr == null)
            {
                Log.Write("IndexChipProbeController", "[BindAxes] AxisManager null");
                return;
            }

            const string unitName = "Unit"; // 축 등록 시 사용된 유닛명(Equipment.CreateAxes에서 동일)

            BindAxis(mgr, unitName, AxisNames.ProbeZ, ref _probeZ);
            BindAxis(mgr, unitName, AxisNames.ProbeCardX, ref _probeCardX);
            BindAxis(mgr, unitName, AxisNames.ProbeCardY, ref _probeCardY);
            BindAxis(mgr, unitName, AxisNames.ProbeCardZ, ref _probeCardZ);
            BindAxis(mgr, unitName, AxisNames.SphereZ, ref _sphereZ);
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

            // Vacuum 별칭으로 조회만
            if (!IoAutoBindings.Vacuums.TryGetValue("ProbeCardVac", out _vacProbeCard))
            {
                Log.Write("IndexChipProbeController", "BindIoDomains", "Vacuums not found: ProbeCardVac");
            }

            if (!IoAutoBindings.Cylinders.TryGetValue("ProbeSphere", out _cylSphere))
            {
                Log.Write("IndexChipProbeController", "BindIoDomains", "Cylinder not found: ProbeSphere");
            }
        }

        // === Domain Control (표준 구동) ===
        public bool SetProbeVac(bool on)
        {
            if (_vacProbeCard == null) return false;
            if (on) _vacProbeCard.On();
            else   _vacProbeCard.Off();
            return true;
        }
        public bool SetSphereFB(bool bFwdBwd)
        {
            if (_cylSphere == null) return false;

            if (bFwdBwd)
            {
                return _cylSphere.Extend();
            }
            else
            {
                return _cylSphere.Retract();
            }
        }
        public bool ProbeVacOk() => _vacProbeCard?.IsOk() ?? false;
        public bool IsSphereForward() => ReadInput(NAME_SPHERE_FW);   // Forward sensor
        public bool IsSphereBackward() => ReadInput(NAME_SPHERE_BW);   // Backward sensor
        /////////////////////


        // === Direct Valve Control (강제 구동) ===
        public bool IsSphereFwdValveOn()       => IsOutputOn(NAME_SPHERE_FW);
        public bool IsProbeVacValveOn()        => IsOutputOn(NAME_PROBE_VAC);
        #endregion

        #region Lifecycle
        public override int OnRun()  { int ret = 0; return ret; }
        public override int OnStop() { int ret = 0; base.OnStop(); return ret; }
        protected override int OnRunReady() { return 0; }
        protected override int OnRunWork() { return 0; }
        protected override int OnRunComplete() { return 0; }
        #endregion

        #region Seq 단위 동작 함수

        public int TopContact()
        {
            int nRet = -1;
            /* TODO */
            return nRet;
        }

        public int BottomContact()
        {
            int nRet = -1;
            /* TODO */
            return nRet;
        }

        #endregion

    }
}