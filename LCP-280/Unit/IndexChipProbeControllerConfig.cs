using Newtonsoft.Json;
using QMC.Common;
using QMC.Common.Component;
using QMC.Common.Motions;
using QMC.Common.Unit;
using QMC.LCP_280.Process.Component;
using System; // Enum 활용
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace QMC.LCP_280.Process.Unit
{
    public class IndexChipProbeControllerConfig : BaseConfig, IPropertyOrderProvider
    {
        /// <summary>장치 IO 명칭</summary>
        internal static class IO
        {
            // Inputs
            public const string SPHERE_FW_SNS  = "SPHERE FW";                // X038 (Forward sensor)
            public const string SPHERE_BW_SNS  = "SPHERE BW";                // X039 (Backward sensor)
            public const string PROBE_VAC_OK   = "PROBE CARD VACUUM CHECK";  // X050
            // Outputs
            public const string BLADE_CONTACT_VLV = "BLADE CONTACT";                // Y012
            public const string PROBECARD_CONTACT_VLV = "PROBECARD CONTACT";        // Y013
            public const string SPHERE_FW_VLV  = "SPHERE FW";                // Y026 (Forward valve)
            public const string SPHERE_BW_VLV  = "SPHERE BW";                // Y027 (Backward valve)
            public const string PROBE_VAC_VLV  = "PROBE CARD VACUUM";        // Y075 (Vac valve or combined channel)
        }

        #region Hard IO Tables
        [JsonIgnore]
        public HardInputDef[] HardInputs => _hardInputs;
        private static readonly HardInputDef[] _hardInputs = new[]
        {
            new HardInputDef { No = 1, Name = IO.SPHERE_FW_SNS, Disp = "X038" },
            new HardInputDef { No = 2, Name = IO.SPHERE_BW_SNS, Disp = "X039" },
            new HardInputDef { No = 3, Name = IO.PROBE_VAC_OK,  Disp = "X050" },
        };

        [JsonIgnore]
        public HardOutputDef[] HardOutputs => _hardOutputs;
        private static readonly HardOutputDef[] _hardOutputs = new[]
        {
            new HardOutputDef { No = 1, Name = IO.BLADE_CONTACT_VLV,        Disp = "Y012" },
            new HardOutputDef { No = 2, Name = IO.PROBECARD_CONTACT_VLV,    Disp = "Y013" },
            new HardOutputDef { No = 3, Name = IO.SPHERE_FW_VLV, Disp = "Y026" },
            new HardOutputDef { No = 4, Name = IO.SPHERE_BW_VLV, Disp = "Y027" },
            new HardOutputDef { No = 5, Name = IO.PROBE_VAC_VLV, Disp = "Y075" },
        };
        #endregion
        
        [JsonIgnore]
        public new List<TeachingPosition> TeachingPositions
        {
            get => base.TeachingPositions;
            set => base.TeachingPositions = value;
        }

        [Category("SetupConfig"), DisplayName("IndexOfProbe")]
        [DefaultValue(0)]
        public int IndexOfProbe { get; set; } = 0;

        //[Category("SetupConfig"), DisplayName("ContectTopMode")]
        //[DefaultValue(false)]
        //public bool ContectTopMode { get; set; } = false;

        [Category("SetupConfig"), DisplayName("InspectTimeOut(ms)")]
        [DefaultValue(0)]
        public int ProbeInspectTimeOutms { get; set; } = 60000;

        [Category("SetupConfig"), DisplayName("Upper WaitTime(ms)")]
        [DefaultValue(0)]
        public int UpperWaitTime { get; set; } = 0;

        [Category("SetupConfig"), DisplayName("SyncProbeCardZ(mm)")]
        [DefaultValue(0)]
        public double SyncProbeCardZReady { get; set; } = 0; // 단위: m (예: 0.0005 for 0.5mm)

        [Category("SetupConfig"), DisplayName("Use Overdrive")]
        [DefaultValue(false)]
        public bool overdriveUse { get; set; } = false; // 단위: m (예: 0.0005 for 0.5mm)

        [Category("SetupConfig"), DisplayName("Overdrive(mm)")]
        [DefaultValue(0)]
        public double overdriveDist { get; set; } = 0; // 단위: m (예: 0.0005 for 0.5mm)

        //ClampX safeMoveDist
        [Category("SetupConfig"), DisplayName("ClampXMoveDist(%)")]
        [DefaultValue(0)]
        public double ClampXMoveDist { get; set; } = 50; // 단위: %

        //ViewMode
        [Category("SetupConfig"), DisplayName("View Mode")]
        [DefaultValue(false)]
        [JsonProperty("ViewMode")]
        public bool ViewMode { get; set; } = false;

        [Category("SetupConfig"), DisplayName("Gripper Mode")]
        [DefaultValue(false)]
        [JsonProperty("GripperMode")]
        public bool GripperMode { get; set; } = false;


        [JsonIgnore]
        private IndexChipProbeControllerRecipe _teachingRecipeCache;
        [JsonIgnore]
        private string _teachingRecipeNameCache; // [ADD] 마지막으로 사용한 recipe name

        private const string UnitKey_Teaching = "ProbeTeaching";

        [JsonIgnore]
        public IndexChipProbeControllerRecipe TeachingRecipe
        {
            get
            {
                try
                {
                    var eq = Equipment.Instance;
                    var er = eq?.EquipmentRecipe;

                    var teachingRecipeName = er?.GetOrLoadUnitTeachingRecipeName(UnitKey_Teaching);
                    if (string.IsNullOrWhiteSpace(teachingRecipeName))
                        teachingRecipeName = "Default_ProbeTeaching";

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

                    _teachingRecipeCache = RecipeManager.LoadOrCreate<IndexChipProbeControllerRecipe>(teachingRecipeName);
                    try { er?.SetUnitTeachingRecipe(UnitKey_Teaching, _teachingRecipeCache, save: false); } catch { }
                    return _teachingRecipeCache;
                }
                catch
                {
                    if (_teachingRecipeCache == null)
                        _teachingRecipeCache = RecipeManager.LoadOrCreate<IndexChipProbeControllerRecipe>("Default_ProbeTeaching");
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
            

        public IndexChipProbeControllerConfig() : base("IndexChipProbeControllerConfig") 
        { 

        }

        public int Saveconfig()
        {
            try 
            { 
                return Save(); 
            }
            catch(Exception ex) 
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
                "IndexOfProbe",
                "InspectTimeOut(ms)",
                "Gripper Mode",
                "View Mode",
                "Use Overdrive",
                "Overdrive(mm)",
                "SyncProbeCardZ(mm)",
                "ClampXMoveDist(%)",
                "Upper WaitTime(ms)",
            };
        #endregion
    }
}