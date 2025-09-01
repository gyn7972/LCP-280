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
        public void Add(string title, object obj = null)
        {
            if (obj == null)
            {
                TitleOnlyProperty titleProp = new TitleOnlyProperty(title);
                _properties.Add(titleProp);
            }
            else if (obj is bool value)
            {
                BoolProperty boolProp = new BoolProperty(title, value);
                _properties.Add(boolProp);
            }
            else if (obj is int intValue)
            {
                IntProperty intProp = new IntProperty(title, intValue);
                _properties.Add(intProp);
            }
            else if (obj is string strValue)
            {
                StringProperty stringProp = new StringProperty(title, strValue);
                _properties.Add(stringProp);
            }
            else if (obj.GetType().IsEnum)
            {
                ComboBoxProperty comboBoxProp = new ComboBoxProperty(title, obj.ToString(), System.Enum.GetNames(obj.GetType()).ToList());
                _properties.Add(comboBoxProp);
            }
            else
            {
                throw new System.ArgumentException("Unsupported object type");
            }
        }

        public T GetValue<T>(string title)
        {
            var prop = _properties.FirstOrDefault(p => p.Title == title);
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
                else
                    throw new InvalidCastException($"Property '{title}' is not of type IntProperty.");
            }
            else if (type == typeof(string))
            {
                if (prop is StringProperty)
                    return (T)prop.Value;
                else if (prop is ComboBoxProperty)
                    return (T)prop.Value;
                else
                    throw new InvalidCastException($"Property '{title}' is not of type StringProperty or ComboBoxProperty.");
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

        public PropertyBase this[int index]
        {
            get { return _properties[index]; }
            set { _properties[index] = value; }
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
    }
}