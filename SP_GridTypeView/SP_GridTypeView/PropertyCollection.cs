using System.Collections;
using System.Collections.Generic;

namespace SP_GridTypeView
{
    public class PropertyCollection : IEnumerable<PropertyBase>
    {
        private readonly List<PropertyBase> _properties = new List<PropertyBase>();
        public bool ShowNoColumn { get; set; } = true; // 기본값 true, 필요시 false로 설정

        public void Add(PropertyBase property)
        {
            _properties.Add(property);
        }

        public bool Remove(PropertyBase property)
        {
            return _properties.Remove(property);
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