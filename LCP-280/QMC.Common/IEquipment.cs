// QMC.Common (MainForm가 포함된 어셈블리)
// IEquipment.cs
using System;

namespace QMC.Common
{
    public interface IEquipment : IDisposable
    {
        System.Threading.Tasks.Task<bool> StopAllUnitsAsync();
        // 필요한 공개 API만 최소한으로…
    }

    public static class EquipmentLocator
    {
        private static IEquipment _instance;
        public static void Initialize(IEquipment instance) => _instance = instance;
        public static IEquipment Instance => _instance;
    }
}
