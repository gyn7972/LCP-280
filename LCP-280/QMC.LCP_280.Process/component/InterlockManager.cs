using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using QMC.Common;
using QMC.Common.IO;
using QMC.Common.DIO;
using QMC.Common.Motions;
using QMC.Common.Motion; // MotionAxis

namespace QMC.LCP_280.Process.Component
{
    /// <summary>
    /// 모션 / IO 실시간 인터락(충돌·조건 위반)을 한 곳에서 관리하는 매니저.
    /// 핵심 특징
    /// 1) 규칙(Rule) 등록 : 축-축 간 최소 간격 / 축-IO 조건 / IO-IO 배타 등
    /// 2) 주기 스레드(기본 50ms)가 모든 Rule.Evaluate() 수행 → 위반 메시지 캐시 & 이벤트 통지
    /// 3) 이동 직전 사전 가상검사 ValidateMove() 제공 (목표 위치 반영하여 미래 충돌 예측)
    /// 4) 홈(Home) 시작 직전 단일 축 관련 Rule만 빠르게 검사 ValidateAxisForHome() 제공
    /// 5) IO 는 DioScanService 캐시값 사용 → 하드웨어 폴링 부하 최소화
    ///
    /// 사용 시나리오 (단일 축 예: pickZ) - 최소 구성 예:
    /// -----------------------------------------------------------------------------
    /// // (1) 초기 1회: 장비 Init 끝난 후
    /// var il = InterlockManager.Instance;
    /// il.Start();                             // 백그라운드 스캔 시작
    /// // (2) Rule 등록: pickZ 축 이동은 커버 센서(CoverIO 모듈의 X3)가 ON(닫힘)일 때만 허용
    /// il.AddAxisIoRequire("PickZ_CoverClosed", pickZ, "CoverIO", "X3", expected:true);
    /// // (선택) 다른 축과 최소 30mm 거리 유지
    /// il.AddAxisClearance("PickZ_vs_PlaceZ_Clear30", pickZ, placeZ, 30.0);
    ///
    /// // (3) 이동 전 검사
    /// if (!il.ValidateMove(pickZ, targetPos, out var reason))
    /// { Log.Write("Interlock", "BLOCK:" + reason); return; }
    /// pickZ.MoveAbs(targetPos);  // 통과 시만 이동 시작
    ///
    /// // (4) 홈 버튼 처리 시 (Motion_Setup 폼 등)
    /// if (!il.ValidateAxisForHome(pickZ, out var reasonHome))
    /// { MessageBox.Show("Home Block: " + reasonHome); return; }
    /// pickZ.HomeAsync();
    ///
    /// // (5) UI 모니터링
    /// il.ViolationsUpdated += list => UpdateListBox(list); // 실시간 위반 메시지
    ///
    /// // (6) 프로그램 종료 시
    /// il.Stop();  // 또는 Dispose()
    /// -----------------------------------------------------------------------------
    ///
    /// 확장: 새로운 Rule 을 만들고 싶으면 InterlockRule 추상클래스를 상속하고
    /// Evaluate / (필요 시) Preview / InvolvesAxis 를 구현 후 AddRule() 로 등록.
    /// </summary>
    public sealed class InterlockManager : IDisposable
    {
        #region Singleton (선택: 공용 단일 사용)
        private static InterlockManager _instance;
        private static readonly object _gate = new object();
        /// <summary>글로벌 단일 인스턴스(간단한 프로젝트에서 재사용)</summary>
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
        /// <summary>
        /// 모든 인터락 규칙의 기본 추상 클래스.
        /// - Evaluate(): 현재 실시간 상태 평가
        /// - Preview(): 이동(목표 위치) 적용한 가상 평가 (기본은 Evaluate 재사용)
        /// - InvolvesAxis(): 특정 축과 직접 관련 여부 (홈 전 단일축 검사에 사용)
        /// </summary>
        public abstract class InterlockRule
        {
            /// <summary>룰 식별 이름(로그/메시지 표기)</summary>
            public string Name { get; private set; }
            protected InterlockRule(string name) { Name = name ?? GetType().Name; }
            /// <summary>실시간 평가. 위반이면 메시지, 정상이면 null.</summary>
            public abstract string Evaluate();
            /// <summary>목표 위치 적용한 사전 이동 가상 평가. 기본은 Evaluate() 그대로 사용.</summary>
            public virtual string Preview(MotionAxis targetAxis, double? targetPos) { return Evaluate(); }
            /// <summary>이 규칙이 전달된 축과 직접적인 연관이 있는지 여부.</summary>
            public virtual bool InvolvesAxis(MotionAxis axis) => false;
        }

        /// <summary>
        /// 두 축 사이 최소 간격 보장 규칙.
        /// - minDistance 보다 가까우면 위반.
        /// - DistanceFunc 제공 시 커스텀 거리 계산(예: 로봇 키네매틱 변환) 사용.
        /// - Preview() 는 이동 대상 축이 포함된 경우 targetPos 적용하여 재평가.
        /// </summary>
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

        /// <summary>
        /// 특정 축은 지정 IO(모듈/채널) 값이 기대 논리(expected)일 때만 동작 허용.
        /// 예) 커버 닫힘 센서가 ON 이어야 PickZ 이동 허용.
        /// - IO 읽기 실패는 무시(옵션) → 안정성 위해 Evaluate 에서 null 처리.
        /// </summary>
        public class AxisIoStateRule : InterlockRule
        {
            private readonly MotionAxis _axis; private readonly string _module; private readonly string _disp; private readonly bool _expected;
            private readonly Func<bool> _ioAccessor; // 직접 함수로 대체 가능
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

        /// <summary>
        /// 두 IO 는 동시에 특정 논리(기본 true=ON)로 활성화되면 안 되는 배타 조건.
        /// 예) DoorA와 DoorB 센서는 동시에 열릴 수 없다거나, 두 실린더 확장 신호 동시 금지.
        /// </summary>
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
        #endregion

        #region Fields
        private readonly List<InterlockRule> _rules = new List<InterlockRule>();
        private readonly object _ruleLock = new object();
        private Thread _worker; private volatile bool _run; private int _periodMs = 50;
        private readonly List<string> _lastViolations = new List<string>();
        /// <summary>위반 목록이 변할 때마다 발생 (UI 연결 용)</summary>
        public event Action<IReadOnlyList<string>> ViolationsUpdated;
        #endregion

        private InterlockManager() { }

        #region Public API
        /// <summary>
        /// 주기 스캔 시작. (이미 실행 중이면 무시)
        /// periodMs 최소 10ms (너무 낮으면 CPU 부하 증가).
        /// </summary>
        public void Start(int periodMs = 50)
        {
            if (periodMs < 10) periodMs = 10; _periodMs = periodMs;
            if (_run) return; _run = true;
            _worker = new Thread(Loop) { IsBackground = true, Name = "InterlockMgr" };
            _worker.Start();
        }
        /// <summary>
        /// 스캔 중지(스레드 Join, 강제 Abort 최후 수단). Dispose() 에서 자동 호출.
        /// </summary>
        public void Stop()
        { _run = false; if (_worker != null && _worker.IsAlive) { if (!_worker.Join(1000)) { try { _worker.Abort(); } catch { } } } _worker = null; }
        /// <summary>자원 정리 (Stop 호출)</summary>
        public void Dispose() { Stop(); }

        /// <summary>
        /// 규칙 추가. (null 무시) - 다중 스레드 안전(간단 lock).
        /// </summary>
        public void AddRule(InterlockRule rule)
        { if (rule == null) return; lock (_ruleLock) { _rules.Add(rule); } }
        /// <summary>
        /// 이름으로 규칙 제거 (대소문자 무시). 성공 여부 반환.
        /// </summary>
        public bool RemoveRule(string name)
        { lock (_ruleLock) { int idx = _rules.FindIndex(r => string.Equals(r.Name, name, StringComparison.OrdinalIgnoreCase)); if (idx >= 0) { _rules.RemoveAt(idx); return true; } return false; } }
        /// <summary>
        /// 현재 등록된 모든 규칙 스냅샷 (배열 복사)
        /// </summary>
        public InterlockRule[] GetRules() { lock (_ruleLock) return _rules.ToArray(); }

        /// <summary>
        /// 이동 사전 검사.
        /// targetAxis 가 이동하려는 targetPosition 을 가정하여 모든 Rule.Preview() 평가.
        /// 위반 발생 시 false + reason.
        /// (이동 전 MoveAbs 호출 직전에 사용)
        /// </summary>
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

        /// <summary>
        /// 홈(Home) 동작 전 단일 축 관련 규칙(Evaluate)만 검사.
        /// - InvolvesAxis(axis)==true 인 Rule 들만 평가 → 빠른 검사
        /// - 위반 시 false + reason 반환 → 홈 시작 차단 가능
        /// </summary>
        public bool ValidateAxisForHome(MotionAxis axis, out string reason)
        {
            reason = null; if (axis == null) return true;
            InterlockRule[] copy; lock (_ruleLock) copy = _rules.ToArray();
            foreach (var r in copy)
            {
                if (!r.InvolvesAxis(axis)) continue;
                string msg = null;
                try { msg = r.Evaluate(); } catch (Exception ex) { msg = r.Name + " EvaluateEx:" + ex.Message; }
                if (!string.IsNullOrEmpty(msg)) { reason = msg; return false; }
            }
            return true;
        }

        /// <summary>
        /// 마지막 스캔 루프에서 위반되었던 메시지 배열 스냅샷.
        /// (UI 폴링 or ViolationsUpdated 이벤트와 조합)
        /// </summary>
        public string[] GetLastViolations() { lock (_lastViolations) return _lastViolations.ToArray(); }
        #endregion

        #region Loop
        /// <summary>
        /// 내부 스레드 루프: 모든 규칙 Evaluate → 위반 목록 비교 → 변경 시 캐시 + 이벤트 발생.
        /// </summary>
        private void Loop()
        {
            var prev = new List<string>();
            while (_run)
            {
                var cur = new List<string>();
                InterlockRule[] copy; lock (_ruleLock) copy = _rules.ToArray();
                for (int i = 0; i < copy.Length; i++)
                {
                    try
                    {
                        var msg = copy[i].Evaluate();
                        if (!string.IsNullOrEmpty(msg)) cur.Add(msg);
                    }
                    catch (Exception ex)
                    {
                        cur.Add(copy[i].Name + " EvaluateEx:" + ex.Message);
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
                    var h = ViolationsUpdated; if (h != null) { try { h(GetLastViolations()); } catch { } }
                }
                Thread.Sleep(_periodMs);
            }
        }
        #endregion

        #region Helper Quick Register Shortcuts
        /// <summary>
        /// 두 축 최소 간격 규칙 빠른 등록 Helper.
        /// </summary>
        public AxisAxisClearanceRule AddAxisClearance(string name, MotionAxis a, MotionAxis b, double minDistance, Func<double> distanceFunc = null)
        { var r = new AxisAxisClearanceRule(name, a, b, minDistance, distanceFunc); AddRule(r); return r; }
        /// <summary>
        /// 축-IO 상태 요구 규칙 빠른 등록 Helper.
        /// </summary>
        public AxisIoStateRule AddAxisIoRequire(string name, MotionAxis axis, string module, string disp, bool expected)
        { var r = new AxisIoStateRule(name, axis, module, disp, expected); AddRule(r); return r; }
        /// <summary>
        /// IO-IO 상호 배타 규칙 빠른 등록 Helper.
        /// </summary>
        public IoIoExclusionRule AddIoExclusion(string name, string m1, string d1, string m2, string d2, bool forbiddenLogical = true)
        { var r = new IoIoExclusionRule(name, m1, d1, m2, d2, forbiddenLogical); AddRule(r); return r; }
        #endregion
    }
}
