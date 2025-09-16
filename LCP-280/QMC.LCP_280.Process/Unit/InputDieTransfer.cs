using QMC.Common;
using QMC.Common.Alarm;
using QMC.Common.Component;
using QMC.Common.IOUtil;
using QMC.Common.Motion;
using QMC.Common.Motions;
using QMC.Common.Unit;
using QMC.LCP_280.Process.Component;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms.DataVisualization.Charting;
using System.Windows.Media;
using static QMC.LCP_280.Process.Equipment;

namespace QMC.LCP_280.Process.Unit
{
    /// <summary>
    /// InputDieTransfer Unit
    ///  - Tool T / Pick Z / Place Z 축 제어 + Teaching Position 및 Offset
    ///  - 4 Arm Vacuum / Blow / Vent 제어
    ///  - Air/Vac Tank Pressure / Arm Flow 등의 입력
    ///  - DryRun 시뮬레이션 지원
    ///  - OutputStage 스타일과 Region/메서드 레이아웃 통일
    /// </summary>
    public class InputDieTransfer : BaseUnit<InputDieTransferConfig>
    {
        public enum AlarmKeys
        {
            eInputStageNotSafe = 4001,
            eRotatyNotSafe,
            eInputStageEjectorPinZNotSafe,
            eInputStageEjectorZNotSafe,
            eInputStageAxesMoving = 4010,
            eRotaryAxesMoving,
            eInputStageEjectorAxesMoving,

        }
        #region InitAlarm
        protected override void InitAlarm()
        {
            base.InitAlarm();
            AlarmInfo alarm = new AlarmInfo();
            alarm.Code = (int)AlarmKeys.eInputStageNotSafe;
            alarm.Title = "InputStage Not Sfarety Pos.";
            alarm.Cause = "InputStage가 안전 위치가 아닙니다.\n 포지션 확인 후 다시 시작 하십시요.";
            alarm.Source = this.UnitName;
            alarm.Grade = AlarmInfo.AlarmType.Warning.ToString();
            m_dicAlarms.Add(alarm.Code, alarm);

            alarm = new AlarmInfo();
            alarm.Code = (int)AlarmKeys.eRotatyNotSafe;
            alarm.Title = "Rotaty Not Sfarety Pos.";
            alarm.Cause = "Rotaty가 안전 위치가 아닙니다.\n 포지션 확인 후 다시 시작 하십시요.";
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

            alarm = new AlarmInfo();
            alarm.Code = (int)AlarmKeys.eInputStageAxesMoving;
            alarm.Title = "InputStage Axis Moving";
            alarm.Cause = "InputStage 축이 이동 중입니다. 정지 후 다시 시도하십시오.";
            alarm.Source = this.UnitName;
            alarm.Grade = AlarmInfo.AlarmType.Warning.ToString();
            m_dicAlarms.Add(alarm.Code, alarm);

            alarm = new AlarmInfo();
            alarm.Code = (int)AlarmKeys.eRotaryAxesMoving;
            alarm.Title = "Rotary Axis Moving";
            alarm.Cause = "Rotary 축이 이동 중입니다. 정지 후 다시 시도하십시오.";
            alarm.Source = this.UnitName;
            alarm.Grade = AlarmInfo.AlarmType.Warning.ToString();
            m_dicAlarms.Add(alarm.Code, alarm);

            alarm = new AlarmInfo();
            alarm.Code = (int)AlarmKeys.eInputStageEjectorAxesMoving;
            alarm.Title = "Ejector Axis Moving";
            alarm.Cause = "InputStageEjector 축이 이동 중입니다. 정지 후 다시 시도하십시오.";
            alarm.Source = this.UnitName;
            alarm.Grade = AlarmInfo.AlarmType.Warning.ToString();
            m_dicAlarms.Add(alarm.Code, alarm);
        }
        #endregion

        #region Config / Teaching
        public InputDieTransferConfig InputDieTransferConfig => Config;
        #endregion

        #region DryRun Simulation
        public bool DryRun { get; private set; }
        public void SetDryRun(bool on) => DryRun = on;
        private readonly Dictionary<string, bool> _simOutputs = new Dictionary<string, bool>(System.StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, bool> _simInputs  = new Dictionary<string, bool>(System.StringComparer.OrdinalIgnoreCase);
        #endregion

        #region Unit
        InputStage InputStage { get; set; }
        InputStageEjector InputStageEjector { get; set; }
        Rotary Rotary { get; set; }
        #endregion

        #region Axes
        private MotionAxis _toolT, _pickZ, _placeZ;
        public MotionAxis AxisToolT => _toolT;
        public MotionAxis AxisPickZ => _pickZ;
        public MotionAxis AxisPlaceZ => _placeZ;
        #endregion

        #region ctor / Initialization
        public InputDieTransfer(InputDieTransferConfig config = null) : base(config ?? new InputDieTransferConfig())
        {
            AddComponents();
        }

        public override void AddComponents()
        {
            Config.LoadAndBindAxes(Equipment.Instance.AxisManager);
            Config.InitializeDefaultTeachingPositions();
            
            BindAxes();
            BindIoDomains();    // (Arm IO 는 단순 DO/DI 이름 관리이므로, 별도 Cylinder/Vacuum Domain 매핑은 선택)
        }
        #endregion

        protected override void OnBindUnit()
        {
            base.OnBindUnit();
            InputStage = Equipment.Instance.GetUnit(UnitKeys.InputStage) as InputStage;
            InputStageEjector = Equipment.Instance.GetUnit(UnitKeys.InputStageEjector) as InputStageEjector;
            Rotary = Equipment.Instance.GetUnit(UnitKeys.Rotary) as Rotary;
        }

        #region Axis Binding / Helpers
        private void BindAxes()
        {
            var mgr = Equipment.Instance?.AxisManager;
            if (mgr == null)
            {
                Log.Write("InputDieTransfer", "[BindAxes] AxisManager null");
                return;
            }

            const string unitName = "Unit"; // Equipment에서 축 등록 시 사용한 유닛명과 동일해야 함
            BindAxis(mgr, unitName, AxisNames.LeftToolT, ref _toolT);
            BindAxis(mgr, unitName, AxisNames.LeftPickZ, ref _pickZ);
            BindAxis(mgr, unitName, AxisNames.LeftPlaceZ, ref _placeZ);
        }

        public override int MoveAxisWithSafety(MotionAxis axis, double target, bool isFine = false)
        {
            if (axis == null) return -1;

            Task<int> task = MoveAxisWithSafetyAsync(axis, target, isFine);
            while (IsEndTask(task) == false)
            {
                if(axis == AxisPickZ)
                {
                    if (InputStage.IsAnyAxisMoving())
                    {
                        AxisToolT.EmgStop();
                        AxisPickZ.EmgStop();
                        AxisPlaceZ.EmgStop();
                        AlarmPost((int)AlarmKeys.eInputStageAxesMoving);
                        return -1;
                    }

                    if (InputStageEjector.IsAxisMoving(AxisNames.EjectorZ))
                    {
                        AxisToolT.EmgStop();
                        AxisPickZ.EmgStop();
                        AxisPlaceZ.EmgStop();
                        AlarmPost((int)AlarmKeys.eInputStageEjectorAxesMoving);
                    }
                    //if (InputStageEjector.IsAnyAxisMoving())
                    //{
                    //    AxisToolT.EmgStop();
                    //    AxisPickZ.EmgStop();
                    //    AxisPlaceZ.EmgStop();
                    //    AlarmPost((int)AlarmKeys.eInputStageEjectorAxesMoving);
                    //}
                }

                if (axis == AxisPlaceZ)
                {
                    if (Rotary.IsAnyAxisMoving())
                    {
                        AxisToolT.EmgStop();
                        AxisPickZ.EmgStop();
                        AxisPlaceZ.EmgStop();
                        AlarmPost((int)AlarmKeys.eRotaryAxesMoving);
                    }
                }

                Thread.Sleep(0);
            }
            return task.Result;
        }

        public int MovePickUpPosition(bool isFine = false)
        {
            Task<int> task = MovePickUpPositionAsync(isFine);
            while (IsEndTask(task) == false)
            {
                // Check Interlock.!!! 구문 넣을것.!!!
                if (InputStage.IsAnyAxisMoving())
                {
                    AxisToolT.EmgStop();
                    AxisPickZ.EmgStop();
                    AxisPlaceZ.EmgStop();
                    AlarmPost((int)AlarmKeys.eInputStageAxesMoving);
                    return -1;
                }

                if (InputStageEjector.IsAnyAxisMoving())
                {
                    AxisToolT.EmgStop();
                    AxisPickZ.EmgStop();
                    AxisPlaceZ.EmgStop();
                    AlarmPost((int)AlarmKeys.eInputStageEjectorAxesMoving);
                }

                if (Rotary.IsAnyAxisMoving())
                {
                    AxisToolT.EmgStop();
                    AxisPickZ.EmgStop();
                    AxisPlaceZ.EmgStop();
                    AlarmPost((int)AlarmKeys.eRotaryAxesMoving);
                }

                Thread.Sleep(0);
            }
            return task.Result;
        }
        public Task<int> MovePickUpPositionAsync(bool isFine = false)
        {
            return Task.Run(() =>
            {
                OnMovePickUpPosition(isFine);
                return 0;
            });
        }
        private int OnMovePickUpPosition(bool isFine = false)
        {
            return MoveTeachingPositionOnce((int)InputDieTransferConfig.TeachingPositionName.Pickup, isFine);
        }


        #region Dual Axis (PickZ + PinZ) Simultaneous Move
        /// <summary>
        /// PickZ 와 PinZ 를 Offset(상대이동)으로 동시에 구동.
        ///  - 두 축 모두 상대이동 (MoveRel) 사용
        ///  - velPickZ / velPinZ = 0 이면 각 축 설정(MaxVelocity/RunAcc/RunDec) 사용
        ///  - timeoutMs > 0 이고 시간 초과 시 -2 반환
        ///  - Interlock 위반 시 두 축 Emergency Stop 후 -1 반환
        /// </summary>
        public int MovePickZAndPinZByOffset(double pickZOffset,
                                            double pinZOffset,
                                            double velPickZ = 0,
                                            double velPinZ = 0,
                                            double acc = 0,
                                            double dec = 0,
                                            int timeoutMs = 0,
                                            bool isFine = false)
        {
            var pick = AxisPickZ;
            var pin = InputStageEjector != null ? InputStageEjector.AxisPinZ : null;

            if (pick == null || pin == null)
            {
                Log.Write(UnitName, "[MovePickZAndPinZByOffset] Axis null");
                return -1;
            }

            // 이동 필요 없으면 즉시 성공
            if (System.Math.Abs(pickZOffset) < 1e-9 && System.Math.Abs(pinZOffset) < 1e-9)
                return 0;

            // 사전 Interlock (다른 관련 Unit 축 동작 중이면 시작하지 않음)
            if (InputStage != null && InputStage.IsAnyAxisMoving())
            {
                AlarmPost((int)AlarmKeys.eInputStageAxesMoving);
                return -1;
            }
            //if (Rotary != null && Rotary.IsAnyAxisMoving())
            //{
            //    AlarmPost((int)AlarmKeys.eRotaryAxesMoving);
            //    return -1;
            //}
            if (InputStageEjector != null && InputStageEjector.IsAnyAxisMoving())
            {
                AlarmPost((int)AlarmKeys.eInputStageEjectorAxesMoving);
                return -1;
            }

            double vPick = velPickZ > 0 ? velPickZ : pick.Config.MaxVelocity;
            double aPick = acc > 0 ? acc : pick.Config.RunAcc;
            double dPick = dec > 0 ? dec : pick.Config.RunDec;

            double vPin = velPinZ > 0 ? velPinZ : pin.Config.MaxVelocity;
            double aPin = acc > 0 ? acc : pin.Config.RunAcc;
            double dPin = dec > 0 ? dec : pin.Config.RunDec;

            // 동시에 시작 (반환코드 OR)
            //ex) Offset값이 양수로 300 이면 Z축이 위로 300 이동
            // 두 개의 축 전부 300이면 동일하게 위로 올라간다.
            int rc = 0;
            rc |= pick.MoveRel(pickZOffset, vPick, aPick, dPick, pick.Config.AccJerkPercent);
            rc |= pin.MoveRel(pinZOffset, vPin, aPin, dPin, pin.Config.AccJerkPercent);
            if (rc != 0)
            {
                Log.Write(UnitName, "[MovePickZAndPinZByOffset] MoveRel start failed rc=" + rc);
                return -1;
            }

            var sw = timeoutMs > 0 ? Stopwatch.StartNew() : null;

            while (true)
            {
                bool pickMoving = pick.IsMoveDone();
                bool pinMoving = pin.IsMoveDone();

                // 완료
                if (pickMoving && pinMoving)
                    break;

                // 진행 중 Interlock 감시 (기존 MoveAxisWithSafety 로직과 유사)
                if (InputStage != null && InputStage.IsAnyAxisMoving())
                {
                    pick.EmgStop(); pin.EmgStop();
                    AlarmPost((int)AlarmKeys.eInputStageAxesMoving);
                    return -1;
                }
                if (Rotary != null && Rotary.IsAnyAxisMoving())
                {
                    pick.EmgStop(); pin.EmgStop();
                    AlarmPost((int)AlarmKeys.eRotaryAxesMoving);
                    return -1;
                }
                // Ejector 다른 축(EjectorZ) 움직임 감시
                if (InputStageEjector != null && InputStageEjector.IsAxisMoving(AxisNames.EjectorZ))
                {
                    pick.EmgStop(); pin.EmgStop();
                    AlarmPost((int)AlarmKeys.eInputStageEjectorAxesMoving);
                    return -1;
                }

                // 타임아웃
                if (sw != null && sw.ElapsedMilliseconds > timeoutMs)
                {
                    pick.EmgStop(); pin.EmgStop();
                    Log.Write(UnitName, "[MovePickZAndPinZByOffset] Timeout");
                    return -2;
                }

                Thread.Sleep(1);
            }

            return 0;
        }

        /// <summary>
        /// 비동기 버전 (Task 반환). 필요 시 UI 에서 await 사용.
        /// </summary>
        public Task<int> MovePickZAndPinZByOffsetAsync(double pickZOffset,
                                                       double pinZOffset,
                                                       double velPickZ = 0,
                                                       double velPinZ = 0,
                                                       double acc = 0,
                                                       double dec = 0,
                                                       int timeoutMs = 0,
                                                       bool isFine = false)
        {
            return Task.Run(() =>
            {
                return MovePickZAndPinZByOffset(pickZOffset, pinZOffset, velPickZ, velPinZ, acc, dec, timeoutMs, isFine);
            });
        }
        #endregion


        public bool InPos(MotionAxis ax, double target) => ax == null || ax.InPosition(target);
        public double GetTP(string tpName, string axisName)
        {
            var tp = Config.GetTeachingPosition(tpName);
            if (tp != null && tp.AxisPositions != null && tp.AxisPositions.TryGetValue(axisName, out var v)) return v;
            return 0.0;
        }
        #endregion

        #region Teaching Helpers
        public int MoveToTeachingPosition(string name, double vel = 0, double acc = 0, double dec = 0, double jerk = 0)
        {
            var tp = Config.GetTeachingPosition(name);
            if (tp == null) return -1;
            var (t, pz, plz) = Config.GetPositionWithOffset(name);
            int rc = 0;
            if (_toolT != null)  rc |= _toolT.MoveAbs(t,   vel > 0 ? vel : _toolT.Config.MaxVelocity,  acc > 0 ? acc : _toolT.Config.RunAcc,  dec > 0 ? dec : _toolT.Config.RunDec,  jerk > 0 ? jerk : _toolT.Config.AccJerkPercent);
            if (_pickZ != null)  rc |= _pickZ.MoveAbs(pz,  vel > 0 ? vel : _pickZ.Config.MaxVelocity,  acc > 0 ? acc : _pickZ.Config.RunAcc,  dec > 0 ? dec : _pickZ.Config.RunDec,  jerk > 0 ? jerk : _pickZ.Config.AccJerkPercent);
            if (_placeZ != null) rc |= _placeZ.MoveAbs(plz, vel > 0 ? vel : _placeZ.Config.MaxVelocity, acc > 0 ? acc : _placeZ.Config.RunAcc, dec > 0 ? dec : _placeZ.Config.RunDec, jerk > 0 ? jerk : _placeZ.Config.AccJerkPercent);
            return rc;
        }
        public bool InPosTeaching(string name)
        {
            var (t, pz, plz) = Config.GetPositionWithOffset(name);
            return InPos(_toolT, t) && InPos(_pickZ, pz) && InPos(_placeZ, plz);
        }

        /// DieTransfer PickZ 축이 SafetyPos Teaching (Offset 적용) 위치(또는 허용오차 범위)인지 확인.
        /// Teaching 이름이 SafetyPos 없으면 SafetyZone 순으로 fallback (둘 다 없으면 false).
        /// 장치/축이 없으면 true(안전)로 간주. 필요 시 treatMissingAsSafe=false 로 변경 가능.
        /// </summary>
        /// <param name="fallbackTolerance">축 설정값을 못 가져올 때 사용할 기본 허용오차</param>
        /// <param name="useAxisInposTolerance">축 Config.InposTolerance 사용 여부</param>
        /// <param name="treatMissingAsSafe">장치/Teaching 미존재 시 true 반환할지 여부</param>
        public bool IsDieTransferPickZSafetyPos(double fallbackTolerance = 0.01,
                                                 bool useAxisInposTolerance = true,
                                                 bool treatMissingAsSafe = true)
        {
            if (AxisPickZ == null)
                return treatMissingAsSafe;

            var cfg = InputDieTransferConfig;
            if (cfg == null) return false;

            // 우선순위: SafetyPos → SafetyZone
            string[] candidateNames =
            {
                "SafetyPos",
                InputDieTransferConfig.TeachingPositionName.SafetyZone.ToString()
            };

            string foundName = null;
            foreach (var name in candidateNames)
            {
                if (cfg.GetTeachingPosition(name) != null)
                {
                    foundName = name;
                    break;
                }
            }

            if (foundName == null)
                return treatMissingAsSafe ? true : false;

            var tp = cfg.GetTeachingPosition(foundName);
            if (tp == null) return false;

            // Offset 적용 PickZ 목표값
            var (_, pickZTarget, _) = cfg.GetPositionWithOffset(foundName);

            double cur = AxisPickZ.GetPosition();
            double tol = useAxisInposTolerance
                ? (AxisPickZ.Config?.InposTolerance ?? fallbackTolerance)
                : fallbackTolerance;

            // 동일위치(=InPos) 판정
            return System.Math.Abs(cur - pickZTarget) <= tol;
        }

        /// <summary>
        /// DieTransfer ToolT 축이 SafetyPos(or SafetyZone fallback) 위치인지 확인.
        /// SafetyZone Teaching에 ToolT 값이 없으면 다음 후보로 넘어감.
        /// </summary>
        public bool IsDieTransferToolTSafetyPos(double fallbackTolerance = 0.01,
                                                 bool useAxisInposTolerance = true,
                                                 bool treatMissingAsSafe = true)
        {
            if (AxisToolT == null)
                return treatMissingAsSafe;

            var cfg = InputDieTransferConfig;
            if (cfg == null) return false;

            string[] candidateNames =
            {
                "SafetyPos",
                InputDieTransferConfig.TeachingPositionName.SafetyZone.ToString()
            };

            string foundName = null;
            foreach (var name in candidateNames)
            {
                var tpTest = cfg.GetTeachingPosition(name);
                if (tpTest == null) continue;
                // 해당 Teaching에 ToolT 좌표가 실제 존재하는지 확인 (없으면 스킵)
                if (tpTest.AxisPositions != null &&
                    tpTest.AxisPositions.Keys.Any(k => string.Equals(k, AxisNames.LeftToolT, StringComparison.OrdinalIgnoreCase)))
                {
                    foundName = name;
                    break;
                }
            }

            if (foundName == null)
                return treatMissingAsSafe;

            var (_, _, _) = cfg.GetPositionWithOffset(foundName);
            // Offset 적용 튜플에서 t 사용
            var (tTarget, _, _) = cfg.GetPositionWithOffset(foundName);

            double cur = AxisToolT.GetPosition();
            double tol = useAxisInposTolerance
                ? (AxisToolT.Config?.InposTolerance ?? fallbackTolerance)
                : fallbackTolerance;

            return System.Math.Abs(cur - tTarget) <= tol;
        }

        /// <summary>
        /// DieTransfer PlaceZ 축이 SafetyPos(or SafetyZone fallback) 위치인지 확인.
        /// </summary>
        public bool IsDieTransferPlaceZSafetyPos(double fallbackTolerance = 0.01,
                                                  bool useAxisInposTolerance = true,
                                                  bool treatMissingAsSafe = true)
        {
            if (AxisPlaceZ == null)
                return treatMissingAsSafe;

            var cfg = InputDieTransferConfig;
            if (cfg == null) return false;

            string[] candidateNames =
            {
                "SafetyPos",
                InputDieTransferConfig.TeachingPositionName.SafetyZone.ToString()
            };

            string foundName = null;
            foreach (var name in candidateNames)
            {
                if (cfg.GetTeachingPosition(name) != null)
                {
                    foundName = name;
                    break;
                }
            }

            if (foundName == null)
                return treatMissingAsSafe;

            var (_, _, placeZTarget) = cfg.GetPositionWithOffset(foundName);

            double cur = AxisPlaceZ.GetPosition();
            double tol = useAxisInposTolerance
                ? (AxisPlaceZ.Config?.InposTolerance ?? fallbackTolerance)
                : fallbackTolerance;

            return System.Math.Abs(cur - placeZTarget) <= tol;
        }

        public void ApplyOffset(string name, double t, double pickZ, double placeZ)
            => Config.SetOffset(name, t, pickZ, placeZ);
        #endregion

        #region Low-Level IO (Name Based + DryRun)
        public bool ReadInput(string name)
        {
            if (DryRun) { return _simInputs.TryGetValue(name, out var v) && v; }
            var hi = Config.HardInputs.FirstOrDefault(i => i.Name.Equals(name, System.StringComparison.OrdinalIgnoreCase));
            if (hi == null) return false;
            var eq = Equipment.Instance; var dio = eq?.DioScan; if (dio == null) return false;
            foreach (var m in eq.UnitIO.Modules)
                if (dio.TryGetInput(m.ModuleName, hi.Disp, out var v)) return v;
            return false;
        }
        public bool WriteOutput(string name, bool on)
        {
            if (DryRun) { _simOutputs[name] = on; return true; }
            var ho = Config.HardOutputs.FirstOrDefault(o => o.Name.Equals(name, System.StringComparison.OrdinalIgnoreCase));
            if (ho == null) return false;
            var eq = Equipment.Instance; var dio = eq?.DioScan; if (dio == null) return false;
            foreach (var m in eq.UnitIO.Modules)
                if (dio.WriteOutput(m.ModuleName, ho.Disp, on) == 0) return true;
            return false;
        }
        #endregion

        #region Arm Vacuum / Blow / Vent Control
        private static readonly string[] VAC_NAMES  = { InputDieTransferConfig.IO.ARM1_VAC,  InputDieTransferConfig.IO.ARM2_VAC,  InputDieTransferConfig.IO.ARM3_VAC,  InputDieTransferConfig.IO.ARM4_VAC };
        private static readonly string[] BLOW_NAMES = { InputDieTransferConfig.IO.ARM1_BLOW, InputDieTransferConfig.IO.ARM2_BLOW, InputDieTransferConfig.IO.ARM3_BLOW, InputDieTransferConfig.IO.ARM4_BLOW };
        private static readonly string[] VENT_NAMES = { InputDieTransferConfig.IO.ARM1_VENT, InputDieTransferConfig.IO.ARM2_VENT, InputDieTransferConfig.IO.ARM3_VENT, InputDieTransferConfig.IO.ARM4_VENT };

        private Vacuum[] _vacuum = new Vacuum[4];              // Vacuum + OK sensor
        public Vacuum[] _blow = new Vacuum[4];
        public Vacuum[] _vent = new Vacuum[4];

        private void BindIoDomains()
        {
            var eq = Equipment.Instance; var unit = eq?.UnitIO; if (unit == null) return;

            // Vacuum 별칭으로 조회만
            if (!IoAutoBindings.Vacuums.TryGetValue("InputDieTransferVac1", out _vacuum[0]))
            {
                Log.Write("InputDieTransfer", "BindIoDomains", "Vacuums not found: InputDieTransferVac1");
            }

            if (!IoAutoBindings.Vacuums.TryGetValue("InputDieTransferVac2", out _vacuum[1]))
            {
                Log.Write("InputDieTransfer", "BindIoDomains", "Vacuums not found: InputDieTransferVac2");
            }

            if (!IoAutoBindings.Vacuums.TryGetValue("InputDieTransferVac3", out _vacuum[2]))
            {
                Log.Write("InputDieTransfer", "BindIoDomains", "Vacuums not found: InputDieTransferVac3");
            }

            if (!IoAutoBindings.Vacuums.TryGetValue("InputDieTransferVac4", out _vacuum[3]))
            {
                Log.Write("InputDieTransfer", "BindIoDomains", "Vacuums not found: InputDieTransferVac4");
            }

            if (!IoAutoBindings.Vacuums.TryGetValue("InputDieTransferBlow1", out _blow[0]))
            {
                Log.Write("InputDieTransfer", "BindIoDomains", "Vacuums not found: InputDieTransferBlow1");
            }

            if (!IoAutoBindings.Vacuums.TryGetValue("InputDieTransferBlow2", out _blow[1]))
            {
                Log.Write("InputDieTransfer", "BindIoDomains", "Vacuums not found: InputDieTransferBlow2");
            }

            if (!IoAutoBindings.Vacuums.TryGetValue("InputDieTransferBlow3", out _blow[2]))
            {
                Log.Write("InputDieTransfer", "BindIoDomains", "Vacuums not found: InputDieTransferBlow3");
            }

            if (!IoAutoBindings.Vacuums.TryGetValue("InputDieTransferBlow4", out _blow[3]))
            {
                Log.Write("InputDieTransfer", "BindIoDomains", "Vacuums not found: InputDieTransferBlow4");
            }

            if (!IoAutoBindings.Vacuums.TryGetValue("InputDieTransferVent1", out _vent[0]))
            {
                Log.Write("InputDieTransfer", "BindIoDomains", "Vacuums not found: InputDieTransferVent1");
            }

            if (!IoAutoBindings.Vacuums.TryGetValue("InputDieTransferVent2", out _vent[1]))
            {
                Log.Write("InputDieTransfer", "BindIoDomains", "Vacuums not found: InputDieTransferVent2");
            }

            if (!IoAutoBindings.Vacuums.TryGetValue("InputDieTransferVent3", out _vent[2]))
            {
                Log.Write("InputDieTransfer", "BindIoDomains", "Vacuums not found: InputDieTransferVent3");
            }

            if (!IoAutoBindings.Vacuums.TryGetValue("InputDieTransferVent4", out _vent[3]))
            {
                Log.Write("InputDieTransfer", "BindIoDomains", "Vacuums not found: InputDieTransferVent4");
            }
        }

        // === Domain Control (표준 구동) ===
        public bool SetVacuum(int nNo, bool on)
        {
            if (_vacuum[nNo] == null) return false;
            if (on) _vacuum[nNo].On();
            else _vacuum[nNo].Off();
            return true;
        }
        public bool SetBlow(int nNo, bool on)
        {
            if (_blow[nNo] == null) return false;
            if (on) _blow[nNo].On();
            else _blow[nNo].Off();
            return true;
        }
        public bool SetVent(int nNo, bool on)
        {
            if (_vent[nNo] == null) return false;
            if (on) _vent[nNo].On();
            else _vent[nNo].Off();
            return true;
        }
        public bool AirTankPressureOk() => ReadInput(InputDieTransferConfig.IO.AIR_TANK_PRESSURE);
        public bool VacTankPressureOk() => ReadInput(InputDieTransferConfig.IO.VAC_TANK_PRESSURE);
        public bool ArmFlowOk(int armIndex)
        {
            switch (armIndex)
            {
                case 0: return ReadInput(InputDieTransferConfig.IO.ARM1_FLOW);
                case 1: return ReadInput(InputDieTransferConfig.IO.ARM2_FLOW);
                case 2: return ReadInput(InputDieTransferConfig.IO.ARM3_FLOW);
                case 3: return ReadInput(InputDieTransferConfig.IO.ARM4_FLOW);
            }
            return false;
        }
        /// //////////////////////////////////////////////////////////////////
        #endregion


        #region Lifecycle
        public override int OnRun() 
        {
            int ret = 0;

            if (this.Status == UnitRunStatus.Stop || 
                this.Status == UnitRunStatus.CycleStop)
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
            base.OnStop(); 
            return ret; 
        }

        protected override int OnRunReady()
        {
            int ret = 0;

            ChipPickUp();

            State = ProcessState.Work;
            return 0;
        }
        protected override int OnRunWork()
        {
            int ret = 0;

            State = ProcessState.Complete;
            return 0;
        }
        protected override int OnRunComplete()
        {
            int ret = 0;

            State = ProcessState.None;
            return 0;
        }

        #endregion

        #region Seq 단위 동작 함수
        public int ChipPickUp()
        {
            int nRet = -1;
            double dPosX = 0;
            double dPosY = 0;

            if(InputStage.IsStatus_CompleteWorking)
            {
                //1. InputStage Pick 위치 이동
                nRet = InputStage.MoveAxisWithSafety(AxisNames.WaferStageX, dPosX);
                nRet = InputStage.MoveAxisWithSafety(AxisNames.WaferStageY, dPosY);
                if (nRet != 0)
                {
                    return -1;
                }

                nRet = MovePickUpPosition();
                if (nRet != 0)
                {
                    return -1;
                }
            }

            //2. Arm Down
            //3. Vacuum On
            //4. Arm Up
            //5. Vacuum OK Check

            return nRet;
        }

        public int RotateArm()
        {
            int nRet = -1;
            /* TODO */
            return nRet;
        }

        public int ChipPlaceDown()
        {
            int nRet = -1;
            /* TODO */
            return nRet;
        }
        #endregion
    }
}