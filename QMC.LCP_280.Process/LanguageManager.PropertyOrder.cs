using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Forms;

namespace QMC.Common
{
    public partial class LanguageManager
    {
        // Configº° Property Order ÀúÀå (Group: TypeName, Key: Name(or Display) -> Order)
        private readonly Dictionary<string, Dictionary<string, int>> _propertyOrders = new Dictionary<string, Dictionary<string, int>>(StringComparer.OrdinalIgnoreCase);

        public void SetPropertyOrder(string group, IEnumerable<string> order)
        {
            if (string.IsNullOrWhiteSpace(group) || order == null) return;
            var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            int i = 0;
            foreach (var name in order)
            {
                if (string.IsNullOrWhiteSpace(name)) continue;
                if (!map.ContainsKey(name)) map[name] = i++;
            }
            _propertyOrders[group] = map;
        }
        public Dictionary<string, int> GetPropertyOrder(string group)
        {
            if (string.IsNullOrWhiteSpace(group)) return new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            if (_propertyOrders.TryGetValue(group, out var map)) return new Dictionary<string, int>(map, StringComparer.OrdinalIgnoreCase);
            return new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        }
        public Dictionary<string, int> GetPropertyOrderMap(string group) => GetPropertyOrder(group);
    }
}
