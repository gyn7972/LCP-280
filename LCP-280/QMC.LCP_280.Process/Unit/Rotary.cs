using QMC.Common;
using QMC.Common.Component;
using QMC.Common.IOUtil;
using QMC.Common.Motion;
using QMC.Common.Motions;
using QMC.Common.Unit;
using QMC.LCP_280.Process.Component;
using System.Collections.Generic;
using System.Linq;
using static QMC.LCP_280.Process.Unit.RotaryConfig.IO; // IO 상수/배열 직접 사용

namespace QMC.LCP_280.Process.Unit
{
    public class Rotary : BaseUnit
    {
        public RotaryConfig RotaryConfig { get; private set; }
        public List<TeachingPosition> TeachingPositions { get; private set; } = new List<TeachingPosition>();

        private MotionAxis _axisT;
        public MotionAxis AxisT => _axisT;

        public bool DryRun { get; private set; }
        public void SetDryRun(bool on) => DryRun = on;
        private readonly Dictionary<string, bool> _simOutputs = new Dictionary<string, bool>(System.StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, bool> _simInputs  = new Dictionary<string, bool>(System.StringComparer.OrdinalIgnoreCase);

        // 로컬 Safe 명칭 허용(오타 포함)
        private static readonly string[] SafeNames = new[] { "SafeZone", "Safe", "SasfeZone", "SAFE", "SAFEZONE", "SAFE_ZONE" };

        public Rotary(RotaryConfig config = null) : base("Rotary")
        {
            RotaryConfig = config ?? new RotaryConfig();
            AddComponents();
        }

        public override void AddComponents()
        {
            RotaryConfig.LoadAndBindAxes(Equipment.Instance.AxisManager);
            RotaryConfig.InitializeDefaultTeachingPositions();
            TeachingPositions.Clear();
            foreach (var tp in RotaryConfig.TeachingPositions) TeachingPositions.Add(tp);
            BindAxes();
            BindIoDomains();

            var il = InterlockManager.Instance;
            il.AddAxisMustBeHomed("RotaryTHomed", _axisT, "T축 Home 완료 후 동작 가능합니다.");
            il.AddGlobalRule("EquipStateRunningBlock", () =>
            {
                return Equipment.Instance != null && Equipment.Instance.State == EquipmentState.Running
                    ? "자동운전 중에는 인덱스 수동 이동이 불가합니다." : null;
            });
        }

        private void BindAxes()
        {
            //AxisNames.IndexT
            var mgr = Equipment.Instance?.AxisManager;
            if (mgr == null)
            {
                Log.Write("InputCassetteLifter", "[BindAxes] AxisManager null");
                return;
            }

            const string unitName = "Unit"; // Equipment에서 축 등록 시 사용한 유닛명과 동일해야 함
            BindAxis(mgr, unitName, AxisNames.IndexT, ref _axisT);

        }

        public override void OnRun() => base.OnRun();
        public override void OnStop() => base.OnStop();

        #region Teaching
        public void TeachCurrentPosition(string name, string description = null)
        {
            var pos = new Dictionary<string, double>();
            foreach (var kv in Axes) pos[kv.Key] = kv.Value.GetPosition();
            RotaryConfig.SetTeachingPosition(new TeachingPosition(name, pos, description));
        }

        public int MoveToTeachingPosition(string name, double vel = 0, double acc = 0, double dec = 0, double jerk = 0)
        {
            var tp = RotaryConfig.GetTeachingPosition(name); if (tp == null) return -1;
            double t = RotaryConfig.GetPositionWithOffset(name);
            if (_axisT == null) return -2;
            return _axisT.MoveAbs(t,
                vel  > 0 ? vel  : _axisT.Config.MaxVelocity,
                acc  > 0 ? acc  : _axisT.Config.RunAcc,
                dec  > 0 ? dec  : _axisT.Config.RunDec,
                jerk > 0 ? jerk : _axisT.Config.AccJerkPercent);
        }

        public bool InPosTeaching(string name)
        {
            double t = RotaryConfig.GetPositionWithOffset(name);
            return InPos(_axisT, t);
        }

        public void ApplyOffset(string name, double deltaT) => RotaryConfig.SetOffset(name, deltaT);
        #endregion

        #region Axis helpers
        public double GetTP(string tpName, string axisName)
        {
            var tp = RotaryConfig.GetTeachingPosition(tpName);
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

        #region Index Move (with Interlock)
        public bool TryMoveIndexPrev(out string reason)
        {
            return TryMoveIndexStep(-1, out reason);
        }

        public bool TryMoveIndexNext(out string reason)
        {
            return TryMoveIndexStep(+1, out reason);
        }

        private bool TryMoveIndexStep(int step, out string reason)
        {
            reason = null;
            var axis = _axisT;
            if (axis == null)
            {
                reason = "T축이 바인딩되지 않았습니다.";
                return false;
            }

            // 1) 로컬 Safe-Zone 인터락: 4개 유닛이 Safe TeachingPosition에 있어야 함
            if (!VerifyAllUnitsSafe(out reason))
                return false;

            // 2) InterlockManager 규칙 검사(전역 + 축 관련)
            var il = InterlockManager.Instance;
            if (!il.ValidateAxisForHome(axis, out reason))
                return false;
            if (!il.ValidateForHomeStep(new[] { axis }, out reason))
                return false;

            // 3) 실제 인덱스 이동
            int rc = step < 0 ? axis.MovePrevIndex() : axis.MoveNextIndex();
            if (rc != 0)
            {
                reason = $"Index 이동 실패(rc={rc})";
                return false;
            }
            return true;
        }

        private bool VerifyAllUnitsSafe(out string reason)
        {
            reason = null;
            var eq = Equipment.Instance;
            if (eq == null || eq.Units == null) return true; // 설비 미준비 시 차단하지 않음

            // IndexChipProbeController
            if (eq.Units.TryGetValue("IndexChipProbeController", out var u1) && u1 is IndexChipProbeController prober)
            {
                if (!IsUnitInSafe(prober.InPosTeaching)) { reason = "IndexChipProbeController가 Safe Zone이 아닙니다."; return false; }
            }
            // IndexLoadAligner
            if (eq.Units.TryGetValue("IndexLoadAligner", out var u2) && u2 is IndexLoadAligner loadAligner)
            {
                if (!IsUnitInSafe(loadAligner.InPosTeaching)) { reason = "IndexLoadAligner가 Safe Zone이 아닙니다."; return false; }
            }
            // InputDieTransfer
            if (eq.Units.TryGetValue("InputDieTransfer", out var u3) && u3 is InputDieTransfer inputDie)
            {
                if (!IsUnitInSafe(inputDie.InPosTeaching)) { reason = "InputDieTransfer가 Safe Zone이 아닙니다."; return false; }
            }
            // OutputDieTransfer
            if (eq.Units.TryGetValue("OutputDieTransfer", out var u4) && u4 is OutputDieTransfer outputDie)
            {
                if (!IsUnitInSafe(outputDie.InPosTeaching)) { reason = "OutputDieTransfer가 Safe Zone이 아닙니다."; return false; }
            }

            return true;
        }

        private bool IsUnitInSafe(System.Func<string, bool> inPosTeaching)
        {
            for (int i = 0; i < SafeNames.Length; i++)
                if (inPosTeaching(SafeNames[i])) return true;
            return false;
        }
        #endregion

        #region IO Helpers
        public bool ReadInput(string name)
        {
            if (DryRun) { bool v; return _simInputs.TryGetValue(name, out v) && v; }
            var hi = RotaryConfig.HardInputs.FirstOrDefault(i => i.Name.Equals(name, System.StringComparison.OrdinalIgnoreCase));
            if (hi == null) return false;
            var eq = Equipment.Instance; var dio = eq?.DioScan; if (dio == null) return false;
            foreach (var m in eq.UnitIO.Modules)
                if (dio.TryGetInput(m.ModuleName, hi.Disp, out var v)) return v;
            return false;
        }

        public bool WriteOutput(string name, bool on)
        {
            if (DryRun) { _simOutputs[name] = on; return true; }
            var ho = RotaryConfig.HardOutputs.FirstOrDefault(o => o.Name.Equals(name, System.StringComparison.OrdinalIgnoreCase));
            if (ho == null) return false;
            var eq = Equipment.Instance; var dio = eq?.DioScan; if (dio == null) return false;
            foreach (var m in eq.UnitIO.Modules)
                if (dio.WriteOutput(m.ModuleName, ho.Disp, on) == 0) return true;
            return false;
        }

        public bool IsOutputOn(string name)
        {
            if (DryRun) { bool v; return _simOutputs.TryGetValue(name, out v) && v; }
            var ho = RotaryConfig.HardOutputs.FirstOrDefault(o => o.Name.Equals(name, System.StringComparison.OrdinalIgnoreCase));
            if (ho == null) return false;
            var eq = Equipment.Instance; var dio = eq?.DioScan; if (dio == null) return false;
            foreach (var m in eq.UnitIO.Modules)
                if (dio.TryGetOutput(m.ModuleName, ho.Disp, out var v)) return v;
            return false;
        }
        #endregion

        private Vacuum[] _vacuum = new Vacuum[8];              // Vacuum + OK sensor
        public Vacuum[] _blow = new Vacuum[8];
        public Vacuum[] _vent = new Vacuum[8];

        private void BindIoDomains()
        {
            var eq = Equipment.Instance; var unit = eq?.UnitIO; if (unit == null) return;

            // Vacuum 별칭으로 조회만
            if (!IoAutoBindings.Vacuums.TryGetValue("RotatyVac1", out _vacuum[0]))
            {
                Log.Write("Rotaty", "BindIoDomains", "Vacuums not found: RotatyVac1");
            }

            if (!IoAutoBindings.Vacuums.TryGetValue("RotatyVac2", out _vacuum[1]))
            {
                Log.Write("Rotaty", "BindIoDomains", "Vacuums not found: RotatyVac2");
            }

            if (!IoAutoBindings.Vacuums.TryGetValue("RotatyVac3", out _vacuum[2]))
            {
                Log.Write("Rotaty", "BindIoDomains", "Vacuums not found: RotatyVac3");
            }

            if (!IoAutoBindings.Vacuums.TryGetValue("RotatyVac4", out _vacuum[3]))
            {
                Log.Write("Rotaty", "BindIoDomains", "Vacuums not found: RotatyVac4");
            }

            if (!IoAutoBindings.Vacuums.TryGetValue("RotatyVac5", out _vacuum[4]))
            {
                Log.Write("Rotaty", "BindIoDomains", "Vacuums not found: RotatyVac5");
            }

            if (!IoAutoBindings.Vacuums.TryGetValue("RotatyVac6", out _vacuum[5]))
            {
                Log.Write("Rotaty", "BindIoDomains", "Vacuums not found: RotatyVac6");
            }

            if (!IoAutoBindings.Vacuums.TryGetValue("RotatyVac7", out _vacuum[6]))
            {
                Log.Write("Rotaty", "BindIoDomains", "Vacuums not found: RotatyVac7");
            }

            if (!IoAutoBindings.Vacuums.TryGetValue("RotatyVac8", out _vacuum[7]))
            {
                Log.Write("Rotaty", "BindIoDomains", "Vacuums not found: RotatyVac8");
            }

            if (!IoAutoBindings.Vacuums.TryGetValue("RotatyBlow1", out _blow[0]))
            {
                Log.Write("Rotaty", "BindIoDomains", "Vacuums not found: RotatyBlow1");
            }

            if (!IoAutoBindings.Vacuums.TryGetValue("RotatyBlow2", out _blow[1]))
            {
                Log.Write("Rotaty", "BindIoDomains", "Vacuums not found: RotatyBlow2");
            }

            if (!IoAutoBindings.Vacuums.TryGetValue("RotatyBlow3", out _blow[2]))
            {
                Log.Write("Rotaty", "BindIoDomains", "Vacuums not found: RotatyBlow3");
            }

            if (!IoAutoBindings.Vacuums.TryGetValue("RotatyBlow4", out _blow[3]))
            {
                Log.Write("Rotaty", "BindIoDomains", "Vacuums not found: RotatyBlow4");
            }

            if (!IoAutoBindings.Vacuums.TryGetValue("RotatyBlow5", out _blow[4]))
            {
                Log.Write("Rotaty", "BindIoDomains", "Vacuums not found: RotatyBlow5");
            }

            if (!IoAutoBindings.Vacuums.TryGetValue("RotatyBlow6", out _blow[5]))
            {
                Log.Write("Rotaty", "BindIoDomains", "Vacuums not found: RotatyBlow6");
            }

            if (!IoAutoBindings.Vacuums.TryGetValue("RotatyBlow7", out _blow[6]))
            {
                Log.Write("Rotaty", "BindIoDomains", "Vacuums not found: RotatyBlow7");
            }

            if (!IoAutoBindings.Vacuums.TryGetValue("RotatyBlow8", out _blow[7]))
            {
                Log.Write("Rotaty", "BindIoDomains", "Vacuums not found: RotatyBlow8");
            }

            if (!IoAutoBindings.Vacuums.TryGetValue("RotatyVent1", out _vent[0]))
            {
                Log.Write("Rotaty", "BindIoDomains", "Vacuums not found: RotatyVent1");
            }

            if (!IoAutoBindings.Vacuums.TryGetValue("RotatyVent2", out _vent[1]))
            {
                Log.Write("Rotaty", "BindIoDomains", "Vacuums not found: RotatyVent2");
            }

            if (!IoAutoBindings.Vacuums.TryGetValue("RotatyVent3", out _vent[2]))
            {
                Log.Write("Rotaty", "BindIoDomains", "Vacuums not found: RotatyVent3");
            }

            if (!IoAutoBindings.Vacuums.TryGetValue("RotatyVent4", out _vent[3]))
            {
                Log.Write("Rotaty", "BindIoDomains", "Vacuums not found: RotatyVent4");
            }

            if (!IoAutoBindings.Vacuums.TryGetValue("RotatyVent5", out _vent[4]))
            {
                Log.Write("Rotaty", "BindIoDomains", "Vacuums not found: RotatyVent5");
            }

            if (!IoAutoBindings.Vacuums.TryGetValue("RotatyVent6", out _vent[5]))
            {
                Log.Write("Rotaty", "BindIoDomains", "Vacuums not found: RotatyVent6");
            }

            if (!IoAutoBindings.Vacuums.TryGetValue("RotatyVent7", out _vent[6]))
            {
                Log.Write("Rotaty", "BindIoDomains", "Vacuums not found: RotatyVent7");
            }

            if (!IoAutoBindings.Vacuums.TryGetValue("RotatyVent8", out _vent[7]))
            {
                Log.Write("Rotaty", "BindIoDomains", "Vacuums not found: RotatyVent8");
            }

        }

        // === Domain Control (표준 구동) ===
        public bool SetVacuum(int nNo, bool on)
        {
            if (_vacuum[nNo] == null) return false;
            if (on) _vacuum[nNo].On();
            else _vacuum[nNo].Off();
            return true;
        }

        public bool SetBlow(int nNo, bool on)
        {
            if (_blow[nNo] == null) return false;
            if (on) _blow[nNo].On();
            else _blow[nNo].Off();
            return true;
        }

        public bool SetVent(int nNo, bool on)
        {
            if (_vent[nNo] == null) return false;
            if (on) _vent[nNo].On();
            else _vent[nNo].Off();
            return true;
        }

        public bool SlotFlowOk(int slotIndex) => slotIndex >= 0 && slotIndex < FLOW.Length && ReadInput(FLOW[slotIndex]);

        #region Pressure
        public bool AirTankPressureOk() => ReadInput(AIR_TANK_PRESSURE);
        public bool VacTankPressureOk() => ReadInput(VAC_TANK_PRESSURE) || ReadInput(VAC_TANK_PRESSURE_LEGACY);
        #endregion
    }
}