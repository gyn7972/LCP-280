namespace QMC.LCP_280.Process.Component
{
    /// <summary>
    /// 하드웨어 입력 정의 클래스
    /// </summary>
    public class HardInputDef
    {
        /// <summary>
        /// 입력 번호
        /// </summary>
        public int No { get; set; }

        /// <summary>
        /// 입력 이름/설명
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 디스플레이 번호 (예: X016, X017 등)
        /// </summary>
        public string Disp { get; set; }
    }

    /// <summary>
    /// 하드웨어 출력 정의 클래스
    /// </summary>
    public class HardOutputDef
    {
        /// <summary>
        /// 출력 번호
        /// </summary>
        public int No { get; set; }

        /// <summary>
        /// 출력 이름/설명
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 디스플레이 번호 (예: Y016, Y017 등)
        /// </summary>
        public string Disp { get; set; }
    }
}