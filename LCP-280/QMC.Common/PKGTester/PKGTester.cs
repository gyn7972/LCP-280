using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using QMC.Common.Component;
using QMC.Common.Keithley;
using QMC.Common.Spectrometer;

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
        #endregion

        #region Properties
        public TestConditionSet ConditionSet { get => conditionSet; }
        public BinningSpecSheet BinningSpecSheet { get => binningSpecSheet; }
        public KeithleySourcemeter Sourcemeter { get => sourcemeter; }
        public CASSpectrometer Spectrometer { get => spectrometer; }
        public PKGTesterResult Result { get => result; }
        public bool IsMeasuring { get => isMeasuring; }
        #endregion

        #region Constructor
        public PKGTester(string name, KeithleySourcemeter sourcemeter, CASSpectrometer spectrometer) : base(name)
        {
            conditionSet = new TestConditionSet($"{name}_conditionSet");
            this.sourcemeter = sourcemeter;
            this.spectrometer = spectrometer;
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
        public async Task<int> MeasureAsync()
        {
            try
            {
                isMeasuring = true;
                int ret = await DoMeasure();
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
        public async Task<int> ManualMeasureAsync()
        {
            try
            {
                isMeasuring = true;
                int ret = await DoMeasure();
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

        public int LoadTestConditionSet(TestConditionSet conditionSet)
        {
            if (conditionSet == null)
                return -1;
            if (!conditionSet.Validate())
                return -1;

            this.conditionSet.CopyConditionFrom(conditionSet);
            RebuildTestMechanism();
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

                sourcemeter.ClearTestItems();
                spectrometer.ClearTestItems();

                foreach (var item in conditionSet.Items)
                {
                    if (item == null)
                        throw new Exception("Invalid TestItem.");

                    switch (item.GetTestItemCategory())
                    {
                        case TestItemCategory.Electrical:
                        case TestItemCategory.ElectricalSource:
                            {
                                if (!sourcemeter.AddTestItem(item))
                                    throw new Exception("Failed to add test item to sourcemeter.");
                            }
                            break;
                        case TestItemCategory.Optical:
                            {
                                if (!spectrometer.AddTestItem(item))
                                    throw new Exception("Failed to add test item to spectrometer.");
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

                    if (item.IsMeasureItem())
                    {
                        result.AddItem(item.Name);
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
        private void ResetResultItem()
        {
            result.ResetItems();
        }
        #endregion

        #region Internal Process
        private async Task<int> DoMeasure()
        {
            // 두 계측기의 시뮬레이션 측정을 비동기로 동시에 실행
            Task<int> spcTask = Task.Run(() => DoSpectrometerMeasure());
            //if (spectrometer.IsReady == false)
            //{
            Thread.Sleep(100);
            //    spectrometer.IsReady = true;
            //}
            Task<int> smuTask = Task.Run(() => DoSourcemeterMeasure());

            try
            {
                ResetResultItem();
                int[] result = await Task.WhenAll(spcTask, smuTask);

                // 두 계측기 중 하나라도 실패하면 예외 처리
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
                int rotaryIndex = 0;
                if (!CalibrateDataProcess(rotaryIndex))
                {
                    throw new Exception("Failed to calibrate data.");
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
                ResetResultItem();
                Log.Write(ex);
                return -1;
            }
            finally
            {
                spcTask.Dispose();
                smuTask.Dispose();
            }
        }
        private async Task<int> DoSourcemeterMeasure()
        {
            if (sourcemeter == null)
                return -1;
            int smuCmdCount = conditionSet.Items.Count(item => item.GetTestItemCategory() == TestItemCategory.Electrical || item.GetTestItemCategory() == TestItemCategory.ElectricalSource);
            if (smuCmdCount == 0)
                return 0;
            return await Task.Run(() => sourcemeter.Measure());
        }
        private async Task<int> DoSpectrometerMeasure()
        {
            if (spectrometer == null)
                return -1;
            int spcCmdCount = conditionSet.Items.Count(item => item.GetTestItemCategory() == TestItemCategory.Optical);
            if (spcCmdCount == 0)
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
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                return false;
            }
            return true;
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
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                return false;
            }
            return true;
        }
        private bool BinningDataProcess()
        {
            try
            {
                BinningResult binningResult = binningClassifier.Classify(result.Items);
                if (binningResult.BinType == BinningType.None)
                    throw new Exception("Failed to classify data.");

                result.BinningResult.CopyFrom(binningResult);
                return true;
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                return false;
            }
        }
        #endregion
        #endregion
    }
}
