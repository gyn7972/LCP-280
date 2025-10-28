using System;
using QMC.Common.Motions;
using QMC.LCP_280.Process.Unit;

namespace QMC.LCP_280.Process.Component
{
    /// <summary>
    /// 홈 완료 후 공통 후처리를 구독/처리하는 전역 훅.
    /// 프로그램 초기화 시 EnsureSubscribed() 한 번만 호출하세요.
    /// </summary>
    public static class HomeHooks
    {
        private static bool _subscribed;

        public static void EnsureSubscribed()
        {
            if (_subscribed) return;
            MotionAxis.HomeSucceeded += OnAxisHomeSucceeded;
            _subscribed = true;
        }

        private static void OnAxisHomeSucceeded(MotionAxis axis)
        {
            if (axis == null) return;

            try
            {
                var name = axis.Name ?? string.Empty;

                if (name.Equals("Wafer Feeder Y Axis", StringComparison.OrdinalIgnoreCase))
                {
                    var eq = Equipment.Instance;
                    if (eq?.Units != null && eq.Units.TryGetValue("InputFeeder", out var u) && u is InputFeeder inFeeder)
                    {
                        double logical = inFeeder.GetTP(
                            InputFeederConfig.TeachingPositionName.SetPosition.ToString(),
                            "Wafer Feeder Y Axis");

                        AjinApi.SetActualPositionPulse(axis.AxisNo, logical);
                        AjinApi.SetCommandPositionPulse(axis.AxisNo, logical);
                    }
                }
                else if (name.Equals("Bin Feeder Y Axis", StringComparison.OrdinalIgnoreCase))
                {
                    var eq = Equipment.Instance;
                    if (eq?.Units != null && eq.Units.TryGetValue("OutputFeeder", out var u2) && u2 is OutputFeeder outFeeder)
                    {
                        double logical = outFeeder.GetTP(
                            OutputFeederConfig.TeachingPositionName.SetPosition.ToString(),
                            "Bin Feeder Y Axis");

                        AjinApi.SetActualPositionPulse(axis.AxisNo, logical);
                        AjinApi.SetCommandPositionPulse(axis.AxisNo, logical);
                    }
                }
                else if (name.Equals("Wafer Stage Y Axis", StringComparison.OrdinalIgnoreCase))
                {
                    var eq = Equipment.Instance;
                    if (eq?.Units != null && eq.Units.TryGetValue("InputStage", out var u3) && u3 is InputStage inStage)
                    {
                        double logical = inStage.GetTP(
                            InputStageConfig.TeachingPositionName.SetPosition.ToString(),
                            "Wafer Stage Y Axis");

                        AjinApi.SetActualPositionPulse(axis.AxisNo, logical);
                        AjinApi.SetCommandPositionPulse(axis.AxisNo, logical);
                    }
                }
                else if (name.Equals("Bin Stage X Axis", StringComparison.OrdinalIgnoreCase) ||
                         name.Equals("Bin Stage Y Axis", StringComparison.OrdinalIgnoreCase))
                {
                    var eq = Equipment.Instance;
                    if (eq?.Units != null && eq.Units.TryGetValue("OutputStage", out var u4) && u4 is OutputStage outStage)
                    {
                        string axisKey = name; // 그대로 사용
                        double logical = outStage.GetTP(
                            OutputStageConfig.TeachingPositionName.SetPosition.ToString(),
                            axisKey);

                        AjinApi.SetActualPositionPulse(axis.AxisNo, logical);
                        AjinApi.SetCommandPositionPulse(axis.AxisNo, logical);
                    }
                }
            }
            catch
            {
                // 필요 시 로깅
            }
        }
    }
}
