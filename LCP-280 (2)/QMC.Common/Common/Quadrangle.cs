using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Drawing;
using System.Globalization;
using System.Reflection;


namespace QMC.Common
{
    /// <summary>
    /// 사각형에 대해 정의한다.
    /// </summary>
    [Serializable]
    [TypeConverter(typeof(QuadrangleConverter))]
    public struct Quadrangle : IShape
    {
        #region Field
        private const string XmlElementNameOne = "One";
        private const string XmlElementNameTwo = "Two";
        private const string XmlElementNameThree = "Three";
        private const string XmlElementNameFour = "Four";

        public static readonly Quadrangle Empty;
        //public static readonly Quadrangle Empty = new Quadrangle(new Point(0, 0), new Point(0, 0), new Point(0, 0), new Point(0, 0));

        private PointD m_One;
        private PointD m_Two;
        private PointD m_Three;
        private PointD m_Four;
        #endregion

        #region Constructor
        public Quadrangle(params PointD[] points)
        {
            if (points == null)
                throw new ArgumentNullException("points");
            if (points.Length != 4)
                throw new ArgumentOutOfRangeException("points.Length");

            this.m_One = points[0];
            this.m_Two = points[1];
            this.m_Three = points[2];
            this.m_Four = points[3];
        }

        public Quadrangle(PointD one, PointD two, PointD three, PointD four) : this(new PointD[] { one, two, three, four }) { }
        #endregion

        #region Property
        public PointD One
        {
            get { return this.m_One; }
            set { this.m_One = value; }
        }

        public PointD Two
        {
            get { return this.m_Two; }
            set { this.m_Two = value; }
        }

        public PointD Three
        {
            get { return this.m_Three; }
            set { this.m_Three = value; }
        }

        public PointD Four
        {
            get { return this.m_Four; }
            set { this.m_Four = value; }
        }

        [Browsable(false)]
        public bool IsEmpty
        {
            get { return this == Quadrangle.Empty; }
        }
        #endregion

        #region Method
        public bool Equals(Quadrangle value)
        {
            Quadrangle a = new Quadrangle(new PointD(), new PointD());


            return this == value;
        }
        #endregion

        #region IShape Members
        bool IShape.Equals(IShape value)
        {
            if (value is Quadrangle)
                return false;
            else
                return this.Equals((Quadrangle)value);
        }
       
        public ShapeLocation Contains(Point point)
        {
            return this.Contains(point.X, point.Y);
        }

        public ShapeLocation Contains(XyCoordinate coordinate)
        {
            return this.Contains(coordinate.X, coordinate.Y);
        }

        public ShapeLocation Contains(double x, double y)
        {
            return this.Contains(new double[] { x, y });
        }

        public ShapeLocation Contains(params double[] positions)
        {
            return this.Contains(new PointD(positions[0], positions[1]));
        }

        /// <summary>
        ///  주어진 점이 사각형의 내부에 있는지를 반환한다.
        /// </summary>
        /// <param name="point">주어진 점의 위치입니다.</param>
        /// <returns>
        /// 사각형의 안쪽에 있는 경우는 1,
        /// 사각형의 위쪽에 있는 경우는 0,
        /// 사각형의 바깥쪽에 있는 경우는 -1을 반환한다.
        /// </returns>
        public ShapeLocation Contains(PointD point)
        {
            List<PointD> points = new List<PointD>();

            points.Add(this.One);
            points.Add(this.Two);
            points.Add(this.Three);
            points.Add(this.Four);

            if (qGeometry.IsPointInsidePolygon(points, point) == 0)
                return ShapeLocation.Overlapping;
            else if (qGeometry.IsPointInsidePolygon(points, point) == 1)
                return ShapeLocation.Inner;
            else
                return ShapeLocation.Outer;
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

        #region Static Members
        public static Quadrangle Parse(string text)
        {
            PointD[] points = new PointD[4];

            string[] token = null;
            double x = 0.0, y = 0.0;

            if (string.IsNullOrEmpty(text) == true)
                return new Quadrangle(new PointD(0, 0), new PointD(0, 0), new PointD(0, 0), new PointD(0, 0));

            token = text.Split(',');

            for (int i = 0; i < points.Length; i++)
            {
                x = double.Parse(token[i * 2].Trim('(', ' ', ')'));
                y = double.Parse(token[i * 2 + 1].Trim('(', ' ', ')'));

                points[i] = new PointD(x, y);
            }

            return new Quadrangle(points);
        }
        #endregion

        #region Object Members
        public override bool Equals(object obj)
        {
            if (obj == null) return false;

            if (obj is Quadrangle == false) return false;
            Quadrangle value = (Quadrangle)obj;

            return this.Equals(value);
        }

        public override int GetHashCode()
        {
            return this.One.GetHashCode() ^ this.Two.GetHashCode() ^ this.Three.GetHashCode() ^ this.Four.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("({0}),({1}),({2}),({3})", this.One, this.Two, this.Three, this.Four);
        }
        #endregion

        #region Operator
        public static bool operator ==(Quadrangle left, Quadrangle right)
        {
            return left.One == right.One && left.Two == right.Two && left.Three == right.Three && left.Four == right.Four;
        }

        public static bool operator !=(Quadrangle left, Quadrangle right)
        {
            return !(left == right);
        }

        public static Quadrangle operator +(Quadrangle left, Quadrangle right)
        {
            return new Quadrangle(left.One + right.One, left.Two + right.Two, left.Three + right.Three, left.Four + right.Four);
        }

        public static Quadrangle operator +(Quadrangle left, PointD right)
        {
            return new Quadrangle(left.One + right, left.Two + right, left.Three + right, left.Four + right);
        }

        public static Quadrangle operator -(Quadrangle left, Quadrangle right)
        {
            return new Quadrangle(left.One - right.One, left.Two - right.Two, left.Three - right.Three, left.Four - right.Four);
        }

        public static Quadrangle operator -(Quadrangle left, PointD right)
        {
            return new Quadrangle(left.One - right, left.Two - right, left.Three - right, left.Four - right);
        }

        public static Quadrangle operator *(Quadrangle left, Quadrangle right)
        {
            return new Quadrangle(left.One * right.One, left.Two * right.Two, left.Three * right.Three, left.Four * right.Four);
        }

        public static Quadrangle operator *(Quadrangle left, PointD right)
        {
            return new Quadrangle(left.One * right, left.Two * right, left.Three * right, left.Four * right);
        }

        public static Quadrangle operator /(Quadrangle left, Quadrangle right)
        {
            return new Quadrangle(left.One / right.One, left.Two / right.Two, left.Three / right.Three, left.Four / right.Four);
        }

        public static Quadrangle operator /(Quadrangle left, PointD right)
        {
            return new Quadrangle(left.One / right, left.Two / right, left.Three / right, left.Four / right);
        }

        //public static implicit operator Quadrangle(string text)
        //{
        //    return Quadrangle.Parse(text);
        //}

        #endregion
    }

    internal class QuadrangleConverter : ExpandableObjectConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(String))
                return true;

            return base.CanConvertFrom(context, sourceType);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(InstanceDescriptor))
                return true;

            return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string)
            {
                string text = value as string;
                string[] token;

                token = text.Split(',');

                Quadrangle specialized = new Quadrangle();

                specialized.One = new PointD(double.Parse(token[0]), double.Parse(token[1]));
                specialized.Two = new PointD(double.Parse(token[2]), double.Parse(token[3]));
                specialized.Three = new PointD(double.Parse(token[4]), double.Parse(token[5]));
                specialized.Four = new PointD(double.Parse(token[6]), double.Parse(token[7]));

                return specialized;
            }
            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (value is Quadrangle)
            {
                if (destinationType == typeof(string))
                {
                    Quadrangle specialized = (Quadrangle)value;
                    return specialized.ToString();
                }
                if (destinationType == typeof(InstanceDescriptor))
                {
                    Quadrangle rv = (Quadrangle)value;

                    ConstructorInfo ctor = typeof(Quadrangle).GetConstructor(new Type[] { typeof(double), typeof(double), typeof(double), typeof(double) });
                    if (ctor != null)
                    {
                        return new InstanceDescriptor(ctor, new object[] { rv.One, rv.Two, rv.Three, rv.Four });
                    }
                }
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }

        public override object CreateInstance(ITypeDescriptorContext context, IDictionary propertyValues)
        {
            object one = propertyValues["One"];
            object two = propertyValues["Two"];
            object three = propertyValues["Three"];
            object four = propertyValues["Four"];

            return new Quadrangle((PointD)one, (PointD)two, (PointD)three, (PointD)four);
        }

        public override bool GetCreateInstanceSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        public override bool GetPropertiesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
        {
            PropertyDescriptorCollection props = TypeDescriptor.GetProperties(typeof(Quadrangle), attributes);
            return props.Sort(new string[] { "One", "Two", "Three", "Four" });
        }
    }

    [Serializable]
    public class QuadrangleCollection : Collection<Quadrangle>
    {
        #region Constructor
        public QuadrangleCollection() : base() { }
        public QuadrangleCollection(IList<Quadrangle> list) : base(list) { }
        #endregion

        #region Method
        #endregion
    }

    //[Serializable]
    //public class QuadrangleInsideSatisfaction : ISatisfiable<PointD>
    //{
    //    #region Field
    //    private Quadrangle m_Quadrangle;
    //    #endregion

    //    #region Constructor
    //    public QuadrangleInsideSatisfaction(Quadrangle quadrangle)
    //    {
    //        this.Quadrangle = quadrangle;
    //    }
    //    public QuadrangleInsideSatisfaction() : this(Quadrangle.Empty) { }
    //    #endregion

    //    #region Property
    //    public Quadrangle Quadrangle
    //    {
    //        get { return this.m_Quadrangle; }
    //        set { this.m_Quadrangle = value; }
    //    }
    //    #endregion

    //    #region Override Members
    //    bool ISatisfiable.Satisfy(object obj)
    //    {
    //        if (obj is PointD == false) return false;
    //        return this.Satisfy((PointD)obj);
    //    }

    //    public bool Satisfy(PointD obj)
    //    {
    //        return ((this.Quadrangle.Contains(obj) == 0) ? true : false);
    //    }
    //    #endregion
    //}

}