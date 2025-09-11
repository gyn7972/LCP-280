using QMC.Common.Motion.Ajin; // AXL/AXM/AXD 래퍼 (Ajin SDK)
using QMC.Common.Motions; // ActiveLevel 등 공용 enum
using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace QMC.Common.Motions
{
    /// <summary>
    /// Ajin SDK 호출을 한 곳으로 모은 경량 래퍼.
    /// 
    /// ✔ 요구사항
    /// - <b>함수명은 기존 그대로 유지</b> (서명/동작 보강만 수행)
    /// - 내부에서는 <see cref="AXL"/> / <see cref="AXM"/> 의 공개 래퍼만 사용
    /// - 각 함수에 실제로 매핑되는 Ajin API 를 주석으로 명시
    /// 
    /// 참고: AXL/AXM 등은 /Common/Motion/Ajin/* 파일에 존재.
    /// </summary>
    public static class AjinApi
    {
        // ===== 내부 프로파일 캐시 (축별) =====
        private static readonly System.Collections.Concurrent.ConcurrentDictionary<int, double> _lastVel = new System.Collections.Concurrent.ConcurrentDictionary<int, double>();
        private static readonly System.Collections.Concurrent.ConcurrentDictionary<int, double> _lastAcc = new System.Collections.Concurrent.ConcurrentDictionary<int, double>();
        private static readonly System.Collections.Concurrent.ConcurrentDictionary<int, double> _lastDec = new System.Collections.Concurrent.ConcurrentDictionary<int, double>();
        private static readonly System.Collections.Concurrent.ConcurrentDictionary<int, int> _lastJerk = new System.Collections.Concurrent.ConcurrentDictionary<int, int>();
        private static readonly System.Collections.Concurrent.ConcurrentDictionary<int, AXT_MOTION_PROFILE_MODE> _lastMode = new System.Collections.Concurrent.ConcurrentDictionary<int, AXT_MOTION_PROFILE_MODE>();


        #region ===== 초기화/해제 =====
        /// <summary>
        /// Ajin 라이브러리(Axl.dll)를 <b>한 번만</b> 오픈합니다.
        /// </summary>
        /// <param name="boardNo">보드 번호(정보성). Ajin 오픈은 프로세스 단위이므로 실제 호출에는 사용하지 않습니다.</param>
        /// <returns>AXT_RT_SUCCESS(0) 또는 오류 코드</returns>
        /// <remarks>
        /// - 실제 매핑: <see cref="AXL.Open"/>
        /// </remarks>
        public static int OpenIfNeeded(int boardNo)
        {
            if (AXL.IsOpened()) return 0;
            return AXL.Open(); // AXL.CheckErrorCode 내부 사용
        }
        #endregion

        #region ===== Servo / Alarm =====
        /// <summary>
        /// 서보 On/Off.
        /// </summary>
        /// <remarks>
        /// 실제 매핑: <see cref="AXM.SetAmpEnabled(int,bool)"/>
        /// (Ajin 원함수: AxmSignalServoOn)
        /// </remarks>
        public static int ServoOnOff(int axisNo, bool on)
        {
            return AXM.SetAmpEnabled(axisNo, on);
        }

        /// <summary>
        /// 서보 알람 리셋(펄스).
        /// </summary>
        /// <remarks>
        /// 실제 매핑: <see cref="AXM.AlarmReset(int,bool)"/> (true → false로 펄스)
        /// (Ajin 원함수: AxmSignalServoAlarmReset)
        /// </remarks>
        public static int AlarmReset(int axisNo)
        {
            var rc = AXM.AlarmReset(axisNo, true);
            if (rc != 0) return rc;
            return AXM.AlarmReset(axisNo, false);
        }
        #endregion

        #region ===== Home =====
        /// <summary>
        /// 홈(원점) 동작 시작.
        /// </summary>
        /// <remarks>
        /// 실제 매핑: <see cref="AXM.StartHome(int)"/>
        /// (Ajin 원함수: AxmHomeSetStart)
        /// </remarks>
        public static int HomeStart(int axisNo)
        {
            return AXM.SetHomeStart(axisNo);
        }

        /// <summary>
        /// 홈 완료 여부.
        /// </summary>
        /// <remarks>
        /// 실제 매핑: <see cref="AXM.GetHomeResult(int,ref int)"/>
        /// (Ajin 원함수: AxmHomeGetResult)
        /// </remarks>
        public static bool HomeIsDone(int axisNo)
        {
            var result = AXT_MOTION_HOME_RESULT.HOME_SEARCHING;
            int rc = AXM.GetHomeResult(axisNo, ref result);
            return rc == 0 && result == AXT_MOTION_HOME_RESULT.HOME_SUCCESS;
        }
        #endregion

        #region ===== Profile(Trap/S-curve) =====
        /// <summary>
        /// 사다리꼴 프로파일 파라미터 설정(최대속도, 가/감속).
        /// </summary>
        /// <remarks>
        /// 실제 매핑: <see cref="AXM.SetProfileMode(int,AXT_MOTION_PROFILE_MODE)"/>,
        /// <see cref="AXM.SetMaxVelocity(int,double)"/>,
        /// <see cref="AXM.SetAcceleration(int,double)"/>,
        /// <see cref="AXM.SetDeceleration(int,double)"/>
        /// (Ajin 원함수: AxmMotSetProfileMode / AxmMotSetMaxVel / AxmMotSet*Accel/Decel)
        /// </remarks>
        public static int SetTrapezoidProfile(int axisNo, double velPulsePerSec, double accPulsePerSec2, double decPulsePerSec2)
        {
            //int rc;
            //if ((rc = AXM.SetProfileMode(axisNo, AXT_MOTION_PROFILE_MODE.SYM_TRAPEZOIDE_MODE)) != 0) return rc;
            //if ((rc = AXM.SetMaxVelocity(axisNo, velPulsePerSec)) != 0) return rc;
            //if ((rc = AXM.SetAcceleration(axisNo, accPulsePerSec2)) != 0) return rc;
            //if ((rc = AXM.SetDeceleration(axisNo, decPulsePerSec2)) != 0) return rc;
            return 0;
        }

        /// <summary>
        /// S-curve 프로파일 + Jerk 설정.
        /// </summary>
        /// <param name="jerk0to1000">Ajin 규격상 0~1000</param>
        /// <remarks>
        /// 실제 매핑: <see cref="AXM.SetProfileMode(int,AXT_MOTION_PROFILE_MODE)"/>,
        /// <see cref="AXM.SetMaxVelocity(int,double)"/>, <see cref="AXM.SetAcceleration(int,double)"/>,
        /// <see cref="AXM.SetDeceleration(int,double)"/>,
        /// <see cref="AXM.SetAccelerationJerk(int,double)"/>, <see cref="AXM.SetDecelerationJerk(int,double)"/>
        /// (Ajin 원함수: AxmMotSetProfileMode / AxmMotSetMaxVel / AxmMotSet*Accel/Decel/Jerk)
        /// </remarks>
        public static int SetSCurveProfile(int axisNo, double velPulsePerSec, double accPulsePerSec2, double decPulsePerSec2, int jerk0to1000)
        {
            //int rc;
            //if ((rc = AXM.SetProfileMode(axisNo, AXT_MOTION_PROFILE_MODE.SYM_S_CURVE_MODE)) != 0) return rc;
            //if ((rc = AXM.SetMaxVelocity(axisNo, velPulsePerSec)) != 0) return rc;
            //if ((rc = AXM.SetAcceleration(axisNo, accPulsePerSec2)) != 0) return rc;
            //if ((rc = AXM.SetDeceleration(axisNo, decPulsePerSec2)) != 0) return rc;
            //// jerk는 0~1000 스케일을 그대로 전달
            //if ((rc = AXM.SetAccelerationJerk(axisNo, jerk0to1000)) != 0) return rc;
            //if ((rc = AXM.SetDecelerationJerk(axisNo, jerk0to1000)) != 0) return rc;
            return 0;
        }
        #endregion

        #region ===== Move =====
        /// <summary>
        /// 절대 위치 이동. 현재 설정된 프로파일을 그대로 사용합니다.
        /// </summary>
        /// <remarks>
        /// 실제 매핑: <see cref="AXM.MovePosition(int,double,double,TimeSpan,TimeSpan)"/>
        /// (Ajin 원함수: AxmMoveStartPos)
        /// 
        /// ⚠ Ajin의 AxmMoveStartPos는 속도/가감속을 인자로 요구합니다. 
        /// 여기서는 사전에 Set*Profile로 설정된 값을 그대로 쓰기 위해, 
        /// 현재 설정된 최대속도/가감속 시간을 추정하지 않고 <b>가속/감속 시간 0</b>으로 전달합니다
        /// (Ajin 드라이버는 내부 프로파일을 사용하여 실행).
        /// </remarks>
        public static int MoveAbs(int axisNo, double targetPulse, double vel, double acc, double dec, double jerk)
        {
            // 가속/감속 시간을 0초로 넘겨 내부 프로파일을 사용하도록 위임
            //return AXM.MovePosition(axisNo, targetPulse, /*velocity*/ 0, TimeSpan.Zero, TimeSpan.Zero);

            //Test
            vel = 5;// GetCommandVelocityPps(axisNo);
            acc = 10;//Math.Max(vel * 2.0, 1.0);
            dec = 10;//acc;
           
            AXM.SetAbsRelMode(axisNo, true);
            return AXM.MovePosition(axisNo, targetPulse, vel, acc, dec);
        }

        #region ===== Jog / Velocity Mode =====
        /// <summary>
        /// 속도 모드로 지속 구동 시작. dVel 부호로 방향(+ CW / - CCW).
        /// </summary>
        /// <remarks>
        /// 실제 매핑: <see cref="AXM.MoveVelocity(int,double,double,double)"/>
        /// (Ajin 원함수: AxmMoveVel)
        /// </remarks>
        public static int MoveVel(int axisNo, double dVelUnitPerSec, double dAccelUnit, double dDecelUnit)
        {
            // UNIT/PULSE 스케일은 시스템 초기화 단계에서 이미 설정되어 있어야 함.
            return AXM.MoveVelocity(axisNo, dVelUnitPerSec, dAccelUnit, dDecelUnit);

            //double vel = 5;// GetCommandVelocityPps(axisNo);
            //double acc = 10;//Math.Max(vel * 2.0, 1.0);
            //double dec = 10;//acc;
            //return AXM.MoveVelocity(axisNo, -vel, acc, dec);
        }
        #endregion

        /// <summary>
        /// 모션 완료 여부(= InMotion이 아님).
        /// </summary>
        /// <remarks>
        /// 실제 매핑: <see cref="AXM.GetInMotion(int,ref bool)"/>
        /// (Ajin 원함수: AxmStatusReadInMotion)
        /// </remarks>
        public static bool MotionDone(int axisNo)
        {
            bool inMotion = false;
            var rc = AXM.GetInMotion(axisNo, ref inMotion);
            if (rc != 0) return false;
            return !inMotion;

            //bool inMotion = false;
            //AXM.GetInMotion(axisNo, ref inMotion);
            //return !inMotion;
        }


        /// <summary>감속 정지(S-Stop)</summary>
        /// <remarks>실제 매핑: <see cref="AXM.StopSlowly(int)"/> (Ajin: AxmMoveSStop)</remarks>
        public static int SStop(int axisNo) => AXM.StopSlowly(axisNo);

        /// <summary>즉시 정지(E-Stop)</summary>
        /// <remarks>실제 매핑: <see cref="AXM.StopEmergency(int)"/> (Ajin: AxmMoveEStop)</remarks>
        public static int EStop(int axisNo) => AXM.StopEmergency(axisNo);
        #endregion

        #region ===== Position IO =====
        /// <summary>Actual 위치(펄스) 읽기. (Ajin: AxmStatusGetActPos)</summary>
        public static double GetActualPositionPulse(int axisNo)
        {
            double pos = 0;
            AXM.GetActualPosition(axisNo, ref pos);
            return pos;
        }

        /// <summary>Actual 위치(펄스) 강제 설정. (Ajin: AxmStatusSetActPos)</summary>
        public static int SetActualPositionPulse(int axisNo, double pulse)
        {
            return AXM.SetActualPosition(axisNo, pulse);
        }

        /// <summary>Command 위치(펄스) 강제 설정. (Ajin: AxmStatusSetCmdPos)</summary>
        public static int SetCommandPositionPulse(int axisNo, double pulse)
        {
            return AXM.SetCommandPosition(axisNo, pulse);
        }

        /// <summary>Command 위치(펄스) 읽기. (Ajin: AxmStatusGetCmdPos)</summary>
        public static double GetCommandPositionPulse(int axisNo)
        {
            double pos = 0;
            AXM.GetCommandPosition(axisNo, ref pos);
            return pos;
        }

        /// <summary>오차 위치(펄스) 읽기. (Ajin: AxmStatusGetPosError)</summary>
        public static double GetErrorPositionPulse(int axisNo)
        {
            double err = 0;
            AXM.GetPositionError(axisNo, ref err);
            return err;
        }

        /// <summary>
        /// Command 속도(PPS).
        /// </summary>
        /// <remarks>
        /// Ajin 원함수로 CmdVel 전용 래퍼가 제공되지 않아, 현재는 실제 속도와 동일하게 반환합니다.
        /// 필요 시 AXM에 <c>GetCommandVelocity</c> 래퍼를 추가해 분리 가능합니다.
        /// </remarks>
        public static double GetCommandVelocityPps(int axisNo)
        {
            return GetActualVelocityPps(axisNo);
        }

        /// <summary>Actual 속도(PPS) 읽기. (Ajin: AxmStatusReadVel)</summary>
        public static double GetActualVelocityPps(int axisNo)
        {
            double vel = 0;
            AXM.GetVelocity(axisNo, ref vel);
            return vel;
        }
        #endregion

        #region ===== I/O (신호) =====
        /// <summary>Servo On 상태. (Ajin: AxmSignalIsServoOn)</summary>
        public static bool GetServoOn(int axisNo)
        {
            bool on = false;
            AXM.GetAmpEnabled(axisNo, ref on);
            return on;
        }

        /// <summary>Alarm 입력 상태. (Ajin: AxmSignalReadServoAlarm)</summary>
        public static bool GetAlarm(int axisNo)
        {
            bool alarm = false;
            AXM.GetAmpFaultValue(axisNo, ref alarm);
            return alarm;
        }

        /// <summary>Negative Limit 상태. (Ajin: AxmSignalReadLimit)</summary>
        public static bool GetNlimit(int axisNo)
        {
            bool on = false;
            AXM.GetNegativeLimitValue(axisNo, ref on);
            return on;
        }

        /// <summary>Positive Limit 상태. (Ajin: AxmSignalReadLimit)</summary>
        public static bool GetPlimit(int axisNo)
        {
            bool on = false;
            AXM.GetPositiveLimitValue(axisNo, ref on);
            return on;
        }

        /// <summary>Home 센서 상태. (Ajin: AxmHomeReadSignal)</summary>
        public static bool GetHomeSensor(int axisNo)
        {
            bool on = false;
            AXM.GetHomeSensorValue(axisNo, ref on);
            return on;
        }
        #endregion

        #region ===== State (완료/인포지션/타임아웃 플래그) =====
        /// <summary>In-Position 입력 상태. (Ajin: AxmSignalReadInpos)</summary>
        public static bool GetInposition(int axisNo)
        {
            bool ip = false;
            AXM.GetInPositionValue(axisNo, ref ip);
            return ip;
        }

        /// <summary>
        /// In-Position 완료 Latched 플래그(사용처에서 래치/타이머 처리). 여기서는 단순 false 반환.
        /// 필요 시 상위에서 상태 래치를 구현하세요.
        /// </summary>
        public static bool GetInpositionDone(int axisNo) { return false; }

        /// <summary>
        /// In-Position 타임아웃 플래그(상위 타이머 연동). 여기서는 단순 false 반환.
        /// </summary>
        public static bool GetInpositionTimeout(int axisNo) { return false; }

        /// <summary>Home 완료 Latched(실시간 상태는 <see cref="HomeIsDone"/> 참조). 여기서는 단순 false 반환.</summary>
        public static bool GetHomeEnd(int axisNo) { return false; }

        /// <summary>Home 타임아웃 플래그(상위 타이머 연동). 여기서는 단순 false 반환.</summary>
        public static bool GetHomeTimeout(int axisNo) { return false; }
        #endregion

        // ─────────────────────────────────────────────────────────────
        // MotionAxisSetup → Ajin 보드 적용
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// 배선/레벨/모드 기본 셋업 적용 (PulsesPerUnit/Scale은 드라이버에서 단위변환에 사용).
        /// </summary>
        public static int ApplySetupBasic(int axisNo, MotionAxisSetup setup)
        {
            int rc;
            var outMethod = MapOutputMethod(setup.PulseOutput);
            if ((rc = AXM.SetOutputMethod(axisNo, outMethod)) != 0) return rc;        // 펄스 출력 방식

            var encMethod = MapEncoderMethod(setup.EncoderInput);
            if ((rc = AXM.SetEncoderMethod(axisNo, encMethod)) != 0) return rc;       // 엔코더 입력 방식

            if ((rc = AXM.SetZPhaseLevel(axisNo, setup.ZPhaseLevel)) != 0) return rc; // Z상 레벨
            if ((rc = AXM.SetAmpEnableLevel(axisNo, setup.ServoOnLevel)) != 0) return rc;// Servo On 레벨
                                                                                       // ※ Inpos/Alarm 레벨 setter가 래퍼에 없으면 .mot 기본값 사용(필요 시 래퍼 추가)

            return 0;
        }

        /// <summary>
        /// 홈(원점) 설정 적용 (방법/방향/센서/속도). 속도/가속은 호출 시 매개변수로 전달.
        /// </summary>
        public static int ApplyHomeFromSetup(
            int axisNo, MotionAxisSetup setup,
            double firstVel, double secondVel, double lastVel,
            double indexVel, double firstAcc, double secondAcc)
        {
            int rc;

            // 1) 홈 방법 (방향/신호/Z상)
            //MapHome(setup.HomeMode, out var dir, out var sig, out var z);
            if ((rc = AXM.SetHomeSensorLevel(axisNo, setup.HomeSignalLevel)) != 0) return rc;

            if ((rc = AXM.SetHomeMethod(axisNo, setup.HomeDirection, setup.HomeSignal, setup.HomeZPhase, setup.HomeClearTime, setup.HomeOffset)) != 0) return rc; // AxmHomeSetMethod :contentReference[oaicite:4]{index=4}

            // 2) 홈 속도/가속
            if ((rc = AXM.SetHomeVelocity(axisNo, firstVel, secondVel, lastVel, indexVel, firstAcc, secondAcc)) != 0) return rc; // AxmHomeSetVel :contentReference[oaicite:5]{index=5}

            return 0;
        }

        // ── 프로파일(Trap/S) 사전 세팅: 속도/저크는 보드에 기록, 가/감속은 MovePosition에서 사용 ──
        public static int ApplyTrapProfile(int axis, double velPps, double accPps2, double decPps2, int unit, int pulse)
        {
            int rc;
            if ((rc = AXM.SetMoveUnitPerPulse(axis, unit, pulse)) != 0) return rc; ;
            if ((rc = AXM.SetProfileMode(axis, AXT_MOTION_PROFILE_MODE.SYM_TRAPEZOIDE_MODE)) != 0) return rc; // AxmMotSetProfileMode :contentReference[oaicite:6]{index=6}
            if ((rc = AXM.SetMaxVelocity(axis, velPps)) != 0) return rc; // AxmMotSetMaxVel (+ Override) :contentReference[oaicite:7]{index=7}
                                                                         // acc/dec는 MovePosition(..., vel, acc, dec)에서 실제 사용 (보드 전역 set 함수 없음)
            return 0;
        }

        public static int ApplySCurveProfile(int axis, double velPps, double accPps2, double decPps2, int jerk0to1000, int unit, int pulse)
        {
            int rc;
            if ((rc = AXM.SetMoveUnitPerPulse(axis, unit, pulse)) != 0) return rc; ;
            if ((rc = AXM.SetProfileMode(axis, AXT_MOTION_PROFILE_MODE.SYM_S_CURVE_MODE)) != 0) return rc; // :contentReference[oaicite:8]{index=8}
            if ((rc = AXM.SetMaxVelocity(axis, velPps)) != 0) return rc;                                   // :contentReference[oaicite:9]{index=9}
            if ((rc = AXM.SetAccelerationJerk(axis, jerk0to1000)) != 0) return rc;                         // AxmMotSetAccelJerk :contentReference[oaicite:10]{index=10}
            if ((rc = AXM.SetDecelerationJerk(axis, jerk0to1000)) != 0) return rc;                         // AxmMotSetDecelJerk :contentReference[oaicite:11]{index=11}
            return 0;
        }

        // ─────────────────────────────────────────────────────────────
        // 매핑 유틸
        // ─────────────────────────────────────────────────────────────
        private static AXM.MotorOutputMethod MapOutputMethod(PulseOutput m)
        {
            switch (m)
            {
                case PulseOutput.TwoPulse_High_CCW_CW: return AXM.MotorOutputMethod.TwoCcwCwHigh; // :contentReference[oaicite:12]{index=12}
                case PulseOutput.TwoPulse_Low_CCW_CW: return AXM.MotorOutputMethod.TwoCcwCwLow;  // :contentReference[oaicite:13]{index=13}
                case PulseOutput.AB_Phase:
                    // 래퍼에 AB-Phase 전용 항목이 없다면(현재 enum에 미포함) TwoCcwCwHigh를 기본값으로 사용.
                    return AXM.MotorOutputMethod.TwoCcwCwHigh;
                default: return AXM.MotorOutputMethod.TwoCcwCwHigh;
            }
        }

        private static AXM.EncoderInputMethod MapEncoderMethod(EncoderInput m)
        {
            switch (m)
            {
                case EncoderInput.Normal: return AXM.EncoderInputMethod.ObverseSqr4Mode;  // :contentReference[oaicite:14]{index=14}
                case EncoderInput.Reverse: return AXM.EncoderInputMethod.ReverseUpDownMode; // 필요시 ReverseSqr4Mode로 변경
                case EncoderInput.Reverse_SQR4: return AXM.EncoderInputMethod.ReverseSqr4Mode;  // :contentReference[oaicite:15]{index=15}
                default: return AXM.EncoderInputMethod.ObverseSqr4Mode;
            }
        }

        private static void MapHome(HomeMode m, out HomeDirection dir, out HomeSignal sig, out HomeZPhase z)
        {
            switch (m)
            {
                case HomeMode.NegativeLimit:
                    dir = HomeDirection.Ccw;                  // 기존 NegDir → Ccw
                    sig = HomeSignal.NegativeLimit;       // 기존 Limit → NegativeLimit
                    z = HomeZPhase.Ccw;               // 기존 Z_Phase → Ccw(방향 기준)
                    break;

                case HomeMode.PositiveLimit:
                    dir = HomeDirection.Cw;                   // 기존 PosDir → Cw
                    sig = HomeSignal.PositiveLimit;       // 기존 Limit → PositiveLimit
                    z = HomeZPhase.Cw;                // 기존 Z_Phase → Cw(방향 기준)
                    break;

                case HomeMode.HomeSensor:
                default:
                    dir = HomeDirection.Ccw;                  // 기존 NegDir → Ccw (기본 방향)
                    sig = HomeSignal.HomeSensor;          // 기존 Home → HomeSensor
                    z = HomeZPhase.Ccw;               // 기존 Z_Phase → Ccw
                    break;
            }
        }

        // ── MotionSignal
        public static int ApplyMotionSignal(int axis, uint eStopMode, uint eStopLevel, ActiveLevel alarmLevel, 
            ActiveLevel alarmResetLevel, InPosition inPosition, ActiveLevel positiveLevel, ActiveLevel negariveLevel)
        {
            int rc;

            if ((rc = AXM.SetSignalStop(axis, eStopMode, eStopLevel)) != 0) return rc;
            if ((rc = AXM.SetAmpFaultLevel(axis, alarmLevel)) != 0) return rc;
            if ((rc = AXM.SetAmpResetLevel(axis, alarmResetLevel)) != 0) return rc;
            if ((rc = AXM.SetInPositionLevel(axis, inPosition)) != 0) return rc;
            if ((rc = AXM.SetPositiveLimitLevel(axis, eStopMode, positiveLevel, alarmLevel)) != 0) return rc;
            return 0;
        }




    }
}
