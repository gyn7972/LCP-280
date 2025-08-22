using QMC.Common;
using QMC.Common.Component;
using QMC.Common.Logging;
using System;

namespace QMC.LCP_280.Process.Component
{
    public class CassetteElevator : BaseComponent
    {
        private readonly AxisManager _axisManager = new AxisManager();
        private readonly ILogger _log = new ConsoleLogger(nameof(CassetteElevator));
        public System.Collections.Generic.IReadOnlyList<AxisDefinition> Axes { get { return _axisManager.Axes; } }

        public AxisDefinition CassetteElevatorZ { get; private set; }
        public CassetteElevatorConfig CassetteElevatorConfig { get; private set; }

        // 강한 Position Key 상수 정의
        private const string PosKeyLoading = "Loading";
        private const string PosKeyUnloading = "Unloading";
        private const string PosKeyReady = "Ready";

        public CassetteElevator(CassetteElevatorConfig config = null)
            : base("CassetteElevator")
        {
            CassetteElevatorConfig = config ?? new CassetteElevatorConfig();
        }

        public override void InitializeAxes(params IMotionAxis[] axes)
        {
            _axisManager.Clear();
            CassetteElevatorZ = null;

            // AxisNameRegistry 기반 해석으로 변경 (필요 시 외부에서 alias 구성)
            var zAxis = AxisNameRegistry.Resolve("Z", axes);
            if (zAxis == null)
            {
                // 레거시 규칙도 함께 시도 (점진적 마이그레이션)
                zAxis = AxisResolver.Resolve("Z", axes, "CassetteElevatorZ", "CassetteZ", "Z1");
            }

            if (zAxis == null)
            {
                _log.Error("Z 축을 찾지 못했습니다. AxisNameRegistry 설정을 확인하세요.");
                return;
            }

            CassetteElevatorZ = _axisManager.Register("Z", "CassetteElevator Z Axis", zAxis);
            _log.Info($"Z axis registered: {zAxis.Name}");
            BuildPositionItemsFromConfig();
        }

        protected override void BuildPositionItemsFromConfig()
        {
            if (CassetteElevatorZ == null) return;

            CassetteElevatorZ.PositionItems.Clear();

            // 강한 키를 사용하여 Position 생성 // Set-Up 화면에서 축별 이동 속도 확인
            CassetteElevatorZ.CreatePositionItem(PosKeyLoading,
                "CassetteElevator Loading Position",
                CassetteElevatorConfig.LoadingZ, 50, 500, 500, 3000);

            CassetteElevatorZ.CreatePositionItem(PosKeyUnloading,
                "CassetteElevator Unloading Position",
                CassetteElevatorConfig.UnloadingZ, 50, 500, 500, 3000);

            CassetteElevatorZ.CreatePositionItem(PosKeyReady,
                "CassetteElevator Ready Position",
                CassetteElevatorConfig.ReadyZ, 50, 500, 500, 3000);

            _log.Debug("Position items built from config.");
        }

        public override void SyncToConfig()
        {
            if (CassetteElevatorZ == null) return;

            // 축 이름으로 위치값 속성 추출
            string axisName = CassetteElevatorZ.MotionAxis.Name;

            var loadingItem = CassetteElevatorZ.GetPositionItemByKey(PosKeyLoading);
            var unloadingItem = CassetteElevatorZ.GetPositionItemByKey(PosKeyUnloading);
            var readyItem = CassetteElevatorZ.GetPositionItemByKey(PosKeyReady);

            var dpLoading = loadingItem?.GetDoubleProperty(axisName);
            var dpUnloading = unloadingItem?.GetDoubleProperty(axisName);
            var dpReady = readyItem?.GetDoubleProperty(axisName);

            if (dpLoading != null) CassetteElevatorConfig.LoadingZ = dpLoading.Value;
            if (dpUnloading != null) CassetteElevatorConfig.UnloadingZ = dpUnloading.Value;
            if (dpReady != null) CassetteElevatorConfig.ReadyZ = dpReady.Value;

            _log.Info("CassetteElevator config synced from UI positions.");
        }

        public override void ReloadFromConfig()
        {
            BuildPositionItemsFromConfig();
        }

        // 필요 시 외부에서 축 검색
        public AxisDefinition FindAxis(string axisKeyOrName)
        {
            return _axisManager.Find(axisKeyOrName);
        }
    }
}