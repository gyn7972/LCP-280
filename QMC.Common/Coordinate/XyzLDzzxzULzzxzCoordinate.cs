using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QMC.Common
{
    #region XyzLDzzxzULzzxzCoordinate

    [Serializable]
    public struct XyzLDzzxzULzzxzCoordinate : IXyCoordinate
    {
        #region Field
        private double m_X;
        private double m_Y;
        private double m_Z;
        private double m_MASK_Y;
        private double m_LD_SZ0;
        private double m_LD_SZ1;
        private double m_LD_TRX;
        private double m_LD_TRZ;
        private double m_ALN_X;
        private double m_ALN_Y;
        private double m_UL_SZ0;
        private double m_UL_SZ1;
        private double m_UL_TRX;
        private double m_UL_TRZ;
        #endregion

        #region Constructor
        //public XyzLDzzxzULzzxzCoordinate(double x, double y, double z, double LD_SZ0, double LD_SZ1, double LD_TRX, double LD_TRZ, double UL_SZ0, double UL_SZ1, double UL_TRX, double UL_TRZ)
        public XyzLDzzxzULzzxzCoordinate(double x, double y, double z, double MASK_Y, double LD_SZ0, double LD_SZ1, double LD_TRX, double LD_TRZ, double ALN_X, double ALN_Y, double UL_SZ0, double UL_SZ1, double UL_TRX, double UL_TRZ)
        {
            this.m_X = x;
            this.m_Y = y;
            this.m_Z = z;
            this.m_MASK_Y = MASK_Y;
            this.m_LD_SZ0 = LD_SZ0;
            this.m_LD_SZ1 = LD_SZ1;
            this.m_LD_TRX = LD_TRX;
            this.m_LD_TRZ = LD_TRZ;
            this.m_ALN_X = ALN_X;
            this.m_ALN_Y = ALN_Y;
            this.m_UL_SZ0 = UL_SZ0;
            this.m_UL_SZ1 = UL_SZ1;
            this.m_UL_TRX = UL_TRX;
            this.m_UL_TRZ = UL_TRZ;
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

        public double LD_SZ0
        {
            get { return this.m_LD_SZ0; }
            set { this.m_LD_SZ0 = value; }
        }

        public double LD_SZ1
        {
            get { return this.m_LD_SZ1; }
            set { this.m_LD_SZ1 = value; }
        }

        public double LD_TRX
        {
            get { return this.m_LD_TRX; }
            set { this.m_LD_TRX = value; }
        }

        public double LD_TRZ
        {
            get { return this.m_LD_TRZ; }
            set { this.m_LD_TRZ = value; }
        }

        public double ALN_X
        {
            get { return this.m_ALN_X; }
            set { this.m_ALN_X = value; }
        }

        public double ALN_Y
        {
            get { return this.m_ALN_Y; }
            set { this.m_ALN_Y = value; }
        }

        public double UL_SZ0
        {
            get { return this.m_UL_SZ0; }
            set { this.m_UL_SZ0 = value; }
        }

        public double UL_SZ1
        {
            get { return this.m_UL_SZ1; }
            set { this.m_UL_SZ1 = value; }
        }

        public double UL_TRX
        {
            get { return this.m_UL_TRX; }
            set { this.m_UL_TRX = value; }
        }

        public double UL_TRZ
        {
            get { return this.m_UL_TRZ; }         
            set { this.m_UL_TRZ = value; }
        }
        #endregion

        #region Method
        public bool Equals(XyzLDzzxzULzzxzCoordinate value)
        {
            return this == value;
        }
        #endregion

        #region Static Members
        public static bool operator ==(XyzLDzzxzULzzxzCoordinate a, XyzLDzzxzULzzxzCoordinate b)
        {
            if (a.X == b.X && a.Y == b.Y && a.Z == b.Z && a.LD_SZ0 == b.LD_SZ0 && a.LD_SZ1 == b.LD_SZ1 && a.LD_TRX == b.LD_TRX && a.LD_TRZ == b.LD_TRZ &&
                                                          a.UL_SZ0 == b.UL_SZ0 && a.UL_SZ1 == b.UL_SZ1 && a.UL_TRX == b.UL_TRX && a.UL_TRZ == b.UL_TRZ)
                return true;
            else
                return false;
        }

        public static bool operator !=(XyzLDzzxzULzzxzCoordinate a, XyzLDzzxzULzzxzCoordinate b)
        {
            if (a.X == b.X && a.Y == b.Y && a.Z == b.Z && a.LD_SZ0 == b.LD_SZ0 && a.LD_SZ1 == b.LD_SZ1 && a.LD_TRX == b.LD_TRX && a.LD_TRZ == b.LD_TRZ &&
                                                          a.UL_SZ0 == b.UL_SZ0 && a.UL_SZ1 == b.UL_SZ1 && a.UL_TRX == b.UL_TRX && a.UL_TRZ == b.UL_TRZ)
                return false;
            else
                return true;
        }

        public static XyzLDzzxzULzzxzCoordinate operator +(XyzLDzzxzULzzxzCoordinate a, XyzLDzzxzULzzxzCoordinate b)
        {
            return new XyzLDzzxzULzzxzCoordinate(a.X + b.X, a.Y + b.Y, a.Z + b.Z, a.m_MASK_Y + b.m_MASK_Y, a.LD_SZ0 + b.LD_SZ0, a.LD_SZ1 + b.LD_SZ1, a.LD_TRX + b.LD_TRX, a.LD_TRZ + b.LD_TRZ, a.m_ALN_X + b.m_ALN_X, a.m_ALN_Y + b.m_ALN_Y,
                                                                                                           a.UL_SZ0 + b.UL_SZ0, a.UL_SZ1 + b.UL_SZ1, a.UL_TRX + b.UL_TRX, a.UL_TRZ + b.UL_TRZ);
        }

        public static XyzLDzzxzULzzxzCoordinate operator +(XyzLDzzxzULzzxzCoordinate a, XyCoordinate b)
        {
            return new XyzLDzzxzULzzxzCoordinate(a.X + b.X, a.Y + b.Y, a.Z, a.m_MASK_Y, a.LD_SZ0, a.LD_SZ1, a.LD_TRX, a.LD_TRZ, a.m_ALN_X, a.m_ALN_Y, a.UL_SZ0, a.UL_SZ1, a.UL_TRX, a.UL_TRZ);
        }

        public static XyzLDzzxzULzzxzCoordinate operator +(XyCoordinate a, XyzLDzzxzULzzxzCoordinate b)
        {
            return new XyzLDzzxzULzzxzCoordinate(a.X + b.X, a.Y + b.Y, b.Z, b.m_MASK_Y, b.LD_SZ0, b.LD_SZ1, b.LD_TRX, b.LD_TRZ, b.m_ALN_X, b.m_ALN_Y, b.UL_SZ0, b.UL_SZ1, b.UL_TRX, b.UL_TRZ);
        }

        //public static UvwxyzzCoordinate operator +(UvwxyzzCoordinate a, XytCoordinate b)
        //{
        //    return new UvwxyzzCoordinate(a.X + b.X, a.Y + b.Y, a.IZ, a.SZ, a.T + b.T);
        //}

        //public static UvwxyzzCoordinate operator +(XytCoordinate a, UvwxyzzCoordinate b)
        //{
        //    return new UvwxyzzCoordinate(a.X + b.X, a.Y + b.Y, b.IZ, b.SZ, a.T + b.T);
        //}

        public static XyzLDzzxzULzzxzCoordinate operator +(XyzLDzzxzULzzxzCoordinate a, XyzCoordinate b)
        {
            return new XyzLDzzxzULzzxzCoordinate(a.X + b.X, a.Y + b.Y, a.Z + b.Z, a.m_MASK_Y, a.LD_SZ0, a.LD_SZ1, a.LD_TRX, a.LD_TRZ, a.m_ALN_X, a.m_ALN_Y, a.UL_SZ0, a.UL_SZ1, a.UL_TRX, a.UL_TRZ);
        }

        public static XyzLDzzxzULzzxzCoordinate operator +(XyzCoordinate a, XyzLDzzxzULzzxzCoordinate b)
        {
            return new XyzLDzzxzULzzxzCoordinate(a.X + b.X, a.Y + b.Y, a.Z + b.Z, b.m_MASK_Y, b.LD_SZ0, b.LD_SZ1, b.LD_TRX, b.LD_TRZ, b.m_ALN_X, b.m_ALN_Y, b.UL_SZ0, b.UL_SZ1, b.UL_TRX, b.UL_TRZ);
        }

        public static XyzLDzzxzULzzxzCoordinate operator +(int a, XyzLDzzxzULzzxzCoordinate b)
        {
            return new XyzLDzzxzULzzxzCoordinate(a + b.X, a + b.Y, a + b.Z, a + b.m_MASK_Y, a + b.LD_SZ0, a + b.LD_SZ1, a + b.LD_TRX, a + b.LD_TRZ, a + b.m_ALN_X, a + b.m_ALN_Y, a + b.UL_SZ0, a + b.UL_SZ1, a + b.UL_TRX, a + b.UL_TRZ);
        }

        public static XyzLDzzxzULzzxzCoordinate operator +(double a, XyzLDzzxzULzzxzCoordinate b)
        {
            return new XyzLDzzxzULzzxzCoordinate(a + b.X, a + b.Y, a + b.Z, a + b.m_MASK_Y, a + b.LD_SZ0, a + b.LD_SZ1, a + b.LD_TRX, a + b.LD_TRZ, a + b.m_ALN_X, a + b.m_ALN_Y, a + b.UL_SZ0, a + b.UL_SZ1, a + b.UL_TRX, a + b.UL_TRZ);
        }

        //public static XyzLDzzxzULzzxzCoordinate operator +(XyzLDzzxzULzzxzCoordinate a, SizeD b)
        //{
        //    return new XyzLDzzxzULzzxzCoordinate(a.U, a.V, a.W, a.EZ, a.X + b.Width, a.Y + b.Height, a.VZ);
        //}

        //public static XyzLDzzxzULzzxzCoordinate operator +(SizeD a, XyzLDzzxzULzzxzCoordinate b)
        //{
        //    return new XyzLDzzxzULzzxzCoordinate(b.U, b.V, b.W, b.EZ, a.Width + b.X, a.Height + b.Y, b.VZ);
        //}

        //public static XyzLDzzxzULzzxzCoordinate operator +(XyzLDzzxzULzzxzCoordinate a, PointD b)
        //{
        //    return new XyzLDzzxzULzzxzCoordinate(a.U, a.V, a.W, a.EZ, a.X + b.X, a.Y + b.Y, a.VZ);
        //}

        //public static XyzLDzzxzULzzxzCoordinate operator +(PointD a, XyzLDzzxzULzzxzCoordinate b)
        //{
        //    return new XyzLDzzxzULzzxzCoordinate(b.U, b.V, b.W, b.EZ, a.X + b.X, a.Y + b.Y, b.VZ);
        //}

        //public static XyzLDzzxzULzzxzCoordinate operator -(XyzLDzzxzULzzxzCoordinate a, XyzLDzzxzULzzxzCoordinate b)
        //{
        //    return new XyzLDzzxzULzzxzCoordinate(a.U - b.U, a.V - b.V, a.W - b.W, a.EZ - b.EZ, a.X - b.X, a.Y - b.Y, a.VZ - b.VZ);
        //}

        //public static XyzLDzzxzULzzxzCoordinate operator -(XyzLDzzxzULzzxzCoordinate a, XyCoordinate b)
        //{
        //    return new XyzLDzzxzULzzxzCoordinate(a.U, a.V, a.W, a.EZ, a.X - b.X, a.Y - b.Y, a.VZ);
        //}

        //public static XyzLDzzxzULzzxzCoordinate operator -(XyCoordinate a, XyzLDzzxzULzzxzCoordinate b)
        //{
        //    return new XyzLDzzxzULzzxzCoordinate(b.U, b.V, b.W, b.EZ, a.X - b.X, a.Y - b.Y, b.VZ);
        //}

        //public static UvwxyzzCoordinate operator -(UvwxyzzCoordinate a, XytCoordinate b)
        //{
        //    return new UvwxyzzCoordinate(a.X - b.X, a.Y - b.Y, a.IZ, a.SZ, a.T - b.T);
        //}

        //public static UvwxyzzCoordinate operator -(XytCoordinate a, UvwxyzzCoordinate b)
        //{
        //    return new UvwxyzzCoordinate(a.X - b.X, a.Y - b.Y, b.IZ, b.SZ, a.T - b.T);
        //}

        //public static XyzLDzzxzULzzxzCoordinate operator -(XyzLDzzxzULzzxzCoordinate a, XyzCoordinate b)
        //{
        //    return new XyzLDzzxzULzzxzCoordinate(a.U, a.V, a.W, a.EZ, a.X - b.X, a.Y - b.Y, a.VZ - b.Z);
        //}

        //public static XyzLDzzxzULzzxzCoordinate operator -(XyzCoordinate a, XyzLDzzxzULzzxzCoordinate b)
        //{
        //    return new XyzLDzzxzULzzxzCoordinate(b.U, b.V, b.W, b.EZ, a.X - b.X, a.Y - b.Y, a.Z - b.VZ);
        //}

        //public static XyzLDzzxzULzzxzCoordinate operator -(XyzLDzzxzULzzxzCoordinate a, SizeD b)
        //{
        //    return new XyzLDzzxzULzzxzCoordinate(a.U, a.V, a.W, a.EZ, a.X - b.Width, a.Y - b.Height, a.VZ);
        //}

        //public static XyzLDzzxzULzzxzCoordinate operator -(SizeD a, XyzLDzzxzULzzxzCoordinate b)
        //{
        //    return new XyzLDzzxzULzzxzCoordinate(b.U, b.V, b.W, b.EZ, a.Width - b.X, a.Height - b.Y, b.VZ);
        //}

        //public static XyzLDzzxzULzzxzCoordinate operator -(XyzLDzzxzULzzxzCoordinate a, PointD b)
        //{
        //    return new XyzLDzzxzULzzxzCoordinate(a.U, a.V, a.W, a.EZ, a.X - b.X, a.Y - b.Y, a.VZ);
        //}

        //public static XyzLDzzxzULzzxzCoordinate operator -(PointD a, XyzLDzzxzULzzxzCoordinate b)
        //{
        //    return new XyzLDzzxzULzzxzCoordinate(b.U, b.V, b.W, b.EZ, a.X - b.X, a.Y - b.Y, b.VZ);
        //}

        //public static XyzLDzzxzULzzxzCoordinate operator *(XyzLDzzxzULzzxzCoordinate a, XyzLDzzxzULzzxzCoordinate b)
        //{
        //    return new XyzLDzzxzULzzxzCoordinate(a.U * b.U, a.V * b.V, a.W * b.W, a.EZ * b.EZ, a.X * b.X, a.Y * b.Y, a.VZ * b.VZ);
        //}

        //public static XyzLDzzxzULzzxzCoordinate operator *(XyzLDzzxzULzzxzCoordinate a, XyCoordinate b)
        //{
        //    return new XyzLDzzxzULzzxzCoordinate(a.U, a.V, a.W, a.EZ, a.X * b.X, a.Y * b.Y, a.VZ);
        //}

        //public static XyzLDzzxzULzzxzCoordinate operator *(XyCoordinate a, XyzLDzzxzULzzxzCoordinate b)
        //{
        //    return new XyzLDzzxzULzzxzCoordinate(b.U, b.V, b.W, b.EZ, a.X * b.X, a.Y * b.Y, b.VZ);
        //}

        //public static UvwxyzzCoordinate operator *(UvwxyzzCoordinate a, XytCoordinate b)
        //{
        //    return new UvwxyzzCoordinate(a.X * b.X, a.Y * b.Y, a.IZ, a.SZ, a.T * b.T);
        //}

        //public static UvwxyzzCoordinate operator *(XytCoordinate a, UvwxyzzCoordinate b)
        //{
        //    return new UvwxyzzCoordinate(a.X * b.X, a.Y * b.Y, b.IZ, b.SZ, a.T * b.T);
        //}

        //public static XyzLDzzxzULzzxzCoordinate operator *(XyzLDzzxzULzzxzCoordinate a, XyzCoordinate b)
        //{
        //    return new XyzLDzzxzULzzxzCoordinate(a.U, a.V, a.W, a.EZ, a.X * b.X, a.Y * b.Y, a.VZ * b.Z);
        //}

        //public static XyzLDzzxzULzzxzCoordinate operator *(XyzCoordinate a, XyzLDzzxzULzzxzCoordinate b)
        //{
        //    return new XyzLDzzxzULzzxzCoordinate(b.U, b.V, b.W, b.EZ, a.X * b.X, a.Y * b.Y, a.Z * b.VZ);
        //}

        //public static XyzLDzzxzULzzxzCoordinate operator *(XyzLDzzxzULzzxzCoordinate a, SizeD b)
        //{
        //    return new XyzLDzzxzULzzxzCoordinate(a.U, a.V, a.W, a.EZ, a.X * b.Width, a.Y * b.Height, a.VZ);
        //}

        //public static XyzLDzzxzULzzxzCoordinate operator *(SizeD a, XyzLDzzxzULzzxzCoordinate b)
        //{
        //    return new XyzLDzzxzULzzxzCoordinate(b.U, b.V, b.W, b.EZ, a.Width * b.X, a.Height * b.Y, b.VZ);
        //}

        //public static XyzLDzzxzULzzxzCoordinate operator *(XyzLDzzxzULzzxzCoordinate a, PointD b)
        //{
        //    return new XyzLDzzxzULzzxzCoordinate(a.U, a.V, a.W, a.EZ, a.X * b.X, a.Y * b.Y, a.VZ);
        //}

        //public static XyzLDzzxzULzzxzCoordinate operator *(PointD a, XyzLDzzxzULzzxzCoordinate b)
        //{
        //    return new XyzLDzzxzULzzxzCoordinate(b.U, b.V, b.W, b.EZ, a.X * b.X, a.Y * b.Y, b.VZ);
        //}

        //public static XyzLDzzxzULzzxzCoordinate operator /(XyzLDzzxzULzzxzCoordinate a, XyzLDzzxzULzzxzCoordinate b)
        //{
        //    return new XyzLDzzxzULzzxzCoordinate(a.U / b.U, a.V / b.V, a.W / b.W, a.EZ / b.EZ, a.X / b.X, a.Y / b.Y, a.VZ / b.VZ);
        //}

        //public static XyzLDzzxzULzzxzCoordinate operator /(XyzLDzzxzULzzxzCoordinate a, XyCoordinate b)
        //{
        //    return new XyzLDzzxzULzzxzCoordinate(a.U, a.V, a.W, a.EZ, a.X / b.X, a.Y / b.Y, a.VZ);
        //}

        //public static XyzLDzzxzULzzxzCoordinate operator /(XyCoordinate a, XyzLDzzxzULzzxzCoordinate b)
        //{
        //    return new XyzLDzzxzULzzxzCoordinate(b.U, b.V, b.W, b.EZ, a.X / b.X, a.Y / b.Y, b.VZ);
        //}

        //public static UvwxyzzCoordinate operator /(UvwxyzzCoordinate a, XytCoordinate b)
        //{
        //    return new UvwxyzzCoordinate(a.X / b.X, a.Y / b.Y, a.IZ, a.SZ, a.T / b.T);
        //}

        //public static UvwxyzzCoordinate operator /(XytCoordinate a, UvwxyzzCoordinate b)
        //{
        //    return new UvwxyzzCoordinate(a.X / b.X, a.Y / b.Y, b.IZ, b.SZ, a.T / b.T);
        //}

        //public static XyzLDzzxzULzzxzCoordinate operator /(XyzLDzzxzULzzxzCoordinate a, XyzCoordinate b)
        //{
        //    return new XyzLDzzxzULzzxzCoordinate(a.U, a.V, a.W, a.EZ, a.X / b.X, a.Y / b.Y, a.VZ / b.Z);
        //}

        //public static XyzLDzzxzULzzxzCoordinate operator /(XyzCoordinate a, XyzLDzzxzULzzxzCoordinate b)
        //{
        //    return new XyzLDzzxzULzzxzCoordinate(b.U, b.V, b.W, b.EZ, a.X / b.X, a.Y / b.Y, a.Z / b.VZ);
        //}

        //public static XyzLDzzxzULzzxzCoordinate operator /(XyzLDzzxzULzzxzCoordinate a, SizeD b)
        //{
        //    return new XyzLDzzxzULzzxzCoordinate(a.U, a.V, a.W, a.EZ, a.X / b.Width, a.Y / b.Height, a.VZ);
        //}

        //public static XyzLDzzxzULzzxzCoordinate operator /(SizeD a, XyzLDzzxzULzzxzCoordinate b)
        //{
        //    return new XyzLDzzxzULzzxzCoordinate(b.U, b.V, b.W, b.EZ, a.Width / b.X, a.Height / b.Y, b.VZ);
        //}

        //public static XyzLDzzxzULzzxzCoordinate operator /(XyzLDzzxzULzzxzCoordinate a, PointD b)
        //{
        //    return new XyzLDzzxzULzzxzCoordinate(a.U, a.V, a.W, a.EZ, a.X / b.X, a.Y / b.Y, a.VZ);
        //}

        //public static XyzLDzzxzULzzxzCoordinate operator /(PointD a, XyzLDzzxzULzzxzCoordinate b)
        //{
        //    return new XyzLDzzxzULzzxzCoordinate(b.U, b.V, b.W, b.EZ, a.X / b.X, a.Y / b.Y, b.VZ);
        //}

        //public static explicit operator XyzLDzzxzULzzxzCoordinate(XyCoordinate a)
        //{
        //    return new XyzLDzzxzULzzxzCoordinate(0.0, 0.0, 0.0, 0.0, a.X, a.Y, 0.0);
        //}

        //public static explicit operator XyzLDzzxzULzzxzCoordinate(XyzCoordinate a)
        //{
        //    return new XyzLDzzxzULzzxzCoordinate(0.0, 0.0, 0.0, 0.0, a.X, a.Y, a.Z);
        //}

        //public static explicit operator UvwxyzzCoordinate(XytCoordinate a)
        //{
        //    return new UvwxyzzCoordinate(a.X, a.Y, 0.0, 0.0, a.T);
        //}

        //public static explicit operator XyzLDzzxzULzzxzCoordinate(PointD a)
        //{
        //    return new XyzLDzzxzULzzxzCoordinate(0.0, 0.0, 0.0, 0.0, a.X, a.Y, 0.0);
        //}
        #endregion

        #region Object Members
        public override bool Equals(object obj)
        {
            if (obj == null) return false;

            if (obj is XyzLDzzxzULzzxzCoordinate == false) return false;
            XyzLDzzxzULzzxzCoordinate value = (XyzLDzzxzULzzxzCoordinate)obj;

            return this.Equals(value);
        }

        public override int GetHashCode()
        {
            //return this.U.GetHashCode() ^ this.V.GetHashCode() ^ this.W.GetHashCode() ^ this.EZ.GetHashCode() ^ this.X.GetHashCode() ^ this.Y.GetHashCode() ^ this.VZ.GetHashCode();
            return this.X.GetHashCode() ^ this.Y.GetHashCode() ^ this.Z.GetHashCode() ^ this.LD_SZ0.GetHashCode() ^ this.LD_SZ1.GetHashCode() ^ this.LD_TRX.GetHashCode() ^ this.LD_TRZ.GetHashCode() ^
                                                                                        this.UL_SZ0.GetHashCode() ^ this.UL_SZ1.GetHashCode() ^ this.UL_TRX.GetHashCode() ^ this.UL_TRZ.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("[{0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}]", this.X, this.Y, this.Z, this.LD_SZ0, this.LD_SZ1, this.LD_TRX, this.LD_TRZ, this.UL_SZ0, this.UL_SZ1, this.UL_TRX, this.UL_TRZ);
        }
        #endregion
    }
    #endregion

    #region XyzLDzzxzULzzxzCoordinate Collection
    [Serializable]
    public class XyzLDzzxzULzzxzCoordinateCollection : Collection<XyzLDzzxzULzzxzCoordinate>
    {
        #region Constructor
        public XyzLDzzxzULzzxzCoordinateCollection(IList<XyzLDzzxzULzzxzCoordinate> list) : base(list) { }
        public XyzLDzzxzULzzxzCoordinateCollection() : base() { }
        #endregion
    }
    #endregion
}
