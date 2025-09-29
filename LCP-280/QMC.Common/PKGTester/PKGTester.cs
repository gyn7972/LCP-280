using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using QMC.Common.Component;
using QMC.Common.Keithley;
using QMC.Common.Spectrometer;
using System.Text.RegularExpressions;

namespace QMC.Common.PKGTester
{
    public class PKGTesterResult
    {
        #region Fields
        private BinningResult binningResult = new BinningResult();
        private Dictionary<string, TestItemResult> items = new Dictionary<string, TestItemResult>();     
        #endregion

        #region Properties
        public BinningResult BinningResult => binningResult;
        public IReadOnlyDictionary<string, TestItemResult> Items => items;
        #endregion

        public PKGTesterResult Clone()
        {
            PKGTesterResult pKGTesterResult = new PKGTesterResult();
            


            return pKGTesterResult;
        }
        #region Constructor
        public PKGTesterResult()
        {
        }
        #endregion

        #region Methods
        public void ClearItems()
        {
            binningResult.Reset();
            items.Clear();
        }
        public void AddItem(string itemName)
        {
            items.Add(itemName, new TestItemResult());
        }
        public void ResetItems()
        {
            binningResult.Reset();
            foreach (var key in items.Keys)
            {
                items[key].Reset();
            }
        }
        public bool AssignItem(string itemName, TestItemResult result)
        {
            if (result == null)
                return false;
            if (items.ContainsKey(itemName) == false)
                return false;

            items[itemName].Assign(result);
            return true;
        }
        #endregion
    }

    public class PKGTester : BaseComponent
    {
        #region Fields
        private KeithleySourcemeter sourcemeter;
        private CASSpectrometer spectrometer;
        private TestConditionSet conditionSet;
        private BinningSpecSheet binningSpecSheet;

        private PKGTesterResult result = new PKGTesterResult();
        private bool isMeasuring = false;

        private BinningClassifier binningClassifier = new BinningClassifier();

        private DataTable evaluator = new DataTable();

        private System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
        private TimeSpan measureTime = TimeSpan.Zero;
        #endregion

        #region Properties
        public TestConditionSet ConditionSet { get => conditionSet; }
        public BinningSpecSheet BinningSpecSheet { get => binningSpecSheet; }
        public KeithleySourcemeter Sourcemeter { get => sourcemeter; }
        public CASSpectrometer Spectrometer { get => spectrometer; }
        public PKGTesterResult Result { get => result; }
        public bool IsMeasuring { get => isMeasuring; }
        public TimeSpan MeasureTime { get => measureTime; }
        #endregion

        #region Constructor / Initialize
        public PKGTester(string name) : base(name)
        {
            conditionSet = new TestConditionSet($"{name}_conditionSet");
        }

        public bool BindSourcemeter(KeithleySourcemeter sourcemeter)
        {
            if (sourcemeter == null)
                return false;

            this.sourcemeter = sourcemeter;
            return true;
        }
        public bool BindSpectrometer(CASSpectrometer spectrometer)
        {
            if (spectrometer == null)
                return false;
            this.spectrometer = spectrometer;
            return true;
        }
        #endregion

        #region Events
        public delegate void PKGTesterEventHandler(object sender);

        public event PKGTesterEventHandler OnConditionSetChanged;
        public event PKGTesterEventHandler OnMeasureCompleted;

        public event PKGTesterEventHandler OnManualMeasureCompleted;
        public event PKGTesterEventHandler OnMeasureAborted;
        #endregion

        #region Methods
        public async Task<int> MeasureAsync(int rotaryIndex)
        {
            try
            {
                isMeasuring = true;
                int ret = await DoMeasure(rotaryIndex);
                if (ret == 0)
                {
                    OnMeasureCompleted?.Invoke(this);
                    return ret;
                }
                else
                {
                    OnMeasureAborted?.Invoke(this);
                    return -1;
                }
            }
            catch (Exception)
            {
                // Error handling
                OnMeasureAborted?.Invoke(this);
                return -1;
            }
            finally
            {
                isMeasuring = false;
            }
        }

        public async Task<int> ManualMeasureAsync(int rotaryIndex)
        {
            try
            {
                isMeasuring = true;
                int ret = await DoMeasure(rotaryIndex);
                if (ret == 0)
                {
                    OnManualMeasureCompleted?.Invoke(this);
                    return ret;
                }
                else
                {
                    OnMeasureAborted?.Invoke(this);
                    return -1;
                }
            }
            catch (Exception)
            {
                // Error handling
                OnMeasureAborted?.Invoke(this);
                return -1;
            }
            finally
            {
                isMeasuring = false;
            }
        }

        public bool CanMeasure()
        {
            // Condition Set
            if (conditionSet == null || !conditionSet.Validate())
                return false;

            // Binning Spec Sheet
            if (binningSpecSheet == null || !binningSpecSheet.Validate())
                return false;

            // Check instruments bound
            if (sourcemeter == null || spectrometer == null)
                return false;

            // Check instruments ready
            if (!sourcemeter.IsReady() || !spectrometer.IsReady())
                return false;

            return true;
        }

        public int LoadTestConditionSet(TestConditionSet conditionSet)
        {
            if (conditionSet == null || !conditionSet.Validate())
                return -1;

            this.conditionSet.CopyConditionFrom(conditionSet);
            if (RebuildTestMechanism() != 0)
                return -1;

            OnConditionSetChanged?.Invoke(this);
            return 0;
        }

        public int LoadBinningSpecSheet(BinningSpecSheet specSheet)
        {
            if (specSheet == null)
                return -1;
            if (!specSheet.Validate())
                return -1;

            binningSpecSheet = new BinningSpecSheet();
            binningSpecSheet.CopyFrom(specSheet);
            binningClassifier.AssignSpecSheet(binningSpecSheet);
            return 0;
        }
        #endregion

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
                if (conditionSet == null)
                    throw new Exception("ConditionSet is not set.");
                if (sourcemeter == null || spectrometer == null)
                    throw new Exception("Instruments are not set.");

                sourcemeter.ClearTestItems();
                spectrometer.ClearTestItems();

                for (int i = 0; i < conditionSet.Items.Count; i++)
                {
                    var item = conditionSet.Items[i];
                    if (item == null)
                        throw new Exception("Invalid TestItem.");

                    switch (item.GetTestItemCategory())
                    {
                        case TestItemCategory.Electrical:
                            {
                                bool isOpticalSource = false;
                                if (i + 1 < conditionSet.Items.Count)
                                {
                                    var nextItem = conditionSet.Items[i + 1];
                                    if (nextItem != null)
                                    {
                                        if (nextItem.GetTestItemCategory() == TestItemCategory.Optical)
                                            isOpticalSource = true;
                                    }
                                }

                                if (!sourcemeter.AddTestItem(item, isOpticalSource))
                                    throw new Exception("Failed to add test item to sourcemeter.");
                            }
                            break;
                        case TestItemCategory.Optical:
                            {
                                if (!spectrometer.AddTestItem(item))
                                    throw new Exception("Failed to add test item to spectrometer.");
                            }
                            break;
                        case TestItemCategory.UserDefined:
                            {
                            }
                            break;
                        default:
                            throw new Exception("Undefined TestItemCategory.");
                    }
                }

                if (!sourcemeter.BuildTestCommands())
                    throw new Exception("Failed to build test commands for sourcemeter.");
                if (!spectrometer.BuildTestCommands())
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
                result.ClearItems();
                foreach (var item in conditionSet.Items)
                {
                    if (item == null)
                        throw new InvalidOperationException("Invalid TestItem");

                    result.AddItem(item.Name);
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                return -1;
            }
            return 0;
        }

        private void ResetResultItem()
        {
            result.ResetItems();
        }
        #endregion

        #region Internal Process
        private async Task<int> DoMeasure(int rotaryIndex)
        {
            stopWatch.Restart();

            TaskCompletionSource<bool> tcs = null;
            CASSpectrometer.DeviceEventHandler handler = null;

            Task<int> spcTask = null;
            Task<int> smuTask = null;

            try
            {
                ResetResultItem();

                // Check can measure
                if (!CanMeasure())
                    throw new InvalidOperationException("Cannot perform measurement. Check the condition set and instrument status.");

                // Spectrometer event for command sent
                tcs = new TaskCompletionSource<bool>();
                handler = (s) => { tcs.TrySetResult(true); };
                spectrometer.OnMeasureCommandSended += handler;

                var timeoutTask = Task.Delay(TimeSpan.FromSeconds(1));

                // Spectrometer task start
                spcTask = Task.Run(() => DoSpectrometerMeasure());

                // Wait spectrometer command sent
                if (HasTaskSpectrometer())
                {
                    var completedTask = await Task.WhenAny(tcs.Task, timeoutTask);
                    if (completedTask == timeoutTask)
                    {
                        throw new TimeoutException("Spectrometer send measurement command timed out.");
                    }
                    Thread.Sleep(10);
                }

                // Sourcemeter task start
                smuTask = Task.Run(() => DoSourcemeterMeasure());

                // Wait for both tasks to complete
                int[] result = await Task.WhenAll(spcTask, smuTask);
                if (result.Any(r => r != 0))
                {
                    throw new Exception("The measurement operation of the instrument was not completed normally.");
                }

                // Get Result Data From Instruments
                if (!GetResultProcess())
                {
                    throw new Exception("Failed to process data.");
                }

                // Calibrate Data
                if (!CalibrateDataProcess(rotaryIndex))
                {
                    throw new Exception("Failed to calibrate data.");
                }

                // Calculate User Define Item
                if (!CalulateUserDefineItem())
                {
                    throw new Exception("Failed to calculate user define item.");
                }

                // Binning Data
                if (!BinningDataProcess())
                {
                    throw new Exception("Failed to binning data");
                }
                return 0;
            }
            catch (Exception ex)
            {
                // Error handling
                ResetResultItem();

                Log.Write(this, ex.Message);
                return -1;
            }
            finally
            {
                spectrometer.OnMeasureCommandSended -= handler;

                if (spcTask != null)
                {
                    spcTask.Dispose();
                }
                if (smuTask != null)
                {
                    smuTask.Dispose();
                }

                stopWatch.Stop();
                measureTime = stopWatch.Elapsed;
            }
        }

        private bool HasTaskSourcemeter()
        {
            int smuCmdCount = conditionSet.Items.Count(item => item.GetTestItemCategory() == TestItemCategory.Electrical);
            return (smuCmdCount > 0);
        }

        private bool HasTaskSpectrometer()
        {
            int spcCmdCount = conditionSet.Items.Count(item => item.GetTestItemCategory() == TestItemCategory.Optical);
            return (spcCmdCount > 0);
        }

        private async Task<int> DoSourcemeterMeasure()
        {
            if (!HasTaskSourcemeter())
                return 0;

            return await Task.Run(() => sourcemeter.Measure());
        }

        private async Task<int> DoSpectrometerMeasure()
        {
            if (!HasTaskSpectrometer())
                return 0;

            return await Task.Run(() => spectrometer.Measure());
        }

        private bool GetResultProcess()
        {
            try
            {
                // Sourcemeter
                if (!sourcemeter.GetResultProcess())
                    throw new Exception("Failed to process result data from sourcemeter.");

                foreach (var key in sourcemeter.Results.Keys)
                {
                    if (!result.AssignItem(key, sourcemeter.Results[key]))
                        throw new Exception($"Failed to assign result item from sourcemeter. (key: {key})");
                }

                // Spectrometer
                if (!spectrometer.GetResultProcess())
                    throw new Exception("Failed to process result data from spectrometer.");

                foreach (var key in spectrometer.Results.Keys)
                {
                    if (!result.AssignItem(key, spectrometer.Results[key]))
                        throw new Exception($"Failed to assign result item from spectrometer. (key: {key})");
                }
                return true;
            }
            catch (Exception ex)
            {
                Log.Write(this, ex.Message);
                return false;
            }
        }

        private bool CalibrateDataProcess(int rotaryIndex)
        {
            try
            {
                foreach (var item in conditionSet.Items)
                {
                    if (item.IsMeasureItem())
                    {
                        TestItemResult itemResult = result.Items[item.Name];

                        // Calibrate
                        double value = itemResult.RawData;
                        if (item.UseGain[rotaryIndex])
                            value *= item.Gain[rotaryIndex];
                        if (item.UseOffset[rotaryIndex])
                            value += item.Offset[rotaryIndex];

                        itemResult.Value = value;
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Log.Write(this, ex.Message);
                return false;
            }
        }

        private bool CalulateUserDefineItem()
        {
            try
            { 
                foreach (var item in conditionSet.Items)
                {
                    if (item.IsComputeItem())
                    {
                        string expression = item.Expression;

                        List<string> assignItems = new List<string>();
                        foreach (var key in result.Items.Keys)
                        {
                            var pattern = $@"\b{Regex.Escape(key)}\b";
                            if (Regex.IsMatch(expression, pattern))
                            {
                                assignItems.Add(key);
                                var value = result.Items[key].Value;
                                expression = Regex.Replace(expression, pattern, value.ToString());
                            }
                        }

                        if (assignItems.Count > 1)
                        {
                            var firstUnit = result.Items[assignItems[0]].Unit;
                            foreach (var key in assignItems)
                            {
                                if (result.Items[key].Unit != firstUnit)
                                {
                                    // 단위가 다르면 예외
                                    throw new InvalidOperationException("Different units in expression.");
                                }
                            }
                        }

                        var computeObj = evaluator.Compute(expression, "");
                        double computedValue = Convert.ToDouble(computeObj);

                        TestItemResult itemResult = result.Items[item.Name];
                        itemResult.RawData = computedValue;
                        itemResult.Value = computedValue;
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Log.Write(this, ex.Message);
                return false;
            }
        }

        private bool BinningDataProcess()
        {
            try
            {
                BinningResult binningResult = binningClassifier.Classify(result.Items);
                if (binningResult.BinType == BinningType.None)
                    throw new Exception("Failed to classify data.");

                return result.BinningResult.CopyFrom(binningResult);
            }
            catch (Exception ex)
            {
                Log.Write(this, ex.Message);
                return false;
            }
        }
        #endregion
    }
}