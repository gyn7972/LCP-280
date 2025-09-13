using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace QMC.LCP_280.Process.Component
{
    [Serializable]
    public sealed class CassetteMaterial : QMC.Common.Material
    {
        public enum CassetteProcessSatate : byte
        {
            Unknown = byte.MaxValue,
            Ready = 0,
            Processing = 1,
            Completed = 2,
        }
        [DefaultValue("")] public string CarrierId { get; set; } = "";
        [DefaultValue(25)] public int SlotCount { get; set; } = 25;

        public List<MaterialWafer> Slots { get; set; } = new List<MaterialWafer>();

        public CassetteMaterial()
        {
            Slots = Enumerable.Repeat<MaterialWafer>(null, SlotCount).ToList();
            //for (int i = 0; i < SlotCount; i++)
            //    Slots.Add(null);
            ProcessSatate = CassetteProcessSatate.Unknown;

        }

        public CassetteProcessSatate ProcessSatate { get; set; } = CassetteProcessSatate.Unknown;

        public MaterialWafer GetWafer(int slot)
        {
            if (slot < 0 || slot >= Slots.Count) return null;
            return Slots[slot];
        }

        public void SetWafer(int slot, MaterialWafer wafer)
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
