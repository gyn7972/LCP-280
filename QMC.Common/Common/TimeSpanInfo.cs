using System;
using System.ComponentModel;

namespace QMC.Common
{
	[Serializable]
	[TypeConverter(typeof(TimeSpanInfoTypeConverter))]
	public struct TimeSpanInfo
	{
		public static readonly TimeSpanInfo MaxValue;

		public static readonly TimeSpanInfo MinValue;

		public static readonly TimeSpanInfo Zero;

		private int m_Days;

		private int m_Hours;

		private int m_Minutes;

		private int m_Seconds;

		private int m_Milliseconds;

		/// <summary>
		/// 일 구성 요소의 값을 가져오거나 설정합니다.
		/// </summary>
		[DefaultValue(0)]
		public int Days
		{
			get
			{
				return m_Days;
			}
			set
			{
				m_Days = value;
			}
		}

		/// <summary>
		/// 시 구성 요소의 값을 가져오거나 설정합니다.
		/// </summary>
		[DefaultValue(0)]
		public int Hours
		{
			get
			{
				return m_Hours;
			}
			set
			{
				m_Hours = value;
			}
		}

		/// <summary>
		/// 분 구성 요소의 값을 가져오거나 설정합니다.
		/// </summary>
		[DefaultValue(0)]
		public int Minutes
		{
			get
			{
				return m_Minutes;
			}
			set
			{
				m_Minutes = value;
			}
		}

		/// <summary>
		/// 초 구성 요소의 값을 가져오거나 설정합니다.
		/// </summary>
		[DefaultValue(0)]
		public int Seconds
		{
			get
			{
				return m_Seconds;
			}
			set
			{
				m_Seconds = value;
			}
		}

		/// <summary>
		/// 밀리 초 구성 요소의 값을 가져오거나 설정합니다.
		/// </summary>
		[DefaultValue(0)]
		public int Milliseconds
		{
			get
			{
				return m_Milliseconds;
			}
			set
			{
				m_Milliseconds = value;
			}
		}

		/// <summary>
		/// 전체 시간을 일로 환산하여 반환합니다.
		/// </summary>
		[Browsable(false)]
		public double TotalDays => ToTimeSpan().TotalDays;

		/// <summary>
		/// 전체 시간을 시로 환산하여 반환합니다.
		/// </summary>
		[Browsable(false)]
		public double TotalHours => ToTimeSpan().TotalHours;

		/// <summary>
		/// 전체 시간을 분으로 환산하여 반환합니다.
		/// </summary>
		[Browsable(false)]
		public double TotalMinutes => ToTimeSpan().TotalMinutes;

		/// <summary>
		/// 전체 시간을 초로 환산하여 반환합니다.
		/// </summary>
		[Browsable(false)]
		public double TotalSeconds => ToTimeSpan().TotalSeconds;

		/// <summary>
		/// 전체 시간을 밀리 초로 환산하여 반환합니다.
		/// </summary>
		[Browsable(false)]
		public double TotalMilliseconds => ToTimeSpan().TotalMilliseconds;

		/// <summary>
		/// 전체 시간을 Tick 수로 환산하여 반환합니다.
		/// </summary>
		[Browsable(false)]
		public long Ticks => ToTimeSpan().Ticks;

		/// <summary>
		/// TimeSpanInfo 클래스의 새 인스턴스를 초기화합니다.
		/// </summary>
		/// <param name="days">일 구성 요소의 값입니다.</param>
		/// <param name="hours">시 구성 요소의 값입니다.</param>
		/// <param name="minutes">분 구성 요소의 값입니다.</param>
		/// <param name="seconds">초 구성 요소의 값입니다.</param>
		/// <param name="milliseconds">밀리 초 구성 요소의 값입니다.</param>
		public TimeSpanInfo(int days, int hours, int minutes, int seconds, int milliseconds)
		{
			m_Days = days;
			m_Hours = hours;
			m_Minutes = minutes;
			m_Seconds = seconds;
			m_Milliseconds = milliseconds;
		}

		/// <summary>
		/// TimeSpanInfo 클래스의 새 인스턴스를 초기화합니다.
		/// </summary>
		/// <param name="days">일 구성 요소의 값입니다.</param>
		/// <param name="hours">시 구성 요소의 값입니다.</param>
		/// <param name="minutes">분 구성 요소의 값입니다.</param>
		/// <param name="seconds">초 구성 요소의 값입니다.</param>
		public TimeSpanInfo(int days, int hours, int minutes, int seconds)
			: this(days, hours, minutes, seconds, 0)
		{
		}

		/// <summary>
		/// TimeSpanInfo 클래스의 새 인스턴스를 초기화합니다.
		/// </summary>
		/// <param name="hours">시 구성 요소의 값입니다.</param>
		/// <param name="minutes">분 구성 요소의 값입니다.</param>
		/// <param name="seconds">초 구성 요소의 값입니다.</param>
		public TimeSpanInfo(int hours, int minutes, int seconds)
			: this(0, hours, minutes, seconds, 0)
		{
		}

		/// <summary>
		/// TimeSpanInfo 클래스의 새 인스턴스를 초기화합니다.
		/// </summary>
		/// <param name="timeSpan">TimeSpan 개체입니다.</param>
		public TimeSpanInfo(TimeSpan timeSpan)
			: this(timeSpan.Days, timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds, timeSpan.Milliseconds)
		{
		}

		static TimeSpanInfo()
		{
			MaxValue = new TimeSpanInfo(TimeSpan.MaxValue);
			MinValue = new TimeSpanInfo(TimeSpan.MinValue);
			Zero = new TimeSpanInfo(TimeSpan.Zero);
		}

		/// <summary>
		/// 지정된 TimeSpan을 사용해서 TimeSpanInfo 개체를 생성합니다.
		/// </summary>
		/// <param name="timeSpan">TimeSpan 개체입니다.</param>
		/// <returns>새로 생성된 TimeSpanInfo 개체입니다.</returns>
		public static TimeSpanInfo FromTimeSpan(TimeSpan timeSpan)
		{
			return new TimeSpanInfo(timeSpan);
		}

		/// <summary>
		/// 지정된 일을 사용해서 TimeSpanInfo 개체를 생성합니다.
		/// </summary>
		/// <param name="value">일을 지정합니다.</param>
		/// <returns>새로 생성된 TimeSpanInfo 개체입니다.</returns>
		public static TimeSpanInfo FromDays(double value)
		{
			return FromTimeSpan(TimeSpan.FromDays(value));
		}

		/// <summary>
		/// 지정된 시를 사용해서 TimeSpanInfo 개체를 생성합니다.
		/// </summary>
		/// <param name="value">시를 지정합니다.</param>
		/// <returns>새로 생성된 TimeSpanInfo 개체입니다.</returns>
		public static TimeSpanInfo FromHours(double value)
		{
			return FromTimeSpan(TimeSpan.FromHours(value));
		}

		/// <summary>
		/// 지정된 분을 사용해서 TimeSpanInfo 개체를 생성합니다.
		/// </summary>
		/// <param name="value">분을 지정합니다.</param>
		/// <returns>새로 생성된 TimeSpanInfo 개체입니다.</returns>
		public static TimeSpanInfo FromMinutes(double value)
		{
			return FromTimeSpan(TimeSpan.FromMinutes(value));
		}

		/// <summary>
		/// 지정된 초를 사용해서 TimeSpanInfo 개체를 생성합니다.
		/// </summary>
		/// <param name="value">초를 지정합니다.</param>
		/// <returns>새로 생성된 TimeSpanInfo 개체입니다.</returns>
		public static TimeSpanInfo FromSeconds(double value)
		{
			return FromTimeSpan(TimeSpan.FromSeconds(value));
		}

		/// <summary>
		/// 지정된 밀리 초를 사용해서 TimeSpanInfo 개체를 생성합니다.
		/// </summary>
		/// <param name="value">밀리 초를 지정합니다.</param>
		/// <returns>새로 생성된 TimeSpanInfo 개체입니다.</returns>
		public static TimeSpanInfo FromMilliseconds(double value)
		{
			return FromTimeSpan(TimeSpan.FromMilliseconds(value));
		}

		/// <summary>
		/// 지정된 Tick 수를 사용해서 TimeSpanInfo 개체를 생성합니다.
		/// </summary>
		/// <param name="value">Tick 수를 지정합니다.</param>
		/// <returns>새로 생성된 TimeSpanInfo 개체입니다.</returns>
		public static TimeSpanInfo FromTicks(long value)
		{
			return FromTimeSpan(TimeSpan.FromTicks(value));
		}

		/// <summary>
		/// 지정된 문자열을 TimeSpanInfo 개체로 변환합니다.
		/// </summary>
		/// <param name="s">TimeSpan을 나타내는 문자열입니다.</param>
		/// <returns>변환된 TimeSpanInfo 개체입니다.</returns>
		public static TimeSpanInfo Parse(string s)
		{
			return FromTimeSpan(TimeSpan.Parse(s));
		}

		/// <summary>
		/// 지정된 문자열을 TimeSpanInfo 개체로 변환합니다.
		/// </summary>
		/// <param name="s">TimeSpan을 나타내는 문자열입니다.</param>
		/// <param name="result">변환된 TimeSpanInfo 개체입니다.</param>
		/// <returns>성공하면 true이고, 그렇지 않으면 false입니다.</returns>
		public static bool TryParse(string s, out TimeSpanInfo result)
		{
			result = default(TimeSpanInfo);
			if (!TimeSpan.TryParse(s, out var result2))
			{
				return false;
			}
			result = new TimeSpanInfo(result2);
			return true;
		}

		/// <summary>
		/// TimeSpan 개체로 변환합니다.
		/// </summary>
		/// <returns>변환된 TimeSpan 개체입니다.</returns>
		public TimeSpan ToTimeSpan()
		{
			return new TimeSpan(Days, Hours, Minutes, Seconds, Milliseconds);
		}

		/// <summary>
		/// 재정의 되었습니다.
		/// </summary>
		/// <returns>TimeSpan과 동일한 문자열을 반환합니다.</returns>
		public override string ToString()
		{
			return ToTimeSpan().ToString();
		}

		public static implicit operator TimeSpan(TimeSpanInfo info)
		{
			return info.ToTimeSpan();
		}

		public static implicit operator TimeSpanInfo(TimeSpan timeSpan)
		{
			return FromTimeSpan(timeSpan);
		}
	}
}
