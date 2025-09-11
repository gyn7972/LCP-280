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
            var mgr = Equipment.Instance?.AxisManager;
            if (mgr == null)
            {
                Log.Write("InputCassetteLifter", "[BindAxes] AxisManager null");
                return;
            }

            const string unitName = "Unit"; // Equipment에서 축 등록 시 사용한 유닛명과 동일해야 함
            BindAxis(mgr, unitName, AxisNames.BinStageX, ref _axX);
            BindAxis(mgr, unitName, AxisNames.BinStageY, ref _axY);
            BindAxis(mgr, unitName, AxisNames.BinStageT, ref _axT);
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

            // Vacuum 별칭으로 조회만
            if (!IoAutoBindings.Vacuums.TryGetValue("OutStageVac", out _vacuum))
            {
                Log.Write("OutputStage", "BindIoDomains", "Vacuums not found: OutStageVac");
            }

            // Cylinder는 중앙 별칭으로 조회만
            if (!IoAutoBindings.Cylinders.TryGetValue("OutStagePlate", out _cylPlate))
            {
                Log.Write("OutputStage", "BindIoDomains", "Cylinder not found: OutStagePlate");
            }

            if (!IoAutoBindings.Cylinders.TryGetValue("OutStageLift", out _cylClampLift))
            {
                Log.Write("OutputStage", "BindIoDomains", "Cylinder not found: OutStageLift");
            }

            if (!IoAutoBindings.Cylinders.TryGetValue("OutStageClampFB", out _cylClampFB))
            {
                Log.Write("OutputStage", "BindIoDomains", "Cylinder not found: OutStageClampFB");
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
            if (_cylPlate == null) 
                return false;

            if (bUpDn) 
                return _cylPlate.Extend();
            else 
                return _cylPlate.Retract();
        }

        public bool SetClampLift(bool bUpDn)
        {
            if (_cylClampLift == null) 
                return false;

            if (bUpDn)
            {
                //if (IsClampBwd())
                //    return false; // 기존 인터락 유지

                return _cylClampLift.Extend();
            }
            else
            {
                if (!IsClampBwd()) 
                    return false; // 기존 인터락 유지

                return _cylClampLift.Retract();
            }
        }

        public bool SetClampFB(bool bFwdBwd)
        {
            if (_cylClampFB == null) 
                return false;

            if (bFwdBwd)
            {
                if (!IsClampLiftUp()) 
                    return false; // 기존 인터락 유지

                return _cylClampFB.Extend();
            }
            else
            {
                if (!IsClampLiftUp())
                    return false; // 기존 인터락 유지

                return _cylClampFB.Retract();
            }
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