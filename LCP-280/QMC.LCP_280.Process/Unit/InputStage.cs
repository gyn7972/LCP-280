using Newtonsoft.Json;
using QMC.Common;
using QMC.Common.Alarm;
using QMC.Common.Cameras; // Camera base
using QMC.Common.Cameras.HIKVISION; // HIK camera
using QMC.Common.Component;
using QMC.Common.IOUtil;
using QMC.Common.Motion;
using QMC.Common.Motions;
using QMC.Common.Unit;
using QMC.Common.Vision;              // VisionImage
using QMC.Common.Vision.Cognex;       // Legacy compatibility
using QMC.Common.Vision.Tools;        // Tool base
using QMC.LCP_280.Process;            // PatternMatchingRunner
using QMC.LCP_280.Process.Component;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using static QMC.LCP_280.Process.Equipment;
using static QMC.LCP_280.Process.Unit.InputCassetteLifter;
using static System.Windows.Forms.AxHost;

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
    public class InputStage : BaseUnit<InputStageConfig>
    {
        public enum AlarmKeys
        {
            eDieTransferPickZNotSafe = 3001,
            eInputFeederCylinderZNotSafe,
            eInputStageEjectorPinZNotSafe,
            eInputStageEjectorZNotSafe,
            eInputFeederYNotSafe,
            eVisionTsearch,
            eVisionXYsearch,

        }
        private struct AngleStats
        {
            public int RawCount;
            public double Average;
            public double StdDev;
            public double Representative;
        }

        #region InitAlarm
        protected override void InitAlarm()
        {
            base.InitAlarm();
            AlarmInfo alarm = new AlarmInfo();
            alarm.Code = (int)AlarmKeys.eDieTransferPickZNotSafe;
            alarm.Title = "Die Tr Z-Axis Not Sfarety Pos.";
            alarm.Cause = "Die TrZAxis이 안전 위치가 아닙니다.\n 포지션 확인 후 다시 시작 하십시요.";
            alarm.Source = this.UnitName;
            alarm.Grade = AlarmInfo.AlarmType.Warning.ToString();
            m_dicAlarms.Add(alarm.Code, alarm);

            alarm = new AlarmInfo();
            alarm.Code = (int)AlarmKeys.eInputFeederCylinderZNotSafe;
            alarm.Title = "Feeder Z-Cylinder Not Sfarety Pos.";
            alarm.Cause = "Feeder Z-Cylinder가 안전 위치가 아닙니다.\n 포지션 확인 후 다시 시작 하십시요.";
            alarm.Source = this.UnitName;
            alarm.Grade = AlarmInfo.AlarmType.Warning.ToString();
            m_dicAlarms.Add(alarm.Code, alarm);

            //,
            alarm = new AlarmInfo();
            alarm.Code = (int)AlarmKeys.eInputStageEjectorPinZNotSafe;
            alarm.Title = "EjectorPin Z-Axis Not Sfarety Pos.";
            alarm.Cause = "EjectorPin Z-Axis가 안전 위치가 아닙니다.\n 포지션 확인 후 다시 시작 하십시요.";
            alarm.Source = this.UnitName;
            alarm.Grade = AlarmInfo.AlarmType.Warning.ToString();
            m_dicAlarms.Add(alarm.Code, alarm);
            //,
            alarm = new AlarmInfo();
            alarm.Code = (int)AlarmKeys.eInputStageEjectorZNotSafe;
            alarm.Title = "Ejector Z-Axis Not Sfarety Pos.";
            alarm.Cause = "Ejector Z-Axis가 안전 위치가 아닙니다.\n 포지션 확인 후 다시 시작 하십시요.";
            alarm.Source = this.UnitName;
            alarm.Grade = AlarmInfo.AlarmType.Warning.ToString();
            m_dicAlarms.Add(alarm.Code, alarm);

            //
            alarm = new AlarmInfo();
            alarm.Code = (int)AlarmKeys.eInputFeederYNotSafe;
            alarm.Title = "Feeder Y-Axis Not Sfarety Pos.";
            alarm.Cause = "Feeder Y-Axis가 안전 위치가 아닙니다.\n 포지션 확인 후 다시 시작 하십시요.";
            alarm.Source = this.UnitName;
            alarm.Grade = AlarmInfo.AlarmType.Warning.ToString();
            m_dicAlarms.Add(alarm.Code, alarm);

            alarm = new AlarmInfo();
            alarm.Code = (int)AlarmKeys.eVisionTsearch;
            alarm.Title = "Vision T Search.";
            alarm.Cause = "Vision T Search Fail.\n Chip Mark 확인 후 다시 시작 하십시요.";
            alarm.Source = this.UnitName;
            alarm.Grade = AlarmInfo.AlarmType.Warning.ToString();
            m_dicAlarms.Add(alarm.Code, alarm);
            //
            alarm = new AlarmInfo();
            alarm.Code = (int)AlarmKeys.eVisionXYsearch;
            alarm.Title = "Vision XY Search.";
            alarm.Cause = "Vision XY Search Fail.\n Chip Mark 확인 후 다시 시작 하십시요.";
            alarm.Source = this.UnitName;
            alarm.Grade = AlarmInfo.AlarmType.Warning.ToString();
            m_dicAlarms.Add(alarm.Code, alarm);
        }
        #endregion

        #region Config / Teaching
        Equipment _equipment => Equipment.Instance;
        #endregion

        #region Vision Hooks / Camera / Runner
        public HIKGigECamera StageCamera { get; private set; }
        public string StageCameraKey { get; set; } = "In_Stage";

        // Pattern Matching Runner (간소화: Recipe 자동 관리)
        private PatternMatchingRunner _pmRunner;
        private bool _runnerInitTried;

        // Pixel -> mm scale
        public double PixelSizeXmm { get; set; } = 0.005;
        public double PixelSizeYmm { get; set; } = 0.005;
        public bool UseImageCenterAsOrigin { get; set; } = true;
        public double ImageOriginX { get; set; } = double.NaN;
        public double ImageOriginY { get; set; } = double.NaN;
        public string PatternRecipeRootDir { get; set; } = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configs", "PatternMatching");
        public string PatternRecipeName { get; set; } = "Default";
        #endregion


        InputDieTransfer InputDieTransfer { get; set; }
        InputFeeder InputFeeder {get; set; }
        InputStageEjector InputStageEjector { get; set; }

        #region Construction / Initialization
        public InputStage(InputStageConfig config = null)
            : base(new InputStageConfig())
        {
            
            AddComponents();

        }

        protected override void OnBindUnit()
        {
            base.OnBindUnit();
            InputFeeder = Equipment.Instance.GetUnit(UnitKeys.InputFeeder) as InputFeeder;
            InputStageEjector = Equipment.Instance.GetUnit(UnitKeys.InputStageEjector) as InputStageEjector;
            InputDieTransfer = Equipment.Instance.GetUnit(UnitKeys.InputDieTransfer) as InputDieTransfer;
        }

        public override void AddComponents()
        {
            Config.LoadAndBindAxes(Equipment.Instance.AxisManager);
            Config.InitializeDefaultTeachingPositions();

            BindAxes();
            BindIoDomains();
            BindCamera();

            Config.IsSimulation = Config.IsSimulation; ;
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

        // ... 클래스 내부 기존 Vision Runner (Pattern Matching) 영역 교체
        #region Vision Runner (Pattern Matching)  // REFACTORED: Hub 사용
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

        private AngleStats ComputeAngleStats(List<double> angles, bool excludeExtremes)
        {
            var st = new AngleStats { RawCount = angles?.Count ?? 0 };
            if (angles == null || angles.Count == 0)
                return st;

            var ordered = angles.OrderBy(a => a).ToList();
            IEnumerable<double> work = ordered;

            if (excludeExtremes && ordered.Count >= 3)
                work = ordered.Skip(1).Take(ordered.Count - 2); // 최솟값/최댓값 1개씩 제거

            var wList = work.ToList();
            if (wList.Count == 0)
                return st;

            double avg = wList.Average();
            double var = 0.0;
            if (wList.Count > 1)
                var = wList.Sum(a => (a - avg) * (a - avg)) / (wList.Count - 1);
            double std = Math.Sqrt(var);

            // 대표값: 평균과 가장 가까운 "원본(전체 angles)" 값
            double rep = angles.OrderBy(a => Math.Abs(a - avg)).First();

            st.Average = avg;
            st.StdDev = std;
            st.Representative = rep;
            return st;
        }

        private (bool ok, double x, double y) CenterSearchViaRunner()
        {
            var res = VisionRunnerHub.SearchCenterOffset(
                CameraKey,
                PixelSizeXmm,
                PixelSizeYmm,
                ImageOriginX,
                ImageOriginY,
                UseImageCenterAsOrigin);

            if (!res.ok) return (false, 0, 0);
            return (true, res.dxMm, res.dyMm);
        }
        #endregion

        #region Axis Helpers
        private MotionAxis _axX, _axY, _axT;
        public MotionAxis AxisX => _axX;
        public MotionAxis AxisY => _axY;
        public MotionAxis AxisT => _axT;

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

        // ================== Generic Single Axis Move (Safety Interlock 동일 구조) ==================
        /// <summary>
        /// 단일 축 이동 (Safety 인터락 포함). 이동 완료까지 블록.
        /// </summary>
        public override int MoveAxisPositionOne(MotionAxis axis, double target, bool isFine = false)
        {
            if (axis == null) return -1;

            if(CheckMoveSafety(axis) != 0)
            {
                return -1;
            }

            Task<int> task = MoveAxisWithSafetyAsync(axis, target, isFine);
            while (IsEndTask(task) == false)
            {
                // 동일 Safety Interlock
                if (!InputStageEjector.IsPinZSafetyPos())
                {
                    AxisX?.EmgStop(); AxisY?.EmgStop(); AxisT?.EmgStop();
                    AlarmPost((int)AlarmKeys.eInputStageEjectorPinZNotSafe);
                    return -1;
                }
                if (!InputStageEjector.IsEjectorZSafetyPos())
                {
                    AxisX?.EmgStop(); AxisY?.EmgStop(); AxisT?.EmgStop();
                    AlarmPost((int)AlarmKeys.eInputStageEjectorZNotSafe);
                    return -1;
                }
                if (!InputDieTransfer.IsDieTransferPickZSafetyPos())
                {
                    AxisX?.EmgStop(); AxisY?.EmgStop(); AxisT?.EmgStop();
                    AlarmPost((int)AlarmKeys.eDieTransferPickZNotSafe);
                    return -1;
                }
                if (!InputFeeder.IsFeederZSafetyPosition())
                {
                    AxisX?.EmgStop(); AxisY?.EmgStop(); AxisT?.EmgStop();
                    AlarmPost((int)AlarmKeys.eInputFeederCylinderZNotSafe);
                    return -1;
                }
                if (!InputFeeder.IsFeederYSafetyPosition())
                {
                    AxisX?.EmgStop(); AxisY?.EmgStop(); AxisT?.EmgStop();
                    AlarmPost((int)AlarmKeys.eInputFeederYNotSafe);
                    return -1;
                }

                Thread.Sleep(0);
            }
            return task.Result;
        }

        //가공시에 스테이지 Area 밖으로 나가는것을 방지하기 위한 함수
        public override int CheckMoveSafety(MotionAxis ax)
        {
            try
            {
                //if (/*다른 유닛 축 이동중*/) return (int)AlarmKeys.xxx;
                // PickZ Safety Check
                // Ejector Pin Z and Ejector Z Safety Check
                // Ejector Pin Z and Ejector Z 이 Safety Position이 아닐 경우
                // X,Y Encoder 위치 기준 min/max 체크하고 움직여야 한다. 


                // 1) Ejector / PinZ Safety 검사 (우선순위 높음)
                bool pinZSafe = true;
                bool ejectorZSafe = true;

                if (InputStageEjector != null)
                {
                    pinZSafe = InputStageEjector.IsPinZSafetyPos();
                    ejectorZSafe = InputStageEjector.IsEjectorZSafetyPos();

                    if (!pinZSafe || !ejectorZSafe)
                    {
                        // PinZ 또는 EjectorZ 가 Safety 가 아닐 때 X/Y 이동 허용 범위 검사
                        if (ax == AxisX || ax == AxisY)
                        {
                            if (!IsAllowedXYWindowWhileEjectorUnsafe())
                            {
                                // 어떤 축이 원인인지에 따라 더 구체적인 알람 선택
                                if (!pinZSafe)
                                    return (int)AlarmKeys.eInputStageEjectorPinZNotSafe;
                                if (!ejectorZSafe)
                                    return (int)AlarmKeys.eInputStageEjectorZNotSafe;
                                // 둘 다 아니면 일반 반환
                                return (int)AlarmKeys.eInputStageEjectorZNotSafe;
                            }
                        }

                        // 범위 내 이동이라도 PinZ / EjectorZ 가 안전하지 않으면 알람(보수적 정책) →
                        // Test 후에 필요 시 주석 처리 해야함.
                        //if (!pinZSafe)
                        //    return (int)AlarmKeys.eInputStageEjectorPinZNotSafe;
                        //if (!ejectorZSafe)
                        //    return (int)AlarmKeys.eInputStageEjectorZNotSafe;
                    }
                }

                // 2) DieTransfer PickZ Safety
                if (InputDieTransfer != null && !InputDieTransfer.IsDieTransferPickZSafetyPos())
                    return (int)AlarmKeys.eDieTransferPickZNotSafe;

                // 3) Feeder Z / Y Safety
                if (InputFeeder != null)
                {
                    if (!InputFeeder.IsFeederZSafetyPosition())
                        return (int)AlarmKeys.eInputFeederCylinderZNotSafe;

                    if (!InputFeeder.IsFeederYSafetyPosition())
                        return (int)AlarmKeys.eInputFeederYNotSafe;
                }

                // 추가로 "다른 유닛 축 이동중" 등을 넣고 싶다면 여기서 검사 후 알람 코드 반환
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                // 예외 발생 시 보수적으로 이동 중단하도록 임의 알람 (PinZ 알람 선택) 반환 가능
                return (int)AlarmKeys.eInputStageEjectorPinZNotSafe;
            }

            return 0; // 0 = OK
        }

        /// <summary>
        /// PinZ / EjectorZ 가 Safety 가 아닐 때 X/Y 축 이동 허용 윈도우 판정.
        /// CenterPoint 티칭 기준 ±UnsafeHalfRange 범위 내만 허용.
        /// 티칭 없거나 좌표 취득 실패 시 false(=허용 안 함).
        /// </summary>
        private bool IsAllowedXYWindowWhileEjectorUnsafe()
        {
            double UnsafeHalfRangeX = Config.dSafeHalfRangeX; // mm (필요 시 Config 로 승격)
            double UnsafeHalfRangeY = Config.dSafeHalfRangeY; // mm

            // CenterPoint Teaching 확보
            var tp = Config.GetTeachingPosition(InputStageConfig.TeachingPositionName.CenterPoint.ToString());
            if (tp == null || tp.AxisPositions == null)
                return false;

            double centerX, centerY;
            if (!tp.AxisPositions.TryGetValue(AxisNames.WaferStageX, out centerX))
                return false;
            if (!tp.AxisPositions.TryGetValue(AxisNames.WaferStageY, out centerY))
                return false;

            double curX = AxisX?.GetPosition() ?? centerX;
            double curY = AxisY?.GetPosition() ?? centerY;

            bool xOk = Math.Abs(curX - centerX) <= UnsafeHalfRangeX;
            bool yOk = Math.Abs(curY - centerY) <= UnsafeHalfRangeY;

            return xOk && yOk;
        }
        
        //protected override MotionAxis ResolveAxis(string name)
        //{
        //    // 특수 축 우선 매핑 후
        //    return base.ResolveAxis(name);
        //}


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

        /// //////////////////////////////////////////////////////////////////////////////////////////////
        // UI, sequence 용 Move 함수
        public int MoveTeachingPositionOnce(InputStageConfig.TeachingPositionName name, bool isFine)
        {
            return MoveTeachingPositionOnce((int)name, isFine);
        }
        
        public int MoveToStageCenterPosition(bool isFine = false)
        {
            Task<int> task = MoveToStageCenterPositionAsync();
            while (IsEndTask(task) == false)
            {
                // Check Interlock.!!! 구문 넣을것.!!!
                if (!InputStageEjector.IsPinZSafetyPos())
                {
                    this.AxisX.EmgStop();
                    this.AxisY.EmgStop();
                    this.AxisT.EmgStop();
                    AlarmPost((int)AlarmKeys.eInputStageEjectorPinZNotSafe);
                    return -1;
                }

                if (!InputStageEjector.IsEjectorZSafetyPos())
                {
                    this.AxisX.EmgStop();
                    this.AxisY.EmgStop();
                    this.AxisT.EmgStop();
                    AlarmPost((int)AlarmKeys.eInputStageEjectorZNotSafe);
                    return -1;
                }

                // DieTransfer PickZ Safety
                if (!InputDieTransfer.IsDieTransferPickZSafetyPos())
                {
                    this.AxisX.EmgStop();
                    this.AxisY.EmgStop();
                    this.AxisT.EmgStop();
                    AlarmPost((int)AlarmKeys.eDieTransferPickZNotSafe);
                    return -1;
                }

                if (!InputFeeder.IsFeederZSafetyPosition())
                {
                    this.AxisX.EmgStop();
                    this.AxisY.EmgStop();
                    this.AxisT.EmgStop();
                    AlarmPost((int)AlarmKeys.eInputFeederCylinderZNotSafe);
                    return -1;
                }

                if(!InputFeeder.IsFeederYSafetyPosition())
                {
                    this.AxisX.EmgStop();
                    this.AxisY.EmgStop();
                    this.AxisT.EmgStop();
                    AlarmPost((int)AlarmKeys.eInputFeederYNotSafe);
                    return -1;
                }

                Thread.Sleep(0);
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
        
        
        public int MoveToStageLoadPosition(bool isFine = false)
        {
            Task<int> task = MoveToStageLoadPositionAsync();
            while (IsEndTask(task) == false)
            {
                // Check Interlock.!!! 구문 넣을것.!!!
                if (!InputStageEjector.IsPinZSafetyPos())
                {
                    this.AxisX.EmgStop();
                    this.AxisY.EmgStop();
                    this.AxisT.EmgStop();
                    AlarmPost((int)AlarmKeys.eInputStageEjectorPinZNotSafe);
                    return -1;
                }

                if (!InputStageEjector.IsEjectorZSafetyPos())
                {
                    this.AxisX.EmgStop();
                    this.AxisY.EmgStop();
                    this.AxisT.EmgStop();
                    AlarmPost((int)AlarmKeys.eInputStageEjectorZNotSafe);
                    return -1;
                }

                // DieTransfer PickZ Safety
                if (!InputDieTransfer.IsDieTransferPickZSafetyPos())
                {
                    this.AxisX.EmgStop();
                    this.AxisY.EmgStop();
                    this.AxisT.EmgStop();
                    AlarmPost((int)AlarmKeys.eDieTransferPickZNotSafe);
                    return -1;
                }
                if(Config.IsSimulation)
                {
                    //Simulation - ok
                }
                else if (!InputFeeder.IsFeederZSafetyPosition())
                {
                    this.AxisX.EmgStop();
                    this.AxisY.EmgStop();
                    this.AxisT.EmgStop();
                    AlarmPost((int)AlarmKeys.eInputFeederCylinderZNotSafe);
                    return -1;
                }

                if (!InputFeeder.IsFeederYSafetyPosition())
                {
                    this.AxisX.EmgStop();
                    this.AxisY.EmgStop();
                    this.AxisT.EmgStop();
                    AlarmPost((int)AlarmKeys.eInputFeederYNotSafe);
                    return -1;
                }

                Thread.Sleep(0);
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


        public int MoveToStageUnloadPosition(bool isFine = false)
        {
            Task<int> task = MoveToStageUnloadPositionAsync();
            while (IsEndTask(task) == false)
            {
                // Check Interlock.!!! 구문 넣을것.!!!
                if (!InputStageEjector.IsPinZSafetyPos())
                {
                    this.AxisX.EmgStop();
                    this.AxisY.EmgStop();
                    this.AxisT.EmgStop();
                    AlarmPost((int)AlarmKeys.eInputStageEjectorPinZNotSafe);
                    return -1;
                }

                if (!InputStageEjector.IsEjectorZSafetyPos())
                {
                    this.AxisX.EmgStop();
                    this.AxisY.EmgStop();
                    this.AxisT.EmgStop();
                    AlarmPost((int)AlarmKeys.eInputStageEjectorZNotSafe);
                    return -1;
                }

                // DieTransfer PickZ Safety
                if (!InputDieTransfer.IsDieTransferPickZSafetyPos())
                {
                    this.AxisX.EmgStop();
                    this.AxisY.EmgStop();
                    this.AxisT.EmgStop();
                    AlarmPost((int)AlarmKeys.eDieTransferPickZNotSafe);
                    return -1;
                }

                if (!InputFeeder.IsFeederZSafetyPosition())
                {
                    this.AxisX.EmgStop();
                    this.AxisY.EmgStop();
                    this.AxisT.EmgStop();
                    AlarmPost((int)AlarmKeys.eInputFeederCylinderZNotSafe);
                    return -1;
                }

                if (!InputFeeder.IsFeederYSafetyPosition())
                {
                    this.AxisX.EmgStop();
                    this.AxisY.EmgStop();
                    this.AxisT.EmgStop();
                    AlarmPost((int)AlarmKeys.eInputFeederYNotSafe);
                    return -1;
                }

                Thread.Sleep(0);
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


        public int MoveToStageReadyPosition(bool isFine = false)
        {
            Task<int> task = MoveToStageReadyPositionAsync();
            while (IsEndTask(task) == false)
            {
                // Check Interlock.!!! 구문 넣을것.!!!
                if (!InputStageEjector.IsPinZSafetyPos())
                {
                    this.AxisX.EmgStop();
                    this.AxisY.EmgStop();
                    this.AxisT.EmgStop();
                    AlarmPost((int)AlarmKeys.eInputStageEjectorPinZNotSafe);
                    return -1;
                }

                if (!InputStageEjector.IsEjectorZSafetyPos())
                {
                    this.AxisX.EmgStop();
                    this.AxisY.EmgStop();
                    this.AxisT.EmgStop();
                    AlarmPost((int)AlarmKeys.eInputStageEjectorZNotSafe);
                    return -1;
                }

                // DieTransfer PickZ Safety
                if (!InputDieTransfer.IsDieTransferPickZSafetyPos())
                {
                    this.AxisX.EmgStop();
                    this.AxisY.EmgStop();
                    this.AxisT.EmgStop();
                    AlarmPost((int)AlarmKeys.eDieTransferPickZNotSafe);
                    return -1;
                }

                if (!InputFeeder.IsFeederZSafetyPosition())
                {
                    this.AxisX.EmgStop();
                    this.AxisY.EmgStop();
                    this.AxisT.EmgStop();
                    AlarmPost((int)AlarmKeys.eInputFeederCylinderZNotSafe);
                    return -1;
                }

                if (!InputFeeder.IsFeederYSafetyPosition())
                {
                    this.AxisX.EmgStop();
                    this.AxisY.EmgStop();
                    this.AxisT.EmgStop();
                    AlarmPost((int)AlarmKeys.eInputFeederYNotSafe);
                    return -1;
                }

                Thread.Sleep(0);
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


        public bool InPosTeaching(string name)
        {
            var (t, pz, plz) = Config.GetPositionWithOffset(name);
            return InPos(_axX, t) && InPos(_axY, pz) && InPos(_axT, plz);
        }
        /// <summary>
        /// 지정한 Teaching Position에서 특정 축만 InPosition 여부를 확인.
        /// - T / PickZ / PlaceZ 는 Offset 적용 값을 사용
        /// - 그 외 축 이름이 오면 TeachingPosition.AxisPositions 값 그대로 비교
        /// </summary>
        /// <param name="tpName">Teaching Position 이름</param>
        /// <param name="axisName">
        /// 확인할 축 키(or 이름). 예:
        ///   AxisNames.LeftToolT / AxisNames.LeftPickZ / AxisNames.LeftPlaceZ
        /// </param>
        /// <returns>true = 지정 축이 목표 위치(InPositionTolerance 내)에 있음</returns>
        public bool InPosTeachingAxis(string tpName, string axisName)
        {
            if (string.IsNullOrEmpty(tpName) || string.IsNullOrEmpty(axisName)) return false;

            var tp = Config.GetTeachingPosition(tpName);
            if (tp == null) return false;

            // 표준 3축(T / PickZ / PlaceZ) 은 Offset 반영된 위치 사용
            var (t, pz, plz) = Config.GetPositionWithOffset(tpName);
            if (string.Equals(axisName, AxisNames.WaferStageX, StringComparison.OrdinalIgnoreCase))
                return InPos(_axX, t);
            if (string.Equals(axisName, AxisNames.WaferStageY, StringComparison.OrdinalIgnoreCase))
                return InPos(_axY, pz);
            if (string.Equals(axisName, AxisNames.WaferStageT, StringComparison.OrdinalIgnoreCase))
                return InPos(_axT, plz);

            // 기타 축 처리: TeachingPosition에 저장된 원본 값 사용 (Offset 미적용)
            MotionAxis axis = null;
            if (tp.Axes != null && tp.Axes.TryGetValue(axisName, out var direct)) axis = direct;
            if (axis == null && Axes.TryGetValue(axisName, out var unitAxis)) axis = unitAxis;
            if (axis == null)
            {
                // Name 기준 추가 검색
                foreach (var kv in Axes)
                {
                    if (kv.Value != null &&
                        string.Equals(kv.Value.Name, axisName, StringComparison.OrdinalIgnoreCase))
                    {
                        axis = kv.Value; break;
                    }
                }
            }
            if (axis == null) return false;

            double target = tp.GetAxisPosition(axisName, 0.0);
            return InPos(axis, target);
        }
        protected bool InPos(MotionAxis ax, double target) => ax == null || ax.InPosition(target);
        #endregion

        #region Teaching Position Move (Batch Style)
        public int MoveToTeachingPosition(string positionName, double vel = 0, double acc = 0, double dec = 0, double jerk = 0)
        {
            var tp = Config.GetTeachingPosition(positionName);
            if (tp == null) return -1;
            var (x, y, t) = Config.GetPositionWithOffset(positionName);   //Offset 포함 위치 - Align 수행 시 data 있음.
            int rc = 0;
            if (AxisX != null) rc |= AxisX.MoveAbs(x, vel > 0 ? vel : AxisX.Config.MaxVelocity, acc > 0 ? acc : AxisX.Config.RunAcc, dec > 0 ? dec : AxisX.Config.RunDec, jerk > 0 ? jerk : AxisX.Config.AccJerkPercent);
            if (AxisY != null) rc |= AxisY.MoveAbs(y, vel > 0 ? vel : AxisY.Config.MaxVelocity, acc > 0 ? acc : AxisY.Config.RunAcc, dec > 0 ? dec : AxisY.Config.RunDec, jerk > 0 ? jerk : AxisY.Config.AccJerkPercent);
            if (AxisT != null) rc |= AxisT.MoveAbs(t, vel > 0 ? vel : AxisT.Config.MaxVelocity, acc > 0 ? acc : AxisT.Config.RunAcc, dec > 0 ? dec : AxisT.Config.RunDec, jerk > 0 ? jerk : AxisT.Config.AccJerkPercent);
            return rc;
        }
        public bool InPosTeaching(TeachingPosition tp)
        {
            if (tp == null)
                return false;
            return InPosTeaching(tp.Name);
        }
        public double GetTP(string tpName, string axisName)
        {
            var tp = Config.GetTeachingPosition(tpName);
            if (tp != null && tp.AxisPositions != null && tp.AxisPositions.TryGetValue(axisName, out var v)) return v;
            return 0.0;
        }
        public double GetTP(TeachingPosition tp, string axisName) => (tp == null || string.IsNullOrEmpty(axisName)) ? 0.0 : (tp.AxisPositions.TryGetValue(axisName, out var v) ? v : 0.0);
        public double GetTP(TeachingPosition tp, MotionAxis axis) => axis == null ? 0.0 : GetTP(tp, axis.Name);
        #endregion

        #region Low-Level IO Access (Refactored to match OutputStage pattern)
        public bool ReadInput(string name)
        {
            // 유효성 검사
            if (string.IsNullOrWhiteSpace(name))
            {
                return false;
            }

            if (Config == null || Config.HardInputs == null)
            {
                return false;
            }

            // 정의된 하드웨어 입력 목록에서 이름 매칭
            var hi = Config.HardInputs.FirstOrDefault(i =>
                i != null &&
                i.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

            if (hi == null)
            {
                return false;
            }

            // 설비 / DIO 스캐너 참조
            var eq = Equipment.Instance;
            if (eq == null)
            {
                return false;
            }

            var dio = eq.DioScan;
            if (dio == null)
            {
                return false;
            }

            // 모듈 순회하며 입력 값 조회
            if (eq.UnitIO != null && eq.UnitIO.Modules != null)
            {
                foreach (var module in eq.UnitIO.Modules)
                {
                    if (module == null)
                    {
                        continue;
                    }

                    bool value;
                    if (dio.TryGetInput(module.ModuleName, hi.Disp, out value))
                    {
                        return value;
                    }
                }
            }
            return false;
        }
        public bool WriteOutput(string name, bool on)
        {
            var ho = Config.HardOutputs.FirstOrDefault(o => o.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (ho == null) return false;
            var eq = Equipment.Instance; var dio = eq?.DioScan; if (dio == null) return false;
            foreach (var m in eq.UnitIO.Modules)
                if (dio.WriteOutput(m.ModuleName, ho.Disp, on) == 0) return true;
            return false;
        }
        public bool IsOutputOn(string name)
        {
            var ho = Config.HardOutputs.FirstOrDefault(o => o.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (ho == null) return false;
            var eq = Equipment.Instance; var dio = eq?.DioScan; if (dio == null) return false;
            foreach (var m in eq.UnitIO.Modules)
                if (dio.TryGetOutput(m.ModuleName, ho.Disp, out var v)) return v;
            return false;
        }
        #endregion

        #region IO Domain Mapping (Reorganized)
        private Cylinder _cylClampLift;      // Lift Up/Down
        private Cylinder _cylClampFB;
        private Cylinder _cylPlate;       // Expander Up/Down
        private Vacuum _vacuum;              // Vacuum + OK sensor

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

            if (!IoAutoBindings.Cylinders.TryGetValue("InStageClampLift", out _cylClampLift))
            {
                Log.Write("InputStage", "BindIoDomains", "Cylinder not found: InStageClampLift");
            }

            if (!IoAutoBindings.Cylinders.TryGetValue("InStageClampFB", out _cylClampFB))
            {
                Log.Write("InputStage", "BindIoDomains", "Cylinder not found: InStageClampFB");
            }
        }
        // === Domain Control (표준 구동) ===
        public bool SetVacuum(bool on, bool bCheckSignal = false)
        {
            if (_vacuum == null) 
                return false;

            if(!bCheckSignal)
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
                if (!IsClampLiftUp()) return false; // 기존 인터락 유지
                return _cylClampFB.Extend();
            }
            else return _cylClampFB.Retract();
        }
        #region High-Level Actuator API (Interlock 포함)
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

            return ReadInput(InputStageConfig.IO.CLAMP_DOWN_SNS);
        }
        public bool IsClampFwd()
        {
            if(Config.IsSimulation)
            {
                return true;
            }

            // Clamp Forward 센서 (클램프 전진 상태) 확인
            return ReadInput(InputStageConfig.IO.CLAMP_FWD_SNS);
        }
        public bool IsClampBwd()
        {
            if (Config.IsSimulation)
            {
                return true;
            }

            return !IsClampFwd();
        }
        public bool Ring0()
        {
            if (Config.IsSimulation)
            {
                return true;
            }

            return ReadInput(InputStageConfig.IO.RING_CHECK0);
        }
        public bool Ring1()
        {
            if (Config.IsSimulation)
            {
                return true;
            }

            return ReadInput(InputStageConfig.IO.RING_CHECK1);
        }
        public bool IsRingPresent()
        {
            if (Config.IsSimulation)
            {
                return true;
            }

            return Ring0() || Ring1();
        }
        #endregion

        // === Direct Valve Control (강제 구동) ===
        public bool IsVacuumValveOn()
        {
            if (Config.IsSimulation)
            {
                return true;
            }
            return IsOutputOn(InputStageConfig.IO.VAC_OUT);
        }
        public bool IsClampLiftUpValveOn()
        {
            if (Config.IsSimulation)
            {
                return true;
            }
            return IsOutputOn(InputStageConfig.IO.CLAMP_UP_OUT);
        }
        public bool IsVacuumOn()
        {
            if (Config.IsSimulation)
            {
                return true;
            }
            return ReadInput(InputStageConfig.IO.VAC_OK_SNS);
        }
        
        public bool IsPlateUp()
        {
            if (Config.IsSimulation)
            {
                return true;
            }
            return ReadInput(InputStageConfig.IO.EXPANDER_UP_SNS);
        }
        
        public bool IsPlateDown()
        {
            if (Config.IsSimulation)
            {
                return true;
            }
            return ReadInput(InputStageConfig.IO.EXPANDER_DOWN_SNS);
        }
        #endregion

        // 파라미터로 빼야하는 Data 및 상수
        public int MoveTimeoutMs { get; set; } = 6000;
        public int PollIntervalMs { get; set; } = 30;
        public double AngleIgnoreThresholdDeg { get; set; } = 0.001;
        public double AngleMaxApplyDeg { get; set; } = 2.0;
        public double AngleApplyGain { get; set; } = 1.0; // 방향 반전 필요 시 -1 사용
        public bool UseOffsetForTAxisCorrection { get; set; } = true; // false면 직접 축 이동 방식으로 전환 가능 (추후 확장)

        private int WaitUntil(Func<bool> cond, int timeoutMs)
        {
            int nRtn = -1;
            var sw = Stopwatch.StartNew();
            while (sw.ElapsedMilliseconds < timeoutMs)
            {
                if (cond()) return nRtn;
                Thread.Sleep(PollIntervalMs);
            }

            nRtn = 0;
            return nRtn;
        }
        private bool WaitIO(Func<bool> cond, int timeoutMs)
        {
            var sw = Stopwatch.StartNew();
            while (sw.ElapsedMilliseconds < timeoutMs)
            {
                if (cond()) return true;
                Thread.Sleep(PollIntervalMs);
            }
            return false;
        }


        #region Seq 단위 동작 함수

        public MaterialWafer GetWaferMaterial()
        {
            var mat = GetMaterial();
            return mat as MaterialWafer;
        }

        public double MaxXYOffsetMm { get; set; } = 2.0;   // XY 최대 보정 허용치 (mm)
        public bool IsStatus_RequestWafer { get; internal set; } = false;

        // === Stage Load/Unload 상태 플래그 (RingTransfer 와 핸드쉐이크 용 가정) ===
        public bool IsStatus_StageLoadingReady { get; private set; }
        public bool IsStatus_StageLoadingDone { get; private set; }
        public bool IsStatus_StageUnloadingReady { get; private set; }
        public bool IsStatus_StageUnloadingDone { get; private set; }
        public bool IsStatus_CompleteWorking
        {
            get
            {
                MaterialWafer mat = GetWaferMaterial();
                if (mat == null)
                {
                    return false;
                }
                if (mat.Presence == Material.MaterialPresence.Exist)
                {
                    return mat.ProcessSatate == Material.MaterialProcessSatate.Completed;
                }
                return false;
            }
            internal set
            {
            }
        }


        // ====== Align Refactor: 상태/결과 보관 필드 ======
        public bool IsStatus_TAlignPrepared { get; private set; }
        public bool IsStatus_TAlignDone { get; private set; }
        public double IsStatus_LastFoundTRawAngle { get; private set; }
        public double IsStatus_LastAppliedTAngle { get; private set; }
        public bool IsStatus_XYAlignPrepared { get; private set; }
        public bool IsStatus_XYAlignDone { get; private set; }
        public double IsStatus_LastFoundDx { get; private set; }
        public double IsStatus_LastFoundDy { get; private set; }




        public override int OnRun()
        {
            int ret = 0;

            if (this.Status == UnitRunStatus.Stop || this.Status == UnitRunStatus.CycleStop)
            {
                this.State = ProcessState.Stop;
                return 1;
            }

            switch (State)
            {
                case ProcessState.Ready:
                    ret = OnRunReady();
                    break;
                case ProcessState.Work:
                    ret = OnRunWork();
                    break;
                case ProcessState.Complete:
                    ret = OnRunComplete();
                    break;
                default:
                    //IsStatus_StageLoadingReady = false;
                    //IsStatus_StageLoadingDone = false;
                    this.State = ProcessState.Ready;
                    break;
            }
            if (ret != 0)
            {
                this.State = ProcessState.Stop;
                this.OnStop();
            }

            return ret;
        }
        public override int OnStop()
        {
            int ret = 0;
            State = ProcessState.Stop;
            base.OnStop();
            return ret;
        }
        protected override int OnRunReady()
        {
            int ret = 0;

            // 이미 웨이퍼 존재하면 준비 단계 불필요 (바로 Work 단계 가능)
            if (IsRingPresent())
            {
                //Plate Up → 
                SetClampPlate(true);
                if(!IsPlateUp())
                {
                    Log.Write(this, "Fail: PlateUp");
                    return -1;
                }

                int rc = LoadingWaferComplete();
                if (rc != 0 && rc != 0)
                    return rc; // rc !=0 이면 오류. (준비단계는 OK=0 외 다른 코드 없음)

                IsStatus_StageLoadingDone = true;

                State = ProcessState.Work;
                Log.Write(this, "Wafer already present -> Skip prepare");
                return 0;
            }
            //else if (!InputFeeder.IsRequestLoadingWafer)
            //{
            //    return 0;
            //}
            else
            {
                IsStatus_RequestWafer = true;
                ret = LoadingWafer();
                if (ret != 0)
                {
                    State = ProcessState.Error;
                    Log.Write(this, "LoadingWafer Failed");
                    return -1;
                }
                else
                {
                    IsStatus_RequestWafer = false;
                    IsStatus_StageLoadingDone = true;
                    State = ProcessState.Work;
                }
            }

            return 0;
        }
        protected override int OnRunWork()
        {
            int ret = 0;

            ret = AlignT();
            if (ret != 0)
            {
                State = ProcessState.Error;
                Log.Write(this, "AlignT Failed");
                return -1;
            }
            else
            {
                ret = AlignXY();
                if (ret != 0)
                {
                    State = ProcessState.Error;
                    Log.Write(this, "AlignXY Failed");
                    return -1;
                }
                else
                {
                    // === Chip Mapping 추가 ===
                    //ret = PerformChipMapping();

                    //TEST필요.
                    //ret = PerformChipMappingV2();
                    //if (ret != 0)
                    //{
                    //    State = ProcessState.Error;
                    //    Log.Write(this, "Chip Mapping Failed");
                    //    return -1;
                    //}

                    State = ProcessState.Complete;
                    return 0;
                }
            }
        }
        protected override int OnRunComplete()
        {
            int ret = 0;

            ret = UnloadingWafer();
            if (ret != 0)
            {
                State = ProcessState.Error;
                Log.Write(this, "UnloadingWafer Failed");
                return -1;
            }

            State = ProcessState.None;
            return 0;
        }

        // 주석   
        /* TODO */
        //웨이퍼 있냐 없냐? 
        // Ring check
        //있으면
        //나가는거고. 
        //없으면
        //인터락 - 외부 유닛 위치 확인
        //스테이지이젝터핀 Z축
        //다이트렌스퍼 Z축
        //링피커 - 실린더 Up 유무
        //웨이퍼 로딩 위치 이동.
        //실린더 Plate Down
        //실린더 백 -> 다운
        //웨이퍼 로딩 준비 완료 플래그 ON
        // 링피커가 로딩 했다는 신호 주면 
        // Plate Up
        // 실린더 Up
        // 실린더 전진
        //인터락 - 외부 유닛 위치 확인
        //스테이지이젝터핀 Z축
        //다이트렌스퍼 Z축
        //링피커 - 실린더 Up 유무
        //스테이지 센터 이동.
        //스테이지 로딩 완료 플래그 ON ?
        // 반환 코드 규약 (선택적): 0 = OK, 1 = 대기(조건 미충족), -1 = 오류
        public int LoadingWaferPrepare()
        {
            int ret = 0;

            Log.Write(this, "Start LoadingWaferPrepare");
            IsStatus_StageLoadingReady = true;
            IsStatus_StageLoadingDone = false;
            
            // 이미 웨이퍼 존재하면 준비 단계 불필요 (바로 완료 단계 가능)
            if(Config.IsSimulation)
            {

            }
            else if (IsRingPresent())
            {
                Log.Write(UnitName, "LoadingPrep", "Wafer already present -> Skip prepare");
                return 0;
            }

            // 로딩 Teaching 이동
            ret = MoveToStageLoadPosition();
            if(ret != 0)
            {
                Log.Write(this, "Fail: Move Load");
                return ret;
            }

            // Clamp Back → Lift Down
            SetClampFB(false);
            if(!IsClampBwd())
            {
                Log.Write(this, "Fail: ClampBack");
                return -1;
            }
            SetClampLift(false);
            if(!IsClampLiftDown())
            {
                Log.Write(this, "Fail: ClampLiftDown");
                return -1;
            }
            //Plate Up → 
            SetClampPlate(false);
            if(!IsPlateDown())
            {
                Log.Write(this, "Fail: PlateUp");
                return -1;
            }
            
            IsStatus_StageLoadingReady = true;
            Log.Write(UnitName, "LoadingPrep", "StageLoadingReady = TRUE (Wait wafer)");

            Log.Write(this, "End LoadingWaferPrepare");
            return 0;
        }
        public int LoadingWaferComplete()
        {
            int ret = 0;

            // 이미 완료
            if (IsStatus_StageLoadingDone)
                return 0;

            // 준비 안 되었으면 호출 순서 오류
            if (!IsStatus_StageLoadingReady && !IsRingPresent())
            {
                Log.Write(UnitName, "LoadingComp", "Not prepared (call LoadingWaferPrepare first)");
                return -1;
            }

            // 아직 Wafer 안 올라옴 → 대기
            bool bRtn = Config.IsSimulation;
            if (IsRingPresent() || bRtn)
            {
                Log.Write(UnitName, "LoadingComp", "Wafer detected -> Completing");

                if(Config.IsSimulation)
                {
                    Thread.Sleep(1000);
                }
                else if (!IsPlateUp())
                {
                    SetClampPlate(true);
                    if (!IsPlateUp())
                    {
                        Log.Write(this, "Fail: PlateUp");
                        return -1;
                    }

                    SetClampLift(true);
                    if (!IsClampLiftUp())
                    {
                        Log.Write(this, "Fail: ClampLiftUp");
                        return -1;
                    }

                    SetClampFB(true);
                    if (!IsClampFwd())
                    {
                        Log.Write(this, "Fail: ClampForward");
                        return -1;
                    }
                }
                else
                {
                    Log.Write(UnitName, "LoadingComp", "Not IsPlateUp");
                    return -1;
                }

                // 센터 Teaching 이동
                ret = MoveToStageCenterPosition();
                if (ret != 0)
                {
                    Log.Write(this, "Fail: Move Load");
                    return ret;
                }

                IsStatus_StageLoadingDone = true;
                IsStatus_StageLoadingReady = false;
                Log.Write(UnitName, "LoadingComp", "Done");

                return 0;
            }
            else
            {
                // 우선 대기? // 신호 이상?
                return -1;
            }

            return ret;
        }
        // 기존 일괄 함수(호환 유지 용). 필요 없으면 제거 가능.
        public int LoadingWafer()
        {
            int rc = LoadingWaferPrepare();
            if (rc != 0)
                return rc; // rc !=0 이면 오류. (준비단계는 OK=0 외 다른 코드 없음)
            
            // Ring 대기
            if(Config.IsSimulation)
            {

            }
            else if (!IsRingPresent())
            {
                if (!WaitIO(() => IsRingPresent(), MoveTimeoutMs))
                    return -1;
            }

            return LoadingWaferComplete();
        }

        private TeachingPosition _lastCenterAlignTp;
        /// <summary>
        /// 공통 Center 이동 + Grab (기존 함수 그대로 사용)
        /// </summary>
        private int PrepareForAlign(out TeachingPosition centerTp, out VisionImage img)
        {
            int nRtn = -1;

            centerTp = null;
            img = null;

            // 1) 인터락
            if (!IsRingPresent())
            {
                Log.Write(UnitName, "Align", "Fail: Ring(Wafer) not present");
                return -1;
            }
            if (!IsClampLiftUp())
            {
                Log.Write(UnitName, "Align", "Fail: Clamp Lift not Up");
                return -1;
            }

            if (!IsClampFwd())
            {
                Log.Write(UnitName, "Align", "Fail: Clamp not FWD");
                return -1;
            }

            nRtn = MoveToStageCenterPosition();
            if(nRtn != 0)
            {
                Log.Write(UnitName, "Align", "Fail: Move Center");
                return -1;
            }

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

            int grabRc;
            try
            {
                // 4) 카메라 그랩
                if (StageCamera == null)
                {
                    Log.Write(UnitName, "Align", "Fail: Camera null");
                    return -1;
                }
                grabRc = StageCamera.GrabSync(out img);
            }
            catch (Exception ex)
            {
                Log.Write(UnitName, "Align", "Exception: " + ex.Message);
                return -1;
            }

            if(Config.IsSimulation)
            {

            }
            else if (grabRc != 0 || img == null || img.RawData == null)
            {
                Log.Write(UnitName, "Align", $"Fail: Grab fail rc={grabRc}");
                img?.Dispose();
                img = null;
                return -1;
            }

            StageCamera.LatestImage = img;
            Log.Write(UnitName, "Align", "Grab OK");
            return 0;
        }
        
        // ===================== T ALIGN 분리 =====================
        /// <summary>
        /// T 정렬 준비 + Vision 각도 리스트 획득/통계 산출
        /// </summary>
        public int AlignTPrepare()
        {
            IsStatus_TAlignPrepared = false;
            IsStatus_TAlignDone = false;
            IsStatus_LastFoundTRawAngle = 0;
            IsStatus_LastAppliedTAngle = 0;
            _lastCenterAlignTp = null;

            Log.Write(UnitName, "T_Align", "Prepare Start");

            if (PrepareForAlign(out var centerTp, out var _img) != 0)
            {
                return -1;

            }

            if (!TryGetMultiAngles(out var angleList) || angleList == null || angleList.Count == 0)
            {
                AlarmPost((int)AlarmKeys.eVisionTsearch);
                Log.Write(UnitName, "T_Align", "Fail: Vision angle search empty");
                return -1;
            }

            var stats = ComputeAngleStats(angleList, excludeExtremes: true);
            if (stats.RawCount == 0)
            {
                Log.Write(UnitName, "T_Align", "Fail: No angle list after filtering");
                return -1;
            }

            double rawAngle = stats.Representative;
            IsStatus_LastFoundTRawAngle = rawAngle;
            _lastCenterAlignTp = centerTp;

            Log.Write(UnitName, "T_Align",
                $"Angle Representative={rawAngle:F6} avg={stats.Average:F6} std={stats.StdDev:F6} rawCount={stats.RawCount}");

            IsStatus_TAlignPrepared = true;
            return 0;
        }
        /// <summary>
        /// T 정렬 적용 (AlignTPrepare 먼저 호출)
        /// </summary>
        public int AlignTApply()
        {
            if (!IsStatus_TAlignPrepared || _lastCenterAlignTp == null)
            {
                Log.Write(UnitName, "T_Align", "Not prepared");
                return -1;
            }

            double rawAngle = IsStatus_LastFoundTRawAngle;
            if (Math.Abs(rawAngle) < AngleIgnoreThresholdDeg)
            {
                Log.Write(UnitName, "T_Align", $"Skip: |{rawAngle:F6}| < Ignore({AngleIgnoreThresholdDeg})");
                IsStatus_TAlignDone = true;
                return 0;
            }
            if (Math.Abs(rawAngle) > AngleMaxApplyDeg)
            {
                Log.Write(UnitName, "T_Align",
                    $"Fail: Angle {rawAngle:F4} > Limit {AngleMaxApplyDeg}");
                return -1;
            }

            double applyAngle = rawAngle * AngleApplyGain;
            IsStatus_LastAppliedTAngle = applyAngle;

            //int rc = UseOffsetForTAxisCorrection
            //    ? MoveApplyOffset(_lastCenterAlignTp.Name, 0.0, 0.0, applyAngle)
            //    : MoveAxisOnce(AxisT, applyAngle);
            //Log.Write(UnitName, "T_Align",
            //    $"{(UseOffsetForTAxisCorrection ? "ApplyOffset" : "DirectMove")} angle={applyAngle:F6} rc={(rc == 0 ? "OK" : "FAIL")}");
            //if (rc != 0)
            //    return -1;

            int rc = MoveApplyOffset(_lastCenterAlignTp.Name, 0.0, 0.0, applyAngle);
            if (rc != 0)
            {
                return -1;
            }

            //// 재 이동(In Offset 적용 시 Teaching 목표 재도달)
            //if (MoveToTeachingPosition(_lastCenterAlignTp) != 0)
            //    return -1;
            //if (WaitUntil(() => InPosTeaching(_lastCenterAlignTp), MoveTimeoutMs) != 0)
            //    return -1;

            IsStatus_TAlignDone = true;
            return 0;
        }
        /// <summary>
        /// 기존 호환: 한번에 실행 (Prepare + Apply)
        /// </summary>
        public int AlignT()
        {
            int rc = AlignTPrepare();
            if (rc != 0) 
                return rc;
            return AlignTApply();
        }

        // ===================== XY ALIGN 분리 =====================
        /// <summary>
        /// XY 정렬 준비 + Vision Offset 획득
        /// </summary>
        public int AlignXYPrepare()
        {
            IsStatus_XYAlignPrepared = false;
            IsStatus_XYAlignDone = false;
            IsStatus_LastFoundDx = 0;
            IsStatus_LastFoundDy = 0;
            _lastCenterAlignTp = null;

            Log.Write(UnitName, "XY_Align", "Prepare Start");

            if (PrepareForAlign(out var centerTp, out var _img) != 0)
                return -1;

            var res = CenterSearchViaRunner();
            if (!res.ok)
            {
                AlarmPost((int)AlarmKeys.eVisionXYsearch);
                Log.Write(UnitName, "XY_Align", "Fail: Vision XY offset search");
                return -1;
            }

            IsStatus_LastFoundDx = res.x;
            IsStatus_LastFoundDy = res.y;
            _lastCenterAlignTp = centerTp;

            Log.Write(UnitName, "XY_Align",
                $"Offset dx={IsStatus_LastFoundDx:F6} dy={IsStatus_LastFoundDy:F6}");

            IsStatus_XYAlignPrepared = true;
            return 0;
        }
        /// <summary>
        /// XY 정렬 적용 (AlignXYPrepare 먼저)
        /// </summary>
        public int AlignXYApply()
        {
            if (!IsStatus_XYAlignPrepared || _lastCenterAlignTp == null)
            {
                Log.Write(UnitName, "XY_Align", "Not prepared");
                return -1;
            }

            double dx = IsStatus_LastFoundDx;
            double dy = IsStatus_LastFoundDy;

            if (Math.Abs(dx) < 0.0001 && Math.Abs(dy) < 0.0001)
            {
                Log.Write(UnitName, "XY_Align", "Skip: offset under threshold");
                IsStatus_XYAlignDone = true;
                return 0;
            }
            if (Math.Abs(dx) > MaxXYOffsetMm || Math.Abs(dy) > MaxXYOffsetMm)
            {
                Log.Write(UnitName, "XY_Align",
                    $"Fail: Over limit dx={dx:F4} dy={dy:F4} limit={MaxXYOffsetMm}");
                return -1;
            }

            int rc = MoveApplyOffset(_lastCenterAlignTp.Name, dx, dy, 0.0);
            Log.Write(UnitName, "XY_Align",
                $"ApplyOffset dx={dx:F6} dy={dy:F6} rc={(rc == 0 ? "OK" : "FAIL")}");

            if (rc != 0)
                return -1;

            //if (MoveToTeachingPosition(_lastCenterAlignTp) != 0)
            //    return -1;
            //if (WaitUntil(() => InPosTeaching(_lastCenterAlignTp), MoveTimeoutMs) != 0)
            //    return -1;

            IsStatus_XYAlignDone = true;
            return 0;
        }
        /// <summary>
        /// 기존 호환: 한번에 실행 (Prepare + Apply)
        /// </summary>
        public int AlignXY()
        {
            int rc = AlignXYPrepare();
            if (rc != 0) return rc;
            return AlignXYApply();
        }

        public int ChipPickUp()
        {
            int nRet = -1;

            // Die Tr이 주인
            /* TODO */

            // Die Tr이 주는 명령대로 움직이는 함수 필요. 
            // Chip Position 위치 이동 함수. 
            // 인터락. 공정 범위 넘어가는지 확인 필요.

            return nRet;
        }

        /* TODO */
        /// <summary>
        /// 언로딩 준비 단계:
        ///  - 웨이퍼 없으면 즉시 Done 처리 (할 것 없음)
        ///  - Unloading Teaching 이동
        ///  - Plate Up / Clamp Back / Lift Down
        ///  - StageUnloadingReady = true (웨이퍼 픽업 대기)
        /// 반환: 0=OK, -1=오류
        /// </summary>
        public int UnloadingWaferPrepare()
        {
            int nRtn = 0;
            Log.Write(UnitName, "UnloadingPrep", "Start");
            IsStatus_StageUnloadingDone = false;
            IsStatus_StageUnloadingReady = false;

            if (!IsRingPresent())
            {
                Log.Write(UnitName, "UnloadingPrep", "No wafer -> Skip");
                IsStatus_StageUnloadingDone = true;
                return 0;
            }

            nRtn = MoveToStageUnloadPosition();
            if(nRtn != 0)
            {
                return -1;
            }

            // Plate Up (이미 Up 일 수도 있으나 통일)
            SetClampPlate(true);
            if (!IsPlateUp())
            {
                Log.Write(this, "Fail: PlateUp");
                return -1;
            }
            SetClampFB(false);
            if (!IsClampBwd())
            {
                Log.Write(this, "Fail: ClampBack");
                return -1;
            }
            SetClampLift(false);
            if (!IsClampLiftDown())
            {
                Log.Write(this, "Fail: ClampLiftDown");
                return -1;
            }

            IsStatus_StageUnloadingReady = true;
            Log.Write(UnitName, "UnloadingPrep", "StageUnloadingReady = TRUE (Wait wafer pick)");
            return 0;
        }
        /// <summary>
        /// 언로딩 완료 단계:
        ///  - 웨이퍼 아직 있으면 1(대기)
        ///  - 제거된 경우 Plate Down
        ///  - Optional: Ready 포지션 복귀
        ///  - StageUnloadingDone = true
        /// 반환: 0=완료, 1=대기, -1=오류
        /// </summary>
        public int UnloadingWaferComplete()
        {
            int nRtn = 0;

            if (IsStatus_StageUnloadingDone)
                return 0;

            if (!IsStatus_StageUnloadingReady && !IsRingPresent())
            {
                Log.Write(UnitName, "UnloadingComp", "Not prepared");
                return -1;
            }

            if (!IsRingPresent())
                return -1; // 아직 픽업 안됨

            Log.Write(UnitName, "UnloadingComp", "Wafer removed -> Completing");

            // Plate Down (원위치)
            SetClampPlate(false);
            if(IsPlateDown())
            {
                return -1;
            }

            nRtn = MoveToStageReadyPosition();
            if (nRtn != 0)
            {
                return -1;
            }

            IsStatus_StageUnloadingDone = true;
            IsStatus_StageUnloadingReady = false;
            Log.Write(UnitName, "UnloadingComp", "Done");
            return 0;
        }
        /// <summary>
        /// 기존 단일 호출 방식 (호환용).
        ///  - Prepare 수행
        ///  - 웨이퍼 존재 시 제거될 때까지 대기
        ///  - Complete 수행
        /// </summary>
        public int UnloadingWafer()
        {
            int rc = UnloadingWaferPrepare();
            if (rc != 0) return rc; // 0 아니면 오류 (언로딩은 대기코드 없음)

            // 웨이퍼 있었다면 제거 대기
            if (IsRingPresent())
            {
                if (!WaitIO(() => !IsRingPresent(), MoveTimeoutMs))
                {
                    Log.Write(UnitName, "Unloading", "Fail: Wafer not removed (timeout)");
                    return -1;
                }
            }
            return UnloadingWaferComplete();
        }
        public bool IsWaferLoadingPosition()
        {
            var tp = TeachingPositions[(int)InputStageConfig.TeachingPositionName.Loading];
            if (tp == null) return false;
            return InPosTeaching(tp);
        }
        #endregion



        #region CHIP MAPPING / PICKUP

        // 매핑 파라미터 (Config 로 승격 가능)
        public double MappingRoiWidthMm { get; set; } = 50.0;
        public double MappingRoiHeightMm { get; set; } = 50.0;
        public double ChipPitchXmm { get; set; } = 5.0;
        public double ChipPitchYmm { get; set; } = 5.0;
        public double DuplicateDistMm { get; set; } = 0.8;          // 중복 판단
        public double MarkMinScore { get; set; } = 0.6;             // Vision 점수 기준 (예시)
        public double MissingAllowScore { get; set; } = 0.5;
        public int MappingMoveTimeoutMs { get; set; } = 4000;
        public bool UseVisionOffsetApply { get; set; } = false;   // 필요시 Vision 미세 중심 보정

        public ChipMapResult CurrentChipMap { get; private set; }
        public bool ChipMappingDone { get; private set; }
        private int _chipPickupCursor = 0;

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

        public int PerformChipMapping()
        {
            ChipMappingDone = false;
            CurrentChipMap = null;

            // 기본 인터락
            if (!IsStatus_TAlignDone || !IsStatus_XYAlignDone)
            {
                Log.Write(UnitName, "ChipMap", "Align not completed");
                return -1;
            }
            if (!IsRingPresent())
            {
                Log.Write(UnitName, "ChipMap", "Wafer (Ring) not present");
                return -1;
            }

            // Center Teaching
            var centerTp = Config.GetTeachingPosition(InputStageConfig.TeachingPositionName.CenterPoint.ToString());
            if (centerTp == null)
            {
                Log.Write(UnitName, "ChipMap", "Center Teaching not found");
                return -1;
            }
            var (baseX, baseY, baseT) = Config.GetPositionWithOffset(centerTp.Name);

            // ROI 그리드계산
            if (ChipPitchXmm <= 0 || ChipPitchYmm <= 0)
                return -1;

            int cols = (int)Math.Floor(MappingRoiWidthMm / ChipPitchXmm) + 1;
            int rows = (int)Math.Floor(MappingRoiHeightMm / ChipPitchYmm) + 1;
            if (rows <= 0 || cols <= 0) return -1;

            double leftTopX = baseX - (MappingRoiWidthMm * 0.5);
            double leftTopY = baseY + (MappingRoiHeightMm * 0.5); // 좌표계 방향(Y+ up/down 프로젝트 기준 확인 필요)

            var map = new ChipMapResult
            {
                Rows = rows,
                Cols = cols,
                PitchX = ChipPitchXmm,
                PitchY = ChipPitchYmm
            };

            int globalIndex = 0;
            VisionImage img = null;

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    double targetX = leftTopX + c * ChipPitchXmm;
                    double targetY = leftTopY - r * ChipPitchYmm; // 위에서 아래로

                    // Stage 이동
                    if (AxisX != null && MoveAxisPositionOne(AxisX, targetX) != 0) 
                        return -1;
                    if (AxisY != null && MoveAxisPositionOne(AxisY, targetY) != 0) 
                        return -1;
                    
                    // 우선.. 확인 하고 넘어가자. 
                    if (WaitUntil(() =>
                        AxisX.InPosition(targetX) && AxisY.InPosition(targetY),
                        MappingMoveTimeoutMs) != 0)
                    {
                        Log.Write(UnitName, "ChipMap", $"Move timeout r={r} c={c}");
                        return -1;
                    }

                    // Grab
                    if (StageCamera == null)
                        return -1;

                    if (!Config.IsSimulation)
                    {
                        if (StageCamera.GrabSync(out img) != 0 || img == null)
                        {
                            Log.Write(UnitName, "ChipMap", $"Grab fail r={r} c={c}");
                            continue;
                        }
                    }

                    // Vision 패턴 검색 (간단: MultiAngles 재사용 or CenterSearch)
                    double score = 0;
                    bool found = false;
                    double visionDx = 0, visionDy = 0;

                    if (Config.IsSimulation)
                    {
                        // 시뮬레이션: 예시로 모두 존재
                        found = true;
                        score = 0.9;
                    }
                    else
                    {
                        // 예시: CenterSearch 사용 (dx,dy 만 필요)
                        var res = CenterSearchViaRunner();
                        if (res.ok)
                        {
                            // dx,dy 는 이미지 중심 기준 mm 오프셋
                            visionDx = res.x;
                            visionDy = res.y;
                            score = 0.8; // 별도 Run 에서 Score 전달받도록 Runner 확장 가능
                            found = (score >= MarkMinScore);
                        }
                    }

                    double finalX = targetX;
                    double finalY = targetY;

                    if (found && UseVisionOffsetApply)
                    {
                        finalX += visionDx;
                        finalY += visionDy;
                    }

                    // 중복 검사
                    if (found)
                    {
                        if (map.Entries.Any(e =>
                            Math.Abs(e.Xmm - finalX) <= DuplicateDistMm &&
                            Math.Abs(e.Ymm - finalY) <= DuplicateDistMm))
                        {
                            // 중복 → Skip
                            found = false;
                        }
                    }

                    var entry = new ChipMapEntry
                    {
                        Index = globalIndex++,
                        Row = r,
                        Col = c,
                        Xmm = finalX,
                        Ymm = finalY,
                        Present = found,
                        Enabled = found, // Missing 은 기본 false (사용자가 Enable 할 수도 있음)
                        Score = score
                    };
                    if (!found)
                    {
                        entry.Enabled = false;
                        entry.Score = 0;
                    }
                    map.Entries.Add(entry);

                    img?.Dispose();
                    img = null;
                }
            }

            // Origin 결정: 첫 Present 칩
            var first = map.Entries.FirstOrDefault(e => e.Present && e.Enabled);
            if (first != null)
            {
                map.OriginX = first.Xmm;
                map.OriginY = first.Ymm;
            }
            else
            {
                Log.Write(UnitName, "ChipMap", "No chip found");
                return -1;
            }

            CurrentChipMap = map;
            _chipPickupCursor = 0;
            ChipMappingDone = true;

            Log.Write(UnitName, "ChipMap",
                $"Done Rows={rows} Cols={cols} Found={map.Entries.Count(e => e.Present)} Missing={map.Entries.Count(e => !e.Present)}");

            return 0;
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

        // === Multi Pattern Raw Search Wrapper (모든 매칭 XY/R/Score) ===
        private (bool ok, List<PatternMatchingResult.PatternMatchingResultValue> matches) MultiPatternSearchViaRunner()
        {
            var ret = VisionRunnerHub.SearchAll(CameraKey);
            if (!ret.ok || ret.matches == null || ret.matches.Count == 0) return (false, null);
            return (true, ret.matches);
        }

        // FOV 기반 멀티 검색 매핑 (새 버전)
        public int PerformChipMappingV2()
        {
            ChipMappingDone = false;
            CurrentChipMap = null;

            if (!IsStatus_TAlignDone || !IsStatus_XYAlignDone)
            {
                Log.Write(UnitName, "ChipMapV2", "Align not completed");
                return -1;
            }
            if (!IsRingPresent())
            {
                Log.Write(UnitName, "ChipMapV2", "Wafer not present");
                return -1;
            }

            var centerTp = Config.GetTeachingPosition(InputStageConfig.TeachingPositionName.CenterPoint.ToString());
            if (centerTp == null)
            {
                Log.Write(UnitName, "ChipMapV2", "Center Teaching missing");
                return -1;
            }
            var (baseX, baseY, _) = Config.GetPositionWithOffset(centerTp.Name);

            // 이미지 크기 & FOV mm
            var img = StageCamera?.LatestImage;
            if (!Config.IsSimulation)
            {
                if (img == null || img.Header == null || img.Header.Width <= 0 || img.Header.Height <= 0)
                {
                    // 최근 이미지 없으면 한 번 스냅
                    if (StageCamera == null || StageCamera.GrabSync(out img) != 0 || img?.Header == null)
                    {
                        Log.Write(UnitName, "ChipMapV2", "Image header not available");
                        return -1;
                    }
                }
            }

            int imgW = img?.Header?.Width ?? 4096;
            int imgH = img?.Header?.Height ?? 3000;

            double fovWmm = imgW * PixelSizeXmm;
            double fovHmm = imgH * PixelSizeYmm;

            // 스캔 영역(Pitch 모를 수도 있으니 ROI 중심 = Center)
            double roiW = MappingRoiWidthMm;
            double roiH = MappingRoiHeightMm;

            double startX = baseX - roiW * 0.5;
            double startY = baseY + roiH * 0.5; // 위쪽이 +Y 인지 -Y 인지 설비 좌표계 확인 필요

            // Overlap 설정
            double overlapRatio = 0.20; // 20% 겹치기
            double stepX = fovWmm * (1.0 - overlapRatio);
            double stepY = fovHmm * (1.0 - overlapRatio);
            if (stepX <= 0 || stepY <= 0) return -1;

            int tilesX = Math.Max(1, (int)Math.Ceiling((roiW - fovWmm) / stepX) + 1);
            int tilesY = Math.Max(1, (int)Math.Ceiling((roiH - fovHmm) / stepY) + 1);

            var map = new ChipMapResult
            {
                PitchX = ChipPitchXmm > 0 ? ChipPitchXmm : 0,
                PitchY = ChipPitchYmm > 0 ? ChipPitchYmm : 0
            };

            List<ChipMapEntry> tempEntries = new List<ChipMapEntry>();

            for (int ty = 0; ty < tilesY; ty++)
            {
                for (int tx = 0; tx < tilesX; tx++)
                {
                    double tileLeft = startX + tx * stepX;
                    double tileTop = startY - ty * stepY;

                    // 타일 중심 (카메라 중심을 해당 지점으로 위치)
                    double targetX = tileLeft + fovWmm * 0.5;
                    double targetY = tileTop - fovHmm * 0.5;

                    if (MoveAxisPositionOne(AxisX, targetX) != 0) return -1;
                    if (MoveAxisPositionOne(AxisY, targetY) != 0) return -1;
                    if (WaitUntil(() => AxisX.InPosition(targetX) && AxisY.InPosition(targetY), MappingMoveTimeoutMs) != 0)
                    {
                        Log.Write(UnitName, "ChipMapV2", $"Move timeout tile ({tx},{ty})");
                        return -1;
                    }

                    VisionImage snap = null;
                    if (!Config.IsSimulation)
                    {
                        if (StageCamera.GrabSync(out snap) != 0 || snap == null)
                        {
                            Log.Write(UnitName, "ChipMapV2", $"Grab fail tile ({tx},{ty})");
                            continue;
                        }
                    }

                    var (ok, matches) = MultiPatternSearchViaRunner();
                    if (!ok || matches == null) continue;

                    double cxPix = imgW / 2.0;
                    double cyPix = imgH / 2.0;
                    double stageTdeg = AxisT?.GetPosition() ?? 0.0;
                    bool useRotation = Math.Abs(stageTdeg) > 0.0005; // 필요시

                    foreach (var m in matches)
                    {
                        // 픽셀 → mm (카메라 좌표 오프셋)
                        double dxPix = m.X - cxPix;
                        double dyPix = m.Y - cyPix;
                        double dxMm = dxPix * PixelSizeXmm;
                        double dyMm = dyPix * PixelSizeYmm;

                        // 회전 보정 (Stage T 적용)
                        if (useRotation)
                        {
                            var rot = qGeometry.CalculateRotationTransformation(
                                new PointD(0, 0),
                                new PointD(dxMm, dyMm),
                                stageTdeg);
                            dxMm = rot.X; dyMm = rot.Y;
                        }

                        double absX = targetX + dxMm;
                        double absY = targetY + dyMm;

                        // 중복 검사
                        if (tempEntries.Any(e =>
                        {
                            double ddx = e.Xmm - absX;
                            double ddy = e.Ymm - absY;
                            return Math.Sqrt(ddx * ddx + ddy * ddy) <= DuplicateDistMm;
                        }))
                        {
                            continue;
                        }

                        tempEntries.Add(new ChipMapEntry
                        {
                            Index = -1, // 나중 재할당
                            Row = -1,
                            Col = -1,
                            Xmm = absX,
                            Ymm = absY,
                            Present = true,
                            Enabled = true,
                            Score = m.Score
                        });
                    }

                    snap?.Dispose();
                }
            }

            if (tempEntries.Count == 0)
            {
                Log.Write(UnitName, "ChipMapV2", "No chips detected");
                return -1;
            }

            // Pitch 자동 추정 (옵션)
            if (ChipPitchXmm <= 0 || ChipPitchYmm <= 0)
            {
                EstimatePitch(tempEntries, out double px, out double py);
                if (ChipPitchXmm <= 0 && px > 0) ChipPitchXmm = px;
                if (ChipPitchYmm <= 0 && py > 0) ChipPitchYmm = py;
            }

            // Row / Col 그룹핑
            BuildGrid(tempEntries, ChipPitchXmm, ChipPitchYmm, out var finalizedEntries, out int rows, out int cols);

            // Origin (첫 Row, 첫 Col)
            var origin = finalizedEntries.Where(e => e.Present && e.Enabled).OrderBy(e => e.Row).ThenBy(e => e.Col).FirstOrDefault();
            if (origin == null)
            {
                Log.Write(UnitName, "ChipMapV2", "Origin not found");
                return -1;
            }

            map.Rows = rows;
            map.Cols = cols;
            map.OriginX = origin.Xmm;
            map.OriginY = origin.Ymm;
            int gIndex = 0;
            foreach (var e in finalizedEntries.OrderBy(e => e.Row).ThenBy(e => e.Col))
            {
                e.Index = gIndex++;
                map.Entries.Add(e);
            }

            CurrentChipMap = map;
            _chipPickupCursor = 0;
            ChipMappingDone = true;

            Log.Write(UnitName, "ChipMapV2",
                $"Tiles=({tilesX}x{tilesY}) Chips={map.Entries.Count(e => e.Present)} Rows={rows} Cols={cols} Pitch=({ChipPitchXmm:F3},{ChipPitchYmm:F3})");

            return 0;
        }

        private void EstimatePitch(List<ChipMapEntry> list, out double pitchX, out double pitchY)
        {
            pitchX = 0; pitchY = 0;
            if (list.Count < 2) return;
            var xs = list.Select(e => e.Xmm).OrderBy(v => v).ToList();
            var ys = list.Select(e => e.Ymm).OrderBy(v => v).ToList();
            List<double> dxs = new List<double>();
            for (int i = 1; i < xs.Count; i++)
            {
                double d = xs[i] - xs[i - 1];
                if (d > 0.2) dxs.Add(d); // 너무 작은 노이즈 제외 (임계 임의)
            }
            List<double> dys = new List<double>();
            for (int i = 1; i < ys.Count; i++)
            {
                double d = ys[i] - ys[i - 1];
                if (d > 0.2) dys.Add(d);
            }
            if (dxs.Count > 0) pitchX = Median(dxs);
            if (dys.Count > 0) pitchY = Median(dys);
        }
        private double Median(List<double> v)
        {
            if (v == null || v.Count == 0) return 0;
            var s = v.OrderBy(x => x).ToList();
            int n = s.Count;
            if (n % 2 == 1) return s[n / 2];
            return 0.5 * (s[n / 2 - 1] + s[n / 2]);
        }

        private void BuildGrid(List<ChipMapEntry> raw, double pitchX, double pitchY,
                               out List<ChipMapEntry> finalized, out int rows, out int cols)
        {
            finalized = new List<ChipMapEntry>();
            rows = 0; cols = 0;
            if (raw.Count == 0) return;

            // Row 그룹핑 (Y 기준)
            double yTol = (pitchY > 0 ? pitchY * 0.5 : 2.0);
            var ordered = raw.OrderBy(e => e.Ymm).ToList();
            List<List<ChipMapEntry>> rowGroups = new List<List<ChipMapEntry>>();
            List<ChipMapEntry> cur = new List<ChipMapEntry>();
            double lastY = double.NaN;

            foreach (var e in ordered)
            {
                if (cur.Count == 0)
                {
                    cur.Add(e);
                    lastY = e.Ymm;
                }
                else
                {
                    if (Math.Abs(e.Ymm - lastY) <= yTol)
                    {
                        cur.Add(e);
                    }
                    else
                    {
                        rowGroups.Add(cur);
                        cur = new List<ChipMapEntry> { e };
                    }
                    lastY = e.Ymm;
                }
            }
            if (cur.Count > 0) rowGroups.Add(cur);

            rows = rowGroups.Count;

            // 각 Row 정렬(X) & Col index
            int globalMaxCol = 0;
            for (int r = 0; r < rowGroups.Count; r++)
            {
                var rowList = rowGroups[r].OrderBy(e => e.Xmm).ToList();
                double xTol = (pitchX > 0 ? pitchX * 0.5 : 2.0);
                int col = 0;
                ChipMapEntry prev = null;
                foreach (var e in rowList)
                {
                    if (prev != null && pitchX > 0)
                    {
                        double gap = e.Xmm - prev.Xmm;
                        if (gap > pitchX + xTol)
                        {
                            // 큰 갭 → 중간 Missing 예상 ⇒ gap/pitchX - 1 개 만큼 빈 칩 삽입(단순)
                            int missingCount = (int)Math.Round(gap / pitchX) - 1;
                            for (int m = 0; m < missingCount; m++)
                            {
                                finalized.Add(new ChipMapEntry
                                {
                                    Row = r,
                                    Col = col + 1 + m,
                                    Present = false,
                                    Enabled = false,
                                    Xmm = prev.Xmm + (m + 1) * pitchX,
                                    Ymm = prev.Ymm,
                                    Score = 0
                                });
                            }
                            col += missingCount;
                        }
                    }
                    e.Row = r;
                    e.Col = col;
                    finalized.Add(e);
                    prev = e;
                    col++;
                }
                if (col > globalMaxCol) globalMaxCol = col;
            }
            cols = globalMaxCol;
        }


        #endregion
    }
}