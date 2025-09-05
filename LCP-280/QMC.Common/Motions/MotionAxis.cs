using QMC.Common.Alarm;
using QMC.Common.Component;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

// CKD
using QMC.Common.Motions.CKD;
using QMC.Common.Motion.Ajin;

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

    public sealed class MotionAxis : BaseComponent
    {
        #region Alarm
        public enum AlarmKey
        {
            FirstAlarm = 1000,
            Axis_XXXXXXXXX1_Fail,
            Axis_XXXXXXXXX2_Fail,
            Axis_XXXXXXXXX3_Fail
        }
        #endregion

        private readonly object _gate = new object();
        private readonly AjinDriver _driver;
        private readonly CKDMotorDriver _ckdDriver;

        public MotionAxisSetup Setup { get; set;  }
        public MotionAxisConfig Config { get; set; }
        public MotionAxisStatus Status { get; }
        private IPropertyCorrection _correction;

        public string Name { get { return Setup.Name; } }
        public int AxisNo { get { return Setup.AxisNo; } }

        public MotionAxis(MotionAxisSetup setup, MotionAxisConfig config, AjinDriver driver, IPropertyCorrection correction = null)
        {
            if (setup == null) throw new ArgumentNullException(nameof(setup));
            if (config == null) throw new ArgumentNullException(nameof(config));
            if (driver == null) throw new ArgumentNullException(nameof(driver));

            Setup = setup;
            Config = config;
            _driver = driver;

            Setup.Validate();
            Config.Validate();

            Status = new MotionAxisStatus();

            _correction = correction ?? new DefaultCorrection(Setup, Config);

            InitAlarm();
        }
        public MotionAxis(MotionAxisSetup setup, MotionAxisConfig config, CKDMotorDriver driver, IPropertyCorrection correction = null)
        {
            if (setup == null) throw new ArgumentNullException(nameof(setup));
            if (config == null) throw new ArgumentNullException(nameof(config));
            if (driver == null) throw new ArgumentNullException(nameof(driver));
            
            Setup = setup;
            Config = config;
            _ckdDriver = driver;
            
            Setup.Validate();
            Config.Validate();
            
            Status = new MotionAxisStatus();
            
            _correction = correction ?? new DefaultCorrection(Setup, Config);
            
            InitAlarm();
        }

        /// <summary>보정 레이어 교체(특수 보정 적용 시)</summary>
        public void SetCorrection(IPropertyCorrection correction)
        {
            if (correction == null) throw new ArgumentNullException(nameof(correction));
            lock (_gate) { _correction = correction; }
        }

        // ===== 장비 내부 알람 =====
        protected override void InitAlarm()
        {
            string strTemp = string.Empty;
            strTemp = string.Format(Name + "Axis Error");
            AlarmInfo alarm = new AlarmInfo();
            alarm.Code = (int)AlarmKey.Axis_XXXXXXXXX1_Fail;
            alarm.Title = "strTemp";
            alarm.Cause = Name + "Axis_XXXXXXXXX1_Fail";
            alarm.Source = Name;
            alarm.Grade = "Error";
            m_dicAlarms.Add(alarm.Code, alarm);

            alarm.Code = (int)AlarmKey.Axis_XXXXXXXXX2_Fail;
            alarm.Title = "strTemp";
            alarm.Cause = Name + "Axis_XXXXXXXXX1_Fail";
            alarm.Source = Name;
            alarm.Grade = "Error";
            m_dicAlarms.Add(alarm.Code, alarm);

            alarm.Code = (int)AlarmKey.Axis_XXXXXXXXX3_Fail;
            alarm.Title = "strTemp";
            alarm.Cause = Name + "Axis_XXXXXXXXX1_Fail";
            alarm.Source = Name;
            alarm.Grade = "Error";
            m_dicAlarms.Add(alarm.Code, alarm);
        }

        public int AlarmPost(AlarmKey AlarmCode)
        {
            try
            {
                AlarmInfo alarm = GetAlarm((int)AlarmCode);
                alarm.GeneratedTime = DateTime.Now;

                // 중복 알람 방지 인터락
                if (AlarmManager.Instance.Alarms.Any(a => a.Code == alarm.Code))
                {
                    //Log.Write("AlarmPost", $"[ALARM 무시 - 중복] Code: {(int)AlarmCode}, 이미 발생 중인 알람입니다.");
                    return (int)AlarmCode;
                }

                // 알람 정보 로그 기록
                Log.Write("AlarmPost", $"[ALARM 발생] Code: {(int)AlarmCode}, Grade: {alarm.Grade}, Cause: {alarm.Cause}");

                string logFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AlarmLog");
                string logFile = Path.Combine(logFolder, $"AlarmLog_{DateTime.Now:yyyyMMdd}.csv");
                Directory.CreateDirectory(logFolder);

                // UTF-8 with BOM로 저장
                using (var fs = new FileStream(logFile, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                using (var writer = new StreamWriter(fs, new UTF8Encoding(true)))
                {
                    string logLine = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss},{alarm.Title},{alarm.Grade},{alarm.Source},{alarm.Cause},{(int)AlarmCode}";
                    writer.WriteLine(logLine);
                }

                if (alarm.Grade.Equals("Error"))
                {
                    //장비 내부 멈춰야 하는 이것저것
                    //this.m_LoaderWork_Start = false;
                }
                AlarmManager.Instance.ShowAlarm(alarm);
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }

            return (int)AlarmCode;
        }


        // ===== 상태 =====
        public double GetPosition()  // 논리 단위(mm/deg)
        {
            if (_driver != null)
            {
                var pulse = _driver.ReadActualPulse(AxisNo);
                return _correction.ToLogical(pulse);
            }
            else if (_ckdDriver != null)
            {
                var degree = _ckdDriver.GetPositionDegree() / 1000.0;
                return degree;
            }
            else
            {
                throw new InvalidOperationException("No valid driver assigned.");
            }
        }

        public bool InPosition(double logicalTarget)
        {
            if (_driver != null)
            {
                var pos = GetPosition();
                return Math.Abs(pos - logicalTarget) <= Config.InposTolerance;
            }
            else if (_ckdDriver != null)
            {
                return _ckdDriver.IsInPosition();
            }
            else
            {
                throw new InvalidOperationException("No valid driver assigned.");
            }
        }

        // ===== 동작 =====
        public int HomeSync()
        {
            if (_driver != null)
            {
                var rc = _driver.Home(AxisNo);
                if (rc != 0) return rc;

                var sw = Stopwatch.StartNew();
                while (sw.ElapsedMilliseconds < Setup.HomeTimeoutMs)
                {
                    if (_driver.IsHomeDone(AxisNo)) return 0;
                    Thread.Sleep(5);
                }
            }
            else if (_ckdDriver != null)
            {
                var rc = _ckdDriver.HomeSearch();
                if (rc != 0) return rc;
                var sw = Stopwatch.StartNew();
                while (sw.ElapsedMilliseconds < Setup.HomeTimeoutMs)
                {
                    if (_ckdDriver.IsHomePosition() && _ckdDriver.IsInPosition()) return 0;
                    Thread.Sleep(5);
                }
            }
            else
            {
                throw new InvalidOperationException("This axis does not support HomeSync.");
            }
            return -1; // timeout
        }

        public int HomeAsync()
        {
            if (_driver != null)
            {
                var rc = _driver.Home(AxisNo);
                if (rc != 0) return rc;
            }
            else if (_ckdDriver != null)
            {
                var rc = _ckdDriver.HomeSearch();
                if (rc != 0) return rc;
            }
            else
            {
                throw new InvalidOperationException("This axis does not support HomeAsync.");
            }
            return 0;
        }

        public int MoveAbs(double logicalTarget, double vel = 5, double acc = 10, double dec = 10, double jerkPercent = 50)
        {
            if (_driver != null)
            {
                GuardSoftLimit(logicalTarget);
                var p = _correction.ToHardware(logicalTarget);
                var jerk = MapJerkPercentToDriver((int)jerkPercent, (int)jerkPercent);
                return _driver.MoveAbsPulse(AxisNo, p, vel, acc, dec, jerk);
            }
            else
            {
                throw new InvalidOperationException("This axis does not support Absolute Move.");
            }
        }

        public int MoveRel(double logicalTarget, double vel, double acc, double dec, double jerkPercent)
        {
            if (_driver != null)
            {
                var target = GetPosition() + logicalTarget;
                return MoveAbs(target, vel, acc, dec, jerkPercent);
            }
            else
            {
                throw new InvalidOperationException("This axis does not support Relative Move.");
            }
        }

        public int MoveNextIndex()
        {
            if (_ckdDriver != null)
            {
                return _ckdDriver.RunProgram(CKDMotorDriver.ProgramNumber.Incremental_Div8_CCW);
            }
            else
            {
                throw new InvalidOperationException("This axis does not support MoveNextIndex.");
            }
        }

        public int MovePrevIndex()
        {
            if (_ckdDriver != null)
            {
                return _ckdDriver.RunProgram(CKDMotorDriver.ProgramNumber.Incremental_Div8_CW);
            }
            else
            {
                throw new InvalidOperationException("This axis does not support MovePrevIndex.");
            }
        }

        public int WaitMoveDone(int timeoutMs)
        {
            if (_driver != null)
            {
                if (timeoutMs < 0) timeoutMs = Setup.MoveTimeoutMs;
                var sw = Stopwatch.StartNew();
                while (sw.ElapsedMilliseconds < timeoutMs)
                {
                    if (_driver.IsMoveDone(AxisNo)) return 0;
                    Thread.Sleep(5);
                }
            }
            else if(_ckdDriver != null)
            {
                if (timeoutMs < 0) timeoutMs = Setup.MoveTimeoutMs;
                var sw = Stopwatch.StartNew();
                while (sw.ElapsedMilliseconds < timeoutMs)
                {
                    if (_ckdDriver.IsInPosition() && _ckdDriver.IsRunWait()) return 0;
                    Thread.Sleep(5);
                }
            }
            else
            {
                throw new InvalidOperationException("No valid driver assigned.");
            }
            return -1;
        }

        public void JogStart(double signedVel)
        {
            if (_driver != null)
            {
                try { this.Servo(true); } catch (Exception ex) { Log.Write(ex); }

                double dAcc = 10; // Config.JogAcc;
                double dDec = 10; // Config.JogDec;

                var drv = this._driver as AjinDriver;
                if (drv != null) drv.JogVelStart(this.AxisNo, signedVel, dAcc, dDec);
            }
            else
            {
                throw new InvalidOperationException("This axis does not support Jog Move.");
            }
        }

        public void JogStop()
        {
            if (_driver != null)
            {
                var drv = this._driver as AjinDriver;
                if (drv != null) drv.JogStop(this.AxisNo);
            }
            else
            {
                throw new InvalidOperationException("This axis does not support Jog Stop.");
            }
        }

        public void JogEStop()
        {
            if (_driver != null)
            {
                var drv = this._driver as AjinDriver;
                if (drv != null) drv.JogEStop(this.AxisNo);
            }
            else
            {
                throw new InvalidOperationException("This axis does not support Jog EStop.");
            }
        }


        public int Stop() 
        { 
            if (_driver != null)
            {
                return _driver.Stop(AxisNo);
            }
            else if(_ckdDriver != null)
            {
                // CKD에서 정지 Command가 있는지 확인 필요
                return 0;
            }
            else
            {
                throw new InvalidOperationException("This axis does not support Stop.");
            }
        }
        public int EmgStop() 
        {   
            if (_driver != null)
            {
                return _driver.EmgStop(AxisNo);
            }
            else if(_ckdDriver != null)
            {
                // CKD에서 Software Emergency Stop Command는 있지만 급정지(E-Stop)은 없음
                return 0;
            }
            else
            {
                throw new InvalidOperationException("This axis does not support EStop.");
            }
        }
        public int Servo(bool on) 
        {   
            if (_driver != null)
            {
                return _driver.Servo(AxisNo, on);
            }
            else if (_ckdDriver != null)
            {
                return _ckdDriver.Servo(on);
            }
            else
            {
                throw new InvalidOperationException("This axis does not support Servo On/Off.");
            }
        }
        public int ClearAlarm() 
        { 
            if (_driver != null)
            {
                return _driver.ClearAlarm(AxisNo);
            }
            else if(_ckdDriver != null)
            {
                return _ckdDriver.AlarmReset();
            }
            else
            {
                throw new InvalidOperationException("This axis does not support ClearAlarm.");
            }
        }

        /// <summary>현재 위치를 논리값으로 재설정(Actual/Command 동기화)</summary>
        public int SetPositionLogical(double logicalValue)
        {
            if (_driver != null)
            {
                var p = _correction.ToHardware(logicalValue);
                var rc = _driver.SetActualPulse(AxisNo, p);
                if (rc != 0) return rc;
                return _driver.SetCommandPulse(AxisNo, p);
            }
            else
            {
                throw new InvalidOperationException("This axis does not support SetPosition.");
            }
        }

        /// <summary>
        /// 현재 Setup/Config를 AjinDriver 하드웨어에 적용
        /// (CKD나 다른 드라이버는 무시)
        /// </summary>
        public int ApplyToDriver()
        {
            if (_driver != null)
            {
                try
                {
                    // 프로파일 모드 동기화
                    _driver.ProfileMode = Config.ProfileMode;
                    
                    // 하드웨어 설정 적용
                    return _driver.ConfigureFromSetupAndConfig(Setup.AxisNo, Setup, Config);
                }
                catch (Exception ex)
                {
                    Log.Write("MotionAxis", $"ApplyToDriver 실패 - 축: {Name}, 오류: {ex.Message}");
                    return -1;
                }
            }
            return 0; // CKD나 다른 드라이버는 성공으로 처리
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
            t.HomeFirstSpeed = c.HomeFirstSpeed;
            t.HomeSecondSpeed = c.HomeSecondSpeed;
            t.HomeThirdSpeed = c.HomeThirdSpeed;
            t.HomeLastSpeed = c.HomeLastSpeed;
            t.HomeFirstAcc = c.HomeFirstAcc;
            t.HomeSecondAcc = c.HomeSecondAcc;
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
            t.PulseOutput = s.PulseOutput;
            t.EncoderInput = s.EncoderInput;
            t.InputSource = s.InputSource;
            t.ZPhaseLevel = s.ZPhaseLevel;
            t.ServoOnLevel = s.ServoOnLevel;
            t.EmergencyLevel = s.EmergencyLevel;
            t.StopMode = s.StopMode;
            t.InpositionLevel = s.InpositionLevel;
            t.SoftwareLimitEnable = s.SoftwareLimitEnable;
            t.SoftwareLength = s.SoftwareLength;
            t.HomeSignalLevel = s.HomeSignalLevel;
            t.HomeMode = s.HomeMode;
            t.HomeClearTime = s.HomeClearTime;
            t.HomeOffset = s.HomeOffset;
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
            dst.PulseOutput = src.PulseOutput;
            dst.EncoderInput = src.EncoderInput;
            dst.InputSource = src.InputSource;
            dst.ZPhaseLevel = src.ZPhaseLevel;
            dst.ServoOnLevel = src.ServoOnLevel;
            dst.EmergencyLevel = src.EmergencyLevel;
            dst.StopMode = src.StopMode;
            dst.InpositionLevel = src.InpositionLevel;
            dst.SoftwareLimitEnable = src.SoftwareLimitEnable;
            dst.SoftwareLength = src.SoftwareLength;
            dst.HomeSignalLevel = src.HomeSignalLevel;
            dst.HomeMode = src.HomeMode;
            dst.HomeClearTime = src.HomeClearTime;
            dst.HomeOffset = src.HomeOffset;
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
            if (_driver != null)
            {
                // 드라이버는 pulse/초 단위 반환 → 보정계층으로 논리단위로 변환
                var pulsesPerUnit = Setup.PulsesPerUnit;

                var cmdPulse = _driver.ReadCommandPulse(AxisNo);
                var actPulse = _driver.ReadActualPulse(AxisNo);
                var errPulse = _driver.ReadErrorPulse(AxisNo);

                var cmdVelPps = _driver.ReadCommandVelPulsePerSec(AxisNo);
                var actVelPps = _driver.ReadActualVelPulsePerSec(AxisNo);

                Status.PV.CommandPosition = _correction.ToLogical(cmdPulse);
                Status.PV.ActualPosition = _correction.ToLogical(actPulse);
                Status.PV.ErrorPosition = _correction.ToLogical(errPulse);
                Status.PV.CommandVelocity = cmdVelPps / pulsesPerUnit;   // pulse/s → unit/s
                Status.PV.ActualVelocity = actVelPps / pulsesPerUnit;

                Status.IO.ServoOn = _driver.ReadServoOn(AxisNo);
                Status.IO.Alarm = _driver.ReadAlarm(AxisNo);
                Status.IO.NegativeLimitSensor = _driver.ReadNegativeLimit(AxisNo);
                Status.IO.PositiveLimitSensor = _driver.ReadPositiveLimit(AxisNo);
                Status.IO.HomeSensor = _driver.ReadHomeSensor(AxisNo);

                Status.State.Done = _driver.ReadDone(AxisNo);
                Status.State.Inposition = _driver.ReadInposition(AxisNo);
                Status.State.InpositionDone = _driver.ReadInpositionDone(AxisNo);
                Status.State.InpositionTimeout = _driver.ReadInpositionTimeout(AxisNo);
                Status.State.HomeEnd = _driver.ReadHomeEnd(AxisNo);
                Status.State.HomeTimeout = _driver.ReadHomeTimeout(AxisNo);

                Status.TimestampUtc = DateTime.UtcNow;
                return Status;
            }
            else if (_ckdDriver != null)
            {
                var cmdDeg = _ckdDriver.GetPositionDegree();
                var actDeg = cmdDeg;
                var errDeg = _ckdDriver.GetErrorDegree();

                var cmdVelRpm = _ckdDriver.GetVelocity();
                var actVelRpm = cmdVelRpm;

                Status.PV.CommandPosition = cmdDeg;
                Status.PV.ActualPosition = actDeg;
                Status.PV.ErrorPosition = errDeg;
                Status.PV.CommandVelocity = cmdVelRpm;
                Status.PV.ActualVelocity = actVelRpm;

                Status.IO.ServoOn = _ckdDriver.IsServoOn();
                Status.IO.Alarm = _ckdDriver.IsAlarm();
                Status.IO.NegativeLimitSensor = false; // DD Motor에 NegativeLimit 없음
                Status.IO.PositiveLimitSensor = false; // DD Motor에 PositiveLimit 없음
                Status.IO.HomeSensor = _ckdDriver.IsHomePosition();

                Status.State.Done = _ckdDriver.IsInPosition() && _ckdDriver.IsRunWait();
                Status.State.Inposition = _ckdDriver.IsInPosition();
                Status.State.InpositionDone = _ckdDriver.IsInPosition() && _ckdDriver.IsRunWait();
                Status.State.InpositionTimeout = false; // 확인 필요.?
                Status.State.HomeEnd = _ckdDriver.IsHomePosition() && _ckdDriver.IsInPosition() && _ckdDriver.IsRunWait();
                Status.State.HomeTimeout = false; // 확인 필요.?
            }
            return Status;
        }
    }
}
