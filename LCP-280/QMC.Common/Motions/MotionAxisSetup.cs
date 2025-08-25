using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace QMC.Common.Motions
{
    public enum ActiveLevel { Low = 0, High = 1 }
    public enum OutputMode { TwoPulse_High_CCW_CW, TwoPulse_Low_CCW_CW, AB_Phase }
    public enum InputMode { Normal, Reverse, Reverse_SQR4 }
    public enum InputSource { Encoder, ServoDriver, External }
    public enum StopMode { Emergency, DecelStop }
    public enum HomeMode { NegativeLimit, PositiveLimit, HomeSensor }

    [Serializable]
    public sealed class MotionAxisSetup : BaseConfig
    {
        // Common
        [Category("Common"), DisplayName("Axis Name")]
        public string Name { get; set; } = "Axis";

        [Category("Common"), DisplayName("Board No")]
        public int BoardNo { get; set; } = 0;

        [Category("Common"), DisplayName("Axis No")]
        public int AxisNo { get; set; } = 0;

        [Category("Common"), DisplayName("Pulses per Unit")]
        public double PulsesPerUnit { get; set; } = 1000.0;

        [Category("Common"), DisplayName("Axis Scale")]
        public double AxisScale { get; set; } = 1.000;

        [Category("Common"), DisplayName("Axis Power (%)")]
        public int AxisPowerPercent { get; set; } = 100;

        // Config (배선/방향)
        [Category("Config"), DisplayName("Output Mode")]
        public OutputMode OutputMode { get; set; } = OutputMode.TwoPulse_High_CCW_CW;

        [Category("Config"), DisplayName("Input Mode")]
        public InputMode InputMode { get; set; } = InputMode.Reverse_SQR4;

        [Category("Config"), DisplayName("Input Source")]
        public InputSource InputSource { get; set; } = InputSource.Encoder;

        [Category("Config"), DisplayName("Z-Phase Level")]
        public ActiveLevel ZPhaseLevel { get; set; } = ActiveLevel.High;

        [Category("Config"), DisplayName("Servo Level")]
        public ActiveLevel ServoLevel { get; set; } = ActiveLevel.High;

        // Emergency
        [Category("Emergency Signal"), DisplayName("Level")]
        public ActiveLevel EmergencyLevel { get; set; } = ActiveLevel.High;

        [Category("Emergency Signal"), DisplayName("Stop Mode")]
        public StopMode StopMode { get; set; } = StopMode.Emergency;

        // Inposition
        [Category("Inposition"), DisplayName("Level")]
        public ActiveLevel InpositionLevel { get; set; } = ActiveLevel.High;

        [Category("Inposition"), DisplayName("Software Limit")]
        public bool SoftwareLimitEnable { get; set; } = false;

        [Category("Inposition"), DisplayName("Software Length (mm)")]
        public double SoftwareLength { get; set; } = 0.000;

        // Home
        [Category("Home"), DisplayName("Signal Level")]
        public ActiveLevel HomeSignalLevel { get; set; } = ActiveLevel.High;

        [Category("Home"), DisplayName("Mode")]
        public HomeMode HomeMode { get; set; } = HomeMode.NegativeLimit;

        // Alarm
        [Category("Alarm"), DisplayName("Reset Signal Level")]
        public ActiveLevel AlarmResetSignal { get; set; } = ActiveLevel.High;

        [Category("Alarm"), DisplayName("Alarm Level")]
        public ActiveLevel AlarmLevel { get; set; } = ActiveLevel.Low;

        // Limit
        [Category("Limit"), DisplayName("Soft Limit Enable")]
        public bool SoftLimitEnable { get; set; } = false;

        [Category("Limit"), DisplayName("Soft Limit Min (mm)")]
        public double SoftLimitMin { get; set; } = -1000.000;

        [Category("Limit"), DisplayName("Soft Limit Max (mm)")]
        public double SoftLimitMax { get; set; } = +1000.000;

        // Timeout
        [Category("Timeout"), DisplayName("Home Timeout (ms)")]
        public int HomeTimeoutMs { get; set; } = 30_000;

        [Category("Timeout"), DisplayName("Move Timeout (ms)")]
        public int MoveTimeoutMs { get; set; } = 20_000;

        // 간단 검증 유틸(원하면 호출)
        public void Validate()
        {
            if (PulsesPerUnit <= 0)
                throw new ArgumentOutOfRangeException(nameof(PulsesPerUnit));

            if (AxisPowerPercent < 0 || AxisPowerPercent > 100)
                throw new ArgumentOutOfRangeException(nameof(AxisPowerPercent));

            if (SoftLimitEnable && SoftLimitMin > SoftLimitMax)
                throw new ArgumentException("SoftLimitMin > SoftLimitMax");
        }

        // ===== JSON 저장/불러오기 =====
        #region JSON Persistence (Newtonsoft.Json)
        private static JsonSerializerSettings JsonSettings
        {
            get
            {
                return new JsonSerializerSettings
                {
                    Formatting = Formatting.None,
                    NullValueHandling = NullValueHandling.Ignore,
                    DefaultValueHandling = DefaultValueHandling.Include,
                    ObjectCreationHandling = ObjectCreationHandling.Replace
                };
            }
        }

        public string ToJson(bool indented = true)
        {
            return JsonConvert.SerializeObject(
                this,
                indented ? Formatting.Indented : Formatting.None,
                JsonSettings
            );
        }

        public void Save(string filePath, bool indented = true)
        {
            Validate();
            var json = ToJson(indented);
            File.WriteAllText(filePath, json, Encoding.UTF8);
        }

        public static MotionAxisSetup FromJson(string json)
        {
            var obj = JsonConvert.DeserializeObject<MotionAxisSetup>(json, JsonSettings);
            if (obj == null) throw new InvalidDataException("Invalid JSON for MotionAxisSetup.");
            obj.Validate();
            return obj;
        }

        public static MotionAxisSetup Load(string filePath)
        {
            var json = File.ReadAllText(filePath, Encoding.UTF8);
            return FromJson(json);
        }

        public static bool TryLoad(string filePath, out MotionAxisSetup result, out string error)
        {
            result = null; error = null;
            try
            {
                result = Load(filePath);
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        public bool TrySave(string filePath, bool indented, out string error)
        {
            error = null;
            try
            {
                Validate();
                var dir = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                var json = ToJson(indented);
                File.WriteAllText(filePath, json, Encoding.UTF8);
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        // 편의 오버로드
        public bool TrySave(string filePath, out string error)
        {
            return TrySave(filePath, true, out error);
        }
        #endregion
    }
}
