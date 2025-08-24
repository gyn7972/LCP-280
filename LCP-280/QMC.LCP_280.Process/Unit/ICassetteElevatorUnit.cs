using QMC.Common.Unit;
using QMC.LCP_280.Process.Component;

namespace QMC.LCP_280.Process.Unit
{
    public interface ICassetteElevatorUnit
    {
        CassetteElevator CassetteElevator { get; }
        WaferSlotScanner WaferSlotScanner { get; }
        WaferTransferArm WaferTransferArm { get; }
    }
}