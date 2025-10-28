using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QMC.Common
{
    //#region IXyCoordinate
    ///// <summary>
    ///// Cartesian Coordinate의 인터페이스를 정의한다.
    ///// </summary>
    //public interface IXyCoordinate
    //{
    //    double X { get; set; }
    //    double Y { get; set; }
    //}
    //#endregion

    [Serializable]
    public struct XyOfUvwCoordinate : IXyCoordinate
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
        public XyOfUvwCoordinate(double x, double y)
        {
            this.m_dX = x;
            this.m_dY = y;
        }
        #endregion

        #region Method
        public bool Equals(XyOfUvwCoordinate value)
        {
            return this == value;
        }
        #endregion

        #region Static Members
        public static bool operator ==(XyOfUvwCoordinate a, XyOfUvwCoordinate b)
        {
            if (a.X == b.X && a.Y == b.Y)
                return true;
            else
                return false;
        }

        public static bool operator !=(XyOfUvwCoordinate a, XyOfUvwCoordinate b)
        {
            if (a.X == b.X && a.Y == b.Y)
                return false;
            else
                return true;
        }

        public static bool operator >(XyOfUvwCoordinate a, XyOfUvwCoordinate b)
        {
            if (a.X > b.X && a.Y > b.Y)
                return true;
            else
                return false;
        }

        public static bool operator <(XyOfUvwCoordinate a, XyOfUvwCoordinate b)
        {
            if (a.X < b.X && a.Y < b.Y)
                return true;
            else
                return false;
        }

        public static XyOfUvwCoordinate operator +(XyOfUvwCoordinate a, XyOfUvwCoordinate b)
        {
            return new XyOfUvwCoordinate(a.X + b.X, a.Y + b.Y);
        }

        public static XyOfUvwCoordinate operator +(XyOfUvwCoordinate a, int b)
        {
            return new XyOfUvwCoordinate(a.X + b, a.Y + b);
        }

        public static XyOfUvwCoordinate operator +(int a, XyOfUvwCoordinate b)
        {
            return new XyOfUvwCoordinate(a + b.X, a + b.Y);
        }

        public static XyOfUvwCoordinate operator +(XyOfUvwCoordinate a, double b)
        {
            return new XyOfUvwCoordinate(a.X + b, a.Y + b);
        }

        public static XyOfUvwCoordinate operator +(double a, XyOfUvwCoordinate b)
        {
            return new XyOfUvwCoordinate(a + b.X, a + b.Y);
        }

        public static XyOfUvwCoordinate operator +(XyOfUvwCoordinate a, SizeD b)
        {
            return new XyOfUvwCoordinate(a.X + b.Width, a.Y + b.Height);
        }

        public static XyOfUvwCoordinate operator +(SizeD a, XyOfUvwCoordinate b)
        {
            return new XyOfUvwCoordinate(a.Width + b.X, a.Height + b.Y);
        }

        public static XyOfUvwCoordinate operator +(XyOfUvwCoordinate a, PointD b)
        {
            return new XyOfUvwCoordinate(a.X + b.X, a.Y + b.Y);
        }

        public static XyOfUvwCoordinate operator +(PointD a, XyOfUvwCoordinate b)
        {
            return new XyOfUvwCoordinate(a.X + b.X, a.Y + b.Y);
        }

        public static XyOfUvwCoordinate operator -(XyOfUvwCoordinate a, XyOfUvwCoordinate b)
        {
            return new XyOfUvwCoordinate(a.X - b.X, a.Y - b.Y);
        }

        public static XyOfUvwCoordinate operator -(XyOfUvwCoordinate a, int b)
        {
            return new XyOfUvwCoordinate(a.X - b, a.Y - b);
        }

        public static XyOfUvwCoordinate operator -(int a, XyOfUvwCoordinate b)
        {
            return new XyOfUvwCoordinate(a - b.X, a - b.Y);
        }

        public static XyOfUvwCoordinate operator -(XyOfUvwCoordinate a, double b)
        {
            return new XyOfUvwCoordinate(a.X - b, a.Y - b);
        }

        public static XyOfUvwCoordinate operator -(double a, XyOfUvwCoordinate b)
        {
            return new XyOfUvwCoordinate(a - b.X, a - b.Y);
        }

        public static XyOfUvwCoordinate operator -(XyOfUvwCoordinate a, SizeD b)
        {
            return new XyOfUvwCoordinate(a.X - b.Width, a.Y - b.Height);
        }

        public static XyOfUvwCoordinate operator -(SizeD a, XyOfUvwCoordinate b)
        {
            return new XyOfUvwCoordinate(a.Width - b.X, a.Height - b.Y);
        }

        public static XyOfUvwCoordinate operator -(XyOfUvwCoordinate a, PointD b)
        {
            return new XyOfUvwCoordinate(a.X - b.X, a.Y - b.Y);
        }

        public static XyOfUvwCoordinate operator -(PointD a, XyOfUvwCoordinate b)
        {
            return new XyOfUvwCoordinate(a.X - b.X, a.Y - b.Y);
        }

        public static XyOfUvwCoordinate operator *(XyOfUvwCoordinate a, XyOfUvwCoordinate b)
        {
            return new XyOfUvwCoordinate(a.X * b.X, a.Y * b.Y);
        }

        public static XyOfUvwCoordinate operator *(XyOfUvwCoordinate a, int b)
        {
            return new XyOfUvwCoordinate(a.X * b, a.Y * b);
        }

        public static XyOfUvwCoordinate operator *(int a, XyOfUvwCoordinate b)
        {
            return new XyOfUvwCoordinate(a * b.X, a * b.Y);
        }

        public static XyOfUvwCoordinate operator *(XyOfUvwCoordinate a, double b)
        {
            return new XyOfUvwCoordinate(a.X * b, a.Y * b);
        }

        public static XyOfUvwCoordinate operator *(double a, XyOfUvwCoordinate b)
        {
            return new XyOfUvwCoordinate(a * b.X, a * b.Y);
        }

        public static XyOfUvwCoordinate operator *(XyOfUvwCoordinate a, SizeD b)
        {
            return new XyOfUvwCoordinate(a.X * b.Width, a.Y * b.Height);
        }

        public static XyOfUvwCoordinate operator *(SizeD a, XyOfUvwCoordinate b)
        {
            return new XyOfUvwCoordinate(a.Width * b.X, a.Height * b.Y);
        }

        public static XyOfUvwCoordinate operator *(XyOfUvwCoordinate a, PointD b)
        {
            return new XyOfUvwCoordinate(a.X * b.X, a.Y * b.Y);
        }

        public static XyOfUvwCoordinate operator *(PointD a, XyOfUvwCoordinate b)
        {
            return new XyOfUvwCoordinate(a.X * b.X, a.Y * b.Y);
        }

        public static XyOfUvwCoordinate operator /(XyOfUvwCoordinate a, XyOfUvwCoordinate b)
        {
            return new XyOfUvwCoordinate(a.X / b.X, a.Y / b.Y);
        }

        public static XyOfUvwCoordinate operator /(XyOfUvwCoordinate a, int b)
        {
            return new XyOfUvwCoordinate(a.X / b, a.Y / b);
        }

        public static XyOfUvwCoordinate operator /(int a, XyOfUvwCoordinate b)
        {
            return new XyOfUvwCoordinate(a / b.X, a / b.Y);
        }

        public static XyOfUvwCoordinate operator /(XyOfUvwCoordinate a, double b)
        {
            return new XyOfUvwCoordinate(a.X / b, a.Y / b);
        }

        public static XyOfUvwCoordinate operator /(double a, XyOfUvwCoordinate b)
        {
            return new XyOfUvwCoordinate(a / b.X, a / b.Y);
        }

        public static XyOfUvwCoordinate operator /(XyOfUvwCoordinate a, SizeD b)
        {
            return new XyOfUvwCoordinate(a.X / b.Width, a.Y / b.Height);
        }

        public static XyOfUvwCoordinate operator /(SizeD a, XyOfUvwCoordinate b)
        {
            return new XyOfUvwCoordinate(a.Width / b.X, a.Height / b.Y);
        }

        public static XyOfUvwCoordinate operator /(XyOfUvwCoordinate a, PointD b)
        {
            return new XyOfUvwCoordinate(a.X / b.X, a.Y / b.Y);
        }

        public static XyOfUvwCoordinate operator /(PointD a, XyOfUvwCoordinate b)
        {
            return new XyOfUvwCoordinate(a.X / b.X, a.Y / b.Y);
        }

        //public static explicit operator XyCoordinate(PatternMatchingResult.PatternMatchingResultValue a)
        //{
        //    return new XyCoordinate(a.X, a.Y);
        //}

        public static explicit operator XyOfUvwCoordinate(XytCoordinate a)
        {
            return new XyOfUvwCoordinate(a.X, a.Y);
        }

        public static explicit operator XyOfUvwCoordinate(XyzCoordinate a)
        {
            return new XyOfUvwCoordinate(a.X, a.Y);
        }

        public static explicit operator XyOfUvwCoordinate(XyztCoordinate a)
        {
            return new XyOfUvwCoordinate(a.X, a.Y);
        }

        public static explicit operator XyOfUvwCoordinate(XyzztCoordinate a)
        {
            return new XyOfUvwCoordinate(a.X, a.Y);
        }
        #endregion

        #region Object Members
        public override bool Equals(object obj)
        {
            if (obj == null) return false;

            if (obj is XyOfUvwCoordinate == false) return false;
            XyOfUvwCoordinate value = (XyOfUvwCoordinate)obj;

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
    public class XyOfUvwCoordinateCollection : Collection<XyCoordinate>
    {
        #region Constructor
        public XyOfUvwCoordinateCollection(IList<XyCoordinate> list) : base(list) { }
        public XyOfUvwCoordinateCollection() : base() { }
        #endregion
    }
    #region IXyCoordinateReadOnlyCollection
    [Serializable]
    public class IXyOfUvwCoordinateReadOnlyCollection : ReadOnlyCollection<IXyCoordinate>
    {
        #region Constructor
        public IXyOfUvwCoordinateReadOnlyCollection(IList<IXyCoordinate> list) : base(list) { }
        #endregion
    }
    #endregion

}
