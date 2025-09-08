using QMC.Common;
using QMC.Common.Component;
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
        }

        private void BindAxes()
        {
            Axes.TryGetValue("Index T Axis", out _axisT);
            bool useInPos = !RotaryConfig.EnablePredictiveControl;
            if (_axisT != null)
            {
                try
                {
                    var mi = _axisT.GetType().GetMethod("SetInPositionEnable");
                    var mr = _axisT.GetType().GetMethod("SetInPositionRange");
                    if (mi != null) mi.Invoke(_axisT, new object[] { useInPos });
                    if (mr != null) mr.Invoke(_axisT, new object[] { RotaryConfig.MoveDoneRemainDistance });
                }
                catch { }
            }
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

        #region Pressure
        public bool AirTankPressureOk() => ReadInput(AIR_TANK_PRESSURE);
        public bool VacTankPressureOk() => ReadInput(VAC_TANK_PRESSURE) || ReadInput(VAC_TANK_PRESSURE_LEGACY);
        #endregion

        #region Slot Vacuum/Blow/Vent Controls
        public void SetSlotVac(int slotIndex, bool on)  => SetIndexedOutput(SLOT_VAC,  slotIndex, on);
        public void SetSlotBlow(int slotIndex, bool on) => SetIndexedOutput(SLOT_BLOW, slotIndex, on);
        public void SetSlotVent(int slotIndex, bool on) => SetIndexedOutput(SLOT_VENT, slotIndex, on);

        public bool IsSlotVacOn(int slotIndex)  => slotIndex >= 0 && slotIndex < SLOT_VAC.Length  && IsOutputOn(SLOT_VAC[slotIndex]);
        public bool IsSlotBlowOn(int slotIndex) => slotIndex >= 0 && slotIndex < SLOT_BLOW.Length && IsOutputOn(SLOT_BLOW[slotIndex]);
        public bool IsSlotVentOn(int slotIndex) => slotIndex >= 0 && slotIndex < SLOT_VENT.Length && IsOutputOn(SLOT_VENT[slotIndex]);

        public bool SlotFlowOk(int slotIndex) => slotIndex >= 0 && slotIndex < FLOW.Length && ReadInput(FLOW[slotIndex]);

        public void AllVacOff()  { for (int i = 0; i < SLOT_VAC.Length; i++)  SetSlotVac(i, false); }
        public void AllBlowOff() { for (int i = 0; i < SLOT_BLOW.Length; i++) SetSlotBlow(i, false); }
        public void AllVentOff() { for (int i = 0; i < SLOT_VENT.Length; i++) SetSlotVent(i, false); }

        private void SetIndexedOutput(string[] arr, int idx, bool on)
        {
            if (idx < 0 || idx >= arr.Length) return;
            WriteOutput(arr[idx], on);
        }
        #endregion
    }
}