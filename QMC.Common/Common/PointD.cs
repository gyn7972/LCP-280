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
    [Serializable]
    [TypeConverter(typeof(PointDConverter))]
    public struct PointD
    {
        #region Field
        private const string XmlElementNameX = "X";
        private const string XmlElementNameY = "Y";

        public static readonly PointD Empty;

        private double m_X;
        private double m_Y;
        #endregion

        #region Constructor
        public PointD(double x, double y)
        {
            this.m_X = x;
            this.m_Y = y;
        }

        static PointD()
        {
            PointD.Empty = new PointD(0, 0);
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

        [Browsable(false)]
        public bool IsEmpty
        {
            get { return this == PointD.Empty; }
        }
        #endregion

        #region Method
        public bool Equals(PointD value)
        {
            return this == value;
        }
        #endregion

        #region Static Member
        public static PointD Parse(string text)
        {
            double x, y;
            string[] token = null;

            token = text.Split(',');

            x = double.Parse(token[0].Trim('(', ' ', ')'));
            y = double.Parse(token[1].Trim('(', ' ', ')'));

            return new PointD(x, y);
        }
        #endregion

        #region Object Members
        public override bool Equals(object obj)
        {
            if (obj == null) return false;

            if (obj is PointD == false) return false;
            PointD value = (PointD)obj;

            return this.Equals(value);
        }

        public override int GetHashCode()
        {
            return this.X.GetHashCode() ^ this.Y.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("{0}, {1}", this.X, this.Y);
        }
        #endregion

        #region Operator
        public static bool operator ==(PointD left, PointD right)
        {
            return left.X == right.X && left.Y == right.Y;
        }

        public static bool operator !=(PointD left, PointD right)
        {
            return !(left == right);
        }

        public static PointD operator +(PointD left, PointD right)
        {
            return new PointD(left.X + right.X, left.Y + right.Y);
        }

        public static PointD operator +(PointD left, double right)
        {
            return new PointD(left.X + right, left.Y + right);
        }

        public static PointD operator +(double left, PointD right)
        {
            return new PointD(left + right.X, left + right.Y);
        }

        public static PointD operator +(PointD left, int right)
        {
            return new PointD(left.X + right, left.Y + right);
        }

        public static PointD operator +(int left, PointD right)
        {
            return new PointD(left + right.X, left + right.Y);
        }

        public static PointD operator -(PointD left, PointD right)
        {
            return new PointD(left.X - right.X, left.Y - right.Y);
        }

        public static PointD operator -(PointD left, double right)
        {
            return new PointD(left.X - right, left.Y - right);
        }

        public static PointD operator -(double left, PointD right)
        {
            return new PointD(left - right.X, left - right.Y);
        }

        public static PointD operator -(PointD left, int right)
        {
            return new PointD(left.X - right, left.Y - right);
        }

        public static PointD operator -(int left, PointD right)
        {
            return new PointD(left - right.X, left - right.Y);
        }

        public static PointD operator /(PointD left, PointD right)
        {
            return new PointD(left.X / right.X, left.Y / right.Y);
        }

        public static PointD operator /(PointD left, double right)
        {
            return new PointD(left.X / right, left.Y / right);
        }

        public static PointD operator /(double left, PointD right)
        {
            return new PointD(left / right.X, left / right.Y);
        }

        public static PointD operator /(PointD left, int right)
        {
            return new PointD(left.X / right, left.Y / right);
        }

        public static PointD operator /(int left, PointD right)
        {
            return new PointD(left / right.X, left / right.Y);
        }

        public static PointD operator *(PointD left, PointD right)
        {
            return new PointD(left.X * right.X, left.Y * right.Y);
        }

        public static PointD operator *(PointD left, double right)
        {
            return new PointD(left.X * right, left.Y * right);
        }

        public static PointD operator *(double left, PointD right)
        {
            return new PointD(left * right.X, left * right.Y);
        }

        public static PointD operator *(PointD left, int right)
        {
            return new PointD(left.X * right, left.Y * right);
        }

        public static PointD operator *(int left, PointD right)
        {
            return new PointD(left * right.X, left * right.Y);
        }

        public static implicit operator XyCoordinate(PointD point)
        {
            return new XyCoordinate((double)point.X, (double)point.Y);
        }

        //public static implicit operator XyzCoordinate(PointD point)
        //{
        //    return new XyzCoordinate((double)point.X, (double)point.Y, 0.0);
        //}

        //public static implicit operator XytCoordinate(PointD point)
        //{
        //    return new XytCoordinate((double)point.X, (double)point.Y);
        //}

        //public static implicit operator XyztCoordinate(PointD point)
        //{
        //    return new XyztCoordinate((double)point.X, (double)point.Y, 0.0, 0.0);
        //}

        public static implicit operator Point(PointD point)
        {
            return new Point((int)point.X, (int)point.Y);
        }

        public static implicit operator PointF(PointD point)
        {
            return new PointF((float)point.X, (float)point.Y);
        }

        public static explicit operator PointD(Point point)
        {
            return new PointD((double)point.X, (double)point.Y);
        }

        public static explicit operator PointD(XyCoordinate coordinate)
        {
            return new PointD((double)coordinate.X, (double)coordinate.Y);
        }
        #endregion
    }

    public class PointDConverter : ExpandableObjectConverter
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
                PointD point = new PointD();
                point.X = double.Parse(array[0]);
                point.Y = double.Parse(array[1]);
                
                return point;
            }
            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string) && value is PointD)
            {
                PointD point = (PointD)value;
                return string.Format("{0}, {1}",
                    point.X,
                    point.Y
                    );
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
        //protected override ObjectCollection CreateInstanceDescriptor(object value)
        //{
        //    ObjectCollection constructMember = new ObjectCollection();
        //    constructMember.Add(this.GetPropertyValue(value, "X"));
        //    constructMember.Add(this.GetPropertyValue(value, "Y"));
        //    return constructMember;
        //}
        //protected override StringCollection GetListProperties()
        //{
        //    StringCollection propertyList = new StringCollection();
        //    propertyList.Add("X");
        //    propertyList.Add("Y");

        //    return propertyList;
        //}
    }

    [Serializable]
    public class PointDCollection : Collection<PointD>
    {
        #region Constructor
        public PointDCollection() : base() { }
        public PointDCollection(IList<PointD> list) : base(list) { }
        #endregion
    }

    #region PointCollection
    public class PointCollection : Collection<Point>
    {
        #region Define
        [Serializable]
        public enum Direction
        {
            X,
            Y,
        }
        #endregion

        #region Constructor
        public PointCollection() : base() { }
        public PointCollection(IList<Point> list) : base(list) { }
        #endregion

        #region Method
        public int GetMaximumIndex(Direction direction)
        {
            Point max = new Point(int.MinValue, int.MinValue);
            int i = 0;

            for (i = 0; i < this.Count; i++)
            {
                if (direction == Direction.X && max.X <= this[i].X)
                    max = this[i];
                else if (direction == Direction.Y && max.Y <= this[i].Y)
                    max = this[i];
            }

            return i;
        }

        public int GetMinimunIndex(Direction direction)
        {
            Point min = new Point(int.MaxValue, int.MaxValue);
            int i = 0;

            for (i = 0; i < this.Count; i++)
            {
                if (direction == Direction.X && this[i].X < min.X)
                    min = this[i];
                else if (direction == Direction.Y && this[i].Y < min.Y)
                    min = this[i];
            }

            return i;
        }
        #endregion
    }
    #endregion
}