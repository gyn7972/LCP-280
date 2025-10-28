using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace QMC.Common
{
    /// <summary>
    /// Config 객체를 PropertyCollectionView 와 바인딩하기 위한 리플렉션 매퍼.
    /// 1) 객체 → PropertyCollection (Build)
    /// 2) UI Apply 후 → 객체 값 반영 (ApplyToObject)
    /// </summary>
    public sealed class ConfigReflectionMapper
    {
        private readonly object _target;
        private readonly Type _type;
        private readonly List<PropEntry> _entries = new List<PropEntry>();

        public PropertyCollection PropertyCollection { get; private set; }

        private class PropEntry
        {
            public PropertyInfo Pi;
            public string Title;
            public string Category;
            public bool ReadOnly;
            public Type PropType;
        }

        public ConfigReflectionMapper(object target)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));
            _target = target;
            _type = target.GetType();
            Build();
        }

        private static bool IsSupported(Type t)
        {
            var nt = Nullable.GetUnderlyingType(t) ?? t;
            if (nt.IsEnum) return true;
            if (nt == typeof(string) || nt == typeof(bool) || nt == typeof(int) ||
                nt == typeof(long) || nt == typeof(float) || nt == typeof(double))
                return true;
            return false;
        }

        private void Build()
        {
            PropertyCollection = new PropertyCollection();
            PropertyCollection.IsInputParameter = true;

            var props = _type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead && p.CanWrite)
                .Where(p => p.GetCustomAttribute<ConfigIgnoreAttribute>() == null)
                .Where(p => IsSupported(p.PropertyType))
                .Select(p =>
                {
                    var dn = p.GetCustomAttribute<DisplayNameAttribute>();
                    var cat = p.GetCustomAttribute<CategoryAttribute>();
                    var ro = p.GetCustomAttribute<ReadOnlyAttribute>();
                    var order = p.GetCustomAttribute<ConfigOrderAttribute>();
                    return new
                    {
                        Pi = p,
                        Title = (dn != null && !string.IsNullOrWhiteSpace(dn.DisplayName)) ? dn.DisplayName : p.Name,
                        Category = (cat != null && !string.IsNullOrWhiteSpace(cat.Category)) ? cat.Category : "General",
                        ReadOnly = ro != null && ro.IsReadOnly,
                        Order = order?.Order ?? 0
                    };
                })
                .GroupBy(x => x.Category)
                .OrderBy(g => g.Key) // 카테고리명 정렬(필요시 별도 순서 체계 도입)
                .ToList();

            foreach (var grp in props)
            {
                PropertyCollection.Add(new TitleOnlyProperty(grp.Key));

                foreach (var p in grp.OrderBy(x => x.Order).ThenBy(x => x.Title))
                {
                    var value = p.Pi.GetValue(_target);
                    var t = p.Pi.PropertyType;
                    var nt = Nullable.GetUnderlyingType(t) ?? t;

                    if (nt.IsEnum)
                    {
                        var names = Enum.GetNames(nt).ToList();
                        string sel = value != null ? value.ToString() : names.FirstOrDefault();
                        var cbProp = new ComboBoxProperty(p.Title, sel, names);
                        if (p.ReadOnly) cbProp.Value = sel; // ReadOnly 시 편집 막기: 아래에서 IsInputParameter 트릭 or 별도 처리
                        PropertyCollection.Add(cbProp);
                        _entries.Add(new PropEntry { Pi = p.Pi, Title = p.Title, Category = grp.Key, ReadOnly = p.ReadOnly, PropType = nt });
                    }
                    else if (nt == typeof(string))
                    {
                        PropertyCollection.Add(new StringProperty(p.Title, value as string ?? string.Empty));
                        _entries.Add(new PropEntry { Pi = p.Pi, Title = p.Title, Category = grp.Key, ReadOnly = p.ReadOnly, PropType = nt });
                    }
                    else if (nt == typeof(bool))
                    {
                        PropertyCollection.Add(new BoolProperty(p.Title, value is bool b ? b : false));
                        _entries.Add(new PropEntry { Pi = p.Pi, Title = p.Title, Category = grp.Key, ReadOnly = p.ReadOnly, PropType = nt });
                    }
                    else if (nt == typeof(int))
                    {
                        PropertyCollection.Add(new IntProperty(p.Title, value is int i ? i : 0));
                        _entries.Add(new PropEntry { Pi = p.Pi, Title = p.Title, Category = grp.Key, ReadOnly = p.ReadOnly, PropType = nt });
                    }
                    else if (nt == typeof(long))
                    {
                        PropertyCollection.Add(new LongProperty(p.Title, value is long l ? l : 0L));
                        _entries.Add(new PropEntry { Pi = p.Pi, Title = p.Title, Category = grp.Key, ReadOnly = p.ReadOnly, PropType = nt });
                    }
                    else if (nt == typeof(float))
                    {
                        PropertyCollection.Add(new FloatProperty(p.Title, value is float f ? f : 0f));
                        _entries.Add(new PropEntry { Pi = p.Pi, Title = p.Title, Category = grp.Key, ReadOnly = p.ReadOnly, PropType = nt });
                    }
                    else if (nt == typeof(double))
                    {
                        PropertyCollection.Add(new DoubleProperty(p.Title, value is double d ? d : 0.0));
                        _entries.Add(new PropEntry { Pi = p.Pi, Title = p.Title, Category = grp.Key, ReadOnly = p.ReadOnly, PropType = nt });
                    }
                }
            }
        }

        public void ApplyToObject(PropertyCollection pc)
        {
            if (pc == null) return;

            foreach (var e in _entries)
            {
                if (e.ReadOnly) continue;
                object newVal = null;

                if (e.PropType.IsEnum)
                {
                    string str = pc.GetValue<string>(e.Title);
                    try { newVal = Enum.Parse(e.PropType, str); }
                    catch { continue; }
                }
                else if (e.PropType == typeof(string))
                {
                    newVal = pc.GetValue<string>(e.Title);
                }
                else if (e.PropType == typeof(bool))
                {
                    newVal = pc.GetValue<bool>(e.Title);
                }
                else if (e.PropType == typeof(int))
                {
                    newVal = pc.GetValue<int>(e.Title);
                }
                else if (e.PropType == typeof(long))
                {
                    newVal = pc.GetValue<long>(e.Title);
                }
                else if (e.PropType == typeof(float))
                {
                    // float GetValue 직접 실패시 string 재파싱
                    try { newVal = pc.GetValue<float>(e.Title); }
                    catch
                    {
                        try
                        {
                            var s = pc.GetValue<string>(e.Title);
                            if (float.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var fv))
                                newVal = fv;
                        }
                        catch { }
                    }
                }
                else if (e.PropType == typeof(double))
                {
                    try { newVal = pc.GetValue<double>(e.Title); }
                    catch
                    {
                        try
                        {
                            var s = pc.GetValue<string>(e.Title);
                            if (double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var dv))
                                newVal = dv;
                        }
                        catch { }
                    }
                }

                if (newVal != null)
                {
                    try { e.Pi.SetValue(_target, newVal); }
                    catch { }
                }
            }
        }
    }
}