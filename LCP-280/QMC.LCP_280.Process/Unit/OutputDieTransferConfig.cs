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
            Pickup,
            PickupWating,
            Place,
            PlaceWating,
            Ready
            // 필요시 추가
        }
        public List<TeachingPosition> TeachingPositions { get; set; } = new List<TeachingPosition>();

        // IO 추가 필요시 여기에 정의
        [JsonIgnore]
        public HardInputDef[] HardInputs => _hardInputs;
        [JsonIgnore]
        private static readonly HardInputDef[] _hardInputs = new[]
        {
            new HardInputDef { No = 1, Name = "RIGHT TOOL AIR TANK PRESSURE CHECK",    Disp = "X051" },
            new HardInputDef { No = 2, Name = "RIGHT TOOL VACUUM TANK PRESSURE CHECK", Disp = "X052" },
            new HardInputDef { No = 3, Name = "RIGHT TOOL ARM 1 FLOW CHECK",           Disp = "X053" },
            new HardInputDef { No = 4, Name = "RIGHT TOOL ARM 2 FLOW CHECK",           Disp = "X054" },
            new HardInputDef { No = 5, Name = "RIGHT TOOL ARM 3 FLOW CHECK",           Disp = "X055" },
            new HardInputDef { No = 6, Name = "RIGHT TOOL ARM 4 FLOW CHECK",           Disp = "X056" }
        };

        [JsonIgnore]
        public HardOutputDef[] HardOutputs => _hardOutputs;
        [JsonIgnore]
        private static readonly HardOutputDef[] _hardOutputs = new[]
        {
            new HardOutputDef { No = 1,  Name = "RIGHT ARM 1 VACUUM", Disp = "Y076" },
            new HardOutputDef { No = 2,  Name = "RIGHT ARM 2 VACUUM", Disp = "Y077" },
            new HardOutputDef { No = 3,  Name = "RIGHT ARM 3 VACUUM", Disp = "Y078" },
            new HardOutputDef { No = 4,  Name = "RIGHT ARM 4 VACUUM", Disp = "Y079" },
            new HardOutputDef { No = 5,  Name = "RIGHT ARM 1 BLOW",   Disp = "Y080" },
            new HardOutputDef { No = 6,  Name = "RIGHT ARM 2 BLOW",   Disp = "Y081" },
            new HardOutputDef { No = 7,  Name = "RIGHT ARM 3 BLOW",   Disp = "Y082" },
            new HardOutputDef { No = 8,  Name = "RIGHT ARM 4 BLOW",   Disp = "Y083" },
            new HardOutputDef { No = 9,  Name = "RIGHT ARM 1 VENT",   Disp = "Y084" },
            new HardOutputDef { No = 10, Name = "RIGHT ARM 2 VENT",   Disp = "Y085" },
            new HardOutputDef { No = 11, Name = "RIGHT ARM 3 VENT",   Disp = "Y086" },
            new HardOutputDef { No = 12, Name = "RIGHT ARM 4 VENT",   Disp = "Y087" }
        };

        public OutputDieTransferConfig() : base("OutputDieTransferConfig")
        {
            //InitializeDefaultTeachingPositions();
        }

        // enum 기반으로 기본 TeachingPosition 생성
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
                // 축 바인딩은 여기서 하지 말고!
            }
            Saveconfig();
        }

        // 포지션 추가/업데이트
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

        // 포지션 조회
        public TeachingPosition GetTeachingPosition(string name)
            => TeachingPositions.FirstOrDefault(p => p.Name == name);

        // 저장: 축 정보(Axes) 제외하고 순수 데이터만 저장
        public int Saveconfig()
        {
            // 축 정보 제외하고 TeachingPositions만 저장
            var purePositions = TeachingPositions
                .Select(tp => new TeachingPosition(tp.Name, tp.AxisPositions, tp.Description) { ExtraInfo = tp.ExtraInfo })
                .ToList();

            // 임시로 TeachingPositions를 교체해서 저장
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

        // 불러오기: 저장 데이터를 불러온 뒤, 런타임에 축 바인딩
        public int LoadAndBindAxes(MotionAxisManager axisManager)
        {
            int result = Load();
            if (result != 0) return result;

            // 각 TeachingPosition에 축 바인딩
            foreach (var tp in TeachingPositions)
            {
                tp.BindAxes(axisManager, "Unit"); // unitName = "Unit" (혹은 필요에 맞게)
            }

            return 0;
        }
    }
}