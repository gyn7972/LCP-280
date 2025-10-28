using QMC.Common.Unit;

namespace QMC.Common.Vision
{
    public sealed class VisionUnit : BaseUnit<VisionConfig>
    {
        public VisionUnit(VisionConfig config) : base(config)
        {
        }

        public override void AddComponents()
        {
            // Vision-specific initialization
        }
    }
}
