using QMC.Common;
using QMC.Common.Alarm;
using QMC.Common.Component;
using QMC.Common.IOUtil;
using QMC.Common.Keithley;
using QMC.Common.Motions;
using QMC.Common.PKGTester;
using QMC.Common.Spectrometer;
using QMC.Common.StrainGage;
using QMC.Common.Unit;
using QMC.LCP_280.Process.Component;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using System.Xml.Serialization.Advanced;
using static QMC.LCP_280.Process.Equipment;

namespace QMC.LCP_280.Process.Unit
{
    public sealed class IndexChipProber : BaseUnit<IndexChipProberConfig>, IDisposable
    {
        public new enum AlarmKeys
        { 
            eNotReadyToMeasure = 10801, // 임시 알람 번호
        }

        #region InitAlarm
        protected override void InitAlarm()
        {
            string source = "Index_Prober";

            base.InitAlarm();
            // 1. 공용 파일 로더에서 알람 목록 가져오기
            var loadedAlarms = GlobalAlarmTable.Instance.GetAlarmsForSource(source);
            if (loadedAlarms == null || loadedAlarms.Count == 0)
            {
                Log.Write("AlarmInit", $"Cannot find alarms for source '{source}' in the alarm file. Only default alarms will be registered.");

                AlarmInfo alarm = new AlarmInfo();
                alarm.Code = (int)AlarmKeys.eNotReadyToMeasure;
                alarm.Title = "Not ready to measure.";
                alarm.Cause = "1. Please check if a Test Condition Set is applied. 2. Please check if the instrument is normally initialized.";
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

        #region Config / Teaching
        public IndexChipProberConfig IndexChipProberConfig => Config;
        #endregion

        #region Bind Unit
        InputStage InputStage {  get; set; }
        OutputStage OutputStage { get; set; }
        Rotary Rotary { get; set; }
        IndexChipProbeController IndexChipProbeController { get; set; }

        #region Components
        private PKGTester tester = Equipment.Instance.Tester;

        // StrainGage 모니터 주입(옵션). 외부에서 설정하거나, OnBindUnit에서 획득하도록 구성.
        public StrainGageMonitor StrainGageMonitor { get; set; }
        #endregion

        protected override void OnBindUnit()
        {
            base.OnBindUnit();
            Rotary = Equipment.Instance.GetUnit(UnitKeys.Rotary) as Rotary;
            IndexChipProbeController = Equipment.Instance.GetUnit(UnitKeys.IndexChipProbeController) as IndexChipProbeController;
            InputStage = Equipment.Instance.GetUnit(UnitKeys.InputStage) as InputStage;
            OutputStage = Equipment.Instance.GetUnit(UnitKeys.OutputStage) as OutputStage;

            // 가능하다면 Equipment에 등록된 모니터를 얻어옵니다. 없으면 그대로 null 유지(주입 방식 허용).
            try
            {
                if (StrainGageMonitor == null)
                {
                    StrainGageMonitor = Equipment.Instance.StrainGageMonitor;
                    // 예: Equipment.Instance.Components.TryGet<StrainGageMonitor>(out var mon);
                    // 프로젝트 구조에 맞게 바꾸세요. 실패해도 무방.
                }
            }
            catch { /* ignore */ }
        }
        #endregion

        #region ctor / Initialization
        public IndexChipProber(IndexChipProberConfig config = null)
            : base(config ?? new IndexChipProberConfig())
        {
            AddComponents();
        }

        public override void AddComponents()
        {
            Config.LoadAndBindAxes(Equipment.Instance.AxisManager);
            Config.InitializeDefaultTeachingPositions();
            BindAxes();
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
            //return base.OnStart();
            var ret = base.OnStart();
            if (ret != 0) 
                return ret;

            return 0;
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

        public void TeachCurrentPosition(string positionName, string description = null)
        {
            var axisPositions = new Dictionary<string, double>();
            foreach (var axisPair in Axes)
                axisPositions[axisPair.Key] = axisPair.Value.GetPosition();
            var tp = new TeachingPosition(positionName, axisPositions, description);
            Config.SetTeachingPosition(tp);
        }

        public int MoveToTeachingPosition(string positionName, bool isFine)
        {
            if (string.IsNullOrWhiteSpace(positionName))
            {
                Log.Write(UnitName, nameof(MoveToTeachingPosition),
                        $"[TeachingMove] Could not find '{positionName}' in TeachingPositions.");
                return -1;
            }

            int result = 0;

            IndexChipProberConfig.TeachingPositionName en;
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
                        $"[TeachingMove] Could not find index for '{positionName}' in TeachingPositions.");
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

        #region Axis Helpers
        private readonly List<MotionAxis> _boundAxes = new List<MotionAxis>();
        public IReadOnlyList<MotionAxis> BoundAxes => _boundAxes;        

        private void BindAxes()
        {
            _boundAxes.Clear();
            foreach (var kv in Axes) _boundAxes.Add(kv.Value);
        }
        #endregion

        #region IO Helpers
        public bool ReadInput(string name)
        {
            // No HardInputs defined currently.
            return false;
        }
        public bool WriteOutput(string name, bool on)
        {
            // No outputs defined.
            return false;
        }
        #endregion

        #region Seq signal
        public bool RequestChipInsp { get; set; }
        public bool InspectDone { get; set; }
        #endregion

        protected override void OnMakeSequence()
        {
            base.OnMakeSequence();
            this.SequencePlayers.Add(MeasureChip);
        }

        #region Seq 단위 동작 함수
        /// <summary>
        /// LED PKG 측정
        /// 순서: 측정 -> 결과를 Material Object에 Assign
        /// </summary>
        public int MeasureChip(bool bFineSpeed = false)
        {
            int bRet = 0;
            this.CurrentFunc = MeasureChip;
            
            try
            {
                LogSequence("Start");
                int nIndex = this.GetProbeIndexNo();

                // 1) Check Can Measure
                InspectDone = false;

                //Log.Write("kkkkkkProb", "m1");

                var equipment = Equipment.Instance;
                bool IsDryRunEqp = equipment.EquipmentConfig.IsDryRun;
                if (Config.IsSimulation == false && (Config.IsDryRun == false && IsDryRunEqp == false))
                {
                    if (!tester.CanMeasure())
                    {
                        PostAlarm((int)AlarmKeys.eNotReadyToMeasure);
                        Log.Write(UnitName, "MeasureChip", "PKG Tester: Not ready to measure.");
                        bRet = -1;
                        return bRet;
                    }

                    // 2) Measure Chip
                    bRet = Measure();
                    if (bRet != 0)
                    {
                        Log.Write(UnitName, "MeasureChip", "Measure() Fail");
                        bRet = 0; // -1;
                        return bRet;
                    }
                }
                else
                {
                    bRet = Measure();
                }

                MaterialDie die = this.Rotary.GetProbeSocketMaterial();
                if(die.Presence == Material.MaterialPresence.Exist)
                {
                    die.TesterResult = tester.Result;
                    die.SocketIndex = this.GetProbeIndexNo();
                    PopulateDieWithTesterResult(die);
                    die.ProcessSatate = Material.MaterialProcessSatate.Processing;
                }

                if(this.RunUnitStatus != UnitStatus.AutoRunning)
                {
                    bRet = AssignDataToMaterialObject();
                    if (bRet != 0)
                    {
                        PostAlarm((int)AlarmKeys.eNotReadyToMeasure);
                        Log.Write(UnitName, "MeasureChip", "Fiel Open Error.");
                        return -1;
                    }
                }

                InspectDone = true;
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                return -1;
            }
            finally
            {
                LogSequence("End");
            }

            return bRet;
        }

        private int Measure()
        {
            int rotaryIndex = this.GetProbeIndexNo();
            try
            {
                var s = Rotary.GetSocket(rotaryIndex);
                if (s != null)
                {
                    MaterialDie currentDie = s.GetMaterialDie();
                    // 1) PKGTester에 현재 측정할 Die의 X, Y 좌표 정보를 전달
                    tester.CurrentDieContext = $"XADR={currentDie?.MapX}, YADR={currentDie?.MapY}";
                }

#if true
                Task<int> task = tester.MeasureAsync(rotaryIndex);
                while (IsEndTask(task) == false)
                {
                    Thread.Sleep(1);
                }
#else
                Task<int> task = tester.MeasureAsync2(rotaryIndex);
                while (!IsEndTask(task))
                {
                    Thread.Sleep(1);
                }
#endif
                // [Patch] 비동기 작업 예외 처리
                if (task.IsFaulted)
                {
                    var ex = task.Exception?.Flatten();
                    Log.Write(UnitName, "Measure", $"Async Error: {ex?.Message}");
                    return -1;
                }

                // [Patch] 취소 처리
                if (task.IsCanceled)
                {
                    Log.Write(UnitName, "Measure", "Async Task Canceled");
                    return -1;
                }

                int rc = task.Result;
                if (rc == 0)
                {
                    // 측정 성공 시 StrainGage 기반 KELFS/KELDG 주입
                    //TryAssignKelItemsFromStrainGage();
                }
                return rc;
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                return -1;
            }
        }

        // StrainGage 1~4 Force 값으로 KELFS / KELDG 평균 계산 후 Result에 주입
        private void TryAssignKelItemsFromStrainGage()
        {
            try
            {
                var res = tester?.Result;
                if (res == null || res.Items == null) return;

                // 스냅샷 평균(노이즈 완화)
                var sgAvg = GetStrainGageSnapshotAveraged(5, 2); // 필요시 샘플/딜레이 조정

                double? GetForce(int ch)
                {
                    string key = $"SG{ch}_Force";
                    if (sgAvg.TryGetValue(key, out var v))
                        return v;
                    return null;
                }

                // KELFS: 채널 1,2 Force 평균
                if (res.Items.TryGetValue("KELFS", out var kelfsItem))
                {
                    var f1 = GetForce(1);
                    var f2 = GetForce(2);
                    if (f1.HasValue && f2.HasValue)
                    {
                        double avg = (f1.Value + f2.Value) / 2.0;
                        kelfsItem.RawData = avg;
                        kelfsItem.Value = avg;
                    }
                }

                // KELDG: 채널 3,4 Force 평균
                if (res.Items.TryGetValue("KELDG", out var keldgItem))
                {
                    var f3 = GetForce(3);
                    var f4 = GetForce(4);
                    if (f3.HasValue && f4.HasValue)
                    {
                        double avg = (f3.Value + f4.Value) / 2.0;
                        keldgItem.RawData = avg;
                        keldgItem.Value = avg;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Write(UnitName, $"[TryAssignKelItemsFromStrainGage] {ex.Message}");
            }
        }

        /// <summary>
        /// 측정 완료 후 Die 객체에 TesterResult 및 위치/메타 정보를 모두 채워 넣는다.
        ///  - TesterResult: Clone() 저장
        ///  - MeasureValues: 항목 Value + Range(Min/Max) + 위치/소켓/시간 메타
        ///  - Bin 정보: Rank / RankName / Pass / RejectReason
        ///  - Wafer / Bin(OutStage 예정 위치: BinX,BinY) / Center(InputStage 픽업 위치: CenterX,CenterY)
        ///  - State / ProcessSatate 갱신
        /// </summary>
        private void PopulateDieWithTesterResult(MaterialDie die)
        {
            if (die == null || tester == null)
                return;

            var res = tester.Result;
            if (res == null)
                return;

            try
            {
                // 현재 시점의 In/Out 스테이지 Wafer 참조
                var inWafer = InputStage?.GetMaterialWafer();
                var outWafer = OutputStage?.GetMaterialWafer();
                if (outWafer == null)
                {
                    Stopwatch sw = Stopwatch.StartNew();
                    int timeOutMs = 5000;

                    while (true)
                    {
                        if (IsStop)
                        {
                            Log.Write(UnitName, "PopulateDieWithTesterResult", "IsStop - outWafer == null");
                            return;
                        }

                        if (sw.ElapsedMilliseconds > timeOutMs)
                        {
                            Log.Write(UnitName, "PopulateDieWithTesterResult", $"Timeout waiting for OutputStage Wafer ({timeOutMs}ms)");
                            // 실패 처리를 하거나, return하여 다음 로직 보호
                            return;
                        }

                        outWafer = OutputStage?.GetMaterialWafer();
                        if (outWafer != null)
                        {
                            break;
                        }
                        Thread.Sleep(1); // Polling 간격 완화
                    }
                }

                int probeIndex = this.GetProbeIndexNo();
                int loadIndex = Rotary?.GetLoadIndexNo() ?? -1;
                int socketCount = Rotary?.GetIndexCount() ?? 0;

                // TesterResult Clone
                var cloned = res.Clone();
                die.TesterResult = cloned;

                // 측정값 딕셔너리 초기화
                if (die.MeasureValues == null)
                    die.MeasureValues = new Dictionary<string, double>();
                else
                    die.MeasureValues.Clear();

                // 개별 TestItem 결과(Value)
                foreach (var kv in cloned.Items)
                {
                    var itemName = kv.Key;
                    var itemRes = kv.Value;
                    if (itemRes == null) continue;
                    die.MeasureValues[itemName] = itemRes.Value;
                    // 필요 시 Raw 저장:
                    // die.MeasureValues[itemName + "_Raw"] = itemRes.RawData;
                }

                // Range 정보 추가 (Ignore 제외)
                var ranges = tester.GetCurrentBinRanges();
                if (ranges != null)
                {
                    foreach (var kv in ranges)
                    {
                        var r = kv.Value;
                        if (r == null || r.Ignore) continue;
                        die.MeasureValues[kv.Key + "_Min"] = r.Min;
                        die.MeasureValues[kv.Key + "_Max"] = r.Max;
                    }
                }

                // ======================
                // 위치/메타 정보 채우기
                // ======================
                // 1) InputStage 픽업 위치 -> CenterX/CenterY (이미 매핑 시 보관된 값 사용)
                //    필요 시 메타에도 기록
                die.MeasureValues["_CenterX"] = die.CenterX;
                die.MeasureValues["_CenterY"] = die.CenterY;

                // 2) OutStage Bin 내려놓을 예정 위치 -> BinX/BinY
                //    - 예약 호출 없이, OutStage Wafer에서 첫번째 비어있는 슬롯을 조회하여 계획 좌표를 기록
                //    - 이미 값이 있다면 유지, 없으면 채움
                lock (outWafer.Dies)
                {
                    if ((die.BinX == 0 && die.BinY == 0) && outWafer?.Dies != null && outWafer.Dies.Count > 0)
                    {
                        var next = outWafer.Dies.FirstOrDefault(d => d != null && d.Presence != Material.MaterialPresence.Exist);
                        if (next != null)
                        {
                            die.BinX = next.BinX;
                            die.BinY = next.BinY;
                        }
                    }
                    die.MeasureValues["_PlanBinX"] = die.BinX;
                    die.MeasureValues["_PlanBinY"] = die.BinY;

                    // 기타 메타
                    die.MeasureValues["_ProbeIndex"] = probeIndex;
                    //die.MeasureValues["_LoadIndex"] = loadIndex;
                    //die.MeasureValues["_SocketCount"] = socketCount;
                    die.MeasureValues["_DieIndex"] = die.Index;
                    die.MeasureValues["_MapX"] = die.MapX;
                    die.MeasureValues["_MapY"] = die.MapY;
                    die.MeasureValues["_AngleDeg"] = die.Angle;
                    die.MeasureValues["_MeasureTimeMs"] = tester.MeasureTime.TotalMilliseconds;

                    // 3) 트래킹 정보(소스/타겟 웨이퍼)
                    if (inWafer != null)
                    {
                        die.SourceWaferId = inWafer.WaferId;
                    }
                    if (outWafer != null)
                    {
                        die.TargetWaferId = outWafer.WaferId;
                        die.TargetSlot = outWafer.SlotIndex;
                        die.TargetChipIndex = die.Index;
                    }
                    else
                    {
                        if (string.IsNullOrWhiteSpace(die.TargetWaferId))
                            die.TargetWaferId = die.SourceWaferId;
                        if (die.TargetSlot < 0) die.TargetSlot = -1;
                        if (die.TargetChipIndex < 0) die.TargetChipIndex = die.Index;
                    }

                    // Bin / Rank 정보
                    var bin = cloned.BinningResult;
                    if (bin != null)
                    {
                        die.Rank = bin.BinNo;
                        die.RankName = string.IsNullOrWhiteSpace(bin.BinLabel) ? "-" : bin.BinLabel;
                        bool isGood = (bin.BinType == BinningType.GoodBin);
                        die.IsPass = isGood;

                        if (!isGood)
                        {
                            if (string.IsNullOrWhiteSpace(die.RejectReason))
                                die.RejectReason = (bin.BinLabel == "NG") ? "NG" : "BinFail";
                            die.State = DieProcessState.Rejected;
                        }
                        else
                        {
                            if (die.State != DieProcessState.Rejected)
                                die.State = DieProcessState.Inspected;
                        }
                    }
                    else
                    {
                        die.Rank = -1;
                        die.RankName = "Error";
                        die.IsPass = false;
                        if (string.IsNullOrWhiteSpace(die.RejectReason))
                            die.RejectReason = "NoBinning";
                        if (die.State != DieProcessState.Rejected)
                            die.State = DieProcessState.Inspected;
                    }

                    // 공정 상태 유지
                    die.ProcessSatate = Material.MaterialProcessSatate.Processing;
                }
            }
            catch (Exception ex)
            {
                Log.Write(UnitName, $"[PopulateDieWithTesterResult] Exception: {ex.Message}");
            }
        }

        // View 변환 옵션 (UI 토글과 동일하게 외부에서 주입/설정)
        public bool InputViewRotate180 { get; set; } = false;
        public bool InputViewCenterOnPivot { get; set; } = true;
        public bool OutputViewRotate180 { get; set; } = false;
        public bool OutputViewCenterOnPivot { get; set; } = true;

        private int AssignDataToMaterialObject()
        {
            PKGTesterResult result = tester.Result;

            string logDir = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
            if (!System.IO.Directory.Exists(logDir))
                System.IO.Directory.CreateDirectory(logDir);

            // ... (Variable setup logic) ...
            var wafer = Rotary.GetMaterial() as MaterialDie;
            var die = Rotary.GetProbeSocketMaterial();
            string waferID = "";
            if (die != null)
            {
                waferID = die.SourceWaferId;
                Log.Write(UnitName, $"Index_{die.Index}, WaferID_{die.SourceWaferId}, BinID_{die.TargetWaferId}, State_{die.State.ToString()}");
            }
            else
            {
                waferID = "None";
                Log.Write(UnitName, "AssignDataToMaterialObject", "die.SourceWaferId Fail");
            }
            int nIndex = this.GetProbeIndexNo();

            string logFile = System.IO.Path.Combine(logDir, $"{waferID}_{DateTime.Now:yyyyMMdd}.csv");

            // [Patch] 파일 I/O 재시도 로직 추가
            int retryCount = 0;
            const int maxRetries = 3;

            while (retryCount < maxRetries)
            {
                try
                {
                    bool fileExists = System.IO.File.Exists(logFile);
                    var sgKeys = new List<string>();

                    // 신규 파일일 때만 StrainGage 컬럼 헤더 준비
                    if (!fileExists && die != null && die.MeasureValues != null)
                    {
                        sgKeys = die.MeasureValues.Keys
                                  .Where(k => k.StartsWith("SG", StringComparison.OrdinalIgnoreCase))
                                  .OrderBy(k => k, StringComparer.OrdinalIgnoreCase)
                                  .ToList();
                    }

                    using (var writer = new System.IO.StreamWriter(logFile, true, System.Text.Encoding.UTF8))
                    {
                        // 헤더 작성
                        if (!fileExists)
                        {
                            writer.Write("Time,SocketNo,DieNo,DiePosX,DiePosY,");
                            writer.Write("BinNo,BinType,BinLabel,");

                            foreach (var item in result.Items)
                            {
                                writer.Write($"{item.Key},");
                            }
                            foreach (var key in sgKeys)
                            {
                                writer.Write($"{key},");
                            }
                            writer.WriteLine();
                        }

                        // 데이터 작성
                        writer.Write($"{DateTime.Now:yyyy-MM-dd HH:mm:ss:fff},");
                        writer.Write($"{nIndex + 1},");
                        writer.Write($"{die.Index + 1},");

                        // 현장 맞춤 (반전)
                        writer.Write($"{die.MapX * -1},");
                        writer.Write($"{die.MapY * -1},");

                        var binResult = result.BinningResult;
                        writer.Write($"{binResult?.BinNo},");
                        writer.Write($"{binResult?.BinType},");
                        writer.Write($"{binResult?.BinLabel},");

                        foreach (var item in result.Items)
                        {
                            writer.Write($"{item.Value},");
                        }

                        // StrainGage 데이터
                        // (주의: sgKeys는 신규 파일 생성 시점에만 채워지므로, 
                        //  기존 파일에 append 할 때도 키를 다시 구해야 정확하지만, 
                        //  기존 로직 흐름상 여기서는 생략 혹은 위쪽 sgKeys 로직을 fileExists 여부 상관없이 가져오도록 수정 필요할 수 있음. 
                        //  일단 원본 유지하되 안전하게 작성)
                        if (die != null && die.MeasureValues != null)
                        {
                            // 매번 키를 가져와서 순서대로 쓰거나, 파일 헤더와 순서를 맞춰야 함.
                            // 여기서는 원본 로직(sgKeys.Count > 0 조건)을 따름
                            if (sgKeys.Count > 0)
                            {
                                foreach (var key in sgKeys)
                                {
                                    double v;
                                    die.MeasureValues.TryGetValue(key, out v);
                                    writer.Write($"{v},");
                                }
                            }
                            else if (fileExists)
                            {
                                // 파일이 이미 있을 때도 SG 데이터가 있다면 적어주는게 맞다면 로직 보강 필요.
                                // 현재는 원본 로직 유지.
                            }
                        }

                        writer.WriteLine();
                    }

                    // 성공 시 루프 탈출
                    return 0;
                }
                catch (IOException ioEx)
                {
                    retryCount++;
                    if (retryCount >= maxRetries)
                    {
                        Log.Write(UnitName, "AssignDataToMaterialObject", $"File IO Failed after {maxRetries} attempts: {ioEx.Message}");
                        return -1;
                    }
                    Thread.Sleep(50); // 잠시 대기 후 재시도
                }
                catch (Exception ex)
                {
                    Log.Write(UnitName, "AssignDataToMaterialObject", $"Unexpected Error: {ex.Message}");
                    return -1;
                }
            }

            return 0;
        }

        //private int AssignDataToMaterialObject()
        //{
        //    PKGTesterResult result = tester.Result;
        //    // 임시 테스트 코드 -----
        //    string logDir = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
        //    if (!System.IO.Directory.Exists(logDir))
        //        System.IO.Directory.CreateDirectory(logDir);

        //    var wafer = Rotary.GetMaterial() as MaterialDie;   //InputStage.GetMaterialWafer();
        //    var die = Rotary.GetProbeSocketMaterial();
        //    string waferID = "";
        //    if (die != null)
        //    {
        //        waferID = die.SourceWaferId;
        //        Log.Write(UnitName, $"Index_{die.Index}, WaferID_{die.SourceWaferId}, " +
        //            $"BinID_{die.TargetWaferId}, State_{die.State.ToString()}");
        //    }
        //    else
        //    {
        //        waferID = "None";
        //        Log.Write(UnitName, "AssignDataToMaterialObject", "die.SourceWaferId Fail");
        //    }

        //    int nIndex = this.GetProbeIndexNo();
        //    string logFile = System.IO.Path.Combine(logDir, $"{waferID}_{DateTime.Now:yyyyMMdd}.csv");
        //    bool fileExists = System.IO.File.Exists(logFile);

        //    // 신규 파일일 때만 StrainGage 컬럼을 헤더에 추가(기존 파일 헤더 불일치 방지)
        //    var sgKeys = new List<string>();
        //    if (!fileExists && die != null && die.MeasureValues != null)
        //    {
        //        sgKeys = die.MeasureValues.Keys
        //                  .Where(k => k.StartsWith("SG", StringComparison.OrdinalIgnoreCase))
        //                  .OrderBy(k => k, StringComparer.OrdinalIgnoreCase)
        //                  .ToList();
        //    }

        //    using (var writer = new System.IO.StreamWriter(logFile, true, System.Text.Encoding.UTF8))
        //    {
        //        // 파일이 없으면 헤더 추가
        //        if (!fileExists)
        //        {
        //            writer.Write("Time,");
        //            writer.Write("SocketNo,");
        //            writer.Write("DieNo,");
        //            writer.Write("DiePosX,");
        //            writer.Write("DiePosY,");

        //            // Bin / Rank 컬럼
        //            writer.Write("BinNo,");
        //            writer.Write("BinType,");
        //            writer.Write("BinLabel,");
        //            //writer.Write("TopRankBinNo,");
        //            //writer.Write("TopRankBinType,");
        //            //writer.Write("TopRankBinLabel,");
        //            //writer.Write("TopRankScore,");

        //            foreach (var item in result.Items)
        //            {
        //                writer.Write($"{item.Key},");
        //            }

        //            // StrainGage 헤더(있을 때만)
        //            foreach (var key in sgKeys)
        //            {
        //                writer.Write($"{key},");
        //            }

        //            writer.WriteLine();
        //        }

        //        // 데이터 행 추가 +1하지말자.
        //        writer.Write($"{DateTime.Now:yyyy-MM-dd HH:mm:ss:fff},");
        //        writer.Write($"{nIndex +1 },");
        //        writer.Write($"{die.Index + 1},");
        //        //writer.Write($"{die.MapX},");
        //        //writer.Write($"{die.MapY},");
        //        //현장맞춤..
        //        writer.Write($"{die.MapX * -1},");
        //        writer.Write($"{die.MapY * -1},");

        //        // Bin / Rank 값
        //        var binResult = result.BinningResult;

        //        // BinNo / BinLabel
        //        writer.Write($"{binResult?.BinNo},");
        //        writer.Write($"{binResult?.BinType},");
        //        writer.Write($"{binResult?.BinLabel},");

        //        foreach (var item in result.Items)
        //        {
        //            writer.Write($"{item.Value},");
        //        }

        //        // 신규 파일 헤더에 StrainGage 키를 넣은 경우에만 값도 함께 출력
        //        if (sgKeys.Count > 0 && die != null && die.MeasureValues != null)
        //        {
        //            foreach (var key in sgKeys)
        //            {
        //                double v;
        //                die.MeasureValues.TryGetValue(key, out v);
        //                writer.Write($"{v},");
        //            }
        //        }

        //        writer.WriteLine();
        //    }
        //    // ---------------------
        //    return 0;
        //}

        public int GetProbeIndexNo()
        {
            if (Rotary == null)
                return 0;

            int loadIndex = Rotary.GetLoadIndexNo();

            // 반시계 방향으로 2칸 이동
            int probeIndex = (loadIndex - this.Config.IndexOfProbe + Rotary.GetIndexCount()) % Rotary.GetIndexCount();

            return probeIndex;
        }

        private void LogSequence(string log)
        {
                if (this.CurrentFunc == null)
                    return;

                Log.Write(UnitName, this.CurrentFunc.Method.Name, $"[Sequence] {log}");
            
        }
        #endregion


        // 클래스 내부에 추가
        public void ResetForNewRun(bool waitRotaryIdle = true, bool rebindAxes = true)
        {
            // 1) 시퀀스/런타임 플래그 초기화
            RequestChipInsp = false;
            InspectDone = false;

            // 4) 계측기 상태 로그(준비 여부 확인)
            try
            {
                var equipment = Equipment.Instance;
                bool IsDryRunEqp = equipment.EquipmentConfig.IsDryRun;
                if (Config.IsSimulation == false && (Config.IsDryRun == false && IsDryRunEqp == false))
                {
                    if (!tester.CanMeasure())
                    {
                        Log.Write(UnitName, "[ResetForNewRun] PKGTester not ready (CanMeasure=false). Please check Test condition or Initialize state.");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Write(UnitName, $"[ResetForNewRun] Tester readiness check failed: {ex.Message}");
            }
        }

        // 짧게 평균한 스냅샷(노이즈 완화)
        private IDictionary<string, double> GetStrainGageSnapshotAveraged(int samples = 5, int interDelayMs = 2)
        {
            var dictSum = new Dictionary<string, (double sum, int cnt)>();
            try
            {
                var items = StrainGageMonitor?.Items;
                if (items == null || items.Count == 0) return new Dictionary<string, double>();

                for (int s = 0; s < Math.Max(1, samples); s++)
                {
                    for (int i = 0; i < items.Count; i++)
                    {
                        var sg = items[i].strainGage;
                        // 키 규칙: SG{n}_Voltage / SG{n}_Force
                        string kV = $"SG{i + 1}_Voltage";
                        string kF = $"SG{i + 1}_Force";

                        if (!dictSum.ContainsKey(kV)) dictSum[kV] = (0, 0);
                        if (!dictSum.ContainsKey(kF)) dictSum[kF] = (0, 0);

                        var v = sg.Voltage; // Zero 보정/LPF 반영됨
                        var f = sg.Force;   // Config 기반 환산 힘(단위는 프로젝트 규격)

                        dictSum[kV] = (dictSum[kV].sum + v, dictSum[kV].cnt + 1);
                        dictSum[kF] = (dictSum[kF].sum + f, dictSum[kF].cnt + 1);
                    }

                    if (interDelayMs > 0 && s < samples - 1)
                        Thread.Sleep(interDelayMs);
                }

                var avg = new Dictionary<string, double>();
                foreach (var kv in dictSum)
                    avg[kv.Key] = kv.Value.cnt > 0 ? (kv.Value.sum / kv.Value.cnt) : 0.0;

                return avg;
            }
            catch (Exception ex)
            {
                Log.Write(UnitName, $"[GetStrainGageSnapshotAveraged] {ex.Message}");
                return new Dictionary<string, double>();
            }
        }

        public int MoveToTeachingPositionBySelectionIndex(int teachingSelIndex, bool isFine = false)
        {
            if (Config == null)
                return -1;

            string tpName;
            if (!Config.GetTeachingPositionName(teachingSelIndex, out tpName) || string.IsNullOrWhiteSpace(tpName))
                return -1;

            IndexChipProberConfig.TeachingPositionName en;
            if (!Enum.TryParse(tpName, out en))
                return -1;

            switch (en)
            {
                // ===== AlignZ Index Up/Ready (Index1~8 -> 0~7) =====
                //case IndexChipProberConfig.TeachingPositionName.AlignZ_Index1:
                //    nIndex = 0; 
                //    return MovePositionAlignZUp(nIndex, isFine);

                default:
                    return -1;
            }

            //return 0;
        }

    }
}