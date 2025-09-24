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

        // 스텝 종료 후 결과 훅(기존)
        public Func<int, IReadOnlyList<HomeAxisResult>, CancellationToken, Task> PostStepAsync { get; set; }

        // 축 시작 전 인터락 훅(기존)
        public Func<int, MotionAxis, CancellationToken, Task<(bool Ok, string Reason)>> PreAxisInterlockAsync { get; set; }

        // 진행 콜백(추가) : 스텝 시작/종료/완료 시 호출
        private Action<OperationProgress> _progressCallback;
        public HomeSequence OnProgress(Action<OperationProgress> cb) { _progressCallback = cb; return this; }

        private void RaiseProgress(OperationProgress p)
        {
            try { _progressCallback?.Invoke(p); } catch { }
        }

        public int TotalSteps => _steps.Count;

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
            Aborted = false; AbortReason = null; AbortStepIndex = null;
            var axes = _manager.GetAllAxes();
            var results = new List<HomeAxisResult>(axes.Count);
            for (int i = 0; i < axes.Count; i++)
            {
                var axis = axes[i];
                if (token.IsCancellationRequested)
                {
                    TryStop(axis); Aborted = true; AbortReason = "Canceled"; break;
                }
                RaiseProgress(new OperationProgress { OperationId = "HOME", Title = "Home", StepIndex = i, TotalSteps = axes.Count, StepAxisCount = 1, StepName = axis.Name, Message = "Axis PreCheck" });
                if (PreAxisInterlockAsync != null)
                {
                    var pre = await PreAxisInterlockAsync(-1, axis, token).ConfigureAwait(false);
                    if (!pre.Ok)
                    {
                        results.Add(HomeAxisResult.NotStarted(axis, pre.Reason));
                        continue;
                    }
                }
                if (!axis.CheckHomeInterlocks(out var reason))
                {
                    results.Add(HomeAxisResult.NotStarted(axis, reason));
                    continue;
                }
                var res = await HomeOneAsync(axis, token).ConfigureAwait(false);
                results.Add(res);
                RaiseProgress(new OperationProgress { OperationId = "HOME", Title = "Home", StepIndex = i, TotalSteps = axes.Count, StepAxisCount = 1, StepFailCount = res.Success ? 0 : 1, StepName = axis.Name, IsStepCompleted = true });
            }
            RaiseProgress(new OperationProgress { OperationId = "HOME", Title = "Home", StepIndex = axes.Count - 1, TotalSteps = axes.Count, IsCompleted = true, IsCanceled = token.IsCancellationRequested, IsAborted = Aborted, Message = AbortReason });
            return results;
        }

        public async Task<IReadOnlyList<HomeAxisResult>> RunAsync(CancellationToken token = default(CancellationToken))
        {
            Aborted = false;
            AbortReason = null;
            AbortStepIndex = null;

            var all = new List<HomeAxisResult>();

            for (int stepIndex = 0; stepIndex < _steps.Count; stepIndex++)
            {
                var step = _steps[stepIndex];
                if (step == null || step.Count == 0) { continue; }

                if (token.IsCancellationRequested)
                {
                    foreach (var ax in step) { TryStop(ax); }
                    Aborted = true; AbortReason = "Canceled"; AbortStepIndex = stepIndex;
                    RaiseProgress(new OperationProgress
                    {
                        OperationId = "HOME",
                        Title = "Home",
                        StepIndex = stepIndex,
                        TotalSteps = _steps.Count,
                        StepAxisCount = step.Count,
                        StepFailCount = step.Count,
                        StepName = string.Join(", ", step.Select(a => a.Name)),
                        IsStepCompleted = true,
                        IsCanceled = true,
                        Message = "Canceled"
                    });
                    break;
                }

                var progress = new OperationProgress
                {
                    OperationId = "HOME",
                    Title = "Home",
                    StepIndex = stepIndex,
                    TotalSteps = _steps.Count,
                    StepAxisCount = step.Count,
                    StepName = string.Join(", ", step.Select(a => a.Name))
                };
                RaiseProgress(progress);

                if (PreStepInterlockAsync != null)
                {
                    var tuple = await PreStepInterlockAsync(stepIndex, step, token).ConfigureAwait(false);
                    if (!tuple.Ok)
                    {
                        foreach (var ax in step) { TryStop(ax); }
                        for (int i = 0; i < step.Count; i++)
                            all.Add(HomeAxisResult.NotStarted(step[i], tuple.Reason));
                        Aborted = true;
                        AbortReason = $"Step {stepIndex} PreStep failed: {tuple.Reason}";
                        AbortStepIndex = stepIndex;
                        RaiseProgress(new OperationProgress
                        {
                            OperationId = "HOME",
                            Title = "Home",
                            StepIndex = stepIndex,
                            TotalSteps = _steps.Count,
                            StepAxisCount = step.Count,
                            StepFailCount = step.Count,
                            StepName = string.Join(", ", step.Select(a => a.Name)),
                            IsStepCompleted = true,
                            IsAborted = true,
                            Message = tuple.Reason
                        });
                        break;
                    }
                }

                var blockedReasons = new Dictionary<MotionAxis, string>();
                var runnable = new List<MotionAxis>(step.Count);
                bool stepCanceled = false;

                foreach (var axis in step)
                {
                    if (token.IsCancellationRequested)
                    {
                        stepCanceled = true;
                        break;
                    }

                    if (PreAxisInterlockAsync != null)
                    {
                        (bool Ok, string Reason) pre;
                        try
                        {
                            pre = await PreAxisInterlockAsync(stepIndex, axis, token).ConfigureAwait(false);
                        }
                        catch (OperationCanceledException)
                        {
                            stepCanceled = true;
                            break;
                        }

                        if (token.IsCancellationRequested)
                        {
                            stepCanceled = true;
                            break;
                        }

                        if (!pre.Ok)
                        {
                            blockedReasons[axis] = pre.Reason ?? "PreAxisInterlock blocked";
                            continue;
                        }
                    }

                    string reason;
                    bool interlockOk = axis.CheckHomeInterlocks(out reason);
                    if (!interlockOk)
                    {
                        blockedReasons[axis] = reason ?? "CheckHomeInterlocks blocked";
                    }
                    else
                    {
                        runnable.Add(axis);
                    }
                }

                if (stepCanceled)
                {
                    foreach (var ax in step) { TryStop(ax); }
                    var stepResults = step.Select(ax => HomeAxisResult.NotStarted(ax, "Canceled")).ToList();
                    foreach (var r in stepResults) { all.Add(r); }
                    Aborted = true; AbortReason = "Canceled"; AbortStepIndex = stepIndex;

                    RaiseProgress(new OperationProgress
                    {
                        OperationId = "HOME",
                        Title = "Home",
                        StepIndex = stepIndex,
                        TotalSteps = _steps.Count,
                        StepAxisCount = step.Count,
                        StepFailCount = stepResults.Count(r => !r.Success),
                        StepName = string.Join(", ", step.Select(a => a.Name)),
                        IsStepCompleted = true,
                        IsCanceled = true,
                        Message = "Canceled"
                    });

                    if (PostStepAsync != null)
                    {
                        try { await PostStepAsync(stepIndex, stepResults, token).ConfigureAwait(false); } catch { }
                    }
                    break;
                }

                if (blockedReasons.Count > 0)
                {
                    foreach (var ax in step) { TryStop(ax); }
                    foreach (var ax in step)
                    {
                        string r;
                        if (!blockedReasons.TryGetValue(ax, out r)) r = "Blocked by other axis interlock";
                        all.Add(HomeAxisResult.NotStarted(ax, r));
                    }
                    Aborted = true;
                    AbortReason = $"Step {stepIndex} blocked by axis interlock";
                    AbortStepIndex = stepIndex;
                    RaiseProgress(new OperationProgress
                    {
                        OperationId = "HOME",
                        Title = "Home",
                        StepIndex = stepIndex,
                        TotalSteps = _steps.Count,
                        StepAxisCount = step.Count,
                        StepFailCount = step.Count,
                        StepName = string.Join(", ", step.Select(a => a.Name)),
                        IsStepCompleted = true,
                        IsAborted = true,
                        Message = AbortReason
                    });
                    break;
                }

                if (runnable.Count == 0)
                {
                    foreach (var ax in step) { TryStop(ax); }
                    Aborted = true;
                    AbortReason = $"Step {stepIndex} has no runnable axes";
                    AbortStepIndex = stepIndex;
                    RaiseProgress(new OperationProgress
                    {
                        OperationId = "HOME",
                        Title = "Home",
                        StepIndex = stepIndex,
                        TotalSteps = _steps.Count,
                        StepAxisCount = step.Count,
                        StepFailCount = step.Count,
                        StepName = string.Join(", ", step.Select(a => a.Name)),
                        IsStepCompleted = true,
                        IsAborted = true,
                        Message = AbortReason
                    });
                    break;
                }

                var jobs = new List<(MotionAxis Axis, Task<HomeAxisResult> Task)>(runnable.Count);
                foreach (var axis in runnable)
                {
                    jobs.Add((axis, HomeOneAsync(axis, token)));
                }

                using (var cancelReg = token.Register(() =>
                {
                    foreach (var ax in runnable)
                    {
                        TryStop(ax);
                    }
                }))
                {
                    try
                    {
                        var pending = new List<Task<HomeAxisResult>>(jobs.Select(j => j.Task));
                        bool earlyFail = false;
                        string earlyFailReason = null;

                        // 취소 대기 태스크 추가: 취소 즉시 WhenAny가 깨어나도록 함
                        var cancelWait = Task.Delay(Timeout.Infinite, token);

                        while (pending.Count > 0)
                        {
                            var any = await Task.WhenAny(pending.Cast<Task>().Concat(new[] { cancelWait })).ConfigureAwait(false);

                            // 취소가 먼저 도착했으면 즉시 스텝 종료
                            if (any == cancelWait)
                            {
                                foreach (var ax in runnable) { TryStop(ax); }

                                var stepResults = new List<HomeAxisResult>(jobs.Count);
                                foreach (var j in jobs)
                                {
                                    if (j.Task.Status == TaskStatus.RanToCompletion)
                                        stepResults.Add(j.Task.Result);
                                    else
                                        stepResults.Add(HomeAxisResult.NotStarted(j.Axis, "Canceled"));
                                }
                                foreach (var r in stepResults) { all.Add(r); }

                                Aborted = true;
                                AbortReason = "Canceled";
                                AbortStepIndex = stepIndex;

                                var cancelProgress = new OperationProgress
                                {
                                    OperationId = "HOME",
                                    Title = "Home",
                                    StepIndex = stepIndex,
                                    TotalSteps = _steps.Count,
                                    StepAxisCount = step.Count,
                                    StepFailCount = stepResults.Count(r => !r.Success),
                                    StepName = string.Join(", ", step.Select(a => a.Name)),
                                    IsStepCompleted = true,
                                    IsCanceled = true,
                                    Message = "Canceled"
                                };
                                RaiseProgress(cancelProgress);

                                if (PostStepAsync != null)
                                {
                                    try { await PostStepAsync(stepIndex, stepResults, token).ConfigureAwait(false); } catch { }
                                }
                                break;
                            }

                            // 작업 하나 완료됨
                            var finished = (Task<HomeAxisResult>)any;
                            pending.Remove(finished);

                            HomeAxisResult res;
                            try
                            {
                                res = await finished.ConfigureAwait(false);
                            }
                            catch (OperationCanceledException)
                            {
                                // 개별 태스크가 취소로 완료된 경우: 전체를 취소 처리
                                foreach (var ax in runnable) { TryStop(ax); }

                                var stepResults = new List<HomeAxisResult>(jobs.Count);
                                foreach (var j in jobs)
                                {
                                    if (j.Task.Status == TaskStatus.RanToCompletion)
                                        stepResults.Add(j.Task.Result);
                                    else
                                        stepResults.Add(HomeAxisResult.NotStarted(j.Axis, "Canceled"));
                                }
                                foreach (var r in stepResults) { all.Add(r); }

                                Aborted = true; AbortReason = "Canceled"; AbortStepIndex = stepIndex;
                                RaiseProgress(new OperationProgress
                                {
                                    OperationId = "HOME",
                                    Title = "Home",
                                    StepIndex = stepIndex,
                                    TotalSteps = _steps.Count,
                                    StepAxisCount = step.Count,
                                    StepFailCount = stepResults.Count(r => !r.Success),
                                    StepName = string.Join(", ", step.Select(a => a.Name)),
                                    IsStepCompleted = true,
                                    IsCanceled = true,
                                    Message = "Canceled"
                                });

                                if (PostStepAsync != null)
                                {
                                    try { await PostStepAsync(stepIndex, stepResults, token).ConfigureAwait(false); } catch { }
                                }
                                break;
                            }

                            if (!res.Success)
                            {
                                earlyFail = true;
                                earlyFailReason = res.FailReason ?? ("ReturnCode=" + res.ReturnCode);
                                break;
                            }
                        }

                        if (token.IsCancellationRequested)
                        {
                            break; // 위에서 처리했으므로 전체 루프 탈출
                        }

                        if (earlyFail)
                        {
                            foreach (var ax in step) { TryStop(ax); }

                            var stepResults = new List<HomeAxisResult>(jobs.Count);
                            foreach (var j in jobs)
                            {
                                if (j.Task.Status == TaskStatus.RanToCompletion)
                                    stepResults.Add(await j.Task.ConfigureAwait(false));
                                else
                                    stepResults.Add(HomeAxisResult.NotStarted(j.Axis, "Aborted by early failure"));
                            }
                            foreach (var r in stepResults) { all.Add(r); }

                            Aborted = true;
                            AbortReason = $"Step {stepIndex} failed early: {earlyFailReason}";
                            AbortStepIndex = stepIndex;

                            var failProgress = new OperationProgress
                            {
                                OperationId = "HOME",
                                Title = "Home",
                                StepIndex = stepIndex,
                                TotalSteps = _steps.Count,
                                StepAxisCount = step.Count,
                                StepFailCount = stepResults.Count(r => !r.Success),
                                StepName = string.Join(", ", step.Select(a => a.Name)),
                                IsStepCompleted = true,
                                IsAborted = true,
                                Message = AbortReason
                            };
                            RaiseProgress(failProgress);

                            if (PostStepAsync != null)
                            {
                                try { await PostStepAsync(stepIndex, stepResults, token).ConfigureAwait(false); } catch { }
                            }
                            break;
                        }
                        else
                        {
                            var results = await Task.WhenAll(jobs.Select(j => j.Task)).ConfigureAwait(false);
                            foreach (var r in results) { all.Add(r); }

                            var doneProgress = new OperationProgress
                            {
                                OperationId = "HOME",
                                Title = "Home",
                                StepIndex = stepIndex,
                                TotalSteps = _steps.Count,
                                StepAxisCount = step.Count,
                                StepFailCount = results.Count(r => !r.Success),
                                StepName = string.Join(", ", step.Select(a => a.Name)),
                                IsStepCompleted = true
                            };
                            RaiseProgress(doneProgress);

                            if (PostStepAsync != null)
                            {
                                try { await PostStepAsync(stepIndex, results, token).ConfigureAwait(false); } catch { }
                            }
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        foreach (var ax in step) { TryStop(ax); }
                        Aborted = true; AbortReason = "Canceled"; AbortStepIndex = stepIndex;
                        RaiseProgress(new OperationProgress
                        {
                            OperationId = "HOME",
                            Title = "Home",
                            StepIndex = stepIndex,
                            TotalSteps = _steps.Count,
                            StepAxisCount = step.Count,
                            StepFailCount = step.Count,
                            StepName = string.Join(", ", step.Select(a => a.Name)),
                            IsStepCompleted = true,
                            IsCanceled = true,
                            Message = "Canceled"
                        });
                        break;
                    }
                }
            }

            var finalProgress = new OperationProgress
            {
                OperationId = "HOME",
                Title = "Home",
                StepIndex = _steps.Count - 1,
                TotalSteps = _steps.Count,
                IsCompleted = true,
                IsCanceled = Aborted && AbortReason == "Canceled",
                IsAborted = Aborted,
                Message = AbortReason
            };
            RaiseProgress(finalProgress);

            return all;
        }

        private static async Task<HomeAxisResult> HomeOneAsync(MotionAxis axis, CancellationToken token)
        {
            if (axis == null) return new HomeAxisResult(null, -1, new ArgumentNullException("axis"), false, "axis null");
            return await Task.Run(() =>
            {
                try
                {
                    token.ThrowIfCancellationRequested();
                    var rc = axis.HomeSync();
                    return new HomeAxisResult(axis, rc, null, true, rc == 0 ? null : "Home failed (rc=" + rc + ")");
                }
                catch (OperationCanceledException)
                {
                    TryStop(axis); throw;
                }
                catch (Exception ex)
                {
                    TryStop(axis); return new HomeAxisResult(axis, -1, ex, true, ex.Message);
                }
            }, token).ConfigureAwait(false);
        }
        private static void TryStop(MotionAxis axis) { try { axis?.EmgStop(); } catch { } try { axis?.Stop(); } catch { } }
    }

    public sealed class HomeAxisResult
    {
        public MotionAxis Axis { get; }
        public string AxisName { get { return Axis != null ? Axis.Name : string.Empty; } }
        public int ReturnCode { get; }
        public Exception Error { get; }
        public bool Success { get { return Started && Error == null && ReturnCode == 0; } }
        public bool Started { get; }
        public string FailReason { get; }
        public HomeAxisResult(MotionAxis axis, int returnCode, Exception error, bool started, string failReason) { Axis = axis; ReturnCode = returnCode; Error = error; Started = started; FailReason = failReason; }
        public static HomeAxisResult NotStarted(MotionAxis axis, string reason) { return new HomeAxisResult(axis, 0, null, false, reason); }
    }
}
