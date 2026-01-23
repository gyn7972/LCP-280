using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.IO;

namespace QMC.Common.GEMSecs
{
    [Serializable]
    public sealed class XLinkGemServiceConfig : BaseConfig
    {
        public enum HsmsMode
        {
            Active = 0,
            Passive = 1
        }

        [Category("GEM"), DisplayName("Enable")]
        [DefaultValue(true)]
        public bool Enable { get; set; } = true;

        [Category("GEM"), DisplayName("HSMS Mode")]
        [DefaultValue(HsmsMode.Active)]
        public HsmsMode Mode { get; set; } = HsmsMode.Active;

        [Category("GEM"), DisplayName("IP Address")]
        [DefaultValue("127.0.0.1")]
        public string Ip { get; set; } = "127.0.0.1";

        [Category("GEM"), DisplayName("Port")]
        [DefaultValue((short)5000)]
        public short Port { get; set; } = 5000;

        [Category("GEM"), DisplayName("Device ID")]
        [DefaultValue((short)0)]
        public short DevId { get; set; } = 0;

        [Category("GEM"), DisplayName("Model Name (MDLN)")]
        [DefaultValue("")]
        public string ModelName { get; set; } = "";

        [Category("GEM"), DisplayName("Software Revision (SOFTREV)")]
        [DefaultValue("")]
        public string SoftRev { get; set; } = "";

        [Category("Timer"), DisplayName("T3 Timeout (sec)")]
        [DefaultValue((short)45)]
        public short T3 { get; set; } = 45;

        [Category("Timer"), DisplayName("T5 Timeout (sec)")]
        [DefaultValue((short)20)]
        public short T5 { get; set; } = 20;

        [Category("Timer"), DisplayName("T6 Timeout (sec)")]
        [DefaultValue((short)5)]
        public short T6 { get; set; } = 5;

        [Category("Timer"), DisplayName("T7 Timeout (sec)")]
        [DefaultValue((short)10)]
        public short T7 { get; set; } = 10;

        [Category("Timer"), DisplayName("T8 Timeout (sec)")]
        [DefaultValue((short)5)]
        public short T8 { get; set; } = 5;

        [Category("Timer"), DisplayName("LinkTest Interval (sec)")]
        [DefaultValue((short)60)]
        public short LinkTestInterval { get; set; } = 60;

        [Category("Timer"), DisplayName("Establish Timeout (sec)")]
        [DefaultValue((short)5)]
        public short EstablishTimeout { get; set; } = 5;

        [Category("Timer"), DisplayName("Time Format (0=12Byte,1=16Byte)")]
        [DefaultValue((short)1)]
        public short TimeFormatDigits { get; set; } = 1;

        [Category("Logging"), DisplayName("Enable Log")]
        [DefaultValue(true)]
        public bool LogEnabled { get; set; } = true;

        [Category("Logging"), DisplayName("Log Path")]
        [DefaultValue("")]
        public string LogPath { get; set; } = "";

        [Category("Logging"), DisplayName("Log Prefix")]
        [DefaultValue("GEM")]
        public string LogPrefix { get; set; } = "GEM";

        [Category("Logging"), DisplayName("Log Keep Days")]
        [DefaultValue((short)30)]
        public short LogKeepDays { get; set; } = 30;

        public XLinkGemServiceConfig() : base("XLinkGEM")
        {
        }

        public override void Reset()
        {
            if (string.IsNullOrWhiteSpace(Name)) Name = "XLinkGEM";

            if (string.IsNullOrWhiteSpace(Ip)) Ip = "127.0.0.1";
            if (Port <= 0) Port = 5000;
            if (DevId < 0) DevId = 0;

            if (T3 <= 0) T3 = 45;
            if (T5 <= 0) T5 = 20;
            if (T6 <= 0) T6 = 5;
            if (T7 <= 0) T7 = 10;
            if (T8 <= 0) T8 = 5;

            if (LinkTestInterval < 0) LinkTestInterval = 60;

            if (EstablishTimeout <= 0) EstablishTimeout = 5;

            // 0 = 12, 1 = 16, 2 = 14
            if (TimeFormatDigits != 0 && TimeFormatDigits != 1 && TimeFormatDigits != 2) 
                TimeFormatDigits = 1;

            if (LogKeepDays < 0) LogKeepDays = 30;
            if (string.IsNullOrWhiteSpace(LogPrefix)) LogPrefix = "GEM";

            if (LogEnabled && string.IsNullOrWhiteSpace(LogPath))
                LogPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs", "GEM");
        }

        protected override void OnLoaded() => Reset();

        public override bool Validate()
        {
            if (string.IsNullOrWhiteSpace(Ip)) throw new ArgumentOutOfRangeException(nameof(Ip));
            if (Port <= 0) throw new ArgumentOutOfRangeException(nameof(Port));
            if (DevId < 0) throw new ArgumentOutOfRangeException(nameof(DevId));

            if (T3 <= 0) throw new ArgumentOutOfRangeException(nameof(T3));
            if (T5 <= 0) throw new ArgumentOutOfRangeException(nameof(T5));
            if (T6 <= 0) throw new ArgumentOutOfRangeException(nameof(T6));
            if (T7 <= 0) throw new ArgumentOutOfRangeException(nameof(T7));
            if (T8 <= 0) throw new ArgumentOutOfRangeException(nameof(T8));
            
            if (EstablishTimeout <= 0) 
                throw new ArgumentOutOfRangeException(nameof(EstablishTimeout));
            
            // 0=12, 1=16, 2=14
            if (TimeFormatDigits != 0 && TimeFormatDigits != 1 && TimeFormatDigits != 2) 
                throw new ArgumentOutOfRangeException(nameof(TimeFormatDigits));

            if (LogEnabled)
            {
                if (string.IsNullOrWhiteSpace(LogPath)) throw new ArgumentOutOfRangeException(nameof(LogPath));
                if (string.IsNullOrWhiteSpace(LogPrefix)) throw new ArgumentOutOfRangeException(nameof(LogPrefix));
                if (LogKeepDays < 0) throw new ArgumentOutOfRangeException(nameof(LogKeepDays));
            }

            return true;
        }

        public override string GetFilePath()
        {
            var dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configs", "GEM");
            var file = string.IsNullOrWhiteSpace(Name) ? "XLinkGEM.json" : (Name + ".json");
            return Path.Combine(dir, file);
        }

        protected override JsonSerializerSettings GetJsonSettings() => WriteSettings;

        private static JsonSerializerSettings WriteSettings => new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Include,
            ObjectCreationHandling = ObjectCreationHandling.Replace
        };

        public static XLinkGemServiceConfig LoadOrCreate(string name = "XLinkGEM")
        {
            var cfg = new XLinkGemServiceConfig { Name = string.IsNullOrWhiteSpace(name) ? "XLinkGEM" : name };

            int rc = cfg.Load();
            if (rc != 0)
            {
                cfg.Reset();
                cfg.Save();
            }

            return cfg;
        }
    }
}