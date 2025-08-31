using QMC.Common.Unit;
using QMC.Common;
using QMC.Common.Motions;

namespace QMC.LCP_280.Process.Unit
{
    public class InputStageEjector : BaseUnit
    {
        public InputStageEjectorConfig InputStageEjectorConfig { get; private set; }

        public InputStageEjector(InputStageEjectorConfig config = null)
            : base("InputStageEjector")
        {
            InputStageEjectorConfig = config ?? new InputStageEjectorConfig();
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
