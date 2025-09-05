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
        #endregion

        #region Field
        private string identification = "";
        
        #endregion

        #region Property
        public new KeithelySourcemeterConfig Config { get; private set; }
        public KeithleyInstrumentCommunicator Communicator { get; private set; }
        public Dictionary<string, KeithleySourcemeterChannel> Channels { get; private set; }
        public string ModelName { get; private set; }
        public string SerialNo { get; private set; }
        public string FirmwareRevision { get; private set; }
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
                    throw new InvalidOperationException($"Failed to open session. (ResourceName: {Config.ResourceName})");
                }

                // Query Identification
                if (!Communicator.Query("*IDN?", ref identification))
                {
                    throw new InvalidOperationException($"Failed to get identification.");
                }

                string[] items = identification.Split(',');
                if (items.Length == 4 && items[0] == "Keithley Instruments")
                {
                    ModelName = items[1].Trim();
                    SerialNo = items[2].Trim();
                    FirmwareRevision = items[3].Trim();
                }
                else
                {
                    throw new InvalidOperationException($"[{Name}] is not keithley sourcemeter.");
                }    

                // Load Script
                if (!LoadScript())
                {
                    throw new InvalidOperationException($"Failed to load script. (FileName: {Config.ScriptFileName})");
                }

                // Apply Channel Config
                foreach (var item in Channels)
                {
                    var channel = item.Value;
                    if (channel.ApplyConfig() != 0)
                    {
                        throw new InvalidOperationException($"Failed to apply channel [{channel.Name}] config.");
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

        // Script Methods
        #region Script Methods
        private bool LoadScript()
        {
            if (!Communicator.IsConnected)
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

                    Communicator.Write(sb.ToString());
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