using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace QMC.Common.IO
{
    /// <summary>
    /// Simulated=true  : 메모리 캐시로 동작(시뮬)
    /// Simulated=false : 내부 real 드라이버로 패스스루(실기)
    /// </summary>
    public sealed class SimDioDriver : IDIODriver, IDisposable
    {
        private struct Key { public int B, P, C; }
        private static Key K(int b, int p, int c) { return new Key { B = b, P = p, C = c }; }

        private readonly IDIODriver _real; // 실기 드라이버(옵션)
        public bool Simulated { get; private set; }

        private readonly ConcurrentDictionary<Key, bool> _inputs = new ConcurrentDictionary<Key, bool>();
        private readonly ConcurrentDictionary<Key, bool> _outputs = new ConcurrentDictionary<Key, bool>();
        private readonly ConcurrentDictionary<Key, Timer> _pulseTimers = new ConcurrentDictionary<Key, Timer>();

        public SimDioDriver(bool simulated = true, IDIODriver real = null)
        {
            Simulated = simulated;
            _real = real;
            if (!Simulated && _real == null)
                throw new ArgumentNullException(nameof(real), "실기 모드에서는 실제 드라이버가 필요합니다.");
        }

        /// <summary>실제 드라이버 래핑용 팩토리</summary>
        public static SimDioDriver FromReal(IDIODriver real) { return new SimDioDriver(false, real); }
        /// <summary>순수 시뮬 팩토리</summary>
        public static SimDioDriver CreateSim() { return new SimDioDriver(true, null); }

        /// <summary>
        /// 런타임 토글(주의: 실기 전환에는 _real 필요)
        /// </summary>
        public void SetSimulated(bool on)
        {
            if (!on && _real == null)
                throw new InvalidOperationException("실기 모드로 전환하려면 real 드라이버가 필요합니다.");
            Simulated = on;
        }

        // ===== IDIODriver =====
        public int ReadInput(int b, int p, int c, out bool value)
        {
            if (!Simulated) return _real.ReadInput(b, p, c, out value);
            value = _inputs.GetOrAdd(K(b, p, c), false);
            return 0;
        }

        public int ReadOutput(int b, int p, int c, out bool value)
        {
            if (!Simulated) return _real.ReadOutput(b, p, c, out value);
            value = _outputs.GetOrAdd(K(b, p, c), false);
            return 0;
        }

        public int WriteOutput(int b, int p, int c, bool value)
        {
            if (!Simulated) return _real.WriteOutput(b, p, c, value);

            var key = K(b, p, c);
            _outputs[key] = value;

            // 기존 펄스 타이머가 돌고 있다면 취소/해제
            Timer old;
            if (_pulseTimers.TryRemove(key, out old))
                try { old.Dispose(); } catch { }

            return 0;
        }

        public int PulseOutput(int b, int p, int c, int widthMs)
        {
            if (!Simulated) return _real.PulseOutput(b, p, c, widthMs);

            var key = K(b, p, c);
            _outputs[key] = true;

            // 기존 펄스 타이머가 있으면 교체
            Timer old;
            if (_pulseTimers.TryRemove(key, out old))
                try { old.Dispose(); } catch { }

            var due = widthMs < 0 ? 0 : widthMs;
            var t = new Timer(_ =>
            {
                _outputs[key] = false;
                Timer removed;
                if (_pulseTimers.TryRemove(key, out removed))
                {
                    try { removed.Dispose(); } catch { }
                }
            }, null, due, Timeout.Infinite);

            _pulseTimers[key] = t;
            return 0;
        }

        // ===== 시뮬 전용 편의 함수 =====
        /// <summary>테스트용: 입력 강제(시뮬 모드에서만 의미 있음)</summary>
        public void SetInput(int b, int p, int c, bool v)
        {
            _inputs[K(b, p, c)] = v;
        }

        public void Dispose()
        {
            foreach (var kv in _pulseTimers)
            {
                try { kv.Value.Dispose(); } catch { }
            }
            _pulseTimers.Clear();
        }
    }
}
