using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QMC.Common
{
    #region XytCoordinate
    [Serializable]
    public struct XytCoordinate : IXyCoordinate
    {
        #region Field
        private double m_X;
        private double m_Y;
        private double m_T;
        #endregion

        #region Constructor
        public XytCoordinate(double x, double y, double t)
        {
            this.m_X = x;
            this.m_Y = y;
            this.m_T = t;
        }
        public XytCoordinate(double x, double y) : this(x, y, 0.0) { }
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

        public double T
        {
            get { return this.m_T; }
            set { this.m_T = value; }
        }
        #endregion

        #region Method
        public bool IsValidData()
        {
            bool bRet = true;
            
            if(X ==0 && Y == 0 && T == 0)
                bRet = false;

            return bRet;
        }
        public bool Equals(XytCoordinate value)
        {
            return this == value;
        }
        #endregion

        #region Static Members
        public static bool operator ==(XytCoordinate a, XytCoordinate b)
        {
            if (a.X == b.X && a.Y == b.Y && a.T == b.T)
                return true;
            else
                return false;
        }

        public static bool operator !=(XytCoordinate a, XytCoordinate b)
        {
            if (a.X == b.X && a.Y == b.Y && a.T == b.T)
                return false;
            else
                return true;
        }

        public static XytCoordinate operator +(XytCoordinate a, XytCoordinate b)
        {
            return new XytCoordinate(a.X + b.X, a.Y + b.Y, a.T + b.T);
        }

        public static XytCoordinate operator +(XytCoordinate a, XyCoordinate b)
        {
            return new XytCoordinate(a.X + b.X, a.Y + b.Y, a.T);
        }

        public static XytCoordinate operator +(XyCoordinate a, XytCoordinate b)
        {
            return new XytCoordinate(a.X + b.X, a.Y + b.Y, b.T);
        }

        public static XytCoordinate operator +(XytCoordinate a, int b)
        {
            return new XytCoordinate(a.X + b, a.Y + b, a.T + b);
        }

        public static XytCoordinate operator +(int a, XytCoordinate b)
        {
            return new XytCoordinate(a + b.X, a + b.Y, a + b.T);
        }

        public static XytCoordinate operator +(XytCoordinate a, double b)
        {
            return new XytCoordinate(a.X + b, a.Y + b, a.T + b);
        }

        public static XytCoordinate operator +(double a, XytCoordinate b)
        {
            return new XytCoordinate(a + b.X, a + b.Y, a + b.T);
        }

        public static XytCoordinate operator +(XytCoordinate a, SizeD b)
        {
            return new XytCoordinate(a.X + b.Width, a.Y + b.Height, a.T);
        }

        public static XytCoordinate operator +(SizeD a, XytCoordinate b)
        {
            return new XytCoordinate(a.Width + b.X, a.Height + b.Y, b.T);
        }

        public static XytCoordinate operator +(XytCoordinate a, PointD b)
        {
            return new XytCoordinate(a.X + b.X, a.Y + b.Y, a.T);
        }

        public static XytCoordinate operator +(PointD a, XytCoordinate b)
        {
            return new XytCoordinate(a.X + b.X, a.Y + b.Y, b.T);
        }

        public static XytCoordinate operator -(XytCoordinate a, XytCoordinate b)
        {
            return new XytCoordinate(a.X - b.X, a.Y - b.Y, a.T - b.T);
        }

        public static XytCoordinate operator -(XytCoordinate a, XyCoordinate b)
        {
            return new XytCoordinate(a.X - b.X, a.Y - b.Y, a.T);
        }

        public static XytCoordinate operator -(XyCoordinate a, XytCoordinate b)
        {
            return new XytCoordinate(a.X - b.X, a.Y - b.Y, b.T);
        }

        public static XytCoordinate operator -(XytCoordinate a, int b)
        {
            return new XytCoordinate(a.X - b, a.Y - b, a.T - b);
        }

        public static XytCoordinate operator -(int a, XytCoordinate b)
        {
            return new XytCoordinate(a - b.X, a - b.Y, a - b.T);
        }

        public static XytCoordinate operator -(XytCoordinate a, double b)
        {
            return new XytCoordinate(a.X - b, a.Y - b, a.T - b);
        }

        public static XytCoordinate operator -(double a, XytCoordinate b)
        {
            return new XytCoordinate(a - b.X, a - b.Y, a - b.T);
        }

        public static XytCoordinate operator -(XytCoordinate a, SizeD b)
        {
            return new XytCoordinate(a.X - b.Width, a.Y - b.Height, a.T);
        }

        public static XytCoordinate operator -(SizeD a, XytCoordinate b)
        {
            return new XytCoordinate(a.Width - b.X, a.Height - b.Y, b.T);
        }

        public static XytCoordinate operator -(XytCoordinate a, PointD b)
        {
            return new XytCoordinate(a.X - b.X, a.Y - b.Y, a.T);
        }

        public static XytCoordinate operator -(PointD a, XytCoordinate b)
        {
            return new XytCoordinate(a.X - b.X, a.Y - b.Y, b.T);
        }

        public static XytCoordinate operator *(XytCoordinate a, XytCoordinate b)
        {
            return new XytCoordinate(a.X * b.X, a.Y * b.Y, a.T * b.T);
        }

        public static XytCoordinate operator *(XytCoordinate a, XyCoordinate b)
        {
            return new XytCoordinate(a.X * b.X, a.Y * b.Y, a.T);
        }

        public static XytCoordinate operator *(XyCoordinate a, XytCoordinate b)
        {
            return new XytCoordinate(a.X * b.X, a.Y * b.Y, b.T);
        }

        public static XytCoordinate operator *(XytCoordinate a, int b)
        {
            return new XytCoordinate(a.X * b, a.Y * b, a.T * b);
        }

        public static XytCoordinate operator *(int a, XytCoordinate b)
        {
            return new XytCoordinate(a * b.X, a * b.Y, a * b.T);
        }

        public static XytCoordinate operator *(XytCoordinate a, double b)
        {
            return new XytCoordinate(a.X * b, a.Y * b, a.T * b);
        }

        public static XytCoordinate operator *(double a, XytCoordinate b)
        {
            return new XytCoordinate(a * b.X, a * b.Y, a * b.T);
        }

        public static XytCoordinate operator *(XytCoordinate a, SizeD b)
        {
            return new XytCoordinate(a.X * b.Width, a.Y * b.Height, a.T);
        }

        public static XytCoordinate operator *(SizeD a, XytCoordinate b)
        {
            return new XytCoordinate(a.Width * b.X, a.Height * b.Y, b.T);
        }

        public static XytCoordinate operator *(XytCoordinate a, PointD b)
        {
            return new XytCoordinate(a.X * b.X, a.Y * b.Y, a.T);
        }

        public static XytCoordinate operator *(PointD a, XytCoordinate b)
        {
            return new XytCoordinate(a.X * b.X, a.Y * b.Y, b.T);
        }

        public static XytCoordinate operator /(XytCoordinate a, XytCoordinate b)
        {
            return new XytCoordinate(a.X / b.X, a.Y / b.Y, a.T / b.T);
        }

        public static XytCoordinate operator /(XytCoordinate a, XyCoordinate b)
        {
            return new XytCoordinate(a.X / b.X, a.Y / b.Y, a.T);
        }

        public static XytCoordinate operator /(XyCoordinate a, XytCoordinate b)
        {
            return new XytCoordinate(a.X / b.X, a.Y / b.Y, b.T);
        }

        public static XytCoordinate operator /(XytCoordinate a, int b)
        {
            return new XytCoordinate(a.X / b, a.Y / b, a.T / b);
        }

        public static XytCoordinate operator /(int a, XytCoordinate b)
        {
            return new XytCoordinate(a / b.X, a / b.Y, a / b.T);
        }

        public static XytCoordinate operator /(XytCoordinate a, double b)
        {
            return new XytCoordinate(a.X / b, a.Y / b, a.T / b);
        }

        public static XytCoordinate operator /(double a, XytCoordinate b)
        {
            return new XytCoordinate(a / b.X, a / b.Y, a / b.T);
        }

        public static XytCoordinate operator /(XytCoordinate a, SizeD b)
        {
            return new XytCoordinate(a.X / b.Width, a.Y / b.Height, a.T);
        }

        public static XytCoordinate operator /(SizeD a, XytCoordinate b)
        {
            return new XytCoordinate(a.Width / b.X, a.Height / b.Y, b.T);
        }

        public static XytCoordinate operator /(XytCoordinate a, PointD b)
        {
            return new XytCoordinate(a.X / b.X, a.Y / b.Y, a.T);
        }

        public static XytCoordinate operator /(PointD a, XytCoordinate b)
        {
            return new XytCoordinate(a.X / b.X, a.Y / b.Y, b.T);
        }

        //public static explicit operator XytCoordinate(PatternMatchingResult.PatternMatchingResultValue a)
        //{
        //    return new XytCoordinate(a.X, a.Y, a.R);
        //}

        public static explicit operator XytCoordinate(XyCoordinate a)
        {
            return new XytCoordinate(a.X, a.Y, 0.0);
        }

        public static explicit operator XytCoordinate(XyzCoordinate a)
        {
            return new XytCoordinate(a.X, a.Y, 0.0);
        }

        public static explicit operator XytCoordinate(XyztCoordinate a)
        {
            return new XytCoordinate(a.X, a.Y, a.T);
        }
        #endregion

        #region Object Members
        public override bool Equals(object obj)
        {
            if (obj == null) return false;

            if (obj is XytCoordinate == false) return false;
            XytCoordinate value = (XytCoordinate)obj;

            return this.Equals(value);
        }

        public override int GetHashCode()
        {
            return this.X.GetHashCode() ^ this.Y.GetHashCode() ^ this.T.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("[{0}, {1}, {2}]", this.X, this.Y, this.T);
        }
        #endregion
    }
    #endregion

    #region XytCoordinateCollection
    [Serializable]
    public class XytCoordinateCollection : Collection<XytCoordinate>
    {
        #region Constructor
        public XytCoordinateCollection(IList<XytCoordinate> list) : base(list) { }
        public XytCoordinateCollection() : base() { }
        #endregion
    }
    #endregion
}
