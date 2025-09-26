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
        #region Defines
        public class StrainGageAutoZeroTracker
        {
            private readonly double step; // 보정 속도 (작을수록 느리게 따라감)
            private readonly double deadband; // 노이즈 허용 범위

            public StrainGageAutoZeroTracker(double step, double deadband)
            {
                this.step = step;
                this.deadband = deadband;
            }

            public double TrackZeroValue(double currentValue, ref double zeroValue)
            {
                double adjusted = currentValue - zeroValue;

                // 입력이 deadband 범위 안에 있으면 "제로 드리프트"로 간주
                if (Math.Abs(adjusted) < deadband)
                {
                    // 기준점을 조금씩 이동 (느리게 따라감)
                    zeroValue += step * adjusted;
                }

                // 보정된 출력 반환
                return currentValue - zeroValue;
            }
        }
        #endregion

        #region Fields
        private double voltage = 0;
        private double zeroVoltage = 0;
        private QmcLowPassFilter lowPassFilter = new QmcLowPassFilter();
        private StrainGageAutoZeroTracker autoZeroTracker = new StrainGageAutoZeroTracker(0.01, 0.005);
        #endregion

        #region Properties
        public double Voltage => voltage;
        public double ZeroVoltage => zeroVoltage;
        public double Force => GetForce();
        public new StrainGageConfig Config { get; private set; }
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
                lowPassFilter.AddValue(value);
                voltage = lowPassFilter.CurrentValue;
            }
            else
            {
                voltage = value;
            }

            // auto zero tracking
            //if (Config.UseAutoZeroTracking)
            //{
            //    autoZeroTracker.TrackZeroValue(voltage, ref zeroVoltage);
            //}
        }
        public void ResetZeroVoltage()
        {
            zeroVoltage = 0;
        }
        public void SetZeroVoltage(double value)
        {
            zeroVoltage = value;
        }
        private double GetForce()
        {
            if (!Config.Validate())
                return 0;

            double scale = (Config.MaxForce - Config.MinForce) / (Config.MaxVoltage - Config.MinVoltage);
            double force = (voltage - zeroVoltage) * scale;
            if (force < 0)
                force = 0;

            return force;
        }
        #endregion
    }
}
