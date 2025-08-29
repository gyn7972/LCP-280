// QMC.Common.Motion.Ajin.IO/AjinDioDriver.cs
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using QMC.Common.IO; // IDIODriver 인터페이스 위치 (네 프로젝트 구조에 맞춰 using 조정)

namespace QMC.Common.Motion.Ajin.IO
{
    /// <summary>
    /// Ajin AXL 래퍼(AXD) 기반 DIO 드라이버.
    /// - 입력 읽기 : AXD.Read(moduleNo, address, ref bool)
    /// - 출력 읽기 : AXD.ReadOutput(moduleNo, address, ref byte)
    /// - 출력 쓰기 : AXD.Write(moduleNo, address, byte)
    ///
    /// (boardNo, portNo) -> moduleNo 는 호출자가 제공하는 mapper로 변환.
    /// </summary>
    public sealed class AjinDioDriver : IDIODriver, IDisposable
    {
        public delegate int ModuleMapper(int boardNo, int portNo);

        private readonly ModuleMapper _map;
        private readonly ConcurrentDictionary<Tuple<int, int, int>, Timer> _pulseTimers
            = new ConcurrentDictionary<Tuple<int, int, int>, Timer>();

        /// <param name="mapper">
        /// (boardNo, portNo) => moduleNo 로 변환하는 함수.
        /// 예) (b,p) => b*8 + p  (현장 구성에 맞게 1회 정의)
        /// </param>
        public AjinDioDriver(ModuleMapper mapper)
        {
            if (mapper == null) throw new ArgumentNullException(nameof(mapper));
            _map = mapper;
        }

        public int ReadInput(int boardNo, int portNo, int channelNo, out bool value)
        {
            var moduleNo = _map(boardNo, portNo);
            value = false;
            // AjinAxlDioModule 참조: AXD.Read(mod, address, ref bool)
            int rc = AXD.Read(moduleNo, channelNo, ref value);
            return rc;
        }

        public int ReadOutput(int boardNo, int portNo, int channelNo, out bool value)
        {
            var moduleNo = _map(boardNo, portNo);
            byte b = 0;
            // AjinAxlDioModule 참조: AXD.ReadOutput(mod, address, ref byte)
            int rc = AXD.ReadOutput(moduleNo, channelNo, ref b);
            value = (b != 0);
            return rc;
        }

        public int WriteOutput(int boardNo, int portNo, int channelNo, bool v)
        {
            var moduleNo = _map(boardNo, portNo);
            // AjinAxlDioModule 참조: AXD.Write(mod, address, byte)
            CancelPulseTimerIfAny(boardNo, portNo, channelNo); // 펄스 중복 방지
            int rc = AXD.Write(moduleNo, channelNo, v ? (byte)1 : (byte)0);
            return rc;
        }

        public int PulseOutput(int boardNo, int portNo, int channelNo, int widthMs)
        {
            if (widthMs < 0) widthMs = 0;
            var key = Tuple.Create(boardNo, portNo, channelNo);

            // 기존 펄스 타이머가 있으면 먼저 제거
            CancelPulseTimerIfAny(boardNo, portNo, channelNo);

            // ON
            var rc = WriteOutput(boardNo, portNo, channelNo, true);
            if (rc != 0) return rc;

            // width 후 OFF
            var t = new Timer(_ =>
            {
                try { WriteOutput(boardNo, portNo, channelNo, false); }
                catch { /* ignore */ }
                finally
                {
                    Timer removed;
                    _pulseTimers.TryRemove(key, out removed);
                    try { removed?.Dispose(); } catch { }
                }
            }, null, widthMs, Timeout.Infinite);

            _pulseTimers[key] = t;
            return 0;
        }

        private void CancelPulseTimerIfAny(int b, int p, int c)
        {
            var key = Tuple.Create(b, p, c);
            Timer old;
            if (_pulseTimers.TryRemove(key, out old))
            {
                try { old.Dispose(); } catch { }
            }
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
