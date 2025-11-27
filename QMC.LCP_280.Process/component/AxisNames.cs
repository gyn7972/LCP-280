using System;

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
        public const string IndexPlaceZ = "Index Place Z Axis";            // 25
        public const string IndexT          = "Index T Axis";            // 26
        

        /// <summary>
        /// AxisNo 순서를 그대로 반영한 배열. 인덱스 = AxisNo.
        /// </summary>
        public static readonly string[] AllInOrder = new[]
        {
            EjectPinZ,      // 00
            LeftToolT,      // 01
            RightToolT,     // 02
            WaferStageX,    // 03
            WaferStageY,    // 04
            WaferStageT,    // 05
            LeftPickZ,      // 06
            LeftPlaceZ,     // 07
            IndexZ,         // 08
            AlignT,         // 09
            SphereZ,        // 10
            ProbeZ,         // 11
            ProbeCardX,     // 12
            ProbeCardY,     // 13
            ProbeCardZ,     // 14
            RightPickZ,     // 15
            RightPlaceZ,    // 16
            BinStageX,      // 17
            BinStageY,      // 18
            BinStageT,      // 19
            WaferLifterZ,   // 20
            WaferFeederY,   // 21
            EjectorZ,       // 22
            BinFeederY,     // 23
            BinLifterZ,     // 24
            IndexPlaceZ,     //25
            IndexT,          // 26
        };
    }
}
