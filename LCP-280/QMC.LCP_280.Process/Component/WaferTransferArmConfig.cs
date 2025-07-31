using QMC.Common.Component;

namespace QMC.LCP_280.Process.Component
{
    public class WaferTransferArmConfig : BaseConfig
    {
        // 암 위치 설정
        public double ExtendPosition { get; set; } = 100.0;
        public double RetractPosition { get; set; } = 0.0;
        public double PickPosition { get; set; } = 80.0;
        public double PlacePosition { get; set; } = 85.0;

        // 회전 설정
        public double RotationSpeed { get; set; } = 90.0; // degrees per second
        public double PickAngle { get; set; } = 0.0;
        public double PlaceAngle { get; set; } = 180.0;

        // 이동 설정
        public double MoveSpeed { get; set; } = 10.0;
        public double MoveAcceleration { get; set; } = 5.0;
        public int MoveTimeoutMs { get; set; } = 3000;

        // 진공 관련 설정
        public bool UseVacuum { get; set; } = true;
        public int VacuumOnDelayMs { get; set; } = 500;
        public int VacuumOffDelayMs { get; set; } = 200;
        public double VacuumThreshold { get; set; } = 0.8;

        // 안전 설정
        public double MinExtendPosition { get; set; } = -10.0;
        public double MaxExtendPosition { get; set; } = 150.0;
        public double PositionTolerance { get; set; } = 0.5;

        public WaferTransferArmConfig() : base("WaferTransferArmConfig")
        {
        }

        public override bool Validate()
        {
            if (ExtendPosition < MinExtendPosition || ExtendPosition > MaxExtendPosition)
                return false;
            if (RetractPosition < MinExtendPosition || RetractPosition > MaxExtendPosition)
                return false;
            if (MoveSpeed <= 0 || MoveAcceleration <= 0)
                return false;
            if (VacuumThreshold < 0 || VacuumThreshold > 1)
                return false;

            return base.Validate();
        }

        public override void Reset()
        {
            ExtendPosition = 100.0;
            RetractPosition = 0.0;
            PickPosition = 80.0;
            PlacePosition = 85.0;
            RotationSpeed = 90.0;
            PickAngle = 0.0;
            PlaceAngle = 180.0;
            MoveSpeed = 10.0;
            MoveAcceleration = 5.0;
            UseVacuum = true;
            VacuumOnDelayMs = 500;
            VacuumOffDelayMs = 200;
            VacuumThreshold = 0.8;

            base.Reset();
        }
    }
}