using QMC.Common.Component;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QMC.Common.LightController
{
    public class LeesOsLightControllerChannel : BaseComponent
    {
        #region Field
        #endregion

        #region Property
        public int ChannelNo { get; private set; }
        public new LeesOsLightControllerChannelConfig Config { get; private set; }
        public LeesOsLightController OwnerController { get; set; }
        #endregion

        #region Constructor
        public LeesOsLightControllerChannel(string name, LeesOsLightController owner) : base(name)
        {
            OwnerController = owner;
            ChannelNo = OwnerController.Channels.Count + 1;
            Config = new LeesOsLightControllerChannelConfig($"{name}_config", this);
        }
        #endregion

        #region Method
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
    }
}
