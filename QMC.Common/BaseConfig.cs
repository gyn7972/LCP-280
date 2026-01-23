using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using QMC.Common.Component;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace QMC.Common
{
    public abstract partial class BaseConfig : ICloneable
    {
        // ===== 공통 메타 속성 =====
        [Category("General"), DisplayName("Name")]
        [DefaultValue(0)]
        public string Name { get; set; }

        //[DefaultValue(true)]
        //public bool IsEnabled { get; set; } = true;

        [Category("Common"), DisplayName("Simulation")]
        [DefaultValue(false)]
        public bool IsSimulation { get; set; } = false;

        [Category("Common"), DisplayName("DryRun")]
        [DefaultValue(false)]
        public bool IsDryRun { get; set; } = false;

        [JsonIgnore]
        private DateTime LastModified { get; set; } = DateTime.Now;

        //[JsonIgnore]
        //public PropertyPosition PropertyPosition { get; set; } = new PropertyPosition();
        public List<TeachingPosition> TeachingPositions { get; set; } = new List<TeachingPosition>();

        // TeachingPosition 관련 유틸
        public TeachingPosition GetTeachingPosition(string name) => TeachingPositions.FirstOrDefault(p => p.Name == name);

        // ===== (추가) 전역 DryRun 관리 =====
        private static readonly object _instancesLock = new object();
        private static readonly List<BaseConfig> _instances = new List<BaseConfig>();
        public static event Action<bool> GlobalDryRunChanged;
        public static bool? GlobalDryRunOverride { get; set; }
        public static event Action<bool> GlobalSimulationChanged;
        public static bool? GlobalSimulationOverride { get; set; }

        // ===== 생성/초기화 =====
        protected BaseConfig(string name = null)
        {
            Name = name ?? GetType().Name;
            lock (_instancesLock)
            {
                _instances.Add(this);
                // 이미 전역 오버라이드가 있으면 생성 즉시 적용
                if (GlobalDryRunOverride.HasValue)
                    IsDryRun = GlobalDryRunOverride.Value;
                if (GlobalSimulationOverride.HasValue)
                    IsSimulation = GlobalSimulationOverride.Value;
            }
            Reset();

            //Name = name ?? GetType().Name;
            //Reset();   // 파생에서 기본값 세팅
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

        public object Clone()
        {
            var settings = GetJsonSettings();
            var json = JsonConvert.SerializeObject(this, settings);
            return JsonConvert.DeserializeObject(json, GetType(), settings);
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

                // [ADD] 저장 경로/타입 로깅(문제 추적)
                try
                {
                    Log.Write(GetType().Name, "Save", $"path='{path}'");
                }
                catch { }

                SaveToFile(path);
                return 0;
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                return -1;
            }
        }

        public int Save(string filePath)
        {
            try
            {
                OnSaving();
                SaveToFile(filePath);
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

            // 저장물 검증: 타입 문자열로 저장됐는지 확인
            var verify = File.ReadAllText(filePath).Trim();
            if (verify.Length > 0 && verify[0] == '"' && verify.IndexOf('{') == -1)
            {
                // 잘못된 저장 → 정상 객체로 다시 저장
                File.Copy(filePath, filePath + ".typeString", true);
                json = JsonConvert.SerializeObject(this /* 인스턴스 */, Formatting.Indented);
                File.WriteAllText(filePath, json, Encoding.UTF8);
            }
        }

        protected void LoadFromFile(string filePath)
        {
            var settings = GetJsonSettings();

            var json = File.ReadAllText(filePath, Encoding.UTF8);
            var deserialized = JsonConvert.DeserializeObject(json, GetType(), settings);
            if (deserialized == null) 
                return;

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

        /// <summary>
        /// 모든 살아있는 BaseConfig 인스턴스의 IsDryRun 값을 전역 설정.
        /// </summary>
        public static void SetGlobalDryRun(bool value)
        {
            GlobalDryRunOverride = value;
            lock (_instancesLock)
            {
                foreach (var cfg in _instances)
                {
                    try 
                    { 
                        cfg.IsDryRun = value; 
                    }
                    catch (Exception ex)
                    { Log.Write(ex); }
                }
            }
            GlobalDryRunChanged?.Invoke(value);
        }

        public static void SetGlobalSimulation(bool value)
        {
            GlobalSimulationOverride = value;
            lock (_instancesLock)
            {
                foreach (var cfg in _instances)
                {
                    try
                    {
                        cfg.IsSimulation = value;
                    }
                    catch (Exception ex)
                    { Log.Write(ex); }
                }
            }
            GlobalSimulationChanged?.Invoke(value);
        }

        /// <summary>
        /// (옵션) 디스크에 존재하는 JSON 기반 Config 파일들의 IsDryRun 값을 일괄 패치.
        /// </summary>
        public static int PatchAllJsonConfigFilesDryRun(bool value)
        {
            int patched = 0;
            try
            {
                var root = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configs");
                if (!Directory.Exists(root)) return 0;

                var files = Directory.GetFiles(root, "*.json", SearchOption.AllDirectories);
                foreach (var f in files)
                {
                    try
                    {
                        var text = File.ReadAllText(f, Encoding.UTF8);
                        if (string.IsNullOrWhiteSpace(text)) continue;

                        // 단순 치환 (기존 키가 없으면 추가)
                        if (text.IndexOf("\"IsDryRun\"", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            // true/false 토글 단순 정규식 대체는 위험 → JSON 파싱
                            var jobj = Newtonsoft.Json.Linq.JObject.Parse(text);
                            jobj["IsDryRun"] = value;
                            File.WriteAllText(f, jobj.ToString(Formatting.Indented), Encoding.UTF8);
                            patched++;
                        }
                        else
                        {
                            // 최상위 객체라 가정 후 속성 추가
                            var jobj = Newtonsoft.Json.Linq.JObject.Parse(text);
                            jobj["IsDryRun"] = value;
                            File.WriteAllText(f, jobj.ToString(Formatting.Indented), Encoding.UTF8);
                            patched++;
                        }
                    }
                    catch (Exception ex)
                    { Log.Write(ex); }
                }
            }
            catch (Exception ex)
            { Log.Write(ex); }

            return patched;
        }

        public static int PatchAllJsonConfigFilesSimulation(bool value)
        {
            int patched = 0;
            try
            {
                var root = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configs");
                if (!Directory.Exists(root)) return 0;

                var files = Directory.GetFiles(root, "*.json", SearchOption.AllDirectories);
                foreach (var f in files)
                {
                    try
                    {
                        var text = File.ReadAllText(f, Encoding.UTF8);
                        if (string.IsNullOrWhiteSpace(text)) continue;

                        // 단순 치환 (기존 키가 없으면 추가)
                        if (text.IndexOf("\"IsSimulation\"", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            // true/false 토글 단순 정규식 대체는 위험 → JSON 파싱
                            var jobj = Newtonsoft.Json.Linq.JObject.Parse(text);
                            jobj["IsSimulation"] = value;
                            File.WriteAllText(f, jobj.ToString(Formatting.Indented), Encoding.UTF8);
                            patched++;
                        }
                        else
                        {
                            // 최상위 객체라 가정 후 속성 추가
                            var jobj = Newtonsoft.Json.Linq.JObject.Parse(text);
                            jobj["IsSimulation"] = value;
                            File.WriteAllText(f, jobj.ToString(Formatting.Indented), Encoding.UTF8);
                            patched++;
                        }
                    }
                    catch (Exception ex)
                    { Log.Write(ex); }
                }
            }
            catch (Exception ex)
            { Log.Write(ex); }

            return patched;
        }

    }
}
