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
            Console.WriteLine("🚀 CassetteElevatorConfig 생성자 시작");
            Console.WriteLine($"   PropertyPosition 초기 상태: {PropertyPosition?.GetType().Name ?? "null"}");
            
            // 🚀 각 Position들을 PropertyPosition에 DoubleProperty로 추가
            Console.WriteLine("🔧 PropertyPosition에 DoubleProperty들 추가 시작:");
            
            PropertyPosition.AddDoubleProperty(nameof(LifterZLoadingPosition), LifterZLoadingPosition);
            Console.WriteLine($"   추가됨: {nameof(LifterZLoadingPosition)} = {LifterZLoadingPosition}");
            
            PropertyPosition.AddDoubleProperty(nameof(LifterZUnloadingPosition), LifterZUnloadingPosition);
            Console.WriteLine($"   추가됨: {nameof(LifterZUnloadingPosition)} = {LifterZUnloadingPosition}");
            
            PropertyPosition.AddDoubleProperty(nameof(CassetteSlotPitch), CassetteSlotPitch);
            Console.WriteLine($"   추가됨: {nameof(CassetteSlotPitch)} = {CassetteSlotPitch}");
            
            PropertyPosition.AddDoubleProperty(nameof(FeederReadyPosition), FeederReadyPosition);
            Console.WriteLine($"   추가됨: {nameof(FeederReadyPosition)} = {FeederReadyPosition}");
            
            PropertyPosition.AddDoubleProperty(nameof(FeederAvoidPosition), FeederAvoidPosition);
            Console.WriteLine($"   추가됨: {nameof(FeederAvoidPosition)} = {FeederAvoidPosition}");
            
            PropertyPosition.AddDoubleProperty(nameof(FeederStagePosition), FeederStagePosition);
            Console.WriteLine($"   추가됨: {nameof(FeederStagePosition)} = {FeederStagePosition}");
            
            PropertyPosition.AddDoubleProperty(nameof(FeederCassettePosition), FeederCassettePosition);
            Console.WriteLine($"   추가됨: {nameof(FeederCassettePosition)} = {FeederCassettePosition}");
            
            Console.WriteLine($"✅ PropertyPosition 초기화 완료. 총 {PropertyPosition.PropertyCount}개 Property 추가됨");
            
            // 🔍 추가된 Property들 확인
            Console.WriteLine("🔍 추가된 Property 목록:");
            var titles = PropertyPosition.GetPropertyTitles();
            for (int i = 0; i < titles.Length; i++)
            {
                var prop = PropertyPosition.GetPropertyByTitle(titles[i]);
                if (prop is DoubleProperty dp)
                {
                    Console.WriteLine($"   [{i}] '{titles[i]}' = {dp.Value:F3}");
                }
            }
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

                Console.WriteLine($"🔄 PropertyPosition → Config 동기화 완료:");
                Console.WriteLine($"   LifterZLoadingPosition: {LifterZLoadingPosition:F3}");
                Console.WriteLine($"   LifterZUnloadingPosition: {LifterZUnloadingPosition:F3}");
                Console.WriteLine($"   CassetteSlotPitch: {CassetteSlotPitch:F3}");
                Console.WriteLine($"   FeederReadyPosition: {FeederReadyPosition:F3}");
                Console.WriteLine($"   FeederAvoidPosition: {FeederAvoidPosition:F3}");
                Console.WriteLine($"   FeederStagePosition: {FeederStagePosition:F3}");
                Console.WriteLine($"   FeederCassettePosition: {FeederCassettePosition:F3}");
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

                Console.WriteLine($"🔄 Config → PropertyPosition 동기화 완료");
            }
        }

        public override bool Validate()
        {

            return base.Validate();
        }

        public override void Reset()
        {
            
            // 🚀 PropertyPosition도 리셋
            SyncToPropertyPosition();

            base.Reset();
        }
    }
}
