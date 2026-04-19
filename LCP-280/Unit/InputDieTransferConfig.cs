using Newtonsoft.Json;
using QMC.Common;
using QMC.Common.Component;
using QMC.Common.Motions;
using QMC.Common.Unit;
using QMC.LCP_280.Process.Component;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace QMC.LCP_280.Process.Unit
{
    public class InputDieTransferConfig : BaseConfig, IPropertyOrderProvider
    {
        /// <summary>장치 IO 명칭 모음</summary>
        internal static class IO
        {
            // Inputs (X032~X037)
            public const string AIR_TANK_PRESSURE = "LEFT TOOL AIR TANK PRESSURE CHECK";      // X032
            public const string VAC_TANK_PRESSURE = "LEFT TOOL VACUUM TANK PRESSURE CHECK";   // X033
            public const string ARM1_FLOW = "LEFT TOOL ARM 1 FLOW CHECK";                     // X034
            public const string ARM2_FLOW = "LEFT TOOL ARM 2 FLOW CHECK";                     // X035
            public const string ARM3_FLOW = "LEFT TOOL ARM 3 FLOW CHECK";                     // X036
            public const string ARM4_FLOW = "LEFT TOOL ARM 4 FLOW CHECK";                     // X037

            // Outputs (Y039~Y050)
            public const string ARM1_VAC = "LEFT ARM 1 VACUUM"; // Y039
            public const string ARM2_VAC = "LEFT ARM 2 VACUUM"; // Y040
            public const string ARM3_VAC = "LEFT ARM 3 VACUUM"; // Y041
            public const string ARM4_VAC = "LEFT ARM 4 VACUUM"; // Y042
            public const string ARM1_BLOW = "LEFT ARM 1 BLOW";   // Y043
            public const string ARM2_BLOW = "LEFT ARM 2 BLOW";   // Y044
            public const string ARM3_BLOW = "LEFT ARM 3 BLOW";   // Y045
            public const string ARM4_BLOW = "LEFT ARM 4 BLOW";   // Y046
            public const string ARM1_VENT = "LEFT ARM 1 VENT";   // Y047
            public const string ARM2_VENT = "LEFT ARM 2 VENT";   // Y048
            public const string ARM3_VENT = "LEFT ARM 3 VENT";   // Y049
            public const string ARM4_VENT = "LEFT ARM 4 VENT";   // Y050
        }

        
        #region Hard IO Tables
        [JsonIgnore]
        public HardInputDef[] HardInputs => _hardInputs;
        private static readonly HardInputDef[] _hardInputs = new[]
        {
            new HardInputDef { No = 1, Name = IO.AIR_TANK_PRESSURE, Disp = "X032" },
            new HardInputDef { No = 2, Name = IO.VAC_TANK_PRESSURE, Disp = "X033" },
            new HardInputDef { No = 3, Name = IO.ARM1_FLOW,         Disp = "X034" },
            new HardInputDef { No = 4, Name = IO.ARM2_FLOW,         Disp = "X035" },
            new HardInputDef { No = 5, Name = IO.ARM3_FLOW,         Disp = "X036" },
            new HardInputDef { No = 6, Name = IO.ARM4_FLOW,         Disp = "X037" },
        };

        [JsonIgnore]
        public HardOutputDef[] HardOutputs => _hardOutputs;
        private static readonly HardOutputDef[] _hardOutputs = new[]
        {
            new HardOutputDef { No = 1,  Name = IO.ARM1_VAC,  Disp = "Y039" },
            new HardOutputDef { No = 2,  Name = IO.ARM2_VAC,  Disp = "Y040" },
            new HardOutputDef { No = 3,  Name = IO.ARM3_VAC,  Disp = "Y041" },
            new HardOutputDef { No = 4,  Name = IO.ARM4_VAC,  Disp = "Y042" },
            new HardOutputDef { No = 5,  Name = IO.ARM1_BLOW, Disp = "Y043" },
            new HardOutputDef { No = 6,  Name = IO.ARM2_BLOW, Disp = "Y044" },
            new HardOutputDef { No = 7,  Name = IO.ARM3_BLOW, Disp = "Y045" },
            new HardOutputDef { No = 8,  Name = IO.ARM4_BLOW, Disp = "Y046" },
            new HardOutputDef { No = 9,  Name = IO.ARM1_VENT, Disp = "Y047" },
            new HardOutputDef { No = 10, Name = IO.ARM2_VENT, Disp = "Y048" },
            new HardOutputDef { No = 11, Name = IO.ARM3_VENT, Disp = "Y049" },
            new HardOutputDef { No = 12, Name = IO.ARM4_VENT, Disp = "Y050" },
        };
        #endregion

       
        [Category("PIckUp"), DisplayName("SeqType")]
        [DefaultValue(0)]
        public int nPickupSeqType { get; set; } = 0;

        [Category("PIckUp"), DisplayName("Up Offset (mm)")]
        [DefaultValue(0.0)]
        public double dPickUpOffset { get; set; } = 0.0;
        [Category("PIckUp"), DisplayName("Up Speed (mm/sec)")]
        [DefaultValue(0.0)]
        public double dPickUpSpeed { get; set; } = 0.0;
        [Category("PIckUp"), DisplayName("Up Acc (mm/sec2)")]
        [DefaultValue(0.0)]
        public double dPickUpAcc { get; set; } = 0.0;
        [Category("PIckUp"), DisplayName("Up Dec (mm/sec2)")]
        [DefaultValue(0.0)]
        public double dPickUpDec { get; set; } = 0.0;
        [Category("PIckUp"), DisplayName("PickUpWaitTime (ms)")]
        [DefaultValue(0)]
        public int nPickUpWaitTime { get; set; } = 0;
        [Category("PlaceUp"), DisplayName("PlaceUpWaitTime (ms)")]
        [DefaultValue(0)]
        public int nPlaceUpWaitTime { get; set; } = 0;

        [Category("PlaceUp"), DisplayName("Before Vac. (ms)")]
        [DefaultValue(0)]
        public int nPlaceBeforeVacWaitTime { get; set; } = 0;

        [Category("PlaceUp"), DisplayName("After Vac. (ms)")]
        [DefaultValue(0)]
        public int nPlaceAfterVacWaitTime { get; set; } = 0;

        [Category("PlaceUp"), DisplayName("Before Blow. (ms)")]
        [DefaultValue(0)]
        public int nPlaceBeforeBlowWaitTime { get; set; } = 0;

        [Category("PlaceUp"), DisplayName("After Blow1. (ms)")]
        [DefaultValue(0)]
        public int nPlaceAfterBlowWaitTime2 { get; set; } = 0; //Blow 키고 대기 시간.

        [Category("PlaceUp"), DisplayName("After Blow2. (ms)")]
        [DefaultValue(0)]
        public int nPlaceAfterBlowWaitTime { get; set; } = 0; //Blow 켜 놓고 올라가는 시간.

            [Category("SetupConfig"), DisplayName("IndexOfStart")]
            [DefaultValue(0)]
            public int IndexOfStart { get; set; } = 0;


        //LdPickMissAlarmThreshold
        [Category("SetupConfig"), DisplayName("Alarm Count")]
        [DefaultValue(0)]
        public int AlarmCount { get; set; } = 0;


        [JsonIgnore]
        public new List<TeachingPosition> TeachingPositions
        {
            get => base.TeachingPositions;
            set => base.TeachingPositions = value;
        }

        [JsonIgnore]
        private InputDieTransferRecipe _teachingRecipeCache;
        [JsonIgnore]
        private string _teachingRecipeNameCache; // [ADD] 마지막으로 사용한 recipe name

        private const string UnitKey_Teaching = "InputDieTransferTeaching";

        [JsonIgnore]
        public InputDieTransferRecipe TeachingRecipe
        {
            get
            {
                try
                {
                    var eq = Equipment.Instance;
                    var er = eq?.EquipmentRecipe;

                    var teachingRecipeName = er?.GetOrLoadUnitTeachingRecipeName(UnitKey_Teaching);
                    if (string.IsNullOrWhiteSpace(teachingRecipeName))
                        teachingRecipeName = "Default_InputDieTransferTeaching";

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
                    var cached = er?.InputDieTransferTeachingRecipe;
                    if (cached != null &&
                        string.Equals(cached.Name, teachingRecipeName, StringComparison.OrdinalIgnoreCase))
                    {
                        _teachingRecipeCache = cached;
                        return _teachingRecipeCache;
                    }

                    // 3) 폴백: 파일에서 로드/생성 후 EquipmentRecipe 캐시에 주입
                    _teachingRecipeCache = RecipeManager.LoadOrCreate<InputDieTransferRecipe>(teachingRecipeName);
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
                        var cached = er?.InputDieTransferTeachingRecipe;
                        if (cached != null)
                        {
                            _teachingRecipeCache = cached;
                            _teachingRecipeNameCache = cached.Name;
                            return _teachingRecipeCache;
                        }
                    }
                    catch { }

                    if (_teachingRecipeCache == null)
                        _teachingRecipeCache = RecipeManager.LoadOrCreate<InputDieTransferRecipe>("Default_LoadAlignTeaching");
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

        public new TeachingPosition GetTeachingPosition(string name)
            => TeachingRecipe.Get(name);

        public int LoadAndBindAxes(MotionAxisManager axisManager)
        {
            int rc = Load();
            if (rc != 0)
                return rc;

            return TeachingRecipe.LoadAndBindAxes(axisManager);
        }


        public InputDieTransferConfig() : base("InputDieTransferConfig") { }

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
                { "SetupConfig", 2 },
                { "PickUp", 3 },
                { "PlaceUp", 4 },
            };

        // Property 순서: (DisplayName 또는 PropertyName)
        // BaseConfig: "Simulation" (IsSimulation)
        // Cassette: "SlotPitch (mm)", "SlotCount (ea)"
        public IEnumerable<string> GetPropertyOrder()
            => new[]
            {
                "Name",
                "Simulation",
                "DryRun",
                "IndexOfStart",
                "SeqType",
                "Up Offset (mm)",
                "Up Speed (mm/sec)",
                "Up Acc (mm/sec2)",
                "Up Dec (mm/sec2)",
                "PickUpWaitTime (ms)",
                "PlaceUpWaitTime (ms)",
                "Before Vac. (ms)",
                "After Vac. (ms)",
                "Before Blow. (ms)",
                "After Blow. (ms)"
            };
        #endregion
    }
}