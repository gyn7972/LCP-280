using QMC.Common.Unit;
using QMC.LCP_280.Process.Component;

namespace QMC.LCP_280.Process.Unit
{
    public interface IDiePickPlace
    {
        DiePicker DiePicker { get; }
        DiePlacer DiePlacer { get; }
    }
}