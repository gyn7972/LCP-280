using QMC.Common.Unit;
using QMC.Common;
using QMC.Common.Motions;

namespace QMC.LCP_280.Process.Unit
{
    public class InputDieTransfer : BaseUnit
    {
        public InputDieTransferConfig InputDieTransferConfig { get; private set; }

        public InputDieTransfer(InputDieTransferConfig config = null)
            : base("InputDieTransfer")
        {
            InputDieTransferConfig = config ?? new InputDieTransferConfig();
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
