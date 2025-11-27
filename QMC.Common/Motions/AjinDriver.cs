using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QMC.Common.Motions
{
    /// <summary>
    /// Ajin 보드 어댑터(IMotionDriver 구현). Ajin SDK 호출부를 AjinApi.* 로 위임합니다.
    /// </summary>
    public sealed class AjinDriver //: IMotionDriver
    {
        private readonly int _boardNo;
        private readonly bool _useLogicalUnits;
        private readonly double _pulsesPerUnit; // mm->pulse, deg->pulse

        private MotionAxisSetup _lastSetup;
        private MotionAxisConfig _lastConfig;

        // 프로파일/모드 설정 (필요 시 외부에서 주입/변경 가능)
        public ProfileMode ProfileMode { get; set; } = ProfileMode.SCurve;

        public AjinDriver(int boardNo, double pulsesPerUnit, bool useLogicalUnits = true)
        {
            _boardNo = boardNo;
            _pulsesPerUnit = pulsesPerUnit > 0 ? pulsesPerUnit : 1000.0;
            _useLogicalUnits = useLogicalUnits;

            // SDK 초기화 (필요 시)
            // AjinApi.OpenIfNeeded(_boardNo);
        }

        // ===== IMotionDriver =====
        public int Home(int axisNo)
        {
            // 1) 홈 파라미터 세팅
            // TODO: 실제 홈 모드/레벨/속도 등 세팅 필요시 여기서 호출
            // AjinApi.HomeSet(axisNo, ...);

            //if (_lastSetup != null && _lastConfig != null)
            //{
            //    // 단위 변환 스케일 (기존 구현과 동일하게 ppu=1 유지; 필요 시 _pulsesPerUnit로 조정)
            //    double ppu = 1; // _pulsesPerUnit;

            //    double h1v = _lastConfig.HomeFirstSpeed * ppu;
            //    double h2v = _lastConfig.HomeSecondSpeed * ppu;
            //    double hlv = _lastConfig.HomeThirdSpeed * ppu;
            //    double izv = _lastConfig.HomeLastSpeed * ppu;
            //    double h1a = _lastConfig.HomeFirstAcc * ppu;
            //    double h2a = _lastConfig.HomeSecondAcc * ppu;

            //    int rc0 = AjinApi.ApplyHomeFromSetup(axisNo, _lastSetup, h1v, h2v, hlv, izv, h1a, h2a);
            //    if (rc0 != 0) return rc0;
            //}

            // 홈 테스트 필요.!!
            // 1) 홈 파라미터 셋팅
            //double firstVel = 0.0, secondVel = 0.0;
            //double thirdVel = 0.0, lastVel = 0.0;
            //double firstAcc = 0.0, secondAcc = 0.0;
            //firstVel = _lastConfig.HomeFirstSpeed;
            //secondVel = _lastConfig.HomeSecondSpeed;
            //thirdVel = _lastConfig.HomeThirdSpeed;
            //lastVel = _lastConfig.HomeLastSpeed;
            //firstAcc = _lastConfig.HomeFirstAcc;
            //secondAcc = _lastConfig.HomeSecondAcc;
            //int rc = AjinApi.ApplyHomeFromSetup(axisNo,
            //    _lastSetup,
            //    firstVel,
            //    secondVel, 
            //    thirdVel, 
            //    lastVel, 
            //    firstAcc, 
            //    secondAcc);
            //if(rc != 0)
            //{
            //    Log.Write("LCP_280", "Home", "파라미터 셋팅 Error");
            //    return rc;
            //}

            // 2) 홈 시작
            return AjinApi.HomeStart(axisNo);
        }

        public bool IsHomeDone(int axisNo)
        {
            // Ajin SDK의 홈 상태 조회 결과를 bool로 변환
            return AjinApi.HomeIsDone(axisNo);
        }

        public int MoveAbsPosition(int axisNo, double targetPulse, double vel, double acc, double dec, double jerk)
        {
            // 속도/가감속 단위 변환 (필요 시)
            double v = vel, a = acc, d = dec;
            //if (_useLogicalUnits)
            //{
            //    // vel: unit/s → pulse/s
            //    v = vel * _pulsesPerUnit;
            //    // acc/dec: unit/s^2 → pulse/s^2
            //    a = acc * _pulsesPerUnit;
            //    d = dec * _pulsesPerUnit;
            //}

            // 프로파일 설정 (Trapezoid / S-Curve)
            //int rc = 0;
            //switch (ProfileMode)
            //{
            //    case ProfileMode.Trapezoid:
            //        rc = AjinApi.SetTrapezoidProfile(axisNo, v, a, d);
            //        break;
            //    case ProfileMode.SCurve:
            //        // jerk(0~1.0) → Ajin 보드 스케일로 매핑 (예: 0~1000 가정)
            //        int jerk0to1000 = MapJerk01ToDriver(jerk);
            //        rc = AjinApi.SetSCurveProfile(axisNo, v, a, d, jerk0to1000);
            //        break;
            //}
            //if (rc != 0) return rc;

            // 절대 위치 이동 시작
            return AjinApi.MoveAbs(axisNo, targetPulse, vel, acc, dec, jerk);
        }

        public bool IsMoveDone(int axisNo)
        {
            return AjinApi.MotionDone(axisNo);
        }

        public int Stop(int axisNo)
        {
            // 감속 정지
            return AjinApi.SStop(axisNo);
        }

        public int EmgStop(int axisNo)
        {
            // 즉시 정지
            return AjinApi.EStop(axisNo);
        }

        public int Servo(int axisNo, bool on)
        {
            return AjinApi.ServoOnOff(axisNo, on);
        }

        public int ClearAlarm(int axisNo)
        {
            return AjinApi.AlarmReset(axisNo);
        }

        public double ReadActualPulse(int axisNo)
        {
            return AjinApi.GetActualPositionPulse(axisNo);
        }

        public int SetActualPulse(int axisNo, double pulse)
        {
            return AjinApi.SetActualPositionPulse(axisNo, pulse);
        }

        public int SetCommandPulse(int axisNo, double pulse)
        {
            return AjinApi.SetCommandPositionPulse(axisNo, pulse);
        }

        // AjinDriver.cs (클래스 내부에 아래 메서드 추가)
        public int JogVelStart(int axisNo, double velUnitPerSec, double accUnit, double decUnit)
        {
            // AjinApi 래퍼 경유(AXM.MoveVelocity 호출)
            return AjinApi.MoveVel(axisNo, velUnitPerSec, accUnit, decUnit);
        }

        public int JogStop(int axisNo)
        {
            // 감속 정지(프리셋)
            return AjinApi.SStop(axisNo);
        }

        public int JogEStop(int axisNo)
        {
            return AjinApi.EStop(axisNo);
        }

        // (선택) 감속값 지정 정지가 꼭 필요하면 Stop 전에 Decel 프로파일만 일시 적용 후 SStop
        public int JogSlowStop(int axisNo, double decUnit)
        {
            // 예: AXM.SetDeceleration(axisNo, decUnit); return AjinApi.SStop(axisNo);
            // 래퍼에 SetDeceleration 있으면 위처럼 적용. 없다면 프로젝트 표준대로 SStop 사용.
            return AjinApi.SStop(axisNo);
        }








        // ===== 내부 유틸 =====

        private static int MapJerk01ToDriver(double jerk01)
        {
            // Ajin이 0~1000 범위를 쓴다고 가정 — 실제 스펙에 맞게 조정
            if (jerk01 < 0) jerk01 = 0;
            if (jerk01 > 1) jerk01 = 1;
            return (int)Math.Round(jerk01 * 1000.0);
        }

        public double ReadCommandPulse(int axisNo) { return AjinApi.GetCommandPositionPulse(axisNo); }
        public double ReadErrorPulse(int axisNo) { return AjinApi.GetErrorPositionPulse(axisNo); }
        public double ReadCommandVelPulsePerSec(int axisNo) { return AjinApi.GetCommandVelocityPps(axisNo); }
        public double ReadActualVelPulsePerSec(int axisNo) { return AjinApi.GetActualVelocityPps(axisNo); }

        public bool ReadServoOn(int axisNo) { return AjinApi.GetServoOn(axisNo); }
        public bool ReadAlarm(int axisNo) { return AjinApi.GetAlarm(axisNo); }
        public bool ReadNegativeLimit(int axisNo) { return AjinApi.GetNlimit(axisNo); }
        public bool ReadPositiveLimit(int axisNo) { return AjinApi.GetPlimit(axisNo); }
        public bool ReadHomeSensor(int axisNo) { return AjinApi.GetHomeSensor(axisNo); }

        public bool ReadDone(int axisNo) { return AjinApi.MotionDone(axisNo); }
        public bool ReadInposition(int axisNo) { return AjinApi.GetInposition(axisNo); }
        public bool ReadInpositionDone(int axisNo) { return AjinApi.GetInpositionDone(axisNo); }
        public bool ReadInpositionTimeout(int axisNo) { return AjinApi.GetInpositionTimeout(axisNo); }
        public bool ReadHomeEnd(int axisNo) { return AjinApi.GetHomeEnd(axisNo); }
        public bool ReadHomeTimeout(int axisNo) { return AjinApi.GetHomeTimeout(axisNo); }

        // AjinDriver.cs
        public int ConfigureFromSetupAndConfig(int axisNo, MotionAxisSetup setup, MotionAxisConfig cfg)
        {
            // 최근 설정 캐시 (Home에서 재사용)
            _lastSetup = setup;
            _lastConfig = cfg;

            // 1) 배선/레벨 등 기본 셋업
            int rc = 0;
            //int rc = AjinApi.ApplySetupBasic(axisNo, setup);
            //if (rc != 0) return rc;

            // 2) 홈 파라미터 (속도/가속은 Config(mm기준)→pulse로 변환)
            double ppu = 1; //_pulsesPerUnit; // mm→pulse 재확인 필요
            double h1v = cfg.HomeFirstSpeed * ppu;
            double h2v = cfg.HomeSecondSpeed * ppu;
            double hlv = cfg.HomeThirdSpeed * ppu;
            double izv = cfg.HomeLastSpeed * ppu;
            double h1a = cfg.HomeFirstAcc * ppu;
            double h2a = cfg.HomeSecondAcc * ppu;

            rc = AjinApi.ApplyHomeFromSetup(axisNo, setup, h1v, h2v, hlv, izv, h1a, h2a);
            if (rc != 0) 
                return rc;

            // 3) 프로파일(주 운전 파라미터)
            double v = cfg.MaxVelocity * ppu;
            double a = cfg.RunAcc * ppu;
            double d = cfg.RunDec * ppu;
            if (ProfileMode == ProfileMode.SCurve)
                rc = AjinApi.ApplySCurveProfile(axisNo, v, a, d, jerk0to1000: (int)Math.Round(cfg.AccJerkPercent * 1.0), setup.PulsesPerUnit, setup.AxisScale);
            else
                rc = AjinApi.ApplyTrapProfile(axisNo, v, a, d, setup.PulsesPerUnit, setup.AxisScale);
            if (rc != 0) return rc;

            if ((rc = AjinApi.ApplyMotionSignal(axisNo, (uint)setup.StopMode, (uint)setup.EmergencyLevel, setup.AlarmLevel, 
                setup.AlarmResetLevel, setup.InPosition, setup.PositiveLimitLevel, setup.NegativeLimitLevel)) != 0)
                return rc;

            return 0;
        }

    }
}
