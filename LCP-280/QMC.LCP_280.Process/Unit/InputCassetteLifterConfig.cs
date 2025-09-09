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
    /// InputCassetteLifterConfig
    ///  - Wafer Lifter (Input side) Teaching Positions 관리
    ///  - Cassette / RingJut / Mapping Sensor 입력 IO 정의
    ///  - OutputStageConfig 와 동일한 패턴(내부 IO 상수, Hard I/O, Save/Load)
    /// </summary>
    public class InputCassetteLifterConfig : BaseConfig
    {
        internal static class IO
        {
            public const string CASSETTE_CHECK0 = "WAFER LIFTER CASSETTE CHECK 0"; // X016
            public const string CASSETTE_CHECK1 = "WAFER LIFTER CASSETTE CHECK 1"; // X017
            public const string RING_JUT_CHECK  = "WAFER LIFTER RING JUT CHECK";   // X018
            public const string MAPPING_SENSOR  = "WAFER MAPPING";                 // X019
        }

        public enum TeachingPositionName
        {
            MappingStart,
            MappingEnd,
            SlotPitch,
            SlotCount,
            Ready,
            // 필요시 확장
        }

        public List<TeachingPosition> TeachingPositions { get; set; } = new List<TeachingPosition>();

        #region Hard IO Tables
        [JsonIgnore]
        public HardInputDef[] HardInputs => _hardInputs;
        private static readonly HardInputDef[] _hardInputs = new[]
        {
            new HardInputDef { No = 1, Name = IO.CASSETTE_CHECK0, Disp = "X016" },
            new HardInputDef { No = 2, Name = IO.CASSETTE_CHECK1, Disp = "X017" },
            new HardInputDef { No = 3, Name = IO.RING_JUT_CHECK,  Disp = "X018" },
            new HardInputDef { No = 4, Name = IO.MAPPING_SENSOR,  Disp = "X019" },
        };

        [JsonIgnore]
        public HardOutputDef[] HardOutputs => _hardOutputs; // currently none
        private static readonly HardOutputDef[] _hardOutputs = new HardOutputDef[0];
        #endregion

        public InputCassetteLifterConfig() : base("InputCassetteLifterConfig") { }

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
                        { "Wafer Stage Y Axis", 200.0 }
                    };
                    TeachingPositions.Add(new TeachingPosition(posName, axisPositions, $"기본 {posName} 위치"));
                }
            }
            Saveconfig();
        }

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

        public int Saveconfig()
        {
            var pure = TeachingPositions
                .Select(tp => new TeachingPosition(tp.Name, tp.AxisPositions, tp.Description) { ExtraInfo = tp.ExtraInfo })
                .ToList();
            var original = TeachingPositions; TeachingPositions = pure;
            try { return Save(); }
            finally { TeachingPositions = original; }
        }

        public int LoadAndBindAxes(MotionAxisManager axisManager)
        {
            int rc = Load(); if (rc != 0) return rc;
            foreach (var tp in TeachingPositions)
                tp.BindAxes(axisManager, "Unit");
            return 0;
        }
    }
}