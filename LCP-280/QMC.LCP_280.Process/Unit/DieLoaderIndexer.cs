using QMC.Common.Unit;
using QMC.LCP_280.Process.Component;

namespace QMC.LCP_280.Process.Unit
{
    public class DieLoaderIndexer : BaseUnit, IDiePickPlace
    {
        public DiePicker DiePicker { get; }
        public DiePlacer DiePlacer { get; }

        public DieLoaderIndexer()
        {
            DiePicker = new DiePicker();
            DiePlacer = new DiePlacer();
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