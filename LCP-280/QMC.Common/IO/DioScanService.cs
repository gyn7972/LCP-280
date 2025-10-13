// QMC.Common.DIO/DioScanService.cs
using QMC.Common.IO;
using System;
using System.Collections.Generic;
using System.Threading;

namespace QMC.Common.DIO
{
    /// <summary>
    /// 드라이버에서 주기적으로 DI/DO를 읽어 캐시에 보관.
    /// 시퀀스/화면은 이 캐시만 조회 → 안정적/일관된 값 사용.
    /// </summary>
    public sealed class DioScanService : IDisposable
    {
        private readonly DIOUnit _unit;
        private readonly IDIODriver _drv;

        private readonly object _gate = new object();
        private Thread _worker;
        private volatile bool _running;
        private int _periodMs = 15;

        // 캐시(key="ModuleName|DisplayNo")
        private readonly Dictionary<string, bool> _inputCache = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, bool> _outputCache = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);

        // 빠른 스캔을 위한 미리 계산된 항목들
        private struct ScanItem
        {
            public string Key;              // Module|DisplayNo
            public int BoardNo, PortNo, ChannelNo;
            public bool Reverse, Monitoring;
        }
        private ScanItem[] _inputs;
        private ScanItem[] _outputs;

        // 변경 이벤트(옵션)
        public event Action<string, string, bool> InputChanged;   // (moduleName, displayNo, value)
        public event Action<string, string, bool> OutputChanged;

        public DioScanService(DIOUnit unitSetup, IDIODriver driver)
        {
            if (unitSetup == null) throw new ArgumentNullException(nameof(unitSetup));
            if (driver == null) throw new ArgumentNullException(nameof(driver));
            _unit = unitSetup; _drv = driver;
            BuildScanLists();
        }

        //private static string KeyOf(string moduleName, string displayNo)
        //{
        //    //return moduleName + "|" + displayNo;
        //    return moduleName + "|" + NormalizeDisp(displayNo);
        //}
        private static string KeyOf(string moduleName, string displayNo)
    => (moduleName ?? "").Trim() + "|" + NormalizeDisp(displayNo);

        private void BuildScanLists()
        {
            var ins = new List<ScanItem>();
            var outs = new List<ScanItem>();

            if (_unit.Modules != null)
            {
                for (int i = 0; i < _unit.Modules.Count; i++)
                {
                    var m = _unit.Modules[i];

                    // Inputs
                    if (m.Inputs != null)
                    {
                        for (int j = 0; j < m.Inputs.Count; j++)
                        {
                            var c = m.Inputs[j];
                            var item = new ScanItem();
                            item.Key = KeyOf(m.ModuleName, c.DisplayNo);
                            item.BoardNo = c.BoardNo != 0 ? c.BoardNo : m.BoardNo;
                            item.PortNo = c.PortNo;
                            //item.ChannelNo = c.ChannelNo == 0 ? c.Index : c.ChannelNo; 
                            item.ChannelNo = c.ChannelNo == 0 ? j : c.ChannelNo;
                            item.Reverse = c.Reverse;
                            item.Monitoring = c.Monitoring;
                            ins.Add(item);
                        }
                    }

                    // Outputs
                    if (m.Outputs != null)
                    {
                        for (int j = 0; j < m.Outputs.Count; j++)
                        {
                            var c = m.Outputs[j];
                            var item = new ScanItem();
                            item.Key = KeyOf(m.ModuleName, c.DisplayNo);
                            item.BoardNo = c.BoardNo != 0 ? c.BoardNo : m.BoardNo;
                            item.PortNo = c.PortNo;
                            item.ChannelNo = c.ChannelNo == 0 ? j : c.ChannelNo;
                            item.Reverse = c.Reverse;
                            item.Monitoring = c.Monitoring;
                            outs.Add(item);
                        }
                    }
                }
            }

            _inputs = ins.ToArray();
            _outputs = outs.ToArray();
        }

        // ========= 제어 =========
        public void Start(int periodMs = 10)
        {
            if (periodMs < 1) periodMs = 1;
            _periodMs = periodMs;
            if (_running) return;

            //Thread.CurrentThread.Name = "DioScanService_WorkLoop";
            _running = true;
            _worker = new Thread(WorkLoop);
            _worker.IsBackground = true;
            _worker.Name = "DioScanService_WorkLoop";
            _worker.Start();
        }

        public void Stop()
        {
            _running = false;
            var w = _worker;
            if (w != null && w.IsAlive)
            {
                if (!w.Join(2000)) try { w.Abort(); } catch { }
            }
            _worker = null;
        }

        public void Dispose() { Stop(); }

        // ========= 캐시 조회 =========
        public bool TryGetInput(string moduleName, string displayNo, out bool value)
        {
            var k = KeyOf(moduleName, displayNo);
            lock (_gate) { return _inputCache.TryGetValue(k, out value); }
        }

        public bool TryGetOutput(string moduleName, string displayNo, out bool value)
        {
            var k = KeyOf(moduleName, displayNo);
            lock (_gate) { return _outputCache.TryGetValue(k, out value); }
        }

        /// <summary>즉시 1회 스캔(필요 시 수동 호출)</summary>
        public void RefreshOnce()
        {
            ScanInputs();
            ScanOutputs();
        }

        // ========= 출력 제어(캐시 동기화) =========
        public int WriteOutput(string moduleName, string displayNo, bool logicalOn)
        {
            // 해당 채널 찾기
            ScanItem? found = null;
            for (int i = 0; i < _outputs.Length; i++)
            {
                if (_outputs[i].Key.Equals(KeyOf(moduleName, displayNo), StringComparison.OrdinalIgnoreCase))
                { found = _outputs[i]; break; }
            }
            if (!found.HasValue) return -1;

            // 8개씩 띄워져서 커지고 지랄하고 있음
            var s = found.Value;
            var raw = s.Reverse ? !logicalOn : logicalOn;
            var rc = _drv.WriteOutput(s.BoardNo, s.PortNo, s.ChannelNo, raw);
            if (rc == 0)
            {
                lock (_gate) { _outputCache[s.Key] = logicalOn; }
                var h = OutputChanged; if (h != null) h(moduleName, displayNo, logicalOn);
            }
            return rc;
        }

        public int PulseOutput(string moduleName, string displayNo, int widthMs)
        {
            ScanItem? found = null;
            for (int i = 0; i < _outputs.Length; i++)
            {
                if (_outputs[i].Key.Equals(KeyOf(moduleName, displayNo), StringComparison.OrdinalIgnoreCase))
                { found = _outputs[i]; break; }
            }
            if (!found.HasValue) return -1;

            var s = found.Value;
            return _drv.PulseOutput(s.BoardNo, s.PortNo, s.ChannelNo, widthMs < 0 ? 0 : widthMs);
        }

        // ========= 내부 루프 =========
        private void WorkLoop()
        {
            // 초기 1회 스캔
            RefreshOnce();

            
            while (_running)
            {
                ScanInputs();
                //ScanOutputs();
                _periodMs = 10;
                Thread.Sleep(_periodMs);
            }
        }

        private void ScanInputs()
        {
            // 변경 이벤트 모아서 락 밖에서 호출
            var changes = new List<ScanItem>();

            for (int i = 0; i < _inputs.Length; i++)
            {
                var s = _inputs[i];
                if (!s.Monitoring) continue;

                bool raw;
                if (_drv.ReadInput(s.BoardNo, s.PortNo, s.ChannelNo, out raw) != 0) continue;
                var logical = s.Reverse ? !raw : raw;

                bool changed = false;
                lock (_gate)
                {
                    bool prev;
                    if (!_inputCache.TryGetValue(s.Key, out prev) || prev != logical)
                    {
                        _inputCache[s.Key] = logical;
                        changed = true;
                    }
                }
                if (changed) changes.Add(s);
            }

            if (changes.Count > 0)
            {
                var h = InputChanged;
                if (h != null)
                {
                    for (int i = 0; i < changes.Count; i++)
                    {
                        var k = changes[i].Key;
                        // 분해: Module|DisplayNo
                        int sep = k.IndexOf('|');
                        var module = sep >= 0 ? k.Substring(0, sep) : k;
                        var disp = sep >= 0 ? k.Substring(sep + 1) : "";
                        bool v; if (!TryGetInput(module, disp, out v)) v = false;
                        h(module, disp, v);
                    }
                }
            }
        }

        private void ScanOutputs()
        {

            
            // 출력도 실제 보드 상태를 읽어 캐시에 반영(외부 변경 대응)
            for (int i = 0; i < _outputs.Length; i++)
            {
                var s = _outputs[i];
                if (!s.Monitoring) continue;

                bool raw;
                if (_drv.ReadOutput(s.BoardNo, s.PortNo, s.ChannelNo, out raw) != 0) continue;
                var logical = s.Reverse ? !raw : raw;

                bool changed = false;
                lock (_gate)
                {
                    bool prev;
                    if (!_outputCache.TryGetValue(s.Key, out prev) || prev != logical)
                    {
                        _outputCache[s.Key] = logical;
                        changed = true;
                    }
                }
                if (changed)
                {
                    var h = OutputChanged;
                    if (h != null)
                    {
                        int sep = s.Key.IndexOf('|');
                        var module = sep >= 0 ? s.Key.Substring(0, sep) : s.Key;
                        var disp = sep >= 0 ? s.Key.Substring(sep + 1) : "";
                        h(module, disp, logical);
                    }
                }
            }
        }

        private static string NormalizeDisp(string disp)
        {
            if (string.IsNullOrWhiteSpace(disp)) return disp;
            disp = disp.Trim().ToUpperInvariant();

            var m = System.Text.RegularExpressions.Regex.Match(disp, @"^(X|Y)\s*0*(\d+)$");
            if (!m.Success) m = System.Text.RegularExpressions.Regex.Match(disp, @"^(X|Y)\s*0*(\d+)\b");
            if (m.Success)
            {
                var letter = m.Groups[1].Value;
                var digits = m.Groups[2].Value;
                if (string.IsNullOrEmpty(digits)) digits = "0";
                return letter + digits; // 예: "Y00","Y003","y3" 모두 "Y3"로 통일
            }
            return disp;

            //if (string.IsNullOrWhiteSpace(disp)) return disp;
            //disp = disp.Trim().ToUpperInvariant();

            //// X/Y + 숫자 → 선행0 제거
            //var m = System.Text.RegularExpressions.Regex.Match(disp, @"^(X|Y)\s*0*(\d+)$");
            //if (!m.Success) m = System.Text.RegularExpressions.Regex.Match(disp, @"^(X|Y)\s*0*(\d+)\b");
            //if (m.Success)
            //{
            //    var letter = m.Groups[1].Value;     // X or Y
            //    var digits = m.Groups[2].Value;     // "0" 또는 "3" 등
            //    if (string.IsNullOrEmpty(digits)) digits = "0";
            //    return letter + digits;             // "Y3" (패딩 없음)
            //}
            //return disp; // 그 외 키는 원문(대문자/트림) 사용
        }

    }
}
