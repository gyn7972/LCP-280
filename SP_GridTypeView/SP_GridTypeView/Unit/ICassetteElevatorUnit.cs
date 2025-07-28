using SP_GridTypeView.Component;
using SP_GridTypeView.Coponent;

namespace SP_GridTypeView.Unit
{
    public interface ICassetteElevatorUnit
    {
        CassetteElevator CassetteElevator { get; }
        CassetteMapper CassetteMapper { get; }
        WaferTransferArm WaferTransferArm { get; }
    }
}