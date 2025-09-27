using QMC.Common.Motion.Ajin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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

            return Value.ToString("F6");
        }
        public string ToStringWithPrefix()
        {
            double abs = Math.Abs(Value);
            if (abs >= 1e9)
                return (Value / 1e9).ToString("0.#####") + "G";
            else if (abs >= 1e6)
                return (Value / 1e6).ToString("0.#####") + "M";
            else if (abs >= 1e3)
                return (Value / 1e3).ToString("0.#####") + "K";
            else if (abs >= 1)
                return (Value).ToString("0.#####");
            else if (abs >= 1e-3)
                return (Value * 1e3).ToString("0.#####") + "m";
            else if (abs >= 1e-6)
                return (Value * 1e6).ToString("0.#####") + "u";
            else if (abs >= 1e-9)
                return (Value * 1e9).ToString("0.#####") + "n";
            else
                return Value.ToString("0.#####E+0");
        }
        #endregion
    }
}