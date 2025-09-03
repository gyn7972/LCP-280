using QMC.Common.Component;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QMC.Common.Keithley
{
    /// <summary>
    /// Keithley Sourcemeter 클래스입니다.
    /// </summary>
    public class KeithleySourcemeter : BaseComponent
    {
        #region Defines
        public enum SMUInstrumentCategory
        {
            Keithley260X,
            Keithley261X,
            Keithley263X,
        }
        public enum SourceCategory
        {
            None,
            Current,
            Voltage,
        }
        public enum MeasureCategory
        {
            None,
            Current,
            Voltage,
            //PulseSweepOnly,
        }
        public class MeasurementConfig
        {
            #region Field & Property
            // Source
            public SourceCategory SourceFunction { get; set; } = SourceCategory.None;
            public double SourceLevel { get; set; } = 0.0;
            public double SourceLimit { get; set; } = 0.0;
            public double SourceRange { get; set; } = 0.0;
            public double SourceOnDelay { get; set; } = 0.0;
            // Measure
            public MeasureCategory MeasureFunction { get; set; } = MeasureCategory.None;
            public double MeasureRange { get; set; } = 0.0;
            public double MeasureNplc { get; set; } = 1.0;
            #endregion

            #region Constructor
            public MeasurementConfig()
            {
                Reset();
            }
            #endregion

            #region Methods
            public void Reset()
            {
                SourceFunction = default;
                SourceLevel = default;
                SourceLimit = default;
                SourceRange = default;
                SourceOnDelay = default;
                MeasureFunction = default;
                MeasureRange = default;
                MeasureNplc = default;
            }
            #endregion
        }
        public class MeasurementResult
        {
            #region Field & Property
            public double MeasureValue { get; set; } = 0.0;
            #endregion

            #region Constructor
            public MeasurementResult()
            {
            }
            #endregion

            #region Methods
            public void Reset()
            {
                this.MeasureValue = default;
            }
            #endregion
        }
        public class MeasurementItem
        {
            #region Field & Property
            public string Name { get; set; } = string.Empty;
            public MeasurementConfig Config;
            public MeasurementResult Result;
            #endregion

            #region Constructor
            public MeasurementItem(string name)
            {
                this.Name = name;
                this.Config = new MeasurementConfig();
                this.Result = new MeasurementResult();
            }
            #endregion

            #region Methods
            public void Reset()
            {
                this.Name = default;
                this.Config.Reset();
                this.Result.Reset();
            }
            #endregion
        }
        #endregion

        #region Field
        private KeithleyInstrumentCommunicator communicator;
        private List<MeasurementItem> measureItems;
        private List<string> commands;
        #endregion

        #region Property
        public new KeithelySourcemeterConfig Config { get; private set; }
        public bool IsConnected
        {
            get { return communicator.IsConnected; }
        }
        #endregion

        #region Constructor
        public KeithleySourcemeter(string name) : base(name)
        {
            communicator = new KeithleyInstrumentCommunicator();
            measureItems = new List<MeasurementItem>();
            commands = new List<string>();

            Config = new KeithelySourcemeterConfig(name);
        }
        #endregion

        #region Event
        // Communicator Event
        public event EventHandler OnReceived
        {
            add { communicator.OnReceived += value; }
            remove { communicator.OnReceived -= value; }
        }
        public event EventHandler OnSessionOpened
        {
            add { communicator.OnSessionOpened += value; }
            remove { communicator.OnSessionOpened -= value; }
        }
        public event EventHandler OnSessionClosed
        {
            add { communicator.OnSessionClosed += value; }
            remove { communicator.OnSessionClosed -= value; }
        }
        public event EventHandler<Exception> OnError
        {
            add { communicator.OnError += value; }
            remove { communicator.OnError -= value; }
        }
        #endregion


        #region Override Methods
        public override int Initialize()
        {
            int ret = 0;
            try
            {
                if (communicator.IsConnected)
                    communicator.CloseSession();

                if (!communicator.OpenSession(Config.ResourceName))
                {
                    throw new InvalidOperationException($"Failed to open session. (ResourceName: {Config.ResourceName})");
                }

                if (!LoadScript())
                {
                    throw new InvalidOperationException($"Failed to load script. (FileName: {Config.ScriptFileName})");
                }
            }
            catch (Exception ex)
            {
                ret = -1;
            }
            return ret;
        }

        public override int Create()
        {
            return 0;
        }

        public override void Close()
        {
            communicator.CloseSession();
        }
        #endregion

        #region Methods
        public int Write(string command)
        {
            if (!communicator.IsConnected)
                return -1;
            int ret = 0;
            if (!communicator.Write(command))
                ret = -2;
            return ret;
        }

        public int Read(ref string response)
        {
            if (!communicator.IsConnected)
                return -1;
            int ret = 0;
            if (!communicator.Read(ref response))
                ret = -2;
            return ret;
        }

        public int Query(string command, ref string response)
        {
            if (!communicator.IsConnected)
                return -1;
            int ret = 0;
            if (!communicator.Query(command, ref response))
                ret = -2;
            return ret;
        }

        public bool LoadParameter()
        {
            bool result = false;
            do
            {
                // Do something
                result = true;
            }
            while (false);
            return result;
        }

        public bool ApplyParameter()
        {
            bool result = false;
            do
            {
                // Do something
                result = true;
            }
            while (false);
            return result;
        }
        public bool Compile()
        {
            bool result = false;
            do
            {
                if (!OnCompile())
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
                result = MeasureSync(10, Config.MeasureTimeout);
            }
            while (false);
            return result;
        }

        // Measurement Methods
        #region Measure Methods
        private bool SendMeasureCommand()
        {
            bool result = false;
            try
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("startmeasure()");
                foreach (string command in commands)
                {
                    sb.AppendLine(command);
                }
                sb.AppendLine("endmeasure()");
                result = this.communicator.Write(sb.ToString());
            }
            catch (Exception ex)
            {
                // Error handling
            }
            return result;
        }
        private bool CheckMeasureCommandFinish()
        {
            bool result = false;
            try
            {
                string readText = string.Empty;
                this.communicator.Read(ref readText);
                result = readText.Contains("END");
            }
            catch (Exception ex)
            {
                // Error handling
            }
            return false;
        }
        private bool QueryMeasureResult()
        {
            bool result = false;
            try
            {
                string writeText = $"printbuffer(1,{this.commands.Count},readingbuffer.readings)";
                string readText = string.Empty;
                if (this.communicator.Query(writeText, ref readText))
                {
                    // Parsing
                    string[] strings = readText.Split(',');
                    for (int i = 0; i < this.commands.Count; i++)
                    {
                        this.measureItems[i].Result.MeasureValue = double.Parse(strings[i].Trim());
                    }
                }
            }
            catch (Exception ex)
            {
                // Error handling
            }
            return result;
        }
        private bool MeasureSync(int intervalDelayMs, int timeoutMs)
        {
            bool result = false;
            try
            {
                // Send the measure command
                if (!SendMeasureCommand())
                    return false;

                // Wait for the measurement to finish
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();

                while (!CheckMeasureCommandFinish())
                {
                    if (stopwatch.ElapsedMilliseconds > timeoutMs)
                        throw new TimeoutException("Measurement command timed out.");

                    Thread.Sleep(intervalDelayMs);
                }

                // Query the measurement results
                if (!QueryMeasureResult())
                    return false;

                result = true;
            }
            catch (Exception ex)
            {
                // Error handling
            }
            finally
            {
            }
            return result;
        }
        #endregion

        // Script Methods
        #region Script Methods
        private bool LoadScript()
        {
            if (!this.communicator.IsConnected)
                return false;
            if (!File.Exists(Config.ScriptFileName))
                return false;

            bool result = false;
            try
            {
                string arrangeText = "";
                int lineCommentPosition = 0;
                bool comment = false;

                string[] textValue = System.IO.File.ReadAllLines(Config.ScriptFileName);
                if (textValue.Length > 0)
                {
                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < textValue.Length; i++)
                    {
                        // 텍스트의 앞 뒤 빈공간 제거
                        arrangeText = textValue[i].Trim();

                        // 빈 텍스트 경우 패스
                        if (arrangeText == "")
                            continue;

                        // 범위 주석처리 경우 패스
                        if (arrangeText.Contains("--[[") == true)
                        {
                            comment = true;
                            continue;
                        }
                        if (arrangeText.Contains("]]--") == true)
                        {
                            comment = false;
                            continue;
                        }
                        if (comment == true)
                            continue;

                        // 주석처리 경우 주석처리 앞에 텍스트가 있다면 가져온다.
                        lineCommentPosition = arrangeText.IndexOf("--");
                        if (lineCommentPosition > 0)
                            arrangeText = arrangeText.Substring(0, lineCommentPosition).Trim();

                        if (arrangeText == "")
                            continue;

                        sb.AppendLine(arrangeText);
                    }

                    this.communicator.Write(sb.ToString());
                    result = true;
                }
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
        #endregion

        // Measurement Item Methods
        #region Measurement Item Methods
        public bool AddItem(MeasurementItem item)
        {
            if (item == null)
                return false;

            bool result = false;
            try
            {
                this.measureItems.Add(item);
                result = true;
            }
            catch (Exception ex)
            {
            }
            return result;
        }
        public void ClearItemList()
        {
            this.measureItems.Clear();
        }
        public MeasurementItem GetItem(int index)
        {
            if (index < 0 || index >= this.measureItems.Count)
                return null;
            return this.measureItems[index];
        }
        public MeasurementItem GetItem(string name)
        {
            if (string.IsNullOrEmpty(name))
                return null;
            return this.measureItems.FirstOrDefault(item => item.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }
        #endregion

        // Compile Methods
        #region Compile Methods
        private bool OnCompile()
        {
            if (this.measureItems.Count == 0)
            {
                return false;
            }

            this.commands.Clear();

            bool result = false;
            try
            {
                for (int i = 0; i < this.measureItems.Count; i++)
                {
                    MeasurementItem item = this.measureItems[i];
                    commands.Add(GetCommandString(item));
                }
                result = true;
            }
            catch (Exception ex)
            {
                commands.Clear();
            }
            return result;
        }
        private string GetCommandString(MeasurementItem item)
        {
            string commandString = string.Empty;
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item), "Measurement item cannot be null.");
            }

            if (item.Config.SourceFunction == SourceCategory.Current && item.Config.MeasureFunction == MeasureCategory.Voltage)
            {
                // I Sweep, V Measure
                commandString = $"iv({item.Config.SourceLevel},{item.Config.SourceRange},{item.Config.MeasureNplc},{item.Config.SourceLimit},{item.Config.SourceOnDelay})";
            }
            else if (item.Config.SourceFunction == SourceCategory.Voltage && item.Config.MeasureFunction == MeasureCategory.Current)
            {
                // V Sweep, I Measure
                commandString = $"vi({item.Config.SourceLevel},{item.Config.SourceRange},{item.Config.MeasureNplc},{item.Config.SourceLimit},{item.Config.SourceOnDelay})";
            }
            else
            {
                throw new NotSupportedException("Unsupported source and measure function combination.");
            }
            return commandString;
        }
        #endregion

        // Range Methods
        #region Range Methods
        public double GetISourceRange(double ISourceValue)
        {
            double sourceSize = Math.Abs(ISourceValue);
            double sourceRange = 0;
            switch (Config.Model)
            {
                case SMUInstrumentCategory.Keithley260X:
                    {
                        if (sourceSize >= 1.5) { sourceRange = 3; }
                        else if (sourceSize >= 1) { sourceRange = 1.5; }
                        else if (sourceSize >= 1e-1) { sourceRange = 1; }
                        else if (sourceSize >= 1e-2) { sourceRange = 1e-1; }
                        else if (sourceSize >= 1e-3) { sourceRange = 1e-2; }
                        else if (sourceSize >= 1e-4) { sourceRange = 1e-3; }
                        else if (sourceSize >= 1e-5) { sourceRange = 1e-4; }
                        else if (sourceSize >= 1e-6) { sourceRange = 1e-5; }
                        else { sourceRange = 1e-6; }
                    }
                    break;
                case SMUInstrumentCategory.Keithley261X:
                case SMUInstrumentCategory.Keithley263X:
                    {
                        if (sourceSize >= 1) { sourceRange = 1.5; }
                        else if (sourceSize >= 1e-1) { sourceRange = 1; }
                        else if (sourceSize >= 1e-2) { sourceRange = 1e-1; }
                        else if (sourceSize >= 1e-3) { sourceRange = 1e-2; }
                        else if (sourceSize >= 1e-4) { sourceRange = 1e-3; }
                        else if (sourceSize >= 1e-5) { sourceRange = 1e-4; }
                        else if (sourceSize >= 1e-6) { sourceRange = 1e-5; }
                        else { sourceRange = 1e-6; }
                    }
                    break;
            }
            return sourceRange;
        }
        public double GetVSourceRange(double VSourceValue)
        {
            double sourceSize = Math.Abs(VSourceValue);
            double sourceRange = 0;
            switch (Config.Model)
            {
                case SMUInstrumentCategory.Keithley260X:
                    {
                        if (sourceSize >= 35) { sourceRange = 40; }
                        else if (sourceSize >= 20) { sourceRange = 35; }
                        else if (sourceSize >= 6) { sourceRange = 20; }
                        else if (sourceSize >= 1) { sourceRange = 6; }
                        else if (sourceSize >= 1e-1) { sourceRange = 1; }
                        else { sourceRange = 1e-1; }
                    }
                    break;
                case SMUInstrumentCategory.Keithley261X:
                case SMUInstrumentCategory.Keithley263X:
                    {
                        if (sourceSize >= 180) { sourceRange = 200; }
                        else if (sourceSize >= 20) { sourceRange = 180; }
                        else if (sourceSize >= 5) { sourceRange = 20; }
                        else if (sourceSize >= 1) { sourceRange = 5; }
                        else if (sourceSize >= 1e-1) { sourceRange = 1; }
                        else { sourceRange = 1e-1; }
                    }
                    break;
            }
            return sourceRange;
        }
        public double GetIMeasureRange(double ISourceLimit)
        {
            double sourceLimitSize = Math.Abs(ISourceLimit);
            double measureRange = 0;
            switch (Config.Model)
            {
                case SMUInstrumentCategory.Keithley260X:
                    {
                        if (sourceLimitSize < 1e-5) { measureRange = 1e-5; }
                        else if (sourceLimitSize < 1e-4) { measureRange = 1e-4; }
                        else if (sourceLimitSize < 1e-3) { measureRange = 1e-3; }
                        else if (sourceLimitSize < 1e-2) { measureRange = 1e-2; }
                        else if (sourceLimitSize < 1e-1) { measureRange = 1e-1; }
                        else if (sourceLimitSize < 1) { measureRange = 1; }
                        else if (sourceLimitSize < 1.5) { measureRange = 1.5; }
                        else { measureRange = 3; }
                    }
                    break;
                case SMUInstrumentCategory.Keithley261X:
                case SMUInstrumentCategory.Keithley263X:
                    {
                        if (sourceLimitSize < 1e-5) { measureRange = 1e-5; }
                        else if (sourceLimitSize < 1e-4) { measureRange = 1e-4; }
                        else if (sourceLimitSize < 1e-3) { measureRange = 1e-3; }
                        else if (sourceLimitSize < 1e-2) { measureRange = 1e-2; }
                        else if (sourceLimitSize < 1e-1) { measureRange = 1e-1; }
                        else if (sourceLimitSize < 1) { measureRange = 1; }
                        else { measureRange = 1.5; }
                    }
                    break;
            }
            return measureRange;
        }
        public double GetVMeasureRange(double VSourceLimit)
        {
            double sourceLimitSize = Math.Abs(VSourceLimit);
            double measureRange = 0;
            switch (Config.Model)    
            {
                case SMUInstrumentCategory.Keithley260X:
                    {
                        if (sourceLimitSize < 1e-1) { measureRange = 1e-1; }
                        else if (sourceLimitSize < 1) { measureRange = 1; }
                        else if (sourceLimitSize < 6) { measureRange = 6; }
                        else if (sourceLimitSize < 20) { measureRange = 20; }
                        else if (sourceLimitSize < 35) { measureRange = 35; }
                        else { measureRange = 40; }
                    }
                    break;
                case SMUInstrumentCategory.Keithley261X:
                case SMUInstrumentCategory.Keithley263X:
                    {
                        if (sourceLimitSize < 1e-1) { measureRange = 1e-1; }
                        else if (sourceLimitSize < 1) { measureRange = 1; }
                        else if (sourceLimitSize < 5) { measureRange = 5; }
                        else if (sourceLimitSize < 20) { measureRange = 20; }
                        else if (sourceLimitSize < 180) { measureRange = 180; }
                        else { measureRange = 200; }
                    }
                    break;
            }
            return measureRange;
        }
        #endregion

        // Other Methods
        #region Other Methods
        public static double ConvertMilliSecondToNplc(double millisecond)
        {
            const double conversion = 60.0 / 1000.0;
            return millisecond * conversion;
        }
        #endregion
        #endregion
    }
}
