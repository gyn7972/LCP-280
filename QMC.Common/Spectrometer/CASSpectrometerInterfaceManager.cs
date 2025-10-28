using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InstrumentSystems.CAS4;

namespace QMC.Common.Spectrometer
{
    #region Defines
    public struct CASSpectrometerInterfaceType
    {
        public string Name;
        public int Value;
        public List<CASSpectrometerInterfaceOption> Options;

        public CASSpectrometerInterfaceType(string name, int value)
        {
            this.Name = name;
            this.Value = value;
            this.Options = new List<CASSpectrometerInterfaceOption>();
        }
    }
    public struct CASSpectrometerInterfaceOption
    {
        public string Name;
        public int Value;

        public CASSpectrometerInterfaceOption(string name, int value)
        {
            this.Name = name;
            this.Value = value;
        }
    }
    public class CASSpectrometerInterface
    {
        #region Field & Property
        public List<CASSpectrometerInterfaceType> Types { get; private set; }
        #endregion

        #region Constructor
        public CASSpectrometerInterface()
        {
            this.Types = new List<CASSpectrometerInterfaceType>();
        }
        #endregion

        #region Methods
        public List<string> GetTypeNameList()
        {
            var typeNames = new List<string>();
            foreach (var type in this.Types)
            {
                typeNames.Add(type.Name);
            }
            return typeNames;
        }

        public List<string> GetOptionNameList(int type)
        {
            var optionNames = new List<string>();
            if (type < 0 || type >= this.Types.Count)
            {
                return optionNames; // Return empty list if type is invalid
            }

            foreach (var opt in this.Types[type].Options)
            {
                optionNames.Add(opt.Name);
            }
            return optionNames;
        }
        #endregion
    }
    #endregion

    /// <summary>
    /// Instrument System사 스펙트로미터 통신 리소스를 제공하는 클래스입니다.
    /// </summary>
    public class CASSpectrometerInterfaceManager
    {
        #region Field & Property
        #endregion

        #region Constructor
        public CASSpectrometerInterfaceManager()
        {
        }
        #endregion

        #region Methods
        public static CASSpectrometerInterface FindSupportInterface()
        {
            CASSpectrometerInterface supportInterface = new CASSpectrometerInterface();
            StringBuilder sb = new StringBuilder(256);

            int supportedTypeCount = CAS4DLL.casGetDeviceTypes();
            for (int type = 0; type < supportedTypeCount; type++)
            {
                CAS4DLL.casGetDeviceTypeName(type, sb, sb.Capacity);

                string typeName = sb.ToString();
                if (string.IsNullOrEmpty(typeName))
                {
                    continue;
                }

                CASSpectrometerInterfaceType interfaceType = new CASSpectrometerInterfaceType(typeName, type);
                supportInterface.Types.Add(interfaceType);

                int supportedOptionCount = CAS4DLL.casGetDeviceTypeOptions(type);
                for (int optIndex = 0; optIndex < supportedOptionCount; optIndex++)
                {
                    int option = CAS4DLL.casGetDeviceTypeOption(type, optIndex);
                    CAS4DLL.casGetDeviceTypeOptionName(type, option, sb, sb.Capacity);

                    string optionName = sb.ToString();
                    if (string.IsNullOrEmpty(optionName))
                    {
                        continue;
                    }

                    CASSpectrometerInterfaceOption interfaceTypeOption = new CASSpectrometerInterfaceOption(optionName, option);
                    interfaceType.Options.Add(interfaceTypeOption);
                }
            }
            return supportInterface;
        }
        #endregion
    }
}