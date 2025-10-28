using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QMC.Common.Motions
{
    /// <summary>
    /// 실제 보드(Ajin, ACS 등) 제어를 위한 최소 추상화 인터페이스
    /// </summary>
    public interface IMotionDriver
    {
        int Home(int axisNo);
        bool IsHomeDone(int axisNo);

        int MoveAbsPulse(int axisNo, double targetPulse, double vel, double acc, double dec, double jerk);
        bool IsMoveDone(int axisNo);

        int Stop(int axisNo);       // 감속 정지
        int EmgStop(int axisNo);    // 즉시 정지
        int Servo(int axisNo, bool on);
        int ClearAlarm(int axisNo);

        double ReadActualPulse(int axisNo);
        int SetActualPulse(int axisNo, double pulse);
        int SetCommandPulse(int axisNo, double pulse);

        // ===== 추가: 상태 읽기 =====
        double ReadCommandPulse(int axisNo);
        double ReadErrorPulse(int axisNo);
        double ReadCommandVelPulsePerSec(int axisNo);
        double ReadActualVelPulsePerSec(int axisNo);

        bool ReadServoOn(int axisNo);
        bool ReadAlarm(int axisNo);
        bool ReadNegativeLimit(int axisNo);
        bool ReadPositiveLimit(int axisNo);
        bool ReadHomeSensor(int axisNo);

        bool ReadDone(int axisNo);              // motion done
        bool ReadInposition(int axisNo);
        bool ReadInpositionDone(int axisNo);
        bool ReadInpositionTimeout(int axisNo);
        bool ReadHomeEnd(int axisNo);
        bool ReadHomeTimeout(int axisNo);
    }
}
