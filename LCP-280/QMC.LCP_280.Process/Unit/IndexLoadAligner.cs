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
    public class IndexLoadAligner : BaseUnit
    {
        public IndexLoadAlignerConfig IndexLoadAlignerConfig { get; private set; }
        public List<TeachingPosition> TeachingPositions { get; private set; } = new List<TeachingPosition>();

        public IndexLoadAligner(IndexLoadAlignerConfig config = null)
            : base("IndexLoadAlignerConfig")
        {
            IndexLoadAlignerConfig = config ?? new IndexLoadAlignerConfig();
            AddComponents();
        }

        public override void AddComponents()
        {
            IndexLoadAlignerConfig.LoadAndBindAxes(Equipment.Instance.AxisManager);
            IndexLoadAlignerConfig.InitializeDefaultTeachingPositions();
            TeachingPositions.Clear();
            foreach (var tp in IndexLoadAlignerConfig.TeachingPositions)
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
            IndexLoadAlignerConfig.SetTeachingPosition(tp);
        }

        public int MoveToTeachingPosition(string positionName, double vel = 5, double acc = 10, double dec = 10, double jerk = 50)
        {
            var tp = IndexLoadAlignerConfig.GetTeachingPosition(positionName);
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
        private MotionAxis _alignT, _indexZ;
        public MotionAxis AlignT => _alignT;
        public MotionAxis IndexZ => _indexZ;
        private void BindAxes()
        {
            Axes.TryGetValue("Align T Axis", out _alignT);
            Axes.TryGetValue("Index Z Axis", out _indexZ);
        }
        public double GetTP(string tpName, string axisName)
        {
            var tp = IndexLoadAlignerConfig.GetTeachingPosition(tpName);
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

        #region IO Helpers (No Hard IO defined in config)
        public bool ReadInput(string name) => false;
        public bool WriteOutput(string name, bool on) => false;
        #endregion
    }
}