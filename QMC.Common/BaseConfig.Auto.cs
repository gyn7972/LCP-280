using System;

namespace QMC.Common
{
    public abstract partial class BaseConfig
    {
        public virtual ConfigReflectionMapper CreateMapper()
        {
            return new ConfigReflectionMapper(this);
        }

        public virtual bool GetTeachingPositionName(int selIndex, out string name)
        {
            name = "None";
            return false;
        }
    }
}