using QMC.Common.Unit;
using QMC.Common;
using QMC.Common.Motions;

namespace QMC.LCP_280.Process.Unit
{
    public class GageRnR : BaseUnit
    {
        public GageRnRConfig GageRnRConfig { get; private set; }

        public GageRnR(GageRnRConfig config = null)
            : base("GageRnR")
        {
            GageRnRConfig = config ?? new GageRnRConfig();
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
