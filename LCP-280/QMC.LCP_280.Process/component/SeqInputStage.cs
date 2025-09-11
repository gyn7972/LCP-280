using System;
using System.Linq;
using QMC.Common;
using QMC.Common.Sequence;
using QMC.Common.Alarm;
using QMC.LCP_280.Process.Unit;
using System.Threading;

namespace QMC.LCP_280.Process.Component
{
    /// <summary>
    /// InputStage 전체 동작(로딩→클램프→파일리딩→얼라인→스캔→워킹→언로딩)을 관리하는 상위 시퀀스.
    /// - 복잡한 세부 비전/스캔 로직은 하위 시퀀스(SeqInputChipAlignVision / SeqWaferLocalScan)에 위임.
    /// - 축/IO/조명 등은 모두 InputStage 객체를 통해 수행.
    /// - V1: 기본 플로우 / 타임아웃 / Alarm 최소화. 필요 시 단계 세분화 가능.
    /// </summary>
    internal class SeqInputStage : SequenceBase
    {
        #region Step Definition
        public enum Step
        {
            Idle = 0,
            // 준비
            InitCheck,
            // 로딩 준비 (스테이지 로딩 위치 이동 후 외부 로딩 허가 대기)
            MoveLoadingPos, MoveLoadingPos_Wait,
            WaitExternalLoadingReady, // 외부(Transfer 등)에서 링 투입 완료 신호 가정 (DryRun 즉시 통과)
            ClampSequence, ClampSequence_Wait,
            FileReading, FileReading_Wait,
            Align_Start, Align_Wait,
            Scan_Start, Scan_Wait,
            WorkingPrep, // 예: 첫 Pick 준비 (단순 구현)
            WorkingReady, // Pick/Place 상위 공정이 소유 (본 시퀀스는 유지)
            Unload_Start, Unload_MoveLoadingPos, Unload_Unclamp, Unload_WaitRingOut,
            Finish,
            Error
        }
        #endregion

        #region Alarm Definition
        private enum AlarmKey
        {
            First = 45000,
            AxisMoveTimeout,
            ClampTimeout,
            FileReadFail,
            AlignFail,
            ScanFail,
            UnloadTimeout,
            GenericError
        }
        private readonly System.Collections.Generic.Dictionary<int, AlarmInfo> _alarms = new System.Collections.Generic.Dictionary<int, AlarmInfo>();
        private bool _alarmsInitialized;
        private void InitAlarms()
        {
            if (_alarmsInitialized) return; _alarmsInitialized = true;
            AddAlarm(AlarmKey.AxisMoveTimeout, "Axis Move Timeout", "축 이동 타임아웃", "Error");
            AddAlarm(AlarmKey.ClampTimeout, "Clamp Timeout", "클램프 구동 타임아웃", "Error");
            AddAlarm(AlarmKey.FileReadFail, "File Read Fail", "맵 파일 로딩 실패", "Error");
            AddAlarm(AlarmKey.AlignFail, "Align Fail", "얼라인 실패", "Error");
            AddAlarm(AlarmKey.ScanFail, "Scan Fail", "스캔 실패", "Error");
            AddAlarm(AlarmKey.UnloadTimeout, "Unload Timeout", "언로딩 타임아웃", "Error");
            AddAlarm(AlarmKey.GenericError, "Generic Error", "일반 오류", "Error");
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
                if (AlarmManager.Instance.Alarms.Any(a => a.Code == info.Code)) return; // 중복 방지
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

        #region Fields / Dependency
        private readonly InputStage _stage;
        private readonly InputStageEjector _ejectorStage;
        private readonly SeqInputChipAlignVision _seqAlign;
        private readonly SeqWaferLocalScan _seqScan;
        private Step _step = Step.Idle;
        private Step _prevLoggedStep = Step.Idle;
        private DateTime _tick; // 타임아웃 기준

        // 상태 플래그 (상위 공정과의 신호 상정 – 실제 연동 시 외부에서 Set)
        public bool ExternalLoadingDone { get; set; } // 링이 올려졌다고 외부에서 셋
        public bool ExternalRingRemoved { get; set; } // 언로딩 시 링 제거 완료

        // 결과 플래그
        public bool ClampDone { get; private set; }
        public bool FileReadDone { get; private set; }
        public bool AlignDone => _seqAlignCurrentResult == SeqResult.Success;
        public bool ScanDone => _seqScanCurrentResult == SeqResult.Success;

        // 하위 시퀀스 결과 캐시
        private SeqResult _seqAlignCurrentResult = SeqResult.None;
        private SeqResult _seqScanCurrentResult = SeqResult.None;

        private enum SeqResult { None, Success, Fail }

        // 파라미터
        public int AxisMoveTimeoutMs { get; set; } = 12000;
        public int ClampTimeoutMs { get; set; } = 4000;
        public int UnloadTimeoutMs { get; set; } = 10000;
        public int FileReadSimMs { get; set; } = 500; // (실제 구현 전 단순 지연)

        public Step CurrentStep => _step;
        private bool IsDryRun => _stage?.DryRun ?? false;
        #endregion

        //public SeqInputStage(InputStage stage, SeqInputChipAlignVision alignSeq, SeqWaferLocalScan scanSeq) : base("SeqInputStage")
        public SeqInputStage(InputStage stage, InputStageEjector inputStageEjector) : base("SeqInputStage")
        {
            _stage = stage ?? throw new ArgumentNullException(nameof(stage));
            _ejectorStage = inputStageEjector ?? throw new ArgumentNullException(nameof(inputStageEjector));

            // 하위 시퀀스가 아직 구현되지 않았거나 reflection 생성 실패 시 No-Op 시퀀스로 대체 (stage 필요)
            _seqAlign = new SeqInputChipAlignVision(stage);
            _seqScan = new SeqWaferLocalScan(stage);

            InitAlarms();
        }

        // Backward compatibility (older code instantiates with only InputStage)
        public SeqInputStage(InputStage stage) : this(stage, new InputStageEjector()) { }

        #region Manual / Single Step Support
        /// <summary>
        /// 단일 Step 이름 기반 실행 (ManualSequenceControl 에서 reflection 으로 호출).
        /// </summary>
        public bool StartSingle(string stepName)
        {
            if (string.IsNullOrWhiteSpace(stepName)) return false;
            if (IsRunning) return false;
            if (!Enum.TryParse(stepName, true, out Step v)) return false;
            _step = v;
            _prevLoggedStep = Step.Idle;
            _tick = DateTime.UtcNow;
            ResetFlags();
            return base.Start(0);
        }

        /// <summary>UI에서 Step 리스트 빠르게 얻기 위한 helper.</summary>
        public static string[] GetStepNames() => Enum.GetNames(typeof(Step));

        /// <summary>Auto-init (ManualSequenceControl) 용 factory.</summary>
        public static SeqInputStage CreateFromUnit(InputStage stage)
            => new SeqInputStage(stage);
        #endregion

        public bool Start(Step first = Step.InitCheck)
        {
            _step = first;
            _prevLoggedStep = Step.Idle;
            _tick = DateTime.UtcNow;
            ResetFlags();
            return base.Start(0);
        }

        private void ResetFlags()
        {
            ClampDone = false; FileReadDone = false; ExternalLoadingDone = false; ExternalRingRemoved = false;
            _seqAlignCurrentResult = SeqResult.None; _seqScanCurrentResult = SeqResult.None;
        }

        private bool Timeout(int ms) => (DateTime.UtcNow - _tick).TotalMilliseconds >= ms;
        private string StepCode(Step s) => ((int)s) + ":" + s;

        private void GoError(string msg, AlarmKey key)
        {
            try { Log.Write(Name, "Error", Name, msg); } catch { }
            PostAlarm(key, $"Step={_step} Msg={msg}");
            _step = Step.Error;
        }

        #region Sub Sequence Poll
        private void StartAlignSub()
        {
            //if (_seqAlign.IsRunning) return;
            //_seqAlignCurrentResult = SeqResult.None;
            //_seqAlign.Start(SeqInputChipAlignVision.Step.Init);
        }
        private void PollAlignSub()
        {
            //if (_seqAlign.IsRunning) return;
            //if (_seqAlign.State == SequenceState.Completed)
            //    _seqAlignCurrentResult = SeqResult.Success;
            //else if (_seqAlign.State == SequenceState.Error || _seqAlign.CurrentStep == SeqInputChipAlignVision.Step.Error)
            //    _seqAlignCurrentResult = SeqResult.Fail;
        }

        private void StartScanSub()
        {
            if (_seqScan.IsRunning) return;
            _seqScanCurrentResult = SeqResult.None;
            _seqScan.Start(SeqWaferLocalScan.Step.InitSummary);
        }
        private void PollScanSub()
        {
            if (_seqScan.IsRunning) return;
            if (_seqScan.State == SequenceState.Completed)
                _seqScanCurrentResult = SeqResult.Success;
            else if (_seqScan.State == SequenceState.Error || _seqScan.CurrentStep == SeqWaferLocalScan.Step.Error)
                _seqScanCurrentResult = SeqResult.Fail;
        }
        #endregion

        #region ExecuteStep
        protected override int ExecuteStep(int current, System.Threading.CancellationToken ct)
        {
            var before = _step;
            switch (_step)
            {
                case Step.Idle: return -1;

                case Step.InitCheck:
                    if (_stage.AxisX == null || _stage.AxisY == null || _stage.AxisT == null)
                    { GoError("Axis null", AlarmKey.GenericError); break; }
                    _step = Step.MoveLoadingPos; _tick = DateTime.UtcNow; break;

                case Step.MoveLoadingPos:
                    _stage.MoveToTeachingPosition(InputStageConfig.TeachingPositionName.Loading);
                    _step = Step.MoveLoadingPos_Wait; _tick = DateTime.UtcNow; break;
                case Step.MoveLoadingPos_Wait:
                    if (_stage.InPosTeaching(InputStageConfig.TeachingPositionName.Loading))
                    { _step = Step.WaitExternalLoadingReady; _tick = DateTime.UtcNow; }
                    else if (Timeout(AxisMoveTimeoutMs)) GoError("LoadingPos move timeout", AlarmKey.AxisMoveTimeout);
                    else Thread.Sleep(5); // prevent tight loop
                    break;

                case Step.WaitExternalLoadingReady:
                    if (IsDryRun || ExternalLoadingDone)
                    { 
                        _step = Step.ClampSequence; 
                        _tick = DateTime.UtcNow; 
                    }
                    else Thread.Sleep(10); // waiting external signal
                    break;

                case Step.ClampSequence:
                    if (IsDryRun)
                    { ClampDone = true; _step = Step.FileReading; _tick = DateTime.UtcNow; break; }
                    _stage.SetClampLiftUpValve(true);
                    _step = Step.ClampSequence_Wait; _tick = DateTime.UtcNow; break;
                case Step.ClampSequence_Wait:
                    if (_stage.IsClampLiftUp()) { ClampDone = true; _step = Step.FileReading; _tick = DateTime.UtcNow; }
                    else if (Timeout(ClampTimeoutMs)) GoError("Clamp timeout", AlarmKey.ClampTimeout);
                    else Thread.Sleep(5);
                    break;

                case Step.FileReading:
                    if (IsDryRun) { FileReadDone = true; _step = Step.Align_Start; _tick = DateTime.UtcNow; break; }
                    // (실제 파일 리딩 로직 자리). 현재는 지연 후 통과.
                    if (Timeout(FileReadSimMs)) { FileReadDone = true; _step = Step.Align_Start; _tick = DateTime.UtcNow; }
                    else Thread.Sleep(5);
                    break;

                case Step.Align_Start:
                    StartAlignSub();
                    _step = Step.Align_Wait; _tick = DateTime.UtcNow; break;
                case Step.Align_Wait:
                    PollAlignSub();
                    if (_seqAlignCurrentResult == SeqResult.Success) { _step = Step.Scan_Start; _tick = DateTime.UtcNow; }
                    else if (_seqAlignCurrentResult == SeqResult.Fail) GoError("Align sub fail", AlarmKey.AlignFail);
                    else Thread.Sleep(5);
                    break;

                case Step.Scan_Start:
                    StartScanSub();
                    _step = Step.Scan_Wait;
                    _tick = DateTime.UtcNow; 
                    break;

                case Step.Scan_Wait:
                    PollScanSub();
                    if (_seqScanCurrentResult == SeqResult.Success) { _step = Step.WorkingPrep; _tick = DateTime.UtcNow; }
                    else if (_seqScanCurrentResult == SeqResult.Fail) GoError("Scan sub fail", AlarmKey.ScanFail);
                    else Thread.Sleep(5);
                    break;

                case Step.WorkingPrep:
                    // 간단: 얼라인 위치에서 Ready 이동
                    _stage.MoveToTeachingPosition(InputStageConfig.TeachingPositionName.CenterPoint);
                    _step = Step.WorkingReady; _tick = DateTime.UtcNow; break;
                case Step.WorkingReady:
                    _step = Step.Unload_Start; _tick = DateTime.UtcNow; break;

                case Step.Unload_Start:
                    _stage.MoveToTeachingPosition(InputStageConfig.TeachingPositionName.Unloading);
                    _step = Step.Unload_MoveLoadingPos; _tick = DateTime.UtcNow; break;
                case Step.Unload_MoveLoadingPos:
                    if (_stage.InPosTeaching(InputStageConfig.TeachingPositionName.Unloading))
                    { _stage.SetClampLiftUpValve(false); _step = Step.Unload_Unclamp; _tick = DateTime.UtcNow; }
                    else if (Timeout(AxisMoveTimeoutMs)) GoError("Unload move timeout", AlarmKey.UnloadTimeout);
                    else Thread.Sleep(5);
                    break;
                case Step.Unload_Unclamp:
                    if (IsDryRun || !_stage.IsClampLiftUp()) { _step = Step.Unload_WaitRingOut; _tick = DateTime.UtcNow; }
                    else if (Timeout(ClampTimeoutMs)) GoError("Unclamp timeout", AlarmKey.UnloadTimeout);
                    else Thread.Sleep(5);
                    break;
                case Step.Unload_WaitRingOut:
                    if (IsDryRun || ExternalRingRemoved) { _step = Step.Finish; _tick = DateTime.UtcNow; }
                    else if (Timeout(UnloadTimeoutMs)) GoError("Ring remove timeout", AlarmKey.UnloadTimeout);
                    else Thread.Sleep(10);
                    break;

                case Step.Finish:
                    return -1;
                case Step.Error:
                    return -1;
            }

            if (before != _step && _prevLoggedStep != _step)
            {
                try { Log.Write(Name, "Step", Name, $"StepChange {StepCode(before)} -> {StepCode(_step)}"); } catch { }
                _prevLoggedStep = _step;
            }
            // 핵심 수정: runaway 증가 방지. 현재 논리 step(enum)을 그대로 반환.
            return (int)_step;
        }
        #endregion
    }
}
