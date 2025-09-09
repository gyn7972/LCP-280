using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace QMC.LCP_280.Process.Component
{
    [Serializable]
    public sealed class WaferData
    {
        // ===== Identification =====
        [DefaultValue("")] public string LotId { get; set; } = "";
        [DefaultValue("")] public string LotDate { get; set; } = DateTime.Now.ToString("yyyyMMdd");

        [DefaultValue("")] public string CarrierId { get; set; } = "";
        [DefaultValue(-1)] public int SlotIndex { get; set; } = -1;

        // ===== Recipe Keys (모든 Chip 공통 검사 Key) =====
        public List<string> RecipeKeys { get; set; } = new List<string>();

        // ===== Wafer Info =====
        public WaferSummary Summary { get; set; } = new WaferSummary();

        // ===== Chip Data =====
        public List<ChipData> Chips { get; set; } = new List<ChipData>();

        // ===== Reset =====
        public void Reset()
        {
            LotId = "";
            LotDate = DateTime.Now.ToString("yyyyMMdd");
            CarrierId = "";
            SlotIndex = -1;
            Summary = new WaferSummary();
            RecipeKeys.Clear();
            Chips.Clear();
        }

        // ===== Chip 관리 함수 =====
        public ChipData AddChip(int index, int mapX, int mapY)
        {
            var chip = new ChipData
            {
                Index = index,
                MapX = mapX,
                MapY = mapY,
                Exists = true,
                State = ChipProcessState.Mapped,
                SourceWaferId = LotId
            };

            foreach (var key in RecipeKeys)
                chip.AddMeasure(key, double.NaN);

            Chips.Add(chip);
            return chip;
        }

        public ChipData GetChipByIndex(int index) =>
            Chips.FirstOrDefault(c => c.Index == index);

        public ChipData GetChipByMap(int x, int y) =>
            Chips.FirstOrDefault(c => c.MapX == x && c.MapY == y);
    }
}
