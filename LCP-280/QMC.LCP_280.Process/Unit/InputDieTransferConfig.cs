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
    /// InputDieTransferConfig
    ///  - Teaching Position + Offset 관리 (T / PickZ / PlaceZ)
    ///  - Arm Vacuum / Blow / Vent 및 Flow / Tank Pressure I/O 상수화
    ///  - Hard I/O 테이블과 저장/로드 로직 제공
    /// </summary>
    public class InputDieTransferConfig : BaseConfig
    {
        /// <summary>장치 IO 명칭 모음</summary>
        internal static class IO
        {
            // Inputs (X032~X037)
            public const string AIR_TANK_PRESSURE = "LEFT TOOL AIR TANK PRESSURE CHECK";      // X032
            public const string VAC_TANK_PRESSURE = "LEFT TOOL VACUUM TANK PRESSURE CHECK";   // X033
            public const string ARM1_FLOW = "LEFT TOOL ARM 1 FLOW CHECK";                     // X034
            public const string ARM2_FLOW = "LEFT TOOL ARM 2 FLOW CHECK";                     // X035
            public const string ARM3_FLOW = "LEFT TOOL ARM 3 FLOW CHECK";                     // X036
            public const string ARM4_FLOW = "LEFT TOOL ARM 4 FLOW CHECK";                     // X037

            // Outputs (Y039~Y050)
            public const string ARM1_VAC = "LEFT ARM 1 VACUUM"; // Y039
            public const string ARM2_VAC = "LEFT ARM 2 VACUUM"; // Y040
            public const string ARM3_VAC = "LEFT ARM 3 VACUUM"; // Y041
            public const string ARM4_VAC = "LEFT ARM 4 VACUUM"; // Y042
            public const string ARM1_BLOW = "LEFT ARM 1 BLOW";   // Y043
            public const string ARM2_BLOW = "LEFT ARM 2 BLOW";   // Y044
            public const string ARM3_BLOW = "LEFT ARM 3 BLOW";   // Y045
            public const string ARM4_BLOW = "LEFT ARM 4 BLOW";   // Y046
            public const string ARM1_VENT = "LEFT ARM 1 VENT";   // Y047
            public const string ARM2_VENT = "LEFT ARM 2 VENT";   // Y048
            public const string ARM3_VENT = "LEFT ARM 3 VENT";   // Y049
            public const string ARM4_VENT = "LEFT ARM 4 VENT";   // Y050
        }

        /// <summary>Teaching Position 이름 (기존 오타(Wating) 유지 - 호환 목적)</summary>
        public enum TeachingPositionName
        {
            Pickup,
            Place_Index1,
            Place_Index2,
            Place_Index3,
            Place_Index4,
            Place_Index5,
            Place_Index6,
            Place_Index7,
            Place_Index8,
            Place_Ready,
            SafeZone
            // 필요시 확장
        }

        /// <summary>Teaching Position 순수 목록</summary>
        public List<TeachingPosition> TeachingPositions { get; set; } = new List<TeachingPosition>();

        /// <summary>Offset: positionName -> (T, PickZ, PlaceZ)</summary>
        public Dictionary<string, (double t, double pickZ, double placeZ)> Offsets { get; set; } = new Dictionary<string, (double t, double pickZ, double placeZ)>();

        // Motion Done 보조 설정
        public bool EnablePredictiveControl { get; set; } = false;
        public double MoveDoneRemainDistance { get; set; } = 0.005;

        #region Hard IO Tables
        [JsonIgnore]
        public HardInputDef[] HardInputs => _hardInputs;
        private static readonly HardInputDef[] _hardInputs = new[]
        {
            new HardInputDef { No = 1, Name = IO.AIR_TANK_PRESSURE, Disp = "X032" },
            new HardInputDef { No = 2, Name = IO.VAC_TANK_PRESSURE, Disp = "X033" },
            new HardInputDef { No = 3, Name = IO.ARM1_FLOW,         Disp = "X034" },
            new HardInputDef { No = 4, Name = IO.ARM2_FLOW,         Disp = "X035" },
            new HardInputDef { No = 5, Name = IO.ARM3_FLOW,         Disp = "X036" },
            new HardInputDef { No = 6, Name = IO.ARM4_FLOW,         Disp = "X037" },
        };

        [JsonIgnore]
        public HardOutputDef[] HardOutputs => _hardOutputs;
        private static readonly HardOutputDef[] _hardOutputs = new[]
        {
            new HardOutputDef { No = 1,  Name = IO.ARM1_VAC,  Disp = "Y039" },
            new HardOutputDef { No = 2,  Name = IO.ARM2_VAC,  Disp = "Y040" },
            new HardOutputDef { No = 3,  Name = IO.ARM3_VAC,  Disp = "Y041" },
            new HardOutputDef { No = 4,  Name = IO.ARM4_VAC,  Disp = "Y042" },
            new HardOutputDef { No = 5,  Name = IO.ARM1_BLOW, Disp = "Y043" },
            new HardOutputDef { No = 6,  Name = IO.ARM2_BLOW, Disp = "Y044" },
            new HardOutputDef { No = 7,  Name = IO.ARM3_BLOW, Disp = "Y045" },
            new HardOutputDef { No = 8,  Name = IO.ARM4_BLOW, Disp = "Y046" },
            new HardOutputDef { No = 9,  Name = IO.ARM1_VENT, Disp = "Y047" },
            new HardOutputDef { No = 10, Name = IO.ARM2_VENT, Disp = "Y048" },
            new HardOutputDef { No = 11, Name = IO.ARM3_VENT, Disp = "Y049" },
            new HardOutputDef { No = 12, Name = IO.ARM4_VENT, Disp = "Y050" },
        };
        #endregion

        public InputDieTransferConfig() : base("InputDieTransferConfig") { }

        /// <summary>Teaching Position 기본 생성</summary>
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
                        { AxisNames.LeftToolT, 0.0 },
                        { AxisNames.LeftPickZ, 0.0 },
                        { AxisNames.LeftPlaceZ, 0.0 },
                    };
                    TeachingPositions.Add(new TeachingPosition(posName, axisPositions, $"Default {posName} Position"));
                }
                if (!Offsets.ContainsKey(posName)) Offsets[posName] = (0, 0, 0);
            }
            Saveconfig();
        }

        /// <summary>Teaching Position 추가/갱신</summary>
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

        /// <summary>Offset 적용된 목표 좌표</summary>
        public (double t, double pickZ, double placeZ) GetPositionWithOffset(string name)
        {
            var tp = GetTeachingPosition(name);
            if (tp == null) return (0, 0, 0);
            double t = tp.AxisPositions.TryGetValue("Left Tool T Axis", out var vt) ? vt : 0;
            double pz = tp.AxisPositions.TryGetValue("Left Pick Z Axis", out var vpz) ? vpz : 0;
            double plz = tp.AxisPositions.TryGetValue("Left Place Z Axis", out var vplz) ? vplz : 0;
            if (Offsets.TryGetValue(name, out var off)) { t += off.t; pz += off.pickZ; plz += off.placeZ; }
            return (t, pz, plz);
        }

        public void SetOffset(string name, double t, double pickZ, double placeZ)
        {
            Offsets[name] = (t, pickZ, placeZ);
            Saveconfig();
        }

        /// <summary>Config 저장 (TeachingPositions 순수화)</summary>
        public int Saveconfig()
        {
            var purePositions = TeachingPositions
                .Select(tp => new TeachingPosition(tp.Name, tp.AxisPositions, tp.Description) { ExtraInfo = tp.ExtraInfo })
                .ToList();
            var original = TeachingPositions; TeachingPositions = purePositions;
            try { return Save(); }
            finally { TeachingPositions = original; }
        }

        /// <summary>Config 로드 + TeachingPosition 축 바인딩 + Offset 키 보정</summary>
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