using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using QMC.Common.Component;
using QMC.Common.Keithley;
using QMC.Common.Spectrometer;

namespace QMC.Common.PKGTester
{
    public class PKGTester : BaseComponent
    {
        #region Fields
        private KeithleySourcemeter sourcemeter;
        private CASSpectrometer spectrometer;
        private TestConditionSet conditionSet;

        private Dictionary<string, TestItemResult> results = new Dictionary<string, TestItemResult>();
        #endregion

        #region Properties
        public TestConditionSet ConditionSet { get => conditionSet; }
        public KeithleySourcemeter Sourcemeter { get => sourcemeter; }
        public CASSpectrometer Spectrometer { get => spectrometer; }
        public IReadOnlyDictionary<string, TestItemResult> Results => results;
        #endregion

        #region Constructor
        public PKGTester(string name) : base(name)
        {
            conditionSet = new TestConditionSet($"{name}_conditionSet");
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
                int ret = await DoMeasure();
                if (ret >= 0)
                {
                    OnMeasureCompleted?.Invoke(this);
                    return ret;
                }
                else
                {
                    return -1;
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                return -1;
            }
        }
        public async Task<int> ManualMeasureAsync(int tryCount, int intervalDelay)
        { 
            return await DoManualMeasure(tryCount, intervalDelay);
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

        #region Attach Instrument
        public int AttachSourcemeter(KeithleySourcemeter sourcemeter)
        {
            if (sourcemeter == null)
                return -1;
            if (this.sourcemeter == sourcemeter)
                return 0;
            this.sourcemeter = sourcemeter;
            return 0;
        }
        public int AttachSpectrometer(CASSpectrometer spectrometer)
        {
            if (spectrometer == null)
                return -1;
            if (this.spectrometer == spectrometer)
                return 0;
            this.spectrometer = spectrometer;
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
                    throw new InvalidOperationException("ConditionSet is not set.");

                sourcemeter.ClearTestItems();
                spectrometer.ClearTestItems();

                foreach (var item in conditionSet.Items)
                {
                    if (item == null)
                        throw new InvalidOperationException("Invalid TestItem.");

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
                results.Clear();
                foreach (var item in conditionSet.Items)
                {
                    if (item == null)
                        throw new InvalidOperationException("Invalid TestItem");

                    if (item.IsMeasureItem())
                    {
                        results.Add(item.Name, new TestItemResult());
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
            foreach (var key in results.Keys)
            {
                results[key].Reset();
            }
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
                    results[key].Assign(sourcemeter.Results[key]);
                }

                // Spectrometer
                if (!spectrometer.GetResultProcess())
                    throw new Exception("Failed to process result data from spectrometer.");
                
                foreach (var key in spectrometer.Results.Keys)
                {
                    results[key].Assign(spectrometer.Results[key]);
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
                int binNo = GetBinFromResult();
                if (binNo < 0)
                {
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
                ResetResultItem();
                Log.Write(ex);
                return -1;
            }
        }
        private async Task<int> DoManualMeasure(int tryCount, int intervalDelay)
        {
            if (tryCount <= 0)
                return -1;

            int ret = -1;
            for (int i = 0; i < tryCount; i++)
            {
                ret = await DoMeasure();
                if (ret >= 0)
                {
                    OnManualMeasureCompleted?.Invoke(this);
                }
                else
                {
                    OnMeasureAborted?.Invoke(this);
                    break;
                }
                await Task.Delay(intervalDelay);
            }
            return ret;
        }
        #endregion
        #endregion
    }
}
