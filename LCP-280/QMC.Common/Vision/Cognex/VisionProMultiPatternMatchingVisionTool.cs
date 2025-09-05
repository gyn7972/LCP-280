using Cognex.VisionPro;
using Cognex.VisionPro.Implementation.Internal;
using Cognex.VisionPro.PMAlign;
using QMC.Common.Vision.Tools;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QMC.Common.Vision.Cognex
{
    #region VisionProMultiPatternMatchingVisionTool
    [Serializable]
    public class VisionProMultiPatternMatchingVisionTool : PatternMatchingVisionTool
    {
        #region Define
        public enum ResultType
        {
            Center,
            LeftTop,
            RightTop,
            LeftBottom,
            RightBottom,
        }

        //public enum RunAlgorithm
        //{
        //    PatMax,
        //    PatFlex
        //}
        #endregion

        #region Field
        [NonSerialized]
        private CogPMAlignMultiTool m_Tool;
        private VisionImage m_LatestImage;
        #endregion

        #region Constructor
        public VisionProMultiPatternMatchingVisionTool(string name) : base(name)
        {
            this.Parameter = new VisionProMultiPatternMatchingVisionToolParameter();
            this.m_Tool = new CogPMAlignMultiTool();
        }
        public VisionProMultiPatternMatchingVisionTool() : this("") { }
        #endregion

        #region Property
        /// <summary>
        /// VisionPro Pattern Match Multi Align Vision Tool을 가져온다.
        /// </summary>
        [Browsable(false)]
        public CogPMAlignMultiTool Tool
        {
            get { return this.m_Tool; }
            private set { this.m_Tool = value; }
        }
        #endregion

        #region Method
        private int SetValue(VisionImage image)
        {
            int ret = 0;

            VisionProCustomizedVisionImage cognexVisionImage = null;
            ICogImage cognexImage = null;

            this.m_LatestImage = image;
            if ((ret = this.SetParameter()) != 0) return ret;

            if (this.IsLearn == false)
                this.LearnTrainImage();

            cognexVisionImage = image.CustomizedData as VisionProCustomizedVisionImage;
            cognexImage = cognexVisionImage.Image as ICogImage;
            this.Tool.InputImage = cognexImage;

            if (cognexVisionImage.Region == null)
            {
                this.Tool.SearchRegion = null;
            }
            else
            {
                this.Tool.SearchRegion = cognexVisionImage.Region;
                cognexVisionImage.Region = null;
            }

            return ret;
        }

        private int GetValue()
        {
            int ret = 0;
            int count = 0;
            PatternMatchingResult result = new PatternMatchingResult(this.Name);
            PatternMatchingResult.PatternMatchingResultValue resultValue;
            CogRectangle roi = null;
            double angle = 0.0;

            try
            {
                this.Tool.Run();

                if (this.Tool.Results != null)
                {
                    if (this.Parameter.MaxInstance == -1)
                        count = this.Tool.Results.PMAlignResults.Count;
                    else
                    {
                        if (this.Tool.Results.PMAlignResults.Count < this.Parameter.MaxInstance)
                            count = this.Tool.Results.PMAlignResults.Count;
                        else
                            count = this.Parameter.MaxInstance;
                    }

                    for (int i = 0; i < count; i++)
                    {
                        if (this.Tool.Results.PMAlignResults[i].Score < this.Parameter.MinScore) continue;
                        if (this.Tool.Results.PMAlignResults[i].Accepted == false) continue;
                        angle = this.Tool.Results.PMAlignResults[i].GetPose().Rotation;

                        // 2018.03.16 jung.cy VisionPro Library에서 Angle 제한이 되지 않아 조건 추가
                        // 단, ZoneAngle을 LowHigh로 설정했을시에만 동작 ( Norminal 일 경우는 결과값이 달라짐 )
                        if (this.Tool.RunParams.PMAlignRunParams.ZoneAngle.Configuration == CogPMAlignZoneConstants.LowHigh)
                        {
                            if (this.Tool.RunParams.PMAlignRunParams.ZoneAngle.Low > this.Tool.Results.PMAlignResults[i].GetPose().Rotation ||
                                this.Tool.Results.PMAlignResults[i].GetPose().Rotation > this.Tool.RunParams.PMAlignRunParams.ZoneAngle.High) continue;
                        }
                        resultValue = new PatternMatchingResult.PatternMatchingResultValue();
                        resultValue.X = this.Tool.Results.PMAlignResults[i].GetPose().TranslationX;
                        resultValue.Y = this.Tool.Results.PMAlignResults[i].GetPose().TranslationY;
                        resultValue.R = this.Tool.Results.PMAlignResults[i].GetPose().Rotation * 180.0 / Math.PI;
                        resultValue.Score = this.Tool.Results.PMAlignResults[i].Score;
                        result.Values.Add(resultValue);
                    }

                    //if (result.Values.Count != 0)
                    //{
                    //    result.Found = true;
                    //}
                    //else
                    //{
                    //    result.Found = false;
                    //}
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

                this.SortingResult();

                //결과값에 대한 TrainImage Size로 해야되지 않나 ?
                this.ConfirmDuplication(new Size(this.Tool.Operator[0].Pattern.TrainImage.Width, this.Tool.Operator[0].Pattern.TrainImage.Height));

                for (int i = 0; i < this.Result.Values.Count; i++)
                {
                    VisionToolLog.Write(this, string.Format("X[{0}] : {1}", i, this.Result.Values[i].X));
                    VisionToolLog.Write(this, string.Format("Y[{0}] : {1}", i, this.Result.Values[i].Y));
                    VisionToolLog.Write(this, string.Format("R[{0}] : {1}", i, this.Result.Values[i].R));
                    VisionToolLog.Write(this, string.Format("Score[{0}] : {1}", i, this.Result.Values[i].Score));
                }

                result.ProcessingTime = this.Tool.RunStatus.ProcessingTime;
                VisionToolLog.Write(this, string.Format("ProcessingTime : {0}", this.Result.ProcessingTime));
                result.ResultMessage = this.Tool.RunStatus.Message;
                VisionToolLog.Write(this, string.Format("ResultMessage : {0}", this.Result.ResultMessage));

                if (this.Tool.Operator[0].Pattern.TrainRegion == null)
                {
                    this.RotateMatchPoint(new Size(this.Tool.Operator[0].Pattern.TrainImage.Width, this.Tool.Operator[0].Pattern.TrainImage.Height));
                }
                else
                {
                    roi = this.Tool.Operator[0].Pattern.TrainRegion as CogRectangle;
                    this.RotateMatchPoint(new Size((int)roi.Width, (int)roi.Height));
                }
            }


            return ret;
        }

        public int LearnTrainImage()
        {
            int ret = 0;

            if ((ret = this.OnLearn()) != 0)
            {
                return ret;
            }

            this.IsLearn = true;

            return ret;
        }
        #endregion

        #region VisionTool Members
        public new VisionProMultiPatternMatchingVisionToolParameter Parameter
        {
            get { return base.Parameter as VisionProMultiPatternMatchingVisionToolParameter; }
            set { base.Parameter = value; }
        }

        protected override int OnCheckedLicense()
        {
            int ret = 0;

            if (this.Parameter.RunParams.PMAlignRunParams.RunAlgorithm == CogPMAlignRunAlgorithmConstants.PatMax)
            {
                //if (CogLicense.IsEnabled(CogLicenseConstants.PatMax) != true) return ErrorManager.Register(string.Format("{0} is not supported", CogLicenseConstants.PatMax));
                if (CogLicense.IsEnabled(CogLicenseConstants.PatMax) != true) return -1;
            }
            else if (this.Parameter.RunParams.PMAlignRunParams.RunAlgorithm == CogPMAlignRunAlgorithmConstants.PatQuick)
            {
                //if (CogLicense.IsEnabled(CogLicenseConstants.PatQuick) != true) return ErrorManager.Register(string.Format("{0} is not supported", CogLicenseConstants.PatQuick));
                if (CogLicense.IsEnabled(CogLicenseConstants.PatQuick) != true) return -1;
            }
            else if (this.Parameter.RunParams.PMAlignRunParams.RunAlgorithm == CogPMAlignRunAlgorithmConstants.PatFlex)
            {
                //if (CogLicense.IsEnabled(CogLicenseConstants.PatFlex) != true) return ErrorManager.Register(string.Format("{0} is not supported", CogLicenseConstants.PatFlex));
                if (CogLicense.IsEnabled(CogLicenseConstants.PatFlex) != true) return -1;
            }

            return ret;
        }

        protected override void OnDispose()
        {
            return;
        }

        protected override int OnPrepare()
        {
            int ret = 0;
            this.Tool = new CogPMAlignMultiTool();
            this.Tool.InputImage = new CogImage8Grey();
            this.Result = new PatternMatchingResult(this.Name);
            return ret;
        }

        protected override int OnRun()
        {
            int ret = 0;
            VisionImage image = this.InputImage;
            if (image != null)
            {
                if (image.CustomizedData == null)
                {
                    if ((ret = VisionProCustomizedVisionImage.Create(ref image)) != 0) return ret;
                }
                if ((ret = this.SetValue(image)) != 0) return ret;
                if ((ret = this.GetValue()) != 0) return ret;
                this.OutputImage = this.InputImage;
            }

            return ret;
        }

        protected override int OnLearn()
        {
            int ret = 0;
            VisionImage image = null;
            ICogImage cognexImage = null;
            VisionProCustomizedVisionImage cognexVisionImage = null;
            CogRectangle rectangle = null;
            CycleTimer timer = new CycleTimer();

            timer.Start();

            try
            {
                this.Parameter.Patterns.Clear();

                for (int i = 0; i < this.Parameter.TrainImages.Count; i++)
                {
                    image = this.Parameter.TrainImages[i];

                    if (image.CustomizedData == null)
                    {
                        if ((ret = VisionProCustomizedVisionImage.Create(ref image)) != 0) return ret;
                    }

                    cognexVisionImage = image.CustomizedData as VisionProCustomizedVisionImage;
                    cognexImage = image.CustomizedData.Image as ICogImage;

                    this.Parameter.Patterns.Add(new CogPMAlignPatternItem());

                    //if (cognexVisionImage.Region != this.Parameter.Patterns[i].Pattern.TrainRegion ||
                    //    this.Parameter.Patterns[i].Pattern.TrainImage != cognexImage)
                    //{
                    if (cognexVisionImage.Region == null)
                    {
                        this.Parameter.Patterns[i].Pattern.TrainRegion = null;
                    }
                    else
                    {
                        this.Parameter.Patterns[i].Pattern.TrainRegion = cognexVisionImage.Region;
                        cognexVisionImage.Region = null;
                    }

                    this.Parameter.Patterns[i].Pattern.TrainImage = cognexImage;

                    rectangle = this.Parameter.Patterns[i].Pattern.TrainRegion as CogRectangle;

                    switch (this.Parameter.Type)
                    {
                        case ResultType.LeftTop:
                            if (rectangle == null)
                            {
                                this.Parameter.Patterns[i].Pattern.Origin.TranslationX = 0;
                                this.Parameter.Patterns[i].Pattern.Origin.TranslationY = 0;
                            }
                            else
                            {
                                this.Parameter.Patterns[i].Pattern.Origin.TranslationX = rectangle.X;
                                this.Parameter.Patterns[i].Pattern.Origin.TranslationY = rectangle.Y;
                            }
                            break;
                        case ResultType.LeftBottom:
                            if (rectangle == null)
                            {
                                this.Parameter.Patterns[i].Pattern.Origin.TranslationX = 0;
                                this.Parameter.Patterns[i].Pattern.Origin.TranslationY = cognexImage.Height;
                            }
                            else
                            {
                                this.Parameter.Patterns[i].Pattern.Origin.TranslationX = rectangle.X;
                                this.Parameter.Patterns[i].Pattern.Origin.TranslationY = rectangle.Y + rectangle.Height;
                            }
                            break;
                        case ResultType.RightTop:
                            if (rectangle == null)
                            {
                                this.Parameter.Patterns[i].Pattern.Origin.TranslationX = cognexImage.Width;
                                this.Parameter.Patterns[i].Pattern.Origin.TranslationY = 0;
                            }
                            else
                            {
                                this.Parameter.Patterns[i].Pattern.Origin.TranslationX = rectangle.Y + rectangle.Width;
                                this.Parameter.Patterns[i].Pattern.Origin.TranslationY = rectangle.Y;
                            }

                            break;
                        case ResultType.RightBottom:
                            if (rectangle == null)
                            {
                                this.Parameter.Patterns[i].Pattern.Origin.TranslationX = cognexImage.Width;
                                this.Parameter.Patterns[i].Pattern.Origin.TranslationY = cognexImage.Height;
                            }
                            else
                            {
                                this.Parameter.Patterns[i].Pattern.Origin.TranslationX = rectangle.Y + rectangle.Width;
                                this.Parameter.Patterns[i].Pattern.Origin.TranslationY = rectangle.Y + rectangle.Height;
                            }
                            break;
                        case ResultType.Center:
                            if (rectangle == null)
                            {
                                this.Parameter.Patterns[i].Pattern.Origin.TranslationX = cognexImage.Width / 2;
                                this.Parameter.Patterns[i].Pattern.Origin.TranslationY = cognexImage.Height / 2;
                            }
                            else
                            {
                                this.Parameter.Patterns[i].Pattern.Origin.TranslationX = rectangle.CenterX;
                                this.Parameter.Patterns[i].Pattern.Origin.TranslationY = rectangle.CenterY;
                            }
                            break;
                    }
                }
                //}

                #region MarkImage 추가
                //MaskImage 추가
                //if (this.Parameter.UseMaskImage == true)
                //{
                //    //임시 주석
                //    //this.Parameter.Patterns[i].Pattern.TrainImageMask = new CogImage8Grey(image.Header.Width, image.Header.Height);

                //    //Train Image Mask Set 255
                //    for (int i = 0; i < this.Parameter.Patterns[i].Pattern.TrainImageMask.Height; i++)
                //    {
                //        for (int j = 0; j < this.Parameter.Patterns[i].Pattern.TrainImageMask.Width; j++)
                //        {
                //            this.Parameter.Patterns[i].Pattern.TrainImageMask.SetPixel(j, i, 255);
                //        }
                //    }

                //    //Train Image Mask Set  0
                //    for (int i = (int)this.Parameter.MaskRegion.Y; i < (int)(this.Parameter.MaskRegion.Y + this.Parameter.MaskRegion.Height); i++)
                //    {
                //        for (int j = (int)this.Parameter.MaskRegion.X; j < (int)(this.Parameter.MaskRegion.X + this.Parameter.MaskRegion.Width); j++)
                //        {
                //            this.Parameter.Patterns[i].Pattern.TrainImageMask.SetPixel(j, i, 0);
                //        }
                //    }
                //}
                ////Mask Image 사용 안한다면 Mask Region 전부 255로 Set
                //else
                //{
                //    //임시주석
                //    //this.Parameter.Patterns[i].Pattern.TrainImageMask = new CogImage8Grey(image.Header.Width, image.Header.Height);

                //    for (int i = 0; i < this.Parameter.Patterns[i].Pattern.TrainImageMask.Height; i++)
                //    {
                //        for (int j = 0; j < this.Parameter.Patterns[i].Pattern.TrainImageMask.Width; j++)
                //        {
                //            this.Parameter.Patterns[i].Pattern.TrainImageMask.SetPixel(j, i, 255);
                //        }
                //    }
                //}
                #endregion

                #region AutoEdgeThreshold Set
                //if (this.Parameter.AutoEdgeThresholdEnabled == true)
                //{
                //    this.Parameter.Patterns[i].Pattern.AutoEdgeThresholdEnabled = true;
                //    this.Parameter.RunParams.PMAlignRunParams.AutoEdgeThresholdEnabled = true;
                //}
                //else
                //{
                //    this.Parameter.Patterns[i].Pattern.AutoEdgeThresholdEnabled = false;
                //    this.Parameter.RunParams.PMAlignRunParams.AutoEdgeThresholdEnabled = false;
                //    this.Parameter.Patterns[i].Pattern.EdgeThreshold = this.Parameter.EdgeThreshold;
                //    this.Parameter.RunParams.PMAlignRunParams.EdgeThreshold = this.Parameter.EdgeThreshold;
                //}
                #endregion

                #region Train Polarity Set
                //if (this.Parameter.IgnorePolarity == true)
                //{
                //    this.Parameter.Patterns[i].Pattern.IgnorePolarity = true;
                //}
                //else
                //{
                //    this.Parameter.Patterns[i].Pattern.IgnorePolarity = false;
                //}
                #endregion

                VisionToolLog.Write(this, string.Format("PatternMatching Algorithm : [{0}]", this.Parameter.RunParams.PMAlignRunParams.RunAlgorithm.ToString()));
                this.Tool.Operator.Clear();

                for (int i = 0; i < this.Parameter.Patterns.Count; i++)
                {
                    this.Tool.Operator.Add(this.Parameter.Patterns[i]);
                    this.Tool.Operator[i].Pattern.Train();
                }
                //this.Tool.Pattern = this.Parameter.Pattern;
                //this.Tool.Pattern.TrainImageMask = this.Parameter.Pattern.TrainImageMask;

                this.Tool.Operator.Train();
                this.Tool.Operator.AddAllItemsToSearchOrder();
            }
            catch (Exception ex)
            {
                Log.Write("Train", ex.Message);
            }
            finally
            {
                timer.End();
                VisionToolLog.Write(this, String.Format($"OnLearn Interval Time :{timer.Maximum.TotalMilliseconds}ms"));
            }

            return ret;
        }

        protected int SetParameter()
        {
            int ret = 0;
            CycleTimer timer = new CycleTimer();

            timer.Start();

            try
            {
                if (this.Parameter.AngleTolerance.Maximum == 0 && this.Parameter.AngleTolerance.Minimum == 0)
                    this.Parameter.RunParams.PMAlignRunParams.ZoneAngle.Configuration = CogPMAlignZoneConstants.Nominal;
                else
                {
                    this.Parameter.RunParams.PMAlignRunParams.ZoneAngle.Configuration = CogPMAlignZoneConstants.LowHigh;
                    this.Parameter.RunParams.PMAlignRunParams.ZoneAngle.Low = this.Parameter.AngleTolerance.Minimum / 180.0 * Math.PI;
                    this.Parameter.RunParams.PMAlignRunParams.ZoneAngle.High = this.Parameter.AngleTolerance.Maximum / 180.0 * Math.PI;
                }

                if (this.Parameter.MaxInstance == -1)
                    this.Parameter.RunParams.PMAlignRunParams.ApproximateNumberToFind = 1;
                else
                    this.Parameter.RunParams.PMAlignRunParams.ApproximateNumberToFind = this.Parameter.MaxInstance;

                this.Parameter.RunParams.PMAlignRunParams.AcceptThreshold = this.Parameter.MinScore;
                this.Tool.RunParams = this.Parameter.RunParams;
            }
            catch (Exception)
            {
                VisionToolLog.Write(this, string.Format($"{this.Name} SetParameter is Failed"));
            }
            finally
            {
                timer.End();
                VisionToolLog.Write(this, String.Format($"OnLearn Interval Time :{timer.Maximum.TotalMilliseconds}ms"));
            }


            return ret;
        }
        #endregion
    }
    #endregion

    #region VisionProMultiPatternMatchingVisionToolParameter
    [Serializable]
    public class VisionProMultiPatternMatchingVisionToolParameter : PatternMatchingVisionToolParameter
    {
        #region Field
        //private CogPMAlignPattern m_Pattern;
        private List<CogPMAlignPatternItem> m_Patterns;
        private CogPMAlignMultiRunParams m_RunParams;
        private VisionProMultiPatternMatchingVisionTool.ResultType m_Type;
        private List<VisionImage> m_TrainImages;
        #endregion

        #region Constructor
        public VisionProMultiPatternMatchingVisionToolParameter() : base()
        {
            //this.Pattern = new CogPMAlignPattern();
            this.RunParams = new CogPMAlignMultiRunParams();
            this.Patterns = new List<CogPMAlignPatternItem>();
            //this.Pattern.TrainAlgorithm = CogPMAlignTrainAlgorithmConstants.PatMax;
            this.RunParams.PMAlignRunParams.RunAlgorithm = CogPMAlignRunAlgorithmConstants.PatMax;
            this.RunParams.PMAlignRunParams.ScoreUsingClutter = false;
            this.RunParams.PMAlignRunParams.ZoneAngle.Configuration = CogPMAlignZoneConstants.LowHigh;
            this.RunParams.PMAlignRunParams.ZoneAngle.High = 0.78539816339744828;
            this.RunParams.PMAlignRunParams.ZoneAngle.Low = -0.78539816339744828;
            this.SortType = PatternMatchingVisionTool.SortType.X;
            this.Type = VisionProMultiPatternMatchingVisionTool.ResultType.Center;
            this.ResultOverlayVisible = true;
            this.MaxInstance = -1;
            this.MinScore = -1;
        }
        #endregion

        #region Property
        public List<CogPMAlignPatternItem> Patterns
        {
            get { return this.m_Patterns; }
            set
            {
                if (this.m_Patterns == value) return;
                this.m_Patterns = value;
                this.HasChanged = true;
            }
        }
        public CogPMAlignMultiRunParams RunParams
        {
            get { return this.m_RunParams; }
            set
            {
                if (this.m_RunParams == value) return;
                this.m_RunParams = value;
                this.HasChanged = true;
            }
        }
        public VisionProMultiPatternMatchingVisionTool.ResultType Type
        {
            get { return this.m_Type; }
            set
            {
                if (this.m_Type == value) return;
                this.m_Type = value;
                this.HasChanged = true;
            }
        }

        public List<VisionImage> TrainImages
        {
            get { return this.m_TrainImages; }
            set { this.m_TrainImages = value; }
        }
        #endregion
    }
    #endregion
}
