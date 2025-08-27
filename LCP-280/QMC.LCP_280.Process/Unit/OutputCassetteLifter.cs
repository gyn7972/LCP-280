using QMC.Common.Unit;
using QMC.Common;
using QMC.Common.Motions;

namespace QMC.LCP_280.Process.Unit
{
    public class OutputCassetteLifter : BaseUnit
    {
        public OutputCassetteLifterConfig OutputCassetteLifterConfig { get; private set; }

        public OutputCassetteLifter(OutputCassetteLifterConfig config = null)
            : base("OutputCassetteLifter")
        {
            OutputCassetteLifterConfig = config ?? new OutputCassetteLifterConfig();
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
