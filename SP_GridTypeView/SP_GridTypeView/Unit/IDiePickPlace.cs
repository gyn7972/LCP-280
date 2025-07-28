using SP_GridTypeView.Coponent;

namespace SP_GridTypeView.Unit
{
    public interface IDiePickPlace
    {
        DiePicker DiePicker { get; }
        DiePlacer DiePlacer { get; }
    }
}