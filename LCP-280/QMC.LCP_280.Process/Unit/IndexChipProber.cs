using QMC.Common.Unit;
using QMC.Common;
using QMC.Common.Motions;

namespace QMC.LCP_280.Process.Unit
{
    public class IndexChipProber : BaseUnit
    {
        public IndexChipProberConfig IndexChipProberConfig { get; private set; }

        public IndexChipProber(IndexChipProberConfig config = null)
            : base("IndexChipProber")
        {
            IndexChipProberConfig = config ?? new IndexChipProberConfig();
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
