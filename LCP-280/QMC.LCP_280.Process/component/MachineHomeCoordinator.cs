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
                    "Probe Card X Axis", "Wafer Feeder Y Axis", "Bin Feeder Y Axis")
                .AddParallelStepByAxisNames(
                    "Index T Axis", "Wafer Stage Y Axis", "Bin Stage Y Axis", "Wafer Lifter Z Axis", "Bin Lifter Z Axis")
                .AddParallelStepByAxisNames(
                    "Wafer Stage X Axis", "Bin Stage X Axis")
                .AddParallelStepByAxisNames(
                    "Wafer Stage T Axis", "Bin Stage T Axis");

            // 단계별 훅: 도어/실린더 등 전역 인터락과 축 사전 체크, 피더/스테이지 전용 IO 동작
            seq.PreStepInterlockAsync = async (stepIndex, list, ct) =>
            {
                string reason;
                var il = InterlockManager.Instance; il.Start();

                // 이 단계에 포함된 축이 무엇인지 검사하여 해당 유닛의 인터락 동작 수행
                bool needWaferFeeder = list != null && list.Any(a => a != null && a.Name.Equals("Wafer Feeder Y Axis", StringComparison.OrdinalIgnoreCase));
                bool needBinFeeder = list != null && list.Any(a => a != null && a.Name.Equals("Bin Feeder Y Axis", StringComparison.OrdinalIgnoreCase));
                bool needRightToolT = list != null && list.Any(a => a != null && a.Name.Equals("Right Tool T Axis", StringComparison.OrdinalIgnoreCase));

                if (needWaferFeeder)
                {
                    try
                    {
                        if (eq.Units != null && eq.Units.TryGetValue("InputRingTransfer", out var u) && u is InputRingTransfer inFeeder)
                        {
                            // Unclamp → 센서 확인
                            inFeeder.SetClamp(false);
                            var until = DateTime.UtcNow.AddMilliseconds(1500);
                            while (DateTime.UtcNow < until)
                            {
                                if (inFeeder.IsUnclamped()) break;
                                await Task.Delay(20, ct).ConfigureAwait(false);
                            }
                            if (!inFeeder.IsUnclamped())
                                return (false, "Wafer Feeder Unclamp 센서 미확인");

                            // 링 존재 시 → +Y 조그로 센서 OFF까지 이동 (Fine Velocity, SensorDetectionTimeoutMs 사용)
                            try
                            {
                                if (inFeeder.IsRingPresent())
                                {
                                    var axis = list.FirstOrDefault(a => a != null && a.Name.Equals("Wafer Feeder Y Axis", StringComparison.OrdinalIgnoreCase));
                                    if (axis != null)
                                    {
                                        var vel = axis.Config != null ? Math.Abs(axis.Config.JogFineVelocity) : 1.0;
                                        var timeoutMs = axis.Setup != null ? axis.Setup.SensorDetectionTimeoutMs : 3000;
                                        var jogUntil = DateTime.UtcNow.AddMilliseconds(timeoutMs);
                                        try { axis.JogStart(+vel); } catch { }
                                        try
                                        {
                                            while (DateTime.UtcNow < jogUntil)
                                            {
                                                if (ct.IsCancellationRequested) break;
                                                if (!inFeeder.IsRingPresent()) break;
                                                await Task.Delay(20, ct).ConfigureAwait(false);
                                            }
                                        }
                                        finally
                                        {
                                            try { axis.JogStop(); } catch { }
                                        }
                                        if (inFeeder.IsRingPresent())
                                            return (false, "Wafer Feeder Ring Clear Timeout");
                                    }
                                }
                            }
                            catch { /* ignore jog errors, fallback to next checks */ }

                            await Task.Delay(100, ct).ConfigureAwait(false);
                            if (!inFeeder.FeederUp(3000))
                                return (false, "Wafer Feeder Up 실패");

                            // Up 센서 확인
                            until = DateTime.UtcNow.AddMilliseconds(1000);
                            while (DateTime.UtcNow < until)
                            {
                                if (inFeeder.IsFeederUp()) break;
                                await Task.Delay(20, ct).ConfigureAwait(false);
                            }
                            if (!inFeeder.IsFeederUp())
                                return (false, "Wafer Feeder Up 센서 미확인");
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
                            // Unclamp → 센서 확인
                            outFeeder.SetClamp(false);
                            var until2 = DateTime.UtcNow.AddMilliseconds(1500);
                            while (DateTime.UtcNow < until2)
                            {
                                if (outFeeder.IsUnclamped()) break;
                                await Task.Delay(20, ct).ConfigureAwait(false);
                            }
                            if (!outFeeder.IsUnclamped())
                                return (false, "Bin Feeder Unclamp 센서 미확인");

                            // 링 존재 시 → +Y 조그로 센서 OFF까지 이동 (Fine Velocity, SensorDetectionTimeoutMs 사용)
                            try
                            {
                                if (outFeeder.IsRingPresent())
                                {
                                    var axis = list.FirstOrDefault(a => a != null && a.Name.Equals("Bin Feeder Y Axis", StringComparison.OrdinalIgnoreCase));
                                    if (axis != null)
                                    {
                                        var vel = axis.Config != null ? Math.Abs(axis.Config.JogFineVelocity) : 1.0;
                                        var timeoutMs = axis.Setup != null ? axis.Setup.SensorDetectionTimeoutMs : 3000;
                                        var jogUntil = DateTime.UtcNow.AddMilliseconds(timeoutMs);
                                        try { axis.JogStart(+vel); } catch { }
                                        try
                                        {
                                            while (DateTime.UtcNow < jogUntil)
                                            {
                                                if (ct.IsCancellationRequested) break;
                                                if (!outFeeder.IsRingPresent()) break;
                                                await Task.Delay(20, ct).ConfigureAwait(false);
                                            }
                                        }
                                        finally
                                        {
                                            try { axis.JogStop(); } catch { }
                                        }
                                        if (outFeeder.IsRingPresent())
                                            return (false, "Wafer Feeder Ring Clear Timeout");
                                    }
                                }
                            }
                            catch { /* ignore jog errors, fallback to next checks */ }

                            await Task.Delay(100, ct).ConfigureAwait(false);
                            if (!outFeeder.FeederUp(3000))
                                return (false, "Bin Feeder Up 실패");

                            // Up 센서 확인
                            until2 = DateTime.UtcNow.AddMilliseconds(1000);
                            while (DateTime.UtcNow < until2)
                            {
                                if (outFeeder.IsFeederUp()) break;
                                await Task.Delay(20, ct).ConfigureAwait(false);
                            }
                            if (!outFeeder.IsFeederUp())
                                return (false, "Bin Feeder Up 센서 미확인");
                        }
                    }
                    catch (Exception ex)
                    {
                        return (false, "Bin Feeder PreStep 실패: " + ex.Message);
                    }
                }

                if (needRightToolT)
                {
                    try
                    {
                        if (eq.Units != null && eq.Units.TryGetValue("OutputStage", out var u3) && u3 is OutputStage outStage)
                        {
                            if (!outStage.PlateDown(3000))
                                return (false, "OutputStage PlateDown 실패");
                            await Task.Delay(50, ct).ConfigureAwait(false);
                            if (!outStage.IsPlateDown())
                                return (false, "OutputStage PlateDown 센서 미확인");
                        }
                    }
                    catch (Exception ex)
                    {
                        return (false, "OutputStage PreStep 실패: " + ex.Message);
                    }
                }

                // 전역/해당축 인터락 평가
                //if (!il.ValidateForHomeStep(list, out reason))
                //    return (false, reason);

                // 축별 사전 체크(Servo/Alarm/Motion)
                foreach (var a in list)
                {
                    if (!a.CheckHomeInterlocks(out reason))
                        return (false, reason);
                }
                return (true, null);
            };

            // 축 단위 훅: 개별 축 홈 직전 전처리/인터락
            seq.PreAxisInterlockAsync = async (stepIndex, axis, ct) =>
            {
                if (axis == null) return (false, "Axis null");

                // 예시: InterlockManager 축 규칙 빠른 검사
                string reason;
                var il = InterlockManager.Instance;
                if (!il.ValidateAxisForHome(axis, out reason))
                    return (false, reason);

                // 축 이름별 전처리/인터락 샘플
                switch (axis.Name)
                {
                    case "Right Tool T Axis":
                        if (eq.Units != null && eq.Units.TryGetValue("OutputStage", out var u) && u is OutputStage outStage)
                        {
                            if (!outStage.IsPlateDown())
                            {
                                if (!outStage.PlateDown(2000) || !outStage.IsPlateDown())
                                    return (false, "OutputStage PlateDown 필요");
                            }
                        }
                        break;
                    default:
                        break;
                }

                if (ct.IsCancellationRequested) return (false, "Canceled");
                return (true, null);
            };

            // PostStep은 글로벌 이벤트(HomeHooks)에서 처리하므로 여기선 설정하지 않음

            return seq;
        }
    }
}
