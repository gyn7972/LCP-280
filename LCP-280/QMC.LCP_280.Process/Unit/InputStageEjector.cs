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
    /// InputStageEjector Unit
    ///  - Ejector Z / Pin Z 두 축 Teaching + Offset 이동
    ///  - OutputStage / InputStage 구조와 통일된 Region 구성
    ///  - DryRun 옵션 (현재는 단순 플래그만 유지, 향후 IO 시뮬 추가 가능)
    /// </summary>
    public class InputStageEjector : BaseUnit
    {
        #region Nested Teaching Collection
        public class TeachingPositionCollection : List<TeachingPosition>
        {
            public TeachingPosition this[InputStageEjectorConfig.TeachingPositionName name]
            {
                get
                {
                    string key = name.ToString();
                    return this.FirstOrDefault(p => p != null && p.Name.Equals(key, System.StringComparison.OrdinalIgnoreCase));
                }
            }
        }
        #endregion

        #region Config / Teaching
        public InputStageEjectorConfig InputStageEjectorConfig { get; private set; }
        public TeachingPositionCollection TeachingPositions { get; private set; } = new TeachingPositionCollection();
        #endregion

        #region Axes
        private MotionAxis _axPinZ, _axEjectorZ;
        public MotionAxis AxisPinZ => _axPinZ;
        public MotionAxis AxisEjectorZ => _axEjectorZ;
        #endregion

        #region DryRun
        public bool DryRun { get; private set; }
        public void SetDryRun(bool on) => DryRun = on;
        #endregion

        #region ctor / Initialization
        public InputStageEjector(InputStageEjectorConfig config = null) : base("InputStageEjector")
        {
            InputStageEjectorConfig = config ?? new InputStageEjectorConfig();
            AddComponents();
        }

        public override void AddComponents()
        {
            InputStageEjectorConfig.LoadAndBindAxes(Equipment.Instance.AxisManager);
            InputStageEjectorConfig.InitializeDefaultTeachingPositions();

            TeachingPositions.Clear();
            foreach (var tp in InputStageEjectorConfig.TeachingPositions)
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
                Log.Write("InputCassetteLifter", "[BindAxes] AxisManager null");
                return;
            }

            const string unitName = "Unit"; // Equipment에서 축 등록 시 사용한 유닛명과 동일해야 함
            BindAxis(mgr, unitName, AxisNames.EjectPinZ, ref _axPinZ);
            BindAxis(mgr, unitName, AxisNames.EjectorZ, ref _axEjectorZ);
        }
        public void MoveAxisOnce(MotionAxis ax, double target)
        {
            if (ax == null) return;
            if (System.Math.Abs(ax.GetPosition() - target) > ax.Config.InposTolerance * 3)
                ax.MoveAbs(target, ax.Config.MaxVelocity, ax.Config.RunAcc, ax.Config.RunDec, ax.Config.AccJerkPercent);
        }
        public bool InPos(MotionAxis ax, double target) => ax == null || ax.InPosition(target);
        public double GetTP(string tpName, string axisKey)
        {
            var tp = InputStageEjectorConfig.GetTeachingPosition(tpName);
            if (tp != null && tp.AxisPositions != null && tp.AxisPositions.TryGetValue(axisKey, out var v)) return v;
            return 0.0;
        }
        public double GetTP(TeachingPosition tp, string axisKey) => (tp == null || string.IsNullOrEmpty(axisKey)) ? 0.0 : (tp.AxisPositions.TryGetValue(axisKey, out var v) ? v : 0.0);
        public double GetTP(TeachingPosition tp, MotionAxis axis) => axis == null ? 0.0 : GetTP(tp, axis.Name);
        #endregion

        #region Teaching
        public void TeachCurrentPosition(string positionName, string description = null)
        {
            var axisPositions = new Dictionary<string, double>();
            foreach (var axisPair in Axes)
                axisPositions[axisPair.Key] = axisPair.Value.GetPosition();
            var tp = new TeachingPosition(positionName, axisPositions, description);
            InputStageEjectorConfig.SetTeachingPosition(tp);
        }

        public int MoveToTeachingPosition(string positionName, double vel = 0, double acc = 0, double dec = 0, double jerk = 0)
        {
            var tp = InputStageEjectorConfig.GetTeachingPosition(positionName);
            if (tp == null) return -1;
            var (z, pz) = InputStageEjectorConfig.GetPositionWithOffset(positionName);
            int rc = 0;
            if (_axEjectorZ != null)
                rc |= _axEjectorZ.MoveAbs(z, vel > 0 ? vel : _axEjectorZ.Config.MaxVelocity, acc > 0 ? acc : _axEjectorZ.Config.RunAcc, dec > 0 ? dec : _axEjectorZ.Config.RunDec, jerk > 0 ? jerk : _axEjectorZ.Config.AccJerkPercent);
            if (_axPinZ != null)
                rc |= _axPinZ.MoveAbs(pz, vel > 0 ? vel : _axPinZ.Config.MaxVelocity, acc > 0 ? acc : _axPinZ.Config.RunAcc, dec > 0 ? dec : _axPinZ.Config.RunDec, jerk > 0 ? jerk : _axPinZ.Config.AccJerkPercent);
            return rc;
        }
        public int MoveToTeachingPosition(TeachingPosition tp, double vel = 0, double acc = 0, double dec = 0, double jerk = 0) => tp == null ? -1 : MoveToTeachingPosition(tp.Name, vel, acc, dec, jerk);
        public int MoveToTeachingPosition(InputStageEjectorConfig.TeachingPositionName name, double vel = 0, double acc = 0, double dec = 0, double jerk = 0) => MoveToTeachingPosition(name.ToString(), vel, acc, dec, jerk);

        public bool InPosTeaching(string positionName)
        {
            var (z, pz) = InputStageEjectorConfig.GetPositionWithOffset(positionName);
            return InPos(_axEjectorZ, z) && InPos(_axPinZ, pz);
        }
        public bool InPosTeaching(TeachingPosition tp) => tp != null && InPosTeaching(tp.Name);
        public bool InPosTeaching(InputStageEjectorConfig.TeachingPositionName name) => InPosTeaching(name.ToString());

        public void ApplyOffset(string positionName, double dzEjector, double dzPin) => InputStageEjectorConfig.SetOffset(positionName, dzEjector, dzPin);
        #endregion

        #region Lifecycle
        public override void OnRun() => base.OnRun();
        public override void OnStop() => base.OnStop();
        #endregion
    }
}