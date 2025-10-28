using System;

namespace QMC.Common
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class ConfigIgnoreAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Property)]
    public sealed class ConfigOrderAttribute : Attribute
    {
        public int Order { get; }
        public ConfigOrderAttribute(int order) { Order = order; }
    }
}