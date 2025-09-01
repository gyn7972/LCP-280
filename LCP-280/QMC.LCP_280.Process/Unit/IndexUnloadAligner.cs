using QMC.Common.Unit;
using QMC.Common;
using QMC.Common.Motions;

namespace QMC.LCP_280.Process.Unit
{
    public class IndexUnloadAligner : BaseUnit
    {
        public IndexUnloadAlignerConfig IndexUnloadAlignerConfig { get; private set; }

        public IndexUnloadAligner(IndexUnloadAlignerConfig config = null)
            : base("IndexUnloadAligner")
        {
            IndexUnloadAlignerConfig = config ?? new IndexUnloadAlignerConfig();
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
