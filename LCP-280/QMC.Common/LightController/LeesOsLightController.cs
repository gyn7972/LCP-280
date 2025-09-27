using NationalInstruments.DAQmx;
using QMC.Common.Common;
using QMC.Common.Component;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.AxHost;

namespace QMC.Common.LightController
{
    public enum LeesOsLightControllerModel
    {
        LPD_3024_1CH,
        LPD_3024_2CH,
        LPD_4024_3CH,
        LPD_4024_4CH,
        LPD_6524_2CH,
        LPD_6524_4CH,
        LPD_12024_8CH,
    }

    public class LeesOsLightController : BaseComponent
    {
        #region Field
        private SerialComm communicator;
        #endregion

        #region Property
        public LeesOsLightControllerModel Model { get; private set; }
        public int MaximumVolume { get; private set; }
        public List<LeesOsLightControllerChannel> Channels { get; private set; }
        public new LeesOsLightControllerConfig Config { get; private set; }
        #endregion

        #region Constructor
        public LeesOsLightController(string name, LeesOsLightControllerModel model) : base(name)
        {
            Channels = new List<LeesOsLightControllerChannel>();
            Model = model;

            int maximumVolume = 0;
            int channelCount = 0;
            switch (model)
            {
                case LeesOsLightControllerModel.LPD_3024_1CH:
                    maximumVolume = 255;
                    channelCount = 1;
                    break;
                case LeesOsLightControllerModel.LPD_3024_2CH:
                    maximumVolume = 255;
                    channelCount = 2;
                    break;
                case LeesOsLightControllerModel.LPD_4024_3CH:
                    maximumVolume = 255;
                    channelCount = 3;
                    break;
                case LeesOsLightControllerModel.LPD_4024_4CH:
                    maximumVolume = 255;
                    channelCount = 4;
                    break;
                case LeesOsLightControllerModel.LPD_6524_2CH:
                    maximumVolume = 4095;
                    channelCount = 2;
                    break;
                case LeesOsLightControllerModel.LPD_6524_4CH:
                    maximumVolume = 4095;
                    channelCount = 4;
                    break;
                case LeesOsLightControllerModel.LPD_12024_8CH:
                    maximumVolume = 4095;
                    channelCount = 8;
                    break;
                default:
                    throw new NotSupportedException($"{model.ToString()} is not supported.");
            }

            MaximumVolume = maximumVolume;
            for (int i = 0; i < channelCount; i++)
            {
                LeesOsLightControllerChannel channel = new LeesOsLightControllerChannel($"{name}_channel.{i + 1}", this);
                channel.Config.ChannelOnStateChanged += Config_ChannelOnStateChanged;
                channel.Config.ChannelVolumeChanged += Config_ChannelVolumeChanged;
                Channels.Add(channel);
            }

            Config = new LeesOsLightControllerConfig(name);

            communicator = new SerialComm();
            communicator.ETXString = "\r\n";
        }
        #endregion

        // 응답 대기 메서드 (SerialComm 클래스 활용)
        private string WaitForResponse(int timeoutMs)
        {
            try
            {
                if (communicator == null || !communicator.IsOpen)
                    return null;

                // SerialComm의 기본 Recv 메서드 사용
                communicator.ConversationTimeout = timeoutMs;

                if (communicator.Recv(out string response))
                {
                    return response;
                }

                return null;
            }
            catch (Exception ex)
            {
                Log.Write($"LightController [{Name}]", $"WaitForResponse error: {ex.Message}");
                return null;
            }
        }

        #region Event Handler
        private void Config_ChannelOnStateChanged(int channelNo, bool state)
        {
            if (communicator != null)
            {
                if (communicator.IsOpen)
                {
                    SetTurnOnOffCommandString(channelNo, state);
                }
            }
        }

        private void Config_ChannelVolumeChanged(int channelNo, int volume)
        {
            if (communicator != null)
            {
                if (communicator.IsOpen)
                {
                    SetVolumeCommandString(channelNo, volume);
                }
            }
        }
        #endregion

        #region Override Method
        public override int Initialize()
        {
            return 0;
        }
        public override int Create()
        {
            return 0;
        }
        public override void Close()
        {

        }
        #endregion

        #region Method
        private bool CommandMaxRetries(string command = "", int maxRetries = 5, int channelNo = 1)
        {
            // 재시도 로직
            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    Log.Write($"LightController [{Name}]", $"Channel {channelNo} ON attempt {attempt}/{maxRetries}");

                    communicator.Send(command);

                    // 응답 대기
                    string response = WaitForResponse(Config.ReplyTimeout);

                    if (!string.IsNullOrEmpty(response))
                    {
                        response = response.Trim().ToUpper();


                        if (response.Contains("ER"))
                        {
                            Log.Write($"LightController [{Name}]", $"Channel {channelNo} ON error response: {response}");
                            // 에러 응답이라도 재시도
                        }
                        else if (response.Contains("OK") || response.Contains("R"))
                        {
                            Log.Write($"LightController [{Name}]", $"Channel {channelNo} ON success on attempt {attempt}");
                            return true;
                        }
                        else
                        {
                            Log.Write($"LightController [{Name}]", $"Channel {channelNo} unexpected response: {response}");
                        }
                    }
                    else
                    {
                        Log.Write($"LightController [{Name}]", $"Channel {channelNo} no response on attempt {attempt}");
                    }

                    // 마지막 시도가 아니면 잠시 대기
                    if (attempt < maxRetries)
                    {
                        Thread.Sleep(100);
                    }
                }
                catch (Exception ex)
                {
                    Log.Write($"LightController [{Name}]", $"Channel {channelNo} ON attempt {attempt} error: {ex.Message}");
                }
            }

            Log.Write($"LightController [{Name}]", $"Channel {channelNo} ON failed after {maxRetries} attempts");
            return false;
        }

        public bool SetTurnOnOffCommandString(int channelNo, bool state)
        {
            string command = "";

            switch (Model)
            {
                // 8Bit Controller
                case LeesOsLightControllerModel.LPD_3024_1CH:
                case LeesOsLightControllerModel.LPD_3024_2CH:
                case LeesOsLightControllerModel.LPD_4024_3CH:
                case LeesOsLightControllerModel.LPD_4024_4CH:
                    command = $"H{channelNo.ToString("X1")}{(state ? "ON" : "OF")}\r\n";
                    break;

                // 12Bit Controller
                case LeesOsLightControllerModel.LPD_6524_2CH:
                case LeesOsLightControllerModel.LPD_6524_4CH:
                case LeesOsLightControllerModel.LPD_12024_8CH:  // 추가됨
                    command = $"LH{channelNo.ToString("X1")}{(state ? "ON" : "OF")}\r\n";
                    break;
            }

            return CommandMaxRetries(command, 100);
        }

        public bool SetVolumeCommandString(int channelNo, int volume)
        {
            string command = "";

            switch (Model)
            {
                // 8Bit Controller
                case LeesOsLightControllerModel.LPD_3024_1CH:
                case LeesOsLightControllerModel.LPD_3024_2CH:
                case LeesOsLightControllerModel.LPD_4024_3CH:
                case LeesOsLightControllerModel.LPD_4024_4CH:
                    command = $"C{channelNo.ToString("X1")}{volume.ToString("X2")}\r\n";
                    break;
                // 12Bit Controller
                case LeesOsLightControllerModel.LPD_6524_2CH:
                case LeesOsLightControllerModel.LPD_6524_4CH:
                case LeesOsLightControllerModel.LPD_12024_8CH:  // 추가됨
                    command = $"LC{channelNo.ToString("X1")}{volume.ToString("X3")}\r\n";  // L 접두사 추가
                    break;
            }

            return CommandMaxRetries(command, 100);
        }

        public bool SetChannelsOn(int channelNo)
        {
            if (communicator == null || !communicator.IsOpen)
                return false;

            string command = "";
            switch (Model)
            {
                case LeesOsLightControllerModel.LPD_3024_1CH:
                case LeesOsLightControllerModel.LPD_3024_2CH:
                case LeesOsLightControllerModel.LPD_4024_3CH:
                case LeesOsLightControllerModel.LPD_4024_4CH:
                    command = $"H{channelNo.ToString("X1")}ON\r\n";
                    break;
                case LeesOsLightControllerModel.LPD_6524_2CH:
                case LeesOsLightControllerModel.LPD_6524_4CH:
                case LeesOsLightControllerModel.LPD_12024_8CH:
                    command = $"LH{channelNo.ToString("X1")}ON\r\n";
                    break;
            }

            if (string.IsNullOrEmpty(command))
                return false;

            return CommandMaxRetries(command, 100, channelNo);
        }

        public bool SetChannelsOff(int channelNo)
        {
            if (communicator == null || !communicator.IsOpen)
                return false;

            string command = "";
            switch (Model)
            {
                case LeesOsLightControllerModel.LPD_3024_1CH:
                case LeesOsLightControllerModel.LPD_3024_2CH:
                case LeesOsLightControllerModel.LPD_4024_3CH:
                case LeesOsLightControllerModel.LPD_4024_4CH:
                    command = $"H{channelNo.ToString("X1")}OF\r\n";
                    break;
                case LeesOsLightControllerModel.LPD_6524_2CH:
                case LeesOsLightControllerModel.LPD_6524_4CH:
                case LeesOsLightControllerModel.LPD_12024_8CH:
                    command = $"LH{channelNo.ToString("X1")}OF\r\n";
                    break;
            }

            if (string.IsNullOrEmpty(command))
                return false;

            return CommandMaxRetries(command, 100, channelNo);
        }

        // 전체 채널 제어 메서드 추가
        public void SetAllChannelsOn()
        {
            if (communicator != null && communicator.IsOpen)
            {
                string command = "";
                switch (Model)
                {
                    case LeesOsLightControllerModel.LPD_3024_1CH:
                    case LeesOsLightControllerModel.LPD_3024_2CH:
                    case LeesOsLightControllerModel.LPD_4024_3CH:
                    case LeesOsLightControllerModel.LPD_4024_4CH:
                        command = "HTON\r\n";
                        break;
                    case LeesOsLightControllerModel.LPD_6524_2CH:
                    case LeesOsLightControllerModel.LPD_6524_4CH:
                    case LeesOsLightControllerModel.LPD_12024_8CH:
                        command = "LHTON\r\n";
                        break;
                }
                if (!string.IsNullOrEmpty(command))
                {
                    communicator.Send(command);
                }
            }
        }

        public void SetAllChannelsOff()
        {
            if (communicator != null && communicator.IsOpen)
            {
                string command = "";
                switch (Model)
                {
                    case LeesOsLightControllerModel.LPD_3024_1CH:
                    case LeesOsLightControllerModel.LPD_3024_2CH:
                    case LeesOsLightControllerModel.LPD_4024_3CH:
                    case LeesOsLightControllerModel.LPD_4024_4CH:
                        command = "HTOF\r\n";
                        break;
                    case LeesOsLightControllerModel.LPD_6524_2CH:
                    case LeesOsLightControllerModel.LPD_6524_4CH:
                    case LeesOsLightControllerModel.LPD_12024_8CH:
                        command = "LHTOF\r\n";
                        break;
                }
                if (!string.IsNullOrEmpty(command))
                {
                    communicator.Send(command);
                }
            }
        }

        public void SetAllChannelsVolume(int volume)
        {
            if (communicator != null && communicator.IsOpen)
            {
                string command = "";
                switch (Model)
                {
                    case LeesOsLightControllerModel.LPD_3024_1CH:
                    case LeesOsLightControllerModel.LPD_3024_2CH:
                    case LeesOsLightControllerModel.LPD_4024_3CH:
                    case LeesOsLightControllerModel.LPD_4024_4CH:
                        command = $"CT{volume.ToString("X2")}\r\n";
                        break;
                    case LeesOsLightControllerModel.LPD_6524_2CH:
                    case LeesOsLightControllerModel.LPD_6524_4CH:
                    case LeesOsLightControllerModel.LPD_12024_8CH:
                        command = $"LCT{volume.ToString("X3")}\r\n";
                        break;
                }
                if (!string.IsNullOrEmpty(command))
                {
                    communicator.Send(command);
                }
            }
        }

        // 통신 연결 메서드
        public int Connect()
        {
            try
            {
                if (communicator == null)
                {
                    communicator = new SerialComm();
                    communicator.ETXString = "\r\n";
                }

                if (communicator.IsOpen)
                    communicator.Close();

                communicator.PortName = Config.PortName;
                communicator.BaudRate = Config.BaudRate;
                communicator.DataBits = Config.DataBits;
                communicator.Parity = Config.Parity;
                communicator.StopBits = Config.StopBits;
                communicator.Handshake = Config.Handshake;

                return communicator.Open() ? 0 : -1;
            }
            catch (Exception ex)
            {
                Log.Write($"LightController [{Name}]", $"Connect error: {ex.Message}");
                return -1;
            }
        }

        public void Disconnect()
        {
            try
            {
                if (communicator != null && communicator.IsOpen)
                {
                    communicator.Close();
                }
            }
            catch (Exception ex)
            {
                Log.Write($"LightController [{Name}]", $"Disconnect error: {ex.Message}");
            }
        }

        public bool IsConnected
        {
            get { return communicator?.IsOpen ?? false; }
        }
        #endregion
    }
}