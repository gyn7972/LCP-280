using System;
using System.Collections.Generic;
using System.Linq; // 여러 물리 DIO 모듈 순회용
using QMC.Common.Unit;
using QMC.Common.DIO;
using QMC.Common.IO;
using QMC.Common;

namespace QMC.LCP_280.Process.Unit
{
    /// <summary>
    /// 설비 공통 상태(I/O) 스냅샷 관리 유닛.
    /// - OP 패널 스위치, 도어 센서, 진공 확인, 이오나이저 알람, 타워램프 출력 등 기본 신호를 캡슐화.
    /// - DioScanService (스캔 스레드/서비스)가 유지하는 캐시를 읽어 값 확보 (고속 폴링 불필요).
    /// - Refresh() 호출 시 내부 스냅샷 교체 (일반적으로 100ms 주기 권장).
    /// - SetOutput()/ApplyTowerPattern() 로 단순 제어 지원.
    /// - Thread-safe: 외부에서 GetSnapshot() 으로 읽을 때 복제본 반환.
    /// 
    /// 확장 아이디어:
    /// - 버튼 Edge 검출/디바운스 → 별도 FSM 유닛 또는 향후 본 클래스 확장.
    /// - Ready/Alarm/Running 상태 판단 → 상위 Control / Sequencer 에서 스냅샷 기반 계산.
    /// </summary>
    public class EquipmentStatus : BaseUnit
    {
        private readonly object _gate = new object();          // 스냅샷 보호용 락
        private EquipmentStatusSnapshot _latest = new EquipmentStatusSnapshot(); // 최신 스냅샷

        //EMG 확인
        private const int RefreshIntervalMs = 100;              
        private int _lastRefreshTick = Environment.TickCount;
        private const int EMG_ALARM_CODE = 20001;
        private bool _prevEmg = false;

        public EquipmentStatus() : base("EquipmentStatus") { }
        // (중요) 실제 장비 설정에서 DIO 모듈명이 고정되지 않을 수 있어
        // 기존 단일 상수명으로 접근 실패 → 모든 등록 모듈 순회하며 DisplayNo 첫 매칭을 시도.
        private static IEnumerable<string> EnumerateModuleNames()
        {
            var eq = Equipment.Instance;
            var unit = eq?.UnitIO;
            if (unit?.Modules == null) yield break;
            foreach (var m in unit.Modules)
                if (!string.IsNullOrWhiteSpace(m.ModuleName))
                    yield return m.ModuleName;
        }

        #region Lifecycle
        protected override void InitAlarm()
        {
            base.InitAlarm();
            // [ADD] EMG 알람 등록
            AlarmRegister(EMG_ALARM_CODE, "EMERGENCY PRESSED", "EMG_SW(X003) 입력", "Error");
        }

        public override int OnRun()
        {
            int ret = 0;

            if (this.Status == UnitRunStatus.Stop || this.Status == UnitRunStatus.CycleStop)
            {
                this.State = ProcessState.Stop;
                return 1;
            }

            ret = OnRunWork();

            //switch (State)
            //{
            //    case ProcessState.Ready:
            //        ret = OnRunReady();
            //        break;
            //    case ProcessState.Work:
            //        ret = OnRunWork();
            //        break;
            //    case ProcessState.Complete:
            //        ret = OnRunComplete();
            //        break;
            //    default:
            //        break;
            //}
            //if (ret != 0)
            //{
            //    this.State = ProcessState.Stop;
            //}

            return ret;
        }
        public override int OnStop() 
        { 
            int ret = 0; 
            base.OnStop(); 
            return ret; 
        }

        protected override int OnRunReady() 
        {
            return 0; 
        }
        protected override int OnRunWork() 
        {
            var now = Environment.TickCount;
            if (unchecked(now - _lastRefreshTick) >= RefreshIntervalMs)
            {
                Refresh();
                _lastRefreshTick = now;

                // [ADD] EMG 즉시 반응 (하강 에지에서만)
                var snap = GetSnapshot();
                var emg = snap.AnyEmg;
                if (!emg && _prevEmg)
                {
                    try
                    {
                        Equipment.Instance?.AxisManager?.EmgStopAll();
                    }
                    catch { /* 드라이버 예외 무시하고 알람은 계속 */ }

                    // 타워램프/부저: Alarm 패턴
                    //ApplyTowerPattern(TowerLampPattern.Alarm); // 설비 정상화시 주석 해제 할 것

                    // 알람 포스트
                    //PostAlarm(EMG_ALARM_CODE); // 설비 정상화시 주석 해제 할 것
                }
                _prevEmg = emg;
            }

            return 0; 
        }
        protected override int OnRunComplete() 
        { 
            return 0; 
        }


        #endregion



        /// <summary>
        /// 입력 신호(Display 번호 기반)를 모든 모듈에서 탐색하여 최초 성공 값을 반환.
        /// 실패 시 false 리턴 (value = false).
        /// </summary>
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

        /// <summary>
        /// 출력 신호(Display 번호 기반)를 모든 모듈에서 탐색하여 최초 성공 값을 반환.
        /// (현재 출력 값 읽기 용도. 일부 장치에서 지원 안 할 수 있음)
        /// </summary>
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

        #region I/O 매핑 (DisplayNo → 논리 키)
        // 물리 Display 번호(X***, Y***) 와 논리 키를 매핑하여 코드 상에서 문자열 키로 접근 단순화.
        private static readonly (string disp, string key)[] _inputMap = new[]
        {
            ("X000","START_SW"),("X001","STOP_SW"),("X002","RESET_SW"),("X003","EMG_SW"),
            ("X004","MAIN_CDA_CHECK"),("X005","MAIN_EJECTOR_CHECK"),("X006","MAIN_VACUUM1"),("X007","MAIN_VACUUM2"),
            ("X008","MAIN_VACUUM3"),("X009","MAIN_VACUUM4"),("X010","FRONT_DOOR"),("X011","LEFT_DOOR"),
            ("X012","REAR_DOOR"),("X013","RIGHT_DOOR"),("X014","WAFER_IONIZER_ALARM"),("X015","BIN_IONIZER_ALARM"),
        };

        private static readonly (string disp, string key)[] _outputMap = new[]
        {
            ("Y000","START_LAMP"),("Y001","STOP_LAMP"),("Y002","RESET_LAMP"),("Y003","TL_RED"),
            ("Y004","TL_YELLOW"),("Y005","TL_GREEN"),("Y006","BUZZER"),("Y012","BLADE_CONTACT"),
            ("Y013","PROBE_CARD_CONTACT"),("Y014","WAFER_IONIZER_ON"),("Y015","BIN_IONIZER_ON"),
        };
        #endregion

        /// <summary>
        /// I/O 스캔 캐시(DioScanService)에서 현재 값을 읽고 파생 상태를 계산하여 스냅샷을 갱신.
        /// - 예외/오류는 상위에서 주기 호출 시 무시해도 되는 경량 작업으로 설계.
        /// - 호출 주기: 100ms (권장) 또는 필요 시 더 빠르게 가능 (단, UI / 로깅 부하 고려).
        /// </summary>
        public void Refresh()
        {
            var dio = Equipment.Instance?.DioScan;
            if (dio == null) return;

            var snap = new EquipmentStatusSnapshot
            {
                Timestamp = DateTime.Now
            };

            // 입력 수집
            foreach (var (disp, key) in _inputMap)
            {
                bool v;
                if (TryReadInput(dio, disp, out v))
                    snap.Inputs[key] = v;
            }

            // 출력 수집 (현재 출력 상태 모니터링 필요 시)
            foreach (var (disp, key) in _outputMap)
            {
                bool v;
                if (TryReadOutput(dio, disp, out v))
                    snap.Outputs[key] = v;
            }

            // 파생 상태 계산
            snap.AllVacuumOk = GetAll(snap.Inputs, "MAIN_VACUUM1","MAIN_VACUUM2","MAIN_VACUUM3","MAIN_VACUUM4");
            snap.AllDoorClosed = GetAll(snap.Inputs, "FRONT_DOOR","LEFT_DOOR","REAR_DOOR","RIGHT_DOOR");
            snap.AnyEmg = snap.Inputs.TryGetValue("EMG_SW", out var emg) && emg;

            // 교체 (lock 최소화)
            lock (_gate)
                _latest = snap;
        }

        /// <summary>
        /// 지정된 Key 들이 모두 true(존재 + true) 인지 검사.
        /// 일부 키가 미존재하거나 false 면 false 반환.
        /// </summary>
        private bool GetAll(Dictionary<string,bool> dict, params string[] keys)
        {
            foreach (var k in keys)
            {
                if (!dict.TryGetValue(k, out var v) || !v)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// 최신 상태의 복제본을 반환 (원본 변경 방지).
        /// - 빈도 높은 호출(UI, 로직) 가능한 형태.
        /// - Dictionary 는 새 인스턴스로 복사됨.
        /// </summary>
        public EquipmentStatusSnapshot GetSnapshot()
        {
            lock (_gate)
                return _latest.Clone();
        }

        /// <summary>
        /// 물리 DisplayNo(Y*** 등) 직접 지정하여 출력 제어.
        /// - 내부적으로 모든 모듈 순회 → 첫 성공 시 0 반환.
        /// - 실패 시 -1.
        /// </summary>
        public int SetOutputRaw(string displayNo, bool on)
        {
            var dio = Equipment.Instance?.DioScan;
            if (dio == null) return -1;

            foreach (var mod in EnumerateModuleNames())
            {
                if (dio.WriteOutput(mod, displayNo, on) == 0)
                    return 0;
            }
            return -1;
        }

        /// <summary>
        /// 논리 키 기반 출력 제어 (예: "TL_RED", "BUZZER").
        /// - 키 불일치 시 -1.
        /// - 성공 시 0.
        /// </summary>
        public int SetOutput(string key, bool on)
        {
            foreach (var (disp, k) in _outputMap)
                if (string.Equals(k, key, StringComparison.OrdinalIgnoreCase))
                    return SetOutputRaw(disp, on);
            return -1;
        }

        /// <summary>
        /// 타워램프 / 부저 기본 패턴 적용.
        /// - 반복 호출 시에도 동일 상태라면 하위 DioScanService 가 중복 Set 을 최적화하거나
        ///   드라이버가 자체적으로 처리할 것을 기대. (필요 시 변경 감지 로직 추가 가능)
        /// </summary>
        public void ApplyTowerPattern(TowerLampPattern pattern)
        {
            switch (pattern)
            {
                case TowerLampPattern.Idle:
                case TowerLampPattern.Running:
                    SetOutput("TL_RED", false);
                    SetOutput("TL_YELLOW", false);
                    SetOutput("TL_GREEN", true);
                    SetOutput("BUZZER", false);
                    break;

                case TowerLampPattern.Warning:
                    SetOutput("TL_RED", false);
                    SetOutput("TL_YELLOW", true);
                    SetOutput("TL_GREEN", false);
                    SetOutput("BUZZER", false);
                    break;

                case TowerLampPattern.Alarm:
                    SetOutput("TL_RED", true);
                    SetOutput("TL_YELLOW", false);
                    SetOutput("TL_GREEN", false);
                    SetOutput("BUZZER", true);
                    break;

                case TowerLampPattern.AllOff:
                    SetOutput("TL_RED", false);
                    SetOutput("TL_YELLOW", false);
                    SetOutput("TL_GREEN", false);
                    SetOutput("BUZZER", false);
                    break;
            }
        }

        public bool IsEmgOn() => GetSnapshot().AnyEmg;
    }

    /// <summary>
    /// 타워램프 / 부저 간단 패턴 정의.
    /// 필요 시 Blink / Flash 등 추가 패턴 확장 가능.
    /// </summary>
    public enum TowerLampPattern
    {
        Idle,
        Running,
        Warning,
        Alarm,
        AllOff
    }

    /// <summary>
    /// 설비 상태 스냅샷.
    /// - Timestamp: 채집 시각
    /// - Inputs/Outputs: 논리 키 기반 I/O 상태
    /// - AllVacuumOk / AllDoorClosed / AnyEmg: 파생 계산 필드
    /// </summary>
    public class EquipmentStatusSnapshot
    {
        public DateTime Timestamp { get; set; }
        public Dictionary<string, bool> Inputs { get; set; } = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, bool> Outputs { get; set; } = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);

        public bool AllVacuumOk { get; set; }
        public bool AllDoorClosed { get; set; }
        public bool AnyEmg { get; set; }

        /// <summary>
        /// 깊은 복제 (Dictionary 새로 생성).
        /// </summary>
        public EquipmentStatusSnapshot Clone() => new EquipmentStatusSnapshot
        {
            Timestamp = Timestamp,
            Inputs = new Dictionary<string, bool>(Inputs, StringComparer.OrdinalIgnoreCase),
            Outputs = new Dictionary<string, bool>(Outputs, StringComparer.OrdinalIgnoreCase),
            AllVacuumOk = AllVacuumOk,
            AllDoorClosed = AllDoorClosed,
            AnyEmg = AnyEmg
        };
    }
}
