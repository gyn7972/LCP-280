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
    }
}