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

        #region NLV-5201 추가 설정

        /// <summary>
        /// 자동 트리거 모드 사용 여부 (기본값: false)
        /// </summary>
        public bool UseAutoTrigger { get; set; }

        /// <summary>
        /// 부저 사용 여부 (기본값: true)
        /// </summary>
        public bool EnableBuzzer { get; set; }

        /// <summary>
        /// 스캔 타임아웃 (초, 0이면 무제한, 기본값: 5초)
        /// </summary>
        public int ScanTimeout { get; set; }

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
            PortName = "COM1";
            BaudRate = 9600;
            DataBits = 8;
            Parity = Parity.None;
            StopBits = StopBits.One;
            Handshake = Handshake.None;
            ConversationTimeout = 1000;
            RetryCount = 1;

            // NLV-5201 추가 설정
            UseAutoTrigger = false;
            EnableBuzzer = true;
            ScanTimeout = 5;
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

            // NLV-5201 추가 검증
            if (ScanTimeout < 0)
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

            // NLV-5201 추가 설정들
            pc.Add(nameof(UseAutoTrigger), "", UseAutoTrigger);
            pc.Add(nameof(EnableBuzzer), "", EnableBuzzer);
            pc.Add(nameof(ScanTimeout), "", ScanTimeout);

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
                UseAutoTrigger = pc.GetValue<bool>(nameof(UseAutoTrigger));
                EnableBuzzer = pc.GetValue<bool>(nameof(EnableBuzzer));
                ScanTimeout = pc.GetValue<int>(nameof(ScanTimeout));
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                return -1;
            }

            return 0;
        }
        #endregion


         #region NLV-5201 유틸리티 메서드

        /// <summary>
        /// 사용 가능한 COM 포트 목록
        /// </summary>
        public static string[] GetAvailablePorts()
        {
            try
            {
                return SerialPort.GetPortNames();
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                return new string[0];
            }
        }

        /// <summary>
        /// NLV-5201 지원 통신 속도 목록
        /// </summary>
        public static int[] GetSupportedBaudRates()
        {
            return new int[]
            {
                300, 600, 1200, 2400, 4800, 9600,
                19200, 38400, 57600, 115200
            };
        }

        #endregion
    }
}
