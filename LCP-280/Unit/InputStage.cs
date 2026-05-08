using Newtonsoft.Json.Linq;
using QMC.Common;
using QMC.Common.Alarm;
using QMC.Common.Cameras;
using QMC.Common.Cameras.HIKVISION; // HIK camera
using QMC.Common.Component;
using QMC.Common.IOUtil;
using QMC.Common.Motions;
using QMC.Common.Unit;
using QMC.Common.Vision;              // VisionImage
using QMC.LCP_280.Process.Component;
using QMC.LCP_280.Process.Unit.FormWork.Repro;
using QMC.LCP_280.Process.Work;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static QMC.Common.Component.BaseComponent;
using static QMC.Common.Material;
using static QMC.LCP_280.Process.Component.MeasurementRecipe;
using static QMC.LCP_280.Process.Equipment;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using Path = System.IO.Path;

namespace QMC.LCP_280.Process.Unit
{
    /// <summary>
    /// InputStage Unit
    ///  - Teaching Position + Offset 관리 (InputStageConfig)
    ///  - 축 바인딩 및 Move Helper 제공
    ///  - IO Domain (Clamp / Expander / Vacuum / Ring Check 등) 추상화
    ///  - Vision Pattern Matching Runner 연계 (멀티/센터 마크 검색)
    ///  - DryRun (시뮬레이션) 지원
    ///  - OutputStage 와 구현 양식 통일 (Axis / IO / Domain / High-Level 구분)
    /// </summary>
    /// 

    // [ADD] 외곽 제거 형상 옵션 열거형
    public enum OuterRemovalShape
    {
        Ellipse,    // 원형/타원 (기존 방식)
        Rectangle,  // 사각형 (트레이/기판 등)
        Morphology  // 형태학적 제거 (반원, 파손된 웨이퍼, 비정형 형상 대응 - 외곽 껍질 벗기기)
    }

    public class InputStage : BaseUnit<InputStageConfig>, IPatternMarkSource
    {
        #region Types / Events
        public event EventHandler<PatternMarksFoundEventArgs> MarksFound;

        public delegate void UpdateUIWafer(MaterialWafer wafer);
        public event UpdateUIWafer EventUpdateUIWafer;

        public new enum AlarmKeys
        {
            eDieTransferPickZNotSafety = 10201,
            eInputFeederCylinderZNotSafety,
            eInputStageEjectorPinZNotSafety,
            eInputStageEjectorZNotSafety,
            eInputFeederYNotSafe,
            eVisionTsearch,
            eVisionXYsearch,
            eInputStageMoveFail,
            eRingLockFailed,
            eInputStageAlignNotDone,
            eInputStageNoWafer,
            eInputStageAlignNotCompleted,
            eInputStageMapMatch,
            eInputStageRingPresent,
            eInputStageLiftUp,
            eInputStageLiftDown,
            eInputStageClampFWD,
            eInputStageClampBWD,
            eInputStageScanEmpty,
        }
        private struct AngleStats
        {
            public int RawCount;
            public double Average;
            public double StdDev;
            public double Representative;
        }

        #endregion

        #region Fields - Units / Components
        InputDieTransfer InputDieTransfer { get; set; }
        InputFeeder InputFeeder { get; set; }
        InputStageEjector InputStageEjector { get; set; }
        #endregion

        #region Fields - Axes
        private MotionAxis _axX, _axY, _axT;
        public MotionAxis AxisX => _axX;
        public MotionAxis AxisY => _axY;
        public MotionAxis AxisT => _axT;
        #endregion

        #region Fields - IO Domains
        private Cylinder _cylClampLift;
        private Cylinder _cylClampFB;
        private Cylinder _cylPlate;
        private Vacuum _vacuum;
        #endregion

        #region Fields - Vision
        public HIKGigECamera StageCamera { get; private set; }
        public string StageCameraKey { get; set; } = "In_Stage";

        public PatternMatchingRunner _pmRunner;
        private bool _runnerInitTried;

        public double PixelSizeXmm { get; set; } = 0.005;
        public double PixelSizeYmm { get; set; } = 0.005;
        public bool UseImageCenterAsOrigin { get; set; } = true;
        public double ImageOriginX { get; set; } = double.NaN;
        public double ImageOriginY { get; set; } = double.NaN;
        public string PatternRecipeRootDir { get; set; } = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configs", "PatternMatching");
        public string PatternRecipeName { get; set; } = "Default";

        #endregion

        #region Fields - Params / Tunings
        // 파라미터로 빼야하는 Data 및 상수
        public int MoveTimeoutMs { get; set; } = 6000;
        public int PollIntervalMs { get; set; } = 1; //30
        public double AngleIgnoreThresholdDeg { get; set; } = 0.001;
        public double AngleMaxApplyDeg { get; set; } = 2.0;
        public double AngleApplyGain { get; set; } = -1.0; // 방향 반전 필요 시 -1 사용

        // Align/Refine params // 정밀 T 재보정 옵션/파라미터
        public bool EnableThetaDualPointRefine { get; set; } = true;
        public int MaxTRefineAttempts { get; set; } = 5;
        public double TRefineResidualToleranceDeg { get; set; } = 0.01; // 최종 허용 잔류 기울기
        public double TRefineShiftRatio { get; set; } = 0.5; // ROI 폭/높이 대비 이동 비율(50%)
        /// <summary>
        /// 현재 스테이지 위치에서 이미지 그랩 후 첫 번째 패턴의 글로벌 좌표(mm) 계산
        /// (스테이지 좌표 + 픽셀->mm 오프셋)
        /// </summary>
        // 허용 오차(mm): X 시도 시 두 점의 Y가 동일 라인, Y 시도 시 두 점의 X가 동일 라인
        public double DirectionalPerpendicularToleranceMm { get; set; } = 0.2;

        // SearchAround params
        public bool SearchAroundReturnToCenter { get; set; } = true;
        public bool EnableSearchAroundCenter { get; set; } = true;
        public int SearchAroundMaxRings { get; set; } = 2;          // 1=상하좌우+대각, 2=확장 한 번 더
        public double SearchAroundPitchScale { get; set; } = 1.0;   // 피치 기준 이동 배율
        public int SearchAroundMoveTimeoutMs { get; set; } = 3000;

        #endregion

        #region State - Seq Signals / Status
        // ====== Align Refactor: 상태/결과 보관 필드 ======
        public bool IsStatus_TAlignPrepared { get; set; }
        public bool IsStatus_TAlignDone { get; set; }
        public double IsStatus_LastFoundTRawAngle { get; set; }
        public double IsStatus_LastAppliedTAngle { get; set; }
        public bool IsStatus_XYAlignPrepared { get; set; }
        public bool IsStatus_XYAlignDone { get; set; }
        public double IsStatus_LastFoundDx { get; set; }
        public double IsStatus_LastFoundDy { get; set; }

        public bool RequestOutputDie { get; set; } = false;
        public bool IsStatus_RequestWafer { get; internal set; } = false;
        #endregion


        #region Construction / Initialization
        public InputStage(InputStageConfig config = null) : base(new InputStageConfig())
        {
            AddComponents();
        }


        public override void AddComponents()
        {
            Config.LoadAndBindAxes(Equipment.Instance.AxisManager);
            Config.InitializeDefaultTeachingPositions();

            BindAxes();
            BindIoDomains();
            BindCamera();

            Config.IsSimulation = Config.IsSimulation;
            if (Config.IsSimulation)
            {
                _axX.Config.IsSimulation = true;
                _axY.Config.IsSimulation = true;
                _axT.Config.IsSimulation = true;

                StageCamera.IsSimulation = true;

                Log.Write("InputStage", "Simulation Mode");
            }
            else
            {
                StageCamera.IsSimulation = false;
            }
        }

        protected override void OnBindUnit()
        {
            base.OnBindUnit();
            InputFeeder = Equipment.Instance.GetUnit(UnitKeys.InputFeeder) as InputFeeder;
            InputStageEjector = Equipment.Instance.GetUnit(UnitKeys.InputStageEjector) as InputStageEjector;
            InputDieTransfer = Equipment.Instance.GetUnit(UnitKeys.InputDieTransfer) as InputDieTransfer;
        }
        #endregion

        #region Alarm
        protected override void InitAlarm()
        {
            string source = "Wafer_Stage";
            base.InitAlarm();

            // 1. 공용 파일 로더에서 알람 목록 가져오기
            var loadedAlarms = GlobalAlarmTable.Instance.GetAlarmsForSource(source);
            if (loadedAlarms == null || loadedAlarms.Count == 0)
            {
                Log.Write("AlarmInit", $"Cannot find alarms for source '{source}' in the alarm file. Only default alarms will be registered.");


                AlarmInfo alarm = new AlarmInfo();
                alarm.Code = (int)AlarmKeys.eDieTransferPickZNotSafety;
                alarm.Title = "Die Tr Z-Axis Not safety Pos.";
                alarm.Cause = "Die Tr Z-Axis is not at safety position. Please check the position and restart.";
                alarm.Source = source;// this.UnitName;
                alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
                m_dicAlarms.Add(alarm.Code, alarm);

                alarm = new AlarmInfo();
                alarm.Code = (int)AlarmKeys.eInputFeederCylinderZNotSafety;
                alarm.Title = "Feeder Z-Cylinder Not safety Pos.";
                alarm.Cause = "Feeder Z-Cylinder is not at safety position. Please check the position and restart.";
                alarm.Source = source;// this.UnitName;
                alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
                m_dicAlarms.Add(alarm.Code, alarm);

                //,
                alarm = new AlarmInfo();
                alarm.Code = (int)AlarmKeys.eInputStageEjectorPinZNotSafety;
                alarm.Title = "EjectorPin Z-Axis Not safety Pos.";
                alarm.Cause = "EjectorPin Z-Axis is not at safety position. Please check the position and restart.";
                alarm.Source = source;// this.UnitName;
                alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
                m_dicAlarms.Add(alarm.Code, alarm);
                //,
                alarm = new AlarmInfo();
                alarm.Code = (int)AlarmKeys.eInputStageEjectorZNotSafety;
                alarm.Title = "Ejector Z-Axis Not safety Pos.";
                alarm.Cause = "Ejector Z-Axis is not at safety position. Please check the position and restart.";
                alarm.Source = source;// this.UnitName;
                alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
                m_dicAlarms.Add(alarm.Code, alarm);

                //
                alarm = new AlarmInfo();
                alarm.Code = (int)AlarmKeys.eInputFeederYNotSafe;
                alarm.Title = "Feeder Y-Axis Not safety Pos.";
                alarm.Cause = "Feeder Y-Axis is not at safety position. Please check the position and restart.";
                alarm.Source = source;// this.UnitName;
                alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
                m_dicAlarms.Add(alarm.Code, alarm);

                alarm = new AlarmInfo();
                alarm.Code = (int)AlarmKeys.eVisionTsearch;
                alarm.Title = "Vision T Search.";
                alarm.Cause = "Vision T Search Fail. Please check the Chip Mark and restart.";
                alarm.Source = source;// this.UnitName;
                alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
                m_dicAlarms.Add(alarm.Code, alarm);
                //
                alarm = new AlarmInfo();
                alarm.Code = (int)AlarmKeys.eVisionXYsearch;
                alarm.Title = "Vision XY Search.";
                alarm.Cause = "Vision XY Search Fail. Please check the Chip Mark and restart.";
                alarm.Source = source;// this.UnitName;
                alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
                m_dicAlarms.Add(alarm.Code, alarm);

                alarm = new AlarmInfo();
                alarm.Code = (int)AlarmKeys.eInputStageMoveFail;
                alarm.Title = "Stage move failed.";
                alarm.Cause = "Please check the motor status.";
                alarm.Source = source;// this.UnitName;
                alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
                m_dicAlarms.Add(alarm.Code, alarm);

                alarm = new AlarmInfo();
                alarm.Code = (int)AlarmKeys.eRingLockFailed;
                alarm.Title = "Stage product lock failed.";
                alarm.Cause = "Please check the stage Lift Lock cylinder status.";
                alarm.Source = source;// this.UnitName;
                alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
                m_dicAlarms.Add(alarm.Code, alarm);

                alarm = new AlarmInfo();
                alarm.Code = (int)AlarmKeys.eInputStageAlignNotDone;
                alarm.Title = "Input Stage Align Not Done.";
                alarm.Cause = "Input Stage Align is not done. Please try again.";
                alarm.Source = source;// this.UnitName;
                alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
                m_dicAlarms.Add(alarm.Code, alarm);


                alarm = new AlarmInfo();
                alarm.Code = (int)AlarmKeys.eInputStageNoWafer;
                alarm.Title = "Input Stage No Wafer.";
                alarm.Cause = "There is no wafer on the Input Stage. Please try again.";
                alarm.Source = source;// this.UnitName;
                alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
                m_dicAlarms.Add(alarm.Code, alarm);

                alarm = new AlarmInfo();
                alarm.Code = (int)AlarmKeys.eInputStageAlignNotCompleted;
                alarm.Title = "Input Stage Align Not Completed.";
                alarm.Cause = "Input Stage Align is not completed. Please try again.";
                alarm.Source = source;// this.UnitName;
                alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
                m_dicAlarms.Add(alarm.Code, alarm);

                alarm = new AlarmInfo();
                alarm.Code = (int)AlarmKeys.eInputStageMapMatch;
                alarm.Title = "Input Stage Map Match Failed.";
                alarm.Cause = "Input Stage Map Match failed. Please try again.";
                alarm.Source = source;// this.UnitName;
                alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
                m_dicAlarms.Add(alarm.Code, alarm);

                alarm = new AlarmInfo();
                alarm.Code = (int)AlarmKeys.eInputStageRingPresent;
                alarm.Title = "Input Stage Ring Present Failed.";
                alarm.Cause = "Product detection on Input Stage failed. Please try again.";
                alarm.Source = source;// this.UnitName;
                alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
                m_dicAlarms.Add(alarm.Code, alarm);

                alarm = new AlarmInfo();
                alarm.Code = (int)AlarmKeys.eInputStageLiftUp;
                alarm.Title = "Input Stage Lift Up Failed.";
                alarm.Cause = "Input Stage Lift Up failed. Please try again.";
                alarm.Source = source;// this.UnitName;
                alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
                m_dicAlarms.Add(alarm.Code, alarm);

                alarm = new AlarmInfo();
                alarm.Code = (int)AlarmKeys.eInputStageLiftDown;
                alarm.Title = "Input Stage Lift Down Failed.";
                alarm.Cause = "Input Stage Lift Down failed. Please try again.";
                alarm.Source = source;// this.UnitName;
                alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
                m_dicAlarms.Add(alarm.Code, alarm);

                //
                alarm = new AlarmInfo();
                alarm.Code = (int)AlarmKeys.eInputStageClampFWD;
                alarm.Title = "Input Stage Clamp FWD Failed.";
                alarm.Cause = "Input Stage Clamp FWD failed. Please try again.";
                alarm.Source = source;// this.UnitName;
                alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
                m_dicAlarms.Add(alarm.Code, alarm);

                alarm = new AlarmInfo();
                alarm.Code = (int)AlarmKeys.eInputStageClampBWD;
                alarm.Title = "Input Stage Clamp BWD Failed.";
                alarm.Cause = "Input Stage Clamp BWD failed. Please try again.";
                alarm.Source = source;// this.UnitName;
                alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
                m_dicAlarms.Add(alarm.Code, alarm);

                alarm = new AlarmInfo();
                alarm.Code = (int)AlarmKeys.eInputStageScanEmpty;
                alarm.Title = "Input Stage Scan Empty.";
                alarm.Cause = "Chip Mapping Scan result is 0. Please check Vision/Recipe/Lighting status and try again.";
                alarm.Source = source;// this.UnitName;
                alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
                m_dicAlarms.Add(alarm.Code, alarm);
            }
            else
            {
                // 2. m_dicAlarms에 일괄 등록
                foreach (var alarmInfo in loadedAlarms)
                {
                    if (!m_dicAlarms.ContainsKey(alarmInfo.Code))
                    {
                        m_dicAlarms.Add(alarmInfo.Code, alarmInfo);
                    }
                }
            }

            
        }
        #endregion

        #region Camera Binding
        private void BindCamera()
        {
            var eq = Equipment.Instance; if (eq == null) return;
            if (eq.Cameras != null && eq.Cameras.TryGetValue(StageCameraKey, out var cam))
                StageCamera = cam as HIKGigECamera;
            else
                StageCamera = eq.InStageCam; // fallback
        }
        #endregion

        #region Vision Runner (Pattern Matching)
        // Pattern Matching Runner (간소화: Recipe 자동 관리)
        public PatternMatchingRunner PmRunner
        {
            get
            {
                if (_pmRunner == null)
                {
                    _pmRunner = VisionRunnerHub.GetOrCreate(StageCameraKey);
                }
                return _pmRunner;
            }
        }
        private string CameraKey => StageCameraKey; // 통일된 키 사용
        private (bool ok, List<double> thetaList) MultiSearchViaRunner()
        {
            var ret = VisionRunnerHub.SearchAngles(CameraKey);
            if (!ret.ok) return (false, null);
            return (true, ret.angles);
        }
        /// <summary>
        /// 멀티 패턴 매칭 각도 리스트 반환 (Align 시퀀스용 래퍼)
        /// DryRun 시 모의 데이터 제공
        /// </summary>
        public bool TryGetMultiAngles(out List<double> angles)
        {
            var (ok, list) = MultiSearchViaRunner();
            angles = ok ? list : null;
            return ok && angles != null && angles.Count > 0;
        }
        #endregion

        #region Axes Binding / Teaching
        private void BindAxes()
        {
            var mgr = Equipment.Instance?.AxisManager;
            if (mgr == null)
            {
                Log.Write("InputStage", "[BindAxes] AxisManager null");
                return;
            }

            const string unitName = "Unit"; // Equipment에서 축 등록 시 사용한 유닛명과 동일해야 함
            BindAxis(mgr, unitName, AxisNames.WaferStageX, ref _axX);
            BindAxis(mgr, unitName, AxisNames.WaferStageY, ref _axY);
            BindAxis(mgr, unitName, AxisNames.WaferStageT, ref _axT);
        }

        public int MoveToTeachingPosition(string positionName, bool isFine)
        {
            if (string.IsNullOrWhiteSpace(positionName))
            {
                Log.Write(UnitName, nameof(MoveToTeachingPosition),
                        $"[TeachingMove] TeachingPositions에서 '{positionName}' 을 찾지 못했습니다.");
                return -1;
            }

            int result = 0;

            InputStageConfig.TeachingPositionName en;
            if (Enum.TryParse(positionName, out en))
            {
                int selIndex = FindTeachingSelectionIndex(positionName);
                if (selIndex >= 0)
                {
                    result = MoveToTeachingPositionBySelectionIndex(selIndex, isFine);
                }
                else
                {
                    Log.Write(UnitName, nameof(MoveToTeachingPosition),
                        $"[TeachingMove] TeachingPositions에서 '{positionName}' index를 찾지 못했습니다.");
                    return -1;
                }
            }

            return result;
        }
        private int FindTeachingSelectionIndex(string positionName)
        {
            try
            {
                var list = GetTeachingList();
                if (list == null)
                    return -1;

                for (int i = 0; i < list.Count; i++)
                {
                    var tp = list[i];
                    if (tp != null && string.Equals(tp.Name, positionName, StringComparison.OrdinalIgnoreCase))
                        return i;
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
            return -1;
        }
        private IList<TeachingPosition> GetTeachingList()
        {
            // 1) Recipe 기반 TeachingRecipe가 있으면 그쪽 우선
            //    (Config 타입마다 TeachingRecipe 프로퍼티 존재 여부가 다르므로 reflection 사용)
            try
            {
                var cfg = Config;
                if (cfg != null)
                {
                    var prop = cfg.GetType().GetProperty("TeachingRecipe",
                        System.Reflection.BindingFlags.Instance |
                        System.Reflection.BindingFlags.Public |
                        System.Reflection.BindingFlags.NonPublic);

                    if (prop != null)
                    {
                        var teachingRecipe = prop.GetValue(cfg, null);
                        if (teachingRecipe != null)
                        {
                            // TeachingRecipe가 IHasTeachingPositions 구현한 경우가 많음
                            var has = teachingRecipe as QMC.LCP_280.Process.Unit.FormConfig.IHasTeachingPositions;
                            if (has != null && has.TeachingPositions != null)
                                return has.TeachingPositions;

                            // 혹시 인터페이스가 다르면 TeachingPositions 프로퍼티를 reflection으로 한번 더 시도
                            var tpProp = teachingRecipe.GetType().GetProperty("TeachingPositions",
                                System.Reflection.BindingFlags.Instance |
                                System.Reflection.BindingFlags.Public |
                                System.Reflection.BindingFlags.NonPublic);

                            var list = tpProp != null ? tpProp.GetValue(teachingRecipe, null) as IList<TeachingPosition> : null;
                            if (list != null)
                                return list;
                        }
                    }
                }
            }
            catch { /* ignore */ }

            // 2) 기본: Config.TeachingPositions
            return Config?.TeachingPositions ?? new List<TeachingPosition>();
        }
        public double GetTP(TeachingPosition tp, string axisName) => (tp == null || string.IsNullOrEmpty(axisName)) ? 0.0 : (tp.AxisPositions.TryGetValue(axisName, out var v) ? v : 0.0);
        public int MoveToTeachingPositionBySelectionIndex(int teachingSelIndex, bool isFine = false)
        {
            if (Config == null)
                return -1;

            string tpName;
            if (!Config.GetTeachingPositionName(teachingSelIndex, out tpName) || string.IsNullOrWhiteSpace(tpName))
                return -1;

            InputStageConfig.TeachingPositionName en;
            if (!Enum.TryParse(tpName, out en))
                return -1;

            switch (en)
            {
                case InputStageConfig.TeachingPositionName.Loading:
                    return MoveToStageLoadPosition(isFine);
                case InputStageConfig.TeachingPositionName.Unloading:
                    return MoveToStageUnloadPosition(isFine);
                case InputStageConfig.TeachingPositionName.CenterPoint:
                    return MoveToStageCenterPosition(isFine);
                case InputStageConfig.TeachingPositionName.Ready:
                    return MoveToStageReadyPosition(isFine);
                default:
                    return -1;
            }
            //return 0;
        }

        #endregion

        #region Move Helpers (Axis/Stage/Interlock)
        public int MoveAxisPositionOne(MotionAxis axis, double target, bool isFine = false)
        {
            if (axis == null) return -1;

            if (IsStageInterLockOK() == false)
            {
                return -1;
            }

            Task<int> task = MoveAxisPositionOneAsync(axis, target, isFine);
            while (IsEndTask(task) == false)
            {
                // 동일 Safety Interlock
                if (!InputStageEjector.IsPinZSafetyPos())
                {
                    AxisX?.EmgStop(); AxisY?.EmgStop(); AxisT?.EmgStop();
                    PostAlarm((int)AlarmKeys.eInputStageEjectorPinZNotSafety);
                    return -1;
                }
                if (!InputStageEjector.IsEjectorZSafetyPos())
                {
                    AxisX?.EmgStop(); AxisY?.EmgStop(); AxisT?.EmgStop();
                    PostAlarm((int)AlarmKeys.eInputStageEjectorZNotSafety);
                    return -1;
                }
                if (!InputDieTransfer.IsPositionPickZSafety())
                {
                    AxisX?.EmgStop(); AxisY?.EmgStop(); AxisT?.EmgStop();
                    PostAlarm((int)AlarmKeys.eDieTransferPickZNotSafety);
                    return -1;
                }
                if (!InputFeeder.IsPositionFeederZSafety())
                {
                    AxisX?.EmgStop(); AxisY?.EmgStop(); AxisT?.EmgStop();
                    PostAlarm((int)AlarmKeys.eInputFeederCylinderZNotSafety);
                    return -1;
                }
                if (!InputFeeder.IsPositionFeederYSafety())
                {
                    AxisX?.EmgStop(); AxisY?.EmgStop(); AxisT?.EmgStop();
                    PostAlarm((int)AlarmKeys.eInputFeederYNotSafe);
                    return -1;
                }

                Thread.Sleep(1);
            }
            return task.Result;
        }
        public int MoveApplyOffset(string positionName, double dx, double dy, double dt)
        {
            // Teaching Position 가져오기
            var tp = Config.GetTeachingPosition(positionName);
            if (tp == null) return -1;

            // 오프셋 적용
            Config.SetOffset(positionName, dx, dy, dt);
            var (x, y, t) = Config.GetPositionWithOffset(positionName);   //Offset 포함 위치 - Align 수행 시 data 있음.

            int rc = 0;
            if (AxisX != null) rc |= MoveAxisPositionOne(AxisX, x, false);
            if (AxisY != null) rc |= MoveAxisPositionOne(AxisY, y, false);
            if (AxisT != null) rc |= MoveAxisPositionOne(AxisT, t, false);
            if (rc != 0) return -1;

            // 필요 시 최종 위치 검증
            if (!InPosTeaching(positionName))
            {
                // 약간의 여유 대기 추가 (조건 흔들림 대비)
                if (WaitUntil(() => InPosTeaching(positionName), MoveTimeoutMs) != 0)
                    return -1;
            }

            return 0;
        }

        public int MoveToStageReadyPosition(bool isFine = false)
        {
            int nRet = 0;
            nRet = this.InputStageEjector.MovePositionEjectBlockSafety();
            if (nRet != 0)
            {
                Log.Write(UnitName, "MoveToStageReadyPosition", "Fail: Ejector Move Ready");
                return -1;
            }
            nRet = this.InputStageEjector.MovePositionEjectPinReady();
            if (nRet != 0)
            {
                Log.Write(UnitName, "MoveToStageReadyPosition", "Fail: Ejector Pin Move Ready");
                return -1;
            }
            if (IsInterlockWithFeederAndDieTransferOk() == false)
            {
                Log.Write(UnitName, "MoveToStageReadyPosition", "Interlock check failed");
                return -1;
            }

            Task<int> task = MoveToStageReadyPositionAsync();
            while (IsEndTask(task) == false)
            {
                // Check Interlock.!!! 구문 넣을것.!!!
                if (!InputStageEjector.IsPinZSafetyPos())
                {
                    this.AxisX.EmgStop();
                    this.AxisY.EmgStop();
                    this.AxisT.EmgStop();
                    PostAlarm((int)AlarmKeys.eInputStageEjectorPinZNotSafety);
                    return -1;
                }

                if (!InputStageEjector.IsEjectorZSafetyPos())
                {
                    this.AxisX.EmgStop();
                    this.AxisY.EmgStop();
                    this.AxisT.EmgStop();
                    PostAlarm((int)AlarmKeys.eInputStageEjectorZNotSafety);
                    return -1;
                }

                // DieTransfer PickZ Safety
                if (!InputDieTransfer.IsPositionPickZSafety())
                {
                    this.AxisX.EmgStop();
                    this.AxisY.EmgStop();
                    this.AxisT.EmgStop();
                    PostAlarm((int)AlarmKeys.eDieTransferPickZNotSafety);
                    return -1;
                }

                if (!InputFeeder.IsPositionFeederZSafety())
                {
                    this.AxisX.EmgStop();
                    this.AxisY.EmgStop();
                    this.AxisT.EmgStop();
                    PostAlarm((int)AlarmKeys.eInputFeederCylinderZNotSafety);
                    return -1;
                }

                if (!InputFeeder.IsPositionFeederYSafety())
                {
                    this.AxisX.EmgStop();
                    this.AxisY.EmgStop();
                    this.AxisT.EmgStop();
                    PostAlarm((int)AlarmKeys.eInputFeederYNotSafe);
                    return -1;
                }

                Thread.Sleep(1);
            }
            return task.Result;
        }
        public Task<int> MoveToStageReadyPositionAsync()
        {
            return Task.Run(() =>
            {
                OnMoveToStageReadyPosition();
                return 0;
            });
        }
        private int OnMoveToStageReadyPosition(bool isFine = false)
        {
            return MoveTeachingPositionOnce((int)InputStageConfig.TeachingPositionName.Ready, isFine);
        }

        public int MoveToStageLoadPosition(bool isFine = false)
        {
            int nRet = 0;
            if (IsPositionWaferLoading())
            {
                return 0; // 이미 로딩 위치에 있으면 무시
            }

            nRet = this.InputStageEjector.MovePositionEjectBlockSafety();
            if (nRet != 0)
            {
                Log.Write(UnitName, "MoveToStageUnloadPosition", "Fail: Ejector Move Ready");
                return -1;
            }

            nRet = this.InputStageEjector.MovePositionEjectPinReady();
            if (nRet != 0)
            {
                Log.Write(UnitName, "MoveToStageUnloadPosition", "Fail: Ejector Pin Move Ready");
                return -1;
            }

            if (IsInterlockWithFeederAndDieTransferOk() == false)
            {
                Log.Write(UnitName, "MoveToStageLoad", "Interlock check failed");
                return -1;
            }

            Task<int> task = MoveToStageLoadPositionAsync();
            while (IsEndTask(task) == false)
            {
                // Check Interlock.!!! 구문 넣을것.!!!
                if (!InputStageEjector.IsPinZSafetyPos())
                {
                    this.AxisX.EmgStop();
                    this.AxisY.EmgStop();
                    this.AxisT.EmgStop();
                    PostAlarm((int)AlarmKeys.eInputStageEjectorPinZNotSafety);
                    return -1;
                }

                if (!InputStageEjector.IsEjectorZSafetyPos())
                {
                    this.AxisX.EmgStop();
                    this.AxisY.EmgStop();
                    this.AxisT.EmgStop();
                    PostAlarm((int)AlarmKeys.eInputStageEjectorZNotSafety);
                    return -1;
                }

                // DieTransfer PickZ Safety
                if (!InputDieTransfer.IsPositionPickZSafety())
                {
                    this.AxisX.EmgStop();
                    this.AxisY.EmgStop();
                    this.AxisT.EmgStop();
                    PostAlarm((int)AlarmKeys.eDieTransferPickZNotSafety);
                    return -1;
                }

                var equipment = Equipment.Instance;
                bool IsDryRunEqp = equipment.EquipmentConfig.IsDryRun;
                if (Config.IsSimulation || (Config.IsDryRun || IsDryRunEqp))
                {
                    //Simulation - ok
                }

                else if (!InputFeeder.IsPositionFeederZSafety())
                {
                    this.AxisX.EmgStop();
                    this.AxisY.EmgStop();
                    this.AxisT.EmgStop();
                    PostAlarm((int)AlarmKeys.eInputFeederCylinderZNotSafety);
                    return -1;
                }

                if (!InputFeeder.IsPositionFeederYSafety())
                {
                    this.AxisX.EmgStop();
                    this.AxisY.EmgStop();
                    this.AxisT.EmgStop();
                    PostAlarm((int)AlarmKeys.eInputFeederYNotSafe);
                    return -1;
                }

                Thread.Sleep(1);
            }
            return task.Result;
        }
        public Task<int> MoveToStageLoadPositionAsync()
        {
            return Task.Run(() =>
            {
                OnMoveToStageLoadPosition();
                return 0;
            });
        }
        private int OnMoveToStageLoadPosition(bool isFine = false)
        {
            return MoveTeachingPositionOnce((int)InputStageConfig.TeachingPositionName.Loading, isFine);
        }
        public bool IsPositionWaferLoading()
        {
            var tp = TeachingPositions[(int)InputStageConfig.TeachingPositionName.Loading];
            if (tp == null)
                return false;

            return InPosTeaching(tp);
        }

        public int MoveToStageCenterPosition(bool isFine = false)
        {
            int nRet = 0;

            if (IsWaferCenterPosition())
            {
                return 0; // 이미 센터 위치에 있으면 무시
            }

            nRet = this.InputStageEjector.MovePositionEjectPinReady();
            if (nRet != 0)
            {
                Log.Write(UnitName, "MoveToStageUnloadPosition", "Fail: Ejector Pin Move Ready");
                return -1;
            }

            nRet = this.InputStageEjector.MovePositionEjectBlockSafety();
            if (nRet != 0)
            {
                Log.Write(UnitName, "MoveToStageUnloadPosition", "Fail: Ejector Move Ready");
                return -1;
            }

            if (IsInterlockWithFeederAndDieTransferOk() == false)
            {
                Log.Write(UnitName, "MoveToCenter", "Interlock with Feeder/DieTransfer not OK");
                return -1;
            }

            Task<int> task = MoveToStageCenterPositionAsync();
            while (IsEndTask(task) == false)
            {
                // Check Interlock.!!! 구문 넣을것.!!!
                if (!InputStageEjector.IsPinZSafetyPos())
                {
                    this.AxisX.EmgStop();
                    this.AxisY.EmgStop();
                    this.AxisT.EmgStop();
                    PostAlarm((int)AlarmKeys.eInputStageEjectorPinZNotSafety);
                    return -1;
                }

                if (!InputStageEjector.IsEjectorZSafetyPos())
                {
                    this.AxisX.EmgStop();
                    this.AxisY.EmgStop();
                    this.AxisT.EmgStop();
                    PostAlarm((int)AlarmKeys.eInputStageEjectorZNotSafety);
                    return -1;
                }

                // DieTransfer PickZ Safety
                if (!InputDieTransfer.IsPositionPickZSafety())
                {
                    this.AxisX.EmgStop();
                    this.AxisY.EmgStop();
                    this.AxisT.EmgStop();
                    PostAlarm((int)AlarmKeys.eDieTransferPickZNotSafety);
                    return -1;
                }

                if (!InputFeeder.IsPositionFeederZSafety())
                {
                    this.AxisX.EmgStop();
                    this.AxisY.EmgStop();
                    this.AxisT.EmgStop();
                    PostAlarm((int)AlarmKeys.eInputFeederCylinderZNotSafety);
                    return -1;
                }

                if (!InputFeeder.IsPositionFeederYSafety())
                {
                    this.AxisX.EmgStop();
                    this.AxisY.EmgStop();
                    this.AxisT.EmgStop();
                    PostAlarm((int)AlarmKeys.eInputFeederYNotSafe);
                    return -1;
                }

                Thread.Sleep(1);
            }
            return task.Result;
        }
        public Task<int> MoveToStageCenterPositionAsync()
        {
            return Task.Run(() =>
            {
                OnMoveToStageCenterPosition();
                return 0;
            });
        }
        private int OnMoveToStageCenterPosition(bool isFine = false)
        {
            return MoveTeachingPositionOnce((int)InputStageConfig.TeachingPositionName.CenterPoint, isFine);
        }
        public bool IsWaferCenterPosition()
        {
            var tp = TeachingPositions[(int)InputStageConfig.TeachingPositionName.CenterPoint];
            if (tp == null) return false;
            return InPosTeaching(tp);
        }

        public int MoveToStageUnloadPosition(bool isFine = false)
        {
            int nRet = 0;
            if (IsPositionWaferUnloading())
            {
                return 0; // 이미 로딩 위치에 있으면 무시
            }

            nRet = this.InputStageEjector.MovePositionEjectBlockSafety();
            if (nRet != 0)
            {
                Log.Write(UnitName, "MoveToStageUnloadPosition", "Fail: Ejector Move Ready");
                return -1;
            }
            nRet = this.InputStageEjector.MovePositionEjectPinReady();
            if (nRet != 0)
            {
                Log.Write(UnitName, "MoveToStageUnloadPosition", "Fail: Ejector Pin Move Ready");
                return -1;
            }

            if (IsInterlockWithFeederAndDieTransferOk() == false)
            {
                Log.Write(UnitName, "MoveToStageUnloadPosition-Interlock check failed");
                return -1;
            }

            Task<int> task = MoveToStageUnloadPositionAsync();
            while (IsEndTask(task) == false)
            {
                // Check Interlock.!!! 구문 넣을것.!!!
                if (!InputStageEjector.IsPinZSafetyPos())
                {
                    this.AxisX.EmgStop();
                    this.AxisY.EmgStop();
                    this.AxisT.EmgStop();
                    PostAlarm((int)AlarmKeys.eInputStageEjectorPinZNotSafety);
                    return -1;
                }

                if (!InputStageEjector.IsEjectorZSafetyPos())
                {
                    this.AxisX.EmgStop();
                    this.AxisY.EmgStop();
                    this.AxisT.EmgStop();
                    PostAlarm((int)AlarmKeys.eInputStageEjectorZNotSafety);
                    return -1;
                }

                // DieTransfer PickZ Safety
                if (!InputDieTransfer.IsPositionPickZSafety())
                {
                    this.AxisX.EmgStop();
                    this.AxisY.EmgStop();
                    this.AxisT.EmgStop();
                    PostAlarm((int)AlarmKeys.eDieTransferPickZNotSafety);
                    return -1;
                }

                if (!InputFeeder.IsPositionFeederZSafety())
                {
                    this.AxisX.EmgStop();
                    this.AxisY.EmgStop();
                    this.AxisT.EmgStop();
                    PostAlarm((int)AlarmKeys.eInputFeederCylinderZNotSafety);
                    return -1;
                }

                if (!InputFeeder.IsPositionFeederYSafety())
                {
                    this.AxisX.EmgStop();
                    this.AxisY.EmgStop();
                    this.AxisT.EmgStop();
                    PostAlarm((int)AlarmKeys.eInputFeederYNotSafe);
                    return -1;
                }

                Thread.Sleep(1);
            }
            return task.Result;
        }
        public Task<int> MoveToStageUnloadPositionAsync(bool isFine = false)
        {
            return Task.Run(() =>
            {
                OnMoveToStageUnloadPosition(isFine);
                return 0;
            });
        }
        private int OnMoveToStageUnloadPosition(bool isFine = false)
        {
            return MoveTeachingPositionOnce((int)InputStageConfig.TeachingPositionName.Unloading, isFine);
        }
        public bool IsPositionWaferUnloading()
        {
            var tp = TeachingPositions[(int)InputStageConfig.TeachingPositionName.Unloading];
            if (tp == null) return false;
            return InPosTeaching(tp);
        }

        public int MoveStage(double x, double y, bool bFineSpeed = false)
        {
            int ret = 0;
            if (!this.InputStageEjector.IsPinZSafetyPos())
            {
                AxisX?.EmgStop(); AxisY?.EmgStop(); AxisT?.EmgStop();
                PostAlarm((int)AlarmKeys.eInputStageEjectorPinZNotSafety);
                return -1;
            }

            if (IsStageInterLockOK(x, y))
            {
                ret = 0;
                bool IsAuto = false;
                if (RunMode == UnitRunMode.Auto ||
                    RunUnitStatus == UnitStatus.AutoRunning ||
                    RunUnitStatus == UnitStatus.ManualRunning)
                {
                    IsAuto = true;
                }
                else
                {
                    IsAuto = false;
                }

                ret = this.AxisX.MoveAbs(x, IsAuto, bFineSpeed);
                if (ret != 0)
                {
                    AxisX?.EmgStop(); AxisY?.EmgStop(); AxisT?.EmgStop();
                    PostAlarm((int)AlarmKeys.eInputStageMoveFail);
                    return ret;
                }

                ret = this.AxisY.MoveAbs(y, IsAuto, bFineSpeed);
                if (ret != 0)
                {
                    AxisX?.EmgStop(); AxisY?.EmgStop(); AxisT?.EmgStop();
                    PostAlarm((int)AlarmKeys.eInputStageMoveFail);
                    return ret;
                }

                var rc = WaitUntil(
                        () => AxisX.IsMoveDone() && AxisY.IsMoveDone() &&
                              AxisX.InPosition(x) && AxisY.InPosition(y),
                        MappingMoveTimeoutMs,
                        2,
                        stableHoldMs: 50 // 50ms 연속 안정 확인
);
                if (rc != 0)
                {
                    return -1;
                }
            }
            else
            {
                return -1;
            }
            return ret;
        }
        private bool IsStageInterLockOK(double x, double y)
        {
            bool bRet = false;
            if (this.InputStageEjector.IsEjectorZSafetyPos() == false)
            {
                string strCenterName = InputStageConfig.TeachingPositionName.CenterPoint.ToString();
                var tp = this.Config.GetTeachingPosition(strCenterName);
                double centerX = tp.GetAxisPosition(AxisNames.WaferStageX);
                double centerY = tp.GetAxisPosition(AxisNames.WaferStageY);
                double dRaius = this.Config.SafeSatageRaius;
                double deltaX = centerX - x;
                double deltaY = centerY - y;
                double dDistance = GetDistance(deltaX, deltaY);
                if (dDistance < dRaius)
                {
                    bRet = true;
                }
                else
                {
                    Log.Write(UnitName, "MoveStage", $"Fail: Stage move out of range. Dist={dDistance:F3} Limit={dRaius}");
                    bRet = false;
                }
            }
            else
            {
                bRet = true;
            }
            return bRet;
        }
        public bool IsStageInterLockOK()
        {
            // Ejector / Pin Z 가 이미 Safety 이면 별도 제한 없이 통과 (호출부 로직 유지)
            if (InputStageEjector == null ||
                (InputStageEjector.IsPinZSafetyPos() && InputStageEjector.IsEjectorZSafetyPos()))
            {
                return true;
            }

            var tp = Config.GetTeachingPosition(InputStageConfig.TeachingPositionName.CenterPoint.ToString());
            if (tp == null || tp.AxisPositions == null)
            {
                Log.Write(UnitName, "IsStageInterLockOK", "CenterPoint teaching not found");
                return false;
            }

            if (tp.AxisPositions.TryGetValue(AxisNames.WaferStageX, out var centerX) == false ||
                tp.AxisPositions.TryGetValue(AxisNames.WaferStageY, out var centerY) == false)
            {
                Log.Write(UnitName, "IsStageInterLockOK", "CenterPoint X/Y value missing");
                return false;
            }

            double radius = Config.SafeSatageRaius;
            if (radius <= 0)
            {
                Log.Write(UnitName, "IsStageInterLockOK", $"Invalid SafeSatageRaius={radius}");
                return false;
            }

            double curX = AxisX?.GetPosition() ?? centerX;
            double curY = AxisY?.GetPosition() ?? centerY;

            double dDist = GetDistance(centerX - curX, centerY - curY);
            if (dDist <= radius)
                return true;

            Log.Write(UnitName, "MoveSafety",
                $"Fail: Current XY out of safe radius while Ejector/PinZ unsafe. Dist={dDist:F3} Limit={radius:F3} Center=({centerX:F3},{centerY:F3}) Cur=({curX:F3},{curY:F3})");
            return false;
        }
        #endregion

        #region IO Domains / Interlock
        private void BindIoDomains()
        {
            var eq = Equipment.Instance; var unit = eq?.UnitIO; if (unit == null) return;

            // Vacuum 별칭으로 조회만
            if (!IoAutoBindings.Vacuums.TryGetValue("InStageVac", out _vacuum))
            {
                Log.Write("InputStage", "BindIoDomains", "Vacuums not found: InStageVac");
            }


            // Cylinder는 중앙 별칭으로 조회만
            if (!IoAutoBindings.Cylinders.TryGetValue("InStageExpander", out _cylPlate))
            {
                Log.Write("InputStage", "BindIoDomains", "Cylinder not found: InStageExpander");
            }
            BindCylinder(_cylPlate);

            if (!IoAutoBindings.Cylinders.TryGetValue("InStageClampLift", out _cylClampLift))
            {
                Log.Write("InputStage", "BindIoDomains", "Cylinder not found: InStageClampLift");
            }
            BindCylinder(_cylClampLift);

            if (!IoAutoBindings.Cylinders.TryGetValue("InStageClampFB", out _cylClampFB))
            {
                Log.Write("InputStage", "BindIoDomains", "Cylinder not found: InStageClampFB");
            }
            BindCylinder(_cylClampFB);
        }
        public override bool IsInterlockOK(BaseComponent baseComponent, InterlockEventArgs e)
        {
            bool bRet = base.IsInterlockOK(baseComponent, e);
            if (baseComponent == this.AxisX || baseComponent == this.AxisY || baseComponent == this.AxisT)
            {
                // Interlock Check EjectorZ Safety Position
                bRet &= IsInterlockOkWidthEjectorZ(baseComponent, e);
                if (this.InputFeeder.IsPositionFeederZSafety() == false)
                {
                    if (this.InputFeeder.IsPositionReady() == false)
                    {
                        bRet = false;
                    }
                }
                if (!bRet)
                {
                    PostAlarm((int)AlarmKeys.eInputFeederCylinderZNotSafety);
                    return bRet;
                }
                bRet = IsInterlockOkEjectorPinZ();
                if (bRet == false)
                {
                    PostAlarm((int)AlarmKeys.eInputStageEjectorPinZNotSafety);
                    return bRet;
                }
                if (this.InputDieTransfer.IsPositionPickZSafety() == false)
                {
                    this.AxisX?.EmgStop();
                    this.AxisY?.EmgStop();
                    this.AxisT?.EmgStop();
                    PostAlarm((int)AlarmKeys.eDieTransferPickZNotSafety);
                    bRet = false;
                    return bRet;
                }
                if (this.IsRingPresent())
                {
                    if (IsClampFwd() == false || IsClampLiftUp() == false)
                    {
                        this.AxisX?.EmgStop();
                        this.AxisY?.EmgStop();
                        this.AxisT?.EmgStop();
                        PostAlarm((int)AlarmKeys.eRingLockFailed);
                        bRet = false;
                        return bRet;
                    }
                }
            }
            else if (baseComponent == this._cylClampLift)
            {
                if (e.IsExtend)
                {
                    // Todo : 상황 봐서 인터락 걸자. 이건 꼬라 박지는 안는거 같다.

                    //if(this.IsRingPresent() == false)
                    //{

                    //}
                }
            }
            else
            {

            }
            return bRet;
        }
        private bool IsInterlockOkEjectorPinZ()
        {
            if (this.InputStageEjector.IsEjectorZSafetyPos() == false)
            {
                double dCurrentPositionPinZ = this.InputStageEjector.AxisPinZ.GetPosition();
                var tp = this.InputStageEjector.InputStageEjectorConfig.GetTeachingPosition(InputStageEjectorConfig.TeachingPositionName.EjectPinReady.ToString());
                double dReadyPosition = this.GetTP(tp, this.InputStageEjector.AxisPinZ.Name);
                if (dCurrentPositionPinZ > (dReadyPosition + this.InputStageEjector.AxisPinZ.Config.InposTolerance))
                {
                    return false;
                }
            }
            return true;
        }
        private bool IsInterlockOkWidthEjectorZ(BaseComponent baseComponent, InterlockEventArgs e)
        {
            bool bRet = true;
            if (this.InputStageEjector.IsEjectorZSafetyPos() == false)
            {
                double dCurrentX = this.AxisX.GetPosition();
                double dCurrentY = this.AxisY.GetPosition();

                if (IsStageInterLockOK(dCurrentX, dCurrentY) == true)
                {
                    if (baseComponent == this.AxisX)
                    {
                        dCurrentX = e.dTargetPosition;
                    }
                    else if (baseComponent == this.AxisY)
                    {
                        dCurrentY = e.dTargetPosition;
                    }
                    if (IsStageInterLockOK(dCurrentX, dCurrentY) == false)
                    {
                        bRet = false;

                    }
                }
                else
                {
                    bRet = false;

                }

                if (bRet == false)
                {
                    PostAlarm((int)AlarmKeys.eInputStageEjectorZNotSafety);

                }
            }

            return bRet;
        }
        public bool SetVacuum(bool on, bool bCheckSignal = false)
        {
            if (_vacuum == null)
                return false;

            if (bCheckSignal == false)
            {
                if (on)
                    _vacuum.On();
                else
                    _vacuum.Off();
            }
            else
            {
                if (on)
                    _vacuum.OnWaitOk();
                else
                    _vacuum.OffWaitOk();
            }

            return true;
        }
        public bool SetClampPlate(bool bUpDn)
        {
            if (_cylPlate == null)
                return false;

            if (bUpDn)
                return _cylPlate.Extend();
            else
                return _cylPlate.Retract();

        }
        public bool SetClampLift(bool bUpDn)
        {
            if (_cylClampLift == null)
                return false;

            if (bUpDn)
                return _cylClampLift.Extend();
            else
            {
                if (!IsClampBwd())
                    return false; // 기존 인터락 유지

                return _cylClampLift.Retract();
            }
        }
        public bool SetClampFB(bool bFwdBwd)
        {
            if (_cylClampFB == null) return false;
            if (bFwdBwd)
            {
                if (!IsClampLiftUp())
                    return false; // 기존 인터락 유지

                return _cylClampFB.Extend();
            }
            else
                return _cylClampFB.Retract();
        }
        public bool IsVacuumOn()
        {
            var equipment = Equipment.Instance;
            bool IsDryRunEqp = equipment.EquipmentConfig.IsDryRun;
            if (Config.IsSimulation || (Config.IsDryRun || IsDryRunEqp))
            {
                return true;
            }
            return this.ReadInput(InputStageConfig.IO.VAC_OK_SNS);
        }
        public bool Ring0()
        {
            var equipment = Equipment.Instance;
            bool IsDryRunEqp = equipment.EquipmentConfig.IsDryRun;
            if (Config.IsSimulation || (Config.IsDryRun || IsDryRunEqp))
            {
                return true;
            }

            return this.ReadInput(InputStageConfig.IO.RING_CHECK0);
        }
        public bool Ring1()
        {
            var equipment = Equipment.Instance;
            bool IsDryRunEqp = equipment.EquipmentConfig.IsDryRun;
            if (Config.IsSimulation || (Config.IsDryRun || IsDryRunEqp))
            {
                return true;
            }

            return this.ReadInput(InputStageConfig.IO.RING_CHECK1);
        }
        public bool IsClampLiftUp()
        {
            if (Config.IsSimulation)
            {
                return true;
            }

            return !IsClampLiftDown();
        }
        public bool IsClampLiftDown()
        {
            if (Config.IsSimulation)
            {
                return true;
            }

            return this.ReadInput(InputStageConfig.IO.CLAMP_DOWN_SNS);
        }
        public bool IsClampFwd()
        {
            if (Config.IsSimulation)
            {
                return true;
            }

            // Clamp Forward 센서 (클램프 전진 상태) 확인
            return this.ReadInput(InputStageConfig.IO.CLAMP_FWD_SNS);
        }
        public bool IsClampBwd()
        {
            if (Config.IsSimulation)
            {
                return true;
            }

            return !IsClampFwd();
        }
        public bool IsPlateUp()
        {
            if (Config.IsSimulation)
            {
                return true;
            }
            return this.ReadInput(InputStageConfig.IO.EXPANDER_UP_SNS);
        }
        public bool IsPlateDown()
        {
            if (Config.IsSimulation)
            {
                return true;
            }
            return this.ReadInput(InputStageConfig.IO.EXPANDER_DOWN_SNS);
        }
        // === Direct Valve Control(강제 구동) ===
        public bool IsVacuumValveOn()
        {
            if (Config.IsSimulation)
            {
                return true;
            }
            return this.IsOutputOn(InputStageConfig.IO.VAC_OUT);
        }
        #endregion

        #region Cylinder High-Level API (With Wait)
        // === Cylinder 고레벨 제어(완료 대기 포함) ===
        public int PlateUp()
        {
            SetClampPlate(true);
            int r = WaitPlateStateOrAlarm(expectUp: true);
            if (r != 0) Log.Write(this, "PlateUp Failed");
            return r;
        }
        public int PlateDown()
        {
            SetClampPlate(false);
            int r = WaitPlateStateOrAlarm(expectUp: false);
            if (r != 0) Log.Write(this, "PlateDown Failed");
            return r;
        }
        public int ClampLiftUp()
        {
            SetClampLift(true);
            int r = WaitClampLiftStateOrAlarm(expectUp: true);
            if (r != 0) Log.Write(this, "ClampLiftUp Failed");
            return r;
        }
        public int ClampLiftDown()
        {
            // 인터락은 SetClampLift(false) 내부에서 IsClampBwd() 확인
            bool issued = SetClampLift(false);
            var equipment = Equipment.Instance;
            bool IsDryRunEqp = equipment.EquipmentConfig.IsDryRun;
            if (!issued && !(Config.IsSimulation || (Config.IsDryRun || IsDryRunEqp)))
            {
                PostAlarm((int)AlarmKeys.eInputStageMoveFail);
                Log.Write(this, "ClampLiftDown Command Rejected (Interlock)");
                return -1;
            }

            int r = WaitClampLiftStateOrAlarm(expectUp: false);
            if (r != 0) Log.Write(this, "ClampLiftDown Failed");
            return r;
        }
        public int ClampForward()
        {
            // 인터락은 SetClampFB(true) 내부에서 IsClampLiftUp() 확인
            bool issued = SetClampFB(true);
            var equipment = Equipment.Instance;
            bool IsDryRunEqp = equipment.EquipmentConfig.IsDryRun;
            if (!issued && !(Config.IsSimulation || (Config.IsDryRun || IsDryRunEqp)))
            {
                PostAlarm((int)AlarmKeys.eInputStageMoveFail);
                Log.Write(this, "ClampForward Command Rejected (Interlock)");
                return -1;
            }

            int r = WaitClampFBStateOrAlarm(expectFwd: true);
            if (r != 0) Log.Write(this, "ClampForward Failed");
            return r;
        }
        public int ClampBackward()
        {
            bool issued = SetClampFB(false);
            var equipment = Equipment.Instance;
            bool IsDryRunEqp = equipment.EquipmentConfig.IsDryRun;
            if (!issued && !(Config.IsSimulation || (Config.IsDryRun || IsDryRunEqp)))
            {
                PostAlarm((int)AlarmKeys.eInputStageMoveFail);
                Log.Write(this, "ClampBackward Command Rejected (Interlock)");
                return -1;
            }

            int r = WaitClampFBStateOrAlarm(expectFwd: false);
            if (r != 0)
                Log.Write(this, "ClampBackward Failed");

            return r;
        }
        #endregion

        #region Unit Lifecycle
        public override int OnRun()
        {
            int ret = 0;
            if (this.RunUnitStatus == UnitStatus.Stopped ||
               this.RunUnitStatus == UnitStatus.Stopping ||
               this.RunUnitStatus == UnitStatus.Error ||
               this.RunUnitStatus == UnitStatus.CycleStop ||
               this.RunUnitStatus == UnitStatus.ManualRunning)
            {
                this.State = ProcessState.Stop;
                return 0;
            }

            if (ret != 0)
            {
                this.State = ProcessState.Stop;
                this.OnStop();
            }
            return ret;
        }
        protected override int OnStart()
        {
            PrepareCameraAndRecipeForAlign();
            return base.OnStart();
        }
        public override int OnStop()
        {
            int ret = 0;
            this.RunUnitStatus = UnitStatus.Stopped;
            base.OnStop();
            return ret;
        }
        protected override int OnRunReady() { return 0; }
        protected override int OnRunWork() { return 0; }
        protected override int OnRunComplete() { return 0; }
        #endregion


        #region Sequence Actions (Manual/Auto Entry)
        public int LoadingWaferComplete(bool isFine = false)
        {
            int nRet = 0;

            if (RunMode == UnitRunMode.Manual)
            {
                this.CurrentFunc = LoadingWaferComplete;
            }

            if (InputFeeder.IsPositionReady() == false)
            {
                Log.Write(UnitName, "LoadingWaferComplete", "Not prepared (call LoadingWaferPrepare first)");
                return -1;
            }

            if (IsRingPresent() == false)
            {
                Log.Write(UnitName, "LoadingWaferComplete", "Not prepared (call LoadingWaferPrepare first)");
                return -1;
            }

            var equipment = Equipment.Instance;
            bool IsDryRunEqp = equipment.EquipmentConfig.IsDryRun;
            if (IsRingPresent() || Config.IsSimulation || (Config.IsDryRun || IsDryRunEqp))
            {
                Log.Write(UnitName, "LoadingWaferComplete", "Wafer detected -> Completing");

                var wafer = GetMaterialWafer();
                if (wafer == null)
                {
                    wafer = new MaterialWafer();
                    SetMaterial(wafer);
                }

                try
                {
                    nRet = PlateUp();
                    if (nRet != 0)
                    {
                        Log.Write(UnitName, "LoadingWaferComplete", "PlateUp Fail");
                        return -1;
                    }

                    nRet = ClampLiftUp();
                    if (nRet != 0)
                    {
                        Log.Write(UnitName, "LoadingWaferComplete", "ClampLiftUp Fail");
                        return -1;
                    }

                    nRet = ClampForward();
                    if (nRet != 0)
                    {
                        Log.Write(UnitName, "LoadingWaferComplete", "ClampForward Fail");
                        return -1;
                    }

                    nRet = MoveToStageCenterPosition();
                    if (nRet != 0)
                    {
                        Log.Write(UnitName, "LoadingWaferComplete", "MoveToStageCenterPosition Fail");
                        return nRet;
                    }
                }
                finally
                {

                }

                Log.Write(UnitName, "LoadingWaferComplete", "Done");
                return nRet;
            }

            return nRet;
        }

        public int AlignTPrepare(bool bFineSpeed = false)
        {
            IsStatus_TAlignPrepared = false;
            IsStatus_TAlignDone = false;
            IsStatus_LastFoundTRawAngle = 0;
            IsStatus_LastAppliedTAngle = 0;
            _lastCenterAlignTp = null;

            // 얼라인 시작 → 이전 맵은 무효. 반드시 리셋
            ResetChipMappingState();

            //20251123 T보정 추가
            ApplyDynamicPitchParameters();
            OnWaferOrRecipeChanged(); // 웨이퍼 교체/레시피 변경 대응

            var equipment = Equipment.Instance;
            bool IsDryRunEqp = equipment.EquipmentConfig.IsDryRun;
            if (this.Config.IsSimulation || (this.Config.IsDryRun || IsDryRunEqp))
            {
                MaterialWafer wafer = GetMaterialWafer();
                if (wafer is null)
                {
                    wafer = new MaterialWafer();
                    SetMaterial(wafer);
                }

                IsStatus_TAlignPrepared = true;
                return 0;
            }
            Log.Write(UnitName, "T_Align", "Prepare Start");

            if (PrepareForAlign(out var centerTp, out var _img) != 0)
            {
                return -1;
            }

            IsStatus_TAlignPrepared = true;
            return 0;
        }

        public int AlignT(bool bFineSpeed = false)
        {
            int nRet = 0;
            if (RunMode == UnitRunMode.Manual)
            {
                this.CurrentFunc = AlignT;
            }

            nRet = InputStageEjector.MovePositionEjectPinOffset();
            if (nRet != 0)
            {
                AxisX.EmgStop();
                AxisY.EmgStop();
                AxisT.EmgStop();
                PostAlarm((int)AlarmKeys.eInputStageEjectorZNotSafety);
                Log.Write(UnitName, "T_Align", "Fail: MovePositionEjectBlockUp");
                return -1;
            }
            SetVacuum(false);
            nRet = InputStageEjector.MovePositionEjectBlockUp();
            if (nRet != 0)
            {
                AxisX.EmgStop();
                AxisY.EmgStop();
                AxisT.EmgStop();
                PostAlarm((int)AlarmKeys.eInputStageEjectorZNotSafety);
                Log.Write(UnitName, "T_Align", "Fail: MovePositionEjectBlockUp");
                return -1;
            }

            nRet = AlignTPrepare(bFineSpeed);
            if (nRet != 0)
            {
                // 내부에서 알람 발생.
                Log.Write(UnitName, "T_Align", "Fail: Prepare");
                return -1;
            }

            nRet = AlignTheta(bFineSpeed);
            if (nRet != 0)
            {
                Log.Write(UnitName, "T_Align", "Fail: AlignTApply");
                return -1;
            }

            return nRet;
        }

        private void PrepareCameraAndRecipeForAlign()
        {
            PmRunner.LoadRecipe();
            var equipment = Equipment.Instance;
            bool IsDryRunEqp = equipment.EquipmentConfig.IsDryRun;
            if (Config.IsSimulation == false && (Config.IsDryRun == false && IsDryRunEqp == false))
            {
                if (StageCamera.IsLiveOn)
                {
                    StageCamera.StopLive();
                    Thread.Sleep(50);
                }
            }
            Thread.Sleep(30);
        }

        private bool TryMeasureTheta(out double angleDeg, out int count, out double stdDeg, bool bFineSpeed = false)
        {
            angleDeg = 0;
            count = 0;
            stdDeg = 0;

            // 멀티 대표값
            if (TryGetRepresentativeTheta(out double rep, out double std, out int c) && c > 0)
            {
                angleDeg = rep;
                stdDeg = std;
                count = c;
                return true;
            }

            // 단일 fallback
            VisionImage img;
            StageCamera.GrabSync(out img);
            if (img == null)
                return false;

            double single;
            PmRunner.SearchTheta(img, out single);

            // 여기서 single==0을 “실패”로 단정하지 말고,
            // 정말 탐색 실패를 나타내는 별도 리턴/플래그가 필요하면 Runner쪽 수정이 맞음.
            // 일단 기존 로직 유지하되, 주변탐색까지 포함:
            angleDeg = single;
            count = 1;

            // single이 0이면 주변 탐색을 시도(옵션)
            if (Math.Abs(single) < 1e-9)
            {
                if (TryFindAngleAroundCenter(out double around, bFineSpeed))
                {
                    angleDeg = around;
                    count = 1;
                    return true;
                }
                return false;
            }

            return true;
        }

        private int ApplyMeasuredTheta(double measuredDeg, bool bFineSpeed)
        {
            if (Math.Abs(measuredDeg) < AngleIgnoreThresholdDeg)
            {
                Log.Write(UnitName, "T_Align",
                    $"Measured angle {measuredDeg:F6}deg ignored (<{AngleIgnoreThresholdDeg})");
                return 0;
            }

            double apply = measuredDeg * AngleApplyGain;
            if (Math.Abs(apply) > AngleMaxApplyDeg)
            {
                Log.Write(UnitName, "T_Align", $"Apply clamp: raw={apply:F4} limit={AngleMaxApplyDeg}");
                apply = Math.Sign(apply) * AngleMaxApplyDeg;
            }

            double cur = AxisT.GetPosition();
            double target = cur + apply;

            if (target < -0.2 || target > 12)
            {
                Log.Write(UnitName, "T_Align", $"Target out of limit: {target:F4}");
                return -1;
            }
            bool IsAuto = false;
            if (RunMode == UnitRunMode.Auto ||
                RunUnitStatus == UnitStatus.AutoRunning ||
                RunUnitStatus == UnitStatus.ManualRunning)
            {
                IsAuto = true;
            }
            else
            {
                IsAuto = false;
            }
            int rc = AxisT.MoveAbs(target, IsAuto, bFineSpeed);
            if (rc != 0) return -1;

            rc = WaitUntil(() => InPos(AxisT, target), MoveTimeoutMs);
            if (rc != 0) return -1;

            IsStatus_LastAppliedTAngle += apply;
            return 0;
        }

        private int RefineThetaWithDualPoint(bool useXAxis, bool bFineSpeed)
        {
            if(useXAxis)
            {
                // X 기준 refine
                if (RefineThetaWithDualPointAxis(useXAxis: true, bFineSpeed) != 0)
                    return -1;
                Log.Write(UnitName, "RefineThetaWithDualPoint", "X-axis dual point refine as configured.");
            }
            else
            {
                // Y 기준 refine
                if (RefineThetaWithDualPointAxis(useXAxis: false, bFineSpeed) != 0)
                    return -1;

                Log.Write(UnitName, "RefineThetaWithDualPoint", "Yaxis dual point refine as configured.");
            }
            return 0;
        }

        int nMaxRetry = 0;
        private int RefineThetaWithDualPointAxis(bool useXAxis, bool bFineSpeed)
        {
            //사이즈를 벗어나서 마크를 못찾으면? 그건 어떻게 하지?
            int maxAttempts = Math.Max(1, MaxTRefineAttempts);
            double toleranceDeg = Math.Max(1e-6, TRefineResidualToleranceDeg);

            //Todo: 
            //step을 wafer 사이즈 기준으로 잡아야함.
            var recipe = Equipment.Instance?.EquipmentRecipe?.CurrentRecipe;
            if (recipe != null)
            {
                maxAttempts = recipe.AlignRepeatCount;
            }
            else
            {
                maxAttempts = 5;
            }

            bool IsAuto = false;
            if (RunMode == UnitRunMode.Auto ||
                RunUnitStatus == UnitStatus.AutoRunning ||
                RunUnitStatus == UnitStatus.ManualRunning)
            {
                IsAuto = true;
            }
            else
            {
                IsAuto = false;
            }

            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                // 1. 측정 (Measure)
                // attempt 변수를 그대로 사용하여 기존의 검색 범위 확장 로직 유지
                if (!TryAcquireDualPointAngle2(useXAxis, out double residualDeg, bFineSpeed, attempt))
                {
                    Log.Write(UnitName, "ThetaRefine", $"Fail attempt {attempt} (axis={(useXAxis ? "X" : "Y")}): cannot acquire dual points.");
                    return -1;
                }

                // 2. 마지막 시도일 때의 처리 (Final Check & Retry Loop)
                if (attempt == maxAttempts)
                {
                    // 2-1. 이미 공차 안에 들어왔는지 확인
                    if (Math.Abs(residualDeg) <= toleranceDeg)
                    {
                        Log.Write(UnitName, "ThetaRefine",
                            $"OK axis={(useXAxis ? "X" : "Y")} attempt {attempt}: residual={residualDeg:F5}deg tol={toleranceDeg:F5}deg");
                        return 0;
                    }
                    else
                    {
                        // 2-2. 마지막 시도에서도 공차를 벗어난 경우 -> [추가 정밀 보정 루프 진입]
                        // 무한정 돌지 않도록 안전장치(SafetyLimit)를 둡니다. (예: 20회 추가 시도)
                        int currentRetry = 0;
                        const int SafetyLimit = 7;

                        Log.Write(UnitName, "ThetaRefine", $"Attempt {attempt} failed ({residualDeg:F5}). Starting Fine-Tune Loop (Max {SafetyLimit})...");

                        // 공차에 들어올 때까지 반복 (단, SafetyLimit 초과 시 중단)
                        while (Math.Abs(residualDeg) > toleranceDeg)
                        {
                            currentRetry++;
                            if (currentRetry > SafetyLimit)
                            {
                                Log.Write(UnitName, "ThetaRefine", $"Fail: Fine-Tune Loop limit reached ({SafetyLimit}). Final residual={residualDeg:F5}");
                                return -1; // 결국 실패
                            }

                            // A. 보정 (Correction)
                            // ApplyThetaCorrection 내부 로직(AngleMaxApplyDeg 제한 등) 활용
                            double fixCorrection = -residualDeg * AngleApplyGain;
                            int rcFix = ApplyThetaCorrection(useXAxis, fixCorrection, IsAuto, bFineSpeed);
                            if (rcFix != 0)
                            {
                                Log.Write(UnitName, "ThetaRefine", "Fail: ApplyThetaCorrection");
                                return -1;
                            }

                            // B. 재측정 (Re-Measure)
                            // 마지막 attempt 단계의 파라미터를 그대로 사용하여 측정
                            if (!TryAcquireDualPointAngle2(useXAxis, out residualDeg, bFineSpeed, attempt))
                            {
                                Log.Write(UnitName, "ThetaRefine", $"Fail in Fine-Tune Loop {currentRetry}: cannot acquire points.");
                                return -1;
                            }

                            // C. 결과 확인 (Check)
                            if (Math.Abs(residualDeg) <= toleranceDeg)
                            {
                                Log.Write(UnitName, "ThetaRefine",
                                    $"OK (Fine-Tune Loop {currentRetry}): residual={residualDeg:F5}deg tol={toleranceDeg:F5}deg");
                                return 0; // 성공!
                            }
                        }
                    }
                }

                // 3. 마지막 시도가 아닐 경우 (for문 중간 단계) -> 일반 보정 수행 후 다음 루프로
                double correction = -residualDeg;
                correction *= AngleApplyGain;
                int rc = ApplyThetaCorrection(useXAxis, correction, IsAuto, bFineSpeed);
                if (rc != 0)
                {
                    return -1;
                }
            }

            Log.Write(UnitName, "ThetaRefine", $"Fail axis={(useXAxis ? "X" : "Y")}: not converged within {maxAttempts} attempts.");
            return -1;
        }

        public int AlignTheta(bool bFineSpeed = false)
        {
            var equipment = Equipment.Instance;
            bool IsDryRunEqp = equipment.EquipmentConfig.IsDryRun;
            if (Config.IsSimulation || (this.Config.IsDryRun || IsDryRunEqp))
            {
                IsStatus_LastAppliedTAngle = 0;
                IsStatus_TAlignDone = true;
                return 0;
            }

            IsStatus_TAlignDone = false;
            try
            {
                //PrepareCameraAndRecipeForAlign();

                // 1) 측정(센터 멀티 우선, 실패 시 단일, 그래도 실패면 주변 탐색 옵션)
                if (TryMeasureTheta(out double measuredDeg, out int sampleCount, out double stdDeg, bFineSpeed) == false)
                {
                    PostAlarm((int)AlarmKeys.eVisionTsearch);
                    Log.Write(UnitName, "T_Align", "Fail: cannot measure theta");
                    return -1;
                }

                IsStatus_LastFoundTRawAngle = measuredDeg;

                // 2) Coarse 적용
                int rc = ApplyMeasuredTheta(measuredDeg, bFineSpeed);
                if (rc != 0)
                {
                    PostAlarm((int)AlarmKeys.eInputStageAlignNotDone);
                    return -1;
                }

                // 3) Refine (옵션)
                if (EnableThetaDualPointRefine)
                {
                    var recipe = Equipment.Instance?.EquipmentRecipe?.CurrentRecipe;
                    // 레시피에 설정된 AlignAxisX 사용 (null이면 기본 true)
                    bool useXAxis = recipe?.AlignAxisX ?? true;
                    //bool useXAxis = true;   // ThetaDualPointRefineUseXAxis;
                    rc = RefineThetaWithDualPoint(useXAxis, bFineSpeed);
                    if (rc != 0)
                    {
                        PostAlarm((int)AlarmKeys.eInputStageAlignNotDone);
                        return -1;
                    }
                }

                IsStatus_TAlignDone = true;
                return 0;
            }
            catch (Exception ex)
            {
                Log.Write(UnitName, "T_Align", $"Exception: {ex}");
                PostAlarm((int)AlarmKeys.eInputStageAlignNotDone);
                return -1;
            }
        }

        public int AlignXY(bool bFineSpeed = false)
        {
            int nRet = 0;
            if (RunMode == UnitRunMode.Manual)
            {
                this.CurrentFunc = AlignXY;
            }

            nRet = AlignXYPrepare(bFineSpeed);
            if (nRet != 0)
            {
                Log.Write(UnitName, "XY_Align", "Fail: Prepare");
                return -1;
            }
            nRet = AlignXYApply(bFineSpeed);
            if (nRet != 0)
            {
                Log.Write(UnitName, "XY_Align", "Fail: Apply");
                return -1;
            }

            return nRet;
        }
        public int AlignXYPrepare(bool bFineSpeed = false)
        {
            IsStatus_XYAlignPrepared = false;
            IsStatus_XYAlignDone = false;
            IsStatus_LastFoundDx = 0;
            IsStatus_LastFoundDy = 0;
            _lastCenterAlignTp = null;

            Log.Write(UnitName, "XY_Align", "Prepare Start");

            IsStatus_XYAlignPrepared = true;
            return 0;
        }

        public int AlignXYApply(bool bFineSpeed = false)
        {
            var equipment = Equipment.Instance;
            bool IsDryRunEqp = equipment.EquipmentConfig.IsDryRun;
            if (this.Config.IsSimulation || (this.Config.IsDryRun || IsDryRunEqp))
            {
                _lastCenterAlignTp = new TeachingPosition();

                IsStatus_XYAlignDone = true;
                return 0;
            }

            IsStatus_XYAlignDone = true;
            return 0;
        }

        public int MoveStageToNextDie(bool bFine = false)
        {
            int nRet = 0;

            if (RunMode == UnitRunMode.Manual)
            {
                CurrentFunc = MoveStageToNextDie;
            }

            MaterialDie die;
            nRet = MoveStageToNextDie(out die);
            if (nRet != 0)
            {
                Log.Write(UnitName, "MoveStageToNextDie", "Fail");
                return nRet;
            }

            return nRet;
        }
        public int MoveStageToNextDie(out MaterialDie die)
        {
            int nRet = 0;
            die = GetNextDie();
            if (die == null)
            {
                return -1;
            }
            if (die.Presence != MaterialPresence.Exist)
            {
                return -1;
            }

            if(false)
            {
                nRet = MoveStage(die.CenterX, die.CenterY, false);
                return nRet;

            }
            else
            {
                //NextDie 이동 시 Offset Data
                double dOffsetX = 0, dOffsetY = 0;
                double dMoveX = 0, dMoveY = 0;

                dOffsetX = this.Config.dOffsetDieX;
                dOffsetY = this.Config.dOffsetDieY;

                //
                if(Math.Abs(dOffsetX) > 0.5 ||
                   Math.Abs(dOffsetY) > 0.5  )
                {
                    dMoveX = die.CenterX;
                    dMoveY = die.CenterY;
                }
                else
                {
                    dMoveX = die.CenterX + dOffsetX;
                    dMoveY = die.CenterY + dOffsetY;
                }

                nRet = MoveStage(dMoveX, dMoveY, false);
                return nRet;
            }
        }

        #endregion


        #region Mapping / Pickup
        public double ChipPitchXmm
        {
            get
            {
                var eq = Equipment.Instance;
                var recip = eq.EquipmentRecipe.CurrentRecipe;
                return recip.WChipPitchX;
            }
            set
            {
                var eq = Equipment.Instance;
                var recip = eq.EquipmentRecipe.CurrentRecipe;
                recip.WChipPitchX = value;
            }
        }
        public double ChipPitchYmm
        {
            get
            {
                var eq = Equipment.Instance;
                var recip = eq.EquipmentRecipe.CurrentRecipe;
                return recip.WChipPitchY;
            }
            set
            {
                var eq = Equipment.Instance;
                var recip = eq.EquipmentRecipe.CurrentRecipe;
                recip.WChipPitchY = value;
            }
        }
        public class ChipMapEntry
        {
            public int Index;
            public int Row;
            public int Col;
            public double Xmm;
            public double Ymm;
            public bool Present;
            public bool Enabled;
            public double Score;
        }
        public class ChipMapResult
        {
            public int Rows;
            public int Cols;
            public double PitchX;
            public double PitchY;
            public double OriginX;
            public double OriginY;
            public List<ChipMapEntry> Entries = new List<ChipMapEntry>();

            public IEnumerable<ChipMapEntry> EnumeratePickup()
                => Entries.Where(e => e.Present && e.Enabled).OrderBy(e => e.Index);
        }
        public double DuplicateDistMm { get; set; } = 0.8;          // 중복 판단
        public double MarkMinScore { get; set; } = 0.8;             // Vision 점수 기준 (예시)
        public double MissingAllowScore { get; set; } = 0.5;
        public int MappingMoveTimeoutMs { get; set; } = 4000;
        public bool UseVisionOffsetApply { get; set; } = false;   // 필요시 Vision 미세 중심 보정

        public ChipMapResult CurrentChipMap { get; set; }
        public bool ChipMappingDone { get; set; }
        private int _chipPickupCursor = 0;

        // InputStage class 내부 (PerformChipMapping 근처) - 기존 PerformChipMapping 교체 + private helper 추가

        // ===== PerformChipMapping 관련: 추가 분리 (기능 동일, 구조만 개선) =====

        public int PerformChipMapping(bool bFineSpeed = false)
        {
            if (RunMode == UnitRunMode.Manual)
                this.CurrentFunc = PerformChipMapping;

            // 맵핑 시작 시점에 상태/커서/결과를 명확히 리셋
            // (스캔 함수 내부에서도 리셋을 하고 있었는데, 외부에서 먼저 리셋하는 편이
            //  실패/중단 시 상태 일관성이 좋아짐)
            ResetChipMappingState();
            StopStageCameraLiveIfNeeded();

            if (!EnsureMappingPrerequisitesOrAlarm())
                return -1;

            if (PrepareEjectorForMappingOrAlarm() != 0)
                return -1;

            MaterialWafer wafer;
            int rc = PerformHardwareScanAndBuildWaferMap(bFineSpeed, out wafer);
            if (rc != 0)
            {
                return rc;
            }

            // wafer null 방어 (예외 방지)
            if (wafer == null)
            {
                AxisX?.EmgStop(); AxisY?.EmgStop(); AxisT?.EmgStop();
                PostAlarm((int)AlarmKeys.eInputStageScanEmpty); // 가장 근접한 알람 재사용 (전용 알람키 있으면 교체 권장)
                Log.Write(UnitName, "PerformChipMapping", "Fail: wafer is null after scan");
                ChipMappingDone = false;
                return -1;
            }
            
            // die 0개 방어 (스캔 실패/레시피 문제/카메라 문제 가능)
            if (wafer.Dies == null || wafer.Dies.Count == 0)
            {
                AxisX?.EmgStop(); AxisY?.EmgStop(); AxisT?.EmgStop();
                PostAlarm((int)AlarmKeys.eInputStageScanEmpty); // 전용 알람키 있으면 교체 권장
                Log.Write(UnitName, "PerformChipMapping", "Fail: Scan completed but die count is 0");
                ChipMappingDone = false;
                return -1;
            }

            // 맵매칭하고 외곽라인 스킵 및 삭제 하자.
            //var recipe = Equipment.Instance.EquipmentRecipe.CurrentRecipe;
            //// [ADD] 외곽 N줄 스킵(Reject 처리)
            //if (recipe != null)
            //{ 
            //    OuterBorderSkipRows = recipe.DieSkipLine;
            //    OuterBorderSkipShape = recipe.DieSkipShape;
            //}
            //bool bSkipMode = true;
            //if(bSkipMode)
            //{
            //    ApplyOuterBorderSkipOrReject(wafer, OuterBorderSkipRows);
            //}
            //else
            //{
            //    // [변경 후] 아예 삭제하고 싶을 경우 (이걸로 교체)
            //    RemoveOuterBorderDies(wafer, OuterBorderSkipRows, OuterBorderSkipShape);
            //}

            rc = EvaluateMapMatchAndDecide(wafer);
            if (rc != 0)
            {
                EventUpdateUIWafer?.BeginInvoke(wafer, null, null);
                ChipMappingDone = false;
                return rc;
            }

            var recipe = Equipment.Instance.EquipmentRecipe.CurrentRecipe;
            // [ADD] 외곽 N줄 스킵(Reject 처리)
            if (recipe != null)
            {
                OuterBorderSkipRows = recipe.DieSkipLine;
                OuterBorderSkipShape = recipe.DieSkipShape;
            }
            bool bSkipMode = true;
            if (bSkipMode)
            {
                ApplyOuterBorderSkipOrReject(wafer, OuterBorderSkipRows);
            }
            else
            {
                // [변경 후] 아예 삭제하고 싶을 경우 (이걸로 교체)
                RemoveOuterBorderDies(wafer, OuterBorderSkipRows, OuterBorderSkipShape);
            }


            // [ADD] MapMatch OK 확정 후에도 OK Die -> Rank=1 재적용(수동 변환/재매칭 후에도 일관성 보장)
            ApplyOkRankToDies(wafer, 1);
            MarkNonRank1DiesAsSkip(wafer, 1);

            // MapMatch하고 적용하도록 옮기자.
            // die 공정 순서 정의 및 index 정의.
            ApplyAndNormalizeDieOrder(wafer);

            // 최종 total Count Update.
            TrySummaryUpdateTotalCount(wafer.Dies.Count);

            // ---------------------------------------------------------
            // [수정됨] 맵핑 완료 후 첫 번째 칩 위치로 선행 이동 (Pre-move)
            // ---------------------------------------------------------
            wafer.ProcessSatate = Material.MaterialProcessSatate.Processing;
            Log.Write(UnitName, "PerformChipMapping", "Pre-moving to the first die position...");

            // 첫 번째 픽업 대상 다이를 가져옵니다.
            var firstDie = GetNextDie();
            if (firstDie != null)
            {
                // 첫 번째 다이 위치로 이동
                int moveRet = MoveStage(firstDie.CenterX, firstDie.CenterY, bFineSpeed);
                if (moveRet != 0)
                {
                    Log.Write(UnitName, "PerformChipMapping", $"Fail: Pre-move to 1st die failed (Idx={firstDie.Index})");
                    // 이동 실패 시 알람 처리 및 리턴 - 필요에 따라 주석 처리 가능하나 안전을 위해 실패 처리 권장
                    return moveRet;
                }

                // 이동 후 위치 안정화 대기 (InPosition 확인은 MoveStage 내부에서 수행됨)
                Log.Write(UnitName, "PerformChipMapping", $"Success: Pre-moved to 1st die (Idx={firstDie.Index}, X={firstDie.CenterX:F3}, Y={firstDie.CenterY:F3})");
            }
            else
            {
                Log.Write(UnitName, "PerformChipMapping", "Warning: No pickable die found for pre-move.");
            }
            // ---------------------------------------------------------

            ChipMappingDone = true; // GetNextDie()가 정상 동작하려면 true여야 함
            EventUpdateUIWafer?.BeginInvoke(wafer, null, null);
            
            return 0;
        }

        private PointD RotateMm(PointD p, double angleDeg)
        {
            double rad = angleDeg * Math.PI / 180.0;
            double c = Math.Cos(rad);
            double s = Math.Sin(rad);
            return new PointD(p.X * c - p.Y * s, p.X * s + p.Y * c);
        }

        // 픽셀 결과를 stage(mm)로 바꿀 때 T까지 반영
        private PointD PixelToStageOffsetMm(double px, double py, double thetaDeg)
        {
            var off = GetPixelToMmScale(px, py);

            // TODO: 부호(+/-)는 장비 좌표계에 따라 달라질 수 있음.
            // 먼저 -theta로 적용해보고, 맵 오차가 반대 방향이면 +theta로 바꾸면 됨.
            return RotateMm(off, -thetaDeg);
        }

        private void StopStageCameraLiveIfNeeded()
        {
            var equipment = Equipment.Instance;
            bool IsDryRunEqp = equipment.EquipmentConfig.IsDryRun;
            if (Config.IsSimulation || (Config.IsDryRun || IsDryRunEqp))
                return;

            if (StageCamera != null && StageCamera.IsLiveOn)
            {
                StageCamera.StopLive();
                Thread.Sleep(50);
            }
        }
        private bool EnsureMappingPrerequisitesOrAlarm()
        {
            // 기본 인터락(기존 유지)
            if (IsStatus_TAlignDone == false)
            {
                AxisX.EmgStop();
                AxisY.EmgStop();
                AxisT.EmgStop();
                PostAlarm((int)AlarmKeys.eInputStageAlignNotCompleted);
                Log.Write(UnitName, "PerformChipMapping", "Align not completed");
                return false;
            }

            if (RunMode == UnitRunMode.Auto)
            {
                if (IsRingPresent() == false)
                {
                    AxisX.EmgStop();
                    AxisY.EmgStop();
                    AxisT.EmgStop();
                    PostAlarm((int)AlarmKeys.eInputStageRingPresent);
                    Log.Write(UnitName, "PerformChipMapping", "Wafer (Ring) not present");
                    return false;
                }
            }

            return true;
        }
        /// <summary>
        /// 1) 하드웨어적으로 스캔하여 좌표 생성 + 웨이퍼 Dies 생성/정렬/인덱싱 + Scan Summary 반영
        /// </summary>
        private int PerformHardwareScanAndBuildWaferMap(bool bFineSpeed, out MaterialWafer wafer)
        {
            wafer = null;
            TrySummaryStartScan();
            try
            {
                wafer = GetMaterialWafer();

                ApplyDynamicPitchParameters();
                OnWaferOrRecipeChanged();

                MakeScanPath(out List<PointD> path);

                List<PointD> raw = new List<PointD>();

                int rc = ScanPathAndCollectRawChips(path, bFineSpeed, raw);
                if (rc != 0)
                    return rc;

                //맵서치완료한 다이 정보 로그 출력을 다른 파일에 저장하도록 변경
                int nIndex = 0;
                DateTime now = DateTime.Now;
                string logFile = string.Empty;
                logFile = string.Format("DieLowDataLog_{0}_{1}", wafer.WaferId, now.ToString("yyyyMMdd_HHmmss"));
                Log.Write(logFile, "rawList", "Index,posX ,posY");
                foreach (var c in raw)
                {
                    if (true)
                    {
                        Log.Write(logFile, "rawList", $"{nIndex},{c.X},{c.Y}");
                        nIndex++;
                    }
                }

                // [TEST] 중복 칩 병합 사용 여부 토글 : false로 설정됨
                if (true)
                {
				    raw = ConsolidateRawChips(raw);
                }

                // UpdateChipInfo 내부에서 wafer 생성/설정까지 수행
                UpdateChipInfo(raw);
                
                if (wafer != null)
                {
                    if (Config.IsSimulation)
                    {
                        if(SimUseRawChipFile == false)
                        {
                            if(false) // 개별 Test용.
                            {
                                //시뮬레이션 모드에서는 다이맵로그파일을 읽어서 다이정보를 넣어주자.
                                //eadDieMapLogFile(wafer);
                            }
                        }
                    }
                    // [ADD] Scan 완료 시 OK Die -> Rank=1
                    ApplyOkRankToDies(wafer, 1);
                    MarkNonRank1DiesAsSkip(wafer, 1);
                    //EventUpdateUIWafer?.BeginInvoke(wafer, null, null);
                }

                return 0;
            }
            finally
            {
                TrySummaryStopScan();
            }
        }

        public bool SimUseRawChipFile { get; set; } = true; // UI에서 옵션으로 열어도 됨
        public string SimRawChipFilePath { get; private set; } = ""; // 선택된 파일 경로 캐시
        private int ScanPathAndCollectRawChips(List<PointD> path, bool bFineSpeed, List<PointD> rawOut)
        {
            if (rawOut == null)
                return -1;

            Task<int> tImageProcess = null;
            try
            {
                foreach (var pt in path)
                {
                    if (this.IsStop)
                    {
                        Log.Write(UnitName, "ChipMap", "IsStop");
                        return 0;
                    }

                    int rcMove = MoveStage(pt.X, pt.Y, bFineSpeed);
                    if (rcMove != 0)
                    {
                        AxisX.EmgStop();
                        AxisY.EmgStop();
                        AxisT.EmgStop();
                        Log.Write(UnitName, "ChipMap", "Fail: MoveStage");
                        PostAlarm((int)AlarmKeys.eInputStageMoveFail);
                        return -1;
                    }

                    var equipment = Equipment.Instance;
                    bool IsDryRunEqp = equipment.EquipmentConfig.IsDryRun;
                    if (Config.IsSimulation || (Config.IsDryRun || IsDryRunEqp))
                    {
                        const int maxSimChips = 20000;
                        rawOut.Clear();
                        bool loadedFromFile = false;

                        SimUseRawChipFile = false;
                        if (SimUseRawChipFile)
                        {
                            //할때마다 초기화 하자.
                            SimRawChipFilePath = string.Empty;

                            if (string.IsNullOrWhiteSpace(SimRawChipFilePath))
                            {
                                // Auto 운전 중에도 파일 선택창 띄우기
                                var loadpath = AskSimRawChipFilePathAsync().GetAwaiter().GetResult(); // 또는 .Wait()
                                if (!string.IsNullOrWhiteSpace(loadpath))
                                    SimRawChipFilePath = loadpath; // set 접근자 필요하면 private set -> set으로 변경
                            }

                            if (!string.IsNullOrWhiteSpace(SimRawChipFilePath) &&
                                TryLoadRawChipsFromFile(SimRawChipFilePath, out var fileChips, maxSimChips))
                            {
                                rawOut.AddRange(fileChips);
                                loadedFromFile = true;

                                Log.Write(UnitName, "Sim",
                                    $"[ChipMap] Loaded chips from file. chips={rawOut.Count} file='{SimRawChipFilePath}'");
                            }
                        }

                        if (!loadedFromFile)
                        {
                            EnsureSimDiePoolGenerated();
                            if (maxSimChips > 0 && _simAllDiesPool.Count > maxSimChips)
                                rawOut.AddRange(_simAllDiesPool.Take(maxSimChips));
                            else
                                rawOut.AddRange(_simAllDiesPool);

                            Log.Write(UnitName, "Sim",
                                $"[ChipMap] Use generated sim pool. chips={rawOut.Count} (pool={_simAllDiesPool.Count})");
                        }

                        break;
                    }

                    if (tImageProcess != null)
                        tImageProcess.Wait();

                    StageCamera.SuspendedImageDisplay = true;

                    StageCamera.GrabSync(out VisionImage grabImage);
                    double dx = pt.X;
                    double dy = pt.Y;

                    Thread.Sleep(30);
                    tImageProcess = Task.Factory.StartNew(() =>
                    {
                        return SearchDies(grabImage, ref rawOut, dx, dy);
                    });
                }

                if (tImageProcess != null)
                    tImageProcess.Wait();

                return 0;
            }
            catch (OperationCanceledException)
            {
                Log.Write(UnitName, "ChipMap", "Cancelled");
                return 0;
            }
            catch (Exception ex)
            {
                Log.Write(UnitName, "ChipMap", "Exception: " + ex.Message);
                return -1;
            }
        }
        private List<PointD> ConsolidateRawChips(List<PointD> raw)
        {
            double tol = DuplicateDistMm;

            double pitchMin = double.MaxValue;
            if (ChipPitchXmm > 0) pitchMin = Math.Min(pitchMin, ChipPitchXmm);
            if (ChipPitchYmm > 0) pitchMin = Math.Min(pitchMin, ChipPitchYmm);
            if (pitchMin < double.MaxValue)
                tol = Math.Min(tol, 0.49 * pitchMin);

            var consolidated = ConsolidateChipCenters(raw, tol);

            Log.Write(UnitName, "ChipMap",
                $"RawCount={(raw != null ? raw.Count : 0)} Consolidated={consolidated.Count} MergeDist={DuplicateDistMm:F3}mm");

            return consolidated;
        }
        private void ApplyAndNormalizeDieOrder(MaterialWafer wafer)
        {
            ApplyDieOrderByPathSettings(wafer);
            NormalizeIndicesSequential(wafer, startIndex: 0, rename: true);
        }
        /// <summary>
        /// 2) 상위에서 맵 다운로드/비교 후 진행 여부 판단 (MapMatchMode 기준)
        /// 기존 consolidated 파라미터는 Evaluate 단계에서 사용하지 않으므로 제거 (동작 동일)
        /// </summary>
        private int EvaluateMapMatchAndDecide(MaterialWafer wafer)
        {
            TrySummaryStartSort();
            try
            {
                if (wafer == null)
                {
                    Log.Write(UnitName, "MapMatch", "No wafer instance. Skip.");
                    return 0;
                }
                if (wafer.Dies == null || wafer.Dies.Count == 0)
                {
                    Log.Write(UnitName, "MapMatch", "No scanned Dies to match. Skip.");
                    return 0;
                }

                if (!IsMapMatchModeEnabled())
                {
                    return AskContinueWhenMapMatchDisabled(wafer);
                }

                // MapMatchMode = true
                string mapFile = PrepareMapFileForMatchingOrAlarm(wafer);
                if (string.IsNullOrWhiteSpace(mapFile))
                    return -1;

                var equipment = Equipment.Instance;
                bool IsDryRunEqp = equipment.EquipmentConfig.IsDryRun;
                if (Config.IsSimulation || (Config.IsDryRun || IsDryRunEqp))
                {
                    Log.Write(UnitName, "MapMatch", "Simulation/DryRun -> skip file-based map matching.");
                    //return 0;
                }
                return RunMapMatchAndDecide(wafer, mapFile);
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                return -1;
            }
            finally
            {
                TrySummaryStopSort();
            }
        }

        private bool IsMapMatchModeEnabled()
        {
            var eqpConfig = Equipment.Instance.EquipmentConfig;
            return eqpConfig != null && eqpConfig.MapMatchMode;
        }
        private int AskContinueWhenMapMatchDisabled(MaterialWafer wafer)
        {
            // [ADD] 외곽 N줄 스킵(Reject 처리)
            //var recipe = Equipment.Instance?.EquipmentRecipe?.CurrentRecipe;
            //if (recipe != null)
            //{
            //    OuterBorderSkipRows = recipe.DieSkipLine;
            //}
            //ApplyOuterBorderSkipOrReject(wafer, OuterBorderSkipRows);
            // [ADD] Skip die 제거 (복구 불가)
            //RemoveSkippedDiesFromWafer("MapMatchDisabled_RemoveSkippedDies");

            // 외곽 N줄 스킵(Reject 처리) 내부에서 함.
            string strMapFile = string.Empty;
            strMapFile = "MapMatchDisabled_NoFile";
            //strMapFile = "D:\\MapTestWafer.waf";
            return RunMapMatchAndDecide(wafer, strMapFile);

            // 기존: 맵핑 완료 후 사용자 확인
            //if (Config.IsSimulation == false)
            //{
            //    var ask = new MessageBoxYesNo();
            //    ask.TopMost = true;
            //    if (ask.ShowDialog("진행 유무 확인", "맵핑 완료. 진행 하시겠습니까?") != DialogResult.Yes)
            //    {
            //        OnStop();
            //        ChipMappingDone = false;

            //        var eq = Equipment.Instance;
            //        eq.SequenceStopAllAsync(CancellationToken.None);

            //        Log.Write(UnitName, "TryShutdownIfAllCassettesEmpty", "모든 관련 Unit 정지 완료.");
            //        return -1;
            //    }
            //}
            //return 0;
        }
        private string PrepareMapFileForMatchingOrAlarm(MaterialWafer wafer)
        {
            var recipe = Equipment.Instance.EquipmentRecipe.CurrentRecipe;
            string mapFilePath = recipe.MapFilePath;
            return PrepareLocalMapFileOrAlarm(wafer, mapFilePath);
        }

        private int RunMapMatchAndDecide(MaterialWafer wafer, string mapFile)
        {
            // 방어
            if (wafer == null)
                return -1;

            if (wafer.Dies == null || wafer.Dies.Count == 0)
            {
                PostAlarm((int)AlarmKeys.eInputStageScanEmpty);
                Log.Write(UnitName, "MapMatch", "Scan Dies empty.");
                return -1;
            }

            var recipe = Equipment.Instance?.EquipmentRecipe?.CurrentRecipe;
            try
            {
                // mapFile이 있으면 기존 로직(파일 기반) 사용
                if (!string.IsNullOrWhiteSpace(mapFile) && File.Exists(mapFile))
                {
                    //var orgPreview = wafer.ReadFileOnline(mapFile, MaterialWafer.MapTyp.waf);
                    var orgPreview = wafer.ReadFileOnline(mapFile, MaterialWafer.MapTyp.txt);
                    if (orgPreview == null || orgPreview.Count == 0)
                    {
                        PostAlarm((int)AlarmKeys.eInputStageAlignNotCompleted);
                        Log.Write(UnitName, "MapMatch", $"Original map parse failed or empty: {mapFile}");
                        return -1;
                    }

                    TrySummaryUpdateOrgAndScanCount(orgPreview.Count, wafer.Dies != null ? wafer.Dies.Count : 0);

                    double bestScore = wafer.Mapmatch(mapFile, MaterialWafer.MapTyp.txt) * 100.0;

                    Log.Write(UnitName, "MapMatch",
                        $"Done. Score={bestScore:F3} OrgCount={orgPreview.Count} ScanCount={wafer.Dies.Count} MapFile='{mapFile}'");

                    double scoreThreshold = recipe != null ? recipe.WaferMatchLimitPercent : 0.0;

                    int rc = DecideWithManualRematchLoop(wafer, mapFile, ref bestScore, scoreThreshold);
                    if (rc != 0)
                        return rc;

                    // 여기에서 다운로드한맵 안에 있는 die와 스캔한 die만 남기고 나머지는 skip 처리
                    // --------------------------------------------------------------------------------
                    // [ADD] 원본 맵(orgPreview)에 없는 Die는 Skip 처리 (다운로드 맵 기준 필터링)
                    // --------------------------------------------------------------------------------
                    try
                    {
                        // 1. 원본 맵의 좌표들을 빠른 검색을 위해 HashSet에 등록 (Key: MapX, MapY)
                        //    MaterialWafer.ReadFileOnline 반환 타입이 List<object> (또는 유사)이므로 캐스팅 주의 필요
                        //    기존 코드를 보면 orgPreview의 요소 타입이 명시되지 않았으나, 보통 MaterialDie나 유사 구조체일 것입니다.
                        //    안전을 위해 dynamic이나 reflection, 혹은 제공된 컨텍스트의 MaterialDie 속성을 가정합니다.

                        var validMapCoords = new HashSet<(int x, int y)>();

                        foreach (var item in orgPreview)
                        {
                            // item이 MaterialDie 타입이거나, MapX/MapY 프로퍼티를 가진 객체라고 가정
                            if (item is MaterialDie d)
                            {
                                validMapCoords.Add(((int)d.MapX, (int)d.MapY));
                            }
                            else
                            {
                                // 만약 item이 object 타입이라 속성 접근이 어렵다면 리플렉션이나 dynamic 사용
                                // (프로젝트 구조상 MaterialDie일 확률이 높음)
                                try
                                {
                                    dynamic dynItem = item;
                                    validMapCoords.Add(((int)dynItem.MapX, (int)dynItem.MapY));
                                }
                                catch { /* 타입 불일치 시 무시 */ }
                            }
                        }

                        bool bDieSkipMode = false;
                        if(bDieSkipMode) //Skip할떄
                        {
                            int skippedByMapFilter = 0;
                            lock (wafer.Dies)
                            {
                                foreach (var die in wafer.Dies)
                                {
                                    if (die == null) continue;
                                    if (die.Presence != MaterialPresence.Exist)
                                        continue;

                                    // 이미 다른 사유(MapMatch 점수 미달 등)로 Skip/Reject 된 경우 패스하려면 아래 조건 추가
                                    // if (die.State != DieProcessState.Mapped) continue;

                                    // 2. 원본 맵에 없는 좌표인지 확인
                                    if (!validMapCoords.Contains(((int)die.MapX, (int)die.MapY)))
                                    {
                                        // 원본 맵에 없으면 Skip 처리
                                        die.SetSkip("NotInDownloadMap");
                                        skippedByMapFilter++;
                                    }
                                }
                            }
                            Log.Write(UnitName, "MapMatch", $"Filtered dies not in original map. Skipped count={skippedByMapFilter}");
                        }
                        else // 아에 제거 하고싶을때
                        {
                            int removedCount = 0;
                            lock (wafer.Dies)
                            {
                                // 2. 리스트 역순 순회 혹은 RemoveAll을 사용하여 조건에 맞지 않는 Die 제거
                                //    (원본 맵에 좌표가 없으면 제거)
                                int beforeCount = wafer.Dies.Count;

                                wafer.Dies.RemoveAll(die =>
                                    die != null &&
                                    die.Presence == MaterialPresence.Exist &&
                                    !validMapCoords.Contains(((int)die.MapX, (int)die.MapY))
                                );

                                removedCount = beforeCount - wafer.Dies.Count;

                                // 3. 인덱스 재정렬 (0부터 순차 부여)
                                if (removedCount > 0)
                                {
                                    NormalizeIndicesSequential(wafer, startIndex: 0, rename: true);
                                }
                            }

                            Log.Write(UnitName, "MapMatch", $"Filtered dies not in original map. Removed count={removedCount}");

                        }

                    }
                    catch (Exception ex)
                    {
                        // 필터링 중 에러가 나더라도 전체 프로세스를 죽이지 않도록 로그만 남기고 진행
                        Log.Write(UnitName, "MapMatch", $"Error during map filtering: {ex.Message}");
                    }
                    // --------------------------------------------------------------------------------

                    return 0;
                }

                // ============================
                // mapFile이 없는 경우(Manual / 다운로드 불가 등)
                // - "원본맵"을 wafer.Dies 기반으로 구성해서 화면/판단 흐름을 동일하게 제공
                // ============================

                var preview = BuildPreviewFromWaferDies(wafer);
                if (preview == null || preview.Count == 0)
                {
                    PostAlarm((int)AlarmKeys.eInputStageAlignNotCompleted);
                    Log.Write(UnitName, "MapMatch", "Preview generation failed (wafer.dies -> preview).");
                    return -1;
                }

                // 파일 기반이 아니므로 org=scan 동일 취급으로 Summary만 반영
                TrySummaryUpdateOrgAndScanCount(preview.Count, wafer.Dies.Count);

                // 초기 스코어는 100으로 시작(원본=스캔으로 간주)
                // 이후 수동 변환이 들어오면, 아래 내부 스코어링으로 다시 계산
                double bestScoreNoFile = 100.0;

                double threshold = recipe != null ? recipe.WaferMatchLimitPercent : 0.0;

                // mapFile이 없으므로 "수동 변환 후 재매칭"은 내부 스코어링으로 처리
                int rcNoFile = DecideWithManualRematchLoop_NoMapFile(wafer, ref bestScoreNoFile, threshold);
                if (rcNoFile != 0)
                    return rcNoFile;

                ApplyOkRankToDies(wafer, 1);
                MarkNonRank1DiesAsSkip(wafer, 1);

                if (recipe != null)
                    OuterBorderSkipRows = recipe.DieSkipLine;

                ApplyOuterBorderSkipOrReject(wafer, OuterBorderSkipRows);

                EventUpdateUIWafer?.BeginInvoke(wafer, null, null);
                return 0;
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                return -1;
            }
        }

        private List<object> BuildPreviewFromWaferDies(MaterialWafer wafer)
        {
            // MaterialWafer.ReadFileOnline(mapFile) 반환 타입이 프로젝트에서 "List<...>"인데,
            // 여기 파일 컨텍스트만으로 정확한 제네릭 타입을 특정할 수 없어서 object로 반환.
            // 실제 이 preview는 count/log 용도라서 타입 안정성이 필요 없도록 설계.
            try
            {
                if (wafer?.Dies == null)
                    return null;

                // Map 좌표가 있는 die만
                var dies = wafer.Dies.Where(d => d != null && d.Presence == MaterialPresence.Exist).ToList();
                if (dies.Count == 0)
                    return new List<object>();

                // “원본맵”이라고 가정할 엔트리 개수만 맞추면 됨(현 코드에서 orgPreview는 Count/log/summary 목적)
                var list = new List<object>(dies.Count);
                for (int i = 0; i < dies.Count; i++)
                    list.Add(dies[i]);

                return list;
            }
            catch (Exception ex)
            {
                Log.Write(UnitName, nameof(BuildPreviewFromWaferDies), ex.Message);
                return null;
            }
        }


        private double ComputeInternalMatchScorePercent(MaterialWafer wafer)
        {
            // 파일이 없을 때 “수동 변환이 얼마나 안정적인지”를 대충이라도 표현하기 위한 내부 점수.
            // 기준:
            //  - MapX/MapY가 얼마나 촘촘하게(중복 없이) 분포하는지
            //  - 중복(MapX,MapY) 발생률이 낮을수록 점수 높음
            try
            {
                if (wafer?.Dies == null || wafer.Dies.Count == 0)
                    return 0.0;

                var dies = wafer.Dies.Where(d => d != null && d.Presence == MaterialPresence.Exist).ToList();
                if (dies.Count == 0)
                    return 0.0;

                var set = new HashSet<long>();
                int dup = 0;

                foreach (var d in dies)
                {
                    long key = (((long)((int)d.MapX)) << 32) ^ (uint)((int)d.MapY);
                    if (!set.Add(key))
                        dup++;
                }

                // 중복이 0이면 100, 전부 중복이면 0
                double unique = dies.Count - dup;
                double score = (unique / Math.Max(1.0, dies.Count)) * 100.0;
                if (score < 0) score = 0;
                if (score > 100) score = 100;
                return score;
            }
            catch (Exception ex)
            {
                Log.Write(UnitName, nameof(ComputeInternalMatchScorePercent), ex.Message);
                return 0.0;
            }
        }

        private int DecideWithManualRematchLoop(MaterialWafer wafer, string mapFile, ref double bestScore, double scoreThreshold)
        {
            while (true)
            {
                double retryScore = ComputeInternalMatchScorePercent(wafer);

                Log.Write(UnitName, "MapMatch",
                    $"Rematch after manual. Score={retryScore:F2} Threshold={scoreThreshold:F2}");

                bestScore = retryScore;

                // Score보다 높으면 자동으로 시작. 
                // 우선 막고 가자.
                if(false)
                {
                    if (Config.IsSimulation == false)
                    {
                        if (bestScore >= scoreThreshold)
                            return 0;
                    }
                }

                DialogResult dr = DialogResult.None;

                // [수정] UI 스레드에서 폼 생성 및 호출
                var mainForm = System.Windows.Forms.Application.OpenForms.Count > 0 ? 
                               System.Windows.Forms.Application.OpenForms[0] : null;

                if (mainForm != null && mainForm.InvokeRequired)
                {
                    mainForm.Invoke((MethodInvoker)delegate
                    {
                        using (var dlg = new FormMapMatchManual())
                        {
                            try
                            {
                                // 화면 최상위 설정
                                dlg.TopMost = true;
                                dlg.StartPosition = FormStartPosition.CenterScreen;

                                // Scan = 항상 장비 웨이퍼(현재 wafer) 기준
                                dlg.BindTargetWafer(wafer);

                                // Download map 파일 경로 전달
                                if (!string.IsNullOrWhiteSpace(mapFile) && File.Exists(mapFile))
                                {
                                    dlg.SetDownloadedMapFile(mapFile);
                                }

                                // Camera 바인딩
                                dlg.BindEquipmentInStageCamera();

                                // 강제로 활성화 (포커스)
                                dlg.Activate();

                                // Owner(mainForm)를 지정하여 모달로 띄움 -> 뒤로 숨지 않음
                                dr = dlg.ShowDialog(mainForm);
                            }
                            catch (Exception ex)
                            {
                                Log.Write(UnitName, "MapMatch", "Manual dialog exception: " + ex.Message);
                                dr = DialogResult.Abort; // 예외 시 중단 처리
                            }
                        }
                    });
                }
                else
                {
                    // Fallback: 메인 폼을 못 찾거나 이미 UI 스레드인 경우 (기존 로직 + TopMost)
                    using (var dlg = new FormMapMatchManual())
                    {
                        dlg.TopMost = true;
                        dlg.StartPosition = FormStartPosition.CenterScreen;
                        dlg.BindTargetWafer(wafer);
                        if (!string.IsNullOrWhiteSpace(mapFile) && File.Exists(mapFile))
                            dlg.SetDownloadedMapFile(mapFile);
                        dlg.BindEquipmentInStageCamera();
                        dlg.Activate();
                        dr = dlg.ShowDialog();
                    }
                }

                // 1) 사용자가 창을 그냥 닫았거나 Cancel이면 중단
                if (dr != DialogResult.OK)
                {
                    PostAlarm((int)AlarmKeys.eInputStageAlignNotCompleted);
                    Log.Write(UnitName, "MapMatch",
                        $"User cancelled manual rematch. Score={bestScore:F2} < Threshold={scoreThreshold:F2}. Sequence aborted.");
                    return -1;
                }

                // Apply는 FormMapMatchManual 내부에서 _targetWafer(=wafer.Dies)에 직접 반영됨.
                EventUpdateUIWafer?.BeginInvoke(wafer, null, null);

                retryScore = ComputeInternalMatchScorePercent(wafer);

                Log.Write(UnitName, "MapMatch",
                    $"Rematch after manual. Score={retryScore:F2} Threshold={scoreThreshold:F2}");

                bestScore = retryScore;

                return 0;
            }
        }

        //private int DecideWithManualRematchLoop(MaterialWafer wafer, string mapFile, ref double bestScore, double scoreThreshold)
        //{
        //    while (true)
        //    {
        //        if(Config.IsSimulation == false)
        //        {
        //            if (bestScore >= scoreThreshold)
        //                return 0;
        //        }

        //        using (var dlg = new FormMapMatchManual())
        //        {
        //            try
        //            {
        //                // Scan = 항상 장비 웨이퍼(현재 wafer) 기준
        //                dlg.BindTargetWafer(wafer);

        //                // Download map 파일 경로 전달(있으면 자동 로드에 사용)
        //                if (!string.IsNullOrWhiteSpace(mapFile) && File.Exists(mapFile))
        //                    dlg.SetDownloadedMapFile(mapFile);

        //                // Camera 바인딩(필요 시)
        //                dlg.BindEquipmentInStageCamera();
        //                var dr = dlg.ShowDialog();

        //                // 1) 사용자가 창을 그냥 닫았거나 Cancel이면 중단
        //                //if (dr != DialogResult.OK || !manualApplied)
        //                // OK 버튼으로 닫힌 경우에만 확정 진행
        //                if (dr != DialogResult.OK)
        //                {
        //                    PostAlarm((int)AlarmKeys.eInputStageAlignNotCompleted);
        //                    Log.Write(UnitName, "MapMatch",
        //                        $"User cancelled manual rematch. Score={bestScore:F2} < Threshold={scoreThreshold:F2}. Sequence aborted.");
        //                    return -1;
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                PostAlarm((int)AlarmKeys.eInputStageAlignNotCompleted);
        //                Log.Write(UnitName, "MapMatch", "Manual dialog exception: " + ex.Message);
        //                return -1;
        //            }
        //        }

        //        // Apply는 FormMapMatchManual 내부에서 _targetWafer(=wafer.Dies)에 직접 반영됨.
        //        EventUpdateUIWafer?.BeginInvoke(wafer, null, null);

        //        // Apply 후 재매칭
        //        //if (!TryRematchAfterManual(wafer, mapFile, out var retryScore))
        //        //{
        //        //    PostAlarm((int)AlarmKeys.eInputStageAlignNotCompleted);
        //        //    Log.Write(UnitName, "MapMatch", "Rematch failed after manual -> abort");
        //        //    return -1;
        //        //}
        //        double retryScore = ComputeInternalMatchScorePercent(wafer);


        //        Log.Write(UnitName, "MapMatch",
        //            $"Rematch after manual. Score={retryScore:F2} Threshold={scoreThreshold:F2}");

        //        bestScore = retryScore;

        //        // User가 확인 후 진행하였으므로 진행시킴.
        //        //if (bestScore >= scoreThreshold)
        //        //    return 0;

        //        return 0;

        //    }
        //}


        private int DecideWithManualRematchLoop_NoMapFile(MaterialWafer wafer, ref double bestScore, double scoreThreshold)
        {
            while (true)
            {
                bestScore = ComputeInternalMatchScorePercent(wafer);

                Log.Write(UnitName, "MapMatch",
                    $"Manual applied (NoMapFile). InternalScore={bestScore:F2} Threshold={scoreThreshold:F2}");

                var equipment = Equipment.Instance;
                bool IsDryRunEqp = equipment.EquipmentConfig.IsDryRun;
                if (Config.IsSimulation || (Config.IsDryRun || IsDryRunEqp))
                {
                    if (bestScore >= scoreThreshold)
                    {
                        return 0;
                    }
                }

                DialogResult dr = DialogResult.None;
                // [수정] UI 스레드에서 폼 생성 및 호출
                var mainForm = System.Windows.Forms.Application.OpenForms.Count > 0 ? System.Windows.Forms.Application.OpenForms[0] : null;
                if (mainForm != null && mainForm.InvokeRequired)
                {
                    mainForm.Invoke((MethodInvoker)delegate
                    {
                        using (var dlg = new FormMapMatchManual())
                        {
                            try
                            {
                                dlg.TopMost = true; // 최상위
                                dlg.StartPosition = FormStartPosition.CenterScreen;

                                dlg.BindTargetWafer(wafer);

                                // 다운로드 파일이 없으니, wafer.Dies를 "다운로드 맵"으로 주입
                                dlg.SetDownloadedMapFromWaferDies(wafer, infoText: "NO_MAPFILE_USE_WAFER_DIES");

                                dlg.BindEquipmentInStageCamera();

                                dlg.ManualMatchApplied += (s, e) =>
                                {
                                    // 이벤트 핸들러 로직 (필요시)
                                };

                                dlg.Activate(); // 활성화
                                dr = dlg.ShowDialog(mainForm); // Owner 지정
                            }
                            catch (Exception ex)
                            {
                                Log.Write(UnitName, "MapMatch", "Manual dialog exception (NoMapFile): " + ex.Message);
                                dr = DialogResult.Abort;
                            }
                        }
                    });
                }
                else
                {
                    // Fallback
                    using (var dlg = new FormMapMatchManual())
                    {
                        dlg.TopMost = true;
                        dlg.BindTargetWafer(wafer);
                        dlg.SetDownloadedMapFromWaferDies(wafer, infoText: "NO_MAPFILE_USE_WAFER_DIES");
                        dlg.BindEquipmentInStageCamera();
                        dlg.Activate();
                        dr = dlg.ShowDialog();
                    }
                }

                if (dr != DialogResult.OK)
                {
                    PostAlarm((int)AlarmKeys.eInputStageAlignNotCompleted);
                    Log.Write(UnitName, "MapMatch", "User cancelled manual map match. (NoMapFile)");
                    return -1;
                }

                EventUpdateUIWafer?.BeginInvoke(wafer, null, null);

                bestScore = ComputeInternalMatchScorePercent(wafer);

                Log.Write(UnitName, "MapMatch",
                    $"Manual applied (NoMapFile). InternalScore={bestScore:F2} Threshold={scoreThreshold:F2}");

                return 0;
            }
        }
        //private int DecideWithManualRematchLoop_NoMapFile(MaterialWafer wafer, ref double bestScore, double scoreThreshold)
        //{
        //    // mapFile이 없더라도 FormMapMatchManual에서 "wafer.Dies를 원본맵(download)"으로 넣어서
        //    // Apply 버튼만으로 진행(OK)할 수 있게 만든다.
        //    while (true)
        //    {
        //        bool manualApplied = false;

        //        using (var dlg = new FormMapMatchManual())
        //        {
        //            try
        //            {
        //                dlg.BindTargetWafer(wafer);

        //                // [ADD] 다운로드 파일이 없으니, wafer.Dies를 "다운로드 맵"으로 주입
        //                // 이렇게 하면 ApplyManualMatch()가 EnsureDownloadedMapLoaded()에서 막히지 않음.
        //                dlg.SetDownloadedMapFromWaferDies(wafer, infoText: "NO_MAPFILE_USE_WAFER_DIES");

        //                dlg.BindEquipmentInStageCamera();

        //                dlg.ManualMatchApplied += (s, e) =>
        //                {
        //                    //manualApplied = true;
        //                    //try { dlg.DialogResult = DialogResult.OK; } catch { }
        //                    //try { dlg.Close(); } catch { }
        //                };

        //                var dr = dlg.ShowDialog();

        //                if (dr != DialogResult.OK )//|| !manualApplied)
        //                {
        //                    PostAlarm((int)AlarmKeys.eInputStageAlignNotCompleted);
        //                    Log.Write(UnitName, "MapMatch", "User cancelled manual map match. (NoMapFile)");
        //                    return -1;
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                PostAlarm((int)AlarmKeys.eInputStageAlignNotCompleted);
        //                Log.Write(UnitName, "MapMatch", "Manual dialog exception (NoMapFile): " + ex.Message);
        //                return -1;
        //            }
        //        }

        //        EventUpdateUIWafer?.BeginInvoke(wafer, null, null);

        //        // mapFile 없으므로 내부 점수(중복/유니크)로만 참고
        //        bestScore = ComputeInternalMatchScorePercent(wafer);

        //        Log.Write(UnitName, "MapMatch",
        //            $"Manual applied (NoMapFile). InternalScore={bestScore:F2} Threshold={scoreThreshold:F2}");

        //        return 0;
        //    }
        //}

        private int PrepareEjectorForMappingOrAlarm()
        {
            int nRet = 0;

            nRet = InputStageEjector.MovePositionEjectPinOffset();
            if (nRet != 0)
            {
                AxisX.EmgStop();
                AxisY.EmgStop();
                AxisT.EmgStop();
                PostAlarm((int)AlarmKeys.eInputStageEjectorZNotSafety);
                Log.Write(UnitName, "PerformChipMapping", "Fail: MovePositionEjectPinOffset");
                return -1;
            }

            SetVacuum(false);

            nRet = InputStageEjector.MovePositionEjectBlockUp();
            if (nRet != 0)
            {
                AxisX.EmgStop();
                AxisY.EmgStop();
                AxisT.EmgStop();
                PostAlarm((int)AlarmKeys.eInputStageEjectorZNotSafety);
                Log.Write(UnitName, "PerformChipMapping", "Fail: MovePositionEjectBlockUp");
                return -1;
            }

            return 0;
        }
        
        private string PrepareLocalMapFileOrAlarm(MaterialWafer wafer, string mapFilePath)
        {
            try
            {
                if (wafer == null || string.IsNullOrWhiteSpace(wafer.WaferId))
                {
                    Log.Write(UnitName, "MapMatch", "Wafer or WaferId empty -> cannot resolve map file");
                    PostAlarm((int)AlarmKeys.eInputStageMapMatch);
                    return null;
                }

                string waferId = wafer.WaferId.Trim();
                string sourceWaf = ResolveSourceWafPath(mapFilePath, waferId);

                // [User Requirement] If sourceWaf is found (or logic dictates), let user select MASKMAP
                //if (string.IsNullOrWhiteSpace(sourceWaf) == false)
                if (string.IsNullOrWhiteSpace(sourceWaf))
                {
                    // Assuming this runs on a non-UI thread, we need to invoke on the main UI thread
                    var mainForm = System.Windows.Forms.Application.OpenForms.Count > 0
                        ? System.Windows.Forms.Application.OpenForms[0]
                        : null;

                    if (mainForm != null && mainForm.InvokeRequired)
                    {
                        string selectedFile = null;
                        mainForm.Invoke((System.Windows.Forms.MethodInvoker)delegate
                        {
                            using (var dlg = new System.Windows.Forms.OpenFileDialog())
                            {
                                dlg.Title = "Select MASKMAP File";
                                dlg.Filter = "Map Files (*.waf;*.txt;*.csv)|*.waf;*.txt;*.csv|All files (*.*)|*.*";
                                dlg.FileName = System.IO.Path.GetFileName(sourceWaf); // Propose the found file name
                                if (!string.IsNullOrWhiteSpace(sourceWaf))
                                {
                                    try
                                    {
                                        dlg.InitialDirectory = System.IO.Path.GetDirectoryName(sourceWaf);
                                    }
                                    catch { /* ignore invalid paths */ }
                                }

                                if (dlg.ShowDialog(mainForm) == System.Windows.Forms.DialogResult.OK)
                                {
                                    selectedFile = dlg.FileName;
                                }
                            }
                        });

                        // If user selected a file, update sourceWaf
                        if (!string.IsNullOrWhiteSpace(selectedFile))
                        {
                            sourceWaf = selectedFile;
                            Log.Write(UnitName, "MapMatch", $"User selected MASKMAP: {sourceWaf}");
                        }
                    }
                }

                string localWaf;
                string dlReason;
                int dl = DownloadWafToLocalMapFolder(sourceWaf, waferId, out localWaf, out dlReason);
                if (dl != 0 || string.IsNullOrWhiteSpace(localWaf) || !File.Exists(localWaf))
                {
                    AxisX?.EmgStop(); AxisY?.EmgStop(); AxisT?.EmgStop();
                    PostAlarm((int)AlarmKeys.eInputStageMapMatch);
                    Log.Write(UnitName, "MapMatch", "Map file download failed: " + dlReason);
                    return null;
                }

                string strMapFile = localWaf.Trim();
                if (!System.IO.Path.IsPathRooted(strMapFile))
                {
                    var baseDir = AppDomain.CurrentDomain.BaseDirectory;
                    strMapFile = System.IO.Path.Combine(baseDir, strMapFile);
                }

                if (!File.Exists(strMapFile))
                {
                    Log.Write(UnitName, "MapMatch", $"Map file invalid or not found: '{strMapFile}'.");
                    PostAlarm((int)AlarmKeys.eInputStageMapMatch);
                    return null;
                }

                Log.Write(UnitName, "MapMatch", $"Map file ready. Source='{sourceWaf}' -> Local='{strMapFile}'");
                return strMapFile;
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                PostAlarm((int)AlarmKeys.eInputStageMapMatch);
                return null;
            }
        }
        private void TrySummaryStartScan()
        {
            try
            {
                var ctx = Equipment.Instance.SummaryContext;
                ctx.GetCurrentSummaryOrNull()?.StartScan();
            }
            catch (Exception ex) { Log.Write(ex); }
        }
        private void TrySummaryStopScan()
        {
            try
            {
                var ctx = Equipment.Instance.SummaryContext;
                ctx.GetCurrentSummaryOrNull()?.StopScan();
            }
            catch (Exception ex) { Log.Write(ex); }
        }
        private void TrySummaryStartSort()
        {
            try
            {
                var ctx = Equipment.Instance.SummaryContext;
                ctx.GetCurrentSummaryOrNull()?.StartSort();
            }
            catch (Exception ex) { Log.Write(ex); }
        }
        private void TrySummaryStopSort()
        {
            try
            {
                var ctx = Equipment.Instance.SummaryContext;
                ctx.GetCurrentSummaryOrNull()?.StopSort();
            }
            catch (Exception ex) { Log.Write(ex); }
        }
        private void TrySummaryUpdateScanAndTotalCount(int nCount)
        {
            try
            {
                var ctx = Equipment.Instance?.SummaryContext;
                var sum = ctx?.GetCurrentSummaryOrNull();

                if (ctx != null && ctx.IsActive && sum != null)
                {
                    int scanCount = nCount; // consolidated != null ? consolidated.Count : 0;
                    sum.SetScanCount(scanCount);
                    sum.SetTotalCount(scanCount);
                }
                else
                {
                    Log.Write(UnitName, "ChipMap", "[Summary] Skip count update (SummaryContext inactive or Current null).");
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
        }

        private void TrySummaryUpdateTotalCount(int TotalCount)
        {
            try
            {
                var ctx = Equipment.Instance?.SummaryContext;
                var sum = ctx?.GetCurrentSummaryOrNull();
                if (ctx != null && ctx.IsActive && sum != null)
                {
                    if (TotalCount < 0) 
                        TotalCount = 0;

                    sum.SetTotalCount(TotalCount);
                }
                else
                {
                    Log.Write(UnitName, "MapMatch", "[Summary] Skip count update (SummaryContext inactive or Current null).");
                }

                // [핵심] ResultWriterManager에 전체 개수 설정
                Equipment.Instance.ResultWriterManager.SetWaferTotalCount(TotalCount);
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
        }

        private void TrySummaryUpdateOrgAndScanCount(int orgCount, int scanCount)
        {
            try
            {
                var ctx = Equipment.Instance?.SummaryContext;
                var sum = ctx?.GetCurrentSummaryOrNull();
                if (ctx != null && ctx.IsActive && sum != null)
                {
                    if (orgCount < 0) orgCount = 0;
                    if (scanCount < 0) scanCount = 0;

                    //sum.AddTotalCount(orgCount);
                    //sum.AddScanCount(scanCount);
                    sum.SetTotalCount(orgCount);
                    sum.SetScanCount(scanCount);
                }
                else
                {
                    Log.Write(UnitName, "MapMatch", "[Summary] Skip count update (SummaryContext inactive or Current null).");
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
        }

        public bool TryGetNextPickupPosition(out double x, out double y, out int chipIndex)
        {
            x = y = 0;
            chipIndex = -1;
            if (!ChipMappingDone || CurrentChipMap == null) return false;

            var seq = CurrentChipMap.EnumeratePickup().ToList();
            if (_chipPickupCursor >= seq.Count) return false;

            var entry = seq[_chipPickupCursor];
            chipIndex = entry.Index;
            x = entry.Xmm;
            y = entry.Ymm;
            return true;
        }
        public int MoveToNextChipForPickup()
        {
            if (!TryGetNextPickupPosition(out var x, out var y, out var idx))
                return 1; // 완료

            if (AxisX != null && MoveAxisPositionOne(AxisX, x) != 0) return -1;
            if (AxisY != null && MoveAxisPositionOne(AxisY, y) != 0) return -1;
            if (WaitUntil(() =>
                AxisX.InPosition(x) && AxisY.InPosition(y),
                MappingMoveTimeoutMs) != 0)
                return -1;

            _chipPickupCursor++;
            return 0;
        }
        public bool IsAllChipPickupDone()
        {
            if (!ChipMappingDone || CurrentChipMap == null) return false;
            return _chipPickupCursor >= CurrentChipMap.EnumeratePickup().Count();
        }
        // 외부(InputDieTransfer) 요청 처리 예시
        public int OnPickupRequestFromDieTransfer()
        {
            if (!ChipMappingDone) return -1;
            if (IsAllChipPickupDone()) return 1;
            return MoveToNextChipForPickup();
        }
        // [ADD] Simulation에서 Rank=1(OK)로 설정될 확률 (0.0~1.0)
        // 예: 0.7 이면 약 70%가 Rank=1, 나머지는 Rank!=1로 NG 처리됨
        public double SimOkRankProbability { get; set; } = 0.7;
        // [ADD] 랜덤 시드 고정(테스트 재현성) 원하면 값 변경/고정 가능
        private readonly Random _simRankRand = new Random();
        private void ApplyOkRankToDies(MaterialWafer wafer, int okRank = 1)
        {
            if (wafer?.Dies == null || wafer.Dies.Count == 0)
                return;

            try
            {
                lock (wafer.Dies)
                {
                    var equipment = Equipment.Instance;
                    bool IsDryRunEqp = equipment.EquipmentConfig.IsDryRun;
                    if (Config.IsSimulation || (Config.IsDryRun || IsDryRunEqp))
                    {
                        SimOkRankProbability = 1;
                        double p = SimOkRankProbability;
                        if (p < 0.0) p = 0.0;
                        if (p > 1.0) p = 1.0;

                        foreach (var die in wafer.Dies)
                        {
                            if (die == null)
                                continue;

                            bool makeOk;
                            lock (_simRankRand)
                            {
                                makeOk = _simRankRand.NextDouble() < p;
                            }

                            if (makeOk)
                                die.Rank = okRank;
                        }

                        Log.Write(UnitName, nameof(ApplyOkRankToDies),
                            $"[SIM] Applied random Rank={okRank}. p={p:0.###} dies={wafer.Dies.Count}");

                        return;
                    }

                    foreach (var die in wafer.Dies)
                    {
                        if (die == null)
                            continue;

                        // OK 판정(현 프로젝트에서 가장 보편적인 기준):
                        // - IsPass == true
                        // - Rejected 상태 제외
                        bool isOk = die.IsPass && die.State != DieProcessState.Rejected;
                        if (!isOk)
                            continue;

                        die.Rank = okRank;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Write(UnitName, nameof(ApplyOkRankToDies), ex.Message);
            }
        }
        private void MarkNonRank1DiesAsSkip(MaterialWafer wafer, int okRank = 1)
        {
            if (wafer?.Dies == null || wafer.Dies.Count == 0)
                return;

            try
            {
                lock (wafer.Dies)
                {
                    foreach (var d in wafer.Dies)
                    {
                        if (d == null)
                            continue;

                        // 대상: 존재하는 die면서 아직 픽업 대상 후보(Mapped)인데 Rank!=1
                        if (d.Presence != MaterialPresence.Exist)
                            continue;

                        if (d.State != DieProcessState.Mapped)
                            continue;

                        if (d.Rank == okRank)
                            continue;

                        //d.SetReject("MapMapping - Rank!=1");
                        d.SetSkip("MapMapping - Rank!=1");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Write(UnitName, nameof(MarkNonRank1DiesAsSkip), ex.Message);
            }
        }

        #endregion


        #region MapMatch / Path Ordering
        private bool _pathBaseLocked;
        private MapPathStartCorner _pathBaseCorner = MapPathStartCorner.BottomLeft;
        private MapPathPrimaryAxis _pathBaseAxis = MapPathPrimaryAxis.XFirst;

        //장비 오른쪽 하단이 원점.
        private void ApplyDieOrderByPathSettings(MaterialWafer wafer)
        {
            try
            {
                if (wafer?.Dies == null || wafer.Dies.Count == 0)
                    return;

                lock (wafer.Dies)
                {
                    // 0. 유효한 다이만 필터링
                    var validDies = wafer.Dies.Where(d => d != null).ToList();
                    if (validDies.Count == 0) 
                        return;

                    var recipe = Equipment.Instance.EquipmentRecipe.CurrentRecipe;

                    // 회전/반전 설정은 무시하고, 시작 모서리와 주축 설정만 사용합니다.
                    _pathBaseCorner = recipe.WaferPathStartCorner;
                    _pathBaseAxis = recipe.WaferPathPrimaryAxis;
                    var traversal = recipe.WaferPathTraversalMode;

                    // 1. 정렬 방향 결정 (1: 오름차순(Asc), -1: 내림차순(Desc))
                    // 장비 좌표계 기준: 일반적으로 Left=Small X, Right=Large X, Bottom=Small Y, Top=Large Y
                    // 사용자의 장비 특성에 맞춰 X축 반전이 필요하다면 sortX 부호를 반대로 설정하면 됩니다.
                    int sortX = 1;
                    int sortY = 1;

                    switch (_pathBaseCorner)
                    {
                        case MapPathStartCorner.BottomLeft:
                            // 시작: 왼쪽 아래 -> X증가, Y증가 방향 진행
                            sortX = 1; sortY = 1;
                            break;
                        case MapPathStartCorner.BottomRight:
                            // 시작: 오른쪽 아래 -> X감소, Y증가 방향 진행
                            sortX = -1; sortY = 1;
                            break;
                        case MapPathStartCorner.TopLeft:
                            // 시작: 왼쪽 위 -> X증가, Y감소 방향 진행
                            sortX = 1; sortY = -1;
                            break;
                        case MapPathStartCorner.TopRight:
                            // 시작: 오른쪽 위 -> X감소, Y감소 방향 진행
                            sortX = -1; sortY = -1;
                            break;
                    }

                    // [중요] 사용자의 장비가 물리적으로 X축이 반대라면 아래 주석을 해제하여 전체 반전
                    sortX *= -1; // LCP-280 X축 반대.

                    List<MaterialDie> ordered = new List<MaterialDie>();

                    // 2. 그룹핑 및 정렬 (좌표계 변환 없이 물리 좌표 MapX, MapY 그대로 사용)
                    if (_pathBaseAxis == MapPathPrimaryAxis.XFirst)
                    {
                        // [X First] : 가로(X) 방향으로 먼저 이동 -> Y(Row)를 기준으로 묶어야 함

                        // 2-1. Row(Y) 정렬
                        var rows = (sortY > 0)
                            ? validDies.GroupBy(d => (int)d.MapY).OrderBy(g => g.Key)       // Y 오름차순 (Bottom -> Top)
                            : validDies.GroupBy(d => (int)d.MapY).OrderByDescending(g => g.Key); // Y 내림차순 (Top -> Bottom)

                        int rowIdx = 0;
                        foreach (var row in rows)
                        {
                            var list = row.ToList();

                            // 2-2. Row 내부의 Col(X) 정렬
                            bool isAscendingX = (sortX > 0);

                            // Zigzag(Serpentine) 처리: 홀수 번째 줄은 방향 반전
                            if (traversal == MapPathTraversalMode.Serpentine && rowIdx % 2 == 1)
                            {
                                isAscendingX = !isAscendingX;
                            }

                            if (isAscendingX)
                                list.Sort((a, b) => a.MapX.CompareTo(b.MapX)); // X 오름차순
                            else
                                list.Sort((a, b) => b.MapX.CompareTo(a.MapX)); // X 내림차순

                            ordered.AddRange(list);
                            rowIdx++;
                        }
                    }
                    else // MapPathPrimaryAxis.YFirst
                    {
                        // [Y First] : 세로(Y) 방향으로 먼저 이동 -> X(Col)를 기준으로 묶어야 함

                        // 2-1. Col(X) 정렬
                        var cols = (sortX > 0)
                            ? validDies.GroupBy(d => (int)d.MapX).OrderBy(g => g.Key)       // X 오름차순 (Left -> Right)
                            : validDies.GroupBy(d => (int)d.MapX).OrderByDescending(g => g.Key); // X 내림차순 (Right -> Left)

                        int colIdx = 0;
                        foreach (var col in cols)
                        {
                            var list = col.ToList();

                            // 2-2. Col 내부의 Row(Y) 정렬
                            bool isAscendingY = (sortY > 0);

                            // Zigzag(Serpentine) 처리: 홀수 번째 줄은 방향 반전
                            if (traversal == MapPathTraversalMode.Serpentine && colIdx % 2 == 1)
                            {
                                isAscendingY = !isAscendingY;
                            }

                            if (isAscendingY)
                                list.Sort((a, b) => a.MapY.CompareTo(b.MapY)); // Y 오름차순
                            else
                                list.Sort((a, b) => b.MapY.CompareTo(a.MapY)); // Y 내림차순

                            ordered.AddRange(list);
                            colIdx++;
                        }
                    }

                    // 3. 정렬된 순서대로 리스트 교체 및 인덱스 재부여
                    for (int i = 0; i < ordered.Count; i++)
                    {
                        ordered[i].Index = i;
                        if (!string.IsNullOrEmpty(wafer.WaferId))
                            ordered[i].Name = $"{wafer.WaferId}_{i}";
                    }

                    wafer.Dies.Clear();
                    wafer.Dies.AddRange(ordered);
                }
            }
            catch (Exception ex)
            {
                Log.Write(UnitName, "ApplyDieOrderByPathSettings", ex.Message);
            }
        }


        // 기존 코드
        //private void ApplyDieOrderByPathSettings(MaterialWafer wafer)
        //{
        //    try
        //    {
        //        if (wafer?.Dies == null || wafer.Dies.Count == 0)
        //            return;

        //        LockPathBaseFromRecipeOnce();

        //        lock (wafer.Dies)
        //        {
        //            // 0. 준비: 원본 좌표 범위 파악
        //            var validDies = wafer.Dies.Where(d => d != null).ToList();
        //            if (validDies.Count == 0) return;

        //            var xs = validDies.Select(d => (int)d.MapX).Distinct().OrderBy(v => v).ToList();
        //            var ys = validDies.Select(d => (int)d.MapY).Distinct().OrderBy(v => v).ToList();

        //            int nx = xs.Count;
        //            int ny = ys.Count;

        //            var recipe = Equipment.Instance.EquipmentRecipe.CurrentRecipe;
        //            var mapRotate = recipe.WaferRotate;
        //            var mapMirror = recipe.WaferMirror;
        //            _pathBaseCorner = recipe.WaferPathStartCorner;
        //            _pathBaseAxis = recipe.WaferPathPrimaryAxis;

        //            // 1. 변환 함수 (물리 좌표 -> 화면/논리 좌표)
        //            (int sx, int sy) GetScreenPos(int _ix, int _iy)
        //            {
        //                // Rotate
        //                int rx = _ix, ry = _iy;
        //                switch (mapRotate)
        //                {
        //                    case MapRotateOption.CW90: rx = ny - 1 - _iy; ry = _ix; break;
        //                    case MapRotateOption.CW180: rx = nx - 1 - _ix; ry = ny - 1 - _iy; break;
        //                    case MapRotateOption.CW270: rx = _iy; ry = nx - 1 - _ix; break;
        //                }

        //                // Mirror (회전 후의 좌표 공간 기준)
        //                int dimX = (mapRotate == MapRotateOption.CW90 || mapRotate == MapRotateOption.CW270) ? ny : nx;
        //                int dimY = (mapRotate == MapRotateOption.CW90 || mapRotate == MapRotateOption.CW270) ? nx : ny;

        //                switch (mapMirror)
        //                {
        //                    case MapMirrorOption.X: return (dimX - 1 - rx, ry);
        //                    case MapMirrorOption.Y: return (rx, dimY - 1 - ry);
        //                    case MapMirrorOption.XY: return (dimX - 1 - rx, dimY - 1 - ry);
        //                    default: return (rx, ry);
        //                }
        //            }

        //            // 2. 모든 다이에 논리 좌표(ScreenX, ScreenY) 부여
        //            var dieInfos = validDies.Select(d =>
        //            {
        //                int mx = (int)d.MapX;
        //                int my = (int)d.MapY;
        //                int ix = xs.IndexOf(mx);
        //                int iy = ys.IndexOf(my);
        //                var pos = GetScreenPos(ix, iy);
        //                return new { Die = d, Sx = pos.sx, Sy = pos.sy };
        //            }).ToList();

        //            // 3. 정렬 기준 설정 [수정됨]
        //            // X축 방향이 반대라는 피드백 반영: Left/Right 로직을 반전시킴
        //            // 기존: Left -> sortX = 1 (오름차순), Right -> sortX = -1 (내림차순)
        //            // 변경: 현상이 반대라면, 아래와 같이 부호를 뒤집거나 비교 구문을 바꿔야 함.
        //            //       여기서는 sortX의 부호를 반전시킵니다.

        //            // 일반적인 좌표계: Left(Small X), Right(Large X), Bottom(Small Y), Top(Large Y)
        //            // 사용자의 장비 현상: "TopLeft 설정 시 TopRight 시작" -> X 정렬이 반대임.
        //            // 따라서 Left일 때 X-Desc(큰거부터), Right일 때 X-Asc(작은거부터)로 동작하고 있었을 가능성이 높음.
        //            // 아래 코드는 "Left면 무조건 오름차순(작은 X -> 큰 X)", "Right면 무조건 내림차순"이 되도록 재검토함.

        //            int sortX = 1; // 1: Asc(Left->Right), -1: Desc(Right->Left)
        //            int sortY = 1; // 1: Asc(Bot->Top), -1: Desc(Top->Bot)

        //            switch (_pathBaseCorner)
        //            {
        //                // [수정 포인트] 
        //                // X축 동작이 전체적으로 반대라면, Left일 때와 Right일 때의 sortX 부호를 반대로 설정해줍니다.
        //                // 다만, 코드 가독성을 위해 switch문 값은 정석대로 두고, 아래 Sort 로직에서 부호의 의미를 명확히 합니다.

        //                // BottomLeft:  X 오름차순(1), Y 오름차순(1)
        //                // BottomRight: X 내림차순(-1), Y 오름차순(1)
        //                // TopLeft:     X 오름차순(1), Y 내림차순(-1)
        //                // TopRight:    X 내림차순(-1), Y 내림차순(-1)

        //                // *중요*: 사용자가 "TopLeft인데 TopRight(X가 큰 쪽)부터 시작했다"고 함.
        //                // 즉, Left(오름차순)를 의도했으나 내림차순으로 동작했음.
        //                // 아래 로직에서 sortX가 1일 때 오름차순이 맞는지 확인 필요.

        //                case MapPathStartCorner.BottomLeft: sortX = 1; sortY = 1; break;
        //                case MapPathStartCorner.BottomRight: sortX = -1; sortY = 1; break;
        //                case MapPathStartCorner.TopLeft: sortX = 1; sortY = -1; break;
        //                case MapPathStartCorner.TopRight: sortX = -1; sortY = -1; break;
        //            }

        //            // [X축 전체 반전 적용]
        //            // 사용자의 요구: "전체적으로 전부 X가 반대다"
        //            // -> Left/Right에 상관없이 X축 정렬 방향 자체를 뒤집습니다.
        //            sortX *= -1;

        //            List<MaterialDie> ordered = new List<MaterialDie>();
        //            var traversal = recipe.WaferPathTraversalMode;

        //            // 4. 주축/Serpentine 처리
        //            if (_pathBaseAxis == MapPathPrimaryAxis.XFirst)
        //            {
        //                // [X First] : 가로로 먼저 이동 (Row 단위 처리)
        //                // 주축이 X이므로, Y(Row)를 기준으로 먼저 그룹핑해야 함.

        //                // Y축 정렬 (Row 순서 결정)
        //                var rows = (sortY > 0)
        //                    ? dieInfos.GroupBy(x => x.Sy).OrderBy(g => g.Key)
        //                    : dieInfos.GroupBy(x => x.Sy).OrderByDescending(g => g.Key);

        //                int rowIdx = 0;
        //                foreach (var row in rows)
        //                {
        //                    var list = row.ToList();

        //                    // X축 정렬 (Row 내부 순서)
        //                    // sortX > 0 이면 오름차순(Asc), 아니면 내림차순(Desc)
        //                    bool asc = (sortX > 0);

        //                    // Zigzag(Serpentine): 홀수번째 Row는 진행 방향 반전
        //                    if (traversal == MapPathTraversalMode.Serpentine && rowIdx % 2 == 1)
        //                        asc = !asc;

        //                    if (asc) list.Sort((a, b) => a.Sx.CompareTo(b.Sx)); // 오름차순
        //                    else list.Sort((a, b) => b.Sx.CompareTo(a.Sx));     // 내림차순

        //                    ordered.AddRange(list.Select(x => x.Die));
        //                    rowIdx++;
        //                }
        //            }
        //            else // YFirst
        //            {
        //                // [Y First] : 세로로 먼저 이동 (Col 단위 처리)
        //                // 주축이 Y이므로, X(Col)를 기준으로 먼저 그룹핑해야 함.

        //                // X축 정렬 (Col 순서 결정)
        //                var cols = (sortX > 0)
        //                    ? dieInfos.GroupBy(x => x.Sx).OrderBy(g => g.Key)
        //                    : dieInfos.GroupBy(x => x.Sx).OrderByDescending(g => g.Key);

        //                int colIdx = 0;
        //                foreach (var col in cols)
        //                {
        //                    var list = col.ToList();

        //                    // Y축 정렬 (Col 내부 순서)
        //                    bool asc = (sortY > 0);

        //                    // Zigzag(Serpentine): 홀수번째 Col은 진행 방향 반전
        //                    if (traversal == MapPathTraversalMode.Serpentine && colIdx % 2 == 1)
        //                        asc = !asc;

        //                    if (asc) list.Sort((a, b) => a.Sy.CompareTo(b.Sy)); // 오름차순
        //                    else list.Sort((a, b) => b.Sy.CompareTo(a.Sy));     // 내림차순

        //                    ordered.AddRange(list.Select(x => x.Die));
        //                    colIdx++;
        //                }
        //            }

        //            // 5. 인덱스 재부여 및 리스트 반영
        //            for (int i = 0; i < ordered.Count; i++)
        //            {
        //                ordered[i].Index = i;
        //                if (!string.IsNullOrEmpty(wafer.WaferId))
        //                    ordered[i].Name = $"{wafer.WaferId}_{i}";
        //            }

        //            wafer.Dies.Clear();
        //            wafer.Dies.AddRange(ordered);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.Write(UnitName, "ApplyDieOrderByPathSettings", ex.Message);
        //    }
        //}

        private static string GetLocalMapFileDir()
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MapFile");
        }
        // [ADD] recipe.MapFilePath + waferId 로 소스 waf 파일 경로 찾기
        // - MapFilePath가 폴더면: {folder}\{waferId}.waf
        // - MapFilePath에 와일드카드(*,?)가 있으면: 첫 매칭 파일 사용
        // - MapFilePath가 파일이면: 그대로 사용 (단, 확장자/waferId 확인은 호출부에서)
        private static string ResolveSourceWafPath(string mapFilePath, string waferId)
        {
            if (string.IsNullOrWhiteSpace(mapFilePath))
                return null;

            mapFilePath = mapFilePath.Trim();

            string FileName = string.Empty;

            // 와일드카드 패턴 지원 (예: \\server\share\waf\*.waf)
            if (mapFilePath.IndexOfAny(new[] { '*', '?' }) >= 0)
            {
                string dir = Path.GetDirectoryName(mapFilePath);
                string pattern = Path.GetFileName(mapFilePath);

                if (string.IsNullOrWhiteSpace(dir) || !Directory.Exists(dir))
                    return null;

                // waferId가 들어간 걸 우선 매칭 (가능하면)
                var candidates = Directory.GetFiles(dir, pattern, SearchOption.TopDirectoryOnly);

                // 우선순위 1) 파일명(확장자 제외)이 waferId와 정확히 일치
                var exact = candidates.FirstOrDefault(f =>
                    string.Equals(Path.GetFileNameWithoutExtension(f), waferId, StringComparison.OrdinalIgnoreCase));
                if (!string.IsNullOrEmpty(exact))
                    return exact;

                // 우선순위 2) 파일명에 waferId 포함
                var contains = candidates.FirstOrDefault(f =>
                    Path.GetFileName(f).IndexOf(waferId, StringComparison.OrdinalIgnoreCase) >= 0);
                if (!string.IsNullOrEmpty(contains))
                    return contains;

                // 마지막) 그냥 첫 파일
                return candidates.FirstOrDefault();
            }

            string strMaskMap = "MASKMAP 4UM-D4848 SETI";
            // 폴더 경로이면 waferId.waf로 조합
            if (Directory.Exists(mapFilePath))
            {
                //D4848은 고정 파일.
                FileName = Path.Combine(mapFilePath, strMaskMap + ".txt");
                //return Path.Combine(mapFilePath, waferId + ".txt");
            }

            // 파일 경로이면 그대로
            //if (File.Exists(FileName))
            if (mapFilePath == FileName)
            {
                return null;
            }

            // 존재하지 않는 폴더/파일이면, "폴더로 가정"하고 조합 시도(UNC 지연 연결 대비)
            // 예: \\server\share\mapfiles (Directory.Exists가 false로 떨어질 수 있음)
            // -> 일단 waferId.waf 경로를 만들어 반환하고, 호출부에서 File.Exists로 최종 체크
            return FileName;

            //return Path.Combine(mapFilePath, strMaskMap + ".txt");
            //return Path.Combine(mapFilePath, waferId + ".txt");
        }

        // [ADD] 소스 -> 로컬 MapFile 폴더로 복사(다운로드) 후 로컬 경로 반환
        private static int DownloadWafToLocalMapFolder(string sourceWafPath, string waferId, out string localWafPath, out string reason)
        {
            localWafPath = null;
            reason = null;

            if (string.IsNullOrWhiteSpace(waferId))
            {
                reason = "WaferId empty";
                return -1;
            }

            if (string.IsNullOrWhiteSpace(sourceWafPath))
            {
                reason = "Source path empty";
                return -1;
            }

            if (!File.Exists(sourceWafPath))
            {
                reason = "Source waf not found: " + sourceWafPath;
                return -1;
            }

            string localDir = GetLocalMapFileDir();
            Directory.CreateDirectory(localDir);

            localWafPath = System.IO.Path.Combine(localDir, waferId + ".txt");

            // 동일 파일이면 스킵, 아니면 덮어쓰기
            try
            {
                File.Copy(sourceWafPath, localWafPath, overwrite: true);
                return 0;
            }
            catch (Exception ex)
            {
                reason = "Copy failed: " + ex.Message;
                return -1;
            }
        }
        // [ADD] Manual MapMatch 설정을 현재 wafer.Dies(MapX/MapY)에 적용
        private bool ApplyManualMapMatchToWafer(MaterialWafer wafer, FormMapMatchManual.ManualTransformSettings s)
        {
            if (wafer == null || wafer.Dies == null || wafer.Dies.Count == 0) return false;
            if (s == null) return false;

            try
            {
                lock (wafer.Dies)
                {
                    foreach (var d in wafer.Dies)
                    {
                        if (d == null)
                            continue;

                        // Map 좌표(정수 grid)를 변환 대상으로 사용
                        var src = new PointF((float)d.MapX, (float)d.MapY);
                        var dst = FormMapMatchManual.Transform(src, s);

                        // MapX/MapY는 프로젝트 내에서 int로 쓰는 흐름이 강함 (표시/Indexing/Mapmatch)
                        d.MapX = (int)Math.Round(dst.X);
                        d.MapY = (int)Math.Round(dst.Y);
                    }
                }

                // 인덱스/이름 재정렬(선택): 현 코드 스타일에 맞춰 정규화 호출
                NormalizeIndicesSequential(wafer, startIndex: 0, rename: true);
                return true;
            }
            catch (Exception ex)
            {
                Log.Write(UnitName, "ApplyManualMapMatchToWafer", $"Exception: {ex.Message}");
                return false;
            }
        }

        // [ADD] 현재 wafer.Dies 기준으로 Mapmatch를 재시도하고 score(%)를 반환
        private bool TryRematchAfterManual(MaterialWafer wafer, string mapFile, out double scorePercent)
        {
            scorePercent = 0.0;
            if (wafer == null) return false;
            if (string.IsNullOrWhiteSpace(mapFile) || !File.Exists(mapFile)) return false;

            try
            {
                double score = wafer.Mapmatch(mapFile, MaterialWafer.MapTyp.txt);
                //double score = wafer.Mapmatch(mapFile, MaterialWafer.MapTyp.waf);
                //double score = wafer.MapmatchFast(mapFile, MaterialWafer.MapTyp.waf);
                scorePercent = score * 100.0;
                return true;
            }
            catch (Exception ex)
            {
                Log.Write(UnitName, "TryRematchAfterManual", $"Exception: {ex.Message}");
                return false;
            }
        }

        #endregion


        #region Helpers / Utilities
        public MaterialWafer GetMaterialWafer()
        {
            var mat = GetMaterial();
            return mat as MaterialWafer;
        }
        public void UpdateUI()
        {
            MaterialWafer materialWafer = GetMaterialWafer();
            if (EventUpdateUIWafer != null)
            {
                // 기존의 무거운 BeginInvoke 대신 Task.Run을 사용하여 
                // 메인 시퀀스 스레드는 즉시 리턴하게 만듭니다.
                Task.Run(() =>
                {
                    try
                    {
                        EventUpdateUIWafer.Invoke(materialWafer);
                    }
                    catch (Exception ex)
                    {
                        Log.Write("UpdateUI", ex.Message);
                    }
                });
            }

           //MaterialWafer materialWafer = GetMaterialWafer();
           //EventUpdateUIWafer?.BeginInvoke(materialWafer, null, null);
        }

        public bool IsRingPresent()
        {
            bool bRtn = true;
            var equipment = Equipment.Instance;
            bool IsDryRunEqp = equipment.EquipmentConfig.IsDryRun;
            if (Config.IsSimulation || (Config.IsDryRun || IsDryRunEqp))
            {
                // 시뮬레이션: 실제 보유 머티리얼로 판단
                return this.GetMaterial() is MaterialWafer;
                //return true;
            }
            else if (!Ring0() || !Ring1())
            {
                //Log.Write(UnitName, "IsRingPresent", $"Ring not present (R0={Ring0()}, R1={Ring1()})");
                return false;
            }

            return bRtn;
        }
        public bool IsWorking()
        {
            bool bRet = false;
            try
            {
                var wafer = GetMaterialWafer();
                var equipment = Equipment.Instance;
                bool IsDryRunEqp = equipment.EquipmentConfig.IsDryRun;
                if (Config.IsSimulation == false && (Config.IsDryRun == false && IsDryRunEqp == false))
                {
                    if (IsRingPresent() == true)
                    {
                        if (wafer == null)
                        {
                            //알람 발생 해야함.
                            // 제품이 있는데 wafer 정보가 없으면 이상
                            //이건 다른곳에서 확인해야 하나? 이 함수에서는,,
                            Log.Write(UnitName, "IsWorkCompleted", "Wafer present but wafer info is null");
                            return false;
                        }
                    }
                    else
                    {
                        if (wafer == null)
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    if (wafer == null)
                    {
                        return false;
                    }
                }

                if (Config.IsSimulation == false && (Config.IsDryRun == false && IsDryRunEqp == false))
                {
                    if (IsRingPresent() == false ||
                        IsPositionWaferLoading() ||
                        IsPositionWaferUnloading())
                    {
                        return false;
                    }
                    else //제품이 있고 wafer상태가 Completed 가 아니면 작업중으로 간주
                    {
                        if (wafer.Presence == Material.MaterialPresence.Exist)
                        {
                            if (wafer.ProcessSatate != Material.MaterialProcessSatate.Completed)
                            {
                                bRet = true;
                            }
                        }
                    }
                }
                else
                {
                    if (wafer.Presence == Material.MaterialPresence.Exist)
                    {
                        if (wafer.ProcessSatate != Material.MaterialProcessSatate.Completed)
                        {
                            // 작업 중임.
                            bRet = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                bRet = false;
                Log.Write(ex);
            }
            return bRet;
        }
        /// <summary>
        /// 다음 픽업 가능한 다이 존재 여부 확인.
        /// - 웨이퍼가 존재하고 Completed가 아니어야 함
        /// - Dies 중 Presence == Exist && State == Mapped 가 하나 이상 있어야 함
        /// - 없으면 웨이퍼 상태를 Completed로 전환
        /// </summary>
        public bool HasNextDie()
        {
            try
            {
                var wafer = GetMaterialWafer();
                if (wafer == null)
                    return false;

                // 맵핑이 아직 안 됐으면 다음 다이 없음으로 취급 (안전 가드)
                if (ChipMappingDone == false)
                    return false;

                bool bRingPresent = false;
                var equipment = Equipment.Instance;
                bool IsDryRunEqp = equipment.EquipmentConfig.IsDryRun;
                if (Config.IsSimulation || (Config.IsDryRun || IsDryRunEqp))
                {
                    bRingPresent = false;
                }
                else
                {
                    bRingPresent = IsRingPresent();
                }

                lock (wafer)
                {
                    if (wafer.Presence != Material.MaterialPresence.Exist)
                        return false;

                    if (wafer.ProcessSatate == Material.MaterialProcessSatate.Completed)
                        return false;

                    var next = wafer.Dies?
                            .Where(d => d != null
                                        && d.Presence == Material.MaterialPresence.Exist
                                        && d.State == DieProcessState.Mapped
                                        && d.Rank == 1) // [ADD] Rank 1만 픽업 대상
                            .OrderBy(d => d.Index)
                            .FirstOrDefault();

                    if (next == null
                        && wafer.Presence == Material.MaterialPresence.Exist
                        && wafer.ProcessSatate == Material.MaterialProcessSatate.Processing)
                    {
                        wafer.ProcessSatate = Material.MaterialProcessSatate.Completed;
                        return false;
                    }
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
        public MaterialDie GetNextDie()
        {
            MaterialDie die = null;
            try
            {
                var wafer = GetMaterialWafer();
                if (wafer == null)
                    return null;

                lock (wafer)
                {
                    lock (wafer.Dies)
                    {
                        if (wafer.Presence == Material.MaterialPresence.Exist)
                        {
                            if (wafer.ProcessSatate != Material.MaterialProcessSatate.Completed)
                            {
                                if (wafer.ProcessSatate == Material.MaterialProcessSatate.Processing)
                                {
                                    var v = wafer.Dies.Where(t => t.Presence == Material.MaterialPresence.Exist
                                                      && t.State == DieProcessState.Mapped
                                                      && t.Rank == 1) // [ADD] Rank 1만
                                             .OrderBy(t => t.Index);

                                    if (v.Any())
                                    {
                                        die = v.FirstOrDefault();
                                    }
                                    else
                                    {
                                        wafer.ProcessSatate = Material.MaterialProcessSatate.Completed;
                                        return null;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                die = null;
                Log.Write(ex);
            }
            return die;
        }

        private int WaitUntil(Func<bool> cond, int timeoutMs, int? pollMs = null, int stableHoldMs = 1, CancellationToken ct = default(CancellationToken))
        {
            if (cond == null)
                return -1;

            int step = pollMs.HasValue ? Math.Max(1, pollMs.Value) : Math.Max(1, this.PollIntervalMs);
            var swTotal = Stopwatch.StartNew();
            int stable = 0;

            while (swTotal.ElapsedMilliseconds < timeoutMs)
            {
                if (ct.IsCancellationRequested)
                    return -2; // 취소

                bool ok = false;
                try { ok = cond(); } catch { ok = false; }

                if (ok)
                {
                    if (stableHoldMs <= 0)
                        return 0; // 즉시 성공

                    if (stable >= stableHoldMs)
                        return 0; // 안정 구간 확보 후 성공

                    Thread.Sleep(step);
                    stable += step;
                    continue;
                }

                // 조건 깨짐 → 안정시간 리셋
                stable = 0;
                Thread.Sleep(step);
            }

            // 타임아웃
            Log.Write(UnitName, $"WaitUntil timeout: {timeoutMs}ms (stableHoldMs={stableHoldMs}, pollMs={step})");
            return -1;
        }
        PointD GetPixelToMmScale(double dX, double dY)
        {
            double mmPerPixelX = (dX - StageCamera.CameraConfig.Resolution.Width / 2) * StageCamera.CameraConfig.Scale.X;
            double mmPerPixelY = (dY - StageCamera.CameraConfig.Resolution.Height / 2) * StageCamera.CameraConfig.Scale.Y;
            return new PointD(mmPerPixelX, mmPerPixelY);
        }
        public int SearchDies(VisionImage visionImage, ref List<PointD> points, double x, double y)
        {
            int ret = 0;

            this.PmRunner.SetSearchMode(PatternMatchingRunner.SearchMode.All);
            var result = this.PmRunner.Search(visionImage);

            if (result != null && result.Success && result.Matches != null && result.Matches.Count > 0)
            {
                int repIdx = 0;
                int trainW = 0, trainH = 0;
                try
                {
                    var ti = PmRunner.Parameters?.TrainImages?
                        .FirstOrDefault(t => t?.Header != null && t.Header.Width > 0 && t.Header.Height > 0);
                    if (ti != null)
                    {
                        trainW = ti.Header.Width;
                        trainH = ti.Header.Height;
                    }
                }
                catch { /* ignore */ }
                OnRawMatchesFound(visionImage, result.Matches, repIdx, trainW, trainH);
                StageCamera.SuspendedImageDisplay = false;
            }

            if (result.Success)
            {
                foreach (var v in result.Matches)
                {
                    lock (points)
                    {
                        PointD pt = GetPixelToMmScale(v.X, v.Y);
                        pt.X += x;
                        pt.Y += y;
                        points.Add(new PointD(pt.X, pt.Y));

                    }
                }
            }
            else
            {
                Log.Write(UnitName, "SearchDies", "SearchDies Fail.");
            }
            return ret;
        }


        private void UpdateChipInfo(List<PointD> chips)
        {
            try
            {
                MaterialWafer materialWafer = GetMaterialWafer();
                lock (materialWafer.Dies)
                {
                    materialWafer.Dies.Clear();

                    // 기존 MakeWaferInfo 대신 RegionGrowing 기반 맵 생성 사용
                    materialWafer.MakeWaferInfoRegionGrowing(
                        chips,
                        this.ChipPitchXmm,
                        this.ChipPitchYmm,
                        autoEstimatePitch: true);

                    int nIndex = 0;
                    var list = materialWafer.Dies.OrderBy(t => t.MapX).ThenBy(t => t.MapY);

                    TrySummaryUpdateScanAndTotalCount(materialWafer.Dies.Count);

                    //맵서치완료한 다이 정보 로그 출력을 다른 파일에 저장하도록 변경
                    DateTime now = DateTime.Now;
                    string logFile = string.Empty;
                    logFile = string.Format("DieMapLog_{0}", now.ToString("yyyyMMdd_HHmmss"));
                    Log.Write(logFile, "DieMap", "Index,MapX,MapY,CenterX ,CenterY");
                    foreach (var c in list)
                    {
                        Log.Write(logFile, "DieMap",
                            $"{nIndex} ,{c.MapX}, {c.MapY}, {c.CenterX:F3}, {c.CenterY:F3}");
                        nIndex++;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
            finally
            {
                MaterialWafer materialWafer = GetMaterialWafer();
                EventUpdateUIWafer?.BeginInvoke(materialWafer, null, null);
            }
        }

        private void ResetChipMappingState()
        {
            InputDieTransfer.taskPrepareNextDie = null;
            
            // 맵핑 상태/커서/결과 초기화
            ChipMappingDone = false;
            _chipPickupCursor = 0;
            CurrentChipMap = null;
            _simDiesGenerated = false; // 풀 재생성 트리거
            _simAllDiesPool.Clear();
            _simAddedKeys.Clear();
        }

        private void MakeScanPath(out List<PointD> path)
        {
            path = new List<PointD>();
            try
            {
                double centerTpX = GetTP(InputStageConfig.TeachingPositionName.CenterPoint.ToString(), AxisX.Name);
                double centerTpY = GetTP(InputStageConfig.TeachingPositionName.CenterPoint.ToString(), AxisY.Name);
                var eq = Equipment.Instance;
                var recip = eq.EquipmentRecipe.CurrentRecipe;
                double dRadius = recip.WaferDiameter / 2;
                bool IsDryRunEqp = eq.EquipmentConfig.IsDryRun;
                try
                {
                    if (Config.IsSimulation == false && (this.Config.IsDryRun == false && IsDryRunEqp == false))
                    {
                        if (PmRunner.IsRecipeLoaded == false)
                        {
                            PmRunner.LoadRecipe();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Write(ex);
                }

                // ROI(mm)
                double roiW = 0.0;
                double roiH = 0.0;
                var equipment = Equipment.Instance;
                if (Config.IsSimulation == false && (this.Config.IsDryRun == false && IsDryRunEqp == false))
                {
                    roiW = Math.Abs((PmRunner._Roi.InspectEnd.X - PmRunner._Roi.InspectStart.X) * StageCamera.CameraConfig.Scale.X);
                    roiH = Math.Abs((PmRunner._Roi.InspectEnd.Y - PmRunner._Roi.InspectStart.Y) * StageCamera.CameraConfig.Scale.Y);
                }
                else
                {
                    roiW = 0.85;
                    roiH = 0.7;
                }

                // Pitch(mm)
                double pitchX = ChipPitchXmm;
                double pitchY = ChipPitchYmm;
                if (pitchX <= 0) pitchX = 0.5;
                if (pitchY <= 0) pitchY = 0.5;

                // ===== [ADD] Over-scan 설정 =====
                // 0.7 -> 30% 겹침(권장 시작값)
                const double scanStepRatio = 0.70;

                // step은 ROI보다 작게 (겹치게)
                double stepX = roiW * scanStepRatio;
                double stepY = roiH * scanStepRatio;

                // 너무 촘촘하면 시간 폭증/중복 과다 -> 최소 스텝 clamp
                double minStepX = pitchX * 0.60;
                double minStepY = pitchY * 0.60;
                if (stepX < minStepX) stepX = minStepX;
                if (stepY < minStepY) stepY = minStepY;

                // ROI가 비정상일 때 방어
                if (stepX <= 0) stepX = pitchX;
                if (stepY <= 0) stepY = pitchY;

                // ROI half diagonal(경계 포함 여유)
                double offsetDist = GetDistance(roiW * 0.5, roiH * 0.5);

                int nHorzCount = (int)((dRadius * 2.0) / stepX) + 1;
                int nVertCount = (int)((dRadius * 2.0) / stepY) + 1;
                if (nHorzCount < 1) nHorzCount = 1;
                if (nVertCount < 1) nVertCount = 1;

                double startX = centerTpX - (nHorzCount - 1) * stepX / 2.0;
                double startY = centerTpY - (nVertCount - 1) * stepY / 2.0;

                bool useYScanFirst = true; // 기존 유지
                if (useYScanFirst)
                {
                    for (int ix = 0; ix < nHorzCount; ix++)
                    {
                        double x = startX + ix * stepX;
                        for (int iy = 0; iy < nVertCount; iy++)
                        {
                            double y = startY + iy * stepY;

                            // 지그재그(Y 방향 반전)
                            if (ix % 2 == 1)
                                y = startY + (nVertCount - 1 - iy) * stepY;

                            double dx = x - centerTpX;
                            double dy = y - centerTpY;
                            double dist = Math.Sqrt(dx * dx + dy * dy);
                            if (dist <= dRadius + offsetDist)
                                path.Add(new PointD(x, y));
                        }
                    }
                }
                else
                {
                    //x방향으로 서치
                    for (int iy = 0; iy < nVertCount; iy++)
                    {
                        double y = startY + iy * stepY;
                        bool reverse = (iy % 2 == 1);
                        for (int ix = 0; ix < nHorzCount; ix++)
                        {
                            int rx = reverse ? (nHorzCount - 1 - ix) : ix;
                            double x = startX + rx * stepX;

                            double dx = x - centerTpX;
                            double dy = y - centerTpY;
                            double dist = Math.Sqrt(dx * dx + dy * dy);
                            if (dist <= dRadius + offsetDist)
                                path.Add(new PointD(x, y));
                        }
                    }
                }

                Log.Write(UnitName, "MakeScanPath",
                    $"Count={path.Count} Radius={dRadius:F3} Center=({centerTpX:F3},{centerTpY:F3}) " +
                    $"ROI=({roiW:F3},{roiH:F3}) StepRatio={scanStepRatio:0.00} Step=({stepX:F3},{stepY:F3}) Pitch=({pitchX:F3},{pitchY:F3})");
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
        }

        /// <summary>
        /// Chip 중심 좌표 리스트에서 서로 가까운(mergeDist 이하) 점들을 하나의 대표 점(평균)으로 병합.
        ///  - O(N^2) 단순 방식 (좌표 수가 매우 많아지면 향후 Grid/Spatial Hash로 최적화 가능)
        ///  - 다중 스캔, ROI 겹침 등으로 인한 중복 제거
        /// </summary>
        /// <param name="raw">원본 좌표 목록</param>
        /// <param name="mergeDist">병합 기준 거리(mm)</param>
        /// <returns>병합된 좌표 목록</returns>
        private List<PointD> ConsolidateChipCenters(List<PointD> raw, double mergeDist)
        {
            if (raw == null || raw.Count == 0)
                return new List<PointD>();

            // 음수/0 보호
            if (mergeDist <= 0)
                return new List<PointD>(raw);

            double dist2 = mergeDist * mergeDist;

            // 순서 민감도 완화: 공간 정렬(X,Y) 후 처리
            var pts = raw.OrderBy(p => p.X).ThenBy(p => p.Y).ToList();
            // 누적 평균 관리를 위한 내부 구조
            // centers[i] : 현재 대표 좌표
            // sums[i]    : (sumX, sumY, count)
            var centers = new List<PointD>();
            var sums = new List<(double sumX, double sumY, int count)>();


            //foreach (var p in raw)
            foreach (var p in pts)
            {
                int found = -1;

                // 가장 먼저 발견되는 클러스터에 병합 (필요 시 '가장 가까운 클러스터'로 개선 가능)
                for (int i = 0; i < centers.Count; i++)
                {
                    double dx = p.X - centers[i].X;
                    double dy = p.Y - centers[i].Y;
                    if (dx * dx + dy * dy <= dist2)
                    {
                        found = i;
                        break;
                    }
                }

                if (found < 0)
                {
                    centers.Add(p);
                    sums.Add((p.X, p.Y, 1));
                }
                else
                {
                    var acc = sums[found];
                    acc.sumX += p.X;
                    acc.sumY += p.Y;
                    acc.count++;
                    sums[found] = acc;

                    centers[found] = new PointD(acc.sumX / acc.count, acc.sumY / acc.count);
                }
            }

            // 2차 병합: 클러스터 간 근접한 것 통합(전이적 병합 보장)
            bool merged;
            do
            {
                merged = false;
                for (int i = 0; i < centers.Count && !merged; i++)
                {
                    for (int j = i + 1; j < centers.Count; j++)
                    {
                        double dx = centers[j].X - centers[i].X;
                        double dy = centers[j].Y - centers[i].Y;
                        if (dx * dx + dy * dy <= dist2)
                        {
                            // i <- i + j
                            var ai = sums[i];
                            var aj = sums[j];
                            var comb = (ai.sumX + aj.sumX, ai.sumY + aj.sumY, ai.count + aj.count);
                            sums[i] = comb;
                            centers[i] = new PointD(comb.Item1 / comb.Item3, comb.Item2 / comb.Item3);

                            // j 제거
                            sums.RemoveAt(j);
                            centers.RemoveAt(j);
                            merged = true;
                            break;
                        }
                    }
                }
            } while (merged);


            return centers;
        }
        // PmRunner.Search() 직접 결과를 받을 때 사용하는 오버로드
        private void OnRawMatchesFound(VisionImage img,
                                   IEnumerable<QMC.Common.Vision.Tools.PatternMatchingResult.PatternMatchingResultValue> rawMatches,
                                   int representativeIndex,
                                   int trainW,
                                   int trainH,
                                   double gX = 0,
                                   double gY = 0)
        {
            var e = new PatternMarksFoundEventArgs
            {
                Suspended = false,
                Image = img,
                RepresentativeIndex = representativeIndex
            };
            if (rawMatches == null || img == null)
            {
                try { MarksFound?.Invoke(this, e); } catch { }
                return;
            }

            foreach (var m in rawMatches)
            {
                if (gX == 0 && gY == 0)
                {
                    e.Marks.Add(new PatternMatchInfo
                    {
                        X = m.X,
                        Y = m.Y,
                        AngleDeg = m.R,
                        Score = m.Score,
                        TrainW = trainW,
                        TrainH = trainH
                    });
                }
                else
                {
                    double dDiffX = Math.Abs(m.X - gX);
                    double dDiffY = Math.Abs(m.Y - gY);
                    if ((dDiffX < 0.0001)
                     && (dDiffY < 0.0001))
                    {
                        e.Marks.Add(new PatternMatchInfo
                        {
                            X = m.X,
                            Y = m.Y,
                            AngleDeg = m.R,
                            Score = m.Score,
                            TrainW = trainW,
                            TrainH = trainH
                        });
                    }
                }
            }
            try { MarksFound?.Invoke(this, e); } catch { }
        }
        #endregion

        #region Simulation Helpers
        private List<PointD> _simAllDiesPool = new List<PointD>();
        private HashSet<long> _simAddedKeys = new HashSet<long>();
        private bool _simDiesGenerated = false;

        // 좌표를 1µm 해상도로 정수 키화(중복 방지)
        private static long MakeQuantKey(PointD p, double scale = 1000.0)
        {
            int qx = (int)Math.Round(p.X * scale);
            int qy = (int)Math.Round(p.Y * scale);
            unchecked
            {
                return ((long)qx << 32) ^ (uint)qy;
            }
        }
        // 시뮬 전역 풀 생성(1회)
        private void EnsureSimDiePoolGenerated()
        {
            if (!_simDiesGenerated)
            {
                _simAllDiesPool.Clear();
                GenerateAllSimDies(_simAllDiesPool); // 웨이퍼 직경/피치 기반 그리드 생성
                _simAddedKeys.Clear();
                _simDiesGenerated = true;
                Log.Write(UnitName, "Sim", $"SimDiePool Generated: {_simAllDiesPool.Count}");
            }
        }
        /// <summary>
        /// 시뮬레이션 모드에서 웨이퍼 직경과 피치를 이용해 전체 칩 중심 좌표를 한 번에 생성.
        /// </summary>
        private void GenerateAllSimDies(List<PointD> dest)
        {
            try
            {
                var eq = Equipment.Instance;
                var recip = eq.EquipmentRecipe.CurrentRecipe;

                double radius = Math.Max(0, recip.WaferDiameter) / 2.0;
                if (radius <= 0)
                    return;

                double pitchX = ChipPitchXmm;
                double pitchY = ChipPitchYmm;
                if (pitchX <= 0) pitchX = 0.5;
                if (pitchY <= 0) pitchY = 0.5;

                // Teaching Center 기준
                double centerX = GetTP(InputStageConfig.TeachingPositionName.CenterPoint.ToString(), AxisX.Name);
                double centerY = GetTP(InputStageConfig.TeachingPositionName.CenterPoint.ToString(), AxisY.Name);

                int cols = Math.Max(1, (int)Math.Floor((radius * 2.0) / pitchX) + 1);
                int rows = Math.Max(1, (int)Math.Floor((radius * 2.0) / pitchY) + 1);

                double originX = centerX - (cols - 1) * pitchX / 2.0;
                double originY = centerY - (rows - 1) * pitchY / 2.0;

                double includeRadius = radius;
                double includeR2 = includeRadius * includeRadius;

                for (int r = 0; r < rows; r++)
                {
                    double y = originY + r * pitchY;
                    for (int c = 0; c < cols; c++)
                    {
                        double x = originX + c * pitchX;
                        double dx = x - centerX;
                        double dy = y - centerY;
                        if (dx * dx + dy * dy <= includeR2)
                        {
                            dest.Add(new PointD(x, y));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Write(UnitName, "GenerateAllSimDies", $"Exception: {ex.Message}");
            }
        }
        #endregion

        #region Teaching Helpers (Small Wrappers)
        /// //////////////////////////////////////////////////////////////////////////////////////////////
        // UI, sequence 용 Move 함수
        public int MoveTeachingPositionOnce(InputStageConfig.TeachingPositionName name, bool isFine)
        {
            return MoveTeachingPositionOnce((int)name, isFine);
        }
        #endregion

        #region Sequence Registration
        protected override void OnMakeSequence()
        {
            base.OnMakeSequence();
            this.SequencePlayers.Add(LoadingWaferComplete);
            this.SequencePlayers.Add(AlignT);
            this.SequencePlayers.Add(PerformChipMapping);
            this.SequencePlayers.Add(MoveStageToNextDie);
        }
        #endregion

        #region Actuator Wait Helpers (Plate / ClampLift / ClampFB)
        // === Cylinder 완료 대기 Helpers === // Plate: expectUp=true(UP 기대), false(DOWN 기대)
        private int WaitPlateStateOrAlarm(bool expectUp, int timeoutMs = 3000, int pollMs = 1)
        {
            var equipment = Equipment.Instance;
            bool IsDryRunEqp = equipment.EquipmentConfig.IsDryRun;
            if (Config.IsSimulation || (Config.IsDryRun || IsDryRunEqp))
                return 0;

            var sw = System.Diagnostics.Stopwatch.StartNew();
            while (sw.ElapsedMilliseconds <= timeoutMs)
            {
                bool ok = expectUp ? IsPlateUp() : IsPlateDown();
                if (ok)
                    return 0;
                Thread.Sleep(pollMs);
            }

            AxisX?.EmgStop(); AxisY?.EmgStop(); AxisT?.EmgStop();
            PostAlarm((int)AlarmKeys.eInputStageMoveFail);
            Log.Write(UnitName, expectUp ? "[Plate] UP timeout" : "[Plate] DOWN timeout");
            return -1;
        }

        // ClampLift: expectUp=true(UP 기대), false(DOWN 기대)
        private int WaitClampLiftStateOrAlarm(bool expectUp, int timeoutMs = 3000, int pollMs = 1)
        {
            var equipment = Equipment.Instance;
            bool IsDryRunEqp = equipment.EquipmentConfig.IsDryRun;
            if (Config.IsSimulation || (Config.IsDryRun || IsDryRunEqp))
                return 0;

            var sw = System.Diagnostics.Stopwatch.StartNew();
            while (sw.ElapsedMilliseconds <= timeoutMs)
            {
                bool ok = expectUp ? IsClampLiftUp() : IsClampLiftDown();
                if (ok) return 0;
                Thread.Sleep(pollMs);
            }

            AxisX?.EmgStop(); AxisY?.EmgStop(); AxisT?.EmgStop();
            PostAlarm((int)AlarmKeys.eInputStageMoveFail);
            Log.Write(UnitName, expectUp ? "[ClampLift] UP timeout" : "[ClampLift] DOWN timeout");
            return -1;
        }
        // Clamp F/B: expectFwd=true(FWD 기대), false(BWD 기대)
        private int WaitClampFBStateOrAlarm(bool expectFwd, int timeoutMs = 3000, int pollMs = 1)
        {
            var equipment = Equipment.Instance;
            bool IsDryRunEqp = equipment.EquipmentConfig.IsDryRun;
            if (Config.IsSimulation || (Config.IsDryRun || IsDryRunEqp))
                return 0;

            var sw = System.Diagnostics.Stopwatch.StartNew();
            while (sw.ElapsedMilliseconds <= timeoutMs)
            {
                bool ok = expectFwd ? IsClampFwd() : IsClampBwd();
                if (ok) return 0;
                Thread.Sleep(pollMs);
            }

            AxisX?.EmgStop(); AxisY?.EmgStop(); AxisT?.EmgStop();
            PostAlarm((int)AlarmKeys.eInputStageMoveFail);
            Log.Write(UnitName, expectFwd ? "[ClampFB] FWD timeout" : "[ClampFB] BWD timeout");
            return -1;
        }
        #endregion

        #region Mapping / Index Utilities
        // 파일 상단 클래스 내부(적절한 private 영역)에 추가)
        private void NormalizeIndicesSequential(MaterialWafer wafer, int startIndex = 0, bool rename = true)
        {
            if (wafer == null || wafer.Dies == null || wafer.Dies.Count == 0) return;

            lock (wafer.Dies)
            {
                // 현재 Index 오름차순 → 연속 재부여
                var orderedByIndex = wafer.Dies.Where(d => d != null).OrderBy(d => d.Index).ToList();
                for (int i = 0; i < orderedByIndex.Count; i++)
                {
                    var d = orderedByIndex[i];
                    d.Index = startIndex + i;
                    if (rename && !string.IsNullOrEmpty(wafer.WaferId))
                        d.Name = $"{wafer.WaferId}_{d.Index}";
                }
            }
        }
        #endregion

        #region Prepare / Load-Unload Helpers
        public int PrepareLoadingStage()
        {
            int nRtn = 0;

            Log.Write(UnitName, "PrepareLoadingStage", "Start LoadingWaferPrepare");

            // 새 웨이퍼 준비 진입 → 맵 상태 리셋
            ResetChipMappingState();

            // ===== [FIX] Stage 센서/데이터 정합성 기반으로 판단 =====
            var equipment = Equipment.Instance;
            bool IsDryRunEqp = equipment.EquipmentConfig.IsDryRun;
            if (Config.IsSimulation == false && (Config.IsDryRun == false && IsDryRunEqp == false))
            {
                bool present = IsRingPresent();
                var wafer = GetMaterialWafer();

                // 센서 ON인데 객체가 없으면 비정상(상위에서 처리할 수도 있지만 여기서 방어)
                if (present && wafer == null)
                {
                    Log.Write(UnitName, "PrepareLoadingStage", "RingPresent but wafer object null");
                    return -1;
                }

                // 센서 ON + 객체 존재 + 아직 완료 상태가 아니다 => 현재 작업/보유중이므로 Prepare 금지
                if (present && wafer != null && wafer.ProcessSatate != MaterialProcessSatate.Completed)
                {
                    Log.Write(UnitName, "PrepareLoadingStage", $"Stage already has wafer. State={wafer.ProcessSatate}");
                    return -1;
                }
            }

            // 로딩 Teaching 이동
            nRtn = MoveToStageLoadPosition();
            if (nRtn != 0)
            {
                Log.Write(UnitName, "LoadingWaferPrepare", "Fail: MoveToStageLoadPosition");
                return nRtn;
            }
            nRtn = ClampBackward();
            if (nRtn != 0)
            {
                Log.Write(UnitName, "LoadingWaferPrepare", "Fail: ClampBackward");
                return nRtn;
            }

            nRtn = ClampLiftDown();
            if (nRtn != 0)
            {
                Log.Write(UnitName, "LoadingWaferPrepare", "Fail: ClampLiftDown");
                return nRtn;
            }

            nRtn = PlateDown();
            if (nRtn != 0)
            {
                Log.Write(UnitName, "LoadingWaferPrepare", "Fail: PlateDown");
                return nRtn;
            }

            Log.Write(UnitName, "LoadingWaferPrepare", "End LoadingWaferPrepare");
            return 0;
        }
        public int PrepareInputStageUnloadingWafer()
        {
            int nRtn = 0;
            Log.Write(UnitName, "UnloadingPrep", "Start");

            // 언로딩 준비 진입 → 맵 상태 리셋
            ResetChipMappingState();

            //여기서 wafer가 없어도 언로더 되도 되지 않나?
            if (!IsRingPresent())
            {
                Log.Write(UnitName, "UnloadingPrep", "No wafer -> Skip");
                return 0;
            }

            nRtn = MoveToStageUnloadPosition();
            if (nRtn != 0)
            {
                Log.Write(UnitName, "UnloadingPrep", "Fail: MoveToStageUnloadPosition");
                return -1;
            }

            nRtn = ClampBackward();
            if (nRtn != 0)
            {
                Log.Write(UnitName, "UnloadingPrep", "Fail: ClampBackward");
                return -1;
            }
            nRtn = ClampLiftDown();
            if (nRtn != 0)
            {
                Log.Write(UnitName, "UnloadingPrep", "Fail: ClampLiftDown");
                return -1;
            }
            nRtn = PlateDown();
            if (nRtn != 0)
            {
                Log.Write(UnitName, "UnloadingPrep", "Fail: PlateDown");
                return -1;
            }

            Log.Write(UnitName, "UnloadingPrep", "StageUnloadingReady = TRUE (Wait wafer pick)");
            return 0;
        }
        #endregion

        #region Interlock Helpers (Feeder / DieTransfer / Ejector)
        public bool IsInterlockWithFeederAndDieTransferOk()
        {
            return IsInterlockWithFeederAndDieTransferOkInt() == 0;
        }
        public int IsInterlockWithFeederAndDieTransferOkInt()
        {
            if(InputFeeder.ExchangeStandbyForNextLoad == false)
            {
                if(InputFeeder.IsPositionBarcode() == false)
                {
                    if (InputFeeder.IsPositionFeederZSafety() == false)
                    {
                        Log.Write(UnitName, "Interlock", "Feeder Z not safe");
                        return -1;
                    }
                    if (InputFeeder.IsPositionFeederYSafety() == false)
                    {
                        Log.Write(UnitName, "Interlock", "Feeder Y not safe");
                        return -2;
                    }
                }
            }

            if(InputFeeder.IsPositionReady() == false)
            {
                Log.Write(UnitName, "Interlock", "InputFeeder Y not safe");
                return -3;
            }
            
            // InputDieTransfer
            if (InputDieTransfer.IsPositionPickZSafety() == false)
            {
                Log.Write(UnitName, "Interlock", "DieTransfer Pick Z not safe");
                return -3;
            }
            // InputStageEjector
            if (InputStageEjector.IsPinZSafetyPos() == false)
            {
                Log.Write(UnitName, "Interlock", "Stage Ejector Pin Z not safe");
                return -4;
            }
            if (InputStageEjector.IsEjectorZSafetyPos() == false)
            {
                Log.Write(UnitName, "Interlock", "Stage Ejector Z not safe");
                return -5;
            }
            return 0;
        }
        #endregion

        #region Vision Align Helpers (Prepare / Directional / Dual-Point / SearchAround)
        private TeachingPosition _lastCenterAlignTp;
        private int PrepareForAlign(out TeachingPosition centerTp, out VisionImage img)
        {
            int nRet = 0;

            centerTp = null;
            img = null;

            // 1) 인터락
            if (!IsRingPresent())
            {
                AxisX?.EmgStop(); AxisY?.EmgStop(); AxisT?.EmgStop();
                PostAlarm((int)AlarmKeys.eInputStageRingPresent);
                Log.Write(UnitName, "Align", "Fail: Ring(Wafer) not present");
                return -1;
            }
            if (!IsClampLiftUp())
            {
                AxisX?.EmgStop(); AxisY?.EmgStop(); AxisT?.EmgStop();
                PostAlarm((int)AlarmKeys.eInputStageLiftUp);
                Log.Write(UnitName, "Align", "Fail: Clamp Lift not Up");
                return -1;
            }

            if (!IsClampFwd())
            {
                AxisX?.EmgStop(); AxisY?.EmgStop(); AxisT?.EmgStop();
                PostAlarm((int)AlarmKeys.eInputStageClampFWD);
                Log.Write(UnitName, "Align", "Fail: Clamp not FWD");
                return -1;
            }

            MaterialWafer wafer = GetMaterialWafer();
            if(wafer is null)
            {
                wafer = new MaterialWafer();
                SetMaterial(wafer);
            }

            //무조건 Center 말고 현재 위치에서 시작하자.
            //nRet = MoveToStageCenterPosition();
            //if (nRet != 0)
            //{
            //    AxisX?.EmgStop(); AxisY?.EmgStop(); AxisT?.EmgStop();
            //    PostAlarm((int)AlarmKeys.eInputStageMoveFail);
            //    Log.Write(UnitName, "Align", "Fail: Move Center");
            //    return -1;
            //}

            // 2) Center TeachingPosition 확보
            //   - 명칭 기반 우선
            centerTp = Config.GetTeachingPosition(InputStageConfig.TeachingPositionName.CenterPoint.ToString());
            if (centerTp == null)
            {
                //   - 인덱스 기반 폴백
                int idx = (int)InputStageConfig.TeachingPositionName.CenterPoint;
                if (Config.TeachingPositions != null &&
                    idx >= 0 && idx < Config.TeachingPositions.Count)
                {
                    centerTp = Config.TeachingPositions[idx];
                }
            }

            return 0;
        }

        private bool TryGetCenterMarkFromRoi(
                bool useXAxis,
                out double gx, out double gy,
                out double angleDeg,
                out double score)
        {
            gx = gy = angleDeg = score = 0;

            var equipment = Equipment.Instance;
            bool IsDryRunEqp = equipment.EquipmentConfig.IsDryRun;
            if (Config.IsSimulation || (Config.IsDryRun || IsDryRunEqp))
                return true;

            // Grab
            StageCamera.SuspendedImageDisplay = true;
            int rc = StageCamera.GrabSync(out VisionImage visionImage);
            if (rc != 0 || visionImage == null)
                return false;

            // ROI 크기 결정 (기존 로직 유지: X/Y 방향에 따라 ROI 형태 다르게)
            double LocalRoiWmm = useXAxis ? 5.0 : 1.5;
            double LocalRoiHmm = useXAxis ? 1.5 : 5.0;

            double mmPerPxX = Math.Abs(StageCamera.CameraConfig.Scale.X);
            double mmPerPxY = Math.Abs(StageCamera.CameraConfig.Scale.Y);
            if (mmPerPxX <= 0) mmPerPxX = PixelSizeXmm;
            if (mmPerPxY <= 0) mmPerPxY = PixelSizeYmm;

            int roiWpx = (int)Math.Round(LocalRoiWmm / mmPerPxX);
            int roiHpx = (int)Math.Round(LocalRoiHmm / mmPerPxY);

            int imgW = visionImage.Header.Width;
            int imgH = visionImage.Header.Height;
            int cx = imgW / 2;
            int cy = imgH / 2;

            int halfW = Math.Max(1, roiWpx / 2);
            int halfH = Math.Max(1, roiHpx / 2);

            int sx0 = Math.Max(0, cx - halfW);
            int sy0 = Math.Max(0, cy - halfH);
            int ex0 = Math.Min(imgW - 1, cx + halfW);
            int ey0 = Math.Min(imgH - 1, cy + halfH);

            // 여기서 SearchWithTemporaryInspectRoi 대신 "CenterMark + ROI" 사용
            //this.PmRunner._opt.PreferCenterMostMatch = true;
            Thread.Sleep(30);
            var result = this.PmRunner.SearchCenterMarkWithTemporaryInspectRoi(
                visionImage,
                new Point(sx0, sy0),
                new Point(ex0, ey0),
                save: false);

            if (result != null && result.Success && result.Matches != null && result.Matches.Count > 0)
            {
                int repIdx = 0;
                int trainW = 0, trainH = 0;
                try
                {
                    var ti = PmRunner.Parameters?.TrainImages?
                        .FirstOrDefault(t => t?.Header != null && t.Header.Width > 0 && t.Header.Height > 0);
                    if (ti != null)
                    {
                        trainW = ti.Header.Width;
                        trainH = ti.Header.Height;
                    }
                }
                catch { /* ignore */ }
                OnRawMatchesFound(visionImage, result.Matches, repIdx, trainW, trainH);
                StageCamera.SuspendedImageDisplay = false;
            }
            else
            {
                StageCamera.SuspendedImageDisplay = false;
                return false;
            }

            // SearchCenterMark는 matches 1개만 남겨줌
            var m = result.Matches[0];

            // pixel -> mm offset
            var off = GetPixelToMmScale(m.X, m.Y);

            double stageX = AxisX.GetPosition();
            double stageY = AxisY.GetPosition();

            gx = stageX + off.X;
            gy = stageY + off.Y;
            angleDeg = m.R;
            score = m.Score;

            return true;
        }

        // mm 단위 Tolerance (레시피 값) → deg 변환 (arc length ≈ R*θ, θ(rad)=s/R)
        private double ComputeResidualToleranceDegFromMm(double toleranceMm)
        {
            if (toleranceMm <= 0) return TRefineResidualToleranceDeg;
            double waferDia = Equipment.Instance?.EquipmentRecipe?.CurrentRecipe?.WaferDiameter ?? 100.0;
            double radius = waferDia / 2.0;
            if (radius <= 0) return TRefineResidualToleranceDeg;
            double rad = toleranceMm / radius;            // θ(rad) = s / R
            return rad * 180.0 / Math.PI;                // deg
        }
        // 웨이퍼/레시피 변경 후 호출 (Pitch, Residual 허용 각 재계산)
        public void OnWaferOrRecipeChanged()
        {
            ApplyDynamicPitchParameters(); // Pitch 기반 파라미터 재설정
            var recip = Equipment.Instance?.EquipmentRecipe?.CurrentRecipe;
            if (recip != null && recip.ToleranceMm > 0)
            {
                //recip.ToleranceMm <- mm X, 각도임.
                TRefineResidualToleranceDeg = recip.ToleranceMm;    // ComputeResidualToleranceDegFromMm(recip.ToleranceMm);
                Log.Write(UnitName, "ThetaTol", $"Recipe.ToleranceMm={recip.ToleranceMm:F4}mm -> ResidualTol={TRefineResidualToleranceDeg:F5}deg");
            }
        }
        // === Pitch 기반 동적 파라미터 자동 적용 ===
        private void ApplyDynamicPitchParameters()
        {
            try
            {
                var recip = Equipment.Instance?.EquipmentRecipe?.CurrentRecipe;
                // 사용자가 전달: 칩 간 거리 0.9mm
                double pitchX = 0.9;
                double pitchY = 0.9;

                // 레시피가 이미 값 가지고 있으면 그 값 우선, 없으면 0.95 적용
                if (recip != null)
                {
                    if (recip.WChipPitchX > 0) pitchX = recip.WChipPitchX;
                    else recip.WChipPitchX = pitchX;

                    if (recip.WChipPitchY > 0) pitchY = recip.WChipPitchY;
                    else recip.WChipPitchY = pitchY;
                }

#if DEBUG
                Log.Write(UnitName, $"[Pitch] ApplyDynamicPitchParameters pitchX={pitchX:F3} pitchY={pitchY:F3}");
#endif

                // 중복 병합 거리: 인접 셀 간섭 피하기 위해 0.45~0.50배
                DuplicateDistMm = Math.Min(pitchX, pitchY) * 0.48;

                // 수직 라인 허용 오차: 너무 크면 잘못된 라인, 너무 작으면 후보 소실 → 0.12~0.18배
                //DirectionalPerpendicularToleranceMm = Math.Min(pitchX, pitchY) * 0.15;
                //DirectionalPerpendicularToleranceMm = Math.Min(pitchX, pitchY) * 0.15;
                DirectionalPerpendicularToleranceMm = Math.Min(pitchX, pitchY) * 0.2;

                // 잔류 각 허용: 웨이퍼 직경 사용 (없으면 100mm 가정)
                double waferDia = recip?.WaferDiameter > 0 ? recip.WaferDiameter : 100.0;
                // 픽업/공정 허용 기준을 다소 보수적으로: 피치 기반 각 허용치(호도 법)
                // 한 칩 오차(피치) 이상 회전 오차로 인한 단차: pitch ≈ R * θ  ⇒ θ ≈ pitch / R (라디안)
                // deg = (pitch / (waferDia/2)) * (180/π)
                double idealDeg = (pitchX / (waferDia / 2.0)) * (180.0 / Math.PI);
                // 너무 작으면 반복 과보정 → 스케일 팩터 2~3배
                TRefineResidualToleranceDeg = Math.Max(0.02, idealDeg * 2.5); // 대략 0.05~0.15 사이 기대

                // 시프트 비율: ROI 폭/높이 없을 때 피치*2 사용 (최소 이격 확보)
                // ROI는 레시피 로드 후 TryAcquireDualPointAngle 내부에서 재조정.
            }
            catch (Exception ex)
            {
                Log.Write(UnitName, "[Pitch] Exception: " + ex.Message);
            }
        }
        // 각도 통계 + 대표값 (멀티결과 사용)
        private bool TryGetRepresentativeTheta(out double repDeg, out double stdDeg, out int count)
        {
            repDeg = 0; stdDeg = 0; count = 0;
            if (!TryGetMultiAngles(out var list) || list == null || list.Count == 0)
                return false;

            count = list.Count;
            // 극단 제거: score/품질 안 들어오므로 단순 상하 1개씩 제거 (N>=5)
            var ordered = list.OrderBy(a => a).ToList();
            if (ordered.Count >= 5)
                ordered = ordered.Skip(1).Take(ordered.Count - 2).ToList();

            double avg = ordered.Average();
            double var = 0;
            if (ordered.Count > 1)
                var = ordered.Sum(a => (a - avg) * (a - avg)) / (ordered.Count - 1);
            stdDeg = Math.Sqrt(var);

            // 대표값: avg 와 가장 가까운 원본
            repDeg = list.OrderBy(a => Math.Abs(a - avg)).First();
            return true;
        }
        // 기울기 계산(두 점 좌표 → 잔류 각도). 작은 기울기만 기대.
        private static double ComputeSlopeDeg(bool useXAxis, double x1, double y1, double x2, double y2)
        {
            double dx = x2 - x1;
            double dy = y2 - y1;
            double dResultT = 0.0;
            if (useXAxis)
            {
                // 수평 기준: ΔY / ΔX
                if (Math.Abs(dx) < 1e-9)
                    return 0;

                return Math.Atan(dy / dx) * 180.0 / Math.PI;
            }
            else
            {
                // 수직 기준: ΔX / ΔY
                if (Math.Abs(dy) < 1e-9) return 0;

                dResultT = Math.Atan(dx / dy) * 180.0 / Math.PI;
                return dResultT * -1;
            }
        }

        PointD ptFirst;
        PointD ptSecond;
        private bool TryAcquireDualPointAngle2(bool useXAxis, out double angleDeg, bool bFineSpeed, int nStep)
        {
            angleDeg = 0;

            var equipment = Equipment.Instance;
            bool IsDryRunEqp = equipment.EquipmentConfig.IsDryRun;
            if (Config.IsSimulation || (Config.IsDryRun || IsDryRunEqp))
                return true;

            // Step 방어 (nStep==0이면 현재 위치 기준, 이동 스텝은 1칩 간격 사용)
            bool useCurrentAsBase = (nStep == 0);
            int step = useCurrentAsBase ? 1 : Math.Max(1, nStep);

            // 이동 거리(피치 기반)
            double pitch = useXAxis ? ChipPitchXmm : ChipPitchYmm;
            if (pitch <= 0) pitch = 1; // fallback

            double baseX;
            double baseY;

            if (useCurrentAsBase)
            {
                // nStep==0: 현재 위치를 기준점으로 사용 (센터 재정렬/이동 없이 진행)
                baseX = AxisX.GetPosition();
                baseY = AxisY.GetPosition();
            }
            else
            {
                // nStep>=1: 기존 동작 유지 (센터 ROI로 마크를 찾아 그 위치로 이동해서 기준점 생성)
                if (!TryGetCenterMarkFromRoi(useXAxis, out var gx0, out var gy0, out var angle0, out var score0))
                {
                    Log.Write(UnitName, "DualPointAngle", "[CenterRoi] Mark0 find fail");
                    return false;
                }

                if (MoveStage(gx0, gy0, bFineSpeed) != 0)
                {
                    Log.Write(UnitName, "DualPointAngle", "[CenterRoi] Move0 fail");
                    return false;
                }
                if (WaitUntil(() => AxisX.InPosition(gx0) && AxisY.InPosition(gy0), MoveTimeoutMs) != 0)
                {
                    Log.Write(UnitName, "DualPointAngle", "[CenterRoi] WaitUntil fail");
                    return false;
                }

                Thread.Sleep(5);

                baseX = AxisX.GetPosition();
                baseY = AxisY.GetPosition();
            }

            // 2) -방향 포인트
            //Todo: 
            //step을 wafer 사이즈 기준으로 잡아야함.
            var recipe = Equipment.Instance?.EquipmentRecipe?.CurrentRecipe;
            int repeatDist = (recipe != null && recipe.AlignRepeatDistance > 0)
                ? recipe.AlignRepeatDistance
                : 3;

            step *= repeatDist;

            // Max radius = wafer radius * 0.8
            // stepMax = floor(maxRadius / pitch)
            double waferDia = (recipe != null && recipe.WaferDiameter > 0)
                ? recipe.WaferDiameter
                : 0.0;

            if (waferDia > 0)
            {
                double waferRadius = waferDia * 0.5;
                double maxRadius = waferRadius * 0.80;

                // useXAxis에 따라 pitch가 다르므로, 여기서는 이동 축 기준 pitch를 사용해야 함.
                // (본 함수 상단에 pitch 변수가 이미 있음)
                if (pitch > 0)
                {
                    int stepMax = (int)Math.Floor(maxRadius / pitch);
                    if (stepMax < 1) stepMax = 1;

                    if (step > stepMax)
                        step = stepMax;
                }
            }


            double negX = baseX + (useXAxis ? -pitch * step : 0.0);
            double negY = baseY + (useXAxis ? 0.0 : -pitch * step);

            if (MoveStage(negX, negY, bFineSpeed) != 0)
            {
                Log.Write(UnitName, "DualPointAngle", "[CenterRoi] Move(-) fail");
                return false;
            }
            if (WaitUntil(() => AxisX.InPosition(negX) && AxisY.InPosition(negY), MoveTimeoutMs) != 0)
                return false;

            Thread.Sleep(30);

            if (!TryGetCenterMarkFromRoi(useXAxis, out var gx1, out var gy1, out var a1, out var s1))
            {
                Log.Write(UnitName, "DualPointAngle", "[CenterRoi] Mark(-) find fail");
                return false;
            }

            // 3) +방향 포인트
            double posX = baseX + (useXAxis ? +pitch * step : 0.0);
            double posY = baseY + (useXAxis ? 0.0 : +pitch * step);

            if (MoveStage(posX, posY, bFineSpeed) != 0)
            {
                Log.Write(UnitName, "DualPointAngle", "[CenterRoi] Move(+) fail");
                return false;
            }
            if (WaitUntil(() => AxisX.InPosition(posX) && AxisY.InPosition(posY), MoveTimeoutMs) != 0)
                return false;

            Thread.Sleep(30);

            if (!TryGetCenterMarkFromRoi(useXAxis, out var gx2, out var gy2, out var a2, out var s2))
            {
                Log.Write(UnitName, "DualPointAngle", "[CenterRoi] Mark(+) find fail");
                return false;
            }

            // 4) 품질 체크: 수직 허용오차
            DirectionalPerpendicularToleranceMm = 0.4;
            double tol = Math.Max(0.0, DirectionalPerpendicularToleranceMm);
            if (useXAxis)
            {
                if (Math.Abs(gy2 - gy1) > tol)
                {
                    Log.Write(UnitName, "DualPointAngle",
                        $"[CenterRoi] Reject: |dy|={Math.Abs(gy2 - gy1):F4} > tol={tol:F4}");
                    return false;
                }
            }
            else
            {
                if (Math.Abs(gx2 - gx1) > tol)
                {
                    Log.Write(UnitName, "DualPointAngle",
                        $"[CenterRoi] Reject: |dx|={Math.Abs(gx2 - gx1):F4} > tol={tol:F4}");
                    return false;
                }
            }

            // 5) 두 점으로 잔류각 계산
            angleDeg = ComputeSlopeDeg(useXAxis, gx1, gy1, gx2, gy2);

            ptFirst = new PointD(gx1, gy1);
            ptSecond = new PointD(gx2, gy2);

            Log.Write(UnitName, "DualPointAngle",
                $"[CenterRoi] axis={(useXAxis ? "X" : "Y")} step={step} base=({baseX:F4},{baseY:F4}) " +
                $"P1=({gx1:F4},{gy1:F4}) P2=({gx2:F4},{gy2:F4}) angle={angleDeg:F6}");

            if (SearchAroundReturnToCenter)
            {
                MoveStage(baseX, baseY, bFineSpeed);
                if (WaitUntil(() => AxisX.InPosition(baseX) && AxisY.InPosition(baseY), MoveTimeoutMs) != 0)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// 추가 각도 보정 적용 (제한/로그/상태 갱신)
        /// </summary>
        private int ApplyThetaCorrection(bool useXAxis, double correctionDeg, bool isAuto, bool bFineSpeed)
        {
            double cur = AxisT.GetPosition();
            double limited = Math.Max(-AngleMaxApplyDeg, Math.Min(AngleMaxApplyDeg, correctionDeg));
            double target = cur + limited;

            Log.Write(UnitName, "ThetaRefine",
                $"Apply correction={correctionDeg:F4}deg (limited={limited:F4}) curT={cur:F4} -> target={target:F4}");

            // WaferStageT : 0 ~ 12도 
            // 이거 이상 움직이면 Limit으로 NG임.
            if (target < -0.2 || target > 12) //Stage 소프트리밋 가져올까?
            {
                PostAlarm((int)AlarmKeys.eInputStageAlignNotDone);
                Log.Write(UnitName, "T_Align", "Coarse move Limit");
                return -1;
            }

            int rc = AxisT.MoveAbs(target, isAuto, bFineSpeed);
            if (rc != 0)
                return rc;

            rc = WaitUntil(() => InPos(AxisT, target), MoveTimeoutMs);
            if (rc == 0)
                IsStatus_LastAppliedTAngle += limited; // 누적 적용 각도

            return rc;
        }

        /// <summary>
        /// 센터에서 마크 실패 시 주변(상/하/좌/우/대각)으로 이동하며 패턴 탐색.
        /// 성공하면 angleDeg 갱신 후 true.
        /// </summary>
        private bool TryFindAngleAroundCenter(out double angleDeg, bool bFineSpeed)
        {
            angleDeg = 0.0;
            if (!EnableSearchAroundCenter)
                return false;

            // 센터 Teaching 좌표
            var tp = Config.GetTeachingPosition(InputStageConfig.TeachingPositionName.CenterPoint.ToString());
            if (tp == null)
                return false;

            double baseX = tp.GetAxisPosition(AxisNames.WaferStageX);
            double baseY = tp.GetAxisPosition(AxisNames.WaferStageY);

            // 이동 스텝(px/mm)
            double pitchX = ChipPitchXmm > 0 ? ChipPitchXmm : 0.8;
            double pitchY = ChipPitchYmm > 0 ? ChipPitchYmm : pitchX;
            double stepX = pitchX * SearchAroundPitchScale;
            double stepY = pitchY * SearchAroundPitchScale;
            double minStep = Math.Min(pitchX, pitchY) * 0.6;
            if (stepX < minStep) stepX = minStep;
            if (stepY < minStep) stepY = minStep;

            // ROI 크기 일부 활용(칩이 더 작을 경우)
            try
            {
                if (PmRunner.IsRecipeLoaded)
                {
                    double roiW = Math.Abs((PmRunner._Roi.InspectEnd.X - PmRunner._Roi.InspectStart.X) * StageCamera.CameraConfig.Scale.X);
                    double roiH = Math.Abs((PmRunner._Roi.InspectEnd.Y - PmRunner._Roi.InspectStart.Y) * StageCamera.CameraConfig.Scale.Y);
                    if (roiW > 0 && roiW < stepX * 0.8) stepX = roiW * 0.8;
                    if (roiH > 0 && roiH < stepY * 0.8) stepY = roiH * 0.8;
                }
            }
            catch { /* ignore */ }

            // 후보 방향 (링 확장)
            var dirsBase = new List<(double dx, double dy)>
            {
                ( 0,  1),( 0, -1),( -1, 0),( 1, 0), // 상하좌우
                ( 1,  1),( -1,  1),( 1, -1),( -1, -1) // 대각
            };

            // 안전 반경
            double safeR = Config.SafeSatageRaius > 0 ? Config.SafeSatageRaius : 9999.0;

            bool VisionTry(out double angle)
            {
                angle = 0.0;
                VisionImage img;
                int rcGrab = StageCamera.GrabSync(out img);
                if (rcGrab != 0 || img == null) return false;

                double a;
                PmRunner.SearchTheta(img, out a);
                if (Math.Abs(a) < 1e-9) return false;
                angle = a;
                return true;
            }

            SearchAroundMaxRings = 5;
            for (int ring = 1; ring <= SearchAroundMaxRings; ring++)
            {
                foreach (var (dxUnit, dyUnit) in dirsBase)
                {
                    double tx = baseX + dxUnit * stepX * ring;
                    double ty = baseY + dyUnit * stepY * ring;

                    // 안전 반경 체크 (Ejector 안전 아닐 때만 제한 → 기존 IsStageInterLockOK 활용)
                    if (!IsStageInterLockOK(tx, ty))
                        continue;

                    if (MoveStage(tx, ty, bFineSpeed) != 0)
                        continue;

                    if (WaitUntil(() => AxisX.InPosition(tx) && AxisY.InPosition(ty),
                                  SearchAroundMoveTimeoutMs) != 0)
                        continue;

                    Thread.Sleep(100);

                    if (VisionTry(out double found))
                    {
                        angleDeg = found;
                        Log.Write(UnitName, "SearchAroundCenter",
                            $"Found angle={found:F5}deg at ring={ring} offset=({tx - baseX:+0.000;-0.000},{ty - baseY:+0.000;-0.000})");

                        // 필요하면 센터로 복귀 후 적용. 여기서는 바로 사용.
                        if (SearchAroundReturnToCenter)
                            MoveStage(baseX, baseY, bFineSpeed);

                        return true;
                    }
                }
            }

            Log.Write(UnitName, "SearchAroundCenter", "Fail: no mark found around center");
            return false;
        }
        #endregion
        #region Diagnostics / Recheck
        public int RecheckDieAndAlign(bool bFineSpeed = false)
        {
            int nRet = 0;
            List<PointD> chips = new List<PointD>();
            Task<int> tImageProcess = null;
            try
            {
                if (this.IsStop)
                {
                    Log.Write(UnitName, "RecheckDieAndAlign", "IsStop");
                    return 0;
                }

                var equipment = Equipment.Instance;
                bool IsDryRunEqp = equipment.EquipmentConfig.IsDryRun;
                if (this.Config.IsSimulation == false && (this.Config.IsDryRun == false && IsDryRunEqp == false))
                {
                    Log.Write(UnitName, "RecheckDieAndAlign", "Start");
                    double dpoX = AxisX.GetPosition();
                    double dpoY = AxisY.GetPosition();

                    if (tImageProcess != null)
                    {
                        tImageProcess.Wait();
                    }

                    double dx = dpoX;
                    double dy = dpoY;
                    StageCamera.SuspendedImageDisplay = true;
                    StageCamera.GrabSync(out VisionImage grabImage);
                    //grabImage.Save(VisionImage.FileFilter.bmp);
                    tImageProcess = Task.Factory.StartNew(() =>
                    {
                        Log.Write(UnitName, "RecheckDieAndAlign", "SearchDies");
                        return SearchDies(grabImage, ref chips, dx, dy);
                    });
                    tImageProcess.Wait();

                    var wafer = GetMaterialWafer();
                    // 병합 임계값 클램프
                    double tol = DuplicateDistMm;
                    double pitchMin = double.MaxValue;
                    if (ChipPitchXmm > 0) pitchMin = Math.Min(pitchMin, ChipPitchXmm);
                    if (ChipPitchYmm > 0) pitchMin = Math.Min(pitchMin, ChipPitchYmm);


                    wafer.UpdateChipInfo(chips, this.ChipPitchXmm, this.ChipPitchYmm);
                    Log.Write(UnitName, "RecheckDieAndAlign", "End");
                }
                if (nRet != 0)
                {
                    Log.Write(UnitName, "ChipMap", "Fail: GrabAndMap");
                    return -1;
                }

                StageCamera.SuspendedImageDisplay = false;
                return nRet;
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                return -1;
            }
            finally
            {

            }
        }
        #endregion

        #region Reset / UI Helpers
        public void ResetForNewRun(bool moveToSafeReady = true, bool clearOffsets = true, bool clearStageMaterial = true)
        {
            int nRtn = 0;
            // 1) 얼라인/검출 상태 초기화
            IsStatus_TAlignPrepared = false;
            IsStatus_TAlignDone = false;
            IsStatus_LastFoundTRawAngle = 0;
            IsStatus_LastAppliedTAngle = 0;

            IsStatus_XYAlignPrepared = false;
            IsStatus_XYAlignDone = false;
            IsStatus_LastFoundDx = 0;
            IsStatus_LastFoundDy = 0;

            _lastCenterAlignTp = null;

            // 2) 매핑 상태 초기화
            ResetChipMappingState(); // ChipMappingDone=false, _chipPickupCursor=0, CurrentChipMap=null

            // 3) 시퀀스/요청 플래그 초기화
            RequestOutputDie = false;
            IsStatus_RequestWafer = false;
            this.CurrentFunc = null;

            // 4) 비전 러너(선택) 재초기화 트리거
            _runnerInitTried = false;

            // 5) 스테이지 보유 머티리얼(선택)
            if (clearStageMaterial)
            {
                try
                {
                    this.SetMaterial(null);
                }
                catch (Exception ex)
                {
                    Log.Write(ex);
                }
            }

            // 7) 안전 IO/Ready 복귀(선택)
            if (moveToSafeReady)
            {
                try
                {
                    // 안전한 기본 상태로 복귀
                    // 순서: 클램프 후퇴 → 리프트 다운 → 플레이트 다운 → Ready 위치 복귀
                    nRtn = ClampBackward();
                    if (nRtn != 0)
                    {
                        Log.Write(UnitName, "ResetForNewRun", "ClampBackward");
                        return;
                    }
                    nRtn = ClampLiftDown();
                    if (nRtn != 0)
                    {
                        Log.Write(UnitName, "ResetForNewRun", "ClampBackward");
                        return;
                    }
                    nRtn = PlateDown();
                    if (nRtn != 0)
                    {
                        Log.Write(UnitName, "ResetForNewRun", "ClampBackward");
                        return;
                    }

                    // 인터락을 통과할 수 있는 경우에만 Ready 복귀
                    //MoveToStageReadyPosition();
                }
                catch (Exception ex)
                {
                    Log.Write(this, $"ResetForNewRun MoveToSafeReady failed: {ex.Message}");
                }
            }
            UpdateUI();
        }
        #endregion


        

        // InputStage class 내부에 추가 (ApplyOuterBorderSkipOrReject 위/아래 아무 곳)
        private static double MedianOf(List<double> values)
        {
            if (values == null || values.Count == 0)
                return 0.0;

            values.Sort();
            int n = values.Count;
            int mid = n / 2;

            if ((n & 1) == 1)
                return values[mid];

            return (values[mid - 1] + values[mid]) * 0.5;
        }

        // [ADD] 외곽 N줄(테두리 N셀) 스킵. 0이면 미사용
        public int OuterBorderSkipRows { get; set; } = 0;

        // [ADD] 외곽 제거 형상 설정 (Recipe 등에서 연결 필요)
        public OuterRemovalShape OuterBorderSkipShape { get; set; } = OuterRemovalShape.Ellipse;

        /// <summary>
        /// [MODIFIED] 외곽 N줄(테두리 N셀)을 아예 리스트에서 삭제합니다.
        /// 형상(Shape) 옵션에 따라 원형, 사각형, 비정형(반원 등) 제거 로직을 분기합니다.
        /// </summary>
        private void RemoveOuterBorderDies(MaterialWafer wafer, int borderRows, OuterRemovalShape shape = OuterRemovalShape.Ellipse)
        {
            if (wafer?.Dies == null || wafer.Dies.Count == 0)
                return;

            if (borderRows <= 0)
                return;

            lock (wafer.Dies)
            {
                // 1. 유효한 다이들만 대상 (존재하고 맵핑된 다이)
                var validDies = wafer.Dies
                    .Where(d => d != null &&
                                d.Presence == MaterialPresence.Exist &&
                                d.State == DieProcessState.Mapped)
                    .ToList();

                if (validDies.Count == 0)
                    return;

                int removedCount = 0;

                // 2. 형상별 로직 분기
                if (shape == OuterRemovalShape.Morphology)
                {
                    // [반원/비정형 대응] 형태학적 침식 (Peeling)
                    // 외곽 테두리를 한 겹씩 borderRows 횟수만큼 벗겨냄
                    removedCount = RemoveDiesByMorphologyPeeling(wafer, validDies, borderRows);
                }
                else
                {
                    // [원형/사각형] Bounding Box 기반 기하학적 제거
                    removedCount = RemoveDiesByGeometryClip(wafer, validDies, borderRows, shape);
                }

                Log.Write(UnitName, "RemoveOuterBorderDies",
                    $"Removed {removedCount} dies from outer border (rows={borderRows}, shape={shape}). Remaining={wafer.Dies.Count(d => d.Presence == MaterialPresence.Exist)}");

                // 인덱스 재정렬 (중간이 빠졌으므로 인덱스를 다시 0부터 맞춰줍니다)
                NormalizeIndicesSequential(wafer, startIndex: 0, rename: true);
            }
        }

        // [New Helper] 기하학적 도형 기준 제거 (원형/사각형)
        private int RemoveDiesByGeometryClip(MaterialWafer wafer, List<MaterialDie> validDies, int borderRows, OuterRemovalShape shape)
        {
            // 그리드 Bounding Box 계산
            int minX = validDies.Min(d => (int)d.MapX);
            int maxX = validDies.Max(d => (int)d.MapX);
            int minY = validDies.Min(d => (int)d.MapY);
            int maxY = validDies.Max(d => (int)d.MapY);

            // 중심점
            double centerMapX = (minX + maxX) * 0.5;
            double centerMapY = (minY + maxY) * 0.5;

            // 외곽 반경 (Grid 기준)
            double aOuter = (maxX - minX) * 0.5;
            double bOuter = (maxY - minY) * 0.5;

            if (aOuter <= 0 || bOuter <= 0) return 0;

            // 살려둘 내부 반경/범위 계산
            double aInner = aOuter - borderRows;
            double bInner = bOuter - borderRows;

            if (aInner <= 0 || bInner <= 0) return 0;

            // 삭제 수행
            return wafer.Dies.RemoveAll(d =>
            {
                if (d == null) return true;
                if (d.Presence != MaterialPresence.Exist || d.State != DieProcessState.Mapped)
                    return false;

                if (shape == OuterRemovalShape.Rectangle)
                {
                    // 사각형: Inner Box 밖이면 삭제
                    double innerMinX = centerMapX - aInner;
                    double innerMaxX = centerMapX + aInner;
                    double innerMinY = centerMapY - bInner;
                    double innerMaxY = centerMapY + bInner;

                    // 범위 밖인지 체크 (경계 포함 여부는 정책 나름, 여기서는 >로 엄격하게 자름)
                    // MapX/MapY는 int지만 double 범위와 비교
                    if (d.MapX < innerMinX - 0.001 || d.MapX > innerMaxX + 0.001 ||
                        d.MapY < innerMinY - 0.001 || d.MapY > innerMaxY + 0.001)
                        return true;
                }
                else // Ellipse (Default)
                {
                    // 타원/원: 타원 방정식 (nx^2 + ny^2 > 1 이면 삭제)
                    double nx = (d.MapX - centerMapX) / aInner;
                    double ny = (d.MapY - centerMapY) / bInner;
                    if ((nx * nx) + (ny * ny) > 1.0001)
                        return true;
                }

                return false;
            });
        }

        // [New Helper] 형태학적 침식 (Morphological Peeling) - 반원, 비정형 대응용
        private int RemoveDiesByMorphologyPeeling(MaterialWafer wafer, List<MaterialDie> currentDies, int peelCount)
        {
            int totalRemoved = 0;

            // 현재 유효한 다이 리스트를 로컬 복사본으로 시작
            var activeSet = new HashSet<MaterialDie>(currentDies);

            // 좌표 Lookup (매 루프마다 갱신 필요)
            var mapLookup = new HashSet<(int x, int y)>();

            // 4방향 이웃 오프셋 (상하좌우가 비어있으면 외곽으로 판단)
            int[] dx = { 0, 0, -1, 1 };
            int[] dy = { -1, 1, 0, 0 };

            for (int k = 0; k < peelCount; k++)
            {
                if (activeSet.Count == 0) break;

                // 1. 현재 상태의 좌표 맵 빌드
                mapLookup.Clear();
                foreach (var d in activeSet)
                    mapLookup.Add(((int)d.MapX, (int)d.MapY));

                // 2. 삭제 대상 식별 (자신은 있는데, 4방향 중 하나라도 없으면 '테두리')
                var toRemove = new List<MaterialDie>();
                foreach (var d in activeSet)
                {
                    int mx = (int)d.MapX;
                    int my = (int)d.MapY;
                    bool isBorder = false;

                    for (int i = 0; i < 4; i++)
                    {
                        if (!mapLookup.Contains((mx + dx[i], my + dy[i])))
                        {
                            isBorder = true;
                            break;
                        }
                    }

                    if (isBorder)
                        toRemove.Add(d);
                }

                if (toRemove.Count == 0) break;

                // 3. 리스트에서 제거
                foreach (var d in toRemove)
                {
                    activeSet.Remove(d);
                    // 실제 Wafer 리스트에서도 제거
                    wafer.Dies.Remove(d);
                }

                totalRemoved += toRemove.Count;
            }

            return totalRemoved;
        }

        /// <summary>
        /// [ADD] 외곽 N줄(테두리 N셀)을 아예 리스트에서 삭제합니다.
        /// 맵 매칭 시 불필요한 외곽 다이를 제거하여 1:1 매칭 확률을 높일 때 사용합니다.
        /// </summary>
        //private void RemoveOuterBorderDies(MaterialWafer wafer, int borderRows)
        //{
        //    if (wafer?.Dies == null || wafer.Dies.Count == 0) 
        //        return;

        //    if (borderRows <= 0) 
        //        return;

        //    lock (wafer.Dies)
        //    {
        //        // 1. 유효한 다이들만 대상으로 그리드 범위 계산 (기존 로직과 동일)
        //        var validDies = wafer.Dies
        //            .Where(d => d != null &&
        //                        d.Presence == MaterialPresence.Exist &&
        //                        d.State == DieProcessState.Mapped)
        //            .ToList();

        //        if (validDies.Count == 0) 
        //            return;

        //        // 2. 그리드 Bounding Box 계산
        //        int minX = validDies.Min(d => (int)d.MapX);
        //        int maxX = validDies.Max(d => (int)d.MapX);
        //        int minY = validDies.Min(d => (int)d.MapY);
        //        int maxY = validDies.Max(d => (int)d.MapY);

        //        // 중심점
        //        double centerMapX = (minX + maxX) * 0.5;
        //        double centerMapY = (minY + maxY) * 0.5;

        //        // 외곽 반경 (Grid 기준)
        //        double aOuter = (maxX - minX) * 0.5;
        //        double bOuter = (maxY - minY) * 0.5;

        //        // 방어 코드: 맵이 너무 작을 경우
        //        if (aOuter <= 0 || bOuter <= 0)
        //        {
        //            Log.Write(UnitName, nameof(RemoveOuterBorderDies),
        //                $"Removal ignored: invalid grid size. rangeX=({minX}~{maxX}), rangeY=({minY}~{maxY})");
        //            return;
        //        }

        //        // 3. 살려둘 내부 타원 반경 계산 (borderRows 만큼 축소)
        //        double aInner = aOuter - borderRows;
        //        double bInner = bOuter - borderRows;

        //        // 만약 깎아내고 남는게 없다면 중단 (다 지울 순 없으므로)
        //        if (aInner <= 0 || bInner <= 0)
        //        {
        //            Log.Write(UnitName, nameof(RemoveOuterBorderDies),
        //                $"Removal ignored: inner radius <= 0. outer=({aOuter:F3},{bOuter:F3}) borderRows={borderRows}");
        //            return;
        //        }

        //        // 4. 리스트에서 조건에 맞지 않는 다이(외곽 다이) 삭제 수행
        //        // RemoveAll은 조건이 true인 요소를 삭제합니다.
        //        int removedCount = wafer.Dies.RemoveAll(d =>
        //        {
        //            if (d == null) return true; // null 객체는 삭제

        //            // 존재하지 않거나 맵핑 상태가 아닌 것은 건드리지 않음(혹은 정책에 따라 삭제)
        //            // 여기서는 '존재하는 맵핑 다이' 중에서 외곽인 것만 삭제 대상으로 봅니다.
        //            if (d.Presence != MaterialPresence.Exist || d.State != DieProcessState.Mapped)
        //                return false;

        //            // 좌표 정규화 (-1.0 ~ +1.0)
        //            double nx = (d.MapX - centerMapX) / aInner;
        //            double ny = (d.MapY - centerMapY) / bInner;

        //            // 타원 방정식: x^2 + y^2 > 1 이면 타원 밖임 -> 삭제 대상(true)
        //            double distSq = (nx * nx) + (ny * ny);

        //            // 1.0 보다 크면(약간의 오차 허용 1.0001) 외곽임
        //            return distSq > 1.0001;
        //        });

        //        Log.Write(UnitName, nameof(RemoveOuterBorderDies),
        //            $"Removed {removedCount} dies from outer border (rows={borderRows}). Remaining={wafer.Dies.Count}");

        //        // 5. 인덱스 재정렬 (중간이 빠졌으므로 인덱스를 다시 0부터 맞춰줍니다)
        //        NormalizeIndicesSequential(wafer, startIndex: 0, rename: true);
        //    }
        //}


        // [ADD] 외곽 N줄(테두리 N셀) 스킵 - Circle/Ellipse 기반 버전
        private void ApplyOuterBorderSkipOrReject(MaterialWafer wafer, int borderRows)
        {
            if (wafer?.Dies == null || wafer.Dies.Count == 0) return;
            if (borderRows <= 0) return;

            lock (wafer.Dies)
            {
                // 기존 공정 조건 유지
                var dies = wafer.Dies
                    .Where(d => d != null &&
                                d.Presence == MaterialPresence.Exist &&
                                d.State == DieProcessState.Mapped)
                    .ToList();

                if (dies.Count == 0) return;

                // 1) 그리드 Bounding Box 기반 중심(치우침 방지)
                int minX = dies.Min(d => (int)d.MapX);
                int maxX = dies.Max(d => (int)d.MapX);
                int minY = dies.Min(d => (int)d.MapY);
                int maxY = dies.Max(d => (int)d.MapY);

                // center: 그리드의 "정중앙"(반셀도 허용)
                double centerMapX = (minX + maxX) * 0.5;
                double centerMapY = (minY + maxY) * 0.5;

                // 2) 외곽 타원/원 파라미터(칩 단위)
                // 반경 a,b는 "그리드 반경"으로 고정 (데이터 분포로 흔들리지 않음)
                double aOuter = (maxX - minX) * 0.5;
                double bOuter = (maxY - minY) * 0.5;

                // 너무 작은 맵 방어
                if (aOuter <= 0 || bOuter <= 0)
                {
                    Log.Write(UnitName, nameof(ApplyOuterBorderSkipOrReject),
                        $"Skip ignored: invalid grid size. rangeX=({minX}~{maxX}), rangeY=({minY}~{maxY})");
                    return;
                }

                // 3) borderRows 만큼 안쪽 타원(생존 영역)
                // borderRows를 축 방향으로 동일하게 감소시키면 "타원 테두리 N줄" 느낌이 가장 균일함
                double aInner = aOuter - borderRows;
                double bInner = bOuter - borderRows;

                if (aInner <= 0 || bInner <= 0)
                {
                    Log.Write(UnitName, nameof(ApplyOuterBorderSkipOrReject),
                        $"Skip ignored: inner radius <= 0. outer=({aOuter:F3},{bOuter:F3}) borderRows={borderRows}");
                    return;
                }

                // 4) 모드 선택: 원/타원
                // - 원으로 하고 싶으면 a=b=min(aInner,bInner)
                // - 타원으로 하고 싶으면 a=aInner, b=bInner
                bool useCircle = false; // 원이면 true, 타원이면 false (원하는 기본값으로 바꾸면 됨)

                double a = useCircle ? Math.Min(aInner, bInner) : aInner;
                double b = useCircle ? Math.Min(aInner, bInner) : bInner;

                double invA2 = 1.0 / (a * a);
                double invB2 = 1.0 / (b * b);

                // 5) 판정: inner 타원 밖이면 Skip
                int skipped = 0;
                foreach (var d in dies)
                {
                    double dx = d.MapX - centerMapX;
                    double dy = d.MapY - centerMapY;

                    // normalized radius^2
                    double n = (dx * dx) * invA2 + (dy * dy) * invB2;

                    // n <= 1 : inner 타원 안쪽(살림)
                    // n >  1 : inner 타원 밖(외곽) => Skip
                    if (n > 1.0)
                    {
                        d.SetSkip($"OuterBorderSkip(Ellipse) rows={borderRows}");
                        skipped++;
                    }
                }

                Log.Write(UnitName, nameof(ApplyOuterBorderSkipOrReject),
                    $"Applied outer border skip by {(useCircle ? "Circle" : "Ellipse")}. borderRows={borderRows}, " +
                    $"centerMap=({centerMapX:F2},{centerMapY:F2}), outer=({aOuter:F3},{bOuter:F3}), inner=({aInner:F3},{bInner:F3}), " +
                    $"skipped={skipped}, total={dies.Count}");
            }
        }

        // median helper (int list)
        private static double MedianOfInt(IList<int> sorted)
        {
            if (sorted == null || sorted.Count == 0) return 0;
            int n = sorted.Count;
            if (n % 2 == 1) return sorted[n / 2];
            return (sorted[n / 2 - 1] + sorted[n / 2]) / 2.0;
        }

        /// <summary>
        /// 프로젝트 상황에 맞게 “CenterPoint 기준 오프셋”을 반환.
        /// 지금 코드만 보면 AlignXY가 실제로 dx/dy를 계산/적용하지 않아서 0을 반환하게 두고,
        /// 나중에 AlignXY 구현되면 여기만 고치면 됨.
        /// </summary>
        private bool TryGetAlignedCenterOffsetMm(out double dx, out double dy)
        {
            dx = 0;
            dy = 0;
            return true;


            // 후보1) 상태 변수 (현재 클래스에 있음)
            // AlignXY가 실제로 dx/dy 찾게 되면 이 값을 쓰면 됨
            if (IsStatus_XYAlignDone)
            {
                dx = IsStatus_LastFoundDx;
                dy = IsStatus_LastFoundDy;

                // 값이 0인 경우가 많을 수 있으니, 의미있는 값일 때만 true 처리하려면 조건 추가
                if (Math.Abs(dx) > 1e-9 || Math.Abs(dy) > 1e-9)
                    return true;
            }

            return false;
        }


        public int RestoreSkippedDiesForRepick(Func<MaterialDie, bool> selector = null)
        {
            var wafer = GetMaterialWafer();
            if (wafer?.Dies == null) return -1;

            lock (wafer.Dies)
            {
                foreach (var d in wafer.Dies)
                {
                    if (d == null) continue;
                    if (d.State != DieProcessState.Skip) continue;

                    if (selector != null && !selector(d))
                        continue;

                    d.SkipReason = "";
                    d.State = DieProcessState.Mapped; // 다시 픽업 후보로 복귀
                }
            }
            return 0;
        }

        // [ADD] Skip 된 Die를 wafer.Dies 리스트에서 제거 (복구 불가)
        public int RemoveSkippedDiesFromWafer(string reason = "RemoveSkippedDiesFromWafer")
        {
            var wafer = GetMaterialWafer();
            if (wafer?.Dies == null) return -1;

            lock (wafer.Dies)
            {
                int before = wafer.Dies.Count;

                // Skip 상태거나 SkipReason이 있는 것도 같이 제거하고 싶으면 조건 추가 가능
                wafer.Dies.RemoveAll(d =>
                    d != null &&
                    d.Presence == MaterialPresence.Exist &&
                    d.State == DieProcessState.Skip);

                int after = wafer.Dies.Count;
                int removed = before - after;

                Log.Write(UnitName, reason, $"Removed Skip dies. before={before}, after={after}, removed={removed}");
            }

            // UI 갱신
            try { EventUpdateUIWafer?.BeginInvoke(wafer, null, null); } catch { }
            return 0;
        }



        private void ApplyOuterBorderSkipOrReject_Rect(MaterialWafer wafer, int borderRows)
        {
            if (wafer?.Dies == null || wafer.Dies.Count == 0)
                return;

            if (borderRows <= 0)
                return;

            // MapX/MapY의 min/max를 기준으로 "테두리 borderRows 셀"을 외곽으로 간주
            int minX, maxX, minY, maxY;
            lock (wafer.Dies)
            {
                var dies = wafer.Dies.Where(d => d != null).ToList();
                if (dies.Count == 0)
                    return;

                minX = dies.Min(d => (int)d.MapX);
                maxX = dies.Max(d => (int)d.MapX);
                minY = dies.Min(d => (int)d.MapY);
                maxY = dies.Max(d => (int)d.MapY);

                // 전체 크기보다 border가 크면 전부 날아가는 사고 방지
                int width = (maxX - minX + 1);
                int height = (maxY - minY + 1);
                if (width <= borderRows * 2 || height <= borderRows * 2)
                {
                    Log.Write(UnitName, nameof(ApplyOuterBorderSkipOrReject_Rect),
                        $"Skip ignored: map size too small. size=({width},{height}) border={borderRows}");
                    return;
                }

                int innerMinX = minX + borderRows;
                int innerMaxX = maxX - borderRows;
                int innerMinY = minY + borderRows;
                int innerMaxY = maxY - borderRows;

                int skiped = 0;
                foreach (var d in dies)
                {
                    if (d.Presence != MaterialPresence.Exist)
                        continue;

                    // 아직 픽업 후보(Mapped)만 스킵 처리
                    if (d.State != DieProcessState.Mapped)
                        continue;

                    int mx = (int)d.MapX;
                    int my = (int)d.MapY;

                    bool isOuter =
                        (mx < innerMinX) || (mx > innerMaxX) ||
                        (my < innerMinY) || (my > innerMaxY);

                    if (!isOuter)
                        continue;

                    d.SetSkip($"OuterBorderSkipCircle({borderRows})");
                    skiped++;
                }

                Log.Write(UnitName, nameof(ApplyOuterBorderSkipOrReject_Rect),
                    $"Applied outer border skip. border={borderRows} Skip={skiped} mapRangeX=({minX}~{maxX}) mapRangeY=({minY}~{maxY})");
            }
        }

        private bool TryLoadRawChipsFromFile(string filePath, out List<PointD> chips, int maxCount = 20000)
        {
            chips = new List<PointD>();
            try
            {
                if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
                    return false;

                foreach (var line in File.ReadLines(filePath))
                {
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    // 헤더/로그 prefix 제거(혹시 "2026-...:rawList>" 같은 prefix가 있으면)
                    var s = line;
                    int idx = s.IndexOf('>');
                    if (idx >= 0) s = s.Substring(idx + 1).Trim();

                    // 헤더 스킵
                    if (s.StartsWith("Index", StringComparison.OrdinalIgnoreCase))
                        continue;

                    var parts = s.Split(',');
                    if (parts.Length < 3)
                        continue;

                    // parts[0] = index (무시 가능)
                    if (!double.TryParse(parts[1].Trim(), System.Globalization.NumberStyles.Float,
                                         System.Globalization.CultureInfo.InvariantCulture, out var x))
                        continue;
                    if (!double.TryParse(parts[2].Trim(), System.Globalization.NumberStyles.Float,
                                         System.Globalization.CultureInfo.InvariantCulture, out var y))
                        continue;

                    chips.Add(new PointD(x, y));

                    if (maxCount > 0 && chips.Count >= maxCount)
                        break;
                }

                return chips.Count > 0;
            }
            catch (Exception ex)
            {
                Log.Write(UnitName, "TryLoadRawChipsFromFile", ex.Message);
                chips = new List<PointD>();
                return false;
            }
        }

        
        public bool SelectSimRawChipFileWithDialog(IWin32Window owner = null)
        {
            try
            {
                using (var dlg = new OpenFileDialog())
                {
                    dlg.Title = "Select Sim Raw Chip File (CSV/TXT)";
                    dlg.Filter = "CSV (*.csv)|*.csv|Text (*.txt)|*.txt|All files (*.*)|*.*";
                    dlg.Multiselect = false;
                    dlg.CheckFileExists = true;
                    dlg.RestoreDirectory = true;

                    // 기본 폴더(원하면 Log/MapFile 폴더로)
                    dlg.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory;

                    var dr = (owner == null) ? dlg.ShowDialog() : dlg.ShowDialog(owner);
                    if (dr != DialogResult.OK)
                        return false;

                    SimRawChipFilePath = dlg.FileName;
                    Log.Write(UnitName, "Sim", $"Selected SimRawChipFilePath='{SimRawChipFilePath}'");
                    return true;
                }
            }
            catch (Exception ex)
            {
                Log.Write(UnitName, "SelectSimRawChipFileWithDialog", ex.Message);
                return false;
            }
        }

        // InputStage.cs
        public Task<string> AskSimRawChipFilePathAsync(IWin32Window owner = null)
        {
            var tcs = new TaskCompletionSource<string>();

            // UI 스레드로 올릴 대상 Control(폼/메인컨트롤)이 필요합니다.
            // 보통 Equipment.Instance.MainForm 또는 어떤 UI Control 참조를 갖고 있어야 합니다.
            var ui = Application.OpenForms.Count > 0 ? Application.OpenForms[0] : null;
            if (ui == null)
            {
                tcs.SetResult(null);
                return tcs.Task;
            }

            ui.BeginInvoke((Action)(() =>
            {
                try
                {
                    using (var dlg = new OpenFileDialog())
                    {
                        dlg.Title = "Sim Raw Chip File 선택";
                        dlg.Filter = "LOG (*.log)|*.log|Text (*.txt)|*.txt|All Files (*.*)|*.*";
                        dlg.Multiselect = false;
                        dlg.CheckFileExists = true;

                        var dr = (owner != null) ? dlg.ShowDialog(owner) : dlg.ShowDialog(ui);
                        if (dr == DialogResult.OK)
                            tcs.SetResult(dlg.FileName);
                        else
                            tcs.SetResult(null);
                    }
                }
                catch (Exception ex)
                {
                    Log.Write(UnitName, "AskSimRawChipFilePathAsync", ex.ToString());
                    tcs.SetResult(null);
                }
            }));

            return tcs.Task;
        }

        private void ReadDieMapLogFile(MaterialWafer wafer)
        {
            if (wafer == null) return;

            string selectedFilePath = string.Empty;

            // 1. Open File Dialog to select file manually
            // Use Invoke if running from a non-UI thread to ensure the dialog shows up correctly
            if (Application.OpenForms.Count > 0)
            {
                var mainForm = Application.OpenForms[0];
                if (mainForm.InvokeRequired)
                {
                    mainForm.Invoke(new Action(() =>
                    {
                        using (OpenFileDialog dlg = new OpenFileDialog())
                        {
                            //dlg.InitialDirectory = Log.LogPath; // Default to log path if available
                            dlg.Filter = "Log Files (*.log)|*.log|All Files (*.*)|*.*";
                            dlg.Title = "Select Die Map Log File";
                            if (dlg.ShowDialog() == DialogResult.OK)
                            {
                                selectedFilePath = dlg.FileName;
                            }
                        }
                    }));
                }
                else
                {
                    using (OpenFileDialog dlg = new OpenFileDialog())
                    {
                        //dlg.InitialDirectory = Log.LogPath;
                        dlg.Filter = "Log Files (*.log)|*.log|All Files (*.*)|*.*";
                        dlg.Title = "Select Die Map Log File";
                        if (dlg.ShowDialog() == DialogResult.OK)
                        {
                            selectedFilePath = dlg.FileName;
                        }
                    }
                }
            }

            if (string.IsNullOrEmpty(selectedFilePath) || !File.Exists(selectedFilePath))
            {
                Log.Write(UnitName, "ReadDieMapLogFile", "No file selected or file does not exist.");
                return;
            }

            Log.Write(UnitName, "ReadDieMapLogFile", $"Reading from user-selected file: {Path.GetFileName(selectedFilePath)}");

            // 2. Clear existing dies
            lock (wafer.Dies)
            {
                wafer.Dies.Clear();
            }

            // 3. Parse the file
            try
            {
                var lines = File.ReadAllLines(selectedFilePath);
                var newDies = new List<MaterialDie>();

                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    // Filter lines containing "DieMap>"
                    if (!line.Contains("DieMap>")) continue;
                    // Skip header line
                    if (line.Contains("Index,MapX,MapY")) continue;

                    // Split by "DieMap>" to get the data part
                    var parts = line.Split(new string[] { "DieMap>" }, StringSplitOptions.None);
                    if (parts.Length < 2) continue;

                    // Data part: "0 ,-101, -43, 170.423, 106.809"
                    string dataPart = parts[1].Trim();
                    var values = dataPart.Split(',');

                    if (values.Length >= 5)
                    {
                        if (int.TryParse(values[0].Trim(), out int index) &&
                            int.TryParse(values[1].Trim(), out int mapX) &&
                            int.TryParse(values[2].Trim(), out int mapY) &&
                            double.TryParse(values[3].Trim(), out double centerX) &&
                            double.TryParse(values[4].Trim(), out double centerY))
                        {
                            var die = new MaterialDie
                            {
                                Index = index,
                                MapX = mapX,
                                MapY = mapY,
                                CenterX = centerX,
                                CenterY = centerY,
                                Presence = MaterialPresence.Exist,
                                State = DieProcessState.Mapped,
                                SourceWaferId = wafer.WaferId,
                                ArrivedTime = DateTime.Now
                            };
                            newDies.Add(die);
                        }
                    }
                }

                if (newDies.Count > 0)
                {
                    lock (wafer.Dies)
                    {
                        wafer.Dies.AddRange(newDies);
                    }
                    Log.Write(UnitName, "ReadDieMapLogFile", $"Successfully loaded {newDies.Count} dies.");
                }
                else
                {
                    Log.Write(UnitName, "ReadDieMapLogFile", "No valid die data found in file.");
                }
            }
            catch (Exception ex)
            {
                Log.Write(UnitName, "ReadDieMapLogFile", $"Exception: {ex.Message}");
            }
        }
    }
}