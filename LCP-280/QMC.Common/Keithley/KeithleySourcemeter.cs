using QMC.Common.Component;
using QMC.Common.PKGTester;
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
    public class KeithleySourcemeter : BaseComponent, IDisposable
    {
        #region Defines
        public enum SMUInstrumentCategory
        {
            Keithley260X,
            Keithley261X,
            Keithley263X,
        }
        #endregion

        #region Field
        private List<TestConditionItem> testItems = new List<TestConditionItem>();
        private Dictionary<string, TestItemResult> results = new Dictionary<string, TestItemResult>();
        #endregion

        #region Property
        public new KeithelySourcemeterConfig Config { get; private set; }
        public KeithleyInstrumentCommunicator Communicator { get; private set; }
        public Dictionary<string, KeithleySourcemeterChannel> Channels { get; private set; }
        public string ModelName { get; private set; }
        public string SerialNo { get; private set; }
        public string FirmwareRevision { get; private set; }
        public IDictionary<string, TestItemResult> Results => results;
        #endregion

        #region Constructor
        public KeithleySourcemeter(string name) : base(name)
        {
            Communicator = new KeithleyInstrumentCommunicator();
            Config = new KeithelySourcemeterConfig(name);

            // 2611B 기준으로 구성함. (추후 변경 필요)
            Channels = new Dictionary<string, KeithleySourcemeterChannel>
            {
                { "smua", new KeithleySourcemeterChannel("smua", this) },
                //{ "smub", new KeithleySourcemeterChannel("smub", this) },
            };
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
                    if (Communicator != null)
                    {
                        if (Communicator.IsConnected)
                        {
                            Communicator.CloseSession();
                        }
                        Communicator = null;
                    }
                }
                // Dispose unmanaged resources
                disposed = true;
            }   
        }
        #endregion

        #region Override Methods
        public override int Initialize()
        {
            try
            {
                // Open session
                if (Communicator.IsConnected)
                    Communicator.CloseSession();

                if (!Communicator.OpenSession(Config.ResourceName))
                {
                    throw new Exception($"Failed to open session. (ResourceName: {Config.ResourceName})");
                }

                // Query Identification
                string identification = "";
                if (!Communicator.Query("*IDN?", ref identification))
                {
                    throw new Exception($"Failed to get identification.");
                }

                string[] items = identification.Split(',');
                if (items.Length == 4 && items[0].Contains("Keithley Instruments"))
                {
                    ModelName = items[1].Trim();
                    SerialNo = items[2].Trim();
                    FirmwareRevision = items[3].Trim();
                }
                else
                {
                    throw new Exception($"[{Name}] is not keithley sourcemeter.");
                }    

                // Load Script
                if (SendUserScript() != 0)
                {
                    throw new Exception($"Failed to send user script. (FileName: {Config.ScriptFileName})");
                }

                // Apply Channel Config
                if (ApplyParameter() != 0)
                {
                    throw new Exception("Failed to apply parameter.");
                }

                //// Initialize SMU
                if (Init() != 0)
                {
                    throw new Exception("Failed to initialize sourcemeter.");
                }

                //// Initialize SMU Channels
                foreach (var item in Channels)
                {
                    var channel = item.Value;
                    if (!channel.Init())
                    {
                        throw new Exception($"Failed to initialize channel [{channel.Name}].");
                    }
                }
            }
            catch (Exception ex)
            {
                ModelName = "";
                SerialNo = "";
                FirmwareRevision = "";

                Log.Write(ex);
                return -1;
            }
            return 0;
        }
        #endregion

        public int ApplyParameter()
        {
            try
            {
                // Sourcemeter Parameter 적용
                // -

                // Sourcemeter Channel Parameter 적용
                foreach (var item in Channels)
                {
                    var channel = item.Value;
                    if (!channel.ApplyConfig())
                    {
                        throw new Exception($"Failed to apply channel [{channel.Name}] config.");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                return -1;
            }
            return 0;
        }

        public int Init()
        {
            try
            {
                if (!Communicator.Write("init()"))
                    throw new Exception("Failed to send init command.");
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                return -1;
            }
            return 0;
        }

        public int Measure()
        {
            try
            {
                KeithleySourcemeterChannel channel = Channels["smua"];
                if (Config.IsSimulated)
                {
                    channel.SimulateBufferData();
                    return 0;
                }

                if (!channel.RunCommands())
                    throw new Exception("Failed to run measure commands.");

                StopWatch sw = new StopWatch();
                sw.Start();

                while (!channel.WaitComplete())
                {
                    if (sw.Elapsed.Milliseconds >= Config.MeasureTimeout)
                        throw new Exception("Measurement timeout occurred.");

                    Thread.Sleep(10);
                }

                if (!channel.ReadBufferData())
                    throw new Exception("Failed to read buffer data.");
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                return -1;
            }
            return 0;
        }

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

            TestItemCategory category = item.GetTestItemCategory();
            if (!(category == TestItemCategory.Electrical || category == TestItemCategory.ElectricalSource))
                return false;

            testItems.Add(item);
            if (category == TestItemCategory.Electrical)
                results.Add(item.Name, new TestItemResult());
            return true;
        }
        public bool BuildTestCommands()
        {
            try
            {
                KeithleySourcemeterChannel channel = Channels["smua"]; // 현재 smua 채널만 사용함. 추후 변경 필요.
                channel.ClearCommands();
                foreach (var item in testItems)
                {
                    KeithleySourcemeterChannel.ChannelCommand command = new KeithleySourcemeterChannel.ChannelCommand();
                    switch (item.Type)
                    {
                        case TestItemType.VF:
                            {
                                command.Name = item.Name;
                                command.Action = KeithleySourcemeterChannel.CommandAction.MeasureV;
                                command.SourceValue = item.SourceValue;
                                command.SourceTime = item.SourceTime;
                                command.SourceLimit = item.SourceLimit;
                                command.SourceRange = GetISourceRange(command.SourceValue);
                                command.MeasureTime = item.MeasureTime;
                                command.MeasureRange = GetVMeasureRange(command.SourceLimit);
                                channel.AddCommand(command);
                            }
                            break;
                        case TestItemType.VR:
                            {
                                command.Name = item.Name;
                                command.Action = KeithleySourcemeterChannel.CommandAction.MeasureV;
                                command.SourceValue = item.SourceValue * -1.0;
                                command.SourceTime = item.SourceTime;
                                command.SourceLimit = item.SourceLimit;
                                command.SourceRange = GetISourceRange(command.SourceValue);
                                command.MeasureTime = item.MeasureTime;
                                command.MeasureRange = GetVMeasureRange(command.SourceLimit);
                                channel.AddCommand(command);
                            }
                            break;
                        case TestItemType.IF:
                            {
                                command.Name = item.Name;
                                command.Action = KeithleySourcemeterChannel.CommandAction.MeasureI;
                                command.SourceValue = item.SourceValue;
                                command.SourceTime = item.SourceTime;
                                command.SourceLimit = item.SourceLimit;
                                command.SourceRange = GetVSourceRange(command.SourceValue);
                                command.MeasureTime = item.MeasureTime;
                                command.MeasureRange = GetIMeasureRange(command.SourceLimit);
                                channel.AddCommand(command);
                            }
                            break;
                        case TestItemType.IR:
                            {
                                command.Name = item.Name;
                                command.Action = KeithleySourcemeterChannel.CommandAction.MeasureI;
                                command.SourceValue = item.SourceValue * -1;
                                command.SourceTime = item.SourceTime;
                                command.SourceLimit = item.SourceLimit;
                                command.SourceRange = GetVSourceRange(command.SourceValue);
                                command.MeasureTime = item.MeasureTime;
                                command.MeasureRange = GetIMeasureRange(command.SourceLimit);
                                channel.AddCommand(command);
                            }
                            break;
                        case TestItemType.VPulseSweep:
                            {
                                command.Name = item.Name;
                                command.Action = KeithleySourcemeterChannel.CommandAction.PulseSweepVAndTrigger;
                                command.SourceValue = item.SourceValue;
                                command.PulseWidth = item.PulseWidth;
                                command.PulsePeriod = item.PulsePeriod;
                                command.PulseCount = item.PulseCount;
                                channel.AddCommand(command);
                            }
                            break;
                        case TestItemType.IPulseSweep:
                            {
                                command.Name = item.Name;
                                command.Action = KeithleySourcemeterChannel.CommandAction.PulseSweepIAndTrigger;
                                command.SourceValue = item.SourceValue;
                                command.PulseWidth = item.PulseWidth;
                                command.PulsePeriod = item.PulsePeriod;
                                command.PulseCount = item.PulseCount;
                                channel.AddCommand(command);
                            }
                            break;
                        default:
                            throw new InvalidOperationException($"Not supported test item type. (Type: {item.Type})");
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
        public bool GetResultProcess()
        {
            try
            {
                KeithleySourcemeterChannel channel = Channels["smua"]; // 현재 smua 채널만 사용함. 추후 변경 필요.

                int measureCount = 0;
                foreach (var item in testItems)
                {
                    if (item.IsMeasureItem())
                        measureCount ++;
                }

                if (channel.BufferDatas.Length != measureCount)
                {
                    throw new InvalidOperationException($"Data count mismatch. (Expected: {testItems.Count}, Actual: {channel.BufferDatas.Length})");
                }

                // Data assign
                for (int i = 0; i < channel.BufferDatas.Length; i ++)
                {
                    TestConditionItem item = testItems[i];
                    TestItemResult itemResult = results[item.Name];

                    double value = double.Parse(channel.BufferDatas[i]);
                    switch (testItems[i].Type)
                    {
                        case TestItemType.VF:
                            {
                                itemResult.RawData = value;
                                itemResult.Value = value;
                                itemResult.Unit = "V";
                            }
                            break;
                        case TestItemType.VR:
                            {
                                itemResult.RawData = value;
                                itemResult.Value = value;
                                itemResult.Unit = "V";
                            }
                            break;
                        case TestItemType.IF:
                            {
                                itemResult.RawData = value;
                                itemResult.Value = value;
                                itemResult.Unit = "A";
                            }
                            break;
                        case TestItemType.IR:
                            {
                                itemResult.RawData = value;
                                itemResult.Value = value;
                                itemResult.Unit = "A";
                            }
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                foreach (var key in results.Keys.ToList())
                {
                    results[key].Reset();
                }
                Log.Write(ex);
                return false;
            }
            return true;
        }
        #endregion

        // Script Methods
        #region Script Methods
        private int SendUserScript()
        {
            if (!File.Exists(Config.ScriptFileName))
                return -1;

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
                        if (!Communicator.Write(arrangeText))
                            throw new Exception("Failed to send user script.");
                    }

                    //string a = sb.ToString();

                    //if (!Communicator.Write(sb.ToString()))
                    //    throw new Exception("Failed to send user script.");
                }
            }
            catch (Exception ex)
            {
                // Error handling
                Log.Write(ex);
                return -1;
            }
            return 0;
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
    }
}