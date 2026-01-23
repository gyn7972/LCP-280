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
    public class InputStage : BaseUnit<InputStageConfig>, IPatternMarkSource
    {
        #region Types / Events
        public event EventHandler<PatternMarksFoundEventArgs> MarksFound;

        public delegate void UpdateUIWafer(MaterialWafer wafer);
        public event UpdateUIWafer EventUpdateUIWafer;

        public enum AlarmKeys
        {
            eDieTransferPickZNotSafety = 3001,
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
        public int PollIntervalMs { get; set; } = 5; //30
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
            base.InitAlarm();
            AlarmInfo alarm = new AlarmInfo();
            alarm.Code = (int)AlarmKeys.eDieTransferPickZNotSafety;
            alarm.Title = "Die Tr Z-Axis Not safety Pos.";
            alarm.Cause = "Die TrZAxis이 안전 위치가 아닙니다. 포지션 확인 후 다시 시작 하십시요.";
            alarm.Source = this.UnitName;
            alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
            m_dicAlarms.Add(alarm.Code, alarm);

            alarm = new AlarmInfo();
            alarm.Code = (int)AlarmKeys.eInputFeederCylinderZNotSafety;
            alarm.Title = "Feeder Z-Cylinder Not safety Pos.";
            alarm.Cause = "Feeder Z-Cylinder가 안전 위치가 아닙니다. 포지션 확인 후 다시 시작 하십시요.";
            alarm.Source = this.UnitName;
            alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
            m_dicAlarms.Add(alarm.Code, alarm);

            //,
            alarm = new AlarmInfo();
            alarm.Code = (int)AlarmKeys.eInputStageEjectorPinZNotSafety;
            alarm.Title = "EjectorPin Z-Axis Not safety Pos.";
            alarm.Cause = "EjectorPin Z-Axis가 안전 위치가 아닙니다. 포지션 확인 후 다시 시작 하십시요.";
            alarm.Source = this.UnitName;
            alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
            m_dicAlarms.Add(alarm.Code, alarm);
            //,
            alarm = new AlarmInfo();
            alarm.Code = (int)AlarmKeys.eInputStageEjectorZNotSafety;
            alarm.Title = "Ejector Z-Axis Not safety Pos.";
            alarm.Cause = "Ejector Z-Axis가 안전 위치가 아닙니다. 포지션 확인 후 다시 시작 하십시요.";
            alarm.Source = this.UnitName;
            alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
            m_dicAlarms.Add(alarm.Code, alarm);

            //
            alarm = new AlarmInfo();
            alarm.Code = (int)AlarmKeys.eInputFeederYNotSafe;
            alarm.Title = "Feeder Y-Axis Not safety Pos.";
            alarm.Cause = "Feeder Y-Axis가 안전 위치가 아닙니다. 포지션 확인 후 다시 시작 하십시요.";
            alarm.Source = this.UnitName;
            alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
            m_dicAlarms.Add(alarm.Code, alarm);

            alarm = new AlarmInfo();
            alarm.Code = (int)AlarmKeys.eVisionTsearch;
            alarm.Title = "Vision T Search.";
            alarm.Cause = "Vision T Search Fail. Chip Mark 확인 후 다시 시작 하십시요.";
            alarm.Source = this.UnitName;
            alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
            m_dicAlarms.Add(alarm.Code, alarm);
            //
            alarm = new AlarmInfo();
            alarm.Code = (int)AlarmKeys.eVisionXYsearch;
            alarm.Title = "Vision XY Search.";
            alarm.Cause = "Vision XY Search Fail. Chip Mark 확인 후 다시 시작 하십시요.";
            alarm.Source = this.UnitName;
            alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
            m_dicAlarms.Add(alarm.Code, alarm);

            alarm = new AlarmInfo();
            alarm.Code = (int)AlarmKeys.eInputStageMoveFail;
            alarm.Title = "스테이지 이동에 실패 하였습니다.";
            alarm.Cause = "모터상태를 확인 하여주십시요.";
            alarm.Source = this.UnitName;
            alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
            m_dicAlarms.Add(alarm.Code, alarm);

            alarm = new AlarmInfo();
            alarm.Code = (int)AlarmKeys.eRingLockFailed;
            alarm.Title = "스테이지 제품 잠금 실패 하였습니다.";
            alarm.Cause = "스테이지 Lift Lock 실린더 상태를 확인 하여주십시요.";
            alarm.Source = this.UnitName;
            alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
            m_dicAlarms.Add(alarm.Code, alarm);

            alarm = new AlarmInfo();
            alarm.Code = (int)AlarmKeys.eInputStageAlignNotDone;
            alarm.Title = "Input Stage Align Not Done.";
            alarm.Cause = "Input Stage Align 가 완료되지 않았습니다. 다시 시도 하십시요.";
            alarm.Source = this.UnitName;
            alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
            m_dicAlarms.Add(alarm.Code, alarm);

            alarm = new AlarmInfo();
            alarm.Code = (int)AlarmKeys.eInputStageNoWafer;
            alarm.Title = "Input Stage No Wafer.";
            alarm.Cause = "Input Stage에 Wafer가 없습니다. 다시 시도 하십시요.";
            alarm.Source = this.UnitName;
            alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
            m_dicAlarms.Add(alarm.Code, alarm);

            alarm = new AlarmInfo();
            alarm.Code = (int)AlarmKeys.eInputStageAlignNotCompleted;
            alarm.Title = "Input Stage Align Not Completed.";
            alarm.Cause = "Input Stage Align 가 완료되지 않았습니다. 다시 시도 하십시요.";
            alarm.Source = this.UnitName;
            alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
            m_dicAlarms.Add(alarm.Code, alarm);

            alarm = new AlarmInfo();
            alarm.Code = (int)AlarmKeys.eInputStageMapMatch;
            alarm.Title = "Input Stage Map Match Failed.";
            alarm.Cause = "Input Stage Map Match 가 실패하였습니다. 다시 시도 하십시요.";
            alarm.Source = this.UnitName;
            alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
            m_dicAlarms.Add(alarm.Code, alarm);

            alarm = new AlarmInfo();
            alarm.Code = (int)AlarmKeys.eInputStageRingPresent;
            alarm.Title = "Input Stage Ring Present Failed.";
            alarm.Cause = "Input Stage에 제품 감지가 실패하였습니다. 다시 시도 하십시요.";
            alarm.Source = this.UnitName;
            alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
            m_dicAlarms.Add(alarm.Code, alarm);

            alarm = new AlarmInfo();
            alarm.Code = (int)AlarmKeys.eInputStageLiftUp;
            alarm.Title = "Input Stage Lift Up Failed.";
            alarm.Cause = "Input Stage에 Lift Up 실패하였습니다. 다시 시도 하십시요.";
            alarm.Source = this.UnitName;
            alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
            m_dicAlarms.Add(alarm.Code, alarm);

            alarm = new AlarmInfo();
            alarm.Code = (int)AlarmKeys.eInputStageLiftDown;
            alarm.Title = "Input Stage Lift Down Failed.";
            alarm.Cause = "Input Stage에 Lift Down 실패하였습니다. 다시 시도 하십시요.";
            alarm.Source = this.UnitName;
            alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
            m_dicAlarms.Add(alarm.Code, alarm);

            //
            alarm = new AlarmInfo();
            alarm.Code = (int)AlarmKeys.eInputStageClampFWD;
            alarm.Title = "Input Stage Clamp FWD Failed.";
            alarm.Cause = "Input Stage에 Clamp FWD 실패하였습니다. 다시 시도 하십시요.";
            alarm.Source = this.UnitName;
            alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
            m_dicAlarms.Add(alarm.Code, alarm);

            alarm = new AlarmInfo();
            alarm.Code = (int)AlarmKeys.eInputStageClampBWD;
            alarm.Title = "Input Stage Clamp BWD Failed.";
            alarm.Cause = "Input Stage에 Clamp BWD 실패하였습니다. 다시 시도 하십시요.";
            alarm.Source = this.UnitName;
            alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
            m_dicAlarms.Add(alarm.Code, alarm);

            alarm = new AlarmInfo();
            alarm.Code = (int)AlarmKeys.eInputStageScanEmpty;
            alarm.Title = "Input Stage Scan Empty.";
            alarm.Cause = "Chip Mapping Scan 결과가 0개입니다. Vision/Recipe/조명 상태를 확인 후 다시 시도 하십시요.";
            alarm.Source = this.UnitName;
            alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
            m_dicAlarms.Add(alarm.Code, alarm);
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
            return 0;
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
            int nRtn = 0;
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

                if (Config.IsSimulation || Config.IsDryRun)
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
                if (RunMode == UnitRunMode.Auto)
                    IsAuto = true;
                else
                    IsAuto = false;

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
            if (Config.IsSimulation || Config.IsDryRun)
            {
                return true;
            }
            return this.ReadInput(InputStageConfig.IO.VAC_OK_SNS);
        }
        public bool Ring0()
        {
            if (Config.IsSimulation || Config.IsDryRun)
            {
                return true;
            }

            return this.ReadInput(InputStageConfig.IO.RING_CHECK0);
        }
        public bool Ring1()
        {
            if (Config.IsSimulation || Config.IsDryRun)
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
            if (Config.IsSimulation)// || Config.IsDryRun)
            {
                return true;
            }
            return this.ReadInput(InputStageConfig.IO.EXPANDER_UP_SNS);
        }
        public bool IsPlateDown()
        {
            if (Config.IsSimulation)// || Config.IsDryRun)
            {
                return true;
            }
            return this.ReadInput(InputStageConfig.IO.EXPANDER_DOWN_SNS);
        }
        // === Direct Valve Control(강제 구동) ===
        public bool IsVacuumValveOn()
        {
            if (Config.IsSimulation)// || Config.IsDryRun)
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
            if (!issued && !(Config.IsSimulation || Config.IsDryRun))
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
            if (!issued && !(Config.IsSimulation || Config.IsDryRun))
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
            if (!issued && !(Config.IsSimulation || Config.IsDryRun))
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

            if (IsRingPresent() || Config.IsSimulation || Config.IsDryRun)
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

            if (this.Config.IsSimulation || this.Config.IsDryRun)
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
            if (!Config.IsSimulation && !Config.IsDryRun)
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

            int rc = AxisT.MoveAbs(target, RunMode == UnitRunMode.Auto, bFineSpeed);
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

        private int RefineThetaWithDualPointAxis(bool useXAxis, bool bFineSpeed)
        {
            //사이즈를 벗어나서 마크를 못찾으면? 그건 어떻게 하지?
            int maxAttempts = Math.Max(1, MaxTRefineAttempts);
            double toleranceDeg = Math.Max(1e-6, TRefineResidualToleranceDeg);

            //Todo: 
            //step을 wafer 사이즈 기준으로 잡아야함.
            maxAttempts = 5;
            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                //if (!TryAcquireDualPointAngle(useXAxis, out double residualDeg, bFineSpeed, attempt))
                if (!TryAcquireDualPointAngle2(useXAxis, out double residualDeg, bFineSpeed, attempt))
                {
                    Log.Write(UnitName, "ThetaRefine", $"Fail attempt {attempt} (axis={(useXAxis ? "X" : "Y")}): cannot acquire dual points.");
                    return -1;
                }

                if (Math.Abs(residualDeg) <= toleranceDeg)
                {
                    if(attempt == maxAttempts)
                    {
                        Log.Write(UnitName, "ThetaRefine",
                        $"OK axis={(useXAxis ? "X" : "Y")} attempt {attempt}: residual={residualDeg:F5}deg tol={toleranceDeg:F5}deg");
                        return 0;
                    }
                }

                // residual을 0으로 만들기 위한 보정
                // ApplyThetaCorrection 내부에서 AngleMaxApplyDeg 제한/누적적용/리밋체크를 함
                // 여기에서 correction 계산 부호가 가장 중요.
                // 기존 코드 흐름을 유지하되, "residual -> correction"을 1곳에서만 처리하도록 단순화
                double correction = -residualDeg;

                // 기존 시스템이 AngleApplyGain(-1)로 방향 반전을 포함하고 있으면
                // correction에 AngleApplyGain을 곱하는 방식은 중복 반전 위험이 있어.
                // -> ApplyThetaCorrection 안에서만 Gain을 쓰거나, 여기서만 쓰거나, 둘 중 하나로 통일해야 함.
                // 현재 ApplyThetaCorrection은 limited 그대로 target에 더하므로,
                // 여기서 Gain을 포함시키는 기존 방식 유지:
                correction *= AngleApplyGain;

                int rc = ApplyThetaCorrection(useXAxis, correction, RunMode == UnitRunMode.Auto, bFineSpeed);
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
            if (Config.IsSimulation || this.Config.IsDryRun)
            {
                IsStatus_LastAppliedTAngle = 0;
                IsStatus_TAlignDone = true;
                return 0;
            }

            IsStatus_TAlignDone = false;

            try
            {
                PrepareCameraAndRecipeForAlign();

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
            if (this.Config.IsSimulation || this.Config.IsDryRun)
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
            nRet = MoveStage(die.CenterX, die.CenterY, false);
            return nRet;
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
            //List<PointD> consolidated;
            //int rc = PerformHardwareScanAndBuildWaferMap(bFineSpeed, out wafer, out consolidated);
            List<PointD> consolidated;
            int rc = PerformHardwareScanAndBuildWaferMap(bFineSpeed, out wafer);
            if (rc != 0)
                return rc;

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
            
            rc = EvaluateMapMatchAndDecide(wafer);
            if (rc != 0)
            {
                ChipMappingDone = false;
                return rc;
            }

            wafer.ProcessSatate = Material.MaterialProcessSatate.Processing;
            ChipMappingDone = true;
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
            if (Config.IsSimulation || Config.IsDryRun)
                return;

            if (StageCamera != null && StageCamera.IsLiveOn)
            {
                StageCamera.StopLive();
                Thread.Sleep(100);
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
        /// 
        //private int PerformHardwareScanAndBuildWaferMap(bool bFineSpeed, out MaterialWafer wafer, out List<PointD> consolidated)
        private int PerformHardwareScanAndBuildWaferMap(bool bFineSpeed, out MaterialWafer wafer)
        {
            wafer = null;
            //consolidated = null;

            TrySummaryStartScan();
            try
            {
                ApplyDynamicPitchParameters();
                OnWaferOrRecipeChanged();

                MakeScanPath(out List<PointD> path);

                List<PointD> raw = new List<PointD>();

                int rc = ScanPathAndCollectRawChips(path, bFineSpeed, raw);
                if (rc != 0)
                    return rc;

                //consolidated = ConsolidateRawChips(raw);
                //UpdateChipInfo(consolidated);
                //TrySummaryUpdateScanAndTotalCount(consolidated);

				raw = ConsolidateRawChips(raw);
                //raw = ConsolidateRawChipsGridAware(raw);    // 그리드 인지 중복 제거

                UpdateChipInfo(raw);
                TrySummaryUpdateScanAndTotalCount(raw);

                wafer = GetMaterialWafer();
                if (wafer != null)
                {
                    ApplyAndNormalizeDieOrder(wafer);

                    // [ADD] Scan 완료 시 OK Die -> Rank=1
                    ApplyOkRankToDies(wafer, 1);
                    MarkNonRank1DiesAsSkippedOrNg(wafer, 1);

                    EventUpdateUIWafer?.BeginInvoke(wafer, null, null);
                }

                return 0;
            }
            finally
            {
                TrySummaryStopScan();
            }
        }
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

                    if (Config.IsSimulation || Config.IsDryRun)
                    {
                        EnsureSimDiePoolGenerated();

                        const int maxSimChips = 20000;
                        rawOut.Clear();
                        if (maxSimChips > 0 && _simAllDiesPool.Count > maxSimChips)
                            rawOut.AddRange(_simAllDiesPool.Take(maxSimChips));
                        else
                            rawOut.AddRange(_simAllDiesPool);

                        Log.Write(UnitName, "Sim",
                            $"[ChipMap] Use full sim pool. chips={rawOut.Count} (pool={_simAllDiesPool.Count})");

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
                    return AskContinueWhenMapMatchDisabled();
                }

                // MapMatchMode = true
                string mapFile = PrepareMapFileForMatchingOrAlarm(wafer);
                if (string.IsNullOrWhiteSpace(mapFile))
                    return -1;

                if (Config.IsSimulation || Config.IsDryRun)
                {
                    Log.Write(UnitName, "MapMatch", "Simulation/DryRun -> skip file-based map matching.");
                    return 0;
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
        private int AskContinueWhenMapMatchDisabled()
        {
            // 기존: 맵핑 완료 후 사용자 확인
            if (Config.IsSimulation == false)
            {
                var ask = new MessageBoxYesNo();
                ask.TopMost = true;
                if (ask.ShowDialog("진행 유무 확인", "맵핑 완료. 진행 하시겠습니까?") != DialogResult.Yes)
                {
                    OnStop();
                    ChipMappingDone = false;

                    var eq = Equipment.Instance;
                    eq.SequenceStopAllAsync(CancellationToken.None);

                    Log.Write(UnitName, "TryShutdownIfAllCassettesEmpty", "모든 관련 Unit 정지 완료.");
                    return -1;
                }
            }
            return 0;
        }
        private string PrepareMapFileForMatchingOrAlarm(MaterialWafer wafer)
        {
            var recipe = Equipment.Instance.EquipmentRecipe.CurrentRecipe;
            string mapFilePath = recipe.MapFilePath;
            return PrepareLocalMapFileOrAlarm(wafer, mapFilePath);
        }
        private int RunMapMatchAndDecide(MaterialWafer wafer, string mapFile)
        {
            var recipe = Equipment.Instance.EquipmentRecipe.CurrentRecipe;

            var orgPreview = wafer.ReadFileOnline(mapFile, MaterialWafer.MapTyp.waf);
            if (orgPreview == null || orgPreview.Count == 0)
            {
                PostAlarm((int)AlarmKeys.eInputStageAlignNotCompleted);
                Log.Write(UnitName, "MapMatch", $"Original map parse failed or empty: {mapFile}");
                return -1;
            }

            TrySummaryUpdateOrgAndScanCount(orgPreview.Count, wafer.Dies != null ? wafer.Dies.Count : 0);

            double bestScore = wafer.Mapmatch(mapFile, MaterialWafer.MapTyp.waf) * 100.0;
            Log.Write(UnitName, "MapMatch",
                $"Done. Score={bestScore:F3} OrgCount={orgPreview.Count} ScanCount={wafer.Dies.Count} MapFile='{mapFile}'");

            double scoreThreshold = recipe.WaferMatchLimitPercent;

            int rc = DecideWithManualRematchLoop(wafer, mapFile, ref bestScore, scoreThreshold);
            if (rc != 0)
                return rc;

            // [ADD] MapMatch OK 확정 후에도 OK Die -> Rank=1 재적용(수동 변환/재매칭 후에도 일관성 보장)
            ApplyOkRankToDies(wafer, 1);
            MarkNonRank1DiesAsSkippedOrNg(wafer, 1);

            EventUpdateUIWafer?.BeginInvoke(wafer, null, null);
            return 0;
        }
        private int DecideWithManualRematchLoop(MaterialWafer wafer, string mapFile, ref double bestScore, double scoreThreshold)
        {
            while (true)
            {
                if (bestScore >= scoreThreshold)
                    return 0;

                using (var dlg = new MapMatchDecisionDialog(bestScore, scoreThreshold, mapFile))
                {
                    var dr = dlg.ShowDialog();
                    if (dr != DialogResult.Yes)
                    {
                        PostAlarm((int)AlarmKeys.eInputStageAlignNotCompleted);
                        Log.Write(UnitName, "MapMatch",
                            $"User chose STOP. Score={bestScore:F2} < Threshold={scoreThreshold:F2}. Sequence aborted.");
                        return -1;
                    }

                    if (dlg.ManualSettings != null)
                    {
                        bool applied = ApplyManualMapMatchToWafer(wafer, dlg.ManualSettings);
                        if (!applied)
                        {
                            PostAlarm((int)AlarmKeys.eInputStageAlignNotCompleted);
                            Log.Write(UnitName, "MapMatch", "Manual settings apply failed -> abort");
                            return -1;
                        }

                        EventUpdateUIWafer?.BeginInvoke(wafer, null, null);

                        if (!TryRematchAfterManual(wafer, mapFile, out var retryScore))
                        {
                            PostAlarm((int)AlarmKeys.eInputStageAlignNotCompleted);
                            Log.Write(UnitName, "MapMatch", "Rematch failed after manual -> abort");
                            return -1;
                        }

                        Log.Write(UnitName, "MapMatch",
                            $"Rematch after manual. Score={retryScore:F2} Threshold={scoreThreshold:F2}");

                        bestScore = retryScore;
                        continue;
                    }

                    Log.Write(UnitName, "MapMatch",
                        $"User chose CONTINUE without manual. Score={bestScore:F2}, Threshold={scoreThreshold:F2}.");
                    return 0;
                }
            }
        }
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
        /// <summary>
        /// 2) 상위에서 맵 다운로드/비교 후 진행 여부 판단 (MapMatchMode 기준)
        /// </summary>
        private int EvaluateMapMatchAndDecide(MaterialWafer wafer, List<PointD> consolidated)
        {
            //Sort Start (기존 유지)
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

                var eqpConfig = Equipment.Instance.EquipmentConfig;
                bool bUse = eqpConfig.MapMatchMode;
                var recipe = Equipment.Instance.EquipmentRecipe.CurrentRecipe;

                if (!bUse)
                {
                    // 기존: 맵핑 완료 후 사용자 확인
                    if (Config.IsSimulation == false)
                    {
                        var ask = new MessageBoxYesNo();
                        ask.TopMost = true;
                        if (ask.ShowDialog("진행 유무 확인", $"맵핑 완료. 진행 하시겠습니까?") != DialogResult.Yes)
                        {
                            OnStop();
                            ChipMappingDone = false;

                            var eq = Equipment.Instance;
                            eq.SequenceStopAllAsync(CancellationToken.None);

                            Log.Write(UnitName, "TryShutdownIfAllCassettesEmpty", "모든 관련 Unit 정지 완료.");
                            return -1;
                        }
                    }
                    return 0;
                }

                // MapMatchMode = true 인 경우
                string mapFilePath = recipe.MapFilePath;
                string strMapFile = PrepareLocalMapFileOrAlarm(wafer, mapFilePath);
                if (string.IsNullOrWhiteSpace(strMapFile))
                    return -1;

                if (Config.IsSimulation || Config.IsDryRun)
                {
                    Log.Write(UnitName, "MapMatch", "Simulation/DryRun -> skip file-based map matching.");
                    return 0;
                }

                // 원본 맵 파일 선검증
                var orgPreview = wafer.ReadFileOnline(strMapFile, MaterialWafer.MapTyp.waf);
                if (orgPreview == null || orgPreview.Count == 0)
                {
                    PostAlarm((int)AlarmKeys.eInputStageAlignNotCompleted);
                    Log.Write(UnitName, "MapMatch", $"Original map parse failed or empty: {strMapFile}");
                    return -1;
                }

                // Summary Total/Scan 반영(기존 유지)
                TrySummaryUpdateOrgAndScanCount(orgPreview.Count, wafer.Dies != null ? wafer.Dies.Count : 0);

                double bestScore = wafer.Mapmatch(strMapFile, MaterialWafer.MapTyp.waf) * 100.0;
                Log.Write(UnitName, "MapMatch",
                    $"Done. Score={bestScore:F3} OrgCount={orgPreview.Count} ScanCount={wafer.Dies.Count} MapFile='{strMapFile}'");

                double scoreThreshold = recipe.WaferMatchLimitPercent;

                while (true)
                {
                    if (bestScore >= scoreThreshold)
                        break;

                    using (var dlg = new MapMatchDecisionDialog(bestScore, scoreThreshold, strMapFile))
                    {
                        var dr = dlg.ShowDialog();
                        if (dr != DialogResult.Yes)
                        {
                            PostAlarm((int)AlarmKeys.eInputStageAlignNotCompleted);
                            Log.Write(UnitName, "MapMatch",
                                $"User chose STOP. Score={bestScore:F2} < Threshold={scoreThreshold:F2}. Sequence aborted.");
                            return -1;
                        }

                        if (dlg.ManualSettings != null)
                        {
                            bool applied = ApplyManualMapMatchToWafer(wafer, dlg.ManualSettings);
                            if (!applied)
                            {
                                PostAlarm((int)AlarmKeys.eInputStageAlignNotCompleted);
                                Log.Write(UnitName, "MapMatch", "Manual settings apply failed -> abort");
                                return -1;
                            }

                            EventUpdateUIWafer?.BeginInvoke(wafer, null, null);

                            if (!TryRematchAfterManual(wafer, strMapFile, out var retryScore))
                            {
                                PostAlarm((int)AlarmKeys.eInputStageAlignNotCompleted);
                                Log.Write(UnitName, "MapMatch", "Rematch failed after manual -> abort");
                                return -1;
                            }

                            Log.Write(UnitName, "MapMatch",
                                $"Rematch after manual. Score={retryScore:F2} Threshold={scoreThreshold:F2}");

                            bestScore = retryScore;
                            continue;
                        }

                        Log.Write(UnitName, "MapMatch",
                            $"User chose CONTINUE without manual. Score={bestScore:F2}, Threshold={scoreThreshold:F2}.");
                        break;
                    }
                }

                EventUpdateUIWafer?.BeginInvoke(wafer, null, null);
                return 0;
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
        private void TrySummaryUpdateScanAndTotalCount(List<PointD> consolidated)
        {
            try
            {
                var ctx = Equipment.Instance?.SummaryContext;
                var sum = ctx?.GetCurrentSummaryOrNull();

                if (ctx != null && ctx.IsActive && sum != null)
                {
                    int scanCount = consolidated != null ? consolidated.Count : 0;
                    //sum.AddScanCount(scanCount);
                    //sum.AddTotalCount(scanCount);
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
                    // [ADD] Simulation/DryRun: 확률 기반으로 Rank=1 적용 (테스트용)
                    if (Config.IsSimulation || Config.IsDryRun)
                    {
                        double p = SimOkRankProbability;
                        if (p < 0.0) p = 0.0;
                        if (p > 1.0) p = 1.0;

                        foreach (var die in wafer.Dies)
                        {
                            if (die == null)
                                continue;

                            // 기존 값이 남아있을 수 있으니 초기화(선택)
                            // die.Rank = 0;

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
        private void MarkNonRank1DiesAsSkippedOrNg(MaterialWafer wafer, int okRank = 1)
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

                        d.SetReject("MapMapping - Rank!=1");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Write(UnitName, nameof(MarkNonRank1DiesAsSkippedOrNg), ex.Message);
            }
        }

        #endregion


        #region MapMatch / Path Ordering
        private bool _pathBaseLocked;
        private MapPathStartCorner _pathBaseCorner = MapPathStartCorner.BottomLeft;
        private MapPathPrimaryAxis _pathBaseAxis = MapPathPrimaryAxis.XFirst;

        private void LockPathBaseFromRecipeOnce()
        {
            if (_pathBaseLocked) return;
            var recipe = Equipment.Instance?.EquipmentRecipe?.CurrentRecipe;
            if (recipe == null) return;

            _pathBaseCorner = recipe.WaferPathStartCorner;
            _pathBaseAxis = recipe.WaferPathPrimaryAxis;
            _pathBaseLocked = true;
        }
        // 옵션에 Chip Loading 순서 변경. Index기준으로 정렬.
        private void ApplyDieOrderByPathSettings(MaterialWafer wafer)
        {
            try
            {
                if (wafer?.Dies == null || wafer.Dies.Count == 0) return;

                // 최초 1회만 베이스 경로(코너/주축) 고정
                LockPathBaseFromRecipeOnce();

                lock (wafer.Dies)
                {
                    // 0) 셀 기준축(MapX/MapY) 구성
                    var xs = wafer.Dies.Where(d => d != null).Select(d => (int)d.MapX).Distinct().OrderBy(v => v).ToList();
                    var ys = wafer.Dies.Where(d => d != null).Select(d => (int)d.MapY).Distinct().OrderBy(v => v).ToList();
                    if (xs.Count == 0 || ys.Count == 0) return;

                    // 1) (ix,iy) -> bucket
                    var grid = new Dictionary<(int ix, int iy), List<MaterialDie>>();
                    foreach (var d in wafer.Dies.Where(d => d != null))
                    {
                        int mx = (int)d.MapX;
                        int my = (int)d.MapY;
                        int ix = xs.IndexOf(mx);
                        int iy = ys.IndexOf(my);
                        if (ix < 0 || iy < 0) continue;

                        var key = (ix, iy);
                        if (!grid.TryGetValue(key, out var bucket))
                        {
                            bucket = new List<MaterialDie>();
                            grid[key] = bucket;
                        }
                        bucket.Add(d);
                    }

                    // 2) 회전/미러(표시/방향)만 적용
                    var recipe = Equipment.Instance.EquipmentRecipe.CurrentRecipe;
                    var mapRotate = recipe.WaferRotate;
                    var mapMirror = recipe.WaferMirror;

                    int nx = xs.Count;
                    int ny = ys.Count;

                    (int tx, int ty) ApplyRotation(int ix, int iy)
                    {
                        switch (mapRotate)
                        {
                            case MapRotateOption.CW90: return (ny - 1 - iy, ix);
                            case MapRotateOption.CW180: return (nx - 1 - ix, ny - 1 - iy);
                            case MapRotateOption.CW270: return (iy, nx - 1 - ix);
                            default: return (ix, iy);
                        }
                    }
                    (int tx, int ty) ApplyMirror(int ix, int iy)
                    {
                        switch (mapMirror)
                        {
                            case MapMirrorOption.X: return (nx - 1 - ix, iy);
                            case MapMirrorOption.Y: return (ix, ny - 1 - iy);
                            case MapMirrorOption.XY: return (nx - 1 - ix, ny - 1 - iy);
                            default: return (ix, iy);
                        }
                    }
                    (int tx, int ty) Transform(int ix, int iy)
                    {
                        var r = ApplyRotation(ix, iy);
                        return ApplyMirror(r.tx, r.ty);
                    }

                    // 3) 베이스 StartCorner/PrimaryAxis로 순회 방향 “한 번만” 결정
                    int xDir = 0, yDir = 0;
                    switch (_pathBaseCorner)
                    {
                        default:
                        case MapPathStartCorner.BottomLeft: xDir = +1; yDir = +1; break;
                        case MapPathStartCorner.BottomRight: xDir = -1; yDir = +1; break;
                        case MapPathStartCorner.TopLeft: xDir = +1; yDir = -1; break;
                        case MapPathStartCorner.TopRight: xDir = -1; yDir = -1; break;
                    }

                    IEnumerable<int> RangeDir(int count, int dir)
                    {
                        if (dir > 0) { for (int i = 0; i < count; i++) yield return i; }
                        else { for (int i = count - 1; i >= 0; i--) yield return i; }
                    }

                    var xLineF = RangeDir(xs.Count, xDir).ToList();
                    var xLineR = xLineF.AsEnumerable().Reverse().ToList();
                    var yLineF = RangeDir(ys.Count, yDir).ToList();
                    var yLineR = yLineF.AsEnumerable().Reverse().ToList();

                    var ordered = new List<MaterialDie>(wafer.Dies.Count);
                    Action<IEnumerable<int>, IEnumerable<int>> addBy = (xSeq, ySeq) =>
                    {
                        foreach (var iy in ySeq)
                        {
                            foreach (var ix in xSeq)
                            {
                                // 조회 키에는 회전/미러만 반영
                                var keyT = Transform(ix, iy);
                                if (!grid.TryGetValue(keyT, out var bucket) || bucket.Count == 0)
                                    continue;

                                // tie-break 안정화
                                var cx = xs[Math.Max(0, Math.Min(xs.Count - 1, ix))];
                                var cy = ys[Math.Max(0, Math.Min(ys.Count - 1, iy))];
                                var sel = bucket
                                    .OrderBy(d => (d.CenterX - cx) * (d.CenterX - cx) + (d.CenterY - cy) * (d.CenterY - cy))
                                    .ThenBy(d => d.MapY)
                                    .ThenBy(d => d.MapX)
                                    .ThenBy(d => d.CenterY)
                                    .ThenBy(d => d.CenterX);

                                foreach (var dd in sel) ordered.Add(dd);
                            }
                        }
                    };

                    // 베이스 주축 기준으로 래스터/지그재그만 고정
                    var traversal = recipe.WaferPathTraversalMode;
                    if (_pathBaseAxis == MapPathPrimaryAxis.XFirst)
                    {
                        for (int row = 0; row < ys.Count; row++)
                        {
                            var xSeq = (traversal == MapPathTraversalMode.Serpentine && (row % 2 == 1)) ? xLineR : xLineF;
                            addBy(xSeq, new[] { yLineF[row] });
                        }
                    }
                    else // YFirst
                    {
                        for (int col = 0; col < xs.Count; col++)
                        {
                            var ySeq = (traversal == MapPathTraversalMode.Serpentine && (col % 2 == 1)) ? yLineR : yLineF;
                            addBy(new[] { xLineF[col] }, ySeq);
                        }
                    }

                    // 4) 인덱스 재부여
                    for (int i = 0; i < ordered.Count; i++)
                    {
                        ordered[i].Index = i;
                        if (!string.IsNullOrEmpty(wafer.WaferId))
                            ordered[i].Name = $"{wafer.WaferId}_{i}";
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Write(UnitName, "ApplyDieOrderByPathSettings", ex.Message);
            }
        }

        // [ADD] Map 파일 로컬 저장 폴더 (프로세스 기준)
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

            // 폴더 경로이면 waferId.waf로 조합
            if (Directory.Exists(mapFilePath))
            {
                return Path.Combine(mapFilePath, waferId + ".waf");
            }

            // 파일 경로이면 그대로
            if (File.Exists(mapFilePath))
            {
                return mapFilePath;
            }

            // 존재하지 않는 폴더/파일이면, "폴더로 가정"하고 조합 시도(UNC 지연 연결 대비)
            // 예: \\server\share\mapfiles (Directory.Exists가 false로 떨어질 수 있음)
            // -> 일단 waferId.waf 경로를 만들어 반환하고, 호출부에서 File.Exists로 최종 체크
            return Path.Combine(mapFilePath, waferId + ".waf");
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

            localWafPath = System.IO.Path.Combine(localDir, waferId + ".waf");

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
                double score = wafer.Mapmatch(mapFile, MaterialWafer.MapTyp.waf);
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
            EventUpdateUIWafer?.BeginInvoke(materialWafer, null, null);
        }
        public bool IsRingPresent()
        {
            bool bRtn = true;
            if (Config.IsSimulation || Config.IsDryRun)
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
                if (Config.IsSimulation == false
                    && Config.IsDryRun == false)
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

                if (Config.IsSimulation == false
                   && Config.IsDryRun == false)
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
                if (Config.IsSimulation
                    || Config.IsDryRun)
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
            catch
            {
                die = null;
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
                // 기존 코드
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
                        autoEstimatePitch: false);

                    int nIndex = 0;
                    var list = materialWafer.Dies.OrderBy(t => t.MapX).ThenBy(t => t.MapY);

                    //맵서치완료한 다이 정보 로그 출력을 다른 파일에 저장하도록 변경
                    DateTime now = DateTime.Now;
                    string logFile = string.Empty;
                    logFile = string.Format("ChipMapLog_{0}.txt", now.ToString("yyyyMMdd"));
                    string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, logFile);
                    foreach (var c in list)
                    {
                        //Log.Write(UnitName, "ChipMap",
                        Log.Write(logPath, "ChipMap",
                            $"Chip={nIndex}: ,X={c.MapX}, Y={c.MapY}, PosX={c.CenterX:F3}, PosY={c.CenterY:F3}");
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
        //private void UpdateChipInfo(List<PointD> chips)
        //{
        //    try
        //    {
        //        MaterialWafer materialWafer = GetMaterialWafer();
        //        lock (materialWafer.Dies)
        //        {
        //            materialWafer.Dies.Clear();
        //            materialWafer.MakeWaferInfo(chips, this.ChipPitchXmm, this.ChipPitchYmm);
        //            //materialWafer.MakeWaferInfo(chips, this.ChipPitchXmm, this.ChipPitchYmm, alreadyConsolidated: true);

        //            int nIndex = 0;
        //            if (true)
        //            {
        //                var list = materialWafer.Dies.OrderBy(t => t.MapX).ThenBy(t => t.MapY);
        //                foreach (var c in list)
        //                {
        //                    Log.Write(UnitName, "ChipMap", $"Chip={nIndex}: ,X={c.MapX}, Y={c.MapY}, PosX={c.CenterX:F3}, PosY={c.CenterY:F3}");
        //                    nIndex++;
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.Write(ex);
        //    }
        //    finally
        //    {
        //        MaterialWafer materialWafer = GetMaterialWafer();
        //        EventUpdateUIWafer?.BeginInvoke(materialWafer, null, null);
        //    }
        //}

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

                try
                {
                    if (Config.IsSimulation == false && this.Config.IsDryRun == false)
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
                if (Config.IsSimulation == false && this.Config.IsDryRun == false)
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

        //private void MakeScanPath(out List<PointD> path)
        //{
        //    path = new List<PointD>();
        //    try
        //    {
        //        double centerTpX = GetTP(InputStageConfig.TeachingPositionName.CenterPoint.ToString(), AxisX.Name);
        //        double centerTpY = GetTP(InputStageConfig.TeachingPositionName.CenterPoint.ToString(), AxisY.Name);
        //        var eq = Equipment.Instance;
        //        var recip = eq.EquipmentRecipe.CurrentRecipe;
        //        double dRadius = recip.WaferDiameter / 2;

        //        try
        //        {
        //            if (Config.IsSimulation == false && this.Config.IsDryRun == false)
        //            {
        //                if (PmRunner.IsRecipeLoaded == false)
        //                {
        //                    PmRunner.LoadRecipe();
        //                }
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            Log.Write(ex);
        //        }

        //        double dRoiWidth = 0.0;// Math.Abs((PmRunner._Roi.InspectEnd.X - PmRunner._Roi.InspectStart.X) * StageCamera.CameraConfig.Scale.X);
        //        double dRoiHeight = 0.0;//Math.Abs((PmRunner._Roi.InspectEnd.Y - PmRunner._Roi.InspectStart.Y) * StageCamera.CameraConfig.Scale.Y);
        //        if (Config.IsSimulation == false && this.Config.IsDryRun == false)
        //        {
        //            dRoiWidth = Math.Abs((PmRunner._Roi.InspectEnd.X - PmRunner._Roi.InspectStart.X) * StageCamera.CameraConfig.Scale.X);
        //            dRoiHeight = Math.Abs((PmRunner._Roi.InspectEnd.Y - PmRunner._Roi.InspectStart.Y) * StageCamera.CameraConfig.Scale.Y);
        //        }
        //        else
        //        {
        //            dRoiWidth = 0.85;
        //            dRoiHeight = 0.7;
        //        }

        //        double dChipPitchX = ChipPitchXmm;
        //        double dChipPitchY = ChipPitchYmm;
        //        if (dChipPitchX <= 0) dChipPitchX = 0.5;
        //        if (dChipPitchY <= 0) dChipPitchY = 0.5;



        //        dRoiWidth -= dChipPitchX * 2;
        //        dRoiHeight -= dChipPitchY * 2;
        //        int nHorzCount = (int)((dRadius - dChipPitchX) * 2 / dRoiWidth) + 1;
        //        int nVertCount = (int)((dRadius - dChipPitchY) * 2 / dRoiHeight) + 1;
        //        if (nHorzCount < 1) nHorzCount = 1;
        //        if (nVertCount < 1) nVertCount = 1;
        //        double startX = centerTpX - (nHorzCount - 1) * dRoiWidth / 2;
        //        double startY = centerTpY - (nVertCount - 1) * dRoiHeight / 2;



        //        bool useYScanFirst = true; // Config에서 선택 가능하도록 개선 예정
        //        if (useYScanFirst)
        //        {
        //            //y방향으로 서치
        //            for (int ix = 0; ix < nHorzCount; ix++)
        //            {
        //                double x = startX + ix * dRoiWidth;
        //                for (int iy = 0; iy < nVertCount; iy++)
        //                {
        //                    double y = startY + iy * dRoiHeight;

        //                    // 지그재그 패턴: X 열 기준으로 Y 스캔 방향 전환
        //                    if (ix % 2 == 1)
        //                    {
        //                        // 홀수 열은 Y를 반대 방향으로 스캔
        //                        y = startY + (nVertCount - 1 - iy) * dRoiHeight;
        //                    }

        //                    double dx = x - centerTpX;
        //                    double dy = y - centerTpY;
        //                    double dist = Math.Sqrt(dx * dx + dy * dy);
        //                    double offsetDist = GetDistance(dRoiWidth / 2, dRoiHeight / 2);
        //                    if (dist <= dRadius + offsetDist)
        //                    {
        //                        path.Add(new PointD(x, y));
        //                    }
        //                }
        //            }
        //        }
        //        else
        //        {
        //            //x방향으로 서치
        //            for (int iy = 0; iy < nVertCount; iy++)
        //            {
        //                double y = startY + iy * dRoiHeight;
        //                // 행 우선 지그재그: Y 고정 후 X 방향 반전
        //                bool reverse = (iy % 2 == 1);
        //                for (int ix = 0; ix < nHorzCount; ix++)
        //                {
        //                    int rx = reverse ? (nHorzCount - 1 - ix) : ix;
        //                    double x = startX + rx * dRoiWidth;

        //                    double dx = x - centerTpX;
        //                    double dy = y - centerTpY;
        //                    double dist = Math.Sqrt(dx * dx + dy * dy);
        //                    double offsetDist = GetDistance(dRoiWidth / 2, dRoiHeight / 2);
        //                    if (dist <= dRadius + offsetDist)
        //                        path.Add(new PointD(x, y));
        //                }
        //            }
        //        }

        //        Log.Write(UnitName, "MakeScanPath",
        //            $"Count={path.Count} Radius={dRadius} Center=({centerTpX:F3},{centerTpY:F3}) ROI=({dRoiWidth:F3},{dRoiHeight:F3}) ChipPitch=({dChipPitchX:F3},{dChipPitchY:F3})");

        //    }
        //    catch (Exception ex)
        //    {
        //        Log.Write(ex);
        //    }
        //    //StageCamera.CameraConfig.Scale
        //}

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
        private int WaitPlateStateOrAlarm(bool expectUp, int timeoutMs = 3000, int pollMs = 2)
        {
            if (Config.IsSimulation || Config.IsDryRun)
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
        private int WaitClampLiftStateOrAlarm(bool expectUp, int timeoutMs = 3000, int pollMs = 2)
        {
            if (Config.IsSimulation || Config.IsDryRun)
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
        private int WaitClampFBStateOrAlarm(bool expectFwd, int timeoutMs = 3000, int pollMs = 2)
        {
            if (Config.IsSimulation || Config.IsDryRun)
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
            if (!Config.IsSimulation && !Config.IsDryRun)
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
            //// 이미 웨이퍼 존재하면 준비 단계 불필요 (바로 완료 단계 가능)
            //if (Config.IsSimulation == false 
            //    && Config.IsDryRun == false)    
            //{
            //    if (IsRingPresent())
            //    {
            //        MaterialWafer wafer = GetMaterialWafer();
            //        if(wafer != null)
            //        {
            //            Log.Write(UnitName, "PrepareLoadingStage", "Fail: wafer != null");
            //            return -1;
            //        }
            //        if(wafer.ProcessSatate != Material.MaterialProcessSatate.Completed)
            //        {
            //            Log.Write(UnitName, "PrepareLoadingStage", "Wafer already present -> Skip prepare");
            //            return nRtn;
            //        }
            //    }
            //}

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

            if (Config.IsSimulation || Config.IsDryRun)
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

            // Simulation/DryRun은 기존처럼 통과 (Refine 로직에서 0으로 수렴했다고 치게 됨)
            if (Config.IsSimulation || Config.IsDryRun)
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
            step = step * 3;
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

                if (this.Config.IsSimulation == false && this.Config.IsDryRun == false)
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



        private struct DetectedChip
        {
            public double X;     // stage mm
            public double Y;     // stage mm
            public double Score; // pattern score (없으면 1.0)
        }

        private struct GridKey : IEquatable<GridKey>
        {
            public int I;
            public int J;
            public GridKey(int i, int j) { I = i; J = j; }
            public bool Equals(GridKey other) => I == other.I && J == other.J;
            public override bool Equals(object obj) => obj is GridKey k && Equals(k);
            public override int GetHashCode() => (I * 397) ^ J;
        }

        private class GridModel
        {
            // stage(mm) = origin + i*uVec + j*vVec
            public PointD Origin;
            public PointD U; // unit: mm/step (pitch direction)
            public PointD V;

            public double PitchU => Math.Sqrt(U.X * U.X + U.Y * U.Y);
            public double PitchV => Math.Sqrt(V.X * V.X + V.Y * V.Y);

            public bool IsValid => PitchU > 1e-6 && PitchV > 1e-6;

            // stage -> (i,j) (real) : solve 2x2
            public bool TryStageToIJ(double x, double y, out double i, out double j)
            {
                i = j = 0;
                double dx = x - Origin.X;
                double dy = y - Origin.Y;

                // [ Ux Vx ] [i] = [dx]
                // [ Uy Vy ] [j]   [dy]
                double det = U.X * V.Y - U.Y * V.X;
                if (Math.Abs(det) < 1e-9) return false;

                i = (dx * V.Y - dy * V.X) / det;
                j = (-dx * U.Y + dy * U.X) / det;
                return true;
            }

            public PointD IJToStage(double i, double j)
            {
                return new PointD(
                    Origin.X + i * U.X + j * V.X,
                    Origin.Y + i * U.Y + j * V.Y);
            }
        }


        //“T 하나”가 아니라, **두 축 벡터(U,V)**를 얻으므로,
        //Y방향이 틀어진(직교가 아니거나) 상황에서도 매칭이 i,j 기반으로 안정화될 수 있다.
        private bool TryEstimateGridModelFromPoints(
                    List<DetectedChip> pts,
                    out GridModel model)
        {
            model = null;
            if (pts == null || pts.Count < 30) return false;

            // 1) 중심(Origin)은 중앙 근처 점으로 임시 설정 (나중에 fit에서 보정됨)
            var cx = pts.Average(p => p.X);
            var cy = pts.Average(p => p.Y);

            // 2) 각 점의 nearest-neighbor 벡터를 모아서 pitch/방향 후보를 만든다
            //    (O(N^2) 방지를 위해 랜덤 샘플링)
            int N = pts.Count;
            int sample = Math.Min(N, 500);
            var rnd = new Random(0);

            var vecs = new List<PointD>(sample);
            for (int s = 0; s < sample; s++)
            {
                var p = pts[rnd.Next(N)];

                // nearest 찾기 (간단히)
                double bestD2 = double.MaxValue;
                DetectedChip best = default;
                for (int k = 0; k < 80; k++)
                {
                    var q = pts[rnd.Next(N)];
                    double dx = q.X - p.X;
                    double dy = q.Y - p.Y;
                    double d2 = dx * dx + dy * dy;
                    if (d2 < 1e-6) continue;
                    if (d2 < bestD2)
                    {
                        bestD2 = d2;
                        best = q;
                    }
                }

                if (bestD2 < double.MaxValue)
                {
                    vecs.Add(new PointD(best.X - p.X, best.Y - p.Y));
                }
            }

            if (vecs.Count < 10) return false;

            // 3) 길이가 pitch 근처인 벡터만 남김
            double pitchGuess = Math.Min(ChipPitchXmm > 0 ? ChipPitchXmm : 0.9,
                                         ChipPitchYmm > 0 ? ChipPitchYmm : 0.9);

            double minLen = pitchGuess * 0.5;
            double maxLen = pitchGuess * 1.5;

            var candidates = vecs
                .Select(v => (v, len: Math.Sqrt(v.X * v.X + v.Y * v.Y)))
                .Where(t => t.len >= minLen && t.len <= maxLen)
                .ToList();

            if (candidates.Count < 10) return false;

            // 4) 방향 2개(서로 직교에 가까운) 찾기
            //    가장 많이 나온 방향(각도)과, 그와 거의 직교인 방향을 선택
            double AngleDeg(PointD v) => Math.Atan2(v.Y, v.X) * 180.0 / Math.PI;

            // 10deg bin
            var bins = candidates
                .Select(t =>
                {
                    double a = AngleDeg(t.v);
                    // 180도 대칭 정규화
                    if (a < 0) a += 180;
                    int b = (int)Math.Round(a / 10.0);
                    return (b, t.v, t.len);
                })
                .GroupBy(x => x.b)
                .OrderByDescending(g => g.Count())
                .ToList();

            if (bins.Count < 2) return false;

            var mainBin = bins[0];
            var mainVec = AverageVec(mainBin.Select(x => x.v));

            // 가장 직교에 가까운 bin 찾기
            double mainAng = AngleDeg(mainVec);
            double bestScore = double.MaxValue;
            PointD orthoVec = default;

            foreach (var g in bins.Skip(1))
            {
                var vv = AverageVec(g.Select(x => x.v));
                double a = AngleDeg(vv);
                double diff = Math.Abs(NormalizeAngleDeg((a - mainAng)));
                diff = Math.Min(diff, Math.Abs(180 - diff)); // 0~90 근처로
                double orthoDiff = Math.Abs(diff - 90);
                if (orthoDiff < bestScore)
                {
                    bestScore = orthoDiff;
                    orthoVec = vv;
                }
            }

            if (bestScore > 30) return false; // 너무 직교가 안나오면 실패로

            // 5) 모델 생성 (origin은 중심 근처)
            var gm = new GridModel
            {
                Origin = new PointD(cx, cy),
                U = NormalizeToPitch(mainVec, ChipPitchXmm > 0 ? ChipPitchXmm : pitchGuess),
                V = NormalizeToPitch(orthoVec, ChipPitchYmm > 0 ? ChipPitchYmm : pitchGuess),
            };

            if (!gm.IsValid) return false;
            model = gm;
            return true;

            // local helpers
            PointD AverageVec(IEnumerable<PointD> vs)
            {
                double sx = 0, sy = 0; int c = 0;
                foreach (var v in vs) { sx += v.X; sy += v.Y; c++; }
                if (c == 0) return new PointD(1, 0);
                return new PointD(sx / c, sy / c);
            }

            PointD NormalizeToPitch(PointD v, double pitch)
            {
                double len = Math.Sqrt(v.X * v.X + v.Y * v.Y);
                if (len < 1e-9) return new PointD(pitch, 0);
                return new PointD(v.X / len * pitch, v.Y / len * pitch);
            }

            double NormalizeAngleDeg(double a)
            {
                while (a < 0) a += 180;
                while (a >= 180) a -= 180;
                return a;
            }
        }

        //ConsolidateRawChips를 아래처럼 바꿔야 함.
        private List<PointD> ConsolidateRawChipsGridAware(List<PointD> rawPoints, List<double> scores = null)
        {
            // rawPoints는 stage mm
            if (rawPoints == null || rawPoints.Count == 0)
                return new List<PointD>();

            // DetectedChip로 변환
            var det = new List<DetectedChip>(rawPoints.Count);
            for (int i = 0; i < rawPoints.Count; i++)
            {
                det.Add(new DetectedChip
                {
                    X = rawPoints[i].X,
                    Y = rawPoints[i].Y,
                    Score = (scores != null && i < scores.Count) ? scores[i] : 1.0
                });
            }

            if (!TryEstimateGridModelFromPoints(det, out var gm))
            {
                // fallback: 기존 거리기반(최소한)
                Log.Write(UnitName, "ChipMap", "[GridAware] grid estimate fail -> fallback distance consolidate");
                return ConsolidateRawChips(rawPoints);
            }

            // 셀 단위로 할당: 같은 (i,j)만 중복
            var cellBest = new Dictionary<GridKey, DetectedChip>();

            foreach (var p in det)
            {
                if (!gm.TryStageToIJ(p.X, p.Y, out double ii, out double jj))
                    continue;

                int iCell = (int)Math.Round(ii);
                int jCell = (int)Math.Round(jj);

                // 셀로부터 너무 멀면 outlier로 버림 (이게 중요: 잘못된 셀로 붙는 걸 방지)
                var pred = gm.IJToStage(iCell, jCell);
                double err = GetDistance(pred.X - p.X, pred.Y - p.Y);

                double gate = Math.Min(gm.PitchU, gm.PitchV) * 0.45; // 튜닝 가능
                if (err > gate)
                    continue;

                var key = new GridKey(iCell, jCell);

                if (!cellBest.TryGetValue(key, out var cur))
                {
                    cellBest[key] = p;
                }
                else
                {
                    // 중복 해결: score 우선, 동점이면 예측점 오차가 작은 것
                    var curPred = gm.IJToStage(iCell, jCell);
                    double curErr = GetDistance(curPred.X - cur.X, curPred.Y - cur.Y);

                    bool replace = false;
                    if (p.Score > cur.Score + 1e-6) replace = true;
                    else if (Math.Abs(p.Score - cur.Score) < 1e-6 && err < curErr) replace = true;

                    if (replace) cellBest[key] = p;
                }
            }

            var merged = cellBest.Values
                .Select(v => new PointD(v.X, v.Y))
                .ToList();

            Log.Write(UnitName, "ChipMap",
                $"[GridAware] raw={rawPoints.Count} merged={merged.Count} pitchU={gm.PitchU:F3} pitchV={gm.PitchV:F3}");

            return merged;
        }


    }
}