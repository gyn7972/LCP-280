using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace QMC.Common.Cameras
{
    // 🔸 기존 Key-Value 방식 중단: 변수(프로퍼티) 기반으로 고정
    [Obsolete("Use CameraConfig's strongly-typed properties instead of ParamCameraConfigKey.")]
    public enum ParamCameraConfigKey
    {
        Resolution,
        AutoSleepEnable,
        EnableExposure,
        SuspendedImageDisplay,
        AutoSleepLimitMin,
        DelayBeforeGrab,
        DelayAfterGrab,
        GrabRetryCount,
        SignalWatingTime,
        PixelResolution,
        ImageFlipX,
        ImageFlipY,
        ImageRotate,
        UseCutImage,
        CutImageWidth,
        CutImageHeight,
        ScaleX,
        ScaleY,
        InvertedX,
        InvertedY,
    }

    [Serializable]
    public class CameraConfig : BaseConfig
    {
        #region Define
        [Serializable]
        public enum AlarmKeys
        {
            OpenFailed,
            CloseFailed,
            CreateFailed,
            PrepareFailed,
            InitializeFailed,
            TerminateFailed,
            GrabFailed,
            StartLiveFailed,
            StopLiveFailed,
            NotOpened,
            GetFrameRateFailed,
            SetFrameRateFailed,
            SendCommandFailed,
            ReciveCommandFailed,
        }

        [Serializable]
        public enum BitPerPixelInfo
        {
            Gray8bpp = 8,
            Color24bpp = 24,
            Color32bpp = 32,
        }
        #endregion

        #region Ctors
        public CameraConfig() : base() { }
        public CameraConfig(string name) : base(name) { }
        #endregion

        #region Property (변수 기반)
        // ⚠️ Name은 BaseConfig에 이미 있음. (여기서 재선언 금지)
        public object Tag { get; set; }
        public string LibraryType { get; set; }

        public VisionScale Scale { get; set; }

        [DefaultValue(typeof(Size), "0,0")]
        public double ScaleX {             
            get 
            { 
                return Scale?.X ?? 0.0; 
            }
            set
            {
                if (Scale == null) Scale = new VisionScale();
                Scale.X = value;
            }
        }

        [DefaultValue(typeof(Size), "0,0")]
        public double ScaleY
        {
            get
            {
                return Scale?.Y ?? 0.0;
            }
            set
            {
                if (Scale == null) Scale = new VisionScale();
                Scale.Y = value;
            }
        }


        [DefaultValue(typeof(Size), "0,0")]
        public Size Resolution
        {
            set { CameraResolution = value; }
            get { return CameraResolution; }
        }

        [DefaultValue(false)] public bool AutoSleepEnable { get; set; }
        [DefaultValue(false)] public bool EnableExposure { get; set; }
        [DefaultValue(false)] public bool SuspendedImageDisplay { get; set; }

        [DefaultValue("")] public string SerialNo { get; set; }
        [DefaultValue("")] public string Ip { get; set; }
        [DefaultValue("")] public string Mac { get; set; }
        #endregion

        #region ConstructConfiguration (실제 저장 대상)
        [DefaultValue(null)] public TimeSpanInfo AutoSleepLimitMin { get; set; }

        [DefaultValue(0)] public int DelayBeforeGrab { get; set; }
        [DefaultValue(0)] public int DelayAfterGrab { get; set; }

        [DefaultValue(1)] public int GrabRetryCount { get; set; }

        [DefaultValue(typeof(Size), "0,0")]
        public Size CameraResolution { get; set; }

        [DefaultValue(300)] public int SignalWatingTime { get; set; }

        [DefaultValue(null)] public SizeD PixelResolution { get; set; }

        [DefaultValue(typeof(Camera.ImageFlip), "Off")]
        public Camera.ImageFlip ImageFlipX { get; set; }

        [DefaultValue(typeof(Camera.ImageFlip), "Off")]
        public Camera.ImageFlip ImageFlipY { get; set; }

        public Camera.ImageRotateInfo ImageRotate { get; set; }

        [DefaultValue(null)] public TimeSpanInfo WaitToGrabTimeout { get; set; }

        [DefaultValue(false)] public bool UseCutImage { get; set; }

        [DefaultValue((uint)0)] public uint CutImageWidth { get; set; }
        [DefaultValue((uint)0)] public uint CutImageHeight { get; set; }
        #endregion

        #region BaseConfig Hooks (초기화/보강/검증)
        /// <summary>기본값/구조 초기화(복합 타입 new 등)</summary>
        public override void Reset()
        {
            // C# 7.3: ??= 사용 금지 → if (x == null) 패턴
            if (Scale == null) Scale = new VisionScale();

            // 값형/참조형 모두 안전하게 체크
            if (IsDefault(AutoSleepLimitMin))
                AutoSleepLimitMin = TimeSpanInfo.FromMinutes(5);

            if (GrabRetryCount <= 0)
                GrabRetryCount = 1;

            if (SignalWatingTime <= 0)
                SignalWatingTime = 300;

            // SizeD가 class/struct 어느 쪽이든 안전
            if (IsDefault(PixelResolution))
                PixelResolution = new SizeD();

            ImageFlipX = Camera.ImageFlip.Off;
            ImageFlipY = Camera.ImageFlip.Off;

            if (CameraResolution.Width == 0 && CameraResolution.Height == 0)
                CameraResolution = new Size(0, 0);
        }

        protected override void OnLoaded()
        {
            if (Scale == null) Scale = new VisionScale();

            if (IsDefault(AutoSleepLimitMin))
                AutoSleepLimitMin = TimeSpanInfo.FromMinutes(5);

            if (GrabRetryCount <= 0)
                GrabRetryCount = 1;

            if (SignalWatingTime <= 0)
                SignalWatingTime = 300;

            if (IsDefault(PixelResolution))
                PixelResolution = new SizeD();
        }

        /// <summary>저장 전 유효성 검사</summary>
        public override bool Validate()
        {
            if (GrabRetryCount < 0) GrabRetryCount = 0;
            if (SignalWatingTime < 0) SignalWatingTime = 0;
            if (CutImageWidth > 0 || CutImageHeight > 0)
                UseCutImage = true;
            return true;
        }
        #endregion

        #region 파일 경로/JSON 설정
        public override string GetFilePath()
        {
            var dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configs", "Camera");
            var file = string.IsNullOrWhiteSpace(Name) ? "default.json" : $"{Name}.json";
            return Path.Combine(dir, file);
        }

        /// <summary>카메라 전용 컨버터/설정</summary>
        protected override JsonSerializerSettings GetJsonSettings()
        {
            return new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Include,   // 기본값 포함 저장
                ObjectCreationHandling = ObjectCreationHandling.Replace,
                TypeNameHandling = TypeNameHandling.Auto,              // 파생 타입 복원
                Converters =
                {
                    new StringEnumConverter(),
                    new TimeSpanInfoJsonConverter(),
                    new SizeDJsonConverter(),
                },
                Error = (sender, args) =>
                {
                    // 필요시 로깅만 하고 계속
                    args.ErrorContext.Handled = true;
                }
            };
        }
        #endregion

        #region Static Helper (편의 API)
        /// <summary>
        /// 파일 없으면 생성, 있으면 로드. 로드 중 누락 보강이 있으면 자동 저장됨(BaseConfig 로직).
        /// </summary>
        public static CameraConfig LoadOrCreate(string cameraName)
        {
            var cfg = new CameraConfig(cameraName);
            var ret = cfg.Load();     // 없으면 -1
            if (ret != 0)
            {
                cfg.Reset();          // 구조/기본값 세팅
                cfg.Save();           // 최초 생성
            }
            return cfg;
        }

        private static bool IsDefault<T>(T value)
        {
            return EqualityComparer<T>.Default.Equals(value, default(T));
        }


        #endregion

        #region Converters
        public class TimeSpanInfoJsonConverter : JsonConverter
        {
            public override bool CanConvert(Type objectType) => objectType.Name == "TimeSpanInfo";

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                writer.WriteValue(value?.ToString());
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                var s = reader.Value as string;
                if (string.IsNullOrEmpty(s)) return null;

                var tryParse = objectType.GetMethod("TryParse", BindingFlags.Public | BindingFlags.Static);
                if (tryParse != null)
                {
                    object[] args = new object[] { s, null };
                    var ok = (bool)tryParse.Invoke(null, args);
                    if (ok) return args[1];
                }

                var fromMinutes = objectType.GetMethod("FromMinutes", BindingFlags.Public | BindingFlags.Static);
                if (fromMinutes != null)
                    return fromMinutes.Invoke(null, new object[] { 0.0 });

                return null;
            }
        }

        public class SizeDJsonConverter : JsonConverter
        {
            public override bool CanConvert(Type objectType) => objectType.Name == "SizeD";

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                if (value == null) { writer.WriteNull(); return; }

                var t = value.GetType();
                var pW = t.GetProperty("Width") ?? t.GetProperty("X");
                var pH = t.GetProperty("Height") ?? t.GetProperty("Y");
                double w = pW != null ? Convert.ToDouble(pW.GetValue(value)) : 0.0;
                double h = pH != null ? Convert.ToDouble(pH.GetValue(value)) : 0.0;

                writer.WriteStartObject();
                writer.WritePropertyName("Width"); writer.WriteValue(w);
                writer.WritePropertyName("Height"); writer.WriteValue(h);
                writer.WriteEndObject();
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                if (reader.TokenType == JsonToken.Null) return null;

                var jo = JObject.Load(reader);
                double w = jo["Width"]?.ToObject<double>() ?? jo["X"]?.ToObject<double>() ?? 0.0;
                double h = jo["Height"]?.ToObject<double>() ?? jo["Y"]?.ToObject<double>() ?? 0.0;

                var ctor = objectType.GetConstructor(new[] { typeof(double), typeof(double) });
                if (ctor != null) return ctor.Invoke(new object[] { w, h });

                var inst = Activator.CreateInstance(objectType);
                var pW = objectType.GetProperty("Width") ?? objectType.GetProperty("X");
                var pH = objectType.GetProperty("Height") ?? objectType.GetProperty("Y");
                pW?.SetValue(inst, w);
                pH?.SetValue(inst, h);
                return inst;
            }
        }
        #endregion
    }
}
