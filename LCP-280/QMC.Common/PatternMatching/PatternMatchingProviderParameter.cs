using System;
using System.ComponentModel;

namespace QMC.Common
{
    [Serializable]
    public enum ParamPatternMatchingProviderParameterKey
    {
        AngleTolerance,
        UseMaskImage,
        MaskRegion,
        AutoEdgeThresholdEnable,
        EdgeThreshold,
        IgnorePolarity,
    }
    [Serializable]
    public class PatternMatchingProviderParameter
    {
        #region Field
        private RangeD m_AngleTolerance;
        private bool m_UseMaskImage;
        private RectangleD m_MaskRegion;
        private bool m_AutoEdgeThresholdEnable;
        private double m_EdgeThreshold;
        private bool m_IgnorePolarity;
        private PatternMatchingParameters m_NormalParameters;
        [Browsable(false)]
        public PatternMatchingParameters NormalParameters
        {
            get
            {
                return m_NormalParameters;
            }
            set
            {
                m_NormalParameters = value;
            }
        }

        #endregion

        #region Constructor
        public PatternMatchingProviderParameter() : base()
        {
            this.AngleTolerance = new RangeD(-5, 5);
            this.UseMaskImage = false;
            this.MaskRegion = new RectangleD();
            this.AutoEdgeThresholdEnable = true;
            this.EdgeThreshold = 10.0;
            this.IgnorePolarity = false;
            if (NormalParameters == null)
                NormalParameters = new PatternMatchingParameters();
        }
        #endregion

        #region Property

        public RangeD AngleTolerance
        {
            get { return this.m_AngleTolerance; }
            set { this.m_AngleTolerance = value; }
        }

        public bool UseMaskImage
        {
            get { return this.m_UseMaskImage; }
            set { this.m_UseMaskImage = value; }
        }

        public RectangleD MaskRegion
        {
            get { return this.m_MaskRegion; }
            set { this.m_MaskRegion = value; }
        }

        public bool AutoEdgeThresholdEnable
        {
            get { return this.m_AutoEdgeThresholdEnable; }
            set { this.m_AutoEdgeThresholdEnable = value; }
        }


        public double EdgeThreshold
        {
            get { return this.m_EdgeThreshold; }
            set { this.m_EdgeThreshold = value; }
        }

        public bool IgnorePolarity
        {
            get { return this.m_IgnorePolarity; }
            set { this.m_IgnorePolarity = value; }
        }

        public ParamGroup GetGroup()
        {
            ParamGroup group = new ParamGroup();
            group.Name = this.GetType().Name;
            {
                Param param = new Param();
                param.SetParam(nameof(AngleTolerance), Param.DisplayTypeKey.Text, AngleTolerance, Param.ValueTypeKey.RangeD, group.Name);
                group.AddParam(param);
            }
            {
                Param param = new Param();
                param.SetParam(nameof(UseMaskImage), Param.DisplayTypeKey.CheckBox, UseMaskImage, Param.ValueTypeKey.Bool, group.Name);
                group.AddParam(param);
            }
            {
                Param param = new Param();
                param.SetParam(nameof(MaskRegion), Param.DisplayTypeKey.Text, MaskRegion, Param.ValueTypeKey.RectangleD, group.Name);
                group.AddParam(param);
            }
            {
                Param param = new Param();
                param.SetParam(nameof(AutoEdgeThresholdEnable), Param.DisplayTypeKey.CheckBox, AutoEdgeThresholdEnable, Param.ValueTypeKey.Bool, group.Name);
                group.AddParam(param);
            }
            {
                Param param = new Param();
                param.SetParam(nameof(EdgeThreshold), Param.DisplayTypeKey.Text, EdgeThreshold, Param.ValueTypeKey.Double, group.Name);
                group.AddParam(param);
            }
            {
                Param param = new Param();
                param.SetParam(nameof(IgnorePolarity), Param.DisplayTypeKey.CheckBox, IgnorePolarity, Param.ValueTypeKey.Bool, group.Name);
                group.AddParam(param);
            }
            {
                if (NormalParameters != null)
                {
                    ParamGroup normalParameterGroup = NormalParameters.GetParamGroup();
                    if (normalParameterGroup != null)
                    {
                        group.SetGroup(normalParameterGroup);
                    }
                }

            }
            return group;
        }

        public void SetGroup(ParamGroup group)
        {
            if(group != null)
            {
                {
                    Param param = group.GetParam((int)ParamPatternMatchingProviderParameterKey.AngleTolerance);
                    if(param != null)
                    {
                        RangeD value = new RangeD();
                        if(param.GetRangeDValue(ref value))
                        {
                            this.AngleTolerance = value;
                        }
                    }
                }
                {
                    Param param = group.GetParam((int)ParamPatternMatchingProviderParameterKey.UseMaskImage);
                    if (param != null)
                    {
                        bool value = false;
                        if (param.GetBoolValue(ref value))
                        {
                            this.UseMaskImage = value;
                        }
                    }
                }
                {
                    Param param = group.GetParam((int)ParamPatternMatchingProviderParameterKey.MaskRegion);
                    if (param != null)
                    {
                        RectangleD value = new RectangleD();
                        if (param.GetRectangleDValue(ref value))
                        {
                            this.MaskRegion = value;
                        }
                    }
                }
                {
                    Param param = group.GetParam((int)ParamPatternMatchingProviderParameterKey.AutoEdgeThresholdEnable);
                    if (param != null)
                    {
                        bool value = false;
                        if (param.GetBoolValue(ref value))
                        {
                            this.AutoEdgeThresholdEnable = value;
                        }
                    }
                }
                {
                    Param param = group.GetParam((int)ParamPatternMatchingProviderParameterKey.EdgeThreshold);
                    if (param != null)
                    {
                        double value = 0;
                        if (param.GetDoubleValue(ref value))
                        {
                            this.EdgeThreshold = value;
                        }
                    }
                }
                {
                    Param param = group.GetParam((int)ParamPatternMatchingProviderParameterKey.IgnorePolarity);
                    if (param != null)
                    {
                        bool value = false;
                        if (param.GetBoolValue(ref value))
                        {
                            this.IgnorePolarity = value;
                        }
                    }
                }
                {
                    ParamGroup normalParameterGroup = group.GetGroup(NormalParameters.GetType().Name);
                    if (normalParameterGroup != null)
                    {
                        if (this.NormalParameters != null)
                        {
                            this.NormalParameters.SetParamGroup(normalParameterGroup);
                        }
                    }
                }
            }
        }

        #endregion
    }
}
