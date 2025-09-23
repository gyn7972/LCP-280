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
    /// <summary>
    /// IndexLoadAlignerConfig
    ///  - Align T / Index Z 축 Teaching Positions 관리 (축 매핑 적용)
    ///  - (현재 별도 IO 없음: 필요 시 IO 클래스 확장)
    ///  - OutputStageConfig 스타일 구조 적용 + Axis filtering
    /// </summary>
    public class IndexLoadAlignerConfig : BaseConfig, IPropertyOrderProvider
    {
        internal static class IO { /* Add inputs/outputs later if needed */ }

        public enum TeachingPositionName
        {
            AlignZ_Index1_Up,
            AlignZ_Index1_Ready,
            AlignZ_Index2_Up,
            AlignZ_Index2_Ready,
            AlignZ_Index3_Up,
            AlignZ_Index3_Ready,
            AlignZ_Index4_Up,
            AlignZ_Index4_Ready,
            AlignZ_Index5_Up,
            AlignZ_Index5_Ready,
            AlignZ_Index6_Up,
            AlignZ_Index6_Ready,
            AlignZ_Index7_Up,
            AlignZ_Index7_Ready,
            AlignZ_Index8_Up,
            AlignZ_Index8_Ready,
            AlignT_Foward,
            AlignT_Backward,
            AlignT_Ready,
            SafetyZone
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
        /// Position 별 허용 축 매핑 (필요 시 일부 Position에서 특정 축만 사용하도록 조정)
        /// </summary>
        [JsonIgnore]
        private static readonly Dictionary<TeachingPositionName, string[]> _axisMap = new Dictionary<TeachingPositionName, string[]>
        {
            { TeachingPositionName.AlignZ_Index1_Up,    new [] { AxisNames.IndexZ } },
            { TeachingPositionName.AlignZ_Index1_Ready, new [] { AxisNames.IndexZ } },
            { TeachingPositionName.AlignZ_Index2_Up,    new [] { AxisNames.IndexZ } },
            { TeachingPositionName.AlignZ_Index2_Ready, new [] { AxisNames.IndexZ } },
            { TeachingPositionName.AlignZ_Index3_Up,    new [] { AxisNames.IndexZ } },
            { TeachingPositionName.AlignZ_Index3_Ready, new [] { AxisNames.IndexZ } },
            { TeachingPositionName.AlignZ_Index4_Up,    new [] { AxisNames.IndexZ } },
            { TeachingPositionName.AlignZ_Index4_Ready, new [] { AxisNames.IndexZ } },
            { TeachingPositionName.AlignZ_Index5_Up,    new [] { AxisNames.IndexZ } },
            { TeachingPositionName.AlignZ_Index5_Ready, new [] { AxisNames.IndexZ } },
            { TeachingPositionName.AlignZ_Index6_Up,    new [] { AxisNames.IndexZ } },
            { TeachingPositionName.AlignZ_Index6_Ready, new [] { AxisNames.IndexZ } },
            { TeachingPositionName.AlignZ_Index7_Up,    new [] { AxisNames.IndexZ } },
            { TeachingPositionName.AlignZ_Index7_Ready, new [] { AxisNames.IndexZ } },
            { TeachingPositionName.AlignZ_Index8_Up,    new [] { AxisNames.IndexZ } },
            { TeachingPositionName.AlignZ_Index8_Ready, new [] { AxisNames.IndexZ } },
            { TeachingPositionName.AlignT_Foward,       new [] { AxisNames.AlignT } },
            { TeachingPositionName.AlignT_Backward,     new [] { AxisNames.AlignT } },
            { TeachingPositionName.AlignT_Ready,        new [] { AxisNames.AlignT } },
            { TeachingPositionName.SafetyZone,          new [] { AxisNames.IndexZ } },
        };

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
        public int IndexOfMAlign { get; internal set; } = 0;

        public IndexLoadAlignerConfig() : base("IndexLoadAlignerConfig") { }

        /// <summary>
        /// 기본 Teaching Position 생성 (매핑 적용)
        /// </summary>
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

        /// <summary>
        /// Position 추가/갱신 (허용된 축만 유지, 누락 축은 초기값 삽입)
        /// </summary>
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

        public TeachingPosition GetTeachingPosition(string name) 
            => TeachingPositions.FirstOrDefault(p => p.Name == name);

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

        /// <summary>
        /// 매핑에 따라 TeachingPositions 정규화 (불필요 축 제거 / 누락 축 추가)
        /// </summary>
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

        /// <summary>
        /// Position 이름 기준 허용 축 목록 반환
        /// </summary>
        public IReadOnlyList<string> GetAxisNamesForPosition(string positionName)
        {
            if (string.IsNullOrWhiteSpace(positionName)) return new List<string>();
            if (System.Enum.TryParse<TeachingPositionName>(positionName, out var en))
            {
                if (_axisMap.TryGetValue(en, out var arr)) return arr;
            }
            // 기본(백워드 호환) 두 축 모두
            return new[] { AxisNames.AlignT, AxisNames.IndexZ };
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