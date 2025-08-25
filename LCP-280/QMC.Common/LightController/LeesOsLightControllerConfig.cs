using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace QMC.Common.LightController
{
    public class LeesOsLightControllerConfig : BaseConfig
    {
        #region Property
        public string PortName { get; set; }
        public int BaudRate { get; set; }
        public int DataBits { get; set; }
        public Parity Parity { get; set; }
        public StopBits StopBits { get; set; }
        public Handshake Handshake { get; set; }
        public int ReplyTimeout { get; set; }
        #endregion

        #region Constructor
        public LeesOsLightControllerConfig(string name) : base(name) 
        {
            Reset();
        }
        #endregion

        #region Method
        public override void Reset()
        {
            PortName = "";
            BaudRate = 9600;
            DataBits = 8;
            Parity = Parity.None;
            StopBits = StopBits.One;
            Handshake = Handshake.None;
            ReplyTimeout = 1000;
            base.Reset();
        }

        public override bool Validate()
        {
            if (string.IsNullOrEmpty(PortName)) 
                return false;
            if (BaudRate <= 0)
                return false;
            if (DataBits <= 0)
                return false;
            if (ReplyTimeout <= 0)
                return false;

            return true;
        }

        public override PropertyCollection GetPropertyCollection()
        {
            PropertyCollection pc = new PropertyCollection();
            PropertyBase p;

            // Title
            string title = $"LightController [{Name}] - Config";
            pc.Add(new TitleOnlyProperty(title));

            // Value
            p = new StringProperty("PortName", PortName);
            pc.Add(p);
            p = new IntProperty("BaudRate", BaudRate);
            pc.Add(p);
            p = new IntProperty("DataBits", DataBits);
            pc.Add(p);
            p = new ComboBoxProperty("Parity", Parity.ToString(), Enum.GetNames(typeof(Parity)).ToList());
            pc.Add(p);
            p = new ComboBoxProperty("StopBits", StopBits.ToString(), Enum.GetNames(typeof(StopBits)).ToList());
            pc.Add(p);
            p = new ComboBoxProperty("Handshake", Handshake.ToString(), Enum.GetNames(typeof(Handshake)).ToList());
            pc.Add(p);
            p = new IntProperty("ReplyTimeout", ReplyTimeout);
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
                        case "PortName":
                            PortName = prop.Value?.ToString() ?? "";
                            break;
                        case "BaudRate":
                            BaudRate = int.Parse(prop.Value?.ToString());
                            break;
                        case "DataBits":
                            DataBits = int.Parse(prop.Value?.ToString());
                            break;
                        case "Parity":
                            Parity = (Parity)Enum.Parse(typeof(Parity), prop.Value?.ToString());
                            break;
                        case "StopBits":
                            StopBits = (StopBits)Enum.Parse(typeof(StopBits), prop.Value?.ToString());
                            break;
                        case "Handshake":
                            Handshake = (Handshake)Enum.Parse(typeof(Handshake), prop.Value?.ToString());
                            break;
                        case "ReplyTimeout":
                            ReplyTimeout = int.Parse(prop.Value?.ToString());
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
