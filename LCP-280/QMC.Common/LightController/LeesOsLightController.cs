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
        LPD_6524_4CH
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
            if (communicator != null)
            {
                if (communicator.IsOpen)
                {
                    string command = GetTurnOnOffCommandString(channelNo, state);
                    communicator.Send(command);
                }
            }
        }

        private void Config_ChannelVolumeChanged(int channelNo, int volume)
        {
            if (communicator != null)
            {
                if (communicator.IsOpen)
                {
                    string command = GetVolumeCommandString(channelNo, volume);
                    communicator.Send(command);
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
        private string GetTurnOnOffCommandString(int channelNo, bool state)
        {
            switch(Model)
            {
                // 8Bit Controller
                case LeesOsLightControllerModel.LPD_3024_1CH:
                case LeesOsLightControllerModel.LPD_3024_2CH:
                case LeesOsLightControllerModel.LPD_4024_3CH:
                case LeesOsLightControllerModel.LPD_4024_4CH:
                    return $"H{channelNo.ToString("X1")}{(state ? "ON":"OF")}\r\n";

                // 12Bit Controller
                case LeesOsLightControllerModel.LPD_6524_2CH:
                case LeesOsLightControllerModel.LPD_6524_4CH:
                    return $"LH{channelNo.ToString("X1")}{(state ? "ON":"OF")}\r\n";
            }
            return "";
        }
        private string GetVolumeCommandString(int channelNo, int volume)
        {
            switch(Model)
            {
                // 8Bit Controller
                case LeesOsLightControllerModel.LPD_3024_1CH:
                case LeesOsLightControllerModel.LPD_3024_2CH:
                case LeesOsLightControllerModel.LPD_4024_3CH:
                case LeesOsLightControllerModel.LPD_4024_4CH:
                    return $"C{channelNo.ToString("X1")}{volume.ToString("X2")}\r\n";

                // 12Bit Controller
                case LeesOsLightControllerModel.LPD_6524_2CH:
                case LeesOsLightControllerModel.LPD_6524_4CH:
                    return $"C{channelNo.ToString("X1")}{volume.ToString("X3")}\r\n";
            }
            return "";
        }
        #endregion
    }
}
