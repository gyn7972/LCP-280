using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace QMC.Common.Motions
{
    //사용 예시
    /*
    준비된 Setup/Config
    var setupX = new MotionAxisSetup { Name="X", AxisNo=0, PulsesPerUnit=1000, SoftLimitEnable=true, SoftLimitMin=-10, SoftLimitMax=310 };
    var cfgX   = new MotionAxisConfig { MaxVelocity=200, RunAcc=800, RunDec=800, InposTolerance=0.002, ProfileMode=ProfileMode.SCurve, AccJerkPercent=50, DecJerkPercent=50 };

    // AjinDriver 주입 (논리단위 사용 → 내부에서 pulse 변환)
    IMotionDriver ajin = new AjinDriver(boardNo: 0, pulsesPerUnit: setupX.PulsesPerUnit, useLogicalUnits: true);

    // 단일 축
    var axisX = new MotionAxis(setupX, cfgX, ajin);

    // 홈 → 이동
    axisX.Servo(true);
    axisX.HomeSync();
    axisX.MoveAbs(100.0);
    axisX.WaitMoveDone(-1);
     */

    public sealed class MotionAxis
    {
        private readonly object _gate = new object();
        private readonly IMotionDriver _driver;

        public MotionAxisSetup Setup { get; }
        public MotionAxisConfig Config { get; private set; }
        private IPropertyCorrection _correction;

        public string Name { get { return Setup.Name; } }
        public int AxisNo { get { return Setup.AxisNo; } }

        public MotionAxis(MotionAxisSetup setup, MotionAxisConfig config, IMotionDriver driver, IPropertyCorrection correction = null)
        {
            if (setup == null) throw new ArgumentNullException(nameof(setup));
            if (config == null) throw new ArgumentNullException(nameof(config));
            if (driver == null) throw new ArgumentNullException(nameof(driver));

            Setup = setup;
            Config = config;
            _driver = driver;

            Setup.Validate();
            Config.Validate();

            _correction = correction ?? new DefaultCorrection(Setup, Config);
        }

        /// <summary>보정 레이어 교체(특수 보정 적용 시)</summary>
        public void SetCorrection(IPropertyCorrection correction)
        {
            if (correction == null) throw new ArgumentNullException(nameof(correction));
            lock (_gate) { _correction = correction; }
        }

        // ===== 상태 =====
        public double GetPosition()  // 논리 단위(mm/deg)
        {
            var pulse = _driver.ReadActualPulse(AxisNo);
            return _correction.ToLogical(pulse);
        }

        public bool InPosition(double logicalTarget)
        {
            var pos = GetPosition();
            return Math.Abs(pos - logicalTarget) <= Config.InposTolerance;
        }

        // ===== 동작 =====
        public int HomeSync()
        {
            var rc = _driver.Home(AxisNo);
            if (rc != 0) return rc;

            var sw = Stopwatch.StartNew();
            while (sw.ElapsedMilliseconds < Setup.HomeTimeoutMs)
            {
                if (_driver.IsHomeDone(AxisNo)) return 0;
                Thread.Sleep(5);
            }
            return -1; // timeout
        }

        public int MoveAbs(double logicalTarget)
        {
            GuardSoftLimit(logicalTarget);
            var p = _correction.ToHardware(logicalTarget);
            var jerk = MapJerkPercentToDriver(Config.AccJerkPercent, Config.DecJerkPercent);
            return _driver.MoveAbsPulse(AxisNo, p, Config.MaxVelocity, Config.RunAcc, Config.RunDec, jerk);
        }

        public int MoveAbs(double logicalTarget, double vel, double acc, double dec, double jerkPercent)
        {
            GuardSoftLimit(logicalTarget);
            var p = _correction.ToHardware(logicalTarget);
            var jerk = MapJerkPercentToDriver((int)jerkPercent, (int)jerkPercent);
            return _driver.MoveAbsPulse(AxisNo, p, vel, acc, dec, jerk);
        }

        public int MoveRel(double logicalDelta)
        {
            var target = GetPosition() + logicalDelta;
            return MoveAbs(target);
        }

        public int WaitMoveDone(int timeoutMs)
        {
            if (timeoutMs < 0) timeoutMs = Setup.MoveTimeoutMs;
            var sw = Stopwatch.StartNew();
            while (sw.ElapsedMilliseconds < timeoutMs)
            {
                if (_driver.IsMoveDone(AxisNo)) return 0;
                Thread.Sleep(5);
            }
            return -1;
        }

        public int Stop() { return _driver.Stop(AxisNo); }
        public int EmgStop() { return _driver.EmgStop(AxisNo); }
        public int Servo(bool on) { return _driver.Servo(AxisNo, on); }
        public int ClearAlarm() { return _driver.ClearAlarm(AxisNo); }

        /// <summary>현재 위치를 논리값으로 재설정(Actual/Command 동기화)</summary>
        public int SetPositionLogical(double logicalValue)
        {
            var p = _correction.ToHardware(logicalValue);
            var rc = _driver.SetActualPulse(AxisNo, p);
            if (rc != 0) return rc;
            return _driver.SetCommandPulse(AxisNo, p);
        }

        // ===== 내부 유틸 =====
        private void GuardSoftLimit(double logicalTarget)
        {
            if (!Setup.SoftLimitEnable) return;
            if (logicalTarget < Setup.SoftLimitMin || logicalTarget > Setup.SoftLimitMax)
                throw new InvalidOperationException(
                    "[" + Name + "] SoftLimit violation: " + logicalTarget + " ∉ [" + Setup.SoftLimitMin + ", " + Setup.SoftLimitMax + "]");
        }

        // 드라이버가 사용하는 jerk 스케일로 매핑(보드별로 교체하세요)
        private static double MapJerkPercentToDriver(int accJerkPercent, int decJerkPercent)
        {
            // 예: 0~100(%) → 0~1.0 (필요 시 0~1000 등으로 변경)
            var a = accJerkPercent < 0 ? 0 : (accJerkPercent > 100 ? 100 : accJerkPercent);
            var d = decJerkPercent < 0 ? 0 : (decJerkPercent > 100 ? 100 : decJerkPercent);
            // 단일 값만 받는 드라이버라면 평균 사용
            return (a + d) / 200.0;
        }


        // ---- 스냅샷 반환(복사본) ----
        public MotionAxisSetup GetSetupSnapshot()
        {
            lock (_gate) { return CloneSetup(Setup); }
        }

        public MotionAxisConfig GetConfigSnapshot()
        {
            lock (_gate) { return CloneConfig(Config); }
        }

        // ---- 적용(검증 후 반영) ----
        public void ApplySetup(MotionAxisSetup newSetup)
        {
            if (newSetup == null) throw new ArgumentNullException(nameof(newSetup));
            newSetup.Validate();

            lock (_gate)
            {
                CopySetup(Setup, newSetup);                     // 참조 유지 + 값 복사
                _correction = new DefaultCorrection(Setup, Config);
            }
        }

        public void ApplyConfig(MotionAxisConfig newConfig)
        {
            if (newConfig == null) throw new ArgumentNullException(nameof(newConfig));
            newConfig.Validate();

            lock (_gate)
            {
                Config = CloneConfig(newConfig);                // 내부 보관은 복사본
                _correction = new DefaultCorrection(Setup, Config);
            }
        }

        // ---- 선택: 델리게이트 기반 편집(스냅샷 가져와서 수정 후 적용) ----
        public void EditSetup(Action<MotionAxisSetup> editor)
        {
            if (editor == null) throw new ArgumentNullException(nameof(editor));
            var snap = GetSetupSnapshot();
            editor(snap);
            ApplySetup(snap);
        }

        public void EditConfig(Action<MotionAxisConfig> editor)
        {
            if (editor == null) throw new ArgumentNullException(nameof(editor));
            var snap = GetConfigSnapshot();
            editor(snap);
            ApplyConfig(snap);
        }

        // === 스냅샷/복사 유틸 (C# 7.3: record X → 수동 복사) ===
        private static MotionAxisConfig CloneConfig(MotionAxisConfig c)
        {
            var t = new MotionAxisConfig();
            t.HomeSpeed = c.HomeSpeed;
            t.HomeReturnSpeed = c.HomeReturnSpeed;
            t.HomeRecursionSpeed = c.HomeRecursionSpeed;
            t.ZPhaseSpeed = c.ZPhaseSpeed;
            t.HomeAcc = c.HomeAcc;
            t.HomeReturnAcc = c.HomeReturnAcc;
            t.JogFineVelocity = c.JogFineVelocity;
            t.JogCoarseVelocity = c.JogCoarseVelocity;
            t.JogAcc = c.JogAcc;
            t.JogDec = c.JogDec;
            t.MaxVelocity = c.MaxVelocity;
            t.RunAcc = c.RunAcc;
            t.RunDec = c.RunDec;
            t.ProfileMode = c.ProfileMode;
            t.AccJerkPercent = c.AccJerkPercent;
            t.DecJerkPercent = c.DecJerkPercent;
            t.InposTolerance = c.InposTolerance;
            t.LogicalScaleFactor = c.LogicalScaleFactor;
            t.Offset = c.Offset;
            return t;
        }

        private static MotionAxisSetup CloneSetup(MotionAxisSetup s)
        {
            var t = new MotionAxisSetup();
            t.Name = s.Name;
            t.BoardNo = s.BoardNo;
            t.AxisNo = s.AxisNo;
            t.PulsesPerUnit = s.PulsesPerUnit;
            t.AxisScale = s.AxisScale;
            t.AxisPowerPercent = s.AxisPowerPercent;
            t.OutputMode = s.OutputMode;
            t.InputMode = s.InputMode;
            t.InputSource = s.InputSource;
            t.ZPhaseLevel = s.ZPhaseLevel;
            t.ServoLevel = s.ServoLevel;
            t.EmergencyLevel = s.EmergencyLevel;
            t.StopMode = s.StopMode;
            t.InpositionLevel = s.InpositionLevel;
            t.SoftwareLimitEnable = s.SoftwareLimitEnable;
            t.SoftwareLength = s.SoftwareLength;
            t.HomeSignalLevel = s.HomeSignalLevel;
            t.HomeMode = s.HomeMode;
            t.AlarmResetSignal = s.AlarmResetSignal;
            t.AlarmLevel = s.AlarmLevel;
            t.SoftLimitEnable = s.SoftLimitEnable;
            t.SoftLimitMin = s.SoftLimitMin;
            t.SoftLimitMax = s.SoftLimitMax;
            t.HomeTimeoutMs = s.HomeTimeoutMs;
            t.MoveTimeoutMs = s.MoveTimeoutMs;
            return t;
        }

        private static void CopySetup(MotionAxisSetup dst, MotionAxisSetup src)
        {
            // dst 참조 유지(의존 객체가 Setup 참조 중일 수 있으니)
            dst.Name = src.Name;
            dst.BoardNo = src.BoardNo;
            dst.AxisNo = src.AxisNo;
            dst.PulsesPerUnit = src.PulsesPerUnit;
            dst.AxisScale = src.AxisScale;
            dst.AxisPowerPercent = src.AxisPowerPercent;
            dst.OutputMode = src.OutputMode;
            dst.InputMode = src.InputMode;
            dst.InputSource = src.InputSource;
            dst.ZPhaseLevel = src.ZPhaseLevel;
            dst.ServoLevel = src.ServoLevel;
            dst.EmergencyLevel = src.EmergencyLevel;
            dst.StopMode = src.StopMode;
            dst.InpositionLevel = src.InpositionLevel;
            dst.SoftwareLimitEnable = src.SoftwareLimitEnable;
            dst.SoftwareLength = src.SoftwareLength;
            dst.HomeSignalLevel = src.HomeSignalLevel;
            dst.HomeMode = src.HomeMode;
            dst.AlarmResetSignal = src.AlarmResetSignal;
            dst.AlarmLevel = src.AlarmLevel;
            dst.SoftLimitEnable = src.SoftLimitEnable;
            dst.SoftLimitMin = src.SoftLimitMin;
            dst.SoftLimitMax = src.SoftLimitMax;
            dst.HomeTimeoutMs = src.HomeTimeoutMs;
            dst.MoveTimeoutMs = src.MoveTimeoutMs;
        }

        //
        public MotionAxisStatus GetStatusSnapshot()
        {
            // 드라이버는 pulse/초 단위 반환 → 보정계층으로 논리단위로 변환
            var s = new MotionAxisStatus();
            var pulsesPerUnit = Setup.PulsesPerUnit;

            var cmdPulse = _driver.ReadCommandPulse(AxisNo);
            var actPulse = _driver.ReadActualPulse(AxisNo);
            var errPulse = _driver.ReadErrorPulse(AxisNo);

            var cmdVelPps = _driver.ReadCommandVelPulsePerSec(AxisNo);
            var actVelPps = _driver.ReadActualVelPulsePerSec(AxisNo);

            s.PV.CommandPosition = _correction.ToLogical(cmdPulse);
            s.PV.ActualPosition = _correction.ToLogical(actPulse);
            s.PV.ErrorPosition = _correction.ToLogical(errPulse);
            s.PV.CommandVelocity = cmdVelPps / pulsesPerUnit;   // pulse/s → unit/s
            s.PV.ActualVelocity = actVelPps / pulsesPerUnit;

            s.IO.ServoOn = _driver.ReadServoOn(AxisNo);
            s.IO.Alarm = _driver.ReadAlarm(AxisNo);
            s.IO.NegativeLimitSensor = _driver.ReadNegativeLimit(AxisNo);
            s.IO.PositiveLimitSensor = _driver.ReadPositiveLimit(AxisNo);
            s.IO.HomeSensor = _driver.ReadHomeSensor(AxisNo);

            s.State.Done = _driver.ReadDone(AxisNo);
            s.State.Inposition = _driver.ReadInposition(AxisNo);
            s.State.InpositionDone = _driver.ReadInpositionDone(AxisNo);
            s.State.InpositionTimeout = _driver.ReadInpositionTimeout(AxisNo);
            s.State.HomeEnd = _driver.ReadHomeEnd(AxisNo);
            s.State.HomeTimeout = _driver.ReadHomeTimeout(AxisNo);

            s.TimestampUtc = DateTime.UtcNow;
            return s;
        }
    }
}
