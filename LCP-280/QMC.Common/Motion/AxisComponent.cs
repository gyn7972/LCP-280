using QMC.Common.Motion.Ajin;
using QMC.Common.Motions;
using System.Collections.Generic;
using System.Linq;

namespace QMC.Common
{
    /// <summary>
    /// Component 가 다수 축을 소유할 수 있도록 하는 베이스
    /// </summary>
    public abstract class AxisComponent
    {
        private readonly List<AxisDefinition> _axes = new List<AxisDefinition>();
        public IReadOnlyList<AxisDefinition> Axes => _axes;

        protected void RegisterAxis(AxisDefinition axis)
        {
            if (axis != null && !_axes.Contains(axis))
                _axes.Add(axis);
        }

        public AxisDefinition FindAxis(string axisKeyOrName)
        {
            return _axes.FirstOrDefault(a =>
                a.AxisKey == axisKeyOrName || a.DisplayName == axisKeyOrName || a.MotionAxis.Name == axisKeyOrName);
        }
    }
}