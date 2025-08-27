using QMC.Common.Unit;
using QMC.Common;
using QMC.Common.Motions;

namespace QMC.LCP_280.Process.Unit
{
    public class IndexProbeController : BaseUnit
    {
        public IndexProbeControllerConfig IndexProbeControllerConfig { get; private set; }

        public IndexProbeController(IndexProbeControllerConfig config = null)
            : base("IndexProbeController")
        {
            IndexProbeControllerConfig = config ?? new IndexProbeControllerConfig();
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
