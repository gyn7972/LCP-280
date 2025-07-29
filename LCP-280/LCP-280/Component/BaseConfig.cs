using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP_GridTypeView.Component
{
    public abstract class BaseConfig
    {
        // 공통 속성
        public string Name { get; set; }
        public bool IsEnabled { get; set; } = true;
        public DateTime LastModified { get; set; } = DateTime.Now;
        
        // 생성자
        protected BaseConfig(string name = null)
        {
            Name = name ?? GetType().Name;
        }

        // 공통 메서드
        public virtual void Reset()
        {
            // 기본값으로 리셋하는 로직
            LastModified = DateTime.Now;
        }

        public virtual bool Validate()
        {
            // Config 값의 유효성을 검사하는 로직
            return true;
        }
        public PropertyCollection propertyBases { get; set; } = new PropertyCollection();
        
    }
}
