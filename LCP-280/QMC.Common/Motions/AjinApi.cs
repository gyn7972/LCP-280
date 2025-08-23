using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QMC.Common.Motions
{
    /// <summary>
    /// Ajin SDK 호출을 한 군데로 모은 래퍼.
    /// 실제 Ajin API 함수명/반환코드로 채우세요.
    /// </summary>
    public static class AjinApi
    {
        // ====== 초기화/해제(옵션) ======
        public static int OpenIfNeeded(int boardNo)
        {
            // TODO: AxlOpen() 또는 보드별 초기화
            // return 0;
            return 0;
        }

        // ====== Servo / Alarm ======
        public static int ServoOnOff(int axisNo, bool on)
        {
            // TODO: Ajin Servo On/Off
            // return AxmSignalServoOn(axisNo, on ? 1 : 0);
            return 0;
        }

        public static int AlarmReset(int axisNo)
        {
            // TODO: Alarm Reset API
            // return AxmSignalSetReset(axisNo, 1);
            return 0;
        }

        // ====== Home ======
        public static int HomeStart(int axisNo)
        {
            // TODO: 홈 시작 API
            // return AxmHomeSetStart(axisNo);
            return 0;
        }

        public static bool HomeIsDone(int axisNo)
        {
            // TODO: 홈 상태 조회
            // int status = 0;
            // AxmHomeGetResult(axisNo, ref status); // 예시
            // return status == SOME_DONE_CODE;
            return true;
        }

        // ====== Profile(Trap/S-curve) ======
        public static int SetTrapezoidProfile(int axisNo, double velPulsePerSec, double accPulsePerSec2, double decPulsePerSec2)
        {
            // TODO: 사다리꼴 프로파일 설정
            // return AxmMotSetProfile(axisNo, velPulsePerSec, accPulsePerSec2, decPulsePerSec2);
            return 0;
        }

        public static int SetSCurveProfile(int axisNo, double velPulsePerSec, double accPulsePerSec2, double decPulsePerSec2, int jerk0to1000)
        {
            // TODO: S-curve 프로파일 + jerk 설정
            // return AxmMotSetProfileS(axisNo, velPulsePerSec, accPulsePerSec2, decPulsePerSec2, jerk0to1000);
            return 0;
        }

        // ====== Move ======
        public static int MoveAbs(int axisNo, double targetPulse)
        {
            // TODO: 절대 위치 이동
            // return AxmMoveStartPos(axisNo, targetPulse);
            return 0;
        }

        public static bool MotionDone(int axisNo)
        {
            // TODO: 모션 완료 플래그
            // int done = 0;
            // AxmStatusReadInMotion(axisNo, ref done);
            // return done == 0; // 예시
            return true;
        }

        public static int SStop(int axisNo)
        {
            // TODO: 감속 정지
            // return AxmMoveSStop(axisNo);
            return 0;
        }

        public static int EStop(int axisNo)
        {
            // TODO: 즉시 정지
            // return AxmMoveEStop(axisNo);
            return 0;
        }

        // ====== Position IO ======
        public static double GetActualPositionPulse(int axisNo)
        {
            // TODO: 실제 위치(펄스) 읽기
            // double pos = 0;
            // AxmStatusGetActPos(axisNo, ref pos);
            // return pos;
            return 0.0;
        }

        public static int SetActualPositionPulse(int axisNo, double pulse)
        {
            // TODO: Actual 위치 강제 설정
            // return AxmStatusSetActPos(axisNo, pulse);
            return 0;
        }

        public static int SetCommandPositionPulse(int axisNo, double pulse)
        {
            // TODO: Command 위치 강제 설정
            // return AxmStatusSetCmdPos(axisNo, pulse);
            return 0;
        }

        // --- 위치/속도 ---
        public static double GetCommandPositionPulse(int axisNo) { /* AxmStatusGetCmdPos */ return 0.0; }
        public static double GetErrorPositionPulse(int axisNo) { /* AxmStatusGetRemain -> or ErrPos */ return 0.0; }
        public static double GetCommandVelocityPps(int axisNo) { /* AxmStatusReadVelCmd */ return 0.0; }
        public static double GetActualVelocityPps(int axisNo) { /* AxmStatusReadVelAct */ return 0.0; }

        // --- I/O ---
        public static bool GetServoOn(int axisNo) { /* AxmSignalIsServoOn */ return false; }
        public static bool GetAlarm(int axisNo) { /* AxmSignalReadAlarm */ return false; }
        public static bool GetNlimit(int axisNo) { /* AxmSignalReadNLim */ return false; }
        public static bool GetPlimit(int axisNo) { /* AxmSignalReadPLim */ return false; }
        public static bool GetHomeSensor(int axisNo) { /* AxmSignalReadHome */ return false; }

        // --- State ---
        public static bool GetInposition(int axisNo) { /* AxmStatusReadInPos */ return false; }
        public static bool GetInpositionDone(int axisNo) { /* custom/latched flag */ return false; }
        public static bool GetInpositionTimeout(int axisNo) { /* custom timer flag */ return false; }
        public static bool GetHomeEnd(int axisNo) { /* AxmHomeGetResult==DONE */ return false; }
        public static bool GetHomeTimeout(int axisNo) { /* custom timer flag */ return false; }

    }
}
