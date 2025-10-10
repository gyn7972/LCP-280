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
using System.Windows.Shapes;
using static QMC.Common.Material;
using static QMC.LCP_280.Process.Equipment;
using static QMC.LCP_280.Process.PatternMatchingRunner;
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

        public delegate void UpdateUIWafer(MaterialWafer wafer);

        public event UpdateUIWafer EventUpdateUIWafer;

        public enum AlarmKeys
        {
            eDieTransferPickZNotSafe = 3001,
            eInputFeederCylinderZNotSafe,
            eInputStageEjectorPinZNotSafe,
            eInputStageEjectorZNotSafe,
            eInputFeederYNotSafe,
            eVisionTsearch,
            eVisionXYsearch,
            eInputStageMoveFail,
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
            alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
            m_dicAlarms.Add(alarm.Code, alarm);

            alarm = new AlarmInfo();
            alarm.Code = (int)AlarmKeys.eInputFeederCylinderZNotSafe;
            alarm.Title = "Feeder Z-Cylinder Not Sfarety Pos.";
            alarm.Cause = "Feeder Z-Cylinder가 안전 위치가 아닙니다.\n 포지션 확인 후 다시 시작 하십시요.";
            alarm.Source = this.UnitName;
            alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
            m_dicAlarms.Add(alarm.Code, alarm);

            //,
            alarm = new AlarmInfo();
            alarm.Code = (int)AlarmKeys.eInputStageEjectorPinZNotSafe;
            alarm.Title = "EjectorPin Z-Axis Not Sfarety Pos.";
            alarm.Cause = "EjectorPin Z-Axis가 안전 위치가 아닙니다.\n 포지션 확인 후 다시 시작 하십시요.";
            alarm.Source = this.UnitName;
            alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
            m_dicAlarms.Add(alarm.Code, alarm);
            //,
            alarm = new AlarmInfo();
            alarm.Code = (int)AlarmKeys.eInputStageEjectorZNotSafe;
            alarm.Title = "Ejector Z-Axis Not Sfarety Pos.";
            alarm.Cause = "Ejector Z-Axis가 안전 위치가 아닙니다.\n 포지션 확인 후 다시 시작 하십시요.";
            alarm.Source = this.UnitName;
            alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
            m_dicAlarms.Add(alarm.Code, alarm);

            //
            alarm = new AlarmInfo();
            alarm.Code = (int)AlarmKeys.eInputFeederYNotSafe;
            alarm.Title = "Feeder Y-Axis Not Sfarety Pos.";
            alarm.Cause = "Feeder Y-Axis가 안전 위치가 아닙니다.\n 포지션 확인 후 다시 시작 하십시요.";
            alarm.Source = this.UnitName;
            alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
            m_dicAlarms.Add(alarm.Code, alarm);

            alarm = new AlarmInfo();
            alarm.Code = (int)AlarmKeys.eVisionTsearch;
            alarm.Title = "Vision T Search.";
            alarm.Cause = "Vision T Search Fail.\n Chip Mark 확인 후 다시 시작 하십시요.";
            alarm.Source = this.UnitName;
            alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
            m_dicAlarms.Add(alarm.Code, alarm);
            //
            alarm = new AlarmInfo();
            alarm.Code = (int)AlarmKeys.eVisionXYsearch;
            alarm.Title = "Vision XY Search.";
            alarm.Cause = "Vision XY Search Fail.\n Chip Mark 확인 후 다시 시작 하십시요.";
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

        }
        #endregion

        

        #region Vision Hooks / Camera / Runner
        public HIKGigECamera StageCamera { get; private set; }
        public string StageCameraKey { get; set; } = "In_Stage";

        public PatternMatchingRunner _pmRunner;

        // Pattern Matching Runner (간소화: Recipe 자동 관리)
        public PatternMatchingRunner PmRunner
        { 
            get
            {
                if(_pmRunner == null)
                {
                    _pmRunner = VisionRunnerHub.GetOrCreate(StageCameraKey);
                }
                return _pmRunner;
            }
        }

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
        InputFeeder InputFeeder { get; set; }
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


        //가공시에 스테이지 Area 밖으로 나가는것을 방지하기 위한 함수
        //public override int CheckMoveSafety(MotionAxis ax)
        //{
        //    try
        //    {
        //        //if (/*다른 유닛 축 이동중*/) return (int)AlarmKeys.xxx;
        //        // PickZ Safety Check
        //        // Ejector Pin Z and Ejector Z Safety Check
        //        // Ejector Pin Z and Ejector Z 이 Safety Position이 아닐 경우
        //        // X,Y Encoder 위치 기준 min/max 체크하고 움직여야 한다. 


        //        // 1) Ejector / PinZ Safety 검사 (우선순위 높음)
        //        bool pinZSafe = true;
        //        bool ejectorZSafe = true;
        //        if (InputStageEjector != null)
        //        {
        //            pinZSafe |= InputStageEjector.IsPinZSafetyPos();
        //            pinZSafe |= InputStageEjector.IsAtEjectPinReady();

        //            ejectorZSafe |= InputStageEjector.IsEjectorZSafetyPos();
        //            ejectorZSafe |= InputStageEjector.IsAtEjectBlockReady();

        //            if (!pinZSafe || !ejectorZSafe)
        //            {
        //                // PinZ 또는 EjectorZ 가 Safety 가 아닐 때 X/Y 이동 허용 범위 검사
        //                if (ax == AxisX || ax == AxisY)
        //                {
        //                    if (!IsStageInterLockOK())
        //                    {
        //                        // 어떤 축이 원인인지에 따라 더 구체적인 알람 선택
        //                        if (!pinZSafe)
        //                            return (int)AlarmKeys.eInputStageEjectorPinZNotSafe;
        //                        if (!ejectorZSafe)
        //                            return (int)AlarmKeys.eInputStageEjectorZNotSafe;
        //                        // 둘 다 아니면 일반 반환
        //                        return (int)AlarmKeys.eInputStageEjectorZNotSafe;
        //                    }
        //                }
        //            }
        //        }

        //        // 2) DieTransfer PickZ Safety
        //        if (InputDieTransfer != null && !InputDieTransfer.IsPickZSafetyPos())
        //            return (int)AlarmKeys.eDieTransferPickZNotSafe;

        //        // 3) Feeder Z / Y Safety
        //        if (InputFeeder != null)
        //        {
        //            if (!InputFeeder.IsFeederZSafetyPosition())
        //                return (int)AlarmKeys.eInputFeederCylinderZNotSafe;

        //            if (!InputFeeder.IsFeederYSafetyPosition())
        //                return (int)AlarmKeys.eInputFeederYNotSafe;
        //        }

        //        // 추가로 "다른 유닛 축 이동중" 등을 넣고 싶다면 여기서 검사 후 알람 코드 반환
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.Write(ex);
        //        // 예외 발생 시 보수적으로 이동 중단하도록 임의 알람 (PinZ 알람 선택) 반환 가능
        //        return (int)AlarmKeys.eInputStageEjectorPinZNotSafe;
        //    }

        //    return 0; // 0 = OK
        //}

        // ================== Generic Single Axis Move (Safety Interlock 동일 구조) ==================
        /// <summary>
        /// 단일 축 이동 (Safety 인터락 포함). 이동 완료까지 블록.
        /// </summary>
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
                    PostAlarm((int)AlarmKeys.eInputStageEjectorPinZNotSafe);
                    return -1;
                }
                if (!InputStageEjector.IsEjectorZSafetyPos())
                {
                    AxisX?.EmgStop(); AxisY?.EmgStop(); AxisT?.EmgStop();
                    PostAlarm((int)AlarmKeys.eInputStageEjectorZNotSafe);
                    return -1;
                }
                if (!InputDieTransfer.IsPositionPickZSafety())
                {
                    AxisX?.EmgStop(); AxisY?.EmgStop(); AxisT?.EmgStop();
                    PostAlarm((int)AlarmKeys.eDieTransferPickZNotSafe);
                    return -1;
                }
                if (!InputFeeder.IsFeederZSafetyPosition())
                {
                    AxisX?.EmgStop(); AxisY?.EmgStop(); AxisT?.EmgStop();
                    PostAlarm((int)AlarmKeys.eInputFeederCylinderZNotSafe);
                    return -1;
                }
                if (!InputFeeder.IsFeederYSafetyPosition())
                {
                    AxisX?.EmgStop(); AxisY?.EmgStop(); AxisT?.EmgStop();
                    PostAlarm((int)AlarmKeys.eInputFeederYNotSafe);
                    return -1;
                }

                Thread.Sleep(0);
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
        /// //////////////////////////////////////////////////////////////////////////////////////////////
        // UI, sequence 용 Move 함수
        public int MoveTeachingPositionOnce(InputStageConfig.TeachingPositionName name, bool isFine)
        {
            return MoveTeachingPositionOnce((int)name, isFine);
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
                    PostAlarm((int)AlarmKeys.eInputStageEjectorPinZNotSafe);
                    return -1;
                }

                if (!InputStageEjector.IsEjectorZSafetyPos())
                {
                    this.AxisX.EmgStop();
                    this.AxisY.EmgStop();
                    this.AxisT.EmgStop();
                    PostAlarm((int)AlarmKeys.eInputStageEjectorZNotSafe);
                    return -1;
                }

                // DieTransfer PickZ Safety
                if (!InputDieTransfer.IsPositionPickZSafety())
                {
                    this.AxisX.EmgStop();
                    this.AxisY.EmgStop();
                    this.AxisT.EmgStop();
                    PostAlarm((int)AlarmKeys.eDieTransferPickZNotSafe);
                    return -1;
                }

                if (!InputFeeder.IsFeederZSafetyPosition())
                {
                    this.AxisX.EmgStop();
                    this.AxisY.EmgStop();
                    this.AxisT.EmgStop();
                    PostAlarm((int)AlarmKeys.eInputFeederCylinderZNotSafe);
                    return -1;
                }

                if (!InputFeeder.IsFeederYSafetyPosition())
                {
                    this.AxisX.EmgStop();
                    this.AxisY.EmgStop();
                    this.AxisT.EmgStop();
                    PostAlarm((int)AlarmKeys.eInputFeederYNotSafe);
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
        #endregion

        //public bool InPosTeaching(string name)
        //{
        //    var (t, pz, plz) = Config.GetPositionWithOffset(name);
        //    return InPos(_axX, t) && InPos(_axY, pz) && InPos(_axT, plz);
        //}

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
        //protected bool InPos(MotionAxis ax, double target) => ax == null || ax.InPosition(target);

        #region Teaching Position Move (Batch Style)
        public int MoveToTeachingPosition(string positionName, double vel = 0, double acc = 0, double dec = 0, double jerk = 0)
        {
            var tp = Config.GetTeachingPosition(positionName);
            if (tp == null) return -1;
            var (x, y, t) = Config.GetPositionWithOffset(positionName);   //Offset 포함 위치 - Align 수행 시 data 있음.
            int rc = 0;

            //Todo : 인터락 확인 후 이동 하도록 수정.
            //if (AxisX != null) rc |= AxisX.MoveAbs(x, vel > 0 ? vel : AxisX.Config.MaxVelocity, acc > 0 ? acc : AxisX.Config.RunAcc, dec > 0 ? dec : AxisX.Config.RunDec, jerk > 0 ? jerk : AxisX.Config.AccJerkPercent);
            //if (AxisY != null) rc |= AxisY.MoveAbs(y, vel > 0 ? vel : AxisY.Config.MaxVelocity, acc > 0 ? acc : AxisY.Config.RunAcc, dec > 0 ? dec : AxisY.Config.RunDec, jerk > 0 ? jerk : AxisY.Config.AccJerkPercent);
            //if (AxisT != null) rc |= AxisT.MoveAbs(t, vel > 0 ? vel : AxisT.Config.MaxVelocity, acc > 0 ? acc : AxisT.Config.RunAcc, dec > 0 ? dec : AxisT.Config.RunDec, jerk > 0 ? jerk : AxisT.Config.AccJerkPercent);

            return rc;
        }
        //public bool InPosTeaching(TeachingPosition tp)
        //{
        //    if (tp == null)
        //        return false;
        //    return InPosTeaching(tp.Name);
        //}
        //public double GetTP(string tpName, string axisName)
        //{
        //    var tp = Config.GetTeachingPosition(tpName);
        //    if (tp != null && tp.AxisPositions != null && tp.AxisPositions.TryGetValue(axisName, out var v)) return v;
        //    return 0.0;
        //}

        public double GetTP(TeachingPosition tp, string axisName) => (tp == null || string.IsNullOrEmpty(axisName)) ? 0.0 : (tp.AxisPositions.TryGetValue(axisName, out var v) ? v : 0.0);
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

            if (!bCheckSignal)
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

        #region High-Level Actuator API (Interlock 포함)
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
        #endregion

        // === Direct Valve Control (강제 구동) ===
        public bool IsVacuumValveOn()
        {
            if (Config.IsSimulation)// || Config.IsDryRun)
            {
                return true;
            }
            return this.IsOutputOn(InputStageConfig.IO.VAC_OUT);
        }
        #endregion

        // 파라미터로 빼야하는 Data 및 상수
        public int MoveTimeoutMs { get; set; } = 6000;
        public int PollIntervalMs { get; set; } = 30;
        public double AngleIgnoreThresholdDeg { get; set; } = 0.001;
        public double AngleMaxApplyDeg { get; set; } = 2.0;
        public double AngleApplyGain { get; set; } =- 1.0; // 방향 반전 필요 시 -1 사용
        public bool UseOffsetForTAxisCorrection { get; set; } = true; // false면 직접 축 이동 방식으로 전환 가능 (추후 확장)
        
        private int WaitUntil(Func<bool> cond, int timeoutMs)
        {
            int nRtn = 0;
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



        #region Seq Signal
        // ====== Align Refactor: 상태/결과 보관 필드 ======
        public bool IsStatus_TAlignPrepared { get; private set; }
        public bool IsStatus_TAlignDone { get; private set; }
        public double IsStatus_LastFoundTRawAngle { get; private set; }
        public double IsStatus_LastAppliedTAngle { get; private set; }
        public bool IsStatus_XYAlignPrepared { get; private set; }
        public bool IsStatus_XYAlignDone { get; private set; }
        public double IsStatus_LastFoundDx { get; private set; }
        public double IsStatus_LastFoundDy { get; private set; }

        // ====== InputDieTr Signal
        public bool RequestOutputDie { get; set; } = false;

        #endregion


        #region Seq

        public MaterialWafer GetMaterialWafer()
        {
            var mat = GetMaterial();
            return mat as MaterialWafer;
        }

        public double MaxXYOffsetMm { get; set; } = 2.0;   // XY 최대 보정 허용치 (mm)
        public bool IsStatus_RequestWafer { get; internal set; } = false;

        

        public override int OnRun()
        {
            int ret = 0;
            if (this.RunUnitStatus == UnitStatus.Stopped ||
                this.RunUnitStatus == UnitStatus.Stopping ||
                this.RunUnitStatus == UnitStatus.CycleStop)
            {
                this.State = ProcessState.Stop;
                ret = -1;
            }
            if (this.RunUnitStatus == UnitStatus.Running)
            {
                return 0;
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
            this.RunUnitStatus = UnitStatus.Stopped;
            this.State = ProcessState.Stop;
            base.OnStop();
            return ret;
        }
        protected override int OnRunReady() { return 0; }
        protected override int OnRunWork() { return 0; }
        protected override int OnRunComplete() { return 0; }
        #endregion

        protected override void OnMakeSequence()
        {
            base.OnMakeSequence();
            this.SequencePlayers.Add(AlignT);
            this.SequencePlayers.Add(AlignXY);
            this.SequencePlayers.Add(PerformChipMapping);
            this.SequencePlayers.Add(MoveStageToNextDie);
        }

        #region Seq 단위 동작 함수
        public int AlignT(bool bFineSpeed = false)
        {
            int nRet = 0;
            if (RunMode == UnitRunMode.Manual)
            {
                this.CurrentFunc = AlignT;
            }

            nRet = AlignTPrepare(bFineSpeed);
            if (nRet != 0)
            {
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
        public int PerformChipMapping(bool bFineSpeed = false)
        {
            int nRet = 0;
            if (RunMode == UnitRunMode.Manual)
            {
                this.CurrentFunc = PerformChipMapping;
            }

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
            MakeScanPath(out List<PointD> path);
            List<PointD> chips = new List<PointD>();
            Task<int> tImageProcess = null;
            try
            {
                foreach (var pt in path)
                {
                    this.CalcelToken?.Token.ThrowIfCancellationRequested();
                    nRet = MoveStage(pt.X, pt.Y, bFineSpeed);
                    if (nRet != 0)
                    {
                        Log.Write(UnitName, "ChipMap", "Fail: MoveStage");
                        return -1;
                    }

                    if (this.Config.IsSimulation || this.Config.IsDryRun)
                    {
                        //시뮬레이션
                        Random rnd = new Random();
                        int nChips = rnd.Next(5, 15);
                        for (int i = 0; i < nChips; i++)
                        {
                            double rx = (rnd.NextDouble() - 0.5) * 10;
                            double ry = (rnd.NextDouble() - 0.5) * 10;
                            chips.Add(new PointD(pt.X + rx, pt.Y + ry));
                        }
                        continue;
                    }
                    else
                    {
                        StageCamera.GrabSync(out VisionImage grabImage);
                        //SearchDies(grabImage, ref chips, pt.X, pt.Y);
                        if (tImageProcess != null)
                        {
                            tImageProcess.Wait();
                        }
                        double dx = pt.X;
                        double dy = pt.Y;
                        tImageProcess = Task.Factory.StartNew(() =>
                        {
                            return SearchDies(grabImage, ref chips, dx, dy);
                        });
                    }
                    if (nRet != 0)
                    {
                        Log.Write(UnitName, "ChipMap", "Fail: GrabAndMap");
                        return -1;
                    }
                }
                if (tImageProcess != null)
                {
                    tImageProcess.Wait();
                }
            }
            catch (OperationCanceledException)
            {
                Log.Write(UnitName, "ChipMap", "Cancelled");
                return nRet;
            }
            catch (Exception ex)
            {
                Log.Write(UnitName, "ChipMap", "Exception: " + ex.Message);
                return -1;
            }

            if (RunMode == UnitRunMode.Manual)
            {
                SetMaterial(new MaterialWafer());
            }

            UpdateChipInfo(chips);
            MaterialWafer wafer = GetMaterialWafer();
            wafer.ProcessSatate = Material.MaterialProcessSatate.Processing;
            return nRet;
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


        public bool IsRingPresent()
        {
            bool bRtn = true;
            if (Config.IsSimulation || Config.IsDryRun)
            {
                return true;
            }
            else if (!Ring0() || !Ring1())
            {
                Log.Write(UnitName, "IsRingPresent", $"Ring not present (R0={Ring0()}, R1={Ring1()})");
                return false;
            }

            return bRtn;
        }
        public bool IsWaferLoadingPosition()
        {
            var tp = TeachingPositions[(int)InputStageConfig.TeachingPositionName.Loading];
            if (tp == null) 
                return false;

            return InPosTeaching(tp);
        }
        public bool IsWaferUnloadingPosition()
        {
            var tp = TeachingPositions[(int)InputStageConfig.TeachingPositionName.Unloading];
            if (tp == null) return false;
            return InPosTeaching(tp);
        }
        public bool IsWaferCenterPosition()
        {
            var tp = TeachingPositions[(int)InputStageConfig.TeachingPositionName.CenterPoint];
            if (tp == null) return false;
            return InPosTeaching(tp);
        }
        public int LoadingWaferPrepare()
        {
            int nRtn = 0;

            Log.Write(this, "Start LoadingWaferPrepare");
            
            // 이미 웨이퍼 존재하면 준비 단계 불필요 (바로 완료 단계 가능)
            if(!Config.IsSimulation && !Config.IsDryRun)    
            {
                if (IsRingPresent())
                if (IsRingPresent())
                {
                    Log.Write(UnitName, "LoadingPrep", "Wafer already present -> Skip prepare");
                    return nRtn;
                }
            }
            
            // 로딩 Teaching 이동
            nRtn = MoveToStageLoadPosition();
            if (nRtn != 0)
            {
                Log.Write(this, "Fail: Move Load");
                return nRtn;
            }
            if (this.IsStop) { return 0; }

            // Clamp Back → Lift Down
            SetClampFB(false);
            if (!IsClampBwd())
            {
                Log.Write(this, "Fail: ClampBack");
                return -1;
            }
            if (this.IsStop) { return 0; }

            SetClampLift(false);
            if (!IsClampLiftDown())
            {
                Log.Write(this, "Fail: ClampLiftDown");
                return -1;
            }
            if (this.IsStop) { return 0; }

            //Plate Down → 
            SetClampPlate(false);
            if (!IsPlateDown())
            {
                Log.Write(this, "Fail: PlateUp");
                return -1;
            }

            Log.Write(UnitName, "LoadingPrep", "StageLoadingReady = TRUE (Wait wafer)");
            Log.Write(UnitName, "End LoadingWaferPrepare");
            return 0;
        }
        public int MoveToStageLoadPosition(bool isFine = false)
        {
            int nRet = 0;
            if (IsWaferLoadingPosition())
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
                    PostAlarm((int)AlarmKeys.eInputStageEjectorPinZNotSafe);
                    return -1;
                }

                if (!InputStageEjector.IsEjectorZSafetyPos())
                {
                    this.AxisX.EmgStop();
                    this.AxisY.EmgStop();
                    this.AxisT.EmgStop();
                    PostAlarm((int)AlarmKeys.eInputStageEjectorZNotSafe);
                    return -1;
                }

                // DieTransfer PickZ Safety
                if (!InputDieTransfer.IsPositionPickZSafety())
                {
                    this.AxisX.EmgStop();
                    this.AxisY.EmgStop();
                    this.AxisT.EmgStop();
                    PostAlarm((int)AlarmKeys.eDieTransferPickZNotSafe);
                    return -1;
                }
                if (Config.IsSimulation || Config.IsDryRun)
                {
                    //Simulation - ok
                }
                else if (!InputFeeder.IsFeederZSafetyPosition())
                {
                    this.AxisX.EmgStop();
                    this.AxisY.EmgStop();
                    this.AxisT.EmgStop();
                    PostAlarm((int)AlarmKeys.eInputFeederCylinderZNotSafe);
                    return -1;
                }

                if (!InputFeeder.IsFeederYSafetyPosition())
                {
                    this.AxisX.EmgStop();
                    this.AxisY.EmgStop();
                    this.AxisT.EmgStop();
                    PostAlarm((int)AlarmKeys.eInputFeederYNotSafe);
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
        public int LoadingWaferComplete()
        {
            int ret = 0;

            // 준비 안 되었으면 호출 순서 오류
            if (!IsRingPresent())
            {
                Log.Write(UnitName, "LoadingComp", "Not prepared (call LoadingWaferPrepare first)");
                return -1;
            }

            // 아직 Wafer 안 올라옴 → 대기
            bool bRtn = Config.IsSimulation;
            if (IsRingPresent() || bRtn || Config.IsDryRun)
            {
                Log.Write(UnitName, "LoadingComp", "Wafer detected -> Completing");

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
                
                // 센터 Teaching 이동
                ret = MoveToStageCenterPosition();
                if (ret != 0)
                {
                    Log.Write(this, "Fail: Move Load");
                    return ret;
                }

                Log.Write(UnitName, "LoadingComp", "Done");

                return ret;
            }

            return ret;
        }
        public bool IsInterlockWithFeederAndDieTransferOk()
        {
            return IsInterlockWithFeederAndDieTransferOkInt() == 0;
        }
        public int IsInterlockWithFeederAndDieTransferOkInt()
        {
            if (InputFeeder.IsFeederZSafetyPosition() == false)
            {
                Log.Write(UnitName, "Interlock", "Feeder Z not safe");
                return -1;
            }
            if (InputFeeder.IsFeederYSafetyPosition() == false)
            {
                Log.Write(UnitName, "Interlock", "Feeder Y not safe");
                return -2;
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
        public int MoveToStageCenterPosition(bool isFine = false)
        {
            int nRet = 0;

            if(IsWaferCenterPosition())
            {
                return 0; // 이미 센터 위치에 있으면 무시
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
                    PostAlarm((int)AlarmKeys.eInputStageEjectorPinZNotSafe);
                    return -1;
                }

                if (!InputStageEjector.IsEjectorZSafetyPos())
                {
                    this.AxisX.EmgStop();
                    this.AxisY.EmgStop();
                    this.AxisT.EmgStop();
                    PostAlarm((int)AlarmKeys.eInputStageEjectorZNotSafe);
                    return -1;
                }

                // DieTransfer PickZ Safety
                if (!InputDieTransfer.IsPositionPickZSafety())
                {
                    this.AxisX.EmgStop();
                    this.AxisY.EmgStop();
                    this.AxisT.EmgStop();
                    PostAlarm((int)AlarmKeys.eDieTransferPickZNotSafe);
                    return -1;
                }

                if (!InputFeeder.IsFeederZSafetyPosition())
                {
                    this.AxisX.EmgStop();
                    this.AxisY.EmgStop();
                    this.AxisT.EmgStop();
                    PostAlarm((int)AlarmKeys.eInputFeederCylinderZNotSafe);
                    return -1;
                }

                if (!InputFeeder.IsFeederYSafetyPosition())
                {
                    this.AxisX.EmgStop();
                    this.AxisY.EmgStop();
                    this.AxisT.EmgStop();
                    PostAlarm((int)AlarmKeys.eInputFeederYNotSafe);
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

        //AlignT_Stage
        //AlignXY_Stage
        //Mapping_Stage
        private TeachingPosition _lastCenterAlignTp;
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

            MaterialWafer wafer = GetMaterialWafer();
            if(wafer is null)
            {
                wafer = new MaterialWafer();
                SetMaterial(wafer);
            }
            nRtn = MoveToStageCenterPosition();
            if (nRtn != 0)
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

           
            return 0;
        }
        public int AlignTPrepare(bool bFineSpeed = false)
        {
            IsStatus_TAlignPrepared = false;
            IsStatus_TAlignDone = false;
            IsStatus_LastFoundTRawAngle = 0;
            IsStatus_LastAppliedTAngle = 0;
            _lastCenterAlignTp = null;

            if(this.Config.IsSimulation || this.Config.IsDryRun)
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
        public int AlignTheta(bool bFineSpeed = false)
        {
            int nRet = 0;

            if(Config.IsSimulation || this.Config.IsDryRun)
            {
                IsStatus_LastAppliedTAngle = 0;
                IsStatus_TAlignDone = true;
                return 0;

            }
            try 
            {
                StageCamera.GrabSync(out VisionImage img);
                PmRunner.SearchTheta(img, out double angle);
                double currentAngle = this.AxisT.GetPosition();
                double dTarget = currentAngle + angle * AngleApplyGain;
                Log.Write(UnitName, "T_Align", $"Vision angle={angle:F4} currentT={currentAngle:F4}");

                IsStatus_LastFoundTRawAngle = angle;

                bool IsAuto = false;
                if (RunMode == UnitRunMode.Auto)
                    IsAuto = true;
                else
                    IsAuto = false;
                this.AxisT.MoveAbs(dTarget, IsAuto, bFineSpeed);
                nRet = WaitUntil(() => InPos(this.AxisT , dTarget), MoveTimeoutMs);
            }
            catch(Exception ex)
            {
                Log.Write(UnitName, "T_Align", $"Exception: {ex.Message}");
                return -1;
            }
            
            IsStatus_TAlignDone = true;
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
            if(this.Config.IsSimulation || this.Config.IsDryRun)
            {
                _lastCenterAlignTp = new TeachingPosition();

                IsStatus_XYAlignDone = true;
                return 0;
            }
            
            IsStatus_XYAlignDone = true;
            return 0;
        }
        public int MoveStage(double x, double y, bool bFineSpeed = false)
        {
            int ret = 0;

            //if (WaitUntil(() =>
            //    this.InputStageEjector.IsAnyAxisMoving(),
            //    MappingMoveTimeoutMs) != 0)
            //    return -1;

            if (!this.InputStageEjector.IsPinZSafetyPos())
            {
                AxisX?.EmgStop(); AxisY?.EmgStop(); AxisT?.EmgStop();
                PostAlarm((int)AlarmKeys.eInputStageEjectorPinZNotSafe);
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

                ret = this.AxisX.MoveAbs(x, IsAuto,bFineSpeed);
                if (ret != 0)
                {
                    AxisX?.EmgStop(); AxisY?.EmgStop(); AxisT?.EmgStop();
                    PostAlarm((int)AlarmKeys.eInputStageMoveFail);
                    return ret;
                }

                ret = this.AxisY.MoveAbs(y, IsAuto,bFineSpeed);
                if (ret != 0)
                {
                    AxisX?.EmgStop(); AxisY?.EmgStop(); AxisT?.EmgStop();
                    PostAlarm((int)AlarmKeys.eInputStageMoveFail);
                    return ret;
                }

                if (WaitUntil(() =>
                    AxisX.InPosition(x) && AxisY.InPosition(y),
                    MappingMoveTimeoutMs) != 0)
                    return -1;
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
                return true;

            var tp = Config.GetTeachingPosition(InputStageConfig.TeachingPositionName.CenterPoint.ToString());
            if (tp == null || tp.AxisPositions == null)
            {
                Log.Write(UnitName, "MoveSafety", "CenterPoint teaching not found");
                return false;
            }

            if (!tp.AxisPositions.TryGetValue(AxisNames.WaferStageX, out var centerX) ||
                !tp.AxisPositions.TryGetValue(AxisNames.WaferStageY, out var centerY))
            {
                Log.Write(UnitName, "MoveSafety", "CenterPoint X/Y value missing");
                return false;
            }

            double radius = Config.SafeSatageRaius;
            if (radius <= 0)
            {
                Log.Write(UnitName, "MoveSafety", $"Invalid SafeSatageRaius={radius}");
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
        PointD GetPixelToMmScale(double dX,double dY)
        {
            double mmPerPixelX = (dX - StageCamera.CameraConfig.Resolution.Width /2 ) * StageCamera.CameraConfig.Scale.X;
            double mmPerPixelY = (dY - StageCamera.CameraConfig.Resolution.Height /2 ) * StageCamera.CameraConfig.Scale.Y;
            return new PointD(mmPerPixelX, mmPerPixelY);
        }
        public int SearchDies(VisionImage visionImage, ref List<PointD> points, double x, double y)
        {
            int ret = 0;
            var result = this.PmRunner.Search(visionImage);

            if(result.Success)
            {
                foreach(var v in result.Matches)
                {
                    lock(points)
                    {
                        PointD pt = GetPixelToMmScale(v.X, v.Y);
                        pt.X += x;
                        pt.Y += y;
                        points.Add(new PointD(pt.X, pt.Y));
                    }
                }
            }
            return ret;
        }
        private void UpdateChipInfo(List<PointD> chips)
        {
            try
            {
                MaterialWafer materialWafer = GetMaterialWafer();
                //if(materialWafer != null)
                //{
                //    Log.Write(UnitName, "ChipMap", $"Total Chips found: {chips.Count}");
                //    return;
                //}

                materialWafer.Dies.Clear();
                materialWafer.MakeWaferInfo(chips, this.ChipPitchXmm, this.ChipPitchYmm);
                var list = materialWafer.Dies.OrderBy(t => t.MapX).ThenBy(t => t.MapY);
                foreach (var c in list)
                {
                    Log.Write(UnitName, "ChipMap", $"Chip: ,X={c.MapX}, Y={c.MapY}, PosX={c.CenterX:F3}, PosY{c.CenterY:F3}");

                }
            }
            catch (Exception ex )
            {
                Log.Write(ex);
            }finally
            {
                MaterialWafer materialWafer = GetMaterialWafer();
                EventUpdateUIWafer?.BeginInvoke(materialWafer,null,null);
            }
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

                double dRoiWidth = 0.0;// Math.Abs((PmRunner._Roi.InspectEnd.X - PmRunner._Roi.InspectStart.X) * StageCamera.CameraConfig.Scale.X);
                double dRoiHeight = 0.0;//Math.Abs((PmRunner._Roi.InspectEnd.Y - PmRunner._Roi.InspectStart.Y) * StageCamera.CameraConfig.Scale.Y);
                if (Config.IsSimulation == false && this.Config.IsDryRun == false)
                {
                    dRoiWidth = Math.Abs((PmRunner._Roi.InspectEnd.X - PmRunner._Roi.InspectStart.X) * StageCamera.CameraConfig.Scale.X);
                    dRoiHeight = Math.Abs((PmRunner._Roi.InspectEnd.Y - PmRunner._Roi.InspectStart.Y) * StageCamera.CameraConfig.Scale.Y);
                }
                else
                {
                    dRoiWidth = 2;
                    dRoiHeight = 2;
                }

                double dChipPitchX = ChipPitchXmm;
                double dChipPitchY = ChipPitchYmm;

                if (dChipPitchX <= 0) dChipPitchX = 0.5;
                if (dChipPitchY <= 0) dChipPitchY = 0.5;

                dRoiWidth -= dChipPitchX * 2;
                dRoiHeight -= dChipPitchY * 2;
                int nHorzCount = (int)((dRadius - dChipPitchX) * 2 / dRoiWidth) + 1;
                int nVertCount = (int)((dRadius - dChipPitchY) * 2 / dRoiHeight) + 1;
                if (nHorzCount < 1) nHorzCount = 1;
                if (nVertCount < 1) nVertCount = 1;
                double startX = centerTpX - (nHorzCount - 1) * dRoiWidth / 2;
                double startY = centerTpY - (nVertCount - 1) * dRoiHeight / 2;

                for (int ix = 0; ix < nHorzCount; ix++)
                {
                    double x = startX + ix * dRoiWidth;
                    for (int iy = 0; iy < nVertCount; iy++)
                    {
                        double y = startY + iy * dRoiHeight;

                        // 지그재그 패턴: X 열 기준으로 Y 스캔 방향 전환
                        if (ix % 2 == 1)
                        {
                            // 홀수 열은 Y를 반대 방향으로 스캔
                            y = startY + (nVertCount - 1 - iy) * dRoiHeight;
                        }

                        double dx = x - centerTpX;
                        double dy = y - centerTpY;
                       
                        double dist = Math.Sqrt(dx * dx + dy * dy);
                        double offsetDist = GetDistance(dRoiWidth / 2, dRoiHeight / 2);
                        if (dist <= dRadius + offsetDist)
                        {
                            path.Add(new PointD(x, y));
                        }
                    }
                }
                Log.Write(UnitName, "MakeScanPath", $"Count={path.Count} Radius={dRadius} Center=({centerTpX:F3},{centerTpY:F3}) ROI=({dRoiWidth:F3},{dRoiHeight:F3}) ChipPitch=({dChipPitchX:F3},{dChipPitchY:F3})");

            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
            //StageCamera.CameraConfig.Scale
        }
        public int PrepareInputStageUnloadingWafer()
        {
            int nRtn = 0;
            Log.Write(UnitName, "UnloadingPrep", "Start");
            
            if (!IsRingPresent())
            {
                Log.Write(UnitName, "UnloadingPrep", "No wafer -> Skip");
                return 0;
            }

            nRtn = MoveToStageUnloadPosition();
            if (nRtn != 0)
            {
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
            SetClampPlate(false);
            if (!IsPlateDown())
            {
                Log.Write(this, "Fail: PlateUp");
                return -1;
            }
            Log.Write(UnitName, "UnloadingPrep", "StageUnloadingReady = TRUE (Wait wafer pick)");
            return 0;
        }
        public int MoveToStageUnloadPosition(bool isFine = false)
        {
            int nRet = 0;
            if (IsWaferUnloadingPosition())
            {
                return 0; // 이미 로딩 위치에 있으면 무시
            }

            nRet = this.InputStageEjector.MovePositionEjectBlockSafety();
            if(nRet != 0)
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
                    PostAlarm((int)AlarmKeys.eInputStageEjectorPinZNotSafe);
                    return -1;
                }

                if (!InputStageEjector.IsEjectorZSafetyPos())
                {
                    this.AxisX.EmgStop();
                    this.AxisY.EmgStop();
                    this.AxisT.EmgStop();
                    PostAlarm((int)AlarmKeys.eInputStageEjectorZNotSafe);
                    return -1;
                }

                // DieTransfer PickZ Safety
                if (!InputDieTransfer.IsPositionPickZSafety())
                {
                    this.AxisX.EmgStop();
                    this.AxisY.EmgStop();
                    this.AxisT.EmgStop();
                    PostAlarm((int)AlarmKeys.eDieTransferPickZNotSafe);
                    return -1;
                }

                if (!InputFeeder.IsFeederZSafetyPosition())
                {
                    this.AxisX.EmgStop();
                    this.AxisY.EmgStop();
                    this.AxisT.EmgStop();
                    PostAlarm((int)AlarmKeys.eInputFeederCylinderZNotSafe);
                    return -1;
                }

                if (!InputFeeder.IsFeederYSafetyPosition())
                {
                    this.AxisX.EmgStop();
                    this.AxisY.EmgStop();
                    this.AxisT.EmgStop();
                    PostAlarm((int)AlarmKeys.eInputFeederYNotSafe);
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

        //UnloadingWaferComplete
        public int UnloadingWaferComplete()
        {
            int nRtn = 0;

            if (IsRingPresent())
            {
                Log.Write(UnitName, "UnloadingComp", "Not prepared");
                return -1;
            }
            Log.Write(UnitName, "UnloadingComp", "Done");
            return nRtn;
        }
        #endregion

        #region CHIP MAPPING / PICKUP
        // 매핑 파라미터 (Config 로 승격 가능)
        public double MappingRoiWidthMm { get; set; } = 2.0;
        public double MappingRoiHeightMm { get; set; } = 2.0;
        public double ChipPitchXmm 
        {
            get 
            {
                var eq = Equipment.Instance;
                var recip = eq.EquipmentRecipe.CurrentRecipe;
                return recip.ChipWidth;
            }
            set 
            {
                var eq = Equipment.Instance;
                var recip = eq.EquipmentRecipe.CurrentRecipe;
                recip.ChipWidth = value;
            } 
        }
        public double ChipPitchYmm
        {
            get
            {
                var eq = Equipment.Instance;
                var recip = eq.EquipmentRecipe.CurrentRecipe;
                return recip.ChipHeight;
            }
            set
            {
                var eq = Equipment.Instance;
                var recip = eq.EquipmentRecipe.CurrentRecipe;
                recip.ChipHeight = value;
            }
        }
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

        public bool IsCompletedWork()
        {
            bool bRet = false;
            try
            {
                var wafer = GetMaterialWafer();
                if (wafer == null)
                    return false;

                if (wafer.Presence == Material.MaterialPresence.Exist)
                {
                    if (wafer.ProcessSatate == Material.MaterialProcessSatate.Completed)
                    {
                        bRet = true;
                    }
                }
            }
            catch
            {
                bRet = false;
            }
            return bRet;

        }

        public bool IsWorking()
        {
            bool bRet = false;
            try
            {
                var wafer = GetMaterialWafer();
                if (wafer == null)
                    return false;

                if (wafer.Presence == Material.MaterialPresence.Exist)
                {
                    if (wafer.ProcessSatate != Material.MaterialProcessSatate.Completed)
                    {
                        bRet = true;
                    }
                }
            }
            catch
            {
                bRet = false;
            }
            return bRet;
        }

        public int MoveStageToNextDie(out MaterialDie die)
        {
            int nRet = 0;
             die = GetNextDie();
            if(die == null)
            {
                return -1;
            }
            if(die.Presence != MaterialPresence.Exist)
            {
                return -1;
            }
            nRet = MoveStage(die.CenterX, die.CenterY, false);
            return nRet;
        }

        public MaterialDie GetNextDie()
        {
            MaterialDie die = null;
            try
            {
                var wafer = GetMaterialWafer();
                if (wafer == null)
                    return null;
                lock(wafer)
                {
                    if (wafer.Presence == Material.MaterialPresence.Exist)
                    {
                        if (wafer.ProcessSatate != Material.MaterialProcessSatate.Completed)
                        {
                            if(wafer.ProcessSatate == Material.MaterialProcessSatate.Processing)
                            {
                                var v = wafer.Dies.Where(t => t.Presence == Material.MaterialPresence.Exist
                                && t.State == DieProcessState.Mapped).OrderBy(t => t.Index);
                                if(v.Count() > 0)
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
            catch
            {
                die = null;
            }
            return die;
        }

        public int RecheckDieAndAlign(bool bFineSpeed = false)
        {
            int nRet = 0;
            List<PointD> chips = new List<PointD>();
            Task<int> tImageProcess = null;
            try
            {
                this.CalcelToken?.Token.ThrowIfCancellationRequested();
                if (this.Config.IsSimulation == false && this.Config.IsDryRun == false)
                {
                    double dpoX = AxisX.GetPosition();
                    double dpoY = AxisY.GetPosition();

                    StageCamera.GrabSync(out VisionImage grabImage);
                    if (tImageProcess != null)
                    {
                        tImageProcess.Wait();
                    }
                    double dx = dpoX;
                    double dy = dpoY;
                    tImageProcess = Task.Factory.StartNew(() =>
                    {
                        return SearchDies(grabImage, ref chips, dx, dy);
                    });
                    tImageProcess.Wait();

                    var wafer = GetMaterialWafer();
                

                    wafer.UpdateChipInfo(chips, this.ChipPitchXmm, this.ChipPitchYmm);

                    //Update Chip Info가 되어야 한다.....
                    //wafer die 정보가 갱신되어야 한다.


                }
                if (nRet != 0)
                {
                    Log.Write(UnitName, "ChipMap", "Fail: GrabAndMap");
                    return -1;
                }

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
    }
}