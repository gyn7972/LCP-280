using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QMC.Common
{
    #region XyzztCoordinate
    [Serializable]
    public struct XyzztCoordinate : IXyCoordinate
    {
        #region Field
        private double m_X;
        private double m_Y;
        private double m_IZ;            //  Inspection Z
        private double m_SZ;            //  Scanner Z
        private double m_T;
        #endregion

        #region Constructor
        public XyzztCoordinate(double x, double y, double z1, double z2, double t)
        {
            this.m_X = x;
            this.m_Y = y;
            this.m_IZ = z1;
            this.m_SZ = z2;
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

        public double IZ
        {
            get { return this.m_IZ; }
            set { this.m_IZ = value; }
        }

        public double SZ
        {
            get { return this.m_SZ; }
            set { this.m_SZ = value; }
        }

        public double T
        {
            get { return this.m_T; }
            set { this.m_T = value; }
        }
        #endregion

        #region Method
        public bool Equals(XyzztCoordinate value)
        {
            return this == value;
        }
        #endregion

        #region Static Members
        public static bool operator ==(XyzztCoordinate a, XyzztCoordinate b)
        {
            if (a.X == b.X && a.Y == b.Y && a.IZ == b.IZ && a.SZ == b.SZ && a.T == b.T)
                return true;
            else
                return false;
        }

        public static bool operator !=(XyzztCoordinate a, XyzztCoordinate b)
        {
            if (a.X == b.X && a.Y == b.Y && a.IZ == b.IZ && a.SZ == b.SZ && a.T == b.T)
                return false;
            else
                return true;
        }

        public static XyzztCoordinate operator +(XyzztCoordinate a, XyzztCoordinate b)
        {
            return new XyzztCoordinate(a.X + b.X, a.Y + b.Y, a.IZ + b.IZ, a.SZ + b.SZ, a.T + b.T);
        }

        public static XyzztCoordinate operator +(XyzztCoordinate a, XyCoordinate b)
        {
            return new XyzztCoordinate(a.X + b.X, a.Y + b.Y, a.IZ, a.SZ, a.T);
        }

        public static XyzztCoordinate operator +(XyCoordinate a, XyzztCoordinate b)
        {
            return new XyzztCoordinate(a.X + b.X, a.Y + b.Y, b.IZ, b.SZ, b.T);
        }

        public static XyzztCoordinate operator +(XyzztCoordinate a, XytCoordinate b)
        {
            return new XyzztCoordinate(a.X + b.X, a.Y + b.Y, a.IZ, a.SZ, a.T + b.T);
        }

        public static XyzztCoordinate operator +(XytCoordinate a, XyzztCoordinate b)
        {
            return new XyzztCoordinate(a.X + b.X, a.Y + b.Y, b.IZ, b.SZ, a.T + b.T);
        }

        public static XyzztCoordinate operator +(XyzztCoordinate a, XyzCoordinate b)
        {
            return new XyzztCoordinate(a.X + b.X, a.Y + b.Y, a.IZ + b.Z, a.SZ + b.Z, a.T);
        }

        public static XyzztCoordinate operator +(XyzCoordinate a, XyzztCoordinate b)
        {
            return new XyzztCoordinate(a.X + b.X, a.Y + b.Y, a.Z + b.IZ, a.Z + b.SZ, b.T);
        }

        [Obsolete("삭제 예정입니다. 무슨 목적으로 만들었는지 공유해주세요.")]
        public static XyzztCoordinate operator +(XyzztCoordinate a, int b)
        {
            return new XyzztCoordinate(a.X + b, a.Y + b, a.IZ + b, a.SZ + b, a.T + b);
        }

        public static XyzztCoordinate operator +(int a, XyzztCoordinate b)
        {
            return new XyzztCoordinate(a + b.X, a + b.Y, a + b.IZ, a + b.SZ, a + b.T);
        }

        [Obsolete("삭제 예정입니다. 무슨 목적으로 만들었는지 공유해주세요.")]
        public static XyzztCoordinate operator +(XyzztCoordinate a, double b)
        {
            return new XyzztCoordinate(a.X + b, a.Y + b, a.IZ + b, a.SZ + b, a.T + b);
        }

        public static XyzztCoordinate operator +(double a, XyzztCoordinate b)
        {
            return new XyzztCoordinate(a + b.X, a + b.Y, a + b.IZ, a + b.SZ, a + b.T);
        }

        public static XyzztCoordinate operator +(XyzztCoordinate a, SizeD b)
        {
            return new XyzztCoordinate(a.X + b.Width, a.Y + b.Height, a.IZ, a.SZ, a.T);
        }

        public static XyzztCoordinate operator +(SizeD a, XyzztCoordinate b)
        {
            return new XyzztCoordinate(a.Width + b.X, a.Height + b.Y, b.IZ, b.SZ, b.T);
        }

        public static XyzztCoordinate operator +(XyzztCoordinate a, PointD b)
        {
            return new XyzztCoordinate(a.X + b.X, a.Y + b.Y, a.IZ, a.SZ, a.T);
        }

        public static XyzztCoordinate operator +(PointD a, XyzztCoordinate b)
        {
            return new XyzztCoordinate(a.X + b.X, a.Y + b.Y, b.IZ, b.SZ, b.T);
        }

        public static XyzztCoordinate operator -(XyzztCoordinate a, XyzztCoordinate b)
        {
            return new XyzztCoordinate(a.X - b.X, a.Y - b.Y, a.IZ - b.IZ, a.SZ - b.SZ, a.T - b.T);
        }

        public static XyzztCoordinate operator -(XyzztCoordinate a, XyCoordinate b)
        {
            return new XyzztCoordinate(a.X - b.X, a.Y - b.Y, a.IZ, a.SZ, a.T);
        }

        public static XyzztCoordinate operator -(XyCoordinate a, XyzztCoordinate b)
        {
            return new XyzztCoordinate(a.X - b.X, a.Y - b.Y, b.IZ, b.SZ, b.T);
        }

        public static XyzztCoordinate operator -(XyzztCoordinate a, XytCoordinate b)
        {
            return new XyzztCoordinate(a.X - b.X, a.Y - b.Y, a.IZ, a.SZ, a.T - b.T);
        }

        public static XyzztCoordinate operator -(XytCoordinate a, XyzztCoordinate b)
        {
            return new XyzztCoordinate(a.X - b.X, a.Y - b.Y, b.IZ, b.SZ, a.T - b.T);
        }

        public static XyzztCoordinate operator -(XyzztCoordinate a, XyzCoordinate b)
        {
            return new XyzztCoordinate(a.X - b.X, a.Y - b.Y, a.IZ - b.Z, a.SZ - b.Z, a.T);
        }

        public static XyzztCoordinate operator -(XyzCoordinate a, XyzztCoordinate b)
        {
            return new XyzztCoordinate(a.X - b.X, a.Y - b.Y, a.Z - b.IZ, a.Z - b.SZ, b.T);
        }

        [Obsolete("삭제 예정입니다. 무슨 목적으로 만들었는지 공유해주세요.")]
        public static XyzztCoordinate operator -(XyzztCoordinate a, int b)
        {
            return new XyzztCoordinate(a.X - b, a.Y - b, a.IZ - b, a.SZ - b, a.T - b);
        }

        [Obsolete("삭제 예정입니다. 무슨 목적으로 만들었는지 공유해주세요.")]
        public static XyzztCoordinate operator -(int a, XyzztCoordinate b)
        {
            return new XyzztCoordinate(a - b.X, a - b.Y, a - b.IZ, a - b.SZ, a - b.T);
        }

        [Obsolete("삭제 예정입니다. 무슨 목적으로 만들었는지 공유해주세요.")]
        public static XyzztCoordinate operator -(XyzztCoordinate a, double b)
        {
            return new XyzztCoordinate(a.X - b, a.Y - b, a.IZ - b, a.SZ - b, a.T - b);
        }

        [Obsolete("삭제 예정입니다. 무슨 목적으로 만들었는지 공유해주세요.")]
        public static XyzztCoordinate operator -(double a, XyzztCoordinate b)
        {
            return new XyzztCoordinate(a - b.X, a - b.Y, a - b.IZ, a - b.SZ, a - b.T);
        }

        public static XyzztCoordinate operator -(XyzztCoordinate a, SizeD b)
        {
            return new XyzztCoordinate(a.X - b.Width, a.Y - b.Height, a.IZ, a.SZ, a.T);
        }

        public static XyzztCoordinate operator -(SizeD a, XyzztCoordinate b)
        {
            return new XyzztCoordinate(a.Width - b.X, a.Height - b.Y, b.IZ, b.SZ, b.T);
        }

        public static XyzztCoordinate operator -(XyzztCoordinate a, PointD b)
        {
            return new XyzztCoordinate(a.X - b.X, a.Y - b.Y, a.IZ, a.SZ, a.T);
        }

        public static XyzztCoordinate operator -(PointD a, XyzztCoordinate b)
        {
            return new XyzztCoordinate(a.X - b.X, a.Y - b.Y, b.IZ, b.SZ, b.T);
        }

        public static XyzztCoordinate operator *(XyzztCoordinate a, XyzztCoordinate b)
        {
            return new XyzztCoordinate(a.X * b.X, a.Y * b.Y, a.IZ * b.IZ, a.SZ * b.SZ, a.T * b.T);
        }

        public static XyzztCoordinate operator *(XyzztCoordinate a, XyCoordinate b)
        {
            return new XyzztCoordinate(a.X * b.X, a.Y * b.Y, a.IZ, a.SZ, a.T);
        }

        public static XyzztCoordinate operator *(XyCoordinate a, XyzztCoordinate b)
        {
            return new XyzztCoordinate(a.X * b.X, a.Y * b.Y, b.IZ, b.SZ, b.T);
        }

        public static XyzztCoordinate operator *(XyzztCoordinate a, XytCoordinate b)
        {
            return new XyzztCoordinate(a.X * b.X, a.Y * b.Y, a.IZ, a.SZ, a.T * b.T);
        }

        public static XyzztCoordinate operator *(XytCoordinate a, XyzztCoordinate b)
        {
            return new XyzztCoordinate(a.X * b.X, a.Y * b.Y, b.IZ, b.SZ, a.T * b.T);
        }

        public static XyzztCoordinate operator *(XyzztCoordinate a, XyzCoordinate b)
        {
            return new XyzztCoordinate(a.X * b.X, a.Y * b.Y, a.IZ * b.Z, a.SZ * b.Z, a.T);
        }

        public static XyzztCoordinate operator *(XyzCoordinate a, XyzztCoordinate b)
        {
            return new XyzztCoordinate(a.X * b.X, a.Y * b.Y, a.Z * b.IZ, a.Z * b.SZ, b.T);
        }

        [Obsolete("삭제 예정입니다. 무슨 목적으로 만들었는지 공유해주세요.")]
        public static XyzztCoordinate operator *(XyzztCoordinate a, int b)
        {
            return new XyzztCoordinate(a.X * b, a.Y * b, a.IZ * b, a.SZ * b, a.T * b);
        }

        public static XyzztCoordinate operator *(int a, XyzztCoordinate b)
        {
            return new XyzztCoordinate(a * b.X, a * b.Y, a * b.IZ, a * b.SZ, a * b.T);
        }

        [Obsolete("삭제 예정입니다. 무슨 목적으로 만들었는지 공유해주세요.")]
        public static XyzztCoordinate operator *(XyzztCoordinate a, double b)
        {
            return new XyzztCoordinate(a.X * b, a.Y * b, a.IZ * b, a.SZ * b, a.T * b);
        }

        [Obsolete("삭제 예정입니다. 무슨 목적으로 만들었는지 공유해주세요.")]
        public static XyzztCoordinate operator *(double a, XyzztCoordinate b)
        {
            return new XyzztCoordinate(a * b.X, a * b.Y, a * b.IZ, a * b.SZ, a * b.T);
        }

        public static XyzztCoordinate operator *(XyzztCoordinate a, SizeD b)
        {
            return new XyzztCoordinate(a.X * b.Width, a.Y * b.Height, a.IZ, a.SZ, a.T);
        }

        public static XyzztCoordinate operator *(SizeD a, XyzztCoordinate b)
        {
            return new XyzztCoordinate(a.Width * b.X, a.Height * b.Y, b.IZ, b.SZ, b.T);
        }

        public static XyzztCoordinate operator *(XyzztCoordinate a, PointD b)
        {
            return new XyzztCoordinate(a.X * b.X, a.Y * b.Y, a.IZ, a.SZ, a.T);
        }

        public static XyzztCoordinate operator *(PointD a, XyzztCoordinate b)
        {
            return new XyzztCoordinate(a.X * b.X, a.Y * b.Y, b.IZ, b.SZ, b.T);
        }

        public static XyzztCoordinate operator /(XyzztCoordinate a, XyzztCoordinate b)
        {
            return new XyzztCoordinate(a.X / b.X, a.Y / b.Y, a.IZ / b.IZ, a.SZ / b.SZ, a.T / b.T);
        }

        public static XyzztCoordinate operator /(XyzztCoordinate a, XyCoordinate b)
        {
            return new XyzztCoordinate(a.X / b.X, a.Y / b.Y, a.IZ, a.SZ, a.T);
        }

        public static XyzztCoordinate operator /(XyCoordinate a, XyzztCoordinate b)
        {
            return new XyzztCoordinate(a.X / b.X, a.Y / b.Y, b.IZ, b.SZ, b.T);
        }

        public static XyzztCoordinate operator /(XyzztCoordinate a, XytCoordinate b)
        {
            return new XyzztCoordinate(a.X / b.X, a.Y / b.Y, a.IZ, a.SZ, a.T / b.T);
        }

        public static XyzztCoordinate operator /(XytCoordinate a, XyzztCoordinate b)
        {
            return new XyzztCoordinate(a.X / b.X, a.Y / b.Y, b.IZ, b.SZ, a.T / b.T);
        }

        public static XyzztCoordinate operator /(XyzztCoordinate a, XyzCoordinate b)
        {
            return new XyzztCoordinate(a.X / b.X, a.Y / b.Y, a.IZ / b.Z, a.SZ / b.Z, a.T);
        }

        public static XyzztCoordinate operator /(XyzCoordinate a, XyzztCoordinate b)
        {
            return new XyzztCoordinate(a.X / b.X, a.Y / b.Y, a.Z / b.IZ, a.Z / b.SZ, b.T);
        }

        [Obsolete("삭제 예정입니다. 무슨 목적으로 만들었는지 공유해주세요.")]
        public static XyzztCoordinate operator /(XyzztCoordinate a, int b)
        {
            return new XyzztCoordinate(a.X / b, a.Y / b, a.IZ / b, a.SZ / b, a.T / b);
        }

        [Obsolete("삭제 예정입니다. 무슨 목적으로 만들었는지 공유해주세요.")]
        public static XyzztCoordinate operator /(int a, XyzztCoordinate b)
        {
            return new XyzztCoordinate(a / b.X, a / b.Y, a / b.IZ, a / b.SZ, a / b.T);
        }

        [Obsolete("삭제 예정입니다. 무슨 목적으로 만들었는지 공유해주세요.")]
        public static XyzztCoordinate operator /(XyzztCoordinate a, double b)
        {
            return new XyzztCoordinate(a.X / b, a.Y / b, a.IZ / b, a.SZ / b, a.T / b);
        }

        [Obsolete("삭제 예정입니다. 무슨 목적으로 만들었는지 공유해주세요.")]
        public static XyzztCoordinate operator /(double a, XyzztCoordinate b)
        {
            return new XyzztCoordinate(a / b.X, a / b.Y, a / b.IZ, a / b.SZ, a / b.T);
        }

        public static XyzztCoordinate operator /(XyzztCoordinate a, SizeD b)
        {
            return new XyzztCoordinate(a.X / b.Width, a.Y / b.Height, a.IZ, a.SZ, a.T);
        }

        public static XyzztCoordinate operator /(SizeD a, XyzztCoordinate b)
        {
            return new XyzztCoordinate(a.Width / b.X, a.Height / b.Y, b.IZ, b.SZ, b.T);
        }

        public static XyzztCoordinate operator /(XyzztCoordinate a, PointD b)
        {
            return new XyzztCoordinate(a.X / b.X, a.Y / b.Y, a.IZ, a.SZ, a.T);
        }

        public static XyzztCoordinate operator /(PointD a, XyzztCoordinate b)
        {
            return new XyzztCoordinate(a.X / b.X, a.Y / b.Y, b.IZ, b.SZ, b.T);
        }

        public static explicit operator XyzztCoordinate(XyCoordinate a)
        {
            return new XyzztCoordinate(a.X, a.Y, 0.0, 0.0, 0.0);
        }

        public static explicit operator XyzztCoordinate(XyzCoordinate a)
        {
            return new XyzztCoordinate(a.X, a.Y, a.Z, a.Z, 0.0);
        }

        public static explicit operator XyzztCoordinate(XytCoordinate a)
        {
            return new XyzztCoordinate(a.X, a.Y, 0.0, 0.0, a.T);
        }

        public static explicit operator XyzztCoordinate(PointD a)
        {
            return new XyzztCoordinate(a.X, a.Y, 0.0, 0.0, 0.0);
        }
        #endregion

        #region Object Members
        public override bool Equals(object obj)
        {
            if (obj == null) return false;

            if (obj is XyzztCoordinate == false) return false;
            XyzztCoordinate value = (XyzztCoordinate)obj;

            return this.Equals(value);
        }

        public override int GetHashCode()
        {
            return this.X.GetHashCode() ^ this.Y.GetHashCode() ^ this.IZ.GetHashCode() ^ this.SZ.GetHashCode() ^ this.T.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("[{0}, {1}, {2}, {3}, {4}]", this.X, this.Y, this.IZ, this.SZ, this.T);
        }
        #endregion
    }
    #endregion

    #region XyzztCoordinateCollection
    [Serializable]
    public class XyzztCoordinateCollection : Collection<XyzztCoordinate>
    {
        #region Constructor
        public XyzztCoordinateCollection(IList<XyzztCoordinate> list) : base(list) { }
        public XyzztCoordinateCollection() : base() { }
        #endregion
    }
    #endregion
}
