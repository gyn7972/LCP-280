using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using QMC.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Linq.Expressions; // LINQ 추가

namespace QMC.LCP_280.Process
{
    [Serializable]
    public class EquipmentConfig : BaseConfig
    {
        #region Meta / Basic
        [JsonProperty("CurrentRecipeName")]
        [DefaultValue("LCP_RECIPE")]
        public string CurrentRecipeName { get; set; } = "LCP_RECIPE";

        [JsonIgnore]
        [Category("EquipmentName"), DisplayName("Equipment Name")]
        [DefaultValue("LCP-280")]
        public string EquipmentName { get; set; } = "LCP-280";

        [Category("EquipmentId"), DisplayName("Equipment Id")]
        [JsonProperty("EquipmentId")]
        [DefaultValue("EqpID")]
        public string EquipmentId { get; set; } = "EqpID";

        // IsDryRun/IsSimulation은 BaseConfig에 있거나(상속) EquipmentConfig에 있을 수 있습니다.
        // 상속된 경우도 직렬화 허용 목록에서 필터링해 저장되도록 처리합니다.
        [Category("Path"), DisplayName("Log Path")]
        [JsonProperty("LogPath")]
        public string LogPath { get; set; }

        [Category("Path"), DisplayName("Result Path")]
        [JsonProperty("ResultPath")]
        public string ResultPath { get; set; }

        [Category("Network"), DisplayName("Network Mode")]
        [JsonProperty("NetworkMode")]
        public int NetworkMode { get; set; }

        [Category("Path"), DisplayName("InspectionMap Path")]
        [JsonProperty("InspectionMapPath")]
        public string InspectionMapPath { get; set; }

        [Category("Path"), DisplayName("TXTResult Path")]
        [JsonProperty("TXTResultPath")]
        public string TXTResultPath { get; set; }

        [Category("Path"), DisplayName("PRDResult Path")]
        [JsonProperty("PRDResultPath")]
        public string PRDResultPath { get; set; }

        [Category("Path"), DisplayName("SUMResult Path")]
        [JsonProperty("SUMResultPath")]
        public string SUMResultPath { get; set; }

        [Category("Path"), DisplayName("BinResult Path")]
        [JsonProperty("BinResultPath")]
        public string BinResultPath { get; set; }

        [Category("Path"), DisplayName("WAFResult Path")]
        [JsonProperty("WAFResultPath")]
        public string WAFResultPath { get; set; }

        [Category("Path"), DisplayName("DBDataServer Path")]
        [JsonProperty("DBDataServerPath")]
        public string DBDataServerPath { get; set; }

        [Category("Path"), DisplayName("ProductionInfo Path")]
        [JsonProperty("ProductionInfoPath")]
        public string ProductionInfoPath { get; set; }

        [Category("MapMatchMode"), DisplayName("MapMatch Mode")]
        [JsonProperty("MapMatchMode")]
        public bool MapMatchMode { get; set; } = false;

        #endregion

        #region Ctor
        public EquipmentConfig() { Reset(); }
        #endregion

        #region Base Overrides
        public override void Reset()
        {
            EquipmentName = "LCP-280";
            CurrentRecipeName = "LCP_RECIPE";
            IsDryRun = false;
            IsSimulation = false;

            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            LogPath = Path.Combine(baseDir, "Log");
            ResultPath = Path.Combine(baseDir, "Result");
        }

        public override bool Validate()
        {
            if (string.IsNullOrWhiteSpace(EquipmentName))
                EquipmentName = "LCP-280";
            if (string.IsNullOrWhiteSpace(CurrentRecipeName))
                CurrentRecipeName = "LCP_RECIPE";

            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            if (string.IsNullOrWhiteSpace(LogPath))
                LogPath = Path.Combine(baseDir, "Log");
            if (string.IsNullOrWhiteSpace(ResultPath))
                ResultPath = Path.Combine(baseDir, "Result");
            return true;
        }

        protected override void OnLoaded()
        {
            try
            {
                var eqType = typeof(Equipment);
                var fld = eqType.GetField("_CurrentRecipeName",
                    System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                if (fld != null) fld.SetValue(null, CurrentRecipeName);
                else
                {
                    var prop = eqType.GetProperty("CurrentRecipeName",
                        System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                    prop?.SetValue(null, CurrentRecipeName, null);
                }
            }
            catch { }
        }

        protected override void OnSaving()
        {
            Validate();
            //EquipmentName
            //EquipmentId

            TryEnsureDirectory(LogPath);
            TryEnsureDirectory(ResultPath);
            //NetworkMode
            TryEnsureDirectory(InspectionMapPath);
            TryEnsureDirectory(TXTResultPath);
            TryEnsureDirectory(PRDResultPath);
            TryEnsureDirectory(SUMResultPath);
            TryEnsureDirectory(BinResultPath);
            TryEnsureDirectory(WAFResultPath);
            TryEnsureDirectory(DBDataServerPath);
            TryEnsureDirectorySafeForFile(ProductionInfoPath);
            //TryExpression(MapMatchMode);
        }

        public override string GetFilePath()
        {
            var dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configs");
            return Path.Combine(dir, "EquipmentConfig.json");
        }

        protected override JsonSerializerSettings GetJsonSettings()
        {
            var s = base.GetJsonSettings();
            s.ContractResolver = EquipmentConfigOnlyResolver.Instance;
            return s;
        }
        #endregion

        #region Static Loader
        // 정적 LoadOrCreate 제공 (EquipmentRecipe 등에서 사용)
        public static EquipmentConfig LoadOrCreate()
        {
            var cfg = new EquipmentConfig();
            try
            {
                var path = cfg.GetFilePath();
                var dir = Path.GetDirectoryName(path);
                if (!string.IsNullOrWhiteSpace(dir))
                    Directory.CreateDirectory(dir);

                if (File.Exists(path))
                {
                    cfg.Load(); // BaseConfig.LoadFromFile 경유
                }
                else
                {
                    cfg.Validate();
                    cfg.Save();
                }

                // 로드 후 필요한 후처리
                cfg.OnLoaded();
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
            return cfg;
        }
        #endregion

        #region Contract Resolver (Allow-list)
        private sealed class EquipmentConfigOnlyResolver : DefaultContractResolver
        {
            internal static readonly EquipmentConfigOnlyResolver Instance = new EquipmentConfigOnlyResolver();

            // C# 실제 속성명 기준 허용 목록
            private static readonly HashSet<string> _allow =
                new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    nameof(EquipmentConfig.EquipmentName),
                    nameof(EquipmentConfig.EquipmentId),
                    nameof(EquipmentConfig.CurrentRecipeName), // JsonIgnore라도 허용, 단 JsonIgnore 우선
                    "IsDryRun",  // BaseConfig 상속 케이스 고려
                    "IsSimulation",
                    nameof(EquipmentConfig.LogPath),
                    nameof(EquipmentConfig.ResultPath),
                    nameof(EquipmentConfig.NetworkMode),
                    nameof(EquipmentConfig.InspectionMapPath),
                    nameof(EquipmentConfig.TXTResultPath),
                    nameof(EquipmentConfig.PRDResultPath),
                     nameof(EquipmentConfig.SUMResultPath),
                    nameof(EquipmentConfig.BinResultPath),
                    nameof(EquipmentConfig.WAFResultPath),
                    nameof(EquipmentConfig.DBDataServerPath),
                    nameof(EquipmentConfig.ProductionInfoPath),
                    nameof(EquipmentConfig.MapMatchMode),

                    // JsonProperty("EquipmentName")로 쓰고 있어서 혼동 방지 (PropertyName 기준 필터 대비)
                    "EquipmentName",
                    "EquipmentId",
                    "LogPath",
                    "ResultPath",
                    "NetworkMode",
                    "InspectionMapPath",
                    "TXTResultPath",
                    "PRDResultPath",
                    "SUMResultPath",
                    "BinResultPath",
                    "WAFResultPath",
                    "DBDataServerPath",
                    "ProductionInfoPath",
                    "MapMatchMode",
                };

            protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
            {
                var props = base.CreateProperties(type, memberSerialization);

                if (type == typeof(EquipmentConfig))
                {
                    // [FIX] UnderlyingName + PropertyName 둘 다 allow-list로 평가
                    props = props
                        .Where(p =>
                            _allow.Contains(p.UnderlyingName) ||
                            _allow.Contains(p.PropertyName))
                        .ToList();
                }

                return props;
            }
        }
        #endregion

        #region Helpers
        private static void TryEnsureDirectory(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return;
            try { Directory.CreateDirectory(path); } catch { }
        }

        private static void TryEnsureDirectorySafeForFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath)) return;
            try
            {
                var dir = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrWhiteSpace(dir))
                    Directory.CreateDirectory(dir);
            }
            catch { }
        }
        #endregion
    }
}