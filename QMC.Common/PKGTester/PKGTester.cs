using NPOI.OpenXmlFormats.Dml;
using QMC.Common.Component;
using QMC.Common.Keithley;
using QMC.Common.Spectrometer;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

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
        public PKGTesterResult() { }
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
                items[key].Reset();
        }
        public bool AssignItem(string itemName, TestItemResult result)
        {
            if (result == null) return false;
            if (!items.ContainsKey(itemName)) return false;
            items[itemName].Assign(result);
            return true;
        }
        public PKGTesterResult Clone()
        {
            PKGTesterResult clone = new PKGTesterResult();
            clone.binningResult.CopyFrom(this.binningResult);
            clone.items = this.items.ToDictionary(entry => entry.Key, entry => entry.Value.Clone());
            return clone;
        }
        #endregion
    }

    public class PKGTester : BaseComponent
    {
        #region Fields
        private KeithleySourcemeter sourcemeter;
        private CASSpectrometer spectrometer;
        private TestConditionSet conditionSet = new TestConditionSet();

        // 기존: 직접 로드/저장 대상이었던 SpecSheet
        private BinningSpecSheet binningSpecSheet = new BinningSpecSheet();

        // [NEW] ExcelBinningModel 주 데이터 소스
        private ExcelBinningModel excelBinningModel = new ExcelBinningModel();
        private string excelBinningModelPath; // 마지막 로드/저장 경로 (옵션)

        private PKGTesterResult result = new PKGTesterResult();
        private bool isMeasuring = false;

        private BinningClassifier binningClassifier = new BinningClassifier();

        #region Helper (추가)
        /// <summary>현재 분류된 BinLabel 기준 Range 조회. NG이면 null.</summary>
        public IReadOnlyDictionary<string, BinningRange> GetCurrentBinRanges()
        {
            if (result == null) return null;
            var label = result.BinningResult != null ? result.BinningResult.BinLabel : null;
            if (string.IsNullOrWhiteSpace(label) || string.Equals(label, "NG", StringComparison.OrdinalIgnoreCase))
                return null;
            return binningClassifier.GetRangesForBin(label);
        }
        #endregion

        private DataTable evaluator = new DataTable();
        private System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
        private TimeSpan measureTime = TimeSpan.Zero;
        #endregion

        #region Properties
        public TestConditionSet ConditionSet => conditionSet;
        public BinningSpecSheet BinningSpecSheet => binningSpecSheet; // 내부 변환 결과
        public ExcelBinningModel ExcelBinningModel => excelBinningModel; // UI/파일 원본
        public string ExcelBinningModelPath => excelBinningModelPath;
        public KeithleySourcemeter Sourcemeter => sourcemeter;
        public CASSpectrometer Spectrometer => spectrometer;
        public PKGTesterResult Result => result;
        public bool IsMeasuring => isMeasuring;
        public TimeSpan MeasureTime => measureTime;
        #endregion

        #region Constructor / Initialize
        public PKGTester(string name) : base(name) { }

        public bool BindSourcemeter(KeithleySourcemeter smu)
        {
            if (smu == null) return false;
            sourcemeter = smu;
            return true;
        }

        public bool BindSpectrometer(CASSpectrometer spc)
        {
            if (spc == null) return false;
            spectrometer = spc;
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

        #region Load / Save (Spec 관련) - NEW
        /// <summary>
        /// Excel(.xlsx/.xls) 또는 BIN(.bin) 파일을 로드하여 ExcelBinningModel로 저장 후
        /// 내부 BinningSpecSheet + BinningClassifier에 변환 적용.
        /// </summary>
        public int LoadBinningModel(string filePath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(filePath) || !System.IO.File.Exists(filePath))
                    return -1;

                var ext = System.IO.Path.GetExtension(filePath).ToLowerInvariant();
                ExcelBinningModel loaded = null;

                if (ext == ".xlsx" || ext == ".xls")
                {
                    loaded = DataBinningExcelLoader.Load(filePath);
                }
                else if (ext == ".bin")
                {
                    loaded = DataBinningBinLoader.LoadBIN(filePath);
                }
                else
                {
                    // 호환 위해 기존 CSV 포맷(BinningSpecSheet) 시도
                    var legacy = new BinningSpecSheet();
                    if (legacy.LoadFromFile(filePath) == 0)
                    {
                        loaded = ExcelBinningModelConverter.ToExcelModel(legacy);
                    }
                }

                if (loaded == null)
                    return -1;

                excelBinningModel = loaded;
                excelBinningModelPath = filePath;

                // 변환 → 기존 검사 구조 유지
                var spec = ExcelBinningModelConverter.ToSpecSheet(excelBinningModel);
                if (!spec.Validate())
                    return -1;

                binningSpecSheet = new BinningSpecSheet();
                if (!binningSpecSheet.CopyFrom(spec))
                    return -1;

                binningClassifier.AssignSpecSheet(binningSpecSheet);
                return 0;
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                return -1;
            }
        }

        /// <summary>
        /// 현재 ExcelBinningModel을 지정 경로로 저장 (확장자에 따라 포맷 결정)
        /// </summary>
        public int SaveBinningModel(string filePath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(filePath))
                    return -1;

                var ext = System.IO.Path.GetExtension(filePath).ToLowerInvariant();
                int rc = -1;

                if (ext == ".xlsx" || ext == ".xls")
                    rc = DataBinningExcelLoader.Save(filePath, excelBinningModel);
                else if (ext == ".bin")
                    rc = DataBinningBinLoader.SaveBIN(filePath, excelBinningModel);
                else
                {
                    // CSV 등 레거시 포맷 요청 시 변환 후 BinningSpecSheet 저장 필요하면 구현
                    // 여기서는 미지원
                    return -1;
                }

                if (rc == 0)
                    excelBinningModelPath = filePath;

                return rc;
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                return -1;
            }
        }

        /// <summary>
        /// (하위호환) 기존 방식 호출 시 새 로더로 포워딩
        /// </summary>
        [Obsolete("Use LoadBinningModel instead.")]
        public int LoadBinningSpecSheet(string filePath)
        {
            return LoadBinningModel(filePath);
        }
        #endregion

        #region TestConditionSet Load (기존 유지)
        public int LoadTestConditionSet(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !System.IO.File.Exists(filePath))
                return -1;

            TestConditionSet tmp = new TestConditionSet();
            if (tmp.LoadFromFile(filePath) != 0)
                return -1;

            conditionSet.CopyFrom(tmp);
            if (RebuildTestMechanism() != 0)
                return -1;

            OnConditionSetChanged?.Invoke(this);
            return 0;
        }
        #endregion

        #region Measure Entry
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
                OnMeasureAborted?.Invoke(this);
                return -1;
            }
            catch
            {
                OnMeasureAborted?.Invoke(this);
                return -1;
            }
            finally { isMeasuring = false; }
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
                OnMeasureAborted?.Invoke(this);
                return -1;
            }
            catch
            {
                OnMeasureAborted?.Invoke(this);
                return -1;
            }
            finally { isMeasuring = false; }
        }

        public bool CanMeasure()
        {
            if (conditionSet == null || !conditionSet.Validate())
                return false;

            // ExcelBinningModel → SpecSheet 변환 결과 검사
            if (binningSpecSheet == null || !binningSpecSheet.Validate())
                return false;

            if (sourcemeter == null || spectrometer == null)
                return false;

            if (!sourcemeter.IsReady() || !spectrometer.IsReady())
                return false;

            return true;
        }
        #endregion

        #region Build Mechanism
        private int RebuildTestMechanism()
        {
            int ret = 0;
            if ((ret = BuildCommandItem()) != 0) return ret;
            if ((ret = BuildResultItem()) != 0) return ret;
            return ret;
        }

        private int BuildCommandItem() //여기서 명령어 만들어 낸다.
        {
            try
            {
                if (conditionSet == null) throw new Exception("ConditionSet is not set.");
                if (sourcemeter == null || spectrometer == null) throw new Exception("Instruments are not set.");

                sourcemeter.ClearTestItems();
                spectrometer.ClearTestItems();

                for (int i = 0; i < conditionSet.Items.Count; i++)
                {
                    var item = conditionSet.Items[i];
                    if (item == null) throw new Exception("Invalid TestItem.");

                    switch (item.GetTestItemCategory())
                    {
                        case TestItemCategory.Electrical:
                            bool isOpticalSource = false;
                            if (i + 1 < conditionSet.Items.Count)
                            {
                                var nextItem = conditionSet.Items[i + 1];
                                if (nextItem != null && nextItem.GetTestItemCategory() == TestItemCategory.Optical)
                                    isOpticalSource = true;
                            }
                            if (!sourcemeter.AddTestItem(item, isOpticalSource))
                                throw new Exception("Failed to add test item to sourcemeter.");
                            break;
                        case TestItemCategory.Optical:
                            if (!spectrometer.AddTestItem(item))
                                throw new Exception("Failed to add test item to spectrometer.");
                            break;
                        case TestItemCategory.UserDefined:
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
                    if (item == null) throw new InvalidOperationException("Invalid TestItem");
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
            Task<int> smuTotal = null;

            try
            {
                ResetResultItem();
                if (!CanMeasure())
                {
                    Log.Write("PKGTester", "Cannot perform measurement. Check the condition set and instrument status.");
                    throw new InvalidOperationException("Cannot perform measurement.");
                }

                tcs = new TaskCompletionSource<bool>();
                handler = (s) => { tcs.TrySetResult(true); };
                spectrometer.OnMeasureCommandSended += handler;

                try
                {
                    if (string.IsNullOrEmpty(Thread.CurrentThread.Name))
                        Thread.CurrentThread.Name = "DoSourcemeterMeasure";
                }
                catch
                { }
                smuTask = Task.Run(() => DoSourcemeterMeasure());

                Thread.Sleep(10);

                try
                {
                    if (string.IsNullOrEmpty(Thread.CurrentThread.Name))
                        Thread.CurrentThread.Name = "DoSpectrometerMeasure";
                }
                catch
                { }
                spcTask = Task.Run(() => DoSpectrometerMeasure());
                var timeoutTask = Task.Delay(2000);
                if (HasTaskSpectrometer())
                {
                    var completed = await Task.WhenAny(tcs.Task, timeoutTask);
                    if (completed == timeoutTask)
                    {
                        Log.Write("PKGTester", "Spectrometer send measurement command timed out.");
                        return -1;
                    }
                    Thread.Sleep(10);
                }

                

                //여기 오면 측정은 완료.
                int[] codes = await Task.WhenAll(spcTask, smuTask);
                if (codes.Any(r => r != 0))
                {
                    Log.Write("PKGTester", "DoMeasure", "Measurement failed in one of the instruments.");
                    return -1;
                }

                stopWatch.Stop();
                measureTime = stopWatch.Elapsed;

                // 순차로 진행. // Script 해보고 안되면 이거다...
                //try
                //{
                //    if (string.IsNullOrEmpty(Thread.CurrentThread.Name))
                //        Thread.CurrentThread.Name = "DoMeasureTotal";
                //}
                //catch
                //{ }
                //smuTotal = Task.Run(() => DoMeasureTotal());
                //int[] codes = await Task.WhenAll(smuTotal);
                //if (codes.Any(r => r != 0))
                //{
                //    Log.Write("PKGTester", "DoMeasure", "Measurement failed in one of the instruments.");
                //    throw new Exception("Instrument measure incomplete.");
                //}

                if (!GetResultProcess())
                {
                    Log.Write("PKGTester", "DoMeasure", "Failed to process measurement results.");
                    throw new Exception("Failed to process data.");
                }

                if (!CalibrateDataProcess(rotaryIndex))
                {
                    Log.Write("PKGTester", "DoMeasure", "Failed to calibrate measurement data.");
                    throw new Exception("Failed to calibrate data.");
                }

                if (!CalulateUserDefineItem())
                {
                    Log.Write("PKGTester", "DoMeasure", "Failed to calculate user defined items.");
                    throw new Exception("Failed to calculate user define item.");
                }

                if (!BinningDataProcess())
                {
                    Log.Write("PKGTester", "DoMeasure", "Failed to binning measurement data.");
                    throw new Exception("Failed to binning data.");
                }

                return 0;
            }
            catch (Exception ex)
            {
                //TEST로 막아둠.
                ResetResultItem();
                Log.Write("PKGTester", ex.Message);
                return -1;
            }
            finally
            {
                spectrometer.OnMeasureCommandSended -= handler;
                spcTask?.Dispose();
                smuTask?.Dispose();
                smuTotal?.Dispose();
                stopWatch.Stop();
                measureTime = stopWatch.Elapsed;
            }
        }

        private bool HasTaskSourcemeter()
        {
            int smuCmdCount = conditionSet.Items.Count(item => item.GetTestItemCategory() == TestItemCategory.Electrical);
            return smuCmdCount > 0;
        }

        private bool HasTaskSpectrometer()
        {
            int spcCmdCount = conditionSet.Items.Count(item => item.GetTestItemCategory() == TestItemCategory.Optical);
            return spcCmdCount > 0;
        }

        private async Task<int> DoSourcemeterMeasure()
        {
            if (!HasTaskSourcemeter()) return 0;
            return await Task.Run(() => sourcemeter.Measure());
        }

        private async Task<int> DoSpectrometerMeasure()
        {
            if (!HasTaskSpectrometer()) return 0;
            return await Task.Run(() => spectrometer.Measure());
        }

        private async Task<int> DoMeasureTotal()
        {
            if (!HasTaskSourcemeter()) return 0;
            if (!HasTaskSpectrometer()) return 0;

            return await Task.Run(() => DoMeasureTotalAsync());
        }


        private System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch(); // Timeout check
        public int DoMeasureTotalAsync()
        {
            try
            {
                KeithleySourcemeterChannel channel = sourcemeter.Channels["smua"];
                channel.ClearMeasureData();
                if (Config.IsSimulation)
                {
                    channel.SimulateBufferData();
                    // 시뮬레이션 시 분광기도 최소 호출 (예외 무시)
                    try { spectrometer.Measure(); } catch { }
                    return 0;
                }

                // 1) 컨택저항
                if (channel.HasContactRCommand())
                {
                    if (!channel.RunContactRCommand())
                    {
                        Log.Write("PKGTester", "DoMeasureTotalAsync", "Contact resistance measurement failed.");
                        return -1;
                    }
                }

                // measure
                if (!channel.RunCommands())
                {
                    Log.Write("PKGTester", "DoMeasureTotalAsync", "RunCommands failed");
                    return -1;
                }


                // 2.1) VF 안정화 대기 (설정 기반, 비차단)
                // 예: ConditionSet 또는 Spectrometer Config에서 가져오도록 변경 가능
                int vfStabilizeMs = 3000; // TODO: 설정으로 치환
                WaitByTime(vfStabilizeMs, pollMs: 2);
                // 또는 await Task.Delay(vfStabilizeMs);


                //TackTime 문제 발생. - 장비 시작전에 셋팅.
                // 3) CAS 파라미터 적용 + 다크 커런트(필요 시)
                // 분광기 설정 정책에 따라 하나를 선택
                //int rcParam = spectrometer.ApplyParameter();
                //if (rcParam != 0)
                //{
                //    Log.Write("PKGTester", "DoMeasureTotalAsync", "ApplyParameter failed rc=" + rcParam);
                //    return -1;
                //}
                // 필요 시: -> 웨이퍼 교체 시 한 번씩 하자.
                // int rcDark = spectrometer.MeasureDarkCurrent();
                // if (rcDark != 0)
                // {
                // Log.Write("PKGTester", "DoMeasureTotalAsync", "MeasureDarkCurrent failed rc=" + rcDark);
                // return -1;
                // }

                // 3) CAS 측정 (이 시점에는 VF 안정됨)
                int casCode = spectrometer.Measure();
                if (casCode != 0)
                {
                    Log.Write("PKGTester", "DoMeasureTotalAsync", "Spectrometer.Measure failed rc=" + casCode);
                    return -1;
                }

                // 5) SMU 완료 대기 + 타임아웃
                stopwatch.Restart();
                while (!channel.WaitComplete())
                {
                    if (stopwatch.ElapsedMilliseconds > sourcemeter.Config.MeasureTimeout)
                    {
                        Log.Write("PKGTester", "DoMeasureTotalAsync", "Timeout waiting SMU channel complete");
                        return -1;
                    }
                    Thread.Sleep(10);
                }

                if (!channel.ReadBufferData())
                {
                    Log.Write("PKGTester", "DoMeasureTotalAsync", "ReadBufferData failed");
                    return -1;
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                //OnMeasureFailed?.Invoke(this, ex.Message);
                return -1;
            }
            finally
            {
                stopwatch.Stop();
            }
            return 0;
        }

        private void WaitByTime(int vfStabilizeMs = 10, int pollMs = 2)
        {
            // 입력 방어
            if (vfStabilizeMs <= 0)
                return;
            if (pollMs < 1)
                pollMs = 1;

            var sw = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                // 지정 시간까지 간단한 폴링 대기
                while (sw.ElapsedMilliseconds < vfStabilizeMs)
                {
                    Thread.Sleep(pollMs);
                }
            }
            finally
            {
                sw.Stop();
            }
        }

        private bool GetResultProcess()
        {
            try
            {
                if (!sourcemeter.GetResultProcess())
                    throw new Exception("Failed sourcemeter result process.");
                foreach (var key in sourcemeter.Results.Keys)
                    if (!result.AssignItem(key, sourcemeter.Results[key]))
                        throw new Exception($"Assign fail (smu key:{key})");

                if (!spectrometer.GetResultProcess())
                    throw new Exception("Failed spectrometer result process.");
                foreach (var key in spectrometer.Results.Keys)
                    if (!result.AssignItem(key, spectrometer.Results[key]))
                        throw new Exception($"Assign fail (spc key:{key})");

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
                        var itemResult = result.Items[item.Name];
                        double value = itemResult.RawData;

                        // 1) Total Gain/Offset 먼저 적용
                        if (item.UseTotalGain)
                            value *= item.TotalGain;
                        if (item.UseTotalOffset)
                            value += item.TotalOffset;

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
                            foreach (var k in assignItems)
                            {
                                if (result.Items[k].Unit != firstUnit)
                                    throw new InvalidOperationException("Different units in expression.");
                            }
                        }

                        var computeObj = evaluator.Compute(expression, "");
                        double computedValue = Convert.ToDouble(computeObj);
                        var itemResult = result.Items[item.Name];
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
                binningClassifier.IsSimulation = false;

                var binningResult = binningClassifier.Classify(result.Items);
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