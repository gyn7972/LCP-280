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
    /// IndexLoadAligner Unit
    ///  - Align T / Index Z 축 Teaching Positions 관리
    ///  - OutputStage 스타일 Region/메서드 구조 적용
    ///  - 현재 별도 IO 없음 (추후 필요 시 IO Mapping 추가)
    /// </summary>
    public class IndexLoadAligner : BaseUnit<IndexLoadAlignerConfig>
    {
        #region Config / Teaching
        
        
        #endregion

        #region Axes
        private MotionAxis _alignT, _indexZ;
        public MotionAxis AlignT => _alignT;
        public MotionAxis IndexZ => _indexZ;
        #endregion

        #region ctor / Initialization
        public IndexLoadAligner(IndexLoadAlignerConfig config = null) : base(new IndexLoadAlignerConfig())
        {
            
            AddComponents();
        }

        public override void AddComponents()
        {
            Config.LoadAndBindAxes(Equipment.Instance.AxisManager);
            Config.InitializeDefaultTeachingPositions();
            TeachingPositions.Clear();
            foreach (var tp in Config.TeachingPositions)
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
                Log.Write("IndexLoadAligner", "[BindAxes] AxisManager null");
                return;
            }

            const string unitName = "Unit"; // Equipment.CreateAxes 에서 사용한 유닛명과 동일해야 함
            BindAxis(mgr, unitName, AxisNames.AlignT, ref _alignT);
            BindAxis(mgr, unitName, AxisNames.IndexZ, ref _indexZ);
        }

        private void BindAxis(MotionAxisManager mgr, string unitName, string axisName, ref MotionAxis field)
        {
            MotionAxis axis;
            if (mgr.TryGet(unitName, axisName, out axis) && axis != null)
            {
                field = axis;
                Axes[axisName] = axis; // 사전에 일관 등록(이미 있으면 갱신)
            }
            else
            {
                if (Axes.ContainsKey(axisName))
                    Axes.Remove(axisName);
                Log.Write("IndexLoadAligner", $"[BindAxes] Axis '{unitName}||{axisName}' 미존재");
            }
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
            var tp = Config.GetTeachingPosition(tpName);
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
            Config.SetTeachingPosition(tp);
        }

        public int MoveToTeachingPosition(string positionName, double vel = 5, double acc = 10, double dec = 10, double jerk = 50)
        {
            var tp = Config.GetTeachingPosition(positionName);
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
            var tp = Config.GetTeachingPosition(positionName);
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
        protected override int OnRunReady() { return 0; }
        protected override int OnRunWork() { return 0; }
        protected override int OnRunComplete() { return 0; }
        #endregion

        #region Seq 단위 동작 함수
        public int VisionAlign()
        {
            int nRet = -1;
            /* TODO */
            return nRet;
        }

        public int M_Align()
        {
            int nRet = -1;
            /* TODO */
            return nRet;
        }
        #endregion
    }
}