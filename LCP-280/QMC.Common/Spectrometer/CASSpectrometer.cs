using InstrumentSystems.CAS4;
using QMC.Common.Component;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QMC.Common.Spectrometer
{
    public class CASSpectrometerResult
    {
        public double WP { get; set; } = 0;
        public double FWHM { get; set; } = 0;
        public double CIEX { get; set; } = 0;
        public double CIEY { get; set; } = 0;
        public double CIEZ { get; set; } = 0;
        public double CIEU { get; set; } = 0;
        public double CIEV1976 { get; set; } = 0;
        public double CIEV1960 { get; set; } = 0;
        public double LambdaDom { get; set; } = 0;
        public double Purity { get; set; } = 0;
        public double RadInt { get; set; } = 0;
        public string RadIntUnit { get; set; } = string.Empty;
        public double PhotInt { get; set; } = 0;
        public string PhotIntUnit { get; set; } = string.Empty;
        public double CCT { get; set; } = 0;
        public double CRI { get; set; } = 0;
        public double Centroid { get; set; } = 0;
        public double StimulusX { get; set; } = 0;
        public double StimulusY { get; set; } = 0;
        public double StimulusZ { get; set; } = 0;
        public double PickValue { get; set; } = 0;
        public int ADC { get; set; } = 0;

        public void Clear()
        {
            WP = default;
            FWHM = default;
            CIEX = default;
            CIEY = default;
            CIEZ = default;
            CIEU = default;
            CIEV1976 = default;
            CIEV1960 = default;
            LambdaDom = default;
            Purity = default;
            RadInt = default;
            RadIntUnit = default;
            PhotInt = default;
            PhotIntUnit = default;
            CCT = default;
            CRI = default;
            Centroid = default;
            StimulusX = default;
            StimulusY = default;
            StimulusZ = default;
            PickValue = default;
            ADC = default;
        }
    }
    public class CASSpectrometerSpectrum
    {
        public double[] WaveLength { get; set; } = new double[0];
        public double[] Intensity { get; set; } = new double[0];
        public double MaximumIntensity { get; set; } = 0;

        public void Clear()
        {
            WaveLength = null;
            Intensity = null;
            MaximumIntensity = 0;
        }
    }
    public class CASSpectrometerDensityFilter
    {
        public int Index { get; set; } = -1;
        public int Value { get; set; } = 0;
        public string Name { get; set; } = string.Empty;

        public void Clear()
        {
            Index = default;
            Value = default;
            Name = default;
        }
    }

    /// <summary>
    /// National Instruments사 Spectrometer(CAS) 클래스입니다.
    /// </summary>
    public class CASSpectrometer : BaseComponent
    {
        #region Field
        private int deviceId;
        private List<CASSpectrometerDensityFilter> densityFilterList;
        private CASSpectrometerResult result;
        private CASSpectrometerSpectrum spectrum;
        #endregion

        #region Property
        public new CASSpectrometerConfig Config { get; private set; }
        public int DeviceId
        {
            get { return deviceId; }
        }
        public List<CASSpectrometerDensityFilter> DensityFilterList
        {
            get { return densityFilterList; }
        }
        public CASSpectrometerResult Result
        {
            get { return result; }
        }
        public CASSpectrometerSpectrum Spectrum
        {
            get { return spectrum; }
        }
        
        #endregion

        #region Constructor
        public CASSpectrometer(string name) : base(name)
        {
            deviceId = -1; // Default value for uninitialized device ID
            densityFilterList = new List<CASSpectrometerDensityFilter>();
            result = new CASSpectrometerResult();
            spectrum = new CASSpectrometerSpectrum();

            Config = new CASSpectrometerConfig(name);
        }
        #endregion

        #region Override Methods
        public override int Initialize()
        {
            int ret = 0;
            if (!OnInitDevice())
            {
                ret = -1;
            }
            return ret;
        }

        public override int Create()
        {
            int ret = 0;
            if (!OnCreateDevice())
            {
                ret = -1;
            }
            return ret;
        }

        public override void Close()
        {
            OnTerminateDevice();
        }
        #endregion

        #region Methods
        public bool LoadParameter()
        {
            if (!IsCreated())
                return false;

            bool result = false;
            do
            {
                if (!LoadMeasurementCondition())
                    break;

                result = true;
            }
            while (false);
            return result;
        }
        public bool ApplyParameter()
        {
            if (!IsCreated())
                return false;

            bool result = false;
            do
            {
                if (!ApplyMeasurementCondition())
                    break;

                result = true;
            }
            while (false);
            return result;
        }
        public bool Measure()
        {
            bool result = false;
            do
            {
                if (this.Config.UseExternalTrigger)
                {
                    // Measure with external trigger
                    if (!OnMeasureAndExternalTrigger())
                        break;
                }
                else
                {
                    // Normal measurement
                    if (!OnMeasure())
                        break;
                }

                result = true;
            }
            while (false);
            return result;
        }
        public bool MeasureDarkCurrent()
        {
            bool result = false;
            do
            {
                if (!OnMeasureDarkCurrent())
                    break;

                result = true;
            }
            while (false);
            return result;
        }

        // Error Handling
        #region Error Handling Methods
        private void CheckCASErrorAndThrow(int errorCode)
        {
            if (errorCode < CAS4DLL.ErrorNoError)
            {
                // An error occurred
                StringBuilder sb = new StringBuilder(256);
                CAS4DLL.casGetErrorMessage(errorCode, sb, sb.Capacity);
                throw new Exception($"CAS Error {errorCode}: {sb.ToString()}");
            }
        }
        private void CheckCASErrorAndThrow()
        {
            CheckCASErrorAndThrow(CAS4DLL.casGetError(this.deviceId));
        }
        #endregion

        // Device Management
        #region Device Management Methods
        private bool OnCreateDevice()
        {
            if (IsCreated())
                return true;

            bool result = false;
            try
            {
                // Create device
                int deviceInterfaceType = 0;
                switch(Config.DeviceInterfaceType)
                {
                    case CASSpectrometerConfig.DeviceInterface.PCI:
                        deviceInterfaceType = CAS4DLL.InterfacePCI;
                        break;
                    case CASSpectrometerConfig.DeviceInterface.Test:
                        deviceInterfaceType = CAS4DLL.InterfaceTest;
                        break;
                    case CASSpectrometerConfig.DeviceInterface.USB:
                        deviceInterfaceType = CAS4DLL.InterfaceUSB;
                        break;
                    case CASSpectrometerConfig.DeviceInterface.PCIe:
                        deviceInterfaceType = CAS4DLL.InterfacePCIe;
                        break;
                    case CASSpectrometerConfig.DeviceInterface.Ethernet:
                        deviceInterfaceType = CAS4DLL.InterfaceEthernet;
                        break;
                    default:
                        throw new NotSupportedException("Unsupported Device Interface Type.");
                }

                int deviceInterfaceOption = Config.DeviceInterfaceOption;

                int deviceId = CAS4DLL.casCreateDeviceEx(deviceInterfaceType, deviceInterfaceOption);
                CheckCASErrorAndThrow(deviceId);

                // Processing after successful device initialization
                UpdateSupportDensityFilterList();

                this.deviceId = deviceId;
                result = true;
            }
            catch (Exception ex)
            {
                // Handle any exceptions that occur during device creation
            }
            return result;
        }        
        private bool OnTerminateDevice()
        {
            bool result = false;
            try
            {
                if(IsCreated())
                {
                    CheckCASErrorAndThrow(CAS4DLL.casDoneDevice(this.deviceId));
                }
                this.deviceId = -1;
                result = true;
            }
            catch (Exception ex)
            {
                // Error Handling
            }
            return result;
        }
        private bool OnInitDevice()
        {
            if (!IsCreated())
                return false;

            bool result = false;
            try
            {
                // Set parameter
                SetDeviceParameter(CAS4DLL.dpidConfigFileName, this.Config.ConfigFileName);
                SetDeviceParameter(CAS4DLL.dpidCalibFileName, this.Config.CalibFileName);
                ApplyMeasurementCondition();

                // Device Initialize
                CheckCASErrorAndThrow(CAS4DLL.casInitialize(this.deviceId, CAS4DLL.InitOnce));
                result = true;
            }
            catch (Exception ex)
            {
                // Error handling
            }
            return result;
        }
        public bool IsCreated()
        {
            return (this.deviceId >= 0);
        }
        public bool IsInitialized()
        {
            if (!IsCreated())
                return false;

            bool result = false;
            try
            {
                double value = 0;
                GetDeviceParameter(CAS4DLL.dpidInitialized, ref value);
                result = (value != 0);
            }
            catch (Exception ex)
            {
                // Error Handling
            }
            return result;
        }     
        #endregion

        // Measurement Methods
        #region Measurement Methods
        private bool OnMeasure()
        {
            if (!IsCreated())
                return false;

            bool result = false;
            try
            {
                // Data clear
                this.result.Clear();
                this.spectrum.Clear();

                // Apply measurement parameter
                //ApplyMeasurementCondition();

                // Measure
                CheckCASErrorAndThrow(CAS4DLL.casMeasure(this.deviceId));

                // Data Process
                GetMeasureData();
                GetSpectrumData();
                result = true;
            }
            catch (Exception ex)
            {
                // Error Handling
                this.result.Clear();
                this.spectrum.Clear();
            }
            return result;
        }
        private bool OnMeasureAndExternalTrigger()
        {
            if (!IsCreated())
                return false;

            bool result = false;
            try
            {
                // Data clear
                this.result.Clear();
                this.spectrum.Clear();

                // Apply measurement parameter
                //ApplyMeasurementCondition();

                // Set trigger source
                SetMeasurementParameter(CAS4DLL.mpidTriggerSource, CAS4DLL.trgFlipFlop);
                SetDeviceParameter(CAS4DLL.dpidLine1FlipFlop, 0);

                // Set digital output for triggering & Measure
                OnDigitalOut(2);
                CheckCASErrorAndThrow(CAS4DLL.casMeasure(this.deviceId));
                OffDigitalOut(2);

                // Data Process
                GetMeasureData();
                GetSpectrumData();
                result = true;
            }
            catch (Exception ex)
            {
                // Error Handling
                this.result.Clear();
                this.spectrum.Clear();
            }
            finally
            {
                OffDigitalOut(2);
            }
            return result;
        }
        private bool OnMeasureDarkCurrent()
        {
            if (!IsCreated())
                return false;

            bool result = false;
            try
            {
                OpenShutter();
                CheckCASErrorAndThrow(CAS4DLL.casMeasureDarkCurrent(this.deviceId));
                CloseShutter();
            }
            catch (Exception ex)
            {
                // Error handling
            }
            finally
            {
                CloseShutter();
            }
            return result;
        }
        private void GetMeasureData()
        {
            // Color metric
            CheckCASErrorAndThrow(CAS4DLL.casColorMetric(this.deviceId));

            // Photometric and Radiometric Integral
            StringBuilder sb = new StringBuilder(256);

            double photInt = 0.0;
            CAS4DLL.casGetPhotInt(this.deviceId, out photInt, sb, sb.Capacity);
            CheckCASErrorAndThrow();
            this.Result.PhotInt = photInt;
            this.result.PhotIntUnit = sb.ToString();

            double radInt = 0.0;
            CAS4DLL.casGetRadInt(this.deviceId, out radInt, sb, sb.Capacity);
            CheckCASErrorAndThrow();
            this.Result.RadInt = radInt;
            this.result.RadIntUnit = sb.ToString();

            // color coordinates
            double CIEX = 0.0, CIEY = 0.0, CIEZ = 0.0, CIEU = 0.0, CIEV1976 = 0.0, CIEV1960 = 0.0;
            CAS4DLL.casGetColorCoordinates(this.deviceId, ref CIEX, ref CIEY, ref CIEZ, ref CIEU, ref CIEV1976, ref CIEV1960);
            CheckCASErrorAndThrow();
            this.Result.CIEX = CIEX;
            this.Result.CIEY = CIEY;
            this.Result.CIEZ = CIEZ;
            this.result.CIEU = CIEU;
            this.result.CIEV1976 = CIEV1976;
            this.result.CIEV1960 = CIEV1960;

            // Tristimulus
            double stimulusX = 0.0, stimulusY = 0.0, stimulusZ = 0.0;
            CAS4DLL.casGetTriStimulus(this.deviceId, ref stimulusX, ref stimulusY, ref stimulusZ);
            this.Result.StimulusX = stimulusX;
            this.Result.StimulusY = stimulusY;
            this.Result.StimulusZ = stimulusZ;

            // calulate lambda dominant wavelength and purity
            double lambdaDom = 0.0, purity = 0.0;
            CheckCASErrorAndThrow(CAS4DLL.casCalculateLambdaDom(this.deviceId, 1.0 / 3.0, 1.0 / 3.0, ref lambdaDom, ref purity));
            this.Result.LambdaDom = lambdaDom;
            this.Result.Purity = purity;

            // Wavelength and peak value
            double WP = 0.0, pickValue = 0.0;
            CAS4DLL.casGetPeak(this.deviceId, out WP, out pickValue);
            CheckCASErrorAndThrow();
            this.result.WP = WP;
            this.result.PickValue = pickValue;
            this.result.Centroid = CAS4DLL.casGetCentroid(this.deviceId);
            this.result.FWHM = CAS4DLL.casGetWidth(this.deviceId);

            // color rendering index
            CAS4DLL.casCalculateCRI(this.deviceId);
            this.result.CRI = CAS4DLL.casGetCRI(this.deviceId, 0);

            // color temperature
            this.result.CCT = CAS4DLL.casGetCCT(this.deviceId);

            // Maximum ADC value
            this.result.ADC = (int)CAS4DLL.casGetMeasurementParameter(this.deviceId, CAS4DLL.mpidMaxADCValue);
        }
        private void GetSpectrumData()
        {
            double value = 0;
            int visiblePixels = 0;
            int deadPixels = 0;

            GetDeviceParameter(CAS4DLL.dpidVisiblePixels, ref value);
            visiblePixels = (int)Math.Round(value);

            GetDeviceParameter(CAS4DLL.dpidDeadPixels, ref value);
            deadPixels = (int)Math.Round(value);

            this.spectrum.WaveLength = new double[visiblePixels];
            this.spectrum.Intensity = new double[visiblePixels];

            for (int i = 0; i < this.spectrum.WaveLength.Length; i++)
            {
                spectrum.Intensity[i] = CAS4DLL.casGetData(this.deviceId, i + deadPixels);
                spectrum.MaximumIntensity = Math.Max(spectrum.MaximumIntensity, spectrum.Intensity[i]);

                spectrum.WaveLength[i] = CAS4DLL.casGetXArray(this.deviceId, i + deadPixels);
            }
        }
        #endregion

        // Control Methods
        #region Control Methods
        private bool OnDigitalOut(int port)
        {
            if (!IsCreated())
                return false;

            bool result = false;
            try
            {
                CAS4DLL.casSetDigitalOut(this.deviceId, port, 1);
                CheckCASErrorAndThrow();
                result = true;
            }
            catch (Exception ex)
            {
                // Error handling
            }
            return result;
        }
        private bool OffDigitalOut(int port)
        {
            if (!IsCreated())
                return false;

            bool result = false;
            try
            {
                CAS4DLL.casSetDigitalOut(this.deviceId, port, 0);
                CheckCASErrorAndThrow();
                result = true;
            }
            catch (Exception ex)
            {
                // Error handling
            }
            return result;
        }
        private bool OpenShutter()
        {
            if (!IsCreated())
                return false;

            bool result = false;
            try
            {
                CAS4DLL.casSetShutter(this.deviceId, CAS4DLL.casShutterOpen);
                CheckCASErrorAndThrow();
                result = true;
            }
            catch (Exception ex)
            {
                // Error handling
            }
            return result;
        }
        private bool CloseShutter()
        {
            if (!IsCreated())
                return false;

            bool result = false;
            try
            {
                CAS4DLL.casSetShutter(this.deviceId, CAS4DLL.casShutterClose);
                CheckCASErrorAndThrow();
                result = true;
            }
            catch (Exception ex)
            {
                // Error handling
            }
            return result;
        }
        #endregion

        // Parameter Methods
        #region Parameter Methods
        private void SetDeviceParameter(int what, double value)
        {
            if (!IsCreated())
                throw new InvalidOperationException("The Device was not created.");

            int result = CAS4DLL.casSetDeviceParameter(this.deviceId, what, value);
            CheckCASErrorAndThrow(result);
        }
        private void SetDeviceParameter(int what, string value)
        {
            if (!IsCreated())
                throw new InvalidOperationException("The Device was not created.");

            int result = CAS4DLL.casSetDeviceParameterString(this.deviceId, what, value);
            CheckCASErrorAndThrow(result);
        }
        private void GetDeviceParameter(int what, ref double value)
        {
            if (!IsCreated())
                throw new InvalidOperationException("The Device was not created.");

            double result = CAS4DLL.casGetDeviceParameter(this.deviceId, what);
            CheckCASErrorAndThrow();
            value = result;
        }
        private void GetDeviceParameter(int what, ref string value)
        {
            if (!IsCreated())
                throw new InvalidOperationException("The Device was not created.");

            StringBuilder sb = new StringBuilder(256);
            int result = CAS4DLL.casGetDeviceParameterString(this.deviceId, what, sb, sb.Capacity);
            CheckCASErrorAndThrow(result);
            value = sb.ToString();
        }
        private void SetMeasurementParameter(int what, double value)
        {
            if (!IsCreated())
                throw new InvalidOperationException("The Device was not created.");

            int result = CAS4DLL.casSetMeasurementParameter(this.deviceId, what, value);
            CheckCASErrorAndThrow(result);
        }
        private void GetMeasurementParameter(int what, ref double value)
        {
            if (!IsCreated())
                throw new InvalidOperationException("The Device was not created.");

            double result = CAS4DLL.casGetMeasurementParameter(this.deviceId, what);
            CheckCASErrorAndThrow();
            value = result;
        }
        private bool LoadMeasurementCondition()
        {
            bool result = false;
            try
            {
                double integrationTime = 0;
                double averages = 0;
                double densityFilter = 0;
                double colormetricStart = 0;
                double colormetricStop = 0;
                double triggerTimeout = 0;

                GetMeasurementParameter(CAS4DLL.mpidIntegrationTime, ref integrationTime);
                GetMeasurementParameter(CAS4DLL.mpidAverages, ref averages);
                GetMeasurementParameter(CAS4DLL.mpidDensityFilter, ref densityFilter);
                GetMeasurementParameter(CAS4DLL.mpidColormetricStart, ref colormetricStart);
                GetMeasurementParameter(CAS4DLL.mpidColormetricStop, ref colormetricStop);
                GetMeasurementParameter(CAS4DLL.mpidTriggerTimeout, ref triggerTimeout);

                this.Config.IntegrationTime = (int)Math.Round(integrationTime);
                this.Config.Averages = (int)Math.Round(averages);
                this.Config.DensityFilter = (int)Math.Round(densityFilter);
                this.Config.ColormetricStart = (int)Math.Round(colormetricStart);
                this.Config.ColormetricStop = (int)Math.Round(colormetricStop);
                this.Config.TriggerTimeout = (int)Math.Round(triggerTimeout);
                result = true;
            }
            catch (Exception ex)
            {
                // Error Handling
            }
            return result;
        }
        private bool ApplyMeasurementCondition()
        {
            bool result = false;
            try
            {
                SetMeasurementParameter(CAS4DLL.mpidIntegrationTime, this.Config.IntegrationTime);
                SetMeasurementParameter(CAS4DLL.mpidAverages, this.Config.Averages);
                SetMeasurementParameter(CAS4DLL.mpidDensityFilter, this.Config.DensityFilter);
                SetMeasurementParameter(CAS4DLL.mpidColormetricStart, this.Config.ColormetricStart);
                SetMeasurementParameter(CAS4DLL.mpidColormetricStop, this.Config.ColormetricStop);
                SetMeasurementParameter(CAS4DLL.mpidObserver, CAS4DLL.cieObserver1931);
                SetMeasurementParameter(CAS4DLL.mpidTriggerTimeout, this.Config.TriggerTimeout);
                result = true;
            }
            catch (Exception ex)
            {
                // Error Handling
            }
            return result;
        }
        #endregion

        // Density Filter Methods
        #region Density Filter Methods
        private void UpdateSupportDensityFilterList()
        {
            DensityFilterList.Clear();
            if (IsCreated())
            {
                StringBuilder stringBuilder = new StringBuilder(256);
                for (int i = 0; i < 7; i++)
                {
                    CAS4DLL.casGetFilterName(this.deviceId, i, stringBuilder, stringBuilder.Capacity);
                    string filterName = stringBuilder.ToString();
                    if (string.IsNullOrEmpty(filterName) || filterName == "none")
                        continue;

                    CASSpectrometerDensityFilter densityFilter = new CASSpectrometerDensityFilter();
                    densityFilter.Index = DensityFilterList.Count;
                    densityFilter.Value = i;
                    densityFilter.Name = filterName;
                    DensityFilterList.Add(densityFilter);
                }
            }
        }
        public string GetDensityFilterNameFromValue(int value)
        {
            string filterName = string.Empty;
            foreach (var filter in this.DensityFilterList)
            {
                if (filter.Value == value)
                {
                    filterName = filter.Name;
                    break;
                }
            }
            return filterName;
        }
        public int GetDensityFilterValueFromName(string name)
        {
            int filterValue = -1;
            foreach (var filter in this.DensityFilterList)
            {
                if (filter.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                {
                    filterValue = filter.Value;
                    break;
                }
            }
            return filterValue;
        }
        public List<string> GetSupportDensityFilterNameList()
        {
            List<string> filterNames = new List<string>();
            foreach (var filter in this.DensityFilterList)
            {
                filterNames.Add(filter.Name);
            }
            return filterNames;
        }
        #endregion

        #endregion
    }
}
