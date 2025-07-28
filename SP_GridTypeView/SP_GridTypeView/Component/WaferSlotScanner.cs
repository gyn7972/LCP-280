using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;

namespace SP_GridTypeView.Component
{
    public class WaferSlotScanner : BaseComponent
    {
        public CassetteDataConfig Config { get; }
        public CassetteData Cassette { get; }
        public CassetteElevator CassetteElevator { get; }

        public WaferSlotScanner() : base("WaferSlotScanner")
        {

        }

        public void ScanWaferSlots()
        {

        }
    }
}
