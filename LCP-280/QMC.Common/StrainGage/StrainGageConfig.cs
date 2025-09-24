using QMC.Common.Keithley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace QMC.Common.StrainGage
{
    public class StrainGageConfig : BaseConfig
    {
        #region Properties
        public double MinVoltage { get; set; }
        public double MaxVoltage { get; set; }
        public string LookupTableFilePath { get; set; }
        public string ReadChannelName { get; set; }
        #endregion

        #region Constructor
        public StrainGageConfig(string name) : base(name)
        {
        }
        #endregion

        #region Methods
        public override void Reset()
        {
            MinVoltage = -2.0;
            MaxVoltage = 2.0;
            LookupTableFilePath = "";
            ReadChannelName = "";
        }

        public override bool Validate()
        {
            if (string.IsNullOrWhiteSpace(LookupTableFilePath))
                return false;
            if (string.IsNullOrWhiteSpace(ReadChannelName))
                return false;
            if (MinVoltage > MaxVoltage)
                return false;

            return true;
        }
        public override PropertyCollection GetPropertyCollection()
        {
            PropertyCollection pc = new PropertyCollection();

            // Title
            pc.Add($"Strain Gage [{Name}] - Config");

            // Value
            pc.Add(nameof(MinVoltage), MinVoltage);
            pc.Add(nameof(MaxVoltage), MaxVoltage);
            pc.Add(nameof(LookupTableFilePath), LookupTableFilePath);
            pc.Add(nameof(ReadChannelName), ReadChannelName);

            return pc;
        }
        public override int ApplyValueFromPropertyCollection(PropertyCollection pc)
        {
            if (pc == null)
                return -1;

            try
            {
                MinVoltage = pc.GetValue<double>(nameof(MinVoltage));
                MaxVoltage = pc.GetValue<double>(nameof(MaxVoltage));
                LookupTableFilePath = pc.GetValue<string>(nameof(LookupTableFilePath));
                ReadChannelName = pc.GetValue<string>(nameof(ReadChannelName));
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                return -1;
            }

            return 0;
        }
        #endregion
    }
}
