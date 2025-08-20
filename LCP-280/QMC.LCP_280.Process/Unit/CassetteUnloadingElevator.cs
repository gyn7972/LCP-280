using QMC.Common.Unit;
using QMC.LCP_280.Process.Component;

namespace QMC.LCP_280.Process.Unit
{
    public class CassetteUnloadingElevator : BaseUnit, ICassetteElevatorUnit
    {
        public CassetteElevator CassetteElevator { get; private set; }
        public WaferSlotScanner WaferSlotScanner { get; private set; }
        public WaferTransferArm WaferTransferArm { get; private set; }

        public CassetteUnloadingElevator()
        {
            // Config를 직접 생성하거나 외부에서 주입 가능
            var elevatorConfig = new CassetteElevatorConfig();
            //elevatorConfig.LoadingPosition = 15.0;
            //elevatorConfig.UnloadingPosition = 25.0;
            //elevatorConfig.ScanningPosition = 20.0;

            var scannerConfig = new WaferSlotScannerConfig();
            scannerConfig.SlotCount = 25;
            scannerConfig.ScanSpeed = 3.0;

            var armConfig = new WaferTransferArmConfig();

            CassetteElevator = new CassetteElevator(elevatorConfig);
            WaferSlotScanner = new WaferSlotScanner(scannerConfig);
            WaferTransferArm = new WaferTransferArm(armConfig);

            // ParentUnit 설정
            CassetteElevator.ParentUnit = this;
            WaferSlotScanner.ParentUnit = this;
            WaferTransferArm.ParentUnit = this;

            // Components에 추가
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

        public void ValidateConfigs()
        {
            // 모든 Component의 Config 유효성 검사
            if (!CassetteElevator.Config.Validate())
            {
                // 알람 발생 또는 오류 처리
            }

            if (!WaferSlotScanner.Config.Validate())
            {
                // 알람 발생 또는 오류 처리
            }

            if (!WaferTransferArm.Config.Validate())
            {
                // 알람 발생 또는 오류 처리
            }
        }

        public override void OnRun()
        {
            base.OnRun();
            // Config 값들을 사용하여 동작 수행
        }

        public override void OnStop()
        {
            base.OnStop();
            // 필요시 동작 구현
        }
    }
}