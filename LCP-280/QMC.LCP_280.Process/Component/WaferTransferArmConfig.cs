using QMC.Common;

namespace QMC.LCP_280.Process.Component
{
    public class WaferTransferArmConfig : BaseConfig
    {
        // 암 위치 설정
        public double WaferTransferArmReadyPosition { get; set; } = 15.0;
        public double WaferTransferArmAvoidPosition { get; set; } = 25.0;
        public double WaferTransferArmStagePosition { get; set; } = 30.0;
        public double WaferTransferArmCassettePosition { get; set; } = 35.0;

        public WaferTransferArmConfig() : base("WaferTransferArmConfig")
        {
            PropertyPosition.AddDoubleProperty(nameof(WaferTransferArmReadyPosition), WaferTransferArmReadyPosition);
            PropertyPosition.AddDoubleProperty(nameof(WaferTransferArmAvoidPosition), WaferTransferArmAvoidPosition);
            PropertyPosition.AddDoubleProperty(nameof(WaferTransferArmStagePosition), WaferTransferArmStagePosition);
            PropertyPosition.AddDoubleProperty(nameof(WaferTransferArmCassettePosition), WaferTransferArmCassettePosition);
        }

        public void SyncFromPropertyPosition()
        {
            if (PropertyPosition != null)
            {
                var WaferTransferArmReadyPos = PropertyPosition.GetPropertyByTitle(nameof(WaferTransferArmReadyPosition)) as DoubleProperty;
                if (WaferTransferArmReadyPos != null) WaferTransferArmReadyPosition = WaferTransferArmReadyPos.Value;

                var WaferTransferArmAvoidPos = PropertyPosition.GetPropertyByTitle(nameof(WaferTransferArmAvoidPosition)) as DoubleProperty;
                if (WaferTransferArmAvoidPos != null) WaferTransferArmAvoidPosition = WaferTransferArmAvoidPos.Value;

                var WaferTransferArmStagePos = PropertyPosition.GetPropertyByTitle(nameof(WaferTransferArmStagePosition)) as DoubleProperty;
                if (WaferTransferArmStagePos != null) WaferTransferArmStagePosition = WaferTransferArmStagePos.Value;

                var WaferTransferArmCassettePos = PropertyPosition.GetPropertyByTitle(nameof(WaferTransferArmCassettePosition)) as DoubleProperty;
                if (WaferTransferArmCassettePos != null) WaferTransferArmCassettePosition = WaferTransferArmCassettePos.Value;
            }
        }

        /// <summary>
        /// 🚀 실제 Config 값들을 PropertyPosition으로 동기화
        /// </summary>
        public void SyncToPropertyPosition()
        {
            if (PropertyPosition != null)
            {
                var WaferTransferArmReadyPos = PropertyPosition.GetPropertyByTitle(nameof(WaferTransferArmReadyPosition)) as DoubleProperty;
                if (WaferTransferArmReadyPos != null) WaferTransferArmReadyPos.Value = WaferTransferArmReadyPosition;

                var WaferTransferArmAvoidPos = PropertyPosition.GetPropertyByTitle(nameof(WaferTransferArmAvoidPosition)) as DoubleProperty;
                if (WaferTransferArmAvoidPos != null) WaferTransferArmAvoidPos.Value = WaferTransferArmAvoidPosition;

                var WaferTransferArmStagePos = PropertyPosition.GetPropertyByTitle(nameof(WaferTransferArmStagePosition)) as DoubleProperty;
                if (WaferTransferArmStagePos != null) WaferTransferArmStagePos.Value = WaferTransferArmStagePosition;

                var WaferTransferArmCassettePos = PropertyPosition.GetPropertyByTitle(nameof(WaferTransferArmCassettePosition)) as DoubleProperty;
                if (WaferTransferArmCassettePos != null) WaferTransferArmCassettePos.Value = WaferTransferArmCassettePosition;
            }
        }
        public override bool Validate()
        {
            return base.Validate();
        }

        public override void Reset()
        {
            base.Reset();
        }
    }
}