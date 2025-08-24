using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;

namespace QMC.Common.Cameras
{
    [Serializable]
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
    #region CameraConfig
    [Serializable]
    public class CameraConfig : BaseConfig
    {
        #region Define
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

        [Serializable]
        public enum ImageFlip
        {
            On,
            Off,
        }
        #endregion

        #region Constructor
        public CameraConfig()
        {
            this.Resolution = new Size(0, 0);
            this.AutoSleepEnable = false;

            this.AutoSleepLimitMin = TimeSpanInfo.FromMinutes(5);

            this.DelayAfterGrab = 0;
            this.DelayBeforeGrab = 0;

            this.GrabRetryCount = 1;
            this.SignalWatingTime = 300;

            this.PixelResolution = new SizeD();

            this.ImageFlipX = Camera.ImageFlip.Off;
            this.ImageFlipY = Camera.ImageFlip.Off;

            Scale = new VisionScale();
        }
        #endregion

        #region Property
        public string Name { set; get; }
        public object Tag { set; get; }
        public string LibraryType { set; get; }
        public VisionScale Scale { set; get; }
        public Size Resolution
        {
            set
            {
                CameraResolution = value;
                //if (ImageRotate == Camera.ImageRotateInfo.None)
                //{
                //    CameraResolution = value;
                //}
                //else
                //{
                //    CameraResolution = new Size(value.Height, value.Width);
                //}

            }
            get
            {
                return CameraResolution;
                //if (ImageRotate == Camera.ImageRotateInfo.None)
                //{
                //    return CameraResolution;
                //}
                //else
                //{
                //    return new Size(CameraResolution.Height, CameraResolution.Width);
                //}

            }
        }

        public bool AutoSleepEnable
        {
            get;
            set;
        }

        public bool EnableExposure { set; get; }

        public bool SuspendedImageDisplay
        {
            get;
            set;
        }
        #endregion

        #region ConstructConfiguration
        public TimeSpanInfo AutoSleepLimitMin
        {
            get;
            set;
        }
        public int DelayBeforeGrab { set; get; }
        public int DelayAfterGrab { set; get; }
        public int GrabRetryCount { set; get; }
        public Size CameraResolution
        {
            get;
            set;
        }
        public int SignalWatingTime
        {
            get;
            set;
        }
        public SizeD PixelResolution { set; get; }
        public Camera.ImageFlip ImageFlipX
        {
            get;
            set;
        }
        public Camera.ImageFlip ImageFlipY
        {
            get;
            set;
        }
        public Camera.ImageRotateInfo ImageRotate
        {
            get;
            set;
        }
        //[Category("Camera")]
        //public double MaxFrameRate
        //{
        //    get;
        //    set;
        //}
        public TimeSpanInfo WaitToGrabTimeout { set; get; }
        public bool UseCutImage { set; get; }
        public uint CutImageWidth { set; get; }
        public uint CutImageHeight { set; get; }
        #endregion

        public void Init()
        {
            if (Scale == null)
            {
                Scale = new VisionScale();
            }
        }

        //#region ListParam        
        //public override ListParam ToListParam()
        //{
        //    ListParam list = new ListParam();
        //    ParamGroup group = new ParamGroup();
        //    group.Name = "Common";
        //    {
        //        Param param = new Param();
        //        param.SetParam(nameof(Resolution), Param.DisplayTypeKey.Text, Resolution, Param.ValueTypeKey.Size, group.Name);

        //        group.AddParam(param);
        //    }

        //    {
        //        Param param = new Param();
        //        param.SetParam(nameof(AutoSleepEnable), Param.DisplayTypeKey.CheckBox, AutoSleepEnable, Param.ValueTypeKey.Bool, group.Name);

        //        group.AddParam(param);
        //    }
        //    {
        //        Param param = new Param();
        //        param.SetParam(nameof(EnableExposure), Param.DisplayTypeKey.CheckBox, EnableExposure, Param.ValueTypeKey.Bool, group.Name);

        //        group.AddParam(param);
        //    }
        //    {
        //        Param param = new Param();
        //        param.SetParam(nameof(SuspendedImageDisplay), Param.DisplayTypeKey.CheckBox, SuspendedImageDisplay, Param.ValueTypeKey.Bool, group.Name);

        //        group.AddParam(param);
        //    }
        //    {
        //        Param param = new Param();
        //        param.SetParam(nameof(AutoSleepLimitMin), Param.DisplayTypeKey.Text, AutoSleepLimitMin, Param.ValueTypeKey.TimeSpanInfo, group.Name);


        //        group.AddParam(param);
        //    }

        //    {
        //        Param param = new Param();
        //        param.SetParam(nameof(DelayBeforeGrab), Param.DisplayTypeKey.Text, DelayBeforeGrab, Param.ValueTypeKey.Int, group.Name);


        //        group.AddParam(param);
        //    }
        //    {
        //        Param param = new Param();
        //        param.SetParam(nameof(DelayAfterGrab), Param.DisplayTypeKey.Text, DelayAfterGrab, Param.ValueTypeKey.Int, group.Name);


        //        group.AddParam(param);
        //    }
        //    {
        //        Param param = new Param();
        //        param.SetParam(nameof(GrabRetryCount), Param.DisplayTypeKey.Text, GrabRetryCount, Param.ValueTypeKey.Int, group.Name);


        //        group.AddParam(param);
        //    }
        //    {
        //        Param param = new Param();
        //        param.SetParam(nameof(SignalWatingTime), Param.DisplayTypeKey.Text, SignalWatingTime, Param.ValueTypeKey.Int, group.Name);


        //        group.AddParam(param);
        //    }
        //    {
        //        Param param = new Param();
        //        param.SetParam(nameof(PixelResolution), Param.DisplayTypeKey.Text, PixelResolution, Param.ValueTypeKey.SizeD, group.Name);

        //        group.AddParam(param);
        //    }
        //    {
        //        Param param = new Param();
        //        param.SetParam(nameof(ImageFlipX), Param.DisplayTypeKey.Combobox, ImageFlipX, Param.ValueTypeKey.Int, group.Name);

        //        param.SelectValues.Clear();
        //        foreach (Enum e in Enum.GetValues(typeof(ImageFlip)))
        //        {
        //            param.SelectValues.Add(e.ToString());
        //        }
        //        group.AddParam(param);
        //    }
        //    {
        //        Param param = new Param();
        //        param.SetParam(nameof(ImageFlipY), Param.DisplayTypeKey.Combobox, ImageFlipY, Param.ValueTypeKey.Int, group.Name);

        //        param.SelectValues.Clear();
        //        foreach (Enum e in Enum.GetValues(typeof(ImageFlip)))
        //        {
        //            param.SelectValues.Add(e.ToString());
        //        }
        //        group.AddParam(param);
        //    }
        //    {
        //        Param param = new Param();
        //        param.SetParam(nameof(ImageRotate), Param.DisplayTypeKey.Combobox, ImageRotate, Param.ValueTypeKey.Int, group.Name);

        //        param.SelectValues.Clear();
        //        foreach (Enum e in Enum.GetValues(typeof(Camera.ImageRotateInfo)))
        //        {
        //            param.SelectValues.Add(e.ToString());
        //        }
        //        group.AddParam(param);
        //    }

        //    {
        //        Param param = new Param();
        //        param.SetParam(nameof(UseCutImage), Param.DisplayTypeKey.CheckBox, UseCutImage, Param.ValueTypeKey.Bool, group.Name);

        //        group.AddParam(param);
        //    }
        //    {
        //        Param param = new Param();
        //        param.SetParam(nameof(CutImageWidth), Param.DisplayTypeKey.Text, CutImageWidth, Param.ValueTypeKey.Int, group.Name);

        //        group.AddParam(param);
        //    }
        //    {                
        //        group.AddGroup(Scale.GetGroup());
        //    }
        //    //{
        //    //    Param param = new Param();
        //    //    param.SetParam("Scale X", Param.DisplayTypeKey.Text, Scale.X, Param.ValueTypeKey.Double, group.Name);

        //    //    group.AddParam(param);
        //    //}
        //    //{
        //    //    Param param = new Param();
        //    //    param.SetParam("Scale Y", Param.DisplayTypeKey.Text, Scale.Y, Param.ValueTypeKey.Double, group.Name);

        //    //    group.AddParam(param);
        //    //}
        //    //{
        //    //    Param param = new Param();
        //    //    param.SetParam("Scale InvetedX", Param.DisplayTypeKey.CheckBox, Scale.InvertedX, Param.ValueTypeKey.Bool, group.Name);

        //    //    group.AddParam(param);
        //    //}
        //    //{
        //    //    Param param = new Param();
        //    //    param.SetParam("Scale InvetedY", Param.DisplayTypeKey.CheckBox, Scale.InvertedY, Param.ValueTypeKey.Bool, group.Name);

        //    //    group.AddParam(param);
        //    //}


        //    list.SetGroup(group);
        //    return list;
        //}
        //public override void SetParam(ListParam listParam)
        //{
        //    ParamGroup group = listParam.GetGroup("Common");
        //    if (group != null)
        //    {
        //        Param param = null;
        //        param = group.GetParam((int)ParamCameraConfigKey.Resolution);
        //        if (param != null)
        //        {
        //            Size value = new Size();
        //            if (param.GetSizeValue(ref value))
        //            {
        //                Resolution = value;
        //            }
        //        }
        //        param = group.GetParam((int)ParamCameraConfigKey.AutoSleepEnable);
        //        if (param != null)
        //        {
        //            bool value = false;
        //            if (param.GetBoolValue(ref value))
        //            {
        //                AutoSleepEnable = value;
        //            }
        //        }
        //        param = group.GetParam((int)ParamCameraConfigKey.EnableExposure);
        //        if (param != null)
        //        {
        //            bool value = false;
        //            if (param.GetBoolValue(ref value))
        //            {
        //                EnableExposure = value;
        //            }
        //        }
        //        param = group.GetParam((int)ParamCameraConfigKey.SuspendedImageDisplay);
        //        if (param != null)
        //        {
        //            bool value = false;
        //            if (param.GetBoolValue(ref value))
        //            {
        //                SuspendedImageDisplay = value;
        //            }
        //        }
        //        param = group.GetParam((int)ParamCameraConfigKey.AutoSleepLimitMin);
        //        if (param != null)
        //        {
        //            string value = String.Empty;
        //            if (param.GetTimeSpanInfoValue(ref value))
        //            {
        //                TimeSpanInfo valueInfo = new TimeSpanInfo();
        //                TimeSpanInfo.TryParse(value, out valueInfo);
        //                AutoSleepLimitMin = valueInfo;
        //            }
        //        }
        //        param = group.GetParam((int)ParamCameraConfigKey.DelayBeforeGrab);
        //        if (param != null)
        //        {
        //            int value = 0;
        //            if (param.GetIntValue(ref value))
        //            {
        //                DelayBeforeGrab = value;
        //            }
        //        }
        //        param = group.GetParam((int)ParamCameraConfigKey.DelayAfterGrab);
        //        if (param != null)
        //        {
        //            int value = 0;
        //            if (param.GetIntValue(ref value))
        //            {
        //                DelayAfterGrab = value;
        //            }
        //        }

        //        param = group.GetParam((int)ParamCameraConfigKey.GrabRetryCount);
        //        if (param != null)
        //        {
        //            int value = 0;
        //            if (param.GetIntValue(ref value))
        //            {
        //                GrabRetryCount = value;
        //            }
        //        }
        //        param = group.GetParam((int)ParamCameraConfigKey.SignalWatingTime);
        //        if (param != null)
        //        {
        //            int value = 0;
        //            if (param.GetIntValue(ref value))
        //            {
        //                SignalWatingTime = value;
        //            }
        //        }
        //        param = group.GetParam((int)ParamCameraConfigKey.PixelResolution);
        //        if (param != null)
        //        {
        //            SizeD value = new SizeD();
        //            if (param.GetSizeDValue(ref value))
        //            {
        //                PixelResolution = value; // 이거 이러면될거같은데?
        //            }
        //        }
        //        param = group.GetParam((int)ParamCameraConfigKey.ImageFlipX);
        //        if (param != null)
        //        {
        //            int value = 0;
        //            if (param.GetIntValue(ref value))
        //            {
        //                ImageFlipX = (Camera.ImageFlip)value;
        //            }
        //        }

        //        param = group.GetParam((int)ParamCameraConfigKey.ImageFlipY);
        //        if (param != null)
        //        {
        //            int value = 0;
        //            if (param.GetIntValue(ref value))
        //            {
        //                ImageFlipY = (Camera.ImageFlip)value;
        //            }
        //        }
        //        param = group.GetParam((int)ParamCameraConfigKey.ImageRotate);
        //        if (param != null)
        //        {
        //            int value = 0;
        //            if (param.GetIntValue(ref value))
        //            {
        //                ImageRotate = (Camera.ImageRotateInfo)value;
        //            }
        //        }
        //        param = group.GetParam((int)ParamCameraConfigKey.UseCutImage);
        //        if (param != null)
        //        {
        //            bool value = false;
        //            if (param.GetBoolValue(ref value))
        //            {
        //                UseCutImage = value;
        //            }
        //        }

        //        param = group.GetParam((int)ParamCameraConfigKey.CutImageWidth);
        //        if (param != null)
        //        {
        //            int value = 0;
        //            if (param.GetIntValue(ref value))
        //            {
        //                CutImageWidth = (uint)value;
        //            }
        //        }
        //        param = group.GetParam((int)ParamCameraConfigKey.CutImageHeight);
        //        if (param != null)
        //        {
        //            int value = 0;
        //            if (param.GetIntValue(ref value))
        //            {
        //                CutImageHeight = (uint)value;
        //            }
        //        }

        //        {
        //            ParamGroup scaleGroup = group.GetGroup(Scale.GetType().Name);
        //            if (scaleGroup != null)
        //            {
        //                Scale.SetGroup(scaleGroup);
        //            }

        //        }
        //        //param = group.GetParam((int)ParamCameraConfigKey.ScaleX);
        //        //if (param != null)
        //        //{
        //        //    double value = 0;
        //        //    if (param.GetDoubleValue(ref value))
        //        //    {
        //        //        Scale.X = (double)value;
        //        //    }
        //        //}
        //        //param = group.GetParam((int)ParamCameraConfigKey.ScaleY);
        //        //if (param != null)
        //        //{
        //        //    double value = 0;
        //        //    if (param.GetDoubleValue(ref value))
        //        //    {
        //        //        Scale.Y = (double)value;
        //        //    }
        //        //}
        //        //param = group.GetParam((int)ParamCameraConfigKey.InvertedX);
        //        //if (param != null)
        //        //{
        //        //    bool value = false;
        //        //    if (param.GetBoolValue(ref value))
        //        //    {
        //        //        Scale.InvertedX = value;
        //        //    }
        //        //}
        //        //param = group.GetParam((int)ParamCameraConfigKey.InvertedY);
        //        //if (param != null)
        //        //{
        //        //    bool value = false;
        //        //    if (param.GetBoolValue(ref value))
        //        //    {
        //        //        Scale.InvertedY = value;
        //        //    }
        //        //}
        //    }

        //}

        //#endregion
        //public override List<object> GetPositions()
        //{
        //    return null;
        //}
    }


}
#endregion