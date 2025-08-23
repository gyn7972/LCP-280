using System;

namespace QMC.Common
{
    /// <summary>
    /// 하위 Motion 라이브러리 추상화 (필요에 따라 확장)
    /// </summary>
    public interface IMotionAxis
    {
        string Name { get; }
        string Unit { get; }

        double GetActualPosition();
        bool MoveAbs(double target, double velocity, double acc, double dec, int timeoutMs, out string error);
        bool JogPositive(double velocity);
        bool JogNegative(double velocity);
        void JogStop();
        void ServoOn();
        void ServoOff();
    }
}