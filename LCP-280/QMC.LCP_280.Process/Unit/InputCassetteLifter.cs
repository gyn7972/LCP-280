using QMC.Common.Unit;
using QMC.Common;
using QMC.Common.Motions;

namespace QMC.LCP_280.Process.Unit
{
    public class InputCassetteLifter : BaseUnit
    {
        public InputCassetteLifterConfig InputCassetteLifterConfig { get; private set; }

        public InputCassetteLifter(InputCassetteLifterConfig config = null)
            : base("InputCassetteLifterConfig")
        {
            InputCassetteLifterConfig = config ?? new InputCassetteLifterConfig();
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