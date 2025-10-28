using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace QMC.Common.Component
{
    /// <summary>
    /// Category(그룹 헤더) 자체의 순서를 제어하고 싶을 때 부여.
    /// (미지정시 1000)
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class CategoryOrderAttribute : Attribute
    {
        public int Order { get; }
        public CategoryOrderAttribute(int order) => Order = order;
    }
    public sealed class DisplayOrderAttribute : Attribute
    {
        public int Order { get; }
        public DisplayOrderAttribute(int order) => Order = order;
    }

    /// <summary>
    /// Config별로 한 번에 순서/카테고리 순서를 제공하고 싶을 때 구현.
    /// (Attribute 보다 “낮은” 우선순위로 적용, 즉 Attribute 있으면 그것이 우선)
    /// </summary>
    public interface IPropertyOrderProvider
    {
        /// <summary>
        /// 속성(표시명 또는 실제 PropertyName) 나열 순서.
        /// 앞에서부터 0,1,2… 순번 부여. 누락된 것은 뒤로 밀리고 기존 규칙 적용.
        /// </summary>
        IEnumerable<string> GetPropertyOrder();

        /// <summary>
        /// Category 이름 → 우선순위 (작을수록 위).
        /// </summary>
        IDictionary<string, int> GetCategoryOrder();
    }
}