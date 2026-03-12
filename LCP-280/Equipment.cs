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
using QMC.Common.GEMSecs;
using System.Diagnostics;
using System.Drawing;
using QMC.Common.Alarm;

namespace QMC.LCP_280.Process
{
    ///////////////////////////////////////////////////////////////////////////////////////////////
    /// 클래스 및 열거형 정의
    #region Supporting Classes and Enums

    /// <summary>
    /// Unit 실행 정보
    /// </summary>
    // [FIX] Equipment._unitExecutions field is private, but its *type arguments* must also be accessible.
    // Keep UnitExecutionInfo internal, and do not expose it in any public signature.
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

    public class Equipment : IDisposable, IEquipment
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

        public sealed class SequenceUiSnapshot
        {
            public IReadOnlyCollection<string> Ready { get; set; }
            public IReadOnlyCollection<string> Running { get; set; }
        }

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

        // =========================================================================================
        // [UNIT] Fields / Properties / Events / Registration / StartStop / Status / Gates
        // =========================================================================================
        #region Unit Core (Fields/Props/Events)
        /// <summary>설비에 등록된 모든 Unit들</summary>
        public ConcurrentDictionary<string, IUnit> Units { get; private set; }

        /// <summary>Unit별 실행 상태 관리</summary>
        private ConcurrentDictionary<string, UnitExecutionInfo> _unitExecutions;

        /// <summary>설비 전체에서 공유하는 취소 토큰</summary>
        private CancellationTokenSource _equipmentCancellationTokenSource;

        /// <summary>Unit 상태 브로드캐스트 중복 방지 캐시</summary>
        private readonly ConcurrentDictionary<string, UnitStatus> _lastBroadcastUnitStatus =
            new ConcurrentDictionary<string, UnitStatus>(StringComparer.OrdinalIgnoreCase);

        /// <summary>Unit 상태 변경 이벤트</summary>
        public event EventHandler<UnitStateChangedEventArgs> UnitStateChanged;

        internal bool TryGetExecutionInfo(string unitName, out UnitExecutionInfo info)
        {
            info = null;
            return _unitExecutions != null && _unitExecutions.TryGetValue(unitName, out info);
        }

        // === Protected Unit Helper (EquipmentStatus 는 항상 실행 유지) ===
        private bool IsProtectedUnit(string unitName)
            => string.Equals(unitName, UnitKeys.EquipmentStatus, StringComparison.OrdinalIgnoreCase);

        private bool IsProtectedUnit(IUnit unit) => unit is EquipmentStatus;

        #endregion

        #region Unit Registration
        /// <summary>기본 Unit들 자동 등록 (필요 시 추가/삭제)</summary>
        private void AutoRegisterUnits()
        {
            // 중복 등록 방지: 이미 존재하면 스킵
            void TryAdd(IUnit u, string name)
            {
                if (!Units.ContainsKey(name))
                    RegisterUnit(u, name);
            }

            TryAdd(new InputCassetteLifter(), UnitKeys.InputCassetteLifter);
            TryAdd(new InputFeeder(), UnitKeys.InputFeeder);
            TryAdd(new InputStage(), UnitKeys.InputStage);
            TryAdd(new InputStageEjector(), UnitKeys.InputStageEjector);
            TryAdd(new InputDieTransfer(), UnitKeys.InputDieTransfer);
            TryAdd(new Rotary(), UnitKeys.Rotary);
            TryAdd(new IndexLoadAligner(), UnitKeys.IndexLoadAligner);
            TryAdd(new IndexChipProbeController(), UnitKeys.IndexChipProbeController);
            TryAdd(new IndexChipProber(), UnitKeys.IndexChipProber);
            TryAdd(new IndexUnloadAligner(), UnitKeys.IndexUnloadAligner);
            TryAdd(new OutputDieTransfer(), UnitKeys.OutputDieTransfer);
            TryAdd(new OutputStage(), UnitKeys.OutputStage);
            TryAdd(new OutputCassetteLifter(), UnitKeys.OutputCassetteLifter);
            TryAdd(new OutputFeeder(), UnitKeys.OutputFeeder);
            TryAdd(new EquipmentStatus(), UnitKeys.EquipmentStatus); // 신규 상태 유닛
        }
        /// <summary>Unit을 설비에 등록</summary>
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
                    _unitExecutions[unitName] = new UnitExecutionInfo(unitName, description);

                    var cfg = (unit as BaseUnit)?.Config;
                    if (cfg != null)
                        ConfigManager.RegisterUnitConfig(unitName, cfg);

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
        /// <summary>Unit 등록 해제</summary>
        public bool UnregisterUnit(string unitName)
        {
            try
            {
                if (IsProtectedUnit(unitName))
                    throw new InvalidOperationException("EquipmentStatus 는 해제할 수 없습니다.");

                if (_unitExecutions.TryGetValue(unitName, out var execInfo) && execInfo.IsRunning)
                {
                    //StopUnitAsync(unitName).GetAwaiter().GetResult();
                    SequenceStopAsync(unitName, CancellationToken.None).GetAwaiter().GetResult();
                }

                bool removed = Units.TryRemove(unitName, out var unit);
                if (removed)
                {
                    _unitExecutions.TryRemove(unitName, out _);

                    if (unit is IDisposable disposableUnit)
                        disposableUnit.Dispose();

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
        #endregion

        #region Unit Start/Stop

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
                if (execInfo.IsRunning)
                    return true;

                SetAndRaiseUnitState(unitName, UnitStatus.Starting);

                if (_equipmentCancellationTokenSource == null
                    || _equipmentCancellationTokenSource.IsCancellationRequested)
                {
                    try { _equipmentCancellationTokenSource?.Dispose(); } catch { }
                    _equipmentCancellationTokenSource = new CancellationTokenSource();
                }

                var linkedCts = IsProtectedUnit(unitObj)
                    ? new CancellationTokenSource()
                    : CancellationTokenSource.CreateLinkedTokenSource(_equipmentCancellationTokenSource.Token);

                execInfo.CancellationTokenSource = linkedCts;
                execInfo.IsRunning = true;
                execInfo.StartTime = DateTime.Now;

                execInfo.ExecutionTask = Task.Run(() =>
                {
                    try
                    {
                        (unitObj as BaseUnit)?.Start();
                        SetAndRaiseUnitState(unitName, UnitStatus.AutoRunning);
                    }
                    catch (OperationCanceledException) { }
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
        public async Task<bool> StopUnitAsync(string unitName)
        {
            if (string.IsNullOrWhiteSpace(unitName))
                return false;

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
                    SetAndRaiseUnitState(unitName, UnitStatus.Stopping);

                (unitObj as QMC.Common.Unit.BaseUnit)?.Stop();
                execInfo.CancellationTokenSource?.Cancel();

                bool byTask = true;
                if (execInfo.ExecutionTask != null)
                    byTask = await WaitForUnitStopAsync(unitName, execInfo.ExecutionTask).ConfigureAwait(false);

                var bu = unitObj as QMC.Common.Unit.BaseUnit;
                bool byState = await PollUnitFullyStoppedAsync(unitName, bu, timeoutMs: 20000, pollMs: 80).ConfigureAwait(false);

                bool ok = byTask && byState;
                Log.Write("Equipment", $"Unit '{unitName}' 정지 {(ok ? "완료" : "실패/타임아웃")} (task={byTask}, state={byState})");

                SetAndRaiseUnitState(unitName, ok ? UnitStatus.Stopped : UnitStatus.Error);
                return ok;
            }
            catch (Exception ex)
            {
                SetAndRaiseUnitState(unitName, UnitStatus.Error);
                Log.Write(ex);
                return false;
            }
        }

        public async Task<bool> TerminateAllUnitsAsync()
        {
            foreach (var kvp in _unitExecutions)
            {
                var unitName = kvp.Key;

                SetAndRaiseUnitState(unitName, UnitStatus.Stopping);
                if (Units.TryGetValue(unitName, out var u))
                {
                    (u as QMC.Common.Unit.BaseUnit)?.Stop();
                    (u as QMC.Common.Unit.BaseUnit)?.Terminate();
                }
            }
            return true;
        }
        private async Task<bool> WaitForUnitStopAsync(string unitName, Task executionTask)
        {
            try
            {
                var timeoutTask = Task.Delay(10000);
                var completedTask = await Task.WhenAny(executionTask, timeoutTask).ConfigureAwait(false);

                if (completedTask == timeoutTask)
                {
                    Log.Write("Equipment", $"Unit '{unitName}' 정지 타임아웃(ExecutionTask)");
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
        private async Task<bool> PollUnitFullyStoppedAsync(string unitName, BaseUnit unit, int timeoutMs, int pollMs)
        {
            if (unit == null)
                return true;

            var sw = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                while (sw.ElapsedMilliseconds < timeoutMs)
                {
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
        #endregion

        #region Unit Status Helpers

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
                    var comps = unit?.Components;
                    compCount = comps != null ? comps.Count : 0;
                }
                catch (Exception ex)
                {
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
        }

        public BaseUnit GetUnit(string name)
        {
            Units.TryGetValue(name, out var unit);
            return unit as BaseUnit;
        }

        #endregion

        #region Unit State Events (Setter/Broadcast)

        public void SetAndRaiseUnitState(string unitName, UnitStatus newState)
        {
            if (string.IsNullOrWhiteSpace(unitName))
                return;

            if (IsProtectedUnit(unitName) && !_isShuttingDown)
            {
                if (newState == UnitStatus.Stopping || newState == UnitStatus.Stopped)
                {
                    return;
                }
            }

            if (Units != null && Units.TryGetValue(unitName, out var u) && u is BaseUnit bu)
            {
                if (bu.RunUnitStatus != newState)
                    bu.RunUnitStatus = newState;
            }

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

            // [ADD] Unit Error는 즉시 설비 Error로 승격(상태 set은 ApplyEquipmentState 한 곳에서만)
            if (newState == UnitStatus.Error)
            {
                ApplyEquipmentState(EquipmentState.Error, reason: $"UnitError:{unitName}");
            }

            if (_lastBroadcastUnitStatus.TryGetValue(unitName, out var last) && last == newState)
                return;

            _lastBroadcastUnitStatus[unitName] = newState;

            OnUnitStateChanged(unitName, newState);
        }

        private void OnUnitStateChanged(string unitName, UnitStatus newState)
        {
            UnitStateChanged?.Invoke(this, new UnitStateChangedEventArgs(unitName, newState));
        }

        #endregion

        #region Unit Gates (Shared-Unit Collision Avoidance)

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

        private void ReleaseUnitGates(List<SemaphoreSlim> acquired)
        {
            if (acquired == null) return;
            for (int i = acquired.Count - 1; i >= 0; i--)
            {
                try { acquired[i]?.Release(); } catch { }
            }
        }

        public async Task<T> WithUnitGatesAsync<T>(IEnumerable<string> unitNames, string opName, CancellationToken ct, Func<Task<T>> action)
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
        #endregion

        #region 04. Sequence (EquipmentSequence Bridge)

        // 1) 단일 시퀀스 엔진 인스턴스
        private readonly EquipmentSequence _sequenceEngine;

        public EquipmentSequence SequenceEngine => _sequenceEngine;

        // 2) UI에서 쓰는 이벤트/스냅샷은 엔진을 통해 제공
        public event EventHandler SequenceUiStateChanged
        {
            add { _sequenceEngine.SequenceUiStateChanged += value; }
            remove { _sequenceEngine.SequenceUiStateChanged -= value; }
        }

        public SequenceUiSnapshot GetSequenceUiSnapshot()
            => _sequenceEngine.GetSequenceUiSnapshot();

        public Task<bool> SequenceReadyAsync(string sequenceName, CancellationToken ct)
            => _sequenceEngine.SequenceReadyAsync(sequenceName, ct);

        public Task<bool> SequenceStartAsync(string sequenceName, CancellationToken ct)
            => _sequenceEngine.SequenceStartAsync(sequenceName, ct);

        public Task<bool> SequenceResetAsync(string sequenceName, CancellationToken ct)
            => _sequenceEngine.SequenceResetAsync(sequenceName, ct);

        public Task<bool> SequenceCycleStopAsync(string sequenceName, CancellationToken ct)
            => _sequenceEngine.SequenceCycleStopAsync(sequenceName, ct);

        public Task SequenceStopAsync(string sequenceName, CancellationToken ct)
            => _sequenceEngine.SequenceStopAsync(sequenceName, ct);

        // All
        public Task<bool> SequenceReadyAllAsync(CancellationToken ct)
            => _sequenceEngine.SequenceReadyAllAsync(ct);

        public Task<bool> SequenceStartAllAsync(CancellationToken ct)
            => _sequenceEngine.SequenceStartAllAsync(ct);

        public Task<bool> SequenceStopAllAsync(CancellationToken ct)
            => _sequenceEngine.SequenceStopAllAsync(ct);

        public Task<bool> SequenceResetAllAsync(CancellationToken ct)
            => _sequenceEngine.SequenceResetAllAsync(ct);

        public Task<bool> SequenceCycleStopAllAsync(CancellationToken ct)
            => _sequenceEngine.SequenceCycleStopAllAsync(ct);

        #endregion


        #region 05. Equipment State / Lifecycle Flags
        private volatile bool _isShuttingDown;
        private volatile bool _isEquipmentInitialized;
        private volatile bool _isAxisHomed;

        public EquipmentState EqState { get; set; }
        public bool IsEquipmentInitialized => _isEquipmentInitialized;
        public bool IsAxisHomed => _isAxisHomed;

        private bool _disposed;
        private volatile bool _threadPoolWarmed;

        private bool _alarmSummaryHooked;
        private void HookAlarmCountToSummaryOnce()
        {
            if (_alarmSummaryHooked) return;
            _alarmSummaryHooked = true;

            QMC.Common.Alarm.AlarmManager.Instance.AlarmAdded += _ =>
            {
                try
                {
                    var ctx = this.SummaryContext;
                    if (ctx != null && ctx.IsActive)
                        ctx.AddAlarm(1);
                }
                catch
                {
                    // 집계 실패가 알람 흐름을 막으면 안 됨
                }
            };
        }

        #endregion

        #region 06. Config / Recipe (Equipment-level)
        public EquipmentConfigManager ConfigManager { get; set; }
        public EquipmentConfig EquipmentConfig { get; set; }
        public EquipmentRecipe EquipmentRecipe { get; set; }
        public string ICurrentRecipe { get; set; }

        // [ADD] 현재 사용자 ID 보관용 프로퍼티 (ResultWriterManager 등에서 참조)
        public string UserId { get; set; }

        public bool m_bBuzzerOff { get; set; } = false;
        public bool bIndexCal { get; set; } = true; //재현성및 1:1모드


        private ResultWriterManager _resultWriterManager;
        public ResultWriterManager ResultWriterManager => _resultWriterManager;
        public T GetUnitConfig<T>(string unitName) where T : class => ConfigManager.GetUnitConfig<T>(unitName);
        public void SetUnitConfig(string unitName, object config)
        {
            if (config is BaseConfig baseConfig) ConfigManager.SetUnitConfig(unitName, baseConfig);
        }
        //public void SetUnitRecipe(string unitName, BaseRecipe recipe) => RecipeManager.SetUnitRecipe(unitName, recipe);
        public bool SaveAllConfigs(string directoryPath = null) => ConfigManager.SaveAllConfigs(directoryPath);
        public bool LoadAllConfigs(string directoryPath = null) => ConfigManager.LoadAllConfigs(directoryPath);

        
        #endregion

        #region 07. Guards (EnsureInitialized / AxisReady)
        public void EnsureInitializedOrThrow(string actionName)
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
        #endregion

        #region 08. Devices

        #region Camera
        public Dictionary<string, HIKGigECamera> Cameras { get; } = new Dictionary<string, HIKGigECamera>(StringComparer.OrdinalIgnoreCase);
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
        #endregion

        #region Light
        public Dictionary<string, LeesOsLightController> LightControllers { get; } = new Dictionary<string, LeesOsLightController>(StringComparer.OrdinalIgnoreCase);
        public LeesOsLightController LeesOsLightController => GetLightController("Light");
        private LeesOsLightController GetLightController(string key)
        {
            return LightControllers.TryGetValue(key, out var light) ? light : null;
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
        #endregion


        #region Sourcemeters / Spectrometers
        public Dictionary<string, KeithleySourcemeter> Sourcemeters { get; } = new Dictionary<string, KeithleySourcemeter>(StringComparer.OrdinalIgnoreCase);
        // == 편의 프로퍼티 추가 ==
        public KeithleySourcemeter Sourcemeter => GetSourcemeter("Index_Prober_Sourcemeter");
        private KeithleySourcemeter GetSourcemeter(string key)
        {
            return Sourcemeters.TryGetValue(key, out var sm) ? sm : null;
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
        // Spectrometer
        public Dictionary<string, CASSpectrometer> Spectrometers { get; } = new Dictionary<string, CASSpectrometer>(StringComparer.OrdinalIgnoreCase);
        // == 편의 프로퍼티 추가 ==
        public CASSpectrometer Spectrometer => GetSpectrometer("Index_Prober_Spectrometer");
        private CASSpectrometer GetSpectrometer(string key)
        {
            return Spectrometers.TryGetValue(key, out var sm) ? sm : null;
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
        #endregion

        #region PKGTester
        public PKGTester Tester { get; private set; }
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
                    //else
                    //{
                    //    // 폴백: 기존 방식(레거시 파일일 수 있음)
                    //    if (Tester.LoadBinningSpecSheet(specPath) != 0)
                    //    {
                    //        var mb = new MessageBoxOk();
                    //        mb.ShowDialog("Error!", $"Failed to load binning spec sheet.");
                    //    }
                    //}
                }
            }
            catch (Exception ex)
            { Log.Write(ex); }
        }
        #endregion

        #region StrainGage
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
        #endregion

        #endregion


        #region 09. Summary (WaferTotalSummary)

        private readonly EquipmentSummaryContext _summaryContext = new EquipmentSummaryContext();
        public EquipmentSummaryContext SummaryContext => _summaryContext;
        private bool IsWaferSummaryActive()
        {
            try
            {
                // SummaryContext는 항상 존재하지만, 공정 시작/종료는 IsActive로 관리
                return _summaryContext != null && _summaryContext.IsActive && _summaryContext.Current != null;
            }
            catch
            {
                return false;
            }
        }
        public void StartRun()
        {
            if (!IsWaferSummaryActive())
                return;

            try
            {
                _summaryContext.Current.StartRun();
            }
            catch (Exception ex)
            {
                Log.Write("Equipment", nameof(StartRun), ex.Message);
            }
        }
        public void StopRun()
        {
            if (!IsWaferSummaryActive())
                return;

            try
            {
                _summaryContext.Current.StopRun();
            }
            catch (Exception ex)
            {
                Log.Write("Equipment", nameof(StopRun), ex.Message);
            }
        }
        public void StartDown()
        {
            if (!IsWaferSummaryActive())
                return;

            try
            {
                // Down 시작 시 Run은 멈추는 정책(겹치지 않게)
                _summaryContext.Current.StopRun();
                _summaryContext.Current.StartDown();
            }
            catch (Exception ex)
            {
                Log.Write("Equipment", nameof(StartDown), ex.Message);
            }
        }
        public void StopDown()
        {
            if (!IsWaferSummaryActive())
                return;

            try
            {
                _summaryContext.Current.StopDown();
                // Down 종료 후 Run 재개 정책(원치 않으면 제거)
                _summaryContext.Current.StartRun();
            }
            catch (Exception ex)
            {
                Log.Write("Equipment", nameof(StopDown), ex.Message);
            }
        }
        // 장비 정지/알람 등으로 "현재 진행 중인 측정"을 모두 종료하고 싶을 때 사용
        public void StopAllSummarySegments()
        {
            if (!IsWaferSummaryActive())
                return;

            try
            {
                _summaryContext.Current.StopAllSegments();
            }
            catch (Exception ex)
            {
                Log.Write("Equipment", "StopAllSummarySegments", ex.Message);
            }
        }

        // Equipment 클래스 내부에 추가 (중복 구독 방지)
        private bool _waferSummaryHooksBound;
        private void BindWaferSummaryHooksOnce()
        {
            if (_waferSummaryHooksBound)
                return;

            _waferSummaryHooksBound = true;

            this.ErrorOccurred += (s, e) =>
            {
                try
                {
                    WaferManager.Instance.CurrentSummary.AddAlarmCount(1);
                }
                catch { }
            };
        }

        #endregion

        #region 10. Initialization (Ctor / InitializeEquipment / Warmup / Bind / Teaching)

        private Equipment()
        {
            // [ADD] 초기화 전에도 UI가 null 참조로 죽지 않도록 빈 컨테이너는 만들어 둠
            Units = new ConcurrentDictionary<string, IUnit>();
            _unitExecutions = new ConcurrentDictionary<string, UnitExecutionInfo>();
            EqState = EquipmentState.Stopped;

            _isEquipmentInitialized = false;

            // 시퀀스 엔진 인스턴스 생성 이동
            _sequenceEngine = new EquipmentSequence();
        }
        public void InitializeEquipment()
        {
            try
            {
                string alarmFilePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configs", "Alarms.csv");
                GlobalAlarmTable.Instance.LoadAlarmsFromFile(alarmFilePath);

                InitializePreconditions();

                InitializeCoreContainers();

                OnStateChanged(EquipmentState.Initializing);

                InitializeMotionIo();

                InitializeDevices();

                InitializeUnits();
                InitializeRecipes();

                // 여기서 VisionRunnerHub 1회 초기화하자.
                try
                {
                    var currentRecipe = EquipmentRecipe?.CurrentRecipe.Name;
                    var recipeBase = PatternMatchingRecipeStore.NormalizeRecipeBaseName(currentRecipe);
                    var recipeRoot = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Recipes", recipeBase, "PatternMatching");
                    VisionRunnerHub.InitializeOnce(cam =>
                    {
                        return new PatternMatchingRunner.RunnerOptions
                        {
                            AutoLoadRecipe = false,
                            RecipeRootDirectory = recipeRoot,
                            RecipeName = currentRecipe,//"Default", // fallback
                            UseInspectRoi = true,
                            Mode = PatternMatchingRunner.SearchMode.All,
                            DrawCrossOnViewer = false,
                            CrossColor = Color.Lime,
                            CrossHalfLength = 15,
                            EnableSaveImage = false
                        };
                    },
                    defaultViewerOptions: new PatternMatchingRunner.ViewerDisplayOptions
                    {
                        DrawCrossOnViewer = false,
                        HighlightReferenceMatch = true,
                        ShowMatchIndexes = false
                    });
                }
                catch (Exception ex)
                {
                    // Hub 초기화 실패가 설비 초기화를 막지 않도록 로그만 남김
                    Log.Write("Equipment", "VisionRunnerHub.InitializeOnce error: " + ex.Message);
                }


                StartEquipmentStatusUnit();

                // ===== GEM 초기화/시작 =====
                // 64bIT 지원 안함.!

                FinalizeInitialization();

                HookAlarmCountToSummaryOnce();

                // 30일이 지난 로그 삭제
                Log.DeleteOldLogs(5);  //Test 5일 지난 파일

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

        private void InitializePreconditions()
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
        }

        private void InitializeCoreContainers()
        {
            Units = new ConcurrentDictionary<string, IUnit>();
            _unitExecutions = new ConcurrentDictionary<string, UnitExecutionInfo>();
            EqState = EquipmentState.Stopped;

            ConfigManager = new EquipmentConfigManager();
            EquipmentConfig = new EquipmentConfig();
            EquipmentRecipe = new EquipmentRecipe();

            _resultWriterManager = new ResultWriterManager();
        }
        private void InitializeMotionIo()
        {
            // 여기서 모든 유닛 축을 직접 생성/로드하여 붙인다.
            BootstrapAxesDirect();
            BootstrapIODirect();
        }

        private void InitializeDevices()
        {
            // === 카메라 초기화 ===
            InitializeCameras();

            // === 조명 초기화 ===
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
        }

        private void InitializeUnits()
        {
            // 기본 Unit들 자동 등록 (개발자가 필요에 따라 추가)
            AutoRegisterUnits();
            BindUnit();
        }

        private void InitializeRecipes()
        {
            // 3) 설비 Config + 메인 Recipe 로드
            EquipmentRecipe.InitGlobalRecipe();

            // ★ [ADD] 프로그램 시작 시점: 현재 레시피 기준 TeachingRecipe 전부 로드 + 축 바인딩
            try
            {
                PreloadAllTeachingUnitRecipes();
            }
            catch (Exception ex)
            {
                Log.Write("Equipment", nameof(PreloadAllTeachingUnitRecipes), ex.Message);
            }
        }

        private void StartEquipmentStatusUnit()
        {
            // [ADD] EquipmentStatus 즉시 시작 (장비 Ready 이전에 상태 수집 시작)
            try
            {
                if (Units.ContainsKey(UnitKeys.EquipmentStatus))
                {
                    //2026-01-13 
                    //EquipmentStatus만 예외로 StartUnitAsync 사용. -> Unit이 아니라.. 걍 Equipment로 빼는게 낮겠다.
                    var ok = StartUnitAsync(UnitKeys.EquipmentStatus).GetAwaiter().GetResult();
                    //var ok = SequenceStartAsync(UnitKeys.EquipmentStatus, CancellationToken.None).GetAwaiter().GetResult();
                    if (!ok)
                    {
                        OnErrorOccurred("EquipmentStatus Unit 시작 실패");
                    }
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
        }

        private void FinalizeInitialization()
        {
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
                //if (Tester.LoadBinningSpecSheet(currentRecipe.BinningSpecSheetFile) != 0)
                //{
                //    mb.ShowDialog("Error!", $"Failed to load binning spec sheet.");
                //}
            }

            _motionSafetyStopIssued = false;
            _isEquipmentInitialized = true; // 모든 초기화가 성공적으로 끝났음을 표시
            OnStateChanged(EquipmentState.Ready);

            BindWaferSummaryHooksOnce();
        }



        private void BindUnit()
        {
            foreach (var v in Units)
            {
                (v.Value as BaseUnit)?.BindUnit();
            }
        }

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
        private void PreloadAllTeachingUnitRecipes()
        {
            try
            {
                // CurrentRecipe 확정 (InitGlobalRecipe에서 잡히지만, 로그/안전용)
                var mr = EquipmentRecipe?.CurrentRecipe;
                Log.Write("Equipment", nameof(PreloadAllTeachingUnitRecipes), $"CurrentRecipe='{mr?.Name}'");

                if (AxisManager == null || EquipmentRecipe == null)
                    return;

                // TeachingRecipe를 사용하는 유닛 키들(추가 발생 시 여기에만 추가)
                string[] unitKeys =
                {
                    EquipmentRecipe.UnitKey_IndexChipProbeControllerTeaching,
                    EquipmentRecipe.UnitKey_IndexLoadAlignerTeaching,
                    EquipmentRecipe.UnitKey_InputDieTransferTeaching,
                    EquipmentRecipe.UnitKey_OutputDieTransferTeaching,
                };

                foreach (var unitKey in unitKeys)
                {
                    try
                    {
                        // (1) 기대 이름 정규화 + 캐시 로드 유도
                        EquipmentRecipe.GetOrLoadUnitTeachingRecipeName(unitKey);

                        // (2) Strongly-typed recipe 인스턴스 얻기
                        QMC.Common.BaseRecipe recipe = null;
                        if (string.Equals(unitKey, EquipmentRecipe.UnitKey_IndexChipProbeControllerTeaching, StringComparison.OrdinalIgnoreCase))
                            recipe = EquipmentRecipe.IndexChipProbeControllerTeachingRecipe;
                        else if (string.Equals(unitKey, EquipmentRecipe.UnitKey_IndexLoadAlignerTeaching, StringComparison.OrdinalIgnoreCase))
                            recipe = EquipmentRecipe.IndexLoadAlignerTeachingRecipe;
                        else if (string.Equals(unitKey, EquipmentRecipe.UnitKey_InputDieTransferTeaching, StringComparison.OrdinalIgnoreCase))
                            recipe = EquipmentRecipe.InputDieTransferTeachingRecipe;
                        else if (string.Equals(unitKey, EquipmentRecipe.UnitKey_OutputDieTransferTeaching, StringComparison.OrdinalIgnoreCase))
                            recipe = EquipmentRecipe.OutputDieTransferTeachingRecipe;

                        if (recipe == null)
                            continue;

                        // (3) LoadAndBindAxes(axisManager) 호출 (있으면)
                        var miLoadBind = recipe.GetType().GetMethod("LoadAndBindAxes",
                            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                        miLoadBind?.Invoke(recipe, new object[] { AxisManager });

                        // (4) Strongly-typed 초기화 호출 (없으면 무시)
                        var miInit = recipe.GetType().GetMethod("Init",
                            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                        miInit?.Invoke(recipe, null);

                        // (5) TP 개수 로그(확인용)
                        try
                        {
                            var tpProp = recipe.GetType().GetProperty("TeachingPositions",
                                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                            var tps = tpProp?.GetValue(recipe, null) as System.Collections.ICollection;
                            Log.Write("Equipment", nameof(PreloadAllTeachingUnitRecipes),
                                $"Loaded unitKey='{unitKey}', recipe='{recipe.Name}', TP.Count={(tps != null ? tps.Count : 0)}");
                        }
                        catch { /* ignore */ }
                    }
                    catch (Exception ex)
                    {
                        Log.Write("Equipment", nameof(PreloadAllTeachingUnitRecipes),
                            $"Teaching preload fail unitKey='{unitKey}': {ex.Message}");
                    }
                }

                // (6) Unit Config cache 무효화(있으면) - UI/Manual이 구레퍼런스 잡는 것 방지
                try
                {
                    foreach (var u in Units?.Values ?? new ConcurrentDictionary<string, IUnit>().Values)
                    {
                        var cfg = u?.Config;
                        if (cfg == null) continue;

                        var miInv = cfg.GetType().GetMethod("InvalidateTeachingRecipeCache",
                            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                        miInv?.Invoke(cfg, null);
                    }
                }
                catch { /* ignore */ }
            }
            catch (Exception ex)
            {
                Log.Write("Equipment", nameof(PreloadAllTeachingUnitRecipes), ex.Message);
            }
        }

        #endregion

        #region 11. Motion / IO Bootstrap

        private AjinAxlBoardHost _axlHost;
        private readonly MotionAxisManager _axisManager = new MotionAxisManager();
        private readonly string _axisRoot = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configs", "Axes");
        public MotionAxisManager AxisManager => _axisManager;

        private DIOUnit _unitIO;
        private IDIODriver _dio;
        private DioScanService _dioScan;
        private readonly string _dioRoot = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DIO");
        public DioScanService DioScan => _dioScan;
        public DIOUnit UnitIO => _unitIO;

        private CKDMotorDriver _ckdDriver;
        private MotionStatusScanner _motionStatusScanner;

        private readonly object _motionSafetyStopLock = new object();
        private volatile bool _motionSafetyStopIssued;

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
                else
                {
                    Log.Write("LCP-280", "BootstrapAxesDirect", "Faild: LCP-280.mot");
                    var mb = new MessageBoxOk();
                    mb.ShowDialog("Error", "Faild: LCP-280.mot");
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
                //_motionStatusScanner = new MotionStatusScanner(_axisManager, periodMs: 20);
                _motionStatusScanner = new MotionStatusScanner(_axisManager, 5);

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
                //파일없으면 프로그램 다운됨.
                _dioScan = new DioScanService(_unitIO, _dio);
                //_dioScan.Start(10);
                _dioScan.Start(5);
            }

            

            IoBindings.RegisterAll();

            bool bIsOpen = false;

            try
            {
                bIsOpen = _axlHost.IsOpen;
            }
            catch (Exception ex)
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

        #endregion

        #region 12. Events (State / Error)

        public event EventHandler<EquipmentStateChangedEventArgs> StateChanged;
        public event EventHandler<EquipmentErrorEventArgs> ErrorOccurred;
        // [ADD] EqState를 세팅하는 유일한 곳(단일 지점)
        internal void ApplyEquipmentState(EquipmentState newState, string reason = null)
        {
            var old = EqState;
            if (old == newState)
                return;

            EqState = newState;

            try
            {
                StateChanged?.Invoke(this, new EquipmentStateChangedEventArgs(old, newState));
            }
            catch { }

            // Summary hook도 여기서만 동작 (상태 변경 단일화)
            try
            {
                if (!IsWaferSummaryActive())
                    return;

                switch (newState)
                {
                    case EquipmentState.AutoRunning:
                    case EquipmentState.Starting:
                    case EquipmentState.ManualRunning:
                        StopDown();
                        StartRun();
                        break;

                    case EquipmentState.Stopping:
                    case EquipmentState.Stopped:
                    case EquipmentState.CycleStop:
                    case EquipmentState.Error:
                    case EquipmentState.Reset:
                        StopRun();
                        StartDown();
                        break;
                }
            }
            catch { }

            // 필요 시 로깅(원하면)
            // Log.Write("Equipment", $"[STATE] {old} -> {newState} ({reason})");
        }

        private void OnStateChanged(EquipmentState newState)
        {
            ApplyEquipmentState(newState, reason: "OnStateChanged");
        }
        private void OnErrorOccurred(string msg)
        {
            try 
            { 
                ErrorOccurred?.Invoke(this, new EquipmentErrorEventArgs(msg)); 
            } 
            catch (Exception ex)
            {
                Log.Write(ex);
            }
            Log.Write("LPC-280", "OnErrorOccurred", msg);
        }

        internal void RaiseErrorFromSequence(string message)
        {
            OnErrorOccurred(message);
        }

        #endregion

        #region 13. Dispose
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
                    //StopAllUnitsAsync(includeEquipmentStatus: false).GetAwaiter().GetResult();
                    SequenceStopAllAsync(CancellationToken.None).GetAwaiter().GetResult();
                }
                catch (Exception ex)
                {
                    Log.Write(ex);
                }

                // 2) 보호 유닛 강제 정지
                ForceStopEquipmentStatus();

                // ===== GEM 종료 =====
                try { _gemBridge?.Dispose(); } catch { }
                _gemBridge = null;

                try
                {
                    if (GemService != null)
                    {
                        try { GemService.Stop(); } catch { }
                        try { GemService.Dispose(); } catch { }
                    }
                }
                catch { }
                GemService = null;
                GemConfig = null;

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



        #region GEM (optional group)
        // ===== GEM (XLinkGEM) =====
        public XLinkGemService GemService { get; set; }
        public XLinkGemServiceConfig GemConfig { get; set; }
        private EquipmentGemBridge _gemBridge;
        #endregion
    }
}