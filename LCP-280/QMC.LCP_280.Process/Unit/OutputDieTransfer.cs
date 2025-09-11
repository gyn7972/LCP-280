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

        #region Arm Vacuum / Blow / Vent Control
        public void SetArmVac(int armIndex, bool on) => SetIndexedOutput(ARM_VAC, armIndex, on);
        public void SetArmBlow(int armIndex, bool on) => SetIndexedOutput(ARM_BLOW, armIndex, on);
        public void SetArmVent(int armIndex, bool on) => SetIndexedOutput(ARM_VENT, armIndex, on);

        public bool IsArmVacOn(int armIndex) => armIndex >= 0 && armIndex < ARM_VAC.Length && IsOutputOn(ARM_VAC[armIndex]);
        public bool IsArmBlowOn(int armIndex) => armIndex >= 0 && armIndex < ARM_BLOW.Length && IsOutputOn(ARM_BLOW[armIndex]);
        public bool IsArmVentOn(int armIndex) => armIndex >= 0 && armIndex < ARM_VENT.Length && IsOutputOn(ARM_VENT[armIndex]);

        public void AllVacOff() { for (int i = 0; i < ARM_VAC.Length; i++) SetArmVac(i, false); }
        public void AllBlowOff() { for (int i = 0; i < ARM_BLOW.Length; i++) SetArmBlow(i, false); }
        public void AllVentOff() { for (int i = 0; i < ARM_VENT.Length; i++) SetArmVent(i, false); }

        public bool ArmFlowOk(int armIndex) => armIndex >= 0 && armIndex < ARM_FLOW.Length && ReadInput(ARM_FLOW[armIndex]);
        public bool AirTankOk() => ReadInput(AIR_TANK_PRESS);
        public bool VacuumTankOk() => ReadInput(VAC_TANK_PRESS);

        private void SetIndexedOutput(string[] arr, int armIdx, bool on)
        {
            if (armIdx < 0 || armIdx >= arr.Length) return;
            WriteOutput(arr[armIdx], on);
        }
        #endregion
    }
}