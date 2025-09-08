using QMC.Common;
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
    public class InputDieTransfer : BaseUnit
    {
        public InputDieTransferConfig InputDieTransferConfig { get; private set; }
        public List<TeachingPosition> TeachingPositions { get; private set; } = new List<TeachingPosition>();

        public bool DryRun { get; private set; }
        public void SetDryRun(bool on) => DryRun = on;

        private MotionAxis _toolT, _pickZ, _placeZ;
        public MotionAxis ToolT => _toolT; public MotionAxis PickZ => _pickZ; public MotionAxis PlaceZ => _placeZ;

        // Sim IO for DryRun
        private readonly Dictionary<string, bool> _simOutputs = new Dictionary<string, bool>(System.StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, bool> _simInputs = new Dictionary<string, bool>(System.StringComparer.OrdinalIgnoreCase);

        public InputDieTransfer(InputDieTransferConfig config = null) : base("InputDieTransfer")
        {
            InputDieTransferConfig = config ?? new InputDieTransferConfig();
            AddComponents();
        }

        public override void AddComponents()
        {
            InputDieTransferConfig.LoadAndBindAxes(Equipment.Instance.AxisManager);
            InputDieTransferConfig.InitializeDefaultTeachingPositions();
            TeachingPositions.Clear();
            foreach (var tp in InputDieTransferConfig.TeachingPositions)
                TeachingPositions.Add(tp);
            BindAxes();
        }

        private void BindAxes()
        {
            Axes.TryGetValue("Left Tool T Axis", out _toolT);
            Axes.TryGetValue("Left Pick Z Axis", out _pickZ);
            Axes.TryGetValue("Left Place Z Axis", out _placeZ);
            bool useInPos = !InputDieTransferConfig.EnablePredictiveControl;
            foreach (var ax in new[] { _toolT, _pickZ, _placeZ })
            {
                if (ax == null) continue;
                try
                {
                    var mi = ax.GetType().GetMethod("SetInPositionEnable");
                    var mr = ax.GetType().GetMethod("SetInPositionRange");
                    if (mi != null) mi.Invoke(ax, new object[] { useInPos });
                    if (mr != null) mr.Invoke(ax, new object[] { InputDieTransferConfig.MoveDoneRemainDistance });
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
            InputDieTransferConfig.SetTeachingPosition(new TeachingPosition(name, pos, description));
        }
        public int MoveToTeachingPosition(string name, double vel = 0, double acc = 0, double dec = 0, double jerk = 0)
        {
            var tp = InputDieTransferConfig.GetTeachingPosition(name);
            if (tp == null) return -1;
            var (t, pz, plz) = InputDieTransferConfig.GetPositionWithOffset(name);
            int rc = 0;
            if (_toolT != null) rc |= _toolT.MoveAbs(t, vel > 0 ? vel : _toolT.Config.MaxVelocity, acc > 0 ? acc : _toolT.Config.RunAcc, dec > 0 ? dec : _toolT.Config.RunDec, jerk > 0 ? jerk : _toolT.Config.AccJerkPercent);
            if (_pickZ != null) rc |= _pickZ.MoveAbs(pz, vel > 0 ? vel : _pickZ.Config.MaxVelocity, acc > 0 ? acc : _pickZ.Config.RunAcc, dec > 0 ? dec : _pickZ.Config.RunDec, jerk > 0 ? jerk : _pickZ.Config.AccJerkPercent);
            if (_placeZ != null) rc |= _placeZ.MoveAbs(plz, vel > 0 ? vel : _placeZ.Config.MaxVelocity, acc > 0 ? acc : _placeZ.Config.RunAcc, dec > 0 ? dec : _placeZ.Config.RunDec, jerk > 0 ? jerk : _placeZ.Config.AccJerkPercent);
            return rc;
        }
        public bool InPosTeaching(string name)
        {
            var (t, pz, plz) = InputDieTransferConfig.GetPositionWithOffset(name);
            return InPos(_toolT, t) && InPos(_pickZ, pz) && InPos(_placeZ, plz);
        }
        public void ApplyOffset(string name, double t, double pickZ, double placeZ)
            => InputDieTransferConfig.SetOffset(name, t, pickZ, placeZ);
        #endregion

        #region Axis helpers
        public double GetTP(string tpName, string axisName)
        {
            var tp = InputDieTransferConfig.GetTeachingPosition(tpName);
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
            var hi = InputDieTransferConfig.HardInputs.FirstOrDefault(i => i.Name.Equals(name, System.StringComparison.OrdinalIgnoreCase));
            if (hi == null) return false;
            var eq = Equipment.Instance; var dio = eq?.DioScan; if (dio == null) return false;
            foreach (var m in eq.UnitIO.Modules)
                if (dio.TryGetInput(m.ModuleName, hi.Disp, out var v)) return v;
            return false;
        }
        public bool WriteOutput(string name, bool on)
        {
            if (DryRun) { _simOutputs[name] = on; return true; }
            var ho = InputDieTransferConfig.HardOutputs.FirstOrDefault(o => o.Name.Equals(name, System.StringComparison.OrdinalIgnoreCase));
            if (ho == null) return false;
            var eq = Equipment.Instance; var dio = eq?.DioScan; if (dio == null) return false;
            foreach (var m in eq.UnitIO.Modules)
                if (dio.WriteOutput(m.ModuleName, ho.Disp, on) == 0) return true;
            return false;
        }
        #endregion

        #region Arm Vacuum / Blow / Vent Control
        private static readonly string[] VAC_NAMES = { InputDieTransferConfig.IO.ARM1_VAC, InputDieTransferConfig.IO.ARM2_VAC, InputDieTransferConfig.IO.ARM3_VAC, InputDieTransferConfig.IO.ARM4_VAC };
        private static readonly string[] BLOW_NAMES = { InputDieTransferConfig.IO.ARM1_BLOW, InputDieTransferConfig.IO.ARM2_BLOW, InputDieTransferConfig.IO.ARM3_BLOW, InputDieTransferConfig.IO.ARM4_BLOW };
        private static readonly string[] VENT_NAMES = { InputDieTransferConfig.IO.ARM1_VENT, InputDieTransferConfig.IO.ARM2_VENT, InputDieTransferConfig.IO.ARM3_VENT, InputDieTransferConfig.IO.ARM4_VENT };
        public void SetArmVac(int armIndex, bool on) => SetIndexedOutput(VAC_NAMES, armIndex, on);
        public void SetArmBlow(int armIndex, bool on) => SetIndexedOutput(BLOW_NAMES, armIndex, on);
        public void SetArmVent(int armIndex, bool on) => SetIndexedOutput(VENT_NAMES, armIndex, on);
        public void AllVacOff() { for (int i = 0; i < VAC_NAMES.Length; i++) SetArmVac(i, false); }
        public void AllBlowOff() { for (int i = 0; i < BLOW_NAMES.Length; i++) SetArmBlow(i, false); }
        public void AllVentOff() { for (int i = 0; i < VENT_NAMES.Length; i++) SetArmVent(i, false); }
        private void SetIndexedOutput(string[] arr, int idx, bool on)
        { if (idx < 0 || idx >= arr.Length) return; WriteOutput(arr[idx], on); }
        #endregion
    }
}