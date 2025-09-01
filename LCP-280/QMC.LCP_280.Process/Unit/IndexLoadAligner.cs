using QMC.Common.Unit;
using QMC.Common;
using QMC.Common.Motions;

namespace QMC.LCP_280.Process.Unit
{
    public class IndexLoadAligner : BaseUnit
    {
        public IndexLoadAlignerConfig IndexLoadAlignerConfig { get; private set; }

        public IndexLoadAligner(IndexLoadAlignerConfig config = null)
            : base("IndexLoadAligner")
        {
            IndexLoadAlignerConfig = config ?? new IndexLoadAlignerConfig();
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
