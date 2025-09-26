using InstrumentSystems.CAS4;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Management.Instrumentation;
using System.Text;
using System.Threading.Tasks;

namespace QMC.Common.Spectrometer
{
    public class CASSpectrometerConfig : BaseConfig
    {
        #region Defines
        public enum DeviceInterface
        {
            None,
            PCI,
            Test,
            USB,
            PCIe,
            Ethernet
        };
        #endregion

        #region Property
        public DeviceInterface DeviceInterfaceType { get; set; }
        public int DeviceInterfaceOption { get; set; }
        public string ConfigFileName { get; set; }
        public string CalibFileName { get; set; }
        public int IntegrationTime { get; set; }
        public int Averages { get; set; }
        public int DensityFilter { get; set; }
        public int ColormetricStart { get; set; }
        public int ColormetricStop { get; set; }
        public int TriggerTimeout { get; set; }
        public bool UseHardwareTrigger { get; set; }
        public bool IsSimulated { get; set; }
        #endregion

        #region Constructor
        public CASSpectrometerConfig(string name) : base(name)
        {
        }
        #endregion

        #region Methods
        public override void Reset()
        {
            // Reset to default values
            DeviceInterfaceType = DeviceInterface.USB;
            DeviceInterfaceOption = 0;
            ConfigFileName = string.Empty;
            CalibFileName = string.Empty;
            IntegrationTime = 30;
            Averages = 1;
            DensityFilter = 0;
            ColormetricStart = 380;
            ColormetricStop = 780;
            TriggerTimeout = 5000;
            UseHardwareTrigger = false;
            IsSimulated = false;
        }
        public override bool Validate()
        {
            // Validate the configuration values
            if (DeviceInterfaceType == DeviceInterface.None)
                return false;
            if (DeviceInterfaceOption < 0)
                return false;
            if (string.IsNullOrEmpty(ConfigFileName) || string.IsNullOrEmpty(CalibFileName))
                return false;
            if (IntegrationTime <= 0 || Averages <= 0)
                return false;
            if (DensityFilter < 0)
                return false;
            if (ColormetricStart < 0 || ColormetricStop < 0 || ColormetricStart >= ColormetricStop)
                return false;
            if (TriggerTimeout < 0)
                return false;

            return true;
        }
        public override PropertyCollection GetPropertyCollection()
        {
            PropertyCollection pc = new PropertyCollection();

            // Title
            pc.Add($"Spectrometer [{Name}] - Config");

            // Value
            pc.Add(nameof(DeviceInterfaceType), DeviceInterfaceType);
            pc.Add(nameof(DeviceInterfaceOption), DeviceInterfaceOption);
            pc.Add(nameof(ConfigFileName), ConfigFileName);
            pc.Add(nameof(CalibFileName), CalibFileName);
            pc.Add(nameof(IntegrationTime), IntegrationTime);
            pc.Add(nameof(Averages), Averages);
            pc.Add(nameof(DensityFilter), DensityFilter);
            pc.Add(nameof(ColormetricStart), ColormetricStart);
            pc.Add(nameof(ColormetricStop), ColormetricStop);
            pc.Add(nameof(TriggerTimeout), TriggerTimeout);
            pc.Add(nameof(UseHardwareTrigger), UseHardwareTrigger);
            pc.Add(nameof(IsSimulated), IsSimulated);
            return pc;
        }

        public override int ApplyValueFromPropertyCollection(PropertyCollection pc)
        {
            if (pc == null)
                return -1;

            try
            {
                DeviceInterfaceType = pc.GetValue<DeviceInterface>(nameof(DeviceInterfaceType));
                DeviceInterfaceOption = pc.GetValue<int>(nameof(DeviceInterfaceOption));
                ConfigFileName = pc.GetValue<string>(nameof(ConfigFileName));
                CalibFileName = pc.GetValue<string>(nameof(CalibFileName));
                IntegrationTime = pc.GetValue<int>(nameof(IntegrationTime));
                Averages = pc.GetValue<int>(nameof(Averages));
                DensityFilter = pc.GetValue<int>(nameof(DensityFilter));
                ColormetricStart = pc.GetValue<int>(nameof(ColormetricStart));
                ColormetricStop = pc.GetValue<int>(nameof(ColormetricStop));
                TriggerTimeout = pc.GetValue<int>(nameof(TriggerTimeout));
                UseHardwareTrigger = pc.GetValue<bool>(nameof(UseHardwareTrigger));
                IsSimulated = pc.GetValue<bool>(nameof(IsSimulated));
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
