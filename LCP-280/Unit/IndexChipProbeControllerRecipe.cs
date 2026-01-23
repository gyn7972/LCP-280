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
    public sealed class IndexChipProbeControllerRecipe : QMC.Common.BaseRecipe, QMC.LCP_280.Process.Unit.FormConfig.IHasTeachingPositions
    {
        public enum TeachingPositionName
        {
            Top_Index1_Contact,
            Top_Index1_Ready,
            Top_Index2_Contact,
            Top_Index2_Ready,
            Top_Index3_Contact,
            Top_Index3_Ready,
            Top_Index4_Contact,
            Top_Index4_Ready,
            Top_Index5_Contact,
            Top_Index5_Ready,
            Top_Index6_Contact,
            Top_Index6_Ready,
            Top_Index7_Contact,
            Top_Index7_Ready,
            Top_Index8_Contact,
            Top_Index8_Ready,
            Bottom_Index1_Contact,
            Bottom_Index1_Ready,
            Bottom_Index2_Contact,
            Bottom_Index2_Ready,
            Bottom_Index3_Contact,
            Bottom_Index3_Ready,
            Bottom_Index4_Contact,
            Bottom_Index4_Ready,
            Bottom_Index5_Contact,
            Bottom_Index5_Ready,
            Bottom_Index6_Contact,
            Bottom_Index6_Ready,
            Bottom_Index7_Contact,
            Bottom_Index7_Ready,
            Bottom_Index8_Contact,
            Bottom_Index8_Ready,
            GripperX_Ready,
            GripperX_Clamp,
            GripperX_Index_Contact,
            SphereZ_Ready,
            SphereZ_Measure,
            SafetyZone
        }

        [JsonIgnore]
        private static readonly Dictionary<TeachingPositionName, string[]> _axisMap =
            new Dictionary<TeachingPositionName, string[]>
            {
                { TeachingPositionName.Top_Index1_Contact,        new [] { AxisNames.ProbeZ } },
                { TeachingPositionName.Top_Index1_Ready,     new [] { AxisNames.ProbeZ } },
                { TeachingPositionName.Top_Index2_Contact,        new [] { AxisNames.ProbeZ } },
                { TeachingPositionName.Top_Index2_Ready,     new [] { AxisNames.ProbeZ } },
                { TeachingPositionName.Top_Index3_Contact,        new [] { AxisNames.ProbeZ } },
                { TeachingPositionName.Top_Index3_Ready,     new [] { AxisNames.ProbeZ } },
                { TeachingPositionName.Top_Index4_Contact,        new [] { AxisNames.ProbeZ } },
                { TeachingPositionName.Top_Index4_Ready,     new [] { AxisNames.ProbeZ } },
                { TeachingPositionName.Top_Index5_Contact,        new [] { AxisNames.ProbeZ } },
                { TeachingPositionName.Top_Index5_Ready,     new [] { AxisNames.ProbeZ } },
                { TeachingPositionName.Top_Index6_Contact,        new [] { AxisNames.ProbeZ } },
                { TeachingPositionName.Top_Index6_Ready,     new [] { AxisNames.ProbeZ } },
                { TeachingPositionName.Top_Index7_Contact,        new [] { AxisNames.ProbeZ } },
                { TeachingPositionName.Top_Index7_Ready,     new [] { AxisNames.ProbeZ } },
                { TeachingPositionName.Top_Index8_Contact,        new [] { AxisNames.ProbeZ } },
                { TeachingPositionName.Top_Index8_Ready,     new [] { AxisNames.ProbeZ } },

                { TeachingPositionName.Bottom_Index1_Contact,     new [] { AxisNames.ProbeCardX, AxisNames.ProbeCardY, AxisNames.ProbeCardZ } },
                { TeachingPositionName.Bottom_Index1_Ready,  new [] { AxisNames.ProbeCardX, AxisNames.ProbeCardY, AxisNames.ProbeCardZ } },
                { TeachingPositionName.Bottom_Index2_Contact,     new [] { AxisNames.ProbeCardX, AxisNames.ProbeCardY, AxisNames.ProbeCardZ } },
                { TeachingPositionName.Bottom_Index2_Ready,  new [] { AxisNames.ProbeCardX, AxisNames.ProbeCardY, AxisNames.ProbeCardZ } },
                { TeachingPositionName.Bottom_Index3_Contact,     new [] { AxisNames.ProbeCardX, AxisNames.ProbeCardY, AxisNames.ProbeCardZ } },
                { TeachingPositionName.Bottom_Index3_Ready,  new [] { AxisNames.ProbeCardX, AxisNames.ProbeCardY, AxisNames.ProbeCardZ } },
                { TeachingPositionName.Bottom_Index4_Contact,     new [] { AxisNames.ProbeCardX, AxisNames.ProbeCardY, AxisNames.ProbeCardZ } },
                { TeachingPositionName.Bottom_Index4_Ready,  new [] { AxisNames.ProbeCardX, AxisNames.ProbeCardY, AxisNames.ProbeCardZ } },
                { TeachingPositionName.Bottom_Index5_Contact,     new [] { AxisNames.ProbeCardX, AxisNames.ProbeCardY, AxisNames.ProbeCardZ } },
                { TeachingPositionName.Bottom_Index5_Ready,  new [] { AxisNames.ProbeCardX, AxisNames.ProbeCardY, AxisNames.ProbeCardZ } },
                { TeachingPositionName.Bottom_Index6_Contact,     new [] { AxisNames.ProbeCardX, AxisNames.ProbeCardY, AxisNames.ProbeCardZ } },
                { TeachingPositionName.Bottom_Index6_Ready,  new [] { AxisNames.ProbeCardX, AxisNames.ProbeCardY, AxisNames.ProbeCardZ } },
                { TeachingPositionName.Bottom_Index7_Contact,     new [] { AxisNames.ProbeCardX, AxisNames.ProbeCardY, AxisNames.ProbeCardZ } },
                { TeachingPositionName.Bottom_Index7_Ready,  new [] { AxisNames.ProbeCardX, AxisNames.ProbeCardY, AxisNames.ProbeCardZ } },
                { TeachingPositionName.Bottom_Index8_Contact,     new [] { AxisNames.ProbeCardX, AxisNames.ProbeCardY, AxisNames.ProbeCardZ } },
                { TeachingPositionName.Bottom_Index8_Ready,  new [] { AxisNames.ProbeCardX, AxisNames.ProbeCardY, AxisNames.ProbeCardZ } },

                { TeachingPositionName.GripperX_Ready,       new [] { AxisNames.GripperX } },
                { TeachingPositionName.GripperX_Clamp,       new [] { AxisNames.GripperX } },
                { TeachingPositionName.GripperX_Index_Contact,    new [] { AxisNames.ProbeZ } },
                { TeachingPositionName.SphereZ_Ready,        new [] { AxisNames.SphereZ } },
                { TeachingPositionName.SphereZ_Measure,         new [] { AxisNames.SphereZ } },
                { TeachingPositionName.SafetyZone,           new [] { AxisNames.ProbeCardZ, AxisNames.ProbeZ } },
            };

        // 파일 저장 대상 (key = enum 문자열)
        [Category("Teaching"), DisplayName("PositionsMap")]
        public Dictionary<string, TeachingPosition> TeachingPositionMap { get; set; }
            = new Dictionary<string, TeachingPosition>(StringComparer.OrdinalIgnoreCase);

        // UI/기존 호환 (파일 저장 제외)
        [JsonIgnore]
        [Category("Teaching"), DisplayName("Positions")]
        public List<TeachingPosition> TeachingPositions { get; set; } = new List<TeachingPosition>();

        public IndexChipProbeControllerRecipe() : this(null)
        {

        }
        public IndexChipProbeControllerRecipe(string name = null) : base(name)
        {

        }

        public string GetBottomContactName(int index0Based)
        {
            return GetIndexedName("Bottom", index0Based, "Contact");
        }

        public string GetBottomReadyName(int index0Based)
        {
            return GetIndexedName("Bottom", index0Based, "Ready");
        }

        public string GetTopContactName(int index0Based)
        {
            return GetIndexedName("Top", index0Based, "Contact");
        }

        public string GetTopReadyName(int index0Based)
        {
            return GetIndexedName("Top", index0Based, "Ready");
        }

        private static string GetIndexedName(string prefix, int index0Based, string suffix)
        {
            if (index0Based < 0 || index0Based >= 8)
                throw new ArgumentOutOfRangeException(nameof(index0Based), "index0Based must be 0..7");

            // enum은 1-base 이름 규칙을 사용함: Bottom_Index1_Contact ...
            int idx1 = index0Based + 1;
            string enumName = $"{prefix}_Index{idx1}_{suffix}";

            TeachingPositionName en;
            if (!Enum.TryParse(enumName, out en) || !Enum.IsDefined(typeof(TeachingPositionName), en))
                throw new InvalidOperationException($"Invalid teaching enum name: '{enumName}'");

            return en.ToString();
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

            return TeachingPositions?.FirstOrDefault(p => p != null && string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase));
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
            return new[] { AxisNames.ProbeZ, AxisNames.ProbeCardX, AxisNames.ProbeCardY, AxisNames.ProbeCardZ, AxisNames.SphereZ };
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

            if (!TeachingPositionMapHelper.IsEnumKey<TeachingPositionName>(tp.Name))
                return;

            // 저장 전 정규화(없으면 생성)
            TeachingPositionMapHelper.NormalizeByEnum<TeachingPositionName>(
                TeachingPositionMap,
                TeachingPositions,
                posKey => GetAxisNamesForPosition(posKey),
                removeNonEnumKeys: true,
                applyAxisMapping: false);

            // 축 필터링 적용
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
                TeachingPositionMap[tp.Name] = tp;
            }
            else
            {
                exist.AxisPositions = filtered;
                exist.Description = tp.Description;
                exist.ExtraInfo = FilterExtraInfo(tp.ExtraInfo);
            }

            // 후처리(축 매핑 + UI 리스트 반영)
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
        //    Log.Write(nameof(IndexChipProbeControllerRecipe), nameof(LoadAndBindAxes),
        //            $"Name='{Name}', file='{filePath}', existedBefore={existed}, LoadRc={rc}");

        //    if (rc != 0)
        //    {
        //        // 파일이 없으면 현재 Name으로 신규 생성
        //        InitializeDefaultTeachingPositions(save: true);

        //        Log.Write(nameof(IndexChipProbeControllerRecipe), nameof(LoadAndBindAxes),
        //            $"Default teaching created & saved. Name='{Name}', file='{filePath}'");
        //    }

        //    // 로드 성공/실패와 무관하게, Map 기준으로 누락 보강 + UI 리스트 동기화
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
            // 저장 대상은 Map이므로 Map 기준 검증
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

            // 저장 시점에 enum key 기준으로 보장(누락 시 생성)
            TeachingPositionMapHelper.NormalizeByEnum<TeachingPositionName>(
                TeachingPositionMap,
                TeachingPositions,
                posKey => GetAxisNamesForPosition(posKey),
                removeNonEnumKeys: true,
                applyAxisMapping: true);

            // 런타임 캐시 제거
            foreach (var kv in TeachingPositionMap)
            {
                var tp = kv.Value;
                if (tp?.Axes != null)
                    tp.Axes.Clear();
            }
        }

        // =========================
        // Path Policy (NEW)
        // =========================

        private const string FilePrefix = "IndexChipProbeControllerRecipe";
        //IndexChipProbeControllerRecipe

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
        // /Recipes/{CurrentRecipeName}/IndexChipProbeControllerRecipe{Name}.json
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

        // 이전 버전 1) /Recipes/IndexChipProbeControllerRecipe/{CurrentRecipeName}/{Name}.json
        private string GetLegacyFilePath_TypeAndRecipeFolder()
        {
            var safeCurrentRecipe = GetCurrentRecipeFolderName();
            var safeName = SanitizeFilePart(string.IsNullOrWhiteSpace(Name) ? "default" : Name);

            var root = AppDomain.CurrentDomain.BaseDirectory;
            var dir = System.IO.Path.Combine(root, "Recipes", typeof(IndexChipProbeControllerRecipe).Name, safeCurrentRecipe);
            return System.IO.Path.Combine(dir, safeName + ".json");
        }

        // 이전 버전 2) /Recipes/IndexChipProbeControllerRecipe/{Name}.json
        private string GetLegacyFilePath_TypeFolderOnly()
        {
            var safeName = SanitizeFilePart(string.IsNullOrWhiteSpace(Name) ? "default" : Name);

            var root = AppDomain.CurrentDomain.BaseDirectory;
            var dir = System.IO.Path.Combine(root, "Recipes", typeof(IndexChipProbeControllerRecipe).Name);
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
                var legacyObj = JsonConvert.DeserializeObject<IndexChipProbeControllerRecipe>(json);
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

            Log.Write(nameof(IndexChipProbeControllerRecipe), nameof(LoadAndBindAxes),
                    $"Name='{Name}', file='{filePath}', existedBefore={existed}, LoadRc={rc}");

            if (rc != 0)
            {
                InitializeDefaultTeachingPositions(save: true);

                Log.Write(nameof(IndexChipProbeControllerRecipe), nameof(LoadAndBindAxes),
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