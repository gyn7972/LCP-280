using QMC.Common.Vision;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QMC.Common
{
    [Serializable]
    public class Param
    {
        public enum DisplayTypeKey
        {
            Image,
            Combobox,
            Text,
            CheckBox,
            Coordinate,

        }
        public enum ValueTypeKey
        {
            None,
            Int,
            Double,
            String,
            Bool,
            Image,
            XY_Coordinate,
            XYT_Coordinate,
            XYZ_Coordinate,
            XYZT_Coordinate,
            Size,
            SizeD,
            TimeSpanInfo,
            PointD,
            Point,
            RectangleD,
            VisionImage,
            Byte,
            Uint,
            RangeD,
        }

        public string Title { get; set; } // 이름
        public DisplayTypeKey DisplayType { get; set; } //표시 형식

        public object Value; // 값
        public ValueTypeKey ValueType { get; set; } //값 형식

        public List<string> SelectValues { set; get; } // Combobox Item's

        public string Group { get; set; } // 그룹명

        public int Index; // Group 안에서 몇번째 Param인지...
        public Param()
        {
            Init();
        }
        public Param(string strTitle, DisplayTypeKey displayType, object value, ValueTypeKey valueType, string strGroup)
        {
            Init();
            SetParam(strTitle, displayType, value, valueType, strGroup);
        }
        private void Init()
        {
            SelectValues = new List<string>();
            Value = null;
            DisplayType = DisplayTypeKey.Text;
            ValueType = ValueTypeKey.None;
            Group = string.Empty;
        }
        public void SetParam(string strTitle, DisplayTypeKey displayType, object value, ValueTypeKey valueType, string strGroup)
        {
            this.Title = strTitle;
            this.ValueType = valueType;
            this.DisplayType = displayType;
            this.Value = value;
            this.Group = strGroup;
        }

        public List<string> GetSelectValues()
        {
            return SelectValues;
        }

        public bool GetIntValue(ref int nValue)
        {
            bool bRet = false;
            if(ValueType == ValueTypeKey.Int)
            {
                bRet = true;
                nValue = Convert.ToInt32(Value);
            }

            return bRet;
        }

        public override string ToString()
        {
            if(Value == null)
                return string.Empty;
            return Value.ToString();
        }

        public bool GetDoubleValue(ref double dValue)
        {
            bool bRet = false;
            if (ValueType == ValueTypeKey.Double)
            {
                bRet = true;
                dValue = Convert.ToDouble(Value);
            }
            return bRet;
        }

        public bool GetStringValue(ref string strValue)
        {
            bool bRet = false;
            if(ValueType == ValueTypeKey.String)
            {
                bRet = true;
                if (Value != null)
                {
                    strValue = Value.ToString();
                }
                else
                {
                    Value = String.Empty;
                }
            }
            return bRet;
        }

        public bool GetBoolValue(ref bool bValue)
        {
            bool bRet = false;
            if (ValueType == ValueTypeKey.Bool)
            {
                bRet = true;
                bValue = Convert.ToBoolean(Value); // 확인 필요... 0, 1 인덱스면 치환해줘야할수도...
            }
            return bRet;
        }

        public bool GetImageValue(ref Image imgValue)
        {
            bool bRet = false;
            if (ValueType == ValueTypeKey.Image)
            {
                bRet = true;
                imgValue = (Image)Value;
            }
            return bRet;
        }
        public bool GetImageValue(ref VisionImage imgValue)
        {
            bool bRet = false;
            if (ValueType == ValueTypeKey.VisionImage)
            {
                bRet = true;
                imgValue = (VisionImage)Value;
            }
            return bRet;
        }

        public bool GetXYValue(ref XyCoordinate xyValue)
        {
            bool bRet = false;
            if (ValueType == ValueTypeKey.XY_Coordinate)
            {
                bRet = true;
                string strTemp = Value.ToString();
                strTemp = RemoveParenthesis(strTemp);
                string[] split_Temp = strTemp.Split(',');
                xyValue.X = Convert.ToDouble(split_Temp[0]);
                xyValue.Y = Convert.ToDouble(split_Temp[1]);
            }
            return bRet;
        }

        public bool GetXYTValue(ref XytCoordinate xytValue)
        {
            bool bRet = false;
            if (ValueType == ValueTypeKey.XYT_Coordinate)
            {
                bRet = true;
                string strTemp = Value.ToString();
                strTemp = RemoveParenthesis(strTemp);
                string[] split_Temp = strTemp.Split(',');
                xytValue.X = Convert.ToDouble(split_Temp[0]);
                xytValue.Y = Convert.ToDouble(split_Temp[1]);
                xytValue.T = Convert.ToDouble(split_Temp[2]);
            }
            return bRet;
        }

        public bool GetXYZValue(ref XyzCoordinate xyzValue)
        {
            bool bRet = false;
            if (ValueType == ValueTypeKey.XYZ_Coordinate)
            {
                bRet = true;
                string strTemp = Value.ToString();
                strTemp = RemoveParenthesis(strTemp);
                string[] split_Temp = strTemp.Split(',');
                xyzValue.X = Convert.ToDouble(split_Temp[0]);
                xyzValue.Y = Convert.ToDouble(split_Temp[1]);
                xyzValue.Z = Convert.ToDouble(split_Temp[2]);
            }
            return bRet;
        }

        public bool GetXYZTValue(ref XyztCoordinate xyztValue)
        {
            bool bRet = false;
            if (ValueType == ValueTypeKey.XYZT_Coordinate)
            {
                bRet = true;
                xyztValue = (XyztCoordinate)Value;
            }
            if (ValueType == ValueTypeKey.Point)
            {
                bRet = true;
                string strTemp = Value.ToString();
                strTemp = RemoveParenthesis(strTemp);
                string[] split_Temp = strTemp.Split(',');
                xyztValue.X = Convert.ToDouble(split_Temp[0]);
                xyztValue.Y = Convert.ToDouble(split_Temp[1]);
                xyztValue.Z = Convert.ToDouble(split_Temp[2]);
                xyztValue.T = Convert.ToDouble(split_Temp[3]);
            }
            return bRet;
        }

        public bool GetPointValue(ref Point xyValue)
        {
            bool bRet = false;
            if (ValueType == ValueTypeKey.Point)
            {
                bRet = true;
                string strTemp = Value.ToString();
                strTemp = RemoveParenthesis(strTemp);
                string[] split_Temp = strTemp.Split(',');
                xyValue.X = Convert.ToInt32(split_Temp[0]);
                xyValue.Y = Convert.ToInt32(split_Temp[1]);
            }
            return bRet;
        }
        public bool GetPointDValue(ref PointD xyValue)
        {
            bool bRet = false;
            if (ValueType == ValueTypeKey.PointD)
            {
                bRet = true;
                string strTemp = Value.ToString();
                strTemp = RemoveParenthesis(strTemp);
                string[] split_Temp = strTemp.Split(',');
                xyValue.X = Convert.ToDouble(split_Temp[0]);
                xyValue.Y = Convert.ToDouble(split_Temp[1]);
            }
            return bRet;
        }

        public bool GetSizeValue(ref Size size)
        {
            bool bRet = false;
            if (ValueType == ValueTypeKey.Size)
            {
                bRet = true;
                string strTemp = Value.ToString();
                strTemp = RemoveParenthesis(strTemp);
                string[] split_Temp = strTemp.Split(',');
                size.Width = Convert.ToInt32(split_Temp[0]);
                size.Height = Convert.ToInt32(split_Temp[1]);
            }
            return bRet;
        }

        public bool GetSizeDValue(ref SizeD sizeD)
        {
            bool bRet = false;
            if (ValueType == ValueTypeKey.SizeD)
            {
                bRet = true;
                string strTemp = Value.ToString();
                strTemp = RemoveParenthesis(strTemp);
                string[] split_Temp = strTemp.Split(',');
                sizeD.Width = Convert.ToDouble(split_Temp[0]);
                sizeD.Height = Convert.ToDouble(split_Temp[1]);
            }
            return bRet;
        }

        public bool GetTimeSpanInfoValue(ref string strTimeSpan)
        {
            bool bRet = false;
            if(ValueType == ValueTypeKey.TimeSpanInfo)
            {
                bRet = true;
                strTimeSpan = Value.ToString();
            }
            return bRet;
        }

        public bool GetRectangleDValue(ref RectangleD rectangleD)
        {
            bool bRet = false;
            if (ValueType == ValueTypeKey.RectangleD)
            {
                bRet = true;
                string strTemp = Value.ToString();
                strTemp = RemoveParenthesis(strTemp);
                string[] split_Temp = strTemp.Split(',');
                rectangleD.X = Convert.ToDouble(split_Temp[0]);
                rectangleD.Y = Convert.ToDouble(split_Temp[1]);
                rectangleD.Width = Convert.ToDouble(split_Temp[2]);
                rectangleD.Height = Convert.ToDouble(split_Temp[3]);
            }
            return bRet;
        }

        public bool GetByteValue(ref byte byteValue)
        {
            bool bRet = false;
            if(ValueType == ValueTypeKey.Byte)
            {
                bRet = true;
                byteValue = (byte)Value;
            }
            return bRet;
        }

        public bool GetUintValue(ref uint nValue)
        {
            bool bRet = false;
            if (ValueType == ValueTypeKey.Uint)
            {
                bRet = true;
                nValue = Convert.ToUInt32(Value);
            }
            return bRet;
        }

        public bool GetRangeDValue(ref RangeD rValue)
        {
            bool bRet = false;
            if (ValueType == ValueTypeKey.RangeD)
            {
                bRet = true;
                string strTemp = Value.ToString();
                strTemp = RemoveParenthesis(strTemp);
                string[] split_Temp = strTemp.Split(',');
                rValue.Minimum = Convert.ToDouble(split_Temp[0]);
                rValue.Maximum = Convert.ToDouble(split_Temp[1]);
            }
            return bRet;
        }
        public void SetValue(int nValue)
        {
            if(ValueType == ValueTypeKey.None || ValueType == ValueTypeKey.Int)
            {
                Value = nValue;
                ValueType = ValueTypeKey.Int;
            }
        }

        public void SetValue(string strValue)
        {
            if (strValue != "")
            {
                if (ValueType == ValueTypeKey.None || ValueType == ValueTypeKey.String)
                {
                    Value = strValue;
                    ValueType = ValueTypeKey.String;
                }
                else if(ValueType == ValueTypeKey.Int)
                {
                    int nValue = Convert.ToInt32(strValue);
                    Value = nValue;
                }
            }
        }

        public void SetValue(double dValue)
        {

            if (ValueType == ValueTypeKey.None || ValueType == ValueTypeKey.Double)
            {
                Value = dValue;
                ValueType = ValueTypeKey.Double;
            }
        }

        public void SetValue(bool bValue)
        {
            if (ValueType == ValueTypeKey.None || ValueType == ValueTypeKey.Bool)
            {
                Value = bValue;
                ValueType = ValueTypeKey.Bool;
            }
        }

        public void SetValue(Image image)
        {
            if (ValueType == ValueTypeKey.None || ValueType == ValueTypeKey.Image)
            {
                Value = image;
                ValueType = ValueTypeKey.Image;
            }
        }
        public void SetValue(VisionImage image)
        {
            if (ValueType == ValueTypeKey.None || ValueType == ValueTypeKey.VisionImage)
            {
                Value = image;
                ValueType = ValueTypeKey.VisionImage;
            }
        }

        public void SetValue(XyCoordinate xyCoordinate)
        {
            if (ValueType == ValueTypeKey.None || ValueType == ValueTypeKey.XY_Coordinate)
            {
                Value = xyCoordinate;
                ValueType = ValueTypeKey.XY_Coordinate;
            }
        }

        public void SetValue(XytCoordinate xytCoordinate)
        {
            if (ValueType == ValueTypeKey.None || ValueType == ValueTypeKey.XYT_Coordinate)
            {
                Value = xytCoordinate;
                ValueType = ValueTypeKey.XYT_Coordinate;
            }
        }

        public void SetValue(XyzCoordinate xyzCoordinate)
        {
            if (ValueType == ValueTypeKey.None || ValueType == ValueTypeKey.XYZ_Coordinate)
            {
                Value = xyzCoordinate;
                ValueType = ValueTypeKey.XYZ_Coordinate;
            }
        }

        public void SetValue(XyztCoordinate xyztCoordinate)
        {
            if (ValueType == ValueTypeKey.None || ValueType == ValueTypeKey.XYZT_Coordinate)
            {
                Value = xyztCoordinate;
                ValueType = ValueTypeKey.XYZT_Coordinate;
            }
        }

        public void SetValue(Size size)
        {
            if (ValueType == ValueTypeKey.None || ValueType == ValueTypeKey.Size)
            {
                Value = size;
                ValueType = ValueTypeKey.Size;
            }
        }
        public void SetValue(SizeD sizeD)
        {
            if (ValueType == ValueTypeKey.None || ValueType == ValueTypeKey.SizeD)
            {
                Value = sizeD;
                ValueType = ValueTypeKey.SizeD;
            }
        }
        public void SetValue(TimeSpan timeSpan)
        {
            if(ValueType == ValueTypeKey.None || ValueType == ValueTypeKey.TimeSpanInfo)
            {
                Value = timeSpan.ToString();
                ValueType = ValueTypeKey.TimeSpanInfo;
            }
        }
        public void SetValue(PointD pointD)
        {
            if (ValueType == ValueTypeKey.None || ValueType == ValueTypeKey.PointD)
            {
                Value = pointD;
                ValueType = ValueTypeKey.PointD;
            }
        }
        public void SetValue(Point point)
        {
            if (ValueType == ValueTypeKey.None || ValueType == ValueTypeKey.Point)
            {
                Value = point.ToString();
                ValueType = ValueTypeKey.Point;
            }
        }
        public void SetValue(RectangleD rectangleD)
        {
            if (ValueType == ValueTypeKey.None || ValueType == ValueTypeKey.RectangleD)
            {
                Value = rectangleD;
                ValueType = ValueTypeKey.RectangleD;
            }
        }

        public void SetValue(byte byteValue)
        {
            if (ValueType == ValueTypeKey.None || ValueType == ValueTypeKey.Byte)
            {
                Value = byteValue;
                ValueType = ValueTypeKey.Byte;
            }
        }

        public void SetValue(uint nValue)
        {
            if (ValueType == ValueTypeKey.None || ValueType == ValueTypeKey.Uint)
            {
                Value = nValue;
                ValueType = ValueTypeKey.Uint;
            }
        }

        public void SetValue(RangeD rangeD)
        {
            if (ValueType == ValueTypeKey.None || ValueType == ValueTypeKey.RangeD)
            {
                Value = rangeD;
                ValueType = ValueTypeKey.RangeD;
            }
        }

        private string RemoveParenthesis(string strTextBoxValue)
        {
            string strTemp1 = string.Empty;
            string strTemp2 = string.Empty;
            string strTemp3 = string.Empty;
            string strTemp4 = string.Empty;
            string strTemp5 = string.Empty;
            string strTemp6 = string.Empty;
            string strTemp7 = string.Empty;
            string strTemp8 = string.Empty;
            string strTemp9 = string.Empty;
            string strTemp10 = string.Empty;


            strTemp1 = strTextBoxValue.Replace("[", "");
            strTemp2 = strTemp1.Replace("]", "");
            strTemp3 = strTemp2.Replace(" ", "");
            strTemp4 = strTemp3.Replace("{", "");
            strTemp5 = strTemp4.Replace("}", "");
            strTemp6 = strTemp5.Replace("Width", "");
            strTemp7 = strTemp6.Replace("Height", "");
            strTemp8 = strTemp7.Replace("=", "");
            strTemp9 = strTemp8.Replace("X", "");
            strTemp10 = strTemp9.Replace("Y", "");

            return strTemp10;
        }
    }
}
