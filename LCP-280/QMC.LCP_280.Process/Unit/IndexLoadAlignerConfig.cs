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
    /// IndexLoadAlignerConfig
    ///  - Align T / Index Z 축 Teaching Positions 관리
    ///  - (현재 별도 IO 없음: 필요 시 IO 클래스 확장)
    ///  - OutputStageConfig 스타일 구조 적용
    /// </summary>
    public class IndexLoadAlignerConfig : BaseConfig
    {
        internal static class IO { /* Add inputs/outputs later if needed */ }

        public enum TeachingPositionName
        {
            Align_Index1_Up,
            Align_Index1_Ready,
            Align_Index2_Up,
            Align_Index2_Ready,
            Align_Index3_Up,
            Align_Index3_Ready,
            Align_Index4_Up,
            Align_Index4_Ready,
            Align_Index5_Up,
            Align_Index5_Ready,
            Align_Index6_Up,
            Align_Index6_Ready,
            Align_Index7_Up,
            Align_Index7_Ready,
            Align_Index8_Up,
            Align_Index8_Read,
            // 필요시 확장
        }

        public List<TeachingPosition> TeachingPositions { get; set; } = new List<TeachingPosition>();

        public IndexLoadAlignerConfig() : base("IndexLoadAlignerConfig") { }

        public void InitializeDefaultTeachingPositions()
        {
            if (TeachingPositions == null) TeachingPositions = new List<TeachingPosition>();
            var existing = new HashSet<string>(TeachingPositions.Select(tp => tp.Name));
            foreach (TeachingPositionName name in System.Enum.GetValues(typeof(TeachingPositionName)))
            {
                string posName = name.ToString();
                if (!existing.Contains(posName))
                {
                    var axisPositions = new Dictionary<string, double>
                    {
                        { "Align T Axis", 100.0 },
                        { "Index Z Axis", 0.0 }
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
            int rc = Load(); if (rc != 0) return rc;
            foreach (var tp in TeachingPositions)
                tp.BindAxes(axisManager, "Unit");
            return 0;
        }
    }
}