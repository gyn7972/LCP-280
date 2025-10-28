using QMC.Common;
using QMC.Common.Alarm;
using QMC.Common.Component;
using QMC.Common.Keithley;
using QMC.Common.Motions;
using QMC.Common.PKGTester;
using QMC.Common.Spectrometer;
using QMC.Common.Unit;
using QMC.LCP_280.Process.Component;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization.Advanced;
using static QMC.LCP_280.Process.Equipment;

namespace QMC.LCP_280.Process.Unit
{
    public sealed class IndexChipProber : BaseUnit<IndexChipProberConfig>, IDisposable
    {
        public enum AlarmKeys
        { 
            eNotReadyToMeasure = 99990, // 임시 알람 번호
        }

        #region InitAlarm
        protected override void InitAlarm()
        {
            base.InitAlarm();
            AlarmInfo alarm = new AlarmInfo();
            alarm.Code = (int)AlarmKeys.eNotReadyToMeasure;
            alarm.Title = "측정 준비가 되지 않았습니다.";
            alarm.Cause = "1. 적용된 Test Condition Set가 있는지 확인하여 주십시오. 2. 계측기가 정상적으로 Initialize 되어 있는지 확인하여 주십시오.";
            alarm.Source = this.UnitName;
            alarm.Grade = AlarmInfo.AlarmType.Warning.ToString();
            m_dicAlarms.Add(alarm.Code, alarm);
        }
        #endregion

        #region Config / Teaching
        public IndexChipProberConfig IndexChipProberConfig => Config;
        #endregion

        #region Bind Unit
        Rotary Rotary { get; set; }
        IndexChipProbeController IndexChipProbeController { get; set; }

        protected override void OnBindUnit()
        {
            base.OnBindUnit();
            Rotary = Equipment.Instance.GetUnit(UnitKeys.Rotary) as Rotary;
            IndexChipProbeController = Equipment.Instance.GetUnit(UnitKeys.IndexChipProbeController) as IndexChipProbeController;
        }
        #endregion

        #region Components
        private PKGTester tester = Equipment.Instance.Tester;
        #endregion

        #region ctor / Initialization
        public IndexChipProber(IndexChipProberConfig config = null)
            : base(config ?? new IndexChipProberConfig())
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

        #region Lifecycle
        public override int OnRun()
        {
            int ret = 0;
            if (this.RunUnitStatus == UnitStatus.Stopped ||
                this.RunUnitStatus == UnitStatus.Stopping ||
                this.RunUnitStatus == UnitStatus.CycleStop)
            {
                this.State = ProcessState.Stop;
                ret = -1;
            }
            if (this.RunUnitStatus == UnitStatus.Running)
            {
                return 0;
            }
            if (ret != 0)
            {
                this.State = ProcessState.Stop;
                this.OnStop();
            }
            return ret;
        }
        protected override int OnStart()
        {
            return base.OnStart();
        }
        public override int OnStop()
        {
            int ret = 0;
            this.RunUnitStatus = UnitStatus.Stopped;
            this.State = ProcessState.Stop;

            base.OnStop();
            return ret;
        }
        protected override int OnRunReady() { return 0; }
        protected override int OnRunWork() { return 0; }
        protected override int OnRunComplete() { return 0; }
        #endregion

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

        #region Axis Helpers
        private readonly List<MotionAxis> _boundAxes = new List<MotionAxis>();
        public IReadOnlyList<MotionAxis> BoundAxes => _boundAxes;        

        private void BindAxes()
        {
            _boundAxes.Clear();
            foreach (var kv in Axes) _boundAxes.Add(kv.Value);
        }
        //public double GetTP(string tpName, string axisName)
        //{
        //    var tp = Config.GetTeachingPosition(tpName);
        //    if (tp != null && tp.AxisPositions != null && tp.AxisPositions.TryGetValue(axisName, out var v)) return v;
        //    return 0.0;
        //}
        //public bool InPos(MotionAxis ax, double target) => ax == null || ax.InPosition(target);
        #endregion

        #region IO Helpers
        public bool ReadInput(string name)
        {
            // No HardInputs defined currently.
            return false;
        }
        public bool WriteOutput(string name, bool on)
        {
            // No outputs defined.
            return false;
        }
        #endregion

        #region Seq signal
        public bool RequestChipInsp { get; set; }
        public bool InspectDone { get; set; }
        #endregion

        protected override void OnMakeSequence()
        {
            base.OnMakeSequence();
            this.SequencePlayers.Add(MeasureChip);
        }

        #region Seq 단위 동작 함수
        /// <summary>
        /// LED PKG 측정
        /// 순서: 측정 -> 결과를 Material Object에 Assign
        /// </summary>
        public int MeasureChip(bool bFineSpeed = false)
        {
            int bRet = 0;
            if (RunMode == UnitRunMode.Manual)
            {
                this.CurrentFunc = MeasureChip;
            }
            try
            {
                LogSequence("Start");
                //int nIndex = Rotary.GetLoadIndexNo();
                int nIndex = IndexChipProbeController.GetProbeIndexNo();

                // 1) Check Can Measure
                InspectDone = false;

                if(Config.IsSimulation == false
                && Config.IsDryRun == false )
                {
                    if (!tester.CanMeasure())
                    {
                        PostAlarm((int)AlarmKeys.eNotReadyToMeasure);
                        Log.Write(this, "PKG Tester: Not ready to measure.");
                        return -1;
                    }

                    // 2) Measure Chip
                    bRet &= Measure();
                    if (bRet != 0)
                    {
                        Log.Write(UnitName, "Measure() Fail");
                        return -1;
                    }
                }
                
                MaterialDie die = this.Rotary.GetProbeSocketMaterial();
                if(die.Presence == Material.MaterialPresence.Exist)
                {
                    die.TesterResult = tester.Result;
                    die.ProcessSatate = Material.MaterialProcessSatate.Processing;
                }

                // 3) Data Assign
                bRet &= AssignDataToMaterialObject();
                if (bRet != 0)
                    return -1;

                InspectDone = true;
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                return -1;
            }
            finally
            {
                LogSequence("End");
            }

            return bRet;
        }

        private int Measure()
        {
            int rotaryIndex = GetProbeIndexNo();

            Task<int> task = tester.MeasureAsync(rotaryIndex);
            while (!IsEndTask(task))
            {
                Thread.Sleep(1);
            }
            return task.Result;
        }

        private int AssignDataToMaterialObject()
        {
            // Do Something...
            PKGTesterResult result = tester.Result;

            // 임시 테스트 코드 -----
            string logDir = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
            if (!System.IO.Directory.Exists(logDir))
                System.IO.Directory.CreateDirectory(logDir);

            string logFile = System.IO.Path.Combine(logDir, $"PKGTesterResult_{DateTime.Now:yyyyMMdd}.csv");

            bool fileExists = System.IO.File.Exists(logFile);

            using (var writer = new System.IO.StreamWriter(logFile, true, System.Text.Encoding.UTF8))
            {
                // 파일이 없으면 헤더 추가
                if (!fileExists)
                {
                    writer.Write("Timestamp,");
                    foreach (var item in result.Items)
                    {
                        writer.Write($",{item.Key}");
                    }
                    writer.WriteLine();
                }

                // 데이터 행 추가
                writer.Write($"{DateTime.Now:yyyy-MM-dd HH:mm:ss},");
                foreach (var item in result.Items)
                {
                    writer.Write($",{item.Value}");
                }
                writer.WriteLine();
            }
            // ---------------------
            return 0;
        }

        public int GetProbeIndexNo()
        {
            int nIndex = 0;
            if (Rotary == null) return nIndex;
            nIndex = (Rotary.GetLoadIndexNo() + this.Config.IndexOfProbe) % Rotary.GetIndexCount();
            return nIndex;
        }

        private void LogSequence(string log)
        {
            if (RunMode == UnitRunMode.Manual)
            {
                if (this.CurrentFunc == null)
                    return;

                Log.Write(UnitName, this.CurrentFunc.Method.Name, $"[Sequence] {log}");
            }
        }
        #endregion
    }
}