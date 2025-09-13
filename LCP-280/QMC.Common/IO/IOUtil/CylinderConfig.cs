using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.IO;
using System.Text;

namespace QMC.Common.IOUtil
{
    [Serializable]
    public sealed class CylinderConfig : BaseConfig
    {
        [Category("Common"), DisplayName("Simulation")]
        [DefaultValue(false)]
        public bool IsSimulation { get; set; } = false;

        [Category("Common"), DisplayName("Test")]
        [DefaultValue(false)]
        public bool IsTest { get; set; } = false;


        [Category("Operation"), DisplayName("Extend Timeout (ms)")]
        [DefaultValue(5000)]
        public int ExtendTimeout { get; set; } = 1000;

        [Category("Operation"), DisplayName("Retract Timeout (ms)")]
        [DefaultValue(5000)]
        public int RetractTimeout { get; set; } = 1000;

        [Category("Operation"), DisplayName("Settle Delay (ms)")]
        [DefaultValue(50)]
        public int SettleDelay { get; set; } = 10;

        [Category("Operation"), DisplayName("Sensor Retry Count")]
        [DefaultValue(3)]
        public int SensorRetryCount { get; set; } = 3;

        // ===== BaseConfig Hooks =====
        public override void Reset()
        {
            if (ExtendTimeout <= 0) ExtendTimeout = 1000;
            if (RetractTimeout <= 0) RetractTimeout = 1000;
            if (SettleDelay <= 0) SettleDelay = 10;
            if (SensorRetryCount < 0) SensorRetryCount = 3;
        }

        protected override void OnLoaded() => Reset();

        public override bool Validate()
        {
            if (ExtendTimeout <= 0) throw new ArgumentOutOfRangeException(nameof(ExtendTimeout));
            if (RetractTimeout <= 0) throw new ArgumentOutOfRangeException(nameof(RetractTimeout));
            if (SettleDelay <= 0) throw new ArgumentOutOfRangeException(nameof(SettleDelay));
            if (SensorRetryCount < 0) throw new ArgumentOutOfRangeException(nameof(SensorRetryCount));
            return true;
        }

        public override string GetFilePath()
        {
            var dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configs", "Cylinders");
            var file = string.IsNullOrWhiteSpace(Name) ? "default.json" : (Name + ".json");
            return Path.Combine(dir, file);
        }

        protected override JsonSerializerSettings GetJsonSettings() => WriteSettings;

        // ===== 공용 JsonSerializerSettings =====
        private static JsonSerializerSettings WriteSettings => new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Include,
            ObjectCreationHandling = ObjectCreationHandling.Replace
        };

        private static JsonSerializerSettings ReadSettings => new JsonSerializerSettings
        {
            Formatting = Formatting.None,
            NullValueHandling = NullValueHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Populate, // [DefaultValue] 반영
            ObjectCreationHandling = ObjectCreationHandling.Replace
        };

        // ===== 직렬화 API =====
        public string ToJson(bool indented = true)
        {
            return JsonConvert.SerializeObject(
                this,
                indented ? Formatting.Indented : Formatting.None,
                WriteSettings
            );
        }

        public void Save(string filePath, bool indented = true)
        {
            Validate();
            var dir = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            File.WriteAllText(filePath, ToJson(indented), Encoding.UTF8);
        }

        public static CylinderConfig FromJson(string json)
        {
            var obj = JsonConvert.DeserializeObject<CylinderConfig>(json, ReadSettings);
            if (obj == null) throw new InvalidDataException("Invalid JSON for CylinderConfig.");
            obj.Validate();
            return obj;
        }

        public static CylinderConfig Load(string filePath)
        {
            var json = File.ReadAllText(filePath, Encoding.UTF8);
            return FromJson(json);
        }

        /// <summary>
        /// 파일 없으면 생성, 있으면 로드 후 누락 필드를 기본값으로 채우고 즉시 저장(backfill)
        /// </summary>
        public static CylinderConfig LoadOrCreate(string filePath, bool indented = true, bool backfill = true)
        {
            CylinderConfig cfg;
            if (!File.Exists(filePath))
            {
                cfg = new CylinderConfig();
                cfg.Save(filePath, indented);
                return cfg;
            }

            var json = File.ReadAllText(filePath, Encoding.UTF8);
            cfg = FromJson(json);

            if (backfill) cfg.Save(filePath, indented);

            return cfg;
        }

        public static bool TryLoad(string filePath, out CylinderConfig result, out string error)
        {
            result = null;
            error = null;
            try
            {
                result = Load(filePath);
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        public bool TrySave(string filePath, bool indented, out string error)
        {
            error = null;
            try
            {
                Validate();
                var dir = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                File.WriteAllText(filePath, ToJson(indented), Encoding.UTF8);
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        public bool TrySave(string filePath, out string error) => TrySave(filePath, true, out error);
    }
}
