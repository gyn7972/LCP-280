using System.Runtime.InteropServices;

namespace QMC.Common
{
    public static class Win32Timer
    {
        [DllImport("winmm.dll", EntryPoint = "timeBeginPeriod")]
        public static extern uint TimeBeginPeriod(uint uMilliseconds);

        [DllImport("winmm.dll", EntryPoint = "timeEndPeriod")]
        public static extern uint TimeEndPeriod(uint uMilliseconds);

        // 프로그램 시작 시 (예: MainForm_Load 또는 Equipment.Init) 한 번만 호출
        public static void SetHighResolution()
        {
            TimeBeginPeriod(1); // 타이머 해상도를 1ms로 변경
        }

        // 프로그램 종료 시 한 번만 호출
        public static void RestoreResolution()
        {
            TimeEndPeriod(1);
        }
    }
}