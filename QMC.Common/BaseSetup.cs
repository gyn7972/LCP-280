using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Text;

namespace QMC.Common
{
    public abstract partial class BaseSetup
    {
        // ===== 공통 메타 속성 =====
        [DefaultValue(null)]
        public string Name { get; set; }

        [DefaultValue(true)]
        public bool IsEnabled { get; set; } = true;

        [JsonIgnore]
        public DateTime LastModified { get; set; } = DateTime.Now;

        [JsonIgnore]
        public PropertyPosition PropertyPosition { get; set; } = new PropertyPosition();

        // ===== 생성/초기화 =====
        protected BaseSetup(string name = null)
        {
            Name = name ?? GetType().Name;
            Reset();   // 파생에서 기본값 세팅
        }

        /// <summary>프로퍼티 기본값(구조체/하위객체 초기화 등) - 파생에서 구현</summary>
        public virtual void Reset() { }

        /// <summary>값 유효성 검사 - 저장 전에 호출됨</summary>
        public virtual bool Validate() => true;

        /// <summary>로드 직후 호출(마이그레이션, 캐싱 등)</summary>
        protected virtual void OnLoaded() { }

        /// <summary>저장 직전 호출(가공/정렬 등)</summary>
        protected virtual void OnSaving() { }

        /// <summary>저장 경로 규약(파생에서 필요하면 재정의)</summary>
        public virtual string GetFilePath()
        {
            // 기본: /Configs/{TypeName}/{Name}.json  (기존과 최대한 호환되도록 Name도 유지)
            var dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configs", GetType().Name);
            var file = string.IsNullOrWhiteSpace(Name) ? $"{GetType().Name}.json" : $"{Name}.json";
            return Path.Combine(dir, file);
        }

        /// <summary>Newtonsoft.Json 설정(파생에서 컨버터 추가/교체 허용)</summary>
        protected virtual JsonSerializerSettings GetJsonSettings() =>
            QMC.Common.ConfigStore.DefaultJsonSettings;

        // ===== 저장/로드 (인스턴스 방식) =====
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

            // 기존 파일 백업
            if (File.Exists(filePath))
            {
                File.Copy(filePath, filePath + ".bak", overwrite: true);
            }

            // 유효성 검사 후 저장
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

            // 역직렬화된 값으로 복사(공개 get/set 대상)
            foreach (var p in deserialized.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (!p.CanRead || !p.CanWrite) continue;
                var val = p.GetValue(deserialized);
                p.SetValue(this, val);
            }

            // [DefaultValue] 기반 자동 보강(누락값 채우기; 하위객체도 재귀)
            var changed = QMC.Common.DefaultValueApplier.Apply(this, recurse: true);

            OnLoaded();

            // 보강으로 변경되었으면 자동 저장(스키마 최신화)
            if (changed)
            {
                try
                {
                    SaveToFile(filePath);
                }
                catch (Exception ex)
                {
                    Log.Write(ex);
                }
            }
        }

        // ===== (옵션) UI 연동 훅 =====
        public virtual PropertyCollection GetPropertyCollection()
        {
            var pc = new PropertyCollection();
            var prop = new StringProperty(nameof(Name), Name);
            pc.Add(prop);
            return pc;
        }

        public virtual int ApplyValueFromPropertyCollection(PropertyCollection pc)
        {
            // 필요 시 파생에서 구현
            return 0;
        }
    }
}
