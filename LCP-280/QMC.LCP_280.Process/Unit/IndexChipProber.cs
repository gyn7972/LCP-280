using QMC.Common;
using QMC.Common.Keithley;
using QMC.Common.Motions;
using QMC.Common.PKGTester;
using QMC.Common.Spectrometer;
using QMC.Common.Unit;
using QMC.LCP_280.Process.Component;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization.Advanced;

namespace QMC.LCP_280.Process.Unit
{
    public class IndexChipProber : BaseUnit
    {
        public IndexChipProberConfig IndexChipProberConfig { get; private set; }
        public List<TeachingPosition> TeachingPositions { get; private set; } = new List<TeachingPosition>();

        // 측정 관련 ===================================
        public PKGTester Tester => Equipment.Instance.Tester;
        // ============================================

        public IndexChipProber(IndexChipProberConfig config = null)
            : base("IndexChipProberConfig")
        {
            IndexChipProberConfig = config ?? new IndexChipProberConfig();
            AddComponents();
        }

        public override void AddComponents()
        {
            IndexChipProberConfig.LoadAndBindAxes(Equipment.Instance.AxisManager);
            IndexChipProberConfig.InitializeDefaultTeachingPositions();
            TeachingPositions.Clear();
            foreach (var tp in IndexChipProberConfig.TeachingPositions)
                TeachingPositions.Add(tp);
            BindAxes();
        }

        public override int OnRun() { int ret = 0; return ret; }
        public override int OnStop() { int ret = 0; base.OnStop(); return ret; }
        protected override int OnRunReady() { return 0; }
        protected override int OnRunWork() { return 0; }
        protected override int OnRunComplete() { return 0; }

        public void TeachCurrentPosition(string positionName, string description = null)
        {
            var axisPositions = new Dictionary<string, double>();
            foreach (var axisPair in Axes)
                axisPositions[axisPair.Key] = axisPair.Value.GetPosition();
            var tp = new TeachingPosition(positionName, axisPositions, description);
            IndexChipProberConfig.SetTeachingPosition(tp);
        }

        public int MoveToTeachingPosition(string positionName, double vel = 5, double acc = 10, double dec = 10, double jerk = 50)
        {
            var tp = IndexChipProberConfig.GetTeachingPosition(positionName);
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
        // Prober config currently defines no hard-coded axis names → provide generic list binding
        private readonly List<MotionAxis> _boundAxes = new List<MotionAxis>();
        public IReadOnlyList<MotionAxis> BoundAxes => _boundAxes;
        private void BindAxes()
        {
            _boundAxes.Clear();
            foreach (var kv in Axes) _boundAxes.Add(kv.Value);
        }
        public double GetTP(string tpName, string axisName)
        {
            var tp = IndexChipProberConfig.GetTeachingPosition(tpName);
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
            // Config has no HardInputs defined (commented). Keep structure for future expansion.
            var hiArray = (IndexChipProberConfig as dynamic); // placeholder; returns none
            return false;
        }
        public bool WriteOutput(string name, bool on)
        {
            // No outputs defined.
            return false;
        }
        #endregion

        #region Seq 단위 동작 함수
        public int Measurement()
        {
            int nRet = -1;
            /* TODO */
            return nRet;
        }
        #endregion
    }
}