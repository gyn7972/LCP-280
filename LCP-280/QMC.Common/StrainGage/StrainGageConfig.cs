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
        public double MinVoltage { get; set; } // V
        public double MaxVoltage { get; set; } // V
        public double MinForce { get; set; } // g
        public double MaxForce { get; set; } // g
        public string ReadChannelName { get; set; }
        public bool UseLowPassFilter { get; set; }
        public double LowPassFilterCutoffFrequency { get; set; }
        public bool UseAutoZeroSet { get; internal set; } = true;
        #endregion

        #region Constructor
        public StrainGageConfig(string name) : base(name)
        {
        }
        #endregion

        #region Methods
        public override void Reset()
        {
            MinVoltage = 0;
            MaxVoltage = 2.0;
            MinForce = 0;
            MaxForce = 10;
            ReadChannelName = "";
            UseLowPassFilter = true;
            LowPassFilterCutoffFrequency = 0.03;
        }

        public override bool Validate()
        {
            if (MinVoltage > MaxVoltage)
                return false;
            if (MinForce > MaxForce)
                return false;
            if (UseLowPassFilter == true)
            {
                if (!(0 <= LowPassFilterCutoffFrequency && LowPassFilterCutoffFrequency <= 1))
                    return false;
            }
            if (IsSimulation == false)
            {
                if (string.IsNullOrWhiteSpace(ReadChannelName))
                    return false;
            }
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
            pc.Add(nameof(MinForce), MinForce);
            pc.Add(nameof(MaxForce), MaxForce);
            pc.Add(nameof(ReadChannelName), ReadChannelName);
            pc.Add(nameof(UseLowPassFilter), UseLowPassFilter);
            pc.Add(nameof(LowPassFilterCutoffFrequency), LowPassFilterCutoffFrequency);
            pc.Add(nameof(IsSimulation), IsSimulation);
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
                MinForce = pc.GetValue<double>(nameof(MinForce));
                MaxForce = pc.GetValue<double>(nameof(MaxForce));
                ReadChannelName = pc.GetValue<string>(nameof(ReadChannelName));
                UseLowPassFilter = pc.GetValue<bool>(nameof(UseLowPassFilter));
                LowPassFilterCutoffFrequency = pc.GetValue<double>(nameof(LowPassFilterCutoffFrequency));
                IsSimulation = pc.GetValue<bool>(nameof(IsSimulation));
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
