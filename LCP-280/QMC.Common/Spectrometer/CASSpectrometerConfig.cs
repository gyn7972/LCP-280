using InstrumentSystems.CAS4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QMC.Common.Spectrometer
{
    public class CASSpectrometerConfig : BaseConfig
    {
        #region Property
        public int DeviceInterfaceType { get; set; }
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
            // Default constructor
        }
        #endregion

        #region Methods
        public override void Reset()
        {
            // Reset to default values
            DeviceInterfaceType = CAS4DLL.InterfaceUSB;
            DeviceInterfaceOption = 0;
            ConfigFileName = string.Empty;
            CalibFileName = string.Empty;
            IntegrationTime = 30;
            Averages = 1;
            DensityFilter = 0;
            ColormetricStart = 380;
            ColormetricStop = 780;
            TriggerTimeout = 10000;
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
        #endregion
    }
}
