using QMC.Common.Component;
using System;
using System.Collections.Generic;
using System.IO.Ports;
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

        public override PropertyCollection GetPropertyCollection()
        {
            PropertyCollection pc = new PropertyCollection();
            PropertyBase p;

            // Title
            string title = $"Sourcemeter [{Name}] - Config";
            pc.Add(new TitleOnlyProperty(title));

            // Value
            p = new ComboBoxProperty("Model", Model.ToString(), Enum.GetNames(typeof(KeithleySourcemeter.SMUInstrumentCategory)).ToList()); 
            pc.Add(p);
            p = new StringProperty("Resource Name", ResourceName); 
            pc.Add(p);
            p = new StringProperty("Script File Name", ScriptFileName); 
            pc.Add(p);
            p = new ComboBoxProperty("Sense Mode", SenseMode.ToString(), Enum.GetNames(typeof(SMUSenseMode)).ToList()); 
            pc.Add(p);
            p = new ComboBoxProperty("Source Sink", SourceSink.ToString(), Enum.GetNames(typeof(SMUSourceSink)).ToList()); 
            pc.Add(p);
            p = new ComboBoxProperty("Source Settling", SourceSettling.ToString(), Enum.GetNames(typeof(SMUSourceSettling)).ToList()); 
            pc.Add(p);
            p = new ComboBoxProperty("Source Off mode", SourceOffmode.ToString(), Enum.GetNames(typeof(SMUSourceOffmode)).ToList()); 
            pc.Add(p);
            p = new ComboBoxProperty("Measure Auto Zero", MeasureAutoZero.ToString(), Enum.GetNames(typeof(SMUMeasureAutoZero)).ToList()); 
            pc.Add(p);
            p = new IntProperty("Measure Timeout", MeasureTimeout); 
            pc.Add(p);

            return pc;
        }
        public override int ApplyValueFromPropertyCollection(PropertyCollection pc)
        {
            if (pc == null)
                return -1;

            foreach (var p in pc)
            {
                try
                {
                    switch (p.Title)
                    {
                        case "Model":
                            Model = (KeithleySourcemeter.SMUInstrumentCategory)Enum.Parse(typeof(KeithleySourcemeter.SMUInstrumentCategory), p.Value?.ToString());
                            break;
                        case "Resource Name":
                            ResourceName = p.Value?.ToString() ?? "";
                            break;
                        case "Script File Name":
                            ScriptFileName = p.Value?.ToString() ?? "";
                            break;
                        case "Sense Mode":
                            SenseMode = (SMUSenseMode)Enum.Parse(typeof(SMUSenseMode), p.Value?.ToString());
                            break;
                        case "Source Sink":
                            SourceSink = (SMUSourceSink)Enum.Parse(typeof(SMUSourceSink), p.Value?.ToString());
                            break;
                        case "Source Settling":
                            SourceSettling = (SMUSourceSettling)Enum.Parse(typeof(SMUSourceSettling), p.Value?.ToString());
                            break;
                        case "Source Off mode":
                            SourceOffmode = (SMUSourceOffmode)Enum.Parse(typeof(SMUSourceOffmode), p.Value?.ToString());
                            break;
                        case "Measure Auto Zero":
                            MeasureAutoZero = (SMUMeasureAutoZero)Enum.Parse(typeof(SMUMeasureAutoZero), p.Value?.ToString());
                            break;
                        case "Measure Timeout":
                            MeasureTimeout = int.Parse(p.Value?.ToString());
                            break;
                        default:
                            // Unknown property, ignore or handle as needed
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Log.Write(ex);
                    return -1;
                }
            }

            return 0;
        }
        #endregion
    }
}
