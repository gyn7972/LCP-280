using System;

namespace QMC.Common.Unit
{
    // Strongly-typed base unit that exposes a typed Config while staying compatible with BaseUnit API.
    public abstract class BaseUnit<TConfig> : BaseUnit where TConfig : BaseConfig
    {
        // Strongly-typed Config. Keeps BaseUnit.Config synchronized.
        public new TConfig Config { get; private set; }

        protected BaseUnit(TConfig config) : base(ResolveUnitName(config))
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            config.Validate();
            Config = config;
            base.Config = config;
        }

        protected void SetConfig(TConfig config)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            config.Validate();
            Config = config;
            base.Config = config;
        }

        private static string ResolveUnitName(TConfig config)
        {
            if (config != null && !string.IsNullOrWhiteSpace(config.Name)) return config.Name;
            var tname = typeof(TConfig).Name;
            return tname != null && tname.EndsWith("Config") ? tname.Substring(0, tname.Length - 6) : (tname ?? "Unit");
        }
    }
}
