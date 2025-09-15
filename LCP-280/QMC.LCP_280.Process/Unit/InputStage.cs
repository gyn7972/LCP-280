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

            Config.IsSimulation = false;
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
        public override int MoveAxisWithSafety(MotionAxis axis, double target, bool isFine = false)
        {
            if (axis == null) return -1;

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

        protected override int CheckMoveSafety(MotionAxis ax)
        {
            //if (/*다른 유닛 축 이동중*/) return (int)AlarmKeys.xxx;
            return 0;
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
            if (AxisX != null) rc |= MoveAxisWithSafety(AxisX, x, false);
            if (AxisY != null) rc |= MoveAxisWithSafety(AxisY, y, false);
            if (AxisT != null) rc |= MoveAxisWithSafety(AxisT, t, false);
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
        private bool ActAndWait(string tag, Func<bool> act, Func<bool> cond)
        {
            if (!act())
            {
                Log.Write(UnitName, "Seq", $"Fail Act {tag}");
                return false;
            }

            if (!WaitIO(cond, MoveTimeoutMs))
            {
                Log.Write(UnitName, "Seq", $"Timeout {tag}");
                return false;
            }
            return true;
        }
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
        public bool SetVacuum(bool on)
        {
            if (_vacuum == null) return false;
            if (on) _vacuum.On();
            else _vacuum.Off();
            return true;
        }
        public bool SetClampPlate(bool bUpDn)
        {
            if (_cylPlate == null) return false;
            if (bUpDn) return _cylPlate.Extend();
            else return _cylPlate.Retract();
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
        public bool IsVacuum()
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
        private int WaitUntilInPos(TeachingPosition tp, int timeoutMs)
        {
            var sw = Stopwatch.StartNew();
            while (sw.ElapsedMilliseconds < timeoutMs)
            {
                if (InPosTeaching(tp))
                    return 0;
                Thread.Sleep(PollIntervalMs);
            }
            return -1;
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
        public bool IsRequestWafer { get; internal set; } = false;

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
                if (!ActAndWait("PlateUp", () => SetClampPlate(true), () => IsPlateUp()))
                {
                    Log.Write(this, "Fail: PlateUp");
                    return -1;
                }

                int rc = LoadingWaferComplete();
                if (rc != 0 && rc != 0)
                    return rc; // rc !=0 이면 오류. (준비단계는 OK=0 외 다른 코드 없음)

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
                ret = LoadingWafer();
                if (ret != 0)
                {
                    State = ProcessState.Error;
                    Log.Write(this, "LoadingWafer Failed");
                    return -1;
                }
                else
                {
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
                IsStatus_StageLoadingDone = true;
                IsRequestWafer = false;

                ret = AlignXY();
                if (ret != 0)
                {
                    State = ProcessState.Error;
                    Log.Write(this, "AlignXY Failed");
                    return -1;
                }
                else
                {
                    State = ProcessState.Complete;
                }
            }

            return 0;
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
            IsStatus_StageLoadingDone = false;
            IsStatus_StageLoadingReady = false;

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
            if (!ActAndWait("ClampBack", () => !SetClampFB(false), () => IsClampBwd()))
            {
                Log.Write(this, "Fail: ClampBack");
                return -1;
            }
            if (!ActAndWait("ClampLiftDown", () => !SetClampLift(false), () => IsClampLiftDown()))
            {
                Log.Write(this, "Fail: ClampLiftDown");
                return -1;
            }
            //Plate Up → 
            if (!ActAndWait("PlateUp", () => !SetClampPlate(false), () => IsPlateDown()))
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
                    if (!ActAndWait("PlateUp", () => SetClampPlate(true), () => IsPlateUp()))
                    {
                        Log.Write(this, "Fail: PlateUp");
                        return -1;
                    }

                    if (ActAndWait("ClampLiftUp", () => SetClampLift(true), () => IsClampLiftUp()))
                        return -1;
                    if (!ActAndWait("ClampForward", () => SetClampFB(true), () => IsClampFwd()))
                        return -1;
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

        // ====== Align Refactor: 상태/결과 보관 필드 ======
        public bool TAlignPrepared { get; private set; }
        public bool TAlignDone { get; private set; }
        public double LastFoundTRawAngle { get; private set; }
        public double LastAppliedTAngle { get; private set; }
        public bool XYAlignPrepared { get; private set; }
        public bool XYAlignDone { get; private set; }
        public double LastFoundDx { get; private set; }
        public double LastFoundDy { get; private set; }
        
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
            TAlignPrepared = false;
            TAlignDone = false;
            LastFoundTRawAngle = 0;
            LastAppliedTAngle = 0;
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
            LastFoundTRawAngle = rawAngle;
            _lastCenterAlignTp = centerTp;

            Log.Write(UnitName, "T_Align",
                $"Angle Representative={rawAngle:F6} avg={stats.Average:F6} std={stats.StdDev:F6} rawCount={stats.RawCount}");

            TAlignPrepared = true;
            return 0;
        }
        /// <summary>
        /// T 정렬 적용 (AlignTPrepare 먼저 호출)
        /// </summary>
        public int AlignTApply()
        {
            if (!TAlignPrepared || _lastCenterAlignTp == null)
            {
                Log.Write(UnitName, "T_Align", "Not prepared");
                return -1;
            }

            double rawAngle = LastFoundTRawAngle;
            if (Math.Abs(rawAngle) < AngleIgnoreThresholdDeg)
            {
                Log.Write(UnitName, "T_Align", $"Skip: |{rawAngle:F6}| < Ignore({AngleIgnoreThresholdDeg})");
                TAlignDone = true;
                return 0;
            }
            if (Math.Abs(rawAngle) > AngleMaxApplyDeg)
            {
                Log.Write(UnitName, "T_Align",
                    $"Fail: Angle {rawAngle:F4} > Limit {AngleMaxApplyDeg}");
                return -1;
            }

            double applyAngle = rawAngle * AngleApplyGain;
            LastAppliedTAngle = applyAngle;

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

            TAlignDone = true;
            return 0;
        }
        /// <summary>
        /// 기존 호환: 한번에 실행 (Prepare + Apply)
        /// </summary>
        public int AlignT()
        {
            int rc = AlignTPrepare();
            if (rc != 0) return rc;
            return AlignTApply();
        }

        // ===================== XY ALIGN 분리 =====================
        /// <summary>
        /// XY 정렬 준비 + Vision Offset 획득
        /// </summary>
        public int AlignXYPrepare()
        {
            XYAlignPrepared = false;
            XYAlignDone = false;
            LastFoundDx = 0;
            LastFoundDy = 0;
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

            LastFoundDx = res.x;
            LastFoundDy = res.y;
            _lastCenterAlignTp = centerTp;

            Log.Write(UnitName, "XY_Align",
                $"Offset dx={LastFoundDx:F6} dy={LastFoundDy:F6}");

            XYAlignPrepared = true;
            return 0;
        }
        /// <summary>
        /// XY 정렬 적용 (AlignXYPrepare 먼저)
        /// </summary>
        public int AlignXYApply()
        {
            if (!XYAlignPrepared || _lastCenterAlignTp == null)
            {
                Log.Write(UnitName, "XY_Align", "Not prepared");
                return -1;
            }

            double dx = LastFoundDx;
            double dy = LastFoundDy;

            if (Math.Abs(dx) < 0.0001 && Math.Abs(dy) < 0.0001)
            {
                Log.Write(UnitName, "XY_Align", "Skip: offset under threshold");
                XYAlignDone = true;
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

            XYAlignDone = true;
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
            if (!ActAndWait("PlateUp", () => SetClampPlate(true), () => IsPlateUp())) 
                return -1;
            // Clamp Back (웨이퍼 픽업 전 클램프 해제)
            if (!ActAndWait("ClampBack", () => SetClampFB(false), () => IsClampBwd())) 
                return -1;
            // Lift Down (픽업 접근 공간 확보)
            if (!ActAndWait("ClampLiftDown", () => SetClampLift(false), () => IsClampLiftDown())) 
                return -1;

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
            if (!ActAndWait("PlateDown", () => SetClampPlate(false), () => IsPlateDown())) 
                return -1;

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
    }
}