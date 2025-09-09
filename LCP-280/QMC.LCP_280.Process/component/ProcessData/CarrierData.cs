using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace QMC.LCP_280.Process.Component
{
    [Serializable]
    public sealed class CarrierData
    {
        [DefaultValue("")] public string CarrierId { get; set; } = "";
        [DefaultValue(25)] public int SlotCount { get; set; } = 25;

        public List<WaferData> Slots { get; set; } = new List<WaferData>();

        public CarrierData()
        {
            Slots = Enumerable.Repeat<WaferData>(null, SlotCount).ToList();
            //for (int i = 0; i < SlotCount; i++)
            //    Slots.Add(null);
        }

        public WaferData GetWafer(int slot)
        {
            if (slot < 0 || slot >= Slots.Count) return null;
            return Slots[slot];
        }

        public void SetWafer(int slot, WaferData wafer)
        {
            if (slot < 0 || slot >= Slots.Count) return;
            wafer.CarrierId = CarrierId;
            wafer.SlotIndex = slot;
            Slots[slot] = wafer;
        }

        public void RemoveWafer(int slot)
        {
            if (slot < 0 || slot >= Slots.Count) return;
            Slots[slot] = null;
        }
    }
}
