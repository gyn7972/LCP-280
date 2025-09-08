using Newtonsoft.Json;
using QMC.Common;
using QMC.Common.Motions;
using QMC.Common.Unit;
using QMC.LCP_280.Process.Component;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace QMC.LCP_280.Process.Unit
{
    public class OutputStageConfig : BaseConfig
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
            Ready
            // 필요시 추가
        }
        public List<TeachingPosition> TeachingPositions { get; set; } = new List<TeachingPosition>();

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

        public OutputStageConfig() : base("OutputStageConfig") { }

        public void InitializeDefaultTeachingPositions()
        {
            if (TeachingPositions == null) TeachingPositions = new List<TeachingPosition>();
            var existingNames = new HashSet<string>(TeachingPositions.Select(tp => tp.Name));
            foreach (TeachingPositionName name in System.Enum.GetValues(typeof(TeachingPositionName)))
            {
                string posName = name.ToString();
                if (!existingNames.Contains(posName))
                {
                    var axisPositions = new Dictionary<string, double>
                    {
                        { "Bin Stage X Axis", 0.0 },
                        { "Bin Stage Y Axis", 100.0 },
                        { "Bin Stage T Axis", 200.0 }
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
                exist.Description = tp.Description;
                exist.ExtraInfo = tp.ExtraInfo;
            }
            else TeachingPositions.Add(tp);
            Saveconfig();
        }

        public TeachingPosition GetTeachingPosition(string name) => TeachingPositions.FirstOrDefault(p => p.Name == name);

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
            return 0;
        }
    }
}