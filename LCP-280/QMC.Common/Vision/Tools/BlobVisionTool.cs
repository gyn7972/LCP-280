/*
 * Purpose
 *      Blob Vision Tool 클래스에 대해서 정의한다.
 * 
 * Revision
 *      1. Created: 2018.01.31 LEE.SH
 * 
 */


using System;
using System.Drawing;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;


public enum Polarity
{
    LightBlobs,
    DarkBlobs
}


namespace QMC.Common.Vision.Tools
{
    #region BlobVisionTool
    [Serializable]
    public abstract class BlobVisionTool : VisionTool
    {
        #region VisionTool Members
        public new BlobVisionToolParameter Parameter
        {
            get { return base.Parameter as BlobVisionToolParameter; }
            set { base.Parameter = value; }
        }

        [Browsable(false)]
        public new BlobResult Result
        {
            get { return base.Result as BlobResult; }
            protected set { base.Result = value; }
        }
        #endregion

        #region Contructor
        public BlobVisionTool(string name) : base(name)
        {
            this.Result = new BlobResult(this.Name);

        }
        public BlobVisionTool() : this("") { }
        #endregion

        #region Property
        #endregion

        #region Method


        protected int CrossLineOverlay(Size size, string centerX, string centerY)
        {
            int ret = 0;

            LineFrameVisionImageOverlay lineXOverlay = null;
            LineFrameVisionImageOverlay lineYOverlay = null;

            double length = size.Width < size.Height ? size.Width / 3 : size.Height / 3;

            if (this.Result.ResultOverlays.Count != 0)
                this.Result.ResultOverlays.Clear();

            for (int i = 0; i < this.Result.PixelValues.Count; i++)
            {
                lineXOverlay = new LineFrameVisionImageOverlay();
                lineXOverlay.StartLocation = new PointD(this.Result.PixelValues[i][centerX].Value - size.Width / 200, this.Result.PixelValues[i][centerY].Value);
                lineXOverlay.EndLocation = new PointD(this.Result.PixelValues[i][centerX].Value + size.Width / 200, this.Result.PixelValues[i][centerY].Value);
                lineXOverlay.Visible = true;
                this.Result.ResultOverlays.Add(lineXOverlay);

                lineYOverlay = new LineFrameVisionImageOverlay();
                lineYOverlay.StartLocation = new PointD(this.Result.PixelValues[i][centerX].Value, this.Result.PixelValues[i][centerY].Value - size.Height / 200);
                lineYOverlay.EndLocation = new PointD(this.Result.PixelValues[i][centerX].Value, this.Result.PixelValues[i][centerY].Value + size.Height / 200);
                lineYOverlay.Visible = true;
                this.Result.ResultOverlays.Add(lineYOverlay);
            }

            return ret;
        }

        protected int CrossLineOverlay(Size size, Enum centerX, Enum centerY)
        {
            int ret = 0;
            string x = centerX.ToString();
            string y = centerY.ToString();

            CrossLineOverlay(size, x, y);

            return ret;
        }
        #endregion


    }
    #endregion

    #region BlobVisionToolParameter
    [Serializable]
    public class BlobVisionToolParameter : VisionToolParameter
    {
        #region Field
        private bool m_ResultOverlayVisible;
        private int m_MinPixels;
        private int m_HardThreshold;
        private Polarity m_Polarity;
        #endregion

        #region Constructor
        public BlobVisionToolParameter() : base()
        {
            this.ResultOverlayVisible = true;
            this.MinPixels = 100;
            this.HardThreshold = 127;
            this.m_Polarity = new Polarity();

        }
        #endregion

        #region Property
        public bool ResultOverlayVisible
        {
            get { return this.m_ResultOverlayVisible; }
            set { this.m_ResultOverlayVisible = value; }
        }

        public int MinPixels
        {
            get { return this.m_MinPixels; }
            set
            {
                if (this.m_MinPixels == value) return;
                this.m_MinPixels = value;
                this.HasChanged = true;
            }
        }


        public int HardThreshold
        {
            get { return this.m_HardThreshold; }
            set
            {
                if (this.m_HardThreshold == value) return;
                this.m_HardThreshold = value;
                this.HasChanged = true;
            }
        }

        public Polarity Polarity
        {
            get { return this.m_Polarity; }
            set { this.m_Polarity = value; }
        }
        #endregion
    }
    #endregion

    #region BlobResult
    [Serializable]
    public class BlobResult : VisionResult
    {
        #region Define
        public class ItemResult
        {
            #region Field
            private Enum m_Name;
            private double m_Value;
            #endregion

            #region Constructor
            public ItemResult()
            {
            }
            #endregion

            #region Property
            public Enum Name
            {
                get { return this.m_Name; }
                set { this.m_Name = value; }
            }

            public double Value
            {
                get { return this.m_Value; }
                set { this.m_Value = value; }
            }
            #endregion
        }

        public class ItemResultKeyedCollection : KeyedCollection<Enum, ItemResult>
        {
            protected override Enum GetKeyForItem(ItemResult item)
            {
                return item.Name;
            }

            public ItemResult this[string key]
            {
                get
                {
                    for (int i = 0; i < this.Count; i++)
                    {
                        if (this[i].Name.ToString() == key)
                            return this[i];
                    }
                    return null;
                }
            }
        }

        public class PixelValueCollection : Collection<ItemResultKeyedCollection>
        {
        }

        [Serializable]
        public class ResultOverlayCollection : Collection<FrameVisionImageOverlay>
        {
        }
        #endregion

        #region Field
        private PixelValueCollection m_PixelValues;
        private ResultOverlayCollection m_ResultOverlays;
        #endregion

        #region Constructor
        public BlobResult(string owner) : base(owner)
        {
            this.PixelValues = new PixelValueCollection();
            this.ResultOverlays = new ResultOverlayCollection();
        }
        #endregion

        #region Property
        public PixelValueCollection PixelValues
        {
            get { return this.m_PixelValues; }
            set { this.m_PixelValues = value; }
        }

        public ResultOverlayCollection ResultOverlays
        {
            get { return this.m_ResultOverlays; }
            set { this.m_ResultOverlays = value; }
        }
        #endregion
    }
    #endregion
}
