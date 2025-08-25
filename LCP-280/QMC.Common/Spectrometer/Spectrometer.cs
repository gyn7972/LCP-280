using System;

namespace QMC.Common.Component
{
    /// <summary>
    /// Spectrometer의 기본 기능을 제공하는 클래스입니다.
    /// </summary>
    public abstract class Spectrometer : BaseComponent
    {
        #region Constructor
        public Spectrometer(string name) : base(name)
        {
        }
        #endregion

        #region Event
        public delegate void SpectroemeterEventHandler(bool result);

        public event SpectroemeterEventHandler CreateCompleted;
        public event SpectroemeterEventHandler TerminateCompleted;
        public event SpectroemeterEventHandler InitializeCompleted;
        public event SpectroemeterEventHandler LoadParameterCompleted;
        public event SpectroemeterEventHandler ApplyParameterCompleted;
        public event SpectroemeterEventHandler MeasureCompleted;
        public event SpectroemeterEventHandler MeasureDarkCurrentCompleted;
        
        protected virtual void OnCreateCompleted(bool result)
        {
            CreateCompleted?.Invoke(result);
        }
        protected virtual void OnTerminateCompleted(bool result)
        {
            TerminateCompleted?.Invoke(result);
        }
        protected virtual void OnInitializeCompleted(bool result)
        {
            InitializeCompleted?.Invoke(result);
        }
        protected virtual void OnLoadParameterCompleted(bool result)
        {
            LoadParameterCompleted?.Invoke(result);
        }
        protected virtual void OnApplyParameterCompleted(bool result)
        {
            ApplyParameterCompleted?.Invoke(result);
        }
        protected virtual void OnMeasureCompleted(bool result)
        {
            MeasureCompleted?.Invoke(result);
        }
        protected virtual void OnMeasureDarkCurrentCompleted(bool result)
        {
            MeasureDarkCurrentCompleted?.Invoke(result);
        }
        #endregion

        #region Methods
        public virtual bool OnCreate()
        {
            bool result = CreateProc();
            OnCreateCompleted(result);
            return result;
        }
        public virtual bool OnTerminate()
        {
            bool result = TerminateProc();
            OnTerminateCompleted(result);
            return result;
        }
        public virtual bool OnInitialize()
        {
            bool result = InitializeProc();
            OnInitializeCompleted(result);
            return result;
        }
        public virtual bool OnLoadParameter()
        {
            bool result = LoadParameterProc();
            OnLoadParameterCompleted(result);
            return result;
        }
        public virtual bool OnApplyParameter()
        {
            bool result = ApplyParameterProc();
            OnApplyParameterCompleted(result);
            return result;
        }
        public virtual bool OnMeasure()
        {
            bool result = MeasureProc();
            OnMeasureCompleted(result);
            return result;
        }
        public virtual bool OnMeasureDarkCurrent()
        {
            bool result = MeasureDarkCurrentProc();
            OnMeasureDarkCurrentCompleted(result);
            return result;
        }
        #endregion

        #region Abstract Methods
        /// <summary>
        /// Spectrometer 디바이스 통신 객체 생성에 대한 동작을 정의합니다.
        /// </summary>
        protected abstract bool CreateProc();

        /// <summary>
        /// Spectrometer 디바이스 통신 객체 제거에 대한 동작을 정의합니다.
        /// </summary>
        protected abstract bool TerminateProc();

        /// <summary>
        /// Spectrometer 디바이스 초기화에 대한 동작을 정의합니다.
        /// </summary>
        protected abstract bool InitializeProc();

        /// <summary>
        /// Spectrometer 디바이스의 파라미터 값을 가져오는 동작을 정의합니다.
        /// </summary>
        protected abstract bool LoadParameterProc();

        /// <summary>
        /// Spectrometer 디바이스 파라미터를 적용하는 동작을 정의합니다.
        /// </summary>
        protected abstract bool ApplyParameterProc();

        /// <summary>
        /// Spectrometer 디바이스 측정에 대한 동작을 정의합니다.
        /// </summary>
        protected abstract bool MeasureProc();

        /// <summary>
        /// Spectrometer 디바이스 암전류 측정에 대한 동작을 정의합니다.
        /// </summary>
        protected abstract bool MeasureDarkCurrentProc();
        #endregion
    }
}
