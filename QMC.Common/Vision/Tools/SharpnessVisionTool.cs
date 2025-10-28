/*
 * Purpose
 *      특정 위치의 Sharpness Score를 찾는 Vision Tool을 정의한다.
 * 
 * Revision
 *      1. Created: 2018.01.09 by JUNG.CY
 *      
 */

using System;
using System.ComponentModel;

namespace QMC.Common.Vision.Tools
{
    #region SharpnessVisionTool
    [Serializable]
    public abstract class SharpnessVisionTool : VisionTool
    {
        #region Constructor
        public SharpnessVisionTool(string name) : base(name)
        {
            this.Result = new SharpnessResult(this.Name);
        }
        public SharpnessVisionTool() : this("") { }
        #endregion

        #region VisionTool Members
        public new SharpnessVisionToolParameter Parameter
        {
            get { return base.Parameter as SharpnessVisionToolParameter; }
            set { base.Parameter = value; }
        }

        [Browsable(false)]
        public new SharpnessResult Result
        {
            get { return base.Result as SharpnessResult; }
            protected set { base.Result = value; }
        }
        #endregion
    }
    #endregion

    #region SharpnessVisionToolParameter
    [Serializable]
    public class SharpnessVisionToolParameter : VisionToolParameter
    {
        #region Define
        [Serializable]
        public enum BestFocusingValueTypes
        {
            Max,
            Min,
        }
        #endregion

        #region Field
        private BestFocusingValueTypes m_BestFocusingValueType;
        #endregion

        #region Constructor
        public SharpnessVisionToolParameter() : base()
        {
            this.BestFocusingValueType = BestFocusingValueTypes.Max;
        }
        #endregion

        #region Property
        public BestFocusingValueTypes BestFocusingValueType
        {
            get { return this.m_BestFocusingValueType; }
            set { this.m_BestFocusingValueType = value; }
        }
        #endregion
    }
    #endregion

    #region SharpnessResult
    [Serializable]
    public class SharpnessResult : VisionResult
    {
        #region Fleid
        private double m_SharpnessScore;

        private VisionImage m_OutputImage;
        
        #endregion

        #region Constructor
        public SharpnessResult(string owner) : base(owner)
        {
            this.SharpnessScore = 0.0;
            this.OutputImage = new VisionImage();
        }
        #endregion

        #region Property
        /// <summary>
        /// Vision Processing의 Sharpness값을 가져오거나 설정한다.
        /// </summary>
        public double SharpnessScore
        {
            get { return this.m_SharpnessScore; }
            set { this.m_SharpnessScore = value; }
        }
        public VisionImage OutputImage
        {
            get { return this.m_OutputImage; }
            set { this.m_OutputImage = value; }
        }

        #endregion
    }
    #endregion
}