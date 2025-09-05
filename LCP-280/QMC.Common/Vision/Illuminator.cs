using QMC.Common.Component;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QMC.Common.Vision
{
    [Serializable]
    public class Illuminator : BaseComponent
    {
        public Illuminator() : this("Illuminator")
        {

        }
        public Illuminator(string strName) : base(strName)
        {
        }

        public override int Initialize()
        {
            return base.Initialize();
        }
        
        public virtual int CheckPowerOn(int channel)
        {
            return -1;
        }

        public virtual int SetVolume(int volume, int channel)
        {
            return -1;
        }

        public virtual int TurnOnOff(bool bOnOff, int channel)
        {
            return -1;
        }

    }
}
