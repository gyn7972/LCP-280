using QMC.Common.Vision;
using QMC.Common.VisionPart;
using QMC.Common.Vision.Cognex;
using QMC.Common.Vision.Tools;
using System.Drawing;
using QMC.Common.Cameras;
using System;

namespace QMC.Common.VisionPart
{ 

    public class MultiPatternMatchingVisionPart : VisionPart
    {
        #region Field
        //protected VisionProMultiPatternMatchingVisionTool m_MultiPatternMatchingTool;
        protected VisionProPatternMatchingVisionTool m_MultiPatternMatchingTool;
        protected VisionProRoiVisionTool m_RoiTrain;
        protected VisionProRoiVisionTool m_RoiInspect;
        #endregion

        #region Property
        public VisionImage TestImage { set; get; }

        public VisionImage TrainImage { set; get; }

        public bool Simulated { set; get; }

        public double OffsetTolerrance { get; set; }
        #endregion

        #region Constructor
        public MultiPatternMatchingVisionPart(string strName) : base(strName)
        {
            //m_MultiPatternMatchingTool = new VisionProMultiPatternMatchingVisionTool();
            m_MultiPatternMatchingTool = new VisionProPatternMatchingVisionTool();
            m_RoiTrain = new VisionProRoiVisionTool();
            m_RoiInspect = new VisionProRoiVisionTool();

            TestImage = new VisionImage();
            TrainImage = new VisionImage();

            OffsetTolerrance = 0;

            // ROI 사용 플래그 활성화 (저장/로드 및 UI 로직에서 사용)
            UseTrainRoi = true;
            UseInspectRoi = true;
        }
        #endregion

        #region VisionPart Members
        public override int Create()
        {
            int ret = 0;

            //if (m_MultiPatternMatchingTool == null)
            //    m_MultiPatternMatchingTool = new VisionProMultiPatternMatchingVisionTool();
            if (m_MultiPatternMatchingTool == null)
                m_MultiPatternMatchingTool = new VisionProPatternMatchingVisionTool();
            if (m_RoiTrain == null)
                m_RoiTrain = new VisionProRoiVisionTool();
            if (m_RoiInspect == null)
                m_RoiInspect = new VisionProRoiVisionTool();

            //m_PatternMatchingTool.Prepare();
            m_MultiPatternMatchingTool.SubTools.Clear();
            m_MultiPatternMatchingTool.SubTools.Add(m_RoiTrain);

            return ret;
        }

        public override void Close()
        {
            base.Close();
        }
        #endregion

        #region ROI Override (저장/로드용 좌표 접근자 구현)
        public override Point GetTrainStartPoint()
        {
            return m_RoiTrain?.Parameter?.StartLocation ?? Point.Empty;
        }
        public override Point GetTrainEndPoint()
        {
            return m_RoiTrain?.Parameter?.EndLocation ?? Point.Empty;
        }
        public override Point GetInspectStartPoint()
        {
            return m_RoiInspect?.Parameter?.StartLocation ?? Point.Empty;
        }
        public override Point GetInspectEndPoint()
        {
            return m_RoiInspect?.Parameter?.EndLocation ?? Point.Empty;
        }

        public override void SetTrainStartPoint(Point point)
        {
            if (m_RoiTrain == null) m_RoiTrain = new VisionProRoiVisionTool();
            m_RoiTrain.Parameter.StartLocation = point;
        }
        public override void SetTrainEndPoint(Point point)
        {
            if (m_RoiTrain == null) m_RoiTrain = new VisionProRoiVisionTool();
            m_RoiTrain.Parameter.EndLocation = point;
        }
        public override void SetInspectStartPoint(Point point)
        {
            if (m_RoiInspect == null) m_RoiInspect = new VisionProRoiVisionTool();
            m_RoiInspect.Parameter.StartLocation = point;
        }
        public override void SetInspectEndPoint(Point point)
        {
            if (m_RoiInspect == null) m_RoiInspect = new VisionProRoiVisionTool();
            m_RoiInspect.Parameter.EndLocation = point;
        }
        #endregion

        #region Method
        public virtual MultiPatternMatchingParameters GetPatternMatchingParameters()
        {
            return null;
        }

        public virtual void SetPatternMatchingParameters(MultiPatternMatchingParameters parameters)
        {

        }


        public virtual void ChangeTrainImageList(MultiPatternMatchingParameters parameter)
        {
            //m_MultiPatternMatchingTool.Parameter.TrainImages = parameter.TrainImages;
            //m_MultiPatternMatchingTool.LearnTrainImage();

        }

        public PatternMatchingResult GetResult()
        {
            return m_MultiPatternMatchingTool.Result;
        }

        public override RoiVisionTool GetTrainRoi()
        {
            return m_RoiTrain;
        }

        public override RoiVisionTool GetInspectRoi()
        {
            return m_RoiInspect;
        }
        public int OnTrain(Point startPoint, Point endPoint, MultiPatternMatchingParameters parameter, IlluminationDataSet illuminationData, int nIndex)
        {
            int ret = 0;
            VisionImage image;
            // 이미 외부에서 Train 이미지가 세팅되어 있고 Simulated 모드라면 덮어쓰지 않고 그대로 사용
            if (Simulated && parameter != null && nIndex >= 0 && nIndex < parameter.TrainImages.Count)
            {
                var existing = parameter.TrainImages[nIndex];
                if (existing != null && existing.RawData != null && existing.Header != null && existing.Header.Width > 0 && existing.Header.Height > 0)
                {
                    TrainImage = existing; // 내부 참조만 갱신
                    return 0; // 기존 이미지를 유지 (덮어쓰기 방지)
                }
            }

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
                for (int i = 0; i < parameter.TrainImages.Count; i++)
                {
                    if (parameter.TrainImages[i].RawData == null)
                    {
                        parameter.TrainImages.RemoveAt(i);
                        i--;
                    }
                }
                if (nIndex < 0)
                    parameter.TrainImages.Add(TrainImage);
                else if (nIndex < parameter.TrainImages.Count)
                    parameter.TrainImages[nIndex] = TrainImage;
            }

            return ret;
        }
        public int OnTrain(Point startPoint, Point endPoint, MultiPatternMatchingParameters parameter, IlluminationDataSet illuminationData)
        {
            int ret = 0;

            ret = OnTrain(startPoint, endPoint, parameter, illuminationData, -1);

            return ret;
        }
        public int OnTrain(int selectIndex, Point startPoint, Point endPoint, MultiPatternMatchingParameters parameter, IlluminationDataSet illuminationData)
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
                //for (int i = 0; i < parameter.TrainImages.Count; i++)
                //{
                //    if (parameter.TrainImages[i].RawData == null)
                //    {
                //        parameter.TrainImages.RemoveAt(i);
                //        i--;
                //    }
                //}

                parameter.TrainImages[selectIndex] = TrainImage;

                //m_MultiPatternMatchingTool.Parameter.TrainImages = parameter.TrainImages;
                //if ((ret = m_MultiPatternMatchingTool.LearnTrainImage()) != 0)
                //{
                //    return ret;
                //}
            }

            return ret;
        }
        public int OnSearch(Point startRoiPoint, Point endRoiPoint, MultiPatternMatchingParameters parameter, IlluminationDataSet illuminationData)
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
            MultiPatternMatchingParameters OffsetParameter = parameter.Clone();
            OffsetParameter.SetOffsetTolerrance(OffsetTolerrance);
            ret = OnSearch(startRoiPoint, endRoiPoint, OffsetParameter, illuminationData, image);

            return ret;
        }

        public int OnSearch(Point startRoiPoint, Point endRoiPoint, MultiPatternMatchingParameters parameter, IlluminationDataSet illuminationData, VisionImage visionImage)
        {
            int ret = 0;

            if (Simulated)
            {
                visionImage = TestImage;
            }

            m_MultiPatternMatchingTool.Parameter.AngleTolerance = new RangeD(parameter.MinTolerance, parameter.MaxTolerance);
            m_MultiPatternMatchingTool.Parameter.DuplicateChecked = parameter.DuplicateChecked;
            m_MultiPatternMatchingTool.Parameter.MaxInstance = parameter.MaxInstance;
            m_MultiPatternMatchingTool.Parameter.MinScore = parameter.MinScore;
            m_MultiPatternMatchingTool.Parameter.MaskRegion = parameter.MaskRegion;
            m_MultiPatternMatchingTool.Parameter.UseMaskImage = parameter.UseMaskImage;

            m_RoiInspect.Parameter.StartLocation = startRoiPoint;
            m_RoiInspect.Parameter.EndLocation = endRoiPoint;

            m_MultiPatternMatchingTool.SubTools.InputImage = parameter.TrainImages[0];

            m_RoiTrain.Parameter.IsFull = true;
            m_RoiInspect.InputImage = visionImage;
            m_RoiInspect.Parameter.IsFull = false;

            if ((ret = m_RoiInspect.Run()) != 0) return ret;
            m_MultiPatternMatchingTool.InputImage = m_RoiInspect.OutputImage;

            if ((ret = m_MultiPatternMatchingTool.Run()) != 0) return ret;

            if (m_MultiPatternMatchingTool.Result.Values.Count <= 0)
            {
                Log.Write(this.Name, "MultiPatternMatching is Failed");
                return ret;
            }

            // OnSearch 내부 - ROI offset 처리 (단 1회만, relative 로 판단될 때)
            if ((startRoiPoint.X != 0 || startRoiPoint.Y != 0) && m_MultiPatternMatchingTool.Result != null)
            {
                try
                {
                    var res = m_MultiPatternMatchingTool.Result;
                    var values = res.Values;
                    if (values != null && values.Count > 0)
                    {
                        int roiW = Math.Max(1, endRoiPoint.X - startRoiPoint.X + 1);
                        int roiH = Math.Max(1, endRoiPoint.Y - startRoiPoint.Y + 1);
                        double minX = double.MaxValue, maxX = double.MinValue, minY = double.MaxValue, maxY = double.MinValue;
                        for (int i = 0; i < values.Count; i++)
                        {
                            var v = values[i];
                            if (v.X < minX) minX = v.X; if (v.X > maxX) maxX = v.X;
                            if (v.Y < minY) minY = v.Y; if (v.Y > maxY) maxY = v.Y;
                        }
                        bool looksRelative = minX >= -0.5 && minY >= -0.5 && maxX <= roiW + 0.5 && maxY <= roiH + 0.5;
                        if (looksRelative)
                        {
                            // Values → 절대좌표
                            for (int i = 0; i < values.Count; i++)
                            {
                                var v = values[i];
                                v.X += startRoiPoint.X;
                                v.Y += startRoiPoint.Y;
                                values[i] = v;
                            }
                            // Overlays (Frame 계열만 처리)
                            if (res.ResultOverlays != null)
                            {
                                foreach (var ov in res.ResultOverlays)
                                {
                                    try
                                    {
                                        if (ov is FrameVisionImageOverlay f)
                                        {
                                            f.StartLocation = new Point(f.StartLocation.X + startRoiPoint.X, f.StartLocation.Y + startRoiPoint.Y);
                                            f.EndLocation = new Point(f.EndLocation.X + startRoiPoint.X, f.EndLocation.Y + startRoiPoint.Y);
                                            f.CenterLocation = new Point(f.CenterLocation.X + startRoiPoint.X, f.CenterLocation.Y + startRoiPoint.Y);
                                        }
                                    }
                                    catch { }
                                }
                            }
                        }
                    }
                }
                catch { }
            }

            return ret;
        }

        public virtual void SetParameter(double dTolerance, int nMaxInstance, double dMinScore, bool bDuplicateChecked, bool bUseMaskImage)
        {

        }
        public virtual void GetParameter(out PatternMatchingParameters parameter)
        {
            parameter = null;
        }

        

        public virtual IlluminationDataSet GetIlluminationDataSet()
        {
            return null;
        }
        #endregion
    }
}
