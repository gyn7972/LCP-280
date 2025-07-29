using SP_GridTypeView.Component;
using System;

namespace SP_GridTypeView.Component
{
    public enum CassetteElevatorPosition
    {
        Ready,
        Loading,
        Unloading,
        Scanning
    }

    public class CassetteElevator : BaseComponent
    {
        public CassetteElevatorConfig Config { get; private set; }
        public CassetteElevatorPosition CurrentPosition { get; private set; }

        public CassetteElevator(CassetteElevatorConfig config = null) : base("CassetteElevator")
        {
            Config = config ?? new CassetteElevatorConfig();
            CurrentPosition = CassetteElevatorPosition.Ready;
        }

        public void MoveToReady()
        {
            // Config의 ReadyPosition을 사용하여 이동
            CurrentPosition = CassetteElevatorPosition.Ready;
            // 실제 하드웨어 제어: Config.ReadyPosition 사용
        }

        public void MoveToLoading()
        {
            // Config의 LoadingPosition을 사용하여 이동
            CurrentPosition = CassetteElevatorPosition.Loading;
            // 실제 하드웨어 제어: Config.LoadingPosition 사용
        }

        public void MoveToUnloading()
        {
            // Config의 UnloadingPosition을 사용하여 이동
            CurrentPosition = CassetteElevatorPosition.Unloading;
            // 실제 하드웨어 제어: Config.UnloadingPosition 사용
        }

        public void MoveToScanning()
        {
            // Config의 ScanningPosition을 사용하여 이동
            CurrentPosition = CassetteElevatorPosition.Scanning;
            // 실제 하드웨어 제어: Config.ScanningPosition 사용
        }
    }
}