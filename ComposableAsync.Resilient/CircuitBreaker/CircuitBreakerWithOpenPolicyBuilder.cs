using System;
using ComposableAsync.Resilient.CircuitBreaker.Open;
using ComposableAsync.Resilient.ExceptionFilter;

namespace ComposableAsync.Resilient.CircuitBreaker
{
    internal class CircuitBreakerWithOpenPolicyBuilder : ICircuitBreakerWithOpenPolicyBuilder
    {
        private readonly IExceptionFilter _ExceptionFilter;
        private IOpenBehaviourVoid _OpenBehaviour;
        private IOpenBehaviourReturn _OpenBehaviourReturn;

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

        private ICircuitBreakerBuilder GetCircuitBreakerBuilder() => new CircuitBreakerBuilder(_ExceptionFilter, _OpenBehaviour ?? OpenBehaviors.ThrowVoid, _OpenBehaviourReturn ?? OpenBehaviors.ThrowReturn);

        public ICircuitBreakerWithOpenPolicyBuilder ReturnsDefaultWhenOpen()
        {
            _OpenBehaviourReturn = OpenBehaviors.DefaultReturn;
            return this;
        }

        public ICircuitBreakerWithOpenPolicyBuilder ReturnsWhenOpen<T>(T value)
        {
            _OpenBehaviourReturn = OpenBehaviors.Return(value);
            return this;
        }

        public ICircuitBreakerWithOpenPolicyBuilder DoNotThrowForVoid()
        {
            _OpenBehaviour = OpenBehaviors.NoThrowVoid;
            return this;
        }
    }
}
