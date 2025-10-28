// Cylinder.cs
using System;
using System.Threading;
using System.Threading.Tasks;
using QMC.Common.DIO;   // DioScanService

namespace QMC.Common.Cylinder
{
    public enum CylinderState
    {
        Unknown,
        MovingForward,
        MovingBackward,
        Forward,
        Backward,
        Fault
    }

    public sealed class Cylinder
    {
        private readonly DioScanService _dio;
        private readonly CylinderConfig _cfg;
        private readonly object _gate = new object();

        public CylinderState State { get; private set; } = CylinderState.Unknown;
        public string LastFault { get; private set; } // null 허용 (C# 7.3)

        public Cylinder(DioScanService dio, CylinderConfig config)
        {
            if (dio == null) throw new ArgumentNullException(nameof(dio));
            if (config == null) throw new ArgumentNullException(nameof(config));
            _dio = dio; _cfg = config;
            RefreshState();
        }

        // ===== Public API =====
        public Task<bool> MoveForwardAsync(CancellationToken ct = default(CancellationToken))
            => MoveAsync(true, ct);

        public Task<bool> MoveBackwardAsync(CancellationToken ct = default(CancellationToken))
            => MoveAsync(false, ct);

        public void StopOutputs()
        {
            // 중립(둘 다 OFF) — 필요 시 보유형 밸브 옵션 추가 가능
            lock (_gate)
            {
                _dio.WriteOutput(_cfg.ModuleName, _cfg.ForwardOut, false);
                _dio.WriteOutput(_cfg.ModuleName, _cfg.BackwardOut, false);
            }
            RefreshState();
        }

        public void RefreshState()
        {
            bool fwd = ReadInputSafe(_cfg.ForwardIn);
            bool bwd = ReadInputSafe(_cfg.BackwardIn);

            if (fwd && !bwd) State = CylinderState.Forward;
            else if (!fwd && bwd) State = CylinderState.Backward;
            else if (fwd && bwd) State = CylinderState.Fault;   // 양끝 동시 ON → 이상
            else State = CylinderState.Unknown; // 중간
        }

        // ===== 내부 =====
        private async Task<bool> MoveAsync(bool forward, CancellationToken ct)
        {
            // 상호배제: 한 쪽만 ON
            lock (_gate)
            {
                _dio.WriteOutput(_cfg.ModuleName, _cfg.ForwardOut, forward);
                _dio.WriteOutput(_cfg.ModuleName, _cfg.BackwardOut, !forward);
            }
            State = forward ? CylinderState.MovingForward : CylinderState.MovingBackward;

            if (!_cfg.Monitoring) return true;

            var t0 = Environment.TickCount;
            while (!ct.IsCancellationRequested)
            {
                // 이상: 양끝 동시 감지
                if (ReadInputSafe(_cfg.ForwardIn) && ReadInputSafe(_cfg.BackwardIn))
                {
                    LastFault = "Both sensors ON";
                    State = CylinderState.Fault;
                    StopOutputs();
                    return false;
                }

                // 도달?
                bool reached = forward ? (ReadInputSafe(_cfg.ForwardIn) && !ReadInputSafe(_cfg.BackwardIn))
                                       : (ReadInputSafe(_cfg.BackwardIn) && !ReadInputSafe(_cfg.ForwardIn));
                if (reached)
                {
                    await Task.Delay(_cfg.SettleMs, ct).ConfigureAwait(false);
                    bool stable = forward ? (ReadInputSafe(_cfg.ForwardIn) && !ReadInputSafe(_cfg.BackwardIn))
                                          : (ReadInputSafe(_cfg.BackwardIn) && !ReadInputSafe(_cfg.ForwardIn));
                    if (stable)
                    {
                        State = forward ? CylinderState.Forward : CylinderState.Backward;
                        return true;
                    }
                }

                if (Environment.TickCount - t0 > _cfg.TimeoutMs)
                {
                    LastFault = "Timeout";
                    State = CylinderState.Fault;
                    StopOutputs();
                    return false;
                }

                await Task.Delay(5, ct).ConfigureAwait(false);
            }

            LastFault = "Canceled";
            StopOutputs();
            return false;
        }

        private bool ReadInputSafe(string displayNo)
        {
            bool v;
            // DioScanService 캐시에서 읽기 (모니터링된 항목)  :contentReference[oaicite:4]{index=4}
            if (_dio.TryGetInput(_cfg.ModuleName, displayNo, out v)) return v;
            // 캐시에 없으면 즉시 1회 스캔 후 재시도
            _dio.RefreshOnce(); // 1회 스캔 API  :contentReference[oaicite:5]{index=5}
            return _dio.TryGetInput(_cfg.ModuleName, displayNo, out v) ? v : false;
        }


        // GUI에서 사용시.
        /*private async void btnForward_Click(object sender, EventArgs e)
        {
            btnForward.Enabled = btnBackward.Enabled = false;
            await feederUp.MoveForwardAsync();
            btnForward.Enabled = btnBackward.Enabled = true;
            UpdateCylinderUI();
        }
        private void UpdateCylinderUI()
        {
            lblStatus.Text = feederUp.State.ToString();
            // 그리드의 Forward In/Out, Backward In/Out 색칠도
            // scan.TryGetInput/Output("DIO Module1", "X20" 등) 값을 사용해 반영하면 됨.  :contentReference[oaicite:16]{index=16}
        }*/

    }

}
