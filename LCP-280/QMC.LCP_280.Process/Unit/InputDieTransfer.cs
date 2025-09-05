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

        public InputDieTransfer(InputDieTransferConfig config = null)
            : base("InputDieTransferConfig")
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
            // No cylinder mapping: only discrete vacuum/blow/vent outputs
        }

        public override void OnRun() => base.OnRun();
        public override void OnStop() => base.OnStop();

        public void TeachCurrentPosition(string positionName, string description = null)
        {
            var axisPositions = new Dictionary<string, double>();
            foreach (var axisPair in Axes)
                axisPositions[axisPair.Key] = axisPair.Value.GetPosition();
            var tp = new TeachingPosition(positionName, axisPositions, description);
            InputDieTransferConfig.SetTeachingPosition(tp);
        }

        public int MoveToTeachingPosition(string positionName, double vel = 5, double acc = 10, double dec = 10, double jerk = 50)
        {
            var tp = InputDieTransferConfig.GetTeachingPosition(positionName);
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
        private MotionAxis _toolT, _pickZ, _placeZ;
        public MotionAxis ToolT => _toolT;
        public MotionAxis PickZ => _pickZ;
        public MotionAxis PlaceZ => _placeZ;
        private void BindAxes()
        {
            Axes.TryGetValue("Left Tool T Axis", out _toolT);
            Axes.TryGetValue("Left Pick Z Axis", out _pickZ);
            Axes.TryGetValue("Left Place Z Axis", out _placeZ);
        }
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
            var hi = InputDieTransferConfig.HardInputs.FirstOrDefault(i => i.Name.Equals(name, System.StringComparison.OrdinalIgnoreCase));
            if (hi == null) return false;
            var eq = Equipment.Instance; var dio = eq?.DioScan; if (dio == null) return false;
            foreach (var m in eq.UnitIO.Modules)
                if (dio.TryGetInput(m.ModuleName, hi.Disp, out var v)) return v;
            return false;
        }
        public bool WriteOutput(string name, bool on)
        {
            var ho = InputDieTransferConfig.HardOutputs.FirstOrDefault(o => o.Name.Equals(name, System.StringComparison.OrdinalIgnoreCase));
            if (ho == null) return false;
            var eq = Equipment.Instance; var dio = eq?.DioScan; if (dio == null) return false;
            foreach (var m in eq.UnitIO.Modules)
                if (dio.WriteOutput(m.ModuleName, ho.Disp, on) == 0) return true;
            return false;
        }
        #endregion

        #region Arm Vacuum / Blow / Vent Control
        private static readonly string[] VAC_NAMES = { "LEFT ARM 1 VACUUM", "LEFT ARM 2 VACUUM", "LEFT ARM 3 VACUUM", "LEFT ARM 4 VACUUM" };
        private static readonly string[] BLOW_NAMES = { "LEFT ARM 1 BLOW", "LEFT ARM 2 BLOW", "LEFT ARM 3 BLOW", "LEFT ARM 4 BLOW" };
        private static readonly string[] VENT_NAMES = { "LEFT ARM 1 VENT", "LEFT ARM 2 VENT", "LEFT ARM 3 VENT", "LEFT ARM 4 VENT" };

        public void SetArmVac(int armIndex, bool on) => SetIndexedOutput(VAC_NAMES, armIndex, on);
        public void SetArmBlow(int armIndex, bool on) => SetIndexedOutput(BLOW_NAMES, armIndex, on);
        public void SetArmVent(int armIndex, bool on) => SetIndexedOutput(VENT_NAMES, armIndex, on);
        public void AllVacOff() { for (int i = 0; i < 4; i++) SetArmVac(i, false); }
        public void AllBlowOff() { for (int i = 0; i < 4; i++) SetArmBlow(i, false); }
        public void AllVentOff() { for (int i = 0; i < 4; i++) SetArmVent(i, false); }

        private void SetIndexedOutput(string[] arr, int armIdx, bool on)
        {
            if (armIdx < 0 || armIdx >= arr.Length) return;
            WriteOutput(arr[armIdx], on);
        }
        #endregion
    }
}