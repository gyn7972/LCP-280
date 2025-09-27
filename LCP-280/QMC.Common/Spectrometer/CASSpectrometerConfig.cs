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
            IsSimulation = false;
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
            pc.Add("Device Interface Type", "", DeviceInterfaceType);
            pc.Add("Device Interface Option", "", DeviceInterfaceOption);
            pc.Add("Config File Name", "", ConfigFileName);
            pc.Add("Calib File Name", "", CalibFileName);
            pc.Add("Integration Time", "ms", IntegrationTime);
            pc.Add("Averages", "", Averages);
            pc.Add("Density Filter", "", DensityFilter);
            pc.Add("Colormetric Start", "", ColormetricStart);
            pc.Add("Colormetric Stop", "", ColormetricStop);
            pc.Add("Trigger Timeout", "ms", TriggerTimeout);
            pc.Add("Use Hardware Trigger", "", UseHardwareTrigger);
            pc.Add("Is Simulation", "", IsSimulation);
            return pc;
        }

        public override int ApplyValueFromPropertyCollection(PropertyCollection pc)
        {
            if (pc == null)
                return -1;

            try
            {
                DeviceInterfaceType = pc.GetValue<DeviceInterface>("Device Interface Type");
                DeviceInterfaceOption = pc.GetValue<int>("Device Interface Option");
                ConfigFileName = pc.GetValue<string>("Config File Name");
                CalibFileName = pc.GetValue<string>("Calib File Name");
                IntegrationTime = pc.GetValue<int>("Integration Time");
                Averages = pc.GetValue<int>("Averages");
                DensityFilter = pc.GetValue<int>("Density Filter");
                ColormetricStart = pc.GetValue<int>("Colormetric Start");
                ColormetricStop = pc.GetValue<int>("Colormetric Stop");
                TriggerTimeout = pc.GetValue<int>("Trigger Timeout");
                UseHardwareTrigger = pc.GetValue<bool>("Use Hardware Trigger");
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
