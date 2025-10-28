using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QMC.Common
{
    #region ZzxzxyCoordinate
    [Serializable]
    public struct ZzxzxyCoordinate //: IXyCoordinate
    {
        #region Field
        private double m_Z0;
        private double m_Z1;
        private double m_TR_X;
        private double m_TR_Z;
        private double m_ALN_X;
        private double m_ALN_Y;
        #endregion

        #region Constructor
        public ZzxzxyCoordinate(double z0, double z1, double trx, double trz, double alnx, double alny)
        {
            this.m_Z0 = z0;
            this.m_Z1 = z1;
            this.m_TR_X = trx;
            this.m_TR_Z = trz;
            this.m_ALN_X = alnx;
            this.m_ALN_Y = alny;
        }
        #endregion

        #region Property
        public double Z0
        {
            get { return this.m_Z0; }
            set { this.m_Z0 = value; }
        }

        public double Z1
        {
            get { return this.m_Z1; }
            set { this.m_Z1 = value; }
        }

        public double TR_X
        {
            get { return this.m_TR_X; }
            set { this.m_TR_X = value; }
        }

        public double TR_Z
        {
            get { return this.m_TR_Z; }
            set { this.m_TR_Z = value; }
        }

        public double ALN_X
        {
            get { return this.m_ALN_X; }
            set { this.m_ALN_Y = value; }
        }

        public double ALN_Y
        {
            get { return this.m_ALN_Y; }
            set { this.m_ALN_Y = value; }
        }
        #endregion

        #region Method
        public bool Equals(ZzxzxyCoordinate value)
        {
            return this == value;
        }
        #endregion

        #region Static Members
        public static bool operator ==(ZzxzxyCoordinate a, ZzxzxyCoordinate b)
        {
            if (a.Z0 == b.Z0 && a.Z1 == b.Z1 && a.TR_X == b.TR_X && a.TR_Z == b.TR_Z && a.ALN_X == b.ALN_X && a.ALN_Y == b.ALN_Y)
                return true;
            else
                return false;
        }

        public static bool operator !=(ZzxzxyCoordinate a, ZzxzxyCoordinate b)
        {
            if (a.Z0 == b.Z0 && a.Z1 == b.Z1 && a.TR_X == b.TR_X && a.TR_Z == b.TR_Z && a.ALN_X == b.ALN_X && a.ALN_Y == b.ALN_Y)
                return false;
            else
                return true;
        }

        public static ZzxzxyCoordinate operator +(ZzxzxyCoordinate a, ZzxzxyCoordinate b)
        {
            return new ZzxzxyCoordinate(a.Z0 + b.Z0, a.Z1 + b.Z1, a.TR_X + b.TR_X, a.TR_Z + b.TR_Z, a.ALN_X + b.ALN_X, a.ALN_Y + b.ALN_Y);
        }

        //public static ZzxzxyCoordinate operator +(ZzxzxyCoordinate a, XyCoordinate b)
        //{
        //    return new ZzxzxyCoordinate(a.X + b.X, a.Y + b.Y, a.IZ, a.SZ, a.T);
        //}

        //public static ZzxzxyCoordinate operator +(XyCoordinate a, ZzxzxyCoordinate b)
        //{
        //    return new ZzxzxyCoordinate(a.X + b.X, a.Y + b.Y, b.IZ, b.SZ, b.T);
        //}

        //public static ZzxzxyCoordinate operator +(ZzxzxyCoordinate a, XytCoordinate b)
        //{
        //    return new ZzxzxyCoordinate(a.X + b.X, a.Y + b.Y, a.IZ, a.SZ, a.T + b.T);
        //}

        //public static ZzxzxyCoordinate operator +(XytCoordinate a, ZzxzxyCoordinate b)
        //{
        //    return new ZzxzxyCoordinate(a.X + b.X, a.Y + b.Y, b.IZ, b.SZ, a.T + b.T);
        //}

        //public static ZzxzxyCoordinate operator +(ZzxzxyCoordinate a, XyzCoordinate b)
        //{
        //    return new ZzxzxyCoordinate(a.X + b.X, a.Y + b.Y, a.IZ + b.Z, a.SZ + b.Z, a.T);
        //}

        //public static ZzxzxyCoordinate operator +(XyzCoordinate a, ZzxzxyCoordinate b)
        //{
        //    return new ZzxzxyCoordinate(a.X + b.X, a.Y + b.Y, a.Z + b.IZ, a.Z + b.SZ, b.T);
        //}

        //public static ZzxzxyCoordinate operator +(int a, ZzxzxyCoordinate b)
        //{
        //    return new ZzxzxyCoordinate(a + b.X, a + b.Y, a + b.IZ, a + b.SZ, a + b.T);
        //}

        //public static ZzxzxyCoordinate operator +(double a, ZzxzxyCoordinate b)
        //{
        //    return new ZzxzxyCoordinate(a + b.X, a + b.Y, a + b.IZ, a + b.SZ, a + b.T);
        //}

        //public static ZzxzxyCoordinate operator +(ZzxzxyCoordinate a, SizeD b)
        //{
        //    return new ZzxzxyCoordinate(a.X + b.Width, a.Y + b.Height, a.IZ, a.SZ, a.T);
        //}

        //public static ZzxzxyCoordinate operator +(SizeD a, ZzxzxyCoordinate b)
        //{
        //    return new ZzxzxyCoordinate(a.Width + b.X, a.Height + b.Y, b.IZ, b.SZ, b.T);
        //}

        //public static ZzxzxyCoordinate operator +(ZzxzxyCoordinate a, PointD b)
        //{
        //    return new ZzxzxyCoordinate(a.X + b.X, a.Y + b.Y, a.IZ, a.SZ, a.T);
        //}

        //public static ZzxzxyCoordinate operator +(PointD a, ZzxzxyCoordinate b)
        //{
        //    return new ZzxzxyCoordinate(a.X + b.X, a.Y + b.Y, b.IZ, b.SZ, b.T);
        //}

        //public static ZzxzxyCoordinate operator -(ZzxzxyCoordinate a, ZzxzxyCoordinate b)
        //{
        //    return new ZzxzxyCoordinate(a.X - b.X, a.Y - b.Y, a.IZ - b.IZ, a.SZ - b.SZ, a.T - b.T);
        //}

        //public static ZzxzxyCoordinate operator -(ZzxzxyCoordinate a, XyCoordinate b)
        //{
        //    return new ZzxzxyCoordinate(a.X - b.X, a.Y - b.Y, a.IZ, a.SZ, a.T);
        //}

        //public static ZzxzxyCoordinate operator -(XyCoordinate a, ZzxzxyCoordinate b)
        //{
        //    return new ZzxzxyCoordinate(a.X - b.X, a.Y - b.Y, b.IZ, b.SZ, b.T);
        //}

        //public static ZzxzxyCoordinate operator -(ZzxzxyCoordinate a, XytCoordinate b)
        //{
        //    return new ZzxzxyCoordinate(a.X - b.X, a.Y - b.Y, a.IZ, a.SZ, a.T - b.T);
        //}

        //public static ZzxzxyCoordinate operator -(XytCoordinate a, ZzxzxyCoordinate b)
        //{
        //    return new ZzxzxyCoordinate(a.X - b.X, a.Y - b.Y, b.IZ, b.SZ, a.T - b.T);
        //}

        //public static ZzxzxyCoordinate operator -(ZzxzxyCoordinate a, XyzCoordinate b)
        //{
        //    return new ZzxzxyCoordinate(a.X - b.X, a.Y - b.Y, a.IZ - b.Z, a.SZ - b.Z, a.T);
        //}

        //public static ZzxzxyCoordinate operator -(XyzCoordinate a, ZzxzxyCoordinate b)
        //{
        //    return new ZzxzxyCoordinate(a.X - b.X, a.Y - b.Y, a.Z - b.IZ, a.Z - b.SZ, b.T);
        //}

        //public static ZzxzxyCoordinate operator -(ZzxzxyCoordinate a, SizeD b)
        //{
        //    return new ZzxzxyCoordinate(a.X - b.Width, a.Y - b.Height, a.IZ, a.SZ, a.T);
        //}

        //public static ZzxzxyCoordinate operator -(SizeD a, ZzxzxyCoordinate b)
        //{
        //    return new ZzxzxyCoordinate(a.Width - b.X, a.Height - b.Y, b.IZ, b.SZ, b.T);
        //}

        //public static ZzxzxyCoordinate operator -(ZzxzxyCoordinate a, PointD b)
        //{
        //    return new ZzxzxyCoordinate(a.X - b.X, a.Y - b.Y, a.IZ, a.SZ, a.T);
        //}

        //public static ZzxzxyCoordinate operator -(PointD a, ZzxzxyCoordinate b)
        //{
        //    return new ZzxzxyCoordinate(a.X - b.X, a.Y - b.Y, b.IZ, b.SZ, b.T);
        //}

        //public static ZzxzxyCoordinate operator *(ZzxzxyCoordinate a, ZzxzxyCoordinate b)
        //{
        //    return new ZzxzxyCoordinate(a.X * b.X, a.Y * b.Y, a.IZ * b.IZ, a.SZ * b.SZ, a.T * b.T);
        //}

        //public static ZzxzxyCoordinate operator *(ZzxzxyCoordinate a, XyCoordinate b)
        //{
        //    return new ZzxzxyCoordinate(a.X * b.X, a.Y * b.Y, a.IZ, a.SZ, a.T);
        //}

        //public static ZzxzxyCoordinate operator *(XyCoordinate a, ZzxzxyCoordinate b)
        //{
        //    return new ZzxzxyCoordinate(a.X * b.X, a.Y * b.Y, b.IZ, b.SZ, b.T);
        //}

        //public static ZzxzxyCoordinate operator *(ZzxzxyCoordinate a, XytCoordinate b)
        //{
        //    return new ZzxzxyCoordinate(a.X * b.X, a.Y * b.Y, a.IZ, a.SZ, a.T * b.T);
        //}

        //public static ZzxzxyCoordinate operator *(XytCoordinate a, ZzxzxyCoordinate b)
        //{
        //    return new ZzxzxyCoordinate(a.X * b.X, a.Y * b.Y, b.IZ, b.SZ, a.T * b.T);
        //}

        //public static ZzxzxyCoordinate operator *(ZzxzxyCoordinate a, XyzCoordinate b)
        //{
        //    return new ZzxzxyCoordinate(a.X * b.X, a.Y * b.Y, a.IZ * b.Z, a.SZ * b.Z, a.T);
        //}

        //public static ZzxzxyCoordinate operator *(XyzCoordinate a, ZzxzxyCoordinate b)
        //{
        //    return new ZzxzxyCoordinate(a.X * b.X, a.Y * b.Y, a.Z * b.IZ, a.Z * b.SZ, b.T);
        //}

        //public static ZzxzxyCoordinate operator *(int a, ZzxzxyCoordinate b)
        //{
        //    return new ZzxzxyCoordinate(a * b.X, a * b.Y, a * b.IZ, a * b.SZ, a * b.T);
        //}

        //public static ZzxzxyCoordinate operator *(ZzxzxyCoordinate a, SizeD b)
        //{
        //    return new ZzxzxyCoordinate(a.X * b.Width, a.Y * b.Height, a.IZ, a.SZ, a.T);
        //}

        //public static ZzxzxyCoordinate operator *(SizeD a, ZzxzxyCoordinate b)
        //{
        //    return new ZzxzxyCoordinate(a.Width * b.X, a.Height * b.Y, b.IZ, b.SZ, b.T);
        //}

        //public static ZzxzxyCoordinate operator *(ZzxzxyCoordinate a, PointD b)
        //{
        //    return new ZzxzxyCoordinate(a.X * b.X, a.Y * b.Y, a.IZ, a.SZ, a.T);
        //}

        //public static ZzxzxyCoordinate operator *(PointD a, ZzxzxyCoordinate b)
        //{
        //    return new ZzxzxyCoordinate(a.X * b.X, a.Y * b.Y, b.IZ, b.SZ, b.T);
        //}

        //public static ZzxzxyCoordinate operator /(ZzxzxyCoordinate a, ZzxzxyCoordinate b)
        //{
        //    return new ZzxzxyCoordinate(a.X / b.X, a.Y / b.Y, a.IZ / b.IZ, a.SZ / b.SZ, a.T / b.T);
        //}

        //public static ZzxzxyCoordinate operator /(ZzxzxyCoordinate a, XyCoordinate b)
        //{
        //    return new ZzxzxyCoordinate(a.X / b.X, a.Y / b.Y, a.IZ, a.SZ, a.T);
        //}

        //public static ZzxzxyCoordinate operator /(XyCoordinate a, ZzxzxyCoordinate b)
        //{
        //    return new ZzxzxyCoordinate(a.X / b.X, a.Y / b.Y, b.IZ, b.SZ, b.T);
        //}

        //public static ZzxzxyCoordinate operator /(ZzxzxyCoordinate a, XytCoordinate b)
        //{
        //    return new ZzxzxyCoordinate(a.X / b.X, a.Y / b.Y, a.IZ, a.SZ, a.T / b.T);
        //}

        //public static ZzxzxyCoordinate operator /(XytCoordinate a, ZzxzxyCoordinate b)
        //{
        //    return new ZzxzxyCoordinate(a.X / b.X, a.Y / b.Y, b.IZ, b.SZ, a.T / b.T);
        //}

        //public static ZzxzxyCoordinate operator /(ZzxzxyCoordinate a, XyzCoordinate b)
        //{
        //    return new ZzxzxyCoordinate(a.X / b.X, a.Y / b.Y, a.IZ / b.Z, a.SZ / b.Z, a.T);
        //}

        //public static ZzxzxyCoordinate operator /(XyzCoordinate a, ZzxzxyCoordinate b)
        //{
        //    return new ZzxzxyCoordinate(a.X / b.X, a.Y / b.Y, a.Z / b.IZ, a.Z / b.SZ, b.T);
        //}

        //public static ZzxzxyCoordinate operator /(ZzxzxyCoordinate a, SizeD b)
        //{
        //    return new ZzxzxyCoordinate(a.X / b.Width, a.Y / b.Height, a.IZ, a.SZ, a.T);
        //}

        //public static ZzxzxyCoordinate operator /(SizeD a, ZzxzxyCoordinate b)
        //{
        //    return new ZzxzxyCoordinate(a.Width / b.X, a.Height / b.Y, b.IZ, b.SZ, b.T);
        //}

        //public static ZzxzxyCoordinate operator /(ZzxzxyCoordinate a, PointD b)
        //{
        //    return new ZzxzxyCoordinate(a.X / b.X, a.Y / b.Y, a.IZ, a.SZ, a.T);
        //}

        //public static ZzxzxyCoordinate operator /(PointD a, ZzxzxyCoordinate b)
        //{
        //    return new ZzxzxyCoordinate(a.X / b.X, a.Y / b.Y, b.IZ, b.SZ, b.T);
        //}

        //public static explicit operator ZzxzxyCoordinate(XyCoordinate a)
        //{
        //    return new ZzxzxyCoordinate(a.X, a.Y, 0.0, 0.0, 0.0);
        //}

        //public static explicit operator ZzxzxyCoordinate(XyzCoordinate a)
        //{
        //    return new ZzxzxyCoordinate(a.X, a.Y, a.Z, a.Z, 0.0);
        //}

        //public static explicit operator ZzxzxyCoordinate(XytCoordinate a)
        //{
        //    return new ZzxzxyCoordinate(a.X, a.Y, 0.0, 0.0, a.T);
        //}

        //public static explicit operator ZzxzxyCoordinate(PointD a)
        //{
        //    return new ZzxzxyCoordinate(a.X, a.Y, 0.0, 0.0, 0.0);
        //}
        #endregion

        #region Object Members
        public override bool Equals(object obj)
        {
            if (obj == null) return false;

            if (obj is ZzxzxyCoordinate == false) return false;
            ZzxzxyCoordinate value = (ZzxzxyCoordinate)obj;

            return this.Equals(value);
        }

        public override int GetHashCode()
        {
            return this.Z0.GetHashCode() ^ this.Z1.GetHashCode() ^ this.TR_X.GetHashCode() ^ this.TR_Z.GetHashCode() ^ this.ALN_X.GetHashCode() ^ this.ALN_Y.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("[{0}, {1}, {2}, {3}, {4}, {5}]", this.Z0, this.Z1, this.TR_X, this.TR_Z, this.ALN_X, this.ALN_Y);
        }
        #endregion
    }
    #endregion

    #region ZzxzxyCoordinateCollection
    [Serializable]
    public class ZzxzxyCoordinateCollection : Collection<ZzxzxyCoordinate>
    {
        #region Constructor
        public ZzxzxyCoordinateCollection(IList<ZzxzxyCoordinate> list) : base(list) { }
        public ZzxzxyCoordinateCollection() : base() { }
        #endregion
    }
    #endregion 
}
