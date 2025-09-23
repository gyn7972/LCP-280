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
    /// <summary>
    /// IndexChipProbeControllerConfig
    ///  - Probe Z / Probe Card XYZ / Sphere Z Teaching Positions
    ///  - Sphere FW/BW Cylinder + Probe Card Vacuum I/O 명칭 상수화
    ///  - OutputStageConfig 패턴 구조 적용
    /// </summary>
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
            public const string SPHERE_FW_VLV  = "SPHERE FW";                // Y026 (Forward valve)
            public const string SPHERE_BW_VLV  = "SPHERE BW";                // Y027 (Backward valve)
            public const string PROBE_VAC_VLV  = "PROBE CARD VACUUM";        // Y075 (Vac valve or combined channel)
        }

        public enum TeachingPositionName
        {
            TopContact_Index1_Up,
            TopContact_Index1_Ready,
            TopContact_Index2_Up,
            TopContact_Index2_Ready,
            TopContact_Index3_Up,
            TopContact_Index3_Ready,
            TopContact_Index4_Up,
            TopContact_Index4_Ready,
            TopContact_Index5_Up,
            TopContact_Index5_Ready,
            TopContact_Index6_Up,
            TopContact_Index6_Ready,
            TopContact_Index7_Up,
            TopContact_Index7_Ready,
            TopContact_Index8_Up,
            TopContact_Index8_Ready,
            Bottom_Index1_Up,
            Bottom_Index1_Ready,
            Bottom_Index2_Up,
            Bottom_Index2_Ready,
            Bottom_Index3_Up,
            Bottom_Index3_Ready,
            Bottom_Index4_Up,
            Bottom_Index4_Ready,
            Bottom_Index5_Up,
            Bottom_Index5_Ready,
            Bottom_Index6_Up,
            Bottom_Index6_Ready,
            Bottom_Index7_Up,
            Bottom_Index7_Ready,
            Bottom_Index8_Up,
            Bottom_Index8_Ready,
            SphereZ_Up,
            SphereZ_Ready,
            SafetyZone,
        }
        
        public override bool GetTeachingPositionName(int selIndex, out string name)
        {
            if (Enum.GetNames(typeof(TeachingPositionName)).Length <= selIndex)
            {
                name = "None";
                return false;
            }
            TeachingPositionName tpn = (TeachingPositionName)selIndex;
            name = tpn.ToString();
            return true;
        }

        [JsonIgnore]
        private static readonly Dictionary<TeachingPositionName, string[]> _axisMap = new Dictionary<TeachingPositionName, string[]>
        {
            // 기본: 2축 모두 사용. 필요 시 특정 포지션에서 한 축만 사용하도록 배열 수정.
            { TeachingPositionName.TopContact_Index1_Up,        new [] { AxisNames.ProbeZ } },
            { TeachingPositionName.TopContact_Index1_Ready,     new [] { AxisNames.ProbeZ } },
            { TeachingPositionName.TopContact_Index2_Up,        new [] { AxisNames.ProbeZ } },
            { TeachingPositionName.TopContact_Index2_Ready,     new [] { AxisNames.ProbeZ } },
            { TeachingPositionName.TopContact_Index3_Up,        new [] { AxisNames.ProbeZ } },
            { TeachingPositionName.TopContact_Index3_Ready,     new [] { AxisNames.ProbeZ } },
            { TeachingPositionName.TopContact_Index4_Up,        new [] { AxisNames.ProbeZ } },
            { TeachingPositionName.TopContact_Index4_Ready,     new [] { AxisNames.ProbeZ } },
            { TeachingPositionName.TopContact_Index5_Up,        new [] { AxisNames.ProbeZ } },
            { TeachingPositionName.TopContact_Index5_Ready,     new [] { AxisNames.ProbeZ } },
            { TeachingPositionName.TopContact_Index6_Up,        new [] { AxisNames.ProbeZ } },
            { TeachingPositionName.TopContact_Index6_Ready,     new [] { AxisNames.ProbeZ } },
            { TeachingPositionName.TopContact_Index7_Up,        new [] { AxisNames.ProbeZ } },
            { TeachingPositionName.TopContact_Index7_Ready,     new [] { AxisNames.ProbeZ } },
            { TeachingPositionName.TopContact_Index8_Up,        new [] { AxisNames.ProbeZ } },
            { TeachingPositionName.TopContact_Index8_Ready,     new [] { AxisNames.ProbeZ } },
            { TeachingPositionName.SphereZ_Up,                  new [] { AxisNames.SphereZ } },
            { TeachingPositionName.SphereZ_Ready,               new [] { AxisNames.SphereZ } },
            { TeachingPositionName.SafetyZone,                  new [] { AxisNames.ProbeZ, AxisNames.ProbeCardZ } },
            { TeachingPositionName.Bottom_Index1_Up,            new [] { AxisNames.ProbeCardX, AxisNames.ProbeCardY, AxisNames.ProbeCardZ } },
            { TeachingPositionName.Bottom_Index1_Ready,         new [] { AxisNames.ProbeCardX, AxisNames.ProbeCardY, AxisNames.ProbeCardZ } },
            { TeachingPositionName.Bottom_Index2_Up,            new [] { AxisNames.ProbeCardX, AxisNames.ProbeCardY, AxisNames.ProbeCardZ } },
            { TeachingPositionName.Bottom_Index2_Ready,         new [] { AxisNames.ProbeCardX, AxisNames.ProbeCardY, AxisNames.ProbeCardZ } },
            { TeachingPositionName.Bottom_Index3_Up,            new [] { AxisNames.ProbeCardX, AxisNames.ProbeCardY, AxisNames.ProbeCardZ } },
            { TeachingPositionName.Bottom_Index3_Ready,         new [] { AxisNames.ProbeCardX, AxisNames.ProbeCardY, AxisNames.ProbeCardZ } },
            { TeachingPositionName.Bottom_Index4_Up,            new [] { AxisNames.ProbeCardX, AxisNames.ProbeCardY, AxisNames.ProbeCardZ } },
            { TeachingPositionName.Bottom_Index4_Ready,         new [] { AxisNames.ProbeCardX, AxisNames.ProbeCardY, AxisNames.ProbeCardZ } },
            { TeachingPositionName.Bottom_Index5_Up,            new [] { AxisNames.ProbeCardX, AxisNames.ProbeCardY, AxisNames.ProbeCardZ } },
            { TeachingPositionName.Bottom_Index5_Ready,         new [] { AxisNames.ProbeCardX, AxisNames.ProbeCardY, AxisNames.ProbeCardZ } },
            { TeachingPositionName.Bottom_Index6_Up,            new [] { AxisNames.ProbeCardX, AxisNames.ProbeCardY, AxisNames.ProbeCardZ } },
            { TeachingPositionName.Bottom_Index6_Ready,         new [] { AxisNames.ProbeCardX, AxisNames.ProbeCardY, AxisNames.ProbeCardZ } },
            { TeachingPositionName.Bottom_Index7_Up,            new [] { AxisNames.ProbeCardX, AxisNames.ProbeCardY, AxisNames.ProbeCardZ } },
            { TeachingPositionName.Bottom_Index7_Ready,         new [] { AxisNames.ProbeCardX, AxisNames.ProbeCardY, AxisNames.ProbeCardZ } },
            { TeachingPositionName.Bottom_Index8_Up,            new [] { AxisNames.ProbeCardX, AxisNames.ProbeCardY, AxisNames.ProbeCardZ } },
            { TeachingPositionName.Bottom_Index8_Ready,         new [] { AxisNames.ProbeCardX, AxisNames.ProbeCardY, AxisNames.ProbeCardZ } },
        };

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
            new HardOutputDef { No = 1, Name = IO.SPHERE_FW_VLV, Disp = "Y026" },
            new HardOutputDef { No = 2, Name = IO.SPHERE_BW_VLV, Disp = "Y027" },
            new HardOutputDef { No = 3, Name = IO.PROBE_VAC_VLV, Disp = "Y075" },
        };
        #endregion

        [Category("SetupConfig"), DisplayName("IndexOfProbe")]
        [DefaultValue(0)]
        public int IndexOfProbe { get; set; } = 0;

        [Category("SetupConfig"), DisplayName("ContectMode")]
        [DefaultValue(false)]
        public bool ContectMode { get; set; } = false;

        [Category("SetupConfig"), DisplayName("InspectTimeOut (ms)")]
        [DefaultValue(0)]
        public int ProbeInspectTimeOutms { get; set; } = 60000;

        public IndexChipProbeControllerConfig() : base("IndexChipProbeControllerConfig") { }

        /// <summary>Teaching Position 기본 생성 (축 매핑 적용)</summary>
        public void InitializeDefaultTeachingPositions()
        {
            if (TeachingPositions == null) TeachingPositions = new List<TeachingPosition>();
            var existing = new HashSet<string>(TeachingPositions.Select(tp => tp.Name));
            foreach (TeachingPositionName name in System.Enum.GetValues(typeof(TeachingPositionName)))
            {
                string posName = name.ToString();
                if (!existing.Contains(posName))
                {
                    var axes = GetAxisNamesForPosition(posName);
                    var axisPositions = new Dictionary<string, double>();
                    foreach (var a in axes) axisPositions[a] = 0.0;
                    TeachingPositions.Add(new TeachingPosition(posName, axisPositions, $"기본 {posName} 위치"));
                }
            }
            ApplyAxisMapping();
            Saveconfig();
        }

        /// <summary>Teaching Position 추가/갱신 (허용된 축만 유지)</summary>
        public void SetTeachingPosition(TeachingPosition tp)
        {
            var allowed = GetAxisNamesForPosition(tp.Name).ToHashSet();
            var filtered = new Dictionary<string, double>();
            foreach (var axis in allowed)
            {
                double v = 0;
                if (tp.AxisPositions != null && tp.AxisPositions.TryGetValue(axis, out var val)) v = val;
                filtered[axis] = v;
            }
            tp.AxisPositions = filtered;

            var exist = TeachingPositions.FirstOrDefault(p => p.Name == tp.Name);
            if (exist != null)
            {
                exist.AxisPositions = tp.AxisPositions;
                exist.Description = tp.Description;
                exist.ExtraInfo = tp.ExtraInfo;
            }
            else TeachingPositions.Add(tp);
            Saveconfig();
        }

        public TeachingPosition GetTeachingPosition(string name) => TeachingPositions.FirstOrDefault(p => p.Name == name);

        /// <summary>Config 저장 (TeachingPositions 순수화)</summary>
        public int Saveconfig()
        {
            var pure = TeachingPositions
                .Select(tp => new TeachingPosition(tp.Name, tp.AxisPositions, tp.Description) { ExtraInfo = tp.ExtraInfo })
                .ToList();
            var backup = TeachingPositions;
            TeachingPositions = pure;
            try { return Save(); }
            finally { TeachingPositions = backup; }
        }

        public int LoadAndBindAxes(MotionAxisManager axisManager)
        {
            int rc = Load();
            if (rc != 0) return rc;

            var loaded = TeachingPositions ?? new List<TeachingPosition>();
            var byName = new Dictionary<string, TeachingPosition>(StringComparer.OrdinalIgnoreCase);
            foreach (var t in loaded)
            {
                if (t == null || string.IsNullOrWhiteSpace(t.Name)) continue;
                if (!byName.ContainsKey(t.Name)) byName[t.Name] = t;
            }

            var rebuilt = new List<TeachingPosition>();
            foreach (TeachingPositionName en in Enum.GetValues(typeof(TeachingPositionName)))
            {
                string posName = en.ToString();
                TeachingPosition tp;
                if (byName.TryGetValue(posName, out tp) && tp != null)
                {
                    rebuilt.Add(tp);
                }
                else
                {
                    var axes = GetAxisNamesForPosition(posName);
                    var axisPositions = new Dictionary<string, double>();
                    foreach (var a in axes) axisPositions[a] = 0.0;
                    rebuilt.Add(new TeachingPosition(posName, axisPositions, $"기본 {posName} 위치"));
                }
            }

            TeachingPositions = rebuilt;

            ApplyAxisMapping();

            if (axisManager != null)
            {
                foreach (var tp in TeachingPositions)
                    tp.BindAxes(axisManager, "Unit");
            }
            return 0;
        }

        /// <summary>매핑에 따라 불필요 축 제거 / 누락 축 추가</summary>
        public void ApplyAxisMapping()
        {
            foreach (var tp in TeachingPositions)
            {
                var allowed = GetAxisNamesForPosition(tp.Name).ToHashSet();
                var current = tp.AxisPositions ?? new Dictionary<string, double>();
                var next = new Dictionary<string, double>();
                foreach (var axis in allowed)
                {
                    if (current.TryGetValue(axis, out var v)) next[axis] = v; else next[axis] = 0.0;
                }
                tp.AxisPositions = next;
            }
        }

        /// <summary>Position 이름 기반 허용 축 목록 반환</summary>
        public IReadOnlyList<string> GetAxisNamesForPosition(string positionName)
        {
            if (string.IsNullOrWhiteSpace(positionName)) return new List<string>();
            if (System.Enum.TryParse<TeachingPositionName>(positionName, out var en))
            {
                if (_axisMap.TryGetValue(en, out var arr)) return arr;
            }
            // 기본(백워드 호환)
            return new[] { AxisNames.ProbeZ, AxisNames.ProbeCardX, AxisNames.ProbeCardY, AxisNames.ProbeCardZ, AxisNames.SphereZ };
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