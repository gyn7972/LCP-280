using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QMC.Common
{
    #region XyztCoordinate
    [Serializable]
    public struct XyztCoordinate : IXyCoordinate
    {
        #region Field
        private double m_X;
        private double m_Y;
        private double m_Z;
        private double m_T;
        #endregion

        #region Constructor
        public XyztCoordinate(double x, double y, double z, double t)
        {
            this.m_X = x;
            this.m_Y = y;
            this.m_Z = z;
            this.m_T = t;
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

        public double T
        {
            get { return this.m_T; }
            set { this.m_T = value; }
        }
        #endregion

        #region Method
        public bool Equals(XyztCoordinate value)
        {
            return this == value;
        }
        #endregion

        #region Static Members
        public static bool operator ==(XyztCoordinate a, XyztCoordinate b)
        {
            if (a.X == b.X && a.Y == b.Y && a.Z == b.Z && a.T == b.T)
                return true;
            else
                return false;
        }

        public static bool operator !=(XyztCoordinate a, XyztCoordinate b)
        {
            if (a.X == b.X && a.Y == b.Y && a.Z == b.Z && a.T == b.T)
                return false;
            else
                return true;
        }

        public static XyztCoordinate operator +(XyztCoordinate a, XyztCoordinate b)
        {
            return new XyztCoordinate(a.X + b.X, a.Y + b.Y, a.Z + b.Z, a.T + b.T);
        }

        public static XyztCoordinate operator +(XyztCoordinate a, XyCoordinate b)
        {
            return new XyztCoordinate(a.X + b.X, a.Y + b.Y, a.Z, a.T);
        }

        public static XyztCoordinate operator +(XyCoordinate a, XyztCoordinate b)
        {
            return new XyztCoordinate(a.X + b.X, a.Y + b.Y, b.Z, b.T);
        }

        public static XyztCoordinate operator +(XyztCoordinate a, XytCoordinate b)
        {
            return new XyztCoordinate(a.X + b.X, a.Y + b.Y, a.Z, a.T + b.T);
        }

        public static XyztCoordinate operator +(XytCoordinate a, XyztCoordinate b)
        {
            return new XyztCoordinate(a.X + b.X, a.Y + b.Y, b.Z, a.T + b.T);
        }

        public static XyztCoordinate operator +(XyztCoordinate a, XyzCoordinate b)
        {
            return new XyztCoordinate(a.X + b.X, a.Y + b.Y, a.Z + b.Z, a.T);
        }

        public static XyztCoordinate operator +(XyzCoordinate a, XyztCoordinate b)
        {
            return new XyztCoordinate(a.X + b.X, a.Y + b.Y, a.Z + b.Z, b.T);
        }

        [Obsolete("삭제 예정입니다. 무슨 목적으로 만들었는지 공유해주세요.")]
        public static XyztCoordinate operator +(XyztCoordinate a, int b)
        {
            return new XyztCoordinate(a.X + b, a.Y + b, a.Z + b, a.T + b);
        }

        public static XyztCoordinate operator +(int a, XyztCoordinate b)
        {
            return new XyztCoordinate(a + b.X, a + b.Y, a + b.Z, a + b.T);
        }

        [Obsolete("삭제 예정입니다. 무슨 목적으로 만들었는지 공유해주세요.")]
        public static XyztCoordinate operator +(XyztCoordinate a, double b)
        {
            return new XyztCoordinate(a.X + b, a.Y + b, a.Z + b, a.T + b);
        }

        public static XyztCoordinate operator +(double a, XyztCoordinate b)
        {
            return new XyztCoordinate(a + b.X, a + b.Y, a + b.Z, a + b.T);
        }

        public static XyztCoordinate operator +(XyztCoordinate a, SizeD b)
        {
            return new XyztCoordinate(a.X + b.Width, a.Y + b.Height, a.Z, a.T);
        }

        public static XyztCoordinate operator +(SizeD a, XyztCoordinate b)
        {
            return new XyztCoordinate(a.Width + b.X, a.Height + b.Y, b.Z, b.T);
        }

        public static XyztCoordinate operator +(XyztCoordinate a, PointD b)
        {
            return new XyztCoordinate(a.X + b.X, a.Y + b.Y, a.Z, a.T);
        }

        public static XyztCoordinate operator +(PointD a, XyztCoordinate b)
        {
            return new XyztCoordinate(a.X + b.X, a.Y + b.Y, b.Z, b.T);
        }

        public static XyztCoordinate operator -(XyztCoordinate a, XyztCoordinate b)
        {
            return new XyztCoordinate(a.X - b.X, a.Y - b.Y, a.Z - b.Z, a.T - b.T);
        }

        public static XyztCoordinate operator -(XyztCoordinate a, XyCoordinate b)
        {
            return new XyztCoordinate(a.X - b.X, a.Y - b.Y, a.Z, a.T);
        }

        public static XyztCoordinate operator -(XyCoordinate a, XyztCoordinate b)
        {
            return new XyztCoordinate(a.X - b.X, a.Y - b.Y, b.Z, b.T);
        }

        public static XyztCoordinate operator -(XyztCoordinate a, XytCoordinate b)
        {
            return new XyztCoordinate(a.X - b.X, a.Y - b.Y, a.Z, a.T - b.T);
        }

        public static XyztCoordinate operator -(XytCoordinate a, XyztCoordinate b)
        {
            return new XyztCoordinate(a.X - b.X, a.Y - b.Y, b.Z, a.T - b.T);
        }

        public static XyztCoordinate operator -(XyztCoordinate a, XyzCoordinate b)
        {
            return new XyztCoordinate(a.X - b.X, a.Y - b.Y, a.Z - b.Z, a.T);
        }

        public static XyztCoordinate operator -(XyzCoordinate a, XyztCoordinate b)
        {
            return new XyztCoordinate(a.X - b.X, a.Y - b.Y, a.Z - b.Z, b.T);
        }

        [Obsolete("삭제 예정입니다. 무슨 목적으로 만들었는지 공유해주세요.")]
        public static XyztCoordinate operator -(XyztCoordinate a, int b)
        {
            return new XyztCoordinate(a.X - b, a.Y - b, a.Z - b, a.T - b);
        }

        [Obsolete("삭제 예정입니다. 무슨 목적으로 만들었는지 공유해주세요.")]
        public static XyztCoordinate operator -(int a, XyztCoordinate b)
        {
            return new XyztCoordinate(a - b.X, a - b.Y, a - b.Z, a - b.T);
        }

        [Obsolete("삭제 예정입니다. 무슨 목적으로 만들었는지 공유해주세요.")]
        public static XyztCoordinate operator -(XyztCoordinate a, double b)
        {
            return new XyztCoordinate(a.X - b, a.Y - b, a.Z - b, a.T - b);
        }

        [Obsolete("삭제 예정입니다. 무슨 목적으로 만들었는지 공유해주세요.")]
        public static XyztCoordinate operator -(double a, XyztCoordinate b)
        {
            return new XyztCoordinate(a - b.X, a - b.Y, a - b.Z, a - b.T);
        }

        public static XyztCoordinate operator -(XyztCoordinate a, SizeD b)
        {
            return new XyztCoordinate(a.X - b.Width, a.Y - b.Height, a.Z, a.T);
        }

        public static XyztCoordinate operator -(SizeD a, XyztCoordinate b)
        {
            return new XyztCoordinate(a.Width - b.X, a.Height - b.Y, b.Z, b.T);
        }

        public static XyztCoordinate operator -(XyztCoordinate a, PointD b)
        {
            return new XyztCoordinate(a.X - b.X, a.Y - b.Y, a.Z, a.T);
        }

        public static XyztCoordinate operator -(PointD a, XyztCoordinate b)
        {
            return new XyztCoordinate(a.X - b.X, a.Y - b.Y, b.Z, b.T);
        }

        public static XyztCoordinate operator *(XyztCoordinate a, XyztCoordinate b)
        {
            return new XyztCoordinate(a.X * b.X, a.Y * b.Y, a.Z * b.Z, a.T * b.T);
        }

        public static XyztCoordinate operator *(XyztCoordinate a, XyCoordinate b)
        {
            return new XyztCoordinate(a.X * b.X, a.Y * b.Y, a.Z, a.T);
        }

        public static XyztCoordinate operator *(XyCoordinate a, XyztCoordinate b)
        {
            return new XyztCoordinate(a.X * b.X, a.Y * b.Y, b.Z, b.T);
        }

        public static XyztCoordinate operator *(XyztCoordinate a, XytCoordinate b)
        {
            return new XyztCoordinate(a.X * b.X, a.Y * b.Y, a.Z, a.T * b.T);
        }

        public static XyztCoordinate operator *(XytCoordinate a, XyztCoordinate b)
        {
            return new XyztCoordinate(a.X * b.X, a.Y * b.Y, b.Z, a.T * b.T);
        }

        public static XyztCoordinate operator *(XyztCoordinate a, XyzCoordinate b)
        {
            return new XyztCoordinate(a.X * b.X, a.Y * b.Y, a.Z * b.Z, a.T);
        }

        public static XyztCoordinate operator *(XyzCoordinate a, XyztCoordinate b)
        {
            return new XyztCoordinate(a.X * b.X, a.Y * b.Y, a.Z * b.Z, b.T);
        }

        [Obsolete("삭제 예정입니다. 무슨 목적으로 만들었는지 공유해주세요.")]
        public static XyztCoordinate operator *(XyztCoordinate a, int b)
        {
            return new XyztCoordinate(a.X * b, a.Y * b, a.Z * b, a.T * b);
        }

        public static XyztCoordinate operator *(int a, XyztCoordinate b)
        {
            return new XyztCoordinate(a * b.X, a * b.Y, a * b.Z, a * b.T);
        }

        [Obsolete("삭제 예정입니다. 무슨 목적으로 만들었는지 공유해주세요.")]
        public static XyztCoordinate operator *(XyztCoordinate a, double b)
        {
            return new XyztCoordinate(a.X * b, a.Y * b, a.Z * b, a.T * b);
        }

        [Obsolete("삭제 예정입니다. 무슨 목적으로 만들었는지 공유해주세요.")]
        public static XyztCoordinate operator *(double a, XyztCoordinate b)
        {
            return new XyztCoordinate(a * b.X, a * b.Y, a * b.Z, a * b.T);
        }

        public static XyztCoordinate operator *(XyztCoordinate a, SizeD b)
        {
            return new XyztCoordinate(a.X * b.Width, a.Y * b.Height, a.Z, a.T);
        }

        public static XyztCoordinate operator *(SizeD a, XyztCoordinate b)
        {
            return new XyztCoordinate(a.Width * b.X, a.Height * b.Y, b.Z, b.T);
        }

        public static XyztCoordinate operator *(XyztCoordinate a, PointD b)
        {
            return new XyztCoordinate(a.X * b.X, a.Y * b.Y, a.Z, a.T);
        }

        public static XyztCoordinate operator *(PointD a, XyztCoordinate b)
        {
            return new XyztCoordinate(a.X * b.X, a.Y * b.Y, b.Z, b.T);
        }

        public static XyztCoordinate operator /(XyztCoordinate a, XyztCoordinate b)
        {
            return new XyztCoordinate(a.X / b.X, a.Y / b.Y, a.Z / b.Z, a.T / b.T);
        }

        public static XyztCoordinate operator /(XyztCoordinate a, XyCoordinate b)
        {
            return new XyztCoordinate(a.X / b.X, a.Y / b.Y, a.Z, a.T);
        }

        public static XyztCoordinate operator /(XyCoordinate a, XyztCoordinate b)
        {
            return new XyztCoordinate(a.X / b.X, a.Y / b.Y, b.Z, b.T);
        }

        public static XyztCoordinate operator /(XyztCoordinate a, XytCoordinate b)
        {
            return new XyztCoordinate(a.X / b.X, a.Y / b.Y, a.Z, a.T / b.T);
        }

        public static XyztCoordinate operator /(XytCoordinate a, XyztCoordinate b)
        {
            return new XyztCoordinate(a.X / b.X, a.Y / b.Y, b.Z, a.T / b.T);
        }

        public static XyztCoordinate operator /(XyztCoordinate a, XyzCoordinate b)
        {
            return new XyztCoordinate(a.X / b.X, a.Y / b.Y, a.Z / b.Z, a.T);
        }

        public static XyztCoordinate operator /(XyzCoordinate a, XyztCoordinate b)
        {
            return new XyztCoordinate(a.X / b.X, a.Y / b.Y, a.Z / b.Z, b.T);
        }

        [Obsolete("삭제 예정입니다. 무슨 목적으로 만들었는지 공유해주세요.")]
        public static XyztCoordinate operator /(XyztCoordinate a, int b)
        {
            return new XyztCoordinate(a.X / b, a.Y / b, a.Z / b, a.T / b);
        }

        [Obsolete("삭제 예정입니다. 무슨 목적으로 만들었는지 공유해주세요.")]
        public static XyztCoordinate operator /(int a, XyztCoordinate b)
        {
            return new XyztCoordinate(a / b.X, a / b.Y, a / b.Z, a / b.T);
        }

        [Obsolete("삭제 예정입니다. 무슨 목적으로 만들었는지 공유해주세요.")]
        public static XyztCoordinate operator /(XyztCoordinate a, double b)
        {
            return new XyztCoordinate(a.X / b, a.Y / b, a.Z / b, a.T / b);
        }

        [Obsolete("삭제 예정입니다. 무슨 목적으로 만들었는지 공유해주세요.")]
        public static XyztCoordinate operator /(double a, XyztCoordinate b)
        {
            return new XyztCoordinate(a / b.X, a / b.Y, a / b.Z, a / b.T);
        }

        public static XyztCoordinate operator /(XyztCoordinate a, SizeD b)
        {
            return new XyztCoordinate(a.X / b.Width, a.Y / b.Height, a.Z, a.T);
        }

        public static XyztCoordinate operator /(SizeD a, XyztCoordinate b)
        {
            return new XyztCoordinate(a.Width / b.X, a.Height / b.Y, b.Z, b.T);
        }

        public static XyztCoordinate operator /(XyztCoordinate a, PointD b)
        {
            return new XyztCoordinate(a.X / b.X, a.Y / b.Y, a.Z, a.T);
        }

        public static XyztCoordinate operator /(PointD a, XyztCoordinate b)
        {
            return new XyztCoordinate(a.X / b.X, a.Y / b.Y, b.Z, b.T);
        }

        public static explicit operator XyztCoordinate(XyCoordinate a)
        {
            return new XyztCoordinate(a.X, a.Y, 0.0, 0.0);
        }

        public static explicit operator XyztCoordinate(XyzCoordinate a)
        {
            return new XyztCoordinate(a.X, a.Y, a.Z, 0.0);
        }

        public static explicit operator XyztCoordinate(XytCoordinate a)
        {
            return new XyztCoordinate(a.X, a.Y, 0.0, a.T);
        }

        public static explicit operator XyztCoordinate(PointD a)
        {
            return new XyztCoordinate(a.X, a.Y, 0.0, 0.0);
        }
        #endregion

        #region Object Members
        public override bool Equals(object obj)
        {
            if (obj == null) return false;

            if (obj is XyztCoordinate == false) return false;
            XyztCoordinate value = (XyztCoordinate)obj;

            return this.Equals(value);
        }

        public override int GetHashCode()
        {
            return this.X.GetHashCode() ^ this.Y.GetHashCode() ^ this.Z.GetHashCode() ^ this.T.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("[{0}, {1}, {2}, {3}]", this.X, this.Y, this.Z, this.T);
        }
        #endregion
    }
    #endregion

    #region XyztCoordinateCollection
    [Serializable]
    public class XyztCoordinateCollection : Collection<XyztCoordinate>
    {
        #region Constructor
        public XyztCoordinateCollection(IList<XyztCoordinate> list) : base(list) { }
        public XyztCoordinateCollection() : base() { }
        #endregion
    }
    #endregion
}
