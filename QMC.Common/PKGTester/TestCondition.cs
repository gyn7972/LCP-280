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
        public double MeasureTime { get; set; }
        public double MeasureLimit { get; set; }
        public double MeasureLow { get; set; }
        public double MeasureHigh { get; set; }


        // User Define
        public string Expression { get; set; }

        // Data Calibration
        public bool[] UseGain { get; set; } = new bool[8];
        public bool[] UseOffset { get; set; } = new bool[8];
        public double[] Gain { get; set; } = new double[8];
        public double[] Offset { get; set; } = new double[8];

        public string Unit { get; set; }

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
            MeasureLimit = 0;
            MeasureTime = 0;
            MeasureLow = 0;
            MeasureHigh = 0;

            Expression = "";

            for (int i = 0; i < 8; i++)
            {
                UseGain[i] = false;
                UseOffset[i] = false;
                Gain[i] = 1;
                Offset[i] = 0;
            }
        }
        public /*override*/ bool Validate()
        {
            if (string.IsNullOrWhiteSpace(Name))
                return false;

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
                        pc.Add("Measure Limit", GetMeasureUnitFromType(), MeasureLimit);
                        pc.Add("Measure Low", GetMeasureUnitFromType(), MeasureLow);
                        pc.Add("Measure High", GetMeasureUnitFromType(), MeasureHigh);
                    }
                    break;
                case TestItemCategory.Optical:
                    {
                        pc.Add("Measure");
                        pc.Add("Measure Low", GetMeasureUnitFromType(), MeasureLow);
                        pc.Add("Measure High", GetMeasureUnitFromType(), MeasureHigh);
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
                Name = "";
                Reset();

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
                            MeasureLimit = pc.GetValue<double>("Measure Limit");
                            MeasureLow = pc.GetValue<double>("Measure Low");
                            MeasureHigh = pc.GetValue<double>("Measure High");
                        }
                        break;
                    case TestItemCategory.Optical:
                        {
                            MeasureLow = pc.GetValue<double>("Measure Low");
                            MeasureHigh = pc.GetValue<double>("Measure High");
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
            switch (Type)
            {
                case TestItemType.VF:
                    Unit = "mA";
                    return "mA";
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
            switch (Type)
            {
                case TestItemType.VF:
                    Unit = "mA";
                    return "mA";
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

                var json = System.IO.File.ReadAllText(filePath, Encoding.UTF8);
                var settings = new Newtonsoft.Json.JsonSerializerSettings();
                settings.Converters.Add(new StringEnumConverter());
                var data = Newtonsoft.Json.JsonConvert.DeserializeObject<TestConditionSetData>(json, settings);
                if (data == null || data.Items == null)
                    return -1;

                items.Clear();
                items.AddRange(data.Items);
                return 0;
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
}