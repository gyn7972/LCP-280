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
    /// <summary>
    /// OutputCassetteLifterConfig
    ///  - Bin Lifter Z Axis Teaching Positions
    ///  - Cassette / RingJut / Mapping sensor IO 정의 (입력 전용)
    ///  - OutputStageConfig 구조와 동일한 패턴(내부 IO 상수 / Hard I/O 테이블 / Save & Load)
    ///  - (추가) TeachingPosition 별 허용 축 필터링 기능 적용
    /// </summary>
    public class OutputCassetteLifterConfig : BaseConfig, IPropertyOrderProvider
    {
        /// <summary>
        /// 장치 IO 명칭 (입력 전용, 출력 없음 -> 필요시 확장)
        /// </summary>
        internal static class IO
        {
            public const string CASSETTE_CHECK0 = "BIN LIFTER CASSETTE CHECK 0"; // X069
            public const string CASSETTE_CHECK1 = "BIN LIFTER CASSETTE CHECK 1"; // X070
            public const string RING_JUT_CHECK  = "BIN LIFTER RING JUT CHECK";   // X071
            public const string MAPPING_SENSOR  = "BIN MAPPING";                 // X072
        }

        public enum TeachingPositionName
        {
            MappingStart,
            MappingEnd,
            CassetteSlot_1,
            UnloadOffset,
            LoadPort
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
        /// <summary>
        /// 모든 포지션은 현재 BinLifterZ 단일축만 사용. (추후 다축 확장 대비 구조 유지)
        /// </summary>
        [JsonIgnore]
        private static readonly Dictionary<TeachingPositionName, string[]> _axisMap = new Dictionary<TeachingPositionName, string[]>
        {
            { TeachingPositionName.MappingStart,   new [] { AxisNames.BinLifterZ } },
            { TeachingPositionName.MappingEnd,     new [] { AxisNames.BinLifterZ } },
            { TeachingPositionName.CassetteSlot_1, new [] { AxisNames.BinLifterZ } },
            { TeachingPositionName.UnloadOffset,   new [] { AxisNames.BinLifterZ } },
            { TeachingPositionName.LoadPort,       new [] { AxisNames.BinLifterZ } },
        };

        #region Hard IO Tables
        [JsonIgnore]
        public HardInputDef[] HardInputs => _hardInputs;
        private static readonly HardInputDef[] _hardInputs = new[]
        {
            new HardInputDef { No = 1, Name = IO.CASSETTE_CHECK0, Disp = "X069" },
            new HardInputDef { No = 2, Name = IO.CASSETTE_CHECK1, Disp = "X070" },
            new HardInputDef { No = 3, Name = IO.RING_JUT_CHECK,  Disp = "X071" },
            new HardInputDef { No = 4, Name = IO.MAPPING_SENSOR,  Disp = "X072" },
        };

        [JsonIgnore]
        public HardOutputDef[] HardOutputs => _hardOutputs;
        private static readonly HardOutputDef[] _hardOutputs = Array.Empty<HardOutputDef>();
        #endregion

        [Category("Cassette"), DisplayName("SlotPitch (mm)")]
        [DefaultValue(0.0)]
        //[DisplayOrder(1)]
        public double SlotPitch { get; set; } = 0.0;

        [Category("Cassette"), DisplayName("SlotCount (ea)")]
        [DefaultValue(0)]
        //[DisplayOrder(1)]
        public int SlotCount { get; set; } = 0;

        [Category("Cassette"), DisplayName("Use Barcode")]
        [DefaultValue(false)]
        [JsonProperty("UseBarcode")] // ← JSON 키를 명시적으로 고정
        public bool UseBarcode { get; set; } = false;

        public OutputCassetteLifterConfig() : base("OutputCassetteLifterConfig") { }

        /// <summary>Teaching Position 기본 생성 + 축 매핑 적용</summary>
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

        /// <summary>Teaching Position 추가/갱신 (허용 축 필터링)</summary>
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

        public new TeachingPosition GetTeachingPosition(string name) => TeachingPositions.FirstOrDefault(p => p.Name == name);

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

        /// <summary>로드 + 축 매핑 + TeachingPosition 축 바인딩</summary>
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

        /// <summary>TeachingPositions 의 AxisPositions 를 허용 축만 유지 / 누락 축 추가</summary>
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

        /// <summary>문자열 Position 이름으로 허용 축 배열 반환</summary>
        public IReadOnlyList<string> GetAxisNamesForPosition(string positionName)
        {
            if (string.IsNullOrWhiteSpace(positionName)) return new List<string>();
            if (System.Enum.TryParse<TeachingPositionName>(positionName, out var en))
            {
                if (_axisMap.TryGetValue(en, out var arr)) return arr;
            }
            // 기본: BinLifterZ 1축
            return new[] { AxisNames.BinLifterZ };
        }

        #region IPropertyOrderProvider 구현 (Category / Property 표시 순서)
        // Category 순서: Common → Cassette
        public IDictionary<string, int> GetCategoryOrder()
            => new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
            {
                { "General", 0 },   // Name 속성 (Category 없음) 정렬 위치 지정
                { "Common", 1 },
                { "Cassette", 2 },
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
                "SlotCount (ea)",
                "Use_Barcode",
            };
        #endregion

        protected override JsonSerializerSettings GetJsonSettings()
        {
            return new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Include,   // ← 기본값도 저장
                ObjectCreationHandling = ObjectCreationHandling.Replace
            };
        }
    }
}