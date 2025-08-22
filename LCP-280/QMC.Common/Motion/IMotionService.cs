using System;
using System.Threading;
using System.Threading.Tasks;

namespace QMC.Common
{
    public interface IMotionService
    {
        Task<MotionResult> HomeAsync(string axisKey, CancellationToken ct);
        Task<MotionResult> MoveAbsAsync(string axisKey, double target, MotionKinematics kin, TimeSpan? timeout, CancellationToken ct);
        Task<MotionResult> JogAsync(string axisKey, JogDirection dir, double velocity, CancellationToken ct);
        Task StopAsync(string axisKey);
        double GetActualPosition(string axisKey);
    }

    public enum JogDirection { Positive, Negative }

    public struct MotionKinematics
    {
        public double Velocity;
        public double Acceleration;
        public double Deceleration;

        public static MotionKinematics Default(double vel) => new MotionKinematics { Velocity = vel, Acceleration = vel * 10, Deceleration = vel * 10 };
    }

    public struct MotionResult
    {
        public bool Success;
        public string Error;
        public static MotionResult Ok() => new MotionResult { Success = true };
        public static MotionResult Fail(string error) => new MotionResult { Success = false, Error = error };
    }
}
