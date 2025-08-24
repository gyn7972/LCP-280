using System;

namespace QMC.Common
{
    /// <summary>
    /// 소프트리밋/인터락 등 공통 안전 정책.
    /// 실제 프로젝트에서는 축별, 유닛별 구성으로 확장할 수 있다.
    /// </summary>
    public class SafetyPolicy
    {
        public double? SoftLimitMin { get; set; }
        public double? SoftLimitMax { get; set; }
        public Func<bool> InterlockGuard { get; set; }

        public bool ValidateMove(double target, out string error)
        {
            if (SoftLimitMin.HasValue && target < SoftLimitMin.Value)
            {
                error = $"Target {target} is below soft limit {SoftLimitMin.Value}";
                return false;
            }
            if (SoftLimitMax.HasValue && target > SoftLimitMax.Value)
            {
                error = $"Target {target} is above soft limit {SoftLimitMax.Value}";
                return false;
            }
            if (InterlockGuard != null && !InterlockGuard())
            {
                error = "Interlock not satisfied";
                return false;
            }
            error = null;
            return true;
        }
    }
}
