using System;
using ComposableAsync.Resilient.CircuitBreaker.Open;
using ComposableAsync.Resilient.ExceptionFilter;

namespace ComposableAsync.Resilient.CircuitBreaker
{
    internal class CircuitBreakerBuilder : ICircuitBreakerBuilder
    {
        private readonly IExceptionFilter _ExceptionFilter;
        private readonly IOpenBehaviourVoid _OpenBehaviour;
        private readonly IOpenBehaviourReturn _OpenBehaviourReturn;

        internal CircuitBreakerBuilder(IExceptionFilter exceptionFilter, IOpenBehaviourVoid openBehaviour, IOpenBehaviourReturn openBehaviourReturn)
        {
            _ExceptionFilter = exceptionFilter;
            _OpenBehaviour = openBehaviour;
            _OpenBehaviourReturn = openBehaviourReturn;
        }

        public IDispatcher WithRetryAndTimeout(int attemptsBeforeOpen, TimeSpan timeoutBeforeRetry)
        {
            return new CircuitBreakerDispatcher(_ExceptionFilter, _OpenBehaviour, _OpenBehaviourReturn, attemptsBeforeOpen, timeoutBeforeRetry).ToFullDispatcher();
        }

        public IDispatcher WithRetryAndTimeout(int attemptsBeforeOpen, int timeoutBeforeRetryInMilliseconds)
        {
            return WithRetryAndTimeout(attemptsBeforeOpen, TimeSpan.FromMilliseconds(timeoutBeforeRetryInMilliseconds));
        }
    }
}
