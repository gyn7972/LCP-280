using System;
using System.Collections.Generic;

namespace QMC.LCP_280.Process
{
    /// <summary>
    /// 설비 전체에서 사용하는 축 이름 상수 모음.
    /// AxisNo (보드상의 순번)과 UI/설정 파일 이름을 한 곳에서 관리.
    /// </summary>
    public static class AxisNames
    {
        public const string EjectPinZ       = "Eject Pin Z Axis";        // 00
        public const string LeftToolT       = "Left Tool T Axis";        // 01
        public const string RightToolT      = "Right Tool T Axis";       // 02
        public const string WaferStageX     = "Wafer Stage X Axis";      // 03
        public const string WaferStageY     = "Wafer Stage Y Axis";      // 04
        public const string WaferStageT     = "Wafer Stage T Axis";      // 05
        public const string LeftPickZ       = "Left Pick Z Axis";        // 06
        public const string LeftPlaceZ      = "Left Place Z Axis";       // 07
        public const string IndexZ          = "Index Z Axis";            // 08
        public const string AlignT          = "Align T Axis";            // 09
        public const string SphereZ         = "Sphere Z Axis";           // 10
        public const string ProbeZ          = "Probe Z Axis";            // 11
        public const string ProbeCardX      = "Probe Card X Axis";       // 12
        public const string ProbeCardY      = "Probe Card Y Axis";       // 13
        public const string ProbeCardZ      = "Probe Card Z Axis";       // 14
        public const string RightPickZ      = "Right Pick Z Axis";       // 15
        public const string RightPlaceZ     = "Right Place Z Axis";      // 16
        public const string BinStageX       = "Bin Stage X Axis";        // 17
        public const string BinStageY       = "Bin Stage Y Axis";        // 18
        public const string BinStageT       = "Bin Stage T Axis";        // 19
        public const string WaferLifterZ    = "Wafer Lifter Z Axis";     // 20
        public const string WaferFeederY    = "Wafer Feeder Y Axis";     // 21
        public const string EjectorZ        = "Ejector Z Axis";          // 22
        public const string BinFeederY      = "Bin Feeder Y Axis";       // 23
        public const string BinLifterZ      = "Bin Lifter Z Axis";       // 24
        public const string IndexPlaceZ     = "Index Place Z Axis";            // 25
        public const string GripperX        = "Gripper X Axis";            // 26
        public const string IndexT          = "Index T Axis";            // 27 //25
        

        /// <summary>
        /// 장비에 맞춰 임의로 지정한 배열. 
        /// </summary>
        public static readonly string[] AllInOrder = new[]
        {
            WaferLifterZ,
            WaferFeederY,
            WaferStageX,
            WaferStageY,
            WaferStageT,
            EjectorZ,
            EjectPinZ,
            LeftPickZ,
            LeftPlaceZ,
            LeftToolT,
            IndexT,
            IndexPlaceZ,
            IndexZ,
            AlignT,
            SphereZ,
            ProbeZ,
            ProbeCardX,
            ProbeCardY,
            ProbeCardZ,
            GripperX,
            RightPickZ,
            RightPlaceZ,
            RightToolT,
            BinStageX,
            BinStageY,
            BinStageT,
            BinFeederY,
            BinLifterZ
        };

        /// <summary>
        /// UI에 보여줄 축 표시명(디스플레이명) 매핑.
        /// Key = 실제 축 이름(MotionAxis.Name), Value = 표시명.
        /// </summary>
        public static readonly IReadOnlyDictionary<string, string> DisplayNames
            = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                // TODO: 여기 Value를 원하는 표기(한글/약어/공정명 등)로 변경
                { WaferLifterZ, "WaferLifterZ" },
                { WaferFeederY, "WaferFeederY" },
                { WaferStageX,  "WaferStageX" },
                { WaferStageY,  "WaferStageY" },
                { WaferStageT,  "WaferStageT" },
                { EjectorZ,     "WaferNeedleZ" },
                { EjectPinZ,    "WaferNeedlePinZ" },
                { LeftPickZ,    "WaferArmPickZ" },
                { LeftPlaceZ,   "WaferArmPlaceZ" },
                { LeftToolT,    "WaferArmT" },
                { IndexT,       "IndexT" },
                { IndexPlaceZ,  "IndexPlaceZ" },
                { IndexZ,       "IndexLoadAlignZ" },
                { AlignT,       "IndexLoadAlignT" },
                { SphereZ,      "IndexSphereZ" },
                { ProbeZ,       "IndexProbeZ" },
                { ProbeCardX,   "IndexProbeCardX" },
                { ProbeCardY,   "IndexProbeCardY" },
                { ProbeCardZ,   "IndexProbeCardZ" },
                { GripperX,     "IndexGripperX" },
                { RightPickZ,   "BinArmPickZ" },
                { RightPlaceZ,  "BinArmPlaceZ" },
                { RightToolT,   "BinArmT" },
                { BinStageX,    "BinStageX" },
                { BinStageY,    "BinStageY" },
                { BinStageT,    "BinStageT" },
                { BinFeederY,   "BinFeederY" },
                { BinLifterZ,   "BinLifterZ" },
            };

        public static string GetDisplayName(string axisName)
        {
            if (string.IsNullOrWhiteSpace(axisName)) return axisName;
            string display;
            return DisplayNames.TryGetValue(axisName, out display) ? display : axisName;
        }

        /// <summary>
        /// "Left Tool T Axis" -> "LeftToolT" 형태로 변환하여 반환합니다.
        /// (알람 Source 매핑이나 Enum Prefix 검색 용도로 사용)
        /// </summary>
        public static string GetPrefixName(string axisName)
        {
            if (string.IsNullOrWhiteSpace(axisName)) return axisName;

            // 1. 끝에 붙은 " Axis" 또는 " Axis"를 무시하기 위해 제거
            string result = axisName.Replace(" Axis", "").Replace(" axis", "");

            // 2. 남은 공백들 모두 제거
            result = result.Replace(" ", "");

            return result;
        }
    }
}
