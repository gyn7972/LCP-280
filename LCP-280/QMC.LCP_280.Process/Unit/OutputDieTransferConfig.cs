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
    public class OutputDieTransferConfig : BaseConfig
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
            Place_Ready,
            SafeZone
            // 필요시 확장
        }

        public List<TeachingPosition> TeachingPositions { get; set; } = new List<TeachingPosition>();

        // ================= IO 이름 상수 (InputStageConfig.IO 패턴과 동일) =================
        internal static class IO
        {
            // Inputs
            public const string AIR_TANK_PRESS = "RIGHT TOOL AIR TANK PRESSURE CHECK";
            public const string VAC_TANK_PRESS = "RIGHT TOOL VACUUM TANK PRESSURE CHECK";
            public const string ARM1_FLOW_CHECK = "RIGHT TOOL ARM 1 FLOW CHECK";
            public const string ARM2_FLOW_CHECK = "RIGHT TOOL ARM 2 FLOW CHECK";
            public const string ARM3_FLOW_CHECK = "RIGHT TOOL ARM 3 FLOW CHECK";
            public const string ARM4_FLOW_CHECK = "RIGHT TOOL ARM 4 FLOW CHECK";

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
            public static readonly string[] ARM_FLOW = { ARM1_FLOW_CHECK, ARM2_FLOW_CHECK, ARM3_FLOW_CHECK, ARM4_FLOW_CHECK };
            public static readonly string[] ARM_VAC = { ARM1_VAC, ARM2_VAC, ARM3_VAC, ARM4_VAC };
            public static readonly string[] ARM_BLOW = { ARM1_BLOW, ARM2_BLOW, ARM3_BLOW, ARM4_BLOW };
            public static readonly string[] ARM_VENT = { ARM1_VENT, ARM2_VENT, ARM3_VENT, ARM4_VENT };
        }

        // ================= Hard IO 정의 (InputStageConfig 패턴과 유사) =================
        [JsonIgnore]
        public HardInputDef[] HardInputs => _hardInputs;
        [JsonIgnore]
        private static readonly HardInputDef[] _hardInputs = new[]
        {
            new HardInputDef { No = 1, Name = IO.AIR_TANK_PRESS,  Disp = "X051" },
            new HardInputDef { No = 2, Name = IO.VAC_TANK_PRESS,  Disp = "X052" },
            new HardInputDef { No = 3, Name = IO.ARM1_FLOW_CHECK, Disp = "X053" },
            new HardInputDef { No = 4, Name = IO.ARM2_FLOW_CHECK, Disp = "X054" },
            new HardInputDef { No = 5, Name = IO.ARM3_FLOW_CHECK, Disp = "X055" },
            new HardInputDef { No = 6, Name = IO.ARM4_FLOW_CHECK, Disp = "X056" }
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

        public OutputDieTransferConfig() : base("OutputDieTransferConfig")
        {
            // 필요 시 TeachingPosition 초기화 호출
            // InitializeDefaultTeachingPositions();
        }

        public void InitializeDefaultTeachingPositions()
        {
            if (TeachingPositions == null) TeachingPositions = new List<TeachingPosition>();
            var existingNames = new HashSet<string>(TeachingPositions.Select(tp => tp.Name));
            foreach (TeachingPositionName name in System.Enum.GetValues(typeof(TeachingPositionName)))
            {
                string posName = name.ToString();
                var tp = TeachingPositions.FirstOrDefault(p => p.Name == posName);
                if (tp == null)
                {
                    var axisPositions = new Dictionary<string, double>
                    {
                        { "Right Tool T Axis", 0.0 },
                        { "Right Pick Z Axis", 100.0 },
                        { "Right Place Z Axis", 100.0 }
                    };
                    tp = new TeachingPosition(posName, axisPositions, $"기본 {posName} 위치");
                    TeachingPositions.Add(tp);
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
            else
            {
                TeachingPositions.Add(tp);
            }
            Saveconfig();
        }

        public TeachingPosition GetTeachingPosition(string name)
            => TeachingPositions.FirstOrDefault(p => p.Name == name);

        public int Saveconfig()
        {
            var purePositions = TeachingPositions
                .Select(tp => new TeachingPosition(tp.Name, tp.AxisPositions, tp.Description) { ExtraInfo = tp.ExtraInfo })
                .ToList();

            var original = TeachingPositions;
            TeachingPositions = purePositions;
            try
            {
                return Save();
            }
            finally
            {
                TeachingPositions = original;
            }
        }

        public int LoadAndBindAxes(MotionAxisManager axisManager)
        {
            int result = Load();
            if (result != 0) return result;

            foreach (var tp in TeachingPositions)
                tp.BindAxes(axisManager, "Unit");

            return 0;
        }
    }
}