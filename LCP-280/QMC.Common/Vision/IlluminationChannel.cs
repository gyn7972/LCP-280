using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QMC.Common
{
    [Serializable]
    public enum ParamIlluminationChannelKey
    {
        ChannelName,
        Channel,
        Min,
        Max,
        Value,
    }

    [Serializable]
    public class IlluminationChannel
    {
        public string ChannelName { set; get; }
        public int Channel { set; get; }
        public double Min { set; get; }
        public double Max { set; get; }
        public int Value { set; get; }
        public IlluminationChannel(string strChannelName)
        {
            ChannelName = strChannelName;
            Channel = 0;
            Min = 0;
            Max = 100;
            Value = 0;
        }

        public ParamGroup GetGroup()
        {
            ParamGroup group = new ParamGroup();
            group.Name = "IlluminationChannel";
            {
                Param param = new Param();
                param.SetParam("ChannelName", Param.DisplayTypeKey.Text, ChannelName, Param.ValueTypeKey.String, group.Name);
                group.AddParam(param);
            }
            {
                Param param = new Param();
                param.SetParam("Channel", Param.DisplayTypeKey.Text, Channel, Param.ValueTypeKey.Int, group.Name);
                group.AddParam(param);
            }
            {
                Param param = new Param();
                param.SetParam("Min", Param.DisplayTypeKey.Text, Min, Param.ValueTypeKey.Double, group.Name);
                group.AddParam(param);
            }
            {
                Param param = new Param();
                param.SetParam("Max", Param.DisplayTypeKey.Text, Max, Param.ValueTypeKey.Double, group.Name);
                group.AddParam(param);
            }
            {
                Param param = new Param();
                param.SetParam("Value", Param.DisplayTypeKey.Text, Value, Param.ValueTypeKey.Int, group.Name);
                group.AddParam(param);
            }

            return group;
        }

        public void SetGroup(ParamGroup group)
        {
            if (group != null)
            {
                Param param = null;
                param = group.GetParam((int)ParamIlluminationChannelKey.ChannelName);
                if (param != null)
                {
                    string value = string.Empty;
                    if (param.GetStringValue(ref value))
                    {
                        ChannelName = value;
                    }
                }
                param = group.GetParam((int)ParamIlluminationChannelKey.Channel);
                if (param != null)
                {
                    int value = 0;
                    if (param.GetIntValue(ref value))
                    {
                        Channel = value;
                    }
                }
                param = group.GetParam((int)ParamIlluminationChannelKey.Min);
                if (param != null)
                {
                    double value = 0;
                    if (param.GetDoubleValue(ref value))
                    {
                        Min = value;
                    }
                }
                param = group.GetParam((int)ParamIlluminationChannelKey.Max);
                if (param != null)
                {
                    double value = 0;
                    if (param.GetDoubleValue(ref value))
                    {
                        Max = value;
                    }
                }
                param = group.GetParam((int)ParamIlluminationChannelKey.Value);
                if (param != null)
                {
                    int value = 0;
                    if (param.GetIntValue(ref value))
                    {
                        Value = value;
                    }
                }
            }
        }
    }
}
