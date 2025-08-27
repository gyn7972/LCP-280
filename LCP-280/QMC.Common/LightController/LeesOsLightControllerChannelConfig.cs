using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QMC.Common.LightController
{
    public class LeesOsLightControllerChannelConfig : BaseConfig
    {
        #region Field
        private bool on;
        private int volume;
        private LeesOsLightControllerChannel ownerChannel;
        #endregion

        #region Property
        public bool On 
        {   
            get
            {
                return on;
            }
            set
            {
                if (on != value)
                {
                    on = value;
                    OnChannelOnStateChanged(ownerChannel.ChannelNo, value);
                }
            }
        }
        public int Volume 
        {
            get
            {
                return volume;
            }
            set
            {
                if (volume != value)
                {
                    volume = value;
                    OnChannelVolumeChanged(ownerChannel.ChannelNo, value);
                }
            }
        }
        public string Descript { get; set; }
        #endregion

        #region Constructor
        public LeesOsLightControllerChannelConfig(string name, LeesOsLightControllerChannel owner) : base(name) 
        {
            ownerChannel = owner;
            Reset();
        }
        #endregion

        #region Event
        public delegate void ChangeOnState(int channelNo, bool state);
        public delegate void ChangeVolume(int channelNo, int volume);

        public event ChangeOnState ChannelOnStateChanged;
        public event ChangeVolume ChannelVolumeChanged;

        private void OnChannelOnStateChanged(int channelNo, bool state)
        {
            ChannelOnStateChanged?.Invoke(channelNo, state);
        }
        private void OnChannelVolumeChanged(int channelNo, int volume)
        {
            ChannelVolumeChanged?.Invoke(channelNo, volume);
        }
        #endregion

        #region Method
        public override void Reset()
        {
            On = false;
            Volume = 0;
            Descript = "";
        }

        public override bool Validate()
        {
            if(Volume < 0 || Volume > ownerChannel.OwnerController.MaximumVolume)
                return false;

            return true;
        }

        public override PropertyCollection GetPropertyCollection()
        {
            PropertyCollection pc = new PropertyCollection();

            // Title
            string title = "Channel Config";
            if (ownerChannel.OwnerController != null)
                title = $"{ownerChannel.OwnerController.Name} - Channel {ownerChannel.ChannelNo} Config";
            pc.Add(title);

            // Value
            pc.Add("On", On);
            pc.Add("Volume", Volume);
            pc.Add("Descript", Descript);

            return pc;
        }
        public override int ApplyValueFromPropertyCollection(PropertyCollection pc)
        {
            if (pc == null)
                return -1;

            try
            {
                On = pc.GetValue<bool>("On");
                Volume = pc.GetValue<int>("Volume");
                Descript = pc.GetValue<string>("Descript");
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
