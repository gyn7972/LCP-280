using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP_GridTypeView
{
    public enum WaferCassetteLoadState
    {
        Empty,             // 비어있는
        Present,           // 존재하는
        Loading,           // 로딩중
        Loaded,            // 로딩완료
        Processing,        // 작업중
        Processed,         // 작업완료
        Unloading,         // 업로딩중
        Unloaded,          // 언로딩완료
        BarcodeReading,    // 바코드 리딩 중
        BarcodeRead,       // 바코드 리딩 완료
        BarcodeReadFail    // 바코드 리딩 실패
    }

    public class WaferData
    {
        public string WaferId { get; set; }
        public string WaferName { get; set; }
        public int SlotNo { get; set; }
        public WaferCassetteLoadState[] SlotStates { get; set; }

        public WaferData(int slotCount)
        {
            SlotStates = new WaferCassetteLoadState[slotCount];
            for (int i = 0; i < slotCount; i++)
            {
                SlotStates[i] = WaferCassetteLoadState.Empty; // 초기 상태는 비어있는 것으로 설정
            }
        }
    }
}