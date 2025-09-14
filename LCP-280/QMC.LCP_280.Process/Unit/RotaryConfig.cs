using Newtonsoft.Json;
using QMC.Common;
using QMC.Common.Component;
using QMC.Common.Motions;
using QMC.Common.Unit;
using QMC.LCP_280.Process.Component;
using System.Collections.Generic;
using System.Linq;

namespace QMC.LCP_280.Process.Unit
{
    public class RotaryConfig : BaseConfig
    {
        internal static class IO
        {
            // Inputs (X040 ~ X049)
            public const string AIR_TANK_PRESSURE = "INDEX AIR TANK PRESSURE CHECK";      // X040
            public const string VAC_TANK_PRESSURE = "INDEX VACCUM TANK PRESSURE CHECK";    // X041 (table spelling)
            // (legacy mis-typed string kept for backward compat)
            public const string VAC_TANK_PRESSURE_LEGACY = "INDEX VACCUM TANK PRRESSURE CHECK"; // old code spelling
            public const string FLOW1 = "INDEX 1 FLOW CHECK"; // X042
            public const string FLOW2 = "INDEX 2 FLOW CHECK"; // X043
            public const string FLOW3 = "INDEX 3 FLOW CHECK"; // X044
            public const string FLOW4 = "INDEX 4 FLOW CHECK"; // X045
            public const string FLOW5 = "INDEX 5 FLOW CHECK"; // X046
            public const string FLOW6 = "INDEX 6 FLOW CHECK"; // X047
            public const string FLOW7 = "INDEX 7 FLOW CHECK"; // X048
            public const string FLOW8 = "INDEX 8 FLOW CHECK"; // X049

            // Outputs (Y051 ~ Y058 Vacuum, Y059 ~ Y066 Blow, Y067 ~ Y074 Vent)
            public const string VAC1 = "INDEX 1 VACUUM"; public const string VAC2 = "INDEX 2 VACUUM"; public const string VAC3 = "INDEX 3 VACUUM"; public const string VAC4 = "INDEX 4 VACUUM";
            public const string VAC5 = "INDEX 5 VACUUM"; public const string VAC6 = "INDEX 6 VACUUM"; public const string VAC7 = "INDEX 7 VACUUM"; public const string VAC8 = "INDEX 8 VACUUM";
            public const string BLOW1 = "INDEX 1 BLOW"; public const string BLOW2 = "INDEX 2 BLOW"; public const string BLOW3 = "INDEX 3 BLOW"; public const string BLOW4 = "INDEX 4 BLOW";
            public const string BLOW5 = "INDEX 5 BLOW"; public const string BLOW6 = "INDEX 6 BLOW"; public const string BLOW7 = "INDEX 7 BLOW"; public const string BLOW8 = "INDEX 8 BLOW";
            public const string VENT1 = "INDEX 1 VENT"; public const string VENT2 = "INDEX 2 VENT"; public const string VENT3 = "INDEX 3 VENT"; public const string VENT4 = "INDEX 4 VENT";
            public const string VENT5 = "INDEX 5 VENT"; public const string VENT6 = "INDEX 6 VENT"; public const string VENT7 = "INDEX 7 VENT"; public const string VENT8 = "INDEX 8 VENT";

            // Flow Inputs (순서: 1~8)
            public static readonly string[] FLOW = { FLOW1, FLOW2, FLOW3, FLOW4, FLOW5, FLOW6, FLOW7, FLOW8 };

            // Vacuum / Blow / Vent Outputs 배열
            public static readonly string[] SLOT_VAC  = { VAC1, VAC2, VAC3, VAC4, VAC5, VAC6, VAC7, VAC8 };
            public static readonly string[] SLOT_BLOW = { BLOW1, BLOW2, BLOW3, BLOW4, BLOW5, BLOW6, BLOW7, BLOW8 };
            public static readonly string[] SLOT_VENT = { VENT1, VENT2, VENT3, VENT4, VENT5, VENT6, VENT7, VENT8 };
        }

        public enum TeachingPositionName
        {
        }

        

        // Single axis rotation offset: positionName -> deltaT
        public Dictionary<string, double> Offsets { get; set; } = new Dictionary<string, double>();

        // Predictive control (same pattern as InputStage)
        public bool EnablePredictiveControl { get; set; } = false;
        public double MoveDoneRemainDistance { get; set; } = 0.005;

        [JsonIgnore]
        public HardInputDef[] HardInputs => _hardInputs;
        [JsonIgnore]
        private static readonly HardInputDef[] _hardInputs = new[]
        {
            new HardInputDef { No = 1, Name = IO.AIR_TANK_PRESSURE,          Disp = "X040" },
            new HardInputDef { No = 2, Name = IO.VAC_TANK_PRESSURE,          Disp = "X041" },
            // legacy duplicate (maps same address) for backward compatibility
            new HardInputDef { No = 3, Name = IO.FLOW1,                      Disp = "X042" },
            new HardInputDef { No = 4, Name = IO.FLOW2,                      Disp = "X043" },
            new HardInputDef { No = 5, Name = IO.FLOW3,                      Disp = "X044" },
            new HardInputDef { No = 6, Name = IO.FLOW4,                      Disp = "X045" },
            new HardInputDef { No = 7, Name = IO.FLOW5,                      Disp = "X046" },
            new HardInputDef { No = 8, Name = IO.FLOW6,                      Disp = "X047" },
            new HardInputDef { No = 9, Name = IO.FLOW7,                      Disp = "X048" },
            new HardInputDef { No = 10, Name = IO.FLOW8,                     Disp = "X049" },
        };

        [JsonIgnore]
        public HardOutputDef[] HardOutputs => _hardOutputs;
        [JsonIgnore]
        private static readonly HardOutputDef[] _hardOutputs = new[]
        {
            new HardOutputDef { No = 1,  Name = IO.VAC1,  Disp = "Y051" },
            new HardOutputDef { No = 2,  Name = IO.VAC2,  Disp = "Y052" },
            new HardOutputDef { No = 3,  Name = IO.VAC3,  Disp = "Y053" },
            new HardOutputDef { No = 4,  Name = IO.VAC4,  Disp = "Y054" },
            new HardOutputDef { No = 5,  Name = IO.VAC5,  Disp = "Y055" },
            new HardOutputDef { No = 6,  Name = IO.VAC6,  Disp = "Y056" },
            new HardOutputDef { No = 7,  Name = IO.VAC7,  Disp = "Y057" },
            new HardOutputDef { No = 8,  Name = IO.VAC8,  Disp = "Y058" },
            new HardOutputDef { No = 9,  Name = IO.BLOW1, Disp = "Y059" },
            new HardOutputDef { No = 10, Name = IO.BLOW2, Disp = "Y060" },
            new HardOutputDef { No = 11, Name = IO.BLOW3, Disp = "Y061" },
            new HardOutputDef { No = 12, Name = IO.BLOW4, Disp = "Y062" },
            new HardOutputDef { No = 13, Name = IO.BLOW5, Disp = "Y063" },
            new HardOutputDef { No = 14, Name = IO.BLOW6, Disp = "Y064" },
            new HardOutputDef { No = 15, Name = IO.BLOW7, Disp = "Y065" },
            new HardOutputDef { No = 16, Name = IO.BLOW8, Disp = "Y066" },
            new HardOutputDef { No = 17, Name = IO.VENT1, Disp = "Y067" },
            new HardOutputDef { No = 18, Name = IO.VENT2, Disp = "Y068" },
            new HardOutputDef { No = 19, Name = IO.VENT3, Disp = "Y069" },
            new HardOutputDef { No = 20, Name = IO.VENT4, Disp = "Y070" },
            new HardOutputDef { No = 21, Name = IO.VENT5, Disp = "Y071" },
            new HardOutputDef { No = 22, Name = IO.VENT6, Disp = "Y072" },
            new HardOutputDef { No = 23, Name = IO.VENT7, Disp = "Y073" },
            new HardOutputDef { No = 24, Name = IO.VENT8, Disp = "Y074" },
        };

        public RotaryConfig() : base("RotaryConfig") { }

        public void InitializeDefaultTeachingPositions()
        {
            if (TeachingPositions == null) TeachingPositions = new List<TeachingPosition>();
            foreach (TeachingPositionName name in System.Enum.GetValues(typeof(TeachingPositionName)))
            {
                string posName = name.ToString();
                if (TeachingPositions.FirstOrDefault(p => p.Name == posName) == null)
                {
                    var axisPositions = new Dictionary<string, double> { { AxisNames.IndexT, 0.0 } };
                    TeachingPositions.Add(new TeachingPosition(posName, axisPositions, $"Default {posName} Position"));
                }
                if (!Offsets.ContainsKey(posName)) Offsets[posName] = 0.0;
            }
            Saveconfig();
        }

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
            if (!Offsets.ContainsKey(tp.Name)) Offsets[tp.Name] = 0.0;
            Saveconfig();
        }

        public TeachingPosition GetTeachingPosition(string name) => TeachingPositions.FirstOrDefault(p => p.Name == name);

        public double GetPositionWithOffset(string name)
        {
            var tp = GetTeachingPosition(name); if (tp == null) return 0.0;
            double t = tp.AxisPositions.TryGetValue(AxisNames.IndexT, out var vt) ? vt : 0.0;
            if (Offsets.TryGetValue(name, out var off)) t += off; return t;
        }

        public void SetOffset(string name, double tDelta)
        {
            Offsets[name] = tDelta; Saveconfig();
        }

        public int Saveconfig()
        {
            var purePositions = TeachingPositions
                .Select(tp => new TeachingPosition(tp.Name, tp.AxisPositions, tp.Description) { ExtraInfo = tp.ExtraInfo })
                .ToList();
            var original = TeachingPositions; TeachingPositions = purePositions;
            try { return Save(); }
            finally { TeachingPositions = original; }
        }

        public int LoadAndBindAxes(MotionAxisManager axisManager)
        {
            int result = Load(); if (result != 0) return result;
            foreach (var tp in TeachingPositions) tp.BindAxes(axisManager, "Unit");
            foreach (var tp in TeachingPositions) if (!Offsets.ContainsKey(tp.Name)) Offsets[tp.Name] = 0.0;
            return 0;
        }
    }
}