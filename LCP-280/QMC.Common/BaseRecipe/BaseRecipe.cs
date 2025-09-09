using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Text;

//코드 구조

//BaseRecipe.cs → 공통 Recipe 저장/로드/검증 베이스
//MeasurementRecipe.cs → 실제 장비에서 사용할 Recipe 클래스
//MeasurementKey.cs → 검사 항목 정의
//RecipeManager.cs → Recipe를 Load/Save/관리하는 유틸리티

namespace QMC.Common
{
    public abstract class BaseRecipe
    {
        // ===== 공통 메타 속성 =====
        [DefaultValue(null)]
        public string Name { get; set; }

        [DefaultValue(true)]
        public bool IsEnabled { get; set; } = true;

        [JsonIgnore]
        public DateTime LastModified { get; private set; } = DateTime.Now;

        // ===== 생성/초기화 =====
        protected BaseRecipe(string name = null)
        {
            Name = name ?? GetType().Name;
            Reset();
        }

        public virtual void Reset() { }
        public virtual bool Validate() => true;
        protected virtual void OnLoaded() { }
        protected virtual void OnSaving() { }

        /// <summary>저장 경로 규약: /Recipes/{TypeName}/{Name}.json</summary>
        public virtual string GetFilePath()
        {
            var dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Recipes", GetType().Name);
            var file = string.IsNullOrWhiteSpace(Name) ? $"{GetType().Name}.json" : $"{Name}.json";
            return Path.Combine(dir, file);
        }

        protected virtual JsonSerializerSettings GetJsonSettings() =>
            QMC.Common.ConfigStore.DefaultJsonSettings;

        // ===== 저장/로드 (인스턴스) =====
        public int Save()
        {
            try
            {
                OnSaving();
                var path = GetFilePath();
                SaveToFile(path);
                return 0;
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                return -1;
            }
        }

        public int Load()
        {
            try
            {
                var path = GetFilePath();
                if (!File.Exists(path)) return -1;
                LoadFromFile(path);
                return 0;
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                return -1;
            }
        }

        // ===== 내부 구현 =====
        protected void SaveToFile(string filePath)
        {
            var settings = GetJsonSettings();

            Directory.CreateDirectory(Path.GetDirectoryName(filePath) ?? ".");

            if (File.Exists(filePath))
                File.Copy(filePath, filePath + ".bak", overwrite: true);

            if (!Validate())
                throw new InvalidOperationException($"{GetType().Name} validation failed.");

            var json = JsonConvert.SerializeObject(this, settings);
            File.WriteAllText(filePath, json, Encoding.UTF8);
            LastModified = DateTime.Now;
        }

        protected void LoadFromFile(string filePath)
        {
            var settings = GetJsonSettings();

            var json = File.ReadAllText(filePath, Encoding.UTF8);
            var deserialized = JsonConvert.DeserializeObject(json, GetType(), settings);
            if (deserialized == null) return;

            foreach (var p in deserialized.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (!p.CanRead || !p.CanWrite) continue;
                var val = p.GetValue(deserialized);
                p.SetValue(this, val);
            }

            var changed = QMC.Common.DefaultValueApplier.Apply(this, recurse: true);
            OnLoaded();

            if (changed)
            {
                try { SaveToFile(filePath); }
                catch (Exception ex) { Log.Write(ex); }
            }
        }
    }
}
