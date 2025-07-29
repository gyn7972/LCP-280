using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP_GridTypeView.Component
{
    public class CassetteElevatorConfig : BaseConfig
    {
        // Z축 위치 설정
        public double ReadyPosition { get; set; } = 0.0;
        public double LoadingPosition { get; set; } = 10.0;
        public double UnloadingPosition { get; set; } = 20.0;
        public double ScanningPosition { get; set; } = 15.0;

        // 이동 관련 설정
        public double MoveSpeed { get; set; } = 5.0;
        public double MoveAcceleration { get; set; } = 2.0;
        public int MoveTimeoutMs { get; set; } = 5000;

        // 안전 관련 설정
        public double MinPosition { get; set; } = -5.0;
        public double MaxPosition { get; set; } = 50.0;
        public double PositionTolerance { get; set; } = 0.1;

        public CassetteElevatorConfig() : base("CassetteElevatorConfig")
        {
            propertyBases.Add(new DoubleProperty("ReadyPosition", ReadyPosition));
            propertyBases.Add(new DoubleProperty("LoadingPosition", LoadingPosition));
            propertyBases.Add(new DoubleProperty("UnloadingPosition", UnloadingPosition));
            propertyBases.Add(new DoubleProperty("ScanningPosition", ScanningPosition));



        }

        public override bool Validate()
        {
            if (ReadyPosition < MinPosition || ReadyPosition > MaxPosition)
                return false;
            if (LoadingPosition < MinPosition || LoadingPosition > MaxPosition)
                return false;
            if (UnloadingPosition < MinPosition || UnloadingPosition > MaxPosition)
                return false;
            if (ScanningPosition < MinPosition || ScanningPosition > MaxPosition)
                return false;
            if (MoveSpeed <= 0 || MoveAcceleration <= 0)
                return false;
            
            return base.Validate();
        }

        public override void Reset()
        {
            ReadyPosition = 0.0;
            LoadingPosition = 10.0;
            UnloadingPosition = 20.0;
            ScanningPosition = 15.0;
            MoveSpeed = 5.0;
            MoveAcceleration = 2.0;
            MoveTimeoutMs = 5000;
            
            base.Reset();
        }
    }
}
