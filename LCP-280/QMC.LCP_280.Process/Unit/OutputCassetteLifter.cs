using QMC.Common;
using QMC.Common.Component;
using QMC.Common.Motion;
using QMC.Common.Motions;
using QMC.Common.Unit;
using QMC.LCP_280.Process.Component;
using System.Collections.Generic;
using System.Linq;

namespace QMC.LCP_280.Process.Unit
{
    /// <summary>
    /// OutputCassetteLifter Unit
    ///  - Z 축 리프팅 Teaching Position
    ///  - Cassette / RingJut / Mapping 센서 상태 제공
    ///  - OutputStage 와 유사한 구조 (Axis / IO / Teaching / Lifecycle)
    /// </summary>
    public class OutputCassetteLifter : BaseUnit
    {
        #region Config / Teaching
        public OutputCassetteLifterConfig OutputCassetteLifterConfig { get; private set; }
        public List<TeachingPosition> TeachingPositions { get; private set; } = new List<TeachingPosition>();
        #endregion

        #region Axis
        private MotionAxis _axZ;
        public MotionAxis AxisZ => _axZ;
        #endregion

        #region ctor / Initialization
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
        }
        #endregion

        #region Axis Binding / Helpers
        private void BindAxes()
        {
            var mgr = Equipment.Instance?.AxisManager;
            if (mgr == null)
            {
                Log.Write("OutputCassetteLifter", "[BindAxes] AxisManager null");
                return;
            }

            const string unitName = "Unit"; // Equipment에서 축 등록 시 사용한 유닛명과 동일해야 함
            BindAxis(mgr, unitName, AxisNames.BinLifterZ, ref _axZ);
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
            var tp = OutputCassetteLifterConfig.GetTeachingPosition(tpName);
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
        public bool InPosTeaching(string positionName)
        {
            var tp = OutputCassetteLifterConfig.GetTeachingPosition(positionName);
            if (tp == null) return false;
            double z = tp.AxisPositions.TryGetValue("Bin Lifter Z Axis", out var vz) ? vz : 0;
            return InPos(_axZ, z);
        }
        #endregion

        #region IO / Sensors
        public bool ReadInput(string name)
        {
            var hi = OutputCassetteLifterConfig.HardInputs?.FirstOrDefault(i => i.Name.Equals(name, System.StringComparison.OrdinalIgnoreCase));
            if (hi == null) return false;
            var eq = Equipment.Instance; var dio = eq?.DioScan; if (dio == null) return false;
            foreach (var m in eq.UnitIO.Modules)
                if (dio.TryGetInput(m.ModuleName, hi.Disp, out var v)) return v;
            return false;
        }

        public bool CassettePresent0() => ReadInput(OutputCassetteLifterConfig.IO.CASSETTE_CHECK0);
        public bool CassettePresent1() => ReadInput(OutputCassetteLifterConfig.IO.CASSETTE_CHECK1);
        public bool AnyCassettePresent() => CassettePresent0() || CassettePresent1();
        public bool RingJut() => !ReadInput(OutputCassetteLifterConfig.IO.RING_JUT_CHECK);
        public bool MappingSensor() => ReadInput(OutputCassetteLifterConfig.IO.MAPPING_SENSOR);
        #endregion

        #region Lifecycle
        public override void OnRun()  => base.OnRun();
        public override void OnStop() => base.OnStop();
        #endregion
    }
}