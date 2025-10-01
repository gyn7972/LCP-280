using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace QMC.Common.Unit
{
    /// <summary>
    /// BaseUnit 공통 IO 확장 메서드
    /// Config 객체가 HardInputs / HardOutputs (Name, Disp 포함) 속성을 가진다고 가정
    /// </summary>
    public static class BaseUnitIoExtensions
    {
        private static IEnumerable<(string Name, string Disp)> GetHardList(object config, string propName)
        {
            if (config == null) return Enumerable.Empty<(string, string)>();
            var pi = config.GetType().GetProperty(propName);
            var raw = pi?.GetValue(config) as IEnumerable;
            if (raw == null) return Enumerable.Empty<(string, string)>();

            var list = new List<(string, string)>();
            foreach (var o in raw)
            {
                try
                {
                    var t = o.GetType();
                    var name = t.GetProperty("Name")?.GetValue(o) as string;
                    var disp = t.GetProperty("Disp")?.GetValue(o) as string;
                    if (!string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(disp))
                        list.Add((name, disp));
                }
                catch { /* ignore one item */ }
            }
            return list;
        }

        /// <summary>입력 신호 읽기</summary>
        public static bool ReadInput(this BaseUnit unit, string name)
        {
            if (unit == null || string.IsNullOrWhiteSpace(name)) 
                return false;

            var hi = GetHardList(unit.Config, "HardInputs")
                .FirstOrDefault(i => i.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            
            if (string.IsNullOrEmpty(hi.Name)) 
                return false;

            var eq = EquipmentLocator.Instance; // EquipmentLocator는 Common에서 접근 가능하다고 가정
            var dio = eq?.DioScan; 
            
            if (dio == null) 
                return false;

            foreach (var m in eq.UnitIO.Modules)
                if (dio.TryGetInput(m.ModuleName, hi.Disp, out var v)) 
                    return v;

            return false;
        }

        /// <summary>출력 On/Off</summary>
        public static bool WriteOutput(this BaseUnit unit, string name, bool on)
        {
            if (unit == null || string.IsNullOrWhiteSpace(name)) return false;
            var ho = GetHardList(unit.Config, "HardOutputs")
                .FirstOrDefault(i => i.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (string.IsNullOrEmpty(ho.Name)) return false;

            var eq = EquipmentLocator.Instance;
            var dio = eq?.DioScan; if (dio == null) return false;

            foreach (var m in eq.UnitIO.Modules)
                if (dio.WriteOutput(m.ModuleName, ho.Disp, on) == 0) return true;
            return false;
        }

        /// <summary>출력 캐시 상태 확인</summary>
        public static bool IsOutputOn(this BaseUnit unit, string name)
        {
            if (unit == null || string.IsNullOrWhiteSpace(name)) return false;
            var ho = GetHardList(unit.Config, "HardOutputs")
                .FirstOrDefault(i => i.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (string.IsNullOrEmpty(ho.Name)) return false;

            var eq = EquipmentLocator.Instance;
            var dio = eq?.DioScan; if (dio == null) return false;

            foreach (var m in eq.UnitIO.Modules)
                if (dio.TryGetOutput(m.ModuleName, ho.Disp, out var v)) return v;
            return false;
        }
    }
}
