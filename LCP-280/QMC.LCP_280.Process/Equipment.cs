using QMC.Common.Unit;
using QMC.Common.Component;
using QMC.Common;
using QMC.LCP_280.Process.Unit;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using QMC.Common.Motions;
using System.IO;
using QMC.Common.DIO;
using QMC.Common.IO;
using QMC.Common.Motion.Ajin.HW;
using QMC.Common.Motion.Ajin.IO;
using System.Windows.Forms;
using QMC.Common.Motion;
using QMC.Common.Cameras.HIKVISION;
using QMC.Common.Cameras;
using QMC.Common.Keithley;
using QMC.Common.Spectrometer;

namespace QMC.LCP_280.Process
{
    /// <summary>
    /// 설비 전체를 관리하는 Equipment 클래스
    /// 모든 Unit들을 등록하고 Start/Stop/Config/Recipe를 중앙에서 제어
    /// </summary>
    public class Equipment : IDisposable
    {
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
        public ConcurrentDictionary<string, BaseUnit> Units { get; private set; }

        /// <summary>
        /// Unit별 실행 상태 관리
        /// </summary>
        private ConcurrentDictionary<string, UnitExecutionInfo> _unitExecutions;

        /// <summary>
        /// 설비 전체 상태
        /// </summary>
        public EquipmentState State { get; private set; }

        /// <summary>
        /// 설비 전체 Config 관리
        /// </summary>
        public EquipmentConfigManager ConfigManager { get; private set; }

        /// <summary>
        /// 설비 전체 Recipe 관리
        /// </summary>
        public EquipmentRecipeManager RecipeManager { get; private set; }

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


        private AjinAxlBoardHost _axlHost;                 // Ajin 보드 수명 관리(AXL.Open/Close + MOT 로드)
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


        // Sourcemeter
        public Dictionary<string, KeithleySourcemeter> Sourcemeters { get; } = new Dictionary<string, KeithleySourcemeter>(StringComparer.OrdinalIgnoreCase);
        // == 편의 프로퍼티 추가 ==
        public KeithleySourcemeter Sourcemeter => GetSourcemeter("Sourcemeter");

        private KeithleySourcemeter GetSourcemeter(string key)
        {
            return Sourcemeters.TryGetValue(key, out var sm) ? sm : null;
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

        #region Constructor & Initialization

        private Equipment()
        {
            //Units = new ConcurrentDictionary<string, BaseUnit>();
            //_unitExecutions = new ConcurrentDictionary<string, UnitExecutionInfo>();
            //State = EquipmentState.Stopped;

            //ConfigManager = new EquipmentConfigManager();
            //RecipeManager = new EquipmentRecipeManager();

            //InitializeEquipment();
        }

        /// <summary>
        /// 설비 초기화
        /// </summary>
        public void InitializeEquipment()
        {
            try
            {
                Units = new ConcurrentDictionary<string, BaseUnit>();
                _unitExecutions = new ConcurrentDictionary<string, UnitExecutionInfo>();
                State = EquipmentState.Stopped;

                ConfigManager = new EquipmentConfigManager();
                RecipeManager = new EquipmentRecipeManager();


                OnStateChanged(EquipmentState.Initializing);

                // 여기서 모든 유닛 축을 직접 생성/로드하여 붙인다.
                BootstrapAxesDirect();
                BootstrapIODirect();

                // === 카메라 초기화 ===
                InitializeCameras();

                // === Sourcemeter 초기화 ===
                InitializeSourcemeters();

                // 기본 Unit들 자동 등록 (개발자가 필요에 따라 추가)
                AutoRegisterUnits();

                OnStateChanged(EquipmentState.Ready);
                Console.WriteLine("Equipment 초기화 완료");
            }
            catch (Exception ex)
            {
                OnStateChanged(EquipmentState.Error);
                OnErrorOccurred($"설비 초기화 중 오류 발생: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 기본 Unit들 자동 등록
        /// </summary>
        private void AutoRegisterUnits()
        {

            // 개발자가 필요한 Unit들을 여기에 추가
            //RegisterUnit(new CassetteLoadingElevator(), "CassetteLoadingElevator");

            // �����ڰ� �ʿ��� Unit���� ���⿡ �߰�
            RegisterUnit(new InputCassetteLifter(), "InputCassetteLifter");

            // 추가 Unit들 예시:
            // RegisterUnit(new WaferAlignmentUnit(), "WaferAlignment");
            // RegisterUnit(new DieLoaderUnit(), "DieLoader");
            // RegisterUnit(new VisionInspectionUnit(), "VisionInspection");
        }

        #endregion

        #region Unit Registration

        /// <summary>
        /// Unit을 설비에 등록
        /// </summary>
        /// <param name="unit">등록할 Unit</param>
        /// <param name="unitName">Unit 이름</param>
        /// <param name="description">Unit 설명</param>
        public void RegisterUnit(BaseUnit unit, string unitName, string description = null)
        {
            if (unit == null) throw new ArgumentNullException(nameof(unit));
            if (string.IsNullOrEmpty(unitName)) throw new ArgumentException("Unit 이름이 필요합니다.", nameof(unitName));

            try
            {
                unit.UnitName = unitName;

                if (Units.TryAdd(unitName, unit))
                {
                    // Unit 실행 정보 초기화
                    _unitExecutions[unitName] = new UnitExecutionInfo(unitName, description);

                    // Config 및 Recipe 등록
                    ConfigManager.RegisterUnitConfig(unitName, unit.Config);
                    RecipeManager.RegisterUnitRecipe(unitName, CreateUnitRecipe(unit));

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
                    ConfigManager.UnregisterUnitConfig(unitName);
                    RecipeManager.UnregisterUnitRecipe(unitName);

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
        private BaseRecipe CreateUnitRecipe(BaseUnit unit)
        {
            // Unit 타입에 따라 적절한 Recipe 생성
            switch (unit)
            {
                case InputCassetteLifter cassetteUnit:
                    return new CassetteElevatorRecipe();
                default:
                    return new DefaultUnitRecipe(unit.UnitName);
            }
        }

        #endregion

        #region Equipment Control

        /// <summary>
        /// 설비 전체 시작
        /// </summary>
        public async Task<bool> StartAllUnitsAsync()
        {
            if (State == EquipmentState.Running)
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

                // 모든 Unit들을 병렬로 시작
                var startTasks = Units.Keys.Select(unitName => StartUnitAsync(unitName));
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
                    // 일부 Unit 시작 실패 시 전체 정지
                    await StopAllUnitsAsync();
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
            if (!Units.TryGetValue(unitName, out var unit))
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
                if (execInfo.IsRunning)
                {
                    Console.WriteLine($"Unit '{unitName}'는 이미 실행 중입니다.");
                    return true;
                }

                OnUnitStateChanged(unitName, UnitState.Starting);

                // Equipment 취소 토큰이 없으면 생성
                if (_equipmentCancellationTokenSource == null)
                {
                    _equipmentCancellationTokenSource = new CancellationTokenSource();
                }

                // Unit별 취소 토큰 생성
                execInfo.CancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_equipmentCancellationTokenSource.Token);

                // Unit 시작
                unit.OnRun();

                // Unit 실행 Task 생성 및 시작
                execInfo.ExecutionTask = Task.Run(async () =>
                    await RunUnitLoopAsync(unitName, unit, execInfo.CancellationTokenSource.Token),
                    execInfo.CancellationTokenSource.Token);

                execInfo.IsRunning = true;
                execInfo.StartTime = DateTime.Now;

                OnUnitStateChanged(unitName, UnitState.Running);
                Console.WriteLine($"Unit '{unitName}' 시작됨");
                return true;
            }
            catch (Exception ex)
            {
                OnUnitStateChanged(unitName, UnitState.Error);
                OnErrorOccurred($"Unit '{unitName}' 시작 중 오류: {ex.Message}");
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
                OnUnitStateChanged(unitName, UnitState.Error);
                OnErrorOccurred($"Unit '{unitName}' 실행 중 오류: {ex.Message}");
            }
            finally
            {
                // Unit 정리 작업
                try
                {
                    unit.OnStop();
                    OnUnitStateChanged(unitName, UnitState.Stopped);
                }
                catch (Exception ex)
                {
                    OnErrorOccurred($"Unit '{unitName}' 정지 중 오류: {ex.Message}");
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
            // Unit 타입별로 다른 작업 수행
            switch (unit)
            {
                case InputCassetteLifter cassetteUnit:
                    await PerformCassetteElevatorCycle(cassetteUnit, cancellationToken);
                    break;
                default:
                    // 기본 Unit 작업
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
        /// 설비 전체 정지
        /// </summary>
        public async Task<bool> StopAllUnitsAsync()
        {
            if (State == EquipmentState.Stopped)
            {
                Console.WriteLine("설비가 이미 정지되어 있습니다.");
                return true;
            }

            try
            {
                OnStateChanged(EquipmentState.Stopping);

                // 모든 Unit 정지 요청
                _equipmentCancellationTokenSource?.Cancel();

                // 실행 중인 Unit들의 정지 작업을 직접 수행
                var stopTasks = new List<Task<bool>>();

                foreach (var kvp in _unitExecutions)
                {
                    var unitName = kvp.Key;
                    var execInfo = kvp.Value;

                    if (execInfo.IsRunning)
                    {
                        // 상태 변경 및 취소 요청
                        OnUnitStateChanged(unitName, UnitState.Stopping);
                        execInfo.CancellationTokenSource?.Cancel();
                        execInfo.IsRunning = false;
                        execInfo.StopTime = DateTime.Now;

                        // Task 완료 대기를 위한 작업 추가
                        if (execInfo.ExecutionTask != null)
                        {
                            stopTasks.Add(WaitForUnitStopAsync(unitName, execInfo.ExecutionTask));
                        }
                    }
                }

                // 모든 정지 작업 완료 대기
                if (stopTasks.Count > 0)
                {
                    var results = await Task.WhenAll(stopTasks);
                    var allStopped = results.All(r => r);

                    if (!allStopped)
                    {
                        OnErrorOccurred("일부 Unit 정지에 실패했습니다.");
                    }
                }

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

        /// <summary>
        /// 개별 Unit 정지
        /// </summary>
        /// <param name="unitName">정지할 Unit 이름</param>
        public async Task<bool> StopUnitAsync(string unitName)
        {
            if (!_unitExecutions.TryGetValue(unitName, out var execInfo))
            {
                OnErrorOccurred($"Unit '{unitName}'를 찾을 수 없습니다.");
                return false;
            }

            try
            {
                if (!execInfo.IsRunning)
                {
                    Console.WriteLine($"Unit '{unitName}'는 이미 정지되어 있습니다.");
                    return true;
                }

                OnUnitStateChanged(unitName, UnitState.Stopping);

                // Unit 정지 요청
                execInfo.CancellationTokenSource?.Cancel();

                // 즉시 실행 상태를 false로 변경 (UI 업데이트용)
                execInfo.IsRunning = false;
                execInfo.StopTime = DateTime.Now;

                // Task 완료 대기 (최대 5초)
                if (execInfo.ExecutionTask != null)
                {
                    var timeoutTask = Task.Delay(5000);
                    var completedTask = await Task.WhenAny(execInfo.ExecutionTask, timeoutTask);

                    if (completedTask == timeoutTask)
                    {
                        OnErrorOccurred($"Unit '{unitName}' 정지 타임아웃");
                        OnUnitStateChanged(unitName, UnitState.Error);
                        return false;
                    }
                }

                OnUnitStateChanged(unitName, UnitState.Stopped);
                Console.WriteLine($"Unit '{unitName}' 정지 완료");
                return true;
            }
            catch (Exception ex)
            {
                OnUnitStateChanged(unitName, UnitState.Error);
                OnErrorOccurred($"Unit '{unitName}' 정지 중 오류: {ex.Message}");
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
                    OnUnitStateChanged(unitName, UnitState.Error);
                    return false;
                }

                Console.WriteLine($"Unit '{unitName}' 정지 완료");
                return true;
            }
            catch (Exception ex)
            {
                OnUnitStateChanged(unitName, UnitState.Error);
                OnErrorOccurred($"Unit '{unitName}' 정지 중 오류: {ex.Message}");
                return false;
            }
        }

        #endregion

        #region Config & Recipe Management

        /// <summary>
        /// 특정 Unit의 Config 가져오기
        /// </summary>
        public T GetUnitConfig<T>(string unitName) where T : class
        {
            return ConfigManager.GetUnitConfig<T>(unitName);
        }

        /// <summary>
        /// 특정 Unit의 Recipe 가져오기
        /// </summary>
        public T GetUnitRecipe<T>(string unitName) where T : BaseRecipe
        {
            return RecipeManager.GetUnitRecipe<T>(unitName);
        }

        /// <summary>
        /// 특정 Unit의 Config 설정
        /// </summary>
        public void SetUnitConfig(string unitName, object config)
        {
            if (config is BaseConfig baseConfig)
            {
                ConfigManager.SetUnitConfig(unitName, baseConfig);
            }
        }

        /// <summary>
        /// 특정 Unit의 Recipe 설정
        /// </summary>
        public void SetUnitRecipe(string unitName, BaseRecipe recipe)
        {
            RecipeManager.SetUnitRecipe(unitName, recipe);
        }

        /// <summary>
        /// 모든 Config 저장
        /// </summary>
        public bool SaveAllConfigs(string directoryPath = null)
        {
            return ConfigManager.SaveAllConfigs(directoryPath);
        }

        /// <summary>
        /// 모든 Config 로드
        /// </summary>
        public bool LoadAllConfigs(string directoryPath = null)
        {
            return ConfigManager.LoadAllConfigs(directoryPath);
        }

        /// <summary>
        /// 모든 Recipe 저장
        /// </summary>
        public bool SaveAllRecipes(string directoryPath = null)
        {
            return RecipeManager.SaveAllRecipes(directoryPath);
        }

        /// <summary>
        /// 모든 Recipe 로드
        /// </summary>
        public bool LoadAllRecipes(string directoryPath = null)
        {
            return RecipeManager.LoadAllRecipes(directoryPath);
        }

        #endregion

        #region Status & Information

        /// <summary>
        /// 모든 Unit의 상태 정보 가져오기
        /// </summary>
        public Dictionary<string, UnitStatusInfo> GetAllUnitStatus()
        {
            var statusDict = new Dictionary<string, UnitStatusInfo>();

            foreach (var kvp in Units)
            {
                var unitName = kvp.Key;
                var unit = kvp.Value;
                var execInfo = _unitExecutions.ContainsKey(unitName) ? _unitExecutions[unitName] : null;

                statusDict[unitName] = new UnitStatusInfo
                {
                    UnitName = unitName,
                    Description = execInfo?.Description ?? "",
                    IsRunning = execInfo?.IsRunning ?? false,
                    State = GetUnitCurrentState(unitName),
                    ComponentCount = unit.Components.Count,
                    StartTime = execInfo?.StartTime,
                    RunningTime = execInfo?.IsRunning == true && execInfo.StartTime.HasValue
                        ? DateTime.Now - execInfo.StartTime.Value
                        : TimeSpan.Zero,
                    LastUpdateTime = DateTime.Now
                };
            }

            return statusDict;
        }

        /// <summary>
        /// 특정 Unit의 현재 상태 가져오기
        /// </summary>
        private UnitState GetUnitCurrentState(string unitName)
        {
            if (_unitExecutions.TryGetValue(unitName, out var execInfo))
            {
                if (execInfo.IsRunning)
                    return UnitState.Running;
                else if (execInfo.ExecutionTask?.IsFaulted == true)
                    return UnitState.Error;
                else
                    return UnitState.Stopped;
            }
            return UnitState.Unknown;
        }

        /// <summary>
        /// 등록된 모든 Unit 이름 목록 가져오기
        /// </summary>
        public List<string> GetRegisteredUnitNames()
        {
            return Units.Keys.ToList();
        }

        #endregion

        #region Event Methods

        private void OnStateChanged(EquipmentState newState)
        {
            var oldState = State;
            State = newState;
            StateChanged?.Invoke(this, new EquipmentStateChangedEventArgs(oldState, newState));
        }

        private void OnUnitStateChanged(string unitName, UnitState newState)
        {
            UnitStateChanged?.Invoke(this, new UnitStateChangedEventArgs(unitName, newState));
        }

        private void OnErrorOccurred(string errorMessage)
        {
            ErrorOccurred?.Invoke(this, new EquipmentErrorEventArgs(errorMessage));
            Console.WriteLine($"Equipment Error: {errorMessage}");
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // 모든 Unit 정지
                StopAllUnitsAsync().GetAwaiter().GetResult();

                // 리소스 정리
                _equipmentCancellationTokenSource?.Dispose();

                foreach (var execInfo in _unitExecutions.Values)
                {
                    execInfo.CancellationTokenSource?.Dispose();
                    execInfo.ExecutionTask?.Dispose();
                }

                _unitExecutions.Clear();

                // Unit들 정리
                foreach (var unit in Units.Values)
                {
                    if (unit is IDisposable disposableUnit)
                    {
                        disposableUnit.Dispose();
                    }
                }

                Units.Clear();
            }
        }

        #endregion

        // === 프로그램 시작시에 1회 호출: 유닛별 필요한 축을 생성/등록/부착 ===
        private void BootstrapAxesDirect()
        {
            // 1) Ajin 보드 오픈 + MOT 로드 (한 번만)
            if (_axlHost == null)
            {
                var motPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "LCP-280.mot");
                if (File.Exists(motPath))
                {
                    _axlHost = new AjinAxlBoardHost(motPath);
                    _axlHost.Open(); // AXL.Open + AxmMotLoadParaAll
                }
                else
                {
                    _axlHost = new AjinAxlBoardHost(motPath);
                    //_axlHost.Open(); // AXL.Open + AxmMotLoadParaAll
                }
            }

            Directory.CreateDirectory(_axisRoot);
            CreateAxes();

            // 예) CassetteLoadingElevator 유닛의 Z축 하나 생성/등록/부착
            //    필요에 맞게 더 추가(Y, X 등)
            //var axisZ = CreateOrLoadAxis("CassetteLoadingElevator", "ElevatorZ", axisNo: 0, boardNo: 0);
            // 매니저에 등록 (이름 중복 시 예외 방지 위해 TryRegister 사용도 가능)
            //_axisManager.Register(axisZ);
            // 유닛 인스턴스에 붙이기 (AxisZ 프로퍼티/필드 or Axes 딕셔너리 등으로 자동 주입 시도)
            //AttachAxisToUnit("CassetteLoadingElevator", "AxisZ", axisZ);

            // === 필요시 다른 유닛/축도 동일 방식으로 추가 ===
            // var axisX = CreateOrLoadAxis("Prober", "StageX", 1, 0);
            // _axisManager.Register(axisX;
            // AttachAxisToUnit("Prober", "AxisX", axisX);
        }

        private void BootstrapIODirect()
        {
            // 2) I/O 드라이버 (AjinDioDriver)
            if (_dio == null)
            {
                // (board, port) -> moduleNo 매핑: 현장 EtherCAT 구성에 맞게 수정
                AjinDioDriver.ModuleMapper map = delegate (int b, int p) { return b * 8 + p; };
                _dio = new AjinDioDriver(map);
            }

            // 3) I/O 스캔 서비스 시작 (Setup JSON 사용)
            if (_dioScan == null)
            {
                Directory.CreateDirectory(_dioRoot);
                //var setupPath = Path.Combine(_dioRoot, "Unit.dio.setup.json"); // CassetteLoadingElevator 등 유닛별로 나눠도 OK
                var setupPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configs", "Unit.dio.setup.json");
                // 유닛 DIO 맵 로드/없으면 생성
                _unitIO = DIOUnit.LoadOrCreateDefault(
                    setupPath,
                    unitName: "Unit",   // 네가 축을 "Unit"으로 묶었으니 동일 명칭 사용. 필요하면 유닛별 파일로 분리.
                    32,
                    32,
                    "DB64R"
                );

                _dioScan = new DioScanService(_unitIO, _dio);
                _dioScan.Start(10); // 10ms 주기 스캔
            }

            // Cylinder
            // 경로
            //string cylPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "cylinders.json");
            string cylPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configs", "cylinders.json");

            // 기본값(최초 1회용)
            var defaults = new List<CylinderConfig>
            {
                new CylinderConfig {
                    Name="Input Feeder Up",
                    ModuleName="DIO Module1",
                    ForwardIn="X20", BackwardIn="X21",
                    ForwardOut="Y30", BackwardOut="Y31",
                    TimeoutMs=5000, SettleMs=50, Monitoring=true
                }
            };

            // 로드 또는 생성
            var all = CylinderConfigs.LoadOrCreate(cylPath, defaults);

            // 조회/수정
            var feeder = all.Get("Input Feeder Up");
            feeder.TimeoutMs = 4000;
            all.Upsert(feeder);

            // 저장
            all.Save(cylPath);
            ////////////////////////////////////////////////////////////////

            if (!_axlHost.IsOpen)
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
                var names = new[]
                {
                "Input Lifter Z Axis",
                "Input Feeder Y Axis",
                "Input Stage X Axis",
                "Input Stage Y Axis",
                "Input Stage T Axis",
                "Needle Z Axis",
                "Tool#1 T Axis",
                "Tool#1 Z Axis",
                "Tool#2 T Axis",
                "Tool#2 Z Axis",
                "Tool#3 T Axis",
                "Tool#3 Pick Z Axis",
                "Tool#3 Place Z Axis",
                "Align Stage X Axis",
                "Align Stage Y Axis",
                "Align Stage T Axis",
                "Index T Axis",
                "Contact Z Axis",
                "Sphere Z Axis",
                "Output Stage X Axis",
                "Output Stage Y Axis",
                "Output Stage T Axis",
                "Output Lifter Z Axis",
                "Output Feeder Y Axis"
            };

                var boardNo = 0; // 필요시 보드별로 바꾸세요.

                for (int i = 0; i < names.Length; i++)
                {
                    var axis = CreateOrLoadAxis(unitName, names[i], axisNo: i, boardNo: boardNo);
                    _axisManager.Register(unitName, axis);
                    AttachAxisToUnit(unitName, "Axis_" + i, axis); // 프로퍼티가 없어도 Axes 딕셔너리에 자동 추가됨
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
        }

        // === 축 1개 생성/로드 ===
        // 저장 경로: Axes/<UnitName>/<AxisName>.setup.json(.config.json)
        private MotionAxis CreateOrLoadAxis(string unitName, string axisName, int axisNo, int boardNo)
        {
            var dir = Path.Combine(_axisRoot, unitName);
            Directory.CreateDirectory(dir);

            var setupPath = Path.Combine(dir, axisName + ".setup.json");
            var configPath = Path.Combine(dir, axisName + ".config.json");

            MotionAxisSetup setup;
            MotionAxisConfig config;

            // Setup
            if (File.Exists(setupPath))
            {
                //setup = MotionAxisSetup.Load(setupPath);
                setup = MotionAxisSetup.LoadOrCreate(setupPath, indented: true, backfill: true);
            }
            else
            {
                setup = new MotionAxisSetup
                {
                    Name = axisName,
                    AxisNo = axisNo,
                    BoardNo = boardNo,
                    PulsesPerUnit = 1000,
                    SoftLimitEnable = true,
                    SoftLimitMin = -10,
                    SoftLimitMax = 310
                };
                string err;
                setup.TrySave(setupPath, out err);
            }

            // Config
            if (File.Exists(configPath))
            {
                //config = MotionAxisConfig.Load(configPath);
                config = MotionAxisConfig.LoadOrCreate(configPath, indented: true, backfill: true);
            }
            else
            {
                config = new MotionAxisConfig
                {
                    MaxVelocity = 200,
                    RunAcc = 800,
                    RunDec = 800,
                    InposTolerance = 0.002,
                    ProfileMode = ProfileMode.SCurve,
                    AccJerkPercent = 50,
                    DecJerkPercent = 50
                };
                string err;
                config.TrySave(configPath, out err);
            }

            // 드라이버: 실제 보드 준비되면 AjinDriver로 교체
            IMotionDriver driver = new AjinDriver(boardNo, setup.PulsesPerUnit, useLogicalUnits: true);
            //IMotionDriver driver = new SimDriver(setup.PulsesPerUnit);

            return new MotionAxis(setup, config, driver);
        }

        // === 생성된 축을 유닛 인스턴스에 주입 ===
        // 1) 같은 이름의 MotionAxis 타입 프로퍼티/필드가 있으면 거기에 세팅 (예: public MotionAxis AxisZ {get;set;})
        // 2) 없으면 'Axes'라는 IDictionary<string, MotionAxis> 프로퍼티/필드를 찾아 추가
        // 3) 둘 다 없으면 장비의 _axisManager 만에 등록된 상태로 유지(필요시 여기서 매핑표 저장 가능)
        private void AttachAxisToUnit(string unitName, string targetMemberName, MotionAxis axis)
        {
            BaseUnit unit;
            if (!Units.TryGetValue(unitName, out unit))
            {
                OnErrorOccurred("AttachAxisToUnit: Unit '" + unitName + "' not found.");
                return;
            }

            var t = unit.GetType();

            // 1) 같은 이름의 프로퍼티 먼저 시도
            var p = t.GetProperty(targetMemberName,
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.NonPublic);
            if (p != null && p.CanWrite && p.PropertyType == typeof(MotionAxis))
            {
                p.SetValue(unit, axis, null);
                return;
            }

            // 1-2) 같은 이름의 필드
            var f = t.GetField(targetMemberName,
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.NonPublic);
            if (f != null && f.FieldType == typeof(MotionAxis))
            {
                f.SetValue(unit, axis);
                return;
            }

            // 2) Axes 딕셔너리 찾기
            //    public Dictionary<string, MotionAxis> Axes {get;} 또는 IDictionary<string, MotionAxis>
            var axesProp = t.GetProperty("Axes",
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.NonPublic);
            if (axesProp != null && typeof(System.Collections.IDictionary).IsAssignableFrom(axesProp.PropertyType))
            {
                var dict = axesProp.GetValue(unit, null) as System.Collections.IDictionary;
                if (dict != null)
                {
                    dict[axis.Name] = axis; // axis.Name == axisName (예: ElevatorZ)
                    return;
                }
            }

            var axesField = t.GetField("Axes",
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.NonPublic);
            if (axesField != null && typeof(System.Collections.IDictionary).IsAssignableFrom(axesField.FieldType))
            {
                var dict = axesField.GetValue(unit) as System.Collections.IDictionary;
                if (dict != null)
                {
                    dict[axis.Name] = axis;
                    return;
                }
            }

            // 3) 주입할 곳이 없으면 로그만
            Console.WriteLine("AttachAxisToUnit: '" + unitName + "'에 '" + targetMemberName + "' 또는 Axes 딕셔너리가 없어 축을 직접 주입하지 못했습니다.");
        }

        //private void InitializeCameras()
        //{
        //    try
        //    {
        //        // 파일로 변경 필요. ( Equipment_Setup.json )
        //        var map = new Dictionary<string, string>
        //        {
        //            { "InputStageCam",       "KV1" },        // Serial
        //            { "OutputStageCam",      "KV2" },        // Serial
        //            { "IndexLoadAlignCam",   "KV3" },        // Serial
        //            { "IndexProcessCam",     "KV4" },        // Serial
        //            { "IndexUnloadAlignCam", "KV5" }         // Serial
        //        };

        //        foreach (var kv in map)
        //        {
        //            var name = kv.Key;
        //            var selector = kv.Value;

        //            var cam = new HIKGigECamera(name);
        //            //cam.CameraConfig = CameraConfig.LoadOrCreate(name);   // JSON 로드 (없으면 생성)
        //            cam.CameraConfig = CameraConfig.LoadOrCreate(name, indented: true, backfill: true);

        //            int ret = cam.OpenBySelectorOrConfig(selector);       // 여기서 열거→매칭→Open
        //            if (ret != 0)
        //            {
        //                Log.Write("Equipment", $"[Camera] '{name}' open failed ({selector})");
        //                continue;
        //            }

        //            // 필요 시 바로 라이브 시작
        //            cam.StartLive();

        //            Cameras[name] = cam;
        //            Console.WriteLine($"[Camera] {name} ready");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.Write(ex);
        //    }
        //}

        // 사용 예:
        // 1) 프로그램 시작 시:  InitializeCameras(connect: false);   // ⚡ config만 미리 로드/생성
        // 2) 나중에 연결 시도:  InitializeCameras(connect: true);    // 🔌 실제 연결

        private void InitializeCameras(bool connect = true)
        {
            try
            {
                // 파일로 변경 필요. ( Equipment_Setup.json )
                var map = new Dictionary<string, string>
                {
                    { "Index_Loader",       "DA7484884" },        // Serial
                    { "In_Stage",           "DA7500464" },        // Serial
                    { "Index_Prober",       "DA7484883" },         // Serial
                    { "Index_Unloader",     "DA7484882" },        // Serial
                    { "Out_Stage",          "DA7500465" }        // Serial
                };

                foreach (var kv in map)
                {
                    var name = kv.Key;
                    var selector = kv.Value;

                    // 1) ⚠ 네이티브 코드 전혀 안 건드리고, Config만 안전하게 로드/생성
                    CameraConfig cfg = null;
                    try
                    {
                        //cfg = CameraConfig.LoadOrCreate(name, indented: true, backfill: true);
                        cfg = CameraConfig.LoadOrCreate(name);
                    }
                    catch (Exception exCfg)
                    {
                        Log.Write("Equipment", $"[Camera] config load failed for '{name}': {exCfg.Message}");
                        // config 자체가 없으면 연결도 의미 없으니 다음 카메라로
                        continue;
                    }

                    // 2) 연결 분리: connect=false면 여기서 끝
                    if (!connect)
                        continue;

                    // 3) 연결 단계 (이때부터 네이티브 DLL 필요)
                    try
                    {
                        var cam = new HIKGigECamera(name);   // 이 시점까지도 튈 수 있으니 try 내부에 둠
                        cam.CameraConfig = cfg;

                        int ret = cam.OpenBySelectorOrConfig(selector); // 열거→매칭→Open
                        //(ret != 0)
                        {
                            Log.Write("Equipment", $"[Camera] '{name}' open failed ({selector}), code=0x{ret:X8}");
                            //continue;
                        }

                        // 필요 시 바로 라이브 시작
                        ret = cam.StartLive();
                        if (ret != 0)
                        {
                            Log.Write("Equipment", $"[Camera] '{name}' StartLive failed, code=0x{ret:X8}");
                            // 라이브 실패는 치명 아님: 연결만 유지하고 넘어가도 됨
                        }

                        Cameras[name] = cam;
                        Console.WriteLine($"[Camera] {name} ready");
                    }
                    catch (DllNotFoundException ex) { Log.Write(ex); break; } // SDK 미배포/경로
                    catch (BadImageFormatException ex) { Log.Write(ex); break; } // x86/x64 불일치
                    catch (EntryPointNotFoundException ex) { Log.Write(ex); break; } // DLL 버전 미스매치
                    catch (Exception ex) { Log.Write(ex); /* 이 장치만 스킵 */ }
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
        }

        private void InitializeSourcemeters()
        {
            // 파일로 변경 필요. ( Equipment_Setup.json )
            var list = new List<string>()
            {
                "Index_Prober_Sourcemeter"
            };

            foreach (string name in list)
            {
                try
                {
                    var smu = new KeithleySourcemeter(name);
                    Sourcemeters[name] = smu;
                    Console.WriteLine($"[Sourcemeter] {name} ready");
                }
                catch (Exception ex)
                {
                    Log.Write(ex);
                }
            }
        }

    }

    #region Supporting Classes and Enums

    /// <summary>
    /// 설비 상태
    /// </summary>
    public enum EquipmentState
    {
        Stopped,
        Initializing,
        Ready,
        Starting,
        Running,
        Stopping,
        Error
    }

    /// <summary>
    /// Unit 상태
    /// </summary>
    public enum UnitState
    {
        Stopped,
        Starting,
        Running,
        Stopping,
        Error,
        Unknown
    }

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
        public UnitState State { get; set; }
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
        public UnitState State { get; }

        public UnitStateChangedEventArgs(string unitName, UnitState state)
        {
            UnitName = unitName;
            State = state;
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