// Vacuum.cs
using System.Diagnostics;
using System.Threading;

namespace QMC.Common.IOUtil
{
    /// <summary>Vacuum 밸브 + Vacuum OK 센서</summary>
    public sealed class Vacuum
    {
        public string Name { get; }
        private readonly string _outKey, _okInKey;

        // 새로 추가: 외부에서 직접 키 접근 가능 (리플렉션 제거 목적)
        public string OutKey => _outKey;
        public string OkInKey => _okInKey;

        // 센서 보유 여부: 키가 비었거나 "/*NO_SENSOR*/" 패턴이면 없음으로 간주
        public bool HasOkSensor =>
            !string.IsNullOrEmpty(_okInKey) &&
            _okInKey.IndexOf("/*NO_SENSOR*/", System.StringComparison.OrdinalIgnoreCase) < 0;

        public Vacuum(string name, string outKey, string okInKey)
        {
            Name = name;
            _outKey = outKey;
            _okInKey = okInKey;
        }

        public void On() => DIO.Out(_outKey, true);
        public void Off() => DIO.Out(_outKey, false);

        // OK 센서를 읽되, 센서가 없으면 false를 반환(읽기 불가 의미)
        public bool TryGetOk(out bool ok)
        {
            ok = false;
            if (!HasOkSensor) 
                return false;
            if (DIO.In(_okInKey, out var v)) 
            { 
                ok = v; 
                return true; 
            }

            return false;
        }

        // 센서가 없으면 false(측정 불가)를 반환. 상태 판단은 OnWaitOk/OffWaitOk를 사용하세요.
        public bool IsOk()
        {
            return HasOkSensor && DIO.In(_okInKey, out var v) && v;
        }

        /// <summary>
        /// Vacuum On 후 OK까지 대기. 센서가 없으면 settleMs 대기 후 성공 처리(옵션).
        /// </summary>
        public bool OnWaitOk(int timeoutMs = 2000, int pollMs = 2, int settleMs = 50, bool assumeOkWhenNoSensor = true)
        {
            On();

            if (!HasOkSensor)
            {
                if (assumeOkWhenNoSensor && settleMs > 0)
                    Thread.Sleep(settleMs);
                return assumeOkWhenNoSensor;
            }

            var sw = Stopwatch.StartNew();
            while (sw.ElapsedMilliseconds < timeoutMs)
            {
                if (IsOk())
                {
                    if (settleMs > 0) Thread.Sleep(settleMs);
                    return true;
                }
                Thread.Sleep(pollMs);
            }
            return false;
        }

        /// <summary>
        /// Vacuum Off 후 OK 센서가 떨어질 때까지 대기. 센서가 없으면 settleMs 대기 후 성공 처리.
        /// </summary>
        public bool OffWaitOk(int timeoutMs = 2000, int pollMs = 2, int settleMs = 50, bool assumeOffWhenNoSensor = true)
        {
            Off();

            if (!HasOkSensor)
            {
                if (assumeOffWhenNoSensor && settleMs > 0)
                    Thread.Sleep(settleMs);
                return assumeOffWhenNoSensor;
            }

            var sw = Stopwatch.StartNew();
            while (sw.ElapsedMilliseconds < timeoutMs)
            {
                // OK가 false가 되면 Off 완료로 간주
                bool has = DIO.In(_okInKey, out var v) && v;
                if (!has)
                {
                    if (settleMs > 0) Thread.Sleep(settleMs);
                    return true;
                }
                Thread.Sleep(pollMs);
            }
            return false;
        }
    }
}