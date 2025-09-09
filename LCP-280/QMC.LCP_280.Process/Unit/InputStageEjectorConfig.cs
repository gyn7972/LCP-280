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
    /// InputStageEjectorConfig
    ///  - Ejector Z / Pin Z 두 축 Teaching Position 및 Offset 관리
    ///  - Predictive Control 옵션 (In-Position 조건 튜닝용)
    ///  - OutputStage / InputStageConfig 와 동일한 구조/주석 스타일로 통일
    ///  - 현재 별도 IO (Cylinder/Vacuum) 정의는 없음 → 필요 시 internal static class IO 에 상수 추가
    /// </summary>
    public class InputStageEjectorConfig : BaseConfig
    {
        /// <summary>
        /// Teaching Position 이름 (기존 이름 유지 - 호환성)
        /// </summary>
        public enum TeachingPositionName
        {
            EjectBlockUp,
            EjectBlockWaiting,
            EjectBlockReady,
            EjectPinOffset,
            EjectPinWaiting,
            EjectPinChange,
            // 필요시 확장
        }

        /// <summary>
        /// Ejector / Pin 축 Teaching Position 목록
        /// </summary>
        public List<TeachingPosition> TeachingPositions { get; set; } = new List<TeachingPosition>();

        /// <summary>
        /// Position Offset (dzEjector, dzPin)
        ///  - 개별 TeachingPosition 이름별로 보정값 유지
        /// </summary>
        public Dictionary<string, (double dzEjector, double dzPin)> Offsets { get; set; } = new Dictionary<string, (double dzEjector, double dzPin)>();

        // Motion Done / In-Position 동작 관련 옵션
        public bool   EnablePredictiveControl { get; set; } = false;
        public double MoveDoneRemainDistance { get; set; } = 0.005;

        public InputStageEjectorConfig() : base("InputStageEjectorConfig") { }

        /// <summary>
        /// enum 기반 기본 Teaching Position 초기화 + Offset 기본값 구성
        /// </summary>
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
                        { "EJECTOR_Z",   0.0 },
                        { "EJECT_PIN_Z", 0.0 }
                    };
                    TeachingPositions.Add(new TeachingPosition(posName, axisPositions, $"Default {posName} Position"));
                }
                if (!Offsets.ContainsKey(posName)) Offsets[posName] = (0, 0);
            }
            Saveconfig();
        }

        /// <summary>Teaching Position 추가 / 갱신</summary>
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
            if (!Offsets.ContainsKey(tp.Name)) Offsets[tp.Name] = (0, 0);
            Saveconfig();
        }

        public TeachingPosition GetTeachingPosition(string name) => TeachingPositions.FirstOrDefault(p => p.Name == name);

        /// <summary>Offset 적용된 좌표 반환</summary>
        public (double z, double pinZ) GetPositionWithOffset(string name)
        {
            var tp = GetTeachingPosition(name);
            if (tp == null) return (0, 0);
            double z  = tp.AxisPositions.TryGetValue("EJECTOR_Z", out var vz) ? vz : 0;
            double pz = tp.AxisPositions.TryGetValue("EJECT_PIN_Z", out var vpz) ? vpz : 0;
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
            var original = TeachingPositions;
            TeachingPositions = pure;
            try { return Save(); }
            finally { TeachingPositions = original; }
        }

        /// <summary>Config 로드 + Axis 바인딩 + Offset 키 보정</summary>
        public int LoadAndBindAxes(MotionAxisManager axisManager)
        {
            int rc = Load();
            if (rc != 0) return rc;
            foreach (var tp in TeachingPositions)
                tp.BindAxes(axisManager, "Unit");
            foreach (var tp in TeachingPositions)
                if (!Offsets.ContainsKey(tp.Name)) Offsets[tp.Name] = (0, 0);
            return 0;
        }
    }
}