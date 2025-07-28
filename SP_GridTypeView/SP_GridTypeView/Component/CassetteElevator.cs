using SP_GridTypeView.Component;
using System;

namespace SP_GridTypeView.Component
{
    public enum CassetteElevatorPosition
    {
        Ready,
        Scan,
        Loading,
        Unloading
    }

    public class CassetteElevator : BaseComponent
    {
        public CassetteElevatorPosition CurrentPosition { get; private set; }

        public CassetteElevator() : base("CassetteElevator")
        {
            CurrentPosition = CassetteElevatorPosition.Ready;
        }

        public void MoveToReady()
        {
            // Z축을 Ready 위치로 이동하는 로직
            CurrentPosition = CassetteElevatorPosition.Ready;
            // 실제 하드웨어 제어 코드 또는 시뮬레이션 코드 추가
            // 오류 발생 시 알람 발생
        }

        public void MoveToScan()
        {
            // Z축을 Scan 위치로 이동하는 로직
            CurrentPosition = CassetteElevatorPosition.Scan;
        }

        public void MoveToLoading()
        {
            // Z축을 Loading 위치로 이동하는 로직
            CurrentPosition = CassetteElevatorPosition.Loading;
        }

        public void MoveToUnloading()
        {
            // Z축을 Unloading 위치로 이동하는 로직
            CurrentPosition = CassetteElevatorPosition.Unloading;
        }
    }
}