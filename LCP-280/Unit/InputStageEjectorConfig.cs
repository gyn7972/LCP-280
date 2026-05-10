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
    /// InputStageEjectorConfig
    ///  - Ejector Z / Pin Z 두 축 Teaching Position 및 Offset 관리
    ///  - Predictive Control 옵션 (In-Position 조건 튜닝용)
    ///  - OutputStage / InputStageConfig 와 동일한 구조/주석 스타일로 통일
    ///  - 현재 별도 IO (Cylinder/Vacuum) 정의는 없음 → 필요 시 internal static class IO 에 상수 추가
    /// </summary>
    public class InputStageEjectorConfig : BaseConfig, IPropertyOrderProvider
    {
        public enum PickupSeqType
        {
            PickUp = 0,        // PIckUp만 활성
            EjectorPickUp_A = 1, // EjectorPickUp_A만 활성
            EjectorPickUp_B = 2, // EjectorPickUp_B만 활성
            //All = 3            // 둘 다 활성
        }

        /// <summary>
        /// Teaching Position 이름 (기존 이름 유지 - 호환성)
        /// </summary>
        public enum TeachingPositionName
        {
            EjectBlockUp,
            EjectBlockReady,
            EjectBlockSafety,
            EjectPinChange,
            EjectPinOffset,
            EjectPinReady,
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
        /// Ejector / Pin 축 Teaching Position 목록
        /// </summary>


        /// <summary>
        /// Position Offset (dzEjector, dzPin)
        ///  - 개별 TeachingPosition 이름별로 보정값 유지
        /// </summary>
        public Dictionary<string, (double dzEjector, double dzPin)> Offsets { get; set; } = new Dictionary<string, (double dzEjector, double dzPin)>();

        /// <summary>
        /// Position 별 사용할 축 매핑 (필요시 여기서 조정)
        ///  - 키: TeachingPositionName
        ///  - 값: 축 이름 배열 (AxisNames.*)
        /// </summary>
        [JsonIgnore]
        private static readonly Dictionary<TeachingPositionName, string[]> _axisMap = new Dictionary<TeachingPositionName, string[]>
        {
            // 기본: 2축 모두 사용. 필요 시 특정 포지션에서 한 축만 사용하도록 배열 수정.
            { TeachingPositionName.EjectBlockUp,      new [] { AxisNames.EjectorZ } },
            { TeachingPositionName.EjectBlockReady,   new [] { AxisNames.EjectorZ } },
            { TeachingPositionName.EjectBlockSafety,   new [] { AxisNames.EjectorZ } },
            { TeachingPositionName.EjectPinChange,    new [] { AxisNames.EjectPinZ } },
            { TeachingPositionName.EjectPinOffset,    new [] { AxisNames.EjectPinZ } },
            { TeachingPositionName.EjectPinReady,   new [] { AxisNames.EjectPinZ } },
        };

        #region Hard IO Tables
        [JsonIgnore]
        public HardInputDef[] HardInputs => _hardInputs;
        [JsonIgnore]
        private static readonly HardInputDef[] _hardInputs = Array.Empty<HardInputDef>();

        [JsonIgnore]
        public HardOutputDef[] HardOutputs => _hardOutputs;
        [JsonIgnore]
        private static readonly HardOutputDef[] _hardOutputs = Array.Empty<HardOutputDef>();

        [ConfigIgnore]
        [DefaultValue(0)]
        public int nPickupSeqType { get; set; } = 0;

        [JsonIgnore]
        [Category("SetupConfig"), DisplayName("SeqType")]
        [DefaultValue(PickupSeqType.PickUp)]
        public PickupSeqType SelectedPickupSeqType
        {
            get
            {
                if (Enum.IsDefined(typeof(PickupSeqType), nPickupSeqType) == false)
                    return PickupSeqType.PickUp;
                return (PickupSeqType)nPickupSeqType;
            }
            set => nPickupSeqType = (int)value;
        }

        [Category("PIckUp"), DisplayName("Up Offset (mm)")]
        [DefaultValue(0.0)]
        public double dPickUpOffset { get; set; } = 0.0;
        [Category("PIckUp"), DisplayName("Up Speed (mm/sec)")]
        [DefaultValue(0.0)]
        public double dPickUpSpeed { get; set; } = 0.0;
        [Category("PIckUp"), DisplayName("Up Acc (mm/sec2)")]
        [DefaultValue(0.0)]
        public double dPickUpAcc { get; set; } = 0.0;
        [Category("PIckUp"), DisplayName("Up Dec (mm/sec2)")]
        [DefaultValue(0.0)]
        public double dPickUpDec { get; set; } = 0.0;

        // Ejector PickUp (Z Down + Pin Z Up) 전용 파라미터
        [Category("EjectorPickUp"), DisplayName("EjectBlock Down Offset (mm)")]
        [DefaultValue(0.0)]
        public double dEjectBlockDownOffset { get; set; } = 0.0;

        [Category("EjectorPickUp"), DisplayName("EjectBlock Speed (mm/sec)")]
        [DefaultValue(0.0)]
        public double dEjectBlockSpeed { get; set; } = 0.0;

        [Category("EjectorPickUp"), DisplayName("EjectBlock Acc (mm/sec2)")]
        [DefaultValue(0.0)]
        public double dEjectBlockAcc { get; set; } = 0.0;

        [Category("EjectorPickUp"), DisplayName("EjectBlock Dec (mm/sec2)")]
        [DefaultValue(0.0)]
        public double dEjectBlockDec { get; set; } = 0.0;

        [Category("EjectorPickUp"), DisplayName("EjectPin Up Offset (mm)")]
        [DefaultValue(0.0)]
        public double dEjectPinUpOffset { get; set; } = 0.0;

        [Category("EjectorPickUp"), DisplayName("EjectPin Up Speed (mm/sec)")]
        [DefaultValue(0.0)]
        public double dEjectPinUpSpeed { get; set; } = 0.0;

        [Category("EjectorPickUp"), DisplayName("EjectPin Up Acc (mm/sec2)")]
        [DefaultValue(0.0)]
        public double dEjectPinUpAcc { get; set; } = 0.0;

        [Category("EjectorPickUp"), DisplayName("EjectPin Up Dec (mm/sec2)")]
        [DefaultValue(0.0)]
        public double dEjectPinUpDec { get; set; } = 0.0;

        #endregion

        public InputStageEjectorConfig() : base("InputStageEjectorConfig") { }

        /// <summary>
        /// enum 기반 기본 Teaching Position 초기화 + Offset 기본값 구성 (축 매핑 반영)
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

        /// <summary>
        /// TeachingPosition 저장 또는 갱신 (매핑에 따라 축 필터 유지)
        /// </summary>
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

        public new TeachingPosition GetTeachingPosition(string name)
        {
            if(TeachingPositions == null)
                return null;

            return TeachingPositions.FirstOrDefault(p => p.Name == name);
        }

        /// <summary>Offset 적용된 좌표 반환</summary>
        public (double z, double pinZ) GetPositionWithOffset(string name)
        {
            var tp = GetTeachingPosition(name);
            if (tp == null) return (0, 0);
            double z  = tp.AxisPositions.TryGetValue(AxisNames.EjectorZ, out var vz) ? vz : 0;
            double pz = tp.AxisPositions.TryGetValue(AxisNames.EjectPinZ, out var vpz) ? vpz : 0;
            if (Offsets.TryGetValue(name, out var off)) { z += off.dzEjector; pz += off.dzPin; }
            return (z, pz);
        }

        /// <summary>Offset 설정</summary>
        public void SetOffset(string name, double dzEjector, double dzPin)
        {
            Offsets[name] = (dzEjector, dzPin);
            Saveconfig();
        }

        /// <summary>Config 저장 (TeachingPositions 순수화)</summary>
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

        /// <summary>Config 로드 + Axis 바인딩 + Offset 키 보정 + 매핑 동기화</summary>
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

        /// <summary>
        /// 매핑에 따라 TeachingPositions 의 AxisPositions 내용 보정 (불필요 축 제거 / 누락 축 추가)
        /// </summary>
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

        /// <summary>
        /// Position 이름(문자열) 기준 축 이름 목록 반환
        /// </summary>
        public IReadOnlyList<string> GetAxisNamesForPosition(string positionName)
        {
            if (string.IsNullOrWhiteSpace(positionName)) return new List<string>();
            if (System.Enum.TryParse<TeachingPositionName>(positionName, out var en))
            {
                if (_axisMap.TryGetValue(en, out var arr)) return arr;
            }
            // 매핑 없는 경우: 현재 사용중인 축(백워드 호환) → 둘 다
            return new[] { AxisNames.EjectorZ, AxisNames.EjectPinZ };
        }

        #region IPropertyOrderProvider 구현 (Category / Property 표시 순서)
        // Category 순서: Common → Cassette
        public IDictionary<string, int> GetCategoryOrder()
            => new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
            {
                { "General", 0 },   // Name 속성 (Category 없음) 정렬 위치 지정
                { "Common", 1 },
                { "SetupConfig", 2 },
                { "PIckUp", 3 },
                { "EjectorPickUp", 4 },
            };

        // Property 순서: (DisplayName 또는 PropertyName)
        // BaseConfig: "Simulation" (IsSimulation)
        // Cassette: "SlotPitch (mm)", "SlotCount (ea)"
        public IEnumerable<string> GetPropertyOrder()
            => new[]
            {
                "Name",
                "Simulation",
                "SeqType",
                "Up Offset (mm)",
                "Up Speed (mm/sec)",
                "Up Acc (mm/sec2)",
                "Up Dec (mm/sec2)",

                "EjectBlock Down Offset (mm)",
                "EjectBlock Speed (mm/sec)",
                "EjectBlock Acc (mm/sec2)",
                "EjectBlock Dec (mm/sec2)",
                "EjectPin Up Offset (mm)",
                "EjectPin Up Speed (mm/sec)",
                "EjectPin Up Acc (mm/sec2)",
                "EjectPin Up Dec (mm/sec2)"
            };
        #endregion
    }
}