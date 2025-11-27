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
    #region CircleD
    [Serializable]
    [TypeConverter(typeof(CircleDConverter))]
    public struct CircleD : IShape
    {
        #region Field
        private const string XmlElementNameCenterX = "CenterX";
        private const string XmlElementNameCenterY = "CenterY";
        private const string XmlElementNameRadius = "Radius";

        public static readonly CircleD Empty;

        private double m_CenterX;
        private double m_CenterY;
        private double m_Radius;
        #endregion

        #region Constructor
        public CircleD(double centerX, double centerY, double radius)
        {
            this.m_CenterX = centerX;
            this.m_CenterY = centerY;
            this.m_Radius = radius;
        }

        public CircleD(PointD point, double radius) : this(point.X, point.Y, radius) { }

        public CircleD(XyCoordinate coordinate, double radius) : this(coordinate.X, coordinate.Y, radius) { }

        static CircleD()
        {
            CircleD.Empty = new CircleD(0, 0, 0);
        }
        #endregion

        #region Property
        public double CenterX
        {
            get { return this.m_CenterX; }
            set { this.m_CenterX = value; }
        }

        public double CenterY
        {
            get { return this.m_CenterY; }
            set { this.m_CenterY = value; }
        }

        public double Radius
        {
            get { return this.m_Radius; }
            set { this.m_Radius = value; }
        }

        [Browsable(false)]
        public bool IsEmpty
        {
            get { return this == CircleD.Empty; }
        }
        #endregion

        #region Method
        public bool Equals(CircleD value)
        {
            return this == value;
        }
        #endregion

        #region IShape Members
        bool IShape.Equals(IShape value)
        {
            if (value is CircleD)
                return false;
            else
                return this.Equals((CircleD)value);
        }

        /// <summary>
        /// 주어진 점이 원의 내부에 있는지를 반환한다.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns>
        /// 원의 안쪽에 있는 경우는 1, 
        /// 원위에 있는 경우는 0, 
        /// 원의 바깥쪽에 있는 경우는 -1을 반환한다.
        /// </returns>
        public ShapeLocation Contains(double x, double y)
        {
            if (Math.Pow(x - this.CenterX, 2) + Math.Pow(y - this.CenterY, 2) < Math.Pow(this.Radius, 2))
            {
                return ShapeLocation.Inner;
            }
            else if (Math.Pow(x - this.CenterX, 2) + Math.Pow(y - this.CenterY, 2) == Math.Pow(this.Radius, 2))
            {
                return ShapeLocation.Overlapping;
            }
            else
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
            double distance = 0.0;
            distance = Math.Sqrt(Math.Pow((this.CenterX - circle.CenterX), 2) + Math.Pow((this.CenterY - circle.CenterY), 2)); //두 원 중심의 거리

            if (circle.Radius < this.Radius && (distance + circle.Radius) < this.Radius)
                return ShapeLocation.Inner;
            else if ((circle.Radius + this.Radius) < distance)
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
        public static CircleD Parse(string text)
        {
            double centerX, centerY, radius;
            string[] token = null;
            token = text.Split(',');

            centerX = double.Parse(token[0].Trim('(', ' ', ')'));
            centerY = double.Parse(token[1].Trim('(', ' ', ')'));
            radius = double.Parse(token[2].Trim('(', ' ', ')'));

            return new CircleD(centerX, centerY, radius);
        }

        /// <summary>
        /// 주어진 3점을 지나는 원의 중심과 반지름을 구한다.
        /// 단. 3점의 위치는 순서대로 입력하여야 한다. (예를 들어서 CW, CCW방향 순서대로)
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="p3"></param>
        /// <param name="center"></param>
        /// <param name="radius"></param>
        /// <returns></returns>
        public static bool GetCircle(PointD p1, PointD p2, PointD p3, ref PointD center, ref double radius)
        {
            try
            {
                double slope1 = (p1.Y - p2.Y) / (p1.X - p2.X);
                double slope2 = (p3.Y - p2.Y) / (p3.X - p2.X);

                double x = (slope1 * slope2 * (p3.Y - p1.Y) + slope1 * (p2.X + p3.X) - slope2 * (p1.X + p2.X)) / (2 * (slope1 - slope2));
                double y = -(1 / slope1) * (x - ((p1.X + p2.X) / 2)) + (p1.Y + p2.Y) / 2;

                center = new PointD(x, y);
                radius = Math.Sqrt(Math.Pow(p1.X - x, 2) + Math.Pow(p1.Y - y, 2));
            }
            catch (Exception ex)
            {
                //if (SafeThread.IsThreadInterrupted(ex) == true)
                //    throw ex;
                Log.Write(ex);
                return false;
            }

            return true;
        }

        public static bool GetCircle(PointD p1, PointD p2, PointD p3, ref CircleD circle)
        {
            PointD center = new PointD();
            double radius = 0;

            if (GetCircle(p1, p2, p3, ref center, ref radius) != true) return false;

            circle = new CircleD(center.X, center.Y, radius);

            return true;
        }

        public static PointDCollection FindLineCircleIntersection(CircleD circle, PointD point, double angle)
        {
            PointDCollection points = new PointDCollection();
            PointD intersectPointA = new PointD();
            PointD intersectPointB = new PointD();
            double linearConstantA;
            double linearConstantB;
            double quadraticConstantA;
            double quadraticConstantB;
            double quadraticConstantC;
            double discriminant;
            double quadraticFormula;


            if (angle == 90 || angle == 270 || angle == -90 || angle == -270)
            {
                quadraticConstantA = 1;
                quadraticConstantB = -2 * circle.CenterY;
                quadraticConstantC = Math.Pow(point.X - circle.CenterX, 2) + Math.Pow(circle.CenterY, 2) - Math.Pow(circle.Radius, 2);
                // 판별식 b^2 - 4ac
                discriminant = Math.Pow(quadraticConstantB, 2) - 4 * quadraticConstantA * quadraticConstantC;

                if (discriminant == 0)
                {
                    quadraticFormula = -quadraticConstantB / (2 * quadraticConstantA);
                    intersectPointA = new PointD(point.X, quadraticFormula);
                    points.Add(intersectPointA);
                }
                else
                {
                    quadraticFormula = (-quadraticConstantB - Math.Sqrt(discriminant)) / (2 * quadraticConstantA);
                    intersectPointA = new PointD(point.X, quadraticFormula);
                    points.Add(intersectPointA);
                    quadraticFormula = (-quadraticConstantB + Math.Sqrt(discriminant)) / (2 * quadraticConstantA);
                    intersectPointB = new PointD(point.X, quadraticFormula);
                    points.Add(intersectPointB);
                }
            }
            else
            {
                // y = ax + b
                linearConstantA = Math.Tan(angle * Math.PI / 180);
                linearConstantB = point.Y - linearConstantA * point.X;
                // (X-centerX)^2 + (Y-centerY)^2 = r^2 과 y = ax+b 연립 
                quadraticConstantA = 1 + Math.Pow(linearConstantA, 2);
                quadraticConstantB = 2 * (linearConstantA * linearConstantB - circle.CenterX - linearConstantA * circle.CenterY);
                quadraticConstantC = Math.Pow(circle.CenterX, 2) + Math.Pow(circle.CenterY, 2) + Math.Pow(linearConstantB, 2) - 2 * circle.CenterY * linearConstantB - Math.Pow(circle.Radius, 2);

                // 판별식 b^2 - 4ac
                discriminant = Math.Pow(quadraticConstantB, 2) - 4 * quadraticConstantA * quadraticConstantC;

                if (discriminant == 0)
                {
                    quadraticFormula = -quadraticConstantB / (2 * quadraticConstantA);
                    intersectPointA = new PointD(quadraticFormula, linearConstantA * quadraticFormula + linearConstantB);
                    points.Add(intersectPointA);
                }
                else
                {
                    quadraticFormula = (-quadraticConstantB - Math.Sqrt(discriminant)) / (2 * quadraticConstantA);
                    intersectPointA = new PointD(quadraticFormula, linearConstantA * quadraticFormula + linearConstantB);
                    points.Add(intersectPointA);
                    quadraticFormula = (-quadraticConstantB + Math.Sqrt(discriminant)) / (2 * quadraticConstantA);
                    intersectPointB = new PointD(quadraticFormula, linearConstantA * quadraticFormula + linearConstantB);
                    points.Add(intersectPointB);
                }
            }
            return points;
        }

        public static XyCoordinateCollection FindLineCircleIntersection(CircleD circle, XyCoordinate point, double angle)
        {
            PointDCollection points = new PointDCollection();
            XyCoordinateCollection coordinates = new XyCoordinateCollection();

            points = CircleD.FindLineCircleIntersection(circle, (PointD)point, angle);

            for (int i = 0; i < points.Count; i++)
            {
                coordinates.Add((XyCoordinate)points[i]);
            }

            return coordinates;
        }
        #endregion

        #region Object Members
        public override bool Equals(object obj)
        {
            if (obj == null) return false;

            if (obj is CircleD == false) return false;
            CircleD value = (CircleD)obj;

            return this.Equals(value);
        }

        public override int GetHashCode()
        {
            return this.CenterX.GetHashCode() ^ this.CenterY.GetHashCode() ^ this.Radius.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("[{0}, {1}, {2}]", this.CenterX, this.CenterY, this.Radius);
        }
        #endregion

        #region Operator
        public static bool operator ==(CircleD left, CircleD right)
        {
            return left.CenterX == right.CenterX && left.CenterY == right.CenterY && left.Radius == right.Radius;
        }

        public static bool operator !=(CircleD left, CircleD right)
        {
            return !(left == right);
        }


        //public static implicit operator CircleD(string text)
        //{
        //    return CircleD.Parse(text);
        //}
        #endregion
    }
    #endregion

    #region CircleDConverter
    public class CircleDConverter : ExpandableObjectConverter
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
                string text = value as string;
                string[] array = text.Split(',');
                CircleD circle = new CircleD();
                circle.CenterX = double.Parse(array[0]);
                circle.CenterY = double.Parse(array[1]);
                circle.Radius = double.Parse(array[2]);

                return circle;
            }
            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string) && value is PointD)
            {
                CircleD circle = (CircleD)value;
                return string.Format("{0}, {1}, {2}",
                    circle.CenterX,
                    circle.CenterY,
                    circle.Radius
                    );
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
        //protected override ObjectCollection CreateInstanceDescriptor(object value)
        //{
        //    ObjectCollection constructMember = new ObjectCollection();
        //    constructMember.Add(this.GetPropertyValue(value, "CenterX"));
        //    constructMember.Add(this.GetPropertyValue(value, "CenterY"));
        //    constructMember.Add(this.GetPropertyValue(value, "Radius"));
        //    return constructMember;
        //}
        //protected override StringCollection GetListProperties()
        //{
        //    StringCollection propertyList = new StringCollection();
        //    propertyList.Add("CenterX");
        //    propertyList.Add("CenterY");
        //    propertyList.Add("Radius");

        //    return propertyList;
        //}
    }
    #endregion

    #region CircleDCollection
    [Serializable]
    public class CircleDCollection : Collection<CircleD>
    {
        #region Constructor
        public CircleDCollection() : base() { }
        public CircleDCollection(IList<CircleD> list) : base(list) { }
        #endregion

        #region Method
        #endregion
    }
    #endregion
}