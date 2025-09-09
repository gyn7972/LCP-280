using Newtonsoft.Json;
using QMC.Common;
using QMC.Common.Motions;
using QMC.Common.Unit;
using QMC.LCP_280.Process.Component; // added for TeachingPosition / HardInputDef / HardOutputDef
using System.Collections.Generic;
using System.Linq;

namespace QMC.LCP_280.Process.Unit
{
    /// <summary>
    /// InputRingTransferConfig
    ///  - Teaching Position 정의/저장/로드
    ///  - Wafer Feeder (Ring Transfer) 관련 IO 이름 상수화
    ///  - Hard Input / Output 테이블 제공
    /// </summary>
    public class InputRingTransferConfig : BaseConfig
    {
        /// <summary>
        /// 장치 IO 명칭 상수
        /// </summary>
        internal static class IO
        {
            // Sensors (Inputs)
            public const string FEEDER_UP = "WAFER FEEDER UP";              // X020
            public const string FEEDER_DOWN = "WAFER FEEDER DOWN";            // X021
            public const string FEEDER_UNCLAMP = "WAFER FEEDER UNCLAMP";         // X022 (Open 상태 확인)
            public const string FEEDER_RING_CHECK = "WAFER FEEDER RING CHECK";      // X023
            public const string FEEDER_OVERLOAD = "WAFER FEEDER OVERLOAD CHECK";  // X024

            // Valves (Outputs)
            public const string FEEDER_UP_VALVE = "WAFER FEEDER UP";              // Y016 Up 솔
            public const string FEEDER_DOWN_VALVE = "WAFER FEEDER DOWN";            // Y017 Down 솔 (기존 DOWNE 오타 정규화)
            public const string FEEDER_CLAMP_VALVE = "WAFER FEEDER CLAMP";           // Y018 Clamp (Close)
            public const string FEEDER_UNCLAMP_VALVE = "WAFER FEEDER UNCLAMP";         // Y019 Unclamp (Open)
        }

        public enum TeachingPositionName
        {
            Loading,
            Unloading,
            BarcodeReading,
            Ready,
            SetPosition   // Positive 를 홈으로 설정, CurrentPosition 변경 용도  
            // 필요시 확장
        }

        /// <summary>Teaching Position 순수 목록</summary>
        public List<TeachingPosition> TeachingPositions { get; set; } = new List<TeachingPosition>();

        #region Hard IO Tables
        [JsonIgnore]
        public HardInputDef[] HardInputs => _hardInputs;
        private static readonly HardInputDef[] _hardInputs = new[]
        {
            new HardInputDef { No = 1, Name = IO.FEEDER_UP,         Disp = "X020" },
            new HardInputDef { No = 2, Name = IO.FEEDER_DOWN,       Disp = "X021" },
            new HardInputDef { No = 3, Name = IO.FEEDER_UNCLAMP,    Disp = "X022" },
            new HardInputDef { No = 4, Name = IO.FEEDER_RING_CHECK, Disp = "X023" },
            new HardInputDef { No = 5, Name = IO.FEEDER_OVERLOAD,   Disp = "X024" }
        };

        [JsonIgnore]
        public HardOutputDef[] HardOutputs => _hardOutputs;
        private static readonly HardOutputDef[] _hardOutputs = new[]
        {
            new HardOutputDef { No = 1, Name = IO.FEEDER_UP_VALVE,      Disp = "Y016" },
            new HardOutputDef { No = 2, Name = IO.FEEDER_DOWN_VALVE,    Disp = "Y017" }, // 기존 DOWNE 표기를 DOWN 으로 통일
            new HardOutputDef { No = 3, Name = IO.FEEDER_CLAMP_VALVE,   Disp = "Y018" },
            new HardOutputDef { No = 4, Name = IO.FEEDER_UNCLAMP_VALVE, Disp = "Y019" }
        };
        #endregion

        public InputRingTransferConfig() : base("InputRingTransferConfig") { }

        /// <summary>
        /// enum 에 정의된 TeachingPositionName 목록을 기준으로 기본 포지션을 채움
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
                    var axisPositions = new Dictionary<string, double>
                    {
                        { "Wafer Feeder Y Axis", 100.0 }
                    };
                    TeachingPositions.Add(new TeachingPosition(posName, axisPositions, $"기본 {posName} 위치"));
                }
            }
            Saveconfig();
        }

        /// <summary>Teaching Position 추가 혹은 갱신</summary>
        public void SetTeachingPosition(TeachingPosition tp)
        {
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

        /// <summary>
        /// Config 저장 (축 객체 참조 제거 후 직렬화)
        /// </summary>
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

        /// <summary>
        /// Config 로드 + TeachingPosition 축 바인딩
        /// </summary>
        public int LoadAndBindAxes(MotionAxisManager axisManager)
        {
            int rc = Load(); if (rc != 0) return rc;
            foreach (var tp in TeachingPositions)
                tp.BindAxes(axisManager, "Unit");
            return 0;
        }
    }
}