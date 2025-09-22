using QMC.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;

namespace QMC.LCP_280.Process
{
    /// <summary>
    /// 설비 전역 Config (현재 단계: 2개 속성만 저장/로드)
    /// - EquipmentName
    /// - CurrentRecipeName
    /// 나머지(BaseConfig.Name, IsSimulation, TeachingPositions 등)는 제외
    /// </summary>
    [Serializable]
    public class EquipmentConfig : BaseConfig
    {
        #region Meta / Basic (Only these two are persisted now)
        [DefaultValue("LCP-280")]
        public string EquipmentName { get; set; } = "LCP-280";

        [DefaultValue("LCP_RECIPE")]
        public string CurrentRecipeName { get; set; } = "LCP_RECIPE";
        #endregion

        #region Ctor
        public EquipmentConfig()
        {
            Reset();
        }
        #endregion

        #region Base Overrides
        public override void Reset()
        {
            EquipmentName = "LCP-280";
            CurrentRecipeName = "LCP_RECIPE";
        }

        public override bool Validate()
        {
            if (string.IsNullOrWhiteSpace(EquipmentName))
                EquipmentName = "LCP-280";
            if (string.IsNullOrWhiteSpace(CurrentRecipeName))
                CurrentRecipeName = "LCP_RECIPE";
            return true;
        }

        protected override void OnLoaded()
        {
            // 로드 후 필요한 전역(예: Equipment._CurrentRecipeName) 반영이 필요하면 여기서 처리
            try
            {
                // Equipment 클래스에 _CurrentRecipeName 필드/프로퍼티가 public/internal 로 열려있다면 직접 할당 가능
                // Equipment._CurrentRecipeName = CurrentRecipeName;
                // 접근 불가 시 Reflection (예외 무시)
                var eqType = typeof(Equipment);
                var fld = eqType.GetField("_CurrentRecipeName", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                if (fld != null)
                    fld.SetValue(null, CurrentRecipeName);
                else
                {
                    var prop = eqType.GetProperty("CurrentRecipeName", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                    prop?.SetValue(null, CurrentRecipeName, null);
                }
            }
            catch { /* 무시 */ }
        }

        protected override void OnSaving()
        {
            Validate();
        }

        public override string GetFilePath()
        {
            var dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configs");
            return Path.Combine(dir, "EquipmentConfig.json");
        }

        /// <summary>
        /// 두 속성만 직렬화/역직렬화 되도록 ContractResolver 적용
        /// </summary>
        protected override JsonSerializerSettings GetJsonSettings()
        {
            var s = base.GetJsonSettings();
            s.ContractResolver = EquipmentConfigOnlyResolver.Instance;
            return s;
        }
        #endregion

        #region Static Loader
        public static EquipmentConfig LoadOrCreate()
        {
            var cfg = new EquipmentConfig();
            try
            {
                var path = cfg.GetFilePath();
                if (File.Exists(path))
                {
                    cfg.Load();          // 내부에서 필터된 직렬화로 두 속성만 로드
                }
                else
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(path) ?? ".");
                    cfg.Save();
                }

                // 로드 직후 전역 반영(안전)
                try
                {
                    var eqType = typeof(Equipment);
                    var fld = eqType.GetField("_CurrentRecipeName", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                    if (fld != null)
                        fld.SetValue(null, cfg.CurrentRecipeName);
                }
                catch { }
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
            return cfg;
        }
        #endregion

        #region Contract Resolver (Only two properties)
        private sealed class EquipmentConfigOnlyResolver : DefaultContractResolver
        {
            internal static readonly EquipmentConfigOnlyResolver Instance = new EquipmentConfigOnlyResolver();
            private static readonly HashSet<string> _allow =
                new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    nameof(EquipmentConfig.EquipmentName),
                    nameof(EquipmentConfig.CurrentRecipeName)
                };

            protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
            {
                var props = base.CreateProperties(type, memberSerialization);
                if (type == typeof(EquipmentConfig))
                {
                    props = props
                        .Where(p => _allow.Contains(p.PropertyName))
                        .ToList();
                }
                return props;
            }
        }
        #endregion
    }
}