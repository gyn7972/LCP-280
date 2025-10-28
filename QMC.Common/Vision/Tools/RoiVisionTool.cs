/*
 * Purpose
 *      관심 영역(ROI : Region of interest)을 설정하는 Vision Tool을 정의한다.
 *      
 * Revision
 *      1. Created: 2018.01.09 by JUNG.CY
 *      
 */
using System;
using System.Drawing;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace QMC.Common.Vision.Tools
{
    #region RoiVisionTool
    [Serializable]
    public abstract class RoiVisionTool : VisionTool
    {
        #region Constructor
        public RoiVisionTool(string name) : base(name)
        {
            this.Result = new RoiResult(this.Name);
            this.Parameter = new RoiVisionToolParameter();
        }
        public RoiVisionTool() : this("") { }
        #endregion

        #region Method
        public int ContainResult(Point startPosition, Point endPosition, ref bool result)
        {
            int ret = 0;

            if (startPosition.X < this.Parameter.StartLocation.X)
            {
                result = false;
                return ret;
            }

            if (startPosition.Y < this.Parameter.StartLocation.Y)
            {
                result = false;
                return ret;
            }

            if (this.Parameter.EndLocation.X < endPosition.X)
            {
                result = false;
                return ret;
            }

            if (this.Parameter.EndLocation.Y < endPosition.Y)
            {
                result = false;
                return ret;
            }
            result = true;
            return ret;
        }

        public int ContainResult(Point center, Size size, ref bool result)
        {
            int ret = 0;

            if (center.X - size.Width / 2 < this.Parameter.StartLocation.X)
            {
                result = false;
                return ret;
            }

            if (center.Y - size.Height / 2 < this.Parameter.StartLocation.Y)
            {
                result = false;
                return ret;
            }

            if (this.Parameter.EndLocation.X < center.X + size.Width / 2)
            {
                result = false;
                return ret;
            }

            if (this.Parameter.EndLocation.Y < center.Y + size.Height / 2)
            {
                result = false;
                return ret;
            }
            result = true;
            return ret;
        }
        #endregion

        #region VisionTool Members
        public new RoiVisionToolParameter Parameter
        {
            get { return base.Parameter as RoiVisionToolParameter; }
            set
            {
                if (base.Parameter == value) return;
                base.Parameter = value;
            }
        }

        /// <summary>
        /// Vision Roi의 결과값을 가져온다.
        /// </summary>
        [Browsable(false)]
        public new RoiResult Result
        {
            get { return base.Result as RoiResult; }
            protected set { base.Result = value; }
        }
        #endregion
    }
    #endregion

    #region RoiVisionToolParameter
    [Serializable]
    public class RoiVisionToolParameter : VisionToolParameter
    {
        #region Define
        [Serializable]
        public enum Type
        {
            Circle,
            Rectangle,
        }
        #endregion

        #region Field
        private Point m_StartLocation;
        private Point m_CenterLocation;
        private Point m_EndLocation;
        private Size m_Size;
        private bool m_IsFull;
        private Type m_RoiType;
        [NonSerialized]
        private RectangleFrameVisionImageOverlay m_Overlay;
        #endregion

        #region Constructor
        public RoiVisionToolParameter(string name)
        {
            this.Overlay = new RectangleFrameVisionImageOverlay(name);

            this.RoiType = Type.Rectangle;

            this.StartLocation = new Point(100, 100);
            this.EndLocation = new Point(200, 200);

            //this.CenterLocation = new Point((this.EndLocation.X - this.StartLocation.X) / 2, (this.EndLocation.Y - this.StartLocation.Y) / 2);
            this.CenterLocation = new Point((this.EndLocation.X + this.StartLocation.X) / 2, (this.EndLocation.Y + this.StartLocation.Y) / 2);            //  더해서 나눠야 중간값이 나오지 않나.... -_-

            this.Size = new Size(this.EndLocation.X - this.StartLocation.X, this.EndLocation.Y - this.StartLocation.Y);
            this.IsFull = false;
        }
        public RoiVisionToolParameter() : this("") { }
        #endregion

        #region Property
        public Type RoiType
        {
            get { return this.m_RoiType; }
            set
            {
                if (this.m_RoiType == value) return;
                this.m_RoiType = value;
                this.HasChanged = true;
            }
        }

        public RectangleFrameVisionImageOverlay Overlay
        {
            get { return this.m_Overlay; }
            set
            {
                this.m_Overlay = value;
                this.HasChanged = true;
            }
        }

        /// <summary>
        /// Vision Roi의 전체 영역 설정 여부를 가져오거나 설정한다.
        /// </summary>
        public bool IsFull
        {
            get { return this.m_IsFull; }
            set
            {
                this.m_IsFull = value;
                if (this.Overlay == null)
                    this.Overlay = new RectangleFrameVisionImageOverlay("");
                this.Overlay.Visible = !value;
                this.HasChanged = true;
            }
        }

        /// <summary>
        /// Vision Roi의 시작하는 좌표를 가져오거나 설정한다.
        /// </summary>
        public Point StartLocation
        {
            get { return this.m_StartLocation; }
            set
            {
                this.m_Size = new Size(this.m_EndLocation.X - value.X, this.m_EndLocation.Y - value.Y);

                //  Center 위치 계산 방법 (기존)
                this.m_CenterLocation = new Point(this.m_EndLocation.X - Size.Width / 2, this.m_EndLocation.Y - Size.Height / 2);

                ////  Center 위치 계산 방법 변경
                //this.m_CenterLocation = new Point((this.m_EndLocation.X + this.m_StartLocation.X) / 2, (this.m_EndLocation.Y + this.m_StartLocation.Y) / 2);

                if (this.Overlay == null)
                    this.Overlay = new RectangleFrameVisionImageOverlay("");

                this.m_StartLocation = this.Overlay.StartLocation = value;
                this.HasChanged = true;
            }
        }

        /// <summary>
        /// Vision Roi의 중심 좌표를 가져오거나 설정한다.
        /// </summary>
        public Point CenterLocation
        {
            get { return this.m_CenterLocation; }
            set
            {
                this.m_StartLocation = new Point(value.X - this.m_Size.Width / 2, value.Y - this.m_Size.Height / 2);
                this.m_EndLocation = new Point(value.X + this.m_Size.Width / 2, value.Y + this.m_Size.Height / 2);

                if (this.Overlay == null)
                    this.Overlay = new RectangleFrameVisionImageOverlay("");
                this.m_CenterLocation = this.Overlay.CenterLocation = value;
                this.HasChanged = true;
            }
        }

        /// <summary>
        /// Vision Roi의 끝나는 좌표를 가져오거나 설정한다.
        /// </summary>
        public Point EndLocation
        {
            get { return this.m_EndLocation; }
            set
            {
                this.m_Size = new Size(value.X - this.m_StartLocation.X, value.Y - this.m_StartLocation.Y);

                //  Center 위치 계산 방법 (기존)
                this.m_CenterLocation = new Point(this.m_EndLocation.X - Size.Width / 2, this.m_EndLocation.Y - Size.Height / 2);

                ////  Center 위치 계산 방법 변경
                //this.m_CenterLocation = new Point((this.m_EndLocation.X + this.m_StartLocation.X) / 2, (this.m_EndLocation.Y + this.m_StartLocation.Y) / 2);

                if (this.Overlay == null)
                    this.Overlay = new RectangleFrameVisionImageOverlay("");
                this.m_EndLocation = this.Overlay.EndLocation = value;
                this.HasChanged = true;
            }
        }

        /// <summary>
        /// Vision Roi의 크기를 가져오거나 설정한다.
        /// </summary>
        public Size Size
        {
            get { return this.m_Size; }
            set
            {
                //  End 위치 계산 방법 (기존)
                this.m_EndLocation = new Point(this.m_StartLocation.X + value.Width, this.m_StartLocation.Y + value.Height);

                //  Center 위치 계산 방법 (기존)
                this.m_CenterLocation = new Point(this.m_StartLocation.X + value.Width / 2, this.m_StartLocation.Y + value.Height / 2);

                ////  End 위치 계산 방법 변경
                //this.m_EndLocation = new Point(this.m_CenterLocation.X + value.Width / 2, this.m_CenterLocation.Y + value.Height / 2);

                ////  Center 위치 계산 방법 변경
                //this.m_CenterLocation = new Point((this.m_EndLocation.X + this.m_StartLocation.X) / 2, (this.m_EndLocation.Y + this.m_StartLocation.Y) / 2);

                if (this.Overlay == null)
                    this.Overlay = new RectangleFrameVisionImageOverlay("");
                this.m_Size = this.Overlay.Size = value;
                this.HasChanged = true;
            }
        }
        #endregion
    }
    #endregion

    #region RoiVisionToolCollection
    [Serializable]
    public class RoiVisionToolCollection : Collection<RoiVisionTool>
    {

    }
    #endregion

    #region RoiResult
    [Serializable]
    public class RoiResult : VisionResult
    {
        #region Fleid

        #endregion

        #region Constructor
        public RoiResult(string owner) : base(owner) { }
        #endregion

        #region Property

        #endregion
    }
    #endregion
}