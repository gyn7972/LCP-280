using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Text;

// 코드 구조
// BaseRecipe.cs → 공통 Recipe 저장/로드/검증 베이스 (BaseConfig 유사 구조)
// MeasurementRecipe.cs → 실제 장비에서 사용할 Recipe 클래스
// MeasurementKey.cs → 검사 항목 정의
// RecipeManager.cs → Recipe를 Load/Save/관리하는 유틸리티

namespace QMC.Common
{
    /// <summary>
    /// 공통 Recipe 베이스: BaseConfig 와 동일한 패턴(백업, 기본값 채움, Clone 등)
    /// </summary>
    public abstract partial class BaseRecipe : ICloneable
    {
        // ===== 공통 메타 속성 =====
        [DefaultValue(null)]
        public string Name { get; set; }

        [DefaultValue(true)]
        public bool IsEnabled { get; set; } = true;

        [JsonIgnore]
        private DateTime LastModified { get; set; } = DateTime.Now;

        // ===== 생성/초기화 =====
        protected BaseRecipe(string name = null)
        {
            Name = name ?? GetType().Name;
            Reset();
        }

        /// <summary>프로퍼티 기본값/구조 초기화</summary>
        public virtual void Reset() { }
        /// <summary>저장 전 유효성 검사</summary>
        public virtual bool Validate() => true;
        /// <summary>로드 직후 마이그레이션/보정</summary>
        protected virtual void OnLoaded() { }
        /// <summary>저장 직전 정렬/계산 등</summary>
        protected virtual void OnSaving() { }

        /// <summary>파일 경로 규약: /Recipes/{TypeName}/{Name}.json</summary>
        public virtual string GetFilePath()
        {
            var dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Recipes", GetType().Name);
            var file = string.IsNullOrWhiteSpace(Name) ? $"{GetType().Name}.json" : $"{Name}.json";
            return Path.Combine(dir, file);
        }

        /// <summary>Json 설정 (파생에서 컨버터 추가/교체 가능)</summary>
        protected virtual JsonSerializerSettings GetJsonSettings() =>
            QMC.Common.ConfigStore.DefaultJsonSettings;

        /// <summary>깊은 복제 (직렬화 기반)</summary>
        public object Clone()
        {
            var settings = GetJsonSettings();
            var json = JsonConvert.SerializeObject(this, settings);
            return JsonConvert.DeserializeObject(json, GetType(), settings);
        }

        // ===== 인스턴스 저장/로드 =====
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

            if (!Validate())
                throw new InvalidOperationException($"{GetType().Name} validation failed.");

            var json = JsonConvert.SerializeObject(this, settings);
            File.WriteAllText(filePath, json, Encoding.UTF8);
            LastModified = DateTime.Now;

            // 저장물 검증: 잘못 직렬화(타입 문자열만) 되었는지 확인
            var verify = File.ReadAllText(filePath).Trim();
            if (verify.Length > 0 && verify[0] == '"' && verify.IndexOf('{') == -1)
            {
                // 잘못된 저장 → 객체 전체로 재저장(Indented)
                File.Copy(filePath, filePath + ".typeString", true);
                json = JsonConvert.SerializeObject(this, Formatting.Indented);
                File.WriteAllText(filePath, json, Encoding.UTF8);
            }
        }

        protected void LoadFromFile(string filePath)
        {
            var settings = GetJsonSettings();
            var json = File.ReadAllText(filePath, Encoding.UTF8);
            var deserialized = JsonConvert.DeserializeObject(json, GetType(), settings);
            if (deserialized == null) return;

            // 공개 get/set 프로퍼티 값 복사
            foreach (var p in deserialized.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (!p.CanRead || !p.CanWrite) continue;
                var val = p.GetValue(deserialized);
                p.SetValue(this, val);
            }

            // [DefaultValue] 기반 누락 채움 및 하위 객체 보강
            var changed = QMC.Common.DefaultValueApplier.Apply(this, recurse: true);
            OnLoaded();

            // 스키마 보강으로 값 변경 시 자동 저장
            if (changed)
            {
                try { SaveToFile(filePath); }
                catch (Exception ex) { Log.Write(ex); }
            }
        }

        // ===== (옵션) UI 연동 훅 =====
        public virtual PropertyCollection GetPropertyCollection()
        {
            var pc = new PropertyCollection();
            pc.Add(new StringProperty(nameof(Name), Name));
            pc.Add(new BoolProperty(nameof(IsEnabled), IsEnabled));
            pc.IsInputParameter = true;
            return pc;
        }

        public virtual int ApplyValueFromPropertyCollection(PropertyCollection pc)
        {
            if (pc == null) return -1;
            try
            {
                Name = pc.GetValue<string>(nameof(Name));
                IsEnabled = pc.GetValue<bool>(nameof(IsEnabled));
                return 0;
            }
            catch { return -1; }
        }
    }
}
