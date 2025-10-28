/*
 * Purpose
 *      QMC 회사만의 Vision Image Overlay Class에 대해서 정의한다.
 *      
 * Remark
 *      Image에 Overlay될 텍스트 또는 선, 도형등의 구조를 설계한다.
 *      
 * Reference
 *      
 * Revision
 *      1. Created: 2017.12.28 JUNG.CY
 *      2. Modify : 2019.01.04 JUNG.CY
 *          - 불필요한 코드 제거 및 최적화.
 *          - Scale 적용된 오버레이 Draw 기능 추가.
 * 
 */

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Collections.ObjectModel;


namespace QMC.Common.Vision
{
    #region VisionImageOverlay
    [Serializable]
    public abstract class VisionImageOverlay : MarshalByRefObject
    {
        #region Field
        private string m_Name;
        private bool m_Visible;
        #endregion

        #region Events
        [field: NonSerialized]
        public event VisionImageOverlayEventHandler ChangingOverlay;
        #endregion

        #region Constructor
        public VisionImageOverlay(string name)
        {
            this.Name = name;
            this.Visible = false;
        }
        public VisionImageOverlay() : this("") { }
        #endregion

        #region Property
        /// <summary>
        /// Overlay될 대상의 이름을 가져오거나 설정한다.
        /// </summary>
        public string Name
        {
            get { return this.m_Name; }
            private set { this.m_Name = value; }
        }

        /// <summary>
        /// Overlay될 대상의 표시여부를 가져오거나 설정한다.
        /// </summary>
        public bool Visible
        {
            get { return this.m_Visible; }
            set
            {
                if (this.m_Visible == value) return;
                this.m_Visible = value;

                this.OnChangingOverlay(new VisionImageOverlayEventArgs());
            }
        }
        #endregion

        #region Method
        protected virtual void OnChangingOverlay(VisionImageOverlayEventArgs e)
        {
            if (this.ChangingOverlay != null)
                this.ChangingOverlay(this, e);
        }

        public void Draw(Graphics graphics)
        {
            if (this.Visible == false) return;

            this.Draw(new Point(0, 0), graphics);
        }

        public void Draw(BufferedGraphics graphics)
        {
            if (this.Visible == false) return;

            this.Draw(new Point(0, 0), graphics);
        }

        public void Draw(SizeD sourceSize, SizeD destinateSize, Graphics graphics)
        {
            if (this.Visible == false) return;

            this.Draw(new Point(0, 0), sourceSize, destinateSize, graphics);
        }

        public void Draw(SizeD sourceSize, SizeD destinateSize, BufferedGraphics graphics)
        {
            if (this.Visible == false) return;

            this.Draw(new Point(0, 0), sourceSize, destinateSize, graphics);
        }

        public void Draw(Point offset, Graphics graphics)
        {
            if (this.Visible == false) return;

            this.OnDraw(offset, null, null, graphics);
        }

        public void Draw(Point offset, BufferedGraphics graphics)
        {
            if (this.Visible == false) return;

            this.OnDraw(offset, null, null, graphics);
        }

        public void Draw(Point offset, SizeD sourceSize, SizeD destinateSize, Graphics graphics)
        {
            if (this.Visible == false) return;

            this.OnDraw(offset, sourceSize, destinateSize, graphics);
        }

        public void Draw(Point offset, SizeD sourceSize, SizeD destinateSize, BufferedGraphics graphics)
        {
            if (this.Visible == false) return;

            this.OnDraw(offset, sourceSize, destinateSize, graphics);
        }

        protected abstract void OnDraw(Point offset, SizeD? sourceSize, SizeD? destinateSize, Graphics graphics);
        protected abstract void OnDraw(Point offset, SizeD? sourceSize, SizeD? destinateSize, BufferedGraphics graphics);
        #endregion
    }

    [Serializable]
    public class VisionImageOverlayCollection : Collection<VisionImageOverlay>
    {

    }
    #endregion

    #region TextVisionImageOverlay
    [Serializable]
    public class TextVisionImageOverlay : FrameVisionImageOverlay
    {
        #region Field
        private Font m_FontStyle;
        private Brush m_BrushColor;
        private Point m_StartLocation;
        private string m_Text;
        #endregion

        #region Constructor
        public TextVisionImageOverlay(string name, Point startLocation, Font fontStyle) : base(name)
        {
            this.Text = name;
            this.FontStyle = fontStyle;
            this.StartLocation = startLocation;
            this.BrushColor = Brushes.LimeGreen;
        }
        public TextVisionImageOverlay(string name, Point startLocation) : this(name, startLocation, new Font(FontFamily.GenericMonospace, 10)) { }
        public TextVisionImageOverlay(string name) : this(name, new Point()) { }
        public TextVisionImageOverlay() : this("") { }
        #endregion

        #region Property
        /// <summary>
        /// Overlay될 Text의 Font를 가져오거나 설정한다.
        /// </summary>
        public Font FontStyle
        {
            get { return this.m_FontStyle; }
            set
            {
                if (this.m_FontStyle == value) return;
                if(this.m_FontStyle != null)
                {
                    this.m_FontStyle.Dispose();
                }
                this.m_FontStyle = value;
                this.OnChangingOverlay(new VisionImageOverlayEventArgs());
            }
        }
        /// <summary>
        /// Overlay될 Text의 시작위치를 가져오거나 설정한다. 
        /// </summary>
        public Point StartLocation
        {
            get { return this.m_StartLocation; }
            set
            {
                if (this.m_StartLocation == value) return;
                this.m_StartLocation = value;
                this.OnChangingOverlay(new VisionImageOverlayEventArgs());
            }
        }
        /// <summary>
        /// Overlay될 Text를 가져오거나 설정한다. 
        /// </summary>
        public string Text
        {
            get { return this.m_Text; }
            set
            {
                if (this.m_Text == value) return;
                this.m_Text = value;
                this.OnChangingOverlay(new VisionImageOverlayEventArgs());
            }
        }
        /// <summary>
        /// Overlay될 Text의 Brush의 색상을 가져오거나 설정한다. 
        /// </summary>
        public Brush BrushColor
        {
            get { return this.m_BrushColor; }
            set
            {
                if (this.m_BrushColor == value) return;
                this.m_BrushColor = value;
                this.OnChangingOverlay(new VisionImageOverlayEventArgs());
            }
        }
        #endregion

        #region Overlay Members
        protected override void OnChangingOverlay(VisionImageOverlayEventArgs e)
        {
            base.OnChangingOverlay(e);
        }

        protected override void OnDraw(Point offset, SizeD? sourceSize, SizeD? destinateSize, Graphics graphics)
        {
            SizeD scale = SizeD.Empty;
            Font font = null;
            Point point = Point.Empty;
            double tempWidth = 0.0;
            double tempHeight = 0.0;

            if (sourceSize == null || destinateSize == null)
                scale = new SizeD(1, 1);
            else
            {
                tempWidth = destinateSize.Value.Width / sourceSize.Value.Width;
                tempHeight = destinateSize.Value.Height / sourceSize.Value.Height;
                scale = new SizeD(tempWidth, tempHeight);
            }

            point = new PointD((this.StartLocation.X - offset.X) * scale.Width, (this.StartLocation.Y - offset.Y) * scale.Height);
            font = new Font(this.FontStyle.FontFamily, (float)(this.FontStyle.Size * scale.Width));

            graphics.DrawString(this.Text, font, this.BrushColor, point);

            font.Dispose();
            
        }

        protected override void OnDraw(Point offset, SizeD? sourceSize, SizeD? destinateSize, BufferedGraphics graphics)
        {
            SizeD scale = SizeD.Empty;
            Font font = null;
            Point point = Point.Empty;
            SolidBrush brushColor;
        double tempWidth = 0.0;
            double tempHeight = 0.0;
            if (sourceSize == null || destinateSize == null)
                scale = new SizeD(1, 1);
            else
            {
                tempWidth = destinateSize.Value.Width / sourceSize.Value.Width;
                tempHeight = destinateSize.Value.Height / sourceSize.Value.Height;
                scale = new SizeD(tempWidth, tempHeight);
            }
            point = new PointD((this.StartLocation.X - offset.X) * scale.Width, (this.StartLocation.Y - offset.Y) * scale.Height);
            font = new Font(this.FontStyle.FontFamily, (float)(this.FontStyle.Size * scale.Width));
            brushColor = new SolidBrush(this.Color);
            if (graphics != null)
            {
                if (graphics.Graphics != null)
                {
                    //graphics.Graphics.DrawString(this.Text, font, this.BrushColor, point);
                    graphics.Graphics.DrawString(this.Text, font, brushColor, point);
                }
            }
            font.Dispose();
            brushColor.Dispose();   
        }
        #endregion
    }
    #endregion

    #region PointVisionImageOverlay
    [Serializable]
    public class PointVisionImageOverlay : VisionImageOverlay
    {
        #region Field
        private Point m_CenterLocation;
        private Color m_Color;
        #endregion

        #region Constructor
        public PointVisionImageOverlay(string name) : base(name)
        {
            this.Color = Color.Red;
        }
        public PointVisionImageOverlay() : this("") { }
        #endregion

        #region Property
        /// <summary>
        /// Overlay될 Frame의 중심 좌표를 가져오거나 설정한다.
        /// </summary>
        public Point CenterLocation
        {
            get { return this.m_CenterLocation; }
            set
            {
                if (this.m_CenterLocation == value) return;
                this.m_CenterLocation = value;
                this.OnChangingOverlay(new VisionImageOverlayEventArgs());
            }
        }

        /// <summary>
        /// Overlay될 Frame의 색상을 가져오거나 설정한다.
        /// </summary>
        public Color Color
        {
            get { return this.m_Color; }
            set
            {
                if (this.m_Color == value) return;
                this.m_Color = value;
                this.OnChangingOverlay(new VisionImageOverlayEventArgs());
            }
        }
        #endregion

        #region Overlay Members
        protected override void OnChangingOverlay(VisionImageOverlayEventArgs e)
        {
            base.OnChangingOverlay(e);
        }

        protected override void OnDraw(Point offset, SizeD? sourceSize, SizeD? destinateSize, Graphics graphics)
        {
            SizeD scale = SizeD.Empty;
            Pen pen = null;
            Point start = Point.Empty;
            Point end = Point.Empty;
            double tempWidth = 0.0;
            double tempHeight = 0.0;

            if (sourceSize == null || destinateSize == null)
                scale = new SizeD(1, 1);
            else
            {
                tempWidth = destinateSize.Value.Width / sourceSize.Value.Width;
                tempHeight = destinateSize.Value.Height / sourceSize.Value.Height;
                scale = new SizeD(tempWidth, tempHeight);
            }

            pen = new Pen(this.Color, 1);
            pen.DashStyle = DashStyle.Solid;

            start = new PointD((this.CenterLocation.X - offset.X - 10) * scale.Width, (this.CenterLocation.Y - offset.Y) * scale.Height);
            end = new PointD((this.CenterLocation.X - offset.X + 10) * scale.Width, (this.CenterLocation.Y - offset.Y) * scale.Height);

            graphics.DrawLine(pen, start, end);

            start = new PointD((this.CenterLocation.X - offset.X) * scale.Width, (this.CenterLocation.Y - offset.Y - 10) * scale.Height);
            end = new PointD((this.CenterLocation.X - offset.X) * scale.Width, (this.CenterLocation.Y - offset.Y + 10) * scale.Height);

            graphics.DrawLine(pen, start, end);

            pen.Dispose();
        }

        protected override void OnDraw(Point offset, SizeD? sourceSize, SizeD? destinateSize, BufferedGraphics graphics)
        {
            SizeD scale = SizeD.Empty;
            Pen pen = null;
            Point start = Point.Empty;
            Point end = Point.Empty;
            double tempWidth = 0.0;
            double tempHeight = 0.0;

            if (sourceSize == null || destinateSize == null)
                scale = new SizeD(1, 1);
            else
            {
                tempWidth = destinateSize.Value.Width / sourceSize.Value.Width;
                tempHeight = destinateSize.Value.Height / sourceSize.Value.Height;
                scale = new SizeD(tempWidth, tempHeight);
            }

            pen = new Pen(this.Color, 1);
            pen.DashStyle = DashStyle.Solid;

            start = new PointD((this.CenterLocation.X - offset.X - 10) * scale.Width, (this.CenterLocation.Y - offset.Y) * scale.Height);
            end = new PointD((this.CenterLocation.X - offset.X + 10) * scale.Width, (this.CenterLocation.Y - offset.Y) * scale.Height);
            if (graphics != null)
            {
                if (graphics.Graphics != null)
                {
                    graphics.Graphics.DrawLine(pen, start, end);
                }
            }

            start = new PointD((this.CenterLocation.X - offset.X) * scale.Width, (this.CenterLocation.Y - offset.Y - 10) * scale.Height);
            end = new PointD((this.CenterLocation.X - offset.X) * scale.Width, (this.CenterLocation.Y - offset.Y + 10) * scale.Height);
            if (graphics != null)
            {
                if (graphics.Graphics != null)
                {
                    graphics.Graphics.DrawLine(pen, start, end);
                }
            }

            pen.Dispose();
        }
        #endregion
    }
    #endregion

    #region FrameVisionImageOverlay
    [Serializable]
    public abstract class FrameVisionImageOverlay : VisionImageOverlay
    {
        #region Field
        private Point m_CenterLocation;
        private Color m_Color;
        private Point m_StartLocation;
        private Point m_EndLocation;
        private int m_Thickness;
        private DashStyle m_DashStyle;
        private Size m_Size;
        #endregion

        #region Constructor
        public FrameVisionImageOverlay(string name) : base(name)
        {
            this.Color = Color.Red;
            this.Thickness = 2;
            this.DashStyle = DashStyle.Solid;
        }
        public FrameVisionImageOverlay() : this("") { }
        #endregion

        #region Property
        /// <summary>
        /// Overlay될 Frame의 중심 좌표를 가져오거나 설정한다.
        /// </summary>
        public Point CenterLocation
        {
            get { return this.m_CenterLocation; }
            set
            {
                if (this.m_CenterLocation == value) return;
                this.m_StartLocation = new Point(value.X - this.m_Size.Width / 2, value.Y - this.m_Size.Height / 2);
                this.m_EndLocation = new Point(value.X + this.m_Size.Width / 2, value.Y + this.m_Size.Height / 2);
                this.m_CenterLocation = value;
                this.OnChangingOverlay(new VisionImageOverlayEventArgs());
            }
        }

        /// <summary>
        /// Overlay될 Frame의 색상을 가져오거나 설정한다.
        /// </summary>
        public Color Color
        {
            get { return this.m_Color; }
            set
            {
                if (this.m_Color == value) return;
                this.m_Color = value;
                this.OnChangingOverlay(new VisionImageOverlayEventArgs());
            }
        }

        /// <summary>
        /// Overlay될 Frame의 시작 좌표를 가져오거나 설정한다.
        /// </summary>
        public Point StartLocation
        {
            get { return this.m_StartLocation; }
            set
            {
                if (this.m_StartLocation == value) return;
                this.m_Size = new Size(this.m_EndLocation.X - value.X, this.m_EndLocation.Y - value.Y);
                this.m_CenterLocation = new Point((this.m_EndLocation.X - value.X) / 2, (this.m_EndLocation.Y - value.Y) / 2);
                this.m_StartLocation = value;
                this.OnChangingOverlay(new VisionImageOverlayEventArgs());
            }
        }

        /// <summary>
        /// Overlay될 Frame의 끝 좌표를 가져오거나 설정한다.
        /// </summary>
        public Point EndLocation
        {
            get { return this.m_EndLocation; }
            set
            {
                if (this.m_EndLocation == value) return;
                this.m_Size = new Size(value.X - this.m_StartLocation.X, value.Y - this.m_StartLocation.Y);
                this.m_CenterLocation = new Point((value.X - this.m_StartLocation.X) / 2, (value.Y - this.m_StartLocation.Y) / 2);
                this.m_EndLocation = value;
                this.OnChangingOverlay(new VisionImageOverlayEventArgs());
            }
        }

        /// <summary>
        /// Overlay될 Frame의 두께를 가져오거나 설정한다.
        /// </summary>
        public int Thickness
        {
            get { return this.m_Thickness; }
            set
            {
                if (this.m_Thickness == value) return;
                this.m_Thickness = value;
                this.OnChangingOverlay(new VisionImageOverlayEventArgs());
            }
        }

        /// <summary>
        /// Overlay될 Frame의 선 스타일을 가져오거나 설정한다.
        /// </summary>
        public DashStyle DashStyle
        {
            get { return this.m_DashStyle; }
            set
            {
                if (this.m_DashStyle == value) return;
                this.m_DashStyle = value;
                this.OnChangingOverlay(new VisionImageOverlayEventArgs());
            }
        }

        /// <summary>
        /// Overlay될 Frame의 Size를 가져오거나 설정한다.
        /// </summary>
        public Size Size
        {
            get { return this.m_Size; }
            set
            {
                if (this.m_Size == value) return;
                this.m_EndLocation = new Point(this.m_StartLocation.X + value.Width, this.m_StartLocation.Y + value.Height);
                this.m_CenterLocation = new Point(this.m_StartLocation.X + value.Width / 2, this.m_StartLocation.Y + value.Height / 2);
                this.m_Size = value;
                this.OnChangingOverlay(new VisionImageOverlayEventArgs());
            }
        }
        #endregion

        #region Overlay Members
        protected override void OnChangingOverlay(VisionImageOverlayEventArgs e)
        {
            base.OnChangingOverlay(e);
        }
        #endregion
    }

    [Serializable]
    public class FrameVisionImageOverlayCollection : Collection<FrameVisionImageOverlay>
    {

    }
    #endregion

    #region LineFrameVisionImageOverlay
    [Serializable]
    public class LineFrameVisionImageOverlay : FrameVisionImageOverlay
    {
        #region Constructor
        public LineFrameVisionImageOverlay(string name, Point startLocation, Point endLocation, Color color, DashStyle dashStyle) : base(name)
        {
            this.StartLocation = startLocation;
            this.EndLocation = endLocation;
            this.Color = color;
            this.DashStyle = dashStyle;
        }
        public LineFrameVisionImageOverlay(string name, Point startLocation, Point endLocation, Color color) : this(name, startLocation, endLocation, color, DashStyle.Solid) { }
        public LineFrameVisionImageOverlay(string name, Point startLocation, Point endLocation) : this(name, startLocation, endLocation, Color.Lime) { }
        public LineFrameVisionImageOverlay(string name) : this(name, new Point(), new Point(100, 100)) { }
        public LineFrameVisionImageOverlay() : this("") { }
        #endregion

        #region Overlay Members
        protected override void OnChangingOverlay(VisionImageOverlayEventArgs e)
        {
            base.OnChangingOverlay(e);
        }

        protected override void OnDraw(Point offset, SizeD? sourceSize, SizeD? destinateSize, Graphics graphics)
        {
            SizeD scale = SizeD.Empty;
            Pen pen = null;
            Point start = Point.Empty;
            Point end = Point.Empty;
            double tempWidth = 0.0;
            double tempHeight = 0.0;

            if (sourceSize == null || destinateSize == null)
                scale = new SizeD(1, 1);
            else
            {
                tempWidth = destinateSize.Value.Width / sourceSize.Value.Width;
                tempHeight = destinateSize.Value.Height / sourceSize.Value.Height;
                scale = new SizeD(tempWidth, tempHeight);
            }

            pen = new Pen(this.Color, this.Thickness);
            pen.DashStyle = this.DashStyle;

            start = new PointD((this.StartLocation.X - offset.X) * scale.Width, (this.StartLocation.Y - offset.Y) * scale.Height);
            end = new PointD((this.EndLocation.X - offset.X) * scale.Width, (this.EndLocation.Y - offset.Y) * scale.Height);

            graphics.DrawLine(pen, start, end);

            pen.Dispose();
        }

        protected override void OnDraw(Point offset, SizeD? sourceSize, SizeD? destinateSize, BufferedGraphics graphics)
        {
            SizeD scale = SizeD.Empty;
            Pen pen = null;
            Point start = Point.Empty;
            Point end = Point.Empty;
            double tempWidth = 0.0;
            double tempHeight = 0.0;

            if (sourceSize == null || destinateSize == null)
                scale = new SizeD(1, 1);
            else
            {
                tempWidth = destinateSize.Value.Width / sourceSize.Value.Width;
                tempHeight = destinateSize.Value.Height / sourceSize.Value.Height;
                scale = new SizeD(tempWidth, tempHeight);
            }

            pen = new Pen(this.Color, this.Thickness);
            pen.DashStyle = this.DashStyle;

            start = new PointD((this.StartLocation.X - offset.X) * scale.Width, (this.StartLocation.Y - offset.Y) * scale.Height);
            end = new PointD((this.EndLocation.X - offset.X) * scale.Width, (this.EndLocation.Y - offset.Y) * scale.Height);
            if(graphics !=null)
            {
                if(graphics.Graphics!=null)
                {

                    graphics.Graphics.DrawLine(pen, start, end);
                }
            }
            

            pen.Dispose();
        }
        #endregion
    }
    #endregion

    #region RectangleFrameVisionImageOverlay
    [Serializable]
    public class RectangleFrameVisionImageOverlay : FrameVisionImageOverlay
    {
        #region Constructor
        public RectangleFrameVisionImageOverlay(string name, Point startLocation, Point endLocation) : base(name)
        {
            this.StartLocation = startLocation;
            this.EndLocation = endLocation;
            this.Size = new Size(this.EndLocation.X - this.StartLocation.X, this.EndLocation.Y - this.StartLocation.Y);
            this.CenterLocation = new Point(this.EndLocation.X - this.Size.Width / 2, this.EndLocation.Y - this.Size.Height / 2);
        }
        public RectangleFrameVisionImageOverlay(string name, PointD startLocation, PointD endLocation) : this(name, new Point((int)startLocation.X, (int)startLocation.Y), new Point((int)endLocation.X, (int)endLocation.Y)) { }
        public RectangleFrameVisionImageOverlay(string name, Point centerLocation, Size size) : base(name)
        {
            this.CenterLocation = centerLocation;
            this.Size = size;
            this.StartLocation = new Point(this.CenterLocation.X - this.Size.Width / 2, this.CenterLocation.Y - this.Size.Height / 2);
            this.EndLocation = new Point(this.CenterLocation.X + this.Size.Width / 2, this.CenterLocation.Y + this.Size.Height / 2);
            this.DashStyle = DashStyle.Dash;
        }
        public RectangleFrameVisionImageOverlay(string name, PointD centerLocation, SizeD size) : this(name, new Point((int)centerLocation.X, (int)centerLocation.Y), new Size((int)size.Width, (int)size.Height)) { }
        public RectangleFrameVisionImageOverlay(string name, int x, int y, int width, int height) : this(name, new Point(x, y), new Size(width, height)) { }
        public RectangleFrameVisionImageOverlay(string name, double x, double y, double width, double height) : this(name, new PointD(x, y), new SizeD(width, height)) { }
        public RectangleFrameVisionImageOverlay(string name) : this(name, 100, 100, 100, 100) { }
        public RectangleFrameVisionImageOverlay() : this("") { }
        #endregion

        #region Property
        #endregion

        #region Overlay Members
        protected override void OnChangingOverlay(VisionImageOverlayEventArgs e)
        {
            base.OnChangingOverlay(e);
        }

        protected override void OnDraw(Point offset, SizeD? sourceSize, SizeD? destinateSize, Graphics graphics)
        {
            SizeD scale = SizeD.Empty;
            SizeD size = SizeD.Empty;
            Pen pen = null;
            PointD point = PointD.Empty;
            double tempWidth = 0.0;
            double tempHeight = 0.0;

            if (sourceSize == null || destinateSize == null)
                scale = new SizeD(1, 1);
            else
            {
                tempWidth = destinateSize.Value.Width / sourceSize.Value.Width;
                tempHeight = destinateSize.Value.Height / sourceSize.Value.Height;
                scale = new SizeD(tempWidth, tempHeight);
            }

            pen = new Pen(this.Color, this.Thickness);
            point = new PointD((this.StartLocation.X - offset.X) * scale.Width, (this.StartLocation.Y - offset.Y) * scale.Height);
            size = new SizeD(this.Size.Width * scale.Width, this.Size.Height * scale.Height);

            graphics.DrawRectangle(pen, new Rectangle(point, (Size) size));

            pen.Dispose();
        }

        protected override void OnDraw(Point offset, SizeD? sourceSize, SizeD? destinateSize, BufferedGraphics graphics)
        {
            SizeD scale = SizeD.Empty;
            SizeD size = SizeD.Empty;
            Pen pen = null;
            PointD point = PointD.Empty;
            double tempWidth = 0.0;
            double tempHeight = 0.0;

            if (sourceSize == null || destinateSize == null)
                scale = new SizeD(1, 1);
            else
            {
                tempWidth = destinateSize.Value.Width / sourceSize.Value.Width;
                tempHeight = destinateSize.Value.Height / sourceSize.Value.Height;
                scale = new SizeD(tempWidth, tempHeight);
            }

            pen = new Pen(this.Color, this.Thickness);
            point = new PointD((this.StartLocation.X - offset.X) * scale.Width, (this.StartLocation.Y - offset.Y) * scale.Height);
            size = new SizeD(this.Size.Width * scale.Width, this.Size.Height * scale.Height);
            if (graphics != null)
            {
                if (graphics.Graphics != null)
                {
                    graphics.Graphics.DrawRectangle(pen, new Rectangle(point, (Size)size));
                }
            }

            pen.Dispose();
        }
        #endregion
    }
    #endregion

    #region EllipseFrameVisionImageOverlay
    [Serializable]
    public class EllipseFrameVisionImageOverlay : FrameVisionImageOverlay
    {
        #region Field
        private int m_Width;
        private int m_Height;
        #endregion

        #region Constructor
        public EllipseFrameVisionImageOverlay(string name, int x, int y, int width, int height, Color color) : base(name)
        {
            this.StartLocation = new Point(x, y);
            this.EndLocation = new Point(x + width, y + height);
            this.Color = color;

            this.DashStyle = DashStyle.Dash;
            this.Width = (this.EndLocation.X - this.StartLocation.X);
            this.Height = (this.EndLocation.Y - this.StartLocation.Y);
        }
        public EllipseFrameVisionImageOverlay(string name, double x, double y, double width, double height, Color color) : base(name)
        {
            this.StartLocation = new Point((int)x, (int)y);
            this.EndLocation = new Point((int)(x + width), (int)(y + height));
            this.Color = color;

            this.DashStyle = DashStyle.Dash;
            this.Width = (this.EndLocation.X - this.StartLocation.X);
            this.Height = (this.EndLocation.Y - this.StartLocation.Y);
        }
        public EllipseFrameVisionImageOverlay(string name, Point startLocation, Point endLocation, Color color) : base(name)
        {
            this.StartLocation = startLocation;
            this.EndLocation = endLocation;
            this.Color = color;

            this.DashStyle = DashStyle.Dash;
            this.Width = (this.EndLocation.X - this.StartLocation.X);
            this.Height = (this.EndLocation.Y - this.StartLocation.Y);
        }
        public EllipseFrameVisionImageOverlay(string name, Point startLocation, Point endLocation) : this(name, startLocation, endLocation, Color.Lime) { }
        public EllipseFrameVisionImageOverlay(string name) : this(name, new Point(), new Point(100, 100)) { }
        public EllipseFrameVisionImageOverlay() : this("") { }
        #endregion

        #region Property
        /// <summary>
        /// Overlay될 Ellipse의 너비를 가져오거나 설정한다.
        /// </summary>
        public int Width
        {
            get { return this.m_Width; }
            set
            {
                if (this.m_Width == value) return;

                this.m_Width = value;
                this.OnChangingOverlay(new VisionImageOverlayEventArgs());
            }
        }
        /// <summary>
        /// Overlay될 Ellipse의 높이를 가져오거나 설정한다.
        /// </summary>
        public int Height
        {
            get { return this.m_Height; }
            set
            {
                if (this.m_Height == value) return;

                this.m_Height = value;
                this.OnChangingOverlay(new VisionImageOverlayEventArgs());
            }
        }
        #endregion

        #region Overlay Members


        
        protected override void OnDraw(Point offset, SizeD? sourceSize, SizeD? destinateSize, Graphics graphics)
        {
            SizeD scale = SizeD.Empty;
            Pen pen = null;
            RectangleF rectange = RectangleF.Empty;
            PointD location = PointD.Empty;
            SizeD size = SizeD.Empty;
            double tempWidth = 0.0;
            double tempHeight = 0.0;

            if (sourceSize == null || destinateSize == null)
                scale = new SizeD(1, 1);
            else
            {
                tempWidth = destinateSize.Value.Width / sourceSize.Value.Width;
                tempHeight = destinateSize.Value.Height / sourceSize.Value.Height;
                scale = new SizeD(tempWidth, tempHeight);
            }

            pen = new Pen(this.Color, this.Thickness);
            pen.DashStyle = this.DashStyle;
            location = new PointD((this.StartLocation.X + offset.X) * scale.Width, (this.StartLocation.Y + offset.Y) * scale.Height);
            size = new SizeD(this.Width / scale.Width, this.Height / scale.Height);

            rectange = new RectangleF(location, size);

            graphics.DrawEllipse(pen, rectange);

            pen.Dispose();
        }

        protected override void OnDraw(Point offset, SizeD? sourceSize, SizeD? destinateSize, BufferedGraphics graphics)
        {
            //SizeD scale = SizeD.Empty;
            //Pen pen = null;
            //RectangleF rectange = RectangleF.Empty;
            //PointD location = PointD.Empty;
            //SizeD size = SizeD.Empty;
            //double tempWidth = 0.0;
            //double tempHeight = 0.0;

            //if (sourceSize == null || destinateSize == null)
            //    scale = new SizeD(1, 1);
            //else
            //{
            //    tempWidth = destinateSize.Value.Width / sourceSize.Value.Width;
            //    tempHeight = destinateSize.Value.Height / sourceSize.Value.Height;
            //    scale = new SizeD(tempWidth, tempHeight);
            //}

            //pen = new Pen(this.Color, 3);
            //pen.DashStyle = this.DashStyle;
            //location = new PointD((this.StartLocation.X + offset.X) * scale.Width, (this.StartLocation.Y + offset.Y) * scale.Height);
            //size = new SizeD(this.Width / scale.Width, this.Height / scale.Height);

            SizeD scale = SizeD.Empty;
            SizeD size = SizeD.Empty;
            Pen pen = null;
            PointD point = PointD.Empty;
            double tempWidth = 0.0;
            double tempHeight = 0.0;

            if (sourceSize == null || destinateSize == null)
                scale = new SizeD(1, 1);
            else
            {
                tempWidth = destinateSize.Value.Width / sourceSize.Value.Width;
                tempHeight = destinateSize.Value.Height / sourceSize.Value.Height;
                scale = new SizeD(tempWidth, tempHeight);
            }

            pen = new Pen(this.Color, this.Thickness);
            point = new PointD((this.StartLocation.X - offset.X) * scale.Width, (this.StartLocation.Y - offset.Y) * scale.Height);
            size = new SizeD(this.Size.Width * scale.Width, this.Size.Height * scale.Height);


            RectangleF rectange = new RectangleF(point, size);
            if (graphics != null)
            {
                if (graphics.Graphics != null)
                {
                    graphics.Graphics.DrawEllipse(pen, rectange);
                }
            }

            pen.Dispose();
        }
        #endregion
    }
    #endregion

    #region PolygonFrameVisionImageOverlay
    [Serializable]
    public class PolygonFrameVisionImageOverlay : FrameVisionImageOverlay
    {
        #region Field
        private PointDCollection m_Points;
        #endregion

        #region Constructor
        public PolygonFrameVisionImageOverlay(string name, PointDCollection points, Color color) : base(name)
        {
            this.Points = points;
            this.Color = color;

            this.DashStyle = DashStyle.Dash;
        }
        public PolygonFrameVisionImageOverlay(string name, PointDCollection points) : this(name, points, Color.Lime) { }
        public PolygonFrameVisionImageOverlay(string name) : this(name, new PointDCollection()) { }
        public PolygonFrameVisionImageOverlay() : this("") { }
        #endregion

        #region Property
        public PointDCollection Points
        {
            get { return this.m_Points; }
            set { this.m_Points = value; }
        }
        #endregion

        #region Overlay Members
        protected override void OnDraw(Point offset, SizeD? sourceSize, SizeD? destinateSize, Graphics graphics)
        {
            SizeD scale = SizeD.Empty;
            Pen pen = null;
            Point[] polygonPoint = null;
            double tempWidth = 0.0;
            double tempHeight = 0.0;

            if (sourceSize == null || destinateSize == null)
                scale = new SizeD(1, 1);
            else
            {
                tempWidth = destinateSize.Value.Width / sourceSize.Value.Width;
                tempHeight = destinateSize.Value.Height / sourceSize.Value.Height;
                scale = new SizeD(tempWidth, tempHeight);
            }

            pen = new Pen(this.Color, this.Thickness);
            pen.DashStyle = this.DashStyle;
            polygonPoint = new Point[this.Points.Count];

            for (int i = 0; i < this.Points.Count; i++)
            {
                polygonPoint[i] = new PointD((this.Points[i].X - offset.X) * scale.Width, (this.Points[i].Y - offset.Y) * scale.Height);
            }
            graphics.DrawPolygon(pen, polygonPoint);

            pen.Dispose();
        }

        protected override void OnDraw(Point offset, SizeD? sourceSize, SizeD? destinateSize, BufferedGraphics graphics)
        {
            SizeD scale = SizeD.Empty;
            Pen pen = null;
            Point[] polygonPoint = null;
            double tempWidth = 0.0;
            double tempHeight = 0.0;

            if (sourceSize == null || destinateSize == null)
                scale = new SizeD(1, 1);
            else
            {
                tempWidth = destinateSize.Value.Width / sourceSize.Value.Width;
                tempHeight = destinateSize.Value.Height / sourceSize.Value.Height;
                scale = new SizeD(tempWidth, tempHeight);
            }

            pen = new Pen(this.Color, this.Thickness);
            pen.DashStyle = this.DashStyle;
            polygonPoint = new Point[this.Points.Count];

            for (int i = 0; i < this.Points.Count; i++)
            {
                polygonPoint[i] = new PointD((this.Points[i].X - offset.X) * scale.Width, (this.Points[i].Y - offset.Y) * scale.Height);
            }
            if (graphics != null)
            {
                if (graphics.Graphics != null)
                {
                    graphics.Graphics.DrawPolygon(pen, polygonPoint);
                }
            }

            pen.Dispose();
        }
        #endregion
    }
    #endregion

    #region VisionImageOverlayEventArgs
    [Serializable]
    public class VisionImageOverlayEventArgs : EquipmentEventArgs
    {
        #region Constructor
        public VisionImageOverlayEventArgs()
        {
        }
        #endregion
    }
    public delegate void VisionImageOverlayEventHandler(object sender, VisionImageOverlayEventArgs e);
    #endregion
}