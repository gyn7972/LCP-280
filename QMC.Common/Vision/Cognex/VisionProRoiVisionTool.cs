/*
 * Purpose
 *      관심 영역을 설정하는 VisionPro Roi Vision Tool을 정의한다.
 * 
 * Revision
 *      1. Created: 2018.01.26 by JUNG.CY
 *      
 */

using System;

using Cognex.VisionPro;


using QMC.Common.Vision.Tools;


namespace QMC.Common.Vision.Cognex
{
    #region VisionProRoiVisionTool
    [Serializable]
    public class VisionProRoiVisionTool : RoiVisionTool
    {
        #region Define
        #endregion

        #region Field
        #endregion

        #region Constructor
        public VisionProRoiVisionTool(string name) : base(name)
        {
            this.Parameter = new VisionProRoiVisionToolParameter(name);
        }
        public VisionProRoiVisionTool() : this("") { }
        #endregion

        #region Property
        #endregion

        #region Method
        private int SetValue(VisionImage visionimage)
        {
            int ret = 0;
            VisionProCustomizedVisionImage cognexVisionImage = visionimage.CustomizedData as VisionProCustomizedVisionImage;
            CogRectangle parameter = null;
            RoiResult result = new RoiResult(this.Name);
            StopWatch timer = new StopWatch();

            try
            {
                timer.Start();

                if (this.Parameter.IsFull == true)
                {
                    cognexVisionImage.Region = null;
                }
                else
                {
                    parameter = new CogRectangle();
                    parameter.X = this.Parameter.StartLocation.X;
                    parameter.Y = this.Parameter.StartLocation.Y;
                    VisionToolLog.Write(this, string.Format("X : {0}, Y : {1}", parameter.X, parameter.Y));
                    parameter.Width = this.Parameter.Size.Width;
                    parameter.Height = this.Parameter.Size.Height;
                    VisionToolLog.Write(this, string.Format("Width : {0}, Height : {1}", parameter.Width, parameter.Height));
                    VisionToolLog.Write(this, string.Format("Full : {0}", this.Parameter.IsFull));
                    cognexVisionImage.Region = parameter;
                }

                timer.Stop();
                result.ProcessingTime = timer.Elapsed.Milliseconds;
                VisionToolLog.Write(this, string.Format("ProcessingTime : {0}", result.ProcessingTime));
            }
            finally
            {
                timer.Stop();
                this.Result = result;
            }
            return ret;
        }
        #endregion

        #region VisionTool Members
        public new VisionProRoiVisionToolParameter Parameter
        {
            get { return base.Parameter as VisionProRoiVisionToolParameter; }
            set { base.Parameter = value; }
        }

        protected override int OnRun()
        {
            int ret = 0;

            // VisionPro 네이티브 모듈 선 로드(디버거에서 LoaderLock 중단을 한 지점으로 고정)
            VisionProBootstrapper.WarmUp();

            VisionImage image = this.InputImage;
            if (image == null) return ret;

            if (image.CustomizedData == null)
            {
                if ((ret = VisionProCustomizedVisionImage.Create(ref image)) != 0) return ret;
            }

            if ((ret = this.SetValue(image)) != 0) return ret;

            this.OutputImage = this.InputImage;
            return ret;

            //int ret = 0;
            //VisionImage image = this.InputImage;
            //if(image == null) return ret;
            //if (image.CustomizedData == null)
            //{
            //    if ((ret = VisionProCustomizedVisionImage.Create(ref image)) != 0) return ret;
            //}

            //if ((ret = this.SetValue(image)) != 0) return ret;

            //this.OutputImage = this.InputImage;
            //return ret;
        }

        protected override int OnCheckedLicense()
        {
            int ret = 0;
            return ret;
        }

        protected override int OnPrepare()
        {
            int ret = 0;
            this.Result = new RoiResult(this.Name);
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

    #region VisionProRoiVisionToolParameter
    [Serializable]
    public class VisionProRoiVisionToolParameter : RoiVisionToolParameter
    {
        #region Field

        #endregion

        #region Constructor
        public VisionProRoiVisionToolParameter(string name) : base(name)
        {

        }
        public VisionProRoiVisionToolParameter() : this("") { }
        #endregion

        #region Property

        #endregion
    }
    #endregion
}