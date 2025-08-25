using QMC.Common.Unit;
using QMC.LCP_280.Process.Component;
using QMC.Common;
using QMC.Common.Motions;

namespace QMC.LCP_280.Process.Unit
{
    public class CassetteLoadingElevator : BaseUnit
    {
        public CassetteElevator CassetteElevator { get; private set; }
        public CassetteLoadingElevatorConfig CassetteLoadingElevatorConfig { get; private set; }

        public CassetteLoadingElevator(CassetteLoadingElevatorConfig config = null)
            : base("CassetteLoadingElevator")
        {
            CassetteLoadingElevatorConfig = config ?? new CassetteLoadingElevatorConfig();
            AddComponents();
        }

        public override void AddComponents()
        {
            CassetteElevator = new CassetteElevator();

            CassetteElevator.ParentUnit = this;
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