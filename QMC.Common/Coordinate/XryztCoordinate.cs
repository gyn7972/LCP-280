using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QMC.Common
{
    #region XryztCoordinate
    [Serializable]
    public struct XryztCoordinate : IXyCoordinate
    {
        #region Field
        private double m_X;
        private double m_R;
        private double m_Y;
        private double m_Z;
        private double m_T;
        #endregion

        #region Constructor
        public XryztCoordinate(double x, double r, double y, double z, double t)
        {
            this.m_X = x;
            this.m_R = r;
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

        public double R
        {
            get { return this.m_R; }
            set { this.m_R = value; }
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
        public bool Equals(XryztCoordinate value)
        {
            return this == value;
        }
        #endregion

        #region Static Members
        public static bool operator ==(XryztCoordinate a, XryztCoordinate b)
        {
            if (a.X == b.X && a.R == b.R && a.Y == b.Y && a.Z == b.Z && a.T == b.T)
                return true;
            else
                return false;
        }

        public static bool operator !=(XryztCoordinate a, XryztCoordinate b)
        {
            if (a.X == b.X && a.R == b.R && a.Y == b.Y && a.Z == b.Z && a.T == b.T)
                return false;
            else
                return true;
        }

        public static XryztCoordinate operator +(XryztCoordinate a, XryztCoordinate b)
        {
            return new XryztCoordinate(a.X + b.X, a.R + b.R, a.Y + b.Y, a.Z + b.Z, a.T + b.T);
        }

        public static XryztCoordinate operator +(XryztCoordinate a, XyCoordinate b)
        {
            return new XryztCoordinate(a.X + b.X, a.R, a.Y + b.Y, a.Z, a.T);
        }

        public static XryztCoordinate operator +(XyCoordinate a, XryztCoordinate b)
        {
            return new XryztCoordinate(a.X + b.X, b.R, a.Y + b.Y, b.Z, b.T);
        }

        public static XryztCoordinate operator +(XryztCoordinate a, XytCoordinate b)
        {
            return new XryztCoordinate(a.X + b.X, a.R, a.Y + b.Y, a.Z, a.T + b.T);
        }

        public static XryztCoordinate operator +(XytCoordinate a, XryztCoordinate b)
        {
            return new XryztCoordinate(a.X + b.X, b.R, a.Y + b.Y, b.Z, a.T + b.T);
        }

        public static XryztCoordinate operator +(XryztCoordinate a, XyzCoordinate b)
        {
            return new XryztCoordinate(a.X + b.X, a.R, a.Y + b.Y, a.Z + b.Z, a.T);
        }

        public static XryztCoordinate operator +(XyzCoordinate a, XryztCoordinate b)
        {
            return new XryztCoordinate(a.X + b.X, b.R, a.Y + b.Y, a.Z + b.Z, b.T);
        }

        [Obsolete("삭제 예정입니다. 무슨 목적으로 만들었는지 공유해주세요.")]
        public static XryztCoordinate operator +(XryztCoordinate a, int b)
        {
            return new XryztCoordinate(a.X + b, a.R + b, a.Y + b, a.Z + b, a.T + b);
        }

        public static XryztCoordinate operator +(int a, XryztCoordinate b)
        {
            return new XryztCoordinate(a + b.X, a + b.R, a + b.Y, a + b.Z, a + b.T);
        }

        [Obsolete("삭제 예정입니다. 무슨 목적으로 만들었는지 공유해주세요.")]
        public static XryztCoordinate operator +(XryztCoordinate a, double b)
        {
            return new XryztCoordinate(a.X + b, a.R + b, a.Y + b, a.Z + b, a.T + b);
        }

        public static XryztCoordinate operator +(double a, XryztCoordinate b)
        {
            return new XryztCoordinate(a + b.X, a + b.R, a + b.Y, a + b.Z, a + b.T);
        }

        public static XryztCoordinate operator +(XryztCoordinate a, SizeD b)
        {
            return new XryztCoordinate(a.X + b.Width, a.R, a.Y + b.Height, a.Z, a.T);
        }

        public static XryztCoordinate operator +(SizeD a, XryztCoordinate b)
        {
            return new XryztCoordinate(a.Width + b.X, b.R, a.Height + b.Y, b.Z, b.T);
        }

        public static XryztCoordinate operator +(XryztCoordinate a, PointD b)
        {
            return new XryztCoordinate(a.X + b.X, a.R, a.Y + b.Y, a.Z, a.T);
        }

        public static XryztCoordinate operator +(PointD a, XryztCoordinate b)
        {
            return new XryztCoordinate(a.X + b.X, b.R, a.Y + b.Y, b.Z, b.T);
        }

        public static XryztCoordinate operator -(XryztCoordinate a, XryztCoordinate b)
        {
            return new XryztCoordinate(a.X - b.X, a.R - b.R, a.Y - b.Y, a.Z - b.Z, a.T - b.T);
        }

        public static XryztCoordinate operator -(XryztCoordinate a, XyCoordinate b)
        {
            return new XryztCoordinate(a.X - b.X, a.R, a.Y - b.Y, a.Z, a.T);
        }

        public static XryztCoordinate operator -(XyCoordinate a, XryztCoordinate b)
        {
            return new XryztCoordinate(a.X - b.X, b.R, a.Y - b.Y, b.Z, b.T);
        }

        public static XryztCoordinate operator -(XryztCoordinate a, XytCoordinate b)
        {
            return new XryztCoordinate(a.X - b.X, a.R, a.Y - b.Y, a.Z, a.T - b.T);
        }

        public static XryztCoordinate operator -(XytCoordinate a, XryztCoordinate b)
        {
            return new XryztCoordinate(a.X - b.X, b.R, a.Y - b.Y, b.Z, a.T - b.T);
        }

        public static XryztCoordinate operator -(XryztCoordinate a, XyzCoordinate b)
        {
            return new XryztCoordinate(a.X - b.X, a.R, a.Y - b.Y, a.Z - b.Z, a.T);
        }

        public static XryztCoordinate operator -(XyzCoordinate a, XryztCoordinate b)
        {
            return new XryztCoordinate(a.X - b.X, b.R, a.Y - b.Y, a.Z - b.Z, b.T);
        }

        [Obsolete("삭제 예정입니다. 무슨 목적으로 만들었는지 공유해주세요.")]
        public static XryztCoordinate operator -(XryztCoordinate a, int b)
        {
            return new XryztCoordinate(a.X - b, a.R - b, a.Y - b, a.Z - b, a.T - b);
        }

        [Obsolete("삭제 예정입니다. 무슨 목적으로 만들었는지 공유해주세요.")]
        public static XryztCoordinate operator -(int a, XryztCoordinate b)
        {
            return new XryztCoordinate(a - b.X, a - b.R, a - b.Y, a - b.Z, a - b.T);
        }

        [Obsolete("삭제 예정입니다. 무슨 목적으로 만들었는지 공유해주세요.")]
        public static XryztCoordinate operator -(XryztCoordinate a, double b)
        {
            return new XryztCoordinate(a.X - b, a.R - b, a.Y - b, a.Z - b, a.T - b);
        }

        [Obsolete("삭제 예정입니다. 무슨 목적으로 만들었는지 공유해주세요.")]
        public static XryztCoordinate operator -(double a, XryztCoordinate b)
        {
            return new XryztCoordinate(a - b.X, a - b.R, a - b.Y, a - b.Z, a - b.T);
        }

        public static XryztCoordinate operator -(XryztCoordinate a, SizeD b)
        {
            return new XryztCoordinate(a.X - b.Width, a.R, a.Y - b.Height, a.Z, a.T);
        }

        public static XryztCoordinate operator -(SizeD a, XryztCoordinate b)
        {
            return new XryztCoordinate(a.Width - b.X, b.R, a.Height - b.Y, b.Z, b.T);
        }

        public static XryztCoordinate operator -(XryztCoordinate a, PointD b)
        {
            return new XryztCoordinate(a.X - b.X, a.R, a.Y - b.Y, a.Z, a.T);
        }

        public static XryztCoordinate operator -(PointD a, XryztCoordinate b)
        {
            return new XryztCoordinate(a.X - b.X, b.R, a.Y - b.Y, b.Z, b.T);
        }

        public static XryztCoordinate operator *(XryztCoordinate a, XryztCoordinate b)
        {
            return new XryztCoordinate(a.X * b.X, a.R * b.R, a.Y * b.Y, a.Z * b.Z, a.T * b.T);
        }

        public static XryztCoordinate operator *(XryztCoordinate a, XyCoordinate b)
        {
            return new XryztCoordinate(a.X * b.X, a.R, a.Y * b.Y, a.Z, a.T);
        }

        public static XryztCoordinate operator *(XyCoordinate a, XryztCoordinate b)
        {
            return new XryztCoordinate(a.X * b.X, b.R, a.Y * b.Y, b.Z, b.T);
        }

        public static XryztCoordinate operator *(XryztCoordinate a, XytCoordinate b)
        {
            return new XryztCoordinate(a.X * b.X, a.R, a.Y * b.Y, a.Z, a.T * b.T);
        }

        public static XryztCoordinate operator *(XytCoordinate a, XryztCoordinate b)
        {
            return new XryztCoordinate(a.X * b.X, b.R, a.Y * b.Y, b.Z, a.T * b.T);
        }

        public static XryztCoordinate operator *(XryztCoordinate a, XyzCoordinate b)
        {
            return new XryztCoordinate(a.X * b.X, a.R, a.Y * b.Y, a.Z * b.Z, a.T);
        }

        public static XryztCoordinate operator *(XyzCoordinate a, XryztCoordinate b)
        {
            return new XryztCoordinate(a.X * b.X, b.R, a.Y * b.Y, a.Z * b.Z, b.T);
        }

        [Obsolete("삭제 예정입니다. 무슨 목적으로 만들었는지 공유해주세요.")]
        public static XryztCoordinate operator *(XryztCoordinate a, int b)
        {
            return new XryztCoordinate(a.X * b, a.R * b, a.Y * b, a.Z * b, a.T * b);
        }

        public static XryztCoordinate operator *(int a, XryztCoordinate b)
        {
            return new XryztCoordinate(a * b.X, a * b.R, a * b.Y, a * b.Z, a * b.T);
        }

        [Obsolete("삭제 예정입니다. 무슨 목적으로 만들었는지 공유해주세요.")]
        public static XryztCoordinate operator *(XryztCoordinate a, double b)
        {
            return new XryztCoordinate(a.X * b, a.R * b, a.Y * b, a.Z * b, a.T * b);
        }

        [Obsolete("삭제 예정입니다. 무슨 목적으로 만들었는지 공유해주세요.")]
        public static XryztCoordinate operator *(double a, XryztCoordinate b)
        {
            return new XryztCoordinate(a * b.X, a * b.R, a * b.Y, a * b.Z, a * b.T);
        }

        public static XryztCoordinate operator *(XryztCoordinate a, SizeD b)
        {
            return new XryztCoordinate(a.X * b.Width, a.R, a.Y * b.Height, a.Z, a.T);
        }

        public static XryztCoordinate operator *(SizeD a, XryztCoordinate b)
        {
            return new XryztCoordinate(a.Width * b.X, b.R, a.Height * b.Y, b.Z, b.T);
        }

        public static XryztCoordinate operator *(XryztCoordinate a, PointD b)
        {
            return new XryztCoordinate(a.X * b.X, a.R, a.Y * b.Y, a.Z, a.T);
        }

        public static XryztCoordinate operator *(PointD a, XryztCoordinate b)
        {
            return new XryztCoordinate(a.X * b.X, b.R, a.Y * b.Y, b.Z, b.T);
        }

        public static XryztCoordinate operator /(XryztCoordinate a, XryztCoordinate b)
        {
            return new XryztCoordinate(a.X / b.X, a.R / b.R, a.Y / b.Y, a.Z / b.Z, a.T / b.T);
        }

        public static XryztCoordinate operator /(XryztCoordinate a, XyCoordinate b)
        {
            return new XryztCoordinate(a.X / b.X, a.R, a.Y / b.Y, a.Z, a.T);
        }

        public static XryztCoordinate operator /(XyCoordinate a, XryztCoordinate b)
        {
            return new XryztCoordinate(a.X / b.X, b.R, a.Y / b.Y, b.Z, b.T);
        }

        public static XryztCoordinate operator /(XryztCoordinate a, XytCoordinate b)
        {
            return new XryztCoordinate(a.X / b.X, a.R, a.Y / b.Y, a.Z, a.T / b.T);
        }

        public static XryztCoordinate operator /(XytCoordinate a, XryztCoordinate b)
        {
            return new XryztCoordinate(a.X / b.X, b.R, a.Y / b.Y, b.Z, a.T / b.T);
        }

        public static XryztCoordinate operator /(XryztCoordinate a, XyzCoordinate b)
        {
            return new XryztCoordinate(a.X / b.X, a.R, a.Y / b.Y, a.Z / b.Z, a.T);
        }

        public static XryztCoordinate operator /(XyzCoordinate a, XryztCoordinate b)
        {
            return new XryztCoordinate(a.X / b.X, b.R, a.Y / b.Y, a.Z / b.Z, b.T);
        }

        [Obsolete("삭제 예정입니다. 무슨 목적으로 만들었는지 공유해주세요.")]
        public static XryztCoordinate operator /(XryztCoordinate a, int b)
        {
            return new XryztCoordinate(a.X / b, a.R / b, a.Y / b, a.Z / b, a.T / b);
        }

        [Obsolete("삭제 예정입니다. 무슨 목적으로 만들었는지 공유해주세요.")]
        public static XryztCoordinate operator /(int a, XryztCoordinate b)
        {
            return new XryztCoordinate(a / b.X, a / b.R, a / b.Y, a / b.Z, a / b.T);
        }

        [Obsolete("삭제 예정입니다. 무슨 목적으로 만들었는지 공유해주세요.")]
        public static XryztCoordinate operator /(XryztCoordinate a, double b)
        {
            return new XryztCoordinate(a.X / b, a.R / b, a.Y / b, a.Z / b, a.T / b);
        }

        [Obsolete("삭제 예정입니다. 무슨 목적으로 만들었는지 공유해주세요.")]
        public static XryztCoordinate operator /(double a, XryztCoordinate b)
        {
            return new XryztCoordinate(a / b.X, a / b.R, a / b.Y, a / b.Z, a / b.T);
        }

        public static XryztCoordinate operator /(XryztCoordinate a, SizeD b)
        {
            return new XryztCoordinate(a.X / b.Width, a.R, a.Y / b.Height, a.Z, a.T);
        }

        public static XryztCoordinate operator /(SizeD a, XryztCoordinate b)
        {
            return new XryztCoordinate(a.Width / b.X, b.R, a.Height / b.Y, b.Z, b.T);
        }

        public static XryztCoordinate operator /(XryztCoordinate a, PointD b)
        {
            return new XryztCoordinate(a.X / b.X, a.R, a.Y / b.Y, a.Z, a.T);
        }

        public static XryztCoordinate operator /(PointD a, XryztCoordinate b)
        {
            return new XryztCoordinate(a.X / b.X, b.R, a.Y / b.Y, b.Z, b.T);
        }

        public static explicit operator XryztCoordinate(XyCoordinate a)
        {
            return new XryztCoordinate(a.X, 0.0, a.Y, 0.0, 0.0);
        }

        public static explicit operator XryztCoordinate(XyzCoordinate a)
        {
            return new XryztCoordinate(a.X, 0.0, a.Y, a.Z, 0.0);
        }

        public static explicit operator XryztCoordinate(XytCoordinate a)
        {
            return new XryztCoordinate(a.X, 0.0, a.Y, 0.0, a.T);
        }

        public static explicit operator XryztCoordinate(PointD a)
        {
            return new XryztCoordinate(a.X, 0.0, a.Y, 0.0, 0.0);
        }
        #endregion

        #region Object Members
        public override bool Equals(object obj)
        {
            if (obj == null) return false;

            if (obj is XryztCoordinate == false) return false;
            XryztCoordinate value = (XryztCoordinate)obj;

            return this.Equals(value);
        }

        public override int GetHashCode()
        {
            return this.X.GetHashCode() ^ this.R.GetHashCode() ^ this.Y.GetHashCode() ^ this.Z.GetHashCode() ^ this.T.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("[{0}, {1}, {2}, {3}, {4}]", this.X, this.R, this.Y, this.Z, this.T);
        }
        #endregion
    }
    #endregion

    #region XryztCoordinateCollection
    [Serializable]
    public class XryztCoordinateCollection : Collection<XryztCoordinate>
    {
        #region Constructor
        public XryztCoordinateCollection(IList<XryztCoordinate> list) : base(list) { }
        public XryztCoordinateCollection() : base() { }
        #endregion
    }
    #endregion
}
