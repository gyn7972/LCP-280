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
        public bool UseExternalTrigger { get; set; }
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
            UseExternalTrigger = false;
        }
        public override bool Validate()
        {
            // Validate the configuration values
            if (DeviceInterfaceType < 0 || DeviceInterfaceType < 0)
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
            pc.Add("DeviceInterfaceType", DeviceInterfaceType);
            pc.Add("DeviceInterfaceOption", DeviceInterfaceOption);
            pc.Add("ConfigFileName", ConfigFileName);
            pc.Add("CalibFileName", CalibFileName);
            pc.Add("IntegrationTime", IntegrationTime);
            pc.Add("Averages", Averages);
            pc.Add("DensityFilter", DensityFilter);
            pc.Add("ColormetricStart", ColormetricStart);
            pc.Add("ColormetricStop", ColormetricStop);
            pc.Add("TriggerTimeout", TriggerTimeout);
            pc.Add("UseExternalTrigger", UseExternalTrigger);
            return pc;
        }

        public override int ApplyValueFromPropertyCollection(PropertyCollection pc)
        {
            if (pc == null)
                return -1;

            try
            {
                DeviceInterfaceType = pc.GetValue<DeviceInterface>("DeviceInterfaceType");
                DeviceInterfaceOption = pc.GetValue<int>("DeviceInterfaceOption");
                ConfigFileName = pc.GetValue<string>("ConfigFileName");
                CalibFileName = pc.GetValue<string>("CalibFileName");
                IntegrationTime = pc.GetValue<int>("IntegrationTime");
                Averages = pc.GetValue<int>("Averages");
                DensityFilter = pc.GetValue<int>("DensityFilter");
                ColormetricStart = pc.GetValue<int>("ColormetricStart");
                ColormetricStop = pc.GetValue<int>("ColormetricStop");
                TriggerTimeout = pc.GetValue<int>("TriggerTimeout");
                UseExternalTrigger = pc.GetValue<bool>("UseExternalTrigger");
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
