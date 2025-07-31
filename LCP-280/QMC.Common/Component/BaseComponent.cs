using QMC.Common.Unit;

namespace QMC.Common.Component
{
    public abstract class BaseComponent
    {
        // 공통적으로 사용할 수 있는 속성 예시
        public string Name { get; set; }

        // Unit 참조 추가
        public BaseUnit ParentUnit { get; set; }

        public BaseConfig Config { get; set; }

        // 생성자
        protected BaseComponent(string name = null)
        {
            Name = name;
        }

        // 필요시 공통 메서드 추가 가능
    }
}