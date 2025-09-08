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

        #region IO Domain Mapping (Reorganized)
        private Cylinder _cylClampLift;      // Up/Down (Clamp Lift)
        private Cylinder _cylClampFB;        // FWD/BWD (Clamp Fwd/Bwd)
        private Cylinder _cylPlate;     // Plate (Expander)
        private Vacuum _vacuum;         // Vacuum

        // === BIN STAGE INPUTS ===
        private const string NAME_RING0 = "BIN STAGE RING CHECK 0";
        private const string NAME_RING1 = "BIN STAGE RING CHECK 1";
        private const string NAME_CLAMP_FWD = "BIN STAGE CLAMP FWD CHECK";      // Lift Up sensor
        private const string NAME_CLAMP_DOWN = "BIN STAGE CLAMP DOWN CHECK";    // Clamp BWD(sensor available)
        private const string NAME_PLATE_UP = "BIN STAGE PLATE UP";      // Plate Up sensor
        private const string NAME_PLATE_DN = "BIN STAGE PLATE DOWN";    // Plate Down sensor
        private const string NAME_VAC_OK = "BIN STAGE VACUUM CHECK";    // Vacuum OK

        // === BIN STAGE OUTPUTS ===
        private const string NAME_CLAMP_UP_OUT = "BIN STAGE CLAMP UP";      // Lift Up sol
        private const string NAME_CLAMP_DOWN_OUT = "BIN STAGE CLAMP DOWN";  // Lift Down sol
        private const string NAME_CLAMP_FWD_OUT = "BIN STAGE CLAMP FWD";    // Clamp Close
        private const string NAME_CLAMP_BWD_OUT = "BIN STAGE CLAMP BWD";    // Clamp Open
        private const string NAME_PLATE_UP_OUT = "BIN STAGE PLATE UP";      // Plate Up
        private const string NAME_PLATE_DN_OUT = "BIN STAGE PLATE DOWN";    // Plate Down
        private const string NAME_VAC_OUT = "BIN STAGE VACUUM";             // Vacuum

        private void BindIoDomains()
        {
            var eq = Equipment.Instance; var unit = eq?.UnitIO; if (unit == null) return;

            // Vacuum
            DIO.MapByName(unit, "OutStage.VacOut", true, NAME_VAC_OUT);
            DIO.MapByName(unit, "OutStage.VacOk", false, NAME_VAC_OK);
            _vacuum = new Vacuum("OutStageVac", "OutStage.VacOut", "OutStage.VacOk");

            // Plate (Expander)
            DIO.MapByName(unit, "OutStage.PlateUpOut", true, NAME_PLATE_UP_OUT);
            DIO.MapByName(unit, "OutStage.PlateDownOut", true, NAME_PLATE_DN_OUT);
            DIO.MapByName(unit, "OutStage.PlateUpIn", false, NAME_PLATE_UP);
            DIO.MapByName(unit, "OutStage.PlateDownIn", false, NAME_PLATE_DN);
            _cylPlate = new Cylinder("OutStagePlate", "OutStage.PlateUpOut", "OutStage.PlateDownOut", "OutStage.PlateUpIn", "OutStage.PlateDownIn");

            // Lift (Up/Down) : Outputs 2개 (UP/DOWN) + Input 1개(UP) 만 존재
            DIO.MapByName(unit, "OutStage.LiftUpOut", true, NAME_CLAMP_UP_OUT);
            DIO.MapByName(unit, "OutStage.LiftDownOut", true, NAME_CLAMP_DOWN_OUT);
            DIO.MapByName(unit, "OutStage.LiftDownIn", false, NAME_CLAMP_DOWN); // Only UP sensor physically present
            // DOWN 센서는 없음 -> Cylinder 생성 시 가상 NO_SENSOR 키 사용
            _cylClampLift = new Cylinder(
                "OutStageLift",
                "OutStage.LiftUpOut",
                "OutStage.LiftDownOut",
                "OutStage.LiftUpIn/*NO_SENSOR*/",
                "OutStage.LiftDownIn");

            // Clamp FWD/BWD (outputs 2 + BWD sensor 1개)
            DIO.MapByName(unit, "OutStage.ClampFwdOut", true, NAME_CLAMP_FWD_OUT);
            DIO.MapByName(unit, "OutStage.ClampBwdOut", true, NAME_CLAMP_BWD_OUT);
            DIO.MapByName(unit, "OutStage.ClampFwdIn", false, NAME_CLAMP_FWD);
            // FWD 센서는 없음 -> Cylinder 생성 시 가상 NO_SENSOR 키 사용
            _cylClampFB = new Cylinder(
                "OutStageLift",
                "OutStage.ClampFwdOut",
                "OutStage.ClampBwdOut",
                "OutStage.ClampFwdIn",
                "OutStage.ClampBwdIn/*NO_SENSOR*/");
        }

        // --- Vacuum ---
        public void VacuumOn() => _vacuum?.On();
        public void VacuumOff() => _vacuum?.Off();
        public bool IsVacuum() => (_vacuum?.IsOk() ?? false) || ReadInput(NAME_VAC_OK);
        [Obsolete("Use IsVacuum() instead")] public bool VacuumCheck() => IsVacuum();
        public bool VacuumOk() => _vacuum?.IsOk() ?? false;
        
        // --- Plate Cylinder API ---
        public bool PlateUp(int timeoutMs = 3000) => _cylPlate?.Extend(timeoutMs) ?? false;
        public bool PlateDown(int timeoutMs = 3000) => _cylPlate?.Retract(timeoutMs) ?? false;
        public bool IsPlateUp() => ReadInput(NAME_PLATE_UP);
        public bool IsPlateDown() => ReadInput(NAME_PLATE_DN);

        // --- Lift Cylinder API ---
        public bool ClampLiftUp(int timeoutMs = 3000) => _cylClampLift?.Extend(timeoutMs) ?? false;
        public bool ClampLiftDown(int timeoutMs = 3000) => _cylClampLift?.Retract(timeoutMs) ?? false;
        public bool IsClampLiftUp() => !IsClampLiftDown();
        // DOWN 센서가 없으므로 UP 센서 반전으로 판단 (필요 시 별도 로직 교체)
        public bool IsClampLiftDown() => ReadInput(NAME_CLAMP_DOWN);

        // --- FB Cylinder API ---
        public bool ClampFwd(int timeoutMs = 3000) => _cylClampFB?.Extend(timeoutMs) ?? false; // FWD = Extend (Close)
        public bool ClampBwd(int timeoutMs = 3000) => _cylClampFB?.Retract(timeoutMs) ?? false; // BWD = Retract (Open)
        // Only BWD sensor present -> FWD(closed) inferred by inverse
        public bool IsClampFwd() => ReadInput(NAME_CLAMP_FWD);
        public bool IsClampBwd() => !IsClampFwd();

        // === Backward Compatibility Wrappers (legacy UI expectation) ===
        // Clamp(bool) previously toggled simple outputs; now map to FWD/BWD cylinder
        public void Clamp(bool on)
        {
            if (on) { ClampFwd(); } else { ClampBwd(); }
        }
        // Legacy IsClamp = closed, IsClampDown = open
        public bool IsClamp() => IsClampFwd();
        public bool IsClampDown() => IsClampBwd();
        // Legacy Expander* mapped to Plate*
        public bool ExpanderUp(int timeoutMs = 3000) => PlateUp(timeoutMs);
        public bool ExpanderDown(int timeoutMs = 3000) => PlateDown(timeoutMs);
        public bool IsExpanderUp() => IsPlateUp();
        public bool IsExpanderDown() => IsPlateDown();
        // === End Wrappers ===

        // --- Ring Sensors ---
        public bool Ring0() => ReadInput(NAME_RING0);
        public bool Ring1() => ReadInput(NAME_RING1);
        public bool IsRingPresent() => Ring0() || Ring1();
        #endregion
    }
}