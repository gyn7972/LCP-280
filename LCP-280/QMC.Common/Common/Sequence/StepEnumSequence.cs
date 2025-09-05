using System;
using System.Collections.Generic;
using System.Threading;

namespace QMC.Common.Sequence
{
    /// <summary>
    /// Enum 기반 Step 시퀀스를 간단히 구현하기 위한 제네릭 래퍼.
    /// - 각 Step 은 등록된 델리게이트(Action/Func) 로 실행
    /// - 델리게이트는 다음 Step(enum) 을 반환하거나 null 반환하면 완료 처리
    /// - 예외 throw 시 SequenceBase 메커니즘에 따라 Error 상태 진입
    /// </summary>
    /// <typeparam name="TStep">Enum 형식 (int 기반)</typeparam>
    public abstract class StepEnumSequence<TStep> : SequenceBase where TStep : struct, IConvertible
    {
        protected delegate TStep? StepHandler(CancellationToken ct);
        private readonly Dictionary<int, StepHandler> _handlers = new Dictionary<int, StepHandler>();

        protected StepEnumSequence(string name) : base(name)
        {
            if (!typeof(TStep).IsEnum)
                throw new ArgumentException("TStep must be enum");
        }

        protected void Register(TStep step, StepHandler handler)
        {
            if (handler == null) throw new ArgumentNullException("handler");
            int key = Convert.ToInt32(step);
            if (_handlers.ContainsKey(key))
                throw new InvalidOperationException("Step already registered: " + step);
            _handlers.Add(key, handler);
        }

        protected override int ExecuteStep(int currentStep, CancellationToken ct)
        {
            StepHandler h;
            if (!_handlers.TryGetValue(currentStep, out h))
                throw new InvalidOperationException("No handler for step " + currentStep + " (" + typeof(TStep).Name + ")");
            var next = h(ct);
            if (!next.HasValue)
                return -1; // complete
            return Convert.ToInt32(next.Value);
        }
    }
}
