using QMC.Common;
using QMC.Common.Component;
using QMC.Common.IOUtil;
using QMC.Common.Motion;
using QMC.Common.Motions;
using QMC.Common.Unit;
using QMC.LCP_280.Process.Component;
using System.Collections.Generic;

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
            // 축 바인딩까지 포함해서 불러오기
            IndexLoadAlignerConfig.LoadAndBindAxes(Equipment.Instance.AxisManager);
            IndexLoadAlignerConfig.InitializeDefaultTeachingPositions();

            // TeachingPosition에 Axis 바인딩
            TeachingPositions.Clear();
            foreach (var tp in IndexLoadAlignerConfig.TeachingPositions)
                TeachingPositions.Add(tp);
        }

        public override void OnRun()
        {
            base.OnRun();
        }

        public override void OnStop()
        {
            base.OnStop();
        }

        public void TeachCurrentPosition(string positionName, string description = null)
        {
            var axisPositions = new Dictionary<string, double>();
            foreach (var axisPair in Axes)
            {
                axisPositions[axisPair.Key] = axisPair.Value.GetPosition();
            }
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
                    if (r != 0) result = r; // 마지막 에러 반환
                }
            }
            return result;
        }
    }
}