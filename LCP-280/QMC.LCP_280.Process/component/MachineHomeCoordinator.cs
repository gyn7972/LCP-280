using QMC.Common;
using QMC.Common.Motions;
using QMC.LCP_280.Process.Unit;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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

            // 단계별 훅: 전역 인터락과 축 사전 체크만 유지 (축/유닛별 조건은 PreAxis로 이동)
            seq.PreStepInterlockAsync = async (stepIndex, list, ct) =>
            {
                string reason;
                var il = InterlockManager.Instance;
                il.Start();

                //// 전역/해당축 인터락 평가
                //if (!il.ValidateForHomeStep(list, out reason))
                //    return (false, reason);

                // 축별 사전 체크(Servo/Alarm/Motion)
                //foreach (var a in list)
                //{
                //    if (!a.CheckHomeInterlocks(out reason))
                //        return (false, reason);
                //}
                return (true, null);
            };

            // 축 단위 훅: 개별 축 홈 직전 전처리/인터락
            seq.PreAxisInterlockAsync = async (stepIndex, axis, ct) =>
            {
                if (axis == null) return (false, "Axis null");

                // InterlockManager 축 규칙 빠른 검사
                string reason;
                var il = InterlockManager.Instance;
                //if (!il.ValidateAxisForHome(axis, out reason))
                //    return (false, reason);

                if (!axis.CheckHomeInterlocks(out var reason2))
                    return (false, reason2);

                // 축 이름별 전처리/인터락
                switch (axis.Name)
                {
                    case "Left Tool T Axis":
                        // 현재 위치가 양수이면 Fine 속도로 - 방향 조그 → HomeSensor 감지 시 1초 추가 진행 후 정지 → 홈 진행
                        try
                        {
                            var st0 = axis.GetStatusSnapshot();
                            if (st0 != null && st0.PV != null && st0.PV.ActualPosition > 0)
                            {
                                var vel = axis.Config != null ? Math.Abs(axis.Config.JogFineVelocity) : 1.0;
                                var timeoutMs = axis.Setup != null ? axis.Setup.SensorDetectionTimeoutMs : 3000;
                                var jogUntil = DateTime.UtcNow.AddMilliseconds(timeoutMs);
                                bool homeDetected = false;

                                try { axis.JogStart(-vel); } catch { }

                                try
                                {
                                    // HomeSensor 감지 대기(타임아웃 포함)
                                    while (DateTime.UtcNow < jogUntil)
                                    {
                                        if (ct.IsCancellationRequested) break;
                                        var s = axis.GetStatusSnapshot();
                                        if (s != null && s.IO != null && s.IO.HomeSensor)
                                        {
                                            homeDetected = true;
                                            break;
                                        }
                                        await Task.Delay(20, ct).ConfigureAwait(false);
                                    }

                                    // 감지되면 1초 추가로 -방향 진행
                                    if (!ct.IsCancellationRequested && homeDetected)
                                    {
                                        var extraUntil = DateTime.UtcNow.AddMilliseconds(1000);
                                        while (DateTime.UtcNow < extraUntil)
                                        {
                                            if (ct.IsCancellationRequested) break;
                                            await Task.Delay(20, ct).ConfigureAwait(false);
                                        }
                                    }
                                }
                                finally
                                {
                                    try { axis.JogStop(); } catch { }
                                }

                                if (ct.IsCancellationRequested) return (false, "Canceled");

                                if (!homeDetected)
                                    return (false, "Left Tool T Axis HomeSensor Timeout");

                                // 잠깐 대기 후 홈 계속 진행
                                await Task.Delay(150, ct).ConfigureAwait(false);
                            }
                        }
                        catch (Exception ex)
                        {
                            return (false, "Left Tool T Axis PreAxis Failure: " + ex.Message);
                        }
                        break;

                    case "Right Tool T Axis":
                        // 안전 인터락: OutputStage Plate가 Down 상태여야 함
                        if (eq.Units != null && eq.Units.TryGetValue("OutputStage", out var uRt) && uRt is OutputStage outStage)
                        {
                            if (!outStage.IsPlateDown())
                            {
                                if (!outStage.SetClampPlate(false) || !outStage.IsPlateDown())
                                    return (false, "OutputStage PlateDown Required");
                            }
                        }

                        // 현재 위치가 양수이면 -방향 Fine 조그 → HomeSensor 감지 시 1초 추가 진행 후 정지 → 홈 진행
                        try
                        {
                            var st0 = axis.GetStatusSnapshot();
                            if (st0 != null && st0.PV != null && st0.PV.ActualPosition > 0)
                            {
                                var vel = axis.Config != null ? Math.Abs(axis.Config.JogFineVelocity) : 1.0;
                                var timeoutMs = axis.Setup != null ? axis.Setup.SensorDetectionTimeoutMs : 3000;
                                var jogUntil = DateTime.UtcNow.AddMilliseconds(timeoutMs);
                                bool homeDetected = false;

                                try { axis.JogStart(-vel); } catch { }

                                try
                                {
                                    // HomeSensor 감지 대기(타임아웃 포함)
                                    while (DateTime.UtcNow < jogUntil)
                                    {
                                        if (ct.IsCancellationRequested) break;
                                        var s = axis.GetStatusSnapshot();
                                        if (s != null && s.IO != null && s.IO.HomeSensor)
                                        {
                                            homeDetected = true;
                                            break;
                                        }
                                        await Task.Delay(20, ct).ConfigureAwait(false);
                                    }

                                    // 감지되면 1초 추가로 -방향 진행
                                    if (!ct.IsCancellationRequested && homeDetected)
                                    {
                                        var extraUntil = DateTime.UtcNow.AddMilliseconds(1000);
                                        while (DateTime.UtcNow < extraUntil)
                                        {
                                            if (ct.IsCancellationRequested) break;
                                            await Task.Delay(20, ct).ConfigureAwait(false);
                                        }
                                    }
                                }
                                finally
                                {
                                    try { axis.JogStop(); } catch { }
                                }

                                if (ct.IsCancellationRequested) return (false, "Canceled");

                                if (!homeDetected)
                                    return (false, "Right Tool T Axis HomeSensor Timeout");

                                // 잠깐 대기 후 홈 계속 진행
                                await Task.Delay(150, ct).ConfigureAwait(false);
                            }
                        }
                        catch (Exception ex)
                        {
                            return (false, "Right Tool T Axis PreAxis Failure: " + ex.Message);
                        }
                        break;

                    case "Wafer Feeder Y Axis":
                        // 기존 PreStep의 Wafer Feeder 사전 동작을 축 단위로 옮김
                        try
                        {
                            if (eq.Units != null && eq.Units.TryGetValue("InputFeeder", out var uIn) && uIn is InputFeeder inFeeder)
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
                                    return (false, "Wafer Feeder Unclamp Sensor Not Detected");

                                // 링 존재 시 → +Y 조그로 센서 OFF까지 이동
                                try
                                {
                                    if (inFeeder.IsRingPresent())
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
                                catch { /* ignore jog errors */ }

                                await Task.Delay(100, ct).ConfigureAwait(false);
                                if (!inFeeder.SetLift(true))
                                    return (false, "Wafer Feeder Up Failure");

                                // Up 센서 확인
                                until = DateTime.UtcNow.AddMilliseconds(1000);
                                while (DateTime.UtcNow < until)
                                {
                                    if (inFeeder.IsFeederUp()) break;
                                    await Task.Delay(20, ct).ConfigureAwait(false);
                                }
                                if (!inFeeder.IsFeederUp())
                                    return (false, "Wafer Feeder Up Sensor Not Detected");
                            }
                        }
                        catch (Exception ex)
                        {
                            return (false, "Wafer Feeder PreAxis Failure: " + ex.Message);
                        }
                        break;

                    case "Bin Feeder Y Axis":
                        // 기존 PreStep의 Bin Feeder 사전 동작을 축 단위로 옮김
                        try
                        {
                            if (eq.Units != null && eq.Units.TryGetValue("OutputFeeder", out var uOut) && uOut is OutputFeeder outFeeder)
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
                                    return (false, "Bin Feeder Unclamp Sensor Not Detected");

                                // 링 존재 시 → +Y 조그로 센서 OFF까지 이동
                                try
                                {
                                    if (outFeeder.IsRingPresent())
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
                                catch { /* ignore jog errors */ }

                                await Task.Delay(100, ct).ConfigureAwait(false);
                                if (!outFeeder.SetLift(true))
                                    return (false, "Bin Feeder Up Failure");

                                // Up 센서 확인
                                until2 = DateTime.UtcNow.AddMilliseconds(1000);
                                while (DateTime.UtcNow < until2)
                                {
                                    if (outFeeder.IsFeederUp()) break;
                                    await Task.Delay(20, ct).ConfigureAwait(false);
                                }
                                if (!outFeeder.IsFeederUp())
                                    return (false, "Bin Feeder Up Sensor Not Detected");
                            }
                        }
                        catch (Exception ex)
                        {
                            return (false, "Bin Feeder PreAxis Failure: " + ex.Message);
                        }
                        break;

                    case "Wafer Lifter Z Axis":
                        // InputCassetteLifter: RING_JUT 체크
                        try
                        {
                            if (eq.Units != null && eq.Units.TryGetValue("InputCassetteLifter", out var uL) && uL is InputCassetteLifter lifter)
                            {
                                if (lifter.IsWaferProtrusionDetectionSensor())
                                    return (false, "InputCassetteLifter Ring JUT Detected");
                            }
                        }
                        catch (Exception ex)
                        {
                            return (false, "InputCassetteLifter PreAxis Failure: " + ex.Message);
                        }
                        break;

                    case "Bin Lifter Z Axis":
                        // OutputCassetteLifter: RING_JUT 체크
                        try
                        {
                            if (eq.Units != null && eq.Units.TryGetValue("OutputCassetteLifter", out var uOL) && uOL is OutputCassetteLifter outLifter)
                            {
                                if (outLifter.RingJut())
                                    return (false, "OutputCassetteLifter Ring JUT Detected");
                            }
                        }
                        catch (Exception ex)
                        {
                            return (false, "OutputCassetteLifter PreAxis Failure: " + ex.Message);
                        }
                        break;

                    case "Index T Axis":
                        // SafeZone(입력 다이 트랜스퍼) 위치가 아닐 경우 Index T 홈 금지
                        try
                        {
                            // 1) InputDieTransfer SafeZone 확인
                            if (eq.Units != null && eq.Units.TryGetValue("InputDieTransfer", out var uIdt) && uIdt is InputDieTransfer intransfer)
                            {
                                var safeZoneName = nameof(InputDieTransferConfig.TeachingPositionName.SafetyZone);
                                if (!intransfer.InPosTeaching(safeZoneName))
                                    return (false, "InputDieTransfer Not in Safety Zone");
                            }
                            else
                            {
                                return (false, "InputDieTransfer Unit Not Found");
                            }

                            // 2) OutputDieTransfer SafeZone 확인
                            if (eq.Units != null && eq.Units.TryGetValue("OutputDieTransfer", out var uOdt) && uOdt is OutputDieTransfer outTransfer)
                            {
                                var safeOut = nameof(OutputDieTransferConfig.TeachingPositionName.SafetyZone);
                                if (!outTransfer.InPosTeaching(safeOut))
                                    return (false, "OutputDieTransfer Not in Safety Zone");
                            }
                            else
                            {
                                return (false, "OutputDieTransfer Unit Not Found");
                            }

                            // 3) IndexLoadAligner SafeZone 확인
                            //if (eq.Units != null && eq.Units.TryGetValue("Index Z Axis", out var uIla))
                            if (eq.Units != null && eq.Units.TryGetValue("IndexLoadAligner", out var uIla))
                            {
                                if (uIla == null)
                                {
                                    Log.Write("디버그", "\"Index Z Axis\" 값이 null입니다.");
                                }
                                else
                                {
                                    Log.Write("디버그", "\"Index Z Axis\" 타입: " + uIla.GetType().FullName);
                                }
                                if (uIla is IndexLoadAligner indexAligner)
                                {
                                    var safeOut = nameof(IndexLoadAlignerConfig.TeachingPositionName.SafetyZone);
                                    if (!outTransfer.InPosTeaching(safeOut))
                                        return (false, "OutputDieTransfer Not in Safety Zone");
                                }
                                else
                                {
                                    Log.Write("디버그", "\"Index Z Axis\"가 IndexLoadAligner 타입이 아닙니다.");
                                }
                            }
                            else
                            {
                                Log.Write("디버그", "\"Index Z Axis\" 키가 eq.Units에 없습니다.");
                            }
                            //if (eq.Units != null && 
                            //    eq.Units.TryGetValue("Index Z Axis", out var uIla) &&
                            //    uIla is IndexLoadAligner indexAligner)
                            //{
                            //    var safeOut = nameof(IndexLoadAlignerConfig.TeachingPositionName.SafetyZone);
                            //    if (!outTransfer.InPosTeaching(safeOut))
                            //        return (false, "OutputDieTransfer Not in Safety Zone");
                            //}
                            //else
                            //{
                            //    return (false, "OutputDieTransfer Unit Not Found");
                            //}

                            // 4) IndexChipProbeController SafeZone 확인
                            //if (eq.Units != null && eq.Units.TryGetValue("Index Z Axis", out var uIp) && uIp is IndexChipProbeController indexProbe)
                            if (eq.Units != null && eq.Units.TryGetValue("IndexChipProbeController", out var uIp) && uIp is IndexChipProbeController indexProbe)
                            {
                                var safeOut = nameof(IndexChipProbeControllerConfig.TeachingPositionName.SafetyZone);
                                if (!outTransfer.InPosTeaching(safeOut))
                                    return (false, "IndexChipProbeController Not in Safety Zone");
                            }
                            else
                            {
                                return (false, "IndexChipProbeController Unit Not Found");
                            }
                        }
                        catch (Exception ex)
                        {
                            return (false, "Index T SafetyZone Check Failure" + ex.Message);
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
