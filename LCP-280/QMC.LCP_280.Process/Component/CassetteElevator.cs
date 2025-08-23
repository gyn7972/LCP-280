using QMC.Common;
using QMC.Common.Component;
using System;

namespace QMC.LCP_280.Process.Component
{
    public class CassetteElevator : BaseComponent
    {
        public CassetteElevatorConfig CassetteElevatorConfig { get; private set; }

        public CassetteElevator(CassetteElevatorConfig config = null)
            : base("CassetteElevator")
        {
            CassetteElevatorConfig = config ?? new CassetteElevatorConfig();
        }
    }
}