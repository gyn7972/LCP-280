using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP_GridTypeView.Component
{
    public class WaferSlotScannerConfig : BaseConfig
    {
        // 스캔 관련 설정
        public int SlotCount { get; set; } = 25;
        public double ScanSpeed { get; set; } = 3.0;
        public int ScanDelayMs { get; set; } = 100;
        public int ScanTimeoutMs { get; set; } = 10000;

        // 센서 관련 설정
        public double SensorThreshold { get; set; } = 0.5;
        public int SensorSamplingCount { get; set; } = 3;
        public bool UseSensorFiltering { get; set; } = true;

        // 매핑 시작/끝 위치
        public double MappingStartPosition { get; set; } = 0.0;
        public double MappingEndPosition { get; set; } = 24.0;
        public double SlotPitch { get; set; } = 1.0;

        public WaferSlotScannerConfig() : base("WaferSlotScannerConfig")
        {
        }

        public override bool Validate()
        {
            if (SlotCount <= 0 || SlotCount > 50)
                return false;
            if (ScanSpeed <= 0)
                return false;
            if (SensorThreshold < 0 || SensorThreshold > 1)
                return false;
            if (MappingEndPosition <= MappingStartPosition)
                return false;
            if (SlotPitch <= 0)
                return false;

            return base.Validate();
        }

        public override void Reset()
        {
            SlotCount = 25;
            ScanSpeed = 3.0;
            ScanDelayMs = 100;
            SensorThreshold = 0.5;
            SensorSamplingCount = 3;
            UseSensorFiltering = true;
            MappingStartPosition = 0.0;
            MappingEndPosition = 24.0;
            SlotPitch = 1.0;

            base.Reset();
        }
    }
}