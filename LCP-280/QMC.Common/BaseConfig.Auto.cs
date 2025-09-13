namespace QMC.Common
{
    public abstract partial class BaseConfig
    {
        public virtual ConfigReflectionMapper CreateMapper()
        {
            return new ConfigReflectionMapper(this);
        }
    }
}