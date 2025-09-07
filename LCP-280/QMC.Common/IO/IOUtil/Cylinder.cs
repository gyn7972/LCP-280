// Cylinder.cs
using System.Diagnostics;
using System.Threading;

namespace QMC.Common.IOUtil
{
    /// <summary>2솔 밸브 + 2센서(UP/DOWN or FWD/BWD) 전형</summary>
    public sealed class Cylinder
    {
        public string Name { get; }
        private readonly string _fwdOutKey, _bwdOutKey, _fwdInKey, _bwdInKey;

        // 키 직접 접근 프로퍼티 (리플렉션 제거 목적)
        public string FwdOutKey => _fwdOutKey;
        public string BwdOutKey => _bwdOutKey;
        public string FwdInKey => _fwdInKey;
        public string BwdInKey => _bwdInKey;

        public Cylinder(string name, string fwdOutKey, string bwdOutKey, string fwdInKey, string bwdInKey)
        { Name = name; _fwdOutKey = fwdOutKey; _bwdOutKey = bwdOutKey; _fwdInKey = fwdInKey; _bwdInKey = bwdInKey; }

        public bool Extend(int timeoutMs = 5000, int settleMs = 50)
        {
            DIO.Out(_bwdOutKey, false);
            DIO.Out(_fwdOutKey, true);

            var sw = Stopwatch.StartNew();
            while (sw.ElapsedMilliseconds < timeoutMs)
            {
                if (DIO.In(_fwdInKey, out var on) && on) { Thread.Sleep(settleMs); return true; }
                Thread.Sleep(10);
            }
            return false;
        }

        public bool Retract(int timeoutMs = 5000, int settleMs = 50)
        {
            DIO.Out(_fwdOutKey, false);
            DIO.Out(_bwdOutKey, true);

            var sw = Stopwatch.StartNew();
            while (sw.ElapsedMilliseconds < timeoutMs)
            {
                if (DIO.In(_bwdInKey, out var on) && on) { Thread.Sleep(settleMs); return true; }
                Thread.Sleep(10);
            }
            return false;
        }

        public void AllOff() { DIO.Out(_fwdOutKey, false); DIO.Out(_bwdOutKey, false); }
    }
}
