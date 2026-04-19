using System;
using System.Text;
using System.Reflection;
using System.Runtime.InteropServices;
using SQLitePCL;
using System.IO;

/// <summary>The InstrumentSystems namespace.</summary>
namespace InstrumentSystems.CAS4
{
    ///@nosubgrouping
    /// <summary>The CAS DLL interface class</summary>
    /// <remarks>
    /// This static class provides access to all methods the CAS DLL exports as well as defining all the constants 
    /// that are used for calling the methods or interpreting the results.
    /// Even if you're accessing the CAS DLL directly and not via this interface class, you can still use
    /// this documentation as a reference for the available methods and constants.
    /// </remarks>
    public static partial class CAS4DLL
    {

#if true
        private const string CASLibName = "CAS4x64";
#else
        private const string CASLibName = "CAS4";
#endif

        //static CAS4DLL()
        //{
        //    NativeLibrary.SetDllImportResolver(typeof(CAS4DLL).Assembly, ImportResolver);
        //}

        //private static IntPtr ImportResolver(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
        //{
        //    //since there can only be one ImportResolver per assembly, we keep this generic enough so it works with companion SDKs like TLCI etc.

        //    IntPtr libHandle = IntPtr.Zero;
        //    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && (!libraryName.Contains(".DLL", StringComparison.CurrentCultureIgnoreCase)))
        //    {
        //        if (Environment.Is64BitProcess)
        //        {
        //            libraryName = libraryName + "x64.dll";
        //        } else {
        //            //CAS4.dll is the only one without x86.dll
        //            if (libraryName != CASLibName)
        //                libraryName = libraryName + "x86.dll";
        //        }
        //    }
        //    return NativeLibrary.Load(libraryName);
        //}

        //대체
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool SetDllDirectory(string lpPathName);

        static CAS4DLL()
        {
            // 예: 앱 시작 시 DLL 폴더 지정
            SetDllDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "x64"));
        }


        ///@name Error handling
        ///Return values and APIs to retrieve error messages
        ///@{
        public const int ErrorNoError             = 0;   ///< No error, i.e. function was successful
        public const int ErrorUnknown             = -1;  ///< An unknown or unexpected error. Retrieve the corresponding error message via <see cref="casGetErrorMessage"/> for additional info.
        public const int ErrorTimeoutRWSNoData    = -2;  ///< A timeout occurred while waiting for the spectrum data
        public const int ErrorInvalidDeviceType   = -3;  ///< Invalid InterfaceType constant passed to <see cref="casCreateDeviceEx"/> or <see cref="casChangeDevice"/>
        public const int ErrorAcquisition         = -4;  ///< Acquisition failed, i.e. an error occurred while the spectrometer was measuring
        public const int ErrorAccuDataStream      = -5;  ///< Averaging failed, increase <see cref="mpidIntegrationTime"/> or make sure PC is not too busy
        public const int ErrorPrivilege           = -6;  ///< no longer used
        public const int ErrorFIFOOverflow        = -7;  ///< It was detected that the FIFO of the interface card or the spectrometer was overflowing. Spectrum should be discarded
        public const int ErrorTimeoutEOSScan      = -8;  ///< The spectrometer FW reported a timeout while waiting for EOS of a spectrum scan
        public const int ErrorTimeoutEOSDummyScan = -9;  ///< The spectrometer FW reported a timeout while waiting for EOS of a dummy scan
        public const int ErrorFifoFull            = -10; ///< It was detected that the FIFO of the interface card or the spectrometer was full. Spectrum should be discarded
        public const int ErrorPixel1FinalCheck    = -11; ///< After reading spectrum data, the FIFO did not contain a pixel marked as first pixel. Spectrum should be discarded
        public const int ErrorCCDTemperatureFail  = -13; ///< The temperature of the CCD is outside of the allowed range. Temperature control failed or ambient temperature too high
        public const int ErrorAdrControl          = -14; ///< The spectrometer hardware associated with the used Device/CASID could not be found or accessed. See chapter @ref interfaceTypesAndOptions
        public const int ErrorFloat               = -15; ///< Floating point error while calculating calibrated spectrum
        public const int ErrorTriggerTimeout      = -16; ///< Timeout reached while waiting for the trigger. See <see cref="mpidTriggerTimeout"/>
        public const int ErrorAbortWaitTrigger    = -17; ///< While waiting for the trigger, the operation was aborted using <see cref="dpidAbortWaitForTrigger"/>
        public const int ErrorDarkArray           = -18; ///< An error occured while measuring the @ref darkCurrentArray
        public const int ErrorNoCalibration       = -19; ///< Invalid calibration filename specified for <see cref="dpidCalibFileName"/> 
        public const int ErrorInterfaceVersion    = -20; ///< The firmware version of the interface card or the driver version is too old to work with this device.
        public const int ErrorCRI                 = -21; ///< Error calculating CRI (color rendering indices). For more info see <see cref="casCalculateCRI"/>
        public const int ErrorNoMultiTrack        = -25; ///< Old way of doing MultiTrack not supported for this interface type. See chapter @ref multiTrackMeasurements
        public const int ErrorInvalidTrack        = -26; ///< no longer used
        public const int ErrorDetectPixel         = -31; ///< An error occurred while trying to detect the number of pixels the spectrometer has
        public const int ErrorSelectParamSet      = -32; ///< Error while activating a parameter set <see cref="dpidCurrentParamSet"/>. See chapter @ref workingWithParameterSets
        public const int ErrorI2CInit             = -35; ///< Initializing I2C bus failed
        public const int ErrorI2CBusy             = -36; ///< I2C operation failed because the I2C bus was busy
        public const int ErrorI2CNotAck           = -37; ///< I2C operation was not acknowledged
        public const int ErrorI2CRelease          = -38; ///< no longer used
        public const int ErrorI2CTimeOut          = -39; ///< I2C operation timed out
        public const int ErrorI2CTransmission     = -40; ///< I2C transmission failed in an unspecified way
        public const int ErrorI2CController       = -41; ///< The I2C controller responded in an unexpected way
        public const int ErrorDataNotAck          = -42; ///< no longer used
        public const int ErrorNoExternalADC       = -52; ///< The temperature ADC should be read, but the spectrometer does not have such an ADC
        public const int ErrorShutterPos          = -53; ///< Positioning the shutter or querying it's state failed. See chapter @ref densityFilter.
        public const int ErrorFilterPos           = -54; ///< Positioning the filterwheel or querying it's position failed. See chapter @ref densityFilter.
        public const int ErrorConfigSerialMismatch = -55; ///< Configuration file not intended for this spectrometer and possible accessories. The serial number stored in the file <see cref="dpidConfigFileName"/> does not match the current <see cref="dpidSerialNo"/>. Refer to chapter @ref usingSerialNumbers.
        public const int ErrorCalibSerialMismatch = -56; ///< Calibration file not intended for this spectrometer and possible accessories. The serial number stored in the file <see cref="dpidCalibFileName"/> does not match the current <see cref="dpidSerialNo"/>. Refer to chapter @ref usingSerialNumbers.
        public const int ErrorInvalidParameter    = -57; ///< Returned if there was an invalid "AWhat" parameter
        public const int ErrorGetFilterPos        = -58; ///< no longer used
        public const int ErrorParamOutOfRange     = -59; ///< Returned by many methods indicating that a parameter was out of range; does not apply for ADevice parameter which results in <see cref="errCasDeviceNotFound"/>
        public const int ErrorDeviceFileChecksum  = -60; ///< Returned by <see cref="dpidGetFilesFromDevice"/> if there was a checksum error, so either the device is corrupt or the communication failed
        public const int ErrorInvalidEEPromType   = -61; ///< Returned by <see cref="dpidGetFilesFromDevice"/> if the device does not support file storage
        public const int ErrorDeviceFileTooLarge  = -62; ///< Not enough storage on the device
        public const int ErrorNoCommunication     = -63; ///< The communication with the spectrometer ended unexpectedly
        public const int ErrorNoFilesOnIdentKey   = -64; ///< Returned by <see cref="dpidGetFilesFromDevice"/> if no files were found on the device
        public const int ErrorExtraCalibFileInvalid = -66; ///< Additional files of the calibration are either missing or corrupt
        public const int ErrorStraylightMismatch    = -67; ///< The straylight calibration file is not usable with this spectrometer
        public const int ErrorFeatureNotSupported   = -68; ///< The requested feature is not supported by the device
        public const int ErrorConfigUpToDate        = -70; ///< Config/calib files do not match the ones on the device. Returned by <see cref="coCheckConfigUpToDate"/> and <see cref="casInitialize"/> if option <see cref="coCheckConfigUpToDate"/> is set.
        public const int ErrorCommunicationTimeout  = -73; ///< The communication with the spectrometer reached a timeout
        public const int ErrorTransmissionSerialMismatch  = -74; ///<Transmission file not intended for this spectrometer and possible accessories. The serial number stored in the file <see cref="dpidTransmissionFileName"/> does not match the current <see cref="dpidSerialNo"/>. Refer to chapter @ref usingSerialNumbers.
        public const int ErrorAutoRangeMaxTrialsReached   = -75; ///<AutoRange measurement failed because it could not meet requirements with 10 trials. Only raised if <see cref="coAutoRangeErrorOnFailure"/> option is enabled. Refer to the chapter @ref autoRange for more details and how to avoid the error.
        public const int ErrorAutoRangeMaxIntTimeReached  = -76; ///<AutoRange measurement failed because <see cref="mpidAutoRangeMaxIntTime"/> was reached. Refer to the chapter @ref autoRange for more details and how to avoid the error.
        public const int ErrorAutoRangeIntTimeMinReached  = -77; ///<AutoRange measurement failed because <see cref="dpidIntTimeMin"/> was reached. Only raised if <see cref="coAutoRangeErrorOnFailure"/> option is enabled. Refer to the chapter @ref autoRange for more details and how to avoid the error.
        public const int ErrorAutoRangeSameIntTimeReached = -78; ///<AutoRange measurement failed because it ended up trying the same integration time twice. Only raised if <see cref="coAutoRangeErrorOnFailure"/> option is enabled. Refer to the chapter @ref autoRange for more details and how to avoid the error.

        public const int ErrorInvalidHandle = -100; ///< a parameter of type handle passed to one of the @ref handleAPI methods is not (or no longer) valid. See @ref handleErrorHandling for details on how to retrieve a more detailed error message using <see cref="cw_all_LastErrorMessage"/>.
        public const int ErrorInvalidIndexString = -101; ///< the parameter AIndexString of one of the @ref handleAPI methods was invalid. See @ref handleErrorHandling for details on how to retrieve a more detailed error message using <see cref="cw_all_LastErrorMessage"/>.

        public const int errCASOK                 = ErrorNoError;  ///< just an alias for <see cref="ErrorNoError"/>
        
        public const int errCASError              = -1000;         ///< not returned; base for a few other errors like <see cref="errCasDeviceNotFound"/>
        public const int errCasNoConfig           = errCASError-3; ///< Invalid filename specified for <see cref="dpidConfigFileName"/>
        public const int errCASDriverMissing      = errCASError-6; ///< no longer used
        public const int errCasDeviceNotFound     = errCASError-10; ///< Invalid ADevice / CASID parameter

        /// <summary>Return error code for a given device/CASID</summary>
        /// <param name="ADevice">The device / CASID</param>
        /// <remarks>
        /// Some methods provided by the CAS DLL do not return error codes. Call casGetError afterwards to check for errors which might have 
        /// occurred during these method calls. A negative value indicates an error. <see cref="ErrorNoError"/> indicates that the previous 
        /// action was successful. See chapter @ref errorHandling for more details.
        /// @note Every subsequent call into the CAS DLL will clear the previous error for the given device!
        /// <see cref="casGetErrorMessage"/> can be used to translate the returned error into an error message.
        /// </remarks>
        [DllImport(CASLibName)]
        public static extern int casGetError(int ADevice);

        /// <summary>Translates a given error code into a readable error message</summary>
        /// <param name="AError">The error code which should be translated</param>
        /// <param name="ADest">Destination for the error message</param>
        /// <param name="AMaxLen">The maximum number of characters ADest can hold</param>
        /// <returns>The return value should not be used</returns>
        /// Use this method to translate an error constant (AError) into a user-readable error message. The error constant can either 
        /// be the return value of a method of CAS DLL or can be explicitly retrieved using <see cref="casGetError"/>.
        /// The message is copied into the buffer ADest is pointing to. This buffer must be able to hold at least the number 
        /// of characters specified by AMaxLen plus a trailing zero.
        [DllImport(CASLibName, CharSet = CharSet.Auto)]
        public static extern IntPtr casGetErrorMessage(int AError, StringBuilder ADest, int AMaxLen);

        ///@name Device Handles and Interfaces Types and Options
        ///As oultined in @ref deviceHandles and @ref interfaceTypesAndOptions
        ///@{
        public const int InterfaceTest        = 3;  ///<Demo mode interface constant. For use with e.g. <see cref="casCreateDeviceEx"/>. See chapter @ref interfaceTypesAndOptions.
        public const int InterfaceUSB         = 5;  ///<USB interface constant. For use with e.g. <see cref="casCreateDeviceEx"/>. See chapter @ref interfaceTypesAndOptions.
        public const int InterfacePCIe        = 10; ///<PCIe interface constant. For use with e.g. <see cref="casCreateDeviceEx"/>. See chapter @ref interfaceTypesAndOptions.
        public const int InterfaceEthernet    = 11; ///<Ethernet interface constant. For use with e.g. <see cref="casCreateDeviceEx"/>. See chapter @ref interfaceTypesAndOptions.

        /// <summary>Creates a device context aka CASID within the CAS DLL</summary>
        /// <param name="AInterfaceType">Type of the interface, i.e. the way the spectrometer is connected to the PC</param>
        /// <param name="AInterfaceOption">The interface option, identifying either the spectrometer itself or the interface card</param>
        /// <returns>If the function was successful, the return value is a device handle (>=0). A negative value indicates an error, e.g. -3 = ErrorInvalidDeviceType</returns>
        /// <remarks>
        /// This method must be called to work with a spectrometer. It returns a device handle which is later used with subsequent function calls to identify a specific device 
        /// (typically called the ADevice parameter, also referred to as CASID). 
        ///
        /// For every device handle created, you should call <see cref="casDoneDevice"/> once it is no longer needed.
        /// 
        /// For interface type and option, there are some predefined interface constants (like <see cref="InterfaceUSB"/>), but it is generally recommended to enumerate 
        /// interface types and options and let the user choose from a set of two lists. Refer to chapter @ref interfaceTypesAndOptions for a detailed overview.
        ///
        /// @note casCreateDeviceEx does not perform hardware initialization. Use <see cref="casInitialize"/> for that.
        /// </remarks>
        [DllImport(CASLibName)]
        public static extern int casCreateDeviceEx(int AInterfaceType, int AInterfaceOption);

        /// <summary>Change the interface type and/or option of a device / CASID</summary>
        /// <param name="ADevice">The device / CASID</param>
        /// <param name="AInterfaceType">New interface type</param>
        /// <param name="AInterfaceOption">New interface option</param>
        /// <returns>The CASID of the changed device or a negative error like <see cref="ErrorInvalidDeviceType"/> or <see cref="errCasDeviceNotFound"/></returns>
        /// <remarks>Using this method it is possible to change the interface type and option of a device created with <see cref="casCreateDeviceEx"/>. 
        /// Refer to the chapter @ref interfaceTypesAndOptions for an overview about interface types and options.
        /// </remarks>
        [DllImport(CASLibName)]
        public static extern int casChangeDevice(int ADevice, int AInterfaceType, int AInterfaceOption);

        /// <summary>Release resources used by the device</summary>
        /// <param name="ADevice">The device / CASID</param>
        /// <returns>The method returns 0 if finalization of the device was successful, otherwise an error code</returns>
        /// <remarks>Call this method for every device which was created with <see cref="casCreateDeviceEx"/>. 
        /// Note that after a successful call to this method, the CASID ADevice is no longer valid!</remarks>
        [DllImport(CASLibName)]
        public static extern int casDoneDevice(int ADevice);

        public const int aoAssignDevice     = 0;///<Only device specific properties are copied. This is rarely necessary to assign separately. Examples: current filter wheel state, flag whether data is present etc. 
        public const int aoAssignParameters = 1;///<Only parameters are assigned. This includes all measurement parameters but also configuration and calibration as well as dark current. Pretty much everything which is not related to the state of the hardware.
        public const int aoAssignComplete   = 2;///<Assigns everything, i.e. equal to calling casAssignDeviceEx with aoAssignDevice and then with aoAssignParameters.

        /// <summary>
        /// Assigns properties and or parameters from one device handle to another
        /// </summary>
        /// <param name="ASourceDevice">Source device</param>
        /// <param name="ADestDevice">Destination device</param>
        /// <param name="AOption">Controls which aspects get assigned from ASourceDevice to ADestDevice</param>
        /// <returns>This method returns 0 if the assignment was successful. Otherwise it returns an Error Code</returns>
        /// <remarks>Assigning devices might be useful if you want to store different measurement setups for a given device. 
        /// By keeping copies of all parameters with a second device handle it is possible to store measurement setups which may 
        /// even differ by calibration or dark current, something which cannot be achieved using parameter sets.
        /// </remarks>
        [DllImport(CASLibName)]
        public static extern int casAssignDeviceEx(int ASourceDevice, int ADestDevice, int AOption);

        /// <summary>
        /// Retrieves the number of interface types the CAS DLL supports
        /// </summary>
        /// <returns>Returns a positive number indicating how many interface types are supported </returns>
        /// <remarks>Use this method when iterating over all interface names using <see cref="casGetDeviceTypeName"/>
        ///
        /// For more details on interface types and options, refer to chapter @ref interfaceTypesAndOptions
        /// </remarks>
        [DllImport(CASLibName)]
        public static extern int casGetDeviceTypes();

        /// <summary>Retrieves the name of the given interface type</summary>
        /// <param name="AInterfaceType">The interface type for which to retrieve the name</param>
        /// <param name="ADest">Destination string for the interface type name</param>
        /// <param name="AMaxLen">The maximum number of characters ADest can hold</param>
        /// <returns>The return value should not be used!</returns>
        /// <remarks>Use this method when iterating over all interface names. For AInterfaceType use constants from 0 to <see cref="casGetDeviceTypes"/> - 1.
        /// @note If an empty string is returned for an interface name, this interface type should not be used nor presented to the user!
        ///
        /// For more details on interface types and options, refer to chapter @ref interfaceTypesAndOptions
        /// </remarks>
        [DllImport(CASLibName, CharSet = CharSet.Auto)]
        public static extern IntPtr casGetDeviceTypeName(int AInterfaceType, StringBuilder ADest, int AMaxLen);

        /// <summary>Retrieves the number of options a given interface type currently supports</summary>
        /// <param name="AInterfaceType">The interface type for which to retrieve the number of options</param>
        /// <returns>Returns a negative error code like <see cref="ErrorInvalidParameter"/> or a number indicating how many interface options the given interface type supports</returns>
        /// <remarks>Use this method when iterating over interface options using <see cref="casGetDeviceTypeOption"/>
        ///
        /// For more details on interface types and options, refer to chapter @ref interfaceTypesAndOptions
        /// </remarks>
        [DllImport(CASLibName)]
        public static extern int casGetDeviceTypeOptions(int AInterfaceType);

        /// <summary>Returns the value of the interface option for the given interface type and option index</summary>
        /// <param name="AInterfaceType">The interface type for which to retrieve the option</param>
        /// <param name="AIndex">0-based index of the option</param>
        /// <returns>Returns an interface option. Do not interpret as an error code!</returns>
        /// <remarks>Use this method when iterating over all interface options. AIndex can range from 0 to <see cref="casGetDeviceTypeOptions"/> - 1.
        /// The returned interface option can be used when calling <see cref="casCreateDeviceEx"/> or <see cref="casChangeDevice"/>.
        ///
        /// For more details on interface types and options, refer to chapter @ref interfaceTypesAndOptions
        /// </remarks>
        [DllImport(CASLibName)]
        public static extern int casGetDeviceTypeOption(int AInterfaceType, int AIndex);

        /// <summary>Retrieves the name of the given interface option</summary>
        /// <param name="AInterfaceType">The interface type for which to retrieve the interface option name</param>
        /// <param name="AInterfaceOptionIndex">The 0-based index of the interface option for which to retrieve the name</param>
        /// <param name="ADest">Destination string for the interface option name</param>
        /// <param name="AMaxLen">The maximum number of characters ADest can hold</param>
        /// <returns>The return value should not be used!</returns>
        /// <remarks>Use this method when iterating over all interface options. For AInterfaceOptionIndex use constants from 0 to <see cref="casGetDeviceTypeOptions"/> - 1.
        ///
        /// For more details on interface types and options, refer to chapter @ref interfaceTypesAndOptions
        /// </remarks>
        [DllImport(CASLibName, CharSet = CharSet.Auto)]
        public static extern IntPtr casGetDeviceTypeOptionName(int AInterfaceType, int AInterfaceOptionIndex, StringBuilder ADest, int AMaxLen);
        ///@}

        ///@name Initialization
        
        ///@{
        public const int InitOnce       = 0; ///<<see cref="casInitialize"/>: load config and calib, and only initialize the hardware if not done before
        public const int InitForced     = 1; ///<<see cref="casInitialize"/>: load config and calib, and always initialize the hardware even if it was initialized before
        public const int InitNoHardware = 2; ///<<see cref="casInitialize"/>: load config and calib, but do not initialize the hardware

        /// <summary>Initializes the hardware of the device after loading the configuration and calibration files</summary>
        /// <param name="ADevice">The device / CASID</param>
        /// <param name="Perform">One of the following constants: <see cref="InitOnce"/>, <see cref="InitForced"/> and <see cref="InitNoHardware"/></param>
        /// <returns>Returns 0 if the device was successfully initialized or a negative error constant like <see cref="errCasDeviceNotFound"/>, 
        /// <see cref="errCasNoConfig"/>, <see cref="ErrorNoCalibration"/> or <see cref="ErrorAdrControl"/></returns>.
        [DllImport(CASLibName)]
        public static extern int casInitialize(int ADevice, int Perform);
        ///@}
        
        ///@name Device parameter
        ///Constants starting with dpid (device parameter ID)

        ///@{
        public const int dpidIntTimeMin                 = 101; ///<float: minimum integration time in ms as supported by the device; check when setting <see cref="mpidIntegrationTime"/>
        public const int dpidIntTimeMax                 = 102; ///<float: maximum integration time in ms as supported by the device; check when setting <see cref="mpidIntegrationTime"/>
        public const int dpidDeadPixels                 = 103; ///<number of dead pixels at the beginning of the pixel array; see <see cref="casGetData"/>
        public const int dpidVisiblePixels              = 104; ///<number of visible pixels after dead pixels in the pixel array that form the spectrum; see <see cref="casGetData"/>
        public const int dpidPixels                     = 105; ///<the total number of pixels in the pixel array; defined by the configuration file and/or the device - should not be modified; see <see cref="casGetData"/>
        public const int dpidParamSets                  = 106; ///<total number of parameter sets defined for this device - see @ref workingWithParameterSets; when set to 0, all are cleared but a new one is added immediately
        public const int dpidCurrentParamSet            = 107; ///<0-based index of the currently active parameter set - see @ref workingWithParameterSets
        public const int dpidADCRange                   = 108; ///<the ADC level (in Counts) where saturation effects may start, i.e. if <see cref="mpidMaxADCValue"/> is larger than dpidADCRange, the measurement was saturated
        public const int dpidADCBits                    = 109; ///<the bit resolution of the ADC - for information purposes only. To check for saturation, use <see cref="dpidADCRange"/>
        public const int dpidSerialNo                   = 110; ///<the complete serial number of spectrometer and accessories if applicable. See @ref usingSerialNumbers for an overview
        public const int dpidTOPSerial                  = 111; ///<deprecated: the serial number of a TOP200 that is required by the current calibration. Rather use <see cref="dpidTOPSerialEx"/> and refer to @ref usingSerialNumbers for an overview
        public const int dpidTransmissionFileName       = 112; ///<Returns or sets the path of thetransmission correction file (typical extension .isa); see <see cref="coUseTransmission"/>. Note that an invalid filename will not raise an error. The file is loaded during <see cref="casInitialize"/> or when <see cref="paUpdateCompleteCalibration"/> is called.
        public const int dpidConfigFileName             = 113; ///<Returns or sets the path of the currently used configuration file (extension .ini). This file is required and loaded during <see cref="casInitialize"/> or <see cref="errCasNoConfig"/> will occur.
        public const int dpidCalibFileName              = 114; ///<Returns or sets the path of the currently used calibration file (extension .isc). This file is required and loaded during <see cref="casInitialize"/> or <see cref="ErrorNoCalibration"/> will occur.
        public const int dpidCalibrationUnit            = 115; ///<the calibration unit as defined by the calibration file <see cref="dpidCalibFileName"/>
        public const int dpidAccessorySerial            = 116; ///<serial number of the ID key connected to the spectrometer. See @ref usingSerialNumbers for an overview
        public const int dpidTriggerCapabilities        = 118; ///<Returns a bit-set describing the trigger capabilities of the spectrometer. <see cref="tcoCanTrigger"/> and others. For more info, see task @ref triggeredMeasurements "Triggered Measurements"
        public const int dpidAveragesMax                = 119; ///<Returns maximum number of averages supported by the device/configuration. Use for validating <see cref="mpidAverages"/>
        public const int dpidFilterType                 = 120; ///<Returns the density filter type: 0 = none, 1 = manual filter, 2,3 = filter wheel. Refer to @ref densityFilter for an overview
        public const int dpidRelSaturationMin           = 123; ///<Returns the minimum relative saturation in % which is still considered good for this device. Compare against <see cref="mpidRelSaturation"/>
        public const int dpidRelSaturationMax           = 124; ///<Returns the maximum relative saturation in % which is still considered good for this device. Compare against <see cref="mpidRelSaturation"/>
        public const int dpidInterfaceVersion           = 125; ///<Returns an optional version information for the devices interface. Currently none of the provided interfaces supports this anymore and all will return 0.
        public const int dpidTriggerDelayTimeMax        = 126; ///<float: maximum trigger delay time in ms supported by the device/configuration. Use to check user input for the <see cref="mpidTriggerDelayTime"/> measurement parameter.
        public const int dpidSpectrometerName           = 127; ///<Returns the name of the spectrometer type as specified in the configuration file.
        public const int dpidNeedDarkCurrent            = 130; ///<Deprecated, rather check <see cref="dpidDCRemeasureReasons"/> and refer to chapter @ref darkCurrent "dark current" for an overview.
        public const int dpidNeedDensityFilterChange    = 131; ///<Returns whether the density filter needs to be moved (all values <> 0). Refer to chapter @ref densityFilter "density filer" for an overview.
        public const int dpidSpectrometerModel          = 132; ///<Returns an ID which identifies the spectrometer type as specified in the configuration file. Rather use <see cref="dpidSpectrometerName"/>
        public const int dpidLine1FlipFlop              = 133; ///<Returns the state of the trigger flipflop on line 1 (CAS140B and CAS140CT only, refer to the hardware manual and the <see cref="tcoGetFlipState"/> trigger capability). The value is 0 for low state or 1 for high. When setting this value, the flipflop is reset, regardless of the value which is passed.
        public const int dpidTimer                      = 134; ///<Returns the internal timer. Use not recommended. Rather use <see cref="mpidTimeSinceScanStart"/>
        public const int dpidInterfaceType              = 135; ///<Returns the interface type for the specified device (or a negative error). See chapter @ref interfaceTypesAndOptions "Interface type and Options" for an overview.
        public const int dpidInterfaceOption            = 136; ///<Returns the interface option for the specified device (or a negative error). See chapter @ref interfaceTypesAndOptions "Interface type and Options" for an overview.
        public const int dpidInitialized                = 137; ///<Returns whether the device has been correctly initialized (see <see cref="casInitialize"/>) or not. A value bigger than 0 indicates it has been initialized, 0 that it hasn't and a negative value indicates an error.
        public const int dpidDCRemeasureReasons         = 138; ///<Returns a set of flags which indicate why a dark current measurement is necessary. Values include <see cref="todcrrNeedDarkCurrent"/> and <see cref="todcrrCCDTemperature"/>. See chapter @ref darkCurrent "dark current" for an overview.
        public const int dpidAbortWaitForTrigger        = 140; ///<Not recommended - setting dpidAbortWaitForTrigger to non-zero will abort a triggered acquisition if the trigger has not yet occurred. No longer supported by any of the provided interfaces.
        public const int dpidGetFilesFromDevice         = 142; ///<Set to the path of an existing directory and immediately a download of the files on the device will start. This might take considerable time, i.e. several seconds. The device does not have to be initialized, but interface type and option have to be set. Might return <see cref="ErrorInvalidEEPromType"/> or <see cref="ErrrorDeviceFileCheckSum"/>
        public const int dpidTOPType                    = 143; ///<Returns the type of TOP which is required by the current configuration. Possible values start at <see cref="ttNone"/>
        public const int dpidTOPSerialEx                = 144; ///<Returns the serial number of the TOP which is required by the current configuration or an empty string if no TOP is required. See @ref usingSerialNumbers.
        public const int dpidAutoRangeFilterMin         = 145; ///<Identifies the lowest density filter index for <see cref="mpidDensityFilter"/> that should be used for @ref autoRange with the <see cref="coAutorangeFilter"/> option. This constant is part of the configuration file and should not be changed.
        public const int dpidAutoRangeFilterMax         = 146; ///<Identifies the highest density filter index for <see cref="mpidDensityFilter"/> that should be used for @ref autoRange with the <see cref="coAutorangeFilter"/> option. This constant is part of the configuration file and should not be changed.
        public const int dpidMultiTrackMaxCount         = 147; ///<Returns the maximum number of tracks which are supported for @ref multiTrackMeasurements with this device.
        public const int dpidSLCFileInfo                = 148; ///<Returns file path and timestamp for a straylight calibration file, if present. Otherwise an empty string is returned.
        public const int dpidCheckConfigFileSerial      = 149; ///<Write-only string device parameter. Call with <see cref="dpidSerialNo"/> from another CASID to check whether this CASID uses the correct configuration file for this accessory.
        public const int dpidCheckCalibFileSerial       = 150; ///<Write-only string device parameter. Call with <see cref="dpidSerialNo"/> from another CASID to check whether this CASID uses the correct calibration file for this accessory.
        public const int dpidExtraTransmissionsFileInfo = 152; ///<Returns file information about additional transmission files the calibration uses or an empty string if it doesn't.
        public const int dpidMultiTrackCount            = 153; ///<Returns the number of MultiTracks which have been initialized/allocated for @ref multiTrackMeasurements with this device.
        public const int dpidIntTimePossibleResolutions = 154; ///<Returns a string containing the supported values for the <see cref="mpidIntTimeResolution"/>, separated by semicolons. The string is formatted using the current system locale, times are in microseconds. Examples: "1000;100;25", but might also be "1000" if only 1ms is supported.
        public const int dpidCalibDate                  = 155; ///<Returns a string in ISO 8601 format containing the calibration date. Only valid after a call to @ref casInitialize
        public const int dpidTriggerDelayTimeMin        = 159; ///<float: minimum trigger delay time in ms supported by the device/configuration. Use to check user input for the <see cref="mpidTriggerDelayTime"/> measurement parameter.
        public const int dpidWavelengthCalibrationType  = 160; ///<string: returns the type of wavelength calibration for the given CAS ID and it's INI/ISC. Only valid after <see cref="casInitialize"/> has been called successfully.
        public const int dpidCheckReferenceSpectrumFile = 162; ///<string, write-only: pass the complete file path of an ACS reference spectrum to check whether that spectrum has matching settings, so it can be compared with spectra of this CAS ID. Returns <see cref="ErrorNoCalibration"/> if the spectrum file could not be loaded, <see cref="ErrorInvalidParameter"/> if the settings of the spectrum do not match that of the CAS ID and <see cref="ErrorNoError"/> if they do match.
        public const int dpidFlashDurationMin           = 163; ///<float, read-only: minimum supported <see cref="mpidFlashDuration"/> in milliseconds. Only applicable if <see cref="dpidTriggerCapabilities"/> contains <see cref="tcoFlashHardwareDuration"/>.
        public const int dpidSpectrumFileFormat         = 167; ///<string: identifies the file format which is used when <see cref="casSaveSpectrum"/> is called for this CAS-ID. For an overview see chapter @ref savingISDs. The default value for dpidSpectrumFileFormat is "CAS4.OriginalISD". Other options can be selected from <see cref="dpidSpectrumFileFormatList"/>. 
        public const int dpidSpectrumFileFormatList     = 168; ///<string, readonly: returns a list of possible values for <see cref="dpidSpectrumFileFormat"/> separated by semicolons. For an overview see chapter @ref savingISDs.
        public const int dpidEnforceUTF8                = 172; ///<integer, global: if this value is set to something different than 0, the ANSI APIs of the CAS SDK will expect and return UTF-8 encoded strings, even if the system code page is not UTF-8. This allows for cross-platform code to always use UTF-8. This dpid does not require that you pass valid CASID when getting or setting this parameter (any value will be accepted).
        public const int dpidObserverList               = 174; ///<string, global: list of names, separated by line-breaks (OS-dependant) of the available observers which can be used for additional colormetric results, see <see cref="mpidAdditionalObserver"/> and for an overview @ref colorMetricAdditionalObservers.

        public const int dpidDebugLogFile               = 204; ///<complete filename and path where debug log should be written to. Set to empty path to stop logging. This dpid does not require that you pass valid CASID when getting or setting this parameter (any value will be accepted).
        public const int dpidDebugLogLevel              = 205; ///<integer level describing what to log, each higher level includes everything the levels below log, plus the things in this level. See constants starting with <see cref="DebugLogLevelErrors"/>. This dpid does not require that you pass valid CASID when getting or setting this parameter (any value will be accepted).
        public const int dpidDebugMaxLogSize            = 206; ///<integer file size, in bytes, the logfile should not exceed. Once this size is reached, a backup file with the same name but a .bak extension of the log is created and a new logfile starts. An already existing backup file will be deleted. This dpid does not require that you pass valid CASID when getting or setting this parameter (any value will be accepted).

        public const int tcoCanTrigger                  = 0x00000001; ///< bit for <see cref="dpidTriggerCapabilities"/>. Spectrometer can be triggered externally, i.e. <see cref="mpidTriggerSource"/> supports <see cref="trgFlipFlop"/>
        public const int tcoTriggerDelay                = 0x00000002; ///< bit for <see cref="dpidTriggerCapabilities"/>. Spectrometer supports a trigger delay (<see cref="mpidTriggerDelayTime"/>).
        public const int tcoTriggerOnlyWhenReady        = 0x00000004; ///< bit for <see cref="dpidTriggerCapabilities"/>. Spectrometer supports changing the <see cref="toAcceptOnlyWhenReady"/> trigger option (<see cref="mpidTriggerOptions"/>)
        public const int tcoAutoRangeTriggering         = 0x00000008; ///< bit for <see cref="dpidTriggerCapabilities"/>. Spectrometer supports the <see cref="toForEachAutoRangeTrial"/> trigger option (<see cref="mpidTriggerOptions"/>)
        public const int tcoShowBusyState               = 0x00000010; ///< bit for <see cref="dpidTriggerCapabilities"/>. Spectrometer supports the <see cref="toShowBusyState"/> trigger option (<see cref="mpidTriggerOptions"/>)
        public const int tcoShowACQState                = 0x00000020; ///< bit for <see cref="dpidTriggerCapabilities"/>. Spectrometer supports the <see cref="toShowACQState"/> trigger option (<see cref="mpidTriggerOptions"/>) 
        public const int tcoFlashOutput                 = 0x00000040; ///< bit for <see cref="dpidTriggerCapabilities"/>. Spectrometer supports flash output in general, i.e. <see cref="mpidFlashType"/>. Rather check <see cref="tcoFlashHardware"/> for <see cref="ftHardware"/> and <see cref="tcoFlashSoftware"/> for <see cref="ftSoftware"/>
        public const int tcoFlashHardware               = 0x00000080; ///< bit for <see cref="dpidTriggerCapabilities"/>. Spectrometer supports flash hardware, <see cref="ftHardware"/> for <see cref="mpidFlashType"/>
        public const int tcoFlashHardwareForEachAverage = 0x00000100; ///< bit for <see cref="dpidTriggerCapabilities"/>. Spectrometer can output a flash signal for each averaged spectrum (<see cref="foEveryAverage"/> option of <see cref="mpidFlashOptions"/>, <see cref="ftHardware"/>)
        public const int tcoFlashSoftwareDelay          = 0x00000200; ///< bit for <see cref="dpidTriggerCapabilities"/>. Spectrometer supports flash delay (<see cref="mpidFlashDelayTime"/>), for <see cref="ftSoftware"/>
        public const int tcoFlashSoftwareDelayNegative  = 0x00000400; ///< bit for <see cref="dpidTriggerCapabilities"/>. Spectrometer supports a negative flash delay (<see cref="mpidFlashDelayTime"/>) for <see cref="ftSoftware"/>
        public const int tcoFlashSoftware               = 0x00000800; ///< bit for <see cref="dpidTriggerCapabilities"/>. Spectrometer supports flash type <see cref="ftSoftware"/> (<see cref="mpidFlashType"/>)
        public const int tcoGetFlipFlopState            = 0x00001000; ///< bit for <see cref="dpidTriggerCapabilities"/>. Spectrometer supports reading the flip flop state using <see cref="dpidLine1FlipFlop"/>
        public const int tcoQueryHasData                = 0x00002000; ///< bit for <see cref="dpidTriggerCapabilities"/>. Spectrometer supports querying the FIFO state using <see cref="casFIFOHasData"/>
        public const int tcoACQStatePolarity            = 0x00004000; ///< bit for <see cref="dpidTriggerCapabilities"/>. Spectrometer supports changing the polarity of the ACQ state line via <see cref="mpidACQStateLinePolarity"/>
        public const int tcoBusyStatePolarity           = 0x00008000; ///< bit for <see cref="dpidTriggerCapabilities"/>. Spectrometer supports changing the polarity of the Busy state line via <see cref="mpidBusyStateLinePolarity"/>
        public const int tcoFlashHardwareDelay          = 0x00010000; ///< bit for <see cref="dpidTriggerCapabilities"/>. Spectrometer supports flash delay (<see cref="mpidFlashDelayTime"/>), for <see cref="ftHardware"/>
        public const int tcoFlashHardwareDelayNegative  = 0x00020000; ///< bit for <see cref="dpidTriggerCapabilities"/>. Spectrometer supports a negative flash delay (<see cref="mpidFlashDelayTime"/>) for <see cref="ftHardware"/>
        public const int tcoFlashHardwareDuration       = 0x00040000; ///< bit for <see cref="dpidTriggerCapabilities"/>. Spectrometer supports an adjustable flash duration (<see cref="mpidFlashDuration"/>) for <see cref="ftHardware"/>

        //deprecated TriggerCapabilities; use the ones they are aliased to
        public const int tcoFlashForEachAverage  = tcoFlashHardwareForEachAverage;
        public const int tcoFlashDelay           = tcoFlashSoftwareDelay;
        public const int tcoFlashDelayNegative   = tcoFlashSoftwareDelayNegative;

        public const int todcrrNeedDarkCurrent   = 0x0001; ///< bit for <see cref="dpidDCRemeasureReasons"/>. No valid dark current measurement present. This flag is identical to the now obsolete <see cref="dpidNeedDarkCurrent"/>.
        public const int todcrrCCDTemperature    = 0x0002; ///< bit for <see cref="dpidDCRemeasureReasons"/>. A new dark current measurement should be done, since the CCD temperature has changed too much (varies depending on device; <see cref="mpidDCCCDTemperature"/> and <see cref="mpidLastCCDTemperature"/> are compared).

        public const int ttNone       = 0; ///< <see cref="dpidTOPType"/> constant for no TOP configured
        public const int ttTOP100     = 1; ///< <see cref="dpidTOPType"/> constant for TOP 100
        public const int ttTOP200     = 2; ///< <see cref="dpidTOPType"/> constant for TOP 200
        public const int ttTOP150     = 3; ///< <see cref="dpidTOPType"/> constant for TOP 150
        public const int ttTOPLumiTOP = 4; ///< <see cref="dpidTOPType"/> constant for LumiTOP
        public const int ttLumiTOPX   = 5; ///< <see cref="dpidTOPType"/> constant for LumiTOPX
        public const int ttTOP300     = 6; ///< <see cref="dpidTOPType"/> constant for TOP300

        public const int DebugLogLevelErrors = 1; ///< <see cref="dpidDebugLogLevel"/> constant, only log errors
        public const int DebugLogLevelSaturation = 2; ///< <see cref="dpidDebugLogLevel"/> constant, only log errors and saturation warnings
        public const int DebugLogLevelHardwareEvents = 3; ///< <see cref="dpidDebugLogLevel"/> constant, additionally log any hardware events
        public const int DebugLogLevelParameterChanges = 4; ///< <see cref="dpidDebugLogLevel"/> constant, additionally log any mpid oder dpid changes
        public const int DebugLogLevelAllMethodCalls = 10; ///< <see cref="dpidDebugLogLevel"/> constant, log every API call - warning, this may slow things down and create a big backlog of things which still need to be logged

        /// <summary>
        /// Retrieves a float representing a device parameter
        /// </summary>
        /// <param name="ADevice">The device / CASID</param>
        /// <param name="AWhat">dpid constant identifying which device parameter to retrieve</param>
        /// <returns>Either an error code or a float whose meaning depends on the AWhat parameter. Refer to the documentation of the dpid constant used for AWhat.</returns>
        [DllImport(CASLibName)]
        public static extern double casGetDeviceParameter(int ADevice, int AWhat);

        /// <summary>
        /// Sets a numerical device parameter
        /// </summary>
        /// <param name="ADevice">The device / CASID</param>
        /// <param name="AWhat">dpid constant identifying which device parameter to change</param>
        /// <param name="AValue">the numerical value the device parameter should be set to</param>
        /// <returns>0 if successful or an error code</returns>
        [DllImport(CASLibName)]
        public static extern int casSetDeviceParameter(int ADevice, int AWhat, double AValue);

        /// <summary>
        /// Retrieves a string representing a device parameter
        /// </summary>
        /// <param name="ADevice">The device / CASID</param>
        /// <param name="AWhat">dpid constant identifying which device parameter to retrieve</param>
        /// <param name="ADest">StringBuilder/PAnsiChar buffer to retrieve the string</param>
        /// <param name="AMaxLen">number of characters ADest can hold - including a terminating zero</param>
        /// <returns>0 if successful or an error code</returns>
        [DllImport(CASLibName, CharSet = CharSet.Auto)]
        public static extern int casGetDeviceParameterString(int ADevice, int AWhat, StringBuilder ADest, int AMaxLen);

        /// <summary>
        /// Sets a string device parameter
        /// </summary>
        /// <param name="ADevice">The device / CASID</param>
        /// <param name="AWhat">dpid constant identifying which device parameter to change</param>
        /// <param name="AValue">the null terminated string value the device parameter should be set to</param>
        /// <returns>0 if successful or an error code</returns>
        [DllImport(CASLibName, CharSet = CharSet.Auto)]
        public static extern int casSetDeviceParameterString(int ADevice, int AWhat, string AValue);
        ///@}

        ///@name Serial numbers
        ///APIs that go beyond <see cref="dpidSerialNo"/>
        ///@{
        public const int casSerialComplete          = 0; ///< <see cref="casGetSerialNumberEx"/> constant to retrieve complete serial number string, identical to <see cref="dpidSerialNo"/>
        public const int casSerialAccessory         = 1; ///< <see cref="casGetSerialNumberEx"/> constant to retrieve the serial number of additional equipment, if present, otherwise "N/A".; identical to <see cref="dpidAccessorySerial"/>
        public const int casSerialExtInfo           = 2; ///< <see cref="casGetSerialNumberEx"/> constant to retrieve extended information, i.e. a multiline string (delimiter: semicolon) with CAS type, ADC bits, serial number, No. of Pixels, ActivePixels and additional information depending on type and version of the firmware.
        public const int casSerialDevice            = 3; ///< <see cref="casGetSerialNumberEx"/> constant to retrieve the serial number read from the device; note that this might differ from case <see cref="casSerialComplete"/> without accessories or a TOP, since casSerialComplete falls back to the configuration file or its file name for a serial number (see @ref usingSerialNumbers for more details)
        public const int casSerialTOP               = 4; ///< <see cref="casGetSerialNumberEx"/> constant to retrieve the serial number of a TOP if required by the calibration; identical to <see cref="dpidTOPSerialEx"/>
        public const int casSerialExtInfoMultiLine  = 5; ///< <see cref="casGetSerialNumberEx"/> constant to retrieve extended information, i.e. a multiline string (CRLF or LF depending on OS) with CAS type, ADC bits, serial number, No. of Pixels, ActivePixels and additional information depending on type and version of the firmware.

        /// <summary>
        /// Retrieves serial numbers of the specified device and/or additional information.
        /// </summary>
        /// <param name="ADevice">The device / CASID</param>
        /// <param name="AWhat">constant starting with "casSerial" identifying which serial number to retrieve</param>
        /// <param name="ADest">StringBuilder/PAnsiChar buffer to retrieve the string</param>
        /// <param name="AMaxLen">number of characters ADest can hold - including a terminating zero</param>
        /// <returns>0 if successful or an error code</returns>
        /// <remarks>As opposed to <see cref="dpidSerialNo"/>, this method can retrieve the serial number of the device and additional equipment separately. See chapter @ref usingSerialNumbers for an overview.
        /// Possible values for AWhat start with <see cref="casSerialComplete"/>
        /// </remarks>
        [DllImport(CASLibName, CharSet = CharSet.Auto)]
        public static extern int casGetSerialNumberEx(int ADevice, int AWhat, StringBuilder ADest, int AMaxLen);

        ///@name Device options
        ///Options and capabilities of a CAS device
        ///@{
        public const int coShutter                        = 0x00000001; ///< <see cref="casGetOptions"/> bit: device has a shutter; see @ref darkCurrent
        public const int coFilter                         = 0x00000002; ///< <see cref="casGetOptions"/> bit: device has a density filter; see @ref densityFilter
        public const int coGetShutter                     = 0x00000004; ///< <see cref="casGetOptions"/> bit: device can check the position of the shutter (aka shutter control)
        public const int coGetFilter                      = 0x00000008; ///< <see cref="casGetOptions"/> bit: device can check the position of the filter wheel (aka filter control)
        public const int coGetAccessories                 = 0x00000010; ///< <see cref="casGetOptions"/> bit: device can retrieve serial number from additional equipment, see <see cref="dpidAccessorySerial"/>
        public const int coGetTemperature                 = 0x00000020; ///< <see cref="casGetOptions"/> bit: device can measure sensor temperature, see <see cref="mpidCurrentCCDTemperature"/>
        public const int coUseDarkcurrentArray            = 0x00000040; ///< <see cref="casGetOptions"/> bit: option to enabled dark current array, see @ref darkCurrent
        public const int coUseTransmission                = 0x00000080; ///< <see cref="casGetOptions"/> bit: option to automatically apply a transmission correction when measuring as defined by <see cref="dpidTransmissionFileName"/>
        public const int coAutorangeMeasurement           = 0x00000100; ///< <see cref="casGetOptions"/> bit: option to automatically detect good integration time, i.e. perform a @ref autoRange
        public const int coAutorangeFilter                = 0x00000200; ///< <see cref="casGetOptions"/> bit: option to use density filter during @ref autoRange
        public const int coCheckCalibConfigSerials        = 0x00000400; ///< <see cref="casGetOptions"/> bit: option to automatically check if config and calibration files are intended for this device; see @ref usingSerialNumbers
        public const int coTOPHasFieldOfViewConfig        = 0x00000800; ///< <see cref="casGetOptions"/> bit: indication that configuration/calibration includes field of view information for the TOP, so <see cref="mpidTOPFieldOfView"/> measurement condition is valid
        public const int coAutoRemeasureDC                = 0x00001000; ///< <see cref="casGetOptions"/> bit: option for turning automatic dark current measurements on/off. The interval after which the dark current is marked as invalid is defined by <see cref="mpidRemeasureDCInterval"/>
        public const int coCanMultiTrack                  = 0x00008000; ///< <see cref="casGetOptions"/> bit: device supports @ref multiTrackMeasurements; depends on interface type
        public const int coCanSwitchLEDOff                = 0x00010000; ///< <see cref="casGetOptions"/> bit: device can switch off status LEDs during measurement (see option below)
        public const int coLEDOffWhileMeasuring           = 0x00020000; ///< <see cref="casGetOptions"/> bit: option for switching off status LEDs during measurements. Only supported if <see cref="coCanSwitchLEDOff"/> is present
        public const int coCheckConfigUpToDate            = 0x00040000; ///< <see cref="casGetOptions"/> bit: option to check whether the Ident-Key contains newer configuration/calibration files; happens during <see cref="casInitialize"/> and might result in <see cref="ErrorConfigUpToDate"/>
        public const int coCanAverageOnDevice             = 0x00080000; ///< <see cref="casGetOptions"/> bit: device supports averaging on device, see <see cref="mpidAveragingOnDevice"/>
        public const int coCanMultiTrackSensorTemp        = 0x00100000; ///< <see cref="casGetOptions"/> bit: device supports sensor temperature for MultiTrack, see <see cref="mpidCMTSensorTemp"/>
        public const int coAutoRangeErrorOnFailure        = 0x00200000; ///< <see cref="casGetOptions"/> bit: option to allow @ref autoRange measurements to return an error if it had to give up.
        public const int coAutoRangeBestIntTimeResolution = 0x00400000; ///< <see cref="casGetOptions"/> bit: option to allow @ref autoRange measurements to switch to the best possible <see cref="mpidIntTimeResolution"/>

        /// <summary>
        /// Returns the features and options the device currently supports or performs. 
        /// </summary>
        /// <param name="ADevice">The device / CASID</param>
        /// <returns>An integer that has the bits starting with <see cref="coShutter"/> set - each bit standing for an option or feature. </returns>
        /// <remarks><para>For historic reasons the options is a mixture of capabilities - which are defined by the device and cannot be changed - and actual options that can be enabled or disabled.</para>
        /// <para>Use the method <see cref="casSetOptionsOnOff"/> to turn specific options on or off.</para>
        /// <para>The following example checks if the device supports measuring the sensor temperature:</para>
        /// [Delphi]
        /// ~~~~~~~~~~~~~{.pas}
        /// if (casGetOptions(CASID) and coGetTemperature) <> 0 then
        ///   ShowMessage('Device supports measuring sensor temperature') else
        ///   ShowMessage('Device does NOT support measuring sensor temperature');
        /// ~~~~~~~~~~~~~
        /// </remarks>
        [DllImport(CASLibName)]
        public static extern int casGetOptions(int ADevice);

        /// <summary>
        /// This method can set or clear several options for the device
        /// </summary>
        /// <param name="ADevice">The device / CASID</param>
        /// <param name="AOptions">Integer which has all bits set for the corresponding options which should modified. Bits start with <see cref="coShutter"/></param>
        /// <param name="AOnOff">0 if the options should be disabled, all other values will enable them</param>
        /// <remarks>
        /// <para>Some options are defined by the device and should not be changed manually, see <see cref="casGetOptions"/>.</para>
        /// [Delphi]
        /// ~~~~~~~~~~~~~{.pas}
        /// //turn AutoRange and AutoRange filter on
        /// casSetOptionsOnOff(CASID, coAutorangeMeasurement or coAutorangeFilter, 1);
        /// 
        /// //turn UseTransmission off
        /// casSetOptionsOnOff(CASID, coUseTransmission, 0);
        /// ~~~~~~~~~~~~~
        /// </remarks>
        [DllImport(CASLibName)]
        public static extern void casSetOptionsOnOff(int ADevice, int AOptions, int AOnOff);

        /// <summary>
        /// This method sets and clears all device options for the device 
        /// </summary>
        /// <param name="ADevice">The device / CASID</param>
        /// <param name="AOptions">Integer which has all bits set for the corresponding options which should be enabled and all bits cleared whose corresponding options should be disabled. Bits start with <see cref="coShutter"/></param>
        /// <remarks>Since some options are defined by the hardware and should not be changed manually, it is recommend to use the method <see cref="casSetOptionsOnOff"/> instead as it allows setting and clearing only the bits you actually want to modify.</remarks>
        [DllImport(CASLibName)]
        public static extern void casSetOptions(int ADevice, int AOptions);
        ///@}

        ///@name Measurement APIs
        ///

        ///@{
        /// <summary>
        /// Performs a measurement for the given device using the measurement parameter which have been previously set
        /// </summary>
        /// <param name="ADevice">The device / CASID</param>
        /// <returns>Negative return values indicate an error which can be translated into an error message using <see cref="casGetErrorMessage"/>.
        ///    All other return values indicate that the measurement was successful</returns>
        /// <remarks>
        /// <para>The spectrum is stored internally and can be accessed using <see cref="casGetData"/>.</para>
        /// <para>Before starting a measurement, you should check whether the @ref densityFilter "density filter"
        ///    needs to be updated and whether a @ref darkCurrent "dark current" measurement is necessary.
        /// Calling <see cref="casPerformActionEx"/> with paPrepareMeasurement is also recommended.</para>
        /// See chapter @ref performingAMeasurement "performing a measurement" for all prerequisites and more details.
        /// If the AutoRange option is enabled (see <see cref="casGetOptions"/>): 
        /// <list type="bullet"> <item>the integration time is automatically 
        /// determined (within the bounds of the <see cref="mpidAutoRangeMinLevel"/>, <see cref="mpidAutoRangeMaxLevel"/> and 
        /// <see cref="mpidAutoRangeMaxIntTime"/>). Refer to the chapter @ref autoRange for more details. </item>
        /// <item>using a dark array may be helpful (see chapter @ref darkCurrent "Dark current"), because casMeasure 
        /// may automatically perform dark current measurements and even change the density filter if necessary and 
        /// enabled (<see cref="coAutorangeFilter"/>)</item>
        /// </remarks>
        [DllImport(CASLibName)]
        public static extern int casMeasure(int ADevice);

        /// <summary>
        /// Starts a measurement for the given device and returns immediately
        /// </summary>
        /// <param name="ADevice">The device / CASID</param>
        /// <returns>The return value depends on the interface type. Use <see cref="casGetError"/> to retrieve error information</returns>
        /// <remarks>
        /// <para>Unlike <see cref="casMeasure"/>, this method does not wait until the measurement has been performed 
        /// and it also doesn't store the measured spectrum internally. Use the method <see cref="casFIFOHasData"/> to 
        /// wait until the measurement has finished and <see cref="casGetFIFOData"/> to store the spectrum internally.</para>
        /// <para>@note It is not guaranteed that all interface types actually return immediately. 
        /// Some may return only after the integration time is over.</para>
        /// <para>Before starting a measurement, you should check whether the @ref densityFilter "density filter"
        ///    needs to be updated and whether a @ref darkCurrent "dark current" measurement is necessary.
        /// Calling <see cref="casPerformActionEx"/> with paPrepareMeasurement is also recommended.</para>
        /// </remarks>
        [DllImport(CASLibName)]
        public static extern int casStart(int ADevice);

        /// <summary>
        /// Use this method to check if the spectrometer has data available which can be read
        /// </summary>
        /// <param name="ADevice">The device / CASID</param>
        /// <returns>The return value is 0 if the spectrometer is still performing the acquisition, and 1 if FIFO data is ready to be read.
        /// Negative return values indicate an error which can be translated into an error message using <see cref="casGetErrorMessage"/></returns>
        /// <remarks>
        /// <para>Calling casFIFOHasData is only necessary if a measurement was started with <see cref="casStart"/>. 
        /// It allows to check if the integration time of the spectrometer is over.</para>
        ///    @note Not all spectrometer types do support this - check <see cref="dpidTriggerCapabilities"/>, namely the <see cref="tcoQueryHasData"/> bit.
        /// </remarks>
        [DllImport(CASLibName)]
        public static extern int casFIFOHasData(int ADevice);

        /// <summary>
        /// This method reads the acquired spectrum from the FIFO and stores it internally
        /// </summary>
        /// <param name="ADevice">The device / CASID</param>
        /// <returns>Negative return values indicate an error which can be translated into an error message using <see cref="casGetErrorMessage"/>.
        ///    All other return values indicate that the spectrum was read successfully</returns>
        /// <remarks>
        /// <para>The spectrum is sensitivity corrected and subsequent averages are performed if <see cref="mpidAverages"/> is > 1.</para>
        /// <para>Calling casGetFIFOData should only be done after a measurement was started with <see cref="casStart"/>. 
        /// <para>To access the acquired spectrum use <see cref="casGetData"/></para>
        /// </remarks>
        [DllImport(CASLibName)]
        public static extern int casGetFIFOData(int ADevice);

        /// <summary>
        /// Use casMeasureDarkCurrent to perform a dark current measurement for the given device.
        /// </summary>
        /// <param name="ADevice">The device / CASID</param>
        /// <returns>Negative return values indicate an error which can be translated into an error message using <see cref="casGetErrorMessage"/>.
        ///    All other return values indicate that the dark current was measured successfully</returns>
        /// <remarks>
        /// <para>@note This method does not close the shutter! Call <see cref="casSetShutter"/> to close the shutter before calling 
        /// casMeasureDarkCurrent and again to open it afterwards</para>
        /// <para>@warning Before measuring dark current, check whether the @ref densityFilter "density filter" needs to be moved!</para>
        /// Refer to chapter @ref darkCurrent "Dark Current" for more details and sample code.</para>
        /// <para>To access the measured dark current use <see cref="casGetDarkCurrent"/></para>
        /// </remarks>
        [DllImport(CASLibName)]
        public static extern int casMeasureDarkCurrent(int ADevice);
        ///@}
        
        ///@name Performing actions
        ///

        ///@{
        public const int paPrepareMeasurement            = 1; ///< <see cref="casPerformActionEx"/> ID: Prepares the spectrometer for the next measurement. This is intended to avoid wasting time within a measurement cycle and should be called before time-critical measurements are started. See chapter @ref performingAMeasurement for other prerequisites.
        public const int paLoadCalibration               = 3; ///< <see cref="casPerformActionEx"/> ID: Reloads the calibration file. Might be useful after certain calibration parts have been modified with <see cref="casClearCalibration"/> or <see cref="casSetCalibrationFactors"/>.
        public const int paCheckAccessories              = 4; ///< <see cref="casPerformActionEx"/> ID: Performs a check whether the current calibration and config files are suitable for the used spectrometer and optional optical probes equipped with an ID Key. The ID key is re-read during the method call. See chapter @ref usingSerialNumbers for more info.
        public const int paMultiTrackStart               = 5; ///< <see cref="casPerformActionEx"/> ID: Starts a MultiTrack measurement. The traditional way of starting MultiTrack with <see cref="casStart"/> is not supported by most interface types. See chapter @ref multiTrackMeasurements for an overview.
        public const int paAutoRangeFindParameter        = 7; ///< <see cref="casPerformActionEx"/> ID: performs AutoRange measurements to find suitable measurement parameter. See chapter @ref autoRange for details.
        public const int paCheckConfigUpToDate           = 12; ///< <see cref="casPerformActionEx"/> ID: Performs a check whether the device has more recent configuration/calibration files. Returns <see cref="ErrorConfigUpToDate"/> if that is the case and files should be downloaded using <see cref="dpidGetFilesFromDevice"/>
        public const int paSearchForDevices              = 13; ///< <see cref="casPerformActionEx"/> ID: Starts a search for devices (i.e. interface options) for one or all interface types (see below). For some interface types like USB, this search is synchronous, for others like Ethernet, the search is started asynchronously. See chapter @ref interfaceTypesAndOptions.  This action does not require that you pass a valid CASID when calling casPerformActionEx. If you want to search only for a specific interface type, pass the interface type as AParam1 to casPerformActionEx, otherwise leave AParam1 at 0. ErrorInvalidParameter will be returned if AParam1 is neither 0 nor a valid interface ID. If AParam2 is 1, some interface types, like USB, perform a full reset and search twice. This used to be necessary for console apps, but since SDK version 4.12 this is no longer required.
        public const int paPrepareShutDown               = 14; ///< <see cref="casPerformActionEx"/> ID: Stops all threads in the CAS library. Depending on how your app loads the library, it might be necessary to call this action from the main thread just before your app shuts down to avoid a dead-lock when the CAS library is unloaded. This action does not require that you pass a valid CASID when calling casPerformActionEx.
        public const int paRecalcSpectrum                = 16; ///< <see cref="casPerformActionEx"/> ID: Recalculates the spectrum from the raw data of the last measurement. This might be useful after you modified the calibration or something else that might influence the spectrum.
        public const int paUpdateSpectralCalibration     = 19; ///< <see cref="casPerformActionEx"/> ID: Recalculates the spectral calibration for all parameter sets. A typical reason to call this method, is when you modified the transmission directly via <see cref="gcfTransmissionFunction"/>. Whenever you change a measurement parameter that influences the spectral calibration (e.g. <see cref="mpidNewDensityFilter"/>), this action is called automatically.
        public const int paUpdateCompleteCalibration     = 20; ///< <see cref="casPerformActionEx"/> ID: Recalculates the X vector for the spectrum and resamples the transmission to the updated wavelength before finally recalculating the spectral calibration. This action should be called whenever the wavelength calibration was modified or when the transmission should be reloaded.
        public const int paCheckAccessoriesTransmission  = 21; ///< <see cref="casPerformActionEx"/> ID: Performs a check whether the current transmission file is suitable for the used spectrometer and optional optical probes equipped with an ID Key. The ID key is re-read during the method call. See chapter @ref usingSerialNumbers for more info.
        public const int paMeasureParamSets              = 24; ///< <see cref="casPerformActionEx"/> ID: Performs measurements for several parameter sets. AParam1 = FirstParamSet, AParam2 = LastParamSet. This action only returns after all measurements have been done or if an error occured. See @ref workingWithParameterSets.
        public const int paMeasureParamSetsDC            = 25; ///< <see cref="casPerformActionEx"/> ID: Performs DC measurements for several parameter sets. AParam1 = FirstParamSet, AParam2 = LastParamSet. This action only returns after all DC measurements have been done or if an error occured. See @ref darkCurrent and @ref workingWithParameterSets.
        public const int paPrepareForTransmission        = 27; ///< <see cref="casPerformActionEx"/> ID: Prepares CAS for transmission measurement where relevant parts of the calibration are disabled. Should be called once before transmission and reference measurements are done. Affects all parameter sets but only prepares the current parameter set for transmission measurements. Call <see cref="paRestoreCalibAfterTransmission"/> to restore CAS for normal calibrated measurements.
        public const int paRestoreCalibAfterTransmission = 28; ///< <see cref="casPerformActionEx"/> ID: Restores CAS for normal calibrated measurements after <see cref="paPrepareForTransmission"/> was called. Affects all parameter sets but only restores the current parameter set.

        /// <summary>
        /// Generic method which performs one of various actions with the specified device. This method replaces the now deprecated method casPerformAction.
        /// </summary>
        /// <param name="ADevice">The device / CASID. For some AActionID constants, a valid CASID is not required, so any value will be accepted.</param>
        /// <param name="AActionID">Integer defining the action to perform. One of the constants starting at <see cref="paPrepareMeasurement"/></param>
        /// <param name="AParam1">First integer parameter for the action to perform. Meaning depends on the action to be performed</param>
        /// <param name="AParam2">Second integer parameter for the action to perform. Meaning depends on the action to be performed</param>
        /// <param name="AParam3">Third pointer parameter for the action to perform. Meaning depends on the action to be performed</param>
        /// <returns>0 for success or a negative error code.</returns>
        [DllImport(CASLibName)]
        public static extern int casPerformActionEx(int ADevice, int AActionID, int AParam1, int AParam2, IntPtr AParam3);

        /// <summary>
        /// Deprecated. Use <see cref="casPerformActionEx"/> instead!
        /// </summary>
        [Obsolete("Please use casPerformActionEx instead.")]
        [DllImport(CASLibName)]
        public static extern int casPerformAction(int ADevice, int AID);
        ///@}

        ///@name Measurement parameter
        ///Constants starting with mpid (measurement parameter ID)
        ///@{
        public const int mpidIntegrationTime          = 01; ///<float: the integration time in ms which will be used for the next measurement for the currently active parameter set. Valid values range from <see cref="dpidIntTimeMin"/> to <see cref="dpidIntTimeMax"/>. Note that after changing the integration time, a new dark current measurement might be necessary. See chapter @ref darkCurrent for an overview. Note that both <see cref="mpidIntTimeResolution"/> and <see cref="mpidIntTimeAlignPeriod"/> will affect the actual integration time that is used when measuring. It is therefore important to verify and document mpidIntegrationTime after the measurement.
        public const int mpidAverages                 = 02; ///<integer: the number of averages which will be used for the next measurement for the currently active parameter set. Valid values range from 1 to <see cref="dpidAveragesMax"/>. Note that after changing the integration time, a new dark current measurement might be necessary. See chapter @ref darkCurrent for an overview.
        public const int mpidTriggerDelayTime         = 03; ///<float: delay time in ms between trigger and start of the spectrum measurement. Valid values range from <see cref="dpidTriggerDelayTimeMin"/> to <see cref="dpidTriggerDelayTimeMax"/>. Only applies to @ref triggeredMeasurements. <see cref="dpidTriggerCapabilities"/> must include <see cref="tcoTriggerDelay"/> for this to work.
        public const int mpidTriggerTimeout           = 04; ///<integer: timeout in ms which must not be exceeded when waiting for the external trigger or <see cref="ErrorTriggerTimeout"/> will be returned. Only applies to @ref triggeredMeasurements.
        public const int mpidCheckStart               = 05; ///<integer: first pixel which is included for checking the maximum value of the ADC during an acquisition, see <see cref="mpidMaxADCValue"/> and <see cref="mpidMaxADCPixel"/>; leave at 0 to always use the first visible pixel of the calibration; see <see cref="dpidDeadPixels"/>; use <see cref="casPixelToNm"/> and <see cref="casNmToPixel"/> for conversion between pixels an wavelength
        public const int mpidCheckStop                = 06; ///<integer: last pixel which is included for checking the maximum value of the ADC during an acquisition, see <see cref="mpidMaxADCValue"/> and <see cref="mpidMaxADCPixel"/>; leave at 0 to always use the last visible pixel of the calibration; see <see cref="dpidVisiblePixels"/>; use <see cref="casPixelToNm"/> and <see cref="casNmToPixel"/> for conversion between pixels an wavelength
        public const int mpidColormetricStart         = 07; ///<lower bound in nm of the spectral range which is used for the colormetric calculation. If it is smaller than the wavelength of the first visible pixel, then this wavelength is used (i.e. the colormetric range can never exceed the range of the spectrum). Default value is 380nm. If you want to use the full range, set mpidColormetricStart and mpidColormetricStop both to 0. See also @ref gettingTheSpectrumAndResults.
        public const int mpidColormetricStop          = 08; ///<upper bound in nm of the spectral range which is used for the colormetric calculation. If it is bigger than the wavelength of the last visible pixel, then this wavelength is used (i.e. the colormetric range can never exceed the range of the spectrum). Default value is 780nm. If you want to use the full range, set mpidColormetricStart and mpidColormetricStop both to 0. See also @ref gettingTheSpectrumAndResults.
        public const int mpidACQTime                  = 10; ///<float: returns the time in ms the last acquisition took. This includes integration time for all averages as well as the time to read out the FIFO. 
        public const int mpidMaxADCValue              = 11; ///<integer: returns the maximum value of the ADC during the last successful acquisition. Check this mpid for ADC-overflow by comparing it with the <see cref="dpidADCRange"/> device parameter. Do *not* calculate a relative signal level and/or saturation yourself using mpidMaxADCValue, but query <see cref="mpidSignalLevel"/> and/or <see cref="mpidRelSaturation"/> (see @ref saturationVersusSignalLevel). <see cref="mpidMaxADCPixel"/> returns the pixel at which this maximum occurred. The pixel range which is taken into account can be customized by <see cref="mpidCheckStart"/> and <see cref="mpidCheckStop"/>. See also the section about validating a measurement in @ref performingAMeasurement.
        public const int mpidMaxADCPixel              = 12; ///<integer: returns the pixel which had the maximum ADC value <see cref="mpidMaxADCValue"/> during the last successful acquisition. The pixel range which is taken into account can be customized by <see cref="mpidCheckStart"/> and <see cref="mpidCheckStop"/>. See also the section about validating a measurement in @ref performingAMeasurement.
        public const int mpidTriggerSource            = 14; ///<integer: the current trigger source. Either <see cref="trgSoftware"/> for software triggering or <see cref="trgFlipFlop"/> when a hardware trigger is used. See chapter @ref triggeredMeasurements for an overview.
        public const int mpidAmpOffset                = 15; ///<integer: returns the calculated amplifier offset of the last dark current measurement which has been performed with this device
        public const int mpidSkipLevel                = 16; ///<intensity below which spectral intensities are not taken into account during colormetric calculations (the so called SkipLevel). This maybe useful to suppress noise from affecting the colormetric calculation. Note that skip-leveling needs to be enabled with <see cref="mpidSkipLevelEnabled"/>.
        public const int mpidSkipLevelEnabled         = 17; ///<integer: determines whether skip-leveling is enabled (values <> 0). Set the actual intensity level using <see cref="mpidSkipLevel"/>
        public const int mpidScanStartTime            = 18; ///<integer: returns the value the internal timer had, when the last measurement was started. For @ref triggeredMeasurements the scan start is just before the spectrometer started accepting triggers, but not when the trigger actually arrived. See also <see cref="mpidTimeSinceScanStart"/> which might be useful for determining the timing at high-accuracy.
        public const int mpidAutoRangeMaxIntTime      = 19; ///<float: maximum integration time in milliseconds which must not be exceeded during an @ref autoRange.
        public const int mpidAutoRangeLevel           = 20; ///<deprecated; use mpidAutoRangeMinLevel below
        public const int mpidAutoRangeMinLevel        = 20; ///<minimum relative maxADCValue between 0 and 100 percent which must be reached during an @ref autoRange. See <see cref="mpidAutoRangeMaxLevel"/> for an upper limit.
        public const int mpidDensityFilter            = 21; ///<integer: returns previously set density filter or -1 if it never has been set. Use <see cref="mpidCurrentDensityFilter"/> to check the current physical filter position. When set, the filter wheel is immediately positioned. Valid values range from 0 to 7, otherwise <see cref="ErrorParamOutOfRange"/> will be returned. See chapter @ref densityFilter for an overview and @ref performingAMeasurement for prerequisites. mpidDensityFilter is not part of the @ref workingWithParameterSets "parameter sets", but <see cref="mpidNewDensityFilter"/> is. Use <see cref="casGetFilterName"/> to translate a filter index into a filter name.
        public const int mpidCurrentDensityFilter     = 22; ///<integer: returns the current physical position of the density filter wheel, if the spectrometer supports reading the filter position (see <see cref="coGetFilter"/> option). If the device has no filter (i.e. <see cref="coFilter"/> option missing), -1 will be returned, otherwise the filter index between 0 and 7.
        public const int mpidNewDensityFilter         = 23; ///<integer: the filter wheel position between 0 and 7 which should be used for the next measurement of the @ref workingWithParameterSets "current parameter set". See chapter @ref densityFilter for an overview and @ref performingAMeasurement for prerequisites. Reading mpidNewDensityFilter does not check the physical filter wheel position; use <see cref="mpidCurrentDensityFilter"/> for that.
        public const int mpidLastDCAge                = 24; ///<integer: returns the number of milliseconds that have passed, since the last @ref darkCurrent was measured for the @ref workingWithParameterSets "current parameter set" or -1 if no DC has been measured.
        public const int mpidRelSaturation            = 25; ///<returns the relative saturation (between 0 and 100%) of the previous successful measurement for the @ref workingWithParameterSets "current parameter set" (for a detailed description see @ref saturationVersusSignalLevel). mpidRelSaturation can be checked against the <see cref="dpidRelSaturationMin"/> and <see cref="dpidRelSaturationMax"/> range, to ensure that the measurement has a sufficient signal level. However, to check for oversaturated measurements use <see cref="mpidMaxADCValue"/> and check it against <see cref="dpidADCRange"/>. Because with averaging, mpidRelSaturation might be < 100% even though there were saturated spectra!
        public const int mpidPulseWidth               = 27; ///<float in ms: When measuring non-continuously operated (i.e. pulsed) DUTs: if the spectrum should be normalized to the pulse on-time instead of the integration time, set this parameter to the overall on-time of all pulses measured during <see cref="mpidIntegrationTime"/>. Note that this is only required if you want to end up with a hypothetical spectrum that simulates 100% on-time of the DUT, which might not be desirable. Leave this parameter at 0 to have the usual normalization.
        public const int mpidRemeasureDCInterval      = 28; ///<integer: the time in ms, after which an automatic dark current measurement should be invalidated, so <see cref="dpidDCRemeasureReasons"/> becomes non-zero. This setting only has an effect, if the <see cref="coAutoRemeasureDC"/> option has been activated. The interval is checked against <see cref="mpidLastDCAge"/>. See chapter @ref darkCurrent for an overview.
        public const int mpidFlashDelayTime           = 29; ///<float: the delay time of the flash output signal in milliseconds. Applies only when flash is activated, i.e. <see cref="mpidFlashType"/> <> <see cref="ftNone"/>. If the delay time is negative, the flash signal and the delay happen before starting the measurement, otherwise the delay and the flash signal occur after the measurement started. @note Not all spectrometers support flash delay! Check the relevant bits in <see cref="dpidTriggerCapabilities"/> to ensure that the desired behaviour is actually supported by the spectrometer.
        public const int mpidTOPAperture              = 30; ///<integer: the TOP aperture which should be taken into account when the calibration is applied to the spectrum. The TOP aperture parameter is a 0-based index ranging from 0 to 6 corresponding to the TOP apertures 1 to 7. This parameter doesn't actually adjust the aperture of the TOP, but controls which aperture calibration factor will be applied. @note If no TOP is used, leave <see cref="mpidTOPAperture"/> at 0. To find out whether a specific TOP aperture is calibrated, use <see cref="casGetCalibrationFactors"/> with <see cref="gcfTOPApertureFactor"/> to check whether the factor for the aperture is not 0.
        public const int mpidTOPDistance              = 31; ///<the distance in mm from the DUT to the reference plane of the TOP. This parameter doesn't actually adjust the distance of the TOP, but controls which calibration factor will be applied. @note To check which distance range is covered by the calibration, use <see cref="casGetCalibrationFactors"/> with <see cref="gcfTOPDistanceFactor"/>. If the calibration does not contain TOP distance factors, this parameter has no effect.
        public const int mpidTOPSpotSize              = 32; ///<returns the size of the measurement spot for the current <see cref="mpidTOPAperture"/> and <see cref="mpidTOPDistance"/>.
        public const int mpidTriggerOptions           = 33; ///<integer: bit-set describing current trigger options. The set consists of the various to<XXX> bits, starting with <see cref="toAcceptOnlyWhenReady"/>. @note Some options apply to hardware and software triggers! See chapter @ref triggeredMeasurements "Triggered Measurements" for an overview. Additionally a device type might enforce specific trigger options, for example <see cref="toShowACQState"/> is always enabled for some CAS USB devices with hardware triggered measurements (<see cref="mpidTriggerSource"/> = <see cref="trgFlipFlop"/>). 
        public const int mpidForceFilter              = 34; ///<integer: flag which controls whether the filter wheel is moved for every measurement, even if it had been set to the same position previously. Internally the flag only affects <see cref="dpidNeedDensityFilterChange"/>, it will always return True if the ForceFilter flag is set. @note Some device types might enable ForceFilter during <see cref="casInitialize"/>. This is typically done for devices which can read the current filter state from the device (<see cref="coGetFilter"/> in <see cref="casGetOptions"/>). If only one application and device handle access the spectrometer, setting mpidForceFilter back to 0 can help minimize the measurement cycle time, but note that some devices might even enforce that ForceFilter is enabled. This is typically done for devices where monitoring the filter is a very fast operation (<1ms). Refer to the chapter @ref densityFilter for an overview of all methods and parameter related to the density filter.
        public const int mpidFlashType                = 35; ///<integer: the type of the flash signal which should be emitted during or before a measurement. This signal is a short pulse during the integration time which is typically used to trigger flash lamps. To check whether the spectrometer supports a flash signal in general, check the <see cref="tcoFlashOutput"/> flag in <see cref="dpidTriggerCapabilities"/>. Use <see cref="mpidFlashOptions"/> to control further options related to the flash. Possible values for mpidFlashType are all ft<XXX> constants, starting at <see cref="ftNone"/>.
        public const int mpidFlashOptions             = 36; ///<integer: bit-set describing the flash options of the device. Only applies, if flash is enabled, i.e. <see cref="mpidFlashType"/> is different from <see cref="ftNone"/>. The bits start with <see cref="foEveryAverage"/>.
        public const int mpidACQStateLine             = 37; ///<integer: number of the digital out port which should be used for the <see cref="toShowACQState"/> option in <see cref="mpidTriggerOptions"/>). The port specified by this parameter is set to the level given by <see cref="mpidACQStateLinePolarity"/> after the spectrometer has been started and is ready to accept a trigger. The level is restored after the acquisition, i.e. after integration and read-out time. For possible port values refer to <see cref="casSetDigitalOut"/> and the hardware manual of the spectrometer. @note When setting this parameter the level of the given line will be changed immediately if the device has been initialized and the <see cref="toShowACQState"/> trigger option is enabled. @warning Not all device types support changing this parameter which also causes varying default values depending on the device type. Therefore always verify this parameter, if you're relying on a specific line. <para>See chapter @ref triggeredMeasurements for an overview of all related options and parameters. </para>
        public const int mpidACQStateLinePolarity     = 38; ///<integer: level of the ACQ state line. Only applies if the <see cref="toShowACQState"/> option is enabled in <see cref="mpidTriggerOptions"/>. The value is 0 for low level, all other values indicate high level. The port specified by <see cref="mpidACQStateLine"/> is set to this level after the spectrometer has been started and is ready to accept a trigger. The level is restored after the acquisition, i.e. after integration and read-out time. @note When setting this parameter the level of the digital port will be changed immediately, if the device has been initialized and the <see cref="toShowACQState"/> trigger option is enabled. @warning Not all device types support changing this parameter, which also causes varying default values depending on the device type. Therefore always verify this parameter, if you're relying on a specific polarity. We recommend using the low level for ACQ. You might also check the <see cref="tcoBusyStatePolarity"/> capability to see if the spectrometer supports a custom polarity. <para>See chapter @ref triggeredMeasurements for an overview of all related options and parameters. </para>
        public const int mpidBusyStateLine            = 39; ///<integer: number of the digital out port which should be used for the <see cref="toShowBusyState"/> option in <see cref="mpidTriggerOptions"/>. The port specified by this parameter is set to the level given by <see cref="mpidBusyStateLinePolarity"/> after the spectrometer has been started and is ready to accept a trigger. The level is restored after the trigger has been received. For possible port values refer to <see cref="casSetDigitalOut"/> and the hardware manual of the spectrometer. @note When setting this parameter the level of the given line will be changed immediately if the device has been initialized and the <see cref="toShowBusyState"/> trigger option is enabled. @warning Not all device types support changing this parameter, which also causes varying default values depending on the device type. Therefore always verify this parameter, if you're relying on a specific line. You might also check the <see cref="tcoBusyStatePolarity"/> capability to see if the spectrometer supports a custom polarity. <para>See chapter @ref triggeredMeasurements for an overview of all related options and parameters. </para>
        public const int mpidBusyStateLinePolarity    = 40; ///<integer: level of the Busy state line. Only applies if the <see cref="toShowBusyState"/> option is enabled in <see cref="mpidTriggerOptions"/>. The value is 0 for low level, all other values indicate high level. The port specified by <see cref="mpidACQStateLine"/> is set to this level after the spectrometer has been started and is ready to accept a trigger. The level is restored after the acquisition, i.e. after integration and read-out time. @note When setting this parameter the level of the digital port will be changed immediately, if the device has been initialized and the <see cref="toShowACQState"/> trigger option is enabled. @warning Not all device types support changing this parameter, which also causes varying default values depending on the device type. Therefore always verify this parameter, if you're relying on a specific polarity. We recommend using the low level for ACQ. <para>See chapter @ref triggeredMeasurements for an overview of all related options and parameters. </para>
        public const int mpidAutoFlowTime             = 41; ///<integer: returns the minimum flow time (in ms) the current source should switch on the DUT for, when it is triggered by either the busy or ACQ state line (<see cref="mpidTriggerOptions"/>). This time includes the <see cref="mpidIntegrationTime"/> for all <see cref="mpidAverages"/>, a <see cref="mpidTriggerDelayTime"/> as well as a flash delay, if it is negative (see <see cref="mpidFlashDelayTime"/>). The read-out time of the sensor is not included. See chapter @ref Synchronization, namely the section @ref sequenceTriggerCAS
        public const int mpidCRIMode                  = 42; ///<integer: controls the way the CRI is calculated (see <see cref="casCalculateCRI"/>). This is a global setting which affects all devices! Possible values are <see cref="criDIN6169"/>, which is the default, and <see cref="criCIE13_3_95"/>.
        public const int mpidObserver                 = 43; ///<integer: Deprecated! See @ref colorMetricAvoidmpidObserver. Determines, which observer is used for <see cref="casColorMetric"/>. This is a global setting which affects all devices! Possible values are 2° <see cref="cieObserver1931"/>, which is the default, and 10° <see cref="cieObserver1964"/>.
        public const int mpidTOPFieldOfView           = 44; ///<returns the field of view for the current <see cref="mpidTOPAperture"/> and <see cref="mpidTOPDistance"/>. @note Not all configurations support the calculation of field of view - in these cases mpidTOPFieldOfView will always return 0. It is recommended to only use this measurement condition when the <see cref="coTOPHasFieldOfViewConfig"/> option is returned by <see cref="casGetOptions"/>.
        public const int mpidCurrentCCDTemperature    = 46; ///< reading this mpid performs a temperature measurement and returns the sensor temperature in degrees Celsius. If the device does not support temperature measurements the return value will be NAN. To verify that the device supports temperature measurements, check the <see cref="coGetTemperature"/> flag in <see cref="casGetOptions"/>. @note This method call might cause an error if the sensor temperature is outside of the allowed range (<see cref="ErrorCCDTemperatureFail"/>). Therefore it is especially important to check <see cref="casGetError"/> after retrieving the current sensor temperature.
        public const int mpidLastCCDTemperature       = 47; ///< returns the previously measured sensor temperature in degrees Celsius. If the last temperature measurement is too old, a new temperature measurement will be performed. The interval which triggers a new temperature measurement is device dependent. If the device does not support temperature measurements the return value will be NAN. To verify that the device supports temperature measurements, check the <see cref="coGetTemperature"/> flag in <see cref="casGetOptions"/>. @note If a new temperature measurement had to be performed, this method call might cause an error if the sensor temperature is outside of the allowed range (<see cref="ErrorCCDTemperatureFail"/>), so it is especially important to check <see cref="casGetError"/>.
        public const int mpidDCCCDTemperature         = 48; ///< returns the sensor temperature in degrees Celsius which was previously measured during the dark current measurement. If the dark current hasn't been measured or if the device does not support temperature measurements, the return value will be NAN. To verify that the device supports temperature measurements, check the <see cref="coGetTemperature"/> flag in <see cref="casGetOptions"/>.
        public const int mpidAutoRangeMaxLevel        = 49; ///<maximum relative maxADCValue between 0 and 100 percent which must not be exceeded during an @ref autoRange. See <see cref="mpidAutoRangeMinLevel"/> for the lower limit.
        public const int mpidMultiTrackAcqTime        = 50; ///<integer: returns the acquisition time in milliseconds of the complete @ref multiTrackMeasurements "MultiTrack measurement series". The accuracy of the timing varies depending on the spectrometer interface type.
        public const int mpidTimeSinceScanStart       = 51; ///<float: returns the time in milliseconds since the spectrometer was started, i.e. <see cref="mpidScanStartTime"/>. This mpid can replace calls to casStopTime, which has been deprecated.
        public const int mpidCMTTrackStart            = 52; ///<float: returns the time in milliseconds between the start of the @ref multiTrackMeasurements "MultiTrack measurement series" and the start of the current track (which was previously set with <see cref="casMultiTrackCopyData"/>). For the first track this is typically 0, the second it would be <see cref="mpidIntegrationTime"/>, and so on.
        public const int mpidColormetricWidthLevel    = 54; ///<integer: level in % which should be used for the next calculation of <see cref="casGetWidthEx"/>. Default is 50% to calculate FWHM. Independent for each parameter set, see @ref workingWithParameterSets
        public const int mpidIntTimeResolution        = 55; ///<float in microseconds, resolution for <see cref="mpidIntegrationTime"/>. Supported values can be queried using <see cref="dpidIntTimePossibleResolutions"/> after the device has been initialized. Might return <see cref="ErrorParamOutOfRange"/> when set to an unsupported value. Attention: changing the mpidIntTimeResolution will affect <see cref="dpidIntTimeMax"/>, so <see cref="mpidIntegrationTime"/> needs to be checked for being inside the allowed range.
        public const int mpidIntTimeAlignPeriod       = 56; ///<float in ms: if non-zero positive, every measurement (also AutoRange, but not DC array measurements) uses an integration time which is a mulitple of this period. Check <see cref="mpidIntegrationTime"/> *after* the measurement for the actual integration time that was measured with.
        public const int mpidColormetricType          = 57; ///<integer: determines the type of colormetric calculations that should be performed. Default is <see cref="cmtDefaultColormetric"/>. Affects a wide range of calculation methods, e.g. <see cref="casColorMetric"/>, <see cref="casGetCCT"/>, <see cref="casCalculateCRI"/> and <see cref="casCalculateLambdaDom"/>. Note: the wavelength range covered by the photopic luminosity function V(&Lambda;) and color matching functions x(&Lambda;), y(&Lambda;), z(&Lambda;) depend on mpidColormetricType. For <see cref="cmtDefaultColormetric"/> the functions range from 380nm to 780nm and for <see cref="cmtSWPColormetric"/> they have a broader range of 360nm to 830nm. When using cmtSWPColormetric it is especially important to adjust <see cref="mpidColormetricStart"/> and <see cref="mpidColormetricStop"/>, because their historic default values of 380nm and 780nm would unexpectedly constrain the calculation of the affected results. Set both mpidColormetricStart and mpidColormetricStop to zero in order to use the full range of the spectrum for calculation.
        public const int mpidAveragingOnDevice        = 58; ///<integer: determines whether averaging happens on the device (values <> 0) or in software, i.e. the CAS SDK. Only supported by some devices; check the <see cref="coCanAverageOnDevice"/> capability. If the device does not support averaging on device, getting the mpid will always return 0. Setting it to something other than 0 will return <see cref="ErrorFeatureNotSupported"/>. Averaging on device might be helpful for a large number of averages with very short integration times, as only the averaged spectrum is transfered. It should however be taken into account that with this option enabled, the ADC non-linearity correction is only performed on the averaged spectrum and not on each measured spectrum before they are averaged.
        public const int mpidMeasured                 = 59; ///<integer (read-only): returns True (value <> 0) if the current parameter set was successfully measured. This flag is reset to False when a new measurement or DC measurement is started. When measuring single parameter sets, evaluating this flag is typically not necessary, as error checking the calls to <see cref="casMeasure"/> etc. is sufficient. When measuring multiple parameter sets with <see cref="paMeasureParamSets"/> or <see cref="paMeasureParamSetsDC"/>, all affected parameter sets get their measured flag reset immediately. This allows determing which spectra were correctly measured, even if an error occured later on in the call.
        public const int mpidCMTSensorTemp            = 61; ///<float (read-only): returns the sensor temperature in degree Celsius for the @ref multiTrackMeasurements "MultiTrack measurement series" of the current track (which was previously set with <see cref="casMultiTrackCopyData"/>). Only devices returning <see cref="coCanMultiTrackSensorTemp"/> support this, others will just return 0.
        public const int mpidFlashDuration            = 63; ///<float: the duration of the flash signal in millseconds. Minimum supported value is <see cref="dpidFlashDurationMin"/>, but only if <see cref="dpidTriggerCapabilities"/> contains <see cref="tcoFlashHardwareDuration"/>
        public const int mpidSignalLevel              = 66; ///<float: the relative part of the used ADC range which was used for detecting light when the last spectrum was measured. See @ref saturationVersusSignalLevel "More about Saturation and Signal Level" for a detailed description.
        public const int mpidColormetricPeakDiffWidth = 67; ///<float: the width in nm that is used for differentiation when calculating the peak wavelength <see cref="casGetPeak"/>. If negative, a default width, which is suitable for your spectrometer model, is used. Default value is -1. Applies to the current CAS ID and the currently active ParamSet only!
        public const int mpidFullSensitivityDelayTime = 68; ///<float: when your DUT is not continuously powered on during the complete integration time, this is the delay time in ms, after the trigger is sent or the ACQ output signals start of the acquisition, after which the spectrometer reaches full sensitivity and it is safe to start powering on the DUT.
        public const int mpidEffectiveAcquisitionTime = 69; ///<float: when your DUT is not continuously powered on during the complete integration time, this is the part of the integration time in ms, where the spectrometer can correctly measure. It starts after <see cref="mpidFullSensitivityDelayTime"/>. If this value is set, <see cref="mpidIntegrationTime"/> is adjusted.
        public const int mpidAdditionalObserver       = 71; ///<integer: the 1-based index of the observer to be used for calculation additional observer results, or 0 for none. For an overview, see @ref colorMetricAdditionalObservers.
        public const int mpidAOIllRefx              = 72; ///<float: color coordinate x for calculating the additional observer results @ref ecvAOLambdaDom and @ref ecvAOPurity. Default = 1/3 for Illuminant E. For an overview, see @ref colorMetricAdditionalObservers.
        public const int mpidAOIllRefy              = 73; ///<float: color coordinate y for calculating the additional observer results @ref ecvAOLambdaDom and @ref ecvAOPurity. Default = 1/3 for Illuminant E. For an overview, see @ref colorMetricAdditionalObservers.

        public const int cmtDefaultColormetric = 0; ///< <see cref="mpidColormetricType"/> value constant: default CAS SDK colormetric like in CAS SDK version 4.9 and before
        public const int cmtSWPColormetric = 100; ///< <see cref="mpidColormetricType"/> value constant: SWP colormetric like in SWP 3.6 and beyond. Note that SWP colormetric calculation might be taking more time than cmtDefaultColormetric due to the increased accuracy of the results.

        public const int toAcceptOnlyWhenReady   = 1; ///< <see cref="mpidTriggerOptions"/> bit: the hardware trigger is only accepted when the spectrometer is ready for acquisition, i.e. the <see cref="dpidLine1FlipFlop"/> is reset before the spectrometer starts waiting for a trigger. This option flag can only be modified if <see cref="tcoTriggerOnlyWhenReady"/> is included in <see cref="dpidTriggerCapabilities"/>. Otherwise it is either enforced or cleared, depending on the spectrometer type.
        public const int toForEachAutoRangeTrial = 2; ///< <see cref="mpidTriggerOptions"/> bit: for hardware triggered @ref autoRange "AutoRange measurement" this flag controls, whether each acquisition during AutoRange requires a hardware trigger or only the first one. Since the number of acquisitions during AutoRange can't be determined in advance, the spectrometer should be triggered whenever either the busy state or the ACQ state (see below) indicate that it is waiting for a trigger. Only supported if <see cref="tcoAutoRangeTriggering"/> flag is set in <see cref="dpidTriggerCapabilities"/>! Has no effect if <see cref="mpidTriggerSource"/> is not <see cref="trgFlipFlop"/>.
        public const int toShowBusyState         = 4; ///< <see cref="mpidTriggerOptions"/> bit: enables the busy signal. As soon as the spectrometer is ready to accept a trigger, the signal on the digital out port identified by <see cref="mpidBusyStateLine"/> changes to the level specified by <see cref="mpidBusyStateLinePolarity"/>. The signal is reset as soon as the trigger was received. Only supported if <see cref="tcoShowBusyState"/> flag is set in <see cref="dpidTriggerCapabilities"/>!
        public const int toShowACQState          = 8; ///< <see cref="mpidTriggerOptions"/> bit: enables the acquisition signal. The digital out port identified by <see cref="mpidACQStateLine"/> changes to the level given by <see cref="mpidACQStateLinePolarity"/> as soon as the spectrometer is waiting for a trigger and is reset only after the acquisition has been finished, i.e. after integration and read-out time. Only supported if <see cref="tcoShowACQState"/> flag is set in <see cref="dpidTriggerCapabilities"/>!

        public const int ftNone     = 0; ///< <see cref="mpidFlashType"/> value constant: No flash signal is emitted. Default value and "supported" by all spectrometers. 
        public const int ftHardware = 1; ///< <see cref="mpidFlashType"/> value constant: The flash signal is controlled by the spectrometer. Refer to the hardware manual for details. Depending on the flag <see cref="tcoFlashHardwareDelay"/> and related flags, this might enable the <see cref="mpidFlashDelayTime"/>. Only supported if the <see cref="tcoFlashHardware"/> flag is set in <see cref="dpidTriggerCapabilities"/>.
        public const int ftSoftware = 2; ///< <see cref="mpidFlashType"/> value constant: The flash signal is controlled by the CAS-DLL. Depending on the flag <see cref="tcoFlashSoftwareDelay"/> and related flags, this might enable the <see cref="mpidFlashDelayTime"/>. Software flash in general is only supported if the <see cref="tcoFlashSoftware"/> flag is set in <see cref="dpidTriggerCapabilities"/>.

        public const int foEveryAverage = 1; ///< <see cref="mpidFlashOptions"/> bit: if set, the flash signal is emitted for each acquisition when <see cref="mpidAverages"/> is bigger than 1. Only applies to hardware flash and only supported by some devices (check <see cref="tcoFlashHardwareForEachAverage"/> in <see cref="dpidTriggerCapabilities"/>).

        public const int trgSoftware = 0; ///< <see cref="mpidTriggerSource"/> value constant: no external trigger, <see cref="casMeasure"/> immediately starts the acquisition
        public const int trgFlipFlop = 3; ///< <see cref="mpidTriggerSource"/> value constant: external trigger at PIN 1 (DB9) is used to start the acquisition. See chapter @ref triggeredMeasurements for a detailed overview on this topic.

        public const int criDIN6169    = 0; ///< <see cref="mpidCRIMode"/> value constant: CRI calculation according to DIN 6169
        public const int criCIE13_3_95 = 1; ///< <see cref="mpidCRIMode"/> value constant: CRI calculation according to CIE 13.3-95

        public const int cieObserver1931 = 0; ///< <see cref="mpidObserver"/> value constant: 2° observer according to CIE 1931
        public const int cieObserver1964 = 1; ///< <see cref="mpidObserver"/> value constant: 10° observer according to CIE 1964

        /// <summary>
        /// Returns a numeric measurement parameter for a given spectrometer
        /// </summary>
        /// <param name="ADevice">The device / CASID</param>
        /// <param name="AWhat">Integer constant, defining which parameter to return. One of the mpid<XXX> constants starting at <see cref="mpidIntegrationTime"/></param>
        /// <returns>The return value is the measurement parameter itself and value depends on AWhat. Call <see cref="casGetError"/> immediately afterwards to do proper See @ref errorHandling</returns>
        /// <remarks>
        /// After performing a measurement, it is often recommended to query the various measurement parameter to build a list of measurement conditions for documentation.
        /// </remarks>
        [DllImport(CASLibName)]
        public static extern double casGetMeasurementParameter(int ADevice, int AWhat);

        /// <summary>
        /// Sets a numeric measurement parameter for a given spectrometer
        /// </summary>
        /// <param name="ADevice">The device / CASID</param>
        /// <param name="AWhat">Integer constant, defining which parameter to modify. One of the mpid<XXX> constants starting at <see cref="mpidIntegrationTime"/></param>
        /// <param name="AValue">Double holding the new value the measurement parameter should have</param>
        /// <returns>The return value is 0 if successful. Negative values indicate an error code. See @ref errorHandling</returns>
        /// <remarks>
        /// Typical error codes include <see cref="ErrorParamOutOfRange"/> and <see cref="ErrorInvalidParameter"/>.
        /// </remarks>
        [DllImport(CASLibName)]
        public static extern int casSetMeasurementParameter(int ADevice, int AWhat, double AValue);
        ///@}

        ///@name Shutter and Filter
        ///
        ///@{
        public const int casShutterInvalid = -1; ///< return value of <see cref="casGetShutter"/> if current shutter state is invalid (device failure) or device has no shutter (check using <see cref="casGetOptions"/>)
        public const int casShutterOpen    = 0; ///< return value of <see cref="casGetShutter"/> if current shutter state is open, i.e. normal spectra will be measured
        public const int casShutterClose   = 1; ///< return value of <see cref="casGetShutter"/> if current shutter state is closed, i.e. dark current will be measured
        
        /// <summary>
        /// Returns the current shutter position of the given device
        /// </summary>
        /// <param name="ADevice">The device / CASID</param>
        /// <returns>The return value is one of the casShutter constants starting with <see cref="casShutterInvalid"/> or one of the error codes, e.g. <see cref="errCasDeviceNotFound"/></returns>
        /// <remarks>
        /// Note that the actual shutter position of the device is only checked if the device supports the <see cref="coGetShutter"/>. Otherwise the most recently set 
        /// shutter state is returned, which may not reflect the actual shutter position.
        /// </remarks>
        [DllImport(CASLibName)]
        public static extern int casGetShutter(int ADevice);

        /// <summary>
        /// Sets the shutter position of the given device.
        /// </summary>
        /// <param name="ADevice">The device / CASID</param>
        /// <param name="OnOff">Desired shutter state: either <see cref="casShutterOpen"/> or <see cref="casShutterClose"/></param>
        /// <remarks>
        /// If the device does not have a shutter (<see cref="coShutter"/> option missing), this method will not raise an error!
        /// @warning Error handling for this method using <see cref="casGetError"/> is essential!
        /// </remarks>
        [DllImport(CASLibName)]
        public static extern void casSetShutter(int ADevice, int OnOff);

        /// <summary>
        /// Translates a filter index into a user-readable density filter name.
        /// </summary>
        /// <param name="ADevice">The device / CASID</param>
        /// <param name="AFilter">The 0-based filter index ranging from 0 to 7</param>
        /// <param name="Dest">Destination buffer for the filter name</param>
        /// <param name="AMaxLen">The maximum number of characters Dest can hold</param>
        /// <returns>The return value is identical to Dest or nil if there was an error</returns>
        /// <remarks>
        /// The density filter names are defined in the configuration file of the device, so the device must have been initialized (see <see cref="casInitialize"/>).
        /// A default name "Filter x" will be returned when the configuration doesn't define a name for a given filter.
        /// </remarks>
        [DllImport(CASLibName, CharSet = CharSet.Auto)]
        public static extern IntPtr casGetFilterName(int ADevice, int AFilter, StringBuilder Dest, int AMaxLen);
        ///@}

        ///@name Calibration

        ///@{
        /// <summary>
        /// Applies the spectral correction to the previously acquired raw spectrum.
        /// </summary>
        /// <param name="ADevice">The device / CASID</param>
        /// <remarks>
        /// This method causes the spectral correction to be applied to the previously acquired raw spectrum but without a transmission correction defined by <see cref="dpidTransmissionFileName"/>! 
        /// This is done automatically after a measurement (but taking the transmission correction into account if applicable), so normally there is no need to call this method explicitly.
        ///
        /// If you want to apply the transmission correction as well, use <see cref="casConvoluteTransmission"/> instead.
        /// @note This method call is useful if for one spectrum, you want the same spectrum with and without the transmission correction applied.
        /// </remarks>
        [DllImport(CASLibName)]
        public static extern void casCalculateCorrectedData(int ADevice);

        /// <summary>
        /// Applies the spectral and transmission correction to the previously acquired raw spectrum.
        /// </summary>
        /// <param name="ADevice">The device / CASID</param>
        /// <remarks>
        /// This method causes the spectral and transmission correction to be applied to the previously acquired raw spectrum! 
        /// This is done automatically after a measurement if the <see cref="coUseTransmission"/> option is enabled, so 
        /// normally there is no need to call this method explicitly. The file containing the transmission correction must have 
        /// been set using <see cref="dpidTransmissionFileName"/> before the device was initialized with <see cref="casInitialize"/>!
        ///
        /// If you don't want to apply the transmission correction as well, use <see cref="casCalculateCorrectedData"/> instead.
        /// @note This method call is useful if for one spectrum, you want the same spectrum with and without the transmission correction applied.
        /// </remarks>
        [DllImport(CASLibName)]
        public static extern void casConvoluteTransmission(int ADevice);

        public const int gcfDensityFunction = 0;///< <see cref="casGetCalibrationFactors"/> AWhat: spectral calibration of the density filters
        public const int gcfSensitivityFunction = 1;///< <see cref="casGetCalibrationFactors"/> AWhat: spectral calibration
        public const int gcfTransmissionFunction = 2;///< <see cref="casGetCalibrationFactors"/> AWhat: transmission of additional optical hardware
        public const int gcfDensityFactor = 3;///< <see cref="casGetCalibrationFactors"/> AWhat: density filter factors
        public const int gcfTOPApertureFactor = 4;///< <see cref="casGetCalibrationFactors"/> AWhat: absolute calibration for TOP
        public const int gcfTOPDistanceFactor = 5;///< <see cref="casGetCalibrationFactors"/> AWhat: distance calibration factors for TOP
        public const int gcfTDDistanceMin = -3;///< <see cref="casGetCalibrationFactors"/> AIndex with AWhat=<see cref="gcfTOPDistanceFactor"/> to retrieve the minimum calibrated TOP distance
        public const int gcfTDDistanceMax = -2;///< <see cref="casGetCalibrationFactors"/> AIndex with AWhat=<see cref="gcfTOPDistanceFactor"/> to retrieve the maximum calibrated TOP distance
        public const int gcfTDCount = -1;///< <see cref="casGetCalibrationFactors"/> AIndex with AWhat=<see cref="gcfTOPDistanceFactor"/> to retrieve the number of TOP distance factors
        public const int gcfTDExtraDistance = 1;///< <see cref="casGetCalibrationFactors"/> AExtra with AWhat=<see cref="gcfTOPDistanceFactor"/> to retrieve a given TOP distance at AIndex
        public const int gcfTDExtraFactor = 2;///< <see cref="casGetCalibrationFactors"/> AExtra with AWhat=<see cref="gcfTOPDistanceFactor"/> to retrieve a given TOP distance factor at AIndex
        public const int gcfWLCalibrationChannel = 6;///< <see cref="casGetCalibrationFactors"/> AWhat: wavelength calibration channels, get/set with 0-based AIndex to get/set channels, i.e. Pixels with calibration points
        public const int gcfWLExtraFirstCoefficient = -6;///< <see cref="casGetCalibrationFactors"/> AExtra with AWhat=<see cref="gcfWLCalibrationChannel"/> to retrieve the first coefficient C0 of a polynom wavelength calibration; check for non-zero to find out whether polynom calibration is used. 
        public const int gcfWLExtraLastCoefficient = -2;///< <see cref="casGetCalibrationFactors"/> AExtra with AWhat=<see cref="gcfWLCalibrationChannel"/> to retrieve the last coefficient C4 of a polynom wavelength calibration
        public const int gcfWLCalibPointCount = -1;///< <see cref="casGetCalibrationFactors"/> AExtra with AWhat=<see cref="gcfWLCalibrationChannel"/> to retrieve the number of wavelength calibration points
        public const int gcfWLExtraCalibrationDelete = 1;///< <see cref="casSetCalibrationFactors"/> AExtra with AWhat=<see cref="gcfWLCalibrationChannel"/>: set to delete WL calibration point at AIndex 
        public const int gcfWLExtraCalibrationDeleteAll = 2;///< <see cref="casSetCalibrationFactors"/> AExtra with AWhat=<see cref="gcfWLCalibrationChannel"/>: set to delete ALL calibration points
        public const int gcfWLCalibrationAlias = 7;///< <see cref="casGetCalibrationFactors"/> AWhat: wavelength calibration wavelengths, get/set with 0-based AIndex to get/set wavelengths for corresponding channels/pixels
        public const int gcfWLCalibrationSave = 8;///< <see cref="casGetCalibrationFactors"/> AWhat: set to any value, to update configuration file with current wavelength calibration
        public const int gcfDarkArrayValues = 9;///< <see cref="casGetCalibrationFactors"/> AWhat: get/set integration times and actual dark current spectra of dark current array
        public const int gcfDarkArrayDepth = -1;///< <see cref="casGetCalibrationFactors"/> AExtra with AWhat=<see cref="gcfDarkArrayValues"/>: get/set depth/length of dark current array
        public const int gcfDarkArrayIntTime = -2;///< <see cref="casGetCalibrationFactors"/> AExtra with AWhat=<see cref="gcfDarkArrayValues"/>: get/set integration time of dark current array at AIndex
        public const int gcfTOPParameter = 11;///< <see cref="casGetCalibrationFactors"/> AWhat: get various TOP parameter
        public const int gcfTOPApertureSize = 0;///< <see cref="casGetCalibrationFactors"/> AExtra with AWhat=<see cref="gcfTOPParameter"/>: get aperture size of TOP aperture AIndex
        public const int gcfTOPSpotSizeDenominator = 1;///< <see cref="casGetCalibrationFactors"/> AExtra with AWhat=<see cref="gcfTOPParameter"/>: get spot size distance denominator to calculate spot size from distance
        public const int gcfTOPSpotSizeOffset = 2;///< <see cref="casGetCalibrationFactors"/> AExtra with AWhat=<see cref="gcfTOPParameter"/>: get spot size distance offset to calculate spot size from distance
        public const int gcfLinearityFunction = 12;///< <see cref="casGetCalibrationFactors"/> AWhat: linearity calibration of the ADC
        public const int gcfLinearityCounts = 0;///< <see cref="casGetCalibrationFactors"/> AExtra with AWhat=<see cref="casGetCalibrationFactors"/>: get counts of linearity calibration array at AIndex
        public const int gcfLinearityFactor = 1;///< <see cref="casGetCalibrationFactors"/> AExtra with AWhat=<see cref="casGetCalibrationFactors"/>: get factor of linearity calibration array at AIndex
        public const int gcfRawData = 14;///< <see cref="casGetCalibrationFactors"/> AWhat: retrieve raw data of previously acquired spectrum, AIndex = channel/pixel

        /// <summary>
        /// Returns various details about the configuration/calibration of the given device.
        /// </summary>
        /// <param name="ADevice">The device / CASID</param>
        /// <param name="AWhat">What calibration part to retrieve: one of the gcf<XXX> constants, e.g. <see cref="gcfDensityFunction"/>.</param>
        /// <param name="AIndex">For calibration parts which consist of lists or spectra, this is the index or pixel for which to retrieve 
        /// the calibration information. See the documentation for the gcf<XXX> constant used as AWhat.</param>
        /// <param name="AExtra">Additional constant to indicate what calibration info to retrieve. Often another gcf<XXX> constant, e.g. <see cref="gcfDarkArrayDepth"/>.</param>
        /// <returns>The return value is the calibration information identified by the parameter which were passed.</returns>
        /// <remarks>
        /// The following table lists the possible values for AIndex and AExtra for the different AWhat constants.
        /// <table>
        /// <tr><td>AWhat</td><td>AIndex</td><td>AExtra</td><td>Description</td></tr>
        /// <tr><td><see cref="gcfDensityFunction"/></td><td>0..Pixel-1</td><td>Density Filter (0..7)</td><td>Spectral calibration of the density filters</td></tr>
        /// <tr><td><see cref="gcfSensitivityFunction"/></td><td>0..Pixel-1</td><td>Unused</td><td>Spectral calibration of the CAS</td></tr>
        /// <tr><td><see cref="gcfTransmissionFunction"/></td><td>0..Pixel-1</td><td>Unused</td><td>Transmission of additional optical hardware</td></tr>
        /// <tr><td><see cref="gcfDensityFactor"/></td><td>0..7</td><td>Unused</td><td>Absolute calibration for the density filters</td></tr>
        /// <tr><td><see cref="gcfTOPApertureFactor"/></td><td>0..6</td><td>Unused</td><td>Absolute calibration factor for TOP, aperture given by Index. Check against 0 to find out, which TOP apertures are calibrated.</td></tr>
        /// <tr><td rowspan=5><see cref="gcfTOPDistanceFactor"/></td><td><see cref="gcfTDDistanceMin"/></td><td>Unused</td><td>The minimum calibrated TOP distance</td></tr>
        /// <tr><td><see cref="gcfTDDistanceMax"/></td><td>Unused</td><td>The maximum calibrated TOP distance</td></tr>
        /// <tr><td><see cref="gcfTDCount"/></td><td>Unused</td><td>Number of distance and factor pairs</td></tr>
        /// <tr><td rowspan=2>0..n</td><td><see cref="gcfTDExtraDistance"/></td><td>TOP distance in mm</td></tr>
        /// <tr><td><see cref="gcfTDExtraFactor"/></td><td>TOP calibration factor for this distance</td></tr>
        /// <tr><td rowspan=2><see cref="gcfWLCalibrationChannel"/></td><td><see cref="gcfWLCalibPointCount"/></td><td>Unused</td><td>Number of calibration points of the wavelength calibration</td></tr>
        /// <tr><td>0..CalibPointCount-1</td><td>Unused</td><td>Wavelength calibration: pixel of the CalibPoint specified by Index</td></tr>
        /// <tr><td><see cref="gcfWLCalibrationAlias"/></td><td>0..CalibPointCount-1</td><td>Unused</td><td>Wavelength calibration: wavelength of the CalibPoint specified by Index</td></tr>
        /// <tr><td rowspan=3><see cref="gcfDarkArrayValues"/></td><td>unused</td><td><see cref="gcfDarkArrayDepth"/></td><td>Depth of the dark array, i.e. the number of dark currents in the array</td></tr>
        /// <tr><td>0..DarkArrayDepth-1</td><td><see cref="gcfDarkArrayIntTime"/></td><td>The integration time of the dark current measurement identified by Index.</td></tr>
        /// <tr><td>0..Pixel-1</td><td>0..DarkArrayDepth-1</td><td>The intensity of the dark current at the pixel given by Index and the dark current measurement identified by Extra.</td></tr>
        /// <tr><td rowspan=3><see cref="gcfTOPParameter"/></td><td>0..6</td><td><see cref="gcfTOPApertureSize"/></td><td>Size of the TOP aperture in mm</td></tr>
        /// <tr><td>Unused</td><td><see cref="gcfTOPSpotSizeDenominator"/></td><td>Denominator for calculation of the spot size of the TOP, rather use <see cref="casCalculateTOPParameter"/></td></tr>
        /// <tr><td>Unused</td><td><see cref="gcfTOPSpotSizeOffset"/></td><td>Offset for calculation of the spot size of the TOP, rather use <see cref="casCalculateTOPParameter"/></td></tr>
        /// <tr><td rowspan=2><see cref="gcfLinearityFunction"/></td><td rowspan=2>0..n</td><td><see cref="gcfLinearityCounts"/></td><td>ADC counts for linearity correction</td></tr>
        /// <tr><td><see cref="gcfLinearityFactor"/></td><td>Correction Factor for ADC range</td></tr>
        /// <tr><td><see cref="gcfRawData"/></td><td>0..Pixel - 1</td><td>Unused</td><td>Spectral raw data, normalized to 1 ms and 1 average</td></tr>
        /// </table>
        /// Some of these gcf<XXX> constants are also used by <see cref="casClearCalibration"/>.
        /// @warning Since this method returns the actual calibration information, error handling has to be done using <see cref="casGetError"/> calls. 
        /// Typical error codes are <see cref="ErrorInvalidParameter"/> and <see cref="ErrorParamOutOfRange"/>.
        /// When enumerating some calibration information lists, there might be no other way than increasing the AIndex parameter 
        /// until <see cref="ErrorParamOutOfRange"/> occurs.
        /// </remarks>
        [DllImport(CASLibName)]
        public static extern double casGetCalibrationFactors(int ADevice, int AWhat, int AIndex, int AExtra);

        /// <summary>
        /// Changes various details about the configuration/calibration of the given device effectively overriding information that normally comes from configuration/calibration files.
        /// </summary>
        /// <param name="ADevice">The device / CASID</param>
        /// <param name="AWhat">What calibration part to modify: one of the gcf<XXX> constants, e.g. <see cref="gcfDensityFunction"/>.</param>
        /// <param name="AIndex">For calibration parts which consist of lists or spectra, this is the index or pixel for which to modify
        /// the calibration information. See the documentation for the gcf<XXX> constant used as AWhat.</param>
        /// <param name="AExtra">Additional constant to indicate what calibration info to change. Often another gcf<XXX> constant, e.g. <see cref="gcfDarkArrayDepth"/>.</param>
        /// <param name="AValue">New value for the calibration information that is identified by the other parameter.</param>
        /// <remarks>
        /// The following table lists the possible values for AIndex and AExtra for the different AWhat constants.
        /// <table>
        /// <tr><td>AWhat</td><td>AIndex</td><td>AExtra</td><td>Description</td></tr>
        /// <tr><td><see cref="gcfDensityFunction"/></td><td>0..Pixel-1</td><td>Density Filter (0..7)</td><td>Spectral calibration of the density filters</td></tr>
        /// <tr><td><see cref="gcfSensitivityFunction"/></td><td>0..Pixel-1</td><td>Unused</td><td>Spectral calibration of the CAS</td></tr>
        /// <tr><td><see cref="gcfTransmissionFunction"/></td><td>0..Pixel-1</td><td>Unused</td><td>Transmission of additional optical hardware</td></tr>
        /// <tr><td><see cref="gcfDensityFactor"/></td><td>0..7</td><td>Unused</td><td>Absolute calibration for the density filters</td></tr>
        /// <tr><td><see cref="gcfTOPApertureFactor"/></td><td>0..6</td><td>Unused</td><td>Absolute calibration factor for TOP, aperture given by Index. Check against 0 to find out, which TOP apertures are calibrated.</td></tr>
        /// <tr><td rowspan=3><see cref="gcfTOPDistanceFactor"/></td><td><see cref="gcfTDCount"/></td><td>Unused</td><td>Number of distance and factor pairs</td></tr>
        /// <tr><td rowspan=2>0..n</td><td><see cref="gcfTDExtraDistance"/></td><td>TOP distance in mm</td></tr>
        /// <tr><td><see cref="gcfTDExtraFactor"/></td><td>TOP calibration factor for this distance</td></tr>
        /// <tr><td rowspan=3><see cref="gcfWLCalibrationChannel"/></td><td>Channel/Pixel to delete</td><td><see cref="gcfWLExtraCalibrationDelete"/></td><td>Deletes the calibration point for the channel/pixel value passed in Index. No error if no calibration point at this pixel.</td></tr>
        /// <tr><td>Unused</td><td><see cref="gcfWLExtraCalibrationDeleteAll"/></td><td>Deletes all calibration points. Identical to calling <see cref="casClearCalibration"/> with <see cref="gcfWLCalibrationChannel"/></td></tr>
        /// <tr><td colspan=3>Use <see cref="gcfWLCalibrationAlias"/> to define calibration points in one go.</td></tr>
        /// <tr><td><see cref="gcfWLCalibrationAlias"/></td><td>0..Pixels-1</td><td>Unused</td><td>Add/modify calibration point for pixel/channel given by Index and set it to the wavelength passed in AValue.</td></tr>
        /// <tr><td><see cref="gcfWLCalibrationSave"/></td><td>Unused</td><td>Unused</td><td>Immediately saves the modified wavelength calibration in the current configuration file specified by <see cref="dpidConfigFileName"/>.</td></tr>
        /// <tr><td rowspan=2><see cref="gcfDarkArrayValues"/></td><td>unused</td><td><see cref="gcfDarkArrayDepth"/></td><td>Depth of the dark array, i.e. the number of dark currents in the array</td></tr>
        /// <tr><td>0..DarkArrayDepth-1</td><td><see cref="gcfDarkArrayIntTime"/></td><td>Sets the integration time of the DarkArray at Index to the integration time passed in value. Note: the integration times in the dark array must be in ascending order and start with <see cref="dpidIntTimeMin"/>!</td></tr>
        /// <tr><td><see cref="gcfLinearityFunction"/></td><td>0..n</td><td>ADCCounts</td><td>Adds/modifies the linearity correction array to the correction factor passed in AValue for the ADC counts passed in AIndex. The array must be sorted in ascending order by ADCCounts.</td></tr>
        /// </table>
        /// @warning After changing the calibration <see cref="paUpdateSpectralCalibration"/> should be called before the next measurement is performed, 
        /// so the new calibration factors are used! Some actions (like changing the current parameter set or the density filter) do this implicitly so it might not always be necessary. 
        /// The wavelength calibration is updated automatically every time it is modified.
        /// <para>Use <see cref="casSaveCalibration"/> if you want to save the modified calibration as a .ISC file. The wavelength calibration can be saved by calling 
        /// casSetCalibrationFactors with <see cref="gcfWLCalibrationSave"/> as mentioned above. All the other information that is stored in the configuration file cannot 
        /// be saved by the CAS-DLL.</para>
        /// </remarks>
        [DllImport(CASLibName)]
        public static extern void casSetCalibrationFactors(int ADevice, int AWhat, int AIndex, int AExtra, double AValue);

        /// <summary>
        /// Saves the calibration of the given device to a file.
        /// </summary>
        /// <param name="ADevice">The device / CASID</param>
        /// <param name="AFileName">The full path where the calibration file should be stored. Typical file extension is .ISC</param>
        /// <remarks>
        /// This calibration file does not contain all details of the calibration, e.g. the wavelength calibration - which is saved in the configuration file. 
        /// Use <see cref="casSetCalibrationFactors"/> with <see cref="gfcWLCalibrationSave"/> to save the wavelength calibration in the current configuration file. 
        /// </remarks>
        [DllImport(CASLibName)]
        public static extern void casSaveCalibration(int ADevice, string AFileName);

        /// <summary>
        /// Clears the specified calibration part of the given device.
        /// </summary>
        /// <param name="ADevice">The device / CASID</param>
        /// <param name="AWhat">One of the gcf<XXX> constants, specifying which calibration part to delete. See table below for possible values.</param>
        /// <remarks>
        /// The following table lists the possible values for AWhat
        /// <table>
        /// <tr><td>AWhat</td><td>Description</td></tr>
        /// <tr><td><see cref="gcfDensityFunction"/></td><td>Spectral calibration of all density filters</td></tr>
        /// <tr><td><see cref="gcfSensitivityFunction"/></td><td>Spectral calibration of the spectrometer</td></tr>
        /// <tr><td><see cref="gcfTransmissionFunction"/></td><td>Transmission correction of additional optical accessories</td></tr>
        /// <tr><td><see cref="gcfTOPApertureFactor"/></td><td>Absolute calibration of the TOP apertures</td></tr>
        /// <tr><td><see cref="gcfTOPDistanceFactor"/></td><td>TOP distance factors</td></tr>
        /// <tr><td><see cref="gcfWLCalibrationChannel"/></td><td>Clears the wavelength calibration</td></tr>
        /// <tr><td><see cref="gcfLinearityFunction"/></td><td>Linearity calibration of the ADC</td></tr>
        /// </table>
        /// There is no need to call <see cref="paUpdateSpectralCalibration"/> after casClearCalibration, as this is done implicitly.
        /// </remarks>
        [DllImport(CASLibName)]
        public static extern void casClearCalibration(int ADevice, int AWhat);
        ///@}

        ///@name Spectrum access
        ///
        
        ///@{
        /// <summary>
        /// Returns the intensity of the previously acquired spectrum for the given pixel.
        /// </summary>
        /// <param name="ADevice">The device / CASID</param>
        /// <param name="AIndex">The 0-based index of the pixel in the complete pixel array for which the intensity should be returned</param>
        /// <returns>The return value is the intensity for the given pixel in the calibration unit <see cref="dpidCalibrationUnit"/>.
        /// Use <see cref="casGetError"/> to check for <see cref="ErrorInvalidParameter"/> and other errors.</returns>
        /// <remarks>
        /// <para>AIndex can range from 0 to <see cref="dpidPixels"/> - 1, but the valid calibrated spectrum ranges from <see cref="dpidDeadPixels"/> to 
        ///    <see cref="dpidDeadPixels"/> + <see cref="dpidVisiblePixels"/> - 1.</para>
        /// <para>Use <see cref="casGetXArray"/> to retrieve the corresponding wavelength. A previously measured dark current can be retrieved using 
        /// <see cref="casGetDarkCurrent"/>.</para>
        /// [Visual Basic]
        /// ~~~~~~~~~~~~~{.vb}
        /// DeadPixels = casGetDeviceParameter(CasID, dpidDeadPixels)
        /// VisiblePixels = casGetDeviceParameter(CasID, dpidVisiblePixels)
        ///   
        /// 'get Lambda and Intensity, skip dead pixels!
        /// For i = 0 To VisiblePixels - 1
        ///   Spectrum(0, i) = casGetXArray(CasID, DeadPixels + i)
        ///   Spectrum(1, i) = casGetData(CasID, DeadPixels + i)
        /// Next i
        /// ~~~~~~~~~~~~~
        /// 
        ///    </remarks>
        [DllImport(CASLibName)]
        public static extern double casGetData(int ADevice, int AIndex);

        /// <summary>
        /// Returns the wavelength of the spectrum for the given pixel.
        /// </summary>
        /// <param name="ADevice">The device / CASID</param>
        /// <param name="AIndex">The 0-based index of the pixel in the complete pixel array for which the wavelength should be returned</param>
        /// <returns>The return value is the wavelength for the given pixel in nm.
        /// Use <see cref="casGetError"/> to check for <see cref="ErrorInvalidParameter"/> and other errors.</returns>
        /// <remarks>
        /// <para>AIndex can range from 0 to <see cref="dpidPixels"/> - 1, but the valid calibrated spectrum ranges from <see cref="dpidDeadPixels"/> to 
        ///    <see cref="dpidDeadPixels"/> + <see cref="dpidVisiblePixels"/> - 1.</para>
        /// <para>Use <see cref="casGetData"/> to retrieve the corresponding intensity at this wavelength.</para>
        /// <para>@note Contrary to casGetData, casGetXArray does not require that a spectrum has been measured. A successful initialization with <see cref="casInitialize"/> is sufficient.</para>
        /// [Visual Basic]
        /// ~~~~~~~~~~~~~{.vb}
        /// DeadPixels = casGetDeviceParameter(CasID, dpidDeadPixels)
        /// VisiblePixels = casGetDeviceParameter(CasID, dpidVisiblePixels)
        ///   
        /// 'get Lambda and Intensity, skip dead pixels!
        /// For i = 0 To VisiblePixels - 1
        ///   Spectrum(0, i) = casGetXArray(CasID, DeadPixels + i)
        ///   Spectrum(1, i) = casGetData(CasID, DeadPixels + i)
        /// Next i
        /// ~~~~~~~~~~~~~
        /// 
        /// </remarks>
        [DllImport(CASLibName)]
        public static extern double casGetXArray(int ADevice, int AIndex);

        /// <summary>
        /// Returns the intensity of the previously measured/calculated dark current for the given pixel.
        /// </summary>
        /// <param name="ADevice">The device / CASID</param>
        /// <param name="AIndex">The 0-based index of the pixel in the complete pixel array for which the dark current should be returned</param>
        /// <returns>The return value is the dark current for the given pixel in negative calibration units.
        /// Use <see cref="casGetError"/> to check for <see cref="ErrorInvalidParameter"/> and other errors.</returns>
        /// <remarks>
        /// <para>AIndex can range from 0 to <see cref="dpidPixels"/> - 1, but the valid dark current ranges from <see cref="dpidDeadPixels"/> to 
        ///    <see cref="dpidDeadPixels"/> + <see cref="dpidVisiblePixels"/> - 1.</para>
        /// <para>Use <see cref="casGetXArray"/> to retrieve the corresponding wavelength and <see cref="casGetData"/> to retrieve the intensity of a measured spectrum.</para>
        ///    </remarks>
        [DllImport(CASLibName)]
        public static extern double casGetDarkCurrent(int ADevice, int AIndex);
        ///@}

        ///@name ColorMetric
        ///
        
        ///@{
        /// <summary>
        /// Retrieves the previously calculated photometric integral and it's data unit.
        /// </summary>
        /// <param name="ADevice">The device / CASID</param>
        /// <param name="APhotInt">Double that will contain the photometric integral after a successful method call.</param>
        /// <param name="AUnit">Destination buffer for string representing the photometric data unit</param>
        /// <param name="AUnitMaxLen">The maximum number of characters AUnit can hold</param>
        /// <remarks>
        /// Before calling casGetPhotInt, a call to <see cref="casColorMetric"/> is necessary, to calculate all results from the previously measured 
        /// spectrum.
        /// <para>The integral itself and the data unit will always be returned using the basic unit, e.g. APhotInt = 1.23E-5 and AUnit = "lx" and not in "mlx" or "klx".</para>
        /// <para>Like all results calculated by casColormetric, the photometric integral is limited to the wavelength range defined by <see cref="mpidColormetricStart"/> and <see cref="mpidColormetricStop"/>. 
        /// The default values of these 2 mpids (380nm and 780nm respectively) might cause unexpected results, so it is recommend to set both to zero to use the full range of the spectrum.</para>
        /// <para>For integration, the spectrum is multiplied with V(Lambda) and the photometric equivalent Km. Because of that, the actual wavelength range of the integration is always constrained 
        /// by the range of V(Lambda), which varies depending on <see cref="mpidColormetricType"/> (380nm to 780nm for <see cref="cmtDefaultColormetric"/> and 360nm to 830nm for <see cref="cmtSWPColormetric"/>).</para>
        /// <para>The integral is independent of <see cref="mpidObserver"/>.</para>
        /// <para>Use <see cref="casGetError"/> to check for errors.</para>
        /// <para><see cref="casGetRadInt"/> retrieves the radiometric integral.</para>
        ///    </remarks>
        [DllImport(CASLibName, CharSet = CharSet.Auto)]
        public static extern void casGetPhotInt(int ADevice, out double APhotInt, StringBuilder AUnit, int AUnitMaxLen);

        /// <summary>
        /// Retrieves the previously calculated radiometric integral and it's data unit.
        /// </summary>
        /// <param name="ADevice">The device / CASID</param>
        /// <param name="ARadInt">Double that will contain the radiometric integral after a successful method call.</param>
        /// <param name="AUnit">Destination buffer for string representing the radiometric data unit</param>
        /// <param name="AUnitMaxLen">The maximum number of characters AUnit can hold</param>
        /// <remarks>
        /// Before calling casGetRadInt, a call to <see cref="casColorMetric"/> is necessary, to calculate all results from the previously measured
        /// spectrum.
        /// <para>The integral itself and the data unit will always be returned using the basic unit, e.g. ARadInt = 1.23E-5 and AUnit = "W" and not in "mW" or "kW".</para>
        /// <para>Like all results calculated by casColormetric, the radiometric integral is limited to the wavelength range defined by <see cref="mpidColormetricStart"/> and <see cref="mpidColormetricStop"/>. 
        /// The default values of these 2 mpids (380nm and 780nm respectively) might cause unexpected results, so it is recommend to set both to zero to use the full range of the spectrum.</para>
        /// <para>Use <see cref="casGetError"/> to check for errors.</para>
        /// <para><see cref="casGetPhotInt"/> retrieves the photometric integral.</para>
        ///    </remarks>
        [DllImport(CASLibName)]
        public static extern void casGetRadInt(int ADevice, out double ARadInt, StringBuilder AUnit, int AUnitMaxLen);

        /// <summary>
        /// Retrieves the previously calculated centroid wavelength.
        /// </summary>
        /// <param name="ADevice">The device / CASID</param>
        /// <returns>The return value is the centroid wavelength in nm.
        /// Use <see cref="casGetError"/> to check for errors.</returns>
        /// <remarks>
        /// Before calling casGetCentroid, a call to <see cref="casColorMetric"/> is necessary, to calculate all results from the current 
        /// spectrum.
        ///    </remarks>
        [DllImport(CASLibName)]
        public static extern double casGetCentroid(int ADevice);

        /// <summary>
        /// Retrieves the previously calculated peak wavelength and intensity.
        /// </summary>
        /// <param name="ADevice">The device / CASID</param>
        /// <param name="x">Receives the wavelength in nm of the interpolated peak</param>
        /// <param name="y">Receives the intensity in calibration units of the interpolated peak</param>
        /// <remarks>
        /// Before calling casGetPeak, a call to <see cref="casColorMetric"/> is necessary, to calculate all results from the current 
        /// spectrum.
        /// <para>Use <see cref="casGetError"/> to check for errors.</para>
        ///    </remarks>
        [DllImport(CASLibName)]
        public static extern void casGetPeak(int ADevice, out double x, out double y);

        /// <summary>
        /// Retrieves the peak width in nm.
        /// </summary>
        /// <param name="ADevice">The device / CASID</param>
        /// <returns>The return value is the peak width in nm.
        /// Use <see cref="casGetError"/> to check for errors.</returns>
        /// <remarks>
        /// casGetWidth calculates the width of the peak and returns just the width in nm. It is recommended to use <see cref="casGetWidthEx"/> instead, as it also returns other aspects of the peak width. The level of the peak width is 50% by default so the FWHM is calculated. It can be adjusted with <see cref="mpidColormetricWidthLevel"/> *before* calling casGetWidth or casGetWidthEx.
        /// <para>As with all colormetric calculations, the range of the spectrum which is used for determining the width is defined by <see cref="mpidColormetricStart"/> and <see cref="mpidColormetricStop"/>.</para>
        /// </remarks>
        [DllImport(CASLibName)]
        public static extern double casGetWidth(int ADevice);

        public const int cLambdaWidth       = 0; ///< AWhat constant for <see cref="casGetWidthEx"/>: returns the peak width in nm, identical to <see cref="casGetWidth"/>
        public const int cLambdaLow         = 1; ///< AWhat constant for <see cref="casGetWidthEx"/>: start wavelength of the width
        public const int cLambdaMiddle      = 2; ///< AWhat constant for <see cref="casGetWidthEx"/>: center wavelength of the width, i.e. Center - Start = Stop - Center
        public const int cLambdaHigh        = 3; ///< AWhat constant for <see cref="casGetWidthEx"/>: stop wavelength of the width
        public const int cLambdaOuterWidth  = 4; ///< AWhat constant for <see cref="casGetWidthEx"/>: identical to cLambdaWidth, but using old algorithm
        public const int cLambdaOuterLow    = 5; ///< AWhat constant for <see cref="casGetWidthEx"/>: identical to cLambdaLow, but using old algorithm
        public const int cLambdaOuterMiddle = 6; ///< AWhat constant for <see cref="casGetWidthEx"/>: identical to cLambdaMiddle, but using old algorithm
        public const int cLambdaOuterHigh   = 7; ///< AWhat constant for <see cref="casGetWidthEx"/>: identical to cLambdaHigh, but using old algorithm
        
        /// <summary>
        /// Calculates and retrieves various aspects about the peak width (e.g. full width half maximum aka FWHM aka 50% bandwidth).
        /// </summary>
        /// <param name="ADevice">The device / CASID</param>
        /// <param name="AWhat">One of the cLambda constants specifying what to retrieve; see table below</param>
        /// <returns>The return value is the detail about the width that was specified by AWhat.
        /// Use <see cref="casGetError"/> to check for errors.</returns>
        /// <remarks>
        /// <para>By default the FWHM is calculated, i.e. the peak level used for the calculation is 50%. You can modify this level independently for each parameter set using <see cref="mpidColormetricWidthLevel"/> before calling casGetWidthEx.</para>
        /// The following table lists possible values for the AWhat parameter and a description which aspect of the peak width is returned. The meaning of the algorithm and calculates columns are explained below.
        /// <table>
        /// <tr><td>AWhat</td><td>Calculates?*</td><td>Algorithm**</td><td>Description</td></tr>
        /// <tr><td><see cref="cLambdaWidth"/></td><td>Yes</td><td>Inner width</td><td>The peak width in nm</td></tr>
        /// <tr><td><see cref="cLambdaLow"/></td><td>No</td><td>Inner width</td><td>The start wavelength of the peak width</td></tr>
        /// <tr><td><see cref="cLambdaMiddle"/></td><td>No</td><td>Inner width</td><td>The center wavelength of the peak width, in the middle between start and stop wavelength</td></tr>
        /// <tr><td><see cref="cLambdaHigh"/></td><td>No</td><td>Inner width</td><td>The stop wavelength of the peak width</td></tr>
        /// <tr><td><see cref="cLambdaOuterWidth"/></td><td>Yes</td><td>Outer width</td><td>Returns the peak width</td></tr>
        /// <tr><td><see cref="cLambdaOuterLow"/></td><td>Yes</td><td>Outer width</td><td>The start wavelength of the peak width</td></tr>
        /// <tr><td><see cref="cLambdaOuterMiddle"/></td><td>Yes</td><td>Outer width</td><td>The center wavelength of the peak width, in the middle between start and stop wavelength</td></tr>
        /// <tr><td><see cref="cLambdaOuterHigh"/></td><td>Yes</td><td>Outer width</td><td>The stop wavelength of the peak width</td></tr>
        /// </table>
        /// <para>As with all colormetric calculations, the range of the spectrum which is used for determining the peak width is defined by <see cref="mpidColormetricStart"/> and <see cref="mpidColormetricStop"/>.</para>
        /// <para>* Some AWhat constants only retrieve a previously calculated value and some perform the actual calculation. For example, before you can retrieve cLambdaLow, you must retrieve cLambdaWidth, as only the latter performs the actual calculation using the Inner width algorithm</para>
        /// <para>** Normally the peak width is determined by "walking" the spectrum up and down from the intensity maximum to check when the intensity falls below the level defined by <see cref="mpidColormetricWidthLevel"/>. That is the so called "Inner width" algorithm which should normally be used. The "Outer width" algorithm is mainly supported because very old CAS DLL implementations (version 3 and before) used this. This algorithm actually starts walking from the ends of the colormetric range towards the center and stops when the width level of the intensity maximum is reached.</para>
        /// </remarks>
        [DllImport(CASLibName)]
        public static extern double casGetWidthEx(int ADevice, int AWhat);

        /// <summary>
        /// Retrieves CIE color coordinates of a previously measured spectrum.
        /// </summary>
        /// <param name="ADevice">The device / CASID</param>
        /// <param name="x">Receives the x color coordinate according to CIE 1931</param>
        /// <param name="y">Receives the y color coordinate according to CIE 1931</param>
        /// <param name="z">Receives the z color coordinate according to CIE 1931</param>
        /// <param name="u">Receives the u / u' color coordinate according to CIE 1960 / CIE 1976</param>
        /// <param name="v1976">Receives the v' color coordinate according to CIE 1976</param>
        /// <param name="v1960">Receives the v color coordinate according to CIE 1960</param>
        /// <remarks>
        /// Before calling casGetColorCoordinates, a call to <see cref="casColorMetric"/> is necessary, to calculate all results from the current spectrum.
        /// <para>As with all colormetric calculations, the range of the spectrum which is used for calculating the color coordinates is defined by <see cref="mpidColormetricStart"/> and <see cref="mpidColormetricStop"/>. The calculation is also affected by <see cref="mpidObserver"/>.</para>
        /// <para>Use <see cref="casGetError"/> to check for errors.</para>
        ///    </remarks>
        [DllImport(CASLibName)]
        public static extern void casGetColorCoordinates(int ADevice, ref double x, ref double y, ref double z, ref double u, ref double v1976, ref double v1960);

        /// <summary>
        /// Calculates the correlated color temperature CCT of a previously measured spectrum.
        /// </summary>
        /// <param name="ADevice">The device / CASID</param>
        /// <returns>The return value is the CCT in K.
        /// Use <see cref="casGetError"/> to check for errors.</returns>
        /// <remarks>
        /// Before calling casGetCCT, a call to <see cref="casColorMetric"/> is necessary, to calculate all results from the current spectrum.
        /// <para>As with all colormetric calculations, the range of the spectrum which is used for calculating the CCT is defined by <see cref="mpidColormetricStart"/> and <see cref="mpidColormetricStop"/>. The calculation is also affected by <see cref="mpidObserver"/>.</para>
        /// <para>Use <see cref="casGetError"/> to check for errors.</para>
        ///    </remarks>
        [DllImport(CASLibName)]
        public static extern double casGetCCT(int ADevice);

        /// <summary>
        /// Retrieves one of the previously calculated the color rendering indices.
        /// </summary>
        /// <param name="ADevice">The device / CASID</param>
        /// <param name="Index">Index ranging from 0 to 16, identifying which index should be returned. 0 returns the common index, sometimes also referred to as Ra, which is the average of the indices 1..8.</param>
        /// <returns>The return value is the CRI.
        /// Use <see cref="casGetError"/> to check for errors.</returns>
        /// <remarks>
        /// The following preconditions must have been met, before calling casGetCRI:
        /// <list type="bullet"> 
        /// <item>the device must have a valid spectrum </item> 
        /// <item>set CRI calculation mode globally using <see cref="mpidCRIMode"/></item>
        /// <item>colormetric calculation using <see cref="casColorMetric"/>)</item>
        /// <item>calculation of the CCT using (<see cref="casGetCCT"/>)</item>
        /// <item>calculation of the CRI using (<see cref="casCalculateCRI"/>)</item>
        /// </list>
        ///    </remarks>
        [DllImport(CASLibName)]
        public static extern double casGetCRI(int ADevice, int Index);

        /// <summary>
        /// Retrieves the previously calculated X, Y and Z tristimulus values.
        /// </summary>
        /// <param name="ADevice">The device / CASID</param>
        /// <param name="X">Variable to receive the tristimulus X</param>
        /// <param name="Y">Variable to receive the tristimulus Y</param>
        /// <param name="Z">Variable to receive the tristimulus Z</param>
        /// <remarks>
        /// Before calling casGetTriStimulus, <see cref="casColorMetric"/> must have been called for an already measured spectrum. 
        /// Calculation is influenced by <see cref="mpidObserver"/>. The tristimulus is calculated in the wavelength range of the color matching function, 
        /// but the range can be further restricted by <see cref="mpidColormetricStart"/> and <see cref="mpidColormetricStop"/>.
        ///    </remarks>
        [DllImport(CASLibName)]
        public static extern void casGetTriStimulus(int ADevice, ref double X, ref double Y, ref double Z);

        public const int ecvVisualEffect      = 2; ///< AWhat constant for <see cref="casGetExtendedColorValues"/>: Visual Effect in %. Tristimulus Y divided by radiometric VIS integral (380..780nm)
        public const int ecvUVA               = 3; ///< AWhat constant for <see cref="casGetExtendedColorValues"/>: UVA radiometric integral 315..400 nm
        public const int ecvUVB               = 4; ///< AWhat constant for <see cref="casGetExtendedColorValues"/>: UVB radiometric integral 280..315 nm
        public const int ecvUVC               = 5; ///< AWhat constant for <see cref="casGetExtendedColorValues"/>: UVC radiometric integral 200..280 nm
        public const int ecvVIS               = 6; ///< AWhat constant for <see cref="casGetExtendedColorValues"/>: VIS radiometric integral 380..780 nm
        public const int ecvCRICCT            = 7; ///< AWhat constant for <see cref="casGetExtendedColorValues"/>: correlated color temperature in K used for CRI calculation. Not identical to <see cref="casGetCCT"/>!
        public const int ecvCDI               = 8; ///< AWhat constant for <see cref="casGetExtendedColorValues"/>: color discrimination index
        public const int ecvDistance          = 9; ///< AWhat constant for <see cref="casGetExtendedColorValues"/>: Planck distance DUV between the color locus (u, v) and the Planck curve
        public const int ecvCalibMin          = 10; ///< AWhat constant for <see cref="casGetExtendedColorValues"/>: minimum wavelength of the currently used calibration
        public const int ecvCalibMax          = 11; ///< AWhat constant for <see cref="casGetExtendedColorValues"/>: maximum wavelength of the currently used calibration
        public const int ecvScotopicInt       = 12; ///< AWhat constant for <see cref="casGetExtendedColorValues"/>: scotopic integral, unit identical to photometric integral retrieved by <see cref="casGetPhotInt"/>
        public const int ecvAOx             = 13; ///< AWhat constant for <see cref="casGetExtendedColorValues"/>: color coordinate x of the additional observer results. Only calculated when <see cref="mpidAdditionalObserver"/> is > 0. See @ref colorMetricAdditionalObservers.
        public const int ecvAOy             = 14; ///< AWhat constant for <see cref="casGetExtendedColorValues"/>: color coordinate y of the additional observer results. Only calculated when <see cref="mpidAdditionalObserver"/> is > 0. See @ref colorMetricAdditionalObservers.
        public const int ecvAOz             = 15; ///< AWhat constant for <see cref="casGetExtendedColorValues"/>: color coordinate z of the additional observer results. Only calculated when <see cref="mpidAdditionalObserver"/> is > 0. See @ref colorMetricAdditionalObservers.
        public const int ecvAOu             = 16; ///< AWhat constant for <see cref="casGetExtendedColorValues"/>: color coordinate u (CIE 1960 and CIE 1976) of the additional observer results. Only calculated when <see cref="mpidAdditionalObserver"/> is > 0. See @ref colorMetricAdditionalObservers.
        public const int ecvAOv1960         = 17; ///< AWhat constant for <see cref="casGetExtendedColorValues"/>: color coordinate v (CIE 1960) of the additional observer results. Only calculated when <see cref="mpidAdditionalObserver"/> is > 0. See @ref colorMetricAdditionalObservers.
        public const int ecvAOv1976         = 18; ///< AWhat constant for <see cref="casGetExtendedColorValues"/>: color coordinate v' (CIE 1976) of the additional observer results. Only calculated when <see cref="mpidAdditionalObserver"/> is > 0. See @ref colorMetricAdditionalObservers.
        public const int ecvAOTriX          = 19; ///< AWhat constant for <see cref="casGetExtendedColorValues"/>: Tristimulus X of the additional observer results. Only calculated when <see cref="mpidAdditionalObserver"/> is > 0. See @ref colorMetricAdditionalObservers.
        public const int ecvAOTriY          = 20; ///< AWhat constant for <see cref="casGetExtendedColorValues"/>: Tristimulus Y of the additional observer results. Only calculated when <see cref="mpidAdditionalObserver"/> is > 0. See @ref colorMetricAdditionalObservers.
        public const int ecvAOTriZ          = 21; ///< AWhat constant for <see cref="casGetExtendedColorValues"/>: Tristimulus Z of the additional observer results. Only calculated when <see cref="mpidAdditionalObserver"/> is > 0. See @ref colorMetricAdditionalObservers.
        public const int ecvAOLambdaDom     = 22; ///< AWhat constant for <see cref="casGetExtendedColorValues"/>: dominant wavelength of the additional observer results. Only calculated when <see cref="mpidAdditionalObserver"/> is > 0. See @ref colorMetricAdditionalObservers.
        public const int ecvAOPurity        = 23; ///< AWhat constant for <see cref="casGetExtendedColorValues"/>: purity of the additional observer results. Only calculated when <see cref="mpidAdditionalObserver"/> is > 0. See @ref colorMetricAdditionalObservers.
        public const int ecvAOCCT           = 24; ///< AWhat constant for <see cref="casGetExtendedColorValues"/>: correlated color temperature of the additional observer results. Only calculated when <see cref="mpidAdditionalObserver"/> is > 0. See @ref colorMetricAdditionalObservers.
        public const int ecvAODistance      = 25; ///< AWhat constant for <see cref="casGetExtendedColorValues"/>: planck distance of the additional observer results. Only calculated when <see cref="mpidAdditionalObserver"/> is > 0. See @ref colorMetricAdditionalObservers.

        public const int ecvCRIFirst          = 100; ///< AWhat constant for <see cref="casGetExtendedColorValues"/>: retrieves first (common) CRI, increase up to <see cref="ecvCRIList"/> to retrieve others.
        public const int ecvCRILast           = 116; ///< AWhat constant for <see cref="casGetExtendedColorValues"/>: retrieves last CRI, see <see cref="ecvCRIFirst"/>.
        public const int ecvCRITriKXFirst     = 120; ///< AWhat constant for <see cref="casGetExtendedColorValues"/>: intermediate result of CRI calculation. See overview table at casGetExtendedColorValues.
        public const int ecvCRITriKXLast      = 136; ///< AWhat constant for <see cref="casGetExtendedColorValues"/>: intermediate result of CRI calculation. See overview table at casGetExtendedColorValues.
        public const int ecvCRITriKYFirst     = 140; ///< AWhat constant for <see cref="casGetExtendedColorValues"/>: intermediate result of CRI calculation. See overview table at casGetExtendedColorValues.
        public const int ecvCRITriKYLast      = 156; ///< AWhat constant for <see cref="casGetExtendedColorValues"/>: intermediate result of CRI calculation. See overview table at casGetExtendedColorValues.
        public const int ecvCRITriKZFirst     = 160; ///< AWhat constant for <see cref="casGetExtendedColorValues"/>: intermediate result of CRI calculation. See overview table at casGetExtendedColorValues.
        public const int ecvCRITriKZLast      = 176; ///< AWhat constant for <see cref="casGetExtendedColorValues"/>: intermediate result of CRI calculation. See overview table at casGetExtendedColorValues.
        public const int ecvCRITriRXordUFirst = 180; ///< AWhat constant for <see cref="casGetExtendedColorValues"/>: intermediate result of CRI calculation. See overview table at casGetExtendedColorValues.
        public const int ecvCRITriRXordULast  = 196; ///< AWhat constant for <see cref="casGetExtendedColorValues"/>: intermediate result of CRI calculation. See overview table at casGetExtendedColorValues.
        public const int ecvCRITriRYordVFirst = 200; ///< AWhat constant for <see cref="casGetExtendedColorValues"/>: intermediate result of CRI calculation. See overview table at casGetExtendedColorValues.
        public const int ecvCRITriRYordVLast  = 216; ///< AWhat constant for <see cref="casGetExtendedColorValues"/>: intermediate result of CRI calculation. See overview table at casGetExtendedColorValues.
        public const int ecvCRITriRZordWFirst = 220; ///< AWhat constant for <see cref="casGetExtendedColorValues"/>: intermediate result of CRI calculation. See overview table at casGetExtendedColorValues.
        public const int ecvCRITriRZordWLast  = 236; ///< AWhat constant for <see cref="casGetExtendedColorValues"/>: intermediate result of CRI calculation. See overview table at casGetExtendedColorValues.

        /// <summary>
        /// Retrieves results and conditions for previously performed colormetric calculations.
        /// </summary>
        /// <param name="ADevice">The device / CASID</param>
        /// <param name="AWhat">Determines which colormetric value to return. One of the ecv constants starting with <see cref="ecvVisualEffect"/></param>
        /// <returns>The return value depends on the ecv constant passed as What parameter.
        /// Use <see cref="casGetError"/> to check for errors.</returns>
        /// <remarks>
        /// Before calling casGetExtendedColorValues the values have to be calculated by calling <see cref="casColorMetric"/>, <see cref="casGetCCT"/>
        /// and <see cref="casCalculateCRI"/>. Refer to these methods to find out which other properties influence their results, like colormetric range or <see cref="mpidCRIMode"/>.
        /// 
        /// The ecv constants starting with <see cref="ecvCRIFirst"/> allow access to intermediate- and end-results of the CRI calculation. 
        /// The following table lists their meaning when <see cref="mpidCRIMode"/> is <see cref="criDIN6169"/> and <see cref="mpidColormetricType"/> is <see cref="cmtDefaultColormetric"/>. For <see cref="cmtSWPColormetric"/> these results are not available and will return 0.
        /// <table>
        /// <tr><td>AWhat range</td><td>Description</td></tr>
        /// <tr><td><see cref="ecvCRIFirst"/>..<see cref="ecvCRILast"/></td><td>Returns the CRI value, identical to <see cref="casGetCRI"/>, i.e. AWhat = ecvCRIFirst returns the common CRI, ecvCRIFirst + 1 .. ecvCRILast return CRIs 1 to 16.</td></tr>
        /// <tr><td><see cref="ecvCRITriKXFirst"/>..<see cref="ecvCRITriKXLast"/></td><td>Returns the Tristimulus X of the spectrum without a test color (ecvCRITriKXFirst) or with test color 1..16 (ecvCRITriKXFirst + 1 .. ecvCRITriKXLast)</td></tr>
        /// <tr><td><see cref="ecvCRITriKYFirst"/>..<see cref="ecvCRITriKYLast"/></td><td>Returns the Tristimulus Y of the spectrum without a test color (ecvCRITriKYFirst) or with test color 1..16 (ecvCRITriKYFirst + 1 .. ecvCRITriKYLast)</td></tr>
        /// <tr><td><see cref="ecvCRITriKZFirst"/>..<see cref="ecvCRITriKZLast"/></td><td>Returns the Tristimulus Z of the spectrum without a test color (ecvCRITriKZFirst) or with test color 1..16 (ecvCRITriKZFirst + 1 .. ecvCRITriKZLast)</td></tr>
        /// <tr><td><see cref="ecvCRITriRXordUFirst"/>..<see cref="ecvCRITriRXordULast"/></td><td>Returns the Tristimulus X of the calculated reference spectrum without a test color (ecvCRITriRXordUFirst) or with test color 1..16 (ecvCRITriRXordUFirst + 1 .. ecvCRITriRXordUFirstLast)</td></tr>
        /// <tr><td><see cref="ecvCRITriRYordVFirst"/>..<see cref="ecvCRITriRYordVLast"/></td><td>Returns the Tristimulus Y of the calculated reference spectrum without a test color (ecvCRITriRYordVFirst) or with test color 1..16 (ecvCRITriRYordVFirst + 1 .. ecvCRITriRYordVLast)</td></tr>
        /// <tr><td><see cref="ecvCRITriRZordWFirst"/>..<see cref="ecvCRITriRZordWLast"/></td><td>Returns the Tristimulus Z of the calculated reference spectrum without a test color (ecvCRITriRZordWFirst) or with test color 1..16 (ecvCRITriRZordWFirst + 1 .. ecvCRITriRZordWLast)</td></tr>
        /// </table>
        /// The following table lists their meaning when <see cref="mpidCRIMode"/> is <see cref="criCIE13_3_95"/>, regardless of <see cref="mpidColormetricType"/>.
        /// <table>
        /// <tr><td>AWhat range</td><td>Description</td></tr>
        /// <tr><td><see cref="ecvCRIFirst"/>..<see cref="ecvCRILast"/></td><td>Returns the CRI value, identical to <see cref="casGetCRI"/>, i.e. AWhat = ecvCRIFirst returns the common CRI, ecvCRIFirst + 1 .. ecvCRILast return CRIs 1 to 16.</td></tr>
        /// <tr><td><see cref="ecvCRITriKXFirst"/>..<see cref="ecvCRITriKXLast"/></td><td>Returns the Tristimulus X of the spectrum without a test color (ecvCRITriKXFirst) or with test color 1..16 (ecvCRITriKXFirst + 1 .. ecvCRITriKXLast)</td></tr>
        /// <tr><td><see cref="ecvCRITriKYFirst"/>..<see cref="ecvCRITriKYLast"/></td><td>Returns the Tristimulus Y of the spectrum without a test color (ecvCRITriKYFirst) or with test color 1..16 (ecvCRITriKYFirst + 1 .. ecvCRITriKYLast)</td></tr>
        /// <tr><td><see cref="ecvCRITriKZFirst"/>..<see cref="ecvCRITriKZLast"/></td><td>Returns the Tristimulus Z of the spectrum without a test color (ecvCRITriKZFirst) or with test color 1..16 (ecvCRITriKZFirst + 1 .. ecvCRITriKZLast)</td></tr>
        /// <tr><td><see cref="ecvCRITriRXordUFirst"/> + 1..<see cref="ecvCRITriRXordULast"/></td><td>Returns dU for CRI 1..16 (ecvCRITriRXordUFirst + 1 .. ecvCRITriRXordUFirstLast)</td></tr>
        /// <tr><td><see cref="ecvCRITriRYordVFirst"/> + 1..<see cref="ecvCRITriRYordVLast"/></td><td>Returns dV for CRI 1..16 (ecvCRITriRYordVFirst + 1 .. ecvCRITriRYordVLast)</td></tr>
        /// <tr><td><see cref="ecvCRITriRZordWFirst"/> + 1..<see cref="ecvCRITriRZordWLast"/></td><td>Returns dW for CRI 1..16 (ecvCRITriRZordWFirst + 1 .. ecvCRITriRZordWLast)</td></tr>
        /// </table>
        ///    </remarks>
        [DllImport(CASLibName)]
        public static extern double casGetExtendedColorValues(int ADevice, int AWhat);

        /// <summary>
        /// Calculates colormetric (and more) results for the previously measured spectrum. This includes results not related to color like the radiometric integral <see cref="casGetRadInt"/>.
        /// </summary>
        /// <param name="ADevice">The device / CASID</param>
        /// <returns>The return value is 0, if the method was successful, or a negative error code (see chapter @ref errorHandling "Error Handling").</returns>
        /// <remarks>
        /// The actual calculation is highly dependent on:
        ///  * the new <see cref="mpidColormetricType"/>
        ///  * the spectral range defined by <see cref="mpidColormetricStart"/> and <see cref="mpidColormetricStop"/>
        ///  * <see cref="mpidObserver"/>
        ///
        /// Refer to the chapter @ref gettingTheSpectrumAndResults "Getting the Spectrum and Results" for an overview of 
        /// the methods (and the order they have to be called) to retrieve the results of this calculation.
        ///    </remarks>
        [DllImport(CASLibName)]
        public static extern int casColorMetric(int ADevice);

        /// <summary>
        /// Calculates the color rendering indices (CRI) of a previously measured spectrum.
        /// </summary>
        /// <param name="ADevice">The device / CASID</param>
        /// <returns>The return value is 0, if the CRI calculation was successful or a negative error code. Most likely is <see cref="ErrorCRI"/>, 
        /// which can indicate that there's not enough signal to calculate CRI or that the CCT is outside a supported range. </returns>
        /// <remarks>
        /// Since different standards for calculating CRI are supported, use the global <see cref="mpidCRIMode"/> to select the standard before calling casCalculateCRI.
        /// @note <see cref="casGetCCT"/> must have been called before calling casCalculateCRI, because the CRI calculation uses a special CCT (<see cref="ecvCRICCT"/>).
        ///
        /// To retrieve the actual color rendering indices, use <see cref="casGetCRI"/>.
        ///
        /// [Delphi]
        /// ~~~~~~~~~~~~~{.pas}
        /// ...
        /// casColorMetric(CASID);
        /// CheckError(casGetError(CASID));
        /// CCT:= casGetCCT(CASID); //also calculates CRICCT and stores it internally
        /// CheckError(casGetError(CASID));
        /// CheckError(casCalculateCRI(CASID));
        /// CRIRa:= casGetCRI(CASID, 0); //Index = 0 retrieves common CRI
        /// CheckError(casGetError(CASID));
        /// ...
        /// ~~~~~~~~~~~~~
        /// </remarks>
        [DllImport(CASLibName)]
        public static extern int casCalculateCRI(int ADevice);

        /// <summary>
        /// Calculates dominant wavelength aka LambdaDom and purity (aka color saturation) from a given color coordinate and illuminant reference. This method is deprecated because it does not take the new <see cref="mpidColormetricType"/> into account. Use <see cref="casCalculateLambdaDom"/> instead.
        /// </summary>
        /// <param name="x">The x color coordinate of the spectrum</param>
        /// <param name="y">The y color coordinate of the spectrum</param>
        /// <param name="IllX">The x color coordinate of the illuminant reference</param>
        /// <param name="IllY">The y color coordinate of the illuminant reference</param>
        /// <param name="LambdaDom">The variable which will receive the calculated dominant wavelength</param>
        /// <param name="Purity">The variable which will receive the calculated purity</param>
        /// <returns>The return value is 0 and should be ignored</returns>
        /// <remarks>
        /// IllX and IllY are the color coordinates of the illuminant reference. One of the typical references is Illuminant E where x and y are 0.3333. 
        /// To use the color coordinates of the previously measured spectrum, retrieve them with <see cref="casGetColorCoordinates"/> and pass them as x and y.
        /// Purity is 0 if x = IllX and y = IllY. Purity will never exceed 1.
        ///    </remarks>
        [Obsolete("This method is deprecated and only kept for backward-compatibility. Use casCalculateLambdaDom instead")]
        [DllImport(CASLibName)]
        public static extern int cmXYToDominantWavelength(double x, double y, double IllX, double IllY, ref double LambdaDom, ref double Purity);

        /// <summary>
        /// Calculates dominant wavelength (a.k.a. LambdaDom) and purity (a.k.a. color saturation) for the specified illuminant reference and the color coordinates that have been calculated by <see cref="casColorMetric"/>. This method replaces the now  deprecated <see cref="cmXYToDominantWavelength"/> and does take the new <see cref="mpidColormetricType"/> into account.
        /// </summary>
        /// <param name="ADevice">The device / CASID</param>
        /// <param name="IllX">The x color coordinate of the illuminant reference</param>
        /// <param name="IllY">The y color coordinate of the illuminant reference</param>
        /// <param name="LambdaDom">The variable which will receive the calculated dominant wavelength</param>
        /// <param name="Purity">The variable which will receive the calculated purity</param>
        /// <returns>The return value is 0, if the calculation was successful, or a negative error code (see chapter @ref errorHandling "Error Handling").</returns>
        /// <remarks>
        /// The device must have a valid spectrum and <see cref="casColorMetric"/> must have been called successfully. 
        /// IllX and IllY are the color coordinates of the illuminant reference. One of the typical references is Illuminant E where x and y are 0.3333. 
        /// Purity is 0 if x = IllX and y = IllY. Purity will never exceed 1.
        ///    </remarks>
        [DllImport(CASLibName)]
        public static extern int casCalculateLambdaDom(int ADevice, double IllX, double IllY, ref double LambdaDom, ref double Purity);

        /// <summary>
        /// Retrieves the complete path and file name of CAS library
        /// </summary>
        /// <param name="Dest">A buffer which can hold at least the number of characters specified in AMaxLen</param>
        /// <param name="AMaxLen">Number of characters including a trailing zero that Dest can hold</param>
        /// <returns>The return value is the pointer passed in Dest and can be ignored</returns>
        /// <remarks>
        /// There are A and W overloads for ANSI and Unicode versions of the method. The returned string is null-terminated.
        ///    </remarks>
        [DllImport(CASLibName, CharSet = CharSet.Auto)]
        public static extern IntPtr casGetDLLFileName(StringBuilder Dest, int AMaxLen);

        /// <summary>
        /// Retrieves the version of CAS library
        /// </summary>
        /// <param name="Dest">A buffer which can hold at least the number of characters specified in AMaxLen</param>
        /// <param name="AMaxLen">Number of characters including a trailing zero that Dest can hold</param>
        /// <returns>The return value is the pointer passed in Dest and can be ignored</returns>
        /// <remarks>
        /// There are A and W overloads for ANSI and Unicode versions of the method. The returned string is null-terminated.
        /// @note some platforms might not support version info and will return "N/A".
        ///    </remarks>
        [DllImport(CASLibName, CharSet = CharSet.Auto)]
        public static extern IntPtr casGetDLLVersionNumber(StringBuilder Dest, int AMaxLen);
        ///@}

        ///@name Spectrum file handling
        ///

        ///@{
        /// <summary>
        /// Saves a previously measured spectrum to an .ISD file.
        /// </summary>
        /// <param name="ADevice">The device / CASID</param>
        /// <param name="AFileName">Full null-terminated path of the .ISD file which should be created.</param>
        /// <returns>The return value is 0 if the method was successful or a negative error code</returns>
        /// <remarks>
        /// The ISD file format is an ANSI/UTF-8 text file which can be loaded by SpecWin Pro and other programs. An existing file at AFileName will be overwritten.
        /// There are A and W overloads for ANSI and Unicode versions of the method.
        ///    </remarks>
        [DllImport(CASLibName, CharSet = CharSet.Auto)]
        public static extern int casSaveSpectrum(int ADevice, string AFileName);

        /// <summary>
        /// Loads a spectrum from an .ISD file into the CASID
        /// </summary>
        /// <param name="ADevice">The device / CASID</param>
        /// <param name="AFileName">Full null-terminated path of the .ISD file which should be loaded</param>
        /// <remarks>
        /// Use casLoadTestData to load a spectrum from the ISD-file specified by AFileName. The spectrum is stored internally for the given device almost as if it would 
        /// have just been measured, so a subsequent call to casColorMetric etc. will perform calculations for this spectrum. 
        /// @warning casLoadTestData will only work for a CASID of a real spectrometer, which is connected to the PC and has been initialized. Otherwise it will return <see cref="ErrorFeatureNotSupported"/>!
        /// @note Loaded spectra will not work with something like <see cref="paRecalcSpectrum"/> since this recalculates the spectrum from the previously acquired raw data - which is
        /// not affected by casLoadTestData.
        /// 
        /// @note The loaded spectrum will be resampled to the wavelength calibration of the spectrometer, so it is only safe to use
        /// casLoadTestData with a CASID that has been initialized with the same calibration files that were used to measure the saved spectrum.
        /// 
        /// @note Call <see cref="casGetError"/> after casLoadTestData for error handling.
        ///
        /// There are A and W overloads for ANSI and Unicode versions of the method.
        /// </remarks>
        [DllImport(CASLibName, CharSet = CharSet.Auto)]
        public static extern void casLoadTestData(int ADevice, string AFileName);
        ///@}

        ///@name Misc
        ///

        ///@{
        /// <summary>
        /// Clears a previously measured dark current of the given device
        /// </summary>
        /// <param name="ADevice">The device / CASID</param>
        /// <returns>The return value is 0 if no error occurred, otherwise it is one of the error codes.</returns>
        /// <remarks>
        /// Normally there is no need to call this method since a new dark current measurement (see <see cref="casMeasureDarkCurrent"/>) automatically replaces the previous one.
        /// This method only affects the dark current of the current parameter set (see @ref workingWithParameterSets). 
        /// Refer to chapter @ref darkCurrent for an overview over related methods and dpid's.
        /// </remarks>
        [DllImport(CASLibName)]
        public static extern int casClearDarkCurrent(int ADevice);

        /// <summary>
        /// Deletes a specific parameter set for the given device. 
        /// </summary>
        /// <param name="ADevice">The device / CASID</param>
        /// <param name="AParamSet">The 0-based index of the parameter set which should be deleted</param>
        /// <returns>The return value is 0 if no error occurred, otherwise it is one of the error codes, e.g. <see cref="ErrorSelectParamSet"/></returns>
        /// <remarks>
        /// AParamSet may range from 0 to <see cref="dpidParamSets"/> - 1. If the <see cref="dpidCurrentParamSet"/> is deleted,
        /// the first parameter set becomes current. There must be at least one parameter set, so casDeleteParamSet cannot delete the last remaining one.
        /// Refer to chapter @ref workingWithParameterSets for an overview.
        /// </remarks>
        [DllImport(CASLibName)]
        public static extern int casDeleteParamSet(int ADevice, int AParamSet);

        /// <summary>
        /// Returns the state the specified digital output port was set to by a former call to <see cref="casSetDigitalOut"/>.
        /// </summary>
        /// <param name="ADevice">The device / CASID</param>
        /// <param name="APort">Identifies which DigitalOut port state to retrieve. The possible values differ with the spectrometer 
        /// type and interface - refer to the hardware manual for more information.</param>
        /// <returns>The return value is 0 for low and 1 for high state. If APort is invalid for this device, 
        /// <see cref="ErrorParamOutOfRange"/> will be returned. Other error codes are possible</returns>
        /// <remarks>
        /// APort=1 is pin 8 and APort=2 for pin 7 of the DB9 connector of the either the spectrometer or the interface card.
        /// </remarks>
        [DllImport(CASLibName)]
        public static extern int casGetDigitalOut(int ADevice, int APort);

        /// <summary>
        /// Sets the state the specified digital output port.
        /// </summary>
        /// <param name="ADevice">The device / CASID</param>
        /// <param name="APort">Identifies which DigitalOut port state to change. The possible values differ with the spectrometer 
        /// type and interface - refer to the hardware manual for more information.</param>
        /// <param name="OnOff">The desired state: 0 for low, all other values for high state</param>
        /// <remarks>
        /// See <see cref="casGetDigitalOut"/> for more information.
        /// @warning Proper error handling using <see cref="casGetError"/> is essential! If APort is not supported by the device, <see cref="ErrorParamOutOfRange"/> will be returned!
        /// </remarks>
        [DllImport(CASLibName)]
        public static extern void casSetDigitalOut(int ADevice, int APort, int OnOff);

        /// <summary>
        /// Returns the state of the specified digital input port.
        /// </summary>
        /// <param name="ADevice">The device / CASID</param>
        /// <param name="APort">Identifies which DigitalIn port state to retrieve. The possible values differ with the spectrometer 
        /// type and interface - refer to the hardware manual for more information.</param>
        /// <returns>The return value is 0 for low and 1 for high state. If APort is invalid for this device, 
        /// <see cref="ErrorParamOutOfRange"/> will be returned. Other error codes are possible</returns>
        [DllImport(CASLibName)]
        public static extern int casGetDigitalIn(int ADevice, int APort);

        public const int extNoError       = 0; ///< AWhat constant for <see cref="casSetStatusLED"/>
        public const int extExternalError = 1; ///< AWhat constant for <see cref="casSetStatusLED"/>
        public const int extFilterBlink   = 2; ///< AWhat constant for <see cref="casSetStatusLED"/>
        public const int extShutterBlink  = 4; ///< AWhat constant for <see cref="casSetStatusLED"/>

        /// <summary>
        /// Method to control the status LED of the spectrometer
        /// </summary>
        /// <param name="ADevice">The device / CASID</param>
        /// <param name="AWhat">One of the ext constants starting with <see cref="extNoError"/> describing the state the status LED should change to</param>
        /// <remarks>
        /// In general, the status LED is controlled by the firmware of the spectrometer and an error of filter or shutter operation is signalled via the status-LED to the operator. 
        /// The casSetStatusLED command can be used to override this and to generate a user defined error (e.g. if the sensor temperature rises above -8°C). 
        /// If the LED signals a malfunction of the shutter or filterwheel, a successful operation of the same component brings the LED back to the "normal" state. 
        /// For example when you manually signal a shutter error using casSetStatusLED, a successful shutter operation causes the LED to stop blinking (the LED is green again). 
        /// If you operate the filter wheel successfully this has no influence on the status LED.
        /// In general, using this method is not recommended nor necessary. Refer to the hardware manual of the spectrometer for more details
        /// about the status LED and it's possible states.
        /// 
        /// The following table list the states of the CAS 140CT
        /// <table>
        /// <tr><td>AWhat</td><td>LED State</td><td>Description</td></tr>
        /// <tr><td>N/A</td><td>red</td><td>after power-on, not initialized</td></tr>
        /// <tr><td>N/A</td><td>orange</td><td>spectrometer is busy, e.g. moving filter wheel</td></tr>
        /// <tr><td><see cref="extNoError"/></td><td>green</td><td>initialized, no error</td></tr>
        /// <tr><td><see cref="extFilterBlink"/></td><td>red, blinking</td><td>filter error</td></tr>
        /// <tr><td><see cref="extShutterBlink"/></td><td>green, blinking</td><td>shutter error</td></tr>
        /// <tr><td><see cref="extExternalError"/></td><td>orange, blinking</td><td>user defined error</td></tr>
        /// </table>
        ///    </remarks>
        /// @note Use <see cref="casGetError"/> for error handling!
        [DllImport(CASLibName)]
        public static extern void casSetStatusLED(int ADevice, int AWhat);

        /// <summary>
        /// Converts a wavelength into the corresponding pixel index of the sensor.
        /// </summary>
        /// <param name="ADevice">The device / CASID</param>
        /// <param name="nm">The wavelength which should be converted into a pixel</param>
        /// <returns>The return value is the 0-based pixel index of the sensor or a negative @ref errorHandling "error code".</returns>
        /// <remarks>
        /// The conversion is done using the wavelength calibration of the specified device. 
        /// Therefore the device must have been initialized using <see cref="casInitialize"/> or the wavelength calibration updated
        /// with <see cref="paUpdateCompleteCalibration"/>.
        /// The return value is the first 0-based pixel whose wavelength is greater or equal than the nm parameter. Only visible pixel are taken into account. 
        /// A negative return value indicates an error.
        /// <see cref="casPixelToNm"/> does the conversion the other way around. 
        /// The returned pixel index can be used with methods like <see cref="casGetXArray"/>, <see cref="casGetData"/> or <see cref="casGetDarkCurrent"/>.
        ///    </remarks>
        [DllImport(CASLibName)]
        public static extern int casNmToPixel(int ADevice, double nm);

        /// <summary>
        /// Converts a sensor pixel index into the corresponding wavelength.
        /// </summary>
        /// <param name="ADevice">The device / CASID</param>
        /// <param name="APixel">The 0-based sensor pixel for which to retrieve the corresponding wavelength</param>
        /// <returns>The return value is the wavelength.
        /// Use <see cref="casGetError"/> for error checking.</returns>
        /// <remarks>
        /// The conversion is done using the wavelength calibration of the specified device. 
        /// Therefore the device must have been initialized using <see cref="casInitialize"/> or the wavelength calibration updated
        /// with <see cref="paUpdateCompleteCalibration"/>.
        /// <see cref="casNmToPixel"/> does the conversion the other way around. 
        ///    </remarks>
        [DllImport(CASLibName)]
        public static extern double casPixelToNm(int ADevice, int APixel);

        /// <summary>
        /// Calculates spot size and field of view of the TOP.
        /// </summary>
        /// <param name="ADevice">The device / CASID</param>
        /// <param name="AAperture">The 0-based index of the TOP aperture, e.g. <see cref="mpidTOPAperture"/></param>
        /// <param name="ADistance">The TOP distance in mm, e.g. <see cref="mpidTOPDistance"/></param>
        /// <param name="ASpotSize">Variable which will receive the calculated spot size in mm.</param>
        /// <param name="AFieldOfView">Variable which will receive the calculated field of view in °.</param>
        /// <returns>The return value is 0 if the method was successful (even if <see cref="coTOPHasFieldOfViewConfig"/> is not present!). Otherwise the return value is an error code.</returns>
        /// <remarks>
        /// This method calculates the spot size and field of view of the TOP for the given aperture and distance and returns them in the ASpotSize and AFieldOfView parameter.
        /// 
        /// ~~~~~~~~~~~~~{.pas}
        /// //this demo displays the FieldOfView in ° and SpotSize in mm for the current TOP aperture
        /// ...
        /// var
        ///   lSpotSize, lFieldOfView: Double;
        /// ...
        /// if casGetOptions(CASID) and coTopHasFieldOfViewConfig <> 0 then
        /// begin
        ///   CheckError(casCalculateTOPParameter(CASID, CurrentTOPAperture, CurrentTOPDistance, lSpotSize, lFieldOfView));
        ///   lblFieldOfView.Caption:= FormatFloat('0.## °', lFieldOfView);
        ///   lblSpotSize.Caption:= FormatFloat('0.## mm', lSpotSize);
        /// end else
        /// begin
        ///   lblFieldOfView.Caption:= 'No Field Of View Info';
        ///   lblSpotSize.Caption:= 'N/A';
        /// end;        
        /// ~~~~~~~~~~~~~
        /// </remarks>
        [DllImport(CASLibName)]
        public static extern int casCalculateTOPParameter(int ADevice, int AAperture, double ADistance, ref double ASpotSize, ref double AFieldOfView);
        ///@}

        ///@name Handle API AWhat: globals
        ///Constants for the @ref handleAPI "handle API" for @ref handleTypeGlobal "global actions without a handle"
        ///@{
        public const int cw_global_CreateCASHandle = 1001; ///< AWhat constant for <see cref="hcasGetHandle"/>: creates a new handle for the CAS ID passed in AIndex1. AHandle must be null. Returns @ref ErrorInvalidDeviceType if the CAS ID passed in AIndex1 could not be found. The returned handle supports AWhat constants starting with "cw_cas_", e.g. <see cref="cw_cas_CopySpectrum"/>. When done the handle needs to be released with <see cref="cw_global_DestroyHandle"/>.
        public const int cw_global_DestroyHandle = 1002; ///< AWhat constant for <see cref="hcasGetHandle"/> and <see cref="hcasSetHandle"/>: Destroys the handle passed in AValue. AHandle must be null. If you use hcasGetHandle the returned AValue will be null, which might be useful for clearing your handle variable. 
        public const int cw_global_CreateColorMetric = 1003; ///< AWhat constant for <see cref="hcasGetHandle"/>: creates a new @ref handleTypeColorMetric "ColorMetric handle". AHandle must be null.
        public const int cw_global_CreateNodeOperation = 1004; ///< AWhat constant for <see cref="hcasGetHandle"/>: creates a new @ref handleTypeNodeOperation "NodeOperation handle". AHandle must be null.
        public const int cw_global_LoadNodeFromFile = 1005; ///< AWhat constant for <see cref="hcasGetHandle"/>: returns a new @ref handleTypeNode "Node handle" with the data loaded from the file passed in the AIndexString parameter. AHandle must be null.
        public const int cw_global_CreateDataUnitInfo = 1006; ///< AWhat constant for <see cref="hcasGetHandle"/>: returns a new @ref handleTypeDataUnitInfo "DataUnitInfo handle". AHandle must be null.
        public const int cw_global_CreateDeviceManagement = 1009; ///< AWhat constant for <see cref="hcasGetHandle"/>: creates a new @ref handleTypeDeviceManagement "DeviceManagement handle". AHandle must be null.
        ///@}
        public const int cw_global_CreateCalibManagement = 1010; ///< AWhat constant for <see cref="hcasGetHandle"/>: creates a new @ref handleTypeCalibManagement "CalibManagement handle". AHandle must be null.
        public const int cw_global_CreateImportCustomObserver = 1011; ///< AWhat constant for <see cref="hcasGetHandle"/>: creates a new @ref handleTypeImportCustomObserver "ImportCustomObserver handle". AHandle must be null.
        ///@}

        ///@name Handle API AWhat: Node handles
        ///Constants for the @ref handleAPI "handle API" for @ref handleTypeNode "Node handles"
        ///@{
        public const int cw_node_ChildCount = 2001; ///< AWhat constant for <see cref="hcasGetInteger"/>: returns the number of child nodes of the node identified by AHandle and AIndexString.
        public const int cw_node_Name = 2002; ///< AWhat constant for <see cref="hcasGetString"/> and <see cref="hcasSetString"/>: returns or sets the node name identified by AHandle, AIndexString and AIndex1. For AIndex1 = <see cref="idx_node_SelfName"/> this is the root node's name if AIndexString is empty/null, otherwise the node identified by AIndexString. If AIndex1 = <see cref="idx_node_AllChildNames"/>, the names of all child nodes can be retrieved - the names are separated by line breaks (according to OS settings). Otherwise AIndex1 is the 0-based index of the child node for which to retrieve/modify the name. For an overview, refer to @ref handleTypeNode. Also see @ref InvalidNodeNames.
          public const int idx_node_SelfName = -1001; ///< AIndex1 constant for <see cref="cw_node_Name"/> to retrieve/change the node identified by AIndexString as opposed to a child node of it.
          public const int idx_node_AllChildNames = -1002; ///< AIndex1 constant for <see cref="cw_node_Name"/> to retrieve all child node names, separated by line breaks.
        public const int cw_node_XDim = 2003; ///< AWhat constant for <see cref="hcasGetInteger"/>: returns the number for data rows the node identified by AHandle and AIndexString contains. Use @ref cw_node_ReDim to change the number of rows.
        public const int cw_node_YDim = 2004; ///< AWhat constant for <see cref="hcasGetInteger"/>: returns the number for data columns the node identified by AHandle and AIndexString contains. Use @ref cw_node_ReDim to change the number of columns.
        public const int cw_node_ReDim = 2005; ///< AWhat constant for <see cref="hcasSetInteger"/>: same as <see cref="cw_node_ReDimWithType"/>, but the passed value is ignored and the data type stays the same. This is merely for convenience so the current data type does not need to be queried and passed as with cw_node_ReDimWithType.
        public const int cw_node_DataType = 2006; ///< AWhat constant for <see cref="hcasGetInteger"/> and <see cref="hcasSetInteger"/>: returns one of the constants starting with node_DataType, e.g. <see cref="node_DataType_String"/> describing the type of 2D data that the node identified by AHandle and AIndexString holds. It is more efficient to use <see cref="cw_node_ReDimWithType"/> if you also want to change any of the dimensions.
          public const int node_DataType_None = 0; ///< the node has no 2D data at all. See <see cref="cw_node_DataType"/>.
          public const int node_DataType_String = 1; ///< the node contains 2D string data. See <see cref="cw_node_DataType"/>.
          public const int node_DataType_Integer = 2; ///< the node contains 32bit integer data. See <see cref="cw_node_DataType"/>.
          public const int node_DataType_Float = 3; ///< the node contains double precision float data. See <see cref="cw_node_DataType"/>.
          public const int node_DataType_DateTime = 4; ///< the node contains double precision float data which should be interpreted as a date time where the intgral part is the number of days since December 30, 1899 and the fractional part is the time. See <see cref="cw_node_DataType"/>.
        public const int cw_node_ReDimWithType = 2007; ///< AWhat constant for <see cref="hcasSetInteger"/>: changes the data type and the number of rows and columns the data has for the node identified by AHandle and AIndexString. AIndex1 = XDim = number of rows, AIndex2 = YDim = number of columns. Any existing data is kept and/or converted, if possible. Some data type changes might fail, e.g. string to numeric if strings are not numeric. Changes from 2D data to no 2D data will fail with <see cref="ErrorInvalidParameter"/>.
        public const int cw_node_DataUnit = 2008; ///< AWhat constant for <see cref="hcasGetString"/> and <see cref="hcasSetString"/>: the data unit for the (numerical) 2D data which the node contains. If AIndex1 equals <see cref="idx_node_XUnit"/>, this is the data unit for the x-vector of the 2D data. If AIndex is <see cref="idx_node_DataUnit"/> this is the data unit for the actual 2D data. For a spectrum, the XUnit is typically "nm" and the DataUnit might be "W/(cm&sup2; sr nm)". For simple 1x1 nodes, e.g. results or measurement conditions, XUnit is ignored.
          public const int idx_node_XUnit = 1; ///< AIndex1 value for <see cref="cw_node_DataUnit"/>: the unit of the x-vector
          public const int idx_node_DataUnit = 2; ///< AIndex1 value for <see cref="cw_node_DataUnit"/>: the unit of the 2D data
        public const int cw_node_StringFormat = 2009; ///< AWhat constant for <see cref="hcasGetString"/> and <see cref="hcasSetString"/>: describes how numerical 2D data of a node should be formatted when queried with <see cref="cw_node_Data"/> and <see cref="hcasGetString"/>. Mostly relevant for <see cref="node_DataType_Float"/>. Formats are "%f", "%e" or "%g" with optional precision specifiers, e.g. "%.2f" for a fixed floating point with 2 decimals, or "%.5g" for 5 significant places which might switch to exponential format, if necessary. Note that the data unit (see <see cref="cw_node_DataUnit"/>) can be added additionally with "%s". If "%s%s" is added, the unit is automatically prefixed to avoid exponential format. A value of 202341.12 with data unit "W" and string format "%.4g %s" will be shown as "2.023E004 W", whereas with a string format of "%.4g %s%s" it will be shown as "20.23 kW".
        public const int cw_node_Data = 2010; ///< AWhat constant for all getter and setter hcas methods, except hcasGet/SetHandle: allows to retrieve or modify the 2D data associated with the node identified by AHandle and AIndexString. See @ref AccessingNodeData for more details. When getting or setting all data using <see cref="hcasGetBuffer"/> or <see cref="hcasSetBuffer"/>, the array is column-major, i.e. starts with all values of the first column, then the 2nd and so on.
          public const int idx_node_Vector = -1001; ///< constant for either AIndex1 or AIndex2 when calling <see cref="cw_node_Data"/> with <see cref="hcasGetBuffer"/> or <see cref="hcasSetBuffer"/> to retrieve or set the complete x-vector (pass in AIndex2), or y-vector (pass in AIndex1), <see cref="hcasGetFloat"/> and <see cref="hcasSetFloat"/> to retrieve/modify the x-vector value at AIndex1 or the y-vector value at AIndex2.
          public const int idx_node_Column = -1002; ///< const for AIndex1 when calling <see cref="cw_node_Data"/> with <see cref="hcasGetString"/> or <see cref="hcasSetString"/> and the 0-based column in AIndex2 to retrieve a multi-line string with OS-dependant linebreaks or fill in the split multi-line string into the data. When setting multi-line string data, <see cref="cw_node_XDim"/> will be automatically increased if necessary. Can also be used with <see cref="hcasGetBuffer"/> or <see cref="hcasSetBuffer"/> to retrieve one column of floating point data. ABuffer must point to an array of double of sufficient size. When setting data, the buffer must have exactly the right size.
        public const int cw_node_Resample = 2011; ///< AWhat constant for <see cref="hcasGetFloat"/>: for 2D numerical data, returns an interpolated data value at the X-vector value that is prefilled into AValue. For a spectrum: when calling hcasGetFloat, AValue must contain the wavelength for which to interpolate the intensity. AIndex2 is the 0-based column for which to interpolate data. Note: no extrapolation possible, i.e. the wavelength / X-vector value for which to interpolate has to be within the range of the X-vector, otherwise <see cref="ErrorInvalidParameter"/> is returned.
        public const int cw_node_DeleteChild = 2012; ///< AWhat constant for <see cref="hcasSetInteger"/>: deletes the childnode at 0-based AIndex1 or, if AIndex1 = <see cref="idx_node_DeleteAllChildren"/> then all child nodes of the node identified by AHandle and AIndexString are deleted. Note that read-only nodes must not be deleted (see <see cref="cw_node_ReadOnly"/> e.g. system-made measurement conditions etc.) which will result in <see cref="ErrorFeatureNotSupported"/> being returned. See also: @ref CreateAndDeleteChildNodes
          public const int idx_node_DeleteAllChildren = -1001; ///< AIndex1 value when deleting all child nodes using <see cref="cw_node_DeleteChild"/>
        public const int cw_node_ReadOnly = 2012; ///< AWhat constant for <see cref="hcasGetInteger"/>: returns 1 if the node identified by AHandle and AIndexString is read-only and 0 if it can be modified/deleted etc. Most conditions and results created by the CAS SDK cannot be modified or deleted separately, to ensure that e.g. measurement conditions are properly documented.
        public const int cw_node_CreateNode = 2013; ///< AWhat constant for <see cref="hcasSetString"/>: creates a new child node for the node identified by AHandle and AIndexString. The name must be valid (see @ref InvalidNodeNames) and AIndex1 must contain one of the constants starting with node_DataType, e.g. <see cref="node_DataType_String"/>. See also: @ref CreateAndDeleteChildNodes.
        public const int cw_node_SaveToFile = 2014; ///< AWhat constant for <see cref="hcasSetString"/>: saves the complete root node including data and child nodes in ISD format into the path and file specified by AValue. In the future, several file formats might be selectable via AIndexString, so it is recommended to pass an empty string. See also @ref SavingNodeHandles
        public const int cw_node_Clone = 2015; ///< AWhat constant for <see cref="hcasGetHandle"/>: The node identified by AHandle and AIndexString is cloned, i.e. all data and child nodes are copied and returned as a new, separate node handle. If AIndex is null/empty, the complete node is cloned. With for example AIndexString = "Measurement Conditions" only all measurement conditions are cloned.
        ///@}

        ///@name Handle API AWhat: CAS handles
        ///Constants for the @ref handleAPI "handle API" for @ref handleTypeCAS "CAS handles"
        ///@{
        public const int cw_cas_CopySpectrum = 3001; ///< AWhat constant for <see cref="hcasGetHandle"/>: returns a node handle for the previously measuremed spectrum of the parameter set specified by AIndex1. Use @ref idx_cas_CurrentParamSet for the currently active parameter set or the 0-based index of the parameter set whose spectrum you want to retrieve. Refer to @ref CASCreateSpectrumCopy for details.
          public const int idx_cas_CurrentParamSet = -1001; ///< constant for AIndex1 if, instead of a 0-based parameter set index, the current parameter set of the CASID should be used.
        public const int cw_cas_CopySpectrumWithResults = 3002; ///< AWhat constant for <see cref="hcasGetHandle"/>: similar to <see cref="cw_cas_CopySpectrum"/> but in addition performs colormetric calculation and adds all results to the node handle. Only possible with the currently active parameter set, so AIndex1 must be <see cref="idx_cas_CurrentParamSet"/>.  Settings for colormetric calculation are defined by the CAS ID. Refer to @ref CASCreateSpectrumCopyWithResults for details.
        public const int cw_cas_CreateColorMetric = 3003; ///< AWhat constant for <see cref="hcasGetHandle"/>: returns a new @ref handleTypeColorMetric "ColorMetric handle" with the settings of the 0-based parameter set passed in AIndex1 or @ref idx_cas_CurrentParamSet for the current parameter set. Refer to @ref CASCreateColorMetric for details.
        public const int cw_cas_ParamSetMeasured = 3005; ///< AWhat constant for <see cref="hcasGetInteger"/> and <see cref="hcasSetInteger"/>: AValue identifies whether the parameter set identified by AIndex1 has been measured (<> 0) or not (0). AIndex1 can be <see cref="idx_cas_CurrentParamSet"/> or range from 0 to <see cref="dpidParamSets"/> - 1, otherwise <see cref="ErrorInvalidParameter"/> will be returned. This is similar to <see cref="mpidMeasured"/> but is not restricted to the current parameter set.
        ///@}

        ///@name Handle API AWhat: ColorMetric handles
        ///Constants for the @ref handleAPI "handle API" for @ref handleTypeColorMetric "ColorMetric handles"
        ///@{
        public const int cw_cm_SkipLevelEnabled = 4001; ///< AWhat constant for <see cref="hcasGetInteger"/> and <see cref="hcasSetInteger"/>: option, if enabled = 1, every spectrum intensity value <= <see cref="cw_cm_SkipLevel"/> is considered 0 when performing the colormetric calculations. Default value is 0. Functions like <see cref="mpidSkipLevelEnabled"/>.
        public const int cw_cm_CalculateCRI = 4002; ///< AWhat constant for <see cref="hcasGetInteger"/> and <see cref="hcasSetInteger"/>: option, if enabled = 1, the CRI (color rendering index) is calculated during <see cref="cw_cm_Calculate"/>. Default value is 0. See also <see cref="cw_cm_CRIMode"/> and @ref ColorMetricSettings "notes about ColorMetric settings".
        public const int cw_cm_CRIMode = 4004; ///< AWhat constant for <see cref="hcasGetInteger"/> and <see cref="hcasSetInteger"/>: Defines which type of CRI should be calculated. One of the constants starting with cmp_CRI, e.g. <see cref="cm_CRI_CIE13_3_95"/> (which is also the default value).
          public const int cm_CRI_DIN6169 = 1; ///< constant for <see cref="cw_cm_CRIMode"/>: CRI is calculated according to DIN 6169
          public const int cm_CRI_CIE13_3_95 = 2; ///< constant for <see cref="cw_cm_CRIMode"/>: CRI is calculated according to CIE 13.3-95
          public const int cm_CRI_JIS_Z_8726 = 3; ///< constant for <see cref="cw_cm_CRIMode"/>: CRI is calculated according to JIS Z 8726 (same as CIE CIE 13.3-95 but using CCT calculated according to JIS)
        public const int cw_cm_IlluminantReferenceName = 4005; ///< AWhat constant for <see cref="hcasGetString"/> and <see cref="hcasSetString"/>: Name of the illuminant reference defined by  the color coordinates of <see cref="cw_cm_IlluminantReference"/>. This name is shown in the measurement conditions but has no further effect.
        public const int cw_cm_IlluminantReference = 4006; ///< AWhat constant for <see cref="hcasGetFloat"/> and <see cref="hcasSetFloat"/>: color coordinates of the illuminant reference. AIndex1 must bei either <see cref="idx_IllRef_x"/> or <see cref="idx_IllRef_y"/> Refer to @ref ColorMetricSettings "ColorMetric Settings" for details.
          public const int idx_IllRef_x = -1001; ///< constant for AIndex1 for <see cref="cw_cm_IlluminantReference"/> for the x color coordinate
          public const int idx_IllRef_y = -1002; ///< constant for AIndex1 for <see cref="cw_cm_IlluminantReference"/> for the y color coordinate
        public const int cw_cm_StartWL = 4007; ///< AWhat constant for <see cref="hcasGetFloat"/> and <see cref="hcasSetFloat"/>: the start wavelength in nm where colormetric calculation should start. If this is smaller than the start wavelength of the spectrum, calculation is started there, which is why you can leave it at the default value of 0 to cover the full range (together with the stop wavelength). The section @ref ColorMetricSettings "ColorMetric Settings" has all the details.
        public const int cw_cm_StopWL = 4008; ///< AWhat constant for <see cref="hcasGetFloat"/> and <see cref="hcasSetFloat"/>: the stop wavelength in nm where colormetric calculation should stop. If this is larger than the stop wavelength of the spectrum, calculation is ending there. If you leave it at the default value of 0, the full spectrum range is used. The section @ref ColorMetricSettings "ColorMetric Settings" has all the details.
        public const int cw_cm_SkipLevel = 4009; ///< AWhat constant for <see cref="hcasGetFloat"/> and <see cref="hcasSetFloat"/>: the threshold where spectrum intensity is considered 0 for colormetric calculation, if <see cref="cw_cm_SkipLevelEnabled"/> is enabled. Works like <see cref="mpidSkipLevel"/>. Default is 0.
        public const int cw_cm_PeakDiffWidth = 4010; ///< AWhat constant for <see cref="hcasGetFloat"/> and <see cref="hcasSetFloat"/>: the width in nm that is used for differentiation when calculating the peak wavelength <see cref="casGetPeak"/>. If negative, a default width, which is suitable for the spectrum, is used. Default value is -1. Works like <see cref="mpidColormetricPeakDiffWidth"/>
        public const int cw_cm_Calculate = 4011; ///< AWhat constant for <see cref="hcasSetHandle"/>: pass in a @ref handleTypeNode "node handle" containing a spectrum to perform colormetric calculations. The "Results" childnode will be cleared and filled with the results of the new calculation.
        public const int cw_cm_AOCount = 4012; ///< AWhat constant for <see cref="hcasGetInteger"/> and <see cref="hcasSetInteger"/>: number of additional observers for which results should be calculated. Default value is 0.
        public const int cw_cm_AOIndex = 4013; ///< AWhat constant for <see cref="hcasGetInteger"/> and <see cref="hcasSetInteger"/>: 1-based index of the observer to be used from <see cref="dpidObserverList"/> for calculation of additional observer results, or 0 for none (the default, when count is increased). AIndex1 must be the 0-based index, ranging from 0 to <see cref="cw_cm_AOCount"/> - 1.
        public const int cw_cm_AOIllRefName = 4014; ///< AWhat constant for <see cref="hcasGetString"/> and <see cref="hcasSetString"/>: the name of the illuminant reference used for calculating additional observer results. Shown in measurement conditions and only for informational purposes. The default value, e.g. when count is increased, is "Illuminant E". AIndex1 must be the 0-based index, ranging from 0 to <see cref="cw_cm_AOCount"/> - 1.
        public const int cw_cm_AOIllRefx = 4015; ///< AWhat constant for <see cref="hcasGetFloat"/> and <see cref="hcasSetFloat"/>: Color coordinate x of the illuminant reference used for calculating additional observer results. The default value, e.g. when count is increased, is 1/3. AIndex1 must be the 0-based index, ranging from 0 to <see cref="cw_cm_AOCount"/> - 1.
        public const int cw_cm_AOIllRefy = 4016; ///< AWhat constant for <see cref="hcasGetFloat"/> and <see cref="hcasSetFloat"/>: Color coordinate y of the illuminant reference used for calculating additional observer results. The default value, e.g. when count is increased, is 1/3. AIndex1 must be the 0-based index, ranging from 0 to <see cref="cw_cm_AOCount"/> - 1.
        public const int cw_cm_ObserverList = 4017; ///< AWhat constant for <see cref="hcasGetString"/>: returns the list of available observers delimited by OS-dependant line breaks. Identical to <see cref="dpidObserverList"/> and provided here for convenience.

        ///@}

        ///@name Handle API AWhat: NodeOperation handles
        ///Constants for the @ref handleAPI "handle API" for @ref handleTypeNodeOperation "NodeOperation handles"
        ///@{
        public const int cw_no_Operand1 = 5001; ///< AWhat constant for <see cref="hcasGetHandle"/> and <see cref="hcasSetHandle"/>, <see cref="hcasGetString"/> and <see cref="hcasSetString"/>: first operand for a node operation - the meaning and type is entirely up to the actual operation, e.g. for <see cref="cw_no_JoinSpectra"/> this is a node handle containing the spectrum which will provide the left part of the joint spectrum.
        public const int cw_no_Operand2 = 5002; ///< AWhat constant for <see cref="hcasGetHandle"/> and <see cref="hcasSetHandle"/>, <see cref="hcasGetString"/> and <see cref="hcasSetString"/>: second operand for a node operation - it is entirely up to the actual operation if this operand is necessary at all and what type it must be, e.g. for <see cref="cw_no_JoinSpectra"/> this is a node handle containing the spectrum which will provide the right part of the joint spectrum.
        public const int cw_no_JoinWavelength = 5003; ///< AWhat constant for <see cref="hcasGetFloat"/> and <see cref="hcasSetFloat"/>: wavelength in nm where the two spectra should be joined using <see cref="cw_no_JoinSpectra"/>. This must be in the range where the spectra overlap. See @ref NodeOperationJoinSpectra.
        public const int cw_no_JoinOptions = 5004; ///< AWhat constant for <see cref="hcasGetInteger"/> and <see cref="hcasSetInteger"/>: options (bit-set) for <see cref="cw_no_JoinSpectra"/>, consisting of the flags that start with "no_JoinOption", e.g. <see cref="no_JoinOption_Match"/>. See @ref NodeOperationJoinSpectra.
          public const int no_JoinOption_Match = 1; ///< flag for <see cref="cw_no_JoinOptions"/>: the intensity of the joint spectra should be matched at <see cref="cw_no_JoinWavelength"/>.
          public const int no_JoinOption_MatchMultiply = 2; ///< flag for <see cref="cw_no_JoinOptions"/>: if <see cref="no_JoinOption_Match"/> is set, match by multiplying the matched operand spectrum instead of adding an offset.
          public const int no_JoinOption_MatchToOperand2 = 4; ///< flag for <see cref="cw_no_JoinOptions"/>: if <see cref="no_JoinOption_Match"/> is set, adjust the left part, i.e. operand 1.
        public const int cw_no_JoinSpectra = 5005; ///< AWhat constant for <see cref="hcasGetHandle"/>: returns a new node handle containing the joint spectrum of operands 1 and 2. See @ref NodeOperationJoinSpectra for a full description of all relevant settings.
        public const int cw_no_StartWavelength = 5006; ///< AWhat constant for <see cref="hcasGetFloat"/> and <see cref="hcasSetFloat"/>: start wavelength in nm used by operations <see cref="cw_no_SpectrumArithmetic"/> and <see cref="cw_no_ResampleOperand1"/>.
        public const int cw_no_StopWavelength = 5007; ///< AWhat constant for <see cref="hcasGetFloat"/> and <see cref="hcasSetFloat"/>: stop wavelength in nm used by operations <see cref="cw_no_SpectrumArithmetic"/> and <see cref="cw_no_ResampleOperand1"/>.
        public const int cw_no_ArithmeticOperation = 5008; ///< AWhat constant for <see cref="hcasGetInteger"/> and <see cref="hcasSetInteger"/>: selects the type of arithmetic operation for <see cref="cw_no_SpectrumArithmetic"/>. One of the constants that start with "no_Arithmetic", e.g. <see cref="no_Arithmetic_Add"/>. See @ref NodeOperationArithmetic.
          public const int no_Arithmetic_Add = 1; ///< constant for <see cref="cw_no_ArithmeticOperation"/>: the spectra are added
          public const int no_Arithmetic_Subtract = 2; ///< constant for <see cref="cw_no_ArithmeticOperation"/>: the spectrum from operand2 is subtracted from operand1
          public const int no_Arithmetic_Divide = 3; ///< constant for <see cref="cw_no_ArithmeticOperation"/>: the spectrum from operand1 is divided by operand2 - intensity will be set to NAN if division is not possible.
          public const int no_Arithmetic_Multiply = 4; ///< constant for <see cref="cw_no_ArithmeticOperation"/>: the spectra are multiplied
        public const int cw_no_SpectrumArithmetic = 5009; ///< AWhat constant for <see cref="hcasGetHandle"/>: returns a new node handle containing the result of the arithmetic operation between node operands 1 and 2. See @ref NodeOperationArithmetic for a full description of all settings.
        public const int cw_no_CopyNodes = 5010; ///< AWhat constant for <see cref="hcasSetString"/>: Copies the node specified by the node handle of Operand1 and the string of Operand1 containing the relative path into the node handle of Operand2, where the owner of the copy is specified by the relative path of string Operand2. The copied node will have the name of the string which is set. See @ref NodeOperationCopyNode for a detailed description.
        public const int cw_no_StepWavelength = 5011; ///< AWhat constant for <see cref="hcasGetFloat"/> and <see cref="hcasSetFloat"/>: step wavelength in nm for <see cref="cw_no_ResampleOperand1"/>.
        public const int cw_no_ResampleOperand1 = 5012; ///< AWhat constant for <see cref="hcasGetHandle"/>: returns a new node handle containing the resampled spectrum from Operand1. See @ref NodeOperationResample for a full description.
        ///@}

        ///@name Handle API AWhat: DataUnitInfo handles
        ///Constants for the @ref handleAPI "handle API" for @ref handleTypeDataUnitInfo "DataUnitInfo handles"
        ///@{
        public const int cw_du_DataUnit = 6001; ///< AWhat constant for <see cref="hcasGetString"/> and <see cref="hcasSetString"/> and <see cref="hcasGetInteger"/> and <see cref="hcasSetInteger"/>. When setting a string, the DataUnit is analyzed, taking <see cref="cw_du_FuzzyCompare"/> into account. Query the other properties of this handle to retrieve information about the analyzed DataUnit. When setting an integer, all other properties except <see cref="cw_du_Type"/> are filled and can be queried.
          public const int du_DataUnit_Unknown = 1; ///< integer constant for <see cref="cw_du_DataUnit"/> corresponding to an unrecognized DataUnit
          public const int du_DataUnit_RadiantPower = 2; ///< integer constant for <see cref="cw_du_DataUnit"/> corresponding to "radiant power" calibration unit W/nm
          public const int du_DataUnit_RadiantIntensity = 3; ///< integer constant for <see cref="cw_du_DataUnit"/> corresponding to "radiant intensity" calibration unit W/(sr nm)
          public const int du_DataUnit_Radiance_cm2 = 4; ///< integer constant for <see cref="cw_du_DataUnit"/> corresponding to "radiance" calibration unit W/(cm&sup2; sr nm)
          public const int du_DataUnit_Radiance = 5; ///< integer constant for <see cref="cw_du_DataUnit"/> corresponding to "radiance" calibration unit W/(m&sup2; sr nm)
          public const int du_DataUnit_Irradiance_cm2 = 6;  ///< integer constant for <see cref="cw_du_DataUnit"/> corresponding to "irradiance" calibration unit W/(cm&sup2; nm)
          public const int du_DataUnit_Irradiance = 7;  ///< integer constant for <see cref="cw_du_DataUnit"/> corresponding to "irradiance" calibration unit W/(m&sup2; nm)
        public const int cw_du_CorrectedDataUnit = 6002; ///< AWhat constant for <see cref="hcasGetString"/>: when a DataUnit was found or correctly set, this contains the correct syntax for the DataUnit, otherwise it is empty.
        public const int cw_du_Type = 6003; ///< AWhat constant for <see cref="hcasGetInteger"/> and <see cref="hcasSetInteger"/>: when search for a string DataUnit via setting <see cref="cw_du_DataUnit"/>, read this to analyze what type of unit was found. Otherwise set this to the desired type to retrieve other properties like <see cref="cw_du_CorrectedDataUnit"/> or <see cref="cw_du_Description"/>.
          public const int du_Type_Unknown = 0; ///<integer constant for <see cref="cw_du_Type"/>: the type of DataUnit is unknown
          public const int du_Type_Calibration = 1; ///<integer constant for <see cref="cw_du_Type"/>: the type is a calibration unit, typical per nanometer.
          public const int du_Type_RadiometricIntegral = 2; ///<integer constant for <see cref="cw_du_Type"/>: the type is a radiometric unit, i.e. result of an integration of the spectrum
          public const int du_Type_PhotometricIntegral = 3; ///<integer constant for <see cref="cw_du_Type"/>: the type of a photometric unit, i.e. result of an integration of a spectrum multiplied with a V(lambda) curve
        public const int cw_du_FuzzyCompare = 6004; ///< AWhat constant for <see cref="hcasGetInteger"/> and <see cref="hcasSetInteger"/>: if non-zero, when searching a DataUnit by setting <see cref="cw_du_DataUnit"/>, tolerate errors in the DataUnit, so brackets can be ommitted, basically any non-alphanumeric character is ignored. This might be helpful if you need to handle older fileformats and cannot ensure that special characters like &sup2; are encoded correctly.
        public const int cw_du_Radio2PhotoFactor = 6005; ///< AWhat constant for <see cref="hcasGetFloat"/>: extra factor that needs to be applied when calculating the photometric integral. This typically happens with non-SI conform DataUnits, e.g. W/cm&sup2; nm). The radiometric integral is in W/cm&sup2; but to calculate an integral in lx = lm/m&sup2; we need a factor of 10000 to convert W/cm&sup2; to W/m&sup2;
        public const int cw_du_Description = 6006; ///< AWhat constant for <see cref="hcasGetString"/>: returns an english string describing the DataUnit or an empty string if the DataUnit is unknown.
        ///@}

        ///@name Handle API AWhat: DeviceManagement handles
        ///Constants for the @ref handleAPI "handle API" for @ref handleTypeDeviceManagement "DeviceManagement handles"
        ///@{
        public const int cw_dm_InterfaceList = 7200; ///< AWhat constant, usage dependant on AIndex1: retrieval and management of the interface types that should be used when searching for devices. See @ref DMInterfaceList for an overview.
          public const int idx_dm_InterfaceCount = -1; ///< AIndex1 constant for usage with <see cref="hcasGetInteger"/> and <see cref="cw_dm_InterfaceList"/>: returns the number of available interfaces aka InterfaceCount.
          public const int idx_dm_InterfaceType = -2; ///< AIndex1 constant for usage with <see cref="hcasGetInteger"/> and <see cref="cw_dm_InterfaceList"/>: returns the interface type of the found device in the list at AIndex2, i.e. AIndex2 must be within the range of 0 to InterfaceCount - 1.
          public const int idx_dm_InterfaceName = -3; ///< AIndex1 constant for usage with <see cref="hcasGetString"/> and <see cref="cw_dm_InterfaceList"/>: returns the name of the interface type of the found device in the list at AIndex2, i.e. AIndex2 must be within the range of 0 to InterfaceCount - 1.
          public const int idx_dm_InterfaceEnabled = -4; ///< AIndex1 constant for usage with <see cref="hcasGetInteger"/> and <see cref="hcasSetInteger"/> with <see cref="cw_dm_InterfaceList"/>: returns or sets whether the interface type should be included when searching for devices (<>0) or not (=0). AIndex2 must be within the range of 0 to InterfaceCount - 1. The default is for all interface types to be enabled for the search.
          public const int idx_dm_RefreshInterfaceList = -5; ///< AIndex1 constant for usage with <see cref="hcasSetInteger"/> with <see cref="cw_dm_InterfaceList"/>: set to any integer value to refresh the interface list and return to its default state with all interface types enabled.
        public const int cw_dm_DeviceList = 7201; ///< AWhat constant, usage dependant on AIndex1: retrieval and management of the list of found devices. See @ref DMDeviceList for an overview.
          public const int idx_dm_SearchForDevices = -1; ///< AWhat constant for usage with <see cref="hcasSetInteger"/> and <see cref="cw_dm_DeviceList"/>: initiates a new search for all enabled interface types. Some interface types can be searched synchronously, so they are in the list as soon as this action returns, while others might appear only after a few seconds and <see cref="idx_dm_RefreshDeviceList"/> has been called.
          public const int idx_dm_RefreshDeviceList = -3; ///< AWhat constant for usage with <see cref="hcasGetInteger"/> and <see cref="cw_dm_DeviceList"/>: refreshes the list and checks wheter anything has changed. The returned value can be <see cref="dm_DeviceList_Unchanged"/>, <see cref="dm_DeviceList_DevicesChanged"/> or <see cref="dm_DeviceList_StateChanged"/>.
            public const int dm_DeviceList_Unchanged = 0; ///< returned value of <see cref="idx_dm_RefreshDeviceList"/> indicating the device list hasn't changed.
            public const int dm_DeviceList_DevicesChanged = 1; ///< returned value of <see cref="idx_dm_RefreshDeviceList"/> indicating the device list has different devices, i.e. at least one appeared or disappeared.
            public const int dm_DeviceList_StateChanged = 2; ///< returned value of <see cref="idx_dm_RefreshDeviceList"/> indicating the device list has the same devices, but one of them has a different <see cref="idx_dm_DeviceState"/>, indicating a looked for device has reappeared or a previously present device has disconnected.
          public const int idx_dm_DeviceCount = -4; ///< AIndex1 constant for usage with <see cref="hcasGetInteger"/> and <see cref="cw_dm_DeviceList"/>: returns the number of found devices in the list aka DeviceCount.
          public const int idx_dm_DeviceSerial = -5; ///< AIndex1 constant for usage with <see cref="hcasGetString"/> and <see cref="cw_dm_DeviceList"/>: returns the serial number of the found device in the list at AIndex2, i.e. AIndex2 must be within the range of 0 to DeviceCount - 1.
          public const int idx_dm_InterfaceOption = -6; ///< AIndex1 constant for usage with <see cref="hcasGetInteger"/> and <see cref="cw_dm_DeviceList"/>: returns the interface option of the found device in the list at AIndex2, i.e. AIndex2 must be within the range of 0 to DeviceCount - 1. Can be used as AInterfaceOption parameter of <see cref="casCreateDeviceEx"/>. Can also be queried with <see cref="hcasGetString"/> to return the interface options name like <see cref="casGetDeviceTypeOptionName"/>.
          public const int idx_dm_DeviceState = -7; ///< AIndex1 constant for usage with <see cref="hcasGetInteger"/> and <see cref="cw_dm_DeviceList"/>: returns the state of the found device in the list at AIndex2, i.e. AIndex2 must be within the range of 0 to DeviceCount - 1. Can be either <see cref="dm_DeviceState_Present"/> or <see cref="dm_DeviceState_LookedFor"/>.
            public const int dm_DeviceState_Present = 0; ///< returned value for <see cref="idx_dm_DeviceState"/>: the device is currently connected and available for operation.
            public const int dm_DeviceState_LookedFor = 1; ///< returned value for <see cref="idx_dm_DeviceState"/>: the device is longer connected but is awaited for reconnection, i.e. there is a CAS-ID which uses these interface type and option values. Typically happens after device connection was lost.
        ///@}

        ///@name Handle API AWhat: CalibManagement handles
        ///Constants for the @ref handleAPI "handle API" for @ref handleTypeCalibManagement "CalibManagement handles"
        ///@{
        public const int cw_cam_CAS = 7400; ///< AWhat constant for <see cref="hcasGetInteger"/>, <see cref="hcasSetInteger"/> and <see cref="hcasSetHandle"/>: get or sets the CAS-ID the CalibManagement handle uses. It can also be set by setting a CAS handle. The CAS-ID or the CAS handle are immediately validated resulting in errors like <see cref="ErrorInvalidHandle"/> or <see cref="errCasDeviceNotFound"/>.
        public const int cw_cam_CalibDirectory = 7401; ///< AWhat constant for <see cref="hcasGetString"/> and <see cref="hcasSetString"/>: the file path of the directory that should be searched for matching calibration files.
        public const int cw_cam_SubDirPerCASSerial = 7402; ///< AWhat constant for <see cref="hcasGetInteger"/> and <see cref="hcasSetInteger"/>: if enabled (=1), searching and downloading appends the CAS serial to the CalibDirectory path. Default is off (=0).
        public const int cw_cam_SearchDirsRecursive = 7403; ///< AWhat constant for <see cref="hcasGetInteger"/> and <see cref="hcasSetInteger"/>: if enabled (=1), searching for matching calibrations will also recursively search subdirectories. Default is off (=0).
        public const int cw_cam_FindMatchingCalibs = 7404; ///< AWhat constant for <see cref="hcasSetInteger"/>: set this to any value to start a synchronous search for matching calibrations. Refer to @ref CAMSearch for a complete overview.
        public const int cw_cam_MatchingCalibs = 7405; ///< AWhat constant, usage dependant on AIndex1: retrieval of the list of found calibrations that match the connected CAS and its accessories. Refer to @ref CAMSearch for a complete overview.
          public const int idx_cam_Count = -1; ///< AIndex1 constant for usage with <see cref="hcasGetInteger"/> and <see cref="cw_cam_MatchingCalibs"/>, <see cref="cw_cam_MismatchingCalibs"/> or <see cref="cw_cam_DownloadedFiles"/>: returns the number of elements in the list. This is required to determine the valid range for AIndex2 (0 to Count - 1), for the other idx_cam constants to retrieve properties of the found calibration or downloaded file.
          public const int idx_cam_ConfigFileName = -2; ///< AIndex1 constant for usage with <see cref="hcasGetString"/> and <see cref="cw_cam_MatchingCalibs"/> or <see cref="cw_cam_MismatchingCalibs"/>: returns the full file path of the configuration file (*.ini) of the found calibration. AIndex2 must be with in 0 to Count - 1.
          public const int idx_cam_CalibFileName = -3; ///< AIndex1 constant for usage with <see cref="hcasGetString"/> and <see cref="cw_cam_MatchingCalibs"/> or <see cref="cw_cam_MismatchingCalibs"/>: returns the full file path of the calibration file (*.isc) of the found calibration. AIndex2 must be with in 0 to Count - 1.
          public const int idx_cam_ConfigFileSerial = -4; ///< AIndex1 constant for usage with <see cref="hcasGetString"/> and <see cref="cw_cam_MatchingCalibs"/> or <see cref="cw_cam_MismatchingCalibs"/>: returns the serial number of combination of serial numbers the found configuration file is intended for (for a matching calibration it would be equal to <see cref="dpidSerialNo"/>. AIndex2 must be with in 0 to Count - 1.
          public const int idx_cam_CalibFileSerial = -5; ///< AIndex1 constant for usage with <see cref="hcasGetString"/> and <see cref="cw_cam_MatchingCalibs"/> or <see cref="cw_cam_MismatchingCalibs"/>: returns the serial number of combination of serial numbers the found calibration file is intended for (for a matching calibration it would be equal to <see cref="dpidSerialNo"/>. AIndex2 must be with in 0 to Count - 1.
          public const int idx_cam_InitializeErrorCode = -6; ///< AIndex1 constant for usage with <see cref="hcasGetInteger"/> and <see cref="cw_cam_MatchingCalibs"/> or <see cref="cw_cam_MismatchingCalibs"/>: returns the return value of <see cref="casInitialize"/> when testing whether the found calibration matches the connected device. For matching calibrations that would be <see cref="ErrorNoError"/>. AIndex2 must be with in 0 to Count - 1.
          public const int idx_cam_InitializeErrorMessage = -7; ///< AIndex1 constant for usage with <see cref="hcasGetString"/> and <see cref="cw_cam_MatchingCalibs"/> or <see cref="cw_cam_MismatchingCalibs"/>: returns the error message after <see cref="casInitialize"/> was called to test whether the found calibration matches the connected device. For matching calibrations it would be empty. AIndex2 must be with in 0 to Count - 1.
        public const int cw_cam_MismatchingCalibs = 7406;  ///< AWhat constant, usage dependant on AIndex1: retrieval of the list of found calibrations that do NOT match the connected CAS and its accessories. Usage identical to <see cref="cw_cam_MatchingCalibs"/>. Refer to @ref CAMSearch for a complete overview.
        public const int cw_cam_DownloadCalibFilesFromDevice = 7407; ///< AWhat constant for <see cref="hcasSetInteger"/>: set this to any value to download calibration files from the associated CAS into memory. Refer to @ref CAMDownload for an overview and possible reasons for failure.
        public const int cw_cam_DownloadedFiles = 7408; ///< AWhat constant, usage dependant on AIndex1: retrieval of the list of downloaded files. If no download has been done or the last download failed, <see cref="ErrorInvalidParameter"/> will be returned.
          public const int idx_cam_DownloadedBytes = -2; ///< AIndex1 constant for use with <see cref="hcasGetInteger"/> and <see cref="cw_cam_DownloadedFiles"/>: returns the total number of bytes downloaded from the CAS.
          public const int idx_cam_FileName = -3; ///< AIndex1 constant for use with <see cref="hcasGetString"/> and <see cref="cw_cam_DownloadedFiles"/>: returns the filename of the downloaded file. AIndex2 must be in the range from 0 to Count - 1.
          public const int idx_cam_FileSize = -4; ///< AIndex1 constant for use with <see cref="hcasGetInteger"/> and <see cref="cw_cam_DownloadedFiles"/>: returns the size in bytes of the downloaded file. AIndex2 must be in the range from 0 to Count - 1.
          public const int idx_cam_FileDateTime = -5; ///< AIndex1 constant for use with <see cref="hcasGetString"/> and <see cref="cw_cam_DownloadedFiles"/>: returns the file date time in ISO 8601 format without timezone info of the downloaded file.
        public const int cw_cam_CopyToCalibDir = 7409; ///< AWhat constant for <see cref="hcasSetInteger"/>: set this to any value to copy the previously downloaded files into the directory specified by <see cref="cw_cam_CalibDirectory"/>. <see cref="cw_cam_SubDirPerCASSerial"/> will be taken into account when enabled. Refer to @ref CAMDownload for an overview and possible reasons for failure.
        ///@}

        ///@name Handle API AWhat: ImportCustomObserver handles
        ///Constants for the @ref handleAPI "handle API" for @ref handleTypeImportCustomObserver "ImportCustomObserver handles"
        ///@{
        public const int cw_ico_Name = 7500; ///< AWhat constant for <see cref="hcasGetString"/> and <see cref="hcasSetString"/>: the name of the custom observer as it will appear in <see cref="dpidObserverList"/> after it has been imported. The default value for the name is "ImportedCustomObserver".
        public const int cw_ico_CMFNode = 7501; ///< AWhat constant for <see cref="hcasGetHandle"/>: returns a @ref handleTypeNode "node handle" which holds the color matching functions (YDim = 3) and the corresponding wavelengths in the x-vector. Use <see cref="cw_node_ReDim"/> to define the number of rows (XDim) and then use <see cref="cw_node_Data"/> to fill in the values. Note that this node handle needs to be destroyed, when no longer used. Destroying it will not destroy the actual data owned by this ImportCustomObserver handle.
        public const int cw_ico_WLRange = 7502; ///< AWhat constant for <see cref="hcasGetFloat"/>: when queried with AIndex1 = <see cref="idx_ico_Low"/> returnes the smallest wavelength of the CMF and with AIndex1 = <see cref="idx_ico_High"/> the largest wavelength. Just for informational purposes as this is information is collected from the <see cref="cw_ico_CMFNode"/>. If the CMF node has not been filled with data yet, both returned values will be NAN.
          public const int idx_ico_Low = -1; ///< constant for AIndex1 to be used with <see cref="cw_ico_WLRange"/> and <see cref="cw_ico_PurpleLine"/> to retrieve the lower end of the range.
          public const int idx_ico_High = -2; ///< constant for AIndex1 to be used with <see cref="cw_ico_WLRange"/> and <see cref="cw_ico_PurpleLine"/> to retrieve the upper end of the range.
        public const int cw_ico_PurpleLine = 7503; ///< AWhat constant for <see cref="hcasGetFloat"/> and <see cref="hcasSetFloat"/>: defines the purple line (or line of purples), connecting the high end (<see cref="idx_ico_High"/>) to the low end (<see cref="idx_ico_Low"/>). Both values must be within the range of the CMF. The default values are 380nm and 700nm, as used with CIE 1931.
        public const int cw_ico_LuminousEfficacy = 7504; ///< AWhat constant for <see cref="hcasGetFloat"/> and <see cref="hcasSetFloat"/>: defines the luminous efficacy (in lm/W) of the CMF. Sometimes also referred to as photometric equivalent or Km. Default value is 683.002, the value for CIE 1931.
        public const int cw_ico_Validate = 7505; ///< AWhat constant for <see cref="hcasSetInteger"/>: performs a validation to check if the custom observer can be imported. If the validation failed, an error like <see cref="ErrorInvalidParameter"/> will be returned and <see cref="cw_all_LastErrorMessage"/> will describe the reason of the failure. Possible reasons are if the <see cref="cw_ico_CMFNode"/> contains malformed data, e.g. the wavelengths do not ascend or if the <see cref="cw_ico_PurpleLine"/> is defined outside of the range of the CMF. Validation will also fail if <see cref="cw_ico_Name"/> is not unique.
        public const int cw_ico_Import = 7506; ///< AWhat constant for <see cref="hcasSetInteger"/>: imports the custom observer into <see cref="dpidObserverList"/>. Validation as in <see cref="cw_ico_Validate"/> is done implicitly. If an error is returned, check <see cref="cw_all_LastErrorMessage"/> why the import failed.
        public const int cw_ico_ImportedIndex = 7508; ///< AWhat constant for <see cref="hcasGetInteger"/>: after <see cref="cw_ico_Import"/> has been called, this returns the 0-based index at which the observer was imported. This can be used for specifying the observer in <see cref="cw_cm_AOIndex"/> or <see cref="mpidAdditionalObserver"/>, but rember to +1, since those use 1-based indices because 0 means no observer. If the observer hasn't been imported yet, -1 will be returned.
        public const int cw_ico_ObserverList = 7509; ///< AWhat constant for <see cref="hcasGetString"/>: returns the list of currently registered observers delimited by line breaks (depending on OS). Same as <see cref="dpidObserverList"/> and provided here for convenience.
        ///@}

        ///@name Handle API AWhat: all handles
        ///Constants for the @ref handleAPI "handle API" for all handles
        ///@{
        public const int cw_all_LastErrorMessage = 10001; ///< AWhat constant for <see cref="hcasGetString"/>: retrieves an error message for the last error that occured with the handle-object passed in AHandle. AHandle must be a valid handle object. Any subsequent call on this handle will clear the error message, except this one, i.e. this can be queried multiple times.
        ///@}

        ///@name Handle API
        ///Generic getter and setter methods used for all Handle API AWhat
        ///@{

        /// <summary>
        /// Performs an action which returns a handle which can be used to call other methods starting with "hcas", e.g. <see cref="hcasGetFloat"/>.
        /// </summary>
        /// <param name="AHandle">Handle to an object that supports the action described by AWhat. For actions starting with "cw_global", e.g. <see cref="cw_global_CreateCASHandle"/>, AHandle must be NULL/nil.</param>
        /// <param name="AWhat">The action to perform which returns a handle. A constant starting with "cw_", e.g. <see cref="cw_cas_CopySpectrum"/>.</param>
        /// <param name="AIndex1">Numerical parameter 1 for the action AWhat. Refer to the documentation of the action which meaning the parameter has. If none is mentioned, this parameter can be ignored by passing 0.</param>
        /// <param name="AIndex2">Numerical parameter 2 for the action AWhat. Refer to the documentation of the action which meaning the parameter has. If none is mentioned, this parameter can be ignored by passing 0.</param>
        /// <param name="AIndexString">String parameter for the action AWhat. Refer to the documentation of the action which meaning the parameter has. If none is mentioned, this parameter can be ignored by passing NULL/nil or an empty string.</param>
        /// <param name="AValue">Variable to retrieve the handle. Some actions also use the handle that is passed in AValue, i.e. the parameter is IN and OUT, e.g. <see cref="cw_global_DestroyHandle"/>.</param>
        /// <returns>The return value is 0 if the method was successful, otherwise a negative error code, e.g. <see cref="ErrorInvalidParameter"/>. Use <see cref="cw_all_LastErrorMessage"/> to retrieve an error message with more details (does not apply for cw_global_ actions which don't operate on a handle).</returns>
        /// <remarks>
        /// This is a generic method call. You need to first find the AWhat constant describing the action. The documentation for that cw_ constant will specify a) if it supports this method, b) which type of handle needs to be passed in AHandle, c) what meaning the AIndex1, AIndex2 and AIndexString parameters have and d) what type of handle is returned in AValue and whether the value passed in AValue is relevant. Refer to @ref handleAPI for an overview on how to use these methods.
        /// </remarks>
        [DllImport(CASLibName)]
        //public static extern int hcasGetHandle(nuint AHandle, int AWhat, nint AIndex1, nint AIndex2, [MarshalAs(UnmanagedType.LPUTF8Str)] string AIndexString, ref nuint AValue);
        public static extern int hcasGetHandle(UIntPtr AHandle, int AWhat, UIntPtr AIndex1, UIntPtr AIndex2, [MarshalAs(UnmanagedType.LPUTF8Str)] string AIndexString, ref UIntPtr AValue);

        /// <summary>
        /// Performs an action which accepts a handle.
        /// </summary>
        /// <param name="AHandle">Handle to an object that supports the action described by AWhat. For actions starting with "cw_global", e.g. <see cref="cw_global_DestroyHandle"/>, AHandle must be NULL/nil.</param>
        /// <param name="AWhat">The action to perform which accepts a handle. A constant starting with "cw_", e.g. <see cref="cw_cm_Calculate"/>.</param>
        /// <param name="AIndex1">Numerical parameter 1 for the action AWhat. Refer to the documentation of the action which meaning the parameter has. If none is mentioned, this parameter can be ignored by passing 0.</param>
        /// <param name="AIndex2">Numerical parameter 2 for the action AWhat. Refer to the documentation of the action which meaning the parameter has. If none is mentioned, this parameter can be ignored by passing 0.</param>
        /// <param name="AIndexString">String parameter for the action AWhat. Refer to the documentation of the action which meaning the parameter has. If none is mentioned, this parameter can be ignored by passing NULL/nil or an empty string.</param>
        /// <param name="AValue">The handle the AWhat action accepts. This is not the same as AHandle, which is the handle supporting the action. Example: <see cref="cw_cm_Calculate"/> operates on colormetric handles (AHandle) and accepts a node handle (AValue).</param>
        /// <returns>The return value is 0 if the method was successful, otherwise a negative error code, e.g. <see cref="ErrorInvalidParameter"/>. Use <see cref="cw_all_LastErrorMessage"/> to retrieve an error message with more details (does not apply for cw_global_ actions which don't operate on a handle).</returns>
        /// <remarks>
        /// This is a generic method call. You need to first find the AWhat constant describing the action. The documentation for that cw_ constant will specify a) if it supports this method, b) which type of handle needs to be passed in AHandle, c) what meaning the AIndex1, AIndex2 and AIndexString parameters have and d) what type of handle is accepted in AValue. Refer to @ref handleAPI for an overview on how to use these methods.
        /// </remarks>
        [DllImport(CASLibName)]
        //public static extern int hcasSetHandle(nuint AHandle, int AWhat, nint AIndex1, nint AIndex2, [MarshalAs(UnmanagedType.LPUTF8Str)] string AIndexString, nuint AValue);
        public static extern int hcasSetHandle(UIntPtr AHandle, int AWhat, UIntPtr AIndex1, UIntPtr AIndex2, [MarshalAs(UnmanagedType.LPUTF8Str)] string AIndexString, UIntPtr AValue);

        /// <summary>
        /// Performs an action which returns an integer.
        /// </summary>
        /// <param name="AHandle">Handle to an object that supports the action described by AWhat. For actions starting with "cw_global", e.g. <see cref="cw_global_CreateCASHandle"/>, AHandle must be NULL/nil.</param>
        /// <param name="AWhat">The action to perform which returns an integer. A constant starting with "cw_", e.g. <see cref="cw_node_ChildCount"/>.</param>
        /// <param name="AIndex1">Numerical parameter 1 for the action AWhat. Refer to the documentation of the action which meaning the parameter has. If none is mentioned, this parameter can be ignored by passing 0.</param>
        /// <param name="AIndex2">Numerical parameter 2 for the action AWhat. Refer to the documentation of the action which meaning the parameter has. If none is mentioned, this parameter can be ignored by passing 0.</param>
        /// <param name="AIndexString">String parameter for the action AWhat. Refer to the documentation of the action which meaning the parameter has. If none is mentioned, this parameter can be ignored by passing NULL/nil or an empty string.</param>
        /// <param name="AValue">Variable to retrieve the integer. Some actions also use the integer value that is passed in AValue, i.e. the parameter is IN and OUT.</param>
        /// <returns>The return value is 0 if the method was successful, otherwise a negative error code, e.g. <see cref="ErrorInvalidParameter"/>. Use <see cref="cw_all_LastErrorMessage"/> to retrieve an error message with more details (does not apply for cw_global_ actions which don't operate on a handle).</returns>
        /// <remarks>
        /// This is a generic method call. You need to first find the AWhat constant describing the action. The documentation for that cw_ constant will specify a) if it supports this method, b) which type of handle needs to be passed in AHandle, c) what meaning the AIndex1, AIndex2 and AIndexString parameters have and d) the meaning of the integer returned in AValue and whether the value passed in AValue is relevant. Refer to @ref handleAPI for an overview on how to use these methods.
        /// </remarks>
        [DllImport(CASLibName)]
        public static extern int hcasGetInteger(UIntPtr AHandle, int AWhat, UIntPtr AIndex1, UIntPtr AIndex2, [MarshalAs(UnmanagedType.LPUTF8Str)] string AIndexString, ref UIntPtr AValue);

        /// <summary>
        /// Performs an action which accepts an integer.
        /// </summary>
        /// <param name="AHandle">Handle to an object that supports the action described by AWhat. For actions starting with "cw_global", e.g. <see cref="cw_global_CreateCASHandle"/>, AHandle must be NULL/nil.</param>
        /// <param name="AWhat">The action to perform which accepts an integer. A constant starting with "cw_", e.g. <see cref="cw_node_DeleteChild"/>.</param>
        /// <param name="AIndex1">Numerical parameter 1 for the action AWhat. Refer to the documentation of the action which meaning the parameter has. If none is mentioned, this parameter can be ignored by passing 0.</param>
        /// <param name="AIndex2">Numerical parameter 2 for the action AWhat. Refer to the documentation of the action which meaning the parameter has. If none is mentioned, this parameter can be ignored by passing 0.</param>
        /// <param name="AIndexString">String parameter for the action AWhat. Refer to the documentation of the action which meaning the parameter has. If none is mentioned, this parameter can be ignored by passing NULL/nil or an empty string.</param>
        /// <param name="AValue">The integer the action accepts.</param>
        /// <returns>The return value is 0 if the method was successful, otherwise a negative error code, e.g. <see cref="ErrorInvalidParameter"/>. Use <see cref="cw_all_LastErrorMessage"/> to retrieve an error message with more details (does not apply for cw_global_ actions which don't operate on a handle).</returns>
        /// <remarks>
        /// This is a generic method call. You need to first find the AWhat constant describing the action. The documentation for that cw_ constant will specify a) if it supports this method, b) which type of handle needs to be passed in AHandle, c) what meaning the AIndex1, AIndex2 and AIndexString parameters have and d) the meaning of the integer passed in AValue. Refer to @ref handleAPI for an overview on how to use these methods.
        /// </remarks>
        [DllImport(CASLibName)]
        public static extern int hcasSetInteger(UIntPtr AHandle, int AWhat, UIntPtr AIndex1, UIntPtr AIndex2, [MarshalAs(UnmanagedType.LPUTF8Str)] string AIndexString, UIntPtr AValue);

        /// <summary>
        /// Performs an action which returns an floating point number, i.e. double.
        /// </summary>
        /// <param name="AHandle">Handle to an object that supports the action described by AWhat. For actions starting with "cw_global", e.g. <see cref="cw_global_CreateCASHandle"/>, AHandle must be NULL/nil.</param>
        /// <param name="AWhat">The action to perform which returns a double. A constant starting with "cw_", e.g. <see cref="cw_cm_StartWL"/>.</param>
        /// <param name="AIndex1">Numerical parameter 1 for the action AWhat. Refer to the documentation of the action which meaning the parameter has. If none is mentioned, this parameter can be ignored by passing 0.</param>
        /// <param name="AIndex2">Numerical parameter 2 for the action AWhat. Refer to the documentation of the action which meaning the parameter has. If none is mentioned, this parameter can be ignored by passing 0.</param>
        /// <param name="AIndexString">String parameter for the action AWhat. Refer to the documentation of the action which meaning the parameter has. If none is mentioned, this parameter can be ignored by passing NULL/nil or an empty string.</param>
        /// <param name="AValue">Variable to retrieve the floating point number. Some actions also use the value that is passed in AValue, i.e. the parameter is IN and OUT, e.g. <see cref="cw_node_Resample"/>.</param>
        /// <returns>The return value is 0 if the method was successful, otherwise a negative error code, e.g. <see cref="ErrorInvalidParameter"/>. Use <see cref="cw_all_LastErrorMessage"/> to retrieve an error message with more details (does not apply for cw_global_ actions which don't operate on a handle).</returns>
        /// <remarks>
        /// This is a generic method call. You need to first find the AWhat constant describing the action. The documentation for that cw_ constant will specify a) if it supports this method, b) which type of handle needs to be passed in AHandle, c) what meaning the AIndex1, AIndex2 and AIndexString parameters have and d) the meaning of the floating point number returned in AValue and whether the value passed in AValue is relevant. Refer to @ref handleAPI for an overview on how to use these methods.
        /// </remarks>
        [DllImport(CASLibName)]
        public static extern int hcasGetFloat(UIntPtr AHandle, int AWhat, UIntPtr AIndex1, UIntPtr AIndex2, [MarshalAs(UnmanagedType.LPUTF8Str)] string AIndexString, ref double AValue);

        /// <summary>
        /// Performs an action which accepts a floating point number.
        /// </summary>
        /// <param name="AHandle">Handle to an object that supports the action described by AWhat. For actions starting with "cw_global", e.g. <see cref="cw_global_CreateCASHandle"/>, AHandle must be NULL/nil.</param>
        /// <param name="AWhat">The action to perform which accepts a floating point number. A constant starting with "cw_", e.g. <see cref="cw_cm_StartWL"/>.</param>
        /// <param name="AIndex1">Numerical parameter 1 for the action AWhat. Refer to the documentation of the action which meaning the parameter has. If none is mentioned, this parameter can be ignored by passing 0.</param>
        /// <param name="AIndex2">Numerical parameter 2 for the action AWhat. Refer to the documentation of the action which meaning the parameter has. If none is mentioned, this parameter can be ignored by passing 0.</param>
        /// <param name="AIndexString">String parameter for the action AWhat. Refer to the documentation of the action which meaning the parameter has. If none is mentioned, this parameter can be ignored by passing NULL/nil or an empty string.</param>
        /// <param name="AValue">The floating point number the action accepts.</param>
        /// <returns>The return value is 0 if the method was successful, otherwise a negative error code, e.g. <see cref="ErrorInvalidParameter"/>. Use <see cref="cw_all_LastErrorMessage"/> to retrieve an error message with more details (does not apply for cw_global_ actions which don't operate on a handle).</returns>
        /// <remarks>
        /// This is a generic method call. You need to first find the AWhat constant describing the action. The documentation for that cw_ constant will specify a) if it supports this method, b) which type of handle needs to be passed in AHandle, c) what meaning the AIndex1, AIndex2 and AIndexString parameters have and d) the meaning of the number passed in AValue. Refer to @ref handleAPI for an overview on how to use these methods.
        /// </remarks>
        [DllImport(CASLibName)]
        public static extern int hcasSetFloat(UIntPtr AHandle, int AWhat, UIntPtr AIndex1, UIntPtr AIndex2, [MarshalAs(UnmanagedType.LPUTF8Str)] string AIndexString, double AValue);

        [DllImport(CASLibName, EntryPoint = "hcasGetString")]
        private static extern int _hcasGetString(UIntPtr AHandle, int AWhat, UIntPtr AIndex1, UIntPtr AIndex2, [In] byte[] AIndexString, [In,Out] byte[] AValue, ref UIntPtr AMaxLen);

        /// <summary>
        /// Performs an action which returns a string.
        /// </summary>
        /// <param name="AHandle">Handle to an object that supports the action described by AWhat. For actions starting with "cw_global", e.g. <see cref="cw_global_CreateCASHandle"/>, AHandle must be NULL/nil.</param>
        /// <param name="AWhat">The action to perform which returns a string. A constant starting with "cw_", e.g. <see cref="cw_node_Name"/>.</param>
        /// <param name="AIndex1">Numerical parameter 1 for the action AWhat. Refer to the documentation of the action which meaning the parameter has. If none is mentioned, this parameter can be ignored by passing 0.</param>
        /// <param name="AIndex2">Numerical parameter 2 for the action AWhat. Refer to the documentation of the action which meaning the parameter has. If none is mentioned, this parameter can be ignored by passing 0.</param>
        /// <param name="AIndexString">String parameter for the action AWhat. Refer to the documentation of the action which meaning the parameter has. If none is mentioned, this parameter can be ignored by passing NULL/nil or an empty string.</param>
        /// <param name="AValue">Variable to retrieve the string. Some actions also use the string that is passed in AValue, i.e. the parameter is IN and OUT.</param>
        /// <param name="AMaxLen">The maximum number of bytes the buffer passed in AValue can hold. This includes the null terminator. See Remarks.</param>
        /// <returns>The return value is 0 if the method was successful, otherwise a negative error code, e.g. <see cref="ErrorInvalidParameter"/>. Use <see cref="cw_all_LastErrorMessage"/> to retrieve an error message with more details (does not apply for cw_global_ actions which don't operate on a handle).</returns>
        /// <remarks>
        /// This is a generic method call. You need to first find the AWhat constant describing the action. The documentation for that cw_ constant will specify a) if it supports this method, b) which type of handle needs to be passed in AHandle, c) what meaning the AIndex1, AIndex2 and AIndexString parameters have and d) the meaning of the string returned in AValue and whether the value passed in AValue is relevant. Refer to @ref handleAPI for an overview on how to use these methods.
        ///
        /// In .NET AValue is of type StringBuilder and the passed instance should have a capacity of at least AMaxLen. All other programming environments should provide a buffer in AValue that is null-terminated and can hold up to AMaxLen number of bytes, including the null-terminator. 
        /// After hcasGetString returns, AMaxLen will contain the number of bytes (including the null-terminator) which are needed to hold the complete requested string. If the number is larger than before the call, this indicates that a larger buffer is needed and the string in AValue was truncated. In that case another call to hcasGetString with a larger buffer should be made.
        /// </remarks>
        public static int hcasGetString(
    UIntPtr AHandle, int AWhat, UIntPtr AIndex1, UIntPtr AIndex2,
    string AIndexString, StringBuilder AValue, ref UIntPtr AMaxLen)
        {
            UIntPtr initialMaxLen = UIntPtr.Zero;

            byte[] utf8Bytes = null;
            if (AValue != null)
            {
                initialMaxLen = AMaxLen;
                utf8Bytes = Encoding.UTF8.GetBytes((AValue.ToString() ?? string.Empty) + "\0");

                if ((ulong)utf8Bytes.Length > AMaxLen.ToUInt64())
                    initialMaxLen = new UIntPtr((uint)utf8Bytes.Length);
                else
                    Array.Resize(ref utf8Bytes, (int)AMaxLen.ToUInt64());
            }

            AMaxLen = initialMaxLen;

            byte[] indexBytes = Encoding.UTF8.GetBytes((AIndexString ?? string.Empty) + "\0");

            int retValue = _hcasGetString(
                AHandle, AWhat, AIndex1, AIndex2,
                indexBytes, utf8Bytes, ref AMaxLen);

            if (AValue != null)
            {
                AValue.Length = 0;

                if (utf8Bytes != null && initialMaxLen != UIntPtr.Zero)
                {
                    ulong initLen = initialMaxLen.ToUInt64();
                    ulong outLen = AMaxLen.ToUInt64();
                    int bytesToRead = (int)Math.Min(initLen, outLen);

                    if (bytesToRead > 0)
                    {
                        // null-terminator 제외
                        AValue.Append(Encoding.UTF8.GetString(utf8Bytes, 0, bytesToRead - 1));
                    }
                }
            }

            return retValue;
        }
        //public static int hcasGetString(UIntPtr AHandle, int AWhat, UIntPtr AIndex1, UIntPtr AIndex2, string AIndexString, StringBuilder AValue, ref UIntPtr AMaxLen)
        //{
        //    UIntPtr initialMaxLen = 0;

        //    byte[] utf8Bytes = null;
        //    if (AValue != null) {
        //        //prefill buffer with what AValue currently contains and ensure null-termination - some APIs might use the initial value
        //        initialMaxLen = AMaxLen;
        //        utf8Bytes = Encoding.UTF8.GetBytes(AValue.ToString() + "\0");
        //        if (utf8Bytes.Length > (int)initialMaxLen)
        //            initialMaxLen = (UIntPtr)utf8Bytes.Length;
        //        else
        //            Array.Resize(ref utf8Bytes, (int)initialMaxLen);
        //    }
        //    AMaxLen = initialMaxLen;

        //    int retValue = _hcasGetString(AHandle, AWhat, AIndex1, AIndex2, Encoding.UTF8.GetBytes(AIndexString + "\0"), utf8Bytes, ref AMaxLen);

        //    if (AValue != null) {
        //        AValue.Clear();
        //        if (utf8Bytes != null)
        //        {
        //            //the returned AMaxLen might exceed the length of utf8Bytes
        //            AValue.Append(Encoding.UTF8.GetString(utf8Bytes!, 0, (int)Math.Min(initialMaxLen, AMaxLen) - 1)); //maxLen includes null-terminator which we don't want
        //        }
        //    }

        //    return retValue;
        //}

        /// <summary>
        /// Performs an action which accepts a string.
        /// </summary>
        /// <param name="AHandle">Handle to an object that supports the action described by AWhat. For actions starting with "cw_global", e.g. <see cref="cw_global_CreateCASHandle"/>, AHandle must be NULL/nil.</param>
        /// <param name="AWhat">The action to perform which accepts a string. A constant starting with "cw_", e.g. <see cref="cw_node_Name"/>.</param>
        /// <param name="AIndex1">Numerical parameter 1 for the action AWhat. Refer to the documentation of the action which meaning the parameter has. If none is mentioned, this parameter can be ignored by passing 0.</param>
        /// <param name="AIndex2">Numerical parameter 2 for the action AWhat. Refer to the documentation of the action which meaning the parameter has. If none is mentioned, this parameter can be ignored by passing 0.</param>
        /// <param name="AIndexString">String parameter for the action AWhat. Refer to the documentation of the action which meaning the parameter has. If none is mentioned, this parameter can be ignored by passing NULL/nil or an empty string.</param>
        /// <param name="AValue">The string accepted by the action.</param>
        /// <returns>The return value is 0 if the method was successful, otherwise a negative error code, e.g. <see cref="ErrorInvalidParameter"/>. Use <see cref="cw_all_LastErrorMessage"/> to retrieve an error message with more details (does not apply for cw_global_ actions which don't operate on a handle).</returns>
        /// <remarks>
        /// This is a generic method call. You need to first find the AWhat constant describing the action. The documentation for that cw_ constant will specify a) if it supports this method, b) which type of handle needs to be passed in AHandle, c) what meaning the AIndex1, AIndex2 and AIndexString parameters have and d) the meaning of string passed in AValue. Refer to @ref handleAPI for an overview on how to use these methods.
        ///
        /// In .NET AValue is type string and null-termination and UTF-8 encoding is handled by CAS4DLL class. All other programming environments must ensure proper UTF-8 encoding and null termination of AValue (and AIndexString). 
        /// </remarks>
        [DllImport(CASLibName)]
        public static extern int hcasSetString(UIntPtr AHandle, int AWhat, UIntPtr AIndex1, UIntPtr AIndex2, [MarshalAs(UnmanagedType.LPUTF8Str)] string AIndexString, [MarshalAs(UnmanagedType.LPUTF8Str)] string AValue);

        /// <summary>
        /// Performs an action which fills a provided buffer.
        /// </summary>
        /// <param name="AHandle">Handle to an object that supports the action described by AWhat. For actions starting with "cw_global", e.g. <see cref="cw_global_CreateCASHandle"/>, AHandle must be NULL/nil.</param>
        /// <param name="AWhat">The action to perform which fills a buffer. A constant starting with "cw_", e.g. <see cref="cw_node_Data"/>.</param>
        /// <param name="AIndex1">Numerical parameter 1 for the action AWhat. Refer to the documentation of the action which meaning the parameter has. If none is mentioned, this parameter can be ignored by passing 0.</param>
        /// <param name="AIndex2">Numerical parameter 2 for the action AWhat. Refer to the documentation of the action which meaning the parameter has. If none is mentioned, this parameter can be ignored by passing 0.</param>
        /// <param name="AIndexString">String parameter for the action AWhat. Refer to the documentation of the action which meaning the parameter has. If none is mentioned, this parameter can be ignored by passing NULL/nil or an empty string.</param>
        /// <param name="AValue">Pointer to a buffer that will be filled by the action</param>
        /// <param name="ABufferSize">When calling: the maximum number of bytes the buffer where AValue points to can hold. After the call: the number of bytes the buffer must have to receive all requested data. See Remarks.</param>
        /// <returns>The return value is 0 if the method was successful, otherwise a negative error code, e.g. <see cref="ErrorInvalidParameter"/>. Use <see cref="cw_all_LastErrorMessage"/> to retrieve an error message with more details (does not apply for cw_global_ actions which don't operate on a handle).</returns>
        /// <remarks>
        /// This is a generic method call. You need to first find the AWhat constant describing the action. The documentation for that cw_ constant will specify a) if it supports this method, b) which type of handle needs to be passed in AHandle, c) what meaning the AIndex1, AIndex2 and AIndexString parameters have and d) the size and/or the layout of the data in the buffer where AValue is pointing to. Refer to @ref handleAPI for an overview on how to use these methods.
        /// </remarks>
        [DllImport(CASLibName)]
        public static extern int hcasGetBuffer(UIntPtr AHandle, int AWhat, UIntPtr AIndex1, UIntPtr AIndex2, [MarshalAs(UnmanagedType.LPUTF8Str)] string AIndexString, IntPtr AValue, ref UIntPtr ABufferSize);

        /// <summary>
        /// Performs an action which accepts a filled buffer.
        /// </summary>
        /// <param name="AHandle">Handle to an object that supports the action described by AWhat. For actions starting with "cw_global", e.g. <see cref="cw_global_CreateCASHandle"/>, AHandle must be NULL/nil.</param>
        /// <param name="AWhat">The action to perform which accepts a buffer. A constant starting with "cw_", e.g. <see cref="cw_node_Data"/>.</param>
        /// <param name="AIndex1">Numerical parameter 1 for the action AWhat. Refer to the documentation of the action which meaning the parameter has. If none is mentioned, this parameter can be ignored by passing 0.</param>
        /// <param name="AIndex2">Numerical parameter 2 for the action AWhat. Refer to the documentation of the action which meaning the parameter has. If none is mentioned, this parameter can be ignored by passing 0.</param>
        /// <param name="AIndexString">String parameter for the action AWhat. Refer to the documentation of the action which meaning the parameter has. If none is mentioned, this parameter can be ignored by passing NULL/nil or an empty string.</param>
        /// <param name="AValue">The buffer accepted by the action.</param>
        /// <param name="ABufferSize">The size of the data in bytes where AValue is pointing to.</param>
        /// <returns>The return value is 0 if the method was successful, otherwise a negative error code, e.g. <see cref="ErrorInvalidParameter"/>. Use <see cref="cw_all_LastErrorMessage"/> to retrieve an error message with more details (does not apply for cw_global_ actions which don't operate on a handle).</returns>
        /// <remarks>
        /// This is a generic method call. You need to first find the AWhat constant describing the action. The documentation for that cw_ constant will specify a) if it supports this method, b) which type of handle needs to be passed in AHandle, c) what meaning the AIndex1, AIndex2 and AIndexString parameters have and d) the meaning of the buffer passed in AValue and which size it should have. Refer to @ref handleAPI for an overview on how to use these methods.
        [DllImport(CASLibName)]
        public static extern int hcasSetBuffer(UIntPtr AHandle, int AWhat, UIntPtr AIndex1, UIntPtr AIndex2, [MarshalAs(UnmanagedType.LPUTF8Str)] string AIndexString, IntPtr AValue, UIntPtr ABufferSize);
        ///@}

        ///@name MultiTrack
        ///APIs for @ref multiTrackMeasurements
        
        ///@{
        /// <summary>
        /// Initializes the MultiTrack buffer
        /// </summary>
        /// <param name="ADevice">The device / CASID</param>
        /// <param name="ATracks">The number of MultiTracks that should later be measured</param>
        /// <returns>The return value is 0 if the method was successful or a negative error code</returns>
        /// <remarks>
        /// casMultiTrackInit prepares a @ref multiTrackMeasurements for the given device. 
        /// ATracks defines the number of tracks (i.e. raw spectra) which will be acquired during the MultiTrack measurement and should not exceed <see cref="dpidMultiTrackMaxCount"/>. 
        /// Since the memory used for saving MultiTrack measurements needs to be allocated beforehand, it is not possible to change the number of tracks on the fly. 
        /// You may use <see cref="dpidMultiTrackCount"/> to check how many tracks have been allocated.
        /// @note It is mandatory to call <see cref="casMultiTrackDone"/> to discard the allocated MultiTrack memory when the MultiTrack data is no longer needed. 
        /// casMultiTrackInit and <see cref="casMultiTrackLoadData"/> implicitly call <see cref="casMultiTrackDone"/> before initializing/loading new MultiTrack data.
        ///    </remarks>
        [DllImport(CASLibName)]
        public static extern int casMultiTrackInit(int ADevice, int ATracks);

        /// <summary>
        /// Releases a previously allocated MultiTrack buffer
        /// </summary>
        /// <param name="ADevice">The device / CASID</param>
        /// <returns>The return value is 0 if the method was successful or a negative error code</returns>
        /// <remarks>
        /// casMultiTrackDone releases the MultiTrack buffer after it has been allocated with <see cref="casMultTrackInit"/> or <see cref="casMultiTrackLoadData"/>.
        ///    Obviously, after that, the raw spectra in the MultiTrack buffer are no longer available.
        /// For an overview, refer to the chapter @ref multiTrackMeasurements.
        ///    </remarks>
        [DllImport(CASLibName)]
        public static extern int casMultiTrackDone(int ADevice);

        /// <summary>
        /// Releases a previously allocated MultiTrack buffer
        /// </summary>
        /// <param name="ADevice">The device / CASID</param>
        /// <param name="ATrack">The 0-based track index which should be copied/activated</param>
        /// <returns>The return value is 0 if the method was successful or a negative error code</returns>
        /// <remarks>
        /// casMultiTrackCopyData retrieves the spectrum specified by ATrack from the MultiTrack measurement buffer 
        /// (ATrack is 0-based, so it may range from 0 to <see cref="dpidMultiTrackCount"/> - 1). 
        /// The MultiTrack measurement may have been performed (<see cref="paMultiTrackStart"/>) or loaded (<see cref="casMultiTrackLoadData"/>).
        /// The spectrum is stored internally as if it would have just been measured, so a subsequent call to <see cref="casColorMetric"/> calculates the colormetric results for it etc.
        /// For an overview, refer to the chapter @ref multiTrackMeasurements.
        ///    </remarks>
        [DllImport(CASLibName)]
        public static extern int casMultiTrackCopyData(int ADevice, int ATrack);

        /// <summary>
        /// Saves a MultiTrack buffer to a .SWM file.
        /// </summary>
        /// <param name="ADevice">The device / CASID</param>
        /// <param name="AFileName">Full null-terminated path of the .SWM file which should be created</param>
        /// <returns>The return value is 0 if the method was successful or a negative error code</returns>
        /// <remarks>
        /// Saves the MultiTrack buffer to AFileName. Depending on the number of tracks, the file can get rather big and saving it may take a while.
        /// The resulting .SWM file can be loaded by SpecWin Pro or by calling <see cref="casMultiTrackLoadData"/>. An existing file at AFileName will be overwritten.
        /// For an overview, refer to the chapter @ref multiTrackMeasurements.
        /// There are A and W overloads for ANSI and Unicode versions of the method.
        /// </remarks>
        [DllImport(CASLibName, CharSet = CharSet.Auto)]
        public static extern int casMultiTrackSaveData(int ADevice, string AFileName);

        /// <summary>
        /// Loads a MultiTrack buffer from a .SWM file.
        /// </summary>
        /// <param name="ADevice">The device / CASID</param>
        /// <param name="AFileName">Full null-terminated path of the .SWM file which should be loaded</param>
        /// <returns>The return value is 0 if the method was successful or a negative error code</returns>
        /// <remarks>
        /// Loads MultiTrack data for the specified device from the file specified by AFileName and discards any MultiTrack data which may have previously been 
        /// loaded or acquired (<see cref="casMultiTrackDone"/> is called implicitly). 
        /// The file must have been saved with <see cref="casMultiTrackSaveData"/>. Use <see cref="dpidMultiTrackCount"/> to check how many tracks have been loaded. 
        /// If you want to retrieve colormetric results or the spectra, call <see cref="casMultiTrackCopyData"/>, but make sure the same configuration and calibration files are used for the device!
        /// @note It is mandatory to call <see cref="casMultiTrackDone"/> to discard the allocated MultiTrack memory, when the MultiTrack data is no longer needed. 
        /// For an overview, refer to the chapter @ref multiTrackMeasurements.
        /// There are A and W overloads for ANSI and Unicode versions of the method.
        ///    </remarks>
        [DllImport(CASLibName, CharSet = CharSet.Auto)]
        public static extern int casMultiTrackLoadData(int ADevice, string AFileName);
        ///@}

        [DllImport(CASLibName)]
        public static extern void casSetData(int ADevice, int AIndex, double Value);

        [DllImport(CASLibName)]
        public static extern void casSetXArray(int ADevice, int AIndex, double Value);

        [DllImport(CASLibName)]
        public static extern void casSetDarkCurrent(int ADevice, int AIndex, double Value);

        [DllImport(CASLibName)]
        public static extern IntPtr casGetDataPtr(int ADevice);

        [DllImport(CASLibName)]
        public static extern IntPtr casGetXPtr(int ADevice);
    }
}
