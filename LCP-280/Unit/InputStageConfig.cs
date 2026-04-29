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
    /// InputStageConfig
    ///  - Teaching Position 기본 세트 및 저장/로드
    ///  - 실제 장치 IO 이름을 상수화 (내부 IO 클래스)
    ///  - Hard I/O 테이블 (스캔/바인딩용) 정의
    ///  - Teaching Position Offset (X/Y/T 보정) 추가 관리 (OutputStageConfig 와의 차별점)
    ///  - (추가) TeachingPosition 별 허용 축 필터링 기능
    /// </summary>
    public class InputStageConfig : BaseConfig, IPropertyOrderProvider
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
            public const string CLAMP_DOWN_SNS  = "WAFER STAGE CLAMP DOWN";    // X027
            public const string CLAMP_FWD_SNS   = "WAFER STAGE CLAMP";         // X028
            public const string EXPANDER_UP_SNS = "WAFER STAGE EXPANDER UP";   // X029
            public const string EXPANDER_DOWN_SNS = "WAFER STAGE EXPANDER DOWN"; // X030
            public const string VAC_OK_SNS      = "EJECTOR VACUUM CHECK";      // X031

            // Outputs (Valve)
            public const string CLAMP_UP_OUT     = "WAFER STAGE CLAMP UP";      // Y020
            public const string CLAMP_DOWN_OUT   = "WAFER STAGE CLAMP DOWN";    // Y021
            public const string CLAMP_FWD_OUT    = "WAFER STAGE CLAMP";         // Y022
            public const string CLAMP_BWD_OUT    = "WAFER STAGE UNCLAMP";       // Y023
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
        /// Position 이름별 허용 축 목록.
        /// </summary>
        [JsonIgnore]
        private static readonly Dictionary<TeachingPositionName, string[]> _axisMap = new Dictionary<TeachingPositionName, string[]>
        {
            { TeachingPositionName.Loading,     new [] { AxisNames.WaferStageX, AxisNames.WaferStageY, AxisNames.WaferStageT } },
            { TeachingPositionName.Unloading,   new [] { AxisNames.WaferStageX, AxisNames.WaferStageY, AxisNames.WaferStageT } },
            { TeachingPositionName.CenterPoint, new [] { AxisNames.WaferStageX, AxisNames.WaferStageY, AxisNames.WaferStageT } },
            { TeachingPositionName.Ready,       new [] { AxisNames.WaferStageX, AxisNames.WaferStageY, AxisNames.WaferStageT } },
            { TeachingPositionName.SetPosition, new [] { AxisNames.WaferStageY } },
        };

        
        /// <summary>
        /// 개별 Teaching Position 에 적용할 오프셋 (X / Y / T)
        /// </summary>
        public Dictionary<string, (double dx, double dy, double dt)> Offsets { get; set; } = new Dictionary<string, (double dx, double dy, double dt)>();


        //
        //[Category("Limit"), DisplayName("safeHalfRangeX(mm)")]
        //[DefaultValue(0.0)]
        //public double dSafeHalfRangeX { get; set; } = 0.0;

        //[Category("Limit"), DisplayName("safeHalfRangeY(mm)")]
        //[DefaultValue(0.0)]
        //public double dSafeHalfRangeY { get; set; } = 0.0;



        [Category("Interlock"), DisplayName("Safty Stage Radius")]
        [DefaultValue(60.0)]
        public double SafeSatageRaius
        {
            get
            {
                return dSafeSatageRaius;
            }
            set
            {
                if (value < 20)
                {
                    value = 20;
                }
                else if (value > 75)
                {
                    value = 75;
                }
                dSafeSatageRaius = value;
            }
        }

        public double dSafeSatageRaius;


        [Category("OffsetDie"), DisplayName("OffsetDieX(mm)")]
        [DefaultValue(0.0)]
        public double dOffsetDieX { get; set; } = 0.0;

        [Category("OffsetDie"), DisplayName("OffsetDieY(mm)")]
        [DefaultValue(0.0)]
        public double dOffsetDieY { get; set; } = 0.0;


        // Motion Done 관련 옵션
        public bool   EnablePredictiveControl   { get; set; } = false;
        public double MoveDoneRemainDistance    { get; set; } = 0.005;

        #region Hard IO Tables
        [JsonIgnore]
        public HardInputDef[] HardInputs => _hardInputs;
        private static readonly HardInputDef[] _hardInputs = new[]
        {
            new HardInputDef { No = 1, Name = IO.RING_CHECK0,       Disp = "X025" },
            new HardInputDef { No = 2, Name = IO.RING_CHECK1,       Disp = "X026" },
            new HardInputDef { No = 3, Name = IO.CLAMP_DOWN_SNS,    Disp = "X027" },
            new HardInputDef { No = 4, Name = IO.CLAMP_FWD_SNS,     Disp = "X028" },
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
            new HardOutputDef { No = 3, Name = IO.CLAMP_FWD_OUT,     Disp = "Y022" },
            new HardOutputDef { No = 4, Name = IO.CLAMP_BWD_OUT,     Disp = "Y023" },
            new HardOutputDef { No = 5, Name = IO.EXPANDER_UP_OUT,   Disp = "Y024" },
            new HardOutputDef { No = 6, Name = IO.EXPANDER_DOWN_OUT, Disp = "Y025" },
            new HardOutputDef { No = 7, Name = IO.VAC_OUT,           Disp = "Y038" },
        };
        #endregion

        public InputStageConfig() : base("InputStageConfig") { }

        /// <summary>
        /// Teaching Position 기본 세트를 생성 (이미 존재하면 건너뜀) + Offsets 초기화 + 축 매핑 적용
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

        /// <summary>Teaching Position 추가/갱신 (허용 축 필터링) + Offset 기본값 보장</summary>
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

        /// <summary>
        /// Teaching Position + Offset 적용 좌표 반환 (X/Y/T)
        /// </summary>
        public (double x, double y, double t) GetPositionWithOffset(string name)
        {
            var tp = GetTeachingPosition(name);
            if (tp == null) return (0, 0, 0);
            double x = tp.AxisPositions.TryGetValue(AxisNames.WaferStageX, out var vx) ? vx : 0;
            double y = tp.AxisPositions.TryGetValue(AxisNames.WaferStageY, out var vy) ? vy : 0;
            double t = tp.AxisPositions.TryGetValue(AxisNames.WaferStageT, out var vt) ? vt : 0;
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
            var backup = TeachingPositions;
            TeachingPositions = pure;
            try { return Save(); }
            finally { TeachingPositions = backup; }
        }

        /// <summary>
        /// Config 로드 후 Axis Binding 수행 + Offset 키 보정 + 축 매핑 적용
        /// </summary>
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

        /// <summary>TeachingPositions 의 AxisPositions 를 허용 축만 남기고 누락 축 추가</summary>
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

        /// <summary>문자열 Position 이름으로 허용 축목록 반환</summary>
        public IReadOnlyList<string> GetAxisNamesForPosition(string positionName)
        {
            if (string.IsNullOrWhiteSpace(positionName)) return new List<string>();
            if (System.Enum.TryParse<TeachingPositionName>(positionName, out var en))
            {
                if (_axisMap.TryGetValue(en, out var arr)) return arr;
            }
            // 기본: X/Y/T 모두 허용
            return new[] { AxisNames.WaferStageX, AxisNames.WaferStageY, AxisNames.WaferStageT };
        }

        #region IPropertyOrderProvider 구현 (Category / Property 표시 순서)
        // Category 순서: Common → Cassette
        public IDictionary<string, int> GetCategoryOrder()
            => new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
            {
                { "General", 0 },   // Name 속성 (Category 없음) 정렬 위치 지정
                { "Common", 1 },
                { "Interlock", 2 },
                { "Limit", 3 },
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