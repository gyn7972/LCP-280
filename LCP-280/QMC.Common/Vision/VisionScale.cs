using QMC.Common.Vision.Tools;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace QMC.Common
{
    [Serializable]
    public enum ParamVisionScaleKey
    {
        X,
        Y,
        XAxisT,
        YAxisT,
        InvertedX,
        InvertedY,
        UsedScaleT,
    }

    [Serializable]
    public sealed class VisionScale
    {
        #region Field
        private double m_X;
        private double m_Y;
        //private double m_T;
        private double m_XAxisT;
        private double m_YAxisT;
        private bool m_InvertedX;
        private bool m_InvertedY;
        private bool m_UsedScaleT;

        public string Name { get; set; }
        #endregion

        #region Constructor
        public VisionScale(double x, double y)
        {
            this.X = x;
            this.Y = y;

            this.InvertedX = false;
            this.InvertedY = false;

            this.UsedScaleT = true;

            this.X = 0.0017536;
            this.Y = 0.0017549;

            this.InvertedX = false;
            this.InvertedY = true;

            this.UsedScaleT = true;
            m_XAxisT = -0.129;
            m_YAxisT = 89.959;

            Name = "VisionScale";
        }
        public VisionScale() : this(0.0, 0.0) { }
        #endregion

        #region Property
        /// <summary>
        /// X축의 Scale값을 가져오거나 설정한다.
        /// </summary>
        public double X
        {
            get { return this.m_X; }
            set { this.m_X = value; }
        }

        /// <summary>
        /// Y축의 Scale값을 가져오거나 설정한다.
        /// </summary>
        public double Y
        {
            get { return this.m_Y; }
            set { this.m_Y = value; }
        }

        ///// <summary>
        ///// Camera와 Motion과의 Tilt된 Theta를 가져오거나 설정한다.
        ///// </summary>
        //public double T
        //{
        //    get { return this.m_T; }
        //    set { this.m_T = value; }
        //}

        /// <summary>
        /// Camera와 Motion X축과의 Tilt된 Theta를 가져오거나 설정한다.
        /// </summary>
        public double XAxisT
        {
            get { return this.m_XAxisT; }
            set { this.m_XAxisT = value; }
        }

        /// <summary>
        /// Camera와 Motion Y축과의 Tilt된 Theta를 가져오거나 설정한다.
        /// </summary>
        public double YAxisT
        {
            get { return this.m_YAxisT; }
            set { this.m_YAxisT = value; }
        }

        public bool InvertedX
        {
            get { return this.m_InvertedX; }
            set { this.m_InvertedX = value; }
        }

        public bool InvertedY
        {
            get { return this.m_InvertedY; }
            set { this.m_InvertedY = value; }
        }

        public bool UsedScaleT
        {
            get { return this.m_UsedScaleT; }
            set { this.m_UsedScaleT = value; }
        }
        #endregion

        #region Method
        #region ConvertPosition
        public static int ConvertPosition<T, U>(VisionScale scale, Size cameraResolution, T currentcoordinate, PointD position, out U coordinate)
            where T : struct
            where U : struct
        {
            int ret = 0;
            IXyCoordinate current;
            IXyCoordinate result;
            PointD convertedX = PointD.Empty;
            PointD convertedY = PointD.Empty;
            PointD converted = PointD.Empty;
            coordinate = default(U);

            current = (XyCoordinate)CopyUtility.GetDeepCopy(currentcoordinate);

            if (scale.UsedScaleT == true)
            {
                convertedX = qGeometry.CalculateRotationTransformation(new PointD(cameraResolution.Width / 2, 0),
                    new PointD(position.X, 0), -scale.XAxisT);

                if (scale.YAxisT <= 0)
                    convertedY = qGeometry.CalculateRotationTransformation(new PointD(0, cameraResolution.Height / 2),
                        new PointD(0, position.Y), -(scale.YAxisT + 90));
                else if ((0 < scale.YAxisT))
                    convertedY = qGeometry.CalculateRotationTransformation(new PointD(0, cameraResolution.Height / 2),
                        new PointD(0, position.Y), -(scale.YAxisT - 90));

                converted = convertedX + convertedY;
            }
            else
            {
                converted = position;
            }

            converted = new PointD((converted.X - cameraResolution.Width / 2) * scale.X * (scale.InvertedX ? 1 : -1), (converted.Y - cameraResolution.Height / 2) * scale.Y * (scale.InvertedY ? 1 : -1));

            if (typeof(U) == typeof(XyztCoordinate))
            {
                if (current is XyztCoordinate)
                    result = new XyztCoordinate(((XyztCoordinate)current).X + converted.X, ((XyztCoordinate)current).Y + converted.Y, ((XyztCoordinate)current).Z, ((XyztCoordinate)current).T);
                else if (current is XytCoordinate)
                    result = new XyztCoordinate(((XytCoordinate)current).X + converted.X, ((XytCoordinate)current).Y + converted.Y, 0, ((XytCoordinate)current).T);
                else if (current is XyzCoordinate)
                    result = new XyztCoordinate(((XyzCoordinate)current).X + converted.X, ((XyzCoordinate)current).Y + converted.Y, ((XyzCoordinate)current).Z, 0);
                else
                    result = new XyztCoordinate(((XyCoordinate)current).X + converted.X, ((XyCoordinate)current).Y + converted.Y, 0, 0);
            }
            else if (typeof(U) == typeof(XytCoordinate))
            {
                if (current is XyztCoordinate)
                    result = new XytCoordinate(((XyztCoordinate)current).X + converted.X, ((XyztCoordinate)current).Y + converted.Y, ((XyztCoordinate)current).T);
                else if (current is XytCoordinate)
                    result = new XytCoordinate(((XytCoordinate)current).X + converted.X, ((XytCoordinate)current).Y + converted.Y, ((XytCoordinate)current).T);
                else if (current is XyzCoordinate)
                    result = new XytCoordinate(((XyzCoordinate)current).X + converted.X, ((XyzCoordinate)current).Y + converted.Y, 0);
                else
                    result = new XytCoordinate(((XyCoordinate)current).X + converted.X, ((XyCoordinate)current).Y + converted.Y, 0);
            }
            else if (typeof(U) == typeof(XyzCoordinate))
            {
                if (current is XyztCoordinate)
                    result = new XyzCoordinate(((XyztCoordinate)current).X + converted.X, ((XyztCoordinate)current).Y + converted.Y, ((XyztCoordinate)current).Z);
                else if (current is XytCoordinate)
                    result = new XyzCoordinate(((XytCoordinate)current).X + converted.X, ((XytCoordinate)current).Y + converted.Y, 0);
                else if (current is XyzCoordinate)
                    result = new XyzCoordinate(((XyzCoordinate)current).X + converted.X, ((XyzCoordinate)current).Y + converted.Y, ((XyzCoordinate)current).Z);
                else
                    result = new XyzCoordinate(((XyCoordinate)current).X + converted.X, ((XyCoordinate)current).Y + converted.Y, 0);
            }
            else
            {
                if (current is XyztCoordinate)
                    result = new XyCoordinate(((XyztCoordinate)current).X + converted.X, ((XyztCoordinate)current).Y + converted.Y);
                else if (current is XytCoordinate)
                    result = new XyCoordinate(((XytCoordinate)current).X + converted.X, ((XytCoordinate)current).Y + converted.Y);
                else if (current is XyzCoordinate)
                    result = new XyCoordinate(((XyzCoordinate)current).X + converted.X, ((XyzCoordinate)current).Y + converted.Y);
                else
                    result = new XyCoordinate(((XyCoordinate)current).X + converted.X, ((XyCoordinate)current).Y + converted.Y);
            }

            coordinate = (U)result;

            //Log.Write("VisionScale", string.Format("ConvertPosition() Coordinate : {0}", coordinate.ToString()));
            return ret;
        }

        public static int ConvertPosition<T, U>(VisionScale scale, Size cameraResolution, T currentcoordinate, PointDCollection positions, out List<U> coordinates)
            where T : struct
            where U : struct
        {
            int ret = 0;
            coordinates = new List<U>();
            U coordinate = default(U);

            for (int i = 0; i < positions.Count; i++)
            {
                VisionScale.ConvertPosition<T, U>(scale, cameraResolution, currentcoordinate, positions[i], out coordinate);

                coordinates.Add(coordinate);
            }

            return ret;
        }

        public static int ConvertPosition<T, U>(VisionScale scale, Size cameraResolution, T currentcoordinate, Point position, out U coordinate)
            where T : struct
            where U : struct
        {
            return VisionScale.ConvertPosition(scale, cameraResolution, currentcoordinate, new PointD(position.X, position.Y), out coordinate);
        }

        public static int ConvertPosition<T, U>(VisionScale scale, Size cameraResolution, T currentcoordinate, PointCollection positions, out List<U> coordinates)
            where T : struct
            where U : struct
        {
            int ret = 0;
            coordinates = new List<U>();
            U coordinate = default(U);

            for (int i = 0; i < positions.Count; i++)
            {
                VisionScale.ConvertPosition<T, U>(scale, cameraResolution, currentcoordinate, positions[i], out coordinate);

                coordinates.Add(coordinate);
            }

            return ret;
        }

        public static int ConvertPosition<T, U>(VisionScale scale, Size cameraResolution, T currenPosition, PatternMatchingResult.PatternMatchingResultValue value, out U coordinate)
            where T : struct
            where U : struct
        {
            return VisionScale.ConvertPosition(scale, cameraResolution, currenPosition, new PointD(value.X, value.Y), out coordinate);
        }

        public static int ConvertPosition<T, U>(VisionScale scale, Size cameraResolution, T currenPosition, PatternMatchingResult value, out List<U> coordinates)
            where T : struct
            where U : struct
        {
            int ret = 0;
            coordinates = new List<U>();
            U coordinate = default(U);

            for (int i = 0; i < value.Values.Count; i++)
            {
                VisionScale.ConvertPosition<T, U>(scale, cameraResolution, currenPosition, new PointD(value.Values[i].X, value.Values[i].Y), out coordinate);

                coordinates.Add(coordinate);
            }

            return ret;
        }

        public static int ConvertPosition<T>(VisionScale scale, Size cameraResolution, PointD position, out T coordinate)
            where T : struct
        {
            return VisionScale.ConvertPosition<T, T>(scale, cameraResolution, Activator.CreateInstance<T>(), position, out coordinate);
        }

        public static int ConvertPosition<T>(VisionScale scale, Size cameraResolution, PointDCollection positions, out List<T> coordinates)
            where T : struct
        {
            return VisionScale.ConvertPosition<T, T>(scale, cameraResolution, Activator.CreateInstance<T>(), positions, out coordinates);
        }

        public static int ConvertPosition<T>(VisionScale scale, Size cameraResolution, Point position, out T coordinate)
            where T : struct
        {
            return VisionScale.ConvertPosition<T, T>(scale, cameraResolution, Activator.CreateInstance<T>(), new PointD(position.X, position.Y), out coordinate);
        }

        public static int ConvertPosition<T>(VisionScale scale, Size cameraResolution, PointCollection positions, out List<T> coordinates)
            where T : struct
        {
            return VisionScale.ConvertPosition<T, T>(scale, cameraResolution, Activator.CreateInstance<T>(), positions, out coordinates);
        }

        public static int ConvertPosition<T>(VisionScale scale, Size cameraResolution, PatternMatchingResult.PatternMatchingResultValue value, out T coordinate)
            where T : struct
        {
            return VisionScale.ConvertPosition<T, T>(scale, cameraResolution, Activator.CreateInstance<T>(), value, out coordinate);
        }

        public static int ConvertPosition<T>(VisionScale scale, Size cameraResolution, PatternMatchingResult value, out List<T> coordinates)
            where T : struct
        {
            return VisionScale.ConvertPosition<T, T>(scale, cameraResolution, Activator.CreateInstance<T>(), value, out coordinates);
        }
        #endregion

        #region ConvertPixel
        public static int ConvertPixel<T, U>(VisionScale scale, Size cameraResolution, T currentcoordinate, U coordinate, out PointD point)
            where T : struct
            where U : struct
        {
            int ret = 0;
            IXyCoordinate current;
            IXyCoordinate target;
            PointD convertedX = PointD.Empty;
            PointD convertedY = PointD.Empty;
            PointD converted = PointD.Empty;
            //int direction = 1;
            point = new PointD();

            current = CopyUtility.GetDeepCopy(currentcoordinate) as IXyCoordinate;
            target = CopyUtility.GetDeepCopy(coordinate) as IXyCoordinate;

            converted = new PointD(target.X - current.X, target.Y - current.Y);

            converted = new PointD(converted.X / scale.X * (scale.InvertedX ? 1 : -1) + cameraResolution.Width / 2, converted.Y / scale.Y * (scale.InvertedY ? 1 : -1) + cameraResolution.Height / 2);

            if (scale.UsedScaleT == true)
            {
                convertedX = qGeometry.CalculateRotationTransformation(new PointD(cameraResolution.Width / 2, 0),
                    new PointD(converted.X, 0), scale.XAxisT);

                if (scale.YAxisT <= 0)
                    convertedY = qGeometry.CalculateRotationTransformation(new PointD(0, cameraResolution.Height / 2),
                        new PointD(0, converted.Y), (scale.YAxisT + 90));
                else if ((0 < scale.YAxisT))
                    convertedY = qGeometry.CalculateRotationTransformation(new PointD(0, cameraResolution.Height / 2),
                        new PointD(0, converted.Y), (scale.YAxisT - 90));

                point = convertedX + convertedY;
            }
            else
            {
                point = new PointD(converted.X, converted.Y);
            }


            return ret;
        }

        public static int ConvertPixel<T>(VisionScale scale, Size cameraResolution, T coordinate, out PointD point)
            where T : struct
        {
            return VisionScale.ConvertPixel<T, T>(scale, cameraResolution, Activator.CreateInstance<T>(), coordinate, out point);
        }

        public static int ConvertPixel<T>(VisionScale scale, Size cameraResolution, T coordinate, out SizeD size)
            where T : struct
        {
            int ret = 0;
            PointD point = new PointD();

            size = new SizeD();

            if ((ret = VisionScale.ConvertPixel<T, T>(scale, cameraResolution, Activator.CreateInstance<T>(), coordinate, out point)) != 0) return ret;

            size = new SizeD(point.X, point.Y);
            return ret;
        }

        public ParamGroup GetGroup()
        {
            ParamGroup paramGroup = new ParamGroup();
            paramGroup.Name = this.GetType().Name;
            {
                Param param = new Param();
                param.SetParam(nameof(X), Param.DisplayTypeKey.Text, X, Param.ValueTypeKey.Double, paramGroup.Name);
                paramGroup.AddParam(param);
            }
            {
                Param param = new Param();
                param.SetParam(nameof(Y), Param.DisplayTypeKey.Text, Y, Param.ValueTypeKey.Double, paramGroup.Name);
                paramGroup.AddParam(param);
            }
            {
                Param param = new Param();
                param.SetParam(nameof(XAxisT), Param.DisplayTypeKey.Text, XAxisT, Param.ValueTypeKey.Double, paramGroup.Name);
                paramGroup.AddParam(param);
            }
            {
                Param param = new Param();
                param.SetParam(nameof(YAxisT), Param.DisplayTypeKey.Text, YAxisT, Param.ValueTypeKey.Double, paramGroup.Name);
                paramGroup.AddParam(param);
            }
            {
                Param param = new Param();
                param.SetParam(nameof(InvertedX), Param.DisplayTypeKey.CheckBox, InvertedX, Param.ValueTypeKey.Bool, paramGroup.Name);
                paramGroup.AddParam(param);
            }
            {
                Param param = new Param();
                param.SetParam(nameof(InvertedY), Param.DisplayTypeKey.CheckBox, InvertedY, Param.ValueTypeKey.Bool, paramGroup.Name);
                paramGroup.AddParam(param);
            }
            {
                Param param = new Param();
                param.SetParam(nameof(UsedScaleT), Param.DisplayTypeKey.CheckBox, UsedScaleT, Param.ValueTypeKey.Bool, paramGroup.Name);
                paramGroup.AddParam(param);
            }

            return paramGroup;
        }

        public void SetGroup(ParamGroup paramGroup)
        {
            if (paramGroup != null)
            {
                Param param = null;
                param = paramGroup.GetParam((int)ParamVisionScaleKey.X);
                if (param != null)
                {
                    double value = 0.0;
                    if (param.GetDoubleValue(ref value))
                    {
                        X = value;
                    }
                }
                param = paramGroup.GetParam((int)ParamVisionScaleKey.Y);
                if (param != null)
                {
                    double value = 0.0;
                    if (param.GetDoubleValue(ref value))
                    {
                        Y = value;
                    }
                }
                param = paramGroup.GetParam((int)ParamVisionScaleKey.XAxisT);
                if (param != null)
                {
                    double value = 0.0;
                    if (param.GetDoubleValue(ref value))
                    {
                        XAxisT = value;
                    }
                }
                param = paramGroup.GetParam((int)ParamVisionScaleKey.YAxisT);
                if (param != null)
                {
                    double value = 0.0;
                    if (param.GetDoubleValue(ref value))
                    {
                        YAxisT = value;
                    }
                }
                param = paramGroup.GetParam((int)ParamVisionScaleKey.InvertedX);
                if (param != null)
                {
                    bool value = false;
                    if (param.GetBoolValue(ref value))
                    {
                        InvertedX = value;
                    }
                }
                param = paramGroup.GetParam((int)ParamVisionScaleKey.InvertedY);
                if (param != null)
                {
                    bool value = false;
                    if (param.GetBoolValue(ref value))
                    {
                        InvertedY = value;
                    }
                }
                param = paramGroup.GetParam((int)ParamVisionScaleKey.UsedScaleT);
                if (param != null)
                {
                    bool value = false;
                    if (param.GetBoolValue(ref value))
                    {
                        UsedScaleT = value;
                    }
                }
            }
        }
    }
    #endregion
    #endregion
}

