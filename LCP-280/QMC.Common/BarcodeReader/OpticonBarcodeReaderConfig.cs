using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;

namespace QMC.Common.BarcodeReader
{
    public class OpticonBarcodeReaderConfig : BaseConfig
    {
        #region Property
        public string PortName { get; set; }
        public int BaudRate { get; set; }
        public int DataBits { get; set; }
        public Parity Parity { get; set; }
        public StopBits StopBits { get; set; }
        public Handshake Handshake { get; set; }
        public int ConversationTimeout { get; set; }
        public int RetryCount { get; set; }
        #endregion

        #region Constructor
        public OpticonBarcodeReaderConfig(string name) : base(name)
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
            ConversationTimeout = 1000;
            RetryCount = 0;
        }
        public override bool Validate()
        {
            if (string.IsNullOrEmpty(PortName))
                return false;
            if (BaudRate <= 0)
                return false;
            if (DataBits <= 0)
                return false;
            if (ConversationTimeout <= 0)
                return false;
            if (RetryCount <= 0)
                return false;

            return true;
        }
        public override PropertyCollection GetPropertyCollection()
        {
            PropertyCollection pc = new PropertyCollection();

            // Title
            pc.Add($"BarcodeReader [{Name}] - Config");

            // Value
            pc.Add("Port Name", "", PortName);
            pc.Add("Baud Rate", "", BaudRate);
            pc.Add("Data Bits", "", DataBits);
            pc.Add("Parity", "", Parity);
            pc.Add("Stop Bits", "", StopBits);
            pc.Add("Handshake", "", Handshake);
            pc.Add("Conversation Timeout", "ms", ConversationTimeout);
            pc.Add("Retry Count", "", RetryCount);

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
                ConversationTimeout = pc.GetValue<int>("Conversation Timeout");
                RetryCount = pc.GetValue<int>("Retry Count");
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
