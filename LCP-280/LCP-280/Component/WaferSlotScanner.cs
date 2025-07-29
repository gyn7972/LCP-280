using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;

namespace SP_GridTypeView.Component
{
    public class WaferSlotScanner : BaseComponent
    {
        public WaferSlotScannerConfig Config { get; private set; }

        public WaferSlotScanner(WaferSlotScannerConfig config = null) : base("WaferSlotScanner")
        {
            Config = config ?? new WaferSlotScannerConfig();
        }

        public void ScanWaferSlots()
        {
            // Config의 설정값들을 사용하여 스캔 수행
            // 예: Config.SlotCount, Config.ScanSpeed 등 사용
        }

        public bool IsWaferPresent(int slotIndex)
        {
            // Config의 SensorThreshold를 사용하여 웨이퍼 유무 판단
            // 실제 센서 값과 Config.SensorThreshold 비교
            return false; // 임시 반환값
        }
    }
}
