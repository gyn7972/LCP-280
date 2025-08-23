namespace QMC.Common
{
    /// <summary>
    /// 도메인 Position 엔티티. UI 모델과 분리.
    /// </summary>
    public class Position
    {
        public string Key { get; set; } // 강한 식별자
        public string Name { get; set; } // UI 표시명
        public string AxisKey { get; set; }
        public string Unit { get; set; }

        public double Value { get; set; }
        public double Velocity { get; set; }
        public double Acceleration { get; set; }
        public double Deceleration { get; set; }
        public int TimeoutMs { get; set; }
    }
}
