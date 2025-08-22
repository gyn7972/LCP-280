using QMC.Common;
using QMC.Common.Component;
using System;

namespace QMC.LCP_280.Process.Component
{
    public class CassetteElevator : BaseComponent
    {
        private readonly AxisManager _axisManager = new AxisManager();
        public System.Collections.Generic.IReadOnlyList<AxisDefinition> Axes { get { return _axisManager.Axes; } }

        public AxisDefinition CassetteElevatorZ { get; private set; }
        public CassetteElevatorConfig CassetteElevatorConfig { get; private set; }

        public CassetteElevator(CassetteElevatorConfig config = null)
            : base("CassetteElevator")
        {
            CassetteElevatorConfig = config ?? new CassetteElevatorConfig();
        }

        public override void InitializeAxes(params IMotionAxis[] axes)
        {
            _axisManager.Clear();
            CassetteElevatorZ = null;

            var zAxis = AxisResolver.Resolve("Z", axes,
                "CassetteElevatorZ", "CassetteZ", "Z1");

            if (zAxis == null)
            {
                LogAxisError("Z 축을 찾지 못했습니다. 이름 규칙(Z / CassetteElevatorZ / CassetteZ / Z1) 확인.");
                return;
            }

            CassetteElevatorZ = _axisManager.Register("Z", "CassetteElevator Z Axis", zAxis);
            BuildPositionItemsFromConfig();
        }

        protected override void BuildPositionItemsFromConfig()
        {
            if (CassetteElevatorZ == null) return;

            CassetteElevatorZ.PositionItems.Clear();

            CassetteElevatorZ.CreatePositionItem("CassetteElevator Loading Position",
                CassetteElevatorConfig.LoadingZ);
            CassetteElevatorZ.CreatePositionItem("CassetteElevator Unloading Position",
                CassetteElevatorConfig.UnloadingZ);
            CassetteElevatorZ.CreatePositionItem("CassetteElevator Ready Position",
                CassetteElevatorConfig.ReadyZ);
        }

        public override void SyncToConfig()
        {
            if (CassetteElevatorZ == null) return;

            foreach (var item in CassetteElevatorZ.PositionItems)
            {
                var posProp = item.GetDoubleProperties()
                                  .Find(p => p.Title == CassetteElevatorZ.MotionAxis.Name);
                if (posProp == null) continue;

                var title = item.Title;
                if (title.IndexOf("Loading", StringComparison.OrdinalIgnoreCase) >= 0)
                    CassetteElevatorConfig.LoadingZ = posProp.Value;
                else if (title.IndexOf("Unloading", StringComparison.OrdinalIgnoreCase) >= 0)
                    CassetteElevatorConfig.UnloadingZ = posProp.Value;
                else if (title.IndexOf("Ready", StringComparison.OrdinalIgnoreCase) >= 0)
                    CassetteElevatorConfig.ReadyZ = posProp.Value;
            }
        }

        public override void ReloadFromConfig()
        {
            BuildPositionItemsFromConfig();
        }

        private void LogAxisError(string msg)
        {
            Console.WriteLine("[CassetteElevator][AxisInit] " + msg);
        }

        // 필요 시 외부에서 축 검색
        public AxisDefinition FindAxis(string axisKeyOrName)
        {
            return _axisManager.Find(axisKeyOrName);
        }
    }
}