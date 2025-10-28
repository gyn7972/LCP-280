/*
 * Purpose
 *      Line(들)을 찾는 Vision Tool을 정의한다.
 * 
 * Revision
 *      1. Created: 2019.04.05 by JUNG.CY
 *      
 */

using System;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace QMC.Common.Vision.Tools
{
    #region LineSearchingVisionTool
    [Serializable]
    public abstract class LineSearchingVisionTool : VisionTool
    {
        #region Define
        public enum PolarityConstants
        {
            DarkToLight,
            LightToDark,
            Either,
            Mixed,
        }
        #endregion

        #region Constructor
        public LineSearchingVisionTool(string name) : base(name)
        {
            this.Result = new LineSearchingResult(this.Name);
        }
        public LineSearchingVisionTool() : this("") { }
        #endregion

        #region VisionTool Members
        public new LineSearchingVisionToolParameter Parameter
        {
            get { return base.Parameter as LineSearchingVisionToolParameter; }
            set { base.Parameter = value; }
        }

        [Browsable(false)]
        public new LineSearchingResult Result
        {
            get { return base.Result as LineSearchingResult; }
            protected set { base.Result = value; }
        }
        #endregion
    }
    #endregion

    #region LineSearchingVisionToolParameter
    [Serializable]
    public class LineSearchingVisionToolParameter : VisionToolParameter
    {
        #region Field
        private bool m_ResultOverlayVisible;
        private double m_ExpectedLineAngle;
        private RangeD m_AngleTolerance;
        private LineSearchingVisionTool.PolarityConstants m_PolarityConstant;
        private int m_LineCount;
        private double m_Angle;
        #endregion

        #region Constructor
        public LineSearchingVisionToolParameter() : base()
        {
            this.ResultOverlayVisible = false;
            this.ExpectedLineAngle = 0.0;
            this.AngleTolerance = new RangeD(-15, 15);
            this.PolarityConstant = LineSearchingVisionTool.PolarityConstants.DarkToLight;
        }
        #endregion

        #region Property
        public bool ResultOverlayVisible
        {
            get { return this.m_ResultOverlayVisible; }
            set { this.m_ResultOverlayVisible = value; }
        }

        /// <summary>
        /// 찾고 싶은 Line의 각도를 가져오거나 설정한다.
        /// 예상 라인 각도.
        /// </summary>
        public double ExpectedLineAngle
        {
            get { return this.m_ExpectedLineAngle; }
            set
            {
                if (this.m_ExpectedLineAngle == value) return;
                this.m_ExpectedLineAngle = value;
                this.HasChanged = true;
            }
        }

        public RangeD AngleTolerance
        {
            get { return this.m_AngleTolerance; }
            set
            {
                if (this.m_AngleTolerance == value) return;
                this.m_AngleTolerance = value;
                this.HasChanged = true;
            }
        }
        public LineSearchingVisionTool.PolarityConstants PolarityConstant
        {
            get { return this.m_PolarityConstant; }
            set { this.m_PolarityConstant = value; }
        }

        public int LineCount
        {
            get { return this.m_LineCount; }
            set { this.m_LineCount = value; }
        }
        #endregion
    }
    #endregion

    #region LineSearchingResult
    [Serializable]
    public class LineSearchingResult : VisionResult
    {
        #region Define
        [Serializable]
        public class ResultOverlayCollection : Collection<LineFrameVisionImageOverlay>
        {

        }
        #endregion

        #region Field
        private LineDCollection m_Lines;
        private ResultOverlayCollection m_ResultOverlays;
        #endregion

        #region Constructor
        public LineSearchingResult(string owner) : base(owner)
        {
            this.Lines = new LineDCollection();
            this.ResultOverlays = new ResultOverlayCollection();
        }
        #endregion

        #region Property
        public LineDCollection Lines
        {
            get { return this.m_Lines; }
            set { this.m_Lines = value; }
        }

        public ResultOverlayCollection ResultOverlays
        {
            get { return this.m_ResultOverlays; }
            set { this.m_ResultOverlays = value; }
        }
        #endregion
    }
    #endregion
}
