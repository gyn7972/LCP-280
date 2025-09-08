using System;
using System.Collections.Generic;
using System.Linq;
using QMC.Common;
using QMC.Common.Sequence;
using QMC.Common.Alarm;
using QMC.LCP_280.Process.Unit;

namespace QMC.LCP_280.Process.Component
{
    /// <summary>
    /// 웨이퍼 스테이지 전체 칩 스캔(LocalScan) 시퀀스 (단일 클래스 직접 접근형)
    /// 간단한 내부 구현(비전/피치/맵)을 실제 InputStage 기능을 이용하여 동작 가능하도록 보완.
    /// </summary>
    internal class SeqWaferLocalScan : SequenceBase
    {
        #region Step 정의
        public enum Step
        {
            Idle = 0,
            InitSummary,
            StartFirstGrab,
            WaitFirstGrab,
            SetupInitialIndex,
            MoveScanForward,
            GrabForward,
            WaitForwardGrab,
            EvalForwardMatch,
            UpdatePitchForward,
            AdvanceForward,
            PrepareReversePivot,
            MoveScanReverse,
            GrabReverse,
            WaitReverseGrab,
            EvalReverseMatch,
            UpdatePitchReverse,
            AdvanceReverse,
            AfterYScanProcess,
            PreFinalize,
            SaveAndMatchMap,
            ResetScanDirFlag,
            Finalize,
            Finish,
            Error
        }
        #endregion

        #region Alarm 정의
        private enum AlarmKey
        {
            First = 43000,
            VisionGrabFail,
            VisionMatchFail,
            MoveTimeout,
            PitchCalcFail,
            MappingFail,
            GenericError
        }
        private readonly Dictionary<int, AlarmInfo> _alarms = new Dictionary<int, AlarmInfo>();
        private bool _alarmsInitialized;
        private void InitAlarms()
        {
            if (_alarmsInitialized) return; _alarmsInitialized = true;
            AddAlarm(AlarmKey.VisionGrabFail, "Vision Grab Fail", "비전 Grab 실패", "Error");
            AddAlarm(AlarmKey.VisionMatchFail, "Vision Match Fail", "비전 매칭 실패 횟수 초과", "Error");
            AddAlarm(AlarmKey.MoveTimeout, "Move Timeout", "축 이동 타임아웃", "Error");
            AddAlarm(AlarmKey.PitchCalcFail, "Pitch Calc Fail", "Pitch 계산 실패", "Error");
            AddAlarm(AlarmKey.MappingFail, "Mapping Fail", "맵 생성/검증 실패", "Error");
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
                if (AlarmManager.Instance.Alarms.Any(a => a.Code == info.Code)) return;
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

        #region Pause / SoftStop
        private volatile bool _pauseRequested;
        private volatile bool _softStopRequested;
        public bool IsPaused => _pauseRequested;
        public void Pause() => _pauseRequested = true;
        public void Resume() { if (_pauseRequested) { _pauseRequested = false; _tick = DateTime.UtcNow; } }
        public void SoftStop() => _softStopRequested = true;
        #endregion

        #region 필드 / 상태
        private readonly InputStage _stage;
        private Step _step = Step.Idle;
        private Step _prevLoggedStep = Step.Idle;
        private DateTime _tick;
        private int _visionRetryCount;
        private int _matchFailTotal;
        private bool _forwardDirection = true;
        private bool _reverseScanDone;
        private bool _yReferenceMade;

        // 스캔 인덱스
        private int _indexX;
        private int _indexY;
        private int _yIndexRef;
        private (double x, double y) _originPos;
        private (double x, double y) _curPos;

        // Pitch/Limit
        private double _pitchY = 1.0;            // 동적 계산 (첫 2 매칭 후 추정)
        private int _scanLimitForward = 200;     // 최대 Forward 스텝 (fallback)
        private int _scanLimitReverse = 200;     // 최대 Reverse 스텝 (fallback)
        private int _minPitchSamples = 3;        // 평균 계산용 최소 매칭 샘플 수

        // Vision / Mapping 캐시
        private bool _visionSearchedThisGrab;    // Grab 후 1회 검색 제어
        private int _lastMatchCount;             // 최근 검색 매칭 개수
        private double _lastMapMatchRate;        // 저장된 맵 매치율
        private readonly List<ScanPointLog> _scanLog = new List<ScanPointLog>();
        private readonly List<double> _pitchYSamples = new List<double>();
        private (double yPos, int yIndex)? _lastPitchRef; // 피치 계산용 이전 매칭 점

        private class ScanPointLog
        {
            public int Ix; public int Iy; public double X; public double Y; public bool Matched; public DateTime Time;
        }

        // Timeout 파라미터
        public int MoveTimeoutMs { get; set; } = 8000;
        public int GrabTimeoutMs { get; set; } = 3000;
        public int VisionMissAllow { get; set; } = 3;

        private bool IsDryRun => _stage?.DryRun ?? false;
        public Step CurrentStep => _step;
        #endregion

        #region 생성 / 시작
        public SeqWaferLocalScan(InputStage stage) : base("SeqWaferLocalScan")
        {
            _stage = stage ?? throw new ArgumentNullException(nameof(stage));
            InitAlarms();
        }
        public bool Start(Step first = Step.InitSummary)
        {
            _step = first;
            _prevLoggedStep = Step.Idle;
            _tick = DateTime.UtcNow;
            _pauseRequested = false; _softStopRequested = false;
            ResetInternals();
            return base.Start(0);
        }
        private void ResetInternals()
        {
            _visionRetryCount = 0;
            _matchFailTotal = 0;
            _forwardDirection = true;
            _reverseScanDone = false;
            _yReferenceMade = false;
            _indexX = 0; _indexY = 0; _yIndexRef = 0;
            _originPos = (0, 0); _curPos = (0, 0);
            _scanLog.Clear(); _pitchYSamples.Clear(); _lastPitchRef = null; _lastMapMatchRate = 0;
            _visionSearchedThisGrab = false; _lastMatchCount = 0;
        }
        #endregion

        #region 내부 Helper (실제 구현)
        private bool IsReady()
        {
            if (_stage == null) return false;
            // 기본 Ready 조건: 축 객체 존재 + 카메라 존재(Open 여부는 optional)
            return _stage.AxisX != null && _stage.AxisY != null;
        }
        private void InitOrResumeSummary()
        {
            // 간단 로그 + 내부 통계 초기화 (재호출 대비)
            Log.Write(Name, "Info", Name, "Summary Init/Resume");
            _scanLog.Clear(); _pitchYSamples.Clear(); _lastPitchRef = null; _lastMapMatchRate = 0; _visionSearchedThisGrab = false; _lastMatchCount = 0;
        }
        private void MarkScanStart() { Log.Write(Name, "Info", Name, "ScanStart: t=" + DateTime.Now.ToString("HH:mm:ss")); }
        private void MarkScanEnd() { Log.Write(Name, "Info", Name, "ScanEnd: Points=" + _scanLog.Count + " MatchRate=" + _lastMapMatchRate.ToString("F2")); }

        private void SaveMap()
        {
            // 실제 프로젝트: 파일/DB 저장. 여기서는 요약 로그만.
            int matched = _scanLog.Count(p => p.Matched);
            Log.Write(Name, "Map", Name, $"SaveMap Points={_scanLog.Count} Matched={matched}");
        }
        private void GenerateProberMap()
        {
            // 간단: 스캔 로그를 기준으로 범위 계산
            if (_scanLog.Count == 0) return;
            int minY = _scanLog.Min(p => p.Iy); int maxY = _scanLog.Max(p => p.Iy);
            Log.Write(Name, "Map", Name, $"GenerateProberMap YRange=[{minY}..{maxY}] PitchY={_pitchY:F4}");
        }
        private void DoMapMatch()
        {
            // 간단한 매치율: (매칭된 포인트 / 전체) * 100
            if (_scanLog.Count == 0) { _lastMapMatchRate = 0; return; }
            int matched = _scanLog.Count(p => p.Matched);
            _lastMapMatchRate = (double)matched / _scanLog.Count * 100.0;
            Log.Write(Name, "Map", Name, $"DoMapMatch MatchRate={_lastMapMatchRate:F2}%");
        }
        private double GetMapMatchRate() => _lastMapMatchRate;
        private void OnMergeSuccess() { Log.Write(Name, "Info", Name, "Merge Success (Map Accept)"); }
        private void OnMergeFail() { Log.Write(Name, "Warn", Name, "Merge Fail (Map Reject)"); }
        private void AllocateAreaAndPickInfo() { /* 실프로젝트: Area/PickInfo 재구성 */ }

        // Vision 처리 --------------------------------------------------
        private bool StartGrab()
        {
            _visionSearchedThisGrab = false; _lastMatchCount = 0;
            if (IsDryRun) return true;
            try { return _stage.GrabImageFunc?.Invoke() ?? false; } catch { return false; }
        }
        private bool VisionWorking() => false; // 동기 Grab

        private void EnsureVisionSearch()
        {
            if (_visionSearchedThisGrab) return;
            _visionSearchedThisGrab = true;
            if (IsDryRun)
            {
                _lastMatchCount = 1; return;
            }
            try
            {
                var multi = _stage.FindMultiMarksFunc?.Invoke();
                if (multi.HasValue && multi.Value.ok && multi.Value.thetaList != null && multi.Value.thetaList.Count > 0)
                {
                    _lastMatchCount = multi.Value.thetaList.Count; // 단순 개수
                }
                else _lastMatchCount = 0;
            }
            catch { _lastMatchCount = 0; }
        }
        private int VisionMatchCountForward() { EnsureVisionSearch(); return _lastMatchCount; }
        private int VisionMatchCountReverse() { EnsureVisionSearch(); return _lastMatchCount; }

        // Pitch 계산 ---------------------------------------------------
        private bool AveragePitchForward()
        {
            // 피치 재계산: 매칭 성공한 위치를 기반으로 Y 간격 누적 평균
            try { UpdatePitchSample(); } catch (Exception ex) { Log.Write(Name, "Pitch", Name, "Forward Pitch sample err:" + ex.Message); }
            return true;
        }
        private bool AveragePitchReverse()
        {
            try { UpdatePitchSample(); } catch (Exception ex) { Log.Write(Name, "Pitch", Name, "Reverse Pitch sample err:" + ex.Message); }
            return true;
        }
        private void UpdatePitchSample()
        {
            // 현재 축 Y 위치
            double curY = _stage?.AxisY?.GetPosition() ?? (_originPos.y + _indexY * _pitchY);
            if (!_lastPitchRef.HasValue)
            {
                _lastPitchRef = (curY, _indexY); return;
            }
            var prev = _lastPitchRef.Value;
            if (_indexY == prev.yIndex) return; // 동일 인덱스는 무시
            double dyAbs = Math.Abs(curY - prev.yPos);
            int stepDiff = Math.Abs(_indexY - prev.yIndex);
            if (stepDiff > 0 && dyAbs > 1e-6)
            {
                double pitch = dyAbs / stepDiff;
                if (pitch > 0.0005 && pitch < 50) // 현실적인 범위 필터
                {
                    _pitchYSamples.Add(pitch);
                    if (_pitchYSamples.Count >= _minPitchSamples)
                        _pitchY = _pitchYSamples.Average();
                }
            }
            _lastPitchRef = (curY, _indexY);
        }

        private void SearchRefYForward() { /* 방향 전환 판단용 추가 로직 필요 시 구현 */ }
        private void SearchRefYReverse() { /* 역방향 기준선 보정용 */ }
        private bool MakeYReference() { return _pitchY > 0; }
        private void SearchRefX() { /* X Ref 탐색 필요 시 구현 */ }

        // 작업 영역 판단 ------------------------------------------------
        private bool InWorkingArea((double x, double y) pos)
        {
            // 1) 인덱스 기반 제한
            if (_forwardDirection && _indexY >= _scanLimitForward) return false;
            if (!_forwardDirection && -_indexY >= _scanLimitReverse) return false;

            // 2) 축 소프트리밋 사용 (가능하면)
            try
            {
                var ay = _stage?.AxisY;
                if (ay != null)
                {
                    // 리플렉션 없이 GetStatusSnapshot() 등 사용 가능하나 간단히 현재 위치만 범위 체크(Teach 기반)
                    // SoftLimitMin/Max 추출이 곤란하면 생략.
                }
            }
            catch { }

            // 3) Pitch 기반 Y 이동 거리 제한 (예:  pitch * scanLimitForward * 1.2)
            double travelY = Math.Abs(pos.y - _originPos.y);
            double maxTravel = _pitchY * _scanLimitForward * 1.2;
            if (travelY > maxTravel && _pitchY > 0.0001) return false;
            return true;
        }

        private (double x, double y) IndexToPosition(int ix, int iy)
        {
            // 현재 구현은 X 고정, Y 는 Pitch 반영
            return (_originPos.x, _originPos.y + iy * _pitchY);
        }
        #endregion

        #region 유틸
        private bool Timeout(int ms) => (DateTime.UtcNow - _tick).TotalMilliseconds >= ms;
        private string StepToCode(Step s) => ((int)s) + ":" + s;
        private void GoError(string msg, AlarmKey key)
        {
            try { System.Diagnostics.Debug.WriteLine("[SeqWaferLocalScan] ERROR: " + msg); } catch { }
            try { Log.Write(Name, "Error", Name, msg); } catch { }
            PostAlarm(key, $"Step={_step} Msg={msg}");
            _step = Step.Error;
        }
        #endregion

        #region ExecuteStep
        protected override int ExecuteStep(int current, System.Threading.CancellationToken ct)
        {
            if (_pauseRequested) return current;

            var before = _step;
            switch (_step)
            {
                case Step.Idle: return -1;

                case Step.InitSummary:
                    if (!IsReady()) { GoError("Not Ready", AlarmKey.GenericError); break; }
                    try { InitOrResumeSummary(); } catch (Exception ex) { GoError("Summary init fail:" + ex.Message, AlarmKey.GenericError); break; }
                    _step = Step.StartFirstGrab; _tick = DateTime.UtcNow; break;

                case Step.StartFirstGrab:
                    MarkScanStart();
                    if (!IsDryRun)
                    {
                        if (!StartGrab()) { GoError("First grab start fail", AlarmKey.VisionGrabFail); break; }
                    }
                    else StartGrab(); // DryRun에서도 상태 초기화 용도
                    _step = Step.WaitFirstGrab; _tick = DateTime.UtcNow; break;

                case Step.WaitFirstGrab:
                    if (IsDryRun || !VisionWorking())
                    {
                        _originPos = (_stage.AxisX?.GetPosition() ?? 0, _stage.AxisY?.GetPosition() ?? 0);
                        _curPos = _originPos;
                        _indexX = 0; _indexY = 0; _yIndexRef = 0;
                        _step = Step.SetupInitialIndex; _tick = DateTime.UtcNow;
                    }
                    else if (Timeout(GrabTimeoutMs)) GoError("Initial grab timeout", AlarmKey.VisionGrabFail);
                    break;

                case Step.SetupInitialIndex:
                    _visionRetryCount = 0; _forwardDirection = true; _reverseScanDone = false; _yReferenceMade = false;
                    _step = Step.MoveScanForward; _tick = DateTime.UtcNow; break;

                case Step.MoveScanForward:
                    _curPos = IndexToPosition(_indexX, _indexY);
                    if (!InWorkingArea(_curPos)) { _step = Step.PrepareReversePivot; _tick = DateTime.UtcNow; break; }
                    if (!IsDryRun)
                    {
                        _stage.MoveAxisOnce(_stage.AxisY, _curPos.y);
                        _stage.MoveAxisOnce(_stage.AxisX, _curPos.x);
                    }
                    _step = Step.GrabForward; _tick = DateTime.UtcNow; break;

                case Step.GrabForward:
                    if (!StartGrab()) { if (!IsDryRun) { GoError("Forward grab fail", AlarmKey.VisionGrabFail); break; } }
                    _step = Step.WaitForwardGrab; _tick = DateTime.UtcNow; break;

                case Step.WaitForwardGrab:
                    if (IsDryRun || !VisionWorking()) { _step = Step.EvalForwardMatch; _tick = DateTime.UtcNow; }
                    else if (Timeout(GrabTimeoutMs)) GoError("Forward grab timeout", AlarmKey.VisionGrabFail);
                    break;

                case Step.EvalForwardMatch:
                    {
                        int mc = VisionMatchCountForward();
                        bool matched = mc > 0;
                        _scanLog.Add(new ScanPointLog { Ix = _indexX, Iy = _indexY, X = _curPos.x, Y = _curPos.y, Matched = matched, Time = DateTime.Now });
                        if (matched)
                        { _visionRetryCount = 0; _step = Step.UpdatePitchForward; _tick = DateTime.UtcNow; }
                        else
                        {
                            _visionRetryCount++;
                            if (_visionRetryCount > VisionMissAllow)
                            { _matchFailTotal++; _visionRetryCount = 0; _step = Step.PrepareReversePivot; }
                            else _step = Step.AdvanceForward;
                            _tick = DateTime.UtcNow;
                        }
                    }
                    break;

                case Step.UpdatePitchForward:
                    if (!AveragePitchForward()) { GoError("AveragePitchForward fail", AlarmKey.PitchCalcFail); break; }
                    SearchRefYForward();
                    _step = Step.AdvanceForward; _tick = DateTime.UtcNow; break;

                case Step.AdvanceForward:
                    _indexY++;
                    if (_indexY >= _scanLimitForward) { _step = Step.PrepareReversePivot; }
                    else { _step = Step.MoveScanForward; }
                    _tick = DateTime.UtcNow; break;

                case Step.PrepareReversePivot:
                    if (!AveragePitchForward()) { GoError("AveragePitch(FwdEnd) fail", AlarmKey.PitchCalcFail); break; }
                    _indexY = _yIndexRef; _curPos = _originPos; _forwardDirection = false; _visionRetryCount = 0;
                    _step = Step.MoveScanReverse; _tick = DateTime.UtcNow; break;

                case Step.MoveScanReverse:
                    if (_reverseScanDone) { _step = Step.AfterYScanProcess; _tick = DateTime.UtcNow; break; }
                    _curPos = IndexToPosition(_indexX, _indexY);
                    if (!InWorkingArea(_curPos)) { _reverseScanDone = true; _step = Step.AfterYScanProcess; break; }
                    if (!IsDryRun)
                    {
                        _stage.MoveAxisOnce(_stage.AxisY, _curPos.y);
                        _stage.MoveAxisOnce(_stage.AxisX, _curPos.x);
                    }
                    _step = Step.GrabReverse; _tick = DateTime.UtcNow; break;

                case Step.GrabReverse:
                    if (!StartGrab()) { if (!IsDryRun) { GoError("Reverse grab fail", AlarmKey.VisionGrabFail); break; } }
                    _step = Step.WaitReverseGrab; _tick = DateTime.UtcNow; break;

                case Step.WaitReverseGrab:
                    if (IsDryRun || !VisionWorking()) { _step = Step.EvalReverseMatch; _tick = DateTime.UtcNow; }
                    else if (Timeout(GrabTimeoutMs)) GoError("Reverse grab timeout", AlarmKey.VisionGrabFail);
                    break;

                case Step.EvalReverseMatch:
                    {
                        int mc = VisionMatchCountReverse();
                        bool matched = mc > 0;
                        _scanLog.Add(new ScanPointLog { Ix = _indexX, Iy = _indexY, X = _curPos.x, Y = _curPos.y, Matched = matched, Time = DateTime.Now });
                        if (matched) { _visionRetryCount = 0; _step = Step.UpdatePitchReverse; }
                        else
                        {
                            _visionRetryCount++;
                            if (_visionRetryCount > VisionMissAllow)
                            { _matchFailTotal++; _visionRetryCount = 0; _reverseScanDone = true; _step = Step.AfterYScanProcess; }
                            else _step = Step.AdvanceReverse;
                        }
                        _tick = DateTime.UtcNow;
                    }
                    break;

                case Step.UpdatePitchReverse:
                    if (!AveragePitchReverse()) { GoError("AveragePitchReverse fail", AlarmKey.PitchCalcFail); break; }
                    SearchRefYReverse();
                    _step = Step.AdvanceReverse; _tick = DateTime.UtcNow; break;

                case Step.AdvanceReverse:
                    _indexY--;
                    if (_indexY <= -_scanLimitReverse) { _reverseScanDone = true; _step = Step.AfterYScanProcess; }
                    else { _step = Step.MoveScanReverse; }
                    _tick = DateTime.UtcNow; break;

                case Step.AfterYScanProcess:
                    if (!_yReferenceMade)
                    {
                        if (!MakeYReference())
                        {
                            _forwardDirection = true; _reverseScanDone = false; _indexY = 0; _curPos = _originPos; _step = Step.MoveScanForward; _tick = DateTime.UtcNow; break;
                        }
                        _yReferenceMade = true; SearchRefX();
                    }
                    _step = Step.PreFinalize; _tick = DateTime.UtcNow; break;

                case Step.PreFinalize:
                    _step = Step.SaveAndMatchMap; _tick = DateTime.UtcNow; break;

                case Step.SaveAndMatchMap:
                    try
                    {
                        SaveMap();
                        GenerateProberMap();
                        DoMapMatch();
                        double rate = GetMapMatchRate();
                        if (rate >= 10 && rate <= 100) { OnMergeSuccess(); }
                        else { OnMergeFail(); }
                        AllocateAreaAndPickInfo();
                    }
                    catch (Exception ex) { GoError("Map Save/Match fail:" + ex.Message, AlarmKey.MappingFail); break; }
                    _step = Step.ResetScanDirFlag; _tick = DateTime.UtcNow; break;

                case Step.ResetScanDirFlag:
                    _forwardDirection = true; _step = Step.Finalize; _tick = DateTime.UtcNow; break;

                case Step.Finalize:
                    MarkScanEnd();
                    _step = Step.Finish; _tick = DateTime.UtcNow; break;

                case Step.Finish: return -1;
                case Step.Error: return -1;
            }

            if (_softStopRequested && _step != Step.Idle && _step != Step.Finish && _step != Step.Error)
            { _step = Step.Idle; return -1; }

            if (before != _step && _prevLoggedStep != _step)
            {
                try { Log.Write(Name, "Step", Name, "StepChange: " + StepToCode(before) + " -> " + StepToCode(_step)); } catch { }
                _prevLoggedStep = _step;
            }
            return current + 1;
        }
        #endregion
    }
}
