using QMC.Common.Motion;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QMC.Common
{
    /// <summary>
    /// 축 이름/키/별칭 매칭 유틸
    /// </summary>
    public static class AxisResolver
    {
        public static IMotionAxis Resolve(string axisKey,
                                          IEnumerable<IMotionAxis> axes,
                                          params string[] aliases)
        {
            if (axes == null) return null;
            // 1. exact (대소문자 무시)
            var match = axes.FirstOrDefault(a => a != null &&
                              string.Equals(a.Name, axisKey, StringComparison.OrdinalIgnoreCase));
            if (match != null) return match;

            // 2. alias exact
            if (aliases != null && aliases.Length > 0)
            {
                match = axes.FirstOrDefault(a => a != null &&
                    aliases.Any(al => string.Equals(a.Name, al, StringComparison.OrdinalIgnoreCase)));
                if (match != null) return match;
            }

            // 3. EndsWith (예: "ElevatorZ", "ArmY")
            match = axes.FirstOrDefault(a => a != null &&
                       a.Name.EndsWith(axisKey, StringComparison.OrdinalIgnoreCase));
            if (match != null) return match;

            // 4. Contains (옵션: 너무 넓으면 제거)
            match = axes.FirstOrDefault(a => a != null &&
                       a.Name.IndexOf(axisKey, StringComparison.OrdinalIgnoreCase) >= 0);
            return match;
        }
    }

    /// <summary>
    /// Component 내 축 컬렉션 관리 (조합)
    /// </summary>
    public class AxisManager
    {
        private readonly List<AxisDefinition> _axes = new List<AxisDefinition>();
        private readonly Dictionary<string, AxisDefinition> _map = new Dictionary<string, AxisDefinition>(StringComparer.OrdinalIgnoreCase);

        public IReadOnlyList<AxisDefinition> Axes { get { return _axes; } }

        public void Clear()
        {
            _axes.Clear();
            _map.Clear();
        }

        public AxisDefinition Register(string axisKey, string displayName, IMotionAxis motionAxis)
        {
            if (motionAxis == null) return null;
            AxisDefinition def;
            if (_map.TryGetValue(axisKey, out def))
                return def;

            def = new AxisDefinition(axisKey, displayName, motionAxis);
            _axes.Add(def);
            _map[axisKey] = def;
            return def;
        }

        public AxisDefinition Find(string axisKeyOrName)
        {
            AxisDefinition def;
            if (_map.TryGetValue(axisKeyOrName, out def))
                return def;

            foreach (var a in _axes)
            {
                if (string.Equals(a.DisplayName, axisKeyOrName, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(a.MotionAxis.Name, axisKeyOrName, StringComparison.OrdinalIgnoreCase))
                    return a;
            }
            return null;
        }
    }
}