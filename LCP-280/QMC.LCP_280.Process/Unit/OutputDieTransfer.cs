using QMC.Common;
using QMC.Common.Component;
using QMC.Common.IOUtil;
using QMC.Common.Motion;
using QMC.Common.Motions;
using QMC.Common.Unit;
using QMC.LCP_280.Process.Component;
using System;
using System.Collections.Generic;
using System.Linq;
using static QMC.LCP_280.Process.Unit.OutputDieTransferConfig.IO; // IO 상수/배열

namespace QMC.LCP_280.Process.Unit
{
    public class OutputDieTransfer : BaseUnit
    {
        public OutputDieTransferConfig OutputDieTransferConfig { get; private set; }
        public List<TeachingPosition> TeachingPositions { get; private set; } = new List<TeachingPosition>();

        public OutputDieTransfer(OutputDieTransferConfig config = null)
            : base("OutputDieTransferConfig")
        {
            OutputDieTransferConfig = config ?? new OutputDieTransferConfig();
            AddComponents();
        }

        public override void AddComponents()
        {
            OutputDieTransferConfig.LoadAndBindAxes(Equipment.Instance.AxisManager);
            OutputDieTransferConfig.InitializeDefaultTeachingPositions();
            TeachingPositions.Clear();
            foreach (var tp in OutputDieTransferConfig.TeachingPositions)
                TeachingPositions.Add(tp);
            BindAxes();
            BindIoDomains();
        }

        public override void OnRun() => base.OnRun();
        public override void OnStop() => base.OnStop();

        public void TeachCurrentPosition(string positionName, string description = null)
        {
            var axisPositions = new Dictionary<string, double>();
            foreach (var axisPair in Axes)
                axisPositions[axisPair.Key] = axisPair.Value.GetPosition();
            var tp = new TeachingPosition(positionName, axisPositions, description);
            OutputDieTransferConfig.SetTeachingPosition(tp);
        }

        public int MoveToTeachingPosition(string positionName, double vel = 5, double acc = 10, double dec = 10, double jerk = 50)
        {
            var tp = OutputDieTransferConfig.GetTeachingPosition(positionName);
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

        public bool InPosTeaching(string positionName)
        {
            var tp = OutputDieTransferConfig.GetTeachingPosition(positionName);
            if (tp == null) return false;
            foreach (var kv in tp.AxisPositions)
            {
                if (!Axes.TryGetValue(kv.Key, out var axis) || !InPos(axis, kv.Value)) return false;
            }
            return true;
        }

        #region Axis Helpers
        private MotionAxis _toolT, _pickZ, _placeZ;
        public MotionAxis ToolT => _toolT;
        public MotionAxis PickZ => _pickZ;
        public MotionAxis PlaceZ => _placeZ;
        private void BindAxes()
        {
            var mgr = Equipment.Instance?.AxisManager;
            if (mgr == null)
            {
                Log.Write("OutputDieTransfer", "[BindAxes] AxisManager null");
                return;
            }

            const string unitName = "Unit"; // Equipment에서 축 등록 시 사용한 유닛명과 동일해야 함
            BindAxis(mgr, unitName, AxisNames.RightToolT, ref _toolT);
            BindAxis(mgr, unitName, AxisNames.RightPickZ, ref _pickZ);
            BindAxis(mgr, unitName, AxisNames.RightPlaceZ, ref _placeZ);

        }
        public double GetTP(string tpName, string axisName)
        {
            var tp = OutputDieTransferConfig.GetTeachingPosition(tpName);
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

        #region IO Helpers (Input / Output 상태)
        public bool ReadInput(string name)
        {
            var hi = OutputDieTransferConfig.HardInputs.FirstOrDefault(i => i.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (hi == null) return false;
            var eq = Equipment.Instance; var dio = eq?.DioScan; if (dio == null) return false;
            foreach (var m in eq.UnitIO.Modules)
                if (dio.TryGetInput(m.ModuleName, hi.Disp, out var v)) return v;
            return false;
        }
        public bool WriteOutput(string name, bool on)
        {
            var ho = OutputDieTransferConfig.HardOutputs.FirstOrDefault(o => o.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (ho == null) return false;
            var eq = Equipment.Instance; var dio = eq?.DioScan; if (dio == null) return false;
            foreach (var m in eq.UnitIO.Modules)
                if (dio.WriteOutput(m.ModuleName, ho.Disp, on) == 0) return true;
            return false;
        }

        // 출력 캐시 상태 조회 (입력과 무관하게 실제 On/Off 표시)
        public bool IsOutputOn(string name)
        {
            var ho = OutputDieTransferConfig.HardOutputs.FirstOrDefault(o => o.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (ho == null) return false;
            var eq = Equipment.Instance; var dio = eq?.DioScan; if (dio == null) return false;
            foreach (var m in eq.UnitIO.Modules)
            {
                if (dio.TryGetOutput(m.ModuleName, ho.Disp, out var v)) return v;
            }
            return false;
        }
        #endregion

        private Vacuum[] _vacuum = new Vacuum[4];              // Vacuum + OK sensor
        public Vacuum[] _blow = new Vacuum[4];
        public Vacuum[] _vent = new Vacuum[4];

        private void BindIoDomains()
        {
            var eq = Equipment.Instance; var unit = eq?.UnitIO; if (unit == null) return;

            // Vacuum 별칭으로 조회만
            if (!IoAutoBindings.Vacuums.TryGetValue("OutputDieTransferVac1", out _vacuum[0]))
            {
                Log.Write("OutputDieTransfer", "BindIoDomains", "Vacuums not found: OutputDieTransferVac1");
            }

            if (!IoAutoBindings.Vacuums.TryGetValue("OutputDieTransferVac2", out _vacuum[1]))
            {
                Log.Write("OutputDieTransfer", "BindIoDomains", "Vacuums not found: OutputDieTransferVac2");
            }

            if (!IoAutoBindings.Vacuums.TryGetValue("OutputDieTransferVac3", out _vacuum[2]))
            {
                Log.Write("OutputDieTransfer", "BindIoDomains", "Vacuums not found: OutputDieTransferVac3");
            }

            if (!IoAutoBindings.Vacuums.TryGetValue("OutputDieTransferVac4", out _vacuum[3]))
            {
                Log.Write("OutputDieTransfer", "BindIoDomains", "Vacuums not found: OutputDieTransferVac4");
            }

            if (!IoAutoBindings.Vacuums.TryGetValue("OutputDieTransferBlow1", out _blow[0]))
            {
                Log.Write("OutputDieTransfer", "BindIoDomains", "Vacuums not found: OutputDieTransferBlow1");
            }

            if (!IoAutoBindings.Vacuums.TryGetValue("OutputDieTransferBlow2", out _blow[1]))
            {
                Log.Write("OutputDieTransfer", "BindIoDomains", "Vacuums not found: OutputDieTransferBlow2");
            }

            if (!IoAutoBindings.Vacuums.TryGetValue("OutputDieTransferBlow3", out _blow[2]))
            {
                Log.Write("OutputDieTransfer", "BindIoDomains", "Vacuums not found: OutputDieTransferBlow3");
            }

            if (!IoAutoBindings.Vacuums.TryGetValue("OutputDieTransferBlow4", out _blow[3]))
            {
                Log.Write("OutputDieTransfer", "BindIoDomains", "Vacuums not found: OutputDieTransferBlow4");
            }

            if (!IoAutoBindings.Vacuums.TryGetValue("OutputDieTransferVent1", out _vent[0]))
            {
                Log.Write("OutputDieTransfer", "BindIoDomains", "Vacuums not found: OutputDieTransferVent1");
            }

            if (!IoAutoBindings.Vacuums.TryGetValue("OutputDieTransferVent2", out _vent[1]))
            {
                Log.Write("OutputDieTransfer", "BindIoDomains", "Vacuums not found: OutputDieTransferVent2");
            }

            if (!IoAutoBindings.Vacuums.TryGetValue("OutputDieTransferVent3", out _vent[2]))
            {
                Log.Write("OutputDieTransfer", "BindIoDomains", "Vacuums not found: OutputDieTransferVent3");
            }

            if (!IoAutoBindings.Vacuums.TryGetValue("OutputDieTransferVent4", out _vent[3]))
            {
                Log.Write("OutputDieTransfer", "BindIoDomains", "Vacuums not found: OutputDieTransferVent4");
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

        #region Arm Vacuum / Blow / Vent Control
        public bool ArmFlowOk(int armIndex) => armIndex >= 0 && armIndex < ARM_FLOW.Length && ReadInput(ARM_FLOW[armIndex]);
        public bool AirTankOk() => ReadInput(AIR_TANK_PRESS);
        public bool VacuumTankOk() => ReadInput(VAC_TANK_PRESS);
        #endregion
        /// //////////////////////////////////////////////////////////////////

        #region Seq 단위 동작 함수
        public int ChipPickUp()
        {
            int nRet = -1;
            /* TODO */
            return nRet;
        }
        public int RotateArm()
        {
            int nRet = -1;
            /* TODO */
            return nRet;
        }

        public int ChipPlaceDown()
        {
            int nRet = -1;
            /* TODO */
            return nRet;
        }
        #endregion

    }
}