using System;
using System.Threading;
using System.Threading.Tasks;

namespace QMC.Common
{
    /// <summary>
    /// SafetyPolicy를 적용하여 이동 전 검증을 수행하는 MotionService 데코레이터.
    /// </summary>
    public class SafeMotionService : IMotionService
    {
        private readonly IMotionService _inner;
        private readonly Func<string, SafetyPolicy> _policyResolver;

        public SafeMotionService(IMotionService inner, Func<string, SafetyPolicy> policyResolver)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
            _policyResolver = policyResolver ?? (_ => null);
        }

        public double GetActualPosition(string axisKey) => _inner.GetActualPosition(axisKey);

        public Task<MotionResult> HomeAsync(string axisKey, CancellationToken ct) => _inner.HomeAsync(axisKey, ct);

        public Task<MotionResult> JogAsync(string axisKey, JogDirection dir, double velocity, CancellationToken ct)
        {
            // Jog은 정책에 따라 제한이 필요할 수 있으나 여기서는 위임
            return _inner.JogAsync(axisKey, dir, velocity, ct);
        }

        public async Task<MotionResult> MoveAbsAsync(string axisKey, double target, MotionKinematics kin, TimeSpan? timeout, CancellationToken ct)
        {
            var policy = _policyResolver(axisKey);
            if (policy != null && !policy.ValidateMove(target, out var error))
            {
                return MotionResult.Fail(error);
            }
            return await _inner.MoveAbsAsync(axisKey, target, kin, timeout, ct).ConfigureAwait(false);
        }

        public Task StopAsync(string axisKey) => _inner.StopAsync(axisKey);
    }
}
