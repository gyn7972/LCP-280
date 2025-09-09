using Newtonsoft.Json;
using QMC.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;

namespace QMC.LCP_280.Process.Component
{
    [Serializable]
    public sealed class WaferDataConfig : BaseConfig
    {
        public List<CarrierData> Carriers { get; set; } = new List<CarrierData>();

        public override void Reset()
        {
            Carriers.Clear();
        }

        protected override void OnLoaded()
        {
            if (Carriers == null) Carriers = new List<CarrierData>();
        }

        public override bool Validate()
        {
            return true;
        }

        public override string GetFilePath()
        {
            var dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configs", "WaferData");
            var file = "WaferDataConfig.json";
            return Path.Combine(dir, file);
        }

        protected override JsonSerializerSettings GetJsonSettings()
        {
            return new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Include,
                ObjectCreationHandling = ObjectCreationHandling.Replace
            };
        }

        // ===== Static API =====
        public static WaferDataConfig FromJson(string json)
        {
            var obj = JsonConvert.DeserializeObject<WaferDataConfig>(json, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Populate,
                ObjectCreationHandling = ObjectCreationHandling.Replace
            });
            if (obj == null)
                throw new InvalidDataException("Invalid JSON for WaferDataConfig.");
            obj.Validate();
            return obj;
        }

        public static WaferDataConfig Load(string filePath)
        {
            var json = File.ReadAllText(filePath);
            return FromJson(json);
        }

        public static WaferDataConfig LoadOrCreate(string filePath, bool backfill = true)
        {
            if (!File.Exists(filePath))
            {
                var cfg = new WaferDataConfig();
                cfg.Save();   // BaseConfig.Save() 사용
                return cfg;
            }

            var json = File.ReadAllText(filePath);
            var obj = FromJson(json);

            if (backfill)
                obj.Save();

            return obj;
        }
    }
}
