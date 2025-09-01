using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Threading;

namespace QMC.Common
{
    [Serializable]
    public enum TimeoutCheckerState
    {
        /// <summary>
        /// 동작하기 전의 초기상태를 나타냅니다.
        /// </summary>
        Idle,
        /// <summary>
        /// 동작하고 있는 상태를 나타냅니다.
        /// </summary>
        Running,
        /// <summary>
        /// 일시정지 상태임을 나타냅니다.
        /// </summary>
        Paused,
        /// <summary>
        /// 시간의 경과에 의해 정상적으로 종료되었음을 나타냅니다.
        /// </summary>
        CompletedSuccessfully,
        /// <summary>
        /// 주어진 Interval만큼 시간이 경과하기 전에 강제적으로 종료되었음을 나타냅니다.
        /// </summary>
        Passed
    }
    public class TimeoutChecker
    {
        [Serializable]
        private struct TickSpan
        {
            private int m_From;

            private int m_To;

            public int From
            {
                get
                {
                    return m_From;
                }
                set
                {
                    m_From = value;
                }
            }

            public int To
            {
                get
                {
                    return m_To;
                }
                set
                {
                    m_To = value;
                }
            }

            public TickSpan(int from, int to)
            {
                m_From = from;
                m_To = to;
            }

            public int GetDuration()
            {
                return To - From;
            }

            public TimeSpan GetTimeSpan()
            {
                return TimeSpan.FromMilliseconds(GetDuration());
            }

            public void CopyTo(TickSpan tickSpan)
            {
                tickSpan.From = From;
                tickSpan.To = To;
            }
        }

        [Serializable]
        private class TickSpanCollection : Collection<TickSpan>
        {
            private object SyncRoot => ((ICollection)this).SyncRoot;

            public int GetDuration()
            {
                int num = 0;
                for (int i = 0; i < base.Count; i++)
                {
                    object syncRoot = SyncRoot;
                    bool lockTaken = false;
                    try
                    {
                        Monitor.Enter(syncRoot, ref lockTaken);
                        num += base[i].GetDuration();
                    }
                    finally
                    {
                        if (lockTaken)
                        {
                            Monitor.Exit(syncRoot);
                        }
                    }
                }
                return num;
            }

            protected override void ClearItems()
            {
                object syncRoot = SyncRoot;
                bool lockTaken = false;
                try
                {
                    Monitor.Enter(syncRoot, ref lockTaken);
                    base.ClearItems();
                }
                finally
                {
                    if (lockTaken)
                    {
                        Monitor.Exit(syncRoot);
                    }
                }
            }

            protected override void InsertItem(int index, TickSpan item)
            {
                lock (SyncRoot)
                {
                    base.InsertItem(index, item);
                }
            }

            protected override void RemoveItem(int index)
            {
                object syncRoot = SyncRoot;
                bool lockTaken = false;
                try
                {
                    Monitor.Enter(syncRoot, ref lockTaken);
                    base.RemoveItem(index);
                }
                finally
                {
                    if (lockTaken)
                    {
                        Monitor.Exit(syncRoot);
                    }
                }
            }

            protected override void SetItem(int index, TickSpan item)
            {
                lock (SyncRoot)
                {
                    base.SetItem(index, item);
                }
            }
        }

        private class SynchronizedTimeoutChecker : TimeoutChecker
        {
            public SynchronizedTimeoutChecker(TimeSpan interval, bool autoStart)
                : base(interval, autoStart, synchronized: true)
            {
            }

            public SynchronizedTimeoutChecker(TimeSpan interval)
                : this(interval, autoStart: false)
            {
            }

            public SynchronizedTimeoutChecker(double interval, bool autoStart)
                : this(VerifyInterval(interval), autoStart)
            {
            }

            public SynchronizedTimeoutChecker(double interval)
                : this(interval, autoStart: false)
            {
            }

            public SynchronizedTimeoutChecker(bool autoStart)
                : this(MaxInterval, autoStart)
            {
            }

            public SynchronizedTimeoutChecker()
                : this(autoStart: false)
            {
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            protected override int CheckForWaiting()
            {
                return base.CheckForWaiting();
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            protected override TimeSpan GetElapsed()
            {
                return base.GetElapsed();
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            protected override TimeSpan GetElapsedTotal()
            {
                return base.GetElapsedTotal();
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            protected override bool GetIsActive()
            {
                return base.GetIsActive();
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            protected override bool GetIsCompleted()
            {
                return base.GetIsCompleted();
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            protected override TimeSpan GetRemainder()
            {
                return base.GetRemainder();
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            protected override int OnInitialize()
            {
                return base.OnInitialize();
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            protected override int OnChangeInterval(TimeSpan interval)
            {
                return base.OnChangeInterval(interval);
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            protected override int OnPass()
            {
                return base.OnPass();
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            protected override int OnPause()
            {
                return base.OnPause();
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            protected override int OnReset(TimeSpan interval)
            {
                return base.OnReset(interval);
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            protected override int OnResume()
            {
                return base.OnResume();
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public override string ToString()
            {
                return base.ToString();
            }
        }

        /// <summary>
        /// 설정할 수 있는 최대 시간을 나타냅니다. 이 필드는 읽기 전용 필드입니다.
        /// </summary>
        private static readonly double g_MaxInterval = 2147483647.0;
        public static readonly TimeSpan MaxInterval = TimeSpan.FromMilliseconds(g_MaxInterval);

        private object m_SyncRoot;

        private bool m_IsSynchronized;

        private TimeoutCheckerState m_State;

        private TimeSpan m_Interval;

        private DateTime m_StartTime;

        private DateTime m_EndTime;

        private int m_StartTick;

        private int m_CompletedTick;

        private TimeSpan m_Remainder;

        private TickSpanCollection m_PausedSlots;

        /// <summary>
        /// 액세스를 동기화하는 데 사용할 수 있는 개체를 가져옵니다.
        /// </summary>
        public object SyncRoot => m_SyncRoot;

        /// <summary>
        /// 액세스가 동기화되어 스레드로부터 안전하게 보호되는지 여부를 나타내는 값을 가져옵니다.
        /// </summary>
        public bool IsSynchronized => m_IsSynchronized;

        /// <summary>
        /// 현재 상태를 가져옵니다.
        /// </summary>
        public TimeoutCheckerState State => m_State;

        /// <summary>
        /// 설정된 시간을 가져옵니다.
        /// </summary>
        public TimeSpan Interval => m_Interval;

        /// <summary>
        /// 설정된 시간을 가져옵니다.
        /// </summary>
        public double IntervalMilliseconds => Interval.TotalMilliseconds;

        /// <summary>
        /// 시작 시간을 가져옵니다.
        /// </summary>
        public DateTime StartTime => m_StartTime;

        /// <summary>
        /// 완료 시간을 가져옵니다.
        /// </summary>
        public DateTime EndTime => m_EndTime;

        /// <summary>
        /// 경과된 시간을 가져옵니다.
        /// </summary>
        public TimeSpan Elapsed => GetElapsed();

        /// <summary>
        /// 경과된 시간을 가져옵니다.
        /// </summary>
        public double ElapsedMilliseconds => Elapsed.TotalMilliseconds;

        /// <summary>
        /// 전체 경과된 시간을 가져옵니다.
        /// </summary>
        public TimeSpan ElapsedTotal => GetElapsedTotal();

        /// <summary>
        /// 전체 경과된 시간을 가져옵니다.
        /// </summary>
        public double ElapsedTotalMilliseconds => ElapsedTotal.TotalMilliseconds;

        /// <summary>
        /// 남은 시간을 가져옵니다.
        /// </summary>
        public TimeSpan Remainder => GetRemainder();

        /// <summary>
        /// 남은 시간을 가져옵니다.
        /// </summary>
        public double RemainderMilliseconds => Remainder.TotalMilliseconds;

        /// <summary>
        /// 완료되었는지 여부를 가져옵니다.
        /// </summary>
        public bool IsCompleted => GetIsCompleted();

        /// <summary>
        /// 동작 상태인지를 가져옵니다.
        /// </summary>
        public bool IsActive => GetIsActive();

        private TimeoutChecker(TimeSpan interval, bool autoStart, bool synchronized)
        {
            m_SyncRoot = new object();
            m_IsSynchronized = synchronized;
            m_State = TimeoutCheckerState.Idle;
            ChangeInterval(interval);
            Initialize();
            if (autoStart)
            {
                Reset();
            }
        }

        /// <summary>
        /// TimeoutChecker 클래스의 새 인스턴스를 초기화합니다.
        /// </summary>
        /// <param name="interval">설정 시간입니다.</param>
        /// <param name="autoStart">자동으로 시작할지 여부입니다.</param>
        public TimeoutChecker(TimeSpan interval, bool autoStart)
            : this(interval, autoStart, synchronized: false)
        {
        }

        /// <summary>
        /// TimeoutChecker 클래스의 새 인스턴스를 초기화합니다.
        /// </summary>
        /// <param name="interval">설정 시간입니다</param>
        public TimeoutChecker(TimeSpan interval)
            : this(interval, autoStart: false)
        {
        }

        /// <summary>
        /// TimeoutChecker 클래스의 새 인스턴스를 초기화합니다.
        /// </summary>
        /// <param name="interval">밀리초의 설정 시간입니다</param>
        /// <param name="autoStart">자동으로 시작할지 여부입니다</param>
        public TimeoutChecker(double interval, bool autoStart)
            : this(VerifyInterval(interval), autoStart)
        {
        }

        /// <summary>
        /// TimeoutChecker 클래스의 새 인스턴스를 초기화합니다.
        /// </summary>
        /// <param name="interval">밀리초의 설정 시간입니다</param>
        public TimeoutChecker(double interval)
            : this(interval, autoStart: false)
        {
        }

        /// <summary>
        /// TimeoutChecker 클래스의 새 인스턴스를 초기화합니다.
        /// </summary>
        /// <param name="autoStart">자동으로 시작할지 여부입니다</param>
        public TimeoutChecker(bool autoStart)
            : this(MaxInterval, autoStart)
        {
        }

        /// <summary>
        /// TimeoutChecker 클래스의 새 인스턴스를 초기화합니다.
        /// </summary>
        public TimeoutChecker()
            : this(autoStart: false)
        {
        }

        private void SetState(TimeoutCheckerState state)
        {
            m_State = state;
        }

        private int OnWaiting()
        {
            while (!IsCompleted)
            {
                Thread.Sleep(1);
            }

            return 0;
        }

        /// <summary>
        /// 동작의 종료 여부를 반환합니다. 현재 상태가 Running 상태이고 시간이 모두 경과하였다면 자동으로 CompletedSuccessfully 상태로 설정됩니다.
        /// </summary>
        /// <returns>동작의 종료 여부입니다.</returns>
        protected virtual bool GetIsCompleted()
        {
            if (TimeoutCheckerStateEnum.IsCompleted(m_State))
            {
                return true;
            }
            if (m_State == TimeoutCheckerState.Running)
            {
                int tickCount = Environment.TickCount;
                DateTime now = DateTime.Now;
                int num = (int)IntervalMilliseconds;
                int duration = m_PausedSlots.GetDuration();
                int num2 = tickCount - m_StartTick;
                int num3 = num2 - duration;
                if (num3 >= num)
                {
                    m_CompletedTick = tickCount;
                    m_Remainder = TimeSpan.Zero;
                    m_EndTime = now;
                    SetState(TimeoutCheckerState.CompletedSuccessfully);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 현재 상태가 동작 상태인지 검사합니다.
        /// </summary>
        /// <returns>동작 중이면 true이고, 그렇지 않으면 false입니다.</returns>
        protected virtual bool GetIsActive()
        {
            if (IsCompleted)
            {
                return false;
            }
            return TimeoutCheckerStateEnum.IsActive(State);
        }

        /// <summary>
        /// 경과된 시간을 산출합니다.
        /// </summary>
        /// <returns>경과된 시간입니다.</returns>
        protected virtual TimeSpan GetElapsed()
        {
            int tickCount = Environment.TickCount;
            int num = 0;
            int num2 = 0;
            if (IsCompleted)
            {
                num = m_CompletedTick - m_StartTick;
                num2 = m_PausedSlots.GetDuration();
                int num3;
                if (State == TimeoutCheckerState.Passed)
                {
                    num3 = ((m_PausedSlots.Count > 0) ? 1 : 0);
                }
                else
                {
                    num3 = 0;
                }
                if (num3 != 0)
                {
                    TickSpan tickSpan = m_PausedSlots[m_PausedSlots.Count - 1];
                    if (tickSpan.From == tickSpan.To)
                    {
                        return TimeSpan.FromMilliseconds(tickSpan.From - m_StartTick - num2);
                    }
                }
                return TimeSpan.FromMilliseconds(num - num2);
            }
            if (State == TimeoutCheckerState.Idle)
            {
                return TimeSpan.Zero;
            }
            if (State == TimeoutCheckerState.Running)
            {
                num = tickCount - m_StartTick;
                num2 = m_PausedSlots.GetDuration();
                return TimeSpan.FromMilliseconds(num - num2);
            }
            if (State == TimeoutCheckerState.Paused)
            {
                num = m_PausedSlots[m_PausedSlots.Count - 1].From - m_StartTick;
                num2 = m_PausedSlots.GetDuration();
                return TimeSpan.FromMilliseconds(num - num2);
            }
            throw new ApplicationException();
        }

        /// <summary>
        /// 경과된 전체 시간을 산출합니다.
        /// </summary>
        /// <returns>경과된 전체 시간입니다.</returns>
        protected virtual TimeSpan GetElapsedTotal()
        {
            if (IsCompleted)
            {
                return TimeSpan.FromMilliseconds(m_CompletedTick - m_StartTick);
            }
            int tickCount = Environment.TickCount;
            if (State == TimeoutCheckerState.Idle)
            {
                return TimeSpan.Zero;
            }
            return TimeSpan.FromMilliseconds(tickCount - m_StartTick);
        }

        /// <summary>
        /// 남은 시간을 산출합니다.
        /// </summary>
        /// <returns>남은 시간입니다.</returns>
        protected virtual TimeSpan GetRemainder()
        {
            int num;
            if (!IsCompleted)
            {
                num = ((State == TimeoutCheckerState.Idle) ? 1 : 0);
            }
            else
            {
                num = 1;
            }
            if (num != 0)
            {
                return m_Remainder;
            }
            TimeSpan interval = Interval;
            TimeSpan elapsed = Elapsed;
            TimeSpan result;
            if (!(interval <= elapsed))
            {
                result = interval - elapsed;
            }
            else
            {
                result = TimeSpan.Zero;
            }
            return result;
        }

        /// <summary>
        /// 설정 시간을 변경합니다.
        /// </summary>
        /// <param name="interval">변경할 설정 시간입니다.</param>
        /// <returns>성공하면 0이고, 그렇지 않으면 0이 아닌 값입니다.</returns>
        protected virtual int OnChangeInterval(TimeSpan interval)
        {
            int result = 0;
            m_Interval = VerifyInterval(interval);
            return result;
        }

        /// <summary>
        /// 동작하기 전의 초기 상태로 만듭니다.
        /// </summary>
        /// <returns>성공하면 0이고, 그렇지 않으면 0이 아닌 값입니다.</returns>
        protected int Initialize()
        {
            return OnInitialize();
        }

        /// <summary>
        /// 동작하기 전의 초기 상태로 만듭니다.
        /// </summary>
        /// <returns>성공하면 0이고, 그렇지 않으면 0이 아닌 값입니다.</returns>
        protected virtual int OnInitialize()
        {
            int result = 0;
            m_StartTime = DateTime.MinValue;
            m_EndTime = DateTime.MinValue;
            m_StartTick = 0;
            m_CompletedTick = 0;
            m_Remainder = TimeSpan.Zero;
            m_PausedSlots = new TickSpanCollection();
            SetState(TimeoutCheckerState.Idle);
            return result;
        }

        /// <summary>
        /// 지정된 설정 시간으로 동작을 다시 시작합니다.
        /// </summary>
        /// <param name="interval">설정 시간입니다.</param>
        /// <returns>성공하면 0이고, 그렇지 않으면 0이 아닌 값입니다.</returns>
        protected virtual int OnReset(TimeSpan interval)
        {
            int result = 0;
            if (IsCompleted)
            {
                Initialize();
            }
            if (TimeoutCheckerStateEnum.IsActive(State))
            {
                Pass();
                Initialize();
            }
            ChangeInterval(interval);
            m_StartTick = Environment.TickCount;
            m_StartTime = DateTime.Now;
            SetState(TimeoutCheckerState.Running);
            return result;
        }

        /// <summary>
        /// 동작을 즉시 중단합니다.
        /// </summary>
        /// <returns>성공하면 0이고, 그렇지 않으면 0이 아닌 값입니다.</returns>
        protected virtual int OnPass()
        {
            int result = 0;
            if (IsCompleted)
            {
                if (State == TimeoutCheckerState.CompletedSuccessfully)
                {
                    return -1;
                }
                return result;
            }
            if (State == TimeoutCheckerState.Idle)
            {
                return -1;
            }
            int tickCount = Environment.TickCount;
            DateTime now = DateTime.Now;
            TimeSpan remainder = Remainder;
            m_CompletedTick = tickCount;
            m_Remainder = remainder;
            m_EndTime = now;
            SetState(TimeoutCheckerState.Passed);
            return result;
        }

        /// <summary>
        /// 동작을 일시 정지합니다.
        /// </summary>
        /// <returns>성공하면 0이고, 그렇지 않으면 0이 아닌 값입니다.</returns>
        protected virtual int OnPause()
        {
            int result = 0;
            if (IsCompleted)
            {
                return -1;
            }
            if (State != TimeoutCheckerState.Running)
            {
                return -1;
            }
            int tickCount = Environment.TickCount;
            m_PausedSlots.Add(new TickSpan(tickCount, tickCount));
            SetState(TimeoutCheckerState.Paused);
            return result;
        }

        /// <summary>
        /// 동작을 재개합니다.
        /// </summary>
        /// <returns>성공하면 0이고, 그렇지 않으면 0이 아닌 값입니다.</returns>
        protected virtual int OnResume()
        {
            int result = 0;
            if (IsCompleted)
            {
                return -1;
            }
            if (State != TimeoutCheckerState.Paused)
            {
                return -1;
            }
            int tickCount = Environment.TickCount;
            TickSpan tickSpan = m_PausedSlots[m_PausedSlots.Count - 1];
            m_PausedSlots[m_PausedSlots.Count - 1] = new TickSpan(tickSpan.From, tickCount);
            SetState(TimeoutCheckerState.Running);
            return result;
        }

        /// <summary>
        /// 동작의 종료를 대기할 수 있는 상태인지 체크합니다. 아직 시작하지 않았거나 이미 종료되었으면 에러를 반환합니다.
        /// </summary>
        /// <returns>성공하면 0이고, 그렇지 않으면 0이 아닌 값입니다.</returns>
        protected virtual int CheckForWaiting()
        {
            int result = 0;
            if (IsCompleted)
            {
                return 1;
            }
            if (State == TimeoutCheckerState.Idle)
            {
                return 1;
            }
            return result;
        }

        /// <summary>
        /// 설정 시간을 변경합니다.
        /// </summary>
        /// <param name="interval">설정 시간입니다.</param>
        /// <returns>작업에 대한 결과를 반환한다. 0이면 성공 그렇지 않으면 실패를 나타냅니다</returns>
        public int ChangeInterval(TimeSpan interval)
        {
            return OnChangeInterval(interval);
        }

        /// <summary>
        /// 설정 시간을 변경합니다.
        /// </summary>
        /// <param name="interval">밀리초의 설정 시간입니다.</param>
        /// <returns>작업에 대한 결과를 반환한다. 0이면 성공 그렇지 않으면 실패를 나타냅니다</returns>
        public int ChangeInterval(double interval)
        {
            return ChangeInterval(VerifyInterval(interval));
        }

        /// <summary>
        /// 설정된 시간으로 다시 시작합니다.
        /// </summary>
        /// <param name="interval">설정 시간입니다</param>
        /// <returns>작업에 대한 결과를 반환한다. 0이면 성공 그렇지 않으면 실패를 나타냅니다</returns>
        public int Reset(TimeSpan interval)
        {
            return OnReset(interval);
        }

        /// <summary>
        /// 설정된 시간으로 다시 시작합니다.
        /// </summary>
        /// <param name="interval">밀리초의 설정 시간입니다</param>
        /// <returns>작업에 대한 결과를 반환한다. 0이면 성공 그렇지 않으면 실패를 나타냅니다</returns>
        public int Reset(double interval)
        {
            return Reset(VerifyInterval(interval));
        }

        /// <summary>
        /// 이전에 설정된 시간으로 다시 시작합니다.
        /// </summary>
        /// <returns>작업에 대한 결과를 반환한다. 0이면 성공 그렇지 않으면 실패를 나타냅니다</returns>
        public int Reset()
        {
            return Reset(Interval);
        }

        /// <summary>
        /// 남은 시간에 상관없이 즉시 완료합니다.
        /// </summary>
        /// <returns>작업에 대한 결과를 반환한다. 0이면 성공 그렇지 않으면 실패를 나타냅니다</returns>
        public int Pass()
        {
            return OnPass();
        }

        /// <summary>
        /// 일시 정지 합니다.
        /// </summary>
        /// <returns>작업에 대한 결과를 반환한다. 0이면 성공 그렇지 않으면 실패를 나타냅니다</returns>
        public int Pause()
        {
            return OnPause();
        }

        /// <summary>
        /// 일시 정지된 경우 재구동 합니다.
        /// </summary>
        /// <returns>작업에 대한 결과를 반환한다. 0이면 성공 그렇지 않으면 실패를 나타냅니다</returns>
        public int Resume()
        {
            return OnResume();
        }

        /// <summary>
        /// 설정 시간이 경과 될 때까지 기다립니다.
        /// </summary>
        /// <param name="reset">시작 시간을 현재로 설정할지 여부입니다.</param>
        /// <returns>작업에 대한 결과를 반환한다. 0이면 성공 그렇지 않으면 실패를 나타냅니다</returns>
        public int Waiting(bool reset)
        {
            int num = 0;
            if (reset)
            {
                if ((num = Reset()) != 0)
                {
                    return num;
                }
            }
            else if ((num = CheckForWaiting()) != 0)
            {
                int result;
                if (num != 1)
                {
                    result = num;
                }
                else
                {
                    result = 0;
                }
                return result;
            }
            return OnWaiting();
        }

        /// <summary>
        /// 설정 시간이 경과 될 때까지 기다립니다.
        /// </summary>
        /// <returns>작업에 대한 결과를 반환한다. 0이면 성공 그렇지 않으면 실패를 나타냅니다</returns>
        public int Waiting()
        {
            return Waiting(reset: false);
        }

        private static TimeSpan VerifyInterval(TimeSpan interval)
        {
            if (interval < TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException();
            }
            if (interval >= MaxInterval)
            {
                return MaxInterval;
            }
            return interval;
        }

        private static TimeSpan VerifyInterval(double interval)
        {
            double value;
            if (interval >= g_MaxInterval)
            {
                value = g_MaxInterval;                
            }
            else
            {
                value = interval;
            }
            return VerifyInterval(TimeSpan.FromMilliseconds(value));
        }

        /// <summary>
        /// TimeoutChecker 에 대해 동기화되어 스레드로부터 안전하게 보호되는 래퍼를 반환합니다.
        /// </summary>
        /// <returns>TimeoutChecker 에 대해 동기화되어 스레드로부터 안전하게 보호되는 래퍼입니다.</returns>
        public static TimeoutChecker Synchronized()
        {
            return Synchronized(autoStart: false);
        }

        /// <summary>
        /// TimeoutChecker 에 대해 동기화되어 스레드로부터 안전하게 보호되는 래퍼를 반환합니다.
        /// </summary>
        /// <param name="autoStart">자동으로 시작할지 여부입니다</param>
        /// <returns>TimeoutChecker 에 대해 동기화되어 스레드로부터 안전하게 보호되는 래퍼입니다.</returns>
        public static TimeoutChecker Synchronized(bool autoStart)
        {
            return Synchronized(MaxInterval, autoStart);
        }

        /// <summary>
        /// TimeoutChecker 에 대해 동기화되어 스레드로부터 안전하게 보호되는 래퍼를 반환합니다.
        /// </summary>
        /// <param name="interval">밀리초의 설정 시간입니다</param>
        /// <returns>TimeoutChecker 에 대해 동기화되어 스레드로부터 안전하게 보호되는 래퍼입니다.</returns>
        public static TimeoutChecker Synchronized(double interval)
        {
            return Synchronized(interval, autoStart: false);
        }

        /// <summary>
        /// TimeoutChecker 에 대해 동기화되어 스레드로부터 안전하게 보호되는 래퍼를 반환합니다.
        /// </summary>
        /// <param name="interval">밀리초의 설정 시간입니다</param>
        /// <param name="autoStart">자동으로 시작할지 여부입니다</param>
        /// <returns>TimeoutChecker 에 대해 동기화되어 스레드로부터 안전하게 보호되는 래퍼입니다.</returns>
        public static TimeoutChecker Synchronized(double interval, bool autoStart)
        {
            return Synchronized(VerifyInterval(interval), autoStart);
        }

        /// <summary>
        /// TimeoutChecker 에 대해 동기화되어 스레드로부터 안전하게 보호되는 래퍼를 반환합니다.
        /// </summary>
        /// <param name="interval">설정 시간입니다</param>
        /// <returns>TimeoutChecker 에 대해 동기화되어 스레드로부터 안전하게 보호되는 래퍼입니다.</returns>
        public static TimeoutChecker Synchronized(TimeSpan interval)
        {
            return Synchronized(interval, autoStart: false);
        }

        /// <summary>
        /// TimeoutChecker 에 대해 동기화되어 스레드로부터 안전하게 보호되는 래퍼를 반환합니다.
        /// </summary>
        /// <param name="interval">설정 시간입니다</param>
        /// <param name="autoStart">자동으로 시작할지 여부입니다</param>
        /// <returns>TimeoutChecker 에 대해 동기화되어 스레드로부터 안전하게 보호되는 래퍼입니다.</returns>
        public static TimeoutChecker Synchronized(TimeSpan interval, bool autoStart)
        {
            return new SynchronizedTimeoutChecker(interval, autoStart);
        }

        /// <summary>
        /// 재정의 되었습니다.
        /// </summary>
        /// <returns>개체를 나타내는 문자열입니다.</returns>
        public override string ToString()
        {
            string empty = string.Empty;
            empty = empty + IsSynchronized + ", ";
            empty = empty + IntervalMilliseconds.ToString("N3") + ", ";
            empty = empty + StartTime.ToString("yyyy-MM-dd HH:mm:ss") + ", ";
            empty = empty + EndTime.ToString("yyyy-MM-dd HH:mm:ss") + ", ";
            empty = empty + ElapsedMilliseconds.ToString("N3") + ", ";
            empty = empty + RemainderMilliseconds.ToString("N3") + ", ";
            empty = empty + ElapsedTotalMilliseconds.ToString("N3") + ", ";
            return empty + State.ToString();
        }
    }
}
