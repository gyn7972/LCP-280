/*
 * Purpose
 *      특정 동작의 Cycle Time을 측정하는 객체를 정의한다.
 *      
 * Revision
 *      1. Created: 2019.08.22 by Im Hyeong Ryeol
 * 
 */

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;

namespace QMC.Common
{
    #region CycleTimer
    /// <summary>
    /// Cycle Time을 측정하는 객체입니다.
    /// </summary>
    public class CycleTimer
    {
        #region Define
        public const int DefaultCapacity = 10;

        #region CycleTimeCollection
        [Serializable]
        public class CycleTimeCollection : List<CycleTime>
        {
            #region Constructor
            public CycleTimeCollection()
            {
            }
            #endregion

            #region Method
            /// <summary>
            /// 현재 배열에서 최소로 소요된 시간을 찾습니다.
            /// </summary>
            public TimeSpan Minimum()
            {
                if (this.Count < 1) return new TimeSpan(0);

                return this.ToList().Min(t => t.Interval);
            }

            /// <summary>
            /// 현재 배열에서 최대로 소요된 시간을 찾습니다.
            /// </summary>
            public TimeSpan Maximum()
            {
                if (this.Count < 1) return new TimeSpan(0);

                return this.ToList().Max(t => t.Interval);
            }

            /// <summary>
            /// 현재 배열에서 소요된 전체 시간의 평균값을 구합니다.
            /// </summary>
            public TimeSpan Average()
            {
                if (this.Count < 1) return new TimeSpan(0);

                long averageValue = Convert.ToInt64(this.ToList().Average(t => t.Interval.Ticks));
                return TimeSpan.FromTicks(averageValue);
            }

            /// <summary>
            /// 현재 배열에서 소요된 최근 CycleTime을 찾습니다.
            /// </summary>
            /// <returns>가장 최근 값을 반환합니다. 없는 경우는 CycleTime.MinValue를 반환합니다.</returns>
            public CycleTime Latest()
            {
                try
                {
                    if (0 < this.Count)
                        return this[this.Count - 1];
                    else
                        return CycleTime.MinValue;
                }
                catch (Exception ex)
                {
                    Log.Write(ex);
                    return CycleTime.MinValue;
                }
            }
            #endregion
        }
        #endregion
        #endregion

        #region Field
        private object m_Owner;
        private Guid m_Uid;
        private RangeD m_AvailableRange;
        private int m_Capacity;
        private bool m_ViewSelect;
        private CycleTimeCollection m_CycleTimes;
        private DateTime StartTime;

        public DateTime ProcessStartTime { get; private set; } = DateTime.MinValue;
        public DateTime ProcessEndTime { get; private set; } = DateTime.MinValue;

        public delegate void CycleTimerAddEventHandler(CycleTime cycleTime);
        #endregion

        #region Constructor
        public CycleTimer(object owner)
        {
            this.m_Uid = Guid.NewGuid();
            this.Owner = owner;
            this.CycleTimes = new CycleTimeCollection();

            this.AvailableRange = RangeD.Empty;
            this.Capacity = CycleTimer.DefaultCapacity;

            this.ViewSelect = true;

            this.StartTime = DateTime.MaxValue;
        }
        public CycleTimer() : this(null) { }
        #endregion

        #region Property
        public string UId
        {
            get { return this.m_Uid.ToString(); }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [DefaultValue(true)]
        public bool ViewSelect
        {
            get { return this.m_ViewSelect; }
            set { this.m_ViewSelect = value; }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public CycleTimeCollection CycleTimes
        {
            get { return this.m_CycleTimes; }
            private set { this.m_CycleTimes = value; }
        }

        /// <summary>
        /// CycleTimer가 측정할때 측정값의 허용 범위를 가져오거나 설정합니다.
        /// </summary>
        [Description("CycleTimer가 측정할때 측정값의 허용 범위를 가져오거나 설정합니다.")]
        public RangeD AvailableRange
        {
            get { return this.m_AvailableRange; }
            set { this.m_AvailableRange = value; }
        }

        /// <summary>
        /// CycleTime을 보관할 최대 갯수를 가져오거나 설정합니다.
        /// </summary>
        [Description("CycleTime을 보관할 최대 갯수를 가져오거나 설정합니다.")]
        [DefaultValue(CycleTimer.DefaultCapacity)]
        public int Capacity
        {
            get { return this.m_Capacity; }
            set
            {
                if (this.m_Capacity == value) return;
                if (value < 1)
                    throw new ArgumentOutOfRangeException("Capacity");
                this.m_Capacity = value;
                this.CheckCapacity();
            }
        }

        /// <summary>
        /// 최소로 소요된 시간입니다.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("최소로 소요된 시간입니다.")]
        public TimeSpan Minimum
        {
            get { return this.m_CycleTimes.Minimum(); }
        }

        /// <summary>
        /// 최대로 소요된 시간입니다.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("최대로 소요된 시간입니다.")]
        public TimeSpan Maximum
        {
            get { return this.m_CycleTimes.Maximum(); }
        }

        /// <summary>
        /// 전체 소요된 시간의 평균 시간 입니다.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("전체 소요된 시간의 평균 시간 입니다.")]
        public TimeSpan Average
        {
            get { return this.m_CycleTimes.Average(); }
        }

        /// <summary>
        /// 최근 마지막 측정된 CycleTime 입니다.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("최근 마지막 측정된 CycleTime 입니다.")]
        public CycleTime Latest
        {
            get { return this.m_CycleTimes.Latest(); }
        }
        #endregion

        #region Event Handler
        /// <summary>
        /// CycleTimer가 측정 완료 하고 값을 저장했을 때 발생합니다.
        /// </summary>
        [Description("CycleTimer가 측정 완료 하고 값을 저장했을 때 발생합니다.")]
        public event CycleTimerAddEventHandler CycleTimerAdd;
        public virtual void OnCycleTimerAdd(CycleTime cycleTime)
        {
            //사용자가 Event를 등록 했을경우만 수행 해야한다.
            if (this.CycleTimerAdd != null)
                this.CycleTimerAdd(cycleTime);
        }
        #endregion

        #region Method
        private void AddCycleTime(CycleTime cycleTime)
        {
            this.CheckCapacity();
            this.CycleTimes.Add(cycleTime);

            this.OnCycleTimerAdd(new CycleTime(new DateTime(cycleTime.Start.Ticks), new DateTime(cycleTime.End.Ticks)));
        }
        /// <summary>
        /// CycleTime의 값을 등록합니다.
        /// </summary>
        private void AddCycleTime(DateTime start, DateTime end)
        {
            this.AddCycleTime(new CycleTime(start, end));
        }

        /// <summary>
        /// CycleTime의 갯수가 최대값이 넘지 않았는지 확인합니다.
        /// </summary>
        private void CheckCapacity()
        {
            try
            {
                if (this.CycleTimes.Count >= this.Capacity)
                    this.CycleTimes.RemoveRange(0, ((this.CycleTimes.Count - this.Capacity) + 1));
            }
            catch(Exception ex)
            {
                Log.Write(ex);
            }
        }

        /// <summary>
        /// 시간 측정을 시작합니다.
        /// </summary>
        public void Start()
        {
            this.StartTime = DateTime.Now;
            this.ProcessStartTime = DateTime.Now;  // 전체 작업 시작 시간 기록
        }

        /// <summary>
        /// 시간 측정을 종료합니다.
        /// </summary>
        public void End()
        {
            CycleTime cycleTime;

            // 유효한 Start() 함수를 실행했는지 여부를 확인한다.
            if (this.StartTime == DateTime.MaxValue)
            {
                // Commented by LIM.WT 2019.11.01
                // 멀티스레드를 사용하기 때문에 StartTime이 리셋되는 경우가 발생할 수 있고
                // 중요한 행위가 아니기 때문에 예외 발생을 하지 않도록 수정한다.
                // throw new Exception("You have not started measuring. Please start before finish of the measurement.");
                return;
            }
            // 새로운 cycle time를 생성한다.
            cycleTime = new CycleTime(this.StartTime, DateTime.Now);
            // 허용 범위의 cycle time인지 여부를 확인한다.
            if (this.AvailableRange.Minimum != this.AvailableRange.Maximum && 
                this.AvailableRange.Contains(cycleTime.Interval.TotalMilliseconds) == false) 
                return;

            // cycle time을 추가한다.
            this.AddCycleTime(cycleTime);
            // start time을 리셋한다.
            this.StartTime = DateTime.MaxValue;
            // 총 소요 시간을 갱신한다.
            TotalElapsed += cycleTime.Interval;

            this.ProcessEndTime = DateTime.Now; // 전체 작업 종료 시간 기록

            // LotLog 기록을 위해 AutoRunTracker를 호출한다. //Test필요.
            // Stop 버튼으로만 장비가 정지하지는 않으니깐.. 흠..
            //AutoRunTracker.OnAutoStop();

            // 로그를 기록한다.
            //Console.WriteLine(string.Format("[Cycle Time] Interval: {0} msec, Start: {1}, End: {2}", cycleTime.Interval.TotalMilliseconds, cycleTime.Start.ToString("yyyy-MM-dd HH:mm:ss.fff"), cycleTime.End.ToString("yyyy-MM-dd HH:mm:ss.fff")));
        }

        /// <summary>
        /// 시간 측정한 데이터들을 초기화 합니다.
        /// </summary>
        public void Clear()
        {
            this.CycleTimes.Clear();
        }

        public void ApplySpecification(CycleTimerSpecification specification)
        {
            this.AvailableRange = specification.AvailableRange;
            this.Capacity = specification.Capacity;
        }

        //20250513 - 실시간 가져오기 위해서 추가.
        public bool IsRunning => StartTime != DateTime.MaxValue;

        public TimeSpan Elapsed => (IsRunning && StartTime != DateTime.MaxValue)
                           ? DateTime.Now - StartTime
                           : TimeSpan.Zero;

        //public TimeSpan TotalElapsed { get; private set; }  // End() 내부에서 누적 갱신
        public TimeSpan TotalElapsed { get;  set; }  // 

        public bool SaveToIni(string section, string path)
        {
            try
            {
                var totalTicks = this.CycleTimes.Sum(c => c.Interval.Ticks);
                var totalTime = TimeSpan.FromTicks(totalTicks);

                //NativeMethods.WritePrivateProfileString(section, "TotalRunningTime", totalTime.TotalMilliseconds.ToString(), path);
                //NativeMethods.WritePrivateProfileString(section, "CycleCount", this.CycleTimes.Count.ToString(), path);
                //// [추가] 전체 공정 시작/종료 시간 저장
                //NativeMethods.WritePrivateProfileString(section, "ProcessStartTime", ProcessStartTime.ToString("yyyy-MM-dd HH:mm:ss"), path);
                //NativeMethods.WritePrivateProfileString(section, "ProcessEndTime", ProcessEndTime.ToString("yyyy-MM-dd HH:mm:ss"), path);

                return true;
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                return false;
            }
        }

        public bool LoadFromIni(string section, string path)
        {
            try
            {
                //StringBuilder sb = new StringBuilder(255);
                //NativeMethods.GetPrivateProfileString(section, "TotalRunningTime", "0", sb, sb.Capacity, path);
                //double totalMs = double.Parse(sb.ToString());

                //NativeMethods.GetPrivateProfileString(section, "CycleCount", "0", sb, sb.Capacity, path);
                //int count = int.Parse(sb.ToString());

                //// [추가] 전체 공정 시작/종료 시간 로드
                //NativeMethods.GetPrivateProfileString(section, "ProcessStartTime", "", sb, sb.Capacity, path);
                //if (DateTime.TryParse(sb.ToString(), out var parsedStart))
                //    this.ProcessStartTime = parsedStart;

                //NativeMethods.GetPrivateProfileString(section, "ProcessEndTime", "", sb, sb.Capacity, path);
                //if (DateTime.TryParse(sb.ToString(), out var parsedEnd))
                //    this.ProcessEndTime = parsedEnd;

                //if (totalMs > 0 && count > 0)
                //{
                //    TimeSpan avg = TimeSpan.FromMilliseconds(totalMs / count);
                //    this.Clear();
                //    for (int i = 0; i < count; i++)
                //    {
                //        var fakeStart = DateTime.Now;
                //        this.CycleTimes.Add(new CycleTime(fakeStart, fakeStart + avg)); // 수정
                //    }

                //    this.TotalElapsed = TimeSpan.FromMilliseconds(totalMs); // 누적 시간도 복원
                //}

                return true;
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                return false;
            }
        }

        //private static CycleTimer[] FindCycleTimer(Part part)
        //{
        //    IPart equipmentPart = PartList.GetParts().SingleOrDefault(t => t == part);
        //    if (equipmentPart == null) return null;

        //    CycleTimer[] cycleTimerList = PartList.GetByTypeAndLocator<IHaveCycleTimer>(equipmentPart.Locator.Name, true).Select(t => t.CycleTimer).ToArray();
        //    if (cycleTimerList.Length < 1) return null;

        //    return cycleTimerList;
        //}

        ///// <summary>
        ///// 현재 Part를 Owner로 둔 CycleTimer를 가져옵니다.
        ///// </summary>
        ///// <param name="part">찾을 Part 입니다.</param>
        //[Description("현재 Part를 Owner로 둔 CycleTimer를 가져옵니다.")]
        //public static CycleTimer[] GetCycleTimer(Part part)
        //{
        //    return FindCycleTimer(part);
        //}
        #endregion


        public object Owner
        {
            get { return this.m_Owner; }
            private set { this.m_Owner = value; }
        }

        /// <summary>
        /// 외부에서 측정된 시작/종료 시간을 그대로 CycleTimer에 누적합니다.
        /// (예: Place-to-Place takt 처럼 "이전 이벤트 시각 → 현재 이벤트 시각")
        /// </summary>
        public void Add(DateTime start, DateTime end)
        {
            try
            {
                // start/end 유효성 보정
                if (start == DateTime.MinValue || end == DateTime.MinValue)
                    return;
                if (end < start)
                    return;

                var cycleTime = new CycleTime(start, end);

                // 허용 범위 필터는 End()와 동일 룰 적용
                if (this.AvailableRange.Minimum != this.AvailableRange.Maximum &&
                    this.AvailableRange.Contains(cycleTime.Interval.TotalMilliseconds) == false)
                    return;

                // 기존 private AddCycleTime 사용(용량/이벤트/TotalElapsed 동일 적용)
                AddCycleTime(cycleTime);
                TotalElapsed += cycleTime.Interval;
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
        }

    }
    #endregion

    #region CycleTimerSpecification
    [Serializable]
    [TypeConverterAttribute(typeof(CycleTimerSpecificationConverter))]
    public class CycleTimerSpecification
    {
        #region Field
        private RangeD m_AvailableRange;
        private int m_Capacity;
        #endregion

        #region Constructor
        public CycleTimerSpecification(RangeD available, int capacity)
        {
            this.m_AvailableRange = available;
            this.m_Capacity = capacity;
        }
        public CycleTimerSpecification(RangeD available) : this(available, CycleTimer.DefaultCapacity)
        {
            this.AvailableRange = available;
        }
        public CycleTimerSpecification(int capacity) : this(RangeD.Empty, capacity)
        {
            this.Capacity = capacity;
        }
        public CycleTimerSpecification() : this(CycleTimer.DefaultCapacity) { }
        #endregion

        #region Property
        /// <summary>
        /// CycleTimer가 측정할때 측정값의 허용 범위를 가져오거나 설정합니다.
        /// </summary>
        [Description("CycleTimer가 측정할때 측정값의 허용 범위를 가져오거나 설정합니다.")]
        public RangeD AvailableRange
        {
            get { return this.m_AvailableRange; }
            set { this.m_AvailableRange = value; }
        }

        /// <summary>
        /// CycleTime을 보관할 최대 갯수를 가져오거나 설정합니다.
        /// </summary>
        [Description("CycleTime을 보관할 최대 갯수를 가져오거나 설정합니다.")]
        [DefaultValue(CycleTimer.DefaultCapacity)]
        public int Capacity
        {
            get { return this.m_Capacity; }
            set
            {
                if (value < 1)
                    throw new ArgumentOutOfRangeException("Capacity");
                this.m_Capacity = value;
            }
        }
        #endregion
    }

    public class CycleTimerSpecificationConverter : ExpandableObjectConverter
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
                RangeD rage = new RangeD();
                rage.Minimum = double.Parse(array[0]);
                rage.Maximum = double.Parse(array[1]);
                int nCapacity = int.Parse(array[2]);
                CycleTimerSpecification specification = new CycleTimerSpecification(rage, nCapacity);
                return specification;
            }
            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string) && value is CycleTimerSpecification)
            {
                CycleTimerSpecification specification = value as CycleTimerSpecification;
                return string.Format("{0}, {1}, {2}",
                    specification.AvailableRange.Minimum,
                    specification.AvailableRange.Maximum,
                    specification.Capacity
                    );
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
        //protected override ObjectCollection CreateInstanceDescriptor(object value)
        //{
        //    ObjectCollection constructMember = new ObjectCollection();
        //    constructMember.Add(this.GetPropertyValue(value, "AvailableRange"));
        //    constructMember.Add(this.GetPropertyValue(value, "Capacity"));
        //    return constructMember;
        //}

        //protected override StringCollection GetListProperties()
        //{
        //    StringCollection propertyList = new StringCollection();
        //    propertyList.Add("AvailableRange");
        //    propertyList.Add("Capacity");

        //    return propertyList;
        //}
    }
    #endregion

    #region CycleTimerCollection
    public class CycleTimerCollection : ObservableCollection<CycleTimer>
    {
        #region Constructor
        public CycleTimerCollection() { }
        public CycleTimerCollection(CycleTimer[] list)
        {
            foreach (CycleTimer cycleTimer in list)
                this.Add(cycleTimer);
        }
        #endregion

        /// <summary>
        /// CycleTimer의 ViewSelect를 변경합니다.
        /// </summary>
        /// <param name="uid">CycleTimer의 uid 값입니다.</param>
        /// <param name="value">바꿀 값입니다.</param>
        /// <returns>변경이 성공했을경우 0, 그렇지 않을경우 0 이 아닌 다른값을 반환합니다.</returns>
        public int ChangeViewSelect(string uid, bool value)
        {
            CycleTimer timer = this.SingleOrDefault(t => t.UId == uid);
            if (timer == null)
                return -1;
            timer.ViewSelect = value;

            return 0;
        }
    }
    #endregion

    #region CycleTime
    /// <summary>
    /// 측정하고자 하는 Cycle의 시간입니다.
    /// </summary>
    [Serializable]
    public struct CycleTime
    {
        #region Field
        public static readonly CycleTime MinValue;
        private const string TimeFormat = "HH:mm:ss:fff";
        private DateTime m_Start;
        private DateTime m_End;
        #endregion

        #region Constructor
        public CycleTime(DateTime start, DateTime end)
        {
            this.m_Start = start;
            this.m_End = end;
        }

        static CycleTime()
        {
            CycleTime.MinValue = new CycleTime(DateTime.MinValue, DateTime.MinValue);
        }
        #endregion

        #region Property
        /// <summary>
        /// 측정 시작 시간입니다.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DateTime Start
        {
            get { return this.m_Start; }
            private set { this.m_Start = value; }
        }
        /// <summary>
        /// 측정 종료 시간 입니다.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DateTime End
        {
            get { return this.m_End; }
            private set { this.m_End = value; }
        }

        /// <summary>
        /// 측정 소요 시간 입니다.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public TimeSpan Interval
        {
            get { return this.End.Subtract(this.Start); }
        }
        #endregion

        #region Object Members
        public override string ToString()
        {
            return string.Format("{0},{1},{2}", this.Start.ToString(CycleTime.TimeFormat), this.End.ToString(CycleTime.TimeFormat), this.Interval.ToString());
        }
        #endregion
    }
    #endregion

    #region IHaveCycleTimer
    /// <summary>
    /// CycleTimer를 가지고 있는 객체를 정의한다.
    /// </summary>
    public interface IHaveCycleTimer
    {
        CycleTimer CycleTimer { get; }
    }
    #endregion
}