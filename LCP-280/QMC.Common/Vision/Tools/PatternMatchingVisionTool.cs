/*
 * Purpose
 *      등록된 패턴을 찾는 Vision Tool을 정의한다.
 * 
 * Revision
 *      1. Created: 2019.04.05 by JUNG.CY
 *      2. Modified: 2020.05.04 by yjbaek
 *          - Duplication 확인
 *      

 *      
 */

using System;
using System.Drawing;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Xml.Serialization;


namespace QMC.Common.Vision.Tools
{
    #region PatternMatchingVisionTool
    [Serializable]
    public abstract class PatternMatchingVisionTool : VisionTool
    {
        #region Define
        [Serializable]
        public enum SortType
        {
            X,
            Y,
            Angle,
            Score,
        }

        [Serializable]
        public enum SortOrder
        {
            //
            // 요약:
            //     항목이 오름차순으로 정렬됩니다.
            Ascending,
            //
            // 요약:
            //     항목이 내림차순으로 정렬됩니다.
            Descending,
        }
        #endregion

        #region Field
        [NonSerialized]
        private bool m_IsLearn;
        #endregion

        #region Constructor
        public PatternMatchingVisionTool(string name) : base(name)
        {
            this.IsLearn = false;
            this.Result = new PatternMatchingResult(this.Name);
        }
        public PatternMatchingVisionTool() : this("") { }
        #endregion

        #region Property
        [Browsable(false)]
        public bool IsLearn
        {
            get { return this.m_IsLearn; }
            protected set { this.m_IsLearn = value; }
        }
        #endregion

        #region Method
        public int Learn()
        {
            int ret = 0;
            if ((ret = this.OnLearn()) != 0) return ret;
            this.IsLearn = true;
            return ret;
        }
        protected abstract int OnLearn();

        private PointD Rotate(PointD pointSource, int index)
        {
            PointD pointDestination = new PointD();
            double radian = this.Result.Values[index].R / 180 * Math.PI;

            pointDestination.X = Math.Cos(radian) * (pointSource.X - this.Result.Values[index].X) -
                                 Math.Sin(radian) * (pointSource.Y - this.Result.Values[index].Y) +
                                 this.Result.Values[index].X;

            pointDestination.Y = Math.Sin(radian) * (pointSource.X - this.Result.Values[index].X) +
                                 Math.Cos(radian) * (pointSource.Y - this.Result.Values[index].Y) +
                                 this.Result.Values[index].Y;

            return pointDestination;
        }

        protected int RotateMatchPoint(Size size)
        {
            int ret = 0;
            PointD rotatePoint;
            PolygonFrameVisionImageOverlay overlay = null;
            LineFrameVisionImageOverlay lineOverlay = null;

            double length = size.Width < size.Height ? size.Width / 3 : size.Height / 3;

            if (this.Result.ResultOverlays.Count != 0)
                this.Result.ResultOverlays.Clear();

            for (int i = 0; i < this.Result.Values.Count; i++)
            {
                overlay = new PolygonFrameVisionImageOverlay();
                rotatePoint = new PointD(this.Result.Values[i].X - size.Width / 2, this.Result.Values[i].Y - size.Height / 2);
                overlay.Points.Add(this.Rotate(rotatePoint, i));
                rotatePoint = new PointD(this.Result.Values[i].X + size.Width / 2, this.Result.Values[i].Y - size.Height / 2);
                overlay.Points.Add(this.Rotate(rotatePoint, i));
                rotatePoint = new PointD(this.Result.Values[i].X + size.Width / 2, this.Result.Values[i].Y + size.Height / 2);
                overlay.Points.Add(this.Rotate(rotatePoint, i));
                rotatePoint = new PointD(this.Result.Values[i].X - size.Width / 2, this.Result.Values[i].Y + size.Height / 2);
                overlay.Points.Add(this.Rotate(rotatePoint, i));
                overlay.Visible = this.Parameter.ResultOverlayVisible;

                this.Result.ResultOverlays.Add(overlay);

                lineOverlay = new LineFrameVisionImageOverlay();

                rotatePoint = new PointD(this.Result.Values[i].X - length / 2, this.Result.Values[i].Y);
                lineOverlay.StartLocation = this.Rotate(rotatePoint, i);
                rotatePoint = new PointD(this.Result.Values[i].X + length / 2, this.Result.Values[i].Y);
                lineOverlay.EndLocation = this.Rotate(rotatePoint, i);
                lineOverlay.Visible = this.Parameter.ResultOverlayVisible;

                this.Result.ResultOverlays.Add(lineOverlay);

                lineOverlay = new LineFrameVisionImageOverlay();

                rotatePoint = new PointD(this.Result.Values[i].X, this.Result.Values[i].Y - length / 2);
                lineOverlay.StartLocation = this.Rotate(rotatePoint, i);
                rotatePoint = new PointD(this.Result.Values[i].X, this.Result.Values[i].Y + length / 2);
                lineOverlay.EndLocation = this.Rotate(rotatePoint, i);
                lineOverlay.Visible = this.Parameter.ResultOverlayVisible;

                this.Result.ResultOverlays.Add(lineOverlay);
            }

            return ret;
        }

        protected int ConfirmDuplication(Size size)
        {
            int ret = 0;
            Quadrangle source = new Quadrangle();
            Quadrangle target = new Quadrangle();
            double distance = qGeometry.GetDistanceBetweenTwoPoints(new PointD(0, 0), new PointD(size.Width, size.Height));

            if (this.Parameter.DuplicateChecked == false) return ret;

            if (this.Result.Values.Count < 2) return ret;

            source.One = new PointD(this.Result.Values[0].X - size.Width, this.Result.Values[0].Y - size.Height);
            source.Two = new PointD(this.Result.Values[0].X + size.Width, this.Result.Values[0].Y - size.Height);
            source.Three = new PointD(this.Result.Values[0].X + size.Width, this.Result.Values[0].Y + size.Height);
            source.Four = new PointD(this.Result.Values[0].X - size.Width, this.Result.Values[0].Y + size.Height);

            for (int i = 1; i < this.Result.Values.Count; i++)
            {
                target = new Quadrangle();
                target.One = new PointD(this.Result.Values[i].X - size.Width, this.Result.Values[i].Y - size.Height);
                target.Two = new PointD(this.Result.Values[i].X + size.Width, this.Result.Values[i].Y - size.Height);
                target.Three = new PointD(this.Result.Values[i].X + size.Width, this.Result.Values[i].Y + size.Height);
                target.Four = new PointD(this.Result.Values[i].X - size.Width, this.Result.Values[i].Y + size.Height);

                try
                {
                    // modified 2020.06.14 by LIM.WT
                    // 4점이 모두 밖에 있어야 겹치지 않는 것이다.
                    if (source.Contains(target.One) == ShapeLocation.Outer &&
                        source.Contains(target.Two) == ShapeLocation.Outer &&
                        source.Contains(target.Three) == ShapeLocation.Outer &&
                        source.Contains(target.Four) == ShapeLocation.Outer) continue;

                    //if (source.Contains(target.One) == ShapeLocation.Outer) continue;
                    //if (source.Contains(target.Two) == ShapeLocation.Outer) continue;
                    //if (source.Contains(target.Three) == ShapeLocation.Outer) continue;
                    //if (source.Contains(target.Four) == ShapeLocation.Outer) continue;
                }
                finally
                {
                    if (ret < 0)
                    {
                        if (this.Result.Values[i].Score < this.Result.Values[i - 1].Score)
                            this.Result.Values.RemoveAt(i);
                        else
                            this.Result.Values.RemoveAt(i - 1);
                        i--;
                    }
                    else
                        source = target;
                }
            }

            return ret;
        }

        /// <summary>
        /// 지정된 Sorting 방법에 의하여 결과값을 Sorting한다.
        /// </summary>
        protected void SortingResult()
        {
            List<PatternMatchingResult.PatternMatchingResultValue> list = new List<PatternMatchingResult.PatternMatchingResultValue>(this.Result.Values);

            if (this.Parameter.SortType == SortType.X)
            {
                if (this.Parameter.SortOrder == SortOrder.Ascending)
                    list.Sort((a, b) => a.X > b.X ? 1 : -1);
                else if (this.Parameter.SortOrder == SortOrder.Descending)
                    list.Sort((a, b) => a.X < b.X ? 1 : -1);
            }
            else if (this.Parameter.SortType == SortType.Y)
            {
                if (this.Parameter.SortOrder == SortOrder.Ascending)
                    list.Sort((a, b) => a.Y > b.Y ? 1 : -1);
                else if (this.Parameter.SortOrder == SortOrder.Descending)
                    list.Sort((a, b) => a.Y < b.Y ? 1 : -1);
            }
            else if (this.Parameter.SortType == SortType.Angle)
            {
                if (this.Parameter.SortOrder == SortOrder.Ascending)
                    list.Sort((a, b) => a.R > b.R ? 1 : -1);
                else if (this.Parameter.SortOrder == SortOrder.Descending)
                    list.Sort((a, b) => a.R < b.R ? 1 : -1);
            }
            else if (this.Parameter.SortType == SortType.Score)
            {
                if (this.Parameter.SortOrder == SortOrder.Ascending)
                    list.Sort((a, b) => a.Score > b.Score ? 1 : -1);
                else if (this.Parameter.SortOrder == SortOrder.Descending)
                    list.Sort((a, b) => a.Score < b.Score ? 1 : -1);
            }


            this.Result.Values = new PatternMatchingResult.PatternMatchingResultValueCollection(list);
        }
        #endregion

        #region VisionTool Members
        public new PatternMatchingVisionToolParameter Parameter
        {
            get { return base.Parameter as PatternMatchingVisionToolParameter; }
            set { base.Parameter = value; }
        }

        [Browsable(false)]
        public new PatternMatchingResult Result
        {
            get { return base.Result as PatternMatchingResult; }
            protected set { base.Result = value; }
        }
        #endregion
    }
    #endregion

    #region PatternMatchingVisionToolParameter
    [Serializable]
    public class PatternMatchingVisionToolParameter : VisionToolParameter
    {
        #region Field
        private double m_MinScore;
        private int m_MaxInstance;
        private bool m_ResultOverlayVisible;
        private RangeD m_AngleTolerance;
        private PatternMatchingVisionTool.SortType m_SortType;
        private PatternMatchingVisionTool.SortOrder m_SortOrder;
        private bool m_DuplicateChecked;
        private bool m_UseMaskImage;
        private RectangleD m_MaskRegion;
        private bool m_AutoEdgeThresholdEnabled;
        private double m_EdgeThreshold;
        private bool m_IgnorePolarity;
        #endregion

        #region Constructor
        public PatternMatchingVisionToolParameter()
        {
            this.MaxInstance = -1;
            this.MinScore = -1;
            this.AngleTolerance = new RangeD(0, 0);
            this.ResultOverlayVisible = false;

            this.SortType = PatternMatchingVisionTool.SortType.Score;
            this.SortOrder = PatternMatchingVisionTool.SortOrder.Ascending;

            this.DuplicateChecked = false;
            this.UseMaskImage = false;
            this.MaskRegion = new RectangleD(0, 0, 0, 0);

            this.AutoEdgeThresholdEnabled = true;
            this.EdgeThreshold = 10.0;
            this.IgnorePolarity = false;
        }
        #endregion

        #region Property
        public double MinScore
        {
            get { return this.m_MinScore; }
            set
            {
                if (this.m_MinScore == value) return;
                this.m_MinScore = value;
                this.HasChanged = true;
            }
        }

        public int MaxInstance
        {
            get { return this.m_MaxInstance; }
            set
            {
                if (this.m_MaxInstance == value) return;
                this.m_MaxInstance = value;
                this.HasChanged = true;
            }
        }

        public RangeD AngleTolerance
        {
            get { return this.m_AngleTolerance; }
            set
            {
                if (this.m_AngleTolerance == value) return;
                this.m_AngleTolerance = value;
                this.HasChanged = true;
            }
        }

        public bool ResultOverlayVisible
        {
            get { return this.m_ResultOverlayVisible; }
            set
            {
                if (this.m_ResultOverlayVisible == value) return;
                this.m_ResultOverlayVisible = value;
                this.HasChanged = true;
            }
        }

        /// <summary>
        /// PatternMatchingResult의 Sorting 방법을 가져오거나 설정한다.
        /// </summary>
        public PatternMatchingVisionTool.SortType SortType
        {
            get { return this.m_SortType; }
            set
            {
                if (this.m_SortType == value) return;
                this.m_SortType = value;
                this.HasChanged = true;
            }
        }

        /// <summary>
        /// PatternMatchingResult의 Sorting 방향을 가져오거나 설정한다.
        /// </summary>
        public PatternMatchingVisionTool.SortOrder SortOrder
        {
            get { return this.m_SortOrder; }
            set { this.m_SortOrder = value; }
        }

        public bool DuplicateChecked
        {
            get { return this.m_DuplicateChecked; }
            set { this.m_DuplicateChecked = value; }
        }

        public bool UseMaskImage
        {
            get { return this.m_UseMaskImage; }
            set
            {
                if (this.m_UseMaskImage == value) return;
                this.m_UseMaskImage = value;
                this.HasChanged = true;
            }
        }

        public RectangleD MaskRegion
        {
            get { return this.m_MaskRegion; }
            set
            {
                if (this.m_MaskRegion == value) return;
                this.m_MaskRegion = value;
                this.HasChanged = true;
            }
        }

        public bool AutoEdgeThresholdEnabled
        {
            get { return this.m_AutoEdgeThresholdEnabled; }
            set
            {
                if (this.m_AutoEdgeThresholdEnabled == value) return;
                this.m_AutoEdgeThresholdEnabled = value;
                this.HasChanged = true;
            }
        }

        public double EdgeThreshold
        {
            get { return this.m_EdgeThreshold; }
            set
            {
                if (this.m_EdgeThreshold == value) return;
                this.m_EdgeThreshold = value;
                this.HasChanged = true;
            }
        }

        public bool IgnorePolarity
        {
            get { return this.m_IgnorePolarity; }
            set
            {
                if (this.m_IgnorePolarity == value) return;
                this.m_IgnorePolarity = value;
                this.HasChanged = true;
            }
        }
        #endregion
    }
    #endregion

    #region PatternMatchingResult
    [Serializable]
    public class PatternMatchingResult : VisionResult
    {
        #region Define
        [Serializable]
        public struct PatternMatchingResultValue
        {
            public double X;
            public double Y;
            public double R;
            public double Score;
        }

        [Serializable]
        public class PatternMatchingResultValueCollection : Collection<PatternMatchingResultValue>
        {
            #region Constructor
            public PatternMatchingResultValueCollection(List<PatternMatchingResultValue> list) : base(list) { }
            public PatternMatchingResultValueCollection() : base() { }

            #endregion
        }

        [Serializable]
        public class ResultOverlayCollection : Collection<FrameVisionImageOverlay>
        {

        }
        #endregion

        #region Field
        private PatternMatchingResultValueCollection m_Values;
        private ResultOverlayCollection m_ResultOverlays;
        #endregion

        #region Constructor
        public PatternMatchingResult(string owner) : base(owner)
        {
            this.Values = new PatternMatchingResultValueCollection();
            this.ResultOverlays = new ResultOverlayCollection();
        }
        public PatternMatchingResult()
        {
            this.Values = new PatternMatchingResultValueCollection();
            this.ResultOverlays = new ResultOverlayCollection();
        }
        #endregion

        #region Property
        /// <summary>
        /// Vision Processing의 결과에 대한 보정값을 가져오거나 설정한다.
        /// </summary>
        public PatternMatchingResultValueCollection Values
        {
            get { return this.m_Values; }
            set { this.m_Values = value; }
        }

        public ResultOverlayCollection ResultOverlays
        {
            get { return this.m_ResultOverlays; }
            set { this.m_ResultOverlays = value; }
        }

        /// <summary>
        /// Pattern Matching을 통해 찾은 결과가 있는지 가져온다.
        /// </summary>
        public bool Found
        {
            get
            {
                if (this.Values.Count != 0)
                    return true;
                else
                    return false;
            }
        }
        #endregion


        #region Method
        /// <summary>
        /// Searh된 결과를 CenterPosition 기준으로 변환한 후 Sorting하여 반환한다.
        /// Sorting은 CenterPosition에서부터 가까운순서.
        /// </summary>
        /// <param name="centerPosition"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public int GetTransformedPolar(PointD centerPosition, out PatternMatchingResult.PatternMatchingResultValueCollection values)
        {
            int ret = 0;
            List<PatternMatchingResultValue> list = null;

            list = new List<PatternMatchingResultValue>(this.Values);

            list.Sort((a, b) => Math.Sqrt(Math.Pow(centerPosition.X - a.X, 2) + Math.Pow(centerPosition.Y - a.Y, 0)) > Math.Sqrt(Math.Pow(centerPosition.X - b.X, 2) + Math.Pow(centerPosition.Y - b.Y, 0)) ? 1 : -1);

            values = new PatternMatchingResultValueCollection(list);

            return ret;
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();

            for (int i = 0; i < this.Values.Count; i++)
            {
                builder.AppendLine(string.Format("X : {0}, Y: {1}, Angle : {2}, Score : {3}", this.Values[i].X, this.Values[i].Y, this.Values[i].R, this.Values[i].Score));
            }

            return builder.ToString();
        }
        #endregion
    }
    #endregion
}
