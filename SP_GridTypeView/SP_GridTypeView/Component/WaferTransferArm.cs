using SP_GridTypeView.Component;

namespace SP_GridTypeView.Coponent
{
    public class WaferTransferArm : BaseComponent
    {
        public WaferTransferArmConfig Config { get; private set; }

        public WaferTransferArm(WaferTransferArmConfig config = null) : base("WaferTransferArm")
        {
            Config = config ?? new WaferTransferArmConfig();
        }

        public void ExtendArm()
        {
            // Config의 ExtendPosition을 사용하여 암 확장
            // 실제 하드웨어 제어: Config.ExtendPosition 사용
        }

        public void RetractArm()
        {
            // Config의 RetractPosition을 사용하여 암 후퇴
            // 실제 하드웨어 제어: Config.RetractPosition 사용
        }

        public void PickWafer()
        {
            // Config의 PickPosition, VacuumOnDelayMs 등을 사용
            // 실제 하드웨어 제어: Config 설정값들 사용
        }

        public void PlaceWafer()
        {
            // Config의 PlacePosition, VacuumOffDelayMs 등을 사용
            // 실제 하드웨어 제어: Config 설정값들 사용
        }
    }
}