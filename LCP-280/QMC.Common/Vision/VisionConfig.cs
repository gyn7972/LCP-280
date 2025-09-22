using QMC.Common;
using System.ComponentModel;

namespace QMC.Common.Vision
{
    public class VisionConfig : BaseConfig
    {
        [Category("Camera"), DisplayName("Default Camera Key")]
        public string DefaultCameraKey { get; set; } = "In_Stage";

        public VisionConfig(string name = "VisionConfig") : base(name)
        {
            Reset();
        }

        public override void Reset()
        {
            DefaultCameraKey = "In_Stage";
        }

        public override bool Validate()
        {
            return !string.IsNullOrWhiteSpace(DefaultCameraKey);
        }
    }
}
