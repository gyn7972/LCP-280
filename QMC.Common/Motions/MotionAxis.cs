using NPOI.SS.Formula.Functions;
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
        //public enum AlarmKey
        //{
        //    //1000~1999 범위로 Axis 관련 알람 정의 (필요시 확장)
        //    //1000 규칙: 첫번째 자리: 축
        //    //0110 규칙: 두번째, 세번째 자리: 축번호
        //    //0001 규칙: 네번째 알람
        //    AxisHomeTimeout = 1001,
        //    AxisServoOff = 1002,
        //    AxisDriverAlarm = 1003,
        //    AxisPositiveLimit = 1004,
        //    AxisNegativeLimit = 1005,
        //    AxisSoftLimit = 1006,
        //}
        public enum AlarmKey
        {
            // ===== Alarm Type (BB) =====
            // 01: HomeTimeout
            // 02: ServoOff
            // 03: DriverAlarm
            // 04: PositiveLimit
            // 05: NegativeLimit
            // 06: SoftLimit

            // 00 Eject Pin Z Axis
            EjectPinZ_HomeTimeout = 0001,
            EjectPinZ_ServoOff = 0002,
            EjectPinZ_DriverAlarm = 0003,
            EjectPinZ_PositiveLimit = 0004,
            EjectPinZ_NegativeLimit = 0005,
            EjectPinZ_SoftLimit = 0006,

            // 01 Left Tool T Axis
            LeftToolT_HomeTimeout = 0101,
            LeftToolT_ServoOff = 0102,
            LeftToolT_DriverAlarm = 0103,
            LeftToolT_PositiveLimit = 0104,
            LeftToolT_NegativeLimit = 0105,
            LeftToolT_SoftLimit = 0106,

            // 02 Right Tool T Axis
            RightToolT_HomeTimeout = 0201,
            RightToolT_ServoOff = 0202,
            RightToolT_DriverAlarm = 0203,
            RightToolT_PositiveLimit = 0204,
            RightToolT_NegativeLimit = 0205,
            RightToolT_SoftLimit = 0206,

            // 03 Wafer Stage X Axis
            WaferStageX_HomeTimeout = 0301,
            WaferStageX_ServoOff = 0302,
            WaferStageX_DriverAlarm = 0303,
            WaferStageX_PositiveLimit = 0304,
            WaferStageX_NegativeLimit = 0305,
            WaferStageX_SoftLimit = 0306,

            // 04 Wafer Stage Y Axis
            WaferStageY_HomeTimeout = 0401,
            WaferStageY_ServoOff = 0402,
            WaferStageY_DriverAlarm = 0403,
            WaferStageY_PositiveLimit = 0404,
            WaferStageY_NegativeLimit = 0405,
            WaferStageY_SoftLimit = 0406,

            // 05 Wafer Stage T Axis
            WaferStageT_HomeTimeout = 0501,
            WaferStageT_ServoOff = 0502,
            WaferStageT_DriverAlarm = 0503,
            WaferStageT_PositiveLimit = 0504,
            WaferStageT_NegativeLimit = 0505,
            WaferStageT_SoftLimit = 0506,

            // 06 Left Pick Z Axis
            LeftPickZ_HomeTimeout = 0601,
            LeftPickZ_ServoOff = 0602,
            LeftPickZ_DriverAlarm = 0603,
            LeftPickZ_PositiveLimit = 0604,
            LeftPickZ_NegativeLimit = 0605,
            LeftPickZ_SoftLimit = 0606,

            // 07 Left Place Z Axis
            LeftPlaceZ_HomeTimeout = 0701,
            LeftPlaceZ_ServoOff = 0702,
            LeftPlaceZ_DriverAlarm = 0703,
            LeftPlaceZ_PositiveLimit = 0704,
            LeftPlaceZ_NegativeLimit = 0705,
            LeftPlaceZ_SoftLimit = 0706,

            // 08 Index Z Axis
            IndexZ_HomeTimeout = 0801,
            IndexZ_ServoOff = 0802,
            IndexZ_DriverAlarm = 0803,
            IndexZ_PositiveLimit = 0804,
            IndexZ_NegativeLimit = 0805,
            IndexZ_SoftLimit = 0806,

            // 09 Align T Axis
            AlignT_HomeTimeout = 0901,
            AlignT_ServoOff = 0902,
            AlignT_DriverAlarm = 0903,
            AlignT_PositiveLimit = 0904,
            AlignT_NegativeLimit = 0905,
            AlignT_SoftLimit = 0906,

            // 10 Sphere Z Axis
            SphereZ_HomeTimeout = 1001,
            SphereZ_ServoOff = 1002,
            SphereZ_DriverAlarm = 1003,
            SphereZ_PositiveLimit = 1004,
            SphereZ_NegativeLimit = 1005,
            SphereZ_SoftLimit = 1006,

            // 11 Probe Z Axis
            ProbeZ_HomeTimeout = 1101,
            ProbeZ_ServoOff = 1102,
            ProbeZ_DriverAlarm = 1103,
            ProbeZ_PositiveLimit = 1104,
            ProbeZ_NegativeLimit = 1105,
            ProbeZ_SoftLimit = 1106,

            // 12 Probe Card X Axis
            ProbeCardX_HomeTimeout = 1201,
            ProbeCardX_ServoOff = 1202,
            ProbeCardX_DriverAlarm = 1203,
            ProbeCardX_PositiveLimit = 1204,
            ProbeCardX_NegativeLimit = 1205,
            ProbeCardX_SoftLimit = 1206,

            // 13 Probe Card Y Axis
            ProbeCardY_HomeTimeout = 1301,
            ProbeCardY_ServoOff = 1302,
            ProbeCardY_DriverAlarm = 1303,
            ProbeCardY_PositiveLimit = 1304,
            ProbeCardY_NegativeLimit = 1305,
            ProbeCardY_SoftLimit = 1306,

            // 14 Probe Card Z Axis
            ProbeCardZ_HomeTimeout = 1401,
            ProbeCardZ_ServoOff = 1402,
            ProbeCardZ_DriverAlarm = 1403,
            ProbeCardZ_PositiveLimit = 1404,
            ProbeCardZ_NegativeLimit = 1405,
            ProbeCardZ_SoftLimit = 1406,

            // 15 Right Pick Z Axis
            RightPickZ_HomeTimeout = 1501,
            RightPickZ_ServoOff = 1502,
            RightPickZ_DriverAlarm = 1503,
            RightPickZ_PositiveLimit = 1504,
            RightPickZ_NegativeLimit = 1505,
            RightPickZ_SoftLimit = 1506,

            // 16 Right Place Z Axis
            RightPlaceZ_HomeTimeout = 1601,
            RightPlaceZ_ServoOff = 1602,
            RightPlaceZ_DriverAlarm = 1603,
            RightPlaceZ_PositiveLimit = 1604,
            RightPlaceZ_NegativeLimit = 1605,
            RightPlaceZ_SoftLimit = 1606,

            // 17 Bin Stage X Axis
            BinStageX_HomeTimeout = 1701,
            BinStageX_ServoOff = 1702,
            BinStageX_DriverAlarm = 1703,
            BinStageX_PositiveLimit = 1704,
            BinStageX_NegativeLimit = 1705,
            BinStageX_SoftLimit = 1706,

            // 18 Bin Stage Y Axis
            BinStageY_HomeTimeout = 1801,
            BinStageY_ServoOff = 1802,
            BinStageY_DriverAlarm = 1803,
            BinStageY_PositiveLimit = 1804,
            BinStageY_NegativeLimit = 1805,
            BinStageY_SoftLimit = 1806,

            // 19 Bin Stage T Axis
            BinStageT_HomeTimeout = 1901,
            BinStageT_ServoOff = 1902,
            BinStageT_DriverAlarm = 1903,
            BinStageT_PositiveLimit = 1904,
            BinStageT_NegativeLimit = 1905,
            BinStageT_SoftLimit = 1906,

            // 20 Wafer Lifter Z Axis
            WaferLifterZ_HomeTimeout = 2001,
            WaferLifterZ_ServoOff = 2002,
            WaferLifterZ_DriverAlarm = 2003,
            WaferLifterZ_PositiveLimit = 2004,
            WaferLifterZ_NegativeLimit = 2005,
            WaferLifterZ_SoftLimit = 2006,

            // 21 Wafer Feeder Y Axis
            WaferFeederY_HomeTimeout = 2101,
            WaferFeederY_ServoOff = 2102,
            WaferFeederY_DriverAlarm = 2103,
            WaferFeederY_PositiveLimit = 2104,
            WaferFeederY_NegativeLimit = 2105,
            WaferFeederY_SoftLimit = 2106,

            // 22 Ejector Z Axis
            EjectorZ_HomeTimeout = 2201,
            EjectorZ_ServoOff = 2202,
            EjectorZ_DriverAlarm = 2203,
            EjectorZ_PositiveLimit = 2204,
            EjectorZ_NegativeLimit = 2205,
            EjectorZ_SoftLimit = 2206,

            // 23 Bin Feeder Y Axis
            BinFeederY_HomeTimeout = 2301,
            BinFeederY_ServoOff = 2302,
            BinFeederY_DriverAlarm = 2303,
            BinFeederY_PositiveLimit = 2304,
            BinFeederY_NegativeLimit = 2305,
            BinFeederY_SoftLimit = 2306,

            // 24 Bin Lifter Z Axis
            BinLifterZ_HomeTimeout = 2401,
            BinLifterZ_ServoOff = 2402,
            BinLifterZ_DriverAlarm = 2403,
            BinLifterZ_PositiveLimit = 2404,
            BinLifterZ_NegativeLimit = 2405,
            BinLifterZ_SoftLimit = 2406,

            // 25 Index Place Z Axis
            IndexPlaceZ_HomeTimeout = 2501,
            IndexPlaceZ_ServoOff = 2502,
            IndexPlaceZ_DriverAlarm = 2503,
            IndexPlaceZ_PositiveLimit = 2504,
            IndexPlaceZ_NegativeLimit = 2505,
            IndexPlaceZ_SoftLimit = 2506,

            // 26 Gripper X Axis
            GripperX_HomeTimeout = 2601,
            GripperX_ServoOff = 2602,
            GripperX_DriverAlarm = 2603,
            GripperX_PositiveLimit = 2604,
            GripperX_NegativeLimit = 2605,
            GripperX_SoftLimit = 2606,

            // 27 Index T Axis
            IndexT_HomeTimeout = 2701,
            IndexT_ServoOff = 2702,
            IndexT_DriverAlarm = 2703,
            IndexT_PositiveLimit = 2704,
            IndexT_NegativeLimit = 2705,
            IndexT_SoftLimit = 2706,
        }

        // ===== 공용 알람 코드 상수 (enum을 수정하지 않고 사용) =====
        private const int BASE_ALARM_HOME_TIMEOUT = 1;
        private const int BASE_ALARM_SERVO_OFF = 2;
        private const int BASE_ALARM_DRIVER_ALARM = 3;
        private const int BASE_ALARM_POSITIVE_LIMIT = 4;
        private const int BASE_ALARM_NEGATIVE_LIMIT = 5;
        private const int BASE_ALARM_SOFT_LIMIT = 6;

        // ===== 축 번호(AxisNo)로 실제 AlarmKey를 동적 계산하는 오버로드 =====
        public int AlarmPost(int baseAlarmType)
        {
            // 예: AxisNo = 27, baseAlarmType = 1 이면 2701 (IndexT_HomeTimeout)
            AlarmKey specificKey = (AlarmKey)(this.AxisNo * 100 + baseAlarmType);
            return AlarmPost(specificKey);
        }
        protected override void InitAlarm()
        {
            string source = string.Empty;
            base.InitAlarm();

            source = string.Format(Name);

            // 1. 공용 파일 로더에서 알람 목록 가져오기
            var loadedAlarms = GlobalAlarmTable.Instance.GetAlarmsForSource(source);
            if (loadedAlarms == null || loadedAlarms.Count == 0)
            {
                Log.Write("AlarmInit", $"Cannot find alarms for source '{source}' in the alarm file. Only default alarms will be registered.");




            }
            else
            {
                // 2. m_dicAlarms에 일괄 등록
                foreach (var alarmInfo in loadedAlarms)
                {
                    if (!m_dicAlarms.ContainsKey(alarmInfo.Code))
                    {
                        m_dicAlarms.Add(alarmInfo.Code, alarmInfo);
                    }
                }
            }
        }

        // ===== 추가: 감시 알람 중복 방지 래치 =====
        private readonly object _alarmLatchLock = new object();
        private readonly HashSet<AlarmKey> _postedMonitorAlarms = new HashSet<AlarmKey>();
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

        private readonly object _lock = new object();

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
                //return _simPosition;
                lock (_simLock) return _simPosition;
            }
            if (_driver != null)
            {
                lock (_lock)
                {
                    var pulse = _driver.ReadActualPulse(AxisNo);
                    return _correction.ToLogical(pulse);
                }
            }
            else if (_ckdDriver != null)
            {
                lock (_lock)
                {
                    var degree = _ckdDriver.GetPositionDegree() / 1000.0;
                    return degree;
                } 
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
                //var pos = Status.PV.ActualPosition;
                bool bRet = Math.Abs(pos - logicalTarget) <= Config.InposTolerance;
                bRet = this.IsMoveDone();
                return bRet;
            }
            else if (_ckdDriver != null)
            {
                bool bDone = Status.State.Done;
                bool bInpositionDone = Status.State.InpositionDone;
                bool bInposition = Status.State.Inposition;
                if(bDone && bInpositionDone && bInposition)
                {
                    var pos = GetPosition();
                    return Math.Abs(pos - logicalTarget) <= Config.InposTolerance;
                }
                else
                {
                    return false;
                }
                //return _ckdDriver.IsInPosition();
            }
            else
            {
                Log.Write("Error", "MotionAxis.InPosition", "No valid driver assigned.");
                return false;
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
                        if (!_simServoOn) 
                            _simServoOn = true;

                        _simAlarm = false;
                        _simIsMoving = true;
                        _simCurrentVelocity = 0;
                    }
                    Thread.Sleep(5);

                    lock (_simLock)
                    {
                        _simPosition = 0 + Setup.HomeOffset;
                        _simCommandPosition = _simPosition;
                        _simHomeSensor = true;
                        _simIsMoving = false;
                        _simCurrentVelocity = 0;
                    }
                    IsHomedLatched = true;

                    try 
                    { 
                        var h = HomeSucceeded; 
                        if (h != null) 
                            h(this); 
                    }
                    catch (Exception ex)
                    {
                        Log.Write(ex);
                    }

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
                        try 
                        { 
                            var h = HomeSucceeded; 
                            if (h != null) 
                                h(this); 
                        } 
                        catch (Exception ex)
                        {
                            Log.Write(ex);
                        }
                        return 0;
                    }
                    Thread.Sleep(5);
                }

                try { _driver.Stop(AxisNo); } catch { }
                try { AlarmPost(BASE_ALARM_HOME_TIMEOUT); } catch { }
                //try { AlarmPost(AlarmKey.AxisHomeTimeout); } catch { }
            }
            else if (_ckdDriver != null) //Index
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
                    Thread.Sleep(5);
                }

                try { _ckdDriver.EmergencyStop(); } catch { }
                try { AlarmPost(BASE_ALARM_HOME_TIMEOUT); } catch { }
                //try { AlarmPost(AlarmKey.AxisHomeTimeout); } catch { }
            }
            else
            {
                throw new InvalidOperationException("This axis does not support HomeSync.");
            }
            return -1;
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
            InterlockEventArgs args = new InterlockEventArgs();
            args.dTargetPosition = logicalTarget;

            if (OnIsInterlockOK(args) == false)
            {
                return -1;
            }
            if (IsSim)
            {
                try
                {
                    GuardSoftLimit(logicalTarget);
                    double v = vel > 0 ? vel : (Config.MaxVelocity > 0 ? Config.MaxVelocity : 5);
                    double a = acc > 0 ? acc : (Config.RunAcc > 0 ? Config.RunAcc : v * 10);
                    double d = dec > 0 ? dec : (Config.RunDec > 0 ? Config.RunDec : v * 10);

                    StartSimMoveTo(logicalTarget, v, a, d);
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

        // 기존 코드
        //public int MoveAbs(double logicalTarget, double vel, double acc, double dec, double jerkPercent)
        //{
        //    InterlockEventArgs args = new InterlockEventArgs();
        //    args.dTargetPosition = logicalTarget;

        //    if(OnIsInterlockOK(args) == false)
        //    {
        //        return -1;
        //    }
        //    if (IsSim)
        //    {
        //        try
        //        {
        //            GuardSoftLimit(logicalTarget);
        //            double v = vel > 0 ? vel : (Config.MaxVelocity > 0 ? Config.MaxVelocity : 5);
        //            StartSimMoveTo(logicalTarget, v);
        //            return 0;
        //        }
        //        catch (Exception ex)
        //        {
        //            Log.Write("MotionAxis.MoveAbs(Sim)", ex.Message);
        //            return -1;
        //        }
        //    }

        //    if (_driver != null)
        //    {
        //        // 1) 소프트리밋 검사
        //        GuardSoftLimit(logicalTarget);

        //        // 2) 하드(물리) 리밋 상태 검사: 이동 방향에 따라 해당 리밋 센서가 이미 Active이면 구동 차단
        //        try
        //        {
        //            var cur = GetPosition();
        //            if (logicalTarget > cur)
        //            {
        //                // + 방향 이동 예정 → +Limit 센서 Active 여부 검사
        //                if (_driver.ReadPositiveLimit(AxisNo))
        //                    throw new InvalidOperationException("[" + Name + "] +Limit Active 상태에서 +방향 이동 불가");
        //            }
        //            else if (logicalTarget < cur)
        //            {
        //                // - 방향 이동 예정 → -Limit 센서 Active 여부 검사
        //                if (_driver.ReadNegativeLimit(AxisNo))
        //                    throw new InvalidOperationException("[" + Name + "] -Limit Active 상태에서 -방향 이동 불가");
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            // 인터락 위반을 알람으로도 남기고 예외 전달 (필요시 정책에 따라 변경)
        //            Log.Write("MotionAxis.MoveAbs", ex.Message);
        //            return -1; // 호출측에서 실패 처리; 예외 throw를 원하면 대신 throw;
        //        }

        //        // 3) 펄스 변환 및 구동 수행
        //        var p = _correction.ToHardware(logicalTarget);
        //        var jerk = MapJerkPercentToDriver((int)jerkPercent, (int)jerkPercent);
        //        return _driver.MoveAbsPosition(AxisNo, p, vel, acc, dec, jerk);
        //    }
        //    else
        //    {
        //        throw new InvalidOperationException("This axis does not support Absolute Move.");
        //    }
        //}

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
            // 공통 timeout 보정
            if (timeoutMs < 0)
                timeoutMs = Setup.MoveTimeoutMs > 0 ? Setup.MoveTimeoutMs : 10000;

            // CPU 점유/응답 균형
            const int pollMs = 1;
            if (IsSim)
            {
                var sw = Stopwatch.StartNew();
                while (sw.ElapsedMilliseconds < timeoutMs)
                {
                    bool moving = false;
                    lock (_simLock) 
                        moving = _simIsMoving;

                    if (!moving) 
                        return 0;

                    Thread.Sleep(pollMs);
                }
                return -1;
            }

            if (_driver != null)
            {
                if (true)
                {
                    var sw = Stopwatch.StartNew();
                    while (sw.ElapsedMilliseconds < timeoutMs)
                    {
                        if (_driver.IsMoveDone(AxisNo))
                        {
                            Thread.Sleep(1);
                            if (_driver.IsMoveDone(AxisNo))
                            {
                                return 0;
                            }
                        }
                        Thread.Sleep(pollMs);
                    }
                }
                else
                {
                    var sw = Stopwatch.StartNew();
                    // 목표 위치(논리단위)와 허용 오차
                    double target = GetStatusSnapshot().PV.CommandPosition; // 또는 move 호출 시 목표를 멤버로 저장해 사용
                    double tol = Math.Max(this.Config?.InposTolerance ?? 0.002, 0.002);
                    while (sw.ElapsedMilliseconds < timeoutMs)
                    {
                        bool done1 = _driver.IsMoveDone(AxisNo);

                        // 위치 오차 확인 (논리 단위)
                        double cur = GetPosition();
                        double err = Math.Abs(cur - target);

                        if (done1 && err <= tol)
                        {
                            // 글리치 방지용 1회 재확인
                            Thread.Sleep(1);

                            bool done2 = _driver.IsMoveDone(AxisNo);
                            double cur2 = GetPosition();
                            double err2 = Math.Abs(cur2 - target);

                            if (done2 && err2 <= tol)
                                return 0;
                        }
                        Thread.Sleep(pollMs);
                    }
                }
            }
            else if(_ckdDriver != null)
            {
                timeoutMs = Setup.MoveTimeoutMs;
                var sw = Stopwatch.StartNew();
                // Index 계열 기본값(8분할): 45deg
                // 필요하면 추후 Setup/Config로 치환 가능
                const double stepDeg = 45.0;
                double tolDeg = Math.Max(this.Config?.InposTolerance ?? 0.002, 0.002);
                bool inPos = false;
                bool runWait = false;

                var swWait = Stopwatch.StartNew();
                while (true)
                {
                    if (swWait.ElapsedMilliseconds > 50)
                        break;
                }
                while (sw.ElapsedMilliseconds < timeoutMs)
                {
                    // 1) 상태 신호
                    inPos = _ckdDriver.IsInPosition();
                    runWait = _ckdDriver.IsRunWait();
                    if(false)
                    {
                        if (inPos && runWait)
                            return 0;
                    }
                    else
                    {
                        // 2) 현재 위치 기반 오차 계산 (중요: GetPosition()은 이미 deg 단위)
                        //    기존 코드의 *1000은 단위 해야함. (장비에서 확인 완료)
                        double curDeg = this.GetPosition() * 1000;
                        double rem = curDeg % stepDeg;
                        if (rem < 0) rem += stepDeg;
                        double err = Math.Min(rem, stepDeg - rem);

                        // 3) 안전 완료 판정:
                        //    - 기본: 상태신호 + 위치오차 동시 만족
                        if (inPos && runWait && err <= tolDeg)
                            return 0;
                    }

                    // 4) fallback:
                    //    드라이버 상태 갱신 지연 시 위치오차만으로도 완료 인정(기존 의도 유지)
                    //if (err <= tolDeg)
                    //    return 0;

                    if (sw.ElapsedMilliseconds > timeoutMs)
                        break;

                    Thread.Sleep(pollMs);

                }
                return -1;
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
            if (!Setup.SoftLimitEnable) 
                return;
            if (logicalTarget < Setup.SoftLimitMin || logicalTarget > Setup.SoftLimitMax)
            {
                AlarmPost(BASE_ALARM_SOFT_LIMIT);
                //AlarmPost(AlarmKey.AxisSoftLimit);
                Log.Write("MotionAxis", $"SoftLimit violation: Target {logicalTarget} ∉ [{Setup.SoftLimitMin}, {Setup.SoftLimitMax}]");
                throw new InvalidOperationException(
                    "[" + Name + "] SoftLimit violation: " + logicalTarget + " ∉ [" + Setup.SoftLimitMin + ", " + Setup.SoftLimitMax + "]");
            }
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

        public bool IsMoveDone()
        {
            if (IsSim)
            {
                lock (_simLock)
                {
                    return !_simIsMoving;
                }
            }
            else if (_driver != null)
            {
                return _driver.ReadDone(AxisNo);
                //bool b1 = Status.State.InpositionDone; //아진 이거없다!!!
                //bool b2 = Status.State.Inposition;
                //bool b3 = Status.State.Done;
                //if (b2 && b3)
                //{
                //    return true;
                //}
                //else
                //{
                //    return false;
                //}
            }
            else if (_ckdDriver != null)
            {
                // CKD는 별도 ReadDone 대신 RunWait 상태가 안전 대기 상태로 간주
                //return _ckdDriver.IsRunWait();
                if (_ckdDriver.IsInPosition() && _ckdDriver.IsRunWait())
                    return true;
            }
            return false;
        }

        // ===== Simulation helpers =====
        private void StartSimMoveTo(double target, double velocity, double acc, double dec)
        {
            lock (_simLock)
            {
                // 소프트리밋 클램프
                if (Setup.SoftLimitEnable)
                {
                    if (target > Setup.SoftLimitMax) target = Setup.SoftLimitMax;
                    if (target < Setup.SoftLimitMin) target = Setup.SoftLimitMin;
                }

                double startPos = _simPosition;
                double dist = target - startPos;
                double dir = Math.Sign(dist);
                double D = Math.Abs(dist); // 총 이동 거리

                double v = Math.Abs(velocity);
                double a = Math.Abs(acc);
                double d = Math.Abs(dec);

                if (v <= 0) v = 5;
                if (a <= 0) a = v * 10;
                if (d <= 0) d = v * 10;

                _simTarget = target;
                _simCommandPosition = target;
                _simIsMoving = true;
                _simCurrentVelocity = 0;

                CancelSimCts_NoLock();
                _simMoveCts = new CancellationTokenSource();
                var token = _simMoveCts.Token;

                _simServoOn = true; // 시뮬에서는 자동 서보온 가정

                _simMoveTask = Task.Run(async () =>
                {
                    try
                    {
                        if (D < 1e-6) // 이미 도달한 경우 바로 종료
                        {
                            lock (_simLock)
                            {
                                _simPosition = target;
                                _simCurrentVelocity = 0;
                            }
                            return;
                        }

                        // 사다리꼴(Trapezoidal) 속도 프로파일 구간 계산
                        double ta = v / a; // 가속 시간
                        double td = v / d; // 감속 시간
                        double da = 0.5 * a * ta * ta; // 가속 이동 거리
                        double dd = 0.5 * d * td * td; // 감속 이동 거리

                        // 최고 속도 도달 전 감속해야 하는 경우 (삼각형 프로파일)
                        if (da + dd > D)
                        {
                            v = Math.Sqrt(2 * D / (1 / a + 1 / d));
                            ta = v / a;
                            td = v / d;
                            da = 0.5 * a * ta * ta;
                            dd = 0.5 * d * td * td;
                        }

                        double dc = D - da - dd; // 정속 이동 거리
                        double tc = dc / v;      // 정속 이동 시간
                        double tTotal = ta + tc + td; // 총 이동 시간

                        var sw = Stopwatch.StartNew();

                        while (!token.IsCancellationRequested)
                        {
                            double t = sw.ElapsedMilliseconds / 1000.0; // 경과 시간 (초)

                            bool reached = false;
                            double currentVel = 0;
                            double currentPos = startPos;

                            // 시간 경과에 따른 위치 및 속도 계산
                            if (t >= tTotal)
                            {
                                currentPos = target;
                                currentVel = 0;
                                reached = true;
                            }
                            else if (t <= ta) // 가속 구간
                            {
                                currentVel = a * t;
                                currentPos = startPos + dir * (0.5 * a * t * t);
                            }
                            else if (t <= ta + tc) // 등속 구간
                            {
                                currentVel = v;
                                double timeInConstant = t - ta;
                                currentPos = startPos + dir * (da + v * timeInConstant);
                            }
                            else // 감속 구간
                            {
                                double timeInDecel = t - (ta + tc);
                                currentVel = v - d * timeInDecel;
                                currentPos = startPos + dir * (da + dc + (v * timeInDecel - 0.5 * d * timeInDecel * timeInDecel));
                            }

                            lock (_simLock)
                            {
                                _simPosition = currentPos;
                                _simCurrentVelocity = currentVel * dir;

                                // 소프트리밋 실시간 재확인
                                if (Setup.SoftLimitEnable)
                                {
                                    if ((dir > 0 && _simPosition >= Setup.SoftLimitMax) ||
                                        (dir < 0 && _simPosition <= Setup.SoftLimitMin))
                                    {
                                        _simPosition = dir > 0 ? Setup.SoftLimitMax : Setup.SoftLimitMin;
                                        _simCurrentVelocity = 0;
                                        reached = true;
                                    }
                                }
                            }

                            if (reached) break;

                            await Task.Delay(5).ConfigureAwait(false); // 5ms 간격으로 위치 갱신
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

        // ===== 수정: 조건이 true일 때 1회만 AlarmPost (동적 Key 매핑 적용) =====
        private void PostOnceWhenTrue(int baseAlarmType, bool condition)
        {
            // 축 번호를 이용해 실제 Enum 값 도출
            AlarmKey key = (AlarmKey)(this.AxisNo * 100 + baseAlarmType);

            if (!condition)
            {
                // 조건이 해제되면 래치 해제(다음에 다시 발생 가능하도록)
                lock (_alarmLatchLock) { _postedMonitorAlarms.Remove(key); }
                return;
            }

            bool alreadyPosted;
            lock (_alarmLatchLock) { alreadyPosted = !_postedMonitorAlarms.Add(key); }
            if (alreadyPosted) return;

            try { AlarmPost(key); }
            catch (Exception ex) { Log.Write(ex); }
        }
        //private void PostOnceWhenTrue(AlarmKey key, bool condition)
        //{
        //    if (!condition)
        //    {
        //        // 조건이 해제되면 래치 해제(다음에 다시 발생 가능하도록)
        //        lock (_alarmLatchLock) { _postedMonitorAlarms.Remove(key); }
        //        return;
        //    }

        //    bool alreadyPosted;
        //    lock (_alarmLatchLock) { alreadyPosted = !_postedMonitorAlarms.Add(key); }
        //    if (alreadyPosted) return;

        //    try { AlarmPost(key); }
        //    catch (Exception ex) { Log.Write(ex); }
        //}

        /// <summary>
        /// 서버오프(Servo OFF), 드라이버 알람, 리밋 상태를 점검하고 필요 시 알람을 발생시킵니다.
        /// - 주기적으로(예: 타이머 100~500ms) 호출하는 용도
        /// - 기존 AlarmPost/AlarmManager 인프라 그대로 사용
        /// </summary>
        public void CheckAndPostSafetyAlarms()
        {
            try
            {
                bool servoOn = true;
                bool alarmOn = false;
                bool posLimit = false;
                bool negLimit = false;

                if (IsSim)
                {
                    lock (_simLock)
                    {
                        servoOn = _simServoOn;
                        alarmOn = _simAlarm;
                        // 시뮬은 리밋 센서 개념이 없으므로 false 유지
                    }
                }
                else if (_driver != null)
                {
                    servoOn = _driver.ReadServoOn(AxisNo);
                    alarmOn = _driver.ReadAlarm(AxisNo);
                    posLimit = _driver.ReadPositiveLimit(AxisNo);
                    negLimit = _driver.ReadNegativeLimit(AxisNo);
                }
                else if (_ckdDriver != null)
                {
                    servoOn = _ckdDriver.IsServoOn();
                    alarmOn = _ckdDriver.IsAlarm();
                    // CKD는 DD Motor라 별도 +/- limit 없음(현재 Status에서도 false 처리)
                }
                else
                {
                    // 드라이버 미할당은 여기서 알람 처리 정책이 애매하므로 우선 무시
                    return;
                }

                // 변경 후: 동적 상수 전달
                PostOnceWhenTrue(BASE_ALARM_SERVO_OFF, !servoOn);
                PostOnceWhenTrue(BASE_ALARM_DRIVER_ALARM, alarmOn);
                PostOnceWhenTrue(BASE_ALARM_POSITIVE_LIMIT, posLimit);
                PostOnceWhenTrue(BASE_ALARM_NEGATIVE_LIMIT, negLimit);
                //// 조건별 알람 발생(조건 true일 때 1회만)
                //PostOnceWhenTrue(AlarmKey.AxisServoOff, !servoOn);
                //PostOnceWhenTrue(AlarmKey.AxisDriverAlarm, alarmOn);
                //PostOnceWhenTrue(AlarmKey.AxisPositiveLimit, posLimit);
                //PostOnceWhenTrue(AlarmKey.AxisNegativeLimit, negLimit);

            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
        }

    }
}
