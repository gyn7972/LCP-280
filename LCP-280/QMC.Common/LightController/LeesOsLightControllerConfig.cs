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

            // Title
            string title = $"LightController [{Name}] - Config";
            pc.Add(title);

            // Value
            pc.Add("Port Name", "", PortName);
            pc.Add("Baud Rate", "", BaudRate);
            pc.Add("Data Bits", "", DataBits);
            pc.Add("Parity", "", Parity);
            pc.Add("StopBits", "", StopBits);
            pc.Add("Handshake", "", Handshake);
            pc.Add("Reply Timeout", "ms", ReplyTimeout);

            return pc;
        }
        public override int ApplyValueFromPropertyCollection(PropertyCollection pc)
        {
            if (pc == null)
                return -1;

            try
            {
                PortName = pc.GetValue<string>("Port Name");
                BaudRate = pc.GetValue<int>("Baud Rate");
                DataBits = pc.GetValue<int>("Data Bits");
                Parity = pc.GetValue<Parity>("Parity");
                StopBits = pc.GetValue<StopBits>("Stop Bits");
                Handshake = pc.GetValue<Handshake>("Handshake");
                ReplyTimeout = pc.GetValue<int>("Reply Timeout");
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
