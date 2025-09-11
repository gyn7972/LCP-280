using QMC.Common;
using QMC.Common.Component;
using QMC.Common.Motion;
using QMC.Common.Motions;
using QMC.Common.Unit;
using QMC.LCP_280.Process.Component;
using System.Collections.Generic;
using System.Linq;

namespace QMC.LCP_280.Process.Unit
{
    /// <summary>
    /// InputCassetteLifter Unit
    ///  - Wafer Lifter (Input) 단일 축 + Teaching Positions
    ///  - Cassette / RingJut / Mapping 센서 상태 제공
    ///  - OutputStage 스타일 Region/메서드 구조
    /// </summary>
    public class InputCassetteLifter : BaseUnit
    {
        #region Config / Teaching
        public InputCassetteLifterConfig InputCassetteLifterConfig { get; private set; }
        public List<TeachingPosition> TeachingPositions { get; private set; } = new List<TeachingPosition>();
        #endregion

        #region Axis
        private MotionAxis _waferLifterZ; // 단일 리프터 축 (Y 혹은 Z)
        public MotionAxis WaferLifterZ => _waferLifterZ;
        #endregion

        #region ctor / Initialization
        public InputCassetteLifter(InputCassetteLifterConfig config = null) : base("InputCassetteLifterConfig")
        {
            InputCassetteLifterConfig = config ?? new InputCassetteLifterConfig();
            AddComponents();
        }

        public override void AddComponents()
        {
            InputCassetteLifterConfig.LoadAndBindAxes(Equipment.Instance.AxisManager);
            InputCassetteLifterConfig.InitializeDefaultTeachingPositions();
            TeachingPositions.Clear();
            foreach (var tp in InputCassetteLifterConfig.TeachingPositions)
                TeachingPositions.Add(tp);
            BindAxes();
        }
        #endregion

        #region Axis Binding / Helpers
        private void BindAxes()
        {
            var mgr = Equipment.Instance?.AxisManager;
            if (mgr == null)
            {
                Log.Write("InputCassetteLifter", "[BindAxes] AxisManager null");
                return;
            }
           
            const string unitName = "Unit"; // Equipment에서 축 등록 시 사용한 유닛명과 동일해야 함
            BindAxis(mgr, unitName, AxisNames.WaferLifterZ, ref _waferLifterZ);
        }

        public void MoveAxisOnce(MotionAxis ax, double target)
        {
            if (ax == null) return;
            if (System.Math.Abs(ax.GetPosition() - target) > ax.Config.InposTolerance * 3)
                ax.MoveAbs(target, ax.Config.MaxVelocity, ax.Config.RunAcc, ax.Config.RunDec, ax.Config.AccJerkPercent);
        }
        public bool InPos(MotionAxis ax, double target) => ax == null || ax.InPosition(target);
        public double GetTP(string tpName, string axisName)
        {
            var tp = InputCassetteLifterConfig.GetTeachingPosition(tpName);
            if (tp != null && tp.AxisPositions != null && tp.AxisPositions.TryGetValue(axisName, out var v)) return v;
            return 0.0;
        }
        #endregion

        #region Teaching Helpers
        public void TeachCurrentPosition(string positionName, string description = null)
        {
            var axisPositions = new Dictionary<string, double>();
            foreach (var axisPair in Axes)
                axisPositions[axisPair.Key] = axisPair.Value.GetPosition();
            var tp = new TeachingPosition(positionName, axisPositions, description);
            InputCassetteLifterConfig.SetTeachingPosition(tp);
        }

        public int MoveToTeachingPosition(string positionName, double vel = 5, double acc = 10, double dec = 10, double jerk = 50)
        {
            var tp = InputCassetteLifterConfig.GetTeachingPosition(positionName);
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
            var tp = InputCassetteLifterConfig.GetTeachingPosition(positionName);
            if (tp == null) return false;
            foreach (var kv in tp.AxisPositions)
                if (!Axes.TryGetValue(kv.Key, out var axis) || !InPos(axis, kv.Value)) return false;
            return true;
        }
        #endregion

        #region IO / Sensors
        public bool ReadInput(string name)
        {
            var hi = InputCassetteLifterConfig.HardInputs?.FirstOrDefault(i => i.Name.Equals(name, System.StringComparison.OrdinalIgnoreCase));
            if (hi == null) return false;
            var eq = Equipment.Instance; var dio = eq?.DioScan; if (dio == null) return false;
            foreach (var m in eq.UnitIO.Modules)
                if (dio.TryGetInput(m.ModuleName, hi.Disp, out var v)) return v;
            return false;
        }

        public bool CassettePresent0() => ReadInput(InputCassetteLifterConfig.IO.CASSETTE_CHECK0);
        public bool CassettePresent1() => ReadInput(InputCassetteLifterConfig.IO.CASSETTE_CHECK1);
        public bool AnyCassettePresent() => CassettePresent0() || CassettePresent1();
        public bool RingJut() => !ReadInput(InputCassetteLifterConfig.IO.RING_JUT_CHECK);
        public bool MappingSensor() => ReadInput(InputCassetteLifterConfig.IO.MAPPING_SENSOR);
        #endregion

        #region Lifecycle
        public override void OnRun()  => base.OnRun();
        public override void OnStop() => base.OnStop();
        #endregion
    }
}