// QMC.Common (MainForm가 포함된 어셈블리)
// IEquipment.cs
using QMC.Common.DIO;
using QMC.Common.IO;
using System;
using System.Diagnostics;
using static QMC.Common.Unit.BaseUnit;

namespace QMC.Common
{
    /// <summary>
    /// 설비 상태
    /// </summary>
    public enum EquipmentState
    {
        Unknown,
        Stopped,
        Initializing,
        Ready,
        Starting,
        AutoRunning,
        ManualRunning,
        Stopping,
        CycleStop,
        Reset,
        Error
    }


    public interface IEquipment : IDisposable
    {
        EquipmentState EqState { get; set; }

        System.Threading.Tasks.Task<bool> StopAllUnitsAsync(bool includeEquipmentStatus = false);

        System.Threading.Tasks.Task<bool> TerminateAllUnitsAsync();
        System.Threading.Tasks.Task<bool> StopUnitAsync(string unitName);

        // 최소 공개 API만 신중히 추가:
        DioScanService DioScan { get; }
        DIOUnit UnitIO { get; }

        // 인터페이스에서는 구현 없이 선언만(C# 7.3 호환)
        string ICurrentRecipe { get; set; }
        bool m_bBuzzerOff { get; set; }

        void SetAndRaiseUnitState(string unitName, UnitStatus newState);
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

        //public bool m_bBuzzerOff = false;

        // C# 7.3 호환: 글로벌 접근 편의를 위한 정적 프록시
        public static string CurrentRecipe
        {
            get
            {
                if (_instance == null) 
                    return null;
                return _instance.ICurrentRecipe;
            }
            set
            {
                if (_instance != null)
                    _instance.ICurrentRecipe = value;
            }
        }

        public static void ShutdownAndDisposeSafely(int stopTimeoutMs = 8000)
        {
            if (_instance == null) return;
            try
            {
                var stopTask = _instance.StopAllUnitsAsync();
                var terminateTask = _instance.TerminateAllUnitsAsync();
                if (!stopTask.Wait(stopTimeoutMs))
                {
                    // timeout -> 계속 진행하여 Dispose
                }

                terminateTask.Wait(stopTimeoutMs);

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
