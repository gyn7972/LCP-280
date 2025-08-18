using QMC.Common.Component;


namespace QMC.LCP_280.Process.Component
{
    public class CassetteElevator : BaseComponent
    {
        public CassetteElevatorConfig Config { get; private set; }

        public CassetteElevator(CassetteElevatorConfig config = null) : base("CassetteElevator")
        {
            Config = config ?? new CassetteElevatorConfig();
        }
    }
}