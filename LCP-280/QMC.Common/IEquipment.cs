// QMC.Common (MainForm가 포함된 어셈블리)
// IEquipment.cs
using QMC.Common.DIO;
using QMC.Common.IO;
using System;
using System.Diagnostics;

namespace QMC.Common
{
    /// <summary>
    /// 설비 상태
    /// </summary>
    public enum EquipmentState
    {
        Stopped,
        Initializing,
        Ready,
        Starting,
        Running,
        Stopping,
        Error
    }

    /// <summary>
    /// Unit 상태
    /// </summary>
    public enum UnitState
    {
        Stopped,
        Starting,
        Running,
        Stopping,
        Error,
        Unknown
    }

    public interface IEquipment : IDisposable
    {
        EquipmentState EqState { get; set; }

        System.Threading.Tasks.Task<bool> StopAllUnitsAsync(bool includeEquipmentStatus = true);
        System.Threading.Tasks.Task<bool> StopUnitAsync(string unitName);

        // 최소 공개 API만 신중히 추가:
        DioScanService DioScan { get; }
        DIOUnit UnitIO { get; }

    }

    public static class EquipmentLocator
    {
        private static IEquipment _instance;
        public static void Initialize(IEquipment instance) => _instance = instance;
        public static IEquipment Instance => _instance;

        public static bool IsInitialized => _instance != null;
        public static bool TryGet(out IEquipment eq)
        {
            eq = _instance;
            return eq != null;
        }
        public static void ShutdownAndDisposeSafely(int stopTimeoutMs = 8000)
        {
            if (_instance == null) return;
            try
            {
                var stopTask = _instance.StopAllUnitsAsync();
                if (!stopTask.Wait(stopTimeoutMs))
                {
                    // timeout -> 계속 진행하여 Dispose
                }
            }
            catch { /* 로그 필요시 추가 */ }
            finally
            {
                try { _instance.Dispose(); } catch { }
                _instance = null;

                //강제종료. Dispose에서 문제가 발생할 경우를 대비.
                Process.GetCurrentProcess().Kill();
            }
        }
    }
}
