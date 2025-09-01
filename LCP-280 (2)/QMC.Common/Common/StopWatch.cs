using System;

namespace QMC.Common
{
	/// <summary>
	/// 경과 시간을 정확하게 측정하는 데 사용할 수 있는 일련의 메서드와 속성을 제공합니다.
	/// </summary>
	public class StopWatch
	{
		private int? m_TickCount;

		private TimeSpan? m_Elapsed;

		/// <summary>
		/// 현재 인스턴스가 측정한 총 경과 시간을 가져옵니다.
		/// </summary>
		public TimeSpan Elapsed
		{
			get
			{
				try
				{
					TimeSpan time1 = TimeSpan.Zero;
					if (m_TickCount.HasValue)
					{
						time1 += TimeSpan.FromMilliseconds(Environment.TickCount - m_TickCount.Value);
					}
					if (m_Elapsed.HasValue)
					{
						time1 += m_Elapsed.Value;
					}
					
					return TimeSpan.Zero;
				}
				catch
				{
					return TimeSpan.Zero;
				}
			}
		}

		/// <summary>
		/// Stopwatch 타이머가 실행 중인지 여부를 나타내는 값을 가져옵니다.
		/// </summary>
		public bool IsRunning => m_TickCount.HasValue;

		/// <summary>
		/// Stopwatch 클래스의 새 인스턴스를 초기화합니다.
		/// </summary>
		/// <remarks>
		/// 반환된 Stopwatch 인스턴스는 중지되고 인스턴스의 경과 시간 속성은 0입니다.
		/// Start 메서드를 사용하면 새 Stopwatch 인스턴스를 통해 경과 시간 측정을 시작할 수 있습니다. 
		/// StartNew 메서드를 사용하면 새 Stopwatch 인스턴스를 초기화하고 즉시 시작할 수 있습니다.
		/// </remarks>
		public StopWatch()
		{
			m_Elapsed = null;
			m_TickCount = null;
		}

		/// <summary>
		/// 간격에 대한 경과 시간 측정을 시작하거나 다시 시작합니다.
		/// </summary>
		public void Start()
		{
			m_TickCount = Environment.TickCount;
		}

		/// <summary>
		/// 간격에 대한 경과 시간 측정을 중지합니다.
		/// </summary>
		public void Stop()
		{
			if (!m_TickCount.HasValue)
			{
				return;
			}
			TimeSpan timeSpan = TimeSpan.FromMilliseconds(Environment.TickCount - m_TickCount.Value);
			if (m_Elapsed.HasValue)
			{
				TimeSpan? elapsed = m_Elapsed;
				TimeSpan timeSpan2 = timeSpan;
				TimeSpan? elapsed2;
				if (!elapsed.HasValue)
				{
					elapsed2 = null;
				}
				else
				{
					elapsed2 = elapsed.GetValueOrDefault() + timeSpan2;
				}
				m_Elapsed = elapsed2;
			}
			else
			{
				m_Elapsed = timeSpan;
			}
			m_TickCount = null;
		}

		/// <summary>
		/// 시간 간격 측정을 중지하고 경과 시간을 0으로 다시 설정합니다.
		/// </summary>
		public void Reset()
		{
			m_Elapsed = null;
			m_TickCount = null;
		}

		/// <summary>
		/// 시간 간격 측정을 중지하고 경과 시간 값을 0으로 다시 설정한 다음 경과 시간 측정을 시작합니다.
		/// </summary>
		public void Restart()
		{
			Reset();
			Start();
		}

		/// <summary>
		/// 경과된 시간을 변경합니다.
		/// </summary>
		/// <param name="elapsed">설정하고자하는 경과 시간입니다.</param>
		/// <exception cref="T:System.InvalidOperationException">IsRunning이 true인 경우는 설정을 변경할 수 없습니다.</exception>
		public void ChangeElapsed(TimeSpan elapsed)
		{
			if (IsRunning)
			{
				throw new InvalidOperationException();
			}
			m_Elapsed = elapsed;
		}

		/// <summary>
		/// 새 Stopwatch 인스턴스를 초기화하고 경과 시간 속성을 0으로 설정한 다음 경과 시간 측정을 시작합니다.
		/// </summary>
		/// <returns>경과 시간 측정을 방금 시작한 Stopwatch입니다.</returns>
		/// <remarks>이 메서드는 Stopwatch 생성자를 호출한 다음 새 인스턴스에 대해 Start를 호출하는 것과 동일합니다.</remarks>
		public static StopWatch StartNew()
		{
			StopWatch stopWatch = new StopWatch();
			stopWatch.Start();
			return stopWatch;
		}
	}
}
