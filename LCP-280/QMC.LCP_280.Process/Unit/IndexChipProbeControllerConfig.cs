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
    /// IndexChipProbeControllerConfig
    ///  - Probe Z / Probe Card XYZ / Sphere Z Teaching Positions
    ///  - Sphere FW/BW Cylinder + Probe Card Vacuum I/O 명칭 상수화
    ///  - OutputStageConfig 패턴 구조 적용
    /// </summary>
    public class IndexChipProbeControllerConfig : BaseConfig
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
            public const string PROBE_VAC_VLV  = "PROBE CARD VACUUM";  // Y075 (Vac valve or combined channel)
        }

        public enum TeachingPositionName
        {
            TopContact_Index1,
            TopContact_Index2,
            TopContact_Index3,
            TopContact_Index4,
            TopContact_Index5,
            TopContact_Index6,
            TopContact_Index7,
            TopContact_Index8,
            aiting,
            Ready
            // 필요시 확장
        }

        public List<TeachingPosition> TeachingPositions { get; set; } = new List<TeachingPosition>();

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

        public IndexChipProbeControllerConfig() : base("IndexChipProbeControllerConfig") { }

        /// <summary>Teaching Position 기본 생성</summary>
        public void InitializeDefaultTeachingPositions()
        {
            if (TeachingPositions == null) TeachingPositions = new List<TeachingPosition>();
            var existing = new HashSet<string>(TeachingPositions.Select(tp => tp.Name));
            foreach (TeachingPositionName name in System.Enum.GetValues(typeof(TeachingPositionName)))
            {
                var posName = name.ToString();
                if (!existing.Contains(posName))
                {
                    var axisPositions = new Dictionary<string, double>
                    {
                        { "Probe Z Axis", 0.0 },
                        { "Probe Card X Axis", 0.0 },
                        { "Probe Card Y Axis", 0.0 },
                        { "Probe Card Z Axis", 0.0 },
                        { "Sphere Z Axis", 0.0 }
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
            var pure = TeachingPositions
                .Select(tp => new TeachingPosition(tp.Name, tp.AxisPositions, tp.Description) { ExtraInfo = tp.ExtraInfo })
                .ToList();
            var original = TeachingPositions; TeachingPositions = pure;
            try { return Save(); }
            finally { TeachingPositions = original; }
        }

        /// <summary>Config 로드 후 축 바인딩</summary>
        public int LoadAndBindAxes(MotionAxisManager axisManager)
        {
            int result = Load(); if (result != 0) return result;
            foreach (var tp in TeachingPositions)
                tp.BindAxes(axisManager, "Unit");
            return 0;
        }
    }
}