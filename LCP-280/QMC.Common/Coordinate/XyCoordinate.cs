using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QMC.Common
{
    #region IXyCoordinate
    /// <summary>
    /// Cartesian Coordinate의 인터페이스를 정의한다.
    /// </summary>
    public interface IXyCoordinate
    {
        double X { get; set; }
        double Y { get; set; }
    }
    #endregion

    [Serializable]
    public struct XyCoordinate : IXyCoordinate
    {
        #region Field
        private double m_dX;
        private double m_dY;
        #endregion

        #region Property
        public double X
        {
            get { return this.m_dX; }
            set { this.m_dX = value; }
        }
        public double Y
        {
            get { return this.m_dY; }
            set { this.m_dY = value; }
        }
        #endregion

        #region Constructor
        public XyCoordinate(double x, double y)
        {
            this.m_dX = x;
            this.m_dY = y;
        }
        #endregion

        #region Method
        public bool Equals(XyCoordinate value)
        {
            return this == value;
        }
        #endregion

        #region Static Members
        public static bool operator ==(XyCoordinate a, XyCoordinate b)
        {
            if (a.X == b.X && a.Y == b.Y)
                return true;
            else
                return false;
        }

        public static bool operator !=(XyCoordinate a, XyCoordinate b)
        {
            if (a.X == b.X && a.Y == b.Y)
                return false;
            else
                return true;
        }

        public static bool operator >(XyCoordinate a, XyCoordinate b)
        {
            if (a.X > b.X && a.Y > b.Y)
                return true;
            else
                return false;
        }

        public static bool operator <(XyCoordinate a, XyCoordinate b)
        {
            if (a.X < b.X && a.Y < b.Y)
                return true;
            else
                return false;
        }

        public static XyCoordinate operator +(XyCoordinate a, XyCoordinate b)
        {
            return new XyCoordinate(a.X + b.X, a.Y + b.Y);
        }

        public static XyCoordinate operator +(XyCoordinate a, int b)
        {
            return new XyCoordinate(a.X + b, a.Y + b);
        }

        public static XyCoordinate operator +(int a, XyCoordinate b)
        {
            return new XyCoordinate(a + b.X, a + b.Y);
        }
        
        public static XyCoordinate operator +(XyCoordinate a, double b)
        {
            return new XyCoordinate(a.X + b, a.Y + b);
        }

        public static XyCoordinate operator +(double a, XyCoordinate b)
        {
            return new XyCoordinate(a + b.X, a + b.Y);
        }

        public static XyCoordinate operator +(XyCoordinate a, SizeD b)
        {
            return new XyCoordinate(a.X + b.Width, a.Y + b.Height);
        }

        public static XyCoordinate operator +(SizeD a, XyCoordinate b)
        {
            return new XyCoordinate(a.Width + b.X, a.Height + b.Y);
        }

        public static XyCoordinate operator +(XyCoordinate a, PointD b)
        {
            return new XyCoordinate(a.X + b.X, a.Y + b.Y);
        }

        public static XyCoordinate operator +(PointD a, XyCoordinate b)
        {
            return new XyCoordinate(a.X + b.X, a.Y + b.Y);
        }

        public static XyCoordinate operator -(XyCoordinate a, XyCoordinate b)
        {
            return new XyCoordinate(a.X - b.X, a.Y - b.Y);
        }

        public static XyCoordinate operator -(XyCoordinate a, int b)
        {
            return new XyCoordinate(a.X - b, a.Y - b);
        }

        public static XyCoordinate operator -(int a, XyCoordinate b)
        {
            return new XyCoordinate(a - b.X, a - b.Y);
        }

        public static XyCoordinate operator -(XyCoordinate a, double b)
        {
            return new XyCoordinate(a.X - b, a.Y - b);
        }

        public static XyCoordinate operator -(double a, XyCoordinate b)
        {
            return new XyCoordinate(a - b.X, a - b.Y);
        }

        public static XyCoordinate operator -(XyCoordinate a, SizeD b)
        {
            return new XyCoordinate(a.X - b.Width, a.Y - b.Height);
        }

        public static XyCoordinate operator -(SizeD a, XyCoordinate b)
        {
            return new XyCoordinate(a.Width - b.X, a.Height - b.Y);
        }

        public static XyCoordinate operator -(XyCoordinate a, PointD b)
        {
            return new XyCoordinate(a.X - b.X, a.Y - b.Y);
        }

        public static XyCoordinate operator -(PointD a, XyCoordinate b)
        {
            return new XyCoordinate(a.X - b.X, a.Y - b.Y);
        }

        public static XyCoordinate operator *(XyCoordinate a, XyCoordinate b)
        {
            return new XyCoordinate(a.X * b.X, a.Y * b.Y);
        }

        public static XyCoordinate operator *(XyCoordinate a, int b)
        {
            return new XyCoordinate(a.X * b, a.Y * b);
        }

        public static XyCoordinate operator *(int a, XyCoordinate b)
        {
            return new XyCoordinate(a * b.X, a * b.Y);
        }

        public static XyCoordinate operator *(XyCoordinate a, double b)
        {
            return new XyCoordinate(a.X * b, a.Y * b);
        }

        public static XyCoordinate operator *(double a, XyCoordinate b)
        {
            return new XyCoordinate(a * b.X, a * b.Y);
        }

        public static XyCoordinate operator *(XyCoordinate a, SizeD b)
        {
            return new XyCoordinate(a.X * b.Width, a.Y * b.Height);
        }

        public static XyCoordinate operator *(SizeD a, XyCoordinate b)
        {
            return new XyCoordinate(a.Width * b.X, a.Height * b.Y);
        }

        public static XyCoordinate operator *(XyCoordinate a, PointD b)
        {
            return new XyCoordinate(a.X * b.X, a.Y * b.Y);
        }

        public static XyCoordinate operator *(PointD a, XyCoordinate b)
        {
            return new XyCoordinate(a.X * b.X, a.Y * b.Y);
        }

        public static XyCoordinate operator /(XyCoordinate a, XyCoordinate b)
        {
            return new XyCoordinate(a.X / b.X, a.Y / b.Y);
        }

        public static XyCoordinate operator /(XyCoordinate a, int b)
        {
            return new XyCoordinate(a.X / b, a.Y / b);
        }

        public static XyCoordinate operator /(int a, XyCoordinate b)
        {
            return new XyCoordinate(a / b.X, a / b.Y);
        }

        public static XyCoordinate operator /(XyCoordinate a, double b)
        {
            return new XyCoordinate(a.X / b, a.Y / b);
        }

        public static XyCoordinate operator /(double a, XyCoordinate b)
        {
            return new XyCoordinate(a / b.X, a / b.Y);
        }

        public static XyCoordinate operator /(XyCoordinate a, SizeD b)
        {
            return new XyCoordinate(a.X / b.Width, a.Y / b.Height);
        }

        public static XyCoordinate operator /(SizeD a, XyCoordinate b)
        {
            return new XyCoordinate(a.Width / b.X, a.Height / b.Y);
        }

        public static XyCoordinate operator /(XyCoordinate a, PointD b)
        {
            return new XyCoordinate(a.X / b.X, a.Y / b.Y);
        }

        public static XyCoordinate operator /(PointD a, XyCoordinate b)
        {
            return new XyCoordinate(a.X / b.X, a.Y / b.Y);
        }

        //public static explicit operator XyCoordinate(PatternMatchingResult.PatternMatchingResultValue a)
        //{
        //    return new XyCoordinate(a.X, a.Y);
        //}

        public static explicit operator XyCoordinate(XytCoordinate a)
        {
            return new XyCoordinate(a.X, a.Y);
        }
       
        public static explicit operator XyCoordinate(XyzCoordinate a)
        {
            return new XyCoordinate(a.X, a.Y);
        }

        public static explicit operator XyCoordinate(XyztCoordinate a)
        {
            return new XyCoordinate(a.X, a.Y);
        }

        public static explicit operator XyCoordinate(XyzztCoordinate a)
        {
            return new XyCoordinate(a.X, a.Y);
        }
        #endregion

        #region Object Members
        public override bool Equals(object obj)
        {
            if (obj == null) return false;

            if (obj is XyCoordinate == false) return false;
            XyCoordinate value = (XyCoordinate)obj;

            return this.Equals(value);
        }

        public override int GetHashCode()
        {
            return this.X.GetHashCode() ^ this.Y.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("[{0}, {1}]", this.X, this.Y);
        }
        #endregion
    }

    [Serializable]
    public class XyCoordinateCollection : Collection<XyCoordinate>
    {
        #region Constructor
        public XyCoordinateCollection(IList<XyCoordinate> list) : base(list) { }
        public XyCoordinateCollection() : base() { }
        #endregion
    }
    #region IXyCoordinateReadOnlyCollection
    [Serializable]
    public class IXyCoordinateReadOnlyCollection : ReadOnlyCollection<IXyCoordinate>
    {
        #region Constructor
        public IXyCoordinateReadOnlyCollection(IList<IXyCoordinate> list) : base(list) { }
        #endregion
    }
    #endregion

}
