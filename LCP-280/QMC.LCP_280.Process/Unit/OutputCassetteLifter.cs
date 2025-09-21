using QMC.Common;
using QMC.Common.Alarm;
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
    /// OutputCassetteLifter Unit
    ///  - Z 축 리프팅 Teaching Position
    ///  - Cassette / RingJut / Mapping 센서 상태 제공
    ///  - OutputStage 와 유사한 구조 (Axis / IO / Teaching / Lifecycle)
    /// </summary>
    public class OutputCassetteLifter : BaseUnit<OutputCassetteLifterConfig>
    {
        public enum AlarmKeys
        {
            eBinProtrusionDetected = 5001,
        }

        #region InitAlarm
        protected override void InitAlarm()
        {
            base.InitAlarm();
            base.InitAlarm();
            AlarmInfo alarm = new AlarmInfo();
            alarm.Code = (int)AlarmKeys.eBinProtrusionDetected;
            alarm.Title = "돌출 감지 센서가 감지 되었습니다.";
            alarm.Cause = "카세트 맵핑 하는데 돌출 감지 센서가 감지 되었습니다.\n 카세트를 점검 하고 다시 시작 하십시요.";
            alarm.Source = this.UnitName;
            alarm.Grade = AlarmInfo.AlarmType.Warning.ToString();
            m_dicAlarms.Add(alarm.Code, alarm);
            //AlarmRegister((int)AlarmKeys.eBinProtrusionDetected,
            //                "Bin Protrusion Detected",
            //                "Bin protrusion detected by sensor during operation. Please check and clear the obstruction before retrying.",
            //                "Error");

        }
        #endregion

        public OutputFeeder OutputFeeder { get; private set; }

        public OutputStage OutputStage { get; private set; }

        #region Axis
        private MotionAxis _axZ;
        public MotionAxis AxisZ => _axZ;
        #endregion

        #region ctor / Initialization
        public OutputCassetteLifter(OutputCassetteLifterConfig config = null) : base(new OutputCassetteLifterConfig())
        {
            
            AddComponents();
        }

        public override void AddComponents()
        {
            Config.LoadAndBindAxes(Equipment.Instance.AxisManager);
            Config.InitializeDefaultTeachingPositions();
            
            BindAxes();
        }
        #endregion

        protected override void OnBindUnit()
        {
            base.OnBindUnit();

            OutputFeeder = Equipment.Instance.GetUnit("OutputFeeder") as OutputFeeder;
            OutputStage = Equipment.Instance.GetUnit("OutputStage") as OutputStage;
        }


        #region Axis Binding / Helpers
        private void BindAxes()
        {
            var mgr = Equipment.Instance?.AxisManager;
            if (mgr == null)
            {
                Log.Write("OutputCassetteLifter", "[BindAxes] AxisManager null");
                return;
            }

            const string unitName = "Unit"; // Equipment에서 축 등록 시 사용한 유닛명과 동일해야 함
            BindAxis(mgr, unitName, AxisNames.BinLifterZ, ref _axZ);
        }

        public bool IsBinReadyForLoading()
        {
            return true;// this.IsBinReadyForloading;
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
            var tp = Config.GetTeachingPosition(tpName);
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
            Config.SetTeachingPosition(tp);
        }
        public int MoveToTeachingPosition(string positionName, double vel = 5, double acc = 10, double dec = 10, double jerk = 50)
        {
            var tp = Config.GetTeachingPosition(positionName);
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
            var tp = Config.GetTeachingPosition(positionName);
            if (tp == null) return false;
            double z = tp.AxisPositions.TryGetValue("Bin Lifter Z Axis", out var vz) ? vz : 0;
            return InPos(_axZ, z);
        }
        #endregion

        #region IO / Sensors
        public bool ReadInput(string name)
        {
            var hi = Config.HardInputs?.FirstOrDefault(i => i.Name.Equals(name, System.StringComparison.OrdinalIgnoreCase));
            if (hi == null) return false;
            var eq = Equipment.Instance; var dio = eq?.DioScan; if (dio == null) return false;
            foreach (var m in eq.UnitIO.Modules)
                if (dio.TryGetInput(m.ModuleName, hi.Disp, out var v)) return v;
            return false;
        }


        public bool CassettePresent0() => ReadInput(OutputCassetteLifterConfig.IO.CASSETTE_CHECK0);
        public bool CassettePresent1() => ReadInput(OutputCassetteLifterConfig.IO.CASSETTE_CHECK1);
        public bool AnyCassettePresent() => CassettePresent0() || CassettePresent1();
        public bool RingJut() => !ReadInput(OutputCassetteLifterConfig.IO.RING_JUT_CHECK);
        public bool MappingSensor() => ReadInput(OutputCassetteLifterConfig.IO.MAPPING_SENSOR);
        #endregion

        //public bool IsBinProtrusionDetectionSensor()
        //{
        //    bool sensorState = ReadInput(OutputCassetteLifterConfig.IO.BIN_PROTRUSION_DETECTION_SENSOR);
        //    return !sensorState;
        //}

        #region Lifecycle
        public override int OnRun()  { int ret = 0; return ret; }
        public override int OnStop() { int ret = 0; base.OnStop(); return ret; }
        protected override int OnRunReady() { return 0; }
        protected override int OnRunWork() { return 0; }
        protected override int OnRunComplete() { return 0; }
        #endregion

        #region Seq 단위 동작 함수
        public int CassetteLoading()
        {
            int nRet = -1;
            /* TODO */
            return nRet;
        }

        public int BinMapping()
        {
            int nRet = -1;
            /* TODO */
            return nRet;
        }

        public int BinLoading()
        {
            int nRet = -1;
            /* TODO */
            return nRet;
        }

        public int BinUnloading()
        {
            int nRet = -1;
            /* TODO */
            return nRet;
        }

        public int CassetteUnloading()
        {
            int nRet = -1;
            /* TODO */
            return nRet;
        }
        #endregion
    }
}