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
        #endregion

        #region IO Domain Mapping
        private Cylinder _clampLiftCylinder;   // CLAMP UP/DOWN
        private Cylinder _plateCylinder;       // Treat as Expander for UI
        private Vacuum _vacuum;                // Stage vacuum

        private const string NAME_CLAMP_UP = "BIN STAGE CLAMP UP";
        private const string NAME_CLAMP_DOWN = "BIN STAGE CLAMP DOWN";
        private const string NAME_CLAMP = "BIN STAGE CLAMP";
        private const string NAME_UNCLAMP = "BIN STAGE UNCLAMP";
        private const string NAME_PLATE_UP = "BIN STAGE PLATE UP";      // used as ExpanderUp
        private const string NAME_PLATE_DOWN = "BIN STAGE PLATE DOWN";  // used as ExpanderDown
        private const string NAME_VAC = "BIN STAGE VACUUM";
        private const string NAME_VAC_OK = "BIN STAGE VACUUM CHECK"; // input
        private const string NAME_RING0 = "BIN STAGE RING CHECK 0";
        private const string NAME_RING1 = "BIN STAGE RING CHECK 1";

        private void BindIoDomains()
        {
            var eq = Equipment.Instance; var unit = eq?.UnitIO; if (unit == null) return;
            // Clamp lift
            DIO.MapByName(unit, "OutStage.ClampUpOut", true, NAME_CLAMP_UP);
            DIO.MapByName(unit, "OutStage.ClampDownOut", true, NAME_CLAMP_DOWN);
            DIO.MapByName(unit, "OutStage.ClampUpIn", false, NAME_CLAMP);
            DIO.MapByName(unit, "OutStage.ClampDownIn", false, NAME_CLAMP_DOWN);
            _clampLiftCylinder = new Cylinder("OutStageClampLift", "OutStage.ClampUpOut", "OutStage.ClampDownOut", "OutStage.ClampUpIn", "OutStage.ClampDownIn");
            // Plate (Expander)
            DIO.MapByName(unit, "OutStage.PlateUpOut", true, NAME_PLATE_UP);
            DIO.MapByName(unit, "OutStage.PlateDownOut", true, NAME_PLATE_DOWN);
            DIO.MapByName(unit, "OutStage.PlateUpIn", false, NAME_PLATE_UP);
            DIO.MapByName(unit, "OutStage.PlateDownIn", false, NAME_PLATE_DOWN);
            _plateCylinder = new Cylinder("OutStagePlate(Expander)", "OutStage.PlateUpOut", "OutStage.PlateDownOut", "OutStage.PlateUpIn", "OutStage.PlateDownIn");
            // Vacuum
            DIO.MapByName(unit, "OutStage.VacOut", true, NAME_VAC);
            DIO.MapByName(unit, "OutStage.VacOk", false, NAME_VAC_OK);
            _vacuum = new Vacuum("OutStage", "OutStage.VacOut", "OutStage.VacOk");
            // Clamp / Unclamp simple outs (already partially mapped above for sensors)
            DIO.MapByName(unit, "OutStage.ClampOut", true, NAME_CLAMP);
            DIO.MapByName(unit, "OutStage.UnclampOut", true, NAME_UNCLAMP);
        }

        // Clamp Lift
        public bool ClampLiftUp(int timeoutMs = 3000) => _clampLiftCylinder?.Extend(timeoutMs) ?? false;
        public bool ClampLiftDown(int timeoutMs = 3000) => _clampLiftCylinder?.Retract(timeoutMs) ?? false;

        // Unified Expander API (uses internal plate cylinder)
        public bool ExpanderUp(int timeoutMs = 3000) => _plateCylinder?.Extend(timeoutMs) ?? false;
        public bool ExpanderDown(int timeoutMs = 3000) => _plateCylinder?.Retract(timeoutMs) ?? false;
        public bool IsExpanderUp() => ReadInput(NAME_PLATE_UP);
        public bool IsExpanderDown() => ReadInput(NAME_PLATE_DOWN);

        // Backward compatibility (mark old Plate* as obsolete if referenced in legacy code)
        [Obsolete("Use ExpanderUp() instead")] public bool PlateUp(int timeoutMs = 3000) => ExpanderUp(timeoutMs);
        [Obsolete("Use ExpanderDown() instead")] public bool PlateDown(int timeoutMs = 3000) => ExpanderDown(timeoutMs);

        // Clamp (simple On/Off outputs)
        public void Clamp(bool on) { WriteOutput(NAME_CLAMP, on); WriteOutput(NAME_UNCLAMP, !on); }
        public bool IsClamp() => ReadInput(NAME_CLAMP);
        public bool IsClampDown() => ReadInput(NAME_CLAMP_DOWN);

        // Vacuum
        public void VacuumOn() => _vacuum?.On();
        public void VacuumOff() => _vacuum?.Off();
        public bool IsVacuum() => (_vacuum?.IsOk() ?? false) || ReadInput(NAME_VAC_OK);
        [Obsolete("Use IsVacuum() instead")] public bool VacuumCheck() => IsVacuum();
        public bool VacuumOk() => _vacuum?.IsOk() ?? false; // keep for potential logic use

        // Ring checks
        public bool Ring0() => ReadInput(NAME_RING0);
        public bool Ring1() => ReadInput(NAME_RING1);
        public bool IsRingPresent() => Ring0() || Ring1();
        #endregion
    }
}