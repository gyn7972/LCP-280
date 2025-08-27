using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace QMC.Common
{
    public static class ConfigStore
    {
        public static readonly JsonSerializerSettings DefaultJsonSettings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore,
            ObjectCreationHandling = ObjectCreationHandling.Replace,
            TypeNameHandling = TypeNameHandling.Auto // 기존 프로젝트 호환
        };

        public static T LoadOrCreate<T>(string path, Func<T> factory, JsonSerializerSettings settings = null)
            where T : class
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path) ?? ".");

            if (!File.Exists(path))
            {
                var fresh = factory();
                DefaultValueApplier.Apply(fresh, recurse: true);
                Save(path, fresh, settings);
                return fresh;
            }

            var json = File.ReadAllText(path, Encoding.UTF8);
            var cfg = JsonConvert.DeserializeObject<T>(json, settings ?? DefaultJsonSettings) ?? factory();
            var changed = DefaultValueApplier.Apply(cfg, recurse: true);
            if (changed) Save(path, cfg, settings);
            return cfg;
        }

        public static void Save<T>(string path, T obj, JsonSerializerSettings settings = null)
        {
            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            // 백업
            if (File.Exists(path))
                File.Copy(path, path + ".bak", overwrite: true);

            var json = JsonConvert.SerializeObject(obj, settings ?? DefaultJsonSettings);
            File.WriteAllText(path, json, Encoding.UTF8);
        }
    }
}
