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
    public class IndexUnloadAligner : BaseUnit
    {
        public IndexUnloadAlignerConfig IndexUnloadAlignerConfig { get; private set; }
        public List<TeachingPosition> TeachingPositions { get; private set; } = new List<TeachingPosition>();

        public IndexUnloadAligner(IndexUnloadAlignerConfig config = null)
            : base("IndexUnloadAlignerConfig")
        {
            IndexUnloadAlignerConfig = config ?? new IndexUnloadAlignerConfig();
            AddComponents();
        }

        public override void AddComponents()
        {
            IndexUnloadAlignerConfig.LoadAndBindAxes(Equipment.Instance.AxisManager);
            IndexUnloadAlignerConfig.InitializeDefaultTeachingPositions();
            TeachingPositions.Clear();
            foreach (var tp in IndexUnloadAlignerConfig.TeachingPositions)
                TeachingPositions.Add(tp);
            BindAxes();
        }

        public override void OnRun() => base.OnRun();
        public override void OnStop() => base.OnStop();

        public void TeachCurrentPosition(string positionName, string description = null)
        {
            var axisPositions = new Dictionary<string, double>();
            foreach (var axisPair in Axes)
                axisPositions[axisPair.Key] = axisPair.Value.GetPosition();
            var tp = new TeachingPosition(positionName, axisPositions, description);
            IndexUnloadAlignerConfig.SetTeachingPosition(tp);
        }

        public int MoveToTeachingPosition(string positionName, double vel = 5, double acc = 10, double dec = 10, double jerk = 50)
        {
            var tp = IndexUnloadAlignerConfig.GetTeachingPosition(positionName);
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
        private readonly List<MotionAxis> _axes = new List<MotionAxis>();
        public IReadOnlyList<MotionAxis> BoundAxes => _axes;
        private void BindAxes()
        {
            _axes.Clear();
            foreach (var kv in Axes) _axes.Add(kv.Value);
        }
        public double GetTP(string tpName, string axisName)
        {
            var tp = IndexUnloadAlignerConfig.GetTeachingPosition(tpName);
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

        #region IO Helpers (No IO defined)
        public bool ReadInput(string name) => false;
        public bool WriteOutput(string name, bool on) => false;
        #endregion
    }
}