using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace QMC.Common
{
    public class PropertyCollection : IEnumerable<PropertyBase>
    {
        private readonly List<PropertyBase> _properties = new List<PropertyBase>();
        public bool ShowNoColumn { get; set; } = true; 
        public bool IsInputParameter { get; set; } = true; // true : 입력파라미터, false : 출력파라미터

        public void Add(PropertyBase property)
        {
            _properties.Add(property);
        }

        public bool Remove(PropertyBase property)
        {
            return _properties.Remove(property);
        }

        // Property Collection 제너릭 처리 추가
        /// <summary>
        /// Title Only Property를 PropertyCollection에 추가합니다.
        /// </summary>
        public void Add(string title)
        {
            TitleOnlyProperty titleProp = new TitleOnlyProperty(title);
            _properties.Add(titleProp);
        }

        /// <summary>
        /// Data type에 따라 적절한 Property를 PropertyCollection에 추가합니다.
        /// </summary>
        public void Add(string title, string valueUnit, object obj)
        {
            string fullTitle = string.IsNullOrWhiteSpace(valueUnit) ? title : $"{title} ({valueUnit})";

            if (obj is bool value)
            {
                BoolProperty boolProp = new BoolProperty(fullTitle, value);
                _properties.Add(boolProp);
            }
            else if (obj is int intValue)
            {
                IntProperty intProp = new IntProperty(fullTitle, intValue);
                _properties.Add(intProp);
            }
            else if (obj is long longValue)
            {
                LongProperty longProp = new LongProperty(fullTitle, longValue);
                _properties.Add(longProp);
            }
            else if (obj is float floatValue)
            {
                FloatProperty floatProp = new FloatProperty(fullTitle, floatValue);
                _properties.Add(floatProp);
            }
            else if (obj is double doubleValue)
            {
                // TeachingPosition.AxisPositions (Dictionary<string,double>) 지원
                DoubleProperty doubleProp = new DoubleProperty(fullTitle, doubleValue);
                _properties.Add(doubleProp);
            }
            else if (obj is string strValue)
            {
                StringProperty stringProp = new StringProperty(fullTitle, strValue);
                _properties.Add(stringProp);
            }
            else if (obj.GetType().IsEnum)
            {
                ComboBoxProperty comboBoxProp = new ComboBoxProperty(fullTitle, obj.ToString(), System.Enum.GetNames(obj.GetType()).ToList());
                _properties.Add(comboBoxProp);
            }
            else
            {
                throw new System.ArgumentException("Unsupported object type");
            }
        }

        /// <summary>
        /// PropertyCollection에서 Title에 해당하는 Property의 값을 반환합니다.
        /// </summary>
        public T GetValue<T>(string title)
        {
            //var prop = _properties.FirstOrDefault(p => p.Title == title);
            PropertyBase prop = null;

            
            title = RemoveUint(title);

            foreach (var p in _properties)
            {
                if (p != null)
                {
                    string propTitle = p.Title;
                    string compareTitle = RemoveUint(propTitle);

                    if (title.Equals(compareTitle, StringComparison.OrdinalIgnoreCase))
                    {
                        prop = p;
                        break;
                    }
                }
            }
            if (prop == null)
                throw new KeyNotFoundException($"Property with title '{title}' not found.");

            Type type = typeof(T);
            if (type == typeof(bool))
            {
                if (prop is BoolProperty)
                    return (T)prop.Value;
                else
                    throw new InvalidCastException($"Property '{title}' is not of type BoolProperty.");
            }
            else if (type == typeof(int))
            {
                if (prop is IntProperty)
                    return (T)prop.Value;
                // 다른 숫자 타입을 int로 변환 허용
                if (prop is LongProperty lp)
                    return (T)(object)checked((int)lp.Value);
                if (prop is FloatProperty fp)
                    return (T)(object)(int)Math.Round(fp.Value);
                if (prop is DoubleProperty dp)
                    return (T)(object)(int)Math.Round(dp.Value);
                else
                    throw new InvalidCastException($"Property '{title}' is not of type IntProperty.");
            }
            else if (type == typeof(long))
            {
                if (prop is LongProperty)
                    return (T)prop.Value;
                if (prop is IntProperty ip)
                    return (T)(object)(long)ip.Value;
                if (prop is DoubleProperty dp2)
                    return (T)(object)(long)Math.Round(dp2.Value);
                throw new InvalidCastException($"Property '{title}' is not of a compatible integer property type.");
            }
            else if (type == typeof(float))
            {
                if (prop is FloatProperty)
                    return (T)prop.Value;
                if (prop is DoubleProperty dp3)
                    return (T)(object)(float)dp3.Value;
                if (prop is IntProperty ip2)
                    return (T)(object)(float)ip2.Value;
                throw new InvalidCastException($"Property '{title}' is not of a compatible float property type.");
            }
            else if (type == typeof(double))
            {
                if (prop is DoubleProperty)
                    return (T)prop.Value;
                if (prop is FloatProperty fp2)
                    return (T)(object)(double)fp2.Value;
                if (prop is IntProperty ip3)
                    return (T)(object)(double)ip3.Value;
                if (prop is LongProperty lp2)
                    return (T)(object)(double)lp2.Value;
                throw new InvalidCastException($"Property '{title}' is not of a compatible double property type.");
            }
            else if (type == typeof(string))
            {
                if (prop is StringProperty)
                    return (T)prop.Value;
                else if (prop is ComboBoxProperty)
                    return (T)prop.Value;
                else
                    return (T)(object)(prop.Value != null ? prop.Value.ToString() : string.Empty); // fallback 문자열 변환
            }
            else if (type.IsEnum)
            {
                if (prop is ComboBoxProperty comboBoxProp)
                {
                    try
                    {
                        return (T)Enum.Parse(type, comboBoxProp.Value.ToString());
                    }
                    catch (ArgumentException ex)
                    {
                        throw new InvalidCastException($"Cannot convert property '{title}' value to enum type '{type.Name}'.", ex);
                    }
                }
                else
                {
                    throw new InvalidCastException($"Property '{title}' is not of type ComboBoxProperty.");
                }
            }
            else
            {
                throw new NotSupportedException($"Type '{type.Name}' is not supported.");
            }
        }

        private static string RemoveUint(string propTitle)
        {
            int idx = propTitle.IndexOf(" (");
            string compareTitle = idx > 0 ? propTitle.Substring(0, idx) : propTitle;
            return compareTitle;
        }

        public PropertyBase this[int index]
        {
            get { return _properties[index]; }
            set { _properties[index] = value; }
        }

        public PropertyBase this[string title]
        {
            get
            {
                var prop = _properties.FirstOrDefault(p => p.Title == title);
                if (prop == null)
                    throw new KeyNotFoundException($"Property with title '{title}' not found.");
                return prop;
            }
            set
            {
                var index = _properties.FindIndex(p => p.Title == title);
                if (index == -1)
                    throw new KeyNotFoundException($"Property with title '{title}' not found.");
                _properties[index] = value;
            }
        }

        public int Count => _properties.Count;

        public IEnumerator<PropertyBase> GetEnumerator()
        {
            return _properties.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public string[] GetPropertyTitles()
        {
            return this.Select(p => p.Title).ToArray();
        }
    }
}