using Newtonsoft.Json;
using QMC.Common.Motions; // MotionAxis 등 사용
using System;
using System.Collections.Generic;

namespace QMC.Common.Component
{
    public class TeachingPosition
    {
        public string Name { get; set; } // 포지션 이름
        public Dictionary<string, double> AxisPositions { get; set; } = new Dictionary<string, double>();
        public string Description { get; set; }
        public Dictionary<string, object> ExtraInfo { get; set; } = new Dictionary<string, object>();

        // 축 정보: 축 이름 → MotionAxis 객체
        [JsonIgnore]
        public Dictionary<string, MotionAxis> Axes { get; set; } = new Dictionary<string, MotionAxis>();

        public TeachingPosition() { }
        public TeachingPosition(string name, Dictionary<string, double> axisPositions, string description = null)
        {
            Name = name;
            AxisPositions = axisPositions ?? new Dictionary<string, double>();
            Description = description;
        }

        public void SetAxisPosition(string axisKey, double position)
        {
            AxisPositions[axisKey] = position;
        }
        public double GetAxisPosition(string axisKey, double defaultValue = 0)
        {
            if (AxisPositions.TryGetValue(axisKey, out var pos))
                return pos;
            return defaultValue;
        }

        // 축 객체를 _axisManager에서 검색해서 등록
        public void BindAxes(MotionAxisManager axisManager, string unitName = "unit")
        {
            foreach (var axisKey in AxisPositions.Keys)
            {
                var axis = axisManager.Get(unitName, axisKey);
                if (axis != null)
                    Axes[axisKey] = axis;
            }
        }
    }
}