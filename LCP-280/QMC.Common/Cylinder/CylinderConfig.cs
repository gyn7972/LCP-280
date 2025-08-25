// CylinderConfig.cs  (C# 7.3 호환)
// Newtonsoft.Json 필요

using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace QMC.Common.Cylinder
{
    /// <summary>단일 실린더 설정</summary>
    public sealed class CylinderConfig
    {
        [JsonProperty("Name")] public string Name;          // UI/식별용
        [JsonProperty("ModuleName")] public string ModuleName;    // DIO 모듈명 (예: "DIO Module1")
        [JsonProperty("ForwardIn")] public string ForwardIn;     // 전진 말단 DI (예: "X20")
        [JsonProperty("BackwardIn")] public string BackwardIn;    // 후진 말단 DI (예: "X21")
        [JsonProperty("ForwardOut")] public string ForwardOut;    // 전진 DO (예: "Y30")
        [JsonProperty("BackwardOut")] public string BackwardOut;   // 후진 DO (예: "Y31")

        [JsonProperty("TimeoutMs")] public int TimeoutMs = 5000;
        [JsonProperty("SettleMs")] public int SettleMs = 50;
        [JsonProperty("Monitoring")] public bool Monitoring = true;
    }

    /// <summary>파일 I/O와 컬렉션 툴을 한 곳에</summary>
    public sealed class CylinderConfigs
    {
        [JsonProperty("Items")]
        public List<CylinderConfig> Items = new List<CylinderConfig>();

        // ---- JSON 옵션 ----
        private static readonly JsonSerializerSettings _settings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore
        };

        // ---- 파일 API ----
        public static CylinderConfigs Load(string path)
        {
            if (!File.Exists(path)) return new CylinderConfigs();
            try
            {
                var json = File.ReadAllText(path);
                var obj = JsonConvert.DeserializeObject<CylinderConfigs>(json, _settings);
                return obj ?? new CylinderConfigs();
            }
            catch
            {
                return new CylinderConfigs();
            }
        }

        public void Save(string path)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path) ?? ".");
            var json = JsonConvert.SerializeObject(this, _settings);
            File.WriteAllText(path, json);
        }

        /// <summary>파일이 없으면 defaults로 생성 후 반환</summary>
        public static CylinderConfigs LoadOrCreate(string path, IList<CylinderConfig> defaults = null)
        {
            if (File.Exists(path)) return Load(path);

            var cfgs = new CylinderConfigs();
            if (defaults != null) cfgs.Items.AddRange(defaults);
            cfgs.Save(path);
            return cfgs;
        }

        // ---- 편의 메서드 ----
        public CylinderConfig Get(string name)
        {
            return Items.Find(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));
        }

        public void Upsert(CylinderConfig cfg)
        {
            var i = Items.FindIndex(x => string.Equals(x.Name, cfg.Name, StringComparison.OrdinalIgnoreCase));
            if (i >= 0) Items[i] = cfg; else Items.Add(cfg);
        }

        public bool Remove(string name)
        {
            var i = Items.FindIndex(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));
            if (i < 0) return false;
            Items.RemoveAt(i);
            return true;
        }
    }
}
