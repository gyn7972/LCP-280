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
            pc.Add(nameof(PortName), PortName);
            pc.Add(nameof(BaudRate), BaudRate);
            pc.Add(nameof(DataBits), DataBits);
            pc.Add(nameof(Parity), Parity);
            pc.Add(nameof(StopBits), StopBits);
            pc.Add(nameof(Handshake), Handshake);
            pc.Add(nameof(ReplyTimeout), ReplyTimeout);

            return pc;
        }
        public override int ApplyValueFromPropertyCollection(PropertyCollection pc)
        {
            if (pc == null)
                return -1;

            try
            {
                PortName = pc.GetValue<string>(nameof(PortName));
                BaudRate = pc.GetValue<int>(nameof(BaudRate));
                DataBits = pc.GetValue<int>(nameof(DataBits));
                Parity = pc.GetValue<Parity>(nameof(Parity));
                StopBits = pc.GetValue<StopBits>(nameof(StopBits));
                Handshake = pc.GetValue<Handshake>(nameof(Handshake));
                ReplyTimeout = pc.GetValue<int>(nameof(ReplyTimeout));
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
