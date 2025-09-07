using QMC.Common;
using QMC.Common.Sequence;
using QMC.LCP_280.Process.Unit;
using System;
using System.Collections.Generic;
using System.Linq;
using QMC.Common.Alarm; // added for AlarmInfo, AlarmManager

namespace QMC.LCP_280.Process.Component
{
    internal class SeqInputChipAlignVision : SequenceBase
    {
        // === Pause / SoftStop 지원 필드 ===
        private volatile bool _pauseRequested;     // 일시정지 플래그
        private volatile bool _softStopRequested;  // 소프트 정지 플래그 (현 스텝 끝나면 Idle)
        public bool IsPaused => _pauseRequested;

        /// <summary>일시정지 요청 (TimeOut 진행 정지 목적). ExecuteStep 상단에서 step 처리 중단.</summary>
        public void Pause()
        {
            _pauseRequested = true;
        }
        /// <summary>일시정지 해제. 타임아웃 기준 tick 재설정.</summary>
        public void Resume()
        {
            if (_pauseRequested)
            {
                _pauseRequested = false;
                _tick = DateTime.UtcNow; // 재개 시 타임아웃 누적 방지
            }
        }
        /// <summary>소프트정지 요청: 현재 step 처리 후 Idle 전환.</summary>
        public void SoftStop()
        {
            _softStopRequested = true;
        }

        public enum Step
        {
            Idle = 0,
            Init,
            VacuumOn, VacuumOn_Check,
            MoveCenter, MoveCenter_Check,
            LightingGrab1,
            SearchMulti,
            MoveTAxis, MoveTAxis_Check,
            LightingGrab2,
            SearchCenter,
            MoveXYAxis, MoveXYAxis_Check,
            Align_Check,
            Finish,
            Error
        }

        // Sequence 전용 알람 키 (범위: 40000~)
        private enum AlarmKey
        {
            First = 40000,
            VacuumTimeout,
            CenterMoveTimeout,
            Grab1Timeout,
            MultiSearchFail,
            ThetaOverLimit,
            MoveTRotationTimeout,
            Grab2Timeout,
            CenterSearchFail,
            XYMoveTimeout,
            AlignInposTimeout,
            MissingPrecondition,
            GenericError
        }

        private readonly Dictionary<int, AlarmInfo> _alarms = new Dictionary<int, AlarmInfo>();
        private bool _alarmsInitialized;

        private Step _step = Step.Idle;
        private Step _prevLoggedStep = Step.Idle; // 이전에 로그 남긴 Step
        private readonly InputStage _stage;
        private DateTime _tick;

        // 외부 델리게이트 (필요 시 Stage에서 자동 바인딩)
        public Func<bool> EnsureCameraReadyFunc { get; set; }
        public Action SetLightingForMultiAction { get; set; }
        public Action SetLightingForCenterAction { get; set; }
        public Func<bool> GrabImageFunc { get; set; }
        public Func<(bool ok, List<double> thetaList)> FindMultiMarksFunc { get; set; }
        public Func<(bool ok, double x, double y)> FindCenterMarkFunc { get; set; }

        // 내부 상태 플래그
        private bool _camReady;
        private bool _vacCmd;
        private bool _grab1Done;
        private bool _grab2Done;
        private bool _multiSearched;
        private bool _centerSearched;
        private double _targetT;
        private double _targetX;
        private double _targetY;
        private double _calcTheta; // 계산된 보정각

        private const string CENTER_TP = "Home"; // 기준 티칭명

        // 타임아웃 / 파라미터
        public int VacuumTimeoutMs { get; set; } = 3000;
        public int MoveTimeoutMs { get; set; } = 10000;
        public int GrabTimeoutMs { get; set; } = 3000;
        public double ThetaShiftFilterExcludeCount { get; set; } = 1; // Trim 개수
        public double ThetaApplyLimitDeg { get; set; } = 2.0;          // 적용 허용 각

        public Step CurrentStep => _step;
        private bool IsDryRun => _stage?.DryRun ?? false;

        public SeqInputChipAlignVision(InputStage stage) : base("SeqInputChipAlignVision")
        {
            _stage = stage ?? throw new ArgumentNullException(nameof(stage));
            InitAlarms();
        }

        private void InitAlarms()
        {
            if (_alarmsInitialized) return;
            _alarmsInitialized = true;
            AddAlarm(AlarmKey.VacuumTimeout,       "VacuumOn Timeout",        "Vacuum 감지 타임아웃", "Error");
            AddAlarm(AlarmKey.CenterMoveTimeout,   "Center Move Timeout",     "센터 위치 이동 타임아웃", "Error");
            AddAlarm(AlarmKey.Grab1Timeout,        "Grab1 Timeout",           "첫 번째 이미지 취득 실패", "Error");
            AddAlarm(AlarmKey.MultiSearchFail,     "Multi Search Fail",       "다중 마크 검색 실패", "Error");
            AddAlarm(AlarmKey.ThetaOverLimit,      "Theta Over Limit",        "보정각 제한 초과", "Error");
            AddAlarm(AlarmKey.MoveTRotationTimeout,"T Axis Move Timeout",     "T 축 이동 타임아웃", "Error");
            AddAlarm(AlarmKey.Grab2Timeout,        "Grab2 Timeout",           "두 번째 이미지 취득 실패", "Error");
            AddAlarm(AlarmKey.CenterSearchFail,    "Center Search Fail",      "센터 마크 검색 실패", "Error");
            AddAlarm(AlarmKey.XYMoveTimeout,       "XY Move Timeout",         "XY 축 이동 타임아웃", "Error");
            AddAlarm(AlarmKey.AlignInposTimeout,   "Align InPos Timeout",     "얼라인 위치 도달 실패", "Error");
            AddAlarm(AlarmKey.MissingPrecondition, "Missing Precondition",    "선행 단계 누락", "Error");
            AddAlarm(AlarmKey.GenericError,        "Seq Generic Error",       "시퀀스 일반 오류", "Error");
        }

        private void AddAlarm(AlarmKey key, string title, string cause, string grade)
        {
            var alarm = new AlarmInfo
            {
                Code = (int)key,
                Title = title,
                Cause = cause,
                Source = Name,
                Grade = grade,
                GeneratedTime = DateTime.Now
            };
            _alarms[alarm.Code] = alarm;
        }

        private void PostAlarm(AlarmKey key, string detail = null)
        {
            try
            {
                if (!_alarmsInitialized) InitAlarms();
                int code = (int)key;
                if (!_alarms.TryGetValue(code, out var baseInfo)) return;
                if (AlarmManager.Instance.Alarms.Any(a => a.Code == code)) return; // 중복
                var clone = new AlarmInfo
                {
                    Code = baseInfo.Code,
                    Title = baseInfo.Title,
                    Cause = string.IsNullOrEmpty(detail) ? baseInfo.Cause : baseInfo.Cause + " | " + detail,
                    Source = baseInfo.Source,
                    Grade = baseInfo.Grade,
                    GeneratedTime = DateTime.Now
                };
                AlarmManager.Instance.ShowAlarm(clone);
                Log.Write("SeqInputChipAlignVision", "Alarm", Name, $"AlarmPost Code={clone.Code} Title={clone.Title} Cause={clone.Cause}");
            }
            catch (Exception ex)
            {
                Log.Write("SeqInputChipAlignVision", "AlarmError", Name, ex.Message);
            }
        }

        public bool Start(Step first = Step.Init)
        {
            BindStageDelegatesIfNeeded();
            _step = first;
            _prevLoggedStep = Step.Idle; // 로그 초기화
            _tick = DateTime.UtcNow;
            _pauseRequested = false; _softStopRequested = false; // 제어 플래그 초기화
            ResetFlags();
            return base.Start(0);
        }

        public bool StartSingle(string stepName)
        {
            if (string.IsNullOrWhiteSpace(stepName)) return false;
            if (IsRunning) return false;
            if (!Enum.TryParse(stepName, true, out Step v)) return false;
            _step = v; _prevLoggedStep = Step.Idle; _tick = DateTime.UtcNow;
            // 기본 플래그 초기화 (기존 Start 와 동일 수준)
            _pauseRequested = false; _softStopRequested = false;
            ResetFlags();
            BindStageDelegatesIfNeeded();
            return base.Start(0);
        }
        public static string[] GetStepNames() => Enum.GetNames(typeof(Step));

        private void BindStageDelegatesIfNeeded()
        {
            if (_stage == null) return;
            if (EnsureCameraReadyFunc == null) EnsureCameraReadyFunc = _stage.CamReadyFunc;
            if (SetLightingForMultiAction == null) SetLightingForMultiAction = _stage.SetLightingMultiAction;
            if (SetLightingForCenterAction == null) SetLightingForCenterAction = _stage.SetLightingCenterAction;
            if (GrabImageFunc == null) GrabImageFunc = _stage.GrabImageFunc;
            if (FindMultiMarksFunc == null) FindMultiMarksFunc = _stage.FindMultiMarksFunc;
            if (FindCenterMarkFunc == null) FindCenterMarkFunc = _stage.FindCenterMarkFunc;
        }

        private void ResetFlags()
        {
            _camReady = false;
            _vacCmd = false;
            _grab1Done = false;
            _grab2Done = false;
            _multiSearched = false;
            _centerSearched = false;
            _targetT = 0;
            _targetX = 0;
            _targetY = 0;
            _calcTheta = 0;
        }

        // Step -> 번호+이름 포맷 (로그용)
        private string StepToCode(Step s)
        {
            return ((int)s).ToString() + ":" + s;
        }

        protected override int ExecuteStep(int current, System.Threading.CancellationToken ct)
        {
            // === Pause 처리: step 로직 실행 중지 (상태 유지) ===
            if (_pauseRequested) return current; // 타임아웃 진행 정지 목적이면 Resume 시 _tick 재설정

            var beforeStep = _step; // 변경 감지용
            switch (_step)
            {
                case Step.Idle:
                    return -1;

                case Step.Init:
                    _camReady = IsDryRun ? true : (EnsureCameraReadyFunc?.Invoke() ?? true);
                    if (_camReady) { _step = Step.VacuumOn; _tick = DateTime.UtcNow; }
                    break;

                case Step.VacuumOn:
                    if (IsDryRun) { _step = Step.VacuumOn_Check; _tick = DateTime.UtcNow; break; }
                    if (!_vacCmd) { _stage.VacuumOn(); _vacCmd = true; _tick = DateTime.UtcNow; }
                    _step = Step.VacuumOn_Check; break;

                case Step.VacuumOn_Check:
                    if (IsDryRun || _stage.VacuumCheck()) { _step = Step.MoveCenter; _tick = DateTime.UtcNow; }
                    else if (Timeout(VacuumTimeoutMs)) GoError("VacuumOn timeout", AlarmKey.VacuumTimeout);
                    break;

                case Step.MoveCenter:
                    _stage.MoveToTeachingPosition(_stage.TeachingPositions[InputStageConfig.TeachingPositionName.Align]);
                    _step = Step.MoveCenter_Check; _tick = DateTime.UtcNow; break;

                case Step.MoveCenter_Check:
                    if (_stage.InPosTeaching(InputStageConfig.TeachingPositionName.Align)) { _step = Step.LightingGrab1; _tick = DateTime.UtcNow; }
                    else if (Timeout(MoveTimeoutMs)) GoError("Center move timeout", AlarmKey.CenterMoveTimeout);
                    break;

                case Step.LightingGrab1:
                    if (IsDryRun) { _grab1Done = true; _step = Step.SearchMulti; _tick = DateTime.UtcNow; break; }
                    SetLightingForMultiAction?.Invoke();
                    if (!_grab1Done)
                    {
                        if (GrabImageFunc?.Invoke() ?? true) { _grab1Done = true; _step = Step.SearchMulti; _tick = DateTime.UtcNow; }
                        else if (Timeout(GrabTimeoutMs)) GoError("Grab1 timeout", AlarmKey.Grab1Timeout);
                    }
                    break;

                case Step.SearchMulti:
                    if (IsDryRun)
                    {
                        _calcTheta = 0.0;
                        _targetT = _stage.GetTP(_stage.TeachingPositions[InputStageConfig.TeachingPositionName.Align], _stage.AxisT);
                        _multiSearched = true; _step = Step.MoveTAxis; _tick = DateTime.UtcNow; break;
                    }
                    var res = FindMultiMarksFunc?.Invoke();
                    if (res.HasValue && res.Value.ok && res.Value.thetaList != null && res.Value.thetaList.Count >= 3)
                    {
                        var list = new List<double>(res.Value.thetaList); list.Sort();
                        int skip = (int)ThetaShiftFilterExcludeCount;
                        if (skip * 2 < list.Count)
                        {
                            var trimmed = list.GetRange(skip, list.Count - skip * 2);
                            double sum = 0; foreach (var v in trimmed) sum += v; _calcTheta = sum / trimmed.Count;
                            if (Math.Abs(_calcTheta) < ThetaApplyLimitDeg)
                            {
                                double baseT = _stage.GetTP(_stage.TeachingPositions[InputStageConfig.TeachingPositionName.Align], _stage.AxisT);
                                _targetT = baseT + _calcTheta; _multiSearched = true; _step = Step.MoveTAxis; _tick = DateTime.UtcNow;
                            }
                            else GoError("Theta over limit", AlarmKey.ThetaOverLimit);
                        }
                        else GoError("Theta list too small after trim", AlarmKey.MultiSearchFail);
                    }
                    else GoError("Multi mark fail", AlarmKey.MultiSearchFail);
                    break;

                case Step.MoveTAxis:
                    if (!_multiSearched) { GoError("MoveTAxis without search", AlarmKey.MissingPrecondition); break; }
                    _stage.MoveAxisOnce(_stage.AxisT, _targetT); _step = Step.MoveTAxis_Check; _tick = DateTime.UtcNow; break;

                case Step.MoveTAxis_Check:
                    if (_stage.InPos(_stage.AxisT, _targetT)) { _step = Step.LightingGrab2; _tick = DateTime.UtcNow; }
                    else if (Timeout(MoveTimeoutMs)) GoError("T move timeout", AlarmKey.MoveTRotationTimeout);
                    break;

                case Step.LightingGrab2:
                    if (IsDryRun) { _grab2Done = true; _step = Step.SearchCenter; _tick = DateTime.UtcNow; break; }
                    SetLightingForCenterAction?.Invoke();
                    if (!_grab2Done)
                    {
                        if (GrabImageFunc?.Invoke() ?? true) { _grab2Done = true; _step = Step.SearchCenter; _tick = DateTime.UtcNow; }
                        else if (Timeout(GrabTimeoutMs)) GoError("Grab2 timeout", AlarmKey.Grab2Timeout);
                    }
                    break;

                case Step.SearchCenter:
                    if (IsDryRun)
                    {
                        _targetX = _stage.GetTP(_stage.TeachingPositions[InputStageConfig.TeachingPositionName.Align], _stage.AxisX);
                        _targetY = _stage.GetTP(_stage.TeachingPositions[InputStageConfig.TeachingPositionName.Align], _stage.AxisY);
                        _centerSearched = true; _step = Step.MoveXYAxis; _tick = DateTime.UtcNow; break;
                    }
                    var rc = FindCenterMarkFunc?.Invoke();
                    if (rc.HasValue && rc.Value.ok)
                    {
                        double baseX = _stage.GetTP(_stage.TeachingPositions[InputStageConfig.TeachingPositionName.Align], _stage.AxisX);
                        double baseY = _stage.GetTP(_stage.TeachingPositions[InputStageConfig.TeachingPositionName.Align], _stage.AxisY);
                        _targetX = baseX + rc.Value.x; _targetY = baseY + rc.Value.y; _centerSearched = true; _step = Step.MoveXYAxis; _tick = DateTime.UtcNow;
                    }
                    else GoError("Center mark fail", AlarmKey.CenterSearchFail);
                    break;

                case Step.MoveXYAxis:
                    if (!_centerSearched) { GoError("MoveXYAxis without center search", AlarmKey.MissingPrecondition); break; }
                    _stage.MoveAxisOnce(_stage.AxisX, _targetX); _stage.MoveAxisOnce(_stage.AxisY, _targetY); _step = Step.MoveXYAxis_Check; _tick = DateTime.UtcNow; break;

                case Step.MoveXYAxis_Check:
                    if (_stage.InPos(_stage.AxisX, _targetX) && _stage.InPos(_stage.AxisY, _targetY)) { _step = Step.Align_Check; _tick = DateTime.UtcNow; }
                    else if (Timeout(MoveTimeoutMs)) GoError("XY move timeout", AlarmKey.XYMoveTimeout);
                    break;

                case Step.Align_Check:
                    if (!_centerSearched) { GoError("Align check without center search", AlarmKey.MissingPrecondition); break; }
                    bool xOk = _stage.InPos(_stage.AxisX, _targetX);
                    bool yOk = _stage.InPos(_stage.AxisY, _targetY);
                    bool tOk = _stage.InPos(_stage.AxisT, _targetT);
                    if (xOk && yOk && tOk) _step = Step.Finish; else if (Timeout(MoveTimeoutMs)) GoError("Align inpos timeout", AlarmKey.AlignInposTimeout);
                    break;

                case Step.Finish:
                    return -1;
                case Step.Error:
                    return -1;
            }

            // SoftStop 처리: Idle 전환 후 루프 종료
            if (_softStopRequested && _step != Step.Idle && _step != Step.Finish && _step != Step.Error)
            {
                _step = Step.Idle;
                return -1;
            }

            // Step 변경 시 로그 (번호:이름 형식)
            if (beforeStep != _step && _prevLoggedStep != _step)
            {
                try
                {
                    string msg = "StepChange: " + StepToCode(beforeStep) + " -> " + StepToCode(_step);
                    Log.Write("SeqInputChipAlignVision", "Step", "AlignVision", msg);
                }
                catch { }
                _prevLoggedStep = _step;
            }

            return current + 1;
        }

        private bool Timeout(int ms) => (DateTime.UtcNow - _tick).TotalMilliseconds >= ms;

        private void GoError(string msg, AlarmKey key)
        {
            try { System.Diagnostics.Debug.WriteLine("[SeqInputChipAlignVision] ERROR: " + msg); } catch { }
            try { Log.Write("SeqInputChipAlignVision", "Error", "AlignVision", msg); } catch { }
            PostAlarm(key, $"Step={_step} Msg={msg}");
            _step = Step.Error;
        }
    }
}
