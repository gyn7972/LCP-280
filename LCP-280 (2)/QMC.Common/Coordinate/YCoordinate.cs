using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QMC.Common
{
    #region IYCoordinate
    /// <summary>
    /// Cartesian Coordinate의 인터페이스를 정의한다.
    /// </summary>
    public interface IYCoordinate
    {
        double Y { get; set; }
    }
    #endregion

    [Serializable]
    public struct YCoordinate : IYCoordinate
    {
        #region Field
        private double m_dY;
        #endregion

        #region Property
        public double Y
        {
            get { return this.m_dY; }
            set { this.m_dY = value; }
        }
        #endregion

        #region Constructor
        public YCoordinate(double y)
        {
            this.m_dY = y;
        }
        #endregion

        #region Method
        public bool Equals(YCoordinate value)
        {
            return this == value;
        }
        #endregion

        #region Static Members
        public static bool operator ==(YCoordinate a, YCoordinate b)
        {
            if (a.Y == b.Y)
                return true;
            else
                return false;
        }

        public static bool operator !=(YCoordinate a, YCoordinate b)
        {
            if (a.Y == b.Y)
                return false;
            else
                return true;
        }

        public static bool operator >(YCoordinate a, YCoordinate b)
        {
            if (a.Y > b.Y)
                return true;
            else
                return false;
        }

        public static bool operator <(YCoordinate a, YCoordinate b)
        {
            if (a.Y < b.Y)
                return true;
            else
                return false;
        }

        public static YCoordinate operator +(YCoordinate a, YCoordinate b)
        {
            return new YCoordinate(a.Y + b.Y);
        }

        public static YCoordinate operator +(YCoordinate a, int b)
        {
            return new YCoordinate(a.Y + b);
        }

        public static YCoordinate operator +(int a, YCoordinate b)
        {
            return new YCoordinate(a + b.Y);
        }

        public static YCoordinate operator +(YCoordinate a, double b)
        {
            return new YCoordinate(a.Y + b);
        }

        public static YCoordinate operator +(double a, YCoordinate b)
        {
            return new YCoordinate(a + b.Y);
        }

        public static YCoordinate operator +(YCoordinate a, SizeD b)
        {
            return new YCoordinate(a.Y + b.Height);
        }

        public static YCoordinate operator +(SizeD a, YCoordinate b)
        {
            return new YCoordinate(a.Height + b.Y);
        }

        public static YCoordinate operator +(YCoordinate a, PointD b)
        {
            return new YCoordinate(a.Y + b.Y);
        }

        public static YCoordinate operator +(PointD a, YCoordinate b)
        {
            return new YCoordinate(a.Y + b.Y);
        }

        public static YCoordinate operator -(YCoordinate a, YCoordinate b)
        {
            return new YCoordinate(a.Y - b.Y);
        }

        public static YCoordinate operator -(YCoordinate a, int b)
        {
            return new YCoordinate(a.Y - b);
        }

        public static YCoordinate operator -(int a, YCoordinate b)
        {
            return new YCoordinate(a - b.Y);
        }

        public static YCoordinate operator -(YCoordinate a, double b)
        {
            return new YCoordinate(a.Y - b);
        }

        public static YCoordinate operator -(double a, YCoordinate b)
        {
            return new YCoordinate(a - b.Y);
        }

        public static YCoordinate operator -(YCoordinate a, SizeD b)
        {
            return new YCoordinate(a.Y - b.Height);
        }

        public static YCoordinate operator -(SizeD a, YCoordinate b)
        {
            return new YCoordinate(a.Height - b.Y);
        }

        public static YCoordinate operator -(YCoordinate a, PointD b)
        {
            return new YCoordinate(a.Y - b.Y);
        }

        public static YCoordinate operator -(PointD a, YCoordinate b)
        {
            return new YCoordinate(a.Y - b.Y);
        }

        public static YCoordinate operator *(YCoordinate a, YCoordinate b)
        {
            return new YCoordinate(a.Y * b.Y);
        }

        public static YCoordinate operator *(YCoordinate a, int b)
        {
            return new YCoordinate(a.Y * b);
        }

        public static YCoordinate operator *(int a, YCoordinate b)
        {
            return new YCoordinate(a * b.Y);
        }

        public static YCoordinate operator *(YCoordinate a, double b)
        {
            return new YCoordinate(a.Y * b);
        }

        public static YCoordinate operator *(double a, YCoordinate b)
        {
            return new YCoordinate(a * b.Y);
        }

        public static YCoordinate operator *(YCoordinate a, SizeD b)
        {
            return new YCoordinate(a.Y * b.Height);
        }

        public static YCoordinate operator *(SizeD a, YCoordinate b)
        {
            return new YCoordinate(a.Height * b.Y);
        }

        public static YCoordinate operator *(YCoordinate a, PointD b)
        {
            return new YCoordinate(a.Y * b.Y);
        }

        public static YCoordinate operator *(PointD a, YCoordinate b)
        {
            return new YCoordinate(a.Y * b.Y);
        }

        public static YCoordinate operator /(YCoordinate a, YCoordinate b)
        {
            return new YCoordinate(a.Y / b.Y);
        }

        public static YCoordinate operator /(YCoordinate a, int b)
        {
            return new YCoordinate(a.Y / b);
        }

        public static YCoordinate operator /(int a, YCoordinate b)
        {
            return new YCoordinate(a / b.Y);
        }

        public static YCoordinate operator /(YCoordinate a, double b)
        {
            return new YCoordinate(a.Y / b);
        }

        public static YCoordinate operator /(double a, YCoordinate b)
        {
            return new YCoordinate(a / b.Y);
        }

        public static YCoordinate operator /(YCoordinate a, SizeD b)
        {
            return new YCoordinate(a.Y / b.Height);
        }

        public static YCoordinate operator /(SizeD a, YCoordinate b)
        {
            return new YCoordinate(a.Height / b.Y);
        }

        public static YCoordinate operator /(YCoordinate a, PointD b)
        {
            return new YCoordinate(a.Y / b.Y);
        }

        public static YCoordinate operator /(PointD a, YCoordinate b)
        {
            return new YCoordinate(a.Y / b.Y);
        }

        //public static explicit operator YCoordinate(PatternMatchingResult.PatternMatchingResultValue a)
        //{
        //    return new YCoordinate(a.X, a.Y);
        //}

        public static explicit operator YCoordinate(XytCoordinate a)
        {
            return new YCoordinate(a.Y);
        }

        public static explicit operator YCoordinate(XyzCoordinate a)
        {
            return new YCoordinate(a.Y);
        }

        public static explicit operator YCoordinate(XyztCoordinate a)
        {
            return new YCoordinate(a.Y);
        }

        public static explicit operator YCoordinate(XyzztCoordinate a)
        {
            return new YCoordinate(a.Y);
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
            return this.Y.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("[{0}]", this.Y);
        }
        #endregion
    }

    [Serializable]
    public class YCoordinateCollection : Collection<YCoordinate>
    {
        #region Constructor
        public YCoordinateCollection(IList<YCoordinate> list) : base(list) { }
        public YCoordinateCollection() : base() { }
        #endregion
    }
    #region IYCoordinateReadOnlyCollection
    [Serializable]
    public class IYCoordinateReadOnlyCollection : ReadOnlyCollection<IYCoordinate>
    {
        #region Constructor
        public IYCoordinateReadOnlyCollection(IList<IYCoordinate> list) : base(list) { }
        #endregion
    }
    #endregion

}
