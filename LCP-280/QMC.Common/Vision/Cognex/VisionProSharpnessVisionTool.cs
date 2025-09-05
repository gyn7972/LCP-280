/*
 * Purpose
 *      특정 위치의 Sharpness를 구하는 VisionPro Sharpness Vision Tool을 정의한다.
 * 
 * Revision
 *      1. Created: 2018.01.26 by JUNG.CY
 *      
 */

using System;
using System.ComponentModel;

using QMC.Common.Vision.Tools;

using Cognex.VisionPro;
using Cognex.VisionPro.ImageProcessing;

namespace QMC.Common.Vision.Cognex
{
    #region VisionProSharpnessVisionTool
    [Serializable]
    public class VisionProSharpnessVisionTool : SharpnessVisionTool
    {
        #region Define
        #endregion

        #region Field
        [NonSerialized]
        private CogImageSharpnessTool m_Tool;
        #endregion

        #region Constructor
        public VisionProSharpnessVisionTool(string name) : base(name)
        {
            this.Parameter = new VisionProSharpnessVisionToolParameter();
        }
        public VisionProSharpnessVisionTool() : this("") { }
        #endregion

        #region Property
        [Browsable(false)]
        public CogImageSharpnessTool Tool
        {
            get { return this.m_Tool; }
            private set { this.m_Tool = value; }
        }
        #endregion

        #region Method
        private int SetValue(VisionImage image)
        {
            int ret = 0;
            VisionProCustomizedVisionImage cognexVisionImage = image.CustomizedData as VisionProCustomizedVisionImage;
            CogImage8Grey cognexImage = image.CustomizedData.Image as CogImage8Grey;

            if (this.Tool == null)
                this.Tool = new CogImageSharpnessTool();

            this.Tool.RunParams = this.Parameter.RunParams;

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
        private int GetValue()
        {
            int ret = 0;
            SharpnessResult result = new SharpnessResult(this.Name);
            try
            {
                this.Tool.Run();

                result.SharpnessScore = this.Tool.Score;
                VisionToolLog.Write(this, string.Format("Score : {0}", result.SharpnessScore));
            }
            finally
            {
                this.Result = result;

                result.ProcessingTime = this.Tool.RunStatus.ProcessingTime;
                VisionToolLog.Write(this, string.Format("ProcessingTime : {0}", result.ProcessingTime));
                result.ResultMessage = this.Tool.RunStatus.Message;
                VisionToolLog.Write(this, string.Format("ResultMessage : {0}", result.ResultMessage));
            }

            return ret;
        }
        #endregion

        #region VisionTool Members
        public new VisionProSharpnessVisionToolParameter Parameter
        {
            get { return base.Parameter as VisionProSharpnessVisionToolParameter; }
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
            return ret;
        }

        protected override int OnPrepare()
        {
            int ret = 0;
            this.Tool = new CogImageSharpnessTool();
            this.Tool.InputImage = new CogImage8Grey();
            this.Result = new SharpnessResult(this.Name);
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

    #region VisionProSharpnessVisionToolParameter
    [Serializable]
    public class VisionProSharpnessVisionToolParameter : SharpnessVisionToolParameter
    {
        #region Field
        private CogImageSharpness m_RunParams;
        #endregion

        #region Constructor
        public VisionProSharpnessVisionToolParameter() : base()
        {
            this.RunParams = new CogImageSharpness();
        }
        #endregion

        #region Property
        public CogImageSharpness RunParams
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