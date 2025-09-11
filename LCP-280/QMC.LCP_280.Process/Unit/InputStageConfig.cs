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
    /// InputStageConfig
    ///  - Teaching Position 기본 세트 및 저장/로드
    ///  - 실제 장치 IO 이름을 상수화 (내부 IO 클래스)
    ///  - Hard I/O 테이블 (스캔/바인딩용) 정의
    ///  - Teaching Position Offset (X/Y/T 보정) 추가 관리 (OutputStageConfig 와의 차별점)
    /// </summary>
    public class InputStageConfig : BaseConfig
    {
        /// <summary>
        /// 장치에서 사용하는 실 I/O 명칭을 한 곳에 모아둔 상수 클래스
        /// (OutputStageConfig.IO 와 유사한 형태 유지)
        /// </summary>
        internal static class IO
        {
            // Inputs (Sensor)
            public const string RING_CHECK0     = "WAFER STAGE RING CHECK 0";  // X025
            public const string RING_CHECK1     = "WAFER STAGE RING CHECK 1";  // X026
            public const string CLAMP_DOWN_SNS  = "WAFER STAGE CLAMP DOWN";    // X027 (클램프 Down 위치 감지)
            public const string CLAMP_FWD_SNS       = "WAFER STAGE CLAMP";         // X028 (클램프 Up/Clamp 상태)
            public const string EXPANDER_UP_SNS = "WAFER STAGE EXPANDER UP";   // X029
            public const string EXPANDER_DOWN_SNS = "WAFER STAGE EXPANDER DOWN"; // X030
            public const string VAC_OK_SNS      = "EJECTOR VACUUM CHECK";      // X031

            // Outputs (Valve)
            public const string CLAMP_UP_OUT     = "WAFER STAGE CLAMP UP";      // Y020 (Lift Up)
            public const string CLAMP_DOWN_OUT   = "WAFER STAGE CLAMP DOWN";    // Y021 (Lift Down)
            public const string CLAMP_FWD_OUT        = "WAFER STAGE CLAMP";         // Y022 (Clamp Forward / Close)
            public const string CLAMP_BWD_OUT      = "WAFER STAGE UNCLAMP";       // Y023 (Clamp Back / Open)
            public const string EXPANDER_UP_OUT  = "WAFER STAGE EXPANDER UP";   // Y024
            public const string EXPANDER_DOWN_OUT= "WAFER STAGE EXPANDER DOWN"; // Y025
            public const string VAC_OUT          = "EJECTOR VACUUM";            // Y038
        }

        /// <summary>Teaching Position 사전 정의 이름</summary>
        public enum TeachingPositionName
        {
            Loading,
            Unloading,
            CenterPoint,
            Ready,
            SetPosition   // Positive 를 홈으로 설정, CurrentPosition 변경 용도  
            // 필요시 확장
        }

        /// <summary>Teaching Position 목록 (순수 위치/설명)</summary>
        public List<TeachingPosition> TeachingPositions { get; set; } = new List<TeachingPosition>();

        /// <summary>
        /// 개별 Teaching Position 에 적용할 오프셋 (X / Y / T)
        ///  - OutputStage 와 다르게 InputStage 는 공정 중 교정된 Fine Offset 을 활용하기 위해 별도 유지
        /// </summary>
        public Dictionary<string, (double dx, double dy, double dt)> Offsets { get; set; } = new Dictionary<string, (double dx, double dy, double dt)>();

        // ==== Motion Done 관련 옵션 (예측 제어 등 확장 기능) ====
        public bool   EnablePredictiveControl   { get; set; } = false;
        public double MoveDoneRemainDistance    { get; set; } = 0.005; // mm 잔여 허용치

        #region Hard I/O 테이블 정의
        [JsonIgnore]
        public HardInputDef[] HardInputs => _hardInputs;
        private static readonly HardInputDef[] _hardInputs = new[]
        {
            new HardInputDef { No = 1, Name = IO.RING_CHECK0,       Disp = "X025" },
            new HardInputDef { No = 2, Name = IO.RING_CHECK1,       Disp = "X026" },
            new HardInputDef { No = 3, Name = IO.CLAMP_DOWN_SNS,    Disp = "X027" },
            new HardInputDef { No = 4, Name = IO.CLAMP_FWD_SNS,         Disp = "X028" },
            new HardInputDef { No = 5, Name = IO.EXPANDER_UP_SNS,   Disp = "X029" },
            new HardInputDef { No = 6, Name = IO.EXPANDER_DOWN_SNS, Disp = "X030" },
            new HardInputDef { No = 7, Name = IO.VAC_OK_SNS,        Disp = "X031" },
        };

        [JsonIgnore]
        public HardOutputDef[] HardOutputs => _hardOutputs;
        private static readonly HardOutputDef[] _hardOutputs = new[]
        {
            new HardOutputDef { No = 1, Name = IO.CLAMP_UP_OUT,      Disp = "Y020" },
            new HardOutputDef { No = 2, Name = IO.CLAMP_DOWN_OUT,    Disp = "Y021" },
            new HardOutputDef { No = 3, Name = IO.CLAMP_FWD_OUT,         Disp = "Y022" },
            new HardOutputDef { No = 4, Name = IO.CLAMP_BWD_OUT,       Disp = "Y023" },
            new HardOutputDef { No = 5, Name = IO.EXPANDER_UP_OUT,   Disp = "Y024" },
            new HardOutputDef { No = 6, Name = IO.EXPANDER_DOWN_OUT, Disp = "Y025" },
            new HardOutputDef { No = 7, Name = IO.VAC_OUT,           Disp = "Y038" },
        };
        #endregion

        public InputStageConfig() : base("InputStageConfig") { }

        /// <summary>
        /// Teaching Position 기본 세트를 생성 (이미 존재하면 건너뜀)
        /// + Offsets 초기화
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
                        { AxisNames.WaferStageX, 0.0 },
                        { AxisNames.WaferStageY, 0.0 },
                        { AxisNames.WaferStageT, 0.0 }
                    };
                    TeachingPositions.Add(new TeachingPosition(posName, axisPositions, $"Default {posName} Position"));
                }
                if (!Offsets.ContainsKey(posName)) Offsets[posName] = (0, 0, 0);
            }
            Saveconfig();
        }

        /// <summary>Teaching Position 추가/갱신 + Offset 기본값 보장</summary>
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
            if (!Offsets.ContainsKey(tp.Name)) Offsets[tp.Name] = (0, 0, 0);
            Saveconfig();
        }

        public TeachingPosition GetTeachingPosition(string name) => TeachingPositions.FirstOrDefault(p => p.Name == name);

        /// <summary>
        /// Teaching Position + Offset 적용 좌표 반환 (X/Y/T)
        /// </summary>
        public (double x, double y, double t) GetPositionWithOffset(string name)
        {
            var tp = GetTeachingPosition(name);
            if (tp == null) return (0, 0, 0);
            double x = tp.AxisPositions.TryGetValue("Wafer Stage X Axis", out var vx) ? vx : 0;
            double y = tp.AxisPositions.TryGetValue("Wafer Stage Y Axis", out var vy) ? vy : 0;
            double t = tp.AxisPositions.TryGetValue("Wafer Stage T Axis", out var vt) ? vt : 0;
            if (Offsets.TryGetValue(name, out var off)) { x += off.dx; y += off.dy; t += off.dt; }
            return (x, y, t);
        }

        /// <summary>개별 Teaching Position 에 Offset 설정</summary>
        public void SetOffset(string name, double dx, double dy, double dt)
        {
            Offsets[name] = (dx, dy, dt);
            Saveconfig();
        }

        /// <summary>
        /// Config 저장 (TeachingPositions 를 순수 데이터 형태로 직렬화)
        /// </summary>
        public int Saveconfig()
        {
            var pure = TeachingPositions
                .Select(tp => new TeachingPosition(tp.Name, tp.AxisPositions, tp.Description) { ExtraInfo = tp.ExtraInfo })
                .ToList();
            var original = TeachingPositions;
            TeachingPositions = pure;
            try { return Save(); }
            finally { TeachingPositions = original; }
        }

        /// <summary>
        /// Config 로드 후 Axis Binding 수행 + Offset 키 보정
        /// </summary>
        public int LoadAndBindAxes(MotionAxisManager axisManager)
        {
            int result = Load();
            if (result != 0) return result;
            foreach (var tp in TeachingPositions)
                tp.BindAxes(axisManager, "Unit");
            foreach (var tp in TeachingPositions)
                if (!Offsets.ContainsKey(tp.Name)) Offsets[tp.Name] = (0, 0, 0);
            return 0;
        }
    }
}