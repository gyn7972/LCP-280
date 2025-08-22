using System;
using QMC.Common;
using QMC.Common.Unit;

namespace QMC.Common.Component
{
    public abstract class BaseComponent
    {
        public string Name { get; set; }
        public BaseUnit ParentUnit { get; set; }
        public BaseConfig Config { get; set; }

        protected BaseComponent(string name = null)
        {
            Name = name;
        }

        /// <summary>
        /// (선택) 하드웨어 축들을 연결/초기화 할 때 호출. 필요 없으면 무시.
        /// 파생 클래스에서 axes 해석 방식 정의.
        /// </summary>
        /// <param name="axes">연결할 IMotionAxis 배열</param>
        public virtual void InitializeAxes(params IMotionAxis[] axes)
        {
            // 기본: 아무 것도 하지 않음
        }

        /// <summary>
        /// (선택) Config 값을 기반으로 PositionItem(PropertyPosition 등) 재구성.
        /// InitializeAxes 내부나 ReloadFromConfig 에서 호출 가능.
        /// </summary>
        protected virtual void BuildPositionItemsFromConfig()
        {
            // no-op
        }

        /// <summary>
        /// (선택) 런타임(편집된 Property) 값을 Config 로 반영.
        /// </summary>
        public virtual void SyncToConfig()
        {
            // no-op
        }

        /// <summary>
        /// (선택) Config 값 변경/리셋 후 런타임 객체 재생성.
        /// </summary>
        public virtual void ReloadFromConfig()
        {
            // 기본 구현: 다시 Build 호출
            BuildPositionItemsFromConfig();
        }
    }
}