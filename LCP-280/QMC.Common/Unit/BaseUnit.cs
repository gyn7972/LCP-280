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

        /// <summary>
        /// Motion 연결 직후, Unit Run 이전에 각 Component 의 InitializeAxes 를 호출하기 위한 훅.
        /// 기본 구현은 아무 것도 하지 않음. 파생 Unit 에서 필요한 축을 provider 로부터 얻어 호출.
        /// </summary>
        /// <param name="provider">축을 제공하는 프로바이더</param>
        public virtual void InitializeUnitAxes(IMotionAxisProvider provider) { }
    }
}