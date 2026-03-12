using QMC.Common;
using QMC.Common.PKGTester;
using QMC.Common.Unit;
using QMC.LCP_280.Process.Component;
using QMC.LCP_280.Process.Component.ProcessData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace QMC.LCP_280.Process.Unit.FormWork.Repro
{
    // 폼 비의존, 재현성 시퀀스 전담 Runner
    public sealed class ManualSeqReproTestRunner : IDisposable
    {
        private readonly Rotary _rotary;
        private readonly InputDieTransfer _dieTransfer;
        private readonly InputStage _inputStage;
        private readonly IndexChipProbeController _probeCtrl;
        private readonly IndexLoadAligner _loadAligner;
        private readonly PKGTester _tester;

        private volatile bool _running;
        private CancellationTokenSource _cts;
        private readonly object _ioLock = new object();

        private readonly object _dataLock = new object();
        private MaterialDie[] _socketDies = new MaterialDie[8];   // 각 소켓에 현재 안착된 Die (Placed 상태)

        private string _statePath;
        private string _dataFilePath;
        private StreamWriter _writer;

        private MaterialDie _lastPickedDie;
        private int _nextSocket;          // 1~8 의미, 내부 비교 시 0~7로 변환
        private bool _holdingDieOnArm;    // 암에 다이 보유 중

        // UI로 측정 결과를 전달하는 이벤트(소켓 0-based, 결과 객체)
        public event Action<int, PKGTesterResult> MeasurementCompleted;

        // [추가] 반복/지연/얼라인 옵션
        public int RepeatCycleCount { get; set; } = 1;       // 지정 횟수만큼 8소켓 반복
        public int MeasureDelayMs { get; set; } = 0;         // 검사 간 지연(옵션)
        public bool AlignBeforeLoad { get; set; } = false;    // 소켓 이동 후, 로딩 전에 얼라인 1회

        // UI 연결 없이 바깥으로 신호만 알림
        public event Action<bool> RunningChanged;
        public event Action<string> Message;
        public event Action<int> SocketAdvanced; // 현재 소켓 완료 시 알림 (1~8)


        public bool IsRunning => _running;
        public int NextSocket => _nextSocket;
        public string DataFilePath => _dataFilePath;

        public ManualSeqReproTestRunner(Rotary rotary,
                               InputDieTransfer dieTransfer,
                               InputStage inputStage,
                               IndexChipProbeController probeCtrl,
                               IndexLoadAligner loadAligner,
                               PKGTester tester = null)
        {
            _rotary = rotary ?? throw new ArgumentNullException(nameof(rotary));
            _dieTransfer = dieTransfer ?? throw new ArgumentNullException(nameof(dieTransfer));
            _inputStage = inputStage ?? throw new ArgumentNullException(nameof(inputStage));
            _probeCtrl = probeCtrl ?? throw new ArgumentNullException(nameof(probeCtrl));
            _loadAligner = loadAligner ?? throw new ArgumentNullException(nameof(loadAligner));
            _tester = tester;

            InitStatePath();
            BindEvents();
            LoadState();
        }

        private void InitStatePath()
        {
            try
            {
                var baseDir = AppDomain.CurrentDomain.BaseDirectory;
                var dir = Path.Combine(baseDir, "ReproTest");
                Directory.CreateDirectory(dir);
                _statePath = Path.Combine(dir, "ReproTestRunner.state");
            }
            catch { }
        }

        private void BindEvents()
        {
            try
            {
                _dieTransfer.DiePicked += (s, e) =>
                {
                    try
                    {
                        if (e != null && e.Die != null)
                        {
                            _lastPickedDie = e.Die;
                        }
                        else if (e != null)
                        {
                            _lastPickedDie = new MaterialDie
                            {
                                MapX = (int)Math.Round(e.MapX),
                                MapY = (int)Math.Round(e.MapY),
                                Presence = Material.MaterialPresence.Exist,
                                State = DieProcessState.Picked
                            };
                        }
                    }
                    catch { }
                };

                if (_tester != null)
                {
                    // 필요 시 테스트기 완료 이벤트를 사용하도록 확장 가능
                    _tester.OnMeasureCompleted += _ => { /* 외부 대기 TCS 사용 시 연결 */ };
                }
            }
            catch { }
        }

        public void Start()
        {
            if (_running)
                return;

            _cts?.Cancel();
            _cts = new CancellationTokenSource();
            var ct = _cts.Token;

            _running = true;

            RunningChanged?.Invoke(true);
            SetUnitsManualRunning(true);

            // ← 추가: 매 실행마다 새 파일을 만들도록 이전 경로/Writer 초기화
            lock (_ioLock)
            {
                try { CloseWriter(); } catch { }
                _dataFilePath = null;   // EnsureWriterOpen()에서 새 타임스탬프 파일명 생성
            }
            EnsureDataFile();

            _nextSocket = 0;
            _holdingDieOnArm = false;
            Array.Clear(_socketDies, 0, _socketDies.Length);

            Task.Run(async () =>
            {
                try
                {
                    Log.Write("ReproTest", "Start()", "재현성 테스트 시작");
                    //await EnsureWaferLoadedAsync(ct).ConfigureAwait(false);

                    // [추가] 반복 사이클
                    int runCount = Math.Max(1, RepeatCycleCount);
                    for (int cycle = 1; cycle <= runCount && !ct.IsCancellationRequested; cycle++)
                    {
                        Info($"사이클 {cycle}/{runCount} 시작");

                        while (!ct.IsCancellationRequested)
                        {
                            if (!_running)
                                break;

                            // 소켓 0~7 순차 진행
                            while (_nextSocket < _rotary.GetIndexCount() && !ct.IsCancellationRequested)
                            {
                                int currentSocket = _nextSocket; // 0-based
                                ct.ThrowIfCancellationRequested();
                                Info($"소켓 {currentSocket} 처리 시작");

                                bool bVacuumOK = false;
                                bool bVacuumOn = false;
                                for (int i = 0; i < _rotary.GetIndexCount(); i++)
                                {
                                    bVacuumOK = _rotary.IsVacuumOK(i);
                                    bVacuumOn = _rotary.IsOutVacummOn(i);

                                    if (bVacuumOn)//&& bVacuumOn)
                                    {
                                        int nRet = 0;

                                        //로더 위치로
                                        nRet = await MoveRotaryToSocketAsync(i, ct).ConfigureAwait(false);
                                        if (nRet != 0)
                                        {
                                            Log.Write("ReproTest", $"Rotary 이동 실패 (socket={currentSocket})");
                                            break;
                                        }
                                        currentSocket = i;

                                        // [추가] 로딩 전에 얼라인 1회(옵션)
                                        //if (AlignBeforeLoad)
                                        //{
                                        //    int arc = await AlignCurrentSocketOnceAsync(ct).ConfigureAwait(false);
                                        //    if (arc != 0)
                                        //    {
                                        //        Log.Write("ReproTest", "사전 얼라인 실패");
                                        //        break;
                                        //    }
                                        //}

                                        if (currentSocket < 8)
                                        {
                                            while (true)
                                            {
                                                if (_rotary.IsIndexMoving() == false)
                                                {
                                                    int idx = _dieTransfer.GetLoadIndexNo(); // 0~7
                                                    if (currentSocket == idx)
                                                    {
                                                        break;
                                                    }
                                                    else
                                                    {
                                                        Log.Write(_tester, $"현재 소켓:{currentSocket}, 다이전달기 로드 인덱스:{idx} 불일치, 재대기");
                                                        break;
                                                    }
                                                }
                                                if (_rotary.IsStop)
                                                {
                                                    break;
                                                }
                                                Thread.Sleep(1);
                                            }

                                            nRet = await PickDieFromCurrentSocketAsync(ct).ConfigureAwait(false);
                                            if (nRet != 0)
                                            {
                                                Log.Write("ReproTest", "다음 소켓 이송을 위한 픽 실패");
                                                break;
                                            }

                                            var pickedAgain = PickupDieFromSocket(currentSocket);
                                            if (pickedAgain == null)
                                            {
                                                Log.Write("ReproTest", "PickupDieFromSocket 반환 null");
                                                break;
                                            }
                                            // [보강] 다음 사이클 시작 시 GetCurrentArmDie() null 방지
                                            _dieTransfer.SetMaterial(pickedAgain);
                                            _holdingDieOnArm = true;
                                            currentSocket = 0;
                                        }
                                    }
                                }

                                // 로터리 Load 위치를 현재 소켓으로 이동 (0 기반 사용)
                                int rc = await MoveRotaryToSocketAsync(currentSocket, ct).ConfigureAwait(false);
                                if (rc != 0)
                                {
                                    Log.Write("ReproTest", $"Rotary 이동 실패 (socket={currentSocket})");
                                    break;
                                }

                                // 암에 현재 다이 참조 (이전 소켓 픽 완료 후 들어있어야 함)
                                var armDie = GetCurrentArmDie();
                                while (true)
                                {
                                    if (_rotary.IsIndexMoving() == false)
                                    {
                                        break;
                                    }
                                    if (_rotary.IsStop)
                                    {
                                        return;
                                    }
                                    Thread.Sleep(1);
                                }

                                // 1) 암에 다이가 없으면 웨이퍼에서 새 다이 픽 → 현재 소켓에 배치
                                if (_holdingDieOnArm == false)
                                {
                                    rc = await PickDieFromWaferAsync(ct).ConfigureAwait(false);
                                    if (rc != 0)
                                    {
                                        Log.Write("ReproTest", "웨이퍼에서 다이 픽 실패");
                                        break;
                                    }

                                    armDie = GetCurrentArmDie();
                                    if (armDie == null)
                                    {
                                        Log.Write("ReproTest", "암 보유 Die null");
                                        break;
                                    }

                                    rc = await PlaceDieFromArmToCurrentSocketAsync(ct).ConfigureAwait(false);
                                    if (rc != 0)
                                    {
                                        Log.Write("ReproTest", "소켓 안착 실패");
                                        break;
                                    }

                                    // 데이터/Material 이동 (DieTransfer -> Rotary)
                                    PlaceDieToSocket(currentSocket, armDie);
                                    _holdingDieOnArm = false;
                                }
                                else
                                {
                                    // 2) 이전 소켓에서 픽해온 동일 다이 재배치
                                    armDie = GetCurrentArmDie();
                                    if (armDie == null)
                                    {
                                        Log.Write("ReproTest", "암 보유 Die null(재배치 단계)");
                                        break;
                                    }

                                    rc = await PlaceDieFromArmToCurrentSocketAsync(ct).ConfigureAwait(false);
                                    if (rc != 0)
                                    {
                                        Log.Write("ReproTest", "소켓 안착 실패(재배치)");
                                        break;
                                    }

                                    PlaceDieToSocket(currentSocket, armDie);
                                    _holdingDieOnArm = false;
                                }

                                // 3) 얼라인 위치 이동(1 step) → 얼라인 수행
                                rc = await MoveRotaryStepsAsync(1, ct).ConfigureAwait(false);
                                if (rc != 0)
                                {
                                    Log.Write("ReproTest", "로딩 얼라인 위치 이동 실패");
                                    break;
                                }

                                rc = await RunLoadAlignerAsync(ct).ConfigureAwait(false);
                                if (rc != 0)
                                {
                                    Log.Write("ReproTest", "M-Align 실패");
                                    break;
                                }

                                // 4) 프로브 위치 이동(1 step) → 검사
                                rc = await MoveRotaryStepsAsync(1, ct).ConfigureAwait(false);
                                if (rc != 0)
                                {
                                    Log.Write("ReproTest", "프로브 위치 이동 실패");
                                    break;
                                }

                                rc = await RunProbeAsyncAndLog(currentSocket, ct).ConfigureAwait(false);
                                if (rc != 0)
                                {
                                    Log.Write("ReproTest", "프로브 수행 실패");
                                    break;
                                }
                                if (_tester != null)
                                {
                                    if(true)
                                    {

                                    }

                                    MarkDieInspected(currentSocket, _tester.Result);
                                }

                                // [추가] 측정 간 지연(옵션)
                                if (MeasureDelayMs > 0)
                                {
                                    await Task.Delay(MeasureDelayMs, ct).ConfigureAwait(false);
                                }

                                // 5) Load 위치 복귀(6 step)
                                rc = await MoveRotaryStepsAsync(6, ct).ConfigureAwait(false);
                                if (rc != 0)
                                {
                                    Log.Write("ReproTest", "로딩 위치 복귀 실패");
                                    break;
                                }

                                // 6) 다음 소켓을 위해 현재 소켓에서 다이 픽 (마지막 소켓(7)은 픽하지 않음)
                                if (currentSocket < 7)
                                {
                                    while (true)
                                    {
                                        if (_rotary.IsIndexMoving() == false)
                                        {
                                            int idx = _dieTransfer.GetLoadIndexNo(); // 0~7
                                            if (currentSocket == idx)
                                            {
                                                break;
                                            }
                                            else
                                            {
                                                Log.Write(_tester, $"현재 소켓:{currentSocket}, 다이전달기 로드 인덱스:{idx} 불일치, 재대기");
                                                return;
                                            }
                                        }
                                        if (_rotary.IsStop)
                                        {
                                            return;
                                        }
                                        Thread.Sleep(1);
                                    }

                                    rc = await PickDieFromCurrentSocketAsync(ct).ConfigureAwait(false);
                                    if (rc != 0)
                                    {
                                        Log.Write("ReproTest", "다음 소켓 이송을 위한 픽 실패");
                                        break;
                                    }

                                    var pickedAgain = PickupDieFromSocket(currentSocket);
                                    if (pickedAgain == null)
                                    {
                                        Log.Write("ReproTest", "PickupDieFromSocket 반환 null");
                                        break;
                                    }
                                    // [보강] 다음 사이클 시작 시 GetCurrentArmDie() null 방지
                                    _dieTransfer.SetMaterial(pickedAgain);
                                    _holdingDieOnArm = true;
                                }
                                else
                                {
                                    // [변경] 마지막 소켓 종료 시: 칩을 원래 웨이퍼 위치로 되돌려 놓기
                                    var lastDie = GetCurrentArmDie(); // 마지막 소켓에서 검사 후 로딩 위치 복귀 상태

                                    // 로터리 Load 인덱스와 다이전달기 인덱스 일치 대기(최대 타임아웃)
                                    const int timeoutMs = 3000;
                                    var sw = System.Diagnostics.Stopwatch.StartNew();
                                    while (_rotary.IsIndexMoving())
                                    {
                                        if (_rotary.IsStop || ct.IsCancellationRequested || sw.ElapsedMilliseconds > timeoutMs) break;
                                        Thread.Sleep(2);
                                    }

                                    if (lastDie != null)
                                    {
                                        // 마지막 소켓에서 픽하지 않았으면 소켓에서 픽해서 들고간다.
                                        // 걍 가지고 와야지?
                                        rc = await PickDieFromCurrentSocketAsync(ct).ConfigureAwait(false);
                                        if (rc == 0)
                                        {
                                            lastDie = PickupDieFromSocket(currentSocket);
                                            if (lastDie != null)
                                                _dieTransfer.SetMaterial(lastDie);
                                        }
                                    }

                                    if (lastDie != null)
                                    {
                                        int prc = await ReturnDieToWaferOriginalAsync(lastDie, ct).ConfigureAwait(false);
                                        if (prc != 0)
                                        {
                                            Log.Write("ReproTest", "마지막 다이 웨이퍼 복귀 실패");
                                        }
                                    }
                                    //_holdingDieOnArm = false; // 마지막 소켓은 회수 없이 종료

                                    // [중요] 로터리 소켓 상태/IO를 안전 상태로 리셋
                                    try
                                    {
                                        var s = _rotary.GetSocket(currentSocket);
                                        if (s != null)
                                        {
                                            s.SetMaterialDie(null);
                                            s.SetState(Rotary.RotarySocketState.Empty);
                                        }
                                        // IO 안전화
                                        _rotary.SetVacuum(currentSocket, false);
                                        _rotary.SetVent(currentSocket, false);
                                        _rotary.SetBlow(currentSocket, false);
                                    }
                                    catch (Exception ex)
                                    {
                                        Log.Write("ReproTest", $"[LastSocketReset] Rotary 소켓/IO 리셋 실패: {ex.Message}");
                                    }

                                    // 암 상태/플래그 초기화
                                    try { _dieTransfer.SetMaterial(null); } catch { }
                                    _holdingDieOnArm = false;

                                    // [권장] 유닛 안전 위치로
                                    try { _dieTransfer.MovePositionSafetyZ(); } catch { }
                                    try { _dieTransfer.MovePositionReady(); } catch { }
                                }

                                _nextSocket = currentSocket + 1;
                                SaveState();

                                //이거 우선 막아보자.
                                //SocketAdvanced?.Invoke(_nextSocket); // 1~8로 보고 싶으면 +1 처리

                                Info($"소켓 {currentSocket} 완료");
                            }

                            // 사이클 종료 (8개 완료)
                            Info("8개 소켓 완료 → 다음 다이 준비");
                            WriteMeasurementBlockAndReset(cycle); // ← 추가
                            PrepareNextDieForNextCycle();

                            _nextSocket = 0;
                            _holdingDieOnArm = false;

                            // 반복 사이클 중이면 루프 계속, 마지막 사이클이면 종료
                            break;
                        }

                        Info($"사이클 {cycle}/{runCount} 종료");
                    }

                    SetUnitsManualRunning(false);
                    _running = false;
                    CloseWriter();
                    Info("재현성 테스트 종료");
                }
                catch (OperationCanceledException)
                {
                    Info("재현성 테스트 중지 요청 수신");
                    SetUnitsManualRunning(false);
                    _running = false;
                    CloseWriter();
                    // [FIX] 정상종료/예외/취소 포함 "종료" 시점에 UI에 반드시 알림
                    try { RunningChanged?.Invoke(false); } catch { }
                }
                catch (Exception ex)
                {
                    Log.Write(ex);
                    SetUnitsManualRunning(false);
                    _running = false;
                    CloseWriter();
                    // [FIX] 정상종료/예외/취소 포함 "종료" 시점에 UI에 반드시 알림
                    try { RunningChanged?.Invoke(false); } catch { }
                }
                finally
                {
                    SetUnitsManualRunning(false);
                    _running = false;
                    CloseWriter();
                    // [FIX] 정상종료/예외/취소 포함 "종료" 시점에 UI에 반드시 알림
                    try { RunningChanged?.Invoke(false); } catch { }
                }
            }, ct);
        }

        public void Stop()
        {
            if (!_running) 
                return;

            _running = false;
            try { _cts?.Cancel(); } catch { }
        }

        public void Reset()
        {
            Stop();
            lock (_ioLock)
            {
                _nextSocket = 0;
                _dataFilePath = null;
                CloseWriter();
                SaveState();
            }
            Info("재현성 테스트 리셋");
        }

        private void SetUnitsManualRunning(bool running)
        {
            try
            {
                if(running)
                {
                    _rotary.StartManual();
                    _dieTransfer.StartManual();
                    _inputStage.StartManual();
                    _probeCtrl.StartManual();
                    _loadAligner.StartManual();
                }
                else
                {
                    _rotary.Stop();
                    _dieTransfer.Stop();
                    _inputStage.Stop();
                    _probeCtrl.Stop();
                    _loadAligner.Stop();
                }
            }
            catch { }
        }

        private void LoadState()
        {
            try
            {
                if (!File.Exists(_statePath))
                {
                    _nextSocket = 0;
                    return;
                }

                var lines = File.ReadAllLines(_statePath, Encoding.UTF8);
                foreach (var l in lines)
                {
                    var kv = l.Split(new[] { '=' }, 2);
                    if (kv.Length != 2) continue;
                    var k = kv[0].Trim();
                    var v = kv[1].Trim();

                    if (string.Equals(k, "NextSocket", StringComparison.OrdinalIgnoreCase))
                    {
                        int tmp;
                        if (int.TryParse(v, out tmp) && tmp >= 1 && tmp <= 8)
                            _nextSocket = tmp;
                    }
                    else if (string.Equals(k, "DataFilePath", StringComparison.OrdinalIgnoreCase))
                    {
                        _dataFilePath = v;
                    }
                }
            }
            catch
            {
                _nextSocket = 0;
            }
        }

        private void SaveState()
        {
            try
            {
                var sb = new StringBuilder();
                sb.AppendLine("NextSocket=" + _nextSocket);
                sb.AppendLine("DataFilePath=" + (_dataFilePath ?? ""));
                File.WriteAllText(_statePath, sb.ToString(), Encoding.UTF8);
            }
            catch { }
        }

        //private void EnsureDataFile()
        //{
        //    lock (_ioLock)
        //    {
        //        var baseDir = AppDomain.CurrentDomain.BaseDirectory;
        //        var dir = Path.Combine(baseDir, "ReproTest");
        //        Directory.CreateDirectory(dir);

        //        if (string.IsNullOrEmpty(_dataFilePath) || !File.Exists(_dataFilePath))
        //        {
        //            var fileName = "재현성Test_" + DateTime.Now.ToString("HHmmss") + ".csv";
        //            _dataFilePath = Path.Combine(dir, fileName);
        //            _writer = new StreamWriter(new FileStream(_dataFilePath, FileMode.Create, FileAccess.Write, FileShare.Read), Encoding.UTF8);
        //            _writer.WriteLine("Time,WaferId,DieIndex,MapX,MapY,Socket,BinType,BinNo,BinLabel");
        //            _writer.Flush();
        //        }
        //        else
        //        {
        //            _writer = new StreamWriter(new FileStream(_dataFilePath, FileMode.Append, FileAccess.Write, FileShare.Read), Encoding.UTF8);
        //        }
        //    }
        //}

        private void CloseWriter()
        {
            lock (_ioLock)
            {
                try { _writer?.Flush(); } catch { }
                try { _writer?.Dispose(); } catch { }
                _writer = null;
            }
        }

        //private void AppendResultCsv(int socket, PKGTesterResult result)
        //{
        //    lock (_ioLock)
        //    {
        //        if (_writer == null) return;

        //        try
        //        {
        //            var wafer = _inputStage != null ? _inputStage.GetMaterialWafer() : null;
        //            string waferId = wafer != null ? wafer.WaferId : "N/A";
        //            int dieIndex = _lastPickedDie != null ? _lastPickedDie.Index : -1;
        //            double mapX = _lastPickedDie != null ? _lastPickedDie.MapX : int.MinValue;
        //            double mapY = _lastPickedDie != null ? _lastPickedDie.MapY : int.MinValue;

        //            var bin = result != null ? result.BinningResult : null;
        //            string binType = bin != null ? bin.BinType.ToString() : "Unknown";
        //            int binNo = bin != null ? bin.BinNo : -1;
        //            string binLabel = bin != null ? (bin.BinLabel ?? "") : "";

        //            string line = string.Format("{0:yyyy-MM-dd HH:mm:ss.fff},{1},{2},{3},{4},{5},{6},{7},{8}",
        //                DateTime.Now, waferId, dieIndex, mapX, mapY, socket, binType, binNo, binLabel.Replace(",", " "));
        //            _writer.WriteLine(line);
        //            _writer.Flush();
        //        }
        //        catch (Exception ex)
        //        {
        //            Log.Write("ReproTest", "CSV 기록 실패: " + ex.Message);
        //        }
        //    }
        //}

        // ====== 시퀀스 요소 ======
        private async Task EnsureWaferLoadedAsync(CancellationToken ct)
        {
            var wafer = _inputStage.GetMaterialWafer();
            if (wafer != null && wafer.Presence == Material.MaterialPresence.Exist && _inputStage.ChipMappingDone)
                return;

            // 필요 시 외부에서 웨이퍼 로딩을 완료시킨 후 시작하도록 가정
            await Task.CompletedTask;
        }

        private async Task<int> MoveRotaryToSocketAsync(int targetSocket, CancellationToken ct)
        {
            int targetIdx0 = (targetSocket + 8) % 8; // 0~7
            for (int i = 0; i < 16; i++)
            {
                ct.ThrowIfCancellationRequested();
                int cur = _rotary.GetLoadIndexNo();
                if (cur == targetIdx0)
                    return 0;

                int rc = _rotary.MovePositionRotate();
                if (rc != 0) 
                    return -1;

                rc = _rotary.WaitIndexMoveDone();
                if (rc != 0)
                    return -1;

                await Task.Delay(50, ct).ConfigureAwait(false);
            }
            return -1;
        }

        private async Task<int> MoveRotaryStepsAsync(int stepCount, CancellationToken ct)
        {
            for (int i = 0; i < stepCount; i++)
            {
                ct.ThrowIfCancellationRequested();

                Log.Write(_rotary.UnitName, "MovePositionRotate", "Start");
                int rc = _rotary.MovePositionRotate();
                if (rc != 0)
                { return -1;}

                while (true)
                {
                    if(_rotary.IsStop)
                    {
                        return 0;
                    }

                    if (_rotary.IsIndexMoving() == false)
                    {
                        Thread.Sleep(200);
                        if (_rotary.IsIndexMoving() == false)
                        {
                            break;
                        }
                    }
                    Thread.Sleep(2);
                }
                Log.Write(_rotary.UnitName, "MovePositionRotate", "End");

                await Task.Delay(50, ct).ConfigureAwait(false);
            }
            return 0;
        }

        private async Task<int> PickDieFromWaferAsync(CancellationToken ct)
        {
            return await Task.Run(() =>
            {
                int nRet = 0;

                nRet = _dieTransfer.MovePositionReady();
                if (nRet != 0)
                {
                    Log.Write(_dieTransfer.UnitName, "PickDieFromWaferAsync", "MovePositionReady failed");
                    return -1;
                }

                nRet = _dieTransfer.RecheckDieAndAlign();
                if (nRet != 0)
                {
                    Log.Write(_dieTransfer.UnitName, "PickDieFromWaferAsync", "RecheckDieAndAlign failed");
                    return -1;
                }

                // 여기서 Die 이동. 
                nRet = _dieTransfer.PrepareNextDie();
                if (nRet != 0)
                {
                    Log.Write(_dieTransfer.UnitName, "PickDieFromWaferAsync", "PrepareNextDie failed");
                    return -1;
                }

                nRet = _dieTransfer.RaiseEjectorForPick();
                if (nRet != 0)
                {
                    Log.Write(_dieTransfer.UnitName, "PickDieFromWaferAsync", "RaiseEjectorForPick failed");
                    return -1;
                }

                nRet = _dieTransfer.PickDownDie();
                if (nRet != 0)
                {
                    Log.Write(_dieTransfer.UnitName, "PickDieFromWaferAsync", "ChipPickDown failed");
                    return -1;
                }

                nRet = _dieTransfer.SyncPickUpDie();
                if (nRet != 0)
                {
                    Log.Write(_dieTransfer.UnitName, "PickDieFromWaferAsync", "SyncPickPinUp failed");
                    return -1;
                }

                nRet = _dieTransfer.SyncPickDieRetreat();
                if (nRet != 0)
                {
                    Log.Write(_dieTransfer.UnitName, "PickDieFromWaferAsync", "SyncPickPinRetreat failed");
                    return -1;
                }

                nRet = _dieTransfer.CommitPickedDie();
                if (nRet != 0)
                {
                    Log.Write(_dieTransfer.UnitName, "PickDieFromWaferAsync", "CommitPickedDie failed");
                    return -1;
                }

                return 0;
            }, ct).ConfigureAwait(false);

        }

        private async Task<int> PlaceDieFromArmToCurrentSocketAsync(CancellationToken ct)
        {
            return await Task.Run(() =>
            {
                int rc = 0;
                rc = _dieTransfer.PlaceDie_ToolT(); if (rc != 0) return -1;
                rc = _dieTransfer.PlaceDownDie(); if (rc != 0) return -1;
                rc = _dieTransfer.PlaceUp(); if (rc != 0) return -1; // 암 Off, 로터리 Vac On
                return 0;
            }, ct).ConfigureAwait(false);
        }

        private async Task<int> PickDieFromCurrentSocketAsync(CancellationToken ct)
        {
            return await Task.Run(() =>
            {
                try
                {
                    int idx = _dieTransfer.GetLoadIndexNo(); // 0~7
                    int arm = _dieTransfer.GetInputTrArmIndex();

                    //if (!_dieTransfer.SetVacuum(arm, true, false))
                    //{
                    //    return -1;
                    //}
                    _dieTransfer.SetVacuum(arm, true, false);

                    int rc = _dieTransfer.MovePositionPlace_Index(idx);
                    if (rc != 0)
                    {
                        return -1;
                    }
                    _rotary.SetVacuum(idx, false);
                    Thread.Sleep(1);

                    _rotary.SetVent(idx, true);
                    Thread.Sleep(1);
                    _rotary.SetVent(idx, false);

                    _rotary.SetBlow(idx, true);
                    Thread.Sleep(200);

                    _dieTransfer.MovePositionSafetyZ();
                    _dieTransfer.MovePositionPickUpToolT();

                    Thread.Sleep(50);
                    _rotary.SetBlow(idx, false);

                    return 0;
                }
                catch
                {
                    return -1;
                }
            }, ct).ConfigureAwait(false);
        }

        private async Task<int> RunLoadAlignerAsync(CancellationToken ct)
        {
            return await Task.Run(() =>
            {
                int rc = _loadAligner.RunAlignSocketOnce();
                return rc == 0 ? 0 : -1;
            }, ct).ConfigureAwait(false);
        }

        private void PrepareNextDieForNextCycle()
        {
            try { _dieTransfer.PrepareNextDie(); } catch { }
        }

        private void Info(string msg)
        {
            try { Message?.Invoke(msg); } catch { }
            Log.Write("ReproTest", msg);
        }

        private void Error(string msg)
        {
            try { Message?.Invoke("[ERROR] " + msg); } catch { }
            Log.Write("ReproTest", "[ERROR] " + msg);
        }

        public void Dispose()
        {
            try { Stop(); } catch { }
            try { CloseWriter(); } catch { }
        }

        // 현재 암(DieTransfer)에 보유 중인 다이 조회
        private MaterialDie GetCurrentArmDie()
        {
            var mat = _dieTransfer.GetMaterial() as MaterialDie;
            return mat;
        }

        // 소켓에 다이 배치(상태/Material 이동 포함)
        private void PlaceDieToSocket(int socketIndex, MaterialDie die)
        {
            if (die == null) 
                return;

            lock (_dataLock)
            {
                die.State = DieProcessState.Placed;
                die.ProcessSatate = Material.MaterialProcessSatate.Processing;
                die.Presence = Material.MaterialPresence.Exist;

                _socketDies[socketIndex] = die;

                // 로터리 소켓 정보 직접 갱신
                try
                {
                    var s = _rotary.GetSocket(socketIndex);
                    if (s != null)
                    {
                        s.SetMaterialDie(die);
                        s.SetState(Rotary.RotarySocketState.Loaded);
                    }
                }
                catch (Exception ex)
                {
                    Log.Write("ReproTest", $"[PlaceDieToSocket] Rotary.SocketInfo 갱신 실패: {ex.Message}");
                }

                try
                {
                    // DieTransfer Unit에서 Rotary로 Material 이동
                    _dieTransfer.MoveMaterial(die, _rotary);
                }
                catch (Exception ex)
                {
                    Log.Write("ReproTest", $"[PlaceDieToSocket] MoveMaterial 실패: {ex.Message}");
                }
            }
        }

        // 소켓에서 다시 암으로 다이 픽업(상태/Material 이동 포함)
        private MaterialDie PickupDieFromSocket(int socketIndex)
        {
            lock (_dataLock)
            {
                var die = _socketDies[socketIndex];
                if (die == null)
                {
                    die = new MaterialDie();
                    return die;
                }

                try
                {
                    _rotary.MoveMaterial(die, _dieTransfer);
                    // [중요] BaseUnit 레벨 Material 보장 (Rotary 구현 차이 대응)
                    _dieTransfer.SetMaterial(die);
                }
                catch (Exception ex)
                {
                    Log.Write("ReproTest", $"[PickupDieFromSocket] MoveMaterial 실패: {ex.Message}");
                    return null;
                }

                die.State = DieProcessState.Picked;
                die.ProcessSatate = Material.MaterialProcessSatate.Processing;
                die.Presence = Material.MaterialPresence.Exist;

                _socketDies[socketIndex] = null;

                // 로터리 소켓 정보 직접 클리어
                try
                {
                    var s = _rotary.GetSocket(socketIndex);
                    if (s != null)
                    {
                        s.SetMaterialDie(null);
                        s.SetState(Rotary.RotarySocketState.Empty);
                    }
                }
                catch (Exception ex)
                {
                    Log.Write("ReproTest", $"[PickupDieFromSocket] Rotary.SocketInfo 클리어 실패: {ex.Message}");
                }

                return die;
            }
        }

        // 검사 완료 후 다이 상태 갱신
        private void MarkDieInspected(int socketIndex, PKGTesterResult result)
        {
            lock (_dataLock)
            {
                var die = _socketDies[socketIndex];
                if (die == null) return;
                // 검사 중 → 검사 완료 상태 전환
                die.State = DieProcessState.Inspected;
                die.ProcessSatate = Material.MaterialProcessSatate.Processing;
                // 결과 매핑 (테스터 결과 객체 속성 구조에 따라 필요 시 확장)
                die.TesterResult = result;

                // 로터리 소켓 상태도 검사 완료로 반영
                try
                {
                    var s = _rotary.GetSocket(socketIndex);
                    if (s != null)
                    {
                        s.SetMaterialDie(die);
                        s.SetState(Rotary.RotarySocketState.Probed);
                    }
                }
                catch (Exception ex)
                {
                    Log.Write("ReproTest", $"[MarkDieInspected] Rotary.SocketInfo 갱신 실패: {ex.Message}");
                }
            }
        }

        // [추가] 로딩 전에 얼라인 1회(메뉴얼 호출용 공개 메서드)
        public async Task<int> AlignCurrentSocketOnceAsync(CancellationToken ct = default(CancellationToken))
        {
            // 현재 Load 위치 인덱스와 Align 유닛 상태 기반으로 1회 얼라인
            try
            {
                int idx = _rotary.GetLoadIndexNo(); // 0~7
                // Align 유닛의 안전 위치/준비 이동(필요 시 추가)
                int rc = await RunLoadAlignerAsync(ct).ConfigureAwait(false);
                return rc;
            }
            catch (Exception ex)
            {
                Log.Write("ReproTest", $"AlignCurrentSocketOnceAsync 예외: {ex.Message}");
                return -1;
            }
        }

        // [추가] 마지막 소켓 종료 시 칩을 웨이퍼 원래 위치로 되돌려 놓기
        private async Task<int> ReturnDieToWaferOriginalAsync(MaterialDie die, CancellationToken ct)
        {
            if (die == null) return 0;
            try
            {
                // 1) 웨이퍼 좌표로 이동 후 언로딩(암 → 웨이퍼) - 기계동작 유지
                int mv = _inputStage.MoveStage(die.CenterX, die.CenterY, bFineSpeed: false);
                if (mv != 0) return -1;

                int rc = 0;
                rc = _dieTransfer.ChipPickDownReturn();
                if (rc != 0) return -1;

                rc = _dieTransfer.MovePositionSafetyZ();
                if (rc != 0) return -1;

                // 3) [단순화] 맵 좌표 기준으로 단일 다이만 추가(Add). 기존 항목은 건드리지 않음.
                var wafer = _inputStage.GetMaterialWafer();
                if (wafer == null)
                {
                    wafer = new MaterialWafer();
                    _inputStage.SetMaterial(wafer);
                }
                if (wafer.Dies == null)
                    wafer.Dies = new List<MaterialDie>();

                // 3) [FIX] 기존 Die가 있으면 "추가"가 아니라 "복구/갱신"
                MaterialDie existing = null;
                lock (wafer.Dies)
                {
                    if (die.Index >= 0)
                        existing = wafer.Dies.FirstOrDefault(d => d != null && d.Index == die.Index);

                    if (existing == null)
                        existing = wafer.Dies.FirstOrDefault(d => d != null && d.MapX == die.MapX && d.MapY == die.MapY);

                    if (existing != null)
                    {
                        // 기존 항목 복구: 카운트 증가 X
                        existing.CenterX = die.CenterX;
                        existing.CenterY = die.CenterY;
                        existing.Angle = die.Angle;
                        existing.Presence = Material.MaterialPresence.Exist;

                        // 다시 픽업 가능하도록 상태를 Mapped로 되돌림(기존 흐름과 맞추기)
                        existing.State = DieProcessState.Mapped;
                        existing.ProcessSatate = Material.MaterialProcessSatate.Processing;
                        existing.SourceWaferId = wafer.WaferId;

                        // TesterResult는 원하면 유지/초기화 선택 가능. 여기서는 기존 코드처럼 초기화.
                        existing.TesterResult = null;
                    }
                    else
                    {
                        // 정말로 리스트에 없을 때만 Add (예외 케이스)
                        var addDie = new MaterialDie
                        {
                            Index = (die.Index >= 0) ? die.Index : wafer.Dies.Count,
                            MapX = die.MapX,
                            MapY = die.MapY,
                            CenterX = die.CenterX,
                            CenterY = die.CenterY,
                            Angle = die.Angle,
                            Presence = Material.MaterialPresence.Exist,
                            State = DieProcessState.Mapped,
                            ProcessSatate = Material.MaterialProcessSatate.Processing,
                            TesterResult = null,
                            SourceWaferId = wafer.WaferId
                        };

                        wafer.Dies.Add(addDie);
                    }
                }

                // 4) 웨이퍼 상태/맵 플래그 복구
                wafer.Presence = Material.MaterialPresence.Exist;
                wafer.ProcessSatate = Material.MaterialProcessSatate.Processing;
                _inputStage.ChipMappingDone = true;

                _dieTransfer.SetMaterial(null);
                // 5) UI 반영
                try { _inputStage.UpdateUI(); } catch { }

                return 0;
            }
            catch (Exception ex)
            {
                Log.Write("ReproTest", $"ReturnDieToWaferOriginalAsync 예외: {ex.Message}");
                return -1;
            }
        }



        private MeasurementBlock _measurementBlock = new MeasurementBlock();
        // EnsureDataFile: 헤더는 테이블 블록용으로 교체하지 않고, 파일 시작에 블록 헤더를 한번만 씁니다.
        // 기존 한줄형 로깅을 유지하고 싶으면 그대로 두고, 아래 블록 출력이 별도로 추가됩니다.
        //private void EnsureDataFile()
        //{
        //    lock (_ioLock)
        //    {
        //        var baseDir = AppDomain.CurrentDomain.BaseDirectory;
        //        var dir = Path.Combine(baseDir, "IndexCal");
        //        Directory.CreateDirectory(dir);

        //        if (string.IsNullOrEmpty(_dataFilePath) || !File.Exists(_dataFilePath))
        //        {
        //            // 날짜+시간 포함으로 충돌 방지
        //            var fileName = "IndexCal_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".csv";
        //            _dataFilePath = Path.Combine(dir, fileName);
        //            _writer = new StreamWriter(new FileStream(_dataFilePath, FileMode.Create, FileAccess.Write, FileShare.Read), Encoding.UTF8);
        //            _writer.WriteLine($"# ReproTest CSV (table blocks), Start={DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}");
        //            _writer.Flush();

        //            // 어디에 저장되는지 로그로 안내
        //            Info($"CSV 파일 생성: {_dataFilePath}");
        //        }
        //        else
        //        {
        //            _writer = new StreamWriter(new FileStream(_dataFilePath, FileMode.Append, FileAccess.Write, FileShare.Read), Encoding.UTF8);
        //            Info($"CSV 파일 계속 기록: {_dataFilePath}");
        //        }
        //    }
        //}
        // 1) Writer 보장 헬퍼 추가
        private void EnsureWriterOpen(bool append)
        {
            lock (_ioLock)
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string dir = Path.Combine(baseDir, "IndexCal");
                Directory.CreateDirectory(dir);

                // 파일 경로가 없으면 새로 만든다
                if (string.IsNullOrWhiteSpace(_dataFilePath))
                {
                    _dataFilePath = Path.Combine(dir, "IndexCal_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".csv");
                    append = false; // 새 파일 생성
                }

                // 기존 writer가 닫혀있으면 새로 연다
                if (_writer == null)
                {
                    var mode = (append && File.Exists(_dataFilePath)) ? FileMode.Append : FileMode.Create;
                    _writer = new StreamWriter(new FileStream(_dataFilePath, mode, FileAccess.Write, FileShare.Read), Encoding.UTF8)
                    {
                        AutoFlush = true
                    };

                    // 새 파일에만 헤더/시작 라인 작성
                    if (mode == FileMode.Create)
                    {
                        _writer.WriteLine($"# ReproTest CSV (table blocks), Start={DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}");
                        _writer.Flush();
                        Info($"CSV 파일 생성: {_dataFilePath}");
                        SaveState(); // 경로 즉시 저장
                    }
                    else
                    {
                        Info($"CSV 파일 계속 기록: {_dataFilePath}");
                    }
                }
            }
        }

        // 2) 기존 EnsureDataFile 교체 (시작 시 한 번 호출)
        private void EnsureDataFile()
        {
            // 시작 시에는 새 파일을 우선 생성(동일 경로가 로드되어 있더라도 새로 시작하는 의도가 아니면
            // 필요에 따라 append로 바꿔도 무방)
            EnsureWriterOpen(append: false);
        }

        // 3) 측정 블록 쓰기 전에 항상 라이터 보장
        //private void WriteMeasurementBlockAndReset(int cycleNo)
        //{
        //    lock (_ioLock)
        //    {
        //        // 파일/라이터 보장 (없으면 Append로 열기)
        //        EnsureWriterOpen(append: true);

        //        try
        //        {
        //            _writer.WriteLine("");
        //            _writer.WriteLine($"# Cycle {cycleNo} - Measurement Table Block ({DateTime.Now:yyyy-MM-dd HH:mm:ss.fff})");
        //            _writer.WriteLine("NO,Item,Index1,Index2,Index3,Index4,Index5,Index6,Index7,Index8");
        //            _measurementBlock.WriteRows(_writer);
        //            _writer.Flush();
        //        }
        //        catch (Exception ex)
        //        {
        //            Log.Write("ReproTest", "테이블 블록 CSV 기록 실패: " + ex.Message);
        //        }
        //        finally
        //        {
        //            _measurementBlock.Reset();
        //        }
        //    }
        //}

        // 4) 단일 라인 CSV를 사용할 경우(옵션): 쓰기 전 보장
        private void AppendResultCsv(int socket, PKGTesterResult result)
        {
            lock (_ioLock)
            {
                EnsureWriterOpen(append: true);
                if (_writer == null) return;

                try
                {
                    var wafer = _inputStage != null ? _inputStage.GetMaterialWafer() : null;
                    string waferId = wafer != null ? wafer.WaferId : "N/A";
                    int dieIndex = _lastPickedDie != null ? _lastPickedDie.Index : -1;
                    double mapX = _lastPickedDie != null ? _lastPickedDie.MapX : int.MinValue;
                    double mapY = _lastPickedDie != null ? _lastPickedDie.MapY : int.MinValue;

                    var bin = result != null ? result.BinningResult : null;
                    string binType = bin != null ? bin.BinType.ToString() : "Unknown";
                    int binNo = bin != null ? bin.BinNo : -1;
                    string binLabel = bin != null ? (bin.BinLabel ?? "") : "";

                    string line = string.Format("{0:yyyy-MM-dd HH:mm:ss.fff},{1},{2},{3},{4},{5},{6},{7},{8}",
                        DateTime.Now, waferId, dieIndex, mapX, mapY, socket, binType, binNo, binLabel.Replace(",", " "));
                    _writer.WriteLine(line);
                    _writer.Flush();
                }
                catch (Exception ex)
                {
                    Log.Write("ReproTest", "CSV 기록 실패: " + ex.Message);
                }
            }
        }


        // 검사 후 기록(소켓별 측정값 집계)에 추가: 테이블 블록용 집계
        private async Task<int> RunProbeAsyncAndLog(int socket, CancellationToken ct)
        {
            return await Task.Run(() =>
            {
                int rc = _probeCtrl.RunInspection();
                if (rc != 0)
                    return -1;

                try
                {
                    if (_tester != null)
                    {
                        // 테이블 블록 집계: 해당 소켓에 측정값 반영
                        _measurementBlock.SetSocketMeasures(socket, _tester.Result);
                        
                        // 필요 시 기존 단일 라인 로깅 유지하려면 아래 호출을 남겨둘 수 있음
                        // AppendResultCsv(socket, _tester.Result);
                        
                        // UI 업데이트용 이벤트 송신
                        try { MeasurementCompleted?.Invoke(socket, _tester.Result); } catch { }
                    }
                }
                catch { }

                return 0;
            }, ct).ConfigureAwait(false);
        }

        // 사이클 종료 시 테이블 블록을 CSV에 출력
        // 기존 코드: "8개 소켓 완료 → 다음 다이 준비" 바로 아래에서 호출
        //   PrepareNextDieForNextCycle();
        //   → 아래 추가
        //   WriteMeasurementBlockAndReset(cycle);
        private void WriteMeasurementBlockAndReset(int cycleNo)
        {
            lock (_ioLock)
            {
                if (_writer == null) 
                    return;

                try
                {
                    // 블록 구분 라인
                    _writer.WriteLine($"");
                    _writer.WriteLine($"# Cycle {cycleNo} - Measurement Table Block ({DateTime.Now:yyyy-MM-dd HH:mm:ss.fff})");

                    // 표 헤더
                    _writer.WriteLine("NO,Item,Index1,Index2,Index3,Index4,Index5,Index6,Index7,Index8");

                    // 각 항목(VF/Watt/WD/WP/TOV) 행 출력
                    _measurementBlock.WriteRows(_writer);

                    _writer.Flush();
                }
                catch (Exception ex)
                {
                    Log.Write("ReproTest", "테이블 블록 CSV 기록 실패: " + ex.Message);
                }
                finally
                {
                    _measurementBlock.Reset();
                }
            }
        }

        // 러너 상태/유닛을 새 실행용으로 초기화
        public void ResetForNewRun(bool moveUnits = true)
        {
            try
            {
                // 실행 상태 플래그/토큰 초기화
                _running = false;
                try { _cts?.Cancel(); } catch { }
                _cts = null;

                // 파일/작성기 초기화
                lock (_ioLock)
                {
                    _dataFilePath = null;
                    CloseWriter();
                }

                // 내부 인덱스/상태 초기화
                _nextSocket = 0;         // 0-based
                _holdingDieOnArm = false;
                Array.Clear(_socketDies, 0, _socketDies.Length);
                _lastPickedDie = null;

                // 측정 블록 초기화
                _measurementBlock?.Reset();

                // 외부 상태 파일 초기화
                SaveState();

                // 유닛 초기화 (옵션) // 다른 방식 고민해야함.
                //if (moveUnits)
                //{
                //    try { _rotary?.ResetForNewRun(clearSockets: true, moveIndexToSafe: true); } catch { }
                //    try { _dieTransfer?.StartManual(); /* 필요 시 추가 초기화 */ } catch { }
                //    try { _inputStage?.ResetForNewRun(moveToSafeReady: true, clearOffsets: false, clearStageMaterial: false); } catch { }
                //    try { _probeCtrl?.StartManual(); } catch { }
                //    try { _loadAligner?.StartManual(); } catch { }
                //}

                // 이벤트 알림 (UI 버튼 상태 갱신을 위해)
                try { RunningChanged?.Invoke(false); } catch { }
            }
            catch { /* 최대한 실패 무시하고 다음 실행에 지장 없도록 */ }
        }



        // ===== 측정 블록 집계 클래스 =====
        private sealed class MeasurementBlock
        {
            // 0~7 소켓 위치에 측정값 저장
            private readonly double?[] _vf = new double?[8];
            private readonly double?[] _watt = new double?[8];
            private readonly double?[] _wdwp = new double?[8];
            private readonly double?[] _tov = new double?[8];

            public void Reset()
            {
                Array.Clear(_vf, 0, _vf.Length);
                Array.Clear(_watt, 0, _watt.Length);
                Array.Clear(_wdwp, 0, _wdwp.Length);
                Array.Clear(_tov, 0, _tov.Length);
            }

            public void SetSocketMeasures(int socketIndex0, PKGTesterResult result)
            {
                if (socketIndex0 < 0 || socketIndex0 >= 8 || result == null)
                    return;

                _vf[socketIndex0] = TryGetMeasure(result, new[] { "VF3", "VF", "Vf", "ForwardVoltage" });
                _watt[socketIndex0] = TryGetMeasure(result, new[] { "WATT", "Watt", "Power", "Pwr" });
                _wdwp[socketIndex0] = TryGetMeasure(result, new[] { "WD", "WP", "WD/WP", "WD_WP", "WdWp", "WDWP" });
                _tov[socketIndex0] = TryGetMeasure(result, new[] { "VF1", "VF5", "TOV", "OverVoltage", "TestOV" });
            }

            public void WriteRows(StreamWriter w)
            {
                WriteRow(w, 1, "VF", _vf);
                WriteRow(w, 2, "Watt", _watt);
                WriteRow(w, 3, "WD/WP", _wdwp);
                WriteRow(w, 4, "TOV", _tov);
            }

            private static void WriteRow(StreamWriter w, int no, string item, double?[] vals)
            {
                var sb = new StringBuilder();
                sb.Append(no).Append(',').Append(item);
                for (int i = 0; i < 8; i++)
                {
                    sb.Append(',');
                    sb.Append(FormatCsvNumber(vals[i]));
                }
                w.WriteLine(sb.ToString());
            }

            private static string FormatCsvNumber(double? v)
            {
                if (!v.HasValue) return ""; // 비측정/누락은 공백
                double d = v.Value;
                if (double.IsNaN(d) || double.IsInfinity(d)) return "";
                return d.ToString("G"); // 로케일 독립 간결 표기
            }

            // MeasurementBlock 내부 TryGetMeasure 교체
            private static double? TryGetMeasure(PKGTesterResult result, string[] keys)
            {
                try
                {
                    if (result == null) return null;

                    // 1) 공개 프로퍼티 Items(IDictionary<string, TestItemResult>) 우선
                    var prop = result.GetType().GetProperty("Items", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                    var dict = prop != null ? prop.GetValue(result, null) as System.Collections.Generic.IDictionary<string, TestItemResult> : null;
                    if (dict != null)
                    {
                        foreach (var k in keys)
                        {
                            TestItemResult item;
                            // 1-a) 정확히 일치
                            if (dict.TryGetValue(k, out item) && item != null) return item.Value;
                            // 1-b) 대소문자 무시하여 검색
                            var kv = dict.FirstOrDefault(p => p.Key != null && p.Key.Equals(k, StringComparison.OrdinalIgnoreCase));
                            if (kv.Value != null) return kv.Value.Value;
                        }
                    }

                    // 2) 비공개 필드 'items'도 지원
                    var field = result.GetType().GetField("items", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    var dict2 = field != null ? field.GetValue(result) as System.Collections.Generic.IDictionary<string, TestItemResult> : null;
                    if (dict2 != null)
                    {
                        foreach (var k in keys)
                        {
                            TestItemResult item;
                            if (dict2.TryGetValue(k, out item) && item != null) return item.Value;
                            var kv = dict2.FirstOrDefault(p => p.Key != null && p.Key.Equals(k, StringComparison.OrdinalIgnoreCase));
                            if (kv.Value != null) return kv.Value.Value;
                        }
                    }

                    // 3) 마지막 시도: 동일 이름의 단일 프로퍼티(double 등)
                    foreach (var k in keys)
                    {
                        var pi = result.GetType().GetProperty(k, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                        if (pi != null)
                        {
                            var v = pi.GetValue(result, null);
                            double d;
                            if (v != null && double.TryParse(v.ToString(), out d)) return d;
                        }
                    }
                }
                catch
                {
                    // 안전 처리
                }
                return null;
            }
        }

    }
}