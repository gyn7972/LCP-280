using System;
using System.Collections.Generic;
using System.Linq; // added for module enumeration
using QMC.Common.Unit;
using QMC.Common.DIO;
using QMC.Common.IO;

namespace QMC.LCP_280.Process.Unit
{
    /// <summary>
    /// 설비 공통 설비상태 (OP S/W, 도어, 진공, 타워램프 등) I/O 를 캡슐화하는 상태 유닛.
    /// - DioScanService 캐시를 이용해 고속 폴링 없이 최신 값을 유지
    /// - Refresh() 로 내부 스냅샷 업데이트 (Equipment.PerformUnitCycle 에서 주기 호출)
    /// - SetOutput() 등 단순 제어 API 제공
    /// </summary>
    public class EquipmentStatusUnit : BaseUnit
    {
        private readonly object _gate = new object();
        private EquipmentStatusSnapshot _latest = new EquipmentStatusSnapshot();

        // 기존: const string ModuleName = "Unit";  // 실제 DIO 모듈명은 "DIO Module1" 등이라 매칭 실패 → 항상 0개
        // 해결: 모든 모듈을 순회하여 해당 DisplayNo 를 최초로 찾는 방식으로 조회

        private static IEnumerable<string> EnumerateModuleNames()
        {
            var eq = Equipment.Instance;
            var unit = eq?.UnitIO;
            if (unit?.Modules == null) yield break;
            foreach (var m in unit.Modules) if (!string.IsNullOrWhiteSpace(m.ModuleName)) yield return m.ModuleName;
        }

        private static bool TryReadInput(DioScanService dio, string disp, out bool value)
        {
            foreach (var mod in EnumerateModuleNames())
            {
                if (dio.TryGetInput(mod, disp, out value)) return true;
            }
            value = false; return false;
        }

        private static bool TryReadOutput(DioScanService dio, string disp, out bool value)
        {
            foreach (var mod in EnumerateModuleNames())
            {
                if (dio.TryGetOutput(mod, disp, out value)) return true;
            }
            value = false; return false;
        }

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

        /// <summary>
        /// Dio 캐시에서 값을 읽어 스냅샷 구성.
        /// (100ms 단위 기본 수행 - 고빈도라 예외는 무시)
        /// </summary>
        public void Refresh()
        {
            var dio = Equipment.Instance?.DioScan; if (dio == null) return;
            var snap = new EquipmentStatusSnapshot { Timestamp = DateTime.Now };

            foreach (var (disp, key) in _inputMap)
            {
                bool v; if (TryReadInput(dio, disp, out v)) snap.Inputs[key] = v;
            }
            foreach (var (disp, key) in _outputMap)
            {
                bool v; if (TryReadOutput(dio, disp, out v)) snap.Outputs[key] = v;
            }

            // 파생 상태 계산
            snap.AllVacuumOk = GetAll(snap.Inputs, "MAIN_VACUUM1","MAIN_VACUUM2","MAIN_VACUUM3","MAIN_VACUUM4");
            snap.AllDoorClosed = GetAll(snap.Inputs, "FRONT_DOOR","LEFT_DOOR","REAR_DOOR","RIGHT_DOOR");
            snap.AnyEmg = snap.Inputs.TryGetValue("EMG_SW", out var emg) && emg;

            lock (_gate) _latest = snap;
        }

        private bool GetAll(Dictionary<string,bool> dict, params string[] keys)
        {
            foreach (var k in keys)
            {
                if (!dict.TryGetValue(k, out var v) || !v) return false;
            }
            return true;
        }

        /// <summary>현재 스냅샷 복제 반환</summary>
        public EquipmentStatusSnapshot GetSnapshot()
        {
            lock (_gate) return _latest.Clone();
        }

        /// <summary>단순 출력 제어 (모듈/DisplayNo 직접)</summary>
        public int SetOutputRaw(string displayNo, bool on)
        {
            var dio = Equipment.Instance?.DioScan; if (dio == null) return -1;
            foreach (var mod in EnumerateModuleNames())
            {
                if (dio.WriteOutput(mod, displayNo, on) == 0) return 0;
            }
            return -1;
        }

        /// <summary>논리 키 기반 출력 제어 (예: SetOutput("TL_RED", true))</summary>
        public int SetOutput(string key, bool on)
        { foreach (var (disp, k) in _outputMap) if (string.Equals(k, key, StringComparison.OrdinalIgnoreCase)) return SetOutputRaw(disp, on); return -1; }

        /// <summary>타워램프/부저 패턴 간단 지원</summary>
        public void ApplyTowerPattern(TowerLampPattern pattern)
        {
            switch (pattern)
            {
                case TowerLampPattern.Idle:
                case TowerLampPattern.Running:
                    SetOutput("TL_RED", false); SetOutput("TL_YELLOW", false); SetOutput("TL_GREEN", true); SetOutput("BUZZER", false); break;
                case TowerLampPattern.Warning:
                    SetOutput("TL_RED", false); SetOutput("TL_YELLOW", true); SetOutput("TL_GREEN", false); SetOutput("BUZZER", false); break;
                case TowerLampPattern.Alarm:
                    SetOutput("TL_RED", true); SetOutput("TL_YELLOW", false); SetOutput("TL_GREEN", false); SetOutput("BUZZER", true); break;
                case TowerLampPattern.AllOff:
                    SetOutput("TL_RED", false); SetOutput("TL_YELLOW", false); SetOutput("TL_GREEN", false); SetOutput("BUZZER", false); break;
            }
        }

        public override int OnRun() { int ret = 0; return ret; }
        public override int OnStop() { int ret = 0; base.OnStop(); return ret; }
    }

    public enum TowerLampPattern { Idle, Running, Warning, Alarm, AllOff }

    public class EquipmentStatusSnapshot
    {
        public DateTime Timestamp { get; set; }
        public Dictionary<string, bool> Inputs { get; set; } = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, bool> Outputs { get; set; } = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
        public bool AllVacuumOk { get; set; }
        public bool AllDoorClosed { get; set; }
        public bool AnyEmg { get; set; }
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
