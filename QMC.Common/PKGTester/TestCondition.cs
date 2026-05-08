using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Globalization;

namespace QMC.Common.PKGTester
{
    public class TestConditionItem// : BaseConfig
    {
        #region Properties
        // Defines
        public string Name { get; set; }
        public TestItemType Type { get; set; }

        // Source
        public double SourceValue { get; set; }
        public double SourceTime { get; set; }
        public double WaitTime { get; set; }
        public double OffTime { get; set; }

        // Measure
        public double MeasureTime { get; set; } //NplcTime
        public double MeasureLow { get; set; }  //Limit 임.
        public double MeasureHigh { get; set; }  //Limit 임.
        // AppRange를 동적으로 구성
        public string AppRange
        {
            get
            {
                string unit = GetMeasureUnitFromType();
                return $"{MeasureLow} - {MeasureHigh}({unit})";
            }
            set { } // JSON 역직렬화 시 에러 방지용 빈 setter
        }

        public double MeasureLimit { get; set; }
        // User Define
        public string Expression { get; set; }

        // Data Calibration
        public bool UseTotalGain { get; set; }
        public bool UseTotalOffset { get; set; }
        public double TotalGain { get; set; }
        public double TotalOffset { get; set; }
        public string Unit { get; set; }

        // Data Calibration
        public bool[] UseGain { get; set; } = new bool[8];
        public bool[] UseOffset { get; set; } = new bool[8];
        public double[] Gain { get; set; } = new double[8];
        public double[] Offset { get; set; } = new double[8];

        // TestConditionItem 클래스 Properties 영역에 추가
        public int KeyChNo { get; set; }
        public int OpenCheckFlag { get; set; }
        public int SourceRange { get; set; }
        public int MeasureRange { get; set; }
        public int MeasureUnit { get; set; }

        public bool Optical { get; set; }
        public int OpticDCUsed { get; set; }
        public int NGSkip { get; set; }

        public int WaveCount { get; set; }
        public int ExposeCount { get; set; }
        public int IvRange { get; set; }
        public int Polarity { get; set; }

        public double FullRangeMin { get; set; }
        public double FullRangeMax { get; set; }
        public double SecondRangeMin { get; set; }
        public double SecondRangeMax { get; set; }

        public int IntegrationTime { get; set; }
        public string ItemName2 { get; set; }

        // OpticItemRaw/Pd/R3
        public Dictionary<string, It2MetricSpec> OpticMetrics { get; set; } = new Dictionary<string, It2MetricSpec>(StringComparer.OrdinalIgnoreCase);

        // WaveItemWP/WH/WX/WY/WD/WPU/CCT/CIE/CRI/CRI9/2ndP/3ndP
        public Dictionary<string, It2MetricSpec> WaveMetrics { get; set; } = new Dictionary<string, It2MetricSpec>(StringComparer.OrdinalIgnoreCase);

        public int LegacyItemCode { get; set; }
        #endregion

        #region Constructor
        public TestConditionItem(string name)// : base(name)
        {
            Name = name;
            Reset();
        }
        #endregion

        #region Methods
        public /*override*/ void Reset()
        {
            Type = TestItemType.None;
            SourceValue = 0;
            SourceTime = 0;
            WaitTime = 0;
            OffTime = 0;
            
            MeasureTime = 0;
            MeasureLow = 0;
            MeasureHigh = 0;
            AppRange = "0.0 - 0.0(단위)";

            MeasureLimit = 0;
            Expression = "";

            // Total calibration defaults
            UseTotalGain = false;
            UseTotalOffset = false;
            TotalGain = 1.0;
            TotalOffset = 0.0;

            Unit = "";

            for (int i = 0; i < 8; i++)
            {
                UseGain[i] = false;
                UseOffset[i] = false;
                Gain[i] = 1;
                Offset[i] = 0;
            }

            // TestConditionItem 클래스 Reset 메서드 수정
            KeyChNo = 0;
            OpenCheckFlag = 0;
            SourceRange = 0;
            MeasureRange = 0;
            MeasureUnit = 0;

            Optical = false;
            OpticDCUsed = 0;
            NGSkip = 0;

            WaveCount = 0;
            ExposeCount = 0;
            IvRange = 0;
            Polarity = 0;

            FullRangeMin = 0;
            FullRangeMax = 0;
            SecondRangeMin = 0;
            SecondRangeMax = 0;

            IntegrationTime = 0;
            ItemName2 = "";
            
            OpticMetrics.Clear();
            WaveMetrics.Clear();

            // PropertyView 편집 대상 기본 생성
            GetOrCreateWaveMetric("WP", "WP");
            GetOrCreateWaveMetric("CRI", "CRI");
            GetOrCreateWaveMetric("CCT", "CCT");

            LegacyItemCode = -1;
        }
        public /*override*/ bool Validate()
        {
            if (string.IsNullOrWhiteSpace(Name))
                return false;

            // Unit에 따른 값 검증 로직 추가
            if (!ValidateUnit())
            {
                return false;
            }

            switch (GetTestItemCategory())
            { 
                case TestItemCategory.Optical:
                    {
                        // Optical Item
                        if (MeasureLow > MeasureHigh)
                            return false;
                    }
                    break;
                case TestItemCategory.Electrical:
                    {
                        // Electrical Item
                        if (SourceValue < 0)
                            return false;
                        if (SourceTime <= 0)
                            return false;
                        if (WaitTime < 0)
                            return false;
                        if (OffTime < 0)
                            return false;
                        if (MeasureLimit < 0)
                            return false;
                        if (MeasureTime <= 0)
                            return false;
                        if (MeasureLow > MeasureHigh)
                            return false;
                    }
                    break;
                case TestItemCategory.UserDefined:
                    {
                        // User Define Item
                        if (string.IsNullOrWhiteSpace(Expression))
                            return false;
                        if (MeasureLow > MeasureHigh)
                            return false;
                    }
                    break;
                default:
                    return false;
            }
            return true;
        }

        // 단위를 강제로 설정할 수 있는 메서드 추가
        public void SetUnit(string unit)
        {
            this.Unit = unit;
        }

        // 단위에 따라 입력값들이 적절한지 확인하는 메서드 추가
        private bool ValidateUnit()
        {
            if (string.IsNullOrEmpty(Unit))
                return true;

            // 예시: 단위별로 값의 범위를 체크하거나 단위 문자열 자체를 검증
            // 실제 장비 스펙에 맞춰 구체적인 확인 구문을 작성하실 수 있습니다.
            switch (Unit)
            {
                case "uA":
                case "mA":
                case "A":
                    // 전류 단위일 때의 검증 로직 (예: 과전류 입력 방지 등)
                    // if (SourceValue > SomeLimit) return false;
                    break;
                case "mV":
                case "V":
                    // 전압 단위일 때의 검증 로직
                    break;
            }

            return true;
        }

        public /*override*/ PropertyCollection GetPropertyCollection()
        {
            PropertyCollection pc = new PropertyCollection();
            
            // Title
            string title = $"Item Config";
            pc.Add(title);

            // Value
            pc.Add("Name", "", Name);
            pc.Add("Type", "", Type);

            // Source
            switch (GetTestItemCategory())
            {
                case TestItemCategory.Electrical:
                    {
                        pc.Add("Source");
                        pc.Add("App Value", GetSourceUnitFromType(), SourceValue);
                        pc.Add("Apply Time", "ms", SourceTime);
                        pc.Add("Wait Time", "ms", WaitTime);
                        pc.Add("Off Time", "ms", OffTime);
                    }
                    break;
            }

            // Measure
            switch (GetTestItemCategory())
            {
                case TestItemCategory.Electrical:
                    {
                        pc.Add("Measure");
                        pc.Add("Measure Time", "ms", MeasureTime);
                        //pc.Add("Measure Limit", GetMeasureUnitFromType(), MeasureLimit);
                        pc.Add("Measure Low", GetMeasureUnitFromType(), MeasureLow);
                        pc.Add("Measure High", GetMeasureUnitFromType(), MeasureHigh);
                    }
                    break;
                case TestItemCategory.Optical:
                    {
                        pc.Add("Measure");
                        pc.Add("Measure Low", GetMeasureUnitFromType(), MeasureLow);
                        pc.Add("Measure High", GetMeasureUnitFromType(), MeasureHigh);

                        var wp = GetOrCreateWaveMetric("WP", "WP");
                        var cri = GetOrCreateWaveMetric("CRI", "CRI");
                        var cct = GetOrCreateWaveMetric("CCT", "CCT");

                        pc.Add("Wave Metrics");
                        pc.Add("WP Use", "", wp.Use);
                        pc.Add("WP Low", "", wp.LowLevel);
                        pc.Add("WP High", "", wp.HighLevel);

                        pc.Add("CRI Use", "", cri.Use);
                        pc.Add("CRI Low", "", cri.LowLevel);
                        pc.Add("CRI High", "", cri.HighLevel);

                        pc.Add("CCT Use", "", cct.Use);
                        pc.Add("CCT Low", "", cct.LowLevel);
                        pc.Add("CCT High", "", cct.HighLevel);
                    }
                    break;
                case TestItemCategory.UserDefined:
                    {
                        pc.Add("Measure");
                        pc.Add("Measure Low", GetMeasureUnitFromType(), MeasureLow);
                        pc.Add("Measure High", GetMeasureUnitFromType(), MeasureHigh);
                    }
                    break;
            }

            // Expression
            switch (GetTestItemCategory())
            {
                case TestItemCategory.UserDefined:
                    {
                        pc.Add("Expression");
                        pc.Add("Expression", "", Expression);
                    }
                    break;
            }

            // Calibration
            switch (GetTestItemCategory())
            {
                case TestItemCategory.Electrical:
                case TestItemCategory.Optical:
                    {
                        // Total calibration first
                        pc.Add("Calibration - Total");
                        pc.Add("Use Total Gain", "", UseTotalGain);
                        pc.Add("Use Total Offset", "", UseTotalOffset);
                        pc.Add("Total Gain", "", TotalGain);
                        pc.Add("Total Offset", "", TotalOffset);

                        pc.Add("Calibration - Use Gain");
                        for (int i = 0; i < UseGain.Length; i++)
                        {
                            pc.Add($"UseGain #{i + 1}", "", UseGain[i]);
                        }

                        pc.Add("Calibration - Use Offset");
                        for (int i = 0; i < UseOffset.Length; i++)
                        {
                            pc.Add($"UseOffset #{i + 1}", "", UseOffset[i]);
                        }

                        pc.Add("Calibration - Gain");
                        for (int i = 0; i < Gain.Length; i++)
                        {
                            pc.Add($"Gain #{i + 1}", "", Gain[i]);
                        }

                        pc.Add("Calibration - Offset");
                        for (int i = 0; i < Offset.Length; i++)
                        {
                            pc.Add($"Offset #{i + 1}", "", Offset[i]);
                        }
                    }
                    break;
            }
            return pc;
        }
        public /*override*/ int ApplyValueFromPropertyCollection(PropertyCollection pc)
        {
            if (pc == null)
                return -1;
            try
            {
                var legacyItemCode = LegacyItemCode;
                var optical = Optical;

                Name = "";
                Reset();

                LegacyItemCode = legacyItemCode;
                Optical = optical;

                Name = pc.GetValue<string>("Name");
                Type = pc.GetValue<TestItemType>("Type");

                // Source
                switch (GetTestItemCategory())
                {
                    case TestItemCategory.Electrical:
                        {
                            SourceValue = pc.GetValue<double>("App Value");
                            SourceTime = pc.GetValue<double>("Apply Time");
                            WaitTime = pc.GetValue<double>("Wait Time");
                            OffTime = pc.GetValue<double>("Off Time");
                        }
                        break;
                }

                // Measure
                switch (GetTestItemCategory())
                {
                    case TestItemCategory.Electrical:
                        {
                            MeasureTime = pc.GetValue<double>("Measure Time");
                            //MeasureLimit = pc.GetValue<double>("Measure Limit");
                            MeasureLow = pc.GetValue<double>("Measure Low");
                            MeasureHigh = pc.GetValue<double>("Measure High");
                        }
                        break;
                    case TestItemCategory.Optical:
                        {
                            MeasureLow = pc.GetValue<double>("Measure Low");
                            MeasureHigh = pc.GetValue<double>("Measure High");

                            var wp = GetOrCreateWaveMetric("WP", "WP");
                            wp.Use = pc.GetValue<bool>("WP Use");
                            wp.LowLevel = pc.GetValue<double>("WP Low");
                            wp.HighLevel = pc.GetValue<double>("WP High");

                            var cri = GetOrCreateWaveMetric("CRI", "CRI");
                            cri.Use = pc.GetValue<bool>("CRI Use");
                            cri.LowLevel = pc.GetValue<double>("CRI Low");
                            cri.HighLevel = pc.GetValue<double>("CRI High");

                            var cct = GetOrCreateWaveMetric("CCT", "CCT");
                            cct.Use = pc.GetValue<bool>("CCT Use");
                            cct.LowLevel = pc.GetValue<double>("CCT Low");
                            cct.HighLevel = pc.GetValue<double>("CCT High");
                        }
                        break;
                    case TestItemCategory.UserDefined:
                        {
                            MeasureLow = pc.GetValue<double>("Measure Low");
                            MeasureHigh = pc.GetValue<double>("Measure High");
                        }
                        break;
                }

                // Expression
                switch (GetTestItemCategory())
                {
                    case TestItemCategory.UserDefined:
                        {
                            Expression = pc.GetValue<string>("Expression");
                        }
                        break;
                }

                // Calibration
                switch (GetTestItemCategory())
                {
                    case TestItemCategory.Electrical:
                    case TestItemCategory.Optical:
                        {
                            // Total calibration
                            UseTotalGain = pc.GetValue<bool>("Use Total Gain");
                            UseTotalOffset = pc.GetValue<bool>("Use Total Offset");
                            TotalGain = pc.GetValue<double>("Total Gain");
                            TotalOffset = pc.GetValue<double>("Total Offset");

                            // Optical Item
                            for (int i = 0; i < 8; i++)
                            {
                                UseGain[i] = pc.GetValue<bool>($"UseGain #{i + 1}");
                                UseOffset[i] = pc.GetValue<bool>($"UseOffset #{i + 1}");
                                Gain[i] = pc.GetValue<double>($"Gain #{i + 1}");
                                Offset[i] = pc.GetValue<double>($"Offset #{i + 1}");
                            }
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                return -1;
            }
            return 0;
        }

        public TestConditionItem Clone()
        {
            TestConditionItem clone = new TestConditionItem(this.Name);
            clone.ApplyValueFromPropertyCollection(this.GetPropertyCollection());
            return clone;
        }
        public TestItemCategory GetTestItemCategory()
        {
            return Type.GetCategory();
        }
        public bool IsMeasureItem()
        {
            switch (GetTestItemCategory())
            {
                case TestItemCategory.Electrical:
                case TestItemCategory.Optical:
                    return true;
            }
            return false;
        }
        public bool IsComputeItem()
        {
            switch (GetTestItemCategory())
            {
                case TestItemCategory.UserDefined:
                    return true;
            }
            return false;
        }
        private string GetSourceUnitFromType()
        {
            // Unit이 이미 설정되어 있다면(코드에서 변경 등) 그 값을 우선 사용
            //if (!string.IsNullOrEmpty(Unit))
            //    return Unit;

            switch (Type)
            {
                case TestItemType.VF:
                    {
                        if (Name == "VF1" || Name == "VF5")
                        {
                            Unit = "uA";
                            return "uA";
                        }
                        Unit = "mA";
                        return "mA";
                    }
                    
                case TestItemType.VR:
                    Unit = "uA";
                    return "uA";
                case TestItemType.IF:
                    Unit = "mV";
                    return "mV";
                case TestItemType.IR:
                    Unit = "V";
                    return "V";
                case TestItemType.KELFS:
                    Unit = "uA";
                    return "uA";
                case TestItemType.KELDG:
                    Unit = "uA";
                    return "uA";
                default:
                    Unit = "";
                    return "";
            }
        }
        private string GetMeasureUnitFromType()
        {
            // Unit이 이미 설정되어 있다면(코드에서 변경 등) 그 값을 우선 사용
            //if (!string.IsNullOrEmpty(Unit))
            //    return Unit;

            switch (Type)
            {
                case TestItemType.VF:
                    {
                        if (Name == "VF1" || Name == "VF5")
                        {
                            Unit = "uA";
                            return "uA";
                        }
                        Unit = "mA";
                        return "mA";
                    }
                case TestItemType.VR:
                    Unit = "uA";
                    return "uA";
                case TestItemType.IF:
                    Unit = "mV";
                    return "mV";
                case TestItemType.IR:
                    Unit = "V";
                    return "V";
                case TestItemType.KELFS:
                    Unit = "uA";
                    return "uA";
                case TestItemType.KELDG:
                    Unit = "uA";
                    return "uA";
                default:
                    Unit = "";
                    return "";
            }
        }

        private It2MetricSpec GetOrCreateWaveMetric(string key, string defaultName)
        {
            if (WaveMetrics == null)
                WaveMetrics = new Dictionary<string, It2MetricSpec>(StringComparer.OrdinalIgnoreCase);

            It2MetricSpec metric;
            if (!WaveMetrics.TryGetValue(key, out metric) || metric == null)
            {
                metric = new It2MetricSpec
                {
                    Name = defaultName,
                    Use = false,
                    LowLevel = 0d,
                    HighLevel = 0d
                };
                WaveMetrics[key] = metric;
            }
            else if (string.IsNullOrWhiteSpace(metric.Name))
            {
                metric.Name = defaultName;
            }

            return metric;
        }

        #endregion
    }

    public class TestConditionSet
    {
        #region Fields
        private List<TestConditionItem> items = new List<TestConditionItem>();
        private static DataTable evaluator = new DataTable();
        #endregion

        #region Proerties
        public IReadOnlyList<TestConditionItem> Items => items.AsReadOnly();

        private Dictionary<string, string> lastIt2RawMap = null;
        public int ContactOP { get; private set; } = 0;
        public List<It2ContactRule> ContactRules { get; private set; } = new List<It2ContactRule>();
        #endregion

        #region Constructor
        public TestConditionSet() : base()
        {
            items = new List<TestConditionItem>();
        }
        #endregion

        #region Edit Item
        public int AddItem(TestConditionItem item)
        {
            if (item == null)
                return -1;
            //if (items.Any(x => x.Name == item.Name))
            //    return -1;

            items.Add(item);
            return 0;
        }
        public int InsertItem(int index, TestConditionItem item)
        {
            if (item == null)
                return -1;
            if (items.Any(x => x.Name == item.Name))
                return -1;
            if (index < 0 || index > items.Count)
                return -1;

            items.Insert(index, item);
            return 0;
        }   
        public int RemoveItemAt(int index)
        {
            if (index < 0 || index >= items.Count)
                return -1;

            items.RemoveAt(index);
            return 0;
        }
        private bool SwapItems(int index1, int index2)
        {
            if (index1 < 0 || index1 >= items.Count || index2 < 0 || index2 >= items.Count)
                return false;
            if (index1 == index2)
                return true;

            var temp = items[index1];
            items[index1] = items[index2];
            items[index2] = temp;
            return true;
        }
        public int MoveItemUp(int index)
        {
            if (index <= 0 || index >= items.Count)
                return -1;
            if (index == 0)
                return 0;
            
            if (!SwapItems(index, index - 1))
                return -1;

            return 0;
        }
        public int MoveItemDown(int index)
        {
            if (index < 0 || index >= items.Count - 1)
                return -1;
            if (index == items.Count - 1)
                return 0;

            if (!SwapItems(index, index + 1))
                return -1;

            return 0;
        }
        public int ClearItems()
        {
            items.Clear();
            return 0;
        }
        public int CopyFrom(TestConditionSet testConditionSet)
        {
            if (testConditionSet == null)
                return -1;

            items.Clear();
            items.AddRange(testConditionSet.Items);
            return 0;
        }
        #endregion

        #region File Methods
        // 래퍼 클래스 정의
        class TestConditionSetData
        {
            public List<TestConditionItem> Items { get; set; }
        }

        public int LoadFromFile(string filePath)
        {
            try
            {
                if (!System.IO.File.Exists(filePath))
                    return -1;

                string ext = System.IO.Path.GetExtension(filePath);
                if (string.Equals(ext, ".it2", StringComparison.OrdinalIgnoreCase))
                    return LoadFromIt2(filePath);

                return LoadFromJson(filePath);
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                return -1;
            }
        }

        public int SaveToFile(string filePath)
        {
            try
            {
                string ext = System.IO.Path.GetExtension(filePath);
                if (string.Equals(ext, ".it2", StringComparison.OrdinalIgnoreCase))
                    return SaveToIt2(filePath);

                return SaveToJson(filePath);
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                return -1;
            }
        }

        private int LoadFromJson(string filePath)
        {
            try
            {
                var json = System.IO.File.ReadAllText(filePath, Encoding.UTF8);
                var settings = new Newtonsoft.Json.JsonSerializerSettings();
                settings.Converters.Add(new StringEnumConverter());
                var data = Newtonsoft.Json.JsonConvert.DeserializeObject<TestConditionSetData>(json, settings);
                if (data == null || data.Items == null)
                    return -1;

                items.Clear();
                items.AddRange(data.Items);
                lastIt2RawMap = null;
                return 0;
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                return -1;
            }
        }

        private int SaveToJson(string filePath)
        {
            try
            {
                var data = new TestConditionSetData
                {
                    Items = this.items.ToList()
                };

                var settings = new Newtonsoft.Json.JsonSerializerSettings();
                settings.Converters.Add(new StringEnumConverter());
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(data, Newtonsoft.Json.Formatting.Indented, settings);
                System.IO.File.WriteAllText(filePath, json, Encoding.UTF8);
                return 0;
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                return -1;
            }
        }

        private int LoadFromIt2(string filePath)
        {
            try
            {
                var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                var lines = System.IO.File.ReadAllLines(filePath, Encoding.Default);

                foreach (var raw in lines)
                {
                    if (string.IsNullOrWhiteSpace(raw))
                        continue;
                    if (raw.StartsWith("["))
                        continue;

                    int eqIndex = raw.IndexOf('=');
                    if (eqIndex <= 0)
                        continue;

                    string key = raw.Substring(0, eqIndex).Trim();
                    string value = raw.Substring(eqIndex + 1);
                    map[key] = value;
                }

                int itemCount = ParseInt(GetValue(map, "ItemCount"), -1);
                if (itemCount <= 0)
                    return -1;

                var loaded = new List<TestConditionItem>();
                for (int i = 0; i < itemCount; i++)
                {
                    string name = GetIndexedValue(map, "ItemName", i);
                    if (string.IsNullOrWhiteSpace(name))
                        name = "Item" + (i + 1).ToString(CultureInfo.InvariantCulture);

                    var item = new TestConditionItem(name.Trim());

                    // LoadFromIt2() item 루프에서 type/optical 세팅 순서 보강(가독성/안전성)
                    string legacyType = GetIndexedValue(map, "Item", i);
                    string opticalFlag = GetIndexedValue(map, "Optical", i);

                    item.LegacyItemCode = ParseInt(legacyType, -1);
                    item.Optical = ParseBool01(opticalFlag, false);
                    item.Type = ParseLegacyType(legacyType, item.Name, opticalFlag);

                    item.SourceValue = ParseDouble(GetIndexedValue(map, "SourceValue", i), 0d);
                    item.SourceTime = ParseDouble(GetIndexedValue(map, "SreDelay", i), 0d);
                    item.WaitTime = ParseDouble(GetIndexedValue(map, "WaitTime", i), 0d);
                    item.OffTime = ParseDouble(GetIndexedValue(map, "OffTime", i), 0d);

                    item.MeasureTime = ParseDouble(GetIndexedValue(map, "NplcTime", i), 0d);
                    item.MeasureLow = ParseDouble(GetIndexedValue(map, "MeasureLow", i), 0d);
                    item.MeasureHigh = ParseDouble(GetIndexedValue(map, "MeasureHigh", i), 0d);
                    item.MeasureLimit = ParseDouble(GetIndexedValue(map, "MeasureLimit", i), 0d);

                    item.Unit = (GetIndexedValue(map, "StrSourceUnit", i) ?? string.Empty).Trim();

                    // LoadFromIt2() 의 item 루프 내부 loaded.Add(item); 직전에 추가
                    item.KeyChNo = ParseInt(GetIndexedValue(map, "KeyChNo", i), 0);
                    item.OpenCheckFlag = ParseInt(GetIndexedValue(map, "OpenCheckFlag", i), 0);
                    item.SourceRange = ParseInt(GetIndexedValue(map, "SourceRange", i), 0);
                    item.MeasureRange = ParseInt(GetIndexedValue(map, "MeasureRange", i), 0);
                    item.MeasureUnit = ParseInt(GetIndexedValue(map, "MeasureUnit", i), 0);

                    item.ItemName2 = (GetIndexedValue(map, "ItemName2", i) ?? string.Empty).TrimEnd();
                    item.Optical = ParseBool01(GetIndexedValue(map, "Optical", i), false);
                    item.OpticDCUsed = ParseInt(GetIndexedValue(map, "OpticDCUsed", i), 0);
                    item.NGSkip = ParseInt(GetIndexedValue(map, "NGSkip", i), 0);

                    item.WaveCount = ParseInt(GetIndexedValue(map, "WaveCount", i), 0);
                    item.ExposeCount = ParseInt(GetIndexedValue(map, "ExposeCount", i), 0);
                    item.IvRange = ParseInt(GetIndexedValue(map, "IvRange", i), 0);
                    item.Polarity = ParseInt(GetIndexedValue(map, "Polarity", i), 0);

                    item.FullRangeMin = ParseDouble(GetIndexedValue(map, "Full RangeMin", i), 0d);
                    item.FullRangeMax = ParseDouble(GetIndexedValue(map, "Full RangeMax", i), 0d);
                    item.SecondRangeMin = ParseDouble(GetIndexedValue(map, "2nd RangeMin", i), 0d);
                    item.SecondRangeMax = ParseDouble(GetIndexedValue(map, "2nd RangeMax", i), 0d);

                    item.IntegrationTime = ParseInt(GetIndexedValue(map, "IntegrationTime", i), 0);

                    ReadMetric(item.OpticMetrics, "Raw", map, "OpticItemRaw", i);
                    ReadMetric(item.OpticMetrics, "Pd", map, "OpticItemPd", i);
                    ReadMetric(item.OpticMetrics, "R3", map, "OpticItemR3", i);

                    ReadMetric(item.WaveMetrics, "WP", map, "WaveItemWP", i);
                    ReadMetric(item.WaveMetrics, "WH", map, "WaveItemWH", i);
                    ReadMetric(item.WaveMetrics, "WX", map, "WaveItemWX", i);
                    ReadMetric(item.WaveMetrics, "WY", map, "WaveItemWY", i);
                    ReadMetric(item.WaveMetrics, "WD", map, "WaveItemWD", i);
                    ReadMetric(item.WaveMetrics, "WPU", map, "WaveItemWPU", i);
                    ReadMetric(item.WaveMetrics, "CCT", map, "WaveItemCCT", i);
                    ReadMetric(item.WaveMetrics, "CIE", map, "WaveItemCIE", i);
                    ReadMetric(item.WaveMetrics, "CRI", map, "WaveItemCRI", i);
                    ReadMetric(item.WaveMetrics, "CRI9", map, "WaveItemCRI9", i);
                    ReadMetric(item.WaveMetrics, "2ndP", map, "WaveItem2ndP", i);
                    ReadMetric(item.WaveMetrics, "3ndP", map, "WaveItem3ndP", i);

                    loaded.Add(item);
                }

                items.Clear();
                items.AddRange(loaded);
                lastIt2RawMap = map;

                // LoadFromIt2() 마지막 items 갱신 직전/직후에 추가
                ContactOP = ParseInt(GetValue(map, "ContactOP"), 0);
                ContactRules.Clear();

                int totalCount = ParseInt(GetValue(map, "TotalCount"), items.Count);
                if (totalCount < 0) totalCount = 0;

                for (int i = 0; i < totalCount; i++)
                {
                    var c = new It2ContactRule
                    {
                        Use = ParseBoolTextOr01(GetIndexedValue(map, "ContactUse", i), false),
                        FailOnSkip = ParseBoolTextOr01(GetIndexedValue(map, "FailOnSkip", i), false),
                        Low = ParseDouble(GetIndexedValue(map, "ContactLow", i), 0d),
                        High = ParseDouble(GetIndexedValue(map, "ContactHigh", i), 0d),
                        Op1 = ParseInt(GetIndexedValue(map, "ContactOp1", i), 0),
                        Op2 = ParseInt(GetIndexedValue(map, "ContactOp2", i), 0),
                        Ok = ParseBoolTextOr01(GetIndexedValue(map, "ContactOK", i), false)
                    };
                    ContactRules.Add(c);
                }

                return 0;
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                return -1;
            }
        }

        private int SaveToIt2(string filePath)
        {
            try
            {
                var map = lastIt2RawMap != null
                    ? new Dictionary<string, string>(lastIt2RawMap, StringComparer.OrdinalIgnoreCase)
                    : new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                map["ItemCount"] = items.Count.ToString(CultureInfo.InvariantCulture);
                if (!map.ContainsKey("TotalCount"))
                    map["TotalCount"] = (items.Count + 1).ToString(CultureInfo.InvariantCulture);
                if (!map.ContainsKey("OpticStart"))
                    map["OpticStart"] = items.Count.ToString(CultureInfo.InvariantCulture);

                for (int i = 0; i < items.Count; i++)
                {
                    var item = items[i];
                    map["Item[" + i + "]"] = ToLegacyTypeCode(item).ToString(CultureInfo.InvariantCulture);
                    map["ItemName[" + i + "]"] = item.Name ?? string.Empty;
                    map["SourceValue[" + i + "]"] = item.SourceValue.ToString("0.000000E+000", CultureInfo.InvariantCulture);
                    map["SreDelay[" + i + "]"] = item.SourceTime.ToString("0.000000E+000", CultureInfo.InvariantCulture);
                    map["WaitTime[" + i + "]"] = item.WaitTime.ToString("0.000000E+000", CultureInfo.InvariantCulture);
                    map["OffTime[" + i + "]"] = item.OffTime.ToString("0.000000E+000", CultureInfo.InvariantCulture);
                    map["NplcTime[" + i + "]"] = item.MeasureTime.ToString("0.000000E+000", CultureInfo.InvariantCulture);
                    map["MeasureLow[" + i + "]"] = item.MeasureLow.ToString("0.000000E+000", CultureInfo.InvariantCulture);
                    map["MeasureHigh[" + i + "]"] = item.MeasureHigh.ToString("0.000000E+000", CultureInfo.InvariantCulture);
                    map["MeasureLimit[" + i + "]"] = item.MeasureLimit.ToString("0.000000E+000", CultureInfo.InvariantCulture);
                    map["StrSourceUnit[" + i + "]"] = string.IsNullOrWhiteSpace(item.Unit) ? string.Empty : item.Unit;
                    map["Optical[" + i + "]"] = item.GetTestItemCategory() == TestItemCategory.Optical ? "1" : "0";

                    // SaveToIt2() item loop 내부에 추가
                    map["ItemName2[" + i + "]"] = item.ItemName2 ?? string.Empty;

                    map["KeyChNo[" + i + "]"] = item.KeyChNo.ToString(CultureInfo.InvariantCulture);
                    map["OpenCheckFlag[" + i + "]"] = item.OpenCheckFlag.ToString(CultureInfo.InvariantCulture);
                    map["SourceRange[" + i + "]"] = item.SourceRange.ToString(CultureInfo.InvariantCulture);
                    map["MeasureRange[" + i + "]"] = item.MeasureRange.ToString(CultureInfo.InvariantCulture);
                    map["MeasureUnit[" + i + "]"] = item.MeasureUnit.ToString(CultureInfo.InvariantCulture);

                    map["OpticDCUsed[" + i + "]"] = item.OpticDCUsed.ToString(CultureInfo.InvariantCulture);
                    map["NGSkip[" + i + "]"] = item.NGSkip.ToString(CultureInfo.InvariantCulture);

                    map["WaveCount[" + i + "]"] = item.WaveCount.ToString(CultureInfo.InvariantCulture);
                    map["ExposeCount[" + i + "]"] = item.ExposeCount.ToString(CultureInfo.InvariantCulture);
                    map["IvRange[" + i + "]"] = item.IvRange.ToString(CultureInfo.InvariantCulture);
                    map["Polarity[" + i + "]"] = item.Polarity.ToString(CultureInfo.InvariantCulture);

                    map["Full RangeMin[" + i + "]"] = item.FullRangeMin.ToString("0.000000E+000", CultureInfo.InvariantCulture);
                    map["Full RangeMax[" + i + "]"] = item.FullRangeMax.ToString("0.000000E+000", CultureInfo.InvariantCulture);
                    map["2nd RangeMin[" + i + "]"] = item.SecondRangeMin.ToString("0.000000E+000", CultureInfo.InvariantCulture);
                    map["2nd RangeMax[" + i + "]"] = item.SecondRangeMax.ToString("0.000000E+000", CultureInfo.InvariantCulture);

                    map["IntegrationTime[" + i + "]"] = item.IntegrationTime.ToString(CultureInfo.InvariantCulture);

                    WriteMetric(item.OpticMetrics, "Raw", map, "OpticItemRaw", i);
                    WriteMetric(item.OpticMetrics, "Pd", map, "OpticItemPd", i);
                    WriteMetric(item.OpticMetrics, "R3", map, "OpticItemR3", i);

                    WriteMetric(item.WaveMetrics, "WP", map, "WaveItemWP", i);
                    WriteMetric(item.WaveMetrics, "WH", map, "WaveItemWH", i);
                    WriteMetric(item.WaveMetrics, "WX", map, "WaveItemWX", i);
                    WriteMetric(item.WaveMetrics, "WY", map, "WaveItemWY", i);
                    WriteMetric(item.WaveMetrics, "WD", map, "WaveItemWD", i);
                    WriteMetric(item.WaveMetrics, "WPU", map, "WaveItemWPU", i);
                    WriteMetric(item.WaveMetrics, "CCT", map, "WaveItemCCT", i);
                    WriteMetric(item.WaveMetrics, "CIE", map, "WaveItemCIE", i);
                    WriteMetric(item.WaveMetrics, "CRI", map, "WaveItemCRI", i);
                    WriteMetric(item.WaveMetrics, "CRI9", map, "WaveItemCRI9", i);
                    WriteMetric(item.WaveMetrics, "2ndP", map, "WaveItem2ndP", i);
                    WriteMetric(item.WaveMetrics, "3ndP", map, "WaveItem3ndP", i);
                }

                // SaveToIt2() item loop 이후에 추가 (Contact 저장)
                map["ContactOP"] = ContactOP.ToString(CultureInfo.InvariantCulture);

                int contactCount = ContactRules != null ? ContactRules.Count : 0;
                if (contactCount == 0)
                {
                    int totalCount = ParseInt(GetValue(map, "TotalCount"), items.Count);
                    contactCount = totalCount > 0 ? totalCount : items.Count;
                }
                map["TotalCount"] = contactCount.ToString(CultureInfo.InvariantCulture);

                for (int i = 0; i < contactCount; i++)
                {
                    It2ContactRule c = (ContactRules != null && i < ContactRules.Count) ? ContactRules[i] : new It2ContactRule();

                    map["ContactUse[" + i + "]"] = c.Use ? "true" : "false";
                    map["FailOnSkip[" + i + "]"] = c.FailOnSkip ? "true" : "false";
                    map["ContactLow[" + i + "]"] = c.Low.ToString("0.000000", CultureInfo.InvariantCulture);
                    map["ContactHigh[" + i + "]"] = c.High.ToString("0.000000", CultureInfo.InvariantCulture);
                    map["ContactOp1[" + i + "]"] = c.Op1.ToString(CultureInfo.InvariantCulture);
                    map["ContactOp2[" + i + "]"] = c.Op2.ToString(CultureInfo.InvariantCulture);
                    map["ContactOK[" + i + "]"] = c.Ok ? "true" : "false";
                }

                var sb = new StringBuilder();
                sb.AppendLine("[TESTER_DATA]");
                foreach (var kv in map.OrderBy(k => k.Key, StringComparer.OrdinalIgnoreCase))
                {
                    sb.Append(kv.Key).Append('=').AppendLine(kv.Value ?? string.Empty);
                }

                System.IO.File.WriteAllText(filePath, sb.ToString(), Encoding.Default);
                return 0;
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                return -1;
            }
        }

        // TestConditionSet 클래스 helper 메서드로 추가
        private static void ReadMetric(Dictionary<string, It2MetricSpec> target, string key, Dictionary<string, string> map, string prefix, int index)
        {
            if (target == null)
                return;

            var metric = new It2MetricSpec
            {
                LowLevel = ParseDouble(GetIndexedValue(map, prefix + "Level", index), 0d),
                HighLevel = ParseDouble(GetIndexedValue(map, prefix + "HighLevel", index), 0d),
                Name = (GetIndexedValue(map, prefix + "Name", index) ?? string.Empty).TrimEnd(),
                Use = ParseBool01(GetIndexedValue(map, prefix + "Use", index), false)
            };

            target[key] = metric;
        }

        private static void WriteMetric(Dictionary<string, It2MetricSpec> source, string key, Dictionary<string, string> map, string prefix, int index)
        {
            if (source == null)
                return;

            It2MetricSpec metric;
            if (!source.TryGetValue(key, out metric) || metric == null)
                return;

            map[prefix + "Level[" + index + "]"] = metric.LowLevel.ToString("0.000000E+000", CultureInfo.InvariantCulture);
            map[prefix + "HighLevel[" + index + "]"] = metric.HighLevel.ToString("0.000000E+000", CultureInfo.InvariantCulture);
            map[prefix + "Name[" + index + "]"] = metric.Name ?? string.Empty;
            map[prefix + "Use[" + index + "]"] = metric.Use ? "1" : "0";
        }

        private static bool ParseBool01(string text, bool defaultValue)
        {
            if (string.IsNullOrWhiteSpace(text))
                return defaultValue;

            text = text.Trim();
            if (text == "1")
                return true;
            if (text == "0")
                return false;

            bool b;
            if (bool.TryParse(text, out b))
                return b;

            int i;
            if (int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out i))
                return i != 0;

            return defaultValue;
        }

        private static bool ParseBoolTextOr01(string text, bool defaultValue)
        {
            return ParseBool01(text, defaultValue);
        }
        private static string GetValue(Dictionary<string, string> map, string key)
        {
            string value;
            return map.TryGetValue(key, out value) ? value : null;
        }

        private static string GetIndexedValue(Dictionary<string, string> map, string key, int index)
        {
            return GetValue(map, key + "[" + index + "]");
        }

        private static int ParseInt(string text, int defaultValue)
        {
            int value;
            return int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out value)
                ? value
                : defaultValue;
        }

        private static double ParseDouble(string text, double defaultValue)
        {
            double value;
            return double.TryParse(text, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out value)
                ? value
                : defaultValue;
        }

        // TestConditionSet 클래스 내부 기존 ParseLegacyType 메서드 교체
        private static TestItemType ParseLegacyType(string legacyCode, string itemName, string opticalFlag)
        {
            // 1) Optical 플래그가 켜져 있으면 우선 Optical 타입으로 해석
            if (ParseBool01(opticalFlag, false))
                return TestItemType.WP; // Optical category 확보용 기본 타입

            // 2) 그 외는 이름 기반 해석
            string name = (itemName ?? string.Empty).Trim().ToUpperInvariant();

            if (name.StartsWith("VF")) return TestItemType.VF;
            if (name.StartsWith("VR")) return TestItemType.VR;
            if (name.StartsWith("IF")) return TestItemType.IF;
            if (name.StartsWith("IR")) return TestItemType.IR;
            if (name.StartsWith("KELFS")) return TestItemType.KELFS;
            if (name.StartsWith("KELDG")) return TestItemType.KELDG;
            if (name.StartsWith("WP")) return TestItemType.WP;
            if (name.StartsWith("FWHM")) return TestItemType.FWHM;
            if (name.StartsWith("CCT")) return TestItemType.CCT;
            if (name.StartsWith("CRI")) return TestItemType.CRI;

            // 3) 코드 기반 해석
            int code;
            if (int.TryParse(legacyCode, NumberStyles.Integer, CultureInfo.InvariantCulture, out code))
            {
                switch (code)
                {
                    case 0: return TestItemType.VF;
                    case 1: return TestItemType.VR;
                    case 2: return TestItemType.IF;
                    case 3: return TestItemType.IR;
                    case 4: return TestItemType.WP; // legacy optical
                    default: break;
                }
            }

            return TestItemType.None;
        }

        private static int ToLegacyTypeCode(TestConditionItem item)
        {
            switch (item.Type)
            {
                case TestItemType.VF: return 0;
                case TestItemType.VR: return 1;
                case TestItemType.IF: return 2;
                case TestItemType.IR: return 3;
            }

            // 매핑 불가 타입은 원본 legacy 코드 보존
            if (item != null && item.LegacyItemCode >= 0)
                return item.LegacyItemCode;

            return 4;
        }
        #endregion

        #region Mehods
        public string[] GetItemNameList()
        {
            return items.Select(x => x.Name).ToArray();
        }
        public bool Validate()
        {
            try
            {
                // 아이템은 최소 1개 이상이여야 한다.
                if (items.Count == 0)
                    return false;

                // 각 아이템들의 이름은 중복을 허용하지 않는다.
                if (items.GroupBy(x => x.Name).Any(g => g.Count() > 1))
                    return false;

                // 모든 아이템들은 유효성 검사를 통과해야 한다.
                foreach (var item in items)
                {
                    if (!item.Validate())
                        return false;
                }

                // User Define 아이템의 정규식이 유효한지 검사
                var userDefineItems = items.Where(x => x.GetTestItemCategory() == TestItemCategory.UserDefined);
                foreach (var item in userDefineItems)
                {
                    if (!IsValidExpression(item.Expression))
                        return false;
                }
            }
            catch /*(Exception ex)*/
            {
                // 예외가 발생하면 유효하지 않은 것으로 간주
                return false;
            }
            return true;
        }

        private bool IsValidExpression(string expression)
        {
            if (string.IsNullOrWhiteSpace(expression))
                return false;

            // 허용된 문자 집합 정의
            var allowedChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+-*/(). ";

            // 각 문자가 허용된 문자 집합에 속하는지 확인
            foreach (var ch in expression)
            {
                if (!allowedChars.Contains(ch))
                    return false;
            }

            // 추가적인 구문 검사 (예: 괄호 짝 맞추기)
            int balance = 0;
            foreach (var ch in expression)
            {
                if (ch == '(') balance++;
                else if (ch == ')') balance--;
                if (balance < 0) 
                    return false; // 닫는 괄호가 더 많음
            }
            if (balance != 0) 
                return false; // 괄호 짝이 맞지 않음

            // 정규식 시뮬레이션하여 오류 발생하는 지 확인
            // 서울 바이오 타입으로 여기 패스 하자.
            //try
            //{
            //    List<string> assignItems = new List<string>();
            //    foreach (var item in Items)
            //    {
            //        var key = item.Name;
            //        var pattern = $@"\b{Regex.Escape(key)}\b";
            //        if (Regex.IsMatch(expression, pattern))
            //        {
            //            assignItems.Add(key);
            //            expression = Regex.Replace(expression, pattern, "0");
            //        }
            //    }

            //    var computeObj = evaluator.Compute(expression, "");
            //    double computedValue = Convert.ToDouble(computeObj);
            //}
            //catch (Exception)
            //{
            //    return false;
            //}

            return true;
        }
        #endregion
    }

    public class It2MetricSpec
    {
        public string Name { get; set; }
        public bool Use { get; set; }
        public double LowLevel { get; set; }
        public double HighLevel { get; set; }

        public It2MetricSpec()
        {
            Name = string.Empty;
            Use = false;
            LowLevel = 0d;
            HighLevel = 0d;
        }
    }

    public class It2ContactRule
    {
        public bool Use { get; set; }
        public bool FailOnSkip { get; set; }
        public double Low { get; set; }
        public double High { get; set; }
        public int Op1 { get; set; }
        public int Op2 { get; set; }
        public bool Ok { get; set; }
    }
}