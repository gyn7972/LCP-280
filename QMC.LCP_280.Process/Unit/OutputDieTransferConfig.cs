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
    public class OutputDieTransferConfig : BaseConfig, IPropertyOrderProvider
    {
        public enum TeachingPositionName
        {
            Pickup_Index1,
            Pickup_Index2,
            Pickup_Index3,
            Pickup_Index4,
            Pickup_Index5,
            Pickup_Index6,
            Pickup_Index7,
            Pickup_Index8,
            Place,
            Ready,
            SafetyZone
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
        /// TeachingPositionName 별 허용 축 목록
        /// </summary>
        [JsonIgnore]
        private static readonly Dictionary<TeachingPositionName, string[]> _axisMap = new Dictionary<TeachingPositionName, string[]>
        {
            { TeachingPositionName.Pickup_Index1, new [] { AxisNames.RightToolT, AxisNames.RightPickZ } },
            { TeachingPositionName.Pickup_Index2, new [] { AxisNames.RightToolT, AxisNames.RightPickZ } },
            { TeachingPositionName.Pickup_Index3, new [] { AxisNames.RightToolT, AxisNames.RightPickZ } },
            { TeachingPositionName.Pickup_Index4, new [] { AxisNames.RightToolT, AxisNames.RightPickZ } },
            { TeachingPositionName.Pickup_Index5, new [] { AxisNames.RightToolT, AxisNames.RightPickZ } },
            { TeachingPositionName.Pickup_Index6, new [] { AxisNames.RightToolT, AxisNames.RightPickZ } },
            { TeachingPositionName.Pickup_Index7, new [] { AxisNames.RightToolT, AxisNames.RightPickZ } },
            { TeachingPositionName.Pickup_Index8, new [] { AxisNames.RightToolT, AxisNames.RightPickZ } },
            { TeachingPositionName.Place,         new [] { AxisNames.RightToolT, AxisNames.RightPlaceZ } },
            { TeachingPositionName.Ready,         new [] { AxisNames.RightToolT, AxisNames.RightPickZ } },
            { TeachingPositionName.SafetyZone,      new [] { AxisNames.RightPickZ, AxisNames.RightPlaceZ } },
        };

        

        // ================= IO 이름 상수 (InputStageConfig.IO 패턴과 동일) =================
        internal static class IO
        {
            // Inputs
            public const string AIR_TANK_PRESS = "RIGHT TOOL AIR TANK PRESSURE CHECK";
            public const string VAC_TANK_PRESS = "RIGHT TOOL VACUUM TANK PRESSURE CHECK";
            public const string ARM1_FLOW = "RIGHT TOOL ARM 1 FLOW CHECK";
            public const string ARM2_FLOW = "RIGHT TOOL ARM 2 FLOW CHECK";
            public const string ARM3_FLOW = "RIGHT TOOL ARM 3 FLOW CHECK";
            public const string ARM4_FLOW = "RIGHT TOOL ARM 4 FLOW CHECK";

            // Outputs (Vac / Blow / Vent)
            public const string ARM1_VAC = "RIGHT ARM 1 VACUUM";
            public const string ARM2_VAC = "RIGHT ARM 2 VACUUM";
            public const string ARM3_VAC = "RIGHT ARM 3 VACUUM";
            public const string ARM4_VAC = "RIGHT ARM 4 VACUUM";

            public const string ARM1_BLOW = "RIGHT ARM 1 BLOW";
            public const string ARM2_BLOW = "RIGHT ARM 2 BLOW";
            public const string ARM3_BLOW = "RIGHT ARM 3 BLOW";
            public const string ARM4_BLOW = "RIGHT ARM 4 BLOW";

            public const string ARM1_VENT = "RIGHT ARM 1 VENT";
            public const string ARM2_VENT = "RIGHT ARM 2 VENT";
            public const string ARM3_VENT = "RIGHT ARM 3 VENT";
            public const string ARM4_VENT = "RIGHT ARM 4 VENT";

            // 그룹 배열 (Unit 코드에서 직접 활용)
            public static readonly string[] ARM_FLOW = { ARM1_FLOW, ARM2_FLOW, ARM3_FLOW, ARM4_FLOW };
            public static readonly string[] ARM_VAC = { ARM1_VAC, ARM2_VAC, ARM3_VAC, ARM4_VAC };
            public static readonly string[] ARM_BLOW = { ARM1_BLOW, ARM2_BLOW, ARM3_BLOW, ARM4_BLOW };
            public static readonly string[] ARM_VENT = { ARM1_VENT, ARM2_VENT, ARM3_VENT, ARM4_VENT };
        }

        #region Hard IO Tables
        [JsonIgnore]
        public HardInputDef[] HardInputs => _hardInputs;
        [JsonIgnore]
        private static readonly HardInputDef[] _hardInputs = new[]
        {
            new HardInputDef { No = 1, Name = IO.AIR_TANK_PRESS,  Disp = "X051" },
            new HardInputDef { No = 2, Name = IO.VAC_TANK_PRESS,  Disp = "X052" },
            new HardInputDef { No = 3, Name = IO.ARM1_FLOW, Disp = "X053" },
            new HardInputDef { No = 4, Name = IO.ARM2_FLOW, Disp = "X054" },
            new HardInputDef { No = 5, Name = IO.ARM3_FLOW, Disp = "X055" },
            new HardInputDef { No = 6, Name = IO.ARM4_FLOW, Disp = "X056" }
        };

        [JsonIgnore]
        public HardOutputDef[] HardOutputs => _hardOutputs;
        [JsonIgnore]
        private static readonly HardOutputDef[] _hardOutputs = new[]
        {
            new HardOutputDef { No = 1,  Name = IO.ARM1_VAC,  Disp = "Y076" },
            new HardOutputDef { No = 2,  Name = IO.ARM2_VAC,  Disp = "Y077" },
            new HardOutputDef { No = 3,  Name = IO.ARM3_VAC,  Disp = "Y078" },
            new HardOutputDef { No = 4,  Name = IO.ARM4_VAC,  Disp = "Y079" },
            new HardOutputDef { No = 5,  Name = IO.ARM1_BLOW, Disp = "Y080" },
            new HardOutputDef { No = 6,  Name = IO.ARM2_BLOW, Disp = "Y081" },
            new HardOutputDef { No = 7,  Name = IO.ARM3_BLOW, Disp = "Y082" },
            new HardOutputDef { No = 8,  Name = IO.ARM4_BLOW, Disp = "Y083" },
            new HardOutputDef { No = 9,  Name = IO.ARM1_VENT, Disp = "Y084" },
            new HardOutputDef { No = 10, Name = IO.ARM2_VENT, Disp = "Y085" },
            new HardOutputDef { No = 11, Name = IO.ARM3_VENT, Disp = "Y086" },
            new HardOutputDef { No = 12, Name = IO.ARM4_VENT, Disp = "Y087" }
        };
        #endregion


        [Category("SetupConfig"), DisplayName("IndexOfEnd")]
        [DefaultValue(0)]
        public int IndexOfEnd { get; set; } = 0;


        public OutputDieTransferConfig() : base("OutputDieTransferConfig") { }

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

        public TeachingPosition GetTeachingPosition(string name) => TeachingPositions.FirstOrDefault(p => p.Name == name);

        /// <summary>Offset: positionName -> (T, PickZ, PlaceZ)</summary>
        public Dictionary<string, (double t, double pickZ, double placeZ)> Offsets { get; set; } =
            new Dictionary<string, (double t, double pickZ, double placeZ)>();

        /// <summary>Offset 적용된 목표 좌표</summary>
        public (double t, double pickZ, double placeZ) GetPositionWithOffset(string name)
        {
            var tp = GetTeachingPosition(name);
            if (tp == null) return (0, 0, 0);
            double t = tp.AxisPositions.TryGetValue(AxisNames.LeftToolT, out var vt) ? vt : 0;
            double pz = tp.AxisPositions.TryGetValue(AxisNames.LeftPickZ, out var vpz) ? vpz : 0;
            double plz = tp.AxisPositions.TryGetValue(AxisNames.LeftPlaceZ, out var vplz) ? vplz : 0;
            if (Offsets.TryGetValue(name, out var off)) { t += off.t; pz += off.pickZ; plz += off.placeZ; }
            return (t, pz, plz);
        }

        public void SetOffset(string name, double t, double pickZ, double placeZ)
        {
            Offsets[name] = (t, pickZ, placeZ);
            Saveconfig();
        }

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

        public IReadOnlyList<string> GetAxisNamesForPosition(string positionName)
        {
            if (string.IsNullOrWhiteSpace(positionName)) return new List<string>();
            if (System.Enum.TryParse<TeachingPositionName>(positionName, out var en))
            {
                if (_axisMap.TryGetValue(en, out var arr)) return arr;
            }
            // 기본: 세 축 모두 허용
            return new[] { AxisNames.RightToolT, AxisNames.RightPickZ, AxisNames.RightPlaceZ };
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