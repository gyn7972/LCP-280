/*
 * Purpose
 *      특정 위치를 찾는 VisionPro Pattern Match Align Vision Tool을 정의한다.
 * 
 * Revision
 *      1. Created: 2018.01.26 by JUNG.CY
 *      2. Modified: 2020.05.04 by yjbaek
 *          - Duplication 확인      
 *      
 */


using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Threading;

using QMC.Common.Vision.Tools;

using Cognex.VisionPro;
using Cognex.VisionPro.PMAlign;
using Cognex.VisionPro.Implementation.Internal;


namespace QMC.Common.Vision.Cognex
{
    #region VisionProPatternMatchingVisionTool
    [Serializable]
    public class VisionProPatternMatchingVisionTool : PatternMatchingVisionTool
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
        //private CogPMAlignTool m_Tool;
        private Lazy<CogPMAlignTool> _lazyTool = 
            new Lazy<CogPMAlignTool>(() => new CogPMAlignTool(), LazyThreadSafetyMode.ExecutionAndPublication);

        private VisionImage m_LatestImage;
        #endregion

        #region Constructor
        public VisionProPatternMatchingVisionTool(string name) : base(name)
        {
            this.Parameter = new VisionProPatternMatchingVisionToolParameter();
            //m_Tool = new CogPMAlignTool();
        }

        public VisionProPatternMatchingVisionTool() : this("") { }
        #endregion

        #region Property
        /// <summary>
        /// VisionPro Pattern Match Align Vision Tool을 가져온다.
        /// </summary>
        [Browsable(false)]
        public CogPMAlignTool Tool
        {
            get { return _lazyTool.Value; }
            // 재초기화가 필요할 때 OnPrepare에서 강제로 교체할 수 있도록 private set 구현
            private set
            {
                _lazyTool = new Lazy<CogPMAlignTool>(() => value, LazyThreadSafetyMode.ExecutionAndPublication);
            }
            //get { return this.m_Tool; }
            //private set { this.m_Tool = value; }
        }
        #endregion

        #region Method
        private int SetValue(VisionImage image)
        {
            int ret = 0;

            VisionProCustomizedVisionImage cognexVisionImage = null;
            ICogImage cognexImage = null;

            this.m_LatestImage = image;
            if ((ret = this.OnLearn()) != 0) return ret;

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
                        count = this.Tool.Results.Count;
                    else
                    {
                        if (this.Tool.Results.Count < this.Parameter.MaxInstance)
                            count = this.Tool.Results.Count;
                        else
                            count = this.Parameter.MaxInstance;
                    }

                    for (int i = 0; i < count; i++)
                    {
                        if (this.Tool.Results[i].Score < this.Parameter.MinScore) continue;
                        if (this.Tool.Results[i].Accepted == false) continue;
                        angle = this.Tool.Results[i].GetPose().Rotation;

                        // 2018.03.16 jung.cy VisionPro Library에서 Angle 제한이 되지 않아 조건 추가
                        // 단, ZoneAngle을 LowHigh로 설정했을시에만 동작 ( Norminal 일 경우는 결과값이 달라짐 )
                        if (this.Tool.RunParams.ZoneAngle.Configuration == CogPMAlignZoneConstants.LowHigh)
                        {
                            if (this.Tool.RunParams.ZoneAngle.Low > this.Tool.Results[i].GetPose().Rotation ||
                                this.Tool.Results[i].GetPose().Rotation > this.Tool.RunParams.ZoneAngle.High) continue;
                        }
                        resultValue = new PatternMatchingResult.PatternMatchingResultValue();
                        resultValue.X = this.Tool.Results[i].GetPose().TranslationX;
                        resultValue.Y = this.Tool.Results[i].GetPose().TranslationY;
                        resultValue.R = this.Tool.Results[i].GetPose().Rotation * 180.0 / Math.PI;
                        resultValue.Score = this.Tool.Results[i].Score;
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

                this.ConfirmDuplication(new Size(this.Tool.Pattern.TrainImage.Width, this.Tool.Pattern.TrainImage.Height));

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

                if (this.Tool.Pattern.TrainRegion == null)
                {
                    this.RotateMatchPoint(new Size(this.Tool.Pattern.TrainImage.Width, this.Tool.Pattern.TrainImage.Height));
                }
                else
                {
                    roi = this.Tool.Pattern.TrainRegion as CogRectangle;
                    this.RotateMatchPoint(new Size((int)roi.Width, (int)roi.Height));
                }
            }

            return ret;
        }
        #endregion

        #region PatternMatchingVisionTool Members
        protected override int OnLearn()
        {
            int ret = 0;
            VisionImage image = null;
            ICogImage cognexImage = null;
            VisionProCustomizedVisionImage cognexVisionImage = null;
            CogRectangle rectangle = null;
            CycleTimer timer = new CycleTimer();

            image = this.SubTools.OutputImage;

            if (image.CustomizedData == null)
            {
                if ((ret = VisionProCustomizedVisionImage.Create(ref image)) != 0) return ret;
            }

            cognexVisionImage = image.CustomizedData as VisionProCustomizedVisionImage;
            cognexImage = image.CustomizedData.Image as ICogImage;

            if (cognexVisionImage.Region != this.Parameter.Pattern.TrainRegion ||
                this.Parameter.Pattern.TrainImage != cognexImage)
            {
                if (cognexVisionImage.Region == null)
                {
                    this.Parameter.Pattern.TrainRegion = null;
                }
                else
                {
                    this.Parameter.Pattern.TrainRegion = cognexVisionImage.Region;
                    cognexVisionImage.Region = null;
                }

                this.Parameter.Pattern.TrainImage = cognexImage;

                rectangle = this.Parameter.Pattern.TrainRegion as CogRectangle;
                switch (this.Parameter.Type)
                {
                    case ResultType.LeftTop:
                        if (rectangle == null)
                        {
                            this.Parameter.Pattern.Origin.TranslationX = 0;
                            this.Parameter.Pattern.Origin.TranslationY = 0;
                        }
                        else
                        {
                            this.Parameter.Pattern.Origin.TranslationX = rectangle.X;
                            this.Parameter.Pattern.Origin.TranslationY = rectangle.Y;
                        }
                        break;
                    case ResultType.LeftBottom:
                        if (rectangle == null)
                        {
                            this.Parameter.Pattern.Origin.TranslationX = 0;
                            this.Parameter.Pattern.Origin.TranslationY = cognexImage.Height;
                        }
                        else
                        {
                            this.Parameter.Pattern.Origin.TranslationX = rectangle.X;
                            this.Parameter.Pattern.Origin.TranslationY = rectangle.Y + rectangle.Height;
                        }
                        break;
                    case ResultType.RightTop:
                        if (rectangle == null)
                        {
                            this.Parameter.Pattern.Origin.TranslationX = cognexImage.Width;
                            this.Parameter.Pattern.Origin.TranslationY = 0;
                        }
                        else
                        {
                            this.Parameter.Pattern.Origin.TranslationX = rectangle.Y + rectangle.Width;
                            this.Parameter.Pattern.Origin.TranslationY = rectangle.Y;
                        }

                        break;
                    case ResultType.RightBottom:
                        if (rectangle == null)
                        {
                            this.Parameter.Pattern.Origin.TranslationX = cognexImage.Width;
                            this.Parameter.Pattern.Origin.TranslationY = cognexImage.Height;
                        }
                        else
                        {
                            this.Parameter.Pattern.Origin.TranslationX = rectangle.Y + rectangle.Width;
                            this.Parameter.Pattern.Origin.TranslationY = rectangle.Y + rectangle.Height;
                        }
                        break;
                    case ResultType.Center:
                        if (rectangle == null)
                        {
                            this.Parameter.Pattern.Origin.TranslationX = cognexImage.Width / 2;
                            this.Parameter.Pattern.Origin.TranslationY = cognexImage.Height / 2;
                        }
                        else
                        {
                            this.Parameter.Pattern.Origin.TranslationX = rectangle.CenterX;
                            this.Parameter.Pattern.Origin.TranslationY = rectangle.CenterY;
                        }
                        break;
                }

                this.Parameter.HasChanged = true;
            }

            timer.Start();
            if (this.Parameter.HasChanged == true)
            {
                if (this.Parameter.AngleTolerance.Maximum == 0 && this.Parameter.AngleTolerance.Minimum == 0)
                    this.Parameter.RunParams.ZoneAngle.Configuration = CogPMAlignZoneConstants.Nominal;
                else
                {
                    this.Parameter.RunParams.ZoneAngle.Configuration = CogPMAlignZoneConstants.LowHigh;
                    this.Parameter.RunParams.ZoneAngle.Low = this.Parameter.AngleTolerance.Minimum / 180.0 * Math.PI;
                    this.Parameter.RunParams.ZoneAngle.High = this.Parameter.AngleTolerance.Maximum / 180.0 * Math.PI;
                }

                if (this.Parameter.MaxInstance == -1)
                    this.Parameter.RunParams.ApproximateNumberToFind = 1;
                else
                    this.Parameter.RunParams.ApproximateNumberToFind = this.Parameter.MaxInstance;

                this.Parameter.RunParams.AcceptThreshold = this.Parameter.MinScore;

                #region MarkImage 추가
                //MaskImage 추가
                if (this.Parameter.UseMaskImage == true)
                {
                    this.Parameter.Pattern.TrainImageMask = new CogImage8Grey(image.Header.Width, image.Header.Height);

                    //Train Image Mask Set 255
                    for (int i = 0; i < this.Parameter.Pattern.TrainImageMask.Height; i++)
                    {
                        for (int j = 0; j < this.Parameter.Pattern.TrainImageMask.Width; j++)
                        {
                            this.Parameter.Pattern.TrainImageMask.SetPixel(j, i, 255);
                        }
                    }

                    //Train Image Mask Set  0
                    for (int i = (int)this.Parameter.MaskRegion.Y; i < (int)(this.Parameter.MaskRegion.Y + this.Parameter.MaskRegion.Height); i++)
                    {
                        for (int j = (int)this.Parameter.MaskRegion.X; j < (int)(this.Parameter.MaskRegion.X + this.Parameter.MaskRegion.Width); j++)
                        {
                            this.Parameter.Pattern.TrainImageMask.SetPixel(j, i, 0);
                        }
                    }
                }
                //Mask Image 사용 안한다면 Mask Region 전부 255로 Set
                else
                {
                    this.Parameter.Pattern.TrainImageMask = new CogImage8Grey(image.Header.Width, image.Header.Height);

                    for (int i = 0; i < this.Parameter.Pattern.TrainImageMask.Height; i++)
                    {
                        for (int j = 0; j < this.Parameter.Pattern.TrainImageMask.Width; j++)
                        {
                            this.Parameter.Pattern.TrainImageMask.SetPixel(j, i, 255);
                        }
                    }
                }
                #endregion

                #region AutoEdgeThreshold Set
                if (this.Parameter.AutoEdgeThresholdEnabled == true)
                {
                    this.Parameter.Pattern.AutoEdgeThresholdEnabled = true;
                    this.Parameter.RunParams.AutoEdgeThresholdEnabled = true;
                }
                else
                {
                    this.Parameter.Pattern.AutoEdgeThresholdEnabled = false;
                    this.Parameter.RunParams.AutoEdgeThresholdEnabled = false;
                    this.Parameter.Pattern.EdgeThreshold = this.Parameter.EdgeThreshold;
                    this.Parameter.RunParams.EdgeThreshold = this.Parameter.EdgeThreshold;
                }
                #endregion

                #region Train Polarity Set
                if (this.Parameter.IgnorePolarity == true)
                {
                    this.Parameter.Pattern.IgnorePolarity = true;
                }
                else
                {
                    this.Parameter.Pattern.IgnorePolarity = false;
                }
                #endregion

                VisionToolLog.Write(this, string.Format("PatternMatching Algorithm : [{0}]", this.Parameter.RunParams.RunAlgorithm.ToString()));
                this.Tool.Pattern = this.Parameter.Pattern;
                this.Tool.Pattern.TrainImageMask = this.Parameter.Pattern.TrainImageMask;
                this.Tool.RunParams = this.Parameter.RunParams;
                this.Tool.Pattern.Train();

                this.Parameter.HasChanged = false;
            }
            timer.End();

            //if (this.m_LatestImage as VisionImage != null)
            //{
            //    if ((this.m_LatestImage as VisionImage).Tag != null)
            //        Log.Write("Cognex", new LogEntry(LogLevel.Highest, $"[{(this.m_LatestImage as VisionImage).Tag.ToString()}] Type = {this.Parameter.Type}, HasChanged = {this.Parameter.HasChanged}, HasChanged Interval = {timer.Latest}, UseMaskImage = {this.Parameter.UseMaskImage}, AutoEdgeThresholdEnabled = {this.Parameter.AutoEdgeThresholdEnabled}, IgnorePolarity = {this.Parameter.IgnorePolarity},MaxInstance = {this.Parameter.MaxInstance},MinScore = {this.Parameter.MinScore},AngleTolerance = {this.Parameter.AngleTolerance}"));
            //    else
            //        Log.Write("Cognex", new LogEntry(LogLevel.Highest, $"Type = {this.Parameter.Type}, HasChanged = {this.Parameter.HasChanged}, HasChanged Interval = {timer.Latest}, UseMaskImage = {this.Parameter.UseMaskImage}, AutoEdgeThresholdEnabled = {this.Parameter.AutoEdgeThresholdEnabled}, IgnorePolarity = {this.Parameter.IgnorePolarity}, MaxInstance = {this.Parameter.MaxInstance}, MinScore = {this.Parameter.MinScore}, AngleTolerance = {this.Parameter.AngleTolerance}"));
            //}
            //else
            //    Log.Write("Cognex", new LogEntry(LogLevel.Highest, $"Type = {this.Parameter.Type}, HasChanged = {this.Parameter.HasChanged}, HasChanged Interval = {timer.Latest}, UseMaskImage = {this.Parameter.UseMaskImage}, AutoEdgeThresholdEnabled = {this.Parameter.AutoEdgeThresholdEnabled}, IgnorePolarity = {this.Parameter.IgnorePolarity},MaxInstance = {this.Parameter.MaxInstance},MinScore = {this.Parameter.MinScore},AngleTolerance = {this.Parameter.AngleTolerance}"));

            return ret;
        }
        #endregion

        #region VisionTool Members
        public new VisionProPatternMatchingVisionToolParameter Parameter
        {
            get { return base.Parameter as VisionProPatternMatchingVisionToolParameter; }
            set { base.Parameter = value; }
        }

        protected override int OnRun()
        {
            int ret = 0;
            VisionImage image = this.InputImage;
            if(image != null)
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

        protected override int OnCheckedLicense()
        {
            int ret = 0;

            if (this.Parameter.RunParams.RunAlgorithm == CogPMAlignRunAlgorithmConstants.PatMax)
            {
                //if (CogLicense.IsEnabled(CogLicenseConstants.PatMax) != true) return ErrorManager.Register(string.Format("{0} is not supported", CogLicenseConstants.PatMax));
                if (CogLicense.IsEnabled(CogLicenseConstants.PatMax) != true) return -1;
            }
            else if (this.Parameter.RunParams.RunAlgorithm == CogPMAlignRunAlgorithmConstants.PatQuick)
            {
                //if (CogLicense.IsEnabled(CogLicenseConstants.PatQuick) != true) return ErrorManager.Register(string.Format("{0} is not supported", CogLicenseConstants.PatQuick));
                if (CogLicense.IsEnabled(CogLicenseConstants.PatQuick) != true) return -1;
            }
            else if (this.Parameter.RunParams.RunAlgorithm == CogPMAlignRunAlgorithmConstants.PatFlex)
            {
                //if (CogLicense.IsEnabled(CogLicenseConstants.PatFlex) != true) return ErrorManager.Register(string.Format("{0} is not supported", CogLicenseConstants.PatFlex));
                if (CogLicense.IsEnabled(CogLicenseConstants.PatFlex) != true) return -1;
            }

            return ret;
        }

        protected override int OnPrepare()
        {
            int ret = 0;
            // 필요 시 강제 재생성
            this.Tool.InputImage = new CogImage8Grey();
            this.Result = new PatternMatchingResult(this.Name);
            return ret;

            //int ret = 0;
            //this.Tool = new CogPMAlignTool();
            //this.Tool.InputImage = new CogImage8Grey();
            //this.Result = new PatternMatchingResult(this.Name);
            //return ret;
        }
        #endregion

        #region IDisposable Member
        protected override void OnDispose()
        {

        }
        #endregion
    }
    #endregion

    #region VisionProPatternMatchingVisionToolParameter
    [Serializable]
    public class VisionProPatternMatchingVisionToolParameter : PatternMatchingVisionToolParameter
    {
        #region Field
        private CogPMAlignPattern m_Pattern;
        private CogPMAlignRunParams m_RunParams;
        private VisionProPatternMatchingVisionTool.ResultType m_Type;
        #endregion

        #region Constructor
        public VisionProPatternMatchingVisionToolParameter() : base()
        {
            this.Pattern = new CogPMAlignPattern();
            this.RunParams = new CogPMAlignRunParams();
            this.Pattern.TrainAlgorithm = CogPMAlignTrainAlgorithmConstants.PatMax;
            this.RunParams.RunAlgorithm = CogPMAlignRunAlgorithmConstants.PatMax;
            this.RunParams.ScoreUsingClutter = false;
            this.RunParams.ZoneAngle.Configuration = CogPMAlignZoneConstants.LowHigh;
            this.RunParams.ZoneAngle.High = 0.78539816339744828;
            this.RunParams.ZoneAngle.Low = -0.78539816339744828;
            this.SortType = PatternMatchingVisionTool.SortType.X;
            this.Type = VisionProPatternMatchingVisionTool.ResultType.Center;
            this.ResultOverlayVisible = true;
            this.MaxInstance = -1;
            this.MinScore = -1;
        } 
        #endregion

        #region Property
        public CogPMAlignPattern Pattern
        {
            get { return this.m_Pattern; }
            set
            {
                if (this.m_Pattern == value) return;
                this.m_Pattern = value;
                this.HasChanged = true;
            }
        }
        public CogPMAlignRunParams RunParams
        {
            get { return this.m_RunParams; }
            set
            {
                if (this.m_RunParams == value) return;
                this.m_RunParams = value;
                this.HasChanged = true;
            }
        }
        public VisionProPatternMatchingVisionTool.ResultType Type
        {
            get { return this.m_Type; }
            set
            {
                if (this.m_Type 
                    == value) return;
                this.m_Type = value;
                this.HasChanged = true;
            }
        }
        #endregion
    }
    #endregion
}