using QMC.Common.Component;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QMC.Common.Keithley
{
    public class KeithelySourcemeterConfig : BaseConfig
    {
        #region Defines
        public enum SMUSenseMode
        {
            LOCAL = 0, // local sense (2 wire)
            REMOTE = 1, // remote sense (4 wite)
            CALA = 3 // calibration sense mode
        }
        public enum SMUSourceSink
        {
            DISABLE = 0, // Turns off sink mode
            ENABLE = 1 // Turns on sink mode
        }
        public enum SMUSourceSettling
        {
            SMOOTH = 0, // Turns off additional settling operations
            FAST_RANGE = 1, // Instructs the source-measure unit (SMU) to use a faster procedure when changing ranges
            FAST_POLARITY = 2, // Instructs the SMU to change polarity without going to zero
            DIRECT_IRANGE = 3, //  Instructs the SMU to change the current range directly
            SMOOTH_100NA = 4, // Enables the use of range rampers for the 100 nA range
            FAST_ALL = 128 // Enables all smuX.SETTLE_FAST_* operations
        }
        public enum SMUSourceOffmode
        {
            NORMAL = 0, //  Configures the source function according to smuX.source.offfunc attribute
            ZERO = 1, // Configures source to output 0 V as smuX.OUTPUT_NORMAL with different compliance handling
            HIGH_Z = 2 // Opens the output relay when the output is turned off
        }
        public enum SMUMeasureAutoZero
        {
            OFF = 0, // Autozero disabled 
            ONCE = 1, // Performs autozero once, then disables autozero
            AUTO = 2 // Automatic checking of reference and zero measurements; an autozero is performed when needed
        }
        #endregion

        #region Field & Property
        public KeithleySourcemeter.SMUInstrumentCategory Model { get; set; }
        public string ResourceName { get; set; }
        public string ScriptFileName { get; set; }
        public SMUSenseMode SenseMode { get; set; }
        public SMUSourceSink SourceSink { get; set; }
        public SMUSourceSettling SourceSettling { get; set; }
        public SMUSourceOffmode SourceOffmode { get; set; }
        public SMUMeasureAutoZero MeasureAutoZero { get; set; }
        public int MeasureTimeout { get; set; }
        #endregion

        #region Constructor
        public KeithelySourcemeterConfig(string name) : base(name)
        {
        }
        #endregion

        #region Methods
        public override void Reset()
        {
            // Reset to default values
            Model = KeithleySourcemeter.SMUInstrumentCategory.Keithley260X;
            ResourceName = string.Empty;
            ScriptFileName = string.Empty;
            SenseMode = SMUSenseMode.LOCAL;
            SourceSink = SMUSourceSink.DISABLE;
            SourceSettling = SMUSourceSettling.SMOOTH;
            SourceOffmode = SMUSourceOffmode.NORMAL;
            MeasureAutoZero = SMUMeasureAutoZero.AUTO;
        }

        public override bool Validate()
        {
            // Validate the configuration values
            if (string.IsNullOrEmpty(ResourceName))
                return false;
            if (string.IsNullOrEmpty(ScriptFileName))
                return false;

            return true;
        }
        #endregion
    }
}
