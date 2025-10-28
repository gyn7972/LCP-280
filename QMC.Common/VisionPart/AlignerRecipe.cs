using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Linq;

namespace QMC.Common.VisionPart
{
    [Serializable]
    public enum ParamAlignerRecipeKey
    {
        UseTwoPointAlign,
        UseFourPointAlign,
        UseFourPointAlignNotTheta,
        FiducialXDistance,
        FiducialYDistance,
        AngleTolerance,
        RetryCount,       
        MaxTolerance,
        MinTolerance,
        VerificationEnable,
        EnableThetaCorrection,
        UseSaveThetaCorrectionData,
        ThetaCorrectionAngleTolerance,
        ThetaCorrectionUnitAngle,
        PatternMatchingParameter,

    }

    [Serializable]
    public class AlignerRecipe : BaseRecipe
    {
        [Serializable]
        public enum Direction
        {
            /// <summary>
            /// 반시계 방향.
            /// </summary>
            CounterClockWise,

            /// <summary>
            /// 시계 방향.
            /// </summary>
            ClockWise,
        }
        [Serializable]
        public enum PathType
        {
            Continuous,
            StepByStep,
        }
        [Serializable]
        public enum PositionAligns
        {
            Reference,
            First,
            Second,
            Third,
            Forth
        }

        public Point InspectRoiStartLocation { set; get; }
        public Point InspectRoiEndLocation { set; get; }
        public Point TrainRoiStartLocation { set; get; }
        public Point TrainRoiEndLocation { set; get; }
        
    }
}
