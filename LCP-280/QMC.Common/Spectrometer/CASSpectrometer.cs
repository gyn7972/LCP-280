using InstrumentSystems.CAS4;
using QMC.Common.Component;
using QMC.Common.PKGTester;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QMC.Common.Spectrometer
{
    /// <summary>
    /// Instrument Systems CAS Spectrometer
    /// </summary>
    public class CASSpectrometer : BaseComponent, IDisposable
    {
        #region Defines
        public class MeasurementData
        {
            #region Properties
            public double WP { get; set; }
            public double FWHM { get; set; }
            public double CIEX { get; set; }
            public double CIEY { get; set; }
            public double CIEZ { get; set; }
            public double CIEU { get; set; }
            public double CIEV1976 { get; set; }
            public double CIEV1960 { get; set; }
            public double LambdaDom { get; set; }
            public double Purity { get; set; }
            public double RadInt { get; set; }
            public string RadIntUnit { get; set; }
            public double PhotInt { get; set; }
            public string PhotIntUnit { get; set; }
            public double CCT { get; set; }
            public double CRI { get; set; }
            public double Centroid { get; set; }
            public double StimulusX { get; set; }
            public double StimulusY { get; set; }
            public double StimulusZ { get; set; }
            public double PickValue { get; set; }
            public int ADC { get; set; }
            #endregion

            #region Methods
            public void Clear()
            {
                WP = 0;
                FWHM = 0;
                CIEX = 0;
                CIEY = 0;
                CIEZ = 0;
                CIEU = 0;
                CIEV1976 = 0;
                CIEV1960 = 0;
                LambdaDom = 0;
                Purity = 0;
                RadInt = 0;
                RadIntUnit = "";
                PhotInt = 0;
                PhotIntUnit = "";
                CCT = 0;
                CRI = 0;
                Centroid = 0;
                StimulusX = 0;
                StimulusY = 0;
                StimulusZ = 0;
                PickValue = 0;
                ADC = 0;
            }
            #endregion
        }

        public class SpectrumData
        {
            #region Properties
            public double[] WaveLength { get; set; }
            public double[] Intensity { get; set; }
            public double MinimumIntensity { get; set; }
            public double MaximumIntensity { get; set; }
            #endregion

            #region Methods
            public void Clear()
            {
                WaveLength = null;
                WaveLength = new double[0];
                Intensity = null;
                Intensity = new double[0];
                MinimumIntensity = 0;
                MaximumIntensity = 0;
            }
            #endregion
        }

        public class DensityFilter
        {
            #region Properties
            public int Index { get; set; }
            public int Value { get; set; }
            public string Name { get; set; }
            #endregion

            #region Methods
            public void Clear()
            {
                Index = -1;
                Value = 0;
                Name = "";
            }
            #endregion
        }

        public class DeviceInformation
        {
            #region Properties
            public string Name { get; set; }
            public string SerialNumber { get; set; }
            public string InterfaceType { get; set; }
            public string InterfaceOption { get; set; }
            #endregion

            #region Methods
            public void Clear()
            {
                Name = "";
                SerialNumber = "";
                InterfaceType = "";
                InterfaceOption = "";
            }
            #endregion
        }
        #endregion

        #region Field
        private int deviceId;
        private List<DensityFilter> densityFilterList = new List<DensityFilter>();
        private MeasurementData result = new MeasurementData();
        private SpectrumData spectrum = new SpectrumData();

        private List<TestConditionItem> testItems = new List<TestConditionItem>();
        private Dictionary<string, TestItemResult> results = new Dictionary<string, TestItemResult>();
        private string intensityUnit = "";
        
        private DeviceInformation deviceInfo = new DeviceInformation();
        private bool useHardwareTrigger = false;
        #endregion

        #region Property
        public new CASSpectrometerConfig Config { get; private set; }
        public DeviceInformation DeviceInfo => deviceInfo;
        public List<DensityFilter> DensityFilterList => densityFilterList;
        public SpectrumData Spectrum => spectrum;
        public IDictionary<string, TestItemResult> Results => results;
        //public bool IsReady { get => bIsReady; set => bIsReady = value; }
        #endregion

        #region Constructor
        public CASSpectrometer(string name) : base(name)
        {
            deviceId = -1; // Default value for uninitialized device ID
            deviceInfo.Clear();
            Config = new CASSpectrometerConfig(name);
        }
        #endregion

        #region IDisposable
        private bool disposed = false;
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        private void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    // Dispose managed resources
                    TerminateDevice();
                    Config = null;
                    result = null;
                    spectrum = null;
                    if (densityFilterList != null)
                    {
                        densityFilterList.Clear();
                        densityFilterList = null;
                    }
                }
                // Free unmanaged resources if any
                disposed = true;
            }
        }
        #endregion

        #region Event
        public delegate void DeviceEventHandler(object sender);

        public event DeviceEventHandler OnDeviceCreated;
        public event DeviceEventHandler OnDeviceTerminated;
        public event DeviceEventHandler OnMeasureCommandSended;
        public event DeviceEventHandler OnMeasureCompleted;
        public event EventHandler<string> OnMeasureFailed;
        #endregion

        #region Override Methods
        public override int Initialize()
        {
            int ret = 0;
            do
            {
                if (IsCreated())
                {
                    TerminateDevice();
                }
                if (!CreateDevice())
                {
                    ret = -1;
                    break;
                }
                if (!InitializeDevice())
                {
                    ret = -1;
                    break;
                }
            }
            while (false);
            return ret;
        }
        #endregion

        #region Test Item Methods
        public void ClearTestItems()
        {
            testItems.Clear();
            results.Clear();
        }
        public bool AddTestItem(TestConditionItem item)
        {
            if (item == null)
                return false;
            if (item.GetTestItemCategory() != TestItemCategory.Optical)
                return false;

            testItems.Add(item);
            results.Add(item.Name, new TestItemResult());
            return true;
        }
        public bool BuildTestCommands()
        {
            try
            {
                // Do something
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                return false;
            }
            return true;
        }
        public bool GetResultProcess()
        {
            try
            {
                foreach (var item in testItems)
                {
                    TestItemResult itemResult = results[item.Name];

                    double value = 0;
                    switch (item.Type)
                    {
                        case TestItemType.RadInt:
                            value = result.RadInt;
                            itemResult.RawData = value;
                            itemResult.Value = value;
                            itemResult.Unit = result.RadIntUnit;
                            break;
                        case TestItemType.PhotInt:
                            value = result.PhotInt;
                            itemResult.RawData = value;
                            itemResult.Value = value;
                            itemResult.Unit = result.PhotIntUnit;
                            break;
                        case TestItemType.WP:
                            value = result.WP;
                            itemResult.RawData = value;
                            itemResult.Value = value;
                            itemResult.Unit = "nm"; // Manual 참조
                            break;
                        case TestItemType.FWHM:
                            value = result.FWHM;
                            itemResult.RawData = value;
                            itemResult.Value = value;
                            itemResult.Unit = "nm"; // Manual 참조
                            break;
                        case TestItemType.CIEX:
                            value = result.CIEX;
                            itemResult.RawData = value;
                            itemResult.Value = value;
                            itemResult.Unit = ""; // 무차원
                            break;
                        case TestItemType.CIEY:
                            value = result.CIEY;
                            itemResult.RawData = value;
                            itemResult.Value = value;
                            itemResult.Unit = ""; // 무차원
                            break;
                        case TestItemType.CIEZ:
                            value = result.CIEZ;
                            itemResult.RawData = value;
                            itemResult.Value = value;
                            itemResult.Unit = ""; // 무차원
                            break;
                        case TestItemType.CIEU:
                            value = result.CIEU;
                            itemResult.RawData = value;
                            itemResult.Value = value;
                            itemResult.Unit = ""; // 무차원
                            break;
                        case TestItemType.CIEV1976:
                            value = result.CIEV1976;
                            itemResult.RawData = value;
                            itemResult.Value = value;
                            itemResult.Unit = ""; // 무차원
                            break;
                        case TestItemType.CIEV1960:
                            value = result.CIEV1960;
                            itemResult.RawData = value;
                            itemResult.Value = value;
                            itemResult.Unit = ""; // 무차원
                            break;
                        case TestItemType.LambdaDom:
                            value = result.LambdaDom;
                            itemResult.RawData = value;
                            itemResult.Value = value;
                            itemResult.Unit = "nm"; // Manual 참조
                            break;
                        case TestItemType.Purity:
                            value = result.Purity;
                            itemResult.RawData = value;
                            itemResult.Value = value;
                            itemResult.Unit = ""; // 무차원
                            break;
                        case TestItemType.CCT:
                            value = result.CCT;
                            itemResult.RawData = value;
                            itemResult.Value = value;
                            itemResult.Unit = ""; // 무차원
                            break;
                        case TestItemType.CRI:
                            value = result.CRI;
                            itemResult.RawData = value;
                            itemResult.Value = value;
                            itemResult.Unit = ""; // 무차원
                            break;
                        case TestItemType.Centroid:
                            value = result.Centroid;
                            itemResult.RawData = value;
                            itemResult.Value = value;
                            itemResult.Unit = "nm"; // Manual 참조
                            break;
                        case TestItemType.StimulusX:
                            value = result.StimulusX;
                            itemResult.RawData = value;
                            itemResult.Value = value;
                            itemResult.Unit = ""; // 무차원
                            break;
                        case TestItemType.StimulusY:
                            value = result.StimulusY;
                            itemResult.RawData = value;
                            itemResult.Value = value;
                            itemResult.Unit = ""; // 무차원
                            break;
                        case TestItemType.StimulusZ:
                            value = result.StimulusZ;
                            itemResult.RawData = value;
                            itemResult.Value = value;
                            itemResult.Unit = ""; // 무차원
                            break;
                        case TestItemType.PickValue:
                            value = result.StimulusX;
                            itemResult.RawData = value;
                            itemResult.Value = value;
                            itemResult.Unit = intensityUnit;
                            break;
                        case TestItemType.ADC:
                            value = result.ADC;
                            itemResult.RawData = value;
                            itemResult.Value = value;
                            itemResult.Unit = ""; // 무차원
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                return false;
            }
            return true;
        }
        #endregion

        #region Methods
        public int ApplyParameter()
        {
            int ret = 0;
            do
            {
                if (!IsCreated())
                {
                    ret = -1;
                    break;
                }
                if (!ApplyMeasurementCondition())
                {
                    ret = -1;
                    break;
                }
            }
            while (false);
            return ret;
        }
        public int Measure()
        {
            if (Config.IsSimulated)
            {
                return MeasureSimulation();
            }

            int ret = 0;
            do
            {
                if (!IsCreated())
                {
                    ret = -1;
                    break;
                }
                if (!SendMeasureCommand())
                {
                    ret = -1;
                    break;
                }
            }
            while (false);
            return ret;
        }
        public int MeasureDarkCurrent()
        {
            int ret = 0;
            do
            {
                if (!IsCreated())
                {
                    ret = -1;
                    break;
                }
                if (!SendMeasureDarkCurrentComand())
                {
                    ret = -1;
                    break;
                }
            }
            while (false);
            return ret;
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
            CheckCASErrorAndThrow(CAS4DLL.casGetError(deviceId));
        }
        #endregion

        // Device Management
        #region Device Management Methods
        private bool CreateDevice()
        {
            if (IsCreated())
                return true;

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

                int newId = CAS4DLL.casCreateDeviceEx(deviceInterfaceType, deviceInterfaceOption);
                CheckCASErrorAndThrow(newId);

                // Processing after successful device initialization
                UpdateSupportDensityFilterList();

                deviceId = newId;
                return true;
            }
            catch (Exception ex)
            {
                Log.Write(this, ex.Message);
                return false;
            }
        }        
        private bool TerminateDevice()
        {
            try
            {
                bool tryDoneDevice = false;
                if(IsCreated())
                {
                    CheckCASErrorAndThrow(CAS4DLL.casDoneDevice(deviceId));
                    tryDoneDevice = true;
                }
                deviceId = -1;
                deviceInfo.Clear();

                if (tryDoneDevice)
                {
                    OnDeviceTerminated?.Invoke(this);
                }
                return true;
            }
            catch (Exception ex)
            {
                Log.Write(this, ex.Message);
                return false;
            }
        }
        private bool InitializeDevice()
        {
            if (!IsCreated())
                return false;

            try
            {
                // Set parameter
                SetDeviceParameter(CAS4DLL.dpidConfigFileName, Config.ConfigFileName);
                SetDeviceParameter(CAS4DLL.dpidCalibFileName, Config.CalibFileName);

                // Device Initialize
                CheckCASErrorAndThrow(CAS4DLL.casInitialize(deviceId, CAS4DLL.InitOnce));
                GetDeviceParameter(CAS4DLL.dpidCalibrationUnit, ref intensityUnit);

                // Set Measurement Condition
                ApplyMeasurementCondition();

                // Set trigger mode
                if (Config.UseHardwareTrigger)
                {
                    SetMeasurementParameter(CAS4DLL.mpidTriggerSource, CAS4DLL.trgFlipFlop);
                    SetMeasurementParameter(CAS4DLL.mpidTriggerTimeout, Config.TriggerTimeout);
                    SetDeviceParameter(CAS4DLL.dpidLine1FlipFlop, 0);
                    //PrepareMeasureAndTrigger();
                    useHardwareTrigger = true;
                }
                else
                {
                    SetMeasurementParameter(CAS4DLL.mpidTriggerSource, CAS4DLL.trgSoftware);
                    useHardwareTrigger = false;
                }

                // Get Device Information
                string devName = "";
                string devSerial = "";
                double devInfType = 0;
                double devInfOption = 0;

                GetDeviceParameter(CAS4DLL.dpidSpectrometerName, ref devName);
                GetDeviceParameter(CAS4DLL.dpidSerialNo, ref devSerial);
                GetDeviceParameter(CAS4DLL.dpidInterfaceType, ref devInfType);
                GetDeviceParameter(CAS4DLL.dpidInterfaceOption, ref devInfOption);

                deviceInfo.Name = devName + (useHardwareTrigger ? " (with H/W Trigger)" : "");
                deviceInfo.SerialNumber = devSerial;

                switch((int)devInfType)
                {
                    case CAS4DLL.InterfacePCI:
                        deviceInfo.InterfaceType = "PCI";
                        break;
                    case CAS4DLL.InterfaceTest:
                        deviceInfo.InterfaceType = "Test";
                        break;
                    case CAS4DLL.InterfaceUSB:
                        deviceInfo.InterfaceType = "USB";
                        break;
                    case CAS4DLL.InterfacePCIe:
                        deviceInfo.InterfaceType = "PCIe";
                        break;
                    case CAS4DLL.InterfaceEthernet:
                        deviceInfo.InterfaceType = "Ethernet";
                        break;
                    default:
                        deviceInfo.InterfaceType = "Unknown";
                        break;
                }

                deviceInfo.InterfaceOption = devInfOption.ToString();
                OnDeviceCreated?.Invoke(this);
                return true;
            }
            catch (Exception ex)
            {
                // Error handling
                deviceInfo.Clear();

                Log.Write(this, ex.Message);
                return false;
            }
        }
        //private void PrepareMeasureAndTrigger()
        //{
        //    if (!IsCreated())
        //        return;
        //    try
        //    {
        //        SetMeasurementParameter(CAS4DLL.mpidTriggerTimeout, 1000);
        //        CheckCASErrorAndThrow(CAS4DLL.casMeasure(deviceId));
        //    }
        //    catch {}
        //    finally
        //    {
        //        try
        //        {
        //            SetMeasurementParameter(CAS4DLL.mpidTriggerTimeout, Config.TriggerTimeout);
        //        }
        //        catch {}
        //    }
        //}

        public bool IsCreated()
        {
            return (deviceId >= 0);
        }
        
        #endregion

        // Measurement Methods
        #region Measurement Methods
        private bool SendMeasureCommand()
        {
            try
            {
                // Data clear
                result.Clear();
                spectrum.Clear();

                // SPC(2) Line ON ( SMU(3))
                if (useHardwareTrigger)
                    OnDigitalOut(2);

                //CAS4DLL.casPerformActionEx(deviceId, CAS4DLL.paPrepareMeasurement, 0, 0, (IntPtr)null);

                OnMeasureCommandSended?.Invoke(this);
                CheckCASErrorAndThrow(CAS4DLL.casMeasure(deviceId));

                // SPC(2) Line OFF (Send Falling edge to SMU(3))
                if (useHardwareTrigger)
                    OffDigitalOut(2);

                // Data Process
                GetMeasureData();
                GetSpectrumData();
                OnMeasureCompleted?.Invoke(this);
                return true;
            }
            catch (Exception ex)
            {
                // Error handling
                result.Clear();
                spectrum.Clear();
                OnMeasureFailed?.Invoke(this, ex.Message);

                Log.Write(this, ex.Message);
                return false;
            }
            finally
            {
                try
                {
                    // if abort measurement -> SPC(2) Line OFF
                    if (useHardwareTrigger)
                        OffDigitalOut(2);
                }
                catch {}
            }
        }
        private bool SendMeasureDarkCurrentComand()
        {
            try
            {
                CloseShutter();                
                CheckCASErrorAndThrow(CAS4DLL.casMeasureDarkCurrent(deviceId));
                OpenShutter();
                return true;
            }
            catch (Exception ex)
            {
                Log.Write(this, ex.Message);
                return false;
            }
            finally
            {
                try 
                { 
                    OpenShutter(); 
                } 
                catch {}
            }
        }
        private void GetMeasureData()
        {
            // Color metric
            CheckCASErrorAndThrow(CAS4DLL.casColorMetric(deviceId));

            // Photometric and Radiometric Integral
            StringBuilder sb = new StringBuilder(256);

            double photInt = 0.0;
            CAS4DLL.casGetPhotInt(deviceId, out photInt, sb, sb.Capacity);
            CheckCASErrorAndThrow();
            result.PhotInt = photInt;
            result.PhotIntUnit = sb.ToString();

            double radInt = 0.0;
            CAS4DLL.casGetRadInt(deviceId, out radInt, sb, sb.Capacity);
            CheckCASErrorAndThrow();
            result.RadInt = radInt;
            result.RadIntUnit = sb.ToString();

            // color coordinates
            double CIEX = 0.0, CIEY = 0.0, CIEZ = 0.0, CIEU = 0.0, CIEV1976 = 0.0, CIEV1960 = 0.0;
            CAS4DLL.casGetColorCoordinates(deviceId, ref CIEX, ref CIEY, ref CIEZ, ref CIEU, ref CIEV1976, ref CIEV1960);
            CheckCASErrorAndThrow();
            result.CIEX = CIEX;
            result.CIEY = CIEY;
            result.CIEZ = CIEZ;
            result.CIEU = CIEU;
            result.CIEV1976 = CIEV1976;
            result.CIEV1960 = CIEV1960;

            // Tristimulus
            double stimulusX = 0.0, stimulusY = 0.0, stimulusZ = 0.0;
            CAS4DLL.casGetTriStimulus(deviceId, ref stimulusX, ref stimulusY, ref stimulusZ);
            result.StimulusX = stimulusX;
            result.StimulusY = stimulusY;
            result.StimulusZ = stimulusZ;

            // calulate lambda dominant wavelength and purity
            double lambdaDom = 0.0, purity = 0.0;
            CheckCASErrorAndThrow(CAS4DLL.casCalculateLambdaDom(deviceId, 1.0 / 3.0, 1.0 / 3.0, ref lambdaDom, ref purity));
            result.LambdaDom = lambdaDom;
            result.Purity = purity;

            // Wavelength and peak value
            double WP = 0.0, pickValue = 0.0;
            CAS4DLL.casGetPeak(deviceId, out WP, out pickValue);
            CheckCASErrorAndThrow();
            result.WP = WP;
            result.PickValue = pickValue;
            result.Centroid = CAS4DLL.casGetCentroid(deviceId);
            result.FWHM = CAS4DLL.casGetWidth(deviceId);

            // color rendering index
            CAS4DLL.casCalculateCRI(deviceId);
            result.CRI = CAS4DLL.casGetCRI(deviceId, 0);

            // color temperature
            result.CCT = CAS4DLL.casGetCCT(deviceId);

            // Maximum ADC value
            result.ADC = (int)CAS4DLL.casGetMeasurementParameter(deviceId, CAS4DLL.mpidMaxADCValue);
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

            spectrum.WaveLength = new double[visiblePixels];
            spectrum.Intensity = new double[visiblePixels];

            for (int i = 0; i < spectrum.WaveLength.Length; i++)
            {
                spectrum.Intensity[i] = CAS4DLL.casGetData(deviceId, i + deadPixels);
                spectrum.MinimumIntensity = Math.Min(spectrum.MinimumIntensity, spectrum.Intensity[i]);
                spectrum.MaximumIntensity = Math.Max(spectrum.MaximumIntensity, spectrum.Intensity[i]);

                spectrum.WaveLength[i] = CAS4DLL.casGetXArray(deviceId, i + deadPixels);
            }
        }
        #endregion

        #region Control Methods
        private void OnDigitalOut(int port)
        {
            CAS4DLL.casSetDigitalOut(deviceId, port, 1);
            CheckCASErrorAndThrow();
        }
        private void OffDigitalOut(int port)
        {
            CAS4DLL.casSetDigitalOut(deviceId, port, 0);
            CheckCASErrorAndThrow();
        }
        private void OpenShutter()
        {
            if (!IsCreated())
                return;

            CAS4DLL.casSetShutter(deviceId, CAS4DLL.casShutterOpen);
            CheckCASErrorAndThrow();
        }
        private void CloseShutter()
        {
            if (!IsCreated())
                return;

            CAS4DLL.casSetShutter(deviceId, CAS4DLL.casShutterClose);
            CheckCASErrorAndThrow();
        }
        #endregion

        #region Parameter Methods
        private void SetDeviceParameter(int what, double value)
        {
            if (!IsCreated())
                throw new InvalidOperationException("The Device was not created.");

            int result = CAS4DLL.casSetDeviceParameter(deviceId, what, value);
            CheckCASErrorAndThrow(result);
        }
        private void SetDeviceParameter(int what, string value)
        {
            if (!IsCreated())
                throw new InvalidOperationException("The Device was not created.");

            int result = CAS4DLL.casSetDeviceParameterString(deviceId, what, value);
            CheckCASErrorAndThrow(result);
        }
        private void GetDeviceParameter(int what, ref double value)
        {
            if (!IsCreated())
                throw new InvalidOperationException("The Device was not created.");

            double result = CAS4DLL.casGetDeviceParameter(deviceId, what);
            CheckCASErrorAndThrow();
            value = result;
        }
        private void GetDeviceParameter(int what, ref string value)
        {
            if (!IsCreated())
                throw new InvalidOperationException("The Device was not created.");

            StringBuilder sb = new StringBuilder(256);
            int result = CAS4DLL.casGetDeviceParameterString(deviceId, what, sb, sb.Capacity);
            CheckCASErrorAndThrow(result);
            value = sb.ToString();
        }
        private void SetMeasurementParameter(int what, double value)
        {
            if (!IsCreated())
                throw new InvalidOperationException("The Device was not created.");

            int result = CAS4DLL.casSetMeasurementParameter(deviceId, what, value);
            CheckCASErrorAndThrow(result);
        }
        private void GetMeasurementParameter(int what, ref double value)
        {
            if (!IsCreated())
                throw new InvalidOperationException("The Device was not created.");

            double result = CAS4DLL.casGetMeasurementParameter(deviceId, what);
            CheckCASErrorAndThrow();
            value = result;
        }
        private bool LoadMeasurementCondition()
        {
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

                Config.IntegrationTime = (int)Math.Round(integrationTime);
                Config.Averages = (int)Math.Round(averages);
                Config.DensityFilter = (int)Math.Round(densityFilter);
                Config.ColormetricStart = (int)Math.Round(colormetricStart);
                Config.ColormetricStop = (int)Math.Round(colormetricStop);
                Config.TriggerTimeout = (int)Math.Round(triggerTimeout);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        private bool ApplyMeasurementCondition()
        {
            try
            {
                SetMeasurementParameter(CAS4DLL.mpidIntegrationTime, Config.IntegrationTime);
                SetMeasurementParameter(CAS4DLL.mpidAverages, Config.Averages);
                SetMeasurementParameter(CAS4DLL.mpidDensityFilter, Config.DensityFilter);
                SetMeasurementParameter(CAS4DLL.mpidColormetricStart, Config.ColormetricStart);
                SetMeasurementParameter(CAS4DLL.mpidColormetricStop, Config.ColormetricStop);
                SetMeasurementParameter(CAS4DLL.mpidObserver, CAS4DLL.cieObserver1931);
                SetMeasurementParameter(CAS4DLL.mpidTriggerTimeout, Config.TriggerTimeout);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        #endregion

        #region Density Filter Methods
        private void UpdateSupportDensityFilterList()
        {
            DensityFilterList.Clear();
            if (IsCreated())
            {
                StringBuilder stringBuilder = new StringBuilder(256);
                for (int i = 0; i < 7; i++)
                {
                    CAS4DLL.casGetFilterName(deviceId, i, stringBuilder, stringBuilder.Capacity);
                    string filterName = stringBuilder.ToString();
                    if (string.IsNullOrEmpty(filterName) || filterName == "none")
                        continue;

                    DensityFilter densityFilter = new DensityFilter();
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
            foreach (var filter in DensityFilterList)
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
            foreach (var filter in DensityFilterList)
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
            foreach (var filter in DensityFilterList)
            {
                filterNames.Add(filter.Name);
            }
            return filterNames;
        }
        #endregion

        #region Simulation Methods
        private void GetSimulationMeasureData()
        {
            var rand = new Random();

            // 파장 관련 (nm)
            result.WP = rand.NextDouble() * (700 - 400) + 400; // 400~700nm
            result.FWHM = rand.NextDouble() * (60 - 10) + 10;  // 10~60nm
            result.LambdaDom = rand.NextDouble() * (700 - 400) + 400; // 400~700nm
            result.Centroid = rand.NextDouble() * (700 - 400) + 400; // 400~700nm

            // 색좌표 (0~1)
            result.CIEX = rand.NextDouble();
            result.CIEY = rand.NextDouble();
            result.CIEZ = rand.NextDouble();
            result.CIEU = rand.NextDouble();
            result.CIEV1976 = rand.NextDouble();
            result.CIEV1960 = rand.NextDouble();

            // 삼자극치 (0~100)
            result.StimulusX = rand.NextDouble() * 100;
            result.StimulusY = rand.NextDouble() * 100;
            result.StimulusZ = rand.NextDouble() * 100;

            // 순도 (0~1)
            result.Purity = rand.NextDouble();

            // 광도, 방사조도 (0~10000)
            result.PhotInt = rand.NextDouble() * 10000;
            result.PhotIntUnit = "lm";
            result.RadInt = rand.NextDouble() * 10000;
            result.RadIntUnit = "W";

            // 색온도 (2000~10000K)
            result.CCT = rand.NextDouble() * (10000 - 2000) + 2000;

            // 연색성 (CRI, 0~100)
            result.CRI = rand.NextDouble() * 100;

            // 최대값 (0~10000)
            result.PickValue = rand.NextDouble() * 10000;

            // ADC (0~65535)
            result.ADC = rand.Next(0, 65536);
        }
        private void GetSimulationSpectrumData()
        {
            var rand = new Random();
            int pixelCount = 1024;
            spectrum.WaveLength = new double[pixelCount];
            spectrum.Intensity = new double[pixelCount];
            spectrum.MaximumIntensity = 0;
            double startWavelength = 200.0; // 시작 파장 (nm)
            double endWavelength = 1100.0;  // 끝 파장 (nm)
            double wavelengthStep = (endWavelength - startWavelength) / pixelCount;

            double mean = 550.0; // 중심 파장 (nm)
            double stddev = 40.0; // 표준편차 (nm)
            double amplitude = 10000.0; // 최대 세기

            for (int i = 0; i < pixelCount; i++)
            {
                double wl = startWavelength + i * wavelengthStep;
                spectrum.WaveLength[i] = wl;

                // 가우시안 파형
                double gauss = amplitude * Math.Exp(-0.5 * Math.Pow((wl - mean) / stddev, 2));

                // 노이즈 추가 (정규분포, 표준편차는 최대값의 2% 수준)
                double noise = (rand.NextDouble() * 2.0 - 1.0) * amplitude * 0.02;

                double intensity = gauss + noise;
                if (intensity < 0) intensity = 0;

                spectrum.Intensity[i] = intensity;
                if (intensity > spectrum.MaximumIntensity)
                    spectrum.MaximumIntensity = intensity;
            }
        }
        private int MeasureSimulation()
        {
            OnMeasureCommandSended?.Invoke(this);
            GetSimulationMeasureData();
            GetSimulationSpectrumData();
            OnMeasureCompleted?.Invoke(this);
            return 0;
        }
        #endregion

        #endregion
    }
}
