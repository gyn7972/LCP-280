using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QMC.Common.Motions
{
    [Serializable]
    public sealed class AxisPositionVelocityStatus
    {
        public double CommandPosition { get; set; }   // mm
        public double ActualPosition { get; set; }   // mm
        public double ErrorPosition { get; set; }   // mm
        public double CommandVelocity { get; set; }   // mm/s
        public double ActualVelocity { get; set; }   // mm/s
    }

    [Serializable]
    public sealed class AxisMotorIOStatus
    {
        public bool ServoOn { get; set; }
        public bool Alarm { get; set; }
        public bool NegativeLimitSensor { get; set; }
        public bool PositiveLimitSensor { get; set; }
        public bool HomeSensor { get; set; }
    }

    [Serializable]
    public sealed class AxisMotorStateStatus
    {
        public bool Done { get; set; }               // Motion done
        public bool Inposition { get; set; }
        public bool InpositionDone { get; set; }
        public bool InpositionTimeout { get; set; }
        public bool HomeEnd { get; set; }
        public bool HomeTimeout { get; set; }
    }

    [Serializable]
    public sealed class MotionAxisStatus
    {
        public AxisPositionVelocityStatus PV { get; set; } = new AxisPositionVelocityStatus();
        public AxisMotorIOStatus IO { get; set; } = new AxisMotorIOStatus();
        public AxisMotorStateStatus State { get; set; } = new AxisMotorStateStatus();
        public DateTime TimestampUtc { get; set; }
    }
}
