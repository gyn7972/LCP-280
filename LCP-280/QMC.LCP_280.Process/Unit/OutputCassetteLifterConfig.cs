using Newtonsoft.Json;
using QMC.Common;
using QMC.Common.Motions;
using QMC.Common.Unit;
using QMC.LCP_280.Process.Component;
using System.Collections.Generic;
using System.Linq;

namespace QMC.LCP_280.Process.Unit
{
    /// <summary>
    /// OutputCassetteLifterConfig
    ///  - Bin Lifter Z Axis Teaching Positions
    ///  - Cassette / RingJut / Mapping sensor IO 정의 (입력 전용)
    ///  - OutputStageConfig 구조와 동일한 패턴(내부 IO 상수 / Hard I/O 테이블 / Save & Load)
    /// </summary>
    public class OutputCassetteLifterConfig : BaseConfig
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
            SlotPitch,
            SlotCount,
            Ready
        }

        public List<TeachingPosition> TeachingPositions { get; set; } = new List<TeachingPosition>();

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

        // 출력 없음: 확장 필요시 HardOutputDef 배열 추가
        [JsonIgnore]
        public HardOutputDef[] HardOutputs => _hardOutputs;
        private static readonly HardOutputDef[] _hardOutputs = new HardOutputDef[0];
        #endregion

        public OutputCassetteLifterConfig() : base("OutputCassetteLifterConfig") { }

        /// <summary>Teaching Position 기본 생성</summary>
        public void InitializeDefaultTeachingPositions()
        {
            if (TeachingPositions == null) TeachingPositions = new List<TeachingPosition>();
            var existing = new HashSet<string>(TeachingPositions.Select(tp => tp.Name));
            foreach (TeachingPositionName name in System.Enum.GetValues(typeof(TeachingPositionName)))
            {
                string posName = name.ToString();
                if (!existing.Contains(posName))
                {
                    var axisPositions = new Dictionary<string, double>
                    {
                        { "Bin Lifter Z Axis", 0.0 }
                    };
                    TeachingPositions.Add(new TeachingPosition(posName, axisPositions, $"기본 {posName} 위치"));
                }
            }
            Saveconfig();
        }

        /// <summary>Teaching Position 추가/갱신</summary>
        public void SetTeachingPosition(TeachingPosition tp)
        {
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
            var purePositions = TeachingPositions
                .Select(tp => new TeachingPosition(tp.Name, tp.AxisPositions, tp.Description) { ExtraInfo = tp.ExtraInfo })
                .ToList();
            var original = TeachingPositions; TeachingPositions = purePositions;
            try { return Save(); }
            finally { TeachingPositions = original; }
        }

        /// <summary>로드 + TeachingPosition 축 바인딩</summary>
        public int LoadAndBindAxes(MotionAxisManager axisManager)
        {
            int rc = Load(); if (rc != 0) return rc;
            foreach (var tp in TeachingPositions)
                tp.BindAxes(axisManager, "Unit");
            return 0;
        }
    }
}