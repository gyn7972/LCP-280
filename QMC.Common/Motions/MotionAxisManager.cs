using System;
using System.Collections.Generic;
using System.Linq;

namespace QMC.Common.Motions
{
    /// <summary>
    /// 여러 MotionAxis를 등록/선택/조회/저장/로드하는 통합 매니저 (C# 7.3 호환)
    /// - 복합키: UnitName + AxisName 기반으로 관리
    /// - 기존 단일 이름 기반 API도 유지(중복 시 null 반환)
    /// </summary>
    public sealed class MotionAxisManager
    {
        private readonly object _gate = new object();

        // "Unit||Axis" -> Axis
        private readonly Dictionary<string, MotionAxis> _byKey =
            new Dictionary<string, MotionAxis>(StringComparer.OrdinalIgnoreCase);

        // 등록 순서 유지 및 인덱스 접근용
        private readonly List<string> _order = new List<string>();

        // UnitName -> ["Unit||Axis", ...]
        private readonly Dictionary<string, List<string>> _unitToKeys =
            new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

        private string _currentKey; // 선택된 축(옵션): "Unit||Axis"

        private static string MakeKey(string unitName, string axisName)
        {
            return unitName + "||" + axisName;
        }

        // ===== 이벤트 =====
        public event Action<MotionAxis> AxisAdded;
        public event Action<MotionAxis> AxisRemoved;
        public event Action<MotionAxis> CurrentAxisChanged;

        // ===== 등록/해제 =====
        public void Register(string unitName, MotionAxis axis)
        {
            if (axis == null) throw new ArgumentNullException(nameof(axis));
            if (string.IsNullOrEmpty(unitName)) throw new ArgumentNullException(nameof(unitName));

            var key = MakeKey(unitName, axis.Name);

            lock (_gate)
            {
                if (_byKey.ContainsKey(key))
                    throw new InvalidOperationException("이미 등록됨: " + key);

                _byKey[key] = axis;
                _order.Add(key);

                List<string> list;
                if (!_unitToKeys.TryGetValue(unitName, out list))
                {
                    list = new List<string>();
                    _unitToKeys[unitName] = list;
                }
                list.Add(key);

                if (_currentKey == null) _currentKey = key;
            }

            var added = AxisAdded; 
            if (added != null) 
                added(axis);

            var changed = CurrentAxisChanged; 
            if (_currentKey == key && changed != null) 
                changed(axis);
        }

        public bool Unregister(string unitName, string axisName)
        {
            var key = MakeKey(unitName, axisName);
            MotionAxis removed = null;

            lock (_gate)
            {
                MotionAxis axis;
                if (!_byKey.TryGetValue(key, out axis)) return false;

                _byKey.Remove(key);
                _order.Remove(key);
                removed = axis;

                List<string> list;
                if (_unitToKeys.TryGetValue(unitName, out list))
                {
                    list.Remove(key);
                    if (list.Count == 0) _unitToKeys.Remove(unitName);
                }

                if (string.Equals(_currentKey, key, StringComparison.OrdinalIgnoreCase))
                {
                    _currentKey = _order.Count > 0 ? _order[0] : null;
                    var hc = CurrentAxisChanged;
                    if (hc != null) hc(_currentKey != null ? _byKey[_currentKey] : null);
                }
            }

            var h = AxisRemoved; if (h != null) h(removed);
            return true;
        }

        // (하위호환) 축명이 전역 유일할 때만 해제
        public bool Unregister(string axisNameOnly)
        {
            if (axisNameOnly == null) return false;

            string foundKey = null;
            lock (_gate)
            {
                foreach (var kv in _byKey)
                {
                    if (kv.Value.Name.Equals(axisNameOnly, StringComparison.OrdinalIgnoreCase))
                    {
                        if (foundKey != null) return false; // 모호
                        foundKey = kv.Key;
                    }
                }
            }
            if (foundKey == null) return false;

            var parts = foundKey.Split(new[] { "||" }, StringSplitOptions.None);
            return Unregister(parts[0], parts[1]);
        }

        // ===== 조회 =====
        public MotionAxis[] GetAll()
        {
            lock (_gate)
            {
                var arr = new MotionAxis[_order.Count];
                for (int i = 0; i < _order.Count; i++) arr[i] = _byKey[_order[i]];
                return arr;
            }
        }

        // "Unit||Axis" 전체 키
        public string[] GetKeys()
        {
            lock (_gate) { return _order.ToArray(); }
        }

        // 특정 유닛의 축명 목록
        public string[] GetAxisNames(string unitName)
        {
            lock (_gate)
            {
                List<string> list;
                if (!_unitToKeys.TryGetValue(unitName, out list)) return new string[0];
                var names = new string[list.Count];
                for (int i = 0; i < list.Count; i++)
                {
                    var k = list[i];
                    // k = "Unit||Axis" → Axis만 추출
                    var idx = k.IndexOf("||", StringComparison.Ordinal);
                    names[i] = idx >= 0 ? k.Substring(idx + 2) : k;
                }
                return names;
            }
        }

        // 복합키 조회
        public MotionAxis Get(string unitName, string axisName)
        {
            var key = MakeKey(unitName, axisName);
            lock (_gate)
            {
                MotionAxis a; return _byKey.TryGetValue(key, out a) ? a : null;
            }
        }

        // (하위호환) 축명이 전역 유일할 때만 반환, 중복이면 null
        public MotionAxis Get(string axisNameOnly)
        {
            lock (_gate)
            {
                MotionAxis found = null;
                foreach (var kv in _byKey)
                {
                    if (kv.Value.Name.Equals(axisNameOnly, StringComparison.OrdinalIgnoreCase))
                    {
                        if (found != null) return null; // 모호
                        found = kv.Value;
                    }
                }
                return found;
            }
        }

        public MotionAxis GetByIndex(int index)
        {
            lock (_gate)
            {
                if (index < 0 || index >= _order.Count) return null;
                return _byKey[_order[index]];
            }
        }

        public int Count
        {
            get { lock (_gate) { return _byKey.Count; } }
        }

        // ===== 선택 관리(현재 축) =====
        public MotionAxis Current
        {
            get { lock (_gate) { return _currentKey != null ? _byKey[_currentKey] : null; } }
        }

        public bool Select(string unitName, string axisName)
        {
            var key = MakeKey(unitName, axisName);
            MotionAxis axis;
            lock (_gate)
            {
                if (!_byKey.TryGetValue(key, out axis)) return false;
                if (string.Equals(_currentKey, key, StringComparison.OrdinalIgnoreCase)) return true;
                _currentKey = key;
            }
            var h = CurrentAxisChanged; if (h != null) h(axis);
            return true;
        }

        // (하위호환) 전역 유일 이름일 때만 선택
        public bool SelectByName(string axisNameOnly)
        {
            var a = Get(axisNameOnly);
            if (a == null) return false;

            // 해당 축의 키를 찾는다
            string key = null;
            lock (_gate)
            {
                foreach (var kv in _byKey)
                {
                    if (object.ReferenceEquals(kv.Value, a)) { key = kv.Key; break; }
                }
                if (key == null) return false;
                if (string.Equals(_currentKey, key, StringComparison.OrdinalIgnoreCase)) return true;
                _currentKey = key;
            }
            var h = CurrentAxisChanged; if (h != null) h(a);
            return true;
        }

        public bool SelectByIndex(int index)
        {
            MotionAxis axis = null;
            lock (_gate)
            {
                if (index < 0 || index >= _order.Count) return false;
                var key = _order[index];
                if (string.Equals(_currentKey, key, StringComparison.OrdinalIgnoreCase)) return true;
                _currentKey = key;
                axis = _byKey[key];
            }
            var h = CurrentAxisChanged; if (h != null) h(axis);
            return true;
        }

        // ===== 브로드캐스트/유틸 =====
        public void ForEach(Action<MotionAxis> action)
        {
            if (action == null) return;
            var arr = GetAll();
            for (int i = 0; i < arr.Length; i++) action(arr[i]);
        }

        public void EnsureSelected()
        {
            lock (_gate)
            {
                if (_currentKey == null && _order.Count > 0)
                {
                    _currentKey = _order[0];
                }
            }
        }

        public IReadOnlyList<MotionAxis> GetAllAxes()
        {
            lock (_gate)
            {
                // 등록 순서를 보장하려면 _order를 기준으로 재구성
                var list = new List<MotionAxis>(_order.Count);
                for (int i = 0; i < _order.Count; i++)
                    list.Add(_byKey[_order[i]]);
                return list; // List<MotionAxis>는 IReadOnlyList<MotionAxis>로 반환 가능
            }
        }

        //유닛 기준으로 가져오기
        public IReadOnlyList<MotionAxis> GetAxesByUnit(string unitName)
        {
            if (string.IsNullOrEmpty(unitName)) return Array.Empty<MotionAxis>();
            lock (_gate)
            {
                if (!_unitToKeys.TryGetValue(unitName, out var keys) || keys.Count == 0)
                    return Array.Empty<MotionAxis>();

                var list = new List<MotionAxis>(keys.Count);
                for (int i = 0; i < keys.Count; i++)
                    list.Add(_byKey[keys[i]]);
                return list;
            }
        }

        //축 이름 배열로 원하는 것만 가져오기 (유닛 범위 내)
        public IReadOnlyList<MotionAxis> GetAxes(string unitName, IEnumerable<string> axisNames)
        {
            if (string.IsNullOrEmpty(unitName) || axisNames == null) return Array.Empty<MotionAxis>();
            var set = new HashSet<string>(axisNames, StringComparer.OrdinalIgnoreCase);

            lock (_gate)
            {
                if (!_unitToKeys.TryGetValue(unitName, out var keys) || keys.Count == 0)
                    return Array.Empty<MotionAxis>();

                var list = new List<MotionAxis>();
                foreach (var key in keys)
                {
                    // "Unit||Axis"에서 Axis 추출
                    var idx = key.IndexOf("||", StringComparison.Ordinal);
                    var axisName = idx >= 0 ? key.Substring(idx + 2) : key;
                    if (set.Contains(axisName))
                        list.Add(_byKey[key]);
                }
                return list;
            }
        }

        //유닛+축명으로 안전하게 꺼내기 (예외 대신 false 반환)
        public bool TryGet(string unitName, string axisName, out MotionAxis axis)
        {
            axis = null;
            if (string.IsNullOrEmpty(unitName) || string.IsNullOrEmpty(axisName)) return false;
            var key = unitName + "||" + axisName;
            lock (_gate) { return _byKey.TryGetValue(key, out axis); }
        }

        /*
            // 사용 예시:
            // 유닛 "Loader"의 모든 축
            var loaderAxes = manager.GetAxesByUnit("Loader");

            // 유닛 "Arm"에서 원하는 축만
            var pickPlace = manager.GetAxes("Arm", new[] { "X", "Z" });

            // 조건으로 (예: 소프트리밋 켠 축만)
            var safeAxes = manager.GetAxes(a => a.Setup.SoftLimitEnable);

            // 안전 접근
            if (manager.TryGet("Conveyor", "Y", out var convY))
            {
                convY.MoveAbs(120);
            }
         */
        public void StopAll()
        {
            var arr = GetAll();
            for (int i = 0; i < arr.Length; i++)
            {
                try { arr[i]?.Stop(); } catch { }
            }
        }
        public void EmgStopAll()
        {
            var arr = GetAll();
            for (int i = 0; i < arr.Length; i++)
            {
                try { arr[i]?.EmgStop(); } catch { }
            }
        }
    }
}
