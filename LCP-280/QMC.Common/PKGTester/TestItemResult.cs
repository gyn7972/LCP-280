using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QMC.Common.PKGTester
{
    public class TestItemResult
    {
        #region Properties
        public double RawData { get; set; }
        public double Value { get; set; }
        public string Unit { get; set; }
        #endregion

        #region Constructor
        public TestItemResult()
        {
            Reset();
        }
        #endregion

        #region Method
        public void Reset()
        {
            RawData = 0;
            Value = 0;
            Unit = "";
        }
        public void Assign(TestItemResult result)
        {
            if (result != null)
            {
                RawData = result.RawData;
                Value = result.Value;
                Unit = result.Unit;
            }   
        }
        public override string ToString()
        {
            if (string.IsNullOrEmpty(Unit))
                return Value.ToString("F6");

            return Value.ToString("F6") + " " + Unit;
        }
        #endregion
    }
}