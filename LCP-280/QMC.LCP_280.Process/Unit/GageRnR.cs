using QMC.Common.Unit;
using QMC.Common;
using QMC.Common.Component;
using QMC.Common.IOUtil;
using QMC.Common.Motion;
using QMC.Common.Motions;
using QMC.Common.Unit;
using QMC.LCP_280.Process.Component;
using System.Collections.Generic;
using System;

namespace QMC.LCP_280.Process.Unit
{
    public class GageRnR : BaseUnit<GageRnRConfig>
    {

        

        public GageRnR(GageRnRConfig config = null)
            : base(new GageRnRConfig())
        {   
            AddComponents();
        }

        public override void AddComponents()
        {
        }

        public override int OnRun()
        {
            int ret = 0;
            return ret;
        }

        public override int OnStop()
        {
            int ret = 0;
            this.RunUnitStatus = UnitStatus.Stopped;
            this.State = ProcessState.Stop;
            base.OnStop();
            return ret;
        }

        protected override int OnRunReady() { return 0; }
        protected override int OnRunWork() { return 0; }
        protected override int OnRunComplete() { return 0; }

    }
}
