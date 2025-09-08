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
    public class InputStageEjector : BaseUnit
    {
        public class TeachingPositionCollection : List<QMC.LCP_280.Process.Component.TeachingPosition>
        {
            public QMC.LCP_280.Process.Component.TeachingPosition this[InputStageEjectorConfig.TeachingPositionName name]
            {
                get
                {
                    string key = name.ToString();
                    return this.FirstOrDefault(p => p != null && p.Name.Equals(key, System.StringComparison.OrdinalIgnoreCase));
                }
            }
        }

        public InputStageEjectorConfig InputStageEjectorConfig { get; private set; }
        public TeachingPositionCollection TeachingPositions { get; private set; } = new TeachingPositionCollection();

        private MotionAxis _axEjectorZ, _axPinZ;
        public MotionAxis AxisEjectorZ => _axEjectorZ;
        public MotionAxis AxisPinZ => _axPinZ;

        public bool DryRun { get; private set; }
        public void SetDryRun(bool on) => DryRun = on;

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

        private void BindAxes()
        {
            Axes.TryGetValue("EJECTOR_Z", out _axEjectorZ);
            Axes.TryGetValue("EJECT_PIN_Z", out _axPinZ);
            bool useInPos = !InputStageEjectorConfig.EnablePredictiveControl;
            foreach (var ax in new[] { _axEjectorZ, _axPinZ })
            {
                if (ax == null) continue;
                try
                {
                    var mi = ax.GetType().GetMethod("SetInPositionEnable");
                    var mr = ax.GetType().GetMethod("SetInPositionRange");
                    if (mi != null) mi.Invoke(ax, new object[] { useInPos });
                    if (mr != null) mr.Invoke(ax, new object[] { InputStageEjectorConfig.MoveDoneRemainDistance });
                }
                catch { }
            }
        }

        public override void OnRun() => base.OnRun();
        public override void OnStop() => base.OnStop();

        #region Teaching helpers
        public void TeachCurrentPosition(string positionName, string description = null)
        {
            var axisPositions = new Dictionary<string, double>();
            foreach (var axisPair in Axes)
                axisPositions[axisPair.Key] = axisPair.Value.GetPosition();
            var tp = new QMC.LCP_280.Process.Component.TeachingPosition(positionName, axisPositions, description);
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
        public int MoveToTeachingPosition(QMC.LCP_280.Process.Component.TeachingPosition tp, double vel = 0, double acc = 0, double dec = 0, double jerk = 0)
        {
            if (tp == null) return -1; return MoveToTeachingPosition(tp.Name, vel, acc, dec, jerk);
        }
        public int MoveToTeachingPosition(InputStageEjectorConfig.TeachingPositionName name, double vel = 0, double acc = 0, double dec = 0, double jerk = 0)
            => MoveToTeachingPosition(name.ToString(), vel, acc, dec, jerk);

        public bool InPosTeaching(string positionName)
        {
            var (z, pz) = InputStageEjectorConfig.GetPositionWithOffset(positionName);
            return InPos(_axEjectorZ, z) && InPos(_axPinZ, pz);
        }
        public bool InPosTeaching(QMC.LCP_280.Process.Component.TeachingPosition tp) => tp != null && InPosTeaching(tp.Name);
        public bool InPosTeaching(InputStageEjectorConfig.TeachingPositionName name) => InPosTeaching(name.ToString());

        public void ApplyOffset(string positionName, double dzEjector, double dzPin)
            => InputStageEjectorConfig.SetOffset(positionName, dzEjector, dzPin);
        #endregion

        #region Axis helpers
        public double GetTP(string tpName, string axisKey)
        {
            var tp = InputStageEjectorConfig.GetTeachingPosition(tpName);
            if (tp != null && tp.AxisPositions != null && tp.AxisPositions.TryGetValue(axisKey, out var v)) return v;
            return 0.0;
        }
        public double GetTP(QMC.LCP_280.Process.Component.TeachingPosition tp, string axisKey)
        {
            if (tp == null || string.IsNullOrEmpty(axisKey)) return 0.0;
            if (tp.AxisPositions != null && tp.AxisPositions.TryGetValue(axisKey, out var v)) return v;
            return 0.0;
        }
        public double GetTP(QMC.LCP_280.Process.Component.TeachingPosition tp, MotionAxis axis)
        {
            if (axis == null) return 0.0; return GetTP(tp, axis.Name);
        }

        public void MoveAxisOnce(MotionAxis ax, double target)
        {
            if (ax == null) return;
            if (System.Math.Abs(ax.GetPosition() - target) > ax.Config.InposTolerance * 3)
                ax.MoveAbs(target, ax.Config.MaxVelocity, ax.Config.RunAcc, ax.Config.RunDec, ax.Config.AccJerkPercent);
        }
        public bool InPos(MotionAxis ax, double target)
        {
            if (ax == null) return true; return ax.InPosition(target);
        }
        #endregion
    }
}