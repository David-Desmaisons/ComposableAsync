using ComposableAsync.Resilient.CircuitBreaker.Open;
using ComposableAsync.Resilient.ExceptionFilter;
using System;
using System.Collections.Generic;

namespace ComposableAsync.Resilient.CircuitBreaker
{
    internal class CircuitBreakerWithOpenPolicyBuilder : ICircuitBreakerWithOpenPolicyBuilder
    {
        private readonly IExceptionFilter _ExceptionFilter;
        private IOpenBehaviourVoid _OpenBehaviour;
        private readonly Dictionary<Type, object> _Values = new Dictionary<Type, object>();
        private bool _UseDefault = false;

        internal CircuitBreakerWithOpenPolicyBuilder(IExceptionFilter exceptionFilter)
        {
            _ExceptionFilter = exceptionFilter; ;
        }

        public IDispatcher WithRetryAndTimeout(int attemptsBeforeOpen, TimeSpan timeoutBeforeRetry)
        {
            return GetCircuitBreakerBuilder().WithRetryAndTimeout(attemptsBeforeOpen, timeoutBeforeRetry);
        }

        public IDispatcher WithRetryAndTimeout(int attemptsBeforeOpen, int timeoutBeforeRetryInMilliseconds)
        {
            return GetCircuitBreakerBuilder().WithRetryAndTimeout(attemptsBeforeOpen, timeoutBeforeRetryInMilliseconds);
        }

        private ICircuitBreakerBuilder GetCircuitBreakerBuilder() => new CircuitBreakerBuilder(_ExceptionFilter, _OpenBehaviour ?? OpenBehaviors.ThrowVoid, OpenBehaviors.Return(_Values, _UseDefault));

        public ICircuitBreakerWithOpenPolicyBuilder ReturnsDefaultWhenOpen()
        {
            _UseDefault = true;
            return this;
        }

        public ICircuitBreakerWithOpenPolicyBuilder ReturnsWhenOpen<T>(T value)
        {
            _Values[typeof(T)] = value;
            return this;
        }

        public ICircuitBreakerWithOpenPolicyBuilder DoNotThrowForVoid()
        {
            _OpenBehaviour = OpenBehaviors.NoThrowVoid;
            return this;
        }
    }
}
