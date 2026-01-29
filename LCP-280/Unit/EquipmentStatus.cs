using QMC.Common;
using QMC.Common.Alarm;
using QMC.Common.DIO;
using QMC.Common.IO;
using QMC.Common.Unit;
using System;
using System.Collections.Generic;
using System.Linq; // 여러 물리 DIO 모듈 순회용
using System.Threading; // [ADD] CancellationToken.None

namespace QMC.LCP_280.Process.Unit
{
    /// <summary>
    /// 설비 공통 상태(I/O) 스냅샷 관리 유닛.
    /// - OP 패널 스위치, 도어 센서, 진공 확인, 이오나이저 알람, 타워램프 출력 등 기본 신호를 캡슐화.
    /// - DioScanService 캐시에서 읽어 일관된 값 사용.
    /// - Refresh() 호출 시 내부 스냅샷 교체(기본 100ms).
    /// - 논리 키 기반 I/O 비즈니스 API 제공.
    /// - 물리 버튼 연동(Start/Stop/Reset) + 경광등 패턴(블링크 포함) 관리.
    /// </summary>
    public class EquipmentStatus : BaseUnit<EquipmentConfig>
    {
        public enum AlarmKeys
        {
            eAlarmEMO = 20001,
            eAlarm_WaferFeederOverload,
            eAlarm_BinFeederOverload,
            eAlarm_MAinCDA,
            eAlarm_MainEjector,
        }

        private readonly object _gate = new object();                // 스냅샷 보호
        private EquipmentStatusSnapshot _latest = new EquipmentStatusSnapshot();

        private const int RefreshIntervalMs = 100;
        private int _lastRefreshTick = Environment.TickCount;

        private bool _prevEmg = false;

        // 클래스 필드(Add): 시작 억제/노이즈 억제/초기 사이클 플래그
        private const int StartupSuppressMs = 3000;   // 시작 후 알람 억제 시간(ms)
        private const int EmgHoldMs = 150;            // EMG 유지 시간(노이즈 필터)
        private int _startupSuppressUntilTick = Environment.TickCount + StartupSuppressMs;
        private int _emgAssertTick = 0;
        private bool _firstCycle = true;

        // ----- 동적 I/O 매핑 -----
        private readonly object _ioMapLock = new object();
        // DisplayNo → 논리키 맵(배열, 표시 순서 유지)
        private (string disp, string key)[] _inputMap = new[]
        {
            ("X000","START_SW"),
            ("X001","STOP_SW"),
            ("X002","RESET_SW"),
            ("X003","EMG_SW"),
            ("X004","MAIN_CDA_CHECK"),
            ("X005","MAIN_EJECTOR_CHECK"),
            ("X006","MAIN_VACUUM1"),
            ("X007","MAIN_VACUUM2"),
            ("X008","MAIN_VACUUM3"),
            ("X009","MAIN_VACUUM4"),
            ("X010","FRONT_DOOR"),
            ("X011","LEFT_DOOR"),
            ("X012","REAR_DOOR"),
            ("X013","RIGHT_DOOR"),
            ("X014","WAFER_IONIZER_ALARM"),
            ("X015","BIN_IONIZER_ALARM"),
            ("X018","WAFER LIFTER RING JUT CHECK"),
            ("X024","WAFER FEEDER OVERLOAD CHECK"),
            ("X032","LEFT TOOL AIR TANK PRESSURE CHECK"),
            ("X033","LEFT TOOL VACUUM TANK PRESSURE CHECK"),
            ("X040","INDEX AIR TANK PRESSURE CHECK"),
            ("X041","INDEX VACCUM TANK PRRESSURE CHECK"),
            ("X050","PROBE CARD VACUUM CHECK"),
            ("X051","RIGHT TOOL AIR TANK PRESSURE CHECK"),
            ("X052","RIGHT TOOL VACUUM TANK PRESSURE CHECK"),
            ("X068","BIN FEEDER OVERLOAD CHECK")
        };
        private (string disp, string key)[] _outputMap = new[]
        {
            ("Y000","START_LAMP"),("Y001","STOP_LAMP"),("Y002","RESET_LAMP"),("Y003","TL_RED"),
            ("Y004","TL_YELLOW"),("Y005","TL_GREEN"),("Y006","BUZZER"),
            ("Y014","WAFER_IONIZER_ON"),("Y015","BIN_IONIZER_ON")
        };
        // 논리키 → DisplayNo 빠른 조회 사전
        private Dictionary<string, string> _inputDispByKey;
        private Dictionary<string, string> _outputDispByKey;

        // ----- 물리 버튼 Edge/디바운스 -----
        private bool _prevStartSw, _prevStopSw, _prevResetSw;
        private int _lastStartEdgeTick, _lastStopEdgeTick, _lastResetEdgeTick;
        private const int ButtonDebounceMs = 150;

        // ----- 경광등 패턴(블링크 관리) -----
        private TowerLampPattern _currentPattern = TowerLampPattern.AllOff;
        private int _lastBlinkTick = Environment.TickCount;
        private bool _blinkPhaseOn = true;
        private const int BlinkPeriodMs_Warning = 500; // Yellow
        private const int BlinkPeriodMs_Alarm = 400;   // Red + Buzzer

        EquipmentConfig Config 
        { 
            get 
            {
                var eq = Equipment.Instance;
                if (eq.EquipmentConfig != null)
                    return eq.EquipmentConfig as EquipmentConfig;
                else
                    return base.Config;
            } 
        }

        public EquipmentStatus(EquipmentConfig config = null)
            : base(new EquipmentConfig())
        {
            RebuildIoDictionaries_NoLock();
        }

        #region Lifecycle
        protected override void InitAlarm()
        {
            base.InitAlarm();
            AlarmRegister((int)AlarmKeys.eAlarmEMO, 
                "EMERGENCY PRESSED", "EMG_SW(X003) 입력", "Error");
            AlarmRegister((int)AlarmKeys.eAlarm_WaferFeederOverload,
                "Wafer Feeder Overload", "wafer feeder 확인 바랍니다.!", "Error");
            AlarmRegister((int)AlarmKeys.eAlarm_BinFeederOverload,
               "Bin Feeder Overload", "Bin feeder 확인 바랍니다.!", "Error");
            //eAlarm_MAinCDA
            AlarmRegister((int)AlarmKeys.eAlarm_MAinCDA,
               "Main CDA", "Main CAD 확인 바랍니다.!", "Error");
            //eAlarm_MainEjector
            AlarmRegister((int)AlarmKeys.eAlarm_MainEjector,
              "Main Ejector", "Main Ejector 확인 바랍니다.!", "Error");
        }

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

            ret = OnRunWork(); 
            return ret;
        }

        public override int OnStop()
        {
            int ret = 0;
            this.RunUnitStatus = UnitStatus.Stopped;
            base.OnStop();
            return ret;
        }

        protected override int OnRunReady() { return 0; }

        //protected override int OnRunWork()
        //{
        //    var now = Environment.TickCount;
        //    if (unchecked(now - _lastRefreshTick) >= RefreshIntervalMs)
        //    {
        //        Refresh();
        //        _lastRefreshTick = now;

        //        var snap = GetSnapshot();
        //        // EMG 상승 에지 시 즉시 정지
        //        var emg = snap.AnyEmg;
        //        if (emg && !_prevEmg)
        //        {
        //            try
        //            {
        //                Equipment.Instance?.AxisManager?.EmgStopAll();
        //            }
        //            catch { /* 드라이버 예외 무시 */ }
        //            ApplyTowerPattern(TowerLampPattern.Alarm);
        //            PostAlarm((int)AlarmKeys.eAlarmEMO);
        //        }
        //        _prevEmg = emg;

        //        // 물리 버튼 처리(상승 에지)
        //        ProcessPhysicalButtons(snap, now);

        //        // 버튼 램프 상태 동기화
        //        UpdateButtonLampsByEqState();

        //        // [추가] 경광등 패턴 결정(버튼 + 상태/안전조건 반영)
        //        UpdateTowerPatternByStateAndInputs(snap);

        //        // 경광등 패턴 블링크/유지
        //        UpdatePatternRuntime(now);

        //        //상시로 확인해야 하는 인터락 신호?
        //        //CheckInterLook();

        //        //노트북 우선 막음.
        //        //if (snap.bWaferFeederOverloadCheck == false)
        //        //{
        //        //    PostAlarm((int)AlarmKeys.eAlarm_WaferFeederOverload);
        //        //}

        //        //if(snap.bBinFeederOverloadCheck == false)
        //        //{
        //        //    PostAlarm((int)AlarmKeys.eAlarm_BinFeederOverload);
        //        //}

        //        //if (snap.bMainCDACheck == false)
        //        //{
        //        //    PostAlarm((int)AlarmKeys.eAlarm_MAinCDA);
        //        //}

        //        //if (snap.bMainEjectorCheck == false)
        //        //{
        //        //    PostAlarm((int)AlarmKeys.eAlarm_MainEjector);
        //        //}


        //    }

        //    return 0;
        //}
        protected override int OnRunWork()
        {
            var now = Environment.TickCount;
            if (unchecked(now - _lastRefreshTick) >= RefreshIntervalMs)
            {
                Refresh();
                _lastRefreshTick = now;

                var snap = GetSnapshot();

                // [수정] 시작 후 알람 억제 + EMG 노이즈 필터 + 첫 사이클 동기화
                // 1) 첫 사이클에서는 에지로 취급하지 않도록 현재 상태로 동기화
                if (_firstCycle)
                {
                    _prevEmg = snap.AnyEmg;
                    _emgAssertTick = snap.AnyEmg ? now : 0;
                    _firstCycle = false;
                }

                // 2) EMG 유지시간 판정(노이즈 필터)
                var emg = snap.AnyEmg;
                bool emgSustained = false;
                if (emg)
                {
                    if (_emgAssertTick == 0) _emgAssertTick = now;
                    emgSustained = unchecked(now - _emgAssertTick) >= EmgHoldMs;
                }
                else
                {
                    _emgAssertTick = 0;
                }

                // 3) 시작 억제 시간 경과 후, 상승 에지이면서 유지시간 충족 시에만 알람
                bool startupSuppressed = unchecked(now - _startupSuppressUntilTick) < 0;
                if (!startupSuppressed && emg && emgSustained && !_prevEmg)
                {
                    try
                    {
                        Equipment.Instance?.AxisManager?.EmgStopAll();
                    }
                    catch { /* 드라이버 미초기화 등 무시 */ }

                    ApplyTowerPattern(TowerLampPattern.Alarm);
                    PostAlarm((int)AlarmKeys.eAlarmEMO);
                }
                _prevEmg = emg;

                // 필요시 인터락/설비 확인 알람은 여기서 추가 검토
                // 프로그램 첫 시작 동안은 설비 확인성 알람 보류
                if(Config.IsSimulation == false)
                {
                    if (!startupSuppressed)
                    {
                        if (snap.bWaferFeederOverloadCheck == false)
                        {
                            PostAlarm((int)AlarmKeys.eAlarm_WaferFeederOverload);
                        }

                        if (snap.bBinFeederOverloadCheck == false)
                        {
                            PostAlarm((int)AlarmKeys.eAlarm_BinFeederOverload);
                        }

                        if (snap.bMainCDACheck == false)
                        {
                            PostAlarm((int)AlarmKeys.eAlarm_MAinCDA);
                        }

                        if (snap.bMainEjectorCheck == false)
                        {
                            PostAlarm((int)AlarmKeys.eAlarm_MainEjector);
                        }
                    }
                }

                // 물리 버튼 처리(시작/정지/리셋)
                ProcessPhysicalButtons(snap, now);

                // 버튼 램프 상태 갱신
                UpdateButtonLampsByEqState();

                // [수정] 타워램프 패턴 결정
                UpdateTowerPatternByStateAndInputs(snap);

                // 타워램프 패턴 동작(점멸 등)
                UpdatePatternRuntime(now);
            }

            return 0;
        }

        protected override int OnRunComplete() { return 0; }
        #endregion

        #region DIO 접근 유틸
        private static IEnumerable<string> EnumerateModuleNames()
        {
            var eq = Equipment.Instance;
            var unit = eq?.UnitIO;
            if (unit?.Modules == null) 
                yield break;
            foreach (var m in unit.Modules)
                if (!string.IsNullOrWhiteSpace(m.ModuleName))
                    yield return m.ModuleName;
        }

        private static bool TryReadInput(DioScanService dio, string disp, out bool value)
        {
            foreach (var mod in EnumerateModuleNames())
            {
                if (dio.TryGetInput(mod, disp, out value))
                    return true;
            }
            value = false;
            return false;
        }

        private static bool TryReadOutput(DioScanService dio, string disp, out bool value)
        {
            foreach (var mod in EnumerateModuleNames())
            {
                if (dio.TryGetOutput(mod, disp, out value))
                    return true;
            }
            value = false;
            return false;
        }
        #endregion

        #region 동적 I/O 매핑 API
        private void RebuildIoDictionaries_NoLock()
        {
            _inputDispByKey = _inputMap.ToDictionary(t => t.key, t => t.disp, StringComparer.OrdinalIgnoreCase);
            _outputDispByKey = _outputMap.ToDictionary(t => t.key, t => t.disp, StringComparer.OrdinalIgnoreCase);
        }

        public void ReplaceIoMaps(IEnumerable<(string disp, string key)> inputs = null,
                                  IEnumerable<(string disp, string key)> outputs = null)
        {
            lock (_ioMapLock)
            {
                if (inputs != null) _inputMap = inputs.ToArray();
                if (outputs != null) _outputMap = outputs.ToArray();
                RebuildIoDictionaries_NoLock();
            }
        }

        public void UpsertInputMap(string disp, string key)
        {
            if (string.IsNullOrWhiteSpace(disp) || string.IsNullOrWhiteSpace(key)) return;
            lock (_ioMapLock)
            {
                var list = _inputMap.ToList();
                int idx = list.FindIndex(p =>
                    p.disp.Equals(disp, StringComparison.OrdinalIgnoreCase) ||
                    p.key.Equals(key, StringComparison.OrdinalIgnoreCase));
                if (idx >= 0) list[idx] = (disp, key);
                else list.Add((disp, key));
                _inputMap = list.ToArray();
                RebuildIoDictionaries_NoLock();
            }
        }

        public void UpsertOutputMap(string disp, string key)
        {
            if (string.IsNullOrWhiteSpace(disp) || string.IsNullOrWhiteSpace(key)) return;
            lock (_ioMapLock)
            {
                var list = _outputMap.ToList();
                int idx = list.FindIndex(p =>
                    p.disp.Equals(disp, StringComparison.OrdinalIgnoreCase) ||
                    p.key.Equals(key, StringComparison.OrdinalIgnoreCase));
                if (idx >= 0) list[idx] = (disp, key);
                else list.Add((disp, key));
                _outputMap = list.ToArray();
                RebuildIoDictionaries_NoLock();
            }
        }

        public bool RemoveInputByDisp(string disp)
        {
            lock (_ioMapLock)
            {
                var list = _inputMap.ToList();
                int removed = list.RemoveAll(p => p.disp.Equals(disp, StringComparison.OrdinalIgnoreCase));
                if (removed > 0)
                {
                    _inputMap = list.ToArray();
                    RebuildIoDictionaries_NoLock();
                    return true;
                }
                return false;
            }
        }

        public bool RemoveOutputByDisp(string disp)
        {
            lock (_ioMapLock)
            {
                var list = _outputMap.ToList();
                int removed = list.RemoveAll(p => p.disp.Equals(disp, StringComparison.OrdinalIgnoreCase));
                if (removed > 0)
                {
                    _outputMap = list.ToArray();
                    RebuildIoDictionaries_NoLock();
                    return true;
                }
                return false;
            }
        }

        public IReadOnlyList<(string disp, string key)> GetInputMapSnapshot()
        {
            lock (_ioMapLock) return _inputMap.ToArray();
        }

        public IReadOnlyList<(string disp, string key)> GetOutputMapSnapshot()
        {
            lock (_ioMapLock) return _outputMap.ToArray();
        }
        #endregion

        #region Refresh / Snapshot
        public void Refresh()
        {
            var dio = Equipment.Instance?.DioScan;
            if (dio == null) return;

            (string disp, string key)[] inMap, outMap;
            lock (_ioMapLock)
            {
                inMap = _inputMap;
                outMap = _outputMap;
            }

            var snap = new EquipmentStatusSnapshot
            {
                Timestamp = DateTime.Now
            };

            foreach (var (disp, key) in inMap)
            {
                bool v;
                if (TryReadInput(dio, disp, out v))
                    snap.Inputs[key] = v;
            }

            foreach (var (disp, key) in outMap)
            {
                bool v;
                if (TryReadOutput(dio, disp, out v))
                    snap.Outputs[key] = v;
            }

            snap.AllVacuumOk = GetAll(snap.Inputs, "MAIN_VACUUM1", "MAIN_VACUUM2", "MAIN_VACUUM3", "MAIN_VACUUM4");
            snap.AllDoorClosed = GetAll(snap.Inputs, "FRONT_DOOR", "LEFT_DOOR", "REAR_DOOR", "RIGHT_DOOR");
            snap.AnyEmg = snap.Inputs.TryGetValue("EMG_SW", out var emg) && emg;

            snap.bWaferFeederOverloadCheck = snap.Inputs.TryGetValue("WAFER FEEDER OVERLOAD CHECK", out var bon) && bon;
            snap.bBinFeederOverloadCheck = snap.Inputs.TryGetValue("BIN FEEDER OVERLOAD CHECK", out var bon1) && bon1;
            snap.bMainCDACheck = snap.Inputs.TryGetValue("MAIN_CDA_CHECK", out var bon2) && bon2;
            snap.bMainEjectorCheck = snap.Inputs.TryGetValue("MAIN_EJECTOR_CHECK", out var bon3) && bon3;

            lock (_gate)
                _latest = snap;
        }

        private bool GetAll(Dictionary<string, bool> dict, params string[] keys)
        {
            foreach (var k in keys)
                if (!dict.TryGetValue(k, out var v) || !v)
                    return false;
            return true;
        }

        public EquipmentStatusSnapshot GetSnapshot()
        {
            lock (_gate)
                return _latest.Clone();
        }
        #endregion

        #region 출력 제어
        public int SetOutputRaw(string displayNo, bool on)
        {
            var dio = Equipment.Instance?.DioScan;
            if (dio == null) return -1;

            foreach (var mod in EnumerateModuleNames())
                if (dio.WriteOutput(mod, displayNo, on) == 0)
                    return 0;

            return -1;
        }

        public int SetOutput(string key, bool on)
        {
            string disp;
            lock (_ioMapLock)
                if (!_outputDispByKey.TryGetValue(key, out disp))
                    return -1;
            return SetOutputRaw(disp, on);
        }
        #endregion

        #region 비즈니스 API (논리키 기반 I/O)
        public bool ReadInput(string key)
        {
            string disp;
            lock (_ioMapLock)
                if (!_inputDispByKey.TryGetValue(key, out disp)) return false;

            var dio = Equipment.Instance?.DioScan;
            if (dio == null) return false;

            bool value;
            return TryReadInput(dio, disp, out value) && value;
        }

        public bool WriteOutput(string key, bool on)
        {
            return SetOutput(key, on) == 0;
        }

        public bool IsOutputOn(string key)
        {
            string disp;
            lock (_ioMapLock)
                if (!_outputDispByKey.TryGetValue(key, out disp)) return false;

            var dio = Equipment.Instance?.DioScan;
            if (dio == null) return false;

            bool value;
            return TryReadOutput(dio, disp, out value) && value;
        }

        public bool GetInput(string key)
        {
            var snap = GetSnapshot();
            return snap.Inputs.TryGetValue(key, out var v) && v;
        }

        public bool GetOutput(string key)
        {
            var snap = GetSnapshot();
            return snap.Outputs.TryGetValue(key, out var v) && v;
        }

        public bool TryGetInputRaw(string key, out bool value)
        {
            value = false;
            string disp;
            lock (_ioMapLock)
                if (!_inputDispByKey.TryGetValue(key, out disp)) return false;

            var dio = Equipment.Instance?.DioScan;
            if (dio == null) return false;

            return TryReadInput(dio, disp, out value);
        }

        public bool TryGetOutputRaw(string key, out bool value)
        {
            value = false;
            string disp;
            lock (_ioMapLock)
                if (!_outputDispByKey.TryGetValue(key, out disp)) return false;

            var dio = Equipment.Instance?.DioScan;
            if (dio == null) return false;

            return TryReadOutput(dio, disp, out value);
        }

        public IReadOnlyList<string> GetKnownInputKeys()
        {
            lock (_ioMapLock) return _inputDispByKey.Keys.ToList();
        }

        public IReadOnlyList<string> GetKnownOutputKeys()
        {
            lock (_ioMapLock) return _outputDispByKey.Keys.ToList();
        }
        #endregion

        #region 물리 버튼 헬퍼
        private static bool IsRisingEdge(bool cur, ref bool prev, ref int lastTick, int now)
        {
            if (cur && !prev && unchecked(now - lastTick) > ButtonDebounceMs)
            {
                lastTick = now;
                prev = cur;
                return true;
            }
            prev = cur;
            return false;
        }

        private bool CanAutoStart(EquipmentStatusSnapshot snap)
        {
            if (snap == null) return false;
            if (snap.AnyEmg) return false;
            if (!snap.AllDoorClosed) return false;
            if (!snap.AllVacuumOk) return false;
            return true;
        }

        private void ProcessPhysicalButtons(EquipmentStatusSnapshot snap, int now)
        {
            if (snap?.Inputs == null) 
                return;

            var eq = Equipment.Instance;
            var state = eq?.EqState ?? EquipmentState.Unknown;

            // START
            var swStart = ReadInput("START_SW");
            if (IsRisingEdge(swStart, ref _prevStartSw, ref _lastStartEdgeTick, now))
            {
                if (CanAutoStart(snap) 
                    && state != EquipmentState.AutoRunning && state != EquipmentState.Starting)
                {
                    try 
                    {
                        //var _ = eq.StartAllUnitsAsync();
                        // [MOD] StartAllUnitsAsync() -> SequenceStartAllAsync()
                        var _ = eq.SequenceStartAllAsync(CancellationToken.None);
                    } 
                    catch { }
                }
                else
                {
                    // 조건 미충족 → Warning 패턴
                    ApplyTowerPattern(TowerLampPattern.Warning);
                }
            }

            // STOP
            var swStop = ReadInput("STOP_SW");
            if (IsRisingEdge(swStop, ref _prevStopSw, ref _lastStopEdgeTick, now))
            {
                if (state != EquipmentState.Stopped && state != EquipmentState.Stopping)
                {
                    try 
                    { 
                        //var _ = eq.StopAllUnitsAsync();
                        var _ = eq.SequenceStopAllAsync(CancellationToken.None);
                    } 
                    catch { }
                }
            }

            // RESET → 부저 OFF
            var swReset = ReadInput("RESET_SW");
            if (IsRisingEdge(swReset, ref _prevResetSw, ref _lastResetEdgeTick, now))
            {
                WriteOutput("BUZZER", false);
            }
        }

        private void UpdateButtonLampsByEqState()
        {
            var eq = Equipment.Instance;
            var state = eq?.EqState ?? EquipmentState.Unknown;

            // 버튼 우선 표시: 눌리는 동안 램프 강제 ON
            var startPressed = ReadInput("START_SW");
            var stopPressed = ReadInput("STOP_SW");
            var resetPressed = ReadInput("RESET_SW");

            WriteOutput("START_LAMP",
                startPressed || state == EquipmentState.AutoRunning || state == EquipmentState.Starting);

            WriteOutput("STOP_LAMP",
                stopPressed || state == EquipmentState.Stopped || state == EquipmentState.Stopping || state == EquipmentState.CycleStop);

            WriteOutput("RESET_LAMP",
                resetPressed || state == EquipmentState.Error || state == EquipmentState.Reset);
        }
        #endregion

        #region 경광등 패턴
        public void ApplyTowerPattern(TowerLampPattern pattern)
        {
            if (_currentPattern == pattern)
                return;

            _currentPattern = pattern;
            _blinkPhaseOn = true;
            _lastBlinkTick = Environment.TickCount;
            UpdateTowerLampsForPattern(pattern, _blinkPhaseOn); // 즉시 1회 반영
        }

        // [추가] 버튼 + 설비상태/안전조건을 결합한 경광등 패턴 결정 로직
        private void UpdateTowerPatternByStateAndInputs(EquipmentStatusSnapshot snap)
        {
            var eq = Equipment.Instance;
            var state = eq?.EqState ?? EquipmentState.Unknown;

            try
            {
                // 1) 기본 패턴: EMG/에러/안전조건/운전상태
                TowerLampPattern pattern;
                if (snap.AnyEmg == false
                    || state == EquipmentState.Error
                    || AlarmManager.Instance.IsAlarm)
                {
                    pattern = TowerLampPattern.Alarm;             // 빨강 + 버저 블링크
                }
                //셋업때만 막자.
                //else if (snap.AllDoorClosed == false
                //         || !snap.AllVacuumOk)
                //{
                //    pattern = TowerLampPattern.Warning;           // 노랑 블링크
                //}
                else if (state == EquipmentState.Stopped
                        || state == EquipmentState.Stopping
                        || state == EquipmentState.CycleStop)
                {
                    pattern = TowerLampPattern.Warning;           // 노랑 블링크
                }
                else if (state == EquipmentState.AutoRunning
                        || state == EquipmentState.Starting)
                {
                    pattern = TowerLampPattern.Running;           // 초록 고정
                }
                else
                    pattern = TowerLampPattern.Idle;              // 초록 고정(Idle 정의 유지)

                // 2) 버튼 오버라이드(즉시 시각 피드백)
                var startPressed = ReadInput("START_SW");
                var stopPressed = ReadInput("STOP_SW");
                var resetPressed = ReadInput("RESET_SW");

                if (stopPressed)
                    pattern = TowerLampPattern.Warning;           // STOP 누르면 노랑 블링크
                else if (startPressed)
                    pattern = TowerLampPattern.Running;           // START 누르면 초록 고정

                // RESET은 버저 OFF만 담당(패턴은 위 기본 규칙 유지)
                // if (resetPressed) { /* buzzer off는 ProcessPhysicalButtons에서 처리 */ }

                ApplyTowerPattern(pattern);
            }
            catch (Exception ex) 
            {
                Log.Write(ex);
            }

            
        }

        private void UpdatePatternRuntime(int nowTick)
        {
            int period = 0;
            switch (_currentPattern)
            {
                case TowerLampPattern.Warning: period = BlinkPeriodMs_Warning; break;
                case TowerLampPattern.Alarm: period = BlinkPeriodMs_Alarm; break;
                default: period = 0; break;
            }

            if (period > 0)
            {
                if (unchecked(nowTick - _lastBlinkTick) >= period)
                {
                    _lastBlinkTick = nowTick;
                    _blinkPhaseOn = !_blinkPhaseOn;
                    UpdateTowerLampsForPattern(_currentPattern, _blinkPhaseOn);
                }
            }
            else
            {
                // 고정 패턴은 유지(외부 변경 대비)
                UpdateTowerLampsForPattern(_currentPattern, true);
            }
        }

        private void UpdateTowerLampsForPattern(TowerLampPattern pattern, bool phaseOn)
        {
            WriteOutput("TL_RED", false);
            WriteOutput("TL_YELLOW", false);
            WriteOutput("TL_GREEN", false);
            WriteOutput("BUZZER", false);

            switch (pattern)
            {
               
                case TowerLampPattern.Running:
                    WriteOutput("TL_GREEN", true);
                    break;

                case TowerLampPattern.Idle:
                case TowerLampPattern.Warning:
                    WriteOutput("TL_YELLOW", phaseOn);
                    break;

                case TowerLampPattern.Alarm:
                    WriteOutput("TL_RED", phaseOn);
                    if(Equipment.Instance.m_bBuzzerOff == true)
                    {
                        WriteOutput("BUZZER", phaseOn);
                    }
                    break;

                case TowerLampPattern.AllOff:
                default:
                    // 모두 OFF
                    break;
            }
        }
        #endregion

        #region 논리 입력/출력 헬퍼
        bool IsSwitchStart() => ReadInput("START_SW");
        bool IsSwitchStop() => ReadInput("STOP_SW");
        bool IsSwitchReset() => ReadInput("RESET_SW");
        bool IsSwitchEmg() => ReadInput("EMG_SW");

        bool IsMainCdaOk() => ReadInput("MAIN_CDA_CHECK");
        bool IsMainEjectorOk() => ReadInput("MAIN_EJECTOR_CHECK");
        bool IsMainVacuum1Ok() => ReadInput("MAIN_VACUUM1");
        bool IsMainVacuum2Ok() => ReadInput("MAIN_VACUUM2");
        bool IsMainVacuum3Ok() => ReadInput("MAIN_VACUUM3");
        bool IsMainVacuum4Ok() => ReadInput("MAIN_VACUUM4");

        bool IsFrontDoorClosed() => ReadInput("FRONT_DOOR");
        bool IsLeftDoorClosed() => ReadInput("LEFT_DOOR");
        bool IsRearDoorClosed() => ReadInput("REAR_DOOR");
        bool IsRightDoorClosed() => ReadInput("RIGHT_DOOR");

        bool IsWaferIonizerAlarmOn() => ReadInput("WAFER_IONIZER_ALARM");
        bool IsBinIonizerAlarmOn() => ReadInput("BIN_IONIZER_ALARM");

        bool AreAllMainVacuumOk() => IsMainVacuum1Ok() && IsMainVacuum2Ok() && IsMainVacuum3Ok() && IsMainVacuum4Ok();
        bool AreAllDoorsClosed() => IsFrontDoorClosed() && IsLeftDoorClosed() && IsRearDoorClosed() && IsRightDoorClosed();

        bool SetStartLamp(bool on) => WriteOutput("START_LAMP", on);
        bool IsStartLampOn() => IsOutputOn("START_LAMP");
        bool SetStopLamp(bool on) => WriteOutput("STOP_LAMP", on);
        bool IsStopLampOn() => IsOutputOn("STOP_LAMP");
        bool SetResetLamp(bool on) => WriteOutput("RESET_LAMP", on);
        bool IsResetLampOn() => IsOutputOn("RESET_LAMP");

        bool SetTowerRed(bool on) => WriteOutput("TL_RED", on);
        bool IsTowerRedOn() => IsOutputOn("TL_RED");
        bool SetTowerYellow(bool on) => WriteOutput("TL_YELLOW", on);
        bool IsTowerYellowOn() => IsOutputOn("TL_YELLOW");
        bool SetTowerGreen(bool on) => WriteOutput("TL_GREEN", on);
        bool IsTowerGreenOn() => IsOutputOn("TL_GREEN");

        bool SetBuzzer(bool on) => WriteOutput("BUZZER", on);
        bool IsBuzzerOn() => IsOutputOn("BUZZER");

        bool SetBladeContact(bool on) => WriteOutput("BLADE_CONTACT", on);
        bool IsBladeContactOn() => IsOutputOn("BLADE_CONTACT");
        bool SetProbeCardContact(bool on) => WriteOutput("PROBECARD_CONTACT", on);
        bool IsProbeCardContactOn() => IsOutputOn("PROBECARD_CONTACT");

        bool SetWaferIonizer(bool on) => WriteOutput("WAFER_IONIZER_ON", on);
        bool IsWaferIonizerOn() => IsOutputOn("WAFER_IONIZER_ON");
        bool SetBinIonizer(bool on) => WriteOutput("BIN_IONIZER_ON", on);
        bool IsBinIonizerOn() => IsOutputOn("BIN_IONIZER_ON");
        #endregion

        public bool IsEmgOn() => GetSnapshot().AnyEmg;
    }

    public enum TowerLampPattern
    {
        Idle,
        Running,
        Warning,
        Alarm,
        AllOff
    }

    public class EquipmentStatusSnapshot
    {
        public DateTime Timestamp { get; set; }
        public Dictionary<string, bool> Inputs { get; set; } = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, bool> Outputs { get; set; } = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);

        public bool AllVacuumOk { get; set; }
        public bool AllDoorClosed { get; set; }
        public bool AnyEmg { get; set; } = true;

        public bool bWaferFeederOverloadCheck { get; set; }
        public bool bBinFeederOverloadCheck { get; set; }
        public bool bMainCDACheck { get; set; }
        public bool bMainEjectorCheck { get; set; }

        public EquipmentStatusSnapshot Clone() => new EquipmentStatusSnapshot
        {
            Timestamp = Timestamp,
            Inputs = new Dictionary<string, bool>(Inputs, StringComparer.OrdinalIgnoreCase),
            Outputs = new Dictionary<string, bool>(Outputs, StringComparer.OrdinalIgnoreCase),
            AllVacuumOk = AllVacuumOk,
            AllDoorClosed = AllDoorClosed,
            AnyEmg = AnyEmg,
            bWaferFeederOverloadCheck = bWaferFeederOverloadCheck,
            bBinFeederOverloadCheck = bBinFeederOverloadCheck,
            bMainCDACheck = bMainCDACheck,
            bMainEjectorCheck = bMainEjectorCheck

        };
    }
}