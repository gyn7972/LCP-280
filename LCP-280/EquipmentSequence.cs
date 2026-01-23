using QMC.Common;
using QMC.Common.Unit;
using QMC.LCP_280.Process.Unit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using static QMC.Common.Unit.BaseUnit;

namespace QMC.LCP_280.Process
{
    public sealed class EquipmentSequence
    {
        public enum SequenceAction { Ready, Start, Stop, Reset, CycleStop }
        private enum SequenceRuntimeState { Idle, Ready, Running, CycleStop }

        private static readonly string[] _sequenceOrder = { "Wafer", "LoadArm", "Index", "UnloadArm", "Bin" };

        private sealed class SequenceStateInfo
        {
            public string Name { get; private set; }
            public SequenceRuntimeState State { get; set; }
            public SequenceStateInfo(string name) { Name = name; State = SequenceRuntimeState.Idle; }
        }

        private Equipment Equipment { get { return Equipment.Instance; } }

        private readonly object _sequenceSync = new object();
        private readonly Dictionary<string, SequenceStateInfo> _sequenceStates =
            new Dictionary<string, SequenceStateInfo>(StringComparer.OrdinalIgnoreCase);

        private readonly object _sequenceOpGateLock = new object();
        private readonly Dictionary<string, SemaphoreSlim> _sequenceOpGates =
            new Dictionary<string, SemaphoreSlim>(StringComparer.OrdinalIgnoreCase);

        public event EventHandler SequenceUiStateChanged;

        private SemaphoreSlim GetSequenceOpGate(string key)
        {
            if (string.IsNullOrWhiteSpace(key)) key = "All";
            lock (_sequenceOpGateLock)
            {
                SemaphoreSlim g;
                if (!_sequenceOpGates.TryGetValue(key, out g))
                {
                    g = new SemaphoreSlim(1, 1);
                    _sequenceOpGates[key] = g;
                }
                return g;
            }
        }

        private SequenceStateInfo GetOrCreateSeqState(string sequenceName)
        {
            if (string.IsNullOrWhiteSpace(sequenceName))
                throw new ArgumentException("sequenceName");

            lock (_sequenceSync)
            {
                SequenceStateInfo st;
                if (!_sequenceStates.TryGetValue(sequenceName, out st))
                {
                    st = new SequenceStateInfo(sequenceName);
                    _sequenceStates[sequenceName] = st;
                }
                return st;
            }
        }

        // ===== Sequence Unit Map (기존 Equipment.GetUnitsForSequence 대체) =====
        private IEnumerable<BaseUnit> GetUnitsForSequence(string sequenceName)
        {
            BaseUnit U(string key) { return Equipment.GetUnit(key); }

            switch (sequenceName)
            {
                case "Wafer":
                    return new[] { U(Equipment.UnitKeys.InputFeeder), U(Equipment.UnitKeys.InputCassetteLifter), U(Equipment.UnitKeys.InputStage), U(Equipment.UnitKeys.InputStageEjector) }
                        .Where(u => u != null);

                case "LoadArm":
                    return new[] { U(Equipment.UnitKeys.InputDieTransfer) }
                        .Where(u => u != null);

                case "Index":
                    return new[]
                    {
                        U(Equipment.UnitKeys.Rotary),
                        U(Equipment.UnitKeys.IndexLoadAligner),
                        U(Equipment.UnitKeys.IndexChipProbeController),
                        U(Equipment.UnitKeys.IndexChipProber),
                        U(Equipment.UnitKeys.IndexUnloadAligner)
                    }.Where(u => u != null);

                case "UnloadArm":
                    return new[] { U(Equipment.UnitKeys.OutputDieTransfer) }
                        .Where(u => u != null);

                case "Bin":
                    return new[] { U(Equipment.UnitKeys.OutputFeeder), U(Equipment.UnitKeys.OutputCassetteLifter), U(Equipment.UnitKeys.OutputStage) }
                        .Where(u => u != null);
            }

            return Enumerable.Empty<BaseUnit>();
        }

        private IEnumerable<string> GetUnitNamesForSequenceStart(string sequenceName)
        {
            return GetUnitsForSequence(sequenceName)
                .Where(u => u != null && !string.IsNullOrWhiteSpace(u.UnitName))
                .Select(u => u.UnitName)
                .Distinct(StringComparer.OrdinalIgnoreCase);
        }

        private IEnumerable<string> GetUnitNamesForSequenceReadyTasks(string sequenceName)
        {
            switch (sequenceName)
            {
                case "Wafer": return new[] { Equipment.UnitKeys.InputFeeder, Equipment.UnitKeys.InputStageEjector };
                case "LoadArm": return new[] { Equipment.UnitKeys.InputDieTransfer };
                case "Index": return new[] { Equipment.UnitKeys.IndexLoadAligner, Equipment.UnitKeys.IndexChipProbeController };
                case "UnloadArm": return new[] { Equipment.UnitKeys.OutputDieTransfer };
                case "Bin": return new[] { Equipment.UnitKeys.OutputFeeder };
            }
            return Enumerable.Empty<string>();
        }

        private IEnumerable<string> GetUnitNamesForSequenceReady(string sequenceName)
        {
            return GetUnitNamesForSequenceReadyTasks(sequenceName)
                .Concat(GetUnitNamesForSequenceStart(sequenceName))
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .Distinct(StringComparer.OrdinalIgnoreCase);
        }

        private IEnumerable<Func<CancellationToken, Task<int>>> BuildReadyTasks(string sequenceName)
        {
            var inputFeeder = Equipment.GetUnit(Equipment.UnitKeys.InputFeeder) as InputFeeder;
            var inputDieTransfer = Equipment.GetUnit(Equipment.UnitKeys.InputDieTransfer) as InputDieTransfer;
            var outputDieTransfer = Equipment.GetUnit(Equipment.UnitKeys.OutputDieTransfer) as OutputDieTransfer;
            var outputFeeder = Equipment.GetUnit(Equipment.UnitKeys.OutputFeeder) as OutputFeeder;
            var inputStageEjector = Equipment.GetUnit(Equipment.UnitKeys.InputStageEjector) as InputStageEjector;
            var indexLoadAligner = Equipment.GetUnit(Equipment.UnitKeys.IndexLoadAligner) as IndexLoadAligner;
            var indexChipProbeController = Equipment.GetUnit(Equipment.UnitKeys.IndexChipProbeController) as IndexChipProbeController;

            switch (sequenceName)
            {
                case "Wafer":
                    return new Func<CancellationToken, Task<int>>[]
                    {
                        ct => Task.Run(() => { ct.ThrowIfCancellationRequested(); return inputFeeder != null ? inputFeeder.EnsureReady() : -1; }, ct),
                        ct => Task.Run(() => { ct.ThrowIfCancellationRequested(); return inputStageEjector != null ? inputStageEjector.CheckReady() : -1; }, ct),
                    };

                case "LoadArm":
                    return new Func<CancellationToken, Task<int>>[]
                    {
                        ct => Task.Run(() => { ct.ThrowIfCancellationRequested(); return inputDieTransfer != null ? inputDieTransfer.EnsureReady() : -1; }, ct),
                    };

                case "Index":
                    return new Func<CancellationToken, Task<int>>[]
                    {
                        ct => Task.Run(() => { ct.ThrowIfCancellationRequested(); return indexLoadAligner != null ? indexLoadAligner.EnsureReady() : -1; }, ct),
                        ct => Task.Run(() => { ct.ThrowIfCancellationRequested(); return indexChipProbeController != null ? indexChipProbeController.EnsureReady() : -1; }, ct),
                    };

                case "UnloadArm":
                    return new Func<CancellationToken, Task<int>>[]
                    {
                        ct => Task.Run(() => { ct.ThrowIfCancellationRequested(); return outputDieTransfer != null ? outputDieTransfer.EnsureReady() : -1; }, ct),
                    };

                case "Bin":
                    return new Func<CancellationToken, Task<int>>[]
                    {
                        ct => Task.Run(() => { ct.ThrowIfCancellationRequested(); return outputFeeder != null ? outputFeeder.EnsureReady() : -1; }, ct),
                    };
            }

            return Enumerable.Empty<Func<CancellationToken, Task<int>>>();
        }

        private async Task<bool> ReadyUnitsForSequenceAsync(string sequenceName, CancellationToken ct, bool parallel)
        {
            var tasksFactory = BuildReadyTasks(sequenceName).ToList();
            if (tasksFactory.Count == 0)
            {
                Log.Write("Equipment", "[SEQ] ReadyTasks 없음: seq='" + sequenceName + "'");
                return false;
            }

            if (parallel)
            {
                var tasks = tasksFactory.Select(f => f(ct)).ToArray();
                var rcs = await Task.WhenAll(tasks).ConfigureAwait(false);
                return rcs.All(rc => rc == 0);
            }

            foreach (var f in tasksFactory)
            {
                ct.ThrowIfCancellationRequested();
                var rc = await f(ct).ConfigureAwait(false);
                if (rc != 0) return false;
            }

            return true;
        }

        private async Task<bool> TryStartUnitAsync(BaseUnit unit)
        {
            if (unit == null) return false;
            if (string.IsNullOrEmpty(unit.UnitName)) return false;

            if (unit.RunUnitStatus == BaseUnit.UnitStatus.AutoRunning || unit.IsRunning)
                return true;

            // Equipment에 StartUnitAsync가 없다면 프로젝트 구조가 깨진 것 (반드시 구현되어 있어야 함)
            return await Equipment.StartUnitAsync(unit.UnitName).ConfigureAwait(false);
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

        private async Task<bool> StartUnitsForSequenceAsync(string sequenceName, CancellationToken ct, bool parallel)
        {
            var units = GetUnitsForSequence(sequenceName).ToList();
            if (units.Count == 0) 
                return false;

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

        private async Task StopUnitsForSequenceAsync(string sequenceName)
        {
            var units = GetUnitsForSequence(sequenceName).ToList();
            foreach (var u in units)
            {
                try
                {
                    if (u != null && !string.IsNullOrEmpty(u.UnitName))
                        await Equipment.StopUnitAsync(u.UnitName).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Log.Write(ex);
                }
            }
        }

        private async Task<bool> ExecuteSequenceAsync(string sequenceName, SequenceAction action, CancellationToken ct)
        {
            Equipment.EnsureInitializedOrThrow("Sequence:" + action + ":" + sequenceName);

            if (action == SequenceAction.Ready || action == SequenceAction.Start)
                Equipment.EnsureAxisReadyForAutoOrMove("Sequence:" + action + ":" + sequenceName);

            ApplyBeginEquipmentState(sequenceName, action);

            var gateKey = string.IsNullOrWhiteSpace(sequenceName) ? "All" : sequenceName;
            var gate = GetSequenceOpGate(gateKey);

            await gate.WaitAsync(ct).ConfigureAwait(false);
            try
            {
                bool ok;
                if (string.Equals(gateKey, "All", StringComparison.OrdinalIgnoreCase))
                    ok = await ExecuteAllCoreAsync(action, ct).ConfigureAwait(false);
                else
                    ok = await ExecuteOneSequenceCoreAsync(sequenceName, action, ct).ConfigureAwait(false);

                if (!ok)
                {
                    ApplyFailureEquipmentState(sequenceName, action, "Execute returned false");
                    return false;
                }

                ApplySuccessEquipmentState(sequenceName, action);
                return true;
            }
            catch (Exception ex)
            {
                ApplyFailureEquipmentState(sequenceName, action, ex.Message);
                throw;
            }
            finally
            {
                try { gate.Release(); } catch { }
            }

            //Equipment.EnsureInitializedOrThrow("Sequence:" + action + ":" + sequenceName);
            //if (action == SequenceAction.Ready || action == SequenceAction.Start)
            //    Equipment.EnsureAxisReadyForAutoOrMove("Sequence:" + action + ":" + sequenceName);
            //var gateKey = string.IsNullOrWhiteSpace(sequenceName) ? "All" : sequenceName;
            //var gate = GetSequenceOpGate(gateKey);
            //await gate.WaitAsync(ct).ConfigureAwait(false);
            //try
            //{
            //    if (string.Equals(gateKey, "All", StringComparison.OrdinalIgnoreCase))
            //    {
            //        return await ExecuteAllCoreAsync(action, ct).ConfigureAwait(false);
            //    }
            //    return await ExecuteOneSequenceCoreAsync(sequenceName, action, ct).ConfigureAwait(false);
            //}
            //finally
            //{
            //    try { gate.Release(); } catch { }
            //}
        }

        private async Task<bool> ExecuteAllCoreAsync(SequenceAction action, CancellationToken ct)
        {
            for (int i = 0; i < _sequenceOrder.Length; i++)
            {
                ct.ThrowIfCancellationRequested();
                var seq = _sequenceOrder[i];

                var ok = await ExecuteOneSequenceCoreAsync(seq, action, ct).ConfigureAwait(false);
                if (!ok && action != SequenceAction.Stop)
                    return false;
            }

            return true;
        }

        private async Task<bool> ExecuteOneSequenceCoreAsync(string sequenceName, SequenceAction action, CancellationToken ct)
        {
            var st = GetOrCreateSeqState(sequenceName);

            SequenceRuntimeState snapshot;
            lock (_sequenceSync) snapshot = st.State;

            if (action == SequenceAction.Ready)
            {
                if (snapshot == SequenceRuntimeState.Ready) return true;
                if (snapshot == SequenceRuntimeState.Running) return false;
            }
            else if (action == SequenceAction.Start)
            {
                if (snapshot == SequenceRuntimeState.Running) return true;
            }
            else if (action == SequenceAction.Stop)
            {
                if (snapshot == SequenceRuntimeState.Idle) { RaiseUiChanged(); return true; }
            }

            IEnumerable<string> lockUnits = (action == SequenceAction.Ready)
                ? GetUnitNamesForSequenceReady(sequenceName)
                : GetUnitNamesForSequenceStart(sequenceName);

            return await Equipment.WithUnitGatesAsync(lockUnits, sequenceName + ":" + action, ct, async () =>
            {
                switch (action)
                {
                    case SequenceAction.Ready:
                        {
                            var ok = await ReadyUnitsForSequenceAsync(sequenceName, ct, true).ConfigureAwait(false);
                            if (!ok) return false;
                            SetSequenceState(sequenceName, SequenceRuntimeState.Ready);
                            return true;
                        }

                    case SequenceAction.Start:
                        {
                            var okReady = await ReadyUnitsForSequenceAsync(sequenceName, ct, true).ConfigureAwait(false);
                            if (!okReady) return false;

                            var okStart = await StartUnitsForSequenceAsync(sequenceName, ct, true).ConfigureAwait(false);
                            if (!okStart)
                            {
                                SetSequenceState(sequenceName, SequenceRuntimeState.Ready);
                                return false;
                            }

                            SetSequenceState(sequenceName, SequenceRuntimeState.Running);
                            return true;
                        }

                    case SequenceAction.Stop:
                        {
                            await StopUnitsForSequenceAsync(sequenceName).ConfigureAwait(false);
                            SetSequenceState(sequenceName, SequenceRuntimeState.Idle);
                            return true;
                        }

                    case SequenceAction.Reset:
                        {
                            await StopUnitsForSequenceAsync(sequenceName).ConfigureAwait(false);

                            var units = GetUnitsForSequence(sequenceName).ToList();
                            foreach (var u in units)
                            {
                                ct.ThrowIfCancellationRequested();

                                var mi = u.GetType().GetMethod("ResetForNewRun",
                                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                                if (mi != null)
                                {
                                    var ps = mi.GetParameters();
                                    if (ps.Length == 0) mi.Invoke(u, null);
                                    else if (ps.Length == 1 && ps[0].ParameterType == typeof(bool)) mi.Invoke(u, new object[] { false });
                                    else if (ps.Length == 3
                                             && ps[0].ParameterType == typeof(bool)
                                             && ps[1].ParameterType == typeof(bool)
                                             && ps[2].ParameterType == typeof(bool))
                                        mi.Invoke(u, new object[] { false, true, true });
                                }

                                try { u.SetMaterial(null); } catch { }
                            }

                            SetSequenceState(sequenceName, SequenceRuntimeState.Idle);
                            return true;
                        }

                    case SequenceAction.CycleStop:
                        {
                            await StopUnitsForSequenceAsync(sequenceName).ConfigureAwait(false);
                            SetSequenceState(sequenceName, SequenceRuntimeState.CycleStop);
                            return true;
                        }
                }

                return false;
            }).ConfigureAwait(false);
        }

        private void SetSequenceState(string sequenceName, SequenceRuntimeState state)
        {
            lock (_sequenceSync)
            {
                var st = GetOrCreateSeqState(sequenceName);
                st.State = state;
            }
            RaiseUiChanged();
        }

        private void RaiseUiChanged()
        {
            try { SequenceUiStateChanged?.Invoke(this, EventArgs.Empty); } catch { }
        }

        private static bool IsAll(string sequenceName)
            => string.Equals(sequenceName, "All", StringComparison.OrdinalIgnoreCase);

        private void ApplyBeginEquipmentState(string sequenceName, SequenceAction action)
        {
            switch (action)
            {
                case SequenceAction.Start:
                    Equipment.ApplyEquipmentState(EquipmentState.Starting, reason: $"SEQ:{sequenceName}:Start(begin)");
                    break;
                case SequenceAction.Stop:
                    Equipment.ApplyEquipmentState(EquipmentState.Stopping, reason: $"SEQ:{sequenceName}:Stop(begin)");
                    break;
                case SequenceAction.Reset:
                    Equipment.ApplyEquipmentState(EquipmentState.Reset, reason: $"SEQ:{sequenceName}:Reset(begin)");
                    break;
                case SequenceAction.CycleStop:
                    Equipment.ApplyEquipmentState(EquipmentState.CycleStop, reason: $"SEQ:{sequenceName}:CycleStop(begin)");
                    break;
                case SequenceAction.Ready:
                default:
                    // Ready는 성공 시 Ready로 올림
                    break;
            }
        }

        private void ApplySuccessEquipmentState(string sequenceName, SequenceAction action)
        {
            switch (action)
            {
                case SequenceAction.Ready:
                    Equipment.ApplyEquipmentState(EquipmentState.Ready, reason: $"SEQ:{sequenceName}:Ready(ok)");
                    break;

                case SequenceAction.Start:
                    // [POLICY] All이면 AutoRunning, 부분 Start면 ManualRunning
                    Equipment.ApplyEquipmentState(
                        IsAll(sequenceName) ? EquipmentState.AutoRunning : EquipmentState.ManualRunning,
                        reason: $"SEQ:{sequenceName}:Start(ok)");
                    break;

                case SequenceAction.Stop:
                    Equipment.ApplyEquipmentState(EquipmentState.Stopped, reason: $"SEQ:{sequenceName}:Stop(ok)");
                    break;

                case SequenceAction.CycleStop:
                    Equipment.ApplyEquipmentState(EquipmentState.CycleStop, reason: $"SEQ:{sequenceName}:CycleStop(ok)");
                    break;

                case SequenceAction.Reset:
                    // [POLICY] Reset 완료 후 Stopped로 복귀
                    Equipment.ApplyEquipmentState(EquipmentState.Stopped, reason: $"SEQ:{sequenceName}:Reset(ok->Stopped)");
                    break;
            }
        }

        private void ApplyFailureEquipmentState(string sequenceName, SequenceAction action, string reason)
        {
            Equipment.ApplyEquipmentState(EquipmentState.Error, reason: $"SEQ:{sequenceName}:{action}(fail)");
            try
            {
                Equipment.RaiseErrorFromSequence($"[SEQ] {action} 실패: {sequenceName}, {reason}");
            }
            catch { }
        }

        // ===== Public API =====
        public Task<bool> SequenceReadyAsync(string sequenceName, CancellationToken ct)
            => ExecuteSequenceAsync(sequenceName, SequenceAction.Ready, ct);

        public Task<bool> SequenceStartAsync(string sequenceName, CancellationToken ct)
            => ExecuteSequenceAsync(sequenceName, SequenceAction.Start, ct);

        public Task SequenceStopAsync(string sequenceName, CancellationToken ct)
        {
            return ExecuteSequenceAsync(sequenceName, SequenceAction.Stop, ct)
                .ContinueWith(t => { }, TaskScheduler.Default);
        }

        public Task<bool> SequenceResetAsync(string sequenceName, CancellationToken ct)
            => ExecuteSequenceAsync(sequenceName, SequenceAction.Reset, ct);

        public Task<bool> SequenceCycleStopAsync(string sequenceName, CancellationToken ct)
            => ExecuteSequenceAsync(sequenceName, SequenceAction.CycleStop, ct);

        public Task<bool> SequenceReadyAllAsync(CancellationToken ct)
            => ExecuteSequenceAsync("All", SequenceAction.Ready, ct);

        public Task<bool> SequenceStartAllAsync(CancellationToken ct)
            => ExecuteSequenceAsync("All", SequenceAction.Start, ct);

        public Task<bool> SequenceStopAllAsync(CancellationToken ct)
            => ExecuteSequenceAsync("All", SequenceAction.Stop, ct);

        public Task<bool> SequenceResetAllAsync(CancellationToken ct)
            => ExecuteSequenceAsync("All", SequenceAction.Reset, ct);

        public Task<bool> SequenceCycleStopAllAsync(CancellationToken ct)
            => ExecuteSequenceAsync("All", SequenceAction.CycleStop, ct);

        public Equipment.SequenceUiSnapshot GetSequenceUiSnapshot()
        {
            lock (_sequenceSync)
            {
                var ready = new List<string>();
                var running = new List<string>();

                foreach (var kv in _sequenceStates)
                {
                    if (kv.Value.State == SequenceRuntimeState.Ready) ready.Add(kv.Key);
                    else if (kv.Value.State == SequenceRuntimeState.Running) running.Add(kv.Key);
                }

                return new Equipment.SequenceUiSnapshot
                {
                    Ready = ready.AsReadOnly(),
                    Running = running.AsReadOnly()
                };
            }
        }
    }
}
