// Cylinder.cs
using QMC.Common.Component;
using System;
using System.Diagnostics;
using System.Threading;

namespace QMC.Common.IOUtil
{
    /// <summary>2솔 밸브 + 2센서(UP/DOWN or FWD/BWD) 전형</summary>
    public sealed class Cylinder : BaseComponent
    {
        private readonly string _fwdOutKey, _bwdOutKey, _fwdInKey, _bwdInKey;

        // UI 빠른 반영(명령 후 즉시 응답) 윈도우(ms)
        public int EarlyAcknowledgeMs { get; set; } = 150;

        // 마지막 명령 기록
        private volatile bool _lastCommandExtend;
        private long _lastCommandTicks;

        public CylinderConfig _config { get; set; }

        public new CylinderConfig Config
        {
            get => _config;
            set
            {
                if (value == null) throw new ArgumentNullException(nameof(value));
                _config = value;
            }
        }

        // 키 직접 접근 프로퍼티 (리플렉션 제거 목적)
        public string FwdOutKey => _fwdOutKey;
        public string BwdOutKey => _bwdOutKey;
        public string FwdInKey => _fwdInKey;
        public string BwdInKey => _bwdInKey;

        public Cylinder(string name, string fwdOutKey, string bwdOutKey, string fwdInKey, string bwdInKey, CylinderConfig config = null)
        {
            Name = name;
            _fwdOutKey = fwdOutKey;
            _bwdOutKey = bwdOutKey;
            _fwdInKey = fwdInKey;
            _bwdInKey = bwdInKey;
            _config = config ?? new CylinderConfig { Name = name };
        }

        public bool Extend(int timeoutMs = 1500, int settleMs = 2)
        {
            DIO.Out(_bwdOutKey, false);
            DIO.Out(_fwdOutKey, true);

            InterlockEventArgs args = new InterlockEventArgs();
            args.IsExtend = true;
            if(OnIsInterlockOK(args) == false)
            {
                return false;
            }

            _lastCommandExtend = true;
            _lastCommandTicks = Stopwatch.GetTimestamp();

            int timeout = (timeoutMs != 0) ? timeoutMs : _config.ExtendTimeout;
            int settle = (settleMs != 0) ? settleMs : _config.SettleDelay;

            if (this.Config.IsSimulation)
            {
                Thread.Sleep(1);
                return true;
            }

            var sw = Stopwatch.StartNew();
            while (sw.ElapsedMilliseconds < timeout)
            {
                if (IsExtendedConfirmed())
                    return true;
                Thread.Sleep(settle);
            }
            return false;
        }

        public bool Retract(int timeoutMs = 1500, int settleMs = 2)
        {
            DIO.Out(_fwdOutKey, false);
            DIO.Out(_bwdOutKey, true);

            InterlockEventArgs args = new InterlockEventArgs();
            args.IsExtend = false;
            if (OnIsInterlockOK(args) == false)
            {

                return false;
            }

            _lastCommandExtend = false;
            _lastCommandTicks = Stopwatch.GetTimestamp();

            int timeout = (timeoutMs != 0) ? timeoutMs : _config.RetractTimeout;
            int settle = (settleMs != 0) ? settleMs : _config.SettleDelay;

            if (this.Config.IsSimulation)
            {
                Thread.Sleep(5);
                return true;
            }

            var sw = Stopwatch.StartNew();
            while (sw.ElapsedMilliseconds < timeout)
            {
                if (IsRetractedConfirmed())
                    return true;
                Thread.Sleep(settle);
            }
            return false;
        }

        public void AllOff()
        {
            DIO.Out(_fwdOutKey, false);
            DIO.Out(_bwdOutKey, false);
        }

        // ================== 단일/이중 센서 판정 ==================

        // UI용: 빠른 표시(명령 직후 Early ACK + 센서 추론)
        public bool IsExtendedFastForUi()
        {
            // 1) 확정 센서 판정이 가능하면 그대로 사용
            if (IsExtendedConfirmed()) return true;

            // 2) 단일 센서거나 읽기 실패 시, 명령 직후 Early ACK 기간에는 명령값을 그대로 반영
            if (_lastCommandExtend && ElapsedSinceLastCommandMs() <= EarlyAcknowledgeMs)
                return true;

            return false;
        }

        public bool IsRetractedFastForUi()
        {
            if (IsRetractedConfirmed()) return true;

            if (!_lastCommandExtend && ElapsedSinceLastCommandMs() <= EarlyAcknowledgeMs)
                return true;

            return false;
        }

        // 확정 판정: 실제 센서 기준(가능하면 두 센서, 하나면 추론)
        private bool IsExtendedConfirmed()
        {
            try
            {
                bool hasFwd = !string.IsNullOrEmpty(_fwdInKey);
                bool hasBwd = !string.IsNullOrEmpty(_bwdInKey);

                bool on;
                // 두 센서가 모두 있으면: FWD=ON && BWD=OFF를 우선
                if (hasFwd && hasBwd)
                {
                    bool fwdOk = DIO.In(_fwdInKey, out var fwdOn);
                    bool bwdOk = DIO.In(_bwdInKey, out var bwdOn);
                    if (fwdOk && bwdOk)
                        return fwdOn && !bwdOn;
                    if (fwdOk) return fwdOn;
                    if (bwdOk) return !bwdOn; // 단일 센서 추론
                    return false;
                }

                // 단일 센서: FWD만 있으면 그대로, BWD만 있으면 부정으로 추론
                if (hasFwd && DIO.In(_fwdInKey, out on)) return on;
                if (hasBwd && DIO.In(_bwdInKey, out on)) return !on;
            }
            catch { /* 로그는 상위에서 처리 */ }

            return false;
        }

        private bool IsRetractedConfirmed()
        {
            try
            {
                bool hasFwd = !string.IsNullOrEmpty(_fwdInKey);
                bool hasBwd = !string.IsNullOrEmpty(_bwdInKey);

                bool on;
                if (hasFwd && hasBwd)
                {
                    bool fwdOk = DIO.In(_fwdInKey, out var fwdOn);
                    bool bwdOk = DIO.In(_bwdInKey, out var bwdOn);
                    if (fwdOk && bwdOk)
                        return !fwdOn && bwdOn;
                    if (bwdOk) return bwdOn;
                    if (fwdOk) return !fwdOn; // 단일 센서 추론
                    return false;
                }

                if (hasBwd && DIO.In(_bwdInKey, out on)) return on;
                if (hasFwd && DIO.In(_fwdInKey, out on)) return !on;
            }
            catch { }

            return false;
        }

        private int ElapsedSinceLastCommandMs()
        {
            long ticks = Stopwatch.GetTimestamp() - _lastCommandTicks;
            // ticks -> ms
            double ms = (ticks * 1000.0) / Stopwatch.Frequency;
            return (int)ms;
        }
    }
}