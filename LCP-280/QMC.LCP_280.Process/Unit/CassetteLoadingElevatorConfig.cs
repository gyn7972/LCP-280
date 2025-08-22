using QMC.Common.Unit;
using QMC.LCP_280.Process.Component;
using QMC.Common;

namespace QMC.LCP_280.Process.Unit
{
    public class CassetteLoadingElevatorConfig : BaseConfig
    {
        public double CassetteElevatorLoadingPosition { get; set; } = 100.0;
        public double CassetteElevatorUnloadingPosition { get; set; } = 10.0;
        public double CassetteSlotPitch { get; set; } = 20.0;
        public double WaferTransferArmReadyPosition { get; set; } = 15.0;
        public double WaferTransferArmAvoidPosition { get; set; } = 25.0;
        public double WaferTransferArmStagePosition { get; set; } = 30.0;
        public double WaferTransferArmCassettePosition { get; set; } = 35.0;
        public CassetteLoadingElevatorConfig()
        {
            PropertyPosition.AddDoubleProperty(nameof(CassetteElevatorLoadingPosition), CassetteElevatorLoadingPosition);
            PropertyPosition.AddDoubleProperty(nameof(CassetteElevatorUnloadingPosition), CassetteElevatorUnloadingPosition);
            PropertyPosition.AddDoubleProperty(nameof(CassetteSlotPitch), CassetteSlotPitch);
            PropertyPosition.AddDoubleProperty(nameof(WaferTransferArmReadyPosition), WaferTransferArmReadyPosition);
            PropertyPosition.AddDoubleProperty(nameof(WaferTransferArmAvoidPosition), WaferTransferArmAvoidPosition);
            PropertyPosition.AddDoubleProperty(nameof(WaferTransferArmStagePosition), WaferTransferArmStagePosition);
            PropertyPosition.AddDoubleProperty(nameof(WaferTransferArmCassettePosition), WaferTransferArmCassettePosition);
        }
    }
}