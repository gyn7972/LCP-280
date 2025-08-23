using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QMC.Common.Motions
{
    /// <summary>
    /// 논리(mm/deg) ↔ 펄스 변환/보정 계층
    /// </summary>
    public interface IPropertyCorrection
    {
        double ToHardware(double logicalValue);  // mm → pulse
        double ToLogical(double hardwareValue);  // pulse → mm
    }

    /// <summary>
    /// 기본 보정: (논리 * Scale + Offset) * PulsesPerUnit
    /// </summary>
    public sealed class DefaultCorrection : IPropertyCorrection
    {
        private readonly MotionAxisSetup _setup;
        private readonly MotionAxisConfig _config;

        public DefaultCorrection(MotionAxisSetup setup, MotionAxisConfig config)
        {
            _setup = setup;
            _config = config;
        }

        public double ToHardware(double logicalValue)
        {
            var v = logicalValue * _config.LogicalScaleFactor + _config.Offset;
            return v * _setup.PulsesPerUnit;
        }

        public double ToLogical(double hardwareValue)
        {
            var v = hardwareValue / _setup.PulsesPerUnit;
            return (v - _config.Offset) / _config.LogicalScaleFactor;
        }
    }
}
