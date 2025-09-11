using QMC.Common;
using QMC.LCP_280.Process.Unit;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace QMC.LCP_280.Process.Component.Sequence
{
    internal class InputStageSequence
    {

        public enum TAlignResult
        {
            Ok = 0,
            RingAbsent = -1,
            ClampLiftNotUp = -2,
            ClampNotFwd = -3,
            FeederBusy = -10,
            EjectorBusy = -11,
            DieTransferBusy = -12,
            MoveCenterCommandFail = -20,
            MoveCenterTimeout = -21,
            CameraNull = -30,
            GrabFail = -31,
            VisionFail = -40,
            NoAngleData = -41,
            AngleOverLimit = -42,
            ApplyOffsetFail = -50,
            CorrectionMoveTimeout = -51,
            Exception = -100
        }

        public enum XYAlignResult
        {
            Ok = 0,
            RingAbsent = -1,
            ClampLiftNotUp = -2,
            ClampNotFwd = -3,
            FeederBusy = -10,
            EjectorBusy = -11,
            DieTransferBusy = -12,
            MoveCenterCommandFail = -20,
            MoveCenterTimeout = -21,
            CameraNull = -30,
            GrabFail = -31,
            VisionFail = -40,
            NoXYData = -41,
            XYOverLimit = -42,
            ApplyOffsetFail = -50,
            CorrectionMoveTimeout = -51,
            Exception = -100
        }

        private readonly InputStage _stage;

        // 외부설비 안전 여부 델리게이트(상위에서 주입) // 인터락 주입하는 용도
        public Func<bool> IsFeederSafeFunc { get; set; }
        public Func<bool> IsEjectorSafeFunc { get; set; }
        public Func<bool> IsDieTransferSafeFunc { get; set; }

        // 파라미터 (필요 시 외부 설정화)
        public int MoveTimeoutMs { get; set; } = 8000;
        public int PollIntervalMs { get; set; } = 20;
        public double AngleIgnoreThresholdDeg { get; set; } = 0.001;
        public double AngleMaxApplyDeg { get; set; } = 2.0;
        public double AngleApplyGain { get; set; } = 1.0; // 방향 반전 필요 시 -1 사용
        public bool UseOffsetForTAxisCorrection { get; set; } = true; // false면 직접 축 이동 방식으로 전환 가능 (추후 확장)


        public InputStageSequence(InputStage stage)
        {
            _stage = stage;
        }

        public int T_Align() 
        {
            int ret = -1;

            try
            {
                Log.Write("InputStageSequence", "T_Align", "T_Align Start");

                // stageCenter 이동전 인터락. // 필요한 인터락을 상황에 맞게 추가/수정
                // 다른 장비 동작중인지 확인 (Interlock) - 어떻게 하지? 
                // 상위에서 T_Align 호출시 다른 장비 동작중인지 확인하고 호출해야 하나?
                // 1) 기본 Stage 자체 인터락
                if (!_stage.IsRingPresent())
                {
                    Log.Write("InputStageSequence", "T_Align", "Fail: Ring(Wafer) not present");
                    return (int)TAlignResult.RingAbsent;
                }
                if (!_stage.IsClampLiftUp())
                {
                    Log.Write("InputStageSequence", "T_Align", "Fail: Clamp Lift not Up");
                    return (int)TAlignResult.ClampLiftNotUp;
                }
                if (!_stage.IsClampFwd())
                {
                    Log.Write("InputStageSequence", "T_Align", "Fail: Clamp not FWD");
                    return (int)TAlignResult.ClampNotFwd;
                }

                // Stage 이동 전 인터락
                // 상태를 확인 하든.. 아래 조건을 확인해야함. 
                // Input Ring Transfer 위치 확인 (WAFER_FEEDER_Y, WAFER FEEDER DOWN)
                // InputStageEjector 위치 확인 (EjectPinZ, EjectorZ)
                // Input Die Transfer 위치 확인 (LEFT PICK_Z, LEFT_TOOL_T?, LEFT PLACE_Z?)
                // 2) 외부설비(Feeder/Ejector/Die Transfer 등) 충돌 위험 인터락
                if (IsFeederSafeFunc != null && !IsFeederSafeFunc())
                {
                    Log.Write("InputStageSequence", "T_Align", "Fail: Feeder area not safe");
                    return (int)TAlignResult.FeederBusy;
                }
                if (IsEjectorSafeFunc != null && !IsEjectorSafeFunc())
                {
                    Log.Write("InputStageSequence", "T_Align", "Fail: Ejector area not safe");
                    return (int)TAlignResult.EjectorBusy;
                }
                if (IsDieTransferSafeFunc != null && !IsDieTransferSafeFunc())
                {
                    Log.Write("InputStageSequence", "T_Align", "Fail: DieTransfer area not safe");
                    return (int)TAlignResult.DieTransferBusy;
                }

                // stageCenter 이동.
                var centerTp = _stage.TeachingPositions[InputStageConfig.TeachingPositionName.CenterPoint];
                if (centerTp == null)
                {
                    Log.Write("InputStageSequence", "T_Align", "Fail: CenterPoint teaching not defined");
                    return (int)TAlignResult.MoveCenterCommandFail;
                }
                if (_stage.MoveToTeachingPosition(centerTp) != 0)
                {
                    Log.Write("InputStageSequence", "T_Align", "Fail: Move center command");
                    return (int)TAlignResult.MoveCenterCommandFail;
                }
                if (!WaitUntil(() => _stage.InPosTeaching(centerTp), MoveTimeoutMs))
                {
                    Log.Write("InputStageSequence", "T_Align", "Fail: Move center timeout");
                    return (int)TAlignResult.MoveCenterTimeout;
                }
                Log.Write("InputStageSequence", "T_Align", "CenterPoint InPos OK");

                // stageCenter 이동 완료 확인.
                if (!_stage.InPosTeaching(InputStageConfig.TeachingPositionName.CenterPoint))
                {
                    return ret;
                }

                // 조명 셋팅 및 Image 그랩
                // 4) 카메라 / 이미지 확보
                if (_stage.StageCamera == null)
                {
                    Log.Write("InputStageSequence", "T_Align", "Fail: Camera null");
                    return (int)TAlignResult.CameraNull;
                }
                var grabRc = _stage.StageCamera.GrabSync(out var img);
                if (grabRc != 0 || img == null || img.RawData == null)
                {
                    Log.Write("InputStageSequence", "T_Align", $"Fail: Grab fail rc={grabRc}");
                    return (int)TAlignResult.GrabFail;
                }
                _stage.StageCamera.LatestImage = img;
                Log.Write("InputStageSequence", "T_Align", "Grab OK");

                // 패턴 매칭 및 각도 보정값 산출
                // 5) Vision (Angle 검색) - InputStage 래퍼 사용
                if (!_stage.TryGetMultiAngles(out var angleList) || angleList == null || angleList.Count == 0)
                {
                    Log.Write("InputStageSequence", "T_Align", "Fail: Vision angle search fail or empty");
                    return (int)TAlignResult.VisionFail;
                }

                // 통계 계산
                var stats = ComputeAngleStats(angleList, excludeExtremes: true);
                if (stats.RawCount == 0)
                {
                    Log.Write("InputStageSequence", "T_Align", "Fail: No angle list after filtering");
                    return (int)TAlignResult.NoAngleData;
                }

                double rawAngle = stats.Representative; // 대표각
                Log.Write("InputStageSequence", "T_Align",
                    $"Angle Representative={rawAngle:F6} avg={stats.Average:F6} std={stats.StdDev:F6} rawCount={stats.RawCount}");

                if (Math.Abs(rawAngle) < AngleIgnoreThresholdDeg)
                {
                    Log.Write("InputStageSequence", "T_Align", "Angle below ignore threshold → treat as 0, skip correction");
                    return (int)TAlignResult.Ok;
                }
                if (Math.Abs(rawAngle) > AngleMaxApplyDeg)
                {
                    Log.Write("InputStageSequence", "T_Align",
                        $"Fail: Angle {rawAngle:F4} over max limit {AngleMaxApplyDeg}");
                    return (int)TAlignResult.AngleOverLimit;
                }

                double applyAngle = rawAngle * AngleApplyGain;

                // 6) 보정 적용
                bool correctionOk;
                if (UseOffsetForTAxisCorrection)
                {
                    correctionOk = ApplyTAxisByOffset(centerTp.Name, applyAngle);
                    Log.Write("InputStageSequence", "T_Align", $"ApplyOffset(T) angle={applyAngle:F6} -> {(correctionOk ? "OK" : "FAIL")}");
                }
                else
                {
                    // 직접 축 이동 (T 절대위치 + delta). Teaching + Offset 구조와 충돌되지 않도록 주의.
                    correctionOk = MoveTAxisDelta(applyAngle);
                    Log.Write("InputStageSequence", "T_Align", $"Direct T Move angle delta={applyAngle:F6} -> {(correctionOk ? "OK" : "FAIL")}");
                }
                if (!correctionOk)
                    return (int)TAlignResult.ApplyOffsetFail;

                // 7) 재-중심 InPos 확인 (Offset 적용 시 재이동 필요)
                if (UseOffsetForTAxisCorrection)
                {
                    if (_stage.MoveToTeachingPosition(centerTp) != 0)
                    {
                        Log.Write("InputStageSequence", "T_Align", "Fail: Move center after correction");
                        return (int)TAlignResult.MoveCenterCommandFail;
                    }
                    if (!WaitUntil(() => _stage.InPosTeaching(centerTp), MoveTimeoutMs))
                    {
                        Log.Write("InputStageSequence", "T_Align", "Fail: Correction move timeout");
                        return (int)TAlignResult.CorrectionMoveTimeout;
                    }
                }
                else
                {
                    // Direct 이동 방식이면 InPos 직접 확인
                    if (!WaitUntil(() =>
                    {
                        var (_, _, t) = _stage.InputStageConfig.GetPositionWithOffset(centerTp.Name);
                        return _stage.InPos(_stage.AxisT, t);
                    }, MoveTimeoutMs))
                    {
                        Log.Write("InputStageSequence", "T_Align", "Fail: T axis in-position timeout (direct)");
                        return (int)TAlignResult.CorrectionMoveTimeout;
                    }
                }

                Log.Write("InputStageSequence", "T_Align", "=== T_Align DONE ===");
                return (int)TAlignResult.Ok;
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                return (int)TAlignResult.Exception;
            }
        }

        private bool WaitUntil(Func<bool> cond, int timeoutMs)
        {
            var sw = Stopwatch.StartNew();
            while (sw.ElapsedMilliseconds < timeoutMs)
            {
                if (cond()) return true;
                Thread.Sleep(PollIntervalMs);
            }
            return false;
        }

        private bool ApplyTAxisByOffset(string centerName, double deltaAngleDeg)
        {
            try
            {
                _stage.ApplyOffset(centerName, 0, 0, deltaAngleDeg);
                return true;
            }
            catch (Exception ex)
            {
                Log.Write("InputStageSequence", "T_Align", "ApplyOffset exception: " + ex.Message);
                return false;
            }
        }

        //public int XY_Align()
        //{
        //    int ret = -1;
        //    try
        //    {
        //        Log.Write("InputStageSequence", "XY_Align", "XY_Align Start");

        //        // 기본 인터락
        //        if (!_stage.IsRingPresent())
        //        {
        //            Log.Write("InputStageSequence", "XY_Align", "Fail: Ring(Wafer) not present");
        //            return (int)XYAlignResult.RingAbsent;
        //        }
        //        if (!_stage.IsClampLiftUp())
        //        {
        //            Log.Write("InputStageSequence", "XY_Align", "Fail: Clamp Lift not Up");
        //            return (int)XYAlignResult.ClampLiftNotUp;
        //        }
        //        if (!_stage.IsClampFwd())
        //        {
        //            Log.Write("InputStageSequence", "XY_Align", "Fail: Clamp not FWD");
        //            return (int)XYAlignResult.ClampNotFwd;
        //        }

        //        // 외부설비 안전
        //        if (IsFeederSafeFunc != null && !IsFeederSafeFunc())
        //        {
        //            Log.Write("InputStageSequence", "XY_Align", "Fail: Feeder area not safe");
        //            return (int)XYAlignResult.FeederBusy;
        //        }
        //        if (IsEjectorSafeFunc != null && !IsEjectorSafeFunc())
        //        {
        //            Log.Write("InputStageSequence", "XY_Align", "Fail: Ejector area not safe");
        //            return (int)XYAlignResult.EjectorBusy;
        //        }
        //        if (IsDieTransferSafeFunc != null && !IsDieTransferSafeFunc())
        //        {
        //            Log.Write("InputStageSequence", "XY_Align", "Fail: DieTransfer area not safe");
        //            return (int)XYAlignResult.DieTransferBusy;
        //        }

        //        // Center 이동
        //        var centerTp = _stage.TeachingPositions[InputStageConfig.TeachingPositionName.CenterPoint];
        //        if (centerTp == null)
        //        {
        //            Log.Write("InputStageSequence", "XY_Align", "Fail: CenterPoint teaching not defined");
        //            return (int)XYAlignResult.MoveCenterCommandFail;
        //        }
        //        if (_stage.MoveToTeachingPosition(centerTp) != 0)
        //        {
        //            Log.Write("InputStageSequence", "XY_Align", "Fail: Move center command");
        //            return (int)XYAlignResult.MoveCenterCommandFail;
        //        }
        //        if (!WaitUntil(() => _stage.InPosTeaching(centerTp), MoveTimeoutMs))
        //        {
        //            Log.Write("InputStageSequence", "XY_Align", "Fail: Move center timeout");
        //            return (int)XYAlignResult.MoveCenterTimeout;
        //        }
        //        Log.Write("InputStageSequence", "XY_Align", "CenterPoint InPos OK");

        //        if (!_stage.InPosTeaching(InputStageConfig.TeachingPositionName.CenterPoint))
        //            return ret;

        //        // 카메라 & Grab
        //        if (_stage.StageCamera == null)
        //        {
        //            Log.Write("InputStageSequence", "XY_Align", "Fail: Camera null");
        //            return (int)XYAlignResult.CameraNull;
        //        }
        //        var grabRc = _stage.StageCamera.GrabSync(out var img);
        //        if (grabRc != 0 || img == null || img.RawData == null)
        //        {
        //            Log.Write("InputStageSequence", "XY_Align", $"Fail: Grab fail rc={grabRc}");
        //            return (int)XYAlignResult.GrabFail;
        //        }
        //        _stage.StageCamera.LatestImage = img;
        //        Log.Write("InputStageSequence", "XY_Align", "Grab OK");

        //        // Vision Center Offset 검색
        //        double dx = 0.0, dy = 0.0;
        //        bool visionOk = false;

        //        if (_stage.DryRun)
        //        {
        //            // DryRun → 보정 필요 없음
        //            visionOk = true;
        //            dx = 0;
        //            dy = 0;
        //        }
        //        else
        //        {
        //            var res = VisionRunnerHub.SearchCenterOffset(
        //                _stage.StageCameraKey,
        //                _stage.PixelSizeXmm,
        //                _stage.PixelSizeYmm,
        //                _stage.ImageOriginX,
        //                _stage.ImageOriginY,
        //                _stage.UseImageCenterAsOrigin);

        //            if (res.ok)
        //            {
        //                visionOk = true;
        //                dx = res.dxMm;
        //                dy = res.dyMm;
        //            }
        //        }

        //        if (!visionOk)
        //        {
        //            Log.Write("InputStageSequence", "XY_Align", "Fail: Vision center search fail");
        //            return (int)XYAlignResult.VisionFail;
        //        }

        //        Log.Write("InputStageSequence", "XY_Align", $"Vision Offset Raw dx={dx:F6} dy={dy:F6}");

        //        // 데이터 유효성
        //        if (double.IsNaN(dx) || double.IsNaN(dy))
        //        {
        //            Log.Write("InputStageSequence", "XY_Align", "Fail: NaN offset");
        //            return (int)XYAlignResult.NoXYData;
        //        }

        //        double dist = Math.Sqrt(dx * dx + dy * dy);
        //        if (dist < XYIgnoreThresholdMm)
        //        {
        //            Log.Write("InputStageSequence", "XY_Align", "Offset below ignore threshold → skip correction");
        //            return (int)XYAlignResult.Ok;
        //        }
        //        if (Math.Abs(dx) > XYMaxApplyMm || Math.Abs(dy) > XYMaxApplyMm)
        //        {
        //            Log.Write("InputStageSequence", "XY_Align",
        //                $"Fail: Offset over limit dx={dx:F4} dy={dy:F4} limit={XYMaxApplyMm}");
        //            return (int)XYAlignResult.XYOverLimit;
        //        }

        //        double applyDx = dx * XYApplyGain;
        //        double applyDy = dy * XYApplyGain;

        //        bool correctionOk;
        //        if (UseOffsetForXYCorrection)
        //        {
        //            correctionOk = ApplyXYByOffset(centerTp.Name, applyDx, applyDy);
        //            Log.Write("InputStageSequence", "XY_Align",
        //                $"ApplyOffset(XY) dx={applyDx:F6} dy={applyDy:F6} -> {(correctionOk ? "OK" : "FAIL")}");
        //        }
        //        else
        //        {
        //            correctionOk = MoveXYAxisDelta(applyDx, applyDy);
        //            Log.Write("InputStageSequence", "XY_Align",
        //                $"Direct Move XY delta dx={applyDx:F6} dy={applyDy:F6} -> {(correctionOk ? "OK" : "FAIL")}");
        //        }

        //        if (!correctionOk)
        //            return (int)XYAlignResult.ApplyOffsetFail;

        //        // 재이동 및 InPos 확인
        //        if (UseOffsetForXYCorrection)
        //        {
        //            if (_stage.MoveToTeachingPosition(centerTp) != 0)
        //            {
        //                Log.Write("InputStageSequence", "XY_Align", "Fail: Move center after correction");
        //                return (int)XYAlignResult.MoveCenterCommandFail;
        //            }
        //            if (!WaitUntil(() => _stage.InPosTeaching(centerTp), MoveTimeoutMs))
        //            {
        //                Log.Write("InputStageSequence", "XY_Align", "Fail: Correction move timeout");
        //                return (int)XYAlignResult.CorrectionMoveTimeout;
        //            }
        //        }
        //        else
        //        {
        //            if (!WaitUntil(() =>
        //            {
        //                var (x, y, _) = _stage.InputStageConfig.GetPositionWithOffset(centerTp.Name);
        //                return _stage.InPos(_stage.AxisX, x) && _stage.InPos(_stage.AxisY, y);
        //            }, MoveTimeoutMs))
        //            {
        //                Log.Write("InputStageSequence", "XY_Align", "Fail: XY axis in-position timeout (direct)");
        //                return (int)XYAlignResult.CorrectionMoveTimeout;
        //            }
        //        }

        //        Log.Write("InputStageSequence", "XY_Align", "=== XY_Align DONE ===");
        //        return (int)XYAlignResult.Ok;
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.Write(ex);
        //        return (int)XYAlignResult.Exception;
        //    }
        //}

        private bool MoveTAxisDelta(double deltaAngleDeg)
        {
            try
            {
                var axT = _stage.AxisT;
                if (axT == null) return false;
                double current = axT.GetPosition();
                double target = current + deltaAngleDeg;
                int rc = axT.MoveAbs(target, axT.Config.MaxVelocity, axT.Config.RunAcc, axT.Config.RunDec, axT.Config.AccJerkPercent);
                if (rc != 0) return false;
                return true;
            }
            catch (Exception ex)
            {
                Log.Write("InputStageSequence", "T_Align", "MoveTAxisDelta exception: " + ex.Message);
                return false;
            }
        }

        private struct AngleStats
        {
            public int RawCount;
            public double Average;
            public double StdDev;
            public double Representative;
        }

        private AngleStats ComputeAngleStats(List<double> angles, bool excludeExtremes)
        {
            var st = new AngleStats { RawCount = angles?.Count ?? 0 };
            if (angles == null || angles.Count == 0)
                return st;

            var ordered = angles.OrderBy(a => a).ToList();
            IEnumerable<double> work = ordered;

            if (excludeExtremes && ordered.Count >= 3)
                work = ordered.Skip(1).Take(ordered.Count - 2); // 최솟값/최댓값 1개씩 제거

            var wList = work.ToList();
            if (wList.Count == 0)
                return st;

            double avg = wList.Average();
            double var = 0.0;
            if (wList.Count > 1)
                var = wList.Sum(a => (a - avg) * (a - avg)) / (wList.Count - 1);
            double std = Math.Sqrt(var);

            // 대표값: 평균과 가장 가까운 "원본(전체 angles)" 값
            double rep = angles.OrderBy(a => Math.Abs(a - avg)).First();

            st.Average = avg;
            st.StdDev = std;
            st.Representative = rep;
            return st;
        }

    }
}
