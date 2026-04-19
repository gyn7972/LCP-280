using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using QMC.Common;
using QMC.Common.IO;
using QMC.Common.DIO;
using QMC.Common.Motions;
using QMC.Common.Motion; // MotionAxis
using QMC.LCP_280.Process.Unit; // for unit-safe checks

namespace QMC.LCP_280.Process.Component
{
    public sealed class InterlockManager : IDisposable
    {
        #region Singleton (선택: 공용 단일 사용)
        private static InterlockManager _instance;
        private static readonly object _gate = new object();
        public static InterlockManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_gate)
                    {
                        if (_instance == null) _instance = new InterlockManager();
                    }
                }
                return _instance;
            }
        }
        #endregion

        #region Internal Types
        public abstract class InterlockRule
        {
            public string Name { get; private set; }
            protected InterlockRule(string name) { Name = name ?? GetType().Name; }
            public abstract string Evaluate();
            public virtual string Preview(MotionAxis targetAxis, double? targetPos) { return Evaluate(); }
            public virtual bool InvolvesAxis(MotionAxis axis) => false;
        }

        public class AxisAxisClearanceRule : InterlockRule
        {
            private readonly MotionAxis _a; private readonly MotionAxis _b;
            private readonly double _minDistance;
            private readonly Func<double> _distanceFunc;
            public AxisAxisClearanceRule(string name, MotionAxis a, MotionAxis b, double minDistance, Func<double> distanceFunc = null)
                : base(name)
            { _a = a; _b = b; _minDistance = minDistance; _distanceFunc = distanceFunc; }

            private double Compute(double? overrideA = null, double? overrideB = null)
            {
                if (_distanceFunc != null) return _distanceFunc();
                double pa = overrideA.HasValue ? overrideA.Value : (_a?.GetPosition() ?? 0);
                double pb = overrideB.HasValue ? overrideB.Value : (_b?.GetPosition() ?? 0);
                return Math.Abs(pa - pb);
            }
            public override string Evaluate()
            {
                if (_a == null || _b == null) return null;
                double d = Compute();
                if (d < _minDistance)
                    return $"{Name}: AxisClearance {d:F3} < Min({_minDistance:F3}) A={_a.Name} B={_b.Name}";
                return null;
            }
            public override string Preview(MotionAxis targetAxis, double? targetPos)
            {
                if (_a == null || _b == null) return null;
                double? a = null; double? b = null;
                if (targetAxis != null && targetPos.HasValue)
                {
                    if (ReferenceEquals(targetAxis, _a)) a = targetPos.Value;
                    if (ReferenceEquals(targetAxis, _b)) b = targetPos.Value;
                }
                double d = Compute(a, b);
                if (d < _minDistance)
                    return $"(Preview){Name}: AxisClearance {d:F3} < Min({_minDistance:F3}) A={_a.Name} B={_b.Name}";
                return null;
            }
            public override bool InvolvesAxis(MotionAxis axis) => ReferenceEquals(axis, _a) || ReferenceEquals(axis, _b);
        }

        public class AxisIoStateRule : InterlockRule
        {
            private readonly MotionAxis _axis; private readonly string _module; private readonly string _disp; private readonly bool _expected;
            private readonly Func<bool> _ioAccessor;
            public AxisIoStateRule(string name, MotionAxis axis, string moduleName, string displayNo, bool expectedLogicalOn, Func<bool> ioAccessor = null)
                : base(name)
            { _axis = axis; _module = moduleName; _disp = displayNo; _expected = expectedLogicalOn; _ioAccessor = ioAccessor; }
            private bool? ReadIO()
            {
                if (_ioAccessor != null) { try { return _ioAccessor(); } catch { return null; } }
                var dio = Equipment.Instance.DioScan; if (dio == null) return null;
                bool v; if (!dio.TryGetInput(_module, _disp, out v)) return null; return v;
            }
            public override string Evaluate()
            {
                if (_axis == null) return null;
                var v = ReadIO(); if (!v.HasValue) return null;
                if (v.Value != _expected) return $"{Name}: IO({_module}:{_disp})={(v.Value ? 1 : 0)} != Expected {(_expected ? 1 : 0)} Axis={_axis.Name}";
                return null;
            }
            public override bool InvolvesAxis(MotionAxis axis) => ReferenceEquals(axis, _axis);
        }

        public class IoIoExclusionRule : InterlockRule
        {
            private readonly string _m1, _d1, _m2, _d2; private readonly bool _forbiddenLogical = true;
            public IoIoExclusionRule(string name, string module1, string disp1, string module2, string disp2, bool forbiddenLogical = true)
                : base(name) { _m1 = module1; _d1 = disp1; _m2 = module2; _d2 = disp2; _forbiddenLogical = forbiddenLogical; }
            public override string Evaluate()
            {
                var dio = Equipment.Instance.DioScan; if (dio == null) return null;
                bool v1, v2; if (!dio.TryGetInput(_m1, _d1, out v1)) return null; if (!dio.TryGetInput(_m2, _d2, out v2)) return null;
                if (v1 == _forbiddenLogical && v2 == _forbiddenLogical)
                    return $"{Name}: IO({_m1}:{_d1}) & IO({_m2}:{_d2}) 둘다 {(_forbiddenLogical ? "ON" : "OFF")} 금지";
                return null;
            }
        }

        public class FuncRule : InterlockRule
        {
            private readonly Func<string> _evaluator;
            private readonly Func<MotionAxis, bool> _involvesSelector;
            public FuncRule(string name, Func<string> evaluator, Func<MotionAxis, bool> involvesSelector = null) : base(name)
            { _evaluator = evaluator; _involvesSelector = involvesSelector; }
            public override string Evaluate() { try { return _evaluator?.Invoke(); } catch (Exception ex) { return Name + ": " + ex.Message; } }
            public override bool InvolvesAxis(MotionAxis axis) => _involvesSelector != null && _involvesSelector(axis);
        }

        public class AxisHomedRule : InterlockRule
        {
            private readonly MotionAxis _axis; private readonly string _message;
            public AxisHomedRule(string name, MotionAxis axis, string message = null) : base(name) 
            { 
                _axis = axis; 
                _message = message ?? "축 Home 완료 후 동작 가능합니다."; 
            }
            public override string Evaluate()
            { 
                if (_axis == null) return "축이 바인딩되지 않았습니다."; 
                return _axis.IsHomedLatched ? null : _message; 
            }

            public override bool InvolvesAxis(MotionAxis axis) => ReferenceEquals(axis, _axis);
        }
        #endregion

        #region Fields
        private readonly List<InterlockRule> _rules = new List<InterlockRule>();
        private readonly object _ruleLock = new object();
        private Thread _worker; 
        private volatile bool _run;
        private int _periodMs = 2; //1;//2; //50
        private readonly List<string> _lastViolations = new List<string>();
        public event Action<IReadOnlyList<string>> ViolationsUpdated;
        #endregion

        private InterlockManager() { }

        #region Public API
        public void Start(int periodMs = 1) //50
        {
            if (periodMs < 1) 
                periodMs = 1; 

            _periodMs = periodMs;

            if (_run) return; _run = true;
            _worker = new Thread(Loop) { IsBackground = true, Name = "InterlockMgr" };
            _worker.Start();
        }
        public void Stop()
        { _run = false; if (_worker != null && _worker.IsAlive) { if (!_worker.Join(1000)) { try { _worker.Abort(); } catch { } } } _worker = null; }
        public void Dispose() { Stop(); }

        public void AddRule(InterlockRule rule)
        { if (rule == null) return; lock (_ruleLock) { _rules.Add(rule); } }
        public bool RemoveRule(string name)
        { lock (_ruleLock) { int idx = _rules.FindIndex(r => string.Equals(r.Name, name, StringComparison.OrdinalIgnoreCase)); if (idx >= 0) { _rules.RemoveAt(idx); return true; } return false; } }
        public InterlockRule[] GetRules() { lock (_ruleLock) return _rules.ToArray(); }

        public bool ValidateMove(MotionAxis targetAxis, double targetPosition, out string reason)
        {
            reason = null; if (targetAxis == null) return true;
            InterlockRule[] copy; lock (_ruleLock) copy = _rules.ToArray();
            for (int i = 0; i < copy.Length; i++)
            {
                try
                {
                    var r = copy[i]; var msg = r.Preview(targetAxis, targetPosition);
                    if (!string.IsNullOrEmpty(msg)) { reason = msg; return false; }
                }
                catch (Exception ex) { reason = "RulePreviewEx: " + ex.Message; return false; }
            }
            return true;
        }

        public bool ValidateAxisForHome(MotionAxis axis, out string reason)
        {
            reason = null; if (axis == null) return true;
            InterlockRule[] copy; lock (_ruleLock) copy = _rules.ToArray();
            foreach (var r in copy)
            {
                if (!r.InvolvesAxis(axis)) 
                    continue;

                string msg = null;
                try 
                { 
                    msg = r.Evaluate();
                } catch (Exception ex) 
                { msg = r.Name + " EvaluateEx:" + ex.Message; }

                if (!string.IsNullOrEmpty(msg)) 
                { 
                    reason = msg; 
                    return false; 
                }
            }
            return true;
        }

        public bool ValidateForHomeStep(IReadOnlyList<MotionAxis> axes, out string reason)
        {
            reason = null;
            var set = new HashSet<MotionAxis>(axes ?? Array.Empty<MotionAxis>());
            InterlockRule[] copy; lock (_ruleLock) copy = _rules.ToArray();

            foreach (var r in copy)
            {
                try
                {
                    // 전역 룰은 항상 평가, 그 외는 현재 스텝 축과 연관될 때만 평가
                    bool isGlobal = (r is FuncRule) || (r is IoIoExclusionRule);
                    bool related = isGlobal || set.Count == 0 || set.Any(ax => r.InvolvesAxis(ax));
                    if (!related) continue;

                    var msg = r.Evaluate();
                    if (!string.IsNullOrEmpty(msg))
                    {
                        // 어떤 룰이 막았는지 추적 가능하도록 이름 포함
                        reason = string.IsNullOrEmpty(r.Name) ? msg : $"{r.Name}: {msg}";
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    reason = (string.IsNullOrEmpty(r.Name) ? "Rule" : r.Name) + " EvaluateEx:" + ex.Message;
                    return false;
                }
            }
            return true;
        }

        public string[] GetLastViolations() { lock (_lastViolations) return _lastViolations.ToArray(); }
        #endregion

        #region Loop
        private void Loop()
        {
            var prev = new List<string>();
            while (_run)
            {
                var cur = new List<string>();
                InterlockRule[] copy; 
                lock (_ruleLock) copy = _rules.ToArray();
                for (int i = 0; i < copy.Length; i++)
                {
                    try
                    {
                        var msg = copy[i].Evaluate();
                        if (!string.IsNullOrEmpty(msg)) 
                            cur.Add(msg);
                    }
                    catch (Exception ex)
                    {
                        cur.Add(copy[i].Name + " EvaluateEx:" + ex.Message);
                        Log.Write(ex);
                    }
                }
                bool changed = !Enumerable.SequenceEqual(prev, cur);
                if (changed)
                {
                    lock (_lastViolations)
                    {
                        _lastViolations.Clear();
                        _lastViolations.AddRange(cur);
                    }
                    prev = cur;
                    var h = ViolationsUpdated; 
                    if (h != null) 
                    { 
                        try 
                        { 
                            h(GetLastViolations()); 
                        } 
                        catch (Exception ex)
                        { Log.Write(ex); } 
                    }
                }
                Thread.Sleep(_periodMs);
            }
        }
        #endregion

        #region Helper Quick Register Shortcuts
        public AxisAxisClearanceRule AddAxisClearance(string name, MotionAxis a, MotionAxis b, double minDistance, Func<double> distanceFunc = null)
        { var r = new AxisAxisClearanceRule(name, a, b, minDistance, distanceFunc); AddRule(r); return r; }
        public AxisIoStateRule AddAxisIoRequire(string name, MotionAxis axis, string module, string disp, bool expected)
        { var r = new AxisIoStateRule(name, axis, module, disp, expected); AddRule(r); return r; }
        public IoIoExclusionRule AddIoExclusion(string name, string m1, string d1, string m2, string d2, bool forbiddenLogical = true)
        { var r = new IoIoExclusionRule(name, m1, d1, m2, d2, forbiddenLogical); AddRule(r); return r; }
        public FuncRule AddGlobalRule(string name, Func<string> evaluator)
        { var r = new FuncRule(name, evaluator, null); AddRule(r); return r; }
        public AxisHomedRule AddAxisMustBeHomed(string name, MotionAxis axis, string message = null)
        { 
            var r = new AxisHomedRule(name, axis, message); 
            AddRule(r); 
            return r; 
        }
        #endregion
    }
}
