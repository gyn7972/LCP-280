using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QMC.Common.PKGTester
{
    public enum BinningType
    {
        GoodBin,
        NgBin,
    };

    public class BinningRange : BaseConfig
    {
        #region Properties
        public double Min { get; set; }
        public double Max { get; set; }
        public bool IgnoreOutOfRange { get; set; }
        #endregion

        #region Constructors
        public BinningRange(string name) : base(name)
        {
            Reset();
        }
        #endregion

        #region Methods
        public override void Reset()
        {
            Min = 0;
            Max = 0;
            IgnoreOutOfRange = false;
        }
        public override bool Validate()
        {
            if (!IgnoreOutOfRange)
            {
                if (Min >= Max)
                    return false;
            }
            return true;
        }
        public bool IsInRange(double value)
        {
            if (IgnoreOutOfRange)
                return true;

            return (value >= Min && value <= Max);
        }
        public override PropertyCollection GetPropertyCollection()
        {
            var pc = new PropertyCollection();
            pc.Add("Binning Range");
            pc.Add("Name", Name); 
            pc.Add("Min", Min);
            pc.Add("Max", Max);
            pc.Add("IgnoreOutOfRange", IgnoreOutOfRange);
            return pc;
        }
        public override int ApplyValueFromPropertyCollection(PropertyCollection pc)
        {
            if (pc == null)
                return -1;
            try
            {
                Name = pc.GetValue<string>("Name");
                Min = pc.GetValue<double>("Min");
                Max = pc.GetValue<double>("Max");
                IgnoreOutOfRange = pc.GetValue<bool>("IgnoreOutOfRange");
            }
            catch (Exception ex)
            {
                // 필요시 로그 처리
                Log.Write(ex);
                return -1;
            }
            return 0;
        }
        #endregion
    }

    public class BinningSpec
    {
        #region Fields
        private Dictionary<string, BinningRange> spec = new Dictionary<string, BinningRange>();
        #endregion

        #region Properties
        public string BinLabel { get; set; }
        public BinningType BinType { get; set; }
        public IReadOnlyDictionary<string, BinningRange> Spec => spec;
        #endregion

        #region Constructors
        public BinningSpec(string label)
        {
            BinLabel = label;
            BinType = BinningType.GoodBin;
        }
        #endregion

        #region Methods
        public bool IsSatisfied(string key, double value)
        {
            if (spec.ContainsKey(key))
            {
                return spec[key].IsInRange(value);
            }
            return true; // key not found, ignore range
        }
        public bool AddDataHeader(string header)
        {
            if (spec.ContainsKey(header))
            {
                // already exists
                return true;
            }
            spec[header] = new BinningRange(header);
            return true;
        }
        public bool RemoveDataHeader(string header)
        {
            if (!spec.ContainsKey(header))
            {
                // not exists
                return true;
            }
            return spec.Remove(header);
        }
        public string[] GetHeaders()
        {
            return spec.Keys.ToArray();
        }
        #endregion
    }

    public class BinningSpecSheet
    {
        #region Fields
        private List<BinningSpec> specSheet = new List<BinningSpec>();
        #endregion

        #region Properties
        #endregion

        #region Constructors
        public BinningSpecSheet()
        {
        }
        #endregion

        #region Methods
        public void Clear()
        {
            specSheet.Clear();
        }
        public bool AddDataHeader(string header)
        {
            if (specSheet.Count == 0)
            {
                // no spec, create a default one
                AddNewBinningSpec("Default");
            }

            for (int i = 0; i < specSheet.Count; i++)
            {
                if (specSheet[i].AddDataHeader(header) == false)
                    return false;
            }
            return true;
        }
        public bool RemoveDataHeader(string header)
        {
            for (int i = 0; i < specSheet.Count; i++)
            {
                if (specSheet[i].RemoveDataHeader(header) == false)
                    return false;
            }
            return true;
        }
        public bool AddNewBinningSpec(string label)
        {
            BinningSpec newSpec = new BinningSpec(label);
            if (specSheet.Count > 0)
            {
                // copy headers from first spec
                var headers = specSheet[0].GetHeaders();
                foreach (var header in headers)
                {
                    newSpec.AddDataHeader(header);
                }
            }
            specSheet.Add(newSpec);
            return true;
        }
        public bool RemoveBinningSpecAt(int index)
        {
            if (index < 0 || index >= specSheet.Count)
                return false;
            specSheet.RemoveAt(index);
            return true;
        }
        public bool UpdateHeaderFromTestConditionSet(TestConditionSet testConditionSet)
        {
            if (testConditionSet == null)
                return false;

            var headers = testConditionSet.GetItemNameList();
            foreach (var header in headers)
            {
                AddDataHeader(header);
            }

            
            return true;
        }
        #endregion
    }
}
