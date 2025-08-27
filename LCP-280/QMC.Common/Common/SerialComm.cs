using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QMC.Common.Common
{
    public class SerialComm
    {
        #region Field
        protected SerialPort serialPort = new SerialPort();
        protected int conversationTimeout = 1000; // ms
        #endregion

        #region Property
        public string PortName
        {
            get => serialPort.PortName;
            set => serialPort.PortName = value;
        }
        public int BaudRate
        {
            get => serialPort.BaudRate;
            set => serialPort.BaudRate = value;
        }
        public int DataBits
        {
            get => serialPort.DataBits;
            set => serialPort.DataBits = value;
        }
        public Parity Parity
        {
            get => serialPort.Parity;
            set => serialPort.Parity = value;
        }
        public StopBits StopBits
        {
            get => serialPort.StopBits;
            set => serialPort.StopBits = value;
        }
        public Handshake Handshake
        {
            get => serialPort.Handshake;
            set => serialPort.Handshake = value;
        }
        public int ConversationTimeout
        {
            get => conversationTimeout;
            set
            {
                conversationTimeout = value;
                serialPort.ReadTimeout = value;
                serialPort.WriteTimeout = value;
            }
        }
        public int WaitRecvDelay { get; set; } = 10; // ms
        public string STXString { get; set; } = string.Empty;
        public string ETXString { get; set; } = "\r\n";
        public bool IsOpen => serialPort.IsOpen;
        #endregion

        #region Constructor
        public SerialComm()
        {
            // Default settings
            serialPort.BaudRate = 9600;
            serialPort.DataBits = 8;
            serialPort.Parity = Parity.None;
            serialPort.StopBits = StopBits.One;
            serialPort.Handshake = Handshake.None;
            serialPort.ReadTimeout = ConversationTimeout;
            serialPort.WriteTimeout = ConversationTimeout;
        }
        #endregion
        
        #region Connection Method
        public bool Open()
        {             
            try
            {
                if (!serialPort.IsOpen)
                    serialPort.Open();
                return true;
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                return false;
            }
        }

        public void Close()
        {
            try
            {
                if (serialPort.IsOpen)
                    serialPort.Close();
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
        }
        #endregion

        #region Send
        public bool Send(string data)
        {
            try
            {
                if (!serialPort.IsOpen)
                    throw new Exception($"Serial port {PortName} is not open.");

                serialPort.Write(data);
                return true;
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                return false;
            }
        }
        #endregion

        #region Recv
        public bool Recv(out string data)
        {
            data = string.Empty;
            try
            {
                if (!serialPort.IsOpen)
                    throw new Exception($"Serial port {PortName} is not open.");

                if (WaitRecv() == false)
                    throw new TimeoutException($"Serial port {PortName} read timeout.");

                byte[] stxBytes = Encoding.ASCII.GetBytes(STXString);
                byte[] etxBytes = Encoding.ASCII.GetBytes(ETXString);

                if (!ReceiveData(out byte[] recvBytes, stxBytes, etxBytes))
                    throw new Exception($"Serial port {PortName} read error.");

                if (recvBytes == null)
                    data = string.Empty;
                else
                    data = Encoding.ASCII.GetString(recvBytes);
                return true;
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                return false;
            }
        }

        private bool ReceiveData(out byte[] value, byte[] stxBytes, byte[] etxBytes)
        {
            string empty = string.Empty;
            bool flag = false;
            List<byte> listBytes = new List<byte>();
            value = null;

            // Check etxBytes validity
            if (etxBytes == null || etxBytes.Length < 1)
            {
                return false;
            }

            int num2;
            if (stxBytes != null)
            {
                num2 = ((stxBytes.Length == 0) ? 1 : 0);
            }
            else
            {
                num2 = 1;
            }
            if (num2 != 0)
            {
                flag = true;
            }

            while (true)
            {
                byte byteData = 0;
                if (ReceiveByte(ref byteData) != true)
                {
                    value = new byte[listBytes.Count];
                    listBytes.CopyTo(value, 0);
                    return false;
                }

                listBytes.Add(byteData);
                if (!flag)
                {
                    if (listBytes.Count < stxBytes.Length)
                    {
                        continue;
                    }
                    bool flag2 = true;
                    for (int i = 0; i < stxBytes.Length; i++)
                    {
                        if (listBytes[listBytes.Count - stxBytes.Length + i] != stxBytes[i])
                        {
                            flag2 = false;
                        }
                    }
                    if (!flag2)
                    {
                        continue;
                    }

                    flag = true;
                    listBytes.Clear();
                    continue;
                }

                if (listBytes.Count < etxBytes.Length)
                {
                    continue;
                }

                bool flag3 = true;
                for (int j = 0; j < etxBytes.Length; j++)
                {
                    if (listBytes[listBytes.Count - etxBytes.Length + j] != etxBytes[j])
                    {
                        flag3 = false;
                    }

                }
                if (flag3)
                {
                    break;
                }
            }
            for (int k = 0; k < etxBytes.Length; k++)
            {
                listBytes.RemoveAt(listBytes.Count - 1);
            }

            value = new byte[listBytes.Count];
            listBytes.CopyTo(value, 0);
            return true;
        }
        
        private bool ReceiveByte(ref byte value)
        {
            byte[] data = null;
            TimeoutChecker timeoutChecker = new TimeoutChecker(ConversationTimeout, autoStart: true);
            while (true)
            {
                if (0 < serialPort.BytesToRead)
                {
                    data = new byte[1];
                    int readCount = serialPort.Read(data, 0, 1);
                    if (readCount != 1)
                    {
                        return false;
                    }

                    value = data[0];
                    return true;
                }
                if (timeoutChecker.IsCompleted || ConversationTimeout <= 0)
                {
                    break;
                }
                Thread.Sleep(1);
            }
            return false;
        }

        private bool WaitRecv()
        {
            DateTime startTime = DateTime.Now;
            while (serialPort.BytesToRead <= 0)
            {
                TimeSpan elapsed = DateTime.Now - startTime;
                if (elapsed.TotalMilliseconds > serialPort.ReadTimeout)
                    return false;
            }
            Thread.Sleep(WaitRecvDelay); // Wait for more data
            return true;
        }
        #endregion

        #region Query
        public bool Query(string send, out string recv)
        {
            recv = string.Empty;

            if (!Send(send))
                return false;
            if (!Recv(out recv))
                return false;

            return true;
        }
        #endregion   
    }
}
