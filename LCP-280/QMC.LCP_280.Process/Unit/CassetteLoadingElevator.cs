using QMC.Common.Unit;
using QMC.LCP_280.Process.Component;

namespace QMC.LCP_280.Process.Unit
{
    public class CassetteLoadingElevator : BaseUnit, ICassetteElevatorUnit
    {
        public CassetteElevator CassetteElevator { get; private set; }
        public WaferSlotScanner WaferSlotScanner { get; private set; }
        public WaferTransferArm WaferTransferArm { get; private set; }

        public CassetteLoadingElevator()
        {
            AddComponents();
        }

        public override void AddComponents()
        {
            // Config를 직접 생성하거나 외부에서 주입 가능
            var elevatorConfig = new CassetteElevatorConfig();
            //elevatorConfig.LoadingPosition = 10.0;
            //elevatorConfig.UnloadingPosition = 20.0;
            //elevatorConfig.ScanningPosition = 15.0;

            var scannerConfig = new WaferSlotScannerConfig();
            scannerConfig.SlotCount = 25;
            scannerConfig.ScanSpeed = 3.0;

            var armConfig = new WaferTransferArmConfig();

            CassetteElevator = new CassetteElevator(elevatorConfig);
            WaferSlotScanner = new WaferSlotScanner(scannerConfig);
            WaferTransferArm = new WaferTransferArm(armConfig);

            CassetteElevator.ParentUnit = this;
            WaferSlotScanner.ParentUnit = this;
            WaferTransferArm.ParentUnit = this;

            Components.Add(CassetteElevator);
            Components.Add(WaferSlotScanner);
            Components.Add(WaferTransferArm);
        }

        // Unit에서 Component의 Config에 자유롭게 접근하는 예시
        public void ConfigureComponents()
        {
            // WaferSlotScanner Config 접근 및 수정
            int slotCount = WaferSlotScanner.Config.SlotCount;
            WaferSlotScanner.Config.ScanSpeed = 5.0;
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