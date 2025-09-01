using QMC.Common.Unit;
using QMC.Common;
using QMC.Common.Motions;

namespace QMC.LCP_280.Process.Unit
{
    public class InputRingTransfer : BaseUnit
    {
        public InputRingTransferConfig InputRingTransferConfig { get; private set; }

        public InputRingTransfer(InputRingTransferConfig config = null)
            : base("InputRingTransfer")
        {
            InputRingTransferConfig = config ?? new InputRingTransferConfig();
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
