using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QMC.Common
{
    #region XyzCoordinate
    [Serializable]
    public struct XyzCoordinate : IXyCoordinate
    {
        #region Field
        private double m_X;
        private double m_Y;
        private double m_Z;
        #endregion

        #region Constructor
        public XyzCoordinate(double x, double y, double z)
        {
            this.m_X = x;
            this.m_Y = y;
            this.m_Z = z;
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

        public double Z
        {
            get { return this.m_Z; }
            set { this.m_Z = value; }
        }
        #endregion

        #region Method
        public bool Equals(XyzCoordinate value)
        {
            return this == value;
        }
        #endregion

        #region Static Members
        public static bool operator ==(XyzCoordinate a, XyzCoordinate b)
        {
            if (a.X == b.X && a.Y == b.Y && a.Z == b.Z)
                return true;
            else
                return false;
        }

        public static bool operator !=(XyzCoordinate a, XyzCoordinate b)
        {
            if (a.X == b.X && a.Y == b.Y && a.Z == b.Z)
                return false;
            else
                return true;
        }

        public static XyzCoordinate operator +(XyzCoordinate a, XyzCoordinate b)
        {
            return new XyzCoordinate(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        }

        public static XyzCoordinate operator +(XyzCoordinate a, XyCoordinate b)
        {
            return new XyzCoordinate(a.X + b.X, a.Y + b.Y, a.Z);
        }

        public static XyzCoordinate operator +(XyCoordinate a, XyzCoordinate b)
        {
            return new XyzCoordinate(a.X + b.X, a.Y + b.Y, b.Z);
        }

        public static XyzCoordinate operator +(XyzCoordinate a, SizeD b)
        {
            return new XyzCoordinate(a.X + b.Width, a.Y + b.Height, a.Z);
        }

        public static XyzCoordinate operator +(SizeD a, XyzCoordinate b)
        {
            return new XyzCoordinate(a.Width + b.X, a.Height + b.Y, b.Z);
        }

        public static XyzCoordinate operator +(XyzCoordinate a, PointD b)
        {
            return new XyzCoordinate(a.X + b.X, a.Y + b.Y, a.Z);
        }

        public static XyzCoordinate operator +(PointD a, XyzCoordinate b)
        {
            return new XyzCoordinate(a.X + b.X, a.Y + b.Y, b.Z);
        }

        public static XyzCoordinate operator +(XyzCoordinate a, int b)
        {
            return new XyzCoordinate(a.X + b, a.Y + b, a.Z + b);
        }

        public static XyzCoordinate operator +(int a, XyzCoordinate b)
        {
            return new XyzCoordinate(a + b.X, a + b.Y, a + b.Z);
        }

        public static XyzCoordinate operator +(XyzCoordinate a, double b)
        {
            return new XyzCoordinate(a.X + b, a.Y + b, a.Z + b);
        }

        public static XyzCoordinate operator +(double a, XyzCoordinate b)
        {
            return new XyzCoordinate(a + b.X, a + b.Y, a + b.Z);
        }

        public static XyzCoordinate operator -(XyzCoordinate a, XyzCoordinate b)
        {
            return new XyzCoordinate(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        }

        public static XyzCoordinate operator -(XyzCoordinate a, XyCoordinate b)
        {
            return new XyzCoordinate(a.X - b.X, a.Y - b.Y, a.Z);
        }

        public static XyzCoordinate operator -(XyCoordinate a, XyzCoordinate b)
        {
            return new XyzCoordinate(a.X - b.X, a.Y - b.Y, b.Z);
        }

        public static XyzCoordinate operator -(XyzCoordinate a, SizeD b)
        {
            return new XyzCoordinate(a.X - b.Width, a.Y - b.Height, a.Z);
        }

        public static XyzCoordinate operator -(SizeD a, XyzCoordinate b)
        {
            return new XyzCoordinate(a.Width - b.X, a.Height - b.Y, b.Z);
        }

        public static XyzCoordinate operator -(XyzCoordinate a, PointD b)
        {
            return new XyzCoordinate(a.X - b.X, a.Y - b.Y, a.Z);
        }

        public static XyzCoordinate operator -(PointD a, XyzCoordinate b)
        {
            return new XyzCoordinate(a.X - b.X, a.Y - b.Y, b.Z);
        }

        public static XyzCoordinate operator -(XyzCoordinate a, int b)
        {
            return new XyzCoordinate(a.X - b, a.Y - b, a.Z - b);
        }

        public static XyzCoordinate operator -(int a, XyzCoordinate b)
        {
            return new XyzCoordinate(a - b.X, a - b.Y, a - b.Z);
        }

        public static XyzCoordinate operator -(XyzCoordinate a, double b)
        {
            return new XyzCoordinate(a.X - b, a.Y - b, a.Z - b);
        }

        public static XyzCoordinate operator -(double a, XyzCoordinate b)
        {
            return new XyzCoordinate(a - b.X, a - b.Y, a - b.Z);
        }

        public static XyzCoordinate operator *(XyzCoordinate a, XyzCoordinate b)
        {
            return new XyzCoordinate(a.X * b.X, a.Y * b.Y, a.Z * b.Z);
        }

        public static XyzCoordinate operator *(XyzCoordinate a, XyCoordinate b)
        {
            return new XyzCoordinate(a.X * b.X, a.Y * b.Y, a.Z);
        }

        public static XyzCoordinate operator *(XyCoordinate a, XyzCoordinate b)
        {
            return new XyzCoordinate(a.X * b.X, a.Y * b.Y, b.Z);
        }

        public static XyzCoordinate operator *(XyzCoordinate a, SizeD b)
        {
            return new XyzCoordinate(a.X * b.Width, a.Y * b.Height, a.Z);
        }

        public static XyzCoordinate operator *(SizeD a, XyzCoordinate b)
        {
            return new XyzCoordinate(a.Width * b.X, a.Height * b.Y, b.Z);
        }

        public static XyzCoordinate operator *(XyzCoordinate a, PointD b)
        {
            return new XyzCoordinate(a.X * b.X, a.Y * b.Y, a.Z);
        }

        public static XyzCoordinate operator *(PointD a, XyzCoordinate b)
        {
            return new XyzCoordinate(a.X * b.X, a.Y * b.Y, b.Z);
        }

        public static XyzCoordinate operator *(XyzCoordinate a, int b)
        {
            return new XyzCoordinate(a.X * b, a.Y * b, a.Z * b);
        }

        public static XyzCoordinate operator *(int a, XyzCoordinate b)
        {
            return new XyzCoordinate(a * b.X, a * b.Y, a * b.Z);
        }

        public static XyzCoordinate operator *(XyzCoordinate a, double b)
        {
            return new XyzCoordinate(a.X * b, a.Y * b, a.Z * b);
        }

        public static XyzCoordinate operator *(double a, XyzCoordinate b)
        {
            return new XyzCoordinate(a * b.X, a * b.Y, a * b.Z);
        }

        public static XyzCoordinate operator /(XyzCoordinate a, XyzCoordinate b)
        {
            return new XyzCoordinate(a.X / b.X, a.Y / b.Y, a.Z / b.Z);
        }

        public static XyzCoordinate operator /(XyzCoordinate a, XyCoordinate b)
        {
            return new XyzCoordinate(a.X / b.X, a.Y / b.Y, a.Z);
        }

        public static XyzCoordinate operator /(XyCoordinate a, XyzCoordinate b)
        {
            return new XyzCoordinate(a.X / b.X, a.Y / b.Y, b.Z);
        }

        public static XyzCoordinate operator /(XyzCoordinate a, SizeD b)
        {
            return new XyzCoordinate(a.X / b.Width, a.Y / b.Height, a.Z);
        }

        public static XyzCoordinate operator /(SizeD a, XyzCoordinate b)
        {
            return new XyzCoordinate(a.Width / b.X, a.Height / b.Y, b.Z);
        }

        public static XyzCoordinate operator /(XyzCoordinate a, PointD b)
        {
            return new XyzCoordinate(a.X / b.X, a.Y / b.Y, a.Z);
        }

        public static XyzCoordinate operator /(PointD a, XyzCoordinate b)
        {
            return new XyzCoordinate(a.X / b.X, a.Y / b.Y, b.Z);
        }

        public static XyzCoordinate operator /(XyzCoordinate a, int b)
        {
            return new XyzCoordinate(a.X / b, a.Y / b, a.Z / b);
        }

        public static XyzCoordinate operator /(int a, XyzCoordinate b)
        {
            return new XyzCoordinate(a / b.X, a / b.Y, a / b.Z);
        }

        public static XyzCoordinate operator /(XyzCoordinate a, double b)
        {
            return new XyzCoordinate(a.X / b, a.Y / b, a.Z / b);
        }

        public static XyzCoordinate operator /(double a, XyzCoordinate b)
        {
            return new XyzCoordinate(a / b.X, a / b.Y, a / b.Z);
        }

        public static explicit operator XyzCoordinate(XyCoordinate a)
        {
            return new XyzCoordinate(a.X, a.Y, 0.0);
        }

        public static explicit operator XyzCoordinate(XytCoordinate a)
        {
            return new XyzCoordinate(a.X, a.Y, 0.0);
        }

        public static explicit operator XyzCoordinate(XyztCoordinate a)
        {
            return new XyzCoordinate(a.X, a.Y, a.Z);
        }

        public static explicit operator XyzCoordinate(PointD a)
        {
            return new XyzCoordinate(a.X, a.Y, 0.0);
        }
        #endregion

        #region Object Members
        public override bool Equals(object obj)
        {
            if (obj == null) return false;

            if (obj is XyzCoordinate == false) return false;
            XyzCoordinate value = (XyzCoordinate)obj;

            return this.Equals(value);
        }

        public override int GetHashCode()
        {
            return this.X.GetHashCode() ^ this.Y.GetHashCode() ^ this.Z.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("[{0}, {1}, {2}]", this.X, this.Y, this.Z);
        }
        #endregion
    }
    #endregion

    #region XyzCoordinateCollection
    [Serializable]
    public class XyzCoordinateCollection : Collection<XyzCoordinate>
    {
        #region Constructor
        public XyzCoordinateCollection(IList<XyzCoordinate> list) : base(list) { }
        public XyzCoordinateCollection() : base() { }
        #endregion
    }
    #endregion
}
