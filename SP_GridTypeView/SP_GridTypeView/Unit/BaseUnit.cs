using System.Collections.Generic;
using SP_GridTypeView.Component;

namespace SP_GridTypeView.Unit
{
    public abstract class BaseUnit
    {
        // Unit 이름
        public string UnitName { get; set; }

        // 포함된 Component 목록
        public List<BaseComponent> Components { get; } = new List<BaseComponent>();

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