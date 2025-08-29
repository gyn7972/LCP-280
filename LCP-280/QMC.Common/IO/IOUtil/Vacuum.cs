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

        public Vacuum(string name, string outKey, string okInKey) { Name = name; _outKey = outKey; _okInKey = okInKey; }

        public void On() => DIO.Out(_outKey, true);
        public void Off() => DIO.Out(_outKey, false);
        public bool IsOk() => DIO.In(_okInKey, out var v) && v;

        public bool OnWaitOk(int timeoutMs = 2000, int pollMs = 10)
        {
            On();
            var sw = Stopwatch.StartNew();
            while (sw.ElapsedMilliseconds < timeoutMs)
            {
                if (IsOk()) return true;
                Thread.Sleep(pollMs);
            }
            return false;
        }
    }
}
