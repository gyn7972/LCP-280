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

        #region Event Handler
        private void Config_ChannelOnStateChanged(int channelNo, bool state)
        {
            try
            {
                if (EnsureConnection())
                {
                    string command = GetTurnOnOffCommandString(channelNo, state);
                    if (!string.IsNullOrEmpty(command))
                    {
                        bool success = SendCommandWithResponse(command, channelNo);
                        Log.Write($"LightController [{Name}]",
                            $"Channel {channelNo} {(state ? "ON" : "OFF")} command: {(success ? "SUCCESS" : "FAILED")}");
                    }
                }
                else
                {
                    Log.Write($"LightController [{Name}]",
                        $"Failed to send Channel {channelNo} {(state ? "ON" : "OFF")} command - Connection failed");
                }
            }
            catch (Exception ex)
            {
                Log.Write($"LightController [{Name}]",
                    $"Error sending Channel {channelNo} {(state ? "ON" : "OFF")} command: {ex.Message}");
            }
        }

        private void Config_ChannelVolumeChanged(int channelNo, int volume)
        {
            try
            {
                if (EnsureConnection())
                {
                    string command = GetVolumeCommandString(channelNo, volume);
                    if (!string.IsNullOrEmpty(command))
                    {
                        bool success = SendCommandWithResponse(command, channelNo);
                        Log.Write($"LightController [{Name}]",
                            $"Channel {channelNo} Volume {volume} command: {(success ? "SUCCESS" : "FAILED")}");
                    }
                }
                else
                {
                    Log.Write($"LightController [{Name}]",
                        $"Failed to send Channel {channelNo} Volume {volume} command - Connection failed");
                }
            }
            catch (Exception ex)
            {
                Log.Write($"LightController [{Name}]",
                    $"Error sending Channel {channelNo} Volume {volume} command: {ex.Message}");
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
        private string GetTurnOnOffCommandString(int channelNo, bool state)
        {
            switch (Model)
            {
                // 8Bit Controller
                case LeesOsLightControllerModel.LPD_3024_1CH:
                case LeesOsLightControllerModel.LPD_3024_2CH:
                case LeesOsLightControllerModel.LPD_4024_3CH:
                case LeesOsLightControllerModel.LPD_4024_4CH:
                    return $"H{channelNo.ToString("X1")}{(state ? "ON" : "OF")}";

                // 12Bit Controller
                case LeesOsLightControllerModel.LPD_6524_2CH:
                case LeesOsLightControllerModel.LPD_6524_4CH:
                case LeesOsLightControllerModel.LPD_12024_8CH:
                    return $"LH{channelNo.ToString("X1")}{(state ? "ON" : "OF")}";
            }
            return "";
        }

        private string GetVolumeCommandString(int channelNo, int volume)
        {
            switch (Model)
            {
                // 8Bit Controller
                case LeesOsLightControllerModel.LPD_3024_1CH:
                case LeesOsLightControllerModel.LPD_3024_2CH:
                case LeesOsLightControllerModel.LPD_4024_3CH:
                case LeesOsLightControllerModel.LPD_4024_4CH:
                    return $"C{channelNo.ToString("X1")}{volume.ToString("X2")}";

                // 12Bit Controller
                case LeesOsLightControllerModel.LPD_6524_2CH:
                case LeesOsLightControllerModel.LPD_6524_4CH:
                case LeesOsLightControllerModel.LPD_12024_8CH:
                    return $"LC{channelNo.ToString("X1")}{volume.ToString("X3")}";
            }
            return "";
        }

        // 응답 처리가 포함된 명령어 전송 메서드
        private bool SendCommandWithResponse(string command, int channelNo)
        {
            try
            {
                if (communicator == null || !communicator.IsOpen)
                    return false;

                // 명령어 전송
                communicator.Send(command);
                Log.Write($"LightController [{Name}]", $"Sent: {command.Trim()}");

                // 응답 대기 (Config에 설정된 타임아웃 사용)
                string response = WaitForResponse(Config.ReplyTimeout);

                if (!string.IsNullOrEmpty(response))
                {
                    Log.Write($"LightController [{Name}]", $"Received: {response.Trim()}");
                    return ParseResponse(response, channelNo);
                }
                else
                {
                    Log.Write($"LightController [{Name}]", "No response received (timeout)");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Log.Write($"LightController [{Name}]", $"SendCommandWithResponse error: {ex.Message}");
                return false;
            }
        }

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

        // 응답 파싱 메서드
        private bool ParseResponse(string response, int channelNo)
        {
            try
            {
                if (string.IsNullOrEmpty(response))
                    return false;

                // 응답 포맷: R + 채널번호 + 상태
                // 예: R1OK, R1ER, R1FFF 등
                response = response.Replace("\r", "").Replace("\n", "").Trim();

                if (response.Length < 3 || !response.StartsWith("R"))
                    return false;

                // 채널번호 확인 (선택사항)
                char responseChannel = response[1];

                // 상태 확인
                string status = response.Substring(2);

                switch (status.ToUpper())
                {
                    case "OK":
                        Log.Write($"LightController [{Name}]", $"Channel {channelNo} command executed successfully");
                        return true;

                    case "ER":
                        Log.Write($"LightController [{Name}]", $"Channel {channelNo} command failed");
                        return false;

                    default:
                        // 볼륨값이나 기타 응답의 경우
                        if (status.Length >= 3)
                        {
                            Log.Write($"LightController [{Name}]", $"Channel {channelNo} status: {status}");
                            return true;
                        }
                        break;
                }

                return false;
            }
            catch (Exception ex)
            {
                Log.Write($"LightController [{Name}]", $"ParseResponse error: {ex.Message}");
                return false;
            }
        }

        // 전체 채널 제어 메서드 (응답 처리 포함)
        public bool SetAllChannelsOn()
        {
            try
            {
                if (EnsureConnection())
                {
                    string command = "";
                    switch (Model)
                    {
                        case LeesOsLightControllerModel.LPD_3024_1CH:
                        case LeesOsLightControllerModel.LPD_3024_2CH:
                        case LeesOsLightControllerModel.LPD_4024_3CH:
                        case LeesOsLightControllerModel.LPD_4024_4CH:
                            command = "HTON";
                            break;
                        case LeesOsLightControllerModel.LPD_6524_2CH:
                        case LeesOsLightControllerModel.LPD_6524_4CH:
                        case LeesOsLightControllerModel.LPD_12024_8CH:
                            command = "LHTON";
                            break;
                    }

                    if (!string.IsNullOrEmpty(command))
                    {
                        return SendCommandWithResponse(command, 0); // 전체 채널은 0으로 표시
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                Log.Write($"LightController [{Name}]", $"SetAllChannelsOn error: {ex.Message}");
                return false;
            }
        }

        public bool SetAllChannelsOff()
        {
            try
            {
                if (EnsureConnection())
                {
                    string command = "";
                    switch (Model)
                    {
                        case LeesOsLightControllerModel.LPD_3024_1CH:
                        case LeesOsLightControllerModel.LPD_3024_2CH:
                        case LeesOsLightControllerModel.LPD_4024_3CH:
                        case LeesOsLightControllerModel.LPD_4024_4CH:
                            command = "HTOF";
                            break;
                        case LeesOsLightControllerModel.LPD_6524_2CH:
                        case LeesOsLightControllerModel.LPD_6524_4CH:
                        case LeesOsLightControllerModel.LPD_12024_8CH:
                            command = "LHTOF";
                            break;
                    }

                    if (!string.IsNullOrEmpty(command))
                    {
                        return SendCommandWithResponse(command, 0);
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                Log.Write($"LightController [{Name}]", $"SetAllChannelsOff error: {ex.Message}");
                return false;
            }
        }

        public bool SetAllChannelsVolume(int volume)
        {
            try
            {
                if (EnsureConnection())
                {
                    string command = "";
                    switch (Model)
                    {
                        case LeesOsLightControllerModel.LPD_3024_1CH:
                        case LeesOsLightControllerModel.LPD_3024_2CH:
                        case LeesOsLightControllerModel.LPD_4024_3CH:
                        case LeesOsLightControllerModel.LPD_4024_4CH:
                            command = $"CT{volume.ToString("X2")}";
                            break;
                        case LeesOsLightControllerModel.LPD_6524_2CH:
                        case LeesOsLightControllerModel.LPD_6524_4CH:
                        case LeesOsLightControllerModel.LPD_12024_8CH:
                            command = $"LCT{volume.ToString("X3")}";
                            break;
                    }

                    if (!string.IsNullOrEmpty(command))
                    {
                        return SendCommandWithResponse(command, 0);
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                Log.Write($"LightController [{Name}]", $"SetAllChannelsVolume error: {ex.Message}");
                return false;
            }
        }

        // 연결 확인 및 자동 연결 메서드
        private bool EnsureConnection()
        {
            try
            {
                if (communicator != null && communicator.IsOpen)
                    return true;

                if (!Config.Validate())
                {
                    Log.Write($"LightController [{Name}]", "Invalid configuration - cannot connect");
                    return false;
                }

                int result = Connect();
                if (result == 0)
                {
                    Log.Write($"LightController [{Name}]", $"Auto-connected to {Config.PortName}");
                    return true;
                }
                else
                {
                    Log.Write($"LightController [{Name}]", $"Failed to auto-connect to {Config.PortName}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Log.Write($"LightController [{Name}]", $"EnsureConnection error: {ex.Message}");
                return false;
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

                return communicator.Open() ? 0 : -1; //0이면 OK
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

        // 상태 확인 메서드 (매뉴얼의 LS 명령어 사용)
        public string GetChannelStatus(int channelNo, int statusType)
        {
            try
            {
                if (!IsConnected)
                    return null;

                string command = "";
                switch (Model)
                {
                    case LeesOsLightControllerModel.LPD_3024_1CH:
                    case LeesOsLightControllerModel.LPD_3024_2CH:
                    case LeesOsLightControllerModel.LPD_4024_3CH:
                    case LeesOsLightControllerModel.LPD_4024_4CH:
                        command = $"S{channelNo.ToString("X1")}{statusType.ToString("D2")}";
                        break;
                    case LeesOsLightControllerModel.LPD_6524_2CH:
                    case LeesOsLightControllerModel.LPD_6524_4CH:
                    case LeesOsLightControllerModel.LPD_12024_8CH:
                        command = $"LS{channelNo.ToString("X1")}{statusType.ToString("D2")}";
                        break;
                }

                if (!string.IsNullOrEmpty(command))
                {
                    SendCommandWithResponse(command, channelNo);
                    return WaitForResponse(Config.ReplyTimeout);
                }

                return null;
            }
            catch (Exception ex)
            {
                Log.Write($"LightController [{Name}]", $"GetChannelStatus error: {ex.Message}");
                return null;
            }
        }
        #endregion
    }
}