using QMC.Common;
using QMC.Common.PKGTester;
using QMC.Common.Unit;
using QMC.LCP_280.Process.Component;
using System;
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

            EnsureDataFile();

            _nextSocket = 0;
            _holdingDieOnArm = false;
            Array.Clear(_socketDies, 0, _socketDies.Length);

            Task.Run(async () =>
            {
                try
                {
                    Log.Write("ReproTest", "Start()", "재현성 테스트 시작");
                    await EnsureWaferLoadedAsync(ct).ConfigureAwait(false);

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
                            for(int i = 0; i < _rotary.GetIndexCount(); i++)
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
                                    // 6) 다음 소켓을 위해 현재 소켓에서 다이 픽 (마지막 소켓(7)은 픽하지 않음)
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
                                                    return;
                                                }
                                            }
                                            if (_rotary.IsStop)
                                            {
                                                return;
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
                            if (!_holdingDieOnArm)
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
                                MarkDieInspected(currentSocket, _tester.Result);

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
                                    if(_rotary.IsIndexMoving() == false)
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
                                _holdingDieOnArm = false; // 마지막 소켓은 회수 없이 종료
                            }

                            _nextSocket = currentSocket + 1;
                            SaveState();
                            SocketAdvanced?.Invoke(_nextSocket); // 1~8로 보고 싶으면 +1 처리
                            Info($"소켓 {currentSocket} 완료");
                        }

                        // 사이클 종료 (8개 완료)
                        Info("8개 소켓 완료 → 다음 다이 준비");
                        PrepareNextDieForNextCycle();

                        // 다음 사이클을 진행하려면 상태 초기화 후 계속, 현재는 1회 실행 후 종료
                        _nextSocket = 0;
                        _holdingDieOnArm = false;

                        SetUnitsManualRunning(false);
                        _running = false;
                        CloseWriter();
                        RunningChanged?.Invoke(false);
                        Info("재현성 테스트 종료");
                        break;
                    }
                }
                catch (OperationCanceledException)
                {
                    Info("재현성 테스트 중지 요청 수신");
                }
                catch (Exception ex)
                {
                    Log.Write(ex);
                }
                finally
                {
                    SetUnitsManualRunning(false);
                    _running = false;
                    CloseWriter();
                    RunningChanged?.Invoke(false);
                }
            }, ct);
        }

        //public void Start()
        //{
        //    if (_running) 
        //        return;

        //    _cts?.Cancel();
        //    _cts = new CancellationTokenSource();
        //    var ct = _cts.Token;

        //    _running = true;
        //    RunningChanged?.Invoke(true);
        //    SetUnitsManualRunning(true);

        //    EnsureDataFile();

        //    _nextSocket = 0;
        //    _holdingDieOnArm = false;

        //    Task.Run(async () =>
        //    {
        //        try
        //        {
        //            Log.Write("ReproTest", "Start()", "재현성 테스트 시작");
        //            await EnsureWaferLoadedAsync(ct).ConfigureAwait(false);

        //            while (ct.IsCancellationRequested == false)
        //            {
        //                if(_running == false)
        //                {
        //                    break;
        //                }

        //                //for (int socket = Math.Max(_nextSocket, 1); socket <= 8; socket++)
        //                for(int socket = 0; socket < 8; socket++)
        //                {
        //                    ct.ThrowIfCancellationRequested();
        //                    Info($"소켓 {_nextSocket} 처리 시작");

        //                    if(_nextSocket > 8)
        //                    {
        //                        Log.Write("ReproTest", "Start()", $"{_nextSocket}, Socket Max:8");
        //                        break;
        //                    }


        //                    int rc = await MoveRotaryToSocketAsync(_nextSocket, ct).ConfigureAwait(false);
        //                    if (rc != 0)
        //                    {
        //                        Log.Write("ReproTest", $"Rotary 이동 실패 (socket={_nextSocket})");
        //                        return;
        //                    }

        //                    // 2) 웨이퍼 → 암 (최초) 또는 이전 소켓 → 암 (이미 암 보유 시 생략)
        //                    var armDie = GetCurrentArmDie();

        //                    //
        //                    if (!_holdingDieOnArm)
        //                    {
        //                        rc = await PickDieFromWaferAsync(ct).ConfigureAwait(false);
        //                        if (rc != 0)
        //                        {
        //                            Log.Write("ReproTest", "웨이퍼에서 다이 픽 실패");
        //                            return;
        //                        }

        //                        armDie = GetCurrentArmDie(); // CommitPickedDie 이후 암 Material
        //                        if (armDie == null)
        //                        {
        //                            Log.Write("ReproTest", "암 보유 Die null");
        //                            break;
        //                        }

        //                        rc = await PlaceDieFromArmToCurrentSocketAsync(ct).ConfigureAwait(false);
        //                        if (rc != 0)
        //                        {
        //                            Log.Write("ReproTest", "소켓 안착 실패");
        //                            return;
        //                        }

        //                        // 데이터 전환 (Placed + Rotary로 Material 이동)
        //                        PlaceDieToSocket(_nextSocket, armDie);
        //                        _holdingDieOnArm = false;
        //                    }
        //                    else
        //                    {
        //                        armDie = GetCurrentArmDie(); // CommitPickedDie 이후 암 Material
        //                        if (armDie == null)
        //                            return;

        //                        rc = await PlaceDieFromArmToCurrentSocketAsync(ct).ConfigureAwait(false);
        //                        if (rc != 0)
        //                        {
        //                            Log.Write("ReproTest", "소켓 안착 실패");
        //                            return;
        //                        }

        //                        // 데이터 전환 (Placed + Rotary로 Material 이동)
        //                        PlaceDieToSocket(_nextSocket, armDie);
        //                        _holdingDieOnArm = false;
        //                    }

        //                    // LoadAlign 위치로 1 step
        //                    rc = await MoveRotaryStepsAsync(1, ct).ConfigureAwait(false);
        //                    if (rc != 0)
        //                    {
        //                        Log.Write("ReproTest", "로딩 얼라인 위치 이동 실패");
        //                        return;
        //                    }

        //                    // LoadAlign 1회 수행
        //                    rc = await RunLoadAlignerAsync(ct).ConfigureAwait(false);
        //                    if (rc != 0)
        //                    {
        //                        Log.Write("ReproTest", "M-Align 실패");
        //                        return;
        //                    }

        //                    // Probe 위치로 1 step
        //                    rc = await MoveRotaryStepsAsync(1, ct).ConfigureAwait(false);
        //                    if (rc != 0)
        //                    {
        //                        Log.Write("ReproTest", "프로브 위치 이동 실패");
        //                        return;
        //                    }

        //                    // Probe 수행(동기 API)
        //                    rc = await RunProbeAsyncAndLog(_nextSocket, ct).ConfigureAwait(false);
        //                    if (rc != 0)
        //                    {
        //                        Log.Write("ReproTest", "프로브 수행 실패");
        //                        return;
        //                    }
        //                    MarkDieInspected(_nextSocket, _tester.Result);

        //                    // Load 위치 복귀 (6 step)
        //                    rc = await MoveRotaryStepsAsync(6, ct).ConfigureAwait(false);
        //                    if (rc != 0)
        //                    {
        //                        Log.Write("ReproTest", "로딩 위치 복귀 실패");
        //                        return;
        //                    }

        //                    // 다음 소켓을 위해 현재 소켓에서 픽(소켓8은 회수 안함)
        //                    if (_nextSocket < 8)
        //                    {
        //                        rc = await PickDieFromCurrentSocketAsync(ct).ConfigureAwait(false);
        //                        if (rc != 0)
        //                        {
        //                            Log.Write("ReproTest", "다음 소켓 이송을 위한 픽 실패");
        //                            return;
        //                        }

        //                        var pickedAgain = PickupDieFromSocket(_nextSocket);
        //                        if (pickedAgain == null) 
        //                            return;

        //                        _holdingDieOnArm = true;
        //                    }
        //                    else
        //                    {
        //                        Array.Clear(_socketDies, 0, _socketDies.Length);
        //                        _holdingDieOnArm = false;
        //                    }

        //                    _nextSocket = _nextSocket + 1;
        //                    SaveState();
        //                    SocketAdvanced?.Invoke(_nextSocket);
        //                    Info($"소켓 {_nextSocket} 완료");
        //                }

        //                Info("8개 소켓 완료 → 다음 다이 준비");
        //                _nextSocket = 0;
        //                SaveState();
        //                PrepareNextDieForNextCycle();

        //                //준비하고 완료.
        //                SetUnitsManualRunning(false);
        //                _running = false;
        //                CloseWriter();
        //                RunningChanged?.Invoke(false);
        //                Info("재현성 테스트 종료");

        //                //
        //            }
        //        }
        //        catch (OperationCanceledException)
        //        {
        //            Info("재현성 테스트 중지 요청 수신");
        //        }
        //        catch (Exception ex)
        //        {
        //            Log.Write(ex);
        //        }
        //        finally
        //        {
        //            SetUnitsManualRunning(false);
        //            _running = false;
        //            CloseWriter();
        //            RunningChanged?.Invoke(false);
        //        }
        //    }, ct);
        //}

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
               

                //_rotary.RunUnitStatus = running ? BaseUnit.UnitStatus.ManualRunning : BaseUnit.UnitStatus.Stopped;
                //_dieTransfer.RunUnitStatus = running ? BaseUnit.UnitStatus.ManualRunning : BaseUnit.UnitStatus.Stopped;
                //_inputStage.RunUnitStatus = running ? BaseUnit.UnitStatus.ManualRunning : BaseUnit.UnitStatus.Stopped;
                //_probeCtrl.RunUnitStatus = running ? BaseUnit.UnitStatus.ManualRunning : BaseUnit.UnitStatus.Stopped;
                //_loadAligner.RunUnitStatus = running ? BaseUnit.UnitStatus.ManualRunning : BaseUnit.UnitStatus.Stopped;
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

        private void EnsureDataFile()
        {
            lock (_ioLock)
            {
                var baseDir = AppDomain.CurrentDomain.BaseDirectory;
                var dir = Path.Combine(baseDir, "ReproTest");
                Directory.CreateDirectory(dir);

                if (string.IsNullOrEmpty(_dataFilePath) || !File.Exists(_dataFilePath))
                {
                    var fileName = "재현성Test_" + DateTime.Now.ToString("HHmmss") + ".csv";
                    _dataFilePath = Path.Combine(dir, fileName);
                    _writer = new StreamWriter(new FileStream(_dataFilePath, FileMode.Create, FileAccess.Write, FileShare.Read), Encoding.UTF8);
                    _writer.WriteLine("Time,WaferId,DieIndex,MapX,MapY,Socket,BinType,BinNo,BinLabel");
                    _writer.Flush();
                }
                else
                {
                    _writer = new StreamWriter(new FileStream(_dataFilePath, FileMode.Append, FileAccess.Write, FileShare.Read), Encoding.UTF8);
                }
            }
        }

        private void CloseWriter()
        {
            lock (_ioLock)
            {
                try { _writer?.Flush(); } catch { }
                try { _writer?.Dispose(); } catch { }
                _writer = null;
            }
        }

        private void AppendResultCsv(int socket, PKGTesterResult result)
        {
            lock (_ioLock)
            {
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

                //rc = _rotary.WaitIndexMoveDone();
                //if (rc != 0)
                //{
                //    return -1;
                //}
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
                    Log.Write(_dieTransfer.UnitName, "PickDieFromWaferAsync", "RecheckDieAndAlign failed");
                    return -1;
                }

                nRet = _dieTransfer.RecheckDieAndAlign();
                if (nRet != 0)
                {
                    Log.Write(_dieTransfer.UnitName, "PickDieFromWaferAsync", "RecheckDieAndAlign failed");
                    return -1;
                }

                nRet = _dieTransfer.PrepareNextDie(); if (nRet != 0) return -1;
                nRet = _dieTransfer.RaiseEjectorForPick(); if (nRet != 0) return -1;
                nRet = _dieTransfer.ChipPickDown(); if (nRet != 0) return -1;
                nRet = _dieTransfer.SyncPickPinUp(); if (nRet != 0) return -1;
                nRet = _dieTransfer.SyncPickPinRetreat(); if (nRet != 0) return -1;
                nRet = _dieTransfer.CommitPickedDie(); if (nRet != 0) return -1;
                return 0;
            }, ct).ConfigureAwait(false);
        }

        private async Task<int> PlaceDieFromArmToCurrentSocketAsync(CancellationToken ct)
        {
            return await Task.Run(() =>
            {
                int rc = 0;
                rc = _dieTransfer.RotateToolTForPlace_AsyncWait(); if (rc != 0) return -1;
                rc = _dieTransfer.PlaceChipDown(); if (rc != 0) return -1;
                rc = _dieTransfer.ReleaseVacuumAndPlaceUp(); if (rc != 0) return -1; // 암 Off, 로터리 Vac On
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

        private async Task<int> RunProbeAsyncAndLog(int socket, CancellationToken ct)
        {
            return await Task.Run(() =>
            {
                int rc = _probeCtrl.RunInspection();
                if (rc != 0) 
                    return -1;

                // 필요 시 _tester.Result 로깅
                try
                {
                    if (_tester != null)
                        AppendResultCsv(socket, _tester.Result);
                }
                catch { }

                return 0;
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
            if (die == null) return;
            lock (_dataLock)
            {
                die.State = DieProcessState.Placed;
                die.ProcessSatate = Material.MaterialProcessSatate.Processing;
                die.Presence = Material.MaterialPresence.Exist;

                _socketDies[socketIndex] = die;
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
                    //return null;
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
            }
        }
    }
}