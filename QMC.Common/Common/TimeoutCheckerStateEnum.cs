namespace QMC.Common
{
	/// <summary>
	/// TimeoutCheckerState와 관련된 기능을 제공합니다.
	/// </summary>
	public static class TimeoutCheckerStateEnum
	{
		/// <summary>
		/// 주어진 상태가 동작 상태인지를 반환합니다.
		/// </summary>
		/// <param name="state">확인하려는 상태입니다.</param>
		/// <returns>주어진 상태가 동작 상태이면 true 그렇지 않으면 false를 반환합니다.</returns>
		public static bool IsActive(TimeoutCheckerState state)
		{
			int result;
			if (state != TimeoutCheckerState.Running)
			{
				result = ((state == TimeoutCheckerState.Paused) ? 1 : 0);
			}
			else
			{
				result = 1;
			}
			return (byte)result != 0;
		}

		/// <summary>
		/// 주어진 상태가 완료 상태인지를 반환합니다.
		/// </summary>
		/// <param name="state">확인하려는 상태입니다.</param>
		/// <returns>주어진 상태가 완료 상태이면 true 그렇지 않으면 false를 반환합니다.</returns>
		public static bool IsCompleted(TimeoutCheckerState state)
		{
			int result;
			if (state != TimeoutCheckerState.CompletedSuccessfully)
			{
				result = ((state == TimeoutCheckerState.Passed) ? 1 : 0);
			}
			else
			{
				result = 1;
			}
			return (byte)result != 0;
		}
	}
}
