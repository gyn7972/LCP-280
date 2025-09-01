using QMC.Common.Unit;
using QMC.Common;
using QMC.Common.Motions;

namespace QMC.LCP_280.Process.Unit
{
    public class Rotary : BaseUnit
    {
        public RotaryConfig RotaryConfig { get; private set; }

        public Rotary(RotaryConfig config = null)
            : base("Rotary")
        {
            RotaryConfig = config ?? new RotaryConfig();
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
