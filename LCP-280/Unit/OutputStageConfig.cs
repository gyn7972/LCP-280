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
    public class OutputStageConfig : BaseConfig, IPropertyOrderProvider
    {
        // Unified IO constant collection (shared with OutputStage)
        internal static class IO
        {
            // Inputs
            public const string RING_CHECK0 = "BIN STAGE RING CHECK 0";          // X057
            public const string RING_CHECK1 = "BIN STAGE RING CHECK 1";          // X058
            public const string CLAMP_FWD_CHECK = "BIN STAGE CLAMP FWD CHECK";   // X059 (Clamp closed)
            public const string CLAMP_DOWN_CHECK = "BIN STAGE CLAMP DOWN CHECK"; // X060 (Lift down)
            public const string PLATE_UP = "BIN STAGE PLATE UP";                 // X061
            public const string PLATE_DOWN = "BIN STAGE PLATE DOWN";             // X062
            public const string VACUUM_CHECK = "BIN STAGE VACUUM CHECK";         // X063

            // Outputs
            public const string CLAMP_UP = "BIN STAGE CLAMP UP";     // Y028 (Lift Up valve)
            public const string CLAMP_DOWN = "BIN STAGE CLAMP DOWN"; // Y029 (Lift Down valve)
            public const string CLAMP_FWD = "BIN STAGE CLAMP FWD";   // Y030 (Clamp Close)
            public const string CLAMP_BWD = "BIN STAGE CLAMP BWD";   // Y031 (Clamp Open)
            public const string PLATE_UP_OUT = "BIN STAGE PLATE UP";   // Y032 (Plate Up valve)
            public const string PLATE_DOWN_OUT = "BIN STAGE PLATE DOWN"; // Y033 (Plate Down valve)
            public const string VACUUM = "BIN STAGE VACUUM";         // Y088 Vacuum valve
        }

        public enum TeachingPositionName
        {
            Loading,
            Unloading,
            CenterPoint,
            Ready,
            SetPosition,   // Positive 를 홈으로 설정, CurrentPosition 변경 용도  
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
        /// Position 별 허용되는 축 목록.
        /// </summary>
        [JsonIgnore]
        private static readonly Dictionary<TeachingPositionName, string[]> _axisMap = new Dictionary<TeachingPositionName, string[]>
        {
            { TeachingPositionName.Loading,     new [] { AxisNames.BinStageX, AxisNames.BinStageY, AxisNames.BinStageT } },
            { TeachingPositionName.Unloading,   new [] { AxisNames.BinStageX, AxisNames.BinStageY, AxisNames.BinStageT } },
            { TeachingPositionName.CenterPoint, new [] { AxisNames.BinStageX, AxisNames.BinStageY, AxisNames.BinStageT } },
            { TeachingPositionName.Ready,       new [] { AxisNames.BinStageX, AxisNames.BinStageY, AxisNames.BinStageT } },
            { TeachingPositionName.SetPosition, new [] { AxisNames.BinStageX, AxisNames.BinStageY } },
        };


        #region Hard IO Tables
        [JsonIgnore]
        public HardInputDef[] HardInputs => _hardInputs;
        [JsonIgnore]
        private static readonly HardInputDef[] _hardInputs = new[]
        {
            new HardInputDef { No = 1, Name = IO.RING_CHECK0,      Disp = "X057" },
            new HardInputDef { No = 2, Name = IO.RING_CHECK1,      Disp = "X058" },
            new HardInputDef { No = 3, Name = IO.CLAMP_FWD_CHECK,  Disp = "X059" },
            new HardInputDef { No = 4, Name = IO.CLAMP_DOWN_CHECK, Disp = "X060" },
            new HardInputDef { No = 5, Name = IO.PLATE_UP,         Disp = "X061" },
            new HardInputDef { No = 6, Name = IO.PLATE_DOWN,       Disp = "X062" },
            new HardInputDef { No = 7, Name = IO.VACUUM_CHECK,     Disp = "X063" },
        };

        [JsonIgnore]
        public HardOutputDef[] HardOutputs => _hardOutputs;
        [JsonIgnore]
        private static readonly HardOutputDef[] _hardOutputs = new[]
        {
            new HardOutputDef { No = 1, Name = IO.CLAMP_UP,       Disp = "Y028" },
            new HardOutputDef { No = 2, Name = IO.CLAMP_DOWN,     Disp = "Y029" },
            new HardOutputDef { No = 3, Name = IO.CLAMP_FWD,      Disp = "Y030" },
            new HardOutputDef { No = 4, Name = IO.CLAMP_BWD,      Disp = "Y031" },
            new HardOutputDef { No = 5, Name = IO.PLATE_UP_OUT,   Disp = "Y032" },
            new HardOutputDef { No = 6, Name = IO.PLATE_DOWN_OUT, Disp = "Y033" },
            new HardOutputDef { No = 7, Name = IO.VACUUM,         Disp = "Y088" },
        };
        #endregion

        [Category("Interlock"), DisplayName("Safty StageX (mm)")]
        [DefaultValue(20.0)]
        public double SafeStageRectHalfWidthX { get; set; }

        [Category("Interlock"), DisplayName("Safty StageY (mm)")]
        [DefaultValue(20.0)]
        public double SafeStageRectHalfHeightY { get; set; }

        [Category("TCorrection"), DisplayName("TCorrection Use")]
        [DefaultValue(false)]
        [JsonProperty("TCorrectionMode")]
        public bool TCorrectionMode { get; set; }

        [Category("TCorrectionFile"), DisplayName("TCorrectionFile")]
        [DefaultValue("")]
        [JsonProperty("TCorrectionFile")]
        public string TCorrectionFile { get; set; }


        public OutputStageConfig() : base("OutputStageConfig") { }

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

        /// <summary>TeachingPositions 의 AxisPositions 를 허용 축만 유지하고 누락 축은 0으로 추가</summary>
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

        /// <summary>문자열 Position 이름으로 허용 축 목록 반환 (기본: X/Y/T 3축)</summary>
        public IReadOnlyList<string> GetAxisNamesForPosition(string positionName)
        {
            if (string.IsNullOrWhiteSpace(positionName)) return new List<string>();
            if (System.Enum.TryParse<TeachingPositionName>(positionName, out var en))
            {
                if (_axisMap.TryGetValue(en, out var arr)) return arr;
            }
            return new[] { AxisNames.BinStageX, AxisNames.BinStageY, AxisNames.BinStageT };
        }

        #region IPropertyOrderProvider 구현 (Category / Property 표시 순서)
        // Category 순서: Common → Cassette
        public IDictionary<string, int> GetCategoryOrder()
            => new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
            {
                { "General", 0 },   // Name 속성 (Category 없음) 정렬 위치 지정
                { "Common", 1 },
                { "Interlock", 2 },
                { "TCorrection", 3 },
                { "TCorrectionFile", 4 },
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

        /// 개별 Teaching Position 에 적용할 오프셋 (X / Y / T)
        public Dictionary<string, (double dx, double dy, double dt)> Offsets { get; set; } = new Dictionary<string, (double dx, double dy, double dt)>();
        
        public (double x, double y, double t) GetPositionWithOffset(string name)
        {
            var tp = GetTeachingPosition(name);
            if (tp == null) return (0, 0, 0);
            double x = tp.AxisPositions.TryGetValue(AxisNames.BinStageX, out var vx) ? vx : 0;
            double y = tp.AxisPositions.TryGetValue(AxisNames.BinStageY, out var vy) ? vy : 0;
            double t = tp.AxisPositions.TryGetValue(AxisNames.BinStageT, out var vt) ? vt : 0;
            if (Offsets.TryGetValue(name, out var off)) { x += off.dx; y += off.dy; t += off.dt; }
            return (x, y, t);
        }

        public void SetOffset(string name, double dx, double dy, double dt)
        {
            Offsets[name] = (dx, dy, dt);
            Saveconfig();
        }
        #endregion
    }
}