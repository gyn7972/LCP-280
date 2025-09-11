using QMC.Common.Cameras;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;

namespace QMC.Common.HIKVISION
{
    [Serializable]
    [Obsolete("Use strongly-typed properties instead of ParamHIKGigECameraConfigKey.")]
    public enum ParamHIKGigECameraConfigKey
    {
        SerialNumber,
        ExposureTime,
        Gain,
        OpenDelayTime,
        RetryCount,
        OffsetX,
        OffsetY,
        CameraType,
    }

    [Serializable]
    public enum CameraType
    {
        Normal,
        HighResolution
    }

    [Serializable]
    //[TypeConverter(typeof(ExpandableObjectConverter))] // PropertyGrid 용
    public class HIKGigECameraConfig : CameraConfig
    {
        #region Ctors
        public HIKGigECameraConfig() : base() { }
        public HIKGigECameraConfig(string name) : base(name) { }
        #endregion

        #region Properties
        [DefaultValue("")]
        public string SerialNumber { get; set; }

        [DefaultValue(5000.0f)]
        public float ExposureTime { get; set; }

        [DefaultValue(1.0f)]
        public float Gain { get; set; }

        [DefaultValue(1000)]
        public int OpenDelayTime { get; set; }

        [DefaultValue(5)]
        public int RetryCount { get; set; }

        [DefaultValue(typeof(uint), "0")]
        public uint OffsetX { get; set; }

        [DefaultValue(typeof(uint), "0")]
        public uint OffsetY { get; set; }

        [DefaultValue(typeof(CameraType), "Normal")]
        public CameraType CameraType { get; set; }
        #endregion

        #region BaseConfig Hooks
        private static bool IsDefault<T>(T v)
        {
            return System.Collections.Generic.EqualityComparer<T>.Default.Equals(v, default(T));
        }

        public override void Reset()
        {
            base.Reset();

            if (SerialNumber == null) SerialNumber = "";
            if (ExposureTime <= 0f) ExposureTime = 5000.0f;
            if (Gain <= 0f) Gain = 1.0f;

            if (OpenDelayTime < 0) OpenDelayTime = 5000;
            if (RetryCount <= 0) RetryCount = 5;

            // OffsetX/OffsetY는 기본 0 유지
            if (!Enum.IsDefined(typeof(CameraType), CameraType))
                CameraType = CameraType.Normal;
        }

        protected override void OnLoaded()
        {
            base.OnLoaded();

            // 파일에서 누락/잘못 저장된 값 보정
            if (SerialNumber == null) SerialNumber = "";
            ExposureTime = 5000.0f;
            //if (ExposureTime <= 0f) ExposureTime = 5000.0f;
            if (Gain <= 0f) Gain = 1.0f;

            if (OpenDelayTime < 0) OpenDelayTime = 5000;
            if (RetryCount <= 0) RetryCount = 5;

            if (!Enum.IsDefined(typeof(CameraType), CameraType))
                CameraType = CameraType.Normal;
        }

        public override bool Validate()
        {
            // 파일에서 누락/잘못 저장된 값 보정
            if (SerialNumber == null) SerialNumber = "";
            if (ExposureTime <= 0f) ExposureTime = 5000.0f;
            if (Gain <= 0f) Gain = 1.0f;

            if (OpenDelayTime < 0) OpenDelayTime = 5000;
            if (RetryCount <= 0) RetryCount = 5;

            if (!Enum.IsDefined(typeof(CameraType), CameraType))
                CameraType = CameraType.Normal;
            return true;
        }

        public override string GetFilePath()
        {
            // 파생 구성 파일은 분리된 폴더에 저장
            var dir = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configs", "Camera", "HIKGigE");
            var file = string.IsNullOrWhiteSpace(Name) ? "default.json" : (Name + ".json");
            return System.IO.Path.Combine(dir, file);
        }

        protected override void OnSaving()
        {
            // Name이 비었으면 SerialNumber로 대체 저장(파일명 구분을 위해)
            if (string.IsNullOrWhiteSpace(Name) && !string.IsNullOrWhiteSpace(SerialNumber))
                Name = SerialNumber;

            base.OnSaving();
        }
        #endregion

        #region Static Helper (편의 API)
        public static HIKGigECameraConfig LoadOrCreate(string name)
        {
            //var cfg = new HIKGigECameraConfig(name);
            //var ret = cfg.Load();
            //if (ret != 0)
            //{
            //    cfg.Reset();
            //    cfg.Save();
            //}
            //return cfg;

            var temp = new HIKGigECameraConfig(name);
            var path = temp.GetFilePath();

            // 레거시(문자열 한 줄) 포맷이면 백업 후 삭제
            if (System.IO.File.Exists(path))
            {
                var text = System.IO.File.ReadAllText(path).Trim();
                if (text.Length > 0 && text[0] == '"' && text.IndexOf('{') == -1)
                {
                    try { System.IO.File.Copy(path, path + ".legacy", true); } catch { }
                    try { System.IO.File.Delete(path); } catch { }
                }
            }

            // 여기 확인 필요하다.
            var cfg = new HIKGigECameraConfig(name);
            var ret = cfg.Load();
            if (ret != 0)
            {
                cfg.Reset();
                cfg.Save();
            }
            return cfg;
        }

        // 기존 호출 호환 (indented/backfill 매개변수 버전이 남아있을 수 있음)
        public static HIKGigECameraConfig LoadOrCreate(string name, bool indented)
        {
            return LoadOrCreate(name);
        }
        public static HIKGigECameraConfig LoadOrCreate(string name, bool indented, bool backfill)
        {
            return LoadOrCreate(name);
        }

        #endregion
    }

    public class HIKGigECameraConfigCollection : Collection<HIKGigECameraConfig>
    {
        public HIKGigECameraConfigCollection() { }
    }
}
