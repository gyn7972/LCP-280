using System;
using System.Drawing;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Reflection;
using System.Collections;

namespace QMC.Common
{
    [Serializable]
    [TypeConverter(typeof(SizeDConverter))]
    public struct SizeD
    {
        public static readonly SizeD Empty;

        #region Field
        private double m_Width;
        private double m_Height;
        #endregion

        #region Constructor
        public SizeD(double width, double height)
        {
            this.m_Width = width;
            this.m_Height = height;
        }

        static SizeD()
        {
            SizeD.Empty = new SizeD(0, 0);
        }
        #endregion

        #region Proprety
        public double Width
        {
            get { return this.m_Width; }
            set { this.m_Width = value; }
        }
        public double Height
        {
            get { return this.m_Height; }
            set { this.m_Height = value; }
        }

        [Browsable(false)]
        public bool IsEmpty
        {
            get { return this == SizeD.Empty; }
        }
        #endregion

        #region Method
        public bool Equals(SizeD value)
        {
            return this == value;
        }
        #endregion

        #region Static Member
        public static SizeD Parse(string text)
        {
            double width, height;
            string[] token = null;

            token = text.Split(',');

            width = double.Parse(token[0].Trim('(', ' ', ')'));
            height = double.Parse(token[1].Trim('(', ' ', ')'));

            return new SizeD(width, height);
        }
        #endregion

        #region Object Members
        public override bool Equals(object obj)
        {
            if (obj == null) return false;

            if (obj is SizeD == false) return false;
            SizeD value = (SizeD)obj;

            return this.Equals(value);
        }

        public override int GetHashCode()
        {
            return this.Width.GetHashCode() ^ this.Height.GetHashCode();
        }
        public override string ToString()
        {
            return string.Format("{0}, {1}", this.Width, this.Height);
        }
        #endregion

        #region Operator
        public static bool operator ==(SizeD left, SizeD right)
        {
            return left.Width == right.Width && left.Height == right.Height;
        }

        public static bool operator !=(SizeD left, SizeD right)
        {
            return !(left == right);
        }

        public static SizeD operator +(SizeD a, SizeD b)
        {
            return new SizeD(a.Width + b.Width, a.Height + b.Height);
        }

        public static SizeD operator +(Size a, SizeD b)
        {
            return new SizeD(a.Width + b.Width, a.Height + b.Height);
        }

        public static SizeD operator +(SizeD a, Size b)
        {
            return new SizeD(a.Width + b.Width, a.Height + b.Height);
        }

        public static SizeD operator +(SizeD a, int b)
        {
            return new SizeD(a.Width + b, a.Height + b);
        }

        public static SizeD operator +(int a, SizeD b)
        {
            return new SizeD(a + b.Width, a + b.Height);
        }

        public static SizeD operator +(SizeD a, double  b)
        {
            return new SizeD(a.Width + b, a.Height + b);
        }

        public static SizeD operator +(double a, SizeD b)
        {
            return new SizeD(a + b.Width, a + b.Height);
        }

        public static SizeD operator -(SizeD a, SizeD b)
        {
            return new SizeD(a.Width - b.Width, a.Height - b.Height);
        }

        public static SizeD operator -(Size a, SizeD b)
        {
            return new SizeD(a.Width - b.Width, a.Height - b.Height);
        }

        public static SizeD operator -(SizeD a, Size b)
        {
            return new SizeD(a.Width - b.Width, a.Height - b.Height);
        }

        public static SizeD operator -(SizeD a, int b)
        {
            return new SizeD(a.Width - b, a.Height - b);
        }

        public static SizeD operator -(int a, SizeD b)
        {
            return new SizeD(a - b.Width, a - b.Height);
        }

        public static SizeD operator -(SizeD a, double b)
        {
            return new SizeD(a.Width - b, a.Height - b);
        }

        public static SizeD operator -(double a, SizeD b)
        {
            return new SizeD(a - b.Width, a - b.Height);
        }

        public static SizeD operator *(SizeD a, SizeD b)
        {
            return new SizeD(a.Width * b.Width, a.Height * b.Height);
        }

        public static SizeD operator *(Size a, SizeD b)
        {
            return new SizeD(a.Width * b.Width, a.Height * b.Height);
        }

        public static SizeD operator *(SizeD a, Size b)
        {
            return new SizeD(a.Width * b.Width, a.Height * b.Height);
        }

        public static SizeD operator *(SizeD a, int b)
        {
            return new SizeD(a.Width * b, a.Height * b);
        }

        public static SizeD operator *(int a, SizeD b)
        {
            return new SizeD(a * b.Width, a * b.Height);
        }

        public static SizeD operator *(SizeD a, double b)
        {
            return new SizeD(a.Width * b, a.Height * b);
        }

        public static SizeD operator *(double a, SizeD b)
        {
            return new SizeD(a * b.Width, a * b.Height);
        }

        public static SizeD operator /(SizeD a, SizeD b)
        {
            return new SizeD(a.Width / b.Width, a.Height / b.Height);
        }

        public static SizeD operator /(Size a, SizeD b)
        {
            return new SizeD(a.Width / b.Width, a.Height / b.Height);
        }

        public static SizeD operator /(SizeD a, Size b)
        {
            return new SizeD(a.Width / b.Width, a.Height / b.Height);
        }

        public static SizeD operator /(SizeD a, int b)
        {
            return new SizeD(a.Width / b, a.Height / b);
        }

        public static SizeD operator /(int a, SizeD b)
        {
            return new SizeD(a / b.Width, a / b.Height);
        }

        public static SizeD operator /(SizeD a, double b)
        {
            return new SizeD(a.Width / b, a.Height / b);
        }

        public static SizeD operator /(double a, SizeD b)
        {
            return new SizeD(a / b.Width, a / b.Height);
        }

        public static explicit operator PointD(SizeD size)
        {
            return new PointD((double)size.Width, (double)size.Height);
        }

        public static implicit operator SizeF(SizeD size)
        {
            return new SizeF((float)size.Width, (float)size.Height);
        }

        public static explicit operator Size(SizeD size)
        {
            return new Size((int) size.Width, (int) size.Height);
        }

        //public static implicit operator Size(SizeD size)
        //{
        //    return new Size((int) size.Width, (int) size.Height);
        //}
        #endregion
    }

    public class SizeDConverter : ExpandableObjectConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                return true;
            }
            return base.CanConvertTo(context, destinationType);
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string))
            {
                return true;
            }
            return base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string)
            {
                string text = value as string;
                string[] array = text.Split(',');
                SizeD size = new SizeD();
                size.Width = double.Parse(array[0]);
                size.Height = double.Parse(array[1]);

                return size;
            }
            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string) && value is SizeD)
            {
                SizeD size = (SizeD)value;
                return string.Format("{0}, {1}",
                    size.Width,
                    size.Height
                    );
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
        //protected override ObjectCollection CreateInstanceDescriptor(object value)
        //{
        //    ObjectCollection constructMember = new ObjectCollection();
        //    constructMember.Add(this.GetPropertyValue(value, "Width"));
        //    constructMember.Add(this.GetPropertyValue(value, "Height"));
        //    return constructMember;
        //}

        //protected override StringCollection GetListProperties()
        //{
        //    StringCollection propertyList = new StringCollection();
        //    propertyList.Add("Width");
        //    propertyList.Add("Height");

        //    return propertyList;
        //}
    }

    [Serializable]
    public class SizeDCollection : Collection<SizeD>
    {
        #region Constructor
        public SizeDCollection() : base() { }
        public SizeDCollection(IList<SizeD> list) : base(list) { }
        #endregion
    }
}
