using System;
using System.Collections.Generic;
using System.Linq;
using QMC.Common;
using QMC.Common.Sequence;
using QMC.LCP_280.Process.Unit;
using QMC.Common.Alarm;

namespace QMC.LCP_280.Process.Component
{
    /// <summary>
    /// 입력 스테이지 칩 매핑 비전 시퀀스
    /// 요구 흐름 (개편):
    ///  1) 변수 초기화 및 카메라 & 조명 준비 확인
    ///  2) 웨이퍼 스캔 영역(가로*세로)을 스텝 이동하며 Chip(마크) 탐색 (X,Y 축 Step 이동)
    ///     - 스캔 스텝 = 서치 ROI(또는 설정된 StepX / StepY)
    ///  3) 각 스캔 포인트에서 그랩 & 서치 → 발견 칩 절대 Stage 좌표 수집
    ///     조건1) 찾은 Chip 은 Raw 리스트에 저장 (나중에 매핑) 
    ///     조건2) 가장 가운데(중심) 칩을 (0,0) 기준으로 매핑
    ///     조건3) 존재하지 않는 칩 셀은 Present=false 로 표시
    ///     조건4) 칩 Raw 좌표로 Pitch 자동 계산 → Grid (? x ?) 판정
    ///  4) Pitch / Center 기준으로 Col/Row 산출 후 Chip Data(맵) 생성
    ///  5) 완료 (Finish)
    /// DryRun: 모션/그랩/서치를 단순 통과 & 가짜 데이터 생성 가능
    /// </summary>
    internal class SeqInputChipMappingVision : SequenceBase
    {
        // === Pause / SoftStop 지원 필드 ===
        private volatile bool _pauseRequested;     // 일시정지
        private volatile bool _softStopRequested;  // 소프트 정지
        public bool IsPaused => _pauseRequested;
        public void Pause() => _pauseRequested = true;
        public void Resume() { if (_pauseRequested) { _pauseRequested = false; _tick = DateTime.UtcNow; } }
        public void SoftStop() => _softStopRequested = true;

        #region Step 정의
        public enum Step
        {
            Idle = 0,
            Init,
            Prepare,
            MoveScanStart, MoveScanStart_Check,
            Scan_Grab, Scan_Search, Scan_Record,
            Scan_Next_Calc, Scan_Next_Move, Scan_Next_Move_Check,
            ComputeMapping,
            Finish,
            Error
        }
        #endregion

        #region Alarm 정의 (필요 최소)
        private enum AlarmKey
        {
            First = 42000,
            CameraNotReady,
            MoveTimeout,
            GrabTimeout,
            SearchFail,
            MappingFail,
            GenericError
        }
        private readonly Dictionary<int, AlarmInfo> _alarms = new Dictionary<int, AlarmInfo>();
        private bool _alarmsInitialized;
        private void InitAlarms()
        {
            if (_alarmsInitialized) return; _alarmsInitialized = true;
            AddAlarm(AlarmKey.CameraNotReady, "Camera Not Ready", "카메라 준비 미완료", "Error");
            AddAlarm(AlarmKey.MoveTimeout, "Move Timeout", "이동 타임아웃", "Error");
            AddAlarm(AlarmKey.GrabTimeout, "Grab Timeout", "이미지 취득 실패", "Error");
            AddAlarm(AlarmKey.SearchFail, "Search Fail", "칩 탐색 실패", "Error");
            AddAlarm(AlarmKey.MappingFail, "Mapping Fail", "매핑 데이터 생성 실패", "Error");
            AddAlarm(AlarmKey.GenericError, "Generic Error", "일반 오류", "Error");
        }
        private void AddAlarm(AlarmKey key, string title, string cause, string grade)
        {
            _alarms[(int)key] = new AlarmInfo { Code = (int)key, Title = title, Cause = cause, Source = Name, Grade = grade, GeneratedTime = DateTime.Now };
        }
        private void PostAlarm(AlarmKey key, string detail = null)
        {
            try
            {
                if (!_alarmsInitialized) InitAlarms();
                if (!_alarms.TryGetValue((int)key, out var info)) return;
                if (AlarmManager.Instance.Alarms.Any(al => al.Code == info.Code)) return; // 중복 방지
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

        #region 외부 Hook 델리게이트 (InputStage 바인딩)
        public Func<bool> EnsureCameraReadyFunc { get; set; }
        public Action SetLightingForMultiAction { get; set; }
        public Func<bool> GrabImageFunc { get; set; }
        /// <summary>
        /// 현재 ROI/포지션에서 칩(마크) 검색 (여러 칩). 좌표 단위: mm (Stage 이동 기준과 동일 스케일)
        /// - 반환: ok, chips 리스트 (x,y) (현재 Stage (기준) 위치에서의 상대Shift(mm))
        /// </summary>
        public Func<(bool ok, List<(double x, double y)> chips)> FindChipsFunc { get; set; }
        #endregion

        #region 내부 상태/데이터
        private readonly InputStage _stage;
        private Step _step = Step.Idle;
        private Step _prevLoggedStep = Step.Idle;
        private DateTime _tick;
        private bool _camReady;
        private bool _grabDone;
        private bool _searchDone;

        // 스캔 인덱스
        private int _scanIndexX;
        private int _scanIndexY;
        private int _totalStepsX;
        private int _totalStepsY;

        // 수집된 Raw Chip 절대좌표 (Stage 기준)
        private readonly List<(double X, double Y)> _rawChipAbs = new List<(double X, double Y)>();

        // 매핑 결과 구조
        public class ChipCell
        {
            public int Col; public int Row; public bool Present; public double AbsX; public double AbsY; }
        public Dictionary<(int Col, int Row), ChipCell> ChipMap { get; private set; } = new Dictionary<(int, int), ChipCell>();

        // Pitch 및 그리드 범위
        public double PitchX { get; private set; }
        public double PitchY { get; private set; }
        public int MinCol { get; private set; }
        public int MaxCol { get; private set; }
        public int MinRow { get; private set; }
        public int MaxRow { get; private set; }
        private (double X, double Y) _centerChip; // 중심 칩 절대 좌표

        // 파라미터 (설정 가능)
        public int MoveTimeoutMs { get; set; } = 8000;
        public int GrabTimeoutMs { get; set; } = 3000;
        public bool SerpentineScan { get; set; } = true; // 짝수/홀수 줄 반대 진행
        public double ScanOriginX { get; set; } = 0.0;    // 스캔 시작 Stage X (mm)
        public double ScanOriginY { get; set; } = 0.0;    // 스캔 시작 Stage Y (mm)
        public double ScanWidth { get; set; } = 50.0;     // 스캔 폭 (mm)
        public double ScanHeight { get; set; } = 50.0;    // 스캔 높이 (mm)
        public double StepX { get; set; } = 5.0;          // X 방향 Step (mm)
        public double StepY { get; set; } = 5.0;          // Y 방향 Step (mm)
        public double PitchDetectTrimRatio { get; set; } = 0.2; // Pitch 추정시 양끝 Trim 비율
        public double PitchToleranceFactor { get; set; } = 0.35; // Pitch 클러스터 허용 비율
        public double MissingChipPosTolerance { get; set; } = 0.45; // 칩 위치 → 그리드 매핑 허용( Pitch * factor )

        private bool IsDryRun => _stage?.DryRun ?? false;
        #endregion

        #region 생성자 / 시작
        public SeqInputChipMappingVision(InputStage stage) : base("SeqInputChipMappingVision")
        {
            _stage = stage ?? throw new ArgumentNullException(nameof(stage));
            InitAlarms();
        }
        public bool Start(Step first = Step.Init)
        {
            BindStageDelegatesIfNeeded();
            _step = first;
            _prevLoggedStep = Step.Idle;
            _tick = DateTime.UtcNow;
            _pauseRequested = false; _softStopRequested = false;
            ResetFlags();
            return base.Start(0);
        }
        public bool StartSingle(string stepName)
        {
            if (string.IsNullOrWhiteSpace(stepName)) return false;
            if (IsRunning) return false;
            if (!Enum.TryParse(stepName, true, out Step v)) return false;
            _step = v; _prevLoggedStep = Step.Idle; _tick = DateTime.UtcNow;
            _pauseRequested = false; _softStopRequested = false;
            ResetFlags();
            BindStageDelegatesIfNeeded();
            return base.Start(0);
        }
        public static string[] GetStepNames() => Enum.GetNames(typeof(Step));
        private void BindStageDelegatesIfNeeded()
        {
            if (EnsureCameraReadyFunc == null) EnsureCameraReadyFunc = _stage.CamReadyFunc;
            if (SetLightingForMultiAction == null) SetLightingForMultiAction = _stage.SetLightingMultiAction;
            if (GrabImageFunc == null) GrabImageFunc = _stage.GrabImageFunc;
            // FindChipsFunc 은 외부(또는 Stage)에서 주입 필요. (없을 경우 DryRun 또는 실패 처리)
        }
        private void ResetFlags()
        {
            _camReady = false; _grabDone = false; _searchDone = false;
            _scanIndexX = 0; _scanIndexY = 0;
            _rawChipAbs.Clear();
            ChipMap.Clear();
            PitchX = PitchY = 0;
            _centerChip = (0, 0);
            MinCol = MaxCol = MinRow = MaxRow = 0;
            // 스텝 계산
            _totalStepsX = StepX > 0 ? Math.Max(1, (int)Math.Floor(ScanWidth / StepX) + 1) : 1;
            _totalStepsY = StepY > 0 ? Math.Max(1, (int)Math.Floor(ScanHeight / StepY) + 1) : 1;
        }
        #endregion

        #region 유틸
        private string StepToCode(Step s) => ((int)s) + ":" + s;
        private bool Timeout(int ms) => (DateTime.UtcNow - _tick).TotalMilliseconds >= ms;
        private void GoError(string msg, AlarmKey key)
        {
            try { System.Diagnostics.Debug.WriteLine("[SeqInputChipMappingVision] ERROR: " + msg); } catch { }
            try { Log.Write(Name, "Error", Name, msg); } catch { }
            PostAlarm(key, $"Step={_step} Msg={msg}");
            _step = Step.Error;
        }
        private (double X, double Y) CurrentScanStageTarget()
        {
            double x = ScanOriginX + _scanIndexX * StepX;
            double yLine = ScanOriginY + _scanIndexY * StepY;
            // Serpentine: 홀수줄 역방향
            if (SerpentineScan && (_scanIndexY % 2 == 1))
                x = ScanOriginX + ( _totalStepsX - 1 - _scanIndexX) * StepX;
            return (x, yLine);
        }
        #endregion

        #region ExecuteStep (switch-case)
        protected override int ExecuteStep(int current, System.Threading.CancellationToken ct)
        {
            if (_pauseRequested) return current; // 일시정지 시 현재 step 유지
            var before = _step;
            switch (_step)
            {
                case Step.Idle:
                    return -1;

                case Step.Init:
                    _camReady = IsDryRun ? true : (EnsureCameraReadyFunc?.Invoke() ?? false);
                    if (!_camReady) { GoError("Camera not ready", AlarmKey.CameraNotReady); break; }
                    _step = Step.Prepare; _tick = DateTime.UtcNow; break;

                case Step.Prepare:
                    // 조명 세팅 (멀티 서치 조명 사용 가정)
                    if (!IsDryRun) SetLightingForMultiAction?.Invoke();
                    _step = Step.MoveScanStart; _tick = DateTime.UtcNow; break;

                case Step.MoveScanStart:
                    {
                        var p = CurrentScanStageTarget();
                        // 첫 위치 이동
                        if (!IsDryRun)
                        {
                            _stage.MoveAxisOnce(_stage.AxisX, p.X);
                            _stage.MoveAxisOnce(_stage.AxisY, p.Y);
                        }
                        _step = Step.MoveScanStart_Check; _tick = DateTime.UtcNow;
                    }
                    break;

                case Step.MoveScanStart_Check:
                    {
                        var p = CurrentScanStageTarget();
                        if (IsDryRun || (_stage.InPos(_stage.AxisX, p.X) && _stage.InPos(_stage.AxisY, p.Y)))
                        { _grabDone = false; _searchDone = false; _step = Step.Scan_Grab; _tick = DateTime.UtcNow; }
                        else if (Timeout(MoveTimeoutMs)) GoError("Move start timeout", AlarmKey.MoveTimeout);
                    }
                    break;

                case Step.Scan_Grab:
                    if (IsDryRun)
                    { _grabDone = true; _step = Step.Scan_Search; _tick = DateTime.UtcNow; break; }
                    if (!_grabDone)
                    {
                        if (GrabImageFunc?.Invoke() ?? true)
                        { _grabDone = true; _step = Step.Scan_Search; _tick = DateTime.UtcNow; }
                        else if (Timeout(GrabTimeoutMs)) GoError("Grab timeout", AlarmKey.GrabTimeout);
                    }
                    break;

                case Step.Scan_Search:
                    if (IsDryRun)
                    {
                        // 가짜 칩 1~2개 생성 (랜덤)
                        var basePos = CurrentScanStageTarget();
                        _rawChipAbs.Add((basePos.X + 0.1, basePos.Y + 0.1));
                        if ((_scanIndexX + _scanIndexY) % 3 == 0)
                            _rawChipAbs.Add((basePos.X + 0.2, basePos.Y + 0.05));
                        _searchDone = true; _step = Step.Scan_Record; _tick = DateTime.UtcNow; break;
                    }
                    if (_searchDone) { _step = Step.Scan_Record; break; }
                    if (FindChipsFunc != null)
                    {
                        var rc = FindChipsFunc();
                        if (rc.ok)
                        {
                            var basePos = CurrentScanStageTarget();
                            if (rc.chips != null)
                            {
                                foreach (var c in rc.chips)
                                {
                                    _rawChipAbs.Add((basePos.X + c.x, basePos.Y + c.y));
                                }
                            }
                            _searchDone = true; _step = Step.Scan_Record; _tick = DateTime.UtcNow;
                        }
                        else
                        {
                            // 검색 실패 → 비어있는 셀로 처리 (칩 없음)
                            _searchDone = true; _step = Step.Scan_Record; _tick = DateTime.UtcNow;
                        }
                    }
                    else
                    {
                        GoError("FindChipsFunc not set", AlarmKey.SearchFail);
                    }
                    break;

                case Step.Scan_Record:
                    // 이미 _rawChipAbs 에 누적됨 → 다음 스캔 위치 계산으로 진행
                    _step = Step.Scan_Next_Calc; _tick = DateTime.UtcNow; break;

                case Step.Scan_Next_Calc:
                    // 다음 인덱스 계산
                    _scanIndexX++;
                    if (_scanIndexX >= _totalStepsX)
                    {
                        _scanIndexX = 0;
                        _scanIndexY++;
                    }
                    if (_scanIndexY >= _totalStepsY)
                    {
                        // 스캔 종료 → 매핑 계산 단계로
                        _step = Step.ComputeMapping; _tick = DateTime.UtcNow; break;
                    }
                    // 다음 위치 이동 준비
                    _grabDone = false; _searchDone = false;
                    var nextPos = CurrentScanStageTarget();
                    if (!IsDryRun)
                    {
                        _stage.MoveAxisOnce(_stage.AxisX, nextPos.X);
                        _stage.MoveAxisOnce(_stage.AxisY, nextPos.Y);
                    }
                    _step = Step.Scan_Next_Move; _tick = DateTime.UtcNow; break;

                case Step.Scan_Next_Move:
                    _step = Step.Scan_Next_Move_Check; _tick = DateTime.UtcNow; break;

                case Step.Scan_Next_Move_Check:
                    {
                        var p = CurrentScanStageTarget();
                        if (IsDryRun || (_stage.InPos(_stage.AxisX, p.X) && _stage.InPos(_stage.AxisY, p.Y)))
                        { _step = Step.Scan_Grab; _tick = DateTime.UtcNow; }
                        else if (Timeout(MoveTimeoutMs)) GoError("Step move timeout", AlarmKey.MoveTimeout);
                    }
                    break;

                case Step.ComputeMapping:
                    if (!ComputeMappingData())
                    {
                        GoError("Mapping compute failed", AlarmKey.MappingFail);
                    }
                    else
                    {
                        _step = Step.Finish; _tick = DateTime.UtcNow;
                    }
                    break;

                case Step.Finish:
                    return -1;
                case Step.Error:
                    return -1;
            }

            // SoftStop 요청 처리: 안전 Idle 전환
            if (_softStopRequested && _step != Step.Idle && _step != Step.Finish && _step != Step.Error)
            {
                _step = Step.Idle;
                return -1;
            }

            // Step 변경 로그
            if (before != _step && _prevLoggedStep != _step)
            {
                try { Log.Write(Name, "Step", Name, "StepChange: " + StepToCode(before) + " -> " + StepToCode(_step)); } catch { }
                _prevLoggedStep = _step;
            }
            return current + 1;
        }
        #endregion

        #region 매핑 계산 로직
        private bool ComputeMappingData()
        {
            try
            {
                if (_rawChipAbs.Count == 0)
                {
                    Log.Write(Name, "Mapping", Name, "No chips detected – empty map");
                    return false;
                }
                // 중심 칩: 평균 또는 중앙값 사용
                double cx = _rawChipAbs.Average(p => p.X);
                double cy = _rawChipAbs.Average(p => p.Y);
                // 가장 가까운 칩을 중심으로
                double bestDist = double.MaxValue; (double X, double Y) center = _rawChipAbs[0];
                foreach (var c in _rawChipAbs)
                {
                    double dx = c.X - cx; double dy = c.Y - cy; double d2 = dx * dx + dy * dy;
                    if (d2 < bestDist) { bestDist = d2; center = c; }
                }
                _centerChip = center;

                // Pitch X,Y 계산
                PitchX = EstimatePitch(_rawChipAbs.Select(p => p.X).ToList());
                PitchY = EstimatePitch(_rawChipAbs.Select(p => p.Y).ToList());
                if (PitchX <= 0 || PitchY <= 0)
                {
                    Log.Write(Name, "Mapping", Name, $"Pitch compute failed (PitchX={PitchX}, PitchY={PitchY})");
                    return false;
                }
                double tolX = PitchX * MissingChipPosTolerance;
                double tolY = PitchY * MissingChipPosTolerance;

                // 칩 → Grid (Col,Row) 매핑 (중심 칩 = (0,0))
                var mappedCells = new Dictionary<(int, int), ChipCell>();
                foreach (var c in _rawChipAbs)
                {
                    double rx = c.X - _centerChip.X; double ry = c.Y - _centerChip.Y;
                    int col = (int)Math.Round(rx / PitchX);
                    int row = (int)Math.Round(ry / PitchY);
                    // 오차 검증
                    double ex = rx - col * PitchX; double ey = ry - row * PitchY;
                    if (Math.Abs(ex) > tolX || Math.Abs(ey) > tolY)
                    {
                        // 격자에서 벗어난 outlier → 무시 (로그만)
                        continue;
                    }
                    var key = (col, row);
                    if (!mappedCells.ContainsKey(key))
                    {
                        mappedCells[key] = new ChipCell { Col = col, Row = row, Present = true, AbsX = c.X, AbsY = c.Y };
                    }
                }
                if (!mappedCells.Any()) return false;
                MinCol = mappedCells.Keys.Min(k => k.Item1); MaxCol = mappedCells.Keys.Max(k => k.Item1);
                MinRow = mappedCells.Keys.Min(k => k.Item2); MaxRow = mappedCells.Keys.Max(k => k.Item2);

                // 누락 칩 셀 채우기 (Present=false)
                for (int r = MinRow; r <= MaxRow; r++)
                {
                    for (int ccol = MinCol; ccol <= MaxCol; ccol++)
                    {
                        var k = (ccol, r);
                        if (!mappedCells.ContainsKey(k))
                        {
                            // 추정 절대 위치 (필요 시 기록) : center + col*PitchX, row*PitchY
                            double ax = _centerChip.X + ccol * PitchX;
                            double ay = _centerChip.Y + r * PitchY;
                            mappedCells[k] = new ChipCell { Col = ccol, Row = r, Present = false, AbsX = ax, AbsY = ay };
                        }
                    }
                }
                ChipMap = mappedCells;
                Log.Write(Name, "Mapping", Name, $"Mapping Complete: Col[{MinCol}..{MaxCol}] Row[{MinRow}..{MaxRow}] PitchX={PitchX:F4} PitchY={PitchY:F4} ChipsRaw={_rawChipAbs.Count} Cells={ChipMap.Count}");
                return true;
            }
            catch (Exception ex)
            {
                Log.Write(Name, "Mapping", Name, "ComputeMapping exception: " + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// 좌표 리스트로부터 Pitch(격자 간격) 추정:
        ///  1) 고유 X 값 정렬 → 인접 차이들 수집 (diff>epsilon)
        ///  2) 차이 리스트 Trim (양끝 비율 PitchDetectTrimRatio)
        ///  3) 나머지 중 중앙값(Median) 반환
        /// 실패 시 -1
        /// </summary>
        private double EstimatePitch(List<double> coords)
        {
            if (coords == null || coords.Count < 2) return -1;
            var uniq = coords.Distinct().OrderBy(v => v).ToList();
            if (uniq.Count < 2) return -1;
            double epsilon = 1e-6;
            var diffs = new List<double>();
            for (int i = 1; i < uniq.Count; i++)
            {
                double d = uniq[i] - uniq[i - 1];
                if (d > epsilon) diffs.Add(d);
            }
            if (diffs.Count == 0) return -1;
            diffs.Sort();
            int trim = (int)Math.Floor(diffs.Count * PitchDetectTrimRatio);
            if (trim * 2 >= diffs.Count) trim = 0; // 과도 Trim 방지
            var core = diffs.GetRange(trim, diffs.Count - trim * 2);
            if (core.Count == 0) core = diffs; // fallback
            // 중앙값
            int mid = core.Count / 2;
            double pitch = (core.Count % 2 == 1) ? core[mid] : (core[mid - 1] + core[mid]) / 2.0;
            return pitch;
        }
        #endregion
    }
}
