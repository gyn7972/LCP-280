using InstrumentSystems.CAS4;
using QMC.Common.Component;
using QMC.Common.PKGTester;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
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
            public double ADC {  get; set; }
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
                ADC = 0;
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
        public MeasurementData result = new MeasurementData();
        public SpectrumData spectrum = new SpectrumData();

        private List<TestConditionItem> testItems = new List<TestConditionItem>();
        private Dictionary<string, TestItemResult> results = new Dictionary<string, TestItemResult>();
        private string intensityUnit = "";
        
        private DeviceInformation deviceInfo = new DeviceInformation();
        private bool useHardwareTrigger = false;
        private bool isInitialized = false;

        private readonly object measureSync = new object();
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
                if (!SendMeasureDarkCurrentComand())
                {
                    ret = -1;
                    break;
                }

                isInitialized = true;
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
                        //case TestItemType.RadInt:
                        //    {
                        //        double v = result.RadInt;
                        //        string u = (result.RadIntUnit ?? "").Trim();

                        //        // unit이 W면 mW로 변환해서 통일
                        //        if (u.Equals("W", StringComparison.OrdinalIgnoreCase))
                        //        {
                        //            v = v * 1000.0;
                        //            u = "mW";
                        //        }
                        //        // unit이 mW면 그대로
                        //        else if (u.Equals("mW", StringComparison.OrdinalIgnoreCase))
                        //        {
                        //            // keep
                        //        }
                        //        else
                        //        {
                        //            // unit이 애매하면 일단 값 그대로 + unit 그대로 (로그로 판단)
                        //        }

                        //        itemResult.RawData = v;
                        //        itemResult.Value = v;
                        //        itemResult.Unit = u;
                        //    }
                        //    break;
                        case TestItemType.RadInt: //A로 들어와서 *1000하면 mA
                            value = result.RadInt * 1000;       // 단위에 맞춰서 뿌려야 하니깐.. // 여긴 우선 이렇게 해야되네...
                            itemResult.RawData = value;
                            itemResult.Value = value;
                            itemResult.Unit = result.RadIntUnit;
                            //itemResult.Unit = "mW"; // Manual 참조
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
                        case TestItemType.LambdaDom:    // 단위에 맞춰서 뿌려야 하니깐.. // 여긴 걍하는디.
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
                            value = result.PickValue;
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
        public bool IsReady()
        {
            if (!Config.IsSimulation)
            {
                if (IsCreated())
                {
                    try
                    {
                        double value = 0;
                        GetDeviceParameter(CAS4DLL.dpidInitialized, ref value);
                        return (value > 0);
                    }
                    catch (Exception)
                    {
                        return false;
                    }
                }
            }
            return true;
        }
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
            lock (measureSync)
            {
                if (Config.IsSimulation)
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
        }

        public bool AbortMeasurementSafe()
        {
            lock (measureSync)
            {
                try
                {
                    if (!IsCreated())
                        return true;

                    try
                    {
                        OffDigitalOut(2);
                    }
                    catch (Exception ex)
                    {
                        Log.Write("CAS", "AbortMeasurementSafe", $"OffDigitalOut(2) failed: {ex.Message}");
                    }

                    try
                    {
                        SetDeviceParameter(CAS4DLL.dpidLine1FlipFlop, 0);
                    }
                    catch (Exception ex)
                    {
                        Log.Write("CAS", "AbortMeasurementSafe", $"Reset FlipFlop failed: {ex.Message}");
                    }

                    result.Clear();
                    spectrum.Clear();

                    foreach (var key in results.Keys.ToList())
                    {
                        results[key].Reset();
                    }

                    Log.Write("CAS", "AbortMeasurementSafe", "Measurement state cleaned.");
                    return true;
                }
                catch (Exception ex)
                {
                    Log.Write("CAS", "AbortMeasurementSafe", ex.Message);
                    return false;
                }
            }
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
        public int ApplyParameterAndMeasureDarkCurrent()
        {
            int ret = 0;
            do
            {
                if(!IsCreated())
                {
                    ret = -1;
                    break;
                }
                if (!ApplyMeasurementCondition())
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

                string errorMsg = $"CAS Error {errorCode}: {sb.ToString()}";

                // 장비 로그에 기록 (현재 클래스의 Log.Write 사용 패턴 따름)
                Log.Write("CAS", "CheckCASErrorAndThrow", errorMsg);
                throw new Exception(errorMsg);
                //// An error occurred
                //StringBuilder sb = new StringBuilder(256);
                //CAS4DLL.casGetErrorMessage(errorCode, sb, sb.Capacity);
                //throw new Exception($"CAS Error {errorCode}: {sb.ToString()}");
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
                    //CAS 5.0에서는 PCI Type 지원 안함.
                    //case CASSpectrometerConfig.DeviceInterface.PCI:
                    //    deviceInterfaceType = CAS4DLL.InterfacePCI;
                    //    break;

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

                int newId = 0;
                dynamic eq = EquipmentLocator.Instance;
                var bIsSim = eq.EquipmentConfig.IsSimulation;
                if (bIsSim)
                {
                    CheckCASErrorAndThrow(newId);
                    // Processing after successful device initialization
                    UpdateSupportDensityFilterList();

                    deviceId = newId;
                    return true;
                }
                else
                {
                    newId = CAS4DLL.casCreateDeviceEx(deviceInterfaceType, deviceInterfaceOption);
                }

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
                    if(Config.IsSimulation == false)
                    {
                        CheckCASErrorAndThrow(CAS4DLL.casDoneDevice(deviceId));
                    }
                    tryDoneDevice = true;
                }
                deviceId = -1;
                deviceInfo.Clear();

                if (tryDoneDevice)
                {
                    OnDeviceTerminated?.Invoke(this);
                }

                isInitialized = false;
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
                if (Config.IsSimulation)
                    return false;
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

                    // [추가] C++과 동일하게 ACQ 및 Busy 상태 라인 지정 (CAS4DLL에 mpid 상수가 없다면 번호 직접 입력)
                    // mpidACQStateLine = 32, mpidBusyStateLine = 33, mpidACQStateLinePolarity = 34, mpidBusyStateLinePolarity = 35
                    SetMeasurementParameter(CAS4DLL.mpidACQStateLine, 1); // mpidACQStateLine
                    SetMeasurementParameter(CAS4DLL.mpidBusyStateLine, 2); // mpidBusyStateLine
                    SetMeasurementParameter(CAS4DLL.mpidACQStateLinePolarity, 0); // mpidACQStateLinePolarity
                    SetMeasurementParameter(CAS4DLL.mpidBusyStateLinePolarity, 0); // mpidBusyStateLinePolarity

                    //add 2025-12-06
                    SetMeasurementParameter(CAS4DLL.mpidTriggerDelayTime, 0);// ms  // 1-> 0 으로
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
                    // CAS 5.0에서는 PCI Type 지원 안함.   
                    //case CAS4DLL.InterfacePCI:
                    //    deviceInfo.InterfaceType = "PCI";
                    //    break;
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
        public bool IsCreated()
        {
            return (deviceId >= 0);
        }
        #endregion

        // Measurement Methods
        #region Measurement Methods
        private bool SendMeasureCommand()
        {
            bool do2On = false;

            try
            {
                result.Clear();
                spectrum.Clear();

                //LogMeasureConditionSnapshot("MeasureStart");

                if (useHardwareTrigger)
                {
                    CheckCASErrorAndThrow(
                        CAS4DLL.casPerformActionEx(deviceId, CAS4DLL.paPrepareMeasurement, 0, 0, IntPtr.Zero)
                    );

                    SetMeasurementParameter(CAS4DLL.mpidTriggerOptions, 0x20);
                    SetDeviceParameter(CAS4DLL.dpidLine1FlipFlop, 0);

                    //Log.Write("CAS", "HWTrigger",
                    //    $"Prepared. ACQLine=1, BusyLine=2, TrgOpt=0x20, FlipFlopReset, TrgDelay={GetTriggerDelayTimeMs()}ms");
                }

                OnMeasureCommandSended?.Invoke(this);

                if (useHardwareTrigger)
                {
                    //Log.Write("CAS", "SPC", "DO2 ON (before casMeasure)");
                    OnDigitalOut(2);
                    do2On = true;
                }

                var sw = Stopwatch.StartNew();
                CheckCASErrorAndThrow(CAS4DLL.casMeasure(deviceId));
                sw.Stop();

                //Log.Write("CAS", "MeasureTime",
                //    $"casMeasure elapsed={sw.ElapsedMilliseconds}ms, HWTrg={useHardwareTrigger}");

                // 중요:
                // SMU가 기다리는 falling edge를 측정 완료 직후 바로 내려준다.
                if (useHardwareTrigger && do2On)
                {
                    //Log.Write("CAS", "SPC", "DO2 OFF (immediately after casMeasure)");
                    OffDigitalOut(2);
                    do2On = false;
                }

                GetSpectrumData();
                GetMeasureData();

                //var diag = CalcSpectrumDiagnostics(250.0, 290.0, 5);
                //LogSpectrumDiagnostics("SpectrumDiag", diag);
                //LogMeasureResultsSummary("MeasureResult");

                OnMeasureCompleted?.Invoke(this);
                return true;
            }
            catch (Exception ex)
            {
                try
                {
                    if (useHardwareTrigger)
                        SetDeviceParameter(CAS4DLL.dpidLine1FlipFlop, 0);
                }
                catch { }

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
                    if (useHardwareTrigger && do2On)
                    {
                        OffDigitalOut(2);
                    }
                }
                catch { }
            }
        }

        //private bool SendMeasureCommand()
        //{
        //    try
        //    {
        //        // Data clear
        //        result.Clear();
        //        spectrum.Clear();

        //        // [ADD] 조건 스냅샷 로그
        //        LogMeasureConditionSnapshot("MeasureStart");

        //        // 20251206 수정. 
        //        if (useHardwareTrigger)
        //        {
        //            CheckCASErrorAndThrow(
        //            CAS4DLL.casPerformActionEx(deviceId, CAS4DLL.paPrepareMeasurement, 0, 0, IntPtr.Zero)
        //            );

        //            // [추가] C++의 casSetMeasurementParameter(m_nCasID, mpidTriggerOptions, toShowACQState) 동일 적용
        //            // toShowACQState 값은 보통 0x00000020 (32) 입니다. (CAS API 문서 참고)
        //            SetMeasurementParameter(CAS4DLL.mpidTriggerOptions, 0x20);

        //            // C++과 같이 1번 핀(Line1FlipFlop) 리셋하여 Trigger 대기 상태 돌입
        //            SetDeviceParameter(CAS4DLL.dpidLine1FlipFlop, 0);

        //            // [ADD] HW Trigger 준비 상태 로그
        //            Log.Write("CAS", "HWTrigger", 
        //                $"Prepared. ACQLine=1, BusyLine=2, " +
        //                $"TrgOpt=0x20, FlipFlopReset, TrgDelay={GetTriggerDelayTimeMs()}ms");
        //        }

        //        // HW Trigger면 이 casMeasure()가 외부 트리거를 기다렸다가 적분한다.
        //        OnMeasureCommandSended?.Invoke(this);

        //        // SPC(2) Line ON ( SMU(3))
        //        // [C++ 동일 처리] 측정 명령이 보내질 때마다 SPC(2) Line ON (Send Rising edge to SMU(3))
        //        // test 후에 사용 유/무 결정. ( 현재는 이거 안하면 측정 안하는디 )
        //        if (useHardwareTrigger)
        //        {
        //            Log.Write("CAS", "SPC", "DO2 ON (before casMeasure)");
        //            OnDigitalOut(2);
        //        }

        //        var sw = Stopwatch.StartNew();
        //        // HW Trigger면 이 casMeasure()가 외부 트리거를 기다렸다가 적분한다.
        //        CheckCASErrorAndThrow(CAS4DLL.casMeasure(deviceId));
        //        sw.Stop();
        //        Log.Write("CAS", "MeasureTime", 
        //            $"casMeasure elapsed={sw.ElapsedMilliseconds}ms, HWTrg={useHardwareTrigger}");
        //        //// SPC(2) Line OFF (Send Falling edge to SMU(3))
        //        //if (useHardwareTrigger)
        //        //{
        //        //OffDigitalOut(2);
        //        //}
        //        // Data Process
        //        GetSpectrumData();
        //        GetMeasureData();

        //        // [ADD] 스펙트럼 진단 + 결과 요약 로그
        //        var diag = CalcSpectrumDiagnostics(250.0, 290.0, 5);
        //        LogSpectrumDiagnostics("SpectrumDiag", diag);
        //        LogMeasureResultsSummary("MeasureResult");

        //        // SPC(2) Line OFF (Send Falling edge to SMU(3))
        //        // [C++ 동일 처리] 측정 명령이 보내질 때마다 SPC(2) Line ON (Send Rising edge to SMU(3))
        //        // test 후에 사용 유/무 결정. ( 현재는 이거 안하면 측정 안하는디 )
        //        if (useHardwareTrigger)
        //        {
        //            Log.Write("CAS", "SPC", "DO2 OFF (after readout)");
        //            OffDigitalOut(2);
        //        }

        //        OnMeasureCompleted?.Invoke(this);

        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        // Error handling
        //        result.Clear();
        //        spectrum.Clear();
        //        OnMeasureFailed?.Invoke(this, ex.Message);

        //        Log.Write(this, ex.Message);
        //        return false;
        //    }
        //    finally
        //    {
        //        try
        //        {
        //            // if abort measurement -> SPC(2) Line OFF
        //            // [C++ 동일 처리] 측정 명령이 보내질 때마다 SPC(2) Line ON (Send Rising edge to SMU(3))
        //            // test 후에 사용 유/무 결정. ( 현재는 이거 안하면 측정 안하는디 )
        //            if (useHardwareTrigger)
        //            {
        //               OffDigitalOut(2);
        //            }
        //        }
        //        catch {}
        //    }
        //}

        private double _lastDarkLowMean = double.NaN;
        private DateTime _lastDarkTime = DateTime.MinValue;
        private bool SendMeasureDarkCurrentComand()
        {
            bool ok = false;
            try
            {
                CloseShutter();                
                CheckCASErrorAndThrow(CAS4DLL.casMeasureDarkCurrent(deviceId));
                ok = true;

                _lastDarkTime = DateTime.Now;
                Log.Write("CAS", "DarkCurrent", $"Dark measured at {_lastDarkTime:HH:mm:ss}");
                return true;
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                return false;
            }
            finally
            {
                try 
                { 
                    OpenShutter(); 
                } 
                catch {}

                if (!ok)
                    Log.Write("CAS", "DarkCurrent", "Dark measure FAILED");
            }
        }


        public double IlluminantRefX { get; set; } = 0.33333; // WD 기준 X
        public double IlluminantRefY { get; set; } = 0.33333; // WD 기준 Y
        public bool UseFullRangeForWP { get; set; } = true;   // WP 측정 시 Full Range 사용 여부
        public bool ConvertWattToMilliWatt { get; set; } = false; // WATT를 mW로 변환할지 여부
        public bool ConvertPurityToPercent { get; set; } = false; // Purity를 %로 변환할지 여부

        private void GetMeasureData()
        {
            // C++처럼 계산 전 파장 대역(Start/Stop) 확실히 적용
            SetMeasurementParameter(CAS4DLL.mpidColormetricStart, Config.ColormetricStart);
            SetMeasurementParameter(CAS4DLL.mpidColormetricStop, Config.ColormetricStop);

            // Color metric
            CheckCASErrorAndThrow(CAS4DLL.casColorMetric(deviceId));

            // Photometric and Radiometric Integral
            StringBuilder sb = new StringBuilder(256);

            // Photometric (광도/광속)
            double photInt = 0.0;
            CAS4DLL.casGetPhotInt(deviceId, out photInt, sb, sb.Capacity);
            CheckCASErrorAndThrow();
            result.PhotInt = photInt;
            result.PhotIntUnit = sb.ToString();
            //Log.Write("CAS", "PhotInt", $"Value={result.PhotInt}, " +
            //    $"Unit={result.PhotIntUnit}, " +
            //    $"Start={Config.ColormetricStart}, " +
            //    $"Stop={Config.ColormetricStop}");

            // Radiometric (방사속/WATT)
            sb.Clear();
            double radInt = 0.0;
            CAS4DLL.casGetRadInt(deviceId, out radInt, sb, sb.Capacity);
            CheckCASErrorAndThrow();
            // [C++ 동일 처리] WATT 단위 변환 옵션 (W -> mW)
            if (ConvertWattToMilliWatt)
            {
                result.RadInt = radInt * 1000.0;
                result.RadIntUnit = "mW"; // 단위 명시적 변경
            }
            else
            {
                result.RadInt = radInt;
                result.RadIntUnit = sb.ToString();
            }
            //result.RadInt = radInt;       // C++에서 mW 단위가 필요하다면 여기서 * 1000 처리 가능
            //result.RadIntUnit = sb.ToString();
            //Log.Write("CAS", "RadInt", $"Value={result.RadInt}, " +
            //   $"Unit={result.RadIntUnit}, " +
            //   $"Start={Config.ColormetricStart}, " +
            //   $"Stop={Config.ColormetricStop}");

            //전체 영역 WATT
            //result.RadInt = GetWattRadInt(deviceId);
            //CheckCASErrorAndThrow();
            //result.RadIntUnit = sb.ToString();
            //Log.Write("CAS", "RadInt", $"Value={result.RadInt}, " +
            //    $"Unit={result.RadIntUnit}, " +
            //    $"Start={Config.ColormetricStart}, " +
            //    $"Stop={Config.ColormetricStop}");
            /////////////////////////////////////////////////////////////////
            ////CAS4DLL.casGetRadInt(deviceId, out radInt, sb, sb.Capacity);
            //CheckCASErrorAndThrow();
            //result.RadInt = radInt;
            //result.RadIntUnit = sb.ToString();
            //WATT Test
            //result.RadInt = GetWatt(deviceId);
            //result.RadIntUnit = sb.ToString();
            //Log.Write("CAS", "result.RadInt", $"GetWatt: {result.RadInt}");
            // 특정 대역의 WATT 구하기.
            // WD값으로 산출 or 대역대 설정하는 파라미터 필요. 
            //result.RadInt = GetWattRadInt(deviceId, 400, 500);
            //Log.Write("CAS", "result.RadInt", $"400~500: {result.RadInt}");


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
            // [C++ 동일 처리] WD(Lambda Dom) 및 Purity 계산 옵션 적용
            double lambdaDom = 0.0, purity = 0.0;
            double refX = IlluminantRefX > 0 ? IlluminantRefX : 0.33333;
            double refY = IlluminantRefY > 0 ? IlluminantRefY : 0.33333;
            CheckCASErrorAndThrow(CAS4DLL.casCalculateLambdaDom(deviceId, refX, refY, ref lambdaDom, ref purity));
            result.LambdaDom = lambdaDom;
            // Purity를 % 단위로 환산 (C++의 *= 100.0f 로직 반영)
            result.Purity = ConvertPurityToPercent ? purity * 100.0 : purity;
            //double lambdaDom = 0.0, purity = 0.0;
            //CheckCASErrorAndThrow(CAS4DLL.casCalculateLambdaDom(deviceId, 1.0 / 3.0, 1.0 / 3.0, ref lambdaDom, ref purity));
            //result.LambdaDom = lambdaDom;
            //result.Purity = purity;

            // Wavelength and peak value
            // [C++ 동일 처리] WP(Peak Wavelength) 추출 전 Full Range 변경 옵션 적용
            UseFullRangeForWP = false;
            if (UseFullRangeForWP)
            {
                // Full Range로 임시 변경 (0 설정 시 전체 스펙트럼)
                SetMeasurementParameter(CAS4DLL.mpidColormetricStart, 0);
                SetMeasurementParameter(CAS4DLL.mpidColormetricStop, 0);
                CheckCASErrorAndThrow(CAS4DLL.casColorMetric(deviceId));
            }

            // Wavelength and peak value
            double WP = 0.0, pickValue = 0.0;
            CAS4DLL.casGetPeak(deviceId, out WP, out pickValue);
            CheckCASErrorAndThrow();
            result.WP = WP;
            result.PickValue = pickValue;
            result.Centroid = CAS4DLL.casGetCentroid(deviceId);
            result.FWHM = CAS4DLL.casGetWidth(deviceId);

            // Full Range로 변경했던 범위를 원래 대역으로 복구
            if (UseFullRangeForWP)
            {
                SetMeasurementParameter(CAS4DLL.mpidColormetricStart, Config.ColormetricStart);
                SetMeasurementParameter(CAS4DLL.mpidColormetricStop, Config.ColormetricStop);
                // 복구 후 Color Metric 재계산 (이후 로직에 영향을 주지 않도록)
                CheckCASErrorAndThrow(CAS4DLL.casColorMetric(deviceId));
            }
            //double WP = 0.0, pickValue = 0.0;
            //CAS4DLL.casGetPeak(deviceId, out WP, out pickValue);
            //CheckCASErrorAndThrow();
            //result.WP = WP;
            //result.PickValue = pickValue;
            //result.Centroid = CAS4DLL.casGetCentroid(deviceId);
            //result.FWHM = CAS4DLL.casGetWidth(deviceId);

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

            //GetDeviceParameter(CAS4DLL.dpidADCBits, ref value);
            // Maximum ADC value
            double dADC = value;
            dADC = (int)CAS4DLL.casGetMeasurementParameter(deviceId, CAS4DLL.mpidMaxADCValue);
            
            spectrum.WaveLength = new double[visiblePixels];
            spectrum.Intensity = new double[visiblePixels];

            // 최소/최대 초기값을 올바르게 설정
            spectrum.MinimumIntensity = double.PositiveInfinity;
            spectrum.MaximumIntensity = double.NegativeInfinity;

            for (int i = 0; i < spectrum.WaveLength.Length; i++)
            {
                spectrum.Intensity[i] = CAS4DLL.casGetData(deviceId, i + deadPixels);
                spectrum.MinimumIntensity = Math.Min(spectrum.MinimumIntensity, spectrum.Intensity[i]);
                spectrum.MaximumIntensity = Math.Max(spectrum.MaximumIntensity, spectrum.Intensity[i]);

                spectrum.WaveLength[i] = CAS4DLL.casGetXArray(deviceId, i + deadPixels);
            }

            spectrum.ADC = dADC;
            result.ADC = (int)dADC;
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

        public void OffTrigger(int port)
        {
            OffDigitalOut(port);
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

                // [추가] Skip Level 설정
                // 1 = Enable, 0 = Disable
                SetMeasurementParameter(CAS4DLL.mpidSkipLevelEnabled, 1);
                // 노이즈로 간주하고 무시할 임계값(Intensity) 설정 (예: Config에서 불러오기)
                SetMeasurementParameter(CAS4DLL.mpidSkipLevel, Config.SkipLevel); // 적절한 값으로 변경

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

        double GetWatt(int device)
        {
            double watt = 0.0;
            double dsum = 0.0;
            for ( int j = 0; j < 10; j++)
            {
                int dead = (int)CAS4DLL.casGetDeviceParameter(device, CAS4DLL.dpidDeadPixels);
                int visible = (int)CAS4DLL.casGetDeviceParameter(device, CAS4DLL.dpidVisiblePixels);
                watt = 0.0;
                for (int i = 0; i < visible - 1; i++)
                {
                    int idx1 = dead + i;
                    int idx2 = dead + i + 1;

                    double lambda1 = CAS4DLL.casGetXArray(device, idx1);
                    double lambda2 = CAS4DLL.casGetXArray(device, idx2);
                    double delta = lambda2 - lambda1; // 거의 1 nm

                    if (lambda1 < 420 || lambda1 > 490)
                        continue;

                    double power = CAS4DLL.casGetData(device, idx1); // "calibration unit / nm"

                    watt += power * delta;
                    //Log.Write("CAS", "GetWatt", $"GetWatt calcul: {watt}");
                }

                dsum += watt;
            }
            watt = dsum / 10;

            return watt;
        }


        public static double GetWattRadInt(int device)
        {
            // 전체 스펙트럼 사용 (0이면 full range)
            //CAS4DLL.casSetMeasurementParameter(device, CAS4DLL.mpidColormetricStart, 0);
            //CAS4DLL.casSetMeasurementParameter(device, CAS4DLL.mpidColormetricStop, 0);
            //CAS4DLL.casSetMeasurementParameter(device, CAS4DLL.mpidColormetricStart, 400);
            //CAS4DLL.casSetMeasurementParameter(device, CAS4DLL.mpidColormetricStop, 500);
            //// 현재 스펙트럼으로 모든 컬러/라디오메트릭 값 계산
            //CAS4DLL.casColorMetric(device);

            var unitBuf = new StringBuilder(255);
            double radInt;
            CAS4DLL.casGetRadInt(device, out radInt, unitBuf, unitBuf.Capacity);
            //Log.Write("CAS", "RadIntUnit", unitBuf.ToString());
            return radInt;
        }

        public int GetTriggerDelayTimeMs()
        {
            if (!IsCreated()) return -1;
            if (!useHardwareTrigger) return -1;

            double v = 0;
            GetMeasurementParameter(CAS4DLL.mpidTriggerDelayTime, ref v);
            return (int)Math.Round(v);
        }

        public bool SetTriggerDelayTimeMs(int ms)
        {
            try
            {
                if (!IsCreated()) return false;
                if (!useHardwareTrigger) return false;

                if (ms < 0) ms = 0;
                // CAS4DLL mpidTriggerDelayTime 주석/관례상 ms 단위로 사용
                SetMeasurementParameter(CAS4DLL.mpidTriggerDelayTime, ms);
                return true;
            }
            catch (Exception ex)
            {
                Log.Write("CAS", "SetTriggerDelayTimeMs", ex.Message);
                return false;
            }
        }
        #endregion



        #region Diagnostics Logging (Saturation / Dark / Integral / Timing)
        private class SpectrumDiagnostics
        {
            public double SpectrumMin { get; set; }
            public double SpectrumMax { get; set; }
            public double DarkMedianLow { get; set; }     // 하위 N% 중앙값
            public double DarkMeanLow { get; set; }       // 하위 N% 평균
            public double Integral_UV { get; set; }       // 예: 250~290nm
            public double Integral_ConfigBand { get; set; } // Config.ColormetricStart~Stop
            public bool SaturationSuspect { get; set; }   // "의심" (raw count가 아닐 수 있음)
            public double Integral_Total { get; set; }      // full range
            public double Ratio_UV_to_Total { get; set; }   // UV 비율
            public double Ratio_RadInt_to_UVInt { get; set; } // RadInt / UVIntegral (선형성 체크)
            public string Note { get; set; }
        }

        
        private SpectrumDiagnostics CalcSpectrumDiagnostics(
            double uvStartNm = 250.0, double uvStopNm = 290.0,
            int lowPercent = 5)
        {
            var diag = new SpectrumDiagnostics();

            if (spectrum?.WaveLength == null || spectrum?.Intensity == null ||
                spectrum.WaveLength.Length == 0 || spectrum.Intensity.Length == 0 ||
                spectrum.WaveLength.Length != spectrum.Intensity.Length)
            {
                diag.Note = "Spectrum array invalid.";
                return diag;
            }

            int n = spectrum.Intensity.Length;

            // Min/Max
            diag.SpectrumMin = spectrum.MinimumIntensity;
            diag.SpectrumMax = spectrum.MaximumIntensity;

            // ---- Dark level 추정 (하위 lowPercent% 기반) ----
            // 주의: casGetData가 calibrated intensity일 수 있음.
            // 그럼에도 baseline drift/offset 진단에는 충분히 유용함.
            int k = (int)Math.Max(1, Math.Round(n * (lowPercent / 100.0)));
            var sorted = spectrum.Intensity.ToArray();
            Array.Sort(sorted);

            double sum = 0;
            for (int i = 0; i < k; i++) sum += sorted[i];
            diag.DarkMeanLow = sum / k;

            // 중앙값(하위 k개의 중앙)
            if (k == 1) diag.DarkMedianLow = sorted[0];
            else
            {
                int mid = k / 2;
                if (k % 2 == 0) diag.DarkMedianLow = (sorted[mid - 1] + sorted[mid]) / 2.0;
                else diag.DarkMedianLow = sorted[mid];
            }

            // ---- 적분 계산 (RawIntegral) ----
            // Intensity가 "unit / nm" 이면 적분값은 "unit"에 가까움.
            // deltaλ(파장 간격)을 곱해서 trapz로 적분.
            diag.Integral_UV = IntegrateBand(uvStartNm, uvStopNm, diag.DarkMeanLow);
            diag.Integral_ConfigBand = IntegrateBand(Config.ColormetricStart, Config.ColormetricStop, diag.DarkMeanLow);

            // ---- Saturation 의심 ----
            // **중요**: CAS4DLL.casGetData가 raw ADC count가 아닐 가능성 큼.
            // 그래서 아래는 "의심"만 기록한다.
            // 만약 intensityUnit이 count 계열(예: "cnt", "count", "ADU")이면 신뢰도가 올라감.
            bool unitLooksLikeCount = false;
            if (!string.IsNullOrWhiteSpace(intensityUnit))
            {
                var u = intensityUnit.Trim().ToLowerInvariant();
                unitLooksLikeCount = (u.Contains("cnt") || u.Contains("count") || u.Contains("adu") || u.Contains("adc"));
            }

            double maxAdc = 65535.0; // default
            try
            {
                // result.ADC는 현재 코드에서 mpidMaxADCValue로 채우고 있음(최대 ADC 값 의미)
                if (result != null && result.ADC > 0) maxAdc = result.ADC;
            }
            catch { }

            // PickValue가 counts일 때가 많음. (장비/설정에 따라 다름)
            // unitLooksLikeCount이면 아래 조건이 더 의미 있음.
            double satThreshold = maxAdc * 0.98;
            diag.SaturationSuspect = unitLooksLikeCount && (result != null && result.PickValue >= satThreshold);

            diag.Integral_Total = IntegrateBand(0, 0, diag.DarkMeanLow); // full range
            diag.Ratio_UV_to_Total = (diag.Integral_Total > 0) ? (diag.Integral_UV / diag.Integral_Total) : 0.0;

            // RadInt는 GetMeasureData()에서 이미 채워졌다고 가정(현재 호출 순서 OK)
            double rad = (result != null) ? result.RadInt : 0.0;
            if (!string.IsNullOrWhiteSpace(result?.RadIntUnit) &&
                result.RadIntUnit.Trim().ToLowerInvariant().Contains("mw"))
            {
                rad = rad / 1000.0;
            }
            diag.Ratio_RadInt_to_UVInt = (diag.Integral_UV > 0) ? (rad / diag.Integral_UV) : 0.0;

            _lastDarkLowMean = diag.DarkMeanLow;

            if (!unitLooksLikeCount)
                diag.Note = "Intensity unit not count-like. Saturation check is suspect-only.";

            return diag;
        }

        private double IntegrateBand(double startNm, double stopNm, double darkLevel)
        {
            if (spectrum?.WaveLength == null || spectrum?.Intensity == null) return 0;
            int n = spectrum.WaveLength.Length;
            if (n < 2) return 0;

            // start/stop == 0이면 full range로 취급하는 케이스 방어
            if (startNm <= 0 || stopNm <= 0 || stopNm <= startNm)
            {
                startNm = spectrum.WaveLength.First();
                stopNm = spectrum.WaveLength.Last();
            }

            double sum = 0.0;
            for (int i = 0; i < n - 1; i++)
            {
                double wl1 = spectrum.WaveLength[i];
                double wl2 = spectrum.WaveLength[i + 1];

                // 구간 밖이면 스킵
                if (wl2 < startNm || wl1 > stopNm) continue;

                // clamp
                double a = Math.Max(startNm, wl1);
                double b = Math.Min(stopNm, wl2);
                double delta = b - a;
                if (delta <= 0) continue;

                // baseline 보정(0 아래는 0)
                double y1 = spectrum.Intensity[i] - darkLevel;
                double y2 = spectrum.Intensity[i + 1] - darkLevel;
                if (y1 < 0) y1 = 0;
                if (y2 < 0) y2 = 0;

                // trapezoid
                sum += (y1 + y2) * 0.5 * delta;
            }
            return sum;
        }

        // [NEW] 측정하는 Die의 정보를 담을 컨텍스트 (예: "X:10, Y:20")
        public string CurrentMeasureContext { get; set; } = string.Empty;

        private void LogMeasureConditionSnapshot(string tag)
        {
            try
            {
                string prefix = string.IsNullOrEmpty(CurrentMeasureContext) ? "" : $"[{CurrentMeasureContext}] ";
                Log.Write("CAS", tag,
                    prefix + $"Cond: IntTime={Config.IntegrationTime}ms, Avg={Config.Averages}, DF={Config.DensityFilter}({GetDensityFilterNameFromValue(Config.DensityFilter)}), " +
                    $"Band={Config.ColormetricStart}~{Config.ColormetricStop}nm, TrgTimeout={Config.TriggerTimeout}ms, SkipEn=1, SkipLevel={Config.SkipLevel}, " +
                    $"HWTrg={useHardwareTrigger}, TrgDelay={GetTriggerDelayTimeMs()}ms");
            }
            catch (Exception ex)
            {
                Log.Write("CAS", tag, $"LogMeasureConditionSnapshot failed: {ex.Message}");
            }
        }

        private void LogSpectrumDiagnostics(string tag, SpectrumDiagnostics d)
        {
            try
            {
                string prefix = string.IsNullOrEmpty(CurrentMeasureContext) ? "" : $"[{CurrentMeasureContext}] ";
                Log.Write("CAS", tag,
                    prefix + $"Spec: Min={d.SpectrumMin:F6}, Max={d.SpectrumMax:F6}, Pick={result?.PickValue:F6}({intensityUnit}), " +
                    $"DarkLowMean={d.DarkMeanLow:F6}, DarkLowMed={d.DarkMedianLow:F6}, " +
                    $"Int_UV(250-290)={d.Integral_UV:F6}, Int_Band({Config.ColormetricStart}-{Config.ColormetricStop})={d.Integral_ConfigBand:F6}, " +
                    $"Int_Total={d.Integral_Total:F6}, UV/Total={d.Ratio_UV_to_Total:F6}, Rad/UVInt={d.Ratio_RadInt_to_UVInt:F6}, " +
                    $"LastDark={_lastDarkTime:HH:mm:ss}, " +
                    $"LastDarkMeanLow={_lastDarkLowMean:F6}, " +
                    $"SatSuspect={d.SaturationSuspect}, Note={d.Note}");
            }
            catch (Exception ex)
            {
                Log.Write("CAS", tag, $"LogSpectrumDiagnostics failed: {ex.Message}");
            }
        }

        private void LogMeasureResultsSummary(string tag)
        {
            try
            {
                string prefix = string.IsNullOrEmpty(CurrentMeasureContext) ? "" : $"[{CurrentMeasureContext}] ";
                Log.Write("CAS", tag,
                    prefix + $"Result: WP={result.WP:F5}nm, FWHM={result.FWHM:F5}nm, " +
                    $"CIEX={result.CIEX:F6}, CIEY={result.CIEY:F6}, " +
                    $"RadInt={result.RadInt:F6}{result.RadIntUnit}, PhotInt={result.PhotInt:F6}{result.PhotIntUnit}, " +
                    $"Centroid={result.Centroid:F5}, MaxADC={result.ADC}, TrgDelay={GetTriggerDelayTimeMs()}ms");
            }
            catch (Exception ex)
            {
                Log.Write("CAS", tag, $"LogMeasureResultsSummary failed: {ex.Message}");
            }
        }
        #endregion

    }
}
