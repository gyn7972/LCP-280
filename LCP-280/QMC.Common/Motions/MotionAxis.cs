using QMC.Common.Alarm;
using QMC.Common.Component;
using QMC.Common.Motion.Ajin;
// CKD
using QMC.Common.Motions.CKD;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms.DataVisualization.Charting;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

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
            Axis_XXXXXXXXX3_Fail,
            AxisHomeTimeout // 홈 타임아웃 추가
        }
        #endregion

        // === 홈 성공 알림 이벤트(글로벌) ===
        public static event Action<MotionAxis> HomeSucceeded;

        private readonly object _gate = new object();
        private readonly AjinDriver _driver;
        private readonly CKDMotorDriver _ckdDriver;

        public MotionAxisSetup Setup { get; set;  }
        public MotionAxisConfig Config { get; set; }
        public MotionAxisStatus Status { get; }
        private IPropertyCorrection _correction;

        public string Name { get { return Setup.Name; } }
        public int AxisNo { get { return Setup.AxisNo; } }

        // Homed 래치(성공 시 true, 필요 시 ClearHomeLatch로 초기화)
        public bool IsHomedLatched { get; private set; }

        // ===== Simulation state =====
        private bool IsSim { get { return Config != null && Setup.IsSimulation; } }
        private readonly object _simLock = new object();
        private CancellationTokenSource _simMoveCts;
        private Task _simMoveTask;
        private double _simPosition;            // unit (mm/deg)
        private double _simCommandPosition;     // unit
        private double _simTarget;              // unit
        private double _simCurrentVelocity;     // unit/s (signed)
        private bool _simIsMoving;
        private bool _simServoOn;
        private bool _simAlarm;
        private bool _simHomeSensor;

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

            // 홈 타임아웃 알람 등록
            alarm = new AlarmInfo();
            alarm.Code = (int)AlarmKey.AxisHomeTimeout;
            alarm.Title = Name + " Home Timeout";
            alarm.Cause = Name + " home operation timed out";
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
            if (IsSim)
            {
                lock (_simLock) return _simPosition;
            }
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
            if (IsSim)
            {
                lock (_simLock)
                {
                    return Math.Abs(_simPosition - logicalTarget) <= Config.InposTolerance;
                }
            }
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
            if (IsSim)
            {
                // 간단한 홈 시뮬레이션: 0.5초 대기 후 위치 0으로 세팅(+오프셋 적용)
                try
                {
                    lock (_simLock)
                    {
                        if (!_simServoOn) _simServoOn = true;
                        _simAlarm = false;
                        _simIsMoving = true;
                        _simCurrentVelocity = 0;
                    }
                    Thread.Sleep(500);
                    lock (_simLock)
                    {
                        _simPosition = 0 + Setup.HomeOffset;
                        _simCommandPosition = _simPosition;
                        _simHomeSensor = true;
                        _simIsMoving = false;
                        _simCurrentVelocity = 0;
                    }
                    IsHomedLatched = true;
                    try { var h = HomeSucceeded; if (h != null) h(this); } catch { }
                    return 0;
                }
                catch { return -1; }
            }

            if (_driver != null)
            {
                var rc = _driver.Home(AxisNo);
                if (rc != 0) return rc;

                var sw = Stopwatch.StartNew();
                while (sw.ElapsedMilliseconds < Setup.HomeTimeoutMs)
                {
                    if (_driver.IsHomeDone(AxisNo))
                    {
                        IsHomedLatched = true;
                        try { var h = HomeSucceeded; if (h != null) h(this); } catch { }
                        return 0;
                    }
                    Thread.Sleep(5);
                }

                try { _driver.Stop(AxisNo); } catch { }
                try { AlarmPost(AlarmKey.AxisHomeTimeout); } catch { }
            }
            else if (_ckdDriver != null)
            {
                var rc = _ckdDriver.HomeSearch();
                if (rc != 0) return rc;

                // (선택) 상태 모니터링 시작
                try { _ckdDriver.StartReadInputDataMonitoring(); } catch { }

                var sw = Stopwatch.StartNew();
                int stable = 0;
                const int requiredStable = 3; // 연속 3회

                while (sw.ElapsedMilliseconds < Setup.HomeTimeoutMs)
                {
                    bool home = _ckdDriver.IsHomePosition();
                    bool inpos = _ckdDriver.IsInPosition();
                    bool idle = _ckdDriver.IsRunWait();

                    if (home && inpos && idle)
                    {
                        stable++;
                        if (stable >= requiredStable)
                        {
                            IsHomedLatched = true;
                            try { var h = HomeSucceeded; if (h != null) h(this); } catch { }
                            return 0;
                        }
                    }
                    else
                    {
                        stable = 0;
                    }
                    Thread.Sleep(10);
                }

                try { _ckdDriver.EmergencyStop(); } catch { }
                try { AlarmPost(AlarmKey.AxisHomeTimeout); } catch { }
            }
            else
            {
                throw new InvalidOperationException("This axis does not support HomeSync.");
            }
            return -1;
        }

        public int HomeAsync()
        {
            if (IsSim)
            {
                Task.Run(async () =>
                {
                    try
                    {
                        lock (_simLock) { _simIsMoving = true; _simAlarm = false; if (!_simServoOn) _simServoOn = true; }
                        await Task.Delay(500).ConfigureAwait(false);
                        lock (_simLock)
                        {
                            _simPosition = 0 + Setup.HomeOffset;
                            _simCommandPosition = _simPosition;
                            _simHomeSensor = true;
                            _simIsMoving = false;
                            _simCurrentVelocity = 0;
                        }
                        IsHomedLatched = true;
                        try { var h = HomeSucceeded; if (h != null) h(this); } catch { }
                    }
                    catch { }
                });
                return 0;
            }

            if (_driver != null)
            {
                var rc = _driver.Home(AxisNo);
                if (rc != 0) return rc;

                Task.Run(async () =>
                {
                    try
                    {
                        var sw = Stopwatch.StartNew();
                        bool ok = false;
                        while (sw.ElapsedMilliseconds < Setup.HomeTimeoutMs)
                        {
                            if (_driver.IsHomeDone(AxisNo))
                            {
                                IsHomedLatched = true;
                                try { var h = HomeSucceeded; if (h != null) h(this); } catch { }
                                ok = true; break;
                            }
                            await Task.Delay(5).ConfigureAwait(false);
                        }
                        if (!ok) { try { _driver.Stop(AxisNo); } catch { } try { AlarmPost(AlarmKey.AxisHomeTimeout); } catch { } }
                    }
                    catch { }
                });
            }
            else if (_ckdDriver != null)
            {
                var rc = _ckdDriver.HomeSearch();
                if (rc != 0) return rc;

                Task.Run(async () =>
                {
                    try
                    {
                        try { _ckdDriver.StartReadInputDataMonitoring(); } catch { }
                        var sw = Stopwatch.StartNew();
                        int stable = 0, requiredStable = 3;
                        bool ok = false;

                        while (sw.ElapsedMilliseconds < Setup.HomeTimeoutMs)
                        {
                            bool home = _ckdDriver.IsHomePosition();
                            bool inpos = _ckdDriver.IsInPosition();
                            bool idle = _ckdDriver.IsRunWait();

                            if (home && inpos && idle)
                            {
                                stable++;
                                if (stable >= requiredStable)
                                {
                                    IsHomedLatched = true;
                                    try { var h = HomeSucceeded; if (h != null) h(this); } catch { }
                                    ok = true; break;
                                }
                            }
                            else
                            {
                                stable = 0;
                            }
                            await Task.Delay(10).ConfigureAwait(false);
                        }

                        if (!ok) { try { _ckdDriver.EmergencyStop(); } catch { } try { AlarmPost(AlarmKey.AxisHomeTimeout); } catch { } }
                    }
                    catch { }
                });
            }
            else
            {
                throw new InvalidOperationException("This axis does not support HomeAsync.");
            }
            return 0;
        }

        public int MoveAbs(double logicalTarget, bool isAuto = false, bool isFine = false)
        {
            double defaultFineVel = 5.0;
            double defaultCoarseVel = 20.0;
            double defaultAcc = 10.0;
            double defaultDec = 10.0;
            double defaultJerk = 50.0;

            double vel = 0.0;
            double acc = 0.0;
            double dec = 0.0;
            double jerk = 0.0;

            if (isAuto)
            {
                //vel = isFine ? (this.Config != null && this.Config.JogFineVelocity > 0 ? this.Config.JogFineVelocity : defaultFineVel)
                //               : (this.Config != null && this.Config.JogCoarseVelocity > 0 ? this.Config.JogCoarseVelocity : defaultCoarseVel);

                vel = this.Config != null && this.Config.MaxVelocity > 0 ? this.Config.MaxVelocity : defaultFineVel;
                acc = this.Config != null && this.Config.RunAcc > 0 ? this.Config.RunAcc : defaultAcc;
                dec = this.Config != null && this.Config.RunDec > 0 ? this.Config.RunDec : defaultDec;
                jerk = this.Config != null ? (this.Config.AccJerkPercent + this.Config.DecJerkPercent) / 2.0 : defaultJerk;
            }
            else
            {
                vel = isFine ? (this.Config != null && this.Config.JogFineVelocity > 0 ? this.Config.JogFineVelocity : defaultFineVel)
                               : (this.Config != null && this.Config.JogCoarseVelocity > 0 ? this.Config.JogCoarseVelocity : defaultCoarseVel);

                acc = this.Config != null && this.Config.JogAcc > 0 ? this.Config.JogAcc : defaultAcc;
                dec = this.Config != null && this.Config.JogDec > 0 ? this.Config.JogDec : defaultDec;
                jerk = this.Config != null ? (this.Config.AccJerkPercent + this.Config.DecJerkPercent) / 2.0 : defaultJerk;
            }
            return MoveAbs(logicalTarget,vel,acc,dec,jerk);
        }

        public int MoveAbs(double logicalTarget, double vel, double acc, double dec, double jerkPercent)
        {
            if (IsSim)
            {
                try
                {
                    GuardSoftLimit(logicalTarget);
                    double v = vel > 0 ? vel : (Config.MaxVelocity > 0 ? Config.MaxVelocity : 5);
                    StartSimMoveTo(logicalTarget, v);
                    return 0;
                }
                catch (Exception ex)
                {
                    Log.Write("MotionAxis.MoveAbs(Sim)", ex.Message);
                    return -1;
                }
            }

            if (_driver != null)
            {
                // 1) 소프트리밋 검사
                GuardSoftLimit(logicalTarget);

                // 2) 하드(물리) 리밋 상태 검사: 이동 방향에 따라 해당 리밋 센서가 이미 Active이면 구동 차단
                try
                {
                    var cur = GetPosition();
                    if (logicalTarget > cur)
                    {
                        // + 방향 이동 예정 → +Limit 센서 Active 여부 검사
                        if (_driver.ReadPositiveLimit(AxisNo))
                            throw new InvalidOperationException("[" + Name + "] +Limit Active 상태에서 +방향 이동 불가");
                    }
                    else if (logicalTarget < cur)
                    {
                        // - 방향 이동 예정 → -Limit 센서 Active 여부 검사
                        if (_driver.ReadNegativeLimit(AxisNo))
                            throw new InvalidOperationException("[" + Name + "] -Limit Active 상태에서 -방향 이동 불가");
                    }
                }
                catch (Exception ex)
                {
                    // 인터락 위반을 알람으로도 남기고 예외 전달 (필요시 정책에 따라 변경)
                    Log.Write("MotionAxis.MoveAbs", ex.Message);
                    return -1; // 호출측에서 실패 처리; 예외 throw를 원하면 대신 throw;
                }

                // 3) 펄스 변환 및 구동 수행
                var p = _correction.ToHardware(logicalTarget);
                var jerk = MapJerkPercentToDriver((int)jerkPercent, (int)jerkPercent);
                return _driver.MoveAbsPosition(AxisNo, p, vel, acc, dec, jerk);
            }
            else
            {
                throw new InvalidOperationException("This axis does not support Absolute Move.");
            }
        }

        public int MoveRel(double logicalTarget, double vel, double acc, double dec, double jerkPercent)
        {
            if (IsSim)
            {
                var target = GetPosition() + logicalTarget;
                return MoveAbs(target, vel, acc, dec, jerkPercent);
            }
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

        // --- 아래 유틸 메서드 추가 ---
        // 360도 래핑 공용 헬퍼 (Simulation 전용 사용)
        private static double Wrap360(double angle)
        {
            angle %= 360.0;
            if (angle < 0) angle += 360.0;

            // 360.0(또는 근사치) → 0 강제
            if (Math.Abs(angle - 360.0) < 1e-6) angle = 0.0;
            // -0 방지
            if (Math.Abs(angle) < 1e-12) angle = 0.0;

            return angle;
        }
        // Simulation 모드에서 360도 래핑
        private void NormalizeSimAngle()
        {
            if (!IsSim) return;

            double pos = GetPosition();   // 현재 시뮬 논리 위치
            double wrapped = Wrap360(pos);

            if (Math.Abs(pos - wrapped) > 1e-9)
            {
                SetPositionLogical(wrapped);
            }
            else
            {
                // pos == wrapped 이지만 360.0 그대로 들어온 경우(이론상 없음) 방어
                if (Math.Abs(pos - 360.0) < 1e-9)
                    SetPositionLogical(0.0);
            }
        }

        public int MoveNextIndex()
        {
            if (IsSim)
            {
                // Div8 기준 45도 (필요 시 Setup/Config 값으로 일반화 가능)
                const double stepDeg = -45.0;

                // 비동기 이동 종료 시점에 Normalize 호출이 안 되어 360이 남는 문제 → 목표를 선행 래핑
                double cur = GetPosition();
                double target = Wrap360(cur + stepDeg);

                // 상대이동 대신 절대이동으로 직접 목표 지정 (이유: MoveRel 후 즉시 NormalizeSimAngle 호출 시 아직 도달 전이라 미적용 문제 회피)
                int rc = MoveAbs(target,
                                 Config.MaxVelocity > 0 ? Config.MaxVelocity : 5,
                                 0, 0, 0);

                if (rc == 0)
                {
                    // 혹시 이동 중간에 다른 연산 후 최종 360 남을 가능성 최소화
                    NormalizeSimAngle();
                }
                return rc;
            }
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
            if (IsSim)
            {
                const double stepDeg = +45.0;

                double cur = GetPosition();
                double target = Wrap360(cur + stepDeg);

                int rc = MoveAbs(target,
                                 Config.MaxVelocity > 0 ? Config.MaxVelocity : 5,
                                 0, 0, 0);

                if (rc == 0)
                {
                    NormalizeSimAngle();
                }
                return rc;
            }
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
            if (IsSim)
            {
                if (timeoutMs < 0) 
                    timeoutMs = Setup.MoveTimeoutMs;

                var sw = Stopwatch.StartNew();
                while (sw.ElapsedMilliseconds < timeoutMs)
                {
                    bool moving = false;
                    lock (_simLock) 
                        moving = _simIsMoving;

                    if (!moving) 
                        return 0;

                    Thread.Sleep(5);
                }
                return -1;
            }
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
            if (IsSim)
            {
                if (Math.Abs(signedVel) < double.Epsilon) return;
                try
                {
                    // 소프트리밋 방향 체크
                    if (Setup.SoftLimitEnable)
                    {
                        var cur = GetPosition();
                        if (signedVel > 0 && cur >= Setup.SoftLimitMax) { Log.Write("MotionAxis.JogStart", "[" + Name + "] SoftLimitMax 도달 - +방향 조그 차단"); return; }
                        if (signedVel < 0 && cur <= Setup.SoftLimitMin) { Log.Write("MotionAxis.JogStart", "[" + Name + "] SoftLimitMin 도달 - -방향 조그 차단"); return; }
                    }
                    StartSimJog(signedVel);
                }
                catch { }
                return;
            }
            if (_driver != null)
            {
                if (Math.Abs(signedVel) < double.Epsilon) return; // 0 속도 무시

                // 1) 소프트리밋: 현재 위치 기준으로 진행 방향 제한
                try
                {
                    if (Setup.SoftLimitEnable)
                    {
                        var cur = GetPosition();
                        if (signedVel > 0)
                        {
                            if (cur >= Setup.SoftLimitMax)
                            {
                                Log.Write("MotionAxis.JogStart", "[" + Name + "] SoftLimitMax 도달 - +방향 조그 차단");
                                return;
                            }
                        }
                        else if (signedVel < 0)
                        {
                            if (cur <= Setup.SoftLimitMin)
                            {
                                Log.Write("MotionAxis.JogStart", "[" + Name + "] SoftLimitMin 도달 - -방향 조그 차단");
                                return;
                            }
                        }
                    }

                    // 2) 하드(물리) 리밋 센서 검사: 해당 방향 리밋 Active면 차단
                    try
                    {
                        if (signedVel > 0 && _driver.ReadPositiveLimit(AxisNo))
                        {
                            Log.Write("MotionAxis.JogStart", "[" + Name + "] +Limit Active - +방향 조그 차단");
                            return;
                        }
                        if (signedVel < 0 && _driver.ReadNegativeLimit(AxisNo))
                        {
                            Log.Write("MotionAxis.JogStart", "[" + Name + "] -Limit Active - -방향 조그 차단");
                            return;
                        }
                    }
                    catch (Exception exLim)
                    {
                        // 센서 읽기 예외 시 안전 차단
                        Log.Write("MotionAxis.JogStart", "Limit 센서 읽기 실패 - 차단: " + exLim.Message);
                        return;
                    }
                }
                catch (Exception exSoft)
                {
                    Log.Write("MotionAxis.JogStart", "SoftLimit 체크 실패 - 차단: " + exSoft.Message);
                    return;
                }

                // 3) Servo On 보장
                try { this.Servo(true); } catch (Exception ex) { Log.Write(ex); }

                double dAcc = Config.JogAcc;
                double dDec = Config.JogDec;

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
            if (IsSim)
            {
                StopSimMotion(false);
                return;
            }
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
            if (IsSim)
            {
                StopSimMotion(true);
                return;
            }
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
            if (IsSim)
            {
                StopSimMotion(false);
                return 0;
            }
            if (_driver != null)
            {
                return _driver.Stop(AxisNo);
            }
            else if(_ckdDriver != null)
            {
                // CKD에서 정지 Command가 있는지 확인 필요
                try { _ckdDriver.EmergencyStop(); } catch { }
                return 0;
            }
            else
            {
                throw new InvalidOperationException("This axis does not support Stop.");
            }
        }
        public int EmgStop() 
        {   
            if (IsSim)
            {
                StopSimMotion(true);
                return 0;
            }
            if (_driver != null)
            {
                return _driver.EmgStop(AxisNo);
            }
            else if(_ckdDriver != null)
            {
                try { _ckdDriver.EmergencyStop(); } catch { }
                return 0;
            }
            else
            {
                throw new InvalidOperationException("This axis does not support EStop.");
            }
        }
        public int Servo(bool on) 
        {   
            if (IsSim)
            {
                lock (_simLock) { _simServoOn = on; if (!on) { StopSimMotion(true); } }
                return 0;
            }
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
            if (IsSim)
            {
                lock (_simLock) { _simAlarm = false; }
                return 0;
            }
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
            if (IsSim)
            {
                lock (_simLock)
                {
                    _simPosition = logicalValue;
                    _simCommandPosition = logicalValue;
                    _simTarget = logicalValue;
                    _simCurrentVelocity = 0;
                }
                return 0;
            }
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
            if (IsSim) return 0;
            if (_driver != null)
            {
                try
                {
                    // 프로파일 모드 동기화
                    _driver.ProfileMode = Config.ProfileMode;
                    
                    // 하드웨어 설정 적용
                    //return _driver.ConfigureFromSetupAndConfig(Setup.AxisNo, Setup, Config);
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

        /// <summary>
        /// 홈 시퀀스 사전 인터락 체크. 필요한 경우 일부 자동 복구 시도.
        /// true면 통과, false면 reason에 사유 기입.
        /// </summary>
        public bool CheckHomeInterlocks(out string reason, bool tryRecover = true, int settleMs = 200)
        {
            reason = null;

            try
            {
                if (IsSim)
                {
                    // 시뮬레이션에서는 서보/알람/이동 상태만 확인
                    lock (_simLock)
                    {
                        if (!_simServoOn)
                        {
                            if (tryRecover) { _simServoOn = true; Thread.Sleep(settleMs); }
                            if (!_simServoOn) { reason = "Servo OFF"; return false; }
                        }
                        if (_simAlarm)
                        {
                            if (tryRecover) { _simAlarm = false; Thread.Sleep(settleMs); }
                            if (_simAlarm) { reason = "Alarm ON"; return false; }
                        }
                        if (_simIsMoving)
                        {
                            if (tryRecover) { StopSimMotion(false); }
                            if (_simIsMoving) { reason = "Axis moving"; return false; }
                        }
                    }
                    return true;
                }

                if (_driver != null)
                {
                    // ===== Ajin 계열 체크 =====
                    // 1) Servo On
                    if (!_driver.ReadServoOn(AxisNo))
                    {
                        if (tryRecover) { try { _driver.Servo(AxisNo, true); Thread.Sleep(settleMs); } catch { } }
                        if (!_driver.ReadServoOn(AxisNo)) { reason = "Servo OFF"; return false; }
                    }

                    // 2) Alarm
                    if (_driver.ReadAlarm(AxisNo))
                    {
                        if (tryRecover) { try { _driver.ClearAlarm(AxisNo); Thread.Sleep(settleMs); } catch { } }
                        if (_driver.ReadAlarm(AxisNo)) { reason = "Alarm ON"; return false; }
                    }

                    // 3) InMotion → Stop and wait
                    if (!_driver.ReadDone(AxisNo))
                    {
                        if (tryRecover) { try { _driver.Stop(AxisNo); } catch { } }
                        var sw = Stopwatch.StartNew();
                        while (sw.ElapsedMilliseconds < 2000)
                        {
                            if (_driver.ReadDone(AxisNo)) break;
                            Thread.Sleep(10);
                        }
                        if (!_driver.ReadDone(AxisNo)) { reason = "Axis moving"; return false; }
                    }

                    // 4) Limit (옵션)
                    //if (_driver.ReadPositiveLimit(AxisNo)) { reason = "+Limit ON"; return false; }
                    //if (_driver.ReadNegativeLimit(AxisNo)) { reason = "-Limit ON"; return false; }

                    return true;
                }
                else if (_ckdDriver != null)
                {
                    // ===== CKD 계열 체크 =====
                    // 1) Servo On
                    if (!_ckdDriver.IsServoOn())
                    {
                        if (tryRecover) { try { _ckdDriver.Servo(true); Thread.Sleep(settleMs); } catch { } }
                        if (!_ckdDriver.IsServoOn()) { reason = "Servo OFF"; return false; }
                    }

                    // 2) Alarm
                    if (_ckdDriver.IsAlarm())
                    {
                        if (tryRecover) { try { _ckdDriver.AlarmReset(); Thread.Sleep(settleMs); } catch { } }
                        if (_ckdDriver.IsAlarm()) { reason = "Alarm ON"; return false; }
                    }

                    // 3) InMotion → Stop(wait) and confirm idle
                    // CKD는 별도 ReadDone 대신 RunWait 상태가 안전 대기 상태로 간주
                    if (!_ckdDriver.IsRunWait())
                    {
                        if (tryRecover)
                        {
                            try { _ckdDriver.EmergencyStop(); } catch { }
                        }
                        var sw = Stopwatch.StartNew();
                        while (sw.ElapsedMilliseconds < 2000)
                        {
                            if (_ckdDriver.IsRunWait()) break;
                            Thread.Sleep(10);
                        }
                        if (!_ckdDriver.IsRunWait()) { reason = "Axis moving"; return false; }
                    }

                    return true;
                }
                else
                {
                    // 드라이버가 연결되지 않은 경우
                    reason = "No motion driver";
                    return false;
                }
            }
            catch (Exception ex)
            {
                reason = ex.Message;
                return false;
            }
        }

        /// <summary>홈 래치 클리어</summary>
        public void ClearHomeLatch() { IsHomedLatched = false; }


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
                if (!Setup.IsSimulation)
                {
                    // 시뮬레이션에서 실제로 전환 시 안전하게 중지
                    StopSimMotion(true);
                }
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
            t.InPosition = s.InPosition;
            t.SoftwareLimitEnable = s.SoftLimitEnable;
            t.SoftwareLength = s.SoftwareLength;
            t.HomeSignalLevel = s.HomeSignalLevel;
            t.HomeMode = s.HomeMode;
            t.HomeDirection = s.HomeDirection;
            t.HomeSignal = s.HomeSignal;
            t.HomeZPhase = s.HomeZPhase;
            t.HomeClearTime = s.HomeClearTime;
            t.HomeOffset = s.HomeOffset;
            t.AlarmResetLevel = s.AlarmResetLevel;
            t.AlarmLevel = s.AlarmLevel;
            t.PositiveLimitLevel = s.PositiveLimitLevel;
            t.NegativeLimitLevel = s.NegativeLimitLevel;
            t.SoftLimitEnable = s.SoftLimitEnable;
            t.SoftLimitMin = s.SoftLimitMin;
            t.SoftLimitMax = s.SoftLimitMax;
            t.HomeTimeoutMs = s.HomeTimeoutMs;
            t.MoveTimeoutMs = s.MoveTimeoutMs;
            t.SensorDetectionTimeoutMs = s.SensorDetectionTimeoutMs;
            t.IsSimulation = s.IsSimulation;
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
            dst.InPosition = src.InPosition;
            dst.SoftwareLimitEnable = src.SoftwareLimitEnable;
            dst.SoftwareLength = src.SoftwareLength;
            dst.HomeSignalLevel = src.HomeSignalLevel;
            dst.HomeMode = src.HomeMode;
            dst.HomeDirection = src.HomeDirection;
            dst.HomeSignal = src.HomeSignal;
            dst.HomeZPhase = src.HomeZPhase;
            dst.HomeClearTime = src.HomeClearTime;
            dst.HomeOffset = src.HomeOffset;
            dst.AlarmResetLevel = src.AlarmResetLevel;
            dst.AlarmLevel = src.AlarmLevel;
            dst.PositiveLimitLevel = src.PositiveLimitLevel;
            dst.NegativeLimitLevel = src.NegativeLimitLevel;
            dst.SoftLimitEnable = src.SoftLimitEnable;
            dst.SoftLimitMin = src.SoftLimitMin;
            dst.SoftLimitMax = src.SoftLimitMax;
            dst.HomeTimeoutMs = src.HomeTimeoutMs;
            dst.MoveTimeoutMs = src.MoveTimeoutMs;
            dst.SensorDetectionTimeoutMs = src.SensorDetectionTimeoutMs;
            dst.IsSimulation = src.IsSimulation;
        }

        //
        public MotionAxisStatus GetStatusSnapshot()
        {
            if (IsSim)
            {
                lock (_simLock)
                {
                    Status.PV.CommandPosition = _simCommandPosition;
                    Status.PV.ActualPosition = _simPosition;
                    Status.PV.ErrorPosition = _simCommandPosition - _simPosition;
                    Status.PV.CommandVelocity = _simCurrentVelocity;
                    Status.PV.ActualVelocity = _simCurrentVelocity;

                    Status.IO.ServoOn = _simServoOn;
                    Status.IO.Alarm = _simAlarm;
                    Status.IO.NegativeLimitSensor = false;
                    Status.IO.PositiveLimitSensor = false;
                    Status.IO.HomeSensor = _simHomeSensor;

                    bool inpos = Math.Abs(_simPosition - _simTarget) <= Config.InposTolerance;
                    bool done = !_simIsMoving && inpos;

                    Status.State.Done = done;
                    Status.State.Inposition = inpos;
                    Status.State.InpositionDone = done;
                    Status.State.InpositionTimeout = false;
                    Status.State.HomeEnd = IsHomedLatched;
                    Status.State.HomeTimeout = false;

                    Status.TimestampUtc = DateTime.UtcNow;
                }
                return Status;
            }

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

                Status.TimestampUtc = DateTime.UtcNow;
                return Status;
            }
            return Status;
        }

        // ===== Simulation helpers =====
        private void StartSimMoveTo(double target, double velocity)
        {
            lock (_simLock)
            {
                // 소프트리밋 클램프(초과 시 예외는 상단에서 처리)
                if (Setup.SoftLimitEnable)
                {
                    if (target > Setup.SoftLimitMax) target = Setup.SoftLimitMax;
                    if (target < Setup.SoftLimitMin) target = Setup.SoftLimitMin;
                }
                _simTarget = target;
                _simCommandPosition = target;
                _simIsMoving = true;
                _simCurrentVelocity = 0; // 가/감속 미적용: 아래 루프에서 부호만 사용
                CancelSimCts_NoLock();
                _simMoveCts = new CancellationTokenSource();
                var token = _simMoveCts.Token;
                double startPos = _simPosition;
                double dist = target - startPos;
                double dir = Math.Sign(dist);
                double v = Math.Abs(velocity);
                if (v <= 0) v = 5;
                _simServoOn = true; // 시뮬에서는 자동 서보온 가정

                _simMoveTask = Task.Run(async () =>
                {
                    var sw = Stopwatch.StartNew();
                    long last = sw.ElapsedMilliseconds;
                    try
                    {
                        while (!token.IsCancellationRequested)
                        {
                            long now = sw.ElapsedMilliseconds;
                            var dtMs = now - last; if (dtMs < 1) { await Task.Delay(1).ConfigureAwait(false); continue; }
                            last = now;
                            double dt = dtMs / 1000.0; // s

                            double step = dir * v * dt;
                            bool reached = false;
                            lock (_simLock)
                            {
                                double remain = _simTarget - _simPosition;
                                if (Math.Sign(remain) != Math.Sign(dir)) { reached = true; }
                                else
                                {
                                    if (Math.Abs(step) >= Math.Abs(remain))
                                    {
                                        _simPosition = _simTarget;
                                        _simCurrentVelocity = 0;
                                        reached = true;
                                    }
                                    else
                                    {
                                        // 진행하면서 소프트리밋 재확인
                                        double next = _simPosition + step;
                                        if (Setup.SoftLimitEnable)
                                        {
                                            if ((dir > 0 && next > Setup.SoftLimitMax) || (dir < 0 && next < Setup.SoftLimitMin))
                                            {
                                                _simPosition = dir > 0 ? Setup.SoftLimitMax : Setup.SoftLimitMin;
                                                _simCurrentVelocity = 0;
                                                reached = true;
                                            }
                                            else
                                            {
                                                _simPosition = next;
                                                _simCurrentVelocity = dir * v;
                                            }
                                        }
                                        else
                                        {
                                            _simPosition = next;
                                            _simCurrentVelocity = dir * v;
                                        }
                                    }
                                }
                            }
                            if (reached) break;
                            await Task.Delay(10).ConfigureAwait(false);
                        }
                       
                    }
                    catch { }
                    finally
                    {
                        lock (_simLock)
                        {
                            _simIsMoving = false;
                            _simCurrentVelocity = 0;
                        }
                    }
                }, token);
            }
        }

        private void StartSimJog(double signedVel)
        {
            lock (_simLock)
            {
                CancelSimCts_NoLock();
                _simMoveCts = new CancellationTokenSource();
                var token = _simMoveCts.Token;
                double v = signedVel;
                _simServoOn = true;
                _simIsMoving = true;
                _simCurrentVelocity = v;
                _simMoveTask = Task.Run(async () =>
                {
                    var sw = Stopwatch.StartNew();
                    long last = sw.ElapsedMilliseconds;
                    try
                    {
                        while (!token.IsCancellationRequested)
                        {
                            long now = sw.ElapsedMilliseconds;
                            var dtMs = now - last; if (dtMs < 1) { await Task.Delay(1).ConfigureAwait(false); continue; }
                            last = now;
                            double dt = dtMs / 1000.0;

                            bool stop = false;
                            lock (_simLock)
                            {
                                double next = _simPosition + _simCurrentVelocity * dt;
                                if (Setup.SoftLimitEnable)
                                {
                                    if (_simCurrentVelocity > 0 && next >= Setup.SoftLimitMax)
                                    {
                                        _simPosition = Setup.SoftLimitMax;
                                        stop = true;
                                    }
                                    else if (_simCurrentVelocity < 0 && next <= Setup.SoftLimitMin)
                                    {
                                        _simPosition = Setup.SoftLimitMin;
                                        stop = true;
                                    }
                                    else
                                    {
                                        _simPosition = next;
                                    }
                                }
                                else
                                {
                                    _simPosition = next;
                                }
                            }
                            if (stop) break;
                            await Task.Delay(10).ConfigureAwait(false);
                        }
                    }
                    catch { }
                    finally
                    {
                        lock (_simLock) { _simIsMoving = false; _simCurrentVelocity = 0; }
                    }
                }, token);
            }
        }

        private void StopSimMotion(bool emergency)
        {
            lock (_simLock)
            {
                CancelSimCts_NoLock();
                _simIsMoving = false;
                _simCurrentVelocity = 0;
            }
        } 

        private void CancelSimCts_NoLock()
        {
            try 
            { 
                if (_simMoveCts != null) 
                { 
                    _simMoveCts.Cancel();
                    _simMoveCts.Dispose(); 
                    _simMoveCts = null; 
                } 
            } 
            catch (Exception ex)
            {
                Log.Write(ex);
            }
        }

        public bool IsMoveDone()
        {
            if(IsSim)
            {
                lock(_simLock)
                {
                    return !_simIsMoving;
                }
            }
            else if(_driver != null)
            {
                return _driver.ReadDone(AxisNo);
            }
            else if(_ckdDriver != null)
            {
                // CKD는 별도 ReadDone 대신 RunWait 상태가 안전 대기 상태로 간주
                return _ckdDriver.IsRunWait();
            }
            return false;
        }
    }
}
