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

        [JsonIgnore]
        public new List<TeachingPosition> TeachingPositions
        {
            get => base.TeachingPositions;
            set => base.TeachingPositions = value;
        }

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
        private IndexLoadAlignerRecipe _teachingRecipeCache;
        [JsonIgnore]
        private string _teachingRecipeNameCache; // [ADD] 마지막으로 사용한 recipe name

        [JsonIgnore]
        public IndexLoadAlignerRecipe TeachingRecipe
        {
            get
            {
                try
                {
                    var eq = Equipment.Instance;
                    var er = eq?.EquipmentRecipe;

                    // [CHG] MeasurementRecipe가 들고있는 UnitRecipeName을 단일 규약으로 사용
                    var teachingRecipeName = er?.GetOrLoadIndexChipProbeControllerTeachingRecipeName();
                    if (string.IsNullOrWhiteSpace(teachingRecipeName))
                        teachingRecipeName = "Default_ProbeTeaching";

                    // [ADD] CurrentRecipe가 나중에 세팅되어 이름이 바뀌면 캐시 무효화
                    if (!string.Equals(_teachingRecipeNameCache, teachingRecipeName, StringComparison.OrdinalIgnoreCase))
                    {
                        _teachingRecipeNameCache = teachingRecipeName;
                        _teachingRecipeCache = null;
                    }

                    if (_teachingRecipeCache != null &&
                        string.Equals(_teachingRecipeCache.Name, teachingRecipeName, StringComparison.OrdinalIgnoreCase))
                        return _teachingRecipeCache;

                    var cached = er?.IndexChipProbeControllerTeachingRecipe;
                    if (cached != null &&
                        string.Equals(cached.Name, teachingRecipeName, StringComparison.OrdinalIgnoreCase))
                    {
                        _teachingRecipeCache = cached;
                        return _teachingRecipeCache;
                    }

                    _teachingRecipeCache = RecipeManager.LoadOrCreate<IndexLoadAlignerRecipe>(teachingRecipeName);
                    try { er?.SetIndexChipProbeControllerTeachingRecipe(_teachingRecipeCache); } catch { }
                    return _teachingRecipeCache;
                }
                catch
                {
                    if (_teachingRecipeCache == null)
                        _teachingRecipeCache = RecipeManager.LoadOrCreate<IndexLoadAlignerRecipe>("Default_ProbeTeaching");
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
        // Category 순서: Common → Cassette
        public IDictionary<string, int> GetCategoryOrder()
            => new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
            {
                { "General", 0 },   // Name 속성 (Category 없음) 정렬 위치 지정
                { "Common", 1 },
            };

        // Property 순서: (DisplayName 또는 PropertyName)
        // BaseConfig: "Simulation" (IsSimulation)
        // Cassette: "SlotPitch (mm)", "SlotCount (ea)"
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