using System.Collections.Generic;

namespace LCP_280
{
    public enum CassetteState
    {
        Empty,      // 비어있는
        Present,    // 존재하는
        Processing, // 작업중
        Completed   // 작업완료
    }

    public class CassetteData
    {
        public int CassetteIndex { get; set; }
        public string CassetteId { get; set; }
        public List<WaferData> WaferList { get; } = new List<WaferData>();
        public CassetteState State { get; set; } = CassetteState.Empty; // 상태 변수 추가
        // 필요에 따라 위치, 타입 등 추가 가능

        public void GenerateWaferData(int slotCount)
        {
            var waferList = new List<WaferData>();
            for (int i = 0; i < slotCount; i++)
            {
                var wafer = new WaferData(slotCount)
                {
                    WaferId = $"W{i + 1}",
                    WaferName = $"Wafer_{i + 1}",
                    SlotNo = i,
                    SlotStates = new WaferCassetteLoadState[slotCount]
                    // waferDataConfig 등 필요한 값 추가 가능
                };
                waferList.Add(wafer);
            }

            WaferList.AddRange(waferList);
        }
    }
}