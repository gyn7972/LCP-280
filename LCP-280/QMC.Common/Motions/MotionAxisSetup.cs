using System;
using System.ComponentModel;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using QMC.Common;

namespace QMC.Common.Motions
{
    //public enum ActiveLevel { Low = 0, High = 1 }
    //public enum OutputMode { TwoPulse_High_CCW_CW, TwoPulse_Low_CCW_CW, AB_Phase }
    //public enum EncoderInput { Normal, Reverse, Reverse_SQR4 }
    //public enum InputSource { Encoder, ServoDriver, External }
    //public enum StopMode { Emergency, DecelStop }
    //public enum HomeSignal { NegativeLimit, PositiveLimit, HomeSensor }

    [Serializable]
    public sealed class MotionAxisSetup : BaseSetup
    {
        // ===== Common =====
        // ⚠️ Name은 BaseConfig에 이미 존재 (여기서 재선언하지 않음)

        [Category("Common"), DisplayName("Board No")]
        [DefaultValue(0)]
        public int BoardNo { get; set; } = 0;

        [Category("Common"), DisplayName("Axis No")]
        [DefaultValue(0)]
        public int AxisNo { get; set; } = 0;

        [Category("Common"), DisplayName("Pulses Per Unit")]
        [DefaultValue(1)]
        public int PulsesPerUnit { get; set; } = 1;

        [Category("Common"), DisplayName("Axis Scale")]
        [DefaultValue(1000)]
        public int AxisScale { get; set; } = 1000;

        [Category("Common"), DisplayName("Axis Power (%)")]
        [DefaultValue(100)]
        public int AxisPowerPercent { get; set; } = 100;

        // ===== Config (배선/방향) =====
        [Category("Config"), DisplayName("Pulse Output")]
        [DefaultValue(PulseOutput.TwoPulse_High_CCW_CW)]
        public PulseOutput PulseOutput { get; set; } = PulseOutput.TwoPulse_High_CCW_CW;

        [Category("Config"), DisplayName("Encoder Input")]
        [DefaultValue(EncoderInput.Reverse_SQR4)]
        public EncoderInput EncoderInput { get; set; } = EncoderInput.Reverse_SQR4;

        [Category("Config"), DisplayName("Input Source")]
        [DefaultValue(InputSource.Encoder)]
        public InputSource InputSource { get; set; } = InputSource.Encoder;

        [Category("Config"), DisplayName("Z-Phase Level")]
        [DefaultValue(ActiveLevel.High)]
        public ActiveLevel ZPhaseLevel { get; set; } = ActiveLevel.High;

        [Category("Config"), DisplayName("Servo On Level")]
        [DefaultValue(ActiveLevel.High)]
        public ActiveLevel ServoOnLevel { get; set; } = ActiveLevel.High;

        // ===== Emergency =====
        [Category("Emergency Signal"), DisplayName("Level")]
        [DefaultValue(ActiveLevel.High)]
        public ActiveLevel EmergencyLevel { get; set; } = ActiveLevel.High;

        [Category("Emergency Signal"), DisplayName("Stop Mode")]
        [DefaultValue(StopMode.Emergency)]
        public StopMode StopMode { get; set; } = StopMode.Emergency;

        // ===== InpPosition =====
        [Category("InPosition"), DisplayName("Level")]
        [DefaultValue(InPosition.High)]
        public InPosition InPosition { get; set; } = InPosition.High;

        [Category("Inposition"), DisplayName("Software Limit")]
        [DefaultValue(false)]
        public bool SoftwareLimitEnable { get; set; } = false;

        [Category("Inposition"), DisplayName("Software Length (mm)")]
        [DefaultValue(0.0)]
        public double SoftwareLength { get; set; } = 0.000;

        // ===== Home =====
        [Category("Home"), DisplayName("Signal Level")]
        [DefaultValue(ActiveLevel.High)]
        public ActiveLevel HomeSignalLevel { get; set; } = ActiveLevel.High;

        [Category("Home"), DisplayName("Mode")]
        [DefaultValue(HomeMode.NegativeLimit)]
        public HomeMode HomeMode { get; set; } = HomeMode.NegativeLimit;

        [Category("Home"), DisplayName("Direction")]
        [DefaultValue(HomeDirection.Ccw)]
        public HomeDirection HomeDirection { get; set; } = HomeDirection.Ccw;

        [Category("Home"), DisplayName("Signal")]
        [DefaultValue(HomeSignal.PositiveLimit)]
        public HomeSignal HomeSignal { get; set; } = HomeSignal.PositiveLimit;

        [Category("Home"), DisplayName("Z Phase")]
        [DefaultValue(HomeZPhase.None)]
        public HomeZPhase HomeZPhase { get; set; } = HomeZPhase.None;

        [Category("Home"), DisplayName("Clear Time(ms)")]
        [DefaultValue(false)]
        public double HomeClearTime { get; set; } = 1000.000;

        [Category("Home"), DisplayName("Offset(mm)")]
        [DefaultValue(false)]
        public double HomeOffset { get; set; } = 0;

        // ===== Alarm =====
        [Category("Alarm"), DisplayName("Reset Signal Level")]
        [DefaultValue(ActiveLevel.High)]
        public ActiveLevel AlarmResetLevel { get; set; } = ActiveLevel.High;

        [Category("Alarm"), DisplayName("Alarm Level")]
        [DefaultValue(ActiveLevel.Low)]
        public ActiveLevel AlarmLevel { get; set; } = ActiveLevel.Low;

        // ===== Limit =====
        [Category("Limit"), DisplayName("Soft Limit Enable")]
        [DefaultValue(false)]
        public bool SoftLimitEnable { get; set; } = false;

        [Category("Limit"), DisplayName("+End Limit")]
        [DefaultValue(ActiveLevel.Low)]
        public ActiveLevel PositiveLimitLevel { get; set; } = ActiveLevel.Low;

        [Category("Limit"), DisplayName("-End Limit")]
        [DefaultValue(ActiveLevel.Low)]
        public ActiveLevel NegativeLimitLevel { get; set; } = ActiveLevel.Low;

        [Category("Limit"), DisplayName("Soft Limit-")]
        [DefaultValue(-1000.0)]
        public double SoftLimitMin { get; set; } = -1000.000;

        [Category("Limit"), DisplayName("Soft Limit+")]
        [DefaultValue(1000.0)]
        public double SoftLimitMax { get; set; } = +1000.000;

        // ===== Timeout =====
        [Category("Timeout"), DisplayName("Home Timeout(ms)")]
        [DefaultValue(30000)]
        public int HomeTimeoutMs { get; set; } = 30_000;

        [Category("Timeout"), DisplayName("Move Timeout(ms)")]
        [DefaultValue(20000)]
        public int MoveTimeoutMs { get; set; } = 20_000;

        [Category("Timeout"), DisplayName("Sensor Detection Timeout(ms)")]
        [DefaultValue(20000)]
        public int SensorDetectionTimeoutMs { get; set; } = 20_000;

        // ===== BaseConfig Hooks =====
        public override void Reset()
        {
            // 구조/기본값 보강 (여기선 값형 위주라 특별 보강 없음)
            if (AxisScale == 0) AxisScale = 1;
        }

        protected override void OnLoaded()
        {
            // 간단 보정 (잘못된 입력값이 저장돼 있을 경우)
            if (AxisPowerPercent < 0) AxisPowerPercent = 0;
            if (AxisPowerPercent > 100) AxisPowerPercent = 100;

            if (SoftLimitEnable && SoftLimitMin > SoftLimitMax)
            {
                // 최소한의 안전 보정: 뒤집어 정렬
                var tmp = SoftLimitMin;
                SoftLimitMin = SoftLimitMax;
                SoftLimitMax = tmp;
            }

            if (AxisScale == 0.0) AxisScale = 1;
            if (PulsesPerUnit <= 0.0) PulsesPerUnit = 1000;
        }

        public override bool Validate()
        {
            if (PulsesPerUnit <= 0)
                throw new ArgumentOutOfRangeException(nameof(PulsesPerUnit));
            if (AxisPowerPercent < 0 || AxisPowerPercent > 100)
                throw new ArgumentOutOfRangeException(nameof(AxisPowerPercent));
            if (SoftLimitEnable && SoftLimitMin > SoftLimitMax)
                throw new ArgumentException("SoftLimitMin > SoftLimitMax");
            return true;
        }

        public override string GetFilePath()
        {
            // /Configs/MotionAxis/Setup/{Name}.json
            var dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configs", "MotionAxis", "Setup");
            var file = string.IsNullOrWhiteSpace(Name) ? "default.json" : (Name + ".json");
            return Path.Combine(dir, file);
        }

        protected override JsonSerializerSettings GetJsonSettings()
        {
            return new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Include,   // 기본값도 저장
                ObjectCreationHandling = ObjectCreationHandling.Replace
            };
        }

        // ===== 정적 JSON 유틸 (기존 호환) =====
        private static JsonSerializerSettings WriteSettings => new JsonSerializerSettings
        {
            Formatting = Formatting.None,
            NullValueHandling = NullValueHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Include,
            ObjectCreationHandling = ObjectCreationHandling.Replace
        };

        private static JsonSerializerSettings ReadSettings => new JsonSerializerSettings
        {
            Formatting = Formatting.None,
            NullValueHandling = NullValueHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Populate,  // [DefaultValue]로 채움
            ObjectCreationHandling = ObjectCreationHandling.Replace
        };

        public string ToJson(bool indented = true)
        {
            return JsonConvert.SerializeObject(
                this,
                indented ? Formatting.Indented : Formatting.None,
                WriteSettings
            );
        }

        public void Save(string filePath, bool indented = true)
        {
            Validate();
            var dir = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            var json = ToJson(indented);
            File.WriteAllText(filePath, json, Encoding.UTF8);
        }

        public static MotionAxisSetup FromJson(string json)
        {
            var obj = JsonConvert.DeserializeObject<MotionAxisSetup>(json, ReadSettings);
            if (obj == null) throw new InvalidDataException("Invalid JSON for MotionAxisSetup.");
            obj.Validate();
            return obj;
        }

        public static MotionAxisSetup Load(string filePath)
        {
            var json = File.ReadAllText(filePath, Encoding.UTF8);
            return FromJson(json);
        }

        /// <summary>
        /// 파일 없으면 생성, 있으면 로드 후 누락 필드를 기본값으로 채우고(backfill) 즉시 저장.
        /// </summary>
        public static MotionAxisSetup LoadOrCreate(string filePath, bool indented = true, bool backfill = true)
        {
            MotionAxisSetup setup;
            if (!File.Exists(filePath))
            {
                setup = new MotionAxisSetup();
                setup.Save(filePath, indented);
                return setup;
            }

            var json = File.ReadAllText(filePath, Encoding.UTF8);
            setup = FromJson(json);

            if (backfill)
                setup.Save(filePath, indented);

            return setup;
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
    }
}
