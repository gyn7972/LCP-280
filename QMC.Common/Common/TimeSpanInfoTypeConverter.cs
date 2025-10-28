using System;
using System.ComponentModel;
using System.Globalization;

namespace QMC.Common
{
	public class TimeSpanInfoTypeConverter : ExpandableObjectConverter
	{
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
		{
			if (sourceType == typeof(string))
			{
				return true;
			}
			return base.CanConvertFrom(context, sourceType);
		}

		/// <summary>
		/// 재정의 되었습니다.
		/// </summary>
		/// <param name="context">An System.ComponentModel.ITypeDescriptorContext that provides a format context.</param>
		/// <param name="destinationType">A System.Type that represents the type you want to convert to.</param>
		/// <returns>true if this converter can perform the conversion; otherwise, false.</returns>
		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
		{
			if (destinationType == typeof(string))
			{
				return true;
			}
			return base.CanConvertTo(context, destinationType);
		}

		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			if (value != null)
			{
				return TimeSpanInfo.Parse(value as string);
			}
			
			return base.ConvertFrom(context, culture, value);
		}

		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if (value is TimeSpanInfo)
			{
				return value.ToString();
			}
			return base.ConvertTo(context, culture, value, destinationType);
		}
	}
}
