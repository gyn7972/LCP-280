using System; // added for Obsolete attribute
using QMC.Common;
using QMC.Common.Cameras.HIKVISION;
using QMC.Common.Component;
using QMC.Common.IOUtil;
using QMC.Common.Motion;
using QMC.Common.Motions;
using QMC.Common.Unit;
using QMC.LCP_280.Process.Component;
using System.Collections.Generic;
using System.Linq;

namespace QMC.LCP_280.Process.Unit
{
    public class OutputStage : BaseUnit
    {
        // Wrapper collection to allow enum index access
        public class TeachingPositionCollection : List<TeachingPosition>
        {
            public TeachingPosition this[OutputStageConfig.TeachingPositionName name]
            {
                get
                {
                    string key = name.ToString();
                    return this.FirstOrDefault(p => p != null && p.Name.Equals(key, System.StringComparison.OrdinalIgnoreCase));
                }
            }
        }

        public OutputStageConfig OutputStageConfig { get; private set; }
        public TeachingPositionCollection TeachingPositions { get; private set; } = new TeachingPositionCollection();

        // Stage camera
        public HIKGigECamera StageCamera { get; private set; }
        public string StageCameraKey { get; set; } = "Out_Stage";

        public OutputStage(OutputStageConfig config = null)
            : base("OutputStageConfig")
        {
            OutputStageConfig = config ?? new OutputStageConfig();
            AddComponents();
        }

        public override void AddComponents()
        {
            OutputStageConfig.LoadAndBindAxes(Equipment.Instance.AxisManager);
            OutputStageConfig.InitializeDefaultTeachingPositions();
            TeachingPositions.Clear();
            foreach (var tp in OutputStageConfig.TeachingPositions)
                TeachingPositions.Add(tp);
            BindAxes();
            BindIoDomains();
            BindCamera();
        }

        private void BindCamera()
        {
            var eq = Equipment.Instance;
            if (eq == null) return;
            if (eq.Cameras != null && eq.Cameras.TryGetValue(StageCameraKey, out var cam))
                StageCamera = cam as HIKGigECamera;
            else
                StageCamera = eq.OutStageCam; // fallback
        }

        public override void OnRun() => base.OnRun();
        public override void OnStop() { base.OnStop(); }

        public void TeachCurrentPosition(string positionName, string description = null)
        {
            var axisPositions = new Dictionary<string, double>();
            foreach (var axisPair in Axes)
                axisPositions[axisPair.Key] = axisPair.Value.GetPosition();
            var tp = new TeachingPosition(positionName, axisPositions, description);
            OutputStageConfig.SetTeachingPosition(tp);
        }

        public int MoveToTeachingPosition(string positionName, double vel = 5, double acc = 10, double dec = 10, double jerk = 50)
        {
            var tp = OutputStageConfig.GetTeachingPosition(positionName);
            if (tp == null) return -1;
            int result = 0;
            foreach (var axisKey in tp.AxisPositions.Keys)
            {
                if (Axes.TryGetValue(axisKey, out var axis))
                {
                    double pos = tp.AxisPositions[axisKey];
                    int r = axis.MoveAbs(pos, vel, acc, dec, jerk);
                    if (r != 0) result = r;
                }
            }
            return result;
        }

        #region Axis Helpers
        private MotionAxis _axX, _axY, _axT;
        public MotionAxis AxisX => _axX;
        public MotionAxis AxisY => _axY;
        public MotionAxis AxisT => _axT;
        private void BindAxes()
        {
            Axes.TryGetValue("Bin Stage X Axis", out _axX);
            Axes.TryGetValue("Bin Stage Y Axis", out _axY);
            Axes.TryGetValue("Bin Stage T Axis", out _axT);
        }
        public double GetTP(string tpName, string axisName)
        {
            var tp = OutputStageConfig.GetTeachingPosition(tpName);
            if (tp != null && tp.AxisPositions != null && tp.AxisPositions.TryGetValue(axisName, out var v)) return v;
            return 0.0;
        }
        public void MoveAxisOnce(MotionAxis ax, double target)
        {
            if (ax == null) return;
            if (System.Math.Abs(ax.GetPosition() - target) > ax.Config.InposTolerance * 3)
                ax.MoveAbs(target, ax.Config.MaxVelocity, ax.Config.RunAcc, ax.Config.RunDec, ax.Config.AccJerkPercent);
        }
        public bool InPos(MotionAxis ax, double target) => ax == null || ax.InPosition(target);
        #endregion

        #region IO Low-Level
        public bool ReadInput(string name)
        {
            var hi = OutputStageConfig.HardInputs.FirstOrDefault(i => i.Name.Equals(name, System.StringComparison.OrdinalIgnoreCase));
            if (hi == null) return false;
            var eq = Equipment.Instance; var dio = eq?.DioScan; if (dio == null) return false;
            foreach (var m in eq.UnitIO.Modules)
                if (dio.TryGetInput(m.ModuleName, hi.Disp, out var v)) return v;
            return false;
        }
        public bool WriteOutput(string name, bool on)
        {
            var ho = OutputStageConfig.HardOutputs.FirstOrDefault(o => o.Name.Equals(name, System.StringComparison.OrdinalIgnoreCase));
            if (ho == null) return false;
            var eq = Equipment.Instance; var dio = eq?.DioScan; if (dio == null) return false;
            foreach (var m in eq.UnitIO.Modules)
                if (dio.WriteOutput(m.ModuleName, ho.Disp, on) == 0) return true;
            return false;
        }
        public bool IsOutputOn(string name)
        {
            var ho = OutputStageConfig.HardOutputs.FirstOrDefault(o => o.Name.Equals(name, System.StringComparison.OrdinalIgnoreCase));
            if (ho == null) return false;
            var eq = Equipment.Instance; var dio = eq?.DioScan; if (dio == null) return false;
            foreach (var m in eq.UnitIO.Modules)
                if (dio.TryGetOutput(m.ModuleName, ho.Disp, out var v)) return v;
            return false;
        }
        #endregion

        #region IO Domain Mapping (Reorganized)
        private Cylinder _cylClampLift;
        private Cylinder _cylClampFB;
        private Cylinder _cylPlate;
        private Vacuum _vacuum;

        private void BindIoDomains()
        {
            var eq = Equipment.Instance; var unit = eq?.UnitIO; if (unit == null) return;

            // Vacuum
            DIO.MapByName(unit, "OutStage.VacOut", true, OutputStageConfig.IO.VACUUM);
            DIO.MapByName(unit, "OutStage.VacOk", false, OutputStageConfig.IO.VACUUM_CHECK);
            _vacuum = new Vacuum("OutStageVac", "OutStage.VacOut", "OutStage.VacOk");

            // Plate
            DIO.MapByName(unit, "OutStage.PlateUpOut", true, OutputStageConfig.IO.PLATE_UP_OUT);
            DIO.MapByName(unit, "OutStage.PlateDownOut", true, OutputStageConfig.IO.PLATE_DOWN_OUT);
            DIO.MapByName(unit, "OutStage.PlateUpIn", false, OutputStageConfig.IO.PLATE_UP);
            DIO.MapByName(unit, "OutStage.PlateDownIn", false, OutputStageConfig.IO.PLATE_DOWN);
            _cylPlate = new Cylinder("OutStagePlate", "OutStage.PlateUpOut", "OutStage.PlateDownOut", "OutStage.PlateUpIn", "OutStage.PlateDownIn");

            // Lift
            DIO.MapByName(unit, "OutStage.LiftUpOut", true, OutputStageConfig.IO.CLAMP_UP);
            DIO.MapByName(unit, "OutStage.LiftDownOut", true, OutputStageConfig.IO.CLAMP_DOWN);
            DIO.MapByName(unit, "OutStage.LiftDownIn", false, OutputStageConfig.IO.CLAMP_DOWN_CHECK);
            _cylClampLift = new Cylinder(
                "OutStageLift",
                "OutStage.LiftUpOut",
                "OutStage.LiftDownOut",
                "OutStage.LiftUpIn/*NO_SENSOR*/",
                "OutStage.LiftDownIn");

            // Clamp FWD/BWD
            DIO.MapByName(unit, "OutStage.ClampFwdOut", true, OutputStageConfig.IO.CLAMP_FWD);
            DIO.MapByName(unit, "OutStage.ClampBwdOut", true, OutputStageConfig.IO.CLAMP_BWD);
            DIO.MapByName(unit, "OutStage.ClampFwdIn", false, OutputStageConfig.IO.CLAMP_FWD_CHECK);
            _cylClampFB = new Cylinder(
                "OutStageClampFB",
                "OutStage.ClampFwdOut",
                "OutStage.ClampBwdOut",
                "OutStage.ClampFwdIn",
                "OutStage.ClampBwdIn/*NO_SENSOR*/");
        }

        // === Direct Valve Control (입력 신호/인터락 무관 강제 구동용) ===
        public void SetVacuumValve(bool on) => WriteOutput(OutputStageConfig.IO.VACUUM, on);
        public bool IsVacuumValveOn() => IsOutputOn(OutputStageConfig.IO.VACUUM);

        public void SetPlateUpValve(bool on) => WriteOutput(OutputStageConfig.IO.PLATE_UP_OUT, on);
        public bool IsPlateUpValveOn() => IsOutputOn(OutputStageConfig.IO.PLATE_UP_OUT);
        public void SetPlateDownValve(bool on) => WriteOutput(OutputStageConfig.IO.PLATE_DOWN_OUT, on);
        public bool IsPlateDownValveOn() => IsOutputOn(OutputStageConfig.IO.PLATE_DOWN_OUT);

        public void SetClampLiftUpValve(bool on) => WriteOutput(OutputStageConfig.IO.CLAMP_UP, on);
        public bool IsClampLiftUpValveOn() => IsOutputOn(OutputStageConfig.IO.CLAMP_UP);
        public void SetClampLiftDownValve(bool on) => WriteOutput(OutputStageConfig.IO.CLAMP_DOWN, on);
        public bool IsClampLiftDownValveOn() => IsOutputOn(OutputStageConfig.IO.CLAMP_DOWN);

        public void SetClampFwdValve(bool on) => WriteOutput(OutputStageConfig.IO.CLAMP_FWD, on);
        public bool IsClampFwdValveOn() => IsOutputOn(OutputStageConfig.IO.CLAMP_FWD);
        public void SetClampBwdValve(bool on) => WriteOutput(OutputStageConfig.IO.CLAMP_BWD, on);
        public bool IsClampBwdValveOn() => IsOutputOn(OutputStageConfig.IO.CLAMP_BWD);

        // --- Existing High-Level APIs (인터락 포함) ---
        public void VacuumOn() => _vacuum?.On();
        public void VacuumOff() => _vacuum?.Off();
        public bool IsVacuum() => (_vacuum?.IsOk() ?? false) || ReadInput(OutputStageConfig.IO.VACUUM_CHECK);
        [Obsolete("Use IsVacuum() instead")] public bool VacuumCheck() => IsVacuum();
        public bool VacuumOk() => _vacuum?.IsOk() ?? false;

        public bool PlateUp(int timeoutMs = 3000) => _cylPlate?.Extend(timeoutMs) ?? false;
        public bool PlateDown(int timeoutMs = 3000) => _cylPlate?.Retract(timeoutMs) ?? false;
        public bool IsPlateUp() => ReadInput(OutputStageConfig.IO.PLATE_UP);
        public bool IsPlateDown() => ReadInput(OutputStageConfig.IO.PLATE_DOWN);

        public bool ClampLiftUp(int timeoutMs = 3000) => _cylClampLift?.Extend(timeoutMs) ?? false;
        public bool ClampLiftDown(int timeoutMs = 3000)
        {
            if (!IsClampBwd()) return false; // 기존 인터락 유지
            return _cylClampLift?.Retract(timeoutMs) ?? false;
        }
        public bool IsClampLiftUp() => !IsClampLiftDown();
        public bool IsClampLiftDown() => ReadInput(OutputStageConfig.IO.CLAMP_DOWN_CHECK);

        public bool ClampFwd(int timeoutMs = 3000)
        {
            if (!IsClampLiftUp()) return false; // 기존 인터락 유지
            return _cylClampFB?.Extend(timeoutMs) ?? false;
        }
        public bool ClampBwd(int timeoutMs = 3000) => _cylClampFB?.Retract(timeoutMs) ?? false;
        public bool IsClampFwd() => ReadInput(OutputStageConfig.IO.CLAMP_FWD_CHECK);
        public bool IsClampBwd() => !IsClampFwd();

        public bool Ring0() => ReadInput(OutputStageConfig.IO.RING_CHECK0);
        public bool Ring1() => ReadInput(OutputStageConfig.IO.RING_CHECK1);
        public bool IsRingPresent() => Ring0() || Ring1();
        #endregion
    }
}