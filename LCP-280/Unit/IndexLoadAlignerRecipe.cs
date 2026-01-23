using Newtonsoft.Json;
using QMC.Common;
using QMC.Common.Component;
using QMC.Common.Motions;
using QMC.LCP_280.Process.Component;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QMC.LCP_280.Process.Unit
{
    [Serializable]
    public sealed class IndexLoadAlignerRecipe
        : QMC.Common.BaseRecipe, QMC.LCP_280.Process.Unit.FormConfig.IHasTeachingPositions
    {
        public enum TeachingPositionName
        {
            AlignZ_Index1_Contact,
            AlignZ_Index1_Ready,
            AlignZ_Index2_Contact,
            AlignZ_Index2_Ready,
            AlignZ_Index3_Contact,
            AlignZ_Index3_Ready,
            AlignZ_Index4_Contact,
            AlignZ_Index4_Ready,
            AlignZ_Index5_Contact,
            AlignZ_Index5_Ready,
            AlignZ_Index6_Contact,
            AlignZ_Index6_Ready,
            AlignZ_Index7_Contact,
            AlignZ_Index7_Ready,
            AlignZ_Index8_Contact,
            AlignZ_Index8_Ready,
            AlignT_Foward,
            AlignT_Backward,
            AlignT_Ready,
            SafetyZone
        }

        /// <summary>
        /// Position 별 허용 축 매핑 (필요 시 일부 Position에서 특정 축만 사용하도록 조정)
        /// </summary>
        [JsonIgnore]
        private static readonly Dictionary<TeachingPositionName, string[]> _axisMap = new Dictionary<TeachingPositionName, string[]>
        {
            { TeachingPositionName.AlignZ_Index1_Contact,    new [] { AxisNames.IndexZ } },
            { TeachingPositionName.AlignZ_Index1_Ready, new [] { AxisNames.IndexZ } },
            { TeachingPositionName.AlignZ_Index2_Contact,    new [] { AxisNames.IndexZ } },
            { TeachingPositionName.AlignZ_Index2_Ready, new [] { AxisNames.IndexZ } },
            { TeachingPositionName.AlignZ_Index3_Contact,    new [] { AxisNames.IndexZ } },
            { TeachingPositionName.AlignZ_Index3_Ready, new [] { AxisNames.IndexZ } },
            { TeachingPositionName.AlignZ_Index4_Contact,    new [] { AxisNames.IndexZ } },
            { TeachingPositionName.AlignZ_Index4_Ready, new [] { AxisNames.IndexZ } },
            { TeachingPositionName.AlignZ_Index5_Contact,    new [] { AxisNames.IndexZ } },
            { TeachingPositionName.AlignZ_Index5_Ready, new [] { AxisNames.IndexZ } },
            { TeachingPositionName.AlignZ_Index6_Contact,    new [] { AxisNames.IndexZ } },
            { TeachingPositionName.AlignZ_Index6_Ready, new [] { AxisNames.IndexZ } },
            { TeachingPositionName.AlignZ_Index7_Contact,    new [] { AxisNames.IndexZ } },
            { TeachingPositionName.AlignZ_Index7_Ready, new [] { AxisNames.IndexZ } },
            { TeachingPositionName.AlignZ_Index8_Contact,    new [] { AxisNames.IndexZ } },
            { TeachingPositionName.AlignZ_Index8_Ready, new [] { AxisNames.IndexZ } },
            { TeachingPositionName.AlignT_Foward,       new [] { AxisNames.AlignT } },
            { TeachingPositionName.AlignT_Backward,     new [] { AxisNames.AlignT } },
            { TeachingPositionName.AlignT_Ready,        new [] { AxisNames.AlignT } },
            { TeachingPositionName.SafetyZone,          new [] { AxisNames.IndexZ } },
        };

        /// <summary>
        /// 파일 저장 대상 (JSON Object의 property key = TeachingPositionName 문자열)
        /// </summary>
        [Category("Teaching"), DisplayName("PositionsMap")]
        public Dictionary<string, TeachingPosition> TeachingPositionMap { get; set; }
            = new Dictionary<string, TeachingPosition>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// UI/기존 코드 호환 (파일 저장 제외)
        /// </summary>
        [JsonIgnore]
        [Category("Teaching"), DisplayName("Positions")]
        public List<TeachingPosition> TeachingPositions { get; set; } = new List<TeachingPosition>();

        public IndexLoadAlignerRecipe() : this(null)
        {

        }
        public IndexLoadAlignerRecipe(string name = null) : base(name)
        {

        }

        public bool GetTeachingPositionName(int selIndex, out string name)
        {
            if (Enum.GetNames(typeof(TeachingPositionName)).Length <= selIndex)
            {
                name = "None";
                return false;
            }
            var tpn = (TeachingPositionName)selIndex;
            name = tpn.ToString();
            return true;
        }

        public TeachingPosition Get(string name)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                TeachingPosition v;
                if (TeachingPositionMap != null && TeachingPositionMap.TryGetValue(name, out v))
                    return v;
            }

            return TeachingPositions?.FirstOrDefault
                (p => p != null && string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase));
        }

        public IReadOnlyList<string> GetAxisNamesForPosition(string positionName)
        {
            if (string.IsNullOrWhiteSpace(positionName))
                return new List<string>();

            TeachingPositionName en;
            if (Enum.TryParse(positionName, out en))
            {
                string[] arr;
                if (_axisMap.TryGetValue(en, out arr)) return arr;
            }

            // 백워드 호환 기본값
            return new[] { AxisNames.IndexZ, AxisNames.AlignT };
        }

        public void ApplyAxisMapping()
        {
            TeachingPositionMapHelper.ApplyAxisMapping(
                TeachingPositionMap,
                posKey => GetAxisNamesForPosition(posKey));
        }

        public void InitializeDefaultTeachingPositions(bool save = true)
        {
            TeachingPositionMapHelper.NormalizeByEnum<TeachingPositionName>(
                TeachingPositionMap,
                TeachingPositions,
                posKey => GetAxisNamesForPosition(posKey),
                removeNonEnumKeys: true,
                applyAxisMapping: true);

            if (save)
                RecipeManager.Save(this);
        }

        private static Dictionary<string, object> FilterExtraInfo(Dictionary<string, object> src)
        {
            var dst = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            if (src == null) return dst;

            foreach (var kv in src)
            {
                if (string.IsNullOrWhiteSpace(kv.Key)) continue;
                var v = kv.Value;
                if (v == null) { dst[kv.Key] = null; continue; }

                var t = v.GetType();
                if (t.IsEnum || v is string || v is bool ||
                    v is int || v is long || v is float || v is double || v is decimal)
                {
                    dst[kv.Key] = v;
                }
                else
                {
                    // 복잡 객체는 문자열로만 남김(폭증 방지)
                    dst[kv.Key] = v.ToString();
                }
            }

            return dst;
        }

        public void UpsertFiltered(TeachingPosition tp, bool save = true)
        {
            if (tp == null || string.IsNullOrWhiteSpace(tp.Name))
                return;

            // key는 enum 문자열만 허용
            if (!TeachingPositionMapHelper.IsEnumKey<TeachingPositionName>(tp.Name))
                return;

            // 먼저: map을 enum 기준으로 정규화해서 key 누락 시 생성
            TeachingPositionMapHelper.NormalizeByEnum<TeachingPositionName>(
                TeachingPositionMap,
                TeachingPositions,
                posKey => GetAxisNamesForPosition(posKey),
                removeNonEnumKeys: true,
                applyAxisMapping: false);

            var allowed = new HashSet<string>(GetAxisNamesForPosition(tp.Name), StringComparer.OrdinalIgnoreCase);

            var filtered = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
            foreach (var axis in allowed)
            {
                double v = 0.0;
                if (tp.AxisPositions != null)
                    tp.AxisPositions.TryGetValue(axis, out v);
                filtered[axis] = v;
            }

            TeachingPosition exist;
            if (!TeachingPositionMap.TryGetValue(tp.Name, out exist) || exist == null)
            {
                tp.AxisPositions = filtered;
                tp.ExtraInfo = FilterExtraInfo(tp.ExtraInfo);
                tp.Name = tp.Name; // key 보정(그대로)
                TeachingPositionMap[tp.Name] = tp;
            }
            else
            {
                exist.AxisPositions = filtered;
                exist.Description = tp.Description;
                exist.ExtraInfo = FilterExtraInfo(tp.ExtraInfo);
            }

            // 후처리: 축 매핑 + UI 리스트 반영
            TeachingPositionMapHelper.NormalizeByEnum<TeachingPositionName>(
                TeachingPositionMap,
                TeachingPositions,
                posKey => GetAxisNamesForPosition(posKey),
                removeNonEnumKeys: true,
                applyAxisMapping: true);

            if (save)
                RecipeManager.Save(this);
        }

        //public int LoadAndBindAxes(MotionAxisManager axisManager)
        //{
        //    // Recipe는 BaseRecipe.Load()가 파일 없으면 -1 반환할 수 있음.
        //    // 없으면 기본 생성 후 저장까지 해두는 쪽이 Teaching UX가 안정적임.
        //    var filePath = GetFilePath();
        //    var existed = System.IO.File.Exists(filePath);

        //    var rc = Load();
        //    Log.Write(nameof(IndexLoadAlignerRecipe), nameof(LoadAndBindAxes),
        //            $"Name='{Name}', file='{filePath}', existedBefore={existed}, LoadRc={rc}");

        //    if (rc != 0)
        //    {
        //        InitializeDefaultTeachingPositions(save: true);

        //        Log.Write(nameof(IndexLoadAlignerRecipe), nameof(LoadAndBindAxes),
        //            $"Default teaching created & saved. Name='{Name}', file='{filePath}'");
        //    }

        //    // 로드 후에도 enum 기준으로 정규화(누락 자동 생성/정렬/축 매핑/리스트 반영)
        //    TeachingPositionMapHelper.NormalizeByEnum<TeachingPositionName>(
        //        TeachingPositionMap,
        //        TeachingPositions,
        //        posKey => GetAxisNamesForPosition(posKey),
        //        removeNonEnumKeys: true,
        //        applyAxisMapping: true);

        //    if (axisManager != null)
        //    {
        //        foreach (var tp in TeachingPositions)
        //            tp.BindAxes(axisManager, "Unit");
        //    }

        //    return 0;
        //}

        public override void Reset()
        {
            if (TeachingPositionMap == null)
                TeachingPositionMap = new Dictionary<string, TeachingPosition>(StringComparer.OrdinalIgnoreCase);

            if (TeachingPositions == null)
                TeachingPositions = new List<TeachingPosition>();
        }

        public override bool Validate()
        {
            // Map 기준 검증
            if (TeachingPositionMap == null) return true;
            return TeachingPositionMap.All(kv => !string.IsNullOrWhiteSpace(kv.Key) && kv.Value != null);
        }

        protected override void OnLoaded()
        {
            base.OnLoaded();

            TeachingPositionMapHelper.NormalizeByEnum<TeachingPositionName>(
                TeachingPositionMap,
                TeachingPositions,
                posKey => GetAxisNamesForPosition(posKey),
                removeNonEnumKeys: true,
                applyAxisMapping: true);
        }

        protected override void OnSaving()
        {
            base.OnSaving();

            // 저장 시점에 enum key 기준으로 보장(누락 시 신규 생성 포함)
            TeachingPositionMapHelper.NormalizeByEnum<TeachingPositionName>(
                TeachingPositionMap,
                TeachingPositions,
                posKey => GetAxisNamesForPosition(posKey),
                removeNonEnumKeys: true,
                applyAxisMapping: true);

            foreach (var kv in TeachingPositionMap)
            {
                var tp = kv.Value;
                if (tp?.Axes != null)
                    tp.Axes.Clear(); // 런타임 캐시 제거(파일에 저장되면 안됨)
            }
        }

        // =========================
        // Path Policy (NEW)
        // =========================

        private const string FilePrefix = "IndexLoadAlignerRecipe";
        //IndexLoadAlignerRecipe

        private static string SanitizeFilePart(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "Default";

            foreach (var c in System.IO.Path.GetInvalidFileNameChars())
                name = name.Replace(c, '_');

            return name.Trim();
        }

        private static string GetCurrentRecipeFolderName()
        {
            string currentRecipeName = null;
            try
            {
                currentRecipeName = Equipment.Instance?.EquipmentRecipe?.CurrentRecipeName;
            }
            catch { currentRecipeName = null; }

            if (string.IsNullOrWhiteSpace(currentRecipeName))
                currentRecipeName = "Default";

            return SanitizeFilePart(currentRecipeName);
        }

        // [CHG] 최종 경로:
        // /Recipes/{CurrentRecipeName}/IndexLoadAlignerRecipe{Name}.json
        public override string GetFilePath()
        {
            var safeCurrentRecipe = GetCurrentRecipeFolderName();

            // Name 자체에도 invalid char가 올 수 있으니 파일 파트로 sanitize
            var safeName = SanitizeFilePart(string.IsNullOrWhiteSpace(Name) ? "default" : Name);

            var root = AppDomain.CurrentDomain.BaseDirectory;
            var dir = System.IO.Path.Combine(root, "Recipes", safeCurrentRecipe, FilePrefix);

            try { System.IO.Directory.CreateDirectory(dir); } catch { }

            //var file = $"{FilePrefix}{safeName}.json";
            var file = $"{safeName}.json";
            return System.IO.Path.Combine(dir, file);
        }

        // ----- Legacy Paths (migrate 지원) -----

        // 이전 버전 1) /Recipes/IndexLoadAlignerRecipe/{CurrentRecipeName}/{Name}.json
        private string GetLegacyFilePath_TypeAndRecipeFolder()
        {
            var safeCurrentRecipe = GetCurrentRecipeFolderName();
            var safeName = SanitizeFilePart(string.IsNullOrWhiteSpace(Name) ? "default" : Name);

            var root = AppDomain.CurrentDomain.BaseDirectory;
            var dir = System.IO.Path.Combine(root, "Recipes", typeof(InputDieTransferRecipe).Name, safeCurrentRecipe);
            return System.IO.Path.Combine(dir, safeName + ".json");
        }

        // 이전 버전 2) /Recipes/IndexLoadAlignerRecipe/{Name}.json
        private string GetLegacyFilePath_TypeFolderOnly()
        {
            var safeName = SanitizeFilePart(string.IsNullOrWhiteSpace(Name) ? "default" : Name);

            var root = AppDomain.CurrentDomain.BaseDirectory;
            var dir = System.IO.Path.Combine(root, "Recipes", typeof(IndexLoadAlignerRecipe).Name);
            return System.IO.Path.Combine(dir, safeName + ".json");
        }

        // [CHG] 현재 경로 우선 로드, 없으면 레거시 경로들에서 읽어와 현재 위치로 저장(마이그레이션)
        public int LoadFromCurrentRecipeFolderOrMigrate()
        {
            try
            {
                var newPath = GetFilePath();
                if (System.IO.File.Exists(newPath))
                    return Load();

                var legacy1 = GetLegacyFilePath_TypeAndRecipeFolder();
                var legacy2 = GetLegacyFilePath_TypeFolderOnly();

                string legacyPath = null;
                if (System.IO.File.Exists(legacy1)) legacyPath = legacy1;
                else if (System.IO.File.Exists(legacy2)) legacyPath = legacy2;

                if (legacyPath == null)
                    return -1;

                var json = System.IO.File.ReadAllText(legacyPath);
                var legacyObj = JsonConvert.DeserializeObject<IndexLoadAlignerRecipe>(json);
                if (legacyObj == null)
                    return -1;

                TeachingPositionMap = legacyObj.TeachingPositionMap ?? new Dictionary<string, TeachingPosition>(StringComparer.OrdinalIgnoreCase);
                TeachingPositions = legacyObj.TeachingPositions ?? new List<TeachingPosition>();
                //Offsets = legacyObj.Offsets ?? new Dictionary<string, (double t, double pickZ, double placeZ)>();

                ApplyAxisMapping();
                Save(); // 새 포맷으로 저장

                return 0;
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
            return -1;
        }

        // [CHG] LoadAndBindAxes에서 새 로더 사용 (파일 없으면 기본 생성 로직 유지)
        public int LoadAndBindAxes(MotionAxisManager axisManager)
        {
            var filePath = GetFilePath();
            var existed = System.IO.File.Exists(filePath);

            var rc = LoadFromCurrentRecipeFolderOrMigrate();
            if (rc != 0)
                rc = Load();

            Log.Write(nameof(IndexLoadAlignerRecipe), nameof(LoadAndBindAxes),
                    $"Name='{Name}', file='{filePath}', existedBefore={existed}, LoadRc={rc}");

            if (rc != 0)
            {
                InitializeDefaultTeachingPositions(save: true);

                Log.Write(nameof(IndexLoadAlignerRecipe), nameof(LoadAndBindAxes),
                    $"Default teaching created & saved. Name='{Name}', file='{filePath}'");
            }

            TeachingPositionMapHelper.NormalizeByEnum<TeachingPositionName>(
                TeachingPositionMap,
                TeachingPositions,
                posKey => GetAxisNamesForPosition(posKey),
                removeNonEnumKeys: true,
                applyAxisMapping: true);

            if (axisManager != null)
            {
                foreach (var tp in TeachingPositions)
                    tp.BindAxes(axisManager, "Unit");
            }

            return 0;
        }
    }
}
