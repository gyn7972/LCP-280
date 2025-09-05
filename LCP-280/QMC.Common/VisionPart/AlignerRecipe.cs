using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Linq;

namespace QMC.Common.VisionPart
{
    [Serializable]
    public enum ParamAlignerRecipeKey
    {
        UseTwoPointAlign,
        UseFourPointAlign,
        UseFourPointAlignNotTheta,
        FiducialXDistance,
        FiducialYDistance,
        AngleTolerance,
        RetryCount,       
        MaxTolerance,
        MinTolerance,
        VerificationEnable,
        EnableThetaCorrection,
        UseSaveThetaCorrectionData,
        ThetaCorrectionAngleTolerance,
        ThetaCorrectionUnitAngle,
        PatternMatchingParameter,

    }

    [Serializable]
    public class AlignerRecipe : BaseRecipe
    {
        [Serializable]
        public enum Direction
        {
            /// <summary>
            /// 반시계 방향.
            /// </summary>
            CounterClockWise,

            /// <summary>
            /// 시계 방향.
            /// </summary>
            ClockWise,
        }
        [Serializable]
        public enum PathType
        {
            Continuous,
            StepByStep,
        }
        [Serializable]
        public enum PositionAligns
        {
            Reference,
            First,
            Second,
            Third,
            Forth
        }
        public Point InspectRoiStartLocation { set; get; }
        public Point InspectRoiEndLocation { set; get; }
        public Point TrainRoiStartLocation { set; get; }
        public Point TrainRoiEndLocation { set; get; }
        //public PathGenerator pathGenerator { set; get; } //2
                                                         //    [TypeConverter(typeof(NormalExpandableObjectConverter))]
                                                         //    public PatternMatchingParameters PatternMatchingParameter { set; get; } //1

        //    public MultiPatternMatchingParameters MultiPatternMatchingParameter { set; get; }

        //    //    public PathParameter PathParameter { set; get; }
        //    [TypeConverter(typeof(NormalExpandableObjectConverter))]


        //    [TypeConverter(typeof(XyPositionExpandableObjectConverter))]
        //    public XyCoordinate FirstFiducialmark { set; get; }
        //    [TypeConverter(typeof(XyPositionExpandableObjectConverter))]
        //    public XyCoordinate SecondFiducialmark { set; get; }
        //    [TypeConverter(typeof(XyPositionExpandableObjectConverter))]
        //    public XyCoordinate ThirdFiducialmark { set; get; }
        //    [TypeConverter(typeof(XyPositionExpandableObjectConverter))]
        //    public XyCoordinate FourthFiducialmark { set; get; }

        //    public bool VerificationEnable { get; set; }
        //    public double AngleTolerance { set; get; }
        //    public int RetryCount { set; get; }
        //    public double ThetaCorrectionAngleTolerance { set; get; }
        //    public double ThetaCorrectionUnitAngle { set; get; }
        //    public bool EnableThetaCorrection { set; get; }

        //    public bool UseSaveThetaCorrectionData { set; get; }

        //    public double MaxTolerance { set; get; }

        //    public double MinTolerance { set; get; }

        //    public bool UseFourPointAlign { set; get; }
        //    public bool UseFourPointAlignNotTheta { set; get; }
        //    public bool UseTwoPointAlign { get; set; }

        //    //public Aligner Aligner;
        //    //[TypeConverter(typeof(XyPositionExpandableObjectConverter))]
        //    [TypeConverter(typeof(NormalExpandableObjectConverter))]
        //    public XytPositionDataCollection AlignPositions { set; get; }

        //    [TypeConverter(typeof(NormalExpandableObjectConverter))]
        //    public IlluminationDataSet IlluminationDataSet { set; get; }

        //    public double FiducialXDistance { set; get; }

        //    public double FiducialYDistance { set; get; }

        //    public AlignerRecipe(Part part)
        //    {
        //        Init(part);

        //        FirstFiducialmark = new XyCoordinate(0, 0);
        //        SecondFiducialmark = new XyCoordinate(10, 1);
        //        ThirdFiducialmark = new XyCoordinate(10, 11);
        //        FourthFiducialmark = new XyCoordinate(0, 10);
        //        FiducialXDistance = 0.0;
        //        FiducialYDistance = 0.0;
        //        RetryCount = 1;
        //        AngleTolerance = 1;

        //        this.VerificationEnable = false;
        //        ThetaCorrectionAngleTolerance = 3;
        //        ThetaCorrectionUnitAngle = 1;
        //        EnableThetaCorrection = false;
        //        UseFourPointAlign = false;
        //        UseFourPointAlignNotTheta = false;
        //        UseTwoPointAlign = true;

        //        InspectRoiStartLocation = new Point();
        //        InspectRoiEndLocation = new Point();
        //        TrainRoiStartLocation = new Point();
        //        TrainRoiEndLocation = new Point();

        //        UseSaveThetaCorrectionData = false;
        //        MaxTolerance = 0;
        //        MinTolerance = 0;
        //    }

        //    public void Init(Part part)
        //    {
        //        if (PatternMatchingParameter == null)
        //        {
        //            PatternMatchingParameter = new PatternMatchingParameters();
        //        }

        //        if (pathGenerator == null)
        //            pathGenerator = new PathGenerator();
        //        if (IlluminationDataSet == null)
        //            IlluminationDataSet = new IlluminationDataSet(part.Name);

        //        if (AlignPositions == null)
        //        {
        //            AlignPositions = new XytPositionDataCollection();
        //            foreach (PositionAligns key in Enum.GetValues(typeof(PositionAligns)))
        //            {
        //                XytPositionData positionBase = new XytPositionData();
        //                positionBase.Name = key.ToString();
        //                //positionBase.Coordinate = 
        //                AlignPositions.Add(positionBase);

        //                XytPositionData positionTarget = new XytPositionData();
        //                positionTarget.Name = key.ToString();
        //                positionTarget.Type = TargetType.Offset;
        //                AlignPositions.Add(positionTarget);
        //            }
        //        }

        //        if(MultiPatternMatchingParameter == null)
        //        {
        //            MultiPatternMatchingParameter = new MultiPatternMatchingParameters();

        //            if(PatternMatchingParameter != null)
        //            {
        //                MultiPatternMatchingParameter.SetParameter(PatternMatchingParameter);
        //            }
        //        }
        //    }

        //    public List<string> GetAlignsPositionList()
        //    {
        //        List<string> ret = new List<string>();
        //        foreach (XytPositionData position in AlignPositions)
        //        {
        //            if (position.Type == TargetType.Base)
        //                ret.Add(position.Name);
        //        }
        //        return ret;
        //    }

        //    public void SetAlignPositionData(string strPosition, TargetType targetType, XytCoordinate coordinate)
        //    {
        //        foreach (XytPositionData position in AlignPositions)
        //        {
        //            if (position.Name == strPosition && position.Type == targetType)
        //            {
        //                position.X = coordinate.X;
        //                position.Y = coordinate.Y;
        //                position.T = coordinate.T;
        //                break;
        //            }
        //        }
        //    }


        //    public XytCoordinate GetAlignPositionData(string strPosition)
        //    {
        //        XytCoordinate coordinate = new XytCoordinate();

        //        foreach (XytPositionData position in AlignPositions)
        //        {
        //            if (position.Name == strPosition)
        //            {
        //                coordinate += position.Coordinate;
        //                break;
        //            }
        //        }

        //        return coordinate;
        //    }

        //    public void UpdateFiducialMark()
        //    {
        //        if (AlignPositions != null && AlignPositions.Count != 0)
        //        {
        //            this.FirstFiducialmark = (XyCoordinate)AlignPositions.GetPositionCoordinate(PositionAligns.First.ToString());
        //            this.SecondFiducialmark = (XyCoordinate)AlignPositions.GetPositionCoordinate(PositionAligns.Second.ToString());
        //            this.ThirdFiducialmark = (XyCoordinate)AlignPositions.GetPositionCoordinate(PositionAligns.Third.ToString());
        //            this.FourthFiducialmark = (XyCoordinate)AlignPositions.GetPositionCoordinate(PositionAligns.Forth.ToString());
        //        }
        //    }

        //    public override ListParam ToListParam()
        //    {
        //        ListParam listParam = new ListParam();
        //        ParamGroup paramGroup = new ParamGroup();
        //        paramGroup.Name = this.GetType().Name;
        //        if (listParam != null)
        //        {
        //            {
        //                Param param = new Param();
        //                param.SetParam(nameof(UseTwoPointAlign), Param.DisplayTypeKey.CheckBox, UseTwoPointAlign, Param.ValueTypeKey.Bool, paramGroup.Name);
        //                paramGroup.AddParam(param);
        //            }
        //            {
        //                Param param = new Param();
        //                param.SetParam(nameof(UseFourPointAlign), Param.DisplayTypeKey.CheckBox, UseFourPointAlign, Param.ValueTypeKey.Bool, paramGroup.Name);
        //                paramGroup.AddParam(param);
        //            }
        //            {
        //                Param param = new Param();
        //                param.SetParam(nameof(UseFourPointAlignNotTheta), Param.DisplayTypeKey.CheckBox, UseFourPointAlignNotTheta, Param.ValueTypeKey.Bool, paramGroup.Name);
        //                paramGroup.AddParam(param);
        //            }                
        //            {
        //                Param param = new Param();
        //                param.SetParam(nameof(FiducialXDistance), Param.DisplayTypeKey.Text, FiducialXDistance, Param.ValueTypeKey.Double, paramGroup.Name);
        //                paramGroup.AddParam(param);
        //            }
        //            {
        //                Param param = new Param();
        //                param.SetParam(nameof(FiducialYDistance), Param.DisplayTypeKey.Text, FiducialYDistance, Param.ValueTypeKey.Double, paramGroup.Name);
        //                paramGroup.AddParam(param);
        //            }
        //            {
        //                Param param = new Param();
        //                param.SetParam(nameof(AngleTolerance), Param.DisplayTypeKey.Text, AngleTolerance, Param.ValueTypeKey.Double, paramGroup.Name);
        //                paramGroup.AddParam(param);
        //            }
        //            {
        //                Param param = new Param();
        //                param.SetParam(nameof(RetryCount), Param.DisplayTypeKey.Text, RetryCount, Param.ValueTypeKey.Int, paramGroup.Name);
        //                paramGroup.AddParam(param);
        //            }             
        //            {
        //                Param param = new Param();
        //                param.SetParam(nameof(MaxTolerance), Param.DisplayTypeKey.Text, MaxTolerance, Param.ValueTypeKey.Double, paramGroup.Name);
        //                paramGroup.AddParam(param);
        //            }
        //            {
        //                Param param = new Param();
        //                param.SetParam(nameof(MinTolerance), Param.DisplayTypeKey.Text, MinTolerance, Param.ValueTypeKey.Double, paramGroup.Name);
        //                paramGroup.AddParam(param);
        //            }
        //            {
        //                Param param = new Param();
        //                param.SetParam(nameof(VerificationEnable), Param.DisplayTypeKey.CheckBox, VerificationEnable, Param.ValueTypeKey.Bool, paramGroup.Name);
        //                paramGroup.AddParam(param);
        //            }
        //            {
        //                Param param = new Param();
        //                param.SetParam(nameof(EnableThetaCorrection), Param.DisplayTypeKey.CheckBox, EnableThetaCorrection, Param.ValueTypeKey.Bool, paramGroup.Name);
        //                paramGroup.AddParam(param);
        //            }
        //            {
        //                Param param = new Param();
        //                param.SetParam(nameof(UseSaveThetaCorrectionData), Param.DisplayTypeKey.CheckBox, UseSaveThetaCorrectionData, Param.ValueTypeKey.Bool, paramGroup.Name);
        //                paramGroup.AddParam(param);
        //            }
        //            {
        //                Param param = new Param();
        //                param.SetParam(nameof(ThetaCorrectionAngleTolerance), Param.DisplayTypeKey.Text, ThetaCorrectionAngleTolerance, Param.ValueTypeKey.Double, paramGroup.Name);
        //                paramGroup.AddParam(param);
        //            }
        //            {
        //                Param param = new Param();
        //                param.SetParam(nameof(ThetaCorrectionUnitAngle), Param.DisplayTypeKey.Text, ThetaCorrectionUnitAngle, Param.ValueTypeKey.Double, paramGroup.Name);
        //                paramGroup.AddParam(param);
        //            }
        //            {
        //                ParamGroup pathGeneratorGroup = new ParamGroup();
        //                if (this.pathGenerator != null)
        //                {
        //                    pathGeneratorGroup = pathGenerator.GetGroup();
        //                    paramGroup.SetGroup(pathGeneratorGroup);
        //                }
        //            }
        //        }
        //        listParam.SetGroup(paramGroup);
        //        return listParam;
        //    }

        //    public override void SetParam(ListParam listParam)
        //    {
        //        ParamGroup group = listParam.GetGroup(this.GetType().Name);
        //        if (group != null)
        //        {
        //            Param param = null;
        //            param = group.GetParam((int)ParamAlignerRecipeKey.UseTwoPointAlign);
        //            if (param != null)
        //            {
        //                bool value = false;
        //                if (param.GetBoolValue(ref value))
        //                {
        //                    this.UseTwoPointAlign = value;
        //                }
        //            }
        //            param = group.GetParam((int)ParamAlignerRecipeKey.UseFourPointAlign);
        //            if (param != null)
        //            {
        //                bool value = false;
        //                if (param.GetBoolValue(ref value))
        //                {
        //                    this.UseFourPointAlign = value;
        //                }
        //            }
        //            param = group.GetParam((int)ParamAlignerRecipeKey.UseFourPointAlignNotTheta);
        //            if (param != null)
        //            {
        //                bool value = false;
        //                if (param.GetBoolValue(ref value))
        //                {
        //                    this.UseFourPointAlignNotTheta = value;
        //                }
        //            }               
        //            param = group.GetParam((int)ParamAlignerRecipeKey.FiducialXDistance);
        //            if (param != null)
        //            {
        //                double value = 0;
        //                if (param.GetDoubleValue(ref value))
        //                {
        //                    FiducialXDistance = value;
        //                }
        //            }
        //            param = group.GetParam((int)ParamAlignerRecipeKey.FiducialYDistance);
        //            if (param != null)
        //            {
        //                double value = 0;
        //                if (param.GetDoubleValue(ref value))
        //                {
        //                    this.FiducialYDistance = value;
        //                }
        //            }
        //            param = group.GetParam((int)ParamAlignerRecipeKey.AngleTolerance);
        //            if (param != null)
        //            {
        //                double value = 0;
        //                if (param.GetDoubleValue(ref value))
        //                {
        //                    this.AngleTolerance = value;
        //                }
        //            }
        //            param = group.GetParam((int)ParamAlignerRecipeKey.RetryCount);
        //            if (param != null)
        //            {
        //                int value = 0;
        //                if (param.GetIntValue(ref value))
        //                {
        //                    this.RetryCount = value;
        //                }
        //            }
        //            param = group.GetParam((int)ParamAlignerRecipeKey.MaxTolerance);
        //            if (param != null)
        //            {
        //                double value = 0;
        //                if (param.GetDoubleValue(ref value))
        //                {
        //                    this.MaxTolerance = value;
        //                }
        //            }
        //            param = group.GetParam((int)ParamAlignerRecipeKey.MinTolerance);
        //            if (param != null)
        //            {
        //                double value = 0;
        //                if (param.GetDoubleValue(ref value))
        //                {
        //                    this.MinTolerance = value;
        //                }
        //            }
        //            param = group.GetParam((int)ParamAlignerRecipeKey.VerificationEnable);
        //            if (param != null)
        //            {
        //                bool value = false;
        //                if (param.GetBoolValue(ref value))
        //                {
        //                    this.VerificationEnable = value;
        //                }
        //            }
        //            param = group.GetParam((int)ParamAlignerRecipeKey.EnableThetaCorrection);
        //            if (param != null)
        //            {
        //                bool value = false;
        //                if (param.GetBoolValue(ref value))
        //                {
        //                    this.EnableThetaCorrection = value;
        //                }
        //            }
        //            param = group.GetParam((int)ParamAlignerRecipeKey.UseSaveThetaCorrectionData);
        //            if (param != null)
        //            {
        //                bool value = false;
        //                if (param.GetBoolValue(ref value))
        //                {
        //                    this.UseSaveThetaCorrectionData = value;
        //                }
        //            }
        //            param = group.GetParam((int)ParamAlignerRecipeKey.ThetaCorrectionAngleTolerance);
        //            if (param != null)
        //            {
        //                double value = 0;
        //                if (param.GetDoubleValue(ref value))
        //                {
        //                    this.ThetaCorrectionAngleTolerance = value;
        //                }
        //            }
        //            param = group.GetParam((int)ParamAlignerRecipeKey.ThetaCorrectionUnitAngle);
        //            if (param != null)
        //            {
        //                double value = 0;
        //                if (param.GetDoubleValue(ref value))
        //                {
        //                    this.ThetaCorrectionUnitAngle = value;
        //                }
        //            }

        //            {
        //                ParamGroup pathGeneratorGroup = group.GetGroup(pathGenerator.GetType().Name);
        //                if (pathGeneratorGroup != null)
        //                {
        //                    if (this.pathGenerator != null)
        //                    {
        //                        this.pathGenerator.SetGroup(pathGeneratorGroup);
        //                    }
        //                }
        //            }
        //        }
        //    }


        //    public override List<object> GetPositions()
        //    {
        //        List<object> positions = new List<object>();
        //        positions.Add(AlignPositions);

        //        return positions;
        //    }
        //    public override IlluminationDataList GetIlluminationDatas()
        //    {
        //        IlluminationDataList IlluminationrList = new IlluminationDataList();
        //        IlluminationrList.Add(IlluminationDataSet);
        //        return IlluminationrList;
        //    }
        //}

        //[Serializable]
        //public enum ParamPathParameterKey
        //{
        //    CenterCoordinate,
        //    Direction,
        //    PathType,
        //    PitchDistance,
        //    Area,
        //    PitchCount,
        //    InvertedX,
        //    InvertedY,
        //}

        //[Serializable]
        //public class PathParameter
        //{
        //    public XyCoordinate CenterCoordinate { set; get; }
        //    public AlignerRecipe.Direction Direction { get; set; }
        //    public AlignerRecipe.PathType PathType { get; set; }
        //    public SizeD PitchDistance { get; set; }
        //    public SizeD Area { get; set; }
        //    public Size PitchCount { get; set; }
        //    public bool InvertedX { set; get; }
        //    public bool InvertedY { set; get; }
        //    public PathParameter()
        //    {
        //        CenterCoordinate = new XyCoordinate(0, 0);
        //        Direction = AlignerRecipe.Direction.ClockWise;
        //        PathType = AlignerRecipe.PathType.StepByStep;
        //        PitchDistance = new SizeD(10, 10);
        //        Area = new SizeD(10, 10);
        //        PitchCount = new Size(10, 10);
        //        InvertedX = false;
        //        InvertedY = false;
        //    }
        //    public ParamGroup GetGroup()
        //    {
        //        ParamGroup group = new ParamGroup();
        //        group.Name = this.GetType().Name;
        //        {
        //            Param param = new Param();
        //            param.SetParam(nameof(CenterCoordinate), Param.DisplayTypeKey.Text, CenterCoordinate, Param.ValueTypeKey.XY_Coordinate, group.Name);
        //            group.AddParam(param);
        //        }
        //        {
        //            Param param = new Param();
        //            param.SetParam(nameof(Direction), Param.DisplayTypeKey.Combobox, Direction, Param.ValueTypeKey.Int, group.Name);

        //            param.SelectValues.Clear();

        //            foreach (Enum e in Enum.GetValues(typeof(AlignerRecipe.Direction)))
        //            {
        //                param.SelectValues.Add(e.ToString());
        //            }

        //            group.AddParam(param);
        //        }
        //        {
        //            Param param = new Param();
        //            param.SetParam(nameof(PathType), Param.DisplayTypeKey.Combobox, PathType, Param.ValueTypeKey.Int, group.Name);
        //            foreach (Enum e in Enum.GetValues(typeof(AlignerRecipe.PathType)))
        //            {
        //                param.SelectValues.Add(e.ToString());
        //            }
        //            group.AddParam(param);
        //        }
        //        {
        //            Param param = new Param();
        //            param.SetParam(nameof(PitchDistance), Param.DisplayTypeKey.Text, PitchDistance, Param.ValueTypeKey.SizeD, group.Name);
        //            group.AddParam(param);
        //        }
        //        {
        //            Param param = new Param();
        //            param.SetParam(nameof(Area), Param.DisplayTypeKey.Text, Area, Param.ValueTypeKey.SizeD, group.Name);
        //            group.AddParam(param);
        //        }
        //        {
        //            Param param = new Param();
        //            param.SetParam(nameof(PitchCount), Param.DisplayTypeKey.Text, PitchCount, Param.ValueTypeKey.Size, group.Name);
        //            group.AddParam(param);
        //        }
        //        {
        //            Param param = new Param();
        //            param.SetParam(nameof(InvertedX), Param.DisplayTypeKey.CheckBox, InvertedX, Param.ValueTypeKey.Bool, group.Name);
        //            group.AddParam(param);
        //        }
        //        {
        //            Param param = new Param();
        //            param.SetParam(nameof(InvertedY), Param.DisplayTypeKey.CheckBox, InvertedY, Param.ValueTypeKey.Bool, group.Name);
        //            group.AddParam(param);
        //        }
        //        return group;
        //    }

        //    public void SetGroup(ParamGroup group)
        //    {
        //        if (group != null)
        //        {
        //            Param param = null;
        //            param = group.GetParam((int)ParamPathParameterKey.CenterCoordinate);
        //            if (param != null)
        //            {
        //                XyCoordinate value = new XyCoordinate();
        //                if (param.GetXYValue(ref value))
        //                {
        //                    this.CenterCoordinate = (XyCoordinate)value;
        //                }
        //            }
        //            param = group.GetParam((int)ParamPathParameterKey.Direction);
        //            if (param != null)
        //            {
        //                int value = 0;
        //                if (param.GetIntValue(ref value))
        //                {
        //                    this.Direction = (AlignerRecipe.Direction)value;
        //                }
        //            }
        //            param = group.GetParam((int)ParamPathParameterKey.PathType);
        //            if (param != null)
        //            {
        //                int value = 0;
        //                if (param.GetIntValue(ref value))
        //                {
        //                    this.PathType = (AlignerRecipe.PathType)value;
        //                }
        //            }
        //            param = group.GetParam((int)ParamPathParameterKey.PitchDistance);
        //            if (param != null)
        //            {
        //                SizeD value = new SizeD();
        //                if (param.GetSizeDValue(ref value))
        //                {
        //                    this.PitchDistance = (SizeD)value;
        //                }
        //            }
        //            param = group.GetParam((int)ParamPathParameterKey.Area);
        //            if (param != null)
        //            {
        //                SizeD value = new SizeD();
        //                if (param.GetSizeDValue(ref value))
        //                {
        //                    this.Area = (SizeD)value;
        //                }
        //            }
        //            param = group.GetParam((int)ParamPathParameterKey.PitchCount);
        //            if (param != null)
        //            {
        //                Size value = new Size();
        //                if (param.GetSizeValue(ref value))
        //                {
        //                    this.PitchCount = (Size)value;
        //                }
        //            }
        //            param = group.GetParam((int)ParamPathParameterKey.InvertedX);
        //            if (param != null)
        //            {
        //                bool value = false;
        //                if (param.GetBoolValue(ref value))
        //                {
        //                    this.InvertedX = (bool)value;
        //                }
        //            }
        //            param = group.GetParam((int)ParamPathParameterKey.InvertedY);
        //            if (param != null)
        //            {
        //                bool value = false;
        //                if (param.GetBoolValue(ref value))
        //                {
        //                    this.InvertedY = (bool)value;
        //                }
        //            }
        //        }
        //    }
        //}
        //[Serializable]
        //public enum ParamPathGeneratorKey // 작업중
        //{
        //    Paths,
        //    PathParameter,
        //}
        //[Serializable]
        //public class PathGenerator
        //{
        //    [TypeConverter(typeof(NormalExpandableObjectConverter))]
        //    public PathParameter PathParameter { set; get; }


        //    public List<XyCoordinate> Paths { get; set; }
        //    public PathGenerator()
        //    {
        //        //     PathParameter = new PathParameter();
        //        Paths = new List<XyCoordinate>();
        //        PathParameter = new PathParameter();
        //    }

        //    protected int OnGenerate(PathParameter paramter)
        //    {
        //        return this.Run(paramter);
        //    }
        //    private int Run(PathParameter parameter)
        //    {//?
        //        int ret = 0;
        //        int directionX = 1;
        //        int directionY = 1;
        //        int loopCount = 0;
        //        List<XyCoordinate> paths = new List<XyCoordinate>(parameter.PitchCount.Width * parameter.PitchCount.Height);
        //        XyCoordinate currentCoordinate = new XyCoordinate(parameter.CenterCoordinate.X, parameter.CenterCoordinate.Y);
        //        int offsetX = 0;
        //        int offsetY = 0;
        //        paths.Add(currentCoordinate);
        //        #region Direction 및 시작 위치 지정
        //        // 시계방향이면서 N x M에서 N이 M보다 크거나 같을때
        //        if (parameter.Direction == AlignerRecipe.Direction.ClockWise && parameter.PitchCount.Height <= parameter.PitchCount.Width)
        //        {
        //            // X : + , Y : +
        //            directionX = 1;
        //            directionY = 1;
        //            //시작위치는 왼쪽 상단.
        //            currentCoordinate = new XyCoordinate(currentCoordinate.X - Math.Truncate((parameter.PitchCount.Width - parameter.PitchCount.Height) / 2 * parameter.PitchDistance.Width), currentCoordinate.Y);
        //        }
        //        // 시계방향이면서 N x M에서 M이 N보다 클때
        //        else if (parameter.Direction == AlignerRecipe.Direction.ClockWise && parameter.PitchCount.Width < parameter.PitchCount.Height)
        //        {
        //            // X : + , Y : -
        //            directionX = 1;
        //            directionY = -1;
        //            // 시작 위치는 왼쪽 하단.
        //            currentCoordinate = new XyCoordinate(currentCoordinate.X, currentCoordinate.Y + Math.Truncate((parameter.PitchCount.Height - parameter.PitchCount.Width) / 2 * parameter.PitchDistance.Height));
        //        }
        //        // 반시계방향이면서 N x M에서 N이 M보다 크거나 같을때
        //        else if (parameter.Direction == AlignerRecipe.Direction.CounterClockWise && parameter.PitchCount.Height <= parameter.PitchCount.Width)
        //        {
        //            // X : - , Y : +
        //            directionX = -1;
        //            directionY = 1;
        //            // 시작 위치는 오른쪽 상단
        //            currentCoordinate = new XyCoordinate(currentCoordinate.X + Math.Truncate((parameter.PitchCount.Width - parameter.PitchCount.Height) / 2 * parameter.PitchDistance.Width), currentCoordinate.Y);
        //        }
        //        // 반시계방향이면서 N x M에서 M이 N보다 클때
        //        else if (parameter.Direction == AlignerRecipe.Direction.CounterClockWise && parameter.PitchCount.Width < parameter.PitchCount.Height)
        //        {
        //            // X : - , Y : -
        //            directionX = -1;
        //            directionY = -1;
        //            // 시작 위치는 오른쪽 하단
        //            currentCoordinate = new XyCoordinate(currentCoordinate.X, currentCoordinate.Y + Math.Truncate((parameter.PitchCount.Height - parameter.PitchCount.Width) / 2 * parameter.PitchDistance.Height));
        //        }

        //        // 짝수일 경우 처리
        //        if (parameter.PitchCount.Width % 2 == 0)
        //            currentCoordinate -= new XyCoordinate(parameter.PitchDistance.Width / 2, 0);
        //        if (parameter.PitchCount.Height % 2 == 0)
        //            currentCoordinate -= new XyCoordinate(0, parameter.PitchDistance.Height / 2);

        //        paths.Add(currentCoordinate);
        //        #endregion

        //        #region Spiral 방식으로 Loop 시작
        //        // 가로의 횟수가 높을 경우.
        //        if (parameter.PitchCount.Height < parameter.PitchCount.Width)
        //        {

        //            loopCount = parameter.PitchCount.Height;

        //            for (int i = 1; i < loopCount + 1; i++)
        //            {
        //                // 마지막 루프에서는 loopCount를 1회 낮추기 위해 Offset값을 1로 변경.
        //                if (i == loopCount && loopCount % 2 == 0) offsetX = 1;

        //                if (parameter.PathType == AlignerRecipe.PathType.StepByStep)
        //                {
        //                    // 가로의 횟수가 높기 때문에 가로부터 진행.
        //                    for (int j = 0; j < i - offsetX + (parameter.PitchCount.Width - (int)Math.Ceiling(parameter.PitchCount.Height / 2.0) * 2); j++)
        //                    {
        //                        currentCoordinate = new XyCoordinate(currentCoordinate.X + parameter.PitchDistance.Width * directionX, currentCoordinate.Y);
        //                        paths.Add(currentCoordinate);
        //                    }
        //                }
        //                else if (parameter.PathType == AlignerRecipe.PathType.Continuous)
        //                {
        //                    currentCoordinate = new XyCoordinate(currentCoordinate.X + parameter.PitchDistance.Width * directionX * (i - offsetX + (parameter.PitchCount.Width - (int)Math.Round(parameter.PitchCount.Height / 2.0) * 2)), currentCoordinate.Y);
        //                    paths.Add(currentCoordinate);
        //                }

        //                // 마지막 루프에서는 height쪽으로 Path를 생성하지 않음.
        //                if (i == loopCount && loopCount % 2 == 0) break;
        //                if (i == loopCount && loopCount % 2 != 0) offsetY = 1;

        //                if (parameter.PathType == AlignerRecipe.PathType.StepByStep)
        //                {
        //                    for (int j = 0; j < i - offsetY; j++)
        //                    {
        //                        currentCoordinate = new XyCoordinate(currentCoordinate.X, currentCoordinate.Y + parameter.PitchDistance.Height * directionY);
        //                        paths.Add(currentCoordinate);
        //                    }
        //                }
        //                else if (parameter.PathType == AlignerRecipe.PathType.Continuous)
        //                {
        //                    currentCoordinate = new XyCoordinate(currentCoordinate.X, currentCoordinate.Y + parameter.PitchDistance.Height * directionY * (i - offsetY));
        //                    paths.Add(currentCoordinate);
        //                }

        //                directionX *= -1;
        //                directionY *= -1;
        //            }
        //        }
        //        // 세로의 횟수가 높을 경우.
        //        else if (parameter.PitchCount.Width < parameter.PitchCount.Height)
        //        {
        //            loopCount = parameter.PitchCount.Width;

        //            for (int i = 1; i < loopCount + 1; i++)
        //            {
        //                // 마지막 루프에서는 loopCount를 1회 낮추기 위해 Offset값을 1로 변경.
        //                if (i == loopCount && loopCount % 2 == 0) offsetY = 1;

        //                if (parameter.PathType == AlignerRecipe.PathType.StepByStep)
        //                {
        //                    // 세로 횟수가 높기 때문에 세로부터 진행.
        //                    for (int j = 0; j < i - offsetY + (parameter.PitchCount.Height - (int)Math.Ceiling(parameter.PitchCount.Width / 2.0) * 2); j++)
        //                    {
        //                        currentCoordinate = new XyCoordinate(currentCoordinate.X, currentCoordinate.Y + parameter.PitchDistance.Height * directionY);
        //                        paths.Add(currentCoordinate);
        //                    }
        //                }
        //                else if (parameter.PathType == AlignerRecipe.PathType.Continuous)
        //                {
        //                    currentCoordinate = new XyCoordinate(currentCoordinate.X, currentCoordinate.Y + parameter.PitchDistance.Height * directionY * (i - offsetY + (parameter.PitchCount.Height - (int)Math.Round(parameter.PitchCount.Width / 2.0) * 2)));
        //                    paths.Add(currentCoordinate);
        //                }

        //                // 마지막 루프에서는 Width쪽으로 Path를 생성하지 않음.
        //                if (i == loopCount && loopCount % 2 == 0) break;
        //                if (i == loopCount && loopCount % 2 != 0) offsetX = 1;

        //                if (parameter.PathType == AlignerRecipe.PathType.StepByStep)
        //                {
        //                    for (int j = 0; j < i - offsetX; j++)
        //                    {
        //                        currentCoordinate = new XyCoordinate(currentCoordinate.X + parameter.PitchDistance.Width * directionX, currentCoordinate.Y);
        //                        paths.Add(currentCoordinate);
        //                    }
        //                }
        //                else if (parameter.PathType == AlignerRecipe.PathType.Continuous)
        //                {
        //                    currentCoordinate = new XyCoordinate(currentCoordinate.X + parameter.PitchDistance.Width * directionX * (i - offsetX), currentCoordinate.Y);
        //                    paths.Add(currentCoordinate);
        //                }

        //                directionX *= -1;
        //                directionY *= -1;
        //            }
        //        }
        //        // 가로, 세로의 횟수가 같을 경우.
        //        else if (parameter.PitchCount.Width == parameter.PitchCount.Height)
        //        {
        //            loopCount = parameter.PitchCount.Width;

        //            for (int i = 1; i <= loopCount + 1; i++)
        //            {
        //                // 마지막 루프에서는 loopCount를 1회 낮추기 위해 Offset값을 1로 변경.
        //                if (i == loopCount) offsetX = 1;

        //                if (parameter.PathType == AlignerRecipe.PathType.StepByStep)
        //                {
        //                    for (int j = 0; j < i - offsetX; j++)
        //                    {
        //                        currentCoordinate = new XyCoordinate(currentCoordinate.X + parameter.PitchDistance.Width * directionX, currentCoordinate.Y);
        //                        paths.Add(currentCoordinate);
        //                    }
        //                }
        //                else if (parameter.PathType == AlignerRecipe.PathType.Continuous)
        //                {
        //                    currentCoordinate = new XyCoordinate(currentCoordinate.X + parameter.PitchDistance.Width * directionX * (i - offsetX), currentCoordinate.Y);
        //                    paths.Add(currentCoordinate);
        //                }

        //                // 마지막 루프에서는 height쪽으로 Path를 생성하지 않음.
        //                if (i == loopCount) break;

        //                if (parameter.PathType == AlignerRecipe.PathType.StepByStep)
        //                {
        //                    for (int j = 0; j < i - offsetY; j++)
        //                    {
        //                        currentCoordinate = new XyCoordinate(currentCoordinate.X, currentCoordinate.Y + parameter.PitchDistance.Height * directionY);
        //                        paths.Add(currentCoordinate);
        //                    }
        //                }
        //                else if (parameter.PathType == AlignerRecipe.PathType.Continuous)
        //                {
        //                    currentCoordinate = new XyCoordinate(currentCoordinate.X, currentCoordinate.Y + parameter.PitchDistance.Height * directionY * (i - offsetY));
        //                    paths.Add(currentCoordinate);
        //                }

        //                directionX *= -1;
        //                directionY *= -1;
        //            }
        //        }
        //        #endregion

        //        this.Paths = paths;

        //        return ret;
        //    }


        //    public PathParameter GetParameter()
        //    {
        //        return (PathParameter)CopyUtility.GetDeepCopy(PathParameter);
        //    }


        //    public int Generate(PathParameter paramter)
        //    {
        //        int ret = 0;

        //        this.PathParameter = paramter;

        //        if ((ret = this.OnGenerate(paramter)) != 0) return ret;

        //        return ret;
        //    }

        //    public ParamGroup GetGroup()
        //    {
        //        ParamGroup paramGroupPathParameter = new ParamGroup();
        //        paramGroupPathParameter.Name = this.GetType().Name;
        //        //{
        //        //    ParamGroup parmaGroupPaths = new ParamGroup();
        //        //    parmaGroupPaths.Name = Paths.GetType().Name;
        //        //    int nindex = 1;
        //        //    foreach (XyCoordinate xyCoordinate in Paths)
        //        //    {
        //        //        Param param = new Param();
        //        //        string strPathNo = string.Format("Path.{0}", nindex);
        //        //        param.SetParam(strPathNo, Param.DisplayTypeKey.Text, xyCoordinate, Param.ValueTypeKey.XY_Coordinate, parmaGroupPaths.Name);
        //        //        parmaGroupPaths.AddParam(param);
        //        //        nindex++;
        //        //    }
        //        //    paramGroupPathParameter.AddGroup(parmaGroupPaths);
        //        //}

        //        {
        //            ParamGroup pathParameterGroup = new ParamGroup();
        //            if (this.PathParameter != null)
        //            {
        //                pathParameterGroup = PathParameter.GetGroup();
        //                paramGroupPathParameter.SetGroup(pathParameterGroup);
        //            }
        //        }

        //        return paramGroupPathParameter;
        //    }

        //    public void SetGroup(ParamGroup group)
        //    {
        //        if (group != null)
        //        {
        //            //{
        //            //    ParamGroup parmaGroupPaths = group.GetGroup(Paths.GetType().Name);
        //            //    if (parmaGroupPaths != null)
        //            //    {
        //            //        for (int i = 0; i < Paths.Count; i++)
        //            //        {
        //            //            XyCoordinate xyCoordinate = new XyCoordinate();
        //            //            Param paramXyCorrdinate = parmaGroupPaths.GetParam(i);
        //            //            if (paramXyCorrdinate != null)
        //            //            {
        //            //                if (paramXyCorrdinate.GetXYValue(ref xyCoordinate))
        //            //                {
        //            //                    Paths[i] = xyCoordinate;
        //            //                }
        //            //            }
        //            //        }
        //            //    }
        //            //}

        //            {
        //                ParamGroup pathParameterGroup = group.GetGroup(PathParameter.GetType().Name);
        //                if (pathParameterGroup != null)
        //                {
        //                    if (this.PathParameter != null)
        //                    {
        //                        this.PathParameter.SetGroup(pathParameterGroup);
        //                    }
        //                }
        //            }
        //        }
        //    }
    }
    //public class PathGeneratorCollection : Collection<PathGenerator>
    //{

    //}
    //public class PathParameterCollection : Collection<PathParameter>
    //{

    //}
}
