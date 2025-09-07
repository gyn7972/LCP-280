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
    public class InputStageEjector : BaseUnit
    {
        public InputStageEjectorConfig InputStageEjectorConfig { get; private set; }
        public List<TeachingPosition> TeachingPositions { get; private set; } = new List<TeachingPosition>();

        public InputStageEjector(InputStageEjectorConfig config = null) : base("InputStageEjectorConfig")
        {
            InputStageEjectorConfig = config ?? new InputStageEjectorConfig();
            AddComponents();
        }

        public override void AddComponents()
        {
            InputStageEjectorConfig.LoadAndBindAxes(Equipment.Instance.AxisManager);
            InputStageEjectorConfig.InitializeDefaultTeachingPositions();
            TeachingPositions.Clear();
            foreach (var tp in InputStageEjectorConfig.TeachingPositions)
                TeachingPositions.Add(tp);
            BindAxes();
            BindIoDomains();
        }

        public override void OnRun() => base.OnRun();
        public override void OnStop() { base.OnStop(); }

        public void TeachCurrentPosition(string positionName, string description = null)
        {
            var axisPositions = new Dictionary<string, double>();
            foreach (var axisPair in Axes)
                axisPositions[axisPair.Key] = axisPair.Value.GetPosition();
            var tp = new TeachingPosition(positionName, axisPositions, description);
            InputStageEjectorConfig.SetTeachingPosition(tp);
        }

        public int MoveToTeachingPosition(string positionName, double vel = 5, double acc = 10, double dec = 10, double jerk = 50)
        {
            var tp = InputStageEjectorConfig.GetTeachingPosition(positionName);
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
        private MotionAxis _axZ;
        private MotionAxis _axPinZ;
        public MotionAxis AxisZ => _axZ;
        public MotionAxis AxisPinZ => _axPinZ;
        private void BindAxes()
        {
            Axes.TryGetValue("Eject Pin Z Axis", out _axPinZ);
            Axes.TryGetValue("Eject Z Axis", out _axZ);
        }
        public double GetTP(string tpName, string axisName)
        {
            var tp = InputStageEjectorConfig.GetTeachingPosition(tpName);
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
            // Hard IO 배열은 현재 주석 처리된 상태이므로 AutoBinding/DIO 직접 매핑 후 키로 접근할 수도 있음.
            // 여기서는 장비 전체 UnitIO에서 이름 기반 MapByName를 사용할 수 있게 래퍼 제공.
            var eq = Equipment.Instance; var unitIO = eq?.UnitIO; var dio = eq?.DioScan; if (unitIO == null || dio == null) return false;
            // 단순 스캔: 설정된 Config에 하드 입력 정의 없는 경우 false
            return false;
        }
        public bool WriteOutput(string name, bool on)
        {
            var eq = Equipment.Instance; var dio = eq?.DioScan; if (eq == null || dio == null) return false;
            return false;
        }
        #endregion

        #region IO Domain (Vacuum Only Example)
        private Vacuum _vacuum; // eject pin vacuum if exists
        private const string NAME_VAC_OUT = "EJECTOR VACUUM";
        private const string NAME_VAC_OK = "EJECTOR VACUUM CHECK";

        private void BindIoDomains()
        {
            var eq = Equipment.Instance; var unit = eq?.UnitIO; if (unit == null) return;
            DIO.MapByName(unit, "Ejector.VacOut", true, NAME_VAC_OUT);
            DIO.MapByName(unit, "Ejector.VacOk", false, NAME_VAC_OK);
            _vacuum = new Vacuum("Ejector", "Ejector.VacOut", "Ejector.VacOk");
        }

        public void VacuumOn() => _vacuum?.On();
        public void VacuumOff() => _vacuum?.Off();
        public bool VacuumOk() => _vacuum?.IsOk() ?? false;
        public bool VacuumCheck() => VacuumOk();
        #endregion
    }
}