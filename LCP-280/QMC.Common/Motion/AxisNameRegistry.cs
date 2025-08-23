using System;
using System.Collections.Generic;
using System.Linq;

namespace QMC.Common
{
    /// <summary>
    /// Logical axis key -> name/alias registry. Avoid hard-coded resolver rules.
    /// </summary>
    public static class AxisNameRegistry
    {
        private static readonly Dictionary<string, HashSet<string>> _aliases = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Replace aliases for a given axis key.
        /// </summary>
        public static void SetAliases(string axisKey, params string[] aliases)
        {
            if (string.IsNullOrWhiteSpace(axisKey)) return;
            _aliases[axisKey] = new HashSet<string>((aliases ?? Enumerable.Empty<string>()).Where(s => !string.IsNullOrWhiteSpace(s)), StringComparer.OrdinalIgnoreCase)
            {
                axisKey
            };
        }

        /// <summary>
        /// Register one alias in addition to existing ones.
        /// </summary>
        public static void RegisterAlias(string axisKey, string alias)
        {
            if (string.IsNullOrWhiteSpace(axisKey) || string.IsNullOrWhiteSpace(alias)) return;
            if (!_aliases.TryGetValue(axisKey, out var set))
            {
                set = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { axisKey };
                _aliases[axisKey] = set;
            }
            set.Add(alias);
        }

        /// <summary>
        /// Resolve axis by registry names. Falls back to legacy AxisResolver when not found.
        /// </summary>
        public static IMotionAxis Resolve(string axisKey, IEnumerable<IMotionAxis> axes)
        {
            if (axes == null) return null;

            if (!string.IsNullOrWhiteSpace(axisKey) && _aliases.TryGetValue(axisKey, out var names))
            {
                // exact match by any registered name
                var match = axes.FirstOrDefault(a => a != null && names.Contains(a.Name));
                if (match != null) return match;
            }

            // Fallback to legacy behavior
            return AxisResolver.Resolve(axisKey, axes);
        }

        /// <summary>
        /// Get configured aliases (including axisKey itself if configured).
        /// </summary>
        public static IReadOnlyCollection<string> GetAliases(string axisKey)
        {
            if (_aliases.TryGetValue(axisKey, out var set)) return set.ToArray();
            return Array.Empty<string>();
        }
    }
}
