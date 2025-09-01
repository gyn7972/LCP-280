using QMC.Common.Component;
using QMC.Common.Motion;
using System.Collections.Generic;

namespace QMC.Common.Unit
{
    public abstract class BaseUnit
    {
        // Unit 이름
        public string UnitName { get; set; }
        // 포함된 Component 목록
        public List<BaseComponent> Components { get; } = new List<BaseComponent>();
        public BaseConfig Config { get; internal set; }

        protected BaseUnit(string unitName = null)
        {
            UnitName = unitName;
        }
        
        public virtual void AddComponents() { }
        // Unit 공통 동작 메서드
        public virtual void OnRun() { }
        public virtual void OnStop() { }
    }
}