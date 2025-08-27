using QMC.Common.Common;
using QMC.Common.Component;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QMC.Common.BarcodeReader
{
    public class OpticonBarcodeReader : BaseComponent
    {
        #region Field
        private SerialComm communicator;
        #endregion

        #region Property
        public new OpticonBarcodeReaderConfig Config { get; private set; }
        #endregion

        #region Constructor
        public OpticonBarcodeReader(string name) : base(name)
        {
            Config = new OpticonBarcodeReaderConfig(name);

            communicator = new SerialComm();
            communicator.ETXString = "\r"; // ETX = 0x0d (CR)

            UpdateCommunicatorConfig();
        }
        #endregion

        #region Override Method
        public override int Initialize()
        {
            if (communicator.IsOpen)
            {
                communicator.Close();
            }

            UpdateCommunicatorConfig();
            if (!communicator.Open())
            {
                return -1;
            }

            return 0;
        }
        public override int Create()
        {
            // Do something if needed
            return 0;
        }
        public override void Close()
        {
            communicator.Close();
        }
        #endregion

        #region Method
        public int Read(out string data)
        {
            data = string.Empty;
            if (communicator == null || !communicator.IsOpen)
                return -1;

            string command = GetBarcodeReadStartCommandString();
            for (int i = 0; i < Config.RetryCount; i ++)
            {
                if (communicator.Send(command) != true)
                    return -2;
                // ETX = 0x0d (CR)
                if (communicator.Recv(out data) == true)
                    break;
            }
            if (communicator.Query(command, out data) != true)
                return -2;

            return 0;
        }
        public int ReadAbort()
        {
            if (communicator == null || !communicator.IsOpen)
                return -1;

            string command = GetBarcodeReadStopCommandString();
            if (communicator.Send(command) != true)
                return -2;

            return 0;
        }
        private string GetBarcodeReadStartCommandString()
        {
            byte[] cmdBytes = { 0x1b, 0x5a, 0x0d }; // [ESC] [Z] [CR]
            return Encoding.ASCII.GetString(cmdBytes);
        }
        private string GetBarcodeReadStopCommandString()
        {
            byte[] cmdBytes = { 0x1b, 0x59, 0x0d }; // [ESC] [Y] [CR]
            return Encoding.ASCII.GetString(cmdBytes);
        }

        private void UpdateCommunicatorConfig()
        {
            if (communicator != null)
            {
                communicator.PortName = Config.PortName;
                communicator.BaudRate = Config.BaudRate;
                communicator.DataBits = Config.DataBits;
                communicator.Parity = Config.Parity;
                communicator.StopBits = Config.StopBits;
                communicator.Handshake = Config.Handshake;
                communicator.ConversationTimeout = Config.ConversationTimeout;
            }
        }
        #endregion
    }
}
