using Newtonsoft.Json;
using QMC.Common;
using QMC.Common.Component;
using QMC.Common.IOUtil;
using QMC.Common.Motion;
using QMC.Common.Motions;
using QMC.Common.Unit;
using QMC.LCP_280.Process.Component;
using QMC.Common.Cameras; // Camera base
using QMC.Common.Cameras.HIKVISION; // HIK camera
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using QMC.Common.Vision;              // VisionImage
using QMC.Common.Vision.Tools;        // Tool base
using QMC.Common.Vision.Cognex;       // Legacy compatibility
using QMC.LCP_280.Process;            // PatternMatchingRunner
using QMC.Common.IOUtil;              // UnitIoHelper

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

        #region DryRun Simulation Fields
        public bool DryRun { get; private set; }
        public void SetDryRun(bool on) => DryRun = on;
        private bool _simClamp;
        private bool _simClampDown;
        private bool _simVac;
        private bool _simRingPresent;
        private bool _simExpUp;
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
            if (DryRun)
                return (true, new List<double> { 0.0, 0.01, -0.005, 0.004, -0.003 });

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

        private (bool ok, double x, double y) CenterSearchViaRunner()
        {
            if (DryRun) return (true, 0.0, 0.0);

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
                Log.Write("UnitAxis", "[BindAxes] AxisManager null");
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
            var (x, y, t) = InputStageConfig.GetPositionWithOffset(positionName);
            int rc = 0;
            if (AxisX != null) rc |= AxisX.MoveAbs(x, vel > 0 ? vel : AxisX.Config.MaxVelocity, acc > 0 ? acc : AxisX.Config.RunAcc, dec > 0 ? dec : AxisX.Config.RunDec, jerk > 0 ? jerk : AxisX.Config.AccJerkPercent);
            if (AxisY != null) rc |= AxisY.MoveAbs(y, vel > 0 ? vel : AxisY.Config.MaxVelocity, acc > 0 ? acc : AxisY.Config.RunAcc, dec > 0 ? dec : AxisY.Config.RunDec, jerk > 0 ? jerk : AxisY.Config.AccJerkPercent);
            if (AxisT != null) rc |= AxisT.MoveAbs(t, vel > 0 ? vel : AxisT.Config.MaxVelocity, acc > 0 ? acc : AxisT.Config.RunAcc, dec > 0 ? dec : AxisT.Config.RunDec, jerk > 0 ? jerk : AxisT.Config.AccJerkPercent);
            return rc;
        }
        public int MoveToTeachingPosition(TeachingPosition tp, double vel = 0, double acc = 0, double dec = 0, double jerk = 0) => tp == null ? -1 : MoveToTeachingPosition(tp.Name, vel, acc, dec, jerk);
        public int MoveToTeachingPosition(InputStageConfig.TeachingPositionName name, double vel = 0, double acc = 0, double dec = 0, double jerk = 0) => MoveToTeachingPosition(name.ToString(), vel, acc, dec, jerk);

        public bool InPosTeaching(string positionName)
        {
            var (x, y, t) = InputStageConfig.GetPositionWithOffset(positionName);
            return InPos(AxisX, x) && InPos(AxisY, y) && InPos(AxisT, t);
        }
        public bool InPosTeaching(TeachingPosition tp) => tp != null && InPosTeaching(tp.Name);
        public bool InPosTeaching(InputStageConfig.TeachingPositionName name) => InPosTeaching(name.ToString());

        public void ApplyOffset(string positionName, double dx, double dy, double dt) => InputStageConfig.SetOffset(positionName, dx, dy, dt);

        public void MoveAxisOnce(MotionAxis ax, double target)
        {
            if (ax == null) return;
            if (Math.Abs(ax.GetPosition() - target) > ax.Config.InposTolerance * 3)
                ax.MoveAbs(target, ax.Config.MaxVelocity, ax.Config.RunAcc, ax.Config.RunDec, ax.Config.AccJerkPercent);
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
            if (DryRun)
            {
                switch (name)
                {
                    case InputStageConfig.IO.CLAMP_FWD_SNS: return _simClamp; // Up/Clamp
                    case InputStageConfig.IO.CLAMP_DOWN_SNS: return _simClampDown; // Down
                    case InputStageConfig.IO.VAC_OK_SNS: return _simVac;
                    case InputStageConfig.IO.RING_CHECK0:
                    case InputStageConfig.IO.RING_CHECK1: return _simRingPresent;
                    case InputStageConfig.IO.EXPANDER_UP_SNS: return _simExpUp;
                    case InputStageConfig.IO.EXPANDER_DOWN_SNS: return !_simExpUp;
                }
                return false;
            }
            var hi = InputStageConfig.HardInputs.FirstOrDefault(i => i.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (hi == null) return false;
            var eq = Equipment.Instance; var dio = eq?.DioScan; if (dio == null) return false;
            foreach (var m in eq.UnitIO.Modules)
                if (dio.TryGetInput(m.ModuleName, hi.Disp, out var v)) return v;
            return false;
        }
        public bool WriteOutput(string name, bool on)
        {
            if (DryRun)
            {
                switch (name)
                {
                    case InputStageConfig.IO.CLAMP_FWD_OUT: _simClamp = on; if (on) _simClampDown = false; break;
                    case InputStageConfig.IO.CLAMP_BWD_OUT: if (on) { _simClamp = false; _simClampDown = true; } break;
                    case InputStageConfig.IO.VAC_OUT: _simVac = on; break;
                    case InputStageConfig.IO.EXPANDER_UP_OUT: if (on) _simExpUp = true; break;
                    case InputStageConfig.IO.EXPANDER_DOWN_OUT: if (on) _simExpUp = false; break;
                    case InputStageConfig.IO.CLAMP_UP_OUT: /* lift up */ break;
                    case InputStageConfig.IO.CLAMP_DOWN_OUT: /* lift down */ break;
                }
                return true;
            }
            var ho = InputStageConfig.HardOutputs.FirstOrDefault(o => o.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (ho == null) return false;
            var eq = Equipment.Instance; var dio = eq?.DioScan; if (dio == null) return false;
            foreach (var m in eq.UnitIO.Modules)
                if (dio.WriteOutput(m.ModuleName, ho.Disp, on) == 0) return true;
            return false;
        }
        public bool IsOutputOn(string name)
        {
            if (DryRun)
            {
                switch (name)
                {
                    case InputStageConfig.IO.CLAMP_FWD_OUT: return _simClamp;
                    case InputStageConfig.IO.CLAMP_BWD_OUT: return _simClampDown && !_simClamp;
                    case InputStageConfig.IO.VAC_OUT: return _simVac;
                    case InputStageConfig.IO.EXPANDER_UP_OUT: return _simExpUp;
                    case InputStageConfig.IO.EXPANDER_DOWN_OUT: return !_simExpUp;
                }
                return false;
            }
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

            // Vacuum
            DIO.MapByName(unit, "InStage.VacOut", true, InputStageConfig.IO.VAC_OUT);
            DIO.MapByName(unit, "InStage.VacOk", false, InputStageConfig.IO.VAC_OK_SNS);
            _vacuum = new Vacuum("InStageVac", "InStage.VacOut", "InStage.VacOk");

            // Plate (Up/Down)
            DIO.MapByName(unit, "InStage.ExpUpOut", true, InputStageConfig.IO.EXPANDER_UP_OUT);
            DIO.MapByName(unit, "InStage.ExpDownOut", true, InputStageConfig.IO.EXPANDER_DOWN_OUT);
            DIO.MapByName(unit, "InStage.ExpUpIn", false, InputStageConfig.IO.EXPANDER_UP_SNS);
            DIO.MapByName(unit, "InStage.ExpDownIn", false, InputStageConfig.IO.EXPANDER_DOWN_SNS);
            _cylPlate = new Cylinder("InStageExpander", "InStage.ExpUpOut", "InStage.ExpDownOut", "InStage.ExpUpIn", "InStage.ExpDownIn");

            // Clamp Lift (Up/Down) -> sensors: Up sensor 없음 (Clamp Up 센서 공용 사용), Down 센서 존재
            DIO.MapByName(unit, "InStage.ClampUpOut",   true,  InputStageConfig.IO.CLAMP_UP_OUT);
            DIO.MapByName(unit, "InStage.ClampDownOut", true,  InputStageConfig.IO.CLAMP_DOWN_OUT);
            DIO.MapByName(unit, "InStage.ClampDownIn",  false, InputStageConfig.IO.CLAMP_DOWN_SNS);
            _cylClampLift = new Cylinder(
                "InStageClampLift", 
                "InStage.ClampUpOut", 
                "InStage.ClampDownOut",
                "InStage.ClampUpIn/*NO_SENSOR*/",
                "InStage.ClampDownIn");

            // Clamp FWD/BWD (direct)
            DIO.MapByName(unit, "InStage.ClampFwdOut", true,  InputStageConfig.IO.CLAMP_FWD_OUT);
            DIO.MapByName(unit, "InStage.ClampBwdOut", true,  InputStageConfig.IO.CLAMP_BWD_OUT);
            DIO.MapByName(unit, "InStage.ClampFwdIn", false, InputStageConfig.IO.CLAMP_FWD_SNS);
            _cylClampFB = new Cylinder(
                "InStageClampFB",
                "InStage.ClampFwdOut",
                "InStage.ClampBwdOut",
                "InStage.ClampFwdIn",
                "InStage.ClampBwdIn/*NO_SENSOR*/");
        }

        // === Direct Valve Control (강제 구동) ===
        public void SetVacuumValve(bool on)         => WriteOutput(InputStageConfig.IO.VAC_OUT, on);
        public bool IsVacuumValveOn()               => IsOutputOn(InputStageConfig.IO.VAC_OUT);

        public void SetPlateUp(bool on) => WriteOutput(InputStageConfig.IO.EXPANDER_UP_OUT, on);
        public bool IsPlateUpOn() => IsOutputOn(InputStageConfig.IO.EXPANDER_UP_OUT);
        public void SetPlateDown(bool on) => WriteOutput(InputStageConfig.IO.EXPANDER_DOWN_OUT, on);
        public bool IsPlateDownOn() => IsOutputOn(InputStageConfig.IO.EXPANDER_DOWN_OUT);


        public void SetClampLiftUpValve(bool on)    => WriteOutput(InputStageConfig.IO.CLAMP_UP_OUT, on);
        public bool IsClampLiftUpValveOn()          => IsOutputOn(InputStageConfig.IO.CLAMP_UP_OUT);
        public void SetClampLiftDownValve(bool on)  => WriteOutput(InputStageConfig.IO.CLAMP_DOWN_OUT, on);
        public bool IsClampLiftDownValveOn()        => IsOutputOn(InputStageConfig.IO.CLAMP_DOWN_OUT);

        public void SetClampFwdValve(bool on)       => WriteOutput(InputStageConfig.IO.CLAMP_FWD_OUT, on);
        public bool IsClampFwdValveOn()             => IsOutputOn(InputStageConfig.IO.CLAMP_FWD_OUT);
        public void SetClampBwdValve(bool on)       => WriteOutput(InputStageConfig.IO.CLAMP_BWD_OUT, on);
        public bool IsClampBwdValveOn()             => IsOutputOn(InputStageConfig.IO.CLAMP_BWD_OUT);

        // Backward compatibility aliases (legacy naming used by existing forms / controls)
        public void VacuumOn() { if (DryRun) { _simVac = true; return; } _vacuum?.On(); }
        public void VacuumOff() { if (DryRun) { _simVac = false; return; } _vacuum?.Off(); }
        public bool VacuumOnWait(int timeoutMs = 1500) { if (DryRun) { _simVac = true; return true; } return _vacuum?.OnWaitOk(timeoutMs) ?? false; }

        public bool IsVacuum() => DryRun ? _simVac : (_vacuum?.IsOk() ?? ReadInput(InputStageConfig.IO.VAC_OK_SNS));
        public bool VacuumCheck() => IsVacuum(); // legacy

        public bool PlateUp(int timeoutMs = 3000) { if (DryRun) { _simExpUp = true; return true; } return _cylPlate?.Extend(timeoutMs) ?? false; }
        public bool PlateDown(int timeoutMs = 3000) { if (DryRun) { _simExpUp = false; return true; } return _cylPlate?.Retract(timeoutMs) ?? false; }
        public bool IsPlateUp() => ReadInput(InputStageConfig.IO.EXPANDER_UP_SNS);
        public bool IsPlateDown() => ReadInput(InputStageConfig.IO.EXPANDER_DOWN_SNS);

        #region High-Level Actuator API (Interlock 포함)
        public bool ClampLiftUp(int timeoutMs = 3000) => _cylClampLift?.Extend(timeoutMs) ?? false;
        public bool ClampLiftDown(int timeoutMs = 3000)
        {
            if (!IsClampBwd()) return false; // 기존 인터락 유지
            return _cylClampLift?.Retract(timeoutMs) ?? false;
        }

        public bool IsClampLiftUp() => !IsClampLiftDown();
        public bool IsClampLiftDown() => ReadInput(InputStageConfig.IO.CLAMP_DOWN_SNS);

        public bool ClampFwd(int timeoutMs = 3000)
        {
            if (!IsClampLiftUp()) return false; // 기존 인터락 유지
            return _cylClampFB?.Extend(timeoutMs) ?? false;
        }
        public bool ClampBwd(int timeoutMs = 3000) => _cylClampFB?.Retract(timeoutMs) ?? false;
        public bool IsClampFwd() => ReadInput(InputStageConfig.IO.CLAMP_FWD_SNS);
        public bool IsClampBwd() => !IsClampFwd();

        public bool Ring0()           => ReadInput(InputStageConfig.IO.RING_CHECK0);
        public bool Ring1()           => ReadInput(InputStageConfig.IO.RING_CHECK1);
        public bool IsRingPresent()   => Ring0() || Ring1();


        public void SetClampUpDown(bool up) 
        {
            if (up)
            {
                _cylClampLift.Extend(); //ClampLiftUp(); 
            }
            else
            {
                _cylClampLift.Retract(); //ClampLiftDown(); 
            }
        }


        #endregion

        public override void OnRun() => base.OnRun();
        public override void OnStop() => base.OnStop();
        #endregion




    }
}