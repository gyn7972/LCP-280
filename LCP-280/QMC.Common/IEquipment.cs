// QMC.Common (MainForm가 포함된 어셈블리)
// IEquipment.cs
using QMC.Common.DIO;
using QMC.Common.IO;
using System;

namespace QMC.Common
{
    public interface IEquipment : IDisposable
    {
        System.Threading.Tasks.Task<bool> StopAllUnitsAsync();

        // 최소 공개 API만 신중히 추가:
        DioScanService DioScan { get; }
        DIOUnit UnitIO { get; }
    }

    public static class EquipmentLocator
    {
        private static IEquipment _instance;
        public static void Initialize(IEquipment instance) => _instance = instance;
        public static IEquipment Instance => _instance;
    }
}
