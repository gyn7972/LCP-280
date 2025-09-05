/*
 * Purpose
 *      VisionPro 3rd Party Library 구현된 Blob Vision Tool 클래스에 대해서 정의한다.
 * 
 * Revision
 *      1. Created: 2018.01.31 LEE.SH
 *      2. Modified: 2018.02.27 by JUNG.CY
 *      3. Modified: 2018.08.31 by JUNG.CY
 *          - Multi Thread 지원안하는 VisionPro Tool -> Run Method를 비동기로 호출.
 */


using System;
using System.ComponentModel;
using System.Drawing;

using QMC.Common.Vision;
using QMC.Common.Vision.Tools;

using Cognex.VisionPro;
using Cognex.VisionPro.Blob;
using Cognex.VisionPro.Implementation.Internal;


namespace QMC.Common.Vision.Cognex
{
    #region VisionProBlobVisionTool
    [Serializable]
    public class VisionProBlobVisionTool : BlobVisionTool
    {
        #region Define
        public delegate int GetValueDelegate();
        #endregion

        #region Field
        [NonSerialized]
        private CogBlobTool m_Tool;
        [NonSerialized]
        private GetValueDelegate m_Operational;
        [NonSerialized]
        private object m_SyncRoot;
        #endregion

        #region Constructor
        public VisionProBlobVisionTool(string name) : base(name)
        {
            this.Tool = new CogBlobTool();
            this.Tool.InputImage = new CogImage8Grey();
            this.Parameter = new VisionProBlobVisionToolParameter();
            this.OnPrepare();
        }
        public VisionProBlobVisionTool() : this("") { }
        #endregion

        #region Property
        [Browsable(false)]
        public CogBlobTool Tool
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
        
        private int GetValueProcedure()
        {
            int ret = 0;
            lock (this.m_SyncRoot)
            {
                BlobResult result = new BlobResult(this.Name);
                BlobResult.ItemResult itemResult = null;
                BlobResult.ItemResultKeyedCollection measures = null;
                CogBlobResultCollection blobResults = null;
                double itemValue = 0;

                try
                {
                    this.Tool.Run();

                    blobResults = this.Tool.Results.GetBlobs();
                    
                    if (blobResults != null)
                    {
                        for (int i = 0; i < blobResults.Count; i++)
                        {
                            measures = new BlobResult.ItemResultKeyedCollection();
                            for (int j = 0; j < this.Parameter.RunParams.RunTimeMeasures.Count; j++)
                            {
                                if (blobResults[i].GetMeasure(CogBlobMeasureConstants.Label) != this.Parameter.UsedLabel) break;
                                itemResult = new BlobResult.ItemResult();
                                itemValue = blobResults[i].GetMeasure(this.Parameter.RunParams.RunTimeMeasures[j].Measure);
                                itemResult = new BlobResult.ItemResult();
                                itemResult.Name = this.Parameter.RunParams.RunTimeMeasures[j].Measure;
                                itemResult.Value = itemValue;
                                measures.Add(itemResult);
                                VisionToolLog.Write(this, string.Format("{0} Result : {1}", itemResult.Name, itemResult.Value));
                            }
                            if (blobResults[i].GetMeasure(CogBlobMeasureConstants.Label) != this.Parameter.UsedLabel) continue;
                            result.PixelValues.Add(measures);
                        }

                        this.InputImage.CustomizedData.Image = this.Tool.Results.CreateBlobImage() as ICogImage;
                    }
                    else
                    {
                        //return ErrorManager.Register(this.Tool.RunStatus.Message);
                        return -1;
                    }
                }
                finally
                {
                    result.ProcessingTime = this.Tool.RunStatus.ProcessingTime;
                    VisionToolLog.Write(this, string.Format("ProcessingTime : {0}", result.ProcessingTime));
                    result.ResultMessage = this.Tool.RunStatus.Message;
                    VisionToolLog.Write(this, string.Format("ResultMessage : {0}", result.ResultMessage));

                    this.Result = result;

                    if (this.Parameter.ResultOverlayVisible == true)
                    {
                        this.CrossLineOverlay(new Size(this.InputImage.Header.Width, this.InputImage.Header.Height), CogBlobMeasureConstants.CenterMassX, CogBlobMeasureConstants.CenterMassY);
                    }
                }
            }
            return ret;
        }

        public int SetValue(VisionImage image)
        {
            int ret = 0;

            VisionProCustomizedVisionImage cognexVisionImage = image.CustomizedData as VisionProCustomizedVisionImage;
            ICogImage sourceImage = cognexVisionImage.Image as ICogImage;

            if (this.Parameter.HasChanged == true)
            {
                this.Tool.RunParams = this.Parameter.RunParams;
                //polarity Parameter 추가해주기.
                if (this.Parameter.Polarity == Polarity.DarkBlobs)
                {
                    this.Tool.RunParams.SegmentationParams.Polarity = CogBlobSegmentationPolarityConstants.DarkBlobs;
                }
                else
                {
                    this.Tool.RunParams.SegmentationParams.Polarity = CogBlobSegmentationPolarityConstants.LightBlobs;
                }                
                
                this.Tool.RunParams.ConnectivityMinPixels = this.Parameter.MinPixels;
                this.Tool.RunParams.SegmentationParams.HardFixedThreshold = this.Parameter.HardThreshold;
                this.Tool.RunParams.SegmentationParams.Mode = CogBlobSegmentationModeConstants.HardFixedThreshold;
                this.Tool.RunParams.SortEnabled = true;
                this.Tool.RunParams.SortAscending = true;
                this.Parameter.HasChanged = false;
            }

            if (cognexVisionImage.Region == null)
            {
                this.Tool.Region = null;
            }
            else
            {
                this.Tool.Region = cognexVisionImage.Region;
                cognexVisionImage.Region = null;
            }

            this.Tool.InputImage = sourceImage;

            return ret;
        }
        #endregion

        #region VisionTool Member
        public new VisionProBlobVisionToolParameter Parameter
        {
            get { return base.Parameter as VisionProBlobVisionToolParameter; }
            set { base.Parameter = value; }
        }

        protected override int OnRun()
        {
            int ret = 0;
        //    this.OnPrepare();
            VisionImage image = this.InputImage;

            if (image.CustomizedData == null)
            {
                if ((ret = VisionProCustomizedVisionImage.Create(ref image)) != 0) return ret;
            }
            if ((ret = this.SetValue(image)) != 0) return ret;

            //2018.08.31 Async Method로 변경. ( Multi Thread 지원 x )
            if ((ret = this.GetValue()) != 0) return ret;

            this.OutputImage = this.InputImage;

            return ret;
        }

        protected override int OnCheckedLicense()
        {
            int ret = 0;

            //if (CogLicense.IsEnabled(CogLicenseConstants.Blob) != true) return ErrorManager.Register(string.Format("{0} is not supported", CogLicenseConstants.Blob));
            if (CogLicense.IsEnabled(CogLicenseConstants.Blob) != true) return -1;

            return ret;
        }

        protected override int OnPrepare()
        {
            int ret = 0;
            this.Tool = new CogBlobTool();
            this.Tool.InputImage = new CogImage8Grey();
            this.m_Operational = new GetValueDelegate(this.GetValueProcedure);
            this.m_SyncRoot = new object();
            this.Result = new BlobResult(this.Name);
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

    #region VisionProBlobVisionToolParameter
    [Serializable]
    public class VisionProBlobVisionToolParameter : BlobVisionToolParameter
    {
        #region Field
        private CogBlob m_RunParams;
        private int m_UsedLabel;
        #endregion

        #region Constructor
        public VisionProBlobVisionToolParameter() : base()
        {
            this.RunParams = new CogBlob();

            this.RunParams.RunTimeMeasures.Add(new CogBlobMeasure(CogBlobMeasureConstants.Area));
            this.RunParams.RunTimeMeasures.Add(new CogBlobMeasure(CogBlobMeasureConstants.CenterMassX));
            this.RunParams.RunTimeMeasures.Add(new CogBlobMeasure(CogBlobMeasureConstants.CenterMassY));
            this.RunParams.RunTimeMeasures.Add(new CogBlobMeasure(CogBlobMeasureConstants.Label));

            this.UsedLabel = 1;
        }
        #endregion

        #region Property
        public CogBlob RunParams
        {
            get { return this.m_RunParams; }
            set
            {
                if (this.m_RunParams == value) return;
                this.m_RunParams = value;
                this.HasChanged = true;
            }
        }

        public int UsedLabel
        {
            get { return this.m_UsedLabel; }
            set
            {
                if (this.m_UsedLabel == value) return;
                this.m_UsedLabel = value;
                this.HasChanged = true;
            }
        }
        #endregion
    }
    #endregion
}