using SP_GridTypeView.Component;
using SP_GridTypeView.Coponent;

namespace SP_GridTypeView.Unit
{
    public class CassetteUnloadingElevator : BaseUnit, ICassetteElevatorUnit
    {
        public CassetteElevator CassetteElevator { get; }
        public WaferSlotScanner WaferSlotScanner { get; }
        public WaferTransferArm WaferTransferArm { get; }

        public CassetteUnloadingElevator()
        {
            CassetteElevator = new CassetteElevator();
            WaferSlotScanner = new WaferSlotScanner();
            WaferTransferArm = new WaferTransferArm();
        }

        public override void OnRun()
        {
            base.OnRun();
            // 필요시 동작 구현
        }

        public override void OnStop()
        {
            base.OnStop();
            // 필요시 동작 구현
        }
    }
}