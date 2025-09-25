/*
 * Purpose
 *      VisionImage를 보여주는 컨트롤을 정의한다.
 *      
 *      
 * Revision
 *      1. Created: 2018.01.09 JUNG.CY
 *      2. Modify : 2019.01.04 JUNG.CY
 *          - Resizing을 통한 최적화 완료.
 *      3. Modify : 2019.10.28 JUNG.CY
 *          - DoubleBufferedGraphics를 이용하여 Drawing시 발생하는 프로그램 느려지는 현상 제거.
 *          - Zoom in-out 변경. (기존 : Center -> 변경 : Mouse 위치 )
 * 
 */

using QMC.Common.Cameras;
using QMC.Common.Component;
using QMC.Common.Unit;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static QMC.Common.Vision.Tools.PatternMatchingResult;
//using QMC.eFramework.Vision.Tools;

namespace QMC.Common.Vision
{


    [System.Drawing.ToolboxBitmap(typeof(PictureBox))]
    public class VisionImageViewer : PictureBox
    {
        #region DllImport
		
        [DllImport("gdi32.dll")]
        static extern int SetStretchBltMode(IntPtr hdc, int iStretchMode);

        private const int HALFTONE = 4;
        [DllImport("gdi32.dll")]
        static extern bool BitBlt(
        IntPtr hdcDest, int nXDest, int nYDest, int nWidth, int nHeight,
        IntPtr hdcSrc, int nXSrc, int nYSrc, int dwRop);
        [DllImport("gdi32.dll")]
        static extern bool DeleteDC(IntPtr hdc);

        [DllImport("gdi32.dll")]
        static extern bool DeleteObject(IntPtr hObject);
        [DllImport("gdi32.dll")]
        static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);

        [DllImport("gdi32.dll")]
        static extern IntPtr CreateCompatibleDC(IntPtr hdc);
        [DllImport("gdi32.dll", SetLastError = true)]
        private static extern bool StretchBlt(
    IntPtr hdcDest, int nXOriginDest, int nYOriginDest, int nWidthDest, int nHeightDest,
    IntPtr hdcSrc, int nXOriginSrc, int nYOriginSrc, int nWidthSrc, int nHeightSrc,
    int dwRop);
        private const int SRCCOPY = 0x00CC0020; 
        #endregion

        #region Define
        [Serializable]
        public enum MenuItems
        {
            [Abbreviation("Live")]
            Live,
            [Abbreviation("Stop")]
            Stop,
            [Abbreviation("Image load")]
            ImageLoad,
            [Abbreviation("Image save")]
            ImageSave,
            [Abbreviation("Result overlay clear")]
            ResultOverlayClear,
            [Abbreviation("Image auto fit")]
            ImageAutoFit,
            [Abbreviation("x2")]
            x2,
            [Abbreviation("x4")]
            x4,
            [Abbreviation("x8")]
            x8,
            [Abbreviation("Custom")]
            Custom,
        }

        [Serializable]
        public enum OperatingTypes
        {
            Center,
            Mouse_Move,
            Mouse_Center,
        }

        public class OwnedOverlayCollection : Collection<VisionImageOverlay>
        {
            #region Constructor
            public OwnedOverlayCollection(VisionImageViewer Owner)
            {

            }
            public OwnedOverlayCollection()
            {

            }
            #endregion
        }

        [Serializable]
        public class ImageScale
        {
            #region Define
            [Serializable]
            public abstract class ShiftSpecification
            {
                #region Define
                [Serializable]
                public enum ShiftDirections
                {
                }
                #endregion

                #region Field
                private ImageScale m_Owner;
                private Enum m_ShiftDirection;
                private double m_Offset;
                private double m_CenterPoint;
                #endregion

                #region Constructor
                public ShiftSpecification(ImageScale owner)
                {
                    this.Owner = owner;
                }
                #endregion

                #region Property
                public ImageScale Owner
                {
                    get { return this.m_Owner; }
                    private set { this.m_Owner = value; }
                }

                public Enum ShiftDirection
                {
                    get { return this.m_ShiftDirection; }
                    set { this.m_ShiftDirection = value; }
                }

                public double Offset
                {
                    get { return this.m_Offset; }
                    set { this.m_Offset = value; }
                }

                public double CenterPoint
                {
                    get { return this.m_CenterPoint; }
                    set { this.m_CenterPoint = value; }
                }
                #endregion

                #region Method
                public void Convert(double point, Size imageSize)
                {
                    this.OnConvert(point, imageSize);
                }

                protected abstract void OnConvert(double point, Size imageSize);
                #endregion
            }

            [Serializable]
            public class HorizontalShiftSpecification : ShiftSpecification
            {
                #region Define
                [Serializable]
                public new enum ShiftDirections
                {
                    Left,
                    Right,
                    Center,
                }
                #endregion

                #region Constructor
                public HorizontalShiftSpecification(ImageScale owner) : base(owner)
                {
                    this.ShiftDirection = ShiftDirections.Center;
                }
                #endregion

                #region Property
                public new ShiftDirections ShiftDirection
                {
                    get { return (ShiftDirections)base.ShiftDirection; }
                    set { base.ShiftDirection = value; }
                }
                #endregion

                #region ShiftSpecification Members
                protected override void OnConvert(double point, Size imageSize)
                {
                    double nextOffset = 0.0;
                    SizeD resize = new SizeD(imageSize.Width * this.Owner.Wheel, imageSize.Height * this.Owner.Wheel);

                    // 이전과 동일하게 이미지가 왼쪽으로 치우쳤을 경우
                    if (point - resize.Width / 2 < 0 && this.ShiftDirection == ShiftDirections.Left)
                    {
                        //if(this.Offset == 0)
                        //{
                        this.Offset = 0;
                        this.CenterPoint = resize.Width / 2;
                        //}
                        //else
                        //{
                        //    nextOffset = point - resize.Width / 2;

                        //    // Offset량이 급격하게 움직이는것을 방지.
                        //    // 다음 Offset이 이미지를 넘어가는 경우.
                        //    nextOffset = (this.Offset - nextOffset) * (0.1 * this.Owner.Wheel);
                        //    nextOffset = this.Offset - nextOffset;

                        //    this.Offset = nextOffset;
                        //    this.CenterPoint = nextOffset + resize.Width / 2;
                        //}
                    }
                    // 이전과 다르게 이미지가 왼쪽으로 치우쳤을 경우
                    else if (point - resize.Width / 2 < 0 && this.ShiftDirection != ShiftDirections.Left)
                    {
                        nextOffset = point - resize.Width / 2;

                        // Offset량이 급격하게 움직이는것을 방지.
                        // 다음 Offset이 이미지를 넘어가는 경우.
                        nextOffset = (this.Offset - nextOffset) * (0.1 * this.Owner.Wheel);
                        nextOffset = this.Offset - nextOffset;

                        this.Offset = nextOffset;
                        this.CenterPoint = nextOffset + resize.Width / 2;

                        this.ShiftDirection = ShiftDirections.Left;
                    }
                    // 이전과 동일하게 이미지가 오른쪽으로 치우쳤을 경우
                    else if (imageSize.Width < point + resize.Width / 2 && this.ShiftDirection == ShiftDirections.Right)
                    {
                        //if(this.Offset == imageSize.Width - resize.Width)
                        //{
                        this.Offset = imageSize.Width - resize.Width;
                        this.CenterPoint = this.Offset + resize.Width / 2;
                        //}
                        //else
                        //{
                        //    nextOffset = point - resize.Width / 2;

                        //    // Offset량이 급격하게 움직이는것을 방지.
                        //    // 다음 Offset이 이미지를 넘어가는 경우.
                        //    nextOffset = (nextOffset - this.Offset) * (0.9 / this.Owner.Wheel);
                        //    nextOffset = this.Offset + nextOffset;

                        //    this.Offset = nextOffset;
                        //    this.CenterPoint = nextOffset + resize.Width / 2;
                        //}
                    }
                    // 이전과 다르게 이미지가 오른쪽으로 치우쳤을 경우
                    else if (imageSize.Width < point + resize.Width / 2 && this.ShiftDirection != ShiftDirections.Right)
                    {
                        nextOffset = point - resize.Width / 2;

                        // Offset량이 급격하게 움직이는것을 방지.
                        // 다음 Offset이 이미지를 넘어가는 경우.
                        nextOffset = (nextOffset - this.Offset) * (1 - 0.1 * this.Owner.Wheel);
                        nextOffset = this.Offset + nextOffset;

                        this.Offset = nextOffset;
                        this.CenterPoint = this.Offset + resize.Width / 2;

                        this.ShiftDirection = ShiftDirections.Right;
                    }
                    else
                    {
                        nextOffset = point - resize.Width / 2;

                        // 왼쪽으로 이동했을시,
                        if (nextOffset < this.Offset)
                        {
                            nextOffset = (this.Offset - nextOffset) * (0.1 * this.Owner.Wheel);
                            nextOffset = this.Offset - nextOffset;
                        }
                        // 오른쪽으로 이동했을시,
                        else if (this.Offset < nextOffset)
                        {
                            nextOffset = (nextOffset - this.Offset) * (1 - 0.1 * this.Owner.Wheel);
                            nextOffset = nextOffset + this.Offset;
                        }

                        this.Offset = nextOffset;
                        this.CenterPoint = this.Offset + resize.Width / 2;

                        this.ShiftDirection = ShiftDirections.Center;
                    }
                }
                #endregion
            }

            [Serializable]
            public class VerticalShiftSpecification : ShiftSpecification
            {
                #region Define
                [Serializable]
                public new enum ShiftDirections
                {
                    Top,
                    Bottom,
                    Center,
                }
                #endregion

                #region Constructor
                public VerticalShiftSpecification(ImageScale owner) : base(owner)
                {
                    this.ShiftDirection = ShiftDirections.Center;
                }
                #endregion

                #region Property
                public new ShiftDirections ShiftDirection
                {
                    get { return (ShiftDirections)base.ShiftDirection; }
                    set { base.ShiftDirection = value; }
                }
                #endregion

                #region ShiftSpecification Members
                protected override void OnConvert(double point, Size imageSize)
                {
                    double nextOffset = 0.0;
                    SizeD resize = new SizeD(imageSize.Width * this.Owner.Wheel, imageSize.Height * this.Owner.Wheel);

                    // 이전과 동일하게 이미지가 위쪽으로 치우쳤을 경우
                    if (point - resize.Height / 2 < 0 && this.ShiftDirection == ShiftDirections.Top)
                    {
                        //if(this.Offset == 0)
                        //{
                        this.Offset = 0;
                        this.CenterPoint = resize.Height / 2;
                        //}
                        //else
                        //{
                        //    nextOffset = point - resize.Height / 2;

                        //    // Offset량이 급격하게 움직이는것을 방지.
                        //    // 다음 Offset이 이미지를 넘어가는 경우.
                        //    nextOffset = (this.Offset - nextOffset) * (0.1 * this.Owner.Wheel);
                        //    nextOffset = this.Offset - nextOffset;

                        //    this.Offset = nextOffset;
                        //    this.CenterPoint = nextOffset + resize.Height / 2;
                        //}
                    }
                    // 이전과 다르게 이미지가 위쪽으로 치우쳤을 경우
                    else if (point - resize.Height / 2 < 0 && this.ShiftDirection != ShiftDirections.Top)
                    {
                        nextOffset = point - resize.Height / 2;

                        // Offset량이 급격하게 움직이는것을 방지.
                        // 다음 Offset이 이미지를 넘어가는 경우.
                        nextOffset = (this.Offset - nextOffset) * (0.1 * this.Owner.Wheel);
                        nextOffset = this.Offset - nextOffset;

                        this.Offset = nextOffset;
                        this.CenterPoint = this.Offset + resize.Height / 2;

                        this.ShiftDirection = ShiftDirections.Top;
                    }
                    // 이전과 동일하게 이미지가 오른쪽으로 치우쳤을 경우
                    else if (imageSize.Height < point + resize.Height / 2 && this.ShiftDirection == ShiftDirections.Bottom)
                    {
                        //if(this.Offset == imageSize.Height - resize.Height)
                        //{
                        this.Offset = imageSize.Height - resize.Height;
                        this.CenterPoint = this.Offset + resize.Height / 2;
                        //}
                        //else
                        //{
                        //    nextOffset = point - resize.Height / 2;

                        //    // Offset량이 급격하게 움직이는것을 방지.
                        //    // 다음 Offset이 이미지를 넘어가는 경우.
                        //    nextOffset = (nextOffset - this.Offset) * (0.9 / this.Owner.Wheel);
                        //    nextOffset = this.Offset + nextOffset;

                        //    this.Offset = nextOffset;
                        //    this.CenterPoint = nextOffset + resize.Height / 2;
                        //}
                    }
                    // 이전과 다르게 이미지가 아래쪽으로 치우쳤을 경우
                    else if (imageSize.Height < point + resize.Height / 2 && this.ShiftDirection != ShiftDirections.Bottom)
                    {
                        nextOffset = point - resize.Height / 2;

                        // Offset량이 급격하게 움직이는것을 방지.
                        // 다음 Offset이 이미지를 넘어가는 경우.
                        nextOffset = (nextOffset - this.Offset) * (1 - 0.1 * this.Owner.Wheel);
                        nextOffset = this.Offset + nextOffset;

                        this.Offset = nextOffset;
                        this.CenterPoint = this.Offset + resize.Height / 2;

                        this.ShiftDirection = ShiftDirections.Bottom;
                    }
                    else
                    {
                        nextOffset = point - resize.Height / 2;

                        // 위쪽으로 이동했을시,
                        if (nextOffset < this.Offset)
                        {
                            nextOffset = (this.Offset - nextOffset) * (0.1 * this.Owner.Wheel);
                            nextOffset = this.Offset - nextOffset;
                        }
                        // 아래쪽으로 이동했을시,
                        else if (this.Offset < nextOffset)
                        {
                            nextOffset = (nextOffset - this.Offset) * (1 - 0.1 * this.Owner.Wheel);
                            nextOffset = nextOffset + this.Offset;
                        }

                        this.Offset = nextOffset;
                        this.CenterPoint = this.Offset + resize.Height / 2;

                        this.ShiftDirection = ShiftDirections.Center;
                    }
                }
                #endregion
            }
            #endregion

            #region Field
            private PointD m_Scale;
            private double m_Wheel;
            private VerticalShiftSpecification m_VerticalShift;
            private HorizontalShiftSpecification m_HorizontalShift;
            #endregion

            #region Constructor
            public ImageScale(double x, double y, double wheel)
            {
                this.Scale = new PointD(x, y);
                this.Wheel = wheel;
                this.HorizontalShift = new HorizontalShiftSpecification(this);
                this.VerticalShift = new VerticalShiftSpecification(this);
            }
            public ImageScale(double x, double y) : this(x, y, 1) { }
            public ImageScale() : this(1, 1) { }
            #endregion

            #region Property
            public PointD Scale
            {
                get { return this.m_Scale; }
                set { this.m_Scale = value; }
            }

            public double Wheel
            {
                get { return this.m_Wheel; }
                set { this.m_Wheel = value; }
            }

            public VerticalShiftSpecification VerticalShift
            {
                get { return this.m_VerticalShift; }
                set { this.m_VerticalShift = value; }
            }

            public HorizontalShiftSpecification HorizontalShift
            {
                get { return this.m_HorizontalShift; }
                set { this.m_HorizontalShift = value; }
            }
            #endregion

            #region Method
            public void SetMousePoint(Point point)
            {
                this.HorizontalShift.CenterPoint = point.X;
                this.VerticalShift.CenterPoint = point.Y;
            }

            public PointD GetCenterPoint()
            {


                return new PointD(this.HorizontalShift.CenterPoint, this.VerticalShift.CenterPoint);
            }

            public PointD GetOffset()
            {
                return new PointD(this.HorizontalShift.Offset, this.VerticalShift.Offset);
            }

            public int SetOffsetAndCenterPoint(PointD offset, Size imageSize)
            {
                int ret = 0;
                SizeD resize = new SizeD();

                resize = new SizeD(imageSize.Width * this.Wheel, imageSize.Height * this.Wheel);

                if (offset.X < 0)
                {
                    this.HorizontalShift.Offset = 0;
                    this.HorizontalShift.CenterPoint = resize.Width / 2;
                }
                else if (imageSize.Width <= resize.Width + offset.X)
                {
                    this.HorizontalShift.Offset = imageSize.Width - resize.Width;
                    this.HorizontalShift.CenterPoint = this.HorizontalShift.Offset + resize.Width / 2;
                }
                else
                {
                    this.HorizontalShift.Offset = offset.X;
                    this.HorizontalShift.CenterPoint = offset.X + resize.Width / 2;
                }

                if (offset.Y < 0)
                {
                    this.VerticalShift.Offset = 0;
                    this.VerticalShift.CenterPoint = resize.Height / 2;
                }
                else if (imageSize.Height <= resize.Height + offset.Y)
                {
                    this.VerticalShift.Offset = imageSize.Height - resize.Height;
                    this.VerticalShift.CenterPoint = this.VerticalShift.Offset + resize.Height / 2;
                }
                else
                {
                    this.VerticalShift.Offset = offset.Y;
                    this.VerticalShift.CenterPoint = offset.Y + resize.Height / 2;
                }

                return ret;
            }

            public PointD GetCenterPoint(Control control)
            {
                return new PointD(this.HorizontalShift.Offset + control.Width / 2 * this.Scale.X * this.Wheel, this.VerticalShift.Offset + control.Height / 2 * this.Scale.Y * this.Wheel);
            }

            public PointD GetCenterPoint(Point point)
            {
                return this.GetCenterPoint(point.X, point.Y);
            }

            public PointD GetCenterPoint(int x, int y)
            {
                return new PointD(this.HorizontalShift.Offset + x / 2 * this.Scale.X * this.Wheel, this.VerticalShift.Offset + y / 2 * this.Scale.Y * this.Wheel);
            }

            public PointD GetPoint(int x, int y)
            {
                return new PointD((this.HorizontalShift.Offset + x * this.Scale.X )* this.Wheel, (this.VerticalShift.Offset + y * this.Scale.Y) * this.Wheel);
            }

            public SizeD GetSize(Control control)
            {
                return new SizeD(control.Width * this.Scale.X * this.Wheel, control.Height * this.Scale.Y * this.Wheel);
            }

            public void Convert(PointD point, Size imageSize)
            {
                this.HorizontalShift.Convert(point.X, imageSize);
                this.VerticalShift.Convert(point.Y, imageSize);
            }

            public void MoveCenter(Size imageSize)
            {
                SizeD resize = new SizeD(imageSize.Width * this.Wheel, imageSize.Height * this.Wheel);

                this.HorizontalShift.Offset = (imageSize.Width - resize.Width) / 2;
                this.VerticalShift.Offset = (imageSize.Height - resize.Height) / 2;
            }
            #endregion
        }

        private const string LogName = "VisionControl";
        private object ViewerSyncRoot;
        #endregion

        #region Field
        private BufferedGraphicsContext m_Context;
        private BufferedGraphics m_Graphics;
        private Graphics m_GraphicsDisplay;
        private Bitmap m_bitmap;
        private System.Drawing.Graphics m_doubleBuffer;
        private OwnedOverlayCollection m_NormalOverlays;
        private OwnedOverlayCollection m_ResultOverlays;
        private ResultOverlayCollection m_ResultOverlayCollection;
        private VisionImage m_InputImage;
        private bool m_SuspendedDisplay;
        private double m_FrameRate;
        private CameraSwitch m_CameraSwitch;
        private Camera m_Camera;
        private ImageScale m_Scale;
        private bool m_IsViewCustomizedImage;
        private bool m_IsChanged;
        private Label m_TopCaption;
        private Label m_BottomCaption;
        private DateTime m_LatestDisplayTime;
        private PointD m_PreviousPoint;
        private OperatingTypes m_OperatingType;
        private bool m_bVisibleCrossLine;
        private LineFrameVisionImageOverlay m_HorizentalLine;
        private LineFrameVisionImageOverlay m_VerticalLine;
        private bool m_FixedByWidth;
        #endregion

        #region Constructor
        public VisionImageViewer()
        {
            //this.SetStyle(ControlStyles.AllPaintingInWmPaint |
            //         ControlStyles.UserPaint |
            //         ControlStyles.OptimizedDoubleBuffer, true);
            //this.UpdateStyles();

            //this.Timer = new SafeTimer();
            this.NormalOverlays = new OwnedOverlayCollection(this);
            this.ResultOverlays = new OwnedOverlayCollection(this);
            this.Scale = new ImageScale();
            this.IsViewCustomizedImage = false;
            this.FrameRate = 1;
            this.OperatingType = OperatingTypes.Center;

            this.VisibleCrossLine = true;

            this.HandleDestroyed += VisionImageViewer_HandleDestroyed;

            this.m_FixedByWidth = false;
            ViewerSyncRoot = new object();

            //UpdateDelayTime = 160;
            UpdateDelayTime = 80;
            OnCreateControl();
        }

        private void VisionImageViewer_HandleDestroyed(object sender, EventArgs e)
        {
            StopUpdateTask();
        }

        private void VisionImageViewer_HandleCreated(object sender, EventArgs e)
        {
            //InitializeBufferedGraphics();
            StopUpdateTask();
        }

        //private void VisionImageViewer_HandleDestroyed(object sender, EventArgs e)
        //{
        //    DisposeBufferedGraphics();
        //}

        private void VisionImageViewer_Resize(object sender, EventArgs e)
        {
            //if (_initialized)
            //    InitializeBufferedGraphics();
        }

        private void InitializeBufferedGraphics()
        {
            if (!this.IsHandleCreated || this.Width <= 0 || this.Height <= 0)
                return;

            if (this.m_Context == null)
                this.m_Context = BufferedGraphicsManager.Current;

            this.m_Context.MaximumBuffer = new Size(this.Width + 1, this.Height + 1);

            if (this.m_Graphics != null)
            {
                lock (this.m_Graphics)
                {
                    this.m_Graphics.Dispose();
                    this.m_Graphics = null;
                }
            }

            this.m_Graphics = this.m_Context.Allocate(this.CreateGraphics(), new Rectangle(0, 0, this.Width, this.Height));
        }
        #endregion

        #region Property

        public int UpdateDelayTime { set; get; }
        public bool Simulated { set; get; } //Load로 사용해 보자.

        [Browsable(false)]
        public VisionImage InputImage
        {
            get { return this.m_InputImage; }
            set
            {
                if (m_InputImage != value)
                {
                    this.m_InputImage = value;
                }
            }
        }
        public void SetImageNDisplay(VisionImage image)
        {
            this.m_InputImage = image;
        }
        [Browsable(false)]
        public OwnedOverlayCollection NormalOverlays
        {
            get { return this.m_NormalOverlays; }
            private set { this.m_NormalOverlays = value; }
        }

        [Browsable(false)]
        public OwnedOverlayCollection ResultOverlays
        {
            get { return this.m_ResultOverlays; }
            set { this.m_ResultOverlays = value; }
        }

        public double FrameRate
        {
            get { return this.m_FrameRate; }
            set
            {
                if (this.m_FrameRate == value) return;
                this.m_FrameRate = value;
            }
        }

        [Browsable(false)]
        public CameraSwitch CameraSwitch
        {
            get { return this.m_CameraSwitch; }
            set
            {
                Camera part = null;

                if (value == null) return;

                if (this.m_CameraSwitch == value) return;

                if (this.m_CameraSwitch != null)
                {
                    for (int i = 0; i < this.m_CameraSwitch.Cameras.Count; i++)
                    {
                        part = this.m_CameraSwitch.Cameras[i];

                        if (part == null) continue;
                        else
                        {
                            //part.Executor.BeforeJobManagerChange -= Executor_BeforeJobManagerChange;
                            //part.Executor.AfterJobManagerChange -= Executor_AfterJobManagerChange;

                            //if (part.Executor.JobManager != null)
                            //    part.Executor.JobManager.SelectedIndexChanged -= JobManager_SelectedIndexChanged;
                        }
                    }
                    this.m_CameraSwitch.AfterChange -= M_CameraSwitch_AfterChange;
                }

                this.m_CameraSwitch = value;

                for (int i = 0; i < this.m_CameraSwitch.Cameras.Count; i++)
                {
                    part = this.m_CameraSwitch.Cameras[i];

                    if (part == null) continue;
                    else
                    {
                        //part.Executor.BeforeJobManagerChange += Executor_BeforeJobManagerChange;
                        //part.Executor.AfterJobManagerChange += Executor_AfterJobManagerChange;

                        //if (part.Executor.JobManager != null)
                        //    part.Executor.JobManager.SelectedIndexChanged += JobManager_SelectedIndexChanged;
                    }

                    //this.SetNormalOverlay(part, part.Executor.JobManager);
                }
                this.m_CameraSwitch.AfterChange += M_CameraSwitch_AfterChange;

                if (this.m_CameraSwitch.SelectCameraIndex != -1)
                {
                    this.m_Context = BufferedGraphicsManager.Current;
                    this.m_Context.MaximumBuffer = this.Size;

                    //lock(ViewerSyncRoot)
                    {
                        BufferedGraphics old = null;
                        if (this.m_Graphics != null)
                        {
                            old = this.m_Graphics;
                        }

                        this.m_Graphics = this.m_Context.Allocate(this.CreateGraphics(),
                            new Rectangle(new Point(0, 0), this.Size));

                        if (old != null)
                        {
                            lock (old)
                            {
                                old.Dispose(); ;
                            }
                        }
                    }

                    this.Scale.SetMousePoint(new PointD(this.m_CameraSwitch.Cameras[this.m_CameraSwitch.SelectCameraIndex].Resolution.Width / 2, this.m_CameraSwitch.Cameras[this.m_CameraSwitch.SelectCameraIndex].Resolution.Height / 2));
                }
            }
        }

        [Browsable(false)]
        public Camera Camera
        {
            get { return this.m_Camera; }
            set
            {
                VisionImage image = null;
                Camera part = null;

                if (value == null) return;

                //if (m_FixedByWidth)
                //{
                //    double ratio = (double)value.Resolution.Width / value.Resolution.Height;
                //    int nHegiht = this.Height;
                //    int nWidth = (int)(nHegiht * ratio);
                //    this.Width = nWidth;
                //}
                //else
                //{
                //    double ratio = (double)value.Resolution.Height / value.Resolution.Width;
                //    int nWidth = this.Width;
                //    int nHegiht = (int)(nWidth * ratio);
                //    this.Height = nHegiht;
                //}

                //if (this.m_Camera == value) return;

                if (this.m_Camera != null)
                {
                    part = this.m_Camera;


                    //if (part != null)
                    //{
                    //    part.Executor.BeforeJobManagerChange -= Executor_BeforeJobManagerChange;
                    //    part.Executor.AfterJobManagerChange -= Executor_AfterJobManagerChange;

                    //    if (part.Executor.JobManager != null)
                    //        part.Executor.JobManager.SelectedIndexChanged -= JobManager_SelectedIndexChanged;
                    //}
                    //if (this.m_Camera.GetModule() != null)
                    //    this.m_Camera.GetModule().ServiceStateChanged -= this.VisionImageViewer_ServiceStateChanged;
                }

                this.m_Camera = value;
                InitCrossLine();
                ShowCrossLine(this.VisibleCrossLine);
                //if (this.m_Camera.GetModule() != null)
                //    this.m_Camera.GetModule().ServiceStateChanged += this.VisionImageViewer_ServiceStateChanged;

                part = this.m_Camera;
                this.m_Camera.GrabSync(out image);

                if (image != null)
                {
                    this.InputImage = image;
                    this.Scale.Wheel = 1.0;

                    this.Scale.SetMousePoint(new Point(this.InputImage.Header.Width / 2, this.InputImage.Header.Height / 2));
                    this.Scale.MoveCenter(new Size(this.InputImage.Header.Width, this.InputImage.Header.Height));
                }

                //if (part != null)
                //{
                //    part.Executor.BeforeJobManagerChange += Executor_BeforeJobManagerChange;
                //    part.Executor.AfterJobManagerChange += Executor_AfterJobManagerChange;

                //    if (part.Executor.JobManager != null)
                //        part.Executor.JobManager.SelectedIndexChanged += JobManager_SelectedIndexChanged;

                //    this.SetNormalOverlay(part, part.Executor.JobManager);
                //}

                this.m_Context = BufferedGraphicsManager.Current;
                if (this.Size.Width <= 0)
                {
                    this.Size = new Size(500, 340);
                }
                if (this.Size.Height <= 0)
                {
                    this.Size = new Size(500, 340);
                }
                this.m_Context.MaximumBuffer = this.Size;

                //lock (ViewerSyncRoot)
                {
                    BufferedGraphics old = null;
                    if (this.m_Graphics != null)
                    {
                        old = this.m_Graphics;
                    }

                    this.m_Graphics = this.m_Context.Allocate(this.CreateGraphics(),
                        new Rectangle(new Point(0, 0), this.Size));
                    if (old != null)
                    {
                        lock (old)
                        {
                            old.Dispose();
                        }
                    }


                }



                m_bitmap = new Bitmap(Size.Width, Size.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                m_doubleBuffer = Graphics.FromImage(m_bitmap);
                //this.Scale.SetMousePoint(new PointD(this.m_Camera.Resolution.Width / 2, this.m_Camera.Resolution.Height / 2));
            }
        }

        /// <summary>
        /// 현재 VisionImageViewer의 Suspend 상태를 가져온다.
        /// </summary>
        [Browsable(false)]
        public bool SuspendedDisplay
        {
            get { return this.m_SuspendedDisplay; }
            protected set
            {
                this.m_SuspendedDisplay = value;
                if (this.InputImage != null)
                {
                    this.Scale.Wheel = 1.0;
                    
                    this.Scale.SetMousePoint(new Point(this.InputImage.Header.Width / 2, this.InputImage.Header.Height / 2));
                    this.Scale.MoveCenter(new Size(this.InputImage.Header.Width, this.InputImage.Header.Height));
                }
            }
        }

        /// <summary>
        /// 현재 VisionImageViewer의 Zoom Scale을 가져온다.
        /// </summary>
        [Browsable(false)]
        public new ImageScale Scale
        {
            get { return this.m_Scale; }
            private set
            {
                if (this.m_Scale == value) return;
                this.m_Scale = value;
            }
        }

        /// <summary>
        /// CustomizedImage를 Display할지에 대한 여부를 가져오거나 설정한다.
        /// </summary>
        [Browsable(true)]
        public bool IsViewCustomizedImage
        {
            get { return this.m_IsViewCustomizedImage; }
            set { this.m_IsViewCustomizedImage = value; }
        }

        public OperatingTypes OperatingType
        {
            get { return this.m_OperatingType; }
            set { this.m_OperatingType = value; }
        }

        public bool VisibleCrossLine
        {
            set
            {
                m_bVisibleCrossLine = value;
                if (this.CurrentCamera != null) // 기존: this.Camera != null
                    ShowCrossLine(m_bVisibleCrossLine);
            }
            get
            {
                return m_bVisibleCrossLine;
            }
        }
        #endregion

        #region Event Handlers
        //private void JobManager_SelectedIndexChanged(object sender, VisionJobManagerEventArgs e)
        //{
        //    VisionJobManager manager = sender as VisionJobManager;

        //    if (manager == null) return;

        //    this.SetNormalOverlay(manager);
        //}

        //private void Executor_BeforeJobManagerChange(object sender, VisionToolExecutorEventArgs e)
        //{
        //    VisionToolExecutor executor = sender as VisionToolExecutor;

        //    if (executor == null) return;

        //    if (executor.JobManager == null) return;

        //    executor.JobManager.SelectedIndexChanged -= JobManager_SelectedIndexChanged;
        //}

        //private void Executor_AfterJobManagerChange(object sender, VisionToolExecutorEventArgs e)
        //{
        //    VisionToolExecutor executor = sender as VisionToolExecutor;

        //    if (executor == null) return;

        //    if (executor.JobManager == null) return;

        //    this.SetNormalOverlay(executor.JobManager);

        //    executor.JobManager.SelectedIndexChanged += JobManager_SelectedIndexChanged;
        //}

        private void M_CameraSwitch_AfterChange(object sender, CameraChangeEventArgs e)
        {
            CameraSwitch cameraSwitch = sender as CameraSwitch;
            Camera part = null;

            if (cameraSwitch == null) return;

            try
            {
                this.m_Context = BufferedGraphicsManager.Current;
                this.m_Context.MaximumBuffer = this.Size;
                //lock (ViewerSyncRoot)
                {
                    BufferedGraphics old = null;
                    if (this.m_Graphics != null)
                    {
                        old = this.m_Graphics;
                    }

                    this.m_Graphics = this.m_Context.Allocate(this.CreateGraphics(),
                        new Rectangle(new Point(0, 0), this.Size));
                    if (old != null)
                    {
                        lock (old)
                        {
                            old.Dispose();
                        }
                    }
                }




                this.Scale.SetMousePoint(new PointD(cameraSwitch.Cameras[cameraSwitch.SelectCameraIndex].Resolution.Width / 2, cameraSwitch.Cameras[cameraSwitch.SelectCameraIndex].Resolution.Height / 2));

                part = cameraSwitch.Cameras[cameraSwitch.SelectCameraIndex];

                if (part == null) return;

                //this.SetNormalOverlay(part, part.Executor.JobManager);
            }
            finally
            {
                this.m_IsChanged = true;
            }
        }

        private void VisionImageViewer_ChangingOverlay(object sender, VisionImageOverlayEventArgs e)
        {
            this.m_IsChanged = true;
        }

        private void M_TopCaption_MouseMove(object sender, MouseEventArgs e)
        {
            Label control = sender as Label;
            StringBuilder builder = new StringBuilder();
            Point point = new Point();

            if (this.InputImage == null) return;

            point = this.Scale.GetCenterPoint(e.X + control.Location.X, e.Y + control.Location.Y);

            builder.AppendFormat("Point : {0}, {1} / Pixel : {2}", point.X, point.Y, this.InputImage.GetPixel(point));

            this.m_BottomCaption.Text = builder.ToString();
        }

        private void Item_Click(object sender, EventArgs e)
        {
            ToolStripItem item = sender as ToolStripItem;
            SaveFileDialog dialog = null;
            StringBuilder builder = null;
            VisionImage.FileFilter[] filter = null;
            if (item.Name == MenuItems.ImageLoad.ToString())
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();

                openFileDialog.Filter = "Image Files|*.bmp;*.jpg;*.jpeg;*.png;*.tif;*.tiff";
                openFileDialog.Title = "이미지 불러오기";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    // 파일 확장자 추출
                    string fileExtension = Path.GetExtension(openFileDialog.FileName).ToLower().TrimStart('.');
                    // VisionImage.FileFilter와 매핑
                    VisionImage.FileFilter? selectedFilter = Enum.GetValues(typeof(VisionImage.FileFilter))
                        .Cast<VisionImage.FileFilter>()
                        .FirstOrDefault(f => f.ToString().Equals(fileExtension, StringComparison.OrdinalIgnoreCase));

                    if (selectedFilter.HasValue)
                    {
                        // 적절한 필터로 Load 호출
                        if (this.InputImage == null)
                        {
                            this.InputImage = new VisionImage();
                            this.InputImage.Load(openFileDialog.FileName, selectedFilter.Value);
                        }
                        else
                        {
                            this.InputImage.RawData = null;
                            this.InputImage.Header = new VisionImageHeader();
                            if (this.m_bitmap != null)
                            {
                                this.m_bitmap.Dispose();
                                this.m_bitmap = null;
                            }
                            this.InputImage.Load(openFileDialog.FileName, selectedFilter.Value);
                            CurrentCamera.LatestImage = this.InputImage;
                        }

                        CurrentCamera.IsSimulation = true;
                        Simulated = true;

                        StartUpdateTask();
                        Refresh();
                    }
                    else
                    {
                        MessageBox.Show($"지원하지 않는 파일 형식입니다: {fileExtension}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            else if (item.Name == MenuItems.ImageSave.ToString())
            {
                if (this.InputImage == null || this.InputImage.Header == null || this.InputImage.RawData == null)
                {
                    MessageBox.Show("InputImage is not exist");
                    return;
                }

                if (this.InputImage.Header.Width <= 0 || this.InputImage.Header.Height <= 0)
                {
                    MessageBox.Show("InputImageSize is not vaild");
                    return;
                }

                dialog = new SaveFileDialog();
                builder = new StringBuilder();
                filter = (VisionImage.FileFilter[])Enum.GetValues(typeof(VisionImage.FileFilter));

                for (int i = 0; i < filter.Length; i++)
                {
                    builder.Append(".");
                    builder.Append(filter[i].ToString());
                    builder.Append("|*.");
                    builder.Append(filter[i].ToString());

                    if (filter.Length - 1 == i) continue;
                    builder.Append("|");
                }

                dialog.Filter = builder.ToString();

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    this.InputImage.Save(dialog.FileName, filter[dialog.FilterIndex - 1]);
                }
            }
            else if (item.Name == MenuItems.ImageAutoFit.ToString())
            {
                this.Scale.Wheel = 1.0;

                this.Scale.SetMousePoint(new Point(this.InputImage.Header.Width / 2, this.InputImage.Header.Height / 2));
                this.Scale.MoveCenter(new Size(this.InputImage.Header.Width, this.InputImage.Header.Height));
            }
            else if (item.Name == MenuItems.x2.ToString())
            {
                this.Scale.Wheel = 1.0 / 2.0;

                this.Scale.SetMousePoint(new Point(this.InputImage.Header.Width / 2, this.InputImage.Header.Height / 2));
                this.Scale.MoveCenter(new Size(this.InputImage.Header.Width, this.InputImage.Header.Height));
            }
            else if (item.Name == MenuItems.x4.ToString())
            {
                this.Scale.Wheel = 1.0 / 4.0;

                this.Scale.SetMousePoint(new Point(this.InputImage.Header.Width / 2, this.InputImage.Header.Height / 2));
                this.Scale.MoveCenter(new Size(this.InputImage.Header.Width, this.InputImage.Header.Height));
            }
            else if (item.Name == MenuItems.x8.ToString())
            {
                this.Scale.Wheel = 1.0 / 8.0;

                this.Scale.SetMousePoint(new Point(this.InputImage.Header.Width / 2, this.InputImage.Header.Height / 2));
                this.Scale.MoveCenter(new Size(this.InputImage.Header.Width, this.InputImage.Header.Height));
            }
            else if (item.Name == MenuItems.ResultOverlayClear.ToString())
            {

                this.ResultOverlays.Clear();
                this.Refresh();
                //Module module = Camera.Owner as Module;
                //if (module.ResultOverlays != null)
                //{
                //    module.ResultOverlays.Clear();
                //}

            }
            else if (item.Name == MenuItems.Live.ToString())
            {
                var cam = CurrentCamera;
                if (cam == null) { MessageBox.Show("Camera is not exist"); return; }
                Simulated = false;
                cam.StartLive();
            }
            else if (item.Name == MenuItems.Stop.ToString())
            {
                var cam = CurrentCamera;
                if (cam == null) { MessageBox.Show("Camera is not exist"); return; }
                Simulated = false;
                cam.StopLive();
            }
        }

        private void ContextMenuStrip_Opened(object sender, EventArgs e)
        {
            ContextMenuStrip control = sender as ContextMenuStrip;

            ((ToolStripMenuItem)control.Items[MenuItems.x2.ToString()]).Checked = this.Scale.Wheel == 1.0 / 2.0 ? true : false;
            ((ToolStripMenuItem)control.Items[MenuItems.x4.ToString()]).Checked = this.Scale.Wheel == 1.0 / 4.0 ? true : false;
            ((ToolStripMenuItem)control.Items[MenuItems.x8.ToString()]).Checked = this.Scale.Wheel == 1.0 / 8.0 ? true : false;
            ((ToolStripMenuItem)control.Items[MenuItems.ImageAutoFit.ToString()]).Checked = this.Scale.Wheel == 1 ? true : false;
            ((ToolStripMenuItem)control.Items[MenuItems.Custom.ToString()]).Checked = this.Scale.Wheel != 1 && this.Scale.Wheel != 1.0 / 2.0 && this.Scale.Wheel != 1.0 / 4.0 && this.Scale.Wheel != 1.0 / 8.0 ? true : false;
            ((ToolStripMenuItem)control.Items[MenuItems.ResultOverlayClear.ToString()]).Enabled = this.ResultOverlays.Count != 0 ? true : false;
        }
        #endregion

        #region Method

        public void SetFixDirection(bool bIsFixByWidth)
        {
            m_FixedByWidth = bIsFixByWidth;
        }
        public void InitCrossLine() //private
        {
            var cam = CurrentCamera;
            if (cam == null) return;

            m_HorizentalLine = new LineFrameVisionImageOverlay();
            m_VerticalLine = new LineFrameVisionImageOverlay();

            int nX = cam.Resolution.Width;      // 기존: Camera.Resolution
            int nY = cam.Resolution.Height;

            m_HorizentalLine.StartLocation = new Point(0, nY / 2);
            m_HorizentalLine.EndLocation = new Point(nX, nY / 2);
            m_HorizentalLine.Color = Color.Lime;
            m_HorizentalLine.DashStyle = DashStyle.Dot;
            m_HorizentalLine.Thickness = 1;
            m_HorizentalLine.Visible = true;

            m_VerticalLine.StartLocation = new Point(nX / 2, 0);
            m_VerticalLine.EndLocation = new Point(nX / 2, nY);
            m_VerticalLine.Color = Color.Lime;
            m_VerticalLine.DashStyle = DashStyle.Dot;
            m_VerticalLine.Thickness = 1;
            m_VerticalLine.Visible = true;

        }
        public void ShowCrossLine(bool bVisible)
        {
            // 라인 객체가 없으면 생성
            if (m_HorizentalLine == null || m_VerticalLine == null)
                InitCrossLine();

            if (bVisible)
            {
                if (m_HorizentalLine != null && !NormalOverlays.Contains(m_HorizentalLine))
                    NormalOverlays.Add(m_HorizentalLine);

                if (m_VerticalLine != null && !NormalOverlays.Contains(m_VerticalLine))
                    NormalOverlays.Add(m_VerticalLine);
            }
            else
            {
                if (m_HorizentalLine != null) NormalOverlays.Remove(m_HorizentalLine);
                if (m_VerticalLine != null) NormalOverlays.Remove(m_VerticalLine);
            }
        }
        public static void DisplayAll(Control control)
        {
            VisionImageViewer viewer = null;

            for (int i = 0; i < control.Controls.Count; i++)
            {
                if (control.Controls[i] is VisionImageViewer)
                {
                    viewer = control.Controls[i] as VisionImageViewer;

                    if (viewer == null) continue;

                    if (viewer.Camera != null)
                    {

                    }
                    else if (viewer.CameraSwitch != null)
                    {
                        if (viewer.CameraSwitch.SelectCameraIndex == -1 && viewer.CameraSwitch.Cameras.Count != 0)
                            viewer.CameraSwitch.Change(0);
                    }
                }
                else
                {
                    VisionImageViewer.DisplayAll(control.Controls[i]);
                }
            }
        }

        public static new bool Contains(Control control)
        {
            bool contains = false;

            for (int i = 0; i < control.Controls.Count; i++)
            {
                if (control.Controls[i] is VisionImageViewer)
                {
                    contains = true;
                    break;
                }
                else
                {
                    if ((contains = VisionImageViewer.Contains(control.Controls[i])) == true) break;
                }
            }

            return contains;
        }

        private void RenderForDisplay(BufferedGraphics m_Graphics)
        {
            try
            {
                if (this.m_GraphicsDisplay != null)
                {
                    lock (m_Graphics)

                    {
                        m_Graphics.Render(this.m_GraphicsDisplay);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
            
        }


        /// <summary>
        /// Display를 정지한다.
        /// </summary>
        public void SuspendDisplay()
        {
            this.SuspendedDisplay = true;
        }

        /// <summary>
        /// Display의 정지를 해제한다.
        /// </summary>
        public void ResumeDisplay()
        {
            if (this.SuspendedDisplay == false) return;
            this.SuspendedDisplay = false;
            this.m_IsChanged = true;
        }

        /// <summary>
        /// VisionPart에 정의되어 있는 DefaultOverlays 및 VisionManager에 있는 Overlay를 등록한다.
        /// </summary>
        /// <param name="part"></param>
        /// <param name="manager"></param>
        //private void SetNormalOverlay(VisionPart part, VisionJobManager manager)
        //{
        //    RoiVisionTool tool = null;
        //    Collection<RoiVisionTool> tools = null;

        //    try
        //    {
        //        this.SuspendDisplay();

        //        for (int i = 0; i < this.NormalOverlays.Count; i++)
        //        {
        //            this.NormalOverlays[i].ChangingOverlay -= VisionImageViewer_ChangingOverlay;
        //        }

        //        this.NormalOverlays.Clear();

        //        if (part != null)
        //        {
        //            for (int i = 0; i < part.DefaultOverlays.Count; i++)
        //            {
        //                this.NormalOverlays.Add(part.DefaultOverlays[i]);
        //                part.DefaultOverlays[i].ChangingOverlay += VisionImageViewer_ChangingOverlay;
        //            }
        //        }

        //        if (manager == null) return;

        //        if (manager.SelectedItem == null) return;

        //        for (int i = 0; i < manager.SelectedItem.Count; i++)
        //        {
        //            for (int j = 0; j < manager.SelectedItem[i].Tools.Count; j++)
        //            {
        //                if (manager.SelectedItem[i].Tools[j] is RoiVisionTool)
        //                {
        //                    tool = manager.SelectedItem[i].Tools[j] as RoiVisionTool;

        //                    if (tool.Parameter.Overlay == null)
        //                        tool.Parameter.Overlay = new RectangleFrameVisionImageOverlay(tool.Name, tool.Parameter.StartLocation, tool.Parameter.EndLocation);
        //                    this.NormalOverlays.Add(tool.Parameter.Overlay);
        //                    tool.Parameter.Overlay.ChangingOverlay += VisionImageViewer_ChangingOverlay;
        //                }
        //                else
        //                {
        //                    tools = VisionJob.GetSubVisionTool<RoiVisionTool>(manager.SelectedItem[i].Tools[j]) as Collection<RoiVisionTool>;

        //                    if (tools == null) continue;

        //                    for (int k = 0; k < tools.Count; k++)
        //                    {
        //                        if (tools[k].Parameter.Overlay == null)
        //                            tools[k].Parameter.Overlay = new RectangleFrameVisionImageOverlay(tools[k].Name, tools[k].Parameter.StartLocation, tools[k].Parameter.EndLocation);
        //                        this.NormalOverlays.Add(tools[k].Parameter.Overlay);
        //                        tools[k].Parameter.Overlay.ChangingOverlay += VisionImageViewer_ChangingOverlay;
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    finally
        //    {
        //        this.ResumeDisplay();
        //    }
        //}

        /// <summary>
        /// VisionManager에 있는 Overlay를 등록한다.
        /// </summary>
        /// <param name="manager"></param>
        //private void SetNormalOverlay(VisionJobManager manager)
        //{
        //    VisionPart part = null;
        //    if (this.Camera != null)
        //    {
        //        part = this.Camera.Owner as VisionPart;
        //    }
        //    else if (this.CameraSwitch != null)
        //    {
        //        if (this.CameraSwitch.SelectCameraIndex == -1)
        //            this.CameraSwitch.Change(0);

        //        part = this.CameraSwitch.Cameras[this.CameraSwitch.SelectCameraIndex].Owner as VisionPart;
        //    }

        //    if (part == null) return;

        //    this.SetNormalOverlay(part, manager);
        //}

        private void SetTopCaption()
        {
            BaseComponent component = CurrentCamera as BaseComponent;
            bool isLive = CurrentCamera?.IsLiveOn == true;
            if (component == null) return;

            StringBuilder builder = new StringBuilder();
            //if (this.Camera != null)
            //{
            //    component = this.Camera;

            //    isLive = this.Camera.IsLiveOn;
            //}
            //else if (this.CameraSwitch != null)
            //{
            //    component = this.CameraSwitch.Cameras[this.CameraSwitch.SelectCameraIndex] as BaseComponent;
            //    isLive = this.CameraSwitch.Cameras[this.CameraSwitch.SelectCameraIndex].IsLiveOn;
            //}

            //if (component == null) return;

            builder.AppendFormat("Camera : {0}    ", component.Name);
            //builder.AppendFormat("Live Status : {0}", isLive);
            //this.m_TopCaption.Text = builder.ToString();
        }

        private void DrawToBuffer(BufferedGraphics bufferedGrphics)
        {
            Bitmap resizeImage = null;
            SizeD size;
            PointD point = this.Scale.GetCenterPoint() ;
            VisionImage visionImage = this.InputImage;
            try
            {
                if (visionImage == null)
                    return;

                if (this.m_IsChanged == true)
                {
                    try
                    {
                        //lock (this.ViewerSyncRoot)
                        {
                            size = new SizeD(visionImage.Header.Width * this.Scale.Wheel, visionImage.Header.Height * this.Scale.Wheel);

                            //확인 해봐야함.
                            if (visionImage.CustomizedData != null && this.IsViewCustomizedImage == true)
                            {
                                Bitmap bmpCutImage = (Bitmap)visionImage.CustomizedData.GetVisionImage().CutImage(point, (Size)size);
                                lock (bufferedGrphics)
                                {
                                    if (bufferedGrphics.Graphics != null)
                                    {
                                        bufferedGrphics.Graphics.DrawImage(bmpCutImage, 0, 0, this.Width, this.Height);
                                    }
                                }
                                if (bmpCutImage != null)
                                {
                                    bmpCutImage.Dispose();
                                    bmpCutImage = null;
                                }
                            }
                            else
                            {
                                Bitmap bmpCutImage = null;
                                try
                                {
                                    bmpCutImage = (Bitmap)visionImage.CutImage(point, (Size)size);

                                    if (bmpCutImage == null)
                                    {
                                        Log.Write("VisionViewer", $"CutImage() 실패 - Point: {point}, Size: {size}");
                                        return;
                                    }
                                    bmpCutImage = new Bitmap(bmpCutImage, this.Width, this.Height);

                                    //여기서 계속 Exeption 발생함.
                                    //bmpCutImage = (Bitmap)visionImage.CutImage(point, (Size)size);
                                    //bmpCutImage = new Bitmap(bmpCutImage, this.Width, this.Height);
                                }
                                catch (Exception ex)
                                {
                                    Log.Write(ex);
                                    Console.WriteLine(ex.Message);
                                }

                                lock (bufferedGrphics)
                                {
                                    if (bufferedGrphics.Graphics != null)
                                    {
                                        if(bmpCutImage != null)
                                        {

                                            //bufferedGrphics.Graphics.DrawImage(bmpCutImage, 0, 0, this.Width, this.Height);
                                            bufferedGrphics.Graphics.DrawImageUnscaled(bmpCutImage, 0, 0);

                                        }
                                    }
                                }

                                if (bmpCutImage != null)
                                {
                                    bmpCutImage.Dispose();
                                    bmpCutImage = null;
                                }
                            }

                            lock (bufferedGrphics)
                            {
                                // Cross 그린다.
                                // Normal overlays
                                var resultNormal = this.NormalOverlays;
                                if (resultNormal != null)
                                {
                                    for (int i = 0; i < resultNormal.Count; i++)
                                    {
                                        var ov = resultNormal[i];
                                        if (ov == null) continue;           // ★ null 방어
                                        if (ov.Visible)
                                            ov.Draw(this.Scale.GetOffset(), size,
                                                    new SizeD(this.Size.Width, this.Size.Height), bufferedGrphics);
                                    }
                                }

                                // Result overlays
                                var resultOverlays = this.ResultOverlays;
                                if (resultOverlays != null)
                                {
                                    lock (resultOverlays)
                                    {
                                        for (int i = 0; i < resultOverlays.Count; i++)
                                        {
                                            var ov = resultOverlays[i];
                                            if (ov == null) continue;       // ★ null 방어
                                            if (ov.Visible)
                                                ov.Draw(this.Scale.GetOffset(), size,
                                                        new SizeD(this.Size.Width, this.Size.Height), bufferedGrphics);
                                        }
                                    }
                                }
                            }
                            //lock (bufferedGrphics)
                            //{
                            //    OwnedOverlayCollection resultNormal = this.NormalOverlays;
                            //    try
                            //    {
                            //        for (int i = 0; i < resultNormal.Count; i++)
                            //        {
                            //            if (resultNormal[i].Visible == true)
                            //                resultNormal[i].Draw(this.Scale.GetOffset(), size, new SizeD(this.Size.Width, this.Size.Height), bufferedGrphics);
                            //        }
                            //    }
                            //    catch (Exception ex)
                            //    {
                            //        Log.Write(ex);
                            //    }

                            //    OwnedOverlayCollection resultOverlays = this.ResultOverlays;
                            //    {
                            //        try
                            //        {
                            //            lock(resultOverlays)
                            //            {
                            //                for (int i = 0; i < resultOverlays.Count; i++)
                            //                {
                            //                    lock(bufferedGrphics)
                            //                    {
                            //                        //Todo: 확인 해봐야함.
                            //                        if (resultOverlays[i].Visible == true)
                            //                        {
                            //                            resultOverlays[i].Draw(this.Scale.GetOffset(), size, new SizeD(this.Size.Width, this.Size.Height), bufferedGrphics);
                            //                            //resultOverlays[i].Draw( this.Scale.GetCenterPoint()+ this.Scale.GetOffset(), size, new SizeD(this.Size.Width, this.Size.Height), bufferedGrphics);

                            //                        }

                            //                    }

                            //                }
                            //            }

                            //        }
                            //        catch (Exception ex)
                            //        {
                            //            Log.Write(ex);
                            //        }
                            //    }
                            //}
                        }

                        this.m_IsChanged = false;
                    }
                    catch (Exception ex)
                    {
                        Log.Write(ex);
                        
                        Console.WriteLine(ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                
                Console.WriteLine(ex.Message);
            }
            finally
            {
                if (resizeImage != null)
                {
                    resizeImage.Dispose();
                    resizeImage = null;
                }

            }

        }
        #endregion

        #region Control Members
        protected override void OnCreateControl()
        {
            MenuItems[] items = (MenuItems[])Enum.GetValues(typeof(MenuItems));
            ToolStripMenuItem item = null;

            base.OnCreateControl();

            // Design mode 
            if (this.DesignMode) return;

            if (this.FrameRate == 0)
                this.FrameRate = 1;

            #region Initial
            this.ViewerSyncRoot = new object();
            this.BackColor = Color.Black;
            this.SizeMode = PictureBoxSizeMode.StretchImage;
            this.m_IsChanged = true;
            #endregion

            #region Label Control
            this.Controls.Clear();

            this.m_TopCaption = new Label();
            this.m_TopCaption.Dock = DockStyle.Top;
            this.m_TopCaption.Height = 0;
            this.m_TopCaption.ForeColor = Color.Lime;
            this.m_TopCaption.BackColor = Color.Transparent;
            this.m_TopCaption.MouseMove += M_TopCaption_MouseMove;
            this.Controls.Add(this.m_TopCaption);

            this.m_BottomCaption = new Label();
            this.m_BottomCaption.Dock = DockStyle.Bottom;
            this.m_BottomCaption.Height = 0;
            this.m_BottomCaption.ForeColor = Color.Lime;
            this.m_BottomCaption.BackColor = Color.Transparent;
            this.m_BottomCaption.MouseMove += M_TopCaption_MouseMove;
            this.m_BottomCaption.Visible = false;
            this.Controls.Add(this.m_BottomCaption);
            #endregion

            #region Menu Strip
            this.ContextMenuStrip = new ContextMenuStrip();
            for (int i = 0; i < items.Length; i++)
            {
                item = new ToolStripMenuItem();
                item.Text = AbbreviationAttribute.GetAbbreviation(items[i]);
                item.Name = items[i].ToString();
                item.Click += Item_Click;

                this.ContextMenuStrip.Items.Add(item);

                if (items[i] == MenuItems.ImageSave || items[i] == MenuItems.ResultOverlayClear)
                {
                    this.ContextMenuStrip.Items.Add(new ToolStripSeparator());
                }
            }

            this.ContextMenuStrip.Opened += ContextMenuStrip_Opened;

            #endregion
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            PointD previousPoint = new PointD();
            ResumeDisplay();
            base.OnMouseWheel(e);

            if (this.InputImage != null)
            {
                if (e.Delta < 0)
                {
                    previousPoint = this.Scale.GetPoint(e.X, e.Y);

                    this.Scale.Wheel = this.Scale.Wheel * 1.05;

                    if (1.0 < this.Scale.Wheel)
                    {
                        this.Scale.Wheel = this.Scale.Wheel / 1.05;
                        return;
                    }
                    else if (1 / 1.05 < this.Scale.Wheel && this.Scale.Wheel < 1)
                    {
                        this.Scale.VerticalShift.Offset = 0;
                        this.Scale.HorizontalShift.Offset = 0;
                        this.Scale.Wheel = 1;
                        this.Scale.SetMousePoint(new Point(this.InputImage.Header.Width / 2, this.InputImage.Header.Height / 2));
                        return;
                    }
                }
                else
                {
                    previousPoint = this.Scale.GetPoint(e.X, e.Y);

                    this.Scale.Wheel = this.Scale.Wheel / 1.05;
                }

                if (this.OperatingType == OperatingTypes.Center)
                {
                    //this.Scale.Convert(new PointD(this.m_InputImage.Header.Width / 2, this.m_InputImage.Header.Height / 2), new Size(this.m_InputImage.Header.Width, this.m_InputImage.Header.Height));
                    this.Scale.SetMousePoint(new Point(this.InputImage.Header.Width / 2, this.InputImage.Header.Height / 2));
                    this.Scale.MoveCenter(new Size(this.InputImage.Header.Width, this.InputImage.Header.Height));
                }
                else if (this.OperatingType == OperatingTypes.Mouse_Move)
                {
                    this.Scale.Convert(previousPoint, new Size(this.InputImage.Header.Width, this.InputImage.Header.Height));
                }
                this.m_IsChanged = true;
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            StringBuilder builder = new StringBuilder();
            Point point = new Point();

            base.OnMouseMove(e);

            if (this.InputImage == null) return;

            point = this.Scale.GetCenterPoint(e.X, e.Y);

            builder.AppendFormat("Point : {0}, {1} / Pixel : {2}", point.X, point.Y, this.InputImage.GetPixel(point));

            this.m_BottomCaption.Text = builder.ToString();

        }

        protected override void OnMouseHover(EventArgs e)
        {
            base.OnMouseHover(e);

            this.Focus();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);

            this.OnLostFocus(new EventArgs());
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            VisionImage image = null;

            base.OnMouseDoubleClick(e);

            if (this.CameraSwitch != null)
            {
                if (e.Button == MouseButtons.Left)
                {
                    if (e.X <= this.Width / 2)
                        this.CameraSwitch.ChangePrevious(false, false);
                    else
                        this.CameraSwitch.ChangeNext(false, false);

                    this.CameraSwitch.Cameras[this.CameraSwitch.SelectCameraIndex].GrabSync(out image);
                    this.InputImage = image;
                }
            }
            else if (CurrentCamera != null)
            {
                this.InputImage = CurrentLatestImage;
            }
            else
            {
                if (this.Camera != null)
                    this.InputImage = this.Camera.LatestImage;
            }
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);

            if (e.Button == MouseButtons.Right)
            {
                this.ContextMenuStrip.Show(e.X, e.Y);
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            this.m_PreviousPoint = this.Scale.GetPoint(e.X, e.Y);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            try
            {

                PointD currentPoint = this.Scale.GetPoint(e.X, e.Y);
                PointD distance = new PointD();

                var latest = CurrentLatestImage;
                if (latest == null || this.InputImage == null) return;
                //if (this.InputImage == null) return;

                base.OnMouseUp(e);
                //double dScale = this.Camera.LatestImage.Header.Width / this.Width;
                double dScale = latest.Header.Width / (double)this.Width;
                distance = (this.m_PreviousPoint - currentPoint) * dScale;

                distance += new PointD(this.Scale.HorizontalShift.Offset, this.Scale.VerticalShift.Offset);

                this.Scale.SetOffsetAndCenterPoint(distance, new Size(this.InputImage.Header.Width, this.InputImage.Header.Height));
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            if (this.m_Graphics != null)
            {
                lock (this.m_Graphics)
                {
                    this.m_Graphics.Render(pe.Graphics);
                }

            }

        }
        #endregion
        private static object objLock = new object();
        private Task m_task;
        private bool m_bStop;
        private CancellationTokenSource cts = null;
        private static int TaskNO = 0;
        public void StartUpdateTask()
        {
            ResumeDisplay();
            if (m_task != null) return;

            m_GraphicsDisplay = this.CreateGraphics();

            m_task = Task.Factory.StartNew(() =>
            {
                lock (objLock) { TaskNO++; Thread.CurrentThread.Name = "VisionImageViewer StartUpdateTask" + TaskNO; }

                while (true)
                {
                    if (m_bStop) break;
                    if (this.SuspendedDisplay) { Thread.Sleep(UpdateDelayTime); continue; }

                    if (!Simulated)
                    {
                        var cam = CurrentCamera;
                        if (cam != null && cam.Opened && !cam.SuspendedImageDisplay)
                        {
                            var latest = CurrentLatestImage;   // = cam?.LatestImage
                            if (latest != null)
                            {
                                // 해상도 변경 시 크로스라인/스케일 보정
                                if (m_InputImage != null && m_InputImage.Header.Width != latest.Header.Width)
                                {
                                    Scale.SetMousePoint(new Point(latest.Header.Width / 2, latest.Header.Height / 2));

                                    int nX = latest.Header.Width;
                                    int nY = latest.Header.Height;
                                    m_HorizentalLine.StartLocation = new Point(0, nY / 2);
                                    m_HorizentalLine.EndLocation = new Point(nX, nY / 2);
                                    m_VerticalLine.StartLocation = new Point(nX / 2, 0);
                                    m_VerticalLine.EndLocation = new Point(nX / 2, nY);
                                }

                                m_InputImage = latest;
                                m_IsChanged = true;

                                UpdateOverlay(false);
                                DrawToBuffer(m_Graphics);
                                RenderForDisplay(m_Graphics);
                            }
                        }
                    }
                    else
                    {
                        // 파일 로드(시뮬레이션) 모드: m_InputImage만 그린다
                        if (m_InputImage != null)
                        {
                            m_IsChanged = true;
                            UpdateOverlay(false);
                            DrawToBuffer(m_Graphics);
                            RenderForDisplay(m_Graphics);
                        }
                    }

                    Thread.Sleep(UpdateDelayTime);
                }
            });
        }

        public void ClearOveray()
        {
            ResultOverlays.Clear();
            m_ResultOverlayCollection = null;
        }
        private void UpdateOverlay(bool bRedraw = true)
        {
            //this.ResultOverlays.Clear();
            BaseUnit unit = CurrentCamera.ParentUnit as BaseUnit;
            //BaseUnit unit = this.Camera.ParentUnit as BaseUnit;
            //this.ResultOverlays = module.ResultOverlays;
            //if(this.m_ResultOverlayCollection != module.ResultOverlays)
            {
                lock (this.ViewerSyncRoot)
                    try
                    {
                        //if (module.ResultOverlays != null)
                        //{
                        //    ResultOverlayCollection ROC = module.ResultOverlays;
                        //    lock (ROC)
                        //    {
                        //        this.ResultOverlays.Clear();
                        //        foreach (var overlay in ROC)
                        //        {
                        //            this.ResultOverlays.Add(overlay);
                        //            overlay.Visible = true;
                        //        }
                        //    }
                            
                        //}
                    }
                    catch (Exception ex)
                    {
                        Log.Write(ex);
                    }
            }
        }

        public void StopUpdateTask()
        {
            this.SuspendDisplay();
        }

        public Camera CurrentCamera =>
                (this.m_CameraSwitch != null && this.m_CameraSwitch.SelectCameraIndex != -1)
                ? this.m_CameraSwitch.Cameras[this.m_CameraSwitch.SelectCameraIndex]
                : this.m_Camera;

        private VisionImage CurrentLatestImage => CurrentCamera?.LatestImage;

    }
}