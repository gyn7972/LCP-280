using System;
using System.ComponentModel;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace QMC.Common.Motions
{
    public enum ProfileMode
    {
        Trapezoid = 0,
        SCurve = 1
    }

    [Serializable]
    public sealed class MotionAxisConfig : BaseConfig
    {
        // ===== Home Speed =====
        [Category("Speed.Home"), DisplayName("Home Speed (mm/s)")]
        [DefaultValue(0.0)]
        public double HomeSpeed { get; set; } = 0.000;

        [Category("Speed.Home"), DisplayName("H-Return Speed (mm/s)")]
        [DefaultValue(0.0)]
        public double HomeReturnSpeed { get; set; } = 0.000;

        [Category("Speed.Home"), DisplayName("H-Recursion Speed (mm/s)")]
        [DefaultValue(0.0)]
        public double HomeRecursionSpeed { get; set; } = 0.000;

        [Category("Speed.Home"), DisplayName("Z-Phase Speed (mm/s)")]
        [DefaultValue(0.0)]
        public double ZPhaseSpeed { get; set; } = 0.000;

        [Category("Speed.Home"), DisplayName("Home Acc (mm/s^2)")]
        [DefaultValue(0.0)]
        public double HomeAcc { get; set; } = 0.000;

        [Category("Speed.Home"), DisplayName("H-Return Acc (mm/s^2)")]
        [DefaultValue(0.0)]
        public double HomeReturnAcc { get; set; } = 0.000;

        // ===== Jog =====
        [Category("Speed.Jog"), DisplayName("Fine Velocity (mm/s)")]
        [DefaultValue(0.0)]
        public double JogFineVelocity { get; set; } = 0.000;

        [Category("Speed.Jog"), DisplayName("Coarse Velocity (mm/s)")]
        [DefaultValue(0.0)]
        public double JogCoarseVelocity { get; set; } = 0.000;

        [Category("Speed.Jog"), DisplayName("Accelerator (mm/s^2)")]
        [DefaultValue(0.0)]
        public double JogAcc { get; set; } = 0.000;

        [Category("Speed.Jog"), DisplayName("Decelerator (mm/s^2)")]
        [DefaultValue(0.0)]
        public double JogDec { get; set; } = 0.000;

        // ===== Run =====
        [Category("Speed.Run"), DisplayName("Max Velocity (mm/s)")]
        [DefaultValue(0.0)]
        public double MaxVelocity { get; set; } = 0.000;

        [Category("Speed.Run"), DisplayName("Accelerator (mm/s^2)")]
        [DefaultValue(0.0)]
        public double RunAcc { get; set; } = 0.000;

        [Category("Speed.Run"), DisplayName("Decelerator (mm/s^2)")]
        [DefaultValue(0.0)]
        public double RunDec { get; set; } = 0.000;

        // ===== Profile =====
        [Category("Profile"), DisplayName("Mode")]
        [DefaultValue(ProfileMode.SCurve)]
        public ProfileMode ProfileMode { get; set; } = ProfileMode.SCurve;

        [Category("Profile"), DisplayName("Accelerator Jerk (%)")]
        [DefaultValue(50)]
        public int AccJerkPercent { get; set; } = 50;

        [Category("Profile"), DisplayName("Decelerator Jerk (%)")]
        [DefaultValue(50)]
        public int DecJerkPercent { get; set; } = 50;

        // ===== 공통 운전 보정/품질 =====
        [Category("Operation"), DisplayName("Inposition Tolerance (mm)")]
        [DefaultValue(0.002)]
        public double InposTolerance { get; set; } = 0.002;

        [Category("Operation"), DisplayName("Logical Scale Factor")]
        [DefaultValue(1.0)]
        public double LogicalScaleFactor { get; set; } = 1.0000;

        [Category("Operation"), DisplayName("Offset (mm)")]
        [DefaultValue(0.0)]
        public double Offset { get; set; } = 0.000;

        [Category("Operation"), DisplayName("SensorLimit")]
        [DefaultValue(false)]
        public bool SensorLimitDic { get; set; } = false;

        [Category("Operation"), DisplayName("SensorLimit")]
        [DefaultValue(false)]
        public bool SensorLimitPlus { get; set; } = false;

        // ===== BaseConfig Hooks =====
        public override void Reset()
        {
            // 필요한 경우 복합 타입/구조 초기화 (현재는 전부 값형/문자열)
            if (LogicalScaleFactor == 0.0) LogicalScaleFactor = 1.0;
        }

        protected override void OnLoaded()
        {
            // 마이그레이션/보강
            if (AccJerkPercent < 0 || AccJerkPercent > 100) AccJerkPercent = 50;
            if (DecJerkPercent < 0 || DecJerkPercent > 100) DecJerkPercent = 50;
            if (InposTolerance < 0) InposTolerance = 0.0;
            if (LogicalScaleFactor == 0.0) LogicalScaleFactor = 1.0;
        }

        public override bool Validate()
        {
            if (AccJerkPercent < 0 || AccJerkPercent > 100)
                throw new ArgumentOutOfRangeException(nameof(AccJerkPercent));
            if (DecJerkPercent < 0 || DecJerkPercent > 100)
                throw new ArgumentOutOfRangeException(nameof(DecJerkPercent));
            if (InposTolerance < 0)
                throw new ArgumentOutOfRangeException(nameof(InposTolerance));
            return true;
        }

        public override string GetFilePath()
        {
            var dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configs", "MotionAxis");
            var file = string.IsNullOrWhiteSpace(Name) ? "default.json" : (Name + ".json");
            return Path.Combine(dir, file);
        }

        protected override JsonSerializerSettings GetJsonSettings()
        {
            // BaseConfig가 기본 세팅을 갖고 있으므로 필요 설정만 명시
            return new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Include,   // 기본값도 저장
                ObjectCreationHandling = ObjectCreationHandling.Replace
            };
        }

        // ===== 기존 정적 직렬화 API (호환용) =====
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
            DefaultValueHandling = DefaultValueHandling.Populate, // [DefaultValue] 적용
            ObjectCreationHandling = ObjectCreationHandling.Replace
        };

        public string ToJson(bool indented)
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

        public static MotionAxisConfig FromJson(string json)
        {
            var obj = JsonConvert.DeserializeObject<MotionAxisConfig>(json, ReadSettings);
            if (obj == null) throw new InvalidDataException("Invalid JSON for MotionAxisConfig.");
            obj.Validate();
            return obj;
        }

        public static MotionAxisConfig Load(string filePath)
        {
            var json = File.ReadAllText(filePath, Encoding.UTF8);
            return FromJson(json);
        }

        /// <summary>파일 없으면 생성, 있으면 로드 후 누락 필드를 기본값으로 채우고 즉시 저장(backfill)</summary>
        public static MotionAxisConfig LoadOrCreate(string filePath, bool indented = true, bool backfill = true)
        {
            MotionAxisConfig cfg;
            if (!File.Exists(filePath))
            {
                cfg = new MotionAxisConfig();
                cfg.Save(filePath, indented);
                return cfg;
            }

            var json = File.ReadAllText(filePath, Encoding.UTF8);
            cfg = FromJson(json);

            if (backfill)
                cfg.Save(filePath, indented);

            return cfg;
        }

        public static bool TryLoad(string filePath, out MotionAxisConfig result, out string error)
        {
            result = null;
            error = null;
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

        public bool TrySave(string filePath, out string error)
        {
            return TrySave(filePath, true, out error);
        }
    }
}
