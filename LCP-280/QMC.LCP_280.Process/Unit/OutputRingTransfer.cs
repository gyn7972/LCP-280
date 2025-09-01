using QMC.Common.Unit;
using QMC.Common;
using QMC.Common.Motions;

namespace QMC.LCP_280.Process.Unit
{
    public class OutputRingTransfer : BaseUnit
    {
        public OutputRingTransferConfig OutputRingTransferConfig { get; private set; }

        public OutputRingTransfer(OutputRingTransferConfig config = null)
            : base("OutputRingTransfer")
        {
            OutputRingTransferConfig = config ?? new OutputRingTransferConfig();
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
