using System;
using System.Threading;
using System.Threading.Tasks;

namespace QMC.Common.Sequence
{
    /* ====================================================================================================
     * Sequence Framework Overview
     * ----------------------------------------------------------------------------------------------------
     * 본 SequenceBase 는 장비/설비 제어 코드에서 반복적으로 필요한 "단계 기반 상태머신"을 공통화하기 위한
     * 최소 Core 추상 클래스이다. 파생 클래스는 ExecuteStep(int currentStep, CancellationToken ct) 하나만
     * 구현하면 되고, 나머지 수명주기(시작/중지/일시정지/에러복구) 제어는 Base 가 담당한다.
     *
     * 1) 실행 모델
     *    - Start() 호출 시 내부 LongRunning Task 생성 → RunLoop() 에서 while 루프.
     *    - 각 loop 마다 ExecuteStep(currentStep) 호출하여 다음 step 번호(int)를 반환받는다.
     *    - 반환값 < 0: Completed 처리, 그 외: 새 step 번호로 진행 (같으면 stay).
     *
     * 2) 상태 (SequenceState)
     *    Idle → Starting → Running → (Paused) → Running → (Stopping) → Stopped/Completed
     *    예외 발생 시 Error 로 진입하며 Recover() 호출 전까지 루프 정지(RunLoop 대기) 상태.
     *
     * 3) 일시정지 / 재개
     *    - Pause(): Running 상태에서 Paused 로 전환하고 _pauseEvent Reset.
     *    - Resume(): Paused → Running, _pauseEvent Set.
     *    - Paused 중에는 ExecuteStep 이 호출되지 않는다.
     *
     * 4) 에러 처리 & 복구
     *    - ExecuteStep 내부 예외 throw 시 ChangeState(Error) → ErrorOccurred 이벤트 발생.
     *    - Recover() 호출 시 Error 상태에서 Running 으로 복귀, 동일 step (혹은 지정 step) 재시도.
     *      (Recover 호출 전까지 _errorWaitEvent.Wait 로 대기) → 런타임 중 안정적인 사용자介入 가능.
     *
     * 5) Thread-safety
     *    - 외부 API (Start / Stop / Pause / Resume / Recover) 는 lock(_sync) 사용.
     *    - Step 변경(Notification) 은 volatile int 대신 lock 후 필드 변경 & 이벤트 발생.
     *    - 파생 클래스에서 공유자원 접근 시 별도 동기화 권장 (Base 는 step 필드와 상태만 보호).
     *
     * 6) Cancellation & Stop
     *    - Stop(): _cts.Cancel() + _stopRequested=true 설정 → 루프 탈출.
     *    - CancellationToken 은 ExecuteStep 에서 긴 동작(대기 / I/O / Sleep) 시 ThrowIfCancellationRequested 로 중단.
     *
     * 7) Sub-Step 패턴 권장 사항
     *    - 복잡한 한 Step 내에서 다수의 비동기/상태전이가 필요할 때 step 을 기하급수적으로 늘리지 말고, 파생 클래스에서
     *      _subStep (지역 필드) 형태의 "미니 상태머신"(switch) 을 두고 같은 step 번호를 반환하여 loop 재호출 하는 패턴 사용.
     *    - 예: Align 단계 내부: 0=시작,1=모션 명령,2=모션 완료 대기,3=비전 검사 … 형식.
     *
     * 8) 성능
     *    - RunLoop 는 바쁜 루프가 아니며 각 ExecuteStep 호출 후 즉시 다음 루프를 도는 구조.
     *    - Polling 기반 Step 은 되도록 Thread.Sleep 또는 외부 신호(Pause, Cancel) 점검 포함.
     *
     * 9) 확장
     *    - ExecuteStep 에서 장비별 이벤트 / 로깅 / Telemetry 를 Insert.
     *    - 이벤트(StateChanged / StepChanged / ErrorOccurred / Completed) 를 구독하여 UI 반영 가능.
     *
     * 10) 사용 예시
     *      class MySeq : SequenceBase {
     *          enum Step { Init=0, Move, WaitSensor, Finish }
     *          protected override int ExecuteStep(int s, CancellationToken ct) {
     *              switch((Step)s){ ... case Step.Finish: return -1; }
     *          }
     *      }
     *      var seq = new MySeq("AxisInit"); seq.Start();
     *
     * ==================================================================================================== */

    /// <summary>
    /// 시퀀스 공통 상태.
    /// </summary>
    public enum SequenceState
    {
        Idle,
        Starting,
        Running,
        Pausing,
        Paused,
        Stopping,
        Stopped,
        Completed,
        Error
    }

    /// <summary>
    /// 공용으로 여러 장비/유닛에서 재사용 가능한 시퀀스 베이스 클래스.
    /// - switch-case 기반 step 실행 (파생 클래스에서 구현)
    /// - Start / Stop / Pause / Resume / Error Recover
    /// - 다중 인스턴스 동시 실행 가능 (각 인스턴스는 자체 Task/Token 보유)
    /// - 예외 발생 시 Error 상태로 전환 후 Recover() 호출 시 재개
    /// - Sub-step (미니 상태머신) 패턴 지원: 같은 step 번호 반환으로 다단계 분해
    /// </summary>
    public abstract class SequenceBase : IDisposable
    {
        #region Events
        /// <summary>
        /// 상태 변경 이벤트 (oldState, newState). UI 갱신 / 로깅 용도.
        /// </summary>
        public event Action<SequenceBase, SequenceState, SequenceState> StateChanged;
        /// <summary>
        /// Step 변경 이벤트. ExecuteStep 반환값이 현재 step 과 다를 때 발생.
        /// </summary>
        public event Action<SequenceBase, int> StepChanged;
        /// <summary>
        /// Error 상태 진입시 발생. Recover 호출까지 RunLoop 정지 (대기) 상태.
        /// </summary>
        public event Action<SequenceBase, Exception> ErrorOccurred;
        /// <summary>
        /// 정상 종료(Completed) 시 발생. Stop 과 구분됨.
        /// </summary>
        public event Action<SequenceBase> Completed;
        #endregion

        #region Fields
        private readonly object _sync = new object();          // 외부 API 동기화
        private CancellationTokenSource _cts;                   // Cancel 제어
        private Task _runTask;                                  // 실행 Task
        private ManualResetEventSlim _pauseEvent = new ManualResetEventSlim(true);  // true(신호)면 진행, false면 대기
        private ManualResetEventSlim _errorWaitEvent = new ManualResetEventSlim(false); // Error 후 Recover()까지 대기
        private volatile bool _stopRequested;                   // Stop 요청 플래그
        private volatile bool _recoverRequested;                // Recover 요청 플래그
        private int _currentStep;                               // 현재 Step (ExecuteStep 인자)
        private SequenceState _state = SequenceState.Idle;
        private bool _disposed;

        private static int _threadNameCounter;                  // 시퀀스 실행 쓰레드 이름 고유 카운터
        #endregion

        #region Properties
        public string Name { get; }
        public SequenceState State { get { lock (_sync) return _state; } }
        public int CurrentStep => _currentStep;
        public bool IsRunning => State == SequenceState.Running;
        public bool IsPaused => State == SequenceState.Paused;
        public bool IsError => State == SequenceState.Error;
        public bool IsCompleted => State == SequenceState.Completed;
        #endregion

        protected SequenceBase(string name)
        {
            Name = name ?? GetType().Name;
        }

        #region Public API
        /// <summary>
        /// 시퀀스 시작. 이미 실행 중이면 false 반환.
        /// </summary>
        public bool Start(int initialStep = 0)
        {
            Task waitTask = null;
            SequenceState snapshot;

            lock (_sync)
            {
                if (_disposed) throw new ObjectDisposedException(Name);

                if (_runTask != null && !_runTask.IsCompleted)
                {
                    snapshot = _state;

                    // 아직 정상 실행 중이면 재시작 불가
                    if (snapshot == SequenceState.Running ||
                        snapshot == SequenceState.Starting ||
                        snapshot == SequenceState.Pausing ||
                        snapshot == SequenceState.Paused)
                    {
                        return false;
                    }

                    // Stopping / Stopped / Completed / Error 이면 잠깐 기다려서 정리 완료 유도
                    waitTask = _runTask;
                }
            }

            // 잠깐 (최대 50ms) 이전 task 종료 대기
            if (waitTask != null)
            {
                try { waitTask.Wait(50); } catch { /* ignore */ }
            }

            lock (_sync)
            {
                // 한 번 더 확인
                if (_runTask != null && !_runTask.IsCompleted)
                    return false;

                _stopRequested = false;
                _recoverRequested = false;
                _pauseEvent.Set();
                _errorWaitEvent.Reset();
                _currentStep = initialStep;
                ChangeState(SequenceState.Starting);
                _cts = new CancellationTokenSource();
                _runTask = Task.Factory.StartNew(() => RunLoop(_cts.Token), TaskCreationOptions.LongRunning);
                return true;
            }
        }

        /// <summary>
        /// 즉시 정지 요청 (Cancel + Stopping 상태). 내부 루프 탈출 후 Stopped.
        /// </summary>
        public void Stop()
        {
            lock (_sync)
            {
                if (_cts == null) return;
                _stopRequested = true;
                ChangeState(SequenceState.Stopping);
                _pauseEvent.Set(); // 혹시 Paused 인 경우 풀기
                _errorWaitEvent.Set(); // Error 대기 중이라면 풀기
                _cts.Cancel();
            }
            try { _runTask?.Wait(); } catch { /* ignore */ }
        }

        /// <summary>
        /// Running -> Paused. (Paused 상태에서만 Resume 가능)
        /// </summary>
        public bool Pause()
        {
            lock (_sync)
            {
                if (State != SequenceState.Running) return false;
                ChangeState(SequenceState.Pausing);
                _pauseEvent.Reset();
                ChangeState(SequenceState.Paused);
                return true;
            }
        }

        /// <summary>
        /// Paused -> Running.
        /// </summary>
        public bool Resume()
        {
            lock (_sync)
            {
                if (State != SequenceState.Paused) return false;
                ChangeState(SequenceState.Running);
                _pauseEvent.Set();
                return true;
            }
        }

        /// <summary>
        /// Error 상태에서 재가동. (같은 step 재시도 기본, goToStep 지정 가능)
        /// </summary>
        public bool Recover(int? goToStep = null)
        {
            lock (_sync)
            {
                if (State != SequenceState.Error) return false;
                if (goToStep.HasValue) _currentStep = goToStep.Value;
                _recoverRequested = true;
                _errorWaitEvent.Set();
                return true;
            }
        }

        /// <summary>
        /// 시퀀스 종료(또는 완료) 대기.
        /// </summary>
        public void Wait(int millisecondsTimeout = -1)
        {
            Task task;
            lock (_sync) task = _runTask;
            task?.Wait(millisecondsTimeout < 0 ? Timeout.Infinite : millisecondsTimeout);
        }
        #endregion

        #region Core Loop
        private void RunLoop(CancellationToken ct)
        {
            var th = Thread.CurrentThread;
            if (th.Name == null)
            {
                try
                {
                    int n = Interlocked.Increment(ref _threadNameCounter);
                    th.Name = $"Seq:{Name}:{n}";
                }
                catch
                {
                    // 무시 (이미 이름 지정된 경우 등)
                }
            }

            try
            {
                ChangeState(SequenceState.Running);
                OnStarted();

                while (true)
                {
                    Thread.Sleep(1);

                    ct.ThrowIfCancellationRequested();
                    if (_stopRequested) break;

                    // Pause 대기
                    _pauseEvent.Wait(ct);
                    if (_stopRequested) break;

                    try
                    {
                        // 파생 클래스 step 실행
                        var next = ExecuteStep(_currentStep, ct);
                        if (next < 0)
                        {

                            // 음수: 완료
                            ChangeState(SequenceState.Completed);
                            OnCompleted();
                            Completed?.Invoke(this);
                            return;
                        }
                        if (next != _currentStep)
                        {
                            _currentStep = next;
                            StepChanged?.Invoke(this, _currentStep);
                        }
                    }
                    catch (OperationCanceledException) { throw; }
                    catch (Exception exStep)
                    {
                        // Step 처리 중 예외 -> Error 상태 전환 & Recover 대기
                        HandleError(exStep);
                        _errorWaitEvent.Wait(ct); // Recover 호출 대기
                        if (_recoverRequested)
                        {
                            _recoverRequested = false;
                            ChangeState(SequenceState.Running);
                            _errorWaitEvent.Reset();
                        }
                    }
                }
            }
            catch (OperationCanceledException) { /* normal cancellation */ }
            catch (Exception ex)
            {
                HandleError(ex);
            }
            finally
            {
                if (State != SequenceState.Completed && State != SequenceState.Error)
                {
                    ChangeState(SequenceState.Stopped);
                }
                OnStopped();
            }
        }
        #endregion

        #region Error Handling
        protected void HandleError(Exception ex)
        {
            ChangeState(SequenceState.Error);
            ErrorOccurred?.Invoke(this, ex);
        }

        // 클래스 내부 (필드 _sync, State, HandleError 이미 존재한다고 가정)
        public bool ForceError(string message, Exception inner = null)
        {
            lock (_sync)
            {
                if (State == SequenceState.Error) 
                    return false;
                var ex = inner ?? new InvalidOperationException(message ?? "Forced sequence error");
                HandleError(ex);         // 기존 Error 처리(상태 = Error, 이벤트 발생 등)
                return true;
            }
        }

        #endregion

        #region Step Helpers (파생 클래스에서 사용)
        /// <summary>
        /// 단순히 현재 step +1 로 이동.
        /// </summary>
        protected void NextStep()
        {
            _currentStep++;
            StepChanged?.Invoke(this, _currentStep);
        }

        /// <summary>
        /// 임의 step 으로 점프 (음수 불가).
        /// </summary>
        protected void JumpStep(int step)
        {
            if (step < 0) throw new ArgumentOutOfRangeException("step");
            _currentStep = step;
            StepChanged?.Invoke(this, _currentStep);
        }

        /// <summary>
        /// 현재 시퀀스를 완료로 표시 (RunLoop 에서 Completed 처리).
        /// </summary>
        protected void Complete()
        {
            _currentStep = -1; // RunLoop 에서 Completed 처리
        }
        #endregion

        #region Virtual Hooks
        protected virtual void OnStarted() { }
        protected virtual void OnStopped() { }
        protected virtual void OnCompleted() { }
        /// <summary>
        /// 실제 한 step 을 수행하고 다음 step 번호를 반환.
        /// - 음수 반환 시 시퀀스 완료 처리.
        /// - 예외 throw 시 Error 상태 → Recover() 후 동일 step 재시도(혹은 goToStep 지정)
        /// - 비동기 하위 단계는 동일 step 반환(서브 스텝 내부 루프) 패턴 사용
        /// </summary>
        protected abstract int ExecuteStep(int currentStep, CancellationToken ct);
        #endregion

        #region State
        private void ChangeState(SequenceState newState)
        {
            SequenceState old;
            lock (_sync)
            {
                if (_state == newState) return;
                old = _state;
                _state = newState;
            }
            StateChanged?.Invoke(this, old, newState);
        }
        #endregion

        #region Dispose
        public void Dispose()
        {
            if (_disposed) return;
            Stop();
            _cts?.Dispose();
            _pauseEvent?.Dispose();
            _errorWaitEvent?.Dispose();
            _disposed = true;
        }
        #endregion
    }

    /// <summary>
    /// 샘플 시퀀스 구현 예시.
    /// - 0: 초기화
    /// - 1: 자재 감지 대기 (폴링)
    /// - 2: 픽업
    /// - 3: 이송
    /// - 4: 플레이스
    /// - 5: 완료 (-1 반환)
    ///   * 5% 확률로 에러 발생 → Error → Recover() 호출 시 동일 step 재시도
    /// </summary>
    public class SamplePickPlaceSequence : SequenceBase
    {
        private enum Step
        {
            Init = 0,
            WaitMaterial,
            Pick,
            Transfer,
            Place,
            Finish
        }

        private readonly Random _rnd = new Random();
        private DateTime _waitStart;

        public SamplePickPlaceSequence(string name = null) : base(name ?? "SamplePickPlace") { }

        protected override void OnStarted() => Console.WriteLine($"[{Name}] Started");
        protected override void OnCompleted() => Console.WriteLine($"[{Name}] Completed");

        protected override int ExecuteStep(int currentStep, CancellationToken ct)
        {
            switch ((Step)currentStep)
            {
                case Step.Init:
                    _waitStart = DateTime.Now;
                    Console.WriteLine($"[{Name}] Init");
                    return (int)Step.WaitMaterial;

                case Step.WaitMaterial:
                    if ((DateTime.Now - _waitStart).TotalSeconds >= 2)
                    {
                        Console.WriteLine($"[{Name}] Material Detected");
                        return (int)Step.Pick;
                    }
                    Thread.Sleep(100); // busy wait 완화
                    return currentStep; // stay

                case Step.Pick:
                    SimulateAction("Pick", 500, 1000, ct);
                    return (int)Step.Transfer;

                case Step.Transfer:
                    SimulateAction("Transfer", 300, 800, ct);
                    return (int)Step.Place;

                case Step.Place:
                    SimulateAction("Place", 400, 900, ct);
                    return (int)Step.Finish;

                case Step.Finish:
                    Console.WriteLine($"[{Name}] Sequence Done");
                    return -1; // complete

                default:
                    throw new InvalidOperationException("Unknown step: " + currentStep);
            }
        }

        private void SimulateAction(string caption, int minMs, int maxMs, CancellationToken ct)
        {
            // 무작위 에러 발생 (테스트 용)
            if (_rnd.Next(0, 100) < 5)
                throw new ApplicationException($"{caption} 동작 중 에러 발생");

            int delay = _rnd.Next(minMs, maxMs);
            Console.WriteLine($"[{Name}] {caption}... ({delay}ms)");
            int elapsed = 0;
            while (elapsed < delay)
            {
                ct.ThrowIfCancellationRequested();
                Thread.Sleep(100);
                elapsed += 100;
            }
        }
    }
}
