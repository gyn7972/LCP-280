using InstrumentSystems.CAS4;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QMC.Common.Spectrometer
{
    public class CASSpectrometerConfig : BaseConfig
    {
        #region Defines
        //public const int InterfacePCI = 1;  ///<PCI interface constant. For use with e.g. <see cref="casCreateDeviceEx"/>. See chapter @ref interfaceTypesAndOptions.
        //public const int InterfaceTest = 3;  ///<Demo mode interface constant. For use with e.g. <see cref="casCreateDeviceEx"/>. See chapter @ref interfaceTypesAndOptions.
        //public const int InterfaceUSB = 5;  ///<USB interface constant. For use with e.g. <see cref="casCreateDeviceEx"/>. See chapter @ref interfaceTypesAndOptions.
        //public const int InterfacePCIe = 10; ///<PCIe interface constant. For use with e.g. <see cref="casCreateDeviceEx"/>. See chapter @ref interfaceTypesAndOptions.
        //public const int InterfaceEthernet

        public enum DeviceInterface
        {
            PCI = CAS4DLL.InterfacePCI,
            Test = CAS4DLL.InterfaceTest,
            USB = CAS4DLL.InterfaceUSB,
            PCIe = CAS4DLL.InterfacePCIe,
            Ethernet = CAS4DLL.InterfaceEthernet
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
        public override PropertyCollection GetPropertyCollection()
        {
            PropertyCollection pc = new PropertyCollection();
            PropertyBase p;

            // Title
            string title = $"Spectrometer [{Name}] - Config";
            pc.Add(new TitleOnlyProperty(title));

            // Value
            p = new ComboBoxProperty("DeviceInterfaceType", DeviceInterfaceType.ToString(), Enum.GetNames(typeof(DeviceInterface)).ToList());
            pc.Add(p);
            p = new IntProperty("DeviceInterfaceOption", DeviceInterfaceOption);
            pc.Add(p);
            p = new StringProperty("ConfigFileName", ConfigFileName);
            pc.Add(p);
            p = new StringProperty("CalibFileName", CalibFileName);
            pc.Add(p);
            p = new IntProperty("IntegrationTime", IntegrationTime);
            pc.Add(p);
            p = new IntProperty("Averages", Averages);
            pc.Add(p);
            p = new IntProperty("DensityFilter", DensityFilter);
            pc.Add(p);
            p = new IntProperty("ColormetricStart", ColormetricStart);
            pc.Add(p);
            p = new IntProperty("ColormetricStop", ColormetricStop);
            pc.Add(p);
            p = new IntProperty("TriggerTimeout", TriggerTimeout);
            pc.Add(p);
            p = new BoolProperty("UseExternalTrigger", UseExternalTrigger);
            pc.Add(p);

            return pc;
        }
        public override int ApplyValueFromPropertyCollection(PropertyCollection pc)
        {
            if (pc == null)
                return -1;

            foreach (var prop in pc)
            {
                try
                {
                    switch (prop.Title)
                    {
                        case "DeviceInterfaceType":
                            DeviceInterfaceType = (DeviceInterface)Enum.Parse(typeof(DeviceInterface), prop.Value?.ToString());
                            break;
                        case "DeviceInterfaceOption":
                            DeviceInterfaceOption = int.Parse(prop.Value?.ToString());
                            break;
                        case "ConfigFileName":
                            ConfigFileName = prop.Value?.ToString() ?? "";
                            break;
                        case "CalibFileName":
                            CalibFileName = prop.Value?.ToString() ?? "";
                            break;
                        case "IntegrationTime":
                            IntegrationTime = int.Parse(prop.Value?.ToString());
                            break;
                        case "Averages":
                            Averages = int.Parse(prop.Value?.ToString());
                            break;
                        case "DensityFilter":
                            DensityFilter = int.Parse(prop.Value?.ToString());
                            break;
                        case "ColormetricStart":
                            ColormetricStart = int.Parse(prop.Value?.ToString());
                            break;
                        case "ColormetricStop":
                            ColormetricStop = int.Parse(prop.Value?.ToString());
                            break;
                        case "TriggerTimeout":
                            TriggerTimeout = int.Parse(prop.Value?.ToString());
                            break;
                        case "UseExternalTrigger":
                            UseExternalTrigger = bool.Parse(prop.Value?.ToString());
                            break;
                        default:
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Log.Write(ex);
                    return -1;
                }
            }

            return 0;
        }
        #endregion
    }
}
