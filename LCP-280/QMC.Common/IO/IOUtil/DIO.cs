// DIO.cs
using System;
using System.Collections.Generic;
using System.Linq;
using QMC.Common.IO;        // DIOUnit / DIOModuleSetup / DIOChannel
using QMC.Common.DIO;       // DioScanService

namespace QMC.Common.IOUtil
{
    /// <summary>
    /// "논리 키" ↔ (ModuleName, DisplayNo) 매핑 + 입력/출력/펄스 API.
    /// EquipmentLocator.Instance를 통해 DioScan/UnitIO에 접근합니다.
    /// </summary>
    public static class DIO
    {
        private sealed class IoPoint
        {
            public string Module;
            public string Disp;
            public bool IsOutput;
            public IoPoint(string m, string d, bool o) { Module = m; Disp = d; IsOutput = o; }
        }

        private static readonly Dictionary<string, IoPoint> _map =
            new Dictionary<string, IoPoint>(StringComparer.OrdinalIgnoreCase);

        private static IEquipment EnsureEq()
            => EquipmentLocator.Instance ?? throw new InvalidOperationException(
                "EquipmentLocator.Initialize(...) 호출 전에 DIO API가 사용되었습니다.");

        /// <summary>설정(DIOUnit)에서 채널 'Name'으로 찾아 키를 등록</summary>
        public static void MapByName(DIOUnit unit, string key, bool isOutput, string channelName)
        {
            if (unit == null) throw new ArgumentNullException(nameof(unit));
            if (TryFindByName(unit, isOutput, channelName, out var moduleName, out var displayNo))
            {
                _map[key] = new IoPoint(moduleName, displayNo, isOutput);
            }
            else
            {

            }
                

            
        }

        /// <summary>모듈/표시번호를 직접 지정해 매핑</summary>
        public static void Map(string key, string moduleName, string displayNo, bool isOutput)
            => _map[key] = new IoPoint(moduleName, displayNo, isOutput);

        /// <summary>출력 ON/OFF</summary>
        public static bool Out(string key, bool on)
        {
            var eq = EnsureEq();
            var dio = eq.DioScan ?? throw new InvalidOperationException("Equipment.DioScan이 초기화되지 않았습니다.");
            if (!_map.TryGetValue(key, out var p) || !p.IsOutput)
                throw new InvalidOperationException($"'{key}'는 출력으로 매핑되지 않았습니다.");
            return dio.WriteOutput(p.Module, p.Disp, on) == 0;
        }

        /// <summary>출력 펄스(ms)</summary>
        public static bool Pulse(string key, int widthMs)
        {
            var eq = EnsureEq();
            var dio = eq.DioScan ?? throw new InvalidOperationException("Equipment.DioScan이 초기화되지 않았습니다.");
            if (!_map.TryGetValue(key, out var p) || !p.IsOutput)
                throw new InvalidOperationException($"'{key}'는 출력으로 매핑되지 않았습니다.");
            return dio.PulseOutput(p.Module, p.Disp, widthMs) == 0;
        }

        /// <summary>입력 읽기</summary>
        public static bool In(string key, out bool value)
        {
            value = false;
            try
            {
                var eq = EnsureEq();
                var dio = eq.DioScan ?? throw new InvalidOperationException("Equipment.DioScan이 초기화되지 않았습니다.");
                
                if (!_map.TryGetValue(key, out var p) || p.IsOutput)
                {
                    throw new InvalidOperationException($"'{key}'는 입력으로 매핑되지 않았습니다.");
                }
                    
                return dio.TryGetInput(p.Module, p.Disp, out value);
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }

            return false;
        }

        /// <summary>출력 현재 상태 읽기 (가능한 장치에서만). 성공 시 true 반환.</summary>
        public static bool TryGetOutputState(string key, out bool value)
        {
            value = false;
            if (!_map.TryGetValue(key, out var p) || !p.IsOutput) return false;
            var eq = EquipmentLocator.Instance as IEquipment;
            var dio = eq?.DioScan; if (dio == null) return false;
            return dio.TryGetOutput(p.Module, p.Disp, out value);
        }

        // ===== Enumeration API (UI에서 Raw 리스트 구성 용도) =====
        /// <summary>모든 입력 키 나열</summary>
        public static IEnumerable<string> GetAllInputKeys() => _map.Where(kv => !kv.Value.IsOutput).Select(kv => kv.Key).OrderBy(k => k);

        /// <summary>모든 출력 키 나열</summary>
        public static IEnumerable<string> GetAllOutputKeys() => _map.Where(kv => kv.Value.IsOutput).Select(kv => kv.Key).OrderBy(k => k);

        /// <summary>키로부터 포인트 정보 얻기</summary>
        public static bool TryGetPointInfo(string key, out bool isOutput, out string module, out string disp)
        {
            isOutput = false; module = null; disp = null;
            if (!_map.TryGetValue(key, out var p)) return false;
            isOutput = p.IsOutput; module = p.Module; disp = p.Disp; return true;
        }

        /// <summary>DIOUnit에서 채널 이름으로 모듈/표시번호 찾기</summary>
        private static bool TryFindByName(DIOUnit unit, bool isOutput, string channelName,
                                          out string moduleName, out string displayNo)
        {
            moduleName = null; displayNo = null;
            if (unit?.Modules == null) return false;

            foreach (var m in unit.Modules)
            {
                var list = isOutput ? m.Outputs : m.Inputs;
                if (list == null) continue;

                var found = list.FirstOrDefault(c =>
                    string.Equals(c.Name?.Trim(), channelName?.Trim(), StringComparison.OrdinalIgnoreCase));
                if (found != null)
                {
                    moduleName = m.ModuleName;
                    displayNo = found.DisplayNo;
                    return true;
                }
            }
            return false;
        }
    }
}
