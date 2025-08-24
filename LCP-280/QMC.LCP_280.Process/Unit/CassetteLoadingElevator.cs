using QMC.Common.Unit;
using QMC.LCP_280.Process.Component;
using QMC.Common;
using QMC.Common.Motion;

namespace QMC.LCP_280.Process.Unit
{
    public class CassetteLoadingElevator : BaseUnit, ICassetteElevatorUnit
    {
        public CassetteElevator CassetteElevator { get; private set; }
        public WaferSlotScanner WaferSlotScanner { get; private set; }
        public WaferTransferArm WaferTransferArm { get; private set; }
        public CassetteLoadingElevatorConfig CassetteLoadingElevatorConfig { get; private set; }

        public CassetteLoadingElevator(CassetteLoadingElevatorConfig config = null)
            : base("CassetteLoadingElevator")
        {
            CassetteLoadingElevatorConfig = config ?? new CassetteLoadingElevatorConfig();
            AddComponents();
        }

        public override void AddComponents()
        {
            CassetteElevator = new CassetteElevator();
            WaferSlotScanner = new WaferSlotScanner();
            WaferTransferArm = new WaferTransferArm();

            CassetteElevator.ParentUnit = this;
            WaferSlotScanner.ParentUnit = this;
            WaferTransferArm.ParentUnit = this;

            Components.Add(CassetteElevator);
            Components.Add(WaferSlotScanner);
            Components.Add(WaferTransferArm);
        }

        /// <summary>
        /// Motion 연결 직후 호출되어 CassetteElevator 및 WaferTransferArm 축을 초기화.
        /// </summary>
        public override void InitializeUnitAxes(IMotionAxisProvider provider)
        {
            if (provider == null) return;
            // 예시: 축 이름 규칙에 따라 조회
            //var z = provider.GetAxis("CassetteElevatorZ") ?? provider.GetAxis("CassetteZ") ?? provider.GetAxis("Z");
            //if (z != null)
            {
                //CassetteElevator?.InitializeAxes(z);
            }

            //var y = provider.GetAxis("WaferTransferArmY") ?? provider.GetAxis("ArmY") ?? provider.GetAxis("Y");
            //if (y != null)
            {
                //WaferTransferArm?.InitializeAxes(y);
            }
        }   

        public override void OnRun()
        {
            base.OnRun();
        }

        public override void OnStop()
        {
            base.OnStop();
        }
    }
}