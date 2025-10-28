using Ivi.Visa;
using NationalInstruments.Visa;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QMC.Common.Keithley
{
    #region Defines
    public class KeithleyInstrumentResource
    {
        public string Name { get; set; }
        public HardwareInterfaceType InterfaceType { get; set; }
        public int InterfaceNumber { get; set; }
        public string ResourceClass { get; set; }

        public KeithleyInstrumentResource()
        {
            this.Name = "";
            this.InterfaceType = HardwareInterfaceType.Custom;
        }
        public KeithleyInstrumentResource(string name, HardwareInterfaceType interfaceType, int interfaceNumber, string resourceClass)
        {
            this.Name = name;
            this.InterfaceType = interfaceType;
            this.InterfaceNumber = interfaceNumber;
            this.ResourceClass = resourceClass;
        }
    }
    #endregion

    /// <summary>
    /// Keithley사 계측기 통신 리소스를 제공하는 클래스입니다. (NI-Visa 사용)
    /// </summary>
    public static class KeithleyInstrumentResourceManager
    {
        #region Method
        public static List<KeithleyInstrumentResource> FindAllInstrumentResources()
        {
            string pattern = "?*INSTR";
            return FindAllInstrumentResourcesFromPatternString(pattern);
        }
        public static List<KeithleyInstrumentResource> FindAllInstrumentResources(HardwareInterfaceType interfaceType)
        {
            string pattern = GetInstrumentPatternString(interfaceType);
            return FindAllInstrumentResourcesFromPatternString(pattern);
        }
        private static List<KeithleyInstrumentResource> FindAllInstrumentResourcesFromPatternString(string pattern)
        {
            List<KeithleyInstrumentResource> resouces = new List<KeithleyInstrumentResource>();
            try
            {
                // Create resource manager
                ResourceManager resourceManager = new ResourceManager();

                IEnumerable<string> findResults = resourceManager.Find(pattern);
                foreach (string s in findResults)
                {
                    ParseResult parseResult = resourceManager.Parse(s);

                    // Add Resource
                    string name = parseResult.OriginalResourceName;
                    HardwareInterfaceType interfaceType = parseResult.InterfaceType;
                    int interfaceNumber = parseResult.InterfaceNumber;
                    string resourceClass = parseResult.ResourceClass;
                    KeithleyInstrumentResource resource = new KeithleyInstrumentResource(name, interfaceType, interfaceNumber, resourceClass);
                    resouces.Add(resource);
                }
            }
            catch (Exception ex)
            {
                // Error handling
                resouces.Clear();
            }
            return resouces;
        }
        private static string GetInstrumentPatternString(HardwareInterfaceType interfaceType)
        {
            string pattern = string.Empty;
            switch (interfaceType)
            {
                case HardwareInterfaceType.Gpib:
                    pattern = "GPIB?*INSTR";
                    break;
                case HardwareInterfaceType.Vxi:
                    pattern = "VXI?*INSTR";
                    break;
                case HardwareInterfaceType.GpibVxi:
                    pattern = "(GPIB|VXI)?*INSTR";
                    break;
                case HardwareInterfaceType.Serial:
                    pattern = "ASRL?*INSTR";
                    break;
                case HardwareInterfaceType.Pxi:
                    pattern = "PXI?*INSTR";
                    break;
                case HardwareInterfaceType.Tcp:
                    pattern = "TCPIP?*INSTR";
                    break;
                case HardwareInterfaceType.Usb:
                    pattern = "USB?*INSTR";
                    break;
            }
            return pattern;
        }
        #endregion
    }
}
