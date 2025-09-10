using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace QMC.Common.Motions
{
    public sealed class HomeSequence
    {
        private readonly MotionAxisManager _manager;
        private readonly List<List<MotionAxis>> _steps = new List<List<MotionAxis>>();

        // 스텝 시작 전 인터락 훅(기존)
        public Func<int, IReadOnlyList<MotionAxis>, CancellationToken, Task<(bool Ok, string Reason)>> PreStepInterlockAsync { get; set; }

        // 스텝 종료 후 결과 훅(추가)
        public Func<int, IReadOnlyList<HomeAxisResult>, CancellationToken, Task> PostStepAsync { get; set; }

        // === 중단 상태/사유 노출 ===
        public bool Aborted { get; private set; }
        public string AbortReason { get; private set; }
        public int? AbortStepIndex { get; private set; }

        public HomeSequence(MotionAxisManager manager)
        {
            _manager = manager ?? throw new ArgumentNullException(nameof(manager));
        }

        public HomeSequence AddParallelStep(params MotionAxis[] axes)
        {
            if (axes == null || axes.Length == 0) return this;
            _steps.Add(new List<MotionAxis>(axes));
            return this;
        }

        public HomeSequence AddParallelStep(string unitName, params string[] axisNames)
        {
            if (string.IsNullOrEmpty(unitName) || axisNames == null || axisNames.Length == 0) return this;
            var axes = _manager.GetAxes(unitName, axisNames);
            if (axes != null && axes.Count > 0)
                _steps.Add(new List<MotionAxis>(axes));
            return this;
        }

        public HomeSequence AddParallelStepByAxisNames(params string[] axisNames)
        {
            if (axisNames == null || axisNames.Length == 0) return this;
            var set = new HashSet<string>(axisNames, StringComparer.OrdinalIgnoreCase);
            var picked = _manager.GetAllAxes()?.Where(a => a != null && set.Contains(a.Name)).ToList();
            if (picked != null && picked.Count > 0)
                _steps.Add(picked);
            return this;
        }

        public Task<IReadOnlyList<HomeAxisResult>> HomeAllParallelAsync(CancellationToken token = default(CancellationToken))
        {
            var axes = _manager.GetAllAxes();
            if (axes == null || axes.Count == 0) return Task.FromResult((IReadOnlyList<HomeAxisResult>)Array.Empty<HomeAxisResult>());
            _steps.Clear();
            _steps.Add(new List<MotionAxis>(axes));
            return RunAsync(token);
        }

        public async Task<IReadOnlyList<HomeAxisResult>> HomeAllSequentialAsync(CancellationToken token = default(CancellationToken))
        {
            // 중단 플래그 초기화
            Aborted = false; AbortReason = null; AbortStepIndex = null;

            var axes = _manager.GetAllAxes();
            var results = new List<HomeAxisResult>(axes.Count);
            foreach (var axis in axes)
            {
                if (token.IsCancellationRequested)
                {
                    TryStop(axis);
                    Aborted = true; AbortReason = "Canceled"; // 순차 모드에서도 취소 사유 기록
                    break;
                }

                if (!axis.CheckHomeInterlocks(out var reason))
                {
                    results.Add(HomeAxisResult.NotStarted(axis, reason));
                    continue;
                }

                var res = await HomeOneAsync(axis, token).ConfigureAwait(false);
                results.Add(res);
            }
            return results;
        }

        public async Task<IReadOnlyList<HomeAxisResult>> RunAsync(CancellationToken token = default(CancellationToken))
        {
            // 중단 플래그 초기화
            Aborted = false; AbortReason = null; AbortStepIndex = null;

            var all = new List<HomeAxisResult>();
            for (int stepIndex = 0; stepIndex < _steps.Count; stepIndex++)
            {
                var step = _steps[stepIndex];
                if (step == null || step.Count == 0) continue;
                if (token.IsCancellationRequested) { Aborted = true; AbortReason = "Canceled"; AbortStepIndex = stepIndex; break; }

                // 0) PreStep 인터락 실패 시: 현재 스텝 축들 즉시 정지 + 결과 생성 + 중단
                if (PreStepInterlockAsync != null)
                {
                    var tuple = await PreStepInterlockAsync(stepIndex, step, token).ConfigureAwait(false);
                    if (!tuple.Ok)
                    {
                        foreach (var ax in step) TryStop(ax);
                        for (int i = 0; i < step.Count; i++)
                            all.Add(HomeAxisResult.NotStarted(step[i], tuple.Reason));
                        Aborted = true; AbortReason = $"Step {stepIndex} PreStep failed: {tuple.Reason}"; AbortStepIndex = stepIndex;
                        break;
                    }
                }

                // 1) 축별 사전 체크: 하나라도 CheckHomeInterlocks 실패하면 스텝 전체 중단(요청사항)
                var blockedReasons = new Dictionary<MotionAxis, string>();
                var runnable = new List<MotionAxis>(step.Count);
                foreach (var axis in step)
                {
                    if (!axis.CheckHomeInterlocks(out var reason))
                    {
                        blockedReasons[axis] = reason ?? "CheckHomeInterlocks blocked";
                    }
                    else
                    {
                        runnable.Add(axis);
                    }
                }

                if (blockedReasons.Count > 0)
                {
                    // 스텝 전체 축 정지
                    foreach (var ax in step) TryStop(ax);

                    // 축별 결과 작성: 실패 축은 개별 사유, 나머지는 전체 차단 사유
                    foreach (var ax in step)
                    {
                        string r;
                        if (!blockedReasons.TryGetValue(ax, out r)) r = "Blocked by other axis interlock";
                        all.Add(HomeAxisResult.NotStarted(ax, r));
                    }

                    Aborted = true; AbortReason = $"Step {stepIndex} blocked by CheckHomeInterlocks"; AbortStepIndex = stepIndex;
                    break;
                }

                // 모든 축이 NotStarted로 빠져 실제 실행할 작업이 없다면 스텝 실패로 간주하고 중단
                if (runnable.Count == 0)
                {
                    foreach (var ax in step) TryStop(ax);
                    Aborted = true; AbortReason = $"Step {stepIndex} has no runnable axes (all blocked by interlocks)"; AbortStepIndex = stepIndex;
                    break;
                }

                // 2) 병렬 홈 실행: 하나라도 실패/타임아웃 발생 시 스텝 내 모든 축 즉시 정지 후 결과 출력
                var tasks = new List<Task<HomeAxisResult>>(runnable.Count);
                foreach (var axis in runnable)
                {
                    tasks.Add(HomeOneAsync(axis, token));
                }

                IReadOnlyList<HomeAxisResult> stepResults = null;

                try
                {
                    // 빠른 실패 감지: WhenAny로 먼저 끝난 축이 실패면 즉시 전 축 정지
                    var pending = new List<Task<HomeAxisResult>>(tasks);
                    bool earlyFail = false;
                    string earlyFailReason = null;

                    while (pending.Count > 0)
                    {
                        var finished = await Task.WhenAny(pending).ConfigureAwait(false);
                        pending.Remove(finished);

                        var res = await finished.ConfigureAwait(false);
                        if (!res.Success)
                        {
                            earlyFail = true;
                            earlyFailReason = !string.IsNullOrEmpty(res.FailReason)
                                ? res.FailReason
                                : ("ReturnCode=" + res.ReturnCode);
                            break;
                        }
                    }

                    if (earlyFail)
                    {
                        // 스텝 내 전체 축 즉시 정지
                        foreach (var ax in step) TryStop(ax);

                        // 모든 결과 수집
                        var results = await Task.WhenAll(tasks).ConfigureAwait(false);
                        stepResults = results;
                        all.AddRange(results);

                        Aborted = true; AbortReason = $"Step {stepIndex} failed early: {earlyFailReason}"; AbortStepIndex = stepIndex;

                        // 실패 스텝이라도 PostStep 콜백으로 알림 (선택)
                        if (PostStepAsync != null)
                        {
                            try { await PostStepAsync(stepIndex, results, token).ConfigureAwait(false); } catch { }
                        }
                        break;
                    }
                    else
                    {
                        // 전부 성공 또는 오류 없이 완료
                        var results = await Task.WhenAll(tasks).ConfigureAwait(false);
                        stepResults = results;
                        all.AddRange(results);

                        if (PostStepAsync != null)
                        {
                            try { await PostStepAsync(stepIndex, results, token).ConfigureAwait(false); } catch { }
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    foreach (var axis in step) TryStop(axis);
                    Aborted = true; AbortReason = "Canceled"; AbortStepIndex = stepIndex;
                    break;
                }
            }
            return all;
        }

        private static async Task<HomeAxisResult> HomeOneAsync(MotionAxis axis, CancellationToken token)
        {
            if (axis == null) return new HomeAxisResult(null, -1, new ArgumentNullException("axis"), started: false, failReason: "axis null");

            return await Task.Run(() =>
            {
                try
                {
                    token.ThrowIfCancellationRequested();
                    var rc = axis.HomeSync(); // 내부에서 HomeTimeout 시 Stop/Alarm 처리 포함
                    return new HomeAxisResult(axis, rc, null, started: true, failReason: rc == 0 ? null : "Home failed (rc=" + rc + ")");
                }
                catch (OperationCanceledException)
                {
                    TryStop(axis);
                    throw;
                }
                catch (Exception ex)
                {
                    TryStop(axis);
                    return new HomeAxisResult(axis, -1, ex, started: true, failReason: ex.Message);
                }
            }, token).ConfigureAwait(false);
        }

        private static void TryStop(MotionAxis axis)
        {
            try { axis?.EmgStop(); } catch { /* ignore */ }
        }
    }

    public sealed class HomeAxisResult
    {
        public MotionAxis Axis { get; }
        public string AxisName => Axis != null ? Axis.Name : string.Empty;
        public int ReturnCode { get; }
        public Exception Error { get; }
        public bool Success => Started && Error == null && ReturnCode == 0;
        public bool Started { get; }
        public string FailReason { get; }

        public HomeAxisResult(MotionAxis axis, int returnCode, Exception error, bool started, string failReason)
        {
            Axis = axis;
            ReturnCode = returnCode;
            Error = error;
            Started = started;
            FailReason = failReason;
        }

        public static HomeAxisResult NotStarted(MotionAxis axis, string reason)
        {
            return new HomeAxisResult(axis, returnCode: 0, error: null, started: false, failReason: reason);
        }
    }
}
