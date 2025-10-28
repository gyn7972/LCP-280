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
        //public bool UseAutoZeroTracking { get; set; };
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
            MaxVoltage = 0.5;
            MinForce = 0;
            MaxForce = 11;
            ReadChannelName = "";
            UseLowPassFilter = false;
            LowPassFilterCutoffFrequency = 0.3;
            IsSimulation = true;
            //UseAutoZeroTracking = false;
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
            pc.Add("Min Voltage", "V", MinVoltage);
            pc.Add("Max Voltage", "V", MaxVoltage);
            pc.Add("Min Force", "g", MinForce);
            pc.Add("Max Force", "g", MaxForce);
            pc.Add("Read Channel Name", "", ReadChannelName);
            pc.Add("Use Low Pass Filter", "", UseLowPassFilter);
            pc.Add("LowPassFilterCutoffFrequency", "", LowPassFilterCutoffFrequency);
            //pc.Add("UseAutoZeroTracking", "", UseAutoZeroTracking);
            pc.Add("IsSimulation", "", IsSimulation);
            return pc;
        }
        public override int ApplyValueFromPropertyCollection(PropertyCollection pc)
        {
            if (pc == null)
                return -1;

            try
            {
                MinVoltage = pc.GetValue<double>("Min Voltage");
                MaxVoltage = pc.GetValue<double>("Max Voltage");
                MinForce = pc.GetValue<double>("Min Force");
                MaxForce = pc.GetValue<double>("Max Force");
                ReadChannelName = pc.GetValue<string>("Read Channel Name");
                UseLowPassFilter = pc.GetValue<bool>("Use Low Pass Filter");
                LowPassFilterCutoffFrequency = pc.GetValue<double>("Low Pass Filter Cutoff Frequency");
                //UseAutoZeroTracking = pc.GetValue<bool>("Use Auto Zero Tracking");
                IsSimulation = pc.GetValue<bool>("Is Simulation");
            }
            catch (Exception)
            {
                return -1;
            }

            return 0;
        }
        #endregion
    }
}
