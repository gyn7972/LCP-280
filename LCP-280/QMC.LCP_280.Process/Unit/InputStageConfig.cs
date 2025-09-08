using Newtonsoft.Json;
using QMC.Common;
using QMC.Common.Motions;
using QMC.Common.Unit;
using QMC.LCP_280.Process.Component;
using System.Collections.Generic;
using System.Linq;

namespace QMC.LCP_280.Process.Unit
{
    public class InputStageConfig : BaseConfig
    {
        internal static class IO // centralized IO names (shared with InputStage)
        {
            public const string RING_CHECK0 = "WAFER STAGE RING CHECK 0";
            public const string RING_CHECK1 = "WAFER STAGE RING CHECK 1";
            public const string CLAMP_DOWN_SNS = "WAFER STAGE CLAMP DOWN";
            public const string CLAMP_SNS = "WAFER STAGE CLAMP";
            public const string EXPANDER_UP_SNS = "WAFER STAGE EXPANDER UP";
            public const string EXPANDER_DOWN_SNS = "WAFER STAGE EXPANDER DOWN";
            public const string VAC_OK_SNS = "EJECTOR VACUUM CHECK";
            public const string CLAMP_UP_OUT = "WAFER STAGE CLAMP UP";
            public const string CLAMP_DOWN_OUT = "WAFER STAGE CLAMP DOWN";
            public const string CLAMP_OUT = "WAFER STAGE CLAMP";
            public const string UNCLAMP_OUT = "WAFER STAGE UNCLAMP";
            public const string EXPANDER_UP_OUT = "WAFER STAGE EXPANDER UP";
            public const string EXPANDER_DOWN_OUT = "WAFER STAGE EXPANDER DOWN";
            public const string VAC_OUT = "EJECTOR VACUUM";
        }

        public enum TeachingPositionName
        {
            Loading,
            Unloading,
            Ready,
            Home,
            Align,
            ScanStart,
            PickUpStart
        }
        public List<TeachingPosition> TeachingPositions { get; set; } = new List<TeachingPosition>();

        // 확장: 오프셋(보정) 테이블 (기본 Position + Offset 분리 운용 대비)
        public Dictionary<string, (double dx, double dy, double dt)> Offsets { get; set; } = new Dictionary<string, (double dx, double dy, double dt)>();

        // 예측/조기 Move Done 사용 여부 (XytStageConfig 패턴 차용)
        public bool EnablePredictiveControl { get; set; } = false;
        public double MoveDoneRemainDistance { get; set; } = 0.005; // mm 허용잔량

        // IO 정의
        [JsonIgnore]
        public HardInputDef[] HardInputs => _hardInputs;
        [JsonIgnore]
        private static readonly HardInputDef[] _hardInputs = new[]
        {
            new HardInputDef { No = 1, Name = IO.RING_CHECK0,  Disp = "X025" },
            new HardInputDef { No = 2, Name = IO.RING_CHECK1,  Disp = "X026" },
            new HardInputDef { No = 3, Name = IO.CLAMP_DOWN_SNS,    Disp = "X027" },
            new HardInputDef { No = 4, Name = IO.CLAMP_SNS,         Disp = "X028" },
            new HardInputDef { No = 5, Name = IO.EXPANDER_UP_SNS,   Disp = "X029" },
            new HardInputDef { No = 6, Name = IO.EXPANDER_DOWN_SNS, Disp = "X030" },
            new HardInputDef { No = 7, Name = IO.VAC_OK_SNS,      Disp = "X031" },
        };

        [JsonIgnore]
        public HardOutputDef[] HardOutputs => _hardOutputs;
        [JsonIgnore]
        private static readonly HardOutputDef[] _hardOutputs = new[]
        {
            new HardOutputDef { No = 1, Name = IO.CLAMP_UP_OUT,      Disp = "Y020" },
            new HardOutputDef { No = 2, Name = IO.CLAMP_DOWN_OUT,    Disp = "Y021" },
            new HardOutputDef { No = 3, Name = IO.CLAMP_OUT,         Disp = "Y022" },
            new HardOutputDef { No = 4, Name = IO.UNCLAMP_OUT,       Disp = "Y023" },
            new HardOutputDef { No = 5, Name = IO.EXPANDER_UP_OUT,   Disp = "Y024" },
            new HardOutputDef { No = 6, Name = IO.EXPANDER_DOWN_OUT, Disp = "Y025" },
            new HardOutputDef { No = 7, Name = IO.VAC_OUT,            Disp = "Y038" },
        };

        public InputStageConfig() : base("InputStageConfig") { }

        public void InitializeDefaultTeachingPositions()
        {
            if (TeachingPositions == null) TeachingPositions = new List<TeachingPosition>();
            foreach (TeachingPositionName name in System.Enum.GetValues(typeof(TeachingPositionName)))
            {
                string posName = name.ToString();
                if (TeachingPositions.FirstOrDefault(p => p.Name == posName) == null)
                {
                    var axisPositions = new Dictionary<string, double>
                    {
                        { "Wafer Stage X Axis", 0.0 },
                        { "Wafer Stage Y Axis", 0.0 },
                        { "Wafer Stage T Axis", 0.0 }
                    };
                    TeachingPositions.Add(new TeachingPosition(posName, axisPositions, $"Default {posName} Position"));
                }
                if (!Offsets.ContainsKey(posName)) Offsets[posName] = (0, 0, 0);
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
            if (!Offsets.ContainsKey(tp.Name)) Offsets[tp.Name] = (0, 0, 0);
            Saveconfig();
        }

        public TeachingPosition GetTeachingPosition(string name) => TeachingPositions.FirstOrDefault(p => p.Name == name);

        // 오프셋 적용 좌표 반환
        public (double x, double y, double t) GetPositionWithOffset(string name)
        {
            var tp = GetTeachingPosition(name);
            if (tp == null) return (0, 0, 0);
            double x = tp.AxisPositions.TryGetValue("Wafer Stage X Axis", out var vx) ? vx : 0;
            double y = tp.AxisPositions.TryGetValue("Wafer Stage Y Axis", out var vy) ? vy : 0;
            double t = tp.AxisPositions.TryGetValue("Wafer Stage T Axis", out var vt) ? vt : 0;
            if (Offsets.TryGetValue(name, out var off))
            {
                x += off.dx; y += off.dy; t += off.dt;
            }
            return (x, y, t);
        }

        public void SetOffset(string name, double dx, double dy, double dt)
        {
            Offsets[name] = (dx, dy, dt);
            Saveconfig();
        }

        public int Saveconfig()
        {
            // 순수 포지션 리스트 복제 저장 (Offsets 및 확장 필드는 그대로 JSON 포함)
            var purePositions = TeachingPositions
                .Select(tp => new TeachingPosition(tp.Name, tp.AxisPositions, tp.Description) { ExtraInfo = tp.ExtraInfo })
                .ToList();
            var original = TeachingPositions;
            TeachingPositions = purePositions;
            try { return Save(); }
            finally { TeachingPositions = original; }
        }

        public int LoadAndBindAxes(MotionAxisManager axisManager)
        {
            int result = Load();
            if (result != 0) return result;
            foreach (var tp in TeachingPositions)
                tp.BindAxes(axisManager, "Unit");
            // 누락 Offset 보강
            foreach (var tp in TeachingPositions)
                if (!Offsets.ContainsKey(tp.Name)) Offsets[tp.Name] = (0, 0, 0);
            return 0;
        }
    }
}