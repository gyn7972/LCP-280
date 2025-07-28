
using SP_GridTypeView.Component;
using SP_GridTypeView.Coponent;

namespace SP_GridTypeView.Unit
{
    public class CassetteLoadingElevator : BaseUnit, ICassetteElevatorUnit
    {
        public CassetteElevator CassetteElevator { get; private set; }
        public WaferSlotScanner WaferSlotScanner { get; private set; }
        public WaferTransferArm WaferTransferArm { get; private set; }

        public CassetteLoadingElevator()
        {
        }

        public override void AddComponents()
        {
            CassetteElevator = new CassetteElevator();
            WaferSlotScanner = new WaferSlotScanner();
            WaferTransferArm = new WaferTransferArm();

            CassetteElevator.ParentUnit = this;
            WaferSlotScanner.ParentUnit = this;
            WaferTransferArm.ParentUnit = this;

            Components.Add(CassetteElevator);
            Components.Add(WaferSlotScanner);
            Components.Add(WaferTransferArm);
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