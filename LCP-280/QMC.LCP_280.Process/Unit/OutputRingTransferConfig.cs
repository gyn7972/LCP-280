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
    /// OutputRingTransferConfig
    ///  - Bin Feeder (Ring Transfer - Output side) Teaching Position 관리
    ///  - Lift / Clamp 관련 IO 명칭 상수화
    ///  - Hard IO 테이블 및 저장/로드
    /// </summary>
    public class OutputRingTransferConfig : BaseConfig
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

        public List<TeachingPosition> TeachingPositions { get; set; } = new List<TeachingPosition>();

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

        public OutputRingTransferConfig() : base("OutputRingTransferConfig") { }

        /// <summary>enum 기반 기본 Teaching Position 생성</summary>
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
                        { AxisNames.WaferFeederY, 0.0 }
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

        /// <summary>Config 로드 후 TeachingPosition 축 바인딩</summary>
        public int LoadAndBindAxes(MotionAxisManager axisManager)
        {
            int result = Load(); if (result != 0) return result;
            foreach (var tp in TeachingPositions)
                tp.BindAxes(axisManager, "Unit");
            return 0;
        }
    }
}
