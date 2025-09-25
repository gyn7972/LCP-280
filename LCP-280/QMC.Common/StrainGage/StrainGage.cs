using QMC.Common.Component;
using QMC.Common.Keithley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QMC.Common.StrainGage
{
    public class StrainGage : BaseComponent
    {
        #region Fields
        private double voltage = 0;
        private QmcLowPassFilter lowPassFilter = new QmcLowPassFilter();
        private bool lowPassFilterHasPrev = false;
        #endregion

        #region Properties
        public double Voltage => voltage;
        public double Force => GetForce();
        public new StrainGageConfig Config { get; private set; }
        public double ZeroVoltage { get; private set; } = 0;
        #endregion

        #region Constructor
        public StrainGage(string name) : base(name)
        {
            Config = new StrainGageConfig(name);
        }
        #endregion

        #region Override Methods
        public override int Initialize()
        {
            try
            {
                if (!Config.Validate())
                    return -1;

                // set low pass filter
                lowPassFilter.CutoffFrequence = Config.LowPassFilterCutoffFrequency;
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                return -1;
            }
            return 0;
        }
        #endregion

        #region Methods
        public void UpdateVoltage(double value)
        {
            double newvalue = value;
            if (Config.UseLowPassFilter)
            {
                if (!lowPassFilterHasPrev)
                {
                    lowPassFilter.ResetValue(value);
                    lowPassFilterHasPrev = true;
                }
                lowPassFilter.AddValue(value);
                this.voltage = lowPassFilter.CurrentValue;

                //if (this.Config.UseAutoZeroSet)
                //{
                //    AutoZeroTracking(value, this.voltage);
                //}
            }
            else
            {
                this.voltage = value;
            }
        }
        public void SetZero()
        {
            this.ZeroVoltage = this.voltage;
        }
        private void AutoZeroTracking(double value, double voltage)
        {

           if((voltage - this.ZeroVoltage) <  0.001 )
            {
                this.ZeroVoltage = voltage;
            }
            
        }

        private double GetForce()
        {
            if (!Config.Validate())
                return 0;

            //if (voltage < Config.MinVoltage)
            //    return Config.MinForce;
            //if (voltage > Config.MaxVoltage)
            //    return Config.MaxForce;

            double scale = (Config.MaxForce - Config.MinForce) / (Config.MaxVoltage - Config.MinVoltage);
            double force = (voltage -this.ZeroVoltage) * scale;
            return force;
        }
        #endregion
    }
}
