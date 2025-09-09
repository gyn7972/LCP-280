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
            var axes = _manager.GetAllAxes();
            var results = new List<HomeAxisResult>(axes.Count);
            foreach (var axis in axes)
            {
                if (token.IsCancellationRequested)
                {
                    TryStop(axis);
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
            var all = new List<HomeAxisResult>();
            for (int stepIndex = 0; stepIndex < _steps.Count; stepIndex++)
            {
                var step = _steps[stepIndex];
                if (step == null || step.Count == 0) continue;
                if (token.IsCancellationRequested) break;

                if (PreStepInterlockAsync != null)
                {
                    var tuple = await PreStepInterlockAsync(stepIndex, step, token).ConfigureAwait(false);
                    if (!tuple.Ok)
                    {
                        for (int i = 0; i < step.Count; i++)
                            all.Add(HomeAxisResult.NotStarted(step[i], tuple.Reason));
                        continue;
                    }
                }

                var tasks = new List<Task<HomeAxisResult>>(step.Count);

                foreach (var axis in step)
                {
                    if (!axis.CheckHomeInterlocks(out var reason))
                    {
                        all.Add(HomeAxisResult.NotStarted(axis, reason));
                    }
                    else
                    {
                        tasks.Add(HomeOneAsync(axis, token));
                    }
                }

                if (tasks.Count == 0) continue;

                try
                {
                    var results = await Task.WhenAll(tasks).ConfigureAwait(false);
                    all.AddRange(results);

                    // 스텝 종료 후 콜백 호출(추가)
                    if (PostStepAsync != null)
                    {
                        try
                        {
                            await PostStepAsync(stepIndex, results, token).ConfigureAwait(false);
                        }
                        catch
                        {
                            // 필요 시 로깅 추가
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    foreach (var axis in step) TryStop(axis);
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
                    var rc = axis.HomeSync();
                    return new HomeAxisResult(axis, rc, null, started: true, failReason: null);
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
