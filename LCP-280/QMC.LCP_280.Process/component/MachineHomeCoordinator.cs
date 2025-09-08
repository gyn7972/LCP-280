using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using QMC.Common.Motions;
using QMC.LCP_280.Process.Unit;

namespace QMC.LCP_280.Process.Component
{
    /// <summary>
    /// 설비 전체 홈 시퀀스 구성을 담당하는 코디네이터.
    /// - UI(Form)에서 직접 훅을 꾸미지 않고, 여기서 단계 구성과 PreStep 동작/인터락을 일괄 관리
    /// - 유지보수/재사용 용이
    /// </summary>
    public static class MachineHomeCoordinator
    {
        /// <summary>
        /// 기본 홈 시퀀스를 구성합니다. 단계별 인터락/사전 IO 동작을 포함합니다.
        /// </summary>
        public static HomeSequence BuildDefaultHomeSequence(Equipment eq)
        {
            if (eq == null) throw new ArgumentNullException(nameof(eq));
            var mgr = eq.AxisManager ?? throw new InvalidOperationException("AxisManager not initialized");

            var seq = new HomeSequence(mgr)
                .AddParallelStepByAxisNames(
                    "Eject Pin Z Axis", "Ejector Z Axis", "Left Pick Z Axis", "Left Place Z Axis", "Right Pick Z Axis",
                    "Right Place Z Axis", "Index Z Axis", "Sphere Z Axis", "Probe Z Axis", "Probe Card Z Axis")
                .AddParallelStepByAxisNames(
                    "Left Tool T Axis", "Right Tool T Axis", "Probe Card Y Axis", "Align T Axis")
                .AddParallelStepByAxisNames(
                    "Wafer Feeder Y Axis", "Bin Feeder Y Axis");

            // 단계별 훅: 도어/실린더 등 전역 인터락과 축 사전 체크, 피더축 전용 IO 동작
            seq.PreStepInterlockAsync = async (stepIndex, list, ct) =>
            {
                string reason;
                var il = InterlockManager.Instance; il.Start();

                // 이 단계에 포함된 축이 무엇인지 검사하여 해당 유닛의 인터락 동작 수행
                bool needWaferFeeder = list != null && list.Any(a => a != null && a.Name.Equals("Wafer Feeder Y Axis", StringComparison.OrdinalIgnoreCase));
                bool needBinFeeder = list != null && list.Any(a => a != null && a.Name.Equals("Bin Feeder Y Axis", StringComparison.OrdinalIgnoreCase));

                if (needWaferFeeder)
                {
                    try
                    {
                        if (eq.Units != null && eq.Units.TryGetValue("InputRingTransfer", out var u) && u is InputRingTransfer inFeeder)
                        {
                            inFeeder.SetClamp(false);
                            await Task.Delay(100, ct).ConfigureAwait(false);
                            if (!inFeeder.FeederUp(3000))
                                return (false, "Wafer Feeder Up 실패");
                        }
                    }
                    catch (Exception ex)
                    {
                        return (false, "Wafer Feeder PreStep 실패: " + ex.Message);
                    }
                }

                if (needBinFeeder)
                {
                    try
                    {
                        if (eq.Units != null && eq.Units.TryGetValue("OutputRingTransfer", out var u2) && u2 is OutputRingTransfer outFeeder)
                        {
                            outFeeder.SetClamp(false);
                            await Task.Delay(100, ct).ConfigureAwait(false);
                            if (!outFeeder.FeederUp(3000))
                                return (false, "Bin Feeder Up 실패");
                        }
                    }
                    catch (Exception ex)
                    {
                        return (false, "Bin Feeder PreStep 실패: " + ex.Message);
                    }
                }

                // 전역/해당축 인터락 평가
                if (!il.ValidateForHomeStep(list, out reason))
                    return (false, reason);

                // 축별 사전 체크(Servo/Alarm/Motion)
                foreach (var a in list)
                {
                    if (!a.CheckHomeInterlocks(out reason))
                        return (false, reason);
                }
                return (true, null);
            };

            return seq;
        }
    }
}
