
using QMC.Common;
using QMC.Common.Alarm;
using QMC.Common.Cameras.HIKVISION;
using QMC.Common.Component;
using QMC.Common.IOUtil;
using QMC.Common.Motion;
using QMC.Common.Motions;
using QMC.Common.PKGTester;
using QMC.Common.ThetaCorrection;
using QMC.Common.Unit;
using QMC.Common.Vision;
using QMC.Common.VisionPart;
using QMC.LCP_280.Process.Component;
using QMC.LCP_280.Process.Unit.FormWork.Repro;
using System; // added for Obsolete attribute
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using static QMC.LCP_280.Process.Equipment;
using System.Diagnostics;

namespace QMC.LCP_280.Process.Unit
{
    public class OutputStage : BaseUnit<OutputStageConfig>
    {
        private bool IsDryRunEqp
        {
            get
            {
                var eq = Equipment.Instance;
                bool r = eq.EquipmentConfig.IsDryRun;
                return r;
            }
        }

        public event EventHandler<PatternMarksFoundEventArgs> MarksFound;

        public LinkTypeXYTStageCorrection linkTypeXYTStageCorrection { get; set; }
        public sealed class DiePlacedEventArgs : EventArgs
        {
            public MaterialDie Die { get; }
            public double BinX { get; }
            public double BinY { get; }

            public DiePlacedEventArgs(MaterialDie die)
            {
                Die = die;
                if (die != null)
                {
                    BinX = die.BinX;
                    BinY = die.BinY;
                }
            }
        }
        public event EventHandler<DiePlacedEventArgs> DiePlaced;

        public delegate void UpdateUIWafer(MaterialWafer wafer);
        public event UpdateUIWafer EventUpdateUIWafer;

        public new enum AlarmKeys
        {
            eDieTransferPlaceZNotSafety = 11101,
            eOutputFeederCylinderZNotSafety,
            eOutputFeederYNotSafe,
            eNoBinDetected,
            eClampFB,
            eClampLift,
            ePlate,
            eStageNotLoding,
            ePlateCyliderZNotDown,
            eNotReadyToMeasure,
        }

        #region InitAlarm
        protected override void InitAlarm()
        {
            string source = "Bin_Stage";
            base.InitAlarm();


            // 1. 공용 파일 로더에서 알람 목록 가져오기
            var loadedAlarms = GlobalAlarmTable.Instance.GetAlarmsForSource(source);
            if (loadedAlarms == null || loadedAlarms.Count == 0)
            {
                Log.Write("AlarmInit", $"Cannot find alarms for source '{source}' in the alarm file. Only default alarms will be registered.");


                AlarmInfo alarm = new AlarmInfo();
                alarm.Code = (int)AlarmKeys.eDieTransferPlaceZNotSafety;
                alarm.Title = "Die TrZAxis Not safety Pos.";
                alarm.Cause = "Die Transfer Z-Axis is not in a safety position. Please check the position and restart.";
                alarm.Source = source;// this.UnitName;
                alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
                m_dicAlarms.Add(alarm.Code, alarm);

                alarm = new AlarmInfo();
                alarm.Code = (int)AlarmKeys.eOutputFeederCylinderZNotSafety;
                alarm.Title = "Feeder Z-Cylinder Not safety Pos.";
                alarm.Cause = "Feeder Z-Cylinder is not in a safety position. Please check the position and restart.";
                alarm.Source = source;// this.UnitName;
                alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
                m_dicAlarms.Add(alarm.Code, alarm);

                //
                alarm = new AlarmInfo();
                alarm.Code = (int)AlarmKeys.eOutputFeederYNotSafe;
                alarm.Title = "Feeder Y-Axis Not safety Pos.";
                alarm.Cause = "Feeder Y-Axis is not in a safety position. Please check the position and restart.";
                alarm.Source = source;// this.UnitName;
                alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
                m_dicAlarms.Add(alarm.Code, alarm);

                //eNoBinDetected
                alarm = new AlarmInfo();
                alarm.Code = (int)AlarmKeys.eNoBinDetected;
                alarm.Title = "No Bin Detected";
                alarm.Cause = "Bin is not detected. Please check if Bin is present and restart.";
                alarm.Source = source;// this.UnitName;
                alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
                m_dicAlarms.Add(alarm.Code, alarm);

                //,
                alarm = new AlarmInfo();
                alarm.Code = (int)AlarmKeys.eClampFB;
                alarm.Title = "Clamp F/B Not Pos.";
                alarm.Cause = "Clamp F/B is not in the designated position. Please check the position and restart.";
                alarm.Source = source;// this.UnitName;
                alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
                m_dicAlarms.Add(alarm.Code, alarm);
                //,
                alarm = new AlarmInfo();
                alarm.Code = (int)AlarmKeys.eClampLift;
                alarm.Title = "Clamp Lift Not Pos.";
                alarm.Cause = "Clamp Lift is not in the designated position. Please check the position and restart.";
                alarm.Source = source;// this.UnitName;
                alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
                m_dicAlarms.Add(alarm.Code, alarm);
                //,
                alarm = new AlarmInfo();
                alarm.Code = (int)AlarmKeys.ePlate;
                alarm.Title = "Plate Not Pos.";
                alarm.Cause = "Plate is not in the designated position. Please check the position and restart.";
                alarm.Source = source;// this.UnitName;
                alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
                m_dicAlarms.Add(alarm.Code, alarm);
                //,
                alarm = new AlarmInfo();
                alarm.Code = (int)AlarmKeys.eStageNotLoding;
                alarm.Title = "Stage Not Loading Pos.";
                alarm.Cause = "Stage is not in the ready position.\n Please check the position and restart.";
                alarm.Source = source;// this.UnitName;
                alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
                m_dicAlarms.Add(alarm.Code, alarm);
                //,
                alarm = new AlarmInfo();
                alarm.Code = (int)AlarmKeys.ePlateCyliderZNotDown;
                alarm.Title = "Plate CylinerZ Not Down.";
                alarm.Cause = "Plate CylinderZ is not in the down position.\n Please check the status and restart.";
                alarm.Source = source;// this.UnitName;
                alarm.Grade = AlarmInfo.AlarmType.Error.ToString();
                m_dicAlarms.Add(alarm.Code, alarm);

                alarm = new AlarmInfo();
                alarm.Code = (int)AlarmKeys.eNotReadyToMeasure;
                alarm.Title = "Not ready to measure.";
                alarm.Cause = "1. Please check if a Test Condition Set is applied. 2. Please check if the instrument is initialized properly.";
                alarm.Source = source;// this.UnitName;
                alarm.Grade = AlarmInfo.AlarmType.Warning.ToString();
                m_dicAlarms.Add(alarm.Code, alarm);
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
        #endregion

        // OutStage camera
        public HIKGigECamera OutStageCamera { get; private set; }
        public string OutStageCameraKey { get; set; } = "Out_Stage";
        public PatternMatchingRunner _pmRunner;
        // Pattern Matching Runner (간소화: Recipe 자동 관리)
        public PatternMatchingRunner PmRunner
        {
            get
            {
                if (_pmRunner == null)
                {
                    _pmRunner = VisionRunnerHub.GetOrCreate(OutStageCameraKey);
                }
                return _pmRunner;
            }
        }


        //Unit
        OutputDieTransfer OutputDieTransfer { get; set; }
        OutputFeeder OutputFeeder { get; set; }
        OutputCassetteLifter OutputCassetteLifter { get; set; }
        Rotary RotaryUnit { get; set; }
        InputStage InputStage { get; set; }

        MaterialDie _currentDie = null;


        // ================== Takt Time(Die Place Interval) ==================
        private readonly object _placeTaktLock = new object();
        private DateTime _lastDiePlacedAt = DateTime.MinValue;

        /// <summary>
        /// "Bin Wafer에 die를 내려놓은 시점" 기준으로 다음 die를 내려놓을 때까지의 간격을 측정합니다.
        /// (즉, Place-to-Place takt)
        /// </summary>
        public CycleTimer DiePlaceTaktTimer { get; } = new CycleTimer();

        public sealed class DiePlaceTaktEventArgs : EventArgs
        {
            public int DieIndex { get; }
            public DateTime PreviousPlacedAt { get; }
            public DateTime CurrentPlacedAt { get; }
            public TimeSpan Interval { get; }

            public DiePlaceTaktEventArgs(int dieIndex, DateTime previous, DateTime current)
            {
                DieIndex = dieIndex;
                PreviousPlacedAt = previous;
                CurrentPlacedAt = current;
                Interval = current - previous;
            }
        }

        /// <summary>
        /// 직전 Place → 이번 Place 간격이 측정되어 CycleTimer에 누적된 직후 발생합니다.
        /// </summary>
        public event EventHandler<DiePlaceTaktEventArgs> DiePlaceTaktMeasured;

        /// <summary>측정 데이터 초기화(런 시작/레시피 변경 시 호출 권장)</summary>
        public void ResetDiePlaceTakt()
        {
            lock (_placeTaktLock)
            {
                _lastDiePlacedAt = DateTime.MinValue;
                DiePlaceTaktTimer.Clear();
                DiePlaceTaktTimer.TotalElapsed = TimeSpan.Zero;
            }
        }

        private void RecordDiePlaceTakt(MaterialDie die)
        {
            // PlaceDie는 die/wafer null 체크를 상위에서 하고 있으나, 안전하게 방어
            if (die == null)
                return;

            DateTime now = DateTime.Now;
            DateTime prev;

            lock (_placeTaktLock)
            {
                prev = _lastDiePlacedAt;

                // 첫 Place는 기준점만 잡고 종료
                if (prev == DateTime.MinValue)
                {
                    _lastDiePlacedAt = now;
                    return;
                }

                // [FIX] 정확한 prev→now를 CycleTimer에 누적
                DiePlaceTaktTimer.Add(prev, now);

                _lastDiePlacedAt = now;
            }

            // 정확한 interval은 prev/now로 계산해서 내보냄
            var args = new DiePlaceTaktEventArgs(die.Index, prev, now);

            // [ADD] WaferSummary CycleTime에 Takt(ms) 기록
            try
            {
                var summary = Equipment.Instance.SummaryContext?.GetCurrentSummaryOrNull();
                summary?.SetCycleTimeMsFromTakt(args.Interval);
            }
            catch (Exception ex)
            {
                Log.Write(UnitName, "DiePlaceTakt->SummaryCycleTime", ex.Message);
            }

            Log.Write(UnitName, "DiePlaceTakt",
                $"DieIndex={args.DieIndex}, IntervalMs={args.Interval.TotalMilliseconds:F1}, Prev={args.PreviousPlacedAt:HH:mm:ss.fff}, Now={args.CurrentPlacedAt:HH:mm:ss.fff}");

            // [ADD] CSV 저장
            try 
            { 
                AppendDiePlaceTaktCsv(args); 
            } 
            catch (Exception ex) 
            { Log.Write(UnitName, "DiePlaceTaktCsv", ex.Message); }

            try { DiePlaceTaktMeasured?.Invoke(this, args); } catch { }
        }

        private readonly object _taktCsvLock = new object();

        private string GetDiePlaceTaktCsvPath(DateTime now)
        {
            string dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Log", "TaktTime");
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            return Path.Combine(dir, $"OutputStage_DiePlaceTakt_{now:yyyyMMdd}.csv");
        }

        private void AppendDiePlaceTaktCsv(DiePlaceTaktEventArgs args)
        {
            if (args == null)
                return;

            var now = DateTime.Now;
            string path = GetDiePlaceTaktCsvPath(now);

            // 통계 스냅샷
            double avgMs = DiePlaceTaktTimer.Average.TotalMilliseconds;
            double minMs = DiePlaceTaktTimer.Minimum.TotalMilliseconds;
            double maxMs = DiePlaceTaktTimer.Maximum.TotalMilliseconds;
            int count = DiePlaceTaktTimer.CycleTimes?.Count ?? 0;

            lock (_taktCsvLock)
            {
                bool exists = File.Exists(path);
                using (var w = new StreamWriter(path, true, Encoding.UTF8))
                {
                    if (!exists)
                    {
                        w.WriteLine("Time,DieIndex,PrevTime,NowTime,IntervalMs,AvgMs,MinMs,MaxMs,Count");
                    }

                    w.WriteLine(string.Format(CultureInfo.InvariantCulture,
                        "{0},{1},{2},{3},{4:F1},{5:F1},{6:F1},{7:F1},{8}",
                        now.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                        args.DieIndex,
                        args.PreviousPlacedAt.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                        args.CurrentPlacedAt.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                        args.Interval.TotalMilliseconds,
                        avgMs, minMs, maxMs, count));
                }
            }
        }



        // [ADD] GetBinWorldPosition()에서 런타임 중심 인덱스가 흔들리지 않도록 스냅샷 캐싱
        private readonly object _indexCenterLock = new object();
        private bool _indexCenterInitialized;
        private double _indexCenterXSnapshot;
        private double _indexCenterYSnapshot;



        public OutputStage(OutputStageConfig config = null)
            : base(new OutputStageConfig())
        {
            AddComponents();
        }

        public override void AddComponents()
        {
            Config.LoadAndBindAxes(Equipment.Instance.AxisManager);
            Config.InitializeDefaultTeachingPositions();

            BindAxes();
            BindIoDomains();
            BindCamera();

            // 사용안한다고 Load를 안할필요는없지...
            //if(Config.TCorrectionMode)
            {
                LoadTCorrectionCsvAndApply(Config.TCorrectionFile);
            }
        }

        protected override void OnBindUnit()
        {
            base.OnBindUnit();
            OutputFeeder = Equipment.Instance.GetUnit(UnitKeys.OutputFeeder) as OutputFeeder;
            OutputDieTransfer = Equipment.Instance.GetUnit(UnitKeys.OutputDieTransfer) as OutputDieTransfer;
            OutputCassetteLifter = Equipment.Instance.GetUnit(UnitKeys.OutputCassetteLifter) as OutputCassetteLifter;
            RotaryUnit = Equipment.Instance.GetUnit(UnitKeys.Rotary) as Rotary;

            // [ADD]
            InputStage = Equipment.Instance.GetUnit(UnitKeys.InputStage) as InputStage;
        }

        private void BindCamera()
        {
            var eq = Equipment.Instance;
            if (eq == null) return;
            if (eq.Cameras != null && eq.Cameras.TryGetValue(OutStageCameraKey, out var cam))
                OutStageCamera = cam as HIKGigECamera;
            else
                OutStageCamera = eq.OutStageCam; // fallback
        }

        #region Axis Helpers
        private MotionAxis _axX, _axY, _axT;
        public MotionAxis AxisX => _axX;
        public MotionAxis AxisY => _axY;
        public MotionAxis AxisT => _axT;

        private void BindAxes()
        {
            var mgr = Equipment.Instance?.AxisManager;
            if (mgr == null)
            {
                Log.Write("OutputStage", "[BindAxes] AxisManager null");
                return;
            }

            const string unitName = "Unit"; // Equipment에서 축 등록 시 사용한 유닛명과 동일해야 함
            BindAxis(mgr, unitName, AxisNames.BinStageX, ref _axX);
            BindAxis(mgr, unitName, AxisNames.BinStageY, ref _axY);
            BindAxis(mgr, unitName, AxisNames.BinStageT, ref _axT);
        }

        #endregion

        #region IO Domain Mapping (Reorganized)
        private Cylinder _cylClampLift;
        private Cylinder _cylClampFB;
        private Cylinder _cylPlate;
        private Vacuum _vacuum;

        private void BindIoDomains()
        {
            var eq = Equipment.Instance; var unit = eq?.UnitIO; if (unit == null) return;

            // Vacuum 별칭으로 조회만
            if (!IoAutoBindings.Vacuums.TryGetValue("OutStageVac", out _vacuum))
            {
                Log.Write("OutputStage", "BindIoDomains", "Vacuums not found: OutStageVac");
            }

            // Cylinder는 중앙 별칭으로 조회만
            if (!IoAutoBindings.Cylinders.TryGetValue("OutStagePlate", out _cylPlate))
            {
                Log.Write("OutputStage", "BindIoDomains", "Cylinder not found: OutStagePlate");
            }
            BindCylinder(_cylPlate);

            if (!IoAutoBindings.Cylinders.TryGetValue("OutStageLift", out _cylClampLift))
            {
                Log.Write("OutputStage", "BindIoDomains", "Cylinder not found: OutStageLift");
            }
            BindCylinder(_cylClampLift);

            if (!IoAutoBindings.Cylinders.TryGetValue("OutStageClampFB", out _cylClampFB))
            {
                Log.Write("OutputStage", "BindIoDomains", "Cylinder not found: OutStageClampFB");
            }
            BindCylinder(_cylClampFB);
        }
        public override bool IsInterlockOK(BaseComponent baseComponent, BaseComponent.InterlockEventArgs e)
        {
            bool bRet = base.IsInterlockOK(baseComponent, e);
            if (baseComponent == AxisX || baseComponent == AxisY || baseComponent == AxisT)
            {
                if (this.OutputDieTransfer.IsPositionPlaceZSafety() == false)
                {
                    this.AxisX?.EmgStop();
                    this.AxisY?.EmgStop();
                    this.AxisT?.EmgStop();
                    PostAlarm((int)AlarmKeys.eDieTransferPlaceZNotSafety);
                    return false;
                }
                else if (this.IsPlateDown() == false)
                {
                    this.AxisX?.EmgStop();
                    this.AxisY?.EmgStop();
                    this.AxisT?.EmgStop();
                    PostAlarm((int)AlarmKeys.ePlateCyliderZNotDown);
                    return false;
                }
                else if (this.OutputFeeder.IsFeederUp() == false)
                {
                    this.AxisX?.EmgStop();
                    this.AxisY?.EmgStop();
                    this.AxisT?.EmgStop();
                    PostAlarm((int)AlarmKeys.eOutputFeederCylinderZNotSafety);
                    return false;
                }
            }
            else if (baseComponent == this._cylPlate)
            {
                if (e.IsExtend)
                {
                    if (this.IsPositionBinLoading() == false)
                    {
                        this.PlateDown();
                        PostAlarm((int)AlarmKeys.eStageNotLoding);
                        return false;
                    }
                    //else if (this.OutputFeeder.IsFeederUp() == false)
                    //{
                    //    this.PlateDown();
                    //    PostAlarm((int)AlarmKeys.eOutputFeederCylinderZNotSafety);
                    //    return false;
                    //}
                }
            }
            else if (baseComponent == this._cylClampLift)
            {
                if (e.IsExtend)
                {
                    if (this.OutputFeeder.IsFeederUp() == false)
                    {
                        this.PlateDown();
                        PostAlarm((int)AlarmKeys.eOutputFeederCylinderZNotSafety);
                        return false;
                    }
                }
            }
            return bRet;
        }
        private bool IsAtTeaching(OutputStageConfig.TeachingPositionName name)
        {
            // Config에 저장된 TeachingPosition 조회
            var tp = Config.GetTeachingPosition(name.ToString());
            if (tp == null || tp.AxisPositions == null || tp.AxisPositions.Count == 0)
                return false;

            // TeachingPosition에 포함된 각 축이 모두 In-Position인지 검사
            foreach (var kv in tp.AxisPositions)
            {
                var axisKey = kv.Key;
                var target = kv.Value;

                MotionAxis ax;
                if (!Axes.TryGetValue(axisKey, out ax) || ax == null)
                    return false;

                if (!InPos(ax, target))
                    return false;
            }
            return true;
        }

        // === Domain Control (표준 구동) ===
        public bool SetVacuum(bool on, bool bCheckSignal = false)
        {
            if (_vacuum == null)
                return false;

            if (bCheckSignal == false)
            {
                if (on)
                    _vacuum.On();
                else
                    _vacuum.Off();
            }
            else
            {
                if (on)
                    _vacuum.OnWaitOk();
                else
                    _vacuum.OffWaitOk();
            }

            return true;
        }

        public bool SetClampPlate(bool bUpDn)
        {
            if (_cylPlate == null)
                return false;

            if (bUpDn)
            {
                //if (!IsAtTeaching(OutputStageConfig.TeachingPositionName.Loading) &&
                //    !IsAtTeaching(OutputStageConfig.TeachingPositionName.Unloading))
                //{
                //    MessageBox.Show("SetClampPlate Interlock",
                //              "Plate UP blocked: not at Loading/Unloading teaching position.",
                //              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                //    return false;
                //}
                return _cylPlate.Extend();
            }
            else
            {
                return _cylPlate.Retract();
            }
        }

        public bool SetClampLift(bool bUpDn)
        {
            if (_cylClampLift == null)
                return false;

            if (bUpDn)
            {
                return _cylClampLift.Extend();
            }
            else
            {
                if (!IsClampBwd())
                    return false; // 기존 인터락 유지

                return _cylClampLift.Retract();
            }
        }

        public bool SetClampFB(bool bFwdBwd)
        {
            if (_cylClampFB == null)
                return false;

            if (bFwdBwd)
            {
                if (!IsClampLiftUp())
                    return false; // 기존 인터락 유지

                return _cylClampFB.Extend();
            }
            else
            {
                //if (!IsClampLiftUp())
                //    return false; // 기존 인터락 유지

                return _cylClampFB.Retract();
            }
        }

        // --- Existing High-Level APIs (인터락 포함) ---
        public bool IsVacuumOn()
        {
            if (Config.IsSimulation || (Config.IsDryRun || IsDryRunEqp))
            {
                return true;
            }

            return this.ReadInput(OutputStageConfig.IO.VACUUM_CHECK);
        }
        public bool Ring0()
        {
            if (Config.IsSimulation || (Config.IsDryRun || IsDryRunEqp))
            {
                return true;
            }

            return this.ReadInput(OutputStageConfig.IO.RING_CHECK0);
        }
        public bool Ring1()
        {
            if (Config.IsSimulation || (Config.IsDryRun || IsDryRunEqp))
            {
                return true;
            }

            return this.ReadInput(OutputStageConfig.IO.RING_CHECK1);
        }
        public bool IsClampLiftUp()
        {
            if (Config.IsSimulation)
            {
                return true;
            }

            return !IsClampLiftDown(); 
        }
        public bool IsClampLiftDown()
        {
            if (Config.IsSimulation)
            {
                return true;
            }

            return this.ReadInput(OutputStageConfig.IO.CLAMP_DOWN_CHECK);
        }
        public bool IsClampFwd()
        {
            if (Config.IsSimulation)
            {
                return true;
            }

            return this.ReadInput(OutputStageConfig.IO.CLAMP_FWD_CHECK);
        }
        public bool IsClampBwd()
        {
            if (Config.IsSimulation)
            {
                return true;
            }

            return !IsClampFwd();
        }
        public bool IsPlateUp()
        {
            if (Config.IsSimulation)
            {
                return true;
            }

            return this.ReadInput(OutputStageConfig.IO.PLATE_UP);
        }
        public bool IsPlateDown()
        {
            if (Config.IsSimulation)
            {
                return true;
            }

            return this.ReadInput(OutputStageConfig.IO.PLATE_DOWN);
        }

        // === Cylinder 완료 대기 Helpers ===
        // Plate: expectUp=true(UP 기대), false(DOWN 기대)
        private int WaitPlateStateOrAlarm(bool expectUp, int timeoutMs = 3000, int pollMs = 1)
        {
            if (Config.IsSimulation || (Config.IsDryRun || IsDryRunEqp))
                return 0;

            var sw = System.Diagnostics.Stopwatch.StartNew();
            while (sw.ElapsedMilliseconds <= timeoutMs)
            {
                bool ok = expectUp ? IsPlateUp() : IsPlateDown();
                if (ok)
                    return 0;

                Thread.Sleep(pollMs);
            }

            PostAlarm((int)AlarmKeys.ePlate);
            Log.Write(UnitName, expectUp ? "[Plate] UP timeout" : "[Plate] DOWN timeout");
            return -1;
        }

        // ClampLift: expectUp=true(UP 기대), false(DOWN 기대)
        private int WaitClampLiftStateOrAlarm(bool expectUp, int timeoutMs = 3000, int pollMs = 1)
        {
            if (Config.IsSimulation || (Config.IsDryRun || IsDryRunEqp))
                return 0;

            var sw = System.Diagnostics.Stopwatch.StartNew();
            while (sw.ElapsedMilliseconds <= timeoutMs)
            {
                bool ok = expectUp ? IsClampLiftUp() : IsClampLiftDown();
                if (ok)
                    return 0;

                Thread.Sleep(pollMs);
            }

            PostAlarm((int)AlarmKeys.eClampLift);
            Log.Write(UnitName, expectUp ? "[ClampLift] UP timeout" : "[ClampLift] DOWN timeout");
            return -1;
        }

        // Clamp F/B: expectFwd=true(FWD 기대), false(BWD 기대)
        private int WaitClampFBStateOrAlarm(bool expectFwd, int timeoutMs = 3000, int pollMs = 1)
        {
            if (Config.IsSimulation || (Config.IsDryRun || IsDryRunEqp))
                return 0;

            var sw = System.Diagnostics.Stopwatch.StartNew();
            while (sw.ElapsedMilliseconds <= timeoutMs)
            {
                bool ok = expectFwd ? IsClampFwd() : IsClampBwd();
                if (ok)
                    return 0;

                Thread.Sleep(pollMs);
            }

            PostAlarm((int)AlarmKeys.eClampFB);
            Log.Write(UnitName, expectFwd ? "[ClampFB] FWD timeout" : "[ClampFB] BWD timeout");
            return -1;
        }

        // === Direct Valve Control (입력 신호/인터락 무관 강제 구동용) ===
        public bool IsVacuumValveOn()
        {
            if (Config.IsSimulation)
            {
                return true;
            }

            return this.IsOutputOn(OutputStageConfig.IO.VACUUM);
        }
        #endregion


        // ================== Generic Single Axis Move (Safety Interlock 동일 구조) ==================
        /// <summary>
        /// 단일 축 이동 (Safety 인터락 포함). 이동 완료까지 블록.
        /// </summary>
        public int MoveAxisPositionOne(MotionAxis axis, double target, bool isFine = false)
        {
            if (axis == null) return -1;

            Task<int> task = MoveAxisPositionOneAsync(axis, target, isFine);
            while (IsEndTask(task) == false)
            {
                // 동일 Safety Interlock
                if (!OutputDieTransfer.IsPositionPlaceZSafety())
                {
                    AxisX?.EmgStop(); AxisY?.EmgStop(); AxisT?.EmgStop();
                    PostAlarm((int)AlarmKeys.eDieTransferPlaceZNotSafety);
                    return -1;
                }
                if (!OutputFeeder.IsFeederZSafetyPosition())
                {
                    AxisX?.EmgStop(); AxisY?.EmgStop(); AxisT?.EmgStop();
                    PostAlarm((int)AlarmKeys.eOutputFeederCylinderZNotSafety);
                    return -1;
                }
                if (!OutputFeeder.IsFeederYSafetyPosition())
                {
                    AxisX?.EmgStop(); AxisY?.EmgStop(); AxisT?.EmgStop();
                    PostAlarm((int)AlarmKeys.eOutputFeederYNotSafe);
                    return -1;
                }

                Thread.Sleep(1);
            }
            return task.Result;
        }
        public int MoveApplyOffset(string positionName, double dx, double dy, double dt)
        {
            // Teaching Position 가져오기
            var tp = Config.GetTeachingPosition(positionName);
            if (tp == null) 
                return -1;

            // 오프셋 적용
            Config.SetOffset(positionName, dx, dy, dt);
            var (x, y, t) = Config.GetPositionWithOffset(positionName);   //Offset 포함 위치 - Align 수행 시 data 있음.

            int rc = 0;
            if (AxisX != null) rc |= MoveAxisPositionOne(AxisX, x, false);
            if (AxisY != null) rc |= MoveAxisPositionOne(AxisY, y, false);
            if (AxisT != null) rc |= MoveAxisPositionOne(AxisT, t, false);
            if (rc != 0) 
                return -1;

            return 0;
        }
        
        /// //////////////////////////////////////////////////////////////////////////////////////////////
        public int MoveTeachingPositionOnce(OutputStageConfig.TeachingPositionName name, bool isFine)
        {
            return MoveTeachingPositionOnce((int)name, isFine);
        }

        private int IsInterlockStageOK()
        {
            int nRet = 0;

            if (OutputDieTransfer.IsPositionPickZSafety() == false)
            {
                return -1;
            }

            if (OutputFeeder.IsPositionReady() == false)
            {
                return -2;
            }

            if (OutputFeeder.IsFeederZSafetyPosition() == false)
            {
                return -3;
            }
            return nRet;
        }
        public int MoveToStageReadyPosition(bool isFine = false)
        {
            int nRet = 0;
            Task<int> task = MoveToStageReadyPositionAsync();
            while (IsEndTask(task) == false)
            {
                // Check Interlock.!!! 구문 넣을것.!!!
                nRet = IsInterlockStageOK();
                if (nRet == -1)
                {
                    this.AxisX.EmgStop();
                    this.AxisY.EmgStop();
                    this.AxisT.EmgStop();
                    PostAlarm((int)AlarmKeys.eDieTransferPlaceZNotSafety);
                    return -1;
                }
                if (nRet == -2)
                {
                    this.AxisX.EmgStop();
                    this.AxisY.EmgStop();
                    this.AxisT.EmgStop();
                    PostAlarm((int)AlarmKeys.eOutputFeederCylinderZNotSafety);
                    return -1;
                }

                Thread.Sleep(1);
            }
            return task.Result;
        }
        public Task<int> MoveToStageReadyPositionAsync()
        {
            return Task.Run(() =>
            {
                OnMoveToStageReadyPosition();
                return 0;
            });
        }
        private int OnMoveToStageReadyPosition(bool isFine = false)
        {
            int nRet = 0;
            nRet = IsInterlockStageOK();
            if (nRet == -1)
            {
                this.AxisX.EmgStop();
                this.AxisY.EmgStop();
                this.AxisT.EmgStop();
                PostAlarm((int)AlarmKeys.eDieTransferPlaceZNotSafety);
                return -1;
            }
            if (nRet == -2)
            {
                this.AxisX.EmgStop();
                this.AxisY.EmgStop();
                this.AxisT.EmgStop();
                PostAlarm((int)AlarmKeys.eOutputFeederCylinderZNotSafety);
                return -1;
            }
            return MoveTeachingPositionOnce((int)OutputStageConfig.TeachingPositionName.Ready, isFine);
        }

        public int MoveToStageCenterPosition(bool isFine = false)
        {
            Task<int> task = MoveToStageCenterPositionAsync(isFine);
            while (IsEndTask(task) == false)
            {
                if (OutputDieTransfer.IsPositionPickZSafety() == false)
                {
                    this.AxisX.EmgStop();
                    this.AxisY.EmgStop();
                    this.AxisT.EmgStop();
                    PostAlarm((int)AlarmKeys.eDieTransferPlaceZNotSafety);
                    return -1;
                }

                if (OutputFeeder.IsFeederZSafetyPosition() == false)
                {
                    this.AxisX.EmgStop();
                    this.AxisY.EmgStop();
                    this.AxisT.EmgStop();
                    PostAlarm((int)AlarmKeys.eOutputFeederCylinderZNotSafety);
                    return -1;
                }

                if (OutputFeeder.IsFeederYSafetyPosition() == false)
                {
                    this.AxisX.EmgStop();
                    this.AxisY.EmgStop();
                    this.AxisT.EmgStop();
                    PostAlarm((int)AlarmKeys.eOutputFeederYNotSafe);
                    return -1;
                }

                if(IsPlateDown() == false)
                {
                    this.AxisX.EmgStop();
                    this.AxisY.EmgStop();
                    this.AxisT.EmgStop();
                    PostAlarm((int)AlarmKeys.ePlateCyliderZNotDown);
                    return -1;
                }

                Thread.Sleep(1);
            }
            return task.Result;
        }
        public Task<int> MoveToStageCenterPositionAsync(bool isFine = false)
        {
            return Task.Run(() =>
            {
                OnMoveToStageCenterPosition(isFine);
                return 0;
            });
        }
        private int OnMoveToStageCenterPosition(bool isFine = false)
        {
            int nRet = 0;
            nRet = IsInterlockStageOK();
            if (nRet == -1)
            {
                this.AxisX.EmgStop();
                this.AxisY.EmgStop();
                this.AxisT.EmgStop();
                PostAlarm((int)AlarmKeys.eDieTransferPlaceZNotSafety);
                return -1;
            }
            if (nRet == -2)
            {
                this.AxisX.EmgStop();
                this.AxisY.EmgStop();
                this.AxisT.EmgStop();
                PostAlarm((int)AlarmKeys.eOutputFeederCylinderZNotSafety);
                return -1;
            }

            if (IsPlateDown() == false)
            {
                this.AxisX.EmgStop();
                this.AxisY.EmgStop();
                this.AxisT.EmgStop();
                PostAlarm((int)AlarmKeys.ePlateCyliderZNotDown);
                return -1;
            }

            return MoveTeachingPositionOnce((int)OutputStageConfig.TeachingPositionName.CenterPoint, isFine);
        }

        public int MoveToStageLoadPosition(bool isFine = false)
        {
            if (IsPositionBinLoading())
                return 0;

            int nRet = 0;
            Task<int> task = MoveToStageLoadPositionAsync();
            while (IsEndTask(task) == false)
            {
                // Check Interlock.!!! 구문 넣을것.!!!
                nRet = IsInterlockStageOK();
                if (nRet == -1)
                {
                    this.AxisX.EmgStop();
                    this.AxisY.EmgStop();
                    this.AxisT.EmgStop();
                    PostAlarm((int)AlarmKeys.eDieTransferPlaceZNotSafety);
                    return -1;
                }
                if (nRet == -2)
                {
                    this.AxisX.EmgStop();
                    this.AxisY.EmgStop();
                    this.AxisT.EmgStop();
                    PostAlarm((int)AlarmKeys.eOutputFeederCylinderZNotSafety);
                    return -1;
                }

                Thread.Sleep(1);
            }
            return task.Result;
        }
        public Task<int> MoveToStageLoadPositionAsync()
        {
            return Task.Run(() =>
            {
                OnMoveToStageLoadPosition();
                return 0;
            });
        }
        private int OnMoveToStageLoadPosition(bool isFine = false)
        {
            int nRet = 0;
            nRet = IsInterlockStageOK();
            if (nRet == -1)
            {
                this.AxisX.EmgStop();
                this.AxisY.EmgStop();
                this.AxisT.EmgStop();
                PostAlarm((int)AlarmKeys.eDieTransferPlaceZNotSafety);
                return -1;
            }
            if (nRet == -2)
            {
                this.AxisX.EmgStop();
                this.AxisY.EmgStop();
                this.AxisT.EmgStop();
                PostAlarm((int)AlarmKeys.eOutputFeederCylinderZNotSafety);
                return -1;
            }
            return MoveTeachingPositionOnce((int)OutputStageConfig.TeachingPositionName.Loading, isFine);
        }

        public int MoveToStageUnloadPosition(bool isFine = false)
        {
            int nRet = 0;
            Task<int> task = MoveToStageUnloadPositionAsync();
            while (IsEndTask(task) == false)
            {
                nRet = IsInterlockStageOK();
                if (nRet == -1)
                {
                    this.AxisX.EmgStop();
                    this.AxisY.EmgStop();
                    this.AxisT.EmgStop();
                    PostAlarm((int)AlarmKeys.eDieTransferPlaceZNotSafety);
                    return -1;
                }
                if (nRet == -2)
                {
                    this.AxisX.EmgStop();
                    this.AxisY.EmgStop();
                    this.AxisT.EmgStop();
                    PostAlarm((int)AlarmKeys.eOutputFeederCylinderZNotSafety);
                    return -1;
                }
                Thread.Sleep(1);
            }
            return task.Result;
        }
        public Task<int> MoveToStageUnloadPositionAsync(bool isFine = false)
        {
            return Task.Run(() =>
            {
                OnMoveToStageUnloadPosition(isFine);
                return 0;
            });
        }
        private int OnMoveToStageUnloadPosition(bool isFine = false)
        {
            int nRet = 0;
            nRet = IsInterlockStageOK();
            if (nRet == -1)
            {
                this.AxisX.EmgStop();
                this.AxisY.EmgStop();
                this.AxisT.EmgStop();
                PostAlarm((int)AlarmKeys.eDieTransferPlaceZNotSafety);
                return -1;
            }
            if (nRet == -2)
            {
                this.AxisX.EmgStop();
                this.AxisY.EmgStop();
                this.AxisT.EmgStop();
                PostAlarm((int)AlarmKeys.eOutputFeederCylinderZNotSafety);
                return -1;
            }

            return MoveTeachingPositionOnce((int)OutputStageConfig.TeachingPositionName.Unloading, isFine);
        }

        public int MoveToTeachingPosition(string positionName, bool isFine)
        {
            if (string.IsNullOrWhiteSpace(positionName))
            {
                Log.Write(UnitName, nameof(MoveToTeachingPosition),
                        $"[TeachingMove] TeachingPositions에서 '{positionName}' 을 찾지 못했습니다.");
                return -1;
            }

            int result = 0;

            OutputStageConfig.TeachingPositionName en;
            if (Enum.TryParse(positionName, out en))
            {
                int selIndex = FindTeachingSelectionIndex(positionName);
                if (selIndex >= 0)
                {
                    result = MoveToTeachingPositionBySelectionIndex(selIndex, isFine);
                }
                else
                {
                    Log.Write(UnitName, nameof(MoveToTeachingPosition),
                        $"[TeachingMove] TeachingPositions에서 '{positionName}' index를 찾지 못했습니다.");
                    return -1;
                }
            }

            return result;
        }

        private int FindTeachingSelectionIndex(string positionName)
        {
            try
            {
                var list = GetTeachingList();
                if (list == null)
                    return -1;

                for (int i = 0; i < list.Count; i++)
                {
                    var tp = list[i];
                    if (tp != null && string.Equals(tp.Name, positionName, StringComparison.OrdinalIgnoreCase))
                        return i;
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
            return -1;
        }

        private IList<TeachingPosition> GetTeachingList()
        {
            // 1) Recipe 기반 TeachingRecipe가 있으면 그쪽 우선
            //    (Config 타입마다 TeachingRecipe 프로퍼티 존재 여부가 다르므로 reflection 사용)
            try
            {
                var cfg = Config;
                if (cfg != null)
                {
                    var prop = cfg.GetType().GetProperty("TeachingRecipe",
                        System.Reflection.BindingFlags.Instance |
                        System.Reflection.BindingFlags.Public |
                        System.Reflection.BindingFlags.NonPublic);

                    if (prop != null)
                    {
                        var teachingRecipe = prop.GetValue(cfg, null);
                        if (teachingRecipe != null)
                        {
                            // TeachingRecipe가 IHasTeachingPositions 구현한 경우가 많음
                            var has = teachingRecipe as QMC.LCP_280.Process.Unit.FormConfig.IHasTeachingPositions;
                            if (has != null && has.TeachingPositions != null)
                                return has.TeachingPositions;

                            // 혹시 인터페이스가 다르면 TeachingPositions 프로퍼티를 reflection으로 한번 더 시도
                            var tpProp = teachingRecipe.GetType().GetProperty("TeachingPositions",
                                System.Reflection.BindingFlags.Instance |
                                System.Reflection.BindingFlags.Public |
                                System.Reflection.BindingFlags.NonPublic);

                            var list = tpProp != null ? tpProp.GetValue(teachingRecipe, null) as IList<TeachingPosition> : null;
                            if (list != null)
                                return list;
                        }
                    }
                }
            }
            catch { /* ignore */ }

            // 2) 기본: Config.TeachingPositions
            return Config?.TeachingPositions ?? new List<TeachingPosition>();
        }

        #region seq signals
        //public bool RequestBin { get; set; }
        //public bool BinLoadingReady { get; set; }
        //public bool BinLoadingDone { get; set; }
        //public bool BinUnloadingDone { get; set; }
        //public bool BinUnloadingReady { get; set; }
        //public bool BinCompleteWorking { get; internal set; }
        //public bool RequestInputDie { get; internal set; }

        public MaterialWafer GetMaterialWafer()
        {
            var mat = GetMaterial();
            return mat as MaterialWafer;
        }

        public bool IsWorking()
        {
            bool bRet = false;
            try
            {
                var Bin = GetMaterialWafer();
                if (Config.IsSimulation == false && (Config.IsDryRun == false && IsDryRunEqp == false))
                {
                    if (IsRingPresent() == true)
                    {
                        if (Bin == null)
                        {
                            //알람 발생 해야함.
                            // 제품이 있는데 wafer 정보가 없으면 이상
                            //이건 다른곳에서 확인해야 하나? 이 함수에서는,,
                            //Log.Write(UnitName, "IsWorkCompleted", "Wafer present but wafer info is null");
                            return false;
                        }
                    }
                    else
                    {
                        if (Bin == null)
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    if (Bin == null)
                    {
                        return false;
                    }
                }

                if (Config.IsSimulation == false
                   && (Config.IsDryRun == false && IsDryRunEqp == false))
                {
                    if (IsRingPresent() == false)
                    {
                        return false;
                    }
                    else //제품이 있고 wafer상태가 Completed 가 아니면 작업중으로 간주
                    {
                        if (Bin.Presence == Material.MaterialPresence.Exist)
                        {
                            if (Bin.ProcessSatate != Material.MaterialProcessSatate.Completed)
                            {
                                bRet = true;
                            }
                        }
                    }
                }
                else
                {
                    if (Bin.Presence == Material.MaterialPresence.Exist)
                    {
                        if (Bin.ProcessSatate != Material.MaterialProcessSatate.Completed)
                        {
                            // 작업 중임.
                            bRet = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                bRet = false;
                Log.Write(ex);
            }
            return bRet;
        }
        #endregion

        #region Lifecycle
        public override int OnRun()
        {
            int ret = 0;
            if (this.RunUnitStatus == UnitStatus.Stopped ||
               this.RunUnitStatus == UnitStatus.Stopping ||
               this.RunUnitStatus == UnitStatus.Error ||
               this.RunUnitStatus == UnitStatus.CycleStop ||
               this.RunUnitStatus == UnitStatus.ManualRunning)
            {
                this.State = ProcessState.Stop;
                return 0;
            }
            
            if (ret != 0)
            {
                this.State = ProcessState.Stop;
                this.OnStop();
            }
            return ret;
        }
        protected override int OnStart()
        {
            return base.OnStart();
        }
        public override int OnStop()
        {
            int ret = 0;
            this.RunUnitStatus = UnitStatus.Stopped;
            base.OnStop();
            return ret;
        }
        protected override int OnRunReady() { return 0; }
        protected override int OnRunWork() { return 0; }
        protected override int OnRunComplete() { return 0; }
        #endregion

        protected override void OnMakeSequence()
        {
            base.OnMakeSequence();
            this.SequencePlayers.Add(LoadingBinPrepare);
            this.SequencePlayers.Add(LoadingBinComplete);
        }

        #region Seq 단위 동작 함수
        public int LoadingBinPrepare(bool isFine = false)
        {
            int nRet = 0;
            if(RunMode == UnitRunMode.Manual)
            {
                CurrentFunc = LoadingBinPrepare;
            }

            Log.Write(UnitName, "LoadingBinPrepare", "LoadingBinPrepare Start");

            // 이미 웨이퍼 존재하면 준비 단계 불필요 (바로 완료 단계 가능)
            if (Config.IsSimulation == false && (Config.IsDryRun == false && IsDryRunEqp == false))
            {
                if (IsRingPresent())
                {
                    Log.Write(UnitName, "LoadingPrep", "Bin already present -> Skip prepare");
                    return nRet;
                }
            }

            // 로딩 Teaching 이동
            nRet = MoveToStageLoadPosition(isFine);
            if (nRet != 0)
            {
                Log.Write(UnitName, "LoadingBinPrepare", "MoveToStageLoadPosition Fail");
                return -1;
            }
            
            bool bSimulation = Config.IsSimulation;
            // Clamp Back → Lift Down
            nRet = ClampBackward();
            if (nRet != 0)
            {
                Log.Write(UnitName, "LoadingBinPrepare", "ClampBackward Fail");
                return -1;
            }
            nRet = ClampLiftDown();
            if (nRet != 0)
            {
                Log.Write(UnitName, "LoadingBinPrepare", "ClampLiftDown Fail");
                return -1;
            }
            //Plate UP → 
            nRet = PlateUp();
            if (nRet != 0)
            {
                Log.Write(UnitName, "LoadingBinPrepare", "PlateUp Fail");
                return -1;
            }
            if (IsStop) { return 0; }
            Log.Write(UnitName, "LoadingBinPrepare", "LoadingBinPrepare End");
            return 0;
        }

        public int LoadingBinComplete(bool isFine = false)
        {
            int nRet = 0;

            if (RunMode == UnitRunMode.Manual)
            {
                CurrentFunc = LoadingBinComplete;
            }

            // 아직 Wafer 안 올라옴 → 대기
            bool bRtn = Config.IsSimulation;
            // 준비 안 되었으면 호출 순서 오류
            if (!IsRingPresent() && bRtn == false && (Config.IsDryRun == false && IsDryRunEqp == false))
            {
                Log.Write(UnitName, "LoadingComp", "Not prepared (call LoadingBinComplete first)");
                return -1;
            }

            
            if (IsRingPresent() || bRtn || (Config.IsDryRun || IsDryRunEqp))
            {
                Log.Write(UnitName, "LoadingComp", "Bin detected -> Completing");
                {
                    nRet = ClampLiftUp();
                    if(nRet != 0)
                    {
                        return -1;
                    }
                    nRet = ClampForward();
                    if (nRet != 0)
                    {
                        return -1;
                    }
                    nRet = PlateDown();
                    if (nRet != 0)
                    {
                        return -1;
                    }
                    SetVacuum(true);
                }
                // 센터 Teaching 이동
                nRet = MoveToStageCenterPosition(isFine);
                if (nRet != 0)
                {
                    Log.Write(this, "Fail: Move Load");
                    return nRet;
                }
                Log.Write(UnitName, "LoadingComp", "Done");
                return nRet;
            }
            else
            {
                AxisX?.EmgStop(); AxisY?.EmgStop(); AxisT?.EmgStop();
                // 우선 대기? // 신호 이상?
                PostAlarm((int)AlarmKeys.eNoBinDetected);
                Log.Write(UnitName, "LoadingComp", "No Bin detected");
                return -1;
            }
        }

        public int PrepareOutputStageUnloadingBin()
        {
            int nRet = 0;
            Log.Write(UnitName, "PrepareOutputStageUnloadingBin", "Start");
            if (!IsRingPresent())
            {
                Log.Write(UnitName, "PrepareOutputStageUnloadingBin", "No Bin");
                return -1;
            }

            nRet = MoveToStageUnloadPosition();
            if (nRet != 0)
            {
                Log.Write(this, "Fail: Move Unload");
                return -1;
            }

            nRet = ClampBackward();
            if (nRet != 0)
            {
                Log.Write(this, "Fail: ClampBackward");
                return -1;
            }
            nRet = ClampLiftDown();
            if (nRet != 0)
            {
                Log.Write(this, "Fail: ClampLiftDown");
                return -1;
            }
            nRet = PlateUp();
            if (nRet != 0)
            {
                Log.Write(this, "Fail: PlateUp");
                return -1;
            }
            SetVacuum(false);

            Log.Write(UnitName, "UnloadingPrep", "StageUnloadingReady = TRUE (Wait wafer pick)");
            return 0;
        }

        public int UnloadingBinComplete()
        {
            int nRtn = 0;

            if (IsRingPresent())
            {
                Log.Write(UnitName, "UnloadingComp", "Not prepared");
                return -1;
            }

            Log.Write(UnitName, "UnloadingComp", "Done");
            return nRtn;
        }
        public bool IsRingPresent()
        {
            bool bRtn = true;
            if (Config.IsSimulation || (Config.IsDryRun || IsDryRunEqp))
            {
                // 시뮬레이션: 실제 보유 머티리얼로 판단
                return this.GetMaterial() is MaterialWafer;
                //return true;
            }
            else if (!Ring0() || !Ring1())
            {
                var v = this.GetMaterial() as MaterialWafer;
                if(v != null)
                {
                    return v.Presence == Material.MaterialPresence.Exist;
                }
                    //Log.Write(UnitName, "IsRingPresent", $"Ring not present (R0={Ring0()}, R1={Ring1()})");
                return false;
            }

            return bRtn;
        }
        public bool IsPositionBinLoading()
        {
            var tp = TeachingPositions[(int)OutputStageConfig.TeachingPositionName.Loading];
            if (tp == null) 
                return false;
            return InPosTeaching(tp);
        }
        public bool IsPositionBinUnloading()
        {
            var tp = TeachingPositions[(int)OutputStageConfig.TeachingPositionName.Unloading];
            if (tp == null) return false;
            return InPosTeaching(tp);
        }
        public bool IsPositionBinCenter()
        {
            var tp = TeachingPositions[(int)OutputStageConfig.TeachingPositionName.CenterPoint];
            if (tp == null) return false;
            return InPosTeaching(tp);
        }
        public bool IsStageInterLockOK()
        {
            // 1) CenterPoint Teaching 확보
            var tp = Config.GetTeachingPosition(OutputStageConfig.TeachingPositionName.CenterPoint.ToString());
            if (tp == null || tp.AxisPositions == null)
            {
                Log.Write(UnitName, "MoveSafety", "CenterPoint teaching not found");
                return false;
            }

            // 2) Center 좌표 (OutputStage 축명은 BinStageX / BinStageY 사용)
            if (!tp.AxisPositions.TryGetValue(AxisNames.BinStageX, out var centerX) ||
                !tp.AxisPositions.TryGetValue(AxisNames.BinStageY, out var centerY))
            {
                Log.Write(UnitName, "MoveSafety", "CenterPoint BinStageX/BinStageY value missing");
                return false;
            }

            // 3) 사각형 Half Range
            double halfX = Config.SafeStageRectHalfWidthX;
            double halfY = Config.SafeStageRectHalfHeightY;
            if (halfX <= 0 || halfY <= 0)
            {
                Log.Write(UnitName, "MoveSafety",
                    $"Invalid rectangle half sizes. HalfX={halfX:F3}, HalfY={halfY:F3}");
                return false;
            }

            // 4) 현재 위치
            double curX = AxisX?.GetPosition() ?? centerX;
            double curY = AxisY?.GetPosition() ?? centerY;

            // 5) 사각형 내부 판정
            bool inRect =
                Math.Abs(curX - centerX) <= halfX &&
                Math.Abs(curY - centerY) <= halfY;

            if (inRect)
                return true;

            Log.Write(UnitName, "MoveSafety",
                $"Fail: Out of RECT safe window. Cur=({curX:F3},{curY:F3}) Center=({centerX:F3},{centerY:F3}) Half=({halfX:F3},{halfY:F3})");
            return false;

        }
        
        public void UpdateUI()
        {
            MaterialWafer materialWafer = GetMaterialWafer();
            EventUpdateUIWafer?.BeginInvoke(materialWafer, null, null);
        }

        public void MarkCurrentReservedMissing()
        {
            try
            {
                var wafer = GetMaterialWafer();
                if (wafer == null || wafer.Dies == null || wafer.Dies.Count == 0)
                    return;

                MaterialDie slot = null;
                lock (_reserveLock)
                    lock (wafer.Dies)
                    {
                        if (_reservedSlotKey == null)
                            return;

                        var key = _reservedSlotKey.Value;
                        slot = wafer.Dies.FirstOrDefault(d =>
                            d != null &&
                            Math.Abs(d.BinX - key.BinX) < 1e-9 &&
                            Math.Abs(d.BinY - key.BinY) < 1e-9);

                        if (slot == null)
                            return;

                        // [중요] 이미 처리된 칩이 아니면 상태 변경
                        if (slot.State == DieProcessState.Placed)
                            return;

                        // [중요] 실패여도 MapX/MapY는 InputStage 기준으로 갱신
                        double mx, my;
                        if (TryGetInputMapByIndex(slot.Index, out mx, out my))
                        {
                            slot.MapX = mx;
                            slot.MapY = my;
                        }

                        slot.State = DieProcessState.Rejected;
                        slot.Presence = Material.MaterialPresence.Exist;
                    }

                UpdateUI();
                Log.Write(UnitName, "MarkCurrentReservedMissing", "Marked reserved slot as Rejected (slot key based).");
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
        }

        public void PlaceDie(MaterialDie die)
        {
            var wafer = GetMaterialWafer();
            if (wafer == null || die == null)
                return;

            bool allPlacedOrRejected = false;
            MaterialDie slot = null;

            lock (_reserveLock)
            {
                lock (wafer.Dies)
                {
                    Log.Write(UnitName, "PlaceDie", "[Processing]01");
                    // 1) 예약 슬롯 찾기(반드시 BinX/BinY 기반)
                    if (_reservedSlotKey != null)
                    {
                        var key = _reservedSlotKey.Value;
                        slot = wafer.Dies.FirstOrDefault(d =>
                            d != null &&
                            Math.Abs(d.BinX - key.BinX) < 1e-9 &&
                            Math.Abs(d.BinY - key.BinY) < 1e-9);

                        Log.Write(UnitName, "PlaceDie", "[Processing] _reservedSlotKey != null");
                    }

                    // fallback
                    if (slot == null && _currentDie != null)
                        slot = _currentDie;

                    if (slot == null)
                        return;

                    // 2) Bin 슬롯 좌표는 슬롯 것 유지
                    die.BinX = slot.BinX;
                    die.BinY = slot.BinY;

                    // 3) [핵심] MapX/MapY는 무조건 InputStage 값으로 덮어쓰기
                    double mx, my;
                    if (TryGetInputMapByIndex(die.Index, out mx, out my))
                    {
                        slot.MapX = mx;
                        slot.MapY = my;
                        // incoming 객체도 일관성 맞추려면 같이 세팅(디버그/후속 처리)
                        die.MapX = mx;
                        die.MapY = my;
                    }
                    else
                    {
                        // Input을 못 읽었으면 최소한 slot의 Map을 유지 (로그 깨짐 방지)
                        die.MapX = slot.MapX;
                        die.MapY = slot.MapY;
                    }

                    // 4) 결과/측정/얼라인 데이터는 outTr die 기준으로 슬롯에 Copy (슬롯 객체 유지)
                    slot.Presence = Material.MaterialPresence.Exist;
                    slot.State = DieProcessState.Placed;
                    slot.TesterResult = die.TesterResult;
                    slot.MeasureValues = (die.MeasureValues != null)
                        ? new Dictionary<string, double>(die.MeasureValues)
                        : new Dictionary<string, double>();

                    // 4) 데이터 복사 [중요 수정]
                    // slot = die;  <-- 이 줄을 삭제해야 합니다! (참조가 끊김)
                    // 대신 die(소스)의 값을 slot(타겟)에 하나씩 복사해야 합니다.


                    slot.CenterX = die.CenterX;
                    slot.CenterY = die.CenterY;
                    slot.Angle = die.Angle;

                    slot.UnloadAlignOffsetX = die.UnloadAlignOffsetX;
                    slot.UnloadAlignOffsetY = die.UnloadAlignOffsetY;
                    slot.UnloadAlignOffsetT = die.UnloadAlignOffsetT;

                    slot.Rank = die.Rank;
                    slot.RankName = die.RankName;
                    slot.IsPass = die.IsPass;
                    slot.RejectReason = die.RejectReason;

                    slot.TargetWaferId = die.TargetWaferId;
                    slot.TargetSlot = die.TargetSlot;
                    slot.TargetChipIndex = die.TargetChipIndex;
                    slot.SourceWaferId = die.SourceWaferId;
                    slot.SocketIndex = die.SocketIndex;

                    _currentDie = slot;

                    //마지막 칩을 안하면.. 요거 안타넹. 
                    //그러면 wafer completed가 안되서 배출이 안됨. 
                    // 5) Completed 판정
                    if (wafer.Dies.All(d => d != null && d.State == DieProcessState.Placed))
                        wafer.ProcessSatate = Material.MaterialProcessSatate.Completed;
                    else if (wafer.Dies.All(d => d != null && (d.State == DieProcessState.Placed || d.State == DieProcessState.Rejected)))
                        wafer.ProcessSatate = Material.MaterialProcessSatate.Completed;

                    allPlacedOrRejected = (wafer.ProcessSatate == Material.MaterialProcessSatate.Completed);
                }
            }

            // ============================================================
            // [패치 핵심] 비동기 큐에 저장 요청 (Deep Copy는 내부에서 처리됨)
            // ============================================================

            // 1. CSV 및 로컬 로그 저장을 위한 데이터 할당 (필요하다면 유지, 시간이 짧다면)
            // AssignDataToMaterialObject는 단순 데이터 매핑이면 여기서 수행, 
            // 파일 쓰기가 포함되어 있으면 이것도 Manager로 이동 권장.
            int rc = AssignDataToMaterialObject(slot);
            if (rc != 0) 
            { 
                PostAlarm((int)AlarmKeys.eNotReadyToMeasure); 
                return; 
            }

            // 2. 비동기 저장 요청 (즉시 리턴됨 -> Takt Time 개선)
            try
            {
                var mgr = Equipment.Instance.ResultWriterManager;

                // [중요] 비동기 큐에 넣기 전에 Deep Copy (스냅샷) 생성
                // ResultWriterManager 내부에서 Clone을 안 한다면, 여기서 해야 데이터 꼬임을 완벽히 방지함.
                // 하지만 이미 ResultWriterManager.EnqueueDieSave 내부에서 처리하도록 설계했다면 slot 그대로 넘김.
                // 현재 코드 흐름상 slot은 lock 밖으로 나왔으므로, 
                // 큐에 넣는 순간의 데이터를 보존하기 위해 Clone을 해서 넘기는 것이 가장 안전합니다.
                MaterialDie snapshot = CloneDie(slot); // 아래 CloneDie 함수 참고
                mgr.EnqueueDieSave(snapshot, Equipment.Instance.Tester.ConditionSet);
            }
            catch (Exception ex)
            {
                Log.Write(UnitName, "PlaceDie_EnqueueError", ex.Message);
            }

            UpdateUI();
            OnDiePlaced(slot);
            RecordDiePlaceTakt(slot);
            try
            {
                var ctx = Equipment.Instance.SummaryContext;
                ctx.GetCurrentSummaryOrNull()?.AddOutCount();
            }
            catch (Exception ex)
            { Log.Write(ex); }
        }

        // [추가] Deep Copy를 위한 헬퍼 함수 (OutputStage 클래스 내부에 추가)
        private MaterialDie CloneDie(MaterialDie org)
        {
            if (org == null) return null;

            MaterialDie newDie = new MaterialDie();

            // 값 타입 및 String 복사
            newDie.Index = org.Index;
            newDie.MapX = org.MapX;
            newDie.MapY = org.MapY;
            newDie.BinX = org.BinX;
            newDie.BinY = org.BinY;
            newDie.State = org.State;
            newDie.Rank = org.Rank;
            newDie.RankName = org.RankName;
            newDie.IsPass = org.IsPass;
            newDie.SourceWaferId = org.SourceWaferId;
            newDie.TargetWaferId = org.TargetWaferId;
            newDie.SocketIndex = org.SocketIndex;

            // 참조 타입 복사 (Dictionary, Object 등)
            if (org.MeasureValues != null)
                newDie.MeasureValues = new Dictionary<string, double>(org.MeasureValues);

            // TesterResult는 보통 참조만 해도 되지만, 내용이 변한다면 Deep Copy 필요 (여기선 참조로 둠)
            newDie.TesterResult = org.TesterResult;

            return newDie;
        }
        #endregion

        /// <summary>
        /// NextDie(Processing 상태에서 Mapped + Presence == Exist)가 존재하는지 여부만 확인.
        /// 내부 상태 변경(Completed 전환 등) 없이 순수 조회만 수행.
        /// </summary>
        /// 
        public bool HasNextDie()
        {
            var wafer = GetMaterialWafer();
            if (wafer == null) 
                return false;

            lock (wafer.Dies)
            {
                // 맵이 없으면 없음
                var dies = wafer.Dies;
                if (dies == null || dies.Count == 0)
                    return false;

                // TryReserveNextEmptyBin과 동일 기준: Presence 무시, State만 사용
                bool has = dies.Any(d =>
                    d != null
                     && d.State != DieProcessState.Placed
                     && d.State != DieProcessState.Rejected
                     && d.State != DieProcessState.Skip);

                // 상태 정합성 보정
                if (has == false)
                {
                    if (wafer.ProcessSatate != Material.MaterialProcessSatate.Completed)
                        wafer.ProcessSatate = Material.MaterialProcessSatate.Completed;
                }
                else
                {
                    if (wafer.ProcessSatate == Material.MaterialProcessSatate.Completed)
                        wafer.ProcessSatate = Material.MaterialProcessSatate.Processing;
                }
                return has;
            }
        }

        // OutputStage 클래스 멤버 필드 근처에 추가
        private (double BinX, double BinY)? _reservedSlotKey;

        // 다음 빈 Bin 예약: 정렬 제거, 리스트 순서(경로 순서) 사용
        public bool TryReserveNextEmptyBin(MaterialDie die, out double binX, out double binY, out double dT, out MaterialDie slot)
        {
            binX = binY = dT = -1;
            slot = null;

            var wafer = this.GetMaterialWafer();
            if (wafer == null || wafer.Dies == null || wafer.Dies.Count == 0)
            {
                return false;
            }

            var recipe = Equipment.Instance.EquipmentRecipe.CurrentRecipe;
            bool useSameAsWafer = recipe.UseSameAsWafer;

            lock (wafer.Dies)
            {
                Func<MaterialDie, bool> isUnplaced = d =>
                d != null &&
                d.State != DieProcessState.Placed
                && d.State != DieProcessState.Rejected
                && d.State != DieProcessState.Skip;

                var dieRotary = RotaryUnit?.GetUnloadSocketMaterial();
                var dieOutTr = OutputDieTransfer.GetMaterial() as MaterialDie;
                MaterialDie next = null;

                if (useSameAsWafer)
                {
                    // 1) OutputDieTransfer가 들고 있는 다이 우선
                    if (dieOutTr != null)
                    {
                        next = wafer.Dies.FirstOrDefault(d => isUnplaced(d) && d.Index == dieOutTr.Index);
                    }

                    // 2) Rotary 소켓 다이
                    if (next == null && dieRotary != null)
                    {
                        next = wafer.Dies.FirstOrDefault(d => isUnplaced(d) && d.Index == dieRotary.Index);
                    }

                    // 3) 그 외 첫 미배치
                    if (next == null)
                        next = wafer.Dies.FirstOrDefault(isUnplaced);
                }
                else
                {
                    // [Case 2] useSameAsWafer == false
                    // 순차적으로 빈 곳(Unplaced)을 찾아서 채움 (기존 로직 중 순차 검색 부분)
                    next = wafer.Dies.FirstOrDefault(isUnplaced);
                }

                if (next == null)
                    return false;

                _currentDie = next;
                // [FIX] 예약 슬롯을 Index가 아니라 Bin 좌표로 고정
                _reservedSlotKey = (next.BinX, next.BinY);

                double baseT = GetTeahcingPosCenterT();
                double dx = 0;
                double dy = 0;
                double dt = baseT;
                
                //각도
                int dirX = -1;
                int dirY = 1;
                int dirT = 1;
                if (die != null)
                {
                    dx = die.UnloadAlignOffsetX * dirX;
                    dy = die.UnloadAlignOffsetY * dirY;
                    dt = die.UnloadAlignOffsetT * dirT;
                }

                double dStagePosT = AxisT?.GetPosition() ?? 0.0;

                // 지정 Bin 위치로 XY 이동
                binX = next.BinX;
                binY = next.BinY;
                var (tx, ty) = GetBinWorldPosition(binX, binY);

                Log.Write(UnitName, "TryReserveNextEmptyBin",
                    $"Origin Index={next.Index}, Bin=({binX},{binY}), State={next.State}, Presence={next.Presence}");

                if (Config.TCorrectionMode)
                {
                    if (this.linkTypeXYTStageCorrection == null)
                    {
                        LoadTCorrectionCsvAndApply(Config.TCorrectionFile);
                    }
                }

                if (Config.TCorrectionMode == true && this.linkTypeXYTStageCorrection != null)
                {
                    XyCoordinate xyCoordinateTarget = new XyCoordinate(tx, ty);
                    XyCoordinate xyCoordinateVision = new XyCoordinate(dx, dy);

                    this.linkTypeXYTStageCorrection.GetCorrectionPoint(dt, xyCoordinateTarget, xyCoordinateVision, out var pointD, out double t);
                    binX = pointD.X;
                    binY = pointD.Y;
                    dT = t;
                }
                else
                {
                    binX = tx + dx;
                    binY = ty + dy;
                    dT = baseT; //0.3;//Todo :티칭 보지션에서 가져와서 넣어주세요 ;
                }
                slot = next;

                bool matchedOutTr = (dieOutTr != null && next.Index == dieOutTr.Index);
                bool matchedRotary = (dieRotary != null && next.Index == dieRotary.Index);

                Log.Write(UnitName, "TryReserveNextEmptyBin",
                    $"Reserved Index={next.Index}, Bin=({binX},{binY}), State={next.State}, Presence={next.Presence}, MatchedByOutTr={matchedOutTr}, MatchedByRotary={matchedRotary}");
            }
            return true;
        }

        public (double x, double y) GetBinWorldPosition(double binX, double binY)
        {
            var eq = Equipment.Instance;
            var recipe = eq.EquipmentRecipe.CurrentRecipe;

            // 1) 피치 결정: ChipWidth/Height 우선, 없으면 BinPitch로 폴백
            //double pitchX = (recipe.WChipPitchX > 0) ? recipe.WChipPitchX :
            //                (recipe.BinPitchXmm > 0) ? recipe.BinPitchXmm : 1.0;
            //double pitchY = (recipe.WChipPitchY > 0) ? recipe.WChipPitchY :
            //                (recipe.BinPitchYmm > 0) ? recipe.BinPitchYmm : 1.0;
            double pitchX = (recipe.BinPitchXmm > -1) ? recipe.BinPitchXmm : 1.0;
            double pitchY = (recipe.BinPitchYmm > -1) ? recipe.BinPitchYmm : 1.0;

            // 2) CenterPoint Teaching (월드 좌표 원점 역할)
            var (centerX, centerY, _) = Config.GetPositionWithOffset(OutputStageConfig.TeachingPositionName.CenterPoint.ToString());

            // 3) 중심 인덱스 계산
            double indexCenterX, indexCenterY;

            lock (_indexCenterLock)
            {
                if (!_indexCenterInitialized)
                {
                    var wafer = GetMaterialWafer();
                    if (wafer?.Dies != null && wafer.Dies.Count > 0)
                    {
                        lock (wafer.Dies)
                        {
                            // 맵 데이터로부터 격자 범위를 구해 중심 인덱스 산출
                            // (주의) BinX/BinY가 "인덱스"라는 전제. 런 중 값이 바뀌면 스냅샷으로 고정해 흔들림 방지
                            int minIdxX = (int)Math.Round(wafer.Dies.Min(d => d.BinX));
                            int maxIdxX = (int)Math.Round(wafer.Dies.Max(d => d.BinX));
                            int minIdxY = (int)Math.Round(wafer.Dies.Min(d => d.BinY));
                            int maxIdxY = (int)Math.Round(wafer.Dies.Max(d => d.BinY));

                            _indexCenterXSnapshot = (minIdxX + maxIdxX) / 2.0;
                            _indexCenterYSnapshot = (minIdxY + maxIdxY) / 2.0;
                            _indexCenterInitialized = true;

                            Log.Write(UnitName, "GetBinWorldPosition",
                                $"IndexCenter snapshot initialized: X={_indexCenterXSnapshot:F3}, Y={_indexCenterYSnapshot:F3}, " +
                                $"MinMaxX=({minIdxX},{maxIdxX}), MinMaxY=({minIdxY},{maxIdxY})");

                            //기존
                            //indexCenterX = (minIdxX + maxIdxX) / 2.0;
                            //indexCenterY = (minIdxY + maxIdxY) / 2.0;
                        }
                    }
                    else
                    {
                        // 맵 데이터가 없으면 웨이퍼 지름 + 피치로 격자 개수를 추정
                        double diameterMm = (recipe.WaferDiameter > 0) ? recipe.WaferDiameter : 0.0;
                        double marginMm = 0.0; // 필요 시 설정으로 분리 가능
                        double radiusMm = Math.Max(0.0, diameterMm / 2.0 - marginMm);

                        int halfCellsX = (pitchX > 0) ? (int)Math.Floor(radiusMm / pitchX) : 0;
                        int halfCellsY = (pitchY > 0) ? (int)Math.Floor(radiusMm / pitchY) : 0;

                        int cntX = Math.Max(1, halfCellsX * 2 + 1);
                        int cntY = Math.Max(1, halfCellsY * 2 + 1);

                        _indexCenterXSnapshot = (cntX - 1) / 2.0;
                        _indexCenterYSnapshot = (cntY - 1) / 2.0;
                        _indexCenterInitialized = true;

                        Log.Write(UnitName, "GetBinWorldPosition",
                            $"IndexCenter snapshot initialized (fallback): X={_indexCenterXSnapshot:F3}, Y={_indexCenterYSnapshot:F3}, cntX={cntX}, cntY={cntY}");

                        //기존
                        //indexCenterX = (cntX - 1) / 2.0;
                        //indexCenterY = (cntY - 1) / 2.0;
                    }
                }

                indexCenterX = _indexCenterXSnapshot;
                indexCenterY = _indexCenterYSnapshot;
            }

            // 4) 인덱스 오프셋 → 월드(mm) 오프셋
            double offsetX = (binX - indexCenterX) * pitchX;
            double offsetY = (binY - indexCenterY) * pitchY;

            // 5) 최종 월드 좌표
            double targetX = centerX + offsetX;
            double targetY = centerY + offsetY;
            return (targetX, targetY);
        }

        public int MoveToBinPosition(double binX, double binY, double dT, bool isFine = false)
        {
            // 지정 Bin 위치로 XY 이동
            //var (tx, ty) = GetBinWorldPosition(binX, binY);

            int rc = 0;
            List<Task<int>> tasks = new List<Task<int>>();
            Task<int> t = MoveAxisPositionOneAsync(AxisX, binX, isFine);
            tasks.Add(t);
            t = MoveAxisPositionOneAsync(AxisY, binY, isFine);
            tasks.Add(t);
            t = MoveAxisPositionOneAsync(AxisT, dT, isFine);
            
            tasks.Add(t);
            foreach(var v in tasks)
            {
                v.Wait();
                if(v.Result != 0)
                {
                    rc = v.Result;
                }
            }

            if (rc != 0) 
                return -1;

            return 0;
        }

        public int MoveToPositionXY(double dX, double dY, bool isFine = true)
        {
            double tx = dX;// + (AxisX?.GetPosition() ?? 0.0);
            double ty = dY;// + (AxisY?.GetPosition() ?? 0.0);
            
            int rc = 0;
            if (AxisX != null) rc |= MoveAxisPositionOne(AxisX, tx, isFine);
            if (AxisY != null) rc |= MoveAxisPositionOne(AxisY, ty, isFine);

            if (rc != 0)
                return -1;

            return 0;
        }

        public int MoveToPositionT(double dT, bool isFine = true)
        {
            double tT = dT + (AxisT?.GetPosition() ?? 0.0);

            int rc = 0;
            if (AxisT != null) rc |= MoveAxisPositionOne(AxisT, tT, isFine);

            if (rc != 0)
                return -1;

            return 0;
        }



        #region Update UI
        public void OnDiePlaced(MaterialDie die)
        {
            try
            {
                DiePlaced?.Invoke(this, new DiePlacedEventArgs(die));
            }
            catch (Exception ex)
            {
                Log.Write(UnitName, "[OnDiePlaced] " + ex.Message);
            }
        }

        public bool CanPlaceDie()
        {
            bool bRet = true;
            bool bMoveDoneAxisX = false;
            bool bMoveDoneAxisY = false;
            bool bMoveDoneAxisT = false;
            bool bHasnexDie = false;
            bMoveDoneAxisX = this.AxisX.IsMoveDone();
            bMoveDoneAxisY = this.AxisY.IsMoveDone();
            bMoveDoneAxisT = this.AxisT.IsMoveDone();
            bHasnexDie = HasNextDie();
            if(bMoveDoneAxisX && bMoveDoneAxisY && bMoveDoneAxisT && bHasnexDie)
            {
                bRet = true;
            }
            else
            {
                bRet = false;
            }
            return bRet;
        }


        // === Cylinder 고레벨 제어(완료 대기 포함) ===
        public int PlateUp()
        {
            SetClampPlate(true);
            int r = WaitPlateStateOrAlarm(expectUp: true);
            if (r != 0)
            {
                AxisX?.EmgStop(); AxisY?.EmgStop(); AxisT?.EmgStop();
                Log.Write(this, "PlateUp Failed");
            }
            return r;
        }

        public int PlateDown()
        {
            SetClampPlate(false);
            int r = WaitPlateStateOrAlarm(expectUp: false);
            if (r != 0)
            {
                AxisX?.EmgStop(); AxisY?.EmgStop(); AxisT?.EmgStop();
                Log.Write(this, "PlateDown Failed");
            }
            return r;
        }

        public int ClampLiftUp()
        {
            SetClampLift(true);
            int r = WaitClampLiftStateOrAlarm(expectUp: true);
            if (r != 0)
            {
                AxisX?.EmgStop();
                AxisY?.EmgStop();
                AxisT?.EmgStop();
                PostAlarm((int)AlarmKeys.eClampLift);
                Log.Write(this, "ClampLiftUp Failed");
            }
            return r;
        }

        public int ClampLiftDown()
        {
            // 인터락은 SetClampLift(false) 내부에서 IsClampBwd() 확인
            bool issued = SetClampLift(false);
            if (!issued && !(Config.IsSimulation || (Config.IsDryRun || IsDryRunEqp)))
            {
                AxisX?.EmgStop();
                AxisY?.EmgStop();
                AxisT?.EmgStop();
                PostAlarm((int)AlarmKeys.eClampLift);
                Log.Write(this, "ClampLiftDown Command Rejected (Interlock)");
                return -1;
            }

            int r = WaitClampLiftStateOrAlarm(expectUp: false);
            if (r != 0)
            {
                AxisX?.EmgStop();
                AxisY?.EmgStop();
                AxisT?.EmgStop();
                PostAlarm((int)AlarmKeys.eClampLift);
                Log.Write(this, "ClampLiftDown Failed");
            }
            return r;
        }

        public int ClampForward()
        {
            // 인터락은 SetClampFB(true) 내부에서 IsClampLiftUp() 확인
            bool issued = SetClampFB(true);
            if (!issued && !(Config.IsSimulation || (Config.IsDryRun || IsDryRunEqp)))
            {
                AxisX?.EmgStop();
                AxisY?.EmgStop();
                AxisT?.EmgStop();
                PostAlarm((int)AlarmKeys.eClampFB);
                Log.Write(this, "ClampForward Command Rejected (Interlock)");
                return -1;
            }

            int r = WaitClampFBStateOrAlarm(expectFwd: true);
            if (r != 0)
            {
                AxisX?.EmgStop();
                AxisY?.EmgStop();
                AxisT?.EmgStop();
                PostAlarm((int)AlarmKeys.eClampFB);
                Log.Write(this, "ClampForward Failed");
            }
            return r;
        }

        public int ClampBackward()
        {
            // 인터락은 SetClampFB(false) 내부에서 IsClampLiftUp() 확인
            bool issued = SetClampFB(false);
            if (!issued && !(Config.IsSimulation || (Config.IsDryRun || IsDryRunEqp)))
            {
                AxisX?.EmgStop();
                AxisY?.EmgStop();
                AxisT?.EmgStop();
                PostAlarm((int)AlarmKeys.eClampFB);
                Log.Write(this, "ClampBackward Command Rejected (Interlock)");
                return -1;
            }

            int r = WaitClampFBStateOrAlarm(expectFwd: false);
            if (r != 0)
            {
                AxisX?.EmgStop();
                AxisY?.EmgStop();
                AxisT?.EmgStop();
                PostAlarm((int)AlarmKeys.eClampFB);
                Log.Write(this, "ClampBackward Failed");
            }
            return r;
        }

        #endregion

        public void ResetForNewRun(bool moveToSafeReady = true, bool clearWafer = true, bool clearOffsets = true)
        {
            int nRet = 0;

            ResetDiePlaceTakt();

            // [ADD] Bin center index snapshot reset
            lock (_indexCenterLock)
            {
                _indexCenterInitialized = false;
                _indexCenterXSnapshot = 0.0;
                _indexCenterYSnapshot = 0.0;
            }

            // 1) 런타임/시퀀스 플래그 초기화
            _currentDie = null;
            // 2) 비전 리소스 정리(선택)
            try
            {
                OutStageCamera?.LatestImage?.Dispose();
                if (OutStageCamera != null) 
                    OutStageCamera.LatestImage = null;
            }
            catch (Exception ex)
            {
                Log.Write(UnitName, $"[ResetForNewRun] Clear camera image failed: {ex.Message}");
            }

            // 3) 머티리얼 정리/초기화(선택)
            try
            {
                var wafer = GetMaterialWafer();
                if (clearWafer)
                {
                    SetMaterial(null);
                    UpdateUI();
                }
                else if (wafer != null)
                {
                    wafer.ProcessSatate = Material.MaterialProcessSatate.Unknown;
                }
            }
            catch (Exception ex)
            {
                Log.Write(UnitName, $"[ResetForNewRun] Material reset failed: {ex.Message}");
            }

            // 4) 오프셋 초기화(선택)
            if (clearOffsets)
            {
                try
                {
                    Config.SetOffset(OutputStageConfig.TeachingPositionName.CenterPoint.ToString(), 0, 0, 0);
                    Config.SetOffset(OutputStageConfig.TeachingPositionName.Loading.ToString(), 0, 0, 0);
                    Config.SetOffset(OutputStageConfig.TeachingPositionName.Unloading.ToString(), 0, 0, 0);
                    Config.SetOffset(OutputStageConfig.TeachingPositionName.Ready.ToString(), 0, 0, 0);
                }
                catch (Exception ex)
                {
                    Log.Write(UnitName, $"[ResetForNewRun] Clear offsets failed: {ex.Message}");
                }
            }

            // 5) IO 안전 상태 복귀
            try
            {
                if (!(Config.IsSimulation || (Config.IsDryRun || IsDryRunEqp)))
                {
                    // 순서: 클램프 후퇴 → 리프트 다운 → 플레이트 다운 → 진공 OFF
                    nRet = ClampBackward();
                    if (nRet != 0)
                    {
                        Log.Write(UnitName, "ResetForNewRun", "Fail: ClampBackward");
                        return;
                    }
                    nRet = ClampLiftDown();
                    if (nRet != 0)
                    {
                        Log.Write(UnitName, "ResetForNewRun", "Fail: ClampLiftDown");
                        return;
                    }

                    if(this.IsRingPresent() || this.IsVacuumOn())
                    {
                        nRet = PlateUp();
                        if (nRet != 0)
                        {
                            Log.Write(UnitName, "ResetForNewRun", "Fail: PlateUp");
                            return;
                        }
                    }
                    else
                    {
                        nRet = PlateDown();
                        if (nRet != 0)
                        {
                            Log.Write(UnitName, "ResetForNewRun", "Fail: PlateDown");
                            return;
                        }
                    }
                    SetVacuum(false);
                }
            }
            catch (Exception ex)
            {
                Log.Write(UnitName, $"[ResetForNewRun] IO safe-state failed: {ex.Message}");
            }
        }

        //OutputStage 클래스 내부(예: PlaceDie 위쪽 또는 ResetForNewRun 아래 편한 위치)
        public int CloneDieMapFromInputStage(InputStage inputStage,
                                             bool rotate180 = false,
                                             bool swapXY = false,
                                             bool mirrorX = false,
                                             bool mirrorY = false)
        {
            try
            {
                var src = inputStage?.GetMaterialWafer();
                if (src == null || src.Dies == null || src.Dies.Count == 0) return -1;

                var dst = GetMaterialWafer();
                if (dst == null)
                {
                    dst = new MaterialWafer();
                    SetMaterial(dst);
                }
                lock (src.Dies)
                {
                    lock (dst.Dies)
                    {
                        // 기본 메타 복사
                        dst.WaferId = string.IsNullOrWhiteSpace(dst.WaferId) ? $"QMC_BIN_{src.WaferId}" : dst.WaferId;
                        dst.CarrierId = src.CarrierId;
                        dst.WaferDate = src.WaferDate;
                        dst.Presence = Material.MaterialPresence.Exist;
                        dst.ProcessSatate = Material.MaterialProcessSatate.Processing;

                        var list = new List<MaterialDie>(src.Dies.Count);
                        foreach (var s in src.Dies)
                        {
                            if (s == null) 
                                continue;

                            // 좌표 변환(필요 시)
                            double mx = s.MapX, my = s.MapY;
                            if (rotate180)
                            {
                                mx = -mx; my = -my;
                            }
                            if (mirrorX) mx = -mx;
                            if (mirrorY) my = -my;

                            if (swapXY)
                            {
                                var tmp = mx; mx = my; my = tmp;
                            }

                            // 복제: Index/Name 보존, 상태는 Output 목적에 맞게 초기화
                            var d = new MaterialDie
                            {
                                Index = s.Index,                   // 보존
                                Name = s.Name,                     // 보존
                                MapX = (int)mx,
                                MapY = (int)my,
                                // Output Bin 좌표는 내부에서 변환 사용 시 따로 설정 가능(없으면 MapX/Y 기반 사용)
                                BinX = mx,
                                BinY = my,

                                // Output 시작 상태: 아직 놓지 않음
                                Presence = Material.MaterialPresence.NotExist,
                                State = DieProcessState.None,

                                SourceWaferId = dst.WaferId
                            };

                            list.Add(d);
                        }

                        // Index는 보존하되, 정렬(순회)은 별도 루틴에서 수행
                        // 리스트는 Index 오름차순으로 정렬하여 보관(선택)
                        dst.Dies = list.OrderBy(d => d.Index).ToList();

                        // [ADD] 맵을 새로 클론하면 중심 인덱스 스냅샷을 다시 계산하도록 리셋
                        lock (_indexCenterLock)
                        {
                            _indexCenterInitialized = false;
                            _indexCenterXSnapshot = 0.0;
                            _indexCenterYSnapshot = 0.0;
                        }

                        UpdateUI();
                        Log.Write(UnitName, "CloneDieMapFromInputStage",
                            $"Cloned {dst.Dies.Count} dies from '{inputStage.UnitName}' (preserved Index/Name)");
                    }
                }
                return 0;
            }
            catch (Exception ex)
            {
                Log.Write(UnitName, "CloneDieMapFromInputStage", ex.Message);
                return -1;
            }

        }

        public int AssignDataToMaterialObject(MaterialDie materialDie)
        {
            if (materialDie == null)
            {
                Log.Write(UnitName, "AssignDataToMaterialObject", "materialDie Info. Fail.");
                return -1;
            }

            PKGTesterResult result = materialDie.TesterResult;
            // 임시 테스트 코드 -----
            string logDir = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
            if (!System.IO.Directory.Exists(logDir))
                System.IO.Directory.CreateDirectory(logDir);

            //var wafer = Rotary.GetMaterial() as MaterialDie;
            //var die = Rotary.GetProbeSocketMaterial();
            string waferID = "";
            if (materialDie != null)
            {
                waferID = materialDie.SourceWaferId;
                Log.Write(UnitName, $"Index_{materialDie.Index}, WaferID_{materialDie.SourceWaferId}, " +
                    $"BinID_{materialDie.TargetWaferId}, State_{materialDie.State.ToString()}");
            }
            else
            {
                waferID = "None";
                Log.Write(UnitName, "AssignDataToMaterialObject", "die.SourceWaferId Fail");
            }
            int nSocketIndex = materialDie.SocketIndex;

            string logFile = System.IO.Path.Combine(logDir, $"{waferID}_{DateTime.Now:yyyyMMdd}.csv");
            bool fileExists = System.IO.File.Exists(logFile);
            // 신규 파일일 때만 StrainGage 컬럼을 헤더에 추가(기존 파일 헤더 불일치 방지)
            var sgKeys = new List<string>();
            ////if (!fileExists && materialDie != null && materialDie.MeasureValues != null)
            //{
                sgKeys = materialDie.MeasureValues.Keys
                          .Where(k => k.StartsWith("SG", StringComparison.OrdinalIgnoreCase))
                          .OrderBy(k => k, StringComparer.OrdinalIgnoreCase)
                          .ToList();
            //}

            using (var writer = new System.IO.StreamWriter(logFile, true, System.Text.Encoding.UTF8))
            {
                // 파일이 없으면 헤더 추가
                if (!fileExists)
                {
                    writer.Write("Time,");
                    writer.Write("SocketNo,");
                    writer.Write("DieNo,");
                    writer.Write("DiePosX,");
                    writer.Write("DiePosY,");

                    // Bin / Rank 컬럼
                    writer.Write("BinNo,");
                    writer.Write("BinType,");
                    writer.Write("BinLabel,");

                    foreach (var item in result.Items)
                    {
                        writer.Write($"{item.Key},");
                    }

                    // StrainGage 헤더(있을 때만)
                    //foreach (var key in sgKeys)
                    //{
                    //    writer.Write($"{key},");
                    //}

                    writer.WriteLine();
                }

                // 데이터 행 추가 +1하지말자.
                writer.Write($"{DateTime.Now:yyyy-MM-dd HH:mm:ss:fff},");
                writer.Write($"{nSocketIndex + 1},");
                writer.Write($"{materialDie.Index + 1},");
                writer.Write($"{materialDie.MapX},");
                writer.Write($"{materialDie.MapY},");
                //writer.Write($"{materialDie.MapX * -1},");
                //writer.Write($"{materialDie.MapY * -1},");

                // Bin / Rank 값
                var binResult = result.BinningResult;

                // BinNo / BinLabel
                writer.Write($"{binResult?.BinNo},");
                writer.Write($"{binResult?.BinType},");
                writer.Write($"{binResult?.BinLabel},");

                //foreach (var item in result.Items)
                //{
                //    writer.Write($"{item.Value},");
                //}

                // 신규 파일 헤더에 StrainGage 키를 넣은 경우에만 값도 함께 출력
                //if (sgKeys.Count > 0 && materialDie != null && materialDie.MeasureValues != null)
                {
                    foreach (var key in sgKeys)
                    {
                        double v;
                        materialDie.MeasureValues.TryGetValue(key, out v);
                        writer.Write($"{v},");
                    }
                }

                writer.WriteLine();
            }
            // ---------------------
            return 0;
        }

        public bool IsStatus_AlignDoneXY { get; set; }
        public bool IsAlignResult { get; set; }
        public double dLastFoundX { get; set; }
        public double dLastFoundY { get; set; }
        public double dLastFoundAngle { get; private set; }


        public int AlignXY(bool bFineSpeed = false)
        {
            int nRet = 0;
            IsStatus_AlignDoneXY = false;
            IsAlignResult = false;
            dLastFoundX = 0.0;
            dLastFoundY = 0.0;
            dLastFoundAngle = 0.0;

            if (Config.IsSimulation || (this.Config.IsDryRun || IsDryRunEqp))
            {
                IsAlignResult = true;
                IsStatus_AlignDoneXY = true;
                dLastFoundX = 0.0;
                dLastFoundY = 0.0;
                dLastFoundAngle = 0.0;
                return 0;
            }
            try
            {
                VisionImage img = null;
                double dX = 0;
                double dY = 0;
                double dAngle = 0;
                OutStageCamera.SuspendedImageDisplay = true;
                OutStageCamera.GrabSync(out img);
                var result = PmRunner.Search(img);
                if (result != null && result.Success && result.Matches != null && result.Matches.Count > 0)
                {
                    int repIdx = 2;// (result.ReferenceIndex >= 0 && result.ReferenceIndex < result.Matches.Count) ? result.ReferenceIndex : 0;
                    RaiseMarks(img, result.Matches.ToArray(), repIdx);
                    OutStageCamera.SuspendedImageDisplay = false;
                }

                if (result.Success)
                {
                    IsAlignResult = true;
                    dX = result.X;
                    dY = result.Y;
                    dAngle = result.R;
                }
                else
                {
                    IsAlignResult = false;
                    dX = 0;
                    dY = 0;
                    dAngle = 0;
                }

                PointD pt = GetPixelToMmScale(dX, dY);
                dLastFoundX = pt.X;
                dLastFoundY = pt.Y;
                dLastFoundAngle = dAngle;
                Log.Write(UnitName, "AlignXY",
                    $"VisionX={dLastFoundX:F4}, " +
                    $"VisionY={dLastFoundY:F4}, " +
                    $"VisionAngle={dLastFoundAngle:F4}");
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                return -1;
            }
            finally
            {
                IsStatus_AlignDoneXY = true;
            }
            return nRet;
        }

        PointD GetPixelToMmScale(double dX, double dY)
        {
            double mmPerPixelX = (dX - OutStageCamera.CameraConfig.Resolution.Width / 2) * OutStageCamera.CameraConfig.Scale.X;
            double mmPerPixelY = (dY - OutStageCamera.CameraConfig.Resolution.Height / 2) * OutStageCamera.CameraConfig.Scale.Y;
            return new PointD(mmPerPixelX, mmPerPixelY);
        }

        private void RaiseMarks(VisionImage img,
                            QMC.Common.Vision.Tools.PatternMatchingResult.PatternMatchingResultValue[] matches,
                            int representativeIndex)
        {
            int trainW = 0, trainH = 0;
            try
            {
                var ti = PmRunner?.Parameters?.TrainImages?
                         .FirstOrDefault(t => t?.Header != null && t.Header.Width > 0 && t.Header.Height > 0);
                if (ti != null) { trainW = ti.Header.Width; trainH = ti.Header.Height; }
            }
            catch { }

            var e = new PatternMarksFoundEventArgs
            {
                Image = img,
                RepresentativeIndex = representativeIndex
            };
            foreach (var m in matches)
            {
                e.Marks.Add(new PatternMatchInfo
                {
                    X = m.X,
                    Y = m.Y,
                    AngleDeg = m.R,
                    Score = m.Score,
                    TrainW = trainW,
                    TrainH = trainH
                });
            }
            try { MarksFound?.Invoke(this, e); } catch { }
        }



        

        // T 보정 스캔 결과 레코드
        public sealed class TCorrectionRecord
        {
            public int MarkIndex { get; set; }
            public double AngleDeg { get; set; }          // 기준 T + 상대각
            public double StageX { get; set; }
            public double StageY { get; set; }
            public double StageT { get; set; }            // 실제 스테이지 T 절대값
            public double FoundOffsetX { get; set; }      // Vision 결과(mm) 기준 중심 대비 오프셋
            public double FoundOffsetY { get; set; }
            public double FoundAngle { get; set; }        // Vision이 찾은 패턴 각도(AlignXY에서 dLastFoundAngle)
            public bool AlignSuccess { get; set; }
        }

        // T 보정 전체 결과 (마크1~4)
        public List<TCorrectionRecord> TCorrectionResults { get; } = new List<TCorrectionRecord>();

        // 스캔 샘플 캡쳐 시 UI 갱신용 이벤트
        public event EventHandler<TCorrectionRecord> TCorrectionSampleCaptured;

        public double GetTeahcingPosCenterT()
        {
            //1) T축 보정 시작
            // 기준 T: CenterPoint Teaching의 T 사용
            var tpCenter = Config.GetTeachingPosition(OutputStageConfig.TeachingPositionName.CenterPoint.ToString());
            double baseT = 0.0;
            if (tpCenter?.AxisPositions != null &&
                tpCenter.AxisPositions.TryGetValue(AxisNames.BinStageT, out var centerT))
            {
                baseT = centerT;
            }
            //else
            //{
            //    baseT = AxisT.GetPosition();
            //}

            return baseT;
        }

        public int StartTCorrection(IReadOnlyList<(double X, double Y)> marks,
            double rangeDeg, double stepDeg, bool fineSpeed,
            System.Threading.CancellationToken ct)
        {
            if (marks == null || marks.Count < 4)
            {
                Log.Write(UnitName, "StartTCorrection", "marks 부족(4개 필요)");
                return -1;
            }

            if (rangeDeg <= 0) rangeDeg = 4.0;
            if (stepDeg <= 0) stepDeg = 0.1;
            if (stepDeg > rangeDeg) stepDeg = rangeDeg;

            TCorrectionResults.Clear();
            if (AxisT == null || AxisX == null || AxisY == null)
            {
                Log.Write(UnitName, "StartTCorrection", "Axis binding missing");
                return -1;
            }

            if (!Config.IsSimulation && OutStageCamera == null)
            {
                Log.Write(UnitName, "StartTCorrection", "Camera not bound");
                return -1;
            }

            try
            {
                ////1) T축 보정 시작
                double baseT = GetTeahcingPosCenterT();

                // 마크 1~4 순회
                //for (int markIndex = 0; markIndex < 4; markIndex++)
                for (int markIndex = 0; markIndex < 4; markIndex++)
                {
                    if (ct.IsCancellationRequested) { Log.Write(UnitName, "StartTCorrection", "Cancelled before mark loop"); return -2; }

                    var (mx, my) = marks[markIndex];
                    // XY 이동
                    int rc = MoveToPositionXY(mx, my, isFine: fineSpeed);
                    if (rc != 0)
                    {
                        Log.Write(UnitName, "StartTCorrection", $"MoveToPositionXY Mark{markIndex + 1} Fail");
                        return rc;
                    }

                    // 기준 T로 이동
                    rc = MoveAxisPositionOne(AxisT, baseT, isFine: fineSpeed);
                    if (rc != 0)
                    {
                        Log.Write(UnitName, "StartTCorrection", $"MoveAxisPositionOne(T) Mark{markIndex + 1} Fail");
                        return rc;
                    }

                    // 각도 스캔: -range ~ +range
                    double startDeg = -rangeDeg;
                    double endDeg = +rangeDeg;
                    // 방향: 사용자가 예시로 -4~+4라 했으므로 오름차순
                    for(int iter = 0; iter < 2; iter ++)
                    {
                        int nDir = 1;
                        if (iter == 0)
                        {
                            nDir = -1;
                        }
                        rc = MoveToPositionXY(mx, my, isFine: fineSpeed);
                        if (rc != 0)
                        {
                            Log.Write(UnitName, "StartTCorrection", $"MoveToPositionXY Mark{markIndex + 1} Fail");
                            return rc;
                        }
                        for (double rel = stepDeg*iter; rel <= rangeDeg; rel += stepDeg)
                        {
                            if (ct.IsCancellationRequested) { Log.Write(UnitName, "StartTCorrection", "Cancelled during angle scan"); return -2; }

                            double targetT = baseT + rel * nDir;
                            rc = MoveAxisPositionOne(AxisT, targetT, isFine: fineSpeed);
                            if (rc != 0)
                            {
                                Log.Write(UnitName, "StartTCorrection", $"T move failed Mark{markIndex + 1} rel={rel:F3}");
                                return rc;
                            }

                            rc = AlignXY(bFineSpeed: fineSpeed);
                            bool success = (rc == 0) && IsAlignResult;

                            var rec = new TCorrectionRecord
                            {
                                MarkIndex = markIndex + 1,
                                AngleDeg = rel,
                                StageX = AxisX.GetPosition(),
                                StageY = AxisY.GetPosition(),
                                StageT = AxisT.GetPosition(),
                                FoundOffsetX = dLastFoundX,
                                FoundOffsetY = dLastFoundY,
                                FoundAngle = dLastFoundAngle,
                                AlignSuccess = success
                            };
                            TCorrectionResults.Add(rec);
                            try { TCorrectionSampleCaptured?.Invoke(this, rec); } catch { }

                            //rc = MoveToPositionXY(mx, my, isFine: fineSpeed);
                            //if (rc != 0)
                            //{
                            //    Log.Write(UnitName, "StartTCorrection", $"MoveToPositionXY Mark{markIndex + 1} Fail");
                            //    return rc;
                            //}

                            double dX = AxisX.GetPosition() + dLastFoundX;
                            double dY = AxisY.GetPosition() + dLastFoundY;

                            rc = MoveToPositionXY(dX, dY, isFine: fineSpeed);
                            if (rc != 0)
                            {
                                Log.Write(UnitName, "StartTCorrection", $"MoveToPositionXY Mark{markIndex + 1} Fail");
                                return rc;
                            }


                            Log.Write(UnitName, "TCorrectionSample",
                                $"Mark={markIndex + 1}, RelT={rel:F3}, StageT={rec.StageT:F3}, " +
                                $"FoundX={rec.FoundOffsetX:F4}, FoundY={rec.FoundOffsetY:F4}, " +
                                $"Angle={rec.FoundAngle:F4}, Success={success}");
                        }
                    }
                    
                }

                Log.Write(UnitName, "StartTCorrection", $"Completed. Total samples={TCorrectionResults.Count}");
                return 0;

            }
            catch (Exception ex)
            {
                Log.Write(ex);
                Log.Write(UnitName, "StartTCorrection", ex.Message);
                return -1;
            }
        }

        // OutputStage 클래스 내부에 CSV 저장 메서드 추가
        public void SaveTCorrectionCsv(string path,
                                       IReadOnlyList<(double X, double Y)> marks,
                                       double rangeDeg,
                                       double stepDeg)
        {
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("path");
            if (marks == null || marks.Count < 4) throw new ArgumentException("marks must have 4 points");

            using (var w = new StreamWriter(path, false, Encoding.UTF8))
            {
                // Meta header
                w.WriteLine("#TCorrectionMeta");
                w.WriteLine(string.Format(CultureInfo.InvariantCulture, "RangeDeg={0:F3}", rangeDeg));
                w.WriteLine(string.Format(CultureInfo.InvariantCulture, "StepDeg={0:F3}", stepDeg));
                for (int i = 0; i < 4; i++)
                {
                    var (mx, my) = marks[i];
                    // 이미지 포맷 예시에 맞춰 "MarkN=X,Y"
                    w.WriteLine(string.Format(CultureInfo.InvariantCulture, "Mark{0}={1:F6},{2:F6}", i + 1, mx, my));
                }

                // Data section
                w.WriteLine("---DATA---");
                w.WriteLine("MarkIndex,Index,Angle,ImageX,ImageY,ImageT,StageX,StageY,StageT,CalX,CalY,CalT");

                // Index는 1부터 증가
                int index = 0;
                foreach (var r in TCorrectionResults)
                {
                    index++;

                    // ImageX/ImageY: FoundOffsetX/FoundOffsetY 사용
                    // Score: AlignSuccess -> 1/0
                    string calX = ""; string calY = ""; string calT = "";

                    w.WriteLine(string.Format(CultureInfo.InvariantCulture,
                        "{0},{1},{2:F3},{3:F3},{4:F3},{5:F3},{6:F3},{7:F3},{8:F3},{9:F3},{10:F3},{11:F3}",
                        r.MarkIndex,
                        index,
                        r.AngleDeg,
                        r.FoundOffsetX,
                        r.FoundOffsetY,
                        r.FoundAngle,
                        r.StageX,
                        r.StageY,
                        r.StageT,
                        calX,
                        calY,
                        calT));
                }
            }
        }

        // 필요 시: 보정 실행 후 바로 저장하는 헬퍼
        public int StartTCorrectionAndSave(IReadOnlyList<(double X, double Y)> marks,
                                           double rangeDeg, double stepDeg, bool fineSpeed,
                                           System.Threading.CancellationToken ct,
                                           string saveCsvPath)
        {
            var rc = StartTCorrection(marks, rangeDeg, stepDeg, fineSpeed, ct);
            if (rc == 0)
            {
                try
                {
                    SaveTCorrectionCsv(saveCsvPath, marks, rangeDeg, stepDeg);
                    Log.Write(UnitName, "StartTCorrectionAndSave", $"Saved: {saveCsvPath}");
                }
                catch (Exception ex)
                {
                    Log.Write(UnitName, "StartTCorrectionAndSave", $"Save failed: {ex.Message}");
                    return -3;
                }
            }
            return rc;
        }

        /// <summary>
        /// SaveTCorrectionCsv로 저장된 CSV를 로드하여 TCorrectionResults를 채우고,
        /// 메타 정보(marks, rangeDeg, stepDeg)를 반환합니다.
        /// </summary>
        /// <param name="path">CSV 파일 경로</param>
        /// <param name="marks">Mark1~4의 (X,Y) 목록</param>
        /// <param name="rangeDeg">스캔 각 범위</param>
        /// <param name="stepDeg">스캔 각 스텝</param>
        /// <returns>파싱된 레코드 리스트(또는 null: 실패)</returns>
        public List<TCorrectionRecord> LoadTCorrectionCsv(string path,
                                                          out List<(double X, double Y)> marks,
                                                          out double rangeDeg,
                                                          out double stepDeg)
        {
            marks = new List<(double X, double Y)>(capacity: 4);
            rangeDeg = 0.0;
            stepDeg = 0.0;

            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            {
                Log.Write(UnitName, "LoadTCorrectionCsv", $"파일 없음: {path}");
                return null;
            }

            var records = new List<TCorrectionRecord>();
            try
            {
                using (var r = new StreamReader(path, Encoding.UTF8))
                {
                    // 1) 메타 섹션 파싱
                    // 첫 줄 "#TCorrectionMeta" 기대
                    string line = r.ReadLine();
                    if (line == null || !line.Trim().StartsWith("#TCorrectionMeta"))
                    {
                        Log.Write(UnitName, "LoadTCorrectionCsv", "메타 헤더('#TCorrectionMeta') 누락");
                        return null;
                    }

                    // RangeDeg=..., StepDeg=...
                    // MarkN=X,Y (N=1..4)
                    var ci = CultureInfo.InvariantCulture;
                    for (; ; )
                    {
                        line = r.ReadLine();
                        if (line == null) { Log.Write(UnitName, "LoadTCorrectionCsv", "예상치 못한 EOF(메타)"); return null; }
                        line = line.Trim();

                        if (line == "---DATA---")
                        {
                            break; // 데이터 섹션으로 진입
                        }

                        if (line.Length == 0) continue;

                        if (line.StartsWith("RangeDeg=", StringComparison.OrdinalIgnoreCase))
                        {
                            var val = line.Substring("RangeDeg=".Length).Trim();
                            if (!double.TryParse(val, NumberStyles.Float, ci, out rangeDeg))
                                Log.Write(UnitName, "LoadTCorrectionCsv", $"RangeDeg 파싱 실패: '{val}'");
                        }
                        else if (line.StartsWith("StepDeg=", StringComparison.OrdinalIgnoreCase))
                        {
                            var val = line.Substring("StepDeg=".Length).Trim();
                            if (!double.TryParse(val, NumberStyles.Float, ci, out stepDeg))
                                Log.Write(UnitName, "LoadTCorrectionCsv", $"StepDeg 파싱 실패: '{val}'");
                        }
                        else if (line.StartsWith("Mark", StringComparison.OrdinalIgnoreCase))
                        {
                            // 형식: MarkN=X,Y
                            int eqIdx = line.IndexOf('=');
                            if (eqIdx > 0 && eqIdx + 1 < line.Length)
                            {
                                var rhs = line.Substring(eqIdx + 1).Trim();
                                var parts = rhs.Split(new[] { ',' }, StringSplitOptions.None);
                                if (parts.Length >= 2 &&
                                    double.TryParse(parts[0], NumberStyles.Float, ci, out var mx) &&
                                    double.TryParse(parts[1], NumberStyles.Float, ci, out var my))
                                {
                                    marks.Add((mx, my));
                                }
                                else
                                {
                                    Log.Write(UnitName, "LoadTCorrectionCsv", $"Mark 파싱 실패: '{line}'");
                                }
                            }
                        }
                        // 그 외 라인은 무시(코멘트 확장 대비)
                    }

                    // 2) 데이터 섹션 헤더 라인
                    line = r.ReadLine();
                    if (line == null)
                    {
                        Log.Write(UnitName, "LoadTCorrectionCsv", "데이터 헤더 누락");
                        return null;
                    }
                    // 기대 헤더:
                    // MarkIndex,Index,Angle,ImageX,ImageY,ImageT,StageX,StageY,StageT,CalX,CalY,CalT
                    // 헤더 검증은 느슨하게(열 수만 확인)
                    var headerCols = line.Split(',');
                    if (headerCols.Length < 12)
                    {
                        Log.Write(UnitName, "LoadTCorrectionCsv", $"데이터 헤더 열 개수 부족({headerCols.Length})");
                        // 계속 진행 시도
                    }

                    // 3) 데이터 행 파싱
                    while ((line = r.ReadLine()) != null)
                    {
                        line = line.Trim();
                        if (line.Length == 0) continue;

                        var cols = line.Split(',');
                        // 최소 12열 기대
                        if (cols.Length < 12)
                        {
                            Log.Write(UnitName, "LoadTCorrectionCsv", $"열 개수 부족({cols.Length}) 라인: {line}");
                            continue;
                        }

                        // 안전 파싱 헬퍼
                        int ParseInt(string s, int def = 0)
                        {
                            return int.TryParse(s, NumberStyles.Integer, ci, out var v) ? v : def;
                        }
                        double ParseDouble(string s, double def = 0)
                        {
                            if (string.IsNullOrWhiteSpace(s)) return def;
                            return double.TryParse(s, NumberStyles.Float, ci, out var v) ? v : def;
                        }

                        var rec = new TCorrectionRecord
                        {
                            MarkIndex = ParseInt(cols[0]),
                            AngleDeg = ParseDouble(cols[2]),
                            FoundOffsetX = ParseDouble(cols[3]),
                            FoundOffsetY = ParseDouble(cols[4]),
                            FoundAngle = ParseDouble(cols[5]),
                            StageX = ParseDouble(cols[6]),
                            StageY = ParseDouble(cols[7]),
                            StageT = ParseDouble(cols[8]),
                            // AlignSuccess는 저장 포맷에 없으므로 추정 불가 → false로 초기화
                            AlignSuccess = false
                        };

                        // CalX/CalY/CalT는 빈 문자열일 수 있으므로 필요시 사용자가 후처리
                        // cols[9], cols[10], cols[11]은 현재 무시

                        records.Add(rec);
                    }
                }

                // 4) 클래스 상태 반영
                TCorrectionResults.Clear();
                TCorrectionResults.AddRange(records);

                Log.Write(UnitName, "LoadTCorrectionCsv",
                    $"로드 완료: {path}, Marks={marks.Count}, Range={rangeDeg:F3}, Step={stepDeg:F3}, Rows={records.Count}");

                return records;
            }
            catch (Exception ex)
            {
                Log.Write(UnitName, "LoadTCorrectionCsv", $"예외: {ex.Message}");
                return null;
            }
        }

        private void CalcTCorrection()
        {
            // _recodes ->TCorrectionResults
            linkTypeXYTStageCorrection = new LinkTypeXYTStageCorrection();
            var v = TCorrectionResults.OrderBy(t => t.StageT).ThenBy(t => t.MarkIndex);

            const double thetaMatchTol = 1e-6; // [ADD] theta grouping tolerance

            foreach (var rec in v)
            {
                var buffer = linkTypeXYTStageCorrection.CorrectionPoints
                    .Where(p => Math.Abs(p.CommandTheta - rec.StageT) <= thetaMatchTol);

                XyCoordinate xyCoordinate = new XyCoordinate()
                { X = rec.StageX + rec.FoundOffsetX, Y = rec.StageY + rec.FoundOffsetY };

                if (buffer.Any() == false)
                {
                    var point = new List<XyCoordinate>();
                    point.Add(xyCoordinate);
                    for (int iter = 0; iter < 3; iter++)
                    {
                        point.Add(new XyCoordinate() { X = 0, Y = 0 });
                    }
                    linkTypeXYTStageCorrection.AddCorrectionPoint(point, rec.StageT);
                }
                else
                {
                    var point = buffer.First().PointDs;
                    if (rec.MarkIndex <= point.Count)
                    {
                        point[rec.MarkIndex - 1] = xyCoordinate;
                    }
                    else
                    {
                        point.Add(xyCoordinate);
                    }
                }
            }

            linkTypeXYTStageCorrection.SetZeroCommandTheta(0.3);
            this.linkTypeXYTStageCorrection = linkTypeXYTStageCorrection;
        }
        //private void CalcTCorrection()
        //{
        //    //_recodes ->TCorrectionResults
        //    linkTypeXYTStageCorrection = new LinkTypeXYTStageCorrection();
        //    var v = TCorrectionResults.OrderBy(t => t.StageT).ThenBy(t => t.MarkIndex);

        //    const double thetaMatchTol = 1e-6; // [ADD] theta grouping tolerance

        //    foreach (var rec in v)
        //    {
        //        var buffer = linkTypeXYTStageCorrection.CorrectionPoints.
        //            Where(p => p.CommandTheta == rec.StageT);

        //        XyCoordinate xyCoordinate = new XyCoordinate() 
        //        { X = rec.StageX + rec.FoundOffsetX, Y = rec.StageY + rec.FoundOffsetY };

        //        if (buffer.Count() == 0)
        //        {
        //            var point = new List<XyCoordinate>();
        //            point.Add(xyCoordinate);
        //            for (int iter = 0; iter < 3; iter++)
        //            {
        //                point.Add(new XyCoordinate() { X = 0, Y = 0 });
        //            }
        //            linkTypeXYTStageCorrection.AddCorrectionPoint(point, rec.StageT);
        //        }
        //        else
        //        {
        //            var point = buffer.First().PointDs;
        //            if (rec.MarkIndex <= point.Count)
        //            {
        //                point[rec.MarkIndex - 1] = xyCoordinate;
        //            }
        //            else
        //            {
        //                point.Add(xyCoordinate);

        //            }

        //        }
        //    }
        //    linkTypeXYTStageCorrection.SetZeroCommandTheta(0.3);
        //    this.linkTypeXYTStageCorrection = linkTypeXYTStageCorrection;
        //}

        /// <summary>
        /// 지정 경로를 강제로 사용하여 로드하고 보정 링크를 구성합니다.
        /// UI나 시퀀스에서 특정 파일을 선택해 호출할 때 사용.
        /// </summary>
        public int LoadTCorrectionCsvAndApply(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                Log.Write(UnitName, "LoadTCorrectionCsvAndApply", "경로가 비어 있습니다.");
                return -1;
            }

            List<(double X, double Y)> marks;
            double rangeDeg;
            double stepDeg;
            var recs = LoadTCorrectionCsv(path, out marks, out rangeDeg, out stepDeg);
            if (recs == null || recs.Count == 0)
            {
                Log.Write(UnitName, "LoadTCorrectionCsvAndApply", $"로드 실패 또는 데이터 없음: {path}");
                return -2;
            }

            try
            {
                CalcTCorrection();
            }
            catch (Exception ex)
            {
                Log.Write(UnitName, "LoadTCorrectionCsvAndApply", $"CalcTCorrection 실패: {ex.Message}");
                return -3;
            }

            Log.Write(UnitName, "LoadTCorrectionCsvAndApply",
                $"성공: {path}, Marks={marks?.Count ?? 0}, Range={rangeDeg:F3}, Step={stepDeg:F3}, Rows={recs.Count}");
            return 0;
        }

       
        private readonly object _reserveLock = new object();
        public int ForceCompleteAndAllowUnload(string reason = null)
        {
            try
            {
                var wafer = GetMaterialWafer();
                if (wafer == null || wafer.Dies == null)
                    return 0;

                lock (_reserveLock)
                {
                    lock (wafer.Dies)
                    {
                        foreach (var d in wafer.Dies)
                        {
                            if (d == null) 
                                continue;

                            // 아직 정리되지 않은 항목은 전부 Rejected로 마킹
                            if (d.State != DieProcessState.Placed
                             && d.State != DieProcessState.Rejected
                             && d.State != DieProcessState.Skip )
                            {
                                d.State = DieProcessState.Rejected;
                                // 여기 Presence는 프로젝트 내 의미가 섞여있는데,
                                // 현재 코드 흐름(HasNextDie/TryReserveNextEmptyBin)이 Presence를 무시하므로
                                // 기존 MarkCurrentReservedMissing과 동일하게 Exist로 맞춥니다.
                                d.Presence = Material.MaterialPresence.Exist;
                            }
                        }

                        //여기서 Completed 할 필요가 없지.
                        // Completed 조건은.. Feeder에서 확인하면 되니깐.
                        //wafer.ProcessSatate = Material.MaterialProcessSatate.Completed;
                    }
                }

                UpdateUI();
                Log.Write(UnitName, "ForceCompleteAndAllowUnload",
                    $"Forced Completed. reason={reason ?? "null"}");

                return 0;
            }
            catch (Exception ex)
            {
                Log.Write(UnitName, "ForceCompleteAndAllowUnload", ex.Message);
                return -1;
            }
        }

        private bool IsExternalDieBufferEmpty()
        {
            try
            {
                // Rotary(언로드 스테이션에 서 있는 소켓) 쪽 die 존재 여부
                // 여기 조건 다시 생각해야함.
                //var rotaryDie = RotaryUnit?.GetUnloadSocketMaterial();

                // OutputDieTransfer가 현재 들고있는 die 존재 여부
                var odtDie = OutputDieTransfer?.GetMaterial() as MaterialDie;

                //return (rotaryDie == null) && (odtDie == null);
                return (odtDie == null);
            }
            catch
            {
                // 안전하게: 판단 불가면 강제완료하지 않음
                return false;
            }
        }

        /// <summary>
        /// "입력 웨이퍼 종료" 같은 외부 종료 조건에서,
        /// Rotary(언로드 소켓) + OutputDieTransfer 모두 die가 비어있을 때만
        /// OutputStage 웨이퍼를 강제 Completed 처리하여 언로딩이 진행되도록 합니다.
        /// </summary>
        public int ForceCompleteAndAllowUnloadWhenBuffersEmpty(string reason = null)
        {
            if (!IsExternalDieBufferEmpty())
            {
                Log.Write(UnitName, "ForceCompleteAndAllowUnloadWhenBuffersEmpty",
                    "Skipped: Rotary unload socket or OutputDieTransfer still has die.");
                return 1; // 스킵(조건 미충족)
            }

            return ForceCompleteAndAllowUnload(reason);
        }

        public int MoveToTeachingPositionBySelectionIndex(int teachingSelIndex, bool isFine = false)
        {
            if (Config == null)
                return -1;

            string tpName;
            if (!Config.GetTeachingPositionName(teachingSelIndex, out tpName) || string.IsNullOrWhiteSpace(tpName))
            {
                return -1;
            }

            OutputStageConfig.TeachingPositionName en;
            if (!Enum.TryParse(tpName, out en))
            {
                return -1;
            }

            switch (en)
            {
                case OutputStageConfig.TeachingPositionName.Loading:
                    return MoveToStageLoadPosition(isFine);
                case OutputStageConfig.TeachingPositionName.Unloading:
                    return MoveToStageUnloadPosition(isFine);
                case OutputStageConfig.TeachingPositionName.CenterPoint:
                    return MoveToStageCenterPosition(isFine);
                case OutputStageConfig.TeachingPositionName.Ready:
                    return MoveToStageReadyPosition(isFine);
                default:
                    return -1;
            }
        }

        // OutputStage 클래스 내부 편한 위치에 추가
        private bool TryGetInputMapByIndex(int dieIndex, out double mapX, out double mapY)
        {
            mapX = 0;
            mapY = 0;

            try
            {
                var src = InputStage?.GetMaterialWafer();
                if (src == null || src.Dies == null || src.Dies.Count == 0)
                    return false;

                lock (src.Dies)
                {
                    var s = src.Dies.FirstOrDefault(d => d != null && d.Index == dieIndex);
                    if (s == null) 
                        return false;

                    mapX = s.MapX;
                    mapY = s.MapY;
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

    }
}