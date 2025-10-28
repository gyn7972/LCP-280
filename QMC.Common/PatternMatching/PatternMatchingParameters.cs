using QMC.Common.Vision;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QMC.Common
{
    [Serializable]

    public enum ParamPatternMatchingParametersKey
    {
        Tolerance,
        MaxInstance,
        MinScore,        
        DuplicateChecked,
        UseMaskImage,
    }
    [Serializable]
    public class PatternMatchingParameters
    {
        public VisionImage TrainImage { get; set; }
        public double MinTolerance { get; set; }
        public double MaxTolerance { get; set; }
        public bool DuplicateChecked { get; set; }
        public int MaxInstance { get; set; }
        public double MinScore { get; set; }

        public RectangleD MaskRegion { get; set; }
        public bool UseMaskImage { get; set; }

        public PatternMatchingParameters()
        {
            TrainImage = new VisionImage();
            MinTolerance = 0;
            MaxTolerance = 0;
            DuplicateChecked = false;
            MaxInstance = 1000;
            MinScore = 0;
            MaskRegion = new RectangleD();
            UseMaskImage = false;
        }

        public void SetOffsetTolerrance(double dOffset)
        {
            MinTolerance += dOffset;
            MaxTolerance += dOffset;
        }

        private void SetTolerance(double dTolerance)
        {
            this.MinTolerance = dTolerance * -1;
            this.MaxTolerance = dTolerance;
        }

        public PatternMatchingParameters Clone()
        {
            PatternMatchingParameters ret = new PatternMatchingParameters();
            ret.TrainImage = TrainImage;
            ret.MinTolerance = MinTolerance;
            ret.MaxTolerance = MaxTolerance;
            ret.DuplicateChecked = DuplicateChecked;
            ret.MaxInstance = MaxInstance;
            ret.MinScore = MinScore;
            ret.MaskRegion = MaskRegion;
            ret.UseMaskImage = UseMaskImage;

            return ret;
        }

        public ParamGroup GetParamGroup()
        {
            ParamGroup group = new ParamGroup();
            group.Name = this.GetType().Name;        
            {
                Param param = new Param();
                param.SetParam("Tolerance", Param.DisplayTypeKey.Text, MaxTolerance, Param.ValueTypeKey.Double, group.Name);
                group.AddParam(param);
            }            
            {
                Param param = new Param();
                param.SetParam(nameof(MaxInstance), Param.DisplayTypeKey.Text, MaxInstance, Param.ValueTypeKey.Int, group.Name);
                group.AddParam(param);
            }
            {
                Param param = new Param();
                param.SetParam(nameof(MinScore), Param.DisplayTypeKey.Text, MinScore, Param.ValueTypeKey.Double, group.Name);
                group.AddParam(param);
            }
            {
                Param param = new Param();
                param.SetParam(nameof(DuplicateChecked), Param.DisplayTypeKey.CheckBox, DuplicateChecked, Param.ValueTypeKey.Bool, group.Name);
                group.AddParam(param);
            }
            {
                Param param = new Param();
                param.SetParam(nameof(UseMaskImage), Param.DisplayTypeKey.CheckBox, UseMaskImage, Param.ValueTypeKey.Bool, group.Name);
                group.AddParam(param);
            }
            return group;
        }

        public void SetParamGroup(ParamGroup group)
        {
            if (group != null)
            {
                Param param = null;
                param = group.GetParam((int)ParamPatternMatchingParametersKey.Tolerance);
                if (param != null)
                {
                    double value = 0;
                    if (param.GetDoubleValue(ref value))
                    {
                        this.SetTolerance((double)value);
                    }
                }
                
                param = group.GetParam((int)ParamPatternMatchingParametersKey.MaxInstance);
                if (param != null)
                {
                    int value = 0;
                    if (param.GetIntValue(ref value))
                    {
                        this.MaxInstance = (int)value;
                    }
                }
                param = group.GetParam((int)ParamPatternMatchingParametersKey.MinScore);
                if (param != null)
                {
                    double value = 0;
                    if (param.GetDoubleValue(ref value))
                    {
                        this.MinScore = (double)value;
                    }
                }
                param = group.GetParam((int)ParamPatternMatchingParametersKey.DuplicateChecked);
                if (param != null)
                {
                    bool value = false;
                    if (param.GetBoolValue(ref value))
                    {
                        this.DuplicateChecked = (bool)value;
                    }
                }
                param = group.GetParam((int)ParamPatternMatchingParametersKey.UseMaskImage);
                if (param != null)
                {
                    bool value = false;
                    if (param.GetBoolValue(ref value))
                    {
                        this.UseMaskImage = (bool)value;
                    }
                }
            }
        }
    }

    [Serializable]
    public enum ParamMultiPatternMatchingParametersKey
    {
        Tolerance,
        MaxInstance,
        MinScore,
        DuplicateChecked,
        UseMaskImage,
    }


    [Serializable]
    public class MultiPatternMatchingParameters
    {
        public List<VisionImage> TrainImages { get; set; }
        public double MinTolerance { get; set; }
        public double MaxTolerance { get; set; }
        public bool DuplicateChecked { get; set; }
        public int MaxInstance { get; set; }
        public double MinScore { get; set; }
        public RectangleD MaskRegion { get; set; }
        public bool UseMaskImage { get; set; }

        public MultiPatternMatchingParameters()
        {
            TrainImages = new List<VisionImage>();
            //TrainImages.Add(new VisionImage());
            MinTolerance = 0;
            MaxTolerance = 0;
            DuplicateChecked = false;
            MaxInstance = 100;
            MinScore = 0.5;
            MaskRegion = new RectangleD();
            UseMaskImage = false;
        }

        public void SetParameter(PatternMatchingParameters patternMatchingParameter)
        {
            if (patternMatchingParameter != null)
            {
                this.MinTolerance = patternMatchingParameter.MinTolerance;
                this.MaxTolerance = patternMatchingParameter.MaxTolerance;
                this.TrainImages.Add(patternMatchingParameter.TrainImage);
                this.DuplicateChecked = patternMatchingParameter.DuplicateChecked;
                this.MaxInstance = patternMatchingParameter.MaxInstance;
                this.MinScore = patternMatchingParameter.MinScore;
                this.UseMaskImage = patternMatchingParameter.UseMaskImage;
                this.MaskRegion = patternMatchingParameter.MaskRegion;
            }
        }

        public void SetOffsetTolerrance(double dOffset)
        {
            MinTolerance += dOffset;
            MaxTolerance += dOffset;
        }
        private void SetTolerance(double dToelrance)
        {
            this.MinTolerance = dToelrance * -1;
            this.MaxTolerance = dToelrance;
        }
        public MultiPatternMatchingParameters Clone()
        {
            MultiPatternMatchingParameters ret = new MultiPatternMatchingParameters();
            ret.TrainImages = TrainImages;
            ret.MinTolerance = MinTolerance;
            ret.MaxTolerance = MaxTolerance;
            ret.DuplicateChecked = DuplicateChecked;
            ret.MaxInstance = MaxInstance;
            ret.MinScore = MinScore;
            ret.MaskRegion = MaskRegion;
            ret.UseMaskImage = UseMaskImage;

            return ret;
        }

        public ParamGroup GetParamGroup()
        {
            ParamGroup group = new ParamGroup();
            group.Name = this.GetType().Name;            
            {
                Param param = new Param();
                param.SetParam("Tolerance", Param.DisplayTypeKey.Text, MaxTolerance, Param.ValueTypeKey.Double, group.Name);
                group.AddParam(param);
            }            
            {
                Param param = new Param();
                param.SetParam(nameof(MaxInstance), Param.DisplayTypeKey.Text, MaxInstance, Param.ValueTypeKey.Int, group.Name);
                group.AddParam(param);
            }
            {
                Param param = new Param();
                param.SetParam(nameof(MinScore), Param.DisplayTypeKey.Text, MinScore, Param.ValueTypeKey.Double, group.Name);
                group.AddParam(param);
            }
            {
                Param param = new Param();
                param.SetParam(nameof(DuplicateChecked), Param.DisplayTypeKey.CheckBox, DuplicateChecked, Param.ValueTypeKey.Bool, group.Name);
                group.AddParam(param);
            }
            {
                Param param = new Param();
                param.SetParam(nameof(UseMaskImage), Param.DisplayTypeKey.CheckBox, UseMaskImage, Param.ValueTypeKey.Bool, group.Name);
                group.AddParam(param);
            }
            
            return group;
        }

        public void SetParamGroup(ParamGroup group)
        {
            if (group != null)
            {
                Param param = null;                
                param = group.GetParam((int)ParamMultiPatternMatchingParametersKey.Tolerance);
                if (param != null)
                {  
                    double value = 0;
                    if (param.GetDoubleValue(ref value))
                    {
                        this.SetTolerance((double)value);
                    }
                }                
                param = group.GetParam((int)ParamMultiPatternMatchingParametersKey.MaxInstance);
                if (param != null)
                {
                    int value = 0;
                    if (param.GetIntValue(ref value))
                    {
                        this.MaxInstance = (int)value;
                    }
                }
                param = group.GetParam((int)ParamMultiPatternMatchingParametersKey.MinScore);
                if (param != null)
                {
                    double value = 0;
                    if (param.GetDoubleValue(ref value))
                    {
                        this.MinScore = (double)value;
                    }
                }               
                param = group.GetParam((int)ParamMultiPatternMatchingParametersKey.DuplicateChecked);
                if (param != null)
                {
                    bool value = false;
                    if (param.GetBoolValue(ref value))
                    {
                        this.DuplicateChecked = (bool)value;
                    }
                }
                param = group.GetParam((int)ParamMultiPatternMatchingParametersKey.UseMaskImage);
                if (param != null)
                {
                    bool value = false;
                    if (param.GetBoolValue(ref value))
                    {
                        this.UseMaskImage = (bool)value;
                    }
                }
            }
        }
    }
}
