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
        private int binNo = -1;
        private Dictionary<string, TestItemResult> items = new Dictionary<string, TestItemResult>();
        #endregion

        #region Properties
        public int BinNo => binNo;
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
            binNo = -1;
            items.Clear();
        }
        public void AddItem(string itemName)
        {
            items.Add(itemName, new TestItemResult());
        }
        public void ResetItems()
        {
            binNo = -1;
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
        public void SetBinNo(int binNo)
        {
            this.binNo = binNo;
        }
        #endregion
    }

    public class PKGTester : BaseComponent
    {
        #region Fields
        private KeithleySourcemeter sourcemeter;
        private CASSpectrometer spectrometer;
        private TestConditionSet conditionSet;

        private PKGTesterResult result = new PKGTesterResult();
        private bool isMeasuring = false;
        #endregion

        #region Properties
        public TestConditionSet ConditionSet { get => conditionSet; }
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
        public async Task<int> ManualMeasureAsync(int tryCount, int intervalDelay)
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

        #region Result Data Process
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
        private bool CalibrateDataProcess()
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
                        //if (item.UseGain)
                        //    value *= item.Gain;
                        //if (item.UseOffset)
                        //    value += item.Offset;

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
        private bool GetBinFromResult()
        {
            try
            {
                
                int binNo = 0;
                {
                    // Do this.
                }
                result.SetBinNo(binNo);
                return true;
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                return false;
            }
        }
        #endregion

        #region Run Measure Process
        private async Task<int> DoMeasure()
        {
            // 두 계측기의 시뮬레이션 측정을 비동기로 동시에 실행
            Task<int> spcTask = Task.Factory.StartNew(() => spectrometer.Measure());
            Task<int> smuTask = Task.Factory.StartNew(() => sourcemeter.Measure());

            try
            {
                ResetResultItem();
                int[] result = await Task.WhenAll(spcTask, smuTask);

                bool taskComplete = true;
                for (int i = 0; i < result.Length; i++)
                {
                    if (result[i] < 0)
                    {
                        taskComplete = false;
                        break;
                    }
                }

                // 두 계측기 중 하나라도 실패하면 예외 처리
                if (!taskComplete)
                {
                    throw new Exception("The measurement operation of the instrument was not completed normally.");
                }

                // Data process
                if (!GetResultProcess())
                {
                    throw new Exception("Failed to process data.");
                }
                if (!CalibrateDataProcess())
                {
                    throw new Exception("Failed to calibrate data.");
                }

                // Binning Data
                if (!GetBinFromResult())
                {
                    throw new Exception("Failed to bin from result data");
                }
                return 0;
            }
            catch (Exception ex)
            {
                ResetResultItem();
                Log.Write(ex);
                return -1;
            }
        }
        #endregion
        #endregion
    }
}
