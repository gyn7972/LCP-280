using QMC.Common.Vision;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace QMC.Common
{
    [Serializable]
    public enum ParameterType
    {
        Value,
        Position,
        Velocity,
        Acceleration,
        Deceleration,
        IO,
        Time,
        Image,
    }
    [Serializable]
    public enum DataType
    {
        Double,
        Bool,
        Long,
        String,
        Image,
    }
    [Serializable]
    public class SettingParameter
    {
        private ParameterType m_ParameterType;
        private DataType m_Type;
        private double m_dValue;
        private bool m_bValue;
        private long m_lValue;
        private string m_strValue;
        private bool m_bIsRecipe;
        private double m_dMinValue;
        private double m_dMaxValue;

        public SettingParameter() : this("", DataType.Double, ParameterType.Position, null)
        {
        }

        public SettingParameter(string strName, DataType type) : this(strName, type, ParameterType.Position, null)
        {
        }

        public SettingParameter(string strName, DataType type, ParameterType parameterType, IActor owner)
        {
            Owner = owner;
            Name = strName;
            m_Type = type;
            m_ParameterType = parameterType;
            m_bIsRecipe = false;
            Image = new VisionImage();
        }

      //  [Browsable(false)]
        public IActor Owner { set; get; }


        [Browsable(false)]
        public double DoubleValue
        {
            set
            {
                m_dValue = value;
                Type = DataType.Double;
            }
            get
            {
                return m_dValue;
            }
        }
        [Browsable(false)]
        public bool BoolValue
        {
            set
            {
                m_bValue = value;
                Type = DataType.Bool;
            }
            get
            {
                return m_bValue;
            }
        }
        [Browsable(false)]
        public long LongValue
        {
            set
            {
                m_lValue = value;
                Type = DataType.Long;
            }
            get
            {
                return m_lValue;
            }
        }



        public DataType Type
        {
            set
            {
                m_Type = value;
            }
            get
            {
                return m_Type;
            }
        }
        [Browsable(false)]
        public string StringValue
        {
            set
            {
                m_strValue = value;
            }
            get
            {
                return m_strValue;
            }
        }
        [Browsable(false)]
        public VisionImage Image { set; get; }

        [Browsable(false)]
        public bool IsRecipe
        {
            set
            {
                m_bIsRecipe = value;
            }
            get
            {
                return m_bIsRecipe;
            }
        }
        public double Min
        {
            set
            {
                m_dMinValue = value;
            }
            get
            {
                return m_dMinValue;
            }
        }

        public double Max
        {
            set
            {
                m_dMaxValue = value;
            }
            get
            {
                return m_dMaxValue;
            }
        }
        public string Name { get; set; }
        public string Tag { get; set; }
        public string Spare { get; set; }
        public ParameterType ParameterType
        {
            get
            {
                return m_ParameterType;
            }
        }

        public string Value
        {
            set
            {
                switch (m_Type)
                {
                    case DataType.Bool:
                        BoolValue = bool.Parse(value);
                        break;
                    case DataType.Double:
                        DoubleValue = double.Parse(value);
                        break;
                    case DataType.Long:
                        LongValue = long.Parse(value);
                        break;
                    case DataType.Image:
                        //Image.Load(value, VisionImage.FileFilter.bmp);
                        break;
                    case DataType.String:
                    default:
                        StringValue = value;
                        break;
                }
            }
            get
            {
                string value;
                switch (m_Type)
                {
                    case DataType.Bool:
                        value = BoolValue.ToString();
                        break;
                    case DataType.Double:
                        value = DoubleValue.ToString();
                        break;
                    case DataType.Long:
                        value = LongValue.ToString();
                        break;
                    case DataType.Image:
                        value = "Image";
                        break;
                    case DataType.String:
                    default:
                        value = StringValue;
                        break;
                }

                return value;
            }
        }


        public SettingParameter DeepCopy()
        {
            SettingParameter copy = new SettingParameter();
            copy.m_ParameterType = this.m_ParameterType;
            copy.m_Type = this.m_Type;
            copy.m_dValue = this.m_dValue;
            copy.m_bValue = this.m_bValue;
            copy.m_lValue = this.m_lValue;
            copy.m_strValue = this.m_strValue;
            copy.m_bIsRecipe = this.m_bIsRecipe;
            copy.Owner = this.Owner;
            copy.Name = this.Name;
            copy.m_dMaxValue = this.m_dMaxValue;
            copy.m_dMinValue = this.m_dMinValue;

            return copy;
        }

        public void SetOffset(SettingParameter param)
        {
            if (this.m_Type == param.m_Type)
            {
                switch (m_Type)
                {
                    case DataType.Bool:
                        this.BoolValue = BoolValue;
                        break;
                    case DataType.Double:
                        this.DoubleValue += param.DoubleValue;
                        break;
                    case DataType.Long:
                        this.LongValue += param.LongValue;
                        break;
                    case DataType.String:
                    default:
                        this.StringValue += param.StringValue;
                        break;
                }
            }
        }

        public void SetValue(SettingParameter parameter)
        {
            this.m_ParameterType = parameter.m_ParameterType;
            this.m_Type = parameter.m_Type;
            this.m_dValue = parameter.m_dValue;
            this.m_bValue = parameter.m_bValue;
            this.m_lValue = parameter.m_lValue;
            this.m_strValue = parameter.m_strValue;
            this.m_bIsRecipe = parameter.m_bIsRecipe;
            this.Image = parameter.Image;
            this.Owner = parameter.Owner;
            this.Name = parameter.Name;
            this.m_dMinValue = parameter.m_dMinValue;
            this.m_dMaxValue = parameter.m_dMaxValue;
        }

    }
    [Serializable]
    public class SettingParameterCollection : Collection<SettingParameter>
    {
        public SettingParameterCollection()
        {
        }

        public void Add(SettingParameterCollection Parameters)
        {
            foreach (SettingParameter parameter in Parameters)
            {
                this.Add(parameter);
            }
        }

        public SettingParameter GetActionParameter(string strName)
        {
            SettingParameter ret = null;
            foreach (SettingParameter parameter in this)
            {
                if (parameter.Name == strName)
                {
                    ret = parameter;
                    break;
                }
            }

            return ret;
        }

        public SettingParameterCollection DeepCopy()
        {
            SettingParameterCollection parameters = new SettingParameterCollection();
            foreach (SettingParameter parameter in this)
            {
                parameters.Add(parameter.DeepCopy());
            }

            return parameters;
        }

        public void CopyValue(SettingParameterCollection actionParameters)
        {
            foreach (SettingParameter dest in this)
            {
                foreach (SettingParameter src in actionParameters)
                {
                    if(dest.Name == src.Name && dest.Owner?.Name == src.Owner?.Name)
                    {
                        dest.SetValue(src);
                        break;
                    }
                }
            }
        }


        public SettingParameterCollection SetOffset(SettingParameterCollection OffsetParameters)
        {
            if (OffsetParameters == null || OffsetParameters.Count == 0)
                return this;
            SettingParameterCollection parameters = new SettingParameterCollection();
            foreach (SettingParameter parameter in this)
            {
                SettingParameter newParam = parameter.DeepCopy();
                
                foreach(SettingParameter offset in OffsetParameters)
                {
                    if (newParam.Name == offset.Name && newParam.Owner.Name == offset.Owner.Name)
                    {
                        newParam.SetOffset(offset);
                        break;
                    }
                }
                parameters.Add(newParam);

            }

            return parameters;
        }

        public SettingParameterCollection GetOffsetCollection()
        {
            SettingParameterCollection offset = new SettingParameterCollection();

            foreach(SettingParameter parameter in this)
            {
                if(parameter.ParameterType == ParameterType.Position)
                {
                    offset.Add(parameter.DeepCopy());
                }
            }

            return offset;
        }
    }


}
