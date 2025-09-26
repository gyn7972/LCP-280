using QMC.Common;
using QMC.Common.Cameras;
using QMC.Common.Cameras.HIKVISION;
using QMC.Common.Component;
using QMC.Common.DIO;
using QMC.Common.IO;
using QMC.Common.IOUtil;
using QMC.Common.Keithley;
using QMC.Common.Motion;
using QMC.Common.Motion.Ajin.HW;
using QMC.Common.Motion.Ajin.IO;
using QMC.Common.Motions;
using QMC.Common.Motions.CKD;
using QMC.Common.PKGTester;
using QMC.Common.Spectrometer;
using QMC.Common.StrainGage;
using QMC.Common.Unit;
using QMC.LCP_280.Process.Component;
using QMC.LCP_280.Process.Unit;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections.ObjectModel;
using static QMC.Common.Unit.BaseUnit;

namespace QMC.LCP_280.Process
{
    public class Equipment : IDisposable, IEquipment
    {
        public static class UnitKeys
        {
            public const string EquipmentStatus = "EquipmentStatus"; // [MOD] 상수화
            public const string IndexChipProbeController = "IndexChipProbeController";
            public const string IndexChipProber = "IndexChipProber";
            public const string IndexLoadAligner = "IndexLoadAligner";
            public const string IndexUnloadAligner = "IndexUnloadAligner";
            public const string InputCassetteLifter = "InputCassetteLifter";
            public const string InputDieTransfer = "InputDieTransfer";
            public const string InputFeeder = "InputFeeder";
            public const string InputStage = "InputStage";
            public const string InputStageEjector = "InputStageEjector";
            public const string OutputCassetteLifter = "OutputCassetteLifter";
            public const string OutputDieTransfer = "OutputDieTransfer";
            public const string OutputFeeder = "OutputFeeder";
            public const string OutputStage = "OutputStage";
            public const string Rotary = "Rotary";
            public const string GageRnR = "GageRnR";
        }

        #region Singleton Pattern

        private static Equipment _instance;
        private static readonly object _lock = new object();

        public static Equipment Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                            _instance = new Equipment();
                    }
                }
                return _instance;
            }
        }
        #endregion

        #region Fields & Properties

        /// <summary>
        /// 설비에 등록된 모든 Unit들
        /// </summary>
        public ConcurrentDictionary<string, IUnit> Units { get; private set; }

        /// <summary>
        /// Unit별 실행 상태 관리
        /// </summary>
        private ConcurrentDictionary<string, UnitExecutionInfo> _unitExecutions;

        // ==== Unit 상태 브로드캐스트 캐시 ====
        private readonly ConcurrentDictionary<string, UnitStatus> _lastBroadcastUnitStatus =
            new ConcurrentDictionary<string, UnitStatus>(StringComparer.OrdinalIgnoreCase);
        // ==================================

        /// <summary>
        /// 설비 전체 상태
        /// </summary>
        public EquipmentState EqState { get; set; }

        /// <summary>
        /// 설비 전체 Config 관리
        /// </summary>
        public EquipmentConfigManager ConfigManager { get; set; }

        /// <summary>
        /// 설비 전체 실행 취소 토큰
        /// </summary>
        private CancellationTokenSource _equipmentCancellationTokenSource;

        /// <summary>
        /// 설비 상태 변경 이벤트
        /// </summary>
        public event EventHandler<EquipmentStateChangedEventArgs> StateChanged;

        /// <summary>
        /// Unit 상태 변경 이벤트
        /// </summary>
        public event EventHandler<UnitStateChangedEventArgs> UnitStateChanged;

        /// <summary>
        /// 설비 오류 발생 이벤트
        /// </summary>
        public event EventHandler<EquipmentErrorEventArgs> ErrorOccurred;


        private AjinAxlBoardHost _axlHost = null;                 // Ajin 보드 수명 관리(AXL.Open/Close + MOT 로드)
        // ==== Motion 관리 ====
        private readonly MotionAxisManager _axisManager = new MotionAxisManager();
        //private readonly string _axisRoot = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Axes");
        private readonly string _axisRoot = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configs", "Axes");
        
        private DIOUnit _unitIO;
        private IDIODriver _dio;                     // AjinDioDriver (실기)
        private DioScanService _dioScan;                 // 주기 스캔(캐시)
        // I/O 설정 파일 루트 (원하는 경로로)
        private readonly string _dioRoot = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DIO");

        // (선택) 외부에서 축 매니저에 접근하고 싶으면 프로퍼티 제공
        public MotionAxisManager AxisManager => _axisManager;
        public DioScanService DioScan => _dioScan;
        public DIOUnit UnitIO => _unitIO;

        // [+] CKD Motor Driver (PDO Mapping) 추가
        private CKDMotorDriver _ckdDriver;
        
        // 기존: public HIKGigECamera Camera { get; set; } = null;
        public Dictionary<string, Camera> Cameras { get; } = new Dictionary<string, Camera>(StringComparer.OrdinalIgnoreCase);
        // === 편의 프로퍼티 추가 ===
        public HIKGigECamera IndexLoaderCam => GetCamera("Index_Loader");
        public HIKGigECamera InStageCam => GetCamera("In_Stage");
        public HIKGigECamera IndexProberCam => GetCamera("Index_Prober");
        public HIKGigECamera IndexUnloaderCam => GetCamera("Index_Unloader");
        public HIKGigECamera OutStageCam => GetCamera("Out_Stage");

        private HIKGigECamera GetCamera(string key)
        {
            return Cameras.TryGetValue(key, out var cam) ? cam as HIKGigECamera : null;
        }

        //카메라 사용 예
        //// 라이브 시작
        //Equipment.Instance.InStageCam?.StartLive();

        //// 라이브 정지
        //Equipment.Instance.InStageCam?.StopLive();

        //// Grab 한 장
        //var ret = Equipment.Instance.InStageCam?.Grab();
        //if (ret == MyCamera.MV_OK)
        //{
        //    var img = Equipment.Instance.InStageCam?.LatestImage;
        //        Console.WriteLine($"Grabbed {img.Width}x{img.Height}");
        //}

        // Sourcemeter
        public Dictionary<string, KeithleySourcemeter> Sourcemeters { get; } = new Dictionary<string, KeithleySourcemeter>(StringComparer.OrdinalIgnoreCase);
        // == 편의 프로퍼티 추가 ==
        public KeithleySourcemeter Sourcemeter => GetSourcemeter("Index_Prober_Sourcemeter");

        private KeithleySourcemeter GetSourcemeter(string key)
        {
            return Sourcemeters.TryGetValue(key, out var sm) ? sm : null;
        }

        // Spectrometer
        public Dictionary<string, CASSpectrometer> Spectrometers { get; } = new Dictionary<string, CASSpectrometer>(StringComparer.OrdinalIgnoreCase);
        // == 편의 프로퍼티 추가 ==
        public CASSpectrometer Spectrometer => GetSpectrometer("Index_Prober_Spectrometer");

        private CASSpectrometer GetSpectrometer(string key)
        {
            return Spectrometers.TryGetValue(key, out var sm) ? sm : null;
        }

        // PKG Tester
        public PKGTester Tester { get; private set; }

        // Strain Gage
        public Dictionary<string, StrainGage> StrainGages { get; } = new Dictionary<string, StrainGage>(StringComparer.OrdinalIgnoreCase);
        // == 편의 프로퍼티 추가 ==
        public StrainGage TopLeftGage => GetStrainGage("Top_Left_Gage");
        public StrainGage TopRightGage => GetStrainGage("Top_Right_Gage");
        public StrainGage BottomLeftGage => GetStrainGage("Bottom_Left_Gage");
        public StrainGage BottomRightGage => GetStrainGage("Bottom_Right_Gage");

        private StrainGage GetStrainGage(string key)
        {
            return StrainGages.TryGetValue(key, out var sg) ? sg : null;
        }

        public StrainGageMonitor StrainGageMonitor { get; } = new StrainGageMonitor("Index_Prober_StrainGage_Monitor");

        // [ADD] 생성여부 노출 (ProcessExit 등에서 강제 생성 방지)
        public static bool IsCreated => _instance != null;
        public static bool TryGet(out Equipment inst) 
        { 
            inst = _instance; 
            return inst != null; 
        }
        // [ADD] MotionStatusScanner 보관 (정상 Stop 위해)
        private MotionStatusScanner _motionStatusScanner;
        // [ADD] Dispose 재진입 방지
        private bool _disposed;

        #endregion

        #region Constructor & Initialization

        private Equipment()
        {

        }

        /// <summary>
        /// 설비 초기화
        /// </summary>
        public void InitializeEquipment()
        {
            try
            {
                Units = new ConcurrentDictionary<string, IUnit>();
                _unitExecutions = new ConcurrentDictionary<string, UnitExecutionInfo>();
                EqState = EquipmentState.Stopped;

                ConfigManager = new EquipmentConfigManager();
                EquipmentConfig = new EquipmentConfig();
                EquipmentRecipe = new EquipmentRecipe();
                
                OnStateChanged(EquipmentState.Initializing);

                // 여기서 모든 유닛 축을 직접 생성/로드하여 붙인다.
                BootstrapAxesDirect();

                BootstrapIODirect();

                // === 카메라 초기화 ===
                InitializeCameras();

                // === Sourcemeter 초기화 ===
                InitializeSourcemeters();

                // === Spectrometer 초기화 ===
                InitializeSpectrometers();

                // === PKG Tester 초기화 ===
                InitializePKGTester();

                // === Strain Gage 초기화 ===
                InitializeStrainGages();

                // 기본 Unit들 자동 등록 (개발자가 필요에 따라 추가)
                AutoRegisterUnits();

                BindUnit();

                // 3) 설비 Config + 메인 Recipe 로드
                EquipmentRecipe.InitGlobalRecipe();

                // [ADD] EquipmentStatus 즉시 시작 (장비 Ready 이전에 상태 수집 시작)
                try
                {
                    if (Units.ContainsKey(UnitKeys.EquipmentStatus))
                    {
                        var ok = StartUnitAsync(UnitKeys.EquipmentStatus).GetAwaiter().GetResult();
                        if (!ok)
                            OnErrorOccurred("EquipmentStatus Unit 시작 실패");
                    }
                    else
                    {
                        OnErrorOccurred("EquipmentStatus Unit 미등록 상태");
                    }
                }
                catch (Exception ex)
                {
                    OnErrorOccurred("EquipmentStatus Unit 시작 중 예외: " + ex.Message);
                }

                // 홈 후처리 글로벌 구독(1회)
                HomeHooks.EnsureSubscribed();

                OnStateChanged(EquipmentState.Ready);
            }
            catch (Exception ex)
            {
                OnStateChanged(EquipmentState.Error);
                OnErrorOccurred($"설비 초기화 중 오류 발생: {ex.Message}");
                //throw;
            }
        }

        private void BindUnit()
        {
            foreach(var v in Units)
            {
                (v.Value as BaseUnit)?.BindUnit();
            }
        }

        // === Protected Unit Helper (EquipmentStatus 는 항상 실행 유지) ===
        private bool IsProtectedUnit(string unitName)
            => string.Equals(unitName, UnitKeys.EquipmentStatus, StringComparison.OrdinalIgnoreCase);
        private bool IsProtectedUnit(IUnit unit) => unit is EquipmentStatus;

        #endregion


        #region Unit Registration

        /// <summary>
        /// 기본 Unit들 자동 등록 (필요 시 추가/삭제)
        /// </summary>
        private void AutoRegisterUnits()
        {
            // 중복 등록 방지: 이미 존재하면 스킵
            void TryAdd(IUnit u, string name)
            {
                if (!Units.ContainsKey(name))
                    RegisterUnit(u, name);
            }

            TryAdd(new InputCassetteLifter(), "InputCassetteLifter");
            TryAdd(new InputFeeder(), "InputFeeder");
            TryAdd(new InputStage(), "InputStage");
            TryAdd(new InputStageEjector(), "InputStageEjector");
            TryAdd(new InputDieTransfer(), "InputDieTransfer");
            TryAdd(new Rotary(), "Rotary");
            TryAdd(new IndexLoadAligner(), "IndexLoadAligner");
            TryAdd(new IndexChipProbeController(), "IndexChipProbeController");
            TryAdd(new IndexChipProber(), "IndexChipProber");
            TryAdd(new IndexUnloadAligner(), "IndexUnloadAligner");
            TryAdd(new OutputDieTransfer(), "OutputDieTransfer");
            TryAdd(new OutputStage(), "OutputStage");
            TryAdd(new OutputCassetteLifter(), "OutputCassetteLifter");
            TryAdd(new OutputFeeder(), "OutputFeeder");
            TryAdd(new GageRnR(), "GageRnR");
            TryAdd(new EquipmentStatus(), "EquipmentStatus"); // 신규 상태 유닛
        }

        /// <summary>
        /// Unit을 설비에 등록
        /// </summary>
        /// <param name="unit">등록할 Unit</param>
        /// <param name="unitName">Unit 이름</param>
        /// <param name="description">Unit 설명</param>
        public void RegisterUnit(IUnit unit, string unitName, string description = null)
        {
            
            if (unit == null) throw new ArgumentNullException(nameof(unit));
            if (string.IsNullOrEmpty(unitName)) throw new ArgumentException("Unit 이름이 필요합니다.", nameof(unitName));

            try
            {
                if (unit is BaseUnit bu)
                    bu.UnitName = unitName;

                if (Units.TryAdd(unitName, unit))
                {
                    // Unit 실행 정보 초기화
                    _unitExecutions[unitName] = new UnitExecutionInfo(unitName, description);

                    // Config 및 Recipe 등록
                    var cfg = (unit as BaseUnit)?.Config;
                    if (cfg != null)
                        ConfigManager.RegisterUnitConfig(unitName, cfg);
                    //RecipeManager.RegisterUnitRecipe(unitName, CreateUnitRecipe(unit));

                    Console.WriteLine($"Unit '{unitName}' 등록 완료");
                }
                else
                {
                    throw new InvalidOperationException($"Unit '{unitName}'는 이미 등록되어 있습니다.");
                }
            }
            catch (Exception ex)
            {
                OnErrorOccurred($"Unit '{unitName}' 등록 중 오류: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Unit 등록 해제
        /// </summary>
        /// <param name="unitName">등록 해제할 Unit 이름</param>
        public bool UnregisterUnit(string unitName)
        {
            try
            {
                if (IsProtectedUnit(unitName)) // [MOD] 보호 유닛 해제 금지
                    throw new InvalidOperationException("EquipmentStatus 는 해제할 수 없습니다.");

                // Unit이 실행 중이면 먼저 정지
                if (_unitExecutions.TryGetValue(unitName, out var execInfo) && execInfo.IsRunning)
                {
                    StopUnitAsync(unitName).GetAwaiter().GetResult();
                }

                // Unit 제거
                bool removed = Units.TryRemove(unitName, out var unit);
                if (removed)
                {
                    _unitExecutions.TryRemove(unitName, out _);
                    //ConfigManager.UnregisterUnitConfig(unitName);
                    //RecipeManager.UnregisterUnitRecipe(unitName); // 제거함으로써 비활성화

                    // Unit 리소스 정리
                    if (unit is IDisposable disposableUnit)
                    {
                        disposableUnit.Dispose();
                    }

                    Console.WriteLine($"Unit '{unitName}' 등록 해제 완료");
                }

                return removed;
            }
            catch (Exception ex)
            {
                OnErrorOccurred($"Unit '{unitName}' 등록 해제 중 오류: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Unit별 기본 Recipe 생성
        /// </summary>
        private BaseRecipe CreateUnitRecipe(IUnit unit)
        {
            // Unit 타입에 따라 적절한 Recipe 생성
            switch (unit)
            {
                case InputCassetteLifter cassetteUnit:
                    return new CassetteElevatorRecipe();
                default:
                    return new CassetteElevatorRecipe();
            }
        }

        #endregion

        #region Equipment Control

        /// <summary>
        /// 설비 전체 시작
        /// </summary>
        public async Task<bool> StartAllUnitsAsync()
        {
            if (EqState == EquipmentState.Running)
            {
                Console.WriteLine("설비가 이미 실행 중입니다.");
                return true;
            }

            try
            {
                OnStateChanged(EquipmentState.Starting);

                // 새로운 취소 토큰 생성
                _equipmentCancellationTokenSource?.Dispose();
                _equipmentCancellationTokenSource = new CancellationTokenSource();

                // [MOD] 1) 보호 유닛(EquipmentStatus) 먼저 기동 (실패해도 계속)
                if (Units.TryGetValue(UnitKeys.EquipmentStatus, out var statusUnit))
                    await StartUnitAsync(UnitKeys.EquipmentStatus);

                // 2) 나머지 유닛 병렬 기동
                var otherUnitNames = Units.Keys.Where(n => !IsProtectedUnit(n)).ToArray();
                var startTasks = otherUnitNames.Select(StartUnitAsync);
                var results = await Task.WhenAll(startTasks);

                // 모든 Unit이 성공적으로 시작되었는지 확인
                if (results.All(r => r))
                {
                    OnStateChanged(EquipmentState.Running);
                    Console.WriteLine("설비 전체 시작 완료");
                    return true;
                }
                else
                {
                    // [MOD] 실패 시에도 EquipmentStatus 는 유지 (StopAllUnitsAsync 호출 시 보호)
                    //await StopAllUnitsAsync(includeEquipmentStatus: false);
                    await StopAllUnitsAsync(includeEquipmentStatus: false).ConfigureAwait(false);
                    OnStateChanged(EquipmentState.Error);
                    OnErrorOccurred("설비 시작 중 일부 Unit 실패");
                    return false;
                }
            }
            catch (Exception ex)
            {
                OnStateChanged(EquipmentState.Error);
                OnErrorOccurred($"설비 시작 중 오류 발생: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 개별 Unit 시작
        /// </summary>
        /// <param name="unitName">시작할 Unit 이름</param>
        public async Task<bool> StartUnitAsync(string unitName)
        {
            if (!Units.TryGetValue(unitName, out var unitObj))
            {
                OnErrorOccurred($"Unit '{unitName}'를 찾을 수 없습니다.");
                return false;
            }
            if (!_unitExecutions.TryGetValue(unitName, out var execInfo))
            {
                OnErrorOccurred($"Unit '{unitName}' 실행 정보를 찾을 수 없습니다.");
                return false;
            }

            try
            {
                // [FIX] 실행 플래그와 실제 RunStatus 정합성 보정
                var bu = unitObj as QMC.Common.Unit.BaseUnit;
                if (execInfo.IsRunning)
                {
                    var rs = bu?.RunUnitStatus;
                    if (rs == UnitStatus.Stopped ||
                        rs == UnitStatus.Stopping ||
                        rs == UnitStatus.CycleStop)
                    {
                        // 실제로는 정지 상태인데 플래그만 Running이던 경우 정정
                        execInfo.IsRunning = false;
                        execInfo.StopTime = DateTime.Now;
                        SetAndRaiseUnitState(unitName, UnitStatus.Stopped);
                        //OnUnitStateChanged(unitName, UnitStatus.Stopped);
                    }
                    else
                    {
                        Console.WriteLine($"Unit '{unitName}'는 이미 실행 중입니다.");
                        return true;
                    }
                }

                SetAndRaiseUnitState(unitName, UnitStatus.Starting);
                //OnUnitStateChanged(unitName, UnitStatus.Starting);

                if (_equipmentCancellationTokenSource == null)
                    _equipmentCancellationTokenSource = new CancellationTokenSource();

                execInfo.CancellationTokenSource = IsProtectedUnit(unitObj)
                    ? new CancellationTokenSource()
                    : CancellationTokenSource.CreateLinkedTokenSource(_equipmentCancellationTokenSource.Token);


                execInfo.IsRunning = true;
                execInfo.StartTime = DateTime.Now;
                SetAndRaiseUnitState(unitName, UnitStatus.Running);
                //OnUnitStateChanged(unitName, UnitStatus.Running);

                (unitObj as BaseUnit)?.Start();

                return true;
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                SetAndRaiseUnitState(unitName, UnitStatus.Error);
                //OnUnitStateChanged(unitName, UnitStatus.Error);
                OnErrorOccurred($"Unit '{unitName}' 시작 중 오류: {ex.Message}");
                return false;
            }
        }
        // === 기존 StartUnitAsync 아래에 추가 (동기 Wrapper) ===
        public bool StartUnitSync(string unitName)
        {
            try
            {
                return StartUnitAsync(unitName).ConfigureAwait(false).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                OnErrorOccurred($"StartUnitSync 실패: {unitName} / {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Unit 실행 루프
        /// </summary>
        private async Task RunUnitLoopAsync(string unitName, BaseUnit unit, CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    // Unit별 주기적 작업 수행
                    await PerformUnitCycle(unit, cancellationToken);

                    // 100ms 대기 (Unit별로 다르게 설정 가능)
                    await Task.Delay(100, cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                // 정상적인 취소
                Console.WriteLine($"Unit '{unitName}' 정상 정지됨");
            }
            catch (Exception ex)
            {
                OnUnitStateChanged(unitName, UnitStatus.Error);
                Log.Write(ex);
                OnErrorOccurred($"Unit '{unitName}' 실행 중 오류: {ex.Message}");
            }
            finally
            {
                // [MOD] 보호 유닛은 외부 Stop 트리거 없으면 OnStop 호출 없이 유지되도록
                if (!IsProtectedUnit(unitName))
                {
                    try
                    {
                        unit.OnStop();
                        OnUnitStateChanged(unitName, UnitStatus.Stopped);
                    }
                    catch (Exception ex)
                    {
                        OnErrorOccurred($"Unit '{unitName}' 정지 중 오류: {ex.Message}");
                    }
                }

                // 실행 정보 정리
                if (_unitExecutions.TryGetValue(unitName, out var execInfo))
                {
                    execInfo.IsRunning = false;
                    execInfo.StopTime = DateTime.Now;
                    execInfo.CancellationTokenSource?.Dispose();
                    execInfo.CancellationTokenSource = null;
                }
            }
        }

        /// <summary>
        /// Unit별 주기적 작업 수행
        /// </summary>
        private async Task PerformUnitCycle(BaseUnit unit, CancellationToken cancellationToken)
        {
            switch (unit)
            {
                case InputCassetteLifter cassetteUnit:
                    await PerformCassetteElevatorCycle(cassetteUnit, cancellationToken);
                    break;
                case EquipmentStatus statusUnit:
                    statusUnit.Refresh();
                    await Task.Delay(1, cancellationToken);
                    break;
                default:
                    await Task.Delay(1, cancellationToken);
                    break;
            }
        }

        /// <summary>
        /// CassetteLoadingElevator 주기적 작업
        /// </summary>
        private async Task PerformCassetteElevatorCycle(InputCassetteLifter unit, CancellationToken cancellationToken)
        {
            // 실제 설비 로직에 맞게 구현
            // 예: 센서 체크, 위치 확인, 에러 체크 등
            await Task.Delay(1, cancellationToken);
        }

        /// <summary>
        /// 설비 전체 정지 (인터페이스 기본 호출용 – EquipmentStatus 포함)
        /// </summary>
        public System.Threading.Tasks.Task<bool> StopAllUnitsAsync()
        {
            // IEquipment 인터페이스(매개변수 없는 버전) 요구 충족용 래퍼
            return StopAllUnitsAsync(includeEquipmentStatus: true);
        }
        /// <summary>
        /// 설비 전체 정지
        /// </summary>
        public async Task<bool> StopAllUnitsAsync(bool includeEquipmentStatus = true)
        {
            try
            {
                OnStateChanged(EquipmentState.Stopping);
                _equipmentCancellationTokenSource?.Cancel();

                var stopTasks = new List<Task<bool>>();

                foreach (var kvp in _unitExecutions)
                {
                    var unitName = kvp.Key;
                    var execInfo = kvp.Value;

                    if (IsProtectedUnit(unitName) && !includeEquipmentStatus)
                        continue;

                    if (execInfo.IsRunning)
                    {
                        SetAndRaiseUnitState(unitName, UnitStatus.Stopping);
                        //OnUnitStateChanged(unitName, UnitStatus.Stopping);

                        // [FIX] 실제 유닛도 멈추도록 호출
                        if (Units.TryGetValue(unitName, out var u))
                            (u as QMC.Common.Unit.BaseUnit)?.Stop();

                        execInfo.CancellationTokenSource?.Cancel();
                        execInfo.IsRunning = false;
                        execInfo.StopTime = DateTime.Now;

                        if (execInfo.ExecutionTask != null)
                            stopTasks.Add(WaitForUnitStopAsync(unitName, execInfo.ExecutionTask));

                        // [FIX] 즉시 Stopped 알림 (Task 루프 미사용 구조 대응)
                        SetAndRaiseUnitState(unitName, UnitStatus.Stopped);
                        //OnUnitStateChanged(unitName, UnitStatus.Stopped);
                    }
                }

                if (stopTasks.Count > 0)
                    await Task.WhenAll(stopTasks).ConfigureAwait(false);

                OnStateChanged(EquipmentState.Stopped);
                Console.WriteLine("설비 전체 정지 완료");
                return true;
            }
            catch (Exception ex)
            {
                OnStateChanged(EquipmentState.Error);
                OnErrorOccurred($"설비 정지 중 오류 발생: {ex.Message}");
                return false;
            }
        }
        // 기존 Equipment 구현(이미 public) + IDisposable
        //public async System.Threading.Tasks.Task<bool> StopAllUnitsAsync() => await StopAllUnitsAsync(); // 기존 메서드 연결
        // Dispose()는 기존 구현 사용
        /// <summary>
        /// 개별 Unit 정지
        /// </summary>
        /// <param name="unitName">정지할 Unit 이름</param>
        public async Task<bool> StopUnitAsync(string unitName)
        {
            if (!Units.TryGetValue(unitName, out var unitObj))
            {
                OnErrorOccurred($"Unit '{unitName}'를 찾을 수 없습니다.");
                return false;
            }

            if (IsProtectedUnit(unitName))
            {
                Console.WriteLine("EquipmentStatus 는 정지 대상에서 제외 (요청 무시).");
                return true;
            }

            if (!_unitExecutions.TryGetValue(unitName, out var execInfo))
            {
                OnErrorOccurred($"Unit '{unitName}'를 찾을 수 없습니다.");
                return false;
            }

            try
            {
                if (execInfo.IsRunning)
                    SetAndRaiseUnitState(unitName, UnitStatus.Stopping);

                (unitObj as QMC.Common.Unit.BaseUnit)?.Stop();

                execInfo.IsRunning = false;
                execInfo.StopTime = DateTime.Now;


                SetAndRaiseUnitState(unitName, UnitStatus.Stopping);
                SetAndRaiseUnitState(unitName, UnitStatus.Stopped);
                //OnUnitStateChanged(unitName, UnitStatus.Stopping);
                //OnUnitStateChanged(unitName, UnitStatus.Stopped);

                Console.WriteLine($"Unit '{unitName}' 정지 완료");
                return true;
            }
            catch (Exception ex)
            {
                SetAndRaiseUnitState(unitName, UnitStatus.Error);
                //OnUnitStateChanged(unitName, UnitStatus.Error);
                OnErrorOccurred($"Unit '{unitName}' 정지 중 오류: {ex.Message}");
                return false;
            }
        }
        public bool StopUnitSync(string unitName)
        {
            try
            {
                return StopUnitAsync(unitName).ConfigureAwait(false).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                OnErrorOccurred($"StartUnitSync 실패: {unitName} / {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Unit 정지 완료 대기
        /// </summary>
        private async Task<bool> WaitForUnitStopAsync(string unitName, Task executionTask)
        {
            try
            {
                var timeoutTask = Task.Delay(5000);
                var completedTask = await Task.WhenAny(executionTask, timeoutTask);

                if (completedTask == timeoutTask)
                {
                    OnErrorOccurred($"Unit '{unitName}' 정지 타임아웃");

                    SetAndRaiseUnitState(unitName, UnitStatus.Error);
                    //OnUnitStateChanged(unitName, UnitStatus.Error);
                    return false;
                }

                Console.WriteLine($"Unit '{unitName}' 정지 완료");
                return true;
            }
            catch (Exception ex)
            {
                SetAndRaiseUnitState(unitName, UnitStatus.Error);
                //OnUnitStateChanged(unitName, UnitStatus.Error);
                OnErrorOccurred($"Unit '{unitName}' 정지 중 오류: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 전체 또는 특정 Unit의 상태를 확인하여 신규 연결 시도
        /// </summary>
        public void RefreshUnitConnections()
        {
            foreach (var unit in Units.Values)
            {
                // 비정상 상태는 무시
                if (unit is EquipmentStatus) continue;
                if (_unitExecutions.TryGetValue((unit as BaseUnit)?.UnitName, out var execInfo) && execInfo.IsRunning)
                    continue;

                try
                {
                    (unit as BaseUnit)?.OnStop();
                    (unit as BaseUnit)?.OnRun();
                }
                catch (Exception ex)
                {
                    OnErrorOccurred($"Unit '{(unit as BaseUnit)?.UnitName}' 연결 상태 확인 중 오류: {ex.Message}");
                }
            }
        }

        // ===== Config & Recipe ===== (원본 동일)
        public T GetUnitConfig<T>(string unitName) where T : class => ConfigManager.GetUnitConfig<T>(unitName);
        //public T GetUnitRecipe<T>(string unitName) where T : BaseRecipe => RecipeManager.GetUnitRecipe<T>(unitName);
        public void SetUnitConfig(string unitName, object config)
        {
            if (config is BaseConfig baseConfig) ConfigManager.SetUnitConfig(unitName, baseConfig);
        }
        //public void SetUnitRecipe(string unitName, BaseRecipe recipe) => RecipeManager.SetUnitRecipe(unitName, recipe);
        public bool SaveAllConfigs(string directoryPath = null) => ConfigManager.SaveAllConfigs(directoryPath);
        public bool LoadAllConfigs(string directoryPath = null) => ConfigManager.LoadAllConfigs(directoryPath);
        //public bool SaveAllRecipes(string directoryPath = null) => RecipeManager.SaveAllRecipes(directoryPath);
        //public bool LoadAllRecipes(string directoryPath = null) => RecipeManager.LoadAllRecipes(directoryPath);

        // ===== Status =====
        public Dictionary<string, UnitStatusInfo> GetAllUnitStatus()
        {
            var result = new Dictionary<string, UnitStatusInfo>();
            if (Units == null) return result;

            foreach (var kv in Units)
            {
                var name = kv.Key;
                var unit = kv.Value as BaseUnit;

                UnitExecutionInfo exec = null;
                _unitExecutions?.TryGetValue(name, out exec);

                int compCount = 0;
                try
                {
                    // Components 가 null 이거나 접근 중 예외 발생 가능성 대응
                    var comps = unit?.Components;
                    compCount = comps != null ? comps.Count : 0;
                }
                catch (Exception ex)
                {
                    // 개별 Unit 문제는 전체 중단시키지 말고 1개만 0 처리
                    Log.Write("Equipment", $"[WARN] GetAllUnitStatus ComponentCount '{name}' : {ex.Message}");
                    compCount = 0;
                }

                result[name] = new UnitStatusInfo
                {
                    UnitName = name,
                    Description = exec?.Description ?? "",
                    IsRunning = exec?.IsRunning ?? false,
                    RunUnitStatus = GetUnitCurrentState(name),
                    ComponentCount = compCount,
                    StartTime = exec?.StartTime,
                    RunningTime = (exec?.IsRunning == true && exec.StartTime.HasValue)
                                        ? DateTime.Now - exec.StartTime.Value
                                        : TimeSpan.Zero,
                    LastUpdateTime = DateTime.Now
                };
            }
            return result;
        }

        public List<string> GetRegisteredUnitNames() => Units?.Keys.ToList() ?? new List<string>();

        // GetUnitCurrentState 우선 BaseUnit.RunUnitStatus 사용하도록 재정렬 (이미 유사 로직 있으나 명확화)
        private UnitStatus GetUnitCurrentState(string unitName)
        {
            if (Units != null && Units.TryGetValue(unitName, out var u) && u is BaseUnit bu)
            {
                if (bu.RunUnitStatus != UnitStatus.Unknown)
                    return bu.RunUnitStatus;
            }

            if (_unitExecutions != null && _unitExecutions.TryGetValue(unitName, out var exec))
                return exec.IsRunning ? UnitStatus.Running : UnitStatus.Stopped;

            return UnitStatus.Unknown;

            //if (_unitExecutions.TryGetValue(unitName, out var exec))
            //{
            //    if (exec.IsRunning) 
            //        return UnitStatus.Running;

            //    else if (exec.ExecutionTask?.IsFaulted == true)
            //        return UnitStatus.Error;

            //    else
            //        return UnitStatus.Stopped;
            //}
            //return UnitStatus.Unknown;
        }

        #endregion

        #region Events

        private void OnStateChanged(EquipmentState newState)
        {
            var old = EqState;
            EqState = newState;
            StateChanged?.Invoke(this, new EquipmentStateChangedEventArgs(old, newState));
        }



        /// <summary>
        /// Unit 상태를 실제 객체(BaseUnit), 실행정보(UnitExecutionInfo)에 반영 후 이벤트 발행.
        /// (외부에서 상태 올릴 때 이 함수만 호출하도록 정리)
        /// </summary>
        public void SetAndRaiseUnitState(string unitName, UnitStatus newState)
        {
            if (string.IsNullOrWhiteSpace(unitName))
                return;

            // 1) BaseUnit 반영
            if (Units != null && Units.TryGetValue(unitName, out var u) && u is BaseUnit bu)
            {
                if (bu.RunUnitStatus != newState)
                    bu.RunUnitStatus = newState;

                switch (newState)
                {
                    case UnitStatus.Starting:
                        bu.RunUnitStatus = UnitStatus.Starting;
                        break;
                    case UnitStatus.Running:
                        bu.RunUnitStatus = UnitStatus.Running;
                        //bu.IsRunning = true;
                        break;
                    case UnitStatus.Stopping:
                        bu.RunUnitStatus = UnitStatus.Stopping;
                        break;
                    case UnitStatus.CycleStop:
                        bu.RunUnitStatus = UnitStatus.CycleStop;
                        break;
                    case UnitStatus.Stopped:
                        bu.RunUnitStatus = UnitStatus.Stopped;
                        break;
                    case UnitStatus.Error:
                        bu.RunUnitStatus = UnitStatus.Error;
                        break;
                    case UnitStatus.Unknown:
                        bu.RunUnitStatus = UnitStatus.Unknown;
                        //bu.IsRunning = false;
                        break;
                }
            }

            // 2) 실행정보 반영
            if (_unitExecutions != null && _unitExecutions.TryGetValue(unitName, out var exec))
            {
                bool running = (newState == UnitStatus.Running || newState == UnitStatus.Starting);
                if (exec.IsRunning != running)
                {
                    exec.IsRunning = running;
                    if (running)
                        exec.StartTime = DateTime.Now;
                    else
                        exec.StopTime = DateTime.Now;
                }
            }

            // 3) 중복 상태면 이벤트 생략
            if (_lastBroadcastUnitStatus.TryGetValue(unitName, out var last) && last == newState)
                return;
            _lastBroadcastUnitStatus[unitName] = newState;

            // 4) 실제 이벤트
            OnUnitStateChanged(unitName, newState);
        }
        private void OnUnitStateChanged(string unitName, UnitStatus newState)
        {
            // 순수 이벤트 브로드캐스트(상태 동기화는 SetAndRaiseUnitState에서 처리)
            UnitStateChanged?.Invoke(this, new UnitStateChangedEventArgs(unitName, newState));
        }

        private void OnErrorOccurred(string msg)
        {
            ErrorOccurred?.Invoke(this, new EquipmentErrorEventArgs(msg));
            Console.WriteLine($"Equipment Error: {msg}");
        }

        #endregion

        #region Dispose

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;
            //if (!_disposed) return;
            _disposed = true;

            try
            {
                // 1) 일반 유닛 정지 (EquipmentStatus 제외)
                try 
                { 
                    StopAllUnitsAsync(includeEquipmentStatus: true).GetAwaiter().GetResult(); 
                } 
                catch { }
                // 2) 보호 유닛 강제 정지
                ForceStopEquipmentStatus();

                // MotionStatusScanner 중지
                try { _motionStatusScanner?.Stop(); } catch { }
                _motionStatusScanner = null;

                // CKD Driver
                try { _ckdDriver?.Dispose(); } catch { }
                _ckdDriver = null;

                // DIO Scan
                try
                {
                    _dioScan?.Stop();
                    _dioScan?.Dispose();
                }
                catch { }
                _dioScan = null;

                // DIO Driver
                if (_dio is IDisposable dioDisp)
                {
                    try { dioDisp.Dispose(); } catch { }
                }
                _dio = null;

                // Ajin Host
                try { _axlHost?.Close(); } catch { }
                _axlHost = null;

                // Cameras
                try
                {
                    foreach (var cam in Cameras.Values)
                    {
                        try { cam.StopLive(); cam.Close(); } catch { }
                    }
                }
                catch { }
                Cameras.Clear();

                // Sourcemeters
                try
                {
                    foreach (var sm in Sourcemeters.Values)
                        try { (sm as IDisposable)?.Dispose(); } catch { }
                }
                catch { }
                Sourcemeters.Clear();

                // Spectrometers
                try
                {
                    foreach (var spc in Spectrometers.Values)
                        try { (spc as IDisposable)?.Dispose(); } catch { }
                }
                catch { }
                Spectrometers.Clear();

                // Units
                try
                {
                    if (Units != null)
                    {
                        foreach (var u in Units.Values)
                            if (u is IDisposable du) { try { du.Dispose(); } catch { } }
                        Units.Clear();
                    }
                }
                catch { }

                // Cancellation
                try
                {
                    _equipmentCancellationTokenSource?.Cancel();
                    _equipmentCancellationTokenSource?.Dispose();
                }
                catch { }
                _equipmentCancellationTokenSource = null;

                // Execution infos
                try
                {
                    if (_unitExecutions != null)
                    {
                        foreach (var exec in _unitExecutions.Values)
                        {
                            try { exec.CancellationTokenSource?.Cancel(); } catch { }
                            try { exec.CancellationTokenSource?.Dispose(); } catch { }
                            try { exec.ExecutionTask?.Dispose(); } catch { }
                        }
                        _unitExecutions.Clear();
                    }
                }
                catch { }
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
        }

        // [MOD] 보호 유닛 강제 종료 전용
        private void ForceStopEquipmentStatus()
        {
            if (_unitExecutions == null) 
                return;

            if (_unitExecutions.TryGetValue(UnitKeys.EquipmentStatus, out var exec))
            {
                try
                {
                    exec.CancellationTokenSource?.Cancel();
                    exec.ExecutionTask?.Wait(2000);
                }
                catch { }
                finally
                {
                    exec.IsRunning = false;
                    exec.CancellationTokenSource?.Dispose();
                    exec.CancellationTokenSource = null;
                }
            }
        }

        #endregion

        #region Motion/IO Bootstrap (unchanged logic trimmed)
        private void BootstrapAxesDirect()
        {
            if (_axlHost == null)
            {
                var motPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "LCP-280.mot");
                _axlHost = new AjinAxlBoardHost(motPath);

                if (File.Exists(motPath))
                {
                    try
                    {
                        _axlHost.Open();
                    }
                    catch (Exception ex)
                    {

                        Log.Write(ex);
                    }
                }
            }
            if (_ckdDriver == null)
            {
                _ckdDriver = new CKDMotorDriver("CKD_DD_MotorDriver");
                _ckdDriver.StartReadInputDataMonitoring();
            }
            Directory.CreateDirectory(_axisRoot);
            CreateAxes();
            ClearAllMotionAlarmsOnStartup();
            ServoOnAllAxesOnStartup();
            ApplyMotionParamsForAllAxes();

            // [MOD] 기존 지역 변수 scanner → 필드 보관
            if (_motionStatusScanner == null)
            {
                _motionStatusScanner = new MotionStatusScanner(_axisManager, periodMs: 20);
                _motionStatusScanner.AxisStatusUpdated += (axis, status) => { /* 필요 시 이벤트 처리 */ };
                _motionStatusScanner.Start();
            }
            //var scanner = new MotionStatusScanner(_axisManager, periodMs: 20);
            //scanner.AxisStatusUpdated += (axis, status) => { };
            //scanner.Start();
        }

        private void ClearAllMotionAlarmsOnStartup()
        {
            try
            {
                var axes = _axisManager.GetAll();
                foreach (var axis in axes)
                {
                    try
                    {
                        var st = axis.GetStatusSnapshot();
                        if (st.IO.Alarm)
                        {
                            axis.ClearAlarm();
                            Thread.Sleep(5);
                        }
                    }
                    catch (Exception ex) { Log.Write(ex); }
                }
            }
            catch (Exception ex) { Log.Write(ex); }
        }

        private void ServoOnAllAxesOnStartup()
        {
            try
            {
                var axes = _axisManager.GetAll();
                foreach (var axis in axes)
                {
                    try
                    {
                        var st = axis.GetStatusSnapshot();
                        if (st.IO.Alarm) continue;
                        if (st.IO.ServoOn) continue;
                        int rc = axis.Servo(true);
                        if (rc != 0) Log.Write("Equipment", $"[ServoOn] Axis='{axis.Name}' 실패 rc={rc}");
                        Thread.Sleep(5);
                    }
                    catch (Exception ex) { Log.Write(ex); }
                }
            }
            catch (Exception ex) { Log.Write(ex); }
        }

        private void ApplyMotionParamsForAllAxes()
        {
            try
            {
                var axes = _axisManager.GetAll();
                foreach (var axis in axes)
                {
                    try
                    {
                        var st = axis.GetStatusSnapshot();
                        if (!st.IO.ServoOn) continue;
                        Thread.Sleep(1);
                    }
                    catch (Exception ex) { Log.Write(ex); }
                }
            }
            catch (Exception ex) { Log.Write(ex); }
        }

        private void BootstrapIODirect()
        {
            if (_dio == null)
            {
                AjinDioDriver.ModuleMapper map = (b, p) => b * 8 + p;
                _dio = new AjinDioDriver(map);
            }
            if (_dioScan == null)
            {
                Directory.CreateDirectory(_dioRoot);
                var setupPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configs", "Unit.dio.setup.json");
                _unitIO = DIOUnit.LoadOrCreateDefault(setupPath, "Unit", 32, 32, "DB64R");
                _dioScan = new DioScanService(_unitIO, _dio);
                _dioScan.Start(10);
            }
            IoBindings.RegisterAll();

            bool bIsOpen = false;

            try
            {
                bIsOpen = _axlHost.IsOpen;
            }catch (Exception ex)
            {
                Log.Write(ex);
            }
            //if (_axlHost != null && !bIsOpen)
            if (!bIsOpen)
            {
                var mb = new MessageBoxOk();
                mb.ShowDialog("Error!", "MOTION, I/O INIT FAIL.");
            }
        }

        private void CreateAxes()
        {
            try
            {
                const string unitName = "Unit";
                var boardNo = 0;
                for (int i = 0; i < AxisNames.AllInOrder.Length; i++)
                {
                    var name = AxisNames.AllInOrder[i];
                    var axis = CreateOrLoadAxis(unitName, name, axisNo: i, boardNo: boardNo);
                    _axisManager.Register(unitName, axis);
                    AttachAxisToUnit(unitName, "Axis_" + i, axis);
                }
            }
            catch (Exception ex) { Log.Write(ex); }
        }

        private MotionAxis CreateOrLoadAxis(string unitName, string axisName, int axisNo, int boardNo)
        {
            var dir = Path.Combine(_axisRoot, unitName);
            Directory.CreateDirectory(dir);
            var setupPath = Path.Combine(dir, axisName + ".setup.json");
            var configPath = Path.Combine(dir, axisName + ".config.json");
            MotionAxisSetup setup = File.Exists(setupPath) ? MotionAxisSetup.LoadOrCreate(setupPath, indented: true, backfill: true) : new MotionAxisSetup
            {
                Name = axisName,
                AxisNo = axisNo,
                BoardNo = boardNo,
                PulsesPerUnit = 1000,
                SoftLimitEnable = true,
                SoftLimitMin = -10,
                SoftLimitMax = 310
            };
            if (!File.Exists(setupPath)) setup.TrySave(setupPath, out _);
            MotionAxisConfig config = File.Exists(configPath) ? MotionAxisConfig.LoadOrCreate(configPath, indented: true, backfill: true) : new MotionAxisConfig
            {
                MaxVelocity = 10,
                RunAcc = 20,
                RunDec = 20,
                InposTolerance = 0.002,
                ProfileMode = ProfileMode.SCurve,
                AccJerkPercent = 50,
                DecJerkPercent = 50
            };
            if (!File.Exists(configPath)) config.TrySave(configPath, out _);
            if (axisName != "Index T Axis")
            {
                var driver = new AjinDriver(boardNo, setup.PulsesPerUnit, useLogicalUnits: true);
                return new MotionAxis(setup, config, driver);
            }
            else
            {
                var driver = _ckdDriver;
                return new MotionAxis(setup, config, driver);
            }
        }

        private void AttachAxisToUnit(string unitName, string targetMemberName, MotionAxis axis)
        {
            if (!Units.TryGetValue(unitName, out var unitObj)) { OnErrorOccurred("AttachAxisToUnit: Unit '" + unitName + "' not found."); return; }
            var unit = unitObj as BaseUnit;
            if (unit == null) { OnErrorOccurred("AttachAxisToUnit: Unit object is not BaseUnit."); return; }

            var t = unit.GetType();
            var p = t.GetProperty(targetMemberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (p != null && p.CanWrite && p.PropertyType == typeof(MotionAxis)) { p.SetValue(unit, axis); return; }
            var f = t.GetField(targetMemberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (f != null && f.FieldType == typeof(MotionAxis)) { f.SetValue(unit, axis); return; }
            var axesProp = t.GetProperty("Axes", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (axesProp != null && typeof(System.Collections.IDictionary).IsAssignableFrom(axesProp.PropertyType))
            { var dict = axesProp.GetValue(unit) as System.Collections.IDictionary; if (dict != null) { dict[axis.Name] = axis; return; } }
            var axesField = t.GetField("Axes", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (axesField != null && typeof(System.Collections.IDictionary).IsAssignableFrom(axesField.FieldType))
            { var dict = axesField.GetValue(unit) as System.Collections.IDictionary; if (dict != null) { dict[axis.Name] = axis; return; } }
            Console.WriteLine("AttachAxisToUnit: '" + unitName + "'에 '" + targetMemberName + "' 또는 Axes 딕셔너리가 없어 축 주입 실패.");
        }

        private void InitializeCameras(bool connect = true)
        {
            try
            {
                var map = new Dictionary<string, string>
                {
                    { "In_Stage","DA7500464" },
                    { "Index_Loader","00G97588297" },
                    { "Index_Aligner","DA7484884" },
                    { "Index_Prober","DA7484883" },
                    { "Index_Unloader","DA7484882" },
                    { "Out_Stage","DA7500465" }
                };

                foreach (var kv in map)
                {
                    var name = kv.Key;
                    var selector = kv.Value;
                    CameraConfig cfg;
                    try { cfg = CameraConfig.LoadOrCreate(name); }
                    catch (Exception exCfg) { Log.Write("Equipment", $"[Camera] config load fail '{name}': {exCfg.Message}"); continue; }
                    if (!connect) continue;

                    try
                    {
                        var cam = new HIKGigECamera(name) { CameraConfig = cfg };
                        int ret = cam.OpenBySelectorOrConfig(selector);
                        cam.StartLive();
                        Cameras[name] = cam;
                        Console.WriteLine($"[Camera] {name} ready");
                    }
                    catch (Exception ex) { Log.Write(ex); }
                }
            }
            catch (Exception ex) { Log.Write(ex); }
        }

        private void InitializeSourcemeters()
        {
            var list = new List<string> { "Index_Prober_Sourcemeter" };
            foreach (var name in list)
            {
                try
                {
                    var smu = new KeithleySourcemeter(name);
                    int ret = smu.Config.Load();
                    if (ret != 0)
                    {
                        Log.Write("Equipment", $"[Sourcemeter] '{name}' config load failed rc=0x{ret:X8}");
                        smu.Config.Reset();
                        smu.Config.Save();
                    }
                    ret = smu.Initialize();
                    if (ret != 0)
                    {
                        MessageBox.Show($"Sourcemeter [{smu.Name}] initialize NG.");
                    }
                    Sourcemeters[name] = smu;
                    Console.WriteLine($"[Sourcemeter] {name} ready");
                }
                catch (Exception ex) { Log.Write(ex); }
            }
        }

        private void InitializeSpectrometers()
        {
            var list = new List<string> { "Index_Prober_Spectrometer" };
            foreach (var name in list)
            {
                try
                {
                    var spc = new CASSpectrometer(name);
                    int ret = spc.Config.Load();
                    if (ret != 0)
                    {
                        Log.Write("Equipment", $"[Spectrometer] '{name}' config load failed rc=0x{ret:X8}");
                        spc.Config.Reset();
                        spc.Config.Save();
                    }
                    ret = spc.Initialize();
                    if (ret != 0)
                    {
                        MessageBox.Show($"Spectrometer [{spc.Name}] initialize NG.");
                    }
                    Spectrometers[name] = spc;
                    Console.WriteLine($"[Spectrometer] {name} ready");
                }
                catch (Exception ex) { Log.Write(ex); }
            }
        }

        private void InitializePKGTester()
        {
            try { Tester = new PKGTester("PKGTester", Sourcemeter, Spectrometer); }
            catch (Exception ex) { Log.Write(ex); }
        }

        private void InitializeStrainGages()
        {
            var list = new List<string> { "Top_Left_Gage", "Top_Right_Gage", "Bottom_Left_Gage", "Bottom_Right_Gage" };
            foreach (var name in list)
            {
                try
                {
                    var gage = new StrainGage(name);
                    int ret = gage.Config.Load();
                    if (ret != 0)
                    {
                        Log.Write("Equipment", $"[StrainGage] '{name}' config load failed rc=0x{ret:X8}");
                        gage.Config.Reset();
                        gage.Config.Save();
                    }
                    ret = gage.Initialize();
                    if (ret != 0)
                    {
                        MessageBox.Show($"StrainGage [{gage.Name}] initialize NG.");
                    }
                    StrainGages[name] = gage;
                    Console.WriteLine($"[StrainGage] {name} ready");
                }
                catch (Exception ex) { Log.Write(ex); }
            }

            foreach (var gage in StrainGages.Values)
            {
                try
                {
                    if (!StrainGageMonitor.Add(gage))
                        throw new Exception("StrainGageMonitor Add fail");

                    //StrainGageMonitor.Start();
                    Console.WriteLine($"[StrainGage] {gage.Name} ready");
                }
                catch (Exception ex) { Log.Write(ex); }
            }
        }

        public BaseUnit GetUnit(string name)
        {
            Units.TryGetValue(name, out var unit);
            return unit as BaseUnit;
        }
        #endregion // Motion/IO Bootstrap


        // ===== 메인 설비 Config / Recipe 로드 추가 =====
        public  EquipmentConfig EquipmentConfig { get;  set; }
        public  EquipmentRecipe EquipmentRecipe { get;  set; }
        public string ICurrentRecipe { get; set; }

        //private void LoadEquipmentConfigAndMainRecipe()
        //{
        //    try
        //    {
        //        // (1) 설비 Config
        //        EquipmentConfig = EquipmentConfig.LoadOrCreate();
        //        _CurrentRecipeName = EquipmentConfig?.CurrentRecipeName ?? "LCP_RECIPE";

        //        // (2) 메인 Measurement Recipe
        //        MeasurementRecipe loaded = null;
        //        try
        //        {
        //            //var br = RecipeManager.LoadOrCreate(typeof(MeasurementRecipe), _CurrentRecipeName) as QMC.Common.BaseRecipe;
        //            var obj = RecipeManager.LoadOrCreate(typeof(MeasurementRecipe), _CurrentRecipeName);
        //            loaded = obj as MeasurementRecipe;
        //        }
        //        catch (Exception rex)
        //        {
        //            OnErrorOccurred("MeasurementRecipe 로드 실패: " + rex.Message);
        //        }

        //        if (loaded == null)
        //        {
        //            // 실패 시 기본 생성
        //            loaded = new MeasurementRecipe(_CurrentRecipeName);
        //            loaded.Reset();
        //            try { loaded.Save(); } catch { }
        //        }

        //        CurrentRecipe = loaded;

        //        Console.WriteLine($"[Recipe] Main Recipe='{CurrentRecipe.Name}' 로드 완료");
        //    }
        //    catch (Exception ex)
        //    {
        //        OnErrorOccurred("LoadEquipmentConfigAndMainRecipe 실패: " + ex.Message);
        //    }
        //}
        //// 메인 Recipe 교체/저장 편의 메서드 (필요 시 UI에서 호출)
        //public static bool ChangeCurrentRecipe(string newName)
        //{
        //    if (string.IsNullOrWhiteSpace(newName))
        //        return false;
        //    try
        //    {
        //        var sanitized = newName.Trim();
        //        var obj = RecipeManager.LoadOrCreate(typeof(MeasurementRecipe), sanitized) as MeasurementRecipe;
        //        if (obj == null)
        //        {
        //            obj = new MeasurementRecipe(sanitized);
        //            obj.Reset();
        //            obj.Save();
        //        }
        //        CurrentRecipe = obj;
        //        _CurrentRecipeName = sanitized;

        //        if (EquipmentConfig == null)
        //            EquipmentConfig = EquipmentConfig.LoadOrCreate();
        //        EquipmentConfig.CurrentRecipeName = sanitized;
        //        EquipmentConfig.Save();

        //        Console.WriteLine($"[Recipe] 변경 및 저장: {sanitized}");
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        //OnErrorOccurred("ChangeCurrentRecipe 오류: " + ex.Message);
        //        Log.Write(ex);
        //        return false;
        //    }
        //}
        //public static bool SaveCurrentRecipe()
        //{
        //    try
        //    {
        //        CurrentRecipe?.Save();
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        //OnErrorOccurred("SaveCurrentRecipe 오류: " + ex.Message);
        //        Log.Write(ex);
        //        return false;
        //    }
        //}




    }

    #region Supporting Classes and Enums
    /// <summary>
    /// Unit 실행 정보
    /// </summary>
    internal class UnitExecutionInfo
    {
        public string UnitName { get; set; }
        public string Description { get; set; }
        public Task ExecutionTask { get; set; }
        public CancellationTokenSource CancellationTokenSource { get; set; }
        public bool IsRunning { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? StopTime { get; set; }

        public UnitExecutionInfo(string unitName, string description = null)
        {
            UnitName = unitName;
            Description = description ?? unitName;
            IsRunning = false;
        }
    }

    /// <summary>
    /// Unit 상태 정보
    /// </summary>
    public class UnitStatusInfo
    {
        public string UnitName { get; set; }
        public string Description { get; set; }
        public bool IsRunning { get; set; }
        public UnitStatus RunUnitStatus { get; set; }
        public int ComponentCount { get; set; }
        public DateTime? StartTime { get; set; }
        public TimeSpan RunningTime { get; set; }
        public DateTime LastUpdateTime { get; set; }
    }

    /// <summary>
    /// 설비 상태 변경 이벤트 인자
    /// </summary>
    public class EquipmentStateChangedEventArgs : EventArgs
    {
        public EquipmentState OldState { get; }
        public EquipmentState NewState { get; }

        public EquipmentStateChangedEventArgs(EquipmentState oldState, EquipmentState newState)
        {
            OldState = oldState;
            NewState = newState;
        }
    }

    /// <summary>
    /// Unit 상태 변경 이벤트 인자
    /// </summary>
    public class UnitStateChangedEventArgs : EventArgs
    {
        public string UnitName { get; }
        public UnitStatus RunUnitStatus { get; }

        public UnitStateChangedEventArgs(string unitName, UnitStatus state)
        {
            UnitName = unitName;
            RunUnitStatus = state;
        }
    }

    /// <summary>
    /// 설비 오류 이벤트 인자
    /// </summary>
    public class EquipmentErrorEventArgs : EventArgs
    {
        public string ErrorMessage { get; }
        public DateTime Timestamp { get; }

        public EquipmentErrorEventArgs(string errorMessage)
        {
            ErrorMessage = errorMessage;
            Timestamp = DateTime.Now;
        }
    }
    #endregion





}