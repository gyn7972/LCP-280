using QMC.Common;
using QMC.Common.Keithley;
using QMC.Common.Motions;
using QMC.Common.PKGTester;
using QMC.Common.Spectrometer;
using QMC.Common.Unit;
using QMC.LCP_280.Process.Component;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization.Advanced;

namespace QMC.LCP_280.Process.Unit
{
    public class IndexChipProber : BaseUnit
    {
        public IndexChipProberConfig IndexChipProberConfig { get; private set; }
        public List<TeachingPosition> TeachingPositions { get; private set; } = new List<TeachingPosition>();

        // 측정 관련 ================================================
        private TestConditionSet conditionSet;
        private Dictionary<string, TestItemResult> results = new Dictionary<string, TestItemResult>();

        public TestConditionSet ConditionSet
        {
            get => conditionSet;
            set
            {
                if (value == null)
                    throw new ArgumentNullException("conditionSet");

                conditionSet = value;
                RebuildTestMechanism();
            }
        }
        public KeithleySourcemeter Sourcemeter => Equipment.Instance.Sourcemeter;
        public CASSpectrometer Spectrometer => Equipment.Instance.Spectrometer;
        public IReadOnlyDictionary<string, TestItemResult> Results => results;
        // =========================================================

        public IndexChipProber(IndexChipProberConfig config = null)
            : base("IndexChipProberConfig")
        {
            IndexChipProberConfig = config ?? new IndexChipProberConfig();
            AddComponents();
        }

        public override void AddComponents()
        {
            IndexChipProberConfig.LoadAndBindAxes(Equipment.Instance.AxisManager);
            IndexChipProberConfig.InitializeDefaultTeachingPositions();
            TeachingPositions.Clear();
            foreach (var tp in IndexChipProberConfig.TeachingPositions)
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
            IndexChipProberConfig.SetTeachingPosition(tp);
        }

        public int MoveToTeachingPosition(string positionName, double vel = 5, double acc = 10, double dec = 10, double jerk = 50)
        {
            var tp = IndexChipProberConfig.GetTeachingPosition(positionName);
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
        // Prober config currently defines no hard-coded axis names → provide generic list binding
        private readonly List<MotionAxis> _boundAxes = new List<MotionAxis>();
        public IReadOnlyList<MotionAxis> BoundAxes => _boundAxes;
        private void BindAxes()
        {
            _boundAxes.Clear();
            foreach (var kv in Axes) _boundAxes.Add(kv.Value);
        }
        public double GetTP(string tpName, string axisName)
        {
            var tp = IndexChipProberConfig.GetTeachingPosition(tpName);
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
            // Config has no HardInputs defined (commented). Keep structure for future expansion.
            var hiArray = (IndexChipProberConfig as dynamic); // placeholder; returns none
            return false;
        }
        public bool WriteOutput(string name, bool on)
        {
            // No outputs defined.
            return false;
        }
        #endregion

        // 측정 관련 ================================================
        /// <summary>
        /// 측정 동작을 진행합니다.
        /// </summary>
        /// <returns>성공하면 0 이상의 BinNo가 반환됩니다. 실패하면 음수(오류코드)가 반환됩니다.</returns>
        public int Measure()
        {
            try
            {
                results.Clear();
                Task<int> task = DoMeasure();
                return task.Result;
            }
            catch (Exception ex)
            {
                results.Clear();
                Log.Write(ex);
                return -1;
            }
        }

        #region Build Mechanism
        private int RebuildTestMechanism()
        {
            int ret = 0;
            if ((ret = BuildCommandItem()) != 0)
                return ret;
            if ((ret = BuildResultItem()) != 0)
                return ret;
            return ret;
        }
        private int BuildCommandItem()
        {
            try
            {
                if (ConditionSet == null)
                    throw new InvalidOperationException("ConditionSet is not set.");

                Sourcemeter.ClearTestItems();
                Spectrometer.ClearTestItems();

                foreach (var item in ConditionSet.Items)
                {
                    if (item == null)
                        throw new InvalidOperationException("Invalid TestItem.");

                    switch (item.Type.GetCategory())
                    {
                        case TestItemCategory.Electrical:
                            if (!Sourcemeter.AddTestItem(item))
                                throw new Exception("Failed to add test item to sourcemeter.");
                            break;
                        case TestItemCategory.Optical:
                            if (!Spectrometer.AddTestItem(item))
                                throw new Exception("Failed to add test item to spectrometer.");
                            break;
                        default:
                            throw new Exception("Undefined TestItemCategory.");
                    }
                }

                if (!Sourcemeter.BuildTestCommands())
                    throw new Exception("Failed to build test commands for sourcemeter.");
                if (!Spectrometer.BuildTestCommands())
                    throw new Exception("Failed to build test commands for spectrometer.");
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                return -1;
            }
            return 0;
        }
        private int BuildResultItem()
        {
            try
            {
                results.Clear();
                foreach (var item in conditionSet.Items)
                {
                    if (item == null)
                        throw new InvalidOperationException("Invalid TestItem");

                    switch (item.Type.GetCategory())
                    {
                        case TestItemCategory.Electrical:
                        case TestItemCategory.Optical:
                            results.Add(item.Name, new TestItemResult());
                            break;
                        default:
                            throw new Exception("Undefined TestItemCategory.");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                return -1;
            }
            return 0;
        }
        #endregion

        #region Result Data Process
        private bool GetResultProcess()
        {
            try
            {
                if (Sourcemeter.GetResultProcess() || Spectrometer.GetResultProcess())
                    throw new Exception("");

                // Sourcemeter
                foreach (var key in Sourcemeter.Results.Keys)
                {
                    results[key].Assign(Sourcemeter.Results[key]);
                }
                // Spectrometer
                foreach (var key in Spectrometer.Results.Keys)
                {
                    results[key].Assign(Spectrometer.Results[key]);
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                return false;
            }
            return true;
        }
        private bool CalibrateDataProcess()
        {
            try
            {
                foreach (var item in conditionSet.Items)
                {
                    TestItemResult itemResult = results[item.Name];

                    // Calibrate
                    double value = itemResult.RawData;
                    //if (item.UseGain)
                    //    value *= item.Gain;
                    //if (item.UseOffset)
                    //    value += item.Offset;

                    itemResult.Value = value;
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                return false;
            }
            return true;
        }
        private int GetBinFromResult()
        {
            try
            {
                // Do this.
                return 0;
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                return -1;
            }
        }
        #endregion

        #region Run Measure Process
        private async Task<int> DoSpectrometerMeasure()
        {
            return await Task.Run(() => Spectrometer.Measure());
        }
        private async Task<int> DoSourcemeterMeasure()
        {
            return await Task.Run(() => Sourcemeter.Measure());
        }
        private async Task<int> DoMeasure()
        {
            Task<int> spcTask = DoSpectrometerMeasure();
            Task<int> smuTask = DoSourcemeterMeasure();

            try
            {
                await Task.WhenAll(spcTask, smuTask);

                int spcRet = await spcTask;
                int smuRet = await smuTask;

                // Check instrument work result.
                if (spcRet != 0 || smuRet != 0)
                {
                    throw new Exception("The measurement operation of the instrument was not completed normally.");
                }

                // Data process
                if (GetResultProcess())
                {
                    throw new Exception("Failed to process data.");
                }
                if (CalibrateDataProcess())
                {
                    throw new Exception("Failed to calibrate data.");
                }

                // Binning Data
                int binNo = GetBinFromResult();
                if (binNo < 0)
                {
                    // Failed to binning data...
                    throw new Exception("Failed to bin from result data");
                }
                else if (binNo == 0)
                {
                    // Ng bin
                    return binNo;
                }
                else
                {
                    // Good bin
                    return binNo;
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                return -1;
            }
        }
        #endregion
        // =========================================================
    }
}