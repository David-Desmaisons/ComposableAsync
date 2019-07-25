using System;
using ComposableAsync.Resilient.ExceptionFilter;

namespace ComposableAsync.Resilient.CircuitBreaker
{
    internal class CircuitBreakerBuilder : ICircuitBreakerBuilder
    {
        private readonly IExceptionFilter _ExceptionFilter;

        internal CircuitBreakerBuilder(IExceptionFilter exceptionFilter)
        {
            _ExceptionFilter = exceptionFilter;
        }

        public IDispatcher WithRetryAndTimeout(int attemptsBeforeOpen, TimeSpan timeoutBeforeRetry)
        {
            return new CircuitBreakerDispatcher(_ExceptionFilter, attemptsBeforeOpen, timeoutBeforeRetry).ToFullDispatcher();
        }

        public IDispatcher WithRetryAndTimeout(int attemptsBeforeOpen, int timeoutBeforeRetryInMilliseconds)
        {
            return WithRetryAndTimeout(attemptsBeforeOpen, TimeSpan.FromMilliseconds(timeoutBeforeRetryInMilliseconds));
        }
    }
}
