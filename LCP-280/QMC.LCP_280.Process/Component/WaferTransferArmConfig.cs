using QMC.Common;

namespace QMC.LCP_280.Process.Component
{
    public class WaferTransferArmConfig : BaseConfig
    {
        // 암 위치 설정
        public double ReadyY { get; set; } = 15.0;
        public double AvoidY { get; set; } = 25.0;
        public double StageY { get; set; } = 30.0;
        public double CassetteY { get; set; } = 35.0;

        public override bool Validate()
        {
            return base.Validate();
        }

        public override void Reset()
        {
            ReadyY = 15.0;
            AvoidY = 25.0;
            StageY = 30.0;
            CassetteY = 35.0;

            base.Reset();
        }
    }
}