using QMC.Common;
using QMC.Common.Component;
using QMC.Common.Motion;
using QMC.Common.Motions;
using QMC.Common.Unit;
using QMC.LCP_280.Process.Component;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QMC.LCP_280.Process.Unit
{
    /// <summary>
    /// IndexUnloadAligner Unit
    ///  - 다축(Align/Index 등) Teaching Positions
    ///  - OutputStage 패턴과 유사한 구조 (Axis / Teaching / Lifecycle)
    /// </summary>
    public class IndexUnloadAligner : BaseUnit
    {
        #region Config / Teaching
        public IndexUnloadAlignerConfig IndexUnloadAlignerConfig { get; private set; }
        public List<TeachingPosition> TeachingPositions { get; private set; } = new List<TeachingPosition>();
        #endregion

        #region Axes
        private MotionAxis _alingT, _indexZ;
        public MotionAxis AlingT => _alingT;
        public MotionAxis IndexZ => _indexZ;
        #endregion

        #region ctor / Initialization
        public IndexUnloadAligner(IndexUnloadAlignerConfig config = null) : base("IndexUnloadAlignerConfig")
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
        #endregion

        #region Axis Binding / Helpers
        private void BindAxes()
        {
            var mgr = Equipment.Instance?.AxisManager;
            if (mgr == null)
            {
                Log.Write("IndexUnloadAligner", "[BindAxes] AxisManager null");
                return;
            }

            const string unitName = "Unit"; // 축 등록 시 사용된 유닛명(Equipment.CreateAxes에서 동일)

            BindAxis(mgr, unitName, AxisNames.AlignT, ref _alingT);
            BindAxis(mgr, unitName, AxisNames.IndexZ, ref _indexZ);
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
            var tp = IndexUnloadAlignerConfig.GetTeachingPosition(tpName);
            if (tp != null && tp.AxisPositions != null && tp.AxisPositions.TryGetValue(axisName, out var v)) return v;
            return 0.0;
        }
        #endregion

        #region Teaching
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
        public bool InPosTeaching(string positionName)
        {
            var tp = IndexUnloadAlignerConfig.GetTeachingPosition(positionName);
            if (tp == null) return false;
            foreach (var kv in tp.AxisPositions)
                if (!Axes.TryGetValue(kv.Key, out var axis) || !InPos(axis, kv.Value)) return false;
            return true;
        }
        #endregion

        #region IO Placeholders
        public bool ReadInput(string name) => false; // No IO defined yet
        public bool WriteOutput(string name, bool on) => false; // No IO defined yet
        #endregion

        #region Lifecycle
        public override int OnRun() { int ret = 0; return ret; }
        public override int OnStop() { int ret = 0; base.OnStop(); return ret; }
        #endregion

        #region Seq 단위 동작 함수
        public int VisionAlign()
        {
            int nRet = -1;
            /* TODO */
            return nRet;
        }
        #endregion
    }
}