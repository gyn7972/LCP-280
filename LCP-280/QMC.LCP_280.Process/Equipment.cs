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

        #endregion

        #region Constructor & Initialization

        private Equipment()
        {
            Units = new ConcurrentDictionary<string, BaseUnit>();
            _unitExecutions = new ConcurrentDictionary<string, UnitExecutionInfo>();
            State = EquipmentState.Stopped;

            ConfigManager = new EquipmentConfigManager();
            RecipeManager = new EquipmentRecipeManager();

            InitializeEquipment();
        }

        /// <summary>
        /// 설비 초기화
        /// </summary>
        private void InitializeEquipment()
        {
            try
            {
                OnStateChanged(EquipmentState.Initializing);

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
            RegisterUnit(new CassetteLoadingElevator(), "CassetteLoadingElevator");
            RegisterUnit(new WaferInputStage(), "WaferInputStage");
            RegisterUnit(new WaferAlignmentSystem(), "WaferAlignmentSystem");
            RegisterUnit(new DieLoaderIndexer(), "DieLoaderIndexer");
            RegisterUnit(new Prober(), "Prober");
            RegisterUnit(new DieUnloaderIndexer(), "DieUnloaderIndexer");
            RegisterUnit(new WaferOutputStage(), "WaferOutputStage");
            RegisterUnit(new CassetteUnloadingElevator(), "CassetteUnloadingElevator");

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
                case CassetteLoadingElevator cassetteUnit:
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
                case CassetteLoadingElevator cassetteUnit:
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
        private async Task PerformCassetteElevatorCycle(CassetteLoadingElevator unit, CancellationToken cancellationToken)
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

        #region Motion Axis Initialization

        /// <summary>
        /// 외부에서 Motion 연결이 완료된 후 호출하여 모든 Unit 의 축을 초기화.
        /// IMotionAxisProvider 는 호출자가 구현/전달.
        /// </summary>
        public void InitializeAllUnitAxes(IMotionAxisProvider provider)
        {
            if (provider == null) return;
            foreach (var unit in Units.Values)
            {
                unit.InitializeUnitAxes(provider);
            }
        }

        #endregion
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