using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QMC.Common
{
    #region XyzyCoordinate
    [Serializable]
    public struct XyzyCoordinate : IXyCoordinate
    {
        #region Field
        private double m_X;
        private double m_Y;
        private double m_Z;
        private double m_MASK_Y;
        #endregion

        #region Constructor
        public XyzyCoordinate(double x, double y, double z, double y1)
        {
            this.m_X = x;
            this.m_Y = y;
            this.m_Z = z;
            this.m_MASK_Y = y1;
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

        public double MASK_Y
        {
            get { return this.m_MASK_Y; }
            set { this.m_MASK_Y = value; }
        }
        #endregion

        #region Method
        public bool Equals(XyzyCoordinate value)
        {
            return this == value;
        }
        #endregion

        #region Static Members
        public static bool operator ==(XyzyCoordinate a, XyzyCoordinate b)
        {
            if (a.X == b.X && a.Y == b.Y && a.Z == b.Z && a.MASK_Y == b.MASK_Y)
                return true;
            else
                return false;
        }

        public static bool operator !=(XyzyCoordinate a, XyzyCoordinate b)
        {
            if (a.X == b.X && a.Y == b.Y && a.Z == b.Z && a.MASK_Y == b.MASK_Y)
                return false;
            else
                return true;
        }

        public static XyzyCoordinate operator +(XyzyCoordinate a, XyzyCoordinate b)
        {
            return new XyzyCoordinate(a.X + b.X, a.Y + b.Y, a.Z + b.Z, a.MASK_Y + b.MASK_Y);
        }

        public static XyzyCoordinate operator +(XyzyCoordinate a, XyCoordinate b)
        {
            return new XyzyCoordinate(a.X + b.X, a.Y + b.Y, a.Z, a.MASK_Y);
        }

        public static XyzyCoordinate operator +(XyCoordinate a, XyzyCoordinate b)
        {
            return new XyzyCoordinate(a.X + b.X, a.Y + b.Y, b.Z, b.MASK_Y);
        }

        //public static XyzyCoordinate operator +(XyzyCoordinate a, XytCoordinate b)
        //{
        //    return new XyzyCoordinate(a.X + b.X, a.Y + b.Y, a.Z, a.Y1 + b.T);
        //}

        //public static XyzyCoordinate operator +(XytCoordinate a, XyzyCoordinate b)
        //{
        //    return new XyzyCoordinate(a.X + b.X, a.Y + b.Y, b.Z, a.T + b.T);
        //}

        public static XyzyCoordinate operator +(XyzyCoordinate a, XyzCoordinate b)
        {
            return new XyzyCoordinate(a.X + b.X, a.Y + b.Y, a.Z + b.Z, a.MASK_Y);
        }

        public static XyzyCoordinate operator +(XyzCoordinate a, XyzyCoordinate b)
        {
            return new XyzyCoordinate(a.X + b.X, a.Y + b.Y, a.Z + b.Z, b.MASK_Y);
        }

        [Obsolete("삭제 예정입니다. 무슨 목적으로 만들었는지 공유해주세요.")]
        public static XyzyCoordinate operator +(XyzyCoordinate a, int b)
        {
            return new XyzyCoordinate(a.X + b, a.Y + b, a.Z + b, a.MASK_Y + b);
        }

        public static XyzyCoordinate operator +(int a, XyzyCoordinate b)
        {
            return new XyzyCoordinate(a + b.X, a + b.Y, a + b.Z, a + b.MASK_Y);
        }

        [Obsolete("삭제 예정입니다. 무슨 목적으로 만들었는지 공유해주세요.")]
        public static XyzyCoordinate operator +(XyzyCoordinate a, double b)
        {
            return new XyzyCoordinate(a.X + b, a.Y + b, a.Z + b, a.MASK_Y + b);
        }

        public static XyzyCoordinate operator +(double a, XyzyCoordinate b)
        {
            return new XyzyCoordinate(a + b.X, a + b.Y, a + b.Z, a + b.MASK_Y);
        }

        public static XyzyCoordinate operator +(XyzyCoordinate a, SizeD b)
        {
            return new XyzyCoordinate(a.X + b.Width, a.Y + b.Height, a.Z, a.MASK_Y);
        }

        public static XyzyCoordinate operator +(SizeD a, XyzyCoordinate b)
        {
            return new XyzyCoordinate(a.Width + b.X, a.Height + b.Y, b.Z, b.MASK_Y);
        }

        public static XyzyCoordinate operator +(XyzyCoordinate a, PointD b)
        {
            return new XyzyCoordinate(a.X + b.X, a.Y + b.Y, a.Z, a.MASK_Y);
        }

        public static XyzyCoordinate operator +(PointD a, XyzyCoordinate b)
        {
            return new XyzyCoordinate(a.X + b.X, a.Y + b.Y, b.Z, b.MASK_Y);
        }

        public static XyzyCoordinate operator -(XyzyCoordinate a, XyzyCoordinate b)
        {
            return new XyzyCoordinate(a.X - b.X, a.Y - b.Y, a.Z - b.Z, a.MASK_Y - b.MASK_Y);
        }

        public static XyzyCoordinate operator -(XyzyCoordinate a, XyCoordinate b)
        {
            return new XyzyCoordinate(a.X - b.X, a.Y - b.Y, a.Z, a.MASK_Y);
        }

        public static XyzyCoordinate operator -(XyCoordinate a, XyzyCoordinate b)
        {
            return new XyzyCoordinate(a.X - b.X, a.Y - b.Y, b.Z, b.MASK_Y);
        }

        //public static XyzyCoordinate operator -(XyzyCoordinate a, XytCoordinate b)
        //{
        //    return new XyzyCoordinate(a.X - b.X, a.Y - b.Y, a.Z, a.Y1 - b.T);
        //}

        //public static XyzyCoordinate operator -(XytCoordinate a, XyzyCoordinate b)
        //{
        //    return new XyzyCoordinate(a.X - b.X, a.Y - b.Y, b.Z, a.T - b.Y1);
        //}

        public static XyzyCoordinate operator -(XyzyCoordinate a, XyzCoordinate b)
        {
            return new XyzyCoordinate(a.X - b.X, a.Y - b.Y, a.Z - b.Z, a.MASK_Y);
        }

        public static XyzyCoordinate operator -(XyzCoordinate a, XyzyCoordinate b)
        {
            return new XyzyCoordinate(a.X - b.X, a.Y - b.Y, a.Z - b.Z, b.MASK_Y);
        }

        [Obsolete("삭제 예정입니다. 무슨 목적으로 만들었는지 공유해주세요.")]
        public static XyzyCoordinate operator -(XyzyCoordinate a, int b)
        {
            return new XyzyCoordinate(a.X - b, a.Y - b, a.Z - b, a.MASK_Y - b);
        }

        [Obsolete("삭제 예정입니다. 무슨 목적으로 만들었는지 공유해주세요.")]
        public static XyzyCoordinate operator -(int a, XyzyCoordinate b)
        {
            return new XyzyCoordinate(a - b.X, a - b.Y, a - b.Z, a - b.MASK_Y);
        }

        [Obsolete("삭제 예정입니다. 무슨 목적으로 만들었는지 공유해주세요.")]
        public static XyzyCoordinate operator -(XyzyCoordinate a, double b)
        {
            return new XyzyCoordinate(a.X - b, a.Y - b, a.Z - b, a.MASK_Y - b);
        }

        [Obsolete("삭제 예정입니다. 무슨 목적으로 만들었는지 공유해주세요.")]
        public static XyzyCoordinate operator -(double a, XyzyCoordinate b)
        {
            return new XyzyCoordinate(a - b.X, a - b.Y, a - b.Z, a - b.MASK_Y);
        }

        public static XyzyCoordinate operator -(XyzyCoordinate a, SizeD b)
        {
            return new XyzyCoordinate(a.X - b.Width, a.Y - b.Height, a.Z, a.MASK_Y);
        }

        public static XyzyCoordinate operator -(SizeD a, XyzyCoordinate b)
        {
            return new XyzyCoordinate(a.Width - b.X, a.Height - b.Y, b.Z, b.MASK_Y);
        }

        public static XyzyCoordinate operator -(XyzyCoordinate a, PointD b)
        {
            return new XyzyCoordinate(a.X - b.X, a.Y - b.Y, a.Z, a.MASK_Y);
        }

        public static XyzyCoordinate operator -(PointD a, XyzyCoordinate b)
        {
            return new XyzyCoordinate(a.X - b.X, a.Y - b.Y, b.Z, b.MASK_Y);
        }

        public static XyzyCoordinate operator *(XyzyCoordinate a, XyzyCoordinate b)
        {
            return new XyzyCoordinate(a.X * b.X, a.Y * b.Y, a.Z * b.Z, a.MASK_Y * b.MASK_Y);
        }

        public static XyzyCoordinate operator *(XyzyCoordinate a, XyCoordinate b)
        {
            return new XyzyCoordinate(a.X * b.X, a.Y * b.Y, a.Z, a.MASK_Y);
        }

        public static XyzyCoordinate operator *(XyCoordinate a, XyzyCoordinate b)
        {
            return new XyzyCoordinate(a.X * b.X, a.Y * b.Y, b.Z, b.MASK_Y);
        }

        //public static XyzyCoordinate operator *(XyzyCoordinate a, XytCoordinate b)
        //{
        //    return new XyzyCoordinate(a.X * b.X, a.Y * b.Y, a.Z, a.Y1 * b.T);
        //}

        //public static XyzyCoordinate operator *(XytCoordinate a, XyzyCoordinate b)
        //{
        //    return new XyzyCoordinate(a.X * b.X, a.Y * b.Y, b.Z, a.T * b.T);
        //}

        public static XyzyCoordinate operator *(XyzyCoordinate a, XyzCoordinate b)
        {
            return new XyzyCoordinate(a.X * b.X, a.Y * b.Y, a.Z * b.Z, a.MASK_Y);
        }

        public static XyzyCoordinate operator *(XyzCoordinate a, XyzyCoordinate b)
        {
            return new XyzyCoordinate(a.X * b.X, a.Y * b.Y, a.Z * b.Z, b.MASK_Y);
        }

        [Obsolete("삭제 예정입니다. 무슨 목적으로 만들었는지 공유해주세요.")]
        public static XyzyCoordinate operator *(XyzyCoordinate a, int b)
        {
            return new XyzyCoordinate(a.X * b, a.Y * b, a.Z * b, a.MASK_Y * b);
        }

        public static XyzyCoordinate operator *(int a, XyzyCoordinate b)
        {
            return new XyzyCoordinate(a * b.X, a * b.Y, a * b.Z, a * b.MASK_Y);
        }

        [Obsolete("삭제 예정입니다. 무슨 목적으로 만들었는지 공유해주세요.")]
        public static XyzyCoordinate operator *(XyzyCoordinate a, double b)
        {
            return new XyzyCoordinate(a.X * b, a.Y * b, a.Z * b, a.MASK_Y * b);
        }

        [Obsolete("삭제 예정입니다. 무슨 목적으로 만들었는지 공유해주세요.")]
        public static XyzyCoordinate operator *(double a, XyzyCoordinate b)
        {
            return new XyzyCoordinate(a * b.X, a * b.Y, a * b.Z, a * b.MASK_Y);
        }

        public static XyzyCoordinate operator *(XyzyCoordinate a, SizeD b)
        {
            return new XyzyCoordinate(a.X * b.Width, a.Y * b.Height, a.Z, a.MASK_Y);
        }

        public static XyzyCoordinate operator *(SizeD a, XyzyCoordinate b)
        {
            return new XyzyCoordinate(a.Width * b.X, a.Height * b.Y, b.Z, b.MASK_Y);
        }

        public static XyzyCoordinate operator *(XyzyCoordinate a, PointD b)
        {
            return new XyzyCoordinate(a.X * b.X, a.Y * b.Y, a.Z, a.MASK_Y);
        }

        public static XyzyCoordinate operator *(PointD a, XyzyCoordinate b)
        {
            return new XyzyCoordinate(a.X * b.X, a.Y * b.Y, b.Z, b.MASK_Y);
        }

        public static XyzyCoordinate operator /(XyzyCoordinate a, XyzyCoordinate b)
        {
            return new XyzyCoordinate(a.X / b.X, a.Y / b.Y, a.Z / b.Z, a.MASK_Y / b.MASK_Y);
        }

        public static XyzyCoordinate operator /(XyzyCoordinate a, XyCoordinate b)
        {
            return new XyzyCoordinate(a.X / b.X, a.Y / b.Y, a.Z, a.MASK_Y);
        }

        public static XyzyCoordinate operator /(XyCoordinate a, XyzyCoordinate b)
        {
            return new XyzyCoordinate(a.X / b.X, a.Y / b.Y, b.Z, b.MASK_Y);
        }

        //public static XyzyCoordinate operator /(XyzyCoordinate a, XytCoordinate b)
        //{
        //    return new XyzyCoordinate(a.X / b.X, a.Y / b.Y, a.Z, a.T / b.T);
        //}

        //public static XyzyCoordinate operator /(XytCoordinate a, XyzyCoordinate b)
        //{
        //    return new XyzyCoordinate(a.X / b.X, a.Y / b.Y, b.Z, a.T / b.T);
        //}

        public static XyzyCoordinate operator /(XyzyCoordinate a, XyzCoordinate b)
        {
            return new XyzyCoordinate(a.X / b.X, a.Y / b.Y, a.Z / b.Z, a.MASK_Y);
        }

        public static XyzyCoordinate operator /(XyzCoordinate a, XyzyCoordinate b)
        {
            return new XyzyCoordinate(a.X / b.X, a.Y / b.Y, a.Z / b.Z, b.MASK_Y);
        }

        [Obsolete("삭제 예정입니다. 무슨 목적으로 만들었는지 공유해주세요.")]
        public static XyzyCoordinate operator /(XyzyCoordinate a, int b)
        {
            return new XyzyCoordinate(a.X / b, a.Y / b, a.Z / b, a.MASK_Y / b);
        }

        [Obsolete("삭제 예정입니다. 무슨 목적으로 만들었는지 공유해주세요.")]
        public static XyzyCoordinate operator /(int a, XyzyCoordinate b)
        {
            return new XyzyCoordinate(a / b.X, a / b.Y, a / b.Z, a / b.MASK_Y);
        }

        [Obsolete("삭제 예정입니다. 무슨 목적으로 만들었는지 공유해주세요.")]
        public static XyzyCoordinate operator /(XyzyCoordinate a, double b)
        {
            return new XyzyCoordinate(a.X / b, a.Y / b, a.Z / b, a.MASK_Y / b);
        }

        [Obsolete("삭제 예정입니다. 무슨 목적으로 만들었는지 공유해주세요.")]
        public static XyzyCoordinate operator /(double a, XyzyCoordinate b)
        {
            return new XyzyCoordinate(a / b.X, a / b.Y, a / b.Z, a / b.MASK_Y);
        }

        public static XyzyCoordinate operator /(XyzyCoordinate a, SizeD b)
        {
            return new XyzyCoordinate(a.X / b.Width, a.Y / b.Height, a.Z, a.MASK_Y);
        }

        public static XyzyCoordinate operator /(SizeD a, XyzyCoordinate b)
        {
            return new XyzyCoordinate(a.Width / b.X, a.Height / b.Y, b.Z, b.MASK_Y);
        }

        public static XyzyCoordinate operator /(XyzyCoordinate a, PointD b)
        {
            return new XyzyCoordinate(a.X / b.X, a.Y / b.Y, a.Z, a.MASK_Y);
        }

        public static XyzyCoordinate operator /(PointD a, XyzyCoordinate b)
        {
            return new XyzyCoordinate(a.X / b.X, a.Y / b.Y, b.Z, b.MASK_Y);
        }

        public static explicit operator XyzyCoordinate(XyCoordinate a)
        {
            return new XyzyCoordinate(a.X, a.Y, 0.0, 0.0);
        }

        public static explicit operator XyzyCoordinate(XyzCoordinate a)
        {
            return new XyzyCoordinate(a.X, a.Y, a.Z, 0.0);
        }

        public static explicit operator XyzyCoordinate(XytCoordinate a)
        {
            return new XyzyCoordinate(a.X, a.Y, 0.0, a.T);
        }

        public static explicit operator XyzyCoordinate(PointD a)
        {
            return new XyzyCoordinate(a.X, a.Y, 0.0, 0.0);
        }
        #endregion

        #region Object Members
        public override bool Equals(object obj)
        {
            if (obj == null) return false;

            if (obj is XyzyCoordinate == false) return false;
            XyzyCoordinate value = (XyzyCoordinate)obj;

            return this.Equals(value);
        }

        public override int GetHashCode()
        {
            return this.X.GetHashCode() ^ this.Y.GetHashCode() ^ this.Z.GetHashCode() ^ this.MASK_Y.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("[{0}, {1}, {2}, {3}]", this.X, this.Y, this.Z, this.MASK_Y);
        }
        #endregion
    }
    #endregion

    #region XyztCoordinateCollection
    [Serializable]
    public class XyzyCoordinateCollection : Collection<XyzyCoordinate>
    {
        #region Constructor
        public XyzyCoordinateCollection(IList<XyzyCoordinate> list) : base(list) { }
        public XyzyCoordinateCollection() : base() { }
        #endregion
    }
    #endregion
}
