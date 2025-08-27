using QMC.Common.Unit;
using QMC.Common;
using QMC.Common.Motions;

namespace QMC.LCP_280.Process.Unit
{
    public class InputStage : BaseUnit
    {
        public InputStageConfig InputStageConfig { get; private set; }

        public InputStage(InputStageConfig config = null)
            : base("InputStage")
        {
            InputStageConfig = config ?? new InputStageConfig();
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
