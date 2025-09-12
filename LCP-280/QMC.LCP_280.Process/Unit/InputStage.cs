using Newtonsoft.Json;
using QMC.Common;
using QMC.Common.Cameras; // Camera base
using QMC.Common.Cameras.HIKVISION; // HIK camera
using QMC.Common.Component;
using QMC.Common.IOUtil;
using QMC.Common.Motion;
using QMC.Common.Motions;
using QMC.Common.Unit;
using QMC.Common.Vision;              // VisionImage
using QMC.Common.Vision.Cognex;       // Legacy compatibility
using QMC.Common.Vision.Tools;        // Tool base
using QMC.LCP_280.Process;            // PatternMatchingRunner
using QMC.LCP_280.Process.Component;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using static System.Windows.Forms.AxHost;

namespace QMC.LCP_280.Process.Unit
{
    /// <summary>
    /// InputStage Unit
    ///  - Teaching Position + Offset 관리 (InputStageConfig)
    ///  - 축 바인딩 및 Move Helper 제공
    ///  - IO Domain (Clamp / Expander / Vacuum / Ring Check 등) 추상화
    ///  - Vision Pattern Matching Runner 연계 (멀티/센터 마크 검색)
    ///  - DryRun (시뮬레이션) 지원
    ///  - OutputStage 와 구현 양식 통일 (Axis / IO / Domain / High-Level 구분)
    /// </summary>
    public class InputStage : BaseUnit
    {
        private struct AngleStats
        {
            public int RawCount;
            public double Average;
            public double StdDev;
            public double Representative;
        }

        #region Nested Teaching Collection (Enum Indexer)
        public class TeachingPositionCollection : List<TeachingPosition>
        {
            public TeachingPosition this[InputStageConfig.TeachingPositionName name]
            {
                get
                {
                    string key = name.ToString();
                    return this.FirstOrDefault(p => p != null && p.Name.Equals(key, StringComparison.OrdinalIgnoreCase));
                }
            }
        }
        #endregion

        #region Config / Teaching
        public InputStageConfig InputStageConfig { get; private set; }
        public TeachingPositionCollection TeachingPositions { get; private set; } = new TeachingPositionCollection();
        #endregion

        #region Vision Hooks / Camera / Runner
        public HIKGigECamera StageCamera { get; private set; }
        public string StageCameraKey { get; set; } = "In_Stage";

        // Pattern Matching Runner (간소화: Recipe 자동 관리)
        private PatternMatchingRunner _pmRunner;
        private bool _runnerInitTried;

        // Pixel -> mm scale
        public double PixelSizeXmm { get; set; } = 0.01;
        public double PixelSizeYmm { get; set; } = 0.01;
        public bool UseImageCenterAsOrigin { get; set; } = true;
        public double ImageOriginX { get; set; } = double.NaN;
        public double ImageOriginY { get; set; } = double.NaN;
        public string PatternRecipeRootDir { get; set; } = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configs", "PatternMatching");
        public string PatternRecipeName { get; set; } = "Default";
        #endregion

        #region Construction / Initialization
        public InputStage(InputStageConfig config = null)
            : base("InputStageConfig")
        {
            InputStageConfig = config ?? new InputStageConfig();
            AddComponents();

        }

        public override void AddComponents()
        {
            InputStageConfig.LoadAndBindAxes(Equipment.Instance.AxisManager);
            InputStageConfig.InitializeDefaultTeachingPositions();

            TeachingPositions.Clear();
            foreach (var tp in InputStageConfig.TeachingPositions)
                TeachingPositions.Add(tp);

            BindAxes();
            BindIoDomains();
            BindCamera();
        }

        #endregion

        #region Camera Binding
        private void BindCamera()
        {
            var eq = Equipment.Instance; if (eq == null) return;
            if (eq.Cameras != null && eq.Cameras.TryGetValue(StageCameraKey, out var cam))
                StageCamera = cam as HIKGigECamera;
            else
                StageCamera = eq.InStageCam; // fallback
        }
        #endregion

        // ... 클래스 내부 기존 Vision Runner (Pattern Matching) 영역 교체
        #region Vision Runner (Pattern Matching)  // REFACTORED: Hub 사용
        private string CameraKey => StageCameraKey; // 통일된 키 사용

        private (bool ok, List<double> thetaList) MultiSearchViaRunner()
        {
            var ret = VisionRunnerHub.SearchAngles(CameraKey);
            if (!ret.ok) return (false, null);
            return (true, ret.angles);
        }

        /// <summary>
        /// 멀티 패턴 매칭 각도 리스트 반환 (Align 시퀀스용 래퍼)
        /// DryRun 시 모의 데이터 제공
        /// </summary>
        public bool TryGetMultiAngles(out List<double> angles)
        {
            var (ok, list) = MultiSearchViaRunner();
            angles = ok ? list : null;
            return ok && angles != null && angles.Count > 0;
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


        private (bool ok, double x, double y) CenterSearchViaRunner()
        {
            var res = VisionRunnerHub.SearchCenterOffset(
                CameraKey,
                PixelSizeXmm,
                PixelSizeYmm,
                ImageOriginX,
                ImageOriginY,
                UseImageCenterAsOrigin);

            if (!res.ok) return (false, 0, 0);
            return (true, res.dxMm, res.dyMm);
        }
        #endregion

        #region Axis Helpers
        private MotionAxis _axX, _axY, _axT;
        public MotionAxis AxisX => _axX;
        public MotionAxis AxisY => _axY;
        public MotionAxis AxisT => _axT;

        private void BindAxes()
        {
            var mgr = Equipment.Instance?.AxisManager;
            if (mgr == null)
            {
                Log.Write("InputStage", "[BindAxes] AxisManager null");
                return;
            }

            const string unitName = "Unit"; // Equipment에서 축 등록 시 사용한 유닛명과 동일해야 함
            BindAxis(mgr, unitName, AxisNames.WaferStageX, ref _axX);
            BindAxis(mgr, unitName, AxisNames.WaferStageY, ref _axY);
            BindAxis(mgr, unitName, AxisNames.WaferStageT, ref _axT);
        }

        public void TeachCurrentPosition(string positionName, string description = null)
        {
            var axisPositions = new Dictionary<string, double>();
            foreach (var axisPair in Axes)
                axisPositions[axisPair.Key] = axisPair.Value.GetPosition();
            var tp = new TeachingPosition(positionName, axisPositions, description);
            InputStageConfig.SetTeachingPosition(tp);
        }

        public int MoveToTeachingPosition(string positionName, double vel = 0, double acc = 0, double dec = 0, double jerk = 0)
        {
            var tp = InputStageConfig.GetTeachingPosition(positionName);
            if (tp == null) return -1;
            var (x, y, t) = InputStageConfig.GetPositionWithOffset(positionName);   //Offset 포함 위치 - Align 수행 시 data 있음.
            int rc = 0;
            if (AxisX != null) rc |= AxisX.MoveAbs(x, vel > 0 ? vel : AxisX.Config.MaxVelocity, acc > 0 ? acc : AxisX.Config.RunAcc, dec > 0 ? dec : AxisX.Config.RunDec, jerk > 0 ? jerk : AxisX.Config.AccJerkPercent);
            if (AxisY != null) rc |= AxisY.MoveAbs(y, vel > 0 ? vel : AxisY.Config.MaxVelocity, acc > 0 ? acc : AxisY.Config.RunAcc, dec > 0 ? dec : AxisY.Config.RunDec, jerk > 0 ? jerk : AxisY.Config.AccJerkPercent);
            if (AxisT != null) rc |= AxisT.MoveAbs(t, vel > 0 ? vel : AxisT.Config.MaxVelocity, acc > 0 ? acc : AxisT.Config.RunAcc, dec > 0 ? dec : AxisT.Config.RunDec, jerk > 0 ? jerk : AxisT.Config.AccJerkPercent);
            return rc;
        }
        public int MoveToTeachingPosition(TeachingPosition tp, double vel = 0, double acc = 0, double dec = 0, double jerk = 0)
        {
            if (tp == null)
                return -1;
            return MoveToTeachingPosition(tp.Name, vel, acc, dec, jerk);
        }

        public int MoveToTeachingPosition(InputStageConfig.TeachingPositionName name, double vel = 0, double acc = 0, double dec = 0, double jerk = 0)
        {
            return MoveToTeachingPosition(name.ToString(), vel, acc, dec, jerk);
        }

        public bool InPosTeaching(string positionName)
        {
            var (x, y, t) = InputStageConfig.GetPositionWithOffset(positionName);
            return InPos(AxisX, x) && InPos(AxisY, y) && InPos(AxisT, t);
        }
        public bool InPosTeaching(TeachingPosition tp)
        {
            if (tp == null)
                return false;
            return InPosTeaching(tp.Name);
        }
        public bool InPosTeaching(InputStageConfig.TeachingPositionName name)
        {
            return InPosTeaching(name.ToString());
        }

        public int ApplyOffset(string positionName, double dx, double dy, double dt)
        {
            int nRtn = -1;
            // Teaching Position 가져오기
            var tp = InputStageConfig.GetTeachingPosition(positionName);
            if (tp == null)
                return nRtn;

            // 오프셋 적용
            InputStageConfig.SetOffset(positionName, dx, dy, dt);

            // 이동 명령 수행
            int rc = MoveToTeachingPosition(positionName);
            if (rc != 0)
                return nRtn;

            // In-Position 확인 (타임아웃 대기)
            nRtn = (int)WaitUntil(() => InPosTeaching(positionName), MoveTimeoutMs);
            return nRtn;
        }

        public int MoveAxisOnce(MotionAxis ax, double target)
        {
            int nRtn = -1;
            if (ax == null) 
                return nRtn;

            if (Math.Abs(ax.GetPosition() - target) > ax.Config.InposTolerance * 3)
            {
                nRtn = ax.MoveAbs(target, ax.Config.MaxVelocity, ax.Config.RunAcc, ax.Config.RunDec, ax.Config.AccJerkPercent);
            }


            return nRtn;
        }
        public bool InPos(MotionAxis ax, double target) => ax == null || ax.InPosition(target);
        public double GetTP(string tpName, string axisName)
        {
            var tp = InputStageConfig.GetTeachingPosition(tpName);
            if (tp != null && tp.AxisPositions != null && tp.AxisPositions.TryGetValue(axisName, out var v)) return v;
            return 0.0;
        }
        public double GetTP(TeachingPosition tp, string axisName) => (tp == null || string.IsNullOrEmpty(axisName)) ? 0.0 : (tp.AxisPositions.TryGetValue(axisName, out var v) ? v : 0.0);
        public double GetTP(TeachingPosition tp, MotionAxis axis) => axis == null ? 0.0 : GetTP(tp, axis.Name);
        #endregion

        #region Low-Level IO Access (Refactored to match OutputStage pattern)
        public bool ReadInput(string name)
        {
            var hi = InputStageConfig.HardInputs.FirstOrDefault(i => i.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (hi == null) return false;
            var eq = Equipment.Instance; var dio = eq?.DioScan; if (dio == null) return false;
            foreach (var m in eq.UnitIO.Modules)
                if (dio.TryGetInput(m.ModuleName, hi.Disp, out var v)) return v;
            return false;
        }
        public bool WriteOutput(string name, bool on)
        {
            var ho = InputStageConfig.HardOutputs.FirstOrDefault(o => o.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (ho == null) return false;
            var eq = Equipment.Instance; var dio = eq?.DioScan; if (dio == null) return false;
            foreach (var m in eq.UnitIO.Modules)
                if (dio.WriteOutput(m.ModuleName, ho.Disp, on) == 0) return true;
            return false;
        }
        public bool IsOutputOn(string name)
        {
            var ho = InputStageConfig.HardOutputs.FirstOrDefault(o => o.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (ho == null) return false;
            var eq = Equipment.Instance; var dio = eq?.DioScan; if (dio == null) return false;
            foreach (var m in eq.UnitIO.Modules)
                if (dio.TryGetOutput(m.ModuleName, ho.Disp, out var v)) return v;
            return false;
        }
        #endregion

        #region IO Domain Mapping (Reorganized)
        private Cylinder _cylClampLift;      // Lift Up/Down
        private Cylinder _cylClampFB;
        private Cylinder _cylPlate;       // Expander Up/Down
        private Vacuum _vacuum;              // Vacuum + OK sensor

        private void BindIoDomains()
        {
            var eq = Equipment.Instance; var unit = eq?.UnitIO; if (unit == null) return;

            // Vacuum 별칭으로 조회만
            if (!IoAutoBindings.Vacuums.TryGetValue("InStageVac", out _vacuum))
            {
                Log.Write("InputStage", "BindIoDomains", "Vacuums not found: InStageVac");
            }

            // Cylinder는 중앙 별칭으로 조회만
            if (!IoAutoBindings.Cylinders.TryGetValue("InStageExpander", out _cylPlate))
            {
                Log.Write("InputStage", "BindIoDomains", "Cylinder not found: InStageExpander");
            }

            if (!IoAutoBindings.Cylinders.TryGetValue("InStageClampLift", out _cylClampLift))
            {
                Log.Write("InputStage", "BindIoDomains", "Cylinder not found: InStageClampLift");
            }
                
            if (!IoAutoBindings.Cylinders.TryGetValue("InStageClampFB", out _cylClampFB))
            {
                Log.Write("InputStage", "BindIoDomains", "Cylinder not found: InStageClampFB");
            }
        }

        // === Domain Control (표준 구동) ===
        public bool SetVacuum(bool on)
        {
            if (_vacuum == null) return false;
            if (on) _vacuum.On();
            else _vacuum.Off();
            return true;
        }

        public bool SetClampPlate(bool bUpDn)
        {
            if (_cylPlate == null) return false;
            if (bUpDn) return _cylPlate.Extend();
            else return _cylPlate.Retract();
        }

        public bool SetClampLift(bool bUpDn)
        {
            if (_cylClampLift == null) return false;
            if (bUpDn) return _cylClampLift.Extend();
            else
            {
                if (!IsClampBwd()) return false; // 기존 인터락 유지
                return _cylClampLift.Retract();
            }
        }

        public bool SetClampFB(bool bFwdBwd)
        {
            if (_cylClampFB == null) return false;
            if (bFwdBwd)
            {
                if (!IsClampLiftUp()) return false; // 기존 인터락 유지
                return _cylClampFB.Extend();
            }
            else return _cylClampFB.Retract();
        }

        #region High-Level Actuator API (Interlock 포함)
        public bool IsClampLiftUp() => !IsClampLiftDown();
        public bool IsClampLiftDown() => ReadInput(InputStageConfig.IO.CLAMP_DOWN_SNS);
        public bool IsClampFwd() => ReadInput(InputStageConfig.IO.CLAMP_FWD_SNS);
        public bool IsClampBwd() => !IsClampFwd();
        public bool Ring0() => ReadInput(InputStageConfig.IO.RING_CHECK0);
        public bool Ring1() => ReadInput(InputStageConfig.IO.RING_CHECK1);
        public bool IsRingPresent() => Ring0() || Ring1();
        #endregion

        // === Direct Valve Control (강제 구동) ===
        public bool IsVacuumValveOn()               => IsOutputOn(InputStageConfig.IO.VAC_OUT);
        public void SetClampLiftUpValve(bool on)    => WriteOutput(InputStageConfig.IO.CLAMP_UP_OUT, on);
        public bool IsClampLiftUpValveOn()          => IsOutputOn(InputStageConfig.IO.CLAMP_UP_OUT);
        public bool IsVacuum() => ReadInput(InputStageConfig.IO.VAC_OK_SNS);
        public bool IsPlateUp() => ReadInput(InputStageConfig.IO.EXPANDER_UP_SNS);
        public bool IsPlateDown() => ReadInput(InputStageConfig.IO.EXPANDER_DOWN_SNS);
        public override void OnRun() => base.OnRun();
        public override void OnStop() => base.OnStop();
        #endregion


        // 파라미터로 빼야하는 Data 및 상수
        public int MoveTimeoutMs { get; set; } = 6000;
        public int PollIntervalMs { get; set; } = 30;
        public double AngleIgnoreThresholdDeg { get; set; } = 0.001;
        public double AngleMaxApplyDeg { get; set; } = 2.0;
        public double AngleApplyGain { get; set; } = 1.0; // 방향 반전 필요 시 -1 사용
        public bool UseOffsetForTAxisCorrection { get; set; } = true; // false면 직접 축 이동 방식으로 전환 가능 (추후 확장)

        private int WaitUntil(Func<bool> cond, int timeoutMs)
        {
            int nRet = -1;
            var sw = Stopwatch.StartNew();
            while (sw.ElapsedMilliseconds < timeoutMs)
            {
                if (cond()) return nRet;
                Thread.Sleep(PollIntervalMs);
            }

            nRet = 0;
            return nRet;
        }
        private int WaitUntilInPos(TeachingPosition tp, int timeoutMs)
        {
            var sw = Stopwatch.StartNew();
            while (sw.ElapsedMilliseconds < timeoutMs)
            {
                if (InPosTeaching(tp))
                    return 0;
                Thread.Sleep(PollIntervalMs);
            }
            return -1;
        }


        #region Seq 단위 동작 함수
        public int WaferLoading()
        {
            int nRet = -1;
            /* TODO */
            return nRet;
        }

        public double MaxXYOffsetMm { get; set; } = 2.0;   // XY 최대 보정 허용치 (mm)
        /// <summary>
        /// 공통: Center Teaching 이동, Grab 이미지까지 수행
        /// </summary>
        private int PrepareForAlign(out TeachingPosition centerTp, out VisionImage img)
        {
            centerTp = null;
            img = null;

            // === 내부 인터락 확인 ===
            if (!IsRingPresent())
            {
                Log.Write(UnitName, "Align", "Fail: Ring(Wafer) not present");
                return -1;
            }
            if (!IsClampLiftUp())
            {
                Log.Write(UnitName, "Align", "Fail: Clamp Lift not Up");
                return -1;
            }
            if (!IsClampFwd())
            {
                Log.Write(UnitName, "Align", "Fail: Clamp not FWD");
                return -1;
            }

            // === Center Teaching Position 이동 ===
            centerTp = TeachingPositions[InputStageConfig.TeachingPositionName.CenterPoint];
            if (centerTp == null)
            {
                Log.Write(UnitName, "Align", "Fail: CenterPoint teaching not defined");
                return -1;
            }
            if (MoveToTeachingPosition(centerTp) != 0)
            {
                Log.Write(UnitName, "Align", "Fail: Move center command");
                return -1;
            }
            if (WaitUntilInPos(centerTp, MoveTimeoutMs) != 0)
            {
                Log.Write(UnitName, "Align", "Fail: Move center timeout");
                return -1;
            }

            // === 카메라 그랩 ===
            if (StageCamera == null)
            {
                Log.Write(UnitName, "Align", "Fail: Camera null");
                return -1;
            }
            var grabRc = StageCamera.GrabSync(out img);
            if (grabRc != 0 || img == null || img.RawData == null)
            {
                Log.Write(UnitName, "Align", $"Fail: Grab fail rc={grabRc}");
                return -1;
            }
            StageCamera.LatestImage = img;
            Log.Write(UnitName, "Align", "Grab OK");

            return 0;
        }

        /// <summary>
        /// T축 정렬
        /// </summary>
        public int T_Align()
        {
            int nRet = -1;
            try
            {
                Log.Write(UnitName, "T_Align", "Start");

                // 공통 준비
                if (PrepareForAlign(out var centerTp, out var img) != 0)
                    return nRet;

                // Vision Angle 검색
                if (!TryGetMultiAngles(out var angleList) || angleList == null || angleList.Count == 0)
                {
                    Log.Write(UnitName, "T_Align", "Fail: Vision angle search fail or empty");
                    return nRet;
                }

                var stats = ComputeAngleStats(angleList, excludeExtremes: true);
                if (stats.RawCount == 0)
                {
                    Log.Write(UnitName, "T_Align", "Fail: No angle list after filtering");
                    return nRet;
                }

                double rawAngle = stats.Representative;
                Log.Write(UnitName, "T_Align",
                    $"Angle Representative={rawAngle:F6} avg={stats.Average:F6} std={stats.StdDev:F6} rawCount={stats.RawCount}");

                // 유효성 체크
                if (Math.Abs(rawAngle) < AngleIgnoreThresholdDeg)
                {
                    Log.Write(UnitName, "T_Align", "Angle below ignore threshold → skip correction");
                    return nRet;
                }
                if (Math.Abs(rawAngle) > AngleMaxApplyDeg)
                {
                    Log.Write(UnitName, "T_Align",
                        $"Fail: Angle {rawAngle:F4} over max limit {AngleMaxApplyDeg}");
                    return nRet;
                }

                double applyAngle = rawAngle * AngleApplyGain;

                // 보정 적용
                int correctionOk = UseOffsetForTAxisCorrection
                    ? ApplyOffset(centerTp.Name, 0.0, 0.0, applyAngle)
                    : MoveAxisOnce(AxisT, applyAngle);

                Log.Write(UnitName, "T_Align",
                    $"{(UseOffsetForTAxisCorrection ? "ApplyOffset" : "DirectMove")}(T) angle={applyAngle:F6} -> {(correctionOk == 0 ? "OK" : "FAIL")}");

                if (correctionOk != 0)
                    return nRet;

                // 보정 후 재이동
                if (MoveToTeachingPosition(centerTp) != 0)
                    return nRet;
                if (WaitUntil(() => InPosTeaching(centerTp), MoveTimeoutMs) != 0)
                    return nRet;
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                return nRet;
            }
            nRet = 0;
            return nRet;
        }

        /// <summary>
        /// XY 정렬
        /// </summary>
        public int XY_Align()
        {
            int nRet = -1;
            try
            {
                Log.Write(UnitName, "XY_Align", "Start");

                // 공통 준비
                if (PrepareForAlign(out var centerTp, out var img) != 0)
                    return nRet;

                // Vision XY Offset 검색
                var res = CenterSearchViaRunner();
                if (!res.ok)
                {
                    Log.Write(UnitName, "XY_Align", "Fail: Vision XY offset search fail");
                    return nRet;
                }

                double dx = res.x;
                double dy = res.y;

                Log.Write(UnitName, "XY_Align",
                    $"XY Offset dx={dx:F6} dy={dy:F6}");

                // 유효성 체크
                if (Math.Abs(dx) < 0.0001 && Math.Abs(dy) < 0.0001)
                {
                    Log.Write(UnitName, "XY_Align", "Offset below threshold → skip correction");
                    return nRet;
                }
                if (Math.Abs(dx) > MaxXYOffsetMm || Math.Abs(dy) > MaxXYOffsetMm)
                {
                    Log.Write(UnitName, "XY_Align",
                        $"Fail: Offset over limit dx={dx:F4}, dy={dy:F4}, limit={MaxXYOffsetMm}");
                    return nRet;
                }

                // 보정 적용
                int correctionOk = ApplyOffset(centerTp.Name, dx, dy, 0.0);
                Log.Write(UnitName, "XY_Align",
                    $"ApplyOffset(XY) dx={dx:F6}, dy={dy:F6} -> {(correctionOk == 0 ? "OK" : "FAIL")}");

                if (correctionOk != 0)
                    return nRet;

                // 보정 후 재이동
                if (MoveToTeachingPosition(centerTp) != 0)
                    return nRet;
                if (WaitUntil(() => InPosTeaching(centerTp), MoveTimeoutMs) != 0)
                    return nRet;
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                return nRet;
            }
            nRet = 0;
            return nRet;
        }


        public int ChipPickUp()
        {
            int nRet = -1;
            /* TODO */
            return nRet;
        }

        public int WaferUnloading()
        {
            int nRet = -1;
            /* TODO */
            return nRet;
        }
        #endregion
    }
}