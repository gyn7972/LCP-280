using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace QMC.Common.PKGTester
{
    public class TestConditionItem : BaseConfig
    {
        #region Properties
        // Defines
        public TestItemType Type { get; set; }

        // Source
        public double SourceValue { get; set; }
        public double SourceTime { get; set; }
        public double SourceLimit { get; set; }

        // Measure
        public double MeasureTime { get; set; }

        // Data Calibration
        public bool UseGain { get; set; }
        public bool UseOffset { get; set; }
        public double Gain { get; set; }
        public double Offset { get; set; }
        #endregion

        #region Constructor
        public TestConditionItem(string name) : base(name)
        {
        }
        #endregion

        #region Methods
        public override void Reset()
        {
            Type = TestItemType.None;
            SourceValue = 0;
            SourceTime = 1;
            SourceLimit = 0;
            MeasureTime = 1;
            UseGain = false;
            UseOffset = false;
            Gain = 1;
            Offset = 0;
        }
        public override bool Validate()
        {
            if (string.IsNullOrWhiteSpace(Name))
                return false;
            if (Type.GetCategory() == TestItemCategory.Undefined)
                return false;
            if (SourceValue < 0)
                return false;
            if (SourceTime <= 0)
                return false;
            if (SourceLimit < 0)
                return false;
            if (MeasureTime <= 0)
                return false;

            return true;
        }
        public override PropertyCollection GetPropertyCollection()
        {
            PropertyCollection pc = new PropertyCollection();
            string title = $"Item Config";
            pc.Add(title);

            pc.Add(nameof(Name), Name);
            pc.Add(nameof(Type), Type);
            if (Type.GetCategory() == TestItemCategory.Electrical)
            {
                pc.Add(nameof(SourceValue), SourceValue);
                pc.Add(nameof(SourceTime), SourceTime);
                pc.Add(nameof(SourceLimit), SourceLimit);
                pc.Add(nameof(MeasureTime), MeasureTime);
            }
            pc.Add(nameof(UseGain), UseGain);
            pc.Add(nameof(UseOffset), UseOffset);
            pc.Add(nameof(Gain), Gain);
            pc.Add(nameof(Offset), Offset);

            return pc;
        }
        public override int ApplyValueFromPropertyCollection(PropertyCollection pc)
        {
            if (pc == null)
                return -1;
            try
            {
                Name = "";
                Reset();

                Name = pc.GetValue<string>(nameof(Name));
                Type = pc.GetValue<TestItemType>(nameof(Type));
                if (Type.GetCategory() == TestItemCategory.Electrical)
                {
                    SourceValue = pc.GetValue<double>(nameof(SourceValue));
                    SourceTime = pc.GetValue<double>(nameof(SourceTime));
                    SourceLimit = pc.GetValue<double>(nameof(SourceLimit));
                    MeasureTime = pc.GetValue<double>(nameof(MeasureTime));
                }
                UseGain = pc.GetValue<bool>(nameof(UseGain));
                UseOffset = pc.GetValue<bool>(nameof(UseOffset));
                Gain = pc.GetValue<double>(nameof(Gain));
                Offset = pc.GetValue<double>(nameof(Offset));
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                return -1;
            }
            return 0;
        }
        public TestItemCategory GetTestItemCategory()
        {
            return Type.GetCategory();
        }
        #endregion
    }

    public class TestConditionSet
    {
        #region Fields
        private List<TestConditionItem> items = new List<TestConditionItem>();
        #endregion

        #region Proerties
        public string Name { get; set; }
        public IReadOnlyList<TestConditionItem> Items => items.AsReadOnly();
        #endregion

        #region Constructor
        public TestConditionSet(string name) : base()
        {
            Name = name;
            items = new List<TestConditionItem>();
        }
        #endregion

        #region Event
        public delegate void ItemsChangedEventHandler(object sender);
        public event ItemsChangedEventHandler ItemsChanged;
        #endregion

        #region Edit Item
        public int AddItem(TestConditionItem item)
        {
            if (item == null)
                return -1;
            //if (items.Any(x => x.Name == item.Name))
            //    return -1;

            items.Add(item);
            ItemsChanged?.Invoke(this);
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
            ItemsChanged?.Invoke(this);
            return 0;
        }   
        public int RemoveItemAt(int index)
        {
            if (index < 0 || index >= items.Count)
                return -1;

            items.RemoveAt(index);
            ItemsChanged?.Invoke(this);
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
            ItemsChanged?.Invoke(this);
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
            
            ItemsChanged?.Invoke(this);
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

            ItemsChanged?.Invoke(this);
            return 0;
        }
        public int ClearItems()
        {
            items.Clear();
            ItemsChanged?.Invoke(this);
            return 0;
        }
        public int CopyConditionFrom(TestConditionSet testConditionSet)
        {
            if (testConditionSet == null)
                return -1;

            items.Clear();
            items.AddRange(testConditionSet.Items);
            ItemsChanged?.Invoke(this);
            return 0;
        }
        #endregion

        #region File Methods
        // 래퍼 클래스 정의
        class TestConditionSetData
        {
            public string Name { get; set; }
            public List<TestConditionItem> Items { get; set; }
        }

        public int LoadFromFile(string filePath)
        {
            try
            {
                if (!System.IO.File.Exists(filePath))
                    return -1;

                var json = System.IO.File.ReadAllText(filePath, Encoding.UTF8);
                var data = Newtonsoft.Json.JsonConvert.DeserializeObject<TestConditionSetData>(json);
                if (data == null || data.Items == null)
                    return -1;

                Name = data.Name;
                items.Clear();
                items.AddRange(data.Items);
                ItemsChanged?.Invoke(this);
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
            string name = Name;

            try
            {
                Name = System.IO.Path.GetFileNameWithoutExtension(filePath);
                var data = new TestConditionSetData
                {
                    Name = this.Name,
                    Items = this.items.ToList()
                };

                var json = Newtonsoft.Json.JsonConvert.SerializeObject(data, Newtonsoft.Json.Formatting.Indented);
                System.IO.File.WriteAllText(filePath, json, Encoding.UTF8);
                return 0;
            }
            catch (Exception ex)
            {
                Name = name;
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
        #endregion
    }
}