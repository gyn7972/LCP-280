using QMC.Common.Unit;
using QMC.Common;
using QMC.Common.Motions;

namespace QMC.LCP_280.Process.Unit
{
    public class OutputStage : BaseUnit
    {
        public OutputStageConfig OutputStageConfig { get; private set; }

        public OutputStage(OutputStageConfig config = null)
            : base("OutputStage")
        {
            OutputStageConfig = config ?? new OutputStageConfig();
            AddComponents();
        }

        public override void AddComponents()
        {
        }

        public override void OnRun()
        {
            base.OnRun();
        }

        public override void OnStop()
        {
            base.OnStop();
        }
    }
}
