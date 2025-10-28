using QMC.Common.Vision;
using QMC.Common.Vision.Cognex;
using QMC.Common.Vision.Tools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace QMC.Common.VisionPart
{
    [Serializable]
    public class ThetaCorrectionCoordinate
    {

        public double RealAngle { set; get; }
        public double CommandAngle { set; get; }
        public XytCoordinateCollection Coordinates { set; get; }

        public QMCMatrix CorrectionMatrix { set; get; }


        public ThetaCorrectionCoordinate()
        {
            RealAngle = 0;
            CommandAngle = 0;
            Coordinates = new XytCoordinateCollection();
            CorrectionMatrix = null;
        }
        public override string ToString()
        {
            string strData;

            strData = string.Format("{0}, {1}, {2},{3},{4},{5}", RealAngle, CommandAngle, Coordinates[0].ToString(), Coordinates[1].ToString(), Coordinates[2].ToString(), Coordinates[3].ToString());

            return strData;
        }

        public void SetStringData(string strData)
        {
            string str = string.Join("", strData.Split('[', ']'));
            List<string> listData = str.Split(',').ToList();

            if (listData.Count > 0)
            {
                RealAngle = double.Parse(listData[0]);
                CommandAngle = double.Parse(listData[1]);
                Coordinates.Add(new XytCoordinate(double.Parse(listData[2]), double.Parse(listData[3]), double.Parse(listData[4])));
                Coordinates.Add(new XytCoordinate(double.Parse(listData[5]), double.Parse(listData[6]), double.Parse(listData[7])));
                Coordinates.Add(new XytCoordinate(double.Parse(listData[8]), double.Parse(listData[9]), double.Parse(listData[10])));
                Coordinates.Add(new XytCoordinate(double.Parse(listData[11]), double.Parse(listData[12]), double.Parse(listData[13])));

            }

        }
        public void GenerateMatrix(XytCoordinateCollection coordinatesSource)
        {
            if (Coordinates != null)
            {
                PerspectiveProjection perspectiveProjection = new PerspectiveProjection();
                CorrectionMatrix = perspectiveProjection.projection_matrix(coordinatesSource, Coordinates);
            }

        }
    }
    [Serializable]
    public class ThetaCorrectionCoordinateCollection : Collection<ThetaCorrectionCoordinate>
    {
        public XytCoordinate GetCorrectionCoordinate(XytCoordinate coordinate)
        {
            XytCoordinate resultCoordinate;
            PerspectiveProjection perspectiveProjection = new PerspectiveProjection();
            ThetaCorrectionCoordinate matrix = this.GetMatrix(coordinate.T);

            Log.Write("RotateMesurement", string.Format("Angle : {0}", coordinate.T));
            Log.Write("RotateMesurement", string.Format("Matrix : \n{0}", matrix.ToString()));

            XyCoordinate resultxyCoordinate = perspectiveProjection.GetPerspectiveProjectionPoint((XyCoordinate)coordinate, matrix.CorrectionMatrix);
            resultCoordinate = new XytCoordinate(resultxyCoordinate.X, resultxyCoordinate.Y, matrix.CommandAngle);

            return resultCoordinate;
        }

        public ThetaCorrectionCoordinate GetMatrix(double dRealAngle)
        {
            QMCMatrix matrix = null;
            var list = (from v in this
                        orderby Math.Abs(v.RealAngle - dRealAngle)
                        select v).Take(1);
            if (list.Count() > 0)
            {
                return list.ElementAt(0);
            }
            else
            {
                return null;
            }
            return null;
        }

        public double GetRealAngle(double dCommandAngle)
        {
            var list = (from v in this
                        orderby Math.Abs(dCommandAngle - v.CommandAngle)
                        select v).Take(1);
            if (list.Count() > 0)
            {
                return list.ElementAt(0).RealAngle;
            }
            else
            {
                return 0;
            }
        }
    }
    public class Aligner : MultiPatternMatchingVisionPart
    {
        #region Define
        [Serializable]
        public enum AlarmKeys
        {
            eFailedPatternMatch = -9,
            eOutOfAngleTolerance = -10,
            ePositionIsNull = -11,
        }
        [Serializable]
        public enum FunctionID
        {
            Train,
            Search,
        }
        [Serializable]
        public enum AlignType
        {
            OnePoint,
            TwoPoint,
        }
        [Serializable]
        public enum FiducialMarkOffsetKeys
        {
            ReferenceToFirst,
            ReferenceToSecond,
            ReferenceToThird,
            ReferenceToFourth,
        }
        #endregion

        #region Event
        public event AutoVisionAlignerEventHandler BeforeScan;
        public event AutoVisionAlignerEventHandler AfterScan;
        #endregion

        #region PathGenerator
        [Serializable]
        public class FiducialMarkOffset
        {
            #region Field
            private Aligner.FiducialMarkOffsetKeys m_Key;
            private PointD m_Offset;
            #endregion

            #region Constructor
            public FiducialMarkOffset(Aligner.FiducialMarkOffsetKeys key)
            {
                this.Key = key;
                this.Offset = new PointD();
            }
            #endregion

            #region Property
            public Aligner.FiducialMarkOffsetKeys Key
            {
                get { return this.m_Key; }
                set { this.m_Key = value; }
            }

            public PointD Offset
            {
                get { return this.m_Offset; }
                set { this.m_Offset = value; }
            }
            #endregion
        }

        [Serializable]
        public class FiducialMarkOffsetKeyedCollection : KeyedCollection<FiducialMarkOffsetKeys, FiducialMarkOffset>
        {
            protected override FiducialMarkOffsetKeys GetKeyForItem(FiducialMarkOffset item)
            {
                return item.Key;
            }
        }

        #endregion


        private ThetaCorrectionCoordinateCollection m_thetaCorrections;
        private XytCoordinateCollection m_GlobalFiducialPositions;
        private XytCoordinateCollection m_LoadGlobalFiducialPositions;

        private readonly string m_BasePath = "C:\\Program Files\\QMC\\MDT-400P\\Setting\\ThetaCorrection";

        //public IlluminationDataSet IlluminationData
        //{
        //    set
        //    {
        //        AlignerRecipe.IlluminationDataSet = value;
        //    }
        //    get
        //    {
        //        return AlignerRecipe.IlluminationDataSet;
        //    }
        //}

        //private XyCoordinateCollection m_MeasurementCoordinates;
        //private XyCoordinateCollection m_RecipeCoordinates;

        ////  public VisionScale Scale { set; get; }
        //public XytStage Stage { set; get; }

        //public AlignerRecipe AlignerRecipe
        //{
        //    set
        //    {
        //        Recipe = value;
        //    }
        //    get
        //    {
        //        return Recipe as AlignerRecipe;
        //    }
        //}
        //public VisionAlignerResult Result { set; get; }
        //public AlignerParameter Parameter { set; get; }

        //public VisionCalibrator VisionCalibrator { set; get; }


        //public CycleTimer CycleTimer { set; get; }
        //public ThetaCorrectionCoordinateCollection ThetaCorrectionCoordinates
        //{
        //    get { return m_thetaCorrections; }
        //    set { m_thetaCorrections = value; }
        //}
        //public XyCoordinateCollection MeasurementCoordinates
        //{
        //    get { return m_MeasurementCoordinates; }
        //    set { m_MeasurementCoordinates = value; }
        //}
        //public XyCoordinateCollection RecipeCoordinates
        //{
        //    get { return m_RecipeCoordinates; }
        //    set { m_RecipeCoordinates = value; }
        //}
        ////    public TwoDimensionPathGeneratorParameter TwoDimensionPathParameter { set; get; }
        //public PathGeneratorCollection PathGenerators { set; get; }

        //public PathParameterCollection PathParameters { set; get; }

        //public bool bIsAlign { get; set; }

        ////      public PathParameter PathParameter { set; get; }
        public Aligner(string strName) : base(strName)
        {
            m_RoiTrain = new VisionProRoiVisionTool();
            m_RoiInspect = new VisionProRoiVisionTool();
            TestImage = new VisionImage();
            TrainImage = new VisionImage();
            //m_MultiPatternMatchingTool = new VisionProMultiPatternMatchingVisionTool();
            //this.ThetaCorrectionCoordinates = new ThetaCorrectionCoordinateCollection();
            //this.MeasurementCoordinates = new XyCoordinateCollection();
            //this.RecipeCoordinates = new XyCoordinateCollection();
            //this.m_GlobalFiducialPositions = new XytCoordinateCollection();
            //this.m_LoadGlobalFiducialPositions = new XytCoordinateCollection();
            //AlignerRecipe = new AlignerRecipe(this);
            //PathGenerators = new PathGeneratorCollection();
            //PathParameters = new PathParameterCollection();
            //bIsAlign = false;
            //UseTrainRoi = true;
            //UseInspectRoi = true;
            //IlluminationData = new IlluminationDataSet(Name);
            //        Scale = new VisionScale();
        }
        //protected override void InitAlarm()
        //{
        //    base.InitAlarm();
        //    Alarm alarm = new Alarm();
        //    alarm.Code = (int)AlarmKeys.eFailedPatternMatch;
        //    alarm.Title = "Failed Pattern Match";
        //    alarm.Cause = "Failed Pattern Match";
        //    alarm.Source = Name;
        //    alarm.Grade = "Error";
        //    m_dicAlarms.Add(alarm.Code, alarm);

        //    alarm = new Alarm();
        //    alarm.Code = (int)AlarmKeys.eOutOfAngleTolerance;
        //    alarm.Title = "Out Of Angle Tolerance Error";
        //    alarm.Cause = "Out Of Angle Tolerance";
        //    alarm.Source = Name;
        //    alarm.Grade = "Error";
        //    m_dicAlarms.Add(alarm.Code, alarm);

        //    alarm = new Alarm();
        //    alarm.Code = (int)AlarmKeys.ePositionIsNull;
        //    alarm.Title = "Global Fiducial Position is Null";
        //    alarm.Cause = "Global Fiducial Position is Null";
        //    alarm.Source = Name;
        //    alarm.Grade = "Error";
        //    m_dicAlarms.Add(alarm.Code, alarm);
        //}
        //public override int Create()
        //{
        //    int ret = base.Create();

        //    //if (m_MultiPatternMatchingTool == null)
        //    //    m_MultiPatternMatchingTool = new VisionProMultiPatternMatchingVisionTool();
        //    if (m_RoiTrain == null)
        //        m_RoiTrain = new VisionProRoiVisionTool();
        //    if (m_RoiInspect == null)
        //        m_RoiInspect = new VisionProRoiVisionTool();
        //    m_MultiPatternMatchingTool.SubTools.Clear();
        //    m_MultiPatternMatchingTool.SubTools.Add(m_RoiTrain);
        //    if (this.PathGenerators.Count == 0)
        //    {
        //        this.PathGenerators.Add(AlignerRecipe.pathGenerator);
        //        this.PathGenerators.Add(AlignerRecipe.pathGenerator);
        //    }

        //    return ret;
        //}

        //public override void Close()
        //{
        //    base.Close();
        //}

        //public XytCoordinate GetCurrentPosition()
        //{
        //    XytCoordinate current = new XytCoordinate();
        //    if (Stage != null)
        //    {
        //        Stage.GetActualPosition(ref current);
        //    }

        //    return current;
        //}
        //public int Train()
        //{
        //    int ret = 0;

        //    if ((ret = OnTrain(AlignerRecipe.TrainRoiStartLocation, AlignerRecipe.TrainRoiEndLocation, AlignerRecipe.MultiPatternMatchingParameter, IlluminationData)) != 0)
        //    {
        //        return ret;
        //    }

        //    if (AlignerRecipe != null)
        //    {
        //        if (AlignerRecipe.PatternMatchingParameter == null)
        //        {
        //            PatternMatchingParameters newParameter = new PatternMatchingParameters();
        //            AlignerRecipe.PatternMatchingParameter = newParameter;
        //        }
        //        AlignerRecipe.PatternMatchingParameter.TrainImage = TrainImage;
        //        Owner.SaveRecipeData();
        //    }
        //    return ret;
        //}

        //public PatternMatchingResult Search()
        //{
        //    int ret = 0;
        //    if ((ret = OnSearch(AlignerRecipe.InspectRoiStartLocation, AlignerRecipe.InspectRoiEndLocation, AlignerRecipe.MultiPatternMatchingParameter, IlluminationData)) != 0)
        //    {
        //        return null;
        //    }

        //    return m_MultiPatternMatchingTool.Result;
        //}
        //private int SetAlignPos(XytCoordinate result, int nIndex)
        //{
        //    int ret = 0;
        //    if (result != null && AlignerRecipe != null)
        //    {
        //        if (nIndex == 0)
        //        {
        //            this.AlignerRecipe.SetAlignPositionData(AlignerRecipe.PositionAligns.First.ToString(), TargetType.Base, result);
        //        }
        //        if (nIndex == 1)
        //        {
        //            this.AlignerRecipe.SetAlignPositionData(AlignerRecipe.PositionAligns.Second.ToString(), TargetType.Base, result);
        //        }
        //        if (nIndex == 2)
        //        {
        //            this.AlignerRecipe.SetAlignPositionData(AlignerRecipe.PositionAligns.Third.ToString(), TargetType.Base, result);
        //        }
        //        if (nIndex == 3)
        //        {
        //            this.AlignerRecipe.SetAlignPositionData(AlignerRecipe.PositionAligns.Forth.ToString(), TargetType.Base, result);
        //        }
        //    }
        //    return ret;
        //}
        //private XyCoordinate[] GetFourPointPosition(/*out XyCoordinate[] coordinates*/)
        //{
        //    XyCoordinate[] coordinates = null;
        //    XyCoordinate referencePos = (XyCoordinate)AlignerRecipe.GetAlignPositionData(AlignerRecipe.PositionAligns.Reference.ToString());
        //    XyCoordinate firstPos = referencePos;
        //    XyCoordinate secondPos = new XyCoordinate(firstPos.X - this.AlignerRecipe.FiducialXDistance, firstPos.Y);
        //    XyCoordinate thirdPos = new XyCoordinate(firstPos.X - this.AlignerRecipe.FiducialXDistance, firstPos.Y + this.AlignerRecipe.FiducialYDistance);
        //    XyCoordinate fourthPos = new XyCoordinate(firstPos.X, firstPos.Y + this.AlignerRecipe.FiducialYDistance);

        //    coordinates = new XyCoordinate[] { firstPos, secondPos, thirdPos, fourthPos };
        //    return coordinates;
        //}
        //private XyCoordinate[] GetTwoPointPosition(/*out XyCoordinate[] coordinates*/)
        //{
        //    XyCoordinate[] coordinates = null;

        //    XyCoordinate referencePos = (XyCoordinate)AlignerRecipe.GetAlignPositionData(AlignerRecipe.PositionAligns.Reference.ToString());
        //    XyCoordinate firstPos = referencePos;
        //    XyCoordinate secondPos = new XyCoordinate(firstPos.X + this.AlignerRecipe.FiducialXDistance, firstPos.Y/* + this.Recipe.FiducialYDistance*/);

        //    coordinates = new XyCoordinate[] { firstPos, secondPos };

        //    return coordinates;
        //}
        //public override int OnWork()
        //{
        //    int ret = 0;
        //    double absoluteAngle = 0.0;
        //    double relativeAngle = 0.0;
        //    VisionAlignerResult result = null;
        //    IList<IXyCoordinate> fiducialmarkCoordinates = new List<IXyCoordinate>();
        //    //    this.CycleTimer.Start();
        //    if (m_Status == RunStatus.Stop) return -1;
        //    XytCoordinate currentCoordinate = new XytCoordinate();
        //    if (this.Stage.GetCommandPosition(ref currentCoordinate) != 0) return -1;

        //    // Reference Position으로 이동
        //    XytCoordinate referencePos = AlignerRecipe.GetAlignPositionData(AlignerRecipe.PositionAligns.Reference.ToString());
        //    if ((this.Stage.MovePosition(referencePos) != 0)) return -1;
        //    //Thread.Sleep(100);

        //    if (this.AlignerRecipe.UseFourPointAlign)
        //    {
        //        //XyCoordinate[] coordinates = { this.Recipe.FirstFiducialmark,
        //        //this.Recipe.SecondFiducialmark,
        //        //this.Recipe.ThirdFiducialmark,
        //        //this.Recipe.FourthFiducialmark };
        //        XyCoordinate[] coordinates = GetFourPointPosition();
        //        XyCoordinateCollection resultCoordinates = new XyCoordinateCollection();

        //        if ((ret = this.FourPointAlign(coordinates, out resultCoordinates, out absoluteAngle, out relativeAngle)) != 0) return ret;
        //        for (int i = 0; i < resultCoordinates.Count; i++)
        //        {
        //            fiducialmarkCoordinates.Add(resultCoordinates[i]);
        //            XytCoordinate newAlignPos = new XytCoordinate(resultCoordinates[i].X, resultCoordinates[i].Y, absoluteAngle);
        //            SetAlignPos(newAlignPos, i);
        //        }
        //    }
        //    else if (this.AlignerRecipe.UseFourPointAlignNotTheta)
        //    {
        //        XyCoordinate[] coordinates = GetFourPointPosition();

        //    }
        //    else if (this.AlignerRecipe.UseTwoPointAlign)
        //    {
        //        XyCoordinate[] coordinates = /*{ Recipe.FirstFiducialmark, Recipe.SecondFiducialmark, }*/GetTwoPointPosition();
        //        XyCoordinateCollection resultCoordinates = null;

        //        resultCoordinates = new XyCoordinateCollection();
        //        if ((ret = this.TwoPointAlign(coordinates, out resultCoordinates, out absoluteAngle, out relativeAngle)) != 0) return ret;

        //        for (int i = 0; i < resultCoordinates.Count; i++)
        //        {
        //            fiducialmarkCoordinates.Add(resultCoordinates[i]);
        //            XytCoordinate newAlignPos = new XytCoordinate(resultCoordinates[i].X, resultCoordinates[i].Y, absoluteAngle);
        //            SetAlignPos(newAlignPos, i);
        //        }

        //        if (this.AlignerRecipe.EnableThetaCorrection)
        //        {
        //            EnableThetaCorrection();
        //        }
        //    }
        //    else
        //    {
        //        MessageBox.Show("Check Align Type in Recipe");
        //        return ret;
        //    }

        //    result = new VisionAlignerResult(absoluteAngle, relativeAngle, fiducialmarkCoordinates);

        //    this.Result = result;


        //    return ret;
        //}
        //private int Scan(PathGenerator generator, PathParameter parameter, AutoVisionAlignerEventArgs eventArgs, out XytCoordinate resultCoordinate)
        //{
        //    int ret = 0;
        //    resultCoordinate = new XytCoordinate();
        //    // MethodCallerAsyncResult ar = null;
        //    PatternMatchingResult result = null;
        //    XyCoordinate currenCoordinate = new XyCoordinate();

        //    //Step 1 : 설정되어 있는 Parameter를 이용하여 Path들을 생성.
        //    if ((ret = generator.Generate(parameter)) != 0) return ret;

        //    // Step 2 : 생성된 Path로 이동 시작.
        //    for (int i = 0; i < generator.Paths.Count; i++)
        //    {
        //        if (m_Status == RunStatus.Stop || m_Status == RunStatus.CycleStop)
        //        {
        //            return 1;
        //        }
        //        if (parameter.PathType == AlignerRecipe.PathType.StepByStep)
        //        {
        //            if ((this.Stage.MovePosition(generator.Paths[i]) != 0)) return -1;

        //            if (i == 0)
        //            {
        //                this.OnBeforeScan(eventArgs);

        //                //      if ((ret = eventArgs.Result) != 0) return ret;
        //            }

        //            //  SafeThread.Delay(this.ConstructConfiguration.DelayAfterMove);


        //            result = this.Search();
        //            currenCoordinate = (XyCoordinate)generator.Paths[i];

        //            if (result != null && result.Values.Count != 0)
        //            {
        //                break;
        //            }


        //            FireUpdateResult(result);
        //        }
        //        else if (parameter.PathType == AlignerRecipe.PathType.Continuous)
        //        {
        //            //ar = this.Stage.BeginMovePosition(generator.Paths[i], this.Recipe.VelocityPercent);

        //            //while (true)
        //            //{
        //            //    if ((ret = this.MotionMover.Motion.GetCommandPosition(ref currenCoordinate)) != 0) return ret;

        //            //    if (i == 0)
        //            //    {
        //            //        this.OnBeforeScan(eventArgs);

        //            //        if ((ret = eventArgs.Result) != 0) return ret;
        //            //    }

        //            //    if ((ret = this.PatternMatchSync(out result)) != 0) return ret;

        //            //    if (ar.IsCompleted == true || result.Values.Count != 0)
        //            //    {
        //            //        this.MotionMover.Stop();
        //            //        break;
        //            //    }
        //            //}
        //        }
        //    }

        //    if ((ret = this.Stage.MovePosition(currenCoordinate)) != 0) return ret;

        //    this.OnAfterScan(eventArgs);

        //    //  if ((ret = eventArgs.Result) != 0) return ret;

        //    //  SafeThread.Delay(this.ConstructConfiguration.DelayAfterMove);

        //    if (result == null || result.Values.Count == 0)
        //        return -1;

        //    //   if ((ret = this.Alarms[AlarmKeys.FailedPatternMatch].Post(this)) != 0) return ret;
        //    if (this.Owner is DieUnloader)
        //    {
        //        DieUnloader dieUnloader = this.Owner as DieUnloader;
        //        if ((ret = VisionScale.ConvertPosition<XyCoordinate, XytCoordinate>(dieUnloader.Scale, this.Camera.Resolution, (XyCoordinate)currenCoordinate, new PointD(result.Values[0].X, result.Values[0].Y), out resultCoordinate)) != 0) return ret;
        //    }
        //    if (this.Owner is DieLoader)
        //    {
        //        DieLoader dieLoader = this.Owner as DieLoader;
        //        if ((ret = VisionScale.ConvertPosition<XyCoordinate, XytCoordinate>(dieLoader.Scale, this.Camera.Resolution, (XyCoordinate)currenCoordinate, new PointD(result.Values[0].X, result.Values[0].Y), out resultCoordinate)) != 0) return ret;
        //    }

        //    return ret;
        //}
        //private int Scan(XyCoordinate coordinate, double dAngle, out XytCoordinate resultCoordinate)
        //{
        //    int ret = 0;
        //    resultCoordinate = new XytCoordinate();
        //    //MethodCallerAsyncResult ar = null;
        //    PatternMatchingResult result = null;

        //    //if()
        //    //if ((ret = this.MotionMover.Motion.MoveSync(coordinate, this.AssignedSubRecipeItem.VelocityPercent)) != 0) return ret;
        //    if ((ret = this.Stage.MovePosition(new XytCoordinate(coordinate.X, coordinate.Y, dAngle))) != 0) return ret;
        //    Thread.Sleep(1000);
        //    // SafeThread.Delay(this.ConstructConfiguration.DelayAfterMove);


        //    result = this.Search();
        //    if (result == null || result.Values.Count == 0)
        //        return ret;
        //    FireUpdateResult(result);
        //    //         if ((ret = this.Alarms[AlarmKeys.FailedPatternMatch].Post(this)) != 0) return ret;
        //    if (this.Owner is DieUnloader)
        //    {
        //        DieUnloader dieUnloader = this.Owner as DieUnloader;
        //        if ((ret = VisionScale.ConvertPosition<XyCoordinate, XytCoordinate>(dieUnloader.Scale, this.Camera.Resolution, (XyCoordinate)coordinate, new PointD(result.Values[0].X, result.Values[0].Y), out resultCoordinate)) != 0) return ret;
        //    }
        //    if (this.Owner is DieLoader)
        //    {
        //        DieLoader dieLoader = this.Owner as DieLoader;
        //        if ((ret = VisionScale.ConvertPosition<XyCoordinate, XytCoordinate>(dieLoader.Scale, this.Camera.Resolution, (XyCoordinate)coordinate, new PointD(result.Values[0].X, result.Values[0].Y), out resultCoordinate)) != 0) return ret;
        //    }
        //    resultCoordinate.T = dAngle;
        //    return ret;
        //}
        ////private int FourPointRotateAlign(XyCoordinate centerCoordinate, double dAngle, int nIndex, out XytCoordinate resultCoordinate)
        ////{
        ////    int ret = 0;
        ////    resultCoordinate = new XytCoordinate();

        ////    if ((ret = this.Scan(centerCoordinate, dAngle, out resultCoordinate)) != 0) return ret;
        ////    Log.Write("RotateMesurement", string.Format("result Coordinate > {0}: {1}", nIndex, resultCoordinate));

        ////    return ret;
        ////}
        //private int FourPointRotateAlign(XyCoordinate centerCoordinate, double dAngle, int nIndex, out XytCoordinate resultCoordinate)
        //{
        //    int ret = 0;
        //    resultCoordinate = new XytCoordinate();
        //    int nRetry = AlignerRecipe.RetryCount;
        //    while (true)
        //    {
        //        if ((ret = this.Scan(centerCoordinate, dAngle, out resultCoordinate)) != 0)
        //        {
        //            if (nRetry-- < 0)
        //            {
        //                Alarm alarm = this.GetAlarm((int)AlarmKeys.eFailedPatternMatch);
        //                AlarmManager.Instance.ShowAlarm(alarm);
        //                return ret;
        //            }

        //            Thread.Sleep(1);
        //        }
        //        else
        //        {
        //            break;
        //        }
        //    }

        //    Log.Write("RotateMesurement", string.Format("result Coordinate > {0}: {1}", nIndex, resultCoordinate));

        //    return ret;
        //}
        //public int EnableThetaCorrection()
        //{
        //    ThetaCorrectionCoordinates.Clear();
        //    int ret = 0;
        //    XyCoordinate[] coordinates = { this.AlignerRecipe.FirstFiducialmark,
        //        this.AlignerRecipe.SecondFiducialmark,
        //        this.AlignerRecipe.ThirdFiducialmark,
        //        this.AlignerRecipe.FourthFiducialmark };

        //    XytCoordinate currenCoordinate = new XytCoordinate();
        //    if ((ret = this.Stage.GetCommandPosition(ref currenCoordinate)) != 0) return ret;

        //    double dRealAngle = 0;
        //    double dAngle = 0;
        //    double dUnitAngle = this.AlignerRecipe.ThetaCorrectionUnitAngle;
        //    int nCount = (int)(this.AlignerRecipe.ThetaCorrectionAngleTolerance / dUnitAngle) + 1;
        //    List<XytCoordinateCollection> listXytCoordinates = new List<XytCoordinateCollection>();
        //    for (int j = 0; j < coordinates.Length; j++)
        //    {
        //        XyCoordinate centerCoordinate = coordinates[j];
        //        XytCoordinateCollection resultCoordinates = new XytCoordinateCollection();
        //        for (int i = 0; i < nCount; i++)
        //        {
        //            dAngle = currenCoordinate.T + dUnitAngle * i;
        //            if (dAngle > this.AlignerRecipe.MaxTolerance)
        //            {
        //                break;
        //            }
        //            XytCoordinate resultCoordinate = new XytCoordinate();
        //            if ((ret = this.FourPointRotateAlign(centerCoordinate, dAngle, j, out resultCoordinate)) != 0) return ret;

        //            resultCoordinates.Add(resultCoordinate);
        //            centerCoordinate = (XyCoordinate)resultCoordinate;
        //        }

        //        centerCoordinate = coordinates[j];
        //        for (int i = 1; i < nCount; i++)
        //        {
        //            dAngle = currenCoordinate.T + dUnitAngle * i * -1;
        //            if (dAngle < this.AlignerRecipe.MinTolerance)
        //            {
        //                break;
        //            }
        //            XytCoordinate resultCoordinate = new XytCoordinate();
        //            if ((ret = this.FourPointRotateAlign(centerCoordinate, dAngle, j, out resultCoordinate)) != 0) return ret;

        //            resultCoordinates.Add(resultCoordinate);
        //            centerCoordinate = (XyCoordinate)resultCoordinate;
        //        }

        //        listXytCoordinates.Add(resultCoordinates);
        //    }

        //    double realAngle = 0;
        //    for (int i = 0; i < listXytCoordinates[0].Count; i++)
        //    {
        //        ThetaCorrectionCoordinate thetaCorrection = new ThetaCorrectionCoordinate();
        //        thetaCorrection.CommandAngle = listXytCoordinates[0][i].T;
        //        thetaCorrection.Coordinates = new XytCoordinateCollection();
        //        thetaCorrection.Coordinates.Add(listXytCoordinates[0][i]);
        //        thetaCorrection.Coordinates.Add(listXytCoordinates[1][i]);
        //        thetaCorrection.Coordinates.Add(listXytCoordinates[2][i]);
        //        thetaCorrection.Coordinates.Add(listXytCoordinates[3][i]);

        //        if (qGeometry.GetAngle((PointD)((XyCoordinate)listXytCoordinates[0][i]), (PointD)((XyCoordinate)listXytCoordinates[1][i]), ref realAngle) == false) return -1;

        //        if (realAngle > 90)
        //        {
        //            realAngle = realAngle - 180;
        //        }
        //        else if (realAngle < -90)
        //        {
        //            realAngle = realAngle + 180;
        //        }

        //        Log.Write("RotateMesurement", string.Format("CommandAngle : {0}, Real Angle : {1}", thetaCorrection.CommandAngle, realAngle));

        //        if (double.IsNaN(realAngle) == true)
        //        {
        //            Alarm alarm = this.GetAlarm((int)AlarmKeys.eOutOfAngleTolerance);
        //            AlarmManager.Instance.ShowAlarm(alarm);
        //        }
        //        thetaCorrection.RealAngle = realAngle;
        //        ThetaCorrectionCoordinates.Add(thetaCorrection);
        //        thetaCorrection.GenerateMatrix(ThetaCorrectionCoordinates[0].Coordinates);

        //    }

        //    //if(Recipe.UseSaveThetaCorrectionData)
        //    //{
        //    //    ReCalculateRealAngle();
        //    //    SaveThetaCorrectionData();
        //    //}
        //    return ret;
        //}

        //private int TwoPointAlign(XyCoordinate[] centerCoordinates, out XyCoordinateCollection coordinates, out double absoluteAngle, out double relativeAngle)
        //{
        //    int ret = 0;
        //    double originAngle = 0.0;
        //    XytCoordinate resultCoordinate = new XytCoordinate();
        //    XytCoordinate currenCoordinate = new XytCoordinate();
        //    PathParameter[] parameters = new PathParameter[2];
        //    AutoVisionAlignerEventArgs eventArgs = null;

        //    absoluteAngle = 0.0;
        //    relativeAngle = 0.0;
        //    coordinates = new XyCoordinateCollection();

        //    // To Do: post alarm
        //    if (qGeometry.GetAngle((PointD)centerCoordinates[0], (PointD)centerCoordinates[1], ref originAngle) == false) return -1;

        //    //  this.WriteLog(LogLevel.Highest, "Origin Angle : {0}", originAngle);

        //    parameters[0] = (PathParameter)CopyUtility.GetDeepCopy(this.AlignerRecipe.pathGenerator.PathParameter);
        //    parameters[1] = (PathParameter)CopyUtility.GetDeepCopy(this.AlignerRecipe.pathGenerator.PathParameter);

        //    for (int i = 0; i < this.AlignerRecipe.RetryCount + 1; i++)
        //    {
        //        // First, Second Position에 가서 PatternMatching
        //        for (int j = 0; j < 2; j++)
        //        {
        //            parameters[j].CenterCoordinate = centerCoordinates[j];
        //            eventArgs = new AutoVisionAlignerEventArgs(AlignType.TwoPoint, j, i);

        //            if ((ret = this.Scan(this.PathGenerators[j], parameters[j], eventArgs, out resultCoordinate)) != 0) return ret;
        //            //SetAlignPos(resultCoordinate, j);
        //            coordinates.Add((XyCoordinate)resultCoordinate);
        //        }

        //        // PattenrMatching으로 찾은 각도를 구함
        //        if (qGeometry.GetAngle((PointD)coordinates[0], (PointD)coordinates[1], ref relativeAngle) == false) return -1;

        //        //          this.WriteLog(LogLevel.Highest, "Relative Angle : {0}", relativeAngle);

        //        if (double.IsNaN(relativeAngle) == true)
        //            return ret;
        //        //           if ((ret = this.Alarms[AlarmKeys.OutOfAngleTolerance].Post(this)) != 0) return ret;

        //        // 회전할 각도를 계산
        //        if ((ret = this.Stage.GetCommandPosition(ref currenCoordinate)) != 0) return ret;

        //        absoluteAngle = currenCoordinate.T + relativeAngle;

        //        if ((ret = this.Stage.MovePosition(new XytCoordinate(currenCoordinate.X, currenCoordinate.Y, absoluteAngle))) != 0) return ret;

        //        centerCoordinates[0] = (XyCoordinate)this.ConvertCoordinate(coordinates[0], -relativeAngle);
        //        centerCoordinates[1] = (XyCoordinate)this.ConvertCoordinate(coordinates[1], -relativeAngle);

        //        //      this.WriteLog(LogLevel.Highest, "CenterCoordinates First : {0}", centerCoordinates[0]);
        //        //       this.WriteLog(LogLevel.Highest, "CenterCoordinates Second : {0}", centerCoordinates[1]);

        //        if (this.AlignerRecipe.AngleTolerance >= relativeAngle && this.AlignerRecipe.AngleTolerance * -1 <= relativeAngle)
        //            break;
        //        else
        //            coordinates.Clear();

        //        if (this.AlignerRecipe.VerificationEnable == false)
        //        {
        //            break;
        //        }
        //    }

        //    if (coordinates.Count == 0)
        //        return ret;
        //    //       if ((ret = this.Alarms[AlarmKeys.OutOfAngleTolerance].Post(this)) != 0) return ret;

        //    return ret;
        //}
        //public IXyCoordinate ConvertCoordinate(IXyCoordinate source, double angle)
        //{
        //    //회전의 중심을 알아야함.
        //    return qGeometry.CalculateRotationTransformation(this.Stage.CenterCoordinate, source, angle);
        //}
        //private int FirstAlign(XyCoordinate[] centerCoordinates, out XyCoordinateCollection coordinates, out double absoluteAngle, out double relativeAngle, PathParameter[] parameters)
        //{
        //    int ret = 0;


        //    //TwoDimensionPathGeneratorParameter[] parameters = new TwoDimensionPathGeneratorParameter[2];
        //    AutoVisionAlignerEventArgs eventArgs = null;
        //    double offSetX = 0.0;
        //    double offSetY = 0.0;
        //    double theta = 0.0;
        //    XytCoordinate resultCoordinate = new XytCoordinate();
        //    absoluteAngle = 0.0;
        //    relativeAngle = 0.0;
        //    coordinates = new XyCoordinateCollection();
        //    XyCoordinateCollection thetaCorrectionCoordinates = new XyCoordinateCollection();

        //    // parameters[0] = CopyUtility.GetDeepCopy<TwoDimensionPathGeneratorParameter>(this.AssignedSubRecipeItem.PathGeneratorParameters[0]);
        //    //  parameters[1] = CopyUtility.GetDeepCopy<TwoDimensionPathGeneratorParameter>(this.AssignedSubRecipeItem.PathGeneratorParameters[1]);

        //    parameters[0].CenterCoordinate = centerCoordinates[0];
        //    eventArgs = new AutoVisionAlignerEventArgs(AlignType.TwoPoint, 0, 0);
        //    if ((ret = this.Scan(this.PathGenerators[0], parameters[0], eventArgs, out resultCoordinate)) != 0) return ret;
        //    coordinates.Add((XyCoordinate)resultCoordinate);
        //    // XyCoordinate xyoffset = coordinates[0] - centerCoordinates[0];//?
        //    //측정한 좌표 - 레시피 좌표 = offset
        //    //각도 구하기 전에 offset 넣는것은 의미가 없음.
        //    //for (int j = 0; j < centerCoordinates.Length; j++)
        //    //{
        //    //    centerCoordinates[j] += xyoffset;
        //    //}
        //    parameters[1].CenterCoordinate = centerCoordinates[1];
        //    eventArgs = new AutoVisionAlignerEventArgs(AlignType.TwoPoint, 1, 0);

        //    if ((ret = this.Scan(this.PathGenerators[1], parameters[1], eventArgs, out resultCoordinate)) != 0) return ret;
        //    coordinates.Add((XyCoordinate)resultCoordinate);


        //    // PattenrMatching으로 찾은 각도를 구함
        //    if (qGeometry.GetAngle((PointD)coordinates[0], (PointD)coordinates[1], ref relativeAngle) == false) return ret;

        //    if (double.IsNaN(relativeAngle) == true)
        //        //         if ((ret = this.Alarms[AlarmKeys.OutOfAngleTolerance].Post(this)) != 0) return -1;
        //        theta = relativeAngle;

        //    thetaCorrectionCoordinates.Add(ThetaCorrecttion(centerCoordinates[0], theta));
        //    thetaCorrectionCoordinates.Add(ThetaCorrecttion(centerCoordinates[1], theta));
        //    thetaCorrectionCoordinates.Add(ThetaCorrecttion(centerCoordinates[2], theta));
        //    thetaCorrectionCoordinates.Add(ThetaCorrecttion(centerCoordinates[3], theta));

        //    //측정값   -     레시피값 각도보상한 값 = OffSet
        //    offSetX = coordinates[0].X - thetaCorrectionCoordinates[0].X;
        //    offSetY = coordinates[0].Y - thetaCorrectionCoordinates[0].Y;
        //    XyCoordinate xyCoordinateOffset = new XyCoordinate(offSetX, offSetY);
        //    thetaCorrectionCoordinates[2] += xyCoordinateOffset;
        //    thetaCorrectionCoordinates[3] += xyCoordinateOffset;


        //    for (int j = 0; j < 2; j++)
        //    {   //각도보상 한것 을 넣어준다.
        //        parameters[j].CenterCoordinate = thetaCorrectionCoordinates[j + 2];
        //        eventArgs = new AutoVisionAlignerEventArgs(AlignType.TwoPoint, j, 0);

        //        if ((ret = this.Scan(this.PathGenerators[j], parameters[j], eventArgs, out resultCoordinate)) != 0) return ret;
        //        //coordinates = 실제 측정한 값
        //        coordinates.Add((XyCoordinate)resultCoordinate);
        //    }

        //    for (int l = 0; l < coordinates.Count; l++)
        //    {
        //        //        this.WriteLog("coordinates = {0}", coordinates[l]);
        //    }

        //    //if (this.MeasurementCoordinates == null)
        //    //{
        //    //    MeasurementCoordinates = new XyCoordinateCollection();
        //    //}
        //    //MeasurementCoordinates = coordinates;



        //    return ret;
        //}
        ////protected virtual int OnCreatePathGenerator(out PathGenerator generator)
        ////{
        ////    int ret = 0;
        ////    PathParameter parameter = null;

        ////    //To Do :
        ////    if ((ret = this.Recipe.pathGenerator.PathParameter.CreatePathGenerator(out generator)) != 0) return ret;
        ////    if ((ret = generator.Generate(generator.GetParameter())) != 0) return ret;
        ////    //parameter = generator.GetParameter();
        ////    //
        ////    //if (parameter is RectangleZigzagTwoDimensionPathGeneratorParameter)
        ////    //    ((RectangleZigzagTwoDimensionPathGeneratorParameter)parameter).StartCoordinate = ((RectangleZigzagTwoDimensionPathGeneratorParameter)this.PathGeneratorParameter).StartCoordinate;
        ////    //else if (parameter is RectangleStripeTwoDimensionPathGeneratorParameter)
        ////    //    ((RectangleStripeTwoDimensionPathGeneratorParameter)parameter).StartCoordinate = ((RectangleStripeTwoDimensionPathGeneratorParameter)this.PathGeneratorParameter).StartCoordinate;
        ////    //
        ////    //if ((ret = generator.Generate(parameter)) != 0) return ret;

        ////    return ret;
        ////}
        //private int FourPointAlign(XyCoordinate[] centerCoordinates, out XyCoordinateCollection coordinates, out double absoluteAngle, out double relativeAngle)
        //{
        //    PathGenerator generator = null;
        //    //  OnCreatePathGenerator(out generator);
        //    int ret = 0;
        //    double originAngle = 0.0;
        //    double offSetX = 0.0;
        //    double offSetY = 0.0;
        //    double theta = 0.0;
        //    XytCoordinate resultCoordinate = new XytCoordinate();
        //    XytCoordinate currentCoordinate = new XytCoordinate();
        //    PathParameter[] parameters = new PathParameter[2];
        //    AutoVisionAlignerEventArgs eventArgs = null;

        //    absoluteAngle = 0.0;
        //    relativeAngle = 0.0;
        //    coordinates = new XyCoordinateCollection();
        //    XyCoordinateCollection measualCoordinates = new XyCoordinateCollection();
        //    XyCoordinateCollection thetaCorrectionCoordinates = new XyCoordinateCollection();
        //    XyCoordinateCollection recipeThetaCorrectionCoordinates = new XyCoordinateCollection();
        //    XyCoordinateCollection offsetxyCoordinates = new XyCoordinateCollection();
        //    // To Do: post alarm
        //    if (qGeometry.GetAngle((PointD)centerCoordinates[0], (PointD)centerCoordinates[1], ref originAngle) == false) return -1;

        //    // this.WriteLog(LogLevel.Highest, "Origin Angle : {0}", originAngle);
        //    //this.Recipe.PathGeneratorParameters.Add(generator.GetParameter());
        //    //this.Recipe.PathGeneratorParameters.Add(generator.GetParameter());
        //    //this.Recipe.PathGeneratorParameters[0].PathType = Common.PathGenerators.PathGenerator.PathType.StepByStep;
        //    //this.Recipe.PathGeneratorParameters[0].CreatePathGenerator
        //    parameters[0] = (PathParameter)CopyUtility.GetDeepCopy(this.AlignerRecipe.pathGenerator.PathParameter);//?
        //    parameters[1] = (PathParameter)CopyUtility.GetDeepCopy(this.AlignerRecipe.pathGenerator.PathParameter);

        //    for (int i = 0; i < this.AlignerRecipe.RetryCount + 1; i++)
        //    {

        //        this.RecipeCoordinates.Clear();
        //        coordinates.Clear();
        //        thetaCorrectionCoordinates.Clear();
        //        recipeThetaCorrectionCoordinates.Clear();
        //        offsetxyCoordinates.Clear();
        //        if (i < 1)
        //        {
        //            this.FirstAlign(centerCoordinates, out coordinates, out absoluteAngle, out relativeAngle, parameters);
        //        }
        //        else
        //        {
        //            for (int j = 0; j < 2; j++)
        //            {   //각도보상 한것 을 넣어준다.
        //                parameters[j].CenterCoordinate = this.MeasurementCoordinates[j];
        //                eventArgs = new AutoVisionAlignerEventArgs(AlignType.TwoPoint, j, i);

        //                if ((ret = this.Scan(this.PathGenerators[j], parameters[j], eventArgs, out resultCoordinate)) != 0) return ret;
        //                //coordinates = 실제 측정한 값
        //                coordinates.Add((XyCoordinate)resultCoordinate);
        //            }
        //            if (qGeometry.GetAngle((PointD)coordinates[0], (PointD)coordinates[1], ref relativeAngle) == false) return -1;
        //            for (int j = 0; j < 2; j++)
        //            {   //각도보상 한것 을 넣어준다.
        //                parameters[j].CenterCoordinate = this.MeasurementCoordinates[j + 2];
        //                eventArgs = new AutoVisionAlignerEventArgs(AlignType.TwoPoint, j, i);

        //                if ((ret = this.Scan(this.PathGenerators[j], parameters[j], eventArgs, out resultCoordinate)) != 0) return ret;
        //                //coordinates = 실제 측정한 값
        //                coordinates.Add((XyCoordinate)resultCoordinate);
        //            }

        //            this.MeasurementCoordinates.Clear();
        //            if (double.IsNaN(relativeAngle) == true)
        //                //          if ((ret = this.Alarms[AlarmKeys.OutOfAngleTolerance].Post(this)) != 0) return ret;
        //                theta = relativeAngle;

        //            for (int l = 0; l < coordinates.Count; l++)
        //            {
        //                //SetAlignPos(resultCoordinate, i);
        //                //           this.WriteLog("coordinates = {0}", coordinates[l]);
        //            }
        //        }
        //        //공통

        //        if (this.MeasurementCoordinates == null)
        //        {
        //            MeasurementCoordinates = new XyCoordinateCollection();
        //        }
        //        if (this.MeasurementCoordinates.Count != 0)
        //        {
        //            this.MeasurementCoordinates.Clear();
        //        }
        //        for (int k = 0; k < coordinates.Count; k++)
        //        {
        //            this.MeasurementCoordinates.Add(coordinates[k]);
        //            XytCoordinate position = new XytCoordinate(coordinates[k].X, coordinates[k].Y, absoluteAngle);
        //            //SetAlignPos(position, k);
        //        }


        //        theta = relativeAngle * -1;
        //        if (coordinates.Count < 4)
        //        {
        //            //           if ((ret = this.Alarms[AlarmKeys.FailedPatternMatch].Post(this)) != 0) return -1;
        //            return -1;
        //        }
        //        recipeThetaCorrectionCoordinates.Add(ThetaCorrecttion(coordinates[0], theta));
        //        recipeThetaCorrectionCoordinates.Add(ThetaCorrecttion(coordinates[1], theta));
        //        recipeThetaCorrectionCoordinates.Add(ThetaCorrecttion(coordinates[2], theta));
        //        recipeThetaCorrectionCoordinates.Add(ThetaCorrecttion(coordinates[3], theta));


        //        offSetX = coordinates[0].X - recipeThetaCorrectionCoordinates[0].X;
        //        offSetY = coordinates[0].Y - recipeThetaCorrectionCoordinates[0].Y;



        //        XyCoordinate xyCoordinateOffset = new XyCoordinate(offSetX, offSetY);
        //        for (int o = 0; o < recipeThetaCorrectionCoordinates.Count; o++)
        //        {
        //            recipeThetaCorrectionCoordinates[o] += xyCoordinateOffset;
        //            if (this.RecipeCoordinates == null)
        //            {
        //                RecipeCoordinates = new XyCoordinateCollection();
        //            }
        //            RecipeCoordinates.Add(recipeThetaCorrectionCoordinates[o]);
        //        }

        //        for (int l = 0; l < recipeThetaCorrectionCoordinates.Count; l++)
        //        {
        //            //          this.WriteLog("coordinates = {0} , xyCoordinateOffset = {1}, centerCoordinates = {2} ,recipeThetaCorrectionCoordinates = {3}, theta = {4}", coordinates[l], xyCoordinateOffset, centerCoordinates[l], recipeThetaCorrectionCoordinates[l], theta);
        //        }

        //    }
        //    return ret;
        //}
        //// 기준 Point 를 측정 각도로 회전변환 시켜주기 위함.
        //private XyCoordinate ThetaCorrecttion(XyCoordinate xyCoordinate, double dAngle)
        //{
        //    double angle = dAngle / 180 * Math.PI;

        //    XyCoordinate coordinate = new XyCoordinate(Math.Cos(angle) * xyCoordinate.X - Math.Sin(angle) * xyCoordinate.Y, Math.Sin(angle) * xyCoordinate.X + Math.Cos(angle) * xyCoordinate.Y);
        //    //xyCoordinate = coordinate;

        //    return coordinate;
        //}
        //public int Scan(PatternMatchingVisionToolParameter parameter, out PatternMatchingVisionTool result)
        //{
        //    int ret = 0;
        //    result = null;
        //    VisionImage image = null;

        //    // if ((ret = OnSetIllumination()) != 0) return ret;
        //    if ((ret = Camera.GrabSync(Purpose.Processing, out image)) != 0) return ret;
        //    //  if ((ret = OnScan(parameter, image, out result)) != 0) return ret;

        //    return ret;
        //}
        ////private int Scan(PathGenerator generator, PathParameter parameter, AutoVisionAlignerEventArgs eventArgs, out XytCoordinate resultCoordinate)
        ////{
        ////    int ret = 0;
        ////    resultCoordinate = new XytCoordinate();
        ////    XytCoordinate xyCoordinate = new XytCoordinate();

        ////    if ((ret = this.Scan(generator, parameter, eventArgs, out xyCoordinate)) != 0) return ret;

        ////    resultCoordinate.X = xytCoordinate.X;
        ////    resultCoordinate.Y = xytCoordinate.Y;

        ////    return ret;
        ////}
        //public void ReCalculateRealAngle()
        //{
        //    double Angle = ThetaCorrectionCoordinates.GetRealAngle(0);
        //    foreach (ThetaCorrectionCoordinate thetaCoordinate in ThetaCorrectionCoordinates)
        //    {
        //        thetaCoordinate.RealAngle -= Angle;
        //    }
        //}

        //public void SaveThetaCorrectionData()
        //{
        //    try
        //    {
        //        string strBaseFolder = m_BasePath;
        //        if (Directory.Exists(strBaseFolder) == false)
        //        {
        //            Log.Write(this, "Directory not exist. Try Create");
        //            Directory.CreateDirectory(strBaseFolder);
        //        }
        //        string strFile = strBaseFolder + string.Format("\\{0}.dat", Name);
        //        using (StreamWriter Writer = new StreamWriter(strFile))
        //        {
        //            foreach (var thetaCorrectionData in ThetaCorrectionCoordinates)
        //            {
        //                Writer.WriteLine(thetaCorrectionData.ToString());
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.Write(this, String.Format("SaveFile Failed[{0} : {1}", Name, ex.Message));
        //    }
        //}

        //public void LoadThetaCorrectionData()
        //{
        //    string strBaseFolder = m_BasePath;
        //    string strFile = strBaseFolder + string.Format("\\{0}.dat", Name);
        //    if (File.Exists(strFile) == false)
        //    {
        //        Log.Write(this, String.Format("LoadFile file not exist {0}", Name));
        //        return;
        //    }

        //    try
        //    {
        //        using (StreamReader Reader = new StreamReader(strFile))
        //        {
        //            m_thetaCorrections.Clear();
        //            while (true)
        //            {
        //                string strContent;
        //                strContent = Reader.ReadLine();
        //                if (!string.IsNullOrEmpty(strContent))
        //                {
        //                    ThetaCorrectionCoordinate thetaCorrection = new ThetaCorrectionCoordinate();
        //                    thetaCorrection.SetStringData(strContent);
        //                    m_thetaCorrections.Add(thetaCorrection);
        //                }
        //                else
        //                {
        //                    break;
        //                }
        //            }

        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        Log.Write(this, String.Format("LoadFile Failed : {0}", Name));
        //    }
        //}

        //protected virtual void OnBeforeScan(AutoVisionAlignerEventArgs e)
        //{
        //    if (this.BeforeScan != null)
        //        this.BeforeScan(this, e);
        //}

        //protected virtual void OnAfterScan(AutoVisionAlignerEventArgs e)
        //{
        //    if (this.AfterScan != null)
        //        this.AfterScan(this, e);
        //}

        //public override void UpdateConfigData() //참고 : 오버라이드,, 파트 콜
        //{

        //    DieUnloader dieUnloader = Owner as DieUnloader;
        //    if (dieUnloader != null)
        //    {
        //        if (AlignerRecipe.IlluminationDataSet == null)
        //            AlignerRecipe.IlluminationDataSet = new IlluminationDataSet(Name);
        //        AlignerRecipe.IlluminationDataSet.SetIlluminationChannel(dieUnloader.DieUnloaderConfig.ListIlluminationChannel);
        //    }

        //    DieLoader dieloader = Owner as DieLoader;
        //    if (dieloader != null)
        //    {
        //        if (AlignerRecipe.IlluminationDataSet == null)
        //            AlignerRecipe.IlluminationDataSet = new IlluminationDataSet(Name);
        //        AlignerRecipe.IlluminationDataSet.SetIlluminationChannel(dieloader.DieLoaderConfig.ListIlluminationChannel);
        //    }
        //}
        //public override void UpdateRecipeData()
        //{
        //    DieUnloader dieUnloader = Owner as DieUnloader;
        //    if (dieUnloader != null)
        //    {
        //        this.AlignerRecipe = dieUnloader.DieUnloaderRecipe.AlignerRecipe;
        //        if (this.AlignerRecipe.IlluminationDataSet != null)
        //        {
        //            IlluminationData = this.AlignerRecipe.IlluminationDataSet;
        //        }
        //        else
        //        {
        //            this.AlignerRecipe.IlluminationDataSet = new IlluminationDataSet(Name);
        //        }
        //        AlignerRecipe.Init(this);
        //    }

        //    base.UpdateRecipeData();
        //}

        //public override MultiPatternMatchingParameters GetPatternMatchingParameters()
        //{
        //    return AlignerRecipe.MultiPatternMatchingParameter;
        //}

        //public override void SetPatternMatchingParameters(MultiPatternMatchingParameters parameters)
        //{
        //    base.SetPatternMatchingParameters(parameters);
        //    AlignerRecipe.MultiPatternMatchingParameter = parameters;
        //}

        //public override Point GetInspectStartPoint()
        //{
        //    return AlignerRecipe.InspectRoiStartLocation;
        //}

        //public override Point GetInspectEndPoint()
        //{
        //    return AlignerRecipe.InspectRoiEndLocation;
        //}

        //public override Point GetTrainStartPoint()
        //{
        //    return AlignerRecipe.TrainRoiStartLocation;
        //}

        //public override Point GetTrainEndPoint()
        //{
        //    return AlignerRecipe.TrainRoiEndLocation;
        //}

        //public override void SetInspectStartPoint(Point point)
        //{
        //    AlignerRecipe.InspectRoiStartLocation = point;
        //}
        //public override void SetInspectEndPoint(Point point)
        //{
        //    AlignerRecipe.InspectRoiEndLocation = point;
        //}

        //public override void SetTrainStartPoint(Point point)
        //{
        //    AlignerRecipe.TrainRoiStartLocation = point;
        //}

        //public override void SetTrainEndPoint(Point point)
        //{
        //    AlignerRecipe.TrainRoiEndLocation = point;
        //}

        //public override IlluminationDataSet GetIlluminationDataSet()
        //{
        //    return IlluminationData;
        //}
        //public override void Stop()
        //{
        //    base.Stop();
        //    Stage.SetRunStatus(RunStatus.Stop);
        //    Stage.Stop();
        //}

        //#region 회전변환

        //private XytCoordinate RotateTransformation(XyCoordinate position, double dAngle)
        //{
        //    XytCoordinate coodinate = new XytCoordinate();

        //    double positionX = position.X;
        //    double positionY = position.Y;

        //    coodinate.X = (Math.Cos(dAngle) * positionX) - (Math.Sin(dAngle) * positionY);
        //    coodinate.Y = (Math.Sin(dAngle) * positionX) + (Math.Cos(dAngle) * positionY);
        //    coodinate.T = dAngle;

        //    return coodinate;
        //}

        //private XytCoordinateCollection RotateAllFiducial(double dAngle)
        //{

        //    XytCoordinateCollection xytCoordinates = new XytCoordinateCollection();

        //    XyCoordinate firstFiducial = (XyCoordinate)AlignerRecipe.GetAlignPositionData(AlignerRecipe.PositionAligns.First.ToString());
        //    XyCoordinate secondFiducial = (XyCoordinate)AlignerRecipe.GetAlignPositionData(AlignerRecipe.PositionAligns.Second.ToString());
        //    XyCoordinate thirdFiducial = (XyCoordinate)AlignerRecipe.GetAlignPositionData(AlignerRecipe.PositionAligns.Third.ToString());
        //    XyCoordinate fourthFiducial = (XyCoordinate)AlignerRecipe.GetAlignPositionData(AlignerRecipe.PositionAligns.Forth.ToString());

        //    xytCoordinates.Add(RotateTransformation(firstFiducial, dAngle));
        //    xytCoordinates.Add(RotateTransformation(secondFiducial, dAngle));
        //    xytCoordinates.Add(RotateTransformation(thirdFiducial, dAngle));
        //    xytCoordinates.Add(RotateTransformation(fourthFiducial, dAngle));

        //    return xytCoordinates;
        //}

        //private XytCoordinateCollection GetFiducialPosition(double dAngle)
        //{

        //    XytCoordinateCollection xytCoordinates = new XytCoordinateCollection();

        //    XytCoordinate firstFiducial = AlignerRecipe.GetAlignPositionData(AlignerRecipe.PositionAligns.First.ToString());
        //    XytCoordinate secondFiducial = AlignerRecipe.GetAlignPositionData(AlignerRecipe.PositionAligns.Second.ToString());
        //    XytCoordinate thirdFiducial = AlignerRecipe.GetAlignPositionData(AlignerRecipe.PositionAligns.Third.ToString());
        //    XytCoordinate fourthFiducial = AlignerRecipe.GetAlignPositionData(AlignerRecipe.PositionAligns.Forth.ToString());

        //    xytCoordinates.Add(firstFiducial);
        //    xytCoordinates.Add(secondFiducial);
        //    xytCoordinates.Add(thirdFiducial);
        //    xytCoordinates.Add(fourthFiducial);

        //    return xytCoordinates;
        //}

        //private XytCoordinateCollection GetFiducialPosition()
        //{

        //    XytCoordinateCollection xytCoordinates = new XytCoordinateCollection();

        //    XytCoordinate firstFiducial = AlignerRecipe.GetAlignPositionData(AlignerRecipe.PositionAligns.First.ToString());

        //    //xytCoordinates.Add(firstFiducial);
        //    xytCoordinates.Add(new XytCoordinate(firstFiducial.X + AlignerRecipe.FiducialXDistance, firstFiducial.Y, firstFiducial.T));
        //    xytCoordinates.Add(new XytCoordinate(firstFiducial.X + AlignerRecipe.FiducialXDistance, firstFiducial.Y + AlignerRecipe.FiducialYDistance, firstFiducial.T));
        //    xytCoordinates.Add(new XytCoordinate(firstFiducial.X, firstFiducial.Y + AlignerRecipe.FiducialYDistance, firstFiducial.T));

        //    return xytCoordinates;
        //}

        //#endregion

        //#region 투영변환

        //private List<XyCoordinate> ProjectionTransformation(double dAngle)
        //{
        //    List<XyCoordinate> alignPositionList = new List<XyCoordinate>();
        //    ThetaCorrectionCoordinate thetaCorrection = new ThetaCorrectionCoordinate();

        //    XytCoordinateCollection SourceCoordinates = new XytCoordinateCollection();
        //    XytCoordinateCollection TargetCoordinates = new XytCoordinateCollection();

        //    TargetCoordinates = GetFiducialPosition(dAngle);
        //    thetaCorrection.Coordinates = TargetCoordinates;

        //    SourceCoordinates = RotateAllFiducial(dAngle);
        //    thetaCorrection.GenerateMatrix(SourceCoordinates);

        //    return alignPositionList;
        //}

        //private XytCoordinate MakeCoordinate(XyCoordinate position, double dAngle)
        //{
        //    XytCoordinate coordinate = new XytCoordinate();

        //    coordinate.X = position.X;
        //    coordinate.Y = position.Y;
        //    coordinate.T = dAngle;

        //    return coordinate;
        //}

        //public int MesureGlobalFiducialPosition()
        //{
        //    int ret = 0;

        //    XytCoordinateCollection positions = GetFiducialPosition();
        //    m_GlobalFiducialPositions = new XytCoordinateCollection();

        //    //start position
        //    XytCoordinate startPosition = AlignerRecipe.GetAlignPositionData(AlignerRecipe.PositionAligns.First.ToString());
        //    m_GlobalFiducialPositions.Add(startPosition);

        //    for (int iter = 0; iter < positions.Count; iter++)
        //    {
        //        XytCoordinate position = new XytCoordinate();
        //        if ((ret = Scan((XyCoordinate)positions[iter], positions[iter].T, out position)) != 0)
        //        {
        //            return ret;
        //        }
        //        m_GlobalFiducialPositions.Add(position);
        //    }

        //    if (AlignerRecipe.AlignPositions.Count > (int)AlignerRecipe.PositionAligns.Forth && m_GlobalFiducialPositions.Count >= 4)
        //    {
        //        AlignerRecipe.SetAlignPositionData(AlignerRecipe.PositionAligns.Third.ToString(), TargetType.Base, m_GlobalFiducialPositions[2]);

        //        AlignerRecipe.SetAlignPositionData(AlignerRecipe.PositionAligns.Forth.ToString(), TargetType.Base, m_GlobalFiducialPositions[3]);
        //    }            

        //    return ret;
        //}

        //public void SaveGloblaFiducialPositions()
        //{
        //    try
        //    {
        //        string strSaveFile = ConfigManager.GetAlignerFilePath();
        //        StringBuilder builder = new StringBuilder();

        //        if (m_GlobalFiducialPositions == null)
        //        {
        //            m_GlobalFiducialPositions = new XytCoordinateCollection();

        //        }
        //        builder.AppendLine(m_GlobalFiducialPositions.ToString());
        //        if (!File.Exists(ConfigManager.GetAlignerPath()))
        //        {
        //            Directory.CreateDirectory(ConfigManager.GetAlignerPath());
        //        }
        //        this.FileSave(strSaveFile, builder);
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.Write(this, String.Format("SaveFile Failed[{0} : {1}", Name, ex.Message));
        //    }
        //}

        //private void FileSave(string path, StringBuilder builder)
        //{
        //    try
        //    {
        //        File.WriteAllText(path, builder.ToString());
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.Write(this, String.Format("SaveFile Failed[{0} : {1}", Name, ex.Message));
        //    }
        //}

        //public void LoadGloblaFiducialPositions()
        //{
        //    try
        //    {
        //        string strLoadFile = ConfigManager.GetAlignerFilePath();
        //        StringBuilder builder = null;
        //        builder = this.FileLoad(strLoadFile);
        //        string[] line = builder.ToString().Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
        //        double positionX = 0D, positionY = 0D, positionT = 0D;
        //        if (m_LoadGlobalFiducialPositions == null)
        //        {
        //            m_LoadGlobalFiducialPositions = new XytCoordinateCollection();
        //        }
        //        m_LoadGlobalFiducialPositions.Clear();
        //        if (line.Length > 0)
        //        {
        //            for (int i = 0; i < line.Length; i++)
        //            {
        //                string[] token = line[i].Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);

        //                if (double.TryParse(token[0], out positionX) == false) continue;
        //                if (double.TryParse(token[1], out positionY) == false) continue;
        //                if (double.TryParse(token[2], out positionT) == false) continue;

        //                m_LoadGlobalFiducialPositions.Add(new XytCoordinate(positionX, positionY, positionT));
        //            }
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        Log.Write(this, String.Format("LoadFile Failed[{0} : {1}", Name, ex.Message));
        //    }
        //}

        //private StringBuilder FileLoad(string path)
        //{
        //    StringBuilder builder = new StringBuilder();

        //    try
        //    {
        //        builder = new StringBuilder(File.ReadAllText(path));
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.Write(this, String.Format("LoadFile Failed[{0} : {1}", Name, ex.Message));
        //    }

        //    return builder;
        //}

        //public XytCoordinate GetFirstStartPosition()
        //{
        //    XytCoordinate position = new XytCoordinate();

        //    position = m_GlobalFiducialPositions[0];

        //    return position;
        //}
        //public XytCoordinate GetLoadFirstStartPosition()
        //{
        //    XytCoordinate position = new XytCoordinate();

        //    position = m_LoadGlobalFiducialPositions[0];

        //    return position;
        //}

        //public QMCMatrix GetMatrix()
        //{
        //    QMCMatrix matrix = null;
        //    PerspectiveProjection perspectiveProjection = new PerspectiveProjection();
        //    perspectiveProjection.m_dOffsetX = 1000;
        //    if (m_LoadGlobalFiducialPositions != null && m_GlobalFiducialPositions != null)
        //    {
        //        matrix = perspectiveProjection.projection_matrix(m_LoadGlobalFiducialPositions, m_GlobalFiducialPositions);
        //    }
        //    else
        //    {
        //        //alarm
        //        Alarm alarm = this.GetAlarm((int)AlarmKeys.ePositionIsNull);
        //        AlarmManager.Instance.ShowAlarm(alarm);

        //    }
        //    return matrix;
        //}

        //public override void SetParameter(double dTolerance, int nMaxInstance, double dMinScore, bool bDuplicateChecked, bool bUseMaskImage)
        //{
        //    this.AlignerRecipe.PatternMatchingParameter.MaxTolerance = dTolerance;
        //    this.AlignerRecipe.PatternMatchingParameter.MinTolerance = dTolerance * -1;
        //    this.AlignerRecipe.PatternMatchingParameter.MaxInstance = nMaxInstance;
        //    this.AlignerRecipe.PatternMatchingParameter.MinScore = dMinScore;
        //    this.AlignerRecipe.PatternMatchingParameter.DuplicateChecked = bDuplicateChecked;
        //    this.AlignerRecipe.PatternMatchingParameter.UseMaskImage = bUseMaskImage;
        //}

        //public override void GetParameter(out PatternMatchingParameters parameter)
        //{
        //    parameter = new PatternMatchingParameters();
        //    parameter.MaxTolerance = AlignerRecipe.PatternMatchingParameter.MaxTolerance;
        //    parameter.MinTolerance = AlignerRecipe.PatternMatchingParameter.MinTolerance;
        //    parameter.MaxInstance = AlignerRecipe.PatternMatchingParameter.MaxInstance;
        //    parameter.MinScore = AlignerRecipe.PatternMatchingParameter.MinScore;
        //    parameter.DuplicateChecked = AlignerRecipe.PatternMatchingParameter.DuplicateChecked;
        //    parameter.UseMaskImage = AlignerRecipe.PatternMatchingParameter.UseMaskImage;
        //}


        //#endregion

        //public override void SetRunStatus(RunStatus status)
        //{
        //    base.SetRunStatus(status);
        //    if(Stage != null)
        //    {
        //        Stage.SetRunStatus(status);
        //    }
        //}
    }


    #region AutoVisionAlignerEventArgs
    [Serializable]
    public class AutoVisionAlignerEventArgs
    {
        #region Field
        private Aligner.AlignType m_AlignType;
        private int m_Index;
        private int m_RetryCount;
        #endregion

        #region Constructor
        public AutoVisionAlignerEventArgs(Aligner.AlignType alignType, int index, int retryCount)
        {
            this.AlignType = alignType;
            this.Index = index;
            this.RetryCount = retryCount;
        }
        #endregion

        #region Property
        /// <summary>
        /// Align 방식을 가져온다.
        /// </summary>
        public Aligner.AlignType AlignType
        {
            get { return this.m_AlignType; }
            private set { this.m_AlignType = value; }
        }

        /// <summary>
        /// 진행중인 Align의 위치를 가져온다.
        /// Index 0은 첫번째 위치
        /// </summary>
        public int Index
        {
            get { return this.m_Index; }
            private set { this.m_Index = value; }
        }

        /// <summary>
        /// Align Retry 횟수를 가져온다.
        /// </summary>
        public int RetryCount
        {
            get { return this.m_RetryCount; }
            private set { this.m_RetryCount = value; }
        }
        #endregion
    }
    public delegate void AutoVisionAlignerEventHandler(object sender, AutoVisionAlignerEventArgs e);
    #endregion

    #region VisionAlignerResult
    [Serializable]
    public class VisionAlignerResult
    {
        #region Field
        private double m_AbsoluteAngle;
        private double m_RelativeAngle;
        private IXyCoordinateReadOnlyCollection m_FiducialmarkCoordinates;
        #endregion

        #region Constructor
        public VisionAlignerResult(double absoluteAngle, double relativeAngle, IXyCoordinateReadOnlyCollection fiducialmarkCoordinates)
        {
            this.AbsoluteAngle = absoluteAngle;
            this.RelativeAngle = relativeAngle;
            this.FiducialmarkCoordinates = fiducialmarkCoordinates;
        }
        public VisionAlignerResult(double absoluteAngle, double relativeAngle, IList<IXyCoordinate> fiducialmarkCoordinates) : this(absoluteAngle, relativeAngle, new IXyCoordinateReadOnlyCollection(fiducialmarkCoordinates)) { }
        public VisionAlignerResult(double absoluteAngle) : this(absoluteAngle, 0.0, null) { }
        #endregion

        #region Property
        /// <summary>
        /// Align 해야하는 절대적인 Angle 값을 가져온다.
        /// </summary>
        public double AbsoluteAngle
        {
            get { return this.m_AbsoluteAngle; }
            private set { this.m_AbsoluteAngle = value; }
        }

        /// <summary>
        /// Align 해야하는 상대적인 Angle 값을 가져온다.
        /// </summary>
        public double RelativeAngle
        {
            get { return this.m_RelativeAngle; }
            private set { this.m_RelativeAngle = value; }
        }

        public IXyCoordinateReadOnlyCollection FiducialmarkCoordinates
        {
            get { return this.m_FiducialmarkCoordinates; }
            private set { this.m_FiducialmarkCoordinates = value; }
        }
        #endregion
    }
    #endregion
}

