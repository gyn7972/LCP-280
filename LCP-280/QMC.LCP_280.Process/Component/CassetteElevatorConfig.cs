using QMC.Common;
using QMC.Common.Component;
using System;

namespace QMC.LCP_280.Process.Component
{
    public class CassetteElevatorConfig : BaseConfig
    {
        public double LoadingZ { get; set; } = 0;
        public double UnloadingZ { get; set; } = 50;
        public double ReadyZ { get; set; } = 25;

        public CassetteElevatorConfig() : base("CassetteElevatorConfig") { }

        public override bool Validate()
        {
            return base.Validate();
        }

        public override void Reset()
        {
            LoadingZ = 0;
            UnloadingZ = 50;
            ReadyZ = 25;

            base.Reset();
        }
    }
}
