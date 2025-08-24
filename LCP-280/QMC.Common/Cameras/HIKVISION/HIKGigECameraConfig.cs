using QMC.Common.Cameras;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QMC.Common.HIKVISION
{
    [Serializable]
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
    [TypeConverter(typeof(HIKGigECameraConfig))]
    public class HIKGigECameraConfig : CameraConfig       
    {
        #region Define
      
        #endregion

        #region Property
        public string SerialNumber { get; set; }
        public float ExposureTime { get; set; }
        public float Gain { get; set; }
        public int OpenDelayTime { get; set; }
        public int RetryCount { get; set; }

        public uint OffsetX { set; get; }
        public uint OffsetY { set; get; }

        public CameraType CameraType { set; get; }
        #endregion

        public HIKGigECameraConfig()
            :base()
        {
            this.SerialNumber = "";
            this.ExposureTime = 1000.0f;
            this.Gain = 1.0f;
            this.OpenDelayTime = 1000;
            this.RetryCount = 5;
            OffsetX = 0;
            OffsetY = 0;
            CameraType = CameraType.Normal;
        }

        //public override ListParam ToListParam()
        //{
        //    ListParam listParam = new ListParam();
        //    ParamGroup group = new ParamGroup();
        //    group.Name = this.GetType().Name;
        //    {
        //        Param param = new Param();
        //        param.SetParam(nameof(SerialNumber), Param.DisplayTypeKey.Text, SerialNumber, Param.ValueTypeKey.String, group.Name);

        //        group.AddParam(param);
        //    }
        //    {
        //        Param param = new Param();
        //        param.SetParam(nameof(ExposureTime), Param.DisplayTypeKey.Text, ExposureTime, Param.ValueTypeKey.Double, group.Name);

        //        group.AddParam(param);
        //    }
        //    {
        //        Param param = new Param();
        //        param.SetParam(nameof(Gain), Param.DisplayTypeKey.Text, Gain, Param.ValueTypeKey.Double, group.Name);

        //        group.AddParam(param);
        //    }
        //    {
        //        Param param = new Param();
        //        param.SetParam(nameof(OpenDelayTime), Param.DisplayTypeKey.Text, OpenDelayTime, Param.ValueTypeKey.Int, group.Name);

        //        group.AddParam(param);
        //    }
        //    {
        //        Param param = new Param();
        //        param.SetParam(nameof(RetryCount), Param.DisplayTypeKey.Text, RetryCount, Param.ValueTypeKey.Int, group.Name);

        //        group.AddParam(param);
        //    }

        //    {
        //        Param param = new Param();
        //        param.SetParam(nameof(OffsetX), Param.DisplayTypeKey.Text, OffsetX, Param.ValueTypeKey.Int, group.Name);

        //        group.AddParam(param);
        //    }

        //    {
        //        Param param = new Param();
        //        param.SetParam(nameof(OffsetY), Param.DisplayTypeKey.Text, OffsetY, Param.ValueTypeKey.Int, group.Name);

        //        group.AddParam(param);
        //    }
        //    {
        //        Param param = new Param();
        //        param.SetParam(nameof(CameraType), Param.DisplayTypeKey.Combobox, CameraType, Param.ValueTypeKey.Int, group.Name);

        //        param.SelectValues.Clear();
        //        foreach (CameraType eType in Enum.GetValues(typeof(CameraType)))
        //        {
        //            param.SelectValues.Add(eType.ToString());
        //        }
        //        group.AddParam(param);
        //    }
        //    {
        //        group.SetGroup(base.ToListParam().GetGroup("Common"));
        //    }

        //    listParam.SetGroup(group);
        //    return listParam;
        //}

        //public override void SetParam(ListParam listParam)
        //{
        //    ParamGroup group = listParam.GetGroup(this.GetType().Name);            
        //    ListParam commonList = new ListParam();
        //    commonList.SetGroup(group.GetGroup("Common"));
        //    base.SetParam(commonList);
        //    if (group != null)
        //    {
        //        Param param = null;
        //        param = group.GetParam((int)ParamHIKGigECameraConfigKey.SerialNumber);
        //        if (param != null)
        //        {
        //            string value = string.Empty;
        //            if (param.GetStringValue(ref value))
        //            {
        //                SerialNumber = (string)value;
        //            }
        //        }
        //        param = group.GetParam((int)ParamHIKGigECameraConfigKey.ExposureTime);
        //        if (param != null)
        //        {
        //            double value = 0;
        //            if (param.GetDoubleValue(ref value))
        //            {
        //                ExposureTime = (float)value;
        //            }
        //        }
        //        param = group.GetParam((int)ParamHIKGigECameraConfigKey.Gain);
        //        if (param != null)
        //        {
        //            double value = 0;
        //            if (param.GetDoubleValue(ref value))
        //            {
        //                Gain = (float)value;
        //            }
        //        }
        //        param = group.GetParam((int)ParamHIKGigECameraConfigKey.OpenDelayTime);
        //        if (param != null)
        //        {
        //            int value = 0;
        //            if (param.GetIntValue(ref value))
        //            {
        //                OpenDelayTime = (int)value;
        //            }
        //        }
        //        param = group.GetParam((int)ParamHIKGigECameraConfigKey.RetryCount);
        //        if (param != null)
        //        {
        //            int value = 0;
        //            if (param.GetIntValue(ref value))
        //            {
        //                RetryCount = (int)value;
        //            }
        //        }
        //        param = group.GetParam((int)ParamHIKGigECameraConfigKey.OffsetX);
        //        if (param != null)
        //        {
        //            int value = 0;
        //            if (param.GetIntValue(ref value))
        //            {
        //                OffsetX = (uint)value;
        //            }
        //        }

        //        param = group.GetParam((int)ParamHIKGigECameraConfigKey.OffsetY);
        //        if (param != null)
        //        {
        //            int value = 0;
        //            if (param.GetIntValue(ref value))
        //            {
        //                OffsetY = (uint)value;
        //            }
        //        }

        //        param = group.GetParam((int)ParamHIKGigECameraConfigKey.CameraType);
        //        if (param != null)
        //        {
        //            int value = 0;
        //            if (param.GetIntValue(ref value))
        //            {
        //                CameraType = (CameraType)value;
        //            }
        //        }
        //    }

        //}

    }
    public class HIKGigECameraConfigCollection : Collection<HIKGigECameraConfig>
    {
        public HIKGigECameraConfigCollection()
        {

        }
    }
   
}
