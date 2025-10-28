using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.Design.Serialization;
using System.Reflection;
using System.Collections;


namespace QMC.Common
{
    /// <summary>
    /// 직각사각형에 대해 정의한다.
    /// </summary>
    [Serializable]
    [TypeConverter(typeof(RectangleDConverter))]
    public struct RectangleD : IShape
    {
        #region Field
        private const string XmlElementNameX = "X";
        private const string XmlElementNameY = "Y";
        private const string XmlElementNameWidth = "Width";
        private const string XmlElementNameHeight = "Height";

        public static readonly RectangleD Empty;

        private double m_X;
        private double m_Y;
        private double m_Width;
        private double m_Height;
        private PointD m_Center;
        #endregion

        #region Constructor
        public RectangleD(double x, double y, double width, double height)
        {
            this.m_X = x;
            this.m_Y = y;
            this.m_Width = width;
            this.m_Height = height;
            this.m_Center = new PointD(0, 0);
        }

        public RectangleD(PointD point, double width, double height) : this(point.X, point.Y, width, height) { }

        public RectangleD(PointD point, SizeD size) : this(point.X, point.Y, size.Width, size.Height) { }

        public RectangleD(XyCoordinate coordinate, double width, double height) : this(coordinate.X, coordinate.Y, width, height) { }

        public RectangleD(XyCoordinate coordinate, SizeD size) : this(coordinate.X, coordinate.Y, size.Width, size.Height) { }

        static RectangleD()
        {
            RectangleD.Empty = new RectangleD(0, 0, 0, 0);
        }
        #endregion

        #region Property
        public double X
        {
            get { return this.m_X; }
            set { this.m_X = value; }
        }

        public double Y
        {
            get { return this.m_Y; }
            set { this.m_Y = value; }
        }

        public double Width
        {
            get { return this.m_Width; }
            set { this.m_Width = value; }
        }

        public double Height
        {
            get { return this.m_Height; }
            set { this.m_Height = value; }
        }

        [Browsable(false)]
        public bool IsEmpty
        {
            get { return this == RectangleD.Empty; }
        }

        public PointD Center
        {
            get { return this.m_Center; }
            set
            {
                this.m_Center = value;
                this.X = this.Center.X - this.Width / 2;
                this.Y = this.Center.Y + this.Height / 2;
            }
        }
        #endregion

        #region Method
        public bool Equals(RectangleD value)
        {
            return this == value;
        }
        #endregion

        #region IShape Members
        bool IShape.Equals(IShape value)
        {
            if (value is RectangleD)
                return false;
            else
                return this.Equals((RectangleD)value);
        }

        /// <summary>
        /// 주어진 점(LeftTop)이 사각형의 내부에 있는지를 반환한다.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns>
        /// 사각형의 안쪽에 있는 경우는 1, 
        /// 사각혀의 위에 있는 경우는 0, 
        /// 사각형의 바깥쪽에 있는 경우는 -1을 반환한다.
        /// </returns>
        public ShapeLocation Contains(double x, double y)
        {
            RangeD xRange = new RangeD(
                this.X,
                this.X + Math.Abs(this.Width)
                );
            RangeD yRange = new RangeD(
                this.Y,
                this.Y + Math.Abs(this.Height)
                );

            if (xRange.Contains(x) == true && yRange.Contains(y) == true) return ShapeLocation.Inner;
            if (xRange.Contains(x) == false && yRange.Contains(y) == true) return ShapeLocation.Overlapping;
            if (xRange.Contains(x) == true && yRange.Contains(y) == false) return ShapeLocation.Overlapping;
            if (xRange.Contains(x) == false && yRange.Contains(y) == false) return ShapeLocation.Outer;

            return ShapeLocation.Outer;
        }

        public ShapeLocation Contains(Point point)
        {
            return this.Contains(point.X, point.Y);
        }

        public ShapeLocation Contains(PointD point)
        {
            return this.Contains(point.X, point.Y);
        }

        public ShapeLocation Contains(XyCoordinate coordinate)
        {
            return this.Contains(coordinate.X, coordinate.Y);
        }

        public ShapeLocation Contains(IShape shape)
        {
            if (shape is CircleD)
                return this.Contains((CircleD)shape);
            else if (shape is RectangleD)
                return this.Contains((RectangleD)shape);
            else if (shape is Quadrangle)
                return this.Contains((Quadrangle)shape);
            else
                throw new TypeAccessException("Unsupported type.");
        }

        private ShapeLocation Contains(CircleD circle)
        {
            ShapeLocation[] location = new ShapeLocation[4];

            location[0] = this.Contains(circle.CenterX - circle.Radius, circle.CenterY - circle.Radius);
            location[1] = this.Contains(circle.CenterX + circle.Radius, circle.CenterY - circle.Radius);
            location[2] = this.Contains(circle.CenterX - circle.Radius, circle.CenterY + circle.Radius);
            location[3] = this.Contains(circle.CenterX + circle.Radius, circle.CenterY + circle.Radius);

            if (location[0] == ShapeLocation.Inner && location[1] == ShapeLocation.Inner && location[2] == ShapeLocation.Inner && location[3] == ShapeLocation.Inner)
                return ShapeLocation.Inner;
            else if (location[0] == ShapeLocation.Outer && location[1] == ShapeLocation.Outer && location[2] == ShapeLocation.Outer && location[3] == ShapeLocation.Outer)
                return ShapeLocation.Outer;
            else
                return ShapeLocation.Overlapping;
        }

        private ShapeLocation Contains(RectangleD rectangle)
        {
            ShapeLocation[] location = new ShapeLocation[4];

            location[0] = this.Contains(rectangle.X, rectangle.Y);
            location[1] = this.Contains(rectangle.X + rectangle.Width, rectangle.Y);
            location[2] = this.Contains(rectangle.X, rectangle.Y + rectangle.Height);
            location[3] = this.Contains(rectangle.X + rectangle.Width, rectangle.Y + rectangle.Height);

            if (location[0] == ShapeLocation.Inner && location[1] == ShapeLocation.Inner && location[2] == ShapeLocation.Inner && location[3] == ShapeLocation.Inner)
                return ShapeLocation.Inner;
            else if (location[0] == ShapeLocation.Outer && location[1] == ShapeLocation.Outer && location[2] == ShapeLocation.Outer && location[3] == ShapeLocation.Outer)
                return ShapeLocation.Outer;
            else
                return ShapeLocation.Overlapping;
        }

        private ShapeLocation Contains(Quadrangle quadrangle)
        {
            ShapeLocation[] location = new ShapeLocation[4];

            location[0] = this.Contains(quadrangle.One);
            location[1] = this.Contains(quadrangle.Two);
            location[2] = this.Contains(quadrangle.Three);
            location[3] = this.Contains(quadrangle.Four);

            if (location[0] == ShapeLocation.Inner && location[1] == ShapeLocation.Inner && location[2] == ShapeLocation.Inner && location[3] == ShapeLocation.Inner)
                return ShapeLocation.Inner;
            else if (location[0] == ShapeLocation.Outer && location[1] == ShapeLocation.Outer && location[2] == ShapeLocation.Outer && location[3] == ShapeLocation.Outer)
                return ShapeLocation.Outer;
            else
                return ShapeLocation.Overlapping;
        }
        #endregion

        #region Static Member
        public static RectangleD Parse(string text)
        {
            double x, y, width, height;
            string[] token = null;

            if (string.IsNullOrEmpty(text) == true)
                return new RectangleD(0, 0, 0, 0);

            token = text.Split(',');

            x = double.Parse(token[0].Trim('(', ' ', ')'));
            y = double.Parse(token[1].Trim('(', ' ', ')'));
            width = double.Parse(token[2].Trim('(', ' ', ')'));
            height = double.Parse(token[3].Trim('(', ' ', ')'));

            return new RectangleD(x, y, width, height);
        }

        /// <summary>
        /// 사각형과 직선의 접점을 구한다. 
        /// 한 점과 그 점을 지나는 직선의 기울기를 파라미터로 받는다.
        /// </summary>
        /// <param name="rectangle">사각형</param>
        /// <param name="point">point 좌표</param>
        /// <param name="angle">point를 지나는 직선의 기울기</param>
        /// <returns></returns>
        public static PointDCollection FindLineRectangleIntersection(RectangleD rectangle, PointD point, double angle)
        {
            PointDCollection points = new PointDCollection();
            double linearConstantA;
            double linearConstantB;
            double leftLineY;
            double rightLineY;
            double topLineX;
            double bottomLineX;
            int decimalPoint = 7;

            if (angle == 90 || angle == 270 || angle == -90 || angle == -270)
            {
                if (point.X <= Math.Round(rectangle.X + rectangle.Width, decimalPoint) && Math.Round(rectangle.X , decimalPoint)<= point.X)
                {
                    points.Add(new PointD(point.X, rectangle.Y - rectangle.Height));
                    points.Add(new PointD(point.X, rectangle.Y));
                }
            }
            else if (angle == 0 || angle == 180 || angle == -180)
            {
                if (point.Y <= Math.Round(rectangle.Y, decimalPoint) && Math.Round(rectangle.Y - rectangle.Height, decimalPoint) <= point.Y)
                {
                    points.Add(new PointD(rectangle.X, point.Y));
                    points.Add(new PointD(Math.Round(rectangle.X + rectangle.Width, decimalPoint), point.Y));
                }
            }
            else
            {
                // y = ax + b
                linearConstantA = Math.Tan(angle * Math.PI / 180);
                linearConstantB = point.Y - linearConstantA * point.X;

                leftLineY = linearConstantA * rectangle.X + linearConstantB;
                rightLineY = linearConstantA * (rectangle.X + rectangle.Width) + linearConstantB;
                topLineX = (rectangle.Y - linearConstantB) / linearConstantA;
                bottomLineX = (rectangle.Y - rectangle.Height - linearConstantB) / linearConstantA;

                if (leftLineY < Math.Round(rectangle.Y, decimalPoint) && Math.Round(rectangle.Y,decimalPoint) - rectangle.Height < leftLineY)
                {
                    points.Add(new PointD(rectangle.X, leftLineY));
                }
                else if (rightLineY < Math.Round(rectangle.Y, decimalPoint) && Math.Round(rectangle.Y, decimalPoint) - rectangle.Height < rightLineY)
                {
                    points.Add(new PointD(rectangle.X + rectangle.Width, rightLineY));
                }
                else if (topLineX < Math.Round(rectangle.X, decimalPoint) + rectangle.Width && Math.Round(rectangle.X, decimalPoint) < topLineX)
                {
                    points.Add(new PointD(topLineX, rectangle.Y));
                }
                else if (bottomLineX < Math.Round(rectangle.X, decimalPoint) + rectangle.Width && Math.Round(rectangle.X, decimalPoint) < bottomLineX)
                {
                    points.Add(new PointD(bottomLineX, rectangle.Y - rectangle.Height));
                }
            }
            return points;
        }

        public static XyCoordinateCollection FindLineRectangleIntersection(RectangleD rectangle, XyCoordinate point, double angle)
        {
            PointDCollection points = new PointDCollection();
            XyCoordinateCollection coordinates = new XyCoordinateCollection();

            points = RectangleD.FindLineRectangleIntersection(rectangle, (PointD)point, angle);

            for (int i = 0; i < points.Count; i++)
            {
                coordinates.Add(points[i]);
            }

            return coordinates;
        }
        #endregion

        #region Object Members
        public override bool Equals(object obj)
        {
            if (obj == null) return false;

            if (obj is RectangleD == false) return false;
            RectangleD value = (RectangleD)obj;

            return this.Equals(value);
        }

        public override int GetHashCode()
        {
            return this.X.GetHashCode() ^ this.Y.GetHashCode() ^ this.Width.GetHashCode() ^ this.Height.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("{0}, {1}, {2}, {3}", this.X, this.Y, this.Width, this.Height);
        }
        #endregion

        #region Operator
        public static bool operator ==(RectangleD left, RectangleD right)
        {
            return left.X == right.X && left.Y == right.Y && left.Width == right.Width && left.Height == right.Height;
        }

        public static bool operator !=(RectangleD left, RectangleD right)
        {
            return !(left == right);
        }


        public static implicit operator Rectangle(RectangleD rectangle)
        {
            return new Rectangle((int)rectangle.X, (int)rectangle.Y, (int)rectangle.Width, (int)rectangle.Height);
        }


        public static explicit operator RectangleD(Rectangle rectangle)
        {
            return new RectangleD((double)rectangle.X, (double)rectangle.Y, (double)rectangle.Width, (double)rectangle.Height);
        }
        #endregion
    }

    public class RectangleDConverter : ExpandableObjectConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                return true;
            }
            return base.CanConvertTo(context, destinationType);
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string))
            {
                return true;
            }
            return base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string)
            {
                culture = culture ?? CultureInfo.InvariantCulture;
                string text = (string)value;
                if (string.IsNullOrWhiteSpace(text)) return RectangleD.Empty;

                // 공백 제거 후 분리
                string[] tokens = text.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                      .Select(t => t.Trim())
                                      .ToArray();

                if (tokens.Length < 4)
                    throw new FormatException($"RectangleD 문자열 형식 오류(필드 수 부족): '{text}'");

                double x = double.Parse(tokens[0], culture);
                double y = double.Parse(tokens[1], culture);
                double w = double.Parse(tokens[2], culture);
                double h = double.Parse(tokens[3], culture);

                RectangleD rect = new RectangleD();
                rect.X = x;
                rect.Y = y;
                rect.Width = w;
                rect.Height = h;

                // 선택적으로 Center (6개 값: X,Y,W,H,CX,CY) 지원
                if (tokens.Length >= 6)
                {
                    double cx = double.Parse(tokens[4], culture);
                    double cy = double.Parse(tokens[5], culture);
                    try
                    {
                        rect.Center = new PointD(cx, cy); // Center 설정 시 X,Y 재계산 로직 존재
                    }
                    catch
                    {
                        // Center 설정으로 인한 예외는 무시 (필요시 로깅)
                    }
                }

                return rect;
            }
            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string) && value is RectangleD)
            {
                // 기존 저장 포맷 (4개 값) 유지. 호환성을 위해 Center 는 출력하지 않음.
                RectangleD rect = (RectangleD)value;
                return string.Format(culture ?? CultureInfo.InvariantCulture, "{0}, {1}, {2}, {3}",
                    rect.X,
                    rect.Y,
                    rect.Width,
                    rect.Height);
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }

    [Serializable]
    public class RectangleDCollection : Collection<RectangleD>
    {
        #region Constructor
        public RectangleDCollection() : base() { }
        public RectangleDCollection(IList<RectangleD> list) : base(list) { }
        #endregion

        #region Method
        #endregion
    }
}