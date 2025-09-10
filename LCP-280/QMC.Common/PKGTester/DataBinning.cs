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
}
