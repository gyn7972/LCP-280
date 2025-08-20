using QMC.Common;
using QMC.Common.Component;
using System;

namespace QMC.LCP_280.Process.Component
{
    public class CassetteElevatorConfig : BaseConfig
    {
        // Z축 위치 설정
        public double CassetteElevatorLoadingPosition { get; set; } = 100.0;
        public double CassetteElevatorUnloadingPosition { get; set; } = 10.0;
        public double CassetteSlotPitch { get; set; } = 20.0;
      


        public CassetteElevatorConfig() : base("CassetteElevatorConfig")
        {
            // 🚀 각 Position들을 PropertyPosition에 DoubleProperty로 추가
            PropertyPosition.AddDoubleProperty(nameof(CassetteElevatorLoadingPosition), CassetteElevatorLoadingPosition);
            PropertyPosition.AddDoubleProperty(nameof(CassetteElevatorUnloadingPosition), CassetteElevatorUnloadingPosition);
            PropertyPosition.AddDoubleProperty(nameof(CassetteSlotPitch), CassetteSlotPitch);
        }

        /// <summary>
        /// 🚀 PropertyPosition에서 실제 Config 값들로 동기화
        /// </summary>
        public void SyncFromPropertyPosition()
        {
            if (PropertyPosition != null)
            {
                var CassetteElevatorLoadingPos = PropertyPosition.GetPropertyByTitle(nameof(CassetteElevatorLoadingPosition)) as DoubleProperty;
                if (CassetteElevatorLoadingPos != null) CassetteElevatorLoadingPosition = CassetteElevatorLoadingPos.Value;

                var CassetteElevatorUnloadingPos = PropertyPosition.GetPropertyByTitle(nameof(CassetteElevatorUnloadingPosition)) as DoubleProperty;
                if (CassetteElevatorUnloadingPos != null) CassetteElevatorUnloadingPosition = CassetteElevatorUnloadingPos.Value;

                var cassetteSlotPitch = PropertyPosition.GetPropertyByTitle(nameof(CassetteSlotPitch)) as DoubleProperty;
                if (cassetteSlotPitch != null) CassetteSlotPitch = cassetteSlotPitch.Value;
            }
        }

        /// <summary>
        /// 🚀 실제 Config 값들을 PropertyPosition으로 동기화
        /// </summary>
        public void SyncToPropertyPosition()
        {
            if (PropertyPosition != null)
            {
                var CassetteElevatorLoadingPos = PropertyPosition.GetPropertyByTitle(nameof(CassetteElevatorLoadingPosition)) as DoubleProperty;
                if (CassetteElevatorLoadingPos != null) CassetteElevatorLoadingPos.Value = CassetteElevatorLoadingPosition;

                var CassetteElevatorUnloadingPos = PropertyPosition.GetPropertyByTitle(nameof(CassetteElevatorUnloadingPosition)) as DoubleProperty;
                if (CassetteElevatorUnloadingPos != null) CassetteElevatorUnloadingPos.Value = CassetteElevatorUnloadingPosition;

                var cassetteSlotPitch = PropertyPosition.GetPropertyByTitle(nameof(CassetteSlotPitch)) as DoubleProperty;
                if (cassetteSlotPitch != null) cassetteSlotPitch.Value = CassetteSlotPitch;
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
