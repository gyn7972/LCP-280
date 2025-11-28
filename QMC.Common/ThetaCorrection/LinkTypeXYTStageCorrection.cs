using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QMC.Common.Vision;
namespace QMC.Common.ThetaCorrection
{
    public class LinkTypeXYTStageCorrection
    {
        List<CorrectionPointXYTStage> correctionPoints = new List<CorrectionPointXYTStage>();
        double dZeroCommandTheta = 0.0;
        CorrectionPointXYTStage ZerocorrectionPointXYTStage = null;
        public List<CorrectionPointXYTStage> CorrectionPoints
        {
            get { return correctionPoints; }
            set { correctionPoints = value; }
        }
        public void SetZeroCommandTheta(double zeroCommandTheta)
        {
            this.dZeroCommandTheta = zeroCommandTheta;
            var v = CorrectionPoints.OrderBy(p => Math.Abs(p.CommandTheta - zeroCommandTheta));
            CorrectionPointXYTStage Zero = v.FirstOrDefault();
            ZerocorrectionPointXYTStage = Zero;
            foreach (var point in correctionPoints)
            {
                point.RelativeTheta = point.ActualTheta - Zero.ActualTheta;
            }
        }
        public CorrectionPointXYTStage GetCorrectionPointXYTStage(double theta)
        {
            CorrectionPointXYTStage result = null;
            var v = correctionPoints.OrderBy(p => Math.Abs(p.RelativeTheta - theta));
            return v.FirstOrDefault();
        }
        public void AddCorrectionPoint(CorrectionPointXYTStage point)
        {
            correctionPoints.Add(point);
        }
        public void AddCorrectionPoint(List<XyCoordinate> points, double commandTheta)
        {
            CorrectionPointXYTStage point = new CorrectionPointXYTStage();
            point.PointDs = points;
            point.CommandTheta = commandTheta;
            point.CalcActualTheta();
            correctionPoints.Add(point);
        }

        public void GetCorrectionPoint(double theta, XyCoordinate currentpt, XyCoordinate visionOffset, out XyCoordinate pointD, out double commandTheta)
        {
            pointD = new XyCoordinate();
            var correctionPoint = GetCorrectionPointXYTStage(theta);
            if (correctionPoint != null && correctionPoint.PointDs.Count > 0)
            {
                pointD = correctionPoint.PointDs[0];
                commandTheta = correctionPoint.CommandTheta;
            }
            else
            {
                commandTheta = ZerocorrectionPointXYTStage.CommandTheta;
            }

            PerspectiveProjection pp = new PerspectiveProjection();
            XyCoordinateCollection source = new XyCoordinateCollection();
            XyCoordinateCollection dest = new XyCoordinateCollection();
            for (int i = 0; i < correctionPoint.PointDs.Count; i++)
            {               
                
                var cp = correctionPoint.PointDs[i];
                dest.Add(new XyCoordinate(cp.X, cp.Y));                  
                
            }
            for (int i = 0; i < ZerocorrectionPointXYTStage.PointDs.Count; i++)
            {
                var cp = ZerocorrectionPointXYTStage.PointDs[i];

                source.Add(new XyCoordinate(cp.X,cp.Y));
                
            }
            QMCMatrix qMCMatrix = pp.projection_matrix(source, dest);

            pointD = pp.GetPerspectiveProjectionPoint(currentpt, qMCMatrix);
            pointD = pointD + visionOffset;
        }
    }
    public class CorrectionPointXYTStage
    {
        public List<XyCoordinate> PointDs { get; set; } = new List<XyCoordinate>();
        public double CommandTheta { get; set; }
        public double ActualTheta { get; set; }
        public double RelativeTheta { get; set; } = 0;

        public void CalcActualTheta()
        {
            for (int i = 0; i < PointDs.Count; i++)
            {
                var p = PointDs[i];
                int nNextIndex = (i + 1) % PointDs.Count;
                var pt = PointDs[nNextIndex];
                var angle = Math.Atan2(pt.Y - p.Y, pt.X - p.X) * 180.0 / Math.PI;
                ActualTheta += angle;
            }
        }
    }
}
