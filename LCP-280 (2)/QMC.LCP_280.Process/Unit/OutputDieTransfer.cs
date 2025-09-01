using QMC.Common.Unit;
using QMC.Common;
using QMC.Common.Motions;

namespace QMC.LCP_280.Process.Unit
{
    public class OutputDieTransfer : BaseUnit
    {
        public OutputDieTransferConfig OutputDieTransferConfig { get; private set; }

        public OutputDieTransfer(OutputDieTransferConfig config = null)
            : base("OutputDieTransfer")
        {
            OutputDieTransferConfig = config ?? new OutputDieTransferConfig();
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
