using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.Design.Serialization;
using System.Reflection;
using System.Collections;


namespace QMC.Common
{
    [Serializable]
    [TypeConverter(typeof(RangeIConverter))]
    public struct RangeI
    {
        #region Field
        private const string XmlElementNameMinimum = "Minimum";
        private const string XmlElementNameMaximum = "Maximum";

        public static readonly RangeI Empty;

        private int m_Minimum;
        private int m_Maximum;
        #endregion

        #region Constructor
        public RangeI(int minimum, int maximum)
        {
            if (maximum < minimum)
                throw new ArgumentOutOfRangeException("maximum", "Maximum is less than minimum");

            this.m_Minimum = minimum;
            this.m_Maximum = maximum;
        }

        static RangeI()
        {
            RangeI.Empty = new RangeI(0, 0);
        }
        #endregion

        #region Property
        [DefaultValue(0)]
        public int Minimum
        {
            get { return this.m_Minimum; }
            set { this.m_Minimum = value; }
        }

        [DefaultValue(0)]
        public int Maximum
        {
            get { return this.m_Maximum; }
            set { this.m_Maximum = value; }
        }

        [Browsable(false)]
        public bool IsEmpty
        {
            get { return this == RangeI.Empty; }
        }
        #endregion

        #region Method
        public bool Equals(RangeI value)
        {
            return this == value;
        }

        public bool Contains(int value)
        {
            return this.Minimum <= value && value <= Maximum;
        }
        #endregion

        #region Static Member
        public static RangeI Parse(string text)
        {
            int minimum, maximum;
            string[] token = null;

            token = text.Split(',');

            minimum = int.Parse(token[0].Trim('(', ' ', ')'));
            maximum = int.Parse(token[1].Trim('(', ' ', ')'));

            return new RangeI(minimum, maximum);
        }
        #endregion

        #region Object Members
        public override bool Equals(object obj)
        {
            if (obj == null) return false;

            if (obj is RangeI == false) return false;
            RangeI value = (RangeI)obj;

            return this.Equals(value);
        }

        public override int GetHashCode()
        {
            return this.Minimum.GetHashCode() ^ this.Maximum.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("{0}, {1}", this.Minimum, this.Maximum);
        }
        #endregion

        #region Operator
        public static bool operator ==(RangeI a, RangeI b)
        {
            return a.Minimum == b.Minimum && a.Maximum == b.Maximum;
        }

        public static bool operator !=(RangeI a, RangeI b)
        {
            return !(a == b);
        }

        //public static implicit operator RangeI(string text)
        //{
        //    return RangeI.Parse(text);
        //}
        #endregion
    }

    public class RangeIConverter : ExpandableObjectConverter
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
                RangeI range = new RangeI();
                range.Minimum = int.Parse(array[0]);
                range.Maximum = int.Parse(array[1]);

                return range;
            }
            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string) && value is RangeI)
            {
                RangeI range = (RangeI)value;
                return string.Format("{0}, {1}",
                    range.Minimum,
                    range.Maximum
                    );
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
        //protected override ObjectCollection CreateInstanceDescriptor(object value)
        //{
        //    ObjectCollection constructMember = new ObjectCollection();
        //    constructMember.Add(this.GetPropertyValue(value, "Minimum"));
        //    constructMember.Add(this.GetPropertyValue(value, "Maximum"));
        //    return constructMember;
        //}

        //protected override StringCollection GetListProperties()
        //{
        //    StringCollection propertyList = new StringCollection();
        //    propertyList.Add("Minimum");
        //    propertyList.Add("Maximum");

        //    return propertyList;
        //}
    }

    [Serializable]
    public class RangeICollection : Collection<RangeI>
    {
        #region Constructor
        public RangeICollection() : base() { }
        public RangeICollection(IList<RangeI> list) : base(list) { }
        #endregion
    }

    [Serializable]
    [TypeConverter(typeof(RangeDConverter))]
    public struct RangeD
    {
        #region Field
        private const string XmlElementNameMinimum = "Minimum";
        private const string XmlElementNameMaximum = "Maximum";

        public static readonly RangeD Empty;

        private double m_Minimum;
        private double m_Maximum;
        #endregion

        #region Constructor
        public RangeD(double minimum, double maximum)
        {
            if (maximum < minimum)
                throw new ArgumentOutOfRangeException("maximum", "Maximum is less than minimum");

            this.m_Minimum = minimum;
            this.m_Maximum = maximum;
        }

        static RangeD()
        {
            RangeD.Empty = new RangeD(0, 0);
        }
        #endregion

        #region Property
        [DefaultValue(0D)]
        public double Minimum
        {
            get { return this.m_Minimum; }
            set { this.m_Minimum = value; }
        }

        [DefaultValue(0D)]
        public double Maximum
        {
            get { return this.m_Maximum; }
            set { this.m_Maximum = value; }
        }

        [Browsable(false)]
        public bool IsEmpty
        {
            get { return this == RangeD.Empty; }
        }

        [Browsable(false)]
        public double Middle
        {
            get { return (this.Maximum + this.Minimum) / 2; }
        }
        #endregion

        #region Method
        public bool Equals(RangeD value)
        {
            return this == value;
        }

        public bool Contains(double value)
        {
            return this.Minimum <= value && value <= Maximum;
        }
        #endregion

        #region Static Member
        public static RangeD Parse(string text)
        {
            double minimum, maximum;
            string[] token = null;

            token = text.Split(',');

            minimum = double.Parse(token[0].Trim('(', ' ', ')'));
            maximum = double.Parse(token[1].Trim('(', ' ', ')'));

            return new RangeD(minimum, maximum);
        }
        #endregion

        #region Object Members
        public override bool Equals(object obj)
        {
            if (obj == null) return false;

            if (obj is RangeD == false) return false;
            RangeD value = (RangeD)obj;

            return this.Equals(value);
        }

        public override int GetHashCode()
        {
            return this.Minimum.GetHashCode() ^ this.Maximum.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("{0}, {1}", this.Minimum, this.Maximum);
        }
        #endregion

        #region Operator
        public static bool operator ==(RangeD a, RangeD b)
        {
            return a.Minimum == b.Minimum && a.Maximum == b.Maximum;
        }

        public static bool operator !=(RangeD a, RangeD b)
        {
            return !(a == b);
        }

        public static RangeD operator +(RangeD a, int b)
        {
            return new RangeD(a.Minimum + b, a.Maximum + b);
        }

        public static RangeD operator +(RangeD a, double b)
        {
            return new RangeD(a.Minimum + b, a.Maximum + b);
        }

        public static RangeD operator +(RangeD a, RangeD b)
        {
            return new RangeD(a.Minimum + b.Minimum, a.Maximum + b.Maximum);
        }

        public static RangeD operator +(RangeD a, RangeI b)
        {
            return new RangeD(a.Minimum + b.Minimum, a.Maximum + b.Maximum);
        }

        public static RangeD operator -(RangeD a, int b)
        {
            return new RangeD(a.Minimum - b, a.Maximum - b);
        }

        public static RangeD operator -(RangeD a, double b)
        {
            return new RangeD(a.Minimum - b, a.Maximum - b);
        }

        public static RangeD operator -(RangeD a, RangeD b)
        {
            return new RangeD(a.Minimum - b.Minimum, a.Maximum - b.Maximum);
        }

        public static RangeD operator -(RangeD a, RangeI b)
        {
            return new RangeD(a.Minimum - b.Minimum, a.Maximum - b.Maximum);
        }

        public static RangeD operator *(RangeD a, int b)
        {
            return new RangeD(a.Minimum * b, a.Maximum * b);
        }

        public static RangeD operator *(RangeD a, double b)
        {
            return new RangeD(a.Minimum * b, a.Maximum * b);
        }

        public static RangeD operator *(RangeD a, RangeD b)
        {
            return new RangeD(a.Minimum * b.Minimum, a.Maximum * b.Maximum);
        }

        public static RangeD operator *(RangeD a, RangeI b)
        {
            return new RangeD(a.Minimum * b.Minimum, a.Maximum * b.Maximum);
        }

        public static RangeD operator /(RangeD a, int b)
        {
            return new RangeD(a.Minimum / b, a.Maximum / b);
        }

        public static RangeD operator /(RangeD a, double b)
        {
            return new RangeD(a.Minimum / b, a.Maximum / b);
        }
        public static RangeD operator /(RangeD a, RangeD b)
        {
            return new RangeD(a.Minimum / b.Minimum, a.Maximum / b.Maximum);
        }

        public static RangeD operator /(RangeD a, RangeI b)
        {
            return new RangeD(a.Minimum / b.Minimum, a.Maximum / b.Maximum);
        }

        public static implicit operator RangeD(string text)
        {
            return RangeD.Parse(text);
        }
        #endregion
    }

    public class RangeDConverter : ExpandableObjectConverter
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
                RangeD range = new RangeD();
                range.Minimum = double.Parse(array[0]);
                range.Maximum = double.Parse(array[1]);

                return range;
            }
            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string) && value is RangeI)
            {
                RangeD range = (RangeD)value;
                return string.Format("{0}, {1}",
                    range.Minimum,
                    range.Maximum
                    );
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
        //protected override ObjectCollection CreateInstanceDescriptor(object value)
        //{
        //    ObjectCollection constructMember = new ObjectCollection();
        //    constructMember.Add(this.GetPropertyValue(value, "Minimum"));
        //    constructMember.Add(this.GetPropertyValue(value, "Maximum"));
        //    return constructMember;
        //}
        //protected override StringCollection GetListProperties()
        //{
        //    StringCollection propertyList = new StringCollection();
        //    propertyList.Add("Minimum");
        //    propertyList.Add("Maximum");

        //    return propertyList;
        //}
    }

    [Serializable]
    public class RangeDCollection : Collection<RangeD>
    {
        #region Constructor
        public RangeDCollection() : base() { }
        public RangeDCollection(IList<RangeD> list) : base(list) { }
        #endregion
    }

    [Serializable]
    public class RangeDReadOnlyCollection : ReadOnlyCollection<RangeD>
    {
        #region Constructor
        public RangeDReadOnlyCollection(IList<RangeD> list) : base(list) { }
        public RangeDReadOnlyCollection(RangeD range) : this(new RangeD[] { range }) { }
        public RangeDReadOnlyCollection() : this(new RangeD[0]) { }
        #endregion
    }
}
