using System;
using System.ComponentModel;

namespace QMC.LCP_280.Process.Component
{
    [Serializable]
    public sealed class MeasurementKey
    {
        [Category("Inspection"), DisplayName("Name")]
        public string Name { get; set; } = "";

        [Category("Inspection"), DisplayName("Lower Limit")]
        [DefaultValue(double.NaN)]
        public double LowerLimit { get; set; } = double.NaN;

        [Category("Inspection"), DisplayName("Upper Limit")]
        [DefaultValue(double.NaN)]
        public double UpperLimit { get; set; } = double.NaN;

        [Category("Inspection"), DisplayName("Unit")]
        public string Unit { get; set; } = "";
    }
}
