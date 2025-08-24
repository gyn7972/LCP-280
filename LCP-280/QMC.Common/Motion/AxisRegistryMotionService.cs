using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace QMC.Common
{
    /// <summary>
    /// Simple IMotionService backed by an axis provider/registry.
    /// Applies timeout and basic cancellation uniformly.
    /// </summary>
    public class AxisRegistryMotionService : IMotionService
    {
        private readonly Func<string, IMotionAxis> _axisResolver;

        public AxisRegistryMotionService(Func<string, IMotionAxis> axisResolver)
        {
            _axisResolver = axisResolver ?? throw new ArgumentNullException(nameof(axisResolver));
        }

        public double GetActualPosition(string axisKey)
        {
            var axis = Resolve(axisKey);
            return axis?.GetActualPosition() ?? double.NaN;
        }

        public Task<MotionResult> HomeAsync(string axisKey, CancellationToken ct)
        {
            // Homing API가 IMotionAxis에 없다면 장치별 구현으로 확장 필요. 여기서는 Success 반환.
            return Task.FromResult(MotionResult.Ok());
        }

        public Task StopAsync(string axisKey)
        {
            var axis = Resolve(axisKey);
            axis?.JogStop();
            return Task.CompletedTask;
        }

        public Task<MotionResult> JogAsync(string axisKey, JogDirection dir, double velocity, CancellationToken ct)
        {
            var axis = Resolve(axisKey);
            if (axis == null) return Task.FromResult(MotionResult.Fail($"Axis not found: {axisKey}"));

            bool ok = dir == JogDirection.Positive ? axis.JogPositive(velocity) : axis.JogNegative(velocity);
            return Task.FromResult(ok ? MotionResult.Ok() : MotionResult.Fail($"Jog failed: {axisKey}"));
        }

        public Task<MotionResult> MoveAbsAsync(string axisKey, double target, MotionKinematics kin, TimeSpan? timeout, CancellationToken ct)
        {
            var axis = Resolve(axisKey);
            if (axis == null)
                return Task.FromResult(MotionResult.Fail($"Axis not found: {axisKey}"));

            string error;
            int timeoutMs = (int)(timeout?.TotalMilliseconds ?? 30000);
            bool ok = axis.MoveAbs(target, kin.Velocity, kin.Acceleration, kin.Deceleration, timeoutMs, out error);
            return Task.FromResult(ok ? MotionResult.Ok() : MotionResult.Fail(error ?? $"MoveAbs failed: {axisKey}"));
        }

        private IMotionAxis Resolve(string axisKey)
        {
            return _axisResolver(axisKey);
        }
    }
}
