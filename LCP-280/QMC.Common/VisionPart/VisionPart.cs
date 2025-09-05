using QMC.Common.Cameras;
using QMC.Common.Component;
using QMC.Common.Vision.Tools;
using QMC.Common.Vision;
using System;
using System.Drawing;

namespace QMC.Common.VisionPart
{
    public delegate void UpdatePatternMatchingResult(PatternMatchingResult result);
    [Serializable]
    public class VisionPart : BaseComponent
    {
        
        public Camera Camera { set; get; }
        public Illuminator Illuminator { set; get; }

        public bool UseTrainRoi { set; get; }
        public bool UseInspectRoi { set; get; }

        public event UpdatePatternMatchingResult UpdateResult;
        
        public VisionPart(string strName) : base(strName)
        {
            UseTrainRoi = false;
            UseInspectRoi = false;
        }

        public override int Create()
        {
            int ret = 0;

            return ret;
        }

        public override void Close()
        {
            base.Close();
        }

        protected int OnSetIllumination(IlluminationDataSet illuminationDataSet, bool bOn)
        {
            int ret = 0;
            if (Illuminator == null)
            {
                return -1;
            }

            foreach (IlluminationChannel illumination in illuminationDataSet.Values)
            {
                if (bOn)
                {
                    if ((ret = Illuminator.TurnOnOff(true, illumination.Channel)) != 0) return ret;
                    if ((ret = Illuminator.SetVolume(illumination.Value, illumination.Channel)) != 0) return ret;
                }
                else
                {
                    if ((ret = Illuminator.TurnOnOff(false, illumination.Channel)) != 0) return ret;
                }

            }

            return ret;
        }

        public void FireUpdateResult(PatternMatchingResult result)
        {
            UpdateResult?.BeginInvoke(result, null, null);
        }

        public virtual Point GetTrainStartPoint()
        {
            Point point = new Point();

            return point;
        }

        public virtual Point GetTrainEndPoint()
        {
            Point point = new Point();

            return point;
        }
        public virtual Point GetInspectStartPoint()
        {
            Point point = new Point();

            return point;
        }

        public virtual Point GetInspectEndPoint()
        {
            Point point = new Point();

            return point;
        }

        public virtual void SetTrainStartPoint(Point point)
        {

        }

        public virtual void SetTrainEndPoint(Point point)
        {

        }
        public virtual void SetInspectStartPoint(Point point)
        {

        }

        public virtual void SetInspectEndPoint(Point point)
        {

        }

        public virtual RoiVisionTool GetTrainRoi()
        {
            return null;
        }

        public virtual RoiVisionTool GetInspectRoi()
        {
            return null;
        }

    }
}
