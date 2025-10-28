/*
 * Purpose
 *      Line 찾는 VisionPro Line Search Vision Tool을 정의한다.
 * 
 * Revision
 *      1. Created: 2018.09.07 by JUNG.CY
 *      
 */


using System;
using System.ComponentModel;

using QMC.Common.Vision.Tools;

using Cognex.VisionPro;
using Cognex.VisionPro.LineMax;
using Cognex.VisionPro.Implementation.Internal;

namespace QMC.Common.Vision.Cognex
{
    #region VisionProLineSearchingVisionTool
    [Serializable]
    public class VisionProLineSearchingVisionTool : LineSearchingVisionTool
    {
        #region Define
        public delegate int GetValueDelegate();
        #endregion

        #region Field
        [NonSerialized]
        private CogLineMaxTool m_Tool;
        [NonSerialized]
        private GetValueDelegate m_Operational;
        [NonSerialized]
        private object m_SyncRoot;
        #endregion

        #region Constructor
        public VisionProLineSearchingVisionTool(string name) : base(name)
        {
            this.Parameter = new VisionProLineSearchingVisionToolParameter();
        }
        public VisionProLineSearchingVisionTool() : this("") { }
        #endregion

        #region Property
        /// <summary>
        /// VisionPro Line Max Vision Tool을 가져온다.
        /// </summary>
        [Browsable(false)]
        public CogLineMaxTool Tool
        {
            get { return this.m_Tool; }
            private set { this.m_Tool = value; }
        }
        #endregion

        #region Method
        private IAsyncResult BeginGetValue(AsyncCallback callback, object value)
        {
            return this.m_Operational.BeginInvoke(callback, value);
        }

        private IAsyncResult BeginGetValue()
        {
            return this.BeginGetValue(null, null);
        }

        public int GetValue()
        {
            IAsyncResult ar = this.BeginGetValue(null, null);
            return this.m_Operational.EndInvoke(ar);
        }

        private int SetValue(VisionImage image)
        {
            int ret = 0;
            CogLineMaxPolarityConstants[] constants = (CogLineMaxPolarityConstants[])Enum.GetValues(typeof(CogLineMaxPolarityConstants));
            VisionProCustomizedVisionImage cognexVisionImage = null;
            ICogImage cognexImage = null;

            this.Tool.RunParams = this.Parameter.RunParams;

            this.Tool.RunParams.ExpectedLineNormal.Angle = qMath.DegreeToRadian(this.Parameter.ExpectedLineAngle);
            this.Tool.RunParams.LineAngleTolerance = qMath.DegreeToRadian((Math.Abs(this.Parameter.AngleTolerance.Maximum) + Math.Abs(this.Parameter.AngleTolerance.Minimum)) / 2);
            this.Tool.RunParams.EdgeAngleTolerance = qMath.DegreeToRadian((Math.Abs(this.Parameter.AngleTolerance.Maximum) + Math.Abs(this.Parameter.AngleTolerance.Minimum)) / 2);
            this.Tool.RunParams.Polarity = constants[(int)this.Parameter.PolarityConstant];
            this.Tool.RunParams.MaxNumLines = 1;

            cognexVisionImage = image.CustomizedData as VisionProCustomizedVisionImage;
            cognexImage = cognexVisionImage.Image as ICogImage;
            this.Tool.InputImage = cognexImage;

            if (cognexVisionImage.Region == null)
            {
                this.Tool.Region = null;
            }
            else
            {
                this.Tool.Region = cognexVisionImage.Region;
                cognexVisionImage.Region = null;
            }

            return ret;
        }
        private int GetValueProcedure()
        {
            int ret = 0;
            LineSearchingResult result = new LineSearchingResult(this.Name);
            CogLineSegment line = null;
            LineD lineValue = new LineD();
            PointD point;
            double angle = 0.0;
            LineFrameVisionImageOverlay overlay = null;

            lock (this.m_SyncRoot)
            {
                try
                {
                    this.Tool.Run();

                    if (this.Tool.Results != null)
                    {
                        for (int i = 0; i < this.Tool.Results.Count; i++)
                        {
                            lineValue = new LineD();
                            line = this.Tool.Results[i].GetLineSegment();

                            point = new PointD(line.StartX, line.StartY);
                            VisionToolLog.Write(this, string.Format("Start [{0}] : {1}, {2}", i, point.X, point.Y));
                            lineValue.Start = point;

                            point = new PointD(line.EndX, line.EndY);
                            VisionToolLog.Write(this, string.Format("End [{0}] : {1}, {2}", i, point.X, point.Y));
                            lineValue.End = point;

                            angle = line.Rotation * 180.0 / Math.PI;
                            VisionToolLog.Write(this, string.Format("Angle [{0}] : {1}", i, angle));
                            lineValue.Angle = angle;

                            VisionToolLog.Write(this, string.Format("Line Count [{0}] : {1}", i, this.Tool.Results[i].Inliers.Count));

                            result.Lines.Add(lineValue);

                            overlay = new LineFrameVisionImageOverlay();
                            overlay.StartLocation = lineValue.Start;
                            overlay.EndLocation = lineValue.End;
                            overlay.Visible = this.Parameter.ResultOverlayVisible;

                            this.Result.ResultOverlays.Add(overlay);
                        }
                    }
                    else
                    {
                        //return ErrorManager.Register(this.Tool.RunStatus.Message);
                        return -1;
                    }

                }
                finally
                {
                    this.Result = result;

                    result.ProcessingTime = this.Tool.RunStatus.ProcessingTime;
                    VisionToolLog.Write(this, string.Format("ProcessingTime : {0}", result.ProcessingTime));
                    result.ResultMessage = this.Tool.RunStatus.Message;
                    VisionToolLog.Write(this, string.Format("ResultMessage : {0}", result.ResultMessage));
                }
            }
            return ret;
        }
        #endregion

        #region VisionTool Members
        public new VisionProLineSearchingVisionToolParameter Parameter
        {
            get { return base.Parameter as VisionProLineSearchingVisionToolParameter; }
            set { base.Parameter = value; }
        }

        protected override int OnRun()
        {
            int ret = 0;
            VisionImage image = this.InputImage;

            if (image.CustomizedData == null)
            {
                if ((ret = VisionProCustomizedVisionImage.Create(ref image)) != 0) return ret;
            }
            if ((ret = this.SetValue(image)) != 0) return ret;
            if ((ret = this.GetValue()) != 0) return ret;

            this.OutputImage = this.InputImage;
            return ret;
        }

        protected override int OnCheckedLicense()
        {
            int ret = 0;

            //if (CogLicense.IsEnabled(CogLicenseConstants.LineMax) != true) return ErrorManager.Register(string.Format("{0} is not supported", CogLicenseConstants.LineMax));
            if (CogLicense.IsEnabled(CogLicenseConstants.LineMax) != true) return -1;

            return ret;
        }

        protected override int OnPrepare()
        {
            int ret = 0;
            this.Tool = new CogLineMaxTool();
            this.Tool.InputImage = new CogImage8Grey();
            this.m_Operational = new GetValueDelegate(this.GetValueProcedure);
            this.m_SyncRoot = new object();
            this.Result = new LineSearchingResult(this.Name);
            return ret;
        }
        #endregion

        #region IDisposable Member
        protected override void OnDispose()
        {

        }
        #endregion
    }
    #endregion

    #region VisionProLineSearchingVisionToolParameter
    [Serializable]
    public class VisionProLineSearchingVisionToolParameter : LineSearchingVisionToolParameter
    {
        #region Field
        private CogLineMax m_RunParams;
        #endregion

        #region Constructor
        public VisionProLineSearchingVisionToolParameter() : base()
        {
            this.RunParams = new CogLineMax();
            this.RunParams.Polarity = CogLineMaxPolarityConstants.DarkToLight;
        }
        #endregion

        #region Property
        public CogLineMax RunParams
        {
            get { return this.m_RunParams; }
            set
            {
                if (this.m_RunParams == value) return;
                this.m_RunParams = value;
                this.HasChanged = true;
            }
        }
        #endregion
    }
    #endregion
}
