using Newtonsoft.Json;
using QMC.Common;
using QMC.Common.Motions;
using QMC.Common.Unit;
using QMC.LCP_280.Process.Component;
using System.Collections.Generic;
using System.Linq;
using System; // Enum

namespace QMC.LCP_280.Process.Unit
{
    /// <summary>
    /// InputCassetteLifterConfig
    ///  - Wafer Lifter (Input side) Teaching Positions 관리 (축 매핑/필터링 적용)
    ///  - Cassette / RingJut / Mapping Sensor 입력 IO 정의
    ///  - OutputStageConfig 패턴 + Axis filtering (다른 Unit들과 일관성)
    /// </summary>
    public class InputCassetteLifterConfig : BaseConfig
    {
        internal static class IO
        {
            public const string CASSETTE_CHECK0 = "WAFER LIFTER CASSETTE CHECK 0"; // X016
            public const string CASSETTE_CHECK1 = "WAFER LIFTER CASSETTE CHECK 1"; // X017
            public const string WAFER_PROTRUSION_DETECTION_SENSOR  = "WAFER LIFTER RING JUT CHECK";   // X018
            public const string MAPPING_SENSOR  = "WAFER MAPPING";                 // X019
        }
        

        public enum TeachingPositionName
        {
            CassetteSlot_1,
            MappingStart,
            MappingEnd,
            SlotPitch,
            SlotCount,
            UnloadOffset,
            LoadPort
        }

        /// <summary>
        /// Position → 허용 축 목록 매핑 (필요시 일부 Position만 축 사용하도록 조정 가능)
        /// 현재는 모든 포지션이 Lifter Z 사용하도록 설정 (SlotPitch/SlotCount 등도 위치 측정 가능성 고려)
        /// 축이 필요 없는 항목을 비우고 싶다면 해당 배열을 new string[0] 로 변경
        /// </summary>
        [JsonIgnore]
        private static readonly Dictionary<TeachingPositionName, string[]> _axisMap = new Dictionary<TeachingPositionName, string[]>
        {
            { TeachingPositionName.CassetteSlot_1, new [] { AxisNames.WaferLifterZ } },
            { TeachingPositionName.MappingStart,   new [] { AxisNames.WaferLifterZ } },
            { TeachingPositionName.MappingEnd,     new [] { AxisNames.WaferLifterZ } },
            { TeachingPositionName.SlotPitch,      new [] { AxisNames.WaferLifterZ } },
            { TeachingPositionName.SlotCount,      new [] { AxisNames.WaferLifterZ } },
            { TeachingPositionName.UnloadOffset,   new [] { AxisNames.WaferLifterZ } },
            { TeachingPositionName.LoadPort,       new [] { AxisNames.WaferLifterZ } },
        };

        public List<TeachingPosition> TeachingPositions { get; set; } = new List<TeachingPosition>();

        #region Hard IO Tables
        [JsonIgnore]
        public HardInputDef[] HardInputs => _hardInputs;
        private static readonly HardInputDef[] _hardInputs = new[]
        {
            new HardInputDef { No = 1, Name = IO.CASSETTE_CHECK0, Disp = "X016" },
            new HardInputDef { No = 2, Name = IO.CASSETTE_CHECK1, Disp = "X017" },
            new HardInputDef { No = 3, Name = IO.WAFER_PROTRUSION_DETECTION_SENSOR,  Disp = "X018" },
            new HardInputDef { No = 4, Name = IO.MAPPING_SENSOR,  Disp = "X019" },
        };

        [JsonIgnore]
        public HardOutputDef[] HardOutputs => _hardOutputs; // currently none
        private static readonly HardOutputDef[] _hardOutputs = new HardOutputDef[0];
        #endregion

        public InputCassetteLifterConfig() : base("InputCassetteLifterConfig") { }

        /// <summary>
        /// 기본 Teaching Position 생성 (축 매핑 적용)
        /// </summary>
        public void InitializeDefaultTeachingPositions()
        {
            if (TeachingPositions == null) TeachingPositions = new List<TeachingPosition>();
            var existing = new HashSet<string>(TeachingPositions.Select(tp => tp.Name));
            foreach (TeachingPositionName name in Enum.GetValues(typeof(TeachingPositionName)))
            {
                string posName = name.ToString();
                if (!existing.Contains(posName))
                {
                    var axes = GetAxisNamesForPosition(posName);
                    var axisPositions = new Dictionary<string, double>();
                    foreach (var a in axes)
                    {
                        double init = (a == AxisNames.WaferLifterZ) ? 200.0 : 0.0; // 기존 기본값 유지
                        axisPositions[a] = init;
                    }
                    TeachingPositions.Add(new TeachingPosition(posName, axisPositions, $"기본 {posName} 위치"));
                }
            }
            ApplyAxisMapping();
            Saveconfig();
        }

        /// <summary>
        /// Position 추가/갱신 (허용된 축만 유지, 누락 축 기본값 삽입)
        /// </summary>
        public void SetTeachingPosition(TeachingPosition tp)
        {
            var allowed = GetAxisNamesForPosition(tp.Name).ToHashSet();
            var filtered = new Dictionary<string, double>();
            var src = tp.AxisPositions ?? new Dictionary<string, double>();
            foreach (var a in allowed)
            {
                double init = (a == AxisNames.WaferLifterZ) ? 200.0 : 0.0;
                if (src.TryGetValue(a, out var v)) filtered[a] = v; else filtered[a] = init;
            }
            tp.AxisPositions = filtered;

            var exist = TeachingPositions.FirstOrDefault(p => p.Name == tp.Name);
            if (exist != null)
            {
                exist.AxisPositions = tp.AxisPositions;
                exist.Description   = tp.Description;
                exist.ExtraInfo     = tp.ExtraInfo;
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
            var original = TeachingPositions; TeachingPositions = pure;
            try { return Save(); }
            finally { TeachingPositions = original; }
        }

        /// <summary>로드 + 축 바인딩 + 축 매핑 동기화</summary>
        public int LoadAndBindAxes(MotionAxisManager axisManager)
        {
            int rc = Load(); if (rc != 0) return rc;
            ApplyAxisMapping();
            foreach (var tp in TeachingPositions)
                tp.BindAxes(axisManager, "Unit");
            return 0;
        }

        /// <summary>
        /// 매핑에 따라 TeachingPositions 보정 (불필요 축 제거 / 누락 축 기본값 추가)
        /// </summary>
        public void ApplyAxisMapping()
        {
            foreach (var tp in TeachingPositions)
            {
                var allowed = GetAxisNamesForPosition(tp.Name).ToHashSet();
                var current = tp.AxisPositions ?? new Dictionary<string, double>();
                var next = new Dictionary<string, double>();
                foreach (var a in allowed)
                {
                    double init = (a == AxisNames.WaferLifterZ) ? 200.0 : 0.0;
                    if (current.TryGetValue(a, out var v)) next[a] = v; else next[a] = init;
                }
                tp.AxisPositions = next;
            }
        }

        /// <summary>
        /// Position 이름 기반 허용 축 목록 반환
        /// </summary>
        public IReadOnlyList<string> GetAxisNamesForPosition(string positionName)
        {
            if (string.IsNullOrWhiteSpace(positionName)) return new string[0];
            if (Enum.TryParse<TeachingPositionName>(positionName, out var en))
            {
                if (_axisMap.TryGetValue(en, out var arr)) return arr;
            }
            // 기본(백워드 호환): Lifter Z 1축
            return new[] { AxisNames.WaferLifterZ };
        }
    }
}