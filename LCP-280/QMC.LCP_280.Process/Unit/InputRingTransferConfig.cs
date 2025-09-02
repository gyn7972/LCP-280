using Newtonsoft.Json;
using QMC.Common;
using QMC.Common.Motions;
using QMC.Common.Unit;
using QMC.LCP_280.Process.Component;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using static QMC.LCP_280.Process.Unit.IndexChipProbeController;

namespace QMC.LCP_280.Process.Unit
{
    public class InputRingTransferConfig : BaseConfig
    {
        public enum TeachingPositionName
        {
            Loading,
            Unloading,
            Ready,
            Home
            // 필요시 추가
        }
        public List<TeachingPosition> TeachingPositions { get; set; } = new List<TeachingPosition>();

        // IO 추가 필요시 여기에 정의
        [JsonIgnore]
        public HardInputDef[] HardInputs => _hardInputs;
        [JsonIgnore]
        private static readonly HardInputDef[] _hardInputs = new[]
        {
            new HardInputDef { No = 1, Name = "WAFER FEEDER UP",             Disp = "X020" },
            new HardInputDef { No = 2, Name = "WAFER FEEDER DOWN",           Disp = "X021" },
            new HardInputDef { No = 3, Name = "WAFER FEEDER UNCLAMP",        Disp = "X022" },
            new HardInputDef { No = 4, Name = "WAFER FEEDER RING CHECK",     Disp = "X023" },
            new HardInputDef { No = 5, Name = "WAFER FEEDER OVERLOAD CHECK", Disp = "X024" }
        };

        [JsonIgnore]
        public HardOutputDef[] HardOutputs => _hardOutputs;
        [JsonIgnore]
        private static readonly HardOutputDef[] _hardOutputs = new[]
        {
            new HardOutputDef { No = 1, Name = "WAFER FEEDER UP",      Disp = "Y016" },
            new HardOutputDef { No = 2, Name = "WAFER FEEDER DOWNE",   Disp = "Y017" },
            new HardOutputDef { No = 3, Name = "WAFER FEEDER CLAMP",   Disp = "Y018" },
            new HardOutputDef { No = 4, Name = "WAFER FEEDER UNCALMP", Disp = "Y019" }
        };

        public InputRingTransferConfig() : base("InputRingTransferConfig")
        {
            //InitializeDefaultTeachingPositions();
        }

        // enum 기반으로 신규 TeachingPosition 생성
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
                        { "Wafer Feeder X Axis", 100.0 }
                    };
                    tp = new TeachingPosition(posName, axisPositions, $"기본 {posName} 위치");
                    TeachingPositions.Add(tp);
                }
                // 축 바인딩은 여기서 하지 않음!
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
            // 축 정보 제외하고 TeachingPositions를 복제
            var purePositions = TeachingPositions
                .Select(tp => new TeachingPosition(tp.Name, tp.AxisPositions, tp.Description) { ExtraInfo = tp.ExtraInfo })
                .ToList();

            // 임시로 TeachingPositions를 교체해서 저장
            var backup = TeachingPositions;
            TeachingPositions = purePositions;
            int result = base.Save();
            TeachingPositions = backup;
            return result;
        }

        // 불러오기: 순수 데이터만 불러온 뒤, 런타임에 축 바인딩
        public int LoadAndBindAxes(MotionAxisManager axisManager)
        {
            int result = base.Load();
            foreach (var tp in TeachingPositions)
                tp.BindAxes(axisManager);
            return result;
        }


    }
}