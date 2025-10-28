using QMC.Common.Cameras;
using QMC.Common.Vision;
using QMC.Common.Vision.Cognex;
using QMC.Common.Vision.Tools;
using System;
using System.Drawing;
using System.Linq;

namespace QMC.Common.VisionPart
{
    [Serializable]
    public abstract class PatternMatchingVisionPart : VisionPart
    {
        protected VisionProPatternMatchingVisionTool m_PatternMatchingTool;
        protected VisionProRoiVisionTool m_RoiTrain;
        protected VisionProRoiVisionTool m_RoiInspect;

        public Camera Camera { set; get; }

        public VisionImage TestImage { set; get; }
        public VisionImage TrainImage { set; get; }

        public bool Simulated { set; get; }
        
        public double OffsetTolerrance { set; get; }

        public PatternMatchingVisionPart(string strName) : base(strName)
        {
            m_PatternMatchingTool = new VisionProPatternMatchingVisionTool();
            m_RoiTrain = new VisionProRoiVisionTool();
            m_RoiInspect = new VisionProRoiVisionTool();

            TestImage = new VisionImage();
            TrainImage = new VisionImage();

            OffsetTolerrance = 0;
        }

        public override int Create()
        {
            int ret = 0;

            if (m_PatternMatchingTool == null)
                m_PatternMatchingTool = new VisionProPatternMatchingVisionTool();
            if (m_RoiTrain == null)
                m_RoiTrain = new VisionProRoiVisionTool();
            if (m_RoiInspect == null)
                m_RoiInspect = new VisionProRoiVisionTool();

            //m_PatternMatchingTool.Prepare();
            m_PatternMatchingTool.SubTools.Clear();
            m_PatternMatchingTool.SubTools.Add(m_RoiTrain);

            return ret;
        }

        public override void Close()
        {
            base.Close();
        }

        public PatternMatchingResult GetResult()
        {
            return m_PatternMatchingTool.Result;
        }
        public RoiVisionTool GetTrainRoi()
        {
            return m_RoiTrain;
        }
        public RoiVisionTool GetInspectRoi()
        {
            return m_RoiInspect;
        }

        public abstract void SetParameter(double dTolerance, int nMaxInstance, double dMinScore, bool bDuplicateChecked, bool bUseMaskImage);
        public abstract void GetParameter(out PatternMatchingParameters parameter);


        public int OnTrain(Point startPoint, Point endPoint, PatternMatchingParameters parameter, IlluminationDataSet illuminationData)
        {
            int ret = 0;
            VisionImage image;
            if (Simulated)
            {
                image = TestImage;
            }
            else
            {
                if ((ret = OnSetIllumination(illuminationData, true)) != 0) return ret;
                if ((ret = Camera.GrabSync(Purpose.Processing, out image)) != 0) return ret;
            }

            m_RoiTrain.Parameter.StartLocation = startPoint;
            m_RoiTrain.Parameter.EndLocation = endPoint;

            TrainImage = image.CutVisionImage(m_RoiTrain.Parameter.StartLocation, m_RoiTrain.Parameter.EndLocation);

            if (parameter != null)
            {
                parameter.TrainImage = TrainImage;
            }

            return ret;
        }

        public int OnSearch(Point startRoiPoint, Point endRoiPoint, PatternMatchingParameters parameter, IlluminationDataSet illuminationData)
        {
            int ret = 0;

            VisionImage image;
            
            if (Simulated)
            {
                image = TestImage;
            }
            else
            {
                if ((ret = OnSetIllumination(illuminationData, true)) != 0) return ret;
                if ((ret = Camera.GrabSync(Purpose.Processing, out image)) != 0) return ret;
                //if ((ret = Camera.Expose()) != 0) return ret;
                //if ((ret = Camera.Readout(out image)) != 0) return ret;
            }
            PatternMatchingParameters OffsetParameter = parameter.Clone();
            OffsetParameter.SetOffsetTolerrance(OffsetTolerrance);
            ret = OnSearch(startRoiPoint, endRoiPoint, OffsetParameter, illuminationData, image);

            return ret;
        }
        
        public int OnSearch(Point startRoiPoint, Point endRoiPoint, PatternMatchingParameters parameter, IlluminationDataSet illuminationData, VisionImage visionImage)
        {
            int ret = 0;


            if (Simulated)
            {
                visionImage = TestImage;
            }
            else
            {
                
            }

            m_PatternMatchingTool.Parameter.AngleTolerance = new RangeD(parameter.MinTolerance, parameter.MaxTolerance);
            m_PatternMatchingTool.Parameter.DuplicateChecked = parameter.DuplicateChecked;
            m_PatternMatchingTool.Parameter.MaxInstance = parameter.MaxInstance;
            m_PatternMatchingTool.Parameter.MinScore = parameter.MinScore;
            m_PatternMatchingTool.Parameter.MaskRegion = parameter.MaskRegion;
            m_PatternMatchingTool.Parameter.UseMaskImage = parameter.UseMaskImage;
            TrainImage = parameter.TrainImage;

            m_RoiInspect.Parameter.StartLocation = startRoiPoint;
            m_RoiInspect.Parameter.EndLocation = endRoiPoint;

            m_PatternMatchingTool.SubTools.InputImage = TrainImage;

            m_RoiTrain.Parameter.IsFull = true;
            m_RoiInspect.InputImage = visionImage;
            m_RoiInspect.Parameter.IsFull = false;

            if ((ret = m_RoiInspect.Run()) != 0) return ret;
            m_PatternMatchingTool.InputImage = m_RoiInspect.OutputImage;

            if ((ret = m_PatternMatchingTool.Run()) != 0) return ret;

            if (m_PatternMatchingTool.Result.Values.Count <= 0)
            {
                Log.Write(this.Name, "VisionCalibrator Fail");
                return ret;
            }

            return ret;
        }
    }
}
