using QMC.Common;
using QMC.Common.Component;
using System;

namespace QMC.LCP_280.Process.Component
{
    public class CassetteElevatorConfig : BaseConfig
    {
        // Z축 위치 설정
        public double LifterZLoadingPosition { get; set; } = 100.0;
        public double LifterZUnloadingPosition { get; set; } = 10.0;
        public double CassetteSlotPitch { get; set; } = 20.0;
        public double FeederReadyPosition { get; set; } = 15.0;
        public double FeederAvoidPosition { get; set; } = 25.0;
        public double FeederStagePosition { get; set; } = 30.0;
        public double FeederCassettePosition { get; set; } = 35.0;


        public CassetteElevatorConfig() : base("CassetteElevatorConfig")
        {
            // 🚀 각 Position들을 PropertyPosition에 DoubleProperty로 추가
            PropertyPosition.AddDoubleProperty(nameof(LifterZLoadingPosition), LifterZLoadingPosition);
            PropertyPosition.AddDoubleProperty(nameof(LifterZUnloadingPosition), LifterZUnloadingPosition);
            PropertyPosition.AddDoubleProperty(nameof(CassetteSlotPitch), CassetteSlotPitch);
            PropertyPosition.AddDoubleProperty(nameof(FeederReadyPosition), FeederReadyPosition);
            PropertyPosition.AddDoubleProperty(nameof(FeederAvoidPosition), FeederAvoidPosition);
            PropertyPosition.AddDoubleProperty(nameof(FeederStagePosition), FeederStagePosition);
            PropertyPosition.AddDoubleProperty(nameof(FeederCassettePosition), FeederCassettePosition);
        }

        /// <summary>
        /// 🚀 PropertyPosition에서 실제 Config 값들로 동기화
        /// </summary>
        public void SyncFromPropertyPosition()
        {
            if (PropertyPosition != null)
            {
                var lifterZLoadingPos = PropertyPosition.GetPropertyByTitle(nameof(LifterZLoadingPosition)) as DoubleProperty;
                if (lifterZLoadingPos != null) LifterZLoadingPosition = lifterZLoadingPos.Value;

                var lifterZUnloadingPos = PropertyPosition.GetPropertyByTitle(nameof(LifterZUnloadingPosition)) as DoubleProperty;
                if (lifterZUnloadingPos != null) LifterZUnloadingPosition = lifterZUnloadingPos.Value;

                var cassetteSlotPitch = PropertyPosition.GetPropertyByTitle(nameof(CassetteSlotPitch)) as DoubleProperty;
                if (cassetteSlotPitch != null) CassetteSlotPitch = cassetteSlotPitch.Value;

                var feederReadyPos = PropertyPosition.GetPropertyByTitle(nameof(FeederReadyPosition)) as DoubleProperty;
                if (feederReadyPos != null) FeederReadyPosition = feederReadyPos.Value;

                var feederAvoidPos = PropertyPosition.GetPropertyByTitle(nameof(FeederAvoidPosition)) as DoubleProperty;
                if (feederAvoidPos != null) FeederAvoidPosition = feederAvoidPos.Value;

                var feederStagePos = PropertyPosition.GetPropertyByTitle(nameof(FeederStagePosition)) as DoubleProperty;
                if (feederStagePos != null) FeederStagePosition = feederStagePos.Value;

                var feederCassettePos = PropertyPosition.GetPropertyByTitle(nameof(FeederCassettePosition)) as DoubleProperty;
                if (feederCassettePos != null) FeederCassettePosition = feederCassettePos.Value;
            }
        }

        /// <summary>
        /// 🚀 실제 Config 값들을 PropertyPosition으로 동기화
        /// </summary>
        public void SyncToPropertyPosition()
        {
            if (PropertyPosition != null)
            {
                var lifterZLoadingPos = PropertyPosition.GetPropertyByTitle(nameof(LifterZLoadingPosition)) as DoubleProperty;
                if (lifterZLoadingPos != null) lifterZLoadingPos.Value = LifterZLoadingPosition;

                var lifterZUnloadingPos = PropertyPosition.GetPropertyByTitle(nameof(LifterZUnloadingPosition)) as DoubleProperty;
                if (lifterZUnloadingPos != null) lifterZUnloadingPos.Value = LifterZUnloadingPosition;

                var cassetteSlotPitch = PropertyPosition.GetPropertyByTitle(nameof(CassetteSlotPitch)) as DoubleProperty;
                if (cassetteSlotPitch != null) cassetteSlotPitch.Value = CassetteSlotPitch;

                var feederReadyPos = PropertyPosition.GetPropertyByTitle(nameof(FeederReadyPosition)) as DoubleProperty;
                if (feederReadyPos != null) feederReadyPos.Value = FeederReadyPosition;

                var feederAvoidPos = PropertyPosition.GetPropertyByTitle(nameof(FeederAvoidPosition)) as DoubleProperty;
                if (feederAvoidPos != null) feederAvoidPos.Value = FeederAvoidPosition;

                var feederStagePos = PropertyPosition.GetPropertyByTitle(nameof(FeederStagePosition)) as DoubleProperty;
                if (feederStagePos != null) feederStagePos.Value = FeederStagePosition;

                var feederCassettePos = PropertyPosition.GetPropertyByTitle(nameof(FeederCassettePosition)) as DoubleProperty;
                if (feederCassettePos != null) feederCassettePos.Value = FeederCassettePosition;
            }
        }

        public override bool Validate()
        {
            return base.Validate();
        }

        public override void Reset()
        {
            SyncToPropertyPosition();

            base.Reset();
        }
    }
}
