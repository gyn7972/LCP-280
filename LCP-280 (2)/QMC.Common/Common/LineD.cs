using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace QMC.Common
{
    #region LineD
    /// <summary>
    /// 직선에 대해 정의한다.
    /// </summary>
    [Serializable]
    [TypeConverter(typeof(LineDConverter))]
    public struct LineD
    {
        #region Field
        private const string XmlElementNameStart = "Start";
        private const string XmlElementNameEnd = "End";

        public static readonly LineD Empty;
        private double m_Angle;

        private PointD m_Start;
        private PointD m_End;
        #endregion

        #region Constructor
        public LineD(params PointD[] points)
        {
            double angle = 0.0;

            if (points == null)
                throw new ArgumentNullException("points");
            if (points.Length != 2)
                throw new ArgumentOutOfRangeException("points.Length");

            this.m_Start = points[0];
            this.m_End = points[1];

            qGeometry.GetAngle(points[0], points[1], ref angle);

            this.m_Angle = angle;
        }

        public LineD(PointD start, PointD end) : this(new PointD[] { start, end }) { }

        static LineD()
        {
            LineD.Empty = new LineD(new PointD(0, 0), new PointD(0, 0));
        }
        #endregion

        #region Property
        public PointD Start
        {
            get { return this.m_Start; }
            set { this.m_Start = value; }
        }

        public PointD End
        {
            get { return this.m_End; }
            set { this.m_End = value; }
        }

        public double Angle
        {
            get { return this.m_Angle; }
            set { this.m_Angle = value; }
        }

        [Browsable(false)]
        public bool IsEmpty
        {
            get { return this == LineD.Empty; }
        }
        #endregion

        #region Method
        public bool Equals(LineD value) 
        {
            return this == value;
        }
        #endregion

        #region Static Members
        public static LineD Parse(string text)
        {
            PointD[] points = new PointD[4];

            string[] token = null;
            double x = 0.0, y = 0.0;

            if (string.IsNullOrEmpty(text) == true)
                return new LineD(new PointD(0,0), new PointD(0,0));

            token = text.Split(',');

            for (int i = 0; i < points.Length; i++)
            {
                x = double.Parse(token[i * 2].Trim('(', ' ', ')'));
                y = double.Parse(token[i * 2 + 1].Trim('(', ' ', ')'));

                points[i] = new PointD(x, y);
            }

            return new LineD(points);
        }

        /// <summary>
        /// 주어진 두개의 직선의 교차점이 있는지를 반환한다.
        /// </summary>
        /// <param name="sourceStart">Source 직선의 시작 지점</param>
        /// <param name="sourceEnd">Source 직선의 끝 지점</param>
        /// <param name="targetStart">Target 직선의 시작 지점</param>
        /// <param name="targetEnd">Target 직선의 끝 지점</param>
        /// <param name="crossingPoint">두 직선의 교차점</param>
        /// <returns>
        /// 교차점이 있는 경우는 true,
        /// 없는 경우는 false를 반환한다.
        /// </returns>
        public static bool GetIntersectPoint(PointD sourceStart, PointD sourceEnd, PointD targetStart, PointD targetEnd, out PointD crossingPoint)
        {
            double targetMolecule = 0;
            double sourceMolecule = 0;
            double target = 0;
            double source = 0;

            // Target 및 Source 공통 분모
            double denominator = (targetEnd.Y - targetStart.Y) * (sourceEnd.X - sourceStart.X) - (targetEnd.X - targetStart.X) * (sourceEnd.Y - sourceStart.Y);

            crossingPoint = new PointD();
            // denominator = 0일 경우,두 직선이 평행 또는 일치하는지 판단.
            if (denominator == 0) return false;

            targetMolecule = (targetEnd.X - targetStart.X) * (sourceStart.Y - targetStart.Y) - (targetEnd.Y - targetStart.Y) * (sourceStart.X - targetStart.X);
            sourceMolecule = (sourceEnd.X - sourceStart.X) * (sourceStart.Y - targetStart.Y) - (sourceEnd.Y - sourceStart.Y) * (sourceStart.X - targetStart.X);

            target = targetMolecule / denominator;
            source = sourceMolecule / denominator;

            if (target < 0.0 || target > 1.0 || source < 0.0 || source > 1.0) return false;
            if (targetMolecule == 0 && sourceMolecule == 0) return false;

            crossingPoint = new PointD(sourceStart.X + target * (double)(sourceEnd.X - sourceStart.X), sourceStart.Y + target * (double)(sourceEnd.Y - sourceStart.Y));

            return true;
        }

        /// <summary>
        /// 주어진 두개의 직선의 교차점이 있는지를 반환한다.
        /// </summary>
        /// <param name="source">Source 직선</param>
        /// <param name="target">Target 직선</param>
        /// <param name="crossingPoint">두 직선의 교차점</param>
        /// <returns>
        /// 교차점이 있는 경우는 true,
        /// 없는 경우는 false를 반환한다.
        /// </returns>
        public static bool GetIntersectPoint(LineD source, LineD target, out PointD crossingPoint)
        {
            return LineD.GetIntersectPoint(source.Start, source.End, target.Start, target.End, out crossingPoint);
        }
        #endregion

        #region Object Members
        public override bool Equals(object obj)
        {
            if (obj == null) return false;

            if (obj is LineD == false) return false;
            LineD value = (LineD)obj;

            return this.Equals(value);
        }

        public override int GetHashCode()
        {
            return this.Start.GetHashCode() ^ this.End.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("({0}),({1}),({2})", this.Start, this.End, this.Angle);
        }
        #endregion

        #region Operator
        public static bool operator ==(LineD left, LineD right)
        {
            return left.Start == right.Start && left.End == right.End;
        }

        public static bool operator !=(LineD left, LineD right)
        {
            return !(left == right);
        }

        public static LineD operator +(LineD left, LineD right)
        {
            return new LineD(left.Start + right.Start, left.End + right.End);
        }

        public static LineD operator -(LineD left, LineD right)
        {
            return new LineD(left.Start - right.Start, left.End - right.End);
        }

        public static LineD operator *(LineD left, LineD right)
        {
            return new LineD(left.Start * right.Start, left.End * right.End);
        }

        public static LineD operator /(LineD left, LineD right)
        {
            return new LineD(left.Start / right.Start, left.End / right.End);
        }

        public static LineD operator /(LineD left, double value)
        {
            return new LineD(left.Start / value, left.End / value);
        }

        //public static implicit operator LineD(string text)
        //{
        //    return LineD.Parse(text);
        //}
        #endregion
    }
    #endregion

    #region LineDConverter
    [Serializable]
    public class LineDConverter : ExpandableObjectConverter
    {

        #region ExpandableObjectConverter Members
        //protected override ObjectCollection CreateInstanceDescriptor(object value)
        //{
        //    ObjectCollection constructMember = new ObjectCollection();
        //    constructMember.Add(this.GetPropertyValue(value, "Start"));
        //    constructMember.Add(this.GetPropertyValue(value, "End"));
        //    return constructMember;
        //}
        //protected override StringCollection GetListProperties()
        //{
        //    StringCollection propertyList = new StringCollection();
        //    propertyList.Add("Start");
        //    propertyList.Add("End");
        //    return propertyList;
        //}
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
                LineD line = new LineD();
                PointD start = new PointD();
                PointD end = new PointD();
                start.X = double.Parse(array[0]);
                start.Y = double.Parse(array[1]);
                end.X = double.Parse(array[2]);
                end.Y = double.Parse(array[3]);
                line.Start = start;
                line.End = end;
                line.Angle = double.Parse(array[4]);

                return line;
            }
            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string) && value is PointD)
            {
                LineD line = (LineD)value;
                return string.Format("{0}, {1}, {2}, {3}, {4}",
                    line.Start.X, line.Start.Y,
                    line.End.X, line.End.Y,
                    line.Angle
                    );
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
        #endregion
    }
    #endregion

    #region LineDCollection
    [Serializable]
    public class LineDCollection : Collection<LineD>
    {
        #region Constructor
        public LineDCollection() : base() { }
        public LineDCollection(IList<LineD> list) : base(list) { }
        #endregion
    }
    #endregion
}