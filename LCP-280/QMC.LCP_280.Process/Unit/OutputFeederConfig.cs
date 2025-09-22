using Newtonsoft.Json;
using QMC.Common;
using QMC.Common.Component;
using QMC.Common.Motions;
using QMC.Common.Unit;
using QMC.LCP_280.Process.Component;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QMC.LCP_280.Process.Unit
{
    /// <summary>
    /// OutputFeederConfig
    ///  - Bin Feeder (Ring Transfer - Output side) Teaching Position 관리
    ///  - Lift / Clamp 관련 IO 명칭 상수화
    ///  - Hard IO 테이블 및 저장/로드
    ///  - (추가) TeachingPosition 별 허용 축 필터링 기능
    /// </summary>
    public class OutputFeederConfig : BaseConfig, IPropertyOrderProvider
    {
        /// <summary>장치 IO 명칭 상수 집합</summary>
        internal static class IO
        {
            // Inputs
            public const string FEEDER_UP         = "BIN FEEDER UP";              // X064
            public const string FEEDER_DOWN       = "BIN FEEDER DOWN";            // X065
            public const string FEEDER_UNCLAMP    = "BIN FEEDER UNCLAMP";         // X066 (Open 상태 확인)
            public const string FEEDER_RING_CHECK = "BIN FEEDER RING CHECK";      // X067
            public const string FEEDER_OVERLOAD   = "BIN FEEDER OVERLOAD CHECK";  // X068

            // Outputs (원본 Config 의 DOWNE / UNCALMP 오타를 정규화하여 사용)
            public const string FEEDER_UP_VALVE      = "BIN FEEDER UP";        // Y034 Up 솔
            public const string FEEDER_DOWN_VALVE    = "BIN FEEDER DOWN";      // Y035 Down 솔 (원본: DOWNE)
            public const string FEEDER_CLAMP_VALVE   = "BIN FEEDER CLAMP";     // Y036 Clamp
            public const string FEEDER_UNCLAMP_VALVE = "BIN FEEDER UNCLAMP";   // Y037 Unclamp (원본: UNCALMP)
        }

        public enum TeachingPositionName
        {
            Ready,
            Stage,
            Barcode,
            Cassette,
            SetPosition   // Positive 를 홈으로 설정, CurrentPosition 변경 용도  
            // 필요시 확장
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
        /// 현재 모든 포지션은 WaferFeederY 단일 축만 사용. 추후 확장 대비 구조 유지.
        /// </summary>
        [JsonIgnore]
        private static readonly Dictionary<TeachingPositionName, string[]> _axisMap = new Dictionary<TeachingPositionName, string[]>
        {
            { TeachingPositionName.Ready,      new [] { AxisNames.BinFeederY } },
            { TeachingPositionName.Stage,      new [] { AxisNames.BinFeederY } },
            { TeachingPositionName.Barcode,    new [] { AxisNames.BinFeederY } },
            { TeachingPositionName.Cassette,   new [] { AxisNames.BinFeederY } },
            { TeachingPositionName.SetPosition,new [] { AxisNames.BinFeederY } },
        };

        #region Hard IO Tables
        [JsonIgnore]
        public HardInputDef[] HardInputs => _hardInputs;
        private static readonly HardInputDef[] _hardInputs = new[]
        {
            new HardInputDef { No = 1, Name = IO.FEEDER_UP,         Disp = "X064" },
            new HardInputDef { No = 2, Name = IO.FEEDER_DOWN,       Disp = "X065" },
            new HardInputDef { No = 3, Name = IO.FEEDER_UNCLAMP,    Disp = "X066" },
            new HardInputDef { No = 4, Name = IO.FEEDER_RING_CHECK, Disp = "X067" },
            new HardInputDef { No = 5, Name = IO.FEEDER_OVERLOAD,   Disp = "X068" }
        };

        [JsonIgnore]
        public HardOutputDef[] HardOutputs => _hardOutputs;
        private static readonly HardOutputDef[] _hardOutputs = new[]
        {
            new HardOutputDef { No = 1, Name = IO.FEEDER_UP_VALVE,      Disp = "Y034" },
            new HardOutputDef { No = 2, Name = IO.FEEDER_DOWN_VALVE,    Disp = "Y035" },
            new HardOutputDef { No = 3, Name = IO.FEEDER_CLAMP_VALVE,   Disp = "Y036" },
            new HardOutputDef { No = 4, Name = IO.FEEDER_UNCLAMP_VALVE, Disp = "Y037" }
        };
        #endregion

        public OutputFeederConfig() : base("OutputFeederConfig") { }

        /// <summary>enum 기반 기본 Teaching Position 생성 + 축 매핑 적용</summary>
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

        /// <summary>Config 로드 후 TeachingPosition 축 바인딩 + 축 매핑 적용</summary>
        public int LoadAndBindAxes(MotionAxisManager axisManager)
        {
            int rc = Load();
            if (rc != 0) return rc;
            ApplyAxisMapping();
            foreach (var tp in TeachingPositions)
                tp.BindAxes(axisManager, "Unit");
            return 0;
        }
       
        /// <summary>TeachingPositions 의 AxisPositions 를 허용 축만 유지/누락 축 추가</summary>
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

        /// <summary>문자열 Position 이름으로 허용 축 목록 반환</summary>
        public IReadOnlyList<string> GetAxisNamesForPosition(string positionName)
        {
            if (string.IsNullOrWhiteSpace(positionName)) return new List<string>();
            if (System.Enum.TryParse<TeachingPositionName>(positionName, out var en))
            {
                if (_axisMap.TryGetValue(en, out var arr)) return arr;
            }
            // 기본: BinFeederY 1축
            return new[] { AxisNames.BinFeederY };
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
