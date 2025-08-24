using QMC.Common;
using QMC.Common.Component;
using QMC.Common.Motion;
using System;

namespace QMC.LCP_280.Process.Component
{
    public class WaferTransferArm : BaseComponent
    {
        private readonly AxisManager _axisManager = new AxisManager();
        public System.Collections.Generic.IReadOnlyList<AxisDefinition> Axes { get { return _axisManager.Axes; } }

        public AxisDefinition WaferTransferArmY { get; private set; }
        public WaferTransferArmConfig WaferTransferArmConfig { get; private set; }

        public WaferTransferArm(WaferTransferArmConfig config = null)
            : base("WaferTransferArm")
        {
            WaferTransferArmConfig = config ?? new WaferTransferArmConfig();
        }

        public override void InitializeAxes(params IMotionAxis[] axes)
        {
            _axisManager.Clear();
            WaferTransferArmY = null;

            var yAxis = AxisResolver.Resolve("Y", axes,
                "WaferTransferArmY", "ArmY", "Y1");

            if (yAxis == null)
            {
                LogAxisError("Y 축을 찾지 못했습니다. 이름 규칙(Y / WaferTransferArmY / ArmY / Y1) 확인.");
                return;
            }

            WaferTransferArmY = _axisManager.Register("Y", "WaferTransferArm Y Axis", yAxis);
            BuildPositionItemsFromConfig();
        }

        protected override void BuildPositionItemsFromConfig()
        {
            if (WaferTransferArmY == null) return;
            WaferTransferArmY.PositionItems.Clear();

            WaferTransferArmY.CreatePositionItem("WaferTransferArm Ready Position",
                WaferTransferArmConfig.ReadyY);
            WaferTransferArmY.CreatePositionItem("WaferTransferArm Avoid Position",
                WaferTransferArmConfig.AvoidY);
            WaferTransferArmY.CreatePositionItem("WaferTransferArm Stage Position",
                WaferTransferArmConfig.StageY);
            WaferTransferArmY.CreatePositionItem("WaferTransferArm Cassette Position",
                WaferTransferArmConfig.CassetteY);
        }

        public override void SyncToConfig()
        {
            if (WaferTransferArmY == null) return;
            foreach (var item in WaferTransferArmY.PositionItems)
            {
                var posProp = item.GetDoubleProperties()
                                  .Find(p => p.Title == WaferTransferArmY.MotionAxis.Name);
                if (posProp == null) continue;

                var t = item.Title;
                if (t.IndexOf("Ready", StringComparison.OrdinalIgnoreCase) >= 0)
                    WaferTransferArmConfig.ReadyY = posProp.Value;
                else if (t.IndexOf("Avoid", StringComparison.OrdinalIgnoreCase) >= 0)
                    WaferTransferArmConfig.AvoidY = posProp.Value;
                else if (t.IndexOf("Stage", StringComparison.OrdinalIgnoreCase) >= 0)
                    WaferTransferArmConfig.StageY = posProp.Value;
                else if (t.IndexOf("Cassette", StringComparison.OrdinalIgnoreCase) >= 0)
                    WaferTransferArmConfig.CassetteY = posProp.Value;
            }
        }

        public override void ReloadFromConfig()
        {
            BuildPositionItemsFromConfig();
        }

        private void LogAxisError(string msg)
        {
            Console.WriteLine("[WaferTransferArm][AxisInit] " + msg);
        }

        public AxisDefinition FindAxis(string axisKeyOrName)
        {
            return _axisManager.Find(axisKeyOrName);
        }
    }
}