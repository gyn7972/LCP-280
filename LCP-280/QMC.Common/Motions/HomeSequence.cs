using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace QMC.Common.Motions
{
    /// <summary>
    /// 여러 축의 홈 동작을 순서/병렬로 실행하는 시퀀서.
    /// - AddParallelStep로 단계별(순차) + 축묶음(동시) 구성
    /// - RunAsync로 단계 순서대로 실행(각 단계 내 축은 병렬)
    /// - HomeAllSequentialAsync / HomeAllParallelAsync 헬퍼 제공
    /// </summary>
    public sealed class HomeSequence
    {
        private readonly MotionAxisManager _manager;
        private readonly List<List<MotionAxis>> _steps = new List<List<MotionAxis>>();

        public HomeSequence(MotionAxisManager manager)
        {
            _manager = manager ?? throw new ArgumentNullException(nameof(manager));
        }

        /// <summary>
        /// 동시 실행될 축 묶음을 한 단계로 추가합니다(실행 순서는 추가한 단계 순으로).
        /// </summary>
        public HomeSequence AddParallelStep(params MotionAxis[] axes)
        {
            if (axes == null || axes.Length == 0) return this;
            _steps.Add(new List<MotionAxis>(axes));
            return this;
        }

        /// <summary>
        /// 유닛명과 축명 배열로 한 단계(병렬)를 추가합니다.
        /// </summary>
        public HomeSequence AddParallelStep(string unitName, params string[] axisNames)
        {
            if (string.IsNullOrEmpty(unitName) || axisNames == null || axisNames.Length == 0) return this;
            var axes = _manager.GetAxes(unitName, axisNames);
            if (axes != null && axes.Count > 0)
                _steps.Add(new List<MotionAxis>(axes));
            return this;
        }

        /// <summary>
        /// 등록된 모든 축을 한 단계(전축 병렬)로 구성하여 실행합니다.
        /// </summary>
        public Task<IReadOnlyList<HomeAxisResult>> HomeAllParallelAsync(CancellationToken token = default(CancellationToken))
        {
            var axes = _manager.GetAllAxes();
            if (axes == null || axes.Count == 0) return Task.FromResult((IReadOnlyList<HomeAxisResult>)Array.Empty<HomeAxisResult>());
            _steps.Clear();
            _steps.Add(new List<MotionAxis>(axes));
            return RunAsync(token);
        }

        /// <summary>
        /// 등록된 모든 축을 순차로 홈합니다(한 축 완료 후 다음 축).
        /// </summary>
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

                // 사전 인터락 체크
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

        /// <summary>
        /// 구성된 단계대로 실행합니다. 각 단계는 병렬, 단계 간은 순차.
        /// </summary>
        public async Task<IReadOnlyList<HomeAxisResult>> RunAsync(CancellationToken token = default(CancellationToken))
        {
            var all = new List<HomeAxisResult>();
            foreach (var step in _steps)
            {
                if (step == null || step.Count == 0) continue;
                if (token.IsCancellationRequested) break;

                var tasks = new List<Task<HomeAxisResult>>(step.Count);

                // 사전 인터락 검사: 통과 축만 시작, 실패 축은 NotStarted로 바로 기록
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

                if (tasks.Count == 0) continue; // 해당 단계에 시작할 축이 없다면 다음 단계로

                try
                {
                    var results = await Task.WhenAll(tasks).ConfigureAwait(false);
                    all.AddRange(results);
                }
                catch (OperationCanceledException)
                {
                    // 취소 시 현재 단계의 축들을 가능한 정지
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
                    var rc = axis.HomeSync(); // 각 축의 개별 타임아웃을 활용
                    return new HomeAxisResult(axis, rc, null, started: true, failReason: null);
                }
                catch (OperationCanceledException)
                {
                    TryStop(axis);
                    throw; // 상위에서 처리
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
