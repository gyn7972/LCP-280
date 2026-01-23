using Newtonsoft.Json;
using QMC.Common;
using QMC.Common.Component; // Enum
using QMC.Common.Motions;
using QMC.Common.Unit;
using QMC.LCP_280.Process.Component;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace QMC.LCP_280.Process.Unit
{
    public class IndexLoadAlignerConfig : BaseConfig, IPropertyOrderProvider
    {
        internal static class IO 
        { 
            /* Add inputs/outputs later if needed */ 
        }

        #region Hard IO Tables
        [JsonIgnore]
        public HardInputDef[] HardInputs => _hardInputs;
        [JsonIgnore]
        private static readonly HardInputDef[] _hardInputs = Array.Empty<HardInputDef>();

        [JsonIgnore]
        public HardOutputDef[] HardOutputs => _hardOutputs;
        [JsonIgnore]
        private static readonly HardOutputDef[] _hardOutputs = Array.Empty<HardOutputDef>();
        #endregion

        [Category("SetupConfig"), DisplayName("IndexOfMAlign")]
        [DefaultValue(0)]
        public int IndexOfMAlign { get; set; } = 0;

        [Category("SetupConfig"), DisplayName("WaitTime 1Step (ms)")]
        [DefaultValue(0)]
        public int WaitTime1Step { get; set; } = 0;

        [Category("SetupConfig"), DisplayName("WaitTime 2Step (ms)")]
        [DefaultValue(0)]
        public int WaitTime2Step { get; set; } = 0;

        [Category("SetupConfig"), DisplayName("WaitTime 3Step (ms)")]
        [DefaultValue(0)]
        public int WaitTime3Step { get; set; } = 0;

        [JsonIgnore]
        public new List<TeachingPosition> TeachingPositions
        {
            get => base.TeachingPositions;
            set => base.TeachingPositions = value;
        }

        [JsonIgnore]
        private IndexLoadAlignerRecipe _teachingRecipeCache;
        [JsonIgnore]
        private string _teachingRecipeNameCache; // [ADD] 마지막으로 사용한 recipe name

        private const string UnitKey_Teaching = "LoadAlignTeaching";

        [JsonIgnore]
        public IndexLoadAlignerRecipe TeachingRecipe
        {
            get
            {
                try
                {
                    var eq = Equipment.Instance;
                    var er = eq?.EquipmentRecipe;

                    var teachingRecipeName = er?.GetOrLoadUnitTeachingRecipeName(UnitKey_Teaching);
                    if (string.IsNullOrWhiteSpace(teachingRecipeName))
                        teachingRecipeName = "Default_LoadAlignTeaching";

                    // 이름이 바뀌면 캐시 무효화
                    if (!string.Equals(_teachingRecipeNameCache, teachingRecipeName, StringComparison.OrdinalIgnoreCase))
                    {
                        _teachingRecipeNameCache = teachingRecipeName;
                        _teachingRecipeCache = null;
                    }

                    // 1) Config 로컬 캐시 우선
                    if (_teachingRecipeCache != null &&
                        string.Equals(_teachingRecipeCache.Name, teachingRecipeName, StringComparison.OrdinalIgnoreCase))
                        return _teachingRecipeCache;

                    // 2) EquipmentRecipe 캐시 우선 (정식 편입된 프로퍼티)
                    var cached = er?.IndexLoadAlignerTeachingRecipe;
                    if (cached != null &&
                        string.Equals(cached.Name, teachingRecipeName, StringComparison.OrdinalIgnoreCase))
                    {
                        _teachingRecipeCache = cached;
                        return _teachingRecipeCache;
                    }

                    // 3) 폴백: 파일에서 로드/생성 후 EquipmentRecipe 캐시에 주입
                    _teachingRecipeCache = RecipeManager.LoadOrCreate<IndexLoadAlignerRecipe>(teachingRecipeName);
                    try { er?.SetUnitTeachingRecipe(UnitKey_Teaching, _teachingRecipeCache, save: false); } catch { }
                    return _teachingRecipeCache;
                }
                catch
                {
                    try
                    {
                        // 예외 시에도 EquipmentRecipe 캐시 우선
                        var eq = Equipment.Instance;
                        var er = eq?.EquipmentRecipe;
                        var cached = er?.IndexLoadAlignerTeachingRecipe;
                        if (cached != null)
                        {
                            _teachingRecipeCache = cached;
                            _teachingRecipeNameCache = cached.Name;
                            return _teachingRecipeCache;
                        }
                    }
                    catch { }

                    if (_teachingRecipeCache == null)
                        _teachingRecipeCache = RecipeManager.LoadOrCreate<IndexLoadAlignerRecipe>("Default_LoadAlignTeaching");
                    return _teachingRecipeCache;
                }
            }
        }

        public void InvalidateTeachingRecipeCache()
        {
            _teachingRecipeCache = null;
            _teachingRecipeNameCache = null;
        }

        // ===== 기존 호환 API: 내부 구현은 Recipe로 위임 =====
        public override bool GetTeachingPositionName(int selIndex, out string name)
            => TeachingRecipe.GetTeachingPositionName(selIndex, out name);

        public void InitializeDefaultTeachingPositions()
            => TeachingRecipe.InitializeDefaultTeachingPositions(save: true);

        public void SetTeachingPosition(TeachingPosition tp)
            => TeachingRecipe.UpsertFiltered(tp, save: true);

        public TeachingPosition GetTeachingPosition(string name)
            => TeachingRecipe.Get(name);

        public int LoadAndBindAxes(MotionAxisManager axisManager)
        {
            int rc = Load();
            if (rc != 0)
                return rc;

            return TeachingRecipe.LoadAndBindAxes(axisManager);
        }

        public IndexLoadAlignerConfig() : base("IndexLoadAlignerConfig")
        {
        }

        public int Saveconfig()
        {
            try
            {
                return Save();
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
            return -1;
        }

        #region IPropertyOrderProvider 구현 (Category / Property 표시 순서)
        public IDictionary<string, int> GetCategoryOrder()
            => new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
            {
                { "General", 0 },
                { "Common", 1 },
            };

        public IEnumerable<string> GetPropertyOrder()
            => new[]
            {
                "Name",
                "Simulation",
                "SlotPitch (mm)",
                "SlotCount (ea)"
            };
        #endregion
    }
}