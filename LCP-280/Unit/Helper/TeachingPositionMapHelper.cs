using Newtonsoft.Json;
using QMC.Common.Component;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QMC.LCP_280.Process.Unit
{
    public static class TeachingPositionMapHelper
    {
        public delegate IReadOnlyList<string> AxisResolver(string positionKey);

        public static Dictionary<string, TeachingPosition> EnsureInitializedMap(Dictionary<string, TeachingPosition> map)
        {
            if (map == null)
                map = new Dictionary<string, TeachingPosition>(StringComparer.OrdinalIgnoreCase);

            // 내부 key comparer 보장(혹시 다른 comparer로 만들어진 경우를 정리)
            if (!(map.Comparer is StringComparer))
            {
                var rebuilt = new Dictionary<string, TeachingPosition>(StringComparer.OrdinalIgnoreCase);
                foreach (var kv in map)
                    rebuilt[kv.Key] = kv.Value;
                map = rebuilt;
            }

            return map;
        }

        public static void NormalizeByEnum<TEnum>(
            Dictionary<string, TeachingPosition> map,
            List<TeachingPosition> uiList,
            AxisResolver axisResolver,
            bool removeNonEnumKeys = true,
            bool applyAxisMapping = true,
            Func<string, string> defaultDescriptionFactory = null)
            where TEnum : struct
        {
            if (!typeof(TEnum).IsEnum)
                throw new ArgumentException("TEnum must be enum.");

            map = EnsureInitializedMap(map);
            if (uiList == null) uiList = new List<TeachingPosition>();

            // value.Name은 항상 key로 보정
            foreach (var k in map.Keys.ToList())
            {
                var tp = map[k];
                if (tp != null) tp.Name = k;
            }

            var validKeys = new HashSet<string>(GetEnumKeys<TEnum>(), StringComparer.OrdinalIgnoreCase);

            // 누락 enum key 생성
            foreach (var key in validKeys)
            {
                TeachingPosition tp;
                if (!map.TryGetValue(key, out tp) || tp == null)
                {
                    var axes = axisResolver != null ? axisResolver(key) : null;
                    var axisPositions = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);

                    if (axes != null)
                    {
                        foreach (var a in axes)
                            axisPositions[a] = 0.0;
                    }

                    var desc = defaultDescriptionFactory != null ? defaultDescriptionFactory(key) : ("기본 " + key + " 위치");
                    tp = new TeachingPosition(key, axisPositions, desc);
                    map[key] = tp;
                }

                tp.Name = key;
            }

            // enum 외 key 제거(정책)
            if (removeNonEnumKeys)
            {
                foreach (var k in map.Keys.ToList())
                {
                    if (!validKeys.Contains(k))
                        map.Remove(k);
                }
            }

            if (applyAxisMapping)
            {
                ApplyAxisMapping(map, axisResolver);
            }

            // UI 리스트는 enum 순서로 재구성
            uiList.Clear();
            foreach (var key in GetEnumKeys<TEnum>())
            {
                TeachingPosition tp;
                if (map.TryGetValue(key, out tp) && tp != null)
                {
                    tp.Name = key;
                    uiList.Add(tp);
                }
            }
        }

        public static void ApplyAxisMapping(
            Dictionary<string, TeachingPosition> map,
            AxisResolver axisResolver)
        {
            if (map == null || axisResolver == null)
                return;

            foreach (var kv in map.ToList())
            {
                var key = kv.Key;
                var tp = kv.Value;
                if (tp == null)
                    continue;

                var allowed = new HashSet<string>(axisResolver(key) ?? Array.Empty<string>(), StringComparer.OrdinalIgnoreCase);
                var current = tp.AxisPositions ?? new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
                var next = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);

                foreach (var axis in allowed)
                {
                    double v;
                    if (current.TryGetValue(axis, out v)) next[axis] = v;
                    else next[axis] = 0.0;
                }

                tp.AxisPositions = next;
            }
        }

        public static bool IsEnumKey<TEnum>(string key)
            where TEnum : struct
        {
            if (string.IsNullOrWhiteSpace(key)) return false;
            if (!typeof(TEnum).IsEnum) return false;

            TEnum tmp;
            return Enum.TryParse(key, out tmp) && Enum.IsDefined(typeof(TEnum), tmp);
        }

        private static IReadOnlyList<string> GetEnumKeys<TEnum>()
            where TEnum : struct
        {
            // Enum.GetValues는 박싱 -> 어쩔 수 없음(빈번 호출되는 경로면 캐시 가능)
            return Enum.GetValues(typeof(TEnum))
                       .Cast<object>()
                       .Select(v => v.ToString())
                       .ToList();
        }
    }
}