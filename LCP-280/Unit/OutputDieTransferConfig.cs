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
    public class OutputDieTransferConfig : BaseConfig, IPropertyOrderProvider
    {
        // ================= IO 이름 상수 (OutputStageConfig.IO 패턴과 동일) =================
        internal static class IO
        {
            // Inputs
            public const string AIR_TANK_PRESS = "RIGHT TOOL AIR TANK PRESSURE CHECK";
            public const string VAC_TANK_PRESS = "RIGHT TOOL VACUUM TANK PRESSURE CHECK";
            public const string ARM1_FLOW = "RIGHT TOOL ARM 1 FLOW CHECK";
            public const string ARM2_FLOW = "RIGHT TOOL ARM 2 FLOW CHECK";
            public const string ARM3_FLOW = "RIGHT TOOL ARM 3 FLOW CHECK";
            public const string ARM4_FLOW = "RIGHT TOOL ARM 4 FLOW CHECK";

            // Outputs (Vac / Blow / Vent)
            public const string ARM1_VAC = "RIGHT ARM 1 VACUUM";
            public const string ARM2_VAC = "RIGHT ARM 2 VACUUM";
            public const string ARM3_VAC = "RIGHT ARM 3 VACUUM";
            public const string ARM4_VAC = "RIGHT ARM 4 VACUUM";

            public const string ARM1_BLOW = "RIGHT ARM 1 BLOW";
            public const string ARM2_BLOW = "RIGHT ARM 2 BLOW";
            public const string ARM3_BLOW = "RIGHT ARM 3 BLOW";
            public const string ARM4_BLOW = "RIGHT ARM 4 BLOW";

            public const string ARM1_VENT = "RIGHT ARM 1 VENT";
            public const string ARM2_VENT = "RIGHT ARM 2 VENT";
            public const string ARM3_VENT = "RIGHT ARM 3 VENT";
            public const string ARM4_VENT = "RIGHT ARM 4 VENT";

            // 그룹 배열 (Unit 코드에서 직접 활용)
            public static readonly string[] ARM_FLOW = { ARM1_FLOW, ARM2_FLOW, ARM3_FLOW, ARM4_FLOW };
            public static readonly string[] ARM_VAC = { ARM1_VAC, ARM2_VAC, ARM3_VAC, ARM4_VAC };
            public static readonly string[] ARM_BLOW = { ARM1_BLOW, ARM2_BLOW, ARM3_BLOW, ARM4_BLOW };
            public static readonly string[] ARM_VENT = { ARM1_VENT, ARM2_VENT, ARM3_VENT, ARM4_VENT };
        }

        #region Hard IO Tables
        [JsonIgnore]
        public HardInputDef[] HardInputs => _hardInputs;
        [JsonIgnore]
        private static readonly HardInputDef[] _hardInputs = new[]
        {
            new HardInputDef { No = 1, Name = IO.AIR_TANK_PRESS,  Disp = "X051" },
            new HardInputDef { No = 2, Name = IO.VAC_TANK_PRESS,  Disp = "X052" },
            new HardInputDef { No = 3, Name = IO.ARM1_FLOW, Disp = "X053" },
            new HardInputDef { No = 4, Name = IO.ARM2_FLOW, Disp = "X054" },
            new HardInputDef { No = 5, Name = IO.ARM3_FLOW, Disp = "X055" },
            new HardInputDef { No = 6, Name = IO.ARM4_FLOW, Disp = "X056" }
        };

        [JsonIgnore]
        public HardOutputDef[] HardOutputs => _hardOutputs;
        [JsonIgnore]
        private static readonly HardOutputDef[] _hardOutputs = new[]
        {
            new HardOutputDef { No = 1,  Name = IO.ARM1_VAC,  Disp = "Y076" },
            new HardOutputDef { No = 2,  Name = IO.ARM2_VAC,  Disp = "Y077" },
            new HardOutputDef { No = 3,  Name = IO.ARM3_VAC,  Disp = "Y078" },
            new HardOutputDef { No = 4,  Name = IO.ARM4_VAC,  Disp = "Y079" },
            new HardOutputDef { No = 5,  Name = IO.ARM1_BLOW, Disp = "Y080" },
            new HardOutputDef { No = 6,  Name = IO.ARM2_BLOW, Disp = "Y081" },
            new HardOutputDef { No = 7,  Name = IO.ARM3_BLOW, Disp = "Y082" },
            new HardOutputDef { No = 8,  Name = IO.ARM4_BLOW, Disp = "Y083" },
            new HardOutputDef { No = 9,  Name = IO.ARM1_VENT, Disp = "Y084" },
            new HardOutputDef { No = 10, Name = IO.ARM2_VENT, Disp = "Y085" },
            new HardOutputDef { No = 11, Name = IO.ARM3_VENT, Disp = "Y086" },
            new HardOutputDef { No = 12, Name = IO.ARM4_VENT, Disp = "Y087" }
        };
        #endregion


        [Category("SetupConfig"), DisplayName("IndexOfEnd")]
        [DefaultValue(0)]
        public int IndexOfEnd { get; set; } = 0;

        [JsonIgnore]
        public new List<TeachingPosition> TeachingPositions
        {
            get => base.TeachingPositions;
            set => base.TeachingPositions = value;
        }

        [JsonIgnore]
        private OutputDieTransferRecipe _teachingRecipeCache;
        [JsonIgnore]
        private string _teachingRecipeNameCache; // [ADD] 마지막으로 사용한 recipe name

        private const string UnitKey_Teaching = "OutputDieTransferTeaching";

        [JsonIgnore]
        public OutputDieTransferRecipe TeachingRecipe
        {
            get
            {
                try
                {
                    var eq = Equipment.Instance;
                    var er = eq?.EquipmentRecipe;

                    var teachingRecipeName = er?.GetOrLoadUnitTeachingRecipeName(UnitKey_Teaching);
                    if (string.IsNullOrWhiteSpace(teachingRecipeName))
                        teachingRecipeName = "Default_OutputDieTransferTeaching";

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
                    var cached = er?.OutputDieTransferTeachingRecipe;
                    if (cached != null &&
                        string.Equals(cached.Name, teachingRecipeName, StringComparison.OrdinalIgnoreCase))
                    {
                        _teachingRecipeCache = cached;
                        return _teachingRecipeCache;
                    }

                    // 3) 폴백: 파일에서 로드/생성 후 EquipmentRecipe 캐시에 주입
                    _teachingRecipeCache = RecipeManager.LoadOrCreate<OutputDieTransferRecipe>(teachingRecipeName);
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
                        var cached = er?.OutputDieTransferTeachingRecipe;
                        if (cached != null)
                        {
                            _teachingRecipeCache = cached;
                            _teachingRecipeNameCache = cached.Name;
                            return _teachingRecipeCache;
                        }
                    }
                    catch { }

                    if (_teachingRecipeCache == null)
                        _teachingRecipeCache = RecipeManager.LoadOrCreate<OutputDieTransferRecipe>("Default_LoadAlignTeaching");
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




        public OutputDieTransferConfig() : base("OutputDieTransferConfig") { }

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