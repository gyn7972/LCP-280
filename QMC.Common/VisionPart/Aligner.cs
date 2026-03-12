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

        ////      public PathParameter PathParameter { set; get; }
        public Aligner(string strName) : base(strName)
        {
            m_RoiTrain = new VisionProRoiVisionTool();
            m_RoiInspect = new VisionProRoiVisionTool();
            TestImage = new VisionImage();
            TrainImage = new VisionImage();
            
        }
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

