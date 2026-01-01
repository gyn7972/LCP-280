using QMC.Common;
using QMC.Common.BarcodeReader;
using QMC.Common.Cameras;
using QMC.Common.Cameras.HIKVISION;
using QMC.Common.Component;
using QMC.Common.DIO;
using QMC.Common.HIKVISION;
using QMC.Common.IO;
using QMC.Common.IOUtil;
using QMC.Common.Keithley;
using QMC.Common.LightController;
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
using QMC.LCP_280.Process.Component.ProcessData;
using QMC.LCP_280.Process.Unit;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
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
                        {
                            _instance = new Equipment();
                            try
                            {
                                // UI가 EquipmentLocator.Instance를 사용할 때 같은 인스턴스를 보도록 연결
                                EquipmentLocator.Initialize(_instance);
                            }
                            catch { /* 로깅 선택 */ }
                        }
                    }
                }
                return _instance;
            }
        }
        #endregion

        #region Fields & Properties

        private volatile bool _isShuttingDown;

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

        // [ADD] 초기화 완료 플래그 (InitializeEquipment 성공적으로 끝났을 때만 true)
        private volatile bool _isEquipmentInitialized;
        public bool IsEquipmentInitialized => _isEquipmentInitialized;

        private volatile bool _isAxisHomed;
        public bool IsAxisHomed => _isAxisHomed;

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

        private readonly object _motionSafetyStopLock = new object();
        private volatile bool _motionSafetyStopIssued;

        #region Camera 관련 
        // 기존: public HIKGigECamera Camera { get; set; } = null;
        public Dictionary<string, HIKGigECamera> Cameras { get; } = new Dictionary<string, HIKGigECamera>(StringComparer.OrdinalIgnoreCase);
        // === 편의 프로퍼티 추가 ===
        
        public HIKGigECamera InStageCam => GetCamera("In_Stage");
        public HIKGigECamera IndexLoaderCam => GetCamera("Index_Loader");
        public HIKGigECamera Index_AlignerCam => GetCamera("Index_Aligner");
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
        #endregion

        #region Light 관련

        // === 조명 컨트롤러 (단일 + 확장 가능) ===
        public Dictionary<string, LeesOsLightController> LightControllers { get; } = new Dictionary<string, LeesOsLightController>(StringComparer.OrdinalIgnoreCase);

        // === 편의 프로퍼티 (메인 조명) ===
        public LeesOsLightController LeesOsLightController => GetLightController("Light");

        private LeesOsLightController GetLightController(string key)
        {
            return LightControllers.TryGetValue(key, out var light) ? light : null;
        }

        //조명 사용 예
        // Equipment.Instance.MainLightController?.Channels[0].Config.On = true;  // 채널 1 켜기
        // Equipment.Instance.MainLightController?.Channels[0].Config.Volume = 128; // 밝기 설정

        #endregion


        #region Barcoder
        // === Barcoder 컨트롤러 (단일 + 확장 가능) ===
        public Dictionary<string, OpticonBarcodeReader> Barcoders { get; } = new Dictionary<string, OpticonBarcodeReader>(StringComparer.OrdinalIgnoreCase);

        // === 편의 프로퍼티 (Barcoder) ===
        public OpticonBarcodeReader BarcoderReader1 => GetBarcoderController("BarcoderReader1"); //Output
        public OpticonBarcodeReader BarcoderReader2 => GetBarcoderController("BarcoderReader2"); //Input

        private OpticonBarcodeReader GetBarcoderController(string key)
        {
            return Barcoders.TryGetValue(key, out var reader) ? reader : null;
        }
        #endregion

        #region Sourcemeters

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
        #endregion

        // PKG Tester
        public PKGTester Tester { get; private set; }
        // 1) 결과 저장 매니저 추가
        private ResultWriterManager _resultWriterManager;
        public ResultWriterManager ResultWriterManager
        {
            get { return _resultWriterManager; }
        }


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

        // [ADD] 워밍업 플래그
        private volatile bool _threadPoolWarmed = false; // 워밍업 수행 여부 플래그


        //재현성및 1:1모드
        public bool bIndexCal { get; set; } = true;

        #endregion

        #region Constructor & Initialization

        private Equipment()
        {
            // [ADD] 초기화 전에도 UI가 null 참조로 죽지 않도록 빈 컨테이너는 만들어 둠
            Units = new ConcurrentDictionary<string, IUnit>();
            _unitExecutions = new ConcurrentDictionary<string, UnitExecutionInfo>();
            EqState = EquipmentState.Stopped;

            _isEquipmentInitialized = false;
        }

        /// <summary>
        /// 설비 초기화
        /// </summary>
        public void InitializeEquipment()
        {
            try
            {
                // [ADD] 초기화 시작 시점에 false로 리셋(부분 실패 시에도 잠김 유지)
                _isEquipmentInitialized = false;
                _isAxisHomed = false; // [ADD] 재기동/재초기화 시 Home 다시 요구

                // [ADD] ThreadPool 워밍업 및 최소 스레드 수 상향 (Task.Run 첫 호출 지연 감소)
                try
                {
                    WarmupThreadPoolIfNeeded();
                }
                catch { }

                // 보강: Locator 초기화
                if (!EquipmentLocator.IsInitialized)
                {
                    EquipmentLocator.Initialize(this);
                }

                Units = new ConcurrentDictionary<string, IUnit>();
                _unitExecutions = new ConcurrentDictionary<string, UnitExecutionInfo>();
                EqState = EquipmentState.Stopped;

                ConfigManager = new EquipmentConfigManager();
                EquipmentConfig = new EquipmentConfig();
                EquipmentRecipe = new EquipmentRecipe();

                _resultWriterManager = new ResultWriterManager();

                OnStateChanged(EquipmentState.Initializing);

                // 여기서 모든 유닛 축을 직접 생성/로드하여 붙인다.
                BootstrapAxesDirect();

                BootstrapIODirect();

                // === 카메라 초기화 ===
                InitializeCameras();

                // === 조명 초기화 === , 기존 구성되어 있느 부분하고 동일하게 작업
                InitializeLightControllers();

                // === 바코드 초기화 ===
                InitializeBarcoderControllers();

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

                // PKG Tester Load Recipe
                var currentRecipe = EquipmentRecipe?.CurrentRecipe;
                if (currentRecipe != null && Tester != null)
                {
                    var mb = new MessageBoxOk();

                    if (Tester.LoadTestConditionSet(currentRecipe.TestConditionSetFile) != 0)
                    {
                        mb.ShowDialog("Error!", $"Failed to load test condition set.");
                    }
                    if (Tester.LoadBinningSpecSheet(currentRecipe.BinningSpecSheetFile) != 0)
                    {
                        mb.ShowDialog("Error!", $"Failed to load binning spec sheet.");
                    }
                }

                _motionSafetyStopIssued = false;
                _isEquipmentInitialized = true; // 모든 초기화가 성공적으로 끝났음을 표시
                OnStateChanged(EquipmentState.Ready);
            }
            catch (Exception ex)
            {
                _isEquipmentInitialized = false;
                _isAxisHomed = false; // [ADD]
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


        #region Equipment Sequence Control
        /// <summary>
        /// 설비 전체 시작
        /// </summary>
        public async Task<bool> StartAllUnitsAsync()
        {
            if (EqState == EquipmentState.AutoRunning)
            {
                Log.Write("Equipment", "설비가 이미 실행 중입니다.");
                return true;
            }

            try
            {
                OnStateChanged(EquipmentState.Starting);

                // 새로운 취소 토큰 생성
                _equipmentCancellationTokenSource?.Dispose();
                _equipmentCancellationTokenSource = new CancellationTokenSource();

                // 1) 보호 유닛(EquipmentStatus) 먼저 기동 (실패해도 계속)
                if (Units.TryGetValue(UnitKeys.EquipmentStatus, out var statusUnit))
                {
                    await StartUnitAsync(UnitKeys.EquipmentStatus).ConfigureAwait(false);
                }

                // 2) 나머지 유닛 병렬 기동
                var otherUnitNames = Units.Keys.Where(n => !IsProtectedUnit(n)).ToArray();
                var startTasks = otherUnitNames.Select(StartUnitAsync).ToArray();
                var results = await Task.WhenAll(startTasks).ConfigureAwait(false);

                if (results.All(r => r))
                {
                    OnStateChanged(EquipmentState.AutoRunning);
                    Log.Write("Equipment", "설비 전체 시작 완료");
                    return true;
                }
                else
                {
                    await StopAllUnitsAsync(includeEquipmentStatus: false).ConfigureAwait(false);
                    OnStateChanged(EquipmentState.Error);
                    Log.Write("Equipment", "설비 시작 중 일부 Unit 실패");
                    return false;
                }
            }
            catch (Exception ex)
            {
                OnStateChanged(EquipmentState.Error);
                Log.Write(ex);
                return false;
            }
        }

        /// <summary>
        /// 설비 전체 정지
        /// </summary>
        public async Task<bool> StopAllUnitsAsync(bool includeEquipmentStatus = false)
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

                    // 종료하고 싶지 않은 유닛은 스킵
                    // EquipmentStatus class는 기본적으로 정지하지 않음
                    if (IsProtectedUnit(unitName) && !includeEquipmentStatus)
                    {
                        continue;
                    }

                    if (execInfo.IsRunning)
                    {
                        SetAndRaiseUnitState(unitName, UnitStatus.Stopping);

                        // 유닛 내부에도 Stop 신호
                        if (Units.TryGetValue(unitName, out var u))
                        {
                            (u as QMC.Common.Unit.BaseUnit)?.Stop();
                        }

                        // 수명 태스크 취소
                        execInfo.CancellationTokenSource?.Cancel();

                        // 종료 대기(수명 태스크에 실제로 바인딩됨)
                        if (execInfo.ExecutionTask != null)
                        {
                            stopTasks.Add(WaitForUnitStopAsync(unitName, execInfo.ExecutionTask));
                        }
                    }
                }

                if (stopTasks.Count > 0)
                {
                    await Task.WhenAll(stopTasks).ConfigureAwait(false);
                }

                OnStateChanged(EquipmentState.Stopped);

                // [FIX] 정지 후 취소된 설비 토큰 정리
                try 
                { 
                    _equipmentCancellationTokenSource?.Dispose(); 
                } 
                catch (Exception ex)
                {
                    Log.Write(ex);
                }
                _equipmentCancellationTokenSource = null;

                Log.Write("Equipment", "설비 전체 정지 완료");
                return true;
            }
            catch (Exception ex)
            {
                OnStateChanged(EquipmentState.Error);
                Log.Write(ex);
                return false;
            }
        }

        /// <summary>
        /// 설비 전체 정지 (인터페이스 기본 호출용 – EquipmentStatus 포함)
        /// </summary>
        public Task<bool> StopAllUnitsAsync()
        {
            // IEquipment 인터페이스(매개변수 없는 버전) 요구 충족용 래퍼
            return StopAllUnitsAsync(includeEquipmentStatus: false);
        }

        /// <summary>
        /// 프로그램 종료 직전 전체 Dispose 역할
        /// </summary>
        /// 
        public async Task<bool> TerminateAllUnitsAsync()
        {
            foreach (var kvp in _unitExecutions)
            {
                var unitName = kvp.Key;
                var execInfo = kvp.Value;

                SetAndRaiseUnitState(unitName, UnitStatus.Stopping);

                // 유닛 내부에도 Stop 신호
                if (Units.TryGetValue(unitName, out var u))
                {
                    (u as QMC.Common.Unit.BaseUnit)?.Stop();
                    (u as QMC.Common.Unit.BaseUnit)?.Terminate();
                }
            }
            return true;
        }

        /// <summary>
        /// 개별 Unit 시작
        /// </summary>
        /// <param name="unitName">시작할 Unit 이름</param>
        public async Task<bool> StartUnitAsync(string unitName)
        {
            if (!Units.TryGetValue(unitName, out var unitObj))
            {
                Log.Write("Equipment", $"Unit '{unitName}'를 찾을 수 없습니다.");
                return false;
            }

            if (!_unitExecutions.TryGetValue(unitName, out var execInfo))
            {
                Log.Write("Equipment", $"Unit '{unitName}' 실행 정보를 찾을 수 없습니다.");
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
                    }
                    else
                    {
                        Log.Write("Equipment", $"Unit '{unitName}'는 이미 실행 중입니다.");
                        return true;
                    }
                }

                SetAndRaiseUnitState(unitName, UnitStatus.Starting);

                // [FIX] 설비 토큰이 취소 상태면 재생성
                if (_equipmentCancellationTokenSource == null 
                    || _equipmentCancellationTokenSource.IsCancellationRequested)
                {
                    try 
                    { 
                        _equipmentCancellationTokenSource?.Dispose(); 
                    } 
                    catch (Exception ex) 
                    { 
                        Log.Write(ex);
                    }
                    _equipmentCancellationTokenSource = new CancellationTokenSource();
                }

                var linkedCts = IsProtectedUnit(unitObj)
                               ? new CancellationTokenSource()
                               : CancellationTokenSource.CreateLinkedTokenSource(_equipmentCancellationTokenSource.Token);

                execInfo.CancellationTokenSource = linkedCts;
                execInfo.IsRunning = true;
                execInfo.StartTime = DateTime.Now;
                
                // 유닛 실행 수명과 바인딩되는 백그라운드 태스크
                // 1) Start() 호출(블로킹이면 여기서 대기)
                // 2) Running 전환
                // 3) RunUnitLoopAsync로 주기 작업 + 취소 토큰으로 lifetime 유지
                execInfo.ExecutionTask = Task.Run(async () =>
                {
                    try
                    {
                        (unitObj as BaseUnit)?.Start(); // 초기화/실행 진입
                        SetAndRaiseUnitState(unitName, UnitStatus.AutoRunning);

                        // 이거 안해도 되는거 같은데.
                        // 이거 안해야 하는거 같다.
                        //await RunUnitLoopAsync(unitName, bu, linkedCts.Token).ConfigureAwait(false);
                    }
                    catch (OperationCanceledException)
                    {
                        // 정상 취소
                    }
                    catch (Exception ex)
                    {
                        Log.Write(ex);
                        SetAndRaiseUnitState(unitName, UnitStatus.Error);
                    }
                }, linkedCts.Token);

                return true;
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                SetAndRaiseUnitState(unitName, UnitStatus.Error);
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
                Log.Write("Equipment", $"Unit '{unitName}'를 찾을 수 없습니다.");
                return false;
            }

            if (IsProtectedUnit(unitName))
            {
                Log.Write("Equipment", "EquipmentStatus 는 정지 대상에서 제외 (요청 무시).");
                return true;
            }

            if (!_unitExecutions.TryGetValue(unitName, out var execInfo))
            {
                Log.Write("Equipment", $"Unit '{unitName}' 실행 정보를 찾을 수 없습니다.");
                return false;
            }

            try
            {
                if (execInfo.IsRunning)
                {
                    SetAndRaiseUnitState(unitName, UnitStatus.Stopping);
                }

                // 유닛 Stop 트리거
                (unitObj as QMC.Common.Unit.BaseUnit)?.Stop();

                // 실행 태스크 취소(있다면)
                execInfo.CancellationTokenSource?.Cancel();

                // 1) 기존 수명 태스크 종료 대기 (있을 경우)
                bool byTask = true;
                if (execInfo.ExecutionTask != null)
                {
                    byTask = await WaitForUnitStopAsync(unitName, execInfo.ExecutionTask).ConfigureAwait(false);
                }

                // 2) 보강: 유닛 상태 폴링(실제 RunUnitStatus/IsStop 기반)로 정지 확인
                var bu = unitObj as QMC.Common.Unit.BaseUnit;
                bool byState = await PollUnitFullyStoppedAsync(unitName, bu, timeoutMs: 20000, pollMs: 80).ConfigureAwait(false);

                bool ok = byTask && byState;
                Log.Write("Equipment", $"Unit '{unitName}' 정지 {(ok ? "완료" : "실패/타임아웃")} (task={byTask}, state={byState})");

                // 최종 상태 반영
                if (ok)
                {
                    SetAndRaiseUnitState(unitName, UnitStatus.Stopped);
                }
                else
                {
                    SetAndRaiseUnitState(unitName, UnitStatus.Error);
                }

                return ok;
            }
            catch (Exception ex)
            {
                SetAndRaiseUnitState(unitName, UnitStatus.Error);
                Log.Write(ex);
                return false;
            }
        }

        // 실행 태스크 대기(기존 로직 유지) + 실패 시 Error
        private async Task<bool> WaitForUnitStopAsync(string unitName, Task executionTask)
        {
            try
            {
                var timeoutTask = Task.Delay(10000);
                var completedTask = await Task.WhenAny(executionTask, timeoutTask).ConfigureAwait(false);

                if (completedTask == timeoutTask)
                {
                    Log.Write("Equipment", $"Unit '{unitName}' 정지 타임아웃(ExecutionTask)");
                    // 주의: 여기서 바로 Error로 확정하지 않고, 상태 폴링으로 추가 확인 예정
                    return false;
                }

                Log.Write("Equipment", $"Unit '{unitName}' 정지(ExecutionTask) 완료");
                return true;
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                return false;
            }
        }

        // [NEW] 유닛의 실제 상태(Stoppd/IsStop)로 완전 정지 확인을 폴링
        private async Task<bool> PollUnitFullyStoppedAsync(string unitName, BaseUnit unit, int timeoutMs, int pollMs)
        {
            if (unit == null) 
                return true; // 유닛 객체가 없으면 더 이상 확인 불가 → 완료로 간주

            var sw = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                while (sw.ElapsedMilliseconds < timeoutMs)
                {
                    // 유닛이 실제로 정지 상태로 전이했는지 확인
                    bool stopped =
                        unit.RunUnitStatus == BaseUnit.UnitStatus.Stopped
                        || unit.IsStop
                        || unit.IsCycleStop;

                    if (stopped)
                        return true;

                    await Task.Delay(pollMs).ConfigureAwait(false);
                }

                Log.Write("Equipment", $"Unit '{unitName}' 상태 폴링 타임아웃 - 실제 정지 확인 실패");
                return false;
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                return false;
            }
        }

        ///// <summary>
        ///// Unit 실행 루프
        ///// </summary>
        //private async Task RunUnitLoopAsync(string unitName, BaseUnit unit, CancellationToken cancellationToken)
        //{
        //    try
        //    {
        //        while (!cancellationToken.IsCancellationRequested)
        //        {
        //            // Unit별 주기적 작업 수행
        //            await PerformUnitCycle(unit, cancellationToken);

        //            // 100ms 대기 (Unit별로 다르게 설정 가능)
        //            await Task.Delay(100, cancellationToken);
        //        }
        //    }
        //    catch (OperationCanceledException)
        //    {
        //        // 정상적인 취소
        //        Log.Write("Equipment", $"Unit '{unitName}' 정지 요청에 따른 종료 처리 시작");
        //    }
        //    catch (Exception ex)
        //    {
        //        OnUnitStateChanged(unitName, UnitStatus.Error);
        //        Log.Write(ex);
        //    }
        //    finally
        //    {
        //        // [MOD] 보호 유닛은 외부 Stop 트리거 없으면 OnStop 호출 없이 유지되도록
        //        if (!IsProtectedUnit(unitName))
        //        {
        //            try
        //            {
        //                unit.OnStop();
        //                OnUnitStateChanged(unitName, UnitStatus.Stopped);
        //            }
        //            catch (Exception ex)
        //            {
        //                Log.Write(ex);
        //            }
        //        }

        //        // 실행 정보 정리
        //        if (_unitExecutions.TryGetValue(unitName, out var execInfo))
        //        {
        //            execInfo.IsRunning = false;
        //            execInfo.StopTime = DateTime.Now;
        //            execInfo.CancellationTokenSource?.Dispose();
        //            execInfo.CancellationTokenSource = null;
        //        }
        //    }
        //}

        ///// <summary>
        ///// Unit별 주기적 작업 수행
        ///// </summary>
        //private async Task PerformUnitCycle(BaseUnit unit, CancellationToken cancellationToken)
        //{
        //    switch (unit)
        //    {
        //        case InputCassetteLifter cassetteUnit:
        //            await PerformCassetteElevatorCycle(cassetteUnit, cancellationToken);
        //            break;
        //        case EquipmentStatus statusUnit:
        //            statusUnit.Refresh();
        //            await Task.Delay(1, cancellationToken);
        //            break;
        //        default:
        //            await Task.Delay(1, cancellationToken);
        //            break;
        //    }
        //}

        ///// <summary>
        ///// CassetteLoadingElevator 주기적 작업
        ///// </summary>
        //private async Task PerformCassetteElevatorCycle(InputCassetteLifter unit, CancellationToken cancellationToken)
        //{
        //    // 실제 설비 로직에 맞게 구현
        //    // 예: 센서 체크, 위치 확인, 에러 체크 등
        //    await Task.Delay(1, cancellationToken);
        //}

        #endregion

        #region Equipment Control
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
                return exec.IsRunning ? UnitStatus.AutoRunning : UnitStatus.Stopped;

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

            // 보호 유닛 상태 보호: 종료(프로세스 종료/Dispose) 중 표시
            if (IsProtectedUnit(unitName) && !_isShuttingDown)
            {
                if (newState == UnitStatus.Stopping || newState == UnitStatus.Stopped)
                {
                    // 무시: EquipmentStatus는 프로그램 종료 전까지 멈추지 않음
                    return;
                }
            }


            // 1) BaseUnit 반영
            if (Units != null && Units.TryGetValue(unitName, out var u) && u is BaseUnit bu)
            {
                if (bu.RunUnitStatus != newState)
                    bu.RunUnitStatus = newState;

                switch (newState)
                {
                    case UnitStatus.Starting:
                        bu.RunUnitStatus = UnitStatus.Starting;
                        EqState = EquipmentState.Starting;
                        break;
                    case UnitStatus.AutoRunning:
                        bu.RunUnitStatus = UnitStatus.AutoRunning;
                        EqState = EquipmentState.AutoRunning;
                        break;
                    case UnitStatus.ManualRunning:
                        bu.RunUnitStatus = UnitStatus.ManualRunning;
                        EqState = EquipmentState.ManualRunning;
                        break;
                    case UnitStatus.Stopping:
                        bu.RunUnitStatus = UnitStatus.Stopping;
                        EqState = EquipmentState.Stopping;
                        break;
                    case UnitStatus.CycleStop:
                        bu.RunUnitStatus = UnitStatus.CycleStop;
                        EqState = EquipmentState.CycleStop;
                        break;
                    case UnitStatus.Stopped:
                        bu.RunUnitStatus = UnitStatus.Stopped;
                        EqState = EquipmentState.Stopped;
                        break;
                    case UnitStatus.Error:
                        bu.RunUnitStatus = UnitStatus.Error;
                        EqState = EquipmentState.Error;
                        break;
                    case UnitStatus.Unknown:
                        bu.RunUnitStatus = UnitStatus.Unknown;
                        EqState = EquipmentState.Unknown;
                        //bu.IsRunning = false;
                        break;
                }
            }

            // 2) 실행정보 반영
            if (_unitExecutions != null && _unitExecutions.TryGetValue(unitName, out var exec))
            {
                bool running = (newState == UnitStatus.AutoRunning || newState == UnitStatus.Starting);
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

            _disposed = true;
            _isShuttingDown = true; // 종료(프로세스 종료/Dispose) 중 표시

            try
            {
                // 1) 일반 유닛 정지 (EquipmentStatus 제외)
                try 
                { 
                    StopAllUnitsAsync(includeEquipmentStatus: false).GetAwaiter().GetResult(); 
                } 
                catch (Exception ex)
                {
                    Log.Write(ex);
                }

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


                // LightControllers 추가
                try
                {
                    foreach (var light in LightControllers.Values)
                    {
                        try
                        {
                            // 모든 채널 끄기
                            foreach (var channel in light.Channels)
                            {
                                channel.Config.On = false;
                            }
                            light.Close();
                        }
                        catch { }
                    }
                }
                catch { }
                LightControllers.Clear();

                // Barcoder 추가
                try
                {
                    foreach (var barcode in Barcoders.Values)
                    {
                        barcode.Close();
                    }
                }
                catch { }
                Barcoders.Clear();

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

                _motionStatusScanner.AxisStatusUpdated += (axis, status) =>
                {
                    try
                    {
                        if (_disposed || _isShuttingDown) return;
                        if (!_isEquipmentInitialized) return;
                        if (!IsAxisHomed) return;
                        if (axis == null || status == null) return;

                        // 1) 기존 알람 시스템 사용 (AlarmPost)
                        axis.CheckAndPostSafetyAlarms();

                        // 2) Fault 조건이면 즉시 정지
                        bool fault =
                            (status.IO != null) &&
                            (!status.IO.ServoOn
                             || status.IO.Alarm
                             || status.IO.PositiveLimitSensor
                             || status.IO.NegativeLimitSensor);

                        if (fault)
                        {
                            IssueImmediateMotionStopOnce();
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Write("Equipment", "[MotionSafety] AxisStatusUpdated error: " + ex);
                    }
                };

                _motionStatusScanner.Start();

                //_motionStatusScanner = new MotionStatusScanner(_axisManager, periodMs: 20);
                //_motionStatusScanner.AxisStatusUpdated += (axis, status) => { /* 필요 시 이벤트 처리 */ };
                //_motionStatusScanner.Start();
            }
        }

        private void IssueImmediateMotionStopOnce()
        {
            if (_motionSafetyStopIssued)
                return;

            lock (_motionSafetyStopLock)
            {
                if (_motionSafetyStopIssued)
                    return;

                _motionSafetyStopIssued = true;
            }

            try
            {
                Log.Write("Equipment", "[MotionSafety] Fault detected -> EMG STOP all axes");

                var axes = _axisManager != null ? _axisManager.GetAll() : null;
                if (axes != null)
                {
                    foreach (var a in axes)
                    {
                        try { a.EmgStop(); } catch { }
                    }
                }

                // 상태 정책: 즉시 Error로
                OnStateChanged(EquipmentState.Error);
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
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
                        Thread.Sleep(1);
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
                InposTolerance = 0.010,
                ProfileMode = ProfileMode.SCurve,
                AccJerkPercent = 50,
                DecJerkPercent = 50
            };
            if (!File.Exists(configPath)) config.TrySave(configPath, out _);
            if (axisName != AxisNames.IndexT)
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
                    HIKGigECameraConfig cfg;
                    try 
                    { 
                        cfg = HIKGigECameraConfig.LoadOrCreate(name); 
                    }
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

        private void InitializeLightControllers()
        {
            try
            {
                var lightConfigs = new[]
                {
                    new { Name = "Light", Model = LeesOsLightControllerModel.LPD_12024_8CH, PortName = "COM1" }
                };

                foreach (var config in lightConfigs)
                {
                    try
                    {
                        var lightController = new LeesOsLightController(config.Name, config.Model);

                        // 1) 메인 컨트롤러 Config 로드
                        int ret = lightController.Config.Load();
                        if (ret != 0)
                        {
                            Log.Write("Equipment", $"[LightController] '{config.Name}' config load failed rc=0x{ret:X8}");
                            lightController.Config.Reset();
                            lightController.Config.PortName = config.PortName;
                            lightController.Config.Save();
                        }

                        // 2) 각 채널별 Config도 로드/생성 추가
                        foreach (var channel in lightController.Channels)
                        {
                            try
                            {
                                int channelRet = channel.Config.Load();
                                if (channelRet != 0)
                                {
                                    Log.Write("Equipment", $"[LightController] Channel '{channel.Config.Name}' config load failed, creating default");
                                    channel.Config.Reset();
                                    channel.Config.Save(); // 채널별 JSON 파일 생성
                                }
                            }
                            catch (Exception chEx)
                            {
                                Log.Write("Equipment", $"[LightController] Channel config error: {chEx.Message}");
                                // 실패 시 기본값으로 저장
                                try
                                {
                                    channel.Config.Reset();
                                    channel.Config.Save();
                                }
                                catch { }
                            }
                        }

                        // 3) 컨트롤러 초기화
                        //ret = lightController.Initialize();
                        ret = lightController.Connect();
                        if (ret != 0)
                        {
                            var mb = new MessageBoxOk();
                            var st = $"LightController [{lightController.Name}] initialize NG.";
                            mb.ShowDialog("initialize NG", st);
                        }
                        else
                        {
                            ret = lightController.Create();
                        }

                        LightControllers[config.Name] = lightController;

                        LightControllers[config.Name].SetAllChannelsOn();

                        Console.WriteLine($"[LightController] {config.Name} ready with {lightController.Channels.Count} channels");
                    }
                    catch (Exception ex)
                    {
                        Log.Write("Equipment", $"[LightController] '{config.Name}' init failed: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Write("Equipment", $"InitializeLightControllers error: {ex.Message}");
            }
        }

        private void InitializeBarcoderControllers()
        {
            try
            {
                var barcoderConfigs = new[]
                {
                    new { Name = "BarcoderReader1", PortName = "COM3" },
                    new { Name = "BarcoderReader2", PortName = "COM3" }
                };

                foreach (var config in barcoderConfigs)
                {
                    try
                    {
                        var barcoderReader = new OpticonBarcodeReader(config.Name);

                        // 1) 메인 컨트롤러 Config 로드
                        int ret = barcoderReader.Config.Load();
                        if (ret != 0)
                        {
                            Log.Write("Equipment", $"[BarcoderReader] '{config.Name}' config load failed rc=0x{ret:X8}");
                            barcoderReader.Config.Reset();
                            barcoderReader.Config.PortName = config.PortName;
                            barcoderReader.Config.Save();
                        }

                        // 3) 컨트롤러 초기화
                        ret = barcoderReader.Initialize();
                        if (ret != 0)
                        {
                            var mb = new MessageBoxOk();
                            var st = $"BarcoderReader [{barcoderReader.Name}] initialize NG.";
                            mb.ShowDialog("initialize NG", st);
                        }
                        else
                        {
                            ret = barcoderReader.Create();
                        }

                        Barcoders[config.Name] = barcoderReader;
                        Console.WriteLine($"[BarcoderReader] {config.Name}");
                    }
                    catch (Exception ex)
                    {
                        Log.Write("Equipment", $"[BarcoderReader] '{config.Name}' init failed: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Write("Equipment", $"InitializeBarcoderReaders error: {ex.Message}");
            }
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
                        var mb = new MessageBoxOk();
                        var st = $"Sourcemeter [{smu.Name}] initialize NG.";
                        mb.ShowDialog("initialize NG", st);
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
                        var mb = new MessageBoxOk();
                        var st = $"Sourcemeter [{spc.Name}] initialize NG.";
                        mb.ShowDialog("initialize NG", st);
                    }
                    Spectrometers[name] = spc;
                    Console.WriteLine($"[Spectrometer] {name} ready");
                }
                catch (Exception ex) { Log.Write(ex); }
            }
        }

        private void InitializePKGTester()
        {
            try 
            {   
                Tester = new PKGTester("PKGTester");
                Tester.BindSourcemeter(Sourcemeter);
                Tester.BindSpectrometer(Spectrometer);

                var currentRecipe = EquipmentRecipe?.CurrentRecipe;
                if (currentRecipe != null)
                {
                    Tester.LoadTestConditionSet(currentRecipe.TestConditionSetFile);

                    // 1) 레시피의 스펙 파일 경로 추출
                    var specPath = currentRecipe.BinningSpecSheetFile;

                    // 2) Excel/BIN → ExcelBinningModel 로드
                    ExcelBinningModel excelModel = null;
                    if (!string.IsNullOrWhiteSpace(specPath) && File.Exists(specPath))
                    {
                        var ext = Path.GetExtension(specPath).ToLowerInvariant();
                        if (ext == ".xlsx" || ext == ".xls")
                        {
                            excelModel = QMC.Common.PKGTester.DataBinningExcelLoader.Load(specPath);
                        }
                        else if (ext == ".bin")
                        {
                            excelModel = DataBinningBinLoader.LoadBIN(specPath);
                        }
                    }

                    // 3) 변환 후 검사 엔진에 주입(기존 분류기 유지)
                    if (excelModel != null)
                    {
                        var sheet = ExcelBinningModelConverter.ToSpecSheet(excelModel);
                        if (!Tester.BinningSpecSheet.CopyFrom(sheet))
                        {
                            var mb = new MessageBoxOk();
                            mb.ShowDialog("Error!", "Failed to apply binning spec (from ExcelBinningModel).");
                        }
                    }
                    else
                    {
                        // 폴백: 기존 방식(레거시 파일일 수 있음)
                        if (Tester.LoadBinningSpecSheet(specPath) != 0)
                        {
                            var mb = new MessageBoxOk();
                            mb.ShowDialog("Error!", $"Failed to load binning spec sheet.");
                        }
                    }
                }

                //var currentRecipe = EquipmentRecipe?.CurrentRecipe;
                //if (currentRecipe != null)
                //{
                //    Tester.LoadTestConditionSet(currentRecipe.TestConditionSetFile);
                //    Tester.LoadBinningSpecSheet(currentRecipe.BinningSpecSheetFile);
                //}
            }
            catch (Exception ex) 
            { Log.Write(ex); }
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
                        var mb = new MessageBoxOk();
                        var st = $"Sourcemeter [{gage.Name}] initialize NG.";
                        mb.ShowDialog("initialize NG", st);
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

        // 안전한 ThreadPool 워밍업 헬퍼
        // - 이미 워밍업된 경우 재실행하지 않음
        // - EqState가 AutoRunning인 경우에는 ThreadPool 최소 스레드 변경을 피하고 가벼운 no-op 태스크만 큐잉
        // - 최소 스레드는 오직 증가만 수행(감소하지 않음)
        private void WarmupThreadPoolIfNeeded()
        {
            if (_threadPoolWarmed) return;

            try
            {
                // 만약 이미 설비가 AutoRunning 상태라면 시스템에 영향을 줄 수 있으므로 최소 스레드 변경은 하지 않음
                if (EqState == EquipmentState.AutoRunning)
                {
                    // 가벼운 워밍업: 소수의 no-op 작업만 큐잉하여 스레드풀 진입 비용을 줄임
                    int warmCount = Math.Max(1, Environment.ProcessorCount);
                    var warmTasks = new Task[warmCount];
                    for (int i = 0; i < warmCount; i++)
                    {
                        warmTasks[i] = Task.Run(() => { /* no-op short work */ Thread.Sleep(1); });
                    }
                    try { Task.WaitAll(warmTasks, 1000); } catch { }
                    _threadPoolWarmed = true;
                    return;
                }

                int minW, minIO;
                ThreadPool.GetMinThreads(out minW, out minIO);

                int target = Math.Max(minW, Math.Max(8, Environment.ProcessorCount * 4));
                int targetIO = Math.Max(minIO, Math.Max(8, Environment.ProcessorCount * 4));

                // 안전: 기존 값보다 작으면 절대 설정하지 않음(기동 중 다운그레이드 방지)
                if (target > minW || targetIO > minIO)
                {
                    try
                    {
                        ThreadPool.SetMinThreads(target, targetIO);
                    }
                    catch (Exception ex)
                    {
                        try { Log.Write(ex); } catch { }
                    }
                }

                // 워밍업 태스크를 사용해 스레드풀을 미리 기동
                var warmups = new Task[Math.Max(1, Environment.ProcessorCount)];
                for (int i = 0; i < warmups.Length; i++)
                    warmups[i] = Task.Run(() => { /* no-op */ Thread.Sleep(1); });

                try { Task.WaitAll(warmups, 2000); } catch { /* timeout 무시 */ }

                _threadPoolWarmed = true;
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
        }

        #endregion

        // ===== 메인 설비 Config / Recipe 로드 추가 =====
        public  EquipmentConfig EquipmentConfig { get;  set; }
        public  EquipmentRecipe EquipmentRecipe { get;  set; }
        public string ICurrentRecipe { get; set; }


        public bool m_bBuzzerOff { get; set; } = false;


        // [ADD] 초기화 필요 동작을 격리하기 위한 공통 Guard
        // - 초기화 전에는 설비가 "어떤 동작도" 하지 않게 하는 핵심 게이트
        private void EnsureInitializedOrThrow(string actionName)
        {
            if (_disposed) 
                throw new ObjectDisposedException(nameof(Equipment));

            if (_isShuttingDown) 
                throw new InvalidOperationException("장비 종료 중입니다.");

            if (!_isEquipmentInitialized)
                throw new InvalidOperationException($"장비 초기화가 필요합니다. (요청 동작: {actionName})");
        }


        // [ADD] 외부(Operator_Main 등)에서 Home 완료를 알리는 API
        public void MarkAxisHomed()
        {
            _isAxisHomed = true;
        }

        // [ADD] Home 완료를 다시 무효화(필요 시: 알람/리셋/재초기화 시점에 호출)
        public void ResetAxisHomed()
        {
            _isAxisHomed = false;
        }

        // [ADD] "오토/이동 가능" 조건을 한 곳에서 관리
        public bool CanRunAutoOrMoveAxes => IsEquipmentInitialized && IsAxisHomed;

        // [ADD] 공통 Guard (오토/축이동 전용)
        public bool EnsureAxisReadyForAutoOrMove(string actionName)
        {
            // 기존 Guard 재사용 (InitializeEquipment 완료 여부)
            EnsureInitializedOrThrow(actionName);
            bool bRet = false;
            if (_isAxisHomed == false)
            {
                var mb = new MessageBoxOk();
                mb.TopMost = true;
                mb.ShowDialog("Error!", "축 Home(원점복귀)가 필요합니다.");

                bRet = false;
                return bRet;
            }

            bRet = true;
            return bRet;
        }

        #region Unit Helper

        // =========================
        // [ADD] Unit-level lock (공유 유닛 충돌 방지)
        // =========================
        private readonly object _unitGateMapLock = new object();
        private readonly Dictionary<string, SemaphoreSlim> _unitGateMap =
            new Dictionary<string, SemaphoreSlim>(StringComparer.OrdinalIgnoreCase);

        private SemaphoreSlim GetUnitGate(string unitName)
        {
            if (string.IsNullOrWhiteSpace(unitName))
                return null;

            lock (_unitGateMapLock)
            {
                if (!_unitGateMap.TryGetValue(unitName, out var gate))
                {
                    gate = new SemaphoreSlim(1, 1);
                    _unitGateMap[unitName] = gate;
                }
                return gate;
            }
        }

        private async Task<bool> TryEnterUnitGatesAsync(IEnumerable<string> unitNames, string opName, CancellationToken ct)
        {
            var names = (unitNames ?? Enumerable.Empty<string>())
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(n => n, StringComparer.OrdinalIgnoreCase) // deadlock 방지: 항상 동일 순서로 lock
                .ToList();

            var acquired = new List<SemaphoreSlim>();
            try
            {
                foreach (var n in names)
                {
                    ct.ThrowIfCancellationRequested();

                    var g = GetUnitGate(n);
                    if (g == null) continue;

                    // Fail-fast 대신 "대기"로 하는 것이 실제 운용에서 자연스러움
                    await g.WaitAsync(ct).ConfigureAwait(false);
                    acquired.Add(g);
                }

                return true;
            }
            catch (OperationCanceledException) { throw; }
            catch (Exception ex)
            {
                Log.Write("Equipment", $"[UnitGate] {opName} gate acquire 실패: {ex.Message}");
                return false;
            }
            finally
            {
                //if (acquired.Count == 0) return;

                // 실 패/예외인 경우 여기서 release
                // 성공인 경우는 호출자가 finally에서 ReleaseUnitGates(acquired) 호출
                // -> 여기서는 성공/실패를 구분하기 어렵기 때문에, 성공 시에는 acquired를 반환하는 패턴이 필요함.
                // 단, 본 프로젝트 스타일 유지 위해 아래 helper(WithUnitGatesAsync)로 감싼다.
            }
        }

        private void ReleaseUnitGates(List<SemaphoreSlim> acquired)
        {
            if (acquired == null) return;
            for (int i = acquired.Count - 1; i >= 0; i--)
            {
                try { acquired[i]?.Release(); } catch { }
            }
        }

        // 유닛 게이트를 잡고 action을 실행하는 공통 헬퍼
        private async Task<T> WithUnitGatesAsync<T>(IEnumerable<string> unitNames, string opName, CancellationToken ct, Func<Task<T>> action)
        {
            var names = (unitNames ?? Enumerable.Empty<string>())
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(n => n, StringComparer.OrdinalIgnoreCase)
                .ToList();

            var acquired = new List<SemaphoreSlim>();
            try
            {
                foreach (var n in names)
                {
                    ct.ThrowIfCancellationRequested();
                    var g = GetUnitGate(n);
                    if (g == null) continue;

                    await g.WaitAsync(ct).ConfigureAwait(false);
                    acquired.Add(g);
                }

                return await action().ConfigureAwait(false);
            }
            finally
            {
                ReleaseUnitGates(acquired);
            }
        }

        private Task WithUnitGatesAsync(IEnumerable<string> unitNames, string opName, CancellationToken ct, Func<Task> action)
        {
            return WithUnitGatesAsync<object>(unitNames, opName, ct, async () =>
            {
                await action().ConfigureAwait(false);
                return null;
            });
        }

        private IEnumerable<string> GetUnitNamesForSequenceStart(string sequenceName)
        {
            return GetUnitsForSequence(sequenceName)
                .Where(u => u != null && !string.IsNullOrWhiteSpace(u.UnitName))
                .Select(u => u.UnitName)
                .Distinct(StringComparer.OrdinalIgnoreCase);
        }

        private IEnumerable<string> GetUnitNamesForSequenceReady(string sequenceName)
        {
            // [FIX] Ready 단계에서 건드릴 가능성이 있는 유닛을 "합집합"으로 락:
            // 1) ReadyTasks에서 실제 호출되는 유닛
            // 2) 해당 시퀀스가 소유하는 유닛(향후 Ready가 내부적으로 이 유닛들도 건드릴 수 있으므로 방어)
            return GetUnitNamesForSequenceReadyTasks(sequenceName)
                .Concat(GetUnitNamesForSequenceStart(sequenceName))
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .Distinct(StringComparer.OrdinalIgnoreCase);
        }

        private IEnumerable<string> GetUnitNamesForSequenceReadyTasks(string sequenceName)
        {
            // BuildReadyTasks에서 실제로 건드리는 유닛 기준
            // (BuildReadyTasks의 switch와 같이 유지보수)
            switch (sequenceName)
            {
                case "InputWafer":
                    return new[] { UnitKeys.InputFeeder, UnitKeys.InputStageEjector };

                case "ChipLoading":
                    return new[] { UnitKeys.InputDieTransfer };

                case "Process":
                    return new[] { UnitKeys.IndexLoadAligner, UnitKeys.IndexChipProbeController };

                case "ChipUnloading":
                    return new[] { UnitKeys.OutputDieTransfer };

                case "OutputWafer":
                    return new[] { UnitKeys.OutputFeeder };
            }

            return Enumerable.Empty<string>();
        }

        #endregion

        #region Sequnce Helper
        // =========================
        // Sequence 통합(NEW)
        // =========================
        // [MOD] All(전체) 동작은 전역 게이트로 직렬화
        private readonly SemaphoreSlim _sequenceAllGate = new SemaphoreSlim(1, 1);

        // [ADD] 단일 시퀀스 동작은 시퀀스별 게이트로 직렬화(서로 다른 시퀀스는 동시 허용)
        private readonly object _sequenceGateMapLock = new object();
        private readonly Dictionary<string, SemaphoreSlim> _sequenceGateMap =
            new Dictionary<string, SemaphoreSlim>(StringComparer.OrdinalIgnoreCase);

        private SemaphoreSlim GetSequenceGate(string sequenceName)
        {
            if (string.IsNullOrWhiteSpace(sequenceName))
                return _sequenceAllGate;

            lock (_sequenceGateMapLock)
            {
                if (!_sequenceGateMap.TryGetValue(sequenceName, out var gate))
                {
                    gate = new SemaphoreSlim(1, 1);
                    _sequenceGateMap[sequenceName] = gate;
                }
                return gate;
            }
        }

        private readonly object _sequenceStateGate = new object();
        private readonly HashSet<string> _sequenceReady = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> _sequenceRunning = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        private static readonly string[] _sequenceOrder =
        {
            "InputWafer","ChipLoading","Process","ChipUnloading","OutputWafer"
        };

        public bool IsAnySequenceBusy
        {
            get
            {
                lock (_sequenceStateGate)
                {
                    return _sequenceRunning.Count > 0;
                }
            }
        }

        public IReadOnlyCollection<string> GetSequenceReadySnapshot()
        {
            lock (_sequenceStateGate) return _sequenceReady.ToList().AsReadOnly();
        }

        public IReadOnlyCollection<string> GetSequenceRunningSnapshot()
        {
            lock (_sequenceStateGate) return _sequenceRunning.ToList().AsReadOnly();
        }

        public async Task<bool> SequenceReadyAsync(string sequenceName, CancellationToken ct)
        {
            if (!await TryEnterPerSequenceGateAsync(sequenceName, "SequenceReadyAsync:" + sequenceName, ct).ConfigureAwait(false))
                return false;

            try
            {
                return await SequenceReadyCoreAsync(sequenceName, ct).ConfigureAwait(false);
            }
            finally
            {
                ExitPerSequenceGate(sequenceName);
            }
        }

        public async Task<bool> SequenceStartAsync(string sequenceName, CancellationToken ct)
        {
            if (!await TryEnterPerSequenceGateAsync(sequenceName, "SequenceStartAsync:" + sequenceName, ct).ConfigureAwait(false))
                return false;

            try
            {
                return await SequenceStartCoreAsync(sequenceName, ct).ConfigureAwait(false);
            }
            finally
            {
                ExitPerSequenceGate(sequenceName);
            }
        }

        public async Task SequenceStopAsync(string sequenceName, CancellationToken ct)
        {
            if (!await TryEnterPerSequenceGateAsync(sequenceName, "SequenceStopAsync:" + sequenceName, ct).ConfigureAwait(false))
                return;

            try
            {
                await SequenceStopCoreAsync(sequenceName, ct).ConfigureAwait(false);
            }
            finally
            {
                ExitPerSequenceGate(sequenceName);
            }
        }

        // ======= All API에서 SequenceXxxAsync 대신 Core 호출 =======

        public async Task<bool> SequenceReadyAllAsync(CancellationToken ct)
        {
            if (!await TryEnterSequenceAllGateAsync("SequenceReadyAllAsync", ct).ConfigureAwait(false))
                return false;

            try
            {
                EnsureAxisReadyForAutoOrMove("SequenceReadyAll");

                lock (_sequenceStateGate)
                {
                    if (_sequenceRunning.Count > 0)
                        return false;

                    _sequenceReady.Clear();
                }
                RaiseSequenceUiStateChanged();

                foreach (var seq in _sequenceOrder)
                {
                    ct.ThrowIfCancellationRequested();

                    var ok = await SequenceReadyCoreAsync(seq, ct).ConfigureAwait(false);
                    if (!ok)
                        return false;
                }

                return true;
            }
            finally
            {
                ExitSequenceAllGate();
            }
        }

        public async Task<bool> SequenceStartAllAsync(CancellationToken ct)
        {
            if (!await TryEnterSequenceAllGateAsync("SequenceStartAllAsync", ct).ConfigureAwait(false))
                return false;

            try
            {
                EnsureAxisReadyForAutoOrMove("SequenceStartAll");

                lock (_sequenceStateGate)
                {
                    if (_sequenceRunning.Count > 0)
                        return false;
                }

                // 1) 전체 Ready를 "시퀀스 단위 병렬"로 수행
                //    (각 시퀀스 내부 Ready Task도 parallel=true 이므로 결과적으로 더 동시성이 높아짐)
                var readyTasks = _sequenceOrder.Select(seq => SequenceReadyCoreAsync(seq, ct)).ToArray();
                var readyResults = await Task.WhenAll(readyTasks).ConfigureAwait(false);
                if (!readyResults.All(r => r))
                {
                    Log.Write("Equipment", "[SEQ] StartAll: 전체 Ready 실패");
                    return false;
                }

                // 2) Start 대상 유닛을 전체 시퀀스에서 한번에 수집(중복 제거)
                var allUnits = GetAllUnitsForAllSequencesDistinct();

                // Running 상태 표시(시퀀스 전체를 Running으로 표시)
                lock (_sequenceStateGate)
                {
                    _sequenceReady.Clear();
                    foreach (var seq in _sequenceOrder)
                        _sequenceRunning.Add(seq);
                }
                RaiseSequenceUiStateChanged();

                // 3) 전체 유닛 병렬 Start + Running 대기
                var startOk = await StartUnitsDistinctAsync(allUnits, ct, parallel: true).ConfigureAwait(false);
                if (!startOk)
                {
                    Log.Write("Equipment", "[SEQ] StartAll: 전체 Start 실패");

                    lock (_sequenceStateGate)
                    {
                        _sequenceRunning.Clear();
                    }

                    RaiseSequenceUiStateChanged();
                    return false;
                }

                return true;
            }
            finally
            {
                ExitSequenceAllGate();
            }
        }

        private List<BaseUnit> GetAllUnitsForAllSequencesDistinct()
        {
            var list = new List<BaseUnit>();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var seq in _sequenceOrder)
            {
                foreach (var u in GetUnitsForSequence(seq))
                {
                    if (u == null) continue;

                    var key = string.IsNullOrEmpty(u.UnitName) ? u.GetHashCode().ToString() : u.UnitName;
                    if (seen.Add(key))
                        list.Add(u);
                }
            }

            return list;
        }

        private async Task<bool> StartUnitsDistinctAsync(List<BaseUnit> units, CancellationToken ct, bool parallel)
        {
            if (units == null || units.Count == 0)
                return false;

            if (parallel)
            {
                var startTasks = units.Select(TryStartUnitAsync).ToArray();
                var started = await Task.WhenAll(startTasks).ConfigureAwait(false);
                if (!started.All(r => r))
                    return false;

                var waitTasks = units.Select(u => WaitForUnitRunningAsync(u, 5000, ct)).ToArray();
                var waited = await Task.WhenAll(waitTasks).ConfigureAwait(false);
                return waited.All(r => r);
            }

            foreach (var u in units)
            {
                ct.ThrowIfCancellationRequested();

                if (!await TryStartUnitAsync(u).ConfigureAwait(false))
                    return false;

                if (!await WaitForUnitRunningAsync(u, 5000, ct).ConfigureAwait(false))
                    return false;
            }

            return true;
        }

        public async Task<bool> SequenceStopAllAsync(CancellationToken ct)
        {
            if (!await TryEnterSequenceAllGateAsync("SequenceStopAllAsync", ct).ConfigureAwait(false))
                return false;

            try
            {
                EnsureInitializedOrThrow("SequenceStopAll");

                foreach (var seq in _sequenceOrder)
                {
                    ct.ThrowIfCancellationRequested();
                    await SequenceStopCoreAsync(seq, ct).ConfigureAwait(false);
                }

                // Unit Stop 보강 (EquipmentStatus 제외)
                if (Units != null)
                {
                    foreach (var u in Units.Values.OfType<BaseUnit>())
                    {
                        ct.ThrowIfCancellationRequested();
                        if (IsStopExemptUnit(u)) continue;

                        try
                        {
                            if (!string.IsNullOrEmpty(u.UnitName))
                                await StopUnitAsync(u.UnitName).ConfigureAwait(false);
                        }
                        catch (Exception ex) { Log.Write(ex); }
                    }
                }

                lock (_sequenceStateGate)
                {
                    _sequenceReady.Clear();
                    _sequenceRunning.Clear();
                }

                RaiseSequenceUiStateChanged();
                return true;
            }
            finally
            {
                ExitSequenceAllGate();
            }
        }

        // ======= Core 구현(게이트 획득 금지) 추가 =======

        private async Task<bool> SequenceReadyCoreAsync(string sequenceName, CancellationToken ct)
        {
            EnsureAxisReadyForAutoOrMove("SequenceReady:" + sequenceName);

            lock (_sequenceStateGate)
            {
                // [MOD] 전체 Running이 있으면 거부(X) → 해당 시퀀스가 Running이면 거부(O)
                if (_sequenceRunning.Contains(sequenceName))
                    return false;

                if (_sequenceReady.Contains(sequenceName))
                    return true;
            }

            // [ADD] 공유 유닛 충돌 방지: Ready에 필요한 유닛 gate 획득 후 실행
            var ok = await WithUnitGatesAsync(
                GetUnitNamesForSequenceReady(sequenceName),
                "ReadyCore:" + sequenceName,
                ct,
                async () => await ReadyUnitsForSequenceAsync(sequenceName, ct, parallel: true).ConfigureAwait(false)
            ).ConfigureAwait(false);

            if (!ok) return false;

            lock (_sequenceStateGate)
            {
                _sequenceReady.Add(sequenceName);
            }

            RaiseSequenceUiStateChanged();
            return true;
        }

        private async Task<bool> SequenceStartCoreAsync(string sequenceName, CancellationToken ct)
        {
            EnsureAxisReadyForAutoOrMove("SequenceStart:" + sequenceName);

            lock (_sequenceStateGate)
            {
                if (_sequenceRunning.Contains(sequenceName))
                    return true;
            }

            // Ready 보정(Core 사용) - ReadyCore 자체가 UnitGate 잡기 때문에 별도 처리 불필요
            var readyOk = await SequenceReadyCoreAsync(sequenceName, ct).ConfigureAwait(false);
            if (!readyOk) return false;

            lock (_sequenceStateGate)
            {
                _sequenceReady.Remove(sequenceName);
                _sequenceRunning.Add(sequenceName);
            }
            RaiseSequenceUiStateChanged();

            // [ADD] 공유 유닛 충돌 방지: Start에 필요한 유닛 gate 획득 후 실행
            var startOk = await WithUnitGatesAsync(
                GetUnitNamesForSequenceStart(sequenceName),
                "StartCore:" + sequenceName,
                ct,
                async () => await StartUnitsForSequenceAsync(sequenceName, ct, parallel: true).ConfigureAwait(false)
            ).ConfigureAwait(false);

            if (!startOk)
            {
                lock (_sequenceStateGate) 
                { 
                    _sequenceRunning.Remove(sequenceName); 
                }
                RaiseSequenceUiStateChanged();
                return false;
            }

            return true;
        }

        private async Task SequenceStopCoreAsync(string sequenceName, CancellationToken ct)
        {
            EnsureInitializedOrThrow("SequenceStop:" + sequenceName);

            // [ADD] 공유 유닛 충돌 방지: Stop에 필요한 유닛 gate 획득 후 실행
            await WithUnitGatesAsync(
                GetUnitNamesForSequenceStart(sequenceName),
                "StopCore:" + sequenceName,
                ct,
                async () => await StopUnitsForSequenceAsync(sequenceName).ConfigureAwait(false)
            ).ConfigureAwait(false);

            lock (_sequenceStateGate)
            {
                _sequenceReady.Remove(sequenceName);
                _sequenceRunning.Remove(sequenceName);
            }
            RaiseSequenceUiStateChanged();
        }

        // --- Gate helpers (Fail-Fast) ---

        private async Task<bool> TryEnterSequenceAllGateAsync(string opName, CancellationToken ct)
        {
            EnsureInitializedOrThrow(opName);

            bool ok = await _sequenceAllGate.WaitAsync(0, ct).ConfigureAwait(false);
            if (!ok)
            {
                Log.Write("Equipment", $"{opName} 거부: 다른 전체 시퀀스 동작이 진행 중");
                return false;
            }
            return true;
        }

        private void ExitSequenceAllGate()
        {
            try { _sequenceAllGate.Release(); } catch { }
        }

        private async Task<bool> TryEnterPerSequenceGateAsync(string sequenceName, string opName, CancellationToken ct)
        {
            EnsureInitializedOrThrow(opName);

            var gate = GetSequenceGate(sequenceName);

            bool ok = await gate.WaitAsync(0, ct).ConfigureAwait(false);
            if (!ok)
            {
                Log.Write("Equipment", $"{opName} 거부: '{sequenceName}' 시퀀스 동작이 이미 진행 중");
                return false;
            }
            return true;
        }

        private void ExitPerSequenceGate(string sequenceName)
        {
            try
            {
                var gate = GetSequenceGate(sequenceName);
                gate.Release();
            }
            catch { }
        }

        // --- Internal helpers ---
        private async Task<bool> ReadyUnitsForSequenceAsync(string sequenceName, CancellationToken ct, bool parallel = true)
        {
            var tasksFactory = BuildReadyTasks(sequenceName).ToList();
            if (tasksFactory.Count == 0)
            {
                Log.Write("Equipment", $"[SEQ] ReadyTasks 없음: seq='{sequenceName}'");
                return false;
            }

            if (parallel)
            {
                var tasks = tasksFactory.Select(f => f(ct)).ToArray();
                var rcs = await Task.WhenAll(tasks).ConfigureAwait(false);

                if (!rcs.All(rc => rc == 0))
                {
                    Log.Write("Equipment", $"[SEQ] Ready 실패: seq='{sequenceName}', rcs=[{string.Join(",", rcs)}]");
                    return false;
                }

                return true;
            }

            foreach (var f in tasksFactory)
            {
                ct.ThrowIfCancellationRequested();
                var rc = await f(ct).ConfigureAwait(false);
                if (rc != 0)
                {
                    Log.Write("Equipment", $"[SEQ] Ready 실패: seq='{sequenceName}', rc={rc}");
                    return false;
                }
            }
            return true;
        }

        private IEnumerable<Func<CancellationToken, Task<int>>> BuildReadyTasks(string sequenceName)
        {
            var inputFeeder = GetUnit(UnitKeys.InputFeeder) as InputFeeder;
            var inputDieTransfer = GetUnit(UnitKeys.InputDieTransfer) as InputDieTransfer;
            var outputDieTransfer = GetUnit(UnitKeys.OutputDieTransfer) as OutputDieTransfer;
            var outputFeeder = GetUnit(UnitKeys.OutputFeeder) as OutputFeeder;
            var inputStageEjector = GetUnit(UnitKeys.InputStageEjector) as InputStageEjector;
            var indexLoadAligner = GetUnit(UnitKeys.IndexLoadAligner) as IndexLoadAligner;
            var indexChipProbeController = GetUnit(UnitKeys.IndexChipProbeController) as IndexChipProbeController;

            switch (sequenceName)
            {
                case "InputWafer":
                    return new Func<CancellationToken, Task<int>>[]
                    {
                        ct => Task.Run(() =>
                        {
                            ct.ThrowIfCancellationRequested();
                            return inputFeeder?.EnsureReady() ?? -1;
                        }, ct),
                        ct => Task.Run(() =>
                        {
                            ct.ThrowIfCancellationRequested();
                            return inputStageEjector?.CheckReady() ?? -1;
                        }, ct),
                    };

                case "ChipLoading":
                    return new Func<CancellationToken, Task<int>>[]
                    {
                        ct => Task.Run(() =>
                        {
                            ct.ThrowIfCancellationRequested();
                            return inputDieTransfer?.EnsureReady() ?? -1;
                        }, ct)
                    };

                case "Process":
                    return new Func<CancellationToken, Task<int>>[]
                    {
                        ct => Task.Run(() =>
                        {
                            ct.ThrowIfCancellationRequested();
                            return indexLoadAligner?.EnsureReady() ?? -1;
                        }, ct),
                        ct => Task.Run(() =>
                        {
                            ct.ThrowIfCancellationRequested();
                            return indexChipProbeController?.EnsureReady() ?? -1;
                        }, ct),
                    };

                case "ChipUnloading":
                    return new Func<CancellationToken, Task<int>>[]
                    {
                        ct => Task.Run(() =>
                        {
                            ct.ThrowIfCancellationRequested();
                            return outputDieTransfer?.EnsureReady() ?? -1;
                        }, ct)
                    };

                case "OutputWafer":
                    return new Func<CancellationToken, Task<int>>[]
                    {
                        ct => Task.Run(() =>
                        {
                            ct.ThrowIfCancellationRequested();
                            return outputFeeder?.EnsureReady() ?? -1;
                        }, ct)
                    };
            }

            return Enumerable.Empty<Func<CancellationToken, Task<int>>>();
        }

        private IEnumerable<BaseUnit> GetUnitsForSequence(string sequenceName)
        {
            BaseUnit U(string key) => GetUnit(key);

            switch (sequenceName)
            {
                case "InputWafer":
                    return new[] { U(UnitKeys.InputFeeder), U(UnitKeys.InputCassetteLifter), U(UnitKeys.InputStage) }.Where(u => u != null);

                case "ChipLoading":
                    return new[] { U(UnitKeys.InputDieTransfer), U(UnitKeys.InputStageEjector) }.Where(u => u != null);

                case "Process":
                    return new[]
                    {
                        U(UnitKeys.Rotary),
                        U(UnitKeys.IndexLoadAligner),
                        U(UnitKeys.IndexChipProbeController),
                        U(UnitKeys.IndexChipProber),
                        U(UnitKeys.IndexUnloadAligner)
                    }.Where(u => u != null);

                case "ChipUnloading":
                    return new[] { U(UnitKeys.OutputDieTransfer) }.Where(u => u != null);

                case "OutputWafer":
                    return new[] { U(UnitKeys.OutputFeeder), U(UnitKeys.OutputCassetteLifter), U(UnitKeys.OutputStage) }.Where(u => u != null);
            }

            return Enumerable.Empty<BaseUnit>();
        }

        private async Task<bool> StartUnitsForSequenceAsync(string sequenceName, CancellationToken ct, bool parallel = true)
        {
            var units = GetUnitsForSequence(sequenceName).ToList();
            if (units.Count == 0) return false;

            // 중복 제거
            var distinctUnits = new List<BaseUnit>();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var u in units)
            {
                var key = string.IsNullOrEmpty(u.UnitName) ? u.GetHashCode().ToString() : u.UnitName;
                if (seen.Add(key)) distinctUnits.Add(u);
            }

            if (parallel)
            {
                var startTasks = distinctUnits.Select(TryStartUnitAsync).ToArray();
                var started = await Task.WhenAll(startTasks).ConfigureAwait(false);
                if (!started.All(r => r)) return false;

                var waitTasks = distinctUnits.Select(u => WaitForUnitRunningAsync(u, 5000, ct)).ToArray();
                var waited = await Task.WhenAll(waitTasks).ConfigureAwait(false);
                return waited.All(r => r);
            }

            foreach (var u in distinctUnits)
            {
                ct.ThrowIfCancellationRequested();
                if (!await TryStartUnitAsync(u).ConfigureAwait(false)) return false;
                if (!await WaitForUnitRunningAsync(u, 5000, ct).ConfigureAwait(false)) return false;
            }
            return true;
        }

        private async Task<bool> TryStartUnitAsync(BaseUnit unit)
        {
            if (unit == null) return false;

            var unitName = unit.UnitName;
            if (string.IsNullOrEmpty(unitName))
                return false;

            if (unit.RunUnitStatus == BaseUnit.UnitStatus.AutoRunning || unit.IsRunning)
                return true;

            return await StartUnitAsync(unitName).ConfigureAwait(false);
        }

        private async Task<bool> WaitForUnitRunningAsync(BaseUnit unit, int timeoutMs, CancellationToken ct)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            while (sw.ElapsedMilliseconds < timeoutMs)
            {
                ct.ThrowIfCancellationRequested();
                if (unit.RunUnitStatus == BaseUnit.UnitStatus.AutoRunning || unit.IsRunning)
                    return true;

                await Task.Delay(100, ct).ConfigureAwait(false);
            }

            return unit.RunUnitStatus == BaseUnit.UnitStatus.AutoRunning || unit.IsRunning;
        }

        private async Task StopUnitsForSequenceAsync(string sequenceName)
        {
            var units = GetUnitsForSequence(sequenceName).ToList();
            foreach (var u in units)
            {
                try
                {
                    if (u != null && !string.IsNullOrEmpty(u.UnitName))
                        await StopUnitAsync(u.UnitName).ConfigureAwait(false);
                }
                catch (Exception ex) { Log.Write(ex); }
            }
        }

        private static bool IsStopExemptUnit(BaseUnit u)
        {
            if (u == null) return false;
            if (u is EquipmentStatus) return true;

            var name = u.UnitName;
            if (!string.IsNullOrEmpty(name) &&
                name.Equals(UnitKeys.EquipmentStatus, StringComparison.OrdinalIgnoreCase))
                return true;

            var typeName = u.GetType().Name;
            if (string.Equals(typeName, "EquipmentStatus", StringComparison.OrdinalIgnoreCase))
                return true;

            return false;
        }


        // =========================
        // [ADD] Sequence UI Snapshot/Event (SSOT)
        // =========================
        public sealed class SequenceUiSnapshot
        {
            public IReadOnlyCollection<string> Ready { get; set; }
            public IReadOnlyCollection<string> Running { get; set; }

            // 편의
            public bool AnyRunning => Running != null && Running.Count > 0;
            public bool AnyReady => Ready != null && Ready.Count > 0;
        }

        public event EventHandler SequenceUiStateChanged;

        private void RaiseSequenceUiStateChanged()
        {
            try { SequenceUiStateChanged?.Invoke(this, EventArgs.Empty); } catch { }
        }

        public SequenceUiSnapshot GetSequenceUiSnapshot()
        {
            lock (_sequenceStateGate)
            {
                return new SequenceUiSnapshot
                {
                    Ready = _sequenceReady.ToList().AsReadOnly(),
                    Running = _sequenceRunning.ToList().AsReadOnly()
                };
            }
        }


        #endregion




    }


    ///////////////////////////////////////////////////////////////////////////////////////////////
    /// 클래스 및 열거형 정의
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