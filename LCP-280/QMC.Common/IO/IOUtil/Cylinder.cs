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

        public CylinderConfig _config { get; set; }

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

        public bool Extend(int timeoutMs = 1000, int settleMs = 50)
        {
            DIO.Out(_bwdOutKey, false);
            DIO.Out(_fwdOutKey, true);

            int timeout = (timeoutMs != 0) ? timeoutMs : _config.ExtendTimeout;
            int settle = (settleMs != 0) ? settleMs : _config.SettleDelay;

            var sw = Stopwatch.StartNew();
            while (sw.ElapsedMilliseconds < timeout)
            {
                bool value = IsExtended();
                if (!value)
                {
                    return value;
                }
                Thread.Sleep(settle);
            }
            return false;
        }

        private bool IsExtended(int recursiveCount = 0)
        {
            try
            {
                if (DIO.In(_fwdInKey, out var on))
                {
                    return on;
                }
                else
                {
                    recursiveCount++;
                    if (recursiveCount >= _config.SensorRetryCount) return false;
                    return !IsRetacted();
                }
            }catch(Exception ex)
            {
                
            }
            return false;
        }

        private bool IsRetacted(int recursiveCount = 0)
        {
            try
            {
                if (DIO.In(_fwdInKey, out var on))
                {
                    return on;
                }
                else
                {
                    recursiveCount++;
                    if (recursiveCount >= _config.SensorRetryCount) return false;

                    return !IsExtended(recursiveCount);
                }
            }
            catch (Exception ex)
            {

            }
            return false;
        }

        public bool Retract(int timeoutMs = 1000, int settleMs = 50)
        {
            DIO.Out(_fwdOutKey, false);
            DIO.Out(_bwdOutKey, true);

            int timeout = (timeoutMs != 0) ? timeoutMs : _config.RetractTimeout;
            int settle = (settleMs != 0) ? settleMs : _config.SettleDelay;

            var sw = Stopwatch.StartNew();
            while (sw.ElapsedMilliseconds < timeout)
            {
                bool value = IsRetacted();
                if (!value)
                {
                    return value;
                }
                Thread.Sleep(settle);
            }
            return false;

            //var sw = Stopwatch.StartNew();
            //while (sw.ElapsedMilliseconds < timeout)
            //{
            //    if (DIO.In(_bwdInKey, out var on) && on) 
            //    { 
            //        Thread.Sleep(settle); 
            //        return true; 
            //    }
            //    Thread.Sleep(5);
            //}
            //return false;
        }

        public void AllOff() { DIO.Out(_fwdOutKey, false); DIO.Out(_bwdOutKey, false); }
    }
}
