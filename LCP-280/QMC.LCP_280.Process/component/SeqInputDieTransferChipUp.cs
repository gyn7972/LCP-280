using System;
using System.Linq;
using QMC.Common;
using QMC.Common.Sequence;
using QMC.Common.Alarm;
using QMC.LCP_280.Process.Unit;

namespace QMC.LCP_280.Process.Component
{
    /// <summary>
    /// Input Die Transfer Chip Up 시퀀스
    /// Enum 순서에 맞추어 재작성
    /// 공통: Vent Off → Vent Delay → Vacuum On → Vacuum On Delay → Vacuum 안정 대기 →
    /// Wafer Stage Ready 대기 → DoubleChip 검사 → 니들 타입 선택 → 니들 동작(Type1/2/3/3-2Step)
    /// 간소화된 Double Chip / Stage Ready 검사 (필요 시 실제 센서 연동 코드 교체)
    /// </summary>
    internal class SeqInputDieTransferChipUp : SequenceBase
    {
        #region Step Definition
        public enum Step
        {
            Idle = 0,
            // 공통 준비
            InitVentCheck,
            VentOffDelayWait,
            VacuumOnCheck,
            VacuumOnDelay,
            VacuumDelayWait,
            WaitWaferStageReady,
            DoubleChipCheck,
            SelectNeedleSeq,

            // Type 1
            Type1_Start = 100,
            Type1_MoveDown,
            Type1_WaitReady,
            Type1_NeedleUpAssign,
            Type1_Complete,

            // Type 2
            Type2_MoveNeedleUp = 200,
            Type2_MoveDown,
            Type2_WaitReady,
            Type2_MoveUp,
            Type2_AssignWafer,
            Type2_Complete,

            // Type 3
            Type3_Start = 300,
            Type3_MoveDown,
            Type3_WaitReady,
            Type3_NeedleUpAssign,
            Type3_MoveUp,
            Type3_Complete,

            // Type 3 Two Step
            Type3_2Step_Start = 400,
            Type3_2Step_MoveDown,
            Type3_2Step_WaitReady,
            Type3_2Step_NeedleUp1,
            Type3_2Step_NeedleUp2,
            Type3_2Step_MoveUp,
            Type3_2Step_Complete,

            Finish,
            Error
        }
        #endregion

        #region Needle Type Selection
        public enum NeedleType
        {
            Type1,
            Type2,
            Type3,
            Type3_TwoStep
        }
        public NeedleType SelectedNeedleType { get; set; } = NeedleType.Type1;
        #endregion

        #region Alarm
        private enum AlarmKey
        {
            First = 47000,
            AxisMoveTimeout,
            VacuumTimeout,
            PositionInvalid,
            GenericError
        }
        private readonly System.Collections.Generic.Dictionary<int, AlarmInfo> _alarms = new System.Collections.Generic.Dictionary<int, AlarmInfo>();
        private bool _alarmsInitialized;
        private void InitAlarms()
        {
            if (_alarmsInitialized) return; _alarmsInitialized = true;
            AddAlarm(AlarmKey.AxisMoveTimeout, "PickZ Move Timeout", "Pick Z 축 이동 타임아웃", "Error");
            AddAlarm(AlarmKey.VacuumTimeout, "Vacuum Timeout", "진공 형성 타임아웃", "Error");
            AddAlarm(AlarmKey.PositionInvalid, "Position Invalid", "티칭 위치 혹은 목표 위치 오류", "Error");
            AddAlarm(AlarmKey.GenericError, "Seq Generic Error", "일반 오류", "Error");
        }
        private void AddAlarm(AlarmKey key, string title, string cause, string grade)
        {
            _alarms[(int)key] = new AlarmInfo
            {
                Code = (int)key,
                Title = title,
                Cause = cause,
                Source = Name,
                Grade = grade,
                GeneratedTime = DateTime.Now
            };
        }
        private void PostAlarm(AlarmKey key, string detail = null)
        {
            try
            {
                if (!_alarmsInitialized) InitAlarms();
                if (!_alarms.TryGetValue((int)key, out var info)) return;
                if (AlarmManager.Instance.Alarms.Any(a => a.Code == info.Code)) return; // prevent duplicates
                var clone = new AlarmInfo
                {
                    Code = info.Code,
                    Title = info.Title,
                    Cause = string.IsNullOrEmpty(detail) ? info.Cause : info.Cause + " | " + detail,
                    Source = info.Source,
                    Grade = info.Grade,
                    GeneratedTime = DateTime.Now
                };
                AlarmManager.Instance.ShowAlarm(clone);
                Log.Write(Name, "Alarm", Name, $"AlarmPost Code={clone.Code} Title={clone.Title} Cause={clone.Cause}");
            }
            catch (Exception ex) { Log.Write(Name, "AlarmError", Name, ex.Message); }
        }
        #endregion

        #region Fields
        private readonly InputDieTransfer _transfer;
        private readonly InputStage _stage;              // optional, wafer stage ready check
        private readonly InputStageEjector _ejector;      // optional, 향후 필요시 사용

        private Step _step = Step.Idle;
        private Step _prevLoggedStep = Step.Idle;
        private DateTime _tick;

        // Motion targets
        private double _targetDownPos;
        private double _targetUpPos;
        private double _targetMidPos; // Two step 사용시 중간 위치

        // Action flags
        private bool _ventClosedIssued;
        private bool _vacOnIssued;
        private bool _vacDelayStarted;
        private bool _downMoveIssued;
        private bool _upMoveIssued;
        private bool _midMoveIssued;

        private int _armIndex;

        // Parameters (ms / mm)
        public int VentCloseDelayMs { get; set; } = 80;
        public int VacuumOnSensorWaitMs { get; set; } = 300;    // VacuumOnDelay 단계 센서/조건 확인 대기 (placeholder)
        public int VacuumDelayMs { get; set; } = 120;           // Vacuum 안정 대기(VacuumDelayWait)
        public int DownStabilizeDelayMs { get; set; } = 50;
        public int MidStabilizeDelayMs { get; set; } = 40;
        public int MoveTimeoutMs { get; set; } = 6000;
        public int VacuumTimeoutMs { get; set; } = 1500;

        // Teaching position names
        public string UpTeachingName { get; set; } = "Ready";  // 안전 위치
        public string DownTeachingName { get; set; } = null;    // 없으면 Up - 2mm
        public string MidTeachingName { get; set; } = null;     // TwoStep 중간 위치 (없으면 Up과 Down 사이 계산)

        // Direct override
        public double? UpPositionOverride { get; set; }
        public double? DownPositionOverride { get; set; }
        public double? MidPositionOverride { get; set; }

        // Result flags
        public bool PickCompleted { get; private set; }
        public bool DoubleChipDetected { get; private set; }   // placeholder

        private bool IsDryRun => _transfer?.DryRun ?? false;
        public Step CurrentStep => _step;
        #endregion

        public SeqInputDieTransferChipUp(InputDieTransfer transfer, int armIndex = 0, InputStage stage = null, InputStageEjector ejector = null) : base("SeqInputDieTransferChipUp")
        {
            _transfer = transfer ?? throw new ArgumentNullException(nameof(transfer));
            _stage = stage; _ejector = ejector;
            _armIndex = armIndex;
            InitAlarms();
        }

        public void SetArmIndex(int idx) { if (idx >= 0) _armIndex = idx; }

        public bool Start(Step first = Step.InitVentCheck)
        {
            _step = first;
            _prevLoggedStep = Step.Idle;
            _tick = DateTime.UtcNow;
            PickCompleted = false;
            DoubleChipDetected = false;
            _ventClosedIssued = false; _vacOnIssued = false; _vacDelayStarted = false;
            _downMoveIssued = false; _upMoveIssued = false; _midMoveIssued = false;
            return base.Start(0);
        }

        private bool Timeout(int ms) => (DateTime.UtcNow - _tick).TotalMilliseconds >= ms;
        private string StepCode(Step s) => ((int)s) + ":" + s;

        private void GoError(string msg, AlarmKey key)
        {
            try { Log.Write(Name, "Error", Name, msg); } catch { }
            PostAlarm(key, $"Step={_step} Msg={msg}");
            _step = Step.Error;
        }

        private bool ResolvePositions()
        {
            try
            {
                var pickZ = _transfer.PickZ;
                if (pickZ == null) { GoError("PickZ axis null", AlarmKey.GenericError); return false; }

                // Up
                if (UpPositionOverride.HasValue) _targetUpPos = UpPositionOverride.Value;
                else if (!string.IsNullOrEmpty(UpTeachingName)) _targetUpPos = _transfer.GetTP(UpTeachingName, pickZ.Setup.Name);
                else { GoError("Up position undefined", AlarmKey.PositionInvalid); return false; }

                // Down
                if (DownPositionOverride.HasValue) _targetDownPos = DownPositionOverride.Value;
                else if (!string.IsNullOrEmpty(DownTeachingName)) _targetDownPos = _transfer.GetTP(DownTeachingName, pickZ.Setup.Name);
                else _targetDownPos = _targetUpPos - 2.0; // fallback

                // Mid (TwoStep)
                if (MidPositionOverride.HasValue) _targetMidPos = MidPositionOverride.Value;
                else if (!string.IsNullOrEmpty(MidTeachingName)) _targetMidPos = _transfer.GetTP(MidTeachingName, pickZ.Setup.Name);
                else _targetMidPos = (_targetUpPos + _targetDownPos) / 2.0;

                return true;
            }
            catch (Exception ex)
            {
                GoError("ResolvePositions ex=" + ex.Message, AlarmKey.PositionInvalid);
                return false;
            }
        }

        private bool StageReady()
        {
            if (_stage == null) return true; // 없으면 통과
            // 간단 조건: 링 존재 & 클램프 업(가정) → 실제 조건 필요시 수정
            try { return _stage.IsRingPresent(); } catch { return true; }
        }

        private bool CheckDoubleChip()
        {
            // TODO: 실제 더블칩 센서 혹은 비전 검사 연동
            DoubleChipDetected = false; // 기본 false
            return !DoubleChipDetected;
        }

        private void CommandPickZ(double target)
        {
            var ax = _transfer.PickZ; if (ax == null) return;
            if (Math.Abs(ax.GetPosition() - target) <= ax.Config.InposTolerance * 3) return;
            ax.MoveAbs(target, ax.Config.MaxVelocity, ax.Config.RunAcc, ax.Config.RunDec, ax.Config.AccJerkPercent);
        }

        private bool InPos(double target)
        {
            var ax = _transfer.PickZ; if (ax == null) return true;
            return ax.InPosition(target);
        }

        protected override int ExecuteStep(int current, System.Threading.CancellationToken ct)
        {
            var before = _step;
            switch (_step)
            {
                case Step.Idle: return -1;

                // 1) Vent Off → Delay
                case Step.InitVentCheck:
                    if (!ResolvePositions()) break;
                    if (!_ventClosedIssued)
                    {
                        _transfer.SetVent(_armIndex, false); // Vent Close
                        _ventClosedIssued = true;
                    }
                    _tick = DateTime.UtcNow; _step = Step.VentOffDelayWait; break;

                case Step.VentOffDelayWait:
                    if (IsDryRun || Timeout(VentCloseDelayMs)) { _step = Step.VacuumOnCheck; _tick = DateTime.UtcNow; }
                    break;

                // 2) Vacuum On → On Delay → 안정 딜레이
                case Step.VacuumOnCheck:
                    if (!_vacOnIssued)
                    {
                        _transfer.SetVacuum(_armIndex, true);
                        _vacOnIssued = true;
                        _tick = DateTime.UtcNow;
                    }
                    _step = Step.VacuumOnDelay; break;

                case Step.VacuumOnDelay:
                    // 센서 확인 혹은 최소 대기
                    if (IsDryRun || Timeout(VacuumOnSensorWaitMs)) { _step = Step.VacuumDelayWait; _tick = DateTime.UtcNow; }
                    else if (Timeout(VacuumTimeoutMs)) GoError("Vacuum timeout", AlarmKey.VacuumTimeout);
                    break;

                case Step.VacuumDelayWait:
                    if (IsDryRun || Timeout(VacuumDelayMs)) { _step = Step.WaitWaferStageReady; _tick = DateTime.UtcNow; }
                    break;

                // 3) Stage Ready 대기
                case Step.WaitWaferStageReady:
                    if (StageReady()) { _step = Step.DoubleChipCheck; _tick = DateTime.UtcNow; }
                    else if (Timeout(3000)) // 임시 타임아웃
                    { GoError("Stage not ready timeout", AlarmKey.GenericError); }
                    break;

                // 4) 더블칩 검사
                case Step.DoubleChipCheck:
                    if (!CheckDoubleChip()) { GoError("Double chip detected", AlarmKey.GenericError); break; }
                    _step = Step.SelectNeedleSeq; _tick = DateTime.UtcNow; break;

                // 5) 니들 시퀀스 선택
                case Step.SelectNeedleSeq:
                    switch (SelectedNeedleType)
                    {
                        default:
                        case NeedleType.Type1: _step = Step.Type1_Start; break;
                        case NeedleType.Type2: _step = Step.Type2_MoveNeedleUp; break;
                        case NeedleType.Type3: _step = Step.Type3_Start; break;
                        case NeedleType.Type3_TwoStep: _step = Step.Type3_2Step_Start; break;
                    }
                    _tick = DateTime.UtcNow; break;

                #region Type1
                case Step.Type1_Start:
                    _downMoveIssued = _upMoveIssued = false;
                    _step = Step.Type1_MoveDown; _tick = DateTime.UtcNow; break;

                case Step.Type1_MoveDown:
                    if (!_downMoveIssued)
                    { CommandPickZ(_targetDownPos); _downMoveIssued = true; _tick = DateTime.UtcNow; }
                    if (IsDryRun || InPos(_targetDownPos)) { _step = Step.Type1_WaitReady; _tick = DateTime.UtcNow; }
                    else if (Timeout(MoveTimeoutMs)) GoError("Type1 down move timeout", AlarmKey.AxisMoveTimeout);
                    break;

                case Step.Type1_WaitReady:
                    if (IsDryRun || Timeout(DownStabilizeDelayMs)) { _step = Step.Type1_NeedleUpAssign; _tick = DateTime.UtcNow; }
                    break;

                case Step.Type1_NeedleUpAssign:
                    if (!_upMoveIssued)
                    { CommandPickZ(_targetUpPos); _upMoveIssued = true; _tick = DateTime.UtcNow; }
                    if (IsDryRun || InPos(_targetUpPos)) { _step = Step.Type1_Complete; _tick = DateTime.UtcNow; }
                    else if (Timeout(MoveTimeoutMs)) GoError("Type1 up move timeout", AlarmKey.AxisMoveTimeout);
                    break;

                case Step.Type1_Complete:
                    PickCompleted = true; _step = Step.Finish; break;
                #endregion

                #region Type2
                case Step.Type2_MoveNeedleUp:
                    if (!_upMoveIssued)
                    { CommandPickZ(_targetUpPos); _upMoveIssued = true; _tick = DateTime.UtcNow; }
                    if (IsDryRun || InPos(_targetUpPos)) { _step = Step.Type2_MoveDown; _downMoveIssued = false; _tick = DateTime.UtcNow; }
                    else if (Timeout(MoveTimeoutMs)) GoError("Type2 pre-up move timeout", AlarmKey.AxisMoveTimeout);
                    break;

                case Step.Type2_MoveDown:
                    if (!_downMoveIssued)
                    { CommandPickZ(_targetDownPos); _downMoveIssued = true; _tick = DateTime.UtcNow; }
                    if (IsDryRun || InPos(_targetDownPos)) { _step = Step.Type2_WaitReady; _tick = DateTime.UtcNow; }
                    else if (Timeout(MoveTimeoutMs)) GoError("Type2 down move timeout", AlarmKey.AxisMoveTimeout);
                    break;

                case Step.Type2_WaitReady:
                    if (IsDryRun || Timeout(DownStabilizeDelayMs)) { _step = Step.Type2_MoveUp; _upMoveIssued = false; _tick = DateTime.UtcNow; }
                    break;

                case Step.Type2_MoveUp:
                    if (!_upMoveIssued)
                    { CommandPickZ(_targetUpPos); _upMoveIssued = true; _tick = DateTime.UtcNow; }
                    if (IsDryRun || InPos(_targetUpPos)) { _step = Step.Type2_AssignWafer; _tick = DateTime.UtcNow; }
                    else if (Timeout(MoveTimeoutMs)) GoError("Type2 up move timeout", AlarmKey.AxisMoveTimeout);
                    break;

                case Step.Type2_AssignWafer:
                    // 실제 Wafer 할당/상태 갱신 필요 시 여기서 처리
                    _step = Step.Type2_Complete; _tick = DateTime.UtcNow; break;

                case Step.Type2_Complete:
                    PickCompleted = true; _step = Step.Finish; break;
                #endregion

                #region Type3 (Single)
                case Step.Type3_Start:
                    _downMoveIssued = _upMoveIssued = false;
                    _step = Step.Type3_MoveDown; _tick = DateTime.UtcNow; break;

                case Step.Type3_MoveDown:
                    if (!_downMoveIssued) { CommandPickZ(_targetDownPos); _downMoveIssued = true; _tick = DateTime.UtcNow; }
                    if (IsDryRun || InPos(_targetDownPos)) { _step = Step.Type3_WaitReady; _tick = DateTime.UtcNow; }
                    else if (Timeout(MoveTimeoutMs)) GoError("Type3 down move timeout", AlarmKey.AxisMoveTimeout);
                    break;

                case Step.Type3_WaitReady:
                    if (IsDryRun || Timeout(DownStabilizeDelayMs)) { _step = Step.Type3_NeedleUpAssign; _tick = DateTime.UtcNow; }
                    break;

                case Step.Type3_NeedleUpAssign:
                    if (!_upMoveIssued) { CommandPickZ(_targetUpPos); _upMoveIssued = true; _tick = DateTime.UtcNow; }
                    if (IsDryRun || InPos(_targetUpPos)) { _step = Step.Type3_MoveUp; _tick = DateTime.UtcNow; }
                    else if (Timeout(MoveTimeoutMs)) GoError("Type3 up move timeout", AlarmKey.AxisMoveTimeout);
                    break;

                case Step.Type3_MoveUp:
                    // Up 위치 유지 확인 후 완료
                    if (IsDryRun || InPos(_targetUpPos)) { _step = Step.Type3_Complete; _tick = DateTime.UtcNow; }
                    else if (Timeout(MoveTimeoutMs)) GoError("Type3 final up timeout", AlarmKey.AxisMoveTimeout);
                    break;

                case Step.Type3_Complete:
                    PickCompleted = true; _step = Step.Finish; break;
                #endregion

                #region Type3 Two Step
                case Step.Type3_2Step_Start:
                    _midMoveIssued = _downMoveIssued = _upMoveIssued = false;
                    _step = Step.Type3_2Step_MoveDown; _tick = DateTime.UtcNow; break;

                case Step.Type3_2Step_MoveDown:
                    if (!_downMoveIssued) { CommandPickZ(_targetDownPos); _downMoveIssued = true; _tick = DateTime.UtcNow; }
                    if (IsDryRun || InPos(_targetDownPos)) { _step = Step.Type3_2Step_WaitReady; _tick = DateTime.UtcNow; }
                    else if (Timeout(MoveTimeoutMs)) GoError("Type3_2Step down timeout", AlarmKey.AxisMoveTimeout);
                    break;

                case Step.Type3_2Step_WaitReady:
                    if (IsDryRun || Timeout(DownStabilizeDelayMs)) { _step = Step.Type3_2Step_NeedleUp1; _tick = DateTime.UtcNow; }
                    break;

                case Step.Type3_2Step_NeedleUp1:
                    if (!_midMoveIssued) { CommandPickZ(_targetMidPos); _midMoveIssued = true; _tick = DateTime.UtcNow; }
                    if (IsDryRun || InPos(_targetMidPos)) { _step = Step.Type3_2Step_NeedleUp2; _tick = DateTime.UtcNow; }
                    else if (Timeout(MoveTimeoutMs)) GoError("Type3_2Step mid move timeout", AlarmKey.AxisMoveTimeout);
                    break;

                case Step.Type3_2Step_NeedleUp2:
                    if (!_upMoveIssued) { CommandPickZ(_targetUpPos); _upMoveIssued = true; _tick = DateTime.UtcNow; }
                    if (IsDryRun || InPos(_targetUpPos)) { _step = Step.Type3_2Step_MoveUp; _tick = DateTime.UtcNow; }
                    else if (Timeout(MoveTimeoutMs)) GoError("Type3_2Step up move timeout", AlarmKey.AxisMoveTimeout);
                    break;

                case Step.Type3_2Step_MoveUp:
                    if (IsDryRun || Timeout(MidStabilizeDelayMs)) { _step = Step.Type3_2Step_Complete; _tick = DateTime.UtcNow; }
                    break;

                case Step.Type3_2Step_Complete:
                    PickCompleted = true; _step = Step.Finish; break;
                #endregion

                case Step.Finish: return -1;
                case Step.Error: return -1;
            }

            if (before != _step && _prevLoggedStep != _step)
            {
                try { Log.Write(Name, "Step", Name, $"StepChange {StepCode(before)} -> {StepCode(_step)} Arm={_armIndex}"); } catch { }
                _prevLoggedStep = _step;
            }
            return current + 1; // 내부 상태 기반 반복
        }
    }
}
