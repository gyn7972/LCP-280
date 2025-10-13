// 새 파일: MotionStatusScanner.cs
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace QMC.Common.Motions
{
    /// <summary>
    /// 모든 축 상태를 주기적으로 스캔하여 이벤트로 내보내는 폴러.
    /// 제어(이동)와 스레드를 분리해 충돌을 줄입니다.
    /// </summary>
    public sealed class MotionStatusScanner : IDisposable
    {
        private readonly MotionAxisManager _manager;
        private readonly int _periodMs;
        private CancellationTokenSource _cts;
        private Task _task;
        private readonly object _gate = new object();

        /// <summary>축 하나의 최신 스냅샷이 올라옵니다.</summary>
        public event Action<MotionAxis, MotionAxisStatus> AxisStatusUpdated;

        public MotionStatusScanner(MotionAxisManager manager, int periodMs = 2)
        {
            _manager = manager ?? throw new ArgumentNullException(nameof(manager));
            if (periodMs < 3) periodMs = 2; // 너무 빠른 폴링 방지
            _periodMs = periodMs;
        }

        public void Start()
        {
            lock (_gate)
            {
                if (_task != null && !_task.IsCompleted) return;
                _cts = new CancellationTokenSource();

                Thread.CurrentThread.Name = "MotionStatusScanner";
                _task = Task.Run(() => LoopAsync(_cts.Token), _cts.Token);
            }
        }

        public void Stop()
        {
            lock (_gate)
            {
                _cts?.Cancel();
            }
            try { _task?.Wait(); } catch { /* ignore */ }
        }

        private async Task LoopAsync(CancellationToken ct)
        {
            var sw = new Stopwatch();
            while (!ct.IsCancellationRequested)
            {
                sw.Restart();
                IReadOnlyList<MotionAxis> axes = null;
                try { axes = _manager.GetAllAxes(); } catch { /* manager 잠금 오류 방지 */ }

                if (axes != null)
                {
                    foreach (var axis in axes)
                    {
                        if (ct.IsCancellationRequested) break;
                        try
                        {
                            // 보드에서 읽어와 보정한 ‘복사본’을 받음 (thread-safe)
                            var snapshot = axis.GetStatusSnapshot();
                            AxisStatusUpdated?.Invoke(axis, snapshot);
                        }
                        catch (Exception ex)
                        {
                            // 상태 읽기 중 예외 삼켜서 폴링이 죽지 않도록
                            Log.Write(ex);
                        }
                    }
                }

                // 주기 유지(작업에 걸린 시간 제외)
                var wait = _periodMs - (int)sw.ElapsedMilliseconds;
                if (wait < 0) wait = 0;
                try { await Task.Delay(wait, ct); } catch { /* canceled */ }
            }
        }

        public void Dispose() => Stop();
    }
}
